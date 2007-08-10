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
 | Author: Rob
 |***************************************************************************/

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
	/// Item History Page
	/// </summary>
	public class ItemHistoryPage : Page
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
		/// The Close Button
		/// </summary>
		protected Button CloseButton;

		/// <summary>
		/// The Item Name
		/// </summary>
		protected Literal ItemName;

		/// <summary>
		/// The Item Image
		/// </summary>
		protected System.Web.UI.WebControls.Image ItemImage;

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
		/// Item ID
		/// </summary>
		private string itemID;

		/// <summary>
		/// Item Type
		/// </summary>
		private string type;

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, EventArgs e)
		{
			// query
			ifolderID = Request.QueryString.Get("iFolder");
			itemID = Request.QueryString.Get("Item");
			type = Request.QueryString.Get("Type");

			// connection
			web = (iFolderWeb)Session["Connection"];

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				// data
				BindData();

				// image
				ItemImage.ImageUrl = String.Format("images/change-{0}.png", type.ToLower());

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
				ChangeEntryType changeEntryType = (ChangeEntryType) Enum.Parse(typeof(ChangeEntryType), type);

				switch(changeEntryType)
				{
					case ChangeEntryType.File:
					case ChangeEntryType.Directory:
                        // entry
						iFolderEntry entry = web.GetEntry(ifolderID, itemID);
						ItemName.Text = entry.Path;
						break;

					case ChangeEntryType.Member:
						iFolderUser member = web.GetUser(itemID);
						ItemName.Text = member.FullName;
						break;
				}

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
			// history
			DataTable historyTable = new DataTable();
			historyTable.Columns.Add("Time");
			historyTable.Columns.Add("Type");
			historyTable.Columns.Add("UserFullName");
			historyTable.Columns.Add("Image");
			historyTable.Columns.Add("NewRights");

			try
			{
				// history
				ChangeEntrySet changes = web.GetChanges(ifolderID, itemID, HistoryPagging.Index, HistoryPagging.PageSize);
				HistoryPagging.Count = changes.Items.Length;
				HistoryPagging.Total = changes.Total;
				
				foreach(ChangeEntry change in changes.Items)
				{
					DataRow row = historyTable.NewRow();

					row["Time"] = WebUtility.FormatDateTime(change.Time, rm);
					row["Type"] = WebUtility.FormatChangeAction(change.Action, rm);
					row["UserFullName"] = change.UserFullName;
					row["Image"] = change.Action.ToString().ToLower();

					if ((change.Type == ChangeEntryType.Member) && (change.Action != ChangeEntryAction.Delete))
					{
						row["NewRights"] = WebUtility.FormatRights(change.MemberNewRights, rm);
					}
					else
					{
						row["NewRights"] = "";
					}

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
				string TrimmedUrl = web.TrimUrl(referrer.ToString());
				url = TrimmedUrl;
			}
			
			// redirect
			Response.Redirect(url);
		}
	}
}
