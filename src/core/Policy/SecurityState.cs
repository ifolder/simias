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
*                 $Author: Ramesh Sunder<sramesh@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        < Encryption Policy Implementation >
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
	public class SecurityState
	{
		#region Class Members
		/// <summary>
		/// Well known name for the sync interval policy.
		/// </summary>
		static public readonly string EncryptionStatePolicyID = "bdd14cb3-f323-40cb-a948-44a0c3275f2f";

		/// <summary>
		/// Well known name for the sync interval policy description.
		/// </summary>
		static public readonly string EncryptionStateShortDescription = "Security Policy Setting";

		/// <summary>
		/// Tag used to lookup and store the interval value on the policy.
		/// </summary>
		static public readonly string StatusTag = "Encrypt";

	/*	/// <summary>
		/// Implies to never synchronize.
		/// </summary>
//		static public readonly int InfiniteSyncInterval = -1; */

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
					val = (int) policy.GetValue( StatusTag );
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
		private SecurityState( Policy policy )
		{
			this.policy = policy;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Creates a system wide Encryption policy.
		/// </summary>
		/// <param name="domainID">Domain that the interval will be associated with.</param>
		/// <param name="status">status.</param>
		static public void Create( string domainID, int status )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( EncryptionStatePolicyID, domainID );
			if ( ( status >= 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( EncryptionStatePolicyID, EncryptionStateShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( StatusTag, status );
				pm.CommitPolicy( policy, domainID );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}


		/// <summary>
		/// Creates a User level Encryption policy.
		/// </summary>
		/// <param name="member">member.</param>
		/// <param name="status">status.</param>
		static public void Create( Member member, int status )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			if ( ( status >= 0 ) )
			{
				pm.CommitPolicy( EncryptionStatePolicyID, status, member);
			}
			else
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( EncryptionStatePolicyID, member );
			}
		}


		/// <summary>
		/// Deletes a system wide sync interval policy.
		/// </summary>
		/// <param name="domainID">Domain that the interval will be associated with.</param>
		static public void Delete( string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( EncryptionStatePolicyID, domainID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}


		/// <summary>
		/// Function to Delete given member.
		/// </summary>
		/// <param name="member">member to delete</param>
		static public void Delete( Member member )
		{
			PolicyManager pm = new PolicyManager();
			pm.DeletePolicy(EncryptionStatePolicyID, member);
		}

		/// <summary>
		/// Gets the interval associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the interval is associated with.</param>
		/// <returns>The sync interval that all users in the domain are limited to.</returns>
		static public int GetStatus( string domainID )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( EncryptionStatePolicyID, domainID );
			return ( policy != null ) ? ( int )policy.GetValue( StatusTag ) : 0;
		}

		/// <summary>
		/// Function to Delete given member.
		/// </summary>
	        /// <param name="member">member for which status need to be retrived.</param>
       		 /// <returns></returns>
		static public int GetStatus( Member member )
		{	
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( EncryptionStatePolicyID, member );
			return ( policy != null ) ? (int) policy.GetValue( StatusTag ) : 0;
		}

		/// <summary>
                /// Gets the aggregate security state policy for the specified member (includes system and group level aggregation)
                /// </summary>
                /// <param name="member">Member that policy is associated with.</param>
                /// <returns>A SecurityState object that contains the policy for the specified member.</returns>
                static public int Get( Member member )
                {
			int SecurityStatusVal = 0;

			Store store = Store.GetStore();
                        string DomainID = Store.GetStore().DefaultDomain;
                        Domain domain = store.GetDomain(DomainID);

			int MemberStatus = GetStatus(member);
			if(MemberStatus != 0)
			{
				SecurityStatusVal = MemberStatus;
			}
			else
			{
				string[] GIDs = domain.GetMemberFamilyList(member.UserID);
				foreach(string gid in GIDs)
				{
					if(gid != member.UserID)
					{
						Member GroupAsMember = domain.GetMemberByID(gid);
						int GroupSecurityStatus = GetStatus(GroupAsMember);
						if(GroupSecurityStatus !=0)
						{
							SecurityStatusVal = GroupSecurityStatus;
							break;
						}
					}
				}
				if(SecurityStatusVal == 0)
				{
					int SystemStatus = GetStatus(DomainID);
					if( SystemStatus != 0 )
					{
						SecurityStatusVal = SystemStatus ;
					}
				}
			}
                    
                        return SecurityStatusVal;
                }


		#endregion
	}
}
