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
	using System.Net;
	using System.Resources;
	using System.Web;
	using System.Web.Services.Protocols;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Xml;

	/// <summary>
	///Summary description for TopNavigation.
	/// </summary>
	public class TopNavigation : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Enumeration of page tabs.
		/// </summary>
		public enum PageTabs
		{
			/// <summary>
			/// User tab
			/// </summary>
			Users,

			/// <summary>
			/// iFolders tab
			/// </summary>
			iFolders,

			/// <summary>
			/// System tab
			/// </summary>
			System,

			/// <summary>
			/// Servers tab
			/// </summary>
			Servers,

			/// <summary>
			/// Reports tab
			/// </summary>
			Reports
		}

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
		/// Anchor controls.
		/// </summary>
		protected HtmlAnchor ReportsLink;


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
				// Initially hide the error panel.
				ErrorPanel.Visible = false;

				Control body = Page.FindControl( "users" );
				if ( body != null )
				{
					UserLink.HRef = null;
					iFolderLink.HRef = "iFolders.aspx";
					ServerLink.HRef = "Servers.aspx";
					SystemLink.HRef = "SystemInfo.aspx";
					ReportsLink.HRef = "Reports.aspx";
				}
				else
				{
					body = Page.FindControl( "ifolders" );
					if ( body != null )
					{
						UserLink.HRef = "Users.aspx";
						iFolderLink.HRef = null;
						ServerLink.HRef = "Servers.aspx";
						SystemLink.HRef = "SystemInfo.aspx";
						ReportsLink.HRef = "Reports.aspx";
					}
					else
					{
						body = Page.FindControl( "system" );
						if ( body != null )
						{
							UserLink.HRef = "Users.aspx";
							iFolderLink.HRef = "iFolders.aspx";
							ServerLink.HRef = "Servers.aspx";
							SystemLink.HRef = null;
							ReportsLink.HRef = "Reports.aspx";
						}
						else
						{
							body = Page.FindControl( "server" );
							if ( body != null )
							{
								UserLink.HRef = "Users.aspx";
								iFolderLink.HRef = "iFolders.aspx";
								ServerLink.HRef = null;
								SystemLink.HRef = "SystemInfo.aspx";
								ReportsLink.HRef = "Reports.aspx";
							}
							else
							{
								
								body = Page.FindControl( "report" );
                                                        	if ( body != null )
								{
									UserLink.HRef = "Users.aspx";
									iFolderLink.HRef = "iFolders.aspx";
									ServerLink.HRef = "Servers.aspx";
									SystemLink.HRef = "SystemInfo.aspx";
									ReportsLink.HRef = null;
								}
								else
								{
									UserLink.HRef = "Users.aspx";
                                                                        iFolderLink.HRef = "iFolders.aspx";
                                                                        ServerLink.HRef = "Servers.aspx";
                                                                        SystemLink.HRef = "SystemInfo.aspx";
									ReportsLink.HRef = "Reports.aspx";
								}
							}
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

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds a bread crumb to the bread crumb list.
		/// </summary>
		/// <param name="crumb"></param>
		/// <param name="link"></param>
		public void AddBreadCrumb( string crumb, string link )
		{
			if ( crumb.Length > 128 )
			{
				crumb = crumb.Substring( 0, 128 );
			}

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
		/// Gets the exception message from the Exception object.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public string ExceptionMessage( Exception e )
		{
			string type = GetExceptionType( e );

			switch ( type )
			{
				case "AuthorizationException":
					return GetString( "ERRORNOTADMINISTRATOR" );

				case "AuthenticationException":
					return GetString( "ERRORAUTHENTICATIONFAILED" );

				case "iFolderDoesNotExistException":
					return GetString( "ERRORIFOLDERDOESNOTEXIST" );

				case "MemberDoesNotExistException":
					return GetString( "ERRORMEMBERDOESNOTEXIST" );

				case "UserDoesNotExistException":
					return GetString( "ERRORUSERDOESNOTEXIST" );

				case "LockException":
					return GetString( "ERRORLOCKEXCEPTION" );

				case "AccessException":
					return GetString( "ERRORACCESSEXCEPTION" );

				case "ArgumentException":
					return e.Message;

				default:
					return type;
			}
		}

		/// <summary>
		/// Gets the exception type from the specified exception.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static string GetExceptionType( Exception e )
		{
			string type = e.GetType().Name;

	
		if ( e is SoapException )
			{
				try
				{
					XmlNode node = ( e as SoapException ).Detail.SelectSingleNode( "OriginalException" );
					if ( node == null )
					{
						// try inside the <detail> tags
						node = ( e as SoapException ).Detail.SelectSingleNode( "*/OriginalException" );
					}

					if ( node != null )
					{
						type = node.Attributes.GetNamedItem( "type" ).Value;
						type = type.Substring( type.LastIndexOf( "." ) + 1 );
					}
				}
				catch {}
			}
			else if ( e is WebException )
			{
				type = (e as WebException ).Status.ToString();

				if ( ( e as WebException ).Status == WebExceptionStatus.ProtocolError )
				{
					HttpWebResponse response = ( e as WebException ).Response as HttpWebResponse;
					type = response.StatusDescription;
				}
			}

			return type;
		}

		/// <summary>
		/// Sets the active page tab for utility pages that are used by multiple root pages.
		/// </summary>
		/// <param name="page"></param>
		public void SetActivePageTab( PageTabs page )
		{
			Control body;

			switch( page )
			{
				case PageTabs.iFolders:
					body = Page.FindControl( "ifolders" );
					if ( body != null )
					{
						UserLink.HRef = "Users.aspx";
						iFolderLink.HRef = null;
						ServerLink.HRef = "Servers.aspx";
						SystemLink.HRef = "SystemInfo.aspx";
						ReportsLink.HRef = "Reports.aspx";
					}
					break;

				case PageTabs.Servers:
					body = Page.FindControl( "servers" );
					if ( body != null )
					{
						UserLink.HRef = "Users.aspx";
						iFolderLink.HRef = "iFolders.aspx";
						ServerLink.HRef = null;
						SystemLink.HRef = "SystemInfo.aspx";
						ReportsLink.HRef = "Reports.aspx";
					}
					break;

				case PageTabs.System:
					body = Page.FindControl( "system" );
					if ( body != null )
					{
						UserLink.HRef = "Users.aspx";
						iFolderLink.HRef = "iFolders.aspx";
						ServerLink.HRef = "Servers.aspx";
						SystemLink.HRef = null;
						ReportsLink.HRef = "Reports.aspx";
					}
					break;

				case PageTabs.Users:
					body = Page.FindControl( "users" );
					if ( body != null )
					{
						UserLink.HRef = null;
						iFolderLink.HRef = "iFolders.aspx";
						ServerLink.HRef = "Servers.aspx";
						SystemLink.HRef = "SystemInfo.aspx";
						ReportsLink.HRef = "Reports.aspx";
					}
					break;

				case PageTabs.Reports:
					body = Page.FindControl( "reports" );
					if ( body != null )
					{
						UserLink.HRef = "Users.aspx";
						iFolderLink.HRef = "iFolders.aspx";
						ServerLink.HRef = "Servers.aspx";
						SystemLink.HRef = "SystemInfo.aspx";
						ReportsLink.HRef = null;
					}
					break;
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

		/// <summary>
		/// Shows up an error below the banner.
		/// </summary>
		/// <param name="ex"></param>
		public void ShowError( Exception ex )
		{
			ShowError( ExceptionMessage( ex ) );
		}

		/// <summary>
		/// Shows up an error below the banner.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="ex"></param>
		public void ShowError( string msg, Exception ex )
		{
			ShowError( String.Format( "{0} {1}", msg, ExceptionMessage( ex ) ) );
		}

		/// <summary>
		/// Determines whether to show exception detail based on the type of exception.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public bool ShowExceptionDetail( Exception e )
		{
			switch ( GetExceptionType( e ) )
			{
				case "AuthorizationException":
				case "AuthenticationException":
				case "iFolderDoesNotExistException":
				case "MemberDoesNotExistException":
				case "UserDoesNotExistException":
				case "LockException":
				case "AccessException":
				case "ArgumentException":
					return false;

				default:
					return true;
			}
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
