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
*                 $Author : Anil Kumar <kuanil@novell.com>
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
using System.Net;
using System.Resources;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for MemberSelect.
	/// </summary>
	public class ProvisionUsers : System.Web.UI.Page
	{
		#region Class Members
		//private static readonly iFolderWebLogger log = new iFolderWebLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
		/// <summary>
		/// Viewable MemberList data grid cell indices.
		/// </summary>
		private const int Member_IDCell       = 0;
		private const int Member_ImageCell    = 1;
		private const int Member_UserNameCell = 2;
		private const int Member_FullNameCell = 3;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

                /// <summary>
                /// iFolder Connection to the remote server
                /// </summary>
                private iFolderAdmin remoteweb;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;

		/// <summary>
		/// Page title control.
		/// </summary>
		protected Label HeaderTitle;

		/// <summary>
		/// Page subtitle control.
		/// </summary>
		protected Label SubHeaderTitle;

		/// <summary>
		/// select the server dropdown.
		/// </summary>
		protected DropDownList SelectServerList;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected Button OkButton;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected Button CancelButton;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected DataGrid MemberList;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected ListFooter MemberListFooter;
		
		#endregion

		#region Properties

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
		/// Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			// Show the proper tab.
			Control body = FindControl( "ifolders" );
			TopNav.AddBreadCrumb( GetString( "USERS" ), "Users.aspx" );
			TopNav.AddBreadCrumb( GetString( "PROVISION_REPROVISION_TITLE" ), null);

			if ( body != null )
			{
				body.ID = "users";
			}

			TopNav.SetActivePageTab( TopNavigation.PageTabs.Users );
			// Pass this page information to create the help link
			TopNav.AddHelpLink(GetString("PROVISIONUSERS"));
		}

		/// <summary>
		/// Creates a DataSource containing user names from a search.
		/// </summary>
		/// <returns>An DataView object containing the ifolder users.</returns>
		private DataView CreateMemberList()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "ProvisionedField", typeof( string ) ) );

			string SelectedUserList = Request.QueryString.Get("UserList"); 
			string [] ListOfSelectedUsers = SelectedUserList.Split(new char[] {':'});
			for(int index = CurrentUserOffset ; (index < (ListOfSelectedUsers.Length - 1)  && index < (CurrentUserOffset + MemberList.PageSize)); index++)
			{
				iFolderUser user = web.GetUser(ListOfSelectedUsers[index]);
					/// Those users will be displayed who were passed from previous page
					dr = dt.NewRow();
					dr[ 0 ] = true;
					dr[ 1 ] = user.ID;
					dr[ 2 ] = user.UserName;
					dr[ 3 ] = user.FullName;
					dr[ 4 ] = "false";

					dt.Rows.Add( dr );
				
			}

			string pSelectedUserList = Request.QueryString.Get("PUserList"); 
			string [] pListOfSelectedUsers = pSelectedUserList.Split(new char[] {':'});
			for(int index = 0 ; (index < (pListOfSelectedUsers.Length -1)  && dt.Rows.Count < MemberList.PageSize); index++)
			{
				iFolderUser user = web.GetUser(pListOfSelectedUsers[index++]);
					/// Those users will be displayed who were passed from previous page
					dr = dt.NewRow();
					dr[ 0 ] = true;
					dr[ 1 ] = user.ID;
					dr[ 2 ] = user.UserName;
					dr[ 3 ] = user.FullName;
					dr[ 4 ] = "true";

					dt.Rows.Add( dr );
				
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < MemberList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				//dr[ 2 ] = false;
				dr[ 2 ] = String.Empty;
				dr[ 3 ] = String.Empty;
				dr[ 4 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotalUsers = (ListOfSelectedUsers.Length - 1 )+ (pListOfSelectedUsers.Length -1);

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// returns whether the user is present in the passed user list or not .
		/// </summary>
		/// <returns></returns>
		//private bool UserPresentInList(string [] ListOfSelectedUsers, string userID)
		//{
		//	int index;
		//	for (index = 0 ; index < ListOfSelectedUsers.Length ; index++)
		//	{
		//		if (userID.Equals(ListOfSelectedUsers[index]))
		//		{
		//			return true;
		//		}
		//	}
		//	return false;
		//}

		/// <summary>
		/// Event handler that is called when a data grid item is bound.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MemberSelect_MemberItemDataBound( object sender, DataGridItemEventArgs e )
		{
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
				CancelButton.Text = GetString( "CANCEL" );

				// Initialize localized fields.
				HeaderTitle.Text = GetString( "SELECTSERVER" );
				OkButton.Text = GetString( "PROVISIONREPROVISION" );
				OkButton.Enabled = false;
				
				string [] ServerList = GetServerList();						
				SelectServerList.DataSource = ServerList;
				SelectServerList.DataBind();
				// Initialize state variables.
				CurrentUserOffset = 0;
				TotalUsers = 0;
			}
			string code = Thread.CurrentThread.CurrentUICulture.Name;
			if ( code.StartsWith("ru") || code.StartsWith("hu") )
				OkButton.Width = 330;
			else if ( code.StartsWith("de") )
				OkButton.Width = 400;
		}
		
		/// <summary>
		/// returns the liset of server names .
		/// </summary>
		/// <returns></returns>
		private string[] GetServerList()
		{
			string [] ProvisioningListStrTemp = web.GetServerProvisioningList();
			if (ProvisioningListStrTemp != null)
			{
				string [] ProvisioningListStr = new string [ProvisioningListStrTemp.Length + 1];
				/// making 1st entry of dropdownlist as None
				ProvisioningListStr[0] = GetString("NOTAPPLICABLE");   
				for (int i = 1; i <= ProvisioningListStrTemp.Length ; i++)
					ProvisioningListStr[i] = String.Copy(ProvisioningListStrTemp[i-1]);
				return ProvisioningListStr;
			}
			return null;
		}


		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender(object sender, EventArgs e)
		{
			// Initially populate the member list.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();
			SetMemberPageButtonState();

			// Build the breadcrumb list.
			BuildBreadCrumbList();
		}

		/// <summary>
		/// Sets the page button state of the Member list.
		/// </summary>
		private void SetMemberPageButtonState()
		{
			MemberListFooter.SetPageButtonState( 
				MemberList, 
				CurrentUserOffset, 
				TotalUsers, 
				GetString( "USERS" ),
				GetString( "USER" ) );
		}
		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler for the cancel button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void CancelButton_Clicked( Object sender, EventArgs e )
		{
			// Return back to the referring page.
			//string url = web.TrimUrl(ReferringPage);
			string url = "Users.aspx";
			Page.Response.Redirect( url );
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
		/// Event handler for the PageFirstButton.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void Member_PageFirstButton_Click( object source, ImageClickEventArgs e )
		{
			// Set to get the first users.
			CurrentUserOffset = 0;

			// Rebind the data source with the new data.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();

			// Set the button state.
			SetMemberPageButtonState();

		}

		/// <summary>
		/// Event that first when the MbrPageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void Member_PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset += MemberList.PageSize;

			// Rebind the data source with the new data.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();

			// Set the button state.
			SetMemberPageButtonState();

		}

		/// <summary>
		/// Event that first when the MbrPageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void Member_PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset =  ( ( TotalUsers - 1 ) / MemberList.PageSize ) * MemberList.PageSize;

			// Rebind the data source with the new data.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();

			// Set the button state.
			SetMemberPageButtonState();

		}

		/// <summary>
		/// Event that first when the MbrPagePreviousButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void Member_PagePreviousButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset -= MemberList.PageSize;
			if ( CurrentUserOffset < 0 )
			{
				CurrentUserOffset = 0;
			}

			// Rebind the data source with the new data.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();

			// Set the button state.
			SetMemberPageButtonState();

		}

                /// <summary>
                /// Gets the image representing the user type.
                /// </summary>
                /// <returns></returns>
                protected string GetUserImage( Object isProvisioned )
                {
			string Provisioned = isProvisioned as string;
			if( Provisioned != null && String.Compare(Provisioned.ToLower(), "true") == 0)
                                return  "images/user.png" ;
			else if(Provisioned != null && String.Compare(Provisioned.ToLower(), "false") == 0)
                        	return  "images/unprovisioned_user.png" ;
			else
				return String.Empty;
                }


		/// <summary>
		/// Event handler for the ok button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OkButton_Clicked( Object sender, EventArgs e )
		{
			/// Extract the user list and server name 
			string SelectedUserList = Request.QueryString.Get("UserList");
            string [] ListOfSelectedUsers = SelectedUserList.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
			string ServerName = SelectServerList.SelectedValue ; 

                       	iFolderServer[] list = web.GetServers();
			remoteweb = new iFolderAdmin ();

			if(ListOfSelectedUsers.Length > 0 && ListOfSelectedUsers[0] != null && ListOfSelectedUsers[0] != String.Empty && ListOfSelectedUsers[0] != "")
                       		foreach( iFolderServer server in list )
                       		{
                               		if (server.IsMaster)
                               		{
                                       		remoteweb.PreAuthenticate = true;
                                       		remoteweb.Credentials = web.Credentials;
                                       		remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
                                       		remoteweb.GetAuthenticatedUser();
                                       		remoteweb.ProvisionUsersToServer(ServerName, ListOfSelectedUsers);
                                       		break;
                               		}
                       		}

                        string pSelectedUserList = Request.QueryString.Get("PUserList");
                        string [] pListOfSelectedUsers = pSelectedUserList.Split(new char[] {':'},StringSplitOptions.RemoveEmptyEntries);
                        for(int index = 0 ; (index < (pListOfSelectedUsers.Length -1) ); index++)
                        {
	                        foreach( iFolderServer server in list )
                        	{
                                	if (String.Compare(server.Name,pListOfSelectedUsers[index+1]) == 0)
                                	{
                                        	remoteweb.PreAuthenticate = true;
                                        	remoteweb.Credentials = web.Credentials;
                                        	remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
                                        	remoteweb.GetAuthenticatedUser();
                                		string SelectedUser = pListOfSelectedUsers[index++];
						if(SelectedUser != null && SelectedUser != String.Empty && SelectedUser != "")
                                        		remoteweb.ReProvisionUsersToServer(ServerName, SelectedUser);
                                	}
                        	}
                        }


			string url = "Users.aspx";
			Page.Response.Redirect( url );
		}

		/// <summary>
		/// Event handler that gets called when selected index is changed in drop-down list.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnSelectServerList_Changed(Object sender, EventArgs e)
		{
			string SelectedServerName = SelectServerList.SelectedItem.Text;
			if( SelectedServerName.Equals(GetString("NOTAPPLICABLE")))
			{
				OkButton.Enabled = false;
			}
			else
			{
				OkButton.Enabled = true;
			}
		}	

		#endregion

		#region Web Form Designer generated code

		/// <summary>
		/// OnInit
		/// </summary>
		/// <param name="e"></param>novell
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
		private void InitializeComponent()
		{
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			MemberList.ItemDataBound += new DataGridItemEventHandler( MemberSelect_MemberItemDataBound );

			MemberListFooter.PageFirstClick += new ImageClickEventHandler( Member_PageFirstButton_Click );
			MemberListFooter.PagePreviousClick += new ImageClickEventHandler( Member_PagePreviousButton_Click );
			MemberListFooter.PageNextClick += new ImageClickEventHandler( Member_PageNextButton_Click );
			MemberListFooter.PageLastClick += new ImageClickEventHandler( Member_PageLastButton_Click );

			this.Load += new System.EventHandler( this.Page_Load );
		}
		#endregion
	}
}
