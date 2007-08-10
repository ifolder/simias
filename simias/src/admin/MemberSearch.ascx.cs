/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
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
 |
 | Author: Mike Lasky (mlasky@novell.com)
 |***************************************************************************/

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
		protected DropDownList SearchAttributeList;

		/// <summary>
		/// Search operations.
		/// </summary>
		protected DropDownList SearchOpList;

		/// <summary>
		/// Search button control.
		/// </summary>
		protected Button SearchButton;

		/// <summary>
		/// Search text.
		/// </summary>
		protected HtmlInputText SearchNameTextBox;


		/// <summary>
		/// Event that notifies consumer that the search button has been clicked.
		/// </summary>
		public event EventHandler Click = null;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the search enumeration attribute from the web selection.
		/// </summary>
		public SearchProperty SearchAttribute
		{
			get
			{
				SearchProperty searchType = SearchProperty.UserName;
				string attribute = SearchAttributeList.SelectedValue;

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
		}

		/// <summary>
		/// Gets the search name value.
		/// </summary>
		public string SearchName
		{
			get { return SearchNameTextBox.Value; }
		}

		/// <summary>
		/// Gets the search enumeration operation from the web selection.
		/// </summary>
		/// <returns>SearchOperation enum type.</returns>
		public SearchOperation SearchOperation
		{
			get
			{
				SearchOperation searchOp = SearchOperation.BeginsWith;
				string attribute = SearchOpList.SelectedValue;

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
		}

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

				SearchAttributeList.Items[ 0 ].Text = GetString( "USERNAME" );
				SearchAttributeList.Items[ 1 ].Text = GetString( "FIRSTNAME" );
				SearchAttributeList.Items[ 2 ].Text = GetString( "LASTNAME" );
				SearchAttributeList.SelectedValue = GetString( "USERNAME" );

				SearchOpList.Items[ 0 ].Text = GetString( "BEGINSWITH" );
				SearchOpList.Items[ 1 ].Text = GetString( "ENDSWITH" );
				SearchOpList.Items[ 2 ].Text = GetString( "CONTAINS" );
				SearchOpList.Items[ 3 ].Text = GetString( "EQUALS" );
				SearchOpList.SelectedValue = GetString( "BEGINSWITH" );

				// Set focus to the inbox control.
				SetFocus( SearchNameTextBox );

				// Set the javascript function that will handle key presses.
				SearchNameTextBox.Attributes[ "OnKeyPress" ] = "return SubmitKeyDown(event, '" + SearchButton.ClientID + "');";
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

		/// <summary>
		/// Event handler that gets called when the search button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnSearchButton_Click( object source, EventArgs e )
		{
			if ( Click != null )
			{
				Click( source, e );
			}
		}

		#endregion

		#region Public Methods

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
			SearchButton.Click += new EventHandler( OnSearchButton_Click );
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
