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
| Author: Bruce Getter <bgetter@novell.com> 
|***************************************************************************/
 
using System;

using Simias;
using Simias.IdentitySync;

namespace iFolder.WebService
{
	/// <summary>
	/// Class that represents the current state and configuration
	/// of the synchronization service.
	/// </summary>
	[ Serializable ]
	public class SyncServiceInfo
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

		/// <summary>
		/// Constructor
		/// </summary>
		public SyncServiceInfo()
		{
		}

		/// <summary>
		/// Get Sync Service Info
		/// </summary>
		/// <returns></returns>
		public static SyncServiceInfo GetSyncServiceInfo()
		{
			SyncServiceInfo info = new SyncServiceInfo();
			
			info.UpSince =
				String.Format(
				"{0}, {1} {2} {3} {4}:{5}:{6} GMT",
				Service.UpSince.DayOfWeek.ToString(),
				Service.UpSince.Day,
				LastSyncInfo.MonthsOfYear[ Service.UpSince.Month - 1 ],
				Service.UpSince.Year.ToString(),
				Service.UpSince.Hour,
				Service.UpSince.Minute,
				Service.UpSince.Second );
			
			info.Cycles = Service.Cycles;
			
			// Get the first provider.   At the moment, sync engine only supports
			// one provider anyway.
			foreach( IIdentitySyncProvider prov in Service.RegisteredProviders.Values )
			{
				info.Provider = prov.Name;
				break;
			}
			
			info.DeleteMemberGracePeriod = Service.DeleteGracePeriod;
			info.SynchronizationInterval = Service.SyncInterval;
			info.Status = Service.Status;
			
			return info;
		}
	}
}
