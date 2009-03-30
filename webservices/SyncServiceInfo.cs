/*****************************************************************************
* Copyright © [2007-08] Unpublished Work of Novell, Inc. All Rights Reserved.
*
* THIS IS AN UNPUBLISHED WORK OF NOVELL, INC.  IT CONTAINS NOVELL'S CONFIDENTIAL, 
* PROPRIETARY, AND TRADE SECRET INFORMATION.	NOVELL RESTRICTS THIS WORK TO 
* NOVELL EMPLOYEES WHO NEED THE WORK TO PERFORM THEIR ASSIGNMENTS AND TO 
* THIRD PARTIES AUTHORIZED BY NOVELL IN WRITING.  THIS WORK MAY NOT BE USED, 
* COPIED, DISTRIBUTED, DISCLOSED, ADAPTED, PERFORMED, DISPLAYED, COLLECTED,
* COMPILED, OR LINKED WITHOUT NOVELL'S PRIOR WRITTEN CONSENT.  USE OR 
* EXPLOITATION OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE 
* PERPETRATOR TO CRIMINAL AND  CIVIL LIABILITY.
*
* Novell is the copyright owner of this file.  Novell may have released an earlier version of this
* file, also owned by Novell, under the GNU General Public License version 2 as part of Novell's 
* iFolder Project; however, Novell is not releasing this file under the GPL.
*
*-----------------------------------------------------------------------------
*
*                 Novell iFolder Enterprise
*
*-----------------------------------------------------------------------------
*
*                 $Author: Bruce Getter <bgetter@novell.com>
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
using System.Collections;
using System.Reflection;


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
		/// Delegate to call SynchronizationDetails method
		/// </summary>
		delegate string SynchronizationDetailsMethod( int type );

		/// <summary>
		/// Enum for Sync details options
		/// </summary>
		public enum sync
               	{
                       	UpDateTime = 1,
                       	SyncCycles = 2,
			DeleteGracePeriod = 3,
			SyncInterval = 4,
			SyncStatus = 5
               	}

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

			if(Simias.Service.Manager.LdapServiceEnabled == true)
			{
               			string identitySyncClass = "Simias.IdentitySynchronization.Service";
                        	string SynchronizationDetailsMethod = "GetSynchronizationDetails";
                        	SynchronizationDetailsMethod SyncDetails = null;
				string assemblyName = Simias.Service.Manager.LdapAssemblyName;
				DateTime UpSince;

                                if ( assemblyName != null )
                                {
                                	Assembly idAssembly = Assembly.Load( assemblyName );
                                	if ( idAssembly != null )
                                	{
                                		Type type = idAssembly.GetType( identitySyncClass );
                                		if ( type != null )
                                		{
                                			MethodInfo SynchronizationDetailsNow=type.GetMethod(SynchronizationDetailsMethod,                                                                                           BindingFlags.Public | BindingFlags.Static );
                                			SyncDetails=(SynchronizationDetailsMethod) Delegate.CreateDelegate( 														typeof(SynchronizationDetailsMethod), SynchronizationDetailsNow);
                                			UpSince = DateTime.Parse(SyncDetails( (int) sync.UpDateTime));
							info.UpSince =
							String.Format(
								"{0}, {1} {2} {3} {4}:{5}:{6} GMT",
								UpSince.DayOfWeek.ToString(),
								UpSince.Day,
								LastSyncInfo.MonthsOfYear[ Service.UpSince.Month - 1 ],
								UpSince.Year.ToString(),
								UpSince.Hour,
								UpSince.Minute,
								UpSince.Second );

							info.Cycles = Convert.ToInt32(SyncDetails((int) sync.SyncCycles));
			
							foreach( IIdentitySyncProvider prov in Service.RegisteredProviders.Values )
							{
								info.Provider = prov.Name;
								break;
							}
			
							info.DeleteMemberGracePeriod = Convert.ToInt32(SyncDetails((int) sync.DeleteGracePeriod));
							info.SynchronizationInterval = Convert.ToInt32(SyncDetails((int) sync.SyncInterval));
							info.Status = SyncDetails((int) sync.SyncStatus);
                                		}
                                	}
                                }

			}
			else
			{
			
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
			
			}
			return info;
		}
	}
}
