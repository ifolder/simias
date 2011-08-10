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
		/// The OverWrite Existing File Label 
		/// </summary>
		protected Label OverWriteExistingFile;
		
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
		///  OverWrite Existing Files Checkbox
		/// </summary>
		protected CheckBox OverWriteCheckbox;

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
				
					if(PPSet && (EncryptionAlgorithm != null && EncryptionAlgorithm != String.Empty) )
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
				OverWriteExistingFile.Text = GetString("TEXTOVERWRITE"); 
       			         OverWriteCheckbox.Checked = true; 
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

				case "DirectoryNotFoundException":
					Message.Text = GetString("ENTRY.NOTMOUNTEDEXCEPTION");
					break;
	
				case "IOException":
					Message.Text = GetString("ENTRY.IOERROR");
					break;

				case "Timeout":
					Message.Text = GetString("ENTRY.TIMEOUT");
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
				        Message.Text = GetString("PASSPHRASE_INCORRECT");
						PassPhraseText.Text = "";
						return;
					}
				}
				else if( PPSet && (EncryptionAlgorithm != null && EncryptionAlgorithm != String.Empty))
				{
					string PassPhrase = Session["SessionPassPhrase"] as string;
					// verify the entered pass-phrase
					Status ObjValidate = web.ValidatePassPhrase(PassPhrase);
					if(ObjValidate.statusCode != StatusCodes.Success)
					{
					        Message.Text = GetString("PASSPHRASE_INCORRECT");
						PassPhraseText.Text = "";
						Session["SessionPassPhrase"] = null;
						Response.Redirect(String.Format("Browse.aspx?iFolder={0}&Message={1}", ifolderID, Message.Text));
						return;
					}				
				}
                // Call WebServices API to get the matched folder list
				string UploadFileName =null;
				bool MatchFound = false;
				string ExistingFiles = null;
			    ArrayList Uploadedfiles = new ArrayList(); 
				HttpPostedFile filename=null;
			if  (!OverWriteCheckbox.Checked) 
            {
		        foreach(string name in Request.Files)
				{
				    filename = Request.Files[name];		
				    Uploadedfiles.Add( WebUtility.GetFileName((filename).FileName.Trim()) );
				}
				string[] Array = new string[Uploadedfiles.Count];
				Uploadedfiles.CopyTo(Array);

			    iFolderEntrySet matchentries = web.GetMatchedEntries(ifolderID, entryID, Array); 
				
				if (matchentries.Total > 0) 
                 {
					//List of files to be Uploaded
				     foreach(string name in Request.Files)
				     {
					     filename = Request.Files[name];
						 UploadFileName = WebUtility.GetFileName( (filename).FileName.Trim() );
						 if (UploadFileName.Length == 0) continue;
						
						 //List of Matched Entries
						 foreach (iFolderEntry child in matchentries.Items)	 
						 {
						     if( (UploadFileName.ToLower()).Equals(child.Name.ToLower()) )
							 {
                                 				MatchFound = true;
							     break;	
							 }	
						 }
						 if (false == MatchFound)
						 {
					         UploadFile(Request.Files[name], PassPhraseStr);
						 }
						 else
						 {
							//reseting the flag for next Iteration	 
						     MatchFound = false;
						 }
				     }   

		        } /* End of IF (matchentries.Total > 0) */
				else
				{
					//if OverWriteCheckbox is checked, but files to be upload doesn't match with the existing file	
                    foreach(string name in Request.Files)
					{
					    UploadFile(Request.Files[name], PassPhraseStr);
				    }
				}
			} /* End of IF  (!OverWriteCheckbox.Checked)  */ 
			else
			{
				//if OverWriteCheckbox is unchecked.
		 	    foreach(string name in Request.Files)
				{	
                    UploadFile(Request.Files[name], PassPhraseStr); 
				}

			}

				Response.Redirect(String.Format("Browse.aspx?iFolder={0}&Entry={1}&Alg={2}", ifolderID, entryID, EncryptionAlgorithm));
			}
			catch(Exception ex)
			{
				if (!HandleException(ex)) throw;
			}
		}

        /// <summary>
        /// Function to upload the file, also checks if it is encrypted
        /// </summary>
        /// <param name="file"></param>
        /// <param name="PassPhraseStr"></param>
		private void UploadFile(HttpPostedFile file, string PassPhraseStr)
		{
			//Blowfish Algorithm assumed here
			Blowfish	bf=null;
			int		boundary=0;
			int 		count=0;
			bool 	EncryptionEnabled = true;
			
			string PassPhrase = Session["SessionPassPhrase"] as string;
			
			bool PPSet = web.IsPassPhraseSet();
			
			if(EncryptionAlgorithm == "" || EncryptionAlgorithm == null)
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

			string ConvertedPath = ParentPath.Text;
			string ConvertedFilename = filename;	
		
			// even index should contain the string which has to be replaced , and next index should contain the new string
			// maintain the same info in file src/webservices/FileHandler.cs 
			string [] ConversionTable = {"&", "amp@:quot"};

			for(int index=0 ; index < ConversionTable.Length ; index+=2)
			{
				ConvertedPath = ConvertedPath.Replace(ConversionTable[index], ConversionTable[index+1]);
				ConvertedFilename = ConvertedFilename.Replace(ConversionTable[index], ConversionTable[index+1]);
			}

			// upload path
			string path = String.Format("{0}/{1}", ConvertedPath, ConvertedFilename);

			// put
			UriBuilder uri = new UriBuilder(web.Url);
			
			uri.Path = String.Format("/simias10/Upload.ashx?iFolder={0}&Path={1}&DontCheckPolicies=false&Length={2}", ifolderID, path, file.ContentLength.ToString());

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
