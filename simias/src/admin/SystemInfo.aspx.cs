/***********************************************************************
 *  $RCSfile: System.aspx.cs,v $
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
using System.Resources;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for System.
	/// </summary>
	public class SystemInfo : System.Web.UI.Page
	{
		#region Class Members
		
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
		/// iFolder system name control.
		/// </summary>
		protected TextBox Name;

		/// <summary>
		/// iFolder system description control.
		/// </summary>
		protected HtmlTextArea Description;

		/// <summary>
		/// Number of users control.
		/// </summary>
		protected Literal NumberOfUsers;

		/// <summary>
		/// Number of ifolders control.
		/// </summary>
		protected Literal NumberOfiFolders;

		/// <summary>
		/// System policy control.
		/// </summary>
		protected Policy Policy;

		
		/// <summary>
		/// All admins checkbox control.
		/// </summary>
		protected CheckBox AllAdminsCheckBox;

		/// <summary>
		/// Admin list control.
		/// </summary>
		protected DataGrid AdminList;

		/// <summary>
		/// Admin list footer control.
		/// </summary>
		protected ListFooter AdminListFooter;

		/// <summary>
		/// Delete admin button control.
		/// </summary>
		protected Button DeleteButton;

		/// <summary>
		/// Add admin button control.
		/// </summary>
		protected Button AddButton;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the members that are checked in the list.
		/// </summary>
		private Hashtable CheckedMembers
		{
			get { return ViewState[ "CheckedMembers" ] as Hashtable; }
			set { ViewState[ "CheckedMembers" ] = value; }
		}

		/// <summary>
		/// Gets or sets the current admin offset.
		/// </summary>
		private int CurrentAdminOffset
		{
			get { return ( int )ViewState[ "CurrentAdminOffset" ]; }
			set { ViewState[ "CurrentAdminOffset" ] = value; }
		}

		/// <summary>
		/// Gets or sets the super admin ID.
		/// </summary>
		private string SuperAdminID
		{
			get { return ViewState[ "SuperAdminID" ] as string; }
			set { ViewState[ "SuperAdminID" ] = value; }
		}

		/// <summary>
		/// Gets or sets the total number of admins contained in
		/// the last search.
		/// </summary>
		private int TotalAdmins
		{
			get { return ( int )ViewState[ "TotalAdmins" ]; }
			set { ViewState[ "TotalAdmins" ] = value; }
		}

		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			TopNav.AddBreadCrumb( GetString( "SYSTEM" ), null );
		}

		/// <summary>
		/// Gets the list view of ifolder administrators.
		/// </summary>
		/// <returns></returns>
		private DataView CreateDataSource()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );

			int total;
			iFolderUser[] adminList = web.GetAdministrators( CurrentAdminOffset, AdminList.PageSize, out total );

			foreach( iFolderUser admin in adminList )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = admin.ID;
				dr[ 2 ] = admin.UserName;
				dr[ 3 ] = admin.FullName;

				dt.Rows.Add( dr );
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < AdminList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = String.Empty;
				dr[ 3 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotalAdmins = total;

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// GetiFolderAdminList
		/// </summary>
		private void GetiFolderAdminList()
		{
			AdminList.DataSource = CreateDataSource();
			AdminList.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Gets the displayable ifolder system information.
		/// </summary>
		private void GetSystemInformation()
		{
			int totaliFolders;
			int totalUsers;

			iFolderSystem system = web.GetSystem();
			Name.Text = system.Name;
			Description.Value = system.Description;

			iFolderUser[] users = web.GetUsers( 0, 1, out totalUsers );
			NumberOfUsers.Text = totalUsers.ToString();

			iFolder[] folders = web.GetiFolders( iFolderType.All, 0, 1, out totaliFolders );
			NumberOfiFolders.Text = totaliFolders.ToString();
		}

		/// <summary>
		/// Returns whether the specified ID is the super admin.
		/// </summary>
		/// <param name="id">User ID.</param>
		/// <returns>True if User ID is super admin, otherwise false is returned.</returns>
		private bool IsSuperAdmin( string id )
		{
			return ( SuperAdminID == id ) ? true : false;
		}

		/// <summary>
		/// Page_Load
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
				DeleteButton.Text = GetString( "DELETE" );
				AddButton.Text = GetString( "ADD" );

				// Initialize state variables.
				CurrentAdminOffset = 0;
				TotalAdmins = 0;
				AllAdminsCheckBox.Checked = false;
				CheckedMembers = new Hashtable();

				// Get the owner of the system.
				iFolder domain = web.GetiFolder( web.GetSystem().ID );
				SuperAdminID = domain.OwnerID;
			}
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void Page_PreRender( object source, EventArgs e )
		{
			// Show the ifolder system information.
			GetSystemInformation();

			// Get the list of ifolder admins.
			GetiFolderAdminList();

			// Get the policy information.
			Policy.GetSystemPolicies();

			// Build the bread crumb list.
			BuildBreadCrumbList();
		}

		/// <summary>
		/// Sets the page button state of the admin list.
		/// </summary>
		private void SetPageButtonState()
		{
			AdminListFooter.SetPageButtonState( 
				AdminList,
				CurrentAdminOffset, 
				TotalAdmins, 
				GetString( "ADMINISTRATORS" ),
				GetString( "ADMINISTRATOR" ) );
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Gets whether the admin is checked.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected bool GetAdminCheckedState( object id )
		{
			return CheckedMembers.ContainsKey( id ) ? true : false;
		}

		/// <summary>
		/// Gets whether the checkbox should be enabled for this admin user.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		protected bool GetAdminEnabledState( object id )
		{
			return !IsSuperAdmin( id as string );
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString( string key )
		{
			return rm.GetString( key );
		}

		/// <summary>
		/// Event handler that gets called when the add admin button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAddButton_Click( object source, EventArgs e )
		{
			Response.Redirect( "MemberSelect.aspx?op=addadmin", true );
		}

		/// <summary>
		/// Event handler that gets called when the admin checkbox is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAdminChecked( object source, EventArgs e )
		{
			// Get the data grid row for this member.
			CheckBox checkBox = source as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string memberID = item.Cells[ 0 ].Text;
			if ( memberID != "&nbsp;" )
			{
				// Member is being added.
				if ( checkBox.Checked )
				{
					CheckedMembers[ memberID ] = item.Cells[ 4 ].Text;
				}
				else
				{
					// Remove this member from the list.
					CheckedMembers.Remove( memberID );
				}
			}

			// See if there are any checked members.
			DeleteButton.Enabled = ( CheckedMembers.Count > 0 );
		}

		/// <summary>
		/// Event handler that gets called when the all admins checkbox is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAllAdminsChecked( object source, EventArgs e )
		{
			CheckBox checkBox = source as CheckBox;
			foreach( DataGridItem item in AdminList.Items )
			{
				// In order to be checked, the row must not be empty.
				string memberID = item.Cells[ 0 ].Text;
				if ( memberID != "&nbsp;" )
				{
					if ( !IsSuperAdmin( memberID ) )
					{
						if ( checkBox.Checked )
						{
							CheckedMembers[ memberID ] = item.Cells[ 4 ].Text;
						}
						else
						{
							// Remove this member from the list.
							CheckedMembers.Remove( memberID );
						}
					}
				}
			}

			// See if there are any checked members.
			DeleteButton.Enabled = ( CheckedMembers.Count > 0 );

			// Rebind the data source with the new data.
			GetiFolderAdminList();
		}

		/// <summary>
		/// Event handler that gets called when the delete admin button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDeleteButton_Click( object source, EventArgs e )
		{
			foreach( string memberID in CheckedMembers.Keys )
			{
				web.RemoveAdministrator( memberID );
			}

			// Clear the checked members.
			CheckedMembers.Clear();
			AllAdminsCheckBox.Checked = false;

			// Disable the action buttons.
			DeleteButton.Enabled = false;

			// Rebind the data source with the new data.
			GetiFolderAdminList();
		}

		/// <summary>
		/// Event that first when the PageFirstButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageFirstButton_Click( object source, ImageClickEventArgs e)
		{
			// Set to get the first admins.
			CurrentAdminOffset = 0;
			GetiFolderAdminList();
		}

		/// <summary>
		/// Event that first when the PagePreviousButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PagePreviousButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentAdminOffset -= AdminList.PageSize;
			if ( CurrentAdminOffset < 0 )
			{
				CurrentAdminOffset = 0;
			}

			GetiFolderAdminList();
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentAdminOffset += AdminList.PageSize;
			GetiFolderAdminList();
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentAdminOffset = ( ( TotalAdmins - 1 ) / AdminList.PageSize ) * AdminList.PageSize;
			GetiFolderAdminList();
		}

		#endregion

		#region Web Form Designer generated code
		/// <summary>
		/// OnInit
		/// </summary>
		/// <param name="e"></param>
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
		private void InitializeComponent()
		{    
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			AdminListFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			AdminListFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			AdminListFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			AdminListFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
