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
*                 $Author: Rob
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
using System.Diagnostics;

namespace Simias.Client
{
	/// <summary>
	/// My Environment
	/// </summary>
	public class MyEnvironment
	{
		private static MyPlatformID platformID;
		private static MyRuntimeID runtimeID;

		/// <summary>
		/// Static Constructor
		/// </summary>
		static MyEnvironment()
		{
			SetRuntimeID();
			SetPlatformID();
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		private MyEnvironment()
		{
		}

		/// <summary>
		/// The MyPlatformID value of the current operating system on the local machine.
		/// </summary>
		public static MyPlatformID Platform
		{
			get { return platformID; }
		}

		/// <summary>
		/// The MyRuntimeID value of the current common runtime on the local machine.
		/// </summary>
		public static MyRuntimeID Runtime
		{
			get { return runtimeID; }
		}

		/// <summary>
		/// Is the current common runtime Mono?
		/// </summary>
		public static bool Mono
		{
			get { return runtimeID == MyRuntimeID.Mono; }
		}

		/// <summary>
		/// Is the current common runtime .Net?
		/// </summary>
		public static bool DotNet
		{
			get { return runtimeID == MyRuntimeID.DotNet; }
		}

		/// <summary>
		/// Is the current operating system Darwin?
		/// </summary>
		public static bool Darwin
		{
			get { return platformID == MyPlatformID.Darwin; }
		}

		/// <summary>
		/// Is the current operating system Unix?
		/// </summary>
		public static bool Unix
		{
			get { return platformID == MyPlatformID.Unix; }
		}

		/// <summary>
		/// Is the current operating system Windows?
		/// </summary>
		public static bool Windows
		{
			get { return platformID == MyPlatformID.Windows; }
		}

		/// <summary>
		/// Get the username for the current session
		/// </summary>
		public static string UserName
		{
			get
			{
				// Stip off all components that backslash separated as is
				// the case when the machine is connected to Active Directory
				string[] comps = Environment.UserName.Split( '\\' );
				return comps[ comps.Length - 1 ];
			}
		}

		/// <summary>
		/// Discover and set the runtime ID
		/// </summary>
		private static void SetRuntimeID()
		{
			if (Type.GetType("Mono.Runtime", false) != null)
			{
				// Mono
				runtimeID = MyRuntimeID.Mono;
			}
			else
			{
				// assume DotNet
				runtimeID = MyRuntimeID.DotNet;
			}
		}

		/// <summary>
		/// Discover and set the platform ID
		/// </summary>
		private static void SetPlatformID()
		{
			int platform = (int) Environment.OSVersion.Platform;
			// on the Mono 1.0 profile, the value for unix is 128, (MS.NET 1.x does not have an enum field for unix)
			// on the Mono 2.0 profile, and MS.NET 2.0 the value for unix is 4
			if (Mono && (platform == 128 || platform == 4))
			{
				// Unix
				platformID = MyPlatformID.Unix;

				Process process = new Process();
                        	process.StartInfo.FileName = "uname";
                        	process.StartInfo.UseShellExecute = false;
                        	process.StartInfo.RedirectStandardOutput = true;
                        	process.Start();
                        	string output = process.StandardOutput.ReadToEnd();
                        	process.WaitForExit();
				if( output.StartsWith("Darwin") )
				{
					platformID = MyPlatformID.Darwin;
				}
			}
			else
			{
				// Windows
				platformID = MyPlatformID.Windows;
			}
		}
	}
}
