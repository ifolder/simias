/***********************************************************************
 *  $RCSfile: TopNavigation.ascx.cs,v $
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
namespace Novell.iFolderWeb.Admin
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Resources;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	/// <summary>
	///		Summary description for TopNavigation.
	/// </summary>
	public class TopNavigation : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Anchor controls.
		/// </summary>
		protected HtmlAnchor UserLink;

		/// <summary>
		/// Anchor controls.
		/// </summary>
		protected HtmlAnchor iFolderLink;

		/// <summary>
		/// Anchor controls.
		/// </summary>
		protected HtmlAnchor ServerLink;

		/// <summary>
		/// Anchor controls.
		/// </summary>
		protected HtmlAnchor SystemLink;
        
		#endregion

		#region Properties
		#endregion

		#region Private Methods

		/// <summary>
		/// Page_Load()
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{

				Control body = Page.FindControl( "users" );
				if ( body != null )
				{
					UserLink.HRef = String.Empty;
					iFolderLink.HRef = "iFolders.aspx";
					ServerLink.HRef = "Servers.aspx";
					SystemLink.HRef = "SystemInfo.aspx";
				}
				else
				{
					body = Page.FindControl( "ifolders" );
					if ( body != null )
					{
						UserLink.HRef = "Users.aspx";
						iFolderLink.HRef = String.Empty;
						ServerLink.HRef = "Servers.aspx";
						SystemLink.HRef = "SystemInfo.aspx";
					}
					else
					{
						body = Page.FindControl( "system" );
						if ( body != null )
						{
							UserLink.HRef = "Users.aspx";
							iFolderLink.HRef = "iFolders.aspx";
							ServerLink.HRef = "Servers.aspx";
							SystemLink.HRef = String.Empty;
						}
						else
						{
							UserLink.HRef = "Users.aspx";
							iFolderLink.HRef = "iFolders.aspx";
							ServerLink.HRef = String.Empty;
							SystemLink.HRef = "SystemInfo.aspx";
						}
					}
				}
			}
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
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
