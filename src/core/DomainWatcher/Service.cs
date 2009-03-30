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
*                 $Author: banderso@novell.com 
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
using System.Diagnostics;

using Simias.Service;

namespace Simias.DomainWatcher
{
	/// <summary>
	/// PO Service
	/// </summary>
	public class Service : IThreadService
	{
		private Simias.DomainWatcher.Manager manager;

		/// <summary>
		/// Constructor
		/// </summary>
		public Service()
		{
		}

		#region BaseProcessService Members

		/// <summary>
		/// Start the Domain Watcher service.
		/// </summary>
		public void Start()
		{
			this.manager = new Simias.DomainWatcher.Manager();
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
		/// Really don't support resume/pause - just 
		/// start and stop under the covers
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
		public int Custom(int message, string data)
		{
			switch (message)
			{
				default:
					break;
			}

			return 0;
		}

		#endregion
	}
}
