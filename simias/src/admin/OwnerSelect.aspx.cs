/****************************************************************************
 |
 | Copyright (c) [2007] Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com

 | Author: Mike Lasky (mlasky@novell.com)
 |***************************************************************************/


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
	/// Summary description for OwnerSelect.
	/// </summary>
	public class OwnerSelect : System.Web.UI.Page
	{
		#region Class Members
	
		/// <summary>
		/// Viewable MemberList data grid cell indices.
		/// </summary>
		private const int Member_IDCell       = 0;
		private const int Member_ImageCell    = 1;
		private const int Member_UserNameCell = 2;
		private const int Member_FullNameCell = 3;


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
		/// Web controls.
		/// </summary>
		protected Button BackButton;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected Button NextButton;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected Button CancelButton;


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
		/// Gets or sets the DataTable object used by the MemberList control.
		/// </summary>
		private DataTable MemberListSource
		{
			get { return ViewState[ "MemberListSource" ] as DataTable; }
			set { ViewState[ "MemberListSource" ] = value; }
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
		/// Gets the iFolder Owner.
		/// </summary>
		private string iFolderOwner
		{
			get 
			{ 
				string param = Request.Params[ "owner" ];
				return ( param == null ) ? String.Empty : param;
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
		/// Gets or sets the currently selected owner.
		/// </summary>
		private OwnerInfo SelectedOwner
		{
			get { return ViewState[ "SelectedOwner" ] as OwnerInfo; }	
			set { ViewState[ "SelectedOwner" ] = value; }
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


		/// <summary>
		/// Gets the starting member list page.
		/// </summary>
		private int MemberListPage
		{
			get 
			{ 
				string param = Request.Params[ "pg" ];
				return ( ( param == null ) || ( param == String.Empty ) ) ? 0 : Convert.ToInt32( param );
			} 
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			TopNav.AddBreadCrumb( GetString( "IFOLDERS" ), "iFolders.aspx" );
			TopNav.AddBreadCrumb( GetString( "CREATENEWIFOLDER" ), null );
			// Pass this page information to create the help link
			TopNav.AddHelpLink(GetString("CREATENEWIFOLDER"));
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
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );

			iFolderUserSet userList = web.GetUsersBySearch( 
				MemberSearch.SearchAttribute, 
				MemberSearch.SearchOperation, 
				( MemberSearch.SearchName == String.Empty ) ? "*" : MemberSearch.SearchName, 
				CurrentUserOffset, 
				MemberList.PageSize );

			foreach( iFolderUser user in userList.Items )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = user.ID;
				dr[ 2 ] = user.UserName;
				dr[ 3 ] = user.FullName;

				dt.Rows.Add( dr );
			}

			// If the page size is not full, finish it with empty entries.
			for ( int i = dt.Rows.Count; i < MemberList.PageSize; ++i )
			{
				dr = dt.NewRow();
				dr[ 0 ] = false;
				dr[ 1 ] = String.Empty;
				dr[ 2 ] = String.Empty;
				dr[ 3 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotalUsers = userList.Total;

			// Save the data table.
			MemberListSource = dt;

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
				// Initialize localized fields.
				HeaderTitle.Text = String.Format( GetString( "SELECTIFOLDEROWNER" ), iFolderName );
				BackButton.Text = GetString( "BACK" );
				NextButton.Text = GetString( "NEXT" );
				CancelButton.Text = GetString( "CANCEL" );

				// Remember the page that we came from.
				string param = Request.Params[ "ref" ];
				ReferringPage = ( ( param == null ) || ( param == String.Empty ) ) ? 
					Page.Request.UrlReferrer.ToString() : param;

				// Initialize state variables.
				CurrentUserOffset = MemberListPage;
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

			SetSelectedMember();
		}

		/// <summary>
		/// Sets the selected member in the data grid.
		/// </summary>
		private void SetSelectedMember()
		{
			if ( SelectedOwner != null )
			{
				OwnerInfo oi = SelectedOwner;
				DataRow dr = MemberListSource.Rows[ oi.Index ];
				MemberList.SelectedIndex = ( dr[ "IDField" ] as string == oi.UserID ) ? oi.Index : -1;
			}
			else
			{
				// See if an owner was specified in the query string.
				if ( iFolderOwner != String.Empty )
				{
					// Loop through the data table by rows until the owner is found.
					DataTable dt = ( MemberList.DataSource as DataView ).Table;
					for( int i = 0; i < dt.Rows.Count; ++i )
					{
						if ( dt.Rows[ i ][ "IDField" ] as string == iFolderOwner )
						{
							SelectedOwner = new OwnerInfo(
								dt.Rows[ i ][ "IDField" ] as string,
								dt.Rows[ i ][ "NameField" ] as string,
								dt.Rows[ i ][ "FullNameField" ] as string,
								i );

							MemberList.SelectedIndex = i;
							NextButton.Enabled = true;
							break;
						}
					}
				}
				else
				{
					MemberList.SelectedIndex = -1;
				}
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler for the back button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void BackButton_Clicked( Object sender, EventArgs e )
		{
			// Return back to the referring page.
			Page.Response.Redirect( 
				String.Format( 
					"CreateiFolder.aspx?name={0}&desc={1}{2}&pg={3}&ref={4}", 
					iFolderName, 
					iFolderDescription,
					( SelectedOwner != null ) ? "&owner=" + SelectedOwner.UserID : String.Empty,
					CurrentUserOffset.ToString(),
					ReferringPage ), 
				true );
		}

		/// <summary>
		/// Event handler for the cancel button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void CancelButton_Clicked( Object sender, EventArgs e )
		{
			// Return back to the referring page.
			string url = web.TrimUrl(ReferringPage);
			Page.Response.Redirect( url, true );
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
		}

		/// <summary>
		/// Event handler for the ok button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void NextButton_Clicked( Object sender, EventArgs e )
		{
			Page.Response.Redirect( 
				String.Format( 
					"MemberSelect.aspx?op=createifolder&name={0}&desc={1}&owner={2}&fn={3}&pg={4}&ref={5}",
					iFolderName,
					iFolderDescription,
					SelectedOwner.UserID,
					SelectedOwner.FullName,
					CurrentUserOffset.ToString(),
					ReferringPage ), 
				true );
		}

		/// <summary>
		/// Event handler that gets called when the member name control is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnMemberName_Click( Object sender, EventArgs e )
		{
			DataTable dt = MemberListSource;
			DataGridItem dgi = ( ( sender as LinkButton ).Parent as TableCell ).Parent as DataGridItem;
			MemberList.SelectedIndex = dgi.ItemIndex;
			MemberList.DataSource = new DataView( dt );
			MemberList.DataBind();

			DataRow dr = dt.Rows[ dgi.ItemIndex ];
			
			SelectedOwner = new OwnerInfo( 
				dr[ "IDField" ] as string,
				dr[ "NameField" ] as string,
				dr[ "FullNameField" ] as string,
				dgi.ItemIndex );

			// Enable the next button.
			NextButton.Enabled = true;
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

			MemberListFooter.PageFirstClick += new ImageClickEventHandler( Member_PageFirstButton_Click );
			MemberListFooter.PagePreviousClick += new ImageClickEventHandler( Member_PagePreviousButton_Click );
			MemberListFooter.PageNextClick += new ImageClickEventHandler( Member_PageNextButton_Click );
			MemberListFooter.PageLastClick += new ImageClickEventHandler( Member_PageLastButton_Click );

			this.Load += new System.EventHandler( this.Page_Load );
		}

		#endregion

		#region OwnerInfo Class

		/// <summary>
		/// Class used to hold intermediate information when a owner is selected.
		/// </summary>
		[ Serializable() ]
		private class OwnerInfo
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

			/// <summary>
			/// Data Table index.
			/// </summary>
			public int Index;

			#endregion

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="userID">ID of the user</param>
			/// <param name="userName">Name of the user</param>
			/// <param name="fullName">Full name of the user</param>
			/// <param name="index">Index in the data table for this user</param>
			public OwnerInfo( string userID, string userName, string fullName, int index )
			{
				UserID = userID;
				UserName = userName;
				FullName = fullName;
				Index = index;
			}

			#endregion
		}

		#endregion
	}
}
