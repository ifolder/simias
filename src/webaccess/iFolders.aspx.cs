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
using System.Net;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Resources;
using System.Web.Security;
using System.Web.Services.Protocols;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// iFolders Page
	/// </summary>
	public class iFoldersPage : Page
	{
		/// <summary>
		/// Context
		/// </summary>
		protected HomeContextControl HomeContext;

		/// <summary>
		/// New iFolder Link
		/// </summary>
		protected HyperLink NewiFolderLink;

		/// <summary>
                /// New iFolder Link
                /// </summary>
                //protected HyperLink NewiFolderLink;

		/// <summary>
                /// The separator stick
                /// </summary>
                protected Label FirstSingleStick;

		/// <summary>
		/// iFolder Data
		/// </summary>
		protected DataGrid iFolderData;

		/// <summary>
                /// The Delete Button
                /// </summary>
                protected LinkButton RemoveMemberDeleteButton;

                /// <summary>
                /// The DeleteDisabled Label
                /// </summary>
                protected Label RemoveMemberDeleteDisabled;

		/// <summary>
                /// Server URL for logged in User.
                /// </summary>
                protected string currentServerURL;

		/// <summary>
		/// Pagging
		/// </summary>
		protected PaggingControl iFolderPagging;

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
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, EventArgs e)
		{
			string ErrorMsg =  Request.QueryString.Get("ErrorMsg");
			// connection
			web = (iFolderWeb)Session["Connection"];
			
			currentServerURL = String.Copy(web.Url);

			// localization
			rm = (ResourceManager) Application["RM"];
			
			if (!IsPostBack)
			{

				// strings
				iFolderData.Columns[ 4 ].HeaderText = GetString( "NAME" );
                                iFolderData.Columns[ 5 ].HeaderText = GetString( "DATE" );
                                iFolderData.Columns[ 6 ].HeaderText = GetString( "OWNER" );
				iFolderPagging.LabelSingular = GetString("IFOLDER");
				iFolderPagging.LabelPlural = GetString("IFOLDERS");
				NewiFolderLink.Text = GetString("NEWIFOLDER");
				//NewiFolderLink1.Text = GetString("NEWIFOLDER");
				RemoveMemberDeleteDisabled.Text = GetString("REMOVEMEMBERDELETE");
				RemoveMemberDeleteButton.Text = GetString("REMOVEMEMBERDELETE"); 
				FirstSingleStick.Text = "|";
				FirstSingleStick.Visible = true;
				

				// data
				BindData();
			}
			if(ErrorMsg != null && ErrorMsg != String.Empty )
			{
                        	Message.Text = GetString("ENTRY.ENTRYINVALIDNAME");
                        	return;
			}

		}


		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			// table
			DataTable ifolderTable = new DataTable();
			ifolderTable.Columns.Add("ID");
			ifolderTable.Columns.Add("Image");
			ifolderTable.Columns.Add("Name");
			ifolderTable.Columns.Add("iFolderFullName");
			ifolderTable.Columns.Add("LastModified");
			ifolderTable.Columns.Add("Description");
			ifolderTable.Columns.Add("Rights");
			ifolderTable.Columns.Add("Size");
			ifolderTable.Columns.Add("OwnerFullName");
			ifolderTable.Columns.Add( new DataColumn( "EnabledField", typeof( bool ) ) );

			// category
			iFolderCategory category = HomeContext.Category;
			MemberRole role = MemberRole.Any;
			DateTime after = DateTime.MinValue;

			switch(category)
			{
				case iFolderCategory.Recent:
					after = DateTime.Now.AddDays(-30);
					break;

				case iFolderCategory.Owned:
					role = MemberRole.Owner;
					break;

				case iFolderCategory.Shared:
					role = MemberRole.Shared;
					break;

				case iFolderCategory.All:
				default:
					break;
			}

			try
			{
				// data
				string escPattern = Regex.Escape(HomeContext.Pattern).Replace("\\*", ".*").Replace("\\?", ".") ;
				iFolderSet ifolders = web.GetiFoldersBySearch(role, after, SearchOperation.Contains,
					escPattern, iFolderPagging.Index, iFolderPagging.PageSize);
				iFolderPagging.Count = ifolders.Items.Length;
				iFolderPagging.Total = ifolders.Total;
				
				string name, ImageUrl;
				bool pattern = (HomeContext.Pattern != null) && (HomeContext.Pattern.Length > 0);

				foreach(iFolder ifolder in ifolders.Items)
				{
				
					bool encrypted = false;
					try{
						string ifolderLocation = web.GetiFolderLocation (ifolder.ID);
						UriBuilder remoteurl = new UriBuilder(ifolderLocation);
	                                        remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
        	                                web.Url = remoteurl.Uri.ToString();
					}catch
					{
						continue;	
					}
					iFolder folder = null ;
					try{
						folder = web.GetiFolder(ifolder.ID);
					}
					catch(Exception e)
					{
						string type = e.GetType().Name;
						if( type != null && type == "MemberDoesNotExistException")
						{
							/// If we get member does not exist exception in the first call itself, no need to make a GetiFolder call to current server.
							continue;
						}
						web.Url = currentServerURL;
						try
						{
        	                                	folder = web.GetiFolder(ifolder.ID);
						}
						catch
						{	
							/// If we are unable to get the iFolder information for the current entry, skip this and go to next entry...
							folder = null;
						}
					}
					/// By any chance if we are not able to get the iFolder information, proceed fetch the details of next entry. 
					if( folder == null)
						continue;
					string EncryptionAlgorithm = folder.EncryptionAlgorithm;
					if(!(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty)))
					{
						// It is an encrypted ifolder 
						encrypted = true;
					}
					
					bool shared = ( ifolder.MemberCount > 1 ) ? true : false;

					bool enabled = ( folder.Enabled) ? true : false;
					
					ImageUrl = (! enabled )? "ifolder_16-gray.gif" : ( (encrypted) ? "encrypt_ilock2_16.gif" : (shared ? "ifolder_user_16.gif" : "ifolder.png") );

					DataRow row = ifolderTable.NewRow();

					// selected name
					if (pattern)
					{
						name = Regex.Replace(ifolder.Name, String.Format("({0})", escPattern),
							"<span class='highlight'>${1}</span>", RegexOptions.IgnoreCase);
					}
					else
					{
						name = ifolder.Name;
					}
					
					string ShortenedName = null;
					int ShortenedLength = 70;
					if(!pattern && name.Length > ShortenedLength)
					{
						// make it of desired length
						ShortenedName = web.GetShortenedName(name, ShortenedLength);
					}
					row["ID"] = ifolder.ID;
					row["Image"] = ImageUrl;
					row["Name"] = ( !pattern && (name.Length > ShortenedLength) ) ? ShortenedName : name;
					row["iFolderFullName"] = name;
					row["LastModified"] = WebUtility.FormatDate(ifolder.LastModified, rm);
					row["Description"] = ifolder.Description;
					row["Rights"] = WebUtility.FormatRights(ifolder.MemberRights, rm);
					row["Size"] = WebUtility.FormatSize(ifolder.Size, rm);
					row["OwnerFullName"] = ( ifolder.OwnerFullName == "")? ifolder.OwnerUserName : ifolder.OwnerFullName;
					row["EnabledField"] = enabled ;

					ifolderTable.Rows.Add(row);
				}
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// view
			DataView ifolderView = new DataView(ifolderTable);
			ifolderView.Sort = "Name";
			
			// data grid
			iFolderData.DataKeyField = "ID";
			iFolderData.DataSource = ifolderView;
			iFolderData.DataBind();
			
			// Pass this page information to create the help link
			Head.AddHelpLink(GetString("IFOLDERS"));

			string PasswordChanged =  Request.QueryString.Get("PasswordChanged");
			if(PasswordChanged != null && PasswordChanged == "true")
			{
				Message.Info = GetString("PASSWORDCHANGESUCCESS");
				return;
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
	
		/// <summary>
		/// Whether an iFolder is enabled or not 
		/// </summary>
		/// <param name="ifolder id"></param>
		/// <returns></returns>
		protected bool IsiFolderEnabled( object Enabled)
		{
			return ((bool) Enabled);
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
			this.ID = "iFolderView";
			this.Load += new System.EventHandler(this.Page_Load);
			this.iFolderPagging.PageChange += new EventHandler(iFolderPagging_PageChange);
			this.HomeContext.Search += new EventHandler(HomeContext_Search);
			this.RemoveMemberDeleteButton.PreRender += new EventHandler(RemoveMemberDeleteButton_PreRender);
                        this.RemoveMemberDeleteButton.Click += new EventHandler(RemoveMemberDeleteButton_Click);
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
				case "InvalidOperationException":
					Message.Text = GetString("REMOVEOWNEREXCEPTION");
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
		/// iFolder Page Change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void iFolderPagging_PageChange(object sender, EventArgs e)
		{
			BindData();
		}

		/// <summary>
		/// Home Context Search Event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HomeContext_Search(object sender, EventArgs e)
		{
			// reset paging
			iFolderPagging.Index = 0;

			BindData();
		}


		/// <summary>
                /// RemoveMemberhsip Button Pre-Render
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                private void RemoveMemberDeleteButton_PreRender(object sender, EventArgs e)
                {
                        RemoveMemberDeleteButton.Attributes["onclick"] = "return ConfirmRemoveDelete(this.form);";
                }

                /// <summary>
                /// Delete Button Click
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                private void RemoveMemberDeleteButton_Click(object sender, EventArgs e)
                {
			iFolder ifolder = null;

                        // selected members
                        try
                        {
                                foreach(DataGridItem item in iFolderData.Items)
                                {
                                        CheckBox checkBox = (CheckBox) item.FindControl("Select");

                                        if (checkBox.Checked)
                                        {
                                                string id = item.Cells[0].Text;
                                                ifolder =  web.GetiFolder(id);

                                                if( !ifolder.IsOwner )
                                                {
							string ifolderLocation = web.GetiFolderLocation (id);
							string CurrentURL = web.Url;
                                			UriBuilder remoteurl = new UriBuilder(ifolderLocation);
                                			remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                                			web.Url = remoteurl.Uri.ToString();
                                                        web.RemoveMembership(id);
							web.Url = CurrentURL;
                                                }
						else 
						{
							web.DeleteiFolder(id);
						}	
                                        }
                                }
                        }

                        catch(SoapException ex)
                        {
				if (ex.Message.IndexOf("readonly rights") != -1 )
                                {
					Message.Text = GetString("GROUPACCESSEXCEPTION");
					return;
                                }

                                if (!HandleException(ex)) throw;
                        }

                        Response.Redirect("iFolders.aspx");
	
                }


	}
}
