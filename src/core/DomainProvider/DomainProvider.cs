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
using System.Web;

using Simias.Client;
using Simias.Storage;

namespace Simias
{
	/// <summary>
	/// Class that implements the domain provider functionality.
	/// </summary>
	public class DomainProvider
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( DomainProvider ) );

		/// <summary>
		/// Table used to keep track of domain provider mappings.
		/// </summary>
		static private Hashtable domainProviderTable = new Hashtable();

		/// <summary>
		/// List that holds the registered providers.
		/// </summary>
		static private Hashtable registeredProviders = new Hashtable();
		#endregion

		#region Properties
		/// <summary>
		/// Gets the number of registered providers.
		/// </summary>
		static public int Count
		{
			get { return registeredProviders.Count; }
		}

		/// <summary>
		/// Returns the registered domain providers.
		/// </summary>
		static public IDomainProvider[] Providers
		{
			get
			{
				IDomainProvider[] providers = new IDomainProvider[ registeredProviders.Count ];
				lock ( typeof( DomainProvider ) )
				{
					registeredProviders.CopyTo( providers, 0 );
				}
				return providers;
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Searches the list of registered location providers and asks if
		/// any claim ownership of the specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of domain to claim ownership for.</param>
		/// <returns>An IDomainProvider object for the provider that claims
		/// the specified domain. A null is returned if no provider claims the
		/// domain.</returns>
		static private IDomainProvider GetDomainProvider( string domainID )
		{
			IDomainProvider provider = null;
log.Debug("domainID {0}", domainID);

			lock ( typeof( DomainProvider ) )
			{
				// See if there is a provider mapping for this domain.
				string idpName = domainProviderTable[ domainID ] as string;
				if ( idpName != null )
				{
					// There is a domain mapping already set.
					provider = registeredProviders[ idpName ] as IDomainProvider;
				}
				else
				{
					// Search for an owner for this domain.
					foreach( IDomainProvider idp in registeredProviders.Values )
					{
						// See if the provider claims this domain.
						if ( idp.OwnsDomain( domainID ) )
						{
							domainProviderTable.Add( domainID, idp.Name );
							provider = idp;
							break;
						}
					}
				}
			}

			return provider;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Performs authentication to the specified domain.
		/// </summary>
		/// <param name="domain">Domain to authenticate to.</param>
		/// <param name="httpContext">HTTP-specific request information. This is passed as a parameter so that a domain 
		/// provider may modify the HTTP request by adding special headers as necessary.
		/// 
		/// NOTE: The domain provider must NOT end the HTTP request.
		/// </param>
		/// <returns>The status from the authentication.</returns>
		static public Authentication.Status Authenticate( Domain domain, HttpContext httpContext )
		{
			IDomainProvider idp = GetDomainProvider( domain.ID );
			if ( idp == null )
			{
				throw new DoesNotExistException( "The specified domain does not exist." );
			}

			return idp.Authenticate( domain, httpContext );
		}

		/// <summary>
		/// Indicates to the provider that the specified collection has
		/// been deleted and a mapping is no longer required.
		/// </summary>
		/// <param name="collection">Collection that is being deleted.</param>
		static public void DeleteLocation( Collection collection )
		{
			IDomainProvider idp = GetDomainProvider( collection.Domain );
			if ( idp != null )
			{
				log.Debug( "Deleting location for collection {0}.", collection.Name );
				idp.DeleteLocation( collection.Domain, collection.ID );
			}
		}

		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain.</param>
		/// <param name="searchContext">Domain provider specific search context returned by 
		/// FindFirstDomainMembers or FindNextDomainMembers methods.</param>
		static public void FindCloseDomainMembers( string domainID, string searchContext )
		{
			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				log.Debug( "Closing search on domain {0}.", domainID );
				idp.FindCloseDomainMembers( searchContext );
			}
		}

		/// <summary>
		/// Starts a search for all domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		static public bool FindFirstDomainMembers( string domainID, int count, out string searchContext, out Member[] memberList, out int total )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			searchContext = null;
			memberList = null;
			total = 0;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				moreEntries = idp.FindFirstDomainMembers( domainID, count, out searchContext, out memberList, out total );
			}

			return moreEntries;
		}

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Attribute name to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		static public bool FindFirstDomainMembers( string domainID, string attributeName, string searchString, SearchOp operation, int count, out string searchContext, out Member[] memberList, out int total )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			searchContext = null;
			memberList = null;
			total = 0;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				moreEntries = idp.FindFirstDomainMembers( domainID, attributeName, searchString, operation, count, out searchContext, out memberList, out total );
			}

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for domain members from the current record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		static public bool FindNextDomainMembers( string domainID, ref string searchContext, int count, out Member[] memberList )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			memberList = null;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				moreEntries = idp.FindNextDomainMembers( ref searchContext, count, out memberList );
			}

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for domain members previous to the current record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		static public bool FindPreviousDomainMembers( string domainID, ref string searchContext, int count, out Member[] memberList )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			memberList = null;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				moreEntries = idp.FindPreviousDomainMembers( ref searchContext, count, out memberList );
			}

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for domain members from the specified record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="offset">Record offset to return members from.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		static public bool FindSeekDomainMembers( string domainID, ref string searchContext, int offset, int count, out Member[] memberList )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			memberList = null;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				moreEntries = idp.FindSeekDomainMembers( ref searchContext, offset, count, out memberList );
			}

			return moreEntries;
		}

		/// <summary>
		/// Informs the domain provider that the specified member object is about to be
		/// committed to the domain's member list. This allows an opportunity for the 
		/// domain provider to add any domain specific attributes to the member object.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <param name="member">Member object that is about to be committed to the domain's member list.</param>
		static public void PreCommit( string domainID, Member member )
		{
			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				idp.PreCommit( domainID, member );
			}
		}

		/// <summary>
		/// Registers the specified domain provider with the domain provider service.
		/// </summary>
		/// <param name="provider">An ILocationProvider interface object.</param>
		static public void RegisterProvider( IDomainProvider provider )
		{
			lock ( typeof( DomainProvider ) )
			{
				log.Debug( "Registering provider {0}.", provider.Name );
				registeredProviders.Add( provider.Name, provider );
			}
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// collection.
		/// </summary>
		/// <param name="collection">Collection to find the network 
		/// location for.</param>
		/// <returns>A Uri object that contains the network location.
		/// If the network location could not be determined, a null
		/// is returned.</returns>
		static public Uri ResolveLocation( Collection collection )
		{
			Uri networkLocation = null;
			IDomainProvider idp = GetDomainProvider( collection.Domain );
			if ( idp != null )
			{
				// See if the provider already knows about this collection.
				networkLocation = idp.ResolveLocation( collection.Domain, collection.ID );
				if ( ( networkLocation == null ) && !collection.IsHosted )
				{
					// This is a new collection, resolve the location that it should be created.
					networkLocation = idp.ResolveLocation( collection.Domain, collection.Owner.UserID, collection.ID );
				}
			}

			return networkLocation;
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// domain.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		static public Uri ResolveLocation( string domainID )
		{
			Uri networkLocation = null;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				// See if the provider already knows about this collection.
				networkLocation = idp.ResolveLocation( domainID );
			}

			return networkLocation;
		}

		/// <summary>
		/// Returns the network location of where to the specified user's POBox is located.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will owns the POBox.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		static public Uri ResolvePOBoxLocation( string domainID, string userID )
		{
			Uri networkLocation = null;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				networkLocation = idp.ResolvePOBoxLocation( domainID, userID );
			}

			return networkLocation;
		}

		/// <summary>
		/// Returns the network location of the specified host.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The host to resolve.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		static public Uri ResolveHostAddress( string domainID, string hostID )
		{
			Uri networkLocation = null;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				networkLocation = idp.ResolveHostAddress( domainID, hostID );
			}

			return networkLocation;
		}
		
		/// <summary>
		/// Unregisters this domain provider from the domain provider service.
		/// </summary>
		/// <param name="provider">Domain provider to unregister.</param>
		static public void Unregister( IDomainProvider provider )
		{
			lock ( typeof ( DomainProvider ) )
			{
				log.Debug( "Unregistering domain provider {0}.", provider.Name );

				// Remove the domain provider from the list.
				registeredProviders.Remove( provider.Name );

				// Remove all domain mappings for this provider.
				string[] domainList = new string[ domainProviderTable.Count ];
				domainProviderTable.Keys.CopyTo( domainList, 0 );
				foreach( string domainID in domainList )
				{
					// Is this mapping for the specified provider?
					if ( domainProviderTable[ domainID ] as string == provider.Name )
					{
						domainProviderTable.Remove( domainID );
					}
				}
			}
		}

		/// <summary>
		/// Sets a new host address for the domain.
		/// </summary>
		/// <param name="domainID">Identifier of the domain for network address
		/// to be changed.</param>
		/// <param name="hostLocation">A Uri object containing the new network
		/// address for the domain.</param>
		static public void SetHostLocation( string domainID, Uri hostLocation )
		{
			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				idp.SetHostLocation( domainID, hostLocation );
			}
		}
		#endregion
	}
}
