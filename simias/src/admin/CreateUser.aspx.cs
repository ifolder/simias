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
	/// Summary description for CreateUser.
	/// </summary>
	public class CreateUser : System.Web.UI.Page
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
		/// User name edit control.
		/// </summary>
		protected TextBox UserName;

		/// <summary>
		/// First name edit control.
		/// </summary>
		protected TextBox FirstName;

		/// <summary>
		/// Last name edit control.
		/// </summary>
		protected TextBox LastName;

		/// <summary>
		/// Full name edit control.
		/// </summary>
		protected TextBox FullName;

		/// <summary>
		/// Password edit control.
		/// </summary>
		protected TextBox Password;

		/// <summary>
		/// Retyped password edit control.
		/// </summary>
		protected TextBox RetypedPassword;


		/// <summary>
		/// Create user button control.
		/// </summary>
		protected Button CreateButton;

		/// <summary>
		/// Cancel button control.
		/// </summary>
		protected Button CancelButton;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the referring page.
		/// </summary>
		private string ReferringPage
		{
			get { return ViewState[ "ReferringPage" ] as String; }
			set { ViewState[ "ReferringPage" ] = value; }
		}

		/// <summary>
		/// Enables the create button if all required fields are present.
		/// </summary>
		private bool ValidPage
		{
			get
			{
				return ( ( UserName.Text.Length > 0 ) &&
					( FirstName.Text.Length > 0 ) &&
					( LastName.Text.Length > 0 ) &&
					( FullName.Text.Length > 0 ) &&
					( Password.Text.Length > 0 ) &&
					( RetypedPassword.Text.Length > 0 ) );
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			TopNav.AddBreadCrumb( GetString( "USERS" ), "Users.aspx" );
			TopNav.AddBreadCrumb( GetString( "CREATEUSER" ), null );
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
				// Remember the page that we came from.
				ReferringPage = Page.Request.UrlReferrer.ToString();

				// Initialize the localized fields.
				CreateButton.Text = GetString( "CREATE" );
				CancelButton.Text = GetString( "CANCEL" );

				// Initialize state variables.
				SetFocus( UserName );
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

		/// <summary>
		/// Sets focus to the specified control.
		/// </summary>
		/// <param name="ctrl"></param>
		private void SetFocus( System.Web.UI.Control ctrl )
		{
			string s = "<SCRIPT language='javascript'>document.getElementById('" + ctrl.ClientID + "').focus() </SCRIPT>";
			Page.RegisterStartupScript( "focus", s );
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
		/// Event handler that gets called when the cancel button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnCancelButton_Click( object source, EventArgs e )
		{
			// Return back to the referring page.
			string url = web.TrimUrl(ReferringPage);
			Page.Response.Redirect( url, true );
		}

		/// <summary>
		/// Event handler that gets called when the create button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnCreateButton_Click( object source, EventArgs e )
		{
			if ( !ValidPage )
			{
				TopNav.ShowError( GetString( "ALLFIELDSREQUIRED" ) );
			}
			else
			{
				// Verify that the retyped password matches.
				if ( Password.Text == RetypedPassword.Text )
				{
					try
					{
						web.CreateUser( 
							UserName.Text, 
							Password.Text, 
							Guid.NewGuid().ToString(), 
							FirstName.Text, 
							LastName.Text, 
							FullName.Text, 
							String.Empty, 
							String.Empty );

						// Return back to the referring page.
						string url = web.TrimUrl(ReferringPage);
						Page.Response.Redirect( url, true );
					}
					catch ( Exception ex )
					{
						TopNav.ShowError( GetString( "ERRORCANNOTCREATEUSER" ), ex );
					}
				}
				else
				{
					TopNav.ShowError( GetString( "PASSWORDSDONOTMATCH" ) );
				}
			}
		}

		/// <summary>
		/// Event handler that gets called when the FirstName text box changes.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnFirstNameChanged( object source, EventArgs e )
		{

		}

		/// <summary>
		/// Event handler that gets called when the LastName text box changes.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnLastNameChanged( object source, EventArgs e )
		{
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
