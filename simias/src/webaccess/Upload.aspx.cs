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
	/// Upload Page
	/// </summary>
	public class UploadPage : Page
	{
		/// <summary>
		/// File Transfer Buffer Size
		/// </summary>
		private const int BUFFERSIZE = (16 * 1024);

		/// <summary>
		/// Parent Entry Path
		/// </summary>
		protected Literal ParentPath;

		/// <summary>
		/// Upload File
		/// </summary>
		protected HtmlInputFile UploadFile;

		/// <summary>
		/// Upload Button
		/// </summary>
		protected LinkButton UploadButton;

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
				UploadButton.Text = GetString("UPLOAD");
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
			this.UploadButton.Click += new EventHandler(UploadButton_Click);
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
		/// Upload Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UploadButton_Click(object sender, EventArgs e)
		{
			// filename
			// KLUDGE: Mono no longer recognizes backslash as a directory seperator
			// Path.GetFileName() is not usable here for that reason
			string filename = WebUtility.GetFileName(UploadFile.PostedFile.FileName.Trim());
			
			// check for file
			if (filename.Length == 0)
			{
				// no file
				MessageBox.Text = GetString("ENTRY.NOUPLOADFILE");

				return;
			}

			// upload path
			string path = String.Format("{0}/{1}", ParentPath.Text, filename);

			iFolderEntry child = null;
			bool newEntry = false;

			try
			{
				try
				{
					// does the entry exist?
					child = web.GetEntryByPath(ifolderID, path);
				}
				catch
				{
					// ignore
				}

				// create the entry
				if (child == null)
				{
					child = web.CreateEntry(ifolderID, entryID, iFolderEntryType.File, filename);
					newEntry = true;
				}

				// check for an empty file
				if (UploadFile.PostedFile.ContentLength == 0)
				{
					MessageBox.Text = GetString("ENTRY.EMPTYFILE");

					return;
				}

				try
				{
					// put
					UriBuilder uri = new UriBuilder(web.Url);
					
					uri.Path = String.Format("/simias10/Upload.ashx?iFolder={0}&Entry={1}",
						child.iFolderID, child.ID);

					HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(uri.Uri);
					webRequest.Method = "PUT";
					webRequest.ContentLength = UploadFile.PostedFile.ContentLength;
					webRequest.PreAuthenticate = true;
					webRequest.Credentials = web.Credentials;
					webRequest.CookieContainer = web.CookieContainer;
					webRequest.AllowWriteStreamBuffering = false;

					Stream webStream = webRequest.GetRequestStream();

					Stream stream = UploadFile.PostedFile.InputStream;
					
					try
					{
						byte[] buffer = new byte[BUFFERSIZE];

						int count;
						
						while((count = stream.Read(buffer, 0, buffer.Length)) > 0)
						{
							webStream.Write(buffer, 0, count);
							webStream.Flush();
						}
					}
					finally
					{
						webStream.Close();
						stream.Close();
					}

					// response
					webRequest.GetResponse().Close();
				}
				catch
				{
					// remove the child if it was freshly created
					if (newEntry)
					{
						try
						{
							web.DeleteEntry(child.iFolderID, child.ID);
						}
						catch
						{
							// ignore
						}
					}

					throw;
				}

				// redirect
				Response.Redirect(String.Format("Browse.aspx?iFolder={0}&Entry={1}", ifolderID, entryID));
			}
			catch(Exception ex)
			{
				if (!HandleException(ex)) throw;
			}
		}
	}
}
