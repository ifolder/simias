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
using System.Web.Services.Protocols;
using System.Net;
using System.Xml;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Settings Page
	/// </summary>
	public class SettingsPage : Page
	{
		/// <summary>
		/// Page Size Label
		/// </summary>
		protected Label PageSizeLabel;
		
		/// <summary>
		/// Page Size List
		/// </summary>
		protected DropDownList PageSizeList;

		/// <summary>
		/// Message Box
		/// </summary>
		protected MessageControl Message;

		/// <summary>
		/// The Save Button
		/// </summary>
		protected Button SaveButton;

		/// <summary>
		/// The Cancel Button
		/// </summary>
		protected Button CancelButton;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;

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
			// connection
			web = (iFolderWeb)Session["Connection"];

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				// data
				BindData();

				// strings
				SaveButton.Text = GetString("SAVE");
				CancelButton.Text = GetString("CANCEL");
				PageSizeLabel.Text = GetString("PAGESIZE");

				// view
				ViewState["Referrer"] = Request.UrlReferrer;
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			// page size options
			string[] sizes = { "10", "25", "50", "100" };

			foreach(string size in sizes)
			{
				PageSizeList.Items.Add(size);
			}

			try
			{
				// load
				WebSettings settings = new WebSettings(web);
				string pageSizeString = settings.PageSize.ToString();

				// page size
				foreach(ListItem item in PageSizeList.Items)
				{
					if (item.Value == pageSizeString)
					{
						item.Selected = true;
					}
				}
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
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
				case "AccessException":
					Message.Text = GetString("ENTRY.ACCESSEXCEPTION");
					break;

				default:
					result = false;
					break;
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
			this.SaveButton.Click += new EventHandler(SaveButton_Click);
			this.CancelButton.Click += new EventHandler(CancelButton_Click);
		}

		#endregion

		/// <summary>
		/// Save Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveButton_Click(object sender, EventArgs e)
		{
			try
			{
				// load
				WebSettings settings = new WebSettings(web);
				
				// page size
				settings.PageSize = int.Parse(PageSizeList.SelectedValue);

				// save
				settings.Save(web);

				Session["Settings"] = settings;
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
			
			// return
			CancelButton_Click(sender, e);
		}

		/// <summary>
		/// Cancel Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CancelButton_Click(object sender, EventArgs e)
		{
			// return
			Uri referrer = (Uri) ViewState["Referrer"];
			string url;

			if ((referrer == null) || (referrer.AbsolutePath.IndexOf("Login.aspx") != -1)
				|| (referrer.AbsolutePath.IndexOf("Settings.aspx") != -1))
			{
				url = "iFolders.aspx";
			}
			else
			{
				url = referrer.ToString();
			}
			
			// redirect
			Response.Redirect(url);
		}
	}
}
