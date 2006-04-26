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
using System.Net;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Resources;
using System.Web.Security;
using System.Web.Services.Protocols;
using System.Xml;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// iFolders Page
	/// </summary>
	public class iFoldersPage : Page
	{
		/// <summary>
		/// New iFolder Link
		/// </summary>
		protected HyperLink NewiFolderLink;

		/// <summary>
		/// Actions Container
		/// </summary>
		protected HtmlContainerControl Tabs;

		/// <summary>
		/// Show All iFolders Button
		/// </summary>
		protected LinkButton AllButton;

		/// <summary>
		/// Show New iFolders Button
		/// </summary>
		protected LinkButton NewButton;

		/// <summary>
		/// Show Owner iFolders Button
		/// </summary>
		protected LinkButton OwnerButton;

		/// <summary>
		/// Show Shared iFolders Button
		/// </summary>
		protected LinkButton SharedButton;

		/// <summary>
		/// Search Pattern
		/// </summary>
		protected TextBox SearchPattern;

		/// <summary>
		/// Search Button
		/// </summary>
		protected Button SearchButton;

		/// <summary>
		/// The Remove Button
		/// </summary>
		protected LinkButton RemoveButton;

		/// <summary>
		/// iFolder Data
		/// </summary>
		protected DataGrid iFolderData;

		/// <summary>
		/// Pagging
		/// </summary>
		protected Pagging iFolderPagging;

		/// <summary>
		/// Message Box
		/// </summary>
		protected Message MessageBox;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		private enum Mode
		{
			All,
			New,
			Owner,
			Shared
		};

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
				// strings
				iFolderPagging.LabelSingular = GetString("IFOLDER");
				iFolderPagging.LabelPlural = GetString("IFOLDERS");
				NewiFolderLink.Text = GetString("NEWIFOLDER");
				SearchButton.Text = GetString("FILTER");
				RemoveButton.Text = GetString("REMOVE");
				AllButton.Text = GetString("ALL");
				NewButton.Text = GetString("NEW");
				OwnerButton.Text = GetString("OWNER");
				SharedButton.Text = GetString("SHARED");

				// search pattern
				ViewState["SearchPattern"] = null;
				ViewState["Mode"] = Mode.All;

				// data
				BindData();
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			int total = 0;

			// keep search pattern consistent
			SearchPattern.Text = (string)ViewState["SearchPattern"];

			// table
			DataTable ifolderTable = new DataTable();
			ifolderTable.Columns.Add("ID");
			ifolderTable.Columns.Add("Image");
			ifolderTable.Columns.Add("Name");
			ifolderTable.Columns.Add("LastModified");
			ifolderTable.Columns.Add("Description");
			ifolderTable.Columns.Add("Rights");
			ifolderTable.Columns.Add("Size");
			ifolderTable.Columns.Add("Owner", typeof(bool));

			// mode
			Mode mode = (Mode) ViewState["Mode"];
			MemberRole role = MemberRole.Any;
			DateTime after = DateTime.MinValue;

			switch(mode)
			{
				case Mode.New:
					after = DateTime.Now.AddDays(-30);
					Tabs.Attributes["class"] = "newPage tabs";
					break;

				case Mode.Owner:
					role = MemberRole.Owner;
					Tabs.Attributes["class"] = "ownerPage tabs";
					break;

				case Mode.Shared:
					role = MemberRole.Shared;
					Tabs.Attributes["class"] = "sharedPage tabs";
					break;

				case Mode.All:
				default:
					Tabs.Attributes["class"] = "allPage tabs";
					break;
			}

			try
			{
				// data
				iFolder[] ifolders = web.GetiFoldersBySearch(role, after, SearchOperation.BeginsWith, SearchPattern.Text, iFolderPagging.Index, iFolderPagging.PageSize, out total);
				iFolderPagging.Count = ifolders.Length;
				iFolderPagging.Total = total;
				
				foreach(iFolder ifolder in ifolders)
				{
					DataRow row = ifolderTable.NewRow();

					row["ID"] = ifolder.ID;
					row["Image"] = "ifolder.png";
					row["Name"] = ifolder.Name;
					row["LastModified"] = WebUtility.FormatDate(ifolder.LastModified, rm);
					row["Description"] = ifolder.Description;
					row["Rights"] = WebUtility.FormatRights(ifolder.Rights, rm);
					row["Size"] = WebUtility.FormatSize(ifolder.Size, rm);
					row["Owner"] = ifolder.IsOwner;

					ifolderTable.Rows.Add(row);
				}
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// view
			DataView ifolderView = new DataView(ifolderTable);
			ifolderView.Sort = "Name";
			
			// data grid
			iFolderData.DataKeyField = "ID";
			iFolderData.DataSource = ifolderView;
			iFolderData.DataBind();
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
			this.ID = "iFolderView";
			this.Load += new System.EventHandler(this.Page_Load);
			this.iFolderPagging.PageChange += new EventHandler(iFolderPagging_PageChange);
			this.SearchButton.Click += new EventHandler(SearchButton_Click);
			this.RemoveButton.PreRender += new EventHandler(RemoveButton_PreRender);
			this.RemoveButton.Click += new EventHandler(RemoveButton_Click);
			this.AllButton.Click += new EventHandler(AllButton_Click);
			this.NewButton.Click += new EventHandler(NewButton_Click);
			this.OwnerButton.Click += new EventHandler(OwnerButton_Click);
			this.SharedButton.Click += new EventHandler(SharedButton_Click);
		}

		#endregion

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
				case "InvalidOperationException":
					MessageBox.Text = GetString("REMOVEOWNEREXCEPTION");
					break;

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
		/// iFolder Page Change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void iFolderPagging_PageChange(object sender, EventArgs e)
		{
			BindData();
		}

		private void SearchButton_Click(object sender, EventArgs e)
		{
			// update search pattern
			ViewState["SearchPattern"] = SearchPattern.Text;

			// reset index
			iFolderPagging.Index = 0;

			BindData();
		}

		/// <summary>
		/// Remove Button Pre-Render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveButton_PreRender(object sender, EventArgs e)
		{
			RemoveButton.Attributes["onclick"] = "return ConfirmRemove(this.form);";
		}

		/// <summary>
		/// Remove Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveButton_Click(object sender, EventArgs e)
		{
			string ifolderList = null;

			// selected iFolders
			foreach(DataGridItem item in iFolderData.Items)
			{
				CheckBox checkBox = (CheckBox) item.FindControl("Select");

				if (checkBox.Checked)
				{
					string id = item.Cells[0].Text;

					if (ifolderList == null)
					{
						ifolderList = id;
					}
					else
					{
						ifolderList = String.Format("{0},{1}", ifolderList, id);
					}
				}
			}

			// remove from iFolders
			if (ifolderList != null)
			{
				try
				{
					web.RemoveiFolder(ifolderList);
				}
				catch(SoapException ex)
				{
					if (!HandleException(ex)) throw;
				}

				iFolderPagging.Index = 0;
				BindData();
			}
		}

		/// <summary>
		/// Switch the page
		/// </summary>
		/// <param name="mode"></param>
		private void SwitchPage(Mode mode)
		{
			ViewState["Mode"] = mode;

			// reset search
			ViewState["SearchPattern"] = null;

			// reset index
			iFolderPagging.Index = 0;

			BindData();
		}

		/// <summary>
		/// All Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AllButton_Click(object sender, EventArgs e)
		{
			SwitchPage(Mode.All);
		}

		/// <summary>
		/// New Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NewButton_Click(object sender, EventArgs e)
		{
			SwitchPage(Mode.New);
		}

		/// <summary>
		/// Owner Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OwnerButton_Click(object sender, EventArgs e)
		{
			SwitchPage(Mode.Owner);
		}

		/// <summary>
		/// Shared Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SharedButton_Click(object sender, EventArgs e)
		{
			SwitchPage(Mode.Shared);
		}
	}
}
