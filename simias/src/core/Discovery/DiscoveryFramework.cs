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
		
		public CollectionInfo GetDetailedCollectionInformation(string collectionID, string hostID)
		{

			DiscoveryService dService = new DiscoveryService();
			CollectionInfo collInformation = dService.GetCollectionInfo(collectionID);
			return collInformation;

		}

		public bool CreateProxyCollection(Store store, string collectionName, string domainID, string hostID, string collectionID)
		{
			ArrayList commitList = new ArrayList();
			SimiasConnection smConn;
			DiscoveryService dService = new DiscoveryService();
			
			Collection c = new Collection(store, collectionName, domainID);
			string uID = store.GetUserIDFromDomainID(domainID);
			Domain domain = store.GetDomain(domainID);
			HostNode hNode = store.GetNodeByID(collectionID, hostID)
			
			c.HostID = hostID;
			
			commitList.Add(c);

			Member member = new Member(/*need username, rights*/);
			member.IsOwner = true;
			member.Proxy = true;
			commitList.Add(member);

			//should we get the connection for the particular member or host?
			smConn = new SimiasConnection(domainID, uID, SimiasConnection.AuthType.BASIC, hNode);
			smConn.Authenticate();
			smConn.InitializeWebClient(DiscoveryService, "DiscoveryService.asmx");
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

		public bool RemoveMembership(string collectionID, string UserID)
		{

			// call the web service method
			return {
				RemoveMemberFromCollection( collectionID, UserID);
				}
		}
	}
}
