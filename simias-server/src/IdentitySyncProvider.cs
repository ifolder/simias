/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Web;

using Simias.Client;
using Simias.Storage;

namespace Simias
{
	/// <summary>
	/// Class that implements the domain provider functionality.
	/// </summary>
	public class IdentityProvider
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Table used to keep track of provider mappings.
		/// </summary>
		static private Hashtable providerTable = new Hashtable();

		/// <summary>
		/// List that holds the registered providers.
		/// </summary>
		static private Hashtable registeredProviders = new Hashtable();
		
		static AutoResetEvent syncEvent = null;
		static bool running = false;
		static bool quit;
		static bool syncOnStart = true;
		static int syncInterval = 60 * 1000;
		static Thread syncThread = null;
		
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
		/// Returns the registered identity providers.
		/// </summary>
		static public IIdentitySyncProvider[] Providers
		{
			get
			{
				IIdentitySyncProvider[] providers = new IIdentitySyncProvider[ registeredProviders.Count ];
				lock ( typeof( IdentityProvider ) )
				{
					registeredProviders.CopyTo( providers, 0 );
				}
				return providers;
			}
		}
		#endregion

		#region Private Methods
		
		/*
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
		*/
		#endregion

		#region Public Methods
		/*
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
		*/
		
		/// <summary>
		/// Force a synchronization cycle immediately
		/// </summary>
		public static int SyncNow( string data )
		{
			log.Debug( "SyncNow called" );
			if ( running == false )
			{
				log.Debug( "  synchronization service not running" );
				return -1;
			}
			
			syncEvent.Set();
			log.Debug( "SyncNow finished" );
			return 0;
		}
		
		/// <summary>
		/// Starts the external identity sync container
		/// </summary>
		/// <returns>N/A</returns>
		static public void StartSyncService( )
		{
			if ( running == true )
			{
				log.Debug( "Identity sync service is already running" );
				return;
			}
			
			log.Debug( "StartSyncService - called" );
			quit = false;
			syncEvent = new AutoResetEvent( false );
			syncThread = new Thread( new ThreadStart( SyncThread ) );
			syncThread.IsBackground = true;
			syncThread.Start();
		}

		/// <summary>
		/// Stops the external identity sync container
		/// </summary>
		/// <returns>N/A</returns>
		static public void StopSyncService( )
		{
			log.Debug( "StopSyncService called" );
			quit = true;
			try
			{
				syncEvent.Set();
				Thread.Sleep( 32 );
				log.Debug( "StopSyncService finished" );
			}
			catch(Exception e)
			{
				log.Debug( "StopSyncService failed with an exception" );
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
			
			return;
		}
		
		private static void SyncThread()
		{
			log.Debug( "SyncThread - starting" );
			
			while ( quit == false )
			{
				running = true;
				if ( syncOnStart == false )
				{
					syncEvent.WaitOne( syncInterval, false );
					if ( quit == true )
					{
						return;
					}
				}
				
				log.Debug( "Start - syncing identities" );

				// Always wait after the first iteration
				syncOnStart = false;
				//ssDomain.SynchronizeMembers();
				
				log.Debug( "Stop - syncing identities" );
			}
			
			syncEvent.Close();
			syncThread = null;
			running = false;
		}
		
		#endregion
	}
	
	
}
