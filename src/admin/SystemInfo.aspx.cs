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
		protected PageFooter AdminListFooter;

		/// <summary>
		/// Delete admin button control.
		/// </summary>
		protected Button DeleteButton;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the current admin offset.
		/// </summary>
		private int CurrentAdminOffset
		{
			get { return ( int )ViewState[ "CurrentAdminOffset" ]; }
			set { ViewState[ "CurrentAdminOffset" ] = value; }
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
				dr[ 1 ] = admin.UserID;
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

				// Initialize state variables.
				CurrentAdminOffset = 0;
				TotalAdmins = 0;
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
			return false;
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
		/// Event handler that gets called when the admin checkbox is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAdminChecked( object source, EventArgs e )
		{
		}

		/// <summary>
		/// Event handler that gets called when the all admins checkbox is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnAllAdminsChecked( object source, EventArgs e )
		{
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

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
