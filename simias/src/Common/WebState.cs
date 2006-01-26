/***********************************************************************
 *  $RCSfile: WebState.cs,v $
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
 *  Author: Russ Young
 *
 ***********************************************************************/

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
			this( DomainID, CollectionID, UserID, CollectionConnection.AuthType.BASIC )
		{
		}
		

		/// <summary>
		/// Get a WebState object for the specified domain.
		/// </summary>
		/// <param name="DomainID">The domain ID.</param>
		/// <param name="CollectionID">The collection ID.</param>
		/// <param name="UserID">User ID of a member in the domain.</param>
		/// <param name="authType">The type of authentication to use.</param>
		public WebState(string DomainID, string CollectionID, string UserID, CollectionConnection.AuthType authType) :
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
		
			if (credentials == null && authType == CollectionConnection.AuthType.BASIC)
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

	public class CollectionConnection
	{
		public enum AuthType
		{
			BASIC,
			PPK,
		}

		WebState	connectionState;
		string		domainID;
		string		collectionID;
		string		userID;
		AuthType	authType;
		bool		needCredentials = false;
		bool		authenticated = false;
		string		baseUri;
		static Hashtable	connectionTable = new Hashtable();

		#region Constructor

		private CollectionConnection(string domainID, string collectionID, string userID, AuthType authType)
		{
			this.domainID = domainID;
			this.collectionID = collectionID;
			this.userID = userID;
			this.authType = authType;
			Collection collection = Store.GetStore().GetCollectionByID(collectionID);
			baseUri = DomainProvider.ResolveLocation(collection).ToString();
			System.UriBuilder uri = new UriBuilder(baseUri);
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
			try
			{
				connectionState = new WebState(domainID, collectionID, userID, authType);
			}
			catch (NeedCredentialsException)
			{
				needCredentials = true;
				authenticated = false;
			}
		}

		#endregion

		#region Private Methods

		private static string GetKey(string collectionID)
		{
			return collectionID;
		}

		#endregion

		#region Public Methods
		
		public static CollectionConnection GetConnection(string domainID, string collectionID, string userID, AuthType authType)
		{
			string key = GetKey(collectionID);
			CollectionConnection conn;
			lock (connectionTable)
			{
				conn = (CollectionConnection)connectionTable[key];
				if (conn == null)
				{
					conn = new CollectionConnection(domainID, collectionID, userID, authType);
					connectionTable[key] = conn;
				}
			}
			return conn;
		}

		public static CollectionConnection GetConnection(string domainID, string collectionID, string userID)
		{
			return GetConnection(domainID, collectionID, userID, AuthType.BASIC);
		}

		public void ClearConnection()
		{
			string key = GetKey(domainID);
			lock (connectionTable)
			{
				connectionTable.Remove(key);
				WebState.ResetWebState(domainID);
			}
		}

		
		public HttpWebRequest GetRequest(string servicePath)
		{
			string uri = baseUri + servicePath.TrimStart('/');
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			connectionState.InitializeWebRequest(request, domainID);
			return request;
		}

		public HttpWebResponse GetResponse(HttpWebRequest webRequest)
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
					if ( resp.StatusCode == HttpStatusCode.Unauthorized)
					{
						needCredentials = true;
						authenticated = false;
						if (authType == AuthType.PPK)
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

		public void InitializeWebClient(HttpWebClientProtocol request, string servicePath)
		{
			request.Url = baseUri + servicePath.TrimStart('/');
			connectionState.InitializeWebClient(request, domainID);
		}

		public bool Authenticate()
		{
			bool bstatus = false;
			if (authType == AuthType.PPK)
			{
				bstatus = Simias.Authentication.Http.AuthenticateWithPPK(domainID, userID);
			}
			else
			{
			}
			if (bstatus)
			{
				needCredentials = false;
				authenticated = true;
			}
			return bstatus;
		}

		
		#endregion
	}
}
