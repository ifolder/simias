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
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading;
using System.Security.Cryptography;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Event;
using Simias.POBox;
using Simias.Policy;
using Simias.Storage.Provider;
using Simias.Sync;
using Simias.CryptoKey;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
    	/// <summary>
    	/// The security status of the collection. This is a bitmap representing Encryption, 
    	/// SSL and other forms of security the collection is capable of
    	///
    	/// </summary>
    	public enum SecurityStatus : int
    	{
        	Encryption = 0x0001,
        	SSL = 0x0002,
        	UNSET = 0x0000
    	};

    	public enum SecurityStatusMask : int
    	{
        	Encryption = 0x0001, // yet to be set
        	SSL = 0xfffd
    	};

    	/// <summary>
    	/// SearchPropertyList class will be used to support MultiQuery Search.
    	/// </summary>
	public class SearchPropertyList
	{
		public ArrayList PropList;
		public ArrayList SearchOpList;	
		public SearchPropertyList()
		{
			PropList = new ArrayList();
			SearchOpList = new ArrayList();	
		}

		public void Add(string propertyName, string propertyValue, SearchOp searchOperator)
		{
                       	Property prop = new Property(propertyName, propertyValue );
			PropList.Add(prop);
			SearchOpList.Add(searchOperator);
		}

		public void Add(string propertyName, int propertyValue, SearchOp searchOperator)
		{
                       	Property prop = new Property(propertyName, propertyValue );
			PropList.Add(prop);
			SearchOpList.Add(searchOperator);
		}

		public void Clean()
		{
			PropList.Clear();
			SearchOpList.Clear();
		}
	}

	/// <summary>
	/// A Collection object is contained by a Store object and describes a relationship between the objects
	/// that it contains.
	/// </summary>
	public class Collection : Node, IEnumerable
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Collection ) );

		/// <summary>
		/// Table to store in-memory collection locks.
		/// </summary>
		static private Hashtable lockTable = new Hashtable();

		/// <summary>
		/// Reference to the store.
		/// </summary>
		private Store store;

		/// <summary>
		/// Access control object for this Collection object.
		/// </summary>
		private AccessControl accessControl;

		/// <summary>
		/// Used to do a quick lookup of the domain ID.
		/// </summary>
		private string domainID = null;

		/// <summary>
		/// If true the managed directory needs to be created.
		/// </summary>
		private bool createManagedPath = false;

		/// <summary>
		/// Change log used to indicate events to a collection.
		/// </summary>
		private ChangeLog changeLog = new ChangeLog();

		/// <summary>
		/// Used when the collection is locked to allow the instance that locked
		/// the collection to make changes.
		/// </summary>
		private string lockString = null;

		/// <summary>
		/// folder security (encryption and SSL)
		/// </summary>
		private bool ssl;

		/// <summary>
		/// Encryption Algorithm type
		/// </summary>
		private string encryptionAlgorithm;

		/// <summary>
		/// Encryption key
		/// </summary>
		private string encryptionKey;

		/// <summary>
		/// Encryption key
		/// </summary>
		private string recoveryKey;

		/// <summary>
		/// Encryption Blob
		/// </summary>
		private string encryptionBlob;

		///<summary>
		/// Enable
		///</summary>
		private bool disabled;

		private static AutoResetEvent syncEvent = new AutoResetEvent(false);

		#endregion

		#region Properties
		/// <summary>
		/// Gets whether this machine is the client or the master.
		/// </summary>
		private bool OnMaster
		{
			get { return ( MasterIncarnation == 0 ) ? true : false; }
		}

		/// <summary>
		/// Gets or sets the sync priority for this collection.
		/// </summary>
		internal int Priority
		{
			get
			{
				Property p = properties.GetSingleProperty( PropertyTags.Priority );
				return ( p != null ) ? ( int )p.Value : -1;
			}

			set 
			{ 
				Property p = new Property( PropertyTags.Priority, value );
				p.LocalProperty = true;
				properties.ModifyNodeProperty( p ); 
			}
		}

		/// <summary>
		/// Gets the name of the domain that this collection belongs to.
		/// </summary>
		public string Domain
		{
			get 
			{ 
				if ( domainID == null )
				{
					// Only look it up one time.
					domainID = properties.FindSingleValue( PropertyTags.DomainID ).Value as string; 
				}

				return domainID;
			}
		}
		
		/// <summary>
		/// Gets the SSL
		/// </summary>
		public bool SSL
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.SecurityStatus);
				bool ssl = false;
		                if (p != null)
                		{// Checking bool for backward compatibility with 3.6 client
		                    if (p.Type == Syntax.Boolean)
                		        ssl = (bool)p.Value;
		                    else
                		        ssl = ((int)p.Value & (int)SecurityStatus.SSL) == (int)SecurityStatus.SSL? true : false;
                		}
				return ssl;
			}
			set
                        { 
				Property p = properties.FindSingleValue(PropertyTags.SecurityStatus);
                                if (p != null)
				{
					int status = 0;
					if (p.Type == Syntax.Boolean)
					{
						if( value )
							status = (int)SecurityStatus.SSL;
						//Property pe = properties.FindSingleValue(PropertyTags.EncryptionType);
						if( p!= null )
							status |= (int)SecurityStatus.Encryption;
						p.DeleteProperty();
						Property pc = new Property( PropertyTags.SecurityStatus, status );
                        	        	properties.ModifyNodeProperty(pc);
					}
					else
					{
						if( value )
							p.Value = (int)p.Value | (int)SecurityStatus.SSL;
						else
							p.Value = (int)p.Value & (int)SecurityStatusMask.SSL;
						Property pc = new Property( PropertyTags.SecurityStatus, p.Value );
                                                properties.ModifyNodeProperty(pc);
					}
				}
                        }
		}

        /// <summary>
        /// Gets the Encryption status
        /// </summary>
        public bool Encryption
        {
            get
            {
                Property p = properties.FindSingleValue(PropertyTags.SecurityStatus);
                bool encr = false;
                if (p != null)
                {//for backward compatibility with 3.6 client
                    if (p.Type == Syntax.Boolean)
                        encr = (bool)p.Value;
                    else
                        encr = ((int)p.Value & (int)SecurityStatus.Encryption) == (int)SecurityStatus.Encryption ? true : false;
                }
                return encr;
            }
        }

		/// <summary>
		/// Gets the encryption algorithm
		/// </summary>
		public string EncryptionAlgorithm
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.EncryptionType);
				string encryptionAlgorithm = (p!=null) ? (string) p.Value as string : null;
				return encryptionAlgorithm;
			}			
		}

        /// <summary>
        /// Get/Set DataMovement value
        /// </summary>
		public bool DataMovement
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.DataMovement);
				if ( p != null )
					return true;
				else
					return false;
			}
			set
			{
				if( value == true)
				{
					Property p = new Property( PropertyTags.DataMovement, value );
					p.LocalProperty = true;
					this.Properties.ModifyProperty( p );
				}
				else
				{
					Property p = properties.FindSingleValue(PropertyTags.DataMovement);
					if (p != null )
						p.DeleteProperty();
				}
			}
		}

         /// <summary>
         /// Get/Set DataMovement value
         /// </summary>
                public int TotalRestoreFileCount
                {
                        get
                        {
                                Property p = properties.FindSingleValue(PropertyTags.TotalRestoreFileCount);
                                if ( p != null )
                                        return (int)p.Value;
                                else
                                        return -1;
                        }
                        set
                        {
                                if( value != -1)
                                {
                                        Property p = new Property( PropertyTags.TotalRestoreFileCount, value );
                                        p.LocalProperty = true;
                                        this.Properties.ModifyProperty( p );
                                }
                                else
                                {
                                        Property p = properties.FindSingleValue(PropertyTags.TotalRestoreFileCount);
                                        if (p != null )
                                                p.DeleteProperty();
                                }
                        }
                }
 

                public int RestoredFileCount
                {
                        get
                        {
                                Property p = properties.FindSingleValue(PropertyTags.RestoredFileCount);
                                if ( p != null )
                                        return (int)p.Value;
                                else
                                        return -1;
                        }
                        set
                        {
                                if( value != -1)
                                {
                                        Property p = new Property( PropertyTags.RestoredFileCount, value );
                                        p.LocalProperty = true;
                                        this.Properties.ModifyProperty( p );
                                }
                                else
                                {
                                        Property p = properties.FindSingleValue(PropertyTags.RestoredFileCount);
                                        if (p != null )
                                                p.DeleteProperty();
                                }
                        }
                }


         /// <summary>
         /// Get/Set DataMovement value
         /// </summary>
                public int RestoreStatus
                {
                        get
                        {
                                Property p = properties.FindSingleValue(PropertyTags.Restore);
                                if ( p != null )
                                        return (int)p.Value;
                                else
                                        return -1;
                        }
                        set
                        {
                                if( value != -1)
                                {
                                        Property p = new Property( PropertyTags.Restore, value );
                                        p.LocalProperty = true;
                                        this.Properties.ModifyProperty( p );
                                }
                                else
                                {
                                        Property p = properties.FindSingleValue(PropertyTags.Restore);
                                        if (p != null )
                                                p.DeleteProperty();
                                }
                        }
                }


        /// <summary>
        /// Get/Set details about MigratedFolder
        /// </summary>
		public int MigratediFolder
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.MigratediFolder);
				int MigrationSource = (p!=null) ? (int) p.Value : 0;
				return MigrationSource;
			}
			set
			{
				this.Properties.ModifyProperty(PropertyTags.MigratediFolder, value);
			}
		}
		/// <summary>
		/// Get/Set the passphrase encrypted encryption key 
		/// </summary>
		public string EncryptionKey
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.EncryptionKey);
				string encryptionKey = (p!=null) ? (string) p.Value as string : null;
				return encryptionKey;
			}
			set
			{	Property p = new Property( PropertyTags.EncryptionKey, value );
				properties.ModifyNodeProperty(p);
			}
		}

		/// <summary>
		/// Gets the encryption Blob
		/// </summary>
		public string EncryptionBlob
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.EncryptionBlob);
				string encryptionBlob = (p!=null) ? (string) p.Value as string : null;
				return encryptionBlob;
			}
			set
			{
				properties.AddNodeProperty(PropertyTags.EncryptionBlob, value);
			}
		}

		/// <summary>
		/// Get/Set the Recovery agent encrypted encryption key
		/// </summary>
		public string RecoveryKey
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.RecoveryKey);
				string recoveryKey = (p!=null) ? (string) p.Value as string : null;
				return recoveryKey;
			}
			set
			{	Property p = new Property(PropertyTags.RecoveryKey, value);
				properties.ModifyNodeProperty(p);
			}
		}

		/// <summary>
		/// Get/Set the local property to merge the server and client data
		/// </summary>
		public bool Merge
		{
			get
			{
				Property p = properties.FindSingleValue(PropertyTags.Merge);
				bool merge = (p!=null) ? (bool) p.Value : false;
				return merge;
			}
			set
			{
				Property p = new Property(PropertyTags.Merge, value);
				p.LocalProperty = true; //always local property
				properties.ModifyNodeProperty(p);
			}
		}


		/// <summary>
		/// Gets the directory where store managed files are kept.
		/// </summary>
		public string ManagedPath
		{
			get { return store.GetStoreManagedPath( id ); }
		}

		/// <summary>
		/// Gets the path to where the data files are stored if this is an enterprise
		/// server. Otherwise this returns a null.
		/// </summary>
		public string UnmanagedPath
		{
			get { return ( Store.IsEnterpriseServer ) ? store.GetStoreUnmanagedPath( id ) : null; }
		}

		/// <summary>
		/// Gets the Indirection Prefix to where the data files are stored if this is an enterprise
		/// server. Otherwise this returns a null.
		/// </summary>
		public string UnmanagedPrefix
		{
			get { return ( Store.IsEnterpriseServer ) ? store.GetStoreUnmanagedPrefix( store.Version ) : null; }
		}

		/// <summary>
		/// Get the master url where the collection is hosted.
		/// </summary>
		public Uri MasterUrl
		{
		//check for SSL and modify the URL
			get { return DomainProvider.ResolveLocation( this ); }
		}

		/// <summary>
		///  Gets the current owner of the collection.
		/// </summary>
		public Member Owner
		{
			get 
			{ 
				Member owner = null;

				// Find the Member object where the Owner tag exists.
				ICSList list = Search( PropertyTags.Owner, Syntax.Boolean );
				foreach ( ShallowNode sn in list )
				{
					owner = new Member( this, sn );
					break;
				}

				return owner;
			}
		}

		/// <summary>
		/// Gets the amount of data in bytes that is stored in this collection.
		/// </summary>
		public long StorageSize
		{
			get 
			{
				Property p = properties.GetSingleProperty( PropertyTags.StorageSize );
				return ( p != null ) ? ( long )p.Value : 0;
			}
		}

		/// <summary>
		/// Gets the Store reference for this Collection object.
		/// </summary>
		public Store StoreReference
		{
			get { return store; }
		}

		/// <summary>
		/// Gets whether this collection can be synchronized.
		/// </summary>
		public bool Synchronizable
		{
			get	
			{ 
				Domain domain = store.GetDomain( Domain );
				return ( ( domain != null ) && ( domain.Role == SyncRoles.Local ) ) ? false : true;
			}
		}

		/// <summary>
		/// Gets whether this collection is locked, preventing modifications from being made.
		/// </summary>
		public bool IsLocked
		{
			get	
			{
				lock ( lockTable )
				{
					string ls = lockTable[ id ] as string;
					return ( ( ls != null ) && (lockString != null ) && (lockString != String.Empty) && ( ls != lockString ) ) ? true : false;
				}
			}
		}

		/// <summary>
                /// Gets whether this collection is enabled.
                /// </summary>
		public bool Disabled
		{
			get
                        {
                                Property p = properties.FindSingleValue( PropertyTags.Disabled );
                                return ( p != null ) ? (bool)p.Value : false;
                        }
                        set
                        {
                                Property p = new Property( PropertyTags.Disabled, value );
                                properties.ModifyNodeProperty( p );
                        }

		}

		/// <summary>
		/// Gets whether the collection has been synchronized to its master
		/// location.
		/// </summary>
		public bool IsHosted
		{
			get { return !CreateMaster; }
		}

		/// <summary>
		/// The syncing role of the base collection.
		/// </summary>
		public SyncRoles Role
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.SyncRole );
				return ( p != null ) ? ( SyncRoles )p.Value : SyncRoles.None;
			}
			set
			{
				// Set the sync role for this collection.
				Property p = new Property( PropertyTags.SyncRole, value );
				p.LocalProperty = true;
				properties.ModifyNodeProperty( p );
			}
		}

		/// <summary>
		/// Does the master collection need to be created?
		/// </summary>
		public bool CreateMaster
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.CreateMaster );
				return ( p != null ) ? ( bool )p.Value : false;
			}

			set 
			{ 
				if ( value )
				{
					Property p = new Property( PropertyTags.CreateMaster, value );
					p.LocalProperty = true;
					properties.ModifyNodeProperty( p );
				}
				else
				{
					properties.DeleteSingleNodeProperty( PropertyTags.CreateMaster );
				}
			}
		}

		/// <summary>
		/// The syncing interval of the collection.
		/// </summary>
		public int Interval
		{
			get { return SyncInterval.Get( this ).Interval; }
		}

		/// <summary>
		/// The store path of the collection.
		/// </summary>
		public string StorePath
		{
			get { return Store.StorePath; }
		}

		/// <summary>
		/// Gets or sets the previous owner of this collection.
		/// </summary>
		public string PreviousOwner
		{
			get 
			{
				Property p = properties.FindSingleValue( PropertyTags.PreviousOwner );
				return ( p != null ) ? p.ToString() : null;
			}

			set 
			{
				if ( ( value != null ) && ( value != String.Empty ) )
				{
					Property p = new Property( PropertyTags.PreviousOwner, value );
					p.LocalProperty = true;
					properties.ModifyNodeProperty( p );
				}
				else
				{
					Property p = properties.FindSingleValue( PropertyTags.PreviousOwner );
					if ( p != null )
					{
						p.DeleteProperty();
					}
				}
			}
		}

		/// <summary>
		/// Gets or Sets the HostID for this collection.
		/// This is the ID of the server that hosts the collection.
		/// </summary>
		public HostNode Host
		{
			get
			{
				string hostID = HostID;
				if (hostID != null)
				{
					Domain domain = Store.GetStore().GetDomain(Domain);
					Member hMember = domain.GetMemberByID(hostID);
					if (hMember != null)
					{
						return new HostNode(hMember);
					}
				}
                return null;
			}
			set
			{
				HostID = value.UserID;
			}
		}


		/// <summary>
		/// Get or Set the HostID for this collection.
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
				if (value != null && value.Length != 0)
				{
					Property p = new Property( PropertyTags.HostID, value );
					p.LocalProperty = true;
					properties.ModifyNodeProperty( p );
				}
				else
				{
					Property p = properties.FindSingleValue( PropertyTags.HostID);
					if ( p != null )
					{
						p.DeleteProperty();
					}
				}
			}
		}

		/// <summary>
		/// Get or Set the HostUri for this collection.
		/// </summary>
		public string HostUri
		{
			get
			{
				string hostUri = null;
				Property p = properties.FindSingleValue( PropertyTags.HostUri );
				if (p != null)
				{
					hostUri = p.ToString();
				}
				return hostUri;
			}
			set
			{
				if (value != null && value.Length != 0)
				{
					Property p = new Property( PropertyTags.HostUri, new Uri(value) );
					p.LocalProperty = true;
					properties.ModifyNodeProperty( p );
				}
				else
				{
					Property p = properties.FindSingleValue( PropertyTags.HostUri);
					if ( p != null )
					{
						p.DeleteProperty();
					}
				}
			}
		}

		/// <summary>
		/// Gets or Sets if SSL should be used.
		/// </summary>
		public bool UseSSL
		{
			get
			{
				Property p = properties.FindSingleValue( PropertyTags.UseSSL );
				return ( p != null ) ? (bool)p.Value : true;
			}
			set
			{
				Property p = new Property( PropertyTags.UseSSL, value );
				p.LocalProperty = true;
				properties.ModifyNodeProperty( p );
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor to create a new Collection object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="collectionName">This is the friendly name that is used by applications to describe the collection.</param>
		/// <param name="domainID">The domain that this object is stored in.</param>
		public Collection( Store storeObject, string collectionName, string domainID ) :
			this ( storeObject, collectionName, Guid.NewGuid().ToString(), domainID )
		{
		}
		
		/// <summary>
		/// Constructor to create a new Collection object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="collectionName">This is the friendly name that is used by applications to describe the collection.</param>
		/// <param name="domainID">The domain that this object is stored in.</param>
		public Collection( Store storeObject, string collectionName, string domainID, byte[] encryptionKey ) :
			this ( storeObject, collectionName, Guid.NewGuid().ToString(), domainID )
		{
		}
		
		

		/// <summary>
		/// Constructor to create a new Collection object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="collectionName">This is the friendly name that is used by applications to describe
		/// this object.</param>
		/// <param name="collectionID">The globally unique identifier for this object.</param>
		/// <param name="domainID">The domain that this object is stored in.</param>
		public Collection( Store storeObject, string collectionName, string collectionID, string domainID ) :
			this( storeObject, collectionName, collectionID, NodeTypes.CollectionType, domainID )
		{
		}

		/// <summary>
		/// Constructor for creating an existing Collection object from a ShallowNode.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		public Collection( Store storeObject, ShallowNode shallowNode ) :
			base( storeObject.GetCollectionByID( shallowNode.ID ) )
		{
			store = storeObject;
			accessControl = new AccessControl( this );
			createManagedPath = !Directory.Exists( ManagedPath );
		}

		/// <summary>
		/// Constructor to create a Collection object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="node">Node object to construct Collection object from.</param>
		public Collection( Store storeObject, Node node ) :
			base( node )
		{
			if ( !IsType( NodeTypes.CollectionType ) )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.CollectionType, type ) );
			}

			store = storeObject;
			accessControl = new AccessControl( this );
			createManagedPath = !Directory.Exists( ManagedPath );
		}

		/// <summary>
		/// Copy constructor for Collection object.
		/// </summary>
		/// <param name="collection">Collection object to construct new Collection object from.</param>
		public Collection( Collection collection ) :
			base( collection )
		{
			store = collection.store;
			accessControl = new AccessControl( this );
			domainID = collection.domainID;
			lockString = collection.lockString;
			createManagedPath = collection.createManagedPath;
		}

		//TODO: add comment
		public Collection( Store storeObject, string collectionName, string domainID, bool ssl, string encryptionAlgorithm, string passphrase, string raPublicKey ) :
			this ( storeObject, collectionName, Guid.NewGuid().ToString(), domainID, ssl, encryptionAlgorithm, passphrase, raPublicKey)
		{
		}

		/// <summary>
		/// Constructor to create a new Collection object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="collectionName">This is the friendly name that is used by applications to describe this object.</param>
		/// <param name="collectionID">The globally unique identifier for this object.</param>
		/// <param name="collectionType">Base type of collection object.</param>
		/// <param name="domainID">The domain that this object is stored in.</param>
		/// <param name="hostID">The ID of the host for this collection.</param>
		internal protected Collection( Store storeObject, string collectionName, string collectionID, string collectionType, string domainID, string hostID ) :
			base( collectionName, collectionID, collectionType )
		{
			store = storeObject;

			// Don't allow this collection to be created, if one already exist by the same id.
			if ( store.GetCollectionByID( id ) != null )
			{
				throw new AlreadyExistsException( String.Format( "The collection: {0} - ID: {1} already exists.", collectionName, collectionID ) );
			}

			// Add that this is a Collection type if it is specified as a derived type.
			if ( collectionType != NodeTypes.CollectionType )
			{
				properties.AddNodeProperty( PropertyTags.Types, NodeTypes.CollectionType );
			}

			// Add the domain ID as a property.
			properties.AddNodeProperty( PropertyTags.DomainID, domainID );

			if (hostID != null)
				this.HostID = hostID;

			// Setup the access control for this collection.
			accessControl = new AccessControl( this );
			createManagedPath = !Directory.Exists( ManagedPath );
		}

        /// <summary>
        /// Constructor to create new collection object
        /// </summary>
        /// <param name="storeObject">Store object that this collection belongs to</param>
        /// <param name="collectionName">This is the friendly name that is used by applications to describe this object</param>
        /// <param name="collectionID">The globally unique identifier for this object</param>
        /// <param name="domainID">The domain that this object is stored in</param>
        /// <param name="ssl">Whether ssl active or not</param>
        /// <param name="encryptionAlgorithm">Encryption algorithm to be used</param>
        /// <param name="passphrase">Pass phrase with which to be encrypted</param>
        /// <param name="raPublicKey">Public key required</param>
		public Collection( Store storeObject, string collectionName, string collectionID, string domainID, bool ssl, string encryptionAlgorithm, string passphrase, string raPublicKey) :
			this( storeObject, collectionName, collectionID, NodeTypes.CollectionType, domainID, ssl, encryptionAlgorithm, passphrase, raPublicKey)
		{
		}

		/// <summary>
		/// Constructor to create a new Collection object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="collectionName">This is the friendly name that is used by applications to describe this object.</param>
		/// <param name="collectionID">The globally unique identifier for this object.</param>
		/// <param name="collectionType">Base type of collection object.</param>
		/// <param name="domainID">The domain that this object is stored in.</param>
		internal protected Collection( Store storeObject, string collectionName, string collectionID, string collectionType, string domainID ) :
			this( storeObject, collectionName, collectionID, collectionType, domainID, null)
		{
		}
		

		/// <summary>
		/// Constructor for creating an existing Collection object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document that describes a Collection object.</param>
		internal protected Collection( Store storeObject, XmlDocument document ) :
			base( document )
		{
			store = storeObject;
			accessControl = new AccessControl( this );
			createManagedPath = !Directory.Exists( ManagedPath );
		}

        /// <summary>
        /// Constructor to create a new Collection object.
        /// </summary>
        /// <param name="storeObject">Store object that this collection belongs to</param>
        /// <param name="collectionName">This is the friendly name that is used by applications to describe this object</param>
        /// <param name="collectionID">The globally unique identifier for this object</param>
        /// <param name="collectionType">Type of collection</param>
        /// <param name="domainID">The domain that this object is stored in</param>
        /// <param name="ssl">Whether ssl active or not</param>
        /// <param name="encryptionAlgorithm">Encryption algorithm to be used</param>
        /// <param name="passphrase">Pass phrase with which to be encrypted</param>
        /// <param name="raPublicKey">Public key required</param>
		internal protected Collection( Store storeObject, string collectionName, string collectionID, string collectionType, string domainID, bool ssl, string encryptionAlgorithm, string passphrase, string raPublicKey) :
			base( collectionName, collectionID, collectionType)
		{
			store = storeObject;
            // Set the security status of the collection based on incoming parameters
            int securityStatus = ssl?(int)SecurityStatus.SSL:(int)SecurityStatus.UNSET;
			this.ssl = ssl;
			this.encryptionAlgorithm = encryptionAlgorithm;

			// Don't allow this collection to be created, if one already exist by the same id.
			if ( store.GetCollectionByID( id ) != null )
				throw new AlreadyExistsException( String.Format( "The collection: {0} - ID: {1} already exists.", collectionName, collectionID ) );
			
			// Add that this is a Collection type if it is specified as a derived type.
			if ( collectionType != NodeTypes.CollectionType )
				properties.AddNodeProperty( PropertyTags.Types, NodeTypes.CollectionType );

			// Add the domain ID as a property.
			properties.AddNodeProperty( PropertyTags.DomainID, domainID );

            if (encryptionAlgorithm != null && encryptionAlgorithm.Length > 0)
			{
				if(passphrase ==null)
					throw new CollectionStoreException("Passphrase not provided");
                //Set the Encryption status of the collection. This needs to be used for all reporting
                securityStatus |= (int)SecurityStatus.Encryption;
                properties.AddNodeProperty(PropertyTags.EncryptionType, encryptionAlgorithm);
                //Hash the passphrase and use it for encryption and decryption
				PassphraseHash hash = new PassphraseHash();
				byte[] Passphrase = hash.HashPassPhrase(passphrase);	
				
				Key key = new Key(128);
				key.EncrypytKey(Passphrase, out this.encryptionKey);
//				log.Debug("iFolder key {0}", key.GetKey());
				this.encryptionBlob = key.HashKey();
			
				properties.AddNodeProperty(PropertyTags.EncryptionKey, this.encryptionKey);
				properties.AddNodeProperty(PropertyTags.EncryptionBlob, this.encryptionBlob);
				
				properties.AddNodeProperty(PropertyTags.EncryptionVersion, EncVersion.version.ToString());

				if(raPublicKey !="")
				{
					RecoveryAgent agent = new RecoveryAgent(raPublicKey);
					this.recoveryKey = agent.EncodeMessage(key.GetKey());
					properties.AddNodeProperty(PropertyTags.RecoveryKey, this.recoveryKey);
				}
			}
            // Add the security status property. This needs to be used for all reporting in Web and client
            properties.AddNodeProperty(PropertyTags.SecurityStatus, securityStatus);
			
			// Setup the access control for this collection.
			accessControl = new AccessControl( this );
			createManagedPath = !Directory.Exists( ManagedPath );
		}

		
		#endregion

		#region Private Methods
		/// <summary>
		/// A member node object has been added to a collection. Look up the POBox for
		/// the new member and create a subscription object letting the new member know
		/// that the collection has been shared with them.
		/// </summary>
		/// <param name="args">The member node that was added to this collection.</param>
		/*
		private void AddSubscription( Member member )
		{
			// Get the current domain for the collection.
			Domain domain = store.GetDomain( Domain );
			if ( domain != null )
			{
#if ( !REMOVE_OLD_INVITATION )
				// Make sure this domain support the new invitation model.
				if ( domain.SupportsNewInvitation )
				{
#endif
					log.Debug( "AddSubscription - Member {0} was added to collection {1}.", member.Name, Name );

					// Get the member that represents the from user.
					Member fromMember = member.IsOwner ? member : GetMemberByID( GetCurrentPrincipal() );
					if ( fromMember != null )
					{
						// Refresh the collection as the current instance may still be a proxy or not be the
						// latest copy. And we don't want to change the state of the current collection object.
						Collection collection = Refresh( new Collection( this ) ) as Collection;

						// Create a subscription object.
						Subscription subscription = new Subscription( domain, collection.Name + " subscription", "Subscription", fromMember.UserID );
						subscription.SubscriptionState = Simias.POBox.SubscriptionStates.Ready;
						subscription.ToName = member.Name;
						subscription.ToIdentity = member.UserID;
						subscription.SubscriptionRights = member.Rights;
						subscription.ToMemberNodeID = member.ID;
						subscription.ToPublicKey = member.PublicKey;
						subscription.FromName = fromMember.Name;
						subscription.FromIdentity = fromMember.UserID;
						subscription.MessageID = subscription.ID;
						subscription.SubscriptionCollectionID = collection.ID;
						subscription.SubscriptionCollectionName = collection.Name;
						subscription.DomainID = domain.ID;
						subscription.DomainName = domain.Name;
						subscription.SubscriptionKey = Guid.NewGuid().ToString();
						subscription.MessageType = "Outbound";
						subscription.SetSubscriptionTypes( collection );

						// Add the host node if we are running against a Multi-Server capable server.
						HostNode host = HostNode.GetLocalHost();
						if (host != null)
						{
							subscription.HostID = host.UserID;
						}

						DirNode dirNode = GetRootDirectory();
						if( dirNode != null )
						{
							subscription.DirNodeID = dirNode.ID;
							subscription.DirNodeName = dirNode.Name;
						}

						// For backwards compatiblity with clients that use the old invitation methods,
						// add the first non-built in type to the subscription as the type.
						MultiValuedList mvl = collection.Properties.FindValues( PropertyTags.Types );
						foreach( Property p in mvl )
						{
							if ( !NodeTypes.IsNodeType( p.ValueString ) )
							{
								subscription.SubscriptionCollectionType = p.ValueString;
								break;
							}
						}

						// Add the subscription to the new member's POBox.
						// Find or create the POBox for the user for this domain.
						Invite(member, subscription);
					}
					else
					{
						log.Error( "Could not add subscription for collection: {0} to user {1}'s POBox.", ID, member.UserID );
					}
#if ( !REMOVE_OLD_INVITATION )
				}
#endif
			}
			else
			{
				log.Error( "Could not find domain {0}.", Domain );
			}
		}

		/// <summary>
		/// Put the subscription into the invitee's POBox.
		/// </summary>
		/// <param name="member">Member to invite.</param>
		/// <param name="subscription">Subscription</param>
		private void Invite( Member member, Subscription subscription )
		{
			try 
			{
				// BUGBUG!! - Brady - Need to fix up this code to remove remote POBoxes.
				HostNode localHost = HostNode.GetLocalHost();
				if (Store.IsEnterpriseServer && 
					( localHost != null ) && 
					( member.HomeServer != null ) && 
					( localHost.ID != member.HomeServer.ID ) )
				{
					SimiasConnection connection = new SimiasConnection(subscription.DomainID, localHost.UserID, SimiasConnection.AuthType.BASIC, member );
					connection.Authenticate();
					POBoxService pos = new POBoxService();
					connection.InitializeWebClient(pos, "POService.asmx");
					pos.SaveSubscription( subscription.Properties.ToString(false) );
				}
				else
				{
					// We are on a client just save the subscription.
					POBox.POBox poBox = POBox.POBox.GetPOBox( store, subscription.DomainID, member.UserID );
					poBox.Commit( subscription );
					log.Debug( "AddSubscription - Successfully invited user {0} to collection {1}.", member.Name, subscription.Name );
				}
			}
			catch (Exception ex)
			{
				log.Error(ex.Message);
			}
		}*/

		/// <summary>
		/// Changes a Node object into a Tombstone object.
		/// </summary>
		/// <param name="node">Node object to change.</param>
		private void ChangeToTombstone( Node node )
		{
			log.Debug("TombStone: changing to tombstonetype... {0}--{1}--{2}", node.ID, node.Name, node.Type);
			string oldType = node.Type;
			node.BaseType = NodeTypes.TombstoneType;
			node.InternalList = new PropertyList( node.Name, node.ID, node.Type );
			node.Properties.AddNodeProperty( PropertyTags.Types, NodeTypes.TombstoneType );
			node.Properties.AddNodeProperty( PropertyTags.TombstoneType, oldType );
			node.IncarnationUpdate = 0;
		}

		/// <summary>
		/// Removes specific nodes from the node list.
		/// </summary>
		/// <param name="nodeList">The node list to remove the specific nodes from.</param>
		/// <param name="indicesOfNodesToRemove">An array of indices indicating which nodes to remove from the node list.</param>
		/// <returns>An array of nodes excluding the specific nodes.</returns>
		private Node[] removeNodesFromNodeList( Node[] nodeList, ArrayList indicesOfNodesToRemove )
		{
			Node[] commitList = new Node[ nodeList.Length - indicesOfNodesToRemove.Count ];

			int index = 0;
			for ( int n = 0; n < nodeList.Length; n++ )
			{
				if ( !indicesOfNodesToRemove.Contains( n ) )
				{
					commitList[ index++ ] = nodeList[ n ];
				}
			}

			return commitList;
		}

		/// <summary>
		/// Removes the collection lock for a collection that is being deleted.
		/// </summary>
		private void DeleteCollectionLock()
		{
			lock ( lockTable )
			{
				lockTable.Remove( id );
			}
		}

		/// <summary>
		/// Gets all of the child descendents of the specified Node for the specified relationship.
		/// </summary>
		/// <param name="name">Name of the relationship property.</param>
		/// <param name="relationship">Relationship to use to search for children.</param>
		/// <param name="childList">ArrayList to add Node children objects to.</param>
		private void GetAllDescendants( string name, Relationship relationship, ArrayList childList )
		{
			// Search for all objects that have this object as a relationship.
			ICSList results = Search( name, relationship );
			foreach ( ShallowNode shallowNode in results )
			{
				childList.Add( Node.NodeFactory( this, shallowNode ) );
				GetAllDescendants( name, new Relationship( id, shallowNode.ID ), childList );
			}
		}

		/// <summary>
		/// Gets the current/impersonating user.
		/// </summary>
		/// <returns>The member ID for the current/impersonating user.</returns>
		private string GetCurrentPrincipal()
		{
			string currentUserID = null;

			// check the impersonation first
			if ( accessControl.IsImpersonating )
			{
				currentUserID = accessControl.ImpersonationMember.UserID;
			}

			// check the current principal set by the authentication module
			else if ( ( Thread.CurrentPrincipal != null ) && ( Thread.CurrentPrincipal.Identity != null )
				&& ( Thread.CurrentPrincipal.Identity.Name != null )
				&& ( Thread.CurrentPrincipal.Identity.Name.Length != 0 ) )
			{
				currentUserID = Thread.CurrentPrincipal.Identity.Name;
			}
				
			// use the domain identity
			if ( currentUserID == null )
			{
				currentUserID = store.GetUserIDFromDomainID( Domain );
			}

			// use the store current user
			if ( currentUserID == null )
			{
				currentUserID = store.CurrentUser.ID;
			}

			return currentUserID;
		}

		/// <summary>
		/// Maps member rights to an event Id to be passed in NodeEventArgs.
		/// </summary>
		/// <param name="node">The node that represents the member object.</param>
		/// <returns>The event Id for the rights.</returns>
		private int GetEventId( Node node )
		{
			int eventId = 0;

			Property pNew = node.Properties.GetSingleProperty( PropertyTags.Ace );
			if ( pNew != null )
			{
				if ( node.DiskNode != null )
				{
					Property pOld = node.DiskNode.Properties.GetSingleProperty( PropertyTags.Ace );
					if ( ( pOld != null ) && !pOld.ValueString.Equals( pNew.ValueString ) )
					{
						AccessControlEntry oldAce = new AccessControlEntry( pOld );
						AccessControlEntry newAce = new AccessControlEntry( pNew );
						switch ( oldAce.Rights )
						{
							case Access.Rights.ReadOnly:
								eventId =
									newAce.Rights.Equals( Access.Rights.ReadWrite ) ?
									(int)MemberRights.ReadOnlyToReadWrite :
									(int)MemberRights.ReadOnlyToAdmin;
								break;
							case Access.Rights.ReadWrite:
								eventId =
									newAce.Rights.Equals( Access.Rights.ReadOnly ) ?
									(int)MemberRights.ReadWriteToReadOnly :
									(int)MemberRights.ReadWriteToAdmin;
								break;
							case Access.Rights.Admin:
								eventId =
									newAce.Rights.Equals( Access.Rights.ReadOnly ) ?
									(int)MemberRights.AdminToReadOnly :
									(int)MemberRights.AdminToReadWrite;
								break;
						}
					}
				}
				else
				{
					AccessControlEntry ace = new AccessControlEntry( pNew );
					switch ( ace.Rights )
					{
						case Access.Rights.ReadOnly:
							eventId = (int)MemberRights.ReadOnly;
							break;
						case Access.Rights.ReadWrite:
							eventId = (int)MemberRights.ReadWrite;
							break;
						case Access.Rights.Admin:
							eventId = (int)MemberRights.Admin;
							break;
					}
				}
			}

			return eventId;
		}

		/// <summary>
		/// Increments the local incarnation property.
		///
		/// NOTE: The database must be locked before making this call and must continue to be held until
		/// this node has been committed to disk.
		/// </summary>
		/// <param name="node">Node object that contains the local incarnation value.</param>
		/// <param name="commitTime">The time of the commit operation.</param>
		private void IncrementLocalIncarnation( Node node, DateTime commitTime )
		{
			ulong incarnationValue;

			// The master incarnation value only needs to be set during import of a Node object.
			if ( node.Properties.State == PropertyList.PropertyListState.Import )
			{
				// Going to use the passed in value for the incarnation number.
				incarnationValue = node.IncarnationUpdate;

				// Make sure that the expected incarnation value matches the current value.
				Node checkNode = ( node.DiskNode != null ) ? node.DiskNode : GetNodeByID( node.ID );
				
				// Check if we are importing on the master or slave.
				if ( !node.IsMaster)
				{
					// No collision if:
					//	1. Specifically told to ignore check.
					//	2. Node object does not exist locally.
					//	3. Master incarnation value is zero (first time sync).
					//	4. Rollback is set in download close (only download the node is in import state)
					//   5. If merge Master incarnation may be zero
					
					//Log.log.Debug("Disk Node LI:{0}  MI:{1}", checkNode.LocalIncarnation, checkNode.MasterIncarnation);
					if ( !node.SkipCollisionCheck && ( checkNode != null ) && ( checkNode.MasterIncarnation != 0 || this.Merge == true))
					{
						// Need to check for a collision here. A collision is defined as an update to the client
						// Node object that the server doesn't know about.
						if ( checkNode.LocalIncarnation != checkNode.MasterIncarnation )
						{
							// Check if there is a collision policy on this Node. If the policy states for the
							// server Node to overwrite the client Node, proceed on and don't throw the
							// collision exception.
							if ( checkNode.CollisionPolicy != CollisionPolicy.ServerWins )
							{
								// There was a collision. Strip the local properties back off the Node object
								// before indication the collision. That way when the collision gets stored it
								// won't duplicate the local properties.
								node.Properties.StripLocalProperties();
								throw new CollisionException( checkNode.ID, checkNode.LocalIncarnation );
							}
						}
					}

					// Update the master and local incarnation value to the specified value.
					node.Properties.ModifyNodeProperty( PropertyTags.MasterIncarnation, incarnationValue );

					// Reset the skip collision check value.
					node.SkipCollisionCheck = false;
				}
				else
				{
					// The server is running.
					// No collision if:
					//	1. Node object does not exist locally.
					//	2. Expected incarnation value is equal to the local incarnation value.
					if ( ( checkNode != null ) && ( node.ExpectedIncarnation != checkNode.LocalIncarnation ) )
					{
						// There was a collision. Strip the local properties back off the Node object
						// before indication the collision.
						node.Properties.StripLocalProperties();
						throw new CollisionException( checkNode.ID, checkNode.LocalIncarnation );
					}
				}
			}
			else
			{
				incarnationValue = node.LocalIncarnation + 1;

				// Update the modifier on the node.
				node.Properties.ModifyNodeProperty( PropertyTags.LastModifier, GetCurrentPrincipal() );
			}

			// Update the local incarnation value to the specified value.
			node.Properties.ModifyNodeProperty( PropertyTags.LocalIncarnation, incarnationValue );
		}

		/// <summary>
		/// Decrements the local incarnation property.
		///
		/// NOTE: The database must be locked before making this call and must continue to be held until
		/// this node has been committed to disk.
		/// </summary>
		/// <param name="node">Node object that contains the local incarnation value.</param>
		/// <param name="commitTime">The time of the commit operation.</param>
		public void DecrementLocalIncarnation( Node node)
		{
			ulong incarnationValue = node.LocalIncarnation - 1;

			// Update the modifier on the node.
			node.Properties.ModifyNodeProperty( PropertyTags.LastModifier, GetCurrentPrincipal());

			// Update the local incarnation value to the specified value.
			node.Properties.ModifyNodeProperty( PropertyTags.LocalIncarnation, incarnationValue);
		}

                public Collection SetEncryptionProperties(string eKey, string eBlob, string eAlgorithm, string rKey)
                {
                        //Set the Encryption status of the collection. This needs to be used for all reporting
                        Property p = properties.FindSingleValue(PropertyTags.SecurityStatus);
                        int securityStatus = 0;
                        if( p!= null)
                                securityStatus = (int)p.Value;
                        securityStatus |= (int)SecurityStatus.Encryption;
                        properties.AddNodeProperty(PropertyTags.EncryptionType, eAlgorithm);
 
                        properties.AddNodeProperty(PropertyTags.EncryptionKey, eKey);
                        properties.AddNodeProperty(PropertyTags.EncryptionBlob, eBlob);
 
                        properties.AddNodeProperty(PropertyTags.EncryptionVersion, EncVersion.version.ToString());

                        properties.AddNodeProperty(PropertyTags.RecoveryKey, rKey);
                        // Add the security status property. This needs to be used for all reporting in Web and client
                        properties.AddNodeProperty(PropertyTags.SecurityStatus, securityStatus);
                        this.Commit();
                        return this;
                }
		


		/// <summary>
		/// Returns whether the specified Node object is a deleted Node object type.
		/// </summary>
		/// <param name="node">Node to check to see if it is a deleted Node object.</param>
		/// <returns>True if the specified Node object is a deleted Node object. Otherwise false.</returns>
		private bool IsTombstone( Node node )
		{
			return ( node.Type == NodeTypes.TombstoneType ) ? true : false;
		}

		/// <summary>
		/// Merges all property changes on the current node with the current object in the database.
		///
		/// Note: The database lock must be acquired before making this call.
		/// </summary>
		/// <param name="node">Existing node that may or may not contain changed properties.</param>
		/// <param name="onlyLocalChanges">Is set to true if only local property changes have been made on the Node object.</param>
		/// <returns>A node that contains the current object from the database with all of the property
		/// changes of the current node.</returns>
		private Node MergeNodeProperties( Node node, out bool onlyLocalChanges )
		{
			// Default the values.
			onlyLocalChanges = true;

			// Get this node from the database.
			Node mergedNode = ( node.DiskNode != null ) ? node.DiskNode : GetNodeByID( node.ID );
			if ( mergedNode != null )
			{
				// If this node is not a tombstone and the merged node is, then the node has been deleted
				// and delete wins.
				if ( !IsTombstone( node ) && IsTombstone( mergedNode ) )
				{
					mergedNode = null;
				}
				else if ( IsTombstone( node ) && !IsTombstone( mergedNode ) )
				{
					// If this node is a tombstone and the merged node is not, then delete wins again and
					// the merged node will be turned into a tombstone.
					mergedNode = node;
				}
				else
				{
					// Before the merge of properties take place, check if there is a collision property
					// and if it is supposed to be included in the merge on the resulting Node object.
					Property collision = node.Properties.FindSingleValue( PropertyTags.Collision );
					if ( ( collision != null ) && ( node.MergeCollisions == false ) )
					{
						collision.DeleteProperty();
						node.MergeCollisions = true;
					}

					// If this node is a tombstone and the merged node is a tombstone, then merge the changes or
					// if this node is not a tombstone and the merged node is not a tombstone, merge the changes.
					// Walk the merge list and perform the changes specified there to the mergedNode.
					foreach ( Property p in node.Properties.ChangeList )
					{
						// See if this is a local property change.
						if ( !p.LocalProperty )
						{
							onlyLocalChanges = false;
						}

						p.ApplyMergeInformation( mergedNode );
					}
				}
			}

			// Clear the change list.
			node.Properties.ClearChangeList();
			return mergedNode;
		}

		/// <summary>
		/// Commits all of the changes made to the Collection object to persistent storage.
		/// After a node has been committed, it will be updated to reflect any new changes that
		/// have occurred if it had to be merged with the current Collection object in the database.
		/// </summary>
		/// <param name="nodeList">Array of Node objects to commit to the database.</param>
		private void ProcessCommit( Node[] nodeList )
		{
			// Get the time that the nodes were committed.
			DateTime commitTime = DateTime.Now;
			bool deleteCollection = false;

			// Create an XML document that will contain all of the changed nodes.
			XmlDocument commitDocument = new XmlDocument();
			commitDocument.AppendChild( commitDocument.CreateElement( XmlTags.ObjectListTag ) );

			// Create an XML document that will contain all of the deleted nodes.
			XmlDocument deleteDocument = new XmlDocument();
			deleteDocument.AppendChild( deleteDocument.CreateElement( XmlTags.ObjectListTag ) );

			// Process the storage size for the list.
			SetStorageSize( nodeList );
			string modifier = GetCurrentPrincipal();

			foreach ( Node node in nodeList )
			{
				if ( node != null )
				{
					switch ( node.Properties.State )
					{
						case PropertyList.PropertyListState.Add:
						{
							// Validate this Collection object.
							ValidateNodeForCommit( node );

							// Increment the local incarnation number for the object.
							IncrementLocalIncarnation( node, commitTime );

							// Set the update time of the node.
							node.UpdateTime = commitTime;

							// Set the creator ID on the node.
							node.Properties.AddNodeProperty( PropertyTags.Creator, modifier );

							// Check so that sync roles can be set on the collection.
							if ( node.IsType( NodeTypes.CollectionType ) )
							{
								// Check if there is a role already set on the collection.
								if ( !node.Properties.HasProperty( PropertyTags.SyncRole ) )
								{
									SetSyncRole( node, node.Properties.State );
								}
							}
							else if ( node.IsBaseType( NodeTypes.StoreFileNodeType ) )
							{
								// If this is a StoreFileNode, commit the buffered stream to disk.
								// This cast is safe because a Node object cannot be a StoreFileNode object
								// and be in the the Add state without having been derived as the right class.
								StoreFileNode sfn = node as StoreFileNode;
								if ( sfn != null )
								{
									sfn.FlushStreamData( this );
								}
							}

							// Copy the XML node over to the modify document.
							XmlNode xmlNode = commitDocument.ImportNode( node.Properties.PropertyRoot, true );
							commitDocument.DocumentElement.AppendChild( xmlNode );
							break;
						}

						case PropertyList.PropertyListState.Delete:
						{
							if ( node.IsType( NodeTypes.CollectionType ) )
							{
								deleteCollection = true;
							}
							else
							{
								// If this is a StoreFileNode object, delete the store managed file.
								if ( node.IsBaseType( NodeTypes.StoreFileNodeType ) )
								{
									try
									{
										// Delete the file.
										StoreFileNode sfn = new StoreFileNode( node );
										File.Delete( sfn.GetFullPath( this ) );
									}
									catch {}
								}

								// Never create Tombstones on the master or if this Node object is already a 
								// Tombstone, delete it.
								if ( !OnMaster && !IsTombstone( node ) )
								{
									// Convert this Node object to a Tombstone.
									ChangeToTombstone( node );

									// Validate this object.
									ValidateNodeForCommit( node );

									// Increment the local incarnation number for the object.
									IncrementLocalIncarnation( node, commitTime );

									// Add a node update property.
									node.UpdateTime = commitTime;

									// Copy the XML node over to the modify document.
									XmlNode xmlNode = commitDocument.ImportNode( node.Properties.PropertyRoot, true );
									commitDocument.DocumentElement.AppendChild( xmlNode );
								}
								else
								{
									// Never create tombstones on the server. Copy the XML node over to the delete document.
									XmlNode xmlNode = deleteDocument.ImportNode( node.Properties.PropertyRoot, true );
									deleteDocument.DocumentElement.AppendChild( xmlNode );
								}
							}
							break;
						}

						case PropertyList.PropertyListState.Update:
						{
							// Make sure that there are changes to the Node object.
							if ( node.IsType( NodeTypes.CollectionType ) || node.Properties.ChangeList.Count != 0 )
							{
								
								bool onlyLocalChanges;
								Node mergeNode = MergeNodeProperties( node, out onlyLocalChanges );
								if ( mergeNode != null )
								{
									// Remember later for event processing.
									node.LocalChanges = onlyLocalChanges;

									// Validate this Collection object.
									ValidateNodeForCommit( mergeNode );

									// Don't bump the incarnation value if only local property changes have
									// been made.
									if ( !onlyLocalChanges )
									{
										// Increment the local incarnation number for the object.
										IncrementLocalIncarnation( mergeNode, commitTime );

										// Set the node update time.
										mergeNode.UpdateTime = commitTime;
									}

									Property rollBack = node.Properties.FindSingleValue(PropertyTags.Rollback);
									if(rollBack != null)
									{
										log.Debug("Roll back to previous version number");
										//Decrement the local incarnation number for the object.
										DecrementLocalIncarnation( mergeNode);										
										mergeNode.Properties.DeleteSingleProperty(PropertyTags.Rollback);
										node.Properties.DeleteSingleProperty(PropertyTags.Rollback);
									}

									// Update the old node with the new merged data.
									node.BaseName = mergeNode.Name;
									node.InternalList = new PropertyList( mergeNode.Properties.PropertyDocument );

									// Copy the XML node over to the modify document.
									XmlNode xmlNode = commitDocument.ImportNode( mergeNode.Properties.PropertyRoot, true );
									commitDocument.DocumentElement.AppendChild( xmlNode );
								}
								else
								{
									// There is no longer a node on the disk. Don't indicate an event.
									node.IndicateEvent = false;
								}
							}
							else
							{
								// Nothing was changed on the node. Don't indicate an event.
								node.IndicateEvent = false;
							}
							break;
						}

						case PropertyList.PropertyListState.Import:
						{
							// Validate this Collection object.
							ValidateNodeForCommit( node );

							// Copy over the local properties to this Node object which is being imported.
							SetLocalProperties( node );

							// Increment the local incarnation number for the object.
							IncrementLocalIncarnation( node, commitTime );						
 

							// Copy the XML node over to the modify document.
							XmlNode xmlNode = commitDocument.ImportNode( node.Properties.PropertyRoot, true );
							commitDocument.DocumentElement.AppendChild( xmlNode );
							break;
						}

						case PropertyList.PropertyListState.Internal:
						{
							// Merge any changes made to the object on the database before this object's
							// changes are committed.
							bool onlyLocalChanges;
							Node mergeNode = MergeNodeProperties( node, out onlyLocalChanges );
							if ( mergeNode != null )
							{
								// Remember later for event processing.
								node.LocalChanges = onlyLocalChanges;

								// Update the old node with the new merged data, but keep the state the same.
								node.BaseName = mergeNode.Name;
								node.InternalList = new PropertyList( mergeNode.Properties.PropertyDocument );
								node.Properties.State = PropertyList.PropertyListState.Internal;

								// Copy the XML node over to the modify document.
								XmlNode xmlNode = commitDocument.ImportNode( mergeNode.Properties.PropertyRoot, true );
								commitDocument.DocumentElement.AppendChild( xmlNode );
							}
							else
							{
								// There is no longer a node on the disk. Don't indicate an event.
								node.IndicateEvent = false;
							}
							break;
						}

						case PropertyList.PropertyListState.Proxy:
						{
							// Validate this Collection object.
							ValidateNodeForCommit( node );

							// Check for the type of sync role that needs to be set if this
							// is a collection.
							if ( node.IsType( NodeTypes.CollectionType ) && ( !node.Properties.HasProperty( PropertyTags.SyncRole ) ) )
							{
								SetSyncRole( node, node.Properties.State );
							}

							// Copy the XML node over to the modify document.
							XmlNode xmlNode = commitDocument.ImportNode( node.Properties.PropertyRoot, true );
							commitDocument.DocumentElement.AppendChild( xmlNode );
							break;
						}

						case PropertyList.PropertyListState.Restore:
						{
							// Validate this Collection object.
							ValidateNodeForCommit( node );

							// Increment the local incarnation number for the object.
							IncrementLocalIncarnation( node, commitTime );

							// Copy the XML node over to the modify document.
							XmlNode xmlNode = commitDocument.ImportNode( node.Properties.PropertyRoot, true );
							commitDocument.DocumentElement.AppendChild( xmlNode );
							break;
						}
					}
				}
			}

			// See if the whole Collection is to be deleted.
			if ( deleteCollection )
			{
				// Delete the collection from the database.
				store.StorageProvider.DeleteContainer( id );

				// If there are store managed files, delete them also.
				if ( Directory.Exists( ManagedPath ) )
				{
					Directory.Delete( ManagedPath, true );
				}

				// If this is an enterprise server delete where the files are stored
				// for this collection.
				string dirPath = UnmanagedPath;
				if ( ( dirPath != null ) && Directory.Exists( dirPath ) )
				{
					Directory.Delete( dirPath, true );
					dirPath = null;
				}

				// Dump all nodes in the cache that belong to this collection.
				store.Cache.DumpCache( id );

				// Delete the collection's change log.
				changeLog.DeleteChangeLogWriter( id );

				// Delete the collection lock.
				DeleteCollectionLock();
			}
			else
			{
				// Call the store provider to update the records.
				store.StorageProvider.CommitRecords( id, commitDocument, deleteDocument );
			}
			
			// Walk the commit list and change all states to updated.
			foreach( Node node in nodeList )
			{
				if ( node != null )
				{
					// If this Node object is a Tombstone that is beinging added, then it came into the commit as
					// an actual node being deleted. Indicate that the object has been deleted. Otherwise do not
					// indicate an event for a Tombstone operation.
					if ( node.IsBaseType( NodeTypes.TombstoneType ) )
					{
						// Check to see if this is a tombstone being deleted.
						if ( node.Properties.State == PropertyList.PropertyListState.Delete )
						{
							// Remove the node from the cache.
							store.Cache.Remove( id, node.ID );
						}
						else
						{
							// The tombstone has changed, update the cache.
							store.Cache.Add( this, node );

							// Indicate the event.
							if ( node.Properties.State == PropertyList.PropertyListState.Add )
							{
								string oldType = node.Properties.FindSingleValue( PropertyTags.TombstoneType ).ToString();
								NodeEventArgs args = new NodeEventArgs( store.Publisher, node.ID, id, modifier, oldType, EventType.NodeDeleted, 0, commitTime, node.MasterIncarnation, node.LocalIncarnation, 0 );
								args.LocalOnly = node.LocalChanges;
								store.EventPublisher.RaiseEvent( args );
							}
						}

						node.Properties.State = PropertyList.PropertyListState.Disposed;
					}
					else
					{
						// If this is a file node type get the length of the file to report in the event.
						long fileSize = node.IsType( NodeTypes.BaseFileNodeType ) ? ( node as BaseFileNode ).Length : 0;

						switch ( node.Properties.State )
						{
							case PropertyList.PropertyListState.Add:
							{
								// Update the cache before indicating the event.
								store.Cache.Add( this, node );

								int eventId = 0;

								// If this is a collection being created, create a change log for it.
								if ( node.IsType( NodeTypes.CollectionType ) )
								{
									changeLog.CreateChangeLogWriter( node.ID );
								}
								else if ( node.IsBaseType( NodeTypes.MemberType ) )
								{
									eventId = GetEventId( node );

									// See if the invitation event is to be processed. Only generate
									// subscriptions for base collection types. LocalDatabase, Domain,
									// etc. base types do not ever have subscriptions made.
									/*
									if ( node.CascadeEvents && IsBaseType( NodeTypes.CollectionType ) )
									{
										// If this is a new node being imported onto a server, check to see if
										// it is a member node being added to a base-type collection. If this
										// member is being added as an owner, then create a subscription so
										// the ifolder can be put on other machines owned by the creator.
										Member member = new Member( node );
										if ( Store.IsEnterpriseServer || member.IsOwner )
										{
											// Add a subscription for the member for this collection.
											AddSubscription( member );
										}
									}*/
								}

								// Indicate the event.
								NodeEventArgs args = new NodeEventArgs( store.Publisher, node.ID, id, modifier, node.Type, EventType.NodeCreated, eventId, commitTime, node.MasterIncarnation, node.LocalIncarnation, fileSize );
								args.LocalOnly = node.LocalChanges;
								store.EventPublisher.RaiseEvent( args );
								node.Properties.State = PropertyList.PropertyListState.Update;
								break;
							}

							case PropertyList.PropertyListState.Proxy:
							{
								// Update the cache before indicating the event.
								store.Cache.Add( this, node );

								int eventId = 0;

								// If this is a collection being created, create a change log for it.
								if ( node.IsType( NodeTypes.CollectionType ) )
								{
									changeLog.CreateChangeLogWriter( node.ID );
								}
								else if ( node.IsType( NodeTypes.MemberType ) )
								{
									eventId = GetEventId( node );
								}

								// Indicate the event.
								NodeEventArgs args = new NodeEventArgs( store.Publisher, node.ID, id, modifier, node.Type, EventType.NodeCreated, eventId, commitTime, node.MasterIncarnation, node.LocalIncarnation, fileSize );
								args.LocalOnly = node.LocalChanges;
								store.EventPublisher.RaiseEvent( args );
								node.Properties.State = PropertyList.PropertyListState.Update;
								break;
							}

							case PropertyList.PropertyListState.Delete:
							{
								// Update the cache before indicating the event.
								store.Cache.Remove( id, node.ID );

								/*
								// See if processing further invitation events are allowed.
								if ( node.CascadeEvents && Store.IsEnterpriseServer && this.DataMovement != true)
								{
									// Check for subscription removal if this is a collection.
									if ( node.IsBaseType( NodeTypes.CollectionType ) )
									{
										// A collection has been deleted.
										RemoveSubscriptionsForCollection();
									}
									else if ( node.IsBaseType( NodeTypes.SubscriptionType ) )
									{
										// A subscription has been removed check to see if the collection needs to be deleted.
										RemoveCollectionBySubscription( new Subscription( node ) );
									}
									else if ( node.IsBaseType( NodeTypes.MemberType ) && IsBaseType( NodeTypes.CollectionType ) )
									{
										// A member has been removed from a collection.
										RemoveSubscriptionByMember( new Member( node ) );
									}
								}*/

								// Indicate the event.
								log.Debug("Raising the node deleted event for {0}--{1}...", node.Name, node.ID);
								NodeEventArgs args = new NodeEventArgs( store.Publisher, node.ID, id, modifier, node.Type, EventType.NodeDeleted, 0, commitTime, node.MasterIncarnation, node.LocalIncarnation, fileSize );
								args.LocalOnly = node.LocalChanges;
								store.EventPublisher.RaiseEvent( args );
								node.Properties.State = PropertyList.PropertyListState.Disposed;
								break;
							}

							case PropertyList.PropertyListState.Import:
							{
								int eventId = 0;

								if ( node.IsBaseType( NodeTypes.MemberType ) )
								{
									eventId = GetEventId( node );
								}

								// Update the cache before indicating the event.
								store.Cache.Add( this, node );

								// If this is a new node being imported onto a server, check to see if
								// it is a member node being added to a base-type collection.
								/*
								if ( node.CascadeEvents && Store.IsEnterpriseServer && ( node.DiskNode == null ) )
								{
									// Only generate subscriptions for base collection types. LocalDatabase, 
									// Domain, etc. base types do not ever have subscriptions made.
									if ( node.IsBaseType( NodeTypes.MemberType ) && IsBaseType( NodeTypes.CollectionType ) )
									{
										// Add a subscription for the member for this collection.
										AddSubscription( new Member( node ) );
									}
								}*/

								// Indicate the event.
								NodeEventArgs args = new NodeEventArgs( "Sync", node.ID, id, modifier, node.Type, ( node.DiskNode != null ) ? EventType.NodeChanged : EventType.NodeCreated, eventId, commitTime, node.MasterIncarnation, node.LocalIncarnation, fileSize );
								args.LocalOnly = node.LocalChanges;
								store.EventPublisher.RaiseEvent( args );
								node.Properties.State = PropertyList.PropertyListState.Update;
								break;
							}

							case PropertyList.PropertyListState.Restore:
							{
								// Update the cache before indicating the event.
								store.Cache.Add( this, node );

								// If this is a collection being created, create a change log for it.
								if ( node.IsType( NodeTypes.CollectionType ) )
								{
									changeLog.CreateChangeLogWriter( node.ID );
								}

								// Indicate the event.
								NodeEventArgs args = new NodeEventArgs( "Backup", node.ID, id, modifier, node.Type, ( node.DiskNode != null ) ? EventType.NodeChanged : EventType.NodeCreated, 0, commitTime, node.MasterIncarnation, node.LocalIncarnation, fileSize );
								args.LocalOnly = node.LocalChanges;
								store.EventPublisher.RaiseEvent( args );
								node.Properties.State = PropertyList.PropertyListState.Update;
								break;
							}

							case PropertyList.PropertyListState.Update:
							{
								bool isMemberNode = node.IsBaseType( NodeTypes.MemberType );
								if ( isMemberNode )
								{
									// Need to get the old node so that we can map rights changes.
									node.DiskNode = GetNodeByID( node.ID );
								}

								// Update the cache before indicating the event.
								store.Cache.Add( this, node );

								// Make sure that it is okay to indicate an event.
								if ( node.IndicateEvent )
								{
									int eventId = 0;

									if ( isMemberNode )
									{
										eventId = GetEventId( node );
									}

									NodeEventArgs args = new NodeEventArgs( store.Publisher, node.ID, id, modifier, node.Type, EventType.NodeChanged, eventId, commitTime, node.MasterIncarnation, node.LocalIncarnation, fileSize );
									args.LocalOnly = node.LocalChanges;
									store.EventPublisher.RaiseEvent( args );

									// If this is a member Node, update the access control entry.
									if ( node.IsBaseType( NodeTypes.MemberType ) )
									{
										// If the node was not instantiated as a Member, then we don't need to
										// worry about cached access control.
										Member member = node as Member;
										if ( member != null )
										{
											member.UpdateAccessControl();
										}
									}
								}
								break;
							}

							case PropertyList.PropertyListState.Internal:
							{
								// Update the cache before indicating the event.
								store.Cache.Add( this, node );

								// See if it is okay to indicate an event.
								if ( node.IndicateEvent )
								{
									// If this node state is a collision being resolved, publish an event so that sync
									// will pick up the resolved node and push it to the server.
									if ( node.MergeCollisions == false )
									{
										NodeEventArgs args = new NodeEventArgs( store.Publisher, node.ID, id, modifier, node.Type, EventType.NodeChanged, 0, commitTime, node.MasterIncarnation, node.LocalIncarnation, fileSize );
										args.LocalOnly = false;
										store.EventPublisher.RaiseEvent( args );
									}

									// If this is a member Node, update the access control entry.
									if ( node.IsBaseType( NodeTypes.MemberType ) )
									{
										// If the node was not instantiated as a Member, then we don't need to
										// worry about cached access control.
										Member member = node as Member;
										if ( member != null )
										{
											member.UpdateAccessControl();
										}
									}
								}

								node.Properties.State = PropertyList.PropertyListState.Update;
								break;
							}
						}
					}

					// Reset in-memory properties.
					node.DiskNode = null;
					node.LocalChanges = false;
					node.IndicateEvent = true;
					node.CascadeEvents = true;
				}
			}
		}

		/// <summary>
		/// Removes the collection from the server if the current user is the owner. Otherwise the
		/// current user's membership is removed from the collection.
		/// </summary>
		/// <param name="subscription">Subscription to the collection.</param>
		private void RemoveCollectionBySubscription( Subscription subscription )
		{
#if ( !REMOVE_OLD_INVITATION )
			Domain domain = store.GetDomain( Domain );
			if ( ( domain != null ) && ( domain.SupportsNewInvitation == true ) )
			{
#endif
				// Determine where the collection is hosted.
				HostNode localHost = HostNode.GetLocalHost();
				HostNode collectionHost = HostNode.GetHostByID( Domain, subscription.HostID );
				if ( Store.IsEnterpriseServer && ( localHost != null ) && ( localHost.ID != collectionHost.ID ) )
				{
					SimiasConnection connection = new SimiasConnection(subscription.DomainID, localHost.UserID, SimiasConnection.AuthType.BASIC, collectionHost );
					connection.Authenticate();
					POBoxService pos = new POBoxService();
					connection.InitializeWebClient(pos, "POService.asmx");
					pos.RemoveCollectionBySubscription(subscription.Properties.ToString(true));
				}
				else
				{
					// The collection is on this box.
					POBox.POBox.RemoveCollectionBySubscription(subscription.Properties.ToString(true));
				}
#if ( !REMOVE_OLD_INVITATION )
			}
#endif
		}

		/// <summary>
		/// Removes all subscriptions associated with this collection.
		/// </summary>
		private void RemoveSubscriptionsForCollection()
		{
#if ( !REMOVE_OLD_INVITATION )
			Domain domain = store.GetDomain( Domain );
			if ( ( domain != null ) && ( domain.SupportsNewInvitation == true ) )
			{
#endif
				// We need to remove all subscriptions on all servers for this collection.
				HostNode localHost = HostNode.GetLocalHost();
				HostNode[] hosts = HostNode.GetHosts(Domain);
				foreach (HostNode host in hosts)
				{
					if ( ( localHost != null ) && ( host.ID != localHost.ID ) )
					{
						try
						{
							SimiasConnection connection = new SimiasConnection(Domain, localHost.UserID, SimiasConnection.AuthType.BASIC, host );
							connection.Authenticate();
							POBoxService pos = new POBoxService();
							connection.InitializeWebClient(pos, "POService.asmx");
							pos.RemoveSubscriptionsForCollection(Domain, ID);
						}
						catch (Exception ex)
						{
							log.Debug(ex.Message);
						}
					}
					else
					{
						POBox.POBox.RemoveSubscriptionsForCollection(Domain, ID);
					}
				}
#if ( !REMOVE_OLD_INVITATION )
			}
#endif
		}

		/// <summary>
		/// Removes the subscription for this collection from the specified member.
		/// </summary>
		/// <param name="member">Member to remove subscription from.</param>
		private void RemoveSubscriptionByMember( Member member )
		{
#if ( !REMOVE_OLD_INVITATION )
			Domain domain = store.GetDomain( Domain );
			if ( ( domain != null ) && ( domain.SupportsNewInvitation == true ) )
			{
#endif
				HostNode localHost = HostNode.GetLocalHost();
				HostNode homeHost = member.HomeServer;
				if (Store.IsEnterpriseServer && ( localHost != null ) && ( homeHost != null ) && ( localHost.ID != homeHost.ID) )
				{
					// The HomeServer for the member is on another host.
					SimiasConnection connection = new SimiasConnection(Domain, localHost.UserID, SimiasConnection.AuthType.BASIC, member );
					connection.Authenticate();
					POBoxService pos = new POBoxService();
					connection.InitializeWebClient(pos, "POService.asmx");
					try{
						pos.RemoveSubscriptionByMember(Domain, ID, member.UserID);
					}catch(Exception ex)
					{
						log.Debug(ex.Message);
					}				
				}
				else
				{
					POBox.POBox.RemoveSubscriptionByMember(Domain, ID, member.UserID);
				}
#if ( !REMOVE_OLD_INVITATION )
			}
#endif
		}

		/// <summary>
		/// Gets the local properties from a Node object in the database and adds them to the specified Node object.
		/// </summary>
		/// <param name="node">Node to copy local properties to.</param>
		private void SetLocalProperties( Node node )
		{
			// Get the local properties from the old node, if it exists, and add them to the new node.
			Node oldNode = ( node.DiskNode != null ) ? node.DiskNode : GetNodeByID( node.ID );
			if ( oldNode != null )
			{
				// Save the node read from the disk.
				node.DiskNode = oldNode;

				// Get the local properties.
				MultiValuedList localProps = new MultiValuedList( oldNode.Properties, Property.Local );
				foreach ( Property p in localProps )
				{
					// Don't copy over a collision property, update property, or last modified property.
					if ( ( p.Name != PropertyTags.Collision ) && 
						( p.Name != PropertyTags.NodeUpdateTime ) &&
						( p.Name != PropertyTags.LastModified ) )
					{
						//copy only if it does not exist
						if(node.Properties.HasProperty(p.Name) == false)
							node.Properties.AddNodeProperty( p );
					}
				}
			}
			else
			{
				// If there is no existing Node object, this imported Node object needs a MasterIncarnation value.
				Property mvProp = new Property( PropertyTags.MasterIncarnation, ( ulong )0 );
				mvProp.LocalProperty = true;
				node.Properties.AddNodeProperty( mvProp );
			}
		}

		/// <summary>
		/// Calculates the storage size of the objects being committed and adds the value to the collection object
		/// contained in the nodeList.
		/// NOTE: The store lock must be held before making this call.
		/// </summary>
		/// <param name="nodeList">List of Node objects to be committed to the store.</param>
		private void SetStorageSize( Node[] nodeList )
		{
			// The collection will be represented as a Node object, which is okay because only the
			// storage property needs to be changed and that can happen when the collection is referenced
			// as a Node object as well.
			Node cNode = null;
			long storeBytes = 0;

			foreach ( Node node in nodeList )
			{
				if ( node != null )
				{
					// Check for BaseFileNode types because they are the only objects that contain files.
					// Don't include the size of the journal.
					if ( node.IsType( NodeTypes.BaseFileNodeType ) && !node.IsType( "Journal" ) )
					{
						// Calculate the new storage size based on the state of the Node object.
						switch ( node.Properties.State )
						{
							case PropertyList.PropertyListState.Add:
							{
								// Add the number of bytes to the overall total.
								BaseFileNode bfn = Node.NodeFactory( this, node ) as BaseFileNode;
								storeBytes += bfn.Length;
								break;
							}

							case PropertyList.PropertyListState.Delete:
							{
								// Subtract the number of bytes from the overall total.
								BaseFileNode bfn = Node.NodeFactory( this, node ) as BaseFileNode;
								storeBytes -= bfn.Length;
								break;
							}

							case PropertyList.PropertyListState.Update:
							case PropertyList.PropertyListState.Import:
							case PropertyList.PropertyListState.Restore:
							{
								long oldLength = 0;

								// Get the current file size from the same Node off the disk.
								BaseFileNode diskNode = ( node.DiskNode != null ) ? node.DiskNode as BaseFileNode : GetNodeByID( node.ID ) as BaseFileNode;
								if ( diskNode != null )
								{
									// Save this so it doesn't have to be looked up again by the commit code.
									node.DiskNode = diskNode;

									// Get the old file size.
									oldLength = diskNode.Length;
								}

								BaseFileNode bfn = Node.NodeFactory( this, node ) as BaseFileNode;
								storeBytes += ( bfn.Length - oldLength );
								break;
							}
						}
					}
					else if ( node.IsType( NodeTypes.CollectionType ) )
					{
						// It could be that there are multiple collection objects in the list. We always want
						// the last one and we also need a reference to the object since we intend to update it
						// and we want the update to be committed.
						cNode = node;
					}
				}
			}

			// Make sure that there is a collection object.
			if ( cNode != null )
			{
				if ( storeBytes != 0 )
				{
					// See if this collection is new or being restored.
					if ( ( cNode.Properties.State == PropertyList.PropertyListState.Add ) ||
						( cNode.Properties.State == PropertyList.PropertyListState.Restore ) )
					{
						// No need to look up the old amount, just add the new amount.
						Property p = new Property( PropertyTags.StorageSize, storeBytes );
						p.LocalProperty = true;
						cNode.Properties.ModifyNodeProperty( p );
					}
					else
					{
						// Get the old storage size and add the delta change size.
						Collection diskCollection = store.GetCollectionByID( id );
						if ( diskCollection != null )
						{
							// Save this so it doesn't have to be looked up again by the commit code.
							cNode.DiskNode = diskCollection;

							// Set the new storage size for the collection.
							Property p = new Property( PropertyTags.StorageSize, diskCollection.StorageSize + storeBytes );
							p.LocalProperty = true;
							cNode.Properties.ModifyNodeProperty( p );
						}
					}
				}
				else
				{
					// If this collection is in the restore state, but there are no file nodes in this
					// commit instance, just set the size to zero.
					if ( cNode.Properties.State == PropertyList.PropertyListState.Restore )
					{
						// No need to look up the old amount, just add the new amount.
						Property p = new Property( PropertyTags.StorageSize, storeBytes );
						p.LocalProperty = true;
						cNode.Properties.ModifyNodeProperty( p );
					}
				}
			}
		}

		/// <summary>
		/// Sets the sync role on the collection node object.
		/// </summary>
		/// <param name="node">Node object that represents a collection.</param>
		/// <param name="state">State of the node.</param>
		private void SetSyncRole( Node node, PropertyList.PropertyListState state )
		{
			// Get the domain so that the domain type can be used to set the role for
			// this collection.
			Storage.Domain domain = store.GetDomain( Domain );
			SyncRoles role = SyncRoles.Local;
			if ( domain != null )
			{
				Storage.Domain.ConfigurationType configType = domain.ConfigType;
				if ( configType == Storage.Domain.ConfigurationType.Workgroup )
				{
					if ( state == PropertyList.PropertyListState.Add )
					{
						role = SyncRoles.Master;
					}
					else if ( state == PropertyList.PropertyListState.Proxy )
					{
						role = SyncRoles.Slave;
					}
				}
				else if ( configType == Storage.Domain.ConfigurationType.ClientServer )
				{
					if ( state == PropertyList.PropertyListState.Add )
					{
						if ( Store.IsEnterpriseServer )
						{
							role = SyncRoles.Master;
						}
						else
						{
							role = SyncRoles.Slave;

							// Set the attribute that the master needs to be created on the enterprise server.
							Property masterProperty = new Property( PropertyTags.CreateMaster, true );
							masterProperty.LocalProperty = true;
							node.Properties.ModifyNodeProperty( masterProperty );
						}
					}
					else if ( state == PropertyList.PropertyListState.Proxy )
					{
						role = Store.IsEnterpriseServer ? SyncRoles.Master : SyncRoles.Slave;
					}
				}
			}

			Property roleProperty = new Property( PropertyTags.SyncRole, role );
			roleProperty.LocalProperty = true;
			node.Properties.ModifyNodeProperty( roleProperty );
		}

		/// <summary>
		/// Validates that the proposed owner changes are valid for this collection.
		/// </summary>
		/// <param name="memberList">List of nodes that contain proposed ownership changes.</param>
		private void ValidateCollectionOwner( ArrayList memberList )
		{
			Member currentOwner = Owner;

			// Only validate if there are changes to the membership.
			if ( memberList.Count > 0 )
			{
				Hashtable ownerTable = new Hashtable( memberList.Count + 1 );
				log.Debug("Member Count {0} Owner Count {1}", memberList.Count, ownerTable.Count);

				// If there is a current owner, add it to the table.
				if ( currentOwner != null )
				{
					ownerTable[ currentOwner.ID ] = currentOwner;
					log.Debug("Current Owner ID {0} Name {1}", currentOwner.ID, currentOwner.Name);
				}

				// Go through each node and add or remove it from the table depending on if
				// it is an owner node.
				foreach( Node node in memberList )
				{
					if ( ( node.Properties.HasProperty( PropertyTags.Owner ) == true ) && 
						( node.Properties.State != PropertyList.PropertyListState.Delete ) )
					{
					log.Debug("Node Owner ID {0} Name {1} Owner true", node.ID, node.Name);
						ownerTable[ node.ID ] = node;
					}
					else
					{
					log.Debug("Node Owner ID {0} Name {1} Owner false", node.ID, node.Name);
                    if (ownerTable.ContainsKey(node.ID) && ownerTable.Count > 1)
						{
							ownerTable.Remove( node.ID );
						}
					}
				}

				log.Debug("Member Count {0} Owner Count {1}", memberList.Count, ownerTable.Count);
				if(ownerTable.Count > 1 && currentOwner != null)
				{
					foreach( Node node in memberList)
					{
						if ( ( node.Properties.HasProperty( PropertyTags.Owner ) == true ) && (node.ID != currentOwner.ID) )
						{
							if(ownerTable.ContainsKey(node.ID))
							{
								ownerTable.Remove(node.ID);
							}
						}
					}	
				}
				// There must be at least one owner and only one owner.
				if ( ownerTable.Count == 0 )
				{
					throw new DoesNotExistException( "Collection must have an owner." );
				}
				else if ( ownerTable.Count > 1 )
				{
					throw new AlreadyExistsException( "Collection cannot have more that one owner." );
				}

				// Make sure that the owner rights on the collection are not being downgraded.
				IEnumerator e = ownerTable.Values.GetEnumerator(); e.MoveNext();
				Member owner = new Member( e.Current as Node );
				if ( !owner.IsProxy && ( owner.Rights != Access.Rights.Admin ) )
				{
					// The only exception to this rule is a POBox. The owner of a POBox will only
					// have ReadWrite access.
					if ( !IsBaseType( NodeTypes.POBoxType ) || ( owner.Rights != Access.Rights.ReadWrite ) )
					{
						throw new CollectionStoreException( "Cannot change owner's rights." );
					}
				}
			}
			else
			{
				// Make sure that there is an owner already on the collection.
				if ( currentOwner == null )
				{
					throw new DoesNotExistException( "Collection must have an owner." );
				}
			}
		}

		/// <summary>
		/// Validates and performs access checks on a Collection before it is committed.
		/// </summary>
		/// <param name="node">Node object to validate changes for.</param>
		private void ValidateNodeForCommit( Node node )
		{
			// Check if there is a valid collection ID property.
			Property property = node.Properties.FindSingleValue( BaseSchema.CollectionId );
			if ( property != null )
			{
				// Verify that this object belongs to this collection.
				if ( property.Value as string != id )
				{
					throw new CollectionStoreException( String.Format( "Node object: {0} - ID: {1} does not belong to collection: {2} - ID: {3}.", node.Name, node.ID, name, id ) );
				}
			}
			else
			{
				// Make sure that this is not a collection object from a different collection.
				if ( node.IsType( NodeTypes.CollectionType ) && ( node.ID != id ) )
				{
					throw new CollectionStoreException( String.Format( "Node object: {0} - ID: {1} does not belong to collection: {2} - ID: {3}.", node.Name, node.ID, name, id ) );
				}

				// Assign the collection id.
				node.Properties.AddNodeProperty( BaseSchema.CollectionId, id );
			}

			// Update the last modified time on the node.
			property = new Property( PropertyTags.LastModified, DateTime.Now );
			property.LocalProperty = true;
			node.Properties.ModifyNodeProperty( property );
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Gets a list of locked collections as ShallowNode objects.
		/// </summary>
		/// <returns>An array of collection IDs representing locked collections.</returns>
		static internal string[] GetLockedList()
		{
			string[] cidList = null;

			lock ( lockTable )
			{
				ICollection keys = lockTable.Keys;
				cidList = new string[ keys.Count ];
				keys.CopyTo( cidList, 0 );
			}

			return cidList;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Aborts non-committed changes to the specified Node object.
		/// </summary>
		/// <param name="node">Node object to abort changes.</param>
		public void Abort( Node node )
		{
			// Save the old PropertyList state.
			PropertyList.PropertyListState oldState = node.Properties.State;

			// Set the current state of the PropertyList object to abort.
			node.Properties.State = PropertyList.PropertyListState.Abort;

			// Walk the merge list and reverse the changes specified there.
			foreach ( Property p in node.Properties.ChangeList )
			{
				p.AbortMergeInformation( node );
			}

			// Get rid of all entries in the change list.
			node.Properties.ClearChangeList();

			// Restore the PropertyList state.
			node.Properties.State = oldState;
		}

		/// <summary>
		/// Changes the owner of the collection and assigns the specified right to the old owner.
		/// Only the current owner can set new ownership on the collection.
		/// </summary>
		/// <param name="newOwner">Member object that is to become the new owner.</param>
		/// <param name="oldOwnerRights">Rights to give the old owner of the collection.</param>
		/// <returns>An array of Nodes which need to be committed to make this operation permanent.</returns>
		public Node[] ChangeOwner( Member newOwner, Access.Rights oldOwnerRights )
		{
			return accessControl.ChangeOwner( newOwner, oldOwnerRights );
		}

		/// <summary>
		/// Collection factory method that constructs a derived Collection object type from the specified 
		/// ShallowNode object.
		/// </summary>
		/// <param name="store">Store object.</param>
		/// <param name="shallowNode">ShallowNode object to construct new Collection object from.</param>
		/// <returns>Downcasts the derived Collection object back to a Collection that can then be 
		/// explicitly casted back up.</returns>
		static public Collection CollectionFactory( Store store, ShallowNode shallowNode )
		{
			Collection rCollection = null;
			switch ( shallowNode.Type )
			{
				case "Collection":
					rCollection = new Collection( store, shallowNode );
					break;

				case "LocalDatabase":
					rCollection = new LocalDatabase( store, shallowNode );
					break;

				case "Domain":
					rCollection = new Domain( store, shallowNode );
					break;

				case "POBox":
					rCollection = new POBox.POBox( store, shallowNode );
					break;

				default:
					throw new CollectionStoreException( "An unknown type: " + shallowNode.Type + " was specified." );
			}

			return rCollection;
		}

		/// <summary>
		/// Commits all of the changes made to the Collection object to persistent storage.
		/// After a Node object has been committed, it will be updated to reflect any new changes that
		/// have occurred if it had to be merged with the current Node object in the database.
		/// </summary>
		public void Commit()
		{
			Commit( this );
		}

		/// <summary>
		/// Commits all of the changes made to the Collection object to persistent storage.
		/// After a node has been committed, it will be updated to reflect any new changes that
		/// have occurred if it had to be merged with the current Collection object in the database.
		/// </summary>
		/// <param name="node">Node object to commit to the database.</param>
		public void Commit( Node node )
		{
			if ( node != null )
			{
				Node[] nodeList = { node };
				Commit( nodeList );
			}
		}

		/// <summary>
		/// Commits all of the changes made to the Collection object to persistent storage.
		/// After a node has been committed, it will be updated to reflect any new changes that
		/// have occurred if it had to be merged with the current Collection object in the database.
		/// </summary>
		/// <param name="nodeList">An array of Node objects to commit to the database.</param>
		public void Commit( Node[] nodeList )
		{
			// Make sure that something is in the list.
			if ( ( nodeList != null ) && ( nodeList.Length > 0 ) )
			{
				bool createCollection = false;
				bool deleteCollection = false;
				bool doAdminCheck = false;
				bool hasCollection = false;
				Member collectionOwner = null;
				ArrayList memberList = new ArrayList();
				ArrayList journalIndices = new ArrayList();

				// See if the database is being shut down.
				if ( store.ShuttingDown )
				{
					// Don't allow any changes - the database is being shut down.
					throw new SimiasShutdownException();
				}

				// See if this collection has been locked and if this instance has the ability to 
				// change the collection.
				if ( IsLocked )
				{
					log.Debug("Collection is Locked {0}, {1}, {2}", id, lockTable[id], lockString);
					throw new LockException();
				}

				int n = 0;

				// Walk the commit list to see if there are any creation and deletion of the collection states.
				foreach( Node node in nodeList )
				{
					if ( node != null )
					{
						if ( node.IsType( NodeTypes.CollectionType ) )
						{
							if ( node.Properties.State == PropertyList.PropertyListState.Delete )
							{
								deleteCollection = true;
							}
							else if ( node.Properties.State == PropertyList.PropertyListState.Add )
							{
								createCollection = true;
							}

							// Remember the slot in the list where the collection object is.
							hasCollection = true;
						}
						else if ( node.IsBaseType( NodeTypes.MemberType ) )
						{
							// Administrative access needs to be checked because collection membership has changed.
							doAdminCheck = true;

							// Convert this node to a member node.
							Member member = new Member( node );

							// See if this member is new.
							if ( node.Properties.State == PropertyList.PropertyListState.Add )
							{
								// Look up to see if this member has already been added.
								if ( GetMemberByName( member.Name ) != null )
								{
									throw new AlreadyExistsException( String.Format( "The member {0} already exists in this collection.", member.Name ) );
								}

								// If this collection is a domain and this member is to be added, call out to the
								// domain provider for this domain to do a pre-commit operation.
								if ( IsBaseType( NodeTypes.DomainType ))
								{
									DomainProvider.PreCommit( Domain, member );
								}
							}
							else if ( node.Properties.State == PropertyList.PropertyListState.Import )
							{
								// Look up to see if this member has already been added.
								Member oldMember = GetMemberByID( member.UserID );
								if ( ( oldMember != null ) && ( oldMember.ID != member.ID ) )
								{
									// Two different clients added the same member represented by
									// different nodes. Last writer wins in this case, so remove
									// the existing member.
									Delete( oldMember );
									memberList.Add( oldMember );
								}
							}

							// Keep track of any ownership changes.
							if ( member.IsOwner )
							{
								collectionOwner = member;
							}

							// Add this member node to the list to validate the collection owner a little later on.
							memberList.Add( node );
						}
						else if ( !doAdminCheck && node.IsBaseType( NodeTypes.PolicyType ) )
						{
							// Administrative access needs to be checked because system policies are controlled objects.
							doAdminCheck = true;
						}
						else if ( node.IsType( NodeTypes.BaseFileNodeType ) )
						{
							// Need to have a collection object for file nodes, because the amount of storage is
							// on the collection object.
							if ( node.IsType( "Journal" ) )
							{
								switch ( node.Properties.State )
								{
									case PropertyList.PropertyListState.Add:
									case PropertyList.PropertyListState.Update:
										if ( !Role.Equals( SyncRoles.Master ) )
										{
											// Don't allow adding/updating a journal node on a slave.
											journalIndices.Add( n );
										}
										break;
									case PropertyList.PropertyListState.Import:
										if ( Role.Equals( SyncRoles.Master ) )
										{
											// Don't allow importing a journal node on a master.
											journalIndices.Add( n );
										}
										else
										{
											// Ignore collisions when importing on a slave.
											node.SkipCollisionCheck = true;
										}
										break;
									case PropertyList.PropertyListState.Delete:
										// Don't allow journal nodes to be deleted.
										journalIndices.Add( n );
										break;
								}
							}
						}
					}

					n++;
				}

				Node[] nodeList2;

				// If on the master and journal nodes are in the commit list, update the node list
				// by removing the journal nodes.
				if ( Role.Equals( SyncRoles.Master ) && ( journalIndices.Count > 0 ) )
				{
					nodeList2 = removeNodesFromNodeList( nodeList, journalIndices );
				}
				else
				{
					nodeList2 = nodeList;
				}

				// If the collection is both created and deleted, then there is nothing to do.
				if ( !deleteCollection || !createCollection )
				{
					Node[] commitList;

					// Delete of a collection supercedes all other operations.  It also is not subject to
					// a rights check. However, a locked collection cannot be deleted by an impersonating
					// member.
					if ( deleteCollection )
					{
						// Only the collection needs to be processed. All other Node objects will automatically
						// be deleted when the collection is deleted.
						commitList = new Node[ 1 ];
						commitList[ 0 ] = this;
					}
					else if ( createCollection )
					{
						// If there is no collection owner specified, then one needs to be created.
						if ( collectionOwner == null )
						{
							// If a collection is being created, then a Member object containing the owner of the
							// collection needs to be created also.
							commitList = new Node[ nodeList2.Length + 1 ];
							nodeList2.CopyTo( commitList, 0 );
							Member owner = accessControl.GetCurrentMember( store, Domain, true );
							commitList[ commitList.Length - 1 ] = owner;
							memberList.Add( owner );
						}
						else
						{
							// The owner is already specified in the list. Use the list as is.
							commitList = nodeList2;
						}
					}
					else
					{
						// If there is no user being impersonated on this collection, there is no need to do any
						// rights checking. Access rights are only checked for impersonated users.
						if ( accessControl.IsImpersonating )
						{
							// Get the impersonating member.
							Member member = accessControl.ImpersonationMember;

							// If membership is changing on the collection, make sure that the current
							// user has sufficient rights.
							if ( doAdminCheck )
							{
								if ( !IsAccessAllowed( member, Access.Rights.Admin ) )
								{
									throw new AccessException( this, member, Access.Rights.Admin, String.Format( "User {0} - ID: {1} does not have sufficient rights to change the member list.", member.Name, member.UserID ) );
								}

								// If ownership rights are changing, make sure the current user has sufficient rights.
								if ( collectionOwner != null )
								{
									// Get the current owner of the collection.
									Member currentOwner = Owner;
									if ( currentOwner != null )
									{
										// See if ownership is changing and if it is, then the current user has to be
										// the current owner.
										if ( ( collectionOwner.UserID != currentOwner.UserID ) && 
											 ( currentOwner.UserID != member.UserID ) )
										{
											throw new AccessException( this, member, String.Format( "User {0} - ID: {1} does not have sufficient rights to change the collection ownership.", member.Name, member.UserID ) );
										}
									}
								}
							}
							else
							{
								// Make sure that current user has write rights to this collection.
								if ( !IsAccessAllowed( member, Access.Rights.ReadWrite ) )
								{
									throw new AccessException( this, member, Access.Rights.ReadWrite, String.Format( "User {0} - ID: {1} does not have sufficient rights to change the collection.", member.Name, member.UserID ) );
								}
							}
						}

						// Validate the collection ownership status.
						ValidateCollectionOwner( memberList );

						// See if we have a collection in the commit list.
						if ( !hasCollection )
						{
							// We have to get a new copy of the collection node instead of just using the
							// 'this' reference because it might contain changes to it that the user doesn't
							// want committed yet.
							commitList = new Node[ nodeList2.Length + 1 ];
							nodeList2.CopyTo( commitList, 0 );
							Collection collection = store.GetCollectionByID( ID );

							// Add the collection to the list.
							commitList[ commitList.Length - 1 ] = collection;
						}
						else
						{
							// Use the passed in list.
							commitList = nodeList2;
						}
					}

					// Acquire the store lock.
					store.LockStore();
					try
					{
						// If the managed directory does not exist, create it.
						if ( !deleteCollection && createManagedPath && !Directory.Exists( ManagedPath ) )
						{
							Directory.CreateDirectory( ManagedPath );
						}

						// Commit to disk.
						ProcessCommit( commitList );
					}
					finally
					{
						// Release the store lock.
						store.UnlockStore();
					}
				}

				// Check if the collection was deleted.
				if ( deleteCollection )
				{
					// Go through each entry marking it deleted.
					foreach( Node node in nodeList )
					{
						if ( node != null )
						{
							node.Properties.State = PropertyList.PropertyListState.Disposed;
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates a property on a Node object that represents the collision of the specified Node object 
		/// with another instance.
		/// </summary>
		/// <param name="collisionNode">Node object that has collided with another instance.</param>
		/// <param name="isFileCollision">True if the collision was caused by a file.</param>
		/// <returns>The Node object that the collision was stored on.</returns>
		public Node CreateCollision( Node collisionNode, bool isFileCollision )
		{
			// Look up the Node where the collision occurred.
			Node localNode = GetNodeByID( collisionNode.ID );
			if ( localNode != null )
			{
				// Set the state to update internally.
				localNode.Properties.State = PropertyList.PropertyListState.Internal;
			}
			else
			{
				// No node exists on the disk, create the collision from the node object that was
				// passed in. Set the state to import so that it commits properly.
				localNode = collisionNode;
				localNode.Properties.State = PropertyList.PropertyListState.Import;
			}

			// See if a collision property already exists.
			Property p = localNode.Properties.GetSingleProperty( PropertyTags.Collision );
			CollisionList cList = ( p == null ) ? new CollisionList() : new CollisionList( p.Value as XmlDocument );

			// Add the new collision to the collision list.
			if ( isFileCollision )
			{
				cList.Modify( new Collision( CollisionType.File, String.Empty ) );
			}
			else
			{
				cList.Modify( new Collision( CollisionType.Node, collisionNode.Properties.PropertyDocument.InnerXml ) );
			}

			// Modify or add the collision list.
			p = new Property( PropertyTags.Collision, cList.Document );
			p.LocalProperty = true;
			localNode.Properties.ModifyNodeProperty( p );

			return localNode;
		}

		/// <summary>
		/// Deletes the specified collection from the persistent store.
		/// </summary>
		/// <returns>The Node object that has been deleted.</returns>
		public Node Delete()
		{
			return Delete( this );
		}

		/// <summary>
		/// Deletes the specified Node object from the persistent store.
		/// </summary>
		/// <param name="node">Node object to delete.</param>
		/// <returns>The Node object that has been deleted.</returns>
		public Node Delete( Node node )
		{
			Node[] nodeList = Delete( node, null );
			return ( nodeList.Length > 0 ) ? nodeList[ 0 ] : null;
		}

		/// <summary>
		/// Deletes an array of Node objects from the persistent store.
		/// </summary>
		/// <param name="nodeList">Array of Node objects to delete.</param>
		/// <returns>An array of Node objects that has been deleted.</returns>
		public Node[] Delete( Node[] nodeList )
		{
			foreach ( Node node in nodeList )
			{
				if ( node != null )
				{
					Delete( node, null );
				}
			}

			return nodeList;
		}

		/// <summary>
		/// Deletes the specified collection from the persistent store.
		/// </summary>
		/// <param name="node">Node to delete.</param>
		/// <param name="relationshipName">If not null, indicates to delete all Node objects that have a
		/// descendent relationship to the specified Node object.</param>
		/// <returns>An array of Node objects that have been deleted.</returns>
		public Node[] Delete( Node node, string relationshipName )
		{
			// Temporary holding list.
			ArrayList tempList = new ArrayList();

			// If the node has not been previously committed or is already deleted, don't add it to the list.
			if ( node.Properties.State == PropertyList.PropertyListState.Update )
			{
				tempList.Add( node );
			}

			if ( relationshipName != null )
			{
				// Get all of the decendents of this object.
				GetAllDescendants( relationshipName, new Relationship( id, node.ID ), tempList );
			}

			// Allocate the Node object array and copy over the results.
			foreach( Node n in tempList )
			{
				n.Properties.State = PropertyList.PropertyListState.Delete;
			}

			return tempList.ToArray( typeof( Node ) ) as Node[];
		}

		/// <summary>
		/// For a given node that has been deleted on master and is a member node, it makes the collections owned
		/// by that node as orphan . This is the case when user was provisioned on slave and it was deleted
		/// and master got that first , During Domain sync , master will send that event to slave and so 
		/// slave will try to make the collections owned by this user as orphan on its local store.
		/// </summary>
		/// <param name="node">Node for which orphan collections has to be made.</param>
		public void OrphanCollections(Node node)
		{
		        if(node == null || !(node.IsBaseType(NodeTypes.MemberType)) || node.IsType (HostNode.HostNodeType) )
				return ;

			// Convert this node to a member node.
			Member member = new Member( node );
		
			string dn =     member.Properties.GetSingleProperty( "DN" ).Value as string;
			if ( dn == null || dn == "" )
			{
				dn = member.Name;
			}	

			Store store = Store.GetStore();
			Domain domain = store.GetDomain(Domain);
			if(domain == null)
			{
				log.Debug("OrphanCollections() : Could not get domain ");
				return;
			}
			
			string [] GroupIDs = domain.GetDeletedMembersGroupList(member.UserID);
			string GroupOrphOwnerID = null;
			bool CheckForSecondaryAdmin = true;

			ICSList cList = store.GetCollectionsByOwner( member.UserID );
			foreach ( ShallowNode sn in cList )
			{
				// Remove the user as a member of this collection.
                                Collection c = new Collection( store, sn );

				if ( !( (Node) c).IsBaseType( NodeTypes.DomainType ) && !( (Node) c).IsBaseType( NodeTypes.POBoxType ) )
				{
					Member cmember = c.GetMemberByID( member.UserID );
					if (cmember != null && cmember.IsOwner == true )
					{

						if( CheckForSecondaryAdmin = true && GroupIDs.Length > 0 ) //make sure this cond gets executed only once, even if collections change
						{
							// foreach group this zombie user belongs to, check if the group has a right secondary admin
							foreach( string groupID in GroupIDs)
							{
								if(groupID == member.UserID)
								{
									// zombie user should not be iterated
									continue;
								}
								ICSList SecondaryAdmins = domain.GetMembersByRights(Access.Rights.Secondary);
								foreach(ShallowNode sns in SecondaryAdmins)
								{
									Member SecondaryAdminMem = new Member(domain, sns);
									long Preference = SecondaryAdminMem.GetPreferencesForGroup(groupID);
									//check, if this secondary admin has rights to own Orphan iFolders of this group
									if (Preference == 0)
									{
										// Secondary admin is not owner of this group, check for next sec admin
										continue;
									}	
									else
									{
										GroupOrphOwnerID = ( Preference & (int)512) != (int)512 ? null : SecondaryAdminMem.UserID;
										if(GroupOrphOwnerID != null)
										{
											log.Debug("GroupOwner has been found for this zombie user, it is "+GroupOrphOwnerID);
											break;
										}
									}
								}	
								// We want this check to be performed only once for one zombie user, so disable the check
								// so that for other collections of same owner, it does not search same data again.
								CheckForSecondaryAdmin = false;
								if(GroupOrphOwnerID != null)
								{
									break;
								}
							}
						}
						// Don't remove an orphaned collection.
						if ( ( member.UserID != domain.Owner.UserID ) )
						{
							// Adding the code so that, if zombie user is member of a group and the group has a setting
							// so that all orphaned iFolders should be owned by groupadmin, then primary admin will not
							// get the ownership

							string SimiasAdminUserID = ( GroupOrphOwnerID == null ? domain.Owner.UserID : GroupOrphOwnerID );
							Member SimiasAdminAsMember = domain.GetMemberByID(SimiasAdminUserID);


							// Now the simias admin must be the member of the collection before he
							// gets the ownership
						
							Member adminMember = c.GetMemberByID( SimiasAdminUserID );
							if ( adminMember == null )
							{
								adminMember = new Member(
												SimiasAdminAsMember.Name,
												SimiasAdminAsMember.UserID,
												Simias.Storage.Access.Rights.Admin );
								c.Commit( adminMember );	
							}
							Property prevProp = new Property( "OrphanedOwner", dn );
							prevProp.LocalProperty = true;
							c.Properties.ModifyProperty( prevProp );
							c.Commit();
						
							c.Commit( c.ChangeOwner( adminMember, Simias.Storage.Access.Rights.Admin ) );
				
							// Now remove the old member
							c.Commit( c.Delete( c.Refresh( cmember ) ) );

							string logMessage =
									String.Format(
										"Domain Sync: Orphaned Collection: {0} - previous owner: {1}",
										c.Name,
										dn );
							log.Info( logMessage );
							
						}
					}
					
				}
			}
			
		}

		/// <summary>
		/// For a given node that has been deleted on master and is a member node, it makes the collections owned
		/// by that node as orphan . This is the case when user was provisioned on slave and it was deleted
		/// and master got that first , During Domain sync , master will send that event to slave and so 
		/// slave will try to remove the memberships from other collections on its local store.
		/// </summary>
		/// <param name="node">Node for which membership has to be removed.</param>
		public void RemoveMemberships(Node node)
		{
			if(node == null || ! (node.IsBaseType(NodeTypes.MemberType) ))
			{
				return ;
			}

			// Convert this node to a member node.
			Member member = new Member( node );
		
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(Domain);
			if(domain == null)
			{
				log.Debug("RemoveMemberships : Could not get domain ");
				return;
			}
			
			ICSList cList = store.GetCollectionsByUser( member.UserID );
			foreach ( ShallowNode sn in cList )
			{
				// Remove the user as a member of this collection.
                                Collection c = new Collection( store, sn );

				if ( ( c.Domain == domain.ID ) && !( (Node) c).IsBaseType( NodeTypes.DomainType ) )
				{
					Member cmember = c.GetMemberByID( member.UserID );
					if (cmember != null && cmember.IsOwner == false )
					{
						// Not the owner, just remove the membership.
						c.Commit( c.Delete( cmember ) );
						string logMessage =
								String.Format(
									"Removed {0}'s membership from Collection: {1}",
									member.UserID,
									c.Name );
						log.Info( logMessage );
					}
				}
			}
		}	

		/// <summary>
		/// For a given node that has been deleted on master and is a member node, it makes the collections owned
		/// by that node as orphan . This is the case when user was provisioned on slave and it was deleted
		/// and master got that first , During Domain sync , master will send that event to slave and so 
		/// slave will try to delete the POBox .
		/// </summary>
		/// <param name="node">Node for which POBox has to be removed.</param>

		public void DeletePOBox(Node node)
		{
			if(node == null || ! (node.IsBaseType(NodeTypes.MemberType) ))
			{
				return ;
			}

			// Convert this node to a member node.
			Member member = new Member( node );
		
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(Domain);
			if(domain == null)
			{
				log.Debug("RemoveMemberships : Could not get domain ");
				return;
			}
			
			ICSList cList = store.GetCollectionsByOwner(member.UserID);
			foreach( ShallowNode sn in cList )
			{
				Collection c = new Collection( store, sn );
				 if ( ( c.Domain == domain.ID ) && ( (Node) c).IsBaseType( NodeTypes.POBoxType ) )
				{
					c.Commit( c.Delete() );
					 string logMessage =
                                                        String.Format(
                                                                "Removed {0}'s POBox from Domain: {1}",
                                                                member.UserID,
                                                                domain.Name );

                                                log.Info( logMessage );
                                                break;

				}
			}
		}

		/// <summary>
		/// Deletes the collision from the specified Node object.
		/// </summary>
		/// <param name="node">Node object from which to delete collision.</param>
		/// <returns>The node object that the collision was deleted from.</returns>
		public Node DeleteCollision( Node node )
		{
			Property p = node.Properties.GetSingleProperty( PropertyTags.Collision );
			if ( p != null )
			{
				p.DeleteProperty();
			}

			return node;
		}

		/// <summary>
		/// Searches all Node objects in the Collection for the specified Types value.
		/// </summary>
		/// <param name="type">String object containing class type to find.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the found Node objects.</returns>
		public ICSList FindType( string type )
		{
			return Search( PropertyTags.Types, type, SearchOp.Equal );
		}

		/// <summary>
		/// Gets a list of ShallowNode objects that represent Node objects that contain collisions in the current
		/// collection.
		/// </summary>
		/// <returns>An ICSList object containing ShallowNode objects representing Node objects that 
		/// contain collisions.</returns>
		public ICSList GetCollisions()
		{
			return Search( PropertyTags.Collision, Syntax.XmlDocument );
		}

		/// <summary>
		/// Gets the type of collision that is stored on the node.
		/// </summary>
		/// <param name="node">Node object that contains a collision property.</param>
		/// <returns>The collision type.</returns>
		public CollisionType GetCollisionType( Node node )
		{
			// Get the collision property.
			Property p = node.Properties.GetSingleProperty( PropertyTags.Collision );
			if ( p == null )
			{
				throw new DoesNotExistException( "A collision does not exist on this Node object." );
			}

			// Get a list of collisions.
			ICSEnumerator e = new CollisionList( p.Value as XmlDocument ).GetEnumerator() as ICSEnumerator;
			if ( e.MoveNext() == false )
			{
				throw new DoesNotExistException( "A collision does not exist on this Node object." );
			}

			Collision c = e.Current as Collision;
			e.Dispose();
			return c.Type;
		}

		/// <summary>
		/// Gets the Member object that represents the currently executing security context.
		/// </summary>
		/// <returns>A Member object that represents the currently executing security context.</returns>
		public Member GetCurrentMember()
		{
			return accessControl.GetCurrentMember( store, Domain, false );
		}

		/// <summary>
		/// Gets the Member object associated with the specified user ID.
		/// </summary>
		/// <param name="userID">Identifier to look up the Member object with.</param>
		/// <returns>The Member object associated with the specified user ID. May return null if the
		/// Member object does not exist in the collection.</returns>
		public Member GetMemberByID( string userID )
		{
			return accessControl.GetMember( userID.ToLower() );
		}

		/// <summary>
		/// Gets the first Member object associated with the specified name.
		/// </summary>
		/// <param name="name">Name to look up the Member object with.</param>
		/// <returns>The first Member object associated with the specified name. May return null if the
		/// Member object does not exist in the collection.</returns>
		public Member GetMemberByName( string name )
		{
			Member member = null;

			ICSList list = Search( BaseSchema.ObjectName, name, SearchOp.Equal );
			foreach ( ShallowNode sn in list )
			{
				if ( sn.Type == NodeTypes.MemberType )
				{
					member = new Member( this, sn );
					break;
				}
			}

			return member;
		}


		/// <summary>
		/// Gets the Member object associated with the specified old username. If grace login is allowed for a changed username, it will get the user
		/// </summary>
		/// <param name="name">Name to look up the Member object with.</param>
		/// <returns>The first Member object associated with the specified name. May return null if the
		/// Member object does not exist in the collection. Also decrements the grace login counter. This must be called only during
		/// Authentication.</returns>
		public Member GetMemberByOldName( string name )
		{
			int RenameGraceLogin = 60 * 60 * 24 * 30; // 30 days 
			Member member = null;
			// check if the member was renamed and grace logins are remaining
			ICSList list = Search( "OldDN", "*", SearchOp.Exists );
			foreach ( ShallowNode sn in list )
			{
				member = new Member( this, sn );
				string Elements = member.OldDN;
				if( Elements == null ) continue;
				string [] CountAndUserNames = Elements.Split(new char[] { ':' });
				if ( CountAndUserNames != null && CountAndUserNames.Length > 1 )
				{
					string [] names = CountAndUserNames[2].Split( new char[] { ';' } );
					if( Array.IndexOf (names, name) >= 0 )
					{
						// user is found, now check for grace login period 
						DateTime counter = new DateTime(Convert.ToInt64( CountAndUserNames[0] ) ) ;
						if ( counter.AddSeconds( RenameGraceLogin ) > DateTime.Now )
						{
							// grace login is within 30 days so allow to login
							log.Debug("GMBON: grace login is within 30 days so allow to login: "+member.OldDN);
							return member;
						}
						else log.Debug("GMBON: expired the grace login of 30 days so don't allow");
					}
					else log.Debug("This name {0} does not exist inside the olddn list {1}",name, CountAndUserNames[2]);
				}
			}
			return null;
		}

		/// <summary>
		/// Gets the Member objects with the specified DN
		/// </summary>
		/// <param name="dn">Members DN value.</param>
		/// <returns>The first Member object associated with the specified DN. May return null if the
		/// Member object does not exist in the collection..</returns>
		public Member GetMemberByDN( string dn )
		{
			Member member = null;
			ICSList list = Search( "DN", dn, SearchOp.Equal );
			foreach ( ShallowNode sn in list )
			{
				if ( sn.Type == NodeTypes.MemberType )
				{
					member = new Member( this, sn );
					break;
				}
			}

			return member;
		}

		/// <summary>
		/// Gets the Member's all parent group id's 
		/// </summary>
		/// <param name="dn">Member ID</param>
		/// <returns>Returns all Members parent group id's
		/// </returns>
		public string[] GetMemberFamilyList( string userID )
		{
                        Member member = GetMemberByID(userID);
                        ArrayList entries = new ArrayList();
			if(member != null)
			{
                        	string groupList = String.Empty;
                        	try
                        	{
                                	groupList = member.Properties.GetSingleProperty( "UserGroups" ).Value as string;
                        	}
                        	catch{}
                        	if(groupList != String.Empty && groupList != "")
                       	 	{
                                	string[] groupArray = groupList.Split(new char[] { ';' });
                                	foreach(string group in groupArray)
                                	{
                                        	if(group != null && group != String.Empty && group != "")
                                        	{
                                                	Member tmpMember = GetMemberByDN(group);
							if(tmpMember != null)
							{
                                                		string[] subGroup = GetMemberFamilyList(tmpMember.ID);
								if(subGroup != null)
                                                			foreach(string groupID in subGroup)
                                                			{
                                                        			entries.Add( groupID );
                                                			}
							}
                                        	}
                                	}
                        	}
                        	entries.Add(userID);
			}

                        return (string[])entries.ToArray( typeof( string ) );
		}

		/// <summary>
		/// Gets the Member's all parent group DN List
		/// </summary>
		/// <param name="dn">Member ID</param>
		/// <returns>Returns all Members parent group DN's
		/// </returns>
		public string[] GetMemberFamilyDNList( string DN )
		{
                        Member member = GetMemberByDN(DN);
                        ArrayList entries = new ArrayList();
			if(member != null)
			{
                        	string groupList = String.Empty;
                        	try
                        	{
                                	groupList = member.Properties.GetSingleProperty( "UserGroups" ).Value as string;
                        	}
                        	catch{}
                        	if(groupList != String.Empty && groupList != "")
                       	 	{
                                	string[] groupArray = groupList.Split(new char[] { ';' });
                                	foreach(string group in groupArray)
                                	{
                                        	if(group != null && group != String.Empty && group != "")
                                        	{
                                                	string[] subGroup = GetMemberFamilyDNList(group);
							if(subGroup != null)
                                                		foreach(string groupID in subGroup)
                                                       			entries.Add( groupID );
							subGroup =  GetGroupsSubgroupList(group);
							if(subGroup != null)
                                                		foreach(string groupID in subGroup)
                                                       			entries.Add( groupID );
                                        	}
                                	}
                        	}
                        	entries.Add(DN);
			}

                        return (string[])entries.ToArray( typeof( string ) );
		}

		/// <summary>
		/// Gets the groupids of a deleted member i.e. groups which he belonged to before deletion
		/// this method is useful after deleted user is disabled in ifolder domain and all his group belonging informations are removed
		/// </summary>
		/// <param name="dn">Member ID</param>
		/// <returns>Returns all Members old group id's (old group ids means that the user might be a member of a group but user was deleted, so we store it in temp property as e-dir does not give this information)
		/// </returns>
		public string[] GetDeletedMembersGroupList( string userID )
		{
                        Member member = GetMemberByID(userID);
                        ArrayList entries = new ArrayList();
			if(member != null)
			{
                        	string tempGroupList = String.Empty;
                        	try
                        	{
                                	tempGroupList = member.Properties.GetSingleProperty( "TempUserGroups" ).Value as string;
                        	}
                        	catch{}
				log.Debug("tmp GetDeletedMembersGroupList , the groupList got here is : "+tempGroupList);
                        	if(tempGroupList != String.Empty && tempGroupList != "")
                       	 	{
                                	string[] groupArray = tempGroupList.Split(new char[] { ';' });
                                	foreach(string group in groupArray)
                                	{
                                        	if(group != null && group != String.Empty && group != "")
                                        	{
                                                	Member tmpMember = GetMemberByDN(group);
							if(tmpMember != null)
							{
                                                		string[] subGroup = GetMemberFamilyList(tmpMember.ID);
								if(subGroup != null)
                                                			foreach(string groupID in subGroup)
                                                			{
                                                        			entries.Add( groupID );
                                                			}
							}
                                        	}
                                	}
                        	}
                        	entries.Add(userID);
			}

                        return (string[])entries.ToArray( typeof( string ) );
		}

		/// <summary>
		/// Gets the Groups Member list
		/// </summary>
		/// <param name="dn">Member ID</param>
		/// <returns>Returns all Groups Members id list
		/// </returns>
		public string[] GetGroupsMemberList( string userID )
		{
                        Member member = GetMemberByID(userID);
                        ArrayList entries = new ArrayList();
                        string memberList = String.Empty;
			if(member != null)
			{
                        	try
                        	{
					MultiValuedList mvl = member.Properties.GetProperties( "MembersList" );
					if( mvl != null && mvl.Count > 0)
					{
						foreach( Property p in mvl )
						{
							if( p != null)
							{
								Member tmpMember = GetMemberByDN(p.Value as string);
								if(tmpMember != null)
								{
									string[] subGroup = GetGroupsMemberList(tmpMember.ID);
									if(subGroup != null)
									{
										foreach(string groupID in subGroup)
										{
											entries.Add( groupID );
										}
									}
								}	
							}
						}
					
					}
                        	}
                        	catch{}
                        	entries.Add(userID);
			}

                        return (string[])entries.ToArray( typeof( string ) );
		}

		/// <summary>
		/// Gets the Groups subgroup DN list
		/// </summary>
		/// <param name="dn">Parent group DN</param>
		/// <returns>Returns all Groups subgroup DN list
		/// </returns>
		public string[] GetGroupsSubgroupList( string DN)
		{
                        Member member = GetMemberByDN(DN);
                        ArrayList entries = new ArrayList();
                        string memberList = String.Empty;
			if(member != null && member.GroupType != null)
			{
                        	try
                        	{
					MultiValuedList mvl = member.Properties.GetProperties( "MembersList" );
					if( mvl != null && mvl.Count > 0)
					{
						foreach( Property p in mvl )
						{
							if( p != null)
							{
								Member tmpMember = GetMemberByDN(p.Value as string);
								if(tmpMember != null && tmpMember.GroupType != null)
								{
									string subgroupDN = null;
									try
									{
										subgroupDN = tmpMember.Properties.GetSingleProperty( "DN" ).Value as string;
									}
									catch{}	
									if(subgroupDN != null)
									{
										string[] subGroup = GetGroupsSubgroupList(subgroupDN);
										if(subGroup != null)
										{
											foreach(string groupDN in subGroup)
											{
												entries.Add( groupDN );
											}
										}
									}
								}	
							}
						}
					
					}
                        	}
                        	catch{}
                        	entries.Add(DN);
			}

                        return (string[])entries.ToArray( typeof( string ) );
		}

		/// <summary>
		/// Gets the Member objects with the specified access rights.
		/// </summary>
		/// <param name="rights">The access rights to search Members with.</param>
		/// <returns>An ICSEnumerator object that will enumerate the member list. The ICSList object
		/// will contain ShallowNode objects that represent Member objects.</returns>
		public ICSList GetMembersByRights( Access.Rights rights )
		{
			return Search( PropertyTags.Ace, rights.ToString(), SearchOp.Ends );
		}

		/// <summary>
		/// Gets the list of Member objects for this collection object.
		/// </summary>
		/// <returns>An ICSEnumerator object that will enumerate the member list. The ICSList object
		/// will contain ShallowNode objects that represent Member objects.</returns>
		public ICSList GetMemberList()
		{
                        Store store = Store.GetStore();
                        Domain domain = store.GetDomain(store.DefaultDomain);

			if( this.ID == domain.ID )
			{
				Simias.Storage.SearchPropertyList SearchPrpList = new Simias.Storage.SearchPropertyList();
				SearchPrpList.Add(BaseSchema.ObjectName, "*", SearchOp.Begins);
				SearchPrpList.Add(BaseSchema.ObjectType, NodeTypes.MemberType, SearchOp.Equal);
				SearchPrpList.Add("DN","*", SearchOp.Exists);
				return domain.Search(SearchPrpList);
			}
			else
				return Search(BaseSchema.ObjectType, NodeTypes.MemberType, SearchOp.Equal);
		}

		/// <summary>
		/// Gets a Node object for the specified identifier.
		/// </summary>
		/// <param name="nodeID">Identifier uniquely naming the node.</param>
		/// <returns>Node object for the specified identifier.</returns>
		public Node GetNodeByID( string nodeID )
		{
			// Normalize the node ID.
			nodeID = nodeID.ToLower();

			// First try to get the node from the cache.
			Node node = store.Cache.Get( id, nodeID );
			if ( node == null )
			{
				// Call the provider to get an XML string that represents this node.
				XmlDocument document = store.StorageProvider.GetRecord( nodeID, id );
				if ( document != null )
				{
					// Construct a temporary Node object from the DOM.
					node = Node.NodeFactory( store, document );

					// Add the node object to the cache.
					store.Cache.Add( this, node );
				}
			}

			return node;
		}	

                public Node GetNodeByPath( string entryPath)
                {
                         ICSList children = this.Search(PropertyTags.FileSystemPath, entryPath, SearchOp.Equal);
                        Node n = null;
                         foreach(ShallowNode sn in children)
                         {
                                 Node child = this.GetNodeByID(sn.ID);
 
                                 if (child.IsBaseType(NodeTypes.FileNodeType) || child.IsBaseType(NodeTypes.DirNodeType))
                                 {
                                         n = child;
                                         break;
                                 }
                         }
 
                         return n;
 
                }


        /// <summary>
        /// Clears the node cache
        /// </summary>
        /// <param name="nodeID">Node ID of the node to be cleared from Cache</param>
		public void ClearNodeCache( string nodeID )
		{
			store.Cache.Remove( this.ID, nodeID );
		}	

		/// <summary>
		/// Get all Node objects that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name for the Node object(s).</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Node object(s)
		/// that that have the specified name.</returns>
		public ICSList GetNodesByName( string name )
		{
			return Search( BaseSchema.ObjectName, name, SearchOp.Equal );
		}

		/// <summary>
		/// Get all Node objects that have the specified type.
		/// </summary>
		/// <param name="typeString">A string containing the type for the Node object(s).</param>
		/// <returns>An ICSList object containing the ShallowNode objects that represent the Node object(s)
		/// that that have the specified type.</returns>
		public ICSList GetNodesByType( string typeString )
		{
			return Search( PropertyTags.Types, typeString , SearchOp.Equal );
		}

		/// <summary>
		/// Gets the Node object that the collision property represents.
		/// </summary>
		/// <param name="node">Node object that contains a collision property.</param>
		/// <returns>The Node object that caused the collision. Otherwise a null is returned.</returns>
		public Node GetNodeFromCollision( Node node )
		{
			Node collisionNode = null;

			// Get the collision property.
			Property p = node.Properties.GetSingleProperty( PropertyTags.Collision );
			if ( p != null )
			{
				// Get a list of collisions.
				ICSEnumerator e = new CollisionList( p.Value as XmlDocument ).GetEnumerator() as ICSEnumerator;
				if ( e.MoveNext() )
				{
					Collision c = e.Current as Collision;
					if ( c.Type == CollisionType.Node )
					{
						XmlDocument document = new XmlDocument();
						document.LoadXml( c.ContextData );
						collisionNode = Node.NodeFactory( StoreReference, document );
					}
				}

				e.Dispose();
			}

			return collisionNode;
		}

		/// <summary>
		/// Gets the DirNode object that represents the root directory in the collection.
		/// </summary>
		/// <returns>A DirNode object that represents the root directory in the Collection. A null may
		/// be returned if no root directory has been specified for the Collection.</returns>
		public DirNode GetRootDirectory()
		{
			DirNode rootDir = null;

			ICSList results = Search( PropertyTags.Root, Syntax.String );
			foreach ( ShallowNode shallowNode in results )
			{
				rootDir = new DirNode( this, shallowNode );
				break;
			}

			return rootDir;
		}

		/// <summary>
		/// Gets the first Node object that matches the specified name.
		/// </summary>
		/// <param name="name">A string containing the name for the Node object.</param>
		/// <returns>The first Node object that matches the specified name.  A null is returned if no
		/// matching Node object is found.</returns>
		public Node GetSingleNodeByName( string name )
		{
			Node node = null;
			ICSList nodeList = GetNodesByName( name );
			foreach( ShallowNode shallowNode in nodeList )
			{
				node = Node.NodeFactory( this, shallowNode );
				break;
			}

			return node;
		}

		/// <summary>
		///  Gets the first Node object that corresponds to the specified type.
		/// </summary>
		/// <param name="typeString">String that contains the type of the node.</param>
		/// <returns>The first Node object that corresponds to the specified node path name.  A null
		/// is returned if no matching Node object is found.</returns>
		public Node GetSingleNodeByType( string typeString )
		{
			Node node = null;
			ICSList nodeList = GetNodesByType( typeString );
			foreach ( ShallowNode shallowNode in nodeList )
			{
				node = Node.NodeFactory( this, shallowNode );
				break;
			}

			return node;
		}

		/// <summary>
		/// Returns whether the collection has collisions.
		/// </summary>
		/// <returns>True if the collection contains collisions, otherwise false is returned.</returns>
		public bool HasCollisions()
		{
			ICSEnumerator e = GetCollisions().GetEnumerator() as ICSEnumerator;
			bool hasCollisions = e.MoveNext();
			e.Dispose();
			return hasCollisions;
		}

		/// <summary>
		/// Returns whether the specified Node object has collisions.
		/// </summary>
		/// <param name="node">Node object to check for collisions.</param>
		/// <returns>True if the Node object contains collisions, otherwise false is returned.</returns>
		public bool HasCollisions( Node node )
		{
			return ( node.Properties.GetSingleProperty( PropertyTags.Collision ) != null ) ? true : false;
		}

		/// <summary>
		/// Impersonates the specified identity, if the user ID is verified.
		/// </summary>
		/// <param name="member">Member object to impersonate.</param>
		public void Impersonate( Member member )
		{
			accessControl.Impersonate( member );
		}

		/// <summary>
		/// Readies a Node object for import into this Collection.
		/// </summary>
		/// <param name="node">Node to import into this Collection.</param>
		/// <param name="isMaster">Indicates whether Node object is being imported on the master or slave store.</param>
		/// <param name="expectedIncarnation">The expected value of the Node object's incarnation number. If
		/// the Node object incarnation value is not equal to the expected value, a collision is the result.</param>
		public void ImportNode( Node node, bool isMaster, ulong expectedIncarnation )
		{
			// Set the current state of the node indicating that it is being imported.
			node.Properties.State = PropertyList.PropertyListState.Import;
			node.ExpectedIncarnation = expectedIncarnation;
			node.IsMaster = isMaster;

			// Strip any local properties that may exist on the Node object.
			node.Properties.StripLocalProperties();
		}

		/// <summary>
		/// Checks whether the specified user has sufficient access rights for an operation.
		/// </summary>
		/// <param name="member">Member object to check access for.</param>
		/// <param name="desiredRights">Desired access rights.</param>
		/// <returns>True if the user has the desired access rights, otherwise false.</returns>
		public bool IsAccessAllowed( Member member, Access.Rights desiredRights )
		{
			return accessControl.IsAccessAllowed( member, desiredRights );
		}

		/// <summary>
		/// Checks if the collection is locked by the specified lockString.
		/// </summary>
		/// <param name="lockString">String to test lock with.</param>
		/// <returns>True if the collection lock is locked by the specified lockString.</returns>
		public bool IsLockedByName( string lockString )
		{
			lock ( lockTable )
			{
				string ls = lockTable[ id ] as string;
				return ( ( ls != null ) && ( ls == lockString ) ) ? true : false;
			}
		}

		/// <summary>
		/// Gets whether the specified member has sufficient rights to share this collection.
		/// </summary>
		/// <param name="member">Member object contained by this collection.</param>
		public bool IsShareable( Member member )
		{
			return ( member.ValidateAce.Rights == Access.Rights.Admin ) ? true : false;
		}

		/// <summary>
		/// Returns whether specified Node object is the specified type.
		/// </summary>
		/// <param name="node">Node object to check type.</param>
		/// <param name="typeString">Type of Node object.</param>
		/// <returns>True if Node object is the specified type, otherwise false is returned.</returns>
		[Obsolete("This method has moved to the Node class. Use Node.IsType(string)")]
		public bool IsType( Node node, string typeString )
		{
			bool isType = false;
			MultiValuedList mvl = node.Properties.FindValues( PropertyTags.Types );
			foreach( Property p in mvl )
			{
				if ( p.ToString() == typeString )
				{
					isType = true;
					break;
				}
			}

			return isType;
		}

		/// <summary>
		/// Returns whether specified Node object is the specified base type.
		/// </summary>
		/// <param name="node">ShallowNode object to check type.</param>
		/// <param name="typeString">Type of Node object.</param>
		/// <returns>True if Node object is the specified type, otherwise false is returned.</returns>
		[Obsolete("This method has moved to the ShallowNode class. Use ShallowNode.IsBaseType(string)")]
		public bool IsBaseType( ShallowNode node, string typeString )
		{
			return ( node.Type == typeString ? true : false );
		}

		/// <summary>
		/// Returns whether specified Node object is the specified base type.
		/// </summary>
		/// <param name="node">Node object to check type.</param>
		/// <param name="typeString">Type of Node object.</param>
		/// <returns>True if Node object is the specified type, otherwise false is returned.</returns>
		[Obsolete("This method has moved to the Node class. Use Node.IsBaseType(string)")]
		public bool IsBaseType( Node node, string typeString )
		{
			return ( node.Type == typeString ? true : false );
		}

		/// <summary>
		/// Locks the collection so that changes to the collection are not allowed.
		/// </summary>
		/// <param name="lockString">Used to personalize the lock so that it can
		/// only be unlocked by using the same string.</param>
		public void Lock( string lockString )
		{
			// User must not be impersonating.
			if ( accessControl.IsImpersonating )
			{
				throw new AccessException( this, GetCurrentMember(), "Insufficent rights to set a lock on this collection." );
			}

			lock ( lockTable )
			{
				// Does a lock already exist?
				string ls = lockTable[ id ] as string;
				if ( ls == null )
				{
					// Create the lock.
					lockTable[ id ] = lockString;
				}
				else
				{
					// Don't allow a different lock.
					if ( ls != lockString )
					{
						throw new AlreadyLockedException( this );
					}
				}
			}

			// Set this lock string in this instance so that changes can be committed using
			// this object instance.
			this.lockString = lockString;
		}

		/// <summary>
		/// Gets a new copy of the Collection object data from the database. All changed Collection object data
		/// will be lost.
		/// </summary>
		/// <returns>The Collection object that was refreshed.</returns>
		public Collection Refresh()
		{
			return Refresh( this ) as Collection;
		}

		/// <summary>
		/// Gets a new copy of the Node object data from the database. All changed Node object data
		/// will be lost.
		/// </summary>
		/// <param name="node">Node object to refresh.</param>
		/// <returns>The Node object that was refreshed.</returns>
		public Node Refresh( Node node )
		{
			// Check and see if the node is in the node cache first.
			Node tempNode = store.Cache.Get( id, node.ID );
			XmlDocument document = ( tempNode != null ) ? tempNode.Properties.PropertyDocument : store.StorageProvider.GetRecord( node.ID, id );
			if ( document != null )
			{
				XmlElement element = document.DocumentElement[ XmlTags.ObjectTag ];

				node.Name = element.GetAttribute( XmlTags.NameAttr );
				node.BaseType = element.GetAttribute( XmlTags.TypeAttr );
				node.InternalList = new PropertyList( document );
				node.IncarnationUpdate = 0;

				// See if node needs to be added to the cache.
				if ( tempNode == null )
				{
					store.Cache.Add( this, node );
				}
			}

			return node;
		}

		/// <summary>
		/// Removes the specified class type from the Types property.
		/// </summary>
		/// <param name="node">Node object to remove type from.</param>
		/// <param name="type">String object containing class type to remove.</param>
		public void RemoveType( Node node, string type )
		{
			if ( NodeTypes.IsNodeType( type ) )
			{
				throw new InvalidOperationException( "Cannot remove base type of Node object." );
			}

			// Get the multi-valued property and search for the specific value.
			MultiValuedList mvl = node.Properties.GetProperties( PropertyTags.Types );
			foreach ( Property p in mvl )
			{
				if ( p.ToString() == type )
				{
					p.DeleteProperty();
					break;
				}
			}
		}

		/// <summary>
		/// Resolves a collision on the specified Node object.
		/// </summary>
		/// <param name="node">Node object that contains a collision.</param>
		/// <param name="incarnationValue">Remote local incarnation value.</param>
		/// <param name="resolveLocal">If true, the local Node becomes authoritative. Otherwise the 
		/// remote Node object becomes authoritative.</param>
		/// <returns>Returns the authoritative Node object.</returns>
		public Node ResolveCollision( Node node, ulong incarnationValue, bool resolveLocal )
		{
			Node resNode;
			
			if ( resolveLocal )
			{
				resNode = node;
				resNode.Properties.State = PropertyList.PropertyListState.Internal;
				resNode.MergeCollisions = false;
				resNode.Properties.ModifyNodeProperty( PropertyTags.MasterIncarnation, incarnationValue );
				resNode.Properties.ModifyNodeProperty( PropertyTags.LocalIncarnation, incarnationValue + 1 );
				DeleteCollision( resNode );
			}
			else
			{
				resNode = GetNodeFromCollision( node );
				resNode.Properties.State = PropertyList.PropertyListState.Import;
				resNode.SkipCollisionCheck = true;
				resNode.IncarnationUpdate = incarnationValue;
				resNode.ExpectedIncarnation = 0;
			}

			return resNode;
		}

		/// <summary>
		/// Readies a Node object for restoration from backup into this Collection.
		/// </summary>
		/// <param name="node">Node to import into this Collection.</param>
		public void RestoreNode( Node node )
		{
			ulong masterIncarnation = 0;
			ulong localIncarnation = 0;

			// Set the current state of the node indicating that it is being restored.
			node.Properties.State = PropertyList.PropertyListState.Restore;

			// See if there is an existing node so the incarnation values can be set properly.
			node.DiskNode = GetNodeByID( node.ID );
			if ( node.DiskNode != null )
			{
				masterIncarnation = node.DiskNode.MasterIncarnation;
				localIncarnation = node.DiskNode.LocalIncarnation;
			}

			node.Properties.ModifyNodeProperty( PropertyTags.MasterIncarnation, masterIncarnation );
			node.Properties.ModifyNodeProperty( PropertyTags.LocalIncarnation, localIncarnation );
		}

		/// <summary>
		/// Reverts back to the previous impersonating identity.
		/// </summary>
		public void Revert()
		{
			accessControl.Revert();
		}

		/// <summary>
		/// Searches the collection for the specified property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Property name to search for.</param>
		/// <param name="propertySyntax">Syntax of property to search for.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, Syntax propertySyntax )
		{
			if ( propertySyntax == Syntax.Uri )
			{
				// The Uri object must contain a valid path or it cannot be constructed. Put in a bogus path
				// so that it can be constructed. The value will be ignored in the search.
				return new ICSList( new NodeEnumerator( this, new Property( propertyName, new Uri( Directory.GetCurrentDirectory() ) ), SearchOp.Exists ) );
			}
			else if ( propertySyntax == Syntax.XmlDocument )
			{
				XmlDocument document = new XmlDocument();
				document.LoadXml( "<Dummy/>" );
				return new ICSList( new NodeEnumerator( this, new Property( propertyName, document ), SearchOp.Exists ) );
			}
			else
			{
				return new ICSList( new NodeEnumerator( this, new Property( propertyName, propertySyntax, String.Empty ), SearchOp.Exists ) );
			}
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="property">Property object containing the value to search for.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( Property property, SearchOp searchOperator )
		{
			return new ICSList( new NodeEnumerator( this, property, searchOperator ) );
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, object propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, string propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified sbyte property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">sbyte value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, sbyte propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified byte property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">byte value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, byte propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified short property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">short value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, short propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified ushort property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">ushort value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, ushort propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified int properties.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">int value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, int propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified uint property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">uint value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, uint propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified long property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">long value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, long propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified ulong property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">ulong value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, ulong propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified char property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">char value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, char propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified float property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">float value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, float propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified bool property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">bool value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, bool propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified DateTime property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">DateTime value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, DateTime propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified Uri property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Uri value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, Uri propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified XmlDocument property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">XmlDocument value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, XmlDocument propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified TimeSpan property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">TimeSpan value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, TimeSpan propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified Relationship property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Relationship value to match.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, Relationship propertyValue )
		{
			// Since GUIDs are unique, only search for the Node object GUID. Don't take the time to compare
			// the Collection object GUID.
			return Search( new Property( propertyName, propertyValue ), SearchOp.Equal );
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="property">Property objects list containing the value to search for.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( SearchPropertyList PropList)
		{
			return new ICSList( new NodeEnumerator( this, PropList) );
		}

		/// <summary>
		/// Sets the Types property to the specified class type.
		/// </summary>
		/// <param name="node">Node object to set type on.</param>
		/// <param name="type">String object containing class type to set.</param>
		public void SetType( Node node, string type )
		{
			if ( NodeTypes.IsNodeType( type ) )
			{
				throw new InvalidOperationException( "Cannot set a base type of a Node object." );
			}

			// See if the node already has this type.
			if ( !node.IsType( type ) )
			{
				// Set the new type.
				node.Properties.AddNodeProperty( PropertyTags.Types, type );
			}
		}

		/// <summary>
		/// Unlocks a previously locked collection.
		/// </summary>
		/// <param name="lockString">The string used to lock the collection. The collection can
		/// only be unlocked with the proper lockString.</param>
		public void Unlock( string lockString )
		{
			// User must not be impersonating.
			if ( accessControl.IsImpersonating )
			{
				throw new AccessException( this, GetCurrentMember(), "Insufficent rights to remove lock on this collection." );
			}

			lock ( lockTable )
			{
				// Is there is a lock for this collection?
				string ls = lockTable[ id ] as string;
				if ( ( ls != null ) && ( ls == lockString ) )
				{
					lockTable.Remove( id );
				}
			}

			// Reset the local instance of the lock string.
			this.lockString = null;
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Gets an enumerator for all of the Node objects belonging to this collection.
		/// </summary>
		/// <returns>An IEnumerator object.</returns>
		public IEnumerator GetEnumerator()
		{
			return new NodeEnumerator( this, new Property( BaseSchema.CollectionId, id ), SearchOp.Equal );
		}

		/// <summary>
		/// Enumerator class for the node object that allows enumeration of specified node objects
		/// within the collection.
		/// </summary>
		protected class NodeEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// Indicates whether this object has been disposed.
			/// </summary>
			private bool disposed = false;

			/// <summary>
			/// Collection associated with this search.
			/// </summary>
			private Collection collection;

			/// <summary>
			/// Property containing the data to search for.
			/// </summary>
			private Property property;

			/// <summary>
			/// Type of search operation.
			/// </summary>
			private SearchOp queryOperator;

			/// <summary>
			/// Enumerator used to enumerate each returned item in the chunk enumerator list.
			/// </summary>
			private IEnumerator nodeListEnumerator;

			/// <summary>
			/// The internal enumerator to use to enumerate all of the child nodes belonging to this node.
			/// </summary>
			private Persist.IResultSet chunkIterator = null;

			/// <summary>
			/// Array where the query results are stored.
			/// </summary>
			private char[] results = new char[ 4096 ];
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the NodeEnumerator object.
			/// </summary>
			/// <param name="collection">Collection object that this enumerator belongs to.</param>
			/// <param name="property">Property object containing the data to search for.</param>
			/// <param name="queryOperator">Query operator to use when comparing value.</param>
			public NodeEnumerator( Collection collection, Property property, SearchOp queryOperator )
			{
				this.collection = collection;
				this.property = property;
				this.queryOperator = queryOperator;
				Reset();
			}

			/// <summary>
			/// Constructor for the NodeEnumerator object.
			/// </summary>
			/// <param name="collection">Collection object that this enumerator belongs to.</param>
			/// <param name="property">Property objects list containing the data to search for.</param>
			public NodeEnumerator( Collection collection, SearchPropertyList PropList)
			{
				this.collection = collection;
				Reset(PropList);
			} 
			#endregion

			#region Properties
			/// <summary>
			/// Gets the total number of objects contained in the search.
			/// </summary>
			public int Count
			{
				get { return chunkIterator.Count; }
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Sets the enumerator to its initial position, which is before
			/// the first element in the collection.
			/// </summary>
			public void Reset()
			{
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				// Release previously allocated chunkIterator.
				if ( chunkIterator != null )
				{
					chunkIterator.Dispose();
				}

				// Create a query object that will return a result set containing the children of this node.
				Persist.Query query = new Persist.Query( collection.id, property.Name, queryOperator, property.SearchString, property.Type );
				chunkIterator = collection.store.StorageProvider.Search( query );
				if ( chunkIterator != null )
				{
					// Get the first set of results from the query.
					int length = chunkIterator.GetNext( ref results );
					if ( length > 0 )
					{
						// Set up the XML document that we will use as the granular query to the client.
						XmlDocument nodeList = new XmlDocument();
						nodeList.LoadXml( new string( results, 0, length ) );
						nodeListEnumerator = nodeList.DocumentElement.GetEnumerator();
					}
					else
					{
						nodeListEnumerator = null;
					}
				}
				else
				{
					nodeListEnumerator = null;
				}
			}

			/// <summary>
			/// Sets the enumerator to its initial position, which is before
			/// the first element in the collection.
			/// </summary>
			public void Reset(SearchPropertyList searchPropList)
			{
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				// Release previously allocated chunkIterator.
				if ( chunkIterator != null )
				{
					chunkIterator.Dispose();
				}

				// Create a query object that will return a result set containing the children of this node.
				ArrayList queryList = new ArrayList();
				int cnt = 0;
				foreach(Property prop in searchPropList.PropList)
				{
					queryList.Add(new Persist.Query( collection.id, prop.Name, (SearchOp)searchPropList.SearchOpList[cnt++], prop.SearchString, prop.Type ));
				}
				chunkIterator = collection.store.StorageProvider.MQSearch( (Persist.Query[])queryList.ToArray(typeof(Persist.Query)) );
				if ( chunkIterator != null )
				{
					// Get the first set of results from the query.
					int length = chunkIterator.GetNext( ref results );
					if ( length > 0 )
					{
						// Set up the XML document that we will use as the granular query to the client.
						XmlDocument nodeList = new XmlDocument();
						nodeList.LoadXml( new string( results, 0, length ) );
						nodeListEnumerator = nodeList.DocumentElement.GetEnumerator();
					}
					else
					{
						nodeListEnumerator = null;
					}
				}
				else
				{
					nodeListEnumerator = null;
				}
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get
				{
					if ( disposed )
					{
						throw new DisposedException( this );
					}

					if ( nodeListEnumerator == null )
					{
						throw new InvalidOperationException( "Empty enumeration" );
					}

					return new ShallowNode( ( XmlElement )nodeListEnumerator.Current, collection.id );
				}
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element;
			/// false if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				bool moreData = false;

				if ( disposed )
				{
					throw new DisposedException( this );
				}

				// Make sure that there is data in the list.
				if ( nodeListEnumerator != null )
				{
					// See if there is anymore data left in this result set.
					moreData = nodeListEnumerator.MoveNext();
					if ( !moreData )
					{
						// Get the next page of the results set.
						int length = chunkIterator.GetNext( ref results );
						if ( length > 0 )
						{
							// Set up the XML document that we will use as the granular query to the client.
							XmlDocument nodeList = new XmlDocument();
							nodeList.LoadXml( new string( results, 0, length ) );
							nodeListEnumerator = nodeList.DocumentElement.GetEnumerator();

							// Move to the first entry in the document.
							moreData = nodeListEnumerator.MoveNext();
							if ( moreData == false )
							{
								// Out of data.
								nodeListEnumerator = null;
							}
						}
						else
						{
							// Out of data.
							nodeListEnumerator = null;
						}
					}
				}

				return moreData;
			}

			/// <summary>
			/// Set the cursor for the current search to the specified index.
			/// </summary>
			/// <param name="origin">The origin to move from.</param>
			/// <param name="offset">The offset to move the index by.</param>
			/// <returns>True if successful, otherwise false is returned.</returns>
			public bool SetCursor( IndexOrigin origin, int offset )
			{
				// Set the new index for the cursor.
				bool cursorSet = chunkIterator.SetIndex( origin, offset );
				if ( cursorSet )
				{
					// Get the next page of the results set.
					int length = chunkIterator.GetNext( ref results );
					if ( length > 0 )
					{
						// Set up the XML document that we will use as the granular query to the client.
						XmlDocument nodeList = new XmlDocument();
						nodeList.LoadXml( new string( results, 0, length ) );
						nodeListEnumerator = nodeList.DocumentElement.GetEnumerator();
					}
					else
					{
						// Out of data.
						nodeListEnumerator = null;
					}
				}

				return cursorSet;
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// Allows for quick release of managed and unmanaged resources.
			/// Called by applications.
			/// </summary>
			public void Dispose()
			{
				Dispose( true );
				GC.SuppressFinalize( this );
			}

			/// <summary>
			/// Dispose( bool disposing ) executes in two distinct scenarios.
			/// If disposing equals true, the method has been called directly
			/// or indirectly by a user's code. Managed and unmanaged resources
			/// can be disposed.
			/// If disposing equals false, the method has been called by the
			/// runtime from inside the finalizer and you should not reference
			/// other objects. Only unmanaged resources can be disposed.
			/// </summary>
			/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
			protected virtual void Dispose( bool disposing )
			{
				// Check to see if Dispose has already been called.
				if ( !disposed )
				{
					// Set disposed here to protect callers from accessing freed members.
					disposed = true;

					// If disposing equals true, dispose all managed and unmanaged resources.
					if ( disposing )
					{
						// Dispose managed resources.
						if ( chunkIterator != null )
						{
							chunkIterator.Dispose();
						}
					}
				}
			}

			/// <summary>
			/// Use C# destructor syntax for finalization code.
			/// This destructor will run only if the Dispose method does not get called.
			/// It gives your base class the opportunity to finalize.
			/// Do not provide destructors in types derived from this class.
			/// </summary>
			~NodeEnumerator()
			{
				Dispose( false );
			}
			#endregion
		}
		#endregion

        /// <summary>
        /// Download collection from server
        /// </summary>
        /// <param name="iFolderID">ID of iFolder to download</param>
        /// <param name="iFolderName">Name of the iFolder</param>
        /// <param name="DomainID">ID of Domain where iFolder is available</param>
        /// <param name="HostID">ID of the host</param>
        /// <param name="DirNodeID">ID of directory node</param>
        /// <param name="MemberUserID">User ID</param>
        /// <param name="colMemberNodeID">Collection's member node ID</param>
        /// <param name="iFolderLocalPath">Path of Local iFolder</param>
        /// <returns></returns>
		public static bool DownloadCollection(string iFolderID, string iFolderName, string DomainID, string HostID, string DirNodeID, string MemberUserID, string colMemberNodeID, string iFolderLocalPath, int sourceFileCount, int sourceDirCount)
		{
			string titleClass = "Simias.UserMovement.iFolderDataMove";
			string ReprovisionMethod = "MoveiFolder";
			Assembly Asmbly = Assembly.LoadWithPartialName(Simias.Service.Manager.UserMovementAssemblyName);
			Type types = Asmbly.GetType(titleClass);
			MethodInfo mInfo = types.GetMethod(ReprovisionMethod);
                        if (mInfo != null)
                        {
                                object[] prms = new object[10];
                                prms[0] = iFolderID;
                                prms[1] = iFolderName;
                                prms[2] = DomainID;
                                prms[3] = HostID;
                                prms[4] = DirNodeID;
                                prms[5] = MemberUserID;
                                prms[6] = colMemberNodeID;
                                prms[7] = iFolderLocalPath;
                                prms[8] = sourceFileCount;
                                prms[9] = sourceDirCount;
                                return ((bool)mInfo.Invoke(null, prms));
                        }
                        else
                                throw new Exception(String.Format("Unable to call Data move method for {0} ",iFolderID));
		}

        /// <summary>
        /// Download the collection locally
        /// </summary>
        /// <param name="iFolderID">ID of iFolder</param>
        /// <param name="iFolderName">Name of iFolder</param>
        /// <param name="DomainID">Domain ID where iFolder resides</param>
        /// <param name="HostID">ID of host where domain is available</param>
        /// <param name="DirNodeID">ID of directory node to download</param>
        /// <param name="MemberUserID">Member user id</param>
        /// <param name="colMemberNodeID">ID of collection member node</param>
        /// <param name="iFolderLocalPath">Path of iFolder</param>
        /// <returns></returns>
		public static bool DownloadCollectionLocally(string iFolderID, string iFolderName, string DomainID, string HostID, string DirNodeID, string MemberUserID, string colMemberNodeID, string iFolderLocalPath, int oldHomeFileCount, int oldHomeDirCount)
		{
			bool status = false;
                        try
                        {
                                log.Debug("DownloadCollection: In DownloadiFolder...");
                                Store store = Store.GetStore ();
                                ArrayList commitList = new ArrayList();
                                string iFolderPath = store.GetStoreUnmanagedPath(iFolderID);
                                iFolderLocalPath = System.IO.Path.Combine( iFolderPath, iFolderName);
				log.Debug("The unmanaged path is: {0}", iFolderLocalPath);
				Collection iFolderCol = store.GetCollectionByID(iFolderID);
				Domain domain = store.GetDomain(DomainID);
				Member iFolderOwner = domain.GetMemberByID (MemberUserID);
				Member CurrentMember = domain.GetCurrentMember();
				Member newDomainMember = new Member( CurrentMember.Name, CurrentMember.UserID, Simias.Storage.Access.Rights.Admin);
				Member newOwnerMember = new Member( iFolderOwner.Name, colMemberNodeID, iFolderOwner.UserID, Simias.Storage.Access.Rights.Admin, null);
				if( iFolderCol == null )
				{
					log.Debug("Setting the collection {0} for moving the data. ", iFolderName);
					iFolderCol = CreateProxyCollection( store, iFolderName, iFolderID, DomainID, HostID, newOwnerMember, newDomainMember, iFolderLocalPath, DirNodeID);
				}
				else
				{
					// Sync has by some chance failed during previous cycle...
					// Check if the current host is a member or not and add the current host as member...
					log.Debug("The user move has failed for the collection {0} in previous cycle. adding the host member again.", iFolderCol.Name);
					RemoveFromCatalogTable( iFolderID); // On destination server, no need of maintaining ignore list in catalog's hashtbl
					try
					{
						Member currHostMember = iFolderCol.GetMemberByID(newDomainMember.UserID);
						if( currHostMember == null)
						{
							log.Debug("currHostMember is null. Adding again...");
							newDomainMember.Proxy = true;
							commitList.Add(iFolderCol);
							commitList.Add(newDomainMember);
							iFolderCol.DataMovement = true;
							iFolderCol.Role = SyncRoles.Slave;
							iFolderCol.Commit((Node[]) commitList.ToArray(typeof(Node)));
							iFolderCol.Commit();
						}
						else
							log.Debug("currhost exists...");
					}
					catch(Exception ex)
					{
						log.Debug("Exception while adding the host member again. {0}--{1}", ex.Message, ex.StackTrace);
					}
				}
				log.Debug("DownloadCollection: About to start sync...");
				CollectionSyncClient syncClient = new CollectionSyncClient(iFolderID, new TimerCallback( TimerFired ) );
				syncEvent.WaitOne();
				lock(CollectionSyncClient.MapObject)
				{
					Simias.Sync.CollectionSyncClient.ServerSyncStatus  |= Simias.Sync.CollectionSyncClient.StateMap.UserMoveSyncStarted;
					Simias.Sync.CollectionSyncClient.ServerSyncStatus  &= ~Simias.Sync.CollectionSyncClient.StateMap.UserMoveSyncFinished;
				}
				syncClient.SyncNow();
				lock(CollectionSyncClient.MapObject)
				{
					Simias.Sync.CollectionSyncClient.ServerSyncStatus  &= ~Simias.Sync.CollectionSyncClient.StateMap.UserMoveSyncStarted;
					Simias.Sync.CollectionSyncClient.ServerSyncStatus  |= Simias.Sync.CollectionSyncClient.StateMap.UserMoveSyncFinished;
				}

				uint count = 0;
				syncClient.GetSyncCount(out count);
				bool CollSyncStatus = syncClient.GetCollectionSyncStatus();
				log.Debug("After Sync now WonkArray Count is {0} CollSyncStatus is {1}", count.ToString(), CollSyncStatus.ToString());
				if( count == 0 && CollSyncStatus == true)
				{
					// before declaring sync successful, match the actual no of files on old home and new home server
					int status2 = 1; // successful matching of number of files
					DirNode dirNode = iFolderCol.GetRootDirectory();
					if (dirNode != null)
					{	
						int newHomeFileCount=0;
						int newHomeDirCount=0;
						string UnManagedPath = dirNode.GetFullPath(iFolderCol);			
						DirectoryInfo d = new DirectoryInfo(UnManagedPath);
						GetDirAndFileCount(d, ref newHomeFileCount, ref newHomeDirCount);
						newHomeDirCount++;
						log.Debug("On New Home Server, Number of file = {0} and number of dirs = {1} for iFolder {2}", newHomeFileCount, newHomeDirCount, iFolderCol.Name);

						// call simias webservice to get same number on old home server

						log.Debug("Old HomeServer: Number of file = {0} and number of dirs = {1} for iFolder {2}",oldHomeFileCount, oldHomeDirCount, iFolderCol.Name);
						
						if( newHomeFileCount < oldHomeFileCount || newHomeDirCount < oldHomeDirCount )
						{
							status2 = -1;
						}
						else
							log.Debug("New HomeServer: Successful match of number of files and dir, so all files/dirs synced");


						if( status2 == -1 )
						{
							log.Debug("Number of files downloaded does not match with actual file on this server, so iFoldermove failed for this iFolder, it will retry after deleting/recreating ifolder");

							// add this ID to catalog's ignore hastable so that delete event will be ignored.
							AddToCatalogTable( iFolderID);
							iFolderCol.Commit(iFolderCol.Delete());

							// Re writing the code which was above to create catalog entries on old home server and sync

							log.Debug("Re-try: Setting the collection {0} for moving the data. ", iFolderName);
							iFolderCol = CreateProxyCollection( store, iFolderName, iFolderID, DomainID, HostID, newOwnerMember, newDomainMember, iFolderLocalPath, DirNodeID);
							return false;
						}
						else if( status2 == 1 )
						{
							log.Debug("After WebService call: Number of files downloaded matched successfully ");
							log.Debug("DownloadCollection: Sync completed successfull, Removing local properties...");
							iFolderCol.DataMovement = false;
							iFolderCol.Commit();
							Member tmpMember = iFolderCol.GetMemberByID(newDomainMember.UserID);
							if(tmpMember != null && !tmpMember.IsOwner)
								iFolderCol.Commit( iFolderCol.Delete( tmpMember ) );
							log.Debug("Removed the datamove flag.");
							return true;
						}

					}
					else 
					{
						// else return false because getrootdirectory null means no collection still synced
						log.Debug("DownloadCollectionLocally: else return false because getrootdirectory null means no collection still synced ");
						return false;
					}
				}
				else
				{
					log.Debug("DownloadCollection: Sync not successfull. Count: {0}", count);
					return false;
				}
                        }
                        catch(Exception ex)
                        {
                                log.Debug("Exception in downloading the iFolder... {0}--{1}", ex.Message, ex.StackTrace);
				lock(CollectionSyncClient.MapObject)
				{
					Simias.Sync.CollectionSyncClient.ServerSyncStatus  &= ~Simias.Sync.CollectionSyncClient.StateMap.UserMoveSyncStarted;
					Simias.Sync.CollectionSyncClient.ServerSyncStatus  |= Simias.Sync.CollectionSyncClient.StateMap.UserMoveSyncFinished;
				}
				
                        }
			return status;		
		}

		// moved the creating proxy collection part to a seperate methos since it was called from 2 places
		public static Collection CreateProxyCollection(Store store, string iFolderName, string iFolderID, string DomainID, string HostID, Member newOwnerMember, Member newDomainMember, string iFolderLocalPath, string DirNodeID)
		{
				log.Debug("CreateProxyCollection: Entered");
                                ArrayList commitList = new ArrayList();
                              	Collection iFolderCol = new Collection(store, iFolderName, iFolderID, DomainID);
                                iFolderCol.HostID = HostID;
       	                        commitList.Add(iFolderCol);
				newOwnerMember.IsOwner = true;
				newOwnerMember.Proxy = true;
				newDomainMember.Proxy = true;
				commitList.Add(newOwnerMember);
				commitList.Add(newDomainMember);
				log.Debug("DownloadCollection: Preparing dir node...");
       	                        DirNode dNode = new DirNode(iFolderCol, iFolderLocalPath, DirNodeID);
                       	        if (!Directory.Exists(iFolderLocalPath))
                               	        Directory.CreateDirectory(iFolderLocalPath);
                                dNode.Proxy = true;
       	                        commitList.Add(dNode);
               	                iFolderCol.Proxy = true;
				iFolderCol.DataMovement = true;
				iFolderCol.Role = SyncRoles.Slave;
                               	iFolderCol.Commit((Node[]) commitList.ToArray(typeof(Node)));
				iFolderCol.Commit();
				log.Debug("CreateProxyCollection: created the proxy collection entry..Returning");
				return iFolderCol;
		}

		// add the collectionID into MovingCollections Hashtable used in catalog.cs so that delete event for those ifolders will be ignored which
		// fail to sync full in one cycle.
		public static void AddToCatalogTable( string iFolderID)
		{
			string titleClass = "Simias.Server.Catalog";
			string AddMethod = "AddCollectionForMovement";
			Assembly Asmbly = Assembly.LoadWithPartialName(Simias.Service.Manager.CatalogAssemblyName);
			Type types = Asmbly.GetType(titleClass);
			MethodInfo mInfo = types.GetMethod(AddMethod);
                        if (mInfo != null)
                        {
                                object[] prms = new object[2];
                                prms[0] = iFolderID;
                                prms[1] = null;
                                mInfo.Invoke(null, prms);
				log.Debug("AddToCatalogTable: Added this iFolder temporarily into hastable to ignore delete event..");	
			}
			else throw new Exception("AddToCatalog: Unable to call catalog method from Collection.cs for :"+iFolderID);
		}

		// remove the collectionID into MovingCollections Hashtable used in catalog.cs so that delete event for those ifolders will be ignored which
		// fail to sync full in one cycle.
		public static void RemoveFromCatalogTable( string iFolderID)
		{
			string titleClass = "Simias.Server.Catalog";
			string AddMethod = "RemoveCollectionForMovement";
			Assembly Asmbly = Assembly.LoadWithPartialName(Simias.Service.Manager.CatalogAssemblyName);
			Type types = Asmbly.GetType(titleClass);
			MethodInfo mInfo = types.GetMethod(AddMethod);
                        if (mInfo != null)
                        {
                                object[] prms = new object[1];
                                prms[0] = iFolderID;
                                mInfo.Invoke(null, prms);
				log.Debug("RemoveFromCatalogTable: Removed this iFolder from catalog hastable to bring in orig state..");	
				
			}
			else throw new Exception("RemoveFromCatalog: Unable to call catalog method from Collection.cs for :"+iFolderID);
		}

                // count total no of files and dirs in this collection (goto actual storage and count)
                public static void GetDirAndFileCount(DirectoryInfo d, ref int filecount, ref int dircount)
                {
                        FileInfo[] fis = d.GetFiles();
			filecount += fis.Length;

                        DirectoryInfo[] dis = d.GetDirectories();
                        foreach (DirectoryInfo di in dis)
                        {
                                dircount++;
                                GetDirAndFileCount(di, ref filecount, ref dircount);
                        }
                }



        /// <summary>
        /// Callback method for timer
        /// </summary>
        /// <param name="collectionClient">Collection client object to be handled when timer triggers</param>
		public static void TimerFired( object collectionClient )
		{
			while(CollectionSyncClient.running || ((Simias.Sync.CollectionSyncClient.ServerSyncStatus & Simias.Sync.CollectionSyncClient.StateMap.CatalogSyncStarted ) == Simias.Sync.CollectionSyncClient.StateMap.CatalogSyncStarted) || ((Simias.Sync.CollectionSyncClient.ServerSyncStatus & Simias.Sync.CollectionSyncClient.StateMap.DomainSyncStarted ) == Simias.Sync.CollectionSyncClient.StateMap.DomainSyncStarted) )
				Thread.Sleep(1000);
			syncEvent.Set();
		}
	}
}
