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
*                 $Author: Mike Lasky <mlasky@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Timers;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Policy;
using Simias.Service;
using Simias.Storage;

namespace Simias.Server
{
	/// <summary>
	/// Implements the report collection. This is the collection intended
	/// for reports that are synced between master and slave servers and
	/// clients.
	/// </summary>
	public class Report
	{
		#region Class Members

		/// <summary>
		/// Report columns enums.
		/// </summary>
		private enum ColumnID
		{
			ReportTime,
			iFolderSystem,
			iFolderServer,
			iFolderID,
			iFolderName,
			iFolderSize,
			iFolderPath,
			iFolderQuota,
			MemberCount,
			FileCount,
			DirectoryCount,
			OwnerID,
			OwnerName,
			OwnerCN,
			OwnerDN,
			OwnerQuota,
			OwnerLastLogin,
			OwnerDisabled,
			PreviousOwner,
			OrphanedOwner,
			LastSyncTime
		};

		/// <summary>
		/// Count of report columns.
		/// </summary>
		private static readonly int count = Enum.GetNames( typeof( ColumnID ) ).Length;

		/// <summary>
		/// Report column collection.
		/// </summary>
		private static readonly ReportColumn[] columns = new ReportColumn[count];

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		/// <summary>
		/// Well-known identifier for the report collection.
		/// </summary>
		private static string reportCollectionID;

		/// <summary>
		/// Report collection name.
		/// </summary>
		private static string reportCollectionName;
		
		/// <summary>
		/// Resource manager.
		/// </summary>
		private static ResourceManager resourceManager;

		/// <summary>
		/// Watches for the creation or deletion of the report settings node.
		/// </summary>
		private EventSubscriber settingsWatcher = null;

		/// <summary>
		/// Watches for changes on the report settings node contained in the report collection.
		/// </summary>
		private EventSubscriber configurationWatcher = null;

		/// <summary>
		/// Timer that is used to schedule report generation.
		/// </summary>
		private System.Timers.Timer timer = new System.Timers.Timer();

		/// <summary>
		/// Store object.
		/// </summary>
		private Store store;

		/// <summary>
		/// The report collection object.
		/// </summary>
		private Collection reportCollection;

		/// <summary>
		/// The node identifier of the report configuration settings node object.
		/// </summary>
		private string reportConfigNodeID;

		/// <summary>
		/// The report time for the next report.
		/// </summary>
		private DateTime nextReportTime = DateTime.MinValue;

		/// <summary>
		/// The report time for the current report.
		/// </summary>
		private DateTime currentReportTime;

        /// <summary>
        /// Culture to use from environment, Mono does not take the LC values automatically
        /// </summary>
        private static CultureInfo cli = null;


		#endregion

		#region Properties

		/// <summary>
		/// Returns the currently configured report path, either the
		/// local directory path or the ifolder report path.
		/// </summary>
		public static string CurrentReportPath
		{
			get
			{
				Report report = new Report();
				ReportConfig rc = report.GetReportConfiguration();

			        //NOTE : Check to ensure ReportConfig availability. If reports is not configured
				//ReportConfig will not be available 

				if (rc != null && rc.IsiFolder)
				{
				        Store store = Store.GetStore();
				        Collection reportCollection = store.GetSingleCollectionByType( "Reports" );
        				DirNode dirNode = reportCollection.GetRootDirectory();

					return dirNode.GetFullPath( reportCollection );
				} else {
 				        return ReportPath;
 				}
			}
		}

		/// <summary>
		/// Returns the well-known report collection identifier.
		/// </summary>
		public static string ReportCollectionID
		{
			get { 
			        if (reportCollectionID == null)
				{
				        Store store = Store.GetStore();
					reportCollectionID = store.GetSingleCollectionByType( "Reports" ).ID;
				}
			        return reportCollectionID;
			}
		}

		/// <summary>
		/// Returns the well-known report collection name.
		/// </summary>
      public static string ReportCollectionName
        {
            get
            {
                 return reportCollectionName; 
           }
        }

		/// <summary>
		/// Returns the absolute path to the report directory.
		/// </summary>
		public static string ReportPath
		{
			get { return Path.Combine( Store.StorePath, "report" ); }
		}

		/// <summary>
		/// Returns the absolute path to the iFolder report directory.
		/// </summary>
		public static string ReportiFolderPath
		{
			get
			{
				Store store = Store.GetStore();
				Collection report = store.GetSingleCollectionByType( "Reports" );

				if ( report == null )
				{
					throw new SimiasException( "Cannot find report collection." );
				}

				DirNode root = report.GetRootDirectory();
				if ( root == null )
				{
					throw new SimiasException( "Cannot find root DirNode for report collection." );
				}

				return root.GetFullPath( report );
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Static constructor for the object.
		/// </summary>
		static Report()
		{
			// Initialize the resources.
			resourceManager = new ResourceManager( "Simias.Server.Report", Assembly.GetExecutingAssembly() );

 			// Initalize the name of the report collection.
 			Store store = Store.GetStore();
			//get the language
			try
			{
			string locale = getLocaleString();
            if(locale != null)
            {
                // we will atleast have POSIX
                log.Debug("Current Culture in environment {0}", locale);
                if (locale.Equals("POSIX") == false)
                {
                    string lang = locale.Substring(0, 5);
                    lang = lang.Replace('_', '-');
                    cli = new CultureInfo(lang, true);
                    log.Debug("Current Culture in environment {0}, {1}", locale, lang);
                }
            }
			}
			catch(Exception ex)
			{
				log.Error("Error while trying to create language culture. {0}", ex.Message);
			}
 		            
            //search to see if its already exists; as domain name is modifiable 
            Collection report = store.GetSingleCollectionByType("Reports");
            if (report != null) reportCollectionName=report.Name;
            else
            {   Domain domain = store.GetDomain(store.DefaultDomain);
                //Note : During First instance, Domain has just been created. So take serverName from the configuration files.
                Simias.Configuration config = Store.Config;
                string serverName = config.Get("Server", "Name");
                reportCollectionName = domain.Name + "-" + serverName + "-" + GetString("REPORTS"); //construct path 
            }
			// columns
			columns[ ( int )ColumnID.ReportTime ]     = new ReportColumn( GetString( "REPORT_TIME" ), "{0:G}" );
			columns[ ( int )ColumnID.iFolderSystem ]  = new ReportColumn( GetString( "IFOLDER_SYSTEM" ) );
			columns[ ( int )ColumnID.iFolderServer ]  = new ReportColumn( GetString( "IFOLDER_SERVER" ) );
			columns[ ( int )ColumnID.iFolderID ]      = new ReportColumn( GetString( "IFOLDER_ID" ) );
			columns[ ( int )ColumnID.iFolderName ]    = new ReportColumn( GetString( "IFOLDER_NAME" ) );
			columns[ ( int )ColumnID.iFolderSize ]    = new ReportColumn( GetString( "IFOLDER_SIZE" ), "{0:N02}" );
			columns[ ( int )ColumnID.iFolderPath ]    = new ReportColumn( GetString( "IFOLDER_PATH" ) );
			columns[ ( int )ColumnID.iFolderQuota ]   = new ReportColumn( GetString( "IFOLDER_QUOTA" ), "{0:N02}" );
			columns[ ( int )ColumnID.MemberCount ]    = new ReportColumn( GetString( "MEMBER_COUNT" ) );
			columns[ ( int )ColumnID.FileCount ]      = new ReportColumn( GetString( "FILE_COUNT" ) );
			columns[ ( int )ColumnID.DirectoryCount ] = new ReportColumn( GetString( "DIRECTORY_COUNT" ) );
			columns[ ( int )ColumnID.OwnerID ]        = new ReportColumn( GetString( "OWNER_ID" ) );
			columns[ ( int )ColumnID.OwnerName ]      = new ReportColumn( GetString( "OWNER_NAME" ) );
			columns[ ( int )ColumnID.OwnerCN ]        = new ReportColumn( GetString( "OWNER_CN" ) );
			columns[ ( int )ColumnID.OwnerDN ]        = new ReportColumn( GetString( "OWNER_DN" ) );
			columns[ ( int )ColumnID.OwnerQuota ]     = new ReportColumn( GetString( "OWNER_QUOTA" ), "{0:N02}" );
			columns[ ( int )ColumnID.OwnerLastLogin ] = new ReportColumn( GetString( "OWNER_LAST_LOGIN" ), "{0:G}" );
			columns[ ( int )ColumnID.OwnerDisabled ]  = new ReportColumn( GetString( "OWNER_DISABLED" ) );
			columns[ ( int )ColumnID.PreviousOwner ]  = new ReportColumn( GetString( "PREVIOUS_OWNER" ) );
			columns[ ( int )ColumnID.OrphanedOwner ]  = new ReportColumn( GetString( "ORPHANED_OWNER" ) );
			columns[ ( int )ColumnID.LastSyncTime ]   = new ReportColumn( GetString( "LAST_SYNC_TIME" ), "{0:G}" );
		}

		static String getLocaleString()
		{
			string retval = getLocaleStringForVariable("LC_ALL");
			if( retval != null)
			{
				log.Debug("locale from LC_ALL: {0}", retval);
				return retval;
			}
			retval = getLocaleStringForVariable("LANG");
			if( retval != null)
			{
				log.Debug("locale from LANG");
				return retval;
			}
			retval = getLocaleStringForVariable("LC_LC_CTYPE");
			log.Debug("locale from LC_TYPE");
			return retval;
		}
		static String getLocaleStringForVariable(String key)
		{
			string retval = null;
			string locale = Environment.GetEnvironmentVariable(key);
			if( locale != null && locale.Length >= 5)
				retval = locale.Substring(0, 5);
			return retval;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public Report()
		{
			// Get the store object.
			store = Store.GetStore();

			// Set the event handler for the timer.
			timer.Enabled = false;
			timer.AutoReset = false;
			timer.Elapsed += new System.Timers.ElapsedEventHandler( GenerateReportThread );

			// Get the report collection object.

			reportCollection = store.GetSingleCollectionByType( "Reports" );
			reportCollectionID = reportCollection.ID;

                        //not necessary anymore .. 
			if ( reportCollectionID == null )
			{
				log.Error( "The report collection {0} cannot be found.", ReportCollectionName );
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Cancels any reports scheduled to be generated.
		/// </summary>
		private void CancelReportGeneration()
		{
			if ( timer.Enabled )
			{
                if (cli == null)
                {
                    log.Debug("Canceling report timer. {0}, {1}, {2}", DateTimeFormatInfo.CurrentInfo.FullDateTimePattern, Thread.CurrentThread.CurrentCulture.DateTimeFormat.FullDateTimePattern, Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
                }
                else
                {
                    log.Debug("Canceling report timer. {0}, cli-{1}, {2}", DateTimeFormatInfo.CurrentInfo.FullDateTimePattern, cli.DateTimeFormat.FullDateTimePattern, cli.TwoLetterISOLanguageName);
                }
				nextReportTime = DateTime.MinValue;
				timer.Stop();
			}
		}

		/// <summary>
		/// Event handler for when the settings node is modified.
		/// </summary>
		/// <param name="args">Object that describes the event.</param>
		private void ConfigurationWatcher_NodeChanged( NodeEventArgs args )
		{
			log.Debug( "Report configuration settings have changed." );

			// Schedule the report to be generated.
			ScheduleReportGeneration();
		}

		/// <summary>
		/// Starts the monitoring the report settings node.
		/// </summary>
		/// <param name="id">The node identifer of the report configuration object.</param>
		private void ConfigurationWatcherStart( string id )
		{
			// Cancel any outstanding events.
			ConfigurationWatcherStop();

			// Remember this node ID.
			reportConfigNodeID = id;

			// Setup an event watcher on the report settings node.
			configurationWatcher = new EventSubscriber();
			configurationWatcher.NodeIDFilter = id;
			configurationWatcher.NodeChanged += new NodeEventHandler( ConfigurationWatcher_NodeChanged );
			configurationWatcher.Enabled = true;

			// Schedule the report to be generated.
			ScheduleReportGeneration();
		}

		/// <summary>
		///  Stops the configuration event watcher and cancels any scheduled
		///  report generation.
		/// </summary>
		private void ConfigurationWatcherStop()
		{
			if ( configurationWatcher != null )
			{
				configurationWatcher.Enabled = false;
				configurationWatcher.NodeChanged -= new NodeEventHandler( ConfigurationWatcher_NodeChanged );
				configurationWatcher = null;

				// Reset the node ID.
				reportConfigNodeID = null;

				// If there is a report scheduled, cancel it.
				CancelReportGeneration();
			}
		}

		/// <summary>
		/// Creates a file node for the specified file.
		/// </summary>
		/// <param name="filePath">Absolute path to file.</param>
		public void CreateFileNode( string filePath )
		{
			// Get the root directory node. All reports will be placed in the root of the ifolder.
			DirNode dirNode = reportCollection.GetRootDirectory();

			// Create the file node.
			FileNode fileNode = new FileNode( reportCollection, dirNode, Path.GetFileName( filePath ) );
			reportCollection.Commit( fileNode );
		}

		/// <summary>
		/// Generates a report
		/// </summary>
		public void GenerateReport()
		{
			bool hadException = false;
			const double MB = 1024 * 1024;
			ReportConfig config = GetReportConfiguration();

			string reportPath = null;
			if ( config.IsiFolder )
			{
				DirNode dirNode = reportCollection.GetRootDirectory();
				reportPath = dirNode.GetFullPath( reportCollection );
			}
			else
			{
				reportPath = ReportPath;
			}

			string fileName = String.Format( "ifolder-{0}-{1}.csv", 
				Environment.MachineName,
				currentReportTime.ToString("yyyyMMdd-HHmmss" ) );

			string filePath = Path.Combine( reportPath, fileName );

			log.Debug( "Report file name = {0}", filePath);
			StreamWriter file = File.CreateText( filePath );

			try
			{
				Domain domain = store.GetDomain( store.DefaultDomain );

				// headers
				WriteHeaderRow( file, columns );

				// list iFolders
				ICSList ifolders = store.GetCollectionsByType( "iFolder" );

				foreach( ShallowNode sn in ifolders )
				{
					object[] cells = new object[ count ];

					Collection ifolder = store.GetCollectionByID( sn.ID );
					Member owner = domain.GetMemberByID( ifolder.Owner.UserID );

					// cells
                    if(cli != null)
                    {
                        cells[(int)ColumnID.ReportTime] = currentReportTime.ToString(cli);
                        cells[(int)ColumnID.OwnerLastLogin] = owner.Properties.GetSingleProperty("LastLogin").ToString(cli);
                        cells[(int)ColumnID.LastSyncTime] = ifolder.Properties.GetSingleProperty("LastModified").ToString(cli);
                    }
                    else
                    {
                        cells[(int)ColumnID.ReportTime] = currentReportTime.ToString(DateTimeFormatInfo.CurrentInfo.FullDateTimePattern);
                        cells[(int)ColumnID.OwnerLastLogin] = owner.Properties.GetSingleProperty("LastLogin").ToString(DateTimeFormatInfo.CurrentInfo.FullDateTimePattern);
                        cells[(int)ColumnID.LastSyncTime] = ifolder.Properties.GetSingleProperty("LastModified").ToString(DateTimeFormatInfo.CurrentInfo.FullDateTimePattern);
                    }
					cells[ ( int )ColumnID.iFolderSystem ] = domain.Name;
					cells[ ( int )ColumnID.iFolderServer ] = Environment.MachineName;
					cells[ ( int )ColumnID.iFolderID ] = ifolder.ID;
					cells[ ( int )ColumnID.iFolderName ] = ifolder.Name;
					cells[ ( int )ColumnID.iFolderSize ] = ifolder.StorageSize / MB;
					cells[ ( int )ColumnID.iFolderPath ] = ifolder.UnmanagedPath;
					cells[ ( int )ColumnID.iFolderQuota ] = DiskSpaceQuota.GetLimit( ifolder ) / MB;
					cells[ ( int )ColumnID.MemberCount ] = ifolder.GetMemberList().Count;
					cells[ ( int )ColumnID.FileCount ] = ifolder.GetNodesByType( NodeTypes.FileNodeType ).Count;
					cells[ ( int )ColumnID.DirectoryCount ] = ifolder.GetNodesByType( NodeTypes.DirNodeType ).Count;
					cells[ ( int )ColumnID.OwnerID ] = owner.UserID;
					cells[ ( int )ColumnID.OwnerName ] = owner.FN;
					cells[ ( int )ColumnID.OwnerCN ] = owner.Name;
					cells[ ( int )ColumnID.OwnerDN ] = owner.Properties.GetSingleProperty( "DN" );
					cells[ ( int )ColumnID.OwnerQuota ] = DiskSpaceQuota.Get( owner ).Limit / MB;
					cells[ ( int )ColumnID.OwnerDisabled ] = domain.IsLoginDisabled( owner.UserID );
					cells[ ( int )ColumnID.PreviousOwner ] = ifolder.PreviousOwner;
					cells[ ( int )ColumnID.OrphanedOwner ] = ifolder.Properties.GetSingleProperty( "OrphanedOwner" );

					WriteRow( file, columns, cells );
				}
			}
			catch ( Exception ex )
			{
				hadException = true;
				file.WriteLine();
				file.WriteLine();
				file.WriteLine( ex );
				file.WriteLine( ex.StackTrace );
			}
			finally
			{
				file.Close();

				if ( hadException == false )
				{
					// If this is an iFolder create the file node for the report file.
					if ( config.IsiFolder )
					{
						CreateFileNode( fileName );
					}

					// Set the time of the last successful report.
					SetLastReportTime( currentReportTime );
				}
			}
		}

		/// <summary>
		/// Starts the report generation thread.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="e">An ElapsedEventArgs object that contains event data.</param>
		public void GenerateReportThread( object source, ElapsedEventArgs e )
		{
			// Make sure that it is time to run the report. If not, then reschedule the timer.
			if ( ( nextReportTime != DateTime.MinValue ) && ( nextReportTime <= e.SignalTime ) )
			{
				// Set the current report time.
				currentReportTime = nextReportTime;

				// Create the report generation thread.
				Thread thread = new Thread( new ThreadStart( GenerateReport ) );
				thread.IsBackground = true;
				thread.Priority = ThreadPriority.Lowest;
				thread.Start();

				// Reschedule for the next report time.
				ScheduleReportGeneration();
			}
			else
			{
				// Don't reschedule timer if no report time is set.
				if ( nextReportTime != DateTime.MinValue )
				{
					// Reschedule for the next report time.
					ScheduleReportGeneration();
				}
			}
		}

		/// <summary>
		/// Gets the ReportConfig object for the Report collection.
		/// </summary>
		/// <returns>A ReportConfig object if successful. Otherwise null is returned.</returns>
		private ReportConfig GetReportConfiguration()
		{
			ReportConfig config = null;

			// Get the report configuration node.
			Node node = reportCollection.GetSingleNodeByType( "Settings" );
			if ( node != null )
			{				// Get the settings property for the reporting.
				Property p = node.Properties.GetSingleProperty( "iFolderSystemReportConfiguration" );
				if ( p != null )
				{
					config = new ReportConfig( p.Value as String );
				}
				else
				{
					log.Error( "No configuration settings found on configuration node: {0}", reportConfigNodeID );
				}
			}
			else
			{
				log.Error( "Cannot locate report configuration node: {0}", reportConfigNodeID );
			}

			return config;
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key">Key to the localized string.</param>
		/// <returns>Localized string.</returns>
		private static string GetString( string key )
		{
			return resourceManager.GetString( key );
		}

		/// <summary>
		/// Schedules a time when report generation should take place.
		/// </summary>
		private void ScheduleReportGeneration()
		{
			// Cancel any outstanding request.
			CancelReportGeneration();

			// Get the current report configuration object.
			ReportConfig config = GetReportConfiguration();
			if ( config != null )
			{
				// Don't schedule if reporting has not been enabled.
				if ( config.Enabled )
				{
					DateTime current = DateTime.Now;
					DateTime scheduled = DateTime.Parse( config.TimeOfDay.ToString(), Thread.CurrentThread.CurrentUICulture );

					switch ( config.Frequency )
					{
						case ReportConfig.ReportFrequency.Daily:
						{
							// If the scheduled time has already passed for today, schedule the timer for tomorrow.
							if ( scheduled <= current )
							{
								scheduled = scheduled.AddDays( 1 );
							}

							break;
						}

						case ReportConfig.ReportFrequency.Weekly:
						{
							// Subtract the current day of the week to get back to the beginning of the week.
							scheduled = scheduled.AddDays( - ( int )scheduled.DayOfWeek );

							// Add the scheduled day of the week back in.
							scheduled = scheduled.AddDays( ( int )config.Weekday );

							// If the current day of the week is greater than the schedule day, schedule for
							// next week.
							if ( scheduled <= current )
							{
								scheduled = scheduled.AddDays( 7 );
							}

							break;
						}

						case ReportConfig.ReportFrequency.Monthly:
						{
							// Construct the time when the report should be generated.
							scheduled = new DateTime( 
								scheduled.Year, 
								scheduled.Month, 
								config.DayOfMonth, 
								scheduled.Hour, 
								scheduled.Minute, 
								0 );

							// If the scheduled time has passed the current time, schedule for
							// next month.
							if ( scheduled <= current )
							{
								scheduled = scheduled.AddMonths( 1 );
							}

							break;
						}
					}

					log.Debug( "Report scheduled {0} to be generated on {1}", 
						config.Frequency.ToString(),
						scheduled.ToString( "F", Thread.CurrentThread.CurrentUICulture ) );

					// Create a time interval.
					TimeSpan dueTime = scheduled - current;

					// The longest time that will be scheduled is one month. This time exceeds the
					// maximum interval value allowed by the timer. If we encounter this situation,
					// schedule the timer for the maximum allowed value and let it fire earlier than
					// the scheduled one month time. Check to see if it is time to run the report and
					// if the current time is less than the scheduled time, then reschedule the report
					// to run at what now should be a much short interval.

					// Check if the interval is greater than the maximum value that can
					// be set for the timer.
					if ( dueTime.TotalMilliseconds > Int32.MaxValue )
					{
						dueTime = TimeSpan.FromMilliseconds( Int32.MaxValue );
						log.Debug( "Timer interval greater than maximum. Setting to maximum." );
					}
					else if ( dueTime.TotalMilliseconds < 0 )
					{
						// Run now.
						dueTime = new TimeSpan( 0 );
					}

					// Remember the scheduled report time.
					nextReportTime = scheduled;

					// Schedule the report to be generated at the specified time.
					timer.Interval = dueTime.TotalMilliseconds;
					timer.Start();
				}
				else
				{
					log.Debug( "Reporting has been disabled." );
				}
			}
		}

		/// <summary>
		/// Sets the time of the last successful report on the report collection.
		/// </summary>
		/// <param name="reportTime">Time that the report was successfully generated.</param>
		private void SetLastReportTime( DateTime reportTime )
		{
			reportCollection.Properties.ModifyProperty( "LastReportTime", reportTime );
			reportCollection.Commit();
		}

		/// <summary>
		/// Event handler for when a node is created in the report collection.
		/// </summary>
		/// <param name="args">Object that describes the event.</param>
		private void SettingsWatcher_NodeCreated( NodeEventArgs args )
		{
			// Check if this node is a settings type node.
			Node settings = reportCollection.GetNodeByID( args.ID );
			if ( ( settings != null ) && settings.IsType( "Settings" ) )
			{
				log.Debug( "Report configuration node has been created." );
				ConfigurationWatcherStart( args.ID );
			}
		}

		/// <summary>
		/// Event handler for when a node is deleted in the report collection.
		/// </summary>
		/// <param name="args">Object that describes the event.</param>
		private void SettingsWatcher_NodeDeleted( NodeEventArgs args )
		{
			// See if it is the settings node that is being deleted.
			if ( ( configurationWatcher != null ) && ( reportConfigNodeID == args.ID ) )
			{
				log.Debug( "Report configuration node has been deleted." );
				ConfigurationWatcherStop();
			}
		}

		/// <summary>
		/// Starts the settings event watcher.
		/// </summary>
		private void SettingsWatcherStart()
		{
			SettingsWatcherStop();

			if (reportCollectionID == null)
			{
			        reportCollectionID = store.GetSingleCollectionByType( "Reports" ).ID;
			}

			settingsWatcher = new EventSubscriber( reportCollectionID );
			settingsWatcher.NodeCreated += new NodeEventHandler( SettingsWatcher_NodeCreated );
			settingsWatcher.NodeDeleted += new NodeEventHandler( SettingsWatcher_NodeDeleted );
			settingsWatcher.Enabled = true;
		}

		/// <summary>
		///  Stops the settings event watcher.
		/// </summary>
		private void SettingsWatcherStop()
		{
			if ( settingsWatcher != null )
			{
				settingsWatcher.Enabled = false;
				settingsWatcher.NodeCreated -= new NodeEventHandler( SettingsWatcher_NodeCreated );
				settingsWatcher.NodeDeleted -= new NodeEventHandler( SettingsWatcher_NodeDeleted );
				settingsWatcher.Dispose();
				settingsWatcher = null;
			}
		}

		/// <summary>
		/// Writes the column header titles to the report file.
		/// </summary>
		/// <param name="writer">File stream to write the titles to.</param>
		/// <param name="columns">Object that contains the header strings.</param>
		private void WriteHeaderRow( StreamWriter writer, ReportColumn[] columns )
		{
			StringBuilder sb = new StringBuilder( 1024 );
			for ( int i = 0; i < columns.Length; ++i )
			{
				sb.AppendFormat( "{0}", columns[ i ].Header );
				if ( i < ( columns.Length - 1 ) )
				{
					sb.Append( "," );
				}
			}

			writer.WriteLine( sb.ToString() );
		}

		/// <summary>
		/// Writes the column data to the report file.
		/// </summary>
		/// <param name="writer">File stream to write the data to.</param>
		/// <param name="columns">Object that contains the data format strings.</param>
		/// <param name="cells">Object that contains the data.</param>
		private void WriteRow( StreamWriter writer, ReportColumn[] columns, object[] cells )
		{
			StringBuilder sb = new StringBuilder( 1024 );
			for ( int i = 0; i < columns.Length; ++i )
			{
				sb.AppendFormat( columns[ i ].Format, cells[ i ] );
				if ( i < ( columns.Length - 1 ) )
				{
					sb.Append( "," );
				}
			}

			writer.WriteLine( sb.ToString() );
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Creates the system report collection.
		/// </summary>
		/// <param name="store">Store object</param>
		/// <param name="domain">The domain to which this collection will belong.</param>
		/// <returns>A collection object that represents the report collection.</returns>
		internal static Collection CreateReportCollection( Store store, Domain domain )
		{
			// Check to see if the report has already been created.
			Collection report = store.GetSingleCollectionByType( "Reports" );

			if ( report == null )
			{
				// Create the new report.
				report = new Collection( store, reportCollectionName, domain.ID );

				// Set the type as an iFolder so it can be accessed and shared by iFolder.
				report.SetType( report, "iFolder" );
				report.SetType( report, "Reports" );

				// Add the admin user for the domain as the owner.
				Member member = new Member( domain.Owner.Name, domain.Owner.UserID, Access.Rights.Admin );
				member.IsOwner = true;

				// Add the directory node 
				string dirPath = Path.Combine( report.UnmanagedPath, reportCollectionName );
				DirNode dirNode = new DirNode( report, dirPath );

				// Create the unmanaged directory for the reports.
				if ( !Directory.Exists( dirPath ) )
				{
					Directory.CreateDirectory( dirPath );
				}

				// Commit the changes.
				report.Commit( new Node[] { report, member, dirNode } );
			}

			reportCollectionID = report.ID;			 

			return report;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Starts monitoring the reporting configuration objects.
		/// </summary>
		public void StartMonitorReportConfiguration()
		{
			// Setup a watcher for any settings object that is created or deleted
			// from the report collection.
			SettingsWatcherStart();

			// Look for the report settings node.
			Node settings = reportCollection.GetSingleNodeByType( "Settings" );
			if ( settings != null )
			{
				// Setup an event watcher on the report settings node.
				ConfigurationWatcherStart( settings.ID );
			}
		}

		/// <summary>
		/// Stops monitor the report configuration and cancels any outstanding report
		/// generation timers.
		/// </summary>
		public void StopMonitorReportConfiguration()
		{
			ConfigurationWatcherStop();
			SettingsWatcherStop();
		}

		#endregion

		#region Report Configuration Object

		/// <summary>
		/// Class used to implement the report configuration.
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
			private bool enabled;

			/// <summary>
			/// How often to generate the report.
			/// </summary>
			private ReportFrequency frequency;

			/// <summary>
			/// The day of the week to generate the report if frequency
			/// is weekly.
			/// </summary>
			private DayOfWeek weekday;

			/// <summary>
			/// The day of the month to generate the report if frequency
			/// is monthly.
			/// </summary>
			private int dayOfMonth;

			/// <summary>
			/// The time of day to generate the report. This value represents
			/// the fraction of the day elapsed since midnight.
			/// </summary>
			private TimeSpan timeOfDay;

			/// <summary>
			/// Indicates if the report output path is an iFolder.
			/// </summary>
			private bool isiFolder;

			/// <summary>
			/// The output report format to use.
			/// </summary>
			private ReportFormat format;

			#endregion

			#region Properties

			/// <summary>
			/// Gets the day of the month to generate report if frequency is Monthly.
			/// </summary>
			public int DayOfMonth
			{
				get { return dayOfMonth; }
			}

			/// <summary>
			/// Gets whether reporting is enabled.
			/// </summary>
			public bool Enabled
			{
				get { return enabled; }
			}

			/// <summary>
			/// Gets the output report format to us.
			/// </summary>
			public ReportFormat Format
			{
				get { return format; }
			}

			/// <summary>
			/// Gets the frequency of report generation.
			/// </summary>
			public ReportFrequency Frequency
			{
				get { return frequency; }
			}

			/// <summary>
			/// Gets whether the ReportPath is an iFolder.
			/// </summary>
			public bool IsiFolder
			{
				get { return isiFolder; }
			}

			/// <summary>
			/// Gets the time of day to generate the report. This  value represents the 
			/// fraction of the day elapsed since midnight.
			/// </summary>
			public TimeSpan TimeOfDay
			{
				get { return timeOfDay; }
			}

			/// <summary>
			/// Gets the day of the week to generate report if frequency is Weekly.
			/// </summary>
			public DayOfWeek Weekday
			{
				get { return weekday; }
			}

			#endregion

			#region Constructor

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
				enabled = Boolean.Parse( doc.DocumentElement.GetAttribute( "enabled" ) );
				if ( Enabled )
				{
					// Get the frequency of the report generation.
					XmlElement element = doc.DocumentElement.SelectSingleNode( "generate" ) as XmlElement;
					if ( element != null )
					{
						string s = element.GetAttribute( "frequency" );
						if ( s != String.Empty )
						{
							frequency = ( ReportFrequency )Enum.Parse( 
								typeof( ReportFrequency ), 
								s, 
								true );
						}
						else
						{
							log.Error( "Invalid report settings format. No frequency attribute was specified." );
						}
					}
					else
					{
						log.Error( "Invalid report settings format. No generate element was specified." );
					}


					// All frequencies will have a time specified.
					XmlElement child = element.SelectSingleNode( "time" ) as XmlElement;
					if ( child != null )
					{
						DateTime dt = DateTime.Parse( child.InnerText );
						timeOfDay = new TimeSpan( dt.Hour, dt.Minute, 0 );
					}
					else
					{
						log.Error( "Invalid report settings format. No time element was specified." );
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
								weekday = ( DayOfWeek )Enum.Parse( typeof( DayOfWeek ), child.InnerText, true );
							}
							else
							{
								log.Error( "Invalid report settings format. No day of week element was specified." );
							}
							break;

						case ReportFrequency.Monthly:
							// Get the day of the month.
							child = element.SelectSingleNode( "dayofmonth" ) as XmlElement;
							if ( child != null )
							{
								dayOfMonth = Convert.ToInt32( child.InnerText );
							}
							else
							{
								log.Error( "Invalid report settings format. No day of month element was specified." );
							}
							break;
					}

					// Get the report location information.
					element = doc.DocumentElement.SelectSingleNode( "location" ) as XmlElement;
					if ( element != null )
					{
						isiFolder = ( element.InnerText == "ifolder" ) ? true : false;
					}
					else
					{
						log.Error( "Invalid report settings format. No location element was specified." );
					}

					// Get the report format.
					element = doc.DocumentElement.SelectSingleNode( "format" ) as XmlElement;
					if ( element != null )
					{
						format = ( ReportFormat )Enum.Parse( typeof( ReportFormat ), element.InnerText, true );
					}
					else
					{
						log.Error( "Invalid report settings format. No format element was specified." );
					}
				}
			}
			
			#endregion
		}

		#endregion

		#region ReportColumn Object

		/// <summary>
		/// Report Column Class
		/// </summary>
		public class ReportColumn
		{
			#region Class Members

			/// <summary>
			/// Column header string.
			/// </summary>
			private string header;

			/// <summary>
			/// Data format string.
			/// </summary>
			private string format;

			#endregion

			#region Properties

			/// <summary>
			/// Gets the column header string.
			/// </summary>
			public string Header
			{
				get { return header; }
			}

			/// <summary>
			/// Gets the data format string.
			/// </summary>
			public string Format
			{
				get { return format; }
			}

			#endregion

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="header">Column header string.</param>
			public ReportColumn( string header ) : 
				this( header, "{0}" )
			{
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="header">Column header string.</param>
			/// <param name="format">Data format string.</param>
			public ReportColumn( string header, string format )
			{
				this.header = "\"" + header + "\"";
				this.format = "\"" + format + "\"";
			}

			#endregion
		}

		#endregion
	}

	/// <summary>
	/// Class that runs as a thread service and generates iFolder reports.
	/// </summary>
	public class ReportService : IThreadService
	{
		#region Class Members

		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		/// <summary>
		/// Report object.
		/// </summary>
		private Report report = null;

		#endregion

		#region Properties
		#endregion

		#region Private Methods
		#endregion

		#region IThreadService Members

		/// <summary>
		/// Starts the thread service.
		/// </summary>
		public void Start()
		{
			report = new Report();
			report.StartMonitorReportConfiguration();
			log.Debug( "Started reporting service." );
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			if ( report != null )
			{
				report.StopMonitorReportConfiguration();
				report = null;
				log.Debug( "Stopped reporting service." );
			}
		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public int Custom(int message, string data)
		{
			return 0;
		}

		#endregion
	}
}
