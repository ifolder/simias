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
*                 $Author: Rob 
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
using System.Net;
using System.IO;
using System.Text;
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
        
        	public static string currentDomainID = null;
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
        private void CreateDomainProxy(Store store, string userID, DomainInfo info, Uri hostAddress, HostInfo hInfo, string ownerNodeId)
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

				domain.HostID = hInfo.ID;

				// Create domain member.
				Access.Rights rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), info.MemberRights );
				Member member = new Member( info.MemberNodeName, info.MemberNodeID, userID, rights, null );
				Property hostidProperty = new Property(hInfo.ID, userID);
				member.Properties.ModifyNodeProperty( hostidProperty );
				member.Proxy = true;
				member.IsOwner = true;
                		HostNode hostNode = new HostNode(hInfo, hostAddress.ToString());
                		hostNode.Proxy = true;
				if( ownerNodeId != null && ownerNodeId != info.MemberNodeID)
				{
					Member ownermember = new Member("adminname", ownerNodeId, "adminid", Access.Rights.Admin, null);
					ownermember.Proxy = true;
					domain.Commit( new Node[] { domain, hostNode, member, ownermember } );
				}
				else
					domain.Commit( new Node[] { domain, hostNode, member} );

			}
        }

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
			HttpWebRequest request = null;
			WebState webState = null;

//			DomainAgent.blockedIPs.Remove(HttpContext.Current.Request.UserHostAddress);
			Simias.Authentication.Status status =	
				new Simias.Authentication.Status( SCodes.Unknown );
            
			Uri loginUri = 
				new Uri( host, Simias.Security.Web.AuthenticationService.Login.Path );
			log.Debug("Uri path: {0}", loginUri);
			request = WebRequest.Create( loginUri ) as HttpWebRequest;
			webState = new WebState(domainID);
			webState.InitializeWebRequest( request, domainID );
            int retrycount = 3;
            retry:
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
				System.Text.Encoding.UTF8.WebName );
			
			request.Method = "POST";
			request.ContentLength = 0;
			request.Timeout = 15 * 1000; 

			try
			{
				Thread.Sleep(3000);
				request.GetRequestStream().Close();
				response = request.GetResponse() as HttpWebResponse;
				if ( response != null )
				{
					request.CookieContainer.Add(response.Cookies);
					string grace = 
						response.GetResponseHeader( 
							Simias.Security.Web.AuthenticationService.Login.GraceTotalHeader );
                    string UserMovedStatus = response.GetResponseHeader(Simias.Security.Web.AuthenticationService.Login.UserMovedHeader);
                    if (UserMovedStatus != null && UserMovedStatus != "")
                    {
                        status.statusCode = SCodes.UserAlreadyMoved;
                    }
                    else
                    {
                        if (grace != null && grace != "")
                        {
                            status.statusCode = SCodes.SuccessInGrace;
                            status.TotalGraceLogins = Convert.ToInt32(grace);

                            grace =
                                response.GetResponseHeader(
                                    Simias.Security.Web.AuthenticationService.Login.GraceRemainingHeader);
                            if (grace != null && grace != "")
                            {
                                status.RemainingGraceLogins = Convert.ToInt32(grace);
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
		                else if (webEx.Status == WebExceptionStatus.KeepAliveFailure)
                		{
		                    retrycount--;
                		    if (retrycount >= 0)
                   		    {

		                        request = WebRequest.Create(loginUri) as HttpWebRequest;
		                        webState = new WebState(domainID);
		                        webState.InitializeWebRequest(request, domainID);
		                        //request.KeepAlive = false;
		                        goto retry;
                    		    }
		                    else
          		            {
		                        status.statusCode = SCodes.Timeout;
                		        throw webEx;
                    		    }
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
		                            Simias.Security.Web.AuthenticationService.Login.SimiasErrorHeader);

		         if (iFolderError != null && iFolderError != "")
		         {
		                            if (iFolderError == StatusCodes.InvalidPassword.ToString())
		                            {
		                                status.statusCode = SCodes.InvalidPassword;
		                            }
                            else if (iFolderError == StatusCodes.AccountDisabled.ToString())
                            {
                                status.statusCode = SCodes.AccountDisabled;
                            }
                            else if (iFolderError == StatusCodes.AccountLockout.ToString())
                            {
                                status.statusCode = SCodes.AccountLockout;
                            }
                            else if (iFolderError == StatusCodes.AmbiguousUser.ToString())
                            {
                                status.statusCode = SCodes.AmbiguousUser;
                            }
                            else if (iFolderError == StatusCodes.UnknownDomain.ToString())
                            {
                                status.statusCode = SCodes.UnknownDomain;
                            }
                            else if (iFolderError == StatusCodes.InternalException.ToString())
                            {
                                status.statusCode = SCodes.InternalException;
                            }
                            else if (iFolderError == StatusCodes.UnknownUser.ToString())
                            {
                                status.statusCode = SCodes.UnknownUser;
                            }
                            else if (iFolderError == StatusCodes.MethodNotSupported.ToString())
                            {
                                status.statusCode = SCodes.MethodNotSupported;
                            }
                            else if (iFolderError == StatusCodes.InvalidCredentials.ToString())
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
                            else if (iFolderError == StatusCodes.SimiasLoginDisabled.ToString())
                            {
                                status.statusCode = SCodes.SimiasLoginDisabled;
                            }
                            else if (iFolderError == StatusCodes.UserAlreadyMoved.ToString())
                            {
                                status.statusCode = SCodes.UserAlreadyMoved;
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
                        log.Debug("Exception1 msg {0}", webEx.Message);
                        log.Debug("Exception1 stack {0}", webEx.StackTrace);
                    }
                }
			}
			catch(Exception ex)
			{
				log.Debug("Exception2 msg {0}",ex.Message);
				log.Debug("Exception2 stack {0}", ex.StackTrace);
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
			string homeServerURL = null;
			Simias.Authentication.Status status = null;
			Simias.Authentication.Status status2 = null;
#if WS_INSPECTION			
			try
			{
				domainServiceUrl = WSInspection.GetServiceUrl(host, DomainServiceType, user, password);
			}
			catch (WebException we)
			{
				// this is a fix for mono, it can't handle TrustFailures
				//if (we.Status == WebExceptionStatus.TrustFailure)

				if (TestTrustFailure(host, we))
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
				else if (we.Status == WebExceptionStatus.ConnectionFailure)
				{
					status = new Simias.Authentication.Status();
					status.statusCode = Simias.Authentication.StatusCodes.Timeout;				   
					return status;
				}
			}
#endif   //WS_INSPECTION

			if ( domainServiceUrl == null )
			{
				UriBuilder tempUri = null;
				if( host.StartsWith( Uri.UriSchemeHttp ) == false)
				{
					tempUri = new UriBuilder(Uri.UriSchemeHttp, host);
				}
				else
		                	tempUri = new UriBuilder(host);
				log.Debug("The temp uri is: {0}", tempUri );
				/// There was a failure in obtaining the service url. Try a hard coded one.
                		if (tempUri.Scheme.Equals(Uri.UriSchemeHttp))
				{
		                    /// change to SSL
                		    tempUri.Scheme = Uri.UriSchemeHttps;
		                    /// changing to default port - this needs to be fixed once we change the client UI to have the SSL port
                		    tempUri.Port = 443;
                		}
				/// always use SSL for auth
				domainServiceUrl = new Uri( tempUri.Uri , DomainServicePath ); 
			}

			UTF8Encoding utf8Name = new UTF8Encoding();
                        byte[] encodedCredsByteArray = utf8Name.GetBytes(user);
                        string iFolderUserBase64 = Convert.ToBase64String(encodedCredsByteArray);

                        encodedCredsByteArray = utf8Name.GetBytes(password);
                        string iFolderPassBase64 = Convert.ToBase64String(encodedCredsByteArray);

			// Build a credential from the user name and password.
			NetworkCredential myCred = new NetworkCredential( iFolderUserBase64, iFolderPassBase64 ); 
			NetworkCredential myOldCred = new NetworkCredential( user, password );

			// Create the domain service web client object.
			DomainService domainService = new DomainService();
			domainService.CookieContainer = cookies;
			domainService.Url = domainServiceUrl.ToString();
			domainService.Proxy = ProxyState.GetProxyState(domainServiceUrl);
			domainService.AllowAutoRedirect = true;
			
			// Check to see if this domain already exists in this store.
			string domainID = null;
			try
			{
				domainID = domainService.GetDomainID();
			}
			catch (WebException we)
			{
				if (TestTrustFailure(host, we))
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

			domainService.Credentials = myCred;
			domainService.PreAuthenticate = true;
	
					
			if ( ( domainID != null ) && ( store.GetDomain( domainID ) != null ) )
			{
				throw new ExistsException( String.Format( "Domain {0}", domainID ) );
			}

			string baseUrl = domainServiceUrl.ToString();
			baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf('/'));

			// This flag is used to identify if server is old and not supporting multi byte login
			// Then creds are sent with out encoding again.

			bool oldServer = false;

//Login to the server mentioned by the user, possible the user is already provisioned and this is a different client that is creating an account
			status = 
				this.Login( 
					new Uri( baseUrl ),
					domainID,
					myCred,
					false);
            if ((status.statusCode != SCodes.Success) && (status.statusCode != SCodes.SuccessInGrace) && (status.statusCode != SCodes.UserAlreadyMoved))
			{
				log.Debug("Got Status {0}", status.statusCode);
				if( status.statusCode == SCodes.InvalidCredentials )
				{
					// Post 3.8.0.2, multibyte char support for usernames and password is added
					// If a new client is connecting to an old server, then auth will fail as creds 
 					// are encoded. Hence we try once more without encoding the creds.
					log.Debug("This might be old server 3.8.0.2 with no multi byte support, trying once more with out encoding the creds");
					status = this.Login( new Uri( baseUrl ), domainID, myOldCred, false );
					if ( status.statusCode == SCodes.Success ||                                                                                             status.statusCode == SCodes.SuccessInGrace )
						oldServer = true;
					else
						return status;
				}
			}
			else
				log.Debug("Got else Status {0}", status.statusCode);

			// Get the Home Server.
			domainService.Credentials = oldServer ? myOldCred : myCred ;
			string hostID = null;
			HostInfo hInfo = new HostInfo();

            try
            {
                hInfo = domainService.GetHomeServer(user);
                if (hInfo == null)
                {
                    string masterServerURL = null;
                    HostInfo[] serverList = domainService.GetHosts();
                    //Provisioning should be done only in master.
                    foreach (HostInfo server in serverList)
                    {
                        if (server.Master)
                            masterServerURL = server.PublicAddress;
                    }

                    //Now Talk to the master server and provision the user.
                    DomainService ds = new DomainService();
                    ds.CookieContainer = cookies;
                    ds.Url = (new Uri(masterServerURL.TrimEnd(new char[] { '/' }) + DomainService)).ToString();
                    ds.Credentials = oldServer ? myOldCred : myCred;
                    ds.PreAuthenticate = true;
                    //						ds.Proxy = ProxyState.GetProxyState( domainServiceUrl );
                    ds.AllowAutoRedirect = true;
                    hInfo = ds.GetHomeServer(user);
                }
            }
            catch (WebException we)
            {
                if (TestTrustFailure(host, we))
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
            catch (Exception ex)
            {
                // We are talking to an older server. We don't support multi-server.
                log.Debug("Exception: While Logging in");
                //rethrow the exception with new message
               if (ex.Message.IndexOf("Server did not recognize the value of HTTP header SOAPAction: \"http://novell.com/simias/domain/GetHomeServer\"") != -1 )
                {
                    throw new Exception("logging to old server");
                }
            }
            

			homeServerURL = hInfo.PublicAddress.TrimEnd( new char[] {'/'} );
            hostID = hInfo.ID;
			//Perform new login only if we have redirected
			if(!(new Uri(hInfo.PublicAddress)).Host.Equals(domainServiceUrl.Host))
			{
				log.Debug("Hinfo {0} DomainUrl {1}", hInfo.PublicAddress, domainServiceUrl.ToString());

				domainServiceUrl = new Uri(homeServerURL + DomainService);
				domainService.Url = domainServiceUrl.ToString();
				hostID = hInfo.ID;

				//logout the previous connection
				status2 = this.Logout(domainID);
				log.Debug("Logging out 1");
		
				// Now login to the homeserver.
				status = 
					this.Login( 
					new Uri( hInfo.PublicAddress ),
					domainID,
					oldServer ? myOldCred : myCred,
					false);
				if ( ( status.statusCode != SCodes.Success ) && ( status.statusCode != SCodes.SuccessInGrace ) )
				{
					log.Debug("Got Status {0} Host ID {1}", status.statusCode, hInfo.PublicAddress.TrimEnd( new char[] {'/'} ));
					if( status.statusCode == SCodes.InvalidCertificate)
					{
						// using the UserName to capture the new host address, so that UI can relogin with the new address - mainly for SSL cert
						status.UserName = (new Uri(homeServerURL)).Host;
						status.RemainingGraceLogins = 5;
						//Log out the current connection
						log.Debug("Logging out 2");
						status2 = this.Logout(domainID);
					}
					return status;
				}

			}

//TODO: We need to find a better way of doing the following(proxy creation) -- FIXME
			// Get just the path portion of the URL.
//			string hostString = domainServiceUrl.ToString();
//			int startIndex = hostString.LastIndexOf( "/" );
			Uri hostUri = new Uri( homeServerURL);

			// The web state object lets the connection use common state information.
			WebState webState = new WebState(domainID);
			webState.InitializeWebClient(domainService, domainID);

			// Save the credentials
			CredentialCache myCache = new CredentialCache();
			myCache.Add(new Uri(domainService.Url), "Basic", oldServer ? myOldCred : myCred);
			domainService.Credentials = myCache;
			domainService.Timeout = 30000;

			log.Debug("Calling " + domainService.Url + " to Initialize the user");

			// provision user
			ProvisionInfo provisionInfo = null;
			try
			{
				log.Debug("Initializing user on the server.....");
				provisionInfo = domainService.InitializeUserInfo(user);
				log.Debug("Initialized the user.....");
			}
			catch(Exception ex)
			{
				log.Debug("Exception while provisioning..possibly 3.6 server or older");
				log.Debug("Provisioning user on the server.....");
				provisionInfo = domainService.ProvisionUser(user, password);
				log.Debug("Provisioned the user.....");
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
				// log.Debug("Skipping creation on domain proxy");
				//putting this in try catch as old server will not have this API implemented and 
				//in that case no need to create any domain owner proxy
				string ownernodeid = null;
				try
				{
					ownernodeid = domainService.GetAdminNodeID();
				}
				catch {}
				CreateDomainProxy(store, provisionInfo.UserID, domainInfo, hostUri, hInfo, ownernodeid);

				// Create PO Box proxy
				log.Debug("Skipping pobox creation");
				//CreatePOBoxProxy(store, domainInfo.ID, provisionInfo, hostID);

				// create domain identity mapping.
				store.AddDomainIdentity(domainInfo.ID, provisionInfo.UserID);

				// authentication was successful - save the credentials
				// we have not yet integrated with CASA -FIX ME
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

			//Trigger the sync
			SyncClient.ScheduleSync(domainInfo.ID);
			//Wait for the logged in member to get synced (it is always synced first)
//localstore will always have the member, as the member proxy is created - possibly we may want to check for the policy information, which is the right thing to check as it gets synced
                        store = Store.GetStore();
                        Simias.Storage.Domain domain = store.GetDomain(domainInfo.ID);
                        bool notsynced=true;
                        int waitTime = 0;
            int syncWaitTime = 0;
            while (notsynced && waitTime < 15 && syncWaitTime < 45) //wait if our collection (domain) is getting synced else wait for 10 sec delay and break
                        {
                                try
                                {
                                        log.Debug("Waiting for Member node Sync ...");
                                        log.Debug("Attach : Sync Col ID:{0} Domain Col ID:{1}", SyncClient.CurrentiFolderID, domainInfo.ID);
                                        Simias.Storage.Member member = domain.GetCurrentMember();
                                        if(member.Family == null)
                                        {
                                                log.Debug("Attach : Sync Col ID:{0} Domain Col ID:{1}", SyncClient.CurrentiFolderID, domainInfo.ID);
                                                if(SyncClient.CurrentiFolderID !=null && SyncClient.CurrentiFolderID == domainInfo.ID)
                                                {
                                                        //wait more time
                                                        syncWaitTime = syncWaitTime + 1;
                            log.Debug("syncWaitTime Count : {0}", syncWaitTime);
                                                }
                                                else
                                                {
                                                        //wait less time
                            waitTime = waitTime + 1;
                            log.Debug("Wait Count : {0}", waitTime);
                                                }
                                                Thread.Sleep(2 * 1000);
                                                continue;
                                        }
                                        else
                                        {
                                                log.Info("Member synced");
                                                notsynced = false;
                                        }
                                }
                                catch (Exception ex)
                                {
                                        log.Info("Attach member sync wait {0}", ex.Message);
                                }
                        }
            if (notsynced == true)
            {
                log.Debug("Notsynced, remove the domain");
                status.statusCode = Simias.Authentication.StatusCodes.Timeout;
            }
			return status;
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

			// This flag is used to identify if server is old and not supporting multi byte login
                        // Then creds are sent with out encoding again.

			bool oldServer = false;
			
			UTF8Encoding utf8Name = new UTF8Encoding();
                        byte[] encodedCredsByteArray = utf8Name.GetBytes(Username);
                        string iFolderUserBase64 = Convert.ToBase64String(encodedCredsByteArray);

                        encodedCredsByteArray = utf8Name.GetBytes(Password);
                        string iFolderPassBase64 = Convert.ToBase64String(encodedCredsByteArray);
				
			NetworkCredential myCred = new NetworkCredential( iFolderUserBase64, iFolderPassBase64 );
			NetworkCredential myOldCred = new NetworkCredential( Username, Password );

			Simias.Authentication.Status status = null;
			Domain cDomain = store.GetDomain( DomainID );
			if ( cDomain != null )
			{
				if ( cDomain.Role == SyncRoles.Slave )
				{
                    UriBuilder tempUri = new UriBuilder(DomainProvider.ResolveLocation( DomainID ));
                    /// only if the URL has http, then we default to default SSL port, otherwise use the port
                    /// that is mentioned in the URL itself
                    if (tempUri.Scheme.Equals(Uri.UriSchemeHttp))
                    {
                        tempUri.Port = 443;
                        tempUri.Scheme = Uri.UriSchemeHttps;
                    }
					status = 
						this.Login( 
							tempUri.Uri, 
							DomainID,
							myCred,
							false );
							
					if ( status.statusCode == SCodes.Success ||
						status.statusCode == SCodes.SuccessInGrace )
					{
						BasicCredentials basic = 
							new BasicCredentials( 
									DomainID, 
									DomainID,
									iFolderUserBase64,
									iFolderPassBase64 );
						basic.Save( false );
						SetDomainState( DomainID, true, true );
					}
					else
					{
						// Post 3.8.0.2, multibyte char support for usernames and password is added
        	                                // If a new client is connecting to an old server, then auth will fail as creds
	                                        // are encoded. Hence we try once more without encoding the creds.
						log.Debug("possibly server is 3.8.0.2 not supporting multi byte,trying again without encoding creds ");
						status = this.Login( tempUri.Uri, DomainID, myOldCred, false);
						if ( status.statusCode == SCodes.Success ||						                                                status.statusCode == SCodes.SuccessInGrace )
						{
							oldServer = true;
							BasicCredentials basic = 
								new BasicCredentials(
										DomainID,
										DomainID,
										Username,
										Password);
							basic.Save( false );
							SetDomainState( DomainID, true, true);
						}
					}

                    if (status.statusCode == SCodes.UserAlreadyMoved && Password != null)
                    {
                        // Connect to master , and login to new server where user has been moved
                        status = ProvisionToNewHomeServer(DomainID, oldServer ? myOldCred : myCred);
                        return status;
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
        /// Provision the user to new server where he has been moved, Also update local store with new server's ip
        /// </summary>
        /// <param name="DomainID">The ID of the domain.</param>
        /// <param name="Creds">Network credential of the user</param>
        /// <returns>The status of the provisioning.</returns>
        public Simias.Authentication.Status ProvisionToNewHomeServer(string DomainID, NetworkCredential creds)
        {
            Simias.Authentication.Status status = new Simias.Authentication.Status();
            Simias.Authentication.Status status2 = new Simias.Authentication.Status();
            status.statusCode = Simias.Authentication.StatusCodes.Success; ;
            
            Domain domain = store.GetDomain(DomainID);
            if (domain == null)
            {
                return (status);
            }
            string masterServerURL = null;
            string homeServerURL = null;
            string hostID = null;
            Uri domainServiceUrl = null;
            CookieContainer cookies = new CookieContainer();

            // Get master server url from local domain service
            
            HostInfo hostinfo = null;
            try
            {
                //serverList = domainService.GetHosts();
                HostNode HNode = HostNode.GetMaster(DomainID);
                if (HNode != null)
                {
                    masterServerURL = HNode.PublicUrl;//.Replace("http","https");
                    
                }
            }
            catch (Exception ex)
            {
                log.Debug("Got exception:");
            }
            

            HostInfo hInfo = null;

            try
            {
                // Now talk to the master server , provision and get the new home server ip
                DomainService ds = new DomainService();
                ds.CookieContainer = cookies;
                ds.Url = (new Uri(masterServerURL.TrimEnd(new char[] { '/' }) + DomainService)).ToString();
                ds.Credentials = creds;
                ds.PreAuthenticate = true;
                ds.AllowAutoRedirect = true;

                // Get home server
                hInfo = ds.GetHomeServer(creds.UserName);
            }
			catch (WebException we)
			{
                if (TestTrustFailure(masterServerURL, we))
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
            catch (Exception ex)
            {
                // We are talking to an older server. We don't support multi-server.
                log.Debug("Exception: While Logging in");
                //rethrow the exception with new message
                if (ex.Message.IndexOf("Server did not recognize the value of HTTP header SOAPAction: \"http://novell.com/simias/domain/GetHomeServer\"") != -1)
                {
                    throw new Exception("logging to old server");
                }
            }

            // Now we have got home server , login to that 
            homeServerURL = hInfo.PublicAddress.TrimEnd(new char[] { '/' });
            //hostID = hInfo.ID;
            hostID = hInfo.MemberID;
            // This hostid is old host id
            string OldHostID = domain.HostID;


            // User logging to new server is successful

            Uri hostUri = new Uri(homeServerURL);
            
           
                Member mem;
                String MemUserID = null;
                mem = domain.GetMemberByName(creds.UserName);
                if (mem == null)
                {
                    log.Debug("member is null , so returning from Provision ToNewHomeServer");
                    status.statusCode = Simias.Authentication.StatusCodes.Unknown;
                    return status;
                }
                MemUserID = mem.UserID;
                Property prop = mem.Properties.FindSingleValue(PropertyTags.HostID);
                if (prop != null)
                {
                    OldHostID = prop.Value.ToString();
                }
                
                if (MemUserID != null)
                {  
                    ICSList CollectionList = store.GetCollectionsByOwner(MemUserID);
                    if (CollectionList != null && CollectionList.Count > 0)
                    {
                        foreach (ShallowNode sn in CollectionList)
                        {
                            ArrayList CommitList = new ArrayList();
                            Collection col = new Collection(store, sn);
                            if (col.HostID != null && col.HostID != "")
                            {
                                Member HomeServerMem = domain.GetMemberByID(OldHostID);
                                 //Either collection's hostid is equal to member's old hostid, or
                                 // collection's hostidid equal to old server's id, update
                                 if (col.HostID == OldHostID || (HomeServerMem != null && HomeServerMem.ID == col.HostID) )
                                 {
                                     col.HostID = hostID;
                                 }
                                 col.Commit();
                            }
                        }
                    }

                    ICSList ColList = store.GetCollectionsByUser(MemUserID);
                    if(ColList != null && ColList.Count > 0)
                    {
                        foreach(ShallowNode sn in ColList)
                        {
                            ArrayList CommitList = new ArrayList();
                            Collection col = new Collection(store, sn);
                            if (col == null)
                            {
                                log.Debug("could not form collection object while changing hostid");
                                continue;
                            }

                           

                        }
                    }
                               
                
                 HostNode newHNode = null;
                 if ((newHNode = HostNode.GetHostByID(domain.ID, hostID)) != null)
                     mem.HomeServer = newHNode;
                 else
                     log.Debug("It will wait till next domain sync");

            
                Property p = new Property(PropertyTags.HostAddress, hostUri);
                p.LocalProperty = true;
                domain.Properties.ModifyNodeProperty(p);
                domain.HostID = hostID;
                domain.Commit();

                try
                {
                    domainServiceUrl = new Uri(homeServerURL + DomainService);

                    status2 = this.Logout(domain.ID);
                    log.Debug("provisiontonewhomeserver: Logging out 1");


                    status =
                        this.Login(
                        new Uri(hInfo.PublicAddress),
                        domain.ID,
                        creds,
                        false);
                    if ((status.statusCode != SCodes.Success) && (status.statusCode != SCodes.SuccessInGrace))
                    {
                        log.Debug("provisiontonewhomeserver: Got Status {0} Host ID {1}", status.statusCode, hInfo.PublicAddress.TrimEnd(new char[] { '/' }));
                        if (status.statusCode == SCodes.InvalidCertificate)
                        {
                            status.UserName = (new Uri(homeServerURL)).Host;
                            status.RemainingGraceLogins = 5;
                            log.Debug("provisiontonewhomeserver: Logging out 2");
                            status2 = this.Logout(domain.ID);
                        }
                        return status;
                    }

                }
                catch (Exception ex)
                { log.Debug("ProvisionToNewHomeServer: exception {0}",ex);}
            }
            status.statusCode = Simias.Authentication.StatusCodes.UserAlreadyMoved;
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
            string userID = store.GetUserIDFromDomainID(DomainID);
			Simias.Storage.Domain domain = store.GetDomain( DomainID );
			if( domain == null )
			{
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
			}

			// Set the state for this domain.
			SetDomainState( DomainID, false, false );

			// Clear the password from the cache.
			Member member = domain.GetMemberByID( userID );
			if ( member != null )
			{
				// Clear the entry from the cache.
				BasicCredentials basic = new BasicCredentials( DomainID, DomainID, member.Name );
				basic.Remove();
			}
			HostConnection.ClearConnection(domain.HostID);
			WebState.ResetWebState( DomainID );
			currentDomainID = DomainID;
			Thread resetconnectionsThread = new Thread(new ThreadStart(PerformResetConnections));
			resetconnectionsThread.Start();
			//SyncClient.ResetConnections( DomainID);
			
			// Clear the cookies for this Uri.
			//WebState.ResetWebState( DomainID );
			return new Simias.Authentication.Status( SCodes.Success );
		}

		public void PerformResetConnections()
		{
			log.Debug("Raising the logout event inside thread ");
			string DomainID = currentDomainID;
			currentDomainID = null;
			SyncClient.ResetConnections(DomainID);
			log.Debug("out logout thread");
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


		public void RemoveCertFromTable(string host)
		{
			Simias.Security.CertificateStore.RemoveCertFromTable(host);
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
			log.Debug("Unattaching the domain");
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

		            // Remove the domain from the table
	                lock (domainTable)
        	        {
                 		domainTable.Remove(domainID);
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
			log.Debug("Url - unattach {0}", domainService.Url.ToString());
			if (!localOnly)
			{
				WebState webState = new WebState(domainID, userID);
				webState.InitializeWebClient(domainService, domainID);
			}

			// Find the user's POBox for this domain.
			POBox.POBox poBox = POBox.POBox.FindPOBox(store, domainID, userID);
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

					if (!localOnly)
					{
						// Remove the user from the domain server.
						domainService.RemoveServerCollections(domainID, userID);
					}
				}
				else
				{
					log.Debug("pobox is null");
				}

               string TrimmedUri = uri.ToString().TrimEnd(new char[] { '/' });
                try
                {
                    Simias.Security.CertificateStore.RemoveCertificate(TrimmedUri);
                }
                catch (Exception ex)
                {
                    log.Debug("Error while removing the certificate for:{0} ", TrimmedUri);
                    throw new SimiasException(String.Format("Error while removing the certificate for:{0} ", TrimmedUri));
                }

                /// remove rest of the nodes in the domain
                System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(RemoveDomainThread));
                DomainAgent.domainID = domainID;
                thread.Priority = ThreadPriority.BelowNormal;
                thread.IsBackground = true;
                thread.Name = "Remove Domain";
                thread.Start();

                
                try
                {
                    ProxyState.DeleteProxyState(new Uri(domainService.Url));
                }
                catch { }
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
			log.Debug("Url - createmaster {0}", domainService.Url.ToString());
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

        public bool IsDomainRemoved(string DomainID)
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
            return false;
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

			// Get the network location of the server.
			Uri uri = DomainProvider.ResolveLocation( domain.ID );
			if (uri == null)
			{
				throw new SimiasException( String.Format( "The network location could not be determined for domain {0}.", domain.ID ) );
			}
            
            /* The function domainService.GetMemberDiskSpaceUsed is not implemented.
             * Uncomment the following lines when it gets implemented 
			// Construct the web client.
			DomainService domainService = new DomainService();
			domainService.Url = uri.ToString() + "/DomainService.asmx";
			log.Debug("Url - GetDomainDiskSpace {0}", domainService.Url.ToString());
			WebState webState = new WebState( domain.ID );
			webState.InitializeWebClient( domainService, domain.ID );

			return domainService.GetMemberDiskSpaceUsed( memberID, out limit );*/

            // Report the disk space quota from the local store. This may not be exact, but it is the 
            // last known good value.
            Member member = domain.GetMemberByID(memberID);
            if (member == null)
            {
                throw new DoesNotExistException(String.Format("Cannot find member {0} in domain {1}", memberID, domain.ID));
            }

            DiskSpaceQuota dsq = DiskSpaceQuota.Get(member);
            limit = dsq.Limit;
            return dsq.UsedSpace;
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

			// Get the network location of the server where this collection is hosted.
			Uri uri = DomainProvider.ResolveLocation( collection );
			if (uri == null)
			{
				throw new SimiasException( String.Format( "The network location could not be determined for collection {0}.", collectionID ) );
			}

            /* The function domainService.GetMemberDiskSpaceUsed is not implemented.
             * Uncomment the following lines when it gets implemented 
			// Construct the web client.
			DomainService domainService = new DomainService();
			domainService.Url = uri.ToString() + "/DomainService.asmx";
			log.Debug("Url - GetDomainDiskSpaceForCollection {0}", domainService.Url.ToString());
			WebState webState = new WebState( collection.Domain, collectionID );
			webState.InitializeWebClient( domainService, collection.Domain );

			// Get the quota from the local store this fixes bug 97331
			long usedSize = domainService.GetiFolderDiskSpaceUsed( collectionID, out limit );
			limit = DiskSpaceQuota.GetLimit( collection );
			return usedSize;*/

            // Report the collection limit from the local store. This may not be exact, but it is the 
            // last known good value.
            limit = DiskSpaceQuota.GetLimit(collection);
            return collection.StorageSize;
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
