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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Text;
using System.Threading;
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
        /// sharing information in enum
        /// </summary>
		public enum Share
		{	
			Sharing = 1,
			EnforcedSharing = 4,
			DisableSharing = 8
		}

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
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;

		/// <summary>
		/// Web Controls.
		/// </summary>
		protected MemberSearch MemberSearch;

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
		protected Literal Orphan;
		
		/// <summary>
		/// Control used to adopt the orphaned ifolders.
		/// </summary>
		protected Button AdoptButton;

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

		protected string currentServerURL;

		protected string iFolderLocation;

		protected static int preference;

		protected static bool AdoptButtonClicked;

		/// <summary>
		/// Display the Server Name, the ifolder belongs
		/// </summary>
		protected Literal ServerName;

		public string UserId;

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
					MemberRightsButton.Enabled =  value;
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
		/// Gets the User ID.
		/// </summary>
		private string UserID
		{
			get { return Request.Params[ "userid" ]; } 
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

		private bool MemberRightsChangeAllowed
		{
			get
			{
				if( preference != -1 && preference != 0xffff)
				{
					UserGroupAdminRights rights = new UserGroupAdminRights(preference);
					if( !rights.ModifyMemberRightAllowed )
					{
						return false;
					}
				}
				return true;
			}
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
			// Pass this page information to create the help link
			TopNav.AddHelpLink(GetString("IFOLDERDETAILS"));
		}

		/// <summary>
		/// Initializes the iFolder detail web controls.
		/// </summary>
		///	<returns>The name of the ifolder.</returns>
		private string GetiFolderDetails()
		{
                        int exp = 0;
                        while(exp < 10)
                        {
	                	try
        	                {
                	        	//Resolve the location of iFolder.
                        	        iFolderLocation = web.GetiFolderLocation (iFolderID);
                                	if(iFolderLocation == null || iFolderLocation == String.Empty || iFolderLocation == "")
                                       	{
                                               	Thread.Sleep(1000);
						exp = exp + 1;
                	                }
                        	        else
					{
						UriBuilder remoteurl = new UriBuilder(iFolderLocation);
						remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
						web.Url = remoteurl.Uri.ToString();
                                                break;
					}
                                }
	                        catch
        	                {
                	                Thread.Sleep(1000);
                        	        if(exp == 10)
                                       	{
                                                Page.Response.Redirect(String.Format("iFolders.aspx"));
                                               	return null;
	                                }
        	                        exp = exp + 1;
                                }
                        }

			iFolderDetails ifolder = null;
			try
			{
				ifolder = web.GetiFolderDetails( iFolderID );
			}
			catch(Exception ex) 
			{
				if(ex.Message.IndexOf("iFolderDoesNotExistException") != -1)
				{
                                        Page.Response.Redirect(String.Format("UserDetails.aspx?id={0}&errormsg={1}",UserID,GetString("ERRORIFOLDERDOESNOTEXIST")));
				}
				else
				{
					web.Url = currentServerURL;
					TopNav.ShowError(GetString("LOGINCONNECTFAILED"));
				}
				return null;
			}

			string loggedinuserid = Session[ "UserID" ] as String;
			preference = web.GetUserGroupRights(loggedinuserid, ifolder.OwnerID);
			if( preference == -1)
				preference = 0xffff;


			string ShortenedName = null;
			int ShortenedLength = 50;
			if(ifolder.Name.Length > ShortenedLength)
			{
				// make it of desired length
				ShortenedName = web.GetShortenedName(ifolder.Name, ShortenedLength);
			}
			Name.Text = (ifolder.Name.Length > ShortenedLength) ? ShortenedName : ifolder.Name;

			Description.Text = ifolder.Description;
			Owner.Text = ifolder.OwnerFullName;
			Owner.NavigateUrl= String.Format( "UserDetails.aspx?id={0}", ifolder.OwnerID );
			//Size.Text = Utils.ConvertToUnitString( ifolder.Size, true, rm );
			Directories.Text = ifolder.DirectoryCount.ToString();
			Files.Text = ifolder.FileCount.ToString();
			
			string IsOrphaned = web.IsOrphanediFolder(iFolderID);
			
			if(IsOrphaned.Equals(""))
			{
				Orphan.Text = GetString( "NO" );
				AdoptButton.Text = GetString("ADOPT");
				AdoptButton.Visible = false;
			}
			else
			{
				//it has returned the previous owner id of this orphaned ifolder
				Orphan.Visible = true;
				Orphan.Text = GetString( "YES" );
				AdoptButton.Visible = true;
				
                                string EncryptAlgorithm = ifolder.EncryptionAlgorithm;
				if(!(EncryptAlgorithm == null || (EncryptAlgorithm == String.Empty)) )
				{
					// It is an encrypted ifolder
					AdoptButton.Enabled = false;
				}
				else
					AdoptButton.Enabled = true;
			}	

			LastModified.Text = ( ifolder.LastModified == DateTime.MinValue ) ? 
				Utils.ToDateTimeString( DateTime.Now ) : 
				Utils.ToDateTimeString( ifolder.LastModified );

			// Allow the browser to break up the path on separator boundries.
			string FullUnManagedPath = ifolder.UnManagedPath.Replace(
				Path.DirectorySeparatorChar.ToString(), 
				Path.DirectorySeparatorChar.ToString() + "<WBR>" );
			string tmp = FullUnManagedPath ;
			if (FullUnManagedPath.Length > 170 )
			{
				tmp = web.GetShortenedName(FullUnManagedPath, 170);
			}

			//Adding Server Name 
			iFolderServer[] list = web.GetServers();
			string name= null;
            ServerName.Text = null;
			foreach( iFolderServer server in list )
			{
			   if (server.PublicUrl == iFolderLocation)
			    {
			         name = server.Name;
			         break;
			    }
			}
			if (name != null)
			{
               ServerName.Text = name;
			}

			UnManagedPath.Text = tmp;


//			web.Url = currentServerURL;

			string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
			if(!(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty)) || !MemberRightsChangeAllowed)
			{
				// It is an encrypted ifolder
				MemberAddButton.Enabled = false;
			}

			return ifolder.Name;
		}

		/// <summary>
		/// Gets the member list view for the specified ifolder.
		/// </summary>
		/// <returns></returns>
		private DataView GetiFolderMemberList(bool AdoptButtonClicked)
		{
			DataTable dt = new DataTable();
			DataRow dr;
			iFolderUserSet userList = null;
			iFolderUserSet memberList = null;

			dt.Columns.Add( new DataColumn( "VisibleField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "OwnerField", typeof( bool ) ) );
			dt.Columns.Add( new DataColumn( "IDField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "FullNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "UserNameField", typeof( string ) ) );
			dt.Columns.Add( new DataColumn( "RightsField", typeof( string ) ) );
                        iFolderLocation = web.GetiFolderLocation(iFolderID);
                        UriBuilder remoteurl = new UriBuilder(iFolderLocation);
                        remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                        web.Url = remoteurl.Uri.ToString();
			try
			{
				
				memberList = 
					web.GetMembers( 
					iFolderID, 
					CurrentMemberOffset, 
					iFolderMemberList.PageSize );
			}
			catch
			{
				return new DataView( dt );
			}
				

			foreach( iFolderUser member in memberList.Items )
			{
				dr = dt.NewRow();
				dr[ 0 ] = true;
				dr[ 1 ] = member.IsOwner;
				dr[ 2 ] = member.ID;
				dr[ 3 ] = member.FullName;
				dr[ 4 ] = member.UserName;
				if(member.MemberRights == Rights.Admin)
				{
					if(member.IsOwner)
					{
						dr[ 5 ] = GetString("OWNER");
					}
					else
					{
						dr[ 5 ] = GetString("FULLCONTROL");
					}
				}
				else
				{
					string attribute = member.MemberRights.ToString();
                        		if ( attribute == Rights.ReadWrite.ToString())
                        		{
                                		dr[ 5 ] = GetString("READWRITE");

                        		}
                        		else if ( attribute == Rights.ReadOnly.ToString() )
                        		{
		                                dr[ 5 ] = GetString("READONLY"); 
                		        }
				}

				dt.Rows.Add( dr );
			}
			
			//if Adopt button was clicked, then show all the users from domain
			if(AdoptButtonClicked)
			{
	                        // userList if adoptButton was clicked
	                        userList = web.GetUsers(
        	                                CurrentMemberOffset,
                	                        iFolderMemberList.PageSize );
	
				int NoOfRows = dt.Rows.Count;	
					
				foreach( iFolderUser user in userList.Items )
				{
					bool UserAlready = false;
					for (int i = 0; i < NoOfRows; i++)
					{
						if(user.ID == (String)dt.Rows[i][2])
						{
							UserAlready = true;
							break;
						}
					}	
						
					if(!UserAlready)
					{
						dr = dt.NewRow();
						dr[0] = true;
						dr[1] = false;
						dr[2] = user.ID;
						dr[3] = user.FullName;
						dr[4] = String.Format("<font color=red>" + GetString("NOTAPPLICABLE") + "</font>");
						
						dt.Rows.Add(dr);
						
					}
				
				}	
				
				AdoptButton.Enabled = false;
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
				dr[ 5 ] = String.Empty;

				dt.Rows.Add( dr );
			}

			// Remember the total number of users.
			if(AdoptButtonClicked)
				TotaliFolderMembers = userList.Total;
			else	
				TotaliFolderMembers = memberList.Total;

			// Build the data view from the table.
			return new DataView( dt );
		}

		/// <summary>
		/// Gets the iFolder members
		/// </summary>
		private void GetiFolderMembers()
		{
			bool AdoptButtonClicked = false;
			iFolderMemberList.DataSource = GetiFolderMemberList(AdoptButtonClicked);
			iFolderMemberList.DataBind();
			SetPageButtonState();
		}
		
		/// <summary>
		/// Gets the iFolder members
		/// </summary>
		private void GetiFolderMembers(bool AdoptButtonClicked)
 		{
 			iFolderMemberList.DataSource = GetiFolderMemberList(AdoptButtonClicked);
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
			if ( attribute == GetString( "FULLCONTROL" ) )
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
			currentServerURL = web.Url;

			remoteweb = new iFolderAdmin();
			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;
			remoteweb.Url = web.Url;
			iFolderLocation = null;
    
			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				iFolderMemberList.Columns[ 3 ].HeaderText = GetString( "TYPE" );
				iFolderMemberList.Columns[ 4 ].HeaderText = GetString( "NAME" );
				iFolderMemberList.Columns[ 5 ].HeaderText = GetString( "USERNAME" );
				iFolderMemberList.Columns[ 6 ].HeaderText = GetString( "RIGHTS" );

				DescriptionButton.Text = GetString( "SAVE" );
				AdoptButton.Text = GetString( "ADOPT" );
				MemberDeleteButton.Text = GetString( "DELETE" );
				MemberAddButton.Text = GetString( "ADD" );
				MemberOwnerButton.Text = GetString( "OWNER" );
				MemberRightsButton.Text = GetString( "SET" );

				MemberRightsList.Items[ 0 ].Text = GetString( "READONLY" );
				MemberRightsList.Items[ 1 ].Text = GetString( "READWRITE" );
				MemberRightsList.Items[ 2 ].Text = GetString( "FULLCONTROL" );

				AdoptButtonClicked = false;

				// Initialize state variables.
				CurrentMemberOffset = 0;
				TotaliFolderMembers = 0;
				MembersChecked = false;
				CheckedMembers = new Hashtable();
			}
					/// Disable all the buttons...
					EnableMemberActionButtons = false;//MemberRightsChangeAllowed;
					EnableOwnerActionButton = false;//MemberRightsChangeAllowed;
		}

		       /// <summary>
        /// Unloads the current page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Page_Unload(object sender, System.EventArgs e)
		{
		        web.Url = currentServerURL;
		}
		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender( object sender, EventArgs e )
		{
//			TopNav.ShowInfo(String.Format("{0} -> {1}", web.Url, iFolderLocation));
			// Get the iFolder Details.
			string ifolderName = GetiFolderDetails();
			if(ifolderName != null)
			{
				// Show the iFolder member list.
				GetiFolderMembers();

				// Fill in the policy information.
				Policy.GetiFolderPolicies();

				// Build the breadcrumb list.
				BuildBreadCrumbList( ifolderName );
			}

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
		
		/// <summary>
		/// whether owner button can be enabled or not based on encryption policy applied on the user.
		/// </summary>
		private bool GetOwnerButtonActionStatus(string userID)
                {
			// if on system level , encryption is enforced , then no user can be assigned owner of a shared ifolder. 

                        try
                        {
                                bool UserEncryptionEnforced = web.IsUserOrSystemEncryptionEnforced(userID);
                                bool LinkStatus = ! UserEncryptionEnforced ;
				
                                return LinkStatus ;
                        }
                        catch
                        {
                                return true;
                        }
                        //return true;
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
			bool DisableSharing = false;
                        string ifolderLocation = web.GetiFolderLocation (iFolderID);
                        UriBuilder remoteurl = new UriBuilder(ifolderLocation);
                        remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                        web.Url = remoteurl.Uri.ToString();
		
			/// First check whether policy allows this iFolder to share itself or not 
			try
			{
				DisableSharing = web.GetSharingPolicy(iFolderID);
			}
			catch (Exception ex)
			{
				TopNav.ShowError(String.Format("{0}, {1}",ex.Message, web.Url));
				return;
			}
			
			if(DisableSharing == false)
			{
				TopNav.ShowError( GetString( "SHARINGPOLICYVIOLATION") );
				return;
			}
			else
			{
				 // Do a utf-8 encoding as there may be multibyte characters in the name
                                UTF8Encoding utf8Name = new UTF8Encoding();
				byte[] EncodediFolderNameInByte = utf8Name.GetBytes((Name.Text));
				string iFolderNameBase64 = Convert.ToBase64String(EncodediFolderNameInByte);
			
				Page.Response.Redirect( 
					String.Format( 
						"MemberSelect.aspx?op=addmember&id={0}&name={1}&desc={2}&fn={3}", 
						iFolderID, 
						iFolderNameBase64,
						Description.Text,
						Owner.Text), 
					true );
			}	
		}

		/// <summary>
		/// Event handler that gets called when the check all members checkbox is selected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void AllMembersChecked( object sender, EventArgs e )
		{
			CheckBox checkBox = sender as CheckBox;
			string memberID = null;
			foreach( DataGridItem item in iFolderMemberList.Items )
			{
				// In order to be checked, the row must not be empty or must not be
				// the owner of the ifolder.
				memberID = item.Cells[ 0 ].Text;
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

			string PossibleOwner = null;
			if(CheckedMembers.Count == 1)
			{
				// before enabling owner button lets get the memberid to check if he can be owner or not 
				foreach( string tmpmemberID in CheckedMembers.Keys )
				{
					PossibleOwner = tmpmemberID;	
				}
			}

			// See if there are any checked members.
			EnableMemberActionButtons = (( CheckedMembers.Count > 0 ) && (!AdoptButton.Visible || AdoptButton.Enabled) && MemberRightsChangeAllowed);
			EnableOwnerActionButton = (( CheckedMembers.Count == 1 ) && GetOwnerButtonActionStatus(PossibleOwner) && (!AdoptButton.Visible && AdoptButton.Enabled) && MemberRightsChangeAllowed); 
			MembersChecked = checkBox.Checked;

			// Rebind the data source with the new data.
			GetiFolderMembers(AdoptButton.Visible && !AdoptButton.Enabled && AdoptButtonClicked);
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
					//check for no of ifolders per user limit policy for the user who is taking up Ownership
                                        if ( web.GetiFolderLimitPolicyStatus(memberID) != 1)
                                        {
                                                TopNav.ShowError( GetString( "ERRORIFOLDERTRANSFEREXCEPTION" ));
                                                return;
                                        }
				
					//Check for the location of this ifolder, if location is not on this server then change the URL
                        		iFolderLocation = web.GetiFolderLocation(iFolderID);
                        		UriBuilder remoteurl = new UriBuilder(iFolderLocation);
                        		remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                        		web.Url = remoteurl.Uri.ToString();
					
					web.SetiFolderOwner( iFolderID, memberID, (AdoptButton.Visible && !AdoptButton.Enabled) );
				}
				catch ( Exception ex )
				{
					TopNav.ShowError( GetString( "ERRORCANNOTCHANGEOWNER" ), ex );
					return;
				}

				// Show the new owner in the page.
				Owner.Text = CheckedMembers[ memberID ] as string;
				Owner.NavigateUrl = String.Format( "UserDetails.aspx?id={0}", memberID );
				
				//if orphaned property was removed then the buttons are changed 
				string IsOrphaned = web.IsOrphanediFolder(iFolderID);
			
				if(IsOrphaned.Equals(""))
				{
					Orphan.Text = GetString( "NO" );
					AdoptButton.Text = GetString("ADOPT");
					AdoptButton.Visible = false;
				}
			}

			// Clear the checked members.
			CheckedMembers.Clear();
			MembersChecked = false;

			// Disable the action buttons.
			EnableMemberActionButtons = EnableOwnerActionButton = false;
			
			//string ifolderName = GetiFolderDetails();

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
		
			string PossibleOwner = null;
			if(CheckedMembers.Count == 1)
			{
				// before enabling owner button lets get the memberid to check if he can be owner or not 
				foreach( string tmpmemberID in CheckedMembers.Keys )
				{
					PossibleOwner = tmpmemberID;	
				}
			}
		
			// See if there are any checked members.
			EnableMemberActionButtons = (( CheckedMembers.Count > 0 ) && (!AdoptButton.Visible || AdoptButton.Enabled) && MemberRightsChangeAllowed);
			EnableOwnerActionButton = ( CheckedMembers.Count == 1 && GetOwnerButtonActionStatus(PossibleOwner) && !(AdoptButton.Visible && AdoptButton.Enabled) && MemberRightsChangeAllowed );
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
			GetiFolderMembers(AdoptButton.Visible && !AdoptButton.Enabled);
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

			GetiFolderMembers(AdoptButton.Visible && !AdoptButton.Enabled);
		}

		/// <summary>
		/// Event that first when the PageNextButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageNextButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentMemberOffset += iFolderMemberList.PageSize;
			GetiFolderMembers(AdoptButton.Visible && !AdoptButton.Enabled);
		}

		/// <summary>
		/// Event that first when the PageLastButton is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void PageLastButton_Click( object source, ImageClickEventArgs e)
		{
			CurrentMemberOffset = ( ( TotaliFolderMembers - 1 ) / iFolderMemberList.PageSize ) * iFolderMemberList.PageSize;
			GetiFolderMembers(AdoptButton.Visible && !AdoptButton.Enabled);
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

		/// <summary>
		/// Event handler that gets called when the Adopt button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void AdoptOrphanediFolder( object sender, EventArgs e )
		{
			AdoptButtonClicked = true;
			GetiFolderMembers(AdoptButtonClicked);

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
			this.Unload += new System.EventHandler (this.Page_Unload);
		}
		#endregion
	}
}
