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
using System.IO;
using System.Net;
using System.Web.Services.Protocols;
using System.Xml;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Browse Page
	/// </summary>
	public class BrowsePage : Page
	{
		/// <summary>
		/// Actions Container
		/// </summary>
		protected HtmlContainerControl Actions;

		/// <summary>
		/// New Folder Link
		/// </summary>
		protected HyperLink NewFolderLink;

		/// <summary>
		/// Upload Link
		/// </summary>
		protected HyperLink UploadFilesLink;

		/// <summary>
		/// Entry Path List
		/// </summary>
		protected Repeater EntryPathList;

		/// <summary>
		/// Entry Path Leaf
		/// </summary>
		protected Literal EntryPathLeaf;

		/// <summary>
		/// The Delete Button
		/// </summary>
		protected LinkButton DeleteButton;

		/// <summary>
		/// The Entry Data
		/// </summary>
		protected DataGrid EntryData;

		/// <summary>
		/// Pagging
		/// </summary>
		protected PaggingControl EntryPagging;

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
		/// Current iFolder ID
		/// </summary>
		private string ifolderID;

		/// <summary>
		/// Current Parent Entry ID
		/// </summary>
		private string entryID;

		/// <summary>
		/// Current Parent Entry Path
		/// </summary>
		private string entryPath;

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
			iFolder ifolder = web.GetiFolder(ifolderID);

			// localization
			rm = (ResourceManager) Application["RM"];
			
			if (!IsPostBack)
			{
				// data
				BindData();

				// strings
				EntryPagging.LabelSingular = GetString("ITEM");
				EntryPagging.LabelPlural = GetString("ITEMS");
				NewFolderLink.Text = GetString("NEWFOLDER");
				UploadFilesLink.Text = GetString("UPLOADFILES");
				DeleteButton.Text = GetString("DELETE");

				// links
				NewFolderLink.NavigateUrl = String.Format("NewFolder.aspx?iFolder={0}&Entry={1}", ifolderID, entryID);
				//UploadFilesLink.NavigateUrl = String.Format("Upload.aspx?iFolder={0}&Entry={1}", ifolderID, entryID);
				UploadFilesLink.NavigateUrl = String.Format("Upload.aspx?iFolder={0}&Entry={1}&Alg={1}", ifolderID, entryID, ifolder.EncryptionAlgorithm);
			}
			else
			{
				entryID = (string)ViewState["EntryID"];
				entryPath = (string)ViewState["EntryPath"];
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			BindParentData();
			BindEntryData();
			BindPathData();
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindParentData()
		{
			try
			{
				// ifolder
                                string ifolderLocation = web.GetiFolderLocation (ifolderID);

				UriBuilder remoteurl = new UriBuilder(ifolderLocation);
				remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
				web.Url = remoteurl.Uri.ToString();

				iFolder ifolder = web.GetiFolder(ifolderID);

				// rights
				Actions.Visible = (ifolder.MemberRights != Rights.ReadOnly);
				EntryData.Columns[1].Visible = Actions.Visible;

				// parent
				iFolderEntry entry;

				if ((entryID == null) || (entryID.Length == 0))
				{
					entry = web.GetEntries(ifolderID, ifolderID, 0, 1).Items[0];
					entryID = entry.ID;
				}
				else
				{
					entry = web.GetEntry(ifolderID, entryID);
				}
				
				entryPath = entry.Path;
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// state
			ViewState["EntryID"] = entryID;
			ViewState["EntryPath"] = entryPath;
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindEntryData()
		{
			// entries
			DataTable entryTable = new DataTable();
			entryTable.Columns.Add("ID");
			entryTable.Columns.Add("iFolderID");
			entryTable.Columns.Add("Link");
			entryTable.Columns.Add("Image");
			entryTable.Columns.Add("Name");
			entryTable.Columns.Add("Size");
			entryTable.Columns.Add("LastModified");
			entryTable.Columns.Add("IsDirectory", typeof(bool));

			try
			{

                                //Location of ifolder.
                                string ifolderLocation = web.GetiFolderLocation (ifolderID);

				UriBuilder remoteurl = new UriBuilder(ifolderLocation);
				remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
				web.Url = remoteurl.Uri.ToString();

				// entries
				iFolderEntrySet entries = web.GetEntries(ifolderID, entryID, EntryPagging.Index, EntryPagging.PageSize);
				
				// pagging
				EntryPagging.Total = entries.Total;
				EntryPagging.Count = entries.Items.Length;

				foreach(iFolderEntry child in entries.Items)
				{
					DataRow row = entryTable.NewRow();

					row["ID"] = child.ID;
					row["iFolderID"] = child.iFolderID;
					row["Name"] = child.Name;

					if (child.IsDirectory)
					{
						row["Link"] = String.Format("Browse.aspx?iFolder={0}&Entry={1}",
							ifolderID, child.ID);
						row["Image"] = "folder.png";
						row["Size"] = "";
					}
					else
					{
						row["Link"] = String.Format("Download.ashx?iFolder={0}&Entry={1}",
							ifolderID, child.ID);
						row["Image"] = "text-x-generic.png";
						row["Size"] = WebUtility.FormatSize(child.Size, rm);
					}

					row["LastModified"] = WebUtility.FormatDate(child.LastModified, rm);
					row["IsDirectory"] = child.IsDirectory;
					
					entryTable.Rows.Add(row);
				}
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// bind
			EntryData.DataKeyField = "ID";
			EntryData.DataSource = entryTable;
			EntryData.DataBind();
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindPathData()
		{
			// context
			DataTable pathTable = new DataTable();
			pathTable.Columns.Add("Name");
			pathTable.Columns.Add("Path");

			// parse the path
			int start = 0;
			int end = 0;

			while((start = entryPath.IndexOf('/', end)) != -1)
			{
				int len = start - end;

				if (len > 0)
				{
					DataRow row = pathTable.NewRow();
					row["Name"] = entryPath.Substring(end, len);
					row["Path"] = entryPath.Substring(0, start);
					pathTable.Rows.Add(row);
				}

				end = start + 1;
			}

			// bind
			EntryPathList.DataSource = pathTable;
			EntryPathList.DataBind();

			// leaf
			EntryPathLeaf.Text = entryPath.Substring(end);
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
		/// On Initialization
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Initialize the Components
		/// </summary>
		private void InitializeComponent()
		{    
			this.EntryPathList.ItemCommand += new RepeaterCommandEventHandler(EntryPathList_ItemCommand);
			this.ID = "EntryView";
			this.Load += new System.EventHandler(this.Page_Load);
			this.EntryPagging.PageChange += new EventHandler(EntryPagging_PageChange);
			this.DeleteButton.PreRender += new EventHandler(DeleteButton_PreRender);
			this.DeleteButton.Click += new EventHandler(DeleteButton_Click);
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
				case "FileDoesNotExistException":
				case "EntryAlreadyExistException":
					Message.Text = GetString("ENTRY.DIRALREADYEXISTS");
					break;

				case "EntryInvalidCharactersException":
					Message.Text = GetString("ENTRY.ENTRYINVALIDCHARACTERS");
					break;

				case "EntryInvalidNameException":
					Message.Text = GetString("ENTRY.ENTRYINVALIDNAME");
					break;

				case "FileSizeException":
					Message.Text = GetString("ENTRY.FILESIZEEXCEPTION");
					break;

				case "DiskQuotaException":
					Message.Text = GetString("ENTRY.DISKQUOTAEXCEPTION");
					break;

				case "FileTypeException":
					Message.Text = GetString("ENTRY.FILETYPEEXCEPTION");
					break;

				case "AccessException":
					Message.Text = GetString("ENTRY.ACCESSEXCEPTION");
					break;

				case "LockException":
					Message.Text = GetString("ENTRY.LOCKEXCEPTION");
					break;

				default:
					result = false;
					break;
			}

			return result;
		}

		/// <summary>
		/// Entry Path List Item Command Handler
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void EntryPathList_ItemCommand(object source, RepeaterCommandEventArgs e)
		{
			string path = (e.CommandSource as LinkButton).CommandName;

			try
			{
				iFolderEntry entry = web.GetEntryByPath(ifolderID, path);
				
				Response.Redirect(String.Format("Browse.aspx?iFolder={0}&Entry={1}", ifolderID, entry.ID));
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
		}

		/// <summary>
		/// On Page Change Event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void EntryPagging_PageChange(object sender, EventArgs e)
		{
			BindData();
		}

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
		/// Delete Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteButton_Click(object sender, EventArgs e)
		{
			string entryList = null;

			// selected members
			foreach(DataGridItem item in EntryData.Items)
			{
				CheckBox checkBox = (CheckBox) item.FindControl("Select");

				if (checkBox.Checked)
				{
					string id = item.Cells[0].Text;

					if (entryList == null)
					{
						entryList = id;
					}
					else
					{
						entryList = String.Format("{0},{1}", entryList, id);
					}
				}
			}

			// delete entries
			if (entryList != null)
			{
				try
				{
					web.DeleteEntry(ifolderID, entryList);
				}
				catch(SoapException ex)
				{
					if (!HandleException(ex)) throw;
				}

				EntryPagging.Index = 0;
				BindEntryData();
			}
		}
	}
}
