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
	/// History Page
	/// </summary>
	public class HistoryPage : Page
	{
		/// <summary>
		/// History Data
		/// </summary>
		protected DataGrid HistoryData;

		/// <summary>
		/// History Pagging
		/// </summary>
		protected PaggingControl HistoryPagging;

		/// <summary>
		/// Message Box
		/// </summary>
		protected MessageControl Message;

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
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, EventArgs e)
		{
			// query
			ifolderID = Request.QueryString.Get("iFolder");

			// connection
			web = (iFolderWeb) Session["Connection"];

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				// data
				BindData();

				// strings
				HistoryPagging.LabelSingular = GetString("CHANGE");
				HistoryPagging.LabelPlural = GetString("CHANGES");
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			BindHistoryData();
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindHistoryData()
		{
			// history
			DataTable historyTable = new DataTable();
			historyTable.Columns.Add("ID");
			historyTable.Columns.Add("iFolderID");
			historyTable.Columns.Add("Time");
			historyTable.Columns.Add("Name");
			historyTable.Columns.Add("ShortName");
			historyTable.Columns.Add("Type");
			historyTable.Columns.Add("Action");
			historyTable.Columns.Add("UserFullName");
			historyTable.Columns.Add("TypeImage");
			historyTable.Columns.Add("ActionImage");

			try
			{
				// ifolder
				iFolder ifolder = web.GetiFolder(ifolderID);

				// history
				ChangeEntrySet changes = web.GetChanges(ifolderID, null, HistoryPagging.Index, HistoryPagging.PageSize);
				HistoryPagging.Count = changes.Items.Length;
				HistoryPagging.Total = changes.Total;
				
				foreach(ChangeEntry change in changes.Items)
				{
					DataRow row = historyTable.NewRow();

					row["ID"] = change.ID;
					row["iFolderID"] = ifolderID;
					row["Time"] = WebUtility.FormatDateTime(change.Time, rm);
					row["Name"] = change.Name;
					row["ShortName"] = WebUtility.GetFileName(change.Name);
					row["Type"] = change.Type.ToString();
					row["Action"] = WebUtility.FormatChangeAction(change.Action, rm);
					row["UserFullName"] = change.UserFullName;
					row["TypeImage"] = change.Type.ToString().ToLower();
					row["ActionImage"] = change.Action.ToString().ToLower();
					
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
			this.HistoryPagging.PageChange += new EventHandler(HistoryPagging_PageChange);
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
	}
}
