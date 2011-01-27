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
*                 $Author: Russ Young
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
using System.Web;
using System.Net;
using System.Text;
using System.Web.Services.Protocols;

using Simias.Storage;
using Simias.Authentication;
using Simias.Event;
using Simias.Client.Event;
using Simias.Client;

namespace Simias
{
	/// <summary>
	/// Class to share and keep proxy state for web communications.
	/// </summary>
	public class ProxyState : IWebProxy
	{
		#region Class Members

		private IWebProxy webProxy;
		private static Hashtable proxyHash = new Hashtable();

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="proxy">The proxy server address.</param>
		/// <param name="proxyUser">The user name for proxy authentication.</param>
		/// <param name="proxyPassword">The password for proxy authentication.</param>
		private ProxyState( Uri proxy, string proxyUser, string proxyPassword )
		{
			InitializeWebProxy( proxy, proxyUser, proxyPassword );
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Initializes the WebProxy object. 
		/// </summary>
		/// <param name="proxy">The proxy server address.</param>
		/// <param name="proxyUser">The user name for proxy authentication.</param>
		/// <param name="proxyPassword">The password for proxy authentication.</param>
		private void InitializeWebProxy( Uri proxy, string proxyUser, string proxyPassword )
		{
			if ( proxy != null )
			{
				if ( ( proxyUser == null ) || ( proxyUser == String.Empty ) )
				{
					webProxy = new WebProxy( proxy, false );
				}
				else
				{
					webProxy = new WebProxy( 
						proxy, 
						false, 
						new string[] {}, 
						new NetworkCredential( proxyUser, proxyPassword, proxy.ToString() ) );
				}
			}
			else
			{
				webProxy = GlobalProxySelection.GetEmptyWebProxy();
			}
		}

		/// <summary>
		/// Creates a proxy key to use for the specified host.
		/// </summary>
		/// <param name="server">Uri of the host.</param>
		/// <returns></returns>
		private static string ProxyKey( Uri server )
		{
			return new UriBuilder( server.Scheme, server.Host.ToLower(), server.Port ).Uri.ToString();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds a ProxyState object for the specified server address.
		/// </summary>
		/// <param name="server">The simias server address.</param>
		public static ProxyState AddProxyState( Uri server )
		{
			return AddProxyState( server, null, null, null );
		}

		/// <summary>
		/// Adds a ProxyState object for the specified server address.
		/// </summary>
		/// <param name="server">The simias server address.</param>
		/// <param name="proxy">The proxy server address.</param>
		/// <param name="proxyUser">The user name for proxy authentication.</param>
		/// <param name="proxyPassword">The password for proxy authentication.</param>
		public static ProxyState AddProxyState( Uri server, Uri proxy, string proxyUser, string proxyPassword )
		{
			lock( proxyHash )
			{
				string key = ProxyKey( server );
				ProxyState ps = proxyHash[ key ] as ProxyState;
				if ( ps != null )
				{
					ps.InitializeWebProxy( proxy, proxyUser, proxyPassword );
				}
				else
				{
					ps = new ProxyState( proxy, proxyUser, proxyPassword );
					proxyHash[ key ] = ps;
				}

				return ps;
			}
		}

		/// <summary>
		/// Deletes the specified ProxyState object for the specified server address.
		/// </summary>
		/// <param name="server">Address of the server to delete a ProxyState object for.</param>
		public static void DeleteProxyState( Uri server )
		{
			lock ( proxyHash )
			{
				proxyHash[ ProxyKey( server ) ] = null;
			}
		}

		/// <summary>
		/// Gets a ProxyState object for the specified server address.
		/// </summary>
		/// <param name="server">Address of the server to find a ProxyState object for.</param>
		/// <returns>A corresponding ProxyState object.</returns>
		public static ProxyState GetProxyState( Uri server )
		{
			ProxyState ps;

			lock ( proxyHash )
			{
				ps = proxyHash[ ProxyKey( server ) ] as ProxyState;
			}

			return ( ps == null ) ? AddProxyState( server ) : ps;
		}

		#endregion

		#region IWebProxy Members

		/// <summary>
		/// Returns the URI of a proxy.
		/// </summary>
		/// <param name="destination">A Uri specifying the requested Internet resource.</param>
		/// <returns>A Uri containing the URI of the proxy used to contact destination.</returns>
		public Uri GetProxy( Uri destination )
		{
			return webProxy.GetProxy( destination );
		}

		/// <summary>
		/// The credentials to submit to the proxy server for authentication.
		/// </summary>
		public ICredentials Credentials
		{
			get { return webProxy.Credentials; }
			set	{ webProxy.Credentials = value; }
		}

		/// <summary>
		/// Indicates that the proxy should not be used for the specified host.
		/// </summary>
		/// <param name="host">The Uri of the host to check for proxy use.</param>
		/// <returns>true if the proxy server should not be used for host; otherwise, false.</returns>
		public bool IsBypassed( Uri host )
		{
			return webProxy.IsBypassed( host );
		}

		#endregion
	}

	/// <summary>
	/// Class to share and keep the client state for web communications.
	/// </summary>
	public class WebState
	{
		static string userAgent = "Simias Client " 
			+ System.Reflection.Assembly.GetCallingAssembly().ImageRuntimeVersion 
			+ " OS=" 
			+ System.Environment.OSVersion.ToString();
			
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
			
		NetworkCredential			credentials;
		static Hashtable			cookieHash = new Hashtable();

		/// <summary>
		/// Get a WebState object for the specified domain and collection.
		/// </summary>
		/// <param name="DomainID">The domain ID.</param>
		/// <param name="CollectionID">The collection associated with the domain</param>
		public WebState(string DomainID, string CollectionID) :
			this(DomainID, CollectionID, null)
		{
		}

		/// <summary>
		/// Get a WebState object for the specified domain.
		/// </summary>
		/// <param name="DomainID">The domain ID.</param>
		/// <param name="CollectionID">The collection ID.</param>
		/// <param name="UserID">User ID of a member in the domain.</param>
		public WebState(string DomainID, string CollectionID, string UserID) :
			this( DomainID, CollectionID, UserID, SimiasConnection.AuthType.BASIC )
		{
		}
		

		/// <summary>
		/// Get a WebState object for the specified domain.
		/// </summary>
		/// <param name="DomainID">The domain ID.</param>
		/// <param name="CollectionID">The collection ID.</param>
		/// <param name="UserID">User ID of a member in the domain.</param>
		/// <param name="authType">The type of authentication to use.</param>
		public WebState(string DomainID, string CollectionID, string UserID, SimiasConnection.AuthType authType) :
			this( DomainID )
		{
			BasicCredentials creds;
			
			// Get the credentials for this collection.
			Member member = null;
			try
			{
				if (UserID == null)
				{
					member = Store.GetStore().GetDomain( DomainID ).GetCurrentMember();
				}
				else
				{
					member = Store.GetStore().GetDomain( DomainID ).GetMemberByID( UserID );
				}

				UTF8Encoding utf8Name = new UTF8Encoding();
                                byte[] encodedCredsByteArray = utf8Name.GetBytes(member.Name);
                                string iFolderUserBase64 = Convert.ToBase64String(encodedCredsByteArray);

				creds = new BasicCredentials( DomainID,	CollectionID,iFolderUserBase64);
				if ( creds.Cached == true )
				{
					credentials = creds.GetNetworkCredential(); 
				}
				else
				{
					// Get the credentials for this collection.
					creds =	new BasicCredentials( DomainID,	DomainID, iFolderUserBase64 );
					if ( creds.Cached == true )
					{
						credentials = creds.GetNetworkCredential(); 
					}
				}
			}
			catch{}
		
			if (credentials == null && authType == SimiasConnection.AuthType.BASIC)
			{
				log.Debug( "failed to get NetworkCredential for {0}", member != null ? member.Name : "" );
				new EventPublisher().RaiseEvent( new NeedCredentialsEventArgs( DomainID, CollectionID ) );
				throw new NeedCredentialsException();
			}
		}

		/// <summary>
		/// Get a WebState with the specified credential.
		/// </summary>
		/// <param name="DomainID">The identifier for the domain.</param>
		public WebState( string DomainID )
		{
			lock( cookieHash )
			{
				if (!cookieHash.ContainsKey(DomainID))
				{
					cookieHash[ DomainID ] = new CookieContainer();
				}
			}
		}
		
		/// <summary>
		/// Initialize the HttpWebRequest.
		/// </summary>
		/// <param name="request">The request to initialize.</param>
		/// <param name="domainID">The identifier for the domain.</param>
		public void InitializeWebRequest(HttpWebRequest request, string domainID)
		{
			//request.KeepAlive = false;
			request.UserAgent = userAgent;
			request.Credentials = credentials;
			request.CookieContainer = cookieHash[ domainID ] as CookieContainer;
			request.Proxy = ProxyState.GetProxyState( request.RequestUri );
			request.PreAuthenticate = true;
		}

		/// <summary>
		/// Initialize the web service proxy stub.
		/// </summary>
		/// <param name="request">The client proxy to initialize</param>
		/// <param name="domainID">The identifier for the domain.</param>
		public void InitializeWebClient(HttpWebClientProtocol request, string domainID)
		{
			request.UserAgent = userAgent;
			request.Credentials = credentials;
			request.CookieContainer = cookieHash[ domainID ] as CookieContainer;
			request.Proxy = ProxyState.GetProxyState( new Uri( request.Url ) );
			request.PreAuthenticate = true;
		}

		/// <summary>
		/// Resets the WebState object.
		/// </summary>
		/// <param name="domainID">The identifier for the domain.</param>
		static public void ResetWebState( string domainID )
		{
			lock( cookieHash )
			{
				cookieHash[ domainID ] = new CookieContainer();
			}
		}
	}

	internal class HostConnection
	{
		WebState	connectionState;
                private static readonly ISimiasLog log =
                        SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		string		domainID;
		string		userID;
		string		collectionID;
		string		hostID;
		SimiasConnection.AuthType	authType;
		bool		initialized = false;
        bool        sslInitialized = false;
		bool		authenticated = false;
		string		key;
		string		baseUri;
		bool		ssl = false;

		public string HostUri
		{
			get { return baseUri; }
		}

		public bool HostSSL
		{
			get { 
				if(baseUri.StartsWith(Uri.UriSchemeHttps)) 
					return true;
				else
					return false;
			}
		}

		static Hashtable	connectionTable = new Hashtable();

		#region Constructors
        /// <summary>
        /// Construction to create new Host connection
        /// </summary>
        /// <param name="domainID">Id of the Domain</param>
        /// <param name="userID">Id of the User</param>
        /// <param name="authType">Authentication type</param>
        /// <param name="collection">Collection Name</param>
		private HostConnection( string domainID, string userID, SimiasConnection.AuthType authType, Collection collection )
		{
			this.domainID = domainID;
			this.userID = userID;
			this.collectionID = collection.ID;
			this.hostID = collection.HostID;
			//need to add the collection.SSL property here to Host, so that collection based SSL sync can be done
			this.ssl = collection.SSL; //there is collection.UseSSL -> need to see who uses it
			this.authType = authType;
//			baseUri = DomainProvider.ResolveLocation( collection ).ToString();
			System.UriBuilder uri = new UriBuilder( DomainProvider.ResolveLocation( collection ).ToString() );
			
			if (collection.SSL)
			{
				uri.Scheme = Uri.UriSchemeHttps;
			}
			
			baseUri = uri.ToString().TrimEnd('/') + '/';

            //baseUri = baseUri.TrimEnd('/') + '/';
		}

        /// <summary>
        /// Constructor to create new host connection
        /// </summary>
        /// <param name="domainID">ID of the domain</param>
        /// <param name="userID">User Id</param>
        /// <param name="authType">Authentication type</param>
        /// <param name="member">Member name</param>
		private HostConnection( string domainID, string userID, SimiasConnection.AuthType authType, Member member )
		{
			this.domainID = domainID;
			this.userID = userID;
			this.collectionID = domainID;
			this.hostID = member.HomeServer.UserID;
			this.authType = authType;
			baseUri = DomainProvider.ResolvePOBoxLocation( domainID, member.UserID ).ToString();
		}

        /// <summary>
        /// Constructor to create new host connection
        /// </summary>
        /// <param name="domainID">ID of the domain</param>
        /// <param name="userID">User Id</param>
        /// <param name="authType">Authentication type</param>
        /// <param name="host">Host name</param>
		private HostConnection( string domainID, string userID, SimiasConnection.AuthType authType, HostNode host )
		{
			this.domainID = domainID;
			this.userID = userID;
			this.collectionID = domainID;
			this.hostID = host.UserID;
			this.authType = authType;
			baseUri = DomainProvider.ResolveHostAddress( domainID, host.UserID ).ToString();
			log.Debug("dID {0}, hUID {1}, UID {2}", domainID, host.UserID, userID);
		}
		#endregion

		#region Private Methods

        /// <summary>
        /// Initializes the connection with new WebState and ports required
        /// </summary>
		private void InitializeConnection()
		{
			if ( initialized == false )
			{
				try
				{
					connectionState = new WebState( domainID, collectionID, userID, authType );
					initialized = true;
				}
				catch (NeedCredentialsException)
				{
					authenticated = false;
				}
//				finally
//				{
  //                  ssl = this.HostSSL;
	//			}
			}
            if (sslInitialized == false && ssl == true)
            {
                System.UriBuilder uri = new UriBuilder(baseUri);

                if (uri.Scheme == Uri.UriSchemeHttp)
                {
                    uri.Scheme = Uri.UriSchemeHttps;
                    if (uri.Port == 80)
                        uri.Port = 443;
                }
                baseUri = uri.Uri.ToString().TrimEnd('/') + '/';
                sslInitialized = true;
            }

		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Get a connection that can be used to access the host of specified collection.
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="authType">The type of authentication.</param>
		/// <param name="collection">The collection to be accessed</param>
		/// <returns>Connection</returns>
		internal static HostConnection GetConnection( string domainID, string userID, SimiasConnection.AuthType authType, Collection collection )
		{
			HostConnection conn;
			string key = collection.HostID;
			if (key != null)
			{
				lock (connectionTable)
				{
					conn = (HostConnection)connectionTable[key];
					if (conn == null)
					{
						conn = new HostConnection( domainID, userID, authType, collection );
						conn.key = key;
						connectionTable[key] = conn;
					}
				}
			}
			else
			{
				conn = new HostConnection( domainID, userID, authType, collection );
			}
            conn.ssl = collection.SSL;
			conn.InitializeConnection();
			return conn;
		}

		/// <summary>
		/// Get a connection to the home server of the specified member
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="authType">The authentication type.</param>
		/// <param name="member">The member</param>
		/// <returns>a connection.</returns>
		internal static HostConnection GetConnection(string domainID, string userID, SimiasConnection.AuthType authType, Member member )
		{
			HostConnection conn;
			string key = member.HomeServer.UserID;
			if (key != null)
			{
				lock (connectionTable)
				{
					conn = (HostConnection)connectionTable[key];
					if (conn == null)
					{
						conn = new HostConnection( domainID, userID, authType, member );
						conn.key = key;
						connectionTable[key] = conn;
					}
				}
			}
			else
			{
				conn = new HostConnection( domainID, userID, authType, member );
			}
			conn.InitializeConnection();
			return conn;
		}

		/// <summary>
		/// Get a connection to the home server of the specified member
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="authType">The authentication type.</param>
		/// <param name="host">Host information against which authentication has to be done.</param>
		/// <returns>a connection.</returns>
		internal static HostConnection GetConnection( string domainID, string userID, SimiasConnection.AuthType authType, HostNode host )
		{
			HostConnection conn;
			string key = host.UserID;
			log.Debug("2-dID {0}, hUID {1}, UID {2}", domainID, host.UserID, userID);
			if (key != null)
			{
				lock (connectionTable)
				{
					conn = (HostConnection)connectionTable[key];
					if (conn == null)
					{
						conn = new HostConnection( domainID, userID, authType, host );
						conn.key = key;
						connectionTable[key] = conn;
					}
				}
			}
			else
			{
				conn = new HostConnection( domainID, userID, authType, host );
			}
			conn.InitializeConnection();
			return conn;
		}
		
        /// <summary>
        /// Clears the connection and resets the webstate
        /// </summary>
		internal void ClearConnection()
		{
			if (key != null)
			{
				lock (connectionTable)
				{
					connectionTable.Remove(key);
				}
			}
			// bug 538046: lot of 401 exceptions during UserMove and hence data loss. After every domain sync/Catalog sync or during web-service
			// ex., server used to remove the cookiehash entry for the domainID, so the connections which were already established earlier were
			// getting 401 status. The removal of cookiehash entry is applicable only for clients as they logout/login from one domain. For 
			// servers, it is only one domain, so no login/logout concept for servers.
			if( ! Store.IsEnterpriseServer)
				WebState.ResetWebState(domainID);
		}

        /// <summary>
        /// Clears the key from connection table
        /// </summary>
        /// <param name="key"></param>
        internal static void ClearConnection(string key)
        {
            if (key != null && key.Length > 0)
            {
                lock (connectionTable)
                {
                    connectionTable.Remove(key);
                }
            }
        }

		/// <summary>
		/// Initializes the connections web request
		/// </summary>
		/// <param name="uri">URI of the connection</param>
		/// <returns>HttpWebRequest of the connection</returns>
		internal HttpWebRequest GetRequest(string uri)
		{
			log.Debug("HC GetReq Uri {0}", uri);
                        // certificate policy
                        ServicePointManager.CertificatePolicy = new Simias.Client.CertPolicy(); 
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( uri );
			connectionState.InitializeWebRequest( request, domainID );
			return request;
		}

        /// <summary>
        /// Sends the request and gets the response
        /// </summary>
        /// <param name="webRequest">Connections web request</param>
        /// <returns>HttpWebResponse for the request sent</returns>
		internal HttpWebResponse GetResponse( HttpWebRequest webRequest )
		{
			try
			{
				return (HttpWebResponse)webRequest.GetResponse();
			}
			catch( WebException webEx )
			{
				// If we got an Unauthorized we need to generate a NeedCredentialsEvent.
				HttpWebResponse resp = webEx.Response as HttpWebResponse;
				if ( resp != null )
				{
					if ( resp.StatusCode == HttpStatusCode.Unauthorized )
					{
						initialized = false;
						authenticated = false;
						if ( authType == SimiasConnection.AuthType.PPK )
						{
							Authenticate();
						}
						else
						{
							new EventPublisher().RaiseEvent( new NeedCredentialsEventArgs( domainID, collectionID ) );
						}
					}
				}
				throw webEx;
			}
		}

        /// <summary>
        /// Handles the soap exception
        /// </summary>
        /// <param name="ex">Soap exception raised</param>
        /// <returns>Just returns false when soap exception is raised</returns>
		internal bool HandleSoapException(SoapException ex)
		{
			return false;
		}

        /// <summary>
        /// Initializes the web client
        /// </summary>
        /// <param name="request">Inital request</param>
		internal void InitializeWebClient( HttpWebClientProtocol request )
		{
			connectionState.InitializeWebClient( request, domainID );
		}

        /// <summary>
        /// Authenticates the connection
        /// </summary>
        /// <returns>True if success and false if failure</returns>
		internal bool Authenticate()
		{
			if ( authenticated == false )
			{
				bool bstatus = false;
				if ( authType == SimiasConnection.AuthType.PPK )
				{
					bstatus = Simias.Authentication.Http.AuthenticateWithPPK( domainID, userID, baseUri );
				}
				else
				{
					bstatus = true;
				}

				if ( bstatus == true )
				{
					authenticated = true;
				}
				return bstatus;
			}
			return true;
		}
		#endregion
	}

	/// <summary>
	///
	/// </summary>
	public class SimiasConnection
	{
		/// <summary>
		///
		/// </summary>
		public enum AuthType
		{
			/// <summary>
			///
			/// </summary>
			BASIC,
			/// <summary>
			///
			/// </summary>
			PPK,
		}

		#region properties

		HostConnection connection;
		string		baseUri;
                private static readonly ISimiasLog log =
                        SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		
		#endregion
		
		#region Constructors
		/// <summary>
		/// Get a connection that can be used to access the specified collection.
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="authType">The type of authentication.</param>
		/// <param name="collection">The collection to be accessed</param>
		/// <returns>Connection</returns>
		public SimiasConnection( string domainID, string userID, AuthType authType, Collection collection )
		{
			connection = HostConnection.GetConnection( domainID, userID, authType, collection );
			InitializeConnection();
		}

		/// <summary>
		/// Get a connection that can be used to access the specified collection.
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="collection">The collection to be accessed</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <returns>Connection</returns>
		public SimiasConnection( string domainID, string userID, Collection collection ) :
			this ( domainID, userID, AuthType.BASIC, collection )
		{
		}

		/// <summary>
		/// Get a connection that can be used to access the specified host.
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="authType">The authentication type.</param>
		/// <param name="host">The Host to connect to</param>
		/// <returns>Connection</returns>
		public SimiasConnection( string domainID, string userID, AuthType authType, HostNode host )
		{
			connection = HostConnection.GetConnection( domainID, userID, authType, host );
			InitializeConnection();
		}

		/// <summary>
		/// Get a connection that can be used to access the specified host.
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="host">The Host to connect to</param>
		/// <returns>Connection</returns>
		public SimiasConnection( string domainID, string userID, HostNode host ) :
			this( domainID, userID, AuthType.BASIC, host )
		{
		}

		/// <summary>
		/// Get a connection to the home server of the specified member
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="authType">The authentication type.</param>
		/// <param name="member">The member</param>
		/// <returns>a connection.</returns>
		public SimiasConnection( string domainID, string userID, AuthType authType, Member member )
		{
			connection = HostConnection.GetConnection(domainID, userID, authType, member );
			InitializeConnection();
		}
		
		/// <summary>
		/// Get a connection to the home server of the specified member
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="member">The member</param>
		/// <returns>a connection.</returns>
		public SimiasConnection( string domainID, string userID, Member member ) :
			this( domainID, userID, AuthType.BASIC, member )
		{
		}
		
		#endregion

		#region Private Methods
        /// <summary>
        /// Initialises the connection
        /// </summary>
		private void InitializeConnection()
		{
			System.UriBuilder uri = new UriBuilder( connection.HostUri );
			
			if (connection.HostSSL)
			{
				uri.Scheme = Uri.UriSchemeHttps;
			}
			else
			{
				uri.Scheme = Uri.UriSchemeHttp;
			}
			
			baseUri = uri.ToString().TrimEnd('/') + '/';
		}
		#endregion

		#region Public Methods
        /// <summary>
        /// Clears the connection
        /// </summary>
		public void ClearConnection()
		{
			connection.ClearConnection();
		}
		
        /// <summary>
        /// Gets the request for connection
        /// </summary>
        /// <param name="servicePath">Path of the request</param>
        /// <returns>Returns the connections request</returns>
		public HttpWebRequest GetRequest( string servicePath )
		{
			string uri = baseUri + servicePath.TrimStart('/');
			log.Debug("SC GetReq Uri {0}", uri);
			return connection.GetRequest( uri );
		}

        /// <summary>
        /// Gets the response for the sent request
        /// </summary>
        /// <param name="webRequest">Request to connection</param>
        /// <returns>Response for the request</returns>
		public HttpWebResponse GetResponse(HttpWebRequest webRequest)
		{
			return connection.GetResponse(webRequest);
		}

        /// <summary>
        /// Initializes web client
        /// </summary>
        /// <param name="request">Request for the connection</param>
        /// <param name="servicePath">Path of the service request</param>
		public void InitializeWebClient(HttpWebClientProtocol request, string servicePath)
		{
			request.Url = baseUri + servicePath.TrimStart('/');
			connection.InitializeWebClient(request);
		}

        /// <summary>
        /// Authenticates the connection
        /// </summary>
        /// <returns></returns>
		public bool Authenticate()
		{
			return connection.Authenticate();
		}
	}
	#endregion

#if NOT_USED 
        /// <summary>
        /// Single Certificate Policy
        /// </summary>
        internal class SyncCertificatePolicy : ICertificatePolicy
        {

                /// <summary>
                /// Check Validation Result
                /// </summary>
                /// <param name="srvPoint"></param>
                /// <param name="certificate"></param>
                /// <param name="request"></param>
                /// <param name="certificateProblem"></param>
                /// <returns></returns>
                public bool CheckValidationResult( ServicePoint srvPoint,
                        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        WebRequest request,
                        int certificateProblem )
                {
			// Accept all, since there is no way to validate other than the user
                        return true;
                }
        }
#endif
}
