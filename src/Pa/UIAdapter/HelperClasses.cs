// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2005-2015, SIL International.
// <copyright from='2005' to='2015' company='SIL International'>
//		Copyright (c) 2005-2015, SIL International.
//    
//		This software is distributed under the MIT License, as specified in the LICENSE.txt file.
// </copyright> 
#endregion
// 
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace SIL.FieldWorks.Common.UIAdapters
{
	/// <summary>Handler for allowing applications to initialize a toolbar.</summary>
	public delegate void InitializeBarHandler(ref TMBarProperties barProps);

	/// <summary>Handler for allowing applications to initialize a toolbar item.</summary>
	public delegate void InitializeItemHandler(ref TMItemProperties itemProps);

	/// <summary>Handler for allowing applications a chance to set the contents of a combobox item.</summary>
	public delegate void InitializeComboItemHandler(string name, ComboBox cboItem);
	
	/// <summary>Handler for allowing applications to respond to a toolbar's request for a
	/// control container control.</summary>
	public delegate Control LoadControlContainerItemHandler(string name);

	/// <summary>Handler for requesting toolbar information from an application so a menu
	/// adapter can provide menu items to toggle the visibility of toolbars.</summary>
	public delegate TMItemProperties[] GetBarInfoForViewMenuHandler();

	/// <summary>Handler for allowing applications to respond
	/// to a recently used file being chosen.</summary>
	public delegate void RecentlyUsedItemChosenHandler(string filename);

	/// <summary>Handler for allowing applications to localize a toolbar/menu item.</summary>
	public delegate void LocalizeItemHandler(object item, string id, TMItemProperties itemProps);

	#region TMItemProperties Class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class is used to pass information back and forth between a ToolBarAdapter object
	/// and toolbar item command handlers for click and update.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class TMItemProperties
	{
		//private string m_statusMsg = string.Empty;
		//private string m_shortcut = "None";
		//private bool m_isImageAppSpecific = false;
		//private int m_imageIndex = -1;

		public TMItemProperties()
		{
			Name = string.Empty;
			Text = string.Empty;
			OriginalText = string.Empty;
			Category = string.Empty;
			Tooltip = string.Empty;
			CommandId = string.Empty;
			Message = string.Empty;
			Enabled = true;
			Checked = false;
			Visible = true;
			BeginGroup = false;
			ShortcutKey = Keys.None;
			Image = null;
			Control = null;
			List = null;
			ParentControl = null;
			Update = false;
			Tag = null;
			Size = Size.Empty;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This setter has no affect except in the adapter code.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ITMAdapter Adapter { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the name for a menu/toolbar item. Setting this value should not change
		/// an item's name within the XML definition. It is only used to be able to pass
		/// an item's name to instantiators of a toolbar adapter.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Name { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the text for a menu/toolbar item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Text { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the value of the item's text as it came from the resources. Setting this
		/// value in your appication has no effect. It is ignored.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string OriginalText { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the category (e.g. "Bold" button's category would be "Format") for a
		/// menu/toolbar item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Category { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the tooltip for a menu/toolbar item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Tooltip { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the command Id for a menu/toolbar item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string CommandId { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the command id for a menu/toolbar item. Setting this value in your
		/// application has no effect. It is ignored.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Message { get; set; }

//		/// ------------------------------------------------------------------------------------
//		/// <summary>
//		/// Gets or sets the status message for a menu/toolbar item.
//		/// </summary>
//		/// ------------------------------------------------------------------------------------
//		public string StatusMessage
//		{
//			get {return m_statusMsg;}
//			set {m_statusMsg = value;}
//		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the enabled state for a menu/toolbar item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool Enabled { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the checked state for a menu/toolbar item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool Checked { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the visible state for a menu/toolbar item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool Visible { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the item begins a group (for menu
		/// items, this would be a separator before this item. For toolbar items it would
		/// be a dividing line.)
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool BeginGroup { get; set; }

//		/// ------------------------------------------------------------------------------------
//		/// <summary>
//		/// Gets or sets the image index for a menu/toolbar item.
//		/// </summary>
//		/// ------------------------------------------------------------------------------------
//		public int ImageIndex
//		{
//			get {return m_imageIndex;}
//			set {m_imageIndex = value;}
//		}
	
//		/// ------------------------------------------------------------------------------------
//		/// <summary>
//		/// Gets or sets a value indicating whether or not the menu/toolbar image is application
//		/// specific or is defined in the FW framework. e.g. Undo, Redo, Cut, Copy, Paste, etc.
//		/// are defined in the framework whereas the image for Insert verse number would be
//		/// an application specific image.
//		/// </summary>
//		/// ------------------------------------------------------------------------------------
//		public bool IsImageAppSpecific
//		{
//			get {return m_isImageAppSpecific;}
//			set {m_isImageAppSpecific = value;}
//		}

//		/// ------------------------------------------------------------------------------------
//		/// <summary>
//		/// Gets or sets the shortcut key for a menu/toolbar item.
//		/// </summary>
//		/// ------------------------------------------------------------------------------------
//		public string Shortcut
//		{
//			get {return m_shortcut;}
//			set {m_shortcut = value;}
//		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the shortcut key for a menu/toolbar item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Keys ShortcutKey { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the image for a menu/toolbar item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Image Image { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the control hosted by toolbar items whose type is ComboBox or control container.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Control Control { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a list of items associated with a toolbar item. For example, if the
		/// toolbar item is a combobox, this list may contain all the item's in the combobox.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ArrayList List { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the form on which the menu/toolbar item is located. Setting this
		/// property does not set the parent form of an item. The setter is only for toolbar
		/// adapters to use.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Control ParentControl { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the menu/toolbar item will be updated
		/// with the properties specified in the <see cref="TMItemProperties"/> after
		/// control leaves the command handler and is returned to the toolbar adapter.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool Update { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets any extra information needed to be passed between the application and
		/// an adapter.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public object Tag { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the item's size
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Size Size { get; set; }
	}

	#endregion

	#region TMBarProperties Class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class is used to pass information back and forth between a ToolBarAdapter object
	/// and an application.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class TMBarProperties
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a TMBarProperties object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TMBarProperties()
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates a TMBarProperties object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TMBarProperties(string name, string text, bool enabled, bool visible,
			Control parentCtrl, ITMAdapter adapter)
		{
			Name = name;
			Text = text;
			Enabled = enabled;
			Visible = visible;
			ParentControl = parentCtrl;
			Adapter = adapter;
			Update = false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the name for a toolbar. Setting this value should not change
		/// a bar's name within the XML definition. It is only used to pass a bar's name to
		/// instantiators of a toolbar adapter.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Name { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the display text for a toolbar.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Text { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the enabled state for a toolbar.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool Enabled { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the visible state for a toolbar.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool Visible { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the control or form on which the toolbar item is located. Setting
		/// this property does not set the parent form of an item. The setter is only for
		/// toolbar adapters to use.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Control ParentControl { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the toolbar will be updated
		/// with the properties specified in the <see cref="TMBarProperties"/> after
		/// control leaves the command handler and is returned to the toolbar adapter.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool Update { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This setter has no affect except in the adapter code.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ITMAdapter Adapter { get; set; }
	}

	#endregion

	#region ToolBarPopupInfo Class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Objects of this class type are used to pass information between a toolbar adapter and
	/// handlers of "DropDown" commands. DropDown commands are issued via the message mediator
	/// when a popup-type toolbar button's popup arrow is clicked.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class ToolBarPopupInfo
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Instantiates a new, uninitialized ToolBarPopupInfo object.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ToolBarPopupInfo()
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Instantiates a new, initialized ToolBarPopupInfo object.
		/// </summary>
		/// <param name="name">Toolbar item's name.</param>
		/// ------------------------------------------------------------------------------------
		public ToolBarPopupInfo(string name)
		{
			Name = name;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This setter has no affect except in the adapter code.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ITMAdapter Adapter { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the name of the toolbar popup item.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string Name { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the control to be popped-up.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Control Control { get; set; }
	}

	#endregion

	#region WindowListInfo Class
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Class used to send information to the update command handler for the window list
	/// item on a menu.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class WindowListInfo
	{
		/// <summary>The TMItemProperties of the first window list menu item. When the window
		/// list item's update handler is called, the update handler should fill an
		/// ArrayList of strings that will be used by the menu adapter to build each window
		/// list menu item. The array list should contain the desired menu texts (without the
		/// numbers, however). Once the ArrayList is built, it should be assigned to the List
		/// property of WindowListItemProperties.</summary>
		public TMItemProperties WindowListItemProperties;
		/// <summary>Index of the item in the ArrayList that represents the active window.
		/// This tells the menu adapter what item in the window list gets checked.</summary>
		public int CheckedItemIndex;
	}

	#endregion

	#region AdapterHelper
	/// ----------------------------------------------------------------------------------------
	public class AdapterHelper
	{
		/// ------------------------------------------------------------------------------------
		public static ITMAdapter CreateTMAdapter()
		{
			return (RunningTests() ? null : new TMAdapter());
		}
		
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Usually <c>false</c>, but can be set to <c>true</c> while running unit tests
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static bool RunningTests()
		{
			// If the real application is ever installed in a path that includes nunit, then
			// this will return true and the app. won't run properly. But what are the chances
			// of that?...
			return (Application.ExecutablePath.ToLower().IndexOf("nunit") != -1);
		}
	}

	#endregion
}
