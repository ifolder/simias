/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Reflection;
using System.Xml;

using log4net;
using log4net.Config;
using log4net.Appender;
using log4net.Repository;
//using log4net.spi;
using log4net.Layout;

using Simias.Client;

namespace Simias
{
	/// <summary>
	/// A light wrapper around the log4net LogManager class.
	/// </summary>
	public class SimiasLogManager
	{
		private static readonly string DefaultConfigFile = "Simias.log4net";

		private static bool configured = false;
        private static string configFile = null;

		/// <summary>
		/// Default Constructor
		/// </summary>
		private SimiasLogManager()
		{
		}

		/// <summary>
		/// Create or retrieve the logger for the type in the Simias domain.
		/// </summary>
		/// <param name="type">The fully qualified name of the type is the
		/// name of the logger.</param>
		/// <returns>A Simias log interface object.</returns>
		public static ISimiasLog GetLogger(Type type)
		{
			return new SimiasLog(LogManager.GetLogger(type));
		}

		/// <summary>
		/// Reset the log4net configuration.
		/// </summary>
		public static void ResetConfiguration()
		{
			LogManager.ResetConfiguration();

			log4net.Config.DOMConfigurator.ConfigureAndWatch(new FileInfo(configFile));
		}

		/// <summary>
		/// Configure the log manater to a specific Simias store.
		/// </summary>
		/// <param name="storePath">The full path to the store directory.</param>
		public static void Configure(String storePath)
		{
			lock(typeof(SimiasLogManager))
            {
                // only configure once
                if (!configured)
                {
					// config file
					configFile = Path.Combine(storePath, DefaultConfigFile);

					// bootstrap config
					if (!File.Exists(configFile))
					{
						// copy over bootstrap configuration file.
						File.Copy(Path.Combine(SimiasSetup.simiasconfdir, DefaultConfigFile), configFile);

						// update log file names to process name
						XmlDocument doc = new XmlDocument();
						doc.Load(configFile);

						// see if the log dir setting is to be overridden by an environment variable.
						string envLogDir = Environment.GetEnvironmentVariable("SimiasLogDir");
						if ( envLogDir != null )
						{
							// try and create the directory.
							if (!Directory.Exists(envLogDir))
							{
								Directory.CreateDirectory(envLogDir);
							}
						}

						XmlNodeList list = doc.GetElementsByTagName("file");
						
						for (int i=0; i < list.Count; i++)
						{   
							XmlNode attr = list[i].Attributes.GetNamedItem("value");

							if (envLogDir == null)
							{
								string logDir = Directory.GetParent(attr.Value).FullName;
								if (!Directory.Exists(logDir))
								{
									Directory.CreateDirectory(logDir);
								}

								attr.Value = attr.Value.Replace("\\", "/");
							}
							else
							{
								string fileName = Path.GetFileName(attr.Value);
								attr.Value = Path.Combine(envLogDir, fileName).Replace("\\", "/");
							}
						}

						list = doc.GetElementsByTagName("header");
						for (int i=0; i < list.Count; i++)
						{   
							XmlNode attr = list[i].Attributes.GetNamedItem("value");
							attr.Value = attr.Value.Replace("%n", Environment.NewLine);
						}

						XmlTextWriter writer = new XmlTextWriter(configFile, null);
						writer.Formatting = Formatting.Indented;
						doc.Save(writer);
						writer.Close();
					}

					ResetConfiguration();

					configured = true;
                }
            }
		}
	}
}
