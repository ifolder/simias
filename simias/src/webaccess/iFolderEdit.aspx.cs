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
	/// iFolder Edit Page
	/// </summary>
	public class iFolderEditPage : Page
	{
		/// <summary>
		/// iFolder Name
		/// </summary>
		protected Literal iFolderName;

		/// <summary>
		/// iFolder Description
		/// </summary>
		protected TextBox iFolderDescription;

		/// <summary>
		/// Save Button
		/// </summary>
		protected Button SaveButton;

		/// <summary>
		/// Cancel Button
		/// </summary>
		protected Button CancelButton;

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

				// strings
				SaveButton.Text = GetString("SAVE");
				CancelButton.Text = GetString("CANCEL");
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
				
				// context
				iFolderName.Text = ifolder.Name;

				// description
				iFolderDescription.Text = ifolder.Description;
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
			this.SaveButton.Click += new EventHandler(SaveButton_Click);
			this.CancelButton.Click += new EventHandler(CancelButton_Click);
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
		/// Save Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SaveButton_Click(object sender, EventArgs e)
		{
			string description = iFolderDescription.Text.Trim();
			
			try
			{
				web.SetiFolderDescription(ifolderID, description);

				// redirect
				Response.Redirect("Details.aspx?iFolder=" + ifolderID);
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
		}

		/// <summary>
		/// Cancel Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CancelButton_Click(object sender, EventArgs e)
		{
			// redirect
			Response.Redirect("Details.aspx?iFolder=" + ifolderID);
		}
	}
}
