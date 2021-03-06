﻿namespace SIL.Pa.UI.Dialogs
{
	partial class UserInterfaceOptionsPage
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.cboUILanguage = new L10NSharp.UI.UILanguageComboBox();
			this.lblUILanguage = new System.Windows.Forms.Label();
			this._tableLayoutOuter = new System.Windows.Forms.TableLayoutPanel();
			this._linkShowLocalizationDialogBox = new System.Windows.Forms.LinkLabel();
			this.locExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._tableLayoutOuter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.locExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// cboUILanguage
			// 
			this.cboUILanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboUILanguage.DisplayMember = "NativeName";
			this.cboUILanguage.DropDownHeight = 200;
			this.cboUILanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboUILanguage.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.cboUILanguage.FormattingEnabled = true;
			this.cboUILanguage.IntegralHeight = false;
			this.locExtender.SetLocalizableToolTip(this.cboUILanguage, null);
			this.locExtender.SetLocalizationComment(this.cboUILanguage, null);
			this.locExtender.SetLocalizationPriority(this.cboUILanguage, L10NSharp.LocalizationPriority.NotLocalizable);
			this.locExtender.SetLocalizingId(this.cboUILanguage, "UserInterfaceOptionsPage.cboUILanguage");
			this.cboUILanguage.Location = new System.Drawing.Point(0, 18);
			this.cboUILanguage.Margin = new System.Windows.Forms.Padding(0);
			this.cboUILanguage.Name = "cboUILanguage";
			this.cboUILanguage.ShowOnlyLanguagesHavingLocalizations = true;
			this.cboUILanguage.Size = new System.Drawing.Size(189, 23);
			this.cboUILanguage.TabIndex = 3;
			// 
			// lblUILanguage
			// 
			this.lblUILanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblUILanguage.AutoSize = true;
			this.locExtender.SetLocalizableToolTip(this.lblUILanguage, null);
			this.locExtender.SetLocalizationComment(this.lblUILanguage, null);
			this.locExtender.SetLocalizingId(this.lblUILanguage, "DialogBoxes.OptionsDlg.UserInterfaceTab.UILanguageLabel");
			this.lblUILanguage.Location = new System.Drawing.Point(0, 0);
			this.lblUILanguage.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
			this.lblUILanguage.Name = "lblUILanguage";
			this.lblUILanguage.Size = new System.Drawing.Size(189, 13);
			this.lblUILanguage.TabIndex = 2;
			this.lblUILanguage.Text = "User Interface Language:";
			// 
			// _tableLayoutOuter
			// 
			this._tableLayoutOuter.BackColor = System.Drawing.Color.Transparent;
			this._tableLayoutOuter.ColumnCount = 1;
			this._tableLayoutOuter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayoutOuter.Controls.Add(this.lblUILanguage, 0, 0);
			this._tableLayoutOuter.Controls.Add(this.cboUILanguage, 0, 1);
			this._tableLayoutOuter.Controls.Add(this._linkShowLocalizationDialogBox, 0, 2);
			this._tableLayoutOuter.Location = new System.Drawing.Point(15, 15);
			this._tableLayoutOuter.Name = "_tableLayoutOuter";
			this._tableLayoutOuter.RowCount = 3;
			this._tableLayoutOuter.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutOuter.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutOuter.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutOuter.Size = new System.Drawing.Size(189, 127);
			this._tableLayoutOuter.TabIndex = 4;
			// 
			// _linkShowLocalizationDialogBox
			// 
			this._linkShowLocalizationDialogBox.AutoSize = true;
			this.locExtender.SetLocalizableToolTip(this._linkShowLocalizationDialogBox, null);
			this.locExtender.SetLocalizationComment(this._linkShowLocalizationDialogBox, null);
			this.locExtender.SetLocalizationPriority(this._linkShowLocalizationDialogBox, L10NSharp.LocalizationPriority.NotLocalizable);
			this.locExtender.SetLocalizingId(this._linkShowLocalizationDialogBox, "DialogBoxes.UserInterfaceLanguageDlg.IWantToLocalizeLink");
			this._linkShowLocalizationDialogBox.Location = new System.Drawing.Point(0, 56);
			this._linkShowLocalizationDialogBox.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
			this._linkShowLocalizationDialogBox.Name = "_linkShowLocalizationDialogBox";
			this._linkShowLocalizationDialogBox.Size = new System.Drawing.Size(187, 26);
			this._linkShowLocalizationDialogBox.TabIndex = 4;
			this._linkShowLocalizationDialogBox.TabStop = true;
			this._linkShowLocalizationDialogBox.Text = "I want to localize Phonology Assistant for another language...";
			// 
			// locExtender
			// 
			this.locExtender.LocalizationManagerId = "Pa";
			// 
			// UserInterfaceOptionsPage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.Controls.Add(this._tableLayoutOuter);
			this.locExtender.SetLocalizableToolTip(this, null);
			this.locExtender.SetLocalizationComment(this, null);
			this.locExtender.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this.locExtender.SetLocalizingId(this, "UserInterfaceOptionsPage.UserInterfaceOptionsPage");
			this.Name = "UserInterfaceOptionsPage";
			this.Size = new System.Drawing.Size(274, 164);
			this._tableLayoutOuter.ResumeLayout(false);
			this._tableLayoutOuter.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.locExtender)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private L10NSharp.UI.UILanguageComboBox cboUILanguage;
		private System.Windows.Forms.Label lblUILanguage;
		private System.Windows.Forms.TableLayoutPanel _tableLayoutOuter;
		protected L10NSharp.UI.L10NSharpExtender locExtender;
		private System.Windows.Forms.LinkLabel _linkShowLocalizationDialogBox;
	}
}
