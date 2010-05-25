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
	public class UserMove : System.Web.UI.Page
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
		/// Logged in admin system rights instance
		/// </summary>
		UserSystemAdminRights uRights;
		
		/// <summary>
		/// Logged in user system rights value
		/// </summary>
		int sysAccessPolicy = 0;

                /// <summary>
                /// Reprovision State admin button control.
                /// </summary>
                protected Button RefreshButton;


		/// <summary>
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected DataGrid Accounts;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected ListFooter AccountsFooter;

		
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
			TopNav.AddBreadCrumb( GetString( "SYSTEM" ), "SystemInfo.aspx" );
			TopNav.AddBreadCrumb( GetString( "REPROVISIONBUTTON" ), null );
			// Pass this page information to create the help link
			TopNav.AddHelpLink("USERMOVE");
		}
		
		/// <summary>
		/// Creates a DataSource containing user names from a search.
		/// </summary>
		/// <returns>An DataView object containing the ifolder users.</returns>
		private DataView CreateDataSource()
		{
			// store the server list , so that it can be used to fill up the drop down box for each user and need not make 
			// calls for each user
		
			DataTable dt = new DataTable();

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "AdminField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "ProvisionedField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "CurrentHomeField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "NewHomeField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "PercentageStatusField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "StatusField", typeof( string ) ) );

			DataRow dr;

			iFolderUserSet userList = web.GetReprovisionUsers( 
				CurrentUserOffset, 
				Accounts.PageSize );

			foreach( iFolderUser user in userList.Items )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = user.ID;
				dr[ 2 ] = ( user.MemberRights == Rights.Admin ) ? true : false;
				dr[ 3 ] = ( user.HomeServer == string.Empty ) ?  null : user.HomeServer ;
				dr[ 4 ] = user.UserName;
				dr[ 5 ] = ( user.HomeServer == string.Empty ) ?  null : user.HomeServer ;
				dr[ 6 ] = ( user.NewHomeServer == string.Empty ) ?  null : user.NewHomeServer ;
				dr[ 7 ] = user.DataMovePercentage.ToString() + " %   ";
				dr[ 8 ] = user.DataMoveStatus;

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

			string userID = Session[ "UserID" ] as String;
			if(userID != null)
				sysAccessPolicy = web.GetUserGroupRights(userID, null);
			else
				sysAccessPolicy = 0; 
			uRights = new UserSystemAdminRights(sysAccessPolicy);
			if(uRights.SystemPolicyManagementAllowed == false)
				Page.Response.Redirect(String.Format("Error.aspx?ex={0}&Msg={1}",GetString( "ACCESSDENIED" ), GetString( "ACCESSDENIEDERROR" )));

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				RefreshButton.Text = GetString( "REFRESH" );

				// Initialize state variables.
				CurrentUserOffset = 0;
				TotalUsers = 0;
				CheckedUsers = new Hashtable();
				ServerProvisioningNames = new Hashtable();
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
		//private void SetActionButtons()
		//{
		//	Hashtable ht = CheckedUsers;
		//	bool UnProvisionedUser = false;
		//	if(ht.Keys.Count > 0 )
		//		UnProvisionedUser = true;
		//	if (UnProvisionedUser == false)
		//	{
		//	}
		//}

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
                /// Event handler that gets called when the refresh admin button is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void OnRefreshButton_Click( object source, EventArgs e )
                {
                        Response.Redirect( "userMove.aspx", true );
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
			}
			else
			{
			}
			return;
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

		//	MemberSearch.Click += new System.EventHandler( SearchButton_Click );

			AccountsFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			AccountsFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			AccountsFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			AccountsFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			this.Load += new System.EventHandler( this.Page_Load );
		}

		#endregion
	}
}
