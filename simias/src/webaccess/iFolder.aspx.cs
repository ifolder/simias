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
		/// The iFolder Name
		/// </summary>
		protected Literal iFolderContextName;

		/// <summary>
		/// The Add Button
		/// </summary>
		protected HyperLink AddButton;

		/// <summary>
		/// The Remove Button
		/// </summary>
		protected LinkButton RemoveButton;

		/// <summary>
		/// The Read Only Button
		/// </summary>
		protected LinkButton ReadOnlyButton;

		/// <summary>
		/// The Read/Write Button
		/// </summary>
		protected LinkButton ReadWriteButton;

		/// <summary>
		/// The Admin Button
		/// </summary>
		protected LinkButton AdminButton;

		/// <summary>
		/// The Owner Button
		/// </summary>
		protected LinkButton OwnerButton;

		/// <summary>
		/// iFolder Button
		/// </summary>
		protected HyperLink iFolderButton;
		
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
		/// iFolder Member Count
		/// </summary>
		protected Literal iFolderMemberCount;

		/// <summary>
		/// iFolder File Count
		/// </summary>
		protected Literal iFolderFileCount;

		/// <summary>
		/// iFolder Folder Count
		/// </summary>
		protected Literal iFolderFolderCount;

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

				// url
				AddButton.NavigateUrl = "Share.aspx?iFolder=" + ifolderID;
				iFolderButton.NavigateUrl = "Entries.aspx?iFolder=" + ifolderID;

				// strings
				HomeButton.Text = GetString("HOME");
				AddButton.Text = GetString("ADD");
				RemoveButton.Text = GetString("REMOVE");
				ReadOnlyButton.Text = GetString("RIGHTS.READONLY");
				ReadWriteButton.Text = GetString("RIGHTS.READWRITE");
				AdminButton.Text = GetString("RIGHTS.ADMIN");
				OwnerButton.Text = GetString("OWNER");
				MemberPagging.LabelSingular = GetString("MEMBER");
				MemberPagging.LabelPlural = GetString("MEMBERS");
				HistoryPagging.LabelSingular = GetString("CHANGE");
				HistoryPagging.LabelPlural = GetString("CHANGES");
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
				iFolderDetails ifolder = web.GetiFolderDetails(ifolderID);
				iFolderContextName.Text = ifolder.Name;

				iFolderButton.Text = ifolder.Name;
				iFolderDescription.Text = ifolder.Description;
				iFolderOwner.Text = ifolder.OwnerFullName;
				iFolderSize.Text = WebUtility.FormatSize(ifolder.Size, rm);
				iFolderMemberCount.Text = ifolder.MemberCount.ToString();
				iFolderFileCount.Text = ifolder.FileCount.ToString();
				iFolderFolderCount.Text = ifolder.DirectoryCount.ToString();
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
			memberTable.Columns.Add("ID");
			memberTable.Columns.Add("Name");
			memberTable.Columns.Add("Rights");
			memberTable.Columns.Add("IsOwner", typeof(bool));

			try
			{
				// member
				iFolderUser[] members = web.GetMembers(ifolderID, MemberPagging.Index, MemberPagging.PageSize, out total);
				MemberPagging.Count = members.Length;
				MemberPagging.Total = total;
				
				foreach(iFolderUser member in members)
				{
					DataRow row = memberTable.NewRow();

					row["ID"] = member.ID;
					row["Name"] = member.FullName;
					row["Rights"] = member.IsOwner ? GetString("OWNER") : WebUtility.FormatRights(member.Rights, rm);
					row["IsOwner"] = member.IsOwner;

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
			this.MemberPagging.PageChange += new EventHandler(MemberPagging_PageChange);
			this.HistoryPagging.PageChange += new EventHandler(HistoryPagging_PageChange);
			this.RemoveButton.Click += new EventHandler(RemoveButton_Click);
			this.ReadOnlyButton.Click += new EventHandler(ReadOnlyButton_Click);
			this.ReadWriteButton.Click += new EventHandler(ReadWriteButton_Click);
			this.AdminButton.Click += new EventHandler(AdminButton_Click);
			this.OwnerButton.Click += new EventHandler(OwnerButton_Click);
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

		/// <summary>
		/// Get the Selected Members
		/// </summary>
		/// <returns>A comma-delimited list of member user ids.</returns>
		private string GetSelectedMembers()
		{
			string memberList = null;

			// selected members
			foreach(DataGridItem item in MemberData.Items)
			{
				CheckBox checkBox = (CheckBox) item.FindControl("Select");

				if (checkBox.Checked)
				{
					string id = item.Cells[0].Text;

					if (memberList == null)
					{
						memberList = id;
					}
					else
					{
						memberList = String.Format("{0},{1}", memberList, id);
					}
				}
			}

			return memberList;
		}

		/// <summary>
		/// Remove Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveButton_Click(object sender, EventArgs e)
		{
			string memberList = GetSelectedMembers();

			// remove members
			if (memberList != null)
			{
				try
				{
					web.RemoveMember(ifolderID, memberList);
				}
				catch(SoapException ex)
				{
					HandleException(ex);
				}

				MemberPagging.Index = 0;
				BindMemberData();
			}
		}

		/// <summary>
		/// Set Rights
		/// </summary>
		/// <param name="rights"></param>
		private void SetRights(Rights rights)
		{
			string memberList = GetSelectedMembers();

			// remove members
			if (memberList != null)
			{
				try
				{
					web.SetMemberRights(ifolderID, memberList, rights);
				}
				catch(SoapException ex)
				{
					HandleException(ex);
				}

				BindMemberData();
			}
		}

		/// <summary>
		/// Read Only Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReadOnlyButton_Click(object sender, EventArgs e)
		{
			SetRights(Rights.ReadOnly);
		}

		/// <summary>
		/// Read/Write Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReadWriteButton_Click(object sender, EventArgs e)
		{
			SetRights(Rights.ReadWrite);
		}

		/// <summary>
		/// Admin Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AdminButton_Click(object sender, EventArgs e)
		{
			SetRights(Rights.Admin);
		}

		/// <summary>
		/// Owner Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OwnerButton_Click(object sender, EventArgs e)
		{
			string ownerID = GetSelectedMembers();

			// remove members
			if (ownerID != null)
			{
				try
				{
					web.SetiFolderOwner(ifolderID, ownerID);
				}
				catch(SoapException ex)
				{
					HandleException(ex);
				}

				BindMemberData();
			}
		}
	}
}
