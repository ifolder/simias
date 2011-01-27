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
*                 $Author: Kalidas Balakrishnan <bkalidas@novell.com>
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

	/// <summary>
	///
	/// </summary>	
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
		public AutoResetEvent listEvent = new AutoResetEvent( false );

		/// <summary>
		/// The default process cycle time for the shared collection.
		/// </summary>
		private const int defaultWait = 360 * 1000; // 5 mins

		/// <summary>
		/// The default pre Authentication cycle time for the shared collection.
		/// </summary>
		private const int preAuthTime = 5 * 1000; // 5 secs

		/// <summary>
		/// Atleast one domain should be processed for shared collection display.
		/// </summary>
		private bool processedOne = false;
		/// <summary>
		///
		/// </summary>	
		public bool processed = false;


		/// <summary>
		/// Tells the Collection List thread to exit.
		/// </summary>
		private bool killThread = false;

		//TODO - we need to ensure collectionList has latest data when a Refresh is triggered
		// now the collectionList gets populated every 60 secs - even this should be configurable from user end.
		// we need to expose a function that will trigger the listEvent, so that the timer expires and the list is generated.
		/// <summary>
		///
		/// </summary>	
		/// <returns></returns>
		static public ArrayList GetCollectionList()
		{
			lock(collectionList)
			{
				return collectionList;
			}
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
			thread.Name = "Discovery";
			thread.Start();
		}

        /// <summary>
        /// Thread method:Run
        /// </summary>
		private void Run()
		{
			while ( !( Simias.Service.Manager.ShuttingDown || killThread ) )
			{
				int waitTime;
				try
				{
					GetCollectionListItem(out waitTime);
					processed = true;
					//Wait unconditionally
					// Wait for next cycle.
					listEvent.WaitOne( waitTime, true );
					processedOne = false;
					processed = false;
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

	        //johnny : can we hook this up with the sync interval ?
        /// <summary>
        /// Get the collection list item
        /// </summary>
        /// <param name="waitTime">Reference: wait time set</param>
		private void GetCollectionListItem(out int waitTime)
		{
			waitTime = preAuthTime;

			try
			{
			        lock (collectionList)
				{

					Store localStore = Store.GetStore();
					ArrayList CollectionArray;
					CatalogInfo[] CatalogInfoArray;

					collectionList.Clear();

					ICSList domainList = localStore.GetDomainList();
				    
					foreach (ShallowNode sn in domainList)
					{

						Domain domain = localStore.GetDomain ( sn.ID );
						// skip local domain
						if(domain.Name.Equals(Store.LocalDomainName))
							continue;

                        DomainAgent da = new DomainAgent();
                        if (da.IsDomainRemoved(domain.ID))
                            continue;

						Member cmember = domain.GetCurrentMember();
						HostNode masterNode = cmember.HomeServer;
                        if (masterNode == null)
                            masterNode = domain.Host;

						try {
							log.Debug("GetCollectionList - Try ");
								
							SimiasConnection smConn = new SimiasConnection(sn.ID, cmember.UserID,
												   SimiasConnection.AuthType.BASIC,
												   masterNode);
								
							DiscoveryService dService = new DiscoveryService();
							DomainAgent dAgent = new DomainAgent();
							bool activeDomain = dAgent.IsDomainActive(domain.ID);
							bool authenticatedDomain = dAgent.IsDomainAuthenticated(domain.ID); 

							if(activeDomain)
							{
								if(authenticatedDomain)
								{
									//not even one is processed, then this will get processed immediately
									// and change the cycle time down
									if(!processedOne)
										processedOne = true;
									else
									{
									// we have processed atleast one, so cycle time will get set
									}
								}
								else
								{
								//	new EventPublisher().RaiseEvent( new NeedCredentialsEventArgs( domain.ID) );
									continue;
								}
							}
							else
							{
								continue;
							}

							dService.Url = masterNode.PrivateUrl;
								
							smConn.Authenticate ();
							smConn.InitializeWebClient(dService, "DiscoveryService.asmx");
								
							CatalogInfoArray = dService.GetAllCatalogInfoForUser (cmember.UserID);

						        CollectionArray = new ArrayList ( Simias.Discovery.DiscoveryFramework.GetDetailedCollectionInformation (sn.ID, CatalogInfoArray, dService));
							log.Info ("CatalogInfoArray : {1} | CollectionArray : {0}", CollectionArray.Count, CatalogInfoArray.Length);

							//TODO : Test this section for MultiDomain. Check for performance issues.
						}
						catch (Exception e) 
						{
							// If DiscoveryWs is N/A  or not running , we skip.
							log.Debug ("GetCollectionList : Skipping Domain : {0} ID : {1} Trace : {2}", 
							domain.Name, domain.ID, e.ToString());
							continue;
						}
						//Get Information from all the Domain and make it available for consumption
						// Add elemetn here . instead of assiging.

						collectionList.AddRange (CollectionArray);
		                                log.Debug ("collectionList : {0}",collectionList.Count);
					}
				}
				// next wait cycle
				// we might need to get this from a preference setting -- TODO
				// need to see if we need to dynamically increase the preAuthTime based on the active domain count
				if(processedOne)
					waitTime = defaultWait; 

				log.Debug("waittime set to {0} ms", waitTime);
			}
			catch(Exception ex)
			{
				log.Error( "Final Exception : " + ex.ToString());
			}
listEvent.Reset();
			return;
		}

        /// <summary>
        /// Connect to user home server and get the latest info about this collection
        /// </summary>
        public static CatalogInfo GetCatalogInfoItem(string CollectionID, string UserID, string DomainID)
        {

            CatalogInfo catInfo = null;
            try
            {
                //lock (collectionList)
                {

                    Store localStore = Store.GetStore();
                    //ArrayList CollectionArray;

                    Domain domain = localStore.GetDomain(DomainID);
                    // skip local domain
                    if (!domain.Name.Equals(Store.LocalDomainName))
                    {
                        DomainAgent da = new DomainAgent();
                        if (!da.IsDomainRemoved(domain.ID))
                        {

                            Member cmember = domain.GetCurrentMember();
                            if (cmember == null)
                            {
                                log.Debug("CetCollectionInfoItem : Member is null in local store");
                                throw new Exception("Member NULL");
                            }
                            HostNode masterNode = cmember.HomeServer; //HostNode.GetMaster(DomainID);//cmember.HomeServer;

                            try
                            {
                                log.Debug("GetCollectionInfoItem - Try ");

                                SimiasConnection smConn = new SimiasConnection(DomainID, cmember.UserID,
                                                       SimiasConnection.AuthType.BASIC,
                                                       masterNode);

                                DiscoveryService dService = new DiscoveryService();
                                DomainAgent dAgent = new DomainAgent();
                                bool activeDomain = dAgent.IsDomainActive(domain.ID);
                                bool authenticatedDomain = dAgent.IsDomainAuthenticated(domain.ID);

                                if (activeDomain)
                                {
                                    if (!authenticatedDomain)
                                    {
                                        new EventPublisher().RaiseEvent(new NeedCredentialsEventArgs(domain.ID));
                                        throw new Exception("Domain Not Authenticated");
                                    }
                                    dService.Url = masterNode.PrivateUrl;

                                    smConn.Authenticate();
                                    smConn.InitializeWebClient(dService, "DiscoveryService.asmx");

                                    catInfo = dService.GetCatalogInfoForCollection(CollectionID);
                                }
                            }
                            catch (Exception e)
                            {
                                // If DiscoveryWs is N/A  or not running , we skip.
                                log.Debug("GetCatalogInfoItem : Skipping Domain : {0} ID : {1} Trace : {2}",
                                domain.Name, domain.ID, e.ToString());
                                throw e;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error("Final Exception : " + ex.ToString());
                throw ex;
            }
            return catInfo;
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

