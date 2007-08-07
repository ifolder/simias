/****************************************************************************
 |
 | Copyright (c) [2007] Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com

 | Author: Mike Lasky (mlasky@novell.com)
 |***************************************************************************/


using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
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
	/// Summary description for ServerDetails.
	/// </summary>
	public class ServerDetails : System.Web.UI.Page
	{
		#region Class Members

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
		/// Server public ip address control.
		/// </summary>
		protected Literal PublicIP;

		/// <summary>
		/// Server private ip address control.
		/// </summary>
		protected Literal PrivateIP;


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
		protected Literal LdapProxyUser;

		/// <summary>
		/// LDAP SSL
		/// </summary>
		protected Literal LdapSsl;

		/// <summary>
		/// External Identity Sync Interval
		/// </summary>
		protected TextBox LdapSearchContext;

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
		protected Button SyncNowButton;

		/// <summary>
		/// LDAP cancel button control.
		/// </summary>
		protected Button CancelButton;

		/// <summary>
		/// LDAP save button control.
		/// </summary>
		protected Button SaveButton;

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
		protected Button ViewLogButton;

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


		protected string currentServerURL;
	
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
                        EnableButton.Enabled = ht.ContainsValue( true );
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
			catch (Exception e) 
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
			iFolderServer server = web.GetServer( ServerID );

			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;

		        remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
			    
			redirectUrl = server.PublicUrl;

			Status.Text = "Online";
			LdapStatus.Text = "N/A";
			iFolderCount.Text = "N/A";
			DnsName.Text = "N/A";
			UserCount.Text = "N/A";
			Name.Text = server.Name;
 
			try 
			{
			        remoteweb.GetAuthenticatedUser();
				server = remoteweb.GetServer ( ServerID);
				DnsName.Text = server.HostName;
				UserCount.Text = server.UserCount.ToString();
 
				iFolderSet ifolders = remoteweb.GetiFolders( iFolderType.All, 0, 1 );
				iFolderCount.Text = ifolders.Total.ToString();

				LdapStatus.Text = remoteweb.IdentitySyncGetServiceInfo ().Status;
			}
			catch ( Exception e)
			{
			        remoteweb = web;
			        Status.Text = "<font color=red><b>Offline</b></font>";
				TopNav.ShowInfo (String.Format ("Unable to reach {0}. Displaying minimal information", Name.Text));
			}

			Name.Text = server.Name;
			Type.Text = GetString( server.IsMaster ? "MASTER" : "SLAVE" );
			PublicIP.Text = server.PublicUrl;
			PrivateIP.Text = server.PrivateUrl;

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
                        foreach( string name in CheckedPaths.Keys )
                        {
					web.ModifyDataStore( name , status);
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
			LdapDeleteGraceInterval.Text = ( syncInfo.DeleteMemberGracePeriod / 60).ToString();
			IDSyncInterval.Text = (syncInfo.SynchronizationInterval / 60).ToString();

		    //Pick information from IdentityProvider
			LdapInfo ldapInfo = remoteweb.GetLdapDetails();
			LdapServer.Text = ldapInfo.Host ;
			LdapSearchContext.Text = ldapInfo.SearchContexts;
			LdapProxyUser.Text = ldapInfo.ProxyDN;
//			LdapSsl.Text = ldapInfo.SSL ? GetString ("YES") : GetString ("NO");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>The name of the host node.</returns>
		private void SetLdapDetails()
		{
		    //Pick the information from SyncService
		        SyncServiceInfo syncInfo = remoteweb.IdentitySyncGetServiceInfo();
			LdapUpSince.Text = syncInfo.UpSince;
			LdapCycles.Text = syncInfo.Cycles.ToString();

		    //Pick information from IdentityProvider
			LdapInfo ldapInfo = remoteweb.GetLdapDetails();
			LdapServer.Text = ldapInfo.Host ;
			LdapSearchContext.Text = ldapInfo.SearchContexts;
			LdapProxyUser.Text = ldapInfo.ProxyDN;
			LdapSsl.Text = ldapInfo.SSL ? GetString ("YES") : GetString ("NO");
		}


		private void GetTailData()
		{
			StringWriter sw = new StringWriter();
			try
			{
				Server.Execute( "LogTailHandler.ashx?Simias.log&lines=30", sw );
				LogText.Text = sw.ToString();
			}
			finally
			{
				sw.Close();
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

			remoteweb = new iFolderAdmin ();

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				ViewReportButton.Text = GetString( "VIEW" );
				ViewLogButton.Text = GetString( "VIEW" );
				LogLevelButton.Text = GetString( "SET" );
				LogLevelLabel.Text = GetString( "LOGLEVELTAG" );
				LogLabel.Text = GetString( "LOGTAG" );
 				SaveButton.Text = GetString( "SAVE" );
 				CancelButton.Text = GetString( "CANCEL" );
				SyncNowButton.Text = GetString ("SYNCNOW");

				ArrayList options = new ArrayList();

				options.Add( GetString( "YES" ) );
				options.Add( GetString( "NO" ) );

				LdapSslList.DataSource = options;
				LdapSslList.DataBind();

                                DisableButton.Text = GetString( "DISABLE" );
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
			
			VolumesList volumelist = web.GetVolumes(CurrentPathOffset , DataPaths.PageSize);
			foreach( Volumes mntpt in volumelist.ItemsArray )
                       	{
                                dr = dt.NewRow();
				String result;
                                dr[ 0 ] = true;
				dr[ 1 ] = false;
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

			TotalPaths = volumelist.NumberOfVolumes;
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
			string serverName = GetServerDetails();

			BuildBreadCrumbList( serverName );

			GetReportList();
			GetLogList();
			GetLdapDetails ();
			GetDataPaths();

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
                /// Gets whether the path should be able to be checked.
                /// </summary>
                /// <param name="path"></param>
                /// <returns>True if the path is allowed to be checked.</returns>
                protected bool IsPathEnabled( object name )
                {
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

			web.SetLogLevel (loggerType, logLevel.ToUpper());
		}

		/// <summary>
		/// Event that gets called when the save button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnSaveButton_Click( object source, EventArgs e )
		{
		    
		        LdapInfo ldapInfo = new LdapInfo ();
			ldapInfo.Host = LdapServer.Text;
			ldapInfo.SearchContexts = LdapSearchContext.Text;
			LdapProxyUser.Text = ldapInfo.ProxyDN;
			ldapInfo.SSL = (LdapSslList.SelectedValue == GetString("YES")) ? true : false;

			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;

		        remoteweb.Url = PublicIP.Text + "/iFolderAdmin.asmx";
		        remoteweb.GetAuthenticatedUser();
			remoteweb.SetLdapDetails (ldapInfo);

		        int syncInterval = Int32.Parse (IDSyncInterval.Text);
			syncInterval = syncInterval * 60;
			if (syncInterval > 0 ) {
			        remoteweb.IdentitySyncSetInterval (syncInterval);
			} else {
			        TopNav.ShowError( GetString ("ERRORINVALIDSYNCINTERVAL"));
			}

		        int deleteGracePeriod = Int32.Parse (LdapDeleteGraceInterval.Text);
			deleteGracePeriod = deleteGracePeriod * 60;
			if (deleteGracePeriod > 0 ) {
			        remoteweb.IdentitySyncSetDeleteMemberGracePeriod (deleteGracePeriod);
			} else {
			        TopNav.ShowError( GetString ("ERRORINVALIDSYNCINTERVAL"));
			}

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

                                if ( name != "&nbsp;" )
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
		        string fileurl = web.GetServer( ServerID ).PrivateUrl + "/admindata/";
			DownloadFile (fileurl, logFileName);

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
		}
		#endregion
	}
}
