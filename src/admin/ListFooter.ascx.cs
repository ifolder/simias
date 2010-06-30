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
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	/// <summary>
	///		Summary description for ListFooter.
	/// </summary>
	public class ListFooter : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Web Controls
		/// </summary>
		protected ImageButton PageFirstButton;

		/// <summary>
		/// Web Controls
		/// </summary>
		protected ImageButton PagePreviousButton;

		/// <summary>
		/// Web Controls
		/// </summary>
		protected ImageButton PageNextButton;

		/// <summary>
		/// Web Controls
		/// </summary>
		protected ImageButton PageLastButton;

		/// <summary>
		/// Web Controls
		/// </summary>
		protected System.Web.UI.WebControls.Image PageFirstDisabledButton;

		/// <summary>
		/// Web Controls
		/// </summary>
		protected System.Web.UI.WebControls.Image PagePreviousDisabledButton;

		/// <summary>
		/// Web Controls
		/// </summary>
		protected System.Web.UI.WebControls.Image PageNextDisabledButton;

		/// <summary>
		/// Web Controls
		/// </summary>
		protected System.Web.UI.WebControls.Image PageLastDisabledButton;

		/// <summary>
		/// Web Controls
		/// </summary>
		protected Label PageText;

		/// <summary>
		/// Event that notifies the consumer that the PageFirstButton was clicked.
		/// </summary>
		public event ImageClickEventHandler PageFirstClick = null;

		/// <summary>
		/// Event that notifies the consumer that the PagePreviousButton was clicked.
		/// </summary>
		public event ImageClickEventHandler PagePreviousClick = null;

		/// <summary>
		/// Event that notifies the consumer that the PageNextButton was clicked.
		/// </summary>
		public event ImageClickEventHandler PageNextClick = null;

		/// <summary>
		/// Event that notifies the consumer that the PageLastButton was clicked.
		/// </summary>
		public event ImageClickEventHandler PageLastClick = null;

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the number of visible items in the specified DataGrid.
		/// </summary>
		/// <param name="dataGrid">DataGrid containing members.</param>
		/// <returns>The number of visible items in the DataGrid.</returns>
		private int GetVisibleItems( DataGrid dataGrid )
		{
			int count = 0;
			foreach( DataRow dr in ( dataGrid.DataSource as DataView ).Table.Rows )
			{
				if ( ( bool )dr[ 0 ] )
				{
					++count;
				}
			}

			return count;
		}

		/// <summary>
		/// Page_Load()
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				PageFirstButton.Attributes.Add( "title", GetString( "FIRSTPAGE" ) );
				PagePreviousButton.Attributes.Add( "title", GetString( "PREVIOUSPAGE" ) );
				PageNextButton.Attributes.Add( "title", GetString( "NEXTPAGE" ) );
				PageLastButton.Attributes.Add( "title", GetString( "LASTPAGE" ) );
				PageFirstDisabledButton.Attributes.Add( "title", GetString( "FIRSTPAGE" ) );
				PagePreviousDisabledButton.Attributes.Add( "title", GetString( "PREVIOUSPAGE" ) );
				PageNextDisabledButton.Attributes.Add( "title", GetString( "NEXTPAGE" ) );
				PageLastDisabledButton.Attributes.Add( "title", GetString( "LASTPAGE" ) );
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the PageFirstButton is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFirstPageClicked( Object sender, ImageClickEventArgs e )
		{
			if ( PageFirstClick != null )
			{
				PageFirstClick( this, e );
			}
		}

		/// <summary>
		/// Event handler that gets called when the PageLastButton is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnLastPageClicked( Object sender, ImageClickEventArgs e )
		{
			if ( PageLastClick != null )
			{
				PageLastClick( this, e );
			}
		}

		/// <summary>
		/// Event handler that gets called when the PageNextButton is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnNextPageClicked( Object sender, ImageClickEventArgs e )
		{
			if ( PageNextClick != null )
			{
				PageNextClick( this, e );
			}
		}

		/// <summary>
		/// Event handler that gets called when the PagePreviousButton is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnPreviousPageClicked( Object sender, ImageClickEventArgs e )
		{
			if ( PagePreviousClick != null )
			{
				PagePreviousClick( this, e );
			}
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key">Key to the localized string.</param>
		/// <returns>Localized string.</returns>
		protected string GetString( string key )
		{
			string s = rm.GetString( key );
			return s;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Sets the text to be displayed at the botton of the list
		/// </summary>
		/// <param name="footerText"> Text to be displayed at the botton of the list</param>
		public void SetPageText( string footerText )
		{
		         PageText.Text = footerText;
		}

		/// <summary>
		/// Enables or disables the search navigation buttons depending on the display data index.
		/// </summary>
		/// <param name="dataGrid"></param>
		/// <param name="currentItemOffset"></param>
		/// <param name="totalItems"></param>
		/// <param name="multipleItemType"></param>
		/// <param name="singleItemType"></param>
		public void SetPageButtonState( DataGrid dataGrid, int currentItemOffset, int totalItems, string multipleItemType, string singleItemType )
		{
			int userCount = GetVisibleItems( dataGrid );

			PageText.Text = String.Format( 
				GetString( "PAGENUMBERFORMAT" ),
				( userCount > 0 ) ? currentItemOffset + 1 : 0, 
				currentItemOffset + userCount,
				totalItems,
				( totalItems == 1 ) ? singleItemType : multipleItemType );			

			if ( currentItemOffset == 0 )
			{
				PageFirstButton.Visible = false;
				PagePreviousButton.Visible = false;
				PageFirstDisabledButton.Visible = true;
				PagePreviousDisabledButton.Visible = true;
			}
			else
			{
				PageFirstButton.Visible = true;
				PagePreviousButton.Visible = true;
				PageFirstDisabledButton.Visible = false;
				PagePreviousDisabledButton.Visible = false;
			}

			if ( ( currentItemOffset + dataGrid.PageSize ) >= totalItems )
			{
				PageNextButton.Visible = false;
				PageLastButton.Visible = false;
				PageNextDisabledButton.Visible = true;
				PageLastDisabledButton.Visible = true;
			}
			else
			{
				PageNextButton.Visible = true;
				PageLastButton.Visible = true;
				PageNextDisabledButton.Visible = false;
				PageLastDisabledButton.Visible = false;
			}

			if(0 == totalItems )
		    {
		        PageNextButton.Visible = false;
		        PageLastButton.Visible = false;
		        PageNextDisabledButton.Visible = false;
		        PageLastDisabledButton.Visible = false;
		        PageFirstButton.Visible = false;
			    PagePreviousButton.Visible = false;
			    PageFirstDisabledButton.Visible = false;
			    PagePreviousDisabledButton.Visible = false;
			    PageText.Text = String.Format(GetString("NOMEMBERAVAILABLE"));
	        }

		}

		#endregion

		#region Web Form Designer generated code

		/// <summary>
		/// OnInit()
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
			PageFirstButton.Click += new ImageClickEventHandler( OnFirstPageClicked );
			PagePreviousButton.Click += new ImageClickEventHandler( OnPreviousPageClicked );
			PageNextButton.Click += new ImageClickEventHandler( OnNextPageClicked );
			PageLastButton.Click += new ImageClickEventHandler( OnLastPageClicked );

			this.Load += new System.EventHandler(this.Page_Load);
		}

		#endregion
	}
}
