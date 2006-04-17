/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Threading;
using System.Resources;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	///	Context Control
	/// </summary>
	public class Context : UserControl
	{
		/// <summary>
		/// The Home Link
		/// </summary>
		protected HyperLink HomeLink;

		/// <summary>
		/// iFolder Name
		/// </summary>
		protected Literal iFolderNameLiteral;

		/// <summary>
		/// The Browse Link
		/// </summary>
		protected HyperLink BrowseLink;

		/// <summary>
		/// The Search Link
		/// </summary>
		protected HyperLink SearchLink;

		/// <summary>
		/// The Details Link
		/// </summary>
		protected HyperLink DetailsLink;

		/// <summary>
		/// The Sharing Link
		/// </summary>
		protected HyperLink SharingLink;

		/// <summary>
		/// The History Link
		/// </summary>
		protected HyperLink HistoryLink;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;
	
		/// <summary>
		/// Current iFolder ID
		/// </summary>
		private string ifolderID;

		/// <summary>
		/// Page Init
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Init(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// query
			ifolderID = Request.QueryString.Get("iFolder");

			// localization
			rm = (ResourceManager) Application["RM"];
			
			if (!IsPostBack)
			{
				// strings
				HomeLink.Text = GetString("IFOLDERS");
				BrowseLink.Text = GetString("BROWSE");
				SearchLink.Text = GetString("SEARCH");
				DetailsLink.Text = GetString("DETAILS");
				SharingLink.Text = GetString("SHARING");
				HistoryLink.Text = GetString("HISTORY");

				// links
				BrowseLink.NavigateUrl = "Browse.aspx?iFolder=" + ifolderID;
				SearchLink.NavigateUrl = "Search.aspx?iFolder=" + ifolderID;
				DetailsLink.NavigateUrl = "Details.aspx?iFolder=" + ifolderID;
				SharingLink.NavigateUrl = "Members.aspx?iFolder=" + ifolderID;
				HistoryLink.NavigateUrl = "History.aspx?iFolder=" + ifolderID;
			}
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString(string key)
		{
			return WebUtility.GetString(key, rm);
		}

		#region Properties

		/// <summary>
		/// iFolder Name
		/// </summary>
		public string iFolderName
		{
			get { return iFolderNameLiteral.Text; }
			set { iFolderNameLiteral.Text = value; }
		}

		#endregion

		#region Web Form Designer
		
		/// <summary>
		/// On Intialize
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
			this.Init += new EventHandler(Page_Init);
			this.Load += new EventHandler(Page_Load);
		}

		#endregion
	}
}
