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
		private const long DefaultDiskQuotaLimit = 100 * 1024 * 1024;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// Control that contains the title for the page.
		/// </summary>
		protected Label Title;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected CheckBox Enabled;

		/// <summary>
		/// Control that contains the limit tag.
		/// </summary>
		protected Label LimitTag;

		/// <summary>
		/// Control that contains the limit value.
		/// </summary>
		protected TextBox LimitValue;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Label UsedTag;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Label UsedValue;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Label UsedUnits;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Label AvailableTag;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Label AvailableValue;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Label AvailableUnits;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Label EffectiveTag;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Label EffectiveValue;

		/// <summary>
		/// Disk Quota policy controls.
		/// </summary>
		protected Label EffectiveUnits;


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
				Title.Text = GetString( "DISKQUOTA" );
				LimitTag.Text = GetString( "LIMITTAG" );
				UsedTag.Text = GetString ( "USEDTAG" );
				AvailableTag.Text = GetString( "AVAILABLETAG" );
				EffectiveTag.Text = GetString( "EFFECTIVETAG" );
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
			if ( Enabled.Checked )
			{
				long limit = ( EffectiveSpace == 0 ) ? DefaultDiskQuotaLimit : EffectiveSpace;
				LimitValue.Text = Utils.ConvertToMBString( limit, false, rm );
			}
			else
			{
				LimitValue.Text = String.Empty;
			}

			LimitValue.Enabled = Enabled.Checked;
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

			Enabled.Checked = LimitValue.Enabled = ( policy.SpaceLimit > 0 );

			UsedValue.Text = Utils.ConvertToMBString( policy.SpaceUsed, false, rm );
			UsedUnits.Text = GetString( "MB" );

			LimitValue.Text = Enabled.Checked ? 
				Utils.ConvertToMBString( policy.SpaceLimit, false, rm ) : String.Empty;

			if ( ( policy.SpaceLimitEffective > 0 ) || ( policy.SpaceLimit > 0 ) )
			{
				AvailableValue.Text = Utils.ConvertToMBString( policy.SpaceAvailable, false, rm );
				AvailableUnits.Text = GetString( "MB" );
			}
			else
			{
				AvailableValue.Text = GetString( "UNLIMITED" );
				AvailableUnits.Text = String.Empty;
			}

			if ( Enabled.Checked )
			{
				EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = false;
			}
			else
			{
				EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = true;
				if ( policy.SpaceLimitEffective > 0 )
				{
					EffectiveValue.Text = Utils.ConvertToMBString( policy.SpaceLimitEffective, false, rm );
					EffectiveUnits.Text = GetString( "MB" );
				}
				else
				{
					EffectiveValue.Text = GetString( "UNLIMITED" );
					EffectiveUnits.Text = String.Empty;
				}
			}
		}

		/// <summary>
		/// Gets the disk quota policy for the current ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy object</param>
		public void GetDiskSpacePolicy( iFolderPolicy policy )
		{
			EffectiveSpace = policy.SpaceLimitEffective;

			Enabled.Checked = LimitValue.Enabled = ( policy.SpaceLimit > 0 );

			UsedValue.Text = Utils.ConvertToMBString( policy.SpaceUsed, false, rm );
			UsedUnits.Text = GetString( "MB" );

			LimitValue.Text = Enabled.Checked ? 
				Utils.ConvertToMBString( policy.SpaceLimit, false, rm ) : String.Empty;

			if ( ( policy.SpaceLimitEffective > 0 ) || ( policy.SpaceLimit > 0 ) )
			{
				AvailableValue.Text = Utils.ConvertToMBString( policy.SpaceAvailable, false, rm );
				AvailableUnits.Text = GetString( "MB" );

				EffectiveValue.Text = Utils.ConvertToMBString( policy.SpaceLimitEffective, false, rm );
				EffectiveUnits.Text = GetString( "MB" );
			}
			else
			{
				AvailableValue.Text = GetString( "UNLIMITED" );
				AvailableUnits.Text = String.Empty;

				EffectiveValue.Text = GetString( "UNLIMITED" );
				EffectiveUnits.Text = String.Empty;
			}
		}

		/// <summary>
		/// Gets the disk quota policy for the system.
		/// </summary>
		/// <param name="policy">System policy object</param>
		public void GetDiskSpacePolicy( SystemPolicy policy )
		{
			EffectiveSpace = 0;

			Enabled.Checked = LimitValue.Enabled = ( policy.SpaceLimitUser > 0 );
			LimitValue.Text = Enabled.Checked ? 
				Utils.ConvertToMBString( policy.SpaceLimitUser, false, rm ) : String.Empty;

			UsedTag.Visible = UsedValue.Visible = false;
			AvailableTag.Visible = AvailableValue.Visible = false;
			EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = false;
		}

		/// <summary>
		/// Sets the disk space policy for this user.
		/// </summary>
		/// <param name="policy">User policy that the new disk space quota will be set.</param>
		public void SetDiskSpacePolicy( UserPolicy policy )
		{
			// Verify that the disk space quota value is valid.
			if ( Enabled.Checked )
			{
				string limitString = LimitValue.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						decimal limit = Convert.ToDecimal( limitString );
						if ( limit >= 1 )
						{
							// Convert from megabytes back to bytes.
							policy.SpaceLimit = Convert.ToInt64( Decimal.Round( limit, 2 ) * 1048576 );
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDQUOTA" ) );
						}
					}
					catch ( FormatException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDQUOTA" ) );
					}
				}
				else
				{
					throw new ArgumentException( GetString( "ERRORNOQUOTA" ) );
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
			if ( Enabled.Checked )
			{
				string limitString = LimitValue.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						decimal limit = Convert.ToDecimal( limitString );
						if ( limit >= 1 )
						{
							// Convert from megabytes back to bytes.
							policy.SpaceLimit = Convert.ToInt64( Decimal.Round( limit, 2 ) * 1048576 );
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDQUOTA" ) );
						}
					}
					catch ( FormatException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDQUOTA" ) );
					}
				}
				else
				{
					throw new ArgumentException( GetString( "ERRORNOQUOTA" ) );
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
			if ( Enabled.Checked )
			{
				string limitString = LimitValue.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						decimal limit = Convert.ToDecimal( limitString );
						if ( limit >= 1 )
						{
							// Convert from megabytes back to bytes.
							policy.SpaceLimitUser = Convert.ToInt64( Decimal.Round( limit, 2 ) * 1048576 );
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDQUOTA" ) );
						}
					}
					catch ( FormatException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDQUOTA" ) );
					}
				}
				else
				{
					throw new ArgumentException( GetString( "ERRORNOQUOTA" ) );
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
			Enabled.CheckedChanged += new EventHandler( QuotaCheckChanged );
			LimitValue.TextChanged += new EventHandler( DiskSpaceQuotaChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
