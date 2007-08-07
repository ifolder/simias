/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |   Author: Russ Young
 |***************************************************************************/

using System;
using System.Collections;
using System.Web;
using System.Net;
using System.Web.Services.Protocols;

using Simias.Storage;
using Simias.Authentication;
using Simias.Event;
using Simias.Client.Event;

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
		/// <param name="domainID">The domain ID.</param>
		/// <param name="collectionID">The collection associated with the domain</param>
		public WebState(string DomainID, string CollectionID) :
			this(DomainID, CollectionID, null)
		{
		}

		/// <summary>
		/// Get a WebState object for the specified domain.
		/// </summary>
		/// <param name="domainID">The domain ID.</param>
		/// <param name="collectionID">The collection ID.</param>
		/// <param name="userID">User ID of a member in the domain.</param>
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
				creds = new BasicCredentials( DomainID,	CollectionID, member.Name );
				if ( creds.Cached == true )
				{
					credentials = creds.GetNetworkCredential(); 
				}
				else
				{
					// Get the credentials for this collection.
					creds =	new BasicCredentials( DomainID,	DomainID, member.Name );
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
		/// <param name="domainID">The identifier for the domain.</param>
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
		string		domainID;
		string		userID;
		string		collectionID;
		string		hostID;
		SimiasConnection.AuthType	authType;
		bool		initialized = false;
		bool		authenticated = false;
		string		key;
		string		baseUri;

		public string HostUri
		{
			get { return baseUri; }
		}

		static Hashtable	connectionTable = new Hashtable();

		#region Constructors
		private HostConnection( string domainID, string userID, SimiasConnection.AuthType authType, Collection collection )
		{
			this.domainID = domainID;
			this.userID = userID;
			this.collectionID = collection.ID;
			this.hostID = collection.HostID;
			this.authType = authType;
			baseUri = DomainProvider.ResolveLocation( collection ).ToString();
		}

		private HostConnection( string domainID, string userID, SimiasConnection.AuthType authType, Member member )
		{
			this.domainID = domainID;
			this.userID = userID;
			this.collectionID = domainID;
			this.hostID = member.HomeServer.UserID;
			this.authType = authType;
			baseUri = DomainProvider.ResolvePOBoxLocation( domainID, member.UserID ).ToString();
		}

		private HostConnection( string domainID, string userID, SimiasConnection.AuthType authType, HostNode host )
		{
			this.domainID = domainID;
			this.userID = userID;
			this.collectionID = domainID;
			this.hostID = host.UserID;
			this.authType = authType;
			baseUri = DomainProvider.ResolveHostAddress( domainID, host.UserID ).ToString();
		}
		#endregion

		#region Private Methods
		private void IntitalizeConnection()
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
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Get a connection that can be used to access the host of specified collection.
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="collection">The collection to be accessed</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="authType">The type of authentication.</param>
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
			conn.IntitalizeConnection();
			return conn;
		}

		/// <summary>
		/// Get a connection to the home server of the specified member
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="member">The member</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="authType">The authentication type.</param>
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
			conn.IntitalizeConnection();
			return conn;
		}

		/// <summary>
		/// Get a connection to the home server of the specified member
		/// </summary>
		/// <param name="domainID">The domain ID</param>
		/// <param name="member">The member</param>
		/// <param name="userID">The user to authenticate as.</param>
		/// <param name="authType">The authentication type.</param>
		/// <returns>a connection.</returns>
		internal static HostConnection GetConnection( string domainID, string userID, SimiasConnection.AuthType authType, HostNode host )
		{
			HostConnection conn;
			string key = host.UserID;
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
			conn.IntitalizeConnection();
			return conn;
		}
		
		internal void ClearConnection()
		{
			if (key != null)
			{
				lock (connectionTable)
				{
					connectionTable.Remove(key);
				}
			}
			WebState.ResetWebState(domainID);
		}

		
		internal HttpWebRequest GetRequest(string uri)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( uri );
			connectionState.InitializeWebRequest( request, domainID );
			return request;
		}

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

		internal bool HandleSoapException(SoapException ex)
		{
			return false;
		}

		internal void InitializeWebClient( HttpWebClientProtocol request )
		{
			connectionState.InitializeWebClient( request, domainID );
		}

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

	public class SimiasConnection
	{
		public enum AuthType
		{
			BASIC,
			PPK,
		}

		#region properties

		HostConnection connection;
		string		baseUri;
		
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
			IntitalizeConnection();
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
			IntitalizeConnection();
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
			IntitalizeConnection();
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
		private void IntitalizeConnection()
		{
			System.UriBuilder uri = new UriBuilder( connection.HostUri );
			// TODO
			/*
			if (collection.UseSSL)
			{
				uri.Scheme = Uri.UriSchemeHttps;
			}
			else
			{
				uri.Scheme = Uri.UriSchemeHttp;
			}
			*/
			baseUri = uri.Uri.ToString().TrimEnd('/') + '/';
		}
		#endregion

		#region Public Methods
		public void ClearConnection()
		{
			connection.ClearConnection();
		}
		
		public HttpWebRequest GetRequest( string servicePath )
		{
			string uri = baseUri + servicePath.TrimStart('/');
			return connection.GetRequest( uri );
		}

		public HttpWebResponse GetResponse(HttpWebRequest webRequest)
		{
			return connection.GetResponse(webRequest);
		}

		public void InitializeWebClient(HttpWebClientProtocol request, string servicePath)
		{
			request.Url = baseUri + servicePath.TrimStart('/');
			connection.InitializeWebClient(request);
		}

		public bool Authenticate()
		{
			return connection.Authenticate();
		}
	}
	#endregion
}
