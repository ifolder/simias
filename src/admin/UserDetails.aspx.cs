/***********************************************************************
 *  $RCSfile: UserDetails.aspx.cs,v $
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
	/// Summary description for UserDetails.
	/// </summary>
	public class Details : System.Web.UI.Page
	{
		#region Class Members

		/// <summary>
		/// iFolder list data grid column indices.
		/// </summary>
		private const int iFolderIDColumn = 0;
		private const int iFolderCheckColumn = 1;
		private const int iFolderTypeColumn = 2;
		private const int iFolderNameColumn = 3;
		private const int iFolderOwnerColumn = 4;
		private const int iFolderSizeColumn = 5;

		/// <summary>
		/// iFolder list display types.
		/// </summary>
		private enum ListDisplayType
		{
			All,
			Owned,
			Shared
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
		/// User detail controls.
		/// </summary>
		protected Literal FullName;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal LdapContext;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal LastLogin;


		/// <summary>
		/// iFolder user policy control.
		/// </summary>
		protected Policy Policy;


		/// <summary>
		/// iFolder list controls.
		/// </summary>
		protected DataGrid iFolderList;

		/// <summary>
		/// iFolder list footer control.
		/// </summary>
		protected PageFooter iFolderListFooter;


		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected LinkButton AlliFoldersLink;

		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected LinkButton OwnediFoldersLink;

		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected LinkButton SharediFoldersLink;

		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected HtmlGenericControl CurrentTab;


		/// <summary>
		/// iFolder create and delete button controls.
		/// </summary>
		protected Button DeleteiFolderButton;

		/// <summary>
		/// iFolder create and delete button controls.
		/// </summary>
		protected Button CreateiFolderButton;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets whether the all iFolder CheckBox is checked.
		/// </summary>
		private bool AlliFoldersChecked
		{
			get { return ( bool )ViewState[ "AlliFoldersChecked" ]; }
			set { ViewState[ "AlliFoldersChecked" ] = value; }
		}

		/// <summary>
		/// Gets or sets the iFolders that are checked in the list.
		/// </summary>
		private Hashtable CheckediFolders
		{
			get { return ViewState[ "CheckediFolders" ] as Hashtable; }
			set { ViewState[ "CheckediFolders" ] = value; }
		}

		/// <summary>
		/// Gets or sets the active ifolder tab.
		/// </summary>
		private ListDisplayType ActiveiFolderTab
		{
			get { return ( ListDisplayType )ViewState[ "ActiveTab" ]; }
			set { ViewState[ "ActiveTab" ] = value; }
		}

		/// <summary>
		/// Gets or sets the current iFolder offset.
		/// </summary>
		private int CurrentiFolderOffset
		{
			get { return ( int )ViewState[ "CurrentiFolderOffset" ]; }
			set { ViewState[ "CurrentiFolderOffset" ] = value; }
		}

		/// <summary>
		/// Enables or disables the ifolder actions buttons.
		/// </summary>
		private bool EnableiFolderActionButtons
		{
			set { DeleteiFolderButton.Enabled = value; }
		}

		/// <summary>
		/// Gets or sets the total number of iFolders contained in
		/// the last search.
		/// </summary>
		private int TotaliFolders
		{
			get { return ( int )ViewState[ "TotaliFolders" ]; }
			set { ViewState[ "TotaliFolders" ] = value; }
		}

		/// <summary>
		/// Gets the iFolder user ID.
		/// </summary>
		private string UserID
		{
			get { return Request.Params[ "ID" ]; } 
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Creates a list of iFolders where the user is a member.
		/// </summary>
		/// <returns>A DataView object containing the iFolder list.</returns>
		private DataView CreateiFolderList()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "OwnerIDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "SharedField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "DisabledField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "OwnerNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "SizeField", typeof( string ) ) );

			// Get the iFolder list for this user.
			int total;
			iFolder[] list;

			switch ( ActiveiFolderTab )
			{
				case ListDisplayType.Owned:
					list = web.GetiFoldersByMember( UserID, MemberRole.Owner, CurrentiFolderOffset, iFolderList.PageSize, out total );
					break;

				case ListDisplayType.Shared:
					list = web.GetiFoldersByMember( UserID, MemberRole.Shared, CurrentiFolderOffset, iFolderList.PageSize, out total );
					break;

				case ListDisplayType.All:
				default:
					list = web.GetiFoldersByMember( UserID, MemberRole.Any, CurrentiFolderOffset, iFolderList.PageSize, out total );
					break;
			}

			foreach( iFolder folder in list )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = folder.ID;
				dr[ 2 ] = folder.OwnerID;
				dr[ 3 ] = ( folder.MemberCount > 1 ) ? true : false;
				dr[ 4 ] = !folder.Enabled;
				dr[ 5 ] = folder.Name;
				dr[ 6 ] = folder.OwnerFullName;
				dr[ 7 ] = Utils.ConvertToUnitString( folder.Size, true, rm );

				dt.Rows.Add( dr );
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < iFolderList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = String.Empty;
				dr[ 3 ] = false;
				dr[ 4 ] = false;
				dr[ 5 ] = String.Empty;
				dr[ 6 ] = String.Empty;
				dr[ 7 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotaliFolders = total;

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// Gets the iFolders for the current user.
		/// </summary>
		private void GetiFolders()
		{
			// Create a data source containing the iFolders.
			iFolderList.DataSource = CreateiFolderList();
			iFolderList.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Gets the details about the user and fills out the details table.
		/// </summary>
		private void GetUserDetails()
		{
			// Get the iFolder user information.
			iFolderUserDetails details = web.GetUserDetails( UserID );

			string lastLogin = ( details.LastLogin == DateTime.MinValue ) ?
				GetString( "NOTAVAILABLE" ) : details.LastLogin.ToString();

			int totaliFolders = details.OwnediFolderCount + details.SharediFolderCount;

			// Add the information rows to the table.
			FullName.Text = details.FullName;
			LdapContext.Text = details.LdapContext;
			LastLogin.Text = lastLogin;
		}

		/// <summary>
		/// Event handler for when a datagrid item is bound.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void iFolderList_DataGridItemBound( Object sender, DataGridItemEventArgs e )
		{
			if ( ( e.Item.ItemType == ListItemType.Item ) || ( e.Item.ItemType == ListItemType.AlternatingItem ) )
			{
				// Check for any rows that are not supposed to be displayed and disable the image.
				// All of the other cells should contain empty strings.
				DataTable dt = ( iFolderList.DataSource as DataView ).Table;
				if ( ( bool )dt.Rows[ e.Item.DataSetIndex ][ "VisibleField" ] == false )
				{
					( e.Item.Cells[ 0 ].FindControl( "iFolderListCheckBox" ) as CheckBox ).Visible = false;
					( e.Item.Cells[ 1 ].FindControl( "iFolderListImage" ) as System.Web.UI.WebControls.Image ).Visible = false;
				}
			}
			else if ( e.Item.ItemType == ListItemType.Header )
			{
				// Set the all users checked state.
				( e.Item.FindControl( "iFolderAllCheckBox" ) as CheckBox ).Checked = AlliFoldersChecked;
			}
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
				iFolderList.Columns[ iFolderTypeColumn ].HeaderText = GetString( "TYPE" );
				iFolderList.Columns[ iFolderNameColumn ].HeaderText = GetString( "NAME" );
				iFolderList.Columns[ iFolderOwnerColumn ].HeaderText = GetString( "OWNER" );
				iFolderList.Columns[ iFolderSizeColumn ].HeaderText = GetString( "SIZE" );

				DeleteiFolderButton.Text = GetString( "DELETE" );
				CreateiFolderButton.Text = GetString( "CREATE" );

				AlliFoldersLink.Text = GetString( "ALL" );
				OwnediFoldersLink.Text = GetString( "OWNED" );
				SharediFoldersLink.Text = GetString( "SHARED" );

				// Initialize state variables.
				CurrentiFolderOffset = 0;
				TotaliFolders = 0;
				AlliFoldersChecked = false;
				CheckediFolders = new Hashtable();

				// Set the active ifolder tab.
				ActiveiFolderTab = ListDisplayType.All;
			}

			// Set the active ifolder display tab.
			SetActiveiFolderListTab( ActiveiFolderTab );
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender(object sender, EventArgs e)
		{
			// Fill in the user details.
			GetUserDetails();

			// Fill in the policy information.
			Policy.GetUserPolicies();

			// Get the iFolders.
			GetiFolders();
		}

		/// <summary>
		/// Rebinds the new data to the iFolder list and sets the new totals.
		/// </summary>
		private void RebindiFolderList()
		{
			iFolderList.DataSource = CreateiFolderList();
			iFolderList.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Sets the active ifolder list display tab.
		/// </summary>
		/// <param name="activeTab"></param>
		/// <returns>The active list tab.</returns>
		private void SetActiveiFolderListTab( ListDisplayType activeTab )
		{
			ActiveiFolderTab = activeTab;
			CurrentTab.ID = activeTab.ToString();
		}

		/// <summary>
		/// Sets the page button state of the ifolder list.
		/// </summary>
		private void SetPageButtonState()
		{
			iFolderListFooter.SetPageButtonState( 
				iFolderList, 
				CurrentiFolderOffset, 
				TotaliFolders, 
				GetString( "IFOLDERS" ),
				GetString( "IFOLDER" ) );
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the all ifolders tab is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void AlliFolders_Clicked( Object sender, EventArgs e )
		{
			SetActiveiFolderListTab( ListDisplayType.All );
			RebindiFolderList();
		}

		/// <summary>
		/// Gets the image representing the iFolder type.
		/// </summary>
		/// <param name="disabled"></param>
		/// <param name="shared"></param>
		/// <returns></returns>
		protected string GetiFolderImage( object disabled, object shared )
		{
			return ( bool )disabled ? "images/ifolder_16-gray.gif" : ( bool )shared ? "images/ifolder_16.gif" : "images/ifolder_16.gif";
		}

		/// <summary>
		/// Returns the checked state for the specified member.
		/// </summary>
		/// <param name="iFolderID">ID of the ifolder</param>
		/// <returns>True if ifolder is checked.</returns>
		protected bool GetMemberCheckedState( Object iFolderID )
		{
			return CheckediFolders.ContainsKey( iFolderID ) ? true : false;
		}

		/// <summary>
		/// Gets the navigation url for the owner of the ifolder if the owner
		/// is not the current user.
		/// </summary>
		/// <param name="ownerID">The ID of the owner of the ifolder.</param>
		/// <returns>The URL to navigate to the owner of the ifolder.</returns>
		protected string GetOwnerUrl( Object ownerID )
		{
			return ( ownerID as string != UserID ) ? String.Format( "UserDetails.aspx?id={0}", ownerID ) : String.Empty;
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
		/// Gets the name of the specified user.
		/// </summary>
		/// <returns>The username</returns>
		protected string GetUserName()
		{
			iFolderUser user = web.GetUser( UserID );
			return user.UserName;
		}

		/// <summary>
		/// Event handler that gets called when the check all iFolders checkbox is selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnAlliFoldersChecked( object sender, EventArgs e )
		{
			CheckBox checkBox = sender as CheckBox;
			foreach( DataGridItem item in iFolderList.Items )
			{
				string iFolderID = item.Cells[ 0 ].Text;
				if ( iFolderID != "&nbsp;" )
				{
					if ( checkBox.Checked )
					{
						CheckediFolders[ iFolderID ] = null;
					}
					else
					{
						// Remove this iFolder from the list.
						CheckediFolders.Remove( iFolderID );
					}
				}
			}

			// See if there are any checked members.
			EnableiFolderActionButtons = ( CheckediFolders.Count > 0 );
			AlliFoldersChecked = checkBox.Checked;

			// Rebind the data source with the new data.
			RebindiFolderList();
		}

		/// <summary>
		/// Event handler that gets called when the create ifolder button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnCreateiFolder( object source, EventArgs e )
		{
			Page.Response.Redirect( String.Format( "MemberSelect.aspx?op=createifolder&owner={0}", UserID ), true );
		}

		/// <summary>
		/// Event handler that gets called when the delete ifolder button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDeleteiFolder( object source, EventArgs e )
		{
			foreach( string iFolderID in CheckediFolders.Keys )
			{
				web.DeleteiFolder( iFolderID );
			}

			// Clear the checked members.
			CheckediFolders.Clear();
			AlliFoldersChecked = false;

			// Rebind the data source with the new data.
			RebindiFolderList();
		}

		/// <summary>
		/// Event handler that gets called when an ifolder is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OniFolderChecked( object source, EventArgs e )
		{
			// Get the data grid row for this ifolder.
			CheckBox checkBox = source as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string iFolderID = item.Cells[ 0 ].Text;
			if ( iFolderID != "&nbsp;" )
			{
				// iFolder is being checked.
				if ( checkBox.Checked )
				{
					CheckediFolders[ iFolderID ] = null;
				}
				else
				{
					// Remove this member from the list.
					CheckediFolders.Remove( iFolderID );
				}
			}

			// See if there are any checked members.
			EnableiFolderActionButtons = ( CheckediFolders.Count > 0 );
		}

		/// <summary>
		/// Event handler that gets called when the owned ifolders tab is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OwnediFolders_Clicked( Object sender, EventArgs e )
		{
			SetActiveiFolderListTab( ListDisplayType.Owned );
			RebindiFolderList();
		}

		/// <summary>
		/// Event that first when the PageFirstButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageFirstButton_Click( object source, ImageClickEventArgs e)
		{
			// Set to get the first iFolders.
			CurrentiFolderOffset = 0;
			RebindiFolderList();
		}

		/// <summary>
		/// Event that first when the PagePreviousButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PagePreviousButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentiFolderOffset -= iFolderList.PageSize;
			if ( CurrentiFolderOffset < 0 )
			{
				CurrentiFolderOffset = 0;
			}

			RebindiFolderList();
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentiFolderOffset += iFolderList.PageSize;
			RebindiFolderList();
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentiFolderOffset = ( ( TotaliFolders - 1 ) / iFolderList.PageSize ) * iFolderList.PageSize;
			RebindiFolderList();
		}

		/// <summary>
		/// Event handler that gets called when the shared ifolders tab is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void SharediFolders_Clicked( Object sender, EventArgs e )
		{
			SetActiveiFolderListTab( ListDisplayType.Shared );
			RebindiFolderList();
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
		/// this.Load += new System.EventHandler(this.Page_Load);
		private void InitializeComponent()
		{    
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			iFolderList.ItemDataBound += new DataGridItemEventHandler( iFolderList_DataGridItemBound );

			iFolderListFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			iFolderListFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			iFolderListFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			iFolderListFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			this.Load += new System.EventHandler(this.Page_Load);
		}

		#endregion
	}
}
