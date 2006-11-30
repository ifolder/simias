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
 *  Author: Ramesh Sunder<sramesh@novell.com>
 *
 ***********************************************************************/

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
		static private readonly string StatusTag = "Encrypt";

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
		/// <param name="interval">Sync interval in seconds that all users in the domain will be set to.</param>
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
		/// <param name="domainID">Domain that the interval will be associated with.</param>
		/// <param name="interval">Sync interval in seconds that all users in the domain will be set to.</param>
		static public void Create( Member member, int status )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( EncryptionStatePolicyID,  member);
			if ( ( status >= 0 ) )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( EncryptionStatePolicyID, EncryptionStateShortDescription );
				}

				// Add the new value and save the policy.
				policy.AddValue( StatusTag, status );
				pm.CommitPolicy( policy, member );
			}
			else if ( policy != null )
			{
				// Setting the interval to zero is the same as deleting the policy.
				pm.DeletePolicy( policy );
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


		static public void Delete( Member member )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( EncryptionStatePolicyID, member );
			if( policy != null)
			{
				pm.DeletePolicy( policy );
			}
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
		static public int GetStatus( Member member )
		{	
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( EncryptionStatePolicyID, member );
			return ( policy != null ) ? (int) policy.GetValue( StatusTag ) : 0;
		}
		#endregion
	}
}
