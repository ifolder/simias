/***********************************************************************
 *  $RCSfile: iFolderDetailsPage.aspx.cs,v $
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
using System.IO;
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
	/// Summary description for iFolderDetailsPage.
	/// </summary>
	public class iFolderDetailsPage : System.Web.UI.Page
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
		/// Controls used to display and edit iFolder information.
		/// </summary>
		protected Literal Name;

		/// <summary>
		/// Controls used to display and edit iFolder information.
		/// </summary>
		protected TextBox Description;

		/// <summary>
		/// Control used to save the changed description.
		/// </summary>
		protected Button DescriptionButton;

		/// <summary>
		/// Controls used to display and edit iFolder information.
		/// </summary>
		protected Table iFolderInfoTable;

		/// <summary>
		/// Controls used to display and edit iFolder information.
		/// </summary>
		protected HyperLink Owner;

		/// <summary>
		/// Controls used to display and edit iFolder information.
		/// </summary>
		protected Literal LastModified;

		/// <summary>
		/// Controls used to display and edit iFolder information.
		/// </summary>
		protected Literal Size;

		/// <summary>
		/// Controls used to display and edit iFolder information.
		/// </summary>
		protected Literal Directories;

		/// <summary>
		/// Controls used to display and edit iFolder information.
		/// </summary>
		protected Literal Files;

		/// <summary>
		/// Controls used to display and edit iFolder information.
		/// </summary>
		protected Literal UnManagedPath;


		/// <summary>
		/// Control used to display iFolder members.
		/// </summary>
		protected DataGrid iFolderMemberList;

		/// <summary>
		/// Control that implements the paging of the member list.
		/// </summary>
		protected ListFooter iFolderMemberListFooter;


		/// <summary>
		/// Control that deletes all checked members.
		/// </summary>
		protected Button MemberDeleteButton;

		/// <summary>
		/// Control that adds members to the ifolder.
		/// </summary>
		protected Button MemberAddButton;

		/// <summary>
		/// Control that sets the rights for members.
		/// </summary>
		protected DropDownList MemberRightsList;

		/// <summary>
		/// Control that applies the membership changes.
		/// </summary>
		protected Button MemberRightsButton;

		/// <summary>
		/// Control that applies ifolder owner changes.
		/// </summary>
		protected Button MemberOwnerButton;


		/// <summary>
		/// iFolder user policy control.
		/// </summary>
		protected Policy Policy;

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
		/// Gets or sets the current ifolder offset.
		/// </summary>
		private int CurrentMemberOffset
		{
			get { return ( int )ViewState[ "CurrentMemberOffset" ]; }
			set { ViewState[ "CurrentMemberOffset" ] = value; }
		}

		/// <summary>
		/// Enables or disables the member actions buttons.
		/// </summary>
		private bool EnableMemberActionButtons
		{
			set 
			{ 
				MemberRightsList.Enabled = 
					MemberRightsButton.Enabled = 
					MemberDeleteButton.Enabled =  value;
			}
		}

		/// <summary>
		/// Enables or disables the owner action button.
		/// </summary>
		private bool EnableOwnerActionButton
		{
			set { MemberOwnerButton.Enabled = value; }
		}

		/// <summary>
		/// Gets the iFolder ID.
		/// </summary>
		private string iFolderID
		{
			get { return Request.Params[ "ID" ]; } 
		}

		/// <summary>
		/// Gets or sets whether the MemberCheckBox is checked.
		/// </summary>
		private bool MembersChecked
		{
			get { return ( bool )ViewState[ "MembersChecked" ]; }
			set { ViewState[ "MembersChecked" ] = value; }
		}

		/// <summary>
		/// Gets or sets the total number of members in the currently displaying
		/// iFolder.
		/// </summary>
		private int TotaliFolderMembers
		{
			get { return ( int )ViewState[ "TotaliFolderMembers" ]; }
			set { ViewState[ "TotaliFolderMembers" ] = value; }
		}

		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		/// <param name="ifolderName">The name of the current ifolder.</param>
		private void BuildBreadCrumbList( string ifolderName )
		{
			TopNav.AddBreadCrumb( GetString( "IFOLDERS" ), "iFolders.aspx" );
			TopNav.AddBreadCrumb( ifolderName, null );
		}

		/// <summary>
		/// Initializes the iFolder detail web controls.
		/// </summary>
		///	<returns>The name of the ifolder.</returns>
		private string GetiFolderDetails()
		{
			iFolderDetails ifolder = web.GetiFolderDetails( iFolderID );
			Name.Text = ifolder.Name;
			Description.Text = ifolder.Description;
			Owner.Text = ifolder.OwnerFullName;
			Owner.NavigateUrl= String.Format( "UserDetails.aspx?id={0}", ifolder.OwnerID );
			Size.Text = Utils.ConvertToUnitString( ifolder.Size, true, rm );
			Directories.Text = ifolder.DirectoryCount.ToString();
			Files.Text = ifolder.FileCount.ToString();

			LastModified.Text = ( ifolder.LastModified == DateTime.MinValue ) ? 
				DateTime.Now.ToString() : ifolder.LastModified.ToString();

			// Allow the browser to break up the path on separator boundries.
			UnManagedPath.Text = ifolder.UnManagedPath.Replace( 
				Path.DirectorySeparatorChar.ToString(), 
				Path.DirectorySeparatorChar.ToString() + "<WBR>" );

			return ifolder.Name;
		}

		/// <summary>
		/// Gets the member list view for the specified ifolder.
		/// </summary>
		/// <returns></returns>
		private DataView GetiFolderMemberList()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "OwnerField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "RightsField", typeof( string ) ) );

			iFolderUserSet memberList = 
				web.GetMembers( 
				iFolderID, 
				CurrentMemberOffset, 
				iFolderMemberList.PageSize );

			foreach( iFolderUser member in memberList.Items )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = member.IsOwner;
				dr[ 2 ] = member.ID;
				dr[ 3 ] = member.FullName;
				dr[ 4 ] = member.Rights.ToString();

				dt.Rows.Add( dr );
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < iFolderMemberList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = false;
				dr[ 2 ] = String.Empty;
				dr[ 3 ] = String.Empty;
				dr[ 4 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotaliFolderMembers = memberList.Total;

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// Gets the iFolder members
		/// </summary>
		private void GetiFolderMembers()
		{
			iFolderMemberList.DataSource = GetiFolderMemberList();
			iFolderMemberList.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Gets the currently selected member rights.
		/// </summary>
		/// <returns>The mapped Rights</returns>
		private Rights GetSelectedMemberRights()
		{
			Rights rights;

			string attribute = MemberRightsList.SelectedValue;
			if ( attribute == GetString( "ADMIN" ) )
			{
				rights = Rights.Admin;
			}
			else if ( attribute == GetString( "READWRITE" ) )
			{
				rights = Rights.ReadWrite;
			}
			else
			{
				rights = Rights.ReadOnly;
			}

			return rights;
		}

		/// <summary>
		/// Event handler for when a datagrid item is bound.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void iFolderMemberList_DataGridItemBound( Object source, DataGridItemEventArgs e )
		{
			if ( ( e.Item.ItemType == ListItemType.Item ) || ( e.Item.ItemType == ListItemType.AlternatingItem ) )
			{
				// Check for any rows that are not supposed to be displayed and disable the image.
				// All of the other cells should contain empty strings.
				DataTable dt = ( iFolderMemberList.DataSource as DataView ).Table;
				if ( ( bool )dt.Rows[ e.Item.DataSetIndex ][ "VisibleField" ] == false )
				{
					( e.Item.Cells[ 0 ].FindControl( "iFolderMemberListCheckBox" ) as CheckBox ).Visible = false;
					( e.Item.Cells[ 1 ].FindControl( "UserImage" ) as System.Web.UI.WebControls.Image ).Visible = false;
				}

				// Disable the owner of the iFolder from being checked and removed from the ifolder.
				if ( ( bool )dt.Rows[ e.Item.DataSetIndex ][ "OwnerField" ] == true )
				{
					( e.Item.Cells[ 0 ].FindControl( "iFolderMemberListCheckBox" ) as CheckBox ).Enabled = false;
				}
			}
			else if ( e.Item.ItemType == ListItemType.Header )
			{
				// Set the all users checked state.
				( e.Item.FindControl( "MemberAllCheckBox" ) as CheckBox ).Checked = MembersChecked;
			}
		}

		/// <summary>
		/// Event handler that gets called if a policy error occurs.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void OnPolicyError( object source, PolicyErrorArgs e )
		{
			TopNav.ShowError( e.Message, e.Exception );
		}

		/// <summary>
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// connection
			web = Session[ "Connection" ] as iFolderAdmin;

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				iFolderMemberList.Columns[ 3 ].HeaderText = GetString( "TYPE" );
				iFolderMemberList.Columns[ 4 ].HeaderText = GetString( "NAME" );
				iFolderMemberList.Columns[ 5 ].HeaderText = GetString( "RIGHTS" );

				DescriptionButton.Text = GetString( "SAVE" );
				MemberDeleteButton.Text = GetString( "DELETE" );
				MemberAddButton.Text = GetString( "ADD" );
				MemberOwnerButton.Text = GetString( "OWNER" );
				MemberRightsButton.Text = GetString( "SET" );

				MemberRightsList.Items[ 0 ].Text = GetString( "READONLY" );
				MemberRightsList.Items[ 1 ].Text = GetString( "READWRITE" );
				MemberRightsList.Items[ 2 ].Text = GetString( "ADMIN" );

				// Initialize state variables.
				CurrentMemberOffset = 0;
				TotaliFolderMembers = 0;
				MembersChecked = false;
				CheckedMembers = new Hashtable();
			}
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender( object sender, EventArgs e )
		{
			// Get the iFolder Details.
			string ifolderName = GetiFolderDetails();

			// Show the iFolder member list.
			GetiFolderMembers();

			// Fill in the policy information.
			Policy.GetiFolderPolicies();

			// Build the breadcrumb list.
			BuildBreadCrumbList( ifolderName );
		}

		/// <summary>
		/// Sets the page button state of the ifolder list.
		/// </summary>
		private void SetPageButtonState()
		{
			iFolderMemberListFooter.SetPageButtonState( 
				iFolderMemberList, 
				CurrentMemberOffset, 
				TotaliFolderMembers, 
				GetString( "MEMBERS" ),
				GetString( "MEMBER" ) );

			MembersChecked = false;
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the add member button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void AddiFolderMembers( Object sender, EventArgs e )
		{
			Page.Response.Redirect( 
				String.Format( 
					"MemberSelect.aspx?op=addmember&id={0}&name={1}&desc={2}&fn={3}", 
					iFolderID, 
					Name.Text,
					Description.Text,
					Owner.Text), 
				true );
		}

		/// <summary>
		/// Event handler that gets called when the check all members checkbox is selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void AllMembersChecked( object sender, EventArgs e )
		{
			CheckBox checkBox = sender as CheckBox;
			foreach( DataGridItem item in iFolderMemberList.Items )
			{
				// In order to be checked, the row must not be empty or must not be
				// the owner of the ifolder.
				string memberID = item.Cells[ 0 ].Text;
				if ( ( memberID != "&nbsp;" ) && ( item.Cells[ 1 ].Text != "True" ) )
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

			// See if there are any checked members.
			EnableMemberActionButtons = ( CheckedMembers.Count > 0 );
			EnableOwnerActionButton = ( CheckedMembers.Count == 1 );
			MembersChecked = checkBox.Checked;

			// Rebind the data source with the new data.
			GetiFolderMembers();
		}

		/// <summary>
		/// Event handler that gets called when the set rights button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ChangeMemberRights( object sender, EventArgs e )
		{
			Rights rights = GetSelectedMemberRights();

			foreach( string memberID in CheckedMembers.Keys )
			{
				try
				{
					web.SetMemberRights( iFolderID, memberID, rights );
				}
				catch( Exception ex )
				{
					string memberName = CheckedMembers[ memberID ] as String;
					TopNav.ShowError( String.Format( GetString( "ERRORCANNOTCHANGERIGHTS" ), memberName ), ex );
					return;
				}
			}

			// Clear the checked members.
			CheckedMembers.Clear();
			MembersChecked = false;

			// Disable the action buttons.
			EnableMemberActionButtons = EnableOwnerActionButton = false;

			// Rebind the data source with the new data.
			GetiFolderMembers();
		}

		/// <summary>
		/// Event handler that gets called when the change owner button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ChangeOwner( object sender, EventArgs e )
		{
			// Get the checked user. There must only be one.
			IEnumerator enumerator = CheckedMembers.Keys.GetEnumerator();
			if ( enumerator.MoveNext() )
			{
				string memberID = enumerator.Current as string;
				try
				{
					web.SetiFolderOwner( iFolderID, memberID );
				}
				catch ( Exception ex )
				{
					TopNav.ShowError( GetString( "ERRORCANNOTCHANGEOWNER" ), ex );
					return;
				}

				// Show the new owner in the page.
				Owner.Text = CheckedMembers[ memberID ] as string;
				Owner.NavigateUrl = String.Format( "UserDetails.aspx?id={0}", memberID );
			}

			// Clear the checked members.
			CheckedMembers.Clear();
			MembersChecked = false;

			// Disable the action buttons.
			EnableMemberActionButtons = EnableOwnerActionButton = false;

			// Rebind the data source with the new data.
			GetiFolderMembers();
		}

		/// <summary>
		/// Event handler that gets called when the delete member button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void DeleteiFolderMembers( Object sender, EventArgs e )
		{
			foreach( string memberID in CheckedMembers.Keys )
			{
				try
				{
					web.RemoveMember( iFolderID, memberID );
				}
				catch ( Exception ex )
				{
					string memberName = CheckedMembers[ memberID ] as String;
					TopNav.ShowError( String.Format( GetString( "ERRORCANNOTDELETEMEMBERS" ), memberName ), ex );
					return;
				}
			}

			// Clear the checked members.
			CheckedMembers.Clear();
			MembersChecked = false;

			// Disable the action buttons.
			EnableMemberActionButtons = EnableOwnerActionButton = false;

			// Rebind the data source with the new data.
			GetiFolderMembers();
		}

		/// <summary>
		/// Event handler that gets called when the description text changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void DescriptionChanged( object sender, EventArgs e )
		{
			DescriptionButton.Enabled = true;
		}

		/// <summary>
		/// Gets the iFolder display name
		/// </summary>
		/// <returns>The display name of the current iFolder</returns>
		protected string GetiFolderName()
		{
			iFolder ifolder = web.GetiFolder( iFolderID );
			return ( ifolder != null ) ? ifolder.Name : String.Empty;
		}

		/// <summary>
		/// Returns the checked state for the specified member.
		/// </summary>
		/// <param name="memberID">ID of the member</param>
		/// <returns>True if user is to be added.</returns>
		protected bool GetMemberCheckedState( Object memberID )
		{
			return CheckedMembers.ContainsKey( memberID ) ? true : false;
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
		/// Gets the image url based on whether the member is an owner.
		/// </summary>
		/// <param name="isOwner"></param>
		/// <returns></returns>
		protected string GetUserImage( Object isOwner )
		{
			return ( bool )isOwner ? "images/ifolder_owner.gif" : "images/ifolder_user.gif";
		}

		/// <summary>
		/// Event handler that gets called when an ifolder member is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void MemberChecked( object source, EventArgs e )
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
			EnableMemberActionButtons = ( CheckedMembers.Count > 0 );
			EnableOwnerActionButton = ( CheckedMembers.Count == 1 );
		}

		/// <summary>
		/// Event that first when the PageFirstButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageFirstButton_Click( object source, ImageClickEventArgs e)
		{
			// Set to get the first members.
			CurrentMemberOffset = 0;
			GetiFolderMembers();
		}

		/// <summary>
		/// Event that first when the PagePreviousButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PagePreviousButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentMemberOffset -= iFolderMemberList.PageSize;
			if ( CurrentMemberOffset < 0 )
			{
				CurrentMemberOffset = 0;
			}

			GetiFolderMembers();
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentMemberOffset += iFolderMemberList.PageSize;
			GetiFolderMembers();
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentMemberOffset = ( ( TotaliFolderMembers - 1 ) / iFolderMemberList.PageSize ) * iFolderMemberList.PageSize;
			GetiFolderMembers();
		}

		/// <summary>
		/// Event handler that gets called when the Description button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void SaveDescription( object sender, EventArgs e )
		{
			try
			{
				web.SetiFolderDescription( iFolderID, Description.Text );
			}
			catch ( Exception ex )
			{
				TopNav.ShowError( GetString( "ERRORCANNOTSETDESCRIPTION" ), ex );
				return;
			}

			DescriptionButton.Enabled = false;
		}

		#endregion

		#region Web Form Designer generated code

		/// <summary>
		/// OnInit()
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
		/// this.Load += new System.EventHandler(this.Page_Load);
		private void InitializeComponent()
		{    
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			Policy.PolicyError += new Policy.PolicyErrorHandler( OnPolicyError );

			iFolderMemberList.ItemDataBound += new DataGridItemEventHandler( iFolderMemberList_DataGridItemBound );

			iFolderMemberListFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			iFolderMemberListFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			iFolderMemberListFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			iFolderMemberListFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
