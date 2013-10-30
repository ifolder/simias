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
*                 $Author: Rob ()
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
		/// The DeleteDisabled Label
		/// </summary>
		protected Label DeleteDisabled;

		/// <summary>
		/// The separator stick
		/// </summary>
		protected Label FirstSingleStick;

		/// <summary>
		/// The separator stick
		/// </summary>
		protected Label SecondSingleStick;

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
		/// Current Parent Entry Path
		/// </summary>
		private string entryPath;
		
		/// <summary>
		/// The pass-phrase Label 
		/// </summary>
		protected Label PassPhraseLabel;

		/// <summary>
		/// pass-phrase text box
		/// </summary>
		protected TextBox PassPhraseText;
		
		/// <summary>
		/// The OK Button
		/// </summary>
		protected Button OKButton;

		/// <summary>
		/// The Cancel Button
		/// </summary>
		protected Button CancelButton;

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
			rm = (ResourceManager) Application["RM"];	
			// connection
			web = (iFolderWeb)Session["Connection"];
			iFolder ifolder = null;
			try
			{
				ifolder = web.GetiFolder(ifolderID);
			}
			catch (Exception)
			{
				Response.Redirect("iFolders.aspx?ErrorMsg=Member does not exist " + ifolderID);
			}
			if(ifolder == null)
				Response.Redirect("iFolders.aspx?ErrorMsg=iFolder does not exist " + ifolderID);	

			// localization
			
			if (!IsPostBack)
			{
				EntryData.Columns[ 3 ].HeaderText = GetString( "NAME" );
                                EntryData.Columns[ 4 ].HeaderText = GetString( "DATE" );
                                EntryData.Columns[ 5 ].HeaderText = GetString( "SIZE" );
                                EntryData.Columns[ 6 ].HeaderText = GetString( "HISTORY" );

				// Pass this page information to create the help link
				Head.AddHelpLink(GetString("BROWSE"));
				
				string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
				if(!(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty)))
				{
					// It is an encrypted ifolder , Make the Members tab invisible
					Tabs.MakeMembersLinkInvisible();
				}
				
				// this function will check whether an ifolder is encrypted or not, if yes, it will ask for passphrase
				// if passphrase matches , then the real page will be loaded.
				CheckForThePassPhrase(); 
			}
			else
			{
				entryID = (string)ViewState["EntryID"];
				entryPath = (string)ViewState["EntryPath"];
				CheckForThePassPhrase();
			}
		}
		/// <summary>
		/// Start the binding of data to the Page.
		/// </summary>
		private void StartBindingData()
		{
			// data
			BindData();
			iFolder ifolder = web.GetiFolder(ifolderID);
			// strings
			EntryPagging.LabelSingular = GetString("ITEM");
			EntryPagging.LabelPlural = GetString("ITEMS");
			NewFolderLink.Text = GetString("NEWFOLDER");
			UploadFilesLink.Text = GetString("UPLOADFILES");
			DeleteButton.Text = GetString("DELETE");
			DeleteDisabled.Text = GetString("DELETE");
			FirstSingleStick.Text = "|";
			SecondSingleStick.Text = "|";
			DeleteDisabled.Visible = true;
			EntryPagging.Visible = true;
			FirstSingleStick.Visible = true;
			SecondSingleStick.Visible = true;
			
			// links
			NewFolderLink.NavigateUrl = String.Format("NewFolder.aspx?iFolder={0}&Entry={1}", ifolderID, entryID);
			//UploadFilesLink.NavigateUrl = String.Format("Upload.aspx?iFolder={0}&Entry={1}", ifolderID, entryID);
			UploadFilesLink.NavigateUrl = String.Format("Upload.aspx?iFolder={0}&Entry={1}&Alg={2}", ifolderID, entryID, ifolder.EncryptionAlgorithm);
			
			PassPhraseLabel.Visible = PassPhraseText.Visible = OKButton.Visible = CancelButton.Visible = false;		
		}
		
		/// <summary>
		/// before loading the page, see if the ifolder was encrypted and show the page accordingly
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CheckForThePassPhrase()
		{
			string PassPhrase = Session["SessionPassPhrase"] as string;
			ifolderID = Request.QueryString.Get("iFolder");
			iFolder ifolder = web.GetiFolder(ifolderID);
			string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
			if(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty))
			{
			 	// It is not an encrypted ifolder , just return and display normal page
				
				PassPhraseLabel.Visible = PassPhraseText.Visible = OKButton.Visible = CancelButton.Visible = false;
				if( !IsPostBack )
					StartBindingData();
			}
			else
			{
				 if(PassPhrase != null)
				 {
					// User is in current session and has already given the passphrase, use this and display normal page 
					// Validate the passphrase...
					Status ObjValidate = web.ValidatePassPhrase(PassPhrase);
					if(ObjValidate.statusCode != StatusCodes.Success)
					{
						PassPhrase = null;	
						Session["SessionPassPhrase"] = null;						
					}					
				 }
				 
				 //Passphrase will become null if incorrect				 
				if(PassPhrase != null)
				{
					PassPhraseLabel.Visible = PassPhraseText.Visible = OKButton.Visible = CancelButton.Visible = false;
					if( !IsPostBack )
						StartBindingData();
				}
				else
				{
					PassPhraseLabel.Visible = PassPhraseText.Visible = OKButton.Visible = CancelButton.Visible = true;
					PassPhraseLabel.Text = GetString("ENTERPASSPHRASE");
					OKButton.Text = GetString("OK");
					CancelButton.Text = GetString("CANCEL");
					DeleteDisabled.Visible = false;
					EntryPagging.Visible = false;
					FirstSingleStick.Visible = false;
					SecondSingleStick.Visible = false;
				 }
			}
		}
		
		/// <summary>
		/// OK Button Click, It will validate the passphrase, and then it will load page.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OKButton_Click(object sender, EventArgs e)
		{
			//string PassPhrase = Session["SessionPassPhrase"] as string;
			ifolderID = Request.QueryString.Get("iFolder");
			iFolder ifolder = web.GetiFolder(ifolderID);
			string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
			if(EncryptionAlgorithm != "")
			{
				string PassPhraseStr = PassPhraseText.Text.Trim();
				if(PassPhraseStr == String.Empty)
				{
					Message.Text = GetString("PASSPHRASE_INCORRECT");
					PassPhraseText.Text = "";
					return;
				}
				try
				{
					Status ObjValidate = web.ValidatePassPhrase(PassPhraseStr);
					if(ObjValidate.statusCode != StatusCodes.Success)
					{
						Message.Text = GetString("PASSPHRASE_INCORRECT");
						PassPhraseText.Text = "";
						return;
					}
					Session["SessionPassPhrase"]= PassPhraseStr;
				}
				catch(SoapException ex)
				{
					if (!HandleException(ex))				
						Message.Text = ex.Message;
					return;				
				}
				catch(Exception ex)
				{
					Message.Text = ex.Message;
					return;
				}
				StartBindingData();
			}
		}
		
	
		/// <summary>
		/// Cancel Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CancelButton_Click(object sender, EventArgs e)
		{
			try
			{
				Response.Redirect(String.Format("iFolders.aspx"));
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
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
				if(ifolderLocation == null)
				{
				//if we are not able to get the ifolder location for a particular ifolder we show the iFolder page instead of an error
				// we fail to get the location, due to catalog update delay
					Response.Redirect("iFolders.aspx");
				}
				
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
					int Namelenght = 70;
					string shortName = null;
					shortName = child.Name;
			    	if (child.Name.Length > Namelenght)
				    {
				       shortName  =  web.GetShortenedName(child.Name, Namelenght);
				    }

					DataRow row = entryTable.NewRow();

					row["ID"] = child.ID;
					row["iFolderID"] = child.iFolderID;
					row["Name"] = shortName;

					if (child.IsDirectory)
					{
						row["Link"] = String.Format("Browse.aspx?iFolder={0}&Entry={1}",
						ifolderID, child.ID);
						row["Image"] = "folder.png";
						row["Size"] = WebUtility.FormatSize(child.Size, rm);
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
		    int Namelenght = 70;
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
					string shortName = entryPath.Substring(end, len);
				    if (shortName.Length > Namelenght)
				    {
					    shortName =  web.GetShortenedName(shortName, Namelenght);
				    }
					DataRow row = pathTable.NewRow();
					row["Name"] = shortName;
					row["Path"] = entryPath.Substring(0, start);;
					pathTable.Rows.Add(row);
				}

				end = start + 1;
			}

			// bind
			EntryPathList.DataSource = pathTable;
			EntryPathList.DataBind();

			// leaf
			EntryPathLeaf.Text = entryPath.Substring(end);
		    if (EntryPathLeaf.Text.Length > Namelenght)
		    {
		         EntryPathLeaf.Text =  web.GetShortenedName(EntryPathLeaf.Text, Namelenght);
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
			this.EntryPathList.ItemCommand += new RepeaterCommandEventHandler(EntryPathList_ItemCommand);
			this.ID = "EntryView";
			this.Load += new System.EventHandler(this.Page_Load);
			this.EntryPagging.PageChange += new EventHandler(EntryPagging_PageChange);
			this.DeleteButton.PreRender += new EventHandler(DeleteButton_PreRender);
			this.DeleteButton.Click += new EventHandler(DeleteButton_Click);
			this.OKButton.Click += new EventHandler(OKButton_Click);
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
					if (!HandleException(ex)) 
					{
						Message.Text = ex.Message;
						return;
					}

				}

				EntryPagging.Index = 0;
				BindEntryData();
			}
		}
	}
}
