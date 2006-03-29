/***********************************************************************
 *  $RCSfile: SyncInterval.ascx.cs,v $
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
	///		Summary description for SyncInterval.
	/// </summary>
	public class SyncInterval : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Default value for the sync interval.
		/// </summary>
		private const int DefaultSyncInterval = 10 * 60;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Sync policy controls.
		/// </summary>
		protected CheckBox SyncIntervalCheckBox;

		/// <summary>
		/// Sync policy controls.
		/// </summary>
		protected TextBox SyncLimit;

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
			if ( SyncIntervalCheckBox.Checked )
			{
				int interval = ( ( EffectiveInterval == 0 ) ? DefaultSyncInterval : EffectiveInterval ) / 60;
				SyncLimit.Text = interval.ToString();
			}
			else
			{
				SyncLimit.Text = String.Empty;				
			}

			SyncLimit.Enabled = SyncIntervalCheckBox.Checked;
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

			SyncIntervalCheckBox.Checked = SyncLimit.Enabled = ( policy.SyncInterval > 0 );
			if ( SyncIntervalCheckBox.Checked )
			{
				int syncInterval = policy.SyncInterval / 60;
				SyncLimit.Text = syncInterval.ToString();
			}
			else
			{
				SyncLimit.Text = String.Empty;
			}
		}

		/// <summary>
		/// Gets the sync policy for the current ifolder.
		/// </summary>
		/// <param name="policy">ifolder policy.</param>
		public void GetSyncPolicy( iFolderPolicy policy )
		{
			EffectiveInterval = 0;

			SyncIntervalCheckBox.Checked = SyncLimit.Enabled = ( policy.SyncInterval > 0 );
			if ( SyncIntervalCheckBox.Checked )
			{
				int syncInterval = policy.SyncInterval / 60;
				SyncLimit.Text = syncInterval.ToString();
			}
			else
			{
				SyncLimit.Text = String.Empty;
			}
		}

		/// <summary>
		/// Gets the sync policy for the system.
		/// </summary>
		/// <param name="policy">System policy.</param>
		public void GetSyncPolicy( SystemPolicy policy )
		{
			EffectiveInterval = 0;

			SyncIntervalCheckBox.Checked = SyncLimit.Enabled = ( policy.SyncInterval > 0 );
			if ( SyncIntervalCheckBox.Checked )
			{
				int syncInterval = policy.SyncInterval / 60;
				SyncLimit.Text = syncInterval.ToString();
			}
			else
			{
				SyncLimit.Text = String.Empty;
			}
		}

		/// <summary>
		/// Sets the synchronization policy for this user.
		/// </summary>
		/// <param name="policy">User policy that the new sync interval will be set.</param>
		public void SetSyncPolicy( UserPolicy policy )
		{
			// Verify that the file size value is valid.
			if ( SyncIntervalCheckBox.Checked )
			{
				string limitString = SyncLimit.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						int limit = Convert.ToInt32( limitString );
						if ( limit > 0 )
						{
							// Convert the interval from minutes to seconds.
							policy.SyncInterval = limit * 60;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
						}
					}
					catch ( Exception ex )
					{
						Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( ex.Message ) );
					}
				}
				else
				{
					Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( GetString( "ERRORNOSYNCINTERVAL" ) ) );
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
			if ( SyncIntervalCheckBox.Checked )
			{
				string limitString = SyncLimit.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						int limit = Convert.ToInt32( limitString );
						if ( limit > 0 )
						{
							// Convert the interval from minutes to seconds.
							policy.SyncInterval = limit * 60;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
						}
					}
					catch ( Exception ex )
					{
						Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( ex.Message ) );
					}
				}
				else
				{
					Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( GetString( "ERRORNOSYNCINTERVAL" ) ) );
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
			if ( SyncIntervalCheckBox.Checked )
			{
				string limitString = SyncLimit.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						int limit = Convert.ToInt32( limitString );
						if ( limit > 0 )
						{
							// Convert the interval from minutes to seconds.
							policy.SyncInterval = limit * 60;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDSYNCINTERVAL" ) );
						}
					}
					catch ( Exception ex )
					{
						Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( ex.Message ) );
					}
				}
				else
				{
					Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( GetString( "ERRORNOSYNCINTERVAL" ) ) );
				}
			}
			else
			{
				policy.SyncInterval = 0;
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
			SyncIntervalCheckBox.CheckedChanged += new EventHandler( SyncCheckChanged );
			SyncLimit.TextChanged += new EventHandler( SyncIntervalChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
