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
	/// New Folder Page
	/// </summary>
	public class NewFolderPage : Page
	{
		/// <summary>
		/// Parent Entry Path
		/// </summary>
		protected Literal ParentPath;

		/// <summary>
		/// New Folder Name
		/// </summary>
		protected TextBox NewFolderName;

		/// <summary>
		/// Create Button
		/// </summary>
		protected LinkButton CreateButton;

		/// <summary>
		/// Cancel Link
		/// </summary>
		protected HyperLink CancelLink;

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
				// data
				BindData();

				// strings
				CreateButton.Text = GetString("CREATE");
				CancelLink.Text = GetString("CANCEL");

				// link
				CancelLink.NavigateUrl = String.Format("Browse.aspx?iFolder={0}&Entry={1}", ifolderID, entryID);
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			try
			{
				// parent
				iFolderEntry entry;

				if ((entryID == null) || (entryID.Length == 0))
				{
					int total;

					entry = web.GetEntries(ifolderID, ifolderID, 0, 1, out total)[0];
					entryID = entry.ID;
				}
				else
				{
					entry = web.GetEntry(ifolderID, entryID);
				}
				
				ParentPath.Text = entry.Path;
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
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
			this.CreateButton.Click += new EventHandler(CreateButton_Click);
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
				case "iFolderFileDoesNotExistException":
				case "iFolderEntryAlreadyExistException":
					MessageBox.Text = GetString("ENTRY.DIRALREADYEXISTS");
					break;

				case "iFolderEntryInvalidCharactersException":
					MessageBox.Text = GetString("ENTRY.ENTRYINVALIDCHARACTERS");
					break;

				case "iFolderEntryInvalidNameException":
					MessageBox.Text = GetString("ENTRY.ENTRYINVALIDNAME");
					break;

				case "FileSizeException":
					MessageBox.Text = GetString("ENTRY.FILESIZEEXCEPTION");
					break;

				case "DiskQuotaException":
					MessageBox.Text = GetString("ENTRY.DISKQUOTAEXCEPTION");
					break;

				case "FileTypeException":
					MessageBox.Text = GetString("ENTRY.FILETYPEEXCEPTION");
					break;

				case "AccessException":
					MessageBox.Text = GetString("ENTRY.ACCESSEXCEPTION");
					break;

				case "LockException":
					MessageBox.Text = GetString("ENTRY.LOCKEXCEPTION");
					break;

				default:
					result = false;
					break;
			}

			return result;
		}

		/// <summary>
		/// Create Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CreateButton_Click(object sender, EventArgs e)
		{
			string name = NewFolderName.Text.Trim();
			
			// check for folder name
			if (name.Length == 0)
			{
				// no name
				MessageBox.Text = GetString("ENTRY.NOFOLDERNAME");
				return;
			}

			// create
			iFolderEntry entry = null;

			try
			{
				entry = web.CreateEntry(ifolderID, entryID, iFolderEntryType.Directory, name);

				// redirect
				Response.Redirect(String.Format("Browse.aspx?iFolder={0}&Entry={1}", entry.iFolderID, entry.ID));
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
		}
	}
}
