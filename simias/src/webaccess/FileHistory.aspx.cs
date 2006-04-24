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

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// File History Page
	/// </summary>
	public class FileHistoryPage : Page
	{
		/// <summary>
		/// History Data
		/// </summary>
		protected DataGrid HistoryData;

		/// <summary>
		/// History Pagging
		/// </summary>
		protected Pagging HistoryPagging;

		/// <summary>
		/// Message Box
		/// </summary>
		protected Message MessageBox;

		/// <summary>
		/// The Close Button
		/// </summary>
		protected Button CloseButton;

		/// <summary>
		/// The iFolder Name
		/// </summary>
		protected Literal EntryName;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// iFolder ID
		/// </summary>
		private string ifolderID;

		/// <summary>
		/// Entry ID
		/// </summary>
		private string entryID;

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, EventArgs e)
		{
			// query
			ifolderID = Request.QueryString.Get("iFolder");
			entryID = Request.QueryString.Get("Entry");

			// connection
			web = (iFolderWeb)Session["Connection"];

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				// data
				BindData();

				// strings
				CloseButton.Text = GetString("CLOSE");
				HistoryPagging.LabelSingular = GetString("CHANGE");
				HistoryPagging.LabelPlural = GetString("CHANGES");

				// view
				ViewState["Referrer"] = Request.UrlReferrer;
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			try
			{
				// entry
				iFolderEntry entry = web.GetEntry(ifolderID, entryID);
				EntryName.Text = entry.Path;
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			BindHistoryData();
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindHistoryData()
		{
			int total = 0;

			// history
			DataTable historyTable = new DataTable();
			historyTable.Columns.Add("Time");
			historyTable.Columns.Add("Type");
			historyTable.Columns.Add("UserFullName");
			historyTable.Columns.Add("Image");

			try
			{
				// history
				ChangeEntry[] changes = web.GetChanges(ifolderID, entryID, HistoryPagging.Index, HistoryPagging.PageSize, out total);
				HistoryPagging.Count = changes.Length;
				HistoryPagging.Total = total;
				
				foreach(ChangeEntry change in changes)
				{
					DataRow row = historyTable.NewRow();

					row["Time"] = WebUtility.FormatDateTime(change.Time, rm);
					row["Type"] = WebUtility.FormatChangeType(change.Type, rm);
					row["UserFullName"] = change.UserFullName;
					row["Image"] = change.Type.ToString().ToLower();

					historyTable.Rows.Add(row);
				}
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// bind
			HistoryData.DataSource = historyTable;
			HistoryData.DataBind();
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
					MessageBox.Text = GetString("ENTRY.ACCESSEXCEPTION");
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
			this.HistoryPagging.PageChange += new EventHandler(HistoryPagging_PageChange);
			this.CloseButton.Click += new EventHandler(CloseButton_Click);
		}

		#endregion

		/// <summary>
		/// History Page Change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HistoryPagging_PageChange(object sender, EventArgs e)
		{
			BindHistoryData();
		}

		/// <summary>
		/// Close Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CloseButton_Click(object sender, EventArgs e)
		{
			Uri referrer = (Uri) ViewState["Referrer"];
			string url;

			if ((referrer == null) || (referrer.AbsolutePath.IndexOf("Login.aspx") != -1))
			{
				url = "Browse.aspx?iFolder=" + ifolderID;
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
