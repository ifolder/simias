/***********************************************************************
 *  $RCSfile: Reports.aspx.cs,v $
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

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for Reports.
	/// </summary>
	public class Reports : System.Web.UI.Page
	{
		#region Class Members

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;

		/// <summary>
		/// Enable reporting control.
		/// </summary>
		protected CheckBox EnableReporting;

		/// <summary>
		/// Enable reporting label control.
		/// </summary>
		protected Label EnableReportingLabel;

		/// <summary>
		/// Reporting frequency list control.
		/// </summary>
		protected RadioButtonList FrequencyList;

		/// <summary>
		/// Reporting time of day control.
		/// </summary>
		protected DropDownList TimeOfDayList;

		/// <summary>
		/// Reporting day of month control.
		/// </summary>
		protected DropDownList DayOfMonthList;

		/// <summary>
		/// Reporting day of week control.
		/// </summary>
		protected DropDownList DayOfWeekList;

		/// <summary>
		/// Day label control.
		/// </summary>
		protected Label DayLabel;

		/// <summary>
		/// Report format control.
		/// </summary>
		protected DropDownList FormatList;

		/// <summary>
		/// Report location control.
		/// </summary>
		protected RadioButtonList ReportLocation;

		/// <summary>
		/// Summary label control.
		/// </summary>
		protected Label Summary;

		#endregion

		#region Properties

		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			TopNav.AddBreadCrumb( GetString( "REPORTS" ), null );
		}

		/// <summary>
		/// Displays the day control based on the frequency control value.
		/// </summary>
		private void DisplayDayControl()
		{
			switch ( FrequencyList.SelectedIndex )
			{
				case 0:
					DayLabel.Text = "&nbsp;";
					DayOfWeekList.Visible = DayOfMonthList.Visible = false;
					break;

				case 1:
					DayLabel.Text = GetString( "EVERYTAG" );
					DayOfWeekList.Visible = true;
					DayOfMonthList.Visible = false;
					break;

				case 2:
					DayLabel.Text = GetString( "DAYTAG" );
					DayOfMonthList.Visible = true;
					DayOfWeekList.Visible = false;
					break;
			}
		}

		/// <summary>
		/// Displays a summary of the report configuration selections.
		/// </summary>
		private void DisplaySummary()
		{
			switch ( FrequencyList.SelectedIndex )
			{
				case 0:
					Summary.Text = 
						String.Format( 
							GetString( "GENERATEDAILYREPORT" ), 
							TimeOfDayList.SelectedValue );
					break;

				case 1:
					Summary.Text = 
						String.Format( 
							GetString( "GENERATEWEEKLYREPORT" ), 
							DayOfWeekList.SelectedValue, 
							TimeOfDayList.SelectedValue );
					break;

				case 2:
					Summary.Text = 
						String.Format( 
							GetString( "GENERATEMONTHLYREPORT" ), 
							DayOfMonthList.SelectedValue, 
							TimeOfDayList.SelectedValue );
					break;
			}
		}

		/// <summary>
		/// Initializes the DayOfMonth dropdown list containing the first 28 days of the month.
		/// </summary>
		private void InitializeDayOfMonthList()
		{
			string[] days = new string[ 28 ];
			for ( int i = 0; i < days.Length; ++i )
			{
				days[ i ] = Convert.ToString( i + 1 );
			}

			DayOfMonthList.DataSource = days;
			DayOfMonthList.DataBind();
		}

		/// <summary>
		/// Initializes the time of day values in the dropdown list.
		/// </summary>
		private void InitializeTimeOfDayList()
		{
			string[] times = new string[ 96 ];

			int hours = 0;
			int minutes = 0;
			for( int i = 0; i < times.Length; ++i )
			{
				DateTime dt = new DateTime( 1962, 2, 11, hours, minutes, 0 );
				times[ i ] = dt.ToString( "t" );

				minutes += 15;
				if ( minutes > 45 )
				{
					++hours;
					minutes = 0;
				}
			}

			TimeOfDayList.DataSource = times;
			TimeOfDayList.DataBind();
		}

		/// <summary>
		/// Initializes the DayOfWeek dropdown list containing the names of the days of the week.
		/// </summary>
		private void InitializeDayOfWeekList()
		{
			DayOfWeekList.DataSource = new DateTimeFormatInfo().DayNames;
			DayOfWeekList.DataBind();
		}

		/// <summary>
		/// Initializes the Format dropdown list containting the report formatting types.
		/// </summary>
		private void InitializeReportFormat()
		{
			string[] formats = new string[] { GetString( "CSVFORMAT" ) };
			FormatList.DataSource = formats;
			FormatList.DataBind();
		}

		/// <summary>
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load( object sender, System.EventArgs e )
		{
			// connection
			web = Session[ "Connection" ] as iFolderAdmin;

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				EnableReportingLabel.Text = GetString( "ENABLEREPORTING" );
				FrequencyList.Items[ 0 ].Text = GetString( "DAILY" );
				FrequencyList.Items[ 1 ].Text = GetString( "WEEKLY" );
				FrequencyList.Items[ 2 ].Text = GetString( "MONTHLY" );
				FrequencyList.SelectedIndex = 0;

				ReportLocation.Items[ 0 ].Text = GetString( "REPORTIFOLDER" );
				ReportLocation.Items[ 1 ].Text = GetString( "REPORTDIRECTORY" );
				ReportLocation.SelectedIndex = 1;

				EnableReporting.Checked = IsReportingEnabled();

				// Populate the dropdown lists.
				InitializeTimeOfDayList();
				InitializeDayOfWeekList();
				InitializeDayOfMonthList();
				InitializeReportFormat();
			}
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender(object sender, EventArgs e)
		{
			// Set the breadcrumb list.
			BuildBreadCrumbList();

			// Initialize the day control.
			DisplayDayControl();

			// Display the summary.
			DisplaySummary();
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString( string key )
		{
			return rm.GetString( key );
		}

		/// <summary>
		/// Returns whether or not reporting is enabled.
		/// </summary>
		/// <returns>True if reporting is enabled, otherwise false is returned.</returns>
		protected bool IsReportingEnabled()
		{
			return true;
		}

		/// <summary>
		/// Event handler that gets called when the day of month selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDayOfMonthList_Changed( object sender, EventArgs e )
		{
			DisplaySummary();
		}

		/// <summary>
		/// Event handler that gets called when the day of week selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDayOfWeekList_Changed( object sender, EventArgs e )
		{
			DisplaySummary();
		}

		/// <summary>
		/// Event handler that gets called when the FrequencyList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFrequencyList_Changed( object sender, EventArgs e )
		{
			DisplayDayControl();
			DisplaySummary();
		}

		/// <summary>
		/// Event handler that gets called when the report FormatList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFormatList_Changed( object sender, EventArgs e )
		{
			DisplaySummary();
		}

		/// <summary>
		/// Event handler that gets called when the report location selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnReportLocation_Changed( object sender, EventArgs e )
		{
			DisplaySummary();
		}

		/// <summary>
		/// Event handler that gets called when the time of day selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnTimeOfDayList_Changed( object sender, EventArgs e )
		{
			DisplaySummary();
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
