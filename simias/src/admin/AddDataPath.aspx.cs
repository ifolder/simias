/***********************************************************************
 *  $RCSfile: CreateUser.aspx.cs,v $
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
        /// Summary description for Configuring DataPaths.
        /// </summary>
        public class AddDataPath : System.Web.UI.Page
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
                /// DataPath name edit control.
                /// </summary>^M
                protected TextBox DataPathName;

		/// <summary>
                /// Full DataPath edit control.
                /// </summary>
                protected TextBox FullPath;

		/// <summary>
                /// Cancel button control.
                /// </summary>
                protected Button AddDataPathButton;

		/// <summary>
                /// Add Data Store button control.
                /// </summary>
                protected Button CancelButton;

		#endregion
		
		#region Properties
		
		/// <summary>
                /// Gets the Server ID.
                /// </summary>
                private string ServerID
                {
                        get { return Request.Params[ "ServerID" ]; } 
                }

		/// <summary>
                /// Gets the Server Name.
                /// </summary>
                private string serverName
                {
                        get { return Request.Params[ "serverName" ]; }
                }

		/// <summary>
                /// Gets or sets the referring page.
                /// </summary>
		private string ReferringPage
                {
                        get { return ViewState[ "ReferringPage" ] as String; }
                        set { ViewState[ "ReferringPage" ] = value; }
                }

		#endregion
		
		#region Private Methods

                /// <summary>
                ///  Builds the breadcrumb list for this page.
                /// </summary>
                private void BuildBreadCrumbList(string ServerID,string serverName)
                {
			TopNav.AddBreadCrumb( GetString( "SERVERS" ), "Servers.aspx" );
			TopNav.AddBreadCrumb( serverName, String.Format( "ServerDetails.aspx?id={0}", ServerID ) );
                        TopNav.AddBreadCrumb( GetString( "ADDDATASTORE" ), null );
			TopNav.SetActivePageTab( TopNavigation.PageTabs.Users );
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

			// localization
                        rm = Application[ "RM" ] as ResourceManager;
	
			if ( !IsPostBack )
			{
				SetFocus( DataPathName );

				AddDataPathButton.Text = GetString( "ADD" );
				CancelButton.Text = GetString( "CANCEL" );

				ReferringPage = Page.Request.UrlReferrer.ToString();
			}
                }

		/// <summary>
                /// Sets focus to the specified control.
                /// </summary>
                /// <param name="ctrl"></param>
                private void SetFocus( System.Web.UI.Control ctrl )
                {
                        string s = "<SCRIPT language='javascript'>document.getElementById('" + ctrl.ClientID + "').focus() </SCRIPT>";
                        Page.RegisterStartupScript( "focus", s );
                }


		
		/// <summary>
                /// Page_PreRender
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                private void Page_PreRender(object sender, EventArgs e)
                {
                        // Set the breadcrumb list.
                        BuildBreadCrumbList(ServerID,serverName);
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

		/// <summary>
                /// Enables the create button if all required fields are present.
                /// </summary>
                private bool ValidPage
                {
                        get
                        {
                                return ( ( DataPathName.Text.Length > 0 ) &&
                                        ( FullPath.Text.Length > 0 )); 
                        }
                }



		#endregion

		#region Protected Methods
		/// <summary>
                /// Event handler that gets called when the Add DataPath button is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void OnAddDataPathButton_Click( object source, EventArgs e )
                {
			if ( !ValidPage )
                        {
                                TopNav.ShowError( GetString( "ALLFIELDSREQUIRED" ) );
                        }
			else
			{
				int result = web.AddDataStore(DataPathName.Text,FullPath.Text,ServerID); 
				if( result == 1 )			
					TopNav.ShowError( GetString( "LINKALREADYEXISTS" ) );		
				else if( result == 2 )
					TopNav.ShowError( GetString( "INVALIDFULLPATH" ) );	
				else
					Page.Response.Redirect( ReferringPage, true );
        	        }
		}

		/// <summary>
                /// Event handler that gets called when the Cancel button is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void OnCancelButton_Click( object source, EventArgs e )
                {
			 Page.Response.Redirect( ReferringPage, true );
                }


		#endregion

		#region Web Form Designer generated code^M

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
	}
}






























