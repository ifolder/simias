/***********************************************************************
 *  StoreWatcher.cs
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob Lyon <rlyon@novell.com>
 * 
 ***********************************************************************/
using System;
using System.IO;
using System.Collections;
using System.Threading;
using System.Diagnostics;

using Simias;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// A collection has been created event handler.
	/// </summary>
	public delegate void CreatedCollectionEventHandler(string id);
	
	/// <summary>
	/// A collection has been deleted event handler.
	/// </summary>
	public delegate void DeletedCollectionEventHandler(string id);

	/// <summary>
	/// Store Watcher
	/// </summary>
	public class StoreWatcher : IDisposable
	{
		/// <summary>
		/// Occurs when a collection is discovered or created.
		/// </summary>
		public event CreatedCollectionEventHandler Created;

		/// <summary>
		/// Occurs when a collection is deleted.
		/// </summary>
		public event DeletedCollectionEventHandler Deleted;

		private Thread worker;
		private bool working;
		private Hashtable collections;
		
		private string path;
		private Store store;

		private int interval = 5;

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="path">The store path.</param>
		public StoreWatcher(string path)
		{
			if (path != null)
			{
				this.path = Path.GetFullPath(path);
			}

			collections = new Hashtable();

			worker = new Thread(new ThreadStart(this.DoWork));
		}

		/// <summary>
		/// Reset the store connection.
		/// </summary>
		private void Reset()
		{
			if (store != null)
			{
				try
				{
					store.Dispose();
				}
				catch
				{
					// ignore
				}

				store = null;
			}

			// connect
			Uri uri = null;
			if (path != null) uri = new Uri(path);
			store = Store.Connect(uri);
			Trace.Assert(store != null);
			
			// TODO: remove
			//store.ImpersonateUser(Access.StoreAdminRole, null);
		}

		/// <summary>
		/// Start watching the Store.
		/// </summary>
		public void Start()
		{
			lock(this)
			{
				// init the store connection
				Reset();

				// start the thread
				working = true;
				worker.Start();
			}
		}

		/// <summary>
		/// Stop watching the Store.
		/// </summary>
		public void Stop()
		{
			lock(this)
			{
				working = false;
			
				try
				{
					worker.Join();
				}
				catch
				{
					// ignore
				}
			}
		}

		/// <summary>
		/// Look for Collection changes.
		/// </summary>
		private void DoWork()
		{
			while(working)
			{
				try
				{
					MyTrace.WriteLine("Scanning the Store...");

					Hashtable oldCollections = (Hashtable)collections.Clone();

					// find created collections
					foreach(Collection c in store)
					{
						string id = c.Id;
						string name = c.Name;

						if (!collections.Contains(id))
						{
							MyTrace.WriteLine("Created Collection: {0} [{1}]",
								name, id);

							collections.Add(id, name);
						
							if (Created != null)
							{
								Created(id);
							}
						}

						oldCollections.Remove(id);
					}

					// drop deleted collections
					foreach(string id in oldCollections.Keys)
					{
						string name = (string)collections[id];

						MyTrace.WriteLine("Deleted Collection: {0} [{1}]",
							name, id);

						if (Deleted != null)
						{
							Deleted(id);
						}
					}
				}
				catch(Exception e)
				{
					MyTrace.WriteLine(String.Format(
						"Reseting on Exception: {0}", e.Message));

					MyTrace.WriteLine("{0}", e);

					// reset the store connection
					Reset();
				}

				// sleep
				Thread.Sleep(TimeSpan.FromSeconds(interval));
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose this object.
		/// </summary>
		public void Dispose()
		{
			Stop();

			if (store != null)
			{
				store.Dispose();
				store = null;
			}
		}

		#endregion

		/// <summary>
		/// Interval, in seconds, between scans of the store.
		/// </summary>
		public int Interval
		{
			get { return interval; }
			set { interval = value; }
		}
	}
}
