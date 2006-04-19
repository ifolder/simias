/***********************************************************************
 *  $RCSfile: TopNavigation.ascx.cs,v $
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
namespace Novell.iFolderWeb.Admin
{
	using System;
	using System.Collections;
	using System.Data;
	using System.Drawing;
	using System.Resources;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Web.Security;

	/// <summary>
	///		Summary description for TopNavigation.
	/// </summary>
	public class TopNavigation : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// Anchor controls.
		/// </summary>
		protected HtmlAnchor UserLink;

		/// <summary>
		/// Anchor controls.
		/// </summary>
		protected HtmlAnchor iFolderLink;

		/// <summary>
		/// Anchor controls.
		/// </summary>
		protected HtmlAnchor ServerLink;

		/// <summary>
		/// Anchor controls.
		/// </summary>
		protected HtmlAnchor SystemLink;

		/// <summary>
		/// Logout button control.
		/// </summary>
		protected LinkButton LogoutButton;

		/// <summary>
		/// Breadcrumb list control.
		/// </summary>
		protected Label BreadCrumbList;


		/// <summary>
		/// Error panel control.
		/// </summary>
		protected HtmlGenericControl ErrorPanel;

		/// <summary>
		/// Error message control.
		/// </summary>
		protected Label ErrorMsg;
        

		/// <summary>
		/// Breadcrumb control.
		/// </summary>
		protected DataList BreadCrumbs;


		/// <summary>
		/// Logged in as label control.
		/// </summary>
		protected Label LoggedInAs;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the bread crumb list.
		/// </summary>
		private ArrayList CrumbList
		{
			get { return ViewState[ "BreadCrumbs" ] as ArrayList; }
			set { ViewState[ "BreadCrumbs" ] = value; }
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Creates a data source of breadcrumbs.
		/// </summary>
		/// <returns>A DataView item containing breadcrumbs</returns>
		private DataView CreateDataSource()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "CrumbField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "LinkField", typeof( string ) ) );

			foreach( BreadCrumbInfo bci in CrumbList )
			{
				dr = dt.NewRow();
				dr[ 0 ] = bci.Crumb;
				dr[ 1 ] = bci.CrumbUri;
				dt.Rows.Add( dr );
			}

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// Logout
		/// </summary>
		/// <param name="type"></param>
		/// <param name="message"></param>
		private void Logout( string type, string message )
		{
			FormsAuthentication.SignOut();
			
			// double-check that the session is abandoned
			Session.Abandon();

			Response.Redirect( 
				String.Format( 
					"Login.aspx?MessageType={0}&MessageText={1}",
					Context.Server.UrlEncode( type ),
					Context.Server.UrlEncode( message ) ) );
		}

		/// <summary>
		/// Page_Load()
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;

			// Hide the error panel if previously visible.
			if ( ErrorPanel.Visible )
			{
				ErrorPanel.Visible = false;
			}

			if ( !IsPostBack )
			{
				// Show the currently logged in user.
				LoggedInAs.Text = String.Format( GetString( "LOGGEDINAS" ), Session[ "NAME" ] );

				// Initially hide the error panel.
				ErrorPanel.Visible = false;

				LogoutButton.Text = GetString( "LOGOUT" );

				Control body = Page.FindControl( "users" );
				if ( body != null )
				{
					UserLink.HRef = String.Empty;
					iFolderLink.HRef = "iFolders.aspx";
					ServerLink.HRef = "Servers.aspx";
					SystemLink.HRef = "SystemInfo.aspx";
				}
				else
				{
					body = Page.FindControl( "ifolders" );
					if ( body != null )
					{
						UserLink.HRef = "Users.aspx";
						iFolderLink.HRef = String.Empty;
						ServerLink.HRef = "Servers.aspx";
						SystemLink.HRef = "SystemInfo.aspx";
					}
					else
					{
						body = Page.FindControl( "system" );
						if ( body != null )
						{
							UserLink.HRef = "Users.aspx";
							iFolderLink.HRef = "iFolders.aspx";
							ServerLink.HRef = "Servers.aspx";
							SystemLink.HRef = String.Empty;
						}
						else
						{
							UserLink.HRef = "Users.aspx";
							iFolderLink.HRef = "iFolders.aspx";
							ServerLink.HRef = String.Empty;
							SystemLink.HRef = "SystemInfo.aspx";
						}
					}
				}

				// Initialize the state variables.
				BreadCrumbs.RepeatColumns = 0;
				CrumbList = new ArrayList();
			}
		}

		#endregion

		#region Protected Methods

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
		/// Event handler that gets called when the LogoutButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnLogoutButton_Click( object source, EventArgs e )
		{
			Logout(GetString( "MESSAGEINFORMATION" ), GetString( "LOGINLOGOUT" ) );
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds a bread crumb to the bread crumb list.
		/// </summary>
		/// <param name="crumb"></param>
		/// <param name="link"></param>
		public void AddBreadCrumb( string crumb, string link )
		{
			CrumbList.Add( new BreadCrumbInfo( crumb, link ) );
			++BreadCrumbs.RepeatColumns;
			BreadCrumbs.DataSource = CreateDataSource();
			BreadCrumbs.DataBind();
		}

		/// <summary>
		/// Adds a bread crumb to the bread crumb list.
		/// </summary>
		/// <param name="crumbs"></param>
		/// <param name="links"></param>
		public void AddBreadCrumb( string[] crumbs, string[] links )
		{
			for ( int i = 0; i < crumbs.Length; ++i )
			{
				AddBreadCrumb( crumbs[ i ], links[ i ] );
			}
		}

		/// <summary>
		/// Shows up an error below the banner.
		/// </summary>
		/// <param name="msg"></param>
		public void ShowError( string msg )
		{
			ErrorMsg.Text = String.Format( GetString( "ERRORTEMPLATE" ), msg );
			ErrorPanel.Visible = true;
		}

		#endregion

		#region Web Form Designer generated code

		/// <summary>
		/// OnInit()
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
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		/// this.Load += new System.EventHandler(this.Page_Load);
		private void InitializeComponent()
		{
			this.Load += new System.EventHandler(this.Page_Load);
		}

		#endregion

		#region BreadCrumbInfo

		/// <summary>
		/// Contains bread crumb information.
		/// </summary>
		[ Serializable() ]
		private class BreadCrumbInfo
		{
			#region Class Members
			
			/// <summary>
			/// The bread crumb label.
			/// </summary>
			public string Crumb;

			/// <summary>
			/// The uri associated with the crumb.
			/// </summary>
			public string CrumbUri;

			#endregion

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="crumb"></param>
			/// <param name="crumbUri"></param>
			public BreadCrumbInfo( string crumb, string crumbUri )
			{
				Crumb = crumb;
				CrumbUri = crumbUri;
			}

			#endregion
		}

		#endregion
	}
}
