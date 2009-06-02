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
	///		Summary description for SyncInterval.
	/// </summary>
	public class SyncInterval : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Default value for the sync interval.
		/// </summary>
		private const int DefaultSyncInterval = 5 * 60;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Sync policy controls.
		/// </summary>
		protected Label Title;

		/// <summary>
		/// Sync policy controls.
		/// </summary>
		protected CheckBox Enabled;

		/// <summary>
		/// Sync policy controls.
		/// </summary>
		protected Label LimitTag;

		/// <summary>
		/// Sync policy controls.
		/// </summary>
		protected TextBox LimitValue;

		/// <summary>
		/// Sync policy controls.
		/// </summary>
		protected Label EffectiveTag;

		/// <summary>
		/// Sync policy controls.
		/// </summary>
		protected Label EffectiveValue;

		/// <summary>
		/// Sync policy controls.
		/// </summary>
		protected Label EffectiveUnits;


		/// <summary>
		/// Event that notifies consumer that the sync interval has changed.
		/// </summary>
		public event EventHandler IntervalChanged = null;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the effective sync interval.
		/// </summary>
		private int EffectiveInterval
		{
			get { return ( int )ViewState[ "EffectiveInterval" ]; }
			set { ViewState[ "EffectiveInterval" ] = value; }
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
				Title.Text = GetString( "SYNCHRONIZATION" );
				LimitTag.Text = GetString( "INTERVALTAG" );
				EffectiveTag.Text = GetString( "EFFECTIVETAG" );
				EffectiveUnits.Text = GetString( "MINUTES" );
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

		/// <summary>
		/// Event handler that gets called when the sync enable checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void SyncCheckChanged( Object sender, EventArgs e )
		{
			if ( Enabled.Checked )
			{
				int interval = ( ( EffectiveInterval == 0 ) ? DefaultSyncInterval : EffectiveInterval ) / 60;
				LimitValue.Text = interval.ToString();
			}
			else
			{
				LimitValue.Text = String.Empty;				
			}

			LimitValue.Enabled = Enabled.Checked;
			SyncIntervalChanged( sender, e );
		}

		/// <summary>
		/// Event handler that gets called when the sync interval policy is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void SyncIntervalChanged( Object sender, EventArgs e )
		{
			if ( IntervalChanged != null )
			{
				IntervalChanged( sender, e );
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the sync policy for the current user.
		/// </summary>
		/// <param name="policy">User policy.</param>
		public void GetSyncPolicy( UserPolicy policy )
		{
			EffectiveInterval = policy.SyncIntervalEffective;

			Enabled.Checked = LimitValue.Enabled = ( policy.SyncInterval > 0 );
			if ( Enabled.Checked )
			{
				int syncInterval = policy.SyncInterval / 60;
				LimitValue.Text = syncInterval.ToString();
			}
			else
			{
				LimitValue.Text = String.Empty;
			}

			if ( Enabled.Checked )
			{
				EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = false;
			}
			else
			{
				EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = true;
				if ( policy.SyncIntervalEffective > 0 )
				{
					int syncInterval = policy.SyncIntervalEffective / 60;
					EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = true;
					EffectiveValue.Text = syncInterval.ToString();
				}
				else
				{
					EffectiveValue.Text = GetString( "UNLIMITED" );
					EffectiveUnits.Text = String.Empty;
				}
			}
		}

		/// <summary>
		/// Gets the sync policy for the current ifolder.
		/// </summary>
		/// <param name="policy">ifolder policy.</param>
		public void GetSyncPolicy( iFolderPolicy policy )
		{
			EffectiveInterval = policy.SyncIntervalEffective;

			Enabled.Checked = LimitValue.Enabled = ( policy.SyncInterval > 0 );
			if ( Enabled.Checked )
			{
				int syncInterval = policy.SyncInterval / 60;
				LimitValue.Text = syncInterval.ToString();
			}
			else
			{
				LimitValue.Text = String.Empty;
			}

			if ( ( policy.SyncIntervalEffective > 0 ) || ( policy.SyncInterval > 0 ) )
			{
				int syncInterval = policy.SyncIntervalEffective / 60;
				EffectiveValue.Text = syncInterval.ToString();
				EffectiveUnits.Text = GetString( "MINUTES" );
			}
			else
			{
				EffectiveValue.Text = GetString( "UNLIMITED" );
				EffectiveUnits.Text = String.Empty;
			}
		}

		/// <summary>
		/// Gets the sync policy for the system.
		/// </summary>
		/// <param name="policy">System policy.</param>
		public void GetSyncPolicy( SystemPolicy policy )
		{
			EffectiveInterval = 0;

			Enabled.Checked = LimitValue.Enabled = ( policy.SyncInterval > 0 );
			if ( Enabled.Checked )
			{
				int syncInterval = policy.SyncInterval / 60;
				LimitValue.Text = syncInterval.ToString();
			}
			else
			{
				LimitValue.Text = String.Empty;
			}

			EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = false;
		}

		/// <summary>
		/// Sets the synchronization policy for this user.
		/// </summary>
		/// <param name="policy">User policy that the new sync interval will be set.</param>
		public void SetSyncPolicy( UserPolicy policy )
		{
			// Verify that the file size value is valid.
			if ( Enabled.Checked )
			{
				string limitString = LimitValue.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						int limit = Convert.ToInt32( limitString );
						if ( limit >= 5 && limit <= 999  )
						{
							// Convert the interval from minutes to seconds.
							policy.SyncInterval = limit * 60;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
						}
					}
					catch ( FormatException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
					}
					catch ( OverflowException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
					}
				}
				else
				{
					throw new ArgumentException( GetString( "ERRORNOSYNCINTERVAL" ) );
				}
			}
			else
			{
				policy.SyncInterval = 0;
			}
		}

		/// <summary>
		/// Sets the synchronization policy for this ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy that the new sync interval will be set.</param>
		public void SetSyncPolicy( iFolderPolicy policy )
		{
			// Verify that the file size value is valid.
			if ( Enabled.Checked )
			{
				string limitString = LimitValue.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						int limit = Convert.ToInt32( limitString );
						if ( limit >= 5  && limit <= 999 )
						{
							// Convert the interval from minutes to seconds.
							policy.SyncInterval = limit * 60;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
						}
					}
					catch ( FormatException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
					}
					catch ( OverflowException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
					}
				}
				else
				{
					throw new ArgumentException( GetString( "ERRORNOSYNCINTERVAL" ) );
				}
			}
			else
			{
				policy.SyncInterval = 0;
			}
		}

		/// <summary>
		/// Sets the synchronization policy for the system.
		/// </summary>
		/// <param name="policy">System policy that the new sync interval will be set.</param>
		public void SetSyncPolicy( SystemPolicy policy )
		{
			// Verify that the file size value is valid.
			if ( Enabled.Checked )
			{
				string limitString = LimitValue.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						int limit = Convert.ToInt32( limitString );
						if ( limit >= 5  && limit <= 999 )
						{
							// Convert the interval from minutes to seconds.
							policy.SyncInterval = limit * 60;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
						}
					}
					catch ( FormatException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
					}
					catch ( OverflowException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
					}
				}
				else
				{
					throw new ArgumentException( GetString( "ERRORNOSYNCINTERVAL" ) );
				}
			}
			else
			{
				policy.SyncInterval = 0;
			}
		}

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
			Enabled.CheckedChanged += new EventHandler( SyncCheckChanged );
			LimitValue.TextChanged += new EventHandler( SyncIntervalChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
