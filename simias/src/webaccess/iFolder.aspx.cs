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
	/// iFolder Page
	/// </summary>
	public class iFolderPage : Page
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
		/// Member Data
		/// </summary>
		protected DataGrid MemberData;

		/// <summary>
		/// Member Pagging
		/// </summary>
		protected Pagging MemberPagging;

		/// <summary>
		/// Message Box
		/// </summary>
		protected Message MessageBox;

		/// <summary>
		/// The Home Button
		/// </summary>
		protected HyperLink HomeButton;

		/// <summary>
		/// Browse Button
		/// </summary>
		protected HyperLink BrowseButton;

		/// <summary>
		/// The iFolder Name
		/// </summary>
		protected Literal iFolderContextName;

		/// <summary>
		/// The Share Button
		/// </summary>
		protected HyperLink ShareButton;

		/// <summary>
		/// iFolder Name
		/// </summary>
		protected Literal iFolderName;
		
		/// <summary>
		/// iFolder Description
		/// </summary>
		protected Literal iFolderDescription;
		
		/// <summary>
		/// iFolder Owner
		/// </summary>
		protected Literal iFolderOwner;
		
		/// <summary>
		/// iFolder Size
		/// </summary>
		protected Literal iFolderSize;

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
			web = (iFolderWeb)Session["Connection"];

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				// data
				BindData();

				// url
				ShareButton.NavigateUrl = "Share.aspx?iFolder=" + ifolderID;
				BrowseButton.NavigateUrl = "Entries.aspx?iFolder=" + ifolderID;

				// strings
				HomeButton.Text = rm.GetString("HOME");
				ShareButton.Text = rm.GetString("SHARE");
				BrowseButton.Text = rm.GetString("BROWSE");
				MemberPagging.LabelSingular = rm.GetString("MEMBER");
				MemberPagging.LabelPlural = rm.GetString("MEMBERS");
				HistoryPagging.LabelSingular = rm.GetString("CHANGE");
				HistoryPagging.LabelPlural = rm.GetString("CHANGES");
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			try
			{
				// ifolder
				iFolder ifolder = web.GetiFolder(ifolderID);
				iFolderContextName.Text = ifolder.Name;

				iFolderName.Text = ifolder.Name;
				iFolderDescription.Text = ifolder.Description;
				iFolderOwner.Text = ifolder.OwnerName;
				iFolderSize.Text = WebUtility.FormatSize(ifolder.Size, rm);
			}
			catch(SoapException ex)
			{
				HandleException(ex);
			}

			BindMemberData();
			BindHistoryData();
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindMemberData()
		{
			int total = 0;

			// member
			DataTable memberTable = new DataTable();
			memberTable.Columns.Add("Name");
			memberTable.Columns.Add("Rights");

			try
			{
				// member
				iFolderUser[] members = web.GetMembers(ifolderID, MemberPagging.Index, MemberPagging.PageSize, out total);
				MemberPagging.Count = members.Length;
				MemberPagging.Total = total;
				
				foreach(iFolderUser member in members)
				{
					DataRow row = memberTable.NewRow();

					row["Name"] = member.FullName;
					row["Rights"] = WebUtility.FormatRights(member.Rights, rm);

					memberTable.Rows.Add(row);
				}
			}
			catch(SoapException ex)
			{
				HandleException(ex);
			}

			// bind
			MemberData.DataSource = memberTable;
			MemberData.DataBind();
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
			historyTable.Columns.Add("EntryName");
			historyTable.Columns.Add("ShortEntryName");
			historyTable.Columns.Add("Type");
			historyTable.Columns.Add("UserFullName");
			historyTable.Columns.Add("Image");
			historyTable.Columns.Add("Link");

			try
			{
				// history
				ChangeEntry[] changes = web.GetChanges(ifolderID, null, HistoryPagging.Index, HistoryPagging.PageSize, out total);
				HistoryPagging.Count = changes.Length;
				HistoryPagging.Total = total;
				
				foreach(ChangeEntry change in changes)
				{
					DataRow row = historyTable.NewRow();

					row["Time"] = WebUtility.FormatDateTime(change.Time, rm);
					row["EntryName"] = change.EntryName;
					row["ShortEntryName"] = WebUtility.GetFileName(change.EntryName);
					row["Type"] = WebUtility.FormatChangeType(change.Type, rm);
					row["UserFullName"] = change.UserFullName;
					row["Image"] = change.Type.ToString().ToLower();
					
					if (change.IsDirectory)
					{
						row["Link"] = String.Format("Entries.aspx?iFolder={0}&Entry={1}", ifolderID, change.EntryID);
					}
					else
					{
						row["Link"] = String.Format("Download.ashx?iFolder={0}&Entry={1}", ifolderID, change.EntryID);
					}

					historyTable.Rows.Add(row);
				}
			}
			catch(SoapException ex)
			{
				HandleException(ex);
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

			string type = e.GetType().Name;

			if (e is SoapException)
			{
				type = WebUtility.GetSmartExceptionType(e as SoapException);
			}
			else if (e is WebException)
			{
				type = WebUtility.GetWebExceptionType(e as WebException);	
			}

			// types
			switch(type)
			{
				case "AccessException":
					MessageBox.Text = rm.GetString("ENTRY.ACCESSEXCEPTION");
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
			string result = rm.GetString(key);

			if ((result == null) || (result.Length == 0))
			{
				result = key;
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
			this.Load += new System.EventHandler(this.Page_Load);
			this.MemberPagging.PageChange += new EventHandler(MemberPagging_PageChange);
			this.HistoryPagging.PageChange += new EventHandler(HistoryPagging_PageChange);
		}

		#endregion

		/// <summary>
		/// Member Page Change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MemberPagging_PageChange(object sender, EventArgs e)
		{
			BindMemberData();
		}

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
