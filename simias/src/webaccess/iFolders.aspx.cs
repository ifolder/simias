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
using System.Text.RegularExpressions;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// iFolders Page
	/// </summary>
	public class iFoldersPage : Page
	{
		/// <summary>
		/// Context
		/// </summary>
		protected HomeContextControl HomeContext;

		/// <summary>
		/// New iFolder Link
		/// </summary>
		protected HyperLink NewiFolderLink;

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
		protected PaggingControl iFolderPagging;

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
				RemoveButton.Text = GetString("REMOVEMEMBERSHIP");

				// data
				BindData();
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
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

			// category
			iFolderCategory category = HomeContext.Category;
			MemberRole role = MemberRole.Any;
			DateTime after = DateTime.MinValue;

			switch(category)
			{
				case iFolderCategory.Recent:
					after = DateTime.Now.AddDays(-30);
					break;

				case iFolderCategory.Owned:
					role = MemberRole.Owner;
					break;

				case iFolderCategory.Shared:
					role = MemberRole.Shared;
					break;

				case iFolderCategory.All:
				default:
					break;
			}

			try
			{
				// data
				iFolderSet ifolders = web.GetiFoldersBySearch(role, after, SearchOperation.Contains,
					HomeContext.Pattern, iFolderPagging.Index, iFolderPagging.PageSize);
				iFolderPagging.Count = ifolders.Items.Length;
				iFolderPagging.Total = ifolders.Total;
				
				string name;
				bool pattern = (HomeContext.Pattern != null) && (HomeContext.Pattern.Length > 0);

				foreach(iFolder ifolder in ifolders.Items)
				{
					DataRow row = ifolderTable.NewRow();

					// selected name
					if (pattern)
					{
						name = Regex.Replace(ifolder.Name, String.Format("({0})", HomeContext.Pattern),
							"<span class='highlight'>${1}</span>", RegexOptions.IgnoreCase);
					}
					else
					{
						name = ifolder.Name;
					}

					row["ID"] = ifolder.ID;
					row["Image"] = "ifolder.png";
					row["Name"] = name;
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
			this.RemoveButton.PreRender += new EventHandler(RemoveButton_PreRender);
			this.RemoveButton.Click += new EventHandler(RemoveButton_Click);
			this.HomeContext.Search += new EventHandler(HomeContext_Search);
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
					Message.Text = GetString("REMOVEOWNEREXCEPTION");
					break;

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
		/// iFolder Page Change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void iFolderPagging_PageChange(object sender, EventArgs e)
		{
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
					web.RemoveMembership(ifolderList);
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
		/// Home Context Search Event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HomeContext_Search(object sender, EventArgs e)
		{
			// reset paging
			iFolderPagging.Index = 0;

			BindData();
		}
	}
}
