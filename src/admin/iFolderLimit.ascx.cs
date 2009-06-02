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
*                 $Author : Ravi Kumar <rkumar@novell.com>
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
	/// Summary description for iFolderLimit.
	/// </summary>
	public class iFolderLimit : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Default constants.
		/// </summary>
		private const int DefaultiFolderLimit = 10;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
                /// iFolder Limit policy controls.
                /// </summary>
                protected HtmlGenericControl iFolderLimitNav;

	
		/// <summary>
		/// Control that contains the title for the page.
		/// </summary>
		protected Label Title;

		/// <summary>
		/// iFolderLimit policy controls.
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
		/// Event that notifies consumer that the disk quota limit has changed.
		/// </summary>
		public event EventHandler LimitChanged = null;

		#endregion

		#region Private Methods

		/// <summary>
                /// Gets or sets the limit value.
                /// </summary>
                private long limit 
                {
                        get { return ( long )ViewState[ "limit" ]; }
                        set { ViewState[ "limit" ] = value; }
                }


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
				Title.Text = GetString( "IFOLDERLIMIT" );
				LimitTag.Text = GetString( "LIMITTAG" );
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the iFolderLimit text is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void iFolderLimitChanged( Object sender, EventArgs e )
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
		/// Event handler that gets called when the limit enable checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void LimitCheckChanged( Object sender, EventArgs e )
		{
		
			if ( Enabled.Checked )
			{
				long value = ( limit >= 0 ) ? limit : DefaultiFolderLimit;
                                LimitValue.Text =  value.ToString(); 
			}
			else
			{
				LimitValue.Text = String.Empty;
			}

			LimitValue.Enabled = Enabled.Checked;
			iFolderLimitChanged( sender, e );
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the iFolderLimit policy for the current user.
		/// </summary>
		/// <param name="policy">User policy object</param>
		public void GetiFolderLimitPolicy( UserPolicy policy )
		{
         	       	if( policy.NoiFoldersLimit == -2 )
	                {
                        	Enabled.Checked = LimitValue.Enabled = false;
                                limit = policy.NoiFoldersLimit;
                        }
                        else
                        {
                                Enabled.Checked = LimitValue.Enabled = ( policy.NoiFoldersLimit >= 0 );
                                limit = ( policy.NoiFoldersLimit == -1 ) ? DefaultiFolderLimit : policy.NoiFoldersLimit;
	
			        if( policy.NoiFoldersLimit >= 0 )
                                       LimitValue.Text = limit.ToString();
                                else
                                       LimitValue.Text = string.Empty;
                        }
	
		}

		/// <summary>
		/// Gets the iFolderLimit policy for the system.
		/// </summary>
		/// <param name="policy">System policy object</param>
		public void GetiFolderLimitPolicy( SystemPolicy policy )
		{
			Enabled.Checked = LimitValue.Enabled = ( policy.NoiFoldersLimit >= 0 );
                        limit = ( policy.NoiFoldersLimit == -1 ) ? DefaultiFolderLimit : policy.NoiFoldersLimit;

                        if( policy.NoiFoldersLimit >= 0 )

                                LimitValue.Text = limit.ToString();
                        else
                                LimitValue.Text = string.Empty;


		}

		/// <summary>
		/// Sets the iFolderLimit policy for this user.
		/// </summary>
		/// <param name="policy">User policy that the new iFolderLimit will be set.</param>
		public void SetiFolderLimitPolicy( UserPolicy policy )
		{

			// Verify that iFolderLimit value is valid.
                        if ( Enabled.Checked )
                        {
                                string limitString = LimitValue.Text;
                                if ( ( limitString != null ) && ( limitString != String.Empty ) )
                                {
                                        try
                                        {
                                                long value = Convert.ToInt64( limitString );
                                                if ( value >= 0 )
                                                {
                                                        policy.NoiFoldersLimit = value;
                                                }
                                                else
                                                {
                                                        throw new ArgumentException( GetString("ERRORINVALIDIFOLDERLIMIT" ) );
                                                }
                                        }
                                        catch ( FormatException )
                                        {
						throw new ArgumentException( GetString("ERRORINVALIDIFOLDERLIMIT" ) );

                                        }
                                        catch ( OverflowException )
                                        {
						throw new ArgumentException( GetString("ERRORINVALIDIFOLDERLIMIT" ) );

                                        }
                                }
                                else
                                {
                                        throw new ArgumentException( GetString("ERRORNOIFOLDERLIMIT" ) );
                                }
                        }
                        else
                        {
                                policy.NoiFoldersLimit = -1;
                        }
		
		}

		/// <summary>
		/// Sets the iFolderLimite policy for the system.
		/// </summary>
		/// <param name="policy">System policy that the iFolderLimit will be set.</param>
		public void SetiFolderLimitPolicy( SystemPolicy policy )
		{
			// Verify that iFolderLimit value is valid.
                        if ( Enabled.Checked )
                        {
                                string limitString = LimitValue.Text;
                                if ( ( limitString != null ) && ( limitString != String.Empty ) )
                                {
                                        try
                                        {
                                                long value = Convert.ToInt64( limitString );
                                                if ( value >= 0 )
                                                {
                                                        policy.NoiFoldersLimit = value;
                                                }
                                                else
                                                {
                                                        throw new ArgumentException( GetString("ERRORINVALIDIFOLDERLIMIT" ) );
                                                }
                                        }
                                        catch ( FormatException )
                                        {
                                                throw new ArgumentException( GetString("ERRORINVALIDIFOLDERLIMIT" ) );

                                        }
                                        catch ( OverflowException )
                                        {
                                                throw new ArgumentException( GetString("ERRORINVALIDIFOLDERLIMIT" ) );

                                        }
                                }
                                else
                                {
                                        throw new ArgumentException( GetString("ERRORNOIFOLDERLIMIT" ) );
                                }
			}
                        else
                        {
                                policy.NoiFoldersLimit = -1;
                        }


		}

		/// <summary>
                /// Gets the ifolder Limit policy for the current ifolder.
                /// </summary>
                /// <param name="policy">iFolder policy object</param>
                public void GetiFolderLimitPolicy( iFolderPolicy policy )
                {
                        iFolderLimitNav.Visible = false;
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
			Enabled.CheckedChanged += new EventHandler( LimitCheckChanged );
			LimitValue.TextChanged += new EventHandler( iFolderLimitChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
