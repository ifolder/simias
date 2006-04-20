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



		/// <summary>
		/// Control that contains the create ifolder controls.
		/// </summary>
		protected HtmlGenericControl CreateiFolderDiv;

		/// <summary>
		/// Control that specifies the new ifolder label.
		/// </summary>
		protected Label NameLabel;

		/// <summary>
		/// Control that gets the name for the new ifolder.
		/// </summary>
		protected TextBox Name;

		/// <summary>
		/// Control that specifies the new ifolder description.
		/// </summary>
		protected Label DescriptionLabel;

		/// <summary>
		/// Control that gets the description for the new ifolder.
		/// </summary>
		protected HtmlTextArea Description;

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
		/// Gets or sets the existing members list.
		/// </summary>
		private Hashtable ExistingMemberList
		{
			get { return ViewState[ "ExistingMembers" ] as Hashtable; }
			set { ViewState[ "ExistingMembers" ] = value; }
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
		/// Gets the owner of the iFolder.
		/// </summary>
		private string Owner
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
			get { return ViewState[ "MembersToAdd" ] as Hashtable; }
			set { ViewState[ "MembersToAdd" ] = value; }
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
						iFolderUser user = web.GetUser( Owner );
						TopNav.AddBreadCrumb( GetString( "USERS" ), "Users.aspx" );
						TopNav.AddBreadCrumb( user.FullName, String.Format( "UserDetails.aspx?id={0}", user.ID ) );
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
					iFolder ifolder = web.GetiFolder( iFolderID );
					TopNav.AddBreadCrumb( GetString( "IFOLDERS" ), "iFolders.aspx" );
					TopNav.AddBreadCrumb( ifolder.Name, String.Format( "iFolderDetailsPage.aspx?id={0}", ifolder.ID ) );
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

			iFolderUser[] userList;
			int total;
			SearchProperty attribute = MemberSearch.SearchAttribute;

			if ( MemberSearch.SearchName == String.Empty )
			{
				userList = web.GetUsersBySearch( 
					attribute, 
					MemberSearch.SearchOperation, 
					"*", 
					CurrentUserOffset, 
					MemberList.PageSize, 
					out total );
			}
			else
			{
				userList = web.GetUsersBySearch( 
					attribute, 
					MemberSearch.SearchOperation, 
					MemberSearch.SearchName, 
					CurrentUserOffset, 
					MemberList.PageSize, 
					out total );
			}

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
			iFolderUser owner = web.GetUser( Owner );
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
				// Remember the page that we came from.
				ReferringPage = Page.Request.UrlReferrer.ToString();

				// Initialize the localized fields.
				OkButton.Text = GetString( "OK" );
				CancelButton.Text = GetString( "CANCEL" );
				NameLabel.Text = GetString( "NAMETAG" );
				DescriptionLabel.Text = GetString( "DESCRIPTIONTAG" );

				switch ( Operation )
				{
					case PageOp.CreateiFolder:
					{
						CreateiFolderDiv.Visible = true;
						ExistingMemberList = CreateNewMemberList();
						SetFocus( Name );
						break;
					}

					case PageOp.AddMember:
					{
						CreateiFolderDiv.Visible = false;
						ExistingMemberList = CreateExistingMemberList();
						break;
					}

					case PageOp.AddAdmin:
					{
						CreateiFolderDiv.Visible = false;
						ExistingMemberList = CreateExistingAdminList();
						break;
					}
				}

				// Initialize state variables.
				Description.Value = String.Empty;
				MembersToAdd = new Hashtable();
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
		/// Sets focus to the specified control.
		/// </summary>
		/// <param name="ctrl"></param>
		private void SetFocus( System.Web.UI.Control ctrl )
		{
			string s = "<SCRIPT language='javascript'>document.getElementById('" + ctrl.ClientID + "').focus() </SCRIPT>";
			Page.RegisterStartupScript( "focus", s );
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

			// Enable the okay button if an ifolder is not being created.
			if ( Operation != PageOp.CreateiFolder )
			{
				OkButton.Enabled = HasMembersToAdd;
			}

			// Rebind the data source with the new data.
			MemberList.DataSource = CreateMemberList();
			MemberList.DataBind();
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

			// Enable the okay button if an ifolder is not being created.
			if ( Operation != PageOp.CreateiFolder )
			{
				OkButton.Enabled = HasMembersToAdd;
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
					iFolder ifolder = web.CreateiFolder( Name.Text, Owner, Description.Value );

					// Add the selected users to the ifolder.
					foreach( MemberInfo mi in MembersToAdd.Values )
					{
						// Check to see if this user is already a member.
						if ( !IsExistingMember( mi.UserID ) )
						{
							web.AddMember( ifolder.ID, mi.UserID, Rights.ReadOnly );
						}
					}
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
							web.AddMember( id, mi.UserID, Rights.ReadOnly );
						}
					}
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
							web.AddAdministrator( mi.UserID );
						}
					}
					break;
				}
			}
			
			Page.Response.Redirect( ReferringPage, true );
		}

		/// <summary>
		/// Event handler that gets called when the ifolder name text changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnNameChanged( Object sender, EventArgs e )
		{
			// Enable the ok button.
			OkButton.Enabled = true;
			SetFocus( Description );
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
