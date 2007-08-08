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
 | Author: Dale Olds <olds@novell.com>
 |***************************************************************************/


using System;
using Simias;
using Simias.Storage;
using Simias.Policy;

namespace Simias.Sync
{
	/// <summary>
	/// Summary description for SyncPolicy.
	/// </summary>
	public class SyncPolicy
	{
		public enum PolicyType
		{
			Quota = 1,
			Size,
			Type
		};
		
		DiskSpaceQuota	dsQuota;
		FileSizeFilter	fsFilter;
		FileTypeFilter	ftFilter;
		PolicyType		reason;

		
		/// <summary>
		/// Constructs a SyncPolicy object.
		/// </summary>
		/// <param name="collection">The collection the policy belongs to.</param>
		public SyncPolicy(Collection collection)
		{
			// Check if files pass policy.
			Member member = collection.GetCurrentMember();
			dsQuota = DiskSpaceQuota.Get(collection);
			fsFilter = FileSizeFilter.Get(collection);
			ftFilter = FileTypeFilter.Get(collection);
		}

		/// <summary>
		/// Called to check if the file passes the policy.
		/// </summary>
		/// <param name="fNode">The node to check.</param>
		/// <returns>True if node passes policy.</returns>
		public bool Allowed(BaseFileNode fNode)
		{
			long fSize = fNode.Length;
			if (!dsQuota.Allowed(fSize))
			{
				reason = PolicyType.Quota;
				return false;
			}
			if (!fsFilter.Allowed(fSize))
			{
				reason = PolicyType.Size;
				return false;
			}
			if (!ftFilter.Allowed(fNode.GetFileName()))
			{
				reason = PolicyType.Type;
				return false;
			}
			return true;
		}

		/// <summary>
		/// Removes this node from the stored policy state.
		/// </summary>
		/// <param name="fNode">The node to remove.</param>
		public void Remove(BaseFileNode fNode)
		{
			dsQuota.Allowed(-fNode.Length);
		}

		/// <summary>
		/// Gets the policy type that failed.
		/// </summary>
		public PolicyType FailedType
		{
			get { return reason; }
		}
	}
}
