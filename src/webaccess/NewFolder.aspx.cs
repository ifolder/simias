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
		protected Button CreateButton;

		/// <summary>
		/// Cancel Button
		/// </summary>
		protected Button CancelButton;

		/// <summary>
		/// Message Box
		/// </summary>
		protected MessageControl Message;
		
		/// <summary>
		/// Header page
		/// </summary>
		protected HeaderControl Head;

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
				
				ParentPath.Text = FormatDirPath(entry.Path);

				// Pass this page information to create the help link
				Head.AddHelpLink(GetString("NEWFOLDER"));
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

				case "DirectoryNotFoundException":
                                        Message.Text = GetString("ENTRY.NOTMOUNTEDEXCEPTION");
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
				Message.Text = GetString("ENTRY.NOFOLDERNAME");
				return;
			}

			// create
			iFolderEntry entry = null;

			try
			{
				entry = web.CreateEntry(ifolderID, entryID, iFolderEntryType.Directory, name);

				// redirect
				Response.Redirect(String.Format("Browse.aspx?iFolder={0}&Entry={1}", entry.iFolderID, entry.ParentID));
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
			Response.Redirect(String.Format("Browse.aspx?iFolder={0}&Entry={1}", ifolderID, entryID));
		}

        private string FormatDirPath(string FormatedPath)
		{
            
	    	int Namelength = 70, start =0, end =0;
			string temp = null;
			bool flag = false;
			string shortName = null;
		    if(FormatedPath.Length > Namelength)
			{
			    while((start = FormatedPath.IndexOf('/', end)) != -1)
			    {
			        int len = start - end;

			        if (len > 0)
					{
					   flag = true;		
					   shortName = FormatedPath.Substring(end, len);
			           if (shortName.Length > Namelength)
			           {
		                 shortName =  web.GetShortenedName(shortName, Namelength);
                       }
                       temp+= shortName + "/";
                       }
			        end = start + 1;
	            }
				if (null != temp)
                {
                    //removing the last '/' direcotry seperator
                    temp = temp.Substring(0,temp.Length - 1);
                }
				if (true == flag)
			    {
			        //this is the last directory in the level
			        shortName = null;
			        shortName = FormatedPath.Substring(end);
			        if (shortName.Length > Namelength)
			        {
				        shortName =  web.GetShortenedName(shortName, Namelength);
				    }
			        FormatedPath = temp +"/" +shortName;
				}
				else
			    {
			        shortName = FormatedPath;
			        if (shortName.Length > Namelength)
			        {
			            shortName =  web.GetShortenedName(shortName, Namelength);
			            FormatedPath = shortName;
			        }
                 }
		  }
		 shortName = FormatedPath;	
         return shortName; 
	  } /*End of Function FormatDirPath*/

		
	}
}
