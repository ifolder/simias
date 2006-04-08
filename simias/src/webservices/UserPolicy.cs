/***********************************************************************
 *  $RCSfile: UserPolicy.cs,v $
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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.Collections;

using Simias;
using Simias.Storage;
using Simias.Policy;

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder User Policy
	/// </summary>
	[Serializable]
	public class UserPolicy 
	{
		/// <summary>
		/// The User ID
		/// </summary>
		public string UserID;

		/// <summary>
		/// Is the User's Login Enabled?
		/// </summary>
		public bool LoginEnabled;

        /// <summary>
        /// The User Disk Space Limit
        /// </summary>
        public long SpaceLimit;
        
		/// <summary>
		/// The Effective User Disk Space Limit
		/// </summary>
		public long SpaceLimitEffective;

		/// <summary>
		/// The Maximum File Size Limit
		/// </summary>
		public long FileSizeLimit;

		/// <summary>
		/// The Effective Maximum File Size Limit
		/// </summary>
		public long FileSizeLimitEffective;

		/// <summary>
		/// The User Disk Space Used
		/// </summary>
		public long SpaceUsed;

		/// <summary>
		/// The User Disk Space Available
		/// </summary>
		public long SpaceAvailable;

		/// <summary>
		/// The User Sync Interval
		/// </summary>
		public int SyncInterval;
		
		/// <summary>
		/// The Effect User Sync Interval
		/// </summary>
		public int SyncIntervalEffective;

		/// <summary>
		/// The File Types to Be Included
		/// </summary>
		public string[] FileTypesIncludes;

		/// <summary>
		/// The File Types to Be Included
		/// </summary>
		public string[] FileTypesIncludesEffective;

		/// <summary>
		/// The File Types to Be Excluded
		/// </summary>
		public string[] FileTypesExcludes;

		/// <summary>
		/// The File Types to Be Excluded
		/// </summary>
		public string[] FileTypesExcludesEffective;

		/// <summary>
		/// Constructor
		/// </summary>
		public UserPolicy()
		{
		}

		/// <summary>
		/// Get the User Policy
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <returns>The UserPolicy Object</returns>
		public static UserPolicy GetPolicy(string userID)
		{
			UserPolicy props = new UserPolicy();

			props.UserID = userID;

			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);
			
			Member member = domain.GetMemberByID(userID);
			
			if (member == null) throw new UserDoesNotExistException(userID);

            props.LoginEnabled = !(domain.IsLoginDisabled(userID));

			// disk space
			DiskSpaceQuota quota = DiskSpaceQuota.Get(member);
			
			props.SpaceLimitEffective = quota.Limit;
			props.SpaceUsed = quota.UsedSpace;
			props.SpaceAvailable = quota.AvailableSpace;

			props.SpaceLimit = DiskSpaceQuota.GetLimit(member);

			// file size
			props.FileSizeLimit = FileSizeFilter.GetLimit(member);
			props.FileSizeLimitEffective = FileSizeFilter.Get(member).Limit;

			// sync interval
			props.SyncInterval = Simias.Policy.SyncInterval.GetInterval(member);
			props.SyncIntervalEffective = Simias.Policy.SyncInterval.Get(member).Interval;

			// file types
			SystemPolicy.SplitFileTypes(FileTypeFilter.GetPatterns(member),
				out props.FileTypesIncludes, out props.FileTypesExcludes);

			// file types effective
			SystemPolicy.SplitFileTypes(FileTypeFilter.Get(member).FilterList,
				out props.FileTypesIncludesEffective, out props.FileTypesExcludesEffective);

			return props;
		}

		/// <summary>
		/// Set the User Policy
		/// </summary>
		/// <param name="props">The UserPolicy Object</param>
		public static void SetPolicy(UserPolicy props)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);
			
			Member member = domain.GetMemberByID(props.UserID);

			if (member == null) throw new UserDoesNotExistException(props.UserID);

			if(props.LoginEnabled == true)
			{
				domain.SetLoginDisabled(props.UserID, false);
			}
			else
			{
				domain.SetLoginDisabled(props.UserID, true);
			}

			// disk space
			if (props.SpaceLimit >= 0)
			{
				DiskSpaceQuota.Set(member, props.SpaceLimit);
			}

			// file size
			if (props.FileSizeLimit >= 0)
			{
				FileSizeFilter.Set(member, props.FileSizeLimit);
			}

			// sync interval
			if (props.SyncInterval >= 0)
			{
				Simias.Policy.SyncInterval.Set(member, props.SyncInterval);
			}

			// file types
			if ((props.FileTypesExcludes != null) || (props.FileTypesIncludes != null))
			{
				FileTypeFilter.Set(member, SystemPolicy.CombineFileTypes(
					props.FileTypesIncludes, props.FileTypesExcludes));
			}
		}
	}
}
