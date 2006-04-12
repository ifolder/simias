/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;

using Simias;
using Simias.Storage;

namespace Simias.Policy
{
	/// <summary>
	/// Implements the file size filter policy.
	/// </summary>
	public class FileSizeFilter
	{
		#region Class Members
		/// <summary>
		/// Well known name for the file size filter policy.
		/// </summary>
		static public string FileSizeFilterPolicyID = "e33e0a4a-d272-4bd0-9f35-b5a4cbe5f237";

		/// <summary>
		/// Well known name for the file size filter policy description.
		/// </summary>
		static public string FileSizeFilterShortDescription = "File size filter";

		/// <summary>
		/// Policy object that contains the aggregate policy for the domain and member only.
		/// </summary>
		private Policy memberPolicy;

		/// <summary>
		/// Policy object that contains the aggregate policy including the collection limits.
		/// </summary>
		private Policy collectionPolicy = null;

		/// <summary>
		///  File size filter limit.
		/// </summary>
		private long limit = 0;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the file size limit.
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
		private FileSizeFilter( Member member )
		{
			PolicyManager pm = new PolicyManager();
			this.memberPolicy = pm.GetAggregatePolicy( FileSizeFilterPolicyID, member );
			this.limit = GetAggregateLimit( memberPolicy );
		}

		/// <summary>
		/// Initializes a new instance of an object.
		/// </summary>
		/// <param name="collection">Collection that this file size filter is associated with.</param>
		private FileSizeFilter( Collection collection )
		{
			PolicyManager pm = new PolicyManager();
			Member member = collection.Owner;
			this.memberPolicy = pm.GetAggregatePolicy( FileSizeFilterPolicyID, member );
			this.collectionPolicy = pm.GetAggregatePolicy( FileSizeFilterPolicyID, member, collection );
			this.limit = GetAggregateLimit( collectionPolicy );
		}
		#endregion

		#region Factory Methods
		/// <summary>
		/// Creates a system wide file size filter policy.
		/// </summary>
		/// <param name="domainID">Domain that the filter will be associated with.</param>
		/// <param name="limit">Size of file in bytes that all users in the domain will be limited to.</param>
		static public void Create( string domainID, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, domainID );
			if ( limit > 0 )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( FileSizeFilterPolicyID, FileSizeFilterShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rule.
					policy.DeleteRule( FileSizeFilter.GetRule( policy ) );
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
		/// Creates a file size filter policy for the specified member.
		/// </summary>
		/// <param name="member">Member that the filter will be associated with.</param>
		/// <param name="limit">Size of file in bytes that all users in the domain will be limited to.</param>
		static public void Create( Member member, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, member );
			if ( limit > 0 )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( FileSizeFilterPolicyID, FileSizeFilterShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rule.
					policy.DeleteRule( FileSizeFilter.GetRule( policy ) );
				}

				// Add the new rule and save the policy.
				policy.AddRule( new Rule( limit, Rule.Operation.Greater, Rule.Result.Deny ) );
				pm.CommitPolicy( policy, member );
			}
			else if ( policy != null )
			{
				// Setting the limit to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a file size filter policy for the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		/// <param name="limit">Size of file in bytes that all users in the domain will be limited to.</param>
		static public void Create( Collection collection, long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, collection );
			if ( limit > 0 )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( FileSizeFilterPolicyID, FileSizeFilterShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rules.
					policy.DeleteRule( FileSizeFilter.GetRule( policy ) );
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
		/// Creates a file size filter policy for the current user on the current machine.
		/// </summary>
		/// <param name="limit">Size of file in bytes that all users in the domain will be limited to.</param>
		static public void Create( long limit )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID );
			if ( limit > 0 )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( FileSizeFilterPolicyID, FileSizeFilterShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rules.
					policy.DeleteRule( FileSizeFilter.GetRule( policy ) );
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
		/// Deletes a system wide file size filter policy.
		/// </summary>
		/// <param name="domainID">Domain that the filter will be associated with.</param>
		static public void Delete( string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, domainID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a file size filter policy for the specified member.
		/// </summary>
		/// <param name="member">Member that the filter will be associated with.</param>
		static public void Delete( Member member )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, member );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a file size filter policy for the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		static public void Delete( Collection collection )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, collection );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a file size filter policy for the current user on the current machine.
		/// </summary>
		static public void Delete()
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Gets the aggregate file size filter policy for the specified member.
		/// </summary>
		/// <param name="member">Member that filter is associated with.</param>
		/// <returns>A FileSizeFilter object that contains the policy for the specified member.</returns>
		static public FileSizeFilter Get( Member member )
		{
			return new FileSizeFilter( member );
		}

		/// <summary>
		/// Gets the aggregate file size filter policy for the specified member and collection.
		/// </summary>
		/// <param name="member">Member that filter is associated with.</param>
		/// <param name="collection">Collection to add to the aggregate size policy.</param>
		/// <returns>A FileSizeFilter object that contains the policy for the specified member.</returns>
		[ Obsolete( "This method is obsolete. Please use FileSizeFilter.Get( Collection collection ) instead.", false ) ]
		static public FileSizeFilter Get( Member member, Collection collection )
		{
			return FileSizeFilter.Get( collection );
		}

		/// <summary>
		/// Gets the aggregate file size filter policy for the specified member and collection.
		/// </summary>
		/// <param name="collection">Collection to add to the aggregate size policy.</param>
		/// <returns>A FileSizeFilter object that contains the policy for the specified member.</returns>
		static public FileSizeFilter Get( Collection collection )
		{
			return new FileSizeFilter( collection );
		}

		/// <summary>
		/// Gets the file size limit associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the filter is associated with.</param>
		/// <returns>Size of files that all users in the domain are limited to.</returns>
		static public long GetLimit( string domainID )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, domainID );
			return ( policy != null ) ? ( long )GetRule( policy ).Operand : 0;
		}

		/// <summary>
		/// Gets the file size limit associated with the specified member.
		/// </summary>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <returns>Size of files that all users in the domain are limited to.</returns>
		static public long GetLimit( Member member )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, member );
			return ( policy != null ) ? ( long )GetRule( policy ).Operand : 0;
		}

		/// <summary>
		/// Gets the file size limit associated with the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the limit is associated with.</param>
		/// <returns>Size of files that all users in the domain are limited to.</returns>
		static public long GetLimit( Collection collection )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID, collection );
			return ( policy != null ) ? ( long )GetRule( policy ).Operand : 0;
		}

		/// <summary>
		/// Gets the file size limit associated with the current user on the current machine.
		/// </summary>
		/// <returns>Size of files that all users in the domain are limited to.</returns>
		static public long GetLimit()
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( FileSizeFilterPolicyID );
			return ( policy != null ) ? ( long )GetRule( policy ).Operand : 0;
		}

		/// <summary>
		/// Sets the file size limit associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the filter is associated with.</param>
		/// <param name="limit">Size of files that all users in the domain will be limited to.</param>
		static public void Set( string domainID, long limit )
		{
			Create( domainID, limit );
		}

		/// <summary>
		/// Sets the file size limit associated with the specified member.
		/// </summary>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <param name="limit">Size of files that all users in the domain will be limited to.</param>
		static public void Set( Member member, long limit )
		{
			Create( member, limit );
		}

		/// <summary>
		/// Sets the file size limit associated with the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter is associated with.</param>
		/// <param name="limit">Size of files that all users in the domain will be limited to.</param>
		static public void Set( Collection collection, long limit )
		{
			Create( collection, limit );
		}

		/// <summary>
		/// Sets the file size limit associated with the current user on the current machine.
		/// </summary>
		/// <param name="limit">Size of files that all users in the domain will be limited to.</param>
		static public void Set( long limit )
		{
			Create( limit );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the aggregate file size filter limit for the specified policy.
		/// </summary>
		/// <param name="policy">Policy to get the file size filter limit from.</param>
		/// <returns>The aggregate file size filter limit.</returns>
		private long GetAggregateLimit( Policy policy )
		{
			// Set to no limit.
			long limit = 0;

			// If there is a policy find the most restrictive limit.
			if ( policy != null )
			{
				foreach ( Rule rule in policy.Rules )
				{
					long ruleLimit = ( long )rule.Operand;
					if ( ( limit == 0 ) || ( ruleLimit < limit ) )
					{
						limit = ruleLimit;
					}
				}
			}

			return limit;
		}

		/// <summary>
		/// Gets the file size rule for the specified policy.
		/// </summary>
		/// <param name="policy">Policy to retrieve the file size rule from.</param>
		/// <returns>The file size Rule from the policy.</returns>
		static private Rule GetRule( Policy policy )
		{
			// There should only be one rule in the file size policy.
			IEnumerator e = policy.Rules.GetEnumerator();
			if ( !e.MoveNext() )
			{
				throw new SimiasException( "No policy rule on file size." );
			}

			return e.Current as Rule;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns whether the specified file size is allowed to pass through the filter.
		/// </summary>
		/// <param name="fileSize">Size in bytes of a file.</param>
		/// <returns>True if the file size is allowed to pass through the filter. Otherwise false is returned.</returns>
		public bool Allowed( long fileSize )
		{
			bool isAllowed = true;

			// Check the overall domain/member policy first to make sure that the
			// file size doesn't exceed the limit.
			if ( memberPolicy != null )
			{
				// Apply the rule to see if there is space available.
				isAllowed = ( memberPolicy.Apply( fileSize ) == Rule.Result.Allow );
			}

			// See if there is a collection policy that limits the size of a file in the collection.
			if ( ( collectionPolicy != null ) && isAllowed )
			{
				// Apply the rule to see if the file size exceeds the policy on the collection.
				isAllowed = ( collectionPolicy.Apply( fileSize ) == Rule.Result.Allow );
			}

			return isAllowed;
		}
		#endregion
	}
}
