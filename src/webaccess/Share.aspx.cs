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
	/// Share Page
	/// </summary>
	public class SharePage : Page
	{
		/// <summary>
		/// iFolder Name
		/// </summary>
		protected Literal iFolderName;

		/// <summary>
		/// History Data
		/// </summary>
		protected DataGrid UserData;

		/// <summary>
		/// History Pagging
		/// </summary>
		protected PaggingControl UserPagging;

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
		/// The Share Button
		/// </summary>
		protected Button ShareButton;

		/// <summary>
		/// The Cancel Button
		/// </summary>
		protected Button CancelButton;

		/// <summary>
		/// iFolder Name
		/// </summary>
		protected DropDownList SearchPropertyList;

		/// <summary>
		/// Search Pattern
		/// </summary>
		protected TextBox SearchPattern;

		/// <summary>
		/// Search Button
		/// </summary>
		protected Button SearchButton;

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
		/// Members
		/// </summary>
		private Hashtable members;

		/// <summary>
		/// Current Members
		/// </summary>
		private Hashtable currentMembers;

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
				// strings
				UserPagging.LabelSingular = GetString("USER");
				UserPagging.LabelPlural = GetString("USERS");
				MemberPagging.LabelSingular = GetString("MEMBER");
				MemberPagging.LabelPlural = GetString("MEMBERS");
				SearchButton.Text = GetString("SEARCH");
				ShareButton.Text = GetString("SHARE");
				CancelButton.Text = GetString("CANCEL");

				// properties
				SearchPropertyList.Items.Add(new ListItem(GetString("FIRSTNAME"), SearchProperty.FirstName.ToString()));
				SearchPropertyList.Items.Add(new ListItem(GetString("LASTNAME"), SearchProperty.LastName.ToString()));
				SearchPropertyList.Items.Add(new ListItem(GetString("USERNAME"), SearchProperty.UserName.ToString()));

				//Default selecting UserName
				SearchPropertyList.SelectedIndex = 2;

				// members
				members = new Hashtable();
				ViewState["Members"] = members;

				// current members
				currentMembers = new Hashtable();
				ViewState["CurrentMembers"] = currentMembers;

				// search pattern
				ViewState["SearchPattern"] = null;

				// data
				BindData();
			}
			else
			{
				// members
				members = (Hashtable)ViewState["Members"];

				// current members
				currentMembers = (Hashtable)ViewState["CurrentMembers"];
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			BindCurrentMembersData();

			BindUserData();

			BindMemberData();
			
			// Pass this page information to create the help link
			Head.AddHelpLink(GetString("SHARE"));
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindCurrentMembersData()
		{
			try
			{
				// name
				iFolder ifolder = web.GetiFolder(ifolderID);
				iFolderName.Text = ifolder.Name;

				// current members
				iFolderUserSet members = web.GetMembers(ifolderID, 0, 0);

				foreach(iFolderUser member in members.Items)
				{
					currentMembers.Add(member.ID, member.UserName);
				}
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindUserData()
		{
			// keep search pattern consistent
			SearchPattern.Text = (string)ViewState["SearchPattern"];

			// user
			DataTable userTable = new DataTable();
			userTable.Columns.Add("ID");
			userTable.Columns.Add("FullName");
			userTable.Columns.Add("Enabled", typeof(bool));

			try
			{
				// user
				SearchProperty prop = (SearchProperty)Enum.Parse(typeof(SearchProperty), SearchPropertyList.SelectedItem.Value);

				iFolderUserSet users = web.GetUsersBySearch(
					prop, SearchOperation.BeginsWith, SearchPattern.Text,
					UserPagging.Index, UserPagging.PageSize);
				UserPagging.Count = users.Items.Length;
				UserPagging.Total = users.Total;
				
				string name;

				foreach(iFolderUser user in users.Items)
				{
					DataRow row = userTable.NewRow();

					// display name
					switch(prop)
					{
					        case SearchProperty.UserName:
						    name = user.UserName;
						    break;

						case SearchProperty.LastName:
							name = String.Format("{0}{1}{2}", user.LastName,
								GetString("LASTFIRSTNAMESEP"), user.FirstName);
							break;

						case SearchProperty.FirstName:
						default:
							name = String.Format("{0}{1}{2}", user.FirstName,
								GetString("FIRSTLASTNAMESEP"), user.LastName);
							break;
					}

					row["ID"] = user.ID;
					row["FullName"] = name;
					row["Enabled"] = !members.ContainsKey(user.ID) && !currentMembers.ContainsKey(user.ID);

					userTable.Rows.Add(row);
				}
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}

			// bind
			UserData.DataSource = userTable;
			UserData.DataKeyField = "ID";
			UserData.DataBind();
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindMemberData()
		{
			// member
			DataTable memberTable = new DataTable();
			memberTable.Columns.Add("ID");
			memberTable.Columns.Add("FullName");
			memberTable.Columns.Add("Enabled", typeof(bool));

			// member
			ArrayList memberList = new ArrayList(members.Values);
			memberList.Sort();

			MemberPagging.Total = memberList.Count;

			for(int i=MemberPagging.Index;
				(i < memberList.Count) && ((i - MemberPagging.Index) < MemberPagging.PageSize ); i++)
			{
				MemberInfo member = (MemberInfo)memberList[i];

				DataRow row = memberTable.NewRow();
				row["ID"] = member.ID;
				row["FullName"] = (member.FullName == "")? member.Username : member.FullName;
				row["Enabled"] = member.Removable;

				memberTable.Rows.Add(row);
			}

			MemberPagging.Count = memberTable.Rows.Count;

			// bind
			MemberData.DataSource = memberTable;
			MemberData.DataKeyField = "ID";
			MemberData.DataBind();
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
					
					// TEMP
					Message.Text = type;

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
			this.UserPagging.PageChange += new EventHandler(UserPagging_PageChange);
			this.MemberPagging.PageChange += new EventHandler(MemberPagging_PageChange);
			this.SearchButton.Click += new EventHandler(SearchButton_Click);
			this.UserData.ItemCommand += new DataGridCommandEventHandler(UserData_ItemCommand);
			this.MemberData.ItemCommand += new DataGridCommandEventHandler(MemberData_ItemCommand);
			this.ShareButton.Click += new EventHandler(ShareButton_Click);
			this.CancelButton.Click += new EventHandler(CancelButton_Click);
			this.PreRender += new EventHandler(SharePage_PreRender);
		}

		#endregion

		/// <summary>
		/// User Page Change
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void UserPagging_PageChange(object sender, EventArgs e)
		{
			BindUserData();
		}

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
		/// Search Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SearchButton_Click(object sender, EventArgs e)
		{
			// update search pattern
			ViewState["SearchPattern"] = SearchPattern.Text;

			// reset pagging
			UserPagging.Index = 0;

			BindUserData();
		}

		/// <summary>
		/// User Data Commmand
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void UserData_ItemCommand(object source, DataGridCommandEventArgs e)
		{
			try
			{
				// add
				iFolderUser user = web.GetUser((string)e.CommandArgument);

				members.Add(user.ID, new MemberInfo(user));

				// reset pagging
				MemberPagging.Index = 0;

				BindUserData();
				BindMemberData();
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
		}

		/// <summary>
		/// Member Data Command
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void MemberData_ItemCommand(object source, DataGridCommandEventArgs e)
		{
			// remove
			members.Remove(e.CommandArgument);

			BindUserData();
			BindMemberData();
		}

		/// <summary>
		/// Share Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ShareButton_Click(object sender, EventArgs e)
		{
			try
			{
				// add the members
				string memberList = null;

				foreach(string userID in members.Keys)
				{
					if (memberList == null)
					{
						memberList = userID;
					}
					else
					{
						memberList = String.Format("{0},{1}", memberList, userID);
					}
				}

				if (memberList != null)
				{
					// add with read only rights
					web.AddMember(ifolderID, memberList, Rights.ReadOnly);
				}

				// back to iFolder
				Response.Redirect("Members.aspx?iFolder=" + ifolderID);
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
			Response.Redirect("Members.aspx?iFolder=" + ifolderID);
		}

		/// <summary>
		/// Page Pre-Render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SharePage_PreRender(object sender, EventArgs e)
		{
			ShareButton.Enabled = (MemberPagging.Count > 0);
		}

		/// <summary>
		/// State Member Info
		/// </summary>
		[Serializable]
		private class MemberInfo : IComparable
		{
			private string id;
			private string username;
			private string fullName;
			private bool removable;
			private DateTime created;

			public MemberInfo(iFolderUser user) : this(user, true)
			{
			}
			/// <summary>
			/// constructor for memberinfo
			/// </summary>
			/// <param name="user"></param>
			/// <param name="removable"></param>
			public MemberInfo(iFolderUser user, bool removable)
			{
				this.id = user.ID;
				this.username = user.UserName;
				this.fullName = user.FullName;
				this.removable = removable;
				this.created = DateTime.Now;
			}
			
			#region Properties
			
            /// <summary>
            /// gets ID
            /// </summary>
			public string ID
			{
				get { return id; }
			}

            /// <summary>
            /// gets username
            /// </summary>
			public string Username
			{
				get { return username; }
			}

            /// <summary>
            /// gets Fullname
            /// </summary>
			public string FullName
			{
				get { return fullName; }
			}


            /// <summary>
            /// Gets removable status
            /// </summary>
			public bool Removable
			{
				get { return removable; }
			}

			#endregion

			#region IComparable Members

            /// <summary>
            /// Icomparable member for comapring 2 objects
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
			public int CompareTo(object obj)
			{
				// sort in reverse chronological order
				return (obj as MemberInfo).created.CompareTo(this.created);
			}

			#endregion
		}
	}
}
