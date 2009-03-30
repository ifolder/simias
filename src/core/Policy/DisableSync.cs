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
*        <Sync Policy Implementation >
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
	public class DisableSync
	{
		#region Class Members
		
		/// <summary>
		/// Well known name for the Disable Sync policy.
		/// </summary>
		static public readonly string DisableSyncPolicyID = "afd14cb3-f174-40db-a474-14a0c3275f3h";

		/// <summary>
		/// Well known name for the Disable Sync policy description.
		/// </summary>
		static public readonly string DisableSyncShortDescription = "Sync Policy Setting";

		/// <summary>
		/// Tag used to lookup and store the Disable sync value.
		/// </summary>
		static private readonly string DisableSyncStatusTag = "DisableSync";

		/// <summary>
		/// Used to hold the aggregate policy.
		/// </summary>
		private Policy policy;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object.
		/// </summary>
		/// <param name="policy">The aggregate policy. This may be null if no policy exists.</param>
		private DisableSync( Policy policy )
		{
			this.policy = policy;
		}
		#endregion

                #region Public Methods
		/// <summary>
		/// Creates a iFolder level Disable Sync policy.
		/// </summary>
		/// <param name="collection">collection that the interval will be associated with.</param>
		/// <param name="status">value of the Disable Sync </param>
		static public void Create( Collection collection, int status )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( DisableSyncPolicyID,  collection);
			if ( ( status == 1 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( DisableSyncPolicyID, DisableSyncShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( DisableSyncStatusTag, status );
				pm.CommitPolicy( policy, collection );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}


		/// <summary>
		/// Sets the Disable sync policy associated with the specified collection.
		/// </summary>
		/// <param name="collection">collection that the Disable sync policy is associated with.</param>
		/// <param name="status">enum value to denote the Disable sync</param>
		static public void Set( Collection collection, int status )
		{
			Create( collection, status );
		}

		
		/// <summary>
		/// Gets the status of Disable sync associated with the specified collection.
		/// </summary>
		/// <param name="collection">collection for which policy is sought</param>
		/// <returns>the status of Disable Sync as an int</returns>
		static public int GetStatus( Collection collection )
		{	
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( DisableSyncPolicyID, collection );
			return ( policy != null ) ? (int) policy.GetValue( DisableSyncStatusTag ) : 0;
		}
		#endregion

	}
}
