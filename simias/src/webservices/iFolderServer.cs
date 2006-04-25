/***********************************************************************
 *  $RCSfile: iFolderServer.cs,v $
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
 *  Author: Rob
 * 
 ***********************************************************************/
 
using System;

using Simias.Storage;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder Server
	/// </summary>
	[Serializable]
	public class iFolderServer
	{
		/// <summary>
		/// Server Version
		/// </summary>
		public string Version;
		
		/// <summary>
		/// The Host Name
		/// </summary>
		public string HostName;
		
		/// <summary>
		/// The Machine Name
		/// </summary>
		public string MachineName;
		
		/// <summary>
		/// The OS Version
		/// </summary>
		public string OSVersion;
		
		/// <summary>
		/// The User Name
		/// </summary>
		public string UserName;

		/// <summary>
		/// The common language runtime version.
		/// </summary>
		public string ClrVersion;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderServer()
		{
		}

		/// <summary>
		/// Get the iFolder Server Information Object
		/// </summary>
		/// <returns>An iFolder Server Object</returns>
		public static iFolderServer GetServer()
		{
			iFolderServer server = new iFolderServer();

			server.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
			server.HostName = System.Net.Dns.GetHostName();
			server.MachineName = System.Environment.MachineName;
			server.OSVersion = System.Environment.OSVersion.ToString();
			server.UserName = System.Environment.UserName;
			server.ClrVersion = System.Environment.Version.ToString();


			return server;
		}
	}
}
