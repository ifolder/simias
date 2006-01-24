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
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace Simias.IdentitySync
{
	/// <summary>
	/// Class that represents the state of the last
	/// synchronization cycle
	/// </summary>
	[ Serializable ]
	public class LastSyncInfo
	{
		/// <summary>
		/// Date and Time when the last sync cycle started
		/// RFC 822 format
		/// </summary>
		public string	StartTime;

		/// <summary>
		/// Date and Time when the last sync cycle finished
		/// RFC 822 format
		/// </summary>
		public string	EndTime;
		
		/// <summary>
		/// The number of members processed during the cycle
		/// </summary>
		public int		MembersProcessed;

		/// <summary>
		/// Number of members added to the domain during
		/// the cycle
		/// </summary>
		public int		MembersAdded;

		/// <summary>
		/// Number of members deleted from the domain during
		/// the cycle
		/// </summary>
		public int		MembersDeleted;
		
		/// <summary>
		/// Number of members disabled in the domain during
		/// the cycle
		/// </summary>
		public int		MembersDisabled;
		
		/// <summary>
		/// Number of reported errors during the cycle
		/// </summary>
		public int		ReportedErrors;

		/// <summary>
		/// Messages reported during the cycle
		/// </summary>
		public string[]	Messages;
	}
	
	/// <summary>
	/// Class that represents the current state and configuration
	/// of the synchronization service.
	/// </summary>
	[ Serializable ]
	public class ServiceInfo
	{
		/// <summary>
		/// Date and Time when the synchronization engine was
		/// started.
		/// RFC 822 format
		/// </summary>
		public string UpSince;
		
		/// <summary>
		/// Number of cycles the engine performed
		/// </summary>
		public int Cycles;
		
		/// <summary>
		/// Name of the registered synchronization provider
		/// </summary>
		public string	Provider;

		/// <summary>
		/// Configured time interval, represented in seconds,
		/// between synchronization cycles.
		/// </summary>
		public int	SynchronizationInterval;

		/// <summary>
		/// Configured grace period, represented in seconds,
		/// the sync service will allow a member to remain
		/// in the domain when the member no longer exists
		/// in the external identity store.
		///
		/// Members become disabled in the Simias domain when
		/// they are no longer exist in the external store.
		/// </summary>
		public int	DeleteMemberGracePeriod;
		
		/// <summary>
		/// Current status of the synchronization engine
		/// status will be one of the following:
		/// "running"
		/// "sleeping"
		/// "disabled"
		/// "shutdown"
		/// </summary>
		public string Status;
	}
	
	/// <summary>
	/// Identity Sync Manager
	/// Web service methods to manage the Identity Sync Service
	/// </summary>
	[WebService(
	 Namespace="http://novell.com/simias/idsyncmgr",
	 Name="Identity Sync Manager",
	 Description="Web Service providing management for the Identity Sync Service.")]
	public class Manager : System.Web.Services.WebService
	{
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
		/// <summary>
		/// Constructor
		/// </summary>
		public Manager()
		{
		}
		
		private static string[] MonthsOfYear =
		{
			"Jan",
			"Feb",
			"Mar",
			"Apr",
			"Jun",
			"Jul",
			"Aug",
			"Sep",
			"Oct",
			"Nov",
			"Dec"
		};
		
		/// <summary>
		/// Method to disable the synchronization service
		/// true - disables
		/// false - enables the synchronization service
		/// Note! once enabled the service will enter a
		/// synchronization cycle ignoring the configured
		/// sync interval time.
		/// </summary>
		/// 
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public void DisableSyncService( bool Disable )
		{
			IdentitySync.Service.syncDisabled = Disable;
		}

		/// <summary>
		/// Tells the sync service to immediately start
		/// a synchronization cycle.
		/// </summary>
		/// 
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public void SyncNow()
		{
			IdentitySync.Service.SyncNow( "" );
		}

		/// <summary>
		/// Get the current status of the identity sync service thread
		/// status could be:
		/// Disabled
		/// Working
		/// Waiting
		/// Authentication Failure
		/// etc..
		/// </summary>
		/// 
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public ServiceInfo GetSyncServiceInfo()
		{
			ServiceInfo info = new ServiceInfo();
			
			info.UpSince = 			
				String.Format(
					"{0}, {1} {2} {3} {4}:{5}:{6} GMT",
					Service.upSince.DayOfWeek.ToString(),
					Service.upSince.Day,
					Simias.IdentitySync.Manager.MonthsOfYear[ Service.upSince.Month - 1 ],
					Service.upSince.Year.ToString(),
					Service.upSince.Hour,
					Service.upSince.Minute,
					Service.upSince.Second );
			
			info.Cycles = IdentitySync.Service.cycles;
			
			// Get the first provider.   At the moment, sync engine only supports
			// one provider anyway.
			foreach( IIdentitySyncProvider prov in Service.registeredProviders.Values )
			{
				info.Provider = prov.Name;
				break;
			}
			
			info.DeleteMemberGracePeriod = IdentitySync.Service.deleteGracePeriod;
			info.SynchronizationInterval = IdentitySync.Service.syncInterval;
			info.Status = IdentitySync.Service.status;
			
			return info;
		}
		
		/// <summary>
		/// Get detailed information about the last synchronization 
		/// cycle.
		/// </summary>
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public LastSyncInfo GetLastSyncInfo()
		{
			log.Debug( "GetLastSyncInfo - called" );
			
			IdentitySync.State state = Simias.IdentitySync.Service.lastState;
			if ( state == null )
			{
				return null;
			}
			
			LastSyncInfo info = new LastSyncInfo();
			info.ReportedErrors = state.Errors;
			info.MembersProcessed = state.Processed;
			log.Debug( "  members processed: "  + info.MembersProcessed.ToString() );

			info.StartTime = 			
				String.Format(
					"{0}, {1} {2} {3} {4}:{5}:{6} GMT",
					state.StartTime.DayOfWeek.ToString(),
					state.StartTime.Day,
					Simias.IdentitySync.Manager.MonthsOfYear[ state.StartTime.Month - 1 ],
					state.StartTime.Year.ToString(),
					state.StartTime.Hour,
					state.StartTime.Minute,
					state.StartTime.Second );
			log.Debug( "  sync start time: " + info.StartTime );
			
			info.EndTime = 			
				String.Format(
					"{0}, {1} {2} {3} {4}:{5}:{6} GMT",
					state.EndTime.DayOfWeek.ToString(),
					state.EndTime.Day,
					Simias.IdentitySync.Manager.MonthsOfYear[ state.EndTime.Month - 1 ],
					state.EndTime.Year.ToString(),
					state.EndTime.Hour,
					state.EndTime.Minute,
					state.EndTime.Second );
			log.Debug( "  sync end time: " + info.EndTime );
					
			info.MembersAdded = 0;
			info.MembersDeleted = state.Deleted;
			info.MembersDisabled = state.Disabled;

			log.Debug( "  start processing messages" );	
			
			if ( state.Messages != null )
			{
				string[] messages = state.Messages;
				info.Messages = new string[ messages.Length ];
				for( int i = 0; i < messages.Length; i++ )
				{
					info.Messages[i] = messages[i]; 
				}
				messages = null;
			}

			state = null;			
			return info;
		}
	
		/// <summary>
		/// Method to set the grace period a member is given
		/// before they are removed from the domain.
		/// Members are disabled during this grace period.
		/// Represented in seconds
		/// </summary>
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public void SetDeleteMemberGracePeriod( int Seconds )
		{
			Service.deleteGracePeriod = Seconds;
		}
	
		/// <summary>
		/// Method to set the synchronization interval for the
		/// sync engine.  Represented in seconds
		/// </summary>
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public void SetSynchronizationInterval( int Seconds )
		{
			Service.syncInterval = Seconds;
		}
	}
	
}
