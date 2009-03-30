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
*        < iFolderLimit Policy Implementation >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.IO;

using Simias;
using Simias.Storage;

namespace Simias.Policy
{
	/// <summary>
	/// Implements the limit no of ifolder policy.
	/// </summary>
	public class iFolderLimit 
	{
		#region Class Members
		/// <summary>
		/// Well known name for the ifolder limit  policy.
		/// </summary>
		static public string iFolderLimitPolicyID = "e34e0a4a-d867-4bd0-9f35-v5a4cbe5f237";

		/// <summary>
		/// Well known name for the ifolder limit policy description.
		/// </summary>
		static public string iFolderLimitShortDescription = "iFolder limit";

		/// <summary>
		/// Policy object that contains the aggregate policy for the domain and member only.
		/// </summary>
		private Policy memberPolicy;

		/// <summary>
		/// Policy object that contains the aggregate policy including the collection limits.
		/// </summary>
		private Policy collectionPolicy = null;

		/// <summary>
		///  ifolder limit.
		/// </summary>
		private long limit = -1;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the ifolder limit.
		/// </summary>
		public long Limit
		{
			get { return limit; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of an object.
		/// </summary>
		/// <param name="member">Member that this file size filter is associated with.</param>
		private iFolderLimit( Member member )
		{
			PolicyManager pm = new PolicyManager();
			this.memberPolicy = pm.GetAggregatePolicy( iFolderLimitPolicyID, member );
			this.limit = GetUserAggregateLimit( memberPolicy );
		}

		/// <summary>
		/// Initializes a new instance of an object.
		/// </summary>
		/// <param name="collection">Collection that this file size filter is associated with.</param>
		private iFolderLimit( Collection collection )
		{
			PolicyManager pm = new PolicyManager();
			// In new implement. all policies are part of user object, so first get full member object
			Store store = Store.GetStore();
			string DomainID = Store.GetStore().DefaultDomain;
			Domain domain = store.GetDomain(DomainID);
			
			Member member = domain.GetMemberByID(collection.Owner.UserID);
			if(member == null)
			{
				// member should not be null , but if it is , then use old code.
				member = collection.Owner;
			}
			this.memberPolicy = pm.GetAggregatePolicy( iFolderLimitPolicyID, member );
			this.collectionPolicy = pm.GetAggregatePolicy( iFolderLimitPolicyID, member, collection );
			this.limit = GetAggregateLimit( collectionPolicy );
		}
		#endregion

		#region Factory Methods
		/// <summary>
		/// Creates a system wide iFolder Limit policy.
		/// </summary>
		/// <param name="domainID">Domain that the filter will be associated with.</param>
		/// <param name="limit">No of ifolders that all users in the domain will be limited to.</param>
		static public void Create( string domainID, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( iFolderLimitPolicyID, domainID );
			if ( limit >= 0 )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( iFolderLimitPolicyID, iFolderLimitShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rule.
					policy.DeleteRule( iFolderLimit.GetRule( policy ) );
				}

				// Add the new rule and save the policy.
				policy.AddRule( new Rule( limit, Rule.Operation.Greater, Rule.Result.Deny ) );
				pm.CommitPolicy( policy, domainID );
			}
			else if ( policy != null )
			{
				// Setting the limit to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a iFolder Limit policy for the specified member.
		/// </summary>
		/// <param name="member">Member that the filter will be associated with.</param>
		/// <param name="limit">No of ifolders that all users in the domain will be limited to.</param>
		static public void Create( Member member, long limit )
		{
			ICSList ftList = new ICSList();

			// Need a policy manager.
			PolicyManager pm = new PolicyManager();

			if ( limit >= 0 || limit == -2 )
			{
				ftList.Add( new Rule( limit, Rule.Operation.Greater, Rule.Result.Deny ));
			
				pm.CommitPolicy(iFolderLimitPolicyID, ftList, member);
			}
			else
			{
				pm.DeletePolicy( iFolderLimitPolicyID, member );
			}
		}

		/// <summary>
		/// Creates a iFolder Limit policy for the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		/// <param name="limit">No of ifolders  that all users in the domain will be limited to.</param>
		static public void Create( Collection collection, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( iFolderLimitPolicyID, collection );
			if ( limit >= 0 )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( iFolderLimitPolicyID, iFolderLimitShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rules.
					policy.DeleteRule( iFolderLimit.GetRule( policy ) );
				}

				// Add the new rules and save the policy.
				policy.AddRule( new Rule( limit, Rule.Operation.Greater, Rule.Result.Deny ) );
				pm.CommitPolicy( policy, collection );
			}
			else if ( policy != null )
			{
				// Setting the limit to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a iFolder Limit policy for the current user on the current machine.
		/// </summary>
		/// <param name="limit">No of ifolders that all users in the domain will be limited to.</param>
		static public void Create( long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( iFolderLimitPolicyID );
			if ( limit >= 0 )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( iFolderLimitPolicyID, iFolderLimitShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rules.
					policy.DeleteRule( iFolderLimit.GetRule( policy ) );
				}

				// Add the new rules and save the policy.
				policy.AddRule( new Rule( limit, Rule.Operation.Greater, Rule.Result.Deny ) );
				pm.CommitLocalMachinePolicy( policy );
			}
			else if ( policy != null )
			{
				// Setting the limit to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a system wide iFolder Limit policy.
		/// </summary>
		/// <param name="domainID">Domain that the filter will be associated with.</param>
		static public void Delete( string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( iFolderLimitPolicyID, domainID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a iFolderLimit policy for the specified member.
		/// </summary>
		/// <param name="member">Member that the filter will be associated with.</param>
		static public void Delete( Member member )
		{	
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			// Delete the policy.
			pm.DeletePolicy(iFolderLimitPolicyID, member);
		}

		/// <summary>
		/// Deletes a iFolderLimit for the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		static public void Delete( Collection collection )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( iFolderLimitPolicyID, collection );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a iFolderLimit policy for the current user on the current machine.
		/// </summary>
		static public void Delete()
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( iFolderLimitPolicyID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Gets the aggregate iFolderLimit policy for the specified member.
		/// </summary>
		/// <param name="member">Member that filter is associated with.</param>
		/// <returns>A FileSizeFilter object that contains the policy for the specified member.</returns>
		static public iFolderLimit Get( Member member )
		{
			return new iFolderLimit( member );
		}

		/// <summary>
		/// Gets the aggregate iFolderLimit policy for the specified member and collection.
		/// </summary>
		/// <param name="member">Member that filter is associated with.</param>
		/// <param name="collection">Collection to add to the aggregate size policy.</param>
		/// <returns>A iFolderLimit object that contains the policy for the specified member.</returns>
		[ Obsolete( "This method is obsolete. Please use iFolderLimit.Get( Collection collection ) instead.", false ) ]
		static public iFolderLimit Get( Member member, Collection collection )
		{
			return iFolderLimit.Get( collection );
		}

		/// <summary>
		/// Gets the aggregate iFolderLimit policy for the specified member and collection.
		/// </summary>
		/// <param name="collection">Collection to add to the aggregate size policy.</param>
		/// <returns>A iFolderLimit object that contains the policy for the specified member.</returns>
		static public iFolderLimit Get( Collection collection )
		{
			return new iFolderLimit( collection );
		}

		/// <summary>
		/// Gets the iFolderLimit associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the filter is associated with.</param>
		/// <returns>No of ifolders that all users in the domain are limited to.</returns>
		static public long GetLimit( string domainID )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( iFolderLimitPolicyID, domainID );
			return ( policy != null ) ? ( long )GetRule( policy ).Operand : -1;
		}

		/// <summary>
		/// Gets the iFolderLimit associated with the specified member.
		/// </summary>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <returns>No of ifolders that all users in the domain are limited to.</returns>
		static public long GetLimit( Member member )
		{
			PolicyManager pm = new PolicyManager();
                        Policy policy = pm.GetPolicy( iFolderLimitPolicyID, member );
                        return ( policy != null ) ? ( long )GetUserRule( policy ).Operand : -1;
		}

		/// <summary>
		/// Gets the iFolderLimit associated with the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the limit is associated with.</param>
		/// <returns>No of ifolders that all users in the domain are limited to.</returns>
		static public long GetLimit( Collection collection )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( iFolderLimitPolicyID, collection );
			return ( policy != null ) ? ( long )GetRule( policy ).Operand : -1;
		}

		/// <summary>
		/// Gets the iFolderLimit associated with the current user on the current machine.
		/// </summary>
		/// <returns>No of ifolders that all users in the domain are limited to.</returns>
		static public long GetLimit()
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( iFolderLimitPolicyID );
			return ( policy != null ) ? ( long )GetRule( policy ).Operand : -1;
		}

		/// <summary>
		/// Sets the iFolderLimit associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the filter is associated with.</param>
		/// <param name="limit">NO if ifolders that all users in the domain will be limited to.</param>
		static public void Set( string domainID, long limit )
		{
			Create( domainID, limit );
		}

		/// <summary>
		/// Sets the iFolderLimit associated with the specified member.
		/// </summary>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <param name="limit">No of ifolders that all users in the domain will be limited to.</param>
		static public void Set( Member member, long limit )
		{
			Create( member, limit );
		}

		/// <summary>
		/// Sets the iFolderLimit associated with the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter is associated with.</param>
		/// <param name="limit">No of ifolders that all users in the domain will be limited to.</param>
		static public void Set( Collection collection, long limit )
		{
			Create( collection, limit );
		}

		/// <summary>
		/// Sets the iFolderLimit associated with the current user on the current machine.
		/// </summary>
		/// <param name="limit">No of ifolders that all users in the domain will be limited to.</param>
		static public void Set( long limit )
		{
			Create( limit );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the aggregate file iFolderLimit for the specified policy.
		/// </summary>
		/// <param name="policy">Policy to get the iFolderLimit file size filter limit from.</param>
		/// <returns>The aggregate iFolderLimit.</returns>
		private long GetAggregateLimit( Policy policy )
		{
			// Set to no limit.
			long limit = -1;

			// If there is a policy find the most restrictive limit.
			if ( policy != null )
			{
				foreach ( Rule rule in policy.Rules )
				{
					long ruleLimit = ( long )rule.Operand;
					if ( ( limit == -1 ) || ( ruleLimit < limit ) )
					{
						limit = ruleLimit;
					}
				}
			}

			return limit;
		}

		/// <summary>
		/// Gets the aggregate file iFolderLimit for the specified policy.
		/// </summary>
		/// <param name="policy">Policy to get the iFolderLimit file size filter limit from.</param>
		/// <returns>The aggregate iFolderLimit.</returns>
		private long GetUserAggregateLimit( Policy policy )
		{
			// Set to no limit.
			long limit = -1;

			// If there is a policy find the most restrictive limit.
			if ( policy != null )
			{
				foreach ( Rule rule in policy.Rules)
				{
					long ruleLimit = ( long )rule.Operand;
					if ( ( limit == -1 ) || ( ruleLimit < limit ) )
					{
						limit = ruleLimit;
					}
				}
			}

			return limit;
		}

		/// <summary> 
		/// Gets the iFolderLimit rule for the specified policy.
		/// </summary>
		/// <param name="policy">Policy to retrieve the  iFolderLimit.</param>
		/// <returns>The iFolderLimit Rule from the policy.</returns>
		static private Rule GetRule( Policy policy )
		{
			// There should only be one rule in the iFolderLimit.
			IEnumerator e = policy.Rules.GetEnumerator();
			if ( !e.MoveNext() )
			{
				throw new SimiasException( "No policy rule on iFolder Limit." );
			}

			return e.Current as Rule;
		}

		/// <summary>
                /// Gets the iFolder limit rule for the specified policy.
                /// </summary>
                /// <param name="policy">Policy to retrieve the quota rule from.</param>
                /// <returns>The quota Rule from the policy.</returns>
                static private Rule GetUserRule( Policy policy )
                {
                        // There should only be one rule in the iFolderLimit policy.
                        IEnumerator e = policy.Rules.GetEnumerator();
                        if ( !e.MoveNext() )
                        {
				throw new SimiasException( "No policy rule on iFolder Limit" );
                        }

                        return e.Current as Rule;
                }


		#endregion

	}
}
