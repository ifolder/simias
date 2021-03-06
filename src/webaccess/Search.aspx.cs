/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Rob
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

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
using System.Text.RegularExpressions;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Search Page
	/// </summary>
	public class SearchPage : Page
	{
		/// <summary>
		/// iFolder Context Control
		/// </summary>
		protected iFolderContextControl iFolderContext;

		/// <summary>
		/// Actions Container
		/// </summary>
		protected HtmlContainerControl Actions;

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
		/// Header page
		/// </summary>
		protected HeaderControl Head;
		
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
		/// Current iFolder ID
		/// </summary>
		private string ifolderID;

		/// <summary>
		/// Current Parent Entry ID
		/// </summary>
		private string entryID;

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

			// localization
			rm = (ResourceManager) Application["RM"];
			
			if (!IsPostBack)
			{
				if(! IfSearchEnabled())
				{
					// Don't load this page , if passphrase was not provided for that encrypted ifolder.
					Response.Redirect(String.Format("Browse.aspx?iFolder={0}&Entry={1}", ifolderID,
					entryID));
				}
				
				iFolder ifolder = web.GetiFolder(ifolderID);
				string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
				if(!(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty)))
				{
					// It is an encrypted ifolder , Make the Members tab invisible
					Tabs.MakeMembersLinkInvisible();
				}
				
				// data
				BindData();

				// strings
				EntryPagging.LabelSingular = GetString("ITEM");
				EntryPagging.LabelPlural = GetString("ITEMS");
				DeleteButton.Text = GetString("DELETE");
			}
			else
			{
				entryID = (string)ViewState["EntryID"];
			}
		}
	
		/// <summary>
		/// Determine to show the search tab or not for encrypted folders
		/// </summary>
		private bool IfSearchEnabled()
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
			BindParentData();
			BindEntryData();
			// Pass this page information to create the help link
			Head.AddHelpLink(GetString("SEARCH"));
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindParentData()
		{
			try
			{
				// ifolder
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
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// state
			ViewState["EntryID"] = entryID;
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindEntryData()
		{
			// query
			string pattern = Request.QueryString.Get("Pattern");

			// TODO: fix
			if ((pattern == null) || (pattern.Length == 0))
			{
				return;
			}

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
				string escPattern = Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") ;
				// entries
				iFolderEntrySet entries = web.GetEntriesByName(ifolderID, entryID, SearchOperation.Contains,
					escPattern, EntryPagging.Index, EntryPagging.PageSize); 
				
				// pagging
				EntryPagging.Total = entries.Total;
				EntryPagging.Count = entries.Items.Length;

				string name;
				string path;

				foreach(iFolderEntry child in entries.Items)
				{
					DataRow row = entryTable.NewRow();

					// selected name
					name = Regex.Replace(child.Name, String.Format("({0})", escPattern),
						"<span class='highlight'>${1}</span>", RegexOptions.IgnoreCase);
					
					// remove the iFolder name from the path
					path = child.Path.Substring(child.Path.IndexOf('/'));
					
					// remove the file name from the path
					path = path.Substring(0, path.LastIndexOf('/') + 1);
					
					row["ID"] = child.ID;
					row["iFolderID"] = child.iFolderID;
					row["Name"] = path + name;

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
