/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Rob <rlyon@novell.com>
 *
 ***********************************************************************/

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
		/// Get domain information
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
				info.MemberRights = member.Rights.ToString();
			}
			else
			{
				throw new SimiasException( "User: " + userID + " does not exist" );
			}

			return info;
		}
		
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
				return ProvisionService.ProvisionUser( user );
			}
			
			return new Simias.Host.HostInfo(hNode);
		}

		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public ProvisionInfo ProvisionUserOnServer(string user, string password, byte[] ticket)
		{
			// Make sure the ticket was issued by the master

			return ProvisionUser( user, password );
		}
		

		/// <summary>
		/// Provision the user
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
			if (member != null)
			{
				info = new ProvisionInfo();
				info.UserID = member.UserID;

				// post-office box
				POBox.POBox poBox = POBox.POBox.GetPOBox( store, domain.ID, info.UserID );

				info.POBoxID = poBox.ID;
				info.POBoxName = poBox.Name;

				Member poMember = poBox.GetMemberByID( member.UserID );
				info.MemberNodeName = poMember.Name;
				info.MemberNodeID = poMember.ID;
				info.MemberRights = poMember.Rights.ToString();
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
				// files path
				string path = Path.Combine(Store.StorePath, FilesDirectory);
				path = Path.Combine(path, collectionID);
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
		
		#endregion
	}
}
