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
		protected Pagging UserPagging;

		/// <summary>
		/// Member Data
		/// </summary>
		protected DataGrid MemberData;

		/// <summary>
		/// Member Pagging
		/// </summary>
		protected Pagging MemberPagging;

		/// <summary>
		/// Message Box
		/// </summary>
		protected Message MessageBox;

		/// <summary>
		/// The Share Button
		/// </summary>
		protected LinkButton ShareButton;

		/// <summary>
		/// The Cancel Link
		/// </summary>
		protected HyperLink CancelLink;

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
				CancelLink.Text = GetString("CANCEL");

				// properties
				SearchPropertyList.Items.Add(new ListItem(GetString("FIRSTNAME"), SearchProperty.Name.ToString()));
				SearchPropertyList.Items.Add(new ListItem(GetString("LASTNAME"), SearchProperty.LastName.ToString()));

				// members
				members = new Hashtable();
				ViewState["Members"] = members;

				// current members
				currentMembers = new Hashtable();
				ViewState["CurrentMembers"] = currentMembers;

				// link
				CancelLink.NavigateUrl = "Members.aspx?iFolder=" + ifolderID;

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
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindCurrentMembersData()
		{
			try
			{
				int total;

				// name
				iFolder ifolder = web.GetiFolder(ifolderID);
				iFolderName.Text = ifolder.Name;

				// current members
				iFolderUser[] members = web.GetMembers(ifolderID, 0, 0, out total);

				if (members != null)
				{
					foreach(iFolderUser member in members)
					{
						currentMembers.Add(member.ID, member.UserName);
					}
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
			int total = 0;

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
				iFolderUser[] users = web.GetUsersBySearch(
					(SearchProperty)Enum.Parse(typeof(SearchProperty), SearchPropertyList.SelectedItem.Value),
					SearchOperation.BeginsWith, SearchPattern.Text,
					UserPagging.Index, UserPagging.PageSize, out total);
				UserPagging.Count = users.Length;
				UserPagging.Total = total;
				
				foreach(iFolderUser user in users)
				{
					DataRow row = userTable.NewRow();

					row["ID"] = user.ID;
					row["FullName"] = user.FullName;
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
				row["FullName"] = member.FullName;
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
				case "AccessException":
					MessageBox.Text = GetString("ENTRY.ACCESSEXCEPTION");
					break;

				default:
					
					// TEMP
					MessageBox.Text = type;

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
			
			public MemberInfo(iFolderUser user, bool removable)
			{
				this.id = user.ID;
				this.username = user.UserName;
				this.fullName = user.FullName;
				this.removable = removable;
				this.created = DateTime.Now;
			}
			
			#region Properties
			
			public string ID
			{
				get { return id; }
			}

			public string Username
			{
				get { return username; }
			}

			public string FullName
			{
				get { return fullName; }
			}

			public bool Removable
			{
				get { return removable; }
			}

			#endregion

			#region IComparable Members

			public int CompareTo(object obj)
			{
				// sort in reverse chronological order
				return (obj as MemberInfo).created.CompareTo(this.created);
			}

			#endregion
		}
	}
}