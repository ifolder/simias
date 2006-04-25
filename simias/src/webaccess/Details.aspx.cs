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
	/// Details Page
	/// </summary>
	public class DetailsPage : Page
	{
		/// <summary>
		/// iFolder Context
		/// </summary>
		protected Context iFolderContext;

		/// <summary>
		/// Actions Container
		/// </summary>
		protected HtmlContainerControl Actions;

		/// <summary>
		/// Detail Data
		/// </summary>
		protected DataGrid DetailData;

		/// <summary>
		/// iFolder Edit Link
		/// </summary>
		protected HyperLink iFolderEditLink;

		/// <summary>
		/// Delete Button
		/// </summary>
		protected Button DeleteButton;

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
				iFolderEditLink.Text = GetString("EDIT");
				DeleteButton.Text = GetString("DELETE");

				// links
				iFolderEditLink.NavigateUrl = "iFolderEdit.aspx?iFolder=" + ifolderID;
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			// table
			DataTable detailTable = new DataTable();
			detailTable.Columns.Add("Label");
			detailTable.Columns.Add("Value");

			try
			{
				// ifolder
				iFolderDetails ifolder = web.GetiFolderDetails(ifolderID);

				// rights
				Actions.Visible = (ifolder.Rights == Rights.Admin);
				DeleteButton.Enabled = ifolder.IsOwner;

				// context
				iFolderContext.iFolderName = ifolder.Name;

				detailTable.Rows.Add(new object[] { GetString("NAME"), ifolder.Name });
				detailTable.Rows.Add(new object[] { GetString("DESCRIPTION"), ifolder.Description });
				detailTable.Rows.Add(new object[] { GetString("LASTMODIFIED"), WebUtility.FormatDate(ifolder.LastModified, rm) });
				detailTable.Rows.Add(new object[] { GetString("CREATED"), WebUtility.FormatDate(ifolder.Created, rm) });
				detailTable.Rows.Add(new object[] { GetString("RIGHTS"), ifolder.Rights });
				detailTable.Rows.Add(new object[] { GetString("OWNER"), ifolder.OwnerFullName });
				detailTable.Rows.Add(new object[] { GetString("SIZE"), WebUtility.FormatSize(ifolder.Size, rm) });
				detailTable.Rows.Add(new object[] { GetString("MEMBERS"), ifolder.MemberCount.ToString() });
				detailTable.Rows.Add(new object[] { GetString("FILES"), ifolder.FileCount.ToString() });
				detailTable.Rows.Add(new object[] { GetString("FOLDERS"), ifolder.DirectoryCount.ToString() });
				detailTable.Rows.Add(new object[] { GetString("PUBLISHED"), WebUtility.FormatYesNo(ifolder.Published, rm) });
				detailTable.Rows.Add(new object[] { GetString("LOCKED"), WebUtility.FormatYesNo(!ifolder.Enabled, rm) });
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// data grid
			DetailData.DataSource = detailTable;
			DetailData.DataBind();
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
			this.DeleteButton.PreRender += new EventHandler(DeleteButton_PreRender);
			this.DeleteButton.Click += new EventHandler(DeleteButton_Click);
		}

		#endregion

		/// <summary>
		/// Delete Button Pre-Render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteButton_PreRender(object sender, EventArgs e)
		{
			DeleteButton.Attributes["onclick"] = "return ConfirmDelete(this.form);";
		}

		/// <summary>
		/// Delete Button Clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteButton_Click(object sender, EventArgs e)
		{
			try
			{
				web.DeleteiFolder(ifolderID);

				Response.Redirect("iFolders.aspx");
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
		}
	}
}
