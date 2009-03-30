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
*                 $Author: Rob
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

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder Policy
	/// </summary>
	[Serializable]
	public class iFolderPolicy 
	{
		/// <summary>
		/// String that is used to lock collections.
		/// </summary>
		static private string lockName = "AdminStopSyncAndOtherChangesString";

		/// <summary>
		/// The iFolder ID
		/// </summary>
		public string iFolderID;

        /// <summary>
        /// Is the iFolder Locked?
        /// </summary>
        public bool Locked;

		/// <summary>
		/// The iFolder Disk Space Limit
		/// </summary>
		public long SpaceLimit;

		/// <summary>
		/// The effective iFolder Disk Space Limit
		/// </summary>
		public long SpaceLimitEffective;

		/// <summary>
		/// The Amount of Disk Space Used by the iFolder
		/// </summary>
		public long SpaceUsed;

		/// <summary>
		/// The Amount of Disk Space Available to the iFolder
		/// </summary>
		public long SpaceAvailable;

		/// <summary>
		/// The iFolder Sync Interval
		/// </summary>
		public int SyncInterval;

		/// <summary>
		/// The effective ifolder sync interval.
		/// </summary>
		public int SyncIntervalEffective;
	
		/// <summary>
		/// The value to store sharing policy value for an iFolder
		/// </summary>
		public int SharingStatus;

		/// <summary>
		/// The File Types to Be Included
		/// </summary>
		public string[] FileTypesIncludes;

		/// <summary>
		/// The effective file type to be included.
		/// </summary>
		public string[] FileTypesIncludesEffective;

		/// <summary>
		/// The File Types to Be Excluded
		/// </summary>
		public string[] FileTypesExcludes;

		/// <summary>
		/// The effective file types to be excluded.
		/// </summary>
		public string[] FileTypesExcludesEffective;

		/// <summary>
		/// The Maximum File Size Limit
		/// </summary>
		public long FileSizeLimit;

		/// <summary>
		/// The effective maximum file size limit.
		/// </summary>
		public long FileSizeLimitEffective;

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderPolicy()
		{
		}

		/// <summary>
		/// Is the iFolder Locked?
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		public static bool IsLocked(Collection collection)
		{
			return collection.IsLockedByName(lockName);
		}

		/// <summary>
		/// Get the iFolder Policy
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolderPolicy Object</returns>
		public static iFolderPolicy GetPolicy(string ifolderID, string accessID)
		{
			iFolderPolicy props = new iFolderPolicy();

			props.iFolderID = ifolderID;

			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(ifolderID);
			
			if (c == null) throw new iFolderDoesNotExistException(ifolderID);

			// impersonate
			iFolder.Impersonate(c, accessID);

			// disk space
			DiskSpaceQuota dsq = DiskSpaceQuota.Get(c);
			props.SpaceLimitEffective = dsq.Limit;
			props.SpaceAvailable = dsq.AvailableSpace;
			props.SpaceUsed = c.StorageSize;
			props.SpaceLimit = DiskSpaceQuota.GetLimit(c);

			// no syncing (locked)
			//props.Locked = IsLocked(c);
			props.Locked = c.Disabled;

			// sync interval
			props.SyncInterval = Simias.Policy.SyncInterval.GetInterval(c);
			props.SyncIntervalEffective = Simias.Policy.SyncInterval.Get(c).Interval;
	
			// to return the value of disable sharing policy for an iFolder
			props.SharingStatus = Simias.Policy.Sharing.GetStatus(c);

			// file types
			SystemPolicy.SplitFileTypes(FileTypeFilter.GetPatterns(c),
				out props.FileTypesIncludes, out props.FileTypesExcludes);

			SystemPolicy.SplitFileTypes(FileTypeFilter.Get(c).FilterList,
				out props.FileTypesIncludesEffective, out props.FileTypesExcludesEffective );

			// file size
			props.FileSizeLimit = Simias.Policy.FileSizeFilter.GetLimit(c);
			props.FileSizeLimitEffective = Simias.Policy.FileSizeFilter.Get(c).Limit;

			return props;
		}

		/// <summary>
		/// Set the iFolder Policy
		/// </summary>
		/// <param name="props">The iFolderPolicy Object</param>
		/// <param name="accessID">The Access User ID</param>
		public static void SetPolicy(iFolderPolicy props, string accessID)
		{
			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(props.iFolderID);

			if (c == null) throw new iFolderDoesNotExistException(props.iFolderID);

			// impersonate
			iFolder.Impersonate(c, accessID);

			// NOTE: always unlock the collection so other policy properties
			// can be modified
			if (c.IsLockedByName(lockName))
			{
				c.Unlock(lockName);
			}

			// disk space
			if (props.SpaceLimit >= 0)
			{
				DiskSpaceQuota.Set(c, props.SpaceLimit);
			}

			// sync interval
			if (props.SyncInterval >= 0)
			{
				Simias.Policy.SyncInterval.Set(c, props.SyncInterval);
			}

			// to set the value for disable sharing policy for this iFolder
			if(props.SharingStatus >= 0)
			{
				Simias.Policy.Sharing.Set(c, props.SharingStatus);
			}

			// file types
			if ((props.FileTypesExcludes != null) || (props.FileTypesIncludes != null))
			{
				FileTypeFilter.Set(c, SystemPolicy.CombineFileTypes(
					props.FileTypesIncludes, props.FileTypesExcludes));
			}

			// file size
			if (props.FileSizeLimit >= 0)
			{
				Simias.Policy.FileSizeFilter.Set(c, props.FileSizeLimit);
			}

			// no syncing (locked)
			// NOTE: re-lock the collection (see the beginning of the method)
			// if a lock was requested
			//if (props.Locked)
			{
			//	c.Lock(lockName);
				c.Disabled = props.Locked;
				c.Commit();
			}
		}
	}
}
