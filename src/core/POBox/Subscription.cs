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
using System.IO;
using System.Security.Cryptography;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Sync;

namespace Simias.POBox
{
	/// <summary>
	/// Subscription states.
	/// </summary>
	public enum SubscriptionStates
	{
#if ( !REMOVE_OLD_INVITATION )

		/// <summary>
		/// The Subscription has been created but not sent.
		/// </summary>
		Invited,

		/// <summary>
		/// The Subscription has been sent.
		/// </summary>
		Posted,

		/// <summary>
		/// The Subscription has been received.
		/// </summary>
		Received,

		/// <summary>
		/// The Subscription has been replied to.
		/// </summary>
		Replied,

		/// <summary>
		/// The Subscription reply has been delivered.
		/// </summary>
		Delivered,

		/// <summary>
		/// The Subscription is waiting to be accepted/declined by the owner.
		/// </summary>
		Pending,

		/// <summary>
		/// The Subscription has been accepted/declined.
		/// </summary>
		Responded,

		/// <summary>
		/// The Subscription acceptance/denial has been acknowledged.
		/// </summary>
		Acknowledged,
#endif
		/// <summary>
		/// The Subscription is ready and can be used to start syncing.
		/// </summary>
		Ready = 8,

		/// <summary>
		/// The subscription state is unknown.
		/// </summary>
		Unknown
	};

	/// <summary>
	/// The disposition of a subscription
	/// </summary>
	public enum SubscriptionDispositions
	{
		/// <summary>
		/// The subscription was accepted.
		/// </summary>
		Accepted,

		/// <summary>
		/// The subscription was declined.
		/// </summary>
		Declined,

		/// <summary>
		/// The subscription was rejected.
		/// </summary>
		Rejected,

		/// <summary>
		/// The disposition is unknown.
		/// </summary>
		Unknown
	};

	/// <summary>
	/// An Subscription object is a specialized message used for inviting someone to a team space.
	/// </summary>
	[Serializable]
	public class Subscription : Message
	{
		#region Class Members
		
		/// <summary>
		/// default root path for collections
		/// </summary>
		public static string DefaultRootPath = Path.Combine(
			Environment.GetFolderPath(
			Environment.SpecialFolder.Personal),
			"My Collections");

		/// <summary>
		/// The name of the property storing the SubscriptionState.
		/// </summary>
		public const string SubscriptionStateProperty = "SbState";

		/// <summary>
		/// The name of the property storing the recipient's public key.
		/// </summary>
		public const string ToPublicKeyProperty = "ToPKey";

		/// <summary>
		/// The name of the property storing the sender's public key.
		/// </summary>
		public const string FromPublicKeyProperty = "FromPKey";

		/// <summary>
		/// The name of the property storing the collection name.
		/// </summary>
		public const string SubscriptionCollectionNameProperty = "SbColName";

		/// <summary>
		/// The name of the property storing the collection ID.
		/// </summary>
		public static readonly string SubscriptionCollectionIDProperty = "SbColID";

		/// <summary>
		/// The name of the property storing the shared collection type.
		/// </summary>
		public const string SubscriptionCollectionTypeProperty = "SbColType";

		/// <summary>
		/// The name of the property storing the value that tells if the collection has a DirNode.
		/// </summary>
		public static readonly string SubscriptionCollectionHasDirNodeProperty = "HasDirNode";

		/// <summary>
		/// The name of the property storing the collection description.
		/// </summary>
		public const string CollectionDescriptionProperty = "ColDesc";

		/// <summary>
		/// The name of the property storing the root path of the collection (on the slave).
		/// </summary>
		public static readonly string CollectionRootProperty = "ColRoot";

		/// <summary>
		/// The name of the property storing the DirNode ID.
		/// </summary>
		public const string DirNodeIDProperty = "DirNodeID";

		/// <summary>
		/// The name of the property storing the DirNode name.
		/// </summary>
		public const string DirNodeNameProperty = "DirNodeName";

		/// <summary>
		/// The name of the property storing the rights requested/granted.
		/// </summary>
		public const string SubscriptionRightsProperty = "SbRights";

		/// <summary>
		/// The name of the property storing the status of the subscription (accepted, declined, etc.).
		/// </summary>
		public static readonly string SubscriptionDispositionProperty = "SbDisposition";
		
		/// <summary>
		/// The name of the property storing the subscription key.
		/// </summary>
		public static readonly string SubscriptionKeyProperty = "SbKey";

		/// <summary>
		/// The To member's node ID in the collection.
		/// </summary>
		public static readonly string ToMemberNodeIDProperty = "SbMemberNode";
		
		/// <summary>
		/// The type of collection that this subscription represents.
		/// </summary>
		public static readonly string SubscriptionTypes = "SbTypes";
		
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new Subscription object.
		/// </summary>
		/// <param name="domain">The domain that this subscription belongs to.</param>
		/// <param name="messageName">The friendly name of the message.</param>
		/// <param name="messageType">The type of the message.</param>
		/// <param name="fromIdentity">The identity of the sender.</param>
		public Subscription(Domain domain, string messageName, string messageType, string fromIdentity) :
			base (messageName, NodeTypes.SubscriptionType, messageType, fromIdentity, null, null, null)
		{
#if ( !REMOVE_OLD_INVITATION )
			if ( domain.SupportsNewInvitation )
			{
#endif
				SubscriptionState = SubscriptionStates.Ready;
#if ( !REMOVE_OLD_INVITATION )
			}
			else
			{
				SubscriptionState = SubscriptionStates.Invited;
			}
#endif
		}

		/// <summary>
		/// Constructor for creating an existing Subscription object from a Node object.
		/// </summary>
		/// <param name="node">The Node object to create the Subscription object from.</param>
		public Subscription(Node node) :
			base (node)
		{
		}

		/// <summary>
		/// Constructor for creating an existing Subscription object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Subscription object from.</param>
		public Subscription(Collection collection, ShallowNode shallowNode) :
			base (collection, shallowNode)
		{
		}

		/// <summary>
		/// Constructor for creating an existing Subscription object from an Xml document.
		/// </summary>
		/// <param name="document">Xml document object to create Subscription object from.</param>
		internal Subscription(XmlDocument document) :
			base (document)
		{
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// Gets/sets the state of the Subscription object.
		/// </summary>
		public SubscriptionStates SubscriptionState
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionStateProperty);

				return (p != null) ? (SubscriptionStates)p.Value : SubscriptionStates.Unknown;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionStateProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the recipient's public key
		/// </summary>
		public RSACryptoServiceProvider ToPublicKey
		{
			get
			{
				RSACryptoServiceProvider pk = null;

				Property p = properties.GetSingleProperty( ToPublicKeyProperty );
				if ( p != null )
				{
					pk = Identity.DummyCsp;
					pk.FromXmlString( p.ToString() );
				}

				return pk;
			}
			set
			{
				if ( value != null )
				{
					properties.ModifyNodeProperty( ToPublicKeyProperty, value.ToXmlString( false ) );
				}
			}
		}

		/// <summary>
		/// Gets/sets the sender's public key.
		/// </summary>
		public RSACryptoServiceProvider FromPublicKey
		{
			get
			{
				RSACryptoServiceProvider pk = null;

				Property p = properties.GetSingleProperty( FromPublicKeyProperty );
				if ( p != null )
				{
					pk = Identity.DummyCsp;
					pk.FromXmlString( p.ToString() );
				}

				return pk;
			}
			set
			{
				if ( value != null )
				{
					properties.ModifyNodeProperty( FromPublicKeyProperty, value.ToXmlString( false ) );
				}
			}
		}

		/// <summary>
		/// Gets/sets the name of the collection to share.
		/// </summary>
		public string SubscriptionCollectionName
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionCollectionNameProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionCollectionNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the ID of the collection to share.
		/// </summary>
		public string SubscriptionCollectionID
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionCollectionIDProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionCollectionIDProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the type of the collection to share.
		/// </summary>
		public string SubscriptionCollectionType
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionCollectionTypeProperty);

				return (p != null) ? p.ToString() : null;
			}
			
			set
			{
				Properties.AddProperty(SubscriptionCollectionTypeProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the description of the collection to share.
		/// </summary>
		public string CollectionDescription
		{
			get
			{
				Property p = properties.FindSingleValue(CollectionDescriptionProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(CollectionDescriptionProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the collection root path on the slave.
		/// </summary>
		public string CollectionRoot
		{
			get
			{
				Property p = properties.FindSingleValue(CollectionRootProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				Property property = new Property(CollectionRootProperty, value);
				property.LocalProperty = true;
				properties.ModifyNodeProperty(property);
			}
		}

		/// <summary>
		/// Gets/sets the ID of the collection's root DirNode.
		/// </summary>
		public string DirNodeID
		{
			get
			{
				Property p = properties.FindSingleValue(DirNodeIDProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(DirNodeIDProperty, value);
			}
		}

		/// <summary>
		/// Gets a value indicating if the collection contains a DirNode.
		/// </summary>
		public bool HasDirNode
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionCollectionHasDirNodeProperty);

				return (p != null) ? (bool)p.Value : false;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionCollectionHasDirNodeProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the name of the collection's root DirNode.
		/// </summary>
		public string DirNodeName
		{
			get
			{
				Property p = properties.FindSingleValue(DirNodeNameProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(DirNodeNameProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the rights that will be granted on the shared collection.
		/// </summary>
		public Access.Rights SubscriptionRights
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionRightsProperty);

				return (p != null) ? (Access.Rights)p.Value : Access.Rights.Deny;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionRightsProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the disposition of the subscription.
		/// </summary>
		public SubscriptionDispositions SubscriptionDisposition
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionDispositionProperty);

				return (p != null) ? (SubscriptionDispositions)p.Value : SubscriptionDispositions.Unknown;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionDispositionProperty, value);
			}
		}


		/// <summary>
		/// Gets/sets the subscription key.
		/// </summary>
		public string SubscriptionKey
		{
			get
			{
				Property p = properties.FindSingleValue(SubscriptionKeyProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(SubscriptionKeyProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the To user's NODE ID.
		/// </summary>
		public string ToMemberNodeID
		{
			get
			{
				Property p = properties.FindSingleValue(ToMemberNodeIDProperty);

				return (p != null) ? p.ToString() : null;
			}
			set
			{
				properties.ModifyNodeProperty(ToMemberNodeIDProperty, value);
			}
		}

		/// <summary>
		/// Gets/sets the originator of this subscription.
		/// </summary>
		public string Originator
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.Originator);
				return (p != null) ? p.ToString() : null;
			}

			set
			{
				Property p = new Property( PropertyTags.Originator, value );
				p.LocalProperty = true;
				properties.ModifyNodeProperty( p );
			}
		}

		/// <summary>
		/// Gets or sets the collection types for this subscription.
		/// </summary>
		public string[] SbTypes
		{
			get 
			{ 
				ArrayList sbtypes = new ArrayList();
				MultiValuedList mvl = properties.FindValues( SubscriptionTypes ); 
				foreach ( Property p in mvl )
				{
					sbtypes.Add( p.ValueString );
				}

				return sbtypes.ToArray( typeof( string ) ) as string[];
			}

			set
			{
				// Clear off all of the current types.
				properties.DeleteNodeProperties( SubscriptionTypes );
				foreach( string s in value )
				{
					properties.AddNodeProperty( SubscriptionTypes, s );
				}
			}
		}

		/// <summary>
		/// Get or Set the HostID for the collection represented by this subscription.
		/// </summary>
		public string HostID
		{
			get
			{
				string hostID = null;
				Property p = properties.FindSingleValue( PropertyTags.HostID );
				if (p != null)
				{
					hostID = p.ToString();
				}
				return hostID;
			}
			set
			{
				Property p = new Property( PropertyTags.HostID, value );
				properties.ModifyNodeProperty( p );
			}
		}

		#endregion

		#region Public Methods

#if ( !REMOVE_OLD_INVITATION )
		/// <summary>
		/// Generates a SubscriptionInfo object from the Subscription object
		/// </summary>
		/// <returns>A SubscriptionInfo object</returns>
		public SubscriptionInfo GenerateInfo(Store store)
		{
			SubscriptionInfo si = new SubscriptionInfo();

			si.DomainID = DomainID;
			si.DomainName = DomainName;
			
			si.SubscriptionCollectionID = SubscriptionCollectionID;
			si.SubscriptionCollectionName = SubscriptionCollectionName;
			si.SubscriptionCollectionType = SubscriptionCollectionType;
			si.SubscriptionID = MessageID;

			// dir node ?
			Collection c = store.GetCollectionByID(SubscriptionCollectionID);

			si.SubscriptionCollectionHasDirNode = 
				(c != null) ? (c.GetRootDirectory() != null) : false;
			
			return si;
		}

		/// <summary>
		/// Generates a SubscriptionInfo object from the Subscription object
		/// </summary>
		/// <returns>A SubscriptionInfo object</returns>
		public SubscriptionMsg GenerateSubscriptionMessage()
		{

			SubscriptionMsg subMsg = new SubscriptionMsg();
			subMsg.DomainID = this.DomainID;
			subMsg.FromID = this.FromIdentity;
			subMsg.FromName = this.FromName;
			subMsg.ToID = this.ToIdentity;
			subMsg.SharedCollectionID = this.SubscriptionCollectionID;
			subMsg.SharedCollectionType = this.SubscriptionCollectionType;
			subMsg.SharedCollectionName = this.SubscriptionCollectionName;
			subMsg.AccessRights = (int) this.SubscriptionRights;
			subMsg.SubscriptionID = this.MessageID;

			Collection collection = Store.GetStore().GetCollectionByID( subMsg.SharedCollectionID );
			if ( collection != null )
			{
				DirNode dirNode = collection.GetRootDirectory();
				if( dirNode != null )
				{
					subMsg.DirNodeID = dirNode.ID;
					subMsg.DirNodeName = dirNode.Name;
				}
			}

			return subMsg;
		}
		
		/// <summary>
		/// Generates a SubscriptionStatus object from the Subscription object
		/// </summary>
		/// <returns>A SubscriptionStatus object</returns>
		public SubscriptionStatus GenerateStatus()
		{
			SubscriptionStatus status = new SubscriptionStatus();

			status.State = this.SubscriptionState;
			status.Disposition = this.SubscriptionDisposition;

			return status;
		}

		/// <summary>
		/// Add the details to the subscription
		/// </summary>
		/// <param name="details">The details object</param>
		public void AddDetails(SubscriptionDetails details)
		{
			if (details != null)
			{
				if ((details.DirNodeID != null) && (details.DirNodeID.Length > 0))
				{
					this.DirNodeID = details.DirNodeID;
				}

				if ((details.DirNodeName != null) && (details.DirNodeName.Length > 0))
				{
					this.DirNodeName = details.DirNodeName;
				}
			}
		}

		/// <summary>
		/// Accept the subscription on the slave side.
		/// </summary>
		/// <param name="store">The store that the POBox belongs to.</param>
		/// <param name="disposition">The disposition to set on the subscription.</param>
		public void Accept(Store store, SubscriptionDispositions disposition)
		{
			Collection c = store.GetCollectionByID(this.Properties.GetSingleProperty(BaseSchema.CollectionId).ToString());

			SubscriptionState = SubscriptionStates.Replied;
			SubscriptionDisposition = disposition;
			Member member = c.GetCurrentMember();
			ToName = member.Name;
			ToIdentity = member.UserID;
			ToPublicKey = member.PublicKey;
		}

		/// <summary>
		/// Accept the subscription on the master side
		/// </summary>
		public Member Accept(Store store, Access.Rights rights)
		{
			Collection c = store.GetCollectionByID(this.SubscriptionCollectionID);

			// check collection
			if (c == null)
				throw new DoesNotExistException("Collection does not exist.");

			// member
			Member member = new Member(this.ToName, this.ToIdentity, rights, this.ToPublicKey);

			// commit
			c.Commit(member);

			// state update
			this.SubscriptionState = SubscriptionStates.Responded;
			this.SubscriptionDisposition = SubscriptionDispositions.Accepted;
			this.ToMemberNodeID = member.ID;

			return member;
		}

		/// <summary>
		/// Decline the subscription on the master side
		/// </summary>
		public void Decline()
		{
			// state update
			this.SubscriptionState = SubscriptionStates.Responded;
			this.SubscriptionDisposition = SubscriptionDispositions.Declined;
		}
#endif

		/// <summary>
		/// Create the slave collection (stub for syncing)
		/// </summary>
		public void CreateSlave(Store store)
		{
			ArrayList commitList = new ArrayList();

			Collection c = new Collection(store, this.SubscriptionCollectionName,
				this.SubscriptionCollectionID, this.DomainID);

			c.HostID = this.HostID;
			
			commitList.Add(c);

			// Check if this is an old type of subscription and add the collection type
			// specified. If it is a new type of subscription, then add all of the
			// specified types.
			if ( c.Properties.HasProperty( SubscriptionTypes ) )
			{
				// Remove all types and add the ones specified by the subscription.
				c.Properties.DeleteNodeProperties( PropertyTags.Types );
				foreach( string s in SbTypes )
				{
					c.SetType( c, s );
				}
			}
			else
			{
				// For backwards compatibility with older clients.
				c.SetType( c, SubscriptionCollectionType );
			}
			
			// Create the member as well
			if (this.ToMemberNodeID != null && this.ToMemberNodeID != "")
			{
				Member member = new Member(this.ToName, this.ToMemberNodeID, this.ToIdentity, this.SubscriptionRights, null);
				member.IsOwner = true;
				member.Proxy = true;
				commitList.Add(member);
			}

			// check for a dir node
			if (((this.DirNodeID != null) && (this.DirNodeID.Length > 0))
				&& (this.DirNodeName != null) && (this.DirNodeName.Length > 0)
				&& (this.CollectionRoot != null) && (this.CollectionRoot.Length > 0))
			{
				string path = Path.Combine(this.CollectionRoot, this.DirNodeName);
				DirNode dn = new DirNode(c, path, this.DirNodeID);
				if (!Directory.Exists(path)) Directory.CreateDirectory(path);

				dn.Proxy = true;
				commitList.Add(dn);
			}

			c.Proxy = true;
			c.Commit((Node[]) commitList.ToArray(typeof(Node)));
		}

		/// <summary>
		/// Sets all of the Types tags specified on the collection as SubscriptionTypes.
		/// </summary>
		/// <param name="collection">Collection to get types from.</param>
		public void SetSubscriptionTypes( Collection collection )
		{
			// Clear off all of the current types.
			properties.DeleteNodeProperties( SubscriptionTypes );

			// Set each node type as subscription types.
			MultiValuedList mvl = collection.Properties.FindValues( PropertyTags.Types );
			foreach( Property p in mvl )
			{
				properties.AddNodeProperty( SubscriptionTypes, p.ValueString );
			}
		}

		#endregion
	}
}
