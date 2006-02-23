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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Web;

namespace Simias.POBoxService.Web
{
	/// <summary>
	/// Status codes returned from POBoxService methods
	/// </summary>
	[Serializable]
	public enum POBoxStatus
	{
		/// <summary>
		/// The method was successful.
		/// </summary>
		Success,

		/// <summary>
		/// The specified PO Box was not found.
		/// </summary>
		UnknownPOBox,

		/// <summary>
		/// The specified identity was not found in the domain.
		/// </summary>
		UnknownIdentity,

#if ( !REMOVE_OLD_INVITATION )
		/// <summary>
		/// The specified subscription was not found.
		/// </summary>
		UnknownSubscription,
#endif

		/// <summary>
		/// The specified collection was not found.
		/// </summary>
		UnknownCollection,

		/// <summary>
		/// The specified domain was not found.
		/// </summary>
		UnknownDomain,

		/// <summary>
		/// The suscription was in an invalid state for the method
		/// </summary>
		InvalidState,

		/// <summary>
		/// The access rights were invalid during an inviate
		/// </summary>
		InvalidAccessRights,

#if ( !REMOVE_OLD_DOMAIN )
		/// <summary>
		/// The subscription was already accepted by another client.
		/// </summary>
		AlreadyAccepted,

		/// <summary>
		/// The subscription was already denied by another client.
		/// </summary>
		AlreadyDeclined,

		/// <summary>
		/// The invitation has not moved to the posted state yet.
		/// </summary>
		NotPosted,
#endif
		/// <summary>
		/// An unknown error was realized.
		/// </summary>
		UnknownError
	};

	/// <summary>
	/// Object used for inviting, accepting/declining subscriptions etc.
	/// </summary>
	[Serializable]
	public class SubscriptionMsg
	{
		/// <summary>
		/// Domain to invite and accept on
		/// </summary>
		public string DomainID;

		/// <summary>
		/// The ID of the user who sent the subscription
		/// </summary>
		public string FromID;

		/// <summary>
		/// The name of the user who sent the subscription
		/// </summary>
		public string FromName;

		/// <summary>
		/// The ID of the user who received the subscription
		/// </summary>
		public string ToID;

		/// <summary>
		/// The ID of the originating subscription
		/// Subscription ID are consistent in both the
		/// sender's and receiver's PO Boxes
		/// </summary>
		public string SubscriptionID;

		/// <summary>
		/// The ID of the collection the sender is wanting
		/// to share.
		/// </summary>
		public string SharedCollectionID;

		/// <summary>
		/// The friendly name of the collection the sender
		/// is wanting to share
		/// </summary>
		public string SharedCollectionName;

		/// <summary>
		/// The type of collection the sender is wanting
		/// to share
		/// </summary>
		public string SharedCollectionType;

		/// <summary>
		/// If the shared collection contains a directory
		/// node, the id will be set on the invite
		/// </summary>
		public string DirNodeID;

		/// <summary>
		/// If the shared collection contains a directory
		/// node, the node's name will be set on the invite
		/// </summary>
		public string DirNodeName;

		/// <summary>
		/// Access rights the sender is wishing to grant
		/// to the receiver for the shared collection
		/// This member is really only valid on the
		/// invite method
		/// </summary>
		public int	AccessRights;
	};

	/// <summary>
	/// Summary description for POBoxService
	/// </summary>
	/// 
	[WebService(Namespace="http://novell.com/simias/pobox/")]
	public class POBoxService : System.Web.Services.WebService
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(POBoxService));

		/// <summary>
		/// </summary>
		public POBoxService()
		{
		}

#if ( !REMOVE_OLD_INVITATION )
		/// <summary>
		/// Checks to see if the specified identity is already a member of the collection.
		/// </summary>
		/// <param name="store">Store handle.</param>
		/// <param name="collectionID">Identifier for the collection.</param>
		/// <param name="identity">Identity to check for membership.</param>
		/// <returns>AlreadyAccepted is returned if the identity is already a member of the collection.</returns>
		private POBoxStatus AlreadyAMember( Store store, string collectionID, string identity )
		{
			POBoxStatus status;

			Collection collection = store.GetCollectionByID( collectionID );
			if ( collection != null )
			{
				status = ( collection.GetMemberByID( identity ) != null ) ? 
					POBoxStatus.AlreadyAccepted :
					POBoxStatus.UnknownSubscription;
			}
			else
			{
				status = POBoxStatus.UnknownCollection;
			}

			return status;
		}
#endif

		/// <summary>
		/// Ping
		/// Method for clients to determine if POBoxService is
		/// up and running.
		/// </summary>
		/// <param name="sleepFor"></param>
		/// <returns>0</returns>
		[WebMethod(EnableSession = true)]
		public int Ping(int sleepFor)
		{
			Thread.Sleep(sleepFor * 1000);
			return 0;
		}

		/// <summary>
		/// Accept subscription
		/// </summary>
		/// <param name="subMsg"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public POBoxStatus AcceptedSubscription( SubscriptionMsg subMsg )
		{
#if ( !REMOVE_OLD_INVITATION )
			POBoxStatus status = POBoxStatus.UnknownError;
			Store store = Store.GetStore();

			// Check if this subscription belong to a domain that doesn't support the new
			// invitation method.
			Domain domain = store.GetDomain( subMsg.DomainID );
			if ( ( domain != null ) && ( domain.SupportsNewInvitation == false ) )
			{
				log.Debug( "AcceptedSubscription - called" );
				log.Debug( "  subscription ID: " + subMsg.SubscriptionID );
				log.Debug( "  collection ID: " + subMsg.SharedCollectionID );
				log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );

				try
				{
					if ( subMsg.ToID != Thread.CurrentPrincipal.Identity.Name )
					{
						log.Error( "Specified \"toIdentity\" is not the caller" );
						return( POBoxStatus.UnknownIdentity );
					}

					// open the post office box
					POBox.POBox poBox = Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID );
					if ( poBox != null )
					{
						// check that the message has already not been posted
						ICSList list = poBox.Search( Message.MessageIDProperty, subMsg.SubscriptionID, SearchOp.Equal );
						if ( list.Count == 0 )
						{
							// See if the toIdentity already exists in the memberlist of the shared collection
							log.Debug( "AcceptedSubscription - Subscription does not exist" );
							status = AlreadyAMember( store, subMsg.SharedCollectionID, subMsg.ToID );
						}
						else
						{
							// Subscription exists in the inviters PO box
							ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
							Subscription cSub = new Subscription( poBox, e.Current as ShallowNode );

							// Identities need to match up
							if ( ( subMsg.FromID == cSub.FromIdentity ) && ( subMsg.ToID == cSub.ToIdentity ) )
							{
								switch ( cSub.SubscriptionState )
								{
									case SubscriptionStates.Invited:
									{
										// Wait for the sender's subscription to be posted.
										status = POBoxStatus.NotPosted;
										break;
									}

									case SubscriptionStates.Posted:
									{
										// Accepted. Next state = Responded and disposition is set.
										try
										{
											cSub.Accept( store, cSub.SubscriptionRights );
											poBox.Commit( cSub );
											status = POBoxStatus.Success;
										}
										catch ( DoesNotExistException )
										{
											status = POBoxStatus.UnknownCollection;
										}
										break;
									}

									case SubscriptionStates.Responded:
									{
										// The subscription has already been accepted or declined.
										status = 
											( cSub.SubscriptionDisposition == SubscriptionDispositions.Accepted ) ? 
											POBoxStatus.AlreadyAccepted :
											POBoxStatus.AlreadyDeclined;
										break;
									}

									default:
									{
										log.Debug( "  invalid accept state = {0}", cSub.SubscriptionState.ToString() );
										status = POBoxStatus.InvalidState;
										break;
									}
								}
							}
							else
							{
								log.Debug( "  to or from identity does not match" );
								status = POBoxStatus.UnknownIdentity;
							}

							e.Dispose();
						}
					}
					else
					{
						status = POBoxStatus.UnknownPOBox;
					}
				}
				catch(Exception e)
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
				}
			
				log.Debug( "AcceptedSubscription exit  status: " + status.ToString() );
			}
			else
			{
				log.Debug( "DEBUG - Obsolete AcceptedSubscription called" );
				status = POBoxStatus.Success;
			}

			return status;
#else
			log.Debug( "DEBUG - Obsolete AcceptedSubscription called" );
			return POBoxStatus.Success;
#endif			
		}

		/// <summary>
		/// Decline subscription
		/// </summary>
		/// <param name="subMsg"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public POBoxStatus DeclinedSubscription( SubscriptionMsg subMsg )
		{
			POBoxStatus status = POBoxStatus.Success;

			log.Debug( "DeclinedSubscription - called" );
			log.Debug( "  subscription ID: " + subMsg.SubscriptionID );
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );

			if ( subMsg.ToID != Thread.CurrentPrincipal.Identity.Name )
			{
				log.Error( "  specified \"toIdentity\" is not the caller" );
				return POBoxStatus.UnknownIdentity;
			}

			// open the post office box of the To user
			Store store = Store.GetStore();
			POBox.POBox toPOBox = Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.ToID );
			if ( toPOBox != null )
			{
				// Get the subscription from the caller's PO box
				ICSList list = toPOBox.Search( Message.MessageIDProperty, subMsg.SubscriptionID, SearchOp.Equal);
				if ( list.Count == 0 )
				{
					// Assume that the subscription has already been declined and cleaned up by
					// a different client. Just return successfully from here.
					log.Debug( "  subscription: " + subMsg.SubscriptionID + " does not exist");
				}
				else
				{
					// Get the subscription object
					ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
					Subscription cSub = new Subscription( toPOBox, e.Current as ShallowNode );
					e.Dispose();

#if ( !REMOVE_OLD_INVITATION )
					// Get the domain to determine if it supports the new invitation model.
					Domain domain = store.GetDomain( cSub.DomainID );
					if ( ( domain != null ) && ( domain.SupportsNewInvitation == false ) )
					{
						// The subscription must be in the Replied, Received or Ready State to decline.
						if ( ( cSub.SubscriptionState == SubscriptionStates.Replied ) || 
							( cSub.SubscriptionState == SubscriptionStates.Received ) ||
							( cSub.SubscriptionState == SubscriptionStates.Ready ) )
						{
							// Identities need to match up
							if ( ( subMsg.FromID == cSub.FromIdentity ) && ( subMsg.ToID == cSub.ToIdentity ) )
							{
								// Validate the shared collection
								Collection cCol = store.GetCollectionByID( cSub.SubscriptionCollectionID );
								if ( cCol != null )
								{
									//
									// Actions taken when a subscription is declined
									//
									// If I'm the owner of the shared collection then the
									// decline is treated as a delete of the shared collection
									// so we must.
									// 1) Delete all the subscriptions in all members PO boxes
									// 2) Delete all outstanding subscriptions assigned to the
									//    shared collection ID.
									// 3) Delete the shared collection itself
									//
									// If I'm already a member of the shared collection but not
									// the owner.
									// 1) Remove myself from the member list of the shared collection
									// 2) Delete my subscription to the shared collection
									//
									// If I'm not yet a member of the shared collection but declined
									// an invitation from another user in the system.  In this case
									// the From and To identies will be different.
									// 1) Delete my subscription to the shared collection.
									// 2) Set the state of the subscription in the inviter's PO
									//    Box to "declined".
									//

									Member toMember = cCol.GetMemberByID( subMsg.ToID );
									if ( toMember == null )
									{
										log.Debug( "  handling case where identity is declining a subscription" );

										// I am not a member of this shared collection and want to decline the subscription.
										// Open the post office box of the from and decline the subscription
										POBox.POBox fromPOBox = 
											POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID );

										if ( fromPOBox != null )
										{
											Subscription cFromMemberSub = 
												fromPOBox.GetSubscriptionByCollectionID( cCol.ID, subMsg.ToID );
											if( cFromMemberSub != null )
											{
												log.Debug(  "declining subscription in fromPOBox." );
												cFromMemberSub.Decline();
												fromPOBox.Commit( cFromMemberSub );
											}
										}

										// Remove the subscription from the "toIdentity" PO box
										log.Debug( "  removing subscription from toPOBox." );
										toPOBox.Commit( toPOBox.Delete( cSub ) );
									}
									else if ( toMember.IsOwner )
									{
										// I am the owner of the shared collection?
										log.Debug( "  handling case where identity is owner of collection" );

										ICSList memberlist = cCol.GetMemberList();
										foreach( ShallowNode sNode in memberlist )
										{
											Member cMember = new Member( cCol, sNode );

											// Get the member's POBox
											POBox.POBox memberPOBox = 
												POBox.POBox.FindPOBox(
												store, 
												cCol.Domain, 
												cMember.UserID );

											if ( memberPOBox != null )
											{
												// Search for the matching subscription
												Subscription memberSub = 
													memberPOBox.GetSubscriptionByCollectionID( cCol.ID, cMember.UserID );

												if( memberSub != null )
												{
													log.Debug( "  removing invitation from toPOBox." );
													memberPOBox.Commit( memberPOBox.Delete( memberSub ) );
												}
											}
										}

										// Now search for all nodes that contain the "sbColID" property
										// which will find all subscriptions to users that have not
										// accepted or declined the subscription

										Property sbProp = new Property( "SbColID", cCol.ID );
										ICSList subList = store.GetNodesByProperty( sbProp, SearchOp.Equal );
										foreach ( ShallowNode sn in subList )
										{
											Collection col = store.GetCollectionByID( sn.CollectionID );
											if ( col != null )
											{
												Subscription sub = new Subscription( col, sn );
												if ( sub != null )
												{
													col.Commit( col.Delete( sub ) );
												}
											}
										}

										// Delete the shared collection itself
										log.Debug( "  deleting shared collection." );
										cCol.Commit( cCol.Delete() );
									}
									else
									{
										// I am a member of the shared collection.
										log.Debug( "  handling case where identity is a member of the collection" );
										cCol.Commit( cCol.Delete( toMember ) );

										// Remove the subscription from the "toIdentity" PO box
										Subscription cMemberSub = 
											toPOBox.GetSubscriptionByCollectionID( cCol.ID, toMember.UserID );
										if( cMemberSub != null )
										{
											log.Debug( "  removing subscription from owner's POBox." );
											toPOBox.Commit( toPOBox.Delete( cMemberSub ) );
										}

										if ( subMsg.FromID != subMsg.ToID )
										{
											// open the post office box of the From user
											POBox.POBox fromPOBox = 
												POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID ); 

											if ( fromPOBox != null )
											{
												// Remove the subscription from the "fromIdentity" PO box
												Subscription cFromMemberSub = 
													fromPOBox.GetSubscriptionByCollectionID( cCol.ID, toMember.UserID );

												if( cFromMemberSub != null )
												{
													log.Debug( "  removing subscription from toPOBox." );
													fromPOBox.Commit( fromPOBox.Delete( cFromMemberSub ) );
												}
											}
										}
									}

									status = POBoxStatus.Success;
								}
								else
								{
									// The shared collection does not exist but
									// if any subscriptions are still associated 
									// delete them from the store

									Property sbProp = new Property( "SbColID", cCol.ID );
									ICSList subList = store.GetNodesByProperty( sbProp, SearchOp.Equal );
									foreach ( ShallowNode sn in subList )
									{
										// Collection should be a PO Box
										Collection col = store.GetCollectionByID( sn.CollectionID );
										if ( col != null )
										{
											Subscription sub = new Subscription( col, sn );
											if ( sub != null )
											{
												col.Commit( col.Delete( sub ) );
											}
										}
									}

									log.Debug( "  collection not found" );
									status = POBoxStatus.UnknownCollection;
								}
							}
							else
							{
								log.Debug( "  from or to identity does not match" );
								status = POBoxStatus.UnknownIdentity;
							}

							e.Dispose();
						}
						else
						{
							// The Delivered state is the only other state that the subscription can be in. If
							// it is then it has already been accepted by another client.
							log.Debug( "  subscription has already been accepted." );
							status = POBoxStatus.AlreadyAccepted;
						}
					}
					else
					{
#endif
						// Remove the subscription from the "toIdentity" PO box
						log.Debug( "  removing subscription from toPOBox." );
						toPOBox.Commit( toPOBox.Delete( cSub ) );
#if ( !REMOVE_OLD_INVITATION )
					}
#endif
				}
			}
			else
			{
				status = POBoxStatus.UnknownPOBox;
			}

			log.Debug( "DeclinedSubscription exit  status: " + status.ToString() );
			return status;
		}

		/// <summary>
		/// Acknowledge the subscription.
		/// </summary>
		/// <param name="subMsg"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		AckSubscription( SubscriptionMsg subMsg )
		{
#if ( !REMOVE_OLD_INVITATION )
			POBoxStatus status;

			// Check to see if the domain supports the new invitation model.
			Store store = Store.GetStore();
			Domain domain = store.GetDomain( subMsg.DomainID );
			if ( ( domain != null ) && ( domain.SupportsNewInvitation == false ) )
			{
				log.Debug( "Acksubscription - called" );
				log.Debug( "  subscription: " + subMsg.SubscriptionID );
				log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name);

				if ( subMsg.ToID != Thread.CurrentPrincipal.Identity.Name )
				{
					log.Error( "specified \"toIdentity\" is not the caller" );
					return POBoxStatus.UnknownIdentity;
				}

				// open the post office box
				POBox.POBox poBox = Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID );
				if ( poBox != null )
				{
					ICSList list = poBox.Search( Message.MessageIDProperty, subMsg.SubscriptionID, SearchOp.Equal);
					if ( list.Count == 0 )
					{
						log.Debug( "  subscription: " + subMsg.SubscriptionID + " does not exist");

						// See if the toIdentity already exists in the memberlist of the shared collection.  If he has 
						// already accepted he can't decline from another machine.
						status = AlreadyAMember( store, subMsg.SharedCollectionID, subMsg.ToID );
					}
					else
					{
						// get the subscription object
						ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
						Subscription cSub = new Subscription( poBox, e.Current as ShallowNode );

						// Must be in the Responded state.
						switch ( cSub.SubscriptionState )
						{
							case SubscriptionStates.Responded:
							{
								// Identities need to match up
								if ( ( subMsg.FromID == cSub.FromIdentity ) && ( subMsg.ToID == cSub.ToIdentity ) )
								{
									// Delete the subscription from the inviters PO box.
									poBox.Commit( poBox.Delete(cSub) );
									status = POBoxStatus.Success;;
								}
								else
								{
									log.Debug( "  to or from identity does not match" );
									status = POBoxStatus.UnknownIdentity;
								}
								break;
							}

							default:
							{
								log.Debug( "  invalid state = {0}", cSub.SubscriptionState.ToString() );
								status = POBoxStatus.InvalidState;
								break;
							}
						}
					}
				}
				else
				{
					log.Debug("  PO Box not found");
					status = POBoxStatus.UnknownPOBox;
				}

				log.Debug( "AckSubscription exit  status: " + status.ToString() );
			}
			else
			{
				log.Debug( "DEBUG - Obsolete AckSubscription called" );
				status = POBoxStatus.Success;
			}

			return status;
#else
			log.Debug( "DEBUG - Obsolete AckSubscription called" );
			return POBoxStatus.Success;
#endif
		}

		/// <summary>
		/// Get the subscription information
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="subscriptionID"></param>
		/// <param name="collectionID"></param>
		/// <returns>success:subinfo  failure:null</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		SubscriptionInformation 
		GetSubscriptionInfo(string domainID, string identityID, string subscriptionID, string collectionID)
		{
#if ( !REMOVE_OLD_INVITATION )
			SubscriptionInformation subInfo = null;

			// See if the domain support the new invitation model.
			Store store = Store.GetStore();
			Domain domain = store.GetDomain( domainID );
			if ( ( domain != null ) && ( domain.SupportsNewInvitation == false ) )
			{
				subInfo = new SubscriptionInformation();

				log.Debug("GetSubscriptionInfo - called");
				log.Debug("  for subscription: " + subscriptionID);

				// open the post office box
				POBox.POBox poBox =	Simias.POBox.POBox.FindPOBox(store, domainID, identityID);
				if (poBox != null)
				{
					ICSList list = poBox.Search( Message.MessageIDProperty, subscriptionID, SearchOp.Equal );
					if ( list.Count == 0 )
					{
						log.Debug("  subscription does not exist");
						subInfo.Status = AlreadyAMember( store, collectionID, identityID );
					}
					else
					{
						// Generate the subscription info object and return it
						ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
						Subscription cSub = new Subscription(poBox, e.Current as ShallowNode);

						switch ( cSub.SubscriptionState )
						{
							case SubscriptionStates.Responded:
							{
								// Validate the shared collection
								Collection cSharedCollection = store.GetCollectionByID(cSub.SubscriptionCollectionID);
								if (cSharedCollection != null)
								{
									subInfo.GenerateFromSubscription(cSub);
									subInfo.Status = POBoxStatus.Success;
								}
								else
								{
									log.Debug("  collection not found");
									subInfo.Status = POBoxStatus.UnknownCollection;
								}
								break;
							}

							default:
							{
								log.Debug( "  invalid state = {0}", cSub.SubscriptionState.ToString() );
								subInfo.Status = POBoxStatus.InvalidState;
								break;
							}
						}
					}
				}
				else
				{
					log.Debug("  getSubscriptionInfo - PO Box not found");
					subInfo.Status = POBoxStatus.UnknownPOBox;
				}

				log.Debug( "GetSubscriptionInfo exit  status: " + subInfo.Status.ToString() );
			}
			else
			{
				log.Debug( "DEBUG - Obsolete GetSubscriptionInfo called" );
			}

			return subInfo;
#else
			log.Debug( "DEBUG - Obsolete GetSubscriptionInfo called" );
			return null;
#endif
		}

		/// <summary>
		/// Creates a subscription in the POBox of the ToUser in the Subscription.
		/// </summary>
		/// <param name="subscription"></param>
		/// <returns></returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public POBoxStatus CreateSubscription(string subscription)
		{
			// Get the subscription Node.
			XmlDocument xNode = new XmlDocument();
			xNode.LoadXml(subscription);
			Subscription sub = (Subscription)Node.NodeFactory(Store.GetStore(), xNode);
			sub.Properties.State = PropertyList.PropertyListState.Add;
			
			log.Debug("Creating Subscription for {0}", sub.ToName);
			
			// Now Set the subscription in the POBox of the recipient.
			POBox.POBox pobox = POBox.POBox.GetPOBox(Store.GetStore(), sub.DomainID, sub.ToIdentity);
			pobox.Commit(sub);
			log.Debug("Subscription create end");
			return POBoxStatus.Success;
		}

		/// <summary>
		/// Invite a user to a shared collection
		/// </summary>
		/// <param name="subMsg"></param>
		/// <returns>True if successful. False if not.</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		Invite( SubscriptionMsg subMsg )
		{
			POBoxStatus status = POBoxStatus.UnknownError;

			log.Debug( "Invite - called" );
			log.Debug( "  DomainID: " + subMsg.DomainID );
			log.Debug( "  FromUserID: " + subMsg.FromID );
			log.Debug( "  ToUserID: " + subMsg.ToID );
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );

			// Verify the domain.
			Store store = Store.GetStore();
			Domain cDomain = store.GetDomain( subMsg.DomainID );
			if ( cDomain != null )
			{
#if ( !REMOVE_OLD_INVITATION )
				// See if the domain supports the new invitation model.
				if ( cDomain.SupportsNewInvitation )
				{
#endif
					Access.Rights rights;

					try
					{
						// Verify the requested access rights.
						rights = ( Access.Rights )Enum.ToObject( typeof( Access.Rights ), subMsg.AccessRights ); 
					}
					catch
					{
						log.Debug( "  invalid access rights: {0}", subMsg.AccessRights );
						return POBoxStatus.InvalidAccessRights;
					}

					// Verify and get additional information about the "To" user
					Member toMember = cDomain.GetMemberByID( subMsg.ToID );
					if ( toMember != null )
					{
						// Don't check for the fromMember in the domain if this is workgroup.
						if ( cDomain.ConfigType != Domain.ConfigurationType.Workgroup )
						{
							// In peer-to-peer the collection won't exist 
							Collection collection = store.GetCollectionByID( subMsg.SharedCollectionID ); 
							if ( collection != null )
							{
								// Verify that the from user exists in the collection which will also verify
								// them as being in the domain. Use the current principal because the
								// FromID can be spoofed.
								Member fromMember = collection.GetMemberByID( Thread.CurrentPrincipal.Identity.Name );
								if ( fromMember != null )
								{
									// Impersonate the caller so we obtain their access rights.
									collection.Impersonate( fromMember );
									try
									{
										// Add the new member to the collection.
										collection.Commit( new Member( toMember.Name, toMember.UserID, rights ) );
										status = POBoxStatus.Success;
									}
									catch ( Simias.Storage.AccessException )
									{
										log.Debug( "  caller {0} has invalid access rights.", fromMember.UserID );
										status = POBoxStatus.InvalidAccessRights;
									}
									catch ( Exception ex )
									{
										log.Debug( "  commit exception - {0}", ex.Message );
										status = POBoxStatus.UnknownError;
									}
									finally
									{
										collection.Revert();
									}

									// Remove the status subscription.
									if ( status == POBoxStatus.Success )
									{
										RemoveStatusSubscription( store, subMsg );
									}
								}
								else
								{
									log.Debug( "  sender {0} does not exist in the domain!", Thread.CurrentPrincipal.Identity.Name );
									status = POBoxStatus.UnknownIdentity;
								}
							}
							else
							{
								log.Debug( "  shared collection {0} does not exist on enterprise", subMsg.SharedCollectionID );
								status = POBoxStatus.UnknownCollection;
							}
						}
						else
						{
							// Look up the invitee's POBox.
							POBox.POBox poBox = POBox.POBox.GetPOBox( store, subMsg.DomainID, subMsg.ToID );
							if ( poBox != null )
							{
								// Create the subscription to put into the invitee's POBox.
								Subscription subscription = new Subscription( 
									cDomain,
									subMsg.SharedCollectionName + " subscription", 
									"Subscription", 
									subMsg.FromID );

								subscription.SubscriptionState = Simias.POBox.SubscriptionStates.Ready;
								subscription.ToName = toMember.Name;
								subscription.ToIdentity = subMsg.ToID;
								subscription.FromName = subMsg.FromName;
								subscription.FromIdentity = subMsg.FromID;
								subscription.SubscriptionRights = rights;
								subscription.MessageID = subMsg.SubscriptionID;
								subscription.SubscriptionCollectionID = subMsg.SharedCollectionID;
								subscription.SubscriptionCollectionType = subMsg.SharedCollectionType;
								subscription.SubscriptionCollectionName = subMsg.SharedCollectionName;
								subscription.DomainID = cDomain.ID;
								subscription.DomainName = cDomain.Name;
								subscription.SubscriptionKey = Guid.NewGuid().ToString();
								subscription.MessageType = "Outbound";
								subscription.DirNodeID = subMsg.DirNodeID;
								subscription.DirNodeName = subMsg.DirNodeName;

								try
								{
									poBox.Commit( subscription );
									status = POBoxStatus.Success;
								}
								catch ( Exception ex )
								{
									log.Debug( "  commit exception - {0}", ex.Message );
									status = POBoxStatus.UnknownError;
								}
							}
							else
							{
								log.Debug( "  cannot find toUser {0} POBox", subMsg.ToID );
								status = POBoxStatus.UnknownPOBox;
							}
						}
					}
					else
					{
						log.Debug( "  specified toUser {0} does not exist in the domain!", subMsg.ToID );
						status = POBoxStatus.UnknownIdentity;
					}
#if ( !REMOVE_OLD_INVITATION )
				}
				else
				{
					Collection sharedCollection = null;
					Simias.POBox.POBox poBox = null;
					Subscription cSub = null;

					// Verify and get additional information about the "To" user
					Member toMember = cDomain.GetMemberByID( subMsg.ToID );
					if ( toMember == null )
					{
						log.Debug( "  specified \"toUserID\" does not exist in the domain!" );
						return POBoxStatus.UnknownIdentity;
					}

					// In peer-to-peer the collection won't exist 
					sharedCollection = store.GetCollectionByID( subMsg.SharedCollectionID ); 
					if ( sharedCollection == null )
					{
						log.Debug( "  shared collection does not exist" );
					}

					// Don't check for the fromMember in the domain if this is workgroup.
					if ( cDomain.ConfigType != Domain.ConfigurationType.Workgroup )
					{
						Member fromMember = cDomain.GetMemberByID( subMsg.FromID );
						if ( fromMember != null )
						{
							// Check that the sender has sufficient rights to invite.
							if ( sharedCollection != null )
							{
								Member collectionMember = sharedCollection.GetMemberByID( fromMember.UserID );
								if ( ( collectionMember == null ) || ( collectionMember.Rights != Access.Rights.Admin ) )
								{
									log.Debug( " sender does not have rights to invite to this collection." );
									return POBoxStatus.InvalidAccessRights;
								}
							}
							else
							{
								// The collection must exist in enterprise.
								log.Debug( " shared collection does not exist on enterprise" );
								return POBoxStatus.UnknownCollection;
							}
						}
						else
						{
							log.Debug( "  specified \"fromUserID\" does not exist in the domain!" );
							return POBoxStatus.UnknownIdentity;
						}
					}

					if ( subMsg.AccessRights > (int) Simias.Storage.Access.Rights.Admin)
					{
						return POBoxStatus.InvalidAccessRights;
					}

					try
					{
						log.Debug( "  looking up POBox for: " + subMsg.ToID );
						poBox = POBox.POBox.GetPOBox( store, subMsg.DomainID, subMsg.ToID );

						cSub = 
							new Subscription( 
							cDomain,
							subMsg.SharedCollectionName + " subscription", 
							"Subscription", 
							subMsg.FromID );

						cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Received;
						cSub.ToName = toMember.Name;
						cSub.ToIdentity = subMsg.ToID;
						cSub.FromName = subMsg.FromName;
						cSub.FromIdentity = subMsg.FromID;
						cSub.SubscriptionRights = (Simias.Storage.Access.Rights) subMsg.AccessRights;
						cSub.MessageID = subMsg.SubscriptionID;

						string appPath = this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} );
						appPath += "/POBoxService.asmx";

						log.Debug("  application path: " + appPath);

						cSub.SubscriptionCollectionID = subMsg.SharedCollectionID;
						cSub.SubscriptionCollectionType = subMsg.SharedCollectionType;
						cSub.SubscriptionCollectionName = subMsg.SharedCollectionName;
						cSub.DomainID = cDomain.ID;
						cSub.DomainName = cDomain.Name;
						cSub.SubscriptionKey = Guid.NewGuid().ToString();
						cSub.MessageType = "Outbound";  // ????

						if ( sharedCollection != null )
						{
							DirNode dirNode = sharedCollection.GetRootDirectory();
							if( dirNode != null )
							{
								cSub.DirNodeID = dirNode.ID;
								cSub.DirNodeName = dirNode.Name;
							}
						}
						else
						{
							cSub.DirNodeID = subMsg.DirNodeID;
							cSub.DirNodeName = subMsg.DirNodeName;
						}

						poBox.Commit( cSub );
						status = POBoxStatus.Success;
					}
					catch(Exception e)
					{
						log.Error("  failed creating subscription");
						log.Error(e.Message);
						log.Error(e.StackTrace);
					}
				}
#endif
			}
			else
			{
				log.Debug( "  invalid Domain ID!" );
				status = POBoxStatus.UnknownDomain;
			}

			log.Debug( "Invite - exit" );
			return status;
		}

		/// <summary>
		/// Removes the status subscription from the caller's POBox.
		/// </summary>
		/// <param name="store">Store handle.</param>
		/// <param name="subMsg">Subscription information.</param>
		private void RemoveStatusSubscription( Store store, SubscriptionMsg subMsg )
		{
			// Look up the invitee's POBox.
			POBox.POBox poBox = POBox.POBox.GetPOBox( store, subMsg.DomainID, subMsg.FromID );
			if ( poBox != null )
			{
				// Look for the status subscription that is associated with the inviter's subscription.		
				ICSList list = poBox.Search( Message.MessageIDProperty, subMsg.SubscriptionID, SearchOp.Equal);
				if ( list.Count > 0 )
				{
					// Get the subscription object
					ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
					Subscription subscription = new Subscription( poBox, e.Current as ShallowNode );
					e.Dispose();

					// Don't allow additional invitation events to occur.
					subscription.CascadeEvents = false;

					// Remove the subscription from the "fromIdentity" PO box
					poBox.Commit( poBox.Delete( subscription ) );
					log.Debug( "  removed subscription from fromPOBox." );
				}
			}
		}
	}



	/// <summary>
	/// </summary>
	[Serializable]
	public class SubscriptionInformation
	{
#if ( !REMOVE_OLD_INVITATION )
		/// <summary>
		/// </summary>
		public string   Name;
		/// <summary>
		/// </summary>
		public string	MsgID;
		/// <summary>
		/// </summary>
		public string	FromID;
		/// <summary>
		/// </summary>
		public string	FromName;
		/// <summary>
		/// </summary>
		public string	ToID;
		/// <summary>
		/// </summary>
		public string	ToNodeID;
		/// <summary>
		/// </summary>
		public string	ToName;
		/// <summary>
		/// </summary>
		public int		AccessRights;

		/// <summary>
		/// </summary>
		public string	CollectionID;
		/// <summary>
		/// </summary>
		public string	CollectionName;
		/// <summary>
		/// </summary>
		public string	CollectionType;

		/// <summary>
		/// </summary>
		public string	DirNodeID;
		/// <summary>
		/// </summary>
		public string	DirNodeName;

		/// <summary>
		/// </summary>
		public string	DomainID;
		/// <summary>
		/// </summary>
		public string	DomainName;

		/// <summary>
		/// </summary>
		public int		State;
		/// <summary>
		/// </summary>
		public int		Disposition;

		public POBoxStatus Status;

		/// <summary>
		/// </summary>
		public SubscriptionInformation()
		{

		}

		internal void GenerateFromSubscription(Subscription cSub)
		{
			this.Name = cSub.Name;
			this.MsgID = cSub.MessageID;
			this.FromID = cSub.FromIdentity;
			this.FromName = cSub.FromName;
			this.ToID = cSub.ToIdentity;
			this.ToNodeID = cSub.ToMemberNodeID;
			this.ToName = cSub.ToName;
			this.AccessRights = (int) cSub.SubscriptionRights;

			this.CollectionID = cSub.SubscriptionCollectionID;
			this.CollectionName = cSub.SubscriptionCollectionName;
			this.CollectionType = cSub.SubscriptionCollectionType;

			this.DirNodeID = cSub.DirNodeID;
			this.DirNodeName = cSub.DirNodeName;

			this.DomainID = cSub.DomainID;
			this.DomainName = cSub.DomainName;

			this.State = (int) cSub.SubscriptionState;
			this.Disposition = (int) cSub.SubscriptionDisposition;
		}
#endif
	}
}
