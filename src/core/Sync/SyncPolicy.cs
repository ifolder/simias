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
*                 $Author: Dale Olds <olds@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/


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
		/// <summary>
		/// 
		/// </summary>
		public enum PolicyType
		{
			/// <summary>
			/// 
			/// </summary>
			Quota = 1,
			/// <summary>
			/// 
			/// </summary>
			Size,
			/// <summary>
			/// 
			/// </summary>
			Type
		};

                /// <summary>
                /// Group Quota Restriction Method.
                /// </summary>
                private enum QuotaRestriction
                {
                                // For current Implementation, enum value AllAdmins is not used, can be used in future
                                UI_Based,
                                Sync_Based
                }

		
		DiskSpaceQuota	dsQuota;
		FileSizeFilter	fsFilter;
		FileTypeFilter	ftFilter;
		PolicyType		reason;
		string OwnerID;

		/// <summary>
		/// 
		/// </summary>
		public static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncService));
		
		/// <summary>
		/// Constructs a SyncPolicy object.
		/// </summary>
		/// <param name="collection">The collection the policy belongs to.</param>
		public SyncPolicy(Collection collection)
		{
			// Check if files pass policy.
			//Member member = collection.GetCurrentMember();
			dsQuota = DiskSpaceQuota.Get(collection);
			fsFilter = FileSizeFilter.Get(collection);
			ftFilter = FileTypeFilter.Get(collection);
			OwnerID = collection.Owner.UserID;
		}

		/// <summary>
		/// Called to check if the file passes the policy.
		/// </summary>
		/// <param name="fNode">The node to check.</param>
		/// <returns>True if node passes policy.</returns>
		public bool Allowed(BaseFileNode fNode)
		{
			long fSize = fNode.Length;
			/* If the file is already present on the server, upload size would be 
			 * size of file on the server subtracted by size of file to upload */
			Store stl =Store.GetStore();
			Domain dom =stl.GetDomain(stl.DefaultDomain);
			Node n1 = dom.GetNodeByID(fNode.ID);
			if(n1 != null)
			{
				FileNode f1 = n1 as FileNode;
				if(f1.Length <= fSize)
					fSize = fSize - f1.Length;
				else
					fSize = 0;
			}
			if(!GroupDiskQuotaUploadAllowed(fSize))
			{
				reason = PolicyType.Quota;
				return false;
			}

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
		/// Verify the given size is allowed for upload.
		/// </summary>
		/// <param name="fSize">size to be verifed.</param>
		/// <returns></returns>
		public bool GroupDiskQuotaUploadAllowed(long fSize)
		{
			// Aggregate disk quota violation for group will be checked on the owner of collection (not on the member who is syncing)
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			string CollectionOwnerID = OwnerID;//collection.Owner.UserID;
			//Member member = domain.GetMemberByID(CollectionOwnerID);
				
			bool Allowed = true;
			bool SpaceAllowed = false;
			string [] GroupIDs = domain.GetMemberFamilyList(CollectionOwnerID);
			foreach(string groupID in GroupIDs)
			{
				if( groupID != CollectionOwnerID )
				{
					Member GroupAsMember = domain.GetMemberByID(groupID);
					long AggregateQuota = GroupAsMember.AggregateDiskQuota;
					if(AggregateQuota < 0  )
					{
						// no group quota applied, so allow
						return true;
					}
					if(domain.GroupQuotaRestrictionMethod == (int)QuotaRestriction.Sync_Based)
					{
						long SpaceUsed = SpaceUsedByGroup(groupID, domain.ID, CollectionOwnerID);
						SpaceAllowed = ( AggregateQuota - SpaceUsed ) > fSize ? true: false;
						if( SpaceAllowed == true)
						{
							return true;
						}
						else
						Allowed = false;
					}

				}
			}
                        //log.Debug("Allowed boolean is  :"+Allowed);
                        return Allowed;

		}

		// We call catalog services to read the entries.. Not using discovery because no need...Only read operation is performed
        /// <summary>
        /// Collective space used by group member. 
        /// </summary>
        /// <param name="groupID">Group id whose members space need to be calculated.</param>
        /// <returns></returns>
		static public long GetSpaceUsedByAllGroupMembers( string groupID )
                {
                        long SpaceUsed = 0;
			string catalogID = "a93266fd-55de-4590-b1c7-428f2fed815d";
			string OwnerProperty = "oid";
			string SizeProperty = "size";
			Collection catalog;
                        //ArrayList entries = new ArrayList();
                        Store store = Store.GetStore();
                        Domain domain = store.GetDomain(store.DefaultDomain);
                        string [] GroupMembers = domain.GetGroupsMemberList(groupID);

			catalog = store.GetCollectionByID( catalogID );

                        foreach(string memberID in GroupMembers)
                        {
                                Property midsProp = new Property( OwnerProperty, memberID );
				ICSList nodes = catalog.Search( midsProp, SearchOp.Equal );
                                foreach( ShallowNode sn in nodes )
                                {
					Node node = new Node(catalog, sn);
					Property p = node.Properties.GetSingleProperty( SizeProperty );
                                        SpaceUsed += ( p != null ) ? ( long )p.Value : 0;;
                                }
                        }

                        return SpaceUsed;
                }


        /// <summary>
        /// Return space used by pass group
        /// </summary>
        /// <param name="groupID">Group id</param>
        /// <param name="domainID">domain id</param>
        /// <param name="userID">user id</param>
        /// <returns>space used</returns>
		public long SpaceUsedByGroup( string groupID, string domainID, string userID)
		{

			// call discovery to get the disk space used by this group (through catalog)
			return GetSpaceUsedByAllGroupMembers(groupID); 
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
