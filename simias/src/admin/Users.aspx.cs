/***********************************************************************
 *  $RCSfile: Users.aspx.cs,v $
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
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Resources;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for Users.
	/// </summary>
	public class Users : System.Web.UI.Page
	{
		#region Class Members

		/// <summary>
		/// Accounts list data grid column indices.
		/// </summary>
		private const int AccountsIDColumn = 0;
		private const int AccountsDisabledColumn = 1;
		private const int AccountsCheckColumn = 2;
		private const int AccountsTypeColumn = 3;
		private const int AccountsNameColumn = 4;
		private const int AccountsFullNameColumn = 5;
		private const int AccountsStatusColumn = 6;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected DataGrid Accounts;

		/// <summary>
		/// Web Controls.
		/// </summary>
		protected MemberSearch MemberSearch;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected PageFooter AccountsFooter;

		
		/// <summary>
		/// All users check box control.
		/// </summary>
		protected CheckBox AllUsersCheckBox;


		/// <summary>
		/// Delete user button control.
		/// </summary>
		protected Button DeleteButton;

		/// <summary>
		/// Disable user button control.
		/// </summary>
		protected Button DisableButton;

		/// <summary>
		/// Enable user button control.
		/// </summary>
		protected Button EnableButton;
        
		/// <summary>
		/// Create user button.
		/// </summary>
		protected Button CreateButton;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the users that are checked in the list.
		/// </summary>
		private Hashtable CheckedUsers
		{
			get { return ViewState[ "CheckedUsers" ] as Hashtable; }
			set { ViewState[ "CheckedUsers" ] = value; }
		}

		/// <summary>
		/// Gets or sets the current user offset.
		/// </summary>
		private int CurrentUserOffset
		{
			get { return ( int )ViewState[ "CurrentUserOffset" ]; }
			set { ViewState[ "CurrentUserOffset" ] = value; }
		}

		/// <summary>
		/// Gets or sets the super admin ID.
		/// </summary>
		private string SuperAdminID
		{
			get { return ViewState[ "SuperAdminID" ] as string; }
			set { ViewState[ "SuperAdminID" ] = value; }
		}

		/// <summary>
		/// Gets or sets the total number of users contained in
		/// the last search.
		/// </summary>
		private int TotalUsers
		{
			get { return ( int )ViewState[ "TotalUsers" ]; }
			set { ViewState[ "TotalUsers" ] = value; }
		}

		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			TopNav.AddBreadCrumb( GetString( "HOME" ), "SystemInfo.aspx" );
			TopNav.AddBreadCrumb( GetString( "USERS" ), null );
		}

		/// <summary>
		/// Creates a DataSource containing user names from a search.
		/// </summary>
		/// <returns>An DataView object containing the ifolder users.</returns>
		private DataView CreateDataSource()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "DisabledField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "AdminField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "StatusField", typeof( string ) ) );

			iFolderUser[] userList;
			int total;

			if ( MemberSearch.SearchName == String.Empty )
			{
				userList = web.GetUsersBySearch( 
					MemberSearch.SearchAttribute, 
					MemberSearch.SearchOperation, 
					"*", 
					CurrentUserOffset, 
					Accounts.PageSize, 
					out total );
			}
			else
			{
				userList = web.GetUsersBySearch( 
					MemberSearch.SearchAttribute, 
					MemberSearch.SearchOperation, 
					MemberSearch.SearchName, 
					CurrentUserOffset, 
					Accounts.PageSize, 
					out total );
			}

			foreach( iFolderUser user in userList )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = user.ID;
				dr[ 2 ] = !user.Enabled;
				dr[ 3 ] = ( user.Rights == Rights.Admin ) ? true : false;
				dr[ 4 ] = user.UserName;
				dr[ 5 ] = user.FullName;
				dr[ 6 ] = GetString( user.Enabled ? "YES" : "NO" );

				dt.Rows.Add( dr );
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < Accounts.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = false;
				dr[ 3 ] = false;
				dr[ 4 ] = String.Empty;
				dr[ 5 ] = String.Empty;
				dr[ 6 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotalUsers = total;

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// Binds the user list to the web control.
		/// </summary>
		private void GetUsers()
		{
			// Initially populate the member list.
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Returns whether the specified ID is the super admin.
		/// </summary>
		/// <param name="id">User ID.</param>
		/// <returns>True if User ID is super admin, otherwise false is returned.</returns>
		private bool IsSuperAdmin( string id )
		{
			return ( SuperAdminID == id ) ? true : false;
		}

		/// <summary>
		/// Page load event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load( object sender, System.EventArgs e )
		{
			// connection
			web = Session[ "Connection" ] as iFolderAdmin;

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				DisableButton.Text = GetString( "DISABLE" );
				EnableButton.Text = GetString( "ENABLE" );

				// Initialize state variables.
				CurrentUserOffset = 0;
				TotalUsers = 0;
				AllUsersCheckBox.Checked = false;
				CheckedUsers = new Hashtable();

				IdentityPolicy policy = web.GetIdentityPolicy();
				if ( policy.CanCreate )
				{
					CreateButton.Text = GetString( "CREATE" );
					CreateButton.Visible = true;
				}

				if ( policy.CanDelete )
				{
					DeleteButton.Text = GetString( "DELETE" );
					DeleteButton.Visible = true;
				}

				// Get the owner of the system.
				iFolder domain = web.GetiFolder( web.GetSystem().ID );
				SuperAdminID = domain.OwnerID;
			}
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender(object sender, EventArgs e)
		{
			// Build the breadcrumb list.
			BuildBreadCrumbList();

			// Get the user list.
			GetUsers();
		}

		/// <summary>
		/// Enables or disables the ifolder action buttons.
		/// </summary>
		private void SetActionButtons()
		{
			Hashtable ht = CheckedUsers;
			DeleteButton.Enabled = ( ht.Count > 0 ) ? true : false;
			DisableButton.Enabled = ht.ContainsValue( true );
			EnableButton.Enabled = ht.ContainsValue( false );
		}

		/// <summary>
		/// Sets the page button state of the Accounts list.
		/// </summary>
		private void SetPageButtonState()
		{
			AccountsFooter.SetPageButtonState( 
				Accounts, 
				CurrentUserOffset, 
				TotalUsers, 
				GetString( "USERS" ),
				GetString( "USER" ) );
		}

		/// <summary>
		/// Sets the enabled status on all selected users.
		/// </summary>
		/// <param name="status">If true then all selected users will be enabled.</param>
		private void SetSelectedUserStatus( bool status )
		{
			foreach( string userID in CheckedUsers.Keys )
			{
				// Don't set the status if already set.
				if ( ( bool )CheckedUsers[ userID ] != status )
				{
					UserPolicy policy = Utils.GetUserPolicyObject( userID );
					policy.LoginEnabled = status;
					web.SetUserPolicy( policy );
				}
			}

			// Clear the checked members.
			CheckedUsers.Clear();
			AllUsersCheckBox.Checked = false;

			// Set the action buttons.
			SetActionButtons();

			// Rebind the data source with the new data.
			GetUsers();
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Returns the checked state for the specified member.
		/// </summary>
		/// <param name="id">ID of the ifolder</param>
		/// <returns>True if ifolder is checked.</returns>
		protected bool GetMemberCheckedState( Object id )
		{
			return CheckedUsers.ContainsKey( id ) ? true : false;
		}

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
		/// Gets the image representing the user type.
		/// </summary>
		/// <returns></returns>
		protected string GetUserImage( Object isAdmin )
		{
			return ( bool )isAdmin ? "images/ifolder_admin.gif" : "images/ifolder_user.gif";
		}

		/// <summary>
		/// Gets whether the user should be able to be checked.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>True if the user is allowed to be checked.</returns>
		protected bool IsUserEnabled( object id )
		{
			return !IsSuperAdmin( id as string );
		}

		/// <summary>
		/// Event handler that gets called when the all user check box is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAllUsersChecked( object source, EventArgs e )
		{
			CheckBox checkBox = source as CheckBox;
			foreach( DataGridItem item in Accounts.Items )
			{
				// In order to be checked, the row must not be empty.
				string userID = item.Cells[ AccountsIDColumn ].Text;
				if ( userID != "&nbsp;" && !IsSuperAdmin( userID ) )
				{
					if ( checkBox.Checked )
					{
						CheckedUsers[ userID ] = item.Cells[ AccountsDisabledColumn ].Text == Boolean.FalseString;
					}
					else
					{
						// Remove this user from the list.
						CheckedUsers.Remove( userID );
					}
				}
			}

			// Set the action buttons appropriately.
			SetActionButtons();

			// Rebind the data source with the new data.
			GetUsers();
		}

		/// <summary>
		/// Event handler that gets called when the create user button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnCreateButton_Click( object source, EventArgs e )
		{
			Response.Redirect( "CreateUser.aspx" );
		}

		/// <summary>
		/// Event handler that gets called when the delete user button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDeleteButton_Click( object source, EventArgs e )
		{
			foreach( string userID in CheckedUsers.Keys )
			{
				// BUGBUG!! - Need a call to delete by user IDs.
				iFolderUser user = web.GetUser( userID );
				web.DeleteUser( user.UserName );
			}

			// Clear the checked members.
			CheckedUsers.Clear();
			AllUsersCheckBox.Checked = false;

			// Set the action buttons.
			SetActionButtons();

			// Rebind the data source with the new data.
			GetUsers();
		}

		/// <summary>
		/// Event handler that gets called when the disable user button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDisableButton_Click( object source, EventArgs e )
		{
			SetSelectedUserStatus( false );
		}

		/// <summary>
		/// Event handler that gets called when the enable user button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnEnableButton_Click( object source, EventArgs e )
		{
			SetSelectedUserStatus( true );
		}

		/// <summary>
		/// Event handler that gets called when the user check box is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnUserChecked( object source, EventArgs e )
		{
			// Get the data grid row for this member.
			CheckBox checkBox = source as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string userID = item.Cells[ AccountsIDColumn ].Text;
			if ( userID != "&nbsp;" )
			{
				// User is being added.
				if ( checkBox.Checked )
				{
					CheckedUsers[ userID ] = item.Cells[ AccountsDisabledColumn ].Text == Boolean.FalseString;
				}
				else
				{
					// Remove this ifolder from the list.
					CheckedUsers.Remove( userID );
				}
			}

			// Set the user action buttons.
			SetActionButtons();
		}

		/// <summary>
		/// SearchButton_Click
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void SearchButton_Click( object source, EventArgs e)
		{
			// Always search from the beginning.
			CurrentUserOffset = 0;
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();
			AllUsersCheckBox.Checked = false;
			SetPageButtonState();
		}

		/// <summary>
		/// Event that first when the PageFirstButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageFirstButton_Click( object source, ImageClickEventArgs e)
		{
			// Set to get the first users.
			CurrentUserOffset = 0;

			// Rebind the data source with the new data.
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();

			// Set the button state.
			AllUsersCheckBox.Checked = false;
			SetPageButtonState();;
		}

		/// <summary>
		/// Event that first when the PagePreviousButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PagePreviousButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset -= Accounts.PageSize;
			if ( CurrentUserOffset < 0 )
			{
				CurrentUserOffset = 0;
			}

			// Rebind the data source with the new data.
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();

			// Set the button state.
			AllUsersCheckBox.Checked = false;
			SetPageButtonState();;
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset += Accounts.PageSize;

			// Rebind the data source with the new data.
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();

			// Set the button state.
			AllUsersCheckBox.Checked = false;
			SetPageButtonState();;
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset = ( ( TotalUsers - 1 ) / Accounts.PageSize ) * Accounts.PageSize;

			// Rebind the data source with the new data.
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();

			// Set the button state.
			AllUsersCheckBox.Checked = false;
			SetPageButtonState();;
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		/// this.Load += new System.EventHandler( this.Page_Load );
		private void InitializeComponent()
		{
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			MemberSearch.Click += new System.EventHandler( SearchButton_Click );

			AccountsFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			AccountsFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			AccountsFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			AccountsFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			this.Load += new System.EventHandler( this.Page_Load );
		}

		#endregion
	}
}