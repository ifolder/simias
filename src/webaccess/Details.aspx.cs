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
	/// Details Page
	/// </summary>
	public class DetailsPage : Page
	{
		/// <summary>
		/// Property Actions Container
		/// </summary>
		protected HtmlContainerControl PropertyActions;

		/// <summary>
		/// Property Data
		/// </summary>
		protected DataGrid PropertyData;

		/// <summary>
		/// Property Edit Link
		/// </summary>
		protected HyperLink PropertyEditLink;

		/// <summary>
		/// Policy Actions Container
		/// </summary>
		protected HtmlContainerControl PolicyActions;

		/// <summary>
		/// Policy Data
		/// </summary>
		protected DataGrid PolicyData;

		/// <summary>
		/// Policy Edit Link
		/// </summary>
		protected HyperLink PolicyEditLink;

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
				
				//Load this page only if passphrase was provided for the encrypted iFolder
				if(! IfDetailsEnabled())
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
				PropertyEditLink.Text = GetString("EDIT");
				PolicyEditLink.Text = GetString("EDIT");

				// links
				PropertyEditLink.NavigateUrl = "iFolderEdit.aspx?iFolder=" + ifolderID;
				PolicyEditLink.NavigateUrl = "iFolderPolicyEdit.aspx?iFolder=" + ifolderID;
			}
		}

		/// <summary>
		/// Determine to show the details tab or not for encrypted folders
		/// </summary>
		private bool IfDetailsEnabled()
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
			BindPropertyData();
			BindPolicyData();
			// Pass this page information to create the help link
			Head.AddHelpLink(GetString("DETAILS"));
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindPropertyData()
		{
			// table
			DataTable propertyTable = new DataTable();
			propertyTable.Columns.Add("Label");
			propertyTable.Columns.Add("Value");

			try
			{
				// ifolder
				iFolderDetails ifolder = web.GetiFolderDetails(ifolderID);

				// rights
				PropertyActions.Visible = (ifolder.MemberRights == Rights.Admin);

				propertyTable.Rows.Add(new object[] { GetString("NAME"), ifolder.Name });
				propertyTable.Rows.Add(new object[] { GetString("DESCRIPTION"), ifolder.Description });
				propertyTable.Rows.Add(new object[] { GetString("LASTMODIFIED"), WebUtility.FormatDate(ifolder.LastModified, rm) });
				propertyTable.Rows.Add(new object[] { GetString("CREATED"), WebUtility.FormatDate(ifolder.Created, rm) });
				propertyTable.Rows.Add(new object[] { GetString("RIGHTS"), WebUtility.FormatRights(ifolder.MemberRights, rm) });
				propertyTable.Rows.Add(new object[] { GetString("OWNER"), ifolder.OwnerFullName });
				propertyTable.Rows.Add(new object[] { GetString("SIZE"), WebUtility.FormatSize(ifolder.Size, rm) });
				propertyTable.Rows.Add(new object[] { GetString("MEMBERS"), ifolder.MemberCount.ToString() });
				propertyTable.Rows.Add(new object[] { GetString("FILES"), ifolder.FileCount.ToString() });
				propertyTable.Rows.Add(new object[] { GetString("FOLDERS"), ifolder.DirectoryCount.ToString() });
				propertyTable.Rows.Add(new object[] { GetString("LOCKED"), WebUtility.FormatYesNo(!ifolder.Enabled, rm) });
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// data grid
			PropertyData.DataSource = propertyTable;
			PropertyData.DataBind();
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindPolicyData()
		{
			// table
			DataTable policyTable = new DataTable();
			policyTable.Columns.Add("Label");
			policyTable.Columns.Add("Value");

			try
			{
				// ifolder
				iFolderPolicy policy = web.GetiFolderPolicy(ifolderID);

				// rights
				//PolicyActions.Visible = (ifolder.Rights == Rights.Admin);
				PolicyActions.Visible = false;

				policyTable.Rows.Add(new object[] { GetString("SYNCINTERVAL"), policy.SyncIntervalEffective });
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// data grid
			PolicyData.DataSource = policyTable;
			PolicyData.DataBind();
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
		}

		#endregion
	}
}
