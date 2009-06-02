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
*                 $Author: Rob <rlyon@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Server;

using Simias.DomainService;


namespace Simias.DomainService.Web
{
	/// <summary>
	/// Domain Service
	/// </summary>
	[WebService(
		Namespace="http://novell.com/simias/domain",
		Name="Domain Service",
		Description="Web Service providing access to Simias domain services.")]
	public class DomainService : System.Web.Services.WebService
	{
		private static readonly string FilesDirectory = "SimiasFiles";
		

		#region Private Methods		
		/// <summary>
		/// Removes the collection subscription from the 
		/// </summary>
		/// <param name="collection">Collection that subscription needs to be removed for.</param>
		private void RemoveCollectionSubscription( Collection collection )
		{
			Store store = Store.GetStore();

			// Get all subscription nodes for this collection.
			ICSList subList = store.GetNodesByProperty( new Property( Subscription.SubscriptionCollectionIDProperty, collection.ID ), SearchOp.Equal );
			foreach ( ShallowNode sn in subList )
			{
				// Get the collection object for this node.
				Collection c = store.GetCollectionByID( sn.CollectionID );
				if ( c != null )
				{
					// Delete this node from the POBox collection.
					c.Commit( c.Delete( new Node( c, sn ) ) );
				}
			}
		}
		#endregion
		

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		public DomainService()
		{
		}
		#endregion
		
		
		#region WebService Methods
		/// <summary>
		/// Get domain information... This should be called only from clients and not slave server. Because for secondary admins, the rights returned
                /// will be ReadWrite. Because older clients do no understand the new rights, so we have to return ReadWrite.
		/// </summary>
		/// <param name="userID">The user ID of the member requesting domain information.</param>
		/// <returns>A DomainInfo object that contains information about the enterprise server.</returns>
		/// 
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public DomainInfo GetDomainInfo( string userID )
		{
			Simias.Server.EnterpriseDomain enterpriseDomain = 
				new Simias.Server.EnterpriseDomain( false );
			if ( enterpriseDomain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}
			
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain( enterpriseDomain.ID );
			if ( domain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}
			
			DomainInfo info = new DomainInfo();
			info.ID = domain.ID;
			info.Name = domain.Name;
			info.Description = domain.Description;
		
			// member info
			Member member = domain.GetMemberByID( userID );
			if ( member != null )
			{
				info.MemberNodeName = member.Name;
				info.MemberNodeID = member.ID;

				// Clients before this build do not understand "Secondary" rights so change that to ReadWrite. Also, Secondary rights
				// are relevant only for server and not for clients, so makes sense.
				Access.Rights rights = ( Access.Rights ) member.Rights;// ( Access.Rights )Enum.Parse( typeof( Access.Rights ), member.Rights );
				Access.Rights NewRight = rights;
				if ( rights == Access.Rights.Secondary) 
					NewRight = Access.Rights.ReadWrite;
				info.MemberRights = NewRight.ToString();
			}
			else
			{
				throw new SimiasException( "User: " + userID + " does not exist" );
			}

			return info;
		}
		
        /// <summary>
        /// Get the list of all hosts in the server domain
        /// </summary>
        /// <returns>HostInfo array object containing info about each host</returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public Simias.Host.HostInfo[] GetHosts()
		{
			Simias.Server.EnterpriseDomain enterpriseDomain = 
				new Simias.Server.EnterpriseDomain( false );
			if ( enterpriseDomain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}
			
			Simias.Host.HostInfo[] infoList;
			HostNode[] hosts = HostNode.GetHosts(enterpriseDomain.ID);

			if (hosts.Length > 0)
			{
				infoList = new Simias.Host.HostInfo[hosts.Length];
				int i = 0;
				foreach (HostNode hn in hosts)
				{
					infoList[i++] = new Simias.Host.HostInfo(hn);
				}
			}
			else
			{
				infoList = new Simias.Host.HostInfo[0];
			}

			return infoList;
		}

        /// <summary>
        /// Get home server for the user
        /// </summary>
        /// <param name="user">user name</param>
        /// <returns>HostInfo object containing user's home server, null if unsuccessful</returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public Simias.Host.HostInfo GetHomeServer( string user )

		{
			Simias.Server.EnterpriseDomain enterpriseDomain = 
				new Simias.Server.EnterpriseDomain( false );
			if ( enterpriseDomain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}
			
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain( enterpriseDomain.ID );
			if ( domain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}
		
			// find user
			Member member = domain.GetMemberByName( user );
			HostNode hNode = member.HomeServer;
			if ( hNode == null )
			{
			        if (HostNode.GetLocalHost().IsMasterHost)
				        return ProvisionService.ProvisionUser( user );
				else 
				{
				        return null;
					//need to get the home server from master.
				}
			} 
			
			return new Simias.Host.HostInfo(hNode);
		}

        /// <summary>
        /// Provision the user on server
        /// </summary>
        /// <param name="user">user name</param>
        /// <param name="password">password</param>
        /// <param name="ticket">byte array for ticket issued by master, but not used currently int his function</param>
        /// <returns>Provision Info about the user account</returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public ProvisionInfo ProvisionUserOnServer(string user, string password, byte[] ticket)
		{
			// Make sure the ticket was issued by the master

			return ProvisionUser( user, password );
		}

		/// <summary>
		/// change the password for user
		/// </summary>
		/// <param name="DomainID">Domain id for this user</param>
		/// <param name="UserID">User ID</param>
		/// <param name="OldPassword">OldPassword</param>
		/// <param name="NewPassword">NewPassword</param>
		/// <returns>the status after password change </returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public int ChangePasswordOnServer( string DomainID, string UserID, string OldPassword, string NewPassword)
		{
			int retval = 0;
			return Simias.Server.User.ChangePassword(UserID, OldPassword, NewPassword);
		}

		/// <summary>
		/// Provision the user, This should be called only from clients and not slave server. Because for secondary admins, the rights returned
		/// will be ReadWrite. Because older clients do no understand the new rights, so we have to return ReadWrite.
		/// </summary>
		/// <param name="user">Identifier of the user to provision on the server.</param>
		/// <param name="password">Password to verify the user's identity.</param>
		/// <returns>A ProvisionInfo object that contains information about the account
		/// setup for the specified user.</returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public ProvisionInfo ProvisionUser(string user, string password)
		{
			ProvisionInfo info = null;
			
			Simias.Server.EnterpriseDomain enterpriseDomain = 
				new Simias.Server.EnterpriseDomain( false );
			if ( enterpriseDomain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}
			
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain( enterpriseDomain.ID );
			if ( domain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}

			// find user
			Member member = domain.GetMemberByName( user );
			if ( member != null )
			{
				// Provision the user on a host
				Simias.Server.ProvisionService.ProvisionUser( user );

				info = new ProvisionInfo();
				info.UserID = member.UserID;

				// post-office box
				//POBox.POBox poBox = POBox.POBox.GetPOBox( store, domain.ID, info.UserID );

				//info.POBoxID = poBox.ID;
				//info.POBoxName = poBox.Name;

				//Member poMember = poBox.GetMemberByID( member.UserID );
				//info.MemberNodeName = poMember.Name;
				//info.MemberNodeID = poMember.ID;
				//info.MemberRights = poMember.Rights.ToString();

				info.MemberNodeName = member.Name;
				info.MemberNodeID = member.ID;

				// Clients before this build do not understand "Secondary" rights so change that to ReadWrite. Also, Secondary rights
				// are relevant only for server and not for clients, so makes sense.
				Access.Rights rights = ( Access.Rights ) member.Rights;// ( Access.Rights )Enum.Parse( typeof( Access.Rights ), member.Rights );
				Access.Rights NewRight = rights;
				if ( rights == Access.Rights.Secondary) 
					NewRight = Access.Rights.ReadWrite;
				info.MemberRights = NewRight.ToString();
			}
			else
			{
				throw new SimiasException( "User: " + user + " does not exist" );
			}

			return info;
		}

		/// <summary>
		/// Initialize the user information like POBox etc. This assumes the user is already provisioned... This should be called only from clients 
		/// and not slave server. Because for secondary admins, the rights returned
                /// will be ReadWrite. Because older clients do no understand the new rights, so we have to return ReadWrite.
		/// </summary>
		/// <param name="user">Identifier of the user to provision on the server.</param>
		/// <param name="password">Password to verify the user's identity.</param>
		/// <returns>A ProvisionInfo object that contains information about the account
		/// setup for the specified user.</returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public ProvisionInfo InitializeUserInfo(string user)
		{
			ProvisionInfo info = null;
			
			Simias.Server.EnterpriseDomain enterpriseDomain = 
				new Simias.Server.EnterpriseDomain( false );
			if ( enterpriseDomain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}
			
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain( enterpriseDomain.ID );
			if ( domain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}

			// find user
			Member member = domain.GetMemberByName( user );
			if ( member != null )
			{

				info = new ProvisionInfo();
				info.UserID = member.UserID;

				// post-office box
				//POBox.POBox poBox = POBox.POBox.GetPOBox( store, domain.ID, info.UserID );

				//info.POBoxID = poBox.ID;
				//info.POBoxName = poBox.Name;

				//Member poMember = poBox.GetMemberByID( member.UserID );
				//info.MemberNodeName = poMember.Name;
				//info.MemberNodeID = poMember.ID;
				//info.MemberRights = poMember.Rights.ToString();
				info.MemberNodeName = member.Name;
				info.MemberNodeID = member.ID;

				// Clients before this build do not understand "Secondary" rights so change that to ReadWrite. Also, Secondary rights
				// are relevant only for server and not for clients, so makes sense.
				Access.Rights rights = ( Access.Rights ) member.Rights;// ( Access.Rights )Enum.Parse( typeof( Access.Rights ), member.Rights );
				Access.Rights NewRight = rights;
				if ( rights == Access.Rights.Secondary) 
					NewRight = Access.Rights.ReadWrite;
				info.MemberRights = NewRight.ToString();
			
			}
			else
			{
				throw new SimiasException( "User: " + user + " does not exist" );
			}

			return info;
		}

		/// <summary>
		/// Create the master collection
		/// </summary>
		/// <param name="collectionID">Identifier of the collection to create.</param>
		/// <param name="collectionName">Name of the collection object.</param>
		/// <param name="rootDirID">Identifier of the rootDir node to create if applicable.</param>
		/// <param name="rootDirName">Name of the rootDir node object</param>
		/// <param name="userID">Identifier of the user who owns this collection.</param>
		/// <param name="memberName">Name of the member object that is the owner of this collection.</param>
		/// <param name="memberID">Identifier of the member object that is the owner of this collection.</param>
		/// <param name="memberRights">Rights of the member that is the owner of this collection.</param>
		/// <returns>The master url that the client should use to contact the server.</returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public string CreateMaster(string collectionID, string collectionName, string rootDirID, string rootDirName, string userID, string memberName, string memberID, string memberRights)
		{
			Simias.Server.EnterpriseDomain enterpriseDomain = 
				new Simias.Server.EnterpriseDomain( false );
			if ( enterpriseDomain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}
			
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain( enterpriseDomain.ID );
			if ( domain == null )
			{
				throw new SimiasException( "Enterprise server domain does not exist." );
			}
			
			ArrayList nodeList = new ArrayList();
			Collection c = new Collection( store, collectionName, collectionID, domain.ID );
			c.Proxy = true;
			nodeList.Add(c);
		
			string existingUserID = Thread.CurrentPrincipal.Identity.Name;
			Member existingMember = domain.GetMemberByID(existingUserID);
			if (existingMember == null)
			{
				throw new SimiasException(String.Format("Impersonating user: {0} is not a member of the domain.", Thread.CurrentPrincipal.Identity.Name));
			}

			// Make sure the creator and the owner are the same ID.
			if (existingUserID != userID)
			{
				throw new SimiasException(String.Format("Creator ID {0} is not the same as the caller ID {1}.", existingUserID, userID));
			}

			// member node.
			Access.Rights rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), memberRights );
			Member member = new Member( memberName, memberID, userID, rights, null );
			member.IsOwner = true;
			member.Proxy = true;
			nodeList.Add( member );
	
			// check for a root dir node
			if (((rootDirID != null) && (rootDirID.Length > 0))
				&& (rootDirName != null) && (rootDirName.Length > 0))
			{
				// Get the collections Unmanaged Path
				string path = c.UnmanagedPath;
				path = Path.Combine(path, rootDirName);

				// create root directory node
				DirNode dn = new DirNode(c, path, rootDirID);

				if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);

				dn.Proxy = true;
				nodeList.Add(dn);
			}

			// Create the collection.
			c.Commit( nodeList.ToArray( typeof(Node) ) as Node[] );

			// get the collection master url
			Uri request = Context.Request.Url;
			UriBuilder uri = 
				new UriBuilder(request.Scheme, request.Host, request.Port, Context.Request.ApplicationPath.TrimStart( new char[] {'/'} ) );
			return uri.ToString();
		}

		/// <summary>
		/// Deletes all of the collections that the specified user is a member of and deletes
		/// the user's membership from all collections that he belongs to from the enterprise server.
		/// </summary>
		/// <param name="DomainID">Identifier of the domain that the userID is in.</param>
		/// <param name="UserID">Identifier of the user to remove.</param>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public void RemoveServerCollections( string DomainID, string UserID)
		{
			Store store = Store.GetStore();
			if(DomainID == null)
			{
				throw new SimiasException("Null Domain ID");
			}
			
			// This method can only target the simple server
			Simias.Storage.Domain domain = store.GetDomain( DomainID ); 
			if ( domain == null )
			{
				throw new SimiasException( "Specified server domain does not exist." );
			}

			/*
			if ( domainID != domain.ID )
			{
				throw new SimiasException("Only the Simias Server domain can be used.");
			}
			*/

			// Make sure that the caller is the current owner.
			string existingUserID = Thread.CurrentPrincipal.Identity.Name;
			Member existingMember = domain.GetMemberByID( existingUserID );
			if ( existingMember == null )
			{
				throw new SimiasException(String.Format("Impersonating user: {0} is not a member of the domain.", Thread.CurrentPrincipal.Identity.Name));
			}

			// Make sure the creator and the owner are the same ID.
			if ( existingUserID != UserID )
			{
				throw new SimiasException( String.Format( "Creator ID {0} is not the same as the caller ID {1}.", existingUserID, UserID ) );
			}

			// Get all of the collections that this user is member of.
			ICSList cList = store.GetCollectionsByUser( UserID );
			foreach( ShallowNode sn in cList )
			{
				// Remove the user as a member of this collection.
				Collection c = new Collection( store, sn );

				// Only look for collections from the specified domain and
				// don't allow this user's membership from being removed from the domain.
				if ( ( c.Domain == DomainID ) && 
					!( (Node) c).IsBaseType( Simias.Client.NodeTypes.DomainType ) )
				{
					Member member = c.GetMemberByID( UserID );
					if ( member != null )
					{
						if ( member.IsOwner )
						{
							// Don't remove an orphaned collection.
							if ( ( member.UserID != domain.Owner.UserID ) || ( c.PreviousOwner == null ) )
							{
								// The user is the owner, delete this collection.
								c.Commit( c.Delete() );
							}
						}
						else
						{
							// Not the owner, just remove the membership.
							c.Commit( c.Delete( member ) );
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the ID for this simias server domain.
		/// </summary>
		/// <returns>Domain ID for the server domain.</returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public string GetDomainID()
		{
			Simias.Server.EnterpriseDomain domain = 
				new Simias.Server.EnterpriseDomain( false );
			return ( domain != null ) ? domain.ID : null;
		}

        	/// <summary>^M
        	/// Get the domain owners node ID
        	/// </summary>^M
        	/// <returns>domain owners node ID string</returns>
                [WebMethod(EnableSession=true)]
                [SoapDocumentMethod]
                public string GetAdminNodeID()
                {
                        try
                        {
                                string nodeid = null;
                                Store store = Store.GetStore();
                                Domain domain = store.GetDomain( store.DefaultDomain );
                                Member owner = domain.Owner;
                                if( owner != null)
                                        return owner.ID;
                                else
                                        return null;
                        }
                        catch(Exception e)
                        {
                                return null;
                        }
                }
		
		#endregion
	}
}
