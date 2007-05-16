using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SIL.SpeechTools.Utils;
using SIL.Pa.Data;
using SIL.Pa.Controls;
using SIL.Pa.Resources;

namespace SIL.Pa.Dialogs
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// 
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public partial class PaProjectDlg : OKCancelDlgBase
	{
		private SilGrid m_grid;
		private PaProject m_project;
		private bool m_newProject;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public PaProjectDlg() : this(null)
		{
			Text = Properties.Resources.kstidNewProjectSettingsDlgCaption;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public PaProjectDlg(PaProject project) : base()
		{
			// Make sure to save the project's field info. list because we may change it while working in
			// this dialog or it's child dialogs (more specifically the custom fields dialog).
			if (project != null)
				project.FieldInfo.Save(project);

			InitializeComponent();

			Text = Properties.Resources.kstidProjectSettingsDlgCaption;
			m_newProject = (project == null);

			pnlGridHdg.Font = FontHelper.UIFont;
			lblLanguage.Font = FontHelper.UIFont;
			lblSpeaker.Font = FontHelper.UIFont;
			lblTranscriber.Font = FontHelper.UIFont;
			lblProjName.Font = FontHelper.UIFont;
			lblComments.Font = FontHelper.UIFont;
			txtLanguage.Font = FontHelper.UIFont;
			txtSpeaker.Font = FontHelper.UIFont;
			txtTranscriber.Font = FontHelper.UIFont;
			txtProjName.Font = FontHelper.UIFont;
			txtComments.Font = FontHelper.UIFont;

			// Changing the font sometimes (depending on the font) moves the text box up.
			txtComments.Top = txtTranscriber.Bottom - txtComments.Height;

			BuildGrid();
			pnlGridHdg.ControlReceivingFocusOnMnemonic = m_grid;

			PaApp.SettingsHandler.LoadFormProperties(this);

			if (project == null)
				m_project = new PaProject(true);
			else
			{
				m_project = project;
				txtProjName.Text = project.ProjectName;
				txtLanguage.Text = project.Language;
				txtTranscriber.Text = project.Transcriber;
				txtSpeaker.Text = project.SpeakerName;
				txtComments.Text = project.Comments;
				LoadGrid(-1);
			}

			FwDataSourcePrep();

			// If the project contains FW data sources, then an attempt must be made to start
			// SQL server. If there are no FW data sources, then only attempt to start SQL
			// server if the AutoStartSQLServer flag is true (which it is by default). The only
			// way for the flag to be false is via an undocumented entry in the settings file
			// (e.g. <setting id="autostart" sqlserver="False" />)
			if (PaApp.AutoStartSQLServer && !FwDBUtils.IsSQLServerStarted)
				FwDBUtils.StartSQLServer(false);

			cmnuAddFwDataSource.Enabled = (FwDBUtils.FwDatabaseInfoList != null);
			
			m_dirty = false;
			Application.Idle += new EventHandler(Application_Idle);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Check for any FW data sources in the project. If any are found, attempt to start 
		/// SQL server if it isn't already. Also backup the writing system information in case
		/// the user goes to the FW data source properties dialog to make changes to the
		/// writing system information.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void FwDataSourcePrep()
		{
			if (m_project.DataSources == null)
				return;

			bool sqlSrvStarted = false;

			foreach (PaDataSource ds in m_project.DataSources)
			{
				if (ds.DataSourceType == DataSourceType.FW)
				{
					ds.FwDataSourceInfo.BackupWritingSystemInfo();

					if (!sqlSrvStarted)
					{
						FwDBUtils.StartSQLServer(true);
						sqlSrvStarted = true;
					}
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Update button enabled states during idle cycles.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		void Application_Idle(object sender, EventArgs e)
		{
			bool enableDelButton = (m_grid.SelectedRows != null && m_grid.SelectedRows.Count > 0);
			if (enableDelButton != btnRemove.Enabled)
				btnRemove.Enabled = enableDelButton;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void BuildGrid()
		{
		    m_grid = new SilGrid();
		    m_grid.Name = Name + "Grid";
		    m_grid.AutoGenerateColumns = false;
		    m_grid.Dock = DockStyle.Fill;
			m_grid.BorderStyle = BorderStyle.None;
		    m_grid.Font = FontHelper.UIFont;
		    m_grid.AllowUserToOrderColumns = false;
			m_grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Raised;
			m_grid.RowEnter += new DataGridViewCellEventHandler(m_grid_RowEnter);

		    DataGridViewColumn col = SilGrid.CreateSilButtonColumn("sourcefiles");
		    col.ReadOnly = true;
		    col.Width = 250;
		    col.HeaderText = Properties.Resources.kstidDataSourceGridSourcFile;
			((SilButtonColumn)col).ButtonWidth = 20;
			((SilButtonColumn)col).DrawTextWithEllipsisPath = true;
			((SilButtonColumn)col).ButtonText = Properties.Resources.kstidDataSourcePropertiesButtonText;
			((SilButtonColumn)col).ButtonClicked +=
				new DataGridViewCellMouseEventHandler(HandleDataSourceFilePropertiesClick);
			m_grid.Columns.Add(col);

		    col = SilGrid.CreateTextBoxColumn("type");
		    col.ReadOnly = true;
		    col.Width = 75;
		    col.HeaderText = Properties.Resources.kstidDataSourceGridType;
		    m_grid.Columns.Add(col);

		    col = SilGrid.CreateSilButtonColumn("xslt");
		    col.ReadOnly = true;
		    col.Width = 170;

		    col.HeaderText = Properties.Resources.kstidDataSourceGridXSLT;
			((SilButtonColumn)col).ButtonWidth = 20;
			((SilButtonColumn)col).DrawTextWithEllipsisPath = true;
			((SilButtonColumn)col).ButtonText = Properties.Resources.kstidXSLTColButtonText;
			((SilButtonColumn)col).ButtonToolTip = Properties.Resources.kstidXSLTColButtonToolTip;
			((SilButtonColumn)col).ButtonClicked +=
				new DataGridViewCellMouseEventHandler(HandleSpecifyXSLTClick);
			m_grid.Columns.Add(col);

		    PaApp.SettingsHandler.LoadGridProperties(m_grid);

			// When xslt transforms are supported when reading data, then this should become visible.
			m_grid.Columns["xslt"].Visible = false;

			pnlGrid.Controls.Add(m_grid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the grid with the project's query source specifications.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void LoadGrid(int preferredRow)
		{
			if (preferredRow == -1)
				preferredRow = (m_grid.CurrentRow != null ? m_grid.CurrentRow.Index : 0);
			
			// Clear the grid and start over.
			m_grid.Rows.Clear();

			// Check if there are any query sources specified for this project.
			if (m_project.DataSources == null || m_project.DataSources.Count == 0)
				return;

			m_grid.Rows.Add(m_project.DataSources.Count);

			for (int i = 0; i < m_project.DataSources.Count; i++)
			{
				m_grid.Rows[i].Cells["sourcefiles"].Value = m_project.DataSources[i].ToString();
				m_grid.Rows[i].Cells["type"].Value = m_project.DataSources[i].DataSourceTypeString;
				m_grid.Rows[i].Cells["xslt"].Value = m_project.DataSources[i].XSLTFile;
			}

			// If the current row used to be the last row and that last row no
			// longer exists, then make the new current row the new last row.
			if (preferredRow == m_grid.Rows.Count)
				preferredRow--;

			// Try to restore the current row to what it was before removing all the rows.
			if (m_grid.Rows.Count > 0 && preferredRow >= 0 && preferredRow < m_grid.Rows.Count)
				m_grid.CurrentCell = m_grid[0, preferredRow];
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public PaProject Project
		{
		    get { return m_project; }
		    set { m_project = value; }
		}

		#region Saving Settings and Verifying/Saving changes
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			Application.Idle -= Application_Idle;

			base.OnFormClosing(e);

			if (!e.Cancel && DialogResult != DialogResult.OK)
			{
				// If the project isn't new and the user is NOT saving the project
				// settings then reload the original field info. for the project.
				if (!m_newProject)
				{
					m_project.LoadFieldInfo();

					// Throw out changes made to FW data source writing
					// system information since the user has canceled.
					foreach (PaDataSource ds in m_project.DataSources)
					{
						if (ds.DataSourceType == DataSourceType.FW)
							ds.FwDataSourceInfo.RestoreBackedupWritingSystemInfo();
					}
				}

				m_project = null;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void SaveSettings()
		{
			base.SaveSettings();
			PaApp.SettingsHandler.SaveFormProperties(this);
			PaApp.SettingsHandler.SaveGridProperties(m_grid);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override bool IsDirty
		{
			get { return (m_dirty || m_grid.IsDirty); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override bool Verify()
		{
			string msg = null;
			int offendingIndex = -1;
			Control offendingCtrl = null;

			// Verify a project name was specified.
			if (txtProjName.Text.Trim() == string.Empty)
			{
				msg = Properties.Resources.kstidDataSourceNoProjName;
				offendingCtrl = txtProjName;
			}
			else if (txtLanguage.Text.Trim() == string.Empty)
			{
				msg = Properties.Resources.kstidDataSourceNoLangName;
				offendingCtrl = txtLanguage;
			}
			else
			{
				for (int i = 0; i < m_project.DataSources.Count; i++)
				{
					if (m_project.DataSources[i].DataSourceType == DataSourceType.PAXML)
						continue;

					if (m_project.DataSources[i].DataSourceType == DataSourceType.XML &&
						string.IsNullOrEmpty(m_project.DataSources[i].XSLTFile))
					{
						// No XSLT file was specified
						offendingIndex = i;
						msg = string.Format(Properties.Resources.kstidDataSourceNoXSLT,
							m_project.DataSources[i].DataSourceFile);
						break;
					}
					else if (!m_project.DataSources[i].MappingsExist &&
						(m_project.DataSources[i].DataSourceType == DataSourceType.SFM ||
						m_project.DataSources[i].DataSourceType == DataSourceType.Toolbox))
					{
						// No mappings have been specified.
						offendingIndex = i;
						msg = string.Format(Properties.Resources.kstidDataSourceNoMappings,
							m_project.DataSources[i].DataSourceFile);
						break;
					}
					else if (m_project.DataSources[i].DataSourceType == DataSourceType.FW &&
						m_project.DataSources[i].FwSourceDirectFromDB &&
						!m_project.DataSources[i].FwDataSourceInfo.IsInfoComplete)
					{
						// FW data source information is incomplete.
						offendingIndex = i;
						msg = string.Format(Properties.Resources.kstidFwDataSourceInfoIncompleteMsg,
							m_project.DataSources[i].FwDataSourceInfo.ToString());
						break;
					}
				}
			}

			if (msg != null)
			{
				STUtils.STMsgBox(msg, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

				// Give the appropriate control focus.
				if (offendingCtrl != null)
					offendingCtrl.Focus();
				else
				{
					// Clear all selected rows.
					for (int i = 0; i < m_grid.Rows.Count; i++)
						m_grid.Rows[i].Selected = false;

					// Select the offending row and give the grid focus.
					m_grid.CurrentCell = m_grid[0, offendingIndex];
					m_grid.Rows[offendingIndex].Selected = true;
					m_grid.Focus();
				}

				return false;
			}
			
			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Save the changes in response to closing the dialog.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override bool SaveChanges()
		{
			// Get a project file name if the project is new.
			if (m_project.ProjectFileName == null)
			{
				m_project.ProjectFileName = GetProjectFileName();
				if (m_project.ProjectFileName == null)
					return false;
			}

			foreach (PaDataSource ds in m_project.DataSources)
			{
				if (ds.DataSourceType == DataSourceType.FW)
					ds.FwDataSourceInfo.ClearBackedupWritingSystemInfo();
			}

			m_project.ProjectName = txtProjName.Text.Trim();
			m_project.Language = txtLanguage.Text.Trim();
			m_project.Transcriber = txtTranscriber.Text.Trim();
			m_project.SpeakerName = txtSpeaker.Text.Trim();
			m_project.Comments = txtComments.Text.Trim();
			m_project.Save();

			return true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Opens the save file dialog, asking the user what file name to give his new project.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private string GetProjectFileName()
		{
		    SaveFileDialog dlg = new SaveFileDialog();
		    dlg.OverwritePrompt = true;
		    dlg.CheckFileExists = false;
		    dlg.CheckPathExists = true;
		    dlg.AddExtension = true;
		    dlg.DefaultExt = "pap";
			
			dlg.Filter = string.Format(ResourceHelper.GetString("kstidFileTypePAProject"),
				Application.ProductName) + "|" + ResourceHelper.GetString("kstidFileTypeAllFiles");
			
			dlg.ShowHelp = false;
		    dlg.Title = string.Format(Properties.Resources.kstidPAFilesCaptionSFD, Application.ProductName);
		    dlg.RestoreDirectory = false;
		    dlg.InitialDirectory = Environment.CurrentDirectory;
		    dlg.FilterIndex = 0;
			dlg.FileName = (txtProjName.Text.Trim() == string.Empty ?
				m_project.ProjectName : txtProjName.Text.Trim()) + ".pap";

		    dlg.ShowDialog();

		    return (string.IsNullOrEmpty(dlg.FileName) ? null : dlg.FileName);
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleTextChanged(object sender, EventArgs e)
		{
		    m_dirty = true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Only show the buttons in the current row under certain circumstances.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		void m_grid_RowEnter(object sender, DataGridViewCellEventArgs e)
		{
			int row = e.RowIndex;

			if (e.RowIndex >= 0 && row < m_project.DataSources.Count)
			{
				DataSourceType type = m_project.DataSources[row].DataSourceType;
				SilButtonColumn col = m_grid.Columns["sourcefiles"] as SilButtonColumn;

				((SilButtonColumn)m_grid.Columns["xslt"]).ShowButton = (type == DataSourceType.XML);
				col.ShowButton = (type == DataSourceType.SFM || type == DataSourceType.Toolbox) ||
					(type == DataSourceType.FW && m_project.DataSources[row].FwSourceDirectFromDB);

				if (col.ShowButton)
				{
					col.ButtonToolTip = (type == DataSourceType.FW ?
						Properties.Resources.kstidFwPropsButtonToolTip :
						Properties.Resources.kstidMappingsButtonToolTip);
				}
			}
		}

		#region Button click handlers
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Set a flag indicating whether or not the cancel button was pressed. That's because
		/// in the form's closing event, we don't know if a DialogResult of Cancel is due to
		/// the user clicking on the cancel button or closing the form in some other way
		/// beside clicking on the OK button.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void btnCancel_Click(object sender, EventArgs e)
		{
			m_cancelButtonPressed = true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Show the open file dialog so the user may specify a non FieldWorks data source.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void cmnuAddOtherDataSource_Click(object sender, EventArgs e)
		{
			int filterIndex = PaApp.SettingsHandler.GetIntSettingsValue("DataSourceOFD", "filter", 0);

			StringBuilder fileTypes = new StringBuilder();
			fileTypes.Append(ResourceHelper.GetString("kstidFileTypeToolboxDB"));
			fileTypes.Append("|");
			fileTypes.Append(ResourceHelper.GetString("kstidFileTypeToolboxITX"));
			fileTypes.Append("|");
			fileTypes.Append(string.Format(ResourceHelper.GetString("kstidFileTypePAXML"), Application.ProductName));
			fileTypes.Append("|");
		/*	fileTypes.Append(ResourceHelper.GetString("kstidFileTypeXML"));
			fileTypes.Append("|");		ADD WHEN WE SUPPORT XML TRANSFORMING ON DATA READ  */
			fileTypes.Append(ResourceHelper.GetString("kstidFiletypeSASoundWave"));
			fileTypes.Append("|");
			fileTypes.Append(ResourceHelper.GetString("kstidFiletypeSASoundMP3"));
			fileTypes.Append("|");
			fileTypes.Append(ResourceHelper.GetString("kstidFiletypeSASoundWMA"));
			fileTypes.Append("|");
			fileTypes.Append(ResourceHelper.GetString("kstidFileTypeAllFiles"));

			string[] filenames = PaApp.OpenFileDialog("db", fileTypes.ToString(),
				ref filterIndex, Properties.Resources.kstidDataSourceOpenFileCaption, true);

			if (filenames.Length > 0)
			{
				PaApp.SettingsHandler.SaveSettingsValue("DataSourceOFD", "filter", filterIndex);

				// Add the selected files to the data source list.
				foreach (string file in filenames)
				{
					if (!ProjectContainsDataSource(file))
						m_project.DataSources.Add(new PaDataSource(file, m_project.DefaultMappings));
				}

				LoadGrid(m_grid.Rows.Count);
				m_grid.Focus();
				m_grid.IsDirty = true;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Show the dialog to allow the user to specify a FieldWorks database as a data source.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void cmnuAddFwDataSource_Click(object sender, EventArgs e)
		{
			using (FwDatabaseDlg dlg = new FwDatabaseDlg())
			{
				if (dlg.ShowDialog() == DialogResult.OK && dlg.ChosenDatabase != null)
				{
					if (!ProjectContainsDataSource(dlg.ChosenDatabase.ToString()))
					{
						m_project.DataSources.Add(new PaDataSource(dlg.ChosenDatabase));
						LoadGrid(m_grid.Rows.Count);
					}
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void btnAdd_Click(object sender, EventArgs e)
		{
			Point pt = btnAdd.PointToScreen(new Point(0, btnAdd.Height));
			cmnuAdd.Show(pt);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void btnRemove_Click(object sender, EventArgs e)
		{
			if (STUtils.STMsgBox(Properties.Resources.kstidDataSourceDeleteConfirmation,
				MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				List<int> indexesToDelete = new List<int>();

				// Start from the end of the list.
				for (int i = m_grid.Rows.Count - 1; i >= 0; i--)
				{
					if (m_grid.Rows[i].Selected)
						indexesToDelete.Add(i);
				}

				foreach (int i in indexesToDelete)
					m_project.DataSources.RemoveAt(i);

				LoadGrid(-1);
				m_grid.Focus();
				m_grid.IsDirty = true;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleDataSourceFilePropertiesClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			PaDataSource dataSource = m_project.DataSources[e.RowIndex];

			if (dataSource.DataSourceType == DataSourceType.SFM ||
				dataSource.DataSourceType == DataSourceType.Toolbox)
			{
				ShowMappingsDialog(dataSource);
			}
			else if (dataSource.DataSourceType == DataSourceType.FW &&
				dataSource.FwSourceDirectFromDB)
			{
				ShowFwDataSourcePropertiesDialog(dataSource);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Shows the mappings dialog for SFM and Toolbox data source types.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void ShowMappingsDialog(PaDataSource dataSource)
		{
			string filename = dataSource.DataSourceFile;

			// Make sure the file exists before going to the mappings dialog.
			if (!File.Exists(filename))
			{
				STUtils.STMsgBox(
					string.Format(Properties.Resources.kstidFileMissingMsg, filename),
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

				return;
			}

			// Open the mappings dialog.
			using (SFMarkerMappingDlg dlg = new SFMarkerMappingDlg(m_project.FieldInfo, dataSource))
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					// If no default mappings yet exist for this project, then clone the ones
					// from the query source whose mappings were just specified.
					if (m_project.DefaultMappings == null || m_project.DefaultMappings.Count == 0)
					{
						m_project.DefaultMappings = new List<SFMarkerMapping>();
						foreach (SFMarkerMapping mapping in dataSource.SFMappings)
							m_project.DefaultMappings.Add(mapping.Clone());
					}

					if (((OKCancelDlgBase)dlg).ChangesWereMade)
						m_dirty = true;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Show properties dialog for FW data sources of types where the data is being read
		/// directly from an FW database as opposed to a PAXML data source with some FW
		/// information in it.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void ShowFwDataSourcePropertiesDialog(PaDataSource dataSource)
		{
			if (dataSource.FwDataSourceInfo.IsMissing)
			{
				dataSource.FwDataSourceInfo.ShowMissingMessage();
				return;
			}

			using (FwDataSourcePropertiesDlg dlg =
				new FwDataSourcePropertiesDlg(dataSource.FwDataSourceInfo))
			{
				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					if (((OKCancelDlgBase)dlg).ChangesWereMade)
						m_dirty = true;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleSpecifyXSLTClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			int filterIndex = PaApp.SettingsHandler.GetIntSettingsValue("DataSourceXSLTOFD", "filter", 0);

			string filter = ResourceHelper.GetString("kstidFileTypeXSLT") + "|" +
				ResourceHelper.GetString("kstidFileTypeAllFiles");

			string filename = PaApp.OpenFileDialog("xslt", filter, ref filterIndex,
				Properties.Resources.kstidDataSourceOpenFileXSLTCaption);

			if (filename != null)
			{
				m_project.DataSources[e.RowIndex].XSLTFile = filename;
				m_grid.Refresh();
				PaApp.SettingsHandler.SaveSettingsValue("DataSourceXSLTOFD", "filter", filterIndex);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void btnCustomFields_Click(object sender, EventArgs e)
		{
			using (CustomFieldsDlg dlg = new CustomFieldsDlg(m_project))
			{
				dlg.ShowDialog(this);
				if (((OKCancelDlgBase)dlg).ChangesWereMade)
					m_dirty = true;
			}
		}
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Returns true if the project contains a query source file with the specified name.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private bool ProjectContainsDataSource(string filename)
		{
			foreach (PaDataSource datasource in m_project.DataSources)
			{
				if (datasource.ToString().ToLower() == filename.ToLower())
					return true;
			}

			return false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void HandleHelpClick(object sender, EventArgs e)
		{
			PaApp.ShowHelpTopic(m_newProject ? "hidNewProjectSettingsDlg" : "hidProjectSettingsDlg");
		}

		#endregion

		#region Painting methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Paint the ellipsis on the button where I want them.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleButtonPaint(object sender, PaintEventArgs e)
		{
			Button btn = sender as Button;

			if (btn == null)
				return;

			using (StringFormat sf = STUtils.GetStringFormat(true))
			using (Font fnt = new Font(btn.Font.Name, 8, FontStyle.Regular, GraphicsUnit.Point))
			{
				e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
				Rectangle rc = btn.ClientRectangle;
				e.Graphics.DrawString("...", fnt, SystemBrushes.ControlText, rc, sf);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Sizes and locates one of the buttons that look like their on the grid.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private Rectangle LocateGridButton(Button btn, Rectangle rcCell)
		{
			btn.Size = new Size(rcCell.Height + 1, rcCell.Height + 1);
			Point pt = m_grid.PointToScreen(new Point(rcCell.Right - rcCell.Height - 1, rcCell.Y - 1));
			btn.Location = m_grid.Parent.PointToClient(pt);
			btn.Invalidate();
			rcCell.Width -= (rcCell.Height + 2);
			return rcCell;
		}

		#endregion
	}
}