/***********************************************************************
 *  $RCSfile: DiskSpaceQuota.ascx.cs,v $
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
	///		Summary description for DiskSpaceQuota.
	/// </summary>
	public class DiskSpaceQuota : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Default constants.
		/// </summary>
		private const long DefaultDiskQuotaLimit = 100 * 1024;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected CheckBox QuotaEnabled;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Literal QuotaUsed;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Literal QuotaAvailable;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Literal QuotaEffectiveHeader;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Literal QuotaEffective;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected TextBox QuotaLimit;

		/// <summary>
		/// Table that displays quota information.
		/// </summary>
		protected HtmlTable QuotaTable;


		/// <summary>
		/// Event that notifies consumer that the disk quota limit has changed.
		/// </summary>
		public event EventHandler LimitChanged = null;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the effective disk space.
		/// </summary>
		private long EffectiveSpace
		{
			get { return ( long )ViewState[ "EffectiveSpace" ]; }
			set { ViewState[ "EffectiveSpace" ] = value; }
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
				QuotaEffectiveHeader.Text = GetString( "EFFECTIVETAG" );
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the disk space quota text is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void DiskSpaceQuotaChanged( Object sender, EventArgs e )
		{
			if ( LimitChanged != null )
			{
				LimitChanged( this, e );
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

		/// <summary>
		/// Event handler that gets called when the quota enable checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void QuotaCheckChanged( Object sender, EventArgs e )
		{
			if ( QuotaEnabled.Checked )
			{
				long limit = ( EffectiveSpace == 0 ) ? DefaultDiskQuotaLimit : EffectiveSpace;
				QuotaLimit.Text = Utils.ConvertToUnitString( limit, false, rm );
			}
			else
			{
				QuotaLimit.Text = String.Empty;
			}

			QuotaLimit.Enabled = QuotaEnabled.Checked;
			DiskSpaceQuotaChanged( sender, e );
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the disk quota policy for the current user.
		/// </summary>
		/// <param name="policy">User policy object</param>
		public void GetDiskSpacePolicy( UserPolicy policy )
		{
			EffectiveSpace = policy.SpaceLimitEffective;

			QuotaEnabled.Checked = QuotaLimit.Enabled = ( policy.SpaceLimit > 0 );
			QuotaUsed.Text = Utils.ConvertToUnitString( policy.SpaceUsed, true, rm );
			QuotaLimit.Text = QuotaEnabled.Checked ? 
				Utils.ConvertToUnitString( policy.SpaceLimit, false, rm ) : String.Empty;

			if ( ( policy.SpaceLimitEffective > 0 ) || ( policy.SpaceLimit > 0 ) )
			{
				QuotaAvailable.Text = Utils.ConvertToUnitString( policy.SpaceAvailable, true, rm );
			}
			else
			{
				QuotaAvailable.Text = GetString( "UNLIMITED" );
			}

			QuotaEffectiveHeader.Visible = false;
			QuotaEffective.Visible = false;
		}

		/// <summary>
		/// Gets the disk quota policy for the current ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy object</param>
		public void GetDiskSpacePolicy( iFolderPolicy policy )
		{
			EffectiveSpace = policy.SpaceLimitEffective;

			QuotaEnabled.Checked = QuotaLimit.Enabled = ( policy.SpaceLimit > 0 );
			QuotaUsed.Text = Utils.ConvertToUnitString( policy.SpaceUsed, true, rm );
			QuotaEffective.Text = Utils.ConvertToUnitString( policy.SpaceLimitEffective, true, rm );
			QuotaLimit.Text = QuotaEnabled.Checked ? 
				Utils.ConvertToUnitString( policy.SpaceLimit, false, rm ) : String.Empty;

			if ( ( policy.SpaceLimitEffective > 0 ) || ( policy.SpaceLimit > 0 ) )
			{
				QuotaAvailable.Text = Utils.ConvertToUnitString( policy.SpaceAvailable, true, rm );
			}
			else
			{
				QuotaAvailable.Text = GetString( "UNLIMITED" );
			}
		}

		/// <summary>
		/// Gets the disk quota policy for the system.
		/// </summary>
		/// <param name="policy">System policy object</param>
		public void GetDiskSpacePolicy( SystemPolicy policy )
		{
			EffectiveSpace = 0;

			QuotaEnabled.Checked = QuotaLimit.Enabled = ( policy.SpaceLimitUser > 0 );
			QuotaLimit.Text = QuotaEnabled.Checked ? 
				Utils.ConvertToUnitString( policy.SpaceLimitUser, false, rm ) : String.Empty;

			QuotaTable.Visible = false;
		}

		/// <summary>
		/// Sets the disk space policy for this user.
		/// </summary>
		/// <param name="policy">User policy that the new disk space quota will be set.</param>
		public void SetDiskSpacePolicy( UserPolicy policy )
		{
			// Verify that the disk space quota value is valid.
			if ( QuotaEnabled.Checked )
			{
				string limitString = QuotaLimit.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						long limit = Convert.ToInt64( limitString );
						if ( limit > 0 )
						{
							policy.SpaceLimit = limit * 1048576;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDQUOTA" ) );
						}
					}
					catch ( Exception ex )
					{
						Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( ex.Message ) );
					}
				}
				else
				{
					Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( GetString( "ERRORNOQUOTA" ) ) );
				}
			}
			else
			{
				policy.SpaceLimit = 0;
			}
		}

		/// <summary>
		/// Sets the disk space policy for this ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy that the new disk space quota will be set.</param>
		public void SetDiskSpacePolicy( iFolderPolicy policy )
		{
			// Verify that the disk space quota value is valid.
			if ( QuotaEnabled.Checked )
			{
				string limitString = QuotaLimit.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						long limit = Convert.ToInt64( limitString );
						if ( limit > 0 )
						{
							policy.SpaceLimit = limit * 1048576;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDQUOTA" ) );
						}
					}
					catch ( Exception ex )
					{
						Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( ex.Message ) );
					}
				}
				else
				{
					Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( GetString( "ERRORNOQUOTA" ) ) );
				}
			}
			else
			{
				policy.SpaceLimit = 0;
			}
		}

		/// <summary>
		/// Sets the disk space policy for the system.
		/// </summary>
		/// <param name="policy">System policy that the new disk space quota will be set.</param>
		public void SetDiskSpacePolicy( SystemPolicy policy )
		{
			// Verify that the disk space quota value is valid.
			if ( QuotaEnabled.Checked )
			{
				string limitString = QuotaLimit.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						long limit = Convert.ToInt64( limitString );
						if ( limit > 0 )
						{
							policy.SpaceLimitUser = limit * 1048576;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDQUOTA" ) );
						}
					}
					catch ( Exception ex )
					{
						Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( ex.Message ) );
					}
				}
				else
				{
					Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( GetString( "ERRORNOQUOTA" ) ) );
				}
			}
			else
			{
				policy.SpaceLimitUser = 0;
			}
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
			QuotaEnabled.CheckedChanged += new EventHandler( QuotaCheckChanged );
			QuotaLimit.TextChanged += new EventHandler( DiskSpaceQuotaChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
