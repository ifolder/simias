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
*                 $Author: Brady Anderson <banderso@novell.com>
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
using System.Net;
using System.Reflection;

using Simias;
using Simias.DomainServices;
using Simias.Event;
using Simias.Storage;
using Simias.Sync;

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
		/// The version of the server and client does not match. to be upgraded.
		/// </summary>
		OlderVersion = 0x1f000103,

		/// <summary>
		/// The version of the server is older.
		/// </summary>
		ServerOld = 0x1f000104,

		/// <summary>
		/// The version of the server is older.
		/// </summary>
		UpgradeNeeded = 0x1f000105,

		/// <summary>
		/// The version of the server is older.
		/// </summary>
		PassPhraseNotSet = 0x1f000106,

		/// <summary>
		/// The version of the server is older.
		/// </summary>
		PassPhraseInvalid = 0x1f000107,

		/// <summary>
        	/// The version of the server is older.
        	/// </summary>
        	ServerUnAvailable = 0x1f000108,

		/// <summary>
		/// Authentication failed with an unknown reason
		/// </summary>
		Unknown = 0x1f001fff,

		/// <summary>
		/// Authentication failed as user is move to different server
		/// </summary>
		UserAlreadyMoved = 0x1f000108
	}

	/// <summary>
	/// Defines the Status class which
	/// is returned on all remote authentication methods.
	/// </summary>
	[Serializable]
	public class Status
	{
        /// <summary>
        /// Set Status to Unknown
        /// </summary>
		public Status()
		{
			statusCode = StatusCodes.Unknown;
		}

        /// <summary>
        /// Set method for Status
        /// </summary>
        /// <param name="status">status to set</param>
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

		/// <summary>
		/// The number of days until the users password expires
		/// 
		/// Valid if status == StatusCode.Success
		/// </summary>
		public int				DaysUntilPasswordExpires = -1;
	}
	
	/// <summary>
	/// Class for maintaining cached Http Basic
	/// credential sets
	/// </summary>
	public class BasicCredentials : SimiasCredentials
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
		private Store store;
		private string password;
		
		#region Properties
		/// <summary>
		/// clear text password 
		/// </summary>
		public string Password
		{
			get{ return this.password; }
			set{ this.password = value; }
		}
		
		/*
		/// <summary>
		/// Username for this credential set
		/// this property is only valid after credentials
		/// have been retrieved or persisted.
		/// </summary>
		public string Username
		{
			get{ return this.username; }
		}
		*/
		#endregion
		
		#region Constructors
		
		/// <summary>
		/// Attempt to load the CASA assembly
		/// </summary>
		static BasicCredentials()
		{
			try
			{
				Simias.Authentication.BasicCredentials.casaAssembly = 
					Assembly.LoadWithPartialName( casaAssemblyName );
				if ( casaAssembly != null )
				{
					log.Debug( "found CASA assembly" );
				}
			}
			catch{}
		}
		
		#region Private Methods
        /// <summary>
        /// Validates the arguments needed for credentials
        /// </summary>
        /// <param name="DomainID">ID of the domain for which credentials are to be validated</param>
        /// <param name="CollectionID">ID of the Collection for which credentials are to be validated</param>
        /// <param name="MemberID">Id of the Member for which credentials are to be validated</param>
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
			
			/*
			this.member = domain.GetMemberByID( MemberID );
			if ( member == null )
			{
				throw new ArgumentException( MemberID );
			}
			
			this.username = member.Name;
			*/
		}
		
		#endregion
		
		/// <summary>
		/// Construct a credential set based on Domain, Collection and Member
		/// If the domain is of type enterprise and scoping is done solely on
		/// the domain, the DomainID should be used for the collection as well.
		///	During construction, the system will attempt to find a full credential
		/// set in cache.  If a full set exists, the Cached property will return
		/// true and all credential properties will be valid.
		/// </summary>
		public BasicCredentials( string DomainID, string CollectionID, string ID ) :
				base( DomainID, CollectionID, ID )
		{
//			this.store = Store.GetStore();
			//ValidateArguments( DomainID, CollectionID, MemberID );
			
			if ( this.Cached == true )
			{
				this.password = this.Blob as string;
			}
#if CASA			
			else
			if ( store.DefaultDomain == DomainID && casaAssembly != null )
			{
				try
				{
					log.Debug( "  attempting to retrieve credentials using CASA's single-signon secret" );
					
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
						
					if ( casaPassword != null && casaUsername != null && (string) casaUsername == ID )
					{
						this.password = casaPassword.ToString();
						this.Save( false );
					}
				}
				catch( Exception e )
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
				}
			}
#endif //CASA			
		}
		
		/// <summary>
		/// Construct a full credential needed for satisfying Http BASIC
		/// authentication.  If a credential set already exists on the
		/// domain or collection it will be overwritten and removed from
		/// the cache with this credential.
		/// </summary>
		public BasicCredentials( string DomainID, string CollectionID, string ID, string Password ) :
			base( DomainID, CollectionID, ID )
		{
			this.store = Store.GetStore();
			
			// A credential for this domain or collection already existed
			// in the cache so let's remove the old one
			if ( this.Cached == true )
			{
				base.Remove();
			}
			
			//ValidateArguments( DomainID, CollectionID, MemberID );
			this.password = Password;
		}
		#endregion
		
		#region Public Methods
        /// <summary>
        /// Save the password (credentials)
        /// </summary>
        /// <param name="persistent">To be permanent or temporary</param>
		public override void Save( bool persistent )
		{
			/*
			if ( this.username == null )
			{
				throw new NotExistException( "username" );
			}
			*/
			
			if ( this.password == null )
			{
				throw new NotExistException( "password" );
			}
			
			this.Blob = password as object;
			base.Save( persistent );			
		}

		/// <summary>
		/// Gets the credentials (if they exist) that are set against
		/// the collection ID passed in the constructor.
		/// </summary>
		/// <returns>NetworkCredential object which can be assigned to the "Credentials" property in a proxy class.</returns>
		public NetworkCredential GetNetworkCredential()
		{
			NetworkCredential realCreds = null;

			try
			{
				if ( this.Cached == true )
				{
					realCreds = new NetworkCredential( this.ID, this.password );
				}
			}
			catch( Exception gnc )
			{
				log.Error( gnc.Message );
				log.Error( gnc.StackTrace );
			}
			
			return( realCreds );
		}
		#endregion
	}

	/// <summary>
	/// Summary description for Credentials
	/// </summary>
	public class SimiasCredentials
	{
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
	
		//private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Credentials));
//		//private Store store;
		
		private class CredentialSet
		{
			public bool				Default;
			public bool				Persisted;
			public bool				Cached;
			public string 			DomainID;
			public string			CollectionID;
			public string			ID;
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
			set{ this.credentials.Default = value; }
		}
		
		/// <summary>
		/// DomainID for this credential set 
		/// </summary>
		public string DomainID
		{
			get{ return this.credentials.DomainID; }
		}
		
		/// <summary>
		/// Principal or ID for this credential set 
		/// </summary>
		public string ID
		{
			get{ return this.credentials.ID; }
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
		/// Construct a credential set based on Domain, Collection and ID
		/// If the domain is of type enterprise and scoping is done solely on
		/// the domain, the DomainID should be used for the collection as well.
		///	During construction, the system will attempt to find a full credential
		/// set in cache.  If a full set exists, the Cached property will return
		/// true and all credential properties will be vallid.
		/// </summary>
		public SimiasCredentials( string DomainID, string CollectionID, string ID )
		{
			if ( DomainID == null )
			{
				throw new ArgumentException( "DomainID" );
			}
			
			if ( CollectionID == null )
			{
				throw new ArgumentException( "CollectionID" );
			}
			
			if ( ID == null )
			{
				throw new ArgumentException( "ID" );
			}
			
			this.credentials = new CredentialSet();
			this.credentials.DomainID = DomainID;
			this.credentials.CollectionID = CollectionID;
			this.credentials.ID = ID;
			this.RetrieveFromCache();
		}
		#endregion
		
		#region Private Methods
        /// <summary>
        /// Get the credentials from Cache if available
        /// </summary>
        /// <returns></returns>
		private bool RetrieveFromCache()
		{
			bool retrieved;
			CredentialSet creds = null;
			
			string key = ( credentials.DomainID != credentials.CollectionID )
							? credentials.CollectionID : credentials.DomainID;
							
			lock( typeof( Simias.Authentication.SimiasCredentials ) )
			{
				if ( SimiasCredentials.credentialList.ContainsKey( key ) )
				{
					creds = SimiasCredentials.credentialList[ key ] as CredentialSet;
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
			log.Debug( "Credential::Save called" );
			lock( typeof( Simias.Authentication.SimiasCredentials ) )
			{
				string key = 
					( this.credentials.DomainID == this.credentials.CollectionID )
						? this.credentials.DomainID : this.credentials.CollectionID;
				if ( SimiasCredentials.credentialList.ContainsKey( key ) == true )
				{
					SimiasCredentials.credentialList.Remove( key );			
				}
			
				log.Debug( "adding: " + key + " to the cache" );
				SimiasCredentials.credentialList.Add( key, this.credentials );
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
			lock( typeof( Simias.Authentication.SimiasCredentials ) )
			{
				string key = 
					( this.credentials.DomainID == this.credentials.CollectionID )
						? this.credentials.DomainID : this.credentials.CollectionID;
				if ( SimiasCredentials.credentialList.ContainsKey( key ) == true )
				{
					SimiasCredentials.credentialList.Remove( key );			
				}
			}
		}
		#endregion
	}
}
