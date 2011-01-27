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
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Event;
using Simias.Policy;
using Simias.Storage.Provider;
using Simias.Sync;
using Simias.Sync.Delta;
using Persist = Simias.Storage.Provider;



namespace Simias.Storage
{
	/// <summary>
	/// This is the top level object for the Collection Store.  The Store object can contain multiple 
	/// collection objects.
	/// </summary>
	public sealed class Store : IEnumerable
	{
		#region Class Members
		private SimiasAccessLogger accessLog = new SimiasAccessLogger("Store",null);

		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );

		/// <summary>
		/// Used to keep track of changes to the layout of the store database.
		/// </summary>
		static private string storeVersion = "1.1.0";

		/// <summary>
		/// Directories where store-managed and unmanaged files are kept.
		/// </summary>
		static private string storeManagedDirectoryName = "CollectionFiles";
		static private string storeUnmanagedDirectoryName = "SimiasFiles";

		/// <summary>
		/// Indirection Prefix and Prefix length where store unmanaged files are kept.
		/// </summary>
		static private string storeUnmanagedPrefix = "0";
		static private int storeUnmanagedPrefixLength = 1;

		/// <summary>
		/// File used to store the local password used to authenticate local
		/// web services.
		/// </summary>
		static private string LocalPasswordFile = ".local.if";

		/// <summary>
		/// Name of the local domain.
		/// </summary>
		static internal string LocalDomainName = "Local";

		/// <summary>
		/// Default sync interval for the machine. Synchronizes every 5 minutes.
		/// </summary>
		static private int DefaultMachineSyncInterval = 300;

		/// <summary>
		/// Flag that indicates whether the database is shutting down. No changes
		/// will be allowed in the database if this flags is true.
		/// </summary>
		private bool shuttingDown = false;

		/// <summary>
		/// Handle to the local store provider.
		/// </summary>
		private Persist.IProvider storageProvider = null;

		/// <summary>
		/// Path to where the store is kept.
		/// </summary>
		private static string storePath = null;

		/// <summary>
		/// Port used by the local loopback service.
		/// </summary>
		private static int localServicePort = -1;

		/// <summary>
		/// Path to where store managed files are kept.
		/// </summary>
		private string storeManagedPath;

		/// <summary>
		/// Path to where the unmanged files are kept.
		/// </summary>
		private string storeUnmanagedPath;

		/// <summary>
		/// Configuration object passed during connect.
		/// </summary>
		private static Configuration config = null;

		/// <summary>
		/// Cross-process database lock function.
		/// </summary>
		static private Mutex storeMutex = new Mutex();

		/// <summary>
		/// String that identifies the publisher of events for this object instance.
		/// </summary>
		private string publisher = "Unspecified";

		/// <summary>
		/// Used to publish collection store events.
		/// </summary>
		private EventPublisher eventPublisher;

		/// <summary>
		/// Used for quick lookup of the current logged on user.
		/// </summary>
		private string identity = null;

		/// <summary>
		/// Used for quick lookup of the local database.
		/// </summary>
		private string localDb = null;

		/// <summary>
		/// Singleton of the store.
		/// </summary>
		private static Store instance = null;

		/// <summary>
		/// Used to indicate whether this instance is running on an enterprise server.
		/// </summary>
		private static bool enterpriseServer = false;

		/// <summary>
		/// Set to true if the store was created when this instance was initialized.
		/// </summary>
		private static bool created = false;

		/// <summary>
		/// Object used to cache node objects.
		/// </summary>
		private NodeCache cache;

		/// <summary>
		/// This is used to keep from generating a new key set everytime a new RSACryptoSecurityProvider
		/// object is instantiated. This is passed as a parameter to the constructor and will 
		/// use the DEFAULT RA key set.
		/// </summary>
		static private CspParameters RAParameters;

		#endregion

		#region Properties
		/// <summary>
		/// Gets the event publisher object.
		/// </summary>
		internal EventPublisher EventPublisher
		{
			get { return eventPublisher; }
		}

		/// <summary>
		/// Gets the local database object.
		/// </summary>
		internal LocalDatabase LocalDb
		{
			get { return GetCollectionByID( localDb ) as LocalDatabase; }
		}

		/// <summary>
		/// Gets or sets the publisher event source identifier.
		/// </summary>
		internal string Publisher
		{
			get { return publisher; }
			set { publisher = value; }
		}

		/// <summary>
		/// Gets the Storage provider interface.
		/// </summary>
		public Persist.IProvider StorageProvider
		{
			get { return storageProvider; }
		}

		/// <summary>
		/// Gets the NodeCache object.
		/// </summary>
		internal NodeCache Cache
		{
			get { return cache; }
		}

		/// <summary>
		/// Gets or sets a local password on the local domain object.
		/// </summary>
		internal string LocalPassword
		{
			get
			{
				Property p = LocalDb.Properties.GetSingleProperty( PropertyTags.LocalPassword );
				return ( p != null ) ? p.ToString() : null;
			}

			set
			{
				LocalDatabase ldb = LocalDb;
				ldb.Properties.ModifyNodeProperty( PropertyTags.LocalPassword, value );
				ldb.Commit();
			}
		}

		/// <summary>
		/// Gets the configuration object passed to the store.Connect() method.
		/// </summary>
		public static Configuration Config
		{
			get 
			{ 
				if ( config == null )
				{
					throw new CollectionStoreException( "The store has not been initialized." );
				}

				return config; 
			}
		}

		/// <summary>
		/// Gets or sets the default domain ID.
		/// </summary>
		public string DefaultDomain
		{
			get { return LocalDb.DefaultDomain; }
			set
			{
				LocalDatabase ldb = LocalDb;
				ldb.DefaultDomain = value;
				ldb.Commit();
			}
		}

		/// <summary>
		/// Gets the identifier for this Collection Store.
		/// </summary>
		public string ID
		{
			get { return localDb; }
		}

		/// <summary>
		/// Gets the Identity object that represents the currently logged on user.
		/// </summary>
		public Identity CurrentUser
		{
			get { return GetNodeByID( localDb, identity ) as Identity; }
		}

		/// <summary>
		/// Gets whether this instance is running on a enterprise server.
		/// </summary>
		public static bool IsEnterpriseServer
		{
			get { return enterpriseServer; }
		}

		/// <summary>
		/// Gets the ID of the local domain.
		/// </summary>
		public string LocalDomain
		{
			get { return LocalDb.Domain; }
		}

		/// <summary>
		/// Gets the port number that the local service is listening on.
		/// </summary>
		public static int LocalServicePort
		{
			get { return localServicePort; }
		}

		/// <summary>
		/// Specifies the reports directory.
		/// </summary>
		public static string ReportPath
		{
			get { return Path.Combine( StorePath, "report" ); }
		}

		/// <summary>
		///  Specifies where the default store path
		/// </summary>
		public static string StorePath
		{
			get 
			{ 
				if ( storePath == null )
				{
					throw new CollectionStoreException( "The store has not been initialized." );
				}

				return storePath; 
			}
		}

		/// <summary>
		/// Gets the version of the database.
		/// </summary>
		public string Version
		{
			get
			{
				Property p = LocalDb.Properties.FindSingleValue( PropertyTags.StoreVersion );
				return ( p != null ) ? p.ToString() : "Unknown version";
			}
		}
		/// <summary>
		///
		/// </summary>
		public static string storeversion
		{
			get
			{
				return storeVersion;
			}
		}

		/// <summary>
		/// Gets whether the database is being shut down. No changes to the database
		/// will be allowed if this property returns true.
		/// </summary>
		public bool ShuttingDown
		{
			get { return shuttingDown; }
		}

		/// <summary>
		///
		/// </summary>
		public RSACryptoServiceProvider DefaultRSARA
		{
			get{
				return DefaultRACert;
			}
		}

		static internal RSACryptoServiceProvider DefaultRACert
		{
			get{
				RSACryptoServiceProvider csp = null;

					try
					{///currently hard-coding it to 1024 - need to see if this can be configured during install time
					///post-install will be a problem, since the already encrypted keys cannot be decrypted -- FIXME
						csp = new RSACryptoServiceProvider(1024, RAParameters );
						csp.PersistKeyInCsp = true;
					}
					catch ( CryptographicException e )
					{
						log.Debug( e, "Corrupt RA cryptographic key container." );
#if WINDOWS
						IntPtr phProv = IntPtr.Zero;
						if ( CryptAcquireContext(
							ref phProv,
							RAParameters.KeyContainerName,
							"Microsoft Strong Cryptographic Provider",
							1, // PROV_RSA_FULL
							0x10) ) // CRYPT_DELETEKEYSET
						{
							csp = new RSACryptoServiceProvider( 1024, RAParameters );
						}
#endif
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

		#region Constructor
		/// <summary>
		/// Constructor for the Store object.
		/// </summary>
		private Store()
		{
			// Setup the event publisher object.
			eventPublisher = new EventPublisher();

			// Initialize the node cache.
			cache = new NodeCache( this );

			// Create or open the underlying database.
			storageProvider = Persist.Provider.Connect( new Persist.ProviderConfig( config, storePath ), out created );

			// Set the managed and unmanaged paths to the store.
			storeManagedPath = Path.Combine( storePath, storeManagedDirectoryName );
			storeUnmanagedPath = Path.Combine( storePath, storeUnmanagedDirectoryName );

			// Create the report directory if it doesn't exist.
			if ( IsEnterpriseServer && !Directory.Exists( ReportPath ) )
			{
				Directory.CreateDirectory( ReportPath );
			}

			CspParameters RAParameters = new CspParameters();
			RAParameters.KeyContainerName = "RAKeyStore";
			log.Debug("RA key store set");

			// Either create the store or authenticate to it.
			if ( created )		// store is newly created...
			{
				try
				{
					// Default the user name to the local logged in user.
					string userName = Environment.UserName;

					// Get the name of the user to create as the identity.
					if ( IsEnterpriseServer )
					{
						// If there is a domain specified, get the specified admin user to be the
						// store owner.
						string adminDNName = config.Get( Domain.SectionName, Domain.AdminDNTag );
						if ( adminDNName != null )
						{
							userName = ParseUserName( adminDNName );
						}
					}

					// Create an object that represents the database collection.
					string localDomainID = Guid.NewGuid().ToString();
					LocalDatabase ldb = new LocalDatabase( this, localDomainID );
					ldb.Properties.AddNodeProperty( PropertyTags.StoreVersion, storeVersion );
					localDb = ldb.ID;

					// Create an identity that represents the current user.  This user will become the 
					// database owner. Add the domain mapping to the identity.
					Identity owner = new Identity( this, userName, Guid.NewGuid().ToString() );
					identity = owner.ID;

					// Create a credential to be used to identify the local user.
					RSACryptoServiceProvider credential = new RSACryptoServiceProvider( 1024 );

					// Create a credential to be used to identify the local user.
					owner.AddDomainIdentity( owner.ID, localDomainID, credential.ToXmlString( true ), CredentialType.PPK );
                    
					// Create a member object that will own the local database.
					Member member = new Member( owner.Name, owner.ID, Access.Rights.Admin );
					member.IsOwner = true;

					// Save the local database changes. Impersonate so that the creator of these nodes
					// can be set before the identity node is committed to the store.
					ldb.Impersonate( member );
					ldb.Commit( new Node[] { ldb, member, owner } );

					// Create the local domain.
					Domain domain = new Domain( this, LocalDomainName, localDomainID, "Local Machine Domain", SyncRoles.Local, Domain.ConfigurationType.None );
					Member domainOwner = new Member( owner.Name, owner.ID, Access.Rights.Admin, owner.PublicKey );
					domainOwner.IsOwner = true;
					domain.Commit( new Node[] { domain, domainOwner } );
				}
				catch ( Exception e )
				{
					// Log this error.
					Console.Error.WriteLine( "Error: Exception {0}. Could not initialize collection store.", e.Message );
					Console.Error.WriteLine( "Stack Trace = {0}", e.StackTrace );

					// The store didn't initialize delete it and rethrow the exception.
					if ( storageProvider != null )
					{
						Console.Error.WriteLine( "       Deleting collection store." );
						storageProvider.DeleteStore();
						storageProvider.Dispose();
					}

					// Rethrow the exception.
					throw;
				}
			}
			else		// store already existing
			{
				// Get the local database object.
				LocalDatabase ldb = GetDatabaseObject();
				if ( ldb == null )
				{
					throw new DoesNotExistException( "Local database object does not exist." );
				}
				// Compare the store version to make sure that it is correct.
				if( Version == "1.0.1")
				{
					try
					{
						ldb.Properties.AddNodeProperty( PropertyTags.StoreVersion, storeVersion );
						ldb.Commit();
					}
					catch
					{
					}
				}
				else if ( storeVersion != Version )
				{
					// Change the store version if the store version is 1.0.1
					throw new SimiasException( String.Format( "Incompatible database version. Expected version {0} - Found version {1}.", storeVersion, Version ) );
				}

				// Get the identity object that represents this logged on user.
				Identity lid = ldb.GetSingleNodeByType( NodeTypes.IdentityType ) as Identity;
				if ( lid != null )
				{
					identity = lid.ID;
				}
				else
				{
					throw new DoesNotExistException( "Identity object does not exist." );
				}
			}

			// Create a one-time password for non-simias processes to use to authenticate to the local web services.
			CreateLocalCredential();
		}
		#endregion

		#region Factory  Methods
		/// <summary>
		/// Gets a handle to the Collection store.
		/// </summary>
		/// <returns>A reference to a Store object.</returns>
		static public Store GetStore()
		{
			lock ( typeof( Store ) )
			{
				if ( instance == null )
				{
					// Make sure that Initialize has been called previously.
					if ( storePath == null )
					{
						throw new CollectionStoreException( "The store has not been initialized." );
					}

					// Create and initialize the store instance.
					instance = new Store();

					// Set the default store policies if the store is new.
					if ( created )
					{
						CreateDefaultPolicies();
					}

					// Create the certificate policy and load the certs.
					new CertPolicy();

					Simias.Security.CertificateStore.LoadCertsFromStore(); //this loads all Certs 
				}

				return instance;
			}
		}

		/// <summary>
		/// Creates the singleton store instance and sets the required store parameters.
		/// </summary>
		/// <param name="simiasStorePath">The directory path to the store.</param>
		/// <param name="isServer">True if running in a server configuration.</param>
		/// <param name="port">The port number that the local service is listening on. If this
		/// is an enterprise server, this value is ignored.</param>
		static public void Initialize( string simiasStorePath, bool isServer, int port )
		{
			lock ( typeof( Store ) )
			{
				if ( instance == null )
				{
					// Save the path to the store.
					storePath = Path.GetFullPath( simiasStorePath );

					// Store the configuration that opened this instance.
					config = new Configuration( storePath, isServer );

					// Does the configuration indicate that this is an enterprise server?
					enterpriseServer = isServer;

					// Save the port that the service is listening on.
					if ( !enterpriseServer )
					{
						localServicePort = port;
					}
				}
				else
				{
					throw new CollectionStoreException( "The store has already been initialized." );
				}
			}
		}

		/// <summary>
		/// Deletes the singleton instance of the store object.
		/// NOTE: This call is for utility programs that need to browse to the store.
		/// Don't call this unless you understand the consequences of closing
		/// the process's only store instance.
		/// </summary>
		static public void DeleteInstance()
		{
			lock ( typeof( Store ) )
			{
				instance = null;
				storePath = null;
				config = null;
				localServicePort = -1;
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Creates the default policies for the store.
		/// </summary>
		private static void CreateDefaultPolicies()
		{
			if ( !IsEnterpriseServer )
			{
				// Create a SyncInterval policy.
				SyncInterval.Create( DefaultMachineSyncInterval );
			}

			// Create a FileFilter local machine policy that disallows the Thumbs.db 
			// and .DS_Store files from synchronizing. This fix in in response to bug #73517.
			FileTypeFilter.Create( new FileTypeEntry[] { new FileTypeEntry( "Thumbs.db", false, true ),
														   new FileTypeEntry( ".DS_Store", false, false )
													   } );
		}

		/// <summary>
		/// Creates a local credential to be used to identify the local user.
		/// </summary>
		private void CreateLocalCredential()
		{
			// Check if the file already exists.
			string path = Path.Combine( StorePath, LocalPasswordFile );
			if ( !File.Exists( path ) )
			{
				// Set a random password.
				LocalPassword = Guid.NewGuid().ToString();

				// Export the credential to a file stored in the directory where the simias store
				// is located. The local web services will use the file credential to authenticate
				// to the local box.
				using ( StreamWriter sw = new StreamWriter( path ) )
				{
					sw.Write( CurrentUser.Name + ":" + LocalDomain + LocalPassword );
				}
			}
			else
			{
				// Check to make sure that this is not an old credential file.
				// If the local identity is missi
				string localCredentials = String.Empty;
				using ( StreamReader sr = new StreamReader( path ) )
				{
					localCredentials = sr.ReadLine();
				}

				if ( localCredentials.IndexOf( ':' ) == -1 )
				{
					File.Delete( path );
					CreateLocalCredential();
				}
			}
		}

		/// <summary>
		/// Returns a Node object for the specified identifier.
		/// </summary>
		/// <param name="collectionID">Identifier of the collection that the node is contained by.</param>
		/// <param name="nodeID">Globally unique identifier for the object.</param>
		/// <returns>A Node object for the specified identifier.  If the object doesn't 
		/// exist a null is returned.</returns>
		private Node GetNodeByID( string collectionID, string nodeID )
		{
			// Normalize the collectionID and node ID.
			collectionID = collectionID.ToLower();
			nodeID = nodeID.ToLower();

			// See if the node exists in the cache first.
			Node node = cache.Get( collectionID, nodeID );
			if ( node == null )
			{
				// Get the specified object from the persistent store.
				XmlDocument document = storageProvider.GetRecord( nodeID, collectionID );
				if ( document != null )
				{
					node = Node.NodeFactory( this, document );
					Collection collection = ( collectionID == nodeID ) ? node as Collection : GetNodeByID( collectionID, collectionID ) as Collection;
					cache.Add( collection, node );
				}
			}		

			return node;
		}

		/// <summary>
		/// Returns the 'cn' portion of the ldap string.
		/// </summary>
		/// <param name="ldapName">LDAP name to parse.</param>
		/// <returns>The 'cn' portion of the LDAP name.</returns>
		private string ParseUserName( string ldapName )
		{
			if ( ldapName == null )
			{
				throw new CollectionStoreException( "LDAP proxy name is null" );
			}

			// Skip over the 'cn=' if it exists.
			int startIndex = ldapName.ToLower().StartsWith( "cn=" ) ? 3 : 0;
			int endIndex = ldapName.IndexOf( ',' );
			int length = ( endIndex == -1 ) ? ldapName.Length - startIndex : endIndex - startIndex;
			return ldapName.Substring( startIndex, length );
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Gets a path to where the store managed files for the specified collection should be created.
		/// </summary>
		/// <param name="collectionID">Collection identifier that files will be associated with.</param>
		/// <returns>A path string that represents the store managed path.</returns>
		internal string GetStoreManagedPath( string collectionID )
		{
			return Path.Combine( storeManagedPath, collectionID.ToLower() );
		}

		/// <summary>
		/// Gets a path to where the store unmanaged files for the specified collection should be created.
		/// </summary>
		/// <param name="collectionID">Collection identifier that files will be associated with.</param>
		/// <returns>A path string that represents the store unmanaged path.</returns>
		internal string GetStoreUnmanagedPath( string collectionID )
		{
                        string tmpPath = null;
			if(MyEnvironment.Windows)
                        {
                                tmpPath = Path.Combine(storeUnmanagedPath, storeUnmanagedPrefix+collectionID.Substring(0,storeUnmanagedPrefixLength));
                        }
                        else
                        {
				#if MONO 
				{
	                                int LowerIndex = 0,HigherIndex=0;
                	                DataStore[] volumes = DataStore.GetVolumes();
                        	        for(LowerIndex=0; LowerIndex < volumes.Length; LowerIndex++)
	                                {
						if( volumes[ LowerIndex ].Enabled )
						{
							if( ( volumes[ LowerIndex  ].CompareTo(volumes[ HigherIndex  ] ) ) >=0 )
        	                	                	HigherIndex = LowerIndex;
						}
        	                        }
					if( HigherIndex != 0 )
					{
						tmpPath = Path.Combine(storeUnmanagedPath,volumes[ HigherIndex  ].DataPath);
						tmpPath = Path.Combine(tmpPath,"SimiasFiles");
					}
					else
						tmpPath = storeUnmanagedPath;
                	                tmpPath = Path.Combine(tmpPath, storeUnmanagedPrefix+collectionID.Substring(0,storeUnmanagedPrefixLength));
				}
				#endif
                        }
                        return Path.Combine( tmpPath, collectionID.ToLower() );
		}

		
		/// <summary>
		/// Gets a path to where the store unmanaged files for the specified collection should be created.
		/// </summary>
		/// <param name="StoreVersion">Collection identifier that files will be associated with.</param>
		/// <returns>A path string that represents the store unmanaged path.</returns>
		internal string GetStoreUnmanagedPrefix(string StoreVersion)
		{
			return storeUnmanagedPrefix;
		}

		/// <summary>
		/// Acquires the store lock protecting the database against simultaneous commits.
		/// </summary>
		internal void LockStore()
		{
			storeMutex.WaitOne();
		}

		/// <summary>
		/// Indicates that the database is being shutdown. No more changes will be allowed
		/// when this method completes.
		/// </summary>
		internal void ShutDown()
		{
			shuttingDown = true;
		}

		/// <summary>
		/// Releases the store lock.
		/// </summary>
		internal void UnlockStore()
		{
			storeMutex.ReleaseMutex();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a domain identity to the Collection Store.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <param name="userID">Identity that this user is known as in the specified domain.</param>
		public void AddDomainIdentity( string domainID, string userID )
		{
			AddDomainIdentity( domainID, userID, null, CredentialType.None );
		}

		/// <summary>
		/// Adds a domain identity to the Collection Store.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <param name="userID">Identity that this user is known as in the specified domain.</param>
		///	<param name="credentials">Credentials for the user. May be null.</param>
		///	<param name="credType">Type of credentials being stored.</param>
		public void AddDomainIdentity( string domainID, string userID, string credentials, CredentialType credType )
		{
			// Add the domain mapping for the specified user.
			LocalDb.Commit( CurrentUser.AddDomainIdentity( userID.ToLower(), domainID, credentials, credType ) );
		}

		/// <summary>
		/// Deletes the persistent store database and disposes this object.
		/// </summary>
		public void Delete()
		{
			// Check if the store managed path still exists. If it does, delete it.
			if ( Directory.Exists( storeManagedPath ) )
			{
				Directory.Delete( storeManagedPath, true );
			}

			// Delete the local password file.
			string path = Path.Combine( StorePath, LocalPasswordFile );
			if ( File.Exists( path ) )
			{
				File.Delete( path );
			}

			// Say bye-bye to the store.
			storageProvider.DeleteStore();
			storageProvider.Dispose();
		}

		/// <summary>
		/// Removes the specified domain identity from the Collection Store.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		public void DeleteDomainIdentity( string domainID )
		{
			// Delete the domain object and commit the changes.
			LocalDb.Commit( CurrentUser.DeleteDomainIdentity( domainID.ToLower() ) );
		}

		/// <summary>
		/// Removes the credentials from the specified domain.
		/// </summary>
		/// <param name="domainID">Domain identifier to remove the password from.</param>
		public void DeleteDomainCredentials( string domainID )
		{
			LocalDb.Commit( CurrentUser.SetDomainCredentials( domainID.ToLower(), null, CredentialType.None ) );
		}

		/// <summary>
		/// Returns a Collection object for the specified identifier.
		/// </summary>
		/// <param name="collectionID">Globally unique identifier for the object.</param>
		/// <returns>A Collection object for the specified identifier.  If the object doesn't 
		/// exist a null is returned.</returns>
		public Collection GetCollectionByID( string collectionID )
		{
			return GetNodeByID( collectionID, collectionID ) as Collection;
		}

		/// <summary>
		/// Gets all collections that belong to the specified domain.
		/// </summary>
		/// <param name="domainID">Domain identifier.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Collection
		/// objects that matched the specified domain.</returns>
		public ICSList GetCollectionsByDomain( string domainID )
		{
			// Create a container object to hold all collections that match the specified domain.
			ICSList collectionList = new ICSList();

			Persist.Query query = new Persist.Query( PropertyTags.DomainID, SearchOp.Equal, domainID.ToLower(), Syntax.String );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						if ( xe.GetAttribute( XmlTags.IdAttr ) == xe.GetAttribute( XmlTags.CIdAttr ) )
						{
							collectionList.Add( new ShallowNode( xe ) );
						}
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return collectionList;
		}

		/// <summary>
		/// Gets all collections that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name of the collection(s) to search for.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Collection 
		/// objects that matched the specified name.</returns>
		public ICSList GetCollectionsByName( string name )
		{
			return GetCollectionsByName( name, SearchOp.Equal );
		}

		/// <summary>
		/// Gets all collections that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name of the collection(s) to search for.</param>
		/// <param name="searchOp">The search operation used with the search.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Collection 
		/// objects that matched the specified name.</returns>
		public ICSList GetCollectionsByName( string name, SearchOp searchOp )
		{
			// Create a container object to hold all collections that match the specified name.
			ICSList collectionList = new ICSList();

			Property p = new Property( BaseSchema.ObjectName, name );
			Persist.Query query = new Persist.Query( p.Name, searchOp, p.SearchString, p.Type );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						// See if this element represents a collection.
						if ( xe.GetAttribute( XmlTags.IdAttr ) == xe.GetAttribute( XmlTags.CIdAttr ) )
						{
							collectionList.Add( new ShallowNode( xe ) );
						}
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return collectionList;
		}

		/// <summary>
		/// Gets a list of collections that the specified user is the owner of.
		/// </summary>
		/// <param name="userID">User identifier that is the owner.</param>
		/// <returns>An ICSList object containing the ShallowNode objects that the specified user is
		/// the owner of.</returns>
		public ICSList GetCollectionsByOwner( string userID )
		{
			return GetCollectionsByOwner( userID, null );
		}

		/// <summary>
		/// Gets a list of collections that the specified user is the owner of.
		/// </summary>
		/// <param name="userID">User identifier that is the owner.</param>
		/// <param name="domainID">Domain identifier to filter the collections by. If this parameter is
		/// null, all collections are returned regardless of which domain they are in.</param>
		/// <returns>An ICSList object containing the ShallowNode objects that the specified user is
		/// the owner of.</returns>
		public ICSList GetCollectionsByOwner( string userID, string domainID )
		{
			userID = userID.ToLower();
			domainID = ( domainID != null ) ? domainID.ToLower() : null;
			ICSList ownerList = new ICSList();

			// Get all of the collections that the user is a member of in the specified domain.
			ICSList collectionList = GetCollectionsByUser( userID );
			foreach ( ShallowNode sn in collectionList )
			{
				Collection c = new Collection( this, sn );
				if ( ( c.Owner.UserID == userID ) && ( ( domainID == null ) || ( c.Domain == domainID ) ) )
				{
					ownerList.Add( sn );
				}
			}

			return ownerList;
		}

///This implementation has to be optimized. The index is used incorrectly. And it should not loop to the index on every call.
		public CollectionKey GetCollectionCryptoKeysByOwner( string userID, string domainID, int index)
		{
			accessLog.LogAccess("ExportiFoldersCryptoKeys","Export CryptoKeys","Starting",userID);
			userID = userID.ToLower();
			domainID = ( domainID != null ) ? domainID.ToLower() : null;
			CollectionKey  cKey = null;
			int count = 0;

			// Get all of the collections that the user is a member of in the specified domain.
			ICSList collectionList = GetCollectionsByUser( userID );
			foreach( ShallowNode sn in collectionList )
			{
				Collection c = new Collection( this, sn );
				if((c.EncryptionAlgorithm !="") && (c.EncryptionAlgorithm != null) && (c.Owner.UserID == userID) && ((c.Domain == domainID)))
				{
					if(count == index)
					{
						//cKey = new  CollectionKey(c.ID, c.EncryptionKey, c.RecoveryKey);
						cKey = new  CollectionKey();
						cKey.NodeID = c.ID;
						cKey.PEDEK = c.EncryptionKey;
						cKey.REDEK= c.RecoveryKey;
						log.Debug("CID {2}\nPEDEK {0}\nREDEK {1}", cKey.PEDEK, cKey.REDEK, cKey.NodeID);
						break;
					}
					count++;
				}
			}
			accessLog.LogAccess("ExportiFoldersCryptoKeys","Export CryptoKeys","Completed",userID);
			return cKey;
		}

		/// <summary>
                /// Get the encryption key hash of the collection (for ifolder)
                /// </summary>
                public string GetCollectionCryptoKeyHash(string collectionID)
                {
                        Collection c = GetCollectionByID(collectionID);
log.Debug("CID {0}\n Blob {1}\n", collectionID,c.EncryptionBlob);
                        return c.EncryptionBlob;
                }
		
		/// <summary>
		///
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="domainID"></param>
		/// <param name="cKey"></param>
		/// <returns></returns>
		public bool SetCollectionCryptoKeysByOwner( string userID, string domainID, CollectionKey cKey)
		{
			accessLog.LogAccess("ImportiFoldersCryptoKeys","Importing CryptoKeys","Starting",userID);
			userID = userID.ToLower();
			domainID = ( domainID != null ) ? domainID.ToLower() : null;
			bool status = false;

			// Get all of the collections that the user is a member of in the specified domain.
			ICSList collectionList = GetCollectionsByUser( userID );
			foreach ( ShallowNode sn in collectionList )
			{
				Collection c = new Collection( this, sn );
				if((c.EncryptionAlgorithm !="") && (c.Owner.UserID == userID) && ((c.Domain == domainID)))
				{
					if(c.ID == cKey.NodeID)
					{
log.Debug("CID {2}\nPEDEK {0}\nREDEK {1}", cKey.PEDEK, cKey.REDEK, cKey.NodeID);
						c.EncryptionKey = cKey.PEDEK;
						if(cKey.REDEK !=null && cKey.REDEK !="")
						c.RecoveryKey = cKey.REDEK; 
						c.Commit();
						
						status = true;
						break;
					}
				}
			}
			accessLog.LogAccess("ImportiFoldersCryptoKeys","Importing CryptoKeys","Completed",userID);
			return status;
		}

		/// <summary>
		/// Gets all collections that contain node objects with the specified property.
		/// </summary>
		/// <param name="property">Property to search for contained in node objects.</param>
		/// <param name="op">Type of search operation.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the
		/// found Collection objects.</returns>
		public ICSList GetCollectionsByProperty( Property property, SearchOp op )
		{
			// Create a container object to hold all collections that match the specified user.
			ICSList collectionList = new ICSList();

			Persist.Query query = new Persist.Query( property.Name, op, property.SearchString, property.Type );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						// Get the collection that this Member object belongs to.
						string collectionID = xe.GetAttribute( XmlTags.CIdAttr );
						
						// Get the collection object.
						XmlDocument cDoc = storageProvider.GetShallowRecord( collectionID );
						collectionList.Add( new ShallowNode( cDoc.DocumentElement[ XmlTags.ObjectTag ] ) );
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return collectionList;
		}

		/// <summary>
		///  Gets all collections that have the specified type.
		/// </summary>
		/// <param name="type">String that contains the type of the collection(s) to search for.</param>
		/// <returns>An ICSList object containing the ShallowNode objects that match the specified 
		/// type.</returns>
		public ICSList GetCollectionsByType( string type )
		{
			// Create a container object to hold all collections that match the specified name.
			ICSList collectionList = new ICSList();

			Property p = new Property( PropertyTags.Types, type );
			Persist.Query query = new Persist.Query( p.Name, SearchOp.Equal, p.SearchString, p.Type );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						// See if this element represents a collection.
						if ( xe.GetAttribute( XmlTags.IdAttr ) == xe.GetAttribute( XmlTags.CIdAttr ) )
						{
							collectionList.Add( new ShallowNode( xe ) );
						}
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return collectionList;
		}

		/// <summary>
		/// Gets all collections that belong to the specified user.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Collection
		/// objects that matched the specified user.</returns>
		public ICSList GetCollectionsByUser( string userID )
		{
			// Create a container object to hold all collections that match the specified user.
			ICSList collectionList = new ICSList();

			Persist.Query query = new Persist.Query( PropertyTags.Ace, SearchOp.Begins, userID, Syntax.String );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						// Get the collection that this Member object belongs to.
						string collectionID = xe.GetAttribute( XmlTags.CIdAttr );
						
						// Get the collection object.
						XmlDocument cDoc = storageProvider.GetShallowRecord( collectionID );
						collectionList.Add( new ShallowNode( cDoc.DocumentElement[ XmlTags.ObjectTag ], collectionID ) );
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return collectionList;
		}

		/// <summary>
		/// Returns the collection that represents the database object.
		/// </summary>
		/// <returns>A LocalDatabase object that represents the local store. A null is returned if
		/// the database object does not exist.</returns>
		public LocalDatabase GetDatabaseObject()
		{
			LocalDatabase ldb = null;

			// See if the local database object has already been looked up.
			if ( localDb == null )
			{
				Persist.Query query = new Persist.Query( BaseSchema.ObjectType, SearchOp.Equal, NodeTypes.LocalDatabaseType, Syntax.String );
				Persist.IResultSet chunkIterator = storageProvider.Search( query );
				if ( chunkIterator != null )
				{
					char[] results = new char[ 4096 ];

					// Get the first set of results from the query.
					int length = chunkIterator.GetNext( ref results );
					if ( length > 0 )
					{
						// Set up the XML document so the data can be easily extracted.
						XmlDocument document = new XmlDocument();
						document.LoadXml( new string( results, 0, length ) );
						ldb = new LocalDatabase( this, new ShallowNode( document.DocumentElement[ XmlTags.ObjectTag ] ) );
						localDb = ldb.ID;
					}

					chunkIterator.Dispose();
				}
			}
			else
			{
				ldb = LocalDb;
			}

			return ldb;
		}

		/// <summary>
		/// Gets the Domain object from its ID.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>Domain object that the specified ID refers to if successful. Otherwise returns a null.</returns>
		public Domain GetDomain( string domainID )
		{
			return GetCollectionByID( domainID ) as Domain;
		}

		/// <summary>
		/// Gets a list of all of the domain objects.
		/// </summary>
		/// <returns>An ICSList object containing all of the domain objects.</returns>
		public ICSList GetDomainList()
		{
			return GetCollectionsByType( NodeTypes.DomainType );
		}

		/// <summary>
		/// Gets the domain credentials for the specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of the domain to get the credentials from.</param>
		/// <param name="userID">Gets the identifier of the domain user.</param>
		/// <param name="credentials">Gets the credentials for the domain.</param>
		/// <returns>The type of credentials.</returns>
		public CredentialType GetDomainCredentials( string domainID, out string userID, out string credentials )
		{
			return CurrentUser.GetDomainCredentials( domainID, out userID, out credentials );
		}

		/// <summary>
		/// Gets the  passphrasefor the specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of the domain to get the credentials from.</param>
		/// <returns>The type of credentials.</returns>
		public string GetPassPhrase( string domainID)
		{
			return CurrentUser.GetPassPhrase( domainID);
		}

		/// <summary>
		/// Gets the  passphrasefor the specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of the domain to get the credentials from.</param>
		/// <returns>The type of credentials.</returns>
		public bool GetRememberOption( string domainID )
		{
			return CurrentUser.GetRememberOption( domainID );
		}

		/// <summary>
		/// Gets the Domain object that the specified user belongs to.
		/// </summary>
		/// <param name="userID">Identifier for the user.</param>
		/// <returns>Domain object that the specified user belongs to if successful. Otherwise returns a null.</returns>
		public Domain GetDomainForUser( string userID )
		{
			string domainID = CurrentUser.GetDomainFromUserID( userID.ToLower() );
			return ( domainID != null ) ? GetDomain( domainID ) : null;
		}

		/// <summary>
		/// Gets a list of collections that have been locked.
		/// </summary>
		/// <returns>An ICSList object containing ShallowNode objects that represent locked collections.</returns>
		public ICSList GetLockedCollections()
		{
			ICSList shallowList = new ICSList();

			// Get the list of locked collection IDs.
			string[] cidList = Collection.GetLockedList();
			if ( cidList != null )
			{
				foreach( string cid in cidList )
				{
					Collection c = GetNodeByID( cid, cid ) as Collection;
					shallowList.Add( new ShallowNode( c.Properties.PropertyRoot, cid ) );
				}
			}

			return shallowList;
		}

		/// <summary>
		/// Gets all Node objects that contain the specified property.
		/// </summary>
		/// <param name="property">Property to search for contained in Node objects.</param>
		/// <param name="op">Type of search operation.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the
		/// found Node objects.</returns>
		public ICSList GetNodesByProperty( Property property, SearchOp op )
		{
			// Create a container object to hold all nodes that match the specified user.
			ICSList nodeList = new ICSList();

			Persist.Query query = new Persist.Query( property.Name, op, property.SearchString, property.Type );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						nodeList.Add( new ShallowNode( xe ) );
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return nodeList;
		}

		/// <summary>
		/// Gets the first Collection object that matches the specified name.
		/// </summary>
		/// <param name="name">A string containing the name for the collection. This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>The first Collection object that matches the specified name.  A null is 
		/// returned if no matching collections are found.</returns>
		public Collection GetSingleCollectionByName( string name )
		{
			Collection collection = null;
			ICSList collectionList = GetCollectionsByName( name );
			foreach ( ShallowNode sn in collectionList )
			{
				collection = new Collection( this, sn );
				break;
			}

			return collection;
		}

		/// <summary>
		/// Gets the first Collection object that matches the specified type.
		/// </summary>
		/// <param name="type">A string containing the type for the collection. This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>The first Collection object that matches the specified type.  A null is 
		/// returned if no matching collections are found.</returns>
		public Collection GetSingleCollectionByType( string type )
		{
			Collection collection = null;
			ICSList collectionList = GetCollectionsByType( type );
			foreach ( ShallowNode sn in collectionList )
			{
				collection = new Collection( this, sn );
				break;
			}

			return collection;
		}

		/// <summary>
		/// Gets the user ID that the logged on user is known as in the specified domain.
		/// </summary>
		/// <param name="domainID">Well known domain identifier.</param>
		/// <returns>The user ID that the logged on user is known as in the specified domain.</returns>
		public string GetUserIDFromDomainID( string domainID )
		{
			return CurrentUser.GetUserIDFromDomain( domainID.ToLower() );
		}

		/// <summary>
		/// Sets credentials for the specified domain.
		/// </summary>
		/// <param name="domainID">The domain ID to set the password for.</param>
		/// <param name="credentials">Credentials for the domain.</param>
		/// <param name="credType">Type of credentials being stored.</param>
		public void SetDomainCredentials( string domainID, string credentials, CredentialType credType )
		{
			LocalDb.Commit( CurrentUser.SetDomainCredentials( domainID.ToLower(), credentials, credType ) );
		}

		/// <summary>
		/// Stores the passphrase for the specified domain.
		/// </summary>
		/// <param name="domainID">The domain ID to store the passphrase for.</param>
		/// <param name="passPhrase">Credentials for the domain.</param>
		/// <param name="credType">Type of credentials being stored.</param>
		/// <param name="rememberPassPhrase"></param>
		public void StorePassPhrase( string domainID, string passPhrase, CredentialType credType, bool rememberPassPhrase )
		{
			LocalDb.Commit( CurrentUser.StorePassPhrase( domainID.ToLower(), passPhrase, credType, rememberPassPhrase ) );
            // Schedule all encrypted iFolders for sync...
            if( credType == CredentialType.Basic )
                SyncClient.RescheduleAllEncryptedColSync(domainID);
		}

		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Method used by applications to enumerate the Collection objects contained in the Collection Store.
		/// </summary>
		/// <returns>IEnumerator object used to enumerate the Collection objects. IEnumerator will return
		/// ShallowNode objects that represent Collection objects.</returns>
		public IEnumerator GetEnumerator()
		{
			return new StoreEnumerator( this );
		}

		/// <summary>
		/// Enumerator class for the Store object that allows enumeration of the Collection objects
		/// within the Store.
		/// </summary>
		private class StoreEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// Indicates whether the object has been disposed.
			/// </summary>
			private bool disposed = false;

			/// <summary>
			/// List of collections that exist under this Store.
			/// </summary>
			private XmlDocument collectionList;

			/// <summary>
			/// Enumerator used to enumerate each returned item in the chunk enumerator list.
			/// </summary>
			private IEnumerator collectionEnumerator;

			/// <summary>
			/// Store object from which the collections are being enumerated.
			/// </summary>
			private Store store;

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
			/// Constructor for the StoreEnumerator class.
			/// </summary>
			/// <param name="storeObject">Store object where to enumerate the collections.</param>
			public StoreEnumerator( Store storeObject )
			{
				store = storeObject;
				Reset();
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
			/// Sets the enumerator to its initial position, which is before the first element in the
			/// collection.
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

				// Create the collection query.
				Persist.Query query = new Persist.Query( PropertyTags.Types, SearchOp.Equal, NodeTypes.CollectionType, Syntax.String );
				chunkIterator = store.storageProvider.Search( query );
				if ( chunkIterator != null )
				{
					// Get the first set of results from the query.
					int length = chunkIterator.GetNext( ref results );
					if ( length > 0 )
					{
						// Set up the XML document that we will use as the granular query to the client.
						collectionList = new XmlDocument();
						collectionList.LoadXml( new string( results, 0, length ) );
						collectionEnumerator = collectionList.DocumentElement.GetEnumerator();
					}
					else
					{
						collectionEnumerator = null;
					}
				}
				else
				{
					collectionEnumerator = null;
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

					if ( collectionEnumerator == null )
					{
						throw new InvalidOperationException( "Empty enumeration" );
					}

					return new ShallowNode( ( XmlElement )collectionEnumerator.Current );
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
				while ( ( collectionEnumerator != null ) && !moreData )
				{
					// See if there is anymore data left in this result set.
					moreData = collectionEnumerator.MoveNext();
					if ( !moreData )
					{
						// Get the next page of the results set.
						int length = chunkIterator.GetNext( ref results );
						if ( length > 0 )
						{
							// Set up the XML document that we will use as the granular query to the client.
							collectionList = new XmlDocument();
							collectionList.LoadXml( new string( results, 0, length ) );
							collectionEnumerator = collectionList.DocumentElement.GetEnumerator();

							// Move to the first entry in the document.
							moreData = collectionEnumerator.MoveNext();
							if ( !moreData )
							{
								// Out of data.
								collectionEnumerator = null;
							}
						}
						else
						{
							// Out of data.
							collectionEnumerator = null;
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
						collectionList = new XmlDocument();
						collectionList.LoadXml( new string( results, 0, length ) );
						collectionEnumerator = collectionList.DocumentElement.GetEnumerator();
					}
					else
					{
						// Out of data.
						collectionEnumerator = null;
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
			private void Dispose( bool disposing )
			{
				// Check to see if Dispose has already been called.
				if ( !disposed )
				{
					// Protect callers from accessing the freed members.
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
			~StoreEnumerator()      
			{
				Dispose( false );
			}
			#endregion
		}

		/// <summary>
		///
		/// </summary> 
		/// <param name="ifolderid"></param>
		/// <returns></returns>
                public static int ResetRootNode(string ifolderid)
                {
                        try{
                        Store store = Store.GetStore();
                        //store.GetCollectionsByType( NodeTypes.Collection);
                        Collection col = store.GetCollectionByID( ifolderid);
                        if( col == null)
                                return 1;
                        ICSList results = col.Search( PropertyTags.Root, Syntax.String );
                         foreach ( ShallowNode shallowNode in results )
                         {
                                 DirNode rootDir = new DirNode( col, shallowNode );
                                string currentpath = ((Property)rootDir.Properties.GetSingleProperty( PropertyTags.Root )).Value as string;
                                string storepath = StorePath;
                                //string suffix = Path.GetFileName( currentpath );
                                string suffix = Path.GetDirectoryName( currentpath );
                                suffix = Path.GetDirectoryName( suffix);
                                suffix = Path.GetDirectoryName(suffix );
                                suffix = currentpath.Substring(suffix.Length+1);        
                                log.Info("currentpath is:{0}--store path is:{1} and suffix is:{2}", currentpath,storepath,suffix);                      
        
                                string newpath = Path.Combine(storepath,suffix); 
                                Property p = new Property( PropertyTags.Root, newpath);
                                p.LocalProperty = true;
                                rootDir.Properties.ModifyNodeProperty( p );
                                col.Commit(rootDir);
 
                                log.Info("new path is:{0}", newpath);                   
                                 break;
                         }
                        }
                        catch(Exception ex)
                        {
                                log.Info("Exception is:{0}--{1}", ex.Message, ex.StackTrace);                   
                        }
                        return 0;
                }

		/// <summary>
		///
		/// </summary> 
		/// <param name="ifolderid"></param>
		/// <param name="nodeid"></param>
		/// <param name="relativepath"></param>
		/// <param name="basepath"></param>
		/// <param name="filetype"></param>
		/// <param name="length"></param>
		/// <returns></returns>
                public int RestoreData(string ifolderid, string nodeid, string relativepath, string basepath, string filetype, long length)
                {
                        log.Info("Entered RestoreData.");
                //      string ifolderid = "b8123e0c-69ff-4dc0-b303-0d760092b0a5";
                        //string relativepath = "encryptedifolder/copydir.txt";
                //      string relativepath = "encryptedifolder/dir2/file3.txt";
                //      string nodeid = "d081e003-23b9-404a-8bb7-9832bb2cdc7113";
                //      string basepath = "/home/banderso/ifolder/recovery/recoverytool/testrecovery";  /// TODO: Add base path of the iFolder of the backed up data in xml file...
                        string backedpath = Path.Combine( basepath, relativepath);
 
                         log.Info("backedpath: {0}", backedpath);
                        if( filetype.Equals("FileNode"))
                        {
                                return RestoreFile(ifolderid, relativepath, nodeid, basepath, length);
                        }
                        else
                        {
                                return RestoreDirectory( ifolderid, relativepath, nodeid, basepath);
                        }
                        //return 0;
                }


		/// <summary>
		///
		/// </summary>
		/// <param name="ifolderid"></param>
		/// <param name="relativepath"></param>
		/// <param name="nodeid"></param>
		/// <param name="basepath"></param>
		/// <returns></returns>
                public int RestoreDirectory(string ifolderid, string relativepath, string nodeid, string basepath)
                {
                        string backedpath = Path.Combine( basepath, relativepath);
                         log.Debug("backedpath: {0}", backedpath);
                        // Check whether the directory present on the target. If not create. else return;
                        Store store = Store.GetStore();
                        if(store == null)
                                return 1000;
                        Collection col = store.GetCollectionByID( ifolderid);
                        if( col == null)
                                return 21;
                        
                        try
                        {
                                //verify whether Directory exist in new path, if return, else create.   
                                string newbasepath = col.UnmanagedPath;
                                 log.Debug("newbasepath: {0}", newbasepath);
                                 string newpath = "";
                                newpath = Path.Combine(newbasepath, relativepath);      
                                 log.Debug("relativepath: {0} and newpath:{1}", relativepath, newpath);
                                if(!Directory.Exists(newpath))
                                {
                                        log.Debug("directory doesn't Exist");
                                        //create direcotry.
                                        Directory.CreateDirectory(newpath);     
                                        log.Info("directory creation completed");
                                }
                                if(!Directory.Exists(newpath))
                                {
                                        log.Debug("Error while creating Directory");
                                        return 22;
                                }       
                                
                                Node dirNode =  col.GetNodeByPath(relativepath);
                                if(null == dirNode)
                                {
                                        log.Info("directory doesn't exist on new path :{0}",relativepath);
                                        //Create directory node 
                                        DirNode parentnode = new DirNode( col.GetNodeByPath(Path.GetDirectoryName(relativepath)));
                                        if(null != parentnode)
                                        {
                                                log.Debug("Parent directory  exist on new path :{0}",relativepath);
                                                DirNode dnode = new DirNode(col, parentnode, Path.GetFileName(newpath));
                                                if(dnode != null)
                                                        col.Commit(dnode);
                                        }
                                        else
                                        {
                                                log.Debug("Parent directory  doesn't exist on new path :{0}",relativepath);
                                                return 24;
                                        }
 
                                }
                                else
                                {
                                        log.Debug("directory  exist on new path :{0} ",relativepath);
                                        return 0;       
                                }
                        }
                        catch(Exception e1)
                        {
                                log.Debug("Exceptioni while creating directory: {0}--{1}", e1.Message, e1.StackTrace);
                                return 23;
                        }
                        return 0;
                }

		/// <summary>
		///
		/// </summary>
		/// <param name="ifolderid"></param>
		/// <param name="relativepath"></param>
		/// <param name="nodeid"></param>
		/// <param name="basepath"></param>
		/// <param name="length"></param>
		/// <returns></returns>		
                public int RestoreFile(string ifolderid, string relativepath, string nodeid, string basepath, long length)
                {
                        log.Debug("Entered RestoreFile.");
                        // Read from the xml file 
                //      string ifolderid = "b8123e0c-69ff-4dc0-b303-0d760092b0a5";
                //      string relativepath = "encryptedifolder/copydir.txt";
                //      string nodeid = "d081e003-23b9-404a-8bb7-9832bb2cdc7113";
                //      string basepath = "/home/banderso/ifolder/recovery/recoverytool/testrecovery";  /// TODO: Add base path of the iFolder of the backed up data in xml file...
                        string backedpath = Path.Combine( basepath, relativepath);
                        //bool encrypted = false;
                        FileInfo fi = new FileInfo( backedpath);
                        long nodelength = fi.Length;
                        log.Info("Starting with datamove from {0}--{1}.", backedpath, nodelength);
                        if( !File.Exists(backedpath) && !Directory.Exists(backedpath))
                                return 2;       // Path does not exist...
                        Store store = Store.GetStore();
                        if( store == null)
                                return 1000;
                        Collection col = store.GetCollectionByID( ifolderid);
                        if( col == null)
                                return 1;       // iFolder not present...
                        if( col.EncryptionAlgorithm != null && col.EncryptionAlgorithm != string.Empty)
                        {
                                nodelength = length;
                        }
                        try
                        {
                                string newbasepath = col.UnmanagedPath;
                                log.Debug("newbasepath: {0}", newbasepath);
                                string newpath = "";
                                Node n1 = col.GetNodeByID(nodeid);
                                FileNode node = null;
                                if( n1 != null)
                                        node = new FileNode(n1);
                                if( node == null)
                                {
                                        // This is a new file. Direct copy and create node.
                                        newpath = Path.Combine(newbasepath, relativepath);
                                        log.Info("newpath: {0}", newpath);
                                        if( File.Exists(newpath))
                                        {
                                                // File with same name but different nodeID already exists.
                                                // may be a delet and upload of a file with same name has happened after backup is taken. skipping for now.
                                                return 3;
                                        }
                                }
                                else
                                {
                                        // The file is renamed after taking backup. so the file name is different but node id is same. 
                                        // Get current file name with the node ID and overwrite this content with the backedup data.
                                        newpath = node.GetFullPath(col);
                                        log.Debug("newpath in else: {0}", newpath);
                                }
 
                                // Check for the existence of the directory... 
                                if( !Directory.Exists( Path.GetDirectoryName(newpath)))
                                {
                                        log.Info("new path directory does not exist. {0}", Path.GetDirectoryName(newpath));
                                        return 4;
                                }
                                log.Info("fetching parent dir node. {0}", Path.GetDirectoryName(relativepath));
                                // Get the dirnode for the parent directory...
                                DirNode parentnode = new DirNode( col.GetNodeByPath(Path.GetDirectoryName(relativepath)));
                                if( parentnode == null)
                                        return 6;
                                try
                                {
                                        File.Copy( backedpath, newpath, true);
                                }
                                catch(Exception e)
                                {
                                        log.Debug("Exception while copy: {0}--{1}", e.Message, e.StackTrace);
                                        return 5;
                                }
 
                                // Create the file node...
                                if( node == null)
                                {
                                        log.Debug("Creating file node. parent ID: {0}", parentnode.ID);
                                        node = new FileNode(col, parentnode, Path.GetFileName(newpath));
                                }
                                if( node == null)
                                        return 7;
                                node.UpdateWebFileInfo(col, nodelength);
                                col.Commit(node);
                                if( col.EncryptionAlgorithm == string.Empty || col.EncryptionAlgorithm == null)
                                {
                                        log.Debug("Creating hash map file for restored file: {0}", node.ID);
                                        HashMap map = new HashMap(col, node);
                                        map.CreateHashMapFile();
                                }
                                log.Debug("End restoredata.");
                        }
                        catch(Exception e1)
                        {
                                log.Info("Exception in restoredata: {0}--{1}", e1.Message, e1.StackTrace);      
                                return 8;
                        }
                        return 0;
 
                }
			





		#endregion
	}

	/// <summary>
	/// Key class, only TripleDES algorithmsupported
	/// </summary>
	public  class Key 
	{
		/// <summary>
		/// Key Name
		/// </summary>
		string	CryptoKey;

		/// <summary>
		/// Key Size
		/// </summary>
		int		CryptoKeySize;

		/// <summary>
		/// Constructs  the object
		/// </summary>
		/// <param name="CrypKey"> it is Base64 string</param>
		public Key(string CrypKey)
		{
			CryptoKey	= CrypKey;
		}

		/// <summary>
		/// Constructs a the blob object
		/// <param name="KeySize"> Size of key to create in bits</param>
		/// </summary>
		public Key(int KeySize)
		{
			CryptoKeySize	= KeySize;
		
			//only TripleDES supported
			TripleDESCryptoServiceProvider tDesKey = new TripleDESCryptoServiceProvider();
			tDesKey.KeySize	= CryptoKeySize;
			tDesKey.GenerateKey();

			CryptoKey	= Convert.ToBase64String(tDesKey.Key);
		}
		
		/// <summary>
		/// Returns the cryptio key associated with the object
		/// </summary>
		public string GetKey()
		{
			//ADD  a property instead of function
			return this.CryptoKey;			
		}

		/// <summary>
		/// Blob the key object
		/// </summary>		
		public string HashKey()
		{
//			UTF8Encoding utf8 = new UTF8Encoding();
			SHA1 sha = new SHA1CryptoServiceProvider();
			byte[] hashedObject = sha.ComputeHash(Convert.FromBase64String(this.CryptoKey));
			return Convert.ToBase64String(hashedObject);
		}

		/// <summary>
		/// Encrypt the key in the instance and returns
		/// </summary>
		/// <param name="PassPhrase"></param>
		/// <param name="EncryptedKey"></param>
		public void EncrypytKey(string PassPhrase, out string EncryptedKey) 
	       {
	       	this.CryptoKeySize	= (PassPhrase.Length)*8;

			UTF8Encoding utf8 = new UTF8Encoding();
			TripleDESCryptoServiceProvider m_des = new TripleDESCryptoServiceProvider();

			byte[] IV ={0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};			
			m_des.KeySize = this.CryptoKeySize;

			//m_des.Mode = CipherMode.CBC;
			//m_des.Padding = PaddingMode.PKCS7;			
			
			byte[] input = Convert.FromBase64String(this.CryptoKey);
			byte[] output = Transform(input, m_des.CreateEncryptor(utf8.GetBytes(PassPhrase), IV));
			EncryptedKey = Convert.ToBase64String(output);
			m_des.Clear();
	       }

		/// <summary>
		///
		/// </summary>
		/// <param name="PassPhrase"></param>
		/// <param name="EncryptedKey"></param>
		public void EncrypytKey(byte[] PassPhrase, out string EncryptedKey) 
	       {
	       	this.CryptoKeySize	= (PassPhrase.Length)*8;

//			UTF8Encoding utf8 = new UTF8Encoding();
			TripleDESCryptoServiceProvider m_des = new TripleDESCryptoServiceProvider();

			byte[] IV ={0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};			
			m_des.KeySize = this.CryptoKeySize;

			//m_des.Mode = CipherMode.CBC;
			//m_des.Padding = PaddingMode.PKCS7;			
			
			byte[] input = Convert.FromBase64String(this.CryptoKey);
			byte[] output = Transform(input, m_des.CreateEncryptor(PassPhrase, IV));
			EncryptedKey = Convert.ToBase64String(output);
			m_des.Clear();
	       }

		/// <summary>
		/// Decrypt the key in the instance and returns
		/// </summary>
		/// <param name="PassPhrase"></param>
		/// <param name="DecryptedKey"></param>	
		public void DecrypytKey(string PassPhrase, out string DecryptedKey) 
		{
			this.CryptoKeySize	= (PassPhrase.Length)*8;
			
			UTF8Encoding utf8 = new UTF8Encoding();
			TripleDESCryptoServiceProvider m_des = new TripleDESCryptoServiceProvider();

			byte[] IV ={0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};			
			m_des.KeySize = this.CryptoKeySize;
			
			//m_des.Mode = CipherMode.CBC;
			//m_des.Padding = PaddingMode.PKCS7;

			byte[] input = Convert.FromBase64String(this.CryptoKey);
			byte[] output = Transform(input, m_des.CreateDecryptor(utf8.GetBytes(PassPhrase), IV));
			DecryptedKey = Convert.ToBase64String(output);
			m_des.Clear();
		}

		public void DecrypytKey(byte[] PassPhrase, out string DecryptedKey) 
		{
			this.CryptoKeySize	= (PassPhrase.Length)*8;
			
//			UTF8Encoding utf8 = new UTF8Encoding();
			TripleDESCryptoServiceProvider m_des = new TripleDESCryptoServiceProvider();

			byte[] IV ={0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0};			
			m_des.KeySize = this.CryptoKeySize;
			
			//m_des.Mode = CipherMode.CBC;
			//m_des.Padding = PaddingMode.PKCS7;

			byte[] input = Convert.FromBase64String(this.CryptoKey);
			byte[] output = Transform(input, m_des.CreateDecryptor(PassPhrase, IV));
			DecryptedKey = Convert.ToBase64String(output);
			m_des.Clear();
		}
		
		/// <summary>
		/// Internal to the class
		/// </summary>
		private byte[] Transform(byte[] input,  ICryptoTransform CryptoTransform)
		{
			// create the necessary streams
			MemoryStream memStream = new MemoryStream();
			CryptoStream cryptStream = new CryptoStream(memStream, CryptoTransform, CryptoStreamMode.Write);
			// transform the bytes as requested
			cryptStream.Write(input, 0, input.Length);
			cryptStream.FlushFinalBlock();
			// Read the memory stream and
			// convert it back into byte array
			memStream.Position = 0;
			byte[] result = memStream.ToArray();
			// close and release the streams
			memStream.Close();
			cryptStream.Close();
			// hand back the encrypted buffer
			return result;
		}
	}
}

