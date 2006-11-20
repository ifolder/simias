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
using System.Collections;
using System.Net;

using Simias.Client;
using Simias.Storage;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder Server Result Set
	/// </summary>
	[Serializable]
	public class iFolderServerSet
	{
		/// <summary>
		/// An Array of iFolder Servers
		/// </summary>
		public iFolderServer[] Items;

		/// <summary>
		/// The Total Number of iFolder Servers
		/// </summary>
		public int Total;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public iFolderServerSet()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="items"></param>
		/// <param name="total"></param>
		public iFolderServerSet(iFolderServer[] items, int total)
		{
			this.Items = items;
			this.Total = total;
		}
	}

	/// <summary>
	/// An iFolder Server
	/// </summary>
	[Serializable]
	public class iFolderServer
	{
		/// <summary>
		/// Server ID
		/// </summary>
		public string ID;

		/// <summary>
		/// Server Name
		/// </summary>
		public string Name;
		
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
		/// The public address for this server.
		/// </summary>
		public string PublicUrl;

		/// <summary>
		/// The private address for this server.
		/// </summary>
		public string PrivateUrl;

		/// <summary>
		/// True if this server is the master.
		/// </summary>
		public bool IsMaster;

		/// <summary>
		/// True if this server is the local server.
		/// </summary>
		public bool IsLocal;

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderServer() : this(HostNode.GetLocalHost())
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="server">HostNode object</param>
		public iFolderServer(HostNode server)
		{
			ID = server.UserID;
			Name = server.Name;
			PublicUrl = server.PublicUrl;
			PrivateUrl = server.PrivateUrl;
			IsMaster = server.IsMasterHost;
			IsLocal = server.IsLocalHost;

			Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
			HostName = System.Net.Dns.GetHostName();
			MachineName = System.Environment.MachineName;
			OSVersion = System.Environment.OSVersion.ToString();
			UserName = System.Environment.UserName;
			ClrVersion = System.Environment.Version.ToString();
		}

		/// <summary>
		/// Get the Master iFolder Server in the system
		/// </summary>
		/// <returns>An iFolder Server Object</returns>
	        public static iFolderServer GetMasterServer ()
		{
		        iFolderServerSet ServerList = GetServersByName (iFolderServerType.Master, SearchOperation.BeginsWith, "*", 0, 0);
			iFolderServer MasterServer = null;
			foreach (iFolderServer server in ServerList.Items)
			{
			    if (server.IsMaster)
			    {
				MasterServer = server;
				break;
			    }
			}

		        return MasterServer;
		}


		/// <summary>
		/// Get the HomeServer URL for User.
		/// </summary>
		/// <returns>Return the URL for the server </returns>
		public static string GetHomeServerForUser( string username, string password )
		{
//
		        string publicUrl;
			try
			{
			        iFolderServer MasterServer = GetMasterServer();
			        DomainService domainService = new DomainService();

				domainService.Url = MasterServer.PublicUrl + "/DomainService.asmx";
				domainService.Credentials = new NetworkCredential(username, password);
				domainService.PreAuthenticate = true;

				publicUrl = domainService.GetHomeServer( username ).PublicAddress;

			}
			catch ( Exception ex )
			{
			        throw (ex);
			}
			return publicUrl;
		}

		/// <summary>
		/// Get the iFolder Home Server Information Object
		/// </summary>
		/// <returns>An iFolder Server Object</returns>
		public static iFolderServer GetHomeServer()
		{
			return new iFolderServer();
		}

		/// <summary>
		/// Get the iFolder Server Information Objects
		/// </summary>
		/// <returns>An Array of iFolder Server Object</returns>
		public static iFolderServer[] GetServers()
		{
			ArrayList list = new ArrayList();
			
			list.Add(GetHomeServer());

			return (iFolderServer[])list.ToArray(typeof(iFolderServer));
		}

		/// <summary>
		/// Get an iFolder Server Information object.
		/// </summary>
		/// <param name="serverID">The Server ID</param>
		/// <returns>An iFolderServer Object</returns>
		public static iFolderServer GetServer(string serverID)
		{
			Store store = Store.GetStore();

			// use host id
			HostNode host = HostNode.GetHostByID(store.DefaultDomain, serverID);

			// check username also
			if (host == null) host = HostNode.GetHostByName(store.DefaultDomain, serverID);

			// not found
			if (host == null) throw new ServerDoesNotExistException(serverID);

			// server
			return new iFolderServer(host);
		}

		/// <summary>
		/// Get an iFolder Server Information object by Name
		/// </summary>
		/// <param name="serverName">The iFolder Server Name</param>
		/// <returns>An iFolder Server Object</returns>
		public static iFolderServer GetServerByName(string serverName)
		{
			Store store = Store.GetStore();
			
			HostNode host = HostNode.GetHostByName(store.DefaultDomain, serverName);

			// not found
			if (host == null) throw new ServerDoesNotExistException(serverName);

			// server
			return new iFolderServer(host);
		}

		/// <summary>
		/// Get iFolder Servers by Name
		/// </summary>
		/// <param name="type">iFolder Server Type</param>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <returns>A Set of iFolder Server Objects</returns>
		public static iFolderServerSet GetServersByName(iFolderServerType type, SearchOperation operation, string pattern, int index, int max)
		{
			bool isMaster = ((type == iFolderServerType.Master) || (type == iFolderServerType.All));
			bool isLocal = ((type == iFolderServerType.Local) || (type == iFolderServerType.All));

			Store store = Store.GetStore();

			// domain
			Domain domain = store.GetDomain(store.DefaultDomain);

			// search operator
			SearchOp searchOperation;

			switch(operation)
			{
				case SearchOperation.BeginsWith:
					searchOperation = SearchOp.Begins;
					break;

				case SearchOperation.EndsWith:
					searchOperation = SearchOp.Ends;
					break;

				case SearchOperation.Contains:
					searchOperation = SearchOp.Contains;
					break;

				case SearchOperation.Equals:
					searchOperation = SearchOp.Equal;
					break;

				default:
					searchOperation = SearchOp.Contains;
					break;
			}
			
			ICSList members = domain.Search(BaseSchema.ObjectName, pattern, searchOperation);

			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in members)
			{
				// throw away non-members
				if (sn.IsBaseType(NodeTypes.MemberType))
				{
					Member member = new Member(domain, sn);

					if (member.IsType(HostNode.HostNodeType))
					{
						HostNode node = new HostNode(member);

					        if ((i >= index) && ((max <= 0) || i < (max + index)))
						    //&& ((isMaster && node.IsMasterHost) || (isLocal && node.IsLocalHost)))
						{
							list.Add(new iFolderServer(node));
						}

						++i;
					}
				}
			}

			return new iFolderServerSet(list.ToArray(typeof(iFolderServer)) as iFolderServer[], i);
		}
	}
}
