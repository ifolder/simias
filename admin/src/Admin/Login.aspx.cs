/***********************************************************************
 *  $RCSfile: Login.aspx.cs,v $
 *
 *  Copyright � Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
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
using System.Web.Services.Protocols;
using System.Resources;
using System.Net;
using System.Threading;
using System.Globalization;
using System.Text;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Login
	/// </summary>
	public class Login : Page
	{
		/// <summary>
		/// Log
		/// </summary>
		private static readonly iFolderWebLogger log = new iFolderWebLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Message Type
		/// </summary>
		protected Literal MessageType;

		/// <summary>
		/// Message
		/// </summary>
		protected Literal MessageText;

		/// <summary>
		/// Server URL
		/// </summary>
		protected Literal ServerUrl;

		/// <summary>
		/// User Name
		/// </summary>
		protected TextBox UserName;

		/// <summary>
		/// Password
		/// </summary>
		protected TextBox Password;

		/// <summary>
		/// Help Button
		/// </summary>
		protected HyperLink HelpButton;

		/// <summary>
		/// Language List
		/// </summary>
		protected DropDownList LanguageList;

		/// <summary>
		/// Login Button
		/// </summary>
		protected Button LoginButton;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// noscript
		/// </summary>
		protected System.Web.UI.HtmlControls.HtmlInputHidden noscript;
	
		/// <summary>
		/// Languages
		/// </summary>
		private ListItem[] languages =
		{
			// language list
			new ListItem("ENGLISH", "en"),
			new ListItem("CHINESE-SIMPLIFIED", "zh-CN"),
			new ListItem("CHINESE-TRADITIONAL", "zh-TW"),
			new ListItem("CZECH", "cs"),
			new ListItem("FRENCH", "fr"),
			new ListItem("GERMAN", "de"),
			new ListItem("HUNGARIAN", "hu"),
			new ListItem("ITALIAN", "it"),
			new ListItem("JAPANESE", "ja"),
			new ListItem("POLISH", "pl"),
			new ListItem("PORTUGUESE-BRAZIL", "pt-BR"),
			new ListItem("RUSSIAN", "ru"),
			new ListItem("SPANISH", "es"),
			new ListItem("SLOVAK", "sk"),
		};

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, EventArgs e)
		{
			// clear any message
			MessageType.Text = "";
			MessageText.Text = "";

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				// query message
				MessageType.Text = Request.QueryString.Get("MessageType");
				MessageText.Text = Request.QueryString.Get("MessageText");

				// basic authentication for iChain
				// NOTE: only check if we are not trying to show a message to the user
				if ((MessageText.Text == null) || (MessageText.Text.Length == 0))
				{
					CheckBasicAuthentication();
				}

				// set a test cookie
				Response.Cookies["test"].Value = true.ToString();
				Response.Cookies["test"].Expires =  DateTime.Now + TimeSpan.FromDays(30);

				// cookies
				HttpCookie usernameCookie = Request.Cookies["username"];
				HttpCookie languageCookie = Request.Cookies["language"];

				// username
				if (usernameCookie != null)
				{
					UserName.Text = HttpUtility.UrlDecode( usernameCookie.Value );
				}

				// culture info
				string code = Thread.CurrentThread.CurrentUICulture.Name;

				if ((Context.Request != null) && (Request.UserLanguages.Length > 0))
				{
					code = Request.UserLanguages[0];
				}
				
				// check language cookie
				if ((languageCookie != null) && (languageCookie.Value != null)
					&& (languageCookie.Value.Length > 0))
				{
					code = languageCookie.Value;
				}

				// set the code
				try
				{
					Thread.CurrentThread.CurrentUICulture =
						CultureInfo.CreateSpecificCulture(code);
				}
				catch(Exception ex)
				{
					log.Info(Context, ex, "Culture: {0}", code);
				}

				// loop and localize languages
				bool found = false;
				foreach(ListItem language in languages)
				{
					language.Text = rm.GetString(language.Text);

					if (code.ToLower().StartsWith(language.Value.ToLower()))
					{
						language.Selected = true;
						found = true;
					}
				}

				// default to first language
				if (!found)
				{
					languages[0].Selected = true;
				}

				// sort languages
				Array.Sort(languages, ListItemTextComparer.Default);

				// add languages
				LanguageList.Items.AddRange(languages);

				// strings
				LoginButton.Text = rm.GetString("LOGIN");
				HelpButton.Text = rm.GetString("HELP");

				// help
				HelpButton.NavigateUrl = String.Format("help/{0}/login.html", code);

				// server url
				ServerUrl.Text = System.Configuration.ConfigurationSettings.AppSettings.Get("SimiasUrl");
			}
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString(string key)
		{
			return rm.GetString(key);
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
			this.Load += new System.EventHandler(this.Page_Load);
			this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
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
					MessageType.Text = rm.GetString("LOGINERROR");
					MessageText.Text = rm.GetString("LOGINNOCOOKIES");
				
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
					MessageType.Text = rm.GetString("LOGINERROR");
					MessageText.Text = rm.GetString("LOGINNOSCRIPT");
				
					// log access
					log.Info(Context, "Login Failed: Browser Scripts Disabled");

					return;
				}
			}

			try
			{
				// connection
				iFolderAdmin web = new iFolderAdmin();

				// update web url
				// always use the original path from the WSDL file
				UriBuilder webUrl = new UriBuilder(System.Configuration.ConfigurationSettings.AppSettings.Get("SimiasUrl"));
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
				Response.Cookies["username"].Value = user.UserName;
				Response.Cookies["username"].Expires = expires;

				// session
				Session["Connection"] = web;
				Session["Name"] = user.FullName;

				// ui language
				Session["Language"] = LanguageList.SelectedValue;

				// new language cookie for 30 days
				Response.Cookies["language"].Value = LanguageList.SelectedValue;
				Response.Cookies["language"].Expires = expires;

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
				MessageType.Text = rm.GetString("LOGINFAILED");
				
				switch(error)
				{
					case "InvalidCertificate":
						MessageText.Text = rm.GetString("LOGINTRUSTFAILED");
						break;

					case "InvalidCredentials":
					case "UnknownUser":
					case "InvalidPassword":
						MessageText.Text = rm.GetString("LOGINUNAUTHORIZED");
						break;

					case "AccountDisabled":
					case "SimiasLoginDisabled":
						MessageText.Text = rm.GetString("LOGINACCOUNTDISABLED");
						break;

					case "AccountLockout":
						MessageText.Text = rm.GetString("LOGINACCOUNTLOCKED");
						break;

					default:
						MessageText.Text = rm.GetString("LOGINCONNECTFAILED");
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
						MessageType.Text = rm.GetString("LOGINFAILED");

						// http code
						HttpStatusCode code = (HttpStatusCode) ((e as WebException).Response as HttpWebResponse).StatusCode;

						switch(code)
						{
							case HttpStatusCode.Unauthorized:
								MessageText.Text = rm.GetString("LOGINUNAUTHORIZED");
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
								
								MessageText.Text = String.Format("{0}<br>{1}", rm.GetString("LOGINREDIRECT"), location);
								break;

							default:
								MessageText.Text = rm.GetString("LOGINCONNECTFAILED");
								break;
						}
					}
						break;
				
					case WebExceptionStatus.ConnectFailure:
						MessageType.Text = rm.GetString("LOGINERROR");
						MessageText.Text = rm.GetString("LOGINCONNECTFAILED");
						break;

					case WebExceptionStatus.TrustFailure:
						MessageType.Text = rm.GetString("LOGINERROR");
						MessageText.Text = rm.GetString("LOGINTRUSTFAILED");
						break;

					case WebExceptionStatus.SecureChannelFailure:
						MessageType.Text = rm.GetString("LOGINERROR");
						MessageText.Text = rm.GetString("LOGINSECUREFAILED");
						break;

					case WebExceptionStatus.SendFailure:
						MessageType.Text = rm.GetString("LOGINERROR");
						MessageText.Text = rm.GetString("LOGINSENDFAILED");
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
