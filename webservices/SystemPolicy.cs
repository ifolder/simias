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
                /// The ifolder  Limit for Users
                /// </summary>
                public long NoiFoldersLimit;
		
		/// <summary>
		/// The iFolder Sync Interval
		/// </summary>
		public int SyncInterval;

		/// <summary>
		/// The iFolder Sync Interval
		/// </summary>
		public int EncryptionStatus;
	
		/// <summary>
		/// The Disable Sharing status
		/// </summary>
		public int SharingStatus;

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

			//ifolder limit
			props.NoiFoldersLimit = iFolderLimit.GetLimit(domain);

			// sync internval
			props.SyncInterval = Simias.Policy.SyncInterval.GetInterval(domain);

			// file size
			props.FileSizeLimit = FileSizeFilter.GetLimit(domain);
			
			props.EncryptionStatus = Simias.Policy.SecurityState.GetStatus(domain);

			// Disable sharing policy
			props.SharingStatus = Simias.Policy.Sharing.GetStatus(domain);
		
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

			// ifolder limit
                        iFolderLimit.Set(domain, props.NoiFoldersLimit);

			// sync interval
			if (props.SyncInterval >= 0)
			{
				Simias.Policy.SyncInterval.Set(domain, props.SyncInterval);
			}
			// Added by Ramesh
			//Encryption Status
			Simias.Policy.SecurityState.Create(domain, props.EncryptionStatus);

			// Setting the enumerator value for disabling sharing
			Simias.Policy.Sharing.Create(domain, props.SharingStatus);

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
