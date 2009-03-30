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
using System.Text;
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
	/// Summary description for CreateiFolder.
	/// </summary>
	public class CreateiFolder : System.Web.UI.Page
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
		/// Web controls.
		/// </summary>
		protected Button NextButton;

		/// <summary>
		/// Web controls.
		/// </summary>
		protected Button CancelButton;


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
		protected TextBox Description;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the owner's full name.
		/// </summary>
		private string FullName
		{
			get
			{
				string param = Request.Params[ "fn" ];
				return ( param == null ) ? String.Empty : param;
			}
		}

		/// <summary>
		/// Gets the iFolder Description.
		/// </summary>
		private string iFolderDescription
		{
				// Now , through asp pages , for ifolderdesc field ,only utf8 encoded and base64 converted strings should be sent
				// It will decode the base64 string sent through URL
                  	        get
	                        {
        	                        string iFolderDescBase64 = Request.Params[ "desc" ] ;
                	                if ( iFolderDescBase64 == null )
                        	        {
                                	        return String.Empty;
	                                }
        	                        string param;
                	                try{
	
        	                                byte[] iFolderDescInByte = Convert.FromBase64String(iFolderDescBase64);
                	                        UTF8Encoding utf8Name = new UTF8Encoding();
                        	                param = utf8Name.GetString(iFolderDescInByte);
                                	}catch(Exception ex)
        	                        {
     	                                   TopNav.ShowError( String.Format ( "MULTIBYTEERROR {0}",ex.Message ) );
                	                   throw ex;
                        	        }
	
        	                        return param;
                	        }

		}

		/// <summary>
		/// Gets the user or iFolder name.
		/// </summary>
		private string iFolderName
		{
			// Now , through asp pages , for ifoldername field ,only utf8 encoded and base64 converted strings should be sent
			get
			{
				string iFolderNameBase64 = Request.Params[ "name" ] ;
                                if ( iFolderNameBase64 == null )
                                {
					return String.Empty;
                                }
                                string param;
                                try{

                                        byte[] iFolderNameInByte = Convert.FromBase64String(iFolderNameBase64);
                                        UTF8Encoding utf8Name = new UTF8Encoding();
                                        param = utf8Name.GetString(iFolderNameInByte);
                                }catch(Exception ex)
                                {
                                        TopNav.ShowError( String.Format ( "MULTIBYTEERROR {0}",ex.Message ) );
                                        throw ex;
                                }

				return param;
			}
		}

		/// <summary>
		/// Gets the owner of the iFolder.
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
		/// Gets the starting owner list page of the owner select page.
		/// </summary>
		private string OwnerListPage
		{
			get 
			{ 
				string param = Request.Params[ "pg" ];
				return ( ( param == null ) || ( param == String.Empty ) ) ? "0" : param;
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

		#endregion

		#region Private Methods

		/// <summary>
		/// Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			// Show the proper tab.
			Control body = FindControl( "ifolders" );

			// Create ifolder is called from both the userdetails page and the main ifolder list
			// page. Need to determine which one we came from.
			Uri uri = new Uri( ReferringPage );
			if ( uri.AbsolutePath.EndsWith( "UserDetails.aspx" ) )
			{
				TopNav.AddBreadCrumb( GetString( "USERS" ), "Users.aspx" );
				TopNav.AddBreadCrumb( FullName, String.Format( "UserDetails.aspx?id={0}", iFolderOwner ) );
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
			// Pass this page information to create the help link 
			TopNav.AddHelpLink(GetString("CREATENEWIFOLDER"));
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
				string param = Request.Params[ "ref" ];
				ReferringPage = ( ( param == null ) || ( param == String.Empty ) ) ?
					Page.Request.UrlReferrer.ToString() : param;

				// Initialize the localized fields.
				CancelButton.Text = GetString( "CANCEL" );
				NextButton.Text = GetString( "NEXT" );
				NameLabel.Text = GetString( "NAMETAG" );
				DescriptionLabel.Text = GetString( "DESCRIPTIONTAG" );

				// Initialize state variables.
				Name.Text = iFolderName;
				Description.Text = iFolderDescription;
				SetFocus( Name );

				// If there is a name, enable the next button.
				if ( Name.Text.Length > 0 )
				{
					NextButton.Enabled = true;
				}
			}
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender(object sender, EventArgs e)
		{
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
	
		#endregion

		#region Protected Methods

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
		/// Event handler for the next button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void NextButton_Clicked( Object sender, EventArgs e )
		{
			// Create ifolder is called from both the userdetails page and the main ifolder list
			// page. Need to determine which one we came from.
			if(Name.Text.Trim().Length > 0)
                        {
				// Do a utf-8 encoding as there may be multibyte characters in the name
				UTF8Encoding utf8Name = new UTF8Encoding();
				byte[] EncodediFolderNameInByte = utf8Name.GetBytes(Name.Text.Trim());
				string iFolderNameBase64 = Convert.ToBase64String(EncodediFolderNameInByte);

				byte[] EncodediFolderDescInByte = utf8Name.GetBytes(Description.Text);
				string iFolderDescBase64 = Convert.ToBase64String(EncodediFolderDescInByte);

				Uri uri = new Uri( ReferringPage );
				if ( uri.AbsolutePath.EndsWith( "UserDetails.aspx" ) )
				{
					Page.Response.Redirect( 
						String.Format( 
							"MemberSelect.aspx?op=createifolder&name={0}&desc={1}&owner={2}&fn={3}&ref={4}",
							iFolderNameBase64, iFolderDescBase64, iFolderOwner, FullName, ReferringPage ), 
						true );
				}
				else
				{
					Page.Response.Redirect( 
						String.Format( 
							"OwnerSelect.aspx?name={0}&desc={1}{2}&pg={3}&ref={4}",
							iFolderNameBase64, 
							iFolderDescBase64, 
							( iFolderOwner != String.Empty ) ? "&owner=" + iFolderOwner : String.Empty,
							OwnerListPage, 
							ReferringPage ), 
						true );
				}
			}
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

			this.Load += new System.EventHandler( this.Page_Load );
		}

		#endregion
	}
}
