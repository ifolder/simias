/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author: banderso@novell.com
 *
 ***********************************************************************/

using System;
using System.Diagnostics;

using Simias.Service;

namespace Simias.SystemMonitor
{
	/// <summary>
	/// Service
	/// </summary>
	public class Service : IThreadService
	{
		private Simias.SystemMonitor.Manager manager;

		/// <summary>
		/// Constructor
		/// </summary>
		public Service()
		{
            Console.WriteLine("Simias.SystemMonitor.Service constructor");
		}

		#region BaseProcessService Members

		/// <summary>
		/// Start the System Monitor service.
		/// </summary>
		public void Start(Configuration config)
		{
            Console.WriteLine("Simias.SystemMonitor.Service - Start called");
			this.manager = new Simias.SystemMonitor.Manager(config);
			this.manager.Start();
		}

		/// <summary>
		/// Stop the service.
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
		/// Resume the service.
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
		/// Pause the service.
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