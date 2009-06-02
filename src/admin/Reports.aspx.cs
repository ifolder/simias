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
using System.Xml;
using System.Threading;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for Reports.
	/// </summary>
	public class Reports : System.Web.UI.Page
	{
		#region Class Members

		/// <summary>
		/// Log
		/// </summary>
		private static readonly iFolderWebLogger log = new iFolderWebLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Name of the iFolder report settings.
		/// </summary>
		private const string ReportSettingName = "iFolderSystemReportConfiguration";

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Logged in admin system rights instance
		/// </summary>
		UserSystemAdminRights uRights;
		
		/// <summary>
		/// Logged in user system rights value
		/// </summary>
		int sysAccessPolicy = 0;

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

		/// <summary>
		/// Save button control.
		/// </summary>
		protected Button SaveReportConfig;

		/// <summary>
		/// Cancel button control.
		/// </summary>
		protected Button CancelReportConfig;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the report ID in the ViewState.
		/// </summary>
		private string ReportID
		{
			get { return ViewState[ "ReportID" ] as string; }
			set { ViewState[ "ReportID" ] = value; }
		}

		/// <summary>
		/// Gets or sets the report name in the ViewState.
		/// </summary>
		private string ReportName
		{
			get { return ViewState[ "ReportName" ] as string; }
			set { ViewState[ "ReportName" ] = value; }
		}

		/// <summary>
		/// Gets or set the report path in the ViewState.
		/// </summary>
		private string ReportPath
		{
			get { return ViewState[ "ReportPath" ] as string; }
			set { ViewState[ "ReportPath" ] = value; }
		}

		#endregion

		#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList()
		{
			TopNav.AddBreadCrumb( GetString( "REPORTS" ), null );
			// Pass this page information to create the help link
			TopNav.AddHelpLink(GetString("REPORTS"));
		}

		/// <summary>
		/// Correlates the TimeSpan object to a list index.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		private int ConvertTimeToIndex( TimeSpan time )
		{
			int index = time.Hours * 4;

			if ( ( time.Minutes > 0 ) && ( time.Minutes <= 15 ) )
			{
				index += 1;
			}
			else if ( ( time.Minutes > 15 ) && ( time.Minutes <= 30 ) )
			{
				index += 2;
			}
			else if ( ( time.Minutes > 30 ) && ( time.Minutes <= 45 ) )
			{
				index += 3;
			}

			return index;
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
                /// Returns the day control based on the frequency control value.
                /// </summary>
                private string GetFrequencyListValue()
                {
                        switch ( FrequencyList.SelectedIndex )
                        {
                                case 0:
					return "Daily";

                                case 1:
					return "Weekly";

                                case 2:
					return "Monthly";
                        }
			return "Daily";
                }

		/// <summary>
		/// Displays a summary of the report configuration selections.
		/// </summary>
		/// <param name="hasChanged"></param>
		private void DisplaySummary( bool hasChanged)
		{
			string location = null;
			switch ( ReportLocation.SelectedIndex )
			{
				case 0:
					location = String.Format( "{0} {1}", GetString( "REPORTIFOLDERTAG" ), ReportName );
					break;

				case 1:
					location = String.Format( "{0} {1}", GetString( "REPORTDIRECTORYTAG" ), ReportPath );
					break;
			}

			switch ( FrequencyList.SelectedIndex )
			{
				case 0:
					Summary.Text = 
						String.Format( 
							GetString( "GENERATEDAILYREPORT" ), 
							TimeOfDayList.SelectedValue,
							location );
					break;

				case 1:
					Summary.Text = 
						String.Format( 
							GetString( "GENERATEWEEKLYREPORT" ), 
							DayOfWeekList.SelectedValue, 
							TimeOfDayList.SelectedValue,
							location );
					break;

				case 2:
					Summary.Text = 
						String.Format( 
							GetString( "GENERATEMONTHLYREPORT" ), 
							DayOfMonthList.SelectedValue, 
							TimeOfDayList.SelectedValue,
							location );
					break;
			}

			// Enable or disable the save and cancel buttons.
			SaveReportConfig.Enabled = CancelReportConfig.Enabled = hasChanged;
		}

		/// <summary>
		/// Gets the report configuration from the server.
		/// </summary>
		private void GetReportConfiguration()
		{
			// Get the system information.
			iFolderSystem system = web.GetSystem();

			// If there are no report settings saved, use the default settings.
			string settings = web.GetiFolderSetting(system.ReportiFolderID, ReportSettingName );
			ReportConfig rc = ( settings != null ) ? new ReportConfig( settings ) : new ReportConfig();

			// Set the web page selections.
			EnableReporting.Checked = Summary.Visible = rc.Enabled;

			FrequencyList.Enabled = rc.Enabled;
			FrequencyList.SelectedIndex = ( int )rc.Frequency;

			ReportLocation.Enabled = rc.Enabled;
			ReportLocation.SelectedIndex = rc.IsiFolder ? 0 : 1;

			DayOfMonthList.Enabled = rc.Enabled;
			DayOfMonthList.SelectedIndex = rc.DayOfMonth - 1;

			DayOfWeekList.Enabled = rc.Enabled;
			DayOfWeekList.SelectedIndex = ( int )rc.Weekday;

			TimeOfDayList.Enabled = rc.Enabled;
			TimeOfDayList.SelectedIndex = ConvertTimeToIndex( rc.TimeOfDay );

			FormatList.Enabled = rc.Enabled;
			FormatList.SelectedIndex = ( int )rc.Format;

			// Save the report settings in the ViewState.
			ReportID = system.ReportiFolderID;
			ReportName = system.ReportiFolderName;
			ReportPath = system.ReportPath;
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
			log.Debug( Context, "Current short time format = {0}", new DateTimeFormatInfo().ShortTimePattern );
			string[] times = new string[ 96 ];

			int hours = 0;
			int minutes = 0;
			for( int i = 0; i < times.Length; ++i )
			{
				DateTime dt = DateTime.Parse( new TimeSpan( hours, minutes, 0 ).ToString() );
				times[ i ] = Utils.ToDateTimeString( "t", dt );

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
			string userID = Session[ "UserID" ] as String;
			if(userID != null)
				sysAccessPolicy = web.GetUserSystemRights(userID, null);
			else
				sysAccessPolicy = 0; 
			uRights = new UserSystemAdminRights(sysAccessPolicy);
			if(uRights.ReportsGenerationAllowed == false)
				Page.Response.Redirect(String.Format("Error.aspx?ex={0}&Msg={1}",GetString( "ACCESSDENIED" ), GetString( "ACCESSDENIEDERROR" )));

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				EnableReportingLabel.Text = GetString( "ENABLEREPORTING" );
				FrequencyList.Items[ 0 ].Text = GetString( "DAILY" );
				FrequencyList.Items[ 1 ].Text = GetString( "WEEKLY" );
				FrequencyList.Items[ 2 ].Text = GetString( "MONTHLY" );

				ReportLocation.Items[ 0 ].Text = GetString( "REPORTIFOLDER" );
				ReportLocation.Items[ 1 ].Text = GetString( "REPORTDIRECTORY" );

				SaveReportConfig.Text = GetString( "SAVE" );
				CancelReportConfig.Text = GetString( "CANCEL" );

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

			// Get the report configuration.
			GetReportConfiguration();

			// Initialize the day control.
			DisplayDayControl();

			// Display the summary.
			DisplaySummary( false );
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
		/// Event handler that gets called when the cancel report button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnCancelReport_Click( object sender, EventArgs e )
		{
			// Restore to previous settings.
			GetReportConfiguration();
			DisplayDayControl();
			DisplaySummary( false );
		}

		/// <summary>
		/// Event handler that gets called when the day of month selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDayOfMonthList_Changed( object sender, EventArgs e )
		{
			DisplaySummary( true );
		}

		/// <summary>
		/// Event handler that gets called when the day of week selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDayOfWeekList_Changed( object sender, EventArgs e )
		{
			DisplaySummary( true );
		}

		/// <summary>
		/// Event handler that gets called when the enable reporting checkbox is checked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnEnableReporting_Changed( object sender, EventArgs e )
		{
			bool isChecked = ( sender as CheckBox ).Checked;

			Summary.Visible =
			FrequencyList.Enabled =
			ReportLocation.Enabled =
			DayOfMonthList.Enabled =
			DayOfWeekList.Enabled =
			TimeOfDayList.Enabled =
			FormatList.Enabled = isChecked;

			SaveReportConfig.Enabled = CancelReportConfig.Enabled = true;
		}

		/// <summary>
		/// Event handler that gets called when the FrequencyList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFrequencyList_Changed( object sender, EventArgs e )
		{
			DisplayDayControl();
			DisplaySummary( true );
		}

		/// <summary>
		/// Event handler that gets called when the report FormatList selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnFormatList_Changed( object sender, EventArgs e )
		{
			DisplaySummary( true );
		}

		/// <summary>
		/// Event handler that gets called when the report location selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnReportLocation_Changed( object sender, EventArgs e )
		{
			DisplaySummary( true );
		}

		/// <summary>
		/// Event handler that gets called when the save report button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnSaveReport_Click( object sender, EventArgs e )
		{
			ReportConfig rc = new ReportConfig();

			// Set the new values in the report configuration.
			rc.Enabled = EnableReporting.Checked;

			// Report frequency.
			rc.Frequency = ( ReportConfig.ReportFrequency )Enum.Parse( 
				typeof( ReportConfig.ReportFrequency ), 
				GetFrequencyListValue(), 
				true );

			// Report time of day.
			DateTime dt = DateTime.ParseExact( TimeOfDayList.SelectedValue.ToString(),"t",Thread.CurrentThread.CurrentUICulture.DateTimeFormat,DateTimeStyles.AllowWhiteSpaces );
			rc.TimeOfDay = new TimeSpan( dt.Hour, dt.Minute, 0 );

			// Report day of month.
			rc.DayOfMonth = Convert.ToInt32( DayOfMonthList.SelectedValue );

			// Report day of week.
			rc.Weekday = ( DayOfWeek )Enum.Parse(
				typeof( DayOfWeek ),
				DayOfWeekList.SelectedValue,
				true );

			// Report format.
			switch ( FormatList.SelectedIndex )
			{
				case 0:
					rc.Format = ReportConfig.ReportFormat.CSV;
					break;
			}

			// Report location.
			rc.IsiFolder = ( ReportLocation.SelectedIndex == 0 ) ? true : false;

			// Call to set the settings.
			web.SetiFolderSetting( ReportID, ReportSettingName, rc.ToString() );

			// Disable the save and cancel buttons.
			SaveReportConfig.Enabled = CancelReportConfig.Enabled = false; 
		}

		/// <summary>
		/// Event handler that gets called when the time of day selection changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnTimeOfDayList_Changed( object sender, EventArgs e )
		{
			DisplaySummary( true );
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

		#region Report Configuration Object

		/// <summary>
		/// Class used to implement the ifolder system report configuration.
		/// </summary>
		private class ReportConfig
		{
			#region Class Members

			/// <summary>
			/// Output format for the report.
			/// </summary>
			public enum ReportFormat
			{
				/// <summary>
				/// Comma Separated Value (CSV) format.
				/// </summary>
				CSV
			}

			/// <summary>
			/// How often the report is generated.
			/// </summary>
			public enum ReportFrequency
			{
				/// <summary>
				/// Generate the report every day.
				/// </summary>
				Daily,

				/// <summary>
				/// Generate the report once a week.
				/// </summary>
				Weekly,

				/// <summary>
				/// Generate the report once a month.
				/// </summary>
				Monthly
			}

			/// <summary>
			/// Indicates if reporting is enabled/disabled.
			/// </summary>
			private bool enabled = false;

			/// <summary>
			/// How often to generate the report.
			/// </summary>
			private ReportFrequency frequency = ReportFrequency.Daily;

			/// <summary>
			/// The day of the week to generate the report if frequency
			/// is weekly.
			/// </summary>
			private DayOfWeek weekday = DayOfWeek.Sunday;

			/// <summary>
			/// The day of the month to generate the report if frequency
			/// is monthly.
			/// </summary>
			private int dayOfMonth = 1;

			/// <summary>
			/// The time of day to generate the report. This value represents
			/// the fraction of the day elapsed since midnight.
			/// </summary>
			private TimeSpan timeOfDay = new TimeSpan( 0, 0, 0 );

			/// <summary>
			/// Indicates if the report output path is an iFolder.
			/// </summary>
			private bool isiFolder = false;

			/// <summary>
			/// The output report format to use.
			/// </summary>
			private ReportFormat format = ReportFormat.CSV;

			#endregion

			#region Properties

			/// <summary>
			/// Gets or sets the day of the month to generate report
			/// if frequency is Monthly.
			/// </summary>
			public int DayOfMonth
			{
				get { return dayOfMonth; }
				set
				{
					if ( ( value >= 1 ) && ( value <= 28 ) )
					{
						dayOfMonth = value;
					}
					else
					{
						throw new ArgumentOutOfRangeException();
					}
				}
			}

			/// <summary>
			/// Gets whether reporting is enabled.
			/// </summary>
			public bool Enabled
			{
				get { return enabled; }
				set { enabled = value; }
			}

			/// <summary>
			/// Gets or sets the output report format to us.
			/// </summary>
			public ReportFormat Format
			{
				get { return format; }
				set { format = value; }
			}

			/// <summary>
			/// Gets or sets the frequency of report generation.
			/// </summary>
			public ReportFrequency Frequency
			{
				get { return frequency; }
				set { frequency = value; }
			}

			/// <summary>
			/// Gets or sets whether the ReportPath is an iFolder.
			/// </summary>
			public bool IsiFolder
			{
				get { return isiFolder; }
				set { isiFolder = value; }
			}

			/// <summary>
			/// Gets or sets the time of day to generate the report. This 
			/// value represents the fraction of the day elapsed since 
			/// midnight.
			/// </summary>
			public TimeSpan TimeOfDay
			{
				get { return timeOfDay; }
				set { timeOfDay = value; }
			}

			/// <summary>
			/// Gets or sets the day of the week to generate report
			/// if frequency is Weekly.
			/// </summary>
			public DayOfWeek Weekday
			{
				get { return weekday; }
				set { weekday = value; }
			}

			#endregion

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			public ReportConfig()
			{
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="settings">Settings string from the server.</param>
			public ReportConfig( string settings )
			{
				// Parse the settings string into the object members.
				ParseSettings( settings );
			}

			#endregion

			#region Private Methods

			/// <summary>
			/// Parses the settings string into its configuration settings.
			/// </summary>
			/// <param name="settings"></param>
			private void ParseSettings( string settings )
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml( settings );

				// Get whether reporting is enabled.
				Enabled = Boolean.Parse( doc.DocumentElement.GetAttribute( "enabled" ) );
				if ( Enabled )
				{
					// Get the frequency of the report generation.
					XmlElement element = doc.DocumentElement.SelectSingleNode( "generate" ) as XmlElement;
					if ( element != null )
					{
						string s = element.GetAttribute( "frequency" );
						if ( s != String.Empty )
						{
							Frequency = ( ReportFrequency )Enum.Parse( 
								typeof( ReportFrequency ), 
								s, 
								true );
						}
						else
						{
							throw new ApplicationException( "Invalid report settings format. No frequency attribute was specified." );
						}
					}
					else
					{
						throw new ApplicationException( "Invalid report settings format. No generate element was specified." );
					}


					// All frequencies will have a time specified.
					XmlElement child = element.SelectSingleNode( "time" ) as XmlElement;
					if ( child != null )
					{
						DateTime dt = DateTime.Parse( child.InnerText );
						TimeOfDay = new TimeSpan( dt.Hour, dt.Minute, 0 );
					}
					else
					{
						throw new ApplicationException( "Invalid report settings format. No time element was specified." );
					}

					// Depending on the frequency type, the time parameters will be different.
					switch ( frequency )
					{
						case ReportFrequency.Daily:
							break;

						case ReportFrequency.Weekly:
							// Get the day of the week.
							child = element.SelectSingleNode( "dayofweek" ) as XmlElement;
							if ( child != null )
							{
								Weekday = ( DayOfWeek )Enum.Parse( typeof( DayOfWeek ), child.InnerText, true );
							}
							else
							{
								throw new ApplicationException( "Invalid report settings format. No day of week element was specified." );
							}
							break;

						case ReportFrequency.Monthly:
							// Get the day of the month.
							child = element.SelectSingleNode( "dayofmonth" ) as XmlElement;
							if ( child != null )
							{
								DayOfMonth = Convert.ToInt32( child.InnerText );
							}
							else
							{
								throw new ApplicationException( "Invalid report settings format. No day of month element was specified." );
							}
							break;
					}

					// Get the report location information.
					element = doc.DocumentElement.SelectSingleNode( "location" ) as XmlElement;
					if ( element != null )
					{
						IsiFolder = ( element.InnerText == "ifolder" ) ? true : false;
					}
					else
					{
						throw new ApplicationException( "Invalid report settings format. No location element was specified." );
					}

					// Get the report format.
					element = doc.DocumentElement.SelectSingleNode( "format" ) as XmlElement;
					if ( element != null )
					{
						Format = ( ReportFormat )Enum.Parse( typeof( ReportFormat ), element.InnerText, true );
					}
					else
					{
						throw new ApplicationException( "Invalid report settings format. No format element was specified." );
					}
				}
			}
			
			#endregion

			#region Public Methods

			/// <summary>
			/// Converts the report object to a settings string.
			/// </summary>
			/// <returns>A string that contains the report configuration settings.</returns>
			public override string ToString()
			{
				XmlDocument doc = new XmlDocument();

				// Create the document root.
				XmlElement rootElement = doc.CreateElement( "report" );
				rootElement.SetAttribute( "enabled", Enabled.ToString() );
				doc.AppendChild( rootElement );

				// If reporting is enabled continue to build the string. Otherwise we're done.
				if ( Enabled )
				{
					// Create the frequency of the report generation.
					XmlElement parent = doc.CreateElement( "generate" );
					parent.SetAttribute( "frequency", Frequency.ToString() );
					rootElement.AppendChild( parent );

					// Create the time element.
					XmlElement child = doc.CreateElement( "time" );
					child.InnerText = TimeOfDay.ToString();
					parent.AppendChild( child );

					// Depending on the frequency type, the time parameters will be different.
					switch ( Frequency )
					{
						case ReportFrequency.Daily:
							break;

						case ReportFrequency.Weekly:
							// Create the day of the week.
							child = doc.CreateElement( "dayofweek" );
							child.InnerText = Weekday.ToString();
							parent.AppendChild( child );
							break;

						case ReportFrequency.Monthly:
							// Create the day of the month.
							child = doc.CreateElement( "dayofmonth" );
							child.InnerText = DayOfMonth.ToString();
							parent.AppendChild( child );
							break;
					}

					// Create the report location information.
					parent = doc.CreateElement( "location" );
					parent.InnerText = ( IsiFolder ) ? "ifolder" : "directory";
					rootElement.AppendChild( parent );

					// Create the report format.
					parent = doc.CreateElement( "format" );
					parent.InnerText = Format.ToString();
					rootElement.AppendChild( parent );
				}

				return doc.InnerXml;
			}

			#endregion
		}

		#endregion
	}
}
