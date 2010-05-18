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
*                 $Author: Ramesh Sunder (sramesh@novell.com)
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
using System.IO;
using System.Threading;
using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System.Xml;

namespace Novell.iFolderWeb.Admin
{
	public class UserGroupAdminRights
	{
		public bool	iFolderLimitAllowed;
		public bool 	DiskQuotaAllowed;
		public bool 	FileSizeAllowed;
		public bool 	SyncIntervalAllowed;
		public bool	AddToExcludePolicyAllowed;
		public bool 	RemoveToExcludePolicyAllowed;
		public bool	ChangeSharingAllowed;
		public bool 	ChangeEncryptionAllowed;
		public bool	ProvisioningAllowed;
		public bool	EnableDisableUserAllowed;
		public bool	OwnOrphaniFolderAllowed;
		public bool	EnableDisableiFolderAllowed;
		public bool	ModifyMemberRightAllowed;
	        public bool     DeleteiFolderAllowed;

		public long 	preference;
		public static 	int defaultpreference;
		
		public enum GroupAdminPreferencesEnum
		{
			iFolderLimitAllowed = 0x00000001,		//1
			DiskQuotaAllowed = 0x00000002,			//2
			FileSizeAllowed = 0x00000004,			//4
			SyncIntervalAllowed = 0x00000008,		//8
			AddToExcludePolicyAllowed = 0x00000010,		//16
			ChangeSharingAllowed = 0x00000020,		//32
			ChangeEncryptionAllowed = 0x00000040,		//64
			ProvisioningAllowed = 0x00000080,		//128
			EnableDisableUserAllowed = 0x00000100,		//256
			OwnOrphaniFolderAllowed = 0x00000200,		//512
			EnableDisableiFolderAllowed = 0x00000400,	//1024
			ModifyMemberRightAllowed = 0x00000800,		//2048
			DeleteiFolderAllowed = 0x00001000,		//4096
		};

		public UserGroupAdminRights(long value)
		{
			this.preference = value;
			this.iFolderLimitAllowed = (this.preference & (int)GroupAdminPreferencesEnum.iFolderLimitAllowed) == 0 ? false:true;
			this.DiskQuotaAllowed = (this.preference & (int)GroupAdminPreferencesEnum.DiskQuotaAllowed) == 0 ? false:true;
			this.FileSizeAllowed = (this.preference & (int)GroupAdminPreferencesEnum.FileSizeAllowed)== 0 ? false:true;
			this.SyncIntervalAllowed = (this.preference & (int)GroupAdminPreferencesEnum.SyncIntervalAllowed) == 0 ? false:true;
			this.AddToExcludePolicyAllowed = (this.preference & (int)GroupAdminPreferencesEnum.AddToExcludePolicyAllowed) == 0 ? false:true;

			this.ChangeSharingAllowed = (this.preference & (int)GroupAdminPreferencesEnum.ChangeSharingAllowed) == 0 ? false:true;
			this.ChangeEncryptionAllowed = (this.preference & (int)GroupAdminPreferencesEnum.ChangeEncryptionAllowed) == 0 ? false:true;
			this.ProvisioningAllowed = (this.preference & (int)GroupAdminPreferencesEnum.ProvisioningAllowed) == 0 ? false:true;
                        this.EnableDisableUserAllowed = (this.preference & (int)GroupAdminPreferencesEnum.EnableDisableUserAllowed) == 0 ? false:true;

                        this.OwnOrphaniFolderAllowed = (this.preference & (int)GroupAdminPreferencesEnum.OwnOrphaniFolderAllowed) == 0 ? false:true;
                        this.EnableDisableiFolderAllowed = (this.preference & (int)GroupAdminPreferencesEnum.EnableDisableiFolderAllowed) == 0 ? false:true;
                        this.ModifyMemberRightAllowed = (this.preference & (int)GroupAdminPreferencesEnum.ModifyMemberRightAllowed) == 0 ? false:true;
                        this.DeleteiFolderAllowed = (this.preference & (int)GroupAdminPreferencesEnum.DeleteiFolderAllowed) == 0 ? false:true;

		}
	}

	public class UserSystemAdminRights
	{
		public bool	ReportsGenerationAllowed;
		public bool	UserManagementAllowed;
		public bool	iFolderManagementAllowed;
		public bool	SystemPolicyManagementAllowed;
		public bool	ServerPolicyManagementAllowed;
		public bool	SecondaryAdminAddAllowed;
		int 	preference;
		
		public enum userSystemAdminPreferencesEnum
		{
			ReportsGenerationAllowed = 0x00000001,		//1
			UserManagementAllowed = 0x00000002,			//2
			iFolderManagementAllowed = 0x00000004,			//4
			SystemPolicyManagementAllowed = 0x00000008,		//8
			ServerPolicyManagementAllowed = 0x00000010,		//16
			SecondaryAdminAddAllowed = 0x00000020,		//32
		};

		public UserSystemAdminRights(int value)
		{
			this.preference = value;
			this.UserManagementAllowed = true;
			this.iFolderManagementAllowed = true;
			this.SystemPolicyManagementAllowed = (this.preference & (int)userSystemAdminPreferencesEnum.SystemPolicyManagementAllowed) == 0 ? false:true;
			this.ServerPolicyManagementAllowed = (this.preference & (int)userSystemAdminPreferencesEnum.ServerPolicyManagementAllowed) == 0 ? false:true;
			this.ReportsGenerationAllowed = (this.preference & (int)userSystemAdminPreferencesEnum.ReportsGenerationAllowed) == 0 ? false:true;
			this.SecondaryAdminAddAllowed = (this.preference & (int)userSystemAdminPreferencesEnum.SecondaryAdminAddAllowed) == 0 ? false:true;
		}
	}
}
