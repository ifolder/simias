/***********************************************************************
 *  $RCSfile: ThreadService.cs,v $
 *
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Threading;
using System.Text;

using Simias;
using Simias.Service;

namespace Simias.MdbSync
{
	/// <summary>
	/// Summary description for MdbThreadService
	/// </summary>
	public class ThreadService : IThreadService
	{
        private Simias.MdbProvider.SyncProvider syncProvider = null;


		/// <summary>
		/// Thread service start up.
		/// </summary>
		public void Start()
		{
            // Register with the server external sync service.
            if ( syncProvider == null )
            {
                syncProvider = new Simias.MdbProvider.SyncProvider();
                Simias.IdentitySync.Service.Register( syncProvider );
            }

			//Simias.MdbProvider.SyncThread.Start();
		}

		/// <summary>
		/// Resume the service.
		/// </summary>
		public void Resume()
		{
			// Not supported
		}

		/// <summary>
		/// Pause the service.
		/// </summary>
		public void Pause()
		{
			// Not supported
		}

		/// <summary>
		/// Custom service method.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public int Custom(int message, string data)
		{
			int status = 0;
			
			/*
			switch ((CustomCode) message)
			{
				case CustomCode.SyncImmediate:
					LdapSync.SyncImmediate( data );
					break;

				case CustomCode.SyncStatus:
					status = (int) Novell.AddressBook.LdapSync.LdapSync.syncStatus;
					break;

				default:
				break;
			}
			*/

			return status;
		}

		/// <summary>
		/// Stop the service.
		/// </summary>
		public void Stop()
		{
            if ( syncProvider != null )
            {
                Simias.IdentitySync.Service.Unregister( syncProvider );
                syncProvider = null;
            }
			//Simias.MdbProvider.SyncThread.Stop();
		}

		/// <summary>
		/// 
		/// </summary>
		public Status GetServiceStatus()
		{
			return new Status();
		}
	}
}
