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
*                 $Author: Ravi Kumar <rkumar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        < Sharing Policy Implementation >
*
*
*******************************************************************************/


using System;

using Simias;
using Simias.Storage;

namespace Simias.Policy
{
	/// <summary>
	/// Implements the synchronization interval policy.
	/// </summary>
	public class Sharing
	{
		#region Class Members
		
		/// <summary>
                /// enum value to denote different combinations of Disabling/Enabling options
                /// </summary>
		public enum Share
		{
			/// <summary>
	                /// 
        	        /// </summary>
			Sharing = 1,
			/// <summary>
	                /// 
        	        /// </summary>
			EnforcedSharing = 4,
			/// <summary>
	                /// 
        	        /// </summary>
			DisableSharing = 8
		}

		/// <summary>
	        /// 
                /// </summary>
		public enum HigherPriority
		{
			/// <summary>
	                /// 
        	        /// </summary>
			System = 1,
			/// <summary>
	                /// 
        	        /// </summary>
			User = 2,
			/// <summary>
	                /// 
        	        /// </summary>
			Group = 4
		}


		
		/// <summary>
		/// Used to log messages.
		/// </summary>
		//static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );
		
		/// <summary>
		/// Well known name for the Sharing policy.
		/// </summary>
		static public readonly string SharingPolicyID = "bxd14cb3-f393-40db-a943-14a0c3275f2y";

		/// <summary>
		/// Well known name for the Sharing policy description.
		/// </summary>
		static public readonly string SharingShortDescription = "Sharing Policy Setting";

		/// <summary>
		/// Tag used to lookup and store the Disable sharing value.
		/// </summary>
		static public readonly string SharingStatusTag = "Sharing";

/*		/// <summary>
		/// Implies to never synchronize.
		/// </summary>
//		static public readonly int InfiniteSyncInterval = -1;*/

		/// <summary>
		/// Used to hold the aggregate policy.
		/// </summary>
		private Policy policy;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the sync interval in seconds. If the policy is aggregated, the largest
		/// sync interval will be returned.
		/// </summary>
		/// 
		public int Status
		{
			get
			{
				int val=0;
				if( policy != null )
				{
					val = (int) policy.GetValue( SharingStatusTag );
				}
				return val;
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="policy">The aggregate policy. This may be null if no policy exists.</param>
		private Sharing( Policy policy )
		{
			this.policy = policy;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Creates a system wide Disable Sharing policy.
		/// </summary>
		/// <param name="domainID">Domain that the interval will be associated with.</param>
		/// <param name="status">value of the disable sharing enumerator</param>
		static public void Create( string domainID, int status )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SharingPolicyID, domainID );
			if ( ( status >= 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( SharingPolicyID, SharingShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( SharingStatusTag, status );
				pm.CommitPolicy( policy, domainID );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}


		/// <summary>
		/// Creates a User level Sharing policy.
		/// </summary>
		/// <param name="member">member that the policy will be associated with.</param>
		/// <param name="status">value of the Sharing enumerator</param>
		static public void Create( Member member, int status )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			if ( ( status >= 0 ) )
			{
				pm.CommitPolicy( SharingPolicyID, status, member);
			}
			else
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( SharingPolicyID, member );
			}
		}

		/// <summary>
		/// Creates a iFolder level  Sharing policy.
		/// </summary>
		/// <param name="collection">collection that the interval will be associated with.</param>
		/// <param name="status">value of the Sharing enumerator</param>
		static public void Create( Collection collection, int status )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SharingPolicyID,  collection);
			if ( ( status >= 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( SharingPolicyID, SharingShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( SharingStatusTag, status );
				pm.CommitPolicy( policy, collection );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		

		/// <summary>
		/// Deletes a system wide Sharing policy.
		/// </summary>
		/// <param name="domainID">Domain that the policy will be associated with.</param>
		static public void Delete( string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( SharingPolicyID, domainID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}


		/// <summary>
		/// Deletes given member
		/// </summary>
		/// <param name="member">member for which policy need to be deleted.</param>
		static public void Delete( Member member )
		{
			PolicyManager pm = new PolicyManager();
			pm.DeletePolicy(SharingPolicyID, member);
		}

		/// <summary>
		/// Sets the sharing policy associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the sharing policy is associated with.</param>
		/// <param name="status">enum value to denote the Disable sharing</param>
		static public void Set( string domainID, int status )
		{
			Create( domainID, status );
			
		}

		/// <summary>
		/// Sets the sharing policy associated with the specified member.
		/// </summary>
		/// <param name="member">member that the  sharing policy is associated with.</param>
		/// <param name="status">enum value to denote the sharing</param>
		static public void Set( Member member, int status )
		{
			Create( member, status );
			
		}

		/// <summary>
		/// Sets the Disable sharing policy associated with the specified collection.
		/// </summary>
		/// <param name="collection">collection that the sharing policy is associated with.</param>
		/// <param name="status">enum value to denote the sharing</param>
		static public void Set( Collection collection, int status )
		{
			Create( collection, status );
			
		}

		/// <summary>
		/// Gets the status of sharing associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the interval is associated with.</param>
		/// <returns>the status of Sharing as an int</returns>
		static public int GetStatus( string domainID )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( SharingPolicyID, domainID );
			return ( policy != null ) ? ( int )policy.GetValue( SharingStatusTag ) : 0;
		}
		
		/// <summary>
		/// Gets the status of sharing associated with the specified member.
		/// </summary>
		/// <param name="member">member for whom policy is sought</param>
		/// <returns>the status of Sharing as an int</returns>
		static public int GetStatus( Member member )
		{	
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( SharingPolicyID, member );
			return ( policy != null ) ? (int) policy.GetValue( SharingStatusTag ) : 0;
		}
		
		/// <summary>
		/// Gets the status of sharing associated with the specified collection.
		/// </summary>
		/// <param name="collection">collection for which policy is sought</param>
		/// <returns>the status of Sharing as an int</returns>
		static public int GetStatus( Collection collection )
		{	
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( SharingPolicyID, collection );
			return ( policy != null ) ? (int) policy.GetValue( SharingStatusTag ) : 0;
		}

        /// <summary>
        /// Gets the aggregate status of sharing associated with the specified collection, its owner, group,system
        /// </summary>
        /// <param name="collection">collection for which policy is sought</param>
        /// <returns>the status of Sharing as an int</returns>
        static public bool IsSharingAllowedForCollection(Collection collection)
        {
            int iFolderSharingStatus = GetStatus(collection);
            Store store = Store.GetStore();
            string DomainID = Store.GetStore().DefaultDomain;
            Domain domain = store.GetDomain(DomainID);
            Member member = domain.GetMemberByID(collection.Owner.UserID);
            int OwnerAndAboveAggregateSharingStatus = Get(member);

            if (((OwnerAndAboveAggregateSharingStatus & (int)Share.EnforcedSharing) == (int)Share.EnforcedSharing))
            {
                // If on system level or user level, enforcement of policy is there, it means the iFolder must not be shared
                if ((OwnerAndAboveAggregateSharingStatus & (int)Share.Sharing) == (int)Share.Sharing)
                    return true;
                return false;
            }
            if (iFolderSharingStatus != 0)
            {
                if ((iFolderSharingStatus & (int)Share.Sharing) == (int)Share.Sharing)
                {
                    // it means, on iFolder Details page, admin had unchecked the box so sharing is enabled now
                    return true;
                }
                if ((iFolderSharingStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                {
                    // it means, on iFolder Details page, admin had checked the box so sharing is disabled
                    return false;
                }
            }
            else
            {
                if ((OwnerAndAboveAggregateSharingStatus & (int)Share.Sharing) == (int)Share.Sharing)
                {
                    return true;
                }
                if ((OwnerAndAboveAggregateSharingStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                {
                    return false;
                }
            }
            return true;

        }

		/// <summary>
                /// Gets the aggregate security state policy for the specified member (includes system and group level aggregation)
                /// </summary>
                /// <param name="member">Member that policy is associated with.</param>
                /// <returns>A SecurityState object that contains the policy for the specified member.</returns>
                static public int Get( Member member )
                {
                        int SharingVal = 0;

                        Store store = Store.GetStore();
                        string DomainID = Store.GetStore().DefaultDomain;
                        Domain domain = store.GetDomain(DomainID);

			int SystemStatus = 0;
			int GroupStatus = 0;
                        int UserStatus = GetStatus(member);
			

                        string[] GIDs = domain.GetMemberFamilyList(member.UserID);
                        foreach(string gid in GIDs)
			{
				if(gid != member.UserID)
				{
					Member GroupAsMember = domain.GetMemberByID(gid);
					GroupStatus = GetStatus(GroupAsMember);
					if(GroupStatus !=0)
					{
						break;
					}
				}
			}
	
			SystemStatus = GetStatus(DomainID);
		
			int DerivePriority = DeriveStatus(SystemStatus, GroupStatus, UserStatus);
			SharingVal = ( (int)(DerivePriority & (int)HigherPriority.System) == (int) HigherPriority.System ) ?
						SystemStatus : ( (int) (DerivePriority & (int)HigherPriority.Group) == (int) HigherPriority.Group ) ? 
								GroupStatus : UserStatus ;

                        return SharingVal;
                }


		///<summary>
                /// Get the policy for an iFolder.
                /// </summary>
                /// <param name="system">system information.</param>
                /// <param name="group">group name.</param>
                /// <param name="user">user name.</param>
                /// <returns>returns drive status.</returns>			
                static public int DeriveStatus(int system, int group, int user)
                {

                        //check for enforcement of policy on system,group or user level
                        bool SystemEnforcement = ( (system & (int)Share.EnforcedSharing) == (int)Share.EnforcedSharing );
                        bool GroupEnforcement = ( (group & (int)Share.EnforcedSharing) == (int)Share.EnforcedSharing );
                        //bool UserEnforcement = ( (user & (int)Share.EnforcedSharing) == (int)Share.EnforcedSharing );

                        if( SystemEnforcement)
                        {
				return (int)HigherPriority.System;
                        }
			else
                        if(GroupEnforcement)
                        {
				return (int)HigherPriority.Group;
                        }
			
			if(user == 0)
			{
				if(group == 0)
				{
					return (int)HigherPriority.System;
				}
				else
				{
					return (int)HigherPriority.Group;
				}
			}
			
			return (int)HigherPriority.User;
		}



		#endregion
	}
}
