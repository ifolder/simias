/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
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
	/// iFolder User Policy
	/// </summary>
	[Serializable]
	public class UserPolicy 
	{
		//private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Member));
		/// <summary>
		/// The User ID
		/// </summary>
		public string UserID;

                /// enum to store different disable sharing value
                public enum Share
                {
                        Sharing = 1,
                        EnforcedSharing = 4,
                        DisableSharing = 8
                }


		/// <summary>
		/// Is the User's Login Enabled?
		/// </summary>
		public bool LoginEnabled;

		/// <summary>
		/// Logged in Admin Groupk Rights
		/// </summary>
		public int AdminGroupRights;

	        /// <summary>
        	/// The User Disk Space Limit
	        /// </summary>
	        public long SpaceLimit;

		// Added by ramesh
		public int EncryptionStatus;
        
		/// <summary>
		/// Disable sharing policy for user
		/// </summary>
		public int SharingStatus;
        
		/// <summary>
		/// The Effective User Disk Space Limit
		/// </summary>
		public long SpaceLimitEffective;

		/// <summary>
		/// The Maximum File Size Limit
		/// </summary>
		public long FileSizeLimit;

		/// <summary>
                /// The Maximum No of ifolders/ user Limit
                /// </summary>
                public long NoiFoldersLimit;

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
		/// Is  User Admin
		///</summary>
		public bool isAdmin;

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

                        Access.Rights rights = (member != null) ? member.Rights : Access.Rights.Deny;

                        props.isAdmin = (rights == Access.Rights.Admin);

            props.LoginEnabled = !(domain.GetLoginpolicy(userID));

			// disk space
			DiskSpaceQuota quota = DiskSpaceQuota.Get(member);
			
			props.SpaceLimitEffective = quota.Limit;
			props.SpaceUsed = quota.UsedSpace;
			props.SpaceAvailable = quota.AvailableSpace;

			props.SpaceLimit = DiskSpaceQuota.GetLimit(member);
			props.EncryptionStatus = Simias.Policy.SecurityState.GetStatus( member );
	
			// To return disable sharing value for an user
			props.SharingStatus = Simias.Policy.Sharing.GetStatus( member );

			// file size
			props.FileSizeLimit = FileSizeFilter.GetLimit(member);
			props.FileSizeLimitEffective = FileSizeFilter.Get(member).Limit;

			//No of ifolders limit
			props.NoiFoldersLimit = iFolderLimit.Get(member).Limit;

			// sync interval
			props.SyncInterval = Simias.Policy.SyncInterval.GetInterval(member);
			props.SyncIntervalEffective = Simias.Policy.SyncInterval.Get(member).Interval;

			// file types
			SystemPolicy.SplitFileTypes(FileTypeFilter.GetPatterns(member),
				out props.FileTypesIncludes, out props.FileTypesExcludes);

			// file types effective
			SystemPolicy.SplitFileTypes(FileTypeFilter.Get(member).FilterUserList,
				out props.FileTypesIncludesEffective, out props.FileTypesExcludesEffective);

			return props;
		}

		/// <summary>
		/// Get the User Policy
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <returns>The UserPolicy Object</returns>
		public static UserPolicy GetPolicy(string userID, string AdminId)
		{
			UserPolicy props = new UserPolicy();

			props.UserID = userID;

			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);
			
			Member member = domain.GetMemberByID(userID);
			
			if (member == null) throw new UserDoesNotExistException(userID);

                        Access.Rights rights = (member != null) ? member.Rights : Access.Rights.Deny;

                        props.isAdmin = (rights == Access.Rights.Admin);

            props.LoginEnabled = !(domain.GetLoginpolicy(userID));

			// disk space
			DiskSpaceQuota quota = DiskSpaceQuota.Get(member);
			
			props.SpaceLimitEffective = quota.Limit;
			//props.SpaceUsed = quota.UsedSpace;
			props.SpaceUsed = Simias.Server.Catalog.GetUsedSpaceOfUserID(userID);
			//props.SpaceAvailable = quota.AvailableSpace;

			props.SpaceLimit = DiskSpaceQuota.GetLimit(member);
			props.SpaceAvailable = props.SpaceLimitEffective - props.SpaceUsed;
			props.EncryptionStatus = Simias.Policy.SecurityState.GetStatus( member );
	
			// To return disable sharing value for an user
			props.SharingStatus = Simias.Policy.Sharing.GetStatus( member );

			// file size
			props.FileSizeLimit = FileSizeFilter.GetLimit(member);
			props.FileSizeLimitEffective = FileSizeFilter.Get(member).Limit;

			//No of ifolders limit
			props.NoiFoldersLimit = iFolderLimit.Get(member).Limit;

			// sync interval
			props.SyncInterval = Simias.Policy.SyncInterval.GetInterval(member);
			props.SyncIntervalEffective = Simias.Policy.SyncInterval.Get(member).Interval;

			// file types
			SystemPolicy.SplitFileTypes(FileTypeFilter.GetPatterns(member),
				out props.FileTypesIncludes, out props.FileTypesExcludes);

			// file types effective
			SystemPolicy.SplitFileTypes(FileTypeFilter.Get(member, false).FilterUserList,
				out props.FileTypesIncludesEffective, out props.FileTypesExcludesEffective);
			props.AdminGroupRights = iFolderUser.GetAdminRights(AdminId , userID);
			return props;
		}

		/// <summary>
		/// Get the User Groups Number of iFolder Policy value.
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <returns> 0 --> Allowed -1 -> Allready exceeding the limit  -2 --> Groups policy not set , check systems  </returns>
		public static int GetUserGroupiFolderLimitPolicy(string userID,long OwnediFolderCount)
		{
			int result = -2;
			UserPolicy user = null;

			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			string[] GIDs = domain.GetMemberFamilyList(userID);
                        foreach(string gid in GIDs)
                        {
				if(gid != userID)
				{
					user = UserPolicy.GetPolicy(gid);
					if(user.NoiFoldersLimit != -1 && user.NoiFoldersLimit != -2)
					{
						if( user.NoiFoldersLimit <= OwnediFolderCount)
							result = -1;
						else
							result = 0;
						break;
					}
				}
                        }
			return result;
		}

		/// <summary>
		/// Get the user groups sharing policy.
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <returns> Returns the user groups sharing policy value </returns>
		public static int GetUserGroupSharingPolicy(string userID)
		{
			int groupSharing = 0;
			int retStatus = 0;
			UserPolicy userPolicy = null;

			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			string[] GIDs = domain.GetMemberFamilyList(userID);
                        foreach(string gid in GIDs)
                        {
				if(gid != userID)
				{
					userPolicy = UserPolicy.GetPolicy(gid);
					groupSharing = userPolicy.SharingStatus;
					if((groupSharing & (int) Share.EnforcedSharing) == (int) Share.EnforcedSharing)
						return groupSharing;
					if(groupSharing  >=  (int)Share.Sharing && retStatus == 0)
                                                retStatus = groupSharing;
				}
                        }
			return retStatus;
		}

		/// <summary>
		/// Get the user groups Encryption  policy.
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <returns> Returns the user groups Encryption policy value </returns>
		public static int GetUserGroupEncryptionPolicy(string userID)
		{
			int groupEncryption = 0;
			UserPolicy userPolicy = null;

			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			string[] GIDs = domain.GetMemberFamilyList(userID);
                        foreach(string gid in GIDs)
                        {
				if(gid != userID)
				{
					userPolicy = UserPolicy.GetPolicy(gid);
					groupEncryption = userPolicy.EncryptionStatus;
					if(groupEncryption != 0 )
						break;
				}
                        }
			return groupEncryption;
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


// Added by Ramesh
			if(props.EncryptionStatus >=0)
			{
				Simias.Policy.SecurityState.Create( member, props.EncryptionStatus );
			}

			// to set disable sharing policy value for an user
			if(props.SharingStatus >=0)
			{
				Simias.Policy.Sharing.Create( member, props.SharingStatus );
			}

			// disk space
			if (props.SpaceLimit >= -1)
			{
				DiskSpaceQuota.Set(member, props.SpaceLimit);
			}


                        //limiting no of ifolder per user policy.
			if( props.NoiFoldersLimit >= -2)
			{
                        	iFolderLimit.Set(member, props.NoiFoldersLimit);
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
