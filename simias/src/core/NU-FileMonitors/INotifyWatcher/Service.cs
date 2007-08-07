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
 |  Author: banderso@novell.com
 |***************************************************************************/
 

using System;
using System.Diagnostics;

using Simias.Service;

namespace Simias.FileMonitor.INotifyWatcher
{
	/// <summary>
	/// Service - INotifyWatcher Service startup/shutdown etc.
	/// </summary>
	public class Service : IThreadService
	{
		private Simias.FileMonitor.INotifyWatcher.Manager manager;

		/// <summary>
		/// Constructor
		/// </summary>
		public Service()
		{
		}

		#region BaseThreadService Members

		/// <summary>
		/// Start the INotify Watcher service.
		/// </summary>
		public void Start(Configuration config)
		{
			this.manager = new Simias.FileMonitor.INotifyWatcher.Manager(config);
			this.manager.Start();
		}

		/// <summary>
		/// Stop the PO service.
		/// </summary>
		public void Stop()
		{
			Debug.Assert(this.manager != null);
			if (this.manager != null)
			{
				this.manager.Stop();
			}
		}

		/// <summary>
		/// Resume the INotifyWatcher service.
		/// </summary>
		public void Resume()
		{
			Debug.Assert(this.manager != null);
			if (this.manager != null)
			{
				this.manager.Start();
			}
		}

		/// <summary>
		/// Pause the INotifyWatcher service.
		/// In our case we'll just stop the service
		/// </summary>
		public void Pause()
		{
			Debug.Assert(this.manager != null);
			if (this.manager != null)
			{
				this.manager.Stop();
			}
		}

		/// <summary>
		/// Custom service method.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public void Custom(int message, string data)
		{
			switch (message)
			{
				default:
					break;
			}
		}

		#endregion
	}
}
