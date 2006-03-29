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
		/// Search Pattern
		/// </summary>
		protected TextBox SearchPattern;

		/// <summary>
		/// Search Button
		/// </summary>
		protected Button SearchButton;

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
				iFolderPagging.LabelSingular = rm.GetString("IFOLDER");
				iFolderPagging.LabelPlural = rm.GetString("IFOLDERS");
				NewiFolderLink.Text = rm.GetString("NEW");
				SearchButton.Text = rm.GetString("SEARCH");

				// search pattern
				ViewState["SearchPattern"] = null;

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

			try
			{
				// data
				iFolder[] ifolders = web.GetiFoldersByName(SearchOperation.BeginsWith, SearchPattern.Text, iFolderPagging.Index, iFolderPagging.PageSize, out total);
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

					ifolderTable.Rows.Add(row);
				}
			}
			catch(SoapException ex)
			{
				HandleException(ex);
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
			return rm.GetString(key);
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
		}

		#endregion

		/// <summary>
		/// Handle Exceptions
		/// </summary>
		/// <param name="e"></param>
		private void HandleException(SoapException e)
		{
			// exception type
			string type = WebUtility.GetSmartExceptionType(e);

			// types
			switch(type)
			{
				default:
					MessageBox.Text = type;
					break;
			}
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
	}
}
