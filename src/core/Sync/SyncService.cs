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
*                 $Author: Russ Young, Dale Olds <olds@novell.com>
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
using System.Collections;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;

using Simias.Storage;
using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Policy;
using Simias.Sync.Delta;

namespace Simias.Sync
{
	
	/// <summary>
	/// Class to synchronize access to a collection.
	/// </summary>
	internal class CollectionLock
	{
		static int			totalCount = 0;
		static int			totalDepth = 64;
		static Hashtable	CollectionLocks = new Hashtable();
		const int			queueDepth = 10;	
		int					count = 0;

		static CollectionLock()
		{
			Configuration conf = Store.Config;
			string str = conf.Get(SyncService.configSection, "ConcurrentClients");
			totalDepth = ( str != null ) ? int.Parse(str) : 64;
		}
		
		/// <summary>
		/// Gets a lock on the collection.
		/// </summary>
		/// <param name="collectionID">The collection to block on.</param>
		internal static CollectionLock GetLock(string collectionID)
		{
			CollectionLock cLock;
			Sync.Log.log.Debug("In Getlock");
			lock (CollectionLocks)
			{
				if (totalCount > totalDepth)
					return null;

				cLock = (CollectionLock)CollectionLocks[collectionID];
				if (cLock == null)
				{
					cLock = new CollectionLock();
					CollectionLocks.Add(collectionID, cLock);
				}
			
				if (cLock.count > queueDepth)
					return null;
				else
				{
					cLock.count++;
					totalCount++;
					Sync.Log.log.Debug("Acquired Lock count = {0}", totalCount);
				}
			}
			Sync.Log.log.Debug("Out of get lock");

			return cLock;
		}

		/// <summary>
		/// Release the Lock on the collection.
		/// </summary>
		internal void ReleaseLock()
		{
			lock (CollectionLocks)
			{
				count--;
				totalCount--;
				Sync.Log.log.Debug("Released Lock count = {0}", totalCount);
			}
		}
	}

	
	/// <summary>
	/// server side top level class of SynkerA-style synchronization
	/// </summary>
	public class SyncService
	{
		internal static string configSection = "SyncService";
		static Store store = Store.GetStore();
		/// <summary>
		/// Used to log to the log file.
		/// </summary>
		public static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncService));
		Collection collection;
		CollectionLock	cLock;
		Member			member;
		Access.Rights	rights = Access.Rights.Deny;
		ArrayList		NodeList = new ArrayList();
		ServerInFile	inFile;
		ServerOutFile	outFile;
		SyncPolicy		policy;
		string			sessionID;
		IEnumerator		nodeContainer;
		string			syncContext;
		bool			getAllNodes;
		const int		MaxBuffSize = 1024 * 64;
		string Requester = null;
		SimiasAccessLogger logger;
			
        
		/// <summary>
		/// Finalizer.
		/// </summary>
		~SyncService()
		{
			Dispose(true);		
		}

		/// <summary>
		/// Called to dispose.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inFinalize"></param>
		private void Dispose(bool inFinalize)
		{
			lock (this)
			{
				if (!inFinalize)
				{
					GC.SuppressFinalize(this);
				}
				if (cLock != null)
				{
					cLock.ReleaseLock();
					cLock = null;
				}
				if (inFile != null)
				{
					inFile.Close(false);
					inFile = null;
				}
				if (outFile != null)
				{
					outFile.Close();
					outFile = null;
				}
			}
		}

		/// <summary>
		/// Get they sync policy for this collection.
		/// </summary>
		private SyncPolicy Policy
		{
			get
			{
				if (policy == null)
					policy = new SyncPolicy(collection);
				return policy;
			}
		}
	
		/// <summary>
		/// start sync of this collection -- perform basic role checks and dredge server file system
		/// </summary>
		/// <param name="si">The start info to initialize the sync.</param>
		/// <param name="user">This is temporary.</param>
		/// <param name="sessionID">The unique sessionID.</param>
		public void Start(ref StartSyncInfo si, string user, string sessionID)
		{
			this.sessionID = sessionID;
			si.Status = StartSyncStatus.Success;
			
			rights = si.Access = Access.Rights.Deny;
			syncContext = si.Context;
			cLock = null;
			nodeContainer = null;
			getAllNodes = false;
            HostNode hNode = HostNode.GetLocalHost();
            si.HostID = hNode.ID;
			Requester = si.Requester;
			IEnumerator		tmpnodeContainer;
			Sync.Log.log.Debug("started syncservice");
            
			collection = store.GetCollectionByID(si.CollectionID);
			if (collection == null)
			{
                try
                {
                    Domain domain = store.GetDomain(store.DefaultDomain);
                    if (domain != null)
                    {
                        Member m = domain.GetCurrentMember();

                        if (m != null && hNode != null)
                        {
                            DiscoveryService dservice = new DiscoveryService();
                            SimiasConnection smconn = new SimiasConnection(domain.ID, m.UserID, SimiasConnection.AuthType.PPK, hNode);
                            dservice.Url = hNode.PublicUrl;
                            smconn.Authenticate();
                            smconn.InitializeWebClient(dservice, "DiscoveryService.asmx");
                            CatalogInfo catInfo = dservice.GetCatalogInfoForCollection(si.CollectionID);
                            if (catInfo == null)
                                log.Debug("catInfo returned null for this collection ID ");

                            if (catInfo != null && catInfo.HostID != null)
                            {
                                log.Debug("sending host id back : " + catInfo.HostID.ToString());
                                si.Status = StartSyncStatus.Moved;
                                si.HostID = catInfo.HostID;
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Debug("Start: Unable to Read Catalog");
                }
				si.Status = StartSyncStatus.NotFound;
				return;
			}

			// If we are locked return locked.
			if (collection.IsLocked)
			{
				si.Status = StartSyncStatus.Locked;
				return;
			}

			else if( collection.Disabled)
			{
				si.Status = StartSyncStatus.SyncDisabled;
			}
		
			// Check our rights.
			member = null;
			string userID = Thread.CurrentPrincipal.Identity.Name;
			if (userID != null && userID.Length != 0)
			{
				member = collection.GetMemberByID(userID);
                        	if(member == null && Simias.Service.Manager.LdapServiceEnabled == true)
                        	{
                                        Domain domain = store.GetDomain(store.DefaultDomain);
                                	string[] IDs = domain.GetMemberFamilyList(userID);
                                	foreach(string id in IDs)
                                	{
                                        	member = collection.GetMemberByID(id);
                                        	if(member != null)
                                                	break;
                                	}
                             }
                                	if(member != null)
                                	{
						collection.Impersonate(member);
						rights = member.Rights;
						if (Requester != null && rights == Access.Rights.Secondary)
						{
							si.Access = Access.Rights.ReadWrite;
						}
						else
							si.Access = rights;
                                	}
					else
					{
						// Check if the collection is under movement...
						if( collection.DataMovement == true)
						{
							log.Debug("The collection is being moved... giving permissions though not the owner...");
							member = collection.Owner;
							collection.Impersonate(member);
							si.Access = member.Rights;
							rights = member.Rights;
						}
						else
						{
							si.Status = StartSyncStatus.AccessDenied;
							return;
						}
					}
			}
			else
			{
				// We are not authenticated.
				si.Status = StartSyncStatus.UserNotAuthenticated;
				return;
			}

			logger = new SimiasAccessLogger(member.Name, si.CollectionID);

			switch (rights)
			{
				case Access.Rights.Secondary:
				case Access.Rights.Admin:
				case Access.Rights.ReadOnly:
				case Access.Rights.ReadWrite:
					// Try to get the lock.
					cLock = CollectionLock.GetLock(collection.ID);
					if (cLock == null)
					{
						nodeContainer = null;
						si.Status = StartSyncStatus.Busy;
						logger.LogAccess("Start", collection.Name, collection.ID, "Busy");
						return;
					}
					
					try
					{
						lock (cLock)
						{
							// See if there is any work to do before we try to get the lock.
							if (si.ChangesOnly)
							{
								// we only need the changes.
								nodeContainer = this.BeginListChangedNodes(out si.ChangesOnly);
								si.Context = syncContext;
								// Check if we have any work to do
								if (si.ChangesOnly && !si.ClientHasChanges && nodeContainer == null)
								{
									si.Status = StartSyncStatus.NoWork;
									break;
								}
							}

							// See if we need to return all of the nodes.
							if (!si.ChangesOnly)
							{
								// We need to get all of the nodes.
								nodeContainer = this.BeginListAllNodes();
								si.Context = syncContext;
								if (nodeContainer == null)
								{
									Dispose(false);
									rights = Access.Rights.Deny;
									si.Access = rights;
								}
							}
						}
					}
					finally
					{
						if (si.Status != StartSyncStatus.Success)
						{
							cLock.ReleaseLock();
							cLock = null;
						}
					}
					break;
					case Access.Rights.Deny:
						si.Status = StartSyncStatus.NotFound;
						break;
				
					}
			logger.LogAccess("Start", collection.Name, collection.ID, si.Status.ToString());
			return;
		}

		/// <summary>
		/// Filter out those members which are not part of the logged in user's group , will be called in case all nodes are to be synced
		/// </summary>
		/// <param name="TempChangeList">changelist which contains all nodes</param>
		/// <return> an enumerator containing all the member nodes which are part of member's groups </returns>
		public IEnumerator FilterOutsideObjects(IEnumerator TempChangeList)
		{
			Domain domain = store.GetDomain(store.DefaultDomain);
			ArrayList changeList = new ArrayList();
			string OwnerID = collection.Owner.UserID;
			string[] GroupList = domain.GetMemberFamilyList(member.UserID);
			
			do	
			{
				ShallowNode sn = (ShallowNode) TempChangeList.Current;
				object obj = TempChangeList.Current;
				if (sn.IsBaseType(NodeTypes.MemberType))
				{
					Member mem = new Member(domain, sn);
					string [] TempGroupList = domain.GetMemberFamilyList(mem.UserID);
					if(mem.UserID == OwnerID || mem.IsType("Host"))
					{
						changeList.Add(obj);
					}
					else
					{
						foreach( string TempGroupDN in TempGroupList)
						{
							if(Array.IndexOf(GroupList, TempGroupDN) >= 0 )
							{
								changeList.Add(obj);
								break;	
							}
						}
					}
				}
				else
				{
					changeList.Add(obj);
				}
			}while(TempChangeList.MoveNext());
			
			if(changeList.Count <= 0)
			{
				log.Debug("FilterOutsideObjects: no matching group found, no member to sync");
				return null;
			}
			log.Debug("Priority: BeginListAllNodes: this is the number of changed nodes  :"+changeList.Count);
			return (changeList.GetEnumerator());
				
		}

		/// <summary>
		/// Filter out those members which are not part of the logged in user's group , will be called in case only changed  nodes are to be synced
		/// </summary>
		/// <param name="TempChangeList">changelist which contains all changed nodes</param>
		/// <return> an enumerator containing all the changed nodes which are part of member's groups </returns>
		public IEnumerator FilterOutsideObjects2(IEnumerator TempChangeList)
		{
			Domain domain = store.GetDomain(store.DefaultDomain);
			ArrayList changeList = new ArrayList();
			string OwnerID = collection.Owner.UserID;

			string[] GroupList = domain.GetMemberFamilyList(member.UserID);

			do	
			{
				SyncNodeInfo  syncnode = new SyncNodeInfo( ((ChangeLogRecord)TempChangeList.Current));
				object obj = TempChangeList.Current;
				Member mem = domain.GetMemberByID(syncnode.ID);
				if ( mem != null &&  ((Node)mem).IsBaseType(NodeTypes.MemberType))
				{
					string [] TempGroupList = domain.GetMemberFamilyList(mem.UserID);
					if(mem.UserID == OwnerID)
					{
						changeList.Add(obj);
					}
					else
					{
						foreach( string TempGroupDN in TempGroupList)
						{
							if(Array.IndexOf(GroupList, TempGroupDN) >= 0)
							{
								changeList.Add(obj);
								break;	
							}
						}
					}
				}
				else
				{
					changeList.Add(obj);
				}
			}while(TempChangeList.MoveNext());

			if(changeList.Count <= 0)
			{
				log.Debug("FilterOutsideObjects2: no matching group found, no member to sync");
				return null;
			}

			log.Debug("Priority: BeginListChangedNodes: this is the number of changed nodes  :"+changeList.Count);
			return (changeList.GetEnumerator());
				
		}

		/// <summary>
		/// get next changed nodes 
		/// </summary>
		/// <param name="count"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public SyncNodeInfo[] NextNodeInfoList(ref int count, out string context)
		{
			context = syncContext;
			if (nodeContainer == null)
			{
				count = 0;
				return new SyncNodeInfo[0];
			}

			SyncNodeInfo[] infoArray = new SyncNodeInfo[count];
            int i = 0;
				
			if (getAllNodes)
			{
				for (i = 0; i < count;)
				{
					Node node = null;
					try
					{
						node = Node.NodeFactory(collection, (ShallowNode)nodeContainer.Current);
					}
					catch (Exception ex)
					{
						try{
						ShallowNode sNode = (ShallowNode)nodeContainer.Current;
						log.Debug(" Member not available to download {0}", sNode.Name);}
						catch{}
						node = null;
					}
						
					if (node != null)
					{
						infoArray[i++] = new SyncNodeInfo(node);
					}
					if (!nodeContainer.MoveNext())
					{
						nodeContainer = null;
						break;
					}
				}
			}
			else
			{
				for (i = 0; i < count;)
				{
					infoArray[i++] = new SyncNodeInfo((ChangeLogRecord)nodeContainer.Current);
					if (!nodeContainer.MoveNext())
					{
						bool valid;
						nodeContainer = BeginListChangedNodes(out valid);
						if (nodeContainer == null)
							break;
						break;
					}
				}
				context = syncContext;
			}
			count = i;
			return infoArray;
		}

		/// <summary>
		/// Gets an enumerator that can be used to list a SyncNodeInfo for all objects in the store.
		/// </summary>
		/// <returns>The enumerator or null.</returns>
		private IEnumerator BeginListAllNodes()
		{
			log.Debug("BeginListAllNodes start");
			if (!IsAccessAllowed(Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");

			syncContext = new ChangeLogReader(collection).GetEventContext().ToString();
							
			IEnumerator tempenumerator = collection.GetEnumerator();
			IEnumerator enumerator = null;
			if (!tempenumerator.MoveNext())
			{
				enumerator = null;
			}
			else
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				string ColType = collection.GetType().ToString();
				bool IsDomainType = ColType.Equals("Simias.Storage.Domain") || ColType.Equals("Domain") ;

				if(Requester != null && IsDomainType && domain.GroupSegregated == "yes")
				{
					enumerator = FilterOutsideObjects(tempenumerator);
					if(enumerator == null || !enumerator.MoveNext())
						enumerator = null;
				}
				else
				{
					enumerator = collection.GetEnumerator();
					enumerator.MoveNext();
				}
			}
			
			getAllNodes = true;
			log.Debug("BeginListAllNodes End{0}", enumerator == null ? " Error No Nodes" : "");
			logger.LogAccess("GetChanges", "-", collection.ID, enumerator == null ? "Error" : "Success");
			return enumerator;
		}

		/// <summary>
		/// List the changed nodes
		/// </summary>
		/// <param name="contextValid">Valid context to search</param>
		/// <returns>Enumeration of the list</returns>
		private IEnumerator BeginListChangedNodes(out bool contextValid)
		{
//			log.Debug("BeginListChangedNodes Start");

			IEnumerator enumerator = null;
			IEnumerator tempenumerator = null;
			EventContext eventContext;
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			// Create a change log reader.
			ChangeLogReader logReader = new ChangeLogReader( collection );
			try
			{	
				// Read the cookie from the last sync and then get the changes since then.
				if (syncContext != null && syncContext.Length != 0)
				{
					ArrayList changeList = null;
					eventContext = new EventContext(syncContext);
					logReader.GetEvents(eventContext, out changeList);
					tempenumerator = changeList.GetEnumerator();
					if (!tempenumerator.MoveNext())
					{
						enumerator = null;
					}
					else
					{
						string ColType = collection.GetType().ToString();
						bool IsDomainType = ColType.Equals("Simias.Storage.Domain") || ColType.Equals("Domain") ;
						enumerator = ( Requester != null && IsDomainType && domain.GroupSegregated == "yes") ? FilterOutsideObjects2(tempenumerator) : changeList.GetEnumerator();
						if(enumerator == null || !enumerator.MoveNext())
						{
							enumerator = null;
						}	
					}
					
					log.Debug("BeginListChangedNodes End. Found {0} changed nodes.", changeList.Count);
					syncContext = eventContext.ToString();
					contextValid = true;
					getAllNodes = false;
					logger.LogAccess("GetChanges", "-", collection.ID, "Success");
					return enumerator;
				}
			}
			catch (Exception ex)
			{
				log.Debug(ex, "BeginListChangedNodes");
			}

			// The cookie is invalid.  Get a valid cookie and save it for the next sync.
			eventContext = logReader.GetEventContext();
			if (eventContext != null)
				syncContext = eventContext.ToString();
//			log.Debug("BeginListChangedNodes End");
			logger.LogAccess("GetChanges", "-", collection.ID, "Error");
			contextValid = false;
			return null;
		}

		/// <summary>
		/// Called when done with the sync cycle.
		/// </summary>
		public void Stop()
		{
			Dispose(false);
			collection.Revert();
			logger.LogAccess("Stop", "-", collection.ID, "Success");
			this.collection = null;
		}

		/// <summary>
		/// Checks if the current user has rights to perform the desired operation.
		/// </summary>
		/// <param name="desiredRights">The desired operation.</param>
		/// <returns>True if allowed.</returns>
		private bool IsAccessAllowed(Access.Rights desiredRights)
		{
			return (rights >= desiredRights) ? true : false;
		}

        /// <summary>
        /// Import the node to collection
        /// </summary>
        /// <param name="node">Node to be imported</param>
        /// <param name="expectedIncarn">Size of the node to be incarnated</param>
		private void Import(Node node, ulong expectedIncarn)
		{
			collection.ImportNode(node, true, expectedIncarn);
			node.IncarnationUpdate = node.LocalIncarnation;
		}

		/// <summary>
		/// Store the supplied nodes in the store.
		/// </summary>
		/// <param name="nodes">The array of nodes to store.</param>
		/// <returns>The status of the operation.</returns>
		public SyncNodeStatus[] PutNonFileNodes(SyncNode [] nodes)
		{
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadWrite))
			{
				return null;
			}

			SyncNodeStatus[]	statusList = new SyncNodeStatus[nodes.Length];
			
			lock (cLock)
			{
				// Try to commit all the nodes at once.
				int i = 0;
				bool DontTransferOwner = false;
				foreach (SyncNode sn in nodes)
				{
					statusList[i] = new SyncNodeStatus();
					statusList[i].status = SyncStatus.ServerFailure;
				
					if (sn != null)
					{
						statusList[i].nodeID = sn.ID;
						XmlDocument xNode = new XmlDocument();
						xNode.LoadXml(sn.node);
						Node node = Node.NodeFactory(store, xNode);
						Import(node, sn.MasterIncarnation);
                        /*SyncPolicy syncpolicy = new SyncPolicy(collection);
                        log.Debug("anil:syncservice: syncpolicy formed for coll : " + collection.ID);
                        SyncPolicy NodeSyncPolicy = null;
                        if ((node.Properties.HasProperty(PropertyTags.Owner) == true) &&
                        (node.Properties.State != PropertyList.PropertyListState.Delete))
                        {
                            if (node.AceProp == null)
                            {
                                log.Debug("anil;syncservice : aceprop returned null");
                            }
                            else
                            {
                                log.Debug("anil:syncerv: aceprop returned " + node.AceProp);
                                NodeSyncPolicy = new SyncPolicy(node);
                                log.Debug("anil:syncservice: syncpolicy formed for node : " + node.ID);
                            }
                            
                            
                        }
                        if (NodeSyncPolicy != null && NodeSyncPolicy.OwnershipTransferAllowedForMember())
                        {
                            log.Debug("anil:syncservice: nodesyncPol is not null and ownership transfer is allowed");
                        }
                        if (node.IsType(NodeTypes.MemberType) && !syncpolicy.SharingAllowed())
                        {
                            log.Debug("anil:syncservice: sharing not allowed : ");
                            NodeList.Add(null);
                            statusList[i++].status = SyncStatus.PolicySharing;
                        }
                        else
                            if (node.IsType(NodeTypes.MemberType) && NodeSyncPolicy != null && !NodeSyncPolicy.OwnershipTransferAllowedForMember())
                            {
                                //Store store = Store.GetStore();
                                //Domain domain = store.GetDomain(store.DefaultDomain);
                                Node NodeofOwnerBeforeSync = (store.GetCollectionByID(collection.ID)).Owner as Node;
                                //NodeofOwnerBeforeSync.Properties.ModifyNodeProperty(PropertyTags.MasterIncarnation, NodeofOwnerBeforeSync.MasterIncarnation++);
                                log.Debug("anil:syncservice: ownership transfer not allowed : ");
                                NodeList.Add(NodeofOwnerBeforeSync);
                                //collection.Owner = (store.GetCollectionByID(collection.ID)).Owner;
                                if ((int)(NodeSyncPolicy.reason) == 6)
                                    statusList[i++].status = SyncStatus.PolicyLimit;
                                else
                                    statusList[i++].status = SyncStatus.PolicyEncryptionEnforced;
                                //DontTransferOwner = true;
                            }
                            else*/
                            {
                                NodeList.Add(node);
                                statusList[i++].status = SyncStatus.Success;
                            }
					}
					else
					{
						NodeList.Add(null);
					}
				}
                if (DontTransferOwner)
                    return statusList;
				if (!CommitNonFileNodes())
				{
					i = 0;
					// If we get here the import failed try to commit the nodes one at a time.
					foreach (Node node in NodeList)
					{
						if (node == null)
							continue;
						try
						{
							collection.Commit(node);
						}
						catch (CollisionException)
						{
							// The current node failed because of a collision.
							statusList[i].status = SyncStatus.UpdateConflict;
						}
						catch (LockException)
						{
							statusList[i].status = SyncStatus.Locked;
						}
						catch
						{
							// Handle any other errors.
							statusList[i].status = SyncStatus.ServerFailure;
						}
						logger.LogAccess("Put", node.ID, collection.ID, statusList[i].status.ToString());
						i++;
					}
				}
			}
			NodeList.Clear();
			return (statusList);
		}

		/// <summary>
		/// Commit the files nodes that are changed/added
		/// </summary>
		/// <returns>True if successful, else false</returns>
		private bool CommitNonFileNodes()
		{
			try
			{
				if (NodeList.Count > 0)
				{
					collection.Commit((Node[])(NodeList.ToArray(typeof(Node))));
				}
			}
			catch
			{
				return false;
			}

			foreach (Node n in NodeList)
			{
				logger.LogAccess("Put", n.ID, collection.ID, "Success");
			}
		
			return true;
		}

		/// <summary>
		/// Add the nodes as directories
		/// </summary>
		/// <param name="nodes">Nodes array to add</param>
		/// <returns>Array of status for all nodes</returns>
		public SyncNodeStatus[] PutDirs(SyncNode [] nodes)
		{
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadWrite))
				return null;

			SyncNodeStatus[]	statusList = new SyncNodeStatus[nodes.Length];

			lock (cLock)
			{
				int i = 0;
				foreach (SyncNode snode in nodes)
				{
					string path = "";
					SyncNodeStatus status = new SyncNodeStatus();
					statusList[i++] = status;
					status.status = SyncStatus.ServerFailure;
					try
					{
						XmlDocument xNode = new XmlDocument();
						xNode.LoadXml(snode.node);
						DirNode node = (DirNode)Node.NodeFactory(store, xNode);
						log.Debug("{0}: Uploading Directory {1}", member.Name, node.Name);

						status.nodeID = node.ID;
						Import(node, snode.MasterIncarnation);
			
						// Get the old node to see if the node was renamed.
						DirNode oldNode = collection.GetNodeByID(node.ID) as DirNode;
						if (node.IsRoot)
						{
							path = oldNode.GetFullPath(collection);
						}
						else
						{
							path = node.GetFullPath(collection);
							if (oldNode != null)
							{
								// We already have this node look for a rename.
								string oldPath = oldNode.GetFullPath(collection);
								if (oldPath != path)
								{
									try
									{
										Directory.Move(oldPath, path);
									}
									catch (IOException)
									{
										// This directory has already been moved by the parent move.
									}
								}
							}
						}

						if (!Directory.Exists(path))
						{
							Directory.CreateDirectory(path);
						}
						collection.Commit(node);
						status.status = SyncStatus.Success;
					}
					catch (CollisionException)
					{
						// The current node failed because of a collision.
						status.status = SyncStatus.UpdateConflict;
					}
					catch (LockException)
					{
						status.status = SyncStatus.Locked;
					}
					catch {}
					logger.LogAccess("PutDir", path, collection.ID, status.status.ToString());
				}
			}
			return statusList;
		}

		/// <summary>
		/// Get the nodes that are not related to files
		/// </summary>
		/// <param name="nodeIDs">List of node ID's to check</param>
		/// <returns>Array of Sync Nodes</returns>
		public SyncNode[] GetNonFileNodes(string[] nodeIDs)
		{
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadOnly))
				return null;

			SyncNode[] nodes = new SyncNode[nodeIDs.Length];

			lock (cLock)
			{
				for (int i = 0; i < nodeIDs.Length; ++i)
				{
					try
					{
						Node node = collection.GetNodeByID(nodeIDs[i]);
						if (node != null)
							nodes[i] = new SyncNode(node, collection.DataMovement);
						else
						{
							nodes[i] = new SyncNode();
							nodes[i].ID = nodeIDs[i];
							nodes[i].Operation = SyncOperation.Delete;
							nodes[i].node = "";
						}
						logger.LogAccess("GetNode", node.ID + "/" + node.BaseType, collection.ID, "Success");
					}
					catch
					{
					}
				}
			}
			return nodes;
		}

        /// <summary>
        /// Get the list of Non Files nodes for a particular user
        /// </summary>
        /// <param name="nodeIDs">List of node ID's to check</param>
        /// <param name="UserID">ID of the user for whom to get list of Nodes</param>
        /// <returns>Array of sync nodes</returns>
        public SyncNode[] GetNonFileNodes(string[] nodeIDs, string UserID)
		{
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadOnly))
				return null;

			SyncNode[] nodes = new SyncNode[nodeIDs.Length];

			lock (cLock)
			{
				for (int i = 0; i < nodeIDs.Length; ++i)
				{
					try
					{
						Node node = collection.GetNodeByID(nodeIDs[i]);
						if (node != null)
						{
							//log.Debug("GetNonFileNodes2: going to form new syncnode for the client");
							nodes[i] = new SyncNode(node, UserID);
							//log.Debug("GetNonFileNodes2: new syncnode formed for the client");
							//log.Debug("GetNonFileNodes2: it is :"+nodes[i].node);
						}
						else
						{
							nodes[i] = new SyncNode();
							nodes[i].ID = nodeIDs[i];
							nodes[i].Operation = SyncOperation.Delete;
							nodes[i].node = "";
						}
						logger.LogAccess("GetNode", node.ID + "/" + node.BaseType, collection.ID, "Success");
					}
					catch
					{
					}
				}
			}
			return nodes;
		}

		/// <summary>
		/// Delete the nodes from the list
		/// </summary>
		/// <param name="nodeIDs">List of Node ID's to be deleted</param>
		/// <returns>Array of status which represents for each node ID</returns>
		public SyncNodeStatus[] DeleteNodes(string[] nodeIDs)
		{
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadWrite))
				return null;

            /// If This is a Domain collection then don't let the deletion to proceed...

            string ColType = collection.GetType().ToString();
            bool IsDomainType = ColType.Equals(PropertyTags.DomainTypeNameSpaceProperty) || ColType.Equals(PropertyTags.Domain);
            if (IsDomainType)
            {
                log.Fatal("Attempt to delete users as part of domain sync. Rejecting the delete request.");
                return null;
            }

			SyncNodeStatus[] statusArray = new SyncNodeStatus[nodeIDs.Length];
		
			lock (cLock)
			{
				int i = 0;
				foreach (string id in nodeIDs)
				{
					string name = id;
					SyncNodeStatus nStatus = new SyncNodeStatus();
					try
					{
						statusArray[i++] = nStatus;
						nStatus.nodeID = id;
						Node node = collection.GetNodeByID(id);
						if (node == null)
						{
							nStatus.status = SyncStatus.Success;
							continue;
						}

						// If this is a directory remove the directory.
						DirNode dn = node as DirNode;
						if (dn != null)
						{
							string path = dn.GetFullPath(collection);
							name = path;
							if (Directory.Exists(path))
								Directory.Delete(path, true);
							// Do a deep delete.
							Node[] deleted = collection.Delete(node, PropertyTags.Parent);
							collection.Commit(deleted);
						}
						else
						{
							// Don't remove store managed files (this is handled in the commit code).
							if ( !node.IsType( NodeTypes.StoreFileNodeType ) )
							{
								// If this is a file delete the file.
								BaseFileNode bfn = node as BaseFileNode;
								if (bfn != null)
								{
									name = bfn.GetFullPath(collection);
									SyncFile.DeleteFile(collection, bfn, name);
								}
							}
							collection.Delete(node);
							collection.Commit(node);
						}

						nStatus.status = SyncStatus.Success;
					}
					catch(LockException)
					{
						nStatus.status = SyncStatus.Locked;
					}
					catch
					{
						nStatus.status = SyncStatus.ServerFailure;
					}
					logger.LogAccess("Delete", name, collection.ID, nStatus.status.ToString());
				}
			}
			return statusArray;
		}

		/// <summary>
		/// Put the node that represents the file to the server. This call is made to begin
		/// an upload of a file.  Close must be called to cleanup resources.
		/// </summary>
		/// <param name="node">The node to put to ther server.</param>
		/// <returns>True if successful.</returns>
		public SyncStatus PutFileNode(SyncNode node)
		{
			if (!IsAccessAllowed(Access.Rights.ReadWrite))
			{
				logger.LogAccess("PutFile", node.ID, collection.ID, "Access");
				return SyncStatus.Access;
			}
			if (cLock == null) 
			{
				logger.LogAccess("PutFile", node.ID, collection.ID, "ClientError");
				return SyncStatus.ClientError;
			}

			lock (cLock)
			{
				inFile = new ServerInFile(collection, node, Policy);
				outFile = null;
				SyncStatus status = inFile.Open();
				logger.LogAccess("PutFile", inFile.Name, collection.ID, status.ToString());
				return status;
			}
		}

		/// <summary>
		/// Get the node that represents the file. This call is made to begin
		/// a download of a file.  Close must be called to cleanup resources.
		/// </summary>
		/// <param name="nodeID">The node to get.</param>
		/// <returns>The SyncNode.</returns>
		public SyncNode GetFileNode(string nodeID)
		{
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadOnly))
			{
				logger.LogAccess("GetFile", nodeID, collection.ID, "Access");
				return null;
			}

			lock (cLock)
			{
				BaseFileNode node = collection.GetNodeByID(nodeID) as BaseFileNode;
				inFile = null;
				outFile = null;
				if (node != null)
				{
					outFile = new ServerOutFile(collection, node);
					outFile.Open(sessionID);
					SyncNode sNode = new SyncNode(node);
					logger.LogAccess("GetFile", outFile.Name, collection.ID, "Success");
					return sNode;
				}
				logger.LogAccess("GetFile", nodeID, collection.ID, "DoesNotExist");
				return null;
			}
		}

		/// <summary>
		/// Get a HashMap of the file.
		/// </summary>
		/// <param name="entryCount">The number of hash entries.</param>
		/// <param name="blockSize">The size of the hashed data blocks.</param>
		/// <returns>The HashMap.</returns>
		public FileStream GetHashMap(out int entryCount, out int blockSize)
		{
			FileStream map;
			string name = "-";
			if (inFile != null)
			{
				map = inFile.GetHashMap(out entryCount, out blockSize);
				name = inFile.Name;
			}
			else
			{
				map = outFile.GetHashMap(out entryCount, out blockSize);
				name = outFile.Name;
			}

			logger.LogAccess("GetMapFile", name, collection.ID, map == null ? "DoesNotExist" : "Success");
			return map;
		}

		/// <summary>
		/// Put a HashMap of the file.
		/// </summary>
		public void PutHashMap(Stream stream, int Size)
		{
			if (inFile != null)
			{
				inFile.PutHashMap(stream, Size);
			}
			
			logger.LogAccess("PutMapFile", inFile.Name, collection.ID, "Success");
		}

		/// <summary>
		/// Write the included data to the new file.
		/// </summary>
		/// <param name="stream">The stream to write.</param>
		/// <param name="offset">The offset in the new file of where to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(Stream stream, long offset, int count)
		{
			log.Debug("InfileWrite offser {0}, count {1}", offset, count);
			inFile.WritePosition = offset;
			inFile.Write(stream, count);
			logger.LogAccessDebug("WriteFile", inFile.Name, collection.ID, "Success");
		}

		/// <summary>
		/// Copy data from the old file to the new file.
		/// </summary>
		/// <param name="oldOffset">The offset in the old (original file).</param>
		/// <param name="offset">The offset in the new file.</param>
		/// <param name="count">The number of bytes to copy.</param>
		public void Copy(long oldOffset, long offset, long count)
		{
			inFile.Copy(oldOffset, offset, count);
			logger.LogAccessDebug("CopyFile", inFile.Name, collection.ID, "Success");
		}

		/// <summary>
		/// Read data from the currently opened file.
		/// </summary>
		/// <param name="stream">Stream to read into.</param>
		/// <param name="offset">The offset to begin reading.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns>The number of bytes read.</returns>
		public int Read(Stream stream, long offset, int count)
		{
			log.Debug("OutfileRead offser {0}, count {1}", offset, count);
			outFile.ReadPosition = offset;
			int bytesRead = outFile.Read(stream, count);
			logger.LogAccessDebug("ReadFile", outFile.Name, collection.ID, "Success");
			return bytesRead;
		}

		/// <summary>
		/// Gets the read stream.
		/// </summary>
		/// <returns>The file stream.</returns>
		public StreamStream GetReadStream()
		{
			return outFile.outStream;
		}

		/// <summary>
		/// Get the WriteStream.
		/// </summary>
		/// <returns>The file stream.</returns>
		public StreamStream GetWriteStream()
		{
			return inFile.inStream;
		}

		/// <summary>
		/// Close the current file.
		/// </summary>
		/// <param name="commit">True: commit the filenode and file.
		/// False: Abort the changes.</param>
		/// <returns>The status of the sync.</returns>
		public SyncNodeStatus CloseFileNode(bool commit)
		{
			if (cLock == null)
				return null;

			lock (cLock)
			{
				SyncNodeStatus status = null;
				string name = "-";
				if (inFile != null)
				{
					status = inFile.Close(commit);
					name = inFile.Name;
				}
				else if (outFile != null)
				{
					status = outFile.Close();
					name = outFile.Name;
				}
				inFile = null;
				outFile = null;
				logger.LogAccess("CloseFile", name, collection.ID, status.status.ToString());
				return status;
			}
		}

		/// <summary>
		/// simple version string, also useful to check remoting
		/// </summary>
		public string Version
		{
			get { return "0.9.0"; }
		}
	}
}
