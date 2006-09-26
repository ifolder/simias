

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
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(CollectionList));
		/// <summary>
		/// List used to hold shared Collection for processing.
		/// </summary>
		private static internal IList collectionList = new IList();

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

		public CollectionList GetCollectionList()
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
					ListItem CollectionList = GetCollectionList(out waitTime);

					if(CollectionList == null)
					{
						// Wait for an item to be placed.
						listEvent.WaitOne( waitTime, true );
					}
				}
				catch( Exception e )
				{
					log.Debug( e, "Exception in CollectionList thread - Ignored" );
					Thread.Sleep( 10 * 1000 );
				}
			}
		}

		private ListItem GetCollectionList(out int waitTime)
		{
			waitTime = Timeout.Infinite;
			DiscoveryService dService = new DiscoveryService();
			// how to resolve the Url here to access the web service
			lock (typeof(CollectionList))
			{
				int nextProcessTime = Int32.MaxValue;
				Store locStore = Simias.Discovery.DiscoveryService.store;
				ICSList domList = locStore.GetDomainList();
				foreach (Domain domain in domList)
				{
					Member member = domain.GetCurrentMember();
					ArrayList CollectionArray = dService.GetAllCollectionIDsByUser(member.UserID);
					ListItem lItem = new ListItem(CollectionArray);
					lItem.ProcessTime = DateTime.Now + TimeSpan.FromSeconds( 10 );
					AddCollection( lItem );
				}
				waitTime = nextProcessTime;
			}
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
		public ListItem(ArayList shColl)
		{
			this.sharedCollection = shColl;
			this.processTime = DateTime.Now;
		}
	}
}

