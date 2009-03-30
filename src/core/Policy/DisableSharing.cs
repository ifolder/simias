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
*                 $Author: Anil Kumar <kuanil@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Sharing Policy Implementation >
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
	public class DisableSharing
	{
		#region Class Members
		
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );
		
		/// <summary>
		/// Well known name for the Disable Sharing policy.
		/// </summary>
		static public readonly string DisableSharingPolicyID = "bxd14cb3-f393-40db-a943-14a0c3275f2y";

		/// <summary>
		/// Well known name for the Disable Sharing policy description.
		/// </summary>
		static public readonly string DisableSharingShortDescription = "Security Policy Setting";

		/// <summary>
		/// Tag used to lookup and store the Disable sharing value.
		/// </summary>
		static private readonly string DisableSharingStatusTag = "DisableSharing";

		/// <summary>
		/// Implies to never synchronize.
		/// </summary>
//		static public readonly int InfiniteSyncInterval = -1;

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
					val = (int) policy.GetValue( DisableSharingStatusTag );
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
		private DisableSharing( Policy policy )
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
			Policy policy = pm.GetPolicy( DisableSharingPolicyID, domainID );
			if ( ( status >= 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( DisableSharingPolicyID, DisableSharingShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( DisableSharingStatusTag, status );
				pm.CommitPolicy( policy, domainID );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}


		/// <summary>
		/// Creates a User level Disable Sharing policy.
		/// </summary>
		/// <param name="member">member that the policy will be associated with.</param>
		/// <param name="status">value of the Disable Sharing enumerator</param>
		static public void Create( Member member, int status )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DisableSharingPolicyID,  member);
			if ( ( status >= 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( DisableSharingPolicyID, DisableSharingShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( DisableSharingStatusTag, status );
				pm.CommitPolicy( policy, member, DisableSharingStatusTag );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a iFolder level Disable Sharing policy.
		/// </summary>
		/// <param name="collection">collection that the interval will be associated with.</param>
		/// <param name="status">value of the Disable Sharing enumerator</param>
		static public void Create( Collection collection, int status )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DisableSharingPolicyID,  collection);
			if ( ( status >= 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( DisableSharingPolicyID, DisableSharingShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( DisableSharingStatusTag, status );
				pm.CommitPolicy( policy, collection );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		

		/// <summary>
		/// Deletes a system wide Disable Sharing policy.
		/// </summary>
		/// <param name="domainID">Domain that the policy will be associated with.</param>
		static public void Delete( string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DisableSharingPolicyID, domainID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}


		static public void Delete( Member member )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( DisableSharingPolicyID, member );
			if( policy != null)
			{
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Sets the Disable sharing policy associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the Disable sharing policy is associated with.</param>
		/// <param name="status">enum value to denote the Disable sharing</param>
		static public void Set( string domainID, int status )
		{
			Create( domainID, status );
			
		}

		/// <summary>
		/// Sets the Disable sharing policy associated with the specified member.
		/// </summary>
		/// <param name="member">member that the Disable sharing policy is associated with.</param>
		/// <param name="status">enum value to denote the Disable sharing</param>
		static public void Set( Member member, int status )
		{
			Create( member, status );
			
		}

		/// <summary>
		/// Sets the Disable sharing policy associated with the specified collection.
		/// </summary>
		/// <param name="collection">collection that the Disable sharing policy is associated with.</param>
		/// <param name="status">enum value to denote the Disable sharing</param>
		static public void Set( Collection collection, int status )
		{
			Create( collection, status );
			
		}

		/// <summary>
		/// Gets the status of Disable sharing associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the interval is associated with.</param>
		/// <returns>the status of Disable Sharing as an int</returns>
		static public int GetStatus( string domainID )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( DisableSharingPolicyID, domainID );
			return ( policy != null ) ? ( int )policy.GetValue( DisableSharingStatusTag ) : 0;
		}
		
		/// <summary>
		/// Gets the status of Disable sharing associated with the specified member.
		/// </summary>
		/// <param name="member">member for whom policy is sought</param>
		/// <returns>the status of Disable Sharing as an int</returns>
		static public int GetStatus( Member member )
		{	
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( DisableSharingPolicyID, member );
			return ( policy != null ) ? (int) policy.GetValue( DisableSharingStatusTag ) : 0;
		}
		
		/// <summary>
		/// Gets the status of Disable sharing associated with the specified collection.
		/// </summary>
		/// <param name="collection">collection for which policy is sought</param>
		/// <returns>the status of Disable Sharing as an int</returns>
		static public int GetStatus( Collection collection )
		{	
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( DisableSharingPolicyID, collection );
			return ( policy != null ) ? (int) policy.GetValue( DisableSharingStatusTag ) : 0;
		}
		#endregion
	}
}
