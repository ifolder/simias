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
using System.Web.Services.Protocols;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	///	Home Context Control
	/// </summary>
	public class HomeContextControl : UserControl
	{
		/// <summary>
		/// Home Link
		/// </summary>
		protected HyperLink HomeLink;
		
		/// <summary>
		/// Search Category
		/// </summary>
		protected DropDownList SearchCategory;

		/// <summary>
		/// Search Pattern
		/// </summary>
		protected TextBox SearchPattern;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;
	
		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;

		#region Events
		
		/// <summary>
		/// On Search Event
		/// </summary>
		public event EventHandler Search;
		
		#endregion

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// connection
			web = (iFolderWeb) Session["Connection"];

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				// data
				BindData();

				// strings
				HomeLink.Text = GetString("HOME");
				
				// categories
				SearchCategory.Items.Add(new ListItem(GetString("ALL"), iFolderCategory.All.ToString()));
				SearchCategory.Items.Add(new ListItem(GetString("RECENT"), iFolderCategory.Recent.ToString()));
				SearchCategory.Items.Add(new ListItem(GetString("OWNED"), iFolderCategory.Owned.ToString()));
				SearchCategory.Items.Add(new ListItem(GetString("SHARED"), iFolderCategory.Shared.ToString()));
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
		}

		/// <summary>
		/// Handle Exceptions
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool HandleException(Exception e)
		{
			bool result = true;

			string type = WebUtility.GetExceptionType(e);

			// types
			switch(type)
			{
				default:
					result = false;
					break;
			}

			return result;
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
		/// Search Pattern
		/// </summary>
		public string Pattern
		{
			get { return SearchPattern.Text; }
		}

		/// <summary>
		/// Search Category
		/// </summary>
		public iFolderCategory Category
		{
			get
			{
				iFolderCategory result = iFolderCategory.All;

				if (SearchCategory.SelectedItem != null)
				{
					result = (iFolderCategory) Enum.Parse(typeof(iFolderCategory), SearchCategory.SelectedItem.Value);
				}

				return result;
			}
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
			this.Load += new System.EventHandler(this.Page_Load);
			this.SearchPattern.TextChanged += new EventHandler(SearchPattern_TextChanged);
			this.SearchCategory.SelectedIndexChanged += new EventHandler(SearchCategory_SelectedIndexChanged);
		}
		
		#endregion

		/// <summary>
		/// Search Pattern Changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SearchPattern_TextChanged(object sender, EventArgs e)
		{
			Search(this, null);
		}

		/// <summary>
		/// Search Category Changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SearchCategory_SelectedIndexChanged(object sender, EventArgs e)
		{
			Search(this, null);
		}
	}

	/// <summary>
	/// iFolder Category
	/// </summary>
	public enum iFolderCategory
	{
		/// <summary>
		/// All iFolders
		/// </summary>
		All,

		/// <summary>
		/// Recently Shared iFolders
		/// </summary>
		Recent,

		/// <summary>
		/// Owned iFolders
		/// </summary>
		Owned,
		
		/// <summary>
		/// Shared iFolders
		/// </summary>
		Shared
	}
}
