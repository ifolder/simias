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
using System.Resources;
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
		/// Report download button control.
		/// </summary>
		protected Button DownloadReport;

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
		/// Gets the list of reports to view from this server.
		/// </summary>
		private void GetReportList()
		{
			ArrayList reports = new ArrayList();
			
			reports.Add( "ifolder-MELDELL-20060606-233000.csv" );
			reports.Add( "ifolder-MELDELL-20060605-233000.csv" );
			reports.Add( "ifolder-MELDELL-20060604-233000.csv" );
			reports.Add( "ifolder-MELDELL-20060603-233000.csv" );
			reports.Add( "ifolder-MELDELL-20060602-233000.csv" );
			reports.Add( "ifolder-MELDELL-20060601-233000.csv" );
			reports.Add( "ifolder-MELDELL-20060631-233000.csv" );

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
			
			Name.Text = server.Name;
			Type.Text = GetString( server.IsMaster ? "MASTER" : "SLAVE" );
			DnsName.Text = server.HostName;
			PublicIP.Text = server.PublicUrl;
			PrivateIP.Text = server.PrivateUrl;

			Status.Text = "Up";
			UserCount.Text = "123";
			iFolderCount.Text = "1234";
			LoggedOnUsersCount.Text = "45";
			SessionCount.Text = "56";
			DiskSpaceUsed.Text = "560 MB";
			DiskSpaceAvailable.Text = "145032 MB";
			LdapStatus.Text = "Up";
			MaxConnectionCount.Text = "40";

			return server.Name;
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

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				DownloadReport.Text = GetString( "DOWNLOAD" );

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
