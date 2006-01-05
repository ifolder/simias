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
//
// Source file contains entry points to stop and start
// the responder
//
using System;
using System.Threading;
//using System.Net;
//using System.Net.Sockets;
using System.Text;

/*
using log4net;
using log4net.Config;
using log4net.Appender;
using log4net.Repository;
using log4net.spi;
using log4net.Layout;
using Simias;
using Simias.Service;
*/

namespace Simias.MdbSync
{
	/// <summary>
	/// Summary description for MdbThreadService
	/// </summary>
	public class MdbThreadService : IThreadService
	{
		/*
		public struct Status
		{
			public bool SyncOnStart
			{
				get
				{
					try
					{
						return (LdapSync.ldapSettings.SyncOnStart); 
					}
					catch
					{
						return false;
					}
				}
			}

			public bool ErrorDuringSync
			{
				get { return (LdapSync.errorDuringSync); }
			}

			public Novell.AddressBook.LdapSync.Status CurrentStatus
			{
				get { return ( LdapSync.syncStatus ); }
			}

			public Exception SyncException
			{
				get { return (LdapSync.syncException); }
			}

			public DateTime LastSyncTime
			{
				get { return (LdapSync.lastSyncTime); }
			}
		}
		*/

		#region CustomCode enum
		/// <summary>
		/// Defines the valid messages for a Service.
		/// </summary>
		public enum CustomCode
		{
			/// <summary>
			///
			/// </summary>
			SyncImmediate = 1,

			/// <summary>
			///
			/// </summary>
			SyncStatus = 2
		};
		#endregion

		/// <summary>
		/// Thread service start up.
		/// </summary>
		public void Start()
		{
			MdbSync.StartMdbSyncThread();
		}

		/// <summary>
		/// Resume the service.
		/// </summary>
		public void Resume()
		{
			// Don't support Resume
		}

		/// <summary>
		/// Pause the service.
		/// </summary>
		public void Pause()
		{
			// Don't support Pause
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
			MdbSync.StopMdbSyncThread();
		}

		/// <summary>
		/// 
		/// </summary>
		public Status GetServiceStatus()
		{
			//return new Status();
		}
	}
}
