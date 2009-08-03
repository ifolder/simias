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

using Simias;
using Simias.Storage;
using Simias.Server;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder System
	/// </summary>
	[Serializable]
	public class iFolderSystem
	{

                /// <summary>
		/// Group Quota Restriction Method.
		/// </summary>
		private enum QuotaRestriction
		{
			UI_Based,
			Sync_Based
		}

		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(iFolderUser));

		/// <summary>
		/// System ID
		/// </summary>
		public string ID;

		/// <summary>
		/// System Name
		/// </summary>
		public string Name;

		/// <summary>
		/// System Version
		/// </summary>
		public string Version;

		/// <summary>
		/// System Description
		/// </summary>
		public string Description;

		/// <summary>
		/// System Full Name Display Setting 
		/// </summary>
		public string UsersFullNameDisplay;

		/// <summary>
		/// Group Quota Restriction Method 
		/// </summary>
		public int GroupQuotaRestrictionMethod;

		/// <summary>
		/// System Group Splitting Property 
		/// </summary>
		public string GroupSegregated;

		/// <summary>
		/// Identifier for the System Report iFolder where iFolder 
		/// reports will be generated.
		/// </summary>
		public string ReportiFolderID;

		/// <summary>
		/// Report collection name for the System Report iFolder.
		/// </summary>
		public string ReportiFolderName;

		/// <summary>
		/// Directory path where iFolder reports will be generated if
		/// not specified to use the report iFolder.
		/// </summary>
		public string ReportPath;

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderSystem()
		{
		}

		/// <summary>
		/// Get the iFolder System Information Object
		/// </summary>
		/// <returns>An iFolderSystem Object</returns>
		public static iFolderSystem GetSystem()
		{
			iFolderSystem system = new iFolderSystem();

			Store store = Store.GetStore();
			
			Domain domain = store.GetDomain(store.DefaultDomain);

			system.ID = domain.ID;
			system.Name = domain.Name;
			system.Version = domain.DomainVersion.ToString();
			system.Description = domain.Description;
			system.UsersFullNameDisplay = domain.UsersFullNameDisplay;
			system.GroupQuotaRestrictionMethod = domain.GroupQuotaRestrictionMethod;
			system.GroupSegregated = domain.GroupSegregated;

			system.ReportPath = Report.ReportPath;
			system.ReportiFolderID = Report.ReportCollectionID;
			system.ReportiFolderName = Report.ReportCollectionName;

            return system;
		}

		/// <summary>
		/// Sets iFolder system information
		/// </summary>
		/// <param name="system">An iFolderSystem Object</param>
		public static void SetSystem(iFolderSystem system)
		{
			Store store = Store.GetStore();
			
			Domain domain = store.GetDomain(store.DefaultDomain);
			domain.Name = system.Name;
			domain.Description = system.Description;
			domain.UsersFullNameDisplay = system.UsersFullNameDisplay;

			// before setting GroupQuotaRestrictionMethod, check if it is going to save the same method again, 
			// If true, then there is no need to set the same value again, just skip.
			if( (int)domain.GroupQuotaRestrictionMethod != (int)system.GroupQuotaRestrictionMethod )
			{
				ChangeDefaultGroupQuota(system.GroupQuotaRestrictionMethod);
				domain.GroupQuotaRestrictionMethod = system.GroupQuotaRestrictionMethod;
			}
			domain.GroupSegregated = system.GroupSegregated;

			domain.Commit();
		}

		/// <summary>
		/// Change the default disk quota for the managed groups (falling under secondary admin) based on sync control method 
		/// It will only change the default disk quota set on each group, If quota was changed by admin, thent his method will not change that.
		/// </summary>
		/// <param name="GroupQuotaRestrictionMethod">the enum which will decide what will be the default disk quota for each group</param>
		public static void ChangeDefaultGroupQuota(int GroupQuotaRestrictionMethod)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			ICSList SecAdmins = domain.GetMembersByRights(Access.Rights.Secondary);
			try
			{
				foreach(ShallowNode sn in SecAdmins)
				{
					Member AdminAsMember = new Member(domain, sn);
					if (!AdminAsMember.IsType("Host"))
					{
						string [] ManagedGroups = AdminAsMember.GetMonitoredGroups();
						foreach(string GroupID in ManagedGroups)
						{
							Member GroupAsMember = domain.GetMemberByID(GroupID);
							long GroupDiskQuota = Simias.Policy.DiskSpaceQuota.Get( GroupAsMember ).Limit;	
							if(GroupQuotaRestrictionMethod == (int)QuotaRestriction.UI_Based && GroupDiskQuota == -1)
							{
								// change the default disk quota for groups, (from Unlimited to 0MB) 
								Simias.Policy.DiskSpaceQuota.Set(GroupAsMember, 0);
							}
							else if (GroupQuotaRestrictionMethod == (int)QuotaRestriction.Sync_Based && GroupDiskQuota == 0)
							{
								// change the default disk quota for groups, (from 0MB to Unlimited)
								Simias.Policy.DiskSpaceQuota.Delete(GroupAsMember);	
							}
						}
					}
				}
				domain.Commit();
			}
			catch (Exception ex)
			{
				log.Debug("Exception during changing the default disk quota. "+ex.ToString());
			}
		}
    }
}
