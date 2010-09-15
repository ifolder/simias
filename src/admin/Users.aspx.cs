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


using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Resources;
using System.Threading;
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
		private const int AccountsHomeServerColumn = 6;
		private const int AccountsStatusColumn = 7;

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
		protected ListFooter AccountsFooter;

		
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
		/// Provision user button control.
		/// </summary>
		protected Button ProvisionButton;

		/// <summary>
		/// Enable user button control.
		/// </summary>
		protected Button EnableButton;
        
		/// <summary>
		/// Save user button.
		/// </summary>
		protected Button SaveButton;

		/// <summary>
		/// Create user button.
		/// </summary>
		protected Button CreateButton;

		/// <summary>
		/// string array to store names of server.
		/// </summary>
		protected string [] ServerList;

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
		/// Gets or sets the server provisioning method for users
		/// </summary>
		private Hashtable ServerProvisioningNames
		{
			get { return ViewState[ "ServerProvisioningNames" ] as Hashtable; }
			set { ViewState[ "ServerProvisioningNames" ] = value; }
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
			TopNav.AddBreadCrumb( GetString( "USERS" ), null );
			// Pass this page information to create the help link
			TopNav.AddHelpLink(GetString("USERS"));
		}

		/// <summary>
		/// Creates a DataSource containing user names from a search.
		/// </summary>
		/// <returns>An DataView object containing the ifolder users.</returns>
		private DataView CreateDataSource()
		{
			int NameWidth = 24; //this is a magic number that is used to display the username and 
						//fullname in shortened format.
			// store the server list , so that it can be used to fill up the drop down box for each user and need not make 
			// calls for each user
			ServerList = GetServerList();
		
			DataTable dt = new DataTable();

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "DisabledField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "AdminField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "ProvisionedField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "StatusField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "PreferenceField", typeof( string) ) );

			DataRow dr;

			iFolderUserSet userList = web.GetUsersBySearch( 
				MemberSearch.SearchAttribute, 
				MemberSearch.SearchOperation, 
				( MemberSearch.SearchName == String.Empty ) ? "*" : MemberSearch.SearchName, 
				CurrentUserOffset, 
				Accounts.PageSize );

			foreach( iFolderUser user in userList.Items )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = user.ID;
				dr[ 2 ] = !user.Enabled;
				dr[ 3 ] = ( user.MemberRights == Rights.Admin ) ? true : false;
				dr[ 4 ] = ( user.HomeServer == string.Empty ) ?  null : user.HomeServer ;
				dr[ 5 ] = (user.UserName.Length > NameWidth) ? web.GetShortenedName(user.UserName, NameWidth) : user.UserName ;
				dr[ 6 ] = (user.UserName.Length > NameWidth) ? web.GetShortenedName(user.FullName, NameWidth) : user.FullName ;
				dr[ 7 ] = GetString( user.Enabled ? "YES" : "NO" );
				dr[8] = Convert.ToString(user.Preference);

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
				dr[ 4 ] = null;
				dr[ 5 ] = String.Empty;
				dr[ 6 ] = String.Empty;
				dr[ 7 ] = String.Empty;
				dr[8] = "0";

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotalUsers = userList.Total;

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
			GetSelectedItem();
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
			//string userID = Session[ "UserID" ] as String;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				DisableButton.Text = GetString( "DISABLE" );
				EnableButton.Text = GetString( "ENABLE" );
				ProvisionButton.Text = GetString( "PROVISION" );
				SaveButton.Text = GetString( "SAVE" );

				// Initialize state variables.
				CurrentUserOffset = 0;
				TotalUsers = 0;
				AllUsersCheckBox.Checked = false;
				CheckedUsers = new Hashtable();
				ServerProvisioningNames = new Hashtable();

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
			
			// In ru/pl/hu as the PROVISION string is too long , increase the width for this language.
			string code = Thread.CurrentThread.CurrentUICulture.Name;
			if ( code.StartsWith("ru") || code.StartsWith("hu") )
				ProvisionButton.Width = 250;
			else if ( code.StartsWith("pl") )
				ProvisionButton.Width = 180;
			else if (code.StartsWith("pt") || code.StartsWith("de"))
				DisableButton.Width = 120;
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
			ProvisionButton.Enabled = true;
			bool UnProvisionedUser = false;
			if(ht.Keys.Count > 0 )
				UnProvisionedUser = true;
			if (UnProvisionedUser == false)
			{
				ProvisionButton.Enabled = false;
			}
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
		/// Get the logged in admin rights for the user. The rights value will be -1 or 0xffff for primary admin.
		/// </summary>
		private int GetRightsForUser(string userID)
		{
			int preference = 0;
			foreach( DataGridItem item in Accounts.Items )
			{
				string userid = item.Cells[ AccountsIDColumn ].Text;
				if( userid == userID )
				{
					preference = Convert.ToInt32(item.Cells[ 8].Text);
					break;
				}
			}
			return preference;
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
					/// Check for the policy for the groupadmin...
					int preference = GetRightsForUser(userID);
					if( preference != -1 && preference != 0xffff)
					{
						UserGroupAdminRights rights = new UserGroupAdminRights((int)preference);
						if( rights.EnableDisableUserAllowed == false)
							continue;
					}

					UserPolicy policy = Utils.GetUserPolicyObject( userID );
					policy.LoginEnabled = status;
					try
					{
						web.SetUserPolicy( policy );
					}
					catch ( Exception ex )
					{
						string errMsg = String.Format( GetString( "ERRORCANNOTSETUSERPOLICY" ), userID );
						TopNav.ShowError( errMsg, ex );
						return;
					}
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
		protected string GetUserImage( Object isAdmin ,Object isProvisioned )
		{
			if( ( bool )isAdmin )
                                return  "images/ifolder_admin.gif";
                        if( ( isProvisioned as string ) != null  ) 
				return  "images/user.png" ;
	
			return  "images/unprovisioned_user.png" ;
		}

		/// <summary>
		/// Gets whether the user should be able to be checked.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>True if the user is allowed to be checked.</returns>
		protected bool IsUserEnabled( object id )
		{
			// return !IsSuperAdmin( id as string );
			// Letting all users for re-provision
			return true;
		}

		/// <summary>
		/// Gets whether the user is provisioned or not.
		/// </summary>
		/// <param name="provisioned value"></param>
		/// <returns>True if the user is allowed to be checked.</returns>
		protected bool IsUserProvisioned( object isProvisioned )
		{
			// If user is provisioned , then enabled field should be false
			return (isProvisioned as string ) == null ? true : false ;
		}
		/// <summary>
		/// Gets whether the user is provisioned or not.
		/// </summary>
		/// <param name="provisioned value"></param>
		/// <returns>True if the user is allowed to be checked.</returns>
		protected bool IsUserProvisioned( object isProvisioned, object  preference)
		{
			// If user is provisioned , then enabled field should be false
			bool retval = false;
			retval = (isProvisioned as string ) == null ? true : false ;
			if( retval == true)
			{
				int val=0;

				string pref = preference as string;
				val = Convert.ToInt32(pref);
				if( val == 0 || val == 0xffff )
					return true;
				
				UserGroupAdminRights rights = new UserGroupAdminRights(val);
				if( rights.ProvisioningAllowed )
					retval = true;
				else
					retval = false;
			}

			return retval;
			//return (isProvisioned as string ) == null ? true : false ;
		}
	
		
		/// <summary>
		/// returns the liset of server names .
		/// </summary>
		/// <returns></returns>
		protected string[] GetServerList()
		{
			
			//string [] ProvisioningListStrTemp= {"Server1", "Server2", "Server3"};  //web.GetServerProvisioningList();
			string [] ProvisioningListStrTemp= web.GetServerProvisioningList();
			if (ProvisioningListStrTemp != null)
			{
				string [] ProvisioningListStr = new string [ProvisioningListStrTemp.Length + 1];
				/// making 1st entry of dropdownlist as None
				ProvisioningListStr[0] = GetString("NOTAPPLICABLE");   
				for (int i = 1; i <= ProvisioningListStrTemp.Length ; i++)
					ProvisioningListStr[i] = String.Copy(ProvisioningListStrTemp[i-1]);
				return ProvisioningListStr;	
			}
			else
			{
				string [] ProvisioningListStrNA = new string [ 1 ];
				ProvisioningListStrNA[0] = GetString("NOTAPPLICABLE");
				return ProvisioningListStrNA;
			}
		}
		
		/// <summary>
		/// whether the drop-down list is visible or not .when search is applied
		/// </summary>
		/// <returns></returns>
		protected bool IsListVisible( Object UserID )
		{
			
			string userID = UserID as string ; 
			if( ! userID.Equals (String.Empty)) 
			{
				return true;
			}
			return false;

		}
		
		/// <summary>
		/// Show the drop-down list to select provisioning method for a particular user.
		/// </summary>
		/// <returns></returns>
		protected string[] ShowProvisionServerList( Object UserID, Object isProvisioned )
		{
			if( (isProvisioned as string ) == null)
			{
				/// show the drop-down box to let admin select the provisioning method
				string userID = UserID as string ; 
				if( ! userID.Equals (String.Empty)) 
				{
					string [] ServerNames = ServerList ; 
					return ServerNames ; 
				}
				return null;
			}
			else
			{	
				/// User is provisioned so only one server name will be displayed.
				//iFolderUser ProvisionedUser = web.GetUser (UserID as string);
				string [] ProvisionedServerName = new string [1];
				ProvisionedServerName[0] = isProvisioned as string; 
				return ProvisionedServerName;
			}
		}

		/// <summary>
		/// return the selected index of server provisioning methods, while binding the data
		/// </summary>
		/// <returns></returns>
		protected void GetSelectedItem() 
		{
			foreach( DataGridItem item in Accounts.Items )
			{
				string userid = item.Cells[ AccountsIDColumn ].Text;
				if (ServerProvisioningNames.ContainsKey( userid ) )//&& (! (bool) isprovisioned))
				{
					((DropDownList)item.FindControl("ProvisionServerList")).SelectedValue = (string) ServerProvisioningNames[userid];
				}

			}
		}

		/// <summary>
		/// event handler that gets called when the all user check box is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAllUsersChecked( object source, EventArgs e )
		{
			CheckBox checkBox = source as CheckBox;
			foreach( DataGridItem item in Accounts.Items )
			{
				// in order to be checked, the row must not be empty.
				string userID = item.Cells[ AccountsIDColumn ].Text;
				if ( userID != "&nbsp;" && !IsSuperAdmin( userID ) )
				{
					if ( checkBox.Checked )
					{
						CheckedUsers[ userID ] = item.Cells[ AccountsDisabledColumn ].Text == Boolean.FalseString;
					}
					else
					{
						// remove this user from the list.
						CheckedUsers.Remove( userID );
					}
				}
			}

			// set the action buttons appropriately.
			SetActionButtons();

			// rebind the data source with the new data.
			GetUsers();
		}

		/// <summary>
		/// event handler that gets called when the create user button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnCreateButton_Click( object source, EventArgs e )
		{
			Response.Redirect( "CreateUser.aspx" );
		}

		/// <summary>
		/// event handler that gets called when the delete user button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDeleteButton_Click( object source, EventArgs e )
		{
			foreach( string userID in CheckedUsers.Keys )
			{
				// bugbug!! - need a call to delete by user ids.
				try
				{
					iFolderUser user = web.GetUser( userID );
					web.DeleteUser( user.UserName );
				}
				catch ( Exception ex )
				{
					string errMsg = String.Format( GetString( "ERRORCANNOTDELETEUSER" ), userID );
					TopNav.ShowError( errMsg, ex );
					return;
				}
			}

			// clear the checked members.
			CheckedUsers.Clear();
			AllUsersCheckBox.Checked = false;

			// set the action buttons.
			SetActionButtons();

			// rebind the data source with the new data.
			GetUsers();
		}

		/// <summary>
		/// event handler that gets called when the disable user button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDisableButton_Click( object source, EventArgs e )
		{
			SetSelectedUserStatus( false );
		}

		/// <summary>
		/// event handler that gets called when the enable user button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnEnableButton_Click( object source, EventArgs e )
		{
			SetSelectedUserStatus( true );
		}

		/// <summary>
		/// event handler that gets called when the provision user button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnProvisionButton_Click( object source, EventArgs e )
		{
			string uplist="";
			string plist="";
			foreach (string userid in CheckedUsers.Keys)
			{
				int preference = GetRightsForUser(userid);
				if( preference == -1 )
					preference = 0xffff;
				UserGroupAdminRights rights = new UserGroupAdminRights((int)preference);
				if( !rights.ProvisioningAllowed )
				{
					continue;
				}
			
				iFolderUser ProvisionedUser = web.GetUser (userid);
				if( ProvisionedUser.HomeServer == null || ProvisionedUser.HomeServer == String.Empty ||  ProvisionedUser.HomeServer == "" )
				{	
					uplist += userid;
					uplist += ":";
				}
				else
				{
					plist += userid;
					plist += ":";
					plist += ProvisionedUser.HomeServer;
					plist += ":";
				}
			}

			/// clear the hastable
			ServerProvisioningNames.Clear();
			if(( uplist == null || uplist == string.Empty) && (plist == null || plist == string.Empty))
			{
				string errormessage = GetString("ERRORACCESSEXCEPTION");
				TopNav.ShowError(errormessage);
				return;
			}

			/// call the next page to provision the users
			Response.Redirect(String.Format("ProvisionUsers.aspx?&userlist={0}&puserlist={1}",  uplist, plist) ) ;
		}

		/// <summary>
		/// event handler that gets called when the save user button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnSaveButton_Click( object source, EventArgs e )
		{
			Hashtable ServerProvisioningNamesNew = new Hashtable();
			foreach(string userid in ServerProvisioningNames.Keys)
			{
				int preference = GetRightsForUser(userid);
				if( preference == -1)
					preference = 0xffff;
				UserGroupAdminRights rights = new UserGroupAdminRights((int)preference);
				if( rights.ProvisioningAllowed )
				{
					ServerProvisioningNamesNew.Add(userid, ServerProvisioningNames[userid]);
				}
			}
			ServerProvisioningNames = ServerProvisioningNamesNew;
			String [] ServerNames = new string [ServerProvisioningNames.Keys.Count];
			String [] UserIDs = new string [ServerProvisioningNames.Keys.Count];
			ServerProvisioningNames.Keys.CopyTo(UserIDs, 0);	
			ServerProvisioningNames.Values.CopyTo(ServerNames, 0);	

			iFolderAdmin remoteweb = new iFolderAdmin ();
                        iFolderServer[] list = web.GetServers();

                        foreach( iFolderServer server in list )
                        {
                                if (server.IsMaster)
                                {	
                        		remoteweb.PreAuthenticate = true;
                        		remoteweb.Credentials = web.Credentials;
                        		remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
                        		remoteweb.GetAuthenticatedUser();
					remoteweb.ProvisionUsersToServers(ServerNames, UserIDs);
                                       	break;
                                }
                        }

			/// clear the hastable
			ServerProvisioningNames.Clear();
			
			SaveButton.Enabled = false;
			
			/// Display the page with new values
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();
			AllUsersCheckBox.Checked = false;
			SetPageButtonState();
			GetSelectedItem();

		}

		/// <summary>
		/// Event handler that gets called when selected index is changed in drop-down list.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void UpdateServerProvisioning(Object sender, EventArgs e)
		{
			DropDownList ServerProvisioningList = sender as DropDownList;
                        DataGridItem item = ServerProvisioningList.Parent.Parent as DataGridItem;
			string SelectedName = ServerProvisioningList.SelectedItem.Text;
			string UserID = item.Cells[ AccountsIDColumn ].Text;
			if(SelectedName.Equals(GetString("NOTAPPLICABLE")))
			{
				/// remove entry fron hashtable, if any for this userid
				if(ServerProvisioningNames.ContainsKey(UserID))
				{
					ServerProvisioningNames.Remove(UserID);
				}
			}
			else
			{
				ServerProvisioningNames[UserID] = SelectedName;
			}

			if (ServerProvisioningNames.Keys.Count > 0)
			{
				SaveButton.Enabled = true;
			}
			else
			{
				SaveButton.Enabled = false;
			}
			return;
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
			GetSelectedItem();
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
			SetPageButtonState();
			GetSelectedItem();
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
			GetSelectedItem();
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
			GetSelectedItem();
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
			GetSelectedItem();
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
