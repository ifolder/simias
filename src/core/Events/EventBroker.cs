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
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Simias;
using Simias.Client.Event;


namespace Simias.Event
{
	#region Delegate Definitions.

	/// <summary>
	/// Delegate definition for handling collection events.
	/// </summary>
	public delegate void SimiasEventHandler(SimiasEventArgs args);

	#endregion

	#region EventBroker class

	/// <summary>
	/// Class used to broker events to the subscribed clients.
	/// </summary>
	internal class EventBroker
	{
		#region Fields

		internal static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(EventBroker));
		static EventBroker instance = new EventBroker();
		Queue	eventQueue = new Queue();
		AutoResetEvent haveEvents = new AutoResetEvent(false);
		public event SimiasEventHandler SimiasEvent;

		#endregion

		#region Constructor

		private EventBroker()
		{
			Thread t1 = new Thread(new ThreadStart(EventQueueThread));
            t1.Name = "Event_1 Queue Thread";
            t1.Priority = ThreadPriority.BelowNormal;
			t1.IsBackground = true;
			t1.Start();

			Thread t2 = new Thread(new ThreadStart(EventQueueThread));
            t2.Name = "Event_2 Queue Thread";
            t2.Priority = ThreadPriority.BelowNormal;
			t2.IsBackground = true;
			t2.Start();

			Thread t3 = new Thread(new ThreadStart(EventQueueThread));
            t3.Name = "Event_3 Queue Thread";
            t3.Priority = ThreadPriority.BelowNormal;
			t3.IsBackground = true;
			t3.Start();
		}

		#endregion

		#region EventQueueThread
        /// <summary>
        /// Thread that waits for the events and process them
        /// </summary>
		void EventQueueThread()
		{
			try
			{
				// Loop forever waiting for events to deliver.
				while (true)
				{
					haveEvents.WaitOne();
					while (true)
					{
						// We have at least one event loop until the queue is empty.
						SimiasEventArgs args = null;
						lock (eventQueue)
						{
							if (eventQueue.Count > 0)
							{
								args = (SimiasEventArgs)eventQueue.Dequeue();
							}
							else
							{
								break;
							}
						}
				
						if (SimiasEvent != null)
						{
							Delegate[] cbList = SimiasEvent.GetInvocationList();
							foreach (SimiasEventHandler cb in cbList)
							{
								try 
								{ 
									cb(args);
									/*
									cb.BeginInvoke(
										args, 
										new AsyncCallback(EventRaisedCallback), 
										null);
									*/
								}
								catch (Exception ex)
								{
									logger.Debug(ex, "Delegate {0}.{1} failed", cb.Target, cb.Method);
								}
							}
						}
					}
				}
			}
			catch {}
		}

		#endregion

		#region Event Signalers

		/// <summary>
		/// Called to raise an event.
		/// </summary>
		/// <param name="args">The arguments for the event.</param>
		public void RaiseEvent(SimiasEventArgs args)
		{
			lock (eventQueue)
			{
				eventQueue.Enqueue(args);
				haveEvents.Set();
			}
		}

		/// <summary>
		/// Call back from the async event invocation.
		/// </summary>
		/// <param name="ar">Results of the call.</param>
		public void EventRaisedCallback(IAsyncResult ar)
		{
			SimiasEventHandler eventDelegate = null;
			try
			{
				eventDelegate = (SimiasEventHandler)((AsyncResult)ar).AsyncDelegate;
				eventDelegate.EndInvoke(ar);
				// the call is successfully finished and 
			}
			catch {}
		}


		#endregion

		#region statics

		/// <summary>
		/// Get a broker for the given simias store.
		/// </summary>
		/// <returns></returns>
		public static EventBroker GetBroker()
		{
			return instance;
		}

		#endregion
	}

	#endregion
}
