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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Resources;
using System.Net;
using System.Threading;
using System.Globalization;
using System.Text;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Login
	/// </summary>
	public class Login : Page
	{
		/// <summary>
		/// Log
		/// </summary>
		private static readonly WebLogger log = new WebLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Message
		/// </summary>
		protected Literal Message;

		/// <summary>
		/// User Name
		/// </summary>
		protected TextBox UserName;

		/// <summary>
		/// Password
		/// </summary>
		protected TextBox Password;

		/// <summary>
		/// Login Button
		/// </summary>
		protected Button LoginButton;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;
	
		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, EventArgs e)
		{
			// clear any message
			Message.Text = "";
			
			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				// query message
				Message.Text = Request.QueryString.Get("Message");

				// basic authentication for iChain
				// NOTE: only check if we are not trying to show a message to the user
				if ((Message.Text == null) || (Message.Text.Length == 0))
				{
					CheckBasicAuthentication();
				}

				// set a test cookie
				Response.Cookies["test"].Value = true.ToString();
				Response.Cookies["test"].Expires =  DateTime.Now + TimeSpan.FromDays(30);

				// cookies
				HttpCookie usernameCookie = Request.Cookies["username"];

				// username
				if (usernameCookie != null)
				{
					UserName.Text = HttpUtility.UrlDecode(usernameCookie.Value);
				}

				// strings
				LoginButton.Text = GetString("LOGIN");
			}
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
		/// On Initialize
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
			this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
			this.Load += new System.EventHandler(this.Page_Load);
		}

		#endregion

		/// <summary>
		/// Login Button Handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoginButton_Click(object sender, System.EventArgs e)
		{
			DoLogin(UserName.Text, Password.Text, true);
		}

		/// <summary>
		/// Do Login
		/// </summary>
		/// <param name="username">Username</param>
		/// <param name="password">Password</param>
		/// <param name="check">Perform browser checks.</param>
		private void DoLogin(string username, string password, bool check)
		{
			// client information
			log.Info(Context, "Client Information: {0}, {1}, {2}, {3}",
				   username, Context.Request.UserHostName,
				   Context.Request.UserLanguages[0], Context.Request.UserAgent);
			
			// expires for new cookies
			DateTime expires = DateTime.Now + TimeSpan.FromDays(30);
			
			// check for cookies
			if (check)
			{
				HttpCookie testCookie = Request.Cookies["test"];
				if (testCookie == null)
				{
					Message.Text = GetString("LOGIN.NOCOOKIES");
				
					// log access
					log.Info(Context, "Login Failed: Browser Cookies Disabled");

					return;
				}
			}

			// check for running scripts
			// NOTE: check the value for iChain
			if (check)
			{
				string noscript = Request.Form.Get("noscript");
				if ((noscript != null) && (noscript == "true"))
				{
					Message.Text = GetString("LOGIN.NOSCRIPT");
				
					// log access
					log.Info(Context, "Login Failed: Browser Scripts Disabled");

					return;
				}
			}

			try
			{
				// connection
				iFolderWeb web = new iFolderWeb();

				// update web url
				// always use the original path from the WSDL file
				string url = System.Configuration.ConfigurationSettings.AppSettings.Get("SimiasUrl");
#if TESTING
				url = "http://localhost:8086";
#endif
				UriBuilder webUrl = new UriBuilder(url);
				webUrl.Path = (new Uri(web.Url)).PathAndQuery;
				web.Url = webUrl.Uri.ToString();

				// credentials
				web.PreAuthenticate = true;
				web.Credentials = new NetworkCredential(username, password);
			
				// cookies
				web.CookieContainer = new CookieContainer();

				// user and system
				iFolderUser user = web.GetAuthenticatedUser();
				iFolderSystem system = web.GetSystem();
				Session["System"] = system.Name;
				iFolderServer server = web.GetServer();
				Session["Version"] = server.Version;

				// new username cookie for 30 days
				Response.Cookies.Remove("username");
				Response.Cookies["username"].Value = user.UserName;
				Response.Cookies["username"].Expires = expires;

				// configuration
				int max = Pagging.DEFAULT_ITEMS_PER_PAGE;
				try
				{
					max = int.Parse(System.Configuration.ConfigurationSettings.AppSettings.Get("ItemsPerPage"));
				}
				catch
				{
				}

				// session
				Session["Connection"] = web;
				Session["UserID"] = user.ID;
				Session["Username"] = user.UserName;
				Session["UserFullName"] = user.FullName;
				Session["ItemsPerPage"] = max;

				// log access
				log.Info(Context, "Login Successful");

				// redirect
				FormsAuthentication.RedirectFromLoginPage(user.UserName, false);
			}
			catch(WebException ex)
			{
				// log access
				log.Info(Context, ex, "Login Failed");

				if (!HandleException(ex)) throw;
			}
			catch(Exception ex)
			{
				// log access
				log.Info(Context, ex, "Login Failed");

				throw ex;
			}
		}

		/// <summary>
		/// Check Basic Authentication
		/// </summary>
		/// <remarks>iChain Requirement</remarks>
		private void CheckBasicAuthentication()
		{
			try
			{
				string auth = Request.Headers["Authorization"];

				if (auth != null)
				{
					auth = auth.Trim();
				
					// basic authentication only
					if (auth.StartsWith("Basic"))
					{
						string credentials = (new ASCIIEncoding()).GetString(
							Convert.FromBase64String(auth.Substring("Basic".Length + 1)));

						string[] parts = credentials.Split(new char [] { ':' });
						string username = parts[0];
						string password = parts[1];

						if ((username.Length > 0) && (password.Length > 0))
						{
							DoLogin(username, password, false);
						}
					}
				}
			}
			catch(Exception e)
			{
				// log
				log.Debug(Context, e, "Check Basic Authentication");
			}
		}

		/// <summary>
		/// Handle Exceptions
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool HandleException(WebException e)
		{
			bool result = true;

			// simias error
			string error = null;
			
			try
			{
				error = e.Response.Headers["Simias-Error"];
			}
			catch
			{
				// ignore
			}

			if ((error != null) && (error.Length > 0))
			{
				switch(error)
				{
					case "InvalidCertificate":
						Message.Text = GetString("LOGIN.TRUSTFAILED");
						break;

					case "InvalidCredentials":
					case "UnknownUser":
					case "InvalidPassword":
						Message.Text = GetString("LOGIN.UNAUTHORIZED");
						break;

					case "AccountDisabled":
					case "SimiasLoginDisabled":
						Message.Text = GetString("LOGIN.ACCOUNTDISABLED");
						break;

					case "AccountLockout":
						Message.Text = GetString("LOGIN.ACCOUNTLOCKED");
						break;

					default:
						Message.Text = GetString("LOGIN.CONNECTFAILED");
						break;
				}

			}
			
			// standard error
			else
			{
				switch(e.Status)
				{
					case WebExceptionStatus.ProtocolError:
					{
						// http code
						HttpStatusCode code = (HttpStatusCode) ((e as WebException).Response as HttpWebResponse).StatusCode;

						switch(code)
						{
							case HttpStatusCode.Unauthorized:
								Message.Text = GetString("LOGIN.UNAUTHORIZED");
								break;

							case HttpStatusCode.Redirect:
								string location = e.Response.Headers["Location"];
								
								try
								{
									UriBuilder uri = new UriBuilder(location);
									uri.Path = "";
									location = uri.ToString();
								}
								catch
								{
									// ignore
								}
								
								Message.Text = String.Format("{0}<br>{1}", GetString("LOGIN.REDIRECT"), location);
								break;

							default:
								Message.Text = GetString("LOGIN.CONNECTFAILED");
								break;
						}
					}
						break;
				
					case WebExceptionStatus.ConnectFailure:
						Message.Text = GetString("LOGIN.CONNECTFAILED");
						break;

					case WebExceptionStatus.TrustFailure:
						Message.Text = GetString("LOGIN.TRUSTFAILED");
						break;

					case WebExceptionStatus.SecureChannelFailure:
						Message.Text = GetString("LOGIN.SECUREFAILED");
						break;

					case WebExceptionStatus.SendFailure:
						Message.Text = GetString("LOGIN.SENDFAILED");
						break;

					default:
						result = false;
						break;
				}
			}

			return result;
		}
	}

	/// <summary>
	/// List Item Text Comparer
	/// </summary>
	public class ListItemTextComparer : IComparer
	{
		/// <summary>
		/// Case Insensitive Comparer
		/// </summary>
		private CaseInsensitiveComparer cic;

		/// <summary>
		/// Constructor
		/// </summary>
		public ListItemTextComparer()
		{
			cic = new CaseInsensitiveComparer();
		}

		/// <summary>
		/// Compare
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public int Compare(object a, object b)
		{
			return cic.Compare((a as ListItem).Text, (b as ListItem).Text);
		}

		/// <summary>
		/// Default Instance
		/// </summary>
		public static ListItemTextComparer Default
		{
			get { return new ListItemTextComparer(); }
		}
	}
}
