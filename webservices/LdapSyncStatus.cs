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
*                 $Author: Scott
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
