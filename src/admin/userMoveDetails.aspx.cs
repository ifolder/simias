/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Mike Lasky (mlasky@novell.com)
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/



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
	public class UserMoveDetails : System.Web.UI.Page
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
		private const int iFolderSizeColumn = 6;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin remoteweb;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Logged in admin system rights instance
		/// </summary>
		UserSystemAdminRights uRights;
		
		/// <summary>
		/// Logged in user system rights value
		/// </summary>
		int sysAccessPolicy = 0;

		/// <summary>
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;


		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal UserName;

		/// <summary>
		/// User detail controls.
		/// </summary>
		protected Literal FullName;

		/// <summary>
		/// User detail controls.
		/// </summary>
     	protected Literal LdapContext;

        /// <summary>
        /// CurrentHome literal
        /// </summary>
		protected Literal CurrentHome;

        /// <summary>
        /// NewHome literal
        /// </summary>
		protected Literal NewHome;

        /// <summary>
        /// Status literal
        /// </summary>
		protected Literal  Completed;

        /// <summary>
        /// state of the reprovision
        /// </summary>
		protected Literal ReprovState;

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
		protected ListFooter iFolderListFooter;

		/// <summary>
		/// iFolder list view tab controls.
		/// </summary>
		protected HtmlGenericControl CurrentTab;

		/// <summary>
		/// Server URL for logged in user.
		/// </summary>
		protected string currentServerURL;
		protected bool reachable = true;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the iFolders that are checked in the list.
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
		///  Builds the breadcrumb list for this page.
		/// </summary>
		/// <param name="fullName">The full name of the current user.</param>
		private void BuildBreadCrumbList( string fullName )
		{
			TopNav.AddBreadCrumb( GetString( "SYSTEM" ), "SystemInfo.aspx" );
			TopNav.AddBreadCrumb( GetString( "REPROVISIONBUTTON" ), "userMove.aspx" );
			TopNav.AddBreadCrumb( fullName, null );
			// Pass this page information to create the help link
			TopNav.AddHelpLink("USERMOVEDETAILS");
		}

		/// <summary>
		/// Creates a list of iFolders where the user is a member.
		/// </summary>
		/// <returns>A DataView object containing the iFolder list.</returns>
		private DataView CreateiFolderList()
		{
			string folderState = String.Empty; 
			DataTable dt = new DataTable();
			DataRow dr;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "OwnerIDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "SharedField", typeof( bool) ) );
			dt.Columns.Add( new DataColumn( "DisabledField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "EncryptedField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "NameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "OwnerNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "ReachableField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "folderSize", typeof( string) ) );
			dt.Columns.Add( new DataColumn( "FolderMoveState", typeof( string) ) );

			// Get the iFolder list for this user.
			iFolderSet list;
			list = web.GetiFoldersByMember( UserID, MemberRole.Owner, CurrentiFolderOffset, iFolderList.PageSize );
			iFolder ifolder = null;

			foreach( iFolder folder in list.Items )
			{
				bool encrypted = false;
				
				try
				{
	                 string ifolderLocation = web.GetiFolderLocation (folder.ID);
        	         UriBuilder remoteurl = new UriBuilder(ifolderLocation);
                     remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
            	     web.Url = remoteurl.Uri.ToString();
				}
				catch
				{
					//skip exceptions
					continue;
				}
				
				try
				{
					ifolder = web.GetiFolder(folder.ID);
				}
				catch
				{
					web.Url = currentServerURL;
					ifolder = web.GetiFolder(folder.ID);
					reachable = false;
				}
				string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
				if(!(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty)))
				{
					// It is an encrypted ifolder 
					encrypted = true;
				}
				string ShortenedName = null;
				int ShortenedLength = 40;
				if(folder.Name.Length > ShortenedLength)
				{
					// make it of desired length
					ShortenedName = web.GetShortenedName(folder.Name, ShortenedLength);
				}


                  switch(ifolder.FolderMoveStatus ) 
				  {
						  case 0 :
								folderState = GetString("INITIALIZING");
							  break;

						  case 1 :
				        		folderState = GetString("TRANSFERING");
				  				break;

						  case 2 :
								folderState = GetString("COMPLETED"); 
								break;
				  }

			
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = folder.ID;
				dr[ 2 ] = folder.OwnerID;
				dr[ 3 ] = ( folder.MemberCount > 1 ) ? true : false;
				dr[ 4 ] = !ifolder.Enabled;
				dr[ 5 ] = ( encrypted ) ? true : false;
				dr[ 6 ] = ( folder.Name.Length > ShortenedLength) ? ShortenedName : folder.Name;
				dr[ 7 ] = folder.OwnerFullName;
				dr[ 8 ] = reachable;
				dr[ 9 ] = folder.Name;
				dr[ 10 ] = folder.Size.ToString();
				dr[ 11 ] = folderState; 

				dt.Rows.Add( dr );
				reachable = true;
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
				dr[ 5 ] = false;
				dr[ 6 ] = String.Empty;
				dr[ 7 ] = String.Empty;
				dr[ 8 ] = false;
				dr[ 9 ] = String.Empty; 
		//		dr[ 10 ] = String.Empty ;  //TODO: verify
				dr[ 11 ] = String.Empty; 

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			TotaliFolders = list.Total;
			web.Url = currentServerURL;

			// Build the data view from the table.
			return new DataView( dt );
		}


		/// <summary>
		/// Gets the iFolders for the current user.
		/// </summary>
		/// <param name="checkedState"></param>
		private void GetiFolders( bool checkedState )
		{
			// Create a data source containing the iFolders.
			iFolderList.DataSource = CreateiFolderList();
			iFolderList.DataBind();
			SetPageButtonState();
		}

		/// <summary>
		/// Gets the details about the user and fills out the details table.
		/// </summary>
		/// <returns>The user's full name.</returns>
		private string GetUserDetails()
		{
			// Insert NewLine char after 80 char	
			int NewlineAt=80;	
			// Get the iFolder user information.
			string NewHomeUrl = web.GetNewHomeServerURLForUserID( UserID );
			if( ! String.IsNullOrEmpty( NewHomeUrl ))
			{
				UriBuilder remoteurl = new UriBuilder( NewHomeUrl );
				remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
				web.Url = remoteurl.Uri.ToString();
			}

			iFolderUserDetails details = null;

			try
			{
				details = web.GetUserDetails( UserID );
			}
			catch
			{
				web.Url = currentServerURL;
				details = web.GetUserDetails( UserID );
			}
			if( details == null )
			{
				return String.Empty;
			}

			// Add the information rows to the table.
			UserName.Text = FormatInputString(details.UserName,NewlineAt);
			FullName.Text = FormatInputString(details.FullName,NewlineAt);
			LdapContext.Text = details.LdapContext;
			NewHome.Text =  details.DetailNewHomeServer;
			Completed.Text = details.DetailDataMovePercentage.ToString();
			CurrentHome.Text = details.DetailHomeServer;
			ReprovState.Text =  details.DetailDataMoveStatus ; 
	
			web.Url = currentServerURL;
			
			return details.FullName;
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
			currentServerURL = web.Url;
			remoteweb = new iFolderAdmin();
			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;
			remoteweb.Url = web.Url;
			reachable = true;

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			string userID = Session[ "UserID" ] as String;
			if(userID != null)
				sysAccessPolicy = web.GetUserGroupRights(userID, null);
			else
				sysAccessPolicy = 0; 
			uRights = new UserSystemAdminRights(sysAccessPolicy);
			if(uRights.SystemPolicyManagementAllowed == false)
				Page.Response.Redirect(String.Format("Error.aspx?ex={0}&Msg={1}",GetString( "ACCESSDENIED" ), GetString( "ACCESSDENIEDERROR" )));

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				iFolderList.Columns[ iFolderTypeColumn ].HeaderText = GetString( "TYPE" );
				iFolderList.Columns[ iFolderNameColumn ].HeaderText = GetString( "NAME" );
				iFolderList.Columns[ iFolderOwnerColumn ].HeaderText = GetString( "OWNER" );

				// Initialize state variables.
				CurrentiFolderOffset = 0;
				TotaliFolders = 0;
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
			// Fill in the user details.
			string fullName = GetUserDetails();

			// Set the breadcrumb list.
			BuildBreadCrumbList( fullName );

			// Fill in the policy information.
	//		if(!Policy.GetUserPolicies())
	//			TopNav.ShowInfo(String.Format ("{0} - {1}", GetString("SERVERSTATUSDOWN"), GetString("MINIMALINFO")));

			// Get the iFolders.
			GetiFolders( false );
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
		/// Gets the image representing the iFolder type.
		/// </summary>
		/// <param name="disabled"></param>
		/// <param name="shared"></param>
		/// <returns></returns>
		protected string GetiFolderImage( object disabled, object shared , object encrypted)
		{
			if( (bool) disabled)
				return "images/ifolder_16-gray.gif";
			else if( (bool) encrypted)
				return "images/encrypt_ilock2_16.gif";
			
			return ( bool )shared ? "images/ifolder_user_16.gif" : "images/ifolder.png";
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
		/// Gets the navigation url for the ifolder if the 
		/// iFolder is reachable.
		/// </summary>
		/// <param name="ownerID">The ID of the ifolder.</param>
		/// <returns>The URL to navigate to the ifolder.</returns>
		protected string GetiFolderUrl( Object reach, Object iFolderID )
		{
			return (bool) reach? String.Format( "iFolderDetailsPage.aspx?id={0}", iFolderID ) : String.Empty;
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
		/// Event handler that gets called when an ifolder is checked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OniFolderChecked( object source, EventArgs e )
		{
			// Get the data grid row for this ifolder.
			CheckBox checkBox = source as CheckBox;
			DataGridItem item = checkBox.Parent.Parent as DataGridItem;
			string ifolderID = item.Cells[ iFolderIDColumn ].Text;
			if ( ifolderID != "&nbsp;" )
			{
				// iFolder is being checked.
				if ( checkBox.Checked )
				{
					CheckediFolders[ ifolderID ] = item.Cells[ iFolderDisabledColumn ].Text;
				}
				else
				{
					// Remove this member from the list.
					CheckediFolders.Remove( ifolderID );
				}
			}

			// Set the ifolder action buttons.
			//SetActionButtons();
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
			GetiFolders( false );
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

			GetiFolders( false );
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentiFolderOffset += iFolderList.PageSize;
			GetiFolders( false );
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentiFolderOffset = ( ( TotaliFolders - 1 ) / iFolderList.PageSize ) * iFolderList.PageSize;
			GetiFolders( false );
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

			iFolderListFooter.PageFirstClick += new ImageClickEventHandler( PageFirstButton_Click );
			iFolderListFooter.PagePreviousClick += new ImageClickEventHandler( PagePreviousButton_Click );
			iFolderListFooter.PageNextClick += new ImageClickEventHandler( PageNextButton_Click );
			iFolderListFooter.PageLastClick += new ImageClickEventHandler( PageLastButton_Click );

			this.Load += new System.EventHandler(this.Page_Load);
		}

		#endregion
		#region Public Methods

		/// <summary>
        /// Function called to format the input String by keeping newline after
        /// every 80 character.
        /// </summary>
        /// <param name="InputString"></param>
        public static string FormatInputString(String InputString,int InsertIndex)
        {
            int strlenght = InputString.Length;
            string str = "\n";
            int count= strlenght/InsertIndex;
            for(int insert=1;count>0;count--,insert++)
            {
                InputString = InputString.Insert(insert*InsertIndex,str);
            }
            return InputString;
        }

        #endregion
		
	}
}
