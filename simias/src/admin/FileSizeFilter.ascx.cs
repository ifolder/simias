/***********************************************************************
 *  $RCSfile: FileSizeFilter.ascx.cs,v $
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
	///		Summary description for FileSizeFilter.
	/// </summary>
	public class FileSizeFilter : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Default limit for the file size filter.
		/// </summary>
		private const long DefaultFileSizeLimit = 10 * 1024;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// File Size policy controls.
		/// </summary>
		protected CheckBox FileSizeEnabled;

		/// <summary>
		/// File Size policy controls.
		/// </summary>
		protected TextBox FileSizeLimit;

		/// <summary>
		/// Event that notifies consumer that the file size limit has changed.
		/// </summary>
		public event EventHandler LimitChanged = null;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the effective file size limit.
		/// </summary>
		private long EffectiveLimit
		{
			get { return ( long )ViewState[ "EffectiveFileSizeLimit" ]; }
			set { ViewState[ "EffectiveFileSizeLimit" ] = value; }
		}

		#endregion

		#region Private Methods

		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the file size enable checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void FileSizeCheckChanged( Object sender, EventArgs e )
		{
			if ( FileSizeEnabled.Checked )
			{
				long limit = ( EffectiveLimit == 0 ) ? DefaultFileSizeLimit : EffectiveLimit;
				FileSizeLimit.Text = Utils.ConvertToUnitString( limit, false, rm );
			}
			else
			{
				FileSizeLimit.Text = String.Empty;
			}

			FileSizeLimit.Enabled = FileSizeEnabled.Checked;
			FileSizeChanged( sender, e );
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
		/// Event handler that gets called when the file size text is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void FileSizeChanged( Object sender, EventArgs e )
		{
			if ( LimitChanged != null )
			{
				LimitChanged( this, e );
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the file size policy for the current user.
		/// </summary>
		/// <param name="policy">User policy</param>
		public void GetFileSizePolicy( UserPolicy policy )
		{
			EffectiveLimit = policy.FileSizeLimitEffective;

			FileSizeEnabled.Checked = FileSizeLimit.Enabled = ( policy.FileSizeLimit > 0 );

			FileSizeLimit.Text = ( FileSizeEnabled.Checked ) ? 
				Utils.ConvertToUnitString( policy.FileSizeLimit, false, rm ) : String.Empty;
		}

		/// <summary>
		/// Gets the file size policy for the current ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy</param>
		public void GetFileSizePolicy( iFolderPolicy policy )
		{
			EffectiveLimit = 0;

			FileSizeEnabled.Checked = FileSizeLimit.Enabled = ( policy.FileSizeLimit > 0 );

			FileSizeLimit.Text = ( FileSizeEnabled.Checked ) ? 
				Utils.ConvertToUnitString( policy.FileSizeLimit, false, rm ) : String.Empty;
		}

		/// <summary>
		/// Gets the file size policy for the system.
		/// </summary>
		/// <param name="policy">System policy</param>
		public void GetFileSizePolicy( SystemPolicy policy )
		{
			EffectiveLimit = 0;

			FileSizeEnabled.Checked = FileSizeLimit.Enabled = ( policy.FileSizeLimit > 0 );

			FileSizeLimit.Text = ( FileSizeEnabled.Checked ) ? 
				Utils.ConvertToUnitString( policy.FileSizeLimit, false, rm ) : String.Empty;
		}

		/// <summary>
		/// Sets the file size policy for this user.
		/// </summary>
		/// <param name="policy">User policy that the new file size filter will be set.</param>
		public void SetFileSizePolicy( UserPolicy policy )
		{
			// Verify that the file size value is valid.
			if ( FileSizeEnabled.Checked )
			{
				string limitString = FileSizeLimit.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						long limit = Convert.ToInt64( limitString );
						if ( limit > 0 )
						{
							policy.FileSizeLimit = limit * 1048576;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
						}
					}
					catch ( Exception ex )
					{
						Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( ex.Message ) );
					}
				}
				else
				{
					Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( GetString( "ERRORNOFILESIZE" ) ) );
				}
			}
			else
			{
				policy.FileSizeLimit = 0;
			}
		}

		/// <summary>
		/// Sets the file size policy for this ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy that the new file size filter will be set.</param>
		public void SetFileSizePolicy( iFolderPolicy policy )
		{
			// Verify that the file size value is valid.
			if ( FileSizeEnabled.Checked )
			{
				string limitString = FileSizeLimit.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						long limit = Convert.ToInt64( limitString );
						if ( limit > 0 )
						{
							policy.FileSizeLimit = limit * 1048576;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
						}
					}
					catch ( Exception ex )
					{
						Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( ex.Message ) );
					}
				}
				else
				{
					Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( GetString( "ERRORNOFILESIZE" ) ) );
				}
			}
			else
			{
				policy.FileSizeLimit = 0;
			}
		}

		/// <summary>
		/// Sets the file size policy for the system.
		/// </summary>
		/// <param name="policy">System policy that the new file size filter will be set.</param>
		public void SetFileSizePolicy( SystemPolicy policy )
		{
			// Verify that the file size value is valid.
			if ( FileSizeEnabled.Checked )
			{
				string limitString = FileSizeLimit.Text;
				if ( ( limitString != null ) && ( limitString != String.Empty ) )
				{
					try
					{
						long limit = Convert.ToInt64( limitString );
						if ( limit > 0 )
						{
							policy.FileSizeLimit = limit * 1048576;
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
						}
					}
					catch ( Exception ex )
					{
						Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( ex.Message ) );
					}
				}
				else
				{
					Response.Redirect( "Error.aspx?Exception=" + Server.UrlEncode( GetString( "ERRORNOFILESIZE" ) ) );
				}
			}
			else
			{
				policy.FileSizeLimit = 0;
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
			FileSizeEnabled.CheckedChanged += new EventHandler( FileSizeCheckChanged );
			FileSizeLimit.TextChanged += new EventHandler( FileSizeChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
