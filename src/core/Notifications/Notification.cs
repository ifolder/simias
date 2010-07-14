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
*                 $Author: Bruce Getter <bgetter@novell.com>
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
using System.Threading;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.POBox;
using Simias.Service;
using Simias.Sync;

namespace Simias.Storage
{
	/// <summary>
	/// Summary description for Notification.
	/// </summary>
	public class NotificationService : IThreadService
	{
		#region Class Members

		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(NotificationService));
		Store store = Store.GetStore();
		EventSubscriber storeEvents;
		SyncEventSubscriber syncEvents;
		Queue eventQueue;
		ManualResetEvent queueEvent;
		Thread notificationThread;
		bool shuttingDown;

		#endregion

		#region Properties
		#endregion

		#region Private Methods


        /// <summary>
        /// Process the events received from simias
        /// </summary>
		private void processEvent()
		{
			while (!shuttingDown)
			{
				// Wait for something to be added to the queue.
				queueEvent.WaitOne();

				// Now loop until the queue is emptied.
				while (true)
				{
					SimiasEventArgs args;
					lock (eventQueue)
					{
						if (eventQueue.Count == 0)
						{
							queueEvent.Reset();
							break;
						}

						args = eventQueue.Dequeue() as SimiasEventArgs;
					}

					// Process the event.
					if (!shuttingDown)
					{
						try
						{
							NodeEventArgs neArgs = null;
							if ( args.GetType().Equals( typeof( NodeEventArgs ) ) )
							{
								neArgs = args as NodeEventArgs;
							}

							NotificationLog notificationLog = NotificationLog.NotificationLogFactory( store );
							if ( notificationLog != null )
							{
								// Don't process events for the notification log collection.
								if ( neArgs != null && notificationLog.ID.Equals( neArgs.Collection ) )
								{
									continue;
								}
							}
							else
							{
								notificationLog = new NotificationLog( store, "NotificationLog", Guid.NewGuid().ToString(), NodeTypes.NotificationLogType, store.LocalDomain );
							}

							if ( neArgs != null )
							{
								notificationLog.processNodeEvent( neArgs );
							}
							else
							{
								notificationLog.processSyncEvent( args );
							}
						}
						catch (Exception e)
						{
							log.Info( "Caught exception in NotificationService thread - {0}", e.Message );
						}
					}
					else
					{
//						log.Info( "Lost notification for node ID = '{0}'.  Event type = '{1}'.  Node type = '{2}'.", args.Node, args.EventType, args.Type );
					}
				}
			}
		}

        /// <summary>
        /// Stores the node event received. It queues the event.
        /// </summary>
        /// <param name="args">Node event details</param>
		private void storeEvents_NodeEvent( NodeEventArgs args )
		{
			lock ( eventQueue )
			{
				eventQueue.Enqueue( args );

				queueEvent.Set();
			}
		}

        /// <summary>
        /// Enqueues the collection sync event
        /// </summary>
        /// <param name="args">Collection sync details</param>
		private void syncEvents_CollectionSync(CollectionSyncEventArgs args)
		{
			lock ( eventQueue )
			{
				eventQueue.Enqueue( args );

				queueEvent.Set();
			}
		}

        /// <summary>
        /// Enqueues the file sync event.
        /// </summary>
        /// <param name="args">File sync event details</param>
		private void syncEvents_FileSync(FileSyncEventArgs args)
		{
			lock ( eventQueue )
			{
				eventQueue.Enqueue( args );

				queueEvent.Set();
			}
		}
		
		#endregion

		#region IThreadService Members

		/// <summary>
		/// Called to start the service.
		/// </summary>
		public void Start()
		{
			shuttingDown = false;
			eventQueue = new Queue();
			queueEvent = new ManualResetEvent(false);

			notificationThread = new Thread( new ThreadStart(processEvent));
            notificationThread.Name = "Notification Event Process";
            notificationThread.Priority = ThreadPriority.BelowNormal;
			notificationThread.IsBackground = true;
			notificationThread.Start();

			// Add handlers for node events.
			storeEvents = new EventSubscriber();
			storeEvents.NodeChanged += new NodeEventHandler( storeEvents_NodeEvent );
			storeEvents.NodeCreated += new NodeEventHandler( storeEvents_NodeEvent );
			storeEvents.NodeDeleted += new NodeEventHandler( storeEvents_NodeEvent );

			// Add handlers for sync events.
			syncEvents = new SyncEventSubscriber();
			syncEvents.CollectionSync += new CollectionSyncEventHandler( syncEvents_CollectionSync );
			syncEvents.FileSync += new FileSyncEventHandler( syncEvents_FileSync );
		}

		/// <summary>
		/// Called to stop the service.
		/// </summary>
		public void Stop()
		{
			shuttingDown = true;
			lock (eventQueue)
			{
				queueEvent.Set();
			}
			notificationThread.Join();
			storeEvents.Dispose();
		}

		/// <summary>
		/// Called to pause the service.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Called to resume the service after a pause.
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Called to process the service defined message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public int Custom(int message, string data)
		{
			return 0;
		}

		#endregion

	}

	/// <summary>
	/// Class used to keep track of outstanding searches.
	/// </summary>
	internal class NotificationSearchState : IDisposable
	{
		#region Class Members
		/// <summary>
		/// Table used to keep track of outstanding search entries.
		/// </summary>
		static private Hashtable searchTable = new Hashtable();

		/// <summary>
		/// Indicates whether the object has been disposed.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Handle used to store and recall this context object.
		/// </summary>
		private string contextHandle = Guid.NewGuid().ToString();

		/// <summary>
		/// Identifier for the collection that is being searched.
		/// </summary>
		private string collectionID;

		/// <summary>
		/// Object used to iteratively return the members from the domain.
		/// </summary>
		private ICSEnumerator enumerator;

		/// <summary>
		/// Total number of records contained in the search.
		/// </summary>
		private int totalRecords;

		/// <summary>
		/// The cursor for the caller.
		/// </summary>
		private int currentRecord = 0;

		/// <summary>
		/// The last count of records returned.
		/// </summary>
		private int previousCount = 0;
		#endregion

		#region Properties
		/// <summary>
		/// Indicates if the object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return disposed; }
		}

		/// <summary>
		/// Gets the context handle for this object.
		/// </summary>
		public string ContextHandle
		{
			get { return contextHandle; }
		}

		/// <summary>
		/// Gets or sets the current record.
		/// </summary>
		public int CurrentRecord
		{
			get { return currentRecord; }
			set { currentRecord = value; }
		}

		/// <summary>
		/// Gets the ID for the collection that is being searched.
		/// </summary>
		public string CollectionID
		{
			get { return collectionID; }
		}

		/// <summary>
		/// Gets or sets the last record count.
		/// </summary>
		public int LastCount
		{
			get { return previousCount; }
			set { previousCount = value; }
		}

		/// <summary>
		/// Gets the search iterator.
		/// </summary>
		public ICSEnumerator Enumerator
		{
			get { return enumerator; }
		}

		/// <summary>
		/// Gets the total number of records contained by this search.
		/// </summary>
		public int TotalRecords
		{
			get { return totalRecords; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of an object.
		/// </summary>
		/// <param name="collectionID">Identifier for the collection that is being searched.</param>
		/// <param name="enumerator">Search iterator.</param>
		/// <param name="totalRecords">The total number of records contained in the search.</param>
		public NotificationSearchState( string collectionID, ICSEnumerator enumerator, int totalRecords )
		{
			this.collectionID = collectionID;
			this.enumerator = enumerator;
			this.totalRecords = totalRecords;

			lock ( searchTable )
			{
				searchTable.Add( contextHandle, this );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Removes this SearchState object from the search table.
		/// </summary>
		private void RemoveSearchState()
		{
			lock ( searchTable )
			{
				// Remove the search context from the table and dispose it.
				searchTable.Remove( contextHandle );
			}
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Returns a search context object that contains the state information for an outstanding search.
		/// </summary>
		/// <param name="contextHandle">Context handle that refers to a specific search context object.</param>
		/// <returns>A SearchState object if a valid one exists, otherwise a null is returned.</returns>
		static public NotificationSearchState GetSearchState( string contextHandle )
		{
			lock ( searchTable )
			{
				return searchTable[ contextHandle ] as NotificationSearchState;
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Allows for quick release of managed and unmanaged resources.
		/// Called by applications.
		/// </summary>
		public void Dispose()
		{
			RemoveSearchState();
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// Dispose( bool disposing ) executes in two distinct scenarios.
		/// If disposing equals true, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals false, the method has been called by the 
		/// runtime from inside the finalizer and you should not reference 
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
		private void Dispose( bool disposing )
		{
			// Check to see if Dispose has already been called.
			if ( !disposed )
			{
				// Protect callers from accessing the freed members.
				disposed = true;

				// If disposing equals true, dispose all managed and unmanaged resources.
				if ( disposing )
				{
					// Dispose managed resources.
					enumerator.Dispose();
				}
			}
		}
		
		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~NotificationSearchState()      
		{
			Dispose( false );
		}

		#endregion
	}

	/// <summary>
	/// The Notification types supported.
	/// </summary>
	[Flags]
	public enum NotificationType
	{
		/// <summary>
		/// The notification is for a conflict occurring.
		/// </summary>
		ConflictOccurred = 1,
		/// <summary>
		/// The notification is for a member joining a collection.
		/// </summary>
		MemberJoined = 2,
		/// <summary>
		/// The notification is for a collection being shared.
		/// </summary>
		CollectionShared = 4,
		/// <summary>
		/// The notification is for a synchronization failure due to quota restrictions.
		/// </summary>
		SyncFailure_Quota = 8,
		/// <summary>
		/// The notification is for a synchronization failure due to read-only restrictions.
		/// </summary>
		SyncFailure_ReadOnly = 16,
	};

	/// <summary>
	/// Class that represents the notification log collection.
	/// </summary>
	public class NotificationLog : Collection
	{
		#region Class Members

		static string lastNodeModified = null;
		static Hashtable initialSyncCollections = new Hashtable();

		bool collectionModified = false;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor for creating an existing Notification object from a ShallowNode.
		/// </summary>
		/// <param name="store">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		public NotificationLog( Store store, ShallowNode shallowNode ) :
			base( store, shallowNode )
		{
		}

		/// <summary>
		/// Copy constructor for Collection object.
		/// </summary>
		/// <param name="collection">Collection object to construct new NotificationLog object from.</param>
		public NotificationLog( Collection collection ) :
			base( collection )
		{
		}

		/// <summary>
		/// Constructor to create a new NotificationLog object.
		/// </summary>
		/// <param name="store">Store object that this collection belongs to.</param>
		/// <param name="collectionName">This is the friendly name that is used by applications to describe this object.</param>
		/// <param name="collectionID">The globally unique identifier for this object.</param>
		/// <param name="collectionType">Base type of collection object.</param>
		/// <param name="domainID">The domain that this object is stored in.</param>
		internal NotificationLog( Store store, string collectionName, string collectionID, string collectionType, string domainID ) :
			base( store, collectionName, collectionID, collectionType, domainID )
		{
			collectionModified = true;
		}

		/// <summary>
		/// Constructor for creating an existing NotificationLog object.
		/// </summary>
		/// <param name="store">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document that describes a Notification object.</param>
		internal protected NotificationLog( Store store, XmlDocument document ) :
			base( store, document )
		{
		}

		#endregion

		#region Internal Methods

        /// <summary>
        /// Processess the Node events
        /// </summary>
        /// <param name="args">Node event details</param>
		internal void processNodeEvent( NodeEventArgs args )
		{
			Node node = null;

			if ( args.EventType == EventType.NodeCreated )
			{
				if ( args.Type == NodeTypes.MemberType )
				{
					// Only generate a notification for events originating from a sync.
					if ( LogMembers && args.Source.Equals( "Sync" ) && !initialSyncCollections.Contains( args.Collection ) )
					{
						node = storeNodeEvent( NotificationType.MemberJoined, args );
					}
				}
				else if ( args.Type == NodeTypes.SubscriptionType )
				{
					if ( LogShared )
					{
						node = storeNodeEvent( NotificationType.CollectionShared, args );
					}
				}
				else if ( args.Type == NodeTypes.CollectionType ||
					args.Type == NodeTypes.DomainType ||
					args.Type == NodeTypes.POBoxType )
				{
					// Don't log notifications for newly created collections.
					initialSyncCollections.Add( args.Collection, null );
				}
			}
			else if ( args.EventType == EventType.NodeChanged )
			{
				if ( LogCollisions && args.Type.Equals( NodeTypes.CollectionType ) )
				{
					// When a collision occurs, the node is modified before the collision is created.
					// We keep track of the last modified node so that we only generate a single collision
					// notification message.
					if ( lastNodeModified != null )
					{
						Collection collection = Store.GetStore().GetCollectionByID( args.Collection );
						Node mNode = collection.GetNodeByID( lastNodeModified );
						if ( mNode != null && mNode.Properties.GetSingleProperty( PropertyTags.Collision ) != null )
						{
							node = storeNodeEvent( NotificationType.ConflictOccurred, args );
							lastNodeModified = null;
						}
					}
				}
				else
				{
					lastNodeModified = args.Node;
				}
			}

			CommitChanges( node );
		}

        /// <summary>
        /// Process both File and Collection Sync event received
        /// </summary>
        /// <param name="args">Simias event</param>
		internal void processSyncEvent( SimiasEventArgs args )
		{
			if ( args.GetType().Equals( typeof( CollectionSyncEventArgs ) ) )
			{
				CollectionSyncEventArgs cseArgs = args as CollectionSyncEventArgs;

				if ( cseArgs.Action.Equals ( Simias.Client.Event.Action.StopSync ) )
				{
					// If the collection is in the initial sync list and the sync finished successfully,
					// remove it from the list.
					if ( initialSyncCollections.Contains(cseArgs.ID) && cseArgs.Connected && !cseArgs.Yielded )
					{
						initialSyncCollections.Remove(cseArgs.ID);
					}
				}
			}
			else if ( args.GetType().Equals( typeof( FileSyncEventArgs ) ) )
			{
				FileSyncEventArgs fseArgs = args as FileSyncEventArgs;

				Node node = null;
				switch ( fseArgs.Status )
				{
					case SyncStatus.PolicyQuota:
						node = storeSyncEvent( NotificationType.SyncFailure_Quota, fseArgs );
						break;
					case SyncStatus.ReadOnly:
						node = storeSyncEvent( NotificationType.SyncFailure_ReadOnly, fseArgs );
						break;
				}

                CommitChanges( node );
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets/sets a value indicating if collision notifications should be logged.
		/// </summary>
		public bool LogCollisions
		{
			get
			{
				return ( NotificationBitMask & NotificationType.ConflictOccurred ) == NotificationType.ConflictOccurred;
			}
			set
			{
				// Update the value if it has changed.
				if ( value != LogCollisions )
				{
					NotificationBitMask ^= NotificationType.ConflictOccurred;
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if member join notifications should be logged.
		/// </summary>
		public bool LogMembers
		{
			get
			{
				return ( NotificationBitMask & NotificationType.MemberJoined ) == NotificationType.MemberJoined;
			}
			set
			{
				// Update the value if it has changed.
				if ( value != LogMembers )
				{
					NotificationBitMask ^= NotificationType.MemberJoined;
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if quota sync failure notifications should be logged.
		/// </summary>
		public bool LogQuotaFailures
		{
			get
			{
				return ( NotificationBitMask & NotificationType.SyncFailure_Quota ) == NotificationType.SyncFailure_Quota;
			}
			set
			{
				// Update the value if it has changed.
				if ( value != LogQuotaFailures )
				{
					NotificationBitMask ^= NotificationType.SyncFailure_Quota;
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if read-only sync failure notifications should be logged.
		/// </summary>
		public bool LogReadOnlyFailures
		{
			get
			{
				return ( NotificationBitMask & NotificationType.SyncFailure_ReadOnly ) == NotificationType.SyncFailure_ReadOnly;
			}
			set
			{
				// Update the value if it has changed.
				if ( value != LogReadOnlyFailures )
				{
					NotificationBitMask ^= NotificationType.SyncFailure_ReadOnly;
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if collection shared notifications should be logged.
		/// </summary>
		public bool LogShared
		{
			get
			{
				return ( NotificationBitMask & NotificationType.CollectionShared ) == NotificationType.CollectionShared;
			}
			set
			{
				// Update the value if it has changed.
				if ( value != LogShared )
				{
					NotificationBitMask ^= NotificationType.CollectionShared;
				}
			}
		}

		/// <summary>
		/// Gets/sets the notification bitmask for the notification log.
		/// </summary>
		public NotificationType NotificationBitMask
		{
			get
			{
				Property property = Properties.GetSingleProperty( "NotifyBitMask" );
				if ( property != null )
				{
					return (NotificationType)property.Value;
				}

				// The property doesn't exist, return the default policy (log all notifications).
				return NotificationType.CollectionShared | NotificationType.ConflictOccurred | NotificationType.MemberJoined | NotificationType.SyncFailure_Quota | NotificationType.SyncFailure_ReadOnly;
			}
			set
			{
				// Update the value if it has changed.
				if ( NotificationBitMask != value )
				{
					Properties.ModifyProperty( "NotifyBitMask", value );
					collectionModified = true;
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if the number of notifications persisted is based on the actual number of
		/// notifications stored (<b>false</b>) or the number of days the notifications have been stored (<b>true</b>).
		/// </summary>
		public bool PersistByDate
		{
			get
			{
				Property property = Properties.GetSingleProperty( "PersistByDate" );
				if ( property != null )
				{
					return (bool)property.Value;
				}

				// The property doesn't exist, return the default value (persist n number of notifications).
				return false;
			}
			set
			{
				// Update the value if it has changed.
				if ( PersistByDate != value )
				{
					Properties.ModifyProperty( "PersistByDate", value );
					collectionModified = true;
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating the number of notifications stored or the number of days that the notifications
		/// are stored (based on the value of <b>PersistByCount</b>.
		/// </summary>
		public int PersistedCount
		{
			get
			{
				Property property = Properties.GetSingleProperty( "PersistedCount" );
				if ( property != null )
				{
					return (int)property.Value;
				}

				// The property doesn't exist, return the default (5 days or 50 entries).
				if ( PersistByDate )
				{
					return 5;
				}
				else
				{
					return 50;
				}
			}
			set
			{
				// Update the value if it has changed.
				if ( PersistedCount != value )
				{
					Properties.ModifyProperty( "PersistedCount", value );
					collectionModified = true;
				}
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets all the notification nodes stored in the Notification Log.
		/// </summary>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Node objects found.</returns>
		private ICSList GetOrderedNotificationNodes()
		{
			ArrayList list = new ArrayList();

			// Get the creation time of the member node for the notification log.
			Node mNode = GetSingleNodeByType( NodeTypes.MemberType );
			if ( mNode != null )
			{
				Property property = mNode.Properties.GetSingleProperty( PropertyTags.NodeCreationTime );

				// Search for all nodes with a creation time greater than the creation time of the member node.  This
				// will cause the notification nodes to be put in the list in chronological order.
				ICSList icsList = Search( property, SearchOp.Greater );

				// In case there are other node types stored in the notification log, we'll walk the list and only return
				// the notification nodes.
				foreach ( ShallowNode sn in icsList )
				{
					if ( sn.Type.Equals( "Notification" ) )
					{
						list.Add( sn );
					}
				}
			}

			return new ICSList( list );
		}

		/// <summary>
		/// Gets all of the notifications in the Notification Log that were created before the specified time.
		/// </summary>
		/// <param name="time">The time used to perform the search.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Node objects found.</returns>
		private ICSList GetOrderedNotificationNodesBeforeTime( DateTime time )
		{
			ArrayList list = new ArrayList();

			// Search for all nodes with a creation time less than the specified time.
			ICSList icsList = Search( PropertyTags.NodeCreationTime, time, SearchOp.Less );

			// In case there are other node types stored in the notification log, we'll walk the list and only return
			// the notification nodes.
			foreach ( ShallowNode sn in icsList )
			{
				if ( sn.Type.Equals( "Notification" ) )
				{
					list.Add( sn );
				}
			}

			return new ICSList( list );
		}

		/// <summary>
		/// Creates a Node object which represents a notification for the given node event.
		/// </summary>
		/// <param name="type">The type of the notification.</param>
		/// <param name="args">The node event used to create the notification.</param>
		/// <returns>A Node object representing the notification.</returns>
		private Node storeNodeEvent( NotificationType type, NodeEventArgs args )
		{
			// TODO: friendly name?
			Node node = new Node( "Notify", Guid.NewGuid().ToString(), "Notification" );

			node.Properties.AddNodeProperty( "nType", type );
			node.Properties.AddNodeProperty( "nCollection", args.Collection );
			node.Properties.AddNodeProperty( "nNode", args.Node );
			node.Properties.AddNodeProperty( "nTimeStamp", args.TimeStamp );

			return node;
		}

		/// <summary>
		/// Creates a Node object which represents a notification for the given sync event.
		/// </summary>
		/// <param name="type">The type of the notification.</param>
		/// <param name="args">The sync event used to create the notification.</param>
		/// <returns>A Node object representing the notification.</returns>
		private Node storeSyncEvent( NotificationType type, FileSyncEventArgs args )
		{
			// Search for an existing notification.
			bool logged = false;
			ICSList list = Search( BaseSchema.ObjectName, args.Name, SearchOp.Equal );
			foreach ( ShallowNode sn in list )
			{
				Node lNode = GetNodeByID( sn.ID );
				Property property = lNode.Properties.GetSingleProperty( "nType" );
				if ( property != null )
				{
					if ( type.Equals( (NotificationType)property.Value ) )
					{
						// TODO: Update existing node with new timestamp???
						logged = true;
						break;
					}
				}
			}

			Node node = null;
			if ( !logged )
			{
				node = new Node( args.Name, Guid.NewGuid().ToString(), "Notification" );

				node.Properties.AddNodeProperty( "nType", type );
				node.Properties.AddNodeProperty( "nCollection", args.CollectionID );
				node.Properties.AddNodeProperty( "nTimeStamp", args.TimeStamp );
			}

			return node;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Clears all entries from the notification log.
		/// </summary>
		public void Clear()
		{
			ICSList list = GetNodesByType( "Notification" );

			Node[] nodeList = new Node[ list.Count ];
			int index = 0;

			foreach ( ShallowNode sn in list )
			{
				nodeList[ index++ ] = GetNodeByID( sn.ID );
			}

			Commit( Delete( nodeList ) );
		}

		/// <summary>
		/// Commits the specified node to the notification log.  If any changes have been made to the
		/// notification log, these changes are also committed.
		/// </summary>
		/// <param name="node">The node to commit to the notification log.</param>
		public void CommitChanges( Node node )
		{
            //Skip all dummy commits
            if (node == null)
                return;

			Node[] deletedNodes;

			if ( PersistByDate )
			{
				// Build the DateTime for n days ago (where n is the number of days to persist notifications).
				DateTime time = DateTime.Now.Subtract( new TimeSpan( PersistedCount, 0, 0, 0, 0 ) );

				// Get a list of all notifications before the calculated DateTime.
				ICSList list = GetOrderedNotificationNodesBeforeTime( time );

				// Walk the returned list, instantiate the nodes and put them in an array to delete.
				int index = 0;
				deletedNodes = new Node[ list.Count ];
				foreach ( ShallowNode sn in list )
				{
					deletedNodes[ index++ ] = new Node( this, sn );
				}
			}
			else
			{
				deletedNodes = new Node[ 0 ];

				// Get a list of all notifications stored.
				ICSList list = GetOrderedNotificationNodes();

				// If the number of notifications stored is bigger than it should be, delete some of the entries.
				int count = list.Count - PersistedCount;
				if ( count > 0 )
				{
					int index = 0;

					// Put the first n nodes in an array to delete.
					deletedNodes = new Node[ count ];
					foreach ( ShallowNode sn in list )
					{
						deletedNodes[ index++ ] = new Node( this, sn );
						if ( index == count )
							break;
					}
				}
			}

			// Delete the nodes that exceed the persist limits.
			deletedNodes = Delete( deletedNodes );

			// Put the deleted nodes into an array along with the modified node(s) so a single commit can be performed.
			Node[] nodeList = new Node[ deletedNodes.Length + 2 ];
			deletedNodes.CopyTo( nodeList, 0 );
			nodeList[ deletedNodes.Length ] = node;
			nodeList[ deletedNodes.Length + 1 ] = collectionModified ? this : null;

			Commit( nodeList );
			collectionModified = false;
		}

		/// <summary>
		/// End the search for notification entries.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstEntries or
		/// FindNextEntries methods.</param>
		public void FindCloseEntries( string searchContext )
		{
			// See if there is a valid search context.
			NotificationSearchState searchState = NotificationSearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				searchState.Dispose();
			}
		}

		/// <summary>
		/// Starts a search for notification entries.
		/// </summary>
		/// <param name="count">Maximum number of Notification objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="notificationList">Receives an array object that contains the Notification objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True, if there are more notification entries; otherwise, false is returned.</returns>
		public bool FindFirstEntries( int count, out string searchContext, out Notification[] notificationList, out int total )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			searchContext = null;
			notificationList = null;
			total = 0;

			// Get the ordered list of notification nodes.
			ICSList list = GetOrderedNotificationNodes();
			NotificationSearchState searchState = new NotificationSearchState( ID, list.GetEnumerator() as ICSEnumerator, list.Count );
			searchContext = searchState.ContextHandle;
			total = list.Count;
			moreEntries = FindNextEntries( ref searchContext, count, out notificationList );

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for notification entries from the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstEntries method.</param>
		/// <param name="count">Maximum number of Notification objects to return.</param>
		/// <param name="notificationList">Receives an array object that contains the Notification objects.</param>
		/// <returns>True, if there are more notification entries; otherwise, false is returned.</returns>
		public bool FindNextEntries( ref string searchContext, int count, out Notification[] notificationList )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			notificationList = null;

			// See if there is a valid search context.
			NotificationSearchState searchState = NotificationSearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				// See if entries are to be returned.
				if ( count > 0 )
				{
					// Get the domain for this collection.
					Domain domain = Store.GetStore().GetDomain( Domain );
					if ( domain != null )
					{
						// Allocate a list to hold the member objects.
						ArrayList tempList = new ArrayList( count );
						ICSEnumerator enumerator = searchState.Enumerator;
						while( ( count > 0 ) && enumerator.MoveNext() )
						{
							// The enumeration returns ShallowNode objects.
							ShallowNode sn = enumerator.Current as ShallowNode;

							try
							{
								Notification notification = new Notification( GetNodeByID( sn.ID ) );
								tempList.Add( notification );
								--count;
							}
							catch{} // Ignore ... the notification probably references an object that no longer exists.
							// TODO: may need to include these notifications so that the count returned is correct.
						}

						if ( tempList.Count > 0 )
						{
							notificationList = tempList.ToArray( typeof ( Notification ) ) as Notification[];
							searchState.CurrentRecord += notificationList.Length;
							searchState.LastCount = notificationList.Length;
							moreEntries = ( count == 0 ) ? true : false;
						}
					}
				}
				else
				{
					if ( searchState.CurrentRecord < searchState.TotalRecords )
					{
						moreEntries = true;
					}
				}
			}

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for notification entries previous to the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstEntries method.</param>
		/// <param name="count">Maximum number of Notification objects to return.</param>
		/// <param name="notificationList">Receives an array object that contains the Notification objects.</param>
		/// <returns>True, if there are more notification entries; otherwise, false is returned.</returns>
		public bool FindPreviousEntries( ref string searchContext, int count, out Notification[] notificationList )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			notificationList = null;

			// See if there is a valid search context.
			NotificationSearchState searchState = NotificationSearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				// Backup the current cursor, but don't go passed the first record.
				if ( searchState.CurrentRecord > 0 )
				{
					bool invalidIndex = false;
					int cursorIndex = ( searchState.CurrentRecord - ( searchState.LastCount + count ) );
					if ( cursorIndex < 0 )
					{
						invalidIndex = true;
						count = searchState.CurrentRecord - searchState.LastCount;
						cursorIndex = 0;
					}

					// Set the new index for the cursor.
					if ( searchState.Enumerator.SetCursor( Simias.Storage.Provider.IndexOrigin.SET, cursorIndex ) )
					{
						// Reset the current record.
						searchState.CurrentRecord = cursorIndex;

						// Complete the search.
						FindNextEntries( ref searchContext, count, out notificationList );

						if ( ( invalidIndex == false ) && ( notificationList != null ) )
						{
							moreEntries = true;
						}
					}
				}
			}

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for notification entries from the specified record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstEntries method.</param>
		/// <param name="offset">Record offset to return notification entries from.</param>
		/// <param name="count">Maximum number of Notification objects to return.</param>
		/// <param name="notificationList">Receives an array object that contains the Notification objects.</param>
		/// <returns>True, if there are more notification entries; otherwise, false is returned.</returns>
		public bool FindSeekEntries( ref string searchContext, int offset, int count, out Notification[] notificationList )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			notificationList = null;

			// See if there is a valid search context.
			NotificationSearchState searchState = NotificationSearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				// Make sure that the specified offset is valid.
				if ( ( offset >= 0 ) && ( offset <= searchState.TotalRecords ) )
				{
					// Set the cursor to the specified offset.
					if ( searchState.Enumerator.SetCursor( Simias.Storage.Provider.IndexOrigin.SET, offset ) )
					{
						// Reset the current record.
						searchState.CurrentRecord = offset;

						// Complete the search.
						moreEntries = FindNextEntries( ref searchContext, count, out notificationList );
					}
				}
			}

			return moreEntries;
		}

		/// <summary>
		/// NotificationLog factory method that constructs a NotificationLog object for the specified Store object.
		/// </summary>
		/// <param name="store">Store object.</param>
		/// <returns>The NotificationLog object for the store.</returns>
		static public NotificationLog NotificationLogFactory( Store store )
		{
			NotificationLog notificationLog = null;

			// Get the notifications collection.
			ICSList list = store.GetCollectionsByType( NodeTypes.NotificationLogType );
			if ( list.Count > 1 )
			{
				// Shouldn't have more than one notification collection.
				throw new SimiasException( "Multiple notification collections exist!" );
			}

			foreach ( ShallowNode sn in list )
			{
				notificationLog = new NotificationLog( store, sn );
				break;
			}

			return notificationLog;
		}

		#endregion
	}

	/// <summary>
	/// Class to hold Notifications.
	/// </summary>
	[ Serializable ]
	public class Notification
	{
		#region Class Members

		string collectionID;
		string collectionName;
		string fileName;
		string memberName;
		string nodeID;
		string sharedCollectionID;
		string sharedCollectionName;
		string sharedCollectionType;
		DateTime timeStamp;
		NotificationType type;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a Notification object from a node that represents a notification.
		/// </summary>
		/// <param name="node">The node that represents the notification.</param>
		public Notification( Node node )
		{
			type = (NotificationType)node.Properties.GetSingleProperty( "nType" ).Value;
			timeStamp = (DateTime)node.Properties.GetSingleProperty( "nTimeStamp" ).Value;
			collectionID = (string)node.Properties.GetSingleProperty( "nCollection" ).Value;

			Store store = Store.GetStore();
			Collection collection = store.GetCollectionByID( collectionID );
			collectionName = collection.Name;

			switch ( type )
			{
				case NotificationType.CollectionShared:
					nodeID = (string)node.Properties.GetSingleProperty( "nNode" ).Value;
					Subscription sub = new Subscription( collection.GetNodeByID( nodeID ) );
					sharedCollectionID = sub.SubscriptionCollectionID;
					sharedCollectionName = sub.SubscriptionCollectionName;
					sharedCollectionType = sub.SubscriptionCollectionType;
					break;
				case NotificationType.ConflictOccurred:
					nodeID = (string)node.Properties.GetSingleProperty( "nNode" ).Value;
					break;
				case NotificationType.MemberJoined:
					nodeID = (string)node.Properties.GetSingleProperty( "nNode" ).Value;
					Domain domain = store.GetDomain( collection.Domain );
					Member member = domain.GetMemberByID( new Member( collection.GetNodeByID( nodeID ) ).UserID );
					memberName = member.FN != null ? member.FN : member.Name;
					break;
				case NotificationType.SyncFailure_Quota:
				case NotificationType.SyncFailure_ReadOnly:
					fileName = node.Name;
					break;
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the identifier of the collection that the notification applies to.
		/// </summary>
		public string CollectionID
		{
			get { return collectionID; }
		}

		/// <summary>
		/// Gets the name of the collection that the notification applies to.
		/// </summary>
		public string CollectionName
		{
			get { return collectionName; }
		}

		/// <summary>
		/// Gets the name of the file that the notification applies to.
		/// <b>Note:</b> This property is only valid for notifications of type <b>NotificationType.SyncFailure_Quota</b>
		/// or <b>NotificationType.SyncFailure_ReadOnly</b>.
		/// </summary>
		public string FileName
		{
			get { return fileName; }
		}

		/// <summary>
		/// Gets the name of the member that the notification applies to.
		/// <b>Note:</b> This property is only valid for notifications of type <b>NotificationType.MemberJoined</b>.
		/// </summary>
		public string MemberName
		{
			get { return memberName; }
		}

		/// <summary>
		/// Gets the identifier of the node that the notification applies to.
		/// <b>Note:</b> This property is only valid for notifications of type <b>NotificationType.CollectionShared</b>,
		/// <b>NotificationType.MemberJoined</b>, or <b>NotificationType.ConflictOccurred</b>.
		/// </summary>
		public string NodeID
		{
			get { return nodeID; }
		}

		/// <summary>
		/// Gets the identifier of the collection that was shared.
		/// <b>Note:</b> This property is only valid for notifications of type <b>NotificationType.CollectionShared</b>.
		/// </summary>
		public string SharedCollectionID
		{
			get { return sharedCollectionID; }
		}

		/// <summary>
		/// Gets the name of the collection that was shared.
		/// <b>Note:</b> This property is only valid for notifications of type <b>NotificationType.CollectionShared</b>.
		/// </summary>
		public string SharedCollectionName
		{
			get { return sharedCollectionName; }
		}

		/// <summary>
		/// Gets the type of the collection that was shared.
		/// <b>Note:</b> This property is only valid for notifications of type <b>NotificationType.CollectionShared</b>.
		/// </summary>
		public string SharedCollectionType
		{
			get { return sharedCollectionType; }
		}

		/// <summary>
		/// Gets the time stamp of the notification.
		/// </summary>
		public DateTime TimeStamp
		{
			get { return timeStamp; }
		}

		/// <summary>
		/// Gets the type of the notification.
		/// </summary>
		public NotificationType Type
		{
			get { return type; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Notification factory method that constructs a Notification object from a NodeEventArgs object.
		/// </summary>
		/// <param name="args">The NodeEventArgs object to construct the Notification object from.  The NodeEventArgs
		/// object will be returned while listening to node events on the Notification Log.</param>
		/// <returns>The Notification object for the event.</returns>
		static public Notification NotificationFactory( NodeEventArgs args )
		{
			Notification notification = null;

			Store store = Store.GetStore();
			Collection collection = store.GetCollectionByID( args.Collection );
			if ( collection != null )
			{
				Node node = collection.GetNodeByID( args.Node );
				if ( node != null )
				{
					notification = new Notification( node );
				}
			}			

			return notification;
		}

		#endregion
	}
}
