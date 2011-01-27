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
*                 $Author: Kalidas Balakrishnan <bkalidas@novell.com>
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
using System.Collections.Specialized;
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
        /// <summary>
        /// 
        /// </summary>
	public class DiscoveryFramework
	{
		private static ArrayList collList;
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

        /// <summary>
        /// Constructor to create new instance of DiscoveryFramework
        /// </summary>
		public DiscoveryFramework()
		{
			collList = Simias.Discovery.CollectionList.GetCollectionList();
		}
		
        /// <summary>
        /// Get the Collection list
        /// </summary>
        /// <returns>Collection List</returns>
		public static ArrayList GetListOfCollections()
		{
			return collList;
		}
		
        /// <summary>
        /// Get the collections for a particular domain
        /// </summary>
        /// <param name="DomainID">ID of the domain whose collections details are needed</param>
        /// <returns>Array of collections</returns>
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

        /// <summary>
        /// Get the detailed information of collections for a domain
        /// </summary>
        /// <param name="domainID">ID of the domain whose collection info needed</param>
        /// <param name="catalogInfoArray">Details of the catalog</param>
        /// <param name="dService">object of Discovery service</param>
        /// <returns></returns>
		public static ArrayList GetDetailedCollectionInformation (string domainID, CatalogInfo[] catalogInfoArray, DiscoveryService dService)
		{
		        ArrayList colInfo = new ArrayList();
			DiscoveryService locService = dService;

			NameValueCollection collectionsOnHost = new NameValueCollection();

 			Member member = Store.GetStore().GetDomain( domainID ).GetCurrentMember();

			foreach ( CatalogInfo ci in catalogInfoArray )
			{
			        //Collect all the CollectionIDs for HostID.
			        collectionsOnHost.Add ( ci.HostID, ci.CollectionID );
			}

			foreach ( string hostID in collectionsOnHost.AllKeys)
			{
			        try 
				{
				       if (member.HomeServer != null && hostID == member.HomeServer.UserID)
				       {
					   //We already have a connection. Reuse it.
					       colInfo.AddRange (locService.GetAllCollectionInfo (collectionsOnHost.GetValues (hostID), member.UserID));
				       }
				       else 
				       {
					   //Get all collection info in one call.
					       HostNode hNode = HostNode.GetHostByID (domainID, hostID);
					       SimiasConnection smConn = new SimiasConnection(domainID,
											      member.UserID,
											      SimiasConnection.AuthType.BASIC, 
											      hNode);
					       DiscoveryService discService = new DiscoveryService();
					       smConn.InitializeWebClient(discService, "DiscoveryService.asmx");
					       colInfo.AddRange (discService.GetAllCollectionInfo (collectionsOnHost.GetValues (hostID), member.UserID));
				       }
				}
				catch(Exception ex)
				{
				        log.Error(ex.Message);
				}

			}
			
			locService = null;
			return colInfo;

		}

        /// <summary>
        /// Get the info from collection ID
        /// </summary>
        /// <param name="collectionID">ID of the collection whose info is needed</param>
        /// <returns>Object of CollectionInfo</returns>
		public static CollectionInfo GetCollectionInfo (string collectionID)
		{
			Simias.Discovery.DiscService.UpdateCollectionList();
			ArrayList collectionList = Simias.Discovery.CollectionList.GetCollectionList();

			//TEMPFIX : We'll wait if we the obj is updating.
			lock (collectionList)
			{
			    foreach( CollectionInfo c in collectionList)
			    {
			        if (c.CollectionID != collectionID)
				        continue;
				return c;
			    }
			}
			return null;
		}

        /// <summary>
        /// Get the info about local collection
        /// </summary>
        /// <param name="collectionID">ID of the local collection</param>
        /// <returns>Object of CollectionInfo</returns>
        public static CollectionInfo GetLocalCollectionInfo(string collectionID)
        {
            //Simias.Discovery.DiscService.UpdateCollectionList();
            ArrayList collectionList = Simias.Discovery.CollectionList.GetCollectionList();

            //TEMPFIX : We'll wait if we the obj is updating.
            lock (collectionList)
            {
                foreach (CollectionInfo c in collectionList)
                {
                    if (c.CollectionID != collectionID)
                        continue;
                    return c;
                }
            }
            return null;
        }

        /// <summary>
        /// Create a proxy for a domain
        /// </summary>
        /// <param name="store">Cbject to refer to store</param>
        /// <param name="cinfo">Collection info object</param>
        /// <param name="DomainID">Id of the domain</param>
        /// <param name="iFolderID">iFolder ID</param>
        /// <param name="localPath">Local path of iFolder</param>
		public static void CreateProxy (Store store, CollectionInfo cinfo, string DomainID,string iFolderID, string localPath )
		{
			ArrayList commitList = new ArrayList();

//			CollectionInfo cinfo = DiscoveryFramework.GetCollectionInfo (iFolderID);

			Collection c = new Collection(store, cinfo.Name,
						      cinfo.CollectionID, DomainID);

			c.HostID = cinfo.HostID;

			commitList.Add(c);

			// Create proxy member node
                        Domain domain = store.GetDomain(DomainID);
			Member m = domain.GetCurrentMember ();

			Member member = new Member(m.Name, cinfo.MemberNodeID, m.UserID, Simias.Storage.Access.Rights.Admin, null);
			member.IsOwner = true;
			member.Proxy = true;
			commitList.Add(member);

 			DirNode dn = new DirNode(c, localPath, cinfo.DirNodeID);

 			if (!Directory.Exists(localPath)) 
			        Directory.CreateDirectory(localPath);

 			dn.Proxy = true;
 			commitList.Add(dn);

			c.Proxy = true;
			c.Commit((Node[]) commitList.ToArray(typeof(Node)));
		}

        /// <summary>
        /// Get the host node depending on domain ID and collectionID
        /// </summary>
        /// <param name="domainID">ID of the domain</param>
        /// <param name="collectionID">Collection ID</param>
        /// <returns>Object of HostNode</returns>
		private static HostNode GetHostNode(string domainID, string collectionID)
		{
			try
			{
				HostNode hNode = null;
				CollectionInfo ci = GetCollectionInfo(collectionID);
				if( ci != null)
				{
					string hostID = ci.HostID;
					if (hostID != null)
					{
						Domain domain = Store.GetStore().GetDomain( domainID );
						Member hMember = domain.GetMemberByID(hostID);
						if (hMember != null)
						{
							log.Debug("Initializing hNode");
							hNode = new HostNode(hMember);
						}
        	                        }
				}
				return hNode;
			}
			catch(Exception ex)
			{
				log.Error(ex.Message);
				return null;
			}
		}

        /// <summary>
        /// Remove the membership of collection from domain
        /// </summary>
        /// <param name="domainID">Domain ID where collection resides</param>
        /// <param name="collectionID">Collection ID to remove from domain</param>
        /// <returns></returns>
		public static bool RemoveMembership(string domainID, string collectionID)
		{
			bool removed = false;
			Domain domain = Store.GetStore().GetDomain( domainID );
			Member member = domain.GetCurrentMember();
			HostNode hNode = GetHostNode(domainID, collectionID);
			if( hNode == null)
				hNode = member.HomeServer;
			try
			{
				DiscoveryService dService = new DiscoveryService();
				SimiasConnection smConn = new SimiasConnection(domainID, member.UserID, SimiasConnection.AuthType.BASIC, hNode);
				smConn.InitializeWebClient(dService, "DiscoveryService.asmx");
				removed = dService.RemoveMemberFromCollection( collectionID, member.UserID);
			}
			catch(Exception ex)
			{
				log.Error(ex.Message);
			}

			return removed;
		}

        /// <summary>
        /// Delete the collection from catalog
        /// </summary>
        /// <param name="domainID">Domain ID in which collection is present</param>
        /// <param name="collectionID">Collection ID to delete</param>
                public static void DeleteCollectionInCatalog(string domainID, string collectionID)
                {
			log.Debug("Domain ID {0}, col ID {1}", domainID, collectionID);
                        Member member = Store.GetStore().GetDomain( domainID ).GetCurrentMember();
			HostNode hNode = GetHostNode(domainID, collectionID);
			if( hNode == null)
				hNode = member.HomeServer;
                        try
                        {
					log.Debug("DeleteCollectionInCatalog called from client");

                                DiscoveryService dService = new DiscoveryService();
                                SimiasConnection smConn = new SimiasConnection(domainID, member.UserID, SimiasConnection.AuthType.BASIC, hNode);
                                smConn.InitializeWebClient(dService, "DiscoveryService.asmx");
                                dService.DeleteCollectionInCatalog( collectionID );
                        }
                        catch(Exception ex)
                        {
                                log.Error(ex.Message);
                        }

                        return ;
		}

	}
}



