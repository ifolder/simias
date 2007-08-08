/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 | Author: Rob
 |***************************************************************************/


using System;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Diagnostics;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Storage;

namespace Simias.POBox
{
	/// <summary>
	/// POBox Manager
	/// </summary>
	public class POBoxManager : IDisposable
	{
		private POBox poBox;
		private Store store;
		private EventSubscriber subscriber;
		private SubscriptionService subscriptionService;

		/// <summary>
		/// Gets the domain that this poBox belongs to.
		/// </summary>
		public string Domain
		{
			get { return poBox.Domain; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="poManager"></param>
		/// <param name="id"></param>
		public POBoxManager(POManager poManager, string id)
		{
			// new store
			store = Store.GetStore();

			// open POBox
			poBox = POBox.GetPOBoxByID(store, id);

			// events
			subscriber = new EventSubscriber(poBox.ID);
			subscriber.Enabled = false;
			subscriber.NodeTypeFilter = NodeTypes.SubscriptionType;
			subscriber.NodeCreated += new NodeEventHandler(OnMessageChanged);
			subscriber.NodeChanged += new NodeEventHandler(OnMessageChanged);
		}

		/// <summary>
		/// Start the PO Box manager.
		/// </summary>
		public void Start()
		{
			subscriptionService = new SubscriptionService(poBox);
			subscriber.Enabled = true;
			
			// Indicate any new subscriptions in the POBox on startup.
			foreach(ShallowNode n in poBox)
			{
				if (n.Type == NodeTypes.SubscriptionType)
				{
					UpdateMessage(n.ID);
				}
			}
		}

		/// <summary>
		/// Stop the PO Box manager.
		/// </summary>
		public void Stop()
		{
			subscriber.Enabled = false;
			subscriptionService.Stop();
		}

		private void OnMessageChanged(NodeEventArgs args)
		{
			UpdateMessage(args.ID);
		}

		private void UpdateMessage(string id)
		{
			Subscription subNode = poBox.GetNodeByID(id) as Subscription;
			if (subNode != null)
			{
				UpdateSubscription(subNode);
			}
		}

		private void UpdateSubscription(Subscription subscription)
		{
			switch(subscription.SubscriptionState)
			{
				// invited (master)
				case SubscriptionStates.Invited:
					if ( subscription.Originator == store.LocalDomain )
					{
						subscriptionService.QueueSubscription( subscription );
					}
					break;

				// replied (slave)
				case SubscriptionStates.Replied:
					subscriptionService.QueueSubscription( subscription );
					break;

				default:
					break;
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
		}

		#endregion
	}
}
