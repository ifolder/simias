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
		/// Different Tabs
		/// </summary>
		protected TabControl Tabs;

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
			
				iFolder ifolder = web.GetiFolder(ifolderID);
				
				//Load this page only if passphrase was provided for the encrypted iFolder
				if(! IfHistoryEnabled())
				{
					// Return to Browse page, if passphrase was not provided for that encrypted ifolder.
					
					Response.Redirect(String.Format("Browse.aspx?iFolder={0}", ifolderID));
				}
				
				string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
				if(!(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty)))
				{
					// It is an encrypted ifolder , Make the Members tab invisible^M
					Tabs.MembersLink.Visible = false;
				}
			
				// data
				BindData();

				// strings
				HistoryPagging.LabelSingular = GetString("CHANGE");
				HistoryPagging.LabelPlural = GetString("CHANGES");
			}
		}
		
		/// <summary>
		/// Determine to show the history tab or not for encrypted folders
		/// </summary>
		private bool IfHistoryEnabled()
		{
			string PassPhrase = Session["SessionPassPhrase"] as string;
			ifolderID = Request.QueryString.Get("iFolder");
			iFolder ifolder = web.GetiFolder(ifolderID);
			string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
			return web.ShowTabDetails(PassPhrase, EncryptionAlgorithm);	
				
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
			historyTable.Columns.Add("NewRights");

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
					
					if ((change.Type == ChangeEntryType.Member) && (change.Action != ChangeEntryAction.Delete))
					{
						row["NewRights"] = String.Format("({0})", WebUtility.FormatRights(change.MemberNewRights, rm));
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
