/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Threading;
using System.Resources;
using System.Web.Services.Protocols;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	///	Header Control
	/// </summary>
	public class HeaderControl : UserControl
	{
		/// <summary>
		/// Log
		/// </summary>
		private static readonly WebLogger log = new WebLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Settings Button
		/// </summary>
		protected HyperLink SettingsLink;
		
		/// <summary>
		/// Help Button
		/// </summary>
		protected HyperLink HelpLink;
		
		/// <summary>
		/// Logout Button
		/// </summary>
		protected LinkButton LogoutButton;
		
		/// <summary>
		/// User Name
		/// </summary>
		protected Literal FullName;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;
	
		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;

		/// <summary>
		/// Max Header String Length
		/// </summary>
		private readonly static int MAX_HEADER_STRING = 30;

		/// <summary>
		/// IChain cookie name
		/// </summary>
		private readonly static string[] IChainCookieName = {"IPCZQX0","IPCZQ0"};

		/// <summary>
		/// Page Init
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Init(object sender, EventArgs e)
		{
			// localization
			rm = (ResourceManager) Application["RM"];

			// connection
			web = (iFolderWeb) Session["Connection"];

			// check connection
			if (web == null) Logout(GetString("LOGIN.LOSTSESSION"));
		}

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			if (!IsPostBack)
			{
				// data
				BindData();

				// full name
				FullName.Text = Trim((Session["User"] as iFolderUser).FullName, MAX_HEADER_STRING);
				
				// strings
				SettingsLink.Text = GetString("SETTINGS");
				HelpLink.Text = GetString("HELP");
				LogoutButton.Text = GetString("LOGOUT");

				// help
				//HelpButton.NavigateUrl = String.Format("help/{0}/index.html",
				//	Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
		}

		/// <summary>
		/// Handle Exceptions
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool HandleException(Exception e)
		{
			bool result = true;

			string type = WebUtility.GetExceptionType(e);

			// types
			switch(type)
			{
				default:
					result = false;
					break;
			}

			return result;
		}

		/// <summary>
		/// Trim a string with an ellipses.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		private string Trim(string text, int length)
		{
			string result = text;

			if ((text != null) && (text.Length > length))
			{
				result = String.Format("{0}{1}", text.Substring(0, length), GetString("ELLIPSES"));
			}

			return result;
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString(string key)
		{
			return WebUtility.GetString(key, rm);
		}

		#region Web Form Designer
		
		/// <summary>
		/// On Intialize
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Initialize Components
		/// </summary>
		private void InitializeComponent()
		{
			this.Init += new EventHandler(Page_Init);
			this.Load += new System.EventHandler(this.Page_Load);
			this.LogoutButton.Click += new System.EventHandler(this.LogoutButton_Click);
		}
		
		#endregion

		/// <summary>
		/// Logout Button Handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LogoutButton_Click(object sender, System.EventArgs e)
		{
			Logout(GetString("LOGIN.LOGOUT"));
		}

		private void Logout(string message)
		{
		        string logoutRedirectURL = String.Format("Login.aspx?Message={0}", Context.Server.UrlEncode(message));

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

			log.Info(Context, "Logout Successful");

			Response.Redirect(logoutRedirectURL);
		}
		
		#region Public Methods
		
		/// <summary>
		/// Adds the page name to the help link.
		/// </summary>
		/// <param name="PageName"></param>
		public void AddHelpLink( string PageName)
		{
			// get the language which admin had chosen during logging in
			string code = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
			if(PageName.Equals(GetString("BROWSE")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "browse.html");
			else if(PageName.Equals(GetString("IFOLDERS")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "home.html");
			else if(PageName.Equals(GetString("SEARCH")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "search.html");
			else if(PageName.Equals(GetString("MEMBERS")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "members.html");
			else if(PageName.Equals(GetString("HISTORY")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "history.html");
			else if(PageName.Equals(GetString("DETAILS")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "details.html");
			else if(PageName.Equals(GetString("IFOLDERNEW")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "newifolder.html");
			else if(PageName.Equals(GetString("CERTIFICATE")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "newifolder.html");	
			else if(PageName.Equals(GetString("UPLOAD")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "upload.html");
			else if(PageName.Equals(GetString("NEWFOLDER")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "newfolder.html");
			else if(PageName.Equals(GetString("SHARE")))
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "share.html");
			else 
				HelpLink.NavigateUrl = String.Format("help/{0}/{1}", code, "home.html");
		}
		
		#endregion
	}
}
