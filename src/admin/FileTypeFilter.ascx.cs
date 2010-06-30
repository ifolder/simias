/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Mike Lasky (mlasky@novell.com)
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

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
		/// File type filter control.
		/// </summary>
		protected Label Title;


		/// <summary>
		/// Non system file type policy controls.
		/// </summary>
		protected DataGrid FileTypeList;

		/// <summary>
		/// Table footer control.
		/// </summary>
		protected ListFooter FileTypeListFooter;


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
		/// Returns true if any FileType source entries are disallowed.
		/// </summary>
		private bool HasDisallowedEntries
		{
			get
			{
				bool isDisallowed = false;
				foreach( FileTypeInfo fti in FileTypeSource.Values )
				{
					if ( fti.IsChecked && !fti.IsAllowed )
					{
						isDisallowed = true;
						break;
					}
				}

				return isDisallowed;
			}
		}

		/// <summary>
		/// Returns true if any FileType source entries are allowed.
		/// </summary>
		private bool HasAllowedEntries
		{
			get
			{
				bool isAllowed = false;
				foreach( FileTypeInfo fti in FileTypeSource.Values )
				{
					if ( fti.IsChecked && fti.IsAllowed )
					{
						isAllowed = true;
						break;
					}
				}

				return isAllowed;
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
			dt.Columns.Add( new DataColumn( "AllowedField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FileNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "EnabledField", typeof( bool ) ) );

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
						dr[ 2 ] = GetString( ftInfoList[ i ].IsAllowed ? "ALLOW" : "DENY" );
						dr[ 3 ] = ftInfoList[ i ].FriendlyFileName;
						dr[ 4 ] = ftInfoList[ i ].IsEnabled;

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
				dr[ 4 ] = false;

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
			UserGroupAdminRights uRights = new UserGroupAdminRights(policy.AdminGroupRights);
			Hashtable ht = new Hashtable();
			foreach( string s in policy.FileTypesExcludesEffective )
			{
				ht[ s ] = new FileTypeInfo( 
					s, 
					Utils.ConvertFromRegEx( s ), 
					IsAllowed( policy.FileTypesIncludes, s ), 
					uRights.AddToExcludePolicyAllowed);
			}
			foreach( string s in policy.FileTypesIncludes )
			{
				ht[ s ] = new FileTypeInfo( 
					s, 
					Utils.ConvertFromRegEx( s ), 
					true, 
					uRights.AddToExcludePolicyAllowed);
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
			UserGroupAdminRights uRights = new UserGroupAdminRights(policy.AdminGroupRights);
			foreach( string s in policy.FileTypesExcludesEffective )
			{
				ht[ s ] = new FileTypeInfo( 
					s, 
					Utils.ConvertFromRegEx( s ), 
					IsAllowed( policy.FileTypesIncludesEffective, s ), 
					false );
			}

			foreach( string s in policy.FileTypesExcludes )
			{
				ht[ s ] = new FileTypeInfo( 
					s, 
					Utils.ConvertFromRegEx( s ), 
					false, 
					uRights.AddToExcludePolicyAllowed);
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
				ht[ s ] = new FileTypeInfo( s, Utils.ConvertFromRegEx( s ), false, true );
			}

			return ht;
		}

		/// <summary>
		/// Checks if the specified file name is already in the list.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>True if the specified file name is already in the list.</returns>
		private bool FileTypeExists( string fileName )
		{
			bool exists = false;

			Hashtable ht = FileTypeSource;
			foreach( FileTypeInfo fti in ht.Values )
			{
				if ( String.Compare( fti.FriendlyFileName, fileName, true ) == 0 )
				{
					exists = true;
					break;
				}
			}

			return exists;
		}

		/// <summary>
		/// Gets whether the specified file type is allowed.
		/// </summary>
		/// <param name="effectivePolicy">Effective user policy</param>
		/// <param name="fileType">Name of file type.</param>
		/// <returns></returns>
		private bool IsAllowed( string[] effectivePolicy, string fileType )
		{
			return ( Array.IndexOf( effectivePolicy, fileType ) != -1 ) ? true : false;
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

				Title.Text = GetString( "EXCLUDEDFILES" );

				// Initialize the state variables.
				CurrentFileOffset = 0;
				TotalFiles = 0;

				// Set the javascript function that will handle key presses.
				NewFileTypeName.Attributes[ "OnKeyPress" ] = "return SubmitKeyDown(event, '" + AddButton.ClientID + "');";
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

			if (TotalFiles == 0)
			        FileTypeListFooter.SetPageText (GetString ("NOPOLICIESAVAILABLE"));
		}

		/// <summary>
		/// Displays an error message on the parent page.
		/// </summary>
		/// <param name="errMsg"></param>
		private void ShowError( string errMsg )
		{
			TopNavigation nav = Page.FindControl( "TopNav" ) as TopNavigation;
			if ( nav != null )
			{
				nav.ShowError( errMsg );
			}
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
						if ( ( checkBox != null ) && checkBox.Enabled )
						{
							fti.IsChecked = checkBox.Checked = allCheckBox.Checked;
						}
					}
				}
			}

			// See if there are any checked members.
			bool hasEntries = allCheckBox.Checked ? true : HasCheckedEntries;
			DeleteButton.Enabled = hasEntries;
			AllowButton.Enabled = HasDisallowedEntries && hasEntries;
			DenyButton.Enabled = HasAllowedEntries && hasEntries;
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
					fti.IsAllowed = true;
					fti.IsChecked = false;
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
					fti.IsAllowed = fti.IsChecked = false;
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
				// Make sure that this entry is not already in the list.
				if ( !FileTypeExists( fileName ) )
				{
					Hashtable ht = FileTypeSource;
					ht[ fileName ] = new FileTypeInfo( Utils.ConvertToRegEx( fileName ), fileName, false, true );

					// A new file was added to the list. Update the page buttons.
					++TotalFiles;

					// Clear out the old entry.
					NewFileTypeName.Text = String.Empty;

					// Indicate an event that the list has changed.
					if ( ListChanged != null )
					{
						ListChanged( this, e );
					}

					// Refresh the policy view.
					FileTypeList.DataSource = CreateFileTypeListView();
					FileTypeList.DataBind();
					SetPageButtonState();
				}
				else
				{
					ShowError( GetString( "FILETYPEALREADYEXISTS" ) );
				}
			}
			else
			{
				AddButton.Enabled = false;
			}
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
					DeleteButton.Enabled = hasEntries;
					AllowButton.Enabled = HasDisallowedEntries && hasEntries;
					DenyButton.Enabled = HasAllowedEntries && hasEntries;
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
		        // Show new file type controls
			NewFileTypeName.Visible = AddButton.Visible = true;

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
		        // Show new file type controls
			NewFileTypeName.Visible = AddButton.Visible = true;

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
		        // Show new file type controls
			NewFileTypeName.Visible = AddButton.Visible = true;

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
			ArrayList filterListAllow = new ArrayList();
			ArrayList filterListDeny = new ArrayList();
			foreach( FileTypeInfo fti in FileTypeSource.Values )
			{
				if ( fti.IsAllowed )
				{
					filterListAllow.Add( fti.RegExFileName );
				}
				else
				{
					filterListDeny.Add( fti.RegExFileName );	
				}
			}

			// Set the user current user policy.
			policy.FileTypesIncludes = filterListAllow.ToArray( typeof( string ) ) as string[];
			policy.FileTypesExcludes = filterListDeny.ToArray( typeof( string ) ) as string[];
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
				if ( fti.IsEnabled && !fti.IsAllowed )
				{
					filterList.Add( fti.RegExFileName );
				}
			}

			// Set the current ifolder policy.
			policy.FileTypesExcludes = filterList.ToArray( typeof( string ) ) as string[];
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
				filterList.Add( fti.RegExFileName );
			}

			// Set the user current system policy.
			policy.FileTypesExcludes = filterList.ToArray( typeof( string ) ) as string[];
		}

		/// <summary>
		/// Sets the checkbox enabled or disabled state
		/// </summary>
		public bool SetCheckBoxEnabledState
		{
			set
			{
				AllowButton.Enabled = value;
				DeleteButton.Enabled = value;
				DenyButton.Enabled = value;
				AllFilesCheckBox.Enabled = value;
				NewFileTypeName.Enabled = value;
			}
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
			public bool IsAllowed;

			/// <summary>
			/// True if entry is a system level policy that can be
			/// enabled/disabled or deleted.
			/// </summary>
			public bool IsEnabled;

			/// <summary>
			/// True if entry has been checked in the list.
			/// </summary>
			public bool IsChecked = false;

			#endregion

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="regExFileName"></param>
			/// <param name="friendlyFileName"></param>
			/// <param name="allowed"></param>
			/// <param name="enabled"></param>
			public FileTypeInfo( 
				string regExFileName, 
				string friendlyFileName, 
				bool allowed, 
				bool enabled )
			{
				RegExFileName = regExFileName;
				FriendlyFileName = friendlyFileName;
				IsAllowed = allowed;
				IsEnabled = enabled;
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
