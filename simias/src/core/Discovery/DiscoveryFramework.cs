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

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Sync;



namespace Simias.Discovery
{
	public class DiscoveryFramework
	{
		private CollectionList collList;

		public DiscoveryFramework()
		{
			collList = CollectionList.GetCollectionList();
		}
		
		public CollectionList GetListOfCollections()
		{
			return collList;
		}
		
		public CollectionInfo GetDetailedCollectionInformation(string domainID, string collectionID, HostNode hNode)
		{

			Member member = Store.GetStore().GetDomain( domainID ).GetCurrentMember();
			DiscoveryService dService = new DiscoveryService();
			SimiasConnection smConn = new SimiasConnection(domainID, member.UserID, SimiasConnection.AuthType.BASIC, hNode);
			smConn.InitializeWebClient(dService, "DiscoveryService.asmx");

			
			return dService.GetCollectionInfo(collectionID);

		}

		public bool CreateProxyCollection(string collectionName, string domainID, HostNode hNode, string collectionID)
		{
			Store store = Store.GetStore();
			ArrayList commitList = new ArrayList();
			SimiasConnection smConn;
			DiscoveryService dService = new DiscoveryService();
			
			Collection c = new Collection(store, collectionName, domainID);
			Domain domain = store.GetDomain(domainID);
			Member member = domain.GetCurrentMember();
			
//			HostNode hNode = store.GetNodeByID(collectionID, hostID)
			
			c.HostID = hNode.ID;
			
			commitList.Add(c);

			member.IsOwner = true;
			member.Proxy = true;
			commitList.Add(member);

			//should we get the connection for the particular member or host?
			smConn = new SimiasConnection(domainID, member.UserID, SimiasConnection.AuthType.BASIC, hNode);
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

			return true;
		}

		public bool RemoveMembership(string domainID, string memberID, string collectionID, HostNode hNode)
		{
			Member member = Store.GetStore().GetDomain( domainID ).GetCurrentMember();
			DiscoveryService dService = new DiscoveryService();
			SimiasConnection smConn = new SimiasConnection(domainID, member.UserID, SimiasConnection.AuthType.BASIC, hNode);
			smConn.InitializeWebClient(dService, "DiscoveryService.asmx");
			// call the web service method
			return {
				dService.RemoveMemberFromCollection( collectionID, memberID);
				}
		}
	}
}

