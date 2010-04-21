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
using System.IO;
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
	/// Summary description for ServerDetails.
	/// </summary>
	public class ServerDetails : System.Web.UI.Page
	{
		#region Class Members
		private static readonly iFolderWebLogger log = new iFolderWebLogger(
				System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
			
		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// iFolder Connection to the remote server
		/// </summary>
	        private iFolderAdmin remoteweb;

		/// <summary>
                /// Control that checks or unchecks all of the ifolders in the current view.
                /// </summary>
                protected CheckBox AllDataPathCheckBox;
		
		/// <summary>
                /// Web controls.
                /// </summary>^M
                protected DataGrid DataPaths;

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
		/// Server name control.
		/// </summary>
		protected Literal Name;

		/// <summary>
		/// Server type control.
		/// </summary>
		protected Literal Type;

		/// <summary>
		/// Server dns name control.
		/// </summary>
		protected Literal DnsName;

		/// <summary>
		/// Server dns name control.
		/// </summary>
		protected Literal MasterUri;

		/// <summary>
		/// Server public ip address control.
		/// </summary>
		protected TextBox PublicIP;

		/// <summary>
		/// Server private ip address control.
		/// </summary>
		protected TextBox PrivateIP;

		/// <summary>
		/// Server private ip address control.
		/// </summary>
		protected TextBox MasterIP;

		/// <summary>
		/// Server status control
		/// </summary>
		protected Literal Status;

		/// <summary>
		/// Server provisioned user count control
		/// </summary>
		protected Literal UserCount;

		/// <summary>
		/// Server ifolder count control
		/// </summary>
		protected Literal iFolderCount;

		/// <summary>
		/// Server logged on user count control
		/// </summary>
		protected Literal LoggedOnUsersCount;

		/// <summary>
		/// Server session count control
		/// </summary>
		protected Literal SessionCount;

		/// <summary>
		/// Server disk space used control
		/// </summary>
		protected Literal DiskSpaceUsed;

		/// <summary>
		/// Server disk space available control
		/// </summary>
		protected Literal DiskSpaceAvailable;

		/// <summary>
		/// Server ldap status control
		/// </summary>
		protected Literal LdapStatus;

		/// <summary>
		/// Server maximum connection count control
		/// </summary>
		protected Literal MaxConnectionCount;

                /// <summary>
                /// Disable datapath  button control.
                /// </summary>
                protected Button DisableButton;
		
		/// <summary>
                /// Delete datapath  button control.
                /// </summary>
                protected Button DeleteButton;

                /// <summary>
                /// Enable datapath button control.
                /// </summary>
                protected Button EnableButton;

                /// <summary>
                /// Create datapath button control.
                /// </summary>
                protected Button AddButton;

		/// <summary>
		/// LDAP Server name.
		/// </summary>
		protected TextBox LdapServer;

		/// <summary>
		/// LDAP Cycles
		/// </summary>
		protected Literal LdapCycles;

		/// <summary>
		/// Server maximum connection count control
		/// </summary>
		protected Literal LdapUpSince;

		/// <summary>
		/// LDAP Proxy User
		/// </summary>
		protected TextBox LdapProxyUser;

		/// <summary>
		/// LDAP Proxy User
		/// </summary>
		protected TextBox LdapProxyUserPwd;

		/// <summary>
		/// LDAP SSL
		/// </summary>
		protected Literal LdapSsl;

		/// <summary>
		/// External Identity Sync Interval
		/// </summary>
		protected HtmlTextArea LdapSearchContext;

		/// <summary>
		/// Member Delete Grace Interval
		/// </summary>
		protected TextBox LdapDeleteGraceInterval;

		/// <summary>
		/// Member Delete Grace Interval
		/// </summary>
		protected TextBox IDSyncInterval;

		/// <summary>
		/// Log list control.
		/// </summary>
		protected DropDownList LdapSslList;

		/// <summary>
		/// External Identity Sync Now Button
		/// </summary>
		protected LinkButton SyncNowButton;

		/// <summary>
		/// External Identity ChangeMasterButton 
		/// </summary>
		protected LinkButton ChangeMasterButton;

		/// <summary>
		/// Server Details cancel button control.
		/// </summary>
		protected Button CancelServerDetailsButton;

		/// <summary>
		/// ServerDetails save button control.
		/// </summary>
		protected Button SaveServerDetailsButton;

		/// <summary>
		/// Ldap Details cancel button control.
		/// </summary>
		protected Button CancelLdapDetailsButton;

		/// <summary>
		/// ServerDetails save button control.
		/// </summary>
		protected Button SaveLdapDetailsButton;

		/// <summary>
		/// LDAP save button control.
		/// </summary>
		protected Button LdapEditButton;

		/// <summary>
		/// Report list control.
		/// </summary>
		protected DropDownList ReportList;

		/// <summary>
		/// Report file view button control.
		/// </summary>
		protected Button ViewReportButton;


		/// <summary>
		/// Log list control.
		/// </summary>
		protected DropDownList LogList;

		/// <summary>
		/// Log text control.
		/// </summary>
		protected TextBox LogText;

		/// <summary>
		/// Log file view button control.
		/// </summary>
		protected LinkButton ViewLogButton;

		/// <summary>
		/// Log level label control.
		/// </summary>
		protected Label LogLevelLabel;

		/// <summary>
		/// Log level label control.
		/// </summary>
		protected Label LogLabel;

		/// <summary>
		/// Log level list control.
		/// </summary>
		protected DropDownList LogLevelList;

		/// <summary>
		/// Log level button control.
		/// </summary>
		protected Button LogLevelButton;

		/// <summary>
		/// Log level button control.
		/// </summary>
		protected string redirectUrl;

		/// <summary>
                /// Web controls.
                /// </summary>
                protected ListFooter DataPathsFooter;


		/// <summary>
                /// Logged in Server URL
                /// </summary>
		protected string currentServerURL;

		/// <summary>
                /// Server online status
                /// </summary>
		protected bool serverStatus = false;
	
		#endregion

		#region Properties

		/// <summary>
		/// Gets the Server ID.
		/// </summary>
		private string ServerID
		{
			get { return Request.Params[ "ID" ]; } 
		}

		/// <summary>
                /// Gets or sets the current datapath offset.
                /// </summary>
                private int CurrentPathOffset
                {
                        get { return ( int )ViewState[ "CurrentPathOffset" ]; }
                        set { ViewState[ "CurrentPathOffset" ] = value; }
                }

		/// <summary>
                /// Gets or sets the total number of users contained in
                /// the last search.
                /// </summary>
                private int TotalPaths
                {
                        get { return ( int )ViewState[ "TotalPaths" ]; }
                        set { ViewState[ "TotalPaths" ] = value; }
                }

		/// <summary>
                /// Gets or sets the paths that are checked in the list.
                /// </summary>
                private Hashtable CheckedPaths
                {
                        get { return ViewState[ "CheckedPaths" ] as Hashtable; }
                        set { ViewState[ "CheckedPaths" ] = value; }
                }

		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		/// <param name="serverName"></param>
		private void BuildBreadCrumbList( string serverName )
		{
			TopNav.AddBreadCrumb( GetString( "SERVERS" ), "Servers.aspx" );
			TopNav.AddBreadCrumb( serverName, null );
			// Pass this page information to create the help link
			TopNav.AddHelpLink(GetString("SERVERDETAILS"));
		}

		/// <summary>
                /// Enables or disables the datapath action buttons.
                /// </summary>
                private void SetActionButtons()
                {
                        Hashtable ht = CheckedPaths;
                        DisableButton.Enabled = ht.ContainsValue( true );
                        EnableButton.Enabled = ht.ContainsValue( false );
			if( ht.Count > 0 )
				DeleteButton.Enabled = true;
			else
				DeleteButton.Enabled = false;
                }
		
		/// <summary>
		/// Gets the list of files to display in the log file list.
		/// </summary>
		private void GetLogList()
		{
			ArrayList levels = new ArrayList();

			levels.Add( GetString( "ALL" ) );
			levels.Add( GetString( "DEBUG" ) );
			levels.Add( GetString( "INFO" ) );
			levels.Add( GetString( "WARN" ) );
			levels.Add( GetString( "ERROR" ) );
			levels.Add( GetString( "FATAL" ) );
			levels.Add( GetString( "OFF" ) );

			LogLevelList.DataSource = levels;
			LogLevelList.DataBind();
		
			ArrayList files = new ArrayList();

			// BUGBUG!! - Make a websvc call to determine the
			// available log files and only add those to the list.

			files.Add( GetString( "SYSTEMLOGFILE" ) );
			files.Add( GetString( "ACCESSLOGFILE" ) );

			//TODO :
			//files.Add( GetString( "ADMINLOGFILE" ) );
			//files.Add( GetString( "WEBACCESSLOGFILE" ) );

			LogList.DataSource = files;
			LogList.DataBind();

			
		}

		/// <summary>
		/// Gets the list of reports to view from this server.
		/// </summary>
		private void GetReportList()
		{
			ArrayList reports = new ArrayList();
			
			string[] files = null;    

			try {
					files = remoteweb.GetReports();
			        foreach (string file in files)
				        reports.Add( file );

		        } 
			catch  
			{
				reports.Add ("N/A");
			}

			if (files == null || files.Length == 0) {
				reports.Add ("N/A");
				ReportList.Enabled = false;
				ViewReportButton.Enabled = false;
			}

			ReportList.DataSource = reports;
			ReportList.DataBind();
		}

		/// <summary>
		/// Gets the details about the current server.
		/// </summary>
		/// <returns>The name of the host node.</returns>
		private string GetServerDetails()
		{
			// Insert NewLine char 40th index	
			int NewLineAt=40;	
			iFolderServer server = null;
			server = web.GetServer( ServerID );

			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;

		        remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
			    
			redirectUrl = server.PublicUrl;

			serverStatus = true;
			Status.Text = "Online";
			LdapStatus.Text = "N/A";
			iFolderCount.Text = "N/A";
			DnsName.Text = "N/A";
			UserCount.Text = "N/A";
			Name.Text = server.Name;

                        //KLUDGE: for SSL enabling the server. Case: When the server is SSL enabled and the Web Admin is not configured for Server SSL
                        try
                        {
                                remoteweb.GetAuthenticatedUser();
                        }
                        catch (WebException ex)
                        {
                                HttpWebResponse htpw = (HttpWebResponse)(ex.Response);
                                if(ex.Status == WebExceptionStatus.ProtocolError)
                                {
                                        //HttpStatusCode code = (HttpStatusCode)htpw.StatusCode;
                                        TopNav.ShowInfo (String.Format("WebException {0}", htpw.StatusDescription));
					serverStatus = false;
					return server.Name;
                                }
                                //Incompatible communication (HTTP->HTTPS with no certificate installed)
                                if(ex.Status == WebExceptionStatus.SendFailure)
                                {
//                                      TopNav.ShowInfo (String.Format("WebException-noproto {0} {1}", ex.Status, remoteweb.Url));
                                        UriBuilder turl = new UriBuilder(server.PublicUrl);
                                        if(turl.Scheme == Uri.UriSchemeHttps)
                                        {
                                                turl.Scheme = Uri.UriSchemeHttp;
                                                //Assuming apache port - what if Simias is running behind XSP?? -- FIX
                                                turl.Port = 80;
                                        }
                                        else
                                        {
                                                turl.Scheme = Uri.UriSchemeHttps;
                                                turl.Port = 443;
                                        }
                                        remoteweb.Url = turl.ToString();
                                        redirectUrl = remoteweb.Url;
                                        remoteweb.Url = remoteweb.Url + "/iFolderAdmin.asmx";
//                                      TopNav.ShowInfo (String.Format("WebException-noproto {0} ", remoteweb.Url));

                                        try
                                        {
                                             remoteweb.GetAuthenticatedUser();
                                        }
                                        //catch (WebException ex1)
                                        catch
                                        {
//                                              TopNav.ShowInfo (String.Format("WebException-noproto {0} {1}", ex1.Status, remoteweb.Url));
                                                remoteweb = web;
                                                Status.Text = "<font color=red><b>Offline</b></font>";
						serverStatus = false;
                                                TopNav.ShowInfo (String.Format ("Unable to reach {0}. Displaying minimal information", Name.Text));
                                        }

                                }
                                else
                                {
					remoteweb = web;
//                                        TopNav.ShowInfo (String.Format("WebException- {0} {1}", ex.Status, remoteweb.Url));
                                	Status.Text = "<font color=red><b>Offline</b></font>";
					serverStatus = false;
                                }
                        }
                        //catch ( Exception e)
                        catch
                        {
                                remoteweb = web;
                                Status.Text = "<font color=red><b>Offline</b></font>";
				serverStatus = false;
//                                TopNav.ShowInfo (String.Format("Exception- {0} {1}", e.Message, remoteweb.Url));
				return server.Name;
                        }

                        try
                        {
                                server = remoteweb.GetServer ( ServerID);
                                DnsName.Text = Details.FormatInputString(server.HostName, NewLineAt);
                                UserCount.Text = server.UserCount.ToString();

                                iFolderSet ifolders = remoteweb.GetiFolders( iFolderType.All, 0, 1 );
                                iFolderCount.Text = ifolders.Total.ToString();

//                                LdapStatus.Text = remoteweb.IdentitySyncGetServiceInfo ().Status;
                        }
                        catch
                        {
                                //Some information failed: Does it mean the Server is not Stable ???
                                Status.Text = "<font color=red><b>Online</b></font>";
                        }
            		Name.Text = Details.FormatInputString(server.Name, NewLineAt); 
			Type.Text = GetString( server.IsMaster ? "MASTER" : "SLAVE" );
			PublicIP.Text = server.PublicUrl;
			PrivateIP.Text = server.PrivateUrl;

			if(! server.IsMaster)
			{
				//MasterIP.Text = get master's IP through an API ;
				LdapInfo ldapInfo = remoteweb.GetLdapDetails();
				MasterIP.Visible = MasterIP.Enabled = true;
				MasterIP.Text = ldapInfo.MasterURL;
				MasterUri.Text = GetString( "MASTERURI" );
				ChangeMasterButton.Enabled = true;
			}
			else
			{
				MasterIP.Enabled = false;
				MasterIP.Text = "";
				MasterUri.Text = "";
				ChangeMasterButton.Enabled = false;
			}

			//LoggedOnUsersCount.Text = "(Not Implemented)";
 			//SessionCount.Text = "(Not Implemented)";
 			//DiskSpaceUsed.Text = "(Not Implemented)";
 			//DiskSpaceAvailable.Text = "(Not Implemented)";
			//LdapStatus.Text = "(Not Implemented)"; //remoteweb.IdentitySyncGetServiceInfo ().Status;
 			//MaxConnectionCount.Text = "(Not Implemented)";

			return server.Name;
		}

		/// <summary>
                /// Sets the enabled status on all selected paths.
                /// </summary>
                /// <param name="status">If true then all selected paths will be enabled.</param>
                private void SetSelectedPathStatus( bool status )
                {
			iFolderServer server = web.GetServer( ServerID );

                        remoteweb.PreAuthenticate = true;
                        remoteweb.Credentials = web.Credentials;

                        remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
                        foreach( string name in CheckedPaths.Keys )
                        {
					remoteweb.ModifyDataStore( name , status);
			}
			// Clear the checked members.
                        CheckedPaths.Clear();
                        AllDataPathCheckBox.Checked = false;

                        // Set the action buttons.
                        SetActionButtons();
			
			// Rebind the data source with the new data.
                        GetDataPaths();
		}

		/// <summary>
                /// Delete a data path.
                /// </summary>
                private void DeleteDataPath()
                {
			iFolderServer server = web.GetServer( ServerID );

                        remoteweb.PreAuthenticate = true;
                        remoteweb.Credentials = web.Credentials;

                        remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
			foreach( string name in CheckedPaths.Keys )
			{
				remoteweb.DeleteDataStore( name );
			}
			// Clear the checked members.
                        CheckedPaths.Clear();
                        AllDataPathCheckBox.Checked = false;

			// Set the action buttons.
                        SetActionButtons();

                        // Rebind the data source with the new data.
                        GetDataPaths();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns>The name of the host node.</returns>
		private void GetLdapDetails()
		{
		    //Pick the information from SyncService
		        SyncServiceInfo syncInfo = remoteweb.IdentitySyncGetServiceInfo();
			LdapUpSince.Text = syncInfo.UpSince;
			LdapCycles.Text = syncInfo.Cycles.ToString();
			LdapStatus.Text = syncInfo.Status;
			LdapDeleteGraceInterval.Text = ( syncInfo.DeleteMemberGracePeriod / 60).ToString();
			IDSyncInterval.Text = (syncInfo.SynchronizationInterval / 60).ToString();

		    //Pick information from IdentityProvider
			LdapInfo ldapInfo = remoteweb.GetLdapDetails();
			LdapServer.Text = ldapInfo.Host ;
			LdapSearchContext.Value = ldapInfo.SearchContexts;
			LdapProxyUser.Text = ldapInfo.ProxyDN;
			LdapProxyUserPwd.Text = ldapInfo.ProxyPassword; 
			LdapSsl.Text = ldapInfo.SSL ? GetString ("YES") : GetString ("NO");
		}

		//private void GetTailData()
		//{
		//	StringWriter sw = new StringWriter();
		//	try
		//	{
		//		Server.Execute( "LogTailHandler.ashx?Simias.log&lines=30", sw );
		//		LogText.Text = sw.ToString();
		//	}
		//	finally
		//	{
		//		sw.Close();
		//	}
		//}

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

			remoteweb = new iFolderAdmin ();

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			string userID = Session[ "UserID" ] as String;
			if(userID != null && ServerID != null && ServerID != String.Empty)
				sysAccessPolicy = web.GetUserSystemRights(userID, ServerID);
			else
				sysAccessPolicy = 0; 
			uRights = new UserSystemAdminRights(sysAccessPolicy);
			if(uRights.ServerPolicyManagementAllowed == false)
				Page.Response.Redirect(String.Format("Error.aspx?ex={0}&Msg={1}",GetString( "ACCESSDENIED" ), GetString( "ACCESSDENIEDERROR" )));

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				ViewReportButton.Text = GetString( "VIEW" );
				ViewLogButton.Text = GetString( "VIEW" );
				LogLevelButton.Text = GetString( "SET" );
				LogLevelLabel.Text = GetString( "LOGLEVELTAG" );
				LogLabel.Text = GetString( "LOGTAG" );
 				SaveServerDetailsButton.Text = GetString( "SAVE" );
 				CancelServerDetailsButton.Text = GetString( "CANCEL" );
 				SaveLdapDetailsButton.Text = GetString( "SAVE" );
 				CancelLdapDetailsButton.Text = GetString( "CANCEL" );
 				LdapEditButton.Text = GetString( "EDIT" );
 				LdapEditButton.Enabled = true; 
				SyncNowButton.Text = GetString ("SYNCNOW");
				ChangeMasterButton.Text = GetString ("CHANGEMASTER");

                                DisableButton.Text = GetString( "DISABLE" );
				DeleteButton.Text = GetString( "DELETE" );
                                EnableButton.Text = GetString("ENABLE");
                                AddButton.Text = GetString( "ADD" );
				AllDataPathCheckBox.Checked = false;
				CheckedPaths = new Hashtable();
				CurrentPathOffset = 0;
				TotalPaths = 0;

 				//TODO : Future!
 			        //LoggedOnUsersCount.Visible = false;
 			        //SessionCount.Visible = false;
 				//DiskSpaceUsed.Visible = false;
 				//DiskSpaceAvailable.Visible = false;
 				//MaxConnectionCount.Visible = false;

				// Initialize state variables.
			}
		}

		private void Page_Unload(object sender, System.EventArgs e)
		{
		        web.Url = currentServerURL;
		}

		/// <summary>
                /// Gets the DataPaths.
                /// </summary>
                private void GetDataPaths()
                {
                        DataPaths.DataSource = CreateDataPathList();
                        DataPaths.DataBind();
			SetPageButtonState();
                }

		/// <summary>
                /// Creates a list of DataPaths
                /// </summary>
                /// <returns>A DataView object containing the datapath list.</returns>
                private DataView CreateDataPathList()
                {
                        DataTable dt = new DataTable();
                        DataRow dr;

	
			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "DisabledField", typeof( string ) ) );
                        dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
                        dt.Columns.Add( new DataColumn( "FullPathField", typeof( string ) ) );
                        dt.Columns.Add( new DataColumn( "FreeSpaceField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "StatusField", typeof( string ) ) );

                        iFolderServer server = web.GetServer( ServerID );

                        remoteweb.PreAuthenticate = true;
                        remoteweb.Credentials = web.Credentials;

                        remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
			try
			{
	                        VolumesList volumelist = remoteweb.GetVolumes(CurrentPathOffset , DataPaths.PageSize);
				TotalPaths = volumelist.NumberOfVolumes;
				foreach( Volumes mntpt in volumelist.ItemsArray )
                	       	{
                        	        dr = dt.NewRow();
					String result;
	                                dr[ 0 ] = true;
					dr[ 1 ] = !mntpt.Enabled;
					dr[ 2 ] = mntpt.DataPath;
                                        dr[ 3 ] = mntpt.FullPath;
					if( mntpt.AvailableFreeSpace  == 0 )
					{
						dr [ 4 ] = rm.GetString("NOTMOUNTED");
					}
					else
					{
						result = Utils.ConvertToUnitString( mntpt.AvailableFreeSpace,true, rm ); 
						dr[ 4 ] = result;
					}
					dr[ 5 ] = GetString( mntpt.Enabled ? "YES" : "NO" );
	                                dt.Rows.Add( dr );
	
        	                }
			}
			catch
			{}

	
                        for ( int RowCount = dt.Rows.Count; RowCount < DataPaths.PageSize; ++RowCount )
                        {
                                dr = dt.NewRow();
                                dr[ 0 ] = false;
                                dr[ 1 ] = false;
                                dr[ 2 ] = String.Empty;
                                dr[ 3 ] = String.Empty;
				dr[ 4 ] = String.Empty;
				dr[ 5 ] = String.Empty;

                                dt.Rows.Add( dr );
                        }

			return new DataView( dt );
                }

		/// <summary>
                /// Sets the page button state of the Accounts list.
                /// </summary>
                private void SetPageButtonState()
                {
                        DataPathsFooter.SetPageButtonState(
                                DataPaths,
                                CurrentPathOffset,
                                TotalPaths,
                                GetString( "VOLUMES" ),
                                GetString( "VOLUME" ) );
                }

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender( object sender, EventArgs e )
		{
//                        TopNav.ShowInfo (String.Format("Exception- {0}", web.Url));
			string serverName = GetServerDetails();

			BuildBreadCrumbList( serverName );

			if(serverStatus)
			{
				GetReportList();
				GetLogList();
				GetLdapDetails ();
				GetDataPaths();
			}

			DeleteButton.Attributes["onclick"] = "return ConfirmDeletion();";
			//TODO : future!
			//GetTailData();
		}

		#endregion

		#region Protected Methods

		/// <summary>
                /// Event handler that gets called when the path check box is checked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void OnPathChecked( object source, EventArgs e )
                {
                        // Get the data grid row for this member.
                        CheckBox checkBox = source as CheckBox;
                        DataGridItem item = checkBox.Parent.Parent as DataGridItem;
                        string name = item.Cells[ 2 ].Text;
                        if ( name != "&nbsp;" )
                        {
                                // User is being added.
                                if ( checkBox.Checked )
                                {
                                        CheckedPaths[ name ] = item.Cells[ 0 ].Text == Boolean.FalseString;
                                }
                                else
                                {
                                        // Remove this ifolder from the list.
                                        CheckedPaths.Remove( name );
                                }
                        }

                        // Set the user action buttons.
                        SetActionButtons(); 
                }


		/// <summary>
                /// Returns the checked state for the specified member.
                /// </summary>
                /// <param name="name">name of the datapath</param>
                /// <returns>True if datapath is checked.</returns>
                protected bool GetMemberCheckedState( Object name )
                {
                        return CheckedPaths.ContainsKey( name ) ? true : false;
                }

		/// <summary>
                /// Returns the friendly string for DataPath 
                /// </summary>
                /// <param name="name">name/path of the datapath</param>
		/// <param name="len">name of the datapath</param>
                /// <returns>Friendly String</returns>
                protected string GetFriendlyString( Object name , int len)
                {
			string str = (string)name;
			if( str != null )
			{
				if( str.Length > len )
                                        str = web.GetShortenedName(str, len);
			}
                        return str;
                }

		/// <summary>
                /// Gets whether the path should be able to be checked.
                /// </summary>
                /// <param name="path"></param>
                /// <returns>True if the path is allowed to be checked.</returns>
                protected bool IsPathEnabled( object name )
                {
                	return IsPathDefault( name as string );       
                }
		
		protected bool IsPathDefault( string name )
		{	
			if( name.Equals("Default-Store") == true)
			return false;
				return true;
		}

		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void DownloadFile( string url, string fileName )
		{
		        string fileurl = url + fileName;
			WebClient wc = new WebClient ();
 			wc.Credentials = web.Credentials;

			Byte[] fileData = null;
			fileData =  wc.DownloadData ( fileurl );

			Response.Clear();
			Response.Cache.SetCacheability( HttpCacheability.NoCache );
			Response.BufferOutput = false;
			Response.AddHeader( 
					"Content-Disposition", 
					String.Format("attachment; filename={0}", fileName ) );
			Response.ContentType = "text/plain";
			Response.AddHeader("Content-Length", fileData.Length.ToString() );
			Response.OutputStream.Write( fileData , 0, fileData.Length );

			Response.Close();
		}

		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void DownloadLogFile( string fileName )
		{
			Response.Redirect(fileName);
		}

		/// <summary>
		/// Event handler that gets called when the ViewLog Button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void LogLevelButtonClicked( object sender, EventArgs e )
		{
			LoggerType loggerType = LoggerType.RootLogger;
			string logLevel = "INFO";

			if ( LogList.SelectedValue == GetString( "ACCESSLOGFILE" ) )
			{
			        loggerType = LoggerType.AccessLogger;
			}
			else
			{
			        loggerType = LoggerType.RootLogger;
			}

			logLevel = LogLevelList.SelectedValue;

                        iFolderServer server = web.GetServer ( ServerID);
                        remoteweb.PreAuthenticate = true;
                        remoteweb.Credentials = web.Credentials;
                        remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
                        remoteweb.GetAuthenticatedUser();
			remoteweb.SetLogLevel (loggerType, logLevel.ToUpper());
		}

		/// <summary>
		/// Event that gets called when the save button for server details is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnSaveServerDetailsButton_Click( object source, EventArgs e )
		{
			try
			{
				string MastersIP = "";
				iFolderServer server = web.GetServer ( ServerID); 

				remoteweb.PreAuthenticate = true;
				remoteweb.Credentials = web.Credentials;

				remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";

				redirectUrl = server.PublicUrl;
				remoteweb.GetAuthenticatedUser();
				server = remoteweb.GetServer ( ServerID);

				if(! server.IsMaster)
				{
					MastersIP = MasterIP.Text.Trim();
				}
				log.Info("remote web = {0}", remoteweb.Url);
				bool ReturnStatus = remoteweb.SetIPDetails(PrivateIP.Text.Trim() , PublicIP.Text.Trim(), MastersIP);
				if (ReturnStatus == true)
				{
					TopNav.ShowInfo(GetString("RESTARTSERVER"));
					return;
				}
				else
				{
					TopNav.ShowInfo(GetString("UNABLETOEDITIPDETAILS"));
					return;
				}
			}
			catch
			{
				TopNav.ShowError(GetString("UNABLETOEDITIPDETAILS"));
				return;
			}
		}

		/// <summary>
		/// Event that gets called when the save button for server details is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnCancelServerDetailsButton_Click( object source, EventArgs e )
		{
			GetServerDetails();
		}
	
		/// <summary>
		/// Event that gets called when the save button for ldap details is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnSaveLdapDetailsButton_Click( object source, EventArgs e )
		{
			iFolderServer server = web.GetServer ( ServerID); 
			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;
			remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
			    
			redirectUrl = server.PublicUrl;
			remoteweb.GetAuthenticatedUser();
			server = remoteweb.GetServer ( ServerID);
			try
			{
			 	int syncInterval = Int32.Parse (IDSyncInterval.Text);
	                        syncInterval = syncInterval * 60;
			
                        	if (syncInterval >= 300 && syncInterval <= 604800 ) 
				{
                                	remoteweb.IdentitySyncSetInterval (syncInterval);
                        	}
				else
				{
                                	TopNav.ShowError( GetString ("ERRORINVALIDIDSYNCINTERVAL"));
                        	}	
				
				int deleteGracePeriod = Int32.Parse (LdapDeleteGraceInterval.Text);
        	                deleteGracePeriod = deleteGracePeriod * 60;
                	        if (deleteGracePeriod >= 300 && deleteGracePeriod <= 604800) 
				{
                        	        remoteweb.IdentitySyncSetDeleteMemberGracePeriod (deleteGracePeriod);
		                } 
				else 
				{
                	                TopNav.ShowError( GetString ("ERRORINVALIDIDSYNCINTERVAL"));
                        	}
				SaveLdapDetailsButton.Enabled = CancelLdapDetailsButton.Enabled = false;
			}
			catch
			{
				TopNav.ShowError( GetString ("ERRORINVALIDIDSYNCINTERVAL"));
				return;
			}
		}

		/// <summary>
		/// Event that gets called when the save button for ldap details is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnCancelLdapDetailsButton_Click( object source, EventArgs e )
		{
			GetServerDetails();	
			GetLdapDetails();
			SaveLdapDetailsButton.Enabled = CancelLdapDetailsButton.Enabled = false;

		}
		
		/// <summary>
		/// Event that gets called when the save button is clicked. ... currently this is called as Edit button
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnLdapEditButton_Click( object source, EventArgs e )
		{
		    
			iFolderServer server = web.GetServer( ServerID );
			string ServerName = server.Name;
			Response.Redirect(String.Format("LdapAdminAuth.aspx?ServerID={0}&serverName={1}",ServerID,ServerName));

		}

		protected void ChangeMaster_PreRender(object source, EventArgs e)
		{
			// TODO : get this stuff from a resource 
			// GetString("CONFIRMCHANGEMASTER")
			ChangeMasterButton.Attributes["onclick"] = "javascript:return confirm('The selected server will be designated as a Master Server. Do you want to continue? Click OK to continue or Cancel to quit.');";
		}

		/// <summary>
		/// Event that gets called when the ChangeMasterButton is clicked 
		/// Here all the calls are made to respective master and slave servers
		/// to update the required details to turn it into new Master or new
		/// Slave. After this all other servers are updated with new master
		/// server information.
		/// TODO: ldapcontext info update could be clubbed here, but at this
		/// moment its opted out. Admin has to update the context once this
		/// operation is successful
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnChangeMasterButton_Click( object source, EventArgs e )
		{
			log.Info("Change Master Server process Initiated");
			bool currentMasterUpdateComplete = false, newMasterUpdateComplete = false;

			string HostID = null, newServerPublicUrl = null;
			iFolderServer mServer = null, newmServer=null;
			try
			{
				try
				{
					//check server ID here 
					newmServer = web.GetServer(ServerID);
					mServer = web.GetMasterServer();
				}
				catch(Exception ex)
				{
					// for some unknow reason first call fails randomly...so tring
					// it again. 
					log.Info("Caught exception while trying to get Master server and slected server, retrying");
					newmServer = web.GetServer(ServerID);
					mServer = web.GetMasterServer();
				}

				if ( newmServer != null && mServer != null && 
						ServerID != null && (newServerPublicUrl = newmServer.PublicUrl) != null)
				{
					log.Info("New Master Server ID = {0}", ServerID);
					log.Info("Current Master Server Url : {0}", mServer.PublicUrl);
					log.Info("New Master Server Url : {0}", newmServer.PublicUrl);

					// First Set current Master server to Slave
					iFolderAdmin currentMasterServer = new iFolderAdmin();
					currentMasterServer.PreAuthenticate = true;
					currentMasterServer.Credentials = web.Credentials;
					currentMasterServer.Url = mServer.PublicUrl + "/iFolderAdmin.asmx";
					log.Info("Current Master Server admin service Url = {0}", currentMasterServer.Url);

					log.Info("Setting as Slave server...");
					try 
					{
						if( !currentMasterServer.SetAsSlaveServer(ServerID, newServerPublicUrl))
						{
							log.Info("Unable to set the server as slave, retry");
							TopNav.ShowError(GetString("UNABLETOSETASSLAVE"));
							return;
						}
					} 
					catch(WebException ex)
					{
						log.Info("Exception while SetAsSlaveServer");
						HttpWebResponse htpw = (HttpWebResponse)(ex.Response);
						if(ex.Status == WebExceptionStatus.ProtocolError)
						{
							TopNav.ShowError(String.Format("WebException {0}", htpw.StatusDescription));
						}
						if(ex.Status == WebExceptionStatus.SendFailure)
						{
							UriBuilder turl = new UriBuilder(mServer.PublicUrl);
							if(turl.Scheme == Uri.UriSchemeHttps)  //check it should be http
							{
								turl.Scheme = Uri.UriSchemeHttp;
								// non standard port does not work
								turl.Port = 80;
							}
							else
							{
								turl.Scheme = Uri.UriSchemeHttps;
								turl.Port = 443;
							}
							currentMasterServer.Url = turl.ToString();
							redirectUrl = currentMasterServer.Url;
							currentMasterServer.Url = currentMasterServer.Url + "/iFolderAdmin.asmx";

							try 
							{
								if( !currentMasterServer.SetAsSlaveServer(ServerID, newServerPublicUrl))
								{
									log.Info("Unable to set the server as slave, retry");
									TopNav.ShowError(GetString("UNABLETOSETASSLAVE"));
									return;
								}
							}
							catch{
								log.Info("Exception while SetAsSlave");
								return;
							}
						}
					}

					currentMasterUpdateComplete = true;
					log.Info("Set as Slave Server Complete.");

					log.Info("Setting selected server as Master Server...");
					// Then, Set the New Master Server
					iFolderAdmin newMasterServer = new iFolderAdmin ();
					newMasterServer.PreAuthenticate = true;
					newMasterServer.Credentials = web.Credentials;
					newMasterServer.Url = newServerPublicUrl + "/iFolderAdmin.asmx";
					log.Info("New Master Server admin service Url = {0}", newMasterServer.Url);
					try 
					{
						if(!newMasterServer.SetAsMasterServer(ServerID))
						{
							log.Info("Unable to set the server as Master, retry");
						}
					}
					catch (WebException ex)
					{
						log.Info("Exception while SetAsMasterServer");
						HttpWebResponse htpw = (HttpWebResponse)(ex.Response);
						if(ex.Status == WebExceptionStatus.ProtocolError)
						{
							TopNav.ShowInfo (String.Format("WebException {0}", htpw.StatusDescription));
						}
						if(ex.Status == WebExceptionStatus.SendFailure)
						{
							UriBuilder turl = new UriBuilder(newmServer.PublicUrl);
							// non standard port does not work ....
							if(turl.Scheme == Uri.UriSchemeHttps)
							{
								turl.Scheme = Uri.UriSchemeHttp;
								turl.Port = 80;
							}
							else
							{
								turl.Scheme = Uri.UriSchemeHttps;
								turl.Port = 443;
							}
							newMasterServer.Url = turl.ToString();
							redirectUrl = newMasterServer.Url;
							newMasterServer.Url = newMasterServer.Url + "/iFolderAdmin.asmx";
							try
							{
								newMasterServer.SetAsMasterServer(ServerID);
							}
							catch
							{
								TopNav.ShowError(String.Format (GetString("CONNECTFAILED"), newMasterServer.Url));
							}
						}
					}

					newMasterUpdateComplete = true;

					// Master and Slave updated, now set the Master node attribute for new Master host on both
					// current master and new master
					if ( currentMasterServer.SetMasterNodeAttribute(newmServer.ID, true ))
					{
						int count = 0;
						while (newMasterServer.GetMasterNodeAttribute(newmServer.ID) != true )
						{
							TopNav.ShowInfo(GetString("MASTERNODEUPDATEWAIT"));
							log.Info("Waiting for master node attrituge to sync acress old and new master server");
							Thread.Sleep(1000);
							count ++;
							if (count > 15)
							{
								log.Info("Unable to get Master node attribute from the new master server, you may have to retry the operation");
								//break;  ///come out from here .....
								TopNav.ShowError(String.Format("CHANGEMASTERRETRY"));
								return;
							}
						}
						log.Info("Set as Master Server Complete");
					}

					//Master and Slave updated and set, now we will let all other slaves
					//know about the changes.
					iFolderServer[] iFolderServers = web.GetServers();
					foreach(iFolderServer ifs in iFolderServers)
					{
						if ( ifs.HostName != newmServer.HostName)
						{
							log.Info("Updating new master url on : {0}", ifs.PublicUrl);
							iFolderAdmin otherSlaveServers = new iFolderAdmin();
							otherSlaveServers.PreAuthenticate = true;
							otherSlaveServers.Credentials = web.Credentials;
							otherSlaveServers.Url = ifs.PublicUrl + "/iFolderAdmin.asmx";
							log.Info("Connecting to : {0}", otherSlaveServers.Url);
							try
							{
								otherSlaveServers.SetMasterServerUrl(ServerID, newServerPublicUrl);
							}catch(Exception ex)
							{
								log.Info("Update master serverurl on {0} failed ", ifs.PublicUrl);
								TopNav.ShowError(String.Format(GetString("UPDATEMASTERURLONSLAVESFAILED"), ifs.PublicUrl));
							}
							// collect all failed url and print once.
						}
					}
					//refresh server deatils
					//GetServerDetails();
					//disable the button so the user does not click it again.
					ChangeMasterButton.Enabled = false;
					TopNav.ShowInfo (String.Format (GetString ("CHANGEMASTERSUCCESSFUL"), newServerPublicUrl));
				}
				else
				{
					log.Info(String.Format ("Unable to get new Master ServerID and newServerPublicUrl, retry"));
					TopNav.ShowError(GetString("CHANGEMASTERINFOFAILED"));
					ChangeMasterButton.Enabled = false;
				}
			}
			finally
			{
				try
				{
					//this is just to get all the changes done on both the
					//servers. does nothing other than logging. Will be useful
					//for debuggind in case of any error
					web.VerifyChangeMaster(mServer.ID, newmServer.ID);
				}
				catch{}
				log.Info("Change Master Server process Complete");
			}
			return;
		}

		/// <summary>
		/// Event that gets called when the SyncNow button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnSyncNowButton_Click( object source, EventArgs e )
		{
			iFolderServer server = web.GetServer ( ServerID);
			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;
			remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
			remoteweb.GetAuthenticatedUser();
			remoteweb.IdentitySyncNow ();
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
                /// Event handler that gets called when the check all Datapath checkbox is selected.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void OnAllDataPathChecked( object source, EventArgs e )
                {
			CheckBox checkBox = source as CheckBox;
                        foreach( DataGridItem item in DataPaths.Items )
                        {
                                // In order to be checked, the row must not be empty.
                                string name = item.Cells[ 2 ].Text;

                                if ( name != "&nbsp;" && IsPathDefault( name ) )
                                {
                                        if ( checkBox.Checked )
                                        {
                                                CheckedPaths[ name ] = item.Cells[ 0 ].Text == Boolean.FalseString;
                                        }
                                        else
                                        {
                                                // Remove this user from the list.
                                                CheckedPaths.Remove( name );
                                        }
                                }
                        }

                        // Set the action buttons appropriately.
                        SetActionButtons();

                        // Rebind the data source with the new data.
			GetDataPaths();
		}

		/// <summary>
                /// Event handler that gets called when the Add DataPath button is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void OnAddButton_Click( object source, EventArgs e )
                {
			string serverName = GetServerDetails();
			Response.Redirect(String.Format("AddDataPath.aspx?ServerID={0}&serverName={1}",ServerID,serverName));
                }

		/// <summary>
                /// Event handler that gets called with the disable datapath button is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void OnDisableButton_Click( object source, EventArgs e )
                {
			 SetSelectedPathStatus( false );
                }

		/// <summary>
                /// Event handler that gets called with the delete datapath button is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void OnDeleteButton_Click( object source, EventArgs e )
                {
			DeleteDataPath();
                }

                /// <summary>
                /// Event handler that gets called with the enable datapath button is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void OnEnableButton_Click( object source, EventArgs e )
                {
			SetSelectedPathStatus( true );
                }

		/// <summary>
                /// Event that first when the PageFirstButton is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void PageFirstButton_Click( object source, ImageClickEventArgs e)
                {
                        // Set to get the first users.
                        CurrentPathOffset = 0;

                        // Rebind the data source with the new data.
                        DataPaths.DataSource = CreateDataPathList();
                        DataPaths.DataBind();
                        // Set the button state.
                       SetPageButtonState();;
               }

                /// <summary>
                /// Event that first when the PagePreviousButton is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void PagePreviousButton_Click( object source, ImageClickEventArgs e)
               {
                       CurrentPathOffset -= DataPaths.PageSize;
                        if ( CurrentPathOffset < 0 )
                        {
                                CurrentPathOffset = 0;
                        }

                        // Rebind the data source with the new data.
                        DataPaths.DataSource = CreateDataPathList();
                        DataPaths.DataBind();

                        // Set the button state.
                        SetPageButtonState();;
                }

                /// <summary>
                /// Event that first when the PageNextButton is clicked.
                /// </summary>
        	/// <param name="source"></param>
                /// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
                {
                        CurrentPathOffset += DataPaths.PageSize; 

                        // Rebind the data source with the new data.
                        DataPaths.DataSource = CreateDataPathList();
                        DataPaths.DataBind();

                        // Set the button state.
                        SetPageButtonState();;
                }

                /// <summary>
                /// Event that first when the PageLastButton is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void PageLastButton_Click( object source, ImageClickEventArgs e)
                {
                        CurrentPathOffset = ( ( TotalPaths - 1 ) / DataPaths.PageSize ) * DataPaths.PageSize;

                        // Rebind the data source with the new data.
                        DataPaths.DataSource = CreateDataPathList();
                        DataPaths.DataBind();

                        // Set the button state.
                        SetPageButtonState();;
                }

		/// <summary>
		/// Event handler that gets called when the ViewReport Button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ViewReportFile( object sender, EventArgs e )
		{
			string reportFileName = ReportList.SelectedValue;

			// Send a request to the report/log file handler.
			// TODO : use the public url of the system we are displaying and redirect there .
			if (reportFileName != null && reportFileName != string.Empty )
			{
			       string fileurl = web.GetServer( ServerID ).PrivateUrl + "/admindata/";
			       DownloadFile (fileurl, reportFileName);
			}

		}

		/// <summary>
		/// Event handler that gets called when the ViewLog Button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ViewLogFile( object sender, EventArgs e )
		{
			string logFileName = "Simias.log";
			if ( LogList.SelectedValue == GetString( "WEBACCESSLOGFILE" ) )
			{
				logFileName = "webaccess.log";
			}
			else if ( LogList.SelectedValue == GetString( "ACCESSLOGFILE" ) )
			{
				logFileName = "Simias.access.log";
			}
			else if ( LogList.SelectedValue == GetString( "ADMINLOGFILE" ) )
			{
				logFileName = "adminweb.log";
			}

			// Send a request to the log file handler.
			DownloadLogFile (logFileName);

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
			DataPathsFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			DataPathsFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			DataPathsFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			DataPathsFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			this.Load += new System.EventHandler(this.Page_Load);
			this.Unload += new System.EventHandler (this.Page_Unload);

			this.SyncNowButton.Click += new System.EventHandler(this.OnSyncNowButton_Click);
			this.ChangeMasterButton.PreRender += new EventHandler(this.ChangeMaster_PreRender);
			this.ChangeMasterButton.Click += new System.EventHandler(this.OnChangeMasterButton_Click);
			this.ViewLogButton.Click += new System.EventHandler(this.ViewLogFile);
		}
		#endregion
	}
}
