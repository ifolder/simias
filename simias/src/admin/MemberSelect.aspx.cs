/***********************************************************************
 *  $RCSfile: MemberSelect.aspx.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Mike Lasky (mlasky@novell.com)
 * 
 ***********************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Resources;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for MemberSelect.
	/// </summary>
	public class MemberSelect : System.Web.UI.Page
	{
		#region Class Members
	
		/// <summary>
		/// Viewable MemberList data grid cell indices.
		/// </summary>
		private const int Member_IDCell       = 0;
		private const int Member_CheckBoxCell = 1;
		private const int Member_ImageCell    = 2;
		private const int Member_UserNameCell = 3;
		private const int Member_FullNameCell = 4;

		/// <summary>
		/// Operations
		/// </summary>
		private enum PageOp
		{
			AddMember,
			AddAdmin,
			CreateiFolder
		}


		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;

		/// <summary>
		/// Page title control.
		/// </summary>
		protected Label HeaderTitle;

		/// <summary>
		/// Page subtitle control.
		/// </summary>
		protected Label SubHeaderTitle;


		/// <summary>
		/// Web controls.
		/// </summary>
		protected Button BackButton;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected Button OkButton;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected Button CancelButton;

		/// <summary>
		/// All members selection checkbox control.
		/// </summary>
		protected CheckBox AllMembersCheckBox;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected DataGrid MemberList;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected MemberSearch MemberSearch;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected ListFooter MemberListFooter;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the current user offset.
		/// </summary>
		private int CurrentUserOffset
		{
			get { return ( int )ViewState[ "CurrentUserOffset" ]; }
			set { ViewState[ "CurrentUserOffset" ] = value; }
		}

		/// <summary>
		/// Gets the iFolder Description.
		/// </summary>
		private string iFolderDescription
		{
			get 
			{ 
				string param = Request.Params[ "desc" ];
				return ( param == null ) ? String.Empty : param;
			} 
		}

		/// <summary>
		/// Gets or sets the existing members list.
		/// </summary>
		private Hashtable ExistingMemberList
		{
			get { return ViewState[ "ExistingMembers" ] as Hashtable; }
			set { ViewState[ "ExistingMembers" ] = value; }
		}

		/// <summary>
		/// Gets the full name of the owner of the iFolder.
		/// </summary>
		private string FullName
		{
			get 
			{ 
				string param = Request.Params[ "fn" ];
				if ( ( param == null ) || ( param == String.Empty ) )
				{
					throw new HttpException( ( int )HttpStatusCode.BadRequest, "No full name was specified." );
				}

				return param;
			}
		}

		/// <summary>
		/// Returns true if any members are checked to be added.
		/// </summary>
		private bool HasMembersToAdd
		{
			get { return ( MembersToAdd.Count > 0 ) ? true : false; }
		}

		/// <summary>
		/// Gets the iFolder ID.
		/// </summary>
		private string iFolderID
		{
			get 
			{ 
				string param = Request.Params[ "id" ];
				if ( ( param == null ) || ( param == String.Empty ) )
				{
					throw new HttpException( ( int )HttpStatusCode.BadRequest, "No ifolder was specified." );
				}

				return param;
			} 
		}

		/// <summary>
		/// Gets the user or iFolder name.
		/// </summary>
		private string iFolderName
		{
			get
			{
				string param = Request.Params[ "name" ];
				if ( ( param == null ) || ( param == String.Empty ) )
				{
					throw new HttpException( ( int )HttpStatusCode.BadRequest, "No user name was specified." );
				}

				return param;
			}
		}

		/// <summary>
		/// Gets the owner of the iFolder.
		/// </summary>
		private string iFolderOwner
		{
			get 
			{ 
				string param = Request.Params[ "owner" ];
				if ( ( param == null ) || ( param == String.Empty ) )
				{
					throw new HttpException( ( int )HttpStatusCode.BadRequest, "No owner was specified." );
				}

				return param;
			}
		}

		/// <summary>
		/// Gets the starting member list page of the previous owner select page.
		/// </summary>
		private string MemberListPage
		{
			get 
			{ 
				string param = Request.Params[ "pg" ];
				return ( ( param == null ) || ( param == String.Empty ) ) ? "0" : param;
			} 
		}

		/// <summary>
		/// Gets the operation to perform for this web page.
		/// </summary>
		private PageOp Operation
		{
			get
			{
				string param = Request.Params[ "op" ];
				if ( ( param != null ) && ( param != String.Empty ) )
				{
					switch ( param.ToLower() )
					{
						case "addmember":
							return PageOp.AddMember;

						case "addadmin":
							return PageOp.AddAdmin;

						case "createifolder":
							return PageOp.CreateiFolder;

						default:
							throw new HttpException( ( int )HttpStatusCode.BadRequest, "An invalid operation was specified." );
					}
				}
				else
				{
					throw new HttpException( ( int ) HttpStatusCode.BadRequest, "No operation was specified." );
				}
			}
		}

		/// <summary>
		/// Gets or sets the referring page.
		/// </summary>
		private string ReferringPage
		{
			get { return ViewState[ "ReferringPage" ] as String; }
			set { ViewState[ "ReferringPage" ] = value; }
		}

		/// <summary>
		/// Gets or sets the members to add information.
		/// </summary>
		private Hashtable MembersToAdd
		{
			get { return Session[ "MembersToAdd" ] as Hashtable; }
			set { Session[ "MembersToAdd" ] = value; }
		}

		/// <summary>
		/// Gets or sets the total number of users contained in
		/// the last search.
		/// </summary>
		private int TotalUsers
		{
			get { return ( int )ViewState[ "TotalUsers" ]; }
			set { ViewState[ "TotalUsers" ] = value; }
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			// Show the proper tab.
			Control body = FindControl( "ifolders" );
			switch ( Operation )
			{
				case PageOp.CreateiFolder:
				{
					// Create ifolder is called from both the userdetails page and the main ifolder list
					// page. Need to determine which one we came from.
					Uri uri = new Uri( ReferringPage );
					if ( uri.AbsolutePath.EndsWith( "UserDetails.aspx" ) )
					{
						TopNav.AddBreadCrumb( GetString( "USERS" ), "Users.aspx" );
						TopNav.AddBreadCrumb( FullName, String.Format( "UserDetails.aspx?id={0}", iFolderOwner ) );
						TopNav.AddBreadCrumb( GetString( "CREATENEWIFOLDER" ), null );

						if ( body != null )
						{
							body.ID = "users";
						}

						// Add the missing href to the ifolder tab and remove the user one.
						TopNav.SetActivePageTab( TopNavigation.PageTabs.Users );
					}
					else
					{
						TopNav.AddBreadCrumb( GetString( "IFOLDERS" ), "iFolders.aspx" );
						TopNav.AddBreadCrumb( GetString( "CREATENEWIFOLDER" ), null );
					}
					break;
				}

				case PageOp.AddMember:
				{
					TopNav.AddBreadCrumb( GetString( "IFOLDERS" ), "iFolders.aspx" );
					TopNav.AddBreadCrumb( iFolderName, String.Format( "iFolderDetailsPage.aspx?id={0}", iFolderID ) );
					TopNav.AddBreadCrumb( GetString( "ADDMEMBERS" ), null );
					break;
				}

				case PageOp.AddAdmin:
				{
					TopNav.AddBreadCrumb( GetString( "SYSTEM" ), "SystemInfo.aspx" );
					TopNav.AddBreadCrumb( GetString( "ADDADMINS" ), null );

					if ( body != null )
					{
						body.ID = "system";
					}

					// Add the missing href to the ifolder tab and remove the user one.
					TopNav.SetActivePageTab( TopNavigation.PageTabs.System );
					break;
				}
			}
		}

		/// <summary>
		/// Creates a list of existing admins.
		/// </summary>
		/// <returns></returns>
		private Hashtable CreateExistingAdminList()
		{
			int total;
			iFolderUser[] adminList = web.GetAdministrators( 0, 0, out total );
			Hashtable ht = new Hashtable( total );
			foreach( iFolderUser admin in adminList )
			{
				ht[ admin.ID ] = new MemberInfo( admin.ID, admin.UserName, admin.FullName );
			}

			return ht;
		}

		/// <summary>
		/// Creates a list of existing members.
		/// </summary>
		/// <returns></returns>
		private Hashtable CreateExistingMemberList()
		{
			int total;
			iFolderUser[] memberList = web.GetMembers( iFolderID, 0, 0, out total );
			Hashtable ht = new Hashtable( total );
			foreach( iFolderUser member in memberList )
			{
				ht[ member.ID ] = new MemberInfo( member.ID, member.UserName, member.FullName );
			}

			return ht;
		}

		/// <summary>
		/// Creates a DataSource containing user names from a search.
		/// </summary>
		/// <returns>An DataView object containing the ifolder users.</returns>
		private DataView CreateMemberList()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "EnabledField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );

			int total;
			iFolderUser[] userList = web.GetUsersBySearch( 
				MemberSearch.SearchAttribute, 
				MemberSearch.SearchOperation, 
				( MemberSearch.SearchName == String.Empty ) ? "*" : MemberSearch.SearchName, 
				CurrentUserOffset, 
				MemberList.PageSize, 
				out total );

			foreach( iFolderUser user in userList )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = user.ID;
				dr[ 2 ] = !IsExistingMember( user.ID );
				dr[ 3 ] = user.UserName;
				dr[ 4 ] = user.FullName;

				dt.Rows.Add( dr );
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < MemberList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = false;
				dr[ 3 ] = String.Empty;
				dr[ 4 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotalUsers = total;

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// Creates a a list of existing members with only the owner specified.
		/// </summary>
		/// <returns></returns>
		private Hashtable CreateNewMemberList()
		{
			// Get the information about the new owner.
			iFolderUser owner = web.GetUser( iFolderOwner );
			Hashtable ht = new Hashtable();
			ht[ owner.ID ] = new MemberInfo( owner.ID, owner.UserName, owner.FullName );
			return ht;
		}

		/// <summary>
		/// Returns whether the specified user is an existing member of
		/// the current ifolder.
		/// </summary>
		/// <param name="userID">User ID</param>
		/// <returns>True if the user is an existing member.</returns>
		private bool IsExistingMember( string userID )
		{
			return ExistingMemberList.ContainsKey( userID );
		}

		/// <summary>
		/// Returns whether the specified user is in the selected list.
		/// </summary>
		/// <param name="userID">User ID</param>
		/// <returns>True if user is in the selected list.</returns>
		private bool IsUserSelected( string userID )
		{
			return MembersToAdd.ContainsKey( userID ) ? true : IsExistingMember( userID );
		}

		/// <summary>
		/// Event handler that is called when a data grid item is bound.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MemberSelect_MemberItemDataBound( object sender, DataGridItemEventArgs e )
		{
			if ( ( e.Item.ItemType == ListItemType.AlternatingItem ) || ( e.Item.ItemType == ListItemType.Item ) )
			{
				// Check for any rows that are not supposed to be displayed and disable the image.
				// All of the other cells should contain empty strings.
				DataTable dt = ( MemberList.DataSource as DataView ).Table;
				if ( ( bool )dt.Rows[ e.Item.DataSetIndex ][ "VisibleField" ] == true )
				{
					if ( ( bool )dt.Rows[ e.Item.DataSetIndex ][ "EnabledField" ] == false )
					{
						e.Item.Cells[ Member_UserNameCell ].Enabled = false;
						e.Item.Cells[ Member_UserNameCell ].Attributes.Add( "class", "disableditem3" );

						e.Item.Cells[ Member_FullNameCell ].Enabled = false;
						e.Item.Cells[ Member_FullNameCell ].Attributes.Add( "class", "disableditem4" );
					}
					else
					{
						e.Item.Cells[ Member_UserNameCell ].Attributes.Add( "class", "memberitem3" );
						e.Item.Cells[ Member_FullNameCell ].Attributes.Add( "class", "memberitem4" );
					}
				}
			}
		}

		/// <summary>
		/// Page load event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load( object sender, System.EventArgs e )
		{
			// connection
			web = Session[ "Connection" ] as iFolderAdmin;

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				BackButton.Text = GetString( "BACK" );
				CancelButton.Text = GetString( "CANCEL" );

				switch ( Operation )
				{
					case PageOp.CreateiFolder:
					{
						// Initialize state variables.
						if ( MembersToAdd == null )
						{
							MembersToAdd = new Hashtable();
						}

						// Initialize localized fields.
						HeaderTitle.Text = String.Format( GetString( "ADDMEMBERSTOIFOLDER" ), iFolderName );
						SubHeaderTitle.Text = String.Format( GetString( "IFOLDERISOWNEDBY" ), FullName );
						OkButton.Text = GetString( "CREATE" );

						// Remember the page that we came from.
						string param = Request.Params[ "ref" ];
						ReferringPage = ( ( param == null ) || ( param == String.Empty ) ) ? 
							Page.Request.UrlReferrer.ToString() : param;

						// Create an existing member list.
						ExistingMemberList = CreateNewMemberList();
						break;
					}

					case PageOp.AddMember:
					{
						// Initialize state variables.
						MembersToAdd = new Hashtable();

						// Initialize localized fields.
						HeaderTitle.Text = String.Format( GetString( "ADDMEMBERSTOIFOLDER" ), iFolderName );
						SubHeaderTitle.Text = String.Format( GetString( "IFOLDERISOWNEDBY" ), FullName );
						OkButton.Text = GetString( "ADD" );

						// Hide the back button.
						BackButton.Visible = false;
						
						// Remember the page that we came from.
						ReferringPage = Page.Request.UrlReferrer.ToString();

						// Create an existing member list.
						ExistingMemberList = CreateExistingMemberList();
						break;
					}

					case PageOp.AddAdmin:
					{
						// Initialize state variables.
						MembersToAdd = new Hashtable();

						// Initialize localized fields.
						HeaderTitle.Text = GetString( "ADDADMINS" );
						SubHeaderTitle.Visible = false;
						OkButton.Text = GetString( "ADD" );

						// Hide the back button.
						BackButton.Visible = false;
						
						// Remember the page that we came from.
						ReferringPage = Page.Request.UrlReferrer.ToString();

						// Create an existing member list.
						ExistingMemberList = CreateExistingAdminList();
						break;
					}
				}

				// Initialize state variables.
				CurrentUserOffset = 0;
				TotalUsers = 0;
			}
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender(object sender, EventArgs e)
		{
			// Initially populate the member list.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();
			SetMemberPageButtonState();

			// Build the breadcrumb list.
			BuildBreadCrumbList();
		}

		/// <summary>
		/// Sets the page button state of the Member list.
		/// </summary>
		private void SetMemberPageButtonState()
		{
			MemberListFooter.SetPageButtonState( 
				MemberList, 
				CurrentUserOffset, 
				TotalUsers, 
				GetString( "USERS" ),
				GetString( "USER" ) );
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when all of the members in the current view
		/// are to be checked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void AllMembersChecked( Object sender, EventArgs e )
		{
			CheckBox checkBox = sender as CheckBox;
			foreach( DataGridItem item in MemberList.Items )
			{
				string userID = item.Cells[ Member_IDCell ].Text;
				if ( userID != "&nbsp;" && !IsExistingMember( userID ) )
				{
					if ( checkBox.Checked )
					{
						MembersToAdd[ userID ] = 
							new MemberInfo( 
								userID, 
								item.Cells[ Member_UserNameCell ].Text, 
								item.Cells[ Member_FullNameCell ].Text );
					}
					else
					{
						// Remove this member from the list.
						MembersToAdd.Remove( userID );
					}
				}
			}

			// Rebind the data source with the new data.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();
		}

		/// <summary>
		/// Event handler for the back button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BackButton_Clicked( Object sender, EventArgs e )
		{
			Uri uri = new Uri( ReferringPage );
			if ( uri.AbsolutePath.EndsWith( "UserDetails.aspx" ) )
			{
				// Return back to the referring page.
				Page.Response.Redirect( 
					String.Format( 
					"CreateiFolder.aspx?&owner={0}&fn={1}&name={2}&desc={3}&ref={4}", 
					iFolderOwner,
					FullName,
					iFolderName, 
					iFolderDescription,
					ReferringPage ), 
					true );
			}
			else
			{
				// Return back to the referring page.
				Page.Response.Redirect( 
					String.Format( 
					"OwnerSelect.aspx?&name={0}&desc={1}&owner={2}&pg={3}&ref={4}", 
					iFolderName, 
					iFolderDescription,
					iFolderOwner,
					MemberListPage,
					ReferringPage ), 
					true );
			}
		}

		/// <summary>
		/// Event handler for the cancel button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void CancelButton_Clicked( Object sender, EventArgs e )
		{
			// Return back to the referring page.
			Page.Response.Redirect( ReferringPage, true );
		}

		/// <summary>
		/// Returns the checked state for the specified member.
		/// </summary>
		/// <param name="userID">ID of the user</param>
		/// <returns>True if user is to be added.</returns>
		protected bool GetMemberCheckedState( Object userID )
		{
			return MembersToAdd.ContainsKey( userID ) ? true : IsUserSelected( userID as String );
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key">Key to the localized string.</param>
		/// <returns>Localized string.</returns>
		protected string GetString( string key )
		{
			return rm.GetString( key );
		}

		/// <summary>
		/// Event handler that gets called when a user in the member list's check
		/// box changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MemberChecked( Object sender, EventArgs e )
		{
			// Get the data grid row for this user.
			CheckBox checkBox = sender as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string userID = item.Cells[ Member_IDCell ].Text;
			if ( userID != "&nbsp;" )
			{
				// User is being added.
				if ( checkBox.Checked )
				{
					MembersToAdd[ userID ] = 
						new MemberInfo( 
							userID, 
							item.Cells[ Member_UserNameCell ].Text, 
							item.Cells[ Member_FullNameCell ].Text );
				}
				else
				{
					// Remove this member from the list.
					MembersToAdd.Remove( userID );
				}
			}
		}

		/// <summary>
		/// Event handler for the PageFirstButton.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void Member_PageFirstButton_Click( object source, ImageClickEventArgs e )
		{
			// Set to get the first users.
			CurrentUserOffset = 0;

			// Rebind the data source with the new data.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();

			// Set the button state.
			SetMemberPageButtonState();

			// Reset the all checked box.
			AllMembersCheckBox.Checked = false;
		}

		/// <summary>
		/// Event that first when the MbrPageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void Member_PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset += MemberList.PageSize;

			// Rebind the data source with the new data.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();

			// Set the button state.
			SetMemberPageButtonState();

			// Reset the all checked box.
			AllMembersCheckBox.Checked = false;
		}

		/// <summary>
		/// Event that first when the MbrPageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void Member_PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset =  ( ( TotalUsers - 1 ) / MemberList.PageSize ) * MemberList.PageSize;

			// Rebind the data source with the new data.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();

			// Set the button state.
			SetMemberPageButtonState();

			// Reset the all checked box.
			AllMembersCheckBox.Checked = false;
		}

		/// <summary>
		/// Event that first when the MbrPagePreviousButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void Member_PagePreviousButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset -= MemberList.PageSize;
			if ( CurrentUserOffset < 0 )
			{
				CurrentUserOffset = 0;
			}

			// Rebind the data source with the new data.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();

			// Set the button state.
			SetMemberPageButtonState();

			// Reset the all checked box.
			AllMembersCheckBox.Checked = false;
		}

		/// <summary>
		/// Event handler for the ok button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OkButton_Clicked( Object sender, EventArgs e )
		{
			// See which operation needs to be performed based on the query string.
			switch ( Operation )
			{
				case PageOp.CreateiFolder:
				{
					// Create the iFolder.
					iFolder ifolder = null;
					try
					{
						ifolder = web.CreateiFolder( iFolderName, iFolderOwner, iFolderDescription );
					}
					catch ( Exception ex )
					{
						// Clear out the member list because it is saved on the session.
						MembersToAdd.Clear();
						MembersToAdd = null;

						TopNav.ShowError( GetString( "ERRORCANNOTCREATEIFOLDER" ), ex );
						return;
					}

					// Add the selected users to the ifolder.
					foreach( MemberInfo mi in MembersToAdd.Values )
					{
						// Check to see if this user is already a member.
						if ( !IsExistingMember( mi.UserID ) )
						{
							try
							{
								web.AddMember( ifolder.ID, mi.UserID, Rights.ReadOnly );
							}
							catch ( Exception ex )
							{
								// Clear out the member list because it is saved on the session.
								MembersToAdd.Clear();
								MembersToAdd = null;

								string errMsg = String.Format( GetString( "ERRORCANNOTADDMEMBER" ), mi.UserName, iFolderName );
								TopNav.ShowError( errMsg, ex );
								return;
							}
						}
					}

					// Clear out the member list because it is saved on the session.
					MembersToAdd.Clear();
					MembersToAdd = null;

					Page.Response.Redirect( String.Format( "iFolderDetailsPage.aspx?id={0}", ifolder.ID ), true );
					break;
				}

				case PageOp.AddMember:
				{
					// Add the selected users to the ifolder.
					string id = iFolderID;
					foreach( MemberInfo mi in MembersToAdd.Values )
					{
						// Check to see if this user is already a member.
						if ( !IsExistingMember( mi.UserID ) )
						{
							try
							{
								web.AddMember( id, mi.UserID, Rights.ReadOnly );
							}
							catch ( Exception ex )
							{
								// Clear out the member list because it is saved on the session.
								MembersToAdd.Clear();
								MembersToAdd = null;

								string errMsg = String.Format( GetString( "ERRORCANNOTADDMEMBER" ), mi.UserName, iFolderName );
								TopNav.ShowError( errMsg, ex );
								return;
							}
						}
					}

					// Clear out the member list because it is saved on the session.
					MembersToAdd.Clear();
					MembersToAdd = null;
					break;
				}

				case PageOp.AddAdmin:
				{
					// Add the selected users as admins.
					foreach( MemberInfo mi in MembersToAdd.Values )
					{
						// Check to see if this user is already a member.
						if ( !IsExistingMember( mi.UserID ) )
						{
							try
							{
								web.AddAdministrator( mi.UserID );
							}
							catch( Exception ex )
							{
								// Clear out the member list because it is saved on the session.
								MembersToAdd.Clear();
								MembersToAdd = null;

								string errMsg = String.Format( GetString( "ERRORCANNOTADDADMIN" ), mi.UserName );
								TopNav.ShowError( errMsg, ex );
								return;
							}
						}
					}

					// Clear out the member list because it is saved on the session.
					MembersToAdd.Clear();
					MembersToAdd = null;
					break;
				}
			}
			
			Page.Response.Redirect( ReferringPage, true );
		}

		/// <summary>
		/// SearchButton_Click
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void SearchButton_Click( object source, EventArgs e)
		{
			// Always search from the beginning.
			CurrentUserOffset = 0;
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();
			SetMemberPageButtonState();

			// Reset the all checked box.
			AllMembersCheckBox.Checked = false;
		}

		#endregion

		#region Web Form Designer generated code

		/// <summary>
		/// OnInit
		/// </summary>
		/// <param name="e"></param>novell
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		/// this.Load += new System.EventHandler( this.Page_Load );
		private void InitializeComponent()
		{
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			MemberSearch.Click += new System.EventHandler( SearchButton_Click );

			MemberList.ItemDataBound += new DataGridItemEventHandler( MemberSelect_MemberItemDataBound );

			MemberListFooter.PageFirstClick += new ImageClickEventHandler( Member_PageFirstButton_Click );
			MemberListFooter.PagePreviousClick += new ImageClickEventHandler( Member_PagePreviousButton_Click );
			MemberListFooter.PageNextClick += new ImageClickEventHandler( Member_PageNextButton_Click );
			MemberListFooter.PageLastClick += new ImageClickEventHandler( Member_PageLastButton_Click );

			this.Load += new System.EventHandler( this.Page_Load );
		}
		#endregion

		#region MemberInfo Class

		/// <summary>
		/// Class used to hold intermediate information when a user
		/// is selected to be added.
		/// </summary>
		[ Serializable() ]
		private class MemberInfo : IComparable
		{
			#region Class Members

			/// <summary>
			/// ID of the user.
			/// </summary>
			public string UserID;

			/// <summary>
			/// Name of the user.
			/// </summary>
			public string UserName;

			/// <summary>
			/// Full name of the user.
			/// </summary>
			public string FullName;

			#endregion

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="userID">ID of the user</param>
			/// <param name="userName">Name of the user</param>
			/// <param name="fullName">Full name of the user</param>
			public MemberInfo( string userID, string userName, string fullName )
			{
				UserID = userID;
				UserName = userName;
				FullName = fullName;
			}

			#endregion

			#region IComparable Members

			/// <summary>
			/// Compares the current instance with another object of the same type.
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public int CompareTo( object obj )
			{
				return String.Compare( UserName, ( obj as MemberInfo ).UserName, true );
			}

			#endregion
		}

		#endregion
	}
}
