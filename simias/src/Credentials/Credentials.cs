/***********************************************************************
 *  $RCSfile: Credentials.cs,v $
 *
 *  Copyright  Unpublished Work of Novell, Inc. All Rights Reserved.
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
using System.IO;
using System.Net;
using System.Reflection;

using Simias;
using Simias.DomainServices;
using Simias.Event;
using Simias.Storage;
using Simias.Sync;
using Novell.Security.ClientPasswordManager;

namespace Simias.Authentication
{
	/// <summary>
	/// status codes returned by remote authentication modules
	/// </summary>
	[Serializable]
	public enum StatusCodes : uint
	{
		/// <summary>
		/// Successful authentication
		/// </summary>
		Success = 0x00000000,

		/// <summary>
		/// Successful authentication but within a grace login period
		/// </summary>
		SuccessInGrace = 0x00000001,

		/// <summary>
		/// The certificate is invalid.
		/// </summary>
		InvalidCertificate = 0x00000002,

		/// <summary>
		/// Invalid or Unknown user specified
		/// </summary>
		UnknownUser = 0x1f000001,

		/// <summary>
		/// Ambigous user - more than one user exists 
		/// </summary>
		AmbiguousUser = 0x1f000002,

		/// <summary>
		/// The credentials may have invalid characters etc.
		/// </summary>
		InvalidCredentials = 0x1f000003,

		/// <summary>
		/// Invalid password specified
		/// </summary>
		InvalidPassword = 0x1f000020,

		/// <summary>
		/// The account has been disabled by an administrator
		/// </summary>
		AccountDisabled = 0x1f000040,

		/// <summary>
		/// The account has been locked due to excessive login failures
		/// or possibly the grace logins have all been consumed
		/// </summary>
		AccountLockout = 0x1f000041,

		/// <summary>
		/// The simias account has been disabled by the administrator.
		/// </summary>
		SimiasLoginDisabled = 0x1f000042,

		/// <summary>
		/// The specified domain was unknown
		/// </summary>
		UnknownDomain = 0x1f000060,

		/// <summary>
		/// Authentication failed due to an internal exception
		/// </summary>
		InternalException = 0x1f000100,

		/// <summary>
		/// The authentication provider does not support the method
		/// </summary>
		MethodNotSupported = 0x1f000101,

		/// <summary>
		/// The operation timed out on the client request
		/// </summary>
		Timeout = 0x1f000102,

		/// <summary>
		/// Authentication failed with an unknown reason
		/// </summary>
		Unknown = 0x1f001fff
	}

	/// <summary>
	/// Defines the Status class which
	/// is returned on all remote authentication methods.
	/// </summary>
	[Serializable]
	public class Status
	{
		public Status()
		{
			statusCode = StatusCodes.Unknown;
		}

		public Status(StatusCodes status)
		{
			statusCode = status;
		}

		/// <summary>
		/// Status of the authentication.
		/// Must always be a valid status code
		/// </summary>
		public StatusCodes		statusCode;

		/// <summary>
		/// Unique ID of the domain.
		/// Valid on a successful authentication.
		/// </summary>
		public string			DomainID;

		/// <summary>
		/// Unique ID of the user
		/// Valid on a successful authentication
		/// </summary>
		public string			UserID;

		/// <summary>
		/// UserName 
		/// 
		/// Valid if the authentication was successful
		/// </summary>
		public string			UserName;

		/// <summary>
		/// Distinguished or unique user name used for
		/// the authentication.  This member can be
		/// the same as the UserName
		/// 
		/// Valid if the authentication was successful
		/// </summary>
		public string			DistinguishedUserName;

		/// <summary>
		/// ExceptionMessage returned when an internal
		/// exception occurred while trying to authenticate
		/// the user.
		/// 
		/// Valid if status == StatusCode.InternalException
		/// </summary>
		public string			ExceptionMessage;

		/// <summary>
		/// TotalGraceLogins the number of allowed on this account by policy
		/// 
		/// Valid if status == StatusCode.SuccessInGrace
		/// </summary>
		public int				TotalGraceLogins;

		/// <summary>
		/// RemainingGraceLogins the number of grace logins left on this account
		/// 
		/// Valid if status == StatusCode.SuccessInGrace
		/// </summary>
		public int				RemainingGraceLogins;
	}

	/// <summary>
	/// Summary description for Credentials
	/// </summary>
	public class Credentials
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Credentials));
		private string collectionID;
		private string memberID;
		private string domainID;
		private Store store;

		/// <summary>
		/// Constructor for checking if credentials exist for collection
		/// </summary>
		public Credentials(string collectionID)
		{
			this.collectionID = collectionID;
			this.store = Store.GetStore();
		}

		/// <summary>
		/// Constructor for checking if credentials exist for a domain
		/// and a member
		/// </summary>
		public Credentials(string domainID, string memberID)
		{
			this.domainID = domainID;
			this.memberID = memberID;
			this.store = Store.GetStore();
		}

		/// <summary>
		/// Constructor for checking if credentials exist for a 
		/// collection and member
		/// </summary>
		public Credentials(string domainID, string collectionID, string memberID)
		{
			this.domainID = domainID;
			this.collectionID = collectionID;
			this.memberID = memberID;
			this.store = Store.GetStore();
		}


		/// <summary>
		/// Gets the credentials (if they exist) that are set against
		/// the collection ID passed in the constructor.
		/// </summary>
		/// <returns>NetworkCredential object which can be assigned to the "Credentials" property in a proxy class.</returns>
		public NetworkCredential GetCredentials()
		{
			//
			// From the collection ID we need to figure out
			// the Realm, Username etc.
			//

			NetworkCredential realCreds = null;
			Simias.Storage.Domain cDomain = null;
			Simias.Storage.Member cMember = null;

			string memberName = "";
			string memberID = this.memberID;

			try
			{
				if ( this.domainID != null && this.collectionID != null && this.memberID != null )
				{
					cDomain = store.GetDomain( this.domainID );
					if ( cDomain != null )
					{
						cMember = cDomain.GetMemberByID( this.memberID );
						if ( cMember != null )
						{
							memberName = cMember.Name;
							memberID = cMember.UserID;
						}
					}
				}
				else
				if ( this.collectionID != null )
				{
					// Validate the shared collection
					Collection cCol = this.store.GetCollectionByID( collectionID );
					if ( cCol != null )
					{
						cMember = 
							( this.memberID != null ) 
								? cCol.GetMemberByID( this.memberID ) 
								: cCol.GetCurrentMember();
						if ( cMember != null )
						{
							memberName = cMember.Name;
							memberID = cMember.UserID;
							cDomain = this.store.GetDomain( cCol.Domain );
						}
						else
						{
							log.Debug( "Credentials::GetCredentials - current member not found" );
						}
					}
					else
					{
						log.Debug( "Credentials::GetCredentials - collection not found" );
					}
				}
				else
				{
					cDomain = this.store.GetDomain( this.domainID );
					cMember = cDomain.GetMemberByID( this.memberID );
					if ( cMember != null )
					{
						memberName = cMember.Name;
						memberID = cMember.UserID;
					}
				}

				//
				// Verify the domain is not marked "inactive" and that a non-workgroup
				// domain is marked authenticated.
				//

				DomainAgent domainAgent = new DomainAgent();
				if ( domainAgent.IsDomainActive( cDomain.ID ) &&
					 (domainAgent.IsDomainAuthenticated( cDomain.ID ) ||
					 cDomain.ConfigType.Equals(Simias.Storage.Domain.ConfigurationType.Workgroup)))
				{
					NetCredential cCreds = 
						new NetCredential(
							"iFolder", 
							this.domainID,
							true, 
							memberName,
							null );

					Uri cUri = DomainProvider.ResolveLocation( this.domainID );
					realCreds = cCreds.GetCredential( cUri, "BASIC" );
					if ( realCreds == null )
					{
						// Check if creds exist for the user ID
						cCreds = 
							new NetCredential(
								"iFolder", 
								this.domainID, 
								true, 
								memberID,
								null );

						realCreds = cCreds.GetCredential( cUri, "BASIC" );
						if ( realCreds == null && this.collectionID != null )
						{
							// Check if creds exist for the user ID and by collection
							cCreds = 
								new NetCredential(
								"iFolder", 
								this.collectionID, 
								true, 
								memberID,
								null );

							realCreds = cCreds.GetCredential( cUri, "BASIC" );
						}
					}
				}
			}
			catch{}
			return(realCreds);
		}
	}
	
	/// <summary>
	/// Class for maintaining cached Http Basic
	/// credential sets
	/// </summary>
	public class HttpBasicCredentials : TempCredentials
	{
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
		private static Assembly casaAssembly = null;
		private static string casaAssemblyName = "Novell.CASA.miCASAWrapper";
		private static string casaClassName = "Novell.CASA.miCASA";
		private static string serviceName = "Simias";
		private static string singleSignOnID = "Network";
		
		private Collection collection;
		private Domain domain;
		private Member member;
		private Store store;
		
		private string	username;
		private string	password;

		private class HttpBasicBlob
		{
			public string Username;
			public string Password;
		}
		
		#region Properties
		/// <summary>
		/// clear text password 
		/// </summary>
		public string Password
		{
			get{ return this.password; }
			set{ this.password = value; }
		}
		
		/// <summary>
		/// Username for this credential set
		/// this property is only valid after credentials
		/// have been retrieved or persisted.
		/// </summary>
		public string Username
		{
			get{ return this.username; }
		}
		#endregion
		
		#region Constructors
		
		/// <summary>
		/// Attempt to load the CASA assembly
		/// </summary>
		static HttpBasicCredentials()
		{
			try
			{
				Simias.Authentication.HttpBasicCredentials.casaAssembly = 
					Assembly.LoadWithPartialName( casaAssemblyName );
				if ( casaAssembly != null )
				{
					Console.WriteLine( "Found casa assembly!" );
				}
			}
			catch{}
		}
		
		#region Private Methods
		private void ValidateArguments( string DomainID, string CollectionID, string MemberID )
		{
			this.domain = this.store.GetDomain( DomainID );
			if ( this.domain == null )
			{
				throw new ArgumentException( DomainID );
			}
			
			this.collection = this.store.GetCollectionByID( CollectionID );
			if ( this.collection == null )
			{
				throw new ArgumentException( CollectionID );
			}
			
			this.member = domain.GetMemberByID( MemberID );
			if ( member == null )
			{
				throw new ArgumentException( MemberID );
			}
		}
		
		#endregion
		
		/// <summary>
		/// Construct a credential set based on Domain, Collection and Member
		/// If the domain is of type enterprise and scoping is done solely on
		/// the domain, the DomainID should be used for the collection as well.
		///	During construction, the system will attempt to find a full credential
		/// set in cache.  If a full set exists, the Cached property will return
		/// true and all credential properties will be vallid.
		/// </summary>
		public HttpBasicCredentials( string DomainID, string CollectionID, string MemberID, bool DefaultDomain ) :
				base( DomainID, CollectionID, MemberID, DefaultDomain )
		{
			if ( DomainID == null || CollectionID == null || MemberID == null )
			{
				throw new ArgumentException( "arguments null" );
			}
			
			this.store = Store.GetStore();
			ValidateArguments( DomainID, CollectionID, MemberID );
			
			if ( this.Cached == true )
			{
				HttpBasicBlob blob = this.Blob as HttpBasicBlob;
				if ( blob != null )
				{
					this.username = blob.Username;
					this.password = blob.Password;
				}
			}
			else
			if ( DefaultDomain == true && casaAssembly != null )
			{
				try
				{
					log.Debug( "  attempting to retrieve credentials from CASA's single-signon secret" );
					
					// If the credential is default and CASA is present, attempt
					// to retrieve a credential based on the well-known single signon
					// ID of "Network"
					
					object[] args = new object[2];
					args[0] = serviceName;
					args[1] = singleSignOnID;
						
					Type type = casaAssembly.GetType( casaClassName );
					object casaInstance = Activator.CreateInstance( type );
							
					// Get the username and password
					object casaUsername = 
						type.InvokeMember( 
							"GetCredentialUsername",
							BindingFlags.InvokeMethod,
							null,
							casaInstance,
							args);
								
					object casaPassword = 
						type.InvokeMember( 
							"GetCredentialPassword",
							BindingFlags.InvokeMethod,
							null,
							casaInstance,
							args);
						
					if ( casaPassword != null && casaUsername != null )
					{
						this.username = casaUsername.ToString();
						this.password = casaPassword.ToString();
						
						this.Save( false );
					}
				}
				catch( Exception e )
				{
					Console.WriteLine( e.Message );
					Console.WriteLine( e.StackTrace );
				}
			}
		}
		
		/// <summary>
		/// Construct a full credential needed for satisfying Http BASIC
		/// authentication.  If a credential set already exists on the
		/// domain or collection it will be overwritten and removed from
		/// the cache with this credential.
		/// </summary>
		public HttpBasicCredentials( string DomainID, string CollectionID, string MemberID, bool DefaultDomain, string Password ) :
				base( DomainID, CollectionID, MemberID, DefaultDomain )
		{
			if ( DomainID == null || CollectionID == null || MemberID == null )
			{
				throw new ArgumentException( "arguments null" );
			}
			
			this.store = Store.GetStore();
			ValidateArguments( DomainID, CollectionID, MemberID );
			
			// A credential for this domain or collection already existed
			// in the cache so let's remove the old one
			if ( this.Cached == true )
			{
				base.Remove();
			}
			
			this.password = Password;
		}
		#endregion
		
		#region Public Methods
		public override void Save( bool persistent )
		{
			if ( this.username == null || this.password == null )
			{
				//throw new NotExistException( "password" );
			}
			
			HttpBasicBlob blob = new HttpBasicBlob();
			blob.Username = username;
			blob.Password = password;
			this.Blob = blob as object;
			base.Save( persistent );			
		}

		/// <summary>
		/// Gets the credentials (if they exist) that are set against
		/// the collection ID passed in the constructor.
		/// </summary>
		/// <returns>NetworkCredential object which can be assigned to the "Credentials" property in a proxy class.</returns>
		public NetworkCredential GetNetworkCredential()
		{
			//
			// From the collection ID we need to figure out
			// the Realm, Username etc.
			//

			NetworkCredential realCreds = null;
			//Simias.Storage.Domain cDomain = null;
			//Simias.Storage.Member cMember = null;

			//string memberName = "";
			//string memberID = this.memberID;

			try
			{
				if ( this.Cached == true )
				{
					realCreds = new NetworkCredential();
					realCreds.Domain = this.DomainID;
					realCreds.UserName = this.Username;
					realCreds.Password = this.Password;
				}

				//
				// Verify the domain is not marked "inactive" and that a non-workgroup
				// domain is marked authenticated.
				//

				/*
				DomainAgent domainAgent = new DomainAgent();
				if ( domainAgent.IsDomainActive( cDomain.ID ) &&
					 (domainAgent.IsDomainAuthenticated( cDomain.ID ) ||
					 cDomain.ConfigType.Equals(Simias.Storage.Domain.ConfigurationType.Workgroup)))
				{
					NetCredential cCreds = 
						new NetCredential(
							"iFolder", 
							this.domainID,
							true, 
							memberName,
							null );

					Uri cUri = DomainProvider.ResolveLocation( this.domainID );
					realCreds = cCreds.GetCredential( cUri, "BASIC" );
					if ( realCreds == null )
					{
						// Check if creds exist for the user ID
						cCreds = 
							new NetCredential(
								"iFolder", 
								this.domainID, 
								true, 
								memberID,
								null );

						realCreds = cCreds.GetCredential( cUri, "BASIC" );
						if ( realCreds == null && this.collectionID != null )
						{
							// Check if creds exist for the user ID and by collection
							cCreds = 
								new NetCredential(
								"iFolder", 
								this.collectionID, 
								true, 
								memberID,
								null );

							realCreds = cCreds.GetCredential( cUri, "BASIC" );
						}
					}
				}
				*/
			}
			catch{}
			return( realCreds );
		}
		#endregion
	}

	/// <summary>
	/// Summary description for Credentials
	/// </summary>
	public class TempCredentials
	{
		//private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Credentials));
//		//private Store store;
		
		private class CredentialSet
		{
			public bool				Default;
			public bool				Persisted;
			public bool				Cached;
			public string 			DomainID;
			public string			CollectionID;
			public string			MemberID;
			public object			Blob;
		}
		
		private CredentialSet credentials = null;
		static private Hashtable credentialList = new Hashtable();

		#region Properties
		/// <summary>
		/// Returns true if this credential set has been cached
		/// false if the credentials have not been cached
		/// </summary>
		public object Blob
		{
			get{ return this.credentials.Blob; }
			set{ this.credentials.Blob = value; }
		}

		/// <summary>
		/// Returns true if this credential set has been cached
		/// false if the credentials have not been cached
		/// </summary>
		public bool Cached
		{
			get{ return this.credentials.Cached; }
		}

		/// <summary>
		/// CollectionID for this credential set 
		/// </summary>
		public string CollectionID
		{
			get{ return this.credentials.CollectionID; }
		}
		
		/// <summary>
		/// Returns true if this credential set is default
		/// default is the default Simias domain
		/// If default, the credential class will attempt
		/// to leverage CASA's "Network" credential set
		/// </summary>
		public bool Default
		{
			get{ return this.credentials.Default; }
		}
		
		/// <summary>
		/// DomainID for this credential set 
		/// </summary>
		public string DomainID
		{
			get{ return this.credentials.MemberID; }
		}
		
		/// <summary>
		/// MemberID for this credential set 
		/// </summary>
		public string MemberID
		{
			get{ return this.credentials.MemberID; }
		}
		
		/// <summary>
		/// Returns true if the credentials have been persisted to long term storage
		/// false if the credentials are not persisted.
		/// </summary>
		public bool Persisted
		{
			get{ return this.credentials.Persisted; }
		}
		#endregion


		#region Constructors
		/// <summary>
		/// Construct a credential set based on Domain, Collection and Member
		/// If the domain is of type enterprise and scoping is done solely on
		/// the domain, the DomainID should be used for the collection as well.
		///	During construction, the system will attempt to find a full credential
		/// set in cache.  If a full set exists, the Cached property will return
		/// true and all credential properties will be vallid.
		/// </summary>
		public TempCredentials( string DomainID, string CollectionID, string MemberID, bool DefaultDomain )
		{
			this.credentials = new CredentialSet();
			this.credentials.DomainID = DomainID;
			this.credentials.CollectionID = CollectionID;
			this.credentials.MemberID = MemberID;
			this.credentials.Default = DefaultDomain;
			this.RetrieveFromCache();
		}
		#endregion
		
		#region Private Methods
		private bool RetrieveFromCache()
		{
			bool retrieved;
			CredentialSet creds = null;
			
			string key = ( credentials.DomainID != credentials.CollectionID )
							? credentials.CollectionID : credentials.DomainID;
							
			lock( typeof( Simias.Authentication.TempCredentials ) )
			{
				if ( Simias.Authentication.TempCredentials.credentialList.ContainsKey( key ) )
				{
					creds = TempCredentials.credentialList.get_Item( key ) as CredentialSet;
				}
			}
		
			if ( creds != null )
			{
				// Update internal credential set
				credentials.Cached = true;
				credentials.Persisted = false;
				credentials.Blob = creds.Blob;
				retrieved = true;
			}
			else
			{
				credentials.Cached = false;
				credentials.Persisted = false;
				retrieved = false;
			}
					
			return retrieved;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Save the credentials to a volatile cache
		/// persistent == true - save to a long term cache
		/// </summary>
		public virtual void Save( bool persistent )
		{
			Console.WriteLine( "Credential::Save called" );
			lock( typeof( Simias.Authentication.TempCredentials ) )
			{
				string key = 
					( this.credentials.DomainID == this.credentials.CollectionID )
						? this.credentials.DomainID : this.credentials.CollectionID;
				if ( Simias.Authentication.TempCredentials.credentialList.ContainsKey( key ) == true )
				{
					Simias.Authentication.TempCredentials.credentialList.Remove( key );			
				}
			
				Console.WriteLine( "adding: " + key + " to the cache" );
				Simias.Authentication.TempCredentials.credentialList.Add( key, this.credentials );
			}
			
			this.credentials.Cached = true;
		}
		
		/// <summary>
		/// Remove the credential from the volatile cache
		/// If the credential has been persisted, remove it from
		/// that cache as well.
		/// </summary>
		public virtual void Remove()
		{
			lock( typeof( Simias.Authentication.TempCredentials ) )
			{
				string key = 
					( this.credentials.DomainID == this.credentials.CollectionID )
						? this.credentials.DomainID : this.credentials.CollectionID;
				if ( Simias.Authentication.TempCredentials.credentialList.ContainsKey( key ) == true )
				{
					Simias.Authentication.TempCredentials.credentialList.Remove( key );			
				}
			}
		}
		#endregion
		
		public static void Main(string[] args)
		{
			//Console.WriteLine( "Setting credential" );
	    
	    	try
	    	{
	    		// Create a set of credentials, cache them and then retrieve 
	    		// them back from cache.
	    		
	    		string domainID = Guid.NewGuid().ToString();
	    		string memberID = Guid.NewGuid().ToString();
	    		string password = "heyya";
	    		
	    		Console.WriteLine( "Caching credential set:" );
	    		Console.WriteLine( "  domain: " + domainID );
	    		Console.WriteLine( "  member: " + memberID );
	    		Console.WriteLine( "  password: " + password );
	    		
	    		Simias.Authentication.HttpBasicCredentials setOne = 
	    			new HttpBasicCredentials( domainID, domainID, memberID, false, password );
				setOne.Save( false );
	    		
	    		// Now retrieve them
	    		Console.WriteLine( "Retrieving credential set:" );
	    		
	    		HttpBasicCredentials setTwo =
	    			new HttpBasicCredentials( domainID, domainID, memberID, false );
	    			
	    		if ( setTwo.Cached == true )
	    		{
		    		Console.WriteLine( "  domain: " + domainID );
		    		Console.WriteLine( "  password: " + setTwo.Password );
	    		}
	    		else
	    		{
	    			Console.WriteLine( " could not find a full credential set for domain: " + domainID );
	    		}


				string collectionID = Guid.NewGuid().ToString();
				password = "zippy";
				
	    		Console.WriteLine( "Caching credential set based on a collection:" );
	    		Console.WriteLine( "  domain: " + domainID );
	    		Console.WriteLine( "  collection: " + collectionID );
	    		Console.WriteLine( "  member: " + memberID );
	    		Console.WriteLine( "  password: " + password );
	    		
	    		HttpBasicCredentials setThree = 
	    			new HttpBasicCredentials( domainID, collectionID, memberID, false, password );
	    		setThree.Save( false );
	    		
	    		// Now retrieve them
	    		Console.WriteLine( "Retrieving credential set:" );
	    		
	    		HttpBasicCredentials setFour =
	    			new HttpBasicCredentials( domainID, collectionID, memberID, false );
	    			
	    		if ( setFour.Cached == true )
	    		{
		    		Console.WriteLine( "  domain: " + domainID );
		    		Console.WriteLine( "  collection: " + collectionID );
		    		Console.WriteLine( "  password: " + setFour.Password );
	    		}
	    		else
	    		{
	    			Console.WriteLine( " could not find a full credential set for collection: " + collectionID );
	    		}
	    		
	    		// Attempt to get the CASA credentials
				domainID = Guid.NewGuid().ToString();
				
	    		Console.WriteLine( "Attempting to retrieve the CASA credentials:" );
	    		
	    		HttpBasicCredentials setTen = 
	    			new HttpBasicCredentials( domainID, domainID, memberID, true );
	    		if ( setTen.Cached == true )
	    		{
		    		Console.WriteLine( "  domain: " + domainID );
		    		Console.WriteLine( "  collection: " + collectionID );
		    		Console.WriteLine( "  username: " + setTen.Username );
		    		Console.WriteLine( "  password: " + setTen.Password );
	    		}
	    		else
	    		{
	    			Console.WriteLine( " could not find the CASA credentials" );
	    		}
		    }
		    catch( Exception e )
		    {
		    	Console.WriteLine( e.Message );
		    	Console.WriteLine( e.StackTrace );
		    }
		}
	}
}
