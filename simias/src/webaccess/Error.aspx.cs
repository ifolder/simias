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

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Error Page
	/// </summary>
	public class Error : System.Web.UI.Page
	{
		/// <summary>
		/// Div
		/// </summary>
		protected HtmlGenericControl DetailsButtonRegion;
		
		/// <summary>
		/// Error Type
		/// </summary>
		protected Label ErrorType;
		
		/// <summary>
		/// Error Instructions
		/// </summary>
		protected Label ErrorInstructions;

		/// <summary>
		/// Error Message
		/// </summary>
		protected Literal ErrorMessage;

		/// <summary>
		/// Error Stack Trace
		/// </summary>
		protected Literal ErrorStackTrace;

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
			ErrorType.Text = GetString("ERROR.TYPE");
			ErrorInstructions.Text = GetString("ERROR.INSTRUCTIONS");

			// message from query string
			string message = Request.QueryString.Get("Exception");

			if ((message == null) || (message.Length < 0))
			{
				// message from session
				message = null;

				Exception ex = (Exception)Session["Exception"];

				if (ex != null)
				{
					message = ex.ToString();
				}
			}
			
			// did we find a message
			if (message != null)
			{
				ErrorMessage.Text = "\n\n" + message + "\n";
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
		}

		#endregion
	}
}
