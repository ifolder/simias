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
		/// Sets the checkbox enabled or disabled state
		/// </summary>
		public bool SetCheckBoxEnabledState
		{
			set
			{
				Enabled.Enabled = value;
			}
		}

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
			Enabled.Enabled = !policy.isAdmin;
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
