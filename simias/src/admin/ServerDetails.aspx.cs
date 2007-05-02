/***********************************************************************
 *  $RCSfile: ServerDetails.aspx.cs,v $
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

		#endregion

		#region Properties

		/// <summary>
		/// Gets the Server ID.
		/// </summary>
		private string ServerID
		{
			get { return Request.Params[ "ID" ]; } 
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
		}

		/// <summary>
		/// Gets the list of files to display in the log file list.
		/// </summary>
		private void GetLogList()
		{
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
		}

		/// <summary>
		/// Gets the list of reports to view from this server.
		/// </summary>
		private void GetReportList()
		{
			ArrayList reports = new ArrayList();
			
			string[] files = remoteweb.GetReports();    

			try {
			        foreach (string file in files)
				        reports.Add( file );

		        } catch (Exception e) {
				reports.Add ("N/A");
			}

			if (files.Length == 0) {
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
			remoteweb.GetAuthenticatedUser();
			    
			redirectUrl = server.PublicUrl;

			server = remoteweb.GetServer ( ServerID);

			Name.Text = server.Name;
			Type.Text = GetString( server.IsMaster ? "MASTER" : "SLAVE" );
			DnsName.Text = server.HostName;
			PublicIP.Text = server.PublicUrl;
			PrivateIP.Text = server.PrivateUrl;

			Status.Text = "(Not Implemented)";
			UserCount.Text = server.UserCount.ToString();

			iFolderSet ifolders = remoteweb.GetiFolders( iFolderType.All, 0, 1 );
			iFolderCount.Text = ifolders.Total.ToString();

			//LoggedOnUsersCount.Text = "(Not Implemented)";
 			//SessionCount.Text = "(Not Implemented)";
 			//DiskSpaceUsed.Text = "(Not Implemented)";
 			//DiskSpaceAvailable.Text = "(Not Implemented)";
			LdapStatus.Text = "(Not Implemented)"; //remoteweb.IdentitySyncGetServiceInfo ().Status;
 			//MaxConnectionCount.Text = "(Not Implemented)";

			return server.Name;
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

 				//TODO : Future!
 			        //LoggedOnUsersCount.Visible = false;
 			        //SessionCount.Visible = false;
 				//DiskSpaceUsed.Visible = false;
 				//DiskSpaceAvailable.Visible = false;
 				//MaxConnectionCount.Visible = false;

				// Initialize state variables.
			}
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

			//TODO : future!
			//GetTailData();
		}

		#endregion

		#region Protected Methods

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
			        Response.Redirect ( web.GetServer( ServerID ).PublicUrl + "/admindata/" +reportFileName );
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
			Response.Redirect( web.GetServer( ServerID ).PublicUrl + "/admindata/" + logFileName );
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
	}
}
