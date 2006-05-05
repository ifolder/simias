/***********************************************************************
 *  $RCSfile: iFolders.aspx.cs,v $
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
using System.Web.Services.Protocols;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for iFolders.
	/// </summary>
	public class iFolders : System.Web.UI.Page
	{
		#region Class Members

		/// <summary>
		/// iFolder list data grid column indices.
		/// </summary>
		private const int iFolderIDColumn = 0;
		private const int iFolderDisabledColumn = 1;
		private const int iFolderCheckColumn = 2;
		private const int iFolderTypeColumn = 3;
		private const int iFolderNameColumn = 4;
		private const int iFolderOwnerColumn = 5;
		private const int iFolderSizeMemberCountColumn = 6;
		private const int iFolderLastModifiedColumn = 7;
		private const int iFolderSizeColumn = 8;

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
		/// Control that checks or unchecks all of the ifolders in the current view.
		/// </summary>
		protected CheckBox AlliFoldersCheckBox;


		/// <summary>
		/// iFolder list controls.
		/// </summary>
		protected DataGrid iFolderList;

		/// <summary>
		/// iFolder list footer control.
		/// </summary>
		protected ListFooter iFolderListFooter;


		/// <summary>
		/// Web Controls.
		/// </summary>
		protected iFolderSearch iFolderSearch;


		/// <summary>
		/// Delete ifolder button control.
		/// </summary>
		protected Button DeleteButton;

		/// <summary>
		/// Disable ifolder button control.
		/// </summary>
		protected Button DisableButton;

		/// <summary>
		/// Enable ifolder button control.
		/// </summary>
		protected Button EnableButton;

		/// <summary>
		/// Create ifolder button control.
		/// </summary>
		protected Button CreateButton;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the ifolders that are checked in the list.
		/// </summary>
		private Hashtable CheckediFolders
		{
			get { return ViewState[ "CheckediFolders" ] as Hashtable; }
			set { ViewState[ "CheckediFolders" ] = value; }
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
		/// Gets or sets the total number of iFolders contained in
		/// the last search.
		/// </summary>
		private int TotaliFolders
		{
			get { return ( int )ViewState[ "TotaliFolders" ]; }
			set { ViewState[ "TotaliFolders" ] = value; }
		}

		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			TopNav.AddBreadCrumb( GetString( "IFOLDERS" ), null );
		}
	
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
			dt.Columns.Add( new DataColumn( "MemberCountField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "LastModifiedField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "SizeField", typeof( string ) ) );

			// Get the iFolder list for this user.
			iFolderSet list = web.GetiFoldersByName(
				iFolderSearch.SearchOperation,
				( iFolderSearch.SearchName == String.Empty ) ? "*" : iFolderSearch.SearchName, 
				CurrentiFolderOffset, 
				iFolderList.PageSize );

			foreach( iFolder folder in list.Items )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = folder.ID;
				dr[ 2 ] = folder.OwnerID;
				dr[ 3 ] = ( folder.MemberCount > 1 ) ? true : false;
				dr[ 4 ] = !folder.Enabled;
				dr[ 5 ] = folder.Name;
				dr[ 6 ] = folder.OwnerFullName;
				dr[ 7 ] = folder.MemberCount.ToString();
				dr[ 8 ] = folder.LastModified.ToShortDateString();
				dr[ 9 ] = Utils.ConvertToUnitString( folder.Size, true, rm );

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
				dr[ 8 ] = String.Empty;
				dr[ 9 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotaliFolders = list.Total;

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
				DisableButton.Text = GetString( "DISABLE" );
				EnableButton.Text = GetString( "ENABLE" );
				CreateButton.Text = GetString( "CREATE" );

				// Initialize state variables.
				CurrentiFolderOffset = 0;
				TotaliFolders = 0;
				AlliFoldersCheckBox.Checked = false;
				CheckediFolders = new Hashtable();
			}
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender(object sender, EventArgs e)
		{
			BuildBreadCrumbList();
			GetiFolders();
		}

		/// <summary>
		/// Enables or disables the ifolder action buttons.
		/// </summary>
		private void SetActionButtons()
		{
			Hashtable ht = CheckediFolders;
			DeleteButton.Enabled = ( ht.Count > 0 ) ? true : false;
			DisableButton.Enabled = ht.ContainsValue( Boolean.FalseString );
			EnableButton.Enabled = ht.ContainsValue( Boolean.TrueString );
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

		/// <summary>
		/// Sets the ifolder synchronization status on all selected ifolders.
		/// </summary>
		/// <param name="syncStatus">If true then all selected ifolders will be enabled.</param>
		private void SetSelectediFolderStatus( bool syncStatus )
		{
			foreach( string ifolderID in CheckediFolders.Keys )
			{
				// Don't set the status if already set.
				if ( CheckediFolders[ ifolderID ] as string != syncStatus.ToString() )
				{
					iFolderPolicy policy = Utils.GetiFolderPolicyObject( ifolderID );
					policy.Locked = syncStatus;
					try
					{
						web.SetiFolderPolicy( policy );
					}
					catch ( Exception ex )
					{
						TopNav.ShowError( GetString( "ERRORCANNOTSETIFOLDERSTATUS" ), ex );
						return;
					}
				}
			}

			// Clear the checked members.
			CheckediFolders.Clear();
			AlliFoldersCheckBox.Checked = false;

			// Set the action buttons.
			SetActionButtons();

			// Rebind the data source with the new data.
			GetiFolders();
		}

		#endregion

		#region Protected Methods

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
		/// <param name="id">ID of the ifolder</param>
		/// <returns>True if ifolder is checked.</returns>
		protected bool GetMemberCheckedState( Object id )
		{
			return CheckediFolders.ContainsKey( id ) ? true : false;
		}

		/// <summary>
		/// Gets the navigation url for the owner of the ifolder if the owner
		/// is not the current user.
		/// </summary>
		/// <param name="ownerID">The ID of the owner of the ifolder.</param>
		/// <returns>The URL to navigate to the owner of the ifolder.</returns>
		protected string GetOwnerUrl( Object ownerID )
		{
			return String.Format( "UserDetails.aspx?id={0}", ownerID );
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
		/// Event handler that gets called when the check all iFolders checkbox is selected.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAlliFoldersChecked( object source, EventArgs e )
		{
			CheckBox checkBox = source as CheckBox;
			foreach( DataGridItem item in iFolderList.Items )
			{
				// In order to be checked, the row must not be empty.
				string ifolderID = item.Cells[ 0 ].Text;
				if ( ifolderID != "&nbsp;" )
				{
					if ( checkBox.Checked )
					{
						CheckediFolders[ ifolderID ] = item.Cells[ iFolderDisabledColumn ].Text;
					}
					else
					{
						// Remove this ifolder the list.
						CheckediFolders.Remove( ifolderID );
					}
				}
			}

			// Set the action buttons appropriately.
			SetActionButtons();

			// Rebind the data source with the new data.
			GetiFolders();
		}

		/// <summary>
		/// Event handler that gets called with the create ifolder button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnCreateButton_Click( object source, EventArgs e )
		{
			Page.Response.Redirect( "CreateiFolder.aspx", true );
		}

		/// <summary>
		/// Event handler that gets called with the delete ifolder button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDeleteButton_Click( object source, EventArgs e )
		{
			foreach( string ifolderID in CheckediFolders.Keys )
			{
				try
				{
					web.DeleteiFolder( ifolderID );
				}
				catch ( Exception ex )
				{
					TopNav.ShowError( GetString( "ERRORCANNOTDELETEIFOLDER" ), ex );
					return;
				}
			}

			// Clear the checked members.
			CheckediFolders.Clear();
			AlliFoldersCheckBox.Checked = false;

			// Set the action buttons.
			SetActionButtons();

			// Rebind the data source with the new data.
			GetiFolders();
		}

		/// <summary>
		/// Event handler that gets called with the disable ifolder button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnDisableButton_Click( object source, EventArgs e )
		{
			SetSelectediFolderStatus( true );
		}

		/// <summary>
		/// Event handler that gets called with the enable ifolder button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnEnableButton_Click( object source, EventArgs e )
		{
			SetSelectediFolderStatus( false );
		}

		/// <summary>
		/// Event handler that gets called when an ifolder is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OniFolderChecked( object source, EventArgs e )
		{
			// Get the data grid row for this member.
			CheckBox checkBox = source as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string ifolderID = item.Cells[ iFolderIDColumn ].Text;
			if ( ifolderID != "&nbsp;" )
			{
				// iFolder is being added.
				if ( checkBox.Checked )
				{
					CheckediFolders[ ifolderID ] = item.Cells[ iFolderDisabledColumn ].Text;
				}
				else
				{
					// Remove this ifolder from the list.
					CheckediFolders.Remove( ifolderID );
				}
			}

			// Set the ifolder action buttons.
			SetActionButtons();
		}

		/// <summary>
		/// Event handler that gets called when the ifolder search button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnSearchButton_Click( object source, EventArgs e )
		{
			// Always search from the beginning.
			CurrentiFolderOffset = 0;
			GetiFolders();
		}

		/// <summary>
		/// Event that first when the PageFirstButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageFirstButton_Click( object source, ImageClickEventArgs e)
		{
			// Set to get the first ifolders.
			CurrentiFolderOffset = 0;
			GetiFolders();
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

			// Rebind the data source with the new data.
			GetiFolders();
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentiFolderOffset += iFolderList.PageSize;

			// Rebind the data source with the new data.
			GetiFolders();
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentiFolderOffset = ( ( TotaliFolders - 1 ) / iFolderList.PageSize ) * iFolderList.PageSize;

			// Rebind the data source with the new data.
			GetiFolders();
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

			iFolderListFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			iFolderListFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			iFolderListFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			iFolderListFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			iFolderSearch.Click += new System.EventHandler( OnSearchButton_Click );
			this.Load += new System.EventHandler(this.Page_Load);
		}

		#endregion
	}
}
