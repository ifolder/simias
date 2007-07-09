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
using System.Text;
using Simias.Encryption;
using Simias.Storage;

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
		/// The pass-phrase Label 
		/// </summary>
		protected Label PassPhraseLabel;
		
		/// <summary>
		/// pass-phrase text box
		/// </summary>
		protected TextBox PassPhraseText;

		/// <summary>
		/// Upload Button
		/// </summary>
		protected Button UploadButton;

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
		/// EncryptionAlgorithm
		/// </summary>
		private  string EncryptionAlgorithm;

		/// <summary>
		/// EncryptionKey
		/// </summary>
		private  string EncryptionKey;

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

			iFolder ifolder = web.GetiFolder(ifolderID);
			EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
			EncryptionKey = ifolder.EncryptionKey;
			
			if (!IsPostBack)
			{
				string PassPhrase = Session["SessionPassPhrase"] as string;
				if(PassPhrase == null)
				{
					bool PPSet = web.IsPassPhraseSet();
				
					if(PPSet && EncryptionAlgorithm != "")
					{
						PassPhraseLabel.Visible = true;
						PassPhraseText.Visible = true;
						PassPhraseLabel.Text = GetString("ENTERPASSPHRASE");
					}
				}	
				// data
				BindData();
				
				// strings
				UploadButton.Text = GetString("UPLOAD");
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
				
				ParentPath.Text = entry.Path;
				
				// Pass this page information to create the help link
				Head.AddHelpLink(GetString("UPLOAD"));
				
				//Enable SSL in web access can be configured by the admin
				// SSL property is used only for thick client to server communication and vice versa
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
		/// Upload Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UploadButton_Click(object sender, EventArgs e)
		{
			try
			{
				bool PPSet = web.IsPassPhraseSet();
				string PassPhraseStr = null;
				if(PPSet && PassPhraseLabel.Visible)
				{
					PassPhraseStr = PassPhraseText.Text.Trim();
					
					if(PassPhraseStr.Length == 0)
					{
						Message.Text = GetString("IFOLDER.NOPASSPHRASE");
						return;
					}
					// verify the entered pass-phrase
					Status ObjValidate = web.ValidatePassPhrase(PassPhraseStr);
					if(ObjValidate.statusCode != StatusCodes.Success)
					{
						Message.Text = GetString("Wrongpassphrase");
						PassPhraseText.Text = "";
						return;
					}
				}
				foreach(string name in Request.Files)
				{
					UploadFile(Request.Files[name], PassPhraseStr);
				}

				Response.Redirect(String.Format("Browse.aspx?iFolder={0}&Entry={1}&Alg={2}", ifolderID, entryID, EncryptionAlgorithm));
			}
			catch(Exception ex)
			{
				if (!HandleException(ex)) throw;
			}
		}

		private void UploadFile(HttpPostedFile file, string PassPhraseStr)
		{
			//Blowfish Algorithm assumed here
			Blowfish	bf=null;
			int		boundary=0;
			int 		count=0;
			bool 	EncryptionEnabled = true;
			
			string PassPhrase = Session["SessionPassPhrase"] as string;
			
			bool PPSet = web.IsPassPhraseSet();
			
			if(EncryptionAlgorithm == "")
					EncryptionEnabled = false;	
			else
			{
				if(PassPhrase == null )
					PassPhrase = PassPhraseStr;
			}
			
			if(EncryptionEnabled )
			{
//				UTF8Encoding utf8 = new UTF8Encoding();
				string DecryptedCryptoKey;

				//Hash the passphrase and use it for encryption and decryption
				PassphraseHash hash = new PassphraseHash();
				byte[] passphrase = hash.HashPassPhrase(PassPhrase);	
				
				Key key = new Key(EncryptionKey);
				key.DecrypytKey(passphrase, out DecryptedCryptoKey);
				//Decrypt the key using passphrase and use it
				bf = new Blowfish(Convert.FromBase64String(DecryptedCryptoKey));
				boundary = 8;
			}

			// filename
			// KLUDGE: Mono no longer recognizes backslash as a directory seperator
			// Path.GetFileName() is not usable here for that reason
			string filename = WebUtility.GetFileName(file.FileName.Trim());
			
			// check for file
			if (filename.Length == 0) return;

			// upload path
			string path = String.Format("{0}/{1}", ParentPath.Text, filename);

			// check for an empty file
			if (file.ContentLength == 0)
			{
				Message.Text = GetString("ENTRY.EMPTYFILE");

				return;
			}

			// put
			UriBuilder uri = new UriBuilder(web.Url);
			
			uri.Path = String.Format("/simias10/Upload.ashx?iFolder={0}&Path={1}&Length={2}",
				ifolderID, path, file.ContentLength.ToString());

			HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(uri.Uri);
			webRequest.Method = "PUT";
			count = file.ContentLength;
			if(EncryptionEnabled && (count %boundary !=0))
				count += boundary - (count %boundary);
			webRequest.ContentLength = count;
			
			webRequest.PreAuthenticate = true;
			webRequest.Credentials = web.Credentials;
			webRequest.CookieContainer = web.CookieContainer;
			webRequest.AllowWriteStreamBuffering = false;

			Stream webStream = webRequest.GetRequestStream();

			Stream stream = file.InputStream;
			
			try
			{
				byte[] buffer = new byte[BUFFERSIZE];
					
				while((count = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					if(EncryptionEnabled)
					{
						if(count %boundary !=0)
							count += boundary - (count %boundary);
						bf.Encipher(buffer, count);
					}					
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
					
			//Set the file length here
			try
			{
				//web.SetFileLength(ifolderID, entryID, file.ContentLength);
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
	}
}
