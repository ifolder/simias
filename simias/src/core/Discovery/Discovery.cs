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
		/// Table used for quick lookup of collection information.
		/// </summary>
		private Hashtable subTable = new Hashtable();

		/// <summary>
		/// Tells the Collection List thread to exit.
		/// </summary>
		private bool killThread = false;

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
					ListItem CollectionList = GetCollectionListItem(out waitTime);
					if(CollectionList == null)
					{
						// Wait for an item to be placed.
						listEvent.WaitOne( waitTime, true );
					}
				}
				catch( Exception e )
				{
					log.Debug( e, "Exception in CollectionList thread - Ignored : {0}" ,e.ToString () );
					Thread.Sleep( 10 * 1000 );
				}
			}
		}

		private ListItem GetCollectionListItem(out int waitTime)
		{

			ListItem lItem = null;

//TODO : Use proper waittime instead of hardcoding.

//			waitTime = Timeout.Infinite;
			waitTime = 400;

			try
			{
				
				lock (typeof(CollectionList))
				{
					int nextProcessTime = Int32.MaxValue;

					Store localStore = Store.GetStore();
					ArrayList CollectionArray;
					ArrayList CollectionIDArray;
					ICSList domainList = localStore.GetDomainList();
				    
					foreach (ShallowNode sn in domainList)
					{

                                                Domain domain = localStore.GetDomain ( sn.ID );
                                                HostNode masterNode = HostNode.GetMaster ( sn.ID );
						try {
						    Member cmember = domain.GetCurrentMember();

						    SimiasConnection smConn = new SimiasConnection(sn.ID, cmember.UserID,
												   SimiasConnection.AuthType.PPK,
												   masterNode);

						    DiscoveryService dService = new DiscoveryService();
						    dService.Url = masterNode.PrivateUrl;

						    smConn.Authenticate ();
						    smConn.InitializeWebClient(dService, "DiscoveryService.asmx");

						    CollectionIDArray = new ArrayList(dService.GetAllCollectionIDsByUser(cmember.UserID));
						    CollectionArray = new ArrayList(dService.GetAllCollectionsByUser(cmember.UserID));

						} catch (Exception e) {

						    // Skips the local domain as there is no DiscoveryWS.
						    // If DiscoveryWs is N/A  or not running , we skip.
						    log.Debug ("GetCollectionList : Skipping Domain : {0} ID : {1} Trace : {2}", 
							       domain.Name, domain.ID, e.ToString());
						    continue;
						}

						lItem = new ListItem(CollectionIDArray, sn.Name);
						    
						lItem.ProcessTime = DateTime.Now + TimeSpan.FromSeconds( 10 );
						    //need to rework this area.
						collectionList = CollectionArray;    
//						AddCollection( lItem );
					}
//					waitTime = nextProcessTime;
                        //BUG : Hardcoded value.
		        waitTime = 400;
				}
			}
			catch(Exception ex)
			{
				log.Error( "Final Exception : " + ex.ToString());
			}
			return lItem;
		}
		
		private bool AddCollection(ListItem lItem)
		{
			bool exists = true;

			collectionList.Add( lItem );
			exists = false;
			log.Debug( "Added Collection.");
			listEvent.Set();

			return exists;
		}
		
		/// <summary>
		/// Stops the subscription service thread.
		/// </summary>
		public void Stop()
		{
			lock( typeof( CollectionList ) )
			{
				collectionList.Clear();
				subTable.Clear();
			}

			killThread = true;
			listEvent.Set();
			log.Debug( "CollectionList service stopped." );
		}

	}

	public class ListItem
	{
		private ArrayList sharedCollection;
		private DateTime processTime;
		private String domName;
 

		/// <summary>
		/// Gets the SharedCollection associated with this instance.
		/// </summary>
		public ArrayList SharedCollection
		{
			get{ return sharedCollection; }
		}

		/// <summary>
		/// Gets or set the wait time before processing this item.
		/// </summary>
		public DateTime ProcessTime
		{
			get { return processTime; }
			set { processTime = value; }
		}

 		
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="collectionList">The CollectionList associated with this sharedCollection.</param>
		/// <param name="sharedCollection">The Collection to be display.</param>
		public ListItem(ArrayList shColl, String domainName)
		{
			this.sharedCollection = shColl;
			this.domName = domainName;
			this.processTime = DateTime.Now;
		}
	}
}

