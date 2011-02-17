using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using SIL.Pa.DataSource;
using SIL.Pa.PhoneticSearching;
using SIL.Pa.Processing;
using SilTools;

namespace SIL.Pa.Model
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This is strictly for serializing and deserializing PaXML files.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class PaXMLContent
	{
		public PaFieldInfoList CustomFields;
		
		[XmlElement("PaRecords")]
		public RecordCache Cache;
	}

	/// ----------------------------------------------------------------------------------------
	public class RecordCache : List<RecordCacheEntry>, IDisposable
	{
		private string m_phoneticFieldName;
		private PaProject m_project;

		/// ------------------------------------------------------------------------------------
		public RecordCache()
		{
		}

		/// ------------------------------------------------------------------------------------
		public RecordCache(PaProject project)
		{
			m_project = project;
			m_phoneticFieldName = m_project.GetPhoneticField().Name;
			RecordCacheEntry.ResetCounter();
		}

		/// ------------------------------------------------------------------------------------
		public void Dispose()
		{
			TempRecordCache.Dispose();
			if (WordCache != null)
			{
				WordCache.Clear();
				WordCache = null;
			}

			Clear();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the word cache that was built from all the record entries.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public WordCache WordCache { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the cache of words that did not match the current filter.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public WordCache WordsNotInCurrentFilter { get; private set; }

		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public PhoneCache PhoneCache { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the cache of phones without respect to current filter.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public PhoneCache UnfilteredPhoneCache { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Deserializes a PAXML file to a RecordCache instance.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static RecordCache Load(PaDataSource dataSource, PaProject project)
		{
			if (dataSource == null)
				return null;

			string filename = dataSource.SourceFile;

			try
			{
				var paxmlcontent = XmlSerializationHelper.DeserializeFromFile<PaXMLContent>(filename);
				var cache = (paxmlcontent == null ? null : paxmlcontent.Cache);

				if (cache == null)
					return null;

				cache.m_project = project;
				cache.m_phoneticFieldName = project.GetPhoneticField().Name;
				cache.DeserializedCustomFields = paxmlcontent.CustomFields;
				string fwServer;
				string fwDBName;
				PaDataSource.GetPaXmlType(filename, out fwServer, out fwDBName);
				dataSource.FwPrjName = fwDBName;

				foreach (var entry in cache)
				{
					entry.PostDeserializeProcess(dataSource, project);

					if (entry.FieldValues.Count > 0 &&
						(entry.WordEntries == null || entry.WordEntries.Count == 0))
					{
						entry.NeedsParsing = true;
					}
				}

				return cache;
			}
			catch (Exception e)
			{
				var msg = App.LocalizeString("LoadingRecordCacheErrorMsg",
					"The following error occurred while loading '{0}':\n{1}",
					"Message displayed when failing to load a PAXML file. Parameter 0 is filename, and parameter 1 is the error message.",
					App.kLocalizationGroupInfoMsg);

				filename = Utils.PrepFilePathForMsgBox(filename);
				Utils.MsgBox(string.Format(msg, filename, e.Message), MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation);
				
				return null;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Serializes the cache contents to an XML file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Save(string filename)
		{
			try
			{
				var paxmlcontent = new PaXMLContent();
				paxmlcontent.Cache = this;

				//paxmlcontent.CustomFields = new PaFieldInfoList();

				//foreach (var field in m_project.Fiel.Where(fi => fi.IsCustom))
				//    paxmlcontent.CustomFields.Add(fieldInfo);

				//if (paxmlcontent.CustomFields.Count == 0)
				//    paxmlcontent.CustomFields = null;

				XmlSerializationHelper.SerializeToFile(filename, paxmlcontent);
			}
			catch (Exception e)
			{
				Utils.MsgBox(string.Format(Properties.Resources.kstidSavingRecordCacheError,
					e.Message), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public PaFieldInfoList DeserializedCustomFields { get; set; }

		/// ------------------------------------------------------------------------------------
		public void BuildWordCache(ToolStripProgressBar progBar)
		{
			FindAutoGeneratedAmbigousSequences();
			var tmpWordCache = new WordCache();

			foreach (var entry in this)
			{
				if (progBar != null)
					progBar.Increment(1);

				// A record entry doesn't need parsing if it came from a PAXML data source.
				// In that case, a word cache entry only needs to have two things done here:
				// 1) have its owning record entry set and 2) it needs to be added to the
				// word cache.
				if (entry.NeedsParsing)
					ParseEntry(entry);

				foreach (WordCacheEntry wentry in entry.WordEntries)
				{
					wentry.RecordEntry = entry;
					tmpWordCache.Add(wentry);
				}	
			}

			UnfilteredPhoneCache = GetPhonesFromWordCache(tmpWordCache);
			SearchEngine.PhoneCache = UnfilteredPhoneCache;
			BuildFilteredWordCache(tmpWordCache);
		}

		/// ------------------------------------------------------------------------------------
		public void BuildFilteredWordCache()
		{
			BuildFilteredWordCache(WordCache.Union(WordsNotInCurrentFilter));
		}

		/// ------------------------------------------------------------------------------------
		private void BuildFilteredWordCache(IEnumerable<WordCacheEntry> tmpWordCache)
		{
			WordsNotInCurrentFilter = new WordCache();
			WordCache = new WordCache();

			foreach (var wentry in tmpWordCache)
			{
				if (m_project.FilterHelper.EntryMatchesCurrentFilter(wentry))
					WordCache.Add(wentry);
				else
					WordsNotInCurrentFilter.Add(wentry);
			}

			PhoneCache = GetPhonesFromWordCache(WordCache);
			m_project.FilterHelper.PostCacheBuildingFinalize();
			ProjectInventoryBuilder.Process(m_project);
		}

		/// ------------------------------------------------------------------------------------
		private PhoneCache GetPhonesFromWordCache(IEnumerable<WordCacheEntry> wordCache)
		{
			var phoneCache = new PhoneCache(m_project);

			foreach (var entry in wordCache)
			{
				var phones = entry.Phones;

				if (phones == null)
					continue;

				for (int i = 0; i < phones.Length; i++)
				{
					// Don't bother adding break characters.
					if (App.BreakChars.Contains(phones[i]))
						continue;

					if (!phoneCache.ContainsKey(phones[i]))
						phoneCache.AddPhone(phones[i]);

					// Determine if the current phone is the primary
					// phone in an uncertain group.
					bool isPrimaryUncertainPhone = (entry.ContiansUncertainties &&
						entry.UncertainPhones.ContainsKey(i));

					// When the phone is the primary phone in an uncertain group, we
					// don't add it to the total count but to the counter that keeps
					// track of the primary	uncertain phones. Then we also add to the
					// cache the non primary uncertain phones.
					if (!isPrimaryUncertainPhone)
						phoneCache[phones[i]].TotalCount++;
					else
					{
						phoneCache[phones[i]].CountAsPrimaryUncertainty++;

						// Go through the uncertain phones and add them to the cache.
						if (entry.ContiansUncertainties)
						{
							AddUncertainPhonesToCache(entry.UncertainPhones[i], phoneCache);
							UpdateSiblingUncertaintys(entry.UncertainPhones, phoneCache);
						}
					}
				}
			}

			if (m_project.FeatureOverrides != null)
				m_project.FeatureOverrides.MergeWithPhoneCache(phoneCache);

			AddUndefinedCharsToCaches(phoneCache);
			return phoneCache;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Updates a uncertain phone sibling lists for each phone in each uncertain group for
		/// the specified uncertain groups.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static void UpdateSiblingUncertaintys(
			IDictionary<int, string[]> uncertainPhones, IDictionary<string, IPhoneInfo> phoneCache)
		{
			// Go through the uncertain phone groups
			foreach (var uPhones in uncertainPhones.Values)
			{
				// Go through the uncertain phones in this group.
				for (int i = 0; i < uPhones.Length; i++)
				{
					IPhoneInfo phoneUpdating;

					// TODO: Log an error that the phone isn't found in the the cache
					// Get the cache entry for the phone whose sibling list will be updated.
					if (!phoneCache.TryGetValue(uPhones[i], out phoneUpdating))
						continue;

					// Go through the sibling phones, adding them to
					// the updated phones sibling list.
					for (int j = 0; j < uPhones.Length; j++)
					{
						// Add the phone pointed to by j if it's not the phone whose
						// cache entry we're updating and if it's not a phone already
						// in the sibling list of the cache entry we're updating.
						if (j != i && !phoneUpdating.SiblingUncertainties.Contains(uPhones[j]))
						{
							phoneUpdating.SiblingUncertainties.Add(
								IPASymbolCache.UncertainGroupAbsentPhoneChars.Contains(uPhones[j]) ?
								IPASymbolCache.UncertainGroupAbsentPhoneChar : uPhones[j]);
						}
					}
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Goes through all the undefined phonetic characters found in data sources and adds
		/// temporary (i.e. as long as this session of PA is running) records for them in the
		/// IPA character cache and the phone cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static void AddUndefinedCharsToCaches(PhoneCache phoneCache)
		{
			if (App.IPASymbolCache.UndefinedCharacters == null ||
				App.IPASymbolCache.UndefinedCharacters.Count == 0)
			{
				return;
			}

			foreach (var upci in App.IPASymbolCache.UndefinedCharacters)
			{
				App.IPASymbolCache.AddUndefinedCharacter(upci.Character);
				phoneCache.AddUndefinedPhone(upci.Character.ToString());
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the specified list of uncertain phones to the phone cache. It is assumed the
		/// first (i.e. primary) phone in the list has already been added to the cache and,
		/// therefore, it will not be added nor its count incremented.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static void AddUncertainPhonesToCache(string[] uncertainPhoneGroup,
			PhoneCache phoneCache)
		{
			// Go through the uncertain phone groups, skipping the
			// primary one in each group since that was already added
			// to the cache above.
			for (int i = 1; i < uncertainPhoneGroup.Length; i++)
			{
				string phone = uncertainPhoneGroup[i];

				// Don't bother adding break characters.
				if (!App.BreakChars.Contains(phone))
				{
					if (!phoneCache.ContainsKey(phone))
						phoneCache.AddPhone(phone);

					phoneCache[phone].CountAsNonPrimaryUncertainty++;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Scans the phonetic transcription in each record for possible ambiguous sequences.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void FindAutoGeneratedAmbigousSequences()
		{
			var ambigSeqs = new List<string>();
			var sequences = from entry in this
							select entry.GetValue(m_phoneticFieldName) into phonetic
							where phonetic != null
							select m_project.PhoneticParser.FindAmbiguousSequences(phonetic) into seqs
							where seqs != null
							select seqs;
			
			foreach (var seqs in sequences)
				ambigSeqs.AddRange(seqs);

			if (ambigSeqs.Count > 0)
			{
				var list = m_project.AmbiguousSequences ?? new AmbiguousSequences();
				list.AddRange(ambigSeqs.Select(seq => new AmbiguousSeq(seq, true, true)));
				m_project.SaveAndLoadAmbiguousSequences(list);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Parses a single record, going through its fields and parsing them individually
		/// as needed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void ParseEntry(RecordCacheEntry entry)
		{
			TempRecordCache.Add(entry.Id, entry[m_phoneticFieldName]);
			entry.WordEntries = new List<WordCacheEntry>();

			// Parse interlinear fields first, if there are any.
			if (entry.HasInterlinearData)
				ParseEntryAsInterlinear(entry);
			
			var eticField = entry.Project.GetPhoneticField();
			var eticMapping = entry.DataSource.FieldMappings.Single(m => m.Field.Name == eticField.Name);
			
			// If we didn't parse any interlinear fields or the phonetic wasn't among
			// them, make sure it gets parsed before any other non interlinear fields.
			if (eticMapping.IsParsed && !entry.GetIsInterlinearField(eticField.Name))
				ParseSingleFieldInEntry(entry, eticField);

			// Parse all the non phonetic, non interlinear fields.
			foreach (var mapping in entry.DataSource.FieldMappings.Where(m => m.IsParsed &&
				m.Field.Type != FieldType.Phonetic && !entry.GetIsInterlinearField(m.Field.Name)))
			{
				ParseSingleFieldInEntry(entry, mapping.Field);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Parses a non interlinear field (if necessary) and saves the field contents in one
		/// or more word cache entries. Parsing will depend on the data source's parse type
		/// and the field being parsed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static void ParseSingleFieldInEntry(RecordCacheEntry entry, PaField field)
		{
			entry.NeedsParsing = false;
			string unparsedData = entry[field.Name];

			if (string.IsNullOrEmpty(unparsedData))
				return;

			// If we're not dealing with the phonetic field then check if our parsing type is
			// only phonetic or none at all. If either casecase then do nothing which will cause
			// any reference to the word cache entry's value for the field to defer to the
			// value that's stored in the word cache entry's owning record entry.
			if (field.Type != FieldType.Phonetic &&
				(entry.DataSource.ParseType == DataSourceParseType.PhoneticOnly ||
				entry.DataSource.ParseType == DataSourceParseType.None))
			{
				return;
			}

			// By this time we know we're dealing with one of three conditions: 1) the
			// field is phonetic or 2) the field should be parsed or 3) both 1 and
			// 2. When the field should be parsed then split it into individual words.
			string[] split = (entry.DataSource.ParseType == DataSourceParseType.None ?
				new[] { unparsedData } : unparsedData.Split(App.BreakChars.ToCharArray(),
					StringSplitOptions.RemoveEmptyEntries));

			for (int i = 0; i < split.Length; i++)
			{
				// Expand the capacity for more word entries if necessary.
				if (i == entry.WordEntries.Count)
					entry.WordEntries.Add(new WordCacheEntry(entry, i, true));

				entry.WordEntries[i][field.Name] = split[i];
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// \tx nimeolewa.
		/// \mb ni-  me - oa    -le  -wa
		/// \ge 1S-  PF - marry -DER -PASS
		/// \ps pro- tns- v     -sfx -sfx
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static void ParseEntryAsInterlinear(RecordCacheEntry entry)
		{
			string firstInterlinearLine = entry[entry.FirstInterlinearField];

			if (string.IsNullOrEmpty(firstInterlinearLine))
				return;

			// Get the width of each interlinear column.
			var colWidths = GetInterlinearColumnWidths(firstInterlinearLine);

			// Store the unparsed interlinear lines in a collection of strings, then remove
			// those lines from the record cache entry so they no longer take up space.
			var unparsedLines = new Dictionary<string, string>();
			foreach (var field in entry.InterlinearFields)
			{
				unparsedLines[field] = entry[field];
				entry.SetValue(field, null);
			}

			// Now parse each interlinear line.
			int i = 0;
			int wordIndex = 0;
			for (int w = 0; w < colWidths.Count; w++)
			{
				var wordEntry = new WordCacheEntry(entry, wordIndex++, true);
				
				foreach (var line in unparsedLines.Where(l => l.Value != null && i < l.Value.Length))
				{
					wordEntry[line.Key] =
						(i + colWidths[w] >= line.Value.Length || w == colWidths.Count - 1 ?
						line.Value.Substring(i).Trim() :
						line.Value.Substring(i, colWidths[w]).Trim());

					if (line.Key == firstInterlinearLine)
						wordEntry.SetFieldAsFirstLineInterlinear(line.Key);
					else
						wordEntry.SetFieldAsSubordinateInterlinear(line.Key);
				}

				entry.WordEntries.Add(wordEntry);
				i += colWidths[w];
			}
		}

		/// ------------------------------------------------------------------------------------
		private static List<int> GetInterlinearColumnWidths(string firstInterlinearLine)
		{
			int i;
			int start = 0;
			List<int> colWidths = new List<int>();

			while ((i = firstInterlinearLine.IndexOf(' ', start)) >= 0)
			{
				while (i < firstInterlinearLine.Length && firstInterlinearLine[i] == ' ')
					i++;

				if (i == firstInterlinearLine.Length)
					break;

				colWidths.Add(i - start);
				start = i;
			}

			colWidths.Add(firstInterlinearLine.Length - start);
			return colWidths;
		}
	}

	#region TempRecordCache class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This temporary cache is used to store the full, original phonetic data from each record
	/// read from a data source. It is not saved in memory but is loaded when the user wants to
	/// jump to the source editor of an SFM data source and the field for jumping is phonetic.
	/// The reason the original is stored is because PA's working version may have been
	/// converted due to specified ambiguous item conversions.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class TempRecordCache
	{
		private static string s_tmpFilename;
		private static Dictionary<int, string> s_cache;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds an entry to the tempoary record cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void Add(int id, string phonetic)
		{
			if (string.IsNullOrEmpty(phonetic))
				return;

			if (s_cache == null)
				s_cache = new Dictionary<int, string>();

			s_cache[id] = phonetic;
		}

		/// ------------------------------------------------------------------------------------
		public static Dictionary<int, string> Load()
		{
			if (!File.Exists(s_tmpFilename))
				return null;

			var tmpList = XmlSerializationHelper.DeserializeFromFile<List<TempRecordCacheEntry>>(s_tmpFilename);
			if (tmpList == null)
				return null;

			s_cache = tmpList.ToDictionary(e => e.Id, e => e.Phonetic);
			tmpList.Clear();
			return s_cache;
		}

		/// ------------------------------------------------------------------------------------
		public static void Save()
		{
			if (s_cache == null)
				return;

			if (string.IsNullOrEmpty(s_tmpFilename))
				s_tmpFilename = Path.GetTempFileName();

			var tmpList = s_cache.Select(e => new TempRecordCacheEntry(e.Key, e.Value)).ToList();
			XmlSerializationHelper.SerializeToFile(s_tmpFilename, tmpList);
			s_cache.Clear();
			tmpList.Clear();
			s_cache = null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets rid of the temporary cache file and clears out the temporary cache list. This
		/// Dispose is not the same as IDisposable's Dispose.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void Dispose()
		{
			if (File.Exists(s_tmpFilename))
				File.Delete(s_tmpFilename);

			if (s_cache != null)
				s_cache.Clear();
	
			s_cache = null;
			s_tmpFilename = null;
		}
	}

	#endregion

	#region TempRecordCacheEntry class
	/// ----------------------------------------------------------------------------------------
	[XmlType("OriginalPhonetic")]
	public class TempRecordCacheEntry
	{
		[XmlAttribute("RecordId")]
		public int Id;
		[XmlAttribute]
		public string Phonetic;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Needed for Deserialization.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TempRecordCacheEntry()
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Constructs and initializes an TempRecordCacheEntry object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TempRecordCacheEntry(int id, string phonetic)
		{
			Id = id;
			Phonetic = phonetic;
		}
	}

	#endregion
}
