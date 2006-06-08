/***********************************************************************
 *  $RCSfile: Servers.aspx.cs,v $
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
	/// Summary description for Server.
	/// </summary>
	public class Server : System.Web.UI.Page
	{
		#region Class Members

		/// <summary>
		/// iFolder list data grid column indices.
		/// </summary>
		private const int ServerIDColumn = 0;
		private const int ServerTypeColumn = 1;
		private const int ServerNameColumn = 2;

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
		/// iFolder list controls.
		/// </summary>
		protected DataGrid ServerList;

		/// <summary>
		/// iFolder list footer control.
		/// </summary>
		protected ListFooter ServerListFooter;


		/// <summary>
		/// Web Controls.
		/// </summary>
		protected iFolderSearch ServerSearch;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the current Server offset.
		/// </summary>
		private int CurrentServerOffset
		{
			get { return ( int )ViewState[ "CurrentServerOffset" ]; }
			set { ViewState[ "CurrentServerOffset" ] = value; }
		}

		/// <summary>
		/// Gets or sets the total number of Servers contained in
		/// the last search.
		/// </summary>
		private int TotalServers
		{
			get { return ( int )ViewState[ "TotalServers" ]; }
			set { ViewState[ "TotalServers" ] = value; }
		}

		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			TopNav.AddBreadCrumb( GetString( "SERVERS" ), null );
		}
	
		/// <summary>
		/// Creates a list of Servers
		/// </summary>
		/// <returns>A DataView object containing the Server list.</returns>
		private DataView CreateServerList()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "TypeField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "DnsField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "PublicUriField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "PrivateUriField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "StatusField", typeof( string ) ) );


			// Get the Server list.
			iFolderServerSet list = web.GetServersByName(
				ServerSearch.SearchOperation,
				( ServerSearch.SearchName == String.Empty ) ? "*" : ServerSearch.SearchName, 
				CurrentServerOffset, 
				ServerList.PageSize );

			foreach( iFolderServer server in list.Items )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = server.ID;
				dr[ 2 ] = GetString( server.IsMaster ? "MASTER" : "SLAVE" );
				dr[ 3 ] = server.Name;
				dr[ 4 ] = server.HostName;
				dr[ 5 ] = server.PublicUrl;
				dr[ 6 ] = server.PrivateUrl;
				dr[ 7 ] = "(Not Implemented)";		// BUGBUG!!!

				dt.Rows.Add( dr );
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < ServerList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = String.Empty;
				dr[ 3 ] = String.Empty;
				dr[ 4 ] = String.Empty;
				dr[ 5 ] = String.Empty;
				dr[ 6 ] = String.Empty;
				dr[ 7 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of servers.
			TotalServers = list.Total;

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// Gets the iFolder servers.
		/// </summary>
		private void GetServers()
		{
			// Create a data source containing the servers.
			ServerList.DataSource = CreateServerList();
			ServerList.DataBind();
			SetPageButtonState();
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

				// Initialize state variables.
				CurrentServerOffset = 0;
				TotalServers = 0;
			}
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender( object sender, EventArgs e )
		{
			BuildBreadCrumbList();
			GetServers();
		}

		/// <summary>
		/// Sets the page button state of the ifolder list.
		/// </summary>
		private void SetPageButtonState()
		{
			ServerListFooter.SetPageButtonState( 
				ServerList, 
				CurrentServerOffset, 
				TotalServers, 
				GetString( "SERVERS" ),
				GetString( "SERVER" ) );
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Returns the server image type for the type of server.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		protected string GetServerImage( object type )
		{
			return ( String.Compare( type as string, GetString( "MASTER" ) ) == 0 ) ? "images/Master.png" : "images/Slave.png";
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
