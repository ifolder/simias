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
| Author: Scott 
|***************************************************************************/

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
