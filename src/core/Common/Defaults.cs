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
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
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


namespace Simias
{
	/// <summary>
	/// Defaults class for simias defaults.
	/// </summary>
	public sealed class Defaults
	{
		//public const string DefaultConfigFile = "@sysconfdir@/defaults.config";
		//public const string SimiasDataDir = "simiasdatadir";
		//public const string RunAsClient = "runasclient";

		/// <summary>
		/// Constructor.
		/// </summary>
		private Defaults()
		{
		}

        /// <summary>
        /// Get/Set simias data directory
        /// </summary>
		public static string SimiasDataDir
		{
			get
			{
				string simiasDataDir = null;

				// path to the defaults.config file
				string defaultsPath = Path.Combine(Simias.Client.SimiasSetup.simiasconfdir, "defaults.config");
				
//				try
//				{
					XmlDocument doc = new XmlDocument();
					doc.Load( defaultsPath );
				
					// Validate the XML
					XmlNode root = doc.FirstChild;
					XmlNode section = root.SelectSingleNode( "/configuration/section[@name='DefaultServerValues']" );
					if ( section != null )
					{
						XmlNode setting = root.SelectSingleNode( "/configuration/section/setting[@name='simiasdatadir']" );
						if ( setting != null )
						{
							foreach( XmlAttribute attr in setting.Attributes )
							{
								if ( attr.Name.ToLower() == "value" )
								{
									simiasDataDir = attr.InnerText;
									break;
								}
							}
						}
					}	
//				}
//				catch( Exception e )
//				{
//					Console.WriteLine( e.Message );
//				}
				
				return simiasDataDir;
			}
		}

        /// <summary>
        /// Get the status of RunAsClient property
        /// </summary>
		public static bool RunsAsClient
		{
			get
			{
				return false;
			}
		}
	}
}


