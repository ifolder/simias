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
*                 $Author: Anil Kumar (kuanil@novell.com)
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
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Resources;
using System.Web.Services.Protocols;
using System.Net;
//using System.Text;


namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Item History Page
	/// </summary>
	public class StatusPage : Page
	{
		
		/// <summary>
		/// Message Box
		/// </summary>
		protected TopNavigation TopNav;
		
		/// <summary>
		/// Header control 
		/// </summary>
		//protected HeaderControl Head;

		/// <summary>
		/// The Accept Button
		/// </summary>
		protected Button RepeatButton;
		
		/// <summary>
		/// The Deny Button
		/// </summary>
		protected Button OKButton;

		/// <summary>
		/// The Deny Button
		/// </summary>
		protected Label StatusLabel;

		/// <summary>
		/// The Deny Button
		/// </summary>
		protected Label SuccessLabel;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

                /// <summary>
                /// Gets the starting member list page of the previous owner select page.
                /// </summary>
                private string SecondaryAdmin
                {
                        get
                        {
                                string param = Request.Params[ "secondaryadmin" ];
                                return ( ( param == null ) || ( param == String.Empty ) ) ? "" : param;
                        }
                }

                /// <summary>
                /// Gets the starting member list page of the previous owner select page.
                /// </summary>
                private string op
                {
                        get
                        {
                                string param = Request.Params[ "op" ];
                                return ( ( param == null ) || ( param == String.Empty ) ) ? "" : param;
                        }
                }

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, EventArgs e)
		{
			// connection
			web = Session[ "Connection" ] as iFolderAdmin;

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if (!IsPostBack)
			{
				// data
				BindData();
								
				// strings
				OKButton.Text = GetString("OK");
				
			}
		}

		private void ShowStatus()
		{
			string StatusMessage = Request.QueryString.Get("status");
			string SecondaryAdmin = Request.QueryString.Get("secondaryadmin");
			string GroupName = Request.QueryString.Get("groupname");
			iFolderUser user = web.GetUser( SecondaryAdmin );
			if(StatusMessage.Equals("SUCCESS"))
			{
				SuccessLabel.BackColor = System.Drawing.Color.LightGreen;
				SuccessLabel.Text = op + "  :  " + "SUCCESSFUL";
				if( op == "ADD")
				{
				
					string SuccessText = String.Format(GetString("GROUPRIGHTSADDSUCCESSFUL"),"<b>",user.FullName,"</b>", "<b>", GroupName, "</b>");
					StatusLabel.Text = SuccessText;
				}
				else if( op == "EDIT")
				{
					string SuccessText = String.Format(GetString("GROUPRIGHTSEDITSUCCESSFUL"),"<b>",user.FullName,"</b>", "<b>", GroupName, "</b>");
					StatusLabel.Text = SuccessText;
				}
				RepeatButton.Text = GetString("REPEAT");
			}	
			else
			{
				StatusLabel.Text = String.Format(GetString("GROUPRIGHTSADDFAILURE"),"<b>",user.FullName,"</b>", "<b>", GroupName, "</b>");
				RepeatButton.Text = GetString("TRYAGAIN");
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

			// Build the breadcrumb list.
			BuildBreadCrumbList();
		}

                /// <summary>
		/// Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			Control body = FindControl( "ifolders" );
			TopNav.AddBreadCrumb( GetString( "SYSTEM" ), "SystemInfo.aspx" );
			TopNav.AddBreadCrumb( GetString( "SECONDARYADMIN" ), null );
			if ( body != null )
			{
				body.ID = "system";
			}	
			TopNav.SetActivePageTab( TopNavigation.PageTabs.System );
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			ShowStatus();
		}	


		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString(string key)
		{
			return rm.GetString(key);
		}

		#region Web Form Designer

		/// <summary>
		/// On Initialize
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Initialize Components
		/// </summary>
		private void InitializeComponent()
		{    
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}
			this.Load += new System.EventHandler(this.Page_Load);
			this.RepeatButton.Click += new EventHandler(RepeatButton_Click);
			this.OKButton.Click += new EventHandler(OKButton_Click);
		}

		#endregion

		/// <summary>
		/// Accept Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RepeatButton_Click(object sender, EventArgs e)
		{
			string operation = "";
			if (op == "EDIT" || op == "ADD")
			{
				operation = "editsecondaryadmin";
			}
                        Response.Redirect(
                                    String.Format(
                                    "MemberSelect.aspx?op={0}&secondaryadminid={1}",
                                     operation, SecondaryAdmin));

		}
		
		/// <summary>
		/// Deny Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OKButton_Click(object sender, EventArgs e)
		{
			string url = "SystemInfo.aspx";
			// redirect
			Response.Redirect(url);
		}
	}
}
