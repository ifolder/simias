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
*                 $Author: Rob
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

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
using System.Text.RegularExpressions;

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
		/// Help Button
		/// </summary>
		protected HyperLink HelpButton;

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
		/// Language List
		/// </summary>
		protected DropDownList LanguageList;

		/// <summary>
		/// Browser Warning
		/// </summary>
		protected Label BrowserWarning;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

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
				HttpCookie languageCookie = Request.Cookies["language"];

				// username
				if (usernameCookie != null)
				{
					try
					{
						byte[] iFolderNameInByte = Convert.FromBase64String(usernameCookie.Value);
                                        	UTF8Encoding utf8Name = new UTF8Encoding();
	                                        UserName.Text = utf8Name.GetString(iFolderNameInByte);
					}
					catch{}
				}
				// culture info
				string code = Thread.CurrentThread.CurrentUICulture.Name;
				try
				{
					if ((Context.Request != null) && (Request.UserLanguages.Length > 0) && (Request.UserLanguages != null ))
					{
						code = Request.UserLanguages[0];
						if(!( code.StartsWith("zh") || code.StartsWith("pt") ))
        	        	                        code = code.Substring(0,2);
					}
				}
				catch(Exception ex)
                                {
                                        log.Info(Context, ex, "Culture: {0}", code);
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
				LoginButton.Text = GetString("LOGIN");
				HelpButton.Text = GetString("HELP");
				
				
				// help
				code = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
                                if(code.StartsWith("zh") || code.StartsWith("pt"))
                                {
                                        // making this change because for chinese, two letter is not sufficient
                                        code = Thread.CurrentThread.CurrentUICulture.Name;
                                }
				HelpButton.NavigateUrl = String.Format("help/{0}/login.html", code);

                        	// new language cookie for 30 days
                        	Response.Cookies["language"].Value = code;
                        	Response.Cookies["language"].Expires = DateTime.Now + TimeSpan.FromDays(30);
                        	Response.Cookies["language"].Path = "/ifolder/";

				// check browser version
				CheckBrowserVersion();
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

		/// <summary>
                /// Event handler that gets called when language is changed.
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                protected void LanguageSelectChanged( object sender, EventArgs e )
                {
			string code = "en";
                        code = LanguageList.SelectedValue;
                        Thread.CurrentThread.CurrentUICulture =
                        			CultureInfo.CreateSpecificCulture(code);
                        LanguageList.Items.Clear();
                        foreach(ListItem language in languages)
                        {
                               language.Text = rm.GetString(language.Text);
                        }

                        // sort languages
                        Array.Sort(languages, ListItemTextComparer.Default);

                        // add languages
                        LanguageList.Items.AddRange(languages);
                        LanguageList.SelectedValue = code;

                        DateTime expires = DateTime.Now + TimeSpan.FromDays(30);
                        // new language cookie for 30 days
                        Response.Cookies["language"].Value = code;
                        Response.Cookies["language"].Expires = expires;
                        Response.Cookies["language"].Path = "/ifolder/";

                        // strings
                        LoginButton.Text = rm.GetString("LOGIN");
                        Response.Redirect("Login.aspx");
		}

		/// <summary>
		/// Check the browser version
		/// </summary>
		private void CheckBrowserVersion()
		{
			// IE
			Regex regexIE = new Regex(@"MSIE (?'version'\d+\.\d+)");
			const float minVersionIE = 6.0F;
			float versionIE = GetBrowserVersion(Request.UserAgent, regexIE);

			// Firefox
			Regex regexFirefox = new Regex(@"Firefox[ /](?'version'\d+\.\d+)");
			const float minVersionFirefox = 1.5F;
			float versionFirefox = GetBrowserVersion(Request.UserAgent, regexFirefox);

			// Safari
			Regex regexSafari = new Regex(@"Safari[ /](?'version'\d+\.\d+)");
			const float minVersionSafari = 417.9F;
			float versionSafari = GetBrowserVersion(Request.UserAgent, regexSafari);

			// check versions
			// this will be true if none of the browsers are matched or
			// if one is found and the version is below the minimum
			if (((versionIE == 0) || (versionIE < minVersionIE)) 
				&& ((versionFirefox == 0) || (versionFirefox < minVersionFirefox))
				&& ((versionSafari == 0) || (versionSafari < minVersionSafari)))
			{
				BrowserWarning.Text = GetString("MINIMUMBROWSER");
			}
		}

		/// <summary>
		/// Get the browser version
		/// </summary>
		/// <param name="agent"></param>
		/// <param name="regex"></param>
		/// <returns></returns>
		private float GetBrowserVersion(string agent, Regex regex)
		{
			float result = 0;
			Match match;

			if (((match = regex.Match(agent)) != null) && match.Success)
			{
				try
				{
					result = float.Parse(match.Groups["version"].Value);
				}
				catch
				{
					// ignore
				}
			}

			return result;
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
			this.LanguageList.SelectedIndexChanged += new System.EventHandler(this.LanguageSelectChanged);
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
			log.Info(Context, "Client Information: {0}, {1}, {2}",
				   username, Context.Request.UserHostName,
				   Context.Request.UserAgent);
			
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
				iFolderWeb weblogin = new iFolderWeb();

				// update web url
				// always use the original path from the WSDL file
	            string url = Environment.GetEnvironmentVariable("SimiasUrl" );
	            if (url == null) url = System.Configuration.ConfigurationSettings.AppSettings.Get("SimiasUrl");

#if TESTING
				url = "http://localhost:8086";
#endif

				UriBuilder loginUrl = new UriBuilder(url);
				loginUrl.Path = (new Uri(weblogin.Url)).PathAndQuery;
				weblogin.Url = loginUrl.Uri.ToString();

				UTF8Encoding utf8Name = new UTF8Encoding();
                                byte[] encodedCredsByteArray = utf8Name.GetBytes(username);
                                string iFolderUserBase64 = Convert.ToBase64String(encodedCredsByteArray);

				encodedCredsByteArray = utf8Name.GetBytes(password);
                                string iFolderPassBase64 = Convert.ToBase64String(encodedCredsByteArray);



				// credentials
				weblogin.PreAuthenticate = true;
				weblogin.Credentials = new NetworkCredential(iFolderUserBase64, iFolderPassBase64);
			
				// cookies
				weblogin.CookieContainer = new CookieContainer();
				//Initial Login
				iFolderUser loginuser = null;;
				try
				{
					loginuser = weblogin.GetAuthenticatedUser();
				}
				catch ( WebException ex)
				{
					log.Info(Context, ex, "Login Failed");
					string ccode = LanguageList.SelectedValue == null ? "en" : LanguageList.SelectedValue;
					Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(ccode);
                                        if(!HandleException(ex)) throw;
					return;
				}
                                catch (Exception e)
                                {
                                        //for now give a general message
                                        log.Info(Context, e, "Login Failed");
					string ccode = LanguageList.SelectedValue == null ? "en" : LanguageList.SelectedValue;
					Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(ccode);
                                        Message.Text = GetString("LOGIN.UNAUTHORIZED");
					return;
                                }


				url = weblogin.GetHomeServerForUser ( loginuser.UserName, password );

				iFolderWeb web = new iFolderWeb();

				UriBuilder webUrl = new UriBuilder(url);
				webUrl.Path = (new Uri(web.Url)).PathAndQuery;
				
				web.Url = webUrl.Uri.ToString();

				// credentials
				web.PreAuthenticate = true;
				web.Credentials = new NetworkCredential(iFolderUserBase64, iFolderPassBase64);
			
				// cookies
				web.CookieContainer = new CookieContainer();

				// user, system, and server
				iFolderUser user = null;
				try
				{
					user = web.GetAuthenticatedUser();
				}
				catch ( WebException ex)
				{
					log.Info(Context, ex, "Login Failed");
					if(!HandleException(ex)) throw;
					return;
				}
				catch (Exception e)
				{
					//for now give a general message
					log.Info(Context, e, "Login Failed");
                                        Message.Text = GetString("LOGIN.UNAUTHORIZED");
					return;
				}

				Session["Connection"] = web;
				Session["User"] = user;
				iFolderSystem system = web.GetSystem();
				Session["System"] = system;
				iFolderServer server = web.GetHomeServer();
				Session["Server"] = server;

                                encodedCredsByteArray = utf8Name.GetBytes(user.UserName);
                                iFolderUserBase64 = Convert.ToBase64String(encodedCredsByteArray);

				// new username cookie for 30 days
				Response.Cookies.Remove("username");
				Response.Cookies["username"].Value = iFolderUserBase64;
				Response.Cookies["username"].Expires = expires;
				Response.Cookies["username"].Path = "/ifolder/";
				Session["Language"] = "en";	
				string code = "en";	
				// ui language
				if (LanguageList.SelectedValue == null || LanguageList.SelectedValue == String.Empty)
				{
					/// Case when single sign on happens without the login page so no language selected
					if((Request.UserLanguages != null ) && (Request.UserLanguages.Length > 0))
       	                                {
               	                                code = Request.UserLanguages[0];
						if(!( code.StartsWith("zh") || code.StartsWith("pt") ))
							code = code.Substring(0,2);	
                                       	        Session["Language"] = code;
                                        }
				}
				else
				{
					/// Normal login so select from the list 
					code = LanguageList.SelectedValue;
					Session["Language"] = LanguageList.SelectedValue;
				}

				// settings
				WebSettings settings = new WebSettings(web);
				Session["Settings"] = settings;

				Response.Cookies["language"].Value = code;
                                Response.Cookies["language"].Expires = expires;
                                Response.Cookies["language"].Path = "/ifolder/";

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

				throw;
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
			try
			{
				// simias error
				string error = e.Response.Headers["Simias-Error"];

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
								HttpStatusCode code = (e.Response as HttpWebResponse).StatusCode;

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
			}
			catch
			{
				// MONO work-around
				// The response object is being prematurely disposed on Mono.
				// For now assume the most common error.
				// cookies
                                HttpCookie languageCookie = Request.Cookies["language"];

                                // culture info
                                string code = "en";

                                if ((Context.Request != null) && (Request.UserLanguages.Length > 0) && (Request.UserLanguages != null ))
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
				Message.Text = GetString("LOGIN.UNAUTHORIZED");
				
			}
			return result;
		}
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

