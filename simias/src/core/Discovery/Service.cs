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
		public static void UpdateCollectionList()
		{
			collectionList.listEvent.Set();
			collectionList.processed = false;
			while(!collectionList.processed)
			{
				Thread.Sleep(0);
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

