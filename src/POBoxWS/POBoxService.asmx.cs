/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright � Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
//using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Web;
using System.Xml;
using System.Xml.Serialization;

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
		/// The specified identity was not found in the roster.
		/// </summary>
		UnknownIdentity,

		/// <summary>
		/// The specified subscription was not found.
		/// </summary>
		UnknownSubscription,

		/// <summary>
		/// The specified collection was not found.
		/// </summary>
		UnknownCollection,

		/// <summary>
		/// The suscription was in an invalid state for the method
		/// </summary>
		InvalidState,

		/// <summary>
		/// An unknown error was realized.
		/// </summary>
		UnknownError
	};

	/// <summary>
	/// Summary description for Service1.
	/// </summary>
	/// 
	
	[WebService(Namespace="http://novell.com/simias/pobox/")]
	public class POBoxService : System.Web.Services.WebService
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(POBoxService));

		public POBoxService()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

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
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="subscriptionID"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		AcceptedSubscription(
			string				domainID, 
			string				fromIdentity, 
			string				toIdentity, 
			string				subscriptionID)
		{
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();

			log.Info("POBoxService::AcceptedSubscription - called");
			log.Info("  subscription: " + subscriptionID);
			
			// open the post office box
			poBox = (domainID == Simias.Storage.Domain.WorkGroupDomainID) 
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, fromIdentity);

			// check the post office box
			if (poBox == null)
			{
				log.Debug("POBoxService::AcceptedSubscription - PO Box not found");
				return(POBoxStatus.UnknownPOBox);
			}

			// check that the message has already not been posted
			IEnumerator e = 
				poBox.Search(
					Message.MessageIDProperty, 
					subscriptionID, 
					SearchOp.Equal).GetEnumerator();
			ShallowNode sn = null;
			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
			{
				log.Debug("POBoxService::AcceptedSubscription - Subscription does not exist");
				return(POBoxStatus.UnknownSubscription);
			}

			// get the subscription object
			Subscription cSub = new Subscription(poBox, sn);

			// Identities need to match up
			if (fromIdentity != cSub.FromIdentity)
			{
				log.Debug("POBoxService::AcceptedSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			if (toIdentity != cSub.ToIdentity)
			{
				log.Debug("POBoxService::AcceptedSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			// FIXME: need to match the caller's ID against the toIdentity

			cSub.Accept(store, cSub.SubscriptionRights);
			poBox.Commit(cSub);
			log.Info("POBoxService::AcceptedSubscription - exit");
			return(POBoxStatus.Success);
		}

		/// <summary>
		/// Decline subscription
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="subscriptionID"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		DeclinedSubscription(
			string			domainID, 
			string			fromIdentity, 
			string			toIdentity, 
			string			subscriptionID)
		{
			Simias.POBox.POBox	toPOBox;
			Store				store = Store.GetStore();
			
			log.Info("POBoxService::DeclinedSubscription - called");
			log.Info("  subscription: " + subscriptionID);

			// open the post office box of the From user
			toPOBox = (domainID == Simias.Storage.Domain.WorkGroupDomainID) 
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, toIdentity);

			// check the post office box
			if (toPOBox == null)
			{
				log.Debug("POBoxService::DeclinedSubscription - PO Box not found");
				return(POBoxStatus.UnknownPOBox);
			}

			// Get the subscription from the caller's PO box
			IEnumerator e = 
				toPOBox.Search(
				Message.MessageIDProperty, 
				subscriptionID, 
				SearchOp.Equal).GetEnumerator();
			ShallowNode sn = null;
			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
			{
				log.Debug(
					"POBoxService::DeclinedSubscription - Subscription: " +
					subscriptionID +
					" does not exist");
				return(POBoxStatus.UnknownSubscription);
			}

			// get the subscription object
			Subscription cSub = new Subscription(toPOBox, sn);

			// Identities need to match up
			if (fromIdentity != cSub.FromIdentity)
			{
				log.Debug("POBoxService::DeclinedSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			if (toIdentity != cSub.ToIdentity)
			{
				log.Debug("POBoxService::DeclinedSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			// FIXME: Verify the caller of the web service is the toIdentity

			// Validate the shared collection
			Collection cCol = store.GetCollectionByID(cSub.SubscriptionCollectionID);
			if (cCol == null)
			{
				// FIXEME:: Do we want to still try and cleanup the subscriptions?
				log.Debug("POBoxService::DeclinedSubscription - Collection not found");
				return(POBoxStatus.UnknownCollection);
			}

			//
			// Actions taken when a subscription is declined
			//
			// If I'm the owner of the shared collection then the
			// decline is treated as a delete of the shared collection
			// so we must.
			// 1) Delete all the subscriptions in all members PO boxes
			// 2) Delete the shared collection itself
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

			Simias.Storage.Member toMember = cCol.GetMemberByID(toIdentity);
			if (toMember == null)
			{
				log.Info("  handling case where identity is declining a subscription");
				// I am not a member of this shared collection and want to
				// decline the subscription.

				// open the post office box of the from and decline the subscription
				Simias.POBox.POBox fromPOBox = 
					(domainID == Simias.Storage.Domain.WorkGroupDomainID) 
					? Simias.POBox.POBox.GetPOBox(store, domainID)
					: Simias.POBox.POBox.FindPOBox(store, domainID, fromIdentity);
				if (fromPOBox != null)
				{
					Subscription cFromMemberSub = 
						fromPOBox.GetSubscriptionByCollectionID(cCol.ID);
					if(cFromMemberSub != null)
					{
						cFromMemberSub.Decline();
						fromPOBox.Commit(cFromMemberSub);
					}
				}

				// Remove the subscription from the "toIdentity" PO box
				toPOBox.Delete(cSub);
				toPOBox.Commit(cSub);

			}
			else
			if (toMember.IsOwner == true)
			{
				// Am I the owner of the shared collection?
				log.Info("  handling case where identity is owner of collection");

				ICSList memberlist = cCol.GetMemberList();
				foreach(ShallowNode sNode in memberlist)
				{
					Simias.Storage.Member cMember =	
						new Simias.Storage.Member(cCol, sNode);

					// Get the member's POBox
					Simias.POBox.POBox memberPOBox = 
						Simias.POBox.POBox.FindPOBox(
						store, 
						cCol.Domain, 
						cMember.UserID );
					if (memberPOBox != null)
					{
						// Search for the matching subscription
						Subscription memberSub = 
							memberPOBox.GetSubscriptionByCollectionID(cCol.ID);
						if(memberSub != null)
						{
							memberPOBox.Delete(memberSub);
							memberPOBox.Commit(memberSub);
						}
					}
				}

				// Delete the shared collection itself
				cCol.Commit(cCol.Delete());
			}
			else
			{
				// Am I a member of the shared collection?
				log.Info("  handling case where identity is a member of the collection");

				cCol.Delete(toMember);
				cCol.Commit(toMember);

				// Remove the subscription from the "toIdentity" PO box
				Subscription cMemberSub = 
					toPOBox.GetSubscriptionByCollectionID(cCol.ID);
				if(cMemberSub != null)
				{
					toPOBox.Delete(cMemberSub);
					toPOBox.Commit(cMemberSub);
				}

				if (fromIdentity != toIdentity)
				{
					// open the post office box of the From user
					Simias.POBox.POBox fromPOBox = 
						(domainID == Simias.Storage.Domain.WorkGroupDomainID) 
						? Simias.POBox.POBox.GetPOBox(store, domainID)
						: Simias.POBox.POBox.FindPOBox(store, domainID, toIdentity);
					if (fromPOBox != null)
					{
						// Remove the subscription from the "fromIdentity" PO box
						Subscription cFromMemberSub = 
							fromPOBox.GetSubscriptionByCollectionID(cCol.ID);
						if(cFromMemberSub != null)
						{
							fromPOBox.Delete(cFromMemberSub);
							fromPOBox.Commit(cFromMemberSub);
						}
					}
				}
			}

			log.Info("POBoxService::DeclinedSubscription - exit");
			return(POBoxStatus.Success);
		}

		/// <summary>
		/// Acknowledge the subscription.
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		AckSubscription(
			string			domainID, 
			string			fromIdentity, 
			string			toIdentity, 
			string			messageID)
		{
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();
			
			log.Info("POBoxService::Acksubscription - called");
			log.Info("  subscription: " + messageID);

			// open the post office box
			poBox = (domainID == Simias.Storage.Domain.WorkGroupDomainID) 
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, fromIdentity);

			// check the post office box
			if (poBox == null)
			{
				log.Debug("POBoxService::AckSubscription - PO Box not found");
				return(POBoxStatus.UnknownPOBox);
			}

			// check that the message has already not been posted
			IEnumerator e = 
				poBox.Search(
					Message.MessageIDProperty, 
					messageID, 
					SearchOp.Equal).GetEnumerator();
			ShallowNode sn = null;
			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
			{
				log.Debug("POBoxService::AckSubscription - Subscription does not exist.");
				return(POBoxStatus.UnknownSubscription);
			}

			// get the subscription object
			Subscription cSub = new Subscription(poBox, sn);

			// Identities need to match up
			if (fromIdentity != cSub.FromIdentity)
			{
				log.Debug("POBoxService::AckSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			if (toIdentity != cSub.ToIdentity)
			{
				log.Debug("POBoxService::AckSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			// FIXME: need to match the caller's ID against the toIdentity

			cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Acknowledged;
			poBox.Commit(cSub);
			poBox.Commit(poBox.Delete(cSub));

			log.Info("POBoxService::Acksubscription - exit");
			return(POBoxStatus.Success);
		}

		/// <summary>
		/// Get the subscription information
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		/// <returns>success:subinfo  failure:null</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		SubscriptionInformation 
		GetSubscriptionInfo(string domainID, string identityID, string messageID)
		{
			Simias.POBox.POBox	poBox;
			Store store = Store.GetStore();

			log.Info("POBoxService::GetSubscriptionInfo - called");
			log.Info("  for subscription: " + messageID);

			// open the post office box
			poBox =
				(domainID == Simias.Storage.Domain.WorkGroupDomainID)
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, identityID);
			
			// check the post office box
			if (poBox == null)
			{
				log.Debug("POBoxService::GetSubscriptionInfo - PO Box not found");
				return(null);
			}

			// check that the message has already not been posted
			IEnumerator e = 
				poBox.Search(Message.MessageIDProperty, messageID, SearchOp.Equal).GetEnumerator();
			
			ShallowNode sn = null;

			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
			{
				log.Debug("POBoxService::GetSubscriptionInfo - Subscription does not exist");
				return(null);
			}

			// generate the subscription info object and return it
			Subscription cSub = new Subscription(poBox, sn);

			// Validate the shared collection
			Collection cSharedCollection = store.GetCollectionByID(cSub.SubscriptionCollectionID);
			if (cSharedCollection == null)
			{
				log.Debug("POBoxService::GetSubscriptionInfo - Collection not found");
				return(null);
			}

			UriBuilder colUri = 
				new UriBuilder(
					this.Context.Request.Url.Scheme,
					this.Context.Request.Url.Host,
					this.Context.Request.Url.Port,
					this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} ));

			log.Info("URI: " + colUri.ToString());
			SubscriptionInformation subInfo = new SubscriptionInformation(colUri.ToString());
			subInfo.GenerateFromSubscription(cSub);

			log.Info("POBoxService::GetSubscriptionInfo - exit");
			return subInfo;
		}

		/// <summary>
		/// Verify that a collection exists
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		/// <returns>success:subinfo  failure:null</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		VerifyCollection(string domainID, string collectionID)
		{
			Store store = Store.GetStore();

			log.Info("POBoxService::VerifyCollection - called");
			log.Info("  for collection: " + collectionID);

			POBoxStatus	wsStatus = POBoxStatus.UnknownCollection;

			// Validate the shared collection
			Collection cSharedCollection = store.GetCollectionByID(collectionID);
			if (cSharedCollection != null)
			{
				// Make sure the collection is not in a proxy state
				if (cSharedCollection.IsProxy == false)
				{
					wsStatus = POBoxStatus.Success;
				}
				else
				{
					log.Info("POBoxService::VerifyCollection - Collection is in the proxy state");
				}
			}
			else
			{
				log.Info("POBoxService::VerifyCollection - Collection not found");
			}

			log.Info("POBoxService::VerifyCollection - exit");
			return(wsStatus);
		}

		/// <summary>
		/// Invite a user to a shared collection
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="fromUserID"></param>
		/// <param name="toUserID"></param>
		/// <param name="sharedCollectionID"></param>
		/// <param name="sharedCollectionType"></param>
		/// <returns>success subscription ID - failure empty string</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		string 
		Invite(
			string			domainID, 
			string			fromUserID,
			string			toUserID,
			string			sharedCollectionID,
			string			sharedCollectionType,
			int				rights)
		{
			Collection			sharedCollection;
			Simias.POBox.POBox	poBox = null;
			Store				store = Store.GetStore();
			Subscription		cSub = null;

			log.Debug("POBoxService::Invite");

			if (domainID == null || domainID == "")
			{
				domainID = store.DefaultDomain;
			}

			// Verify domain
			Simias.Storage.Domain cDomain = store.GetDomain(domainID);
			if (cDomain == null)
			{
				throw new ApplicationException("Invalid Domain ID");
			}

			// Verify and get additional information about the "To" user
			Simias.Storage.Roster currentRoster = cDomain.GetRoster(store);
			if (currentRoster == null)
			{
				throw new ApplicationException("No member Roster exists for the specified Domain");
			}

			Member toMember = currentRoster.GetMemberByID(toUserID);
			if (toMember == null)
			{
				throw new ApplicationException("Specified \"toUserID\" does not exist in the Domain Roster");
			}

			Member fromMember = currentRoster.GetMemberByID(fromUserID);
			if (fromMember == null)
			{
				throw new ApplicationException("Specified \"fromUserID\" does not exist in the Domain Roster");
			}

			// FIXME:  Verify the fromMember is the caller

			sharedCollection = store.GetCollectionByID(sharedCollectionID); 
			if (sharedCollection == null)
			{
				throw new ApplicationException("Invalid shared collection ID");
			}

			if (rights > (int) Simias.Storage.Access.Rights.Admin)
			{
				throw new ApplicationException("Invalid access rights");
			}

			try
			{
				log.Debug("  looking up POBox for: " + toUserID);
			
				poBox = 
					(domainID == Simias.Storage.Domain.WorkGroupDomainID)
						? POBox.POBox.GetPOBox(store, domainID)
						: POBox.POBox.GetPOBox(store, domainID, toUserID);

				log.Debug("  newup subscription");
				cSub = new Subscription(sharedCollection.Name + " subscription", "Subscription", fromUserID);
				cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Received;
				cSub.ToName = toMember.Name;
				cSub.ToIdentity = toUserID;
				cSub.FromName = fromMember.Name;
				cSub.FromIdentity = fromUserID;
				cSub.SubscriptionRights = (Simias.Storage.Access.Rights) rights;

				string appPath = this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} );
				appPath += "/POBoxService.asmx";

				log.Info(  "application path: " + appPath);

				UriBuilder poUri = 
					new UriBuilder(
						this.Context.Request.Url.Scheme,
						this.Context.Request.Url.Host,
						this.Context.Request.Url.Port,
						appPath);

				log.Info("  newup service url: " + poUri.ToString());

				cSub.POServiceURL = new Uri(poUri.ToString());
				cSub.SubscriptionCollectionID = sharedCollection.ID;
				cSub.SubscriptionCollectionType = sharedCollectionType;
				cSub.SubscriptionCollectionName = sharedCollection.Name;
				cSub.DomainID = domainID;
				cSub.DomainName = cDomain.Name;
				cSub.SubscriptionKey = Guid.NewGuid().ToString();
				cSub.MessageType = "Outbound";  // ????

				UriBuilder coUri = 
					new UriBuilder(
						this.Context.Request.Url.Scheme,
						this.Context.Request.Url.Host,
						this.Context.Request.Url.Port,
						this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} ));

				cSub.SubscriptionCollectionURL = coUri.ToString();
				log.Info("SubscriptionCollectionURL: " + cSub.SubscriptionCollectionURL);
 
				DirNode dirNode = sharedCollection.GetRootDirectory();
				if(dirNode != null)
				{
					cSub.DirNodeID = dirNode.ID;
					cSub.DirNodeName = dirNode.Name;
				}

				poBox.Commit(cSub);
				return(cSub.MessageID);
			}
			catch(Exception e)
			{
				log.Debug("  failed creating subscription");
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
			}
			return("");
		}

		/// <summary>
		/// Return the Default Domain
		/// </summary>
		/// <param name="dummy">Dummy parameter so stub generators won't produce empty structures</param>
		/// <returns>default domain</returns>
		[WebMethod(EnableSession = true)]
		public string GetDefaultDomain(int dummy)
		{
			return(Store.GetStore().DefaultDomain);
		}
	}

	[Serializable]
	public class SubscriptionInformation
	{
		public string   Name;
		public string	MsgID;
		public string	FromID;
		public string	FromName;
		public string	ToID;
		public string	ToNodeID;
		public string	ToName;
		public int		AccessRights;

		public string	CollectionID;
		public string	CollectionName;
		public string	CollectionType;
		public string	CollectionUrl;

		public string	DirNodeID;
		public string	DirNodeName;

		public string	DomainID;
		public string	DomainName;

		public int		State;
		public int		Disposition;

		public SubscriptionInformation()
		{

		}

		public SubscriptionInformation(string collectionUrl)
		{
			this.CollectionUrl = collectionUrl;
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


			//this.CollectionUrl = cSub.SubscriptionCollectionURL;

			this.DirNodeID = cSub.DirNodeID;
			this.DirNodeName = cSub.DirNodeName;

			this.DomainID = cSub.DomainID;
			this.DomainName = cSub.DomainName;

			this.State = (int) cSub.SubscriptionState;
			this.Disposition = (int) cSub.SubscriptionDisposition;
		}
	}
}
