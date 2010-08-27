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
*                 $Author: Russ Young
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
using System.Net;
using Simias;
using Simias.Sync.Http;
using Simias.Storage;
using Simias.Service;
using Simias.Event;
using Simias.Client;
using Simias.Client.Event;
using Simias.DomainServices;



namespace Simias.Sync
{
	#region StartSyncStatus

	/// <summary>
	/// 
	/// </summary>
	public enum StartSyncStatus : byte
	{
		/// <summary>
		/// We need to sync.
		/// </summary>
		Success,
		/// <summary>
		/// There is nothing to do.
		/// </summary>
		NoWork,
		/// <summary>
		/// The collection was not found.
		/// </summary>
		NotFound,
		/// <summary>
		/// Someone is sync-ing now come back latter.
		/// </summary>
		Busy,
		/// <summary>
		/// The user is not authenticated.
		/// </summary>
		AccessDenied,
		/// <summary>
		/// The collection is locked.
		/// </summary>
		Locked,
		/// <summary>
		/// The user has not been authenticated.
		/// </summary>
		UserNotAuthenticated,
        /// <summary>
        /// The collection has been moved from this store tp other one
        /// </summary>
        Moved,
		/// <summary>
		/// The collection has been disabled for sync
		/// </summary>
		SyncDisabled,
	};

	#endregion

	#region SyncOperation

	/// <summary>
	/// Sync operation.
	/// </summary>
	public enum SyncOperation : byte
	{
		/// <summary>
		/// The node exists but no log record has been created.
		/// Do a brute force sync.
		/// </summary>
		Unknown,
		/// <summary>
		/// Node object was created.
		/// </summary>
		Create,
		/// <summary>
		/// Node object was deleted.
		/// </summary>
		Delete,
		/// <summary>
		/// Node object was changed.
		/// </summary>
		Change,
		/// <summary>
		/// Node object was renamed.
		/// </summary>
		Rename
	};

	#endregion
	/// <summary>
	/// Exception when sync is fataly aborted.
	/// </summary>
	
	    public class SyncAbortedException : Exception
	    {
		public SyncAbortedException() {}
		public SyncAbortedException(string message) : base(message) {}
		public SyncAbortedException(string message, System.Exception inner) : base(message, inner) {}
	    }

	#region SyncClient

	/// <summary>
	/// Class that manages the synchronization for all collection.
	/// </summary>
	public class SyncClient : Simias.Service.IThreadService
	{
		#region Fields

		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncClient));
		Store				store;
		static Hashtable	collections;
		EventSubscriber		storeEvents;
		Queue				syncQueue;
		Queue				priorityQueue;
		ManualResetEvent	queueEvent;
		Thread				syncThread;
		bool				shuttingDown;
		bool				paused;
		static EventPublisher	eventPublisher = new EventPublisher();
        public static string currentiFolderID = null;
        static Object lockobj = new Object();

		#endregion

		#region public methods.
        /// <summary>
        /// Get/Set Method for Current iFolder ID
        /// </summary>
        public static string CurrentiFolderID
        {
            get
            {
                return currentiFolderID;
            }
            set
            {
                lock (lockobj)
                {
                    currentiFolderID = value;
                }
            }
        }

		/// <summary>
		/// Called by The CollectionSyncClient when it is time to run another sync pass.
		/// </summary>
		/// <param name="collectionClient">The client that is ready to sync.</param>
		public void TimeToSync(object collectionClient)
		{
			lock (syncQueue)
			{
				// Queue the sync.
				CollectionSyncClient cc = collectionClient as CollectionSyncClient;
				if (cc.HighPriority)
				{
					if (!priorityQueue.Contains(cc))
						priorityQueue.Enqueue(cc);
				}
				else
				{
					if (!syncQueue.Contains(cc) && !priorityQueue.Contains(cc))
						syncQueue.Enqueue(cc);
				}
				queueEvent.Set();
			}
		}

		#region static methods

		/// <summary>
		/// Returns the number of bytes to sync to the server.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <param name="fileCount">Returns the number of nodes to sync to the server.</param>
		public static void GetCountToSync(string collectionID, out uint fileCount)
		{
			CollectionSyncClient sc = GetCollectionSyncClient(collectionID);
			if (sc != null)
				sc.GetSyncCount(out fileCount);
			else
			{
				fileCount = 0;
			}
		}

		/// <summary>
		/// Call to schedule the collection to sync.
		/// </summary>
		/// <param name="collectionID">The collection to sync.</param>
		public static void ScheduleSync(string collectionID)
		{
			CollectionSyncClient sc = GetCollectionSyncClient(collectionID);
			if (sc != null)
			{
				log.Debug("Rescheduling for sync:");
				sc.Reschedule(true);
			}
		}

        /// <summary>
        /// Called to schedule a sync operation for all collections and domain. done immediately after login
        /// </summary>
        public static void RescheduleAllColSync(string DomainID)
        {
            // Reschedule all the iFolders of the Domain for sync.....
            Store store = Store.GetStore();
            if (store != null)
            {
                ICSList cList = store.GetCollectionsByDomain(DomainID);
                foreach (ShallowNode sn in cList)
                {
                    SyncClient.ScheduleSync(sn.ID);
                }
            }
        }

        public static void RescheduleAllEncryptedColSync(string DomainID)
        {
            // Reschedule all the iFolders of the Domain for sync.....
            Store store = Store.GetStore();
            if (store != null)
            {
                ICSList cList = store.GetCollectionsByDomain(DomainID);
                foreach (ShallowNode sn in cList)
                {
                    try
                    {
                        Collection col = store.GetCollectionByID(sn.ID);
                        if (col.EncryptionAlgorithm != null && col.EncryptionAlgorithm != string.Empty)
                            SyncClient.ScheduleSync(col.ID);
                    }
                    catch (Exception ex)
                    {
                        log.Debug("RescheduleAllEncryptedColSync: Error scheduling {0} for sync.", sn.ID);
						log.Debug("Exception.Message: {0}, StackTrace: {1}", ex.Message, ex.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// Reset the connection for the domain
        /// </summary>
        /// <param name="DomainID">ID of the domain</param>
		public static void ResetConnections(string DomainID)
		{
			log.Debug("In Reset connections for domain: {0}. This from the logout event man.", DomainID);
			Store store = Store.GetStore();
			if( store == null)
			{
				store = Store.GetStore();
			}
			if( store == null)
				return;
			ICSList iFolderList = store.GetCollectionsByType("iFolder");
			if( iFolderList == null)
				return;
			foreach( ShallowNode sn in iFolderList)
			{
		                try
                		{
					log.Debug("Trying to reset connection for: {0}. sleeping for 50 secs.", sn.ID);
					DomainAgent domainAgent = new DomainAgent();
					bool stat = domainAgent.IsDomainAuthenticated(DomainID);
					if( stat == true)
					{
						log.Debug("The domain is authenticated. Logged-in again. no need to resetconnections.");
						break;
					}
					CollectionSyncClient sc = GetCollectionSyncClient(sn.ID);
					if (sc == null)
						continue;
					int count = 20;
					if (CurrentiFolderID == sn.ID)
					{
						log.Debug("Suspending the Sync for {0}", sn.ID);
						/// check the authenticated status of the domain...
						sc.Suspend();
						do
						{
							log.Debug("Waiting for suspending the sync.");
							if (CurrentiFolderID == null || CurrentiFolderID != sn.ID)
							break;
							Thread.Sleep(1000);
							count--;
						} while (count > 0);
					}
					sc.ResetConnection();
					log.Debug("Resetted connection for: {0}", sn.ID);
				}
				catch (Exception ex)
				{
					log.Debug("Exception in Resetconnections: {0}--{1}", ex.Message, ex.StackTrace);
				}
			}
			log.Debug("Out of reset connections for donain: {0}", DomainID);
		}

		/// <summary>
		/// Get the last time the Collection was in sync.
		/// </summary>
		/// <param name="collectionID">The collection to query.</param>
		/// <returns>The last time the collection was in sync.</returns>
		public static DateTime GetLastSyncTime(string collectionID)
		{
			CollectionSyncClient sc = GetCollectionSyncClient(collectionID);
			if (sc != null)
				return sc.GetLastSyncTime();
			return DateTime.MinValue;
		}

		/// <summary>
		/// Stop the 
		/// </summary>
		/// <param name="collectionID"></param>
		public static void Stop(string collectionID)
		{
			CollectionSyncClient sc = GetCollectionSyncClient(collectionID);
			if (sc != null)
				sc.Stop();
		}

        /// <summary>
        /// Stop the 
        /// </summary>
        /// <param name="collectionID"></param>
        public static void Suspend(string collectionID)
        {
            CollectionSyncClient sc = GetCollectionSyncClient(collectionID);
            if (sc == null || CurrentiFolderID != collectionID)
                return;

            sc.Suspend();
            int count = 20;
            do
            {
                log.Debug("Waiting for suspending the sync.");
                if (CurrentiFolderID == null || CurrentiFolderID != collectionID)
                    break;
                Thread.Sleep(1000);
                count--;
            } while (count > 0);
            log.Debug("Out of suspend...");
        }

		/// <summary>
		/// Gets the state of the server for this collection.
		/// </summary>
		/// <param name="collectionID">The collection to check.</param>
		/// <returns>True if the server is unavailable</returns>
		public static bool ServerUnavailable(string collectionID)
		{
			CollectionSyncClient sc = GetCollectionSyncClient(collectionID);
			if (sc != null)
				return !sc.Connected;
			return false;
		}
	
		#endregion
		#endregion

		#region private methods.

		/// <summary>
		/// Gets the CollectionSyncClient object for the specified collection.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <returns></returns>
		private static CollectionSyncClient GetCollectionSyncClient(string collectionID)
		{
			CollectionSyncClient sc;
			lock (collections)
			{
				sc = (CollectionSyncClient)collections[collectionID];
			}
			return sc;
		}

		/// <summary>
		/// The main synchronization thread.
		/// </summary>
		private void StartSync()
		{
			// Used to not starve the normal queue.
			int	 priorityCount = 0;
			while (!(Simias.Service.Manager.ShuttingDown || shuttingDown))
			{
				// Wait for something to be added to the queue.
				queueEvent.WaitOne();

				// Now loop until the queue is emptied.
				while (true)
				{
					CollectionSyncClient cClient;
					lock (syncQueue)
					{
						if (priorityQueue.Count != 0 && (priorityCount < 3 || syncQueue.Count == 0))
						{
							cClient = priorityQueue.Dequeue() as CollectionSyncClient;
							priorityCount++;
						}
						else
						{
							priorityCount = 0;
							if (syncQueue.Count == 0)
							{
								queueEvent.Reset();
								break;
							}
							cClient = syncQueue.Dequeue() as CollectionSyncClient;
						}
					}
                    if (!(Simias.Service.Manager.ShuttingDown || shuttingDown) && !paused)
                    {



                        bool serverWasConnected = cClient.Connected;
                        // Sync this collection now.
                        if (serverWasConnected)
                            log.Debug("{0} : Starting Sync.", cClient);
                        try
                        {
                            if (cClient.SyncEnabled)
                            {
                                CurrentiFolderID = cClient.CollectionID;
                                cClient.SyncNow();
                                CurrentiFolderID = null;
                                log.Info("{0} : Finished Sync.", cClient);
                            }
			
                            else
                            {
				cClient.isSyncDisabled = true;
				eventPublisher.RaiseEvent(new CollectionSyncEventArgs(cClient.collection.Name, cClient.collection.ID, Simias.Client.Event.Action.DisabledSync, true, false));
                                log.Info("{0} : Sync Disabled.", cClient);
                            }
			
                        }
                        catch (NeedCredentialsException ex)
                        {
                            CurrentiFolderID = null;
                            log.Debug(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            CurrentiFolderID = null;
                            if (!cClient.Connected)
                            {
                                if (serverWasConnected)
                                    log.Info("Server for {0} is unavailable. {0}--{1}", cClient, ex.Message, ex.StackTrace);
                            }
                            else
                            {
                                log.Error(ex, "Finished Sync. Error =  {0}", ex.Message);
                            }
                        }
                        try
                        {
                            cClient.Reschedule(false);
                        }
                        catch { /* If we could not reschedule this collection will no longer sync. */};


                    }
				}
			}
		}

		/// <summary>
		/// Called when a node is created.
		/// </summary>
		/// <param name="args">Arguments of the create.</param>
		private void storeEvents_NodeCreated(NodeEventArgs args)
		{
			// If the ID matched the Collection this is a collection.
			if (args.ID == args.Collection)
			{
				lock (collections)
				{
					// Add this to the collections to sync if not already added.
					if (!collections.Contains(args.ID))
					{
						collections.Add(args.ID, new CollectionSyncClient(args.ID, new TimerCallback(TimeToSync)));
					}
				}
			}
			else if (args.Type == NodeTypes.PolicyType)
			{
				PolicyChanged();
			}
		}

		/// <summary>
		/// Called when a Node is deleted.
		/// </summary>
		/// <param name="args">Arguments of the delete.</param>
		private void storeEvents_NodeDeleted(NodeEventArgs args)
		{
			// If the ID matched the Collection this is a collection.
			if (args.ID == args.Collection)
			{
				lock (collections)
				{
					// Remove the CollectionSyncClient.
					CollectionSyncClient client = (CollectionSyncClient)collections[args.ID];
					if (client != null)
					{
						client.Stop();
						collections.Remove(args.ID);
						client.Dispose();
					}
				}
			}
			else if (args.Type == NodeTypes.PolicyType)
			{
				PolicyChanged();
			}
		}

		/// <summary>
		/// Called when a Node has been modified.
		/// </summary>
		/// <param name="args">Arguments of the modifiction.</param>
		private void storeEvents_NodeChanged(NodeEventArgs args)
		{
			if (args.Type == NodeTypes.PolicyType)
			{
				PolicyChanged();
			}
		}

		/// <summary>
		/// Called when a plolicy has been changed.
		/// </summary>
		private void PolicyChanged()
		{
			lock (collections)
			{
				foreach (CollectionSyncClient sc in collections.Values)
				{
					sc.PolicyChanged();
				}
			}
		}

		#endregion

		#region IThreadService Members

		/// <summary>
		/// Starts the Sync client.
		/// </summary>
		/// <param name="conf">The configuration object to use.</param>
		public void Start()
		{
			shuttingDown = false;
			paused = false;
			store = Store.GetStore();

			// Subscribe for node events so that we can add new CollectionSyncClients.
			storeEvents = new EventSubscriber();
			storeEvents.NodeCreated +=new NodeEventHandler(storeEvents_NodeCreated);
			storeEvents.NodeDeleted +=new NodeEventHandler(storeEvents_NodeDeleted);
			storeEvents.NodeChanged +=new NodeEventHandler(storeEvents_NodeChanged);

			syncQueue = new Queue();
			priorityQueue = new Queue();
			queueEvent = new ManualResetEvent(false);
			collections = new Hashtable();

			lock (collections)
			{
				foreach(ShallowNode sn in store)
				{
					collections.Add(sn.ID, new CollectionSyncClient(sn.ID, new TimerCallback(TimeToSync)));
				}
			}

			// Start the Sync Thread.
			syncThread = new Thread(new ThreadStart(StartSync));
            syncThread.Name = "Client Sync";
            syncThread.Priority = ThreadPriority.Normal;
			syncThread.Start();
		}

		/// <summary>
		/// Resumes a paused sync.
		/// </summary>
		public void Resume()
		{
			foreach(CollectionSyncClient cClient in collections.Values)
			{
				cClient.Reschedule(true);
			}
		}

		/// <summary>
		/// Pauses the synchronization service.
		/// </summary>
		public void Pause()
		{
			paused = true;
			foreach(CollectionSyncClient cClient in collections.Values)
			{
				cClient.Stop();
			}
		}

		/// <summary>
		/// Used to send custom control messages to the service.
		/// Not used.
		/// </summary>
		/// <param name="message">The custom message.</param>
		/// <param name="data">The data of the message.</param>
		public int Custom(int message, string data)
		{
			return 0;
		}

		/// <summary>
		/// Called to stop the synchronization service.
		/// </summary>
		public void Stop()
		{
			shuttingDown = true;
			lock (collections)
			{
				foreach(CollectionSyncClient cClient in collections.Values)
				{
					cClient.Stop();
				}
				collections.Clear();
			}
			lock (syncQueue)
			{
				queueEvent.Set();
			}
			syncThread.Join();
			storeEvents.Dispose();
		}

		#endregion

	}

	#endregion

	#region CollectionSyncClient

	/// <summary>
	/// Class that implements the synchronization logic for a collection.
	/// </summary>
	public class CollectionSyncClient
	{
		#region fields

		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(CollectionSyncClient));
		static EventPublisher	eventPublisher = new EventPublisher();
		HttpSyncProxy	service;
		SyncWorkArray	workArray;
		Store			store;
		public Collection		collection;
		bool			serverAlive = true;
		bool			CollSyncStatus = true;
		StartSyncStatus	serverStatus;
		Timer			timer;
		TimerCallback	callback;
		FileWatcher		fileMonitor;
		public bool			stopping;
		Access.Rights	rights;
		string			serverContext;
		string			clientContext;
		static int		BATCH_SIZE = 50;
		private const string	ServerCLContextProp = "ServerCLContext";
		private const string	ClientCLContextProp = "ClientCLContext";
		private Object syncObject = new Object();

		static int		initialSyncDelay = 10 * 1000; // 10 seconds.
		DateTime		syncStartTime; // Time stamp when sync was called.
		const int		timeSlice = 3; //Timeslice in minutes.
		SyncFile		syncFile;
		bool			firstSync = true;
		bool			yielded = false;
        bool resetSync = false;
		DateTime		lastSyncTime = DateTime.MinValue;
		public static bool	running = false;

		Thread			scanThread;
		public bool	        isSyncDisabled = false;
        

                public enum StateMap : uint
                {
                        unchanged = 0x00000000,
            		DomainSyncOnce = 0x00000001,
            		DomainSyncStarted = 0x00000002,
            		DomainSyncFinished = 0x00000004,
            		CatalogSyncOnce = 0x00000010,
            		CatalogSyncStarted = 0x00000020,
                        CatalogSyncFinished = 0x00000040
                }

        	public static StateMap SyncStateMap = StateMap.unchanged;

		/// <summary>
		/// Returns true if we should yield our timeslice.
        /// why 3 minutes? What if we do not yield?
        /// Possibly a large ifolder might take all time...
		/// </summary>
		bool Yield
		{
			get 
			{
                if (!SyncEnabled)
                {
                    return false;
                }
                if (stopping || resetSync)
                {
                    return true;
                }
				TimeSpan ts = DateTime.Now - syncStartTime;
				if (ts.Minutes > timeSlice)
				{
					yielded = true;
					return true;
				}
				return false;
			}
		}

        /// <summary>
        /// Get the collection ID
        /// </summary>
        public string CollectionID
        {
            get
            {
                return this.collection.ID;
            }
        }

        /// <summary>
        /// Whether Synchronization enabled or not
        /// Whether Synchronization enabled or not
        /// </summary>
        internal bool SyncEnabled
        {
            get
            {
                // In the server if value is false , then SYnc Enabled is true for that iFolder ( return !cinfo.Disabled ). If value is not set on server then return true .
                CollectionInfo cinfo = Discovery.DiscoveryFramework.GetLocalCollectionInfo(collection.ID);

		        if( cinfo != null )
	                        return !cinfo.Disabled;
		        return true;
            }
        }

        /// <summary>
        /// Whether high priority or not
        /// </summary>
		internal bool HighPriority
		{
			get 
			{ 
				// If the server is not responding make sure that we are not high priority.
				if (!serverAlive)
					return false;
				
				return collection.Priority == 0 ? true : false; 
			}
		}

		#endregion
	
		#region Constructor / Finalizer
		/// <summary>
		/// Construct a new CollectionSyncClient.
		/// </summary>
		/// <param name="nid">The node ID.</param>
		/// <param name="callback">Callback that is called when collection should be synced.</param>
		public CollectionSyncClient(string nid, TimerCallback callback)
		{
			store = Store.GetStore();
			collection = store.GetCollectionByID(nid);
			this.callback = callback;
			stopping = false;
			Initialize();
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~CollectionSyncClient()
		{
			Dispose(true);
		}

		#endregion

		#region public Methods.

		/// <summary>
		/// Returns the name of the collection.
		/// </summary>
		/// <returns>Name of the collection.</returns>
		public override string ToString()
		{
			return collection.Name;
		}

		#endregion

		#region internal Methods.

		/// <summary>
		/// Called to dispose the CollectionSyncClient.
		/// </summary>
		internal void Dispose()
		{
			Dispose(false);
		}

		/// <summary>
		/// Get whether the server is connected.
		/// </summary>
		internal bool Connected
		{
			get { return serverAlive; }
		}

		/// <summary>
		/// Reschedule the sync
		/// </summary>
		/// <param name="syncNow">Whether to sync immediately or wait for some time</param>
		public void Reschedule(bool syncNow)
		{
			if (syncNow) Reschedule(true, 0);
			else Reschedule(false, 0);
		}
		
        /// <summary>
        /// Find whether server alive or not
        /// </summary>
        /// <returns>True if server is alive else false</returns>
		bool GetServerAliveStatus()
		{
			bool stat = false;
			DomainAgent domainAgent = new DomainAgent();
			stat = domainAgent.IsDomainAuthenticated(collection.Domain);
			if( stat)
			{
				// Check whether webservices are running...
				stat = domainAgent.Ping(collection.Domain);
			}
			log.Debug("GetServerAliveStatus: returned: {0}", stat.ToString());
			return stat;
		}
		
        /// <summary>
        /// Reset the connection service
        /// </summary>
		public void ResetConnection()
		{
			if( service == null)
			{
				log.Debug("The service object is null. no need to reset connection");
				return;
			}
			service.ResetConnection();
		}

		/// <summary>
		/// Called to schedule a sync operation a the set sync Interval.
		/// </summary>
		/// <param name="overridePolicy"></param>
		/// <param name="delay"></param>
		public void Reschedule(bool overridePolicy, int delay)
		{
			if (!stopping)
			{
                		int seconds;
				if( !serverAlive )
				{
					// Check whether the server is up and logged-in by now...
					serverAlive = GetServerAliveStatus();
				}
				// If we had to yield put ourselves back on the queue.
				if (Yield)
				{
                    			if (resetSync)
                    			{
                        			log.Debug("Unsetting the resetSync flag.");
                        			resetSync = false;
                        			seconds = 5;
                    			}
                    			else
                        			seconds = 0;
				}
				else if ( !serverAlive || isSyncDisabled ) 
				{
                    			//wait for 5mins for local collections, sync disabled  and also if server is down
					seconds = 300;
					isSyncDisabled = false;
				}
				else 
				{
					seconds = overridePolicy ? delay : collection.Interval;
					if (serverStatus == StartSyncStatus.Busy)
					{
						// Reschedule to sync within 30 seconds, but no less than 10 seconds.
						seconds = new Random().Next(20) + 10;
					}
				}

				timer.Change(seconds * 1000, Timeout.Infinite);
				yielded = false;//dredge the collection
			}
		}

		/// <summary>
		/// Get the last time the collection was in sync.
		/// </summary>
		/// <returns></returns>
		internal DateTime GetLastSyncTime()
		{
			if (lastSyncTime != DateTime.MinValue)
				return lastSyncTime;

			Property cc = collection.Properties.GetSingleProperty(ClientCLContextProp);
			if (cc != null)
			{
				return (new EventContext(cc.Value.ToString()).TimeStamp);
			}
			return DateTime.MinValue;
		}

		/// <summary>
		/// Called to stop this instance from sync-ing.
		/// </summary>
		internal void Stop()
		{
			timer.Change(Timeout.Infinite, Timeout.Infinite);
			lock (this)
			{
				stopping = true;
				if (syncFile != null)
					syncFile.Stop = true;
			}

			if(fileMonitor.scanThreadRunning == true)
				scanThread.Abort();
				//scanThread.Join();
		}

        /// <summary>
        /// Suspend the thread for yielding
        /// </summary>
        internal void Suspend()
        {
            log.Debug("Calling suspend for: {0}. Yielding the sync thread.", this.collection.ToString());
            lock (this)
            {
                resetSync = true;
                if (syncFile != null)
                {
                    syncFile.Stop = true;
                    log.Debug("Currently syncing: {0}. This will be stopped", syncFile.Name);
                }
            }

            if (fileMonitor.scanThreadRunning == true)
                scanThread.Abort();
            log.Debug("Exiting suspend method for: {0}", this.collection.ToString());
        }

		/// <summary>
		/// Get the number of bytes to sync.
		/// </summary>
		/// <param name="fileCount">Returns the number of files to be synced.</param>
		internal void GetSyncCount(out uint fileCount)
		{
			fileCount = (uint)workArray.Count;
		}

		/// <summary>
		/// Get Collection sync status
		/// </summary>
		internal bool GetCollectionSyncStatus()
		{
			return CollSyncStatus;
		}

		/// <summary>
		/// Called to notify that a policy has changed.
		/// </summary>
		internal void PolicyChanged()
		{
			firstSync = true;
		}


		/// <summary>
		/// Checks if the collection is a LocalDomain, LocalDatabase or a NotificationLog type.
		/// </summary>
		private static bool IsLocalDomainOrDb (string ID)
		{
			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(ID);

			return (col.IsType (NodeTypes.LocalDatabaseType) ||  //LocalDatabase
			       (col.IsBaseType (NodeTypes.DomainType) && col.Role == SyncRoles.Local) || //Local Domain
				col.IsBaseType (NodeTypes.NotificationLogType)); //Notification Logs.
		}

		/// <summary>
		/// Called to synchronize this collection.
		/// </summary>
		public void SyncNow()
		{
			log.Debug("In sync now...");
			// Assume the server is alive.
			bool sAlive = false;
			bool nopassphrase = false;

			//Note : Suppress events emitted for LocalDatabase and Local domain when simias is running as client.
			bool publishEvent = (!Store.IsEnterpriseServer && !IsLocalDomainOrDb (collection.ID));
			try
			{
				if (publishEvent)
					eventPublisher.RaiseEvent(new CollectionSyncEventArgs(collection.Name, collection.ID, Simias.Client.Event.Action.StartLocalSync, true, false));

				syncStartTime = DateTime.Now;
				running = true;
				bool isNewifolder = false;
				serverStatus = StartSyncStatus.Success;
				// Refresh the collection.
				collection.Refresh();
				log.Debug("Refreshing the collection...");
				Member currentMember = collection.GetCurrentMember();

				if(this.collection.DataMovement != true)
				{
			                if (this.collection.EncryptionAlgorithm != null && this.collection.EncryptionAlgorithm != string.Empty)
        			        {
		                	    log.Info("Syncing an encrypted iFolder");
	        		            Store store = Store.GetStore();
		        	            string Passphrase = store.GetPassPhrase(collection.Domain);
                			    if (Passphrase == null)
			                    {
        			                log.Info("Passphrase not provided, will not sync");
                	        		eventPublisher.RaiseEvent(new CollectionSyncEventArgs(collection.Name, collection.ID, Simias.Client.Event.Action.NoPassphrase, true, false));
		                        	nopassphrase = true;
	        		                return;
		        	            }
                			}
			                else
        			            log.Info("Syncing regular ifolder");
				}
				// Make sure the master exists.
				if (collection.CreateMaster)
				{
					isNewifolder = true;
					new DomainAgent().CreateMaster(collection);
				}
				if (firstSync)
				{
					if (collection.Role == SyncRoles.Slave)
					{
						workArray.SetAccess = currentMember.Rights;
				
						// We are just starting add all the modified nodes to the array.
						// Get all nodes from store that have not been synced.
						ICSList updateList = collection.Search(PropertyTags.NodeUpdateTime, DateTime.Now, SearchOp.Exists);
						log.Debug("Found {0} nodes that have NodeUpdate Time", updateList.Count);
						foreach (ShallowNode sn in updateList)
						{
							workArray.AddNodeToServer(new SyncNodeInfo(collection, sn));
						}
					}
					firstSync = false;
				}
				bool firstSyncAfterClientUp =  fileMonitor.IsFirstSyncAfterClientUp();
				
				// Only syncronize local changes when we have finished with the 
				// Server side changes.
				if ((workArray == null || workArray.DownCount == 0) && !yielded && collection.Merge == false && fileMonitor.scanThreadRunning == false && collection.DataMovement != true)
				{
					if(isNewifolder == true)
					{
						log.Debug("new iFolder start the scan thread");
						scanThread = new Thread(new ThreadStart(fileMonitor.CheckForFileChanges));
						scanThread.IsBackground = true;
						scanThread.Priority = ThreadPriority.BelowNormal;
						scanThread.Name = collection.Name + " Scan";
						//if(firstSyncAfterClientUp == false)//second time
						//	scanThread.Start();
						///We get all new entries in the collection and set the cookie, after the reconcile and before setting the cookie, the scanned entries are missed, 
						/// so postphone the start till the rconcile gets over
					}	
					else
					{
						log.Debug("do the scan and start the sync");
						fileMonitor.CheckForFileChanges();
					}
				}
				// Reset the sync time so that we only yield for network time.
				syncStartTime = DateTime.Now;
				yielded = false;
				if (collection.Role != SyncRoles.Slave)
				{
					CollSyncStatus = false;
					return;
				}

				// Setup the url to the server.
				string userID = currentMember.UserID;
				string userName = currentMember.Name;
				log.Debug("In sync now: collection ID: {0} user Name: {1} User ID: {2}.", collection.ID, userName, userID);
				service = new HttpSyncProxy(collection, userName, userID);

				//SyncNodeInfo[] cstamps;
			
				// Get the current sync state.
				string tempClientContext;
				string tempServerContext;
				GetChangeLogContext(out tempServerContext, out tempClientContext);
				
				log.Debug("got context server {0} client {1} ", tempServerContext, tempClientContext);
				//bool gotClientChanges = this.GetChangedNodeInfoArray(out cstamps, ref tempClientContext);

				// Setup the SyncStartInfo.
				StartSyncInfo si = new StartSyncInfo();
				si.CollectionID = collection.ID;
				si.Context = tempServerContext;
				bool gotClientChanges = !firstSyncAfterClientUp; //this gives if the scan thread is going to dredge or not.
				si.ChangesOnly = gotClientChanges | !workArray.Complete;
				si.ClientHasChanges = si.ChangesOnly;
                
				// Start the Sync pass and save the rights.
				try
				{
					service.StartSync(ref si);
				}
				catch (Exception ex)
				{
					service = null;
			                log.Debug("Error in startsync. Message: {0}", ex.Message);
					throw ex;
				}
                
				sAlive = true;
			
				eventPublisher.RaiseEvent(new CollectionSyncEventArgs(collection.Name, collection.ID, Simias.Client.Event.Action.StartSync, true, false));

				tempServerContext = si.Context;
				workArray.SetAccess = rights = si.Access;
				
				serverStatus = si.Status;
				switch (si.Status)
				{
					case StartSyncStatus.AccessDenied:
						eventPublisher.RaiseEvent(
							new NodeEventArgs(
							"Sync", collection.ID, collection.ID, 
							collection.BaseType, EventType.NoAccess, 
							0, DateTime.Now, collection.MasterIncarnation,
							collection.LocalIncarnation, 0)); 
						log.Info("The user no longer has rights.");
						collection.Commit(collection.Delete());
						break;
					case StartSyncStatus.Locked:
						sAlive = false;
						CollSyncStatus = false;
						log.Info("The collection is locked");
						break;
					case StartSyncStatus.Busy:
						sAlive = false;
						CollSyncStatus = false;
						log.Info("The server is busy");
						break;
					case StartSyncStatus.SyncDisabled:
						log.Info("Collection is disabled.");
						CollSyncStatus = false;
						eventPublisher.RaiseEvent(new CollectionSyncEventArgs(collection.Name, collection.ID, Simias.Client.Event.Action.DisabledSync, true, false));
						break;
					case StartSyncStatus.NotFound:
                        if (CheckForUserMovement(collection.ID, collection.HostID, userID, currentMember.GetDomainID(store)) == 0)
                        {
                            eventPublisher.RaiseEvent(
                                new NodeEventArgs(
                                "Sync", collection.ID, collection.ID,
                                collection.BaseType, EventType.NoAccess,
                                0, DateTime.Now, collection.MasterIncarnation,
                                collection.LocalIncarnation, 0));

                            log.Info("The collection no longer exists");
                            // The collection does not exist or we do not have rights.
                            collection.Commit(collection.Delete());
                        }
						break;
						case StartSyncStatus.Moved:
						string MovedToHostID = si.HostID;
						bool UserMovedStatus = CheckForUserMovement(MovedToHostID, userID, currentMember.GetDomainID(store));
						if (UserMovedStatus == true)
						{
						    log.Debug("User had been moved to new server so updated local hostID");
						}
						break;
					case StartSyncStatus.UserNotAuthenticated:
						log.Debug("The user could not be authenticated");
						CollSyncStatus = false;
						break;						
					case StartSyncStatus.NoWork:
						//This case will not encounter since scanning is being done parallel now
						CollSyncStatus = false;
						log.Debug("Nothing to get from server");
						break;
					case StartSyncStatus.Success:
					switch (rights)
					{
						case Access.Rights.Deny:
							break;
						case Access.Rights.Admin:
						case Access.Rights.ReadOnly:
						case Access.Rights.ReadWrite:
							try
							{							
								lock(syncObject)
								{
									SyncNodeInfo[] cstamps;
									bool moreEntries = false;
									while (true)
									{
										if (!si.ChangesOnly)
										{
											// We don't have any state. So do a full sync.
											//we will get back the context so that next sync cycle/loop can get the changes
											ReconcileAllNodeStamps(ref tempClientContext);

											// since dredge would have started we need not reconcile again, instead get the
											// changed nodes created by the file monitor.
											si.ChangesOnly = true;
											
											//we start here for the first sync after client up, this condition will occur for all ifolders after the client start
											if(scanThread !=null)
												scanThread.Start();
										}
										else
										{
											// We only need to look at the changed nodes.
											gotClientChanges = GetChangedNodeInfoArrayIncrements(out cstamps, ref tempClientContext, ref moreEntries);
											ProcessChangedNodeStamps(cstamps, ref tempServerContext);
											cstamps = null;
										}

										
										// we are not queuing anymore as we expect all to be done once started
										if (!ServerIs36())										
											AddMemberNodeFromServer();
										
										ExecuteSync();

										// Cache the sync state.
										log.Debug("caching context server {0} client {1} ", tempServerContext, tempClientContext);
										SetChangeLogContext(tempServerContext, tempClientContext, false);

										if(/*workArray.Count == 0 && */moreEntries ==false && fileMonitor.scanThreadRunning == false)
										{
											//we are done with the workarray and the scan thread is no more
											//it is not guarantee that the scan complete means work complete???
											// but see GetChangedNodeInfoArrayIncrements on how we manage scan and work complete
											break;
										}

										// this check is to ensure that we are yielding
										// In this case scanthread is not expected to join the sync thread, so not checking for
                                        // scanThreadRunning status
										if (yielded == true)
											break;
									}
									if(collection.Merge == true)
									{
										//remove the merge property, no longer required since the sync for this collection 
                                       					 //got over
										collection.Merge = false;
										collection.Commit();
                                        //TODO: we need to schedule an immediate sync instead of calling the same recursively
										SyncNow();
									}

								}
							}
							catch (SyncAbortedException ex)
							{
							    if (ex.InnerException != null && ex.InnerException is UnauthorizedAccessException) {
								log.Info("UnauthorizedAccessException occured: Collection not committed. Detailed StackTrace is {0} ", 
									 ex.InnerException.StackTrace.ToString());
							    }
							}
							finally
							{

								bool status = workArray.Complete;
								if (status)
									lastSyncTime = DateTime.Now;

								log.Debug("caching context with status {2} server {0} client {1} ", tempServerContext, tempClientContext, status);
								// Save the sync state permanently
								SetChangeLogContext(tempServerContext, tempClientContext, status);
								
								// End the sync.
								service.EndSync();
                                // wait for the scan thread to complete. Ideally we do not need this
                                // but having it for safety reasons
                                //scanThread.Join();

                                if (!Store.IsEnterpriseServer && ServerIs36())
                                {
                                    // If this client is connected to 3.6 server , then read policies from POBox 
                                    // and store it into member object
                                    SetUserPoliciesFromPOBox();
                                }
                                
							}
							break;
					}
						break;
				}
			}
			finally
			{
				serverAlive = sAlive;
				running = false;
                fileMonitor.ToDredge = false;
                if( !nopassphrase && publishEvent)
                eventPublisher.RaiseEvent(new CollectionSyncEventArgs(collection.Name, collection.ID, Simias.Client.Event.Action.StopSync, sAlive, yielded));
			}
		}

        /// <summary>
        /// returns true/false depending on whether server is 3.6 or not
        /// </summary>
        private bool ServerIs36()
        {
            bool OldServer = false;
            if (service.SyncEngineVersion() == "1.0")
                OldServer = true;
            return OldServer;
        }

        /// <summary>
        /// Called to read user policies from pobox, system policies from store and store aggregate in member object
        /// </summary>
        private void SetUserPoliciesFromPOBox()
        {
            
            //read POBox and fetch policy information and store it in member object.
            string ColType = collection.GetType().ToString();
            bool IsDomainType = ColType.Equals("Simias.Storage.Domain") || ColType.Equals("Domain");
            
            string DomainID = collection.Domain;
            if (DomainID == null || !IsDomainType)
            {
                return;
            }
            Domain d = store.GetDomain(DomainID);
            //Skip local domain
            if (d.Name.Equals(Store.LocalDomainName))
                return;

            Member mem = d.GetCurrentMember();

            string RuleList = "RuleList";

            int SysEncStatus = Simias.Policy.SecurityState.GetStatus(DomainID);
            long SysDiskQuota = Simias.Policy.DiskSpaceQuota.GetLimit(DomainID);
            long UsedDiskSpaceByMemberOnServer = Simias.Policy.DiskSpaceQuota.Get(mem).UsedSpace;


            Simias.Policy.Rule rule = new Simias.Policy.Rule(SysDiskQuota, Simias.Policy.Rule.Operation.Greater, Simias.Policy.Rule.Result.Deny);
            int UserEncryptionValue = SysEncStatus;

            POBox.POBox poBox = POBox.POBox.FindPOBox(store, DomainID, mem.UserID);

            if ( poBox != null )
				{
					ICSList list = poBox.Search( PropertyTags.PolicyID, Simias.Policy.SecurityState.EncryptionStatePolicyID, SearchOp.Equal );
					foreach ( ShallowNode sn in list )
					{
						Simias.Policy.Policy tempPolicy = new Simias.Policy.Policy( poBox, sn );
						if ( tempPolicy.IsSystemPolicy )
						{
                            MultiValuedList mvl = tempPolicy.Properties.GetProperties(Simias.Policy.SecurityState.StatusTag);
                            if (mvl.Count > 0)
                            {
                                foreach (Property p in mvl)
                                {
                                    if (p != null)
                                    {
                                        UserEncryptionValue =(int) p.Value;
                                        break;
                                    }
                                }
                            }
						}
					}

                    
                    ICSList list2 = poBox.Search(PropertyTags.PolicyID, Simias.Policy.DiskSpaceQuota.DiskSpaceQuotaPolicyID, SearchOp.Equal);
                
                    foreach (ShallowNode sn in list2)
                    {
                        Simias.Policy.Policy tempPolicy = new Simias.Policy.Policy(poBox, sn);
                        if (tempPolicy.IsSystemPolicy)
                        {
                            MultiValuedList mvl = tempPolicy.Properties.GetProperties(RuleList);
                            if (mvl.Count > 0)
                            {
                                foreach (Property p in mvl)
                                {
                                    if (p != null)
                                    {
                                        rule = new Simias.Policy.Rule(p.Value);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                
                Property memEncProperty = new Property(Simias.Policy.SecurityState.EncryptionStatePolicyID, UserEncryptionValue);
                memEncProperty.LocalProperty = true;
                mem.Properties.ModifyNodeProperty(memEncProperty);

                Property memDiskProperty = new Property(Simias.Policy.DiskSpaceQuota.DiskSpaceQuotaPolicyID, rule.ToString());
                memDiskProperty.LocalProperty = true;
                mem.Properties.ModifyNodeProperty(memDiskProperty);

                Property memDiskUsedProperty = new Property(Simias.Policy.DiskSpaceQuota.UsedDiskSpaceOnServer, UsedDiskSpaceByMemberOnServer);
                memDiskUsedProperty.LocalProperty = true;
                mem.Properties.ModifyNodeProperty(memDiskUsedProperty);

				}

            d.Commit(mem);
        }

        /// <summary>
        /// Check whether user moved or not based on collection and user
        /// </summary>
        /// <param name="collectionID">ID of the collection</param>
        /// <param name="collectionHostID">ID of the host collection</param>
        /// <param name="UserID">ID of the user to check for user moment</param>
        /// <param name="DomainID">ID of the domain that belongs to user</param>
        /// <returns>1 if user had been moved else -1</returns>
        public int CheckForUserMovement(string collectionID, string collectionHostID, string UserID, string DomainID)
        {
            try
            {
                CatalogInfo CatInfo = Simias.Discovery.CollectionList.GetCatalogInfoItem(collectionID, UserID, DomainID);
                if (CatInfo == null)
                {
                    // Collection has been found deleted, through discovery service too , so return false
                    return 0;
                }
                if (CheckForUserMovement(CatInfo.HostID, UserID, DomainID) == true)
                    log.Debug("User had been moved to new server so updated local hostID");
                else
                    log.Debug("User had been moved to new server but local hostID updation failed");
                return 1;
                
            }
            catch (Exception ex)
            {
                log.Debug("Exception while CheckForUserMovement :"+ex);
                return -1;
            }
            
            
        }

        /// <summary>
        /// Called if collection is moved, if yes , then set the new host id into the collection
        /// Also checks if its owner is moved , if yes , then update domain's HostID
        /// If it returns false, it means, collection is either deleted really on server, or it is there. 
        /// true return means , collection has been moved from old server and it has been updated here also
        /// </summary>
 
        public bool CheckForUserMovement(string NewHostID, string UserID, string DomainID)
        {
            string OldHostID = collection.HostID;
            bool CollectionMoved = false;
            bool UserMovementUpdationDone = false;

            Domain domain = store.GetDomain(DomainID);

            if(NewHostID == null || NewHostID == "")
            {
                // Collection has been found deleted, through discovery service too , so return false
                return CollectionMoved;
            }
            if (NewHostID == OldHostID)
            {
                // old host id is same as the new host id on the server so collection is there, return false
                return CollectionMoved;
            }
            if (NewHostID != OldHostID)
            {
                // Through catalog, we got new host ID, so update this info in local store , for next time sync
                collection.HostID = NewHostID;
                collection.Commit();
                if (collection.Owner.UserID == UserID)
                {
                    // If current UserID is same as owner's user id , it means user was also moved so update domain too
                    // If hostid is already updated , the skip
                    if (!(domain.HostID == NewHostID))
                    {
                        // Get new Host URI
                        HostNode hNode = HostNode.GetHostByID(DomainID, NewHostID);
                        string homeServerURL = hNode.PublicUrl.TrimEnd(new char[] { '/' });
                        Uri hostUri = new Uri(homeServerURL);

                        Property p = new Property(PropertyTags.HostAddress, hostUri);
                        p.LocalProperty = true;
                        domain.Properties.ModifyNodeProperty(p);
                        domain.HostID = NewHostID;
                        domain.Commit();
                    }
                }
                UserMovementUpdationDone = true;
            }
            return UserMovementUpdationDone;
        }



		#endregion

		#region private Methods

		/// <summary>
		/// Called to dispose the CollectionSyncClient.
		/// </summary>
		/// <param name="inFinalizer">True if called from the finalizer.</param>
		private void Dispose(bool inFinalizer)
		{
			if (!inFinalizer)
			{
				GC.SuppressFinalize(this);
			}
			if (timer != null)
			{
				timer.Dispose();
				timer = null;
			}
			if (fileMonitor != null)
			{
				fileMonitor.Dispose();
				fileMonitor = null;
			}
		}

		/// <summary>
		/// Called to reinitilize if we failed.
		/// This will happen when we are disconected.
		/// </summary>
		/// <param name="collectionClient"></param>
		private void InitializeSlave(object collectionClient)
		{
			log.Debug("In InitializeSlave");
			int delay;
			if (collection.MasterIncarnation == 0) delay = initialSyncDelay;
			else delay = collection.Interval == Timeout.Infinite ? Timeout.Infinite : initialSyncDelay;
			
			// If the master has not been created. Do it now.
			try
			{
				workArray = new SyncWorkArray(collection);
				serverContext = null;
				clientContext = null;
				log.Debug("setting client and server contexts to null...");
				timer = new Timer(callback, this, delay, Timeout.Infinite);
			}
			catch
			{
				log.Debug("Exception in InitializeSlave");
				timer = new Timer(new TimerCallback(InitializeSlave), this, delay, Timeout.Infinite);
			}
			log.Debug("out of InitializeSlave");
		}

		/// <summary>
		/// Initializes the instance.
		/// </summary>
		private void Initialize()
		{
			try
			{
				fileMonitor = new FileWatcher(collection, false);
				switch(collection.Role)
				{
					case SyncRoles.Master:
					case SyncRoles.Local:
					default:
						timer = new Timer(callback, this, initialSyncDelay, Timeout.Infinite);				
						break;

					case SyncRoles.Slave:
						InitializeSlave(this);
						break;
				}
			}
			catch ( Exception ex )
			{
                if (collection.GetRootDirectory() == null ||
                    !Directory.Exists(collection.GetRootDirectory().GetFullPath(collection)))
				{
					collection.Commit(collection.Delete());
				}
				else
				{
					throw ex;
				}
			}
		}

		/// <summary>
		/// Set the new context strings that are used by ChangeLog.
		/// If persist is true save the contexts to the store.
		/// </summary>
		/// <param name="serverContext">The server context.</param>
		/// <param name="clientContext">The client context.</param>
		/// <param name="persist">Persist the changes if true.</param>
		private void SetChangeLogContext(string serverContext, string clientContext, bool persist)
		{
			this.serverContext = serverContext;
			this.clientContext = clientContext;
	
			if (persist)
			{
				if (serverContext != null)
				{
					Property sc = new Property(ServerCLContextProp, serverContext);
					sc.LocalProperty = true;
					collection.Properties.ModifyProperty(sc);
				}
				if (clientContext != null)
				{
					Property cc = new Property(ClientCLContextProp, clientContext);
					cc.LocalProperty = true;
					collection.Properties.ModifyProperty(cc);
				}
				collection.Commit();
			}
		}

		/// <summary>
		/// Get the new context strings that are used by ChangeLog.
		/// </summary>
		/// <param name="serverContext">The server context.</param>
		/// <param name="clientContext">The client context.</param>
		private void GetChangeLogContext(out string serverContext, out string clientContext)
		{
			if (this.serverContext == null)
			{
				Property sc = collection.Properties.GetSingleProperty(ServerCLContextProp);
				if (sc != null)
				{
					this.serverContext = sc.Value.ToString();
				}
			}
			if (this.clientContext == null)
			{
				Property cc = collection.Properties.GetSingleProperty(ClientCLContextProp);
				if (cc != null)
				{
					this.clientContext = cc.Value.ToString();
				}
			}
			serverContext = this.serverContext != null ? this.serverContext : "";
			clientContext = this.clientContext != null ? this.clientContext : "";
		}

		/// <summary>
		/// Returns information about all of the nodes in the collection.
		/// The nodes can be used to determine what nodes need to be synced.
		/// </summary>
		/// <returns>Array of NodeStamps</returns>
		private SyncNodeInfo[] GetNodeInfoArray()
		{
			log.Debug("GetNodeInfoArray start");
			ArrayList infoList = new ArrayList();
			foreach (ShallowNode sn in collection)
			{
				Node node;
				try
				{
					node = new Node(collection, sn);
					if (collection.HasCollisions(node))
						continue;
					infoList.Add(new SyncNodeInfo(node));
				}
				catch (Storage.DoesNotExistException)
				{
					log.Debug("Node: Name:{0} ID:{1} Type:{2} no longer exists.", sn.Name, sn.ID, sn.Type);
					continue;
				}
			}
			log.Debug("GetNodeInfoArray returning {0} nodes", infoList.Count);
			return (SyncNodeInfo[])infoList.ToArray(typeof(SyncNodeInfo));
		}

		/// <summary>
		/// Get the changes from the change log in increments
		/// </summary>
		/// <param name="nodes">returns the list of changes.</param>
		/// <param name="context">The context handed back from the last call.</param>
		/// <returns>false the call failed. The context is initialized.</returns>
		public bool GetChangedNodeInfoArrayIncrements(out SyncNodeInfo[] cstamps, ref string tempClientContext, ref bool moreEntries)
		{
			bool gotClientChanges = false;
			bool scanThreadStatus = false;
			moreEntries = true;
			
			while(true)
			{
				// doing atomic
				lock (fileMonitor)
				{
					scanThreadStatus = fileMonitor.scanThreadRunning;
				}
				if(scanThreadStatus == false)
				{
					// do not come back since scan thread has dead already
					// moreEntries is tied to the scan thread running state, but there might be more
					// entries even if scan thread has stopped (scan executed faster than upload) - ???
					moreEntries = false;
				}
					
				gotClientChanges = this.GetChangedNodeInfoArray(out cstamps, ref tempClientContext);
				if(gotClientChanges == true)
				{
					//Got some changed entries so break and return
					break;
				}
				else
				{
					if(scanThreadStatus == false)
					{
						// resetting moreEntries, so that the SyncNow does not loop
						// since gotClientChanges is false, so obviously no more entries
						moreEntries = false;
						break;
					}
					else
					{
						//Allow scan thread to do some work and ping after 10 ms
						Thread.Sleep(10);	
					}
				}
			}
			return gotClientChanges;
		}
			
		/// <summary>
		/// Get the changes from the change log.
		/// </summary>
		/// <param name="nodes">returns the list of changes.</param>
		/// <param name="context">The context handed back from the last call.</param>
		/// <returns>false the call failed. The context is initialized.</returns>
		private bool GetChangedNodeInfoArray(out SyncNodeInfo[] nodes, ref string context)
		{
			//log.Debug("GetChangedNodes Start");
			EventContext eventContext;
			ArrayList changeList = null;
			ArrayList infoList = new ArrayList();

			// Create a change log reader.
			ChangeLogReader logReader = new ChangeLogReader( collection );
			nodes = null;
			bool more = true;

			try
			{
				// Read the cookie from the last sync and then get the changes since then.
				if (context != null && context.Length != 0)
				{
					eventContext = new EventContext(context);
					while (more)
					{
						more = logReader.GetEvents(eventContext, out changeList);
						foreach( ChangeLogRecord rec in changeList )
						{
							infoList.Add(new SyncNodeInfo(rec));
						}
					}

					nodes = (SyncNodeInfo[])infoList.ToArray(typeof(SyncNodeInfo));
					context = eventContext.ToString();
					
					if (infoList.Count > 0)
					{
						log.Debug("Found {0} changed nodes.", infoList.Count);	
						return true;
					}
					else
						return false;
				}
			}
			catch
			{
				log.Debug("Could not get changes");
			}
			// The cookie is invalid.  Get a valid cookie and save it for the next sync.
			eventContext = logReader.GetEventContext();
			if (eventContext != null)
				context = eventContext.ToString();
			return false;
		}
	
		/// <summary>
		/// Using the change node stamps, determine what sync work needs to be done.
		/// </summary>
		/// <param name="cstamps">The client changes.</param>
		/// <param name="context">The sync context.</param>
		private void ProcessChangedNodeStamps(SyncNodeInfo[] cstamps, ref string context)
		{
			SyncNodeInfo[] infoList;
			string tempContext = null;
			while ((infoList = service.GetNextInfoList(out tempContext)) != null)
			{
				foreach(SyncNodeInfo nodeInfo in infoList)
				{
					workArray.AddNodeFromServer(nodeInfo);
				}
			}

			if (cstamps != null)
			{
				log.Debug("Client Change Log has {0} node modifications.", cstamps.Length);
				for (int i = 0; i < cstamps.Length; ++i)
				{
					workArray.AddNodeToServer(cstamps[i]);
				}
			}
		
			if (tempContext != null)
				context = tempContext;
			
		}

        /// <summary>
        /// Restore all node stamps for the client context
        /// </summary>
        /// <param name="clientcontext">Client context reference</param>
		private void ReconcileAllNodeStamps(ref string clientcontext)
		{
			EventContext eventContext;

			// Create a change log reader.
			ChangeLogReader logReader = new ChangeLogReader(collection);

			ReconcileAllNodeStamps();

			// The cookie is invalid.  Get a valid cookie and save it for the next sync.
			eventContext = logReader.GetEventContext();
			if (eventContext != null)
				clientcontext = eventContext.ToString();
		}

		
		/// <summary>
		/// Determines which nodes need to by synced.
		/// This is done by comparing all nodes on the server with all nodes on the client.
		/// </summary>
		private void ReconcileAllNodeStamps()
		{
			log.Debug("Brute force sync");
			SyncNodeInfo[] cstamps = GetNodeInfoArray();
			SyncNodeInfo[] sstamps;
			// Clear the current work because we are doing a full sync.
			workArray.Clear();
			
			Hashtable tempTable = new Hashtable();

			// Add all of the server nodes to the hashtable and then we can reconcile them
			// against the client nodes.
			string context;
			while ((sstamps = service.GetNextInfoList(out context)) != null)
			{
				foreach (SyncNodeInfo sStamp in sstamps)
				{
					tempTable.Add(sStamp.ID, sStamp);
				}
			}
			log.Debug("Server has {0} nodes. Client has {1} nodes", tempTable.Count, cstamps.Length);

			foreach (SyncNodeInfo cStamp in cstamps)
			{
				SyncNodeInfo sStamp = (SyncNodeInfo)tempTable[cStamp.ID];
				if (sStamp == null)
				{
					// If the Master Incarnation is not 0 then this node has been deleted on the server.
					if (cStamp.MasterIncarnation != 0)
					{
						cStamp.Operation = SyncOperation.Delete;
						cStamp.MasterIncarnation = 0;
						workArray.AddNodeFromServer(cStamp);
					}
					else
					{
						// The node is on the client but not the server send it to the server.
						workArray.AddNodeToServer(cStamp);
					}
				}
				else
				{
					// The node is on both the server and the client.  Check which way the node
					// should go.
					if (cStamp.NodeType == SyncNodeType.Tombstone)
					{
						// This node has been deleted on the client.
						workArray.AddNodeToServer(cStamp);
					}
					else if (cStamp.LocalIncarnation != cStamp.MasterIncarnation)
					{
						// The file has been changed locally if the master is correct, push this file.
						if (cStamp.MasterIncarnation == sStamp.LocalIncarnation)
						{
							workArray.AddNodeToServer(cStamp);
						}
						else
						{
							// This will be a conflict get the servers latest revision.
							workArray.AddNodeFromServer(sStamp);
						}
					}
					else
					{
						// The node has not been changed locally see if we need to get the node.
						if (cStamp.MasterIncarnation != sStamp.LocalIncarnation)
						{
							workArray.AddNodeFromServer(sStamp);
						}
					}
					tempTable.Remove(cStamp.ID);
				}
			}

			// Now Get any nodes left, that are on the server but not on the client.
			foreach(SyncNodeInfo sStamp in tempTable.Values)
			{
				workArray.AddNodeFromServer(sStamp);
			}
			
		}
		
		/// <summary>
		/// For domain sync add the member node always (irrespective of whether modified or not)
		/// This is required because all policies are transfered through the member node
		/// </summary>
		private void AddMemberNodeFromServer()
		{
			Domain domain = null;
			Member currentMember = null; 

			
			string ColType = collection.GetType().ToString();
			bool IsDomainType = ColType.Equals("Simias.Storage.Domain") || ColType.Equals("Domain") ;

			string DomainID = collection.Domain;		
			if (DomainID == null)
			{
				log.Info("ProcessNodesFromServer  DomainID null");
				return;
			}			
			domain = store.GetDomain(DomainID);
			if(domain ==null)
			{
				Log.log.Debug("ProcessNodesFromServer  domain null");
				return;
			}
	        
			if (IsDomainType && !Store.IsEnterpriseServer)
			{

				currentMember =  domain.GetCurrentMember();
				Node memberNode = collection.GetNodeByID(currentMember.UserID);

				// here we assume the member node already exists in the store. The Sync sequence has to be maintained
				// for this to be true. Mainly for new account creation
				if (memberNode != null) 
				{
					SyncNodeInfo memberSyncInfo = new SyncNodeInfo(memberNode);
					workArray.AddMemberNodeFromServerMerge(memberSyncInfo);
				}			
				else
				Log.log.Debug("AddmemberNodeFromServer member node null");
			}
			else
			{
				//if no node to sync and this is not a domain collection then return
				return;
			}
		}

		/// <summary>
		/// Do the work that is needed to synchronize the client and server.
		/// </summary>
		private void ExecuteSync()
		{
			if (workArray.MergeCount != 0)
			{
				// Get the updates from the server.
				ProcessDeleteOnClient(true);
				ProcessNodesFromServer(true);
				// Make sure that we have all subdirs down before we 
				// start on files.
				if (ProcessDirsFromServer(true))
					ProcessFilesFromServer(true);
			}
			// we will download all the nodes needed from the server. This is for new account creation
			// were the user and host node has to be present for the Merge to happen properly.
			// While we ProcessNodesFromServer we assume the member node exists
			if (workArray.DownCount != 0)
			{
				// Get the updates from the server.
				ProcessDeleteOnClient(false);
				ProcessNodesFromServer(false);
				// Make sure that we have all subdirs down before we 
				// start on files.
				if (ProcessDirsFromServer(false))
					ProcessFilesFromServer(false);
			}
			if (workArray.UpCount != 0)
			{
				// Push the updates from the client.
				ProcessDeleteOnServer();
				ProcessNodesToServer();
				// Make sure that we put the subdirs up before the files.
				if (ProcessDirsToServer())
					ProcessFilesToServer();
			}
		}

		/// <summary>
		/// Delete the nodes on the client.
		/// </summary>
		private void ProcessDeleteOnClient(bool merge)
		{
			
			// remove deleted nodes from client
			string[] idList = workArray.DeletesFromServer(merge);
			if (idList.Length == 0)
				return;
			log.Info("Deleting {0} nodes on client", idList.Length);
			foreach (string id in idList)
			{
				if (Yield)
				{
					return;
				}
				try
				{
					Node node = collection.GetNodeByID(id);
					if (node == null)
					{
						log.Debug("Ignoring attempt to delete non-existent node {0}", id);
						workArray.RemoveNodeFromServer(id, merge);
						continue;
					}

					log.Info("Deleting {0} on client", node.Name);
					// If this is a collision node then delete the collision file.
					if (collection.HasCollisions(node))
					{
						Conflict conflict = new Conflict(collection, node);
						string conflictPath;
						if (conflict.IsFileNameConflict)
						{
							conflictPath = conflict.FileNameConflictPath;
						}
						else 
						{
							conflictPath = conflict.UpdateConflictPath;
						}
						if (conflictPath != null)
						{
							if (File.Exists(conflictPath))
								File.Delete(conflictPath);
						}
					}
						
					DirNode dn = node as DirNode;
					if (dn != null)
					{
						string fullPath = dn.GetFullPath(collection); 
						if (Directory.Exists(fullPath))
							Directory.Delete(fullPath, true);
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Directory, true, node.Name, 0, 0, 0, Direction.Downloading));
						Node[] deleted = collection.Delete(node, PropertyTags.Parent);
						collection.Commit(deleted);
						collection.Commit(deleted);
					}
					else
					{
						BaseFileNode bfn = node as BaseFileNode;
						if (bfn != null)
						{
							try
							{
								eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, true, node.Name, 0, 0, 0, Direction.Downloading));
								FileInfo fi = new FileInfo(bfn.GetFullPath(collection));
								if (rights == Access.Rights.ReadOnly)
									fi.Attributes = fi.Attributes & ~FileAttributes.ReadOnly;
								fi.Delete();
							}
							catch {}
						}
						else
						{
							eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Unknown, true, node.Name, 0, 0, 0, Direction.Downloading));
						}
						collection.DeletePOBox(node);
						if (Store.IsEnterpriseServer == true)
						{
							collection.OrphanCollections(node);
						}
						collection.RemoveMemberships(node);
						collection.Delete(node);
						collection.Commit(node);
						collection.Commit(node);
					}
					workArray.RemoveNodeFromServer(id, merge);
				}
				catch(Exception e)
				{
					// Try to delete the next node.
					//FIXME: we must not remove the node on exception - workaround for new sync engine
					log.Debug("SyncClient.cs: On exception {0}, the nodeid {1} is removed from workarray",e.Message, id);
					workArray.RemoveNodeFromServer(id, merge);
				}
			}
		}

		/// <summary>
		/// Copy the generic nodes from the server.
		/// </summary>
		private void ProcessNodesFromServer(bool merge)
		{
			SyncNode[] updates = null;            
            
			// get small nodes and files from server
			string[] nodeIDs = workArray.GenericsFromServer(merge);
			
            if (nodeIDs.Length == 0)
                return;

			log.Info("Downloading {0} Nodes from server", nodeIDs.Length);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length)
			{
				if (Yield)
					return;
				int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
				try
				{
					string[] batchIDs = new string[batchCount];
					Array.Copy(nodeIDs, offset, batchIDs, 0, batchCount);
					updates = service.GetNodes(batchIDs);
					StoreNodes(updates, merge);
				}
                		catch (Exception ex){log.Debug("SyncClient.cs: ProcessNodesFromServer, got exception, "+ex.ToString());}
				offset += batchCount;
			}
			// Update the collection in case it changed.
			collection.Refresh();
		}
	
		/// <summary>
		/// Save the nodes from the server in the local store.
		/// </summary>
		/// <param name="nodes"></param>
		private void StoreNodes(SyncNode [] nodes, bool merge)
		{
			ArrayList	commitArray = new ArrayList();
			Node[]		commitList = null;
			bool collectionChanged = false;
			// Try to commit all the nodes at once.
			foreach (SyncNode sn in nodes)
			{
				if (sn.node != null && sn.node.Length != 0)
				{
					XmlDocument xNode = new XmlDocument();
					xNode.LoadXml(sn.node);
					Node node = Node.NodeFactory(store, xNode);
					if (node.IsBaseType(NodeTypes.CollectionType))
					{
						collectionChanged = true;
					}
					log.Info("Importing {0} {1} from server", node.Type, node.Name);
					Import(node);
					commitArray.Add(node);
				}
				else
				{
					workArray.RemoveNodeFromServer(sn.ID, merge);
				}
			}
			try
			{
				commitList = (Node[])commitArray.ToArray(typeof(Node));
				collection.Commit(commitList);
				foreach ( Node node in commitList)
				{
					if (node != null)
						workArray.RemoveNodeFromServer(node.ID, merge);
				}
			}
			catch
			{
				// If we get here we need to try to commit the nodes one at a time.
				foreach (Node node in commitList)
				{
					try
					{
						if (node != null)
						{
							collection.Commit(node);
						}
						workArray.RemoveNodeFromServer(node.ID, merge);
					}
					catch (CollisionException)
					{
						try
						{
							// The current node failed because of a collision.
							Node cNode = collection.CreateCollision(node, false);
							collection.Commit(cNode);
							workArray.RemoveNodeFromServer(cNode.ID, merge);
						}
						catch
						{
						}
					}
					catch 
					{
						// Handle any other errors.
						log.Debug("SyncClient.cs, caught exception inside StoreNodes for collection: "+collection.Name);
					}
				}
			}
			if (collectionChanged)
			{
				collection.Refresh();
			}
		}

		/// <summary>
		/// Get the directory nodes from the server.
		/// </summary>
		/// <returns>true if successful.</returns>
		private bool ProcessDirsFromServer(bool merge)
		{
			bool status = true;
			SyncNode[] updates = null;

			// get small nodes and files from server
			string[] nodeIDs = workArray.DirsFromServer(merge);
			if (nodeIDs.Length == 0)
				return true;
			log.Info("Downloading {0} Directories from server", nodeIDs.Length);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length)
			{
				if (Yield)
				{
					return false;
				}

				int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
				try
				{
					string[] batchIDs = new string[batchCount];
					Array.Copy(nodeIDs, offset, batchIDs, 0, batchCount);
					
					updates = service.GetDirs(batchIDs);

					foreach (SyncNode snode in updates)
					{
						if (!StoreDir(snode, merge))
							status = false;
					}
				}
				catch
				{
				}
				offset += batchCount;
			}
			return status;
		}
	
		/// <summary>
		/// Called to import a node.
		/// </summary>
		/// <param name="node"></param>
		private void Import(Node node)
		{
			collection.ImportNode(node, false, 0);
			node.IncarnationUpdate = node.LocalIncarnation;
		}

		/// <summary>
		/// Store the directory node in the local store also create the directory.
		/// </summary>
		/// <param name="snode">The node to store.</param>
		/// <returns>ture if successful.</returns>
		private bool StoreDir(SyncNode snode, bool merge)
		{
			try
			{
				if (snode.node != null && snode.node.Length != 0)
				{
					XmlDocument xNode = new XmlDocument();
					xNode.LoadXml(snode.node);
					DirNode node = (DirNode)Node.NodeFactory(store, xNode);
					log.Info("Importing Directory {0} from server", node.Name);
					Import(node);
			
					// Get the old node to see if the node was renamed.
					DirNode oldNode = collection.GetNodeByID(node.ID) as DirNode;
					string path;
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
								catch (IOException ex)
								{
									// We got here because of one of the following.
									// 1 The destination directory already exists
									// 2 The source and destion are the same.
									// 3 The directory is in use and could not be moved.
									if (Directory.Exists(path))
									{
										if ((oldPath != path) && (string.Compare(oldPath, path, true) == 0))
										{
											// This was a case rename. Move to a temporary name and then to the
											// new name.
											string tmpName = oldPath + ".simias~";
											Directory.Move(oldPath, tmpName);
											Directory.Move(tmpName, path);
										}
										// This directory has already been moved by the parent move.
									}
									else
									{
										// The path does not exist we could not create the directory.
										// Probably because it is open.
										log.Info("Could not rename directory {0}. Possibly in use.", oldPath);
										throw ex;
									}
								}
							}
						}
					}

					if (!Directory.Exists(path))
					{
						bool conflict = false;
						try
						{
							DirectoryInfo di = Directory.CreateDirectory(path);
							if ((di.Attributes & FileAttributes.Directory) == 0)
								conflict = true;
						}
						catch
						{
							// If the parent exists this is a conflict.
							if (Directory.Exists(Path.GetDirectoryName(path)))
								conflict = true;
							else
								throw;
						}
						if (conflict)
						{
							// Create a collision.
							node = Conflict.CreateNameConflict(collection, node) as DirNode;
							// Find the conflicting node.
							string rpath = node.GetRelativePath();
							foreach (ShallowNode sn in collection.Search(PropertyTags.FileSystemPath, rpath, SearchOp.Equal))
							{
								Node cfNode = collection.GetNodeByID(sn.ID);
								cfNode = Conflict.CreateNameConflict(collection, cfNode, node.GetFullPath(collection));
								Conflict.LinkConflictingNodes(cfNode, node);
								collection.Commit(cfNode);
								break;
							}
						}
					}
					else
					{
						// If we have a node with the same fspath and a unique ID this is a conflict delete the local dir node.
						ICSList nodeList = collection.Search(PropertyTags.FileSystemPath, node.GetRelativePath(), SearchOp.Equal);
						foreach (ShallowNode sn in nodeList)
						{
							if (sn.ID == node.ID)
								continue;
							// We have a dir conflict. delete the local DirNode
							Node conflictDir = collection.GetNodeByID(sn.ID);
							Node[] deleted = collection.Delete(conflictDir, PropertyTags.Parent);
							collection.Commit(deleted);
							fileMonitor.NeedToDredge = true;
							break;
						}
					}
					collection.Commit(node);
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Directory, false, node.Name, 0, 0, 0, Direction.Downloading));
				}
				workArray.RemoveNodeFromServer(snode.ID, merge);
				return true;
			}
			catch 
			{
				log.Debug("SyncClient.cs: caught exception inside StoreDir for collectionid : "+snode.ID);
				return false;
			}
		}

		/// <summary>
		/// Get the file nodes from the server.
		/// </summary>
		private void ProcessFilesFromServer(bool merge)
		{
			 HttpClientInFile file = null;

                        //we need not download Journal nodes from the server to client
                        //it is only needed on the server
                        string[] nodeIDs = workArray.FilesFromServer(merge, Store.IsEnterpriseServer);
                        if (nodeIDs.Length == 0)
                        return;

                        string[] deleteNodeIDs = workArray.DeletesToServer();

                        if(deleteNodeIDs.Length > 0)
                        {
                                foreach(string nodeid in nodeIDs)
                                {
                                        foreach(string deleteid in deleteNodeIDs)
                                        {
                                                if(nodeid == deleteid)
                                                {
                                                        workArray.RemoveNodeFromServer(nodeid);
                                                }
                                        }
                                }
                        }
                        nodeIDs = workArray.FilesFromServer(merge, Store.IsEnterpriseServer);
                        if (nodeIDs.Length == 0)
                                return;
                        log.Info("Downloading {0} Files from server", nodeIDs.Length);
                        foreach (string nodeID in nodeIDs)
			{
				try
				{
					if (Yield)
					{
						log.Debug("Yielding the download files");
						return;
					}

					file = new HttpClientInFile(collection, nodeID, service);
					if (file.Open(rights == Access.Rights.ReadOnly ? true : false))
					{
						bool success = false;
						try
						{
							lock (this) {syncFile = file;}
							log.Info("Downloading File {0} from server", file.Name);
							success = file.DownLoadFile();
						}
						catch (Exception ex)
						{
							log.Debug(ex, "Failed Download before close");
						}
						finally
						{
							success = file.Close(success);
							lock (this) {syncFile = null;}
							if (success)
							{
								workArray.RemoveNodeFromServer(nodeID, merge);
							}
							else
							{
								log.Info("Failed Downloading File {0}", file.Name);
							}

							if(file.removeNodeToserver !=null)
							{
								workArray.RemoveNodeToServer(file.removeNodeToserver);
								log.Info("Local node removed - Name conflict is resolved -RemoveNodeToServer   {0}", file.removeNodeToserver);
							}
						}
					}
					else
					{
						// There is no file to pull down.
						workArray.RemoveNodeFromServer(nodeID, merge);
					}
				}
				catch (DirectoryNotFoundException)
				{
					// The directory does not exist.
					// It has either been deleted or it has a conflict.
					FileNode fn = collection.GetNodeByID(nodeID) as FileNode;
					if (fn != null)
					{
						DirNode parent = fn.GetParent(collection);
						if (parent != null && !collection.HasCollisions(parent))
						{
							// The directory has been deleted.
							workArray.RemoveNodeFromServer(nodeID, merge);
						}
					}
				}
				catch(PathTooLongException pex)
				{
			                workArray.RemoveNodeFromServer(nodeID, merge);
					log.Info("PathTooLongException occured: Detailed StackTrace is {0} ", pex.ToString());
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, file.Name, 0,0,0, Direction.Downloading,SyncStatus.PathTooLong));
				}
				catch (UnauthorizedAccessException ex)
				{
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, file.Name, 0,0,0, Direction.Downloading,SyncStatus.IOError));
					throw new SyncAbortedException (file.Name, ex);
				}
                catch (WebException we)
                {
                    //This is to handle any webException while performing upload/download
                    //not to remove node from workarray in this case
                    //To remove if required, check the error message explicitly
                    Log.log.Debug(we, "Failed Downloading File, WebException");
                }   
				catch (Exception ex)
				{
					if( this.collection.DataMovement != true)
				               workArray.RemoveNodeFromServer(nodeID, merge);
					Log.log.Debug(ex, "Failed Downloading File during close");
				}
			}
		}

		/// <summary>
		/// Delete nodes from the server.
		/// </summary>
		private void ProcessDeleteOnServer()
		{
			// remove deleted nodes from server
			//TODO: This function needs optimization -- Kalis

			try
			{
				string[] idList = workArray.DeletesToServer();
				if (idList.Length == 0)
					return;
				
				log.Info("Deleting {0} Nodes on server", idList.Length);

				SyncNodeStatus[] nodeStatus = service.DeleteNodes(idList);
				foreach (SyncNodeStatus status in nodeStatus)
				{
					log.Debug("Node ID to delete {0}", status.nodeID);
					try
					{
						if (status.status == SyncStatus.Success)
						{
							Node node = collection.GetNodeByID(status.nodeID);
							//Skip the member node deletion from client, this case should not come ideally
							if (node != null && node.Type != NodeTypes.MemberType)
							{
								eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Unknown, true, node.Name, 0, 0, 0, Direction.Uploading));
								log.Info("Deleting {0} from server", node.Name);
								// Delete the tombstone.
								try
								{
									collection.Commit(collection.Delete(node));
								}
								catch
								{
									// The node does not exist. just ignore the error.
								}
							}
							workArray.RemoveNodeToServer(status.nodeID);
						}
						else if(status.status == SyncStatus.Locked)
							break;
					}
					catch (Exception ex)
					{
						log.Debug("Exception in ProcessDeleteOnServer--- {0}", ex.Message);
						log.Debug("Exception in ProcessDeleteOnServer--- {0}",ex.StackTrace);
					}
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Upload the nodes to the server.
		/// </summary>
		private void ProcessNodesToServer()
		{
			// get small nodes and files from server
			string[] nodeIDs = workArray.GenericsToServer();
			if (nodeIDs.Length == 0)
				return;
			log.Info("Uploading {0} Nodes To server", nodeIDs.Length);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length)
			{
				if (Yield)
				{
					return;
				}
				int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
				SyncNode[] updates = new SyncNode[batchCount];
				Node[] nodes = new Node[batchCount];
				try
				{
					for (int i = offset; i < offset + batchCount; ++ i)
					{
						Node node = collection.GetNodeByID(nodeIDs[i]);
						if (node != null)
						{
							log.Info("Uploading {0} {1} to server", node.Type, node.Name);
							nodes[i - offset] = node;
							updates[i - offset] = new SyncNode(node);
						}
						else
						{
							workArray.RemoveNodeToServer(nodeIDs[i]);
						}
					}

					SyncNodeStatus[] nodeStatus = service.PutNodes(updates);
					
					for (int i = 0; i < nodes.Length; ++ i)
					{
						Node node = nodes[i];
						if (node == null)
							continue;
						SyncNodeStatus status = nodeStatus[i];
						switch (status.status)
						{
							case SyncStatus.Success:
								node.SetMasterIncarnation(node.LocalIncarnation);
								collection.Commit(node);
								workArray.RemoveNodeToServer(node.ID);
								break;
							case SyncStatus.UpdateConflict:
							case SyncStatus.FileNameConflict:
								// The file has been changed on the server lets get it next pass.
								log.Debug("Skipping update of node {0} due to {1} on server",
									status.nodeID, status.status);
									
								SyncNodeInfo ns = new SyncNodeInfo(node);
								ns.MasterIncarnation++;
								workArray.AddNodeFromServer(ns);
								workArray.RemoveNodeToServer(node.ID);
								break;
							case SyncStatus.Locked:
								// The collection is locked exit.
								i = nodes.Length;
								break;
                            /*case SyncStatus.PolicyEncryptionEnforced:
                            case SyncStatus.PolicyLimit:
                                log.Debug("Skipping the update of node {0} due to {1} on server",
                                    status.nodeID, status.status);
                                log.Debug("either limit crossed or encry enforced");
                                   
                                Domain domain = store.GetDomain(store.DefaultDomain);
                                Member memberBeforeSync = collection.GetCurrentMember(); domain.GetCurrentMember();
                                // Encryption was enforced for the owner so revert back ownership
                                collection.ChangeOwner(memberBeforeSync, Access.Rights.Admin);
                                log.Debug("committed the collection : " + collection.ID);
                                log.Debug("committed the above collection with ownerid : " + memberBeforeSync.ID);
                                collection.Commit();
                                break;*/
							default:
								log.Debug("Skipping update of node {0} due to {1} on server",
									status.nodeID, status.status);
                                //we need to see if this is really required??? how does Reconcile/changes sync help
                                workArray.RemoveNodeToServer(node.ID);
								break;
						}
					}
				}
				catch
				{
				}
				offset += batchCount;
			}
		}

		/// <summary>
		/// Copy subdirs to the server.
		/// </summary>
		/// <returns>true if successful.</returns>
		private bool ProcessDirsToServer()
		{
			bool bStatus = true;
			// get small nodes and files from server
			string[] nodeIDs = workArray.DirsToServer();
			if (nodeIDs.Length == 0)
				return true;
			log.Info("Uploading {0} Directories To server", nodeIDs.Length);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length)
			{
				if (Yield)
				{
					return false;
				}

				int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
				SyncNode[] updates = new SyncNode[batchCount];
				Node[] nodes = new Node[batchCount];
				try
				{
					for (int i = offset; i < offset + batchCount; ++ i)
					{
						Node node = collection.GetNodeByID(nodeIDs[i]);
						if (node != null & !collection.HasCollisions(node))
						{
							log.Info("Uploading Directory {0} to server", node.Name);
							nodes[i - offset] = node;
							updates[i - offset] = new SyncNode(node);
						}
						else
						{
							// The node no longer exists or has a collision.
							workArray.RemoveNodeToServer(nodeIDs[i]);
						}
					}

					SyncNodeStatus[] nodeStatus = service.PutDirs(updates);
					
					for (int i = 0; i < nodes.Length; ++ i)
					{
						Node node = nodes[i];
						SyncNodeStatus status = nodeStatus[i];
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Directory, false, node.Name, 0, 0, 0, Direction.Uploading, status.status));
						switch (status.status)
						{
							case SyncStatus.Success:
								node.SetMasterIncarnation(node.LocalIncarnation);
								collection.Commit(node);
								workArray.RemoveNodeToServer(node.ID);
								break;
							case SyncStatus.UpdateConflict:
							case SyncStatus.FileNameConflict:
								// The file has been changed on the server lets get it next pass.
								log.Debug("Failed update of node {0} due to {1} on server",
									status.nodeID, status.status);
									
								SyncNodeInfo ns = new SyncNodeInfo(node);
								ns.MasterIncarnation++;
								workArray.AddNodeFromServer(ns);
								workArray.RemoveNodeToServer(node.ID);
								break;
							case SyncStatus.Locked:
								// The collection is locked.
								i = nodes.Length;
								bStatus = false;
								break;
							default:
								log.Debug("Failed update of node {0} due to {1} on server",
									status.nodeID, status.status);
								bStatus = false;
								break;
						}
					}
				}
				catch
				{
					bStatus = false;
				}
				offset += batchCount;
			}
			return bStatus;
		}

        /// <summary>
        /// Process the files that are to be uploaded to server
        /// </summary>
		private void ProcessFilesToServer()
		{
			string[] nodeIDs = workArray.FilesToServer();
			if (nodeIDs.Length == 0)
				return;
			BaseFileNode node = null;

			log.Info("Uploading {0} Files To server", nodeIDs.Length);
			
			foreach (string nodeID in nodeIDs)
			{
				try
				{
					if (Yield)
					{
						return;
					}
					node = collection.GetNodeByID(nodeID) as BaseFileNode;
					if (node != null)
					{
						if (collection.HasCollisions(node))
						{
							// We have a collision do not sync.
							workArray.RemoveNodeFromServer(nodeID);
						}
						HttpClientOutFile file = new HttpClientOutFile(collection, node, service);
						SyncStatus status = file.Open();
						if (status == SyncStatus.Success)
						{
							bool success = false;
							try
							{
								lock (this) {syncFile = file;}
								log.Info("Uploading File {0} to server", file.Name);
								success = file.UploadFile();
                                //Avoid creating hash map - see if this can be done at the server side
                                //we create hash map on server side for 3.7
								if(success)
								{
									log.Info("Uploading hash map for File {0} to server", file.Name);
									file.UploadHashMap();
								}
							}
							finally
							{
								SyncNodeStatus syncStatus = file.Close(success);
								lock (this) {syncFile = null;}
								switch (syncStatus.status)
								{
									case SyncStatus.OnlyDateModified:										
									case SyncStatus.Success:	
										if(syncStatus.status == SyncStatus.OnlyDateModified)
											log.Info("Cancelled Uploading File {0} : reason {1}", file.Name, syncStatus.status.ToString());
										workArray.RemoveNodeToServer(nodeID);
										break;
									case SyncStatus.InProgess:
									case SyncStatus.InUse:
									case SyncStatus.ServerFailure:
										log.Info("Failed Uploading File {0} : reason {1}", file.Name, syncStatus.status.ToString());
										break;
									case SyncStatus.UpdateConflict:
										// Since we had a conflict we need to get the conflict node down.
										workArray.RemoveNodeToServer(nodeID);
										SyncNodeInfo ns = new SyncNodeInfo(node);
										ns.MasterIncarnation++;
										workArray.AddNodeFromServer(ns);
										log.Info("Failed Uploading File {0} : reason {1}", file.Name, syncStatus.status.ToString());
										break;
								}
							}
						}
						else
						{
							eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Directory, false, node.GetFullPath(collection), 0, 0, 0, Direction.Uploading, status));
							switch (status)
							{
								case SyncStatus.FileNameConflict:
									// Since we had a conflict we need to set the conflict.
									BaseFileNode conflictNode = Conflict.CreateNameConflict(collection, node, node.GetFullPath(collection)) as BaseFileNode;
									collection.Commit(conflictNode);
									workArray.RemoveNodeToServer(nodeID);
									log.Info("Failed Uploading File {0} : reason {1}", file.Name, status.ToString());
									break;
								case SyncStatus.PolicyQuota:
								case SyncStatus.PolicySize:
									log.Info("Failed Uploading File {0} : reason {1}", file.Name, status.ToString());
									//workArray.RemoveNodeToServer(nodeID);
									break;
								case SyncStatus.PolicyType:
									log.Info("Failed Uploading File {0} : reason {1}", file.Name, status.ToString());
									workArray.RemoveNodeToServer(nodeID);
									break;
								case SyncStatus.Locked:
									log.Info("Failed Uploading File {0} : reason {1}", file.Name, status.ToString());
									return;
								case SyncStatus.InUse:
									log.Info("Failed Uploading File {0} : reason {1}", file.Name, status.ToString());
									return;
								default:
									log.Info("Failed Uploading File {0} : reason {1}", file.Name, status.ToString());
									break;
							}
						}
					}
					else
					{
						// The file no longer exists.
						workArray.RemoveNodeToServer(nodeID);
						log.Debug("Mystery File node {0}", nodeID);
					}
				}
				catch (FileNotFoundException excep)
				{
			        Log.log.Debug(excep, "Failed Uploading File, FileNotFoundException");
					// The file no longer exists. this line added for 344792
					workArray.RemoveNodeToServer(nodeID);
					
					//do not know why this the following (1 line) exists
					workArray.RemoveNodeFromServer(nodeID);
				}
                catch (WebException we)
                {
                    //This is to handle any webException while performing upload and 
                    //not to remove node from workarray in this case
                    //if required,To remove, check the error message explicitly
                    Log.log.Debug(we, "Failed Uploading File, WebException");
					if( node != null) {
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, node.GetFullPath(collection), 0, 0, 0, Direction.Uploading, SyncStatus.Error));
					} else {
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, nodeID, 0, 0, 0, Direction.Uploading, SyncStatus.Error));	
					}
                }
				catch (IOException ioe)
                {
                    //This is to handle any IOException while performing upload and 
                    //not to remove node from workarray in this case
                    //if required, To remove, check the error message explicitly
                    Log.log.Debug(ioe, "Failed Uploading File, IOException");
					if( node != null) {
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, node.GetFullPath(collection), 0, 0, 0, Direction.Uploading, SyncStatus.Error));
					} else {
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, nodeID, 0, 0, 0, Direction.Uploading, SyncStatus.Error));	
					}
                }
				catch (ArgumentOutOfRangeException Aex)
                {
					Log.log.Debug(string.Format("ArgumentOutOfRangeException with message :{0} and stacktrace: {1}",Aex.Message, Aex.StackTrace));
					if( node != null) {
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, node.GetFullPath(collection), 0, 0, 0, Direction.Uploading, SyncStatus.Error));
					} else {
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, nodeID, 0, 0, 0, Direction.Uploading, SyncStatus.Error));	
					}
                }
                catch (Exception ex)
				{
			        workArray.RemoveNodeToServer(nodeID);
					Log.log.Debug(ex, "Failed Uploading File");
				}
			}
		}

		#endregion
	}

	internal class SyncWorkArray
	{
		Collection		collection;
		Hashtable		nodesFromServerDownload;
		Hashtable		nodesToServer;
        Hashtable nodesFromServerMerge;
        Hashtable nodesFromServer;

        Access.Rights rights;
		bool			sparseReplica = false;
		static EventPublisher eventPublisher = new EventPublisher();
		
		internal class nodeTypeEntry : IComparable
		{
			static int				counter = 0;
			internal string			ID;
			internal SyncNodeType	Type;
			int						EntryNumber;
            
			internal nodeTypeEntry(string ID, SyncNodeType type)
			{
				this.ID = ID;
				this.Type = type;
				this.EntryNumber = Interlocked.Increment(ref counter);
			}

			#region IComparable Members
            /// <summary>
            /// Compare the object members
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
			public int CompareTo(object obj)
			{
				nodeTypeEntry se = obj as nodeTypeEntry;
				return EntryNumber.CompareTo(se.EntryNumber);
			}

			#endregion
		}

		
		/// <summary>
		/// Array of items to sync.
		/// </summary>
		/// <param name="collection">The collection.</param>
		internal SyncWorkArray(Collection collection)
		{
			this.collection = collection;
			nodesFromServerMerge = new Hashtable();
            nodesFromServerDownload = new Hashtable();
			nodesToServer = new Hashtable();
		}

		/// <summary>
		/// Clear all of the current work to do.
		/// </summary>
		internal void Clear()
		{
			nodesFromServerMerge.Clear();
            nodesFromServerDownload.Clear();
			nodesToServer.Clear();
		}

		/// <summary>
		/// Determins how this node should be retrieved from the server.
		/// </summary>
		/// <param name="stamp">The SyncNodeStamp describing this node.</param>
		internal void AddNodeFromServer(SyncNodeInfo stamp)
		{
            bool NewNode = false;
			if(NodeHasChanged(stamp.ID, stamp.LocalIncarnation, out NewNode) || stamp.Operation == SyncOperation.Delete)
			{
				// Make sure the node does not exist in the nodesToServer table.
				if (nodesToServer.Contains(stamp.ID))
				{
					nodesToServer.Remove(stamp.ID);
				}
				if (stamp.Operation == SyncOperation.Delete)
				{
					Log.log.Debug("In AddNodeFromServer. sync operation on server is delete... {0}", stamp.ID);
					nodesFromServerMerge[stamp.ID] = new nodeTypeEntry(stamp.ID, SyncNodeType.Tombstone);
				}
				else
				{
					if(NewNode == false)
						nodesFromServerMerge[stamp.ID] = new nodeTypeEntry(stamp.ID, stamp.NodeType);
					else
						nodesFromServerDownload[stamp.ID] = new nodeTypeEntry(stamp.ID, stamp.NodeType);
				}
			}
		}

        /// <summary>
		/// This method will add member node always and should be called only by domain sync and thick client.
		/// </summary>
		/// <param name="stamp">The SyncNodeStamp describing this node.</param>
        internal void AddMemberNodeFromServerMerge(SyncNodeInfo stamp)
        {
            if(! nodesFromServerMerge.Contains(stamp.ID))
                nodesFromServerMerge[stamp.ID] = new nodeTypeEntry(stamp.ID, stamp.NodeType);
        }

		/// <summary>
		/// Determins how this node should be sent to the server.
		/// </summary>
		/// <param name="stamp">The SyncNodeStamp describing this node.</param>
		internal void AddNodeToServer(SyncNodeInfo stamp)
		{
		        //Nodes have changed.
			if (stamp.MasterIncarnation != stamp.LocalIncarnation)
			{
				if (rights == Access.Rights.ReadOnly)
				{
				        // If there is a change, by policy we don't sync
				        // if it is a newly created node or a existing file
				        // So just emit a event for client
					Node tNode = collection.GetNodeByID(stamp.ID);
					if (tNode != null)
					{
						if (stamp.Operation == SyncOperation.Delete)
						{
							// Since this is a delete just delete the tombstone.
							collection.Delete(tNode);
							collection.Commit(tNode);
						}
						else
						{
							ObjectType type;
							switch (stamp.NodeType)
							{
								case SyncNodeType.Directory:
									type = ObjectType.Directory;
									break;
								case SyncNodeType.File:
									type = ObjectType.File;
									break;
								default:
									type = ObjectType.Unknown;
									break;
							}
							eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, type, false, tNode.Name, 0, 0, 0, Direction.Uploading, SyncStatus.ReadOnly));
							Log.log.Debug("Failed Uploading Node (ReadOnly rights)");
						}
					}
				}
				else if (nodesFromServer != null && nodesFromServer.Contains(stamp.ID))
				{
					// This node has changed on the server we have a collision that we need to get.
					// Unless this is a delete.
					if (stamp.Operation == SyncOperation.Delete)
					{
						Log.log.Debug("In addnodestoserver: if the type is delete...{0}", stamp.ID);
						RemoveNodeFromServer(stamp.ID);
						nodesToServer[stamp.ID] = new nodeTypeEntry(stamp.ID, SyncNodeType.Tombstone);
					}
				}
				else
				{
					if (stamp.Operation == SyncOperation.Delete)
					{
						Node tNode = collection.GetNodeByID(stamp.ID);
						
						Log.log.Debug("AddNodeToServerr: if the type is delete... case else... {0} .....Type-> {1}", stamp.ID, tNode.Type);

						//Tomstone meber type should not get synced to server from client
						//if it does then the server node will get deleted
						//Ideally the meber node should not get deleted in the client
						
						if(tNode.Type != NodeTypes.MemberType)
							nodesToServer[stamp.ID] = new nodeTypeEntry(stamp.ID, SyncNodeType.Tombstone);
					}
					else
					{
						nodesToServer[stamp.ID] = new nodeTypeEntry(stamp.ID, stamp.NodeType);
					}
				}
			}
		}

        /// <summary>
        /// Removes the node in all server hashtable
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="merge"></param>
        internal void RemoveNodeFromServer(string nodeID)
        {
            nodesFromServerMerge.Remove(nodeID);
            nodesFromServerDownload.Remove(nodeID);
            nodesFromServer.Remove(nodeID);
        }

		/// <summary>
		/// Remove the node from the work table.
		/// </summary>
		/// <param name="nodeID">The node to remove.</param>
		internal void RemoveNodeFromServer(string nodeID, bool merge)
		{
            if (merge)
                nodesFromServerMerge.Remove(nodeID);
            else
                nodesFromServerDownload.Remove(nodeID);

			nodesFromServer.Remove(nodeID);
		}

		/// <summary>
		/// Remove the node from the work table.
		/// </summary>
		/// <param name="nodeID">The node to remove.</param>
		internal void RemoveNodeToServer(string nodeID)
		{
			nodesToServer.Remove(nodeID);
		}

        /// <summary>
        /// Get an array of the IDs of the Nodes to retrieve from the server, based on operation
        /// </summary>
        /// <param name="oType"></param>
        /// <param name="merge"></param>
        /// <returns></returns>
        private string[] FromServer(SyncNodeType oType, bool merge)
        {
            if (merge)
                nodesFromServer = nodesFromServerMerge;
            else
                nodesFromServer = nodesFromServerDownload;
            return FromServer(oType);
        }

		/// <summary>
		/// Get an array of the IDs of the Nodes to retrieve from the server.
		/// </summary>
		/// <param name="oType">The Type of objects to return.</param>
		/// <returns></returns>
		private string[] FromServer(SyncNodeType oType)
		{
			ArrayList na = new ArrayList();
			bool haveCollection = false;
			
			// Lets sync the collection first if it is in our list.
			if (oType == SyncNodeType.Generic)
			{
				haveCollection = nodesFromServer.Contains(collection.ID);
				if (haveCollection)
					na.Add(collection.ID);
			}
			
			nodeTypeEntry[] entryArray = new nodeTypeEntry[nodesFromServer.Count];
			nodesFromServer.Values.CopyTo(entryArray, 0);
			Array.Sort(entryArray);
			
			foreach (nodeTypeEntry entry in entryArray)
			{
				if (entry.Type == oType)
				{
					if (haveCollection && entry.ID == collection.ID)
						haveCollection = false;
					else
						na.Add(entry.ID);
				}
			}

			return (string[])na.ToArray(typeof(string));
		}

		/// <summary>
		/// Get an array of the IDs to delete on the client.
		/// </summary>
		/// <returns></returns>
		internal string[] DeletesFromServer(bool merge)
		{
			return FromServer(SyncNodeType.Tombstone, merge);
		}

		/// <summary>
		/// Get an array of the IDs of the Nodes to retrieve from the server.
		/// </summary>
		/// <returns></returns>
		internal string[] GenericsFromServer(bool merge)
		{
			return FromServer(SyncNodeType.Generic, merge);
		}

		/// <summary>
		/// Get an array of the IDs of the Directory Nodes to retrieve from the server.
		/// </summary>
		/// <returns></returns>
		internal string[] DirsFromServer(bool merge)
		{
			return FromServer(SyncNodeType.Directory, merge);
		}

		/// <summary>
		/// Get an array of the IDs of the File Nodes to retrieve from the server.
		/// </summary>
		/// <returns></returns>
		internal string[] FilesFromServer(bool merge, bool server)
		{
			if(server)
			{
				ArrayList totList = new ArrayList();
				totList.AddRange(FromServer(SyncNodeType.StoreFileNode, merge));
				totList.AddRange(FromServer(SyncNodeType.File, merge));
				return (string[])totList.ToArray(typeof(string));
			}
			else
			{
				string[] storenodes = FromServer(SyncNodeType.StoreFileNode, merge);
				foreach( string nodeID in storenodes)
				{
					RemoveNodeFromServer(nodeID,merge);
				}
				return FromServer(SyncNodeType.File, merge);
			}
		}

		/// <summary>
		/// Get an array of the IDs of the Nodes to push up to the server.
		/// </summary>
		/// <param name="oType">The Type of objects to return.</param>
		/// <returns></returns>
		private string[] ToServer(SyncNodeType oType)
		{
			ArrayList na = new ArrayList();
			bool haveCollection = false;
			
			// Lets sync the collection first if it is in our list.
			if (oType == SyncNodeType.Generic)
			{
				haveCollection = nodesToServer.Contains(collection.ID);
				if (haveCollection)
					na.Add(collection.ID);
			}

			nodeTypeEntry[] entryArray = new nodeTypeEntry[nodesToServer.Count];
			nodesToServer.Values.CopyTo(entryArray, 0);
			Array.Sort(entryArray);
			
			foreach (nodeTypeEntry entry in entryArray)
			{
				if (entry.Type == oType)
				{
					if (haveCollection && entry.ID == collection.ID)
						haveCollection = false;
					else
						na.Add(entry.ID);
				}
			}

			return (string[])na.ToArray(typeof(string));
		}

		/// <summary>
		/// Get an array of the IDs to delete on the server.
		/// </summary>
		/// <returns></returns>
		internal string[] DeletesToServer()
		{
			return ToServer(SyncNodeType.Tombstone);
		}

		/// <summary>
		/// Get an array of the IDs of the Nodes to push up to the server.
		/// </summary>
		/// <returns></returns>
		internal string[] GenericsToServer()
		{
			return ToServer(SyncNodeType.Generic);
		}

		/// <summary>
		/// Get an array of the IDs of the Directory Nodes to push up to the server.
		/// </summary>
		/// <returns></returns>
		internal string[] DirsToServer()
		{
			return ToServer(SyncNodeType.Directory);
		}

		/// <summary>
		/// Get an array of the IDs of the File Nodes to push up to the server.
		/// </summary>
		/// <returns></returns>
		internal string[] FilesToServer()
		{
			return ToServer(SyncNodeType.File);
		}
        /// <summary>
        /// Checks if the node is new to be downloaded
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        bool NodeIsNew(string nodeID)
        {
            Node oldNode = collection.GetNodeByID(nodeID);
            if ((oldNode == null && !sparseReplica))
                return true;
            return false;
        }
		/// <summary>
		/// Checks to see if the node needs updating.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="MasterIncarnation"></param>
		/// <returns></returns>
		bool NodeHasChanged(string nodeID, ulong MasterIncarnation, out bool newNode)
		{
            newNode = false;
			Node oldNode = collection.GetNodeByID(nodeID);
			if ((oldNode == null && !sparseReplica))
                return newNode = true;
            else if(oldNode.MasterIncarnation != MasterIncarnation)
				return true;
			return false;
		}

		/// <summary>
		/// Gets if the work is complete.
		/// </summary>
		internal bool Complete
		{
			get
			{
				int count = 0;
				count += nodesFromServerDownload.Count;
				count += nodesToServer.Count;
                count += nodesFromServerMerge.Count;
				
				return count == 0 ? true : false;
			}
		}

		/// <summary>
		/// Set the Access that is allowed to the collection.
		/// </summary>
		internal Access.Rights SetAccess
		{
			set
			{
				rights = value;
			}
		}

		/// <summary>
		/// Get the number of nodes that need to be synced.
		/// </summary>
		internal int Count
		{
			get
			{
				return nodesToServer.Count + nodesFromServerDownload.Count + nodesFromServerMerge.Count;
			}
		}

		/// <summary>
		/// Get the number of node to sync up to the server.
		/// </summary>
		internal int UpCount
		{
			get
			{
				return nodesToServer.Count;
			}
		}

		/// <summary>
		/// Get the number of node to sync down from the server.
		/// </summary>
		internal int DownCount
		{
			get
			{
				return nodesFromServerDownload.Count;
			}
		}

        /// <summary>
        /// Get the number of node to merge down from the server.
        /// </summary>
        internal int MergeCount
        {
            get
            {
                return nodesFromServerMerge.Count;
            }
        }

    }
	#endregion
}
