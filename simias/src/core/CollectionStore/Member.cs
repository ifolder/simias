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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Xml;

using Simias.Client;
using Simias.Storage;
using Simias.CryptoKey;


namespace Simias.Storage
{
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

		/// </summary>
		private string encryptionKey;

		/// <summary>

		/// </summary>
		private string encryptionType;

		/// <summary>

		/// </summary>
		private string encryptionBlob;

		/// <summary>

		/// </summary>
		private string raName;

		/// <summary>

		/// </summary>
		private string raPublicKey;

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
				properties.AddNodeProperty(PropertyTags.EncryptionKey, value);
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
				properties.AddNodeProperty(PropertyTags.EncryptionBlob, value);
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
				properties.AddNodeProperty(PropertyTags.RAName, value);
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
				properties.AddNodeProperty(PropertyTags.RAPublicKey, value);
			}
		}

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
					properties.ModifyNodeProperty( PropertyTags.Given, value ); 
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
					properties.ModifyNodeProperty( PropertyTags.Family, value ); 
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
		}

		/// <summary>
		/// Constructor for creating a new Member object.
		/// </summary>
		/// <param name="userName">User name of the member.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		/// <param name="rights">Collection access rights granted to the user.</param>
		/// <param name="givenName">Given (first) name of the contact</param>
		/// <param name="familyName">Family (last) name of the contact</param>
		public Member( string userName, string userGuid, Access.Rights rights, string givenName, string familyName ) :
			this ( userName, userGuid, rights, null )
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
		public void ServerSetPassPhrase(string EncryptedCryptoKey, string CryptoKeyBlob, string RAName, string PublicKey)
		{
			Store store = Store.GetStore();
			string DomainID = this.GetDomainID(store);
			string UserID = store.GetUserIDFromDomainID(DomainID);

			Domain domain = store.GetDomain(GetDomainID(store));	
			Member member = domain.GetMemberByID(UserID);
			
			log.Debug("ServerSetPassPhrase user:{0}...userID={1}",member.Name, UserID);
			log.Debug("ServerSetPassPhrase Commit {0}...{1}...{2}...{3}",EncryptedCryptoKey, CryptoKeyBlob,RAName, PublicKey);
			
			if(EncryptedCryptoKey !=null)
			{
				//member.properties.AddNodeProperty(PropertyTags.EncryptionKey, EncryptedCryptoKey);
				Property p = new Property(PropertyTags.EncryptionKey, EncryptedCryptoKey);
				this.properties.ModifyNodeProperty(p);
			}
			if(CryptoKeyBlob !=null)
			{
				//member.properties.AddNodeProperty(PropertyTags.EncryptionBlob, CryptoKeyBlob);
				Property p = new Property(PropertyTags.EncryptionBlob, CryptoKeyBlob);
				this.properties.ModifyNodeProperty(p);
			}
			if(RAName !=null)
			{
				//member.properties.AddNodeProperty(PropertyTags.RAName, RAName);
				Property p = new Property(PropertyTags.RAName, RAName);
				this.properties.ModifyNodeProperty(p);
			}
			if(PublicKey !=null)
			{
				//member.properties.AddNodeProperty(PropertyTags.RAPublicKey, PublicKey);
				Property p = new Property(PropertyTags.RAPublicKey, PublicKey);
				this.properties.ModifyNodeProperty(p);
			}
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
		/// Set the passphrase(key encrypted by passphrase and SHA1 of key) and recovery agent name and key
		/// </summary>
		public void SetPassPhrase(string Passphrase, string RAName, string PublicKey)
		{
			try
			{
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = this.HomeServer; //home server

				log.Debug("SetPassPhrase member entry");
				
				SimiasConnection smConn = new SimiasConnection(DomainID,
															UserID,
															SimiasConnection.AuthType.BASIC,
															host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;

				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");

				Key key = new Key((Passphrase.Length)*8);//create the key 
				string EncrypCryptoKey;
				key.EncrypytKey(Passphrase, out EncrypCryptoKey); //encrypt the key
				Key HashKey = new Key(EncrypCryptoKey);
				
				log.Debug("SetPassPhrase {0}...{1}...{2}...{3}",EncrypCryptoKey, HashKey.HashKey(), RAName, PublicKey);
				svc.ServerSetPassPhrase(DomainID, UserID, EncrypCryptoKey, HashKey.HashKey(), RAName, PublicKey);
			}
			catch(Exception ex)
			{
				log.Debug("SetPassPhrase : {0}", ex.Message);
				throw ex;
			}
		}

		/// <summary>
		/// Set the passphrase(key encrypted by passphrase and SHA1 of key) and recovery agent name and key
		/// </summary>
		public bool ReSetPassPhrase(string OldPassphrase, string Passphrase, string RAName, string PublicKey)
		{
			if(ValidatePassPhrase(OldPassphrase) != Simias.Authentication.StatusCodes.Success)
				return false;
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

				Key key = new Key(128);
				string EncrypCryptoKey = null;
				key.EncrypytKey(Passphrase, out EncrypCryptoKey);			
				Key HashKey = new Key(EncrypCryptoKey);
				
				svc.ServerSetPassPhrase(DomainID, UserID, EncrypCryptoKey, HashKey.HashKey(), RAName, PublicKey);

				CollectionKey OldKey = null;
				CollectionKey NewKey = new CollectionKey();
				int index = 0;
				string DecryptedKey = null;
				string EncryptedKey = null;
				
				while((OldKey = svc.GetiFolderCryptoKeys(DomainID, UserID, index)) != null)
				{
					//Decrypt and encrypt the key
					Simias.Storage.Key DeKey = new Key(OldKey.PEDEK);	
					DeKey.DecrypytKey(OldPassphrase, out DecryptedKey);
					Simias.Storage.Key EnKey = new Key(DecryptedKey);
					EnKey.EncrypytKey(Passphrase, out EncryptedKey);

					//Send back to server					
					NewKey.NodeID = OldKey.NodeID;
					NewKey.PEDEK = EncryptedKey;
					NewKey.REDEK = OldKey.REDEK;
					if(svc.SetiFolderCryptoKeys(DomainID, UserID, NewKey)==false)
					{
						log.Debug("ReSetPassPhrase : failed for ifolder ID:", NewKey.NodeID);
						throw new CollectionStoreException("The specified cryptographic key not found");
					}
					index++;
				}
			}
			catch(Exception ex)
			{
				log.Debug("ReSetPassPhrase : {0}", ex.Message);
				throw ex;
			}
			return true;
		}

		
		/// <summary>
		/// Validate the passphrase
		/// </summary>
		public Simias.Authentication.StatusCodes ValidatePassPhrase(string PassPhrase)
		{
			string OldHash = null;
			string NewHash = null;
			
			try
			{
				Store store = Store.GetStore();
				string DomainID = this.GetDomainID(store);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				HostNode host = this.HomeServer; //home server

				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				Simias.Storage.Member member = domain.GetCurrentMember();
				log.Debug("Member ValidatePassPhrase User:{0}...{1}...{2} ", member.Name, member.UserID, UserID);
				
				SimiasConnection smConn = new SimiasConnection(DomainID,
															UserID,
															SimiasConnection.AuthType.BASIC,
															host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;

				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");			

				log.Debug("ValidatePassPhrase : got PassKey");
				string EncrypCryptoKey = svc.ServerGetEncrypPassKey(DomainID, UserID);
				log.Debug("ValidatePassPhrase : got PassKey:{0}",EncrypCryptoKey);

				//Decrypt it
				string DecryptedCryptoKey; 
				Key DeKey = new Key(EncrypCryptoKey);
				DeKey.DecrypytKey(PassPhrase, out DecryptedCryptoKey);

				//Encrypt using passphrase
				string EncryptedCryptoKey;
				Key EnKey = new Key(DecryptedCryptoKey);
				EnKey.EncrypytKey(PassPhrase, out EncryptedCryptoKey);

				//SHA1
				Key HashKey = new Key(EncryptedCryptoKey);
				NewHash = HashKey.HashKey();

				OldHash = svc.ServerGetPassKeyHash(DomainID, UserID);
				log.Debug("ValidatePassPhrase : getting OldHash:{0}", OldHash);
			}
			catch(Exception ex)
			{
				log.Debug("ValidatePassPhrase : {0}", ex.Message);
				throw ex;
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
		/// Export the crypto keys from server
		/// </summary>
		public  void ExportiFoldersCryptoKeys(string FilePath)
		{
			if(File.Exists(FilePath))
				File.Delete(FilePath);
			StreamWriter ExpStream = File.CreateText(FilePath);
			
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
					ExpStream.WriteLine("{0}:{1}", Key.NodeID, Key.REDEK);
					index++;
				}
			}
			catch(Exception ex)
			{
				log.Debug("ExportiFoldersCryptoKeys : {0}", ex.Message);
				throw ex;
			}
			finally
			{
				if(ExpStream !=null)
					ExpStream.Close();
			}
		}
		/// <summary>
		/// Import the crypto keys from server
		/// </summary>
		public void ImportiFoldersCryptoKeys(string OneTimePassphrase, string FilePath)
		{
			if(!File.Exists(FilePath))
				throw new CollectionStoreException("File not found"); //will be caught by the caller					
			StreamReader ImpStream = File.OpenText(FilePath);
			
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

				CollectionKey Key = new CollectionKey();
				string buffer = null;
				while((buffer = ImpStream.ReadLine())!=null)
				{
					string[] parts = buffer.Split(new char [] { ':' });
					Key.NodeID = parts[0];
					
					string EncrypCryptoKey = buffer.Remove(0, Key.NodeID.Length+1);//remove the : as well
					string DecryptedCryptoKey = null;
					if(OneTimePassphrase !=null)
					{					
						Key DeKey = new Key(EncrypCryptoKey);
						DeKey.DecrypytKey(OneTimePassphrase, out DecryptedCryptoKey);
					}
					else
						DecryptedCryptoKey = EncrypCryptoKey;
						
					Key.PEDEK = DecryptedCryptoKey;
					
					if(svc.SetiFolderCryptoKeys(DomainID, UserID, Key)==false)
					{
						log.Debug("ImportiFoldersCryptoKeys failed in SetiFolderCryptoKeys:", Key.NodeID);
						throw new CollectionStoreException("The specified cryptographic key not found");
					}
				}
			}
			catch(Exception ex)
			{
				log.Debug("ExportiFoldersCryptoKeys : {0}", ex.Message);
				throw ex;
			}
			finally
			{
				if(ImpStream !=null)
					ImpStream.Close();
			}
		}
		#endregion
	}
}
