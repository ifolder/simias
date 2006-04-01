/***********************************************************************
 *  $RCSfile: FileTypeFilter.ascx.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Mike Lasky (mlasky@novell.com)
 * 
 ***********************************************************************/
namespace Novell.iFolderWeb.Admin
{
	using System;
	using System.Collections;
	using System.Data;
	using System.Drawing;
	using System.Resources;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	/// <summary>
	///		Summary description for FileTypeFilter.
	/// </summary>
	public class FileTypeFilter : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// Non system file type policy controls.
		/// </summary>
		protected DataGrid FileTypeList;

		/// <summary>
		/// Table footer control.
		/// </summary>
		protected PageFooter FileTypeListFooter;


		/// <summary>
		/// Button control.
		/// </summary>
		protected Button DeleteButton;

		/// <summary>
		/// Button control.
		/// </summary>
		protected Button AddButton;

		/// <summary>
		/// Button control.
		/// </summary>
		protected Button AllowButton;

		/// <summary>
		/// Button control.
		/// </summary>
		protected Button DenyButton;


		/// <summary>
		/// Control that allows a new file type to be added.
		/// </summary>
		protected TextBox NewFileTypeName;


		/// <summary>
		/// Controls that selects file type entries.
		/// </summary>
		protected CheckBox AllFilesCheckBox;


		/// <summary>
		/// Event that notifies consumer that the filter list has changed.
		/// </summary>
		public event EventHandler ListChanged = null;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the current file offset.
		/// </summary>
		private int CurrentFileOffset
		{
			get { return ( int )ViewState[ "CurrentFileOffset" ]; }
			set { ViewState[ "CurrentFileOffset" ] = value; }
		}

		/// <summary>
		/// Gets or sets the file type data source value.
		/// </summary>
		private Hashtable FileTypeSource
		{
			get { return ViewState[ "FileTypeSource" ] as Hashtable; }
			set { ViewState[ "FileTypeSource" ] = value; }
		}

		/// <summary>
		/// Returns true if any FileType source entries are checked.
		/// </summary>
		private bool HasCheckedEntries
		{
			get
			{
				bool isChecked = false;
				foreach( FileTypeInfo fti in FileTypeSource.Values )
				{
					if ( fti.IsChecked )
					{
						isChecked = true;
						break;
					}
				}

				return isChecked;
			}
		}

		/// <summary>
		/// Returns true if any FileType source entries are disabled.
		/// </summary>
		private bool HasDisabledEntries
		{
			get
			{
				bool isDisabled = false;
				foreach( FileTypeInfo fti in FileTypeSource.Values )
				{
					if ( fti.IsChecked && !fti.IsEnabled )
					{
						isDisabled = true;
						break;
					}
				}

				return isDisabled;
			}
		}

		/// <summary>
		/// Returns true if any FileType source entries are enabled.
		/// </summary>
		private bool HasEnabledEntries
		{
			get
			{
				bool isEnabled = false;
				foreach( FileTypeInfo fti in FileTypeSource.Values )
				{
					if ( fti.IsChecked && fti.IsEnabled )
					{
						isEnabled = true;
						break;
					}
				}

				return isEnabled;
			}
		}

		/// <summary>
		/// Gets or sets the total number of files contained in
		/// the exclude list.
		/// </summary>
		private int TotalFiles
		{
			get { return ( int )ViewState[ "TotalFiles" ]; }
			set { ViewState[ "TotalFiles" ] = value; }
		}

		#endregion

		#region Private Methods
        
		/// <summary>
		/// Creates the file type list view for the web control.
		/// </summary>
		private DataView CreateFileTypeListView()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "FileRegExField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "EnabledField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FileNameField", typeof( string ) ) );

			// Fill the data table from the saved selected member list.
			Hashtable ht = FileTypeSource;
			FileTypeInfo[] ftInfoList = new FileTypeInfo[ ht.Count ];
			TotalFiles = ht.Count;

			// Copy the Values to the array so that they can be sorted.
			ht.Values.CopyTo( ftInfoList, 0 );
			Array.Sort( ftInfoList );

			for ( int i = 0; i < ftInfoList.Length; ++i )
			{
				// Don't add until at the right display offset.
				if ( i >= CurrentFileOffset )
				{
					// Don't add more than one page worth of data.
					if ( i < ( CurrentFileOffset + FileTypeList.PageSize ) )
					{
						dr = dt.NewRow();
						dr[ 0 ] = true;
						dr[ 1 ] = ftInfoList[ i ].RegExFileName;
						dr[ 2 ] = ftInfoList[ i ].IsPending ? GetString( "PENDING" ) : GetString( ftInfoList[ i ].IsEnabled ? "DENY" : "ALLOW" );
						dr[ 3 ] = ftInfoList[ i ].FriendlyFileName;

						dt.Rows.Add( dr );
					}
					else
					{
						break;
					}
				}
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < FileTypeList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = String.Empty;
				dr[ 3 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// Creates a stateful list of file type filters.
		/// </summary>
		/// <param name="policy">User policy object</param>
		/// <returns>A hashtable containing the file type filters.</returns>
		private Hashtable CreateFileTypeSource( UserPolicy policy )
		{
			// Keep the state in a hashtable.
			Hashtable ht = new Hashtable();
			foreach( string s in policy.FileTypesExcludesEffective )
			{
				ht[ s ] = new FileTypeInfo( s, Utils.ConvertFromRegEx( s ), IsFilterEnabled( policy, s ), false );
			}

			return ht;
		}

		/// <summary>
		/// Creates a stateful list of file type filters.
		/// </summary>
		/// <param name="policy">iFolder policy object</param>
		/// <returns>A hashtable containing the file type filters.</returns>
		private Hashtable CreateFileTypeSource( iFolderPolicy policy )
		{
			// Keep the state in a hashtable.
			Hashtable ht = new Hashtable();
			foreach( string s in policy.FileTypesExcludes )
			{
				ht[ s ] = new FileTypeInfo( s, Utils.ConvertFromRegEx( s ), IsFilterEnabled( policy, s ), false );
			}

			return ht;
		}

		/// <summary>
		/// Creates a stateful list of file type filters.
		/// </summary>
		/// <param name="policy">System policy object</param>
		/// <returns>A hashtable containing the file type filters.</returns>
		private Hashtable CreateFileTypeSource( SystemPolicy policy )
		{
			// Keep the state in a hashtable.
			Hashtable ht = new Hashtable();
			foreach( string s in policy.FileTypesExcludes )
			{
				ht[ s ] = new FileTypeInfo( s, Utils.ConvertFromRegEx( s ), true, false );
			}

			return ht;
		}

		/// <summary>
		/// Gets whether the specified file type is in enabled in the exception list.
		/// </summary>
		/// <param name="policy">User policy</param>
		/// <param name="fileType">Name of file type.</param>
		/// <returns></returns>
		private bool IsFilterEnabled( UserPolicy policy, string fileType )
		{
			return ( Array.IndexOf( policy.FileTypesIncludes, fileType ) == -1 ) ? true : false;
		}

		/// <summary>
		/// Gets whether the specified file type is in enabled in the exception list.
		/// </summary>
		/// <param name="policy">iFolder policy</param>
		/// <param name="fileType">Name of file type.</param>
		/// <returns></returns>
		private bool IsFilterEnabled( iFolderPolicy policy, string fileType )
		{
			return ( Array.IndexOf( policy.FileTypesIncludes, fileType ) == -1 ) ? true : false;
		}

		/// <summary>
		/// Event handler for when this page is loaded.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				DeleteButton.Text = GetString( "DELETE" );
				AddButton.Text = GetString( "ADD" );
				DenyButton.Text = GetString( "DENY" );
				AllowButton.Text = GetString( "ALLOW" );

				// Initialize the state variables.
				CurrentFileOffset = 0;
				TotalFiles = 0;
			}
		}

		/// <summary>
		/// Sets the page button state of the file type list.
		/// </summary>
		private void SetPageButtonState()
		{
			FileTypeListFooter.SetPageButtonState( 
				FileTypeList, 
				CurrentFileOffset, 
				TotalFiles, 
				GetString( "FILES" ),
				GetString( "FILE" ) );
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key">Key to the localized string.</param>
		/// <returns>Localized string.</returns>
		protected string GetString( string key )
		{
			return rm.GetString( key );
		}

		/// <summary>
		/// Returns whether the entry has been checked.
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		protected bool IsEntryChecked( Object entry )
		{
			FileTypeInfo fti = FileTypeSource[ entry ] as FileTypeInfo;
			return ( fti != null ) ? fti.IsChecked : false;
		}

		/// <summary>
		/// Event handler that gets called when the all files checkbox is checked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnAllFilesChecked( Object sender, EventArgs e )
		{
			CheckBox allCheckBox = sender as CheckBox;

			foreach( DataGridItem item in FileTypeList.Items )
			{
				string fileName = item.Cells[ 0 ].Text;
				if ( fileName != "&nbsp;" )
				{
					FileTypeInfo fti = FileTypeSource[ fileName ] as FileTypeInfo;
					if ( fti != null )
					{
						CheckBox checkBox = item.Cells[ 1 ].FindControl( "FileTypeCheckBox" ) as CheckBox;
						if ( checkBox != null )
						{
							fti.IsChecked = checkBox.Checked = allCheckBox.Checked;
						}
					}
				}
			}

			// See if there are any checked members.
			bool hasEntries = allCheckBox.Checked ? true : HasCheckedEntries;
			if ( DeleteButton.Visible )
			{
				DeleteButton.Enabled = hasEntries;
			}
			else
			{
				AllowButton.Enabled = HasEnabledEntries && hasEntries;
				DenyButton.Enabled = HasDisabledEntries && hasEntries;
			}
		}

		/// <summary>
		/// Event handler that gets called when the delete button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDeleteFileType( Object sender, EventArgs e )
		{
			// Get all of the values from the hashtable.
			Hashtable ht = FileTypeSource;
			FileTypeInfo[] ftInfoList = new FileTypeInfo[ ht.Count ];
			ht.Values.CopyTo( ftInfoList, 0 );

			foreach( FileTypeInfo fti in ftInfoList )
			{
				if ( fti.IsChecked )
				{
					ht.Remove( fti.RegExFileName );
				}
			}

			// Reset the all files check box.
			AllFilesCheckBox.Checked = false;
			DeleteButton.Enabled = false;

			// If there are no entries in the current view, set the current page back one page.
			if ( CurrentFileOffset >= ht.Count )
			{
				CurrentFileOffset -= FileTypeList.PageSize;
				if ( CurrentFileOffset < 0 )
				{
					CurrentFileOffset = 0;
				}
			}

			// Refresh the policy view.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();
			SetPageButtonState();

			// Indicate an event that the list has changed.
			if ( ListChanged != null )
			{
				ListChanged( this, e );
			}
		}

		/// <summary>
		/// Event handler that gets called when the allow file type button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnAllowFileType( Object sender, EventArgs e )
		{
			// Get all of the values from the hashtable.
			foreach( FileTypeInfo fti in FileTypeSource.Values )
			{
				if ( fti.IsChecked )
				{
					fti.IsEnabled = fti.IsChecked = false;
				}
			}

			// Reset the all files check box.
			AllFilesCheckBox.Checked = false;
			AllowButton.Enabled = false;

			// Refresh the policy view.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();

			// Indicate an event that the list has changed.
			if ( ListChanged != null )
			{
				ListChanged( this, e );
			}
		}

		/// <summary>
		/// Event handler that gets called when the deny file type button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDenyFileType( Object sender, EventArgs e )
		{
			// Get all of the values from the hashtable.
			foreach( FileTypeInfo fti in FileTypeSource.Values )
			{
				if ( fti.IsChecked )
				{
					fti.IsEnabled = true;
					fti.IsChecked = false;
				}
			}

			// Reset the all files check box.
			AllFilesCheckBox.Checked = false;
			DenyButton.Enabled = false;

			// Refresh the policy view.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();

			// Indicate an event that the list has changed.
			if ( ListChanged != null )
			{
				ListChanged( this, e );
			}
		}

		/// <summary>
		/// Event handler that gets called when the add button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFileTypeAddClick( Object sender, EventArgs e )
		{
			string fileName = NewFileTypeName.Text;
			if ( ( fileName != null ) && ( fileName != String.Empty ) )
			{
				Hashtable ht = FileTypeSource;
				ht[ fileName ] = new FileTypeInfo( Utils.ConvertToRegEx( fileName ), fileName, true, true );

				// A new file was added to the list. Update the page buttons.
				++TotalFiles;

				// Clear out the old entry.
				NewFileTypeName.Text = String.Empty;

				// Indicate an event that the list has changed.
				if ( ListChanged != null )
				{
					ListChanged( this, e );
				}
			}
			
			// Refresh the policy view.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Event handler that gets called when the file type checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFileTypeCheckChanged( Object sender, EventArgs e )
		{
			CheckBox checkBox = sender as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string fileName = item.Cells[ 0 ].Text;
			if ( fileName != "&nbsp;" )
			{
				FileTypeInfo fti = FileTypeSource[ fileName ] as FileTypeInfo;
				if ( fti != null )
				{
					fti.IsChecked = checkBox.Checked;

					bool hasEntries = checkBox.Checked ? true : HasCheckedEntries;
					if ( DeleteButton.Visible )
					{
						DeleteButton.Enabled = hasEntries;
					}
					else
					{
						AllowButton.Enabled = HasEnabledEntries && hasEntries;
						DenyButton.Enabled = HasDisabledEntries && hasEntries;
					}
				}
			}
		}

		/// <summary>
		/// Event handler for the PageFirstButton.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageFirstButton_Click( object source, ImageClickEventArgs e )
		{
			// Set to get the first files.
			CurrentFileOffset = 0;

			// Rebind the data source with the new data.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();

			// Set the button state.
			SetPageButtonState();
		
			// Reset the all files checkbox.
			AllFilesCheckBox.Checked = false;
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentFileOffset += FileTypeList.PageSize;

			// Rebind the data source with the new data.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();

			// Set the button state.
			SetPageButtonState();

			// Reset the all files checkbox.
			AllFilesCheckBox.Checked = false;
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentFileOffset =  ( ( TotalFiles - 1 ) / FileTypeList.PageSize ) * FileTypeList.PageSize;

			// Rebind the data source with the new data.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();

			// Set the button state.
			SetPageButtonState();

			// Reset the all files checkbox.
			AllFilesCheckBox.Checked = false;
		}

		/// <summary>
		/// Event that first when the PagePreviousButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PagePreviousButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentFileOffset -= FileTypeList.PageSize;
			if ( CurrentFileOffset < 0 )
			{
				CurrentFileOffset = 0;
			}

			// Rebind the data source with the new data.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();

			// Set the button state.
			SetPageButtonState();

			// Reset the all files checkbox.
			AllFilesCheckBox.Checked = false;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the file type policy for the current user.
		/// </summary>
		/// <param name="policy">User policy.</param>
		public void GetFileTypePolicy( UserPolicy policy )
		{
			// Show the proper control buttons.
			AllowButton.Visible = DenyButton.Visible = true;

			// Create a list from the file type policy.
			FileTypeSource = CreateFileTypeSource( policy );

			// Build the data view from the table.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Gets the file type policy for the current ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy.</param>
		public void GetFileTypePolicy( iFolderPolicy policy )
		{
			// Show the proper control buttons.
			AllowButton.Visible = DenyButton.Visible = true;

			// Create a list from the file type policy.
			FileTypeSource = CreateFileTypeSource( policy );

			// Build the data view from the table.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Gets the file type policy for the system.
		/// </summary>
		/// <param name="policy">System policy.</param>
		public void GetFileTypePolicy( SystemPolicy policy )
		{
			// Enable the add/delete controls.
			NewFileTypeName.Visible = AddButton.Visible = DeleteButton.Visible = true;

			// Create a list from the file type policy.
			FileTypeSource = CreateFileTypeSource( policy );

			// Build the data view from the table.
			FileTypeList.DataSource = CreateFileTypeListView();
			FileTypeList.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Sets the file type policy for this user.
		/// </summary>
		/// <param name="policy">User policy that the new file type filter will be set.</param>
		public void SetFileTypePolicy( UserPolicy policy )
		{
			// Build a list of checked file types.
			ArrayList filterList = new ArrayList();
			foreach( FileTypeInfo fti in FileTypeSource.Values )
			{
				if ( fti.IsEnabled )
				{
					filterList.Add( fti.RegExFileName );
				}
			}

			// Set the user current user policy.
			policy.FileTypesIncludes = filterList.ToArray( typeof( string ) ) as string[];
		}

		/// <summary>
		/// Sets the file type policy for this ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy that the new file type filter will be set.</param>
		public void SetFileTypePolicy( iFolderPolicy policy )
		{
			// Build a list of checked file types.
			ArrayList filterList = new ArrayList();
			foreach( FileTypeInfo fti in FileTypeSource.Values )
			{
				if ( fti.IsEnabled )
				{
					filterList.Add( fti.RegExFileName );
				}
			}

			// Set the user current user policy.
			policy.FileTypesIncludes = filterList.ToArray( typeof( string ) ) as string[];
		}

		/// <summary>
		/// Sets the file type policy for the system.
		/// </summary>
		/// <param name="policy">System policy that the new file type filter will be set.</param>
		public void SetFileTypePolicy( SystemPolicy policy )
		{
			// Build a list of checked file types.
			ArrayList filterList = new ArrayList();
			foreach( FileTypeInfo fti in FileTypeSource.Values )
			{
				if ( fti.IsEnabled )
				{
					filterList.Add( fti.RegExFileName );
				}
			}

			// Set the user current system policy.
			policy.FileTypesExcludes = filterList.ToArray( typeof( string ) ) as string[];
		}

		#endregion

		#region Web Form Designer generated code

		/// <summary>
		/// OnInit
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			FileTypeListFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			FileTypeListFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			FileTypeListFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			FileTypeListFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );
			
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		#region FileTypeInfo Class

		/// <summary>
		/// Class used to hold File Type filter information.
		/// </summary>
		[ Serializable() ]
			private class FileTypeInfo : IComparable
		{
			#region Class Members

			/// <summary>
			/// The regular expression version of the file name.
			/// </summary>
			public string RegExFileName;

			/// <summary>
			/// The friendly version of the file name.
			/// </summary>
			public string FriendlyFileName;

			/// <summary>
			/// If the file name is enabled as a filter.
			/// </summary>
			public bool IsEnabled;

			/// <summary>
			/// True if entry has not been committed yet.
			/// </summary>
			public bool IsPending;

			/// <summary>
			/// True if entry has been checked in the list.
			/// </summary>
			public bool IsChecked;

			#endregion

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="regExFileName"></param>
			/// <param name="friendlyFileName"></param>
			/// <param name="enabled"></param>
			/// <param name="pending"></param>
			public FileTypeInfo( string regExFileName, string friendlyFileName, bool enabled, bool pending )
			{
				RegExFileName = regExFileName;
				FriendlyFileName = friendlyFileName;
				IsEnabled = enabled;
				IsPending = pending;
				IsChecked = false;
			}

			#endregion

			#region IComparable Members

			/// <summary>
			/// Compares the current instance with another object of the same type.
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public int CompareTo( object obj )
			{
				return String.Compare( FriendlyFileName, ( obj as FileTypeInfo ).FriendlyFileName, false );
			}

			#endregion
		}

		#endregion
	}
}
