using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SIL.SpeechTools.Utils;

namespace SIL.Pa.Controls
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// 
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class ClassListView : ListView
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// The type of window for which the class list applies.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public enum ListApplicationType
		{
			FindPhoneWindow,
			ClassesDialog,
			DefineClassesDialog
		}

		public static string kMemberSubitem = "Members";
		public static string kBasedOnSubitem = "BasedOn";

		// This boolean keeps track of whether they're classes to delete when the
		// SaveChanges() method goes to save all the changes made to the class list.
		private bool m_deletedClass = false;

		private ListApplicationType m_appliesTo = ListApplicationType.ClassesDialog;
		private bool m_showMembersAndClassTypeColumns = true;
		internal Font PhoneticFont = null;
		private SortOrder m_sortOrder = SortOrder.None;
		private int m_sortColumn = -1;

		#region Construction and loading
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Constructs a new list resultView for phonetic classes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ClassListView()
		{
			base.DoubleBuffered = true;
			Name = "lvClasses";
			HeaderStyle = ColumnHeaderStyle.Clickable;
			MultiSelect = false;
			OwnerDraw = true;
			View = View.Details;
			FullRowSelect = true;
			HideSelection = false;

			// This will ensure the row height will accomodate the tallest font.
			base.Font = FontHelper.UIFont;

			AddColumns();
			//AddGroups();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing && PhoneticFont != null && PhoneticFont != FontHelper.PhoneticFont)
			{
				PhoneticFont.Dispose();
				PhoneticFont = null;
			}

			base.Dispose(disposing);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adds the columns to the list
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void AddColumns()
		{
			SmallImageList = null;
			Columns.Clear();

			// Add the column for the class name.
			ColumnHeader hdr = new ColumnHeader();
			hdr.Name = "hdr" + ClassListViewItem.kClassNameSubitem;
			hdr.Text = Properties.Resources.kstidClassListViewNameText;
			hdr.Width = 180;
			Columns.Add(hdr);

			if (m_showMembersAndClassTypeColumns)
			{
				PhoneticFont = (FontHelper.PhoneticFont.SizeInPoints <= 10 ?
					FontHelper.PhoneticFont : FontHelper.MakeFont(FontHelper.PhoneticFont, 10));

				// This will force the height of items to fit the larger of the phonetic or
				// UI fonts. I realize this is sort of a kludge, but it's a workable one.
				int itemHeight = Math.Max(PhoneticFont.Height, FontHelper.UIFont.Height);
				SmallImageList = new ImageList();
				SmallImageList.ImageSize = new Size(1, itemHeight);

				// Add a column for the pattern of the class.
				hdr = new ColumnHeader();
				hdr.Name = "hdr" + kMemberSubitem;
				hdr.Text = Properties.Resources.kstidClassListViewMembersText;
				hdr.Width = 205;
				Columns.Add(hdr);

				// Add a column for the text showing what the class is based on.
				hdr = new ColumnHeader();
				hdr.Name = "hdr" + kBasedOnSubitem;
				hdr.Text = Properties.Resources.kstidClassListViewClassTypeText;
				hdr.Width = 175;
				Columns.Add(hdr);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Loads the list with classes and their members.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void Load()
		{
			if (PaApp.DesignMode)
				return;

			Items.Clear();

			foreach (SearchClass srchClass in PaApp.Project.SearchClasses)
				Items.Add(ClassListViewItem.Create(srchClass));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			if (m_sortColumn >= 0)
			{
				// Toggle the value now because it will be toggled
				// back in the OnColumnClick method.
				m_sortOrder = (m_sortOrder == SortOrder.Ascending ?
					SortOrder.Descending : SortOrder.Ascending);

				OnColumnClick(new ColumnClickEventArgs(m_sortColumn));
			}

			SelectedItems.Clear();
			if (Items.Count > 0)
				Items[0].Selected = true;
		}

		#endregion

		#region Loading/Restoring Settings and Saving changes to database
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Restores the list's column widths the query file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void LoadSettings(string parentFormName)
		{
			foreach (ColumnHeader hdr in Columns)
			{
				hdr.Width = PaApp.SettingsHandler.GetIntSettingsValue(
					parentFormName, "col" + hdr.Index, hdr.Width);
			}

			m_sortColumn = PaApp.SettingsHandler.GetIntSettingsValue(
				parentFormName, "sortedcolumn", -1);

			try
			{
				string sortOrder = PaApp.SettingsHandler.GetStringSettingsValue(
					parentFormName, "sortorder", "None");

				m_sortOrder = (SortOrder)Enum.Parse(typeof(SortOrder), sortOrder, true);
			}
			catch
			{
				m_sortOrder = SortOrder.None;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves the list's column widths the query file.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SaveSettings(string parentFormName)
		{
			foreach (ColumnHeader hdr in Columns)
				PaApp.SettingsHandler.SaveSettingsValue(parentFormName, "col" + hdr.Index, hdr.Width);

			PaApp.SettingsHandler.SaveSettingsValue(parentFormName, "sortedcolumn", m_sortColumn);
			PaApp.SettingsHandler.SaveSettingsValue(parentFormName, "sortorder", m_sortOrder);
		}

		#endregion

		#region Methods for saving changes to classes
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Saves to the database any changes made to the lists items.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void SaveChanges()
		{
			m_deletedClass = false;
			PaApp.Project.SearchClasses.Clear();

			foreach (ClassListViewItem item in Items)
				PaApp.Project.SearchClasses.Add(item.SearchClass);

			PaApp.Project.SearchClasses.Save();		
		}		

		#endregion

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnBeforeLabelEdit(LabelEditEventArgs e)
		{
			ClassListViewItem item = Items[e.Item] as ClassListViewItem;
			if (item != null)
				item.InEditMode = true;

			base.OnBeforeLabelEdit(e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Make sure to dirty the item after its class name changes.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnAfterLabelEdit(LabelEditEventArgs e)
		{
			base.OnAfterLabelEdit(e);

			ClassListViewItem item = Items[e.Item] as ClassListViewItem;

			if (item != null)
			{
				if (DoesClassNameExist(e.Label, item, true))
					e.CancelEdit = true;
				else
					item.IsDirty = true;

				item.InEditMode = false;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Removes the specified item from the list and keeps track of its Id so the class
		/// will be deleted from the database when the SaveChanges method is called.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void DeleteItem(ClassListViewItem item)
		{
			int prevIndex = item.Index;
			m_deletedClass = true;
			Items.Remove(item);

			while (prevIndex >= Items.Count)
				prevIndex--;

			if (Items.Count > 0)
				Items[prevIndex].Selected = true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the Members and class type columns
		/// should be shown.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool ShowMembersAndTypeColumns
		{
			get { return m_showMembersAndClassTypeColumns; }
			set
			{
				if (PhoneticFont != null && PhoneticFont != FontHelper.PhoneticFont)
				{
					PhoneticFont.Dispose();
					PhoneticFont = null;
				}
				
				m_showMembersAndClassTypeColumns = value;
				AddColumns();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not to show all the classes (i.e. for
		/// the find phone window) or not (i.e. for the class dialog).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ListApplicationType AppliesTo
		{
			get { return m_appliesTo; }
			set
			{
				m_appliesTo = value;
				Load();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the height of the columnn header portion of the listview.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int ColumnHeaderHeight
		{
			get
			{
				IntPtr hwnd = new IntPtr(STUtils.FindWindowEx(Handle, 0, "SysHeader32", null));
				STUtils.RECT rc;
				return (STUtils.GetWindowRect(hwnd, out rc) ? rc.bottom - rc.top + 1 : 0);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not any of the items in the list are dirty.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsDirty
		{
			get
			{
				if (m_deletedClass)
					return true;

				foreach (ClassListViewItem item in Items)
				{
					if (item.IsDirty)
						return true;
				}

				return false;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Checks if the specified class name already exists.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool DoesClassNameExist(string className, ClassListViewItem origClassInfo, bool showMsg)
		{
			// Ensure the new class doesn't have a duplicate class name
			foreach (ClassListViewItem item in Items)
			{
				if (item.Text == className && item != origClassInfo)
				{
					STUtils.STMsgBox(string.Format(Properties.Resources.kstidDefineClassDupClassName,
						className), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return true;
				}
			}

			return false;
		}

		#region Drawing methods
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Need to draw IPA character class members with the phonetic font.
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnDrawItem(DrawListViewItemEventArgs e)
		{
			e.DrawDefault = !m_showMembersAndClassTypeColumns;
			base.OnDrawItem(e);
		
			if (!e.DrawDefault)
			{
				ClassListViewItem item = e.Item as ClassListViewItem;
				e.DrawDefault = (item == null || !item.Draw(e));
			}
		}
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Must have this when owner draw is turned on, even if this item isn't custom drawn.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
		{
			if (m_sortColumn != e.ColumnIndex)
			{
				e.DrawDefault = true;
				base.OnDrawColumnHeader(e);
				return;
			}

			e.DrawDefault = false;
			e.DrawBackground();
			e.DrawText();

			// Draw the arrow glyph, indicating the sort direction. The
			// arrow will be drawn from its wider end to the point.
			int inc = 1;
			int start = e.Bounds.Top + ((e.Bounds.Height - 5) / 2) - 1;
			if (m_sortOrder == SortOrder.Ascending)
			{
				// Draw fat end at the bottom and move up.
				start += 4;
				inc = -1;
			}

			Point pt1 = new Point(e.Bounds.Right - 20, start);
			Point pt2 = new Point(pt1.X + 8, start);

			// Draw four lines, each successive line being two pixels shorter
			// than the previous and one line up or down from the previous.
			for (int i = 0; i < 4; i++)
			{
				e.Graphics.DrawLine(SystemPens.ControlDark, pt1, pt2);
				pt1.X++;
				pt2.X--;
				pt1.Y += inc;
				pt2.Y += inc;
			}

			// Draw the point. Since a line cannot be 1 pixel long, draw a vertical
			// line that starts at the pixel that is the point and ends one pixel
			// into the body of the arrow.
			pt2.Y -= inc;
			e.Graphics.DrawLine(SystemPens.ControlDark, pt1, pt2);
		}

		#endregion

		#region Sorting methods and comparer class
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Allow sorting on the class name or the class type.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnColumnClick(ColumnClickEventArgs e)
		{
			base.OnColumnClick(e);

			if (e.Column != 1)
			{
				m_sortColumn = e.Column;
				m_sortOrder = (m_sortOrder == SortOrder.Ascending ?
					SortOrder.Descending : SortOrder.Ascending);

				ListViewItem item = (SelectedItems.Count > 0 ? SelectedItems[0] : null);
				ListViewItemSorter = new ClassListComparer(m_sortOrder, m_sortColumn);
				ListViewItemSorter = null;
				Invalidate(new Rectangle(0, 0, Width, ColumnHeaderHeight), true);
				if (item != null)
					item.Focused = true;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// 
		/// </summary>
		/// ------------------------------------------------------------------------------------
		class ClassListComparer : System.Collections.IComparer
		{
			private readonly SortOrder m_sortOrder;
			private readonly int m_sortColumn;

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// 
			/// </summary>
			/// --------------------------------------------------------------------------------
			internal ClassListComparer(SortOrder order, int sortColumn)
			{
				m_sortOrder = order;
				m_sortColumn = sortColumn;
			}

			/// --------------------------------------------------------------------------------
			/// <summary>
			/// 
			/// </summary>
			/// --------------------------------------------------------------------------------
			public int Compare(object ox, object oy)
			{
				ListViewItem x = ox as ListViewItem;
				ListViewItem y = oy as ListViewItem;

				int result = 0;

				if (m_sortColumn != 0)
					result = x.SubItems[2].Text.CompareTo(y.SubItems[2].Text);
				
				if (result == 0)
					result = x.Text.CompareTo(y.Text);

				return (result != 0 && m_sortOrder == SortOrder.Descending ? -result : result);
			}
		}

		#endregion
	}
}
