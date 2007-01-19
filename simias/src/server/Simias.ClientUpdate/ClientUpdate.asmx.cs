/***********************************************************************
 *  $RCSfile: ClientUpdate.asmx.cs,v $
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Authors:
 *  	Mike Lasky <mlasky@novell.com>
 *  	Boyd Timothy <btimothy@novell.com> (added methods for Unix)
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

using Simias;
using Simias.Client;

namespace Novell.iFolder.Enterprise.Web
{
	/// <summary>
	/// iFolder Enterprise System Web Service
	/// </summary>
	[WebService(
		Namespace="http://novell.com/ifolder/web/",
		Name="ClientUpdate",
		Description="Client Update Web Service")]
	public class ClientUpdate : WebService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( ClientUpdate ) );

		/// <summary>
		/// Session state variables.
		/// </summary>
		internal static string PlatformType = "PlatformType";
		private static string VersionString = "Version";

		private  string[,] VersionCompatibilityTable = new string[,]{
								{"1.0.1", "3.0", "3.4"},
								{"1.1.0", "3.5", "3.5"}
								};
		/// <summary>
		/// Xml tags used to defined the version information in the version.config file.
		/// Format of the version.config file for Windows is as follows:
		///		<versioninfo>
		///			<version>"1.0.1817.29628"</version>
		///			<filename>"ifolder3-3.0.20041222-1-setup.exe"</filename>
		///		</versioninfo>
		///
		/// Format of the unix-version.config file is as follows:
		/// 		<versioninfo>
		/// 			<distribution match="Novell Linux Desktop 9 (i586)">
		/// 				<version>3.6.1234.1</version>
		/// 				<download-directory>nld-9-i586</download-directory>
		/// 			</distribution>
		/// 		</versioninfo>
		/// </summary>
		private static string VersionTag = "version";
		private static string FileNameTag = "filename";
		
		private static string DistributionTag      = "distribution";
		private static string MatchAttribute       = "match";
		private static string DownloadDirectoryTag = "download-directory";

		private static string DefaultMatchValue    = "DEFAULT";

		/// <summary>
		/// iFolder Client update directory paths.
		/// </summary>
		internal static string UpdateDir = "update";
		internal static string WindowsUpdateDir = Path.Combine( UpdateDir, "windows" );
		internal static string UnixUpdateDir = Path.Combine( UpdateDir, "unix" );
		private static string VersionFile = "version.config";
		private static string UnixVersionFile = "unix-version.config";

		#endregion
		public ClientUpdate()
		{
		}
		#region Private Methods
		/// <summary>
		/// Gets the iFolder application file name from the specified directory.
		/// </summary>
		/// <returns>The full path to the iFolder application if it exists. Otherwise a null is returned.</returns>
		private string GetiFolderWindowsApplicationFile()
		{
			string applicationFile = null;

			// Build a path to the windows update directory.
			string versionFile = Path.Combine( SimiasSetup.webdir, Path.Combine( WindowsUpdateDir, VersionFile ) );
			if ( File.Exists( versionFile ) )
			{
				XmlDocument document = new XmlDocument();
				document.Load( versionFile );
				XmlNode node = document.DocumentElement[ FileNameTag ];
				if ( node != null )
				{
					if ( node.InnerText.Length > 0 )
					{
						applicationFile = Path.GetFileName( node.InnerText );
					}
				}
				else
				{
					log.Error( "No filename tag exists in windows version.config file." );
				}
			}

			return applicationFile;
		}

		/// <summary>
		/// Gets the version of the windows iFolder application.
		/// </summary>
		/// <returns>A Version object containing the version of the iFolder application.</returns>
		private Version GetiFolderWindowsApplicationVersion()
		{
			Version applicationVersion = null;

			// Build a path to the windows update directory.
			string versionFile = Path.Combine( SimiasSetup.webdir, Path.Combine( WindowsUpdateDir, VersionFile ) );
			if ( File.Exists( versionFile ) )
			{
				XmlDocument document = new XmlDocument();
				document.Load( versionFile );
				XmlNode node = document.DocumentElement[ VersionTag ];
				if ( node != null )
				{
					if ( node.InnerText.Length > 0 )
					{
						applicationVersion = new Version( node.InnerText );
					}
				}
				else
				{
					log.Error( "No version tag exists in windows version.config file." );
				}
			}

			return applicationVersion;
		}

		///
		/// <summary>
		/// Returns an XmlDocument for the Unix Client Update Configuration file or
		/// null if one does not exist.
		/// </summary>
		/// <returns>An XmlDocument for the Unix Client Update Configuration file.</returns>
		private static XmlDocument GetUnixConfigDocument()
		{
			string file = Path.Combine( SimiasSetup.webdir, Path.Combine( UnixUpdateDir, UnixVersionFile ) );
			if ( File.Exists( file ) )
			{
				XmlDocument document = new XmlDocument();
				document.Load( file );
				return document;
			}

			return null;
		}

		///
		/// <summary>
		/// Look up the correct "distribution" node for the specified unix distribution.
		/// </summary>
		/// <returns>An XmlNode that represents the "distribution" node for the specified unix distribution or null if a matching distribution was not found.</returns>
		private static XmlNode GetDistributionNode( XmlDocument document, string distribution )
		{
			XmlNodeList nodeList = document.GetElementsByTagName( DistributionTag );
			if ( nodeList == null )
				return null;

			XmlNode distributionNode = null;

			foreach( XmlNode node in nodeList )
			{
				XmlAttribute attr = node.Attributes[ MatchAttribute ];
				string matchVal = attr.Value;

				if ( matchVal == DefaultMatchValue )
					distributionNode = node;
				else
				{
					if ( distribution.IndexOf( matchVal ) >= 0 )
					{
						distributionNode = node;
						break;
					}
				}
			}

			return distributionNode;
		}

		private static Version GetDistributionVersion( string distribution )
		{
			XmlDocument document = GetUnixConfigDocument();
			if ( document == null )
				return null;
			
			XmlNode distributionNode = GetDistributionNode( document, distribution );
			if ( distributionNode == null )
				return null;

			XmlNode versionNode = distributionNode.SelectSingleNode( VersionTag + "/text()" );
			if ( versionNode == null )
				return null;

			return new Version( versionNode.Value );
		}

		private static string[] GetDistributionFileList( string distribution )
		{
			string[] files = null;

			XmlDocument document = GetUnixConfigDocument();
			if ( document == null )
				return null;
			
			XmlNode distributionNode = GetDistributionNode( document, distribution );
			if ( distributionNode == null )
				return null;

			XmlNode downloadDirectoryNode = distributionNode.SelectSingleNode( DownloadDirectoryTag + "/text()" );
			if ( downloadDirectoryNode == null )
			{
				return null;
			}

			string downloadDirectory = Path.Combine( SimiasSetup.webdir, Path.Combine( UnixUpdateDir, downloadDirectoryNode.Value ) );
			if ( Directory.Exists( downloadDirectory ) )
			{
				files = Directory.GetFiles( downloadDirectory );
				if ( files != null && files.Length > 0 )
				{
					// Remove the complete path so it's just the filename and nothing more.
					for( int i = 0; i < files.Length; i++ )
					{
						int lastPathSeparatorPos = files[ i ].LastIndexOf( Path.DirectorySeparatorChar );
						if ( lastPathSeparatorPos >= 0 )
						{
							try
							{
								files[ i ] = files[ i ].Substring( lastPathSeparatorPos + 1 );
							}catch{} // Ignore ArgumentOutOfRangeException
						}
					}
				}
			}
			
			return files;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the update files associated with the specified version.
		/// </summary>
		/// <returns>An array of files that constitute the update.</returns>
		[WebMethod(
			 Description="Gets the update files associated with the specified version.",
			 EnableSession=true)]
		[SoapRpcMethod]
		public string[] GetUpdateFiles()
		{
			string[] fileList = null;
			try
			{
				// Make sure that there is a version to look for.
				string versionString = Session[ VersionString ] as string;
				if ( versionString != null )
				{
					// Get the file list for the specified version.
					string platform = Session[ PlatformType ] as string;
					if ( platform == MyPlatformID.Windows.ToString() )
					{
						// Get the windows update application.
						string fileName = GetiFolderWindowsApplicationFile();
						if ( fileName != null )
						{
							// Build a relative path to the windows update directory.
							fileList = new string[] { fileName };
						}
					}
					else
					{
						fileList = GetDistributionFileList( Session[ PlatformType ] as string );
					}
				}

			}
			catch ( Exception ex )
			{
				log.Error( "Error: {0}, getting update files.", ex.Message );
			}
			Console.WriteLine("out of getupdate files");

			return fileList;
		}

		/// <summary>
		/// Checks to see if a new version of the iFolder client application is available.
		/// </summary>
		/// <param name="platform">The operating system platform the client is running on.</param>
		/// <param name="currentVersion">The version of the iFolder application that the
		/// client is currently running.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		[WebMethod(
			 Description="Is Client Update Available",
			 EnableSession=true)]
		[SoapRpcMethod]
		public string IsUpdateAvailableActual( string platform, string currentVersion )
		{
			log.Debug("In IsUpdateAvailableActual");
			string updateVersion = null;
			try
			{
				Session[ PlatformType ] = platform;
				Version applicationVersion = null;
				Version min= null, max = null;

				// The update is platform specific.
				if ( platform == MyPlatformID.Windows.ToString() )
				{
					// See if there is an iFolder application update available.
					applicationVersion = GetiFolderWindowsApplicationVersion();
				}
				else
				{
					// See if there is an iFolder application update availble
					applicationVersion = GetDistributionVersion(platform);
				}
			
				// Get the application version from store.
				applicationVersion = new Version(Simias.Storage.Store.storeversion);
				for( int i=0; i<VersionCompatibilityTable.Length; i+=3)
				{
					if(VersionCompatibilityTable[i/3,0] == applicationVersion.ToString() )
					{
						min = new Version(VersionCompatibilityTable[i/3,1]);
						max = new Version(VersionCompatibilityTable[i/3,2]);
						break;
					}
				}

				if ( applicationVersion != null )
				{
					// For Client Upgrade needed min > current version
					if( (max.Major > (new Version(currentVersion)).Major) ||( (max.Major == (new Version(currentVersion)).Major) && max.Minor > (new Version(currentVersion)).Minor))
				//	if ( applicationVersion > new Version( currentVersion ) )
					{
						updateVersion = applicationVersion.ToString();
						Session[ VersionString ] = updateVersion;
					}
					/*
                                        else if(applicationVersion < new Version( currentVersion))
                                        {
                                                updateVersion = "Server is older";
                                                Session[ VersionString ] = updateVersion;
                                        }
					*/
				}
			}
			catch ( Exception ex )
			{
				log.Error( "Error: {0}, checking for application update.", ex.Message );
			}

			return updateVersion;
		}


		/// <summary>
		/// Checks to see if a new version of the iFolder client application is available.
		/// </summary>
		/// <param name="platform">The operating system platform the client is running on.</param>
		/// <param name="currentVersion">The version of the iFolder application that the
		/// client is currently running.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		[WebMethod(
			 Description="Is Client Update Available",
			 EnableSession=true)]
		[SoapRpcMethod]
		public string IsUpdateAvailable( string platform, string currentVersion )
		{
			log.Debug("IsUpdateAvailable(\"{0}\", \"{1}\")", platform, currentVersion);
			string updateVersion = null;
			try
			{
				Session[ PlatformType ] = platform;
				Version applicationVersion = null;
				Version min= null, max = null;

				
				// The update is platform specific.
				if ( platform == MyPlatformID.Windows.ToString() )
				{
					// See if there is an iFolder application update available.
					applicationVersion = GetiFolderWindowsApplicationVersion();
				}
				else
				{
					// See if there is an iFolder application update availble
					applicationVersion = GetDistributionVersion(platform);
				}
			
				// Get the application version from store.
				applicationVersion = new Version(Simias.Storage.Store.storeversion);
				for( int i=0; i<VersionCompatibilityTable.Length; i+=3)
				{
					if(VersionCompatibilityTable[i/3,0] == applicationVersion.ToString() )
					{
						min = new Version(VersionCompatibilityTable[i/3,1]);
						max = new Version(VersionCompatibilityTable[i/3,2]);
						break;
					}
				}

				if ( applicationVersion != null )
				{
					// For Client Upgrade needed min > current version
					if( (min.Major > (new Version(currentVersion)).Major) ||( (min.Major == (new Version(currentVersion)).Major) && min.Minor > (new Version(currentVersion)).Minor))
				//	if ( applicationVersion > new Version( currentVersion ) )
					{
						updateVersion = applicationVersion.ToString();
						Session[ VersionString ] = updateVersion;
					}
					/*
                                        else if(applicationVersion < new Version( currentVersion))
                                        {
                                                updateVersion = "Server is older";
                                                Session[ VersionString ] = updateVersion;
                                        }
					*/
				}
			}
			catch ( Exception ex )
			{
				log.Error( "Error: {0}, checking for application update.", ex.Message );
			}

			return updateVersion;
		}

		/// <summary>
		/// Checks to see if a the server is running an older version of simias 
		/// </summary>
		/// <param name="platform">The operating system platform the client is running on.</param>
		/// <param name="currentVersion">The version of the iFolder application that the
		/// client is currently running.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		[WebMethod(
			 Description="Is server older",
			 EnableSession=true)]
		[SoapRpcMethod]
		public bool IsServerOlder( string platform, string currentVersion )
		{
			log.Debug("IsUpdateAvailable(\"{0}\", \"{1}\")", platform, currentVersion);
			string updateVersion = null;
			bool serverOlder = false;
			Version min=null, max=null;
			try
			{
				Session[ PlatformType ] = platform;
				Version applicationVersion = null;

				
				// The update is platform specific.
				if ( platform == MyPlatformID.Windows.ToString() )
				{
					// See if there is an iFolder application update available.
					applicationVersion = GetiFolderWindowsApplicationVersion();
				}
				else
				{
					// See if there is an iFolder application update availble
					applicationVersion = GetDistributionVersion(platform);
				}
				// Get the application version from store.
				applicationVersion = new Version(Simias.Storage.Store.storeversion);
				for( int i=0; i<VersionCompatibilityTable.Length; i+=3)
				{
					if(VersionCompatibilityTable[i/3,0] == applicationVersion.ToString() )
					{
						min = new Version(VersionCompatibilityTable[i/3,1]);
						max = new Version(VersionCompatibilityTable[i/3,2]);
						break;
					}
				}
				if ( applicationVersion != null )
				{
					Version ClientVersion = new Version(currentVersion);
					if( max.Major == 0)
					{
						serverOlder = false;
					}
					else if(max.Major < ClientVersion.Major ||(max.Major== ClientVersion.Major && max.Minor < ClientVersion.Minor))
                                        {
                                                // updateVersion = "Server is older";
						serverOlder = true;
                                                Session[ VersionString ] = updateVersion;
                                        }
				}
			}
			catch ( Exception ex )
			{
				log.Error( "Error: {0}, checking for application update.", ex.Message );
			}

			return serverOlder;
		}



		public static string GetDistributionDownloadDirectory( string distribution )
		{
			XmlDocument document = GetUnixConfigDocument();
			if ( document == null )
				return null;
			
			XmlNode distributionNode = GetDistributionNode( document, distribution );
			if ( distributionNode == null )
				return null;
			
			XmlNode downloadDirectoryNode = distributionNode.SelectSingleNode( DownloadDirectoryTag + "/text()" );
			if ( downloadDirectoryNode == null )
			{
				log.Debug( "download-directory tag is null" );
				return null;
			}
			
			return downloadDirectoryNode.Value;
		}
		#endregion
	}
}
