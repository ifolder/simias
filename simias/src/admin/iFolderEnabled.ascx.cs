/***********************************************************************
 *  $RCSfile: iFolderEnabled.ascx.cs,v $
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
	///		Summary description for iFolder enabled policy.
	/// </summary>
	public class iFolderEnabled : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// iFolder enabled policy controls.
		/// </summary>
		protected HtmlGenericControl iFolderEnabledNav;

		/// <summary>
		/// iFolder enabled policy controls.
		/// </summary>
		protected Label Title;

		/// <summary>
		/// iFolder enabled policy controls.
		/// </summary>
		protected CheckBox Enabled;

		/// <summary>
		/// iFolder enabled policy controls.
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
				Title.Text = GetString( "IFOLDER" );
				DisabledTag.Text = GetString( "IFOLDERDISABLED" );
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the ifolder disable checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void iFolderCheckChanged( Object sender, EventArgs e )
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
		/// Gets the ifolder enabled policy for the current ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy object</param>
		public void GetiFolderEnabledPolicy( iFolderPolicy policy )
		{
			Enabled.Checked = policy.Locked;
		}

		/// <summary>
		/// Gets the ifolder enabled policy for the system.
		/// </summary>
		/// <param name="policy">System policy object</param>
		public void GetiFolderEnabledPolicy( SystemPolicy policy )
		{
			iFolderEnabledNav.Visible = false;
		}

		/// <summary>
		/// Gets the ifolder enabled policy for the current user.
		/// </summary>
		/// <param name="policy">User policy object</param>
		public void GetiFolderEnabledPolicy( UserPolicy policy )
		{
			iFolderEnabledNav.Visible = false;
		}

		/// <summary>
		/// Sets the ifolder enabled policy for the ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy where the synchronization information will be set.</param>
		public void SetiFolderEnabledPolicy( iFolderPolicy policy )
		{
			policy.Locked = Enabled.Checked;
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
			Enabled.CheckedChanged += new EventHandler( iFolderCheckChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
