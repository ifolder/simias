/***********************************************************************
 *  $RCSfile: LdapSyncStatus.cs,v $
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Scott
 *
 ***********************************************************************/

using System;
using System.Collections;

using Simias;
using Simias.Storage;
using Simias.Policy;
using Novell.iFolder;
using Novell.AddressBook.LdapSync;

namespace Novell.iFolder.Enterprise.Web
{
	/// <summary>
	/// iFolder LDAP Sync Status
	/// </summary>
	[Serializable]
	public class LdapSyncStatus 
	{
		/// <summary>
		/// Sync status
		/// </summary>
		public Novell.AddressBook.LdapSync.Status Status;

		/// <summary>
		/// Was there an error during the last sync?
		/// </summary>
		public bool LastSyncError;

		/// <summary>
		/// The last sync exception
		/// </summary>
		public string LastSyncException;

		/// <summary>
		/// The last sync time
		/// </summary>
		public DateTime LastSyncTime;

		/// <summary>
		/// Constructor
		/// </summary>
		public LdapSyncStatus()
		{
		}

		/// <summary>
		/// Get the iFolder LDAP Sync Status
		/// </summary>
		/// <returns>An LdapSyncStatus Object</returns>
		public static LdapSyncStatus GetStatus()
		{
			LdapSyncStatus result = new LdapSyncStatus();

            LdapSystemBookService service = new LdapSystemBookService();
            LdapSystemBookService.Status status  = service.GetServiceStatus();

			result.Status = status.CurrentStatus;
			result.LastSyncError = status.ErrorDuringSync;
            result.LastSyncException = (status.SyncException != null) ? status.SyncException.ToString() : "";
			result.LastSyncTime = status.LastSyncTime;

			return result;
		}
	}
}
