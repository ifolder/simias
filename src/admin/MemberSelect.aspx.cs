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
using System.Net;
using System.Resources;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using Simias.Client;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for MemberSelect.
	/// </summary>
	public class MemberSelect : System.Web.UI.Page
	{
		#region Class Members
	
		/// <summary>
		/// enum value to store the different disable sharing options
		/// </summary>
		public enum Share
		{	
			Sharing = 1,
			EnforcedSharing = 4,
			DisableSharing = 8
		}

		                /// <summary>
                /// iFolder list display types.
                /// </summary>
                private enum ListAdmins
                {
                                // For current Implementation, enum value AllAdmins is not used, can be used in future
                                AllAdmins = 0,
                                GroupAdmins = 1,
                                PrimaryAdmins = 2
                }

		
		/// <summary>
		/// Viewable MemberList data grid cell indices.
		/// </summary>
		private const int Member_IDCell       = 0;
		private const int Member_CheckBoxCell = 1;
		private const int Member_ImageCell    = 2;
		private const int Member_UserNameCell = 3;
		private const int Member_FullNameCell = 4;

		/// <summary>
		/// Operations
		/// </summary>
		private enum PageOp
		{
			AddMember,
			AddAdmin,
			CreateiFolder,
			AddSecondaryAdmin,
			EditSecondaryAdmin,
			DeleteSecondaryAdmin
		}


		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

                /// <summary>
                /// Remote iFolder Connection
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
		/// Web controls.
		/// </summary>
		protected Button BackButton;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected Button OkButton;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected Button CancelButton;

		/// <summary>
		/// All members selection checkbox control.
		/// </summary>
		protected CheckBox AllMembersCheckBox;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected DataGrid MemberList;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected MemberSearch MemberSearch;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected ListFooter MemberListFooter;
		
		/// <summary>
                /// Current server URL
                /// </summary>
                protected string currentServerURL;
		
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
		/// Gets the iFolder Description.
		/// </summary>
		private string iFolderDescription
		{
			// Now , through asp pages , for ifolderdescription field ,only utf8 encoded and base64 converted strings should be sent
                        get
                        {
                                string iFolderDescBase64 = Request.Params[ "desc" ] ;
				if(iFolderDescBase64 == null)
				{
					return String.Empty;
				}
                                string param = "";
                                try{

                                        byte[] iFolderDescInByte = Convert.FromBase64String(iFolderDescBase64);
                                        UTF8Encoding utf8Name = new UTF8Encoding();
                                        param = utf8Name.GetString(iFolderDescInByte);
                                }catch(Exception ex)
                                {
                                        throw ex;
                                }
                                return param;
                        }

		}

		/// <summary>
		/// Gets or sets the existing members list.
		/// </summary>
		private Hashtable ExistingMemberList
		{
			get { return ViewState[ "ExistingMembers" ] as Hashtable; }
			set { ViewState[ "ExistingMembers" ] = value; }
		}

		/// <summary>
		/// Gets the full name of the owner of the iFolder.
		/// </summary>
		private string FullName
		{
			get 
			{ 
				string param = Request.Params[ "fn" ];
				if ( ( param == null ) || ( param == String.Empty ) )
				{
					throw new HttpException( ( int )HttpStatusCode.BadRequest, "No full name was specified." );
				}

				return param;
			}
		}

		/// <summary>
		/// Gets the iFolder ID.
		/// </summary>
		private string iFolderID
		{
			get 
			{ 
				string param = Request.Params[ "id" ];
				if ( ( param == null ) || ( param == String.Empty ) )
				{
					throw new HttpException( ( int )HttpStatusCode.BadRequest, "No ifolder was specified." );
				}

				return param;
			} 
		}

		/// <summary>
		/// Gets the Secondary Admin ID.
		/// </summary>
		private string SecondaryAdminID
		{
			get 
			{ 
				string param = Request.Params[ "secondaryadminid" ];
				if ( ( param == null ) || ( param == String.Empty ) )
				{
					throw new HttpException( ( int )HttpStatusCode.BadRequest, "No Secondary Admin ID was specified." );
				}

				return param;
			} 
		}

		/// <summary>
		/// Gets the user or iFolder name.
		/// </summary>
		private string iFolderName
		{
			get
			{
				// Now , through asp pages , for ifoldername field ,only utf8 encoded and base64 converted strings should be sent 

				string iFolderNameBase64 = Request.Params[ "name" ] ;
                                if ( ( iFolderNameBase64 == null ) || ( iFolderNameBase64 == String.Empty ) )
                                {
                                        throw new HttpException( ( int )HttpStatusCode.BadRequest, "No user name was specified." );
                                }
                                string param;
                                try{

                                        byte[] iFolderNameInByte = Convert.FromBase64String(iFolderNameBase64);
                                        UTF8Encoding utf8Name = new UTF8Encoding();
                                        param = utf8Name.GetString(iFolderNameInByte);
                                }catch(Exception ex)
                                {
                                        TopNav.ShowError( String.Format ( "MULTIBYTEERROR {0}",ex.Message ) );
                                        throw ex;
                                }
                                return param;
			}
		}

		/// <summary>
		/// Gets the owner of the iFolder.
		/// </summary>
		private string iFolderOwner
		{
			get 
			{ 
				string param = Request.Params[ "owner" ];
				if ( ( param == null ) || ( param == String.Empty ) )
				{
					throw new HttpException( ( int )HttpStatusCode.BadRequest, "No owner was specified." );
				}

				return param;
			}
		}

		/// <summary>
		/// Gets the starting member list page of the previous owner select page.
		/// </summary>
		private string MemberListPage
		{
			get 
			{ 
				string param = Request.Params[ "pg" ];
				return ( ( param == null ) || ( param == String.Empty ) ) ? "0" : param;
			} 
		}

		/// <summary>
		/// Gets the operation to perform for this web page.
		/// </summary>
		private PageOp Operation
		{
			get
			{
				string param = Request.Params[ "op" ];
				if ( ( param != null ) && ( param != String.Empty ) )
				{
					switch ( param.ToLower() )
					{
						case "addmember":
							return PageOp.AddMember;

						case "addadmin":
							return PageOp.AddAdmin;

						case "createifolder":
							return PageOp.CreateiFolder;

						case "addsecondaryadmin":
							return PageOp.AddSecondaryAdmin;

						case "editsecondaryadmin":
							return PageOp.EditSecondaryAdmin;

						case "deletesecondaryadmin":
							return PageOp.DeleteSecondaryAdmin;

						default:
							throw new HttpException( ( int )HttpStatusCode.BadRequest, "An invalid operation was specified." );
					}
				}
				else
				{
					throw new HttpException( ( int ) HttpStatusCode.BadRequest, "No operation was specified." );
				}
			}
		}

		/// <summary>
		/// Gets or sets the referring page.
		/// </summary>
		private string ReferringPage
		{
			get { return ViewState[ "ReferringPage" ] as String; }
			set { ViewState[ "ReferringPage" ] = value; }
		}

		/// <summary>
		/// Gets or sets the members to add information.
		/// </summary>
		private Hashtable MembersToAdd
		{
			get { return Session[ "MembersToAdd" ] as Hashtable; }
			set { Session[ "MembersToAdd" ] = value; }
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
			switch ( Operation )
			{
				case PageOp.CreateiFolder:
				{
					// Create ifolder is called from both the userdetails page and the main ifolder list
					// page. Need to determine which one we came from.
					Uri uri = new Uri( ReferringPage );
					if ( uri.AbsolutePath.EndsWith( "UserDetails.aspx" ) )
					{
						TopNav.AddBreadCrumb( GetString( "USERS" ), "Users.aspx" );
						TopNav.AddBreadCrumb( FullName, String.Format( "UserDetails.aspx?id={0}", iFolderOwner ) );
						TopNav.AddBreadCrumb( GetString( "CREATENEWIFOLDER" ), null );

						if ( body != null )
						{
							body.ID = "users";
						}

						// Add the missing href to the ifolder tab and remove the user one.
						TopNav.SetActivePageTab( TopNavigation.PageTabs.Users );
					}
					else
					{
						TopNav.AddBreadCrumb( GetString( "IFOLDERS" ), "iFolders.aspx" );
						TopNav.AddBreadCrumb( GetString( "CREATENEWIFOLDER" ), null );
					}
					TopNav.AddHelpLink(GetString("CREATENEWIFOLDER"));
					break;
				}

				case PageOp.AddMember:
				{
					iFolderDetails folder = web.GetiFolderDetails(iFolderID);
					string BreadFolderName = folder.Name;
					TopNav.AddBreadCrumb( GetString( "IFOLDERS" ), "iFolders.aspx" );
					TopNav.AddBreadCrumb( BreadFolderName, String.Format( "iFolderDetailsPage.aspx?id={0}", iFolderID ) );
					TopNav.AddBreadCrumb( GetString( "ADDMEMBERS" ), null );
					break;
				}

				case PageOp.AddAdmin:
				{
					TopNav.AddBreadCrumb( GetString( "SYSTEM" ), "SystemInfo.aspx" );
					TopNav.AddBreadCrumb( GetString( "ADDADMINS" ), null );

					if ( body != null )
					{
						body.ID = "system";
					}

					// Add the missing href to the ifolder tab and remove the user one.
					TopNav.SetActivePageTab( TopNavigation.PageTabs.System );
					break;
				}

				case PageOp.AddSecondaryAdmin:
				case PageOp.EditSecondaryAdmin:
				case PageOp.DeleteSecondaryAdmin:
				{
					TopNav.AddBreadCrumb( GetString( "SYSTEM" ), "SystemInfo.aspx" );
					TopNav.AddBreadCrumb( GetString( "SECONDARYADMIN" ), null );

					if ( body != null )
					{
						body.ID = "system";
					}

					// Add the missing href to the ifolder tab and remove the user one.
					TopNav.SetActivePageTab( TopNavigation.PageTabs.System );
					//TopNav.AddHelpLink("SELECTSECADMIN");
					break;
				}

			}
			// Pass this page information to create the help link
			//TopNav.AddHelpLink(GetString("CREATENEWIFOLDER"));
		}

		/// <summary>
		/// Creates a list of existing admins.
		/// </summary>
		/// <returns></returns>
		private Hashtable CreateExistingAdminList()
		{
			// last param 0  is to display all admins (group/secondary)
			iFolderUserSet adminList = web.GetAdministrators( 0, 0, (int) ListAdmins.AllAdmins );
			Hashtable ht = new Hashtable( adminList.Total );
			foreach( iFolderUser admin in adminList.Items )
			{
				ht[ admin.ID ] = new MemberInfo( admin.ID, admin.UserName, admin.FullName );
			}

			return ht;
		}

		/// <summary>
		/// Creates a list of existing members.
		/// </summary>
		/// <returns></returns>
		private Hashtable CreateExistingMemberList()
		{
			Hashtable ht = null;
			switch(Operation)
			{
				// For editing/deleting secondary admin, display the objects (e.g. groups) he is managing 
				case PageOp.EditSecondaryAdmin: 
				case PageOp.DeleteSecondaryAdmin: 
							string [] MonitoredGroups = web.GetMonitoredGroups(SecondaryAdminID);
							ht = new Hashtable( MonitoredGroups.Length);
							foreach( string groupID in MonitoredGroups)
							{		
								iFolderUser GroupObject = web.GetUser(groupID);
								ht[ GroupObject.ID ] = new MemberInfo( GroupObject.ID, GroupObject.UserName, GroupObject.FullName);						
							}		
							break;
								
				default: 

							iFolderUserSet memberList = web.GetMembers( iFolderID, 0, 0 );
							ht = new Hashtable( memberList.Total );
							foreach( iFolderUser member in memberList.Items )
							{
								ht[ member.ID ] = new MemberInfo( member.ID, member.UserName, member.FullName );
							}
							break;
			}

			return ht;
		}

		/// <summary>
		/// Creates a DataSource containing user names from a search.
		/// </summary>
		/// <returns>An DataView object containing the ifolder users.</returns>
		private DataView CreateMemberList()
		{
			string CurrentPage = "";
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "AdminField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "ProvisionedField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "EnabledField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );

			iFolderUserSet userList = null;
			
			// If it is edit or delete Secondary admin operation, then only monitored groups should be displayed
			switch( Operation)
			{
				case PageOp.EditSecondaryAdmin:
				case PageOp.DeleteSecondaryAdmin:
						bool MonitoredGroups = true;
						
						userList = web.GetMonitoredGroupsBySearch( 
							MemberSearch.SearchAttribute, 
							MemberSearch.SearchOperation, 
							( MemberSearch.SearchName == String.Empty ) ? "*" : MemberSearch.SearchName, 
							CurrentUserOffset, 
							MemberList.PageSize, SecondaryAdminID, MonitoredGroups, false );
						
					break;
				case PageOp.AddSecondaryAdmin:
					CurrentPage = "AddSecondaryAdmin";
					userList = web.GetUsersBySearch( 
						MemberSearch.SearchAttribute, 
						MemberSearch.SearchOperation, 
						( MemberSearch.SearchName == String.Empty ) ? "*" : MemberSearch.SearchName, 
						CurrentUserOffset, 
						MemberList.PageSize );
					break;
				case PageOp.AddAdmin:
				default:
					CurrentPage = "AddPrimaryAdmin";
					userList = web.GetUsersBySearch( 
						MemberSearch.SearchAttribute, 
						MemberSearch.SearchOperation, 
						( MemberSearch.SearchName == String.Empty ) ? "*" : MemberSearch.SearchName, 
						CurrentUserOffset, 
						MemberList.PageSize );
					break;
			}

			foreach( iFolderUser user in userList.Items )
			{
				dr = dt.NewRow();
				dr[ 0 ] = ( user.MemberRights == Rights.Admin ) ? true : false;
				dr[ 1 ] = ( user.HomeServer == string.Empty ) ?  null : user.HomeServer ; 
				dr[ 2 ] = true;
				dr[ 3 ] = user.ID;
				// if already disabled then don't change, but if a group is enabled then disable (group cannot be admin)
				if(CurrentPage == "AddPrimaryAdmin" ||  CurrentPage == "AddSecondaryAdmin" )
					dr[ 4 ] = (!IsExistingMember( user.ID )) ? (!user.IsGroup) : !IsExistingMember(user.ID) ; 
				else
					dr[ 4 ] = !IsExistingMember( user.ID );
				dr[ 5 ] = user.UserName;
				dr[ 6 ] = user.FullName;
	
				dt.Rows.Add( dr );
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < MemberList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = false;
				dr[ 3 ] = String.Empty;
				dr[ 4 ] = false;
				dr[ 5 ] = String.Empty;
				dr[ 6 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotalUsers = userList.Total;

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// Creates a a list of existing members with only the owner specified.
		/// </summary>
		/// <returns></returns>
		private Hashtable CreateNewMemberList()
		{
			// Get the information about the new owner.
			iFolderUser owner = web.GetUser( iFolderOwner );
			Hashtable ht = new Hashtable();
			ht[ owner.ID ] = new MemberInfo( owner.ID, owner.UserName, owner.FullName );
			return ht;
		}

		/// <summary>
		/// Returns whether the specified user is an existing member of
		/// the current ifolder.
		/// </summary>
		/// <param name="userID">User ID</param>
		/// <returns>True if the user is an existing member.</returns>
		private bool IsExistingMember( string userID )
		{
			bool retval = false;
			if(ExistingMemberList != null && ExistingMemberList.ContainsKey( userID ))
				retval = true;
			return retval;
		}

		/// <summary>
		/// Returns whether the specified user is in the selected list.
		/// </summary>
		/// <param name="userID">User ID</param>
		/// <returns>True if user is in the selected list.</returns>
		private bool IsUserSelected( string userID )
		{
			return (MembersToAdd != null && MembersToAdd.ContainsKey( userID )) ? true : IsExistingMember( userID );
		}

		/// <summary>
		/// Event handler that is called when a data grid item is bound.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MemberSelect_MemberItemDataBound( object sender, DataGridItemEventArgs e )
		{
			if ( ( e.Item.ItemType == ListItemType.AlternatingItem ) || ( e.Item.ItemType == ListItemType.Item ) )
			{
				// Check for any rows that are not supposed to be displayed and disable the image.
				// All of the other cells should contain empty strings.
				DataTable dt = ( MemberList.DataSource as DataView ).Table;
				if ( ( bool )dt.Rows[ e.Item.DataSetIndex ][ "VisibleField" ] == true )
				{
					if ( ( bool )dt.Rows[ e.Item.DataSetIndex ][ "EnabledField" ] == false )
					{
						e.Item.Cells[ Member_UserNameCell ].Enabled = false;
						e.Item.Cells[ Member_UserNameCell ].Attributes.Add( "class", "disableditem3" );

						e.Item.Cells[ Member_FullNameCell ].Enabled = false;
						e.Item.Cells[ Member_FullNameCell ].Attributes.Add( "class", "disableditem4" );
					}
					else
					{
						e.Item.Cells[ Member_UserNameCell ].Attributes.Add( "class", "memberitem3" );
						e.Item.Cells[ Member_FullNameCell ].Attributes.Add( "class", "memberitem4" );
					}
				}
			}
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
			currentServerURL = web.Url;

			remoteweb = new iFolderAdmin();

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				BackButton.Text = GetString( "BACK" );
				CancelButton.Text = GetString( "CANCEL" );

				switch ( Operation )
				{
					case PageOp.CreateiFolder:
					{
						// Initialize state variables.
						if ( MembersToAdd == null )
						{
							MembersToAdd = new Hashtable();
						}

						// Initialize localized fields.
						HeaderTitle.Text = String.Format( GetString( "ADDMEMBERSTOIFOLDER" ), iFolderName );
						SubHeaderTitle.Text = String.Format( GetString( "IFOLDERISOWNEDBY" ), FullName );
						OkButton.Text = GetString( "CREATE" );

						// Remember the page that we came from.
						string param = Request.Params[ "ref" ];
						ReferringPage = ( ( param == null ) || ( param == String.Empty ) ) ? 
							Page.Request.UrlReferrer.ToString() : param;

						// Create an existing member list.
						ExistingMemberList = CreateNewMemberList();
						string publicUrl = web.GetHomeServerURLForUserID(iFolderOwner);
						if(String.Compare(web.Url, String.Concat(publicUrl + "/iFolderAdmin.asmx")) != 0)
						{

								if(publicUrl == null)
								{
										iFolderServer MasterServer = web.GetHomeServer();
										DomainService domainService = new DomainService();

										domainService.Url = MasterServer.PublicUrl + "/DomainService.asmx";
										domainService.Credentials = web.Credentials;
										domainService.PreAuthenticate = true;

										publicUrl = domainService.GetHomeServer( web.GetUser(iFolderOwner).UserName ).PublicAddress;
								}

								remoteweb.Url = publicUrl + "/iFolderAdmin.asmx";
								remoteweb.PreAuthenticate = true;
								remoteweb.Credentials = web.Credentials;
								web.Url = remoteweb.Url;
						}
						else
								remoteweb = web;

						break;
					}

					case PageOp.AddMember:
					{
						// Initialize state variables.
						MembersToAdd = new Hashtable();

                                                string ifolderLocation = web.GetiFolderLocation (iFolderID);
                                                UriBuilder remoteurl = new UriBuilder(ifolderLocation);
                                                remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                                                web.Url = remoteurl.Uri.ToString();

						iFolderDetails folder = web.GetiFolderDetails(iFolderID);
						string AddToiFolder = folder.Name;
						// Initialize localized fields.
						HeaderTitle.Text = String.Format( GetString( "ADDMEMBERSTOIFOLDER" ), AddToiFolder );
						SubHeaderTitle.Text = String.Format( GetString( "IFOLDERISOWNEDBY" ), FullName );
						OkButton.Text = GetString( "OK" );

						// Hide the back button.
						BackButton.Visible = false;
						
						// Remember the page that we came from.
						ReferringPage = Page.Request.UrlReferrer.ToString();

						// Create an existing member list.
						ExistingMemberList = CreateExistingMemberList();
						break;
					}

					case PageOp.AddAdmin:
					{
						// Initialize state variables.
						MembersToAdd = new Hashtable();

						// Initialize localized fields.
						HeaderTitle.Text = GetString( "ADDADMINS" );
						SubHeaderTitle.Visible = false;
						OkButton.Text = GetString( "ADD" );

						// Hide the back button.
						BackButton.Visible = false;
						
						// Remember the page that we came from.
						ReferringPage = Page.Request.UrlReferrer.ToString();

						// Create an existing member list.
						ExistingMemberList = CreateExistingAdminList();
						break;
					}

					case PageOp.EditSecondaryAdmin:
					{
						// Initialize state variables.
						MembersToAdd = new Hashtable();

						// Initialize localized fields.
						HeaderTitle.Text = GetString( "ADDNEWORSELECTGROUPTOEDIT" );
						SubHeaderTitle.Visible = false;
						OkButton.Text = GetString( "EDIT" );

						// Hide the back button.
						BackButton.Visible = false;
						
						// Remember the page that we came from.
						ReferringPage = Page.Request.UrlReferrer.ToString();

						// Create an existing member list.
						//ExistingMemberList = CreateExistingMemberList();
						AllMembersCheckBox.Enabled = false;

						BackButton.Text = GetString("ADDNEW");
						BackButton.Visible = true;

						break;
					}

					case PageOp.DeleteSecondaryAdmin:
					{
						// Initialize state variables.
						MembersToAdd = new Hashtable();

						// Initialize localized fields.
						HeaderTitle.Text = GetString( "SELECTGROUPTODELETE" );
						SubHeaderTitle.Visible = false;
						OkButton.Text = GetString( "DELETE" );

						// Hide the back button.
						BackButton.Visible = false;
						
						// Remember the page that we came from.
						ReferringPage = Page.Request.UrlReferrer.ToString();

						// Create an existing member list.
						//ExistingMemberList = CreateExistingMemberList();

						break;
					}

					case PageOp.AddSecondaryAdmin:
					{
						// Initialize state variables.
						MembersToAdd = new Hashtable();

						// Initialize localized fields.
						HeaderTitle.Text = GetString( "SELECTMEMBERTOADD" );
						SubHeaderTitle.Visible = false;
						OkButton.Text = GetString( "NEXT" );

						// Hide the back button.
						BackButton.Visible = false;
						
						// Remember the page that we came from.
						ReferringPage = Page.Request.UrlReferrer.ToString();

						// Create an existing member list.
						ExistingMemberList = CreateExistingAdminList();

						AllMembersCheckBox.Enabled = false;
						break;
					}
				}

				// Initialize state variables.
				CurrentUserOffset = 0;
				TotalUsers = 0;
			}
		}

                //private void Page_Unload(object sender, System.EventArgs e)
                //{
                //        web.Url = currentServerURL;
                //}


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
		/// <summary>
                ///  Gets the sharing policy for the user. This is called only during adding members while creating iFolders. No iFolder policy to be 
		///  checked since iFolder is not all created. 
                /// </summary>
		private bool GetSharingPolicy()
		{
			string ownerID = null;
			iFolder ifolder = null;	
			switch( Operation )
			{
				case PageOp.AddAdmin:
				case PageOp.AddSecondaryAdmin:
				case PageOp.EditSecondaryAdmin:
				case PageOp.DeleteSecondaryAdmin:
					return true;
				case PageOp.AddMember:
					ifolder = web.GetiFolder(iFolderID);
					ownerID = ifolder.OwnerID;
					break;
				case PageOp.CreateiFolder:
					ownerID = iFolderOwner;
					break;
			}
                        UserPolicy userPolicy = web.GetUserPolicy(ownerID, null);
                        SystemPolicy systemPolicy = web.GetSystemPolicy();
                        int UserSharingStatus = userPolicy.SharingStatus;
                        int GroupSharingStatus = web.GetUserGroupSharingPolicy(ownerID);
                        int SystemSharingStatus = systemPolicy.SharingStatus;
                        if(( (SystemSharingStatus & (int) Share.EnforcedSharing) == (int) Share.EnforcedSharing))
                        {
                                /// If on system level or user level, enforcement of policy is there, it means the iFolder must not be shared
                                if( (SystemSharingStatus & (int) Share.Sharing) == (int) Share.Sharing)
                                        return true;
                                return false;
                        }
                        else if(( (GroupSharingStatus & (int) Share.EnforcedSharing) == (int) Share.EnforcedSharing))
                        {
                                if( (GroupSharingStatus & (int) Share.Sharing) == (int) Share.Sharing)
                                        return true;
                                return false;
                        }
                        else if(( (UserSharingStatus & (int) Share.EnforcedSharing) == (int) Share.EnforcedSharing))
                        {
                                if( (UserSharingStatus & (int) Share.Sharing) == (int) Share.Sharing)
                                        return true;
                                return false;
                        }
                        else
                        {

                                                if( (UserSharingStatus & (int) Share.Sharing) == (int) Share.Sharing )
                                                {
                                                        /// it means, on User Details page, admin had unchecked the box so sharing is enabled now
                                                        return true;
                                                }
                                                if( (UserSharingStatus & (int) Share.DisableSharing) == (int) Share.DisableSharing)
                                                {
                                                        /// it means, on User Details page, admin had checked the box so sharing is disabled
                                                        return false;
                                                }

                                                /// check for Group level policy as there was no user level or ifolder level policy applied
                                                /// No policy found on iFolder level or User level, no enforcement also, so follow group level
                                                if( (GroupSharingStatus & (int) Share.DisableSharing) == (int) Share.DisableSharing)
                                                {
                                                        return false;
                                                }
                                                if( (GroupSharingStatus & (int) Share.Sharing) == (int) Share.Sharing )
                                                {
                                                        return true;
                                                }

                                                /// check for system level policy as there was no user level or ifolder level policy applied
                                                /// No policy found on iFolder level or User level, no enforcement also, so follow system level
                                                if( (SystemSharingStatus & (int) Share.DisableSharing) == (int) Share.DisableSharing)
                                                {
                                                        return false;
                                                }
                                                if( (SystemSharingStatus & (int) Share.Sharing) == (int) Share.Sharing || SystemSharingStatus == 0)
                                                {
                                                        return true;
                                                }

			}
                        return false;

                }


		#endregion

		#region Protected Methods

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
		/// Event handler that gets called when all of the members in the current view
		/// are to be checked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void AllMembersChecked( Object sender, EventArgs e )
		{
			CheckBox checkBox = sender as CheckBox;
			if( GetSharingPolicy() == true ) 
			{
				switch(Operation)
				{
					case PageOp.AddSecondaryAdmin:
					case PageOp.EditSecondaryAdmin:
						checkBox.Checked = false;
						return;
					default:
						break;
				}
			
				/// if sharing was not disabled for this owner, then only, allow the member box to be checked
				foreach( DataGridItem item in MemberList.Items )
				{
					string userID = item.Cells[ Member_IDCell ].Text;
					if ( userID != "&nbsp;" && !IsExistingMember( userID ) )
					{
						if ( checkBox.Checked )
						{				
							MembersToAdd[ userID ] = 
								new MemberInfo( 
									userID, 
									item.Cells[ Member_UserNameCell ].Text,
									item.Cells[ Member_FullNameCell ].Text );
						}
						else
						{
							// Remove this member from the list.
							MembersToAdd.Remove( userID );
						}
					}
				}
	
				// Rebind the data source with the new data.
				MemberList.DataSource = CreateMemberList();
				MemberList.DataBind();
			}
			else	
			{
				checkBox.Checked = false;
				TopNav.ShowError(GetString("SHARINGPOLICYVIOLATION"));
				OkButton.Enabled = false;	
				return;
			}
		
		}

		/// <summary>
		/// Event handler for the back button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BackButton_Clicked( Object sender, EventArgs e )
		{

			switch( Operation )
			{
				case PageOp.EditSecondaryAdmin:
				 		string op = "ADD";
						Response.Redirect(
							String.Format(
									"AdminRights.aspx?op={0}&secondaryadmin={1}",
									op, SecondaryAdminID));
				break;
	
			}

			string iFolderNameBase64 = Request.Params[ "name" ];
			string iFolderDescBase64 = Request.Params[ "desc" ];

   		        if ( ( iFolderNameBase64 == null ) || ( iFolderNameBase64 == String.Empty ) )
                        {
                              throw new HttpException( ( int )HttpStatusCode.BadRequest, "No ifolder name was specified." );
                        }

   		        if ( iFolderDescBase64 == null )
                        {
				iFolderDescBase64 = String.Empty;
                        }

			Uri uri = new Uri( ReferringPage );
			if ( uri.AbsolutePath.EndsWith( "UserDetails.aspx" ) )
			{
				// Return back to the referring page.
				Page.Response.Redirect( 
					String.Format( 
					"CreateiFolder.aspx?&owner={0}&fn={1}&name={2}&desc={3}&ref={4}", 
					iFolderOwner,
					FullName,
					iFolderNameBase64, 
					iFolderDescBase64,
					ReferringPage ), 
					true );
			}
			else
			{
				// Return back to the referring page.
				Page.Response.Redirect( 
					String.Format( 
					"OwnerSelect.aspx?&name={0}&desc={1}&owner={2}&pg={3}&ref={4}", 
					iFolderNameBase64, 
					iFolderDescBase64,
					iFolderOwner,
					MemberListPage,
					ReferringPage ), 
					true );
			}
		}

		/// <summary>
		/// Event handler for the cancel button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void CancelButton_Clicked( Object sender, EventArgs e )
		{
			// Return back to the referring page.
			string url = web.TrimUrl(ReferringPage);
			Page.Response.Redirect( url, true );
		}

		/// <summary>
		/// Returns the checked state for the specified member.
		/// </summary>
		/// <param name="userID">ID of the user</param>
		/// <returns>True if user is to be added.</returns>
		protected bool GetMemberCheckedState( Object userID )
		{
			return MembersToAdd.ContainsKey( userID ) ? true : IsUserSelected( userID as String );
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
		/// Event handler that gets called when a user in the member list's check
		/// box changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MemberChecked( Object sender, EventArgs e )
		{
			// Get the data grid row for this user.
			CheckBox checkBox = sender as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string userID = item.Cells[ Member_IDCell ].Text;
			if ( userID != "&nbsp;" )
			{
				// User is being added.
				if ( checkBox.Checked )
				{
					if(GetSharingPolicy() == true)
					{
						 		 
						switch(Operation)
						{
							// For Secondary admins, we don't want to allow more than one checked
							case PageOp.AddSecondaryAdmin:
							case PageOp.EditSecondaryAdmin:
								if(MembersToAdd.Count >= 1 )
								{
									checkBox.Checked = false;
									return;
								}
								break;
							default:
								break;
						}
						/// if sharing was not disabled for this owner, then only allow the member box to be checked
						MembersToAdd[ userID ] = 
							new MemberInfo( 
								userID, 
								item.Cells[ Member_UserNameCell ].Text, 
								item.Cells[ Member_FullNameCell ].Text );
						OkButton.Enabled = true;
					}
					else
					{
						checkBox.Checked = false;
        	        	                TopNav.ShowError(GetString("SHARINGPOLICYVIOLATION"));
						// Create should be allowed without sharing. So the create button should be 
						// on and iFolder creation should continue.
						//OkButton.Enabled = false;
                	        	        return;	
					}
				}
				else
				{
					// Remove this member from the list.
					MembersToAdd.Remove( userID );
				}
			}
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

			// Reset the all checked box.
			AllMembersCheckBox.Checked = false;
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

			// Reset the all checked box.
			AllMembersCheckBox.Checked = false;
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

			// Reset the all checked box.
			AllMembersCheckBox.Checked = false;
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

			// Reset the all checked box.
			AllMembersCheckBox.Checked = false;
		}

		/// <summary>
		/// Event handler for the ok button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OkButton_Clicked( Object sender, EventArgs e )
		{
			// See which operation needs to be performed based on the query string.
			switch ( Operation )
			{
				case PageOp.CreateiFolder:
				{
					// Create the iFolder.
					iFolder ifolder = null;
					try
					{
						//iFolderUser owner = web.GetUser( iFolderOwner );

						if( web.GetiFolderLimitPolicyStatus(iFolderOwner) != 1 )
						{
							TopNav.ShowError( GetString( "INVALIDLIMITERROR" ) );
							return;
						}
						ifolder = web.CreateiFolder( iFolderName, iFolderOwner, iFolderDescription );
					}
					catch ( Exception ex )
					{
						// Clear out the member list because it is saved on the session.
						MembersToAdd.Clear();
						MembersToAdd = null;
			                        if( TopNavigation.GetExceptionType(ex) == "PathTooLongException" )
                        			{
			                            OkButton.Enabled = false;
                        			}

						TopNav.ShowError( GetString( "ERRORCANNOTCREATEIFOLDER" ), ex );
						return;
					}

					// Add the selected users to the ifolder.
					foreach( MemberInfo mi in MembersToAdd.Values )
					{
						// Check to see if this user is already a member.
						if ( !IsExistingMember( mi.UserID ) )
						{
							try
							{
								web.AddMember( ifolder.ID, mi.UserID, Rights.ReadOnly );
							}
							catch ( Exception ex )
							{
								// Clear out the member list because it is saved on the session.
								MembersToAdd.Clear();
								MembersToAdd = null;

								string errMsg = String.Format( GetString( "ERRORCANNOTADDMEMBER" ), mi.UserName, iFolderName );
								TopNav.ShowError( errMsg, ex );
								return;
							}
						}
					}

					// Clear out the member list because it is saved on the session.
					MembersToAdd.Clear();
					MembersToAdd = null;

					Page.Response.Redirect( String.Format( "iFolderDetailsPage.aspx?id={0}", ifolder.ID ), true );
					break;
				}

				case PageOp.AddMember:
				{
					// Add the selected users to the ifolder.
					string id = iFolderID;
					foreach( MemberInfo mi in MembersToAdd.Values )
					{
						// Check to see if this user is already a member.
						if ( !IsExistingMember( mi.UserID ) )
						{
							try
							{
								web.AddMember( id, mi.UserID, Rights.ReadOnly );
							}
							catch ( Exception ex )
							{
								// Clear out the member list because it is saved on the session.
								MembersToAdd.Clear();
								MembersToAdd = null;

								string errMsg = String.Format( GetString( "ERRORCANNOTADDMEMBER" ), mi.UserName, iFolderName );
								TopNav.ShowError( errMsg, ex );
								return;
							}
						}
					}

					// Clear out the member list because it is saved on the session.
					MembersToAdd.Clear();
					MembersToAdd = null;
					break;
				}

				case PageOp.AddAdmin:
				{
					// Add the selected users as admins.
					foreach( MemberInfo mi in MembersToAdd.Values )
					{
						// Check to see if this user is already a member.
						if ( !IsExistingMember( mi.UserID ) )
						{
							try
							{
								ConnectMaster();
								web.PreAuthenticate = true;
								web.AddAdministrator( mi.UserID );
							}
							catch( Exception ex )
							{
								DisconnectMaster();
								// Clear out the member list because it is saved on the session.
								MembersToAdd.Clear();
								MembersToAdd = null;

								string errMsg = String.Format( GetString( "ERRORCANNOTADDADMIN" ), mi.UserName );
								TopNav.ShowError( errMsg, ex );
								return;
							}
							DisconnectMaster();
						}
					}

					// Clear out the member list because it is saved on the session.
					MembersToAdd.Clear();
					MembersToAdd = null;
					break;
				}

				case PageOp.AddSecondaryAdmin:
				{
					// Add the selected users as admins.
					foreach( MemberInfo mi in MembersToAdd.Values )
					{
						// Check to see if this user is already a member.
						if ( !IsExistingMember( mi.UserID ) )
						{
							try
							{
								string op = "ADD";
								Response.Redirect(
				                                                String.Format(
                                			                        	"AdminRights.aspx?op={0}&secondaryadmin={1}&groupname={2}",
                                                        				op, mi.UserID, null));

							}
							catch( Exception ex )
							{
								// Clear out the member list because it is saved on the session.
								MembersToAdd.Clear();
								MembersToAdd = null;

								string errMsg = String.Format( GetString( "ERRORCANNOTADDDECONDARYADMIN" ), mi.UserName );
								TopNav.ShowError( errMsg, ex );
								return;
							}
						}
					}

					// Clear out the member list because it is saved on the session.
					MembersToAdd.Clear();
					MembersToAdd = null;
					break;
				}

				case PageOp.EditSecondaryAdmin:
				{
					// Add the selected users as admins.
					foreach( MemberInfo mi in MembersToAdd.Values )
					{
						// Check to see if this user is already a member.
						if ( !IsExistingMember( mi.UserID ) )
						{
							try
							{
								//web.AddAdministrator( mi.UserID );
								string op = "EDIT";
								Response.Redirect(
				                                                String.Format(
                                			                        	"AdminRights.aspx?op={0}&secondaryadmin={1}&groupid={2}",
                                                        				op, SecondaryAdminID, mi.UserID));

							}
							catch( Exception ex )
							{
								// Clear out the member list because it is saved on the session.
								MembersToAdd.Clear();
								MembersToAdd = null;

								string errMsg = String.Format( GetString( "ERRORCANNOTEDITSECONDARYADMIN" ), mi.UserName );
								TopNav.ShowError( errMsg, ex );
								return;
							}
						}
					}

					// Clear out the member list because it is saved on the session.
					MembersToAdd.Clear();
					MembersToAdd = null;
					break;
				}

				case PageOp.DeleteSecondaryAdmin:
				{
					// Add the selected users as admins.
					foreach( MemberInfo mi in MembersToAdd.Values )
					{
						// Check to see if this user is already a member.
						if ( !IsExistingMember( mi.UserID ) )
						{
							try
							{
								// connect to master to modify member's property
								ConnectMaster();
								// pass secondaryadminID and groupID to be deleted
								web.RemoveGroupAdministrator( mi.UserID, SecondaryAdminID );
							}
							catch( Exception ex )
							{
								DisconnectMaster();
								web.Url = currentServerURL;
								// Clear out the member list because it is saved on the session.
								MembersToAdd.Clear();
								MembersToAdd = null;

								string errMsg = String.Format( GetString( "ERRORCANNOTDELETESECONDARYADMIN" ), mi.UserName );
								TopNav.ShowError( errMsg, ex );
								return;
							}
							DisconnectMaster();
							web.Url = currentServerURL;
						}
					}

					// Clear out the member list because it is saved on the session.
					MembersToAdd.Clear();
					MembersToAdd = null;
					break;
				}
			}
			
			string url = web.TrimUrl(ReferringPage);
			Page.Response.Redirect( url, true );
		}

                /// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectMaster ()
		{
			iFolderServer[] list = web.GetServers();

			foreach( iFolderServer server in list )
			{
				if (server.IsMaster)
				{
					UriBuilder remoteurl = new UriBuilder(server.PublicUrl);
					remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
					web.Url = remoteurl.Uri.ToString();
					break;
				}
			}

		}

                /// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DisconnectMaster ()
		{
			web.Url = currentServerURL;
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
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();
			SetMemberPageButtonState();

			// Reset the all checked box.
			AllMembersCheckBox.Checked = false;
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
		/// this.Load += new System.EventHandler( this.Page_Load );
		private void InitializeComponent()
		{
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			MemberSearch.Click += new System.EventHandler( SearchButton_Click );

			MemberList.ItemDataBound += new DataGridItemEventHandler( MemberSelect_MemberItemDataBound );

			MemberListFooter.PageFirstClick += new ImageClickEventHandler( Member_PageFirstButton_Click );
			MemberListFooter.PagePreviousClick += new ImageClickEventHandler( Member_PagePreviousButton_Click );
			MemberListFooter.PageNextClick += new ImageClickEventHandler( Member_PageNextButton_Click );
			MemberListFooter.PageLastClick += new ImageClickEventHandler( Member_PageLastButton_Click );

			this.Load += new System.EventHandler( this.Page_Load );
		}
		#endregion

		#region MemberInfo Class

		/// <summary>
		/// Class used to hold intermediate information when a user
		/// is selected to be added.
		/// </summary>
		[ Serializable() ]
		private class MemberInfo : IComparable
		{
			#region Class Members

			/// <summary>
			/// ID of the user.
			/// </summary>
			public string UserID;

			/// <summary>
			/// Name of the user.
			/// </summary>
			public string UserName;

			/// <summary>
			/// Full name of the user.
			/// </summary>
			public string FullName;

			#endregion

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="userID">ID of the user</param>
			/// <param name="userName">Name of the user</param>
			/// <param name="fullName">Full name of the user</param>
			public MemberInfo( string userID, string userName, string fullName )
			{
				UserID = userID;
				UserName = userName;
				FullName = fullName;
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
				return String.Compare( UserName, ( obj as MemberInfo ).UserName, true );
			}

			#endregion
		}

		#endregion
	}
}
