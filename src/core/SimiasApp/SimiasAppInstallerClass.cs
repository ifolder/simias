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

#if WINDOWS

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Configuration.Install;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;

namespace SimiasApp
{
	/// <summary>
	/// Provides custom installation for iFolderApp.exe.
	/// </summary>
	// Set 'RunInstaller' attribute to true.
	[ RunInstallerAttribute( true ) ]
	public class SimiasAppInstallerClass: Installer
	{
		/// <summary>
		/// The default mapping directories for the specific platforms.
		/// </summary>
		private static string DefaultWindowsMappingDir = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.CommonApplicationData ), "Simias" );
		private const string MappingFile = "SimiasDirectoryMapping";

		/// <summary>
		/// Constructor.
		/// </summary>
		public SimiasAppInstallerClass() : base()
		{
		}

		/// <summary>
		/// Override the 'Install' method.
		/// </summary>
		/// <param name="savedState"></param>
		public override void Install( IDictionary savedState )
		{
			base.Install( savedState );

			// Get the install location for Simias.exe.
			string assemblyPath = Assembly.GetExecutingAssembly().Location;

			// The SimiasDefaultMapping file is in the directory that contains 'web\bin\Simias.exe'.
			string installDir = Path.GetFullPath( Path.Combine( Path.GetDirectoryName( assemblyPath ), "../../../../" ) );

			// See if the SimiasDirectoryMapping file exists.
			string dirMappingFile = Path.Combine( installDir, MappingFile );
			if ( File.Exists( dirMappingFile ) )
			{
				// Write the install location of Simias to the file.
				try
				{
					// Get the directory where the assembly is running.
					using ( StreamWriter sw = new StreamWriter( dirMappingFile ) )
					{
						sw.WriteLine( assemblyPath );
					}
				}
				catch ( Exception ex )
				{
					if ( Context != null )
					{
						Context.LogMessage( String.Format( "ERROR: Exception {0} writing to {1}", ex.Message, dirMappingFile ) );
					}
				}

				// Copy the changed file to the common application directory.
				string destFile = Path.Combine( DefaultWindowsMappingDir, MappingFile );
				try
				{
					File.Copy( dirMappingFile, destFile, true );
				}
				catch ( Exception ex )
				{
					if ( Context != null )
					{
						Context.LogMessage( String.Format( "ERROR: Exception {0} copying mapping file to {1}", ex.Message, destFile ) );
					}
				}
			}
			else
			{
				if ( Context != null )
				{
					Context.LogMessage( String.Format( "ERROR: Cannot find {0}", dirMappingFile ) );
				}
			}
		}

		/// <summary>
		/// Override the 'Commit' method.
		/// </summary>
		/// <param name="savedState"></param>
		public override void Commit( IDictionary savedState )
		{
			base.Commit( savedState );
		}

		/// <summary>
		/// Override the 'Rollback' method.
		/// </summary>
		/// <param name="savedState"></param>
		public override void Rollback( IDictionary savedState )
		{
			base.Rollback( savedState );
		}

		/// <summary>
		/// Override the 'Uninstall' method.
		/// </summary>
		/// <param name="savedState"></param>
        public override void Uninstall( IDictionary savedState )
		{
            if (savedState == null)
            {
                throw new InstallException("Simias Uninstall: savedState should not be null");
            }
            else
            {
                base.Uninstall(savedState);
                Console.WriteLine("Simias Uninstall");

                // Kill iFolderApp
                Process[] simiasProcesses = Process.GetProcessesByName("Simias");
                foreach (Process process in simiasProcesses)
                {
                    try
                    {
                        process.Kill(); // This will throw if the process is no longer running
                    }
                    catch { }
                    process.Close();
                }

            }
        }
	}
}

#endif
