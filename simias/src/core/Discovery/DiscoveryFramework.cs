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
		private static ArrayList collList;
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		public DiscoveryFramework()
		{
			collList = Simias.Discovery.CollectionList.GetCollectionList();
		}
		
		public static ArrayList GetListOfCollections()
		{
			return collList;
		}
		
		public static ArrayList GetCollectionsForDomain( string DomainID)
		{
		        ArrayList collectionList = new ArrayList ();
		        foreach (CollectionInfo ci in collList)
			{
			    if ( ci.DomainID != DomainID)
				continue;
			    collectionList.Add (ci);
			}
			return collectionList;
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
				colInfo = dService.GetCollectionInfo (collectionID);
			}
			catch(Exception ex)
			{
				log.Error(ex.Message);
			}
			
			return colInfo;

		}

		public static CollectionInfo GetCollectionInfo (string collectionID)
		{
//		        ArrayList collectionList =  GetListOfCollections();

			ArrayList collectionList = Simias.Discovery.CollectionList.GetCollectionList();
			foreach( CollectionInfo c in collectionList)
			{
			        if (c.CollectionID != collectionID)
				        continue;
				return c;
 			}

			return null;
		}

//		public static void CreateProxyCollection (Store store, string DomainID,string iFolderID, string localPath )
		public static void CreateProxy (Store store, string DomainID,string iFolderID, string localPath )
		{
			ArrayList commitList = new ArrayList();

			CollectionInfo cinfo = DiscoveryFramework.GetCollectionInfo (iFolderID);

			Collection c = new Collection(store, cinfo.Name,
						      cinfo.CollectionID, DomainID);

			c.HostID = cinfo.HostID;
//			c.HostID = HostNode.GetLocalHost().UserID; ///this is what is done in subscriptions .. but why ??
			
			commitList.Add(c);

			// Create the member node
                        Domain domain = store.GetDomain(DomainID);
			Member m = domain.GetCurrentMember ();

			Member member = new Member(m.Name, cinfo.MemberNodeID, m.UserID, Simias.Storage.Access.Rights.Admin, null);
			member.IsOwner = true;
			member.Proxy = true;
			commitList.Add(member);

                        //NOTE : Do proper checking before commiting code
// 			if (((this.DirNodeID != null) && (this.DirNodeID.Length > 0))
// 				&& (this.DirNodeName != null) && (this.DirNodeName.Length > 0)
// 				&& (this.CollectionRoot != null) && (this.CollectionRoot.Length > 0))
// 			{

                        //DN Syncing issue
 			DirNode dn = new DirNode(c, localPath, cinfo.DirNodeID);
// 			DirNode dn = new DirNode(c, localPath);
 			if (!Directory.Exists(localPath)) Directory.CreateDirectory(localPath);

			//NOTE : Wont work. Exception : Cannot change a system property.

// 			Property path = new Property( PropertyTags.FileSystemPath, localPath );
// 			path.LocalProperty = true; 
// 			dn.Properties.ModifyProperty( path );

 			dn.Proxy = true;
 			commitList.Add(dn);
//			}

			c.Proxy = true;
			c.Commit((Node[]) commitList.ToArray(typeof(Node)));
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



