/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com
 |
 | Author: Mike Lasky (mlasky@novell.com)
 |***************************************************************************/

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
	/// Summary description for WebForm1.
	/// </summary>
	public class iFolderAdminPage : System.Web.UI.Page
	{
		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;
	
		private void Page_Load(object sender, System.EventArgs e)
		{
			if ( !IsPostBack )
			{
				// localization
				rm = Application[ "RM" ] as ResourceManager;
			}
		}

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
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString(string key)
		{
			return rm.GetString(key);
		}
	}
}
