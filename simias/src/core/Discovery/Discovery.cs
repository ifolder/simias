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
 *  Author: Kalidas Balakrishnan <bkalidas@novell.com>
 *
 ***********************************************************************/


using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

using Simias;
using Simias.Event;
using Simias.POBox;
using Simias.Service;
using Simias.Storage;
using Simias.DomainServices;
using Simias.Client.Event;

namespace Simias.Discovery
{

	public class CollectionList
	{
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
		/// <summary>
		/// List used to hold shared Collection for processing.
		/// </summary>
		internal static ArrayList collectionList = new ArrayList();

		/// <summary>
		/// Event used to signal thread that items have been placed on the queue.
		/// </summary>
		private AutoResetEvent listEvent = new AutoResetEvent( false );

		/// <summary>
		/// The default process cycle time for the shared collection.
		/// </summary>
		private const int defaultWait = 60 * 1000; // 60 secs

		/// <summary>
		/// The default pre Authentication cycle time for the shared collection.
		/// </summary>
		private const int preAuthTime = 5 * 1000; // 5 secs

		/// <summary>
		/// Atleast one domain should be processed for shared collection display.
		/// </summary>
		private bool processedOne = false;

		/// <summary>
		/// Tells the Collection List thread to exit.
		/// </summary>
		private bool killThread = false;

		//TODO - we need to ensure collectionList has latest data when a Refresh is triggered
		// now the collectionList gets populated every 60 secs - even this should be configurable from user end.
		// we need to expose a function that will trigger the listEvent, so that the timer expires and the list is generated.
		static public ArrayList GetCollectionList()
		{
			return collectionList;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public CollectionList()
		{
			// Start the Collection List thread running.
			Thread thread = new Thread( new ThreadStart( Run ) );
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}
		
		private void Run()
		{
			while ( !( Simias.Service.Manager.ShuttingDown || killThread ) )
			{
				int waitTime;
				try
				{
					GetCollectionListItem(out waitTime);
					//Wait unconditionally
					// Wait for next cycle.
					listEvent.WaitOne( waitTime, true );
					processedOne = false;
				}
				catch( Exception e )
				{
					log.Debug( e, "Exception in CollectionList thread - Ignored : {0}" ,e.ToString () );
					Thread.Sleep( 10 * 1000 );
				}
			}
		}

		//No time processing is done, we will default to 1 min cycle. We might need to get this info from Preference or Web Access page.
		// On a right-click refresh we need to signal the thread  
		private void GetCollectionListItem(out int waitTime)
		{
			waitTime = Timeout.Infinite;

			try
			{
				lock (typeof(CollectionList))
				{

					Store localStore = Store.GetStore();
					ArrayList CollectionArray;
					ArrayList CollectionIDArray;
					ICSList domainList = localStore.GetDomainList();
				    
					foreach (ShallowNode sn in domainList)
					{

						Domain domain = localStore.GetDomain ( sn.ID );
						// skip local domain
						if(domain.Name.Equals(Store.LocalDomainName))
							continue;
						HostNode masterNode = HostNode.GetMaster ( sn.ID );

						try {
							log.Debug("GetCollectionList - Try");
							Member cmember = domain.GetCurrentMember();
								
							SimiasConnection smConn = new SimiasConnection(sn.ID, cmember.UserID,
												   SimiasConnection.AuthType.BASIC,
												   masterNode);
								
							DiscoveryService dService = new DiscoveryService();
							DomainAgent dAgent = new DomainAgent();
							bool activeDomain = dAgent.IsDomainActive(domain.ID);
							bool authenticatedDomain = dAgent.IsDomainAuthenticated(domain.ID); 

							if(activeDomain && (authenticatedDomain && !processedOne))
								processedOne = true;
							else 
								//raise the credential event only if active and not authenticated 
								if(activeDomain && !authenticatedDomain) 
									new EventPublisher().RaiseEvent( new NeedCredentialsEventArgs( domain.ID) );

							dService.Url = masterNode.PrivateUrl;
								
							smConn.Authenticate ();
							smConn.InitializeWebClient(dService, "DiscoveryService.asmx");
								
							CollectionIDArray = new ArrayList(dService.GetAllCollectionIDsByUser(cmember.UserID));
							CollectionArray = new ArrayList(dService.GetAllCollectionsByUser(cmember.UserID));
						}
						catch (Exception e) 
						{
							// If DiscoveryWs is N/A  or not running , we skip.
							log.Debug ("GetCollectionList : Skipping Domain : {0} ID : {1} Trace : {2}", 
							domain.Name, domain.ID, e.ToString());
							continue;
						}
						//need to rework this area.
						collectionList = CollectionArray;    
					}

				}
				// next wait cycle
				// we might need to get this from a preference setting -- TODO
				if(processedOne)
					waitTime = defaultWait; 
				else
					waitTime = preAuthTime;

				log.Debug("waittime set to {0} ms", waitTime);
			}
			catch(Exception ex)
			{
				log.Error( "Final Exception : " + ex.ToString());
			}
			return;
		}
		
					
		/// <summary>
		/// Stops the Collection list  thread.
		/// </summary>
		public void Stop()
		{
			lock( typeof( CollectionList ) )
			{
				collectionList.Clear();
			}

			killThread = true;
			listEvent.Set();
			log.Debug( "CollectionList service stopped." );
		}

	}
}

