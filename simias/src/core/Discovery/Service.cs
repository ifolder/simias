

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
		private Store store;
		
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
			lock ( typeof( DiscoveryService) )
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

