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
*                 $Author: Rob
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
using System.Diagnostics;

using Simias.Client;
using Simias.Client.Event;
using Simias.DomainServices;
using Simias.Service;
using Simias.Storage;

namespace Simias.POBox
{
	/// <summary>
	/// PO Service
	/// </summary>
	public class POService : IThreadService
	{
		#region Class Members

		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( POService ) );

		/// <summary>
		/// Subscribers used to watch for important POBox events.
		/// </summary>
		private EventSubscriber poBoxSubscriber;
		private EventSubscriber noAccessSubscriber;
#if ( !REMOVE_OLD_INVITATION )
		private EventSubscriber invitationSubscriber;

		/// <summary>
		/// Object used to send and receive subscriptions from the server.
		/// </summary>
		private static SubscriptionService subscriptionService = null;
#endif

		/// <summary>
		/// Hashtable used to map from a POBox ID to its domain.
		/// </summary>
		private Hashtable poBoxTable = Hashtable.Synchronized( new Hashtable() );

		/// <summary>
		/// Store object.
		/// </summary>
		private Store store;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public POService()
		{
			// events
			poBoxSubscriber = new EventSubscriber();
			poBoxSubscriber.Enabled = false;
			poBoxSubscriber.NodeTypeFilter = NodeTypes.POBoxType;
			poBoxSubscriber.NodeCreated += new NodeEventHandler(OnPOBoxCreated);
			poBoxSubscriber.NodeDeleted += new NodeEventHandler(OnPOBoxDeleted);

#if ( !REMOVE_OLD_INVITATION )
			invitationSubscriber = new EventSubscriber();
			invitationSubscriber.Enabled = false;
			invitationSubscriber.NodeTypeFilter = NodeTypes.SubscriptionType;
			invitationSubscriber.NodeCreated += new NodeEventHandler(OnSubscriptionChanged);
			invitationSubscriber.NodeChanged += new NodeEventHandler(OnSubscriptionChanged);
#endif
			// Removes invitations from POBoxes.
			noAccessSubscriber = new EventSubscriber();
			noAccessSubscriber.Enabled = false;
			noAccessSubscriber.NoAccess += new NodeEventHandler(OnCollectionNoAccess);

			store = Store.GetStore();
		}

		#endregion

		#region Private Methods

        /// <summary>
        /// Call back when PO Box is created
        /// </summary>
        /// <param name="args"></param>
		private void OnPOBoxCreated(NodeEventArgs args)
		{
			// Get the POBox that caused the event.
			POBox poBox = POBox.GetPOBoxByID( store, args.ID );
			if ( poBox != null )
			{
				// Save the domain ID for this POBox.
				poBoxTable[ args.ID ] = poBox.Domain;
			}
		}

        /// <summary>
        /// Call back when PO Box is deleted
        /// </summary>
        /// <param name="args"></param>
		private void OnPOBoxDeleted(NodeEventArgs args)
		{
			// Get the domain ID for this PO box from its name.
			string domainID = poBoxTable[ args.ID ] as string;
			if ( domainID != null )
			{
				// This POBox is being deleted. Call to get rid of the domain information.
				new DomainAgent().RemoveDomainInformation( domainID );
				poBoxTable.Remove( args.ID );
			}
		}

		/// <summary>
		/// Removes all subscriptions for the collection that is contained in the event.
		/// </summary>
		/// <param name="args">Node event arguments.</param>
		private void OnCollectionNoAccess( NodeEventArgs args )
		{
			// Make sure that this is an event for a collection.
			if ( args.Collection == args.ID )
			{
				// Search the POBox collections for a subscription for this collection.
				Property p = new Property( Subscription.SubscriptionCollectionIDProperty, args.ID );

				// Find all of the subscriptions for this POBox.
				ICSList list = store.GetNodesByProperty( p, SearchOp.Equal );
				foreach (ShallowNode sn in list)
				{
					// Make sure that this node is a subscription.
					if ( sn.Type == NodeTypes.SubscriptionType )
					{
						// Get the collection (POBox) for this subscription.
						POBox poBox = POBox.GetPOBoxByID( store, sn.CollectionID );
						if ( poBox != null )
						{
							// Delete this subscription from the POBox.
							poBox.Commit( poBox.Delete( new Subscription( poBox, sn ) ) );
						}
					}
				}
			}
		}

#if ( !REMOVE_OLD_INVITATION )
		/// <summary>
		/// Handles queuing of subscriptions to the subscription thread when they change.
		/// </summary>
		/// <param name="args"></param>
		private void OnSubscriptionChanged( NodeEventArgs args )
		{
			// Get the POBox for this subscription.
			POBox poBox = POBox.GetPOBoxByID( store, args.Collection );
			if ( poBox != null )
			{
				StartSubscription( poBox, args.ID );
			}
			else
			{
				log.Debug( "Error: OnSubscriptionChanged - Cannot find POBox {0} for subscription {1}.", args.Collection, args.ID );
			}
		}

		/// <summary>
		/// Queues the subscription to the subscription thread queue to start it processing.
		/// </summary>
		/// <param name="poBox">POBox that contains the subscription.</param>
		/// <param name="subscriptionID">The Node ID for the subscription.</param>
		private void StartSubscription( POBox poBox, string subscriptionID )
		{
			// See if this POBox belongs to an old domain.
			Domain domain = store.GetDomain( poBox.Domain );
			if ( ( domain != null ) && ( domain.SupportsNewInvitation == false ) )
			{
				// Get the subscription node.
				Subscription subscription = poBox.GetNodeByID( subscriptionID ) as Subscription;
				if ( subscription != null )
				{
					switch( subscription.SubscriptionState )
					{
							// invited (master)
						case SubscriptionStates.Invited:
							if ( subscription.Originator == store.LocalDomain )
							{
								subscriptionService.QueueSubscription( poBox, subscription );
							}
							break;

							// replied (slave)
						case SubscriptionStates.Replied:
							subscriptionService.QueueSubscription( poBox, subscription );
							break;

						default:
							break;
					}
				}
				else
				{
					log.Debug( "Error: OnSubscriptionChanged - Cannot find subscription {0} in POBox {1}.", subscriptionID, poBox.ID );
				}
			}
			else
			{
				if ( domain == null )
				{
					log.Debug( "Error: OnSubscriptionChanged - Cannot find domain {0}", poBox.Domain );
				}
			}
		}

		/// <summary>
		/// Looks for active subscriptions in the POBox and queues them to the subscription thread.
		/// </summary>
		/// <param name="poBox">POBox to look for subscriptions in.</param>
		private void ProcessWaitingSubscriptions( POBox poBox )
		{
			foreach( ShallowNode sn in poBox )
			{
				if ( sn.Type == NodeTypes.SubscriptionType )
				{
					StartSubscription( poBox, sn.ID );
				}
			}
		}
#endif

		#endregion

		#region BaseProcessService Members

		/// <summary>
		/// Start the PO service.
		/// </summary>
		public void Start()
		{
#if ( !REMOVE_OLD_INVITATION )
			lock ( typeof( POService ) )
			{
				if ( subscriptionService == null )
				{
					// Start the subscription thread.
					subscriptionService = new SubscriptionService();
				}
			}
#endif
			// Get a list of all POBoxes.
			ICSList poBoxList = store.GetCollectionsByType( NodeTypes.POBoxType );
			foreach( ShallowNode sn in poBoxList )
			{
				// Add the existing POBoxes to the mapping table.
				POBox poBox = new POBox( store, sn );
				poBoxTable[ poBox.ID ] = poBox.Domain;

#if ( !REMOVE_OLD_INVITATION )
				// Process any active subscriptions.
				ProcessWaitingSubscriptions( poBox );
#endif
			}

			poBoxSubscriber.Enabled = true;
			invitationSubscriber.Enabled = true;
#if ( !REMOVE_OLD_INVITATION )
			noAccessSubscriber.Enabled = true;
#endif
		}

		/// <summary>
		/// Stop the PO service.
		/// </summary>
		public void Stop()
		{
			poBoxSubscriber.Enabled = false;
			invitationSubscriber.Enabled = false;
#if ( !REMOVE_OLD_INVITATION )
			noAccessSubscriber.Enabled = false;

			lock ( typeof( POService ) )
			{
				if ( subscriptionService != null )
				{
					// Stop the subscription thread.
					subscriptionService.Stop();
					subscriptionService = null;
				}
			}
#endif
		}

		/// <summary>
		/// Resume the PO service.
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Pause the PO service.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// A custom event for the PO service.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public int Custom( int message, string data )
		{
			return 0;
		}

		#endregion
	}
}
