/***********************************************************************
 *  $RCSfile: MemberSearch.ascx.cs,v $
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
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	/// <summary>
	///		Summary description for MemberSearch.
	/// </summary>
	public class MemberSearch : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Search attributes.
		/// </summary>
		protected DropDownList SearchAttribute;

		/// <summary>
		/// Search operations.
		/// </summary>
		protected DropDownList SearchOp;

		/// <summary>
		/// Search button control.
		/// </summary>
		public Button SearchButton;

		/// <summary>
		/// Search text.
		/// </summary>
		public HtmlInputText SearchName;

		#endregion

		#region Properties
		#endregion

		#region Private Methods

        /// <summary>
        /// Page_Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				SearchButton.Text = GetString( "SEARCH" );

				SearchAttribute.Items[ 0 ].Text = GetString( "USERNAME" );
				SearchAttribute.Items[ 1 ].Text = GetString( "FIRSTNAME" );
				SearchAttribute.Items[ 2 ].Text = GetString( "LASTNAME" );
				SearchAttribute.SelectedValue = GetString( "USERNAME" );

				SearchOp.Items[ 0 ].Text = GetString( "BEGINSWITH" );
				SearchOp.Items[ 1 ].Text = GetString( "ENDSWITH" );
				SearchOp.Items[ 2 ].Text = GetString( "CONTAINS" );
				SearchOp.Items[ 3 ].Text = GetString( "EQUALS" );
				SearchOp.SelectedValue = GetString( "BEGINSWITH" );

				// Set focus to the inbox control.
				SetFocus( SearchName );

				// Set the javascript function that will handle key presses.
				SearchName.Attributes[ "OnKeyDown" ] = "return SubmitKeyDown(event, '" + SearchButton.ClientID + "');";
			}
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
		/// Default action for enter key.
		/// </summary>
		/// <returns></returns>
		protected string GetKeyDown()
		{
			return "SubmitKeyDown(event, " + SearchButton.ClientID + ")";
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

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the search enumeration attribute from the web selection.
		/// </summary>
		/// <returns>SearchProperty enum type.</returns>
		public SearchProperty GetSearchAttribute()
		{
			SearchProperty searchType = SearchProperty.UserName;
			string attribute = SearchAttribute.SelectedValue;

			if ( attribute == GetString( "USERNAME" ) )
			{
				searchType = SearchProperty.UserName;
			}
			else if ( attribute == GetString( "FIRSTNAME" ) )
			{
				searchType = SearchProperty.FirstName;
			}
			else
			{
				searchType = SearchProperty.LastName;
			}

			return searchType;
		}

		/// <summary>
		/// Gets the search enumeration operation from the web selection.
		/// </summary>
		/// <returns>SearchOperation enum type.</returns>
		public SearchOperation GetSearchOperation()
		{
			SearchOperation searchOp = SearchOperation.BeginsWith;
			string attribute = SearchOp.SelectedValue;

			if ( attribute == GetString( "BEGINSWITH" ) )
			{
				searchOp = SearchOperation.BeginsWith;
			}
			else if ( attribute == GetString( "ENDSWITH" ) )
			{
				searchOp = SearchOperation.EndsWith;
			}
			else if ( attribute == GetString( "CONTAINS" ) )
			{
				searchOp = SearchOperation.Contains;
			}
			else
			{
				searchOp = SearchOperation.Equals;
			}

			return searchOp;
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
