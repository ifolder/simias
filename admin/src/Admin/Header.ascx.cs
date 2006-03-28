/***********************************************************************
 *  $RCSfile: Header.ascx.cs,v $
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
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
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Threading;
using System.Resources;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	///	Header
	/// </summary>
	public class Header : UserControl
	{
		/// <summary>
		/// Log
		/// </summary>
		private static readonly iFolderWebLogger log = new iFolderWebLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Logout Button
		/// </summary>
		protected LinkButton LogoutButton;
		
		/// <summary>
		/// Help Button
		/// </summary>
		protected HyperLink HelpButton;
		
		/// <summary>
		/// User Name
		/// </summary>
		protected Literal UserName;

		/// <summary>
		/// System Name
		/// </summary>
		protected Literal SystemName;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;
	
		/// <summary>
		/// Max Header String Length
		/// </summary>
		private readonly static int MAX_HEADER_STRING = 30;

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = (ResourceManager) Application["RM"];

			// check connection
			iFolderAdmin web = (iFolderAdmin)Session["Connection"];
			if (web == null) Logout(rm.GetString("MESSAGEINFORMATION"), rm.GetString("LOGINLOSTSESSION"));
			
			if (!IsPostBack)
			{
				// username
				UserName.Text = Trim((string)Session["Name"], MAX_HEADER_STRING);
				
				// system name
				SystemName.Text = String.Format("{0} - {1}", Trim((string)Session["System"], MAX_HEADER_STRING),
					(string)Session["Version"]);

				// strings
				LogoutButton.Text = rm.GetString("LOGOUT");
				HelpButton.Text = rm.GetString("HELP");

				// help
				HelpButton.NavigateUrl = String.Format("help/{0}/index.html",
					Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
			}
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
				result = String.Format("{0}{1}", text.Substring(0, length), rm.GetString("ELLIPSES"));
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
			return rm.GetString(key);
		}

		#region Web Form Designer generated code
		
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
			this.LogoutButton.Click += new System.EventHandler(this.LogoutButton_Click);
			this.Load += new System.EventHandler(this.Page_Load);
		}
		
		#endregion

		/// <summary>
		/// Logout Button Handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LogoutButton_Click(object sender, System.EventArgs e)
		{
			Logout(rm.GetString("MESSAGEINFORMATION"), rm.GetString("LOGINLOGOUT"));
		}

		private void Logout(string type, string message)
		{
			FormsAuthentication.SignOut();
			
			// double-check that the session is abandoned
			Session.Abandon();

			log.Info(Context, "Logout Successful");

			Response.Redirect(String.Format(
				"Login.aspx?MessageType={0}&MessageText={1}",
				Context.Server.UrlEncode(type),
				Context.Server.UrlEncode(message)));
		}
	}
}
