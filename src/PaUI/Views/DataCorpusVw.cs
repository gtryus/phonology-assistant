using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using SIL.FieldWorks.Common.UIAdapters;
using SIL.Pa.UI.Controls;
using SilUtils;
using SIL.Pa.UI.Dialogs;

namespace SIL.Pa.UI.Views
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// 
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public partial class DataCorpusVw : UserControl, IxCoreColleague, ITabView
	{
		private PaWordListGrid m_grid;
		private WordListCache m_cache;
		private ITMAdapter m_tmAdapter;
		private SortOptionsDropDown m_phoneticSortOptionsDropDown;
		private bool m_rawRecViewOn = true;
		private bool m_activeView;
		private PlaybackSpeedAdjuster m_playbackSpeedAdjuster;
		private bool m_initialDock = true;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public DataCorpusVw()
		{
			if (!PaApp.DesignMode)
			{
				PaApp.InitializeProgressBarForLoadingView(
					Properties.Resources.kstidDataCorpusViewText, 2);
			}

			InitializeComponent();
			Name = "DataCorpusVw";

			if (PaApp.DesignMode)
				return;

			PaApp.IncProgressBar();
			LoadToolbar();
			PaApp.IncProgressBar();
			LoadWindow();
			PaApp.UninitializeProgressBar();

			base.DoubleBuffered = true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void LoadToolbar()
		{
			if (m_tmAdapter != null)
			{
				PaApp.UnPrepareAdapterForLocalizationSupport(m_tmAdapter);
				m_tmAdapter.Dispose();
			}

			m_tmAdapter = AdapterHelper.CreateTMAdapter();

			if (m_grid != null)
				m_grid.TMAdapter = m_tmAdapter;

			if (m_tmAdapter != null)
			{
				PaApp.PrepareAdapterForLocalizationSupport(m_tmAdapter);
				m_tmAdapter.LoadControlContainerItem += m_tmAdapter_LoadControlContainerItem;
				string[] defs = new string[1];
				defs[0] = Path.Combine(Application.StartupPath, "DataCorpusTMDefinition.xml");
				m_tmAdapter.Initialize(this, PaApp.MsgMediator, PaApp.ApplicationRegKeyPath, defs);
				m_tmAdapter.AllowUpdates = true;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Give the toolbar/menu adapter the playback speed adjuster control.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private Control m_tmAdapter_LoadControlContainerItem(string name)
		{
			if (name == "tbbAdjustPlaybackSpeed")
			{
				if (m_playbackSpeedAdjuster == null)
				{
					m_playbackSpeedAdjuster = new PlaybackSpeedAdjuster();
					m_playbackSpeedAdjuster.lnkPlay.Click += HandlePlaybackSpeedAdjusterPlayClick;
					m_playbackSpeedAdjuster.Disposed += m_playbackSpeedAdjuster_Disposed;
				}
				
				return m_playbackSpeedAdjuster;
			}

			return null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void m_playbackSpeedAdjuster_Disposed(object sender, EventArgs e)
		{
			m_playbackSpeedAdjuster.lnkPlay.Click -= HandlePlaybackSpeedAdjusterPlayClick;
			m_playbackSpeedAdjuster.Disposed -= m_playbackSpeedAdjuster_Disposed;
			m_playbackSpeedAdjuster = null;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a wordEntry list grid with the specified cache and adds it to the form.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Initialize(WordListCache cache)
		{
			m_cache = cache;

			if (m_grid != null)
				m_grid.Cache = m_cache;
			else
			{
				m_grid = new PaWordListGrid(cache, GetType(), false);
				m_grid.TMAdapter = m_tmAdapter;

				// Even thought the grid is docked, setting it's size here prevents the user
				// from seeing that split second during which time the grid goes from it's
				// small, default size to its docked size.
				m_grid.Size = new Size(splitOuter.Panel1.Width, splitOuter.Panel1.Height);

				m_grid.Name = Name + "Grid";
				m_grid.LoadSettings();
				m_grid.RowEnter += m_grid_RowEnter;
				m_grid.Visible = false;
				splitOuter.Panel1.Controls.Add(m_grid);
				m_grid.Visible = true;
				m_grid.TabIndex = 0;
				m_grid.Focus();
				m_grid.SortOptions = PaApp.Project.DataCorpusVwSortOptions;
				m_grid.IsCurrentPlaybackGrid = true;
				m_grid.UseWaitCursor = false;
				m_grid.Cursor = Cursors.Default;
			}

			// This will enforce an update of the record pane.
			rtfRecVw.UpdateRecord(m_grid.GetRecord(), true);
		}

		#region Method for loading
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Called when the desire is to build a word list that is not a list of find Phone
		/// search results.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void LoadWindow()
		{
			WordListCache cache = new WordListCache();
			foreach (WordCacheEntry entry in PaApp.WordCache)
				cache.Add(entry);

			Initialize(cache);
		}

		#endregion

		#region ITabView Members
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool ActiveView
		{
			get { return m_activeView; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SetViewActive(bool makeActive, bool isDocked)
		{
			m_activeView = makeActive;

			if (makeActive)
			{
				FindInfo.Grid = m_grid;

				if (isDocked && m_grid != null)
				{
					m_grid.SetStatusBarText();
					m_grid.Focus();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Form OwningForm
		{
			get { return FindForm(); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnBeginViewUnDocking(object args)
		{
			if (args == this)
				SaveSettings();

			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves some misc. settings.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SaveSettings()
		{
			m_grid.SaveSettings();
			
			float splitRatio = splitOuter.SplitterDistance / (float)splitOuter.Height;
			PaApp.SettingsHandler.SaveSettingsValue(Name, "splitratio", splitRatio);
			PaApp.SettingsHandler.SaveSettingsValue(Name, "recordpanevisible", RawRecViewOn);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnBeginViewClosing(object args)
		{
			if (args == this)
				SaveSettings();

			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnBeginViewDocking(object args)
		{
			if (args == this && IsHandleCreated)
				SaveSettings();

			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnViewDocked(object args)
		{
			if (args == this)
			{
				try
				{
					// These are in a try/catch because sometimes they might throw an exception
					// in rare cases. The exception has to do with a condition in the underlying
					// .Net framework that I haven't been able to make sense of. Anyway, if an
					// exception is thrown, no big deal, the splitter distances will just be set
					// to their default values.
					float splitRatio = PaApp.SettingsHandler.GetFloatSettingsValue(Name, "splitratio", 0.8f);
					splitOuter.SplitterDistance = (int)(splitOuter.Height * splitRatio);
				}
				catch { }

				// Don't need to load the tool bar or menus if this is the first time
				// the view was docked since that all gets done during construction.
				if (m_initialDock)
					m_initialDock = false;
				else
				{
					// The toolbar has to be recreated each time the view is removed from it's
					// (undocked) form and docked back into the main form. The reason has to
					// do with tooltips. They seem to form an attachment, somehow, with the
					// form that owns the controls the tooltip is extending. When that form
					// gets pulled out from under the tooltips, sometimes the program will crash.
					LoadToolbar();
				}

				if (m_grid != null)
					m_grid.SetStatusBarText();
			}

			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the view's toolbar/menu adapter.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ITMAdapter TMAdapter
		{
			get { return m_tmAdapter; }
		}

		#endregion

		#region Playback related methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandlePlaybackSpeedAdjusterPlayClick(object sender, EventArgs e)
		{
			m_tmAdapter.HideBarItemsPopup("tbbAdjustPlaybackSpeedParent");
			m_tmAdapter.HideBarItemsPopup("tbbPlayback");
			m_grid.OnPlayback(null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnDropDownAdjustPlaybackSpeed(object args)
		{
			if (!m_activeView || m_grid == null ||
				m_grid.Cache == null)
			{
				return false;
			}

			m_playbackSpeedAdjuster.PlaybackSpeed =
				PaApp.SettingsHandler.GetIntSettingsValue(GetType().Name, "playbackspeed", 100);

			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnDropDownClosedAdjustPlaybackSpeed(object args)
		{
			if (!m_activeView)
				return false;

			PaApp.SettingsHandler.SaveSettingsValue(GetType().Name, "playbackspeed",
				m_playbackSpeedAdjuster.PlaybackSpeed);
		
			return true;
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			RawRecViewOn =
				PaApp.SettingsHandler.GetBoolSettingsValue(Name, "recordpanevisible", true);

			OnViewDocked(this);
			m_initialDock = true;
			Application.DoEvents();
			m_grid.Focus();
		}
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Updates the record pane with the raw record query for the current row.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		void m_grid_RowEnter(object sender, DataGridViewCellEventArgs e)
		{
			rtfRecVw.UpdateRecord(m_grid.GetRecord(e.RowIndex));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Updates the record view after a sort has taken place since the grid's RowEnter
		/// event doesn't seem to take care of it.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnWordListGridSorted(object args)
		{
			PaWordListGrid grid = args as PaWordListGrid;
			if (grid != m_grid)
				return false;

			rtfRecVw.UpdateRecord(m_grid.GetRecord(m_grid.CurrentCellAddress.Y));
			return true;
		}

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public PaWordListGrid WordListGrid
		{
			get { return m_grid; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool RawRecViewOn
		{
			get { return m_rawRecViewOn; }
			set
			{
				if (m_rawRecViewOn != value)
				{
					m_rawRecViewOn = value;
					splitOuter.Panel2Collapsed = !value;
					Padding padding = splitOuter.Panel1.Padding;
					padding = new Padding(padding.Left, padding.Top, padding.Right,
						(value ? 0 : splitOuter.Panel2.Padding.Bottom));
					splitOuter.Panel1.Padding = padding;
				}
			}
		}

		#endregion

		#region Message Handlers
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnViewUndocked(object args)
		{
			if (args == this)
				m_grid.SetStatusBarText();

			return false;
		}
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnDropDownGroupByFieldParent(object args)
		{
			ToolBarPopupInfo itemProps = args as ToolBarPopupInfo;
			if (itemProps == null || !m_activeView)
				return false;

			m_grid.BuildGroupByMenu(itemProps.Name, m_tmAdapter);
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateGroupByFieldParent(object args)
		{
			TMItemProperties itemProps = args as TMItemProperties;
			if (!m_activeView || itemProps == null || itemProps.Name.StartsWith("tbb"))
				return false;

			m_grid.BuildGroupByMenu(itemProps.Name, PaApp.TMAdapter);
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnGroupBySortedField(object args)
		{
			if (!m_activeView)
				return false;

			if (m_grid.IsGroupedByField)
				m_grid.GroupByField = null;
			else if (m_grid.SortOptions.SortInformationList != null &&
				m_grid.SortOptions.SortInformationList.Count > 0)
			{
				m_grid.GroupByField = m_grid.SortOptions.SortInformationList[0].FieldInfo;
				if (PaApp.SettingsHandler.GetBoolSettingsValue("wordlists", "collapseongrouping", false))
					m_grid.ToggleGroupExpansion(false);
			}

			if (!m_grid.CurrentCell.Displayed && m_grid.CurrentCell != null)
				m_grid.ScrollRowToMiddleOfGrid(m_grid.CurrentCell.RowIndex);

			FindInfo.ResetStartSearchCell(true);
			FindInfo.CanFindAgain = true;
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateGroupBySortedField(object args)
		{
			TMItemProperties itemProps = args as TMItemProperties;
			if (!m_activeView || itemProps == null)
				return false;

			bool enable = (m_grid != null && m_grid.Cache != null && m_grid.RowCount > 1);
			
			if (m_grid.RowCount == 0)
			{
				if (itemProps.Enabled)
				{
					itemProps.Visible = true;
					itemProps.Checked = false;
					itemProps.Enabled = false;
					itemProps.Update = true;
				}
			}
			else if (itemProps.Checked != m_grid.IsGroupedByField || enable != itemProps.Enabled)
			{
				itemProps.Visible = true;
				itemProps.Enabled = enable;
				itemProps.Checked = m_grid.IsGroupedByField;
				itemProps.Update = true;
			}

			return true;
		}

		///// ------------------------------------------------------------------------------------
		///// <summary>
		///// 
		///// </summary>
		///// ------------------------------------------------------------------------------------
		//protected bool OnUpdateShowCIEResults(object args)
		//{
		//    TMItemProperties itemProps = args as TMItemProperties;
		//    if (!m_activeView || itemProps == null)
		//        return false;

		//    if (itemProps.Enabled || itemProps.Checked)
		//    {
		//        itemProps.Visible = true;
		//        itemProps.Enabled = false;
		//        itemProps.Checked = false;
		//        itemProps.Update = true;
		//    }

		//    return true;
		//}
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handle the logic for all methods OnUpdateEditFind(Next/Previous)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private bool HandleFindItemUpdate(TMItemProperties itemProps, bool enableAllow)
		{
			if (!m_activeView || itemProps == null)
				return false;

			bool enable = (enableAllow && m_grid != null &&
				m_grid.Cache != null && m_grid.RowCount > 0);

			if (itemProps.Enabled != enable)
			{
				itemProps.Enabled = enable;
				itemProps.Visible = true;
				itemProps.Update = true;
			}

			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handle OnUpdateEditFind.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateEditFind(object args)
		{
			return HandleFindItemUpdate(args as TMItemProperties, true);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handle OnUpdateEditFindNext.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateEditFindNext(object args)
		{
			return HandleFindItemUpdate(args as TMItemProperties, FindInfo.CanFindAgain);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handle OnUpdateEditFindPrevious.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateEditFindPrevious(object args)
		{
			return HandleFindItemUpdate(args as TMItemProperties, FindInfo.CanFindAgain);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnExpandAllGroups(object args)
		{
			if (!m_activeView)
				return false;

			m_grid.ToggleGroupExpansion(true);
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateExpandAllGroups(object args)
		{
			TMItemProperties itemProps = args as TMItemProperties;
			if (!m_activeView || itemProps == null)
				return false;

			if (itemProps.Enabled != (m_grid.IsGroupedByField && !m_grid.AllGroupsExpanded))
			{
				itemProps.Visible = true;
				itemProps.Enabled = (m_grid.IsGroupedByField && !m_grid.AllGroupsExpanded);
				itemProps.Update = true;
			}

			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnCollapseAllGroups(object args)
		{
			if (!m_activeView)
				return false;

			m_grid.ToggleGroupExpansion(false);
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateCollapseAllGroups(object args)
		{
			TMItemProperties itemProps = args as TMItemProperties;
			if (!m_activeView || itemProps == null)
				return false;

			if (itemProps.Enabled != (m_grid.IsGroupedByField && !m_grid.AllGroupsCollapsed))
			{
				itemProps.Visible = true;
				itemProps.Enabled = (m_grid.IsGroupedByField && !m_grid.AllGroupsCollapsed);
				itemProps.Update = true;
			}

			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Toggles showing the record pane below the tab groups.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnShowRecordPane(object args)
		{
			if (!m_activeView)
				return false;

			RawRecViewOn = !RawRecViewOn;
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Toggles showing the record pane below the tab groups.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateShowRecordPane(object args)
		{
			TMItemProperties itemProps = args as TMItemProperties;
			if (!m_activeView || itemProps == null)
				return false;

			bool enable = (m_grid != null);

			if (itemProps.Checked != m_rawRecViewOn || enable != itemProps.Enabled)
			{
				itemProps.Visible = true;
				itemProps.Enabled = enable;
				itemProps.Checked = m_rawRecViewOn;
				itemProps.Update = true;
			}
			
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This method gets called when the CV patterns get changed in the options dialog.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnCVPatternsChanged(object args)
		{
			return OnRecordViewOptionsChanged(args);
		}
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Update the record view when the user changed the order or visibility of fields.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnRecordViewOptionsChanged(object args)
		{
			rtfRecVw.UpdateRecord(m_grid.GetRecord(), true);
			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnDataSourcesModified(object args)
		{
			int savCurrRowIndex = 0;
			int savCurrColIndex = 0;
			int savFirstRowIndex = 0;
			SortOptions savSortOptions = null;

			if (m_grid != null)
			{
				// Save the index of the row that's current and the index of the first visible row.
				savCurrRowIndex = (m_grid.CurrentRow != null ? m_grid.CurrentRow.Index : 0);
				savCurrColIndex = (m_grid.CurrentCell != null ? m_grid.CurrentCell.ColumnIndex : 0);
				savFirstRowIndex = m_grid.FirstDisplayedScrollingRowIndex;
				
				// Save the current sort options
				savSortOptions = m_grid.SortOptions;
			}

			// Update the fonts in case a custom field's name
			// has changed (since each field has it's own font).
			rtfRecVw.UpdateFonts();

			// Update the record in case we're pointing to a new record or it's data changed.
			rtfRecVw.UpdateRecord(null);

			// Rebuild the contents of the window.
			LoadWindow();

			// Restore the current row to what it was before rebuilding.
			// Then make sure the row is visible.
			if (m_grid != null)
			{
				m_grid.PostDataSourceModifiedRestore(
					savCurrRowIndex, savCurrColIndex, savFirstRowIndex, savSortOptions);
			}

			return false;
		}
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This method gets called when the font(s) get changed in the options dialog.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnPaFontsChanged(object args)
		{
			rtfRecVw.UpdateFonts();

			// Return false to allow other windows to update their fonts.
			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Compares the grid sent in args with the data corpus' grid.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnCompareGrid(object args)
		{
			PaWordListGrid grid = args as PaWordListGrid;
			return (grid != null && m_grid == grid);
		}

		#endregion

		#region Export Message Handlers
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Display the RtfExportDlg form.
		/// </summary>
		/// <param name="args"></param>
		/// <returns>true if the message was handled</returns>
		/// ------------------------------------------------------------------------------------
		protected bool OnExportAsRTF(object args)
		{
			if (!m_activeView)
				return false;

			RtfExportDlg rtfExp = new RtfExportDlg(m_grid);
			rtfExp.ShowDialog(this);
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateExportAsRTF(object args)
		{
			TMItemProperties itemProps = args as TMItemProperties;
			if (!m_activeView || itemProps == null)
				return false;

			itemProps.Visible = true;
			itemProps.Enabled = m_activeView;
			itemProps.Update = true;
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnExportAsHTML(object args)
		{
			if (!m_activeView)
				return false;

			string defaultHTMLFileName =
				string.Format(Properties.Resources.kstidDataCorpusHTMLFileName,
				PaApp.Project.Language);

			string outputFileName = HTMLGridWriter.Export(m_grid, defaultHTMLFileName,
				new[] { Properties.Resources.kstidDataCorpusHTMLChartType });

			if (File.Exists(outputFileName))
				LaunchHTMLDlg.PostExportProcess(FindForm(), outputFileName);

			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateExportAsHTML(object args)
		{
			TMItemProperties itemProps = args as TMItemProperties;
			if (!m_activeView || itemProps == null)
				return false;

			itemProps.Visible = true;
			itemProps.Enabled = true;
			itemProps.Update = true;
			return true;
		}
		#endregion

		#region Phonetic Sort methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnDropDownDataCorpusPhoneticSort(object args)
		{
			if (!m_activeView)
				return false;

			ToolBarPopupInfo itemProps = args as ToolBarPopupInfo;
			if (itemProps == null)
				return false;

			m_phoneticSortOptionsDropDown =
				new SortOptionsDropDown(m_grid.SortOptions, false);

			m_phoneticSortOptionsDropDown.SortOptionsChanged += HandlePhoneticSortOptionsChanged;
			itemProps.Control = m_phoneticSortOptionsDropDown;
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Called when sort options change.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandlePhoneticSortOptionsChanged(SortOptions sortOptions)
		{
			m_grid.SortOptions = sortOptions;
			rtfRecVw.UpdateRecord(m_grid.GetRecord());
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnDropDownClosedDataCorpusPhoneticSort(object args)
		{
			if (!m_activeView)
				return false;

			m_phoneticSortOptionsDropDown = null;
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool OnUpdateDataCorpusPhoneticSort(object args)
		{
			TMItemProperties itemProps = args as TMItemProperties;
			if (!m_activeView || itemProps == null)
				return false;

			if (!itemProps.Enabled)
			{
				itemProps.Visible = true;
				itemProps.Enabled = true;
				itemProps.Update = true;
			}
			
			return true;
		}
		
		#endregion

		#region IxCoreColleague Members
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Init(Mediator mediator, XmlNode configurationParameters)
		{
			// Not used in PA.
		}

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