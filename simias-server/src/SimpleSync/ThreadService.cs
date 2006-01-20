/***********************************************************************
 *  $RCSfile$
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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

using Simias;
using Simias.Service;
using Simias.Storage;

namespace Simias.SimpleServer
{
	/// <summary>
	/// Thread service class for registering
	/// sync and authentication providers
	/// </summary>
	public class Service : IThreadService
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			
		Simias.SimpleServer.SyncProvider syncProvider = null;
		Simias.SimpleServer.Authentication authProvider = null;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public Service()
		{
		}
		#endregion

		#region IThreadService Members
		/// <summary>
		/// Starts the thread service.
		/// </summary>
		/// <param name="config">
		/// Configuration file object for the configured store 
		/// Store to use.
		/// </param>
		public void Start()
		{
			log.Debug( "Start called" );
			//this.config = config;
			
			// Instantiate the simple server domain
			// The domain will be created the first time the
			// server is run
			new Simias.SimpleServer.Domain( true );

			// Register with the domain provider service.
			if ( authProvider == null )
			{
				authProvider = new Simias.SimpleServer.Authentication();
				DomainProvider.RegisterProvider( this.authProvider );
			}
			
			if ( syncProvider == null )
			{
				syncProvider = new Simias.SimpleServer.SyncProvider();
				Simias.IdentitySync.Service.Register( syncProvider );
			}
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
			log.Debug( "Stop called" );
			
			if ( syncProvider != null )
			{
				Simias.IdentitySync.Service.Unregister( syncProvider );
				syncProvider = null;
			}
			
			if ( authProvider != null )
			{
				DomainProvider.Unregister( authProvider );
				authProvider = null;
			}
		}
		#endregion
	}
}
