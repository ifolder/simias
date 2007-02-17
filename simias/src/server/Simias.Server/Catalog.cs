/***********************************************************************
 *  $RCSfile: Catalog.cs,v $
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

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
		internal static string catalogID = "a93266fd-55de-4590-b1c7-428f2fed815d";
		internal static string catalogName = "Collection Catalogue";

		private static CollectionSyncClient syncClient;
		private static AutoResetEvent syncEvent = new AutoResetEvent( false );
		private static SimiasConnection connection;

		#endregion

		#region Properties
		#endregion

		#region Constructors

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

		#region Private Methods
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
				conn.InitializeWebClient( hostlocation, "HostLocation.asmx" );
				log.Debug( "Master's HostLocation url {0}", hostlocation.Url );
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

									// DEBUG CODE
									CatalogEntry[] entries = Catalog.GetAllEntriesByUserID( member.UserID );
									foreach( CatalogEntry ce in entries )
									{
										log.Debug( "" );
										log.Debug( "Catalog Entry" );
										log.Debug( "\t{0}", ce.Name );
										log.Debug( "\tCID: {0}", ce.CollectionID );
										log.Debug( "\tHID: {0}", ce.HostID );

										string[] members = ce.UserIDs;
										log.Debug( "\tMembers" );
										foreach( string userid in members )
										{
											log.Debug( "\t\t{0}", userid );
										}
										log.Debug( "" );
									}
									// END DEBUG CODE
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
			CatalogEntry catentry;

			if ( catalog != null )
			{
				log.Debug( "Starting collection scan..." );
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
									if (member.IsOwner)
									{
									    catentry.AddOwner ( member.UserID );
									}
									    
									catentry.AddMember( member.UserID, member.ID );
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

		static private void SyncCatalog()
		{
			int retry = 10;
			while( down == false )
			{
				if ( catalog == null )
				{
					syncEvent.WaitOne( 10000, false );
					continue;
				}

				HostNode host = null;
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
					if ( retry <= 0 )
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

				try
				{
					syncClient.SyncNow();
				}
				catch {}

				if ( down == true )
				{
					break;
				}

				syncClient.Reschedule( true, 30 );
			}
		}

		/// <summary>
		/// Called by The CollectionSyncClient when it is time to run another sync pass.
		/// </summary>
		/// <param name="collectionClient">The client that is ready to sync.</param>
		internal static void TimerFired( object collectionClient )
		{
			while(CollectionSyncClient.running)
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
						catCreateThread.IsBackground = true;
						catCreateThread.Start();
					}
				}
				else
				{
					scanThread = new Thread( new ThreadStart( ScanCollections ) );
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
				catThread.IsBackground = true;
				catThread.Start();

				if ( domain.Role == Simias.Sync.SyncRoles.Slave )
				{
					// Now start the sync process for the catalog collection.
					Thread syncThread = new Thread( new ThreadStart( SyncCatalog ) );
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
		static public string[] GetAllEntryIDsByUserID( string UserID )
		{
			Property midsProp = new Property( CatalogEntry.MemberProperty, UserID );
			ICSList nodes = store.GetNodesByProperty( midsProp, SearchOp.Begins );
			string[] entryids = new string[ nodes.Count ];
			int x = 0;
			foreach( ShallowNode sn in nodes )
			{
				entryids[ x++ ] = sn.ID;
			}

			return entryids;
		}

		/// <summary>
		/// Method to retrieve all catalog entries the specified user
		/// is a member of.
		/// </summary> 
		static public CatalogEntry[] GetAllEntriesByUserID( string UserID )
		{
			ArrayList entries = new ArrayList();

			Property midsProp = new Property( CatalogEntry.MemberProperty, UserID );
			ICSList nodes = store.GetNodesByProperty( midsProp, SearchOp.Begins );
			foreach( ShallowNode sn in nodes )
			{
				entries.Add( new CatalogEntry( sn ) );
			}

			return entries.ToArray( typeof( CatalogEntry ) ) as CatalogEntry[];
		}

		/// <summary>
		/// Get a catalog entry for the specified collection
		/// </summary>
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
		}

		/// <summary>
		/// Returns the size of the data in this collection
		/// </summary>
		public string OwnerID
		{
			get
			{
				return this.Properties.GetSingleProperty( OwnerProperty ).Value as string;
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
				if ( mv.Count > 0 )
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

		#region Constructor

		static CatalogEntry()
		{
			store = Store.GetStore();
			catalog = store.GetCollectionByID( Catalog.catalogID );
			if ( catalog == null )
			{
				throw new SimiasException( "Failed to find the catalog collection" );
			}

			HostNode local = HostNode.GetLocalHost();
			localhostID = local.UserID;
		}

		internal CatalogEntry( ShallowNode sn ) : base( CatalogEntry.catalog, sn )
		{


		}

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
		/// Remove a member from the catalog entry.
		/// Note: called when a member is removed from the collection.
		/// See above explanation for kludgy format
		/// </summary>
		internal bool RemoveMember( string NodeID )
		{
			MultiValuedList mv = this.Properties.GetProperties( MemberProperty );
			foreach( Property prop in mv )
			{
				string[] comps = ( (string) prop.Value ).Split( ':' );
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

		internal void AddOwner( string UserID )
		{
		       Property oprop = new Property( OwnerProperty, UserID );
		       this.Properties.ModifyProperty( oprop );
		       catalog.Commit( this );
		}


		#endregion 
	}
}
