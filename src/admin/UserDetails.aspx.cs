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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Resources;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for UserDetails.
	/// </summary>
	public class Details : System.Web.UI.Page
	{
		#region Class Members

		/// <summary>
		/// iFolder list data grid column indices.
		/// </summary>
		private const int iFolderIDColumn = 0;
		private const int iFolderDisabledColumn = 1;
		private const int iFolderCheckColumn = 2;
		private const int iFolderTypeColumn = 3;
		private const int iFolderNameColumn = 4;
		private const int iFolderOwnerColumn = 5;
		private const int iFolderSizeColumn = 6;

		/// <summary>
		/// iFolder list display types.
		/// </summary>
		private enum ListDisplayType
		{
			All,
			Owned,
			Shared
		}

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin remoteweb;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Logged in admin system rights instance
		/// </summary>
		UserGroupAdminRights uRights;
		
		/// <summary>
		/// Logged in user system rights value
		/// </summary>
		int grpAccessPolicy = 0;

		/// <summary>
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;


		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal UserName;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal FullName;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal MemberType;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal MembersTag;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal MembersList;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal LdapContext;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal LastLogin;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal GroupDiskQuotaHeader;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal DiskQuotaUsedHeader;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal DiskQuotaUsedLiteral;
	
		/// <summary>
		/// User detail controls.
		/// </summary>
		protected TextBox GroupDiskQuotaText;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal GroupDiskQuotaLiteral;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Button GroupDiskQuotaSave;

		/// <summary>
		/// iFolder user policy control.
		/// </summary>
		protected Policy Policy;


		/// <summary>
		/// iFolder list controls.
		/// </summary>
		protected DataGrid iFolderList;

		/// <summary>
		/// iFolder list footer control.
		/// </summary>
		protected ListFooter iFolderListFooter;


		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected LinkButton AlliFoldersLink;

		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected LinkButton OwnediFoldersLink;

		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected LinkButton SharediFoldersLink;

		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected HtmlGenericControl CurrentTab;

		/// <summary>
		/// iFolder disable button control.
		/// </summary>
		protected Button DisableiFolderButton;

		/// <summary>
		/// iFolder enable button control.
		/// </summary>
		protected Button EnableiFolderButton;

		/// <summary>
		/// Delete ifolder button control.
		/// </summary>
		protected Button DeleteiFolderButton;

		/// <summary>
		/// iFolder create button control.
		/// </summary>
		//protected Button CreateiFolderButton;


		/// <summary>
		/// All checked ifolders control.
		/// </summary>
		protected CheckBox AlliFoldersCheckBox;

		/// <summary>
		/// ifolders checked control.
		/// </summary>
		protected CheckBox iFolderListCheckBox;

		/// <summary>
		/// Server URL for logged in user.
		/// </summary>
		protected string currentServerURL;

        /// <summary>
        /// bool showing server reachable state
        /// </summary>
		protected bool reachable = true;


		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the iFolders that are checked in the list.
		/// </summary>
		private Hashtable CheckediFolders
		{
			get { return ViewState[ "CheckediFolders" ] as Hashtable; }
			set { ViewState[ "CheckediFolders" ] = value; }
		}

		/// <summary>
		/// Gets or sets the active ifolder tab.
		/// </summary>
		private ListDisplayType ActiveiFolderTab
		{
			get { return ( ListDisplayType )ViewState[ "ActiveTab" ]; }
			set { ViewState[ "ActiveTab" ] = value; }
		}

		/// <summary>
		/// Gets or sets the current iFolder offset.
		/// </summary>
		private int CurrentiFolderOffset
		{
			get { return ( int )ViewState[ "CurrentiFolderOffset" ]; }
			set { ViewState[ "CurrentiFolderOffset" ] = value; }
		}

		/// <summary>
		/// Gets or sets the total number of iFolders contained in
		/// the last search.
		/// </summary>
		private int TotaliFolders
		{
			get { return ( int )ViewState[ "TotaliFolders" ]; }
			set { ViewState[ "TotaliFolders" ] = value; }
		}

		/// <summary>
		/// Gets the iFolder user ID.
		/// </summary>
		private string UserID
		{
			get { return Request.Params[ "ID" ]; } 
		}

		public string ErrorMsg
                {
                        get { return Request.Params[ "errormsg" ]; }
                }



		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		/// <param name="fullName">The full name of the current user.</param>
		private void BuildBreadCrumbList( string fullName )
		{
			TopNav.AddBreadCrumb( GetString( "USERS" ), "Users.aspx" );
			TopNav.AddBreadCrumb( fullName, null );
			// Pass this page information to create the help link
			TopNav.AddHelpLink(GetString("USERDETAILS"));
		}

		/// <summary>
		/// Creates a list of iFolders where the user is a member.
		/// </summary>
		/// <returns>A DataView object containing the iFolder list.</returns>
		private DataView CreateiFolderList()
		{

			if(ErrorMsg !=null )
			{	
                        	TopNav.ShowError(ErrorMsg);
			}

			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "OwnerIDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "SharedField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "DisabledField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "EncryptedField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "OwnerNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "ReachableField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "PreferenceField", typeof( int) ) );

			// Get the iFolder list for this user.
			iFolderSet list;

			switch ( ActiveiFolderTab )
			{
				case ListDisplayType.Owned:
					list = web.GetiFoldersByMember( UserID, MemberRole.Owner, CurrentiFolderOffset, iFolderList.PageSize );
					break;

				case ListDisplayType.Shared:
					list = web.GetiFoldersByMember( UserID, MemberRole.Shared, CurrentiFolderOffset, iFolderList.PageSize );
					break;

				case ListDisplayType.All:
				default:
					list = web.GetiFoldersByMember( UserID, MemberRole.Any, CurrentiFolderOffset, iFolderList.PageSize );
					break;
			}
			iFolder ifolder = null;

			foreach( iFolder folder in list.Items )
			{
				bool encrypted = false;
				try
				{
	                                string ifolderLocation = web.GetiFolderLocation (folder.ID);
        	                        UriBuilder remoteurl = new UriBuilder(ifolderLocation);
                	                remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                        	        web.Url = remoteurl.Uri.ToString();
				}
				catch
				{
					//skip exceptions
					continue;
				}
				
				try
				{
					ifolder = web.GetiFolder(folder.ID);
				}
				catch
				{
					web.Url = currentServerURL;
					ifolder = web.GetiFolder(folder.ID);
					reachable = false;
				}
				string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
				if(!(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty)))
				{
					// It is an encrypted ifolder 
					encrypted = true;
				}
				string ShortenedName = null;
				int ShortenedLength = 40;
				if(folder.Name.Length > ShortenedLength)
				{
					// make it of desired length
					ShortenedName = web.GetShortenedName(folder.Name, ShortenedLength);
				}

			
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = folder.ID;
				dr[ 2 ] = folder.OwnerID;
				dr[ 3 ] = ( folder.MemberCount > 1 ) ? true : false;
				dr[ 4 ] = !ifolder.Enabled;
				dr[ 5 ] = ( encrypted ) ? true : false;
				dr[ 6 ] = ( folder.Name.Length > ShortenedLength) ? ShortenedName : folder.Name;
				dr[ 7 ] = folder.OwnerFullName;
				dr[ 8 ] = reachable;
				dr[ 9 ] = folder.Name;
				dr[10 ] = folder.Preference;

				dt.Rows.Add( dr );
				reachable = true;
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < iFolderList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = String.Empty;
				dr[ 3 ] = false;
				dr[ 4 ] = false;
				dr[ 5 ] = false;
				dr[ 6 ] = String.Empty;
				dr[ 7 ] = String.Empty;
				dr[ 8 ] = false;
				dr[ 9 ] = String.Empty;
				dr[10] = 0;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotaliFolders = list.Total;
			web.Url = currentServerURL;

			// Build the data view from the table.
			return new DataView( dt );
		}

                /// <summary>
                /// Returns the checked state for the specified member.
                /// </summary>
                /// <param name="id">ID of the ifolder</param>
                /// <returns>True if ifolder is checked.</returns>
                protected bool IsiFolderEnabled( Object pref )
                {
			int preference = (int)pref;
			if( preference == -1)
				preference = 0xffff;
                        UserGroupAdminRights rights = new UserGroupAdminRights((int)preference);
                        return rights.EnableDisableiFolderAllowed;
                //      return CheckediFolders.ContainsKey( id ) ? true : false;
                }

		/// <summary>
		/// Gets an ifolder policy object that is set-able.
		/// </summary>
		/// <param name="ifolderID"></param>
		/// <returns></returns>
		private iFolderPolicy GetiFolderPolicyObject( string ifolderID )
		{
			iFolderPolicy policy = new iFolderPolicy();
			policy.iFolderID = ifolderID;
			policy.FileSizeLimit = policy.SpaceLimit = policy.SyncInterval = -1;
			policy.FileTypesExcludes = policy.FileTypesIncludes = null;
			policy.Locked = false;
			return policy;
		}

		/// <summary>
		/// Gets the iFolders for the current user.
		/// </summary>
		/// <param name="checkedState"></param>
		private void GetiFolders( bool checkedState )
		{
			// Create a data source containing the iFolders.
			iFolderList.DataSource = CreateiFolderList();
			iFolderList.DataBind();
			SetPageButtonState();
			AlliFoldersCheckBox.Checked = checkedState;
		}

		/// <summary>
		/// Gets the details about the user and fills out the details table.
		/// </summary>
		/// <returns>The user's full name.</returns>
		private string GetUserDetails()
		{
			// Insert NewLine char after 80 char	
			int NewlineAt=80;	
			// Get the iFolder user information.
			iFolderUserDetails details = web.GetUserDetails( UserID );

			string lastLogin = ( details.LastLogin == DateTime.MinValue ) ?
				GetString( "NOTAVAILABLE" ) : Utils.ToDateTimeString( details.LastLogin );

			// Add the information rows to the table.
			UserName.Text = FormatInputString(details.UserName,NewlineAt);
			FullName.Text = FormatInputString(details.FullName,NewlineAt);
			switch(details.MemberType)
			{
				case 1:
				MemberType.Text = GetString( "MEMBERTYPELDAPGROUP" );
				GroupDiskQuotaHeader.Text = GetString("GROUPDISKQUOTA");
				GroupDiskQuotaHeader.Visible = true;
				long GroupDiskQuotaValue = details.GroupDiskQuota;
				if( GroupDiskQuotaValue == -1 )
					GroupDiskQuotaText.Text = "";
				else
					GroupDiskQuotaText.Text = Utils.ConvertToMBString( GroupDiskQuotaValue, false, rm );
				GroupDiskQuotaText.Visible = true;
				GroupDiskQuotaLiteral.Text = GetString("MB");
				GroupDiskQuotaLiteral.Visible = true;
				GroupDiskQuotaSave.Text = GetString("SAVE");
				GroupDiskQuotaSave.Visible = true;
				/// Disable for groupadmin...
				string loggedinuserID = Session[ "UserID" ] as String;
				int pref = web.GetUserGroupRights(loggedinuserID, UserID);
				if( pref != -1 && pref != 0xffff )
				{
					GroupDiskQuotaText.Enabled = false;
					GroupDiskQuotaSave.Enabled = false;
				}
			
				DiskQuotaUsedHeader.Text = GetString("DISKSPACEUSED");
				DiskQuotaUsedHeader.Visible = true;
				DiskQuotaUsedLiteral.Text = Utils.ConvertToMBString( web.SpaceUsedByGroup(UserID), false, rm ) + " " + GetString("MB");
				DiskQuotaUsedLiteral.Visible = true;
				break;
				case 2:
				MemberType.Text = GetString( "MEMBERTYPELOCALGROUP" );
				break;
				default:
				MemberType.Text = GetString( "MEMBERTYPEUSER" );
				break;
			}
			if(details.MemberType > 0)
				MembersTag.Text = GetString( "MEMBERLISTTAG" );
			else
				MembersTag.Text = GetString( "GROUPLISTTAG" );
				MembersList.Text = FormatInputString(details.GroupOrMemberList,NewlineAt); 
			LdapContext.Text = details.LdapContext;
			LastLogin.Text = lastLogin;
			return details.FullName;
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
		private void Page_Load(object sender, System.EventArgs e)
		{
			// connection
			web = Session[ "Connection" ] as iFolderAdmin;
			currentServerURL = web.Url;
			remoteweb = new iFolderAdmin();
			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;
			remoteweb.Url = web.Url;
			reachable = true;

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			string userID = Session[ "UserID" ] as String;
			grpAccessPolicy = web.GetUserGroupRights(userID, null);
			uRights = new UserGroupAdminRights(grpAccessPolicy);
			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				iFolderList.Columns[ iFolderTypeColumn ].HeaderText = GetString( "TYPE" );
				iFolderList.Columns[ iFolderNameColumn ].HeaderText = GetString( "NAME" );
				iFolderList.Columns[ iFolderOwnerColumn ].HeaderText = GetString( "OWNER" );

				//DeleteiFolderButton.Text = GetString( "DELETE" );
				DisableiFolderButton.Text = GetString( "DISABLE" );
				EnableiFolderButton.Text = GetString( "ENABLE" );
				DeleteiFolderButton.Text = GetString ("DELETE");
				//CreateiFolderButton.Text = GetString( "CREATE" );

				AlliFoldersLink.Text = GetString( "ALL" );
				OwnediFoldersLink.Text = GetString( "OWNED" );
				SharediFoldersLink.Text = GetString( "SHARED" );

				// Initialize state variables.
				CurrentiFolderOffset = 0;
				TotaliFolders = 0;
				AlliFoldersCheckBox.Checked = false;
				CheckediFolders = new Hashtable();

				// Set the active ifolder tab.
				ActiveiFolderTab = ListDisplayType.All;

				//CreateiFolderButton.Enabled = GetCreateButtonStatus();
			}

			// Set the active ifolder display tab.
			SetActiveiFolderListTab( ActiveiFolderTab );
			DeleteiFolderButton.Enabled = uRights.DeleteiFolderAllowed;

			if(uRights.EnableDisableiFolderAllowed == false)
			{
				AlliFoldersCheckBox.Enabled = false;	
				//iFolderListCheckBox.Enabled = false;
			}
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender(object sender, EventArgs e)
		{
			// Fill in the user details.
			string fullName = GetUserDetails();

			// Set the breadcrumb list.
			BuildBreadCrumbList( fullName );

			// Fill in the policy information.
			if(!Policy.GetUserPolicies())
				TopNav.ShowInfo(String.Format ("{0} - {1}", GetString("SERVERSTATUSDOWN"), GetString("MINIMALINFO")));

			// Get the iFolders.
			GetiFolders( false );
		}

		/// <summary>
		/// Enables or disables the ifolder action buttons.
		/// </summary>
		private void SetActionButtons()
		{
			Hashtable ht = CheckediFolders;
			DeleteiFolderButton.Enabled = ( ht.Count > 0 ) ? true : false;
			DisableiFolderButton.Enabled = ht.ContainsValue( Boolean.FalseString );
			EnableiFolderButton.Enabled = ht.ContainsValue( Boolean.TrueString );
		}

		/// <summary>
		/// Sets the active ifolder list display tab.
		/// </summary>
		/// <param name="activeTab"></param>
		/// <returns>The active list tab.</returns>
		private void SetActiveiFolderListTab( ListDisplayType activeTab )
		{
			ActiveiFolderTab = activeTab;
			CurrentTab.ID = activeTab.ToString();
		}

		/// <summary>
		/// Sets the page button state of the ifolder list.
		/// </summary>
		private void SetPageButtonState()
		{
			iFolderListFooter.SetPageButtonState( 
				iFolderList, 
				CurrentiFolderOffset, 
				TotaliFolders, 
				GetString( "IFOLDERS" ),
				GetString( "IFOLDER" ) );
		}

		/// <summary>
		/// returns whether this user can be owner of shared ifolder or not . If encryption is enforced then return false.
		/// </summary>
        /// <returns>true if the user can be owner</returns>
		//private bool GetCreateButtonStatus()
		//{
		//	try{
		//		bool UserEncryptionEnforced = web.IsUserOrSystemEncryptionEnforced(UserID);
		//		bool CreateButtonStatus = ! UserEncryptionEnforced ;	
		//		return CreateButtonStatus;
		//	}catch
		//	{	
		//		return true;
		//	}	
		//}

		/// <summary>
		/// Get Logged in admin rights for iFolder.
		/// </summary>
                private int GetRightsForiFolder(string iFolderID)
                {
                        int preference = 0;
                        foreach( DataGridItem item in iFolderList.Items )
                        {
                                string ifolderid = item.Cells[ iFolderIDColumn].Text;
                                if( ifolderid ==iFolderID )
                                {
                                        preference = Convert.ToInt32(item.Cells[ 10].Text);
                                        break;
                                }
                        }
                        return preference;
                }

		/// <summary>
		/// Gets iFolder's owner ID
		/// </summary>
		private string GetiFolderOwnerID(string iFolderID)
		{
			foreach( DataGridItem item in iFolderList.Items )
			{
				string ifolderid = item.Cells[iFolderIDColumn].Text;
				if( ifolderid == iFolderID ) 
				    //FIXME : Magic numbers
				    return item.Cells [2].Text; //iFolder ID column.
			}
			return null;
		}

		/// <summary>
		/// Sets the ifolder synchronization status on all selected ifolders.
		/// </summary>
		/// <param name="syncStatus">If true then all selected ifolders will be enabled.</param>
		private void SetSelectediFolderStatus( bool syncStatus )
		{
			foreach( string ifolderID in CheckediFolders.Keys )
			{
				// Check if this check (next 6 lines) can be removed after UT, Is it a redundant check??
				int preference = GetRightsForiFolder(ifolderID);
				if( preference == -1)
					preference = 0xffff;
				UserGroupAdminRights rights = new UserGroupAdminRights((int)preference);
				if( !rights.EnableDisableiFolderAllowed )
					continue;
				// Don't set the status if already set.
				if ( CheckediFolders[ ifolderID ] as string != syncStatus.ToString() )
				{
					iFolderPolicy policy = GetiFolderPolicyObject( ifolderID );
					policy.Locked = syncStatus;
                                        string ifolderLocation = web.GetiFolderLocation (ifolderID);
                                        UriBuilder remoteurl = new UriBuilder(ifolderLocation);
                                        remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                                        web.Url = remoteurl.Uri.ToString();

					try
					{
						web.SetiFolderPolicy( policy );
					}
					catch ( Exception ex )
					{
						TopNav.ShowError( GetString( "ERRORCANNOTSETIFOLDERSTATUS" ), ex );
						web.Url = currentServerURL;
						return;
					}
				}
			}

			// Clear the checked members.
			CheckediFolders.Clear();

			web.Url = currentServerURL;
			SetActionButtons();

			// Rebind the data source with the new data.
			GetiFolders( false );
		}

		#endregion

		#region Protected Methods

		/// <summary>
                /// Event handler that gets called when the save group limit button is clicked.
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                protected void SaveGroupDiskQuota( object sender, EventArgs e )
                {
                        long GroupDiskLimit = 0;
                        string limitString = GroupDiskQuotaText.Text;
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

					// connect to master to set member property
					ConnectMaster();
					// call webservice and pass parameters to commit.

					bool retval = web.SetAggregateDiskQuota(UserID, GroupDiskLimit);
					DisconnectMaster();
					web.Url = currentServerURL;
                                        if(retval == false)
                                        {
                                                TopNav.ShowError( GetString( "INVALIDGROUPQUOTA" ) );
                                                return;
                                        }

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
			catch(Exception ex)
			{
				if(ex.Message.IndexOf("timed out") != -1)
				{		
					TopNav.ShowInfo(GetString("TIMEOUT"));
					return;
				}
				else
				{
					DisconnectMaster();
					web.Url = currentServerURL;
					TopNav.ShowError( GetString( "ERRORUNKNOWNERROR" ));
					return; 
				}
			}

			GetUserDetails();
                        GroupDiskQuotaSave.Enabled = false;
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
                /// Event handler that gets called when the description text changes.
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                protected void LimitChanged( object sender, EventArgs e )
                {
                        GroupDiskQuotaSave.Enabled = true;
                }

		/// <summary>
		/// Event handler that gets called when the all ifolders tab is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void AlliFolders_Clicked( Object sender, EventArgs e )
		{
			SetActiveiFolderListTab( ListDisplayType.All );
			GetiFolders( false );
		}

		/// <summary>
		/// Gets the image representing the iFolder type.
		/// </summary>
		/// <param name="disabled"></param>
		/// <param name="shared"></param>
		/// <returns></returns>
		protected string GetiFolderImage( object disabled, object shared , object encrypted)
		{
			if( (bool) disabled)
				return "images/ifolder_16-gray.gif";
			else if( (bool) encrypted)
				return "images/encrypt_ilock2_16.gif";
			
			return ( bool )shared ? "images/ifolder_user_16.gif" : "images/ifolder.png";
		}

		/// <summary>
		/// Returns the checked state for the specified member.
		/// </summary>
		/// <param name="iFolderID">ID of the ifolder</param>
		/// <returns>True if ifolder is checked.</returns>
		protected bool GetMemberCheckedState( Object iFolderID )
		{
			return CheckediFolders.ContainsKey( iFolderID ) ? true : false;
		}

		/// <summary>
		/// Gets the navigation url for the owner of the ifolder if the owner
		/// is not the current user.
		/// </summary>
		/// <param name="ownerID">The ID of the owner of the ifolder.</param>
		/// <returns>The URL to navigate to the owner of the ifolder.</returns>
		protected string GetOwnerUrl( Object ownerID )
		{
			return ( ownerID as string != UserID ) ? String.Format( "UserDetails.aspx?id={0}", ownerID ) : String.Empty;
		}

		/// <summary>
		/// Gets the navigation url for the ifolder if the 
		/// iFolder is reachable.
		/// </summary>
		/// <param name="ownerID">The ID of the ifolder.</param>
		/// <returns>The URL to navigate to the ifolder.</returns>
		protected string GetiFolderUrl( Object reach, Object iFolderID )
		{
			return (bool) reach? String.Format( "iFolderDetailsPage.aspx?id={0}&userid={1}", iFolderID,UserID ) : String.Empty;
		}

		private string GetiFolderName(string iFolderID)
		{
			foreach( DataGridItem item in iFolderList.Items )
			{
				string ifolderid = item.Cells[ iFolderIDColumn].Text;
				if( ifolderid == iFolderID )
				    //FIXME : Magic numbers
				    return item.Cells [9].Text; //iFolder name column.
			}
			return null;
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
		/// Event handler that gets called when the delete ifolder button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDeleteiFolderButton_Click( object source, EventArgs e )
		{
			string skippediFolderNames = "";
			string loggedinuserID = Session[ "UserID" ] as String;

			foreach( string ifolderID in CheckediFolders.Keys )
			{
				try
				{
				        int rights = GetRightsForiFolder(ifolderID);
					if (rights == -1 ) rights = 0xffff;

					UserGroupAdminRights adminRights = new UserGroupAdminRights(rights);
					string ownerID = GetiFolderOwnerID (ifolderID);
					/*Condition for skipping iFolders for deletion. We allow the owner to 
					  delete his own iFolder. */
					if (loggedinuserID != ownerID) {
					    if(!adminRights.DeleteiFolderAllowed) {
						string ifolderName = GetiFolderName (ifolderID);
						if (skippediFolderNames.Length > 0 ) //Just for adding a comma.
						    skippediFolderNames += ", " + ifolderName;
						else
						    skippediFolderNames += ifolderName;

						continue;
					    }
					}

		                        string ifolderLocation = web.GetiFolderLocation (ifolderID);
                		        UriBuilder remoteurl = new UriBuilder(ifolderLocation);
		                        remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                		        web.Url = remoteurl.Uri.ToString();
					web.DeleteiFolder( ifolderID );
				}
				catch ( Exception ex )
				{
					TopNav.ShowError( GetString( "ERRORCANNOTDELETEIFOLDER" ), ex );
					web.Url = currentServerURL;
					return;
				}

			}
			web.Url = currentServerURL;

			// Clear the checked members.
			CheckediFolders.Clear();
			AlliFoldersCheckBox.Checked = false;

			// Rebind the data source with the new data.
			GetiFolders( false );

			//If we have skipped some iFolders, tell the admin.
			if (skippediFolderNames.Length > 0)
			    TopNav.ShowError(string.Format (GetString ("ERRORCANNOTDELETEIFOLDER"), skippediFolderNames));
		}

		/// <summary>
		/// Event handler that gets called when the check all iFolders checkbox is selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnAlliFoldersChecked( object sender, EventArgs e )
		{
			CheckBox checkBox = sender as CheckBox;
			foreach( DataGridItem item in iFolderList.Items )
			{
				string ifolderID = item.Cells[ iFolderIDColumn ].Text;
				if ( ifolderID != "&nbsp;" )
				{
					if ( checkBox.Checked )
					{
						CheckediFolders[ ifolderID ] = item.Cells[ iFolderDisabledColumn ].Text;
					}
					else
					{
						// Remove this iFolder from the list.
						CheckediFolders.Remove( ifolderID );
					}
				}
			}

			// Set the ifolder action buttons.
			SetActionButtons();

			// Rebind the data source with the new data.
			GetiFolders( checkBox.Checked );
		}

		/// <summary>
		/// Event handler that gets called when the create ifolder button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnCreateiFolder( object source, EventArgs e )
		{
			Page.Response.Redirect( String.Format( "CreateiFolder.aspx?owner={0}&fn={1}", UserID, FullName.Text ), true );
		}

		/// <summary>
		/// Event handler that gets called when the disable ifolder button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDisableiFolder( object source, EventArgs e )
		{
			SetSelectediFolderStatus( true );
		}

		/// <summary>
		/// Event handler that gets called when the enable ifolder button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnEnableiFolder( object source, EventArgs e )
		{
			SetSelectediFolderStatus( false );
		}

		/// <summary>
		/// Event handler that gets called when an ifolder is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OniFolderChecked( object source, EventArgs e )
		{
			// Get the data grid row for this ifolder.
			CheckBox checkBox = source as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string ifolderID = item.Cells[ iFolderIDColumn ].Text;
			if ( ifolderID != "&nbsp;" )
			{
				// iFolder is being checked.
				if ( checkBox.Checked )
				{
					CheckediFolders[ ifolderID ] = item.Cells[ iFolderDisabledColumn ].Text;
				}
				else
				{
					// Remove this member from the list.
					CheckediFolders.Remove( ifolderID );
				}
			}

			// Set the ifolder action buttons.
			SetActionButtons();
		}

		/// <summary>
		/// Event handler that gets called when the owned ifolders tab is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OwnediFolders_Clicked( Object sender, EventArgs e )
		{
			SetActiveiFolderListTab( ListDisplayType.Owned );
			GetiFolders( false );
		}

		/// <summary>
		/// Event that first when the PageFirstButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageFirstButton_Click( object source, ImageClickEventArgs e)
		{
			// Set to get the first iFolders.
			CurrentiFolderOffset = 0;
			GetiFolders( false );
		}

		/// <summary>
		/// Event that first when the PagePreviousButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PagePreviousButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentiFolderOffset -= iFolderList.PageSize;
			if ( CurrentiFolderOffset < 0 )
			{
				CurrentiFolderOffset = 0;
			}

			GetiFolders( false );
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentiFolderOffset += iFolderList.PageSize;
			GetiFolders( false );
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentiFolderOffset = ( ( TotaliFolders - 1 ) / iFolderList.PageSize ) * iFolderList.PageSize;
			GetiFolders( false );
		}

		/// <summary>
		/// Event handler that gets called when the shared ifolders tab is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void SharediFolders_Clicked( Object sender, EventArgs e )
		{
			SetActiveiFolderListTab( ListDisplayType.Shared );
			GetiFolders( false );
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
		/// this.Load += new System.EventHandler(this.Page_Load);
		private void InitializeComponent()
		{    
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			Policy.PolicyError += new Policy.PolicyErrorHandler( OnPolicyError );

			iFolderListFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			iFolderListFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			iFolderListFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			iFolderListFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			this.Load += new System.EventHandler(this.Page_Load);
		}

		#endregion
		#region Public Methods

		/// <summary>
        /// Function called to format the input String by keeping newline after
        /// every 80 character.
        /// </summary>
        /// <param name="InputString"></param>
        public static string FormatInputString(String InputString,int InsertIndex)
        {
            int strlenght = InputString.Length;
            string str = "\n";
            int count= strlenght/InsertIndex;
            for(int insert=1;count>0;count--,insert++)
            {
                InputString = InputString.Insert(insert*InsertIndex,str);
            }
            return InputString;
        }

        #endregion
		
	}
}
