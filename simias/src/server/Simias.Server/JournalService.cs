/***********************************************************************
 *  $RCSfile: JournalService.cs,v $
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
 *  Author: Bruce Getter <bgetter@novell.com>
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

namespace Novell.Journaling
{
	/// <summary>
	/// Class that manages journaling of store events.
	/// </summary>
	public class JournalService : IThreadService
	{
		#region Class Members

		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(JournalService));
		//static Hashtable journals;
		EventSubscriber storeEvents;
		Queue eventQueue;
		ManualResetEvent queueEvent;
		Thread journalThread;
		bool shuttingDown;

		#endregion

		#region Properties
		#endregion

		#region Private Methods

		private void processEvent()
		{
			while (!shuttingDown)
			{
				// Wait for something to be added to the queue.
				queueEvent.WaitOne();

				// Now loop until the queue is emptied.
				while (true)
				{
					NodeEventArgs args;
					lock (eventQueue)
					{
						if (eventQueue.Count == 0)
						{
							queueEvent.Reset();
							break;
						}

						args = eventQueue.Dequeue() as NodeEventArgs;
					}

					// Process the event.
					if (!shuttingDown)
					{
						if ( args.Type.Equals( NodeTypes.FileNodeType ) || args.Type.Equals( NodeTypes.DirNodeType ) )
						{
							Journal journal = new Journal( args.Collection );
							journal.UpdateJournal( args );
							journal.Commit();
						}
					}
					else
					{
						log.Info( "Lost journal entry for node ID = '{0}'. Event type = '{1}'. Node type = '{2}'", args.Node, args.EventType, args.Type );
					}
				}
			}
		}

		private void storeEvents_NodeEvent(NodeEventArgs args)
		{
			lock (eventQueue)
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

			journalThread = new Thread( new ThreadStart(processEvent));
			journalThread.IsBackground = true;
			journalThread.Start();

			storeEvents = new EventSubscriber();
			//			storeEvents.NodeTypeFilter
			storeEvents.NodeChanged += new NodeEventHandler(storeEvents_NodeEvent);
			storeEvents.NodeCreated += new NodeEventHandler(storeEvents_NodeEvent);
			storeEvents.NodeDeleted += new NodeEventHandler(storeEvents_NodeEvent);
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
			journalThread.Join();
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
}
