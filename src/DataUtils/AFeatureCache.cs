using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using SIL.SpeechTools.Utils;

namespace SIL.Pa.Data
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// 
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class AFeatureCache : SortedDictionary<string, AFeature>
	{
		public const string kDefaultAFeatureCacheFile = "DefaultAFeatures.xml";
		public const string kAFeatureCacheFile = "AFeatures.xml";
		
		private string m_cacheFileName = null;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal AFeatureCache(string projectFileName)
		{
			m_cacheFileName = BuildFileName(projectFileName, true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Builds the name from which to load or save the cache file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private string BuildFileName(string projectFileName, bool mustExist)
		{
			string filename = (projectFileName == null ? string.Empty : projectFileName);
			filename += (filename.EndsWith(".") ? string.Empty : ".") + kAFeatureCacheFile;

			if (!File.Exists(filename) && mustExist)
				filename = kDefaultAFeatureCacheFile;

			return filename;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the articulatory feature table from the database into a memory cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static AFeatureCache Load()
		{
			return Load(null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the articulatory feature table from the database into a memory cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static AFeatureCache Load(string projectFileName)
		{
			AFeatureCache cache = new AFeatureCache(projectFileName);

			// Deserialize to a list because Dictionaries are not deserializable.
			List<AFeature> tmpList = STUtils.DeserializeData(cache.CacheFileName,
				typeof(List<AFeature>)) as List<AFeature>;

			foreach (AFeature feature in tmpList)
				cache[feature.Name.ToLower()] = feature;

			tmpList.Clear();
			tmpList = null;
			return (cache.Count == 0 ? null : cache);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves the cache to it's XML file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Save(string projectFileName)
		{
			m_cacheFileName = BuildFileName(projectFileName, false);
			Save();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves the cache to it's XML file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Save()
		{
			// Copy cache to a sorted list (sorted by bit), then to a list because
			// Dictionaries cannot be serialized.
			SortedList<int, AFeature> tmpSortedList = new SortedList<int, AFeature>();
			List<AFeature> tmpList = new List<AFeature>();

			foreach (KeyValuePair<string, AFeature> feature in this)
				tmpSortedList[feature.Value.Bit] = feature.Value;

			foreach (KeyValuePair<int, AFeature> feature in tmpSortedList)
				tmpList.Add(feature.Value);

			STUtils.SerializeData(m_cacheFileName, tmpList);
			tmpSortedList.Clear();
			tmpSortedList = null;
			tmpList.Clear();
			tmpList = null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool FeatureExits(string name, bool showMsgWhenAlreadyExists)
		{
			string key = (name == null ? null : name.ToLower());

			if (ContainsKey(key))
			{
				if (showMsgWhenAlreadyExists)
				{
					STUtils.STMsgBox(
						string.Format(Properties.Resources.kstidFeatureExistsMsg, name),
						System.Windows.Forms.MessageBoxButtons.OK,
						System.Windows.Forms.MessageBoxIcon.Exclamation);
				}

				return true;
			}

			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the custom feature name to the cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public AFeature Add(string name, bool showMsgWhenAlreadyExists)
		{
			string key = (name == null ? null : name.ToLower());
			
			if (string.IsNullOrEmpty(key) || FeatureExits(name, showMsgWhenAlreadyExists))
				return null;

			// Go through the cache to find the first blank
			// feature where the new feature can be added.
			foreach (KeyValuePair<string, AFeature> feature in this)
			{
				if (feature.Key.StartsWith(AFeature.kBlankPrefix))
				{
					AFeature newFeature = feature.Value;
					Remove(feature.Value.Name.ToLower());
					newFeature.Name = name;
					this[key] = newFeature;
					return newFeature;
				}
			}

			return null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public new AFeature this[string featureName]
		{
			get
			{
				System.Diagnostics.Debug.Assert(featureName != null);
				featureName = featureName.Trim();
				System.Diagnostics.Debug.Assert(featureName.Length > 0);
				AFeature feature;
				return (TryGetValue(featureName, out feature) ? feature : null);
			}
			set {base[featureName] = value;}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Delete(string name, bool showMsgWhenCantDelete)
		{
			string key = (name == null ? null : name.ToLower());

			if (!ContainsKey(key))
				return;
			
			if (this[key].Name.StartsWith(AFeature.kBlankPrefix))
			{
				if (showMsgWhenCantDelete)
				{
					STUtils.STMsgBox(
						string.Format(Properties.Resources.kstidFeatureCantBeDeletedMsg, name),
						System.Windows.Forms.MessageBoxButtons.OK,
						System.Windows.Forms.MessageBoxIcon.Exclamation);

					return;
				}
			}

			AFeature feature = this[key];
			Remove(key);

			// Features never really get deleted. Their name just gets set to something
			// that tells the program it's not being used for a custom feature. Just make
			// sure the name is unique and starts with the blank prefix. 
			feature.Name = AFeature.kBlankPrefix + Guid.NewGuid().ToString();
			this[feature.Name.ToLower()] = feature;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string CacheFileName
		{
			get {return m_cacheFileName;}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not the cache can accomodate another custom
		/// feature added.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool CanAddCustomFeature
		{
			get
			{
				foreach (KeyValuePair<string, AFeature> feature in this)
				{
					if (feature.Key.StartsWith(AFeature.kBlankPrefix))
						return true;
				}

				return false;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Determines whether or not the specified key (that's been converted to lowercase and
		/// had all its spaces removed) is in the cache.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public AFeature FeatureFromCompactedKey(string compactKey)
		{
			foreach (KeyValuePair<string, AFeature> feature in this)
			{
				string modifiedKey = feature.Key.Replace(" ", string.Empty);
				if (compactKey == modifiedKey.ToLower())
					return feature.Value;
			}

			return null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets an array of feature names for the features in the specified masks.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string[] GetFeatureList(ulong[] masks)
		{
			List<string> featureList = new List<string>();
			
			foreach (KeyValuePair<string, AFeature> feature in this)
			{
				ulong mask = masks[feature.Value.MaskNumber];
				if ((mask & feature.Value.Mask) > 0)
					featureList.Add(feature.Value.Name);
			}

			return featureList.ToArray();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a string representing all the features in the specified masks. The feature
		/// names are joined and delimited by a comma and space.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string GetFeaturesText(ulong[] masks)
		{
			string[] featureList = GetFeatureList(masks);
			StringBuilder bldrfeatures = new StringBuilder();
			
			foreach (string feature in featureList)
			{
				bldrfeatures.Append(feature);
				bldrfeatures.Append(", ");
			}

			// Remove the last comma and space.
			if (bldrfeatures.Length >= 2)
				bldrfeatures.Length -= 2;

			return (bldrfeatures.ToString());
		}
	}

	#region AFeature class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// A class defining an object to store the information for a single articulatory feature.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class AFeature
	{
		internal const string kBlankPrefix = "_~blank";

		private int m_bit;
		private string m_name;
		private int m_maskNumber;
		private ulong m_mask;
		private bool m_isCustomFeature = false;

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute]
		public int Bit
		{
			get { return m_bit; }
			set { m_bit = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlAttribute]
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public int MaskNumber
		{
			get { return m_maskNumber; }
			set { m_maskNumber = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ulong Mask
		{
			get { return m_mask; }
			set { m_mask = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool IsCustomFeature
		{
			get {return m_isCustomFeature;}
			set { m_isCustomFeature = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not the feature is blank. If it is, it means
		/// it's a custom placeholder.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[XmlIgnore]
		public bool IsBlank
		{
			get { return m_name.StartsWith(kBlankPrefix); }
		}

		#endregion
	}

	#endregion
}
