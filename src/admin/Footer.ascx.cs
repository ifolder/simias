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

namespace Novell.iFolderWeb.Admin
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Resources;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using System.Web.Security;
	using System.Globalization;
	using System.Threading;

	/// <summary>
	///	Implements a footer control for a web page.
	/// </summary>
	public class Footer : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// Logged in as label control.
		/// </summary>
		protected Label LoggedInAs;

		/// <summary>
		/// Logged in name label control.
		/// </summary>
		protected Label LoggedInName;
		
		/// <summary>
		/// Anchor controls.
		/// </summary>
		protected HyperLink HelpButton;

		/// <summary>
		/// Logout button control.
		/// </summary>
		protected LinkButton LogoutButton;

		/// <summary>
		/// IChain cookie name
		/// </summary>
		private readonly static string[] IChainCookieName = {"IPCZQX0","IPCZQ0"};

		#endregion

		#region Properties
		#endregion

		#region Private Methods

		/// <summary>
		/// Logout
		/// </summary>
		/// <param name="type"></param>
		/// <param name="message"></param>
		private void Logout( string type, string message )
		{
		        string logoutRedirectURL = String.Format("Login.aspx?Message={0}&MessageText={1}", Context.Server.UrlEncode( type ), Context.Server.UrlEncode(message));
			for (int i=0; i< Request.Cookies.Count; i++)
			{
			        if (Request.Cookies[i].Name.StartsWith (IChainCookieName[0]) || Request.Cookies[i].Name.StartsWith (IChainCookieName[1]) ) 
				{
				        string logoutURL =  Environment.GetEnvironmentVariable("LogoutUrl" );
					if(logoutURL != null && logoutURL != String.Empty )
				        	logoutRedirectURL =  Environment.GetEnvironmentVariable("LogoutUrl" );
					break;
				}
			}

			FormsAuthentication.SignOut();
			
			// double-check that the session is abandoned
			Session.Abandon();

			Response.Redirect(logoutRedirectURL);

		}

		/// <summary>
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;
		
			// get the language which admin had chosen during login
			string code = "en";
			HttpCookie languageCookie = Request.Cookies["Language"];
                        // check language cookie
                        if ((languageCookie != null) && (languageCookie.Value != null)
                                && (languageCookie.Value.Length > 0))
                        {
                                code = languageCookie.Value;
                        }

			// First Login 
			if( code == null )
				code = "en"; 

                        // set the code
                        Thread.CurrentThread.CurrentUICulture =
                              CultureInfo.CreateSpecificCulture(code);


			// Check to see if the session is still valid.
			if ( Session[ "Connection" ] == null )
			{
				// The session has timed out. Make sure it's really gone and redirect
				// to the login page.
				Session.Abandon();

				Response.Redirect( 
					String.Format( 
					"Login.aspx?MessageType={0}&MessageText={1}",
					Context.Server.UrlEncode( GetString( "LOGINERROR" ) ),
					Context.Server.UrlEncode( "SESSIONCLOSED" ) ) );
			}

			if ( !IsPostBack )
			{
				// Show the currently logged in user.
				if ( Session[ "NAME" ] != null )
				{
					LoggedInAs.Text = GetString( "LOGGEDINAS" );
					LoggedInName.Text = Session[ "NAME" ] as String;
					HelpButton.Text = GetString( "HELP" );
					HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "member.html");
					LogoutButton.Text = GetString( "LOGOUT" );
				}
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
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Load += new System.EventHandler(this.Page_Load);
		}
		
		#endregion
		
		#region Public Methods
		/// <summary>
		/// Adds the page name to the help link.
		/// </summary>
		/// <param name="PageName"></param>
		public void AddHelpLink( string PageName)
		{
			// get the language which admin had chosen during logging in
			string code = Session["Language"] as string;
			if(code == null || code == String.Empty)
			{
				code = "en";
			}
			if(PageName.Equals(GetString("USERS")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "users.html");
			else if(PageName.Equals(GetString("IFOLDERS")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "ifolders.html");
			else if(PageName.Equals(GetString("SYSTEM")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "systeminfo.html");
			else if(PageName.Equals(GetString("SERVERS")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "servers.html");
			else if(PageName.Equals(GetString("REPORTS")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "reports.html");
			else if(PageName.Equals(GetString("USERDETAILS")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "userdetails.html");
			else if(PageName.Equals(GetString("IFOLDERDETAILS")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "ifolderdetails.html");
			else if(PageName.Equals(GetString("CREATENEWIFOLDER")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "createifolder.html");
			else if(PageName.Equals(GetString("SERVERDETAILS")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "serverdetails.html");
			else if(PageName.Equals(GetString("LDAPADMINAUTH")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "ldapsettings.html");
			else if(PageName.Equals(GetString("PROVISIONUSERS")))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "provisionusers.html");
			else if(PageName.Equals("USERMOVE"))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "usermove.html");
			else if(PageName.Equals("USERMOVEDETAILS"))
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "usermove_details.html");
			else
				HelpButton.NavigateUrl = String.Format("help/{0}/{1}", code, "member.html");
		}
		#endregion
	}
}
