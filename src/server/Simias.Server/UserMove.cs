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
*                 $Author: Mahabaleshwar
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Threading;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Service;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Server
{
	public class UserMove
	{
		private static string titleClass = "Simias.UserMovement.iFolderUserMove";
		private static string ReprovisionMethod = "Reprovision";
        /// <summary>
        /// Reprovision method to reprovision on a server
        /// </summary>
        /// <param name="userID">user id </param>
        /// <param name="masterServer">Master server object</param>
        /// <param name="currentServer">Current Homeserver object</param>
        /// <param name="newServer">New server where it needs to be provisioned</param>
        /// <returns>New HostInfo after success, otherwise null</returns>
		public static Simias.Host.HostInfo Reprovision(string userID, HostNode masterServer, HostNode currentServer, HostNode newServer)
		{
			if(currentServer == null)
				throw new Exception(String.Format("currentServer User move Failed for {0} ",userID));
			Type types = LoadDllMethod();
			MethodInfo mInfo = types.GetMethod(ReprovisionMethod);
			if (mInfo != null)
			{
				object[] prms = new object[4];
				prms[0] = userID;
				prms[1] = masterServer;
				prms[2] = currentServer;
				prms[3] = newServer;
				HostNode hNode = (HostNode)mInfo.Invoke(null, prms);
				if(hNode == null)
					throw new Exception(String.Format("User move Failed for {0} ",userID));
				return new Simias.Host.HostInfo( hNode );
			}
			else
				throw new Exception(String.Format("Unable to call User move method for {0} ",userID));
		}
		
        /// <summary>
        /// method to load the DLL
        /// </summary>
        /// <returns></returns>
		static Type LoadDllMethod()
		{
			Assembly Asmbly = Assembly.LoadWithPartialName(Simias.Service.Manager.UserMovementAssemblyName);
			Type types = Asmbly.GetType(titleClass);
			return types;
		}
	}
}
