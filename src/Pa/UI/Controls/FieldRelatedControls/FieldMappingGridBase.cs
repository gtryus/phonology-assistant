﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SIL.Pa.Model;
using SilTools;

namespace SIL.Pa.UI.Controls
{
	public class FieldMappingGridBase : SilGrid
	{
		protected Func<string> m_sourceFieldColumnHeadingTextHandler;
		protected Func<string> m_targetFieldColumnHeadingTextHandler;

		protected List<FieldMapping> m_mappings;
		protected IEnumerable<PaField> m_potentialFields;
		protected readonly CellCustomDropDownList m_cellDropDown;
		protected readonly FontPicker m_fontPicker;

		/// ------------------------------------------------------------------------------------
		public FieldMappingGridBase()
		{
			VirtualMode = true;
			Font = FontHelper.UIFont;
			RowHeadersVisible = false;
			BorderStyle = BorderStyle.None;
			App.SetGridSelectionColors(this, true);
			
			m_cellDropDown = new CellCustomDropDownList();
			m_fontPicker = new FontPicker();
			m_fontPicker.Closed += HandleFontPickerClosed;

			AddColumns();
		}

		///// ------------------------------------------------------------------------------------
		//public FieldMappingGridBase(IEnumerable<PaField> potentialFields, IEnumerable<FieldMapping> mappings,
		//    Func<string> srcFldColHdgTextHandler, Func<string> tgtFldColHdgTextHandler)
		//    : this()
		//{
		//    m_sourceFieldColumnHeadingTextHandler = srcFldColHdgTextHandler;
		//    m_targetFieldColumnHeadingTextHandler = tgtFldColHdgTextHandler;
		//    Intialize(potentialFields, mappings, true);
		//}

		///// ------------------------------------------------------------------------------------
		//public FieldMappingGridBase(IEnumerable<PaField> potentialFields) : this()
		//{
		//    Intialize(potentialFields,
		//        potentialFields.Select(f => new FieldMapping { Field = f }), false);

		//    var col = Columns["tgtfield"] as SilButtonColumn;
		//    col.ShowButton = false;
		//    col.ReadOnly = true;
		//}

		///// ------------------------------------------------------------------------------------
		//protected virtual void Intialize(IEnumerable<PaField> potentialFields,
		//    IEnumerable<FieldMapping> mappings, bool showOnlyMappableFiels)
		//{
		//    m_potentialFields = (showOnlyMappableFiels ?
		//        potentialFields.Where(f => f.AllowUserToMap) : potentialFields);

		//    m_mappings = mappings.Select(m => m.Copy()).ToList();

		//    AddColumns();
		//    ShowFontColumn(false);
		//    RowCount = m_mappings.Count;
		//}
		
		/// ------------------------------------------------------------------------------------
		protected virtual void AddColumns()
		{
			// Create target field column.
			DataGridViewColumn col = new SilButtonColumn("tgtfield");
			((SilButtonColumn)col).ButtonStyle = SilButtonColumn.ButtonType.MinimalistCombo;
			((SilButtonColumn)col).ButtonClicked += OnFieldColumnButtonClicked;
			((SilButtonColumn)col).DrawDefaultComboButtonWidth = false;
			col.SortMode = DataGridViewColumnSortMode.NotSortable;

			var text = (m_targetFieldColumnHeadingTextHandler != null ?
				m_targetFieldColumnHeadingTextHandler() : null);

			if (string.IsNullOrEmpty(text))
			{
				text = App.LocalizeString(
					"FieldMappingGrid.TargetFieldColumnHeadingText", "Field",
					App.kLocalizationGroupUICtrls);
			}

			col.HeaderText = text;
			Columns.Add(col);

			// Create font column.
			col = new SilButtonColumn("font");
			((SilButtonColumn)col).ButtonStyle = SilButtonColumn.ButtonType.MinimalistCombo;
			((SilButtonColumn)col).ButtonClicked += OnFontColumnButtonClicked;
			((SilButtonColumn)col).DrawDefaultComboButtonWidth = false;
			col.SortMode = DataGridViewColumnSortMode.NotSortable;
			col.Visible = false;
			int i = Columns.Add(col);
			App.LocalizeObject(Columns[i], "FieldMappingGrid.FontColumnHeadingText",
				"Font", App.kLocalizationGroupUICtrls);
		}

		/// ------------------------------------------------------------------------------------
		public virtual IEnumerable<FieldMapping> Mappings
		{
			get { return m_mappings.Where(m => m.Field != null); }
		}

		/// ------------------------------------------------------------------------------------
		public virtual void ShowFontColumn(bool show)
		{
			Columns["font"].Visible = show;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Makes the target field column readonly and turns off display of the combobox
		/// drop-down button.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public virtual void LockTargetFieldColumn()
		{
			var col = Columns["tgtfield"] as SilButtonColumn;
			col.ShowButton = false;
			col.ReadOnly = true;
		}

		/// ------------------------------------------------------------------------------------
		protected int FontColumnIndex
		{
			get { return Columns["font"].Index; }
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void OnFieldColumnButtonClicked(object sender, DataGridViewCellMouseEventArgs e)
		{
			var potentialFieldNames = m_potentialFields.Select(f => f.DisplayName).ToList();
			potentialFieldNames.Insert(0, GetNoMappingText());
			m_cellDropDown.Show(this[e.ColumnIndex, e.RowIndex], potentialFieldNames);
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void OnFontColumnButtonClicked(object sender, DataGridViewCellMouseEventArgs e)
		{
			var field = GetFieldAt(e.RowIndex);

			if (field != null)
				m_fontPicker.ShowForGridCell(field.Font, this[e.ColumnIndex, e.RowIndex], true);
	
			// In the else case, display the fading message window.
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void HandleFontPickerClosed(FontPicker sender, DialogResult result)
		{
			if (result == DialogResult.OK)
			{
				var mapping = GetFieldAt(CurrentCellAddress.Y);
				mapping.Font = (Font)m_fontPicker.Font.Clone();
				AutoResizeRows(DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders);
			}
		}

		/// ------------------------------------------------------------------------------------
		protected virtual string GetNoMappingText()
		{
			return App.LocalizeString("FieldMappingGrid.NoMappingText",
				"(no mapping)", App.kLocalizationGroupUICtrls);
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnCellValueNeeded(DataGridViewCellValueEventArgs e)
		{
			if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
			{
				var mapping = m_mappings[e.RowIndex];

				switch (Columns[e.ColumnIndex].Name)
				{
					case "tgtfield": e.Value = (mapping.Field == null ? null : mapping.Field.DisplayName); break;
					case "font": e.Value = (mapping.Field == null ? null :
						GetFontDisplayString(mapping.Field.Font)); break;
				}
			}

			base.OnCellValueNeeded(e);
		}

		/// ------------------------------------------------------------------------------------
		private static string GetFontDisplayString(Font fnt)
		{
			string fmt;

			if (fnt.Bold && fnt.Italic)
			{
				fmt = App.LocalizeString("FieldMappingGrid.FontDisplayFormatAll",
						"{0}, {1}pt, Bold, Italic", App.kLocalizationGroupUICtrls);
			}
			else if (fnt.Bold)
			{
				fmt = App.LocalizeString("FieldMappingGrid.FontDisplayFormatBold",
						"{0}, {1}pt, Bold", App.kLocalizationGroupUICtrls);
			}
			else if (fnt.Italic)
			{
				fmt = App.LocalizeString("FieldMappingGrid.FontDisplayFormatItalic",
						"{0}, {1}pt, Italic", App.kLocalizationGroupUICtrls);
			}
			else
			{
				fmt = App.LocalizeString("FieldMappingGrid.FontDisplayFormat",
						"{0}, {1}pt", App.kLocalizationGroupUICtrls);
			}

			return string.Format(fmt, fnt.FontFamily.Name, (int)fnt.SizeInPoints);
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnCellValuePushed(DataGridViewCellValueEventArgs e)
		{
			base.OnCellValuePushed(e);

			if (e.ColumnIndex < 0 || e.RowIndex < 0)
				return;

			if (Columns[e.ColumnIndex].Name == "tgtfield")
			{
				var valAsString = (e.Value as string ?? string.Empty);
				valAsString = valAsString.Trim();
				var mapping = m_mappings[e.RowIndex];
				
				if (valAsString == GetNoMappingText() || valAsString == string.Empty)
				{
					mapping.Field = null;
					mapping.IsParsed = false;
					mapping.IsInterlinear = false;
				}
				else
				{
					mapping.Field =
						m_potentialFields.SingleOrDefault(f => f.DisplayName == valAsString) ??
						new PaField(valAsString, GetTypeAtOrDefault(e.RowIndex));
				}
			}

			InvalidateRow(e.RowIndex);
		}

		/// ------------------------------------------------------------------------------------
		protected virtual FieldType GetTypeAtOrDefault(int index)
		{
			var field = GetFieldAt(index);
			return (field != null ? field.Type : default(FieldType));
		}

		/// ------------------------------------------------------------------------------------
		protected virtual PaField GetFieldAt(int index)
		{
			return (index >= 0 && index < m_mappings.Count ?  m_mappings[index].Field : null);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Make sure the cells for fields that can't be parsed or interlinear are set to
		/// readonly.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
			{
				var mapping = m_mappings[e.RowIndex];
				if (Columns[e.ColumnIndex].Name == "font" && mapping.Field != null)
					e.CellStyle.Font = mapping.Field.Font;
			}
	
			base.OnCellFormatting(e);
		}

		/// ------------------------------------------------------------------------------------
		public virtual bool GetAreAnyFieldsMappedMultipleTimes()
		{
			return Mappings.Select(m => m.Field.Name)
				.Any(fname => Mappings.Count(m => m.Field.Name == fname) > 1);
		}

		/// ------------------------------------------------------------------------------------
		public virtual bool GetIsTargetFieldMapped(string fieldName)
		{
			return Mappings.Any(m => m.Field.Name == fieldName);
		}

		/// ------------------------------------------------------------------------------------
		public virtual bool GetIsSourceFieldMapped(string fieldName)
		{
			return Mappings.Any(m => m.NameInDataSource == fieldName);
		}

		/// ------------------------------------------------------------------------------------
		public virtual PaField GetMappedFieldForSourceField(string fieldName)
		{
			return (from m in Mappings where m.Field.Name == fieldName select m.Field).FirstOrDefault();
		}
	}
}