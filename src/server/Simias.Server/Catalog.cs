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
*                 $Author: Brady Anderson <banderso@novell.com>
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
using System.Threading;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Service;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Server
{
	/// <summary>
	/// Class to manage the creation, deletion and querying
	/// of a global collection catalog that is replicated 
	/// across all simias servers within a simias system.
	/// </summary>
	public class Catalog
	{
		#region Types

		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		//static Hashtable journals;
		private static string lockObj = "locker";
		private static EventSubscriber storeEvents;
		private static Queue eventQueue;
		private static ManualResetEvent queueEvent;
		private static Thread catThread;
		private static Thread catCreateThread;
		private static bool down = false;
		private static bool started = false;
		private static Store store;
		private static Domain domain;
		private static Collection catalog;
		public static string catalogID = "a93266fd-55de-4590-b1c7-428f2fed815d";
		internal static string catalogName = "Collection Catalogue";

		private static CollectionSyncClient syncClient;
		private static AutoResetEvent syncEvent = new AutoResetEvent( false );
		private static SimiasConnection connection;
		private static Hashtable MovingCollections = new Hashtable();

		#endregion

		#region Properties
		#endregion

		#region Constructors

        /// <summary>
        /// constructor
        /// </summary>
		static Catalog()
		{
			store = Store.GetStore();
			domain = store.GetDomain( store.DefaultDomain );

			try
			{
				catalog = store.GetCollectionByID( catalogID );
			}
			catch{}
		}
		#endregion

				static internal string SyncRoleProperty = "Sync Role"; 
				/// <summary>
				/// Set/Get the SyncRol
				/// </summary>
				public SyncRoles SyncRole
				{
					get
					{
						Collection col = store.GetCollectionByID( catalogID );
						return (col != null)?(SyncRoles)col.Properties.GetSingleProperty( SyncRoleProperty ).Value :(Simias.Sync.SyncRoles.None);
					}
					set
					{
						Collection col = store.GetCollectionByID( catalogID );
						Property hprop = new Property( SyncRoleProperty, value );
						col.Properties.ModifyProperty( hprop );
						col.Commit();
					}
				}
		#region Private Methods
        /// <summary>
        /// Event handler to handle an event
        /// </summary>
        /// <param name="args">event arguments containing details of event</param>
		static private void OnEvent( NodeEventArgs args )
		{
			// Filter out events coming from the catalog
			// collection
			if ( args.Collection == catalogID )
			{
				log.Debug( "ignoring event fired from the catalog collection" );
				return;
			}

			lock( eventQueue )
			{
				eventQueue.Enqueue( args );
				queueEvent.Set();
			}
		}

		/// <summary>
		/// Method to get this host's node ID in the catalog collection
		/// the node id is necessary to create a proxy node
		/// </summary>
		/// <returns>the host's node id from the collection catalog on the master server.</returns>
		static private string GetNodeIDFromMaster( string HostUserID )
		{
			string nodeid = null;

			try
			{
				HostLocation hostlocation = new HostLocation();
				HostNode host = HostNode.GetMaster( domain.ID );
				SimiasConnection conn = 
					new SimiasConnection( domain.ID, HostNode.GetLocalHost().UserID, SimiasConnection.AuthType.PPK, host );
				conn.Authenticate();
				conn.InitializeWebClient( hostlocation, "HostLocation.asmx" );
				log.Debug( "Master's HostLocation url {0}== {1}== {2}", hostlocation.Url, catalogID, HostUserID  );
				HostInformation hostinfo = hostlocation.GetHostInfo( catalogID, HostUserID );
				nodeid = hostinfo.ID;
			}
			catch( Exception ghnid )
			{
				log.Error( "Exception getting the remote host node id" );
				log.Error( ghnid.Message );
				log.Error( ghnid.StackTrace );
			}

			return nodeid;
		}

		/// <summary>
		/// Creates the Collection Catalog
		/// </summary>
		/// <returns>Generates an exception if an error occurs.</returns>
		static private void CreateCollectionCatalog()
		{
			if ( domain != null )
			{
				// The collection catalog is only created on the master server
				// but replicated and updated on slaves

				catalog = store.GetCollectionByID( catalogID );
				if ( catalog == null )
				{
					if ( domain.Role == SyncRoles.Master )
					{
						Collection tempCatalog;
						ArrayList nodes = new ArrayList();

						// Create the collection catalog.
						tempCatalog = new Collection( store, catalogName, catalogID, domain.ID );
						tempCatalog.SetType( tempCatalog, "Catalog" );
						nodes.Add( tempCatalog );

						// All host nodes must be added as members to the collection
						Member member;
						HostNode[] hosts = Simias.Storage.HostNode.GetHosts( domain.ID );
						foreach( HostNode host in hosts )
						{
							member = new Member( host.Name, host.UserID, ( host.IsMasterHost == true ) ? Access.Rights.Admin : Access.Rights.ReadWrite );
							tempCatalog.SetType( member, "Host" );
							nodes.Add( member );
						}

						// Add the admin user for the domain as the owner.
						member = new Member( domain.Owner.Name, domain.Owner.UserID, Access.Rights.Admin );
						member.IsOwner = true;	
						nodes.Add( member );

						// Commit the changes.
						tempCatalog.Commit( nodes.ToArray( typeof( Node ) ) as Node[] );
						catalog = tempCatalog;
					}
				}
			}
		}

		/// <summary>
		/// Method to create the proxy catalog collection.
		/// The proxy catalog collection cannot be created
		/// until the slave server has successfully downloaded
		/// the domain from the master server.
		/// This method is called in a new thread and will
		/// continually poll for the master host in the
		/// domain.  Once the master host is known the 
		/// catalog proxy is created and the thread will 
		/// terminate.
		/// </summary>
		/// <returns>N/A.</returns>
		static private void CreateCatalogProxy()
		{
			while( catalog == null && down == false )
			{
				log.Debug( "Attempting to create the proxy catalog" );
				if((Simias.Sync.CollectionSyncClient.SyncStateMap & Simias.Sync.CollectionSyncClient.StateMap.DomainSyncOnce) != Simias.Sync.CollectionSyncClient.StateMap.DomainSyncOnce )
				{
					syncEvent.WaitOne( 5000, false );
					continue;
				}

				// First call the master server to retreive the node id
				// of this host's membership in the catalog collection
				HostNode host = HostNode.GetLocalHost();
				string hostnodeid = GetNodeIDFromMaster( host.UserID );
				if ( hostnodeid != null )
				{
					try
					{
						// Create a proxy catalog collection
						Collection proxyCatalog;
						ArrayList nodes = new ArrayList();

						proxyCatalog = new Collection( store, catalogName, catalogID, domain.ID );
						proxyCatalog.Proxy = true;
						proxyCatalog.SetType( proxyCatalog, "Catalog" );
						proxyCatalog.Role = Simias.Sync.SyncRoles.Slave;

						// Get the Host ID from the domain
						Property hostid = domain.Properties.GetSingleProperty( PropertyTags.HostID );
						Property hid = new Property( PropertyTags.HostID, hostid.Value );
						proxyCatalog.HostID = hid.Value as string;
						nodes.Add( proxyCatalog );

						// Add the local host as a proxy member of the collection
						Member member = new Member( host.Name, hostnodeid, host.UserID, Access.Rights.Admin, null );
						member.IsOwner = true;
						member.Proxy = true;
						proxyCatalog.SetType( member, "Host" );
						nodes.Add( member );

						// Commit the changes.
						proxyCatalog.Commit( nodes.ToArray( typeof( Node ) ) as Node[] );
						catalog = proxyCatalog;
					}
					catch( Exception ccp )
					{
						log.Error( "Exception creating proxy catalog collection" );
						log.Error( ccp.Message );
						log.Error( ccp.StackTrace );
					}
				}
				else
				{
					syncEvent.WaitOne( 30000, false );
				}
			}

			Thread scanThread = new Thread( new ThreadStart( Catalog.ScanCollections ) );
			scanThread.IsBackground = true;
			scanThread.Start();
		}

		/// <summary>
		/// Process any store events that have been placed
		/// on the queue.
		/// </summary>
		/// <returns>N/A - returns when local object down == true.</returns>
		static private void ProcessEvents()
		{
		        //In Slaves ,The first time.. Wait till the catalog is created.
			while ( catalog == null )
			    Thread.Sleep (3 * 1000); //Wait For 3 seconds.

			while( down == false )
			{
				// Wait for something to be added to the queue.
				queueEvent.WaitOne();

				// Now loop until the queue is emptied.
				while( true )
				{
					NodeEventArgs args;
					lock( eventQueue )
					{
						if ( eventQueue.Count == 0 )
						{
							queueEvent.Reset();
							break;
						}

						args = eventQueue.Dequeue() as NodeEventArgs;
					}

					if ( args.Collection.Equals( catalogID ) == true )
					{
						log.Debug( "Received an event generated within the catalog collection - ignoring" );
						continue;
					}
					if( MovingCollections != null)
					{
						if( MovingCollections.ContainsKey(args.Collection))
						{
							log.Debug("This is a moving collection: {0}", args.Collection);
							continue;
						}
					}
					try
					{
						Store st = Store.GetStore();
						Collection col1 = st.GetCollectionByID(args.Collection);
						if( col1 != null && col1.DataMovement == true)
						{
							log.Debug("The collection {0} data move is set.Ignoring the event..", args.Collection);
							continue;
						}
					}
					catch(Exception ex)
					{
						log.Debug(" Exception in reading the collection {0} in catalog: {1}--{2}", args.Collection, ex.Message, ex.StackTrace);
					}
					// Process the event.
					if ( down == false )
					{
						if ( args.Type.Equals( NodeTypes.CollectionType ) &&
								args.EventType.Equals( EventType.NodeCreated ) )
						{
							try
							{
								CatalogEntry entry = Catalog.GetEntryByCollectionID( args.Collection );
								if ( entry == null )
								{
									entry = new CatalogEntry( args.Collection );
									catalog.Commit( entry );
								}
								log.Info( "Collection: {0} created", args.Collection );
							}
							catch( Exception cnc )
							{
								log.Error( "Exception in Collection Create event" );
								log.Error( cnc.Message );
								log.Error( cnc.StackTrace );
							}
						}
						else if ( args.Type.Equals( NodeTypes.CollectionType ) &&
									args.EventType.Equals( EventType.NodeChanged ) )
						{
							log.Debug( "Collection: {0} Updated", args.Collection );
							try
							{
								CatalogEntry entry = Catalog.GetEntryByCollectionID( args.Collection );
								log.Debug( "Updating Catalog Entry {0}", entry.ID );
								entry.SetCollSizeByCollectionID( args.Collection );
							}
							catch{}
						}
						else if ( args.Type.Equals( NodeTypes.CollectionType ) &&
									args.EventType.Equals( EventType.NodeDeleted ) )
						{
							log.Debug( "Collection: {0} deleted", args.Collection );
							try
							{
								CatalogEntry entry = Catalog.GetEntryByCollectionID( args.Collection );
								log.Debug( "Deleting Catalog Entry {0}", entry.ID );
								catalog.Commit( catalog.Delete( entry ) );
							}
							catch{}
						}
						else if ( args.Type.Equals( NodeTypes.MemberType ) &&
									args.EventType.Equals( EventType.NodeCreated ) )
						{
							CatalogEntry entry = Catalog.GetEntryByCollectionID( args.Collection );
							if ( entry != null )
							{
								try
								{
									Collection col = store.GetCollectionByID( args.Collection );
									Member member = new Member( col.GetNodeByID( args.Node ) );
									if ( entry.AddMember( member.UserID, args.Node ) == true )
									{
									        if (col.GetMemberByID(member.UserID).IsOwner)
									        {
										    entry.AddOwner (member.UserID);
										    log.Debug( "OwnerID {0} added to collection {1}", member.UserID, col.ID );
									        }
										log.Debug( "Member {0} added to collection {1}", member.UserID, col.ID );
									}
								}
								catch( Exception mc )
								{
									log.Error( "Exception in Member Create event" );
									log.Error( mc.Message );
									log.Error( mc.StackTrace );
								}
							}

							// Check if the event was a Host created in the default domain
							// if so we want to add him as a member to the catalog collection
							if ( domain.Role == Simias.Sync.SyncRoles.Master && args.Collection == domain.ID )
							{
								try
								{
									Node node = domain.GetNodeByID( args.Node );
									if ( node != null && node.IsType( "Host" ) == true )
									{
										UpdateHostList();
									}
								}
								catch{}
							}
						}
						else if ( args.Type.Equals( NodeTypes.MemberType ) &&
									args.EventType.Equals( EventType.NodeDeleted ) )
						{
							log.Debug( "Member {0} deleted from collection {0}", args.Node, args.Collection );

							try
							{
	
								// Get all catalog entries the user is a member of then
								// delete the mids property for each entry

								CatalogEntry entry;
								Property midsProp = new Property( CatalogEntry.MemberProperty, args.Node );
								ICSList nodes = store.GetNodesByProperty( midsProp, SearchOp.Ends );
								foreach( ShallowNode sn in nodes )
								{
									entry = new CatalogEntry( sn );
									
									log.Debug( "removing mid from entry {0}", entry.ID );
									entry.RemoveMember( args.Node );
								}
							}
							catch( Exception md )
							{
								log.Error( "Exception in Delete Member event" );
								log.Error( md.Message );
								log.Error( md.StackTrace );
							}
						}
						else if ( args.Type.Equals( NodeTypes.MemberType ) &&
									args.EventType.Equals( EventType.NodeChanged ) )
						{
							log.Debug( "Member {0} modified in collection {1}", args.Node, args.Collection );
							CatalogEntry entry = Catalog.GetEntryByCollectionID( args.Collection );

							try
							{
								Collection col = store.GetCollectionByID( args.Collection );
								Member member = new Member( col.GetNodeByID( args.Node ) );

							        if (entry != null && member != null && col.GetMemberByID(member.UserID).IsOwner)
							        {
									Property p = col.Properties.GetSingleProperty( "OrphanedOwner" );
									if( p != null)
									{
										String OrphOwner = p.Value as String;
										entry.AddOrphanedOwner(OrphOwner );	
									}
									else
									{
										entry.RemoveOrphanedOwner();
									}
								       entry.AddOwner (member.UserID);
								       log.Debug( "Owner Change : OwnerID {0} added to collection {1}", member.UserID, col.ID );
							        }
							}
							catch( Exception md )
							{
								log.Error( "Exception in Modify Member event" );
								log.Error( md.Message );
								log.Error( md.StackTrace );
							}
						}

					}
					else
					{
						log.Info( "Lost event for node ID = '{0}'. Event type = '{1}'. Node type = '{2}'", args.Node, args.EventType, args.Type );
					}
				}
			}
		}

		/// <summary>
		/// Method to update the collection catalog with
		/// any collections that may have been missed via
		/// the event system.
		/// </summary>
		/// <returns>N/A </returns>
		static private void ScanCollections()
		{
			if ( catalog != null )
			{
				log.Debug( "Starting collection scan..." );

				CatalogEntry catentry;
				ICSList collections = store.GetCollectionsByDomain( domain.ID );
				foreach( ShallowNode sn in collections )
				{
					// service stopping?
					if ( down == true )
					{
						break;
					}

					try
					{
						if ( sn.IsBaseType( NodeTypes.DomainType ) == false &&
							sn.IsBaseType( NodeTypes.POBoxType) == false &&
							sn.ID != catalogID )
						{
							catentry = GetEntryByCollectionID( sn.ID );
							if ( catentry == null )
							{
								log.Debug( "creating catalog entry for {0}", sn.Name );
								catentry = new CatalogEntry( sn.ID, sn.Name );
								Collection col = new Collection( store, sn );
								ICSList members = col.GetMemberList();
								foreach( ShallowNode msn in members )
								{
									Member member = new Member( col, msn );
									log.Debug("Adding user {0} for the collection", member.Name);
									if (member.IsOwner)
									{
									    catentry.AddOwner ( member.UserID );
									    // Note : Required during upgrade (3.2 - 3.6) .
									    log.Debug( "OwnerID {0} added  at pos 1 to collection {1}", member.UserID, col.ID );
									    if ( member.HomeServer == null )
									    {
										member.HomeServer = HostNode.GetLocalHost();
										domain.Commit(member);
									    }
									}
									    
									bool retval = catentry.AddMember( member.UserID, member.ID );
									log.Debug( "Member {0} added at pos1 to collection {1}. return value: {2}", member.UserID, col.ID, retval );
								}
								catalog.Commit( catentry );
							}
							else
							{
								log.Debug( "catalog entry exists for {0}", sn.Name );
							}
						}
					}
					catch( Exception sc )
					{
						log.Error( "Exception scanning collections" );
						log.Error( sc.Message );
						log.Error( sc.StackTrace );
					}
				}
				log.Debug( "Collection scan finished" );
			}
		}

        /// <summary>
        /// sync the catalog with server
        /// </summary>
		static private void SyncCatalog()
		{
			int retry = 10;
			HostNode host = null;
			while( down == false )
			{
				if ( catalog == null )
				{
					syncEvent.WaitOne( 10000, false );
					continue;
				}

				try
				{
					host = HostNode.GetLocalHost();
					log.Debug( "Creating a simias connection as {0}", host.Name );
					connection = new SimiasConnection( domain.ID, host.UserID, SimiasConnection.AuthType.PPK, domain );

					// We need to get a one time password to use to authenticate.
					connection.Authenticate();
					break;
				}
				catch( Exception sc )
				{
					log.Error( "Exception creating a SimiasConnection as {0} to {1}", host.Name, domain.Name );
					log.Error( sc.Message );
					log.Error( sc.StackTrace );
				
					Thread.Sleep( 10000 );
					if ( --retry <= 0 )
					{
						break;
					}
				}
			}
			
			syncClient = new CollectionSyncClient( catalog.ID, new TimerCallback( TimerFired ) );
			while ( true )
			{
				syncEvent.WaitOne();
				if ( down == true )
				{
					break;
				}
				retry = 3;
				while(retry-- > 0)
				{
					try
					{
                                        	Simias.Sync.CollectionSyncClient.SyncStateMap  &= ~Simias.Sync.CollectionSyncClient.StateMap.CatalogSyncFinished;
						Simias.Sync.CollectionSyncClient.SyncStateMap  |= Simias.Sync.CollectionSyncClient.StateMap.CatalogSyncStarted;
						log.Debug("About to Start Catalog Sync");
						syncClient.SyncNow();
						log.Debug("Catalog Sync Completed");
						Simias.Sync.CollectionSyncClient.SyncStateMap  &= ~Simias.Sync.CollectionSyncClient.StateMap.CatalogSyncStarted;
						Simias.Sync.CollectionSyncClient.SyncStateMap  |= Simias.Sync.CollectionSyncClient.StateMap.CatalogSyncFinished;
						if(domain.SystemSyncStatus == 0)
						{
							Simias.Sync.CollectionSyncClient.SyncStateMap  |= Simias.Sync.CollectionSyncClient.StateMap.CatalogSyncOnce;
							domain.SystemSyncStatus = (ulong)Simias.Sync.CollectionSyncClient.StateMap.CatalogSyncOnce;
							domain.Commit();
							log.Debug( "System Sync Status Value is set to {0}", domain.SystemSyncStatus.ToString() );
						}
						break;
					}
                                        catch (Exception ex){
                                                log.Debug("Catalog Sync, got an exception.{0} {1}", ex.Message, ex.StackTrace);
						Thread.Sleep( 30 * 1000 );
                                                log.Debug("Catalog Sync, Possibly connection lost. Setting simias connection again");
                                                try
                                                {
                                                        connection.ClearConnection();
							host = HostNode.GetLocalHost();
							connection = new SimiasConnection( domain.ID, host.UserID, SimiasConnection.AuthType.PPK, domain );

							connection.Authenticate();
                                                }
                                                catch (Exception SimiasConnEx){
                                                        log.Debug("Catalog Sync, second Simias connection got exception.{0} {1}", SimiasConnEx.Message, SimiasConnEx.StackTrace);
                                                }
                                                log.Debug("Catalog Sync, Simias authenticate for Catalog Sync Connection successful....");

                                        }
				}

				if ( down == true )
				{
					break;
				}

				syncClient.Reschedule( true, 30 );
			}
		}

        /// <summary>
        /// Adds the collection into MovingCollection hashtable
        /// </summary>
        /// <param name="collectionID">collection id</param>
        /// <param name="status">not used inthis function</param>
		public static void AddCollectionForMovement(string collectionID, bool status)
		{
			if( MovingCollections == null)
				MovingCollections = new Hashtable();
			if( MovingCollections.ContainsKey(collectionID))
				return;
			else
				MovingCollections[collectionID] = "Started";
		}

        /// <summary>
        /// remove the collectionID from hashtable
        /// </summary>
        /// <param name="collectionID"></param>
		public static void RemoveCollectionForMovement(string collectionID)
		{
			if( MovingCollections == null)
				return;
			if( MovingCollections.ContainsKey(collectionID))
				MovingCollections.Remove(collectionID);
		}

		public static void RecreateEntryForCollection(string collectionID)
		{
			log.Debug("Entered RecreateEntryForCollection {0}", collectionID);
			try
			{
				DeleteCatalogEntryByCollectionID(collectionID);
				Collection col = store.GetCollectionByID(collectionID);
				if( col == null)
				{
					log.Debug("Cannot create the catalogentry as collection is null.");
					return;
				}
				CatalogEntry catentry = new CatalogEntry( col.ID, col.Name );
                                ICSList members = col.GetMemberList();
                                foreach( ShallowNode msn in members )
                                {
	                                Member member = new Member( col, msn );
        	                        log.Debug("Adding user {0} for the collection", member.Name);
                	                if (member.IsOwner)
                        	        {
                                		catentry.AddOwner ( member.UserID );
		                                // Note : Required during upgrade (3.2 - 3.6) .
                		                log.Debug( "OwnerID {0} added  at pos 2 to collection {1}:{2}", member.UserID, col.ID, col.Name );
                                		if ( member.HomeServer == null )
		                                {
                			                member.HomeServer = HostNode.GetLocalHost();
			                                domain.Commit(member);
                        		        }
	                                }
        	                        bool retval = catentry.AddMember( member.UserID, member.ID );
                	                log.Debug( "Member {0} added at pos1 to collection {1}. return value: {2}", member.UserID, col.ID, retval );
                                }
                                catalog.Commit( catentry );
				log.Debug("Out of RecreateEntryForCollection. Added catalog entry: {0}", catentry.ID);
			}
			catch(Exception ex)
			{
				log.Debug("RecreateEntryForCollection: Exception: {0}--{1}", ex.Message, ex.StackTrace);
			}
		}

		/// <summary>
		/// Called by The CollectionSyncClient when it is time to run another sync pass.
		/// </summary>
		/// <param name="collectionClient">The client that is ready to sync.</param>
		internal static void TimerFired( object collectionClient )
		{
			while(CollectionSyncClient.running || ((Simias.Sync.CollectionSyncClient.SyncStateMap & Simias.Sync.CollectionSyncClient.StateMap.DomainSyncStarted ) == Simias.Sync.CollectionSyncClient.StateMap.DomainSyncStarted))
				Thread.Sleep(1000);
			syncEvent.Set();
		}

		/// <summary>
		/// Method to guarantee that all hosts are
		/// members of the Catalog collection.
		/// </summary>
		static private void UpdateHostList()
		{
			if ( domain.Role == Simias.Sync.SyncRoles.Master )
			{
				try
				{
					log.Debug( "Checking for new hosts" );
					HostNode[] hosts = Simias.Storage.HostNode.GetHosts( domain.ID );
					foreach( HostNode host in hosts )
					{
						Member member = catalog.GetMemberByID( host.UserID );
						if ( member == null )
						{
							member = new Member( host.Name, host.UserID, Access.Rights.ReadWrite );
							catalog.SetType( member, "Host" );
							catalog.Commit( member );
							log.Debug( "Added host {0} to the catalog collection member list", member.Name );
						}
					}
				}
				catch( Exception uhl )
				{
					log.Error( uhl.Message );
					log.Error( uhl.StackTrace );
				}
			}
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Called to start the catalog service.
		/// </summary>
		static internal bool StartCatalogService()
		{
			Thread scanThread;

			lock( lockObj )
			{
				if ( started == true )
				{
					log.Info( "StartCatalogService failed because the service is already running." );
					return false;
				}

				down = false;

				// The collection catalog is only created on the master server
				// but replicated and updated on slaves
				catalog = store.GetCollectionByID( catalogID );
				if ( catalog == null )
				{
					if ( domain.Role == SyncRoles.Master )
					{
						CreateCollectionCatalog();
						UpdateHostList();

						scanThread = new Thread( new ThreadStart( ScanCollections ) );
						scanThread.Priority = ThreadPriority.BelowNormal;
						scanThread.IsBackground = true;
						scanThread.Start();
					}
					else
					{
						// The proxy catalog cannot be created until the domain from
						// master has synchronized to this slave.  That said, the
						// proxy collection is created in the background once the 
						// domain information is in sync.
						catCreateThread = new Thread( new ThreadStart( CreateCatalogProxy ) );
						catCreateThread.Priority = ThreadPriority.BelowNormal;
						catCreateThread.IsBackground = true;
						catCreateThread.Start();
					}
				}
				else
				{
					scanThread = new Thread( new ThreadStart( ScanCollections ) );
					scanThread.Priority = ThreadPriority.BelowNormal;
					scanThread.IsBackground = true;
					scanThread.Start();
				}

				eventQueue = new Queue();
				queueEvent = new ManualResetEvent( false );

				// Setup an event handler to subscribe to all store events
				storeEvents = new EventSubscriber();
				storeEvents.NodeChanged += new NodeEventHandler( OnEvent );
				storeEvents.NodeCreated += new NodeEventHandler( OnEvent );
				storeEvents.NodeDeleted += new NodeEventHandler( OnEvent );

				// Create a thread to process queued events
				catThread = new Thread( new ThreadStart( ProcessEvents ) );
				catThread.Priority = ThreadPriority.BelowNormal;
				catThread.IsBackground = true;
				catThread.Start();

				if ( domain.Role == Simias.Sync.SyncRoles.Slave )
				{
					// Now start the sync process for the catalog collection.
					Thread syncThread = new Thread( new ThreadStart( SyncCatalog ) );
					syncThread.Priority = ThreadPriority.BelowNormal;
					syncThread.Name = "Catalog Thread";
					syncThread.IsBackground = true;
					syncThread.Start();
				}

				log.Debug( "Collection Catalog service started..." );
				started = true;
			}

			return true;
		}

		/// <summary>
		/// Called to stop the catalog service.
		/// </summary>
		static internal void StopCatalogService()
		{
			down = true;
			queueEvent.Set();
			catThread.Join();
			storeEvents.Dispose();
			Thread.Sleep( 0 );
			syncEvent.Set();
			Thread.Sleep( 0 );
			started = false;
			log.Debug( "Collection Catalog Service stopped..." );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Method to retrieve all collection IDs the specified user
		/// is a member of.
		/// </summary>
        /// <param name="UserID">userid sought for</param>
        /// <returns>string array containing ids of collection</returns>
		static public string[] GetAllCollectionIDsByUserID( string UserID )
		{
			CatalogEntry entry = null;
			Property midsProp = new Property( CatalogEntry.MemberProperty, UserID );
			ICSList nodes = store.GetNodesByProperty( midsProp, SearchOp.Begins );
			string[] collectionids = new string[ nodes.Count ];
			int x = 0;
			
			foreach( ShallowNode sn in nodes )
			{
				entry = new CatalogEntry( sn );
				collectionids[ x++ ] = entry.CollectionID;
			}

			return collectionids;
		}

		/// <summary>
		/// Method to retrieve all catalog entry IDs the specified user
		/// is a member of.
		/// </summary>
        	/// <param name="UserID">userid</param>
        	/// <returns>catalog entryIDs for this userid</returns>
		static public CatalogEntry[] GetAllEntryIDsByUserID( string UserID )
		{
			Property midsProp = new Property( CatalogEntry.MemberProperty, UserID );
			ICSList nodes = store.GetNodesByProperty( midsProp, SearchOp.Begins );
			ArrayList entries = new ArrayList();
			foreach( ShallowNode sn in nodes )
			{
				entries.Add( new CatalogEntry( sn ) );
			}
			return entries.ToArray( typeof( CatalogEntry ) ) as CatalogEntry[];	
		}

		/// <summary>
		/// Method to retrieve all catalog entries the specified user
		/// is a member of.
		/// </summary> 
        	/// <param name="UserID">userid</param>
        	/// <returns>catalog entry arrray for this userid</returns>
		static public CatalogEntry[] GetAllEntriesByUserID( string UserID )
		{
			ArrayList entries = new ArrayList();
			if(Simias.Service.Manager.LdapServiceEnabled == true)
			{
				string[] IDs = domain.GetMemberFamilyList(UserID);
				foreach(string id in IDs)
				{
					Property midsProp = new Property( CatalogEntry.MemberProperty, id);
					ICSList nodes = store.GetNodesByProperty( midsProp, SearchOp.Begins );
					foreach( ShallowNode sn in nodes )
					{
						CatalogEntry cEnt = new CatalogEntry( sn );
						if(id == UserID || cEnt.isSharedWithUser(UserID) == false)
						entries.Add( cEnt );
					}
					
				}
			}
			else
			{
				Property midsProp = new Property( CatalogEntry.MemberProperty, UserID );
				ICSList nodes = store.GetNodesByProperty( midsProp, SearchOp.Begins );
				foreach( ShallowNode sn in nodes )
				{
					entries.Add( new CatalogEntry( sn ) );
				}
			}

			return entries.ToArray( typeof( CatalogEntry ) ) as CatalogEntry[];
		}	

		
		/// <summary>
		/// Get the total disc space used by an User 
		/// </summary> 
		static public long GetUsedSpaceOfUserID( string UserID )
		{
			long tempSize = 0;
			CatalogEntry[] catalogEntries = GetAllEntriesByUserID(UserID);
			foreach( CatalogEntry catEntry in catalogEntries )
			{
				tempSize += catEntry.CollectionSize;
			}
			return tempSize;
		}

		/// <summary>
		/// Method to retrieve all catalog entries the specified user
		/// is a owner of.
		/// </summary> 
		static public CatalogEntry[] GetAllEntriesByOwnerID( string ownerID )
		{
			ArrayList entries = new ArrayList();
			Property midsProp = new Property( CatalogEntry.OwnerProperty, ownerID );
			ICSList nodes = store.GetNodesByProperty( midsProp, SearchOp.Begins );
			foreach( ShallowNode sn in nodes )
			{
				entries.Add( new CatalogEntry( sn ) );
			}

			return entries.ToArray( typeof( CatalogEntry ) ) as CatalogEntry[];
		}	

		/// <summary>
		/// Method to retrieve all catalog entries the specified user
		/// is a owner of.
		/// </summary> 
		static public CatalogEntry[] GetAllOwnedEntriesByGroupMembers( string groupID )
		{
			ArrayList entries = new ArrayList();
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			string [] GroupMembers = domain.GetGroupsMemberList(groupID);
			foreach(string memberID in GroupMembers)
			{
				Property midsProp = new Property( CatalogEntry.OwnerProperty, memberID );
				ICSList nodes = store.GetNodesByProperty( midsProp, SearchOp.Begins );
				foreach( ShallowNode sn in nodes )
				{
					entries.Add( new CatalogEntry( sn ) );
				}
			}

			return entries.ToArray( typeof( CatalogEntry ) ) as CatalogEntry[];
		}	

		/// <summary>
		/// Method to retrieve size of all catalog entries owned by all members of the group 
		/// </summary> 
		static public long GetSpaceUsedByGroupMembers( string groupID )
		{
			long SpaceUsed = 0;

			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			string [] GroupMembers = domain.GetGroupsMemberList(groupID);
			foreach(string memberID in GroupMembers)
			{
				Property midsProp = new Property( CatalogEntry.OwnerProperty, memberID );
				ICSList nodes = store.GetNodesByProperty( midsProp, SearchOp.Begins );
				foreach( ShallowNode sn in nodes )
				{
					CatalogEntry CatEntry = new CatalogEntry( sn );
					SpaceUsed += CatEntry.CollectionSize;
				}
			}

			return SpaceUsed;
		}	

		/// <summary>
		/// Method to retrieve all catalog entries the specified user
		/// is a member of.
		/// </summary> 
		static public CatalogEntry ConvertToCataloEntry( ShallowNode sn )
		{
			CatalogEntry CatEntry = new CatalogEntry( sn );
			return CatEntry;
		}

		/// <summary>
		/// Method to retrieve all catalog entries the specified user
		/// is a member of.
		/// </summary> 
		static public CatalogEntry[] GetAllEntriesByGroupAdminID( string UserID, string name, SearchOp searchOp , int index, int max, out int total)
		{
			ArrayList entries = new ArrayList();
			ArrayList sortList = new ArrayList();
			int i = 0;
			Hashtable UniqueObjectsHashTable = new Hashtable();
			Member groupadmin = domain.GetMemberByID(UserID);
			Hashtable htForMonitoredUsers = groupadmin.GetMonitoredUsers(true);
			if(! htForMonitoredUsers.ContainsKey(UserID))
			{
				htForMonitoredUsers.Add(UserID,"" );
			}
			string[] MonitoredUsers = new string[htForMonitoredUsers.Count];
			htForMonitoredUsers.Keys.CopyTo(MonitoredUsers, 0);
			foreach(string groupMember in MonitoredUsers)
			{
				CatalogEntry[] catUserEntries = GetAllEntriesByOwnerID(groupMember);
				foreach(CatalogEntry catUserEntry in catUserEntries)
				{
					if(!UniqueObjectsHashTable.ContainsKey(catUserEntry.CollectionID))
					{
						UniqueObjectsHashTable.Add(catUserEntry.CollectionID,"");
						sortList.Add(catUserEntry);
					}
				}
			}
			sortList.Sort();
			total = sortList.Count;
			foreach(CatalogEntry cEntry in sortList)
			{
				if (max == 0 || ((i >= index) && (i < (max + index))))
					entries.Add(cEntry);
				i++;
			}
			return entries.ToArray( typeof( CatalogEntry ) ) as CatalogEntry[];
		}

		/// <summary>
		/// Implements Search for catalog
		/// </summary> 
        	/// <returns>catalog Search Result linst</returns>
		static public ICSList Search(Simias.Storage.SearchPropertyList SearchPrpList)
		{
			return catalog.Search( SearchPrpList );
		}

		/// <summary>
		/// Get all the entries in the catalogue
		/// </summary> 
        /// <param name="name">entryname</param>
        /// <param name="searchOp">search operation e.g. equals,contains etc</param>
        /// <returns>catalog entry array</returns>
		static public ICSList GetAllEntriesByName (string name, SearchOp searchOp)
		{
                        Simias.Storage.SearchPropertyList SearchPrpList = new Simias.Storage.SearchPropertyList();

                        SearchPrpList.Add(BaseSchema.ObjectName, name, searchOp);
                        SearchPrpList.Add(BaseSchema.ObjectType, NodeTypes.MemberType, SearchOp.Not_Equal);
                        SearchPrpList.Add(BaseSchema.ObjectType, NodeTypes.StoreFileNodeType, SearchOp.Not_Equal);
                        SearchPrpList.Add(BaseSchema.ObjectType, NodeTypes.CollectionType, SearchOp.Not_Equal);
			ICSList nodes = catalog.Search( SearchPrpList );
			return nodes;
		}

		/// <summary>
		/// Get a catalog entry for the specified collection
		/// </summary>
        /// <returns>catalog entry</returns>
		static public CatalogEntry GetEntryByCollectionID( string CollectionID )
		{
			CatalogEntry entry = null;

			Property colProp = new Property( CatalogEntry.CollectionProperty, CollectionID );
			ICSList nodes = store.GetNodesByProperty( colProp, SearchOp.Equal );
			foreach( ShallowNode sn in nodes )
			{
				entry = new CatalogEntry( sn );
				break;
			}

			return entry;
		}

        /// <summary>
        /// set the hostid property for this collection
        /// </summary>
        /// <param name="collectionID">collection id</param>
        /// <param name="hostID">host id to set</param>
		static public void SetHostForCollection( string collectionID, string hostID)
		{
			CatalogEntry entry = null;
			log.Debug("SetHostForCollection: hostid: {0}", hostID);
			try
			{
				Property colProp = new Property( CatalogEntry.CollectionProperty, collectionID );
				ICSList nodes = store.GetNodesByProperty( colProp, SearchOp.Equal );
				if( nodes == null )
				{
					log.Debug("SetHostForCollection: Returned null");
					return;
				}
				foreach( ShallowNode sn in nodes )
				{
					entry = new CatalogEntry( sn );
					if( entry.HostID  != hostID)
					{
						log.Debug("Adding hostid for old node. {0}", entry.ID);
						if( catalog.GetNodeByID(entry.ID) != null)
						{
							entry.SetHostID(hostID, true);
						}
					}
					else
						log.Debug("the new entry {0} is synced", entry.ID);
				}
			}
			catch(Exception ex)
			{
				log.Debug("Exception while setting the host id for catalog: {0}, StackTrace is {1}", collectionID,ex.StackTrace);
			}
		}

                /// <summary>
                /// Delete a catalog entry for the specified collection
                /// </summary>
                static public void DeleteCatalogEntryByCollectionID( string CollectionID )
                {
                        CatalogEntry entry = GetEntryByCollectionID(CollectionID);
			if(entry != null)
			{
				log.Debug("DeleteCatalogEntryByCollectionID Deleting --{0}--{1}---" ,entry.ID, CollectionID);
				catalog.Commit(catalog.Delete(entry));
			}
                        return ;
                }

		/// <summary>
                /// Delete a catalog entry for the slave host that is to be removed
                /// </summary>
                static public void DeleteSlaveEntryFromCatalog(string name )
                {
			log.Debug(" Deleting catalog entry of slave server - {0}", name);

			ICSList list = catalog.FindType(NodeTypes.MemberType);
			Node node = null;
			foreach( ShallowNode sn in list )
			{
				if( sn != null)
				{
					if(sn.Name.Equals(name))
					{
						node = catalog.GetNodeByID(sn.ID); 
						if( node != null && node.IsType("Host"))
						{
							log.Debug("Delting the node from catalog {0} --- {1}",node.Name, node.ID);
						
							CatalogEntry entry = new CatalogEntry(sn);
							if( entry != null )
							{
								catalog.Commit(catalog.Delete(entry));
								log.Debug(" Deleted catalog entry of slave server - {0}", name);
							}
							else
							{
								log.Debug("Unable to delete the catalog Entry for slave server {0}",name);
							}
							break;
						}
					}
				}
			}
                        return;
                }

                /// <summary>
                /// Delete a catalog entry for the specified collection
                /// </summary>
                static public void DeleteEntryByCollectionID( string CollectionID )
				{
					log.Debug("In DeleteEntryByCollectionID ...");
					Collection c = store.GetCollectionByID(CollectionID);
					CatalogEntry entry = GetEntryByCollectionID(CollectionID);
					if(entry != null)
					{
						catalog.Commit(catalog.Delete(entry));
						c.Commit(c.Delete());
					} 
					log.Debug("Out of DeleteEntryByCollectionID ...");
					return ;
				}

		#endregion
	}

	/// <summary>
	/// Catalog Entry
	/// A catalog entry is a node that belongs to the system
	/// catalog collection that replicates across all servers
	/// in a multi-server ifolder system.
	/// There should be one catalog entry for each collection
	/// in the system which makes it possible to find any collection
	/// from any host in the system.
	/// The three distinguishing properties on a CatalogEntry are:
	///   HostID - Host where the associated collection lives
	///   CollectionID - Associated Collection ID
	///   UserIDs - List of members for the associated collection.
	/// </summary>
	public class CatalogEntry : Node
	{
		#region Types
		static private Store store;
		static private Collection catalog;
		static private string localhostID;

		// Distinguishing properties for a node
		// to be a CatalogEntry
		static internal string CollectionProperty = "cid";
		static internal string HostProperty = "hid";
		static internal string MemberProperty = "mid";
		static internal string OwnerProperty = "oid";
		static internal string SizeProperty = "size";
		static internal string DisabledProperty = "Disabled";
		static internal string OrphanedOwnerProperty = "OrphOwnerDN";

		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
		#endregion

		#region Properties
		/// <summary>
		/// Returns the actual collection this entry relates to
		/// </summary>
		public string CollectionID
		{
			get
			{
				return this.Properties.GetSingleProperty( CollectionProperty ).Value as string;
 			}
		}

		/// <summary>
		/// Returns the HostID where this collection is located
		/// </summary>
		public string HostID
		{
			get
			{
				return this.Properties.GetSingleProperty( HostProperty ).Value as string;
			}
			set
			{
				log.Debug("Changing the host ID to: {0}", value);
				Property hprop = new Property( HostProperty, value );
	            this.Properties.ModifyProperty( hprop );
				catalog.Commit(this);
			}
		}

		/// <summary>
		/// Returns the size of the data in this collection
		/// </summary>
		public long CollectionSize
		{
			get
			{
				Property p = this.Properties.GetSingleProperty( SizeProperty );
				return ( p != null ) ? ( long )p.Value : 0;
 			}
			set
			{
			        Property CollectionSizeProp = new Property( SizeProperty, value );
			        this.Properties.ModifyProperty( CollectionSizeProp );
			}
		}

		/// <summary>
		/// Returns the size of the data in this collection
		/// </summary>
		public string OwnerID
		{
			get
			{
				try
				{
					return this.Properties.GetSingleProperty( OwnerProperty ).Value as string;
				}
				catch(Exception ex)
				{
					log.Debug("Exception in getowner: "+ex.ToString());
					return null;
				}
 			}
		}

		/// <summary>
		/// Returns Dn of orphaned owner
		/// </summary>
		public string OrphanedOwnerDN
		{
			get
			{
				Property Prop = this.Properties.GetSingleProperty( OrphanedOwnerProperty );
				if( Prop != null)
				{
					return Prop.Value as string;
				}
				return null;
 			}
		}

		/// <summary>
                /// Returns the collection enabled / disabled status
                /// </summary>
                public bool Disabled 
                {
                        get
                        {
                                Property p = this.Properties.GetSingleProperty( DisabledProperty );
				return ( p != null ) ? (bool)p.Value : false;
                        }
                }

		/// <summary>
		/// Returns an array of UserIDs which are members of the collection
		/// </summary>
		public string[] UserIDs
		{
			get
			{
				string[] userids = null;
				MultiValuedList mv = this.Properties.GetProperties( MemberProperty );
				if ( mv != null && mv.Count > 0 )
				{
					userids = new string[ mv.Count ];
					int x = 0;
					foreach( Property prop in mv )
					{
						string[] comps = ( (string) prop.Value ).Split( ':' );
						userids[ x++ ] = comps[0];
					}
				}

				return userids;
			}
		}
		#endregion
                public void SetHostID(string hostid, bool localproperty)
                {
                        log.Debug("SetHostID: Changing the host ID to: {0}", hostid);
                        Property hprop = new Property( HostProperty, hostid);
			if( localproperty )
				hprop.LocalProperty = true;
                        this.Properties.ModifyProperty( hprop );
                        catalog.Commit(this);
                }

		#region Constructor
        /// <summary>
        /// constructor
        /// </summary>
		static CatalogEntry()
		{
			store = Store.GetStore();
			catalog = store.GetCollectionByID( Catalog.catalogID );
			if ( catalog == null )
			{
				throw new SimiasException( "Failed to find the catalog collection" );
			}

			HostNode local = HostNode.GetLocalHost();
			if( local != null)
				localhostID = local.UserID;
		}

		internal CatalogEntry( ShallowNode sn ) : base( CatalogEntry.catalog, sn )
		{


		}
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="collectionID"></param>
		internal CatalogEntry( string collectionID )
		{
			Property cprop = new Property( CollectionProperty, collectionID );
			this.Properties.ModifyProperty( cprop );

			Property hprop = new Property( HostProperty, localhostID );
			this.Properties.ModifyProperty( hprop );

			Collection col = store.GetCollectionByID( collectionID );
			if ( col != null )
			{
				this.Name = col.Name;

			        Property sprop = new Property( SizeProperty, col.StorageSize );
			        this.Properties.ModifyProperty( sprop );
			}
		}
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="CollectionID"></param>
        /// <param name="Name"></param>
		internal CatalogEntry( string CollectionID, string Name  )
		{
			Property cprop = new Property( CollectionProperty, CollectionID );
			this.Properties.ModifyProperty( cprop );

			Property hprop = new Property( HostProperty, localhostID );
			this.Properties.ModifyProperty( hprop );

			Collection col = store.GetCollectionByID( CollectionID );
			if ( col != null )
			{
			        Property sprop = new Property( SizeProperty, col.StorageSize );
				this.Properties.ModifyProperty( sprop );
			}

			this.Name = Name;
		}

		#endregion

		#region Private Methods

		#endregion

		#region Public Methods
		/// <summary>
		/// Add a member member to the catalog entry.
		/// Note: called when a new member is added to the collection.
		/// 
		/// Kludge: When a member node is deleted, the simias event framework
		/// does NOT pass the member's user ID in event record only the node ID.
		/// Since the node has already been deleted when we get the event it's pretty
		/// much useless unless the service is just logging that a delete occurred.
		/// So for the catalog both the memberID and the nodeID will be saved
		/// this way we can matchup the delete of the member ace if it ever
		/// does occurr.
		/// </summary>
		internal bool AddMember( string UserID, string NodeID )
		{
			MultiValuedList mv = this.Properties.GetProperties( MemberProperty );
			foreach( Property prop in mv )
			{
				string[] comps =  ( (string) prop.Value ).Split( ':' );
				if ( comps[0] == UserID && comps[1] == NodeID )
				{
					return false;
				}
			}

			string storageFormat = String.Format( "{0}:{1}", UserID, NodeID );
			this.Properties.AddProperty( new Property( MemberProperty, storageFormat ) );
			catalog.Commit( this );
			return true;
		}

        /// <summary>
        /// Is this catalog entry shared with this user
        /// </summary>
        /// <param name="UserID">userid</param>
        /// <returns>true if shared</returns>
		public bool isSharedWithUser(string UserID)
		{
			MultiValuedList mv = this.Properties.GetProperties( MemberProperty );
			if ( mv.Count > 0 )
			{
				foreach( Property prop in mv )
				{
					string[] comps = ( (string) prop.Value ).Split( ':' );
					if(String.Compare(comps[0],UserID) == 0)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Remove a member from the catalog entry.
		/// Note: called when a member is removed from the collection.
		/// See above explanation for kludgy format
		/// </summary>
		public bool RemoveMember( string NodeID )
		{
			MultiValuedList mv = this.Properties.GetProperties( MemberProperty );
			foreach( Property prop in mv )
			{
				string[] comps = ( (string) prop.Value ).Split( ':' );
				log.Debug("comp1 {0} comp2 {1}", comps[0], comps[1]);
				if ( comps.Length == 2 && comps[1] == NodeID )
				{
					prop.Delete();
					catalog.Commit( this );
					log.Debug( "deleting user property!" );
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Add previous owner as orphaned owner in the catalog entry.
		/// Note: called when ownership is transferred and collection is made orphaned .
		/// </summary>
		public void SetCollSizeByCollectionID( string collectionID )
		{
			Collection col = store.GetCollectionByID( CollectionID );
			if ( col != null )
			{
			        Property sprop = new Property( SizeProperty, col.StorageSize );
                                try
                                {
                                        Property catalogsizeproperty = this.Properties.GetSingleProperty(SizeProperty);
                                        if( catalogsizeproperty != null )
                                        {
                                                long size = (long)catalogsizeproperty.Value;
                                                if( size == col.StorageSize )
                                                {
                                                        log.Debug("The size is same. Nothing to update here. Not changing the node entry.");
                                                        return;
                                                }
                                                else
                                                        log.Debug("The size on catalog: {0} and size on collection: {1}", size, col.StorageSize);
                                        }
                                }
                                catch(Exception e)
                                {
                                        log.Debug("Exception in SetCollSizeByCollectionID. {0}--{1}", e.Message, e.StackTrace);
                                }
				this.Properties.ModifyProperty( sprop );
			}
			catalog.Commit(this);
			return;
		}

		/// <summary>
		/// Add previous owner as orphaned owner in the catalog entry.
		/// Note: called when ownership is transferred and collection is made orphaned .
		/// </summary>
		 /// <param name="UserDN">user DN which will be added as Orphaned Owner</param>
		public void AddOrphanedOwner( string UserDN )
		{
			Property Prop = new Property(OrphanedOwnerProperty, UserDN);
			this.Properties.ModifyProperty(Prop);
			catalog.Commit(this);
			log.Debug("Owner Change : Added OrphanedOwner into Catalog: "+UserDN);
		}

		/// <summary>
		/// remove orphaned owner property when iFolder is adopted, i.e. assigned an owner.
		/// Note: called when ownership is transferred and collection is made orphaned .
		/// </summary>
		public void RemoveOrphanedOwner( )
		{
			Property Prop = this.Properties.GetSingleProperty( OrphanedOwnerProperty);
			if (Prop != null)
			{
				this.Properties.DeleteSingleProperty( Prop);
				catalog.Commit(this);
				log.Debug("Owner Change : Removed OrphanedOwner property from Catalog ");
			}
		}

        /// <summary>
        /// add owner to catalog
        /// </summary>
        /// <param name="UserID">userid</param>
		internal void AddOwner( string UserID )
		{
		       Property oprop = new Property( OwnerProperty, UserID );
		       this.Properties.ModifyProperty( oprop );
		       catalog.Commit( this );
		}


		#endregion 
	}

}
