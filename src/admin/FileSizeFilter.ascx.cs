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
	///		Summary description for FileSizeFilter.
	/// </summary>
	public class FileSizeFilter : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Default limit for the file size filter.
		/// </summary>
		private const long DefaultFileSizeLimit = 1024 * 1024;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// File size policy control.
		/// </summary>
		protected Label Title;

		/// <summary>
		/// File Size policy control.
		/// </summary>
		protected CheckBox Enabled;

		/// <summary>
		/// File size policy control.
		/// </summary>
		protected Label LimitTag;

		/// <summary>
		/// File Size policy control.
		/// </summary>
		protected TextBox LimitValue;

		/// <summary>
		/// File size effective label control.
		/// </summary>
		protected Label EffectiveTag;

		/// <summary>
		/// File size effective display control.
		/// </summary>
		protected Label EffectiveValue;

		/// <summary>
		/// File size effective display control.
		/// </summary>
		protected Label EffectiveUnits;


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
        /// <summary>
        /// Page load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				Title.Text = GetString( "FILESIZE" );
				LimitTag.Text = GetString( "LIMITTAG" );
				EffectiveTag.Text = GetString( "EFFECTIVETAG" );
			}
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
			if ( Enabled.Checked )
			{
				long limit = ( EffectiveLimit == 0 ) ? DefaultFileSizeLimit : EffectiveLimit;
				LimitValue.Text = Utils.ConvertToMBString( limit, false, rm );
			}
			else
			{
				LimitValue.Text = String.Empty;
			}

			LimitValue.Enabled = Enabled.Checked;
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
			Enabled.Checked = LimitValue.Enabled = ( policy.FileSizeLimit > 0 );

			LimitValue.Text = ( Enabled.Checked ) ? 
				Utils.ConvertToMBString( policy.FileSizeLimit, false, rm ) : String.Empty;

			//Commenting the below code to display the Effective limit, when user file size limit is specified.
		//	if ( Enabled.Checked )
			{
		//		EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = false;
			}
		//	else
			{
				EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = true;
				if ( policy.FileSizeLimitEffective > 0 )
				{
					EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = true;
					EffectiveValue.Text = Utils.ConvertToMBString( policy.FileSizeLimitEffective, false, rm );
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
		/// Gets the file size policy for the current ifolder.
		/// </summary>
		/// <param name="policy">iFolder policy</param>
		public void GetFileSizePolicy( iFolderPolicy policy )
		{
			EffectiveLimit = policy.FileSizeLimitEffective;
			Enabled.Checked = LimitValue.Enabled = ( policy.FileSizeLimit > 0 );

			LimitValue.Text = ( Enabled.Checked ) ? 
				Utils.ConvertToMBString( policy.FileSizeLimit, false, rm ) : String.Empty;

			if ( ( policy.FileSizeLimitEffective > 0 ) || ( policy.FileSizeLimit > 0 ) )
			{
				EffectiveValue.Text = Utils.ConvertToMBString( policy.FileSizeLimitEffective, false, rm );
				EffectiveUnits.Text = GetString( "MB" );
			}
			else
			{
				EffectiveValue.Text = GetString( "UNLIMITED" );
				EffectiveUnits.Text = String.Empty;
			}
		}

		/// <summary>
		/// Gets the file size policy for the system.
		/// </summary>
		/// <param name="policy">System policy</param>
		public void GetFileSizePolicy( SystemPolicy policy )
		{
			EffectiveLimit = 0;

			Enabled.Checked = LimitValue.Enabled = ( policy.FileSizeLimit > 0 );

			LimitValue.Text = ( Enabled.Checked ) ? 
				Utils.ConvertToMBString( policy.FileSizeLimit, false, rm ) : String.Empty;

			EffectiveTag.Visible = EffectiveValue.Visible = EffectiveUnits.Visible = false;
		}

		/// <summary>
		/// Sets the file size policy for this user.
		/// </summary>
		/// <param name="policy">User policy that the new file size filter will be set.</param>
		public void SetFileSizePolicy( UserPolicy policy )
		{
			// Verify that the file size value is valid.
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
							// Convert megabytes to bytes.
							policy.FileSizeLimit = Convert.ToInt64( Decimal.Round( limit, 2 ) * 1048576 );
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
						}
					}
					catch ( FormatException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
					}
                                        catch ( OverflowException )
                                        {
                                                throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
                                        }
				}
				else
				{
					throw new ArgumentException( GetString( "ERRORNOFILESIZE" ) );
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
							// Convert megabytes to bytes.
							long filelimit = Convert.ToInt64( Decimal.Round( limit, 2 ) * 1048576 );
							if ( policy.SpaceLimit > 0 )
							{
								if ( filelimit >= policy.SpaceLimit ) 
								{
									throw new ArgumentException( GetString( "ERRORINVALIDLIMIT" ) );
								}
								else
								{
									policy.FileSizeLimit = filelimit;
								}
							}
							else
							{
								policy.FileSizeLimit = filelimit;
							}
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
						}
					}
					catch ( FormatException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
					}
                                        catch ( OverflowException )
                                        {
                                                throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
                                        }
				}
				else
				{
					throw new ArgumentException( GetString( "ERRORNOFILESIZE" ) );
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
							// Convert megabytes to bytes.
							policy.FileSizeLimit = Convert.ToInt64( Decimal.Round( limit, 2 ) * 1048576 );
						}
						else
						{
							throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
						}
					}
					catch ( FormatException )
					{
						throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
					}
                                        catch ( OverflowException )
                                        {
                                                throw new ArgumentException( GetString( "ERRORINVALIDFILESIZE" ) );
                                        }
				}
				else
				{
					throw new ArgumentException( GetString( "ERRORNOFILESIZE" ) );
				}
			}
			else
			{
				policy.FileSizeLimit = 0;
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
			Enabled.CheckedChanged += new EventHandler( FileSizeCheckChanged );
			LimitValue.TextChanged += new EventHandler( FileSizeChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
