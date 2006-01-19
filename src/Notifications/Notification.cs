/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Client.Event;
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

		private void processEvent()
		{
			string notificationID = Notification.GetNotificationID( store );

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
						NodeEventArgs neArgs = null;

						if ( args.GetType().Equals( typeof( NodeEventArgs ) ) )
						{
							neArgs = args as NodeEventArgs;
						}

						Notification notification;
						if ( notificationID != null )
						{
							// Don't process events for the notification log collection.
							if ( neArgs != null && notificationID.Equals( neArgs.Collection ) )
							{
								continue;
							}

							notification = new Notification( store.GetCollectionByID( notificationID ) );
						}
						else
						{
							notification = new Notification( store, "NotificationLog", Guid.NewGuid().ToString(), "NotificationLog", store.LocalDomain );
							notificationID = notification.ID;
						}

						if ( neArgs != null )
						{
							notification.processNodeEvent( neArgs );
						}
						else
						{
							notification.processSyncEvent( args );
						}
					}
					else
					{
//						log.Info( "Lost notification for node ID = '{0}'.  Event type = '{1}'.  Node type = '{2}'.", args.Node, args.EventType, args.Type );
					}
				}
			}
		}

		private void storeEvents_NodeEvent( NodeEventArgs args )
		{
			lock ( eventQueue )
			{
				eventQueue.Enqueue( args );

				queueEvent.Set();
			}
		}

		private void syncEvents_CollectionSync(CollectionSyncEventArgs args)
		{
			lock ( eventQueue )
			{
				eventQueue.Enqueue( args );

				queueEvent.Set();
			}
		}

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
			notificationThread.IsBackground = true;
			notificationThread.Start();

			storeEvents = new EventSubscriber();
			storeEvents.NodeChanged += new NodeEventHandler( storeEvents_NodeEvent );
			storeEvents.NodeCreated += new NodeEventHandler( storeEvents_NodeEvent );
			storeEvents.NodeDeleted += new NodeEventHandler( storeEvents_NodeEvent );

			// TODO: need to add handlers for sync events.
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
	/// The Notification types supported.
	/// </summary>
	[Flags]
	public enum NotificationType : short
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

	public class Notification : Collection
	{
		#region Class Members

//		Store store;
//		Collection collection;
		static string lastNodeModified = null;
		static Hashtable initialSyncCollections = new Hashtable();

		bool commitCollection = false;
		NotificationType notificationBitMask;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor for creating an existing Notification object from a ShallowNode.
		/// </summary>
		/// <param name="store">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		public Notification( Store store, ShallowNode shallowNode ) :
			base( store, shallowNode )
		{
			Property property = Properties.GetSingleProperty( "NotifyBitMask" );
			if ( property == null )
			{
				// Create the default policy (log all notifications).
				notificationBitMask = NotificationType.CollectionShared | NotificationType.ConflictOccurred | NotificationType.MemberJoined | NotificationType.SyncFailure_Quota | NotificationType.SyncFailure_ReadOnly;
				Properties.AddNodeProperty( "NotifyBitMask", notificationBitMask );
				commitCollection = true;
			}
			else
			{
				notificationBitMask = (NotificationType)property.Value;
			}
		}

		/// <summary>
		/// Copy constructor for Collection object.
		/// </summary>
		/// <param name="collection">Collection object to construct new Notification object from.</param>
		public Notification( Collection collection ) :
			base( collection )
		{
			Property property = Properties.GetSingleProperty( "NotifyBitMask" );
			if ( property == null )
			{
				// Create the default policy (log all notifications).
				notificationBitMask = NotificationType.CollectionShared | NotificationType.ConflictOccurred | NotificationType.MemberJoined | NotificationType.SyncFailure_Quota | NotificationType.SyncFailure_ReadOnly;
				Properties.AddNodeProperty( "NotifyBitMask", notificationBitMask );
				commitCollection = true;
			}
			else
			{
				notificationBitMask = (NotificationType)property.Value;
			}
		}

		/// <summary>
		/// Constructor to create a new Notification object.
		/// </summary>
		/// <param name="store">Store object that this collection belongs to.</param>
		/// <param name="collectionName">This is the friendly name that is used by applications to describe this object.</param>
		/// <param name="collectionID">The globally unique identifier for this object.</param>
		/// <param name="collectionType">Base type of collection object.</param>
		/// <param name="domainID">The domain that this object is stored in.</param>
		internal Notification( Store store, string collectionName, string collectionID, string collectionType, string domainID ) :
			base( store, collectionName, collectionID, collectionType, domainID )
		{
			// Create the default policy (log all notifications).
			notificationBitMask = NotificationType.CollectionShared | NotificationType.ConflictOccurred | NotificationType.MemberJoined | NotificationType.SyncFailure_Quota | NotificationType.SyncFailure_ReadOnly;
			Properties.AddNodeProperty( "NotifyBitMask", notificationBitMask );
			commitCollection = true;
		}

		/// <summary>
		/// Constructor for creating an existing Notification object.
		/// </summary>
		/// <param name="store">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document that describes a Notification object.</param>
		internal protected Notification( Store store, XmlDocument document ) :
			base( store, document )
		{
			Property property = Properties.GetSingleProperty( "NotifyBitMask" );
			if ( property == null )
			{
				// Create the default policy (log all notifications).
				notificationBitMask = NotificationType.CollectionShared | NotificationType.ConflictOccurred | NotificationType.MemberJoined | NotificationType.SyncFailure_Quota | NotificationType.SyncFailure_ReadOnly;
				Properties.AddNodeProperty( "NotifyBitMask", notificationBitMask );
				commitCollection = true;
			}
			else
			{
				notificationBitMask = (NotificationType)property.Value;
			}
		}

		#endregion

		#region Internal Methods

		internal void processNodeEvent( NodeEventArgs args )
		{
			Node node = null;

//			if ( LogMembers && 
//				args.Type == NodeTypes.MemberType && 
//				args.EventType == EventType.NodeCreated /*&&
//				args.Source.Equals( "Sync" )*/ )
//			{
//				// Store the event.
//				node = storeEvent( "NewMember", args );
//			}

			if ( args.EventType == EventType.NodeCreated )
			{
				// TODO: we need to know if the event comes from sync ... we don't want to generate a notification
				// for an action that the local user performed.
				if ( args.Type == NodeTypes.MemberType /*&& args.Source.Equals( "Sync" )*/ )
				{
					if ( LogMembers && !initialSyncCollections.Contains( args.Collection ) )
					{
						node = storeNodeEvent( "NewMember", args );
					}
				}
				else if ( args.Type == NodeTypes.SubscriptionType )
				{
					if ( LogShared )
					{
						node = storeNodeEvent( "NewShare", args );
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
						if ( mNode.Properties.GetSingleProperty( PropertyTags.Collision ) != null )
						{
							node = storeNodeEvent( "Conflict", args );
							lastNodeModified = null;
						}
					}
				}
				else
				{
					lastNodeModified = args.Node;
				}
			}

			commitChanges( node );
		}

		internal void processSyncEvent( SimiasEventArgs args )
		{
			if ( args.GetType().Equals( typeof( CollectionSyncEventArgs ) ) )
			{
				CollectionSyncEventArgs cseArgs = args as CollectionSyncEventArgs;

				if ( cseArgs.Action.Equals ( Action.StopSync ) )
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

				if ( fseArgs.Status == SyncStatus.PolicyQuota ||
					fseArgs.Status == SyncStatus.ReadOnly )
				{
					commitChanges( storeSyncEvent( "SyncFailure", fseArgs ) );
				}
			}
		}

		#endregion

		#region Properties

		public bool LogCollisions
		{
			get
			{
				return ( notificationBitMask & NotificationType.ConflictOccurred ) == NotificationType.ConflictOccurred;
			}
			set
			{
				if ( value != LogCollisions )
				{
					notificationBitMask ^= NotificationType.ConflictOccurred;

					commitCollection = true;
				}
			}
		}

		public bool LogMembers
		{
			get
			{
				return ( notificationBitMask & NotificationType.MemberJoined ) == NotificationType.MemberJoined;
			}
			set
			{
				if ( value != LogMembers )
				{
					notificationBitMask ^= NotificationType.MemberJoined;

					commitCollection = true;
				}
			}
		}

		public bool LogQuotaFailures
		{
			get
			{
				return ( notificationBitMask & NotificationType.SyncFailure_Quota ) == NotificationType.SyncFailure_Quota;
			}
			set
			{
				if ( value != LogQuotaFailures )
				{
					notificationBitMask ^= NotificationType.SyncFailure_Quota;

					commitCollection = true;
				}
			}
		}

		public bool LogReadOnlyFailures
		{
			get
			{
				return ( notificationBitMask & NotificationType.SyncFailure_ReadOnly ) == NotificationType.SyncFailure_ReadOnly;
			}
			set
			{
				if ( value != LogReadOnlyFailures )
				{
					notificationBitMask ^= NotificationType.SyncFailure_ReadOnly;

					commitCollection = true;
				}
			}
		}

		public bool LogShared
		{
			get
			{
				return ( notificationBitMask & NotificationType.CollectionShared ) == NotificationType.CollectionShared;
			}
			set
			{
				if ( value != LogShared )
				{
					notificationBitMask ^= NotificationType.CollectionShared;

					commitCollection = true;
				}
			}
		}

		#endregion

		#region Private Methods

		private void commitChanges( Node node )
		{
			if ( commitCollection )
			{
				Commit( new Node[] { this, node } );
			}
			else
			{
				Commit( node );
			}
		}

		private Node storeNodeEvent( string type, NodeEventArgs args )
		{
			// TODO: friendly name?
			Node node = new Node( "Notify", Guid.NewGuid().ToString(), "Notification" );

			node.Properties.AddNodeProperty( "NotifyType", type );
			node.Properties.AddNodeProperty( "args.Collection", args.Collection );
			node.Properties.AddNodeProperty( "args.Node", args.Node );
			node.Properties.AddNodeProperty( "args.TimeStamp", args.TimeStamp );
			node.Properties.AddNodeProperty( "args.Type", args.Type );

			return node;
		}

		private Node storeSyncEvent( string type, FileSyncEventArgs args )
		{
			// Search for an existing notification.
			bool logged = false;
			ICSList list = Search( BaseSchema.ObjectName, args.Name, SearchOp.Equal );
			foreach ( ShallowNode sn in list )
			{
				Node lNode = GetNodeByID( sn.ID );
				Property property = lNode.Properties.GetSingleProperty( "args.Status" );
				if ( property != null )
				{
					if ( args.Status.Equals( (SyncStatus)property.Value ) )
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

				node.Properties.AddNodeProperty( "NotifyType", type );
				node.Properties.AddNodeProperty( "args.Collection", args.CollectionID );
				node.Properties.AddNodeProperty( "args.TimeStamp", args.TimeStamp );
				node.Properties.AddNodeProperty( "args.Status", args.Status );
			}

			return node;
		}

		#endregion

		#region Public Methods

		static public string GetNotificationID( Store store )
		{
			string notificationID = null;

			// Get the notifications collection.
			ICSList list = store.GetCollectionsByType( "NotificationLog" );
			if ( list.Count > 1 )
			{
				// Shouldn't have more than one notification collection.
				throw new SimiasException( "Multiple notification collections exist!" );
			}

			foreach ( ShallowNode sn in list )
			{
				notificationID = sn.ID;
				break;
			}

			return notificationID;
		}

		#endregion
	}
}
