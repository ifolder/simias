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
*			Author: Calvin Gaisford <cgaisford@novell.com>
*	 			Bruce Getter <bgetter@novell.com>
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
using System.IO;
using System.Xml;

using Simias.Client;

namespace Simias
{
	/// <summary>
	/// Configuration class for simias components.
	/// </summary>
	public sealed class Configuration
	{
		#region Class Members
		public const string DefaultConfigFileName = "Simias.config";
		public const string RenamedConfigFileName = "Simiasconfig.removed";
		

		private const string SectionTag = "section";
		private const string SettingTag = "setting";
		private const string NameAttr = "name";
		private const string ValueAttr = "value";
		private const string DefaultSection = "SimiasDefault";
		public string ConfigPath;
		private static string defaultConfigPath;

		private XmlDocument configDoc;
		public static string DefaultPath
		{
			get
			{
				return defaultConfigPath;
			}
		}

		public static string DefaultFilePath
		{
			get
			{
				return defaultConfigPath;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="storePath">The directory path to the store.</param>
		/// <param name="isServer">True if running in a server configuration.</param>
		public Configuration( string storePath, bool isServer )
		{
			// The server's Simias.config file must always be in the data directory.
			defaultConfigPath = ConfigPath = Path.Combine( storePath, DefaultConfigFileName );
			
			if ( !isServer )
			{
				lock ( typeof ( Configuration ) )
				{
					// See if there is an overriding Simias.config file in the client's data
					// directory. If not, then get the global copy.
					if ( !File.Exists( ConfigPath ) || !IsValidConfigurationFile( ConfigPath ) )
					{
						ConfigPath = Path.Combine( SimiasSetup.simiasconfdir, DefaultConfigFileName );
					}
					else
					{
						Console.Error.WriteLine( "Global configuration file: {0} is being overriden by a local copy.", DefaultConfigFileName );
					}
				}
			}

			// Check to see if the file already exists.
			if ( !File.Exists( ConfigPath ) )
			{
				throw new SimiasException( String.Format( "Cannot locate configuration file: {0}", ConfigPath ) );
			}

			// Load the configuration document from the file.
			configDoc = new XmlDocument();
			configDoc.Load( ConfigPath );
		}

		#endregion

		#region Private Methods

        /// <summary>
        /// Gets the section details from XML
        /// </summary>
        /// <param name="section">Section name that has to get from XML</param>
        /// <returns>XMLElement of the section</returns>
		private XmlElement GetSection( string section )
		{
			string str = string.Format( "//section[@name='{0}']", section );
			return configDoc.DocumentElement.SelectSingleNode( str ) as XmlElement;
		}

        /// <summary>
        /// Get the key from a section
        /// </summary>
        /// <param name="section">Section from which key element needed</param>
        /// <param name="key">Key element of the key</param>
        /// <returns>Key element as XMLElement</returns>
		private XmlElement GetKey( string section, string key )
		{
			XmlElement keyElement = null;

			// Get the section that the key belongs to.
			XmlElement sectionElement = GetSection( section );
			if ( sectionElement != null )
			{
				string str = string.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key );
				keyElement = sectionElement.SelectSingleNode( str ) as XmlElement;
			}

			return keyElement;
		}

        /// <summary>
        /// Check for valid configuration file
        /// </summary>
        /// <param name="configFilePath">Path of the configuration file</param>
        /// <returns></returns>
		private bool IsValidConfigurationFile( string configFilePath )
		{
			bool isValid = false;
			try
			{
				// Load the configuration document from the file.
				configDoc = new XmlDocument();
				configDoc.Load( configFilePath );

				// Look for a known tag that is not in the old configuration file
				// and look for an old tag that is not in the new configuration file.
				isValid = ( ( GetSection( "Authentication" ) != null ) && ( GetSection( "ServiceManager" ) == null ) );
				configDoc = null;
			}
			catch
			{}

			return isValid;
		}

        /// <summary>
        /// Check whether key exists in section or not
        /// </summary>
        /// <param name="section">Secition to search for</param>
        /// <param name="key">Key to look in the section</param>
        /// <returns>True or false depending on key exists</returns>
		private bool KeyExists( string section, string key )
		{
			return ( GetKey( section, key ) != null ) ? true : false;
		}

        /// <summary>
        /// Check whether section exists or not
        /// </summary>
        /// <param name="section">Section to check for</param>
        /// <returns></returns>
		private bool SectionExists( string section )
		{
			return ( GetSection( section ) != null ) ? true : false;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns the XmlElement for the specified key.  
		/// Creates the key if does not exist.
		/// </summary>
		/// <param name="key">The key to return.</param>
		/// <returns>The key as an XmlElement.</returns>
		public XmlElement GetElement( string key )
		{
			return GetElement( DefaultSection, key );
		}

		/// <summary>
		/// Returns the XmlElement for the specified key.  
		/// </summary>
		/// <param name="section">The section where the key is stored.</param>
		/// <param name="key">The key to return.</param>
		/// <returns>The key as an XmlElement if successful. Otherwise a null is returned.</returns>
		public XmlElement GetElement( string section, string key )
		{
			XmlElement element = GetKey( section, key );
			return ( element != null ) ? element.Clone() as XmlElement : null;
		}

		/// <summary>
		/// Returns the value for the specified key.
		/// </summary>
		/// <param name="key">The key to get the value for.</param>
		/// <returns>The value as a string if successful. Otherwise a null is returned.</returns>
		public string Get( string key )
		{
			return Get( DefaultSection, key );
		}

		/// <summary>
		/// Returns the value for the specified key.
		/// </summary>
		/// <param name="section">The section where the key exists.</param>
		/// <param name="key">The key to get the value for.</param>
		/// <returns>The value as a string if successful. Otherwise a null is returned.</returns>
		public string Get( string section, string key )
		{
			XmlElement keyElement = GetKey( section, key );
			return ( keyElement != null ) ? keyElement.GetAttribute( ValueAttr ) : null;
		}

		/// <summary>
		/// Checks for existence of a specified key.
		/// </summary>
		/// <param name="key">The key to check for existence.</param>
		/// <returns>True if the key exists, otherwise false is returned.</returns>
		public bool Exists( string key )
		{
			return Exists( DefaultSection, key );
		}

		/// <summary>
		/// Checks for existence of a specified section and key.
		/// </summary>
		/// <param name="section">The section for the tuple</param>
		/// <param name="key">The key to set. If this parameter is null, then only the section
		/// is checked for existence.</param>
		/// <returns>True if the section and key exists, otherwise false is returned.</returns>
		public bool Exists( string section, string key )
		{
			return ( ( key != null ) && ( key != String.Empty ) ) ? KeyExists( section, key ) : SectionExists( section );
		}

		/// <summary>
		/// Returns the XML representation of the config file.
		/// </summary>
		/// <returns></returns>
		public string ToXml()
		{
			return configDoc.InnerXml;
		}

		#endregion
	}
}


