/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 | Author: Russ Young
 |***************************************************************************/


using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Simias;
using Simias.Event;
using Simias.Client.Event;


namespace Simias.Sync
{
	#region Delegate Definitions.

	/// <summary>
	/// Delegate definition for handling file events.
	/// </summary>
	public delegate void FileSyncEventHandler(FileSyncEventArgs args);

	/// <summary>
	/// Used to get around a marshalling problem seen with explorer.
	/// </summary>
	public delegate void CollectionSyncEventHandler(CollectionSyncEventArgs args);

	
	#endregion
	
	/// <summary>
	/// Class to Subscibe to collection events.
	/// </summary>
	public class SyncEventSubscriber : IDisposable
	{
		#region Events

		/// <summary>
		/// Event to recieve collection sync event.
		/// </summary>
		public event CollectionSyncEventHandler CollectionSync;
		/// <summary>
		/// Event to handle File sync events.
		/// </summary>
		public event FileSyncEventHandler FileSync;
		
		#endregion

		#region Private Fields

		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(SyncEventSubscriber));
		DefaultSubscriber	subscriber = null;
		bool		enabled;
		bool		alreadyDisposed;
		
		#endregion

		#region Constructor/Finalizer

		/// <summary>
		/// Creates a Subscriber to watch for sync events.
		/// </summary>
		public SyncEventSubscriber()
		{
			enabled = true;
			alreadyDisposed = false;
			subscriber = new DefaultSubscriber();
			subscriber.SimiasEvent += new SimiasEventHandler(OnSyncEvent);
		}

		
		/// <summary>
		/// Finalizer.
		/// </summary>
		~SyncEventSubscriber()
		{
			Dispose(true);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets and set the enabled state.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				enabled = value;
			}
		}

		#endregion

		#region Callbacks

		private void OnSyncEvent(SimiasEventArgs args)
		{
			try
			{
				if (enabled)
				{
					CollectionSyncEventArgs cArgs = args as CollectionSyncEventArgs;
					if (cArgs != null)
					{
						if (CollectionSync != null)
						{
							Delegate[] cbList = CollectionSync.GetInvocationList();
							foreach (CollectionSyncEventHandler cb in cbList)
							{
								try 
								{ 
									cb(cArgs);
								}
								catch(Exception ex)
								{
									logger.Debug(ex, "Delegate {0}.{1} failed", cb.Target, cb.Method);
									CollectionSync -= cb;
								}
							}
						}
					}
					else
					{
						FileSyncEventArgs fArgs = args as FileSyncEventArgs;
						if (fArgs != null)
						{
							if (FileSync != null)
							{
								Delegate[] cbList = FileSync.GetInvocationList();
								foreach (FileSyncEventHandler cb in cbList)
								{
									try 
									{ 
										cb(fArgs);
									}
									catch(Exception ex)
									{
										logger.Debug(ex, "Delegate {0}.{1} failed", cb.Target, cb.Method);
										FileSync -= cb;
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				new SimiasException(args.ToString(), ex);
			}
		}

		#endregion

		#region Private Methods

		private void Dispose(bool inFinalize)
		{
			try 
			{
				if (!alreadyDisposed)
				{
					alreadyDisposed = true;
					
					// Deregister delegates.
					subscriber.SimiasEvent -= new SimiasEventHandler(OnSyncEvent);
					subscriber.Dispose();
					if (!inFinalize)
					{
						GC.SuppressFinalize(this);
					}
				}
			}
			catch {};
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Called to cleanup any resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}
}
