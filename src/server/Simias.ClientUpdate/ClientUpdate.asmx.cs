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
*                 $Modified by: Boyd Timothy <btimothy@novell.com> (added methods for Unix)
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
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Authentication;
using Simias.DomainServices;

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

		private  static string[,] VersionCompatibilityTable = new string[,]{
								{"1.0.1", "3.0", "3.4"},
								{"1.1.0", "3.7", "3.9"}
								};
		/// <summary>
		/// Xml tags used to defined the version information in the version.config file.
		/// Format of the version.config file for Windows is as follows:
		///		<versioninfo>
		///			<distribution match="windows">
		///				<version>"3.7.1.*"</version>
		///				<filename>"ifolder3-windows-*.exe"</filename>
		///			</distribution>
		///		</versioninfo>
		///
		/// Format of the unix-version.config file is as follows:
		/// 		<versioninfo>
		/// 			<distribution match="Novell Linux Desktop 9 (i586)">
		/// 				<version>3.6.1234.1</version>
		/// 				<download-directory>nld-9-i586</download-directory>
		/// 			</distribution>
		/// 		</versioninfo>
 		/// Format of the mac-version.config file is as follows:
                ///             <versioninfo>
                ///                     <version>3.7.1.6</version>
                ///                     <filename>ifolder3-mac.dmg</filename>
                ///             </versioninfo>
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
		internal static string MacUpdateDir = Path.Combine( UpdateDir, "mac" );
		private static string VersionFile = "version.config";
		private static string UnixVersionFile = "unix-version.config";
		private static string MacVersionFile = "mac-version.config";

		#endregion
		public ClientUpdate()
		{
		}
		#region Private Methods

		/// <summary>
                /// Gets the Mac's iFolder application file name from the specified directory.
                /// </summary>
                /// <returns>The full path to the iFolder application if it exists. Otherwise a null is returned.</returns>
                private string GetiFolderMacApplicationFile()                 
		{                         
			string applicationFile = null; 
                        // Build a path to the windows update directory.
                        string versionFile = Path.Combine( SimiasSetup.webdir, Path.Combine( MacUpdateDir, MacVersionFile ) );
 
                        if ( File.Exists( versionFile ) )                         {
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
                                        log.Error( "No filename tag exists in mac-version.config file." );
                                }
                        }

                        return applicationFile;
                }

		/// <summary>
                /// Gets the version of the windows iFolder application.
                /// </summary>
                /// <returns>A Version object containing the version of the iFolder application.</returns>
                private Version GetiFolderMacApplicationVersion()
                {
                        Version applicationVersion = null;

                        // Build a path to the windows update directory.
                        string versionFile = Path.Combine( SimiasSetup.webdir, Path.Combine( MacUpdateDir, MacVersionFile ) );
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
                                        log.Error( "No version tag exists in mac-version.config file." );
                                }
                        }

                        return applicationVersion;
                }

		/// <summary>
		/// Gets the iFolder application file name from the specified directory.
		/// </summary>
		/// <returns>The full path to the iFolder application if it exists. Otherwise a null is returned.</returns>
		private string GetiFolderWindowsApplicationFile(string distribution)
		{
			string applicationFile = null;

			// Build a path to the windows update directory.
			string versionFile = Path.Combine( SimiasSetup.webdir, Path.Combine( WindowsUpdateDir, VersionFile ) );
			if ( File.Exists( versionFile ) )
			{
				XmlDocument document = new XmlDocument();
				document.Load( versionFile );

                	        XmlNode distributionNode = GetDistributionNode( document, distribution );
                        	if ( distributionNode == null )
                                	return null;

	                        XmlNode applicationNode = distributionNode.SelectSingleNode( FileNameTag + "/text()" );
        	                if ( applicationNode == null )
                	                return null;

                        	applicationFile = applicationNode.Value;
			}

			return applicationFile;
		}

		/// <summary>
		/// Gets the version of the windows iFolder application.
		/// </summary>
		/// <returns>A Version object containing the version of the iFolder application.</returns>
		private Version GetiFolderWindowsApplicationVersion(string distribution)
		{
			Version applicationVersion = null;

			// Build a path to the windows update directory.
			string versionFile = Path.Combine( SimiasSetup.webdir, Path.Combine( WindowsUpdateDir, VersionFile ) );
			if ( File.Exists( versionFile ) )
			{
				XmlDocument document = new XmlDocument();
				document.Load( versionFile );

                	        XmlNode distributionNode = GetDistributionNode( document, distribution );
                        	if ( distributionNode == null )
                                	return null;
	
                	        XmlNode versionNode = distributionNode.SelectSingleNode( VersionTag + "/text()" );
        	                if ( versionNode == null )
                        	        return null;

	                       applicationVersion = new Version( versionNode.Value );

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

        /// <summary>
        /// get which distribution version
        /// </summary>
        /// <param name="distribution">string containing the distribution</param>
        /// <returns>Version object</returns>
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

        /// <summary>
        /// get all the filelist
        /// </summary>
        /// <param name="distribution">string representing distribution</param>
        /// <returns>the string array containing filelist</returns>
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
					// Get the file list for the specified version.
					string platform = Session[ PlatformType ] as string;
					if ( platform.Equals("Darwin") )
					{
						log.Debug("Inside GetUpdateFiles for Darwin");
						string fileName = GetiFolderMacApplicationFile();
						if( fileName != null)
						{
							fileList = new string[] { fileName };
						}
					}
					else if ( platform.StartsWith("windows") || platform == MyPlatformID.Windows.ToString())
					{
						
						//Get the windows update application.
						string fileName = GetiFolderWindowsApplicationFile(platform);
						if ( fileName != null )
						{
							//Build a relative path to the windows update directory.
							fileList = new string[] { fileName };
						}
					}
					else
					{
						fileList = GetDistributionFileList( Session[ PlatformType ] as string );
					}

			}
			catch ( Exception ex )
			{
				log.Error( "Error: {0}, getting update files.", ex.Message );
			}
			log.Debug("out of getupdate files");

			return fileList;
		}

		/// <summary>
		/// Gets the update files associated with the specified version.
		/// </summary>
		/// <returns>An array of files that constitute the update.</returns>
		[WebMethod(
			 Description="Gets the update files associated with the specified version.",
			 EnableSession=true)]
		[SoapDocumentMethod]
		public string[] GetUpdateFilesSoapDocMethod()
		{
			string[] fileList = null;
			try
			{
					// Get the file list for the specified version.
					string platform = Session[ PlatformType ] as string;
					if ( platform.Equals("Darwin") )
					{
						log.Debug("Inside GetUpdateFiles for Darwin");
						string fileName = GetiFolderMacApplicationFile();
						if( fileName != null)
						{
							fileList = new string[] { fileName };
						}
					}
					else if ( platform.StartsWith("windows") || platform == MyPlatformID.Windows.ToString())
					{
						
						//Get the windows update application.
						string fileName = GetiFolderWindowsApplicationFile(platform);
						if ( fileName != null )
						{
							//Build a relative path to the windows update directory.
							fileList = new string[] { fileName };
						}
					}
					else
					{
						fileList = GetDistributionFileList( Session[ PlatformType ] as string );
					}

			}
			catch ( Exception ex )
			{
				log.Error( "Error: {0}, getting update files.", ex.Message );
			}
			log.Debug("out of getupdate files");

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
				if ( platform.StartsWith("windows") || platform == MyPlatformID.Windows.ToString())
				{
					// See if there is an iFolder application update available.
					applicationVersion = GetiFolderWindowsApplicationVersion(platform);
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
					{
						updateVersion = max.ToString();
						Session[ VersionString ] = updateVersion;
					}
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
		[SoapDocumentMethod]
		public string IsUpdateAvailableActualSoapDocMethod( string platform, string currentVersion )
		{
			log.Debug("In IsUpdateAvailableActual");
			string updateVersion = null;
			try
			{
				Session[ PlatformType ] = platform;
				Version applicationVersion = null;
				Version min= null, max = null;

				// The update is platform specific.
				if ( platform.StartsWith("windows") || platform == MyPlatformID.Windows.ToString())
				{
					// See if there is an iFolder application update available.
					applicationVersion = GetiFolderWindowsApplicationVersion(platform);
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
					{
						updateVersion = max.ToString();
						Session[ VersionString ] = updateVersion;
					}
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
                        log.Debug("Adding to blocked list");
			if( DomainAgent.blockedIPs.ContainsKey(HttpContext.Current.Request.UserHostAddress) == false)
			{
				DomainAgent.blockedIPs.Add(HttpContext.Current.Request.UserHostAddress, null);
			}
		//	DomainAgent.blockedIPs.Add(HttpContext.Current.Request.UserHostAddress);
                        log.Debug("Blocked the IP: {0}", HttpContext.Current.Request.UserHostAddress);
                        if( DomainAgent.blockedIPs != null)
                                log.Debug("Added IP to the blocked list");
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
					applicationVersion = GetiFolderWindowsApplicationVersion(platform);
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
					{
						updateVersion = max.ToString();
						Session[ VersionString ] = updateVersion;
					}
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
		[SoapDocumentMethod]
		public string IsUpdateAvailableSoapDocMethod( string platform, string currentVersion )
		{
			log.Debug("IsUpdateAvailable(\"{0}\", \"{1}\")", platform, currentVersion);
                        log.Debug("Adding to blocked list");
			if( DomainAgent.blockedIPs.ContainsKey(HttpContext.Current.Request.UserHostAddress) == false)
			{
				DomainAgent.blockedIPs.Add(HttpContext.Current.Request.UserHostAddress, null);
			}
		//	DomainAgent.blockedIPs.Add(HttpContext.Current.Request.UserHostAddress);
                        log.Debug("Blocked the IP: {0}", HttpContext.Current.Request.UserHostAddress);
                        if( DomainAgent.blockedIPs != null)
                                log.Debug("Added IP to the blocked list");
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
					applicationVersion = GetiFolderWindowsApplicationVersion(platform);
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
					{
						updateVersion = max.ToString();
						Session[ VersionString ] = updateVersion;
					}
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
				if ( platform.StartsWith("windows") || platform == MyPlatformID.Windows.ToString())
				{
					// See if there is an iFolder application update available.
					applicationVersion = GetiFolderWindowsApplicationVersion(platform);
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
		[SoapDocumentMethod]
		public bool IsServerOlderSoapDocMethod( string platform, string currentVersion )
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
				if ( platform.StartsWith("windows") || platform == MyPlatformID.Windows.ToString())
				{
					// See if there is an iFolder application update available.
					applicationVersion = GetiFolderWindowsApplicationVersion(platform);
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

		/// <summary>
		/// Checks to check for client updates and compatibility with the server.
		/// </summary>
		/// <param name="platform">The operating system platform the client is running on.</param>
		/// <param name="currentVersion">The version of the iFolder application that the
		/// client is currently running.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		[WebMethod(
			 Description="Check for Client Updates and compatibility with server",
			 EnableSession=true)]
		[SoapRpcMethod]
		public StatusCodes CheckForUpdate( string platform, string currentVersion, out string serverVersion )
		{
			StatusCodes stat = (StatusCodes)StatusCodes.Unknown;
			serverVersion = null;
			try
			{
				Session[ PlatformType ] = platform;
				Version applicationVersion = null;
				Version PresServerVersion = null;
				Version min= null, max = null;
				// The update is platform specific.

				if (platform.Equals("Darwin"))
				{
					log.Debug("Checking For Darwin files");
					PresServerVersion =  GetiFolderMacApplicationVersion();
				}
				else if ( platform.StartsWith("windows") || platform == MyPlatformID.Windows.ToString())
				{
					// See if there is an iFolder application update available.
					PresServerVersion = GetiFolderWindowsApplicationVersion(platform);
				}
				else
				{
					// See if there is an iFolder application update availble
					PresServerVersion = GetDistributionVersion(platform);
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
				Version CurrentVersion = new Version(currentVersion);
				serverVersion = PresServerVersion.ToString();
				log.Debug("The present server version is: {0}", serverVersion.ToString());
                log.Debug("The present client version is: {0}", CurrentVersion.ToString());
				log.Debug("The present client version is: {0}", CurrentVersion.ToString());


				if( applicationVersion != null)
				{
					if( (min.Major < CurrentVersion.Major) || (min.Major == CurrentVersion.Major && min.Minor <= CurrentVersion.Minor))
					{
						if( max.Major == CurrentVersion.Major && max.Minor == CurrentVersion.Minor)
						{
							//check for thrid version number - API version number .
							if (PresServerVersion.Build > CurrentVersion.Build || ((PresServerVersion.Build == CurrentVersion.Build) && (PresServerVersion.Revision > CurrentVersion.Revision)))
								stat=(StatusCodes)StatusCodes.OlderVersion;
							else
								stat = (StatusCodes)StatusCodes.Success;
						}
						else if(( max.Major > CurrentVersion.Major ) || ( max.Major == CurrentVersion.Major && max.Minor > CurrentVersion.Minor))
						{
							// Client Update available but not needed.....
							stat =(StatusCodes)StatusCodes.OlderVersion;
						}
						else
						{
							// Client is of later version.  server has no clue.. Just send the server version. let the client decide
							stat = (StatusCodes)StatusCodes.ServerOld;
						}
					}
					else
					{
						// min.Major > CurrentVersion.Major.....
						// Client update is needed.....
						stat = (StatusCodes)StatusCodes.UpgradeNeeded;
					}
				}	
			/*
				if ( applicationVersion != null )
				{
					// For Client Upgrade needed min > current version
					if( (min.Major > (new Version(currentVersion)).Major) ||( (min.Major == (new Version(currentVersion)).Major) && min.Minor > (new Version(currentVersion)).Minor))
					{
						updateVersion = max.ToString();
						Session[ VersionString ] = updateVersion;
					}
				}
			*/
			}
			catch ( Exception ex )
			{
				log.Error( "Error: {0}, checking for application update.", ex.Message );
				return (StatusCodes)StatusCodes.Unknown;
			}

			return (StatusCodes)stat;
		}


		/// <summary>
		/// Checks to check for client updates and compatibility with the server.
		/// </summary>
		/// <param name="platform">The operating system platform the client is running on.</param>
		/// <param name="currentVersion">The version of the iFolder application that the
		/// client is currently running.</param>
		/// <returns>The version of the update if available. Otherwise null is returned.</returns>
		[WebMethod(
			 Description="Check for Client Updates and compatibility with server",
			 EnableSession=true)]
		[SoapDocumentMethod]
		public StatusCodes CheckForUpdateSoapDocMethod( string platform, string currentVersion, out string serverVersion )
		{
			StatusCodes stat = (StatusCodes)StatusCodes.Unknown;
			serverVersion = null;
			try
			{
				Session[ PlatformType ] = platform;
				Version applicationVersion = null;
				Version PresServerVersion = null;
				Version min= null, max = null;
				// The update is platform specific.

				if (platform.Equals("Darwin"))
				{
					log.Debug("Checking For Darwin files");
					PresServerVersion =  GetiFolderMacApplicationVersion();
				}
				else if ( platform.StartsWith("windows") || platform == MyPlatformID.Windows.ToString())
				{
					// See if there is an iFolder application update available.
					PresServerVersion = GetiFolderWindowsApplicationVersion(platform);
				}
				else
				{
					// See if there is an iFolder application update availble
					PresServerVersion = GetDistributionVersion(platform);
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
				Version CurrentVersion = new Version(currentVersion);
				serverVersion = PresServerVersion.ToString();
				log.Debug("The present server version is: {0}", serverVersion.ToString());
                log.Debug("The present client version is: {0}", CurrentVersion.ToString());
				log.Debug("The present client version is: {0}", CurrentVersion.ToString());

				if( applicationVersion != null)
				{
					if( (min.Major < CurrentVersion.Major) || (min.Major == CurrentVersion.Major && min.Minor <= CurrentVersion.Minor))
					{
						if( max.Major == CurrentVersion.Major && max.Minor == CurrentVersion.Minor)
						{
							//check for thrid version number - API version number .
							if (PresServerVersion.Build > CurrentVersion.Build || ((PresServerVersion.Build == CurrentVersion.Build) && (PresServerVersion.Revision > CurrentVersion.Revision)))						
								stat=(StatusCodes)StatusCodes.OlderVersion;
							else
								stat = (StatusCodes)StatusCodes.Success;
						}
						else if(( max.Major > CurrentVersion.Major ) || ( max.Major == CurrentVersion.Major && max.Minor > CurrentVersion.Minor))
						{
							// Client Update available but not needed.....
							stat =(StatusCodes)StatusCodes.OlderVersion;
						}
						else
						{
							// Client is of later version.  server has no clue.. Just send the server version. let the client decide
							stat = (StatusCodes)StatusCodes.ServerOld;
						}
					}
					else
					{
						// min.Major > CurrentVersion.Major.....
						// Client update is needed.....
						stat = (StatusCodes)StatusCodes.UpgradeNeeded;
					}
				}	
				if((stat == (StatusCodes)StatusCodes.OlderVersion) && (platform.StartsWith("windows") || platform == MyPlatformID.Windows.ToString()) && currentVersion.Equals("3.7.2.0"))
                                {
                                        log.Debug("Received request from 3.7.2.0");
                                        Version tempVersion = new Version(3,8,3,0);
                                        serverVersion = tempVersion.ToString();
                                }
			/*
				if ( applicationVersion != null )
				{
					// For Client Upgrade needed min > current version
					if( (min.Major > (new Version(currentVersion)).Major) ||( (min.Major == (new Version(currentVersion)).Major) && min.Minor > (new Version(currentVersion)).Minor))
					{
						updateVersion = max.ToString();
						Session[ VersionString ] = updateVersion;
					}
				}
			*/
			}
			catch ( Exception ex )
			{
				log.Error( "Error: {0}, checking for application update.", ex.Message );
				return (StatusCodes)StatusCodes.Unknown;
			}

			return (StatusCodes)stat;
		}

        /// <summary>
        /// get distribution directory
        /// </summary>
        /// <param name="distribution">string representing distribution</param>
        /// <returns>the string object for distribution</returns>
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
