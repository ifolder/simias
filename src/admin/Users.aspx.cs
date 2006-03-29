/***********************************************************************
 *  $RCSfile: Users.aspx.cs,v $
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
	/// Summary description for Users.
	/// </summary>
	public class Users : System.Web.UI.Page
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
		/// Web controls.
		/// </summary>
		protected DataGrid Accounts;

		/// <summary>
		/// Web Controls.
		/// </summary>
		protected MemberSearch MemberSearch;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected PageFooter AccountsFooter;

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
		/// Event handler for when a datagrid item is bound.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		private void Accounts_DataGridItemBound( Object source, DataGridItemEventArgs e )
		{
			if ( ( e.Item.ItemType == ListItemType.Item ) || ( e.Item.ItemType == ListItemType.AlternatingItem ) )
			{
				// Check for any rows that are not supposed to be displayed and disable the image.
				// All of the other cells should contain empty strings.
				DataTable dt = ( Accounts.DataSource as DataView ).Table;
				if ( ( bool )dt.Rows[ e.Item.DataSetIndex ][ "VisibleField" ] == false )
				{
					( e.Item.Cells[ 0 ].FindControl( "UserImage" ) as System.Web.UI.WebControls.Image ).Visible = false;
				}
			}
		}

		/// <summary>
		/// Creates a DataSource containing user names from a search.
		/// </summary>
		/// <returns>An DataView object containing the ifolder users.</returns>
		private DataView CreateDataSource()
		{
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "AdminField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "StatusField", typeof( string ) ) );

			iFolderUser[] userList;
			int total;

			if ( MemberSearch.SearchName.Value == String.Empty )
			{
				userList = web.GetUsersBySearch( 
					MemberSearch.GetSearchAttribute(), 
					MemberSearch.GetSearchOperation(), 
					"*", 
					CurrentUserOffset, 
					Accounts.PageSize, 
					out total );
			}
			else
			{
				userList = web.GetUsersBySearch( 
					MemberSearch.GetSearchAttribute(), 
					MemberSearch.GetSearchOperation(), 
					MemberSearch.SearchName.Value, 
					CurrentUserOffset, 
					Accounts.PageSize, 
					out total );
			}

			foreach( iFolderUser user in userList )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = user.UserID;
				dr[ 2 ] = user.IsAdmin;
				dr[ 3 ] = user.UserName;
				dr[ 4 ] = user.FullName;
				dr[ 5 ] = user.Enabled.ToString();

				dt.Rows.Add( dr );
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < Accounts.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = false;
				dr[ 3 ] = String.Empty;
				dr[ 4 ] = String.Empty;
				dr[ 5 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotalUsers = total;

			// Build the data view from the table.
			return new DataView( dt );
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
				Accounts.Columns[ 0 ].HeaderText = GetString( "TYPE" );
				Accounts.Columns[ 1 ].HeaderText = GetString( "USERNAME" );
				Accounts.Columns[ 2 ].HeaderText = GetString( "FULLNAME" );
				Accounts.Columns[ 3 ].HeaderText = GetString( "ENABLED" );

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
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Sets the page button state of the Accounts list.
		/// </summary>
		private void SetPageButtonState()
		{
			AccountsFooter.SetPageButtonState( 
				Accounts, 
				CurrentUserOffset, 
				TotalUsers, 
				GetString( "USERS" ),
				GetString( "USER" ) );
		}

		#endregion

		#region Protected Methods

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
		/// Gets the image representing the user type.
		/// </summary>
		/// <returns></returns>
		protected string GetUserImage( Object isAdmin )
		{
			return ( bool )isAdmin ? "images/ifolder_admin.gif" : "images/ifolder_user.gif";
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
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Event that first when the PageFirstButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageFirstButton_Click( object source, ImageClickEventArgs e)
		{
			// Set to get the first users.
			CurrentUserOffset = 0;

			// Rebind the data source with the new data.
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();

			// Set the button state.
			SetPageButtonState();;
		}

		/// <summary>
		/// Event that first when the PagePreviousButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PagePreviousButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset -= Accounts.PageSize;
			if ( CurrentUserOffset < 0 )
			{
				CurrentUserOffset = 0;
			}

			// Rebind the data source with the new data.
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();

			// Set the button state.
			SetPageButtonState();;
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset += Accounts.PageSize;

			// Rebind the data source with the new data.
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();

			// Set the button state.
			SetPageButtonState();;
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentUserOffset = ( ( TotalUsers - 1 ) / Accounts.PageSize ) * Accounts.PageSize;

			// Rebind the data source with the new data.
			Accounts.DataSource = CreateDataSource();
			Accounts.DataBind();

			// Set the button state.
			SetPageButtonState();;
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
		/// this.Load += new System.EventHandler( this.Page_Load );
		private void InitializeComponent()
		{
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			MemberSearch.SearchButton.Click += new System.EventHandler( SearchButton_Click );
			Accounts.ItemDataBound += new DataGridItemEventHandler( Accounts_DataGridItemBound );

			AccountsFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			AccountsFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			AccountsFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			AccountsFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			this.Load += new System.EventHandler( this.Page_Load );
		}

		#endregion
	}
}
