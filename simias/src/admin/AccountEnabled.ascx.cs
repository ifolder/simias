/***********************************************************************
 *  $RCSfile: AccountEnabled.ascx.cs,v $
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
	///		Summary description for AccountEnabled.
	/// </summary>
	public class AccountEnabled : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Account DIV.
		/// </summary>
		protected HtmlGenericControl AccountNav;

		/// <summary>
		/// Account policy control.
		/// </summary>
		protected Label Title;

		/// <summary>
		/// Account policy control.
		/// </summary>
		protected CheckBox Enabled;

		/// <summary>
		/// Account policy control.
		/// </summary>
		protected Label DisabledTag;


		/// <summary>
		/// Event that notifies consumer that the checkbox has changed.
		/// </summary>
		public event EventHandler CheckChanged = null;

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
				Title.Text = GetString( "ACCOUNT" );
				DisabledTag.Text = GetString( "USERLOGINDISABLED" );
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the account enable checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void AccountCheckChanged( Object sender, EventArgs e )
		{
			if ( CheckChanged != null )
			{
				CheckChanged( sender, e );
			}
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
		/// Gets the account policy for the current ifolder user.
		/// </summary>
		/// <param name="policy">iFolder policy object</param>
		public void GetAccountPolicy( iFolderPolicy policy )
		{
			AccountNav.Visible = false;
		}

		/// <summary>
		/// Gets the account policy for the system.
		/// </summary>
		/// <param name="policy">System policy object</param>
		public void GetAccountPolicy( SystemPolicy policy )
		{
			AccountNav.Visible = false;
		}

		/// <summary>
		/// Gets the account policy for the current user.
		/// </summary>
		/// <param name="policy">User policy object</param>
		public void GetAccountPolicy( UserPolicy policy )
		{
			Enabled.Checked = !policy.LoginEnabled;
		}

		/// <summary>
		/// Sets the account policy for the user.
		/// </summary>
		/// <param name="policy">User policy where the account information will be set.</param>
		public void SetAccountPolicy( UserPolicy policy )
		{
			policy.LoginEnabled = !Enabled.Checked;
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
			Enabled.CheckedChanged += new EventHandler( AccountCheckChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
