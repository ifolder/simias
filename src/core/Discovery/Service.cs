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


namespace Simias.Discovery
{
	/// <summary>
	/// Class the handles presence as a service
	/// </summary>
	public class DiscService : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( DiscoveryService ) );

		/// <summary>
		/// Object used to Get the CollectionList from the server.
		/// </summary>
		private static CollectionList collectionList = null;


		/// <summary>
		/// Store object.
		/// </summary>
		internal static Store store;
		
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public DiscService()
		{
			store = Store.GetStore();
		}
		#endregion

		#region IThreadService Members
        /// <summary>
        /// Update the collection list every 5 minutes
        /// </summary>
		public static void UpdateCollectionList()
		{
			collectionList.processed = false;
			collectionList.listEvent.Set();
			while(!collectionList.processed)
			{
				Thread.Sleep(300);
			}
			return;
		}
		
		/// <summary>
		/// Starts the thread service.
		/// </summary>
		public void Start()
		{
			log.Debug( "Start called for Discovery" );

			lock ( typeof( DiscoveryService) )
			{
				if ( collectionList == null )
				{
					// Start the CollectionList thread.
					collectionList = new CollectionList();
				}
			}
			// Get a list of all Collections.
				// Process any active Collection.
		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public int Custom(int message, string data)
		{
			return 0;
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			lock ( typeof( DiscService) )
			{
				if ( collectionList != null )
				{
					// Stop the CollectionList thread.
					collectionList.Stop();
					collectionList = null;
				}
			}
			log.Debug( "Stop called for Discovery" );

		}
		#endregion
	}
}

