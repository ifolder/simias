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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

using Simias.Client;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{	
	/// <summary>
	/// Defines the credential types stored on a domain.
	/// </summary>
	[Serializable]
	public enum CredentialType
	{
		/// <summary>
		/// Credentials have not been set on this domain.
		/// </summary>
		None,

		/// <summary>
		/// Credentials are not required for this domain.
		/// </summary>
		NotRequired,

		/// <summary>
		/// HTTP basic credentials.
		/// </summary>
		Basic,

		/// <summary>
		/// Public/Private key credentials.
		/// </summary>
		PPK
	}

	/// <summary>
	/// Class that represents a user identity in the Collection Store.
	/// </summary>
	public class Identity : Node
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Identity ) );

		/// <summary>
		/// This is used to keep from generating a new key set everytime a new RSACryptoSecurityProvider
		/// object is instantiated. This is passed as a parameter to the constructor and will initially
		/// use the dummy key set until the real key set is imported.
		/// </summary>
		static private CspParameters DummyParameters;

		/// <summary>
		/// Xml tags used to store the domain mapping information.
		/// </summary>
		static private readonly string MappingTag = "Mapping";
		static private readonly string DomainTag = "Domain";
		static private readonly string UserTag = "User";
		static private readonly string CredentialTag = "Credential";
		static private readonly string TypeTag = "Type";
		static private readonly string PassPhraseTag = "PassPhrase";
		static private readonly string PassPhraseTypeTag = "PassPhraseType";
		static private readonly string RememberPassPhraseTag = "RememberPassPhrase";

		/// <summary>
		/// Handle to the store.
		/// </summary>
		private Store store = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the store handle.
		/// </summary>
		private Store StoreReference
		{
			get
			{
				if ( store == null )
				{
					store = Store.GetStore();
				}

				return store;
			}
		}

		/// <summary>
		/// Gets the public/private key values for the local identity.
		/// </summary>
		public RSACryptoServiceProvider Credential
		{
			get
			{
				RSACryptoServiceProvider credential = null;

				// Lookup the credential property on the identity.
				XmlDocument mapDoc = GetDocumentByDomain( StoreReference.LocalDomain );
				if ( mapDoc != null )
				{
					credential = DummyCsp;
					credential.FromXmlString( mapDoc.DocumentElement.GetAttribute( CredentialTag ) );
				}

				return credential;
			}
		}

		/// <summary>
		/// Returns the number of subscribed domains.
		/// </summary>
		internal int DomainCount
		{
			get
			{
				MultiValuedList mvl = properties.GetProperties( PropertyTags.Domain );
				return mvl.Count;
			}
		}

		/// <summary>
		/// Gets the public key for the Identity object.
		/// </summary>
		public RSACryptoServiceProvider PublicKey
		{
			get
			{
				// Export the public key from the credential set.
				RSACryptoServiceProvider pk = null;
				RSACryptoServiceProvider credential = Credential;
				if ( credential != null )
				{
					pk = DummyCsp;
					pk.ImportParameters( credential.ExportParameters( false ) );
				}

				return pk;
			}
		}

		/// <summary>
		/// Gets the CSP for the dummy key container.
		/// </summary>
		static internal RSACryptoServiceProvider DummyCsp
		{
			get
			{
				RSACryptoServiceProvider csp = null;

				lock( DummyParameters )
				{
					try
					{
						csp = new RSACryptoServiceProvider( DummyParameters );
					}
					catch ( CryptographicException e )
					{
						log.Debug( e, "Corrupt cryptographic key container." );
#if WINDOWS
						IntPtr phProv = IntPtr.Zero;
						if ( CryptAcquireContext(
							ref phProv,
							DummyParameters.KeyContainerName,
							"Microsoft Strong Cryptographic Provider",
							1, // PROV_RSA_FULL
							0x10) ) // CRYPT_DELETEKEYSET
						{
							csp = new RSACryptoServiceProvider( DummyParameters );
						}
#endif
					}
				}

				return csp;
			}
		}
		#endregion

		#region Win32APIs
#if WINDOWS
		[System.Runtime.InteropServices.DllImport( "advapi32.dll", SetLastError=true )]
		static extern bool CryptAcquireContext( ref IntPtr phProv, string pszContainer, string pszProvider, uint dwProvType, uint dwFlags );
#endif
		#endregion

		#region Constructors
		/// <summary>
		/// Static constructor for the object.
		/// </summary>
		static Identity()
		{
			// Set up the dummy key store so that it will contain a dummy key set.
			DummyParameters = new CspParameters();
			DummyParameters.KeyContainerName = "DummyKeyStore";
		}

		/// <summary>
		/// Constructor for creating a new Identity object.
		/// </summary>
		/// <param name="store">A handle to the store.</param>
		/// <param name="userName">User name of the identity.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		internal Identity( Store store, string userName, string userGuid ) :
			base ( userName, userGuid, NodeTypes.IdentityType )
		{
			this.store = store;	
		}

		/// <summary>
		/// Constructor that creates an Identity object from a Node object.
		/// </summary>
		/// <param name="node">Node object to create the Identity object from.</param>
		internal Identity( Node node ) :
			base( node )
		{
			if ( type != NodeTypes.IdentityType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates an Identity object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the specified Node object belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Identity object from.</param>
		internal Identity( Collection collection, ShallowNode shallowNode ) :
			base( collection, shallowNode )
		{
			if ( type != NodeTypes.IdentityType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates an Identity object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create the Identity object from.</param>
		internal Identity( XmlDocument document ) :
			base( document )
		{
			if ( type != NodeTypes.IdentityType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Decrypts the credential.
		/// </summary>
		/// <param name="encryptedCredential">A string object that contain the encrypted credential.</param>
		/// <returns>A string object containing the clear credential.</returns>
		private string DecryptCredential( string encryptedCredential )
		{
			// Decrypt the byte array and convert it back into a string.
			byte[] buffer = Credential.Decrypt( Convert.FromBase64String( encryptedCredential ), false );
			return new UTF8Encoding().GetString( buffer );
		}

		/// <summary>
		/// Encrypts the credential.
		/// </summary>
		/// <param name="credential">Credential to encrypt.</param>
		/// <returns>A string object containing the encrypted credential.</returns>
		private string EncryptCredential( string credential )
		{
			// Convert the string to a byte array.
			UTF8Encoding encoding = new UTF8Encoding();
			int byteCount = encoding.GetByteCount( credential );
			byte[] buffer = new byte[ byteCount ];
			encoding.GetBytes( credential, 0, credential.Length, buffer, 0 );

			// Encrypt the byte array and turn it into a string.
			return Convert.ToBase64String( Credential.Encrypt( buffer, false ) );
		}

		/// <summary>
		/// Gets the XML document that contains the specified Domain property.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>An XmlDocument object containing the found domain property.</returns>
		private XmlDocument GetDocumentByDomain( string domainID )
		{
			XmlDocument document = null;

			MultiValuedList mvl = properties.GetProperties( PropertyTags.Domain );
			foreach ( Property p in mvl )
			{
				XmlDocument mapDoc = p.Value as XmlDocument;
				if ( mapDoc.DocumentElement.GetAttribute( DomainTag ) == domainID )
				{
					document = mapDoc;
					break;
				}
			}

			return document;
		}

		/// <summary>
		/// Gets the XML document that contains the specified Domain property.
		/// </summary>
		/// <param name="userID">User ID to use to discover domain property.</param>
		/// <returns>An XmlDocument object containing the found domain property.</returns>
		private XmlDocument GetDocumentByUserID( string userID )
		{
			XmlDocument document = null;

			MultiValuedList mvl = properties.GetProperties( PropertyTags.Domain );
			foreach ( Property p in mvl )
			{
				XmlDocument mapDoc = p.Value as XmlDocument;
				if ( mapDoc.DocumentElement.GetAttribute( UserTag ) == userID )
				{
					document = mapDoc;
					break;
				}
			}

			return document;
		}

		/// <summary>
		/// Gets the specified Domain property.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>A Property object containing the found domain property.</returns>
		private Property GetPropertyByDomain( string domainID )
		{
			Property property = null;

			MultiValuedList mvl = properties.GetProperties( PropertyTags.Domain );
			foreach ( Property p in mvl )
			{
				XmlDocument mapDoc = p.Value as XmlDocument;
				if ( mapDoc.DocumentElement.GetAttribute( DomainTag ) == domainID )
				{
					property = p;
					break;
				}
			}

			return property;
		}

		/// <summary>
		/// Gets the specified Domain property.
		/// </summary>
		/// <param name="userID">User ID to use to discover domain property.</param>
		/// <returns>A Property object containing the found domain property.</returns>
		private Property GetPropertyByUserID( string userID )
		{
			Property property = null;

			MultiValuedList mvl = properties.GetProperties( PropertyTags.Domain );
			foreach ( Property p in mvl )
			{
				XmlDocument mapDoc = p.Value as XmlDocument;
				if ( mapDoc.DocumentElement.GetAttribute( UserTag ) == userID )
				{
					property = p;
					break;
				}
			}

			return property;
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Adds a domain identity property to the Identity object.
		/// </summary>
		/// <param name="userID">Identity that this user is known as in the specified domain.</param>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>The modified identity object.</returns>
		internal Identity AddDomainIdentity( string userID, string domainID )
		{
			return AddDomainIdentity( userID, domainID, null, CredentialType.None );
		}

		/// <summary>
		/// Adds a domain identity property to the Identity object.
		/// </summary>
		/// <param name="userID">Identity that this user is known as in the specified domain.</param>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <param name="credentials">Credentials for this domain. This may be null.</param>
		/// <param name="type">The type of credentials stored.</param>
		/// <returns>The modified identity object.</returns>
		internal Identity AddDomainIdentity( string userID, string domainID, string credentials, CredentialType type )
		{
			XmlDocument mapDoc = null;
			
			// Check to see if the domain already exists.
			Property p = GetPropertyByDomain( domainID );
			if ( p != null )
			{
				mapDoc = p.Value as XmlDocument;
			}
			else
			{
				mapDoc = new XmlDocument();
				XmlElement root = mapDoc.CreateElement( MappingTag );
				mapDoc.AppendChild( root );
				mapDoc.DocumentElement.SetAttribute( DomainTag, domainID );

				p = new Property( PropertyTags.Domain, mapDoc );
				properties.AddNodeProperty( p );
			}

			mapDoc.DocumentElement.SetAttribute( UserTag, userID );
			mapDoc.DocumentElement.SetAttribute( TypeTag, type.ToString() );

			if ( ( credentials != null ) && ( type != CredentialType.None ) )
			{
				if ( type == CredentialType.Basic )
				{
					mapDoc.DocumentElement.SetAttribute( CredentialTag, EncryptCredential( credentials ) );
				}
				else
				{
					mapDoc.DocumentElement.SetAttribute( CredentialTag, credentials );
				}
			}

			p.SetPropertyValue( mapDoc );
			return this;
		}

		/// <summary>
		/// Removes the specified domain mapping from the identity object.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>The modified identity object.</returns>
		internal Identity DeleteDomainIdentity( string domainID )
		{
			// Do not allow the local domain to be deleted.
			if ( domainID == StoreReference.LocalDomain )
			{
				throw new CollectionStoreException( "Cannot remove the local domain." );
			}

			// Find the property to be deleted.
			Property p = GetPropertyByDomain( domainID );
			if ( p != null )
			{
				p.DeleteProperty();
			}

			return this;
		}

		/// <summary>
		/// Gets the domain associated with the specified user ID.
		/// </summary>
		/// <param name="userID">User ID to find the associated domain for.</param>
		/// <returns>Domain name associated with the specified user ID if it exists. Otherwise null is returned.</returns>
		internal string GetDomainFromUserID( string userID )
		{
			string domainID = null;

			// Find the property associated with the user ID.
			XmlDocument document = GetDocumentByUserID( userID );
			if ( document != null )
			{
				domainID = document.DocumentElement.GetAttribute( DomainTag );
			}

			return ( ( domainID != null ) && ( domainID != String.Empty ) ) ? domainID : null;
		}

		/// <summary>
		/// Gets the user ID associated with the specified domain ID.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>User ID associated with the specified domain ID if it exists. Otherwise null is returned.</returns>
		internal string GetUserIDFromDomain( string domainID )
		{
			string userID = null;

			// Find the property associated with the user ID.
			XmlDocument document = GetDocumentByDomain( domainID );
			if ( document != null )
			{
				userID = document.DocumentElement.GetAttribute( UserTag );
			}

			return ( ( userID != null ) && ( userID != String.Empty ) ) ? userID : null;
		}

		/// <summary>
		/// Gets the user identifier and credentials for the specified domain.
		/// </summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <param name="userID">Gets the userID of the user associated with the specified domain.</param>
		/// <param name="credentials">Gets the credentials for the user.</param>
		/// <returns>CredentialType enumerated object.</returns>
		internal CredentialType GetDomainCredentials( string domainID, out string userID, out string credentials )
		{
			// Find the property associated with the domain.
			XmlDocument document = GetDocumentByDomain( domainID );
			if ( document == null )
			{
				throw new CollectionStoreException( "The specified domain does not exist." );
			}

			// Return the User ID.
			userID = document.DocumentElement.GetAttribute( UserTag );

			// Get the credential type.
			string credTypeString = document.DocumentElement.GetAttribute( TypeTag );
			CredentialType credType = ( CredentialType )Enum.Parse( typeof( CredentialType ), credTypeString, true );

			// Return the credentials.
			credentials = document.DocumentElement.GetAttribute( CredentialTag );
			if ( credentials != String.Empty )
			{
				if ( credType == CredentialType.Basic )
				{
					credentials = DecryptCredential( credentials );
				}
			}
			else
			{
				credentials = null;
			}

			return credType;
		}

		/// <summary>
		/// Gets the user identifier and  pass-phrase for the specified domain.
		/// </summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <returns>CredentialType enumerated object.</returns>
		internal bool GetRememberOption( string domainID)
		{
			string remember;
			// Find the property associated with the domain.
			XmlDocument document = GetDocumentByDomain( domainID );
			if ( document == null )
			{
				throw new CollectionStoreException( "The specified domain does not exist." );
			}

			// Return the remember 
			remember = document.DocumentElement.GetAttribute( RememberPassPhraseTag );
			if (remember == "true")
				return true;
			else
				return false;
				
		}

		/// <summary>
		/// Gets the user identifier and  pass-phrase for the specified domain.
		/// </summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <returns>CredentialType enumerated object.</returns>
		internal string GetPassPhrase( string domainID)
		{
			// Find the property associated with the domain.
			XmlDocument document = GetDocumentByDomain( domainID );
			if ( document == null )
			{
				throw new CollectionStoreException( "The specified domain does not exist." );
			}

			// Get the credential type.
			string credTypeString = document.DocumentElement.GetAttribute( PassPhraseTypeTag );
			if( credTypeString == null || credTypeString == String.Empty)
			{
				return null;
			}
			CredentialType credType = ( CredentialType )Enum.Parse( typeof( CredentialType ), credTypeString, true );

			// Return the credentials.
			string passPhrase = document.DocumentElement.GetAttribute( PassPhraseTag );
			if ( passPhrase != null && passPhrase != String.Empty )
			{
				if ( credType == CredentialType.Basic )
				{
					passPhrase = DecryptCredential( passPhrase );
				}
			}
			else
			{
				passPhrase = null;
			}
			return passPhrase;
		}

		/// <summary>
		/// Sets the credentials for the specified domain.
		/// </summary>
		/// <param name="domainID">The domain to set the password for.</param>
		/// <param name="credentials">The domain credentials.</param>
		/// <param name="type">Type of credentials.</param>
		/// <returns>The modified identity object.</returns>
		internal Identity SetDomainCredentials( string domainID, string credentials, CredentialType type )
		{
			Property p = GetPropertyByDomain( domainID );
			if ( p == null )
			{
				throw new CollectionStoreException( "There is no mapping for this domain." );
			}

			// Set the password on the mapping.
			XmlDocument mapDoc = p.Value as XmlDocument;
			if ( type == CredentialType.None )
			{
				if ( domainID == StoreReference.LocalDomain )
				{
					throw new CollectionStoreException( "Cannot remove the local domain credentials." );
				}

				mapDoc.DocumentElement.RemoveAttribute( CredentialTag );
			}
			else
			{
				if ( type == CredentialType.Basic )
				{
					mapDoc.DocumentElement.SetAttribute( CredentialTag, EncryptCredential( credentials ) );
				}
				else
				{
					mapDoc.DocumentElement.SetAttribute( CredentialTag, credentials );
				}
			}

			mapDoc.DocumentElement.SetAttribute( TypeTag, type.ToString() );
			p.SetPropertyValue( mapDoc );
			return this;
		}

		/// <summary>
		/// Stores the passphrase for the specified domain.
		/// </summary>
		/// <param name="domainID">The domain to store the passphrase for.</param>
		/// <param name="passPhrase">The domain passphrase.</param>
		/// <param name="type">Type of credentials.</param>
		/// <param name="rememberPassPhrase"></param>
		/// <returns>The modified identity object.</returns>
		internal Identity StorePassPhrase( string domainID, string passPhrase, CredentialType type, bool rememberPassPhrase)
		{
			Property p = GetPropertyByDomain( domainID );
			if ( p == null )
			{
				throw new CollectionStoreException( "There is no mapping for this domain." );
			}

			// Set the password on the mapping.
			XmlDocument mapDoc = p.Value as XmlDocument;
			if ( type == CredentialType.None )
			{
				mapDoc.DocumentElement.RemoveAttribute( PassPhraseTag );
				mapDoc.DocumentElement.RemoveAttribute(RememberPassPhraseTag);
			}
			else
			{
				if ( type == CredentialType.Basic )
				{
					if( passPhrase != null && passPhrase != "")
						mapDoc.DocumentElement.SetAttribute( PassPhraseTag, EncryptCredential( passPhrase ) );
				}
				else
				{
					mapDoc.DocumentElement.SetAttribute( PassPhraseTag, passPhrase );
				}
				if(rememberPassPhrase)
					mapDoc.DocumentElement.SetAttribute( RememberPassPhraseTag, "true");
				else
					mapDoc.DocumentElement.SetAttribute( RememberPassPhraseTag, "false");
			}

			mapDoc.DocumentElement.SetAttribute( PassPhraseTypeTag, type.ToString() );
			p.SetPropertyValue( mapDoc );
			return this;
		}
	        /// <summary>
        	/// 
	        /// </summary>
        	/// <param name="domainID"></param>
	        /// <returns></returns>
		public RSACryptoServiceProvider GetDomainCredential(string domainID)
		{
			RSACryptoServiceProvider credential = null;

			// Lookup the credential property on the identity.
			XmlDocument mapDoc = GetDocumentByDomain( domainID );
			if ( mapDoc != null )
			{
				credential = DummyCsp;
				credential.FromXmlString( mapDoc.DocumentElement.GetAttribute( CredentialTag ) );
			}
			return credential;
		}

		#endregion
	}
}
