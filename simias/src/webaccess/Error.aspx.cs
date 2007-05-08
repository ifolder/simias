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
using System.Resources;
using System.IO;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Error Page
	/// </summary>
	public class Error : System.Web.UI.Page
	{
		/// <summary>
		/// Error Instructions
		/// </summary>
		protected Label ErrorMessage;

		/// <summary>
		/// Login Button
		/// </summary>
		protected Button LoginButton;

		/// <summary>
		/// Error Details
		/// </summary>
		protected TextBox ErrorDetails;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = (ResourceManager) Application["RM"];
				
			// strings
			ErrorMessage.Text = GetString("ERROR.MESSAGE");
			LoginButton.Text = GetString("LOGIN");

			// details
			StringWriter details = new StringWriter();

			// message from query string
			string message = Request.QueryString.Get("Exception");

			if ((message != null) && (message.Length != 0))
			{
				details.WriteLine("Message: {0}", message);
				details.WriteLine();
			}
			
			// session additions
			if (Session != null)
			{
				// exception
				Exception ex = (Exception) Session["Exception"];
				if (ex != null)
				{
					details.WriteLine("Exception Message: {0}", ex.Message);
					details.WriteLine("Exception Type: {0}", ex.GetType());
					details.WriteLine("Exception Site: {0}", ex.TargetSite);
					details.WriteLine("Exception Source: {0}", ex.Source);
					details.WriteLine();
					details.WriteLine("Exception Stack:");
					details.WriteLine();
					details.WriteLine(ex);
					details.WriteLine();
				}

				// user
				iFolderUser user = (iFolderUser) Session["User"];
				if (user != null)
				{
					details.WriteLine("Username: {0}", user.UserName);
					details.WriteLine("User Full Name: {0}", user.FullName);
					details.WriteLine();
				}

				// system
				iFolderSystem system = (iFolderSystem) Session["System"];
				if (system != null)
				{
					details.WriteLine("System iFolder Name: {0}", system.Name);
					details.WriteLine("System iFolder Version: {0}", system.Version);
					details.WriteLine();
				}
				
				// server
				iFolderServer server = (iFolderServer) Session["Server"];
				if (server != null)
				{
					details.WriteLine("Server iFolder Version: {0}", server.Version);
					details.WriteLine("Server CLR Version: {0}", server.ClrVersion);
					details.WriteLine("Server Host: {0}", server.HostName);
					details.WriteLine("Server Machine: {0}", server.MachineName);
					details.WriteLine("Server Operating System: {0}", server.OSVersion);
					details.WriteLine("Server Username: {0}", server.UserName);
					details.WriteLine();
				}
			}

			// details
			ErrorDetails.Text = details.ToString();
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
		/// On Intialization
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
			this.LoginButton.Click += new EventHandler(LoginButton_Click);
		}

		#endregion

		/// <summary>
		/// Login Button Clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LoginButton_Click(object sender, EventArgs e)
		{
			Response.Redirect("Login.aspx");
		}
	}
}
