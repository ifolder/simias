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
| Author: Rob 
|***************************************************************************/

using System;
using System.Collections;

using Simias;
using Simias.Storage;
using Simias.Policy;

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder System Policy
	/// </summary>
	[Serializable]
	public class SystemPolicy 
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Member));

		/// <summary>
		/// The Disk Space Limit for Users
		/// </summary>
		public long SpaceLimitUser;
		
		/// <summary>
		/// The iFolder Sync Interval
		/// </summary>
		public int SyncInterval;

		/// <summary>
		/// The iFolder Sync Interval
		/// </summary>
		public int EncryptionStatus;

		/// <summary>
		/// The Maximum File Size Limit
		/// </summary>
		public long FileSizeLimit;

		/// <summary>
		/// The File Types to Be Included
		/// </summary>
		public string[] FileTypesIncludes;

		/// <summary>
		/// The File Types to Be Excluded
		/// </summary>
		public string[] FileTypesExcludes;

		/// <summary>
		/// Constructor
		/// </summary>
		public SystemPolicy()
		{
		}

		/// <summary>
		/// Get the iFolder System Policy
		/// </summary>
		/// <returns>An SystemPolicy Object</returns>
		public static SystemPolicy GetPolicy()
		{
			SystemPolicy props = new SystemPolicy();

			Store store = Store.GetStore();
			
			string domain = store.DefaultDomain;

			// space limit
			props.SpaceLimitUser = DiskSpaceQuota.GetLimit(domain);

			// sync internval
			props.SyncInterval = Simias.Policy.SyncInterval.GetInterval(domain);

			// file size
			props.FileSizeLimit = FileSizeFilter.GetLimit(domain);
			
			props.EncryptionStatus = Simias.Policy.SecurityState.GetStatus(domain);

			// file types
			SystemPolicy.SplitFileTypes(FileTypeFilter.GetPatterns(domain),
				out props.FileTypesIncludes, out props.FileTypesExcludes);

			return props;
		}

		/// <summary>
		/// Set the iFolder System Policy
		/// </summary>
		/// <param name="props">The SystemPolicy Object</param>
		public static void SetPolicy(SystemPolicy props)
		{
			Store store = Store.GetStore();
			
			string domain = store.DefaultDomain;

			// space limit
			if (props.SpaceLimitUser >= 0)
			{
				DiskSpaceQuota.Set(domain, props.SpaceLimitUser);
			}

			// sync interval
			if (props.SyncInterval >= 0)
			{
				Simias.Policy.SyncInterval.Set(domain, props.SyncInterval);
			}
			// Added by Ramesh
			//Encryption Status
			Simias.Policy.SecurityState.Create(domain, props.EncryptionStatus);

			// file size
			if (props.FileSizeLimit >= 0)
			{
				FileSizeFilter.Set(domain, props.FileSizeLimit);
			}

			// file types
			if ((props.FileTypesExcludes != null) || (props.FileTypesIncludes != null))
			{
				FileTypeFilter.Set(domain, SystemPolicy.CombineFileTypes(
					props.FileTypesIncludes, props.FileTypesExcludes));
			}
		}

		/// <summary>
		/// Split the file types into include and exclude lists.
		/// </summary>
		/// <param name="entries"></param>
		/// <param name="includes"></param>
		/// <param name="excludes"></param>
		internal static void SplitFileTypes(FileTypeEntry[] entries,
			out string[] includes, out string[] excludes)
		{
			ArrayList includesList = new ArrayList();
			ArrayList excludesList = new ArrayList();
			
			if (entries != null)
			{	
				foreach(FileTypeEntry entry in entries)
				{
					if (entry.Allowed)
					{
						includesList.Add(entry.Name);
					}
					else
					{
						excludesList.Add(entry.Name);
					}
				}
			}

			includes = (string[])includesList.ToArray(typeof(string));
			excludes = (string[])excludesList.ToArray(typeof(string));
		}

		/// <summary>
		/// Combine the file type entries from include and exclude lists.
		/// </summary>
		/// <param name="includes"></param>
		/// <param name="excludes"></param>
		/// <returns></returns>
		/// <remarks>always ignore case (assumbed by iManager)</remarks>
		internal static FileTypeEntry[] CombineFileTypes(string[] includes, string[] excludes)
		{
			ArrayList entries = new ArrayList();

			foreach(string name in includes)
			{
				entries.Add(new FileTypeEntry(name, true, true));
			}

			foreach(string name in excludes)
			{
				entries.Add(new FileTypeEntry(name, false, true));
			}

			return (FileTypeEntry[]) entries.ToArray(typeof(FileTypeEntry));
		}
	}
}
