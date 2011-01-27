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
using System.Xml;

using Simias.Client;
using Simias.Storage;
using Simias.Sync;

namespace Simias.POBox
{
	/// <summary>
	/// A POBox object is a specialized collection used to hold messages.
	/// </summary>
	public class POBox : Collection
	{
		#region Class Members
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(POBox));

		/// <summary>
		/// The name of the property storing the DirNode name.
		/// </summary>
		public const string POServiceUrlProperty = "POServiceUrl";

		#endregion

		#region Properties
		
		/// <summary>
		/// Gets/sets the post-office service url.
		/// </summary>
		public string POServiceUrl
		{
			get
			{
				string result = null;

				Property p = Properties.GetSingleProperty(POServiceUrlProperty);

				if (p != null)
				{
					result = p.ToString();
				}
				else
				{
					result = this.MasterUrl.ToString() + "/POService.asmx";
				}

				return result;
			}
			set
			{
				Property p = new Property(POServiceUrlProperty, value);
				p.LocalProperty = true;

				Properties.ModifyProperty(p);
			}
		}

		#endregion

		#region Constructors
		/// <summary>
		/// Constructor to create a POBox object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this POBox belongs to.</param>
		/// <param name="node">Node object to construct POBox object from.</param>
		internal POBox(Store storeObject, Node node) :
			base (storeObject, node)
		{
		}

		/// <summary>
		/// Constructor to create a POBox object from a ShallowNode object.
		/// </summary>
		/// <param name="storeObject">The Store object that this POBox belongs to.</param>
		/// <param name="shallowNode">The ShallowNode object to contruct the POBox object from.</param>
		internal POBox(Store storeObject, ShallowNode shallowNode) :
			base (storeObject, shallowNode)
		{
		}

		/// <summary>
		/// Constructor to create an existing POBox object from an Xml document object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document object to construct this object from.</param>
		internal POBox( Store storeObject, XmlDocument document ) :
			base( storeObject, document )
		{
		}

		/// <summary>
		/// Constructor to create a POBox object.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox will belong to.</param>
		/// <param name="collectionName">The name of the POBox.</param>
		/// <param name="domainName">The name of the domain that the POBox belongs to.</param>
		public POBox(Store storeObject, string collectionName, string domainName) :
			this (storeObject, collectionName, Guid.NewGuid().ToString(), domainName)
		{
		}

		/// <summary>
		/// Constructor to create a POBox object.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox will belong to.</param>
		/// <param name="collectionName">The name of the POBox.</param>
		/// <param name="collectionID">The identifier of the POBox.</param>
		/// <param name="domainName">The name of the domain that the POBox belongs to.</param>
		internal POBox(Store storeObject, string collectionName, string collectionID, string domainName) :
			base( storeObject, collectionName, collectionID, NodeTypes.POBoxType, domainName )
		{
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Deserialize the serialized Subscription Node.
		/// </summary>
		/// <param name="subscription">The XML node.</param>
		/// <returns>The Subscription</returns>
		static private Subscription DeserializeSubscription(string subscription)
		{
			// Get the subscription Node.
			XmlDocument xNode = new XmlDocument();
			xNode.LoadXml(subscription);
			return (Subscription)Node.NodeFactory(Store.GetStore(), xNode);
		}
		
		#endregion

		#region Internal Methods
		/// <summary>
		/// POBox factory method that constructs a POBox object for the specified user in the specified domain.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox belongs to.</param>
		/// <param name="domainId">The ID of the domain that the POBox belongs to.</param>
		/// <param name="userId">The ID of the user that the POBox belongs to.</param>
		/// <returns></returns>
		public static POBox FindPOBox(Store storeObject, string domainId, string userId)
		{
			POBox poBox = null;

			// Build the name of the POBox.
			string name = "POBox:" + domainId + ":" + userId;

			// Search for the POBox.
			ICSEnumerator listEnum = storeObject.GetCollectionsByName(name).GetEnumerator() as ICSEnumerator;

			// There should only be one value returned...
			if (listEnum.MoveNext())
			{
				ShallowNode shallowNode = (ShallowNode)listEnum.Current;

				if (listEnum.MoveNext())
				{
					// TODO: multiple values were returned ... throw an exception.
				}

				poBox = new POBox(storeObject, shallowNode);
			}

			listEnum.Dispose();
			return poBox;
		}
		#endregion

		#region Public Methods
		
		/// <summary>
		/// POBox factory method that constructs a POBox object for the specified domain ID.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox belongs to.</param>
		/// <param name="domainId">The ID of the domain that the POBox belongs to.</param>
		/// <returns></returns>
		public static POBox GetPOBox(Store storeObject, string domainId)
		{
			return GetPOBox(storeObject, domainId, storeObject.GetUserIDFromDomainID(domainId));
		}

		/// <summary>
		/// POBox factory method that constructs a POBox object for the specified user in the specified domain.
		/// </summary>
		/// <param name="storeObject">The Store object that the POBox belongs to.</param>
		/// <param name="domainId">The ID of the domain that the POBox belongs to.</param>
		/// <param name="userId">The ID of the user that the POBox belongs to.</param>
		/// <returns></returns>
		public static POBox GetPOBox(Store storeObject, string domainId, string userId)
		{
			POBox poBox = FindPOBox( storeObject, domainId, userId );
			if (poBox == null)
			{
				// If the POBox cannot be found, create it.
				// Build the name of the POBox.
				string name = "POBox:" + domainId + ":" + userId;
				poBox = new POBox(storeObject, name, domainId);
				
				Domain domain = storeObject.GetDomain( domainId );
				Member current = domain.GetMemberByID(userId);

				Member member = new Member(current.Name, current.UserID, Access.Rights.ReadWrite);
				member.IsOwner = true;

				poBox.Commit(new Node[] { poBox, member });
			}

			return poBox;
		}

		/// <summary>
		/// POBox factory method that constructs a POBox object from it's id.
		/// </summary>
		/// <param name="store">The Store object that the POBox belongs to.</param>
		/// <param name="id">The ID of the POBox collection.</param>
		/// <returns>The POBox object.</returns>
		public static POBox GetPOBoxByID(Store store, string id)
		{
			return store.GetCollectionByID(id) as POBox;
		}
		
		/// <summary>
		/// Adds a message to the POBox object.
		/// </summary>
		/// <param name="message">The message to add to the collection.</param>
		public void AddMessage(Message message)
		{
			Commit(message);
		}

		/// <summary>
		/// Adds an array of Message objects to the POBox object.
		/// </summary>
		/// <param name="messageList">An array of Message objects to add to the POBox object.</param>
		public void AddMessage(Message[] messageList)
		{
			Commit(messageList);
		}

		/// <summary>
		/// Get all the Message objects that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name to search for.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the
		/// Message object(s) that have the specified name.</returns>
		public ICSList GetMessagesByName(string name)
		{
			return this.GetNodesByName(name);		
		}

		/// <summary>
		/// Get all the Message objects that have the specified type.
		/// </summary>
		/// <param name="type">A string containing the type to search for.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the
		/// Message object(s) that have the specified type.</returns>
		public ICSList GetMessagesByMessageType(string type)
		{
			return this.Search(Message.MessageTypeProperty, type, SearchOp.Equal);
		}



		/// <summary>
		/// Locates a Subscription in the POBox by CollectionID
		/// </summary>
		/// <param name="collectionID">The ID of the collection.</param>
		/// <returns>A Subscription object.</returns>
		public Subscription GetSubscriptionByCollectionID(string collectionID)
		{
			ICSList subList = this.Search(
						Subscription.SubscriptionCollectionIDProperty,
						collectionID,
						SearchOp.Equal);

			foreach(ShallowNode sNode in subList)
			{
				return new Subscription(this, sNode);
			}
			return null;
		}

		/// <summary>
		/// Locates a Subscription in the POBox by CollectionID and UserID.
		/// </summary>
		/// <param name="collectionID">The ID of the collection.</param>
		/// <param name="userID">The ID of the user to whom the subscription is addressed.</param>
		/// <returns>A Subscription object.</returns>
		public Subscription GetSubscriptionByCollectionID(string collectionID, string userID)
		{
			ICSList subList = this.Search(
				Subscription.SubscriptionCollectionIDProperty,
				collectionID,
				SearchOp.Equal);

			foreach (ShallowNode sn in subList)
			{
				Subscription sub = new Subscription(this, sn);
				if (sub.ToIdentity.Equals(userID))
				{
					return sub;
				}
			}

			return null;
		}


		/// <summary>
		/// Creates a Subscription object for the specified collection.
		/// </summary>
		/// <param name="collection">The Collection object that will be shared.</param>
		/// <param name="fromMember">The Member that is sharing the collection.</param>
		/// <param name="type"></param>
		/// <returns>A Subscription object.  This object must be added to the POBox using one of the AddMessage() methods.</returns>
		public Subscription CreateSubscription(Collection collection, Member fromMember, string type)
		{
			// Get the domain for this collection.
			Domain domain = Store.GetStore().GetDomain( collection.Domain );
			Subscription subscription = new Subscription(domain, collection.Name + " Subscription", Message.OutboundMessage, fromMember.UserID);

			subscription.FromName = fromMember.Name;
			subscription.FromIdentity = fromMember.UserID;
			subscription.FromPublicKey = fromMember.PublicKey;
			subscription.SubscriptionCollectionName = collection.Name;
			subscription.SubscriptionCollectionID = collection.ID;
			subscription.DomainID = collection.Domain;
			subscription.DomainName = domain.Name;
			subscription.SubscriptionCollectionType = type;
			subscription.SubscriptionKey = Guid.NewGuid().ToString();
			subscription.Originator = collection.StoreReference.LocalDomain;
			subscription.HostID = collection.HostID;

			// TODO: clean this up
			subscription.HasDirNode = (collection != null) ? (collection.GetRootDirectory() != null) : false;

			return subscription;
		}

		/*
		/// <summary>
		/// Creates a subscription in the POBox of the ToUser in the Subscription.
		/// </summary>
		/// <param name="subscription"></param>
		/// <returns></returns>
		public static POBoxStatus SaveSubscription(string subscription)
		{
			// Get the subscription Node.
			Subscription sub = DeserializeSubscription(subscription);
			
			log.Debug("Creating Subscription for {0}", sub.ToName);
			
			// Now Set the subscription in the POBox of the recipient.
			POBox pobox = POBox.GetPOBox(Store.GetStore(), sub.DomainID, sub.ToIdentity);
			pobox.ImportNode(sub, true, 1);
			pobox.Commit(sub);
			log.Debug("Subscription create end");
			return POBoxStatus.Success;
		}*/

		/// <summary>
		/// Removes the collection from the server if the current user is the owner. Otherwise the
		/// current user's membership is removed from the collection.
		/// </summary>
		/// <param name="subscription">Subscription to the collection.</param>
		public static void RemoveCollectionBySubscription( string subscription )
		{
			Store store = Store.GetStore();
			Subscription sub = DeserializeSubscription(subscription);
#if ( !REMOVE_OLD_INVITATION )
			Domain domain = store.GetDomain( sub.DomainID );
			if ( ( domain != null ) && ( domain.SupportsNewInvitation == true ) )
			{
#endif
				// Get the collection that this subscription represents.
				Collection collection = store.GetCollectionByID( sub.SubscriptionCollectionID );
				if ( collection != null )
				{
					// See if the invitee is the owner of this collection.
					if ( collection.Owner.UserID == sub.ToIdentity )
					{
						log.Debug( "RemoveCollectionBySubscription - Invitee {0} is owner.", sub.ToIdentity );
						log.Debug( "RemoveCollectionBySubscription - Removing collection {0}.", collection.ID );

						// The current principal is the owner of the collection. Delete the entire collection.
						collection.Commit( collection.Delete() );
					}
					else
					{
						log.Debug( "RemoveCollectionBySubscription - Invitee {0} is not owner.", sub.ToIdentity );

						// The invitee is only a member of the collection. Remove the membership.
						Member member = collection.GetMemberByID( sub.ToIdentity );
						if ( member != null )
						{
							// No need to process further invitation events. The subscription for this member
							// has already been removed.
							member.CascadeEvents = false;

							// Remove the member from the collection.
							collection.Commit( collection.Delete( member ) );
							log.Debug( "RemoveCollectionBySubscription - Removing membership for {0} from collection {1}.", member.UserID, collection.ID );
						}
						else
						{
							log.Debug( "RemoveCollectionBySubscription - Cannot find member {0} in collection {1}.", sub.ToIdentity, collection.ID );
						}
					}
				}
				else
				{
					log.Debug( "RemoveCollectionBySubscription - Subscription for collection {0} was declined.", sub.SubscriptionCollectionID );
				}
#if ( !REMOVE_OLD_INVITATION )
			}
#endif
		}

		/// <summary>
		/// Removes all subscriptions associated with this collection.
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="collectionID"></param>
		/// <param name="userID"></param>		
		public static void RemoveSubscriptionsForCollection(string domainID, string collectionID)
		{
			Store store = Store.GetStore();
#if ( !REMOVE_OLD_INVITATION )
			Domain domain = store.GetDomain( domainID );
			if ( ( domain != null ) && ( domain.SupportsNewInvitation == true ) )
			{
#endif
				ICSList subList = store.GetNodesByProperty( new Property( Subscription.SubscriptionCollectionIDProperty, collectionID ), SearchOp.Equal );
				if ( subList.Count > 0 )
				{
					foreach( ShallowNode sn in subList )
					{
						// The collection for the subscription nodes will be the POBox.
						Collection collection = store.GetCollectionByID( sn.CollectionID );
						if ( collection != null )
						{
							// No need to process further invitation events. The collection associated with
							// this subscription has already been removed.
							Subscription subscription = new Subscription( collection, sn );
							subscription.CascadeEvents = false;
							collection.Commit( collection.Delete( subscription ) );
							log.Debug( "RemoveSubscriptionsForCollection - Removed subscription {0} for collection {1}.", sn.ID, sn.CollectionID );
						}
						else
						{
							log.Debug( "RemoveSubscriptionsForCollection - Cannot find POBox {0}.", sn.CollectionID );
						}
					}
				}
				else
				{
					log.Debug( "RemoveSubscriptionsForCollection - No subscriptions found for collection {0}.", collectionID );
				}
#if ( !REMOVE_OLD_INVITATION )
			}
#endif
		}

		/// <summary>
		/// Removes the subscription for this collection from the specified member.
		/// </summary>
		/// <param name="member">Member to remove subscription from.</param>
		public static void RemoveSubscriptionByMember( string domainID, string collectionID, string userID )
		{
			Store store = Store.GetStore();
#if ( !REMOVE_OLD_INVITATION )
			Domain domain = store.GetDomain( domainID );
			if ( ( domain != null ) && ( domain.SupportsNewInvitation == true ) )
			{
#endif
				// Get the member's POBox.
				POBox poBox = POBox.FindPOBox( store, domainID, userID );
				if ( poBox != null )
				{
					ICSList subList = poBox.Search( Subscription.SubscriptionCollectionIDProperty, collectionID, SearchOp.Equal );
					if ( subList.Count > 0 )
					{
						foreach( ShallowNode sn in subList )
						{
							// No need to process further invitation events. The collection cannot be deleted
							// by removing its members, because the owner of the collection can never be deleted.
							Subscription subscription = new Subscription( poBox, sn );
							subscription.CascadeEvents = false;
							poBox.Commit( poBox.Delete( subscription ) );
							log.Debug( "RemoveSubscriptionByMember - Removed subscription {0} from member {1}.", sn.ID, userID );
						}
					}
					else
					{
						log.Debug( "RemoveSubscriptionByMember - No subscriptions found for member {0}.", userID );
					}
				}
				else
				{
					log.Debug( "RemoveSubscriptionByMember - Cannot find POBox for member {0}.", userID );
				}
#if ( !REMOVE_OLD_INVITATION )
			}
#endif
		}

		#endregion
	}
}
