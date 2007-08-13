/***********************************************************************
 |  $RCSfile: DomainAgent.cs,v $
 |
 | Copyright (c) [2007] Novell, Inc.
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
 |  Author: Rob
 |
 ***********************************************************************/

using System;
using System.Collections;
using System.Net;
using System.IO;
using System.Threading;
using System.Web;
using System.Xml;

using Simias;
using Simias.Authentication;
using Simias.Client;
using Simias.POBox;
using Simias.Policy;
using Simias.Security.Web.AuthenticationService;
using Simias.Storage;
using Simias.Sync;

//using Novell.Security.ClientPasswordManager;

// Alias
using PostOffice = Simias.POBox;
using SCodes = Simias.Authentication.StatusCodes;


namespace Simias.DomainServices
{
	/// <summary>
	/// Simias Domain Agent
	/// </summary>
	public class DomainAgent
	{
		#region Class Members
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(DomainAgent));
	
		/// <summary>
		/// Service type for this service.
		/// </summary>
		private static string DomainServiceType = "Domain Service";
		private static string DomainServicePath = "/simias10/DomainService.asmx";
		private static string DomainService = "/DomainService.asmx";

		private CollectionSyncClient syncClient;

		/// <summary>
		/// Property name for declaring a domain active/inactive.
		/// If the property doesn't exist on a Domain, then that
		/// domain by default is active
		/// </summary>
		private readonly string activePropertyName = "Active";

		private Store store = Store.GetStore();
		private static Hashtable domainTable = new Hashtable();
		private static string domainID;
		public static Hashtable blockedIPs = new Hashtable();
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		public DomainAgent()
		{
		}
		#endregion

		#region Private Methods
		private void CreateDomainProxy(Store store, string userID, DomainInfo info, Uri hostAddress, string hostID)
		{
			// Make sure the domain doesn't exist.
			if (store.GetCollectionByID(info.ID) == null)
			{
				log.Debug("Creating Domain Proxy: {0}", info.Name);

				// Create a new domain
				Domain domain = new Domain(store, info.Name, info.ID, info.Description, SyncRoles.Slave, Domain.ConfigurationType.ClientServer);
				domain.Proxy = true;
				domain.SetType( domain, "Enterprise" );
			
				// Mark the domain inactive until we get the POBox created
				Property p = new Property( activePropertyName, false );
				p.LocalProperty = true;
				domain.Properties.AddNodeProperty( p );

				p = new Property( PropertyTags.HostAddress, hostAddress );
				p.LocalProperty = true;
				domain.Properties.AddNodeProperty( p );

				domain.HostID = hostID;

				// Create domain member.
				Access.Rights rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), info.MemberRights );
				Member member = new Member( info.MemberNodeName, info.MemberNodeID, userID, rights, null );
				member.Proxy = true;
				member.IsOwner = true;

				// commit
				domain.Commit( new Node[] { domain, member } );
			}
		}

		private void CreatePOBoxProxy(Store store, string domainID, ProvisionInfo info, string hostID)
		{
			if (store.GetCollectionByID(info.POBoxID) == null)
			{
				log.Debug( "Creating PO Box Proxy: {0}", info.POBoxName );

				// Create a new POBox
				PostOffice.POBox poBox = new PostOffice.POBox(store, info.POBoxName, info.POBoxID, domainID);
				poBox.Priority = 0;
				poBox.Proxy = true;
			
				// Create member.
				Access.Rights rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), info.MemberRights );
				Member member = new Member( info.MemberNodeName, info.MemberNodeID, info.UserID, rights, null );
				if( member == null)
					log.Debug("Member is null");
				member.Proxy = true;
				member.IsOwner = true;

				poBox.HostID = hostID;
			
				// commit
				poBox.Commit( new Node[] { poBox, member } );
				log.Debug("Created PO Box Proxy");
			}
		}

		/// <summary>
		/// Login to a remote domain using username and password
		/// Assumes a slave domain has been provisioned locally
		/// </summary>
		/// <param name="host">The uri to the host.</param>
		/// <param name="domainID">ID of the remote domain.</param>
		/// <param name="networkCredential">The credentials to authenticate with.</param>
		/// <param name="calledRecursive">True if called recursively.</param>
		/// <returns>
		/// The status of the remote authentication
		/// </returns>
		private 
		Simias.Authentication.Status
		Login(Uri host, string domainID, NetworkCredential networkCredential, bool calledRecursive)
		{
			HttpWebResponse response = null;

//			DomainAgent.blockedIPs.Remove(HttpContext.Current.Request.UserHostAddress);
			Simias.Authentication.Status status =	
				new Simias.Authentication.Status( SCodes.Unknown );

			Uri loginUri = 
				new Uri( host, Simias.Security.Web.AuthenticationService.Login.Path );
			log.Debug("Uri path: {0}", loginUri);
			HttpWebRequest request = WebRequest.Create( loginUri ) as HttpWebRequest;
			WebState webState = new WebState(domainID);
			webState.InitializeWebRequest( request, domainID );
			request.Credentials = networkCredential;
			request.PreAuthenticate = true;
			
			if ( domainID != null && domainID != "")
			{
				request.Headers.Add( 
					Simias.Security.Web.AuthenticationService.Login.DomainIDHeader,
					domainID);
			}

			request.Headers.Add(
				Simias.Security.Web.AuthenticationService.Login.BasicEncodingHeader,
#if MONO
				// bht: Fix for Bug 73324 - Client fails to authenticate if LDAP
				// username has an international character in it.
				//
				// Mono converts the username and password to a byte array
				// without paying attention to the encoding.  In NLD, the
				// default encoding is UTF-8.  Without this fix, we ended up
				// sending the username and password in 1252 but the server
				// was attempting to decode it as UTF-8.  This fix forces the
				// username and password to be sent with Windows-1252 encoding
				// which properly gets decoded on the server.
				System.Text.Encoding.GetEncoding(1252).WebName );
#else
				System.Text.Encoding.Default.WebName );
#endif
			
			request.Method = "POST";
			request.ContentLength = 0;

			try
			{
				request.GetRequestStream().Close();
				response = request.GetResponse() as HttpWebResponse;
				if ( response != null )
				{
					request.CookieContainer.Add(response.Cookies);
					string grace = 
						response.GetResponseHeader( 
							Simias.Security.Web.AuthenticationService.Login.GraceTotalHeader );
					if ( grace != null && grace != "" )
					{
						status.statusCode = SCodes.SuccessInGrace;
						status.TotalGraceLogins = Convert.ToInt32( grace );

						grace = 
							response.GetResponseHeader( 
								Simias.Security.Web.AuthenticationService.Login.GraceRemainingHeader );
						if ( grace != null && grace != "" )
						{
							status.RemainingGraceLogins = Convert.ToInt32( grace );
						}
						else
						{
							// fail to worst case
							status.RemainingGraceLogins = 0;
						}
					}
					else
					{
						status.statusCode = SCodes.Success;
					}
				}
			}
			catch(WebException webEx)
			{
				// Changed the test for a mono bug
				//if (webEx.Status == WebExceptionStatus.TrustFailure)
				if(TestTrustFailure(host.Host, webEx))
				{
					// The Certificate is invalid.
					status.statusCode = SCodes.InvalidCertificate;
				}
				else
				{
					response = webEx.Response as HttpWebResponse;
					if (response != null)
					{
						request.CookieContainer.Add(response.Cookies);
						// Look for our special header to give us more
						// information why the authentication failed
						string iFolderError = 
							response.GetResponseHeader( 
							Simias.Security.Web.AuthenticationService.Login.SimiasErrorHeader );

						if ( iFolderError != null && iFolderError != "" )
						{
							if ( iFolderError == StatusCodes.InvalidPassword.ToString() )
							{
								status.statusCode = SCodes.InvalidPassword;
							}
							else if ( iFolderError == StatusCodes.AccountDisabled.ToString() )
							{
								status.statusCode = SCodes.AccountDisabled;
							}
							else if ( iFolderError == StatusCodes.AccountLockout.ToString() )
							{
								status.statusCode = SCodes.AccountLockout;
							}
							else if ( iFolderError == StatusCodes.AmbiguousUser.ToString() )
							{
								status.statusCode = SCodes.AmbiguousUser;
							}
							else if ( iFolderError == StatusCodes.UnknownDomain.ToString() )
							{
								status.statusCode = SCodes.UnknownDomain;
							}
							else if ( iFolderError == StatusCodes.InternalException.ToString() )
							{
								status.statusCode = SCodes.InternalException;
							}
							else if ( iFolderError == StatusCodes.UnknownUser.ToString() )
							{
								status.statusCode = SCodes.UnknownUser;
							}
							else if ( iFolderError == StatusCodes.MethodNotSupported.ToString() )
							{
								status.statusCode = SCodes.MethodNotSupported;
							}
							else if ( iFolderError == StatusCodes.InvalidCredentials.ToString() )
							{
								// This could have failed because of iChain.
								// Check for a via header.
								string viaHeader = response.Headers.Get("via");
								if (viaHeader != null && !calledRecursive)
								{
									// Try again.
									return Login(host, domainID, networkCredential, true);
								}
								status.statusCode = SCodes.InvalidCredentials;
							}
							else if ( iFolderError == StatusCodes.SimiasLoginDisabled.ToString() )
							{
								status.statusCode = SCodes.SimiasLoginDisabled;
							}
						}
						else if (response.StatusCode == HttpStatusCode.Unauthorized)
						{
							// This call is a free call on the server.
							// If we get a 401 we must have iChain between us.
							// The user was invalid.
							status.statusCode = SCodes.UnknownUser;
						}
					}
					else
					{
						log.Debug(webEx.Message);
						log.Debug(webEx.StackTrace);
					}
				}
			}
			catch(Exception ex)
			{
				log.Debug(ex.Message);
				log.Debug(ex.StackTrace);
			}

			return status;
		}

		#endregion

		#region Internal Methods
		/// <summary>
		/// Removes all traces of the domain from this machine.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to remove.</param>
		internal void RemoveDomainInformation( string domainID )
		{
			// Cannot remove the local domain.
			if ( domainID == store.LocalDomain )
			{
				throw new SimiasException("The local domain cannot be removed.");
			}

			// If the default domain is the one that is being deleted, set a new one.
			if (store.DefaultDomain == domainID)
			{
				// If there are no other domains present, there is no default.
				string defaultDomain = null;

				// Set the new default domain.
				ICSList dList = store.GetDomainList();
				foreach(ShallowNode sn in dList)
				{
					// Find the first domain that is not the one being deleted or is the
					// local domain.
					if ((sn.ID != domainID) && (sn.ID != store.LocalDomain))
					{
						defaultDomain = sn.ID;
						break;
					}
				}

				// Set the new default domain.
				store.DefaultDomain = defaultDomain;
			}

			// Get a list of all the collections that belong to this domain and delete them.
			ICSList cList = store.GetCollectionsByDomain(domainID);
			foreach(ShallowNode sn in cList)
			{
				Collection c = new Collection(store, sn);
				c.Commit(c.Delete());
			}

			// Remove the local domain information.
			store.DeleteDomainIdentity(domainID);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Attach to an enterprise system.
		/// </summary>
		/// <param name="host">Url of the enterprise server.</param>
		/// <param name="user">User to provision on the server.</param>
		/// <param name="password">Password to validate user.</param>
		/// <returns>
		/// The Domain ID of the newly attached Domain
		/// </returns>
		public Simias.Authentication.Status Attach(string host, string user, string password)
		{
			CookieContainer cookies = new CookieContainer();
			Store store = Store.GetStore();

			// Get a URL to our service.
			Uri domainServiceUrl = null;
			Simias.Authentication.Status status = null;
			try
			{
				domainServiceUrl = WSInspection.GetServiceUrl( host, DomainServiceType, user, password );
			}
			catch (WebException we)
			{
				// this is a fix for mono, it can't handle TrustFailures
				//if (we.Status == WebExceptionStatus.TrustFailure)
				if(TestTrustFailure(host, we))
				{
					status = new Simias.Authentication.Status();
					status.statusCode = Simias.Authentication.StatusCodes.InvalidCertificate;
					return status;
				}
				else if ((we.Status == WebExceptionStatus.Timeout) || 
						 (we.Status == WebExceptionStatus.NameResolutionFailure))
				{
					status = new Simias.Authentication.Status();
					status.statusCode = Simias.Authentication.StatusCodes.UnknownDomain;
					return status;
				}
			}
			if ( domainServiceUrl == null )
			{
				// There was a failure in obtaining the service url. Try a hard coded one.
				if ( host.StartsWith( Uri.UriSchemeHttp ) || host.StartsWith( Uri.UriSchemeHttps ) )
				{	
					domainServiceUrl = new Uri( host.TrimEnd( new char[] {'/'} ) + DomainServicePath ); 
				}
				else
				{
					domainServiceUrl = new Uri( Uri.UriSchemeHttp + Uri.SchemeDelimiter + host.TrimEnd( new char[] {'/'} ) + DomainServicePath );
				}
			}

			// Build a credential from the user name and password.
			NetworkCredential myCred = new NetworkCredential( user, password ); 

			// Create the domain service web client object.
			DomainService domainService = new DomainService();
			domainService.CookieContainer = cookies;
			domainService.Url = domainServiceUrl.ToString();
			domainService.Credentials = myCred;
			domainService.PreAuthenticate = true;
			domainService.Proxy = ProxyState.GetProxyState( domainServiceUrl );
			domainService.AllowAutoRedirect = true;

			// Check to see if this domain already exists in this store.
			string domainID = domainService.GetDomainID();
			if ( ( domainID != null ) && ( store.GetDomain( domainID ) != null ) )
			{
				throw new ExistsException( String.Format( "Domain {0}", domainID ) );
			}

			string baseUrl = domainServiceUrl.ToString();
			baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf('/'));

			status = 
				this.Login( 
					new Uri( baseUrl ),
					domainID,
					myCred,
					false);
			if ( ( status.statusCode != SCodes.Success ) && ( status.statusCode != SCodes.SuccessInGrace ) )
			{
				return status;
			}

			// Get the Home Server.
			string hostID = null;
			HostInfo hInfo = new HostInfo();
			try
			{
//Provisioning should be done only in master.

				    hInfo = domainService.GetHomeServer(user);
				    if (hInfo == null)
				    {
					    string masterServerURL = null;
					    HostInfo[] serverList = domainService.GetHosts ();
					    foreach (HostInfo server in serverList)
					    {
						if (server.Master)
						    masterServerURL = server.PublicAddress;
					    }
				    
					    //Now Talk to the master server and provision the user.
					    DomainService ds = new DomainService();
					    ds.CookieContainer = cookies;
					    ds.Url = (new Uri(masterServerURL.TrimEnd( new char[] {'/'} ) + DomainService)).ToString();
					    ds.Credentials = myCred;
					    ds.PreAuthenticate = true;
//					    ds.Proxy = ProxyState.GetProxyState( domainServiceUrl );
					    ds.AllowAutoRedirect = true;
					    hInfo = ds.GetHomeServer(user);
				}

				domainServiceUrl = new Uri(hInfo.PublicAddress.TrimEnd( new char[] {'/'} ) + DomainService);
				domainService.Url = domainServiceUrl.ToString();
				hostID = hInfo.ID;
			
				// Now login to the homeserver.
				status = 
					this.Login( 
					new Uri( hInfo.PublicAddress ),
					domainID,
					myCred,
					false);
				if ( ( status.statusCode != SCodes.Success ) && ( status.statusCode != SCodes.SuccessInGrace ) )
				{
					return status;
				}
			}
			catch
			{
				// We are talking to an older server. We don't support multi-server.
				log.Debug("Exception: While Logging in");
			}

			// Get just the path portion of the URL.
			string hostString = domainServiceUrl.ToString();
			int startIndex = hostString.LastIndexOf( "/" );
			Uri hostUri = new Uri( hostString.Remove( startIndex, hostString.Length - startIndex ) );

			// The web state object lets the connection use common state information.
			WebState webState = new WebState(domainID);
			webState.InitializeWebClient(domainService, domainID);

			// Save the credentials
			CredentialCache myCache = new CredentialCache();
			myCache.Add(new Uri(domainService.Url), "Basic", myCred);
			domainService.Credentials = myCache;
			domainService.Timeout = 30000;

			log.Debug("Calling " + domainService.Url + " to provision the user");

			// provision user
			ProvisionInfo provisionInfo = null;
			try
			{
				log.Debug("Calling provisioning on the server.....");
				provisionInfo = domainService.ProvisionUser(user, password);
				log.Debug("Prvisioned the user.....");
			}
			catch(Exception ex)
			{
				log.Debug("Exception while provisioning");
			}
			if (provisionInfo == null)
			{
				throw new ApplicationException("User does not exist on server.");
			}
				
			log.Debug("the user has been provisioned on the remote domain");

			// get domain info
			DomainInfo domainInfo = domainService.GetDomainInfo(provisionInfo.UserID);

			if( domainInfo != null)
			{
				// Create domain proxy
			//	log.Debug("Skipping creation on domain proxy");
				CreateDomainProxy(store, provisionInfo.UserID, domainInfo, hostUri, hostID);

				// Create PO Box proxy
				log.Debug("Skipping pobox creation");
				CreatePOBoxProxy(store, domainInfo.ID, provisionInfo, hostID);

				// create domain identity mapping.
				store.AddDomainIdentity(domainInfo.ID, provisionInfo.UserID);

				// authentication was successful - save the credentials
				BasicCredentials basic = 
					new BasicCredentials( 
						domainInfo.ID, 
						domainInfo.ID, 
						user, 
						password );
				basic.Save( false );
			}
			else
			{
				log.Debug("Unable to get domaininfo");
			}

			// Domain is ready to sync
			this.SetDomainActive( domainInfo.ID );
			status.DomainID = domainInfo.ID;
			SetDomainState( domainInfo.ID, true, true);

			//Down Sync the domain
			if(hInfo.Master == false)
			{
				log.Debug("Waiting for the home server to sync the master user");
				Thread.Sleep( 33 * 1000 );
			}
			
			log.Debug("Attach sync begin");
			syncClient = new CollectionSyncClient(domainInfo.ID, new TimerCallback( TimerFired ) );			
			syncClient.SyncNow();
			log.Debug("Attach sync done");
			if ( ( status.statusCode != SCodes.Success ) && ( status.statusCode != SCodes.SuccessInGrace ) )
			{
				log.Debug("Status is SUCCESS");
			}
			else
				log.Debug("Status is NOT success");
			return status;
		}

		///call back for sync
		public void TimerFired( object collectionClient )
		{

		}

		/// <summary>
		/// Check if the domain is marked Active or in a connected state
		/// </summary>
		/// <param name="DomainID">The identifier of the domain to check status on.</param>
		public bool IsDomainActive( string DomainID )
		{
			bool active = true;

			try
			{
				Domain cDomain = store.GetDomain( DomainID );
					
				// Make sure this domain is a slave 
				if ( cDomain.Role == SyncRoles.Slave )
				{
					Property p = 
						cDomain.Properties.GetSingleProperty( this.activePropertyName );

					if ( p != null && (bool) p.Value == false )
					{
						active = false;
					}
				}
			}
			catch{}
			return active;
		}

		/// <summary>
		/// Login to a remote domain using username and password
		/// Assumes a slave domain has been provisioned locally
		/// </summary>
		/// <param name="DomainID">ID of the remote domain.</param>
		/// <param name="Username">Member to login as</param>
		/// <param name="Password">Password to validate user.</param>
		/// <returns>
		/// The status of the remote authentication
		/// </returns>
		public 
		Simias.Authentication.Status
		Login( string DomainID, string Username, string Password )
		{
			log.Debug( "Login - called" );
			log.Debug( "  DomainID: " + DomainID );
			log.Debug( "  Username: " + Username );
			
			Simias.Authentication.Status status = null;
			Domain cDomain = store.GetDomain( DomainID );
			if ( cDomain != null )
			{
				if ( cDomain.Role == SyncRoles.Slave )
				{
					status = 
						this.Login( 
							DomainProvider.ResolveLocation( DomainID ), 
							DomainID,
							new NetworkCredential( Username, Password ),
							false );
							
					if ( status.statusCode == SCodes.Success ||
						status.statusCode == SCodes.SuccessInGrace )
					{
						BasicCredentials basic = 
							new BasicCredentials( 
									DomainID, 
									DomainID,
									Username,
									Password );
						basic.Save( false );
						SetDomainState( DomainID, true, true );
					}
				}
				else
				{
					status = new Simias.Authentication.Status( SCodes.UnknownDomain );
				}
			}
			else
			{
				status = new Simias.Authentication.Status( SCodes.UnknownDomain );
			}

			log.Debug( "Login - exit  Status: " + status.statusCode.ToString() );
			return status;
		}

		/// <summary>
		/// Logout from a domain.
		/// </summary>
		/// <param name="DomainID">The ID of the domain.</param>
		/// <returns>The status of the logout.</returns>
		public
		Simias.Authentication.Status
		Logout( string DomainID )
		{
			// Get the domain.
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain( DomainID );
			if( domain == null )
			{
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
			}

			// Set the state for this domain.
			SetDomainState( DomainID, false, false );

			// Clear the password from the cache.
			Member member = domain.GetMemberByID( store.GetUserIDFromDomainID( DomainID ) );
			if ( member != null )
			{
				// Clear the entry from the cache.
				BasicCredentials basic = new BasicCredentials( DomainID, DomainID, member.Name );
				basic.Remove();
			}
			
			// Clear the cookies for this Uri.
			WebState.ResetWebState( DomainID );

			return new Simias.Authentication.Status( SCodes.Success );
		}
		
		/// <summary>
		/// Attempts to "ping" the remote domain.
		/// </summary>
		/// <param name="DomainID">The identifier of the domain.</param>
		public bool Ping( string DomainID )
		{
			bool domainUp = false;
			string pongDomainID = null;
			
			log.Debug( "DomainAgent::Ping - called" );
			
			try
			{
				// Get the network location of the server where this collection is hosted.
				log.Debug( "  resolving location for domain: " + DomainID );
				Uri uri = DomainProvider.ResolveLocation( DomainID );
				Uri domainServiceUrl = new Uri( uri.ToString().TrimEnd( new char[] {'/'} ) + DomainService );
				log.Debug( "  domain: " + DomainID + " is located at: " + domainServiceUrl.ToString() );
				
				// Build a fake credential - not needed to get the domain id
				NetworkCredential myCred = 
					new NetworkCredential( 
							Store.GetStore().GetDomain( DomainID ).GetCurrentMember().Name, 
							Guid.NewGuid().ToString() );

				// Create the domain service web client object.
				DomainService domainService = new DomainService();
				domainService.CookieContainer = new CookieContainer();
				domainService.Url = domainServiceUrl.ToString();
				domainService.Credentials = myCred;
				domainService.PreAuthenticate = true;
				domainService.Proxy = ProxyState.GetProxyState( domainServiceUrl );

				log.Debug( "  calling web service - GetDomainID " );
				pongDomainID = domainService.GetDomainID();
			}
			catch ( WebException we )
			{
				log.Debug( we.Message );
				// This is a fix for mono
				//if ( we.Status == WebExceptionStatus.TrustFailure )
				Uri uri = DomainProvider.ResolveLocation( DomainID );
				Uri domainServiceUrl = new Uri( uri.ToString().TrimEnd( new char[] {'/'} ) + DomainService );
				if(TestTrustFailure(domainServiceUrl.Host, we))
				{
					domainUp = true;
				}
			}
			
			if ( pongDomainID != null )
			{
				domainUp = true;
			}

			log.Debug( "DomainAgent::Ping returning: " + domainUp.ToString() );
			return domainUp;		
		}
		

		/// <summary>
		/// Sets the status of the specified domain to Active.
		/// </summary>
		/// <param name="DomainID">The identifier of the domain.</param>
		public void SetDomainActive( string DomainID )
		{
			try
			{
				Domain cDomain = store.GetDomain( DomainID );
				if ( cDomain.Role == SyncRoles.Slave )
				{
					Property p = new Property( this.activePropertyName, true );
					p.LocalProperty = true;
					cDomain.Properties.ModifyNodeProperty( p );
					cDomain.Commit();
				}
			}
			catch( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace);
			}
		}

		/// <summary>
		/// Sets the status of the specified domain to Inactive.
		/// setting a domain to inactive will disable all 
		/// synchronization activity.
		/// </summary>
		/// <param name="DomainID">The identifier of the domain.</param>
		public void SetDomainInactive( string DomainID )
		{
			try
			{
				Domain cDomain = store.GetDomain( DomainID );
				if ( cDomain.Role == SyncRoles.Slave )
				{
					Property p = new Property( this.activePropertyName, false );
					p.LocalProperty = true;
					cDomain.Properties.ModifyNodeProperty( p );
					cDomain.Commit();
				}
			}
			catch( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace);
			}
		}

		/// <summary>
		/// Removes this workstation from the domain or removes the workstation from all machines
		/// owned by the user.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to remove.</param>
		/// <param name="localOnly">If true then only this workstation is removed from the domain.
		/// If false, then the domain will be deleted from every workstation that the user owns.</param>
		public void Unattach(string domainID, bool localOnly)
		{
			// Cannot remove the local domain.
			log.Debug("Ramesh: Unattaching the domain");
			if ( domainID == store.LocalDomain )
			{
				throw new SimiasException("The local domain cannot be removed.");
			}

			// Get the domain object.
			Simias.Storage.Domain domain = store.GetDomain(domainID);
			if (domain == null)
			{
				throw new SimiasException("The domain does not exist in the store.");
			}

			// Get who the user is in the specified domain.
			string userID = store.GetUserIDFromDomainID(domainID);
			Member member = domain.GetMemberByID( store.GetUserIDFromDomainID( domainID ) );
			
			// This information needs to be gathered before the local domain collections are deleted.
			// Set the address to the server.
			Uri uri = DomainProvider.ResolveLocation(domainID);
			if (uri == null)
			{
				throw new SimiasException(String.Format("Cannot get location for domain {0}.", domain.Name));
			}
			// Construct the web client.
			DomainService domainService = new DomainService();
			domainService.Url = uri.ToString() + "/DomainService.asmx";
			if (!localOnly)
			{
				WebState webState = new WebState(domainID, userID);
				webState.InitializeWebClient(domainService, domainID);
			}
			// Find the user's POBox for this domain.
			POBox.POBox poBox = POBox.POBox.FindPOBox(store, domainID, userID);
			System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(RemoveDomainThread));
			DomainAgent.domainID = domainID;
			thread.Start();
	 	//	this.RemoveDomainInformation(domainID);
			Collection domainColl = store.GetCollectionByID(domainID);
			//POBox will not be available for new accounts, so remove only if available
/*			if (poBox == null)
			{
				throw new SimiasException(String.Format("Cannot find POBox belonging to domain {0}", domainID));
			}*/

			try
			{
				if(poBox != null)
				{
					// Delete the POBox for this domain which will start the domain cleanup process.
					poBox.Commit(poBox.Delete());
					
					// Remove the domain from the table
					lock (domainTable)
					{
						domainTable.Remove(domainID);
					}
					
					if (!localOnly)
					{
						// Remove the user from the domain server.
						domainService.RemoveServerCollections(domainID, userID);
					}
				}
				else
				{
					log.Debug("Ramesh: pobox is null");
				}
			}
			finally
			{
				// Clear the password from the cache.
				if (member != null)
				{
					BasicCredentials basic = 
						new BasicCredentials( 
								domainID, 
								domainID, 
								member.UserID); 
					basic.Remove();
				}
				
				// Clear the cookies for this Uri.
				WebState.ResetWebState(domainID);
			}
		}

		internal void RemoveDomainThread()
		{
			log.Debug("Called remove domain thread");
			this.RemoveDomainInformation(DomainAgent.domainID);
			DomainAgent.domainID = null;
			return;
		}

		/// <summary>
		/// Create the master on the server.
		/// </summary>
		/// <param name="collection">Collection to create on the enterprise server.</param>
		public void CreateMaster(Collection collection)
		{
			// Set the host to the home server.
			Domain domain = store.GetDomain(collection.Domain);
			collection.HostID = domain.HostID;
			collection.Commit();
			// Get the network location of the server where this collection is to be created.
			Uri uri = DomainProvider.ResolveLocation(collection);
			if (uri == null)
			{
				throw new SimiasException(String.Format("The network location could not be determined for collection {0}.", collection.ID));
			}

			// Construct the web client.
			DomainService domainService = new DomainService();
			domainService.Url = uri.ToString() + "/DomainService.asmx";
			WebState webState = new WebState(collection.Domain, store.GetUserIDFromDomainID(collection.Domain));
			webState.InitializeWebClient(domainService, collection.Domain);
			
			string rootID = null;
			string rootName = null;

			DirNode rootNode = collection.GetRootDirectory();
			if (rootNode != null)
			{
				rootID = rootNode.ID;
				rootName = rootNode.Name;
			}

			Member member = collection.Owner;

			domainService.CreateMaster(
				collection.ID, 
				collection.Name, 
				rootID, 
				rootName, 
				member.UserID, 
				member.Name, 
				member.ID, 
				member.Rights.ToString() );

			collection.CreateMaster = false;
			collection.Commit();
		}

		public void SetDomainState(string DomainID, bool Authenticated, bool AutoLogin)
		{
			lock (domainTable)
			{
				DomainState domainState = (DomainState)domainTable[DomainID];
				if (domainState == null)
				{
					domainState = new DomainState();
				}

				domainState.Authenticated = Authenticated;
				domainState.AutoLogin = AutoLogin;
				domainTable[DomainID] = domainState;
			}
		}

		public bool IsDomainAuthenticated(string DomainID)
		{
			DomainState domainState;

			lock (domainTable)
			{
				domainState = (DomainState)domainTable[DomainID];
			}

			if (domainState == null)
			{
				return false;
			}
			
			return domainState.Authenticated;
		}

		public bool IsDomainAutoLoginEnabled(string DomainID)
		{
			DomainState domainState;

			lock (domainTable)
			{
				domainState = (DomainState)domainTable[DomainID];
			}

			if (domainState == null)
			{
				return true;
			}

			return domainState.AutoLogin;
		}

		/// <summary>
		/// Gets the amount of disk space used on the server by the specified user.
		/// </summary>
		/// <param name="memberID">Member ID for the user.</param>
		/// <param name="limit">Gets the disk space limit for this user.</param>
		/// <returns>The amount of disk space used on the server. Zero indicates 
		/// that there is no disk space restriction.</returns>
		public long GetDomainDiskSpaceForMember( string memberID, out long limit )
		{
			// Get the domain for this user.
			Domain domain = store.GetDomainForUser( memberID );
			if ( domain == null )
			{
				throw new DoesNotExistException( String.Format( "Cannot get domain for user {0}", memberID ) );
			}

			try
			{
				// Get the network location of the server.
				Uri uri = DomainProvider.ResolveLocation( domain.ID );
				if (uri == null)
				{
					throw new SimiasException( String.Format( "The network location could not be determined for domain {0}.", domain.ID ) );
				}

				// Construct the web client.
				DomainService domainService = new DomainService();
				domainService.Url = uri.ToString() + "/DomainService.asmx";
				WebState webState = new WebState( domain.ID );
				webState.InitializeWebClient( domainService, domain.ID );

				return domainService.GetMemberDiskSpaceUsed( memberID, out limit );
			}
			catch
			{
				// Report the disk space quota from the local store. This may not be exact, but it is the 
				// last known good value.
				Member member = domain.GetMemberByID( memberID );
				if ( member == null )
				{
					throw new DoesNotExistException( String.Format( "Cannot find member {0} in domain {1}", memberID, domain.ID ) );
				}

				DiskSpaceQuota dsq = DiskSpaceQuota.Get( member );
				limit = dsq.Limit;
				return dsq.UsedSpace;
			}
		}

		/// <summary>
		/// Gets the amount of disk space used on the server by the specified collection.
		/// </summary>
		/// <param name="collectionID">Collection ID to get disk space for.</param>
		/// <param name="limit">Gets the disk space limit for this collection.</param>
		/// <returns>The amount of disk space used on the server by the specified collection.</returns>
		public long GetDomainDiskSpaceForCollection( string collectionID, out long limit )
		{
			// Get the collection from the specified ID.
			Collection collection = store.GetCollectionByID( collectionID );
			if ( collection == null )
			{
				throw new DoesNotExistException( "The specified collection does not exist." );
			}

			try
			{
				// Get the network location of the server where this collection is hosted.
				Uri uri = DomainProvider.ResolveLocation( collection );
				if (uri == null)
				{
					throw new SimiasException( String.Format( "The network location could not be determined for collection {0}.", collectionID ) );
				}

				// Construct the web client.
				DomainService domainService = new DomainService();
				domainService.Url = uri.ToString() + "/DomainService.asmx";
				WebState webState = new WebState( collection.Domain, collectionID );
				webState.InitializeWebClient( domainService, collection.Domain );

				// Get the quota from the local store this fixes bug 97331
				long usedSize = domainService.GetiFolderDiskSpaceUsed( collectionID, out limit );
				limit = DiskSpaceQuota.GetLimit( collection );
				return usedSize;
			}
			catch
			{
				// Report the collection limit from the local store. This may not be exact, but it is the 
				// last known good value.
				limit = DiskSpaceQuota.GetLimit( collection );
				return collection.StorageSize;
			}
		}

		static public bool TestTrustFailure(string host, WebException we)
		{
			if (we.Status == WebExceptionStatus.TrustFailure )
			{
				return true;
			}
			CertPolicy.CertificateState cs = CertPolicy.GetCertificate(host);
			if (cs != null && !cs.Accepted)
			{
				return true;
			}
			return false;
		}

		#endregion
	}

	internal class DomainState
	{
		#region Class Members
		private bool authenticated = false;
		private bool autoLogin = true;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a DomainState object.
		/// </summary>
		public DomainState()
		{
		}

		/// <summary>
		/// Constructs a DomainState object.
		/// </summary>
		/// <param name="Authenticated">A value indicating if the domain has been authenticated to.</param>
		/// <param name="AutoLogin">A value indicating if the client should attempt to automatically
		/// login to the domain.</param>
		public DomainState(bool Authenticated, bool AutoLogin)
		{
			authenticated = Authenticated;
			autoLogin = AutoLogin;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Get/sets a value indicating if the domain has been authenticated to.
		/// </summary>
		public bool Authenticated
		{
			get { return authenticated; }
			set { authenticated = value; }
		}

		/// <summary>
		/// Gets/sets a value indicating if the client should attempt to
		/// automatically login to the domain.
		/// </summary>
		public bool AutoLogin
		{
			get { return autoLogin; }
			set { autoLogin = value; }
		}
		#endregion
	}			  
}
