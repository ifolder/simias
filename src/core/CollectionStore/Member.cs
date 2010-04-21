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
*                 $Author: Mike Lasky <mlasky@novell.com>
*                 $Modified by: Kalidas Balakrishnan <bkalidas@novell.com> 
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        Added functionality to handle encryption keys - RSA and X509 - including
*	Recovery of key data
*
*******************************************************************************/

 
using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Xml;

using Simias.Client;
using Simias.Storage;
using Simias.CryptoKey;
using Simias.Sync;



namespace Simias.Storage
{
	public enum EncVersion
	{
		/// <summary>
		/// Encryption version
		/// </summary>
		version = 0
	}

	/// <summary>
	/// Class that represents a member that has rights to a collection.
	/// </summary>
	[ Serializable ]
	public class Member : Node
	{
		#region Class Members
		/// <summary>
		/// Cached access control entry that is used when validating access check operations.
		/// </summary>
		[ NonSerialized() ]
		private AccessControlEntry ace;
		#endregion

		/// <summary>

		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Member));
		private SimiasAccessLogger accessLog = new SimiasAccessLogger("Passphrase",null);

		[ FlagsAttribute ]
		private enum EncryptionBlobFlag
		{
			/// <summary>
			/// indicates that this member has set passphrase 
			/// </summary>
			//BlobFlag = 0x00100000
			BlobFlag = 32

		};

        	public enum userMoveStates
        	{
                	/// <summary>
                	/// The member move is yet to start
                	/// </summary>
                	Nousermove,

                	/// <summary>
                	/// The member move was initialized..
                	/// </summary>
                	Initialized,

               	 	/// <summary>
                	/// The member was disabled.
                	/// </summary>
                	UserDisabled,

                	/// <summary>
                	/// The member's iFolders are disabled.
                	/// </summary>
                	iFoldersDisabled,

                	/// <summary>
                	/// The members data move is started.
                	/// </summary>
                	DataMoveStarted,

                	/// <summary>
                	/// The member move completed..
                	/// </summary>
                	Reprovision,	

                	/// <summary>
                	/// The member move completed..
                	/// </summary>
                	MoveCompleted
        	};


		/// </summary>
		/// <summary>
		/// Get/Set the encryption key
		/// </summary>
		public string EncryptionKey
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.EncryptionKey);
				string encryptionKey = (p!=null) ? (string) p.Value as string : "";
				return encryptionKey;
			}
			set
			{
		                Property p = new Property(PropertyTags.EncryptionKey, value);
                                p.LocalProperty = true;
                                properties.ModifyNodeProperty(p);
			}
		}

		/// </summary>
		/// <summary>
		/// Get/Set the encryption version
		/// </summary>
		public string EncryptionVersion
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.EncryptionVersion);
				string encryptionVersion = (p!=null) ? (string) p.Value as string : "";
				return encryptionVersion;
			}
			set
			{
		                Property p = new Property(PropertyTags.EncryptionVersion, value);
                                p.LocalProperty = true;
                                properties.ModifyNodeProperty(p);
			}
		}

		/// </summary>
		/// <summary>
		/// Get/Set the encryption key
		/// </summary>
		public long AggregateDiskQuota
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.AggregateDiskQuota);
				long value= (p!=null) ? (long) p.Value:(long)-1;
				return value;
			}
			set
			{
		                Property p = new Property(PropertyTags.AggregateDiskQuota, value);
                                p.ServerOnlyProperty = true;
                                properties.ModifyNodeProperty(p);
				Store store = Store.GetStore();	
				Domain domain = store.GetDomain(store.DefaultDomain);
				domain.Commit(this);
			}
		}

		/// <summary>
		/// Deletes properties
		/// </summary>
		public string DeleteProperty
		{
			set
			{
				Property p = properties.FindSingleValue(value);
				if (p != null )
					p.DeleteProperty();
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(GetDomainID(store));
				domain.Commit(this);
			}
		}

		/// <summary>
		/// Gets / sets the User already disabled status 
		/// </summary>
		public bool LoginAlreadyDisabled
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.LoginAlreadyDisabled);
				if (p != null && (bool)p.Value == true )
					return true;
				else
					return false;
			}
			set
			{
				properties.ModifyNodeProperty(PropertyTags.LoginAlreadyDisabled, value);
			}
		}

		/// <summary>
		/// Gets / sets the User already disabled status 
		/// </summary>
		public int UserMoveState
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.UserMoveState);
				if (p != null)
					return (int)p.Value;
				else
					return -1;
			}
			set
			{
				Property p = properties.FindSingleValue(PropertyTags.UserMoveState);
				if (p != null && value == 0)
				{
					p.DeleteProperty();
				}
				else
				{
					Property UserMoveStateProp = new Property( PropertyTags.UserMoveState, value );
					UserMoveStateProp.ServerOnlyProperty = true;
					properties.ModifyNodeProperty(UserMoveStateProp);
				}
			}
		}

		/// <summary>
		/// Gets the encryption blob
		/// </summary>
		public string EncryptionType
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.EncryptionType);
				string encryptionType = (p!=null) ? (string) p.Value as string : "";
				return encryptionType;
			}
			set
			{
				properties.AddNodeProperty(PropertyTags.EncryptionType, value);
			}
		}
		/// <summary>
		/// Gets the encryption blob
		/// </summary>
		public string EncryptionBlob
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.EncryptionBlob);
				string encryptionBlob = (p!=null) ? (string) p.Value as string : "";
				return encryptionBlob;
			}
			set
			{
		                Property p = new Property(PropertyTags.EncryptionBlob, value);
                                p.LocalProperty = true;
                                properties.ModifyNodeProperty(p);
			}
		}

		/// <summary>
		/// Gets the encryption blob status , a bool value
		/// </summary>
		public bool EncryptionBlobStatus
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.SecurityStatus);
				if (p != null)
				{
					if( (((int)p.Value & (int)EncryptionBlobFlag.BlobFlag)) == (int)EncryptionBlobFlag.BlobFlag)
					{
						// so , the flag was set when user has created an encrypted ifolder
						return true;
					}
				}
				return false;
			}
		
		}	
		
		/// <summary>
		/// Gets the encryption blob
		/// </summary>
		public string RAName
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.RAName);
				string name = (p!=null) ? (string) p.Value as string : "";
				return name;
			}
			set
			{
		                Property p = new Property(PropertyTags.RAName, value);
                                p.LocalProperty = true;
                                properties.ModifyNodeProperty(p);
			}
		}
		/// <summary>
		/// Gets the encryption blob
		/// </summary>
		public string RAPublicKey
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.RAPublicKey);
				string key = (p!=null) ? (string) p.Value as string : "";
				return key;
			}
			set
			{
		                Property p = new Property(PropertyTags.RAPublicKey, value);
                                p.LocalProperty = true;
                                properties.ModifyNodeProperty(p);
			}
		}

/*		/// <summary>
		/// Gets/Sets the LDAP Sync source
		/// </summary>
		public string SyncSource
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.SyncSource);
				string name = (p!=null) ? (string) p.Value as string : "";
				return name;
			}
			set
			{
		                Property p = new Property(PropertyTags.SyncSource, value);
                                p.ServerOnlyProperty = true;
                                properties.ModifyNodeProperty(p);
			}
		}
*/
		#region Properties
		/// <summary>
		/// Gets the access control entry stored on this object.
		/// </summary>
		private AccessControlEntry AceProperty
		{
			get
			{
				// Get the user ID from the ace.
				Property p = properties.FindSingleValue( PropertyTags.Ace );
				if ( p == null )
				{
					throw new DoesNotExistException( String.Format( "Member object {0} - ID: {1} does not contain {2} property.", name, id, PropertyTags.Ace ) );
				}

				return new AccessControlEntry( p );
			}
		}

		/// <summary>
		/// Get the cached ACE that is used to validate access.
		/// </summary>
		internal AccessControlEntry ValidateAce
		{
			get { return ace; }
		}

		/// <summary>
		/// Gets or sets whether this Member object is the collection owner.
		/// </summary>
		public bool IsOwner
		{
			get { return properties.HasProperty( PropertyTags.Owner ); }
			set 
			{ 
				if ( value )
				{
					properties.ModifyNodeProperty( PropertyTags.Owner, true ); 
				}
				else
				{
					properties.DeleteSingleNodeProperty( PropertyTags.Owner );
				}
			}
		}

		/// <summary>
		/// Gets the public key stored on this Member object. May return null if no public key is set on the object.
		/// </summary>
		public RSACryptoServiceProvider PublicKey
		{
			get
			{
				RSACryptoServiceProvider pk = null;

				Property p = properties.GetSingleProperty( PropertyTags.PublicKey );
				if ( p != null )
				{
					pk = Identity.DummyCsp;
					pk.FromXmlString( p.ToString() );
				}

				return pk;
			}
		}

		/// <summary>
		/// Gets or sets the members's access rights.
		/// </summary>
		public Access.Rights Rights
		{
			get { return AceProperty.Rights; }
			set { AceProperty.Rights = value; }
		}

		/// <summary>
		/// Gets the user identitifer for this object.
		/// </summary>
		public string UserID
		{
			get { return ace.ID; }
		}

		/// <summary>
		/// Gets and sets the member's given (first) name
		/// returns null if the property does not exist
		/// </summary>
		public string Given
		{
			get
			{
				Property p = properties.FindSingleValue( PropertyTags.Given );
				if ( p != null )
				{
					return p.ValueString;
				}

				return null;
			}

			set
			{
				if ( value != null && value != "" )
				{
					Property p = new Property( PropertyTags.Given, value );
					properties.ModifyNodeProperty( p ); 
				}
				else
				{
					properties.DeleteSingleNodeProperty( PropertyTags.Given );
				}
			}
		}

		/// <summary>
		/// Gets and sets the member's family (last) name
		/// returns null if the property does not exist
		/// </summary>
		public string Family
		{
			get
			{
				Property p = properties.FindSingleValue( PropertyTags.Family );
				if ( p != null )
				{
					return p.ValueString;
				}

				return null;
			}

			set
			{
				if ( value != null && value != "" )
				{
					Property p = new Property( PropertyTags.Family, value );
					properties.ModifyNodeProperty( p ); 
				}
				else
				{
					properties.DeleteSingleNodeProperty( PropertyTags.Family );
				}
			}
		}

		/// <summary>
		/// Gets and sets the member's full name
		/// returns null if the property does not exist
		/// </summary>
		public string FN
		{
			get
			{
				Property p = properties.FindSingleValue( PropertyTags.FullName );
				if ( p != null )
				{
					return p.ValueString;
				}

				return null;
			}

			set
			{
				if ( value != null && value != "" )
				{
					properties.ModifyNodeProperty( PropertyTags.FullName, value ); 
				}
				else
				{
					properties.DeleteSingleNodeProperty( PropertyTags.FullName );
				}
			}
		}

		/// <summary>
		/// Gets or Sets the HostID for the home server for this user.
		/// This is the ID of the server that hosts this user.
		/// </summary>
		public HostNode HomeServer
		{
			get
			{
				// Make sure we check the member object in the domain.
				HostNode host = null;
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(GetDomainID(store));
				Member m = domain.GetMemberByID(UserID);
				Property p = m.properties.FindSingleValue( PropertyTags.HostID );
				if (p != null)
				{
					host = new HostNode(domain.GetMemberByID(p.ToString()));
				}
				return host;
			}
			set
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(GetDomainID(store));
				Member m = domain.GetMemberByID(UserID);
				Property p = new Property( PropertyTags.HostID, value.UserID );
				m.properties.ModifyNodeProperty( p );
				domain.Commit(m);
			}
		}

		/// <summary>
		/// Gets or Sets the HostID for the New home server for this user.
		/// This is the ID of the server to which user needs to be moved .
		/// </summary>
		public string NewHomeServer
		{
			get
			{
				// Make sure we check the member object in the domain.
				string hostID = null;
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(GetDomainID(store));
				Member m = domain.GetMemberByID(UserID);
				Property p = m.properties.FindSingleValue( PropertyTags.NewHostID );
				if (p != null)
				{
					hostID = p.ToString();
				}
				return hostID;
			}
			set
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(GetDomainID(store));
				Member m = domain.GetMemberByID(UserID);
				Property p = new Property( PropertyTags.NewHostID, value );
				m.properties.ModifyNodeProperty( p );
				domain.Commit(m);
			}
		}

		/// <summary>
		/// Gets or Sets whether it is a group or not.
		/// </summary>
               public string GroupType
               {
                       get
                       {
                               Property p = properties.FindSingleValue(PropertyTags.GroupType);
                               string name = (p!=null) ? (string) p.Value as string : null;
                               return name;
                       }
                       set
                       {
                               Property p = new Property(PropertyTags.GroupType, value);
                                p.ServerOnlyProperty = true;
                                properties.ModifyNodeProperty(p);
                       }
               }


		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new Member object.
		/// </summary>
		/// <param name="userName">User name of the member.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		/// <param name="rights">Collection access rights granted to the user.</param>
		public Member( string userName, string userGuid, Access.Rights rights ) :
			this ( userName, userGuid, rights, null )
		{
		}
		/// <summary>
		/// Constructor for creating a new Member object.
		/// </summary>
		/// <param name="userName">User name of the member.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		/// <param name="rights">Collection access rights granted to the user.</param>
		/// <param name="publicKey">Public key that will be used to authenticate the user.</param>
		public Member( string userName, string userGuid, Access.Rights rights, RSACryptoServiceProvider publicKey ) :
			this ( userName, Guid.NewGuid().ToString(), userGuid, rights, publicKey )
		{
		}

		/// <summary>
		/// Constructor for creating a new Member object.
		/// </summary>
		/// <param name="userName">User name of the member.</param>
		/// <param name="nodeID">Identifier for the Node object.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		/// <param name="rights">Collection access rights granted to the user.</param>
		/// <param name="publicKey">Public key that will be used to authenticate the user.</param>
		public Member( string userName, string nodeID, string userGuid, Access.Rights rights, RSACryptoServiceProvider publicKey ) :
			base ( userName, nodeID, NodeTypes.MemberType )
		{
			// Create an access control entry and store it on the object.
			ace = new AccessControlEntry( userGuid, rights );
			ace.Set( this );

			// Add the public key as a property of the object.
			if ( publicKey != null )
			{
				properties.ModifyNodeProperty( PropertyTags.PublicKey, publicKey.ToXmlString( false ) );
			}

			// TODO: Since no full name was specified, use the userName as the full name for now.
			FN = userName;

			//Member gets synced from Master to Slave and from Server to client. There must not be any collisions in case
			//of Multi-server and client sync.
			CollisionPolicy = CollisionPolicy.ServerWins;
		}

		/// <summary>
		/// Constructor for creating a new Member object. Parameter validation is the responsibility of the calling function. 
		/// </summary>
		/// <param name="userName">User name of the member.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		/// <param name="rights">Collection access rights granted to the user.</param>
		/// <param name="givenName">Given (first) name of the contact</param>
		/// <param name="familyName">Family (last) name of the contact</param>
		public Member( string userName, string userGuid, Access.Rights rights, string givenName, string familyName ) :
			this( userName, userGuid, userGuid, rights, null )
		{
			this.Given = givenName;
			this.Family = familyName;

			if ( givenName != null && familyName != null )
			{
				this.FN = givenName + " " + familyName;
			}
		}


		/// <summary>
		/// Constructor that creates a Member object from a Node object.
		/// </summary>
		/// <param name="node">Node object to create the Member object from.</param>
		public Member( Node node ) :
			base( node )
		{
			if ( type != NodeTypes.MemberType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.MemberType, type ) );
			}

			ace = AceProperty;
		}

		/// <summary>
		/// Constructor that creates a Member object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the specified Node object belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Member object from.</param>
		public Member( Collection collection, ShallowNode shallowNode ) :
			base( collection, shallowNode )
		{
			if ( type != NodeTypes.MemberType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.MemberType, type ) );
			}

			ace = AceProperty;
		}

		/// <summary>
		/// Constructor that creates a Member object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create the Member object from.</param>
		internal Member( XmlDocument document ) :
			base( document )
		{
			if ( type != NodeTypes.MemberType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.MemberType, type ) );
			}

			ace = AceProperty;
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Gets the domain associated with this Member.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <returns>A string containing the Domain ID that the member belongs to. If the Member
		/// object has not been committed, a null is returned.</returns>
		internal string GetDomainID( Store store )
		{
			string domainID = null;

			Property p = properties.FindSingleValue( BaseSchema.CollectionId );
			if ( p != null )
			{
				Collection c = store.GetCollectionByID( p.Value as string );
				if ( c != null )
				{
					domainID = c.Domain;
				}
			}

			return domainID;
		}

		/// <summary>
		/// Updates the cached access control after the object has been committed.
		/// </summary>
		internal void UpdateAccessControl()
		{
			ace.Rights = AceProperty.Rights;
		}

		/// <summary>
		/// Set the passphrase(key encrypted by passphrase and SHA1 of key) and recovery agent name and key
		/// </summary>
		public bool ServerSetDefaultAccount(string iFolderID)
		{
			try
			{
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);

				Domain domain = store.GetDomain(GetDomainID(store));	
				Member member = domain.GetMemberByID(UserID);
				
				log.Debug("ServerSetdefault account: user:{0}...userID={1}",member.Name, UserID);
			//	if(iFolderID !=null)
			//	{
					Property p = new Property(PropertyTags.DefaultAccount, iFolderID);
					p.LocalProperty = true;
					this.properties.ModifyNodeProperty(p);
					log.Debug("Modified default account to: {0}", iFolderID);
			//	}
				Property pr = this.properties.FindSingleValue( PropertyTags.DefaultAccount);
				string iFID = (pr!=null) ? pr.Value as string : null;
				if( iFID != null)
					log.Debug("ServerGetDefault: got {0}", iFID);
				else
					log.Debug("ServerGetDefault gives null");
				domain.Commit(this);
				
				return true;
			}
			catch(Exception ex)
			{
				log.Debug("exception: {0}", ex.Message);
				return false;
			}
		}

		/// <summary>
		/// This adds the current member as the group admin with the preference as the rights.
		/// </summary>
		public void AddToGroupList( string groupid, int preference)
		{
			try
			{
				string value = null;
				value = groupid+":"+Convert.ToString(preference);
				RemoveFromGroupList(groupid);
				this.properties.AddNodeProperty(PropertyTags.UserAdminRights, value);
				this.Rights = Access.Rights.Secondary;
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(GetDomainID(store));
				domain.Commit(this);
			}
			catch(Exception ex)
			{
				log.Debug("Exception in AddToGroupList. message: {0}--{1}", ex.Message, ex.StackTrace);
				throw;
			}
		}

		/// <summary>
		/// Remove the current member as group admin for the groupid passed. Returns the number of groups for which the current user is a group admin.
		/// </summary>
		public int RemoveFromGroupList(string groupid)
		{
			try
			{
				string[] groupvaluearray = GetGroupListValues(false);
				if( groupvaluearray == null)
					return 0;
				RemoveGroupList();
				int NoOfGroups = 0;
				foreach(string str in groupvaluearray)
				{
					if( str != null && str.StartsWith(groupid) )
						continue;
					else
					{
						this.properties.AddNodeProperty(PropertyTags.UserAdminRights, str);
						NoOfGroups++;
					}
				}
				// If no groups are remaining for this admin, then change his rights to ReadOnly so that he becomes normal user
				if(NoOfGroups == 0)
					this.Rights = Access.Rights.ReadOnly;
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(GetDomainID(store));
				domain.Commit(this);		
				return NoOfGroups;
			}
			catch(Exception ex)
			{
				log.Debug("Exception in RemoveFromGroupList. message: {0}--{1}", ex.Message, ex.StackTrace);
				throw;
			}
		}

		/// <summary>
		/// Returns the rights for this member on the group, given the group id.
		/// </summary>
		public int GetPreferencesForGroup(string groupid)
		{
			log.Debug("Entering GetPreferencesForGroup: {0}", groupid);
			string[] grouplist = GetGroupListValues(false);
			if( grouplist == null)
				return 0;
			string[] groupadmins;

			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID(groupid);
			if( member == null)
			{
				throw new Exception("Member does not exist");
			}
			Property Groupproperty = member.Properties.GetSingleProperty( "GroupType" );
			if( Groupproperty == null)
			{
				/// Member node.
				groupadmins = domain.GetMemberFamilyList(groupid);
			}
			else
			{
				groupadmins = new string[1];
				groupadmins[0] = groupid;
			}

			foreach(string str in grouplist)
			{
				foreach(string groupid1 in groupadmins)
				{
					if( str.StartsWith(groupid1) )		
					{
						int ind = str.LastIndexOf(":");
						if( ind > 0)
						{
							string retval = str.Substring( ind+1);
							log.Debug("The preferences value: {0}--{1}", retval, Convert.ToInt32(retval));
							int val = Convert.ToInt32(retval);
							return val;
						}
					}
				}
			}
			return 0;
		}

		/// <summary>
		/// Returns all the groups for which the current member is an admin for.
		/// If the onlygroups flag is set, it returns only the group list array.
		/// Otherwise it gives the groups appended by the corresponding rights.
		/// </summary>
		public string[] GetGroupListValues(bool onlygroups)
		{
			log.Debug("Calling GetGroupListValues");
			MultiValuedList mvl = this.Properties.GetProperties( PropertyTags.UserAdminRights );
			if( mvl == null )
				return null;

			ArrayList grouplist  = new ArrayList();
			if(mvl != null )
			{
				foreach( Property p in mvl )
				{
					if( p!= null && p.Value as string != null)
					{
						if(onlygroups)
						{
							string value = p.Value as string;
							int ind = value.LastIndexOf(":");
							if( ind > 0)
							{
								grouplist.Add(value.Substring(0, ind));
							}
							else
							{
								continue;
							}
						}
						else
							grouplist.Add(p.Value as string);
					}
				}
			}		
			return (string[])grouplist.ToArray(typeof(string));
		}

		/// <summary>
		/// Removes the current member as a group admin from all the groups.
		/// </summary>
		public void RemoveGroupList()
		{
			try
			{
				this.Properties.DeleteNodeProperties(PropertyTags.UserAdminRights);
				this.Rights = Access.Rights.Secondary;
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(GetDomainID(store));
				domain.Commit(this);		
				return;
			}
			catch(Exception ex)
			{
				log.Debug("message: {0}--{1}", ex.Message, ex.StackTrace);
				throw;
			}
		}

		/// <summary>
		/// Returned the list of all groups for which the current user is an admin for.
		/// </summary>
		public string[] GetMonitoredGroups()
		{
			return GetGroupListValues(true);
		}

		/// <summary>
		/// Returns set of users which are a part of the groups he is admin for.
		/// If the includeGroup flag is set, it returns the group members as well.
		/// </summary>
		public Hashtable GetMonitoredUsers(bool includeGroup)
		{
			Hashtable ht = new Hashtable();
			string[] groups = GetMonitoredGroups();
			if( groups == null || groups.Length ==0)
				return ht;
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			foreach(string group in groups)
			{
				string[] members = domain.GetGroupsMemberList(group);
				foreach(string member in members)
				{
					if( !ht.ContainsKey(member))
						ht.Add(member, "");
				}
				if( includeGroup == false && ht.ContainsKey(group))
				{
					ht.Remove(group);
				}
			}
			return ht;
		}

		/// <summary>
		/// Returns set of groups / subgroups owned by secondary admin
		/// </summary>
		public string[] GetMonitoredSubGroups()
		{
			string[] groups = GetMonitoredGroups();
			ArrayList list = new ArrayList();
			if( groups != null && groups.Length != 0 )
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				foreach(string group in groups)
				{
					Member groupObject = domain.GetMemberByID(group);
					string dn = String.Empty;
					try
					{
						dn = groupObject.Properties.GetSingleProperty( "DN" ).Value as string;
					}
					catch{}
					if(dn != null)
					{
						string[] subgroups = domain.GetGroupsSubgroupList(dn);
						foreach(string subgroup in subgroups)
						{
							list.Add(subgroup);
						}
					}
				}
			}
			return (string[])list.ToArray( typeof( string ) );
		}

		/// <summary>
		/// Given a member/group id, returns whether the current member is an admin for that.
		/// </summary>
		public bool IsGroupAdmin(string nodeid)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID(nodeid);
			if( member == null)
				return false;
			Property Groupproperty = member.Properties.GetSingleProperty( "GroupType" );
			if( Groupproperty == null)
			{
				/// Member node.
				return IsGroupAdmin(nodeid, 0);
			}
			else
				return IsGroupAdmin(nodeid, 1);
		}

		/// <summary>
		/// Given a member/group id, returns whether the current member is an admin for that.
		/// Member type determines whether the id passed is a member id or groupid.
		/// 0 for user type and 1 for group type.
		/// </summary>
		public bool IsGroupAdmin(string nodeid, int membertype)
		{
			if( membertype == 0)
			{
				/// Member node. Get all the groups.
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				string[] groupids = domain.GetMemberFamilyList(nodeid);
				return IsGroupAdmin(groupids);
			}
			else
			{
				/// Group node.
				string[] groupids = new string[1];
				groupids[0] = nodeid;
				return IsGroupAdmin(groupids);
			}
		}

		
		/// <summary>
		/// Checks whether the member is a group admin for any of the group's passed as input.
		/// </summary>
		public bool IsGroupAdmin(string[] groupids)
		{
			string[] groupadmins = GetGroupListValues(true);
			
			/// Checks whether the given member is admin for any of the groups mentioned above...
			if( groupids == null || groupadmins == null)
				return false;

			foreach( string groupid in groupids)
			{
				foreach( string groupadmin in groupadmins )
				{
					if( groupid == groupadmin )
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Set the passphrase(key encrypted by passphrase and SHA1 of key) and recovery agent name and key
		/// </summary>
		public string ServerGetDefaultiFolder()
		{
			try
			{
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);

				Domain domain = store.GetDomain(GetDomainID(store));	
				Member member = domain.GetMemberByID(UserID);
				
				log.Debug("ServerGetdefaultiFolder user:{0}...userID={1}",member.Name, UserID);
				Property p = this.properties.FindSingleValue( PropertyTags.DefaultAccount);
				string iFolderID = (p!=null) ? p.Value as string : null;
				if( iFolderID != null)
					log.Debug("ServerGetDefault: got {0}", iFolderID);
				else
					log.Debug("ServerGetDefault gives null");
				if( iFolderID != null)
				{
					Collection col = store.GetCollectionByID( iFolderID );
					if( col == null)
					{
						log.Debug("Collection does not exist: ");
						iFolderID = null;
					}
					else
						log.Debug("Collection Exists:");
				}
				return iFolderID;
			}
			catch(Exception ex)
			{
				log.Debug("exception: {0}", ex.Message);
				return null;
			}
		}


		/// <summary>
		/// Set the passphrase(key encrypted by passphrase and SHA1 of key) and recovery agent name and key
		/// </summary>
		public void ServerSetPassPhrase(string EncryptedCryptoKey, string CryptoKeyBlob, string RAName, string RAPublicKey)
		{
			Store store = Store.GetStore();
			string DomainID = this.GetDomainID(store);
			string UserID = store.GetUserIDFromDomainID(DomainID);

			Domain domain = store.GetDomain(GetDomainID(store));	
			Member member = domain.GetMemberByID(UserID);
			
			log.Debug("ServerSetPassPhrase user-name:{0}...userID={1}",member.Name, UserID);
			accessLog.LogAccess("ServerSetPassPhrase","Setting Passphrase","Starting",member.Name);
						
			if(EncryptedCryptoKey !=null)
			{
				Property p = new Property(PropertyTags.EncryptionKey, EncryptedCryptoKey);
				p.LocalProperty = true;
				this.properties.ModifyNodeProperty(p);
			}
			if(CryptoKeyBlob !=null)
			{
				Property p = new Property(PropertyTags.EncryptionBlob, CryptoKeyBlob);
				p.LocalProperty = true;
				this.properties.ModifyNodeProperty(p);
			}
			if(RAName !=null)
			{
				Property p = new Property(PropertyTags.RAName, RAName);
				p.LocalProperty = true;
				this.properties.ModifyNodeProperty(p);
			}
			if(RAPublicKey !=null)
			{
				Property p = new Property(PropertyTags.RAPublicKey, RAPublicKey);
				p.LocalProperty = true;
				this.properties.ModifyNodeProperty(p);
			}
			
			Property prty = new Property(PropertyTags.EncryptionVersion, EncVersion.version.ToString());
			prty.LocalProperty= true;
			this.properties.ModifyNodeProperty(prty);			
			
			domain.Commit(this);
			accessLog.LogAccess("ServerSetPassPhrase","Setting Passphrase","Completed",member.Name);
			
		}
		
		/// <summary>
        /// Simias client will call this whenever user sets passphrase for the first time. through thick client.
		/// </summary>
		/// <param name="store">Stroe in which encryption flag has to be set</param>
		public void SetEncryptionBlobFlag(Store store)
		{
			bool set = false;
                        int count = 1;
			// From Linux client , pass phrase is not being set for the first 			 //time due to exception, Error writing request, possibly mono bug
                        while( set == false && count <=3 )
                        {
				count++;
				try
				{
					string DomainID = this.GetDomainID(store);
					string userID = store.GetUserIDFromDomainID(DomainID);
					HostNode host = HostNode.GetMaster(DomainID);
	
					SimiasConnection smConn = new SimiasConnection(DomainID,
												userID,
												SimiasConnection.AuthType.BASIC,
												host);
					SimiasWebService svc = new SimiasWebService();
					svc.Url = host.PublicUrl;

					smConn.Authenticate ();
					smConn.InitializeWebClient(svc, "Simias.asmx");
					svc.SetOnMasterEncryptionBlobFlag(DomainID, UserID);
                			smConn.ClearConnection();

			                //SetEncryptionBlobFlag it on Home ServerGetDefaultiFolder also.
			                if (host.UserID != this.HomeServer.UserID)
                			{
			                        host = this.HomeServer;
				 		smConn = new SimiasConnection(DomainID, userID, SimiasConnection.AuthType.BASIC, host);
				                svc = new SimiasWebService();
			                        svc.Url = host.PublicUrl;

			                        smConn.Authenticate();
			                        smConn.InitializeWebClient(svc, "Simias.asmx");
				                svc.SetOnMasterEncryptionBlobFlag(DomainID, UserID);
			                        smConn.ClearConnection();
                			}
					set = true;
					log.Debug("Passphrase set sucessfully");
				}
				catch
				{
					log.Debug("SetEncryptionBlobFlag : pass-phrase status could not be set");
					continue;
				}
			}
		}


		/// <summary>
        /// whenever user sets passphrase for the first time. throughweb-access, this will be called. 
		/// </summary>
		/// <param name="store">Store in which encryption flag has to be set</param>
		/// <param name="userID">To which User ID the flag has to set</param>
		public void SetEncryptionBlobFlagServer(Store store, string userID)
		{

			try
			{
				string DomainID = this.GetDomainID(store);
				//string userID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = HostNode.GetMaster(DomainID);
                HostNode home = this.HomeServer;
				bool OnMaster = false;
				// do a simias connection only if user is not on master server
				// for bug #408392, for admin connecting through simias webservice is not working, and no need
				// to connect since admin is already provisioned to master (same applies for other users)
				HostNode hNode = HostNode.GetLocalHost();
				if(hNode.UserID == host.UserID)
				{
					OnMaster = true;
				}
				if(OnMaster)
				{
					this.SetOnMasterEncryptionBlobFlag(DomainID);
					log.Debug("SetEncryptionBlobFlagServer: User is on master, set encryptionblob flag locally");
				}
				else
				{
					SimiasConnection smConn = new SimiasConnection(DomainID,
													userID,
													SimiasConnection.AuthType.PPK,
													host);
					SimiasWebService svc = new SimiasWebService();
					svc.Url = host.PublicUrl;
	
					smConn.Authenticate ();
					smConn.InitializeWebClient(svc, "Simias.asmx");
					svc.SetOnMasterEncryptionBlobFlag(DomainID, UserID);
					log.Debug("SetEncryptionBlobFlagServer: user is not on master, set encryptionblob flag through web-service");
				}
                // This flag was being only set on the Master . Now this is being set on the Home Server of the User.
                if ( home.UserID != host.UserID )
                {
                    SimiasConnection smConn = new SimiasConnection(DomainID,
                                                       userID,
                                                       SimiasConnection.AuthType.PPK,
                                                       home);
                    SimiasWebService svc = new SimiasWebService();
                    svc.Url = home.PublicUrl;

                    smConn.Authenticate();
                    smConn.InitializeWebClient(svc, "Simias.asmx");
                    svc.SetOnMasterEncryptionBlobFlag(DomainID, UserID);
                    log.Debug("SetEncryptionBlobFlagServer: user is not on master nor on home server set encryptionblob flag on home server through web-service");
                }

			}
			catch(Exception ex)
			{
				log.Debug("SetEncryptionBlobFlagServer: pass-phrase status could not be set {0}", ex.Message);
			}

		}
		
		/// <summary>
		/// Commits the value iFolderMoveState. Used during moving iFolder from one machine to other.
		/// </summary>
		/// <param name="DomainID">Domain ID to which iFolder belongs</param>
		/// <param name="set">Whether to set or to get the value</param>
        /// <param name="iFolderID">ID of iFolder which we have to handle</param>
		/// <param name="state">Whether to delete or to modify or to add</param>
		/// <param name="iFolderSize">Size of iFolder</param>
		/// <returns>If set is false, it returns the value of state. If set is true, sets the value and returns 0</returns>
		public int iFolderMoveState(string DomainID, bool set, string iFolderID, int state, long iFolderSize)
		{	
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(DomainID);
			
			if(set == true)
			{
				Property pNew = new Property(iFolderID, String.Format("{0}:{1}",state.ToString(), iFolderSize.ToString()));
				pNew.LocalProperty = true;
				Property p = properties.GetSingleProperty(iFolderID);
				if(p != null)
				{
					if(state == 0)
						p.DeleteProperty();
					else
						properties.ModifyNodeProperty(pNew);
					domain.Commit(this);
				}
				else
				{
					if(state != 0)
					{
					 	properties.AddNodeProperty(pNew);
						domain.Commit(this);
					}
				}
				log.Debug("iFolderMoveState: committing this value for iFolder move state: " + state);
			}
			else
			{
				Property p = properties.GetSingleProperty(iFolderID);
				if(p != null)
				{
					string result = (string)p.Value;
					string [] MoveStatus = result.Split(new char[] {':'});
					return (int)Convert.ToInt32(MoveStatus[0]);
				}
			}
			return 0;
		}		

        /// <summary>
        /// To get the size of iFolder that was moved from one machine to other
        /// </summary>
        /// <param name="DomainID">Domain ID in which iFolder is available</param>
        /// <param name="iFolderID">ID of iFolder</param>
        /// <returns></returns>
		public long MovediFolderSize(string DomainID, string iFolderID)
		{	
			
			Property p = properties.GetSingleProperty(iFolderID);
			if(p != null)
			{
				string result = (string)p.Value;
				string [] MoveStatus = result.Split(new char[] {':'});
				return (long)Convert.ToInt64(MoveStatus[1]);
			}
			return 0;
		}		
	
		/// <summary>
        /// This method is used to modify the properties of member. Called from Simias.asmx
        /// </summary>
        /// <param name="ModifiedMember">Member to be modified</param>
        /// <param name="DomainID">Domain ID to which member belongs</param>
		public void ModifyMemberProperties(Member ModifiedMember, string DomainID)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(DomainID);
			Member MemberOnMaster = domain.GetMemberByID(ModifiedMember.UserID);
			ModifyProperties(ref MemberOnMaster, ModifiedMember, Simias.Policy.FileTypeFilter.FileTypeFilterPolicyID);
			ModifyProperties(ref MemberOnMaster, ModifiedMember, Simias.Policy.DiskSpaceQuota.DiskSpaceQuotaPolicyID);
			ModifyProperties(ref MemberOnMaster, ModifiedMember, Simias.Policy.FileSizeFilter.FileSizeFilterPolicyID);
			ModifyProperties(ref MemberOnMaster, ModifiedMember, Simias.Policy.iFolderLimit.iFolderLimitPolicyID);
			ModifyProperties(ref MemberOnMaster, ModifiedMember, Simias.Policy.SecurityState.EncryptionStatePolicyID);
			ModifyProperties(ref MemberOnMaster, ModifiedMember, Simias.Policy.Sharing.SharingPolicyID);
			ModifyProperties(ref MemberOnMaster, ModifiedMember, Simias.Policy.SyncInterval.SyncIntervalPolicyID);
			log.Debug("ModifyMemberProperties: going to commit the modified member object");
			domain.Commit(MemberOnMaster);
		}
		
        /// <summary>
        /// This method is used to modify the properties of member with reference to master.
        /// </summary>
        /// <param name="MemberOnMaster">Reference to Member on master</param>
        /// <param name="ModifiedMember">Member to be modified</param>
        /// <param name="PolicyID">ID of policy to be modified</param>
		public void ModifyProperties(ref Member MemberOnMaster, Member ModifiedMember, string PolicyID)
		{

			MultiValuedList mvl = ModifiedMember.Properties.GetProperties(PolicyID);
			if(mvl == null || mvl.Count == 0)
			{
				return ;
			}
			MemberOnMaster.Properties.DeleteNodeProperties(PolicyID);
			foreach( Property p in mvl )
			{
				MemberOnMaster.Properties.AddProperty(p);
			}
		}

        /// <summary>
        /// Sets the master encryption blob flag
        /// </summary>
        /// <param name="DomainID">Domain Id to which the flag has to set</param>
		public void SetOnMasterEncryptionBlobFlag(string DomainID)
		{	
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(DomainID);
			Property p = properties.GetSingleProperty(PropertyTags.SecurityStatus);
			int value ;
			if(p != null)
			{
				value = (int)p.Value;
				value |= (int)EncryptionBlobFlag.BlobFlag ;
			}
			else
			{
				value = (int)EncryptionBlobFlag.BlobFlag ;
			}
			Property pNew = new Property(PropertyTags.SecurityStatus, value);
			properties.ModifyNodeProperty(pNew);
			log.Debug("SetOnMasterEncryptionBlobFlag: committing this value for securitystate: "+value);
			domain.Commit(this);
		}		

		/// <summary>
		/// Validate the passphrase
		/// </summary>
		public string ServerGetEncrypPassKey()
		{
			return this.EncryptionKey;
		}

		/// <summary>
		/// Validate the passphrase
		/// </summary>
		public string ServerGetPassKeyHash()
		{
			return this.EncryptionBlob;
		}

		/// <summary>
		/// whether passphrase is set or not , returns a bool value
		/// </summary>
		public bool PassPhraseSetStatus()
		{
			
			return this.EncryptionBlobStatus;
		}

		
		/// <summary>
		/// Set the passphrase(key encrypted by passphrase and SHA1 of key) and recovery agent name and key
		/// </summary>
		public void SetPassPhrase(string Passphrase, string RAName, string RAPublicKey)
		{
            string DomainID = null;
			try
			{
				if(RAPublicKey != null && RAPublicKey != "" && RAName != "DEFAULT")//RAName null allowed
					KeyCorrection(ref RAName, ref RAPublicKey);			
				
				Store store = Store.GetStore();
				DomainID = this.GetDomainID(store);
                Domain domain = store.GetDomain(DomainID);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = this.HomeServer; //home server
                // suspend the current domain sync for a while so that passphrase setting goes through fast
                SyncClient.Suspend(DomainID);

				SimiasConnection smConn = new SimiasConnection(DomainID,
															UserID,
															SimiasConnection.AuthType.BASIC,
															host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;

				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");

				//Hash the passphrase and use it for encryption and decryption
				PassphraseHash hash = new PassphraseHash();
				byte[] passphrase = hash.HashPassPhrase(Passphrase);
				
				Key key = new Key((passphrase.Length)*8);//create the key 
				string EncrypCryptoKey;
				key.EncrypytKey(passphrase, out EncrypCryptoKey); //encrypt the key
				Key HashKey = new Key(EncrypCryptoKey);
				
				log.Debug("SetPassPhrase:  {0}...{1}...{2}...{3}",EncrypCryptoKey, HashKey.HashKey(), RAName, RAPublicKey);
				svc.ServerSetPassPhrase(DomainID, UserID, EncrypCryptoKey, HashKey.HashKey(), RAName, RAPublicKey);
				
				SetEncryptionBlobFlag(store);
				//making it local variable for faster disposal
				CollectionSyncClient syncClient = null;

                // commit encryption related values locally on client for faster access. it will be overwritten in 
                // next fomain sync
                try
                {
                    this.EncryptionKey = EncrypCryptoKey;
                    this.EncryptionBlob = HashKey.HashKey();
                    this.RAName = RAName;
                    this.RAPublicKey = RAPublicKey;
                    this.EncryptionVersion = "version";

                    Property p = properties.GetSingleProperty(PropertyTags.SecurityStatus);
                    int value;
                    if (p != null)
                    {
                        value = (int)p.Value;
                        value |= (int)EncryptionBlobFlag.BlobFlag;
                    }
                    else
                    {
                        value = (int)EncryptionBlobFlag.BlobFlag;
                    }
                    Property pNew = new Property(PropertyTags.SecurityStatus, value);
                    pNew.LocalProperty = true;
                    properties.ModifyNodeProperty(pNew);
                    domain.Commit(this);
				}
				catch
				{
					//ignoring exceptions, since domain is synced periodically
					//whatif passphrase is not synced??? -- FIXFIX
                    log.Debug("Setting the encryption values locally failed - SetPassPhrase");
				}
			}
			catch(Exception ex)
			{
				log.Debug("SetPassPhrase : {0}", ex.Message);
				throw ex;
			}
		}

                /// <summary>
                /// Change the password for this member, It will be called by thick client and will be running on simias client
                /// </summary>
                public int ChangePassword(string OldPassword, string NewPassword)
                {
                        Store store = Store.GetStore();
                        string DomainID = this.GetDomainID(store);

                        HostNode host = this.HomeServer; //home server

                        SimiasConnection smConn = new SimiasConnection(DomainID,
                                                                                        this.UserID,
                                                                                        SimiasConnection.AuthType.BASIC,
                                                                                        host);
                        DomainService svc = new DomainService();
                        svc.Url = host.PublicUrl;

                        smConn.Authenticate ();
                        smConn.InitializeWebClient(svc, "DomainService.asmx");

                        return( svc.ChangePasswordOnServer(DomainID, UserID, OldPassword, NewPassword)) ;

                }


		/// <summary>
		/// Call back for timer
		/// </summary>
		/// <param name="collectionClient">Collection client as object to handle</param>
		public void TimerFired( object collectionClient )
		{

		}

        /// <summary>
        /// Sets the default account for the user.
        /// </summary>
        /// <param name="iFolderID">ID of iFolder to set as default account</param>
        /// <returns>Returns true if success or else false</returns>
		public bool DefaultAccount(string iFolderID)
		{
			try
			{
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = this.HomeServer; //home server

				SimiasConnection smConn = new SimiasConnection(DomainID,
															UserID,
															SimiasConnection.AuthType.BASIC,
															host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;

				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");		
				return svc.ServerSetDefaultAccount(DomainID, UserID, iFolderID);
			}
			catch(Exception ex)
			{
				log.Debug("SetPassPhrase : {0}", ex.Message);
				return false;
			}
		}

        /// <summary>
        /// Gets the default iFolder name
        /// </summary>
        /// <returns>Returns the name of default iFolder</returns>
		public string GetDefaultiFolder()
		{
			try
			{
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = this.HomeServer; //home server

				SimiasConnection smConn = new SimiasConnection(DomainID,
															UserID,
															SimiasConnection.AuthType.BASIC,
															host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;

				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");		
				return svc.ServerGetDefaultiFolder(DomainID, UserID);
			}
			catch(Exception ex)
			{
				log.Debug("SetPassPhrase : {0}", ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Set the passphrase(key encrypted by passphrase and SHA1 of key) and recovery agent name and key
		/// </summary>
		public bool ReSetPassPhrase(string OldPassphrase, string Passphrase, string RAName, string RAPublicKey)
		{
             		log.Debug("Enter ReSetPassPhrase Function, old :{0} , new:{1}, RAName:{2}, RAPublicKey:{3}",
                 	OldPassphrase, Passphrase, RAName, RAPublicKey);
             		bool decryptpassed = true;
             		if (ValidatePassPhrase(OldPassphrase) != Simias.Authentication.StatusCodes.Success)
             		{
                 		log.Debug("Failed for Old:{0}", OldPassphrase);
                 		if (ValidatePassPhrase(Passphrase) != Simias.Authentication.StatusCodes.Success)
                 		{
                     			log.Debug("Reset Pass phrase passed for both old and new:{0}", Passphrase);
                     			return false;
                 		}
                 		log.Debug("Reset Pass phrase passed for old and and passed for new");
             		}

			try
			{	
				if(RAPublicKey != null && RAPublicKey != "" && RAName != "DEFAULT")//RAName null allowed - find a better way to represent "DEFAULT"
					KeyCorrection(ref RAName, ref RAPublicKey);				
				
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = this.HomeServer; //home server
				
				SimiasConnection smConn = new SimiasConnection(DomainID,
															UserID,
															SimiasConnection.AuthType.BASIC,
															host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;

				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");

				//Hash the passphrase and use it for encryption and decryption
				PassphraseHash hash = new PassphraseHash();
				byte[] passphrase = hash.HashPassPhrase(Passphrase);

				//Hash the passphrase and use it for encryption and decryption
				byte[] oldPassphrase = hash.HashPassPhrase(OldPassphrase);

				Key key = new Key(128);
				string EncrypCryptoKey = null;
				key.EncrypytKey(passphrase, out EncrypCryptoKey);			
				Key HashKey = new Key(EncrypCryptoKey);
				
				svc.ServerSetPassPhrase(DomainID, UserID, EncrypCryptoKey, HashKey.HashKey(), RAName, RAPublicKey);

				CollectionKey OldKey = null;
				CollectionKey NewKey = new CollectionKey();
				int index = 0;
				string DecryptedKey = null;
				string EncryptedKey = null;
				
				while((OldKey = svc.GetiFolderCryptoKeys(DomainID, UserID, index)) != null)
				{

                     try
                     {
                         log.Debug("In side while loop");
                         //Decrypt and encrypt the key
                         Simias.Storage.Key DeKey = new Key(OldKey.PEDEK);
                         DeKey.DecrypytKey(oldPassphrase, out DecryptedKey);
                         Simias.Storage.Key EnKey = new Key(DecryptedKey);
                         EnKey.EncrypytKey(passphrase, out EncryptedKey);
                         //Send back to server
                         NewKey.NodeID = OldKey.NodeID;
                         NewKey.PEDEK = EncryptedKey;
                         if (RAPublicKey != null && RAPublicKey != "")
                         {
                             RecoveryAgent agent = new RecoveryAgent(RAPublicKey);
                             NewKey.REDEK = agent.EncodeMessage(DecryptedKey); // recoveryKey
                         }
                         else
                             NewKey.REDEK = null; // since we are not changing the recovery agent
 
                         if (svc.SetiFolderCryptoKeys(DomainID, UserID, NewKey) == false)
                         {
                             log.Debug("ReSetPassPhrase : failed for ifolder ID:", NewKey.NodeID);
                             //throw new CollectionStoreException("The specified cryptographic key not found");
                             decryptpassed = false;
                         }
                     }
                     catch (Exception ex)
                     {
                         log.Debug("Exception in reset pass: {0}--{1}", ex.Message, ex.StackTrace);
                     }
                     finally
                     {
                         index++;
                     }
 
		

				}

				//making it local variable for faster disposal
				CollectionSyncClient syncClient = null;
				try{

				log.Debug("ReSetPassPhrase Domain sync begin");
				syncClient = new CollectionSyncClient(DomainID, new TimerCallback( TimerFired ) );
				syncClient.SyncNow();
				log.Debug("ReSetPassPhrase Domain sync end");
				}
				catch
				{
					//ignoring exception as domain sync happens periodically
					log.Debug("Domain Sync failed - ResetPassPhrase");
				}
			}
			catch(Exception ex)
			{
				log.Debug("ReSetPassPhrase : {0}", ex.Message);
				throw ex;
			}
			return decryptpassed;
		}

		/// <summary>
		/// KeyCorrection
		/// </summary>
		public void KeyCorrection(ref string RAName, ref string RAPublicKey)
		{
			//caller has to validate the input params
			//if(RAPublicKey != null && RAPublicKey != "" && RAName != null && RAName != "")
			{
				byte [] key = Convert.FromBase64String(RAPublicKey);
				if(key.Length > 64 && key.Length < 128) //remove the 5 byte header and 5 byte trailer
				{
					byte[] NewKey = new byte[key.Length-10];
					Array.Copy(key, 5, NewKey, 0, key.Length-10);
					RAPublicKey = Convert.ToBase64String(NewKey);
				}
				else if(key.Length > 128 && key.Length < 256) //remove the 7 byte header and 5 byte trailer
				{
					byte[] NewKey = new byte[key.Length-12];
					Array.Copy(key, 7, NewKey, 0, key.Length-12);
					RAPublicKey = Convert.ToBase64String(NewKey);
				}					
				else if(key.Length > 256) //remove the 9 byte header and 5 byte trailer
				{
					byte[] NewKey = new byte[key.Length-14];
					Array.Copy(key, 9, NewKey, 0, key.Length-14);
					RAPublicKey = Convert.ToBase64String(NewKey);
				}					
				else
				{
					log.Debug("KeyCorrection RAName: {0}", RAName);
					log.Debug("KeyCorrection RAPublicKey: {0}", RAPublicKey);
					log.Debug("KeyCorrection key.Length: {0}", key.Length);
					throw new SimiasException("Recovery key size not suported");
				}
			}
		}

                /// <summary>
                /// adds or removes the members DN from the search contexts
                /// </summary>
                public bool UpdateSearchContexts(bool add)
             	{
			log.Debug("UpdateSearchContexts: {0}", add.ToString());
                        try
                        {
				Configuration config = new Configuration(Store.StorePath, true );
				bool ContextPresent = false;
				string OldLdapSystemBookSection = "LdapProvider";
				string SearchKey = "Search";
				string XmlContextTag = "Context";
				string Context ;
				ArrayList searchContexts = new ArrayList();
				XmlElement searchElement;
				string XmlDNAttr = "dn";
                                Property p = properties.FindSingleValue("DN");
				if(p == null)
					return false;
                                string memberDN = (string) p.Value as string ;
                                searchElement = config.GetElement( OldLdapSystemBookSection, SearchKey );
                                if ( searchElement != null )
                                {
                                        XmlNodeList contextNodes = searchElement.SelectNodes( XmlContextTag );
                                        foreach( XmlElement contextNode in contextNodes )
                                        {
                                                Context = contextNode.GetAttribute( XmlDNAttr );
						if( add == true )
						{
							log.Debug("UpdateSearchContexts: add Checking {0}:{1}", memberDN, Context);
							if(memberDN.ToLower().EndsWith(Context))	
								return true;
							else
                                                		searchContexts.Add( Context );
						}	
						else 
						{
							log.Debug("UpdateSearchContexts: delete Checking {0}:{1}", memberDN, Context);
							if(String.Compare(Context, memberDN) != 0)
                                                		searchContexts.Add( Context );
							else
								ContextPresent = true;
						}
                                        }
					if(add)
						searchContexts.Add( memberDN );
					else if(!ContextPresent)
						return true;
                                }
                                string configFilePath =
                                        Path.Combine( Store.StorePath, Simias.Configuration.DefaultConfigFileName );
				
				Hashtable Dup = new Hashtable();

                                // Load the configuration file into an xml document.
                                XmlDocument configDoc = new XmlDocument();
                                configDoc.Load( configFilePath );
                                searchElement = GetSearchElement( configDoc, OldLdapSystemBookSection, SearchKey );
                                if ( searchElement != null )
                                {
                                        XmlNodeList contextNodes = searchElement.SelectNodes( XmlContextTag );
					
                                        for( int count = contextNodes.Count - 1; count >= 0; count-- )
                                        {
                                                searchElement.RemoveChild( contextNodes[count] );
                                        }

                                        foreach( string dn in searchContexts )
                                        {
						if( ! Dup.ContainsKey(dn) )
						{
							log.Debug("UpdateSearchContexts: Adding {0}", dn);
       		                                        XmlElement element = configDoc.CreateElement( XmlContextTag );
       		                                        element.SetAttribute( XmlDNAttr, dn );
                	                                searchElement.AppendChild( element );
							Dup.Add( dn, "" );
						}
                                        }
                                }
				else
					log.Debug("UpdateSearchContexts: searchElement is null ");
				log.Debug("UpdateSearchContexts: Updating Configfile {0}", configFilePath);
			 	XmlTextWriter xtw = new XmlTextWriter( configFilePath, Encoding.UTF8 );
				try
                                {
                                        xtw.Formatting = Formatting.Indented;
                                        configDoc.WriteTo( xtw );
                                }
                                finally
                                {
                                        xtw.Close();
					Dup.Clear();
                                }

                        }
                        catch(Exception ex)
                        {
                                log.Debug("UpdateSearchContexts : Exception {0}", ex.Message);
				return false;
                        }
                        return true;
                }

                private XmlElement GetSearchElement( XmlDocument document, string OldLdapSystemBookSection, string SearchKey)
                {
			string SectionTag = "section";
			string NameAttr = "name";
			string LdapSystemBookSection = "LdapProvider";
			string SettingTag = "setting";
                        string str = String.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, LdapSystemBookSection, SettingTag, SearchKey );
                        XmlElement element = ( XmlElement )document.DocumentElement.SelectSingleNode( str );
                        if ( element == null )
                        {
                                // The setting doesn't exist, so create it.
                                element = document.CreateElement( SettingTag );
                                element.SetAttribute( NameAttr, SearchKey );
                                str = string.Format( "//{0}[@{1}='{2}']", SectionTag, NameAttr, LdapSystemBookSection );
                                XmlElement eSection = ( XmlElement )document.DocumentElement.SelectSingleNode(str);
                                if ( eSection == null )
                                {
                                        // If the section doesn't exist, create it.
                                        eSection = document.CreateElement( SectionTag );
                                        eSection.SetAttribute( NameAttr, LdapSystemBookSection );
                                        document.DocumentElement.AppendChild( eSection );
                                }

                                eSection.AppendChild(element);
                        }

                        return element;
                }



                /// <summary>
                /// GetGroupsiFolderLimitPolicy gets you the user groups sharing policy.
                /// </summary>
                public int GetGroupsiFolderLimitPolicy(string DomainID, string UserID)
             	{
                        int Count = -1;
                        try
                        {
                                HostNode host = this.HomeServer; //home server

                                SimiasConnection smConn = new SimiasConnection(DomainID,
                                                                                        UserID,
                                                                                        SimiasConnection.AuthType.BASIC,
                                                                                        host);
                                SimiasWebService svc = new SimiasWebService();
                                svc.Url = host.PublicUrl;

                                smConn.Authenticate ();
                                smConn.InitializeWebClient(svc, "Simias.asmx");

                                Count = svc.GetGroupsiFolderLimitPolicy(DomainID, UserID);
                        }
                        catch(Exception ex)
                        {
                                log.Debug("GetGroupsiFolderLimitPolicy : {0}", ex.Message);
                                Count = -1;
                        }
                        return Count;
                }

		/// <summary>
               /// GetUseriFolderLimitPolicy gets you the user limit policy - transfer of ownership.
                /// </summary>
                public bool IsTransferAllowed(string DomainID, string oldownerID, string newownerID)
                {
                        bool result = true;
                        try
                        {
                                HostNode host = this.HomeServer; //home server

                                SimiasConnection smConn = new SimiasConnection(DomainID,
                                                                                        oldownerID,
                                                                                        SimiasConnection.AuthType.BASIC,
                                                                                        host);
                                SimiasWebService svc = new SimiasWebService();
                                svc.Url = host.PublicUrl;

                                smConn.Authenticate ();
                                smConn.InitializeWebClient(svc, "Simias.asmx");

                                result = svc.IsTransferAllowed(DomainID, newownerID);
                        }
                        catch(Exception ex)
                        {
                                log.Debug("GetUseriFolderLimitPolicy : {0}", ex.Message);
                                result = false;
                        }
                        return result;
                }

	        /// <summary>
                /// GetGroupsiFolderLimitPolicy gets you the user groups iFolder Limit policy.
                /// </summary>
                public int GroupsiFolderLimit(string DomainID, string UserID)
                {
			long Limit = -1;
                        Store store = Store.GetStore();
                        Domain domain = store.GetDomain(DomainID);
			Member member = null; 
                        string[] GIDs = domain.GetMemberFamilyList(UserID);
                        foreach(string gid in GIDs)
                        {
                                if(gid != UserID)
                                {
					member = domain.GetMemberByID(gid);
                                        Limit = Simias.Policy.iFolderLimit.Get(member).Limit;
                                        if(Limit != -1 && Limit != -2)
						break;
                                }
                        }
                        return (int)Limit;
                }

                /// <summary>
                /// GetGroupsSharingPolicy gets you the user groups sharing policy.
                /// </summary>
                public int GetGroupsSharingPolicy(string DomainID, string UserID)
                {
                        int Status = 0;
                        try
                        {
                                HostNode host = this.HomeServer; //home server

                                SimiasConnection smConn = new SimiasConnection(DomainID,
                                                                                        UserID,
                                                                                        SimiasConnection.AuthType.BASIC,
                                                                                        host);
                                SimiasWebService svc = new SimiasWebService();
                                svc.Url = host.PublicUrl;

                                smConn.Authenticate ();
                                smConn.InitializeWebClient(svc, "Simias.asmx");

                                Status = svc.GetGroupsSharingPolicy(DomainID, UserID);
                        }
                        catch(Exception ex)
                        {
                                log.Debug("GetGroupsSharingPolicy Exception : {0}", ex.Message);
                                Status = 0;
                        }
                        return Status;
                }

		/// <summary>
                /// Is Transfer Allowed gets you the user iFolder Limit policy - transfer of Ownership.
                /// </summary>
                public bool IsTransferAllowed(string DomainID, string UserID)
                {
                       int count = 0;
                       long SysPolicy=0, UserPolicy=0;
                       bool result = true;
                       try
                       {
                                Simias.Storage.Store store = Simias.Storage.Store.GetStore();
                                Simias.Storage.Domain domain = store.GetDomain(DomainID);
                                Simias.Storage.Member member = domain.GetMemberByID(UserID);

                                ICSList ifList = store.GetCollectionsByUser(this.ID);
                                foreach ( ShallowNode sn in ifList )
                                {
                                       Collection c = new Collection( store, sn );
                                       if ( c.IsType( "iFolder" ) )
                                               if ( c.Owner.UserID == this.ID )
                                                       ++count;
                                }

				log.Debug(count.ToString());
                                UserPolicy = Simias.Policy.iFolderLimit.Get( member ).Limit;

                                if (UserPolicy != -1 && UserPolicy != -2)
                                {
					if (UserPolicy <= count)
					{
                                        	result = false;
					}
                                }
                                else
                                {
                                        int GroupCount  = member.GroupsiFolderLimit(domain.ID, UserID);
                                        if( GroupCount != -1  )
                                                if( GroupCount <= count )
						{
                                                        result = false;
						}
                                                else
                                                        return result;
                                        SysPolicy = Simias.Policy.iFolderLimit.GetLimit(store.DefaultDomain);
                                        if (SysPolicy <= count && SysPolicy != -1)
					{
                	       			result = false;
					}
                               }
                               return result;
                        }
                        catch( Exception e )
                        {
                                Console.WriteLine(e);
                                throw(e);
                        }

                }

	        /// <summary>
                /// GetGroupsiFolderLimitPolicy gets you the user groups sharing policy.
                /// </summary>
                public int GroupsSharingPolicy(string DomainID, string UserID)
                {

			int groupSharing = 0;
                        int retStatus = 0;
			Member member = null; 
			int EnforcedSharing = 4;

                        Store store = Store.GetStore();
                        Domain domain = store.GetDomain(DomainID);
                        string[] GIDs = domain.GetMemberFamilyList(UserID);
                        foreach(string gid in GIDs)
                        {
                                if(gid != UserID)
                                {
					member = domain.GetMemberByID(gid);
                                        groupSharing = Simias.Policy.Sharing.GetStatus(member);
					if((groupSharing & EnforcedSharing) == EnforcedSharing)
                        			return groupSharing;
                                        if(groupSharing  >= 0 && retStatus == 0)
						retStatus = groupSharing;
                                }
                        }
                        log.Debug("GetGroupsSharingPolicy value returned for user {0}, of group {1} is {2} ",UserID, member.UserID, groupSharing );
                        return retStatus;

                }

                /// <summary>
                /// GetGroupSecurityPolicy gets you the user groups Encryption Policy .
                /// </summary>
                public int GetGroupSecurityPolicy(string DomainID, string UserID)
                {
                        int Status = 0;
                        try
                        {
                                HostNode host = this.HomeServer; //home server

                                SimiasConnection smConn = new SimiasConnection(DomainID,
                                                                                        UserID,
                                                                                        SimiasConnection.AuthType.BASIC,
                                                                                        host);
                                SimiasWebService svc = new SimiasWebService();
                                svc.Url = host.PublicUrl;

                                smConn.Authenticate ();
                                smConn.InitializeWebClient(svc, "Simias.asmx");

                                Status = svc.GetGroupsSecurityPolicy(DomainID, UserID);
                        }
                        catch(Exception ex)
                        {
                                log.Debug("GetGroupsSecurityPolicy Exception : {0}", ex.Message);
                                Status = 0;
                        }
                        return Status;
                }

	        /// <summary>
                /// GroupsSecurityPolicy gets you the user groups Encryption policy.
                /// </summary>
                public int GroupsSecurityPolicy(string DomainID, string UserID)
                {

                        int groupEncryption = 0;
			Member member = null; 

                        Store store = Store.GetStore();
                        Domain domain = store.GetDomain(DomainID);
                        string[] GIDs = domain.GetMemberFamilyList(UserID);
                        foreach(string gid in GIDs)
                        {
                                if(gid != UserID)
                                {
					member = domain.GetMemberByID(gid);
                                        groupEncryption = Simias.Policy.SecurityState.GetStatus(member);
                                        if(groupEncryption > 0 )
                                                break;
                                }
                        }
                        return groupEncryption;
                }

                /// <summary>
                /// GetEffectiveSyncPolicy gets you the essective sync interval Policy .
                /// </summary>
                public int GetEffectiveSyncPolicy(string DomainID, string UserID, String CollectionID)
                {
                        int Interval = 0;
                        try
                        {
                                HostNode host = this.HomeServer; //home server

                                SimiasConnection smConn = new SimiasConnection(DomainID,
                                                                                        UserID,
                                                                                        SimiasConnection.AuthType.BASIC,
                                                                                        host);
                                SimiasWebService svc = new SimiasWebService();
                                svc.Url = host.PublicUrl;

                                smConn.Authenticate ();
                                smConn.InitializeWebClient(svc, "Simias.asmx");

                                Interval = svc.GetEffectiveSyncPolicy(DomainID, UserID, CollectionID);
                        }
                        catch(Exception ex)
                        {
                                log.Debug("GetEffectiveSyncPolicy Exception : {0}", ex.Message);
                                Interval = 0;
                        }
                        return Interval;
                }


                /// <summary>
                /// EffectiveSyncPolicy gets you the essective sync interval Policy .
                /// </summary>
                public int EffectiveSyncPolicy(Collection collection)
                {

                        int Interval = 0;

                        try
                        {
                            Simias.Policy.SyncInterval interval = Simias.Policy.SyncInterval.Get(collection);
                            Interval = interval.Interval;
                        }
                        catch
                        {
                                Interval = 0;
                        }
                        return Interval;
                }


		/// <summary>
		/// Validate the passphrase
		/// </summary>
		public Simias.Authentication.StatusCodes ValidatePassPhrase(string Passphrase)
		{
			 log.Debug("Enter ValidatePassPhrase for validating passphrase :{0}", Passphrase);
			string OldHash = null;
			string NewHash = null;

			try
			{
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = this.HomeServer; //home server

				SimiasConnection smConn = new SimiasConnection(DomainID,
															UserID,
															SimiasConnection.AuthType.BASIC,
															host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;

				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");			

				//Hash the passphrase and use it for encryption and decryption
				PassphraseHash hash = new PassphraseHash();
				byte[] passphrase = hash.HashPassPhrase(Passphrase);	

				string EncrypCryptoKey = svc.ServerGetEncrypPassKey(DomainID, UserID);

				//Decrypt it
				string DecryptedCryptoKey; 
				Key DeKey = new Key(EncrypCryptoKey);
				DeKey.DecrypytKey(passphrase, out DecryptedCryptoKey);

				//Encrypt using passphrase
				string EncryptedCryptoKey;
				Key EnKey = new Key(DecryptedCryptoKey);
				EnKey.EncrypytKey(passphrase, out EncryptedCryptoKey);

				//SHA1
				Key HashKey = new Key(EncryptedCryptoKey);
				NewHash = HashKey.HashKey();

				OldHash = svc.ServerGetPassKeyHash(DomainID, UserID);
			}
			catch(Exception ex)
			{
				log.Debug("ValidatePassPhrase : {0} and return value is :{1}", ex.Message, Simias.Authentication.StatusCodes.PassPhraseInvalid.ToString());
                 		return Simias.Authentication.StatusCodes.PassPhraseInvalid;
                                //throw ex;

			}
			
			//Compare
			log.Debug("ValidatePassPhrase : Comparing blobs {0}...{1}",OldHash, NewHash);
			if(String.Equals(OldHash, NewHash)==true)
			{
				log.Debug("ValidatePassPhrase : true");
				return Simias.Authentication.StatusCodes.Success;
			}
			else	
			{
				log.Debug("ValidatePassPhrase : false");			
				return Simias.Authentication.StatusCodes.PassPhraseInvalid;
			}
		}

		/// <summary>
		/// Hash the passphrase
		/// </summary>
		public byte[] HashPassPhrase(string Passphrase)
		{
			byte[] salt={0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8};
			UTF8Encoding utf8 = new UTF8Encoding();
			byte[] data = utf8.GetBytes(Passphrase);
			HMACSHA1 sha1= new HMACSHA1();
			sha1.Key = salt;
			for(int i=0; i<1000; i++)
			{
				sha1.ComputeHash(data);					
				data = sha1.Hash;
			}			
			byte[] NewPassphrase = new byte[data.Length+4]; //20+4
			Array.Copy(data, 0, NewPassphrase, 0,data.Length);
			Array.Copy(data, 0, NewPassphrase, 20, 4);
			log.Debug("HashPassPhrase passphrase :{0}....:length{1}", utf8.GetString(NewPassphrase), NewPassphrase.Length);
			return NewPassphrase;
		}
		
		/// <summary>
		/// Validate the passphrase
		/// </summary>
		public bool IsPassPhraseSet()
		{
			string  CryptoKeyBlob = null;
			try
			{
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = this.HomeServer; //home server
				
				SimiasConnection smConn = new SimiasConnection(DomainID,
															UserID,
															SimiasConnection.AuthType.BASIC,
															host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;

				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");
				CryptoKeyBlob =  svc.ServerGetPassKeyHash(DomainID, UserID);
			}
			catch(Exception ex)
			{
				log.Debug("IsPassPhraseSet : {0}", ex.Message);
				throw ex;
			}
			log.Debug("IsPassPhraseSet :{0}", CryptoKeyBlob);
			if(CryptoKeyBlob == String.Empty)
			{
				log.Debug("IsPassPhraseSet : false");
				return false;
			}
			else	
			{
				log.Debug("IsPassPhraseSet : true");
				return true;
			}
		}

		/// <summary>
		/// Export the crypto keys of user
		/// </summary>
		public  void ExportiFoldersCryptoKeys(out XmlDocument keyDocument, string FilePath)
		{
			XmlDocument  document = new XmlDocument();
			XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0","utf-8",null); 
			document.InsertBefore(xmlDeclaration, document.DocumentElement); 
			XmlElement title  = document.CreateElement("CryptoKeyRecovery");
			document.AppendChild(title);

			try
			{
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = this.HomeServer; //home server
				SimiasConnection smConn = new SimiasConnection(DomainID,
										UserID,
										SimiasConnection.AuthType.BASIC,
										host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;

				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");

				int index = 0;
				CollectionKey Key = null;
				while((Key = svc.GetiFolderCryptoKeys(DomainID, UserID, index)) != null)
				{
					XmlNode newElem1 = document.CreateNode("element", "iFolderCollection", "");
       				newElem1.InnerText = "";
					document.DocumentElement.AppendChild(newElem1);

					XmlNode newElem2 = document.CreateNode("element", "iFolderID", "");
       				newElem2.InnerText = Key.NodeID;
log.Debug("iFolderID {0}", Key.NodeID);
					newElem1.AppendChild(newElem2);
					
					XmlNode newElem3 = document.CreateNode("element", "Key", "");
       				newElem3.InnerText = Key.REDEK;
log.Debug("REDEK {0}", Key.REDEK);
				       newElem1.AppendChild(newElem3);
					index++;
				}
				if(FilePath != null )
				{
                    if(File.Exists(FilePath))
					    File.Delete(FilePath);
                   document.Save(FilePath);
                }

				keyDocument = document;
			}
			catch(Exception ex)
			{
				log.Debug("ExportiFoldersCryptoKeys : {0}", ex.Message);
				throw ex;
			}
			finally{}
		}

		/// <summary>
		/// Export the crypto keys from server
		/// </summary>
		public  void ExportiFoldersCryptoKeys(string FilePath)
		{
			XmlDocument keyDoc = null;
			if(FilePath != null)
				ExportiFoldersCryptoKeys(out keyDoc, FilePath);
		}

		/// <summary>
		/// Import the crypto keys from server
		/// </summary>
		public void ImportiFoldersCryptoKeys(XmlDocument keyDocument, string NewPassphrase, string OneTimePassphrase, bool usingFile, string FilePath)
		{
		//if usingFile is false, then keyDocument must not be null and must be validated by the caller
			if(usingFile && !File.Exists(FilePath))
				throw new CollectionStoreException("File not found"); //will be caught by the caller					

			string strKey = string.Format("//{0}/{1}", "iFolderCollection", "Key");
			string strID = string.Format("//{0}/{1}", "iFolderCollection", "iFolderID");
			XmlDocument encFile;
			
			if(usingFile)
			{
				encFile = new XmlDocument();
				encFile.Load(FilePath);
			}
			else
				encFile = keyDocument;
			
			XmlNodeList keyNodeList, idNodeList;
			XmlElement root = encFile.DocumentElement;
			
			keyNodeList = root.SelectNodes(strKey);
			idNodeList = root.SelectNodes(strID);
			
			try
			{
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = this.HomeServer; //home server
				SimiasConnection smConn = new SimiasConnection(DomainID,
										UserID,
										SimiasConnection.AuthType.BASIC,
										host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;

				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");

				CollectionKey cKey = new CollectionKey();
				int count = 0;
				foreach (XmlNode idNode in idNodeList)
				{
					log.Debug("RECOVERY: Parsing Element :{0}", count);
					PassphraseHash hash = new PassphraseHash();
					
					log.Debug("length of KeyNodeList is :{0}", keyNodeList.Count.ToString());

					XmlNode keyNode = keyNodeList[count++];					
		                    	string RecoveredCryptoKey = null;
                		    	if (keyNode != null)
                        			RecoveredCryptoKey = keyNode.InnerText;
                    			else
                    			{
                        			log.Debug("keyNode is null");
                        			continue;
                    			}
                    			log.Debug("RecoveredCryptoKey {0}",RecoveredCryptoKey);

					string DecrypRecoveredCryptoKey = null;
					if(OneTimePassphrase !=null && OneTimePassphrase !="")
					{					
						byte[] Passphrase = hash.HashPassPhrase(OneTimePassphrase);	
						Key DeKey = new Key(RecoveredCryptoKey);
						DeKey.DecrypytKey(Passphrase, out DecrypRecoveredCryptoKey);
					}
					else
						DecrypRecoveredCryptoKey = RecoveredCryptoKey;

					//Verify the recovered key matches with the original key
					Key HashKey = new Key(DecrypRecoveredCryptoKey);	
					string serverHash = svc.ServerGetCollectionHashKey(idNode.InnerText);
                    			if (serverHash == null)
                    			{
                        			log.Debug("The specified cryptographic key does not found in server");
                        			//throw new CollectionStoreException("The specified cryptographic key does not found in server");

                    			}


                    			if (HashKey.HashKey() != serverHash)
                    			{
                        			//throw new CollectionStoreException("The recovered cryptographic key does not match");
                        			log.Debug("The recovered cryptographic key does not match");

			                }
										

					log.Debug("RECOVERY: The recovery key macth with the server key");				
					
					//Encrypted the recovered key using the new passphrase
					byte[] passphrase = hash.HashPassPhrase(NewPassphrase);	
					string EncryptedKey = null;
					Key EnKey = new Key(DecrypRecoveredCryptoKey);
						EnKey.EncrypytKey(passphrase, out EncryptedKey);
					
					cKey.PEDEK = EncryptedKey;
					cKey.NodeID =  idNode.InnerText;
					cKey.REDEK =  null;
					
					if(svc.SetiFolderCryptoKeys(DomainID, UserID, cKey)==false)
					{
						log.Debug("ImportiFoldersCryptoKeys failed in SetiFolderCryptoKeys:{0}", cKey.NodeID);
						log.Debug("The specified cryptographic key does not found");
                                               //throw new CollectionStoreException("The specified cryptographic key does not found");

					}
				}		
				SetPassPhrase(NewPassphrase, null, null);				
			}
			catch(Exception ex)
			{
				log.Debug("ImportiFoldersCryptoKeys : {0}", ex.Message);
				throw ex;
			}
			finally{}
		}
		
		/// <summary>
		/// Import the crypto keys from server
		/// </summary>
		public void ImportiFoldersCryptoKeys(string FilePath, string NewPassphrase, string OneTimePassword)
		{
			if(FilePath != null)
				ImportiFoldersCryptoKeys(null, NewPassphrase, OneTimePassword, true, FilePath);
		}

		public void RecoverKeys(string RAName, bool isRSA, XmlDocument keyDocument, string oneTimePP, out XmlDocument decryptedKeyDoc)
		{
			string titleTag = "CryptoKeyRecovery";
			string CollectionIDTag = "iFolderCollection";
			string iFolderIDTag = "iFolderID";
			string KeyTag = "Key";
			string strKey = string.Format("//{0}/{1}", CollectionIDTag, KeyTag);
			string strID = string.Format("//{0}/{1}", CollectionIDTag, iFolderIDTag);
			string decKey;
			byte[] decKeyByteArray;
			RecoveryAgent agent = null;
			XmlDocument document = new XmlDocument();
			try
			{
			    XmlDocument encFile = keyDocument;
			    XmlNodeList keyNodeList, idNodeList;

			    XmlElement root = encFile.DocumentElement;

			    keyNodeList = root.SelectNodes(strKey);
			    idNodeList = root.SelectNodes(strID);

			    XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
			    document.InsertBefore(xmlDeclaration, document.DocumentElement);
			    XmlElement title = document.CreateElement(titleTag);
			    document.AppendChild(title);
			    int i = 0;
			    foreach (XmlNode idNode in idNodeList)
			    {
			        if (idNode.InnerText == null || idNode.InnerText == String.Empty)
			            continue;
			        log.Debug("ID {0}",idNode.InnerText);
			        XmlNode newNode = document.CreateNode("element", CollectionIDTag, "");
			        newNode.InnerText = "";
			        document.DocumentElement.AppendChild(newNode);
			        XmlNode innerNode = document.CreateNode("element", iFolderIDTag, "");
			        innerNode.InnerText = idNode.InnerText;
			        newNode.AppendChild(innerNode);
			        {
			            XmlNode keyNode = keyNodeList[i++];
					decKey = keyNode.InnerText;
					log.Debug("DecKey {0}", decKey);
			            decKeyByteArray = Convert.FromBase64String(decKey);
			            XmlNode newElem2 = document.CreateNode("element", KeyTag, "");
			            if (decKey == null || decKey == String.Empty)
			                continue;
					if(isRSA)
					{
						UTF8Encoding utf8 = new UTF8Encoding();
						RSACryptoServiceProvider raRSA = new RSACryptoServiceProvider();
						string xmlStr = utf8.GetString(Convert.FromBase64String(this.GetDefaultRSAFromServer()));
						raRSA.FromXmlString(xmlStr);
						agent = new RecoveryAgent(raRSA);
					}
			            if (oneTimePP != null && oneTimePP != String.Empty)
			                newElem2.InnerText = agent.DecodeMessage(decKeyByteArray, oneTimePP);
			            else
			                newElem2.InnerText = agent.DecodeMessage(decKeyByteArray);
			            newNode.AppendChild(newElem2);
			        }
			    }
			}
			catch (Exception e)
			{
			    Console.WriteLine("Exception while processing" + e.Message + e.StackTrace);
			}
			decryptedKeyDoc = document;
		}
                /// <summary>
               /// Gets the credentials from the specified domain object.
               /// </summary>
               /// <param name="DomainID">The ID of the domain to set the credentials on.</param>
               /// <returns>The Default public key </returns>
                public string  GetDefaultRSAFromServer()
                {

                        Store store = Store.GetStore();
                        string DomainID = this.GetDomainID(store);

                        HostNode host = this.HomeServer; //home server
                log.Debug("DomainID {0}, User ID {1}", DomainID, UserID);
                        SimiasConnection smConn = new SimiasConnection(DomainID,
                                                              this.UserID,
                                                              SimiasConnection.AuthType.BASIC,
                                                              host);
                        SimiasWebService svc = new SimiasWebService();
                        svc.Url = host.PublicUrl;

                        smConn.Authenticate ();
                        smConn.InitializeWebClient(svc, "Simias.asmx");
                        return svc.GetDefaultRSAKey(DomainID);
                }

		/// <summary>
	       /// Gets the credentials from the specified domain object.
	       /// </summary>
	       /// <param name="DomainID">The ID of the domain to set the credentials on.</param>
	       /// <returns>The Default public key </returns>
	        public string GetDefaultPublicKeyFromServer()
	        {

			Store store = Store.GetStore();
			string DomainID = this.GetDomainID(store);
			
			HostNode host = this.HomeServer; //home server
		log.Debug("DomainID {0}, User ID {1}", DomainID, UserID);	
			SimiasConnection smConn = new SimiasConnection(DomainID,
									this.UserID,
									SimiasConnection.AuthType.BASIC,
									host);
			SimiasWebService svc = new SimiasWebService();
			svc.Url = host.PublicUrl;
			
			smConn.Authenticate ();
			smConn.InitializeWebClient(svc, "Simias.asmx");
			
			return svc.GetDefaultPublicKey(DomainID);
	        }

	

                /// <summary>
               /// Gets the credentials from the specified domain object.
               /// </summary>
               /// <param name="DomainID">The ID of the domain to set the credentials on.</param>
               /// <returns>The Default public key </returns>
                public string GetDefaultRSAKey()
                {
//	log.Debug("GetDefaultRSAKey \n {0}", Simias.Security.RSAStore.Default_RA.ToXmlString(true));
	/// FIXME - This is an insecure way of sending the key pair. This has to be obfuscated. - BUGBUG
			UTF8Encoding utf8 = new UTF8Encoding();
                        return Convert.ToBase64String(utf8.GetBytes(Simias.Security.RSAStore.Default_RA.ToXmlString(true)));
                }
	
		/// <summary>
	       /// Gets the credentials from the specified domain object.
	       /// </summary>
	       /// <param name="DomainID">The ID of the domain to set the credentials on.</param>
	       /// <returns>The Default public key </returns>
	        public string GetDefaultPublicKey()
	        {
	log.Debug("GetDefaultPublicKey");
	/// FIXME - Entire Public Parameters have to be sent. Currently the Exponent is assumed and hardcoded- BUGBUG
	/// use the ToXmlString(false) - see CryptoKey.cs
			string pKey = "SimiasRSA"+Convert.ToBase64String((Simias.Security.RSAStore.Default_RA.ExportParameters(false)).Modulus);
			return pKey;
	        }
		#endregion
	}	
	
	/// <summary>
	/// Hash the passphrase
	/// </summary>
	public class PassphraseHash
	{
		public PassphraseHash()
		{
		}
		public byte[] HashPassPhrase(string Passphrase)
		{
			/*change to PasswordDeriveBytes.CryptDeriveKey once the  implementation is done mono

			PasswordDeriveBytes pdb = new PasswordDeriveBytes(Passphrase, salt);
			TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
			tdes.Key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, tdes.IV);
			//tdes.Key is the NewPassphrase
			
			*/
			byte[] salt={0x49, 0x46, 0x4F, 0x4C, 0x44, 0x45, 0x52};
			UTF8Encoding utf8 = new UTF8Encoding();
			byte[] data = utf8.GetBytes(Passphrase);
			HMACSHA1 sha1= new HMACSHA1();
			sha1.Key = salt;
			for(int i=0; i<1000; i++)
			{
				sha1.ComputeHash(data);					
				data = sha1.Hash;
			}			
			byte[] NewPassphrase = new byte[data.Length+4]; //20+4
			Array.Copy(data, 0, NewPassphrase, 0,data.Length);
			Array.Copy(data, 0, NewPassphrase, 20, 4);
			return NewPassphrase;
		}
	}
}
