/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Kalidas Balakrishnan <bkalidas@novell.com>
 *
 ***********************************************************************/


using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Service;
using Simias.Client;
using Simias.Storage;
using Simias.Sync;



namespace Simias.Discovery
{
	public class DiscoveryFramework
	{
		private ArrayList collList;
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		public DiscoveryFramework()
		{
			collList = Simias.Discovery.CollectionList.GetCollectionList();
		}
		
		public ArrayList GetListOfCollections()
		{
			return collList;
		}
		
		public CollectionInfo GetDetailedCollectionInformation(string domainID, string collectionID, HostNode hNode)
		{
			CollectionInfo colInfo = null;

			Member member = Store.GetStore().GetDomain( domainID ).GetCurrentMember();
			try
			{
				SimiasConnection smConn = new SimiasConnection(domainID, member.UserID, SimiasConnection.AuthType.BASIC, hNode);
				DiscoveryService dService = new DiscoveryService();
				smConn.InitializeWebClient(dService, "DiscoveryService.asmx");
				colInfo = dService.GetCollectionInfo(collectionID);
			}
			catch(Exception ex)
			{
				log.Error(ex.Message);
			}
			
			return colInfo;

		}

		public bool CreateProxyCollection(string collectionName, string domainID, HostNode hNode, string collectionID)
		{
			Store store = Store.GetStore();
			ArrayList commitList = new ArrayList();
			SimiasConnection smConn;
			
			Collection c = new Collection(store, collectionName, domainID);
			Domain domain = store.GetDomain(domainID);
			Member member = domain.GetCurrentMember();
			
//			HostNode hNode = store.GetNodeByID(collectionID, hostID)
			
			c.HostID = hNode.ID;
			
			commitList.Add(c);

			member.IsOwner = true;
			member.Proxy = true;
			commitList.Add(member);

			try
			{
				//should we get the connection for the particular member or host?
				smConn = new SimiasConnection(domainID, member.UserID, SimiasConnection.AuthType.BASIC, hNode);
				DiscoveryService dService = new DiscoveryService();
				smConn.InitializeWebClient(dService, "DiscoveryService.asmx");
				// calls the GetCollectionDirNodeID WebService API
				string dirNodeID = dService.GetCollectionDirNodeID ( c.ID );
				DirNode dirNode = new DirNode(c, dirNodeID);
				string path = dirNode.GetFullPath(c);
				
				if (!Directory.Exists(path)) Directory.CreateDirectory(path);
				
					dirNode.Proxy = true;
					commitList.Add(dirNode);
				
				c.Proxy = true;
				c.Commit((Node[]) commitList.ToArray(typeof(Node)));
			}
			catch(Exception ex)
			{
				log.Error(ex.Message);
			}

			return true;
		}

		public bool RemoveMembership(string domainID, string memberID, string collectionID, HostNode hNode)
		{
			bool removed = false;
			Member member = Store.GetStore().GetDomain( domainID ).GetCurrentMember();
			try
			{
				DiscoveryService dService = new DiscoveryService();
				SimiasConnection smConn = new SimiasConnection(domainID, member.UserID, SimiasConnection.AuthType.BASIC, hNode);
				smConn.InitializeWebClient(dService, "DiscoveryService.asmx");
				removed = dService.RemoveMemberFromCollection( collectionID, memberID);
			}
			catch(Exception ex)
			{
				log.Error(ex.Message);
			}

			return removed;
		}
	}
}

