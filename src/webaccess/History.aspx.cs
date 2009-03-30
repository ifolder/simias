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
using System.Web.Services.Protocols;
using System.Net;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// History Page
	/// </summary>
	public class HistoryPage : Page
	{
		/// <summary>
		/// History Data
		/// </summary>
		protected DataGrid HistoryData;

		/// <summary>
		/// History Pagging
		/// </summary>
		protected PaggingControl HistoryPagging;

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
		/// iFolder ID
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
			web = (iFolderWeb) Session["Connection"];

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
			
				iFolder ifolder = web.GetiFolder(ifolderID);
				HistoryData.Columns[ 2 ].HeaderText = GetString( "NAME" );
                                HistoryData.Columns[ 3 ].HeaderText = GetString( "ACTION" );
				HistoryData.Columns[ 4 ].HeaderText = GetString( "DATE/TIME" );
				
				//Load this page only if passphrase was provided for the encrypted iFolder
				if(! IfHistoryEnabled())
				{
					// Return to Browse page, if passphrase was not provided for that encrypted ifolder.
					
					Response.Redirect(String.Format("Browse.aspx?iFolder={0}", ifolderID));
				}
				
				string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
				if(!(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty)))
				{
					// It is an encrypted ifolder , Make the Members tab invisible
					Tabs.MakeMembersLinkInvisible();
				}
			
				// data
				BindData();

				// strings
				HistoryPagging.LabelSingular = GetString("CHANGE");
				HistoryPagging.LabelPlural = GetString("CHANGES");
			}
		}
		
		/// <summary>
		/// Determine to show the history tab or not for encrypted folders
		/// </summary>
		private bool IfHistoryEnabled()
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
			BindHistoryData();
			// Pass this page information to create the help link
			Head.AddHelpLink(GetString("HISTORY"));
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindHistoryData()
		{
			// history
			DataTable historyTable = new DataTable();
			historyTable.Columns.Add("ID");
			historyTable.Columns.Add("iFolderID");
			historyTable.Columns.Add("Time");
			historyTable.Columns.Add("Name");
			historyTable.Columns.Add("ShortName");
			historyTable.Columns.Add("ShortenedName");
			historyTable.Columns.Add("Type");
			historyTable.Columns.Add("Action");
			historyTable.Columns.Add("UserFullName");
			historyTable.Columns.Add("TypeImage");
			historyTable.Columns.Add("ActionImage");
			historyTable.Columns.Add("NewRights");

			try
			{
				// ifolder
				iFolder ifolder = web.GetiFolder(ifolderID);

				// history
				ChangeEntrySet changes = web.GetChanges(ifolderID, null, HistoryPagging.Index, HistoryPagging.PageSize);
				HistoryPagging.Count = changes.Items.Length;
				HistoryPagging.Total = changes.Total;
				
				string tempname = null;

				foreach(ChangeEntry change in changes.Items)
				{
					DataRow row = historyTable.NewRow();
					row["ID"] = change.ID;
					row["iFolderID"] = ifolderID;
					row["Time"] = WebUtility.FormatDateTime(change.Time, rm);
					row["Name"] = change.Name;
					string NameWithoutDir = WebUtility.GetFileName(change.Name);
					int ShortenedLength = 70;
					if(NameWithoutDir.Length > ShortenedLength)
					{
						tempname = web.GetShortenedName(NameWithoutDir, ShortenedLength);
					}
					row["ShortName"] = NameWithoutDir;
					row["ShortenedName"] = (NameWithoutDir.Length > ShortenedLength)  ? tempname.ToString() : NameWithoutDir;
					row["Type"] = change.Type.ToString();
					row["Action"] = WebUtility.FormatChangeAction(change.Action, rm);
					row["UserFullName"] = change.UserFullName;
					row["TypeImage"] = change.Type.ToString().ToLower();
					row["ActionImage"] = change.Action.ToString().ToLower();
					
					if ((change.Type == ChangeEntryType.Member) && (change.Action != ChangeEntryAction.Delete))
					{
						row["NewRights"] = String.Format("({0})", WebUtility.FormatRights(change.MemberNewRights, rm));
					}
					else
					{
						row["NewRights"] = "";
					}

					historyTable.Rows.Add(row);
				}
			}
			catch(NullReferenceException ex)
                        {
                                Message.Text = GetString("HISTORYNOTAVAILABLE");
                        }
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// bind
			HistoryData.DataSource = historyTable;
			HistoryData.DataBind();
		}

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
				case "AccessException":
					Message.Text = GetString("ENTRY.ACCESSEXCEPTION");
					break;

				default:
					result = false;
					break;
			}

			return result;
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
			this.Load += new System.EventHandler(this.Page_Load);
			this.HistoryPagging.PageChange += new EventHandler(HistoryPagging_PageChange);
		}

		#endregion

		/// <summary>
		/// History Page Change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HistoryPagging_PageChange(object sender, EventArgs e)
		{
			BindHistoryData();
		}
	}
}
