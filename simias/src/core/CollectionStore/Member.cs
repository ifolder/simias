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
		/// Gets the encryption blob
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
		/// Validate the passphrase for the correctness
		/// </summary>
		public Simias.Authentication.Status ValidatePassPhrase(string passPhrase)
		{
			string oldBlob = this.EncryptionBlob;

			if(oldBlob ==null)
			{
				//log.Info( "IsPassPhraseSet : FALSE" );
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.PassPhraseNotSet);
			}
			else
			{
				//log.Debug( "SetPassPhrase  validate the passphrase: " );

				//Decrypt the node properties (key, algorithm) using the passphrase provided by the user
				string encryptedkey = this.EncryptionKey;
				
				TripleDESCryptoServiceProvider tDesKey = new TripleDESCryptoServiceProvider();
				UTF8Encoding utf8 = new UTF8Encoding();

				byte[] IV = new byte[0];
				byte[] Buffer = utf8.GetBytes(encryptedkey);
				Stream Outstream  = new MemoryStream(Buffer) as Stream;
				tDesKey.KeySize = 128;
				tDesKey.Key = utf8.GetBytes(passPhrase);					
				CryptoStream cStream = new CryptoStream(Outstream, tDesKey.CreateDecryptor(utf8.GetBytes(passPhrase), IV), CryptoStreamMode.Read);
				StreamReader eStream = new StreamReader(cStream);
				string Decryptedkey = eStream.ReadLine();//decrypt the date
				eStream.Close();
				cStream.Close();
				Outstream.Close();	

				//log.Debug( "Decrypted Key : {0}", Decryptedkey);

				//Retrieve the old unencrypted blob (already done see above)
				
				//Create the new blob using the decrypted key and algorithm
				PassPhrase  newBlob = new PassPhrase(Decryptedkey,"blowFish");
				
				//validate the blobs
				if(String.Equals(newBlob.HashPassPhrase(), oldBlob)==true)
				{
					//log.Debug( "Validating the user entered passphrase: passphrase match succeded.........oldBlob :{0}.......NewBlob: {1}",oldBlob, newBlob.HashPassPhrase());
					return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.Success);			
				}
				else
				{
					//log.Debug( "Validating the user entered passphrase: passphrase match failed..........oldBlob :{0}.......NewBlob: {1}",oldBlob, newBlob.HashPassPhrase());
					return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.PassPhraseInvalid);
				}
			}
		}		

		/// <summary>
		/// Validate the passphrase for the correctness
		/// </summary>
		public Simias.Authentication.Status SetPassPhrase(string passPhrase, string RAName, string PublicKey)
		{
			string oldBlob = this.EncryptionBlob;

			//log.Debug( "SetPassPhrase  trying to get oldBlob is : NULL, setting the passphrase" );

			this.RAName = RAName;
			this.RAPublicKey = PublicKey;

			//Get the random key
			TripleDESCryptoServiceProvider tDesKey = new TripleDESCryptoServiceProvider();
			//tDesKey.KeySize();
			UTF8Encoding utf8 = new UTF8Encoding();
			string encryptionKey = utf8.GetString(tDesKey.Key);

			// Encrypt the key, algorithm here using the passphrase
			byte[] IV = new byte[0];
			byte[] Buffer = new byte[1000];
			Stream Outstream  = new MemoryStream(Buffer) as Stream;
			tDesKey.KeySize = 128;
			tDesKey.Key = utf8.GetBytes(passPhrase);
			CryptoStream cStream = new CryptoStream(Outstream, tDesKey.CreateEncryptor(utf8.GetBytes(passPhrase), IV), CryptoStreamMode.Write);
			StreamWriter eStream = new StreamWriter(cStream);
			eStream.WriteLine(encryptionKey);//encrypt the date
			byte[] Encryptedkey = new byte[Outstream.Length];
			Outstream.Read(Encryptedkey, 0, encryptionKey.Length);//read the encrypted date
			eStream.Close();
			cStream.Close();
			Outstream.Close();	
			//log.Debug( "TO be Encrypted Key : {0}", encryptionKey);

			//Add the encrypted key, algorithm type as a user node property
			this.EncryptionKey = utf8.GetString(Encryptedkey);
			this.EncryptionType = "blowFish";

			//Using the unencrypted key, algorithm type and create the blob and add as a property
			PassPhrase blob = new PassPhrase(passPhrase,"blowFish");
			this.EncryptionBlob = blob.HashPassPhrase();

			//og.Debug( "SetPassPhrase  got set congratulations.............................{0}",member.EncryptionBlob);
			return new Simias.Authentication.Status  (Simias.Authentication.StatusCodes.Success); 
		}
		#endregion
	}
}
