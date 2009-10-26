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
	/// Summary description for System.
	/// </summary>
	public class SystemInfo : System.Web.UI.Page
	{
		#region Class Members
	
		/// <summary>
                /// iFolder list display types.
                /// </summary>
                private enum ListDisplayType
                {
				// For current Implementation, enum value AllAdmins is not used, can be used in future
                                AllAdmins,
                                GroupAdmins,
                                PrimaryAdmins
                }

		/// <summary>
                /// Group Quota Restriction Method.
                /// </summary>
                private enum QuotaRestriction
                {
				// For current Implementation, enum value AllAdmins is not used, can be used in future
                                UI_Based,
                                Sync_Based
                }
		
		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

        /// <summary>
        /// log header
        /// </summary>
		//private static readonly iFolderWebLogger log = new iFolderWebLogger(
		//	System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;

		/// <summary>
		/// Logged in admin system rights instance
		/// </summary>
		UserSystemAdminRights uRights;
		
		/// <summary>
		/// Logged in user system rights value
		/// </summary>
		int sysAccessPolicy = 0;

		/// <summary>
		/// iFolder system name control.
		/// </summary>
		protected TextBox Name;

		/// <summary>
		/// iFolder system description control.
		/// </summary>
		protected HtmlTextArea Description;
		
		/// <summary>
                /// SSL dropdown list .
                /// </summary>
                protected DropDownList SSLValue;

		/// <summary>
		/// iFolder system cancel button control.
		/// </summary>
		protected Button CancelButton;

		/// <summary>
		/// iFolder system save button control.
		/// </summary>
		protected Button SaveButton;

		/// <summary>
		/// Number of users control.
		/// </summary>
		protected Literal NumberOfUsers;

		/// <summary>
		/// Number of ifolders control.
		/// </summary>
		protected Literal NumberOfiFolders;

		/// <summary>
		/// Full Name Display Setting  
		/// </summary>
		protected RadioButtonList FullNameSetting;

		/// <summary>
		/// Group Quota Restriction Method  
		/// </summary>
		protected RadioButtonList GroupQuotaRestriction;

		/// <summary>
		/// System policy control.
		/// </summary>
		protected Policy Policy;
	
		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected LinkButton PrimaryAdminsLink;

		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected LinkButton GroupAdminsLink;
		
		/// <summary>
		/// All admins checkbox control.
		/// </summary>
		protected CheckBox AllAdminsCheckBox;

		/// <summary>
		/// GroupName Literal
		/// </summary>
		protected Literal GroupName;

		/// <summary>
		/// Admin list control.
		/// </summary>
		protected DataGrid AdminList;

		/// <summary>
		/// Admin list footer control.
		/// </summary>
		protected ListFooter AdminListFooter;

		/// <summary>
		/// Delete admin button control.
		/// </summary>
		protected Button DeleteButton;

		/// <summary>
		/// Edit admin rights button control.
		/// </summary>
		protected Button EditButton;
		
		/// <summary>
                /// iFolder list view tab controls.
                /// </summary>
                protected HtmlGenericControl CurrentTab;


		/// <summary>
		/// Add admin button control.
		/// </summary>
		protected Button AddButton;

		/// <summary>
		/// Add admin button control.
		/// </summary>
		//protected Button AddSecButton;

		/// <summary>
		/// Reprovision State admin button control.
		/// </summary>
		protected Button ReprovisionStatusButton;

		/// <summary>
		/// Current server URL
		/// </summary>
		protected string currentServerURL;

		/// <summary>
		/// used by aspx page to see if SSLOption combo box changes
		/// </summary>
		public string SSLOptionChanged ;

		/// <summary>
		/// used by aspx page to see if SSLOption combo box changes
		/// </summary>
		protected CheckBox GroupSegregated ;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the members that are checked in the list.
		/// </summary>
		private Hashtable CheckedMembers
		{
			get { return ViewState[ "CheckedMembers" ] as Hashtable; }
			set { ViewState[ "CheckedMembers" ] = value; }
		}

		/// <summary>
		/// Gets or sets the current admin offset.
		/// </summary>
		private int CurrentAdminOffset
		{
			get { return ( int )ViewState[ "CurrentAdminOffset" ]; }
			set { ViewState[ "CurrentAdminOffset" ] = value; }
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
		/// Gets or sets the total number of admins contained in
		/// the last search.
		/// </summary>
		private int TotalAdmins
		{
			get { return ( int )ViewState[ "TotalAdmins" ]; }
			set { ViewState[ "TotalAdmins" ] = value; }
		}

        /// <summary>
        /// Gets or sets if SSL option was changed in current page
        /// </summary>
		private string IsSSLOptionChanged
		{
			get { return ( string )ViewState[ "SSLOptionChanged" ]; }
			set { ViewState[ "SSLOptionChanged" ] = value; }
		}
		
		/// <summary>
		/// Gets or sets the active ifolder tab.
		/// </summary>
		private ListDisplayType ActiveAdminTab
		{
			get { return ( ListDisplayType )ViewState[ "ActiveAdminTab" ]; }
			set { ViewState[ "ActiveAdminTab" ] = value; }
		}


		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			TopNav.AddBreadCrumb( GetString( "SYSTEM" ), null );
			// Pass this page information to create the help link
			TopNav.AddHelpLink(GetString("SYSTEM"));
		}

		/// <summary>
		/// Gets the list view of ifolder administrators.
		/// </summary>
		/// <returns></returns>
		private DataView CreateDataSource()
		{
			DataTable dt = new DataTable();
			DataRow dr;
			iFolderUserSet adminList = null;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "GroupListVisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "NAField", typeof( string ) ) );

			
			switch(ActiveAdminTab)
			{
				case ListDisplayType.GroupAdmins:
								adminList = web.GetAdministrators( CurrentAdminOffset, AdminList.PageSize, (int)ListDisplayType.GroupAdmins );
								AllAdminsCheckBox.Enabled = false;
								AllAdminsCheckBox.Checked = false;
								break;
				case ListDisplayType.PrimaryAdmins:
							default:
								adminList = web.GetAdministrators( CurrentAdminOffset, AdminList.PageSize, (int)ListDisplayType.PrimaryAdmins );
								AllAdminsCheckBox.Enabled = true;
								break;
			}

			foreach( iFolderUser admin in adminList.Items )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = admin.ID;
				dr[ 2 ] = admin.UserName;
				dr[ 3 ] = admin.FullName;
				dr[ 4 ] = ! (AllAdminsCheckBox.Enabled); 	//if it system admin tab, then do not show the group list	
				dr[ 5 ] = "";

				dt.Rows.Add( dr );
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < AdminList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = String.Empty;
				dr[ 3 ] = String.Empty;
				dr[ 4 ] = false;
				dr[ 5 ] = String.Empty;
				//dr[ 6 ] = false;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotalAdmins = adminList.Total;

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// GetiFolderAdminList
		/// </summary>
		private void GetiFolderAdminList()
		{
			AdminList.DataSource = CreateDataSource();
			AdminList.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Gets the displayable ifolder system information.
		/// </summary>
		private void GetSystemInformation()
		{
			iFolderSystem system = web.GetSystem();
			Name.Text = system.Name;
			Description.Value = system.Description;
			if(system.UsersFullNameDisplay == "FirstNameLastName")
			{
				FullNameSetting.SelectedIndex = 0;
			}
			else
			{
				FullNameSetting.SelectedIndex = 1;
			}
			if(system.GroupSegregated == "yes")
			{
				GroupSegregated.Checked = true;
			}

			if( system.GroupQuotaRestrictionMethod == (int)QuotaRestriction.UI_Based )
			{
				GroupQuotaRestriction.SelectedIndex = 0;
			}
			else if( system.GroupQuotaRestrictionMethod == (int)QuotaRestriction.Sync_Based )
			{
				GroupQuotaRestriction.SelectedIndex = 1;
			}

			iFolderUserSet users = web.GetUsers( 0, 1 );
			NumberOfUsers.Text = users.Total.ToString();

			iFolderSet ifolders = web.GetiFolders( iFolderType.All, 0, 1 );
			NumberOfiFolders.Text = ifolders.Total.ToString();
			
			web.Url = currentServerURL;
			iFolderServer[] list = web.GetServers();
			foreach( iFolderServer server in list )
			{
				if (server.IsLocal) 
				{
					UriBuilder remoteurl = new UriBuilder(server.PublicUrl);
                                        remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                                        web.Url = remoteurl.Uri.ToString();

					string [] SSlOptionsStr = new string [3];
					SSlOptionsStr[0] = GetString("SSLSTRING");
					SSlOptionsStr[1] = GetString("NONSSLSTRING");
					SSlOptionsStr[2] = GetString("BOTHSTRING");
					string SelectedString = "";
					string simiassslstatus = web.GetSimiasRequiresSSLStatus();
					UriBuilder urlforssl = new UriBuilder(web.Url);
					if(urlforssl.Scheme == Uri.UriSchemeHttps)
					{
						if(simiassslstatus == "no")
						{
							SelectedString = GetString("BOTHSTRING");
						}
						else
						{
							SelectedString = GetString("SSLSTRING");
						}
					}
					else
					{
						SelectedString = GetString("NONSSLSTRING");
					}
					SSLValue.DataSource = SSlOptionsStr;
					SSLValue.DataBind();
					SSLValue.SelectedValue = SelectedString;
					break;
				}
			}	
			SSLValue.Enabled = false;
			// SSLOption was server specific, now connect back to master 
			ConnectToMaster();
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
		/// Event handler that gets called if a policy error occurs.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void OnPolicyError( object source, PolicyErrorArgs e )
		{
			TopNav.ShowError( e.Message, e.Exception );
		}

		/// <summary>
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectToMaster ( )
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

			try 
			{
				iFolder domain = web.GetiFolder( web.GetSystem().ID );
				SuperAdminID = domain.OwnerID;
			}
			catch (Exception e )
			{
			        	web.Url = currentServerURL;
			        	throw new Exception ("Unable to connect to master server"+e);
			}
		}
		/// <summary>
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load( object sender, System.EventArgs e )
		{
			// connection
			web = Session[ "Connection" ] as iFolderAdmin;
			currentServerURL = web.Url;

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			string userID = Session[ "UserID" ] as String;
			if(userID != null)
				sysAccessPolicy = web.GetUserSystemRights(userID, null);
			else
				sysAccessPolicy = 0; 
			uRights = new UserSystemAdminRights(sysAccessPolicy);
			if(uRights.SystemPolicyManagementAllowed == false)
				Page.Response.Redirect(String.Format("Error.aspx?ex={0}&Msg={1}",GetString( "ACCESSDENIED" ), GetString( "ACCESSDENIEDERROR" )));

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				DeleteButton.Text = GetString( "DELETE" );
				EditButton.Text = GetString( "EDIT" );
				AddButton.Text = GetString( "ADD" );
				//AddSecButton.Text = GetString( "ADD" ) + " " + GetString( "SECONDARY" );
				ReprovisionStatusButton.Text = GetString( "REPROVISIONBUTTON" );
				SaveButton.Text = GetString( "SAVE" );
				CancelButton.Text = GetString( "CANCEL" );
				GroupSegregated.Text = GetString("CREATESEGREGATEDGROUPS");
				PrimaryAdminsLink.Text = GetString( "PRIMARY" );
				GroupAdminsLink.Text = GetString( "SECONDARY" );

				FullNameSetting.Items[ 0 ].Text = "(" + GetString("FIRSTNAME") + ", " + GetString("LASTNAME") + ")";
				FullNameSetting.Items[ 1 ].Text = "(" + GetString("LASTNAME") + ", " + GetString("FIRSTNAME") + ")";
				GroupQuotaRestriction.Items[ 0 ].Text = GetString("UIBASED") ;
				GroupQuotaRestriction.Items[ 1 ].Text = GetString("SYNCBASED");

				// Initialize state variables.
				CurrentAdminOffset = 0;
				TotalAdmins = 0;
				AllAdminsCheckBox.Checked = false;
				CheckedMembers = new Hashtable();
				//select the active admin tab
				ActiveAdminTab = ListDisplayType.PrimaryAdmins;
				EditButton.Visible = false;
				//AddSecButton.Visible = false;
			}
			SetActiveAdminListTab( ActiveAdminTab );
		}

        /// <summary>
        /// Unload the current page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Page_Unload(object sender, System.EventArgs e)
		{
		        web.Url = currentServerURL;
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void Page_PreRender( object source, EventArgs e )
		{
			ConnectToMaster ();
			
			// Show the ifolder system information.
			GetSystemInformation();

			// Get the list of ifolder admins.
			GetiFolderAdminList();

			// Get the policy information.
			Policy.GetSystemPolicies();
			// Build the bread crumb list.
			BuildBreadCrumbList();
		}

		/// <summary>
		/// Sets the page button state of the admin list.
		/// </summary>
		private void SetPageButtonState()
		{
			AdminListFooter.SetPageButtonState( 
				AdminList,
				CurrentAdminOffset, 
				TotalAdmins, 
				GetString( "ADMINISTRATORS" ),
				GetString( "ADMINISTRATOR" ) );
		}

		/// <summary>
		/// Sets the active ifolder list display tab.
		/// </summary>
		/// <param name="activeTab"></param>
		/// <returns>The active list tab.</returns>
		private void SetActiveAdminListTab( ListDisplayType activeTab )
		{
			ActiveAdminTab = activeTab;
			CurrentTab.ID = activeTab.ToString();
			
			 switch(ActiveAdminTab)
                        {
                                case ListDisplayType.GroupAdmins:
					GroupName.Text = GetString("GROUPNAME");
					GroupName.Visible = true;
                                        break;
                                case ListDisplayType.PrimaryAdmins:
                                default:
					GroupName.Visible = false;
                                        break;
                        }

		}



		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets whether the admin is checked.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected bool GetAdminCheckedState( object id )
		{
			return CheckedMembers.ContainsKey( id ) ? true : false;
		}

		/// <summary>
		/// Gets whether the checkbox should be enabled for this admin user.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected bool GetAdminEnabledState( object id )
		{
			return !IsSuperAdmin( id as string );
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString( string key )
		{
			return rm.GetString( key );
		}

		/// <summary>
		/// Event handler that gets called when the all ifolders tab is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void PrimaryAdmins_Clicked( Object sender, EventArgs e )
		{
			SetActiveAdminListTab( ListDisplayType.PrimaryAdmins );
			GetiFolderAdminList();
			EditButton.Visible = false;
			//AddButton.Enabled = true;
			//AddSecButton.Enabled = false;
			//CreateButton.Enabled = true;
			//GetiFolders();
		}

		/// <summary>
		/// Event handler that gets called when the all ifolders tab is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void GroupAdmins_Clicked( Object sender, EventArgs e )
		{
			SetActiveAdminListTab( ListDisplayType.GroupAdmins );
			GetiFolderAdminList();
			EditButton.Visible = true;
			//EditButton.Enabled = false;
			//AddButton.Enabled = false;
			//AddSecButton.Visible = AddSecButton.Enabled =true;
			EditButton.Enabled = (CheckedMembers.Count == 1 );
			DeleteButton.Enabled = (CheckedMembers.Count == 1 );

			//CreateButton.Enabled = true;
			//GetiFolders();
		}
		
		/// <summary>
		/// Event handler that gets called when the report location selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFullNameSetting_Changed( object sender, EventArgs e )
		{
		}

		/// <summary>
		/// Event handler that gets called when the add admin button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnReprovisionStatusButton_Click( object source, EventArgs e )
		{
			Response.Redirect( "userMove.aspx", true );
		}

		/// <summary>
		/// Event handler that gets called when the edit admin button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnEditButton_Click( object source, EventArgs e )
		{
		//	Response.Redirect( "MemberSelect.aspx?op=addadmin", true );
			
			string op = "editsecondaryadmin";
			foreach( string memberID in CheckedMembers.Keys )
			{
				Response.Redirect(
					String.Format(
						"MemberSelect.aspx?op={0}&secondaryadminid={1}",
						op, memberID));
				// edit secondary admin can be done only for one at a time
				break;	
			}			

		}

		/// <summary>
		/// Event handler that gets called when the add admin button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAddButton_Click( object source, EventArgs e )
		{
			switch(ActiveAdminTab)
			{
				case ListDisplayType.GroupAdmins:
					Response.Redirect( "MemberSelect.aspx?op=addsecondaryadmin", true );
					break;
				case ListDisplayType.PrimaryAdmins:
				default:
					Response.Redirect( "MemberSelect.aspx?op=addadmin", true );
					break;
			}
		}

		/// <summary>
		/// Event handler that gets called when the add admin button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		//protected void OnAddSecButton_Click( object source, EventArgs e )
		//{
			//switch(ActiveAdminTab)
			//{
			//	case ListDisplayType.GroupAdmins:
		//			Response.Redirect( "MemberSelect.aspx?op=addsecondaryadmin", true );
			//		break;
			//	case ListDisplayType.AllAdmins:
			//	default:
			//		Response.Redirect( "MemberSelect.aspx?op=addadmin", true );
			//		break;
			//}
		//}

		/// <summary>
		/// Event handler that gets called when the admin checkbox is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAdminChecked( object source, EventArgs e )
		{
			// Get the data grid row for this member.
			CheckBox checkBox = source as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string memberID = item.Cells[ 0 ].Text;
			if ( memberID != "&nbsp;" )
			{
				// Member is being added.
				if ( checkBox.Checked )
				{
					switch(ActiveAdminTab)
					{
						case ListDisplayType.GroupAdmins:
								//foreach( DataGridItem itemcb in AdminList.Items )
								//{
									//itemcb.Cells[ 1 ].Enabled = false;
								//}
								//checkBox.Enabled = true;
								//checkBox.Checked = true;
								CheckedMembers[ memberID ] = item.Cells[ 4 ].Text;

								EditButton.Visible = true;
								EditButton.Enabled = (CheckedMembers.Count == 1 );
								DeleteButton.Enabled = (CheckedMembers.Count == 1 );
								break;
						case ListDisplayType.PrimaryAdmins:
						default:
								CheckedMembers[ memberID ] = item.Cells[ 4 ].Text;
								EditButton.Visible = false;
								DeleteButton.Enabled = ( CheckedMembers.Count > 0 );
								break;
					}
					//CheckedMembers[ memberID ] = item.Cells[ 4 ].Text;
				}
				else
				{
					// Remove this member from the list.
					switch(ActiveAdminTab)
					{
						case ListDisplayType.GroupAdmins:
							//foreach( DataGridItem itemcb in AdminList.Items )
							//{
							//	itemcb.Cells[ 1 ].Enabled = true;
							//}
							CheckedMembers.Remove( memberID );
							DeleteButton.Enabled = (CheckedMembers.Count == 1 );
							EditButton.Visible = true;
							EditButton.Enabled = (CheckedMembers.Count == 1 );
							break;
						case ListDisplayType.PrimaryAdmins:
						default:
							// See if there are any checked members.
							CheckedMembers.Remove( memberID );
							DeleteButton.Enabled = ( CheckedMembers.Count > 0 );
							EditButton.Visible = false;
							break;
							
					}
					//CheckedMembers.Remove( memberID );
				}
			}

			// See if there are any checked members.
			//DeleteButton.Enabled = ( CheckedMembers.Count > 0 );
		}

		/// <summary>
		/// Event handler that gets called when the all admins checkbox is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAllAdminsChecked( object source, EventArgs e )
		{
			CheckBox checkBox = source as CheckBox;
			foreach( DataGridItem item in AdminList.Items )
			{
				// In order to be checked, the row must not be empty.
				string memberID = item.Cells[ 0 ].Text;
				if ( memberID != "&nbsp;" )
				{
					if ( !IsSuperAdmin( memberID ) )
					{
						if ( checkBox.Checked )
						{
							switch(ActiveAdminTab)
							{
								case ListDisplayType.GroupAdmins:
									DeleteButton.Enabled = (CheckedMembers.Count == 1 );
									EditButton.Visible = true;
									EditButton.Enabled = (CheckedMembers.Count == 1 );
									break;
								case ListDisplayType.PrimaryAdmins:
								default:
									CheckedMembers[ memberID ] = item.Cells[ 4 ].Text;
									DeleteButton.Enabled = ( CheckedMembers.Count > 0 );
									EditButton.Visible = false;
									break;
							}

						}
						else
						{
							switch(ActiveAdminTab)
							{
								case ListDisplayType.GroupAdmins:
									DeleteButton.Enabled = (CheckedMembers.Count == 1 );
									EditButton.Visible = true;
									EditButton.Enabled = (CheckedMembers.Count == 1 );
									break;
								case ListDisplayType.PrimaryAdmins:
								default:
									// Remove this member from the list.
									CheckedMembers.Remove( memberID );
									DeleteButton.Enabled = ( CheckedMembers.Count > 0 );
									EditButton.Visible = false;
									break;
							}
						}
					}
				}
			}

			// See if there are any checked members.
			DeleteButton.Enabled = ( CheckedMembers.Count > 0 );

			// Rebind the data source with the new data.
			GetiFolderAdminList();
		}

		/// <summary>
		/// Event that gets called when the SyncNow button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnSyncNowButton_Click( object source, EventArgs e )
		{
		        web.IdentitySyncNow ();
		}

		/// <summary>
		/// Event that gets called when the cancel button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnCancelButton_Click( object source, EventArgs e )
		{
			// Refresh system information.
			GetSystemInformation();
			SaveButton.Enabled=CancelButton.Enabled = false;
		}

		/// <summary>
		/// Event handler that gets called when the delete admin button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDeleteButton_Click( object source, EventArgs e )
		{
			switch(ActiveAdminTab)
			{
				case ListDisplayType.GroupAdmins:

		                       	string op = "deletesecondaryadmin";
					foreach( string memberID in CheckedMembers.Keys )
					{
						Response.Redirect(
							String.Format(
							"MemberSelect.aspx?op={0}&secondaryadminid={1}",
							op, memberID));
		
							// delete secondary admin can be done only for one at a time
						//return;
						return;
					}
					break;

				case ListDisplayType.PrimaryAdmins:
				default:
					foreach( string memberID in CheckedMembers.Keys )
					{
						try
						{
							iFolderUser user = web.GetUser(memberID);
							if(user.MemberRights == Rights.Admin)
							{
								web.RemoveAdministrator( memberID );
							}
							
						}
						catch ( Exception ex )
						{
							string name = CheckedMembers[ memberID ] as String;
							TopNav.ShowError( String.Format( GetString( "ERRORCANNOTREMOVEADMIN" ), name ), ex );
							return;
						}
					}
					// Clear the checked members.
					CheckedMembers.Clear();
					AllAdminsCheckBox.Checked = false;
	
					// Disable the action buttons.
					DeleteButton.Enabled = false;
		
					// Rebind the data source with the new data.
					GetiFolderAdminList();
					break;
			}

			// Clear the checked members.
			//CheckedMembers.Clear();
			//AllAdminsCheckBox.Checked = false;

			// Disable the action buttons.
			//DeleteButton.Enabled = false;

			// Rebind the data source with the new data.
			//GetiFolderAdminList();
		}

		/// <summary>
		/// Event that gets called when the save button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnSaveButton_Click( object source, EventArgs e )
		{
			iFolderSystem system = new iFolderSystem();
			system.Name = Name.Text;
			system.Description = Description.Value;
			if(FullNameSetting.SelectedIndex == 0)
				system.UsersFullNameDisplay = "FirstNameLastName";
			else
				system.UsersFullNameDisplay = "LastNameFirstName";
			if(GroupQuotaRestriction.SelectedIndex == 0)
				system.GroupQuotaRestrictionMethod = (int) QuotaRestriction.UI_Based;
			else if( GroupQuotaRestriction.SelectedIndex == 1 )
				system.GroupQuotaRestrictionMethod = (int) QuotaRestriction.Sync_Based;

			if(GroupSegregated.Checked == true)
				system.GroupSegregated = "yes";
			else
				system.GroupSegregated = "no";
				
			ConnectToMaster();
			web.SetSystem( system );

			// To Set SSL option connect to local server, not master
			web.Url = currentServerURL;
			iFolderServer[] list = web.GetServers();
			foreach( iFolderServer server in list )
			{
				if (server.IsLocal)  
				{
					UriBuilder remoteurl = new UriBuilder(server.PublicUrl);
		                        remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
 		                        web.Url = remoteurl.Uri.ToString();
				
					int SelectedIndex = 0;
					SaveButton.Enabled=CancelButton.Enabled = false;
					if( IsSSLOptionChanged  == "true")
					{
						SelectedIndex = SSLValue.SelectedIndex;
						string SelectedValue = "both";
						bool SSLUpdated = false;
						switch(SelectedIndex)
						{
							case 0 : // SSL
								 SelectedValue = "ssl";
								 break;
							case 1 : // NONSSL
								 SelectedValue = "nonssl";
								 break;
							case 2 : // BOTH
							default:
								SelectedValue = "both";
								break;
						}
						SSLUpdated = web.SetSimiasSSLStatus(SelectedValue);
						IsSSLOptionChanged = "false";	

						// Now connect back to master
						if(! server.IsMaster)
						{
							ConnectToMaster();
						}

						if(SSLUpdated == true)
						{
							// restart server message
							TopNav.ShowInfo(GetString("RESTARTSERVER"));
							return;
						}
						else
						{
							// updation of ssl failed message
							TopNav.ShowInfo(GetString("UNKNOWNERROR"));
							return;
						}
					}
				}
			}

			GetSystemInformation();
		}

		/// <summary>
		/// Show the drop-down list to select provisioning method for a particular user.
		/// </summary>
		/// <returns></returns>
		protected string[] ShowGroupList( Object UserID )
		{
			string userID = UserID as string ;	
			string [] MonitoredGroupNames = web.GetMonitoredGroupNames(userID);
			if( ! userID.Equals (String.Empty) && MonitoredGroupNames != null && MonitoredGroupNames.Length > 0 )
			{
				Array.Sort(MonitoredGroupNames);
				return MonitoredGroupNames;
			}
			else
			{
				string [] GroupListArray = new string [1];
				GroupListArray[0] = GetString("NONE");
				return GroupListArray;
			}
		}

		/// <summary>
		/// Event that first when the PageFirstButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageFirstButton_Click( object source, ImageClickEventArgs e)
		{
			// Set to get the first admins.
			CurrentAdminOffset = 0;
			GetiFolderAdminList();
		}

		/// <summary>
		/// Event that first when the PagePreviousButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PagePreviousButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentAdminOffset -= AdminList.PageSize;
			if ( CurrentAdminOffset < 0 )
			{
				CurrentAdminOffset = 0;
			}

			GetiFolderAdminList();
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentAdminOffset += AdminList.PageSize;
			GetiFolderAdminList();
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentAdminOffset = ( ( TotalAdmins - 1 ) / AdminList.PageSize ) * AdminList.PageSize;
			GetiFolderAdminList();
		}
		
        /// <summary>
        /// Enable the save/cancel buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		protected void EnableSaveButtons(Object sender, EventArgs e)
		{
			//IsSSLOptionChanged="true";
			SaveButton.Enabled = true;	
			CancelButton.Enabled = true;
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
        /// Prerender the policy apply button which is on policy.ascx for click action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Policy_PolicyApplyButton_PreRender(object sender, EventArgs e)
                {
                        this.Policy.PolicyApplyButton.Attributes["onclick"] = "return alertuser();";
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
			
			Policy.PolicyError += new Policy.PolicyErrorHandler( OnPolicyError );
			AdminListFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			AdminListFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			AdminListFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			AdminListFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			this.Policy.PolicyApplyButton.PreRender += new EventHandler(Policy_PolicyApplyButton_PreRender);			

			this.Load += new System.EventHandler(this.Page_Load);
			this.Unload += new System.EventHandler (this.Page_Unload);
		}
		#endregion
	}
}
