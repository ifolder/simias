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

using Simias;
using Simias.IdentitySync;

namespace iFolder.WebService
{
	/// <summary>
	/// Class that represents the state of the last identity
	/// synchronization cycle
	/// </summary>
	[ Serializable ]
	public class LastSyncInfo
	{
		internal static string[] MonthsOfYear =
		{
			"Jan",
			"Feb",
			"Mar",
			"Apr",
			"May",
			"Jun",
			"Jul",
			"Aug",
			"Sep",
			"Oct",
			"Nov",
			"Dec"
		};
		
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log =
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
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
		/// Number of members changed or updated because
		/// of meta-data changing ex. First Name
		/// </summary>
		public int		MembersUpdated;

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

		/// <summary>
		/// Constructor
		/// </summary>
		public LastSyncInfo()
		{
		}

		/// <summary>
		/// Get Last Sync Info
		/// </summary>
		/// <returns></returns>
		public static LastSyncInfo GetLastSyncInfo()
		{
			if ( Service.LastState == null )
			{
				return null;
			}

			State state = Service.LastState;
			LastSyncInfo info = new LastSyncInfo();
			info.ReportedErrors = state.Errors;
			info.MembersProcessed = state.Processed;
			log.Debug( "  members processed: "  + info.MembersProcessed.ToString() );

			info.StartTime =
				String.Format(
				"{0}, {1} {2} {3} {4}:{5}:{6} GMT",
				state.StartTime.DayOfWeek.ToString(),
				state.StartTime.Day,
				MonthsOfYear[ state.StartTime.Month - 1 ],
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
				MonthsOfYear[ state.EndTime.Month - 1 ],
				state.EndTime.Year.ToString(),
				state.EndTime.Hour,
				state.EndTime.Minute,
				state.EndTime.Second );
			log.Debug( "  sync end time: " + info.EndTime );
					
			info.MembersAdded = state.Created;
			info.MembersUpdated = state.Updated;
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
	}
}
