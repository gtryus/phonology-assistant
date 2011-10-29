using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIL.Pa.Model;
using SilTools;

namespace SIL.Pa.UI.Controls
{
	/// ----------------------------------------------------------------------------------------
	public partial class PatternBuilderComponents : UserControl, IxCoreColleague
	{
		public ItemDragEventHandler ConPickerDragHandler;
		public ItemDragEventHandler VowPickerDragHandler;
		public ToolStripItemClickedEventHandler ConPickerClickedHandler;
		public ToolStripItemClickedEventHandler VowPickerClickedHandler;
		public ItemDragEventHandler OtherCharDragHandler;
		public CharPicker.CharPickedHandler OtherCharPickedHandler;
		public ItemDragEventHandler FeatureListsItemDragHandler;
		public KeyPressEventHandler FeatureListsKeyPressHandler;
		public FeatureListViewBase.CustomDoubleClickHandler FeatureListDoubleClickHandler;
		public ItemDragEventHandler ClassListItemDragHandler;
		public KeyPressEventHandler ClassListKeyPressHandler;
		public MouseEventHandler ClassListDoubleClickHandler;

		private FeatureListViewBase m_lvArticulatoryFeatures;
		private FeatureListViewBase m_lvBinaryFeatures;
		private CharPickerRows m_conPicker;
		private CharPickerRows m_vowPicker;
		private List<char> m_diacriticsInCache;

		/// ------------------------------------------------------------------------------------
		public PatternBuilderComponents()
		{
			InitializeComponent();
			App.AddMediatorColleague(this);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Remove ourselves from the mediator list.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();

				App.RemoveMediatorColleague(this);
			}

			base.Dispose(disposing);
		}

		/// ------------------------------------------------------------------------------------
		public void Initialize()
		{
			pnlConsonants.SuspendLayout();
			pnlVowels.SuspendLayout();
			tpgAFeatures.SuspendLayout();
			tpgBFeatures.SuspendLayout();

			tabPatternBlding.Font = FontHelper.UIFont;
			tpgClasses.Tag = lvClasses.Items;
			
			lvClasses.Load();
			lvClasses.LoadSettings(Name);
			lvClasses.ItemDrag += ClassListItemDragHandler;
			lvClasses.KeyPress += ClassListKeyPressHandler;
			lvClasses.MouseDoubleClick += ClassListDoubleClickHandler;

			SetupVowConPickers(true);
			SetupOtherPicker();

			m_lvArticulatoryFeatures = InitializeFeatureList(new DescriptiveFeatureListView { CheckBoxes = false });
			m_lvBinaryFeatures = InitializeFeatureList(new DistinctiveFeatureListView { CheckBoxes = true });
			tpgAFeatures.Controls.Add(m_lvArticulatoryFeatures);
			tpgBFeatures.Controls.Add(m_lvBinaryFeatures);

			tpgBFeatures.ResumeLayout(false);
			tpgAFeatures.ResumeLayout(false);
			pnlVowels.ResumeLayout(false);
			pnlConsonants.ResumeLayout(false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads settings using the specified form name.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void LoadSettings(string frmName, StringCollection explorerSettings)
		{
			charExplorer.LoadSettings(explorerSettings);
			lvClasses.LoadSettings(frmName);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves settings using the specified form name.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SaveSettings(string frmName, Action<StringCollection> getCharExplorerStatesAction)
		{
			lvClasses.SaveSettings(frmName);
			getCharExplorerStatesAction(charExplorer.GetExpandedStates());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The classes were updated in the class dialog, so rebuild the class list.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnSearchClassesChanged(object args)
		{
			lvClasses.Load();
			return false;
		}

		/// ------------------------------------------------------------------------------------
		protected bool OnDataSourcesModified(object args)
		{
			m_lvArticulatoryFeatures.Load();
			m_lvBinaryFeatures.Load();
			return false;
		}

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public FeatureListViewBase ArticulatoryFeaturesList
		{
			get { return m_lvArticulatoryFeatures; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public FeatureListViewBase BinaryFeaturesList
		{
			get { return m_lvBinaryFeatures; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public CharPickerRows ConsonantPicker
		{
			get { return m_conPicker; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public CharPickerRows VowelPicker
		{
			get { return m_vowPicker; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ClassListView ClassListView
		{
			get { return lvClasses; }
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		private void SetupVowConPickers(bool firstTime)
		{
			if (!firstTime)
			{
				m_conPicker.ItemDrag -= ConPickerDragHandler;
				m_conPicker.ItemClicked -= ConPickerClickedHandler;
				m_vowPicker.ItemDrag -= VowPickerDragHandler;
				m_vowPicker.ItemClicked -= VowPickerClickedHandler;
				pnlVowels.Controls.Clear();
				pnlConsonants.Controls.Clear();
				m_conPicker.Dispose();
				m_vowPicker.Dispose();
			}

			// Create the consonant picker on the con. tab.
			m_conPicker = new CharPickerRows();
			m_conPicker.Location = new Point(0, 0);
			m_conPicker.BackColor = pnlConsonants.BackColor;
			CharGridBuilder bldr = new CharGridBuilder(m_conPicker, IPASymbolType.consonant);
			bldr.Build();
			pnlConsonants.Controls.Add(m_conPicker);

			// Create the consonant picker on the vow. tab.
			m_vowPicker = new CharPickerRows();
			m_vowPicker.Location = new Point(0, 0);
			m_vowPicker.BackColor = pnlVowels.BackColor;
			bldr = new CharGridBuilder(m_vowPicker, IPASymbolType.vowel);
			bldr.Build();
			pnlVowels.Controls.Add(m_vowPicker);

			m_conPicker.ItemDrag += ConPickerDragHandler;
			m_conPicker.ItemClicked += ConPickerClickedHandler;
			m_vowPicker.ItemDrag += VowPickerDragHandler;
			m_vowPicker.ItemClicked += VowPickerClickedHandler;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sets up the character explorer on the "Other" tab of the side panel.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void SetupOtherPicker()
		{
			m_diacriticsInCache = new List<char>();

			// Go through all the phones in the cache and strip off their diacritics,
			// add the diacritics to a collection of diacritics.
			foreach (var phoneInfo in App.Project.PhoneCache)
			{
				foreach (char c in from c in phoneInfo.Key
								   let ci = App.IPASymbolCache[c]
								   where ci != null && !ci.IsBase
								   select c)
				{
					m_diacriticsInCache.Add(c);
				}
			}

			charExplorer.ItemDrag += OtherCharDragHandler;
			charExplorer.CharPicked += OtherCharPickedHandler;
			charExplorer.Load((int)IPASymbolType.diacritic | (int)IPASymbolSubType.All, ci =>
			{
				// TODO: Fix this when chao characters are supported.

				// Always allow non consonants.
				if (ci.Type != IPASymbolType.consonant)
					return true;

				char chr = ci.Literal[0];

				// The only consonants to allow are the tie bars.
				return (m_diacriticsInCache.Contains(chr) ||
					chr == App.kTopTieBarC || chr == App.kBottomTieBarC);
			});

			m_diacriticsInCache = null;
		}
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates and initializes a feature list resultView and returns it.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private FeatureListViewBase InitializeFeatureList(FeatureListViewBase flv)
		{
			flv.Load();
			flv.Dock = DockStyle.Fill;
			flv.LabelEdit = false;
			flv.AllowDoubleClickToChangeCheckState = false;
			flv.EmphasizeCheckedItems = false;
			flv.ItemDrag += FeatureListsItemDragHandler;
			flv.KeyPress += FeatureListsKeyPressHandler;
			flv.CustomDoubleClick += FeatureListDoubleClickHandler;

			return flv;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// For some reason .Net doesn't paint tab page backgrounds properly when visual
		/// styles are active and the tab control changes sizes. Therefore, this will force
		/// repainting after a resize.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void tabPatternBlding_ClientSizeChanged(object sender, EventArgs e)
		{
			tabPatternBlding.SelectedTab.Invalidate();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Rebuilds the components when the project query changes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void RefreshComponents()
		{
			SetupVowConPickers(false);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Reloads a chart.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnPhoneChartArrangementChanged(object args)
		{
			RefreshComponents();
			return false;
		}
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Provides a way to force the components to update their fonts.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void RefreshFonts()
		{
			m_conPicker.RefreshFont();
			m_vowPicker.RefreshFont();
			charExplorer.RefreshFont();
		}

		#region IxCoreColleague Members
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public IxCoreColleague[] GetMessageTargets()
		{
			return new IxCoreColleague[] {this};
		}

		#endregion
	}
}