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
	/// Members Page
	/// </summary>
	public class MembersPage : Page
	{
		/// <summary>
		/// Actions Container
		/// </summary>
		protected HtmlContainerControl Actions;

		/// <summary>
		/// Actions Container
		/// </summary>
		protected HtmlContainerControl ActionsOwner;

		/// <summary>
		/// Member Data
		/// </summary>
		protected DataGrid MemberData;

		/// <summary>
		/// Member Pagging
		/// </summary>
		protected PaggingControl MemberPagging;

		/// <summary>
		/// Message Box
		/// </summary>
		protected MessageControl Message;
		
		/// <summary>
		/// Header page
		/// </summary>
		protected HeaderControl Head;

		/// <summary>
		/// The Add Button
		/// </summary>
		protected LinkButton AddButton;

		/// <summary>
		/// The Remove Button
		/// </summary>
		protected LinkButton RemoveButton;

		/// <summary>
		/// The Read Only Button
		/// </summary>
		protected LinkButton ReadOnlyButton;

		/// <summary>
		/// The Read/Write Button
		/// </summary>
		protected LinkButton ReadWriteButton;

		/// <summary>
		/// The Admin Button
		/// </summary>
		protected LinkButton AdminButton;

		/// <summary>
		/// The Owner Button
		/// </summary>
		protected LinkButton OwnerButton;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb remoteweb;

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

			remoteweb = new iFolderWeb();
			remoteweb.PreAuthenticate = true;
                        remoteweb.Credentials = web.Credentials;
                        remoteweb.Url = web.Url;

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				// data
				MemberData.Columns[ 3 ].HeaderText = GetString( "MEMBERS" );
                                MemberData.Columns[ 4 ].HeaderText = GetString( "RIGHTS" );
                                MemberData.Columns[ 5 ].HeaderText = GetString( "HISTORY" );

				BindData();

				// url
				//AddButton.NavigateUrl = "Share.aspx?iFolder=" + ifolderID;

				// strings
				AddButton.Text = GetString("ADD");
				RemoveButton.Text = GetString("REMOVE");
				ReadOnlyButton.Text = GetString("RIGHTS.READONLY");
				ReadWriteButton.Text = GetString("RIGHTS.READWRITE");
				AdminButton.Text = GetString("RIGHTS.ADMIN");
				OwnerButton.Text = GetString("OWNER");
				MemberPagging.LabelSingular = GetString("MEMBER");
				MemberPagging.LabelPlural = GetString("MEMBERS");
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			BindMemberData();
			// Pass this page information to create the help link
			Head.AddHelpLink(GetString("MEMBERS"));
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindMemberData()
		{
			// member
			DataTable memberTable = new DataTable();
			memberTable.Columns.Add("ID");
			memberTable.Columns.Add("Name");
			memberTable.Columns.Add("UserName");
			memberTable.Columns.Add("Rights");
			memberTable.Columns.Add("Enabled", typeof(bool));
			memberTable.Columns.Add("iFolderID");

			try
			{
				// ifolder
				iFolder ifolder = web.GetiFolder(ifolderID);

				// rights
				Actions.Visible = (ifolder.MemberRights == Rights.Admin);
				ActionsOwner.Visible = ifolder.IsOwner;
				MemberData.Columns[1].Visible = Actions.Visible;

				// member
				iFolderUserSet members = web.GetMembers(ifolderID, MemberPagging.Index, MemberPagging.PageSize);
				MemberPagging.Count = members.Items.Length;
				MemberPagging.Total = members.Total;
				string accessID = (Session["User"] as iFolderUser).ID;

				foreach(iFolderUser member in members.Items)
				{
					DataRow row = memberTable.NewRow();

					row["ID"] = member.ID;
					row["Name"] = ( member.FullName == "" ) ? member.UserName : member.FullName;
					row["UserName"] = member.UserName;
					row["Rights"] = member.IsOwner ? GetString("OWNER") : WebUtility.FormatRights(member.MemberRights, rm);
					row["Enabled"] = !member.IsOwner && (member.ID != accessID && !IsGroupMember(member.ID, accessID) ) ;
					row["iFolderID"] = ifolderID;

					memberTable.Rows.Add(row);
				}
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// bind
			MemberData.DataSource = memberTable;
			MemberData.DataBind();
		}

		/// <summary>
		/// return whether user is member of group or not
		/// </summary>
		/// <returns> true/false</returns>
		private bool IsGroupMember(string GroupID, string UserID)
		{
			bool GroupMember = false;
			string [] GroupIDs = web.GetGroupIDs(UserID);
			foreach ( string groupid in GroupIDs )
			{
				if ( GroupID == groupid )
				{
					GroupMember = true;
					break;
				}
			}
			return GroupMember;
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
				case "InvalidOperationException":
					Message.Text = GetString("CHANGEOWNERRIGHTSEXCEPTION");
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
			this.MemberPagging.PageChange += new EventHandler(MemberPagging_PageChange);
			this.AddButton.Click += new EventHandler(AddButton_Click);
			this.RemoveButton.Click += new EventHandler(RemoveButton_Click);
			this.ReadOnlyButton.Click += new EventHandler(ReadOnlyButton_Click);
			this.ReadWriteButton.Click += new EventHandler(ReadWriteButton_Click);
			this.AdminButton.Click += new EventHandler(AdminButton_Click);
			this.OwnerButton.PreRender += new EventHandler(OwnerButton_PreRender);
			this.OwnerButton.Click += new EventHandler(OwnerButton_Click);
		}

		#endregion

		/// <summary>
		/// Member Page Change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MemberPagging_PageChange(object sender, EventArgs e)
		{
			BindMemberData();
		}

		/// <summary>
		/// Get the Selected Members
		/// </summary>
		/// <returns>A comma-delimited list of member user ids.</returns>
		private string GetSelectedMembers()
		{
			string memberList = null;

			// selected members
			foreach(DataGridItem item in MemberData.Items)
			{
				CheckBox checkBox = (CheckBox) item.FindControl("Select");

				if (checkBox.Checked)
				{
					string id = item.Cells[0].Text;

					if (memberList == null)
					{
						memberList = id;
					}
					else
					{
						memberList = String.Format("{0},{1}", memberList, id);
					}
				}
			}

			return memberList;
		}

		/// <summary>
		/// Add Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AddButton_Click(object sender, EventArgs e)
		{
			/// allow members to be added only if sharing was not disabled
			bool Sharing = web.GetSharingPolicy(ifolderID);
			if (Sharing == true)
			{
				Response.Redirect("Share.aspx?iFolder=" + ifolderID);
			}
			else
			{
				Message.Text = GetString("SHARINGPOLICYVIOLATION");
				return;
			}
		}	

		/// <summary>
		/// Remove Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveButton_Click(object sender, EventArgs e)
		{
			string memberList = GetSelectedMembers();

			// remove members
			if (memberList != null)
			{
				try
				{
					web.RemoveMember(ifolderID, memberList);
				}
				catch(SoapException ex)
				{
					if (!HandleException(ex)) throw;
				}

				MemberPagging.Index = 0;
				BindMemberData();
			}
		}

		/// <summary>
		/// Set Rights
		/// </summary>
		/// <param name="rights"></param>
		private void SetRights(Rights rights)
		{
			string memberList = GetSelectedMembers();

			// remove members
			if (memberList != null)
			{
				try
				{
					web.SetMemberRights(ifolderID, memberList, rights);
				}
				catch(SoapException ex)
				{
					if (!HandleException(ex)) throw;
				}

				BindMemberData();
			}
		}

		/// <summary>
		/// Read Only Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReadOnlyButton_Click(object sender, EventArgs e)
		{
			SetRights(Rights.ReadOnly);
		}

		/// <summary>
		/// Read/Write Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReadWriteButton_Click(object sender, EventArgs e)
		{
			SetRights(Rights.ReadWrite);
		}

		/// <summary>
		/// Admin Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AdminButton_Click(object sender, EventArgs e)
		{
			SetRights(Rights.Admin);
		}

		/// <summary>
		/// Owner Button Pre-Render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OwnerButton_PreRender(object sender, EventArgs e)
		{
			OwnerButton.Attributes["onclick"] = "return ConfirmChangeOwner(this.form);";
		}

		/// <summary>
		/// Owner Button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OwnerButton_Click(object sender, EventArgs e)
		{
			string ownerID = GetSelectedMembers();

			// remove members
			if (ownerID != null)
			{
				try
				{
					// if for this user , encryption is enforced , then show the message telling it can not take the ownership	

					bool IsEncryptionEnforced = web.IsUserOrSystemEncryptionEnforced(ownerID) ;
					if( IsEncryptionEnforced)
					{
                	                        Message.Text = GetString("ERRORIFOLDERENCRYPTIONENFORCED");
                        	                return;
					}
						
					//check for no of ifolders per user limit policy for the user who is taking up Ownership
	                                if ( web.GetiFolderLimitPolicyStatus(ownerID) != 1)
        	                        {
                	                        Message.Text = GetString("ERRORIFOLDERTRANSFEREXCEPTION");
                        	                return;
                                	}
					// 3rd argument is false because its not going to adopt any orphaned ifolder
					web.SetiFolderOwner(ifolderID, ownerID, false);
				}
				catch(SoapException ex)
				{
					if (!HandleException(ex)) throw;
				}

				BindMemberData();
			}
		}
	}
}
