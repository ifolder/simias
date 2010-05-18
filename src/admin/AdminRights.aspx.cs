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
*                 $Author: Anil Kumar (kuanil@novell.com)
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
using System.Globalization;
using System.Resources;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Threading;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for Reports.
	/// </summary>
	public class AdminRights : System.Web.UI.Page
	{
		#region Class Members

		public enum GroupAdminPreferencesEnum
                {
                        iFolderLimitAllowed = 1,
                        DiskQuotaAllowed = 2,
                        FileSizeAllowed = 4,
                        SyncIntervalAllowed = 8,
                        AddToExcludePolicyAllowed = 16,
                        ChangeSharingAllowed = 32,
                        ChangeEnforcedSharingAllowed = 64,
                        RevokeSharingAllowed = 128,
                        ChangeEncryptionAllowed = 256,
                        ChangeEnforcedEncryptionAllowed = 512,
                        ProvisioningAllowed = 1024,
                        ReProvisioningAllowed = 2048,
                        EnableDisableUserAllowed = 4096,
			OwnOrphaniFolderAllowed = 8192,
			EnableDisableiFolderAllowed = 16384,
			ModifyMemberRightAllowed = 32768,
			DeleteiFolderAllowed = 65536,
                };


		/// <summary>
		/// Log
		/// </summary>
		//private static readonly iFolderWebLogger log = new iFolderWebLogger(
		//	System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

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
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected CheckBoxList NoOfiFoldersList;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected CheckBoxList DiskQuotaRightsList;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected CheckBoxList FileSizeRightsList;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected CheckBoxList SyncIntervalRightsList;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected CheckBoxList FileListRightsList;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected CheckBoxList SharingRightsList;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected CheckBoxList EncryptionRightsList;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected CheckBoxList ProvisioningRightsList;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected CheckBoxList iFolderRightsList;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected Label AggregateDiskQuotaLabel;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected Label StorageUnitLabel;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected TextBox AggregateDiskQuotaText;

		/// <summary>
		/// Reporting day of week control.
		/// </summary>
		//protected DropDownList DayOfWeekList;

		/// <summary>
		/// Full Name label control.
		/// </summary>
		protected Label AdminFullNameLabel;

		/// <summary>
		/// Report format control.
		/// </summary>
		protected DropDownList GroupList;

		/// <summary>
                /// Current server URL
                /// </summary>
                protected string currentServerURL;

		/// <summary>
		/// Save button control.
		/// </summary>
		protected Button SaveAdminRights;

		/// <summary>
		/// Cancel button control.
		/// </summary>
		protected Button CancelAdminRights;


		#endregion

		#region Properties

               /// <summary>
                /// Gets or sets the existing members list.
                /// </summary>
                private Hashtable GroupHashTable
                {
                        get { return ViewState[ "GroupHashTable" ] as Hashtable; }
                        set { ViewState[ "GroupHashTable" ] = value; }
                }


		/// <summary>
		/// Gets the starting member list page of the previous owner select page.
		/// </summary>
		private string SecondaryAdmin
		{
			get
			{
				string param = Request.Params[ "secondaryadmin" ];
				return ( ( param == null ) || ( param == String.Empty ) ) ? "" : param;
			}
		}

		/// <summary>
		/// Gets the starting member list page of the previous owner select page.
		/// </summary>
		private string GroupID 
		{
			get
			{
				string param = Request.Params[ "groupid" ];
				return ( ( param == null ) || ( param == String.Empty ) ) ? "" : param;
			}
		}

		/// <summary>
		/// Gets the starting member list page of the previous owner select page.
		/// </summary>
		private string op 
		{
			get
			{
				string param = Request.Params[ "op" ];
				return ( ( param == null ) || ( param == String.Empty ) ) ? "" : param;
			}
		}


		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			Control body = FindControl( "system" );
                        TopNav.AddBreadCrumb( GetString( "SYSTEM" ), "SystemInfo.aspx" );
                        TopNav.AddBreadCrumb( GetString( "SECONDARYADMIN" ), null );
                        if ( body != null )
                        {
                                body.ID = "system";
                        }
                        TopNav.SetActivePageTab( TopNavigation.PageTabs.System );

			// Pass this page information to create the help link
			TopNav.AddHelpLink(GetString("SYSTEM"));
		}

		/// <summary>
		/// Initializes the time of day values in the dropdown list.
		/// </summary>
		public void InitializeGroupList()
		{
			string Operation = op;
			//string [] GroupListStr ;//= null;
			GroupHashTable = new Hashtable();

			// Call web-service to get the groups for which this user is secondary admin
			
			if(Operation == "ADD")
			{
					
					// This will search only groups from whole domain
	                		iFolderUserSet userList = web.GetUsersBySearch(
        	       	                	SearchProperty.GroupOnly,
	                                	SearchOperation.BeginsWith,
                                		"*",
  		       	                	0,
        	                        	0 );
				//string [] GroupStr = new string[ userList.Total ]; 
				string [] MonitoredGroupsIDs = web.GetMonitoredGroups(SecondaryAdmin);
				
				GroupList.Items.Add(GetString("NONE"));
				foreach(iFolderUser user in userList.Items)
				{
					if( (MonitoredGroupsIDs == null || MonitoredGroupsIDs.Length == 0) || Array.IndexOf(MonitoredGroupsIDs, user.ID) < 0)
					{
						// Add only those groups which are not monitored currently by this secondary admin
						GroupHashTable[ user.FullName ] = user.ID;
						GroupList.Items.Add(user.FullName);
					}
				}
				GroupList.SelectedIndex = 0;
				
				
			}
			else if(Operation == "EDIT")
			{
				// get all groups for which this member is an admin
				
				iFolderUser user = web.GetUser(GroupID);
				if(GroupID != "")
				{
					GroupHashTable[ user.FullName ] = user.ID;
					GroupList.Items.Add(user.FullName);
					GroupList.SelectedIndex = 0;
					GroupList.Enabled = false;
						
					long limit = web.GetAggregateDiskQuota(GroupID);
					if( limit == -1 )
						AggregateDiskQuotaText.Text = "";
					else
						AggregateDiskQuotaText.Text = Utils.ConvertToMBString( limit, false, rm );
					ShowSecondaryAdminRights(GroupID);
				}

				
				//AggregateDiskQuotaText.Text = "100";
				SaveAdminRights.Enabled = true;	
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
			if(uRights.SecondaryAdminAddAllowed == false)
				Page.Response.Redirect(String.Format("Error.aspx?ex={0}&Msg={1}",GetString( "ACCESSDENIED" ), GetString( "ACCESSDENIEDERROR" )));

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				
				AggregateDiskQuotaLabel.Text = "Set the Aggregate Disk Quota Limit For Entire Group: ";
				StorageUnitLabel.Text = GetString("MB");	

				NoOfiFoldersList.Items[ 0 ].Text = GetString( "ALLOW" );
				NoOfiFoldersList.Items[ 0 ].Selected = true;

				DiskQuotaRightsList.Items[ 0 ].Text = GetString( "ALLOW" );
				DiskQuotaRightsList.Items[ 0 ].Selected = true;

				FileSizeRightsList.Items[ 0 ].Text = GetString( "ALLOW" );
				FileSizeRightsList.Items[ 0 ].Selected = true;

				SyncIntervalRightsList.Items[ 0 ].Text = GetString( "ALLOW" );
				SyncIntervalRightsList.Items[ 0 ].Selected = true;

				FileListRightsList.Items[ 0 ].Text = GetString( "ALLOW" );
				FileListRightsList.Items[ 0 ].Selected = true;

				SharingRightsList.Items[ 0 ].Text = GetString( "ALLOWMODIFYSHARINGPOLICY" );
				SharingRightsList.Items[ 0 ].Selected = true;

				EncryptionRightsList.Items[ 0 ].Text = GetString( "ALLOWMODIFYENCRYPTIONPOLICY" );
				EncryptionRightsList.Items[ 0 ].Selected = true;

				ProvisioningRightsList.Items[ 0 ].Text = GetString( "ALLOWUSERPROVISIONING" );
				ProvisioningRightsList.Items[ 1 ].Text = GetString( "ALLOWUSERENABLING" );
				ProvisioningRightsList.Items[ 0 ].Selected = true;
				ProvisioningRightsList.Items[ 1 ].Selected = true;

				iFolderRightsList.Items[ 0 ].Text = GetString( "ALLOWORPHANIFOLDEROWNERSHIP" );
				iFolderRightsList.Items[ 1 ].Text = GetString( "ALLOWIFOLDERENABLING" );
				iFolderRightsList.Items[ 2 ].Text = GetString( "ALLOWSHAREDMEMBERRIGHTS" );
				iFolderRightsList.Items[ 3 ].Text = GetString( "ALLOWDELETEIFOLDERRIGHTS" );
				iFolderRightsList.Items[ 0 ].Selected = true;
				iFolderRightsList.Items[ 1 ].Selected = true;
				iFolderRightsList.Items[ 2 ].Selected = true;
				iFolderRightsList.Items[ 3 ].Selected = true;

				iFolderUser user = web.GetUser( SecondaryAdmin );
				string LebelDisplay;
				if(GetString(op) == "Add")
					LebelDisplay = GetString("ASSIGNADMINRIGHTS");
				else
					LebelDisplay = GetString(op) + " " + GetString("ADMINRIGHTSFOR"); 
				AdminFullNameLabel.Text = LebelDisplay+user.FullName;

				SaveAdminRights.Text = GetString( "SAVE" );
				CancelAdminRights.Text = GetString( "CANCEL" );
				
				InitializeGroupList();

			}
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender(object sender, EventArgs e)
		{
			// Set the breadcrumb list.
			BuildBreadCrumbList();

		}
		
		//private void Page_Unload(object sender, System.EventArgs e)
                //{
                //       web.Url = currentServerURL;
                //}

		#endregion

		#region Protected Methods

                /// <summary>
		/// Page_Load
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
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DisconnectMaster ()
		{
			web.Url = currentServerURL;
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
		/// Event handler that gets called when the cancel report button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnCancelAdminRights_Click( object sender, EventArgs e )
		{
			// Restore to previous settings.
			Response.Redirect("SystemInfo.aspx");
		}


		/// <summary>
		/// Event handler that gets called when the FrequencyList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnNoOfiFoldersList_Changed( object sender, EventArgs e )
		{
		}

		/// <summary>
		/// Event handler that gets called when the FrequencyList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnGroupList_Changed( object sender, EventArgs e )
		{
			string GroupName = GroupList.SelectedValue;
			string groupID = "";
			if(op == "ADD")
			{
					Hashtable ht = GroupHashTable;
					if(ht.ContainsKey(GroupName))
					{
						groupID = (string)GroupHashTable[ GroupName ];
						long limit = web.GetAggregateDiskQuota(groupID);
						if( limit == -1 )
							AggregateDiskQuotaText.Text = "";
						else
							AggregateDiskQuotaText.Text = Utils.ConvertToMBString( limit, false, rm );
					}
			}

                        if(GroupName.Equals(GetString("NONE")))
			{
				AggregateDiskQuotaText.Text = "";//web.GetAggregateDiskQuotaForGroup();
				SaveAdminRights.Enabled = false;
			}
			else
				SaveAdminRights.Enabled = true;
		}

		/// <summary>
		/// Event handler that gets called when the FrequencyList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDiskQuotaRightsList_Changed( object sender, EventArgs e )
		{
			if(AggregateDiskQuotaText.Text.Trim() != "")
			{
				SaveAdminRights.Enabled = true;
			}
		}

		/// <summary>
		/// Event handler that gets called when the report FormatList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFileSizeRightsList_Changed( object sender, EventArgs e )
		{
			if(AggregateDiskQuotaText.Text.Trim() != "")
			{
				SaveAdminRights.Enabled = true;
			}
		}

		/// <summary>
		/// Event handler that gets called when the report FormatList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnSyncIntervalRightsList_Changed( object sender, EventArgs e )
		{
			if(AggregateDiskQuotaText.Text.Trim() != "")
			{
				SaveAdminRights.Enabled = true;
			}
		}

		/// <summary>
		/// Event handler that gets called when the report FormatList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFileListRightsList_Changed( object sender, EventArgs e )
		{
			if(AggregateDiskQuotaText.Text.Trim() != "")
			{
				SaveAdminRights.Enabled = true;
			}
		}

		/// <summary>
		/// Event handler that gets called when the report FormatList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnSharingRightsList_Changed( object sender, EventArgs e )
		{
			if(AggregateDiskQuotaText.Text.Trim() != "")
			{
				SaveAdminRights.Enabled = true;
			}
		}

		/// <summary>
		/// Event handler that gets called when the report FormatList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnEncryptionRightsList_Changed( object sender, EventArgs e )
		{
			if(AggregateDiskQuotaText.Text.Trim() != "")
			{
				SaveAdminRights.Enabled = true;
			}
		}

		/// <summary>
		/// Event handler that gets called when the report FormatList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnProvisioningRightsList_Changed( object sender, EventArgs e )
		{
			if(AggregateDiskQuotaText.Text.Trim() != "")
			{
				SaveAdminRights.Enabled = true;
			}
		}

		/// <summary>
		/// Event handler that gets called when the report FormatList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OniFolderRightsList_Changed( object sender, EventArgs e )
		{
			if(AggregateDiskQuotaText.Text.Trim() != "")
			{
				SaveAdminRights.Enabled = true;
			}
		}

		protected void ShowSecondaryAdminRights(string GroupID)
		{
			long RightsValue = web.GetUserGroupRights(SecondaryAdmin, GroupID);

			UserGroupAdminRights UsersGroupRights = new UserGroupAdminRights(RightsValue);
			NoOfiFoldersList.Items[0].Selected = UsersGroupRights.iFolderLimitAllowed;

			DiskQuotaRightsList.Items[0].Selected = UsersGroupRights.DiskQuotaAllowed;

			FileSizeRightsList.Items[0].Selected = UsersGroupRights.FileSizeAllowed;

			SyncIntervalRightsList.Items[0].Selected = UsersGroupRights.SyncIntervalAllowed;

			FileListRightsList.Items[0].Selected = UsersGroupRights.AddToExcludePolicyAllowed;

			SharingRightsList.Items[0].Selected = UsersGroupRights.ChangeSharingAllowed;

			EncryptionRightsList.Items[0].Selected = UsersGroupRights.ChangeEncryptionAllowed;

			ProvisioningRightsList.Items[0].Selected = UsersGroupRights.ProvisioningAllowed;
			ProvisioningRightsList.Items[1].Selected = UsersGroupRights.EnableDisableUserAllowed;

			iFolderRightsList.Items[0].Selected = UsersGroupRights.OwnOrphaniFolderAllowed;
			iFolderRightsList.Items[1].Selected = UsersGroupRights.EnableDisableiFolderAllowed;
			iFolderRightsList.Items[2].Selected = UsersGroupRights.ModifyMemberRightAllowed;
			iFolderRightsList.Items[3].Selected = UsersGroupRights.DeleteiFolderAllowed;			
		}

		protected int GetSelectedRights()
		{
			int value = 0;
			value += (NoOfiFoldersList.Items[0].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.iFolderLimitAllowed : 0);

			value += (DiskQuotaRightsList.Items[0].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.DiskQuotaAllowed : 0);
			value += (FileSizeRightsList.Items[0].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.FileSizeAllowed : 0);
			value += (SyncIntervalRightsList.Items[0].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.SyncIntervalAllowed : 0);
			value += (FileListRightsList.Items[0].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.AddToExcludePolicyAllowed : 0);

			value += (SharingRightsList.Items[0].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.ChangeSharingAllowed : 0);

			value += (EncryptionRightsList.Items[0].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.ChangeEncryptionAllowed : 0);

			value += (ProvisioningRightsList.Items[0].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.ProvisioningAllowed : 0);
			value += (ProvisioningRightsList.Items[1].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.EnableDisableUserAllowed : 0);

			value += (iFolderRightsList.Items[0].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.OwnOrphaniFolderAllowed : 0);
			value += (iFolderRightsList.Items[1].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.EnableDisableiFolderAllowed : 0);
			value += (iFolderRightsList.Items[2].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.ModifyMemberRightAllowed : 0);
			value += (iFolderRightsList.Items[3].Selected ? (int)Novell.iFolderWeb.Admin.UserGroupAdminRights.GroupAdminPreferencesEnum.DeleteiFolderAllowed : 0);
			return value;
		}

		/// <summary>
		/// Event handler that gets called when the save report button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnSaveAdminRights_Click( object sender, EventArgs e )
		{
			string Status = "SUCCESS";

			string GroupID = null;
			string GroupName = GroupList.SelectedValue;
			Hashtable ht = GroupHashTable;
			if(ht.ContainsKey(GroupName))
				GroupID = (string)GroupHashTable[ GroupName ];

			if(GroupName.Equals(GetString("NONE")))
			{
				TopNav.ShowError(GetString("ERRORSELECTGROUP"));
				return;
			}
			long GroupDiskLimit = 1;
			string limitString = AggregateDiskQuotaText.Text;
			if (limitString == null || limitString == String.Empty)
			{
				limitString = "Unlimited";
			}
			try
			{
				decimal limit = Convert.ToDecimal( limitString == "Unlimited" ? "-1" : limitString );
				if ( limit > 0 || limitString == "Unlimited")
				{
					// Convert from megabytes back to bytes.
					
					GroupDiskLimit = limitString == "Unlimited" ? -1 : Convert.ToInt64( Decimal.Round( limit, 2 ) * 1048576 );
					// Get the values from the rights field	
					int RightsValue = GetSelectedRights();

					ConnectMaster();
					// call webservice and pass two parameters to commit.
					web.AddGroupAdministrator( GroupID, SecondaryAdmin, RightsValue);
					bool retval = web.SetAggregateDiskQuota( GroupID, GroupDiskLimit);
					if(retval == false)
					{
						TopNav.ShowError( GetString( "INVALIDGROUPQUOTA" ) );
						return;
					}
	
					Response.Redirect(
                          			                     String.Format(
                                               			        "StatusPage.aspx?status={0}&secondaryadmin={1}&groupname={2}&op={3}",
                 			                                         Status, SecondaryAdmin, GroupName, op));
                                       			                 //true );
					DisconnectMaster();
					
				}
				else
				{
					TopNav.ShowError(GetString("ERRORINVALIDQUOTA"));
					return;
				}
			}
			catch( FormatException )
			{
				TopNav.ShowError( GetString( "ERRORINVALIDQUOTA" ) );
				return;
			}
			catch ( OverflowException )
			{
				TopNav.ShowError( GetString( "ERRORINVALIDQUOTA" ) );
				return;
			}
			catch 
			{
				DisconnectMaster();
				if(op == "ADD")
				{
					string errMsg = String.Format( GetString( "ERRORCANNOTADDSECONDARYADMIN" ), SecondaryAdmin );
					TopNav.ShowError( errMsg );
					return;
				}
				else if( op == "EDIT")
				{
					string errMsg = String.Format( GetString( "ERRORCANNOTEDITSECONDARYADMIN" ), SecondaryAdmin );
					TopNav.ShowError( errMsg );
					return;
					
				}
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

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		#region Report Configuration Object

		#endregion
	}
}
