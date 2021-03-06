// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2005-2015, SIL International.
// <copyright from='2005' to='2015' company='SIL International'>
//		Copyright (c) 2005-2015, SIL International.
//    
//		This software is distributed under the MIT License, as specified in the LICENSE.txt file.
// </copyright> 
#endregion
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SIL.SpeechTools.Utils;

namespace SIL.Pa.SearchResultAddOn
{
	public partial class NumberOfPhonesToMatchCtrl : UserControl
	{
		private string m_msgFormat;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public NumberOfPhonesToMatchCtrl()
		{
			InitializeComponent();

			m_msgFormat = lblMessage.Text;
			lnkApply.Font = FontHelper.UIFont;
			lblMessage.Font = FontHelper.UIFont;
			updnPhones.Font = FontHelper.UIFont;

			Size = new Size(tblCtrls.PreferredSize.Width + Padding.Left + Padding.Right,
				tblCtrls.PreferredSize.Height + Padding.Top + Padding.Bottom);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Environment
		{
			set 
			{
				lblMessage.Text = string.Format(m_msgFormat, value);
				Size = new Size(tblCtrls.PreferredSize.Width + Padding.Left + Padding.Right,
					tblCtrls.PreferredSize.Height + Padding.Top + Padding.Bottom);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public int NumberOfPhones
		{
			get { return (int)updnPhones.Value; }
			set { updnPhones.Value = value; }
		}
	}
}
