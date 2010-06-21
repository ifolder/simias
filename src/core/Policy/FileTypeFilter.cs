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
*                 $Author: Mike Lasky <mlasky@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        < FileType Policy Implementation >
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
	/// List used to specify a filter list to the policy.
	/// </summary>
	public struct FileTypeEntry
	{
		#region Class Members

		/// <summary>
		/// File extension to add as filter.
		/// </summary>
		private string fileName;

		/// <summary>
		/// If true then file extension will be allowed to pass through the filter
		/// If false then file will be disallowed to pass through the filter.
		/// </summary>
		private bool allowed;

		/// <summary>
		/// If true then filter comparision will be case-insensitive.
		/// </summary>
		private bool ignoreCase;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the filter extension name.
		/// </summary>
		public string Name
		{
			get { return fileName; }
		}

		/// <summary>
		/// Gets whether filter extension is allowed or disallowed through the filter.
		/// </summary>
		public bool Allowed
		{
			get { return allowed; }
		}

		/// <summary>
		/// Gets whether the filter comparision will be case-insensitive.
		/// </summary>
		public bool IgnoreCase
		{
			get { return ignoreCase; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="fileName">Filename to use as a filter. Can be a regular expression.</param>
		/// <param name="allowed">If true then all files that have extensions that match the 
		/// fileNameExtension parameter will be allowed to pass through the filter.</param>
		public FileTypeEntry( string fileName, bool allowed ) :
			this ( fileName, allowed, false )
		{
		}

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="fileName">Filename to use as a filter. Can be a regular expression.</param>
		/// <param name="allowed">If true then all files that have extensions that match the 
		/// fileNameExtension parameter will be allowed to pass through the filter.</param>
		/// <param name="ignoreCase">If true filter comparision will be case-insensitive.</param>
		public FileTypeEntry( string fileName, bool allowed, bool ignoreCase )
		{
			this.fileName = fileName;
			this.allowed = allowed;
			this.ignoreCase = ignoreCase;
		}
		#endregion
	}

	/// <summary>
	/// Implements the file type filter policy.
	/// </summary>
	public class FileTypeFilter
	{
		#region Class Members
		
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );

		/// <summary>
		/// Well known name for the file type filter policy.
		/// </summary>
		static public string FileTypeFilterPolicyID = "e69ff680-3f75-412e-a929-1b0247ed4041";

		/// <summary>
		/// Well known name for the file type filter policy description.
		/// </summary>
		static public string FileTypeFilterShortDescription = "File type filter";

		/// <summary>
		/// Policy object that contains the aggregate policy for the domain and member only.
		/// </summary>
		private Policy memberPolicy;

		/// <summary>
		/// Policy object that contains the aggregate policy including the collection limits.
		/// </summary>
		private Policy collectionPolicy = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the file type filter list.
		/// </summary>
		public FileTypeEntry[] FilterList
		{
			get { return GetPatterns( ( collectionPolicy != null ) ? collectionPolicy : memberPolicy  ); }
		}

		/// <summary>
		/// Gets the file type filter list.
		/// </summary>
		public FileTypeEntry[] FilterUserList
		{
			get
			{
				if(collectionPolicy != null)
				{
					return GetPatterns( collectionPolicy);
				}
			
				return GetUserPatterns(memberPolicy);
				//get { return GetUserPatterns( ( collectionPolicy != null ) ? collectionPolicy : memberPolicy  ); }
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of an object.
		/// </summary>
		/// <param name="member">Member that this file type filter is associated with.</param>
		private FileTypeFilter( Member member )
		{
			PolicyManager pm = new PolicyManager();
			this.memberPolicy = pm.GetAggregatePolicy( FileTypeFilterPolicyID, member, true );
		}

		/// <summary>
		/// Initializes a new instance of an object.
		/// </summary>
		/// <param name="member">Member that this file type filter is associated with.</param>
		/// <param name="includeLocalMachinePolicy">Includes local machine policy.</param>
	        private FileTypeFilter( Member member, bool includeLocalMachinePolicy )
		{
			PolicyManager pm = new PolicyManager();
			this.memberPolicy = pm.GetAggregatePolicy( FileTypeFilterPolicyID, member, true, includeLocalMachinePolicy );
		}

		/// <summary>
		/// Initializes a new instance of an object.
		/// </summary>
		/// <param name="collection">Collection that this disk space quota is associated with.</param>
	        private FileTypeFilter( Collection collection ) :this( collection, true )
	        {
		}

		/// <summary>
		/// Initializes a new instance of an object.
		/// </summary>
		/// <param name="collection">Collection that this disk space quota is associated with.</param>
		/// <param name="includeLocalMachinePolicy">Includes local machine policy.</param>
	        private FileTypeFilter( Collection collection, bool includeLocalMachinePolicy )
		{
			PolicyManager pm = new PolicyManager();

			// In new implement. all policies are part of user object, so first get full member object
			Store store = Store.GetStore();
			string DomainID = Store.GetStore().DefaultDomain;
			Domain domain = store.GetDomain(DomainID);
				
			Member member = domain.GetMemberByID(collection.Owner.UserID);
			if(member == null)
			{
				// BUG BUG : If control comes here , then its a bug as member should exist in the default domain
				// member should not be null , but if it is , then use old code.
				member = collection.Owner;
			}
			this.memberPolicy = pm.GetAggregatePolicy( FileTypeFilterPolicyID, member, true, includeLocalMachinePolicy );
			
			this.collectionPolicy = pm.GetAggregatePolicy( FileTypeFilterPolicyID, member, collection, true, includeLocalMachinePolicy );
		}
		#endregion

		#region Factory Methods
		/// <summary>
		/// Creates a system wide file filter policy.
		/// </summary>
		/// <param name="domainID">Domain that the filter will be associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Create( string domainID, FileTypeEntry[] patterns )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, domainID );
			if ( patterns.Length > 0 )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( FileTypeFilterPolicyID, FileTypeFilterShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rules.
					foreach ( Rule r in policy.Rules )
					{
						policy.DeleteRule( r );
					}
				}

				// Add the new rules and save the policy.
				foreach( FileTypeEntry fte in patterns )
				{
					policy.AddRule( new Rule( fte.Name, fte.IgnoreCase ? Rule.Operation.RegExp_IgnoreCase : Rule.Operation.RegExp, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
				}

				pm.CommitPolicy( policy, domainID );
			}
			else if ( policy != null )
			{
				// An empty array is the same thing as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a file type filter policy for the specified member.
		/// </summary>
		/// <param name="member">Member that the filter will be associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Create( Member member, FileTypeEntry[] patterns )
		{
			ICSList ftList = new ICSList();

			// Need a policy manager.
			PolicyManager pm = new PolicyManager();

			if ( patterns.Length > 0 )
			{
				// Add the new rules and save the policy.
				foreach( FileTypeEntry fte in patterns )
				{
					ftList.Add( new Rule( fte.Name, fte.IgnoreCase ? Rule.Operation.RegExp_IgnoreCase : Rule.Operation.RegExp, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ));
				}
				
				pm.CommitPolicy(FileTypeFilterPolicyID, ftList, member);
			}
			else
			{
				pm.DeletePolicy( FileTypeFilterPolicyID, member );
			}
		}

		/// <summary>
		/// Creates a file type filter policy for the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Create( Collection collection, FileTypeEntry[] patterns )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, collection );
			if ( patterns.Length > 0 )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( FileTypeFilterPolicyID, FileTypeFilterShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rules.
					foreach ( Rule r in policy.Rules )
					{
						policy.DeleteRule( r );
					}
				}

				// Add the new rules and save the policy.
				foreach( FileTypeEntry fte in patterns )
				{
					policy.AddRule( new Rule( fte.Name, fte.IgnoreCase ? Rule.Operation.RegExp_IgnoreCase : Rule.Operation.RegExp, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
				}

				pm.CommitPolicy( policy, collection );
			}
			else if ( policy != null )
			{
				// An empty array is the same thing as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Creates a file type filter policy for the current user on the current machine.
		/// </summary>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Create( FileTypeEntry[] patterns )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID );
			if ( patterns.Length > 0 )
			{
				if ( policy == null )
				{
					// The policy does not exist, create a new one and add the rules.
					policy = new Policy( FileTypeFilterPolicyID, FileTypeFilterShortDescription );
				}
				else
				{
					// The policy already exists, delete the old rules.
					foreach ( Rule r in policy.Rules )
					{
						policy.DeleteRule( r );
					}
				}

				// Add the new rules and save the policy.
				foreach( FileTypeEntry fte in patterns )
				{
					policy.AddRule( new Rule( fte.Name, fte.IgnoreCase ? Rule.Operation.RegExp_IgnoreCase : Rule.Operation.RegExp, fte.Allowed ? Rule.Result.Allow : Rule.Result.Deny ) );
				}

				pm.CommitLocalMachinePolicy( policy );
			}
			else if ( policy != null )
			{
				// An empty array is the same thing as deleting the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a system wide file filter policy.
		/// </summary>
		/// <param name="domainID">Domain that the filter will be associated with.</param>
		static public void Delete( string domainID )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, domainID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a file type filter policy for the specified member.
		/// </summary>
		/// <param name="member">Member that the filter will be associated with.</param>
		static public void Delete( Member member )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			pm.DeletePolicy(FileTypeFilterPolicyID, member);
		}

		/// <summary>
		/// Deletes a file type filter policy for the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter will be associated with.</param>
		static public void Delete( Collection collection )
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, collection );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Deletes a file type filter policy for the current user on the current machine.
		/// </summary>
		static public void Delete()
		{
			// Need a policy manager.
			PolicyManager pm = new PolicyManager();
			
			// See if the policy already exists.
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID );
			if ( policy != null )
			{
				// Delete the policy.
				pm.DeletePolicy( policy );
			}
		}

		/// <summary>
		/// Gets the aggregate file type filter policy for the specified member.
		/// </summary>
		/// <param name="member">Member that filter is associated with.</param>
		/// <param name="includeLocalMachinePolicy">Includes local machine policy.</param>
		/// <returns>A FileTypeFilter object that contains the policy for the specified member.</returns>
	        static public FileTypeFilter Get( Member member, bool includeLocalMachinePolicy )
		{
		         return new FileTypeFilter( member,  includeLocalMachinePolicy);
		}

		/// <summary>
		/// Gets the aggregate file type filter policy for the specified member.
		/// </summary>
		/// <param name="member">Member that filter is associated with.</param>
		/// <returns>A FileTypeFilter object that contains the policy for the specified member.</returns>
		static public FileTypeFilter Get( Member member )
		{
			return new FileTypeFilter( member );
		}

		/// <summary>
		/// Gets the aggregate file type filter policy for the specified member and collection.
		/// </summary>
		/// <param name="member">Member that filter is associated with.</param>
		/// <param name="collection">Collection to add to the aggregate quota policy.</param>
		/// <returns>A FileTypeFilter object that contains the policy for the specified member.</returns>
		[ Obsolete( "This method is obsolete. Please use DiskSpaceQuota.Get( Collection collection ) instead.", false ) ]
		static public FileTypeFilter Get( Member member, Collection collection )
		{
			return FileTypeFilter.Get( collection );
		}

		/// <summary>
		/// Gets the aggregate file type filter policy for the specified member and collection.
		/// </summary>
		/// <param name="collection">Collection to add to the aggregate quota policy.</param>
		/// <returns>A FileTypeFilter object that contains the policy for the specified member.</returns>
		static public FileTypeFilter Get( Collection collection )
		{
			return new FileTypeFilter( collection );
		}

		/// <summary>
		/// Gets the aggregate file type filter policy for the specified member and collection.
		/// </summary>
		/// <param name="collection">Collection to add to the aggregate quota policy.</param>
		/// <param name="includeLocalMachinePolicy">Includes local machine policy.</param>
		/// <returns>A FileTypeFilter object that contains the policy for the specified member.</returns>
	        static public FileTypeFilter Get( Collection collection, bool includeLocalMachinePolicy )
		{
		    return new FileTypeFilter( collection, includeLocalMachinePolicy );
		}

		/// <summary>
		/// Gets the file type filter patterns associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the filter is associated with.</param>
		/// <returns>Array of file type filter patterns for this policy if successful. If there are no
		/// filter patterns then null is returned.</returns>
		static public FileTypeEntry[] GetPatterns( string domainID )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, domainID );
			return ( policy != null ) ? GetPatterns( policy ) : null;
		}

		/// <summary>
		/// Gets the file type filter patterns associated with the specified member.
		/// </summary>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <returns>Array of file type filter patterns for this policy if successful. If there are no
		/// filter patterns then null is returned.</returns>
		static public FileTypeEntry[] GetPatterns( Member member )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, member );
			return ( policy != null ) ? GetUserPatterns( policy ) : null;
		}

		/// <summary>
		/// Gets the file type filter patterns associated with the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the limit is associated with.</param>
		/// <returns>Array of file type filter patterns for this policy if successful. If there are no
		/// filter patterns then null is returned.</returns>
		static public FileTypeEntry[] GetPatterns( Collection collection )
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID, collection );
			return ( policy != null ) ? GetPatterns( policy ) : null;
		}

		/// <summary>
		/// Gets the file type filter patterns associated with the current user on the current machine.
		/// </summary>
		/// <returns>Array of file type filter patterns for this policy if successful. If there are no
		/// filter patterns then null is returned.</returns>
		static public FileTypeEntry[] GetPatterns()
		{
			PolicyManager pm = new PolicyManager();
			Policy policy = pm.GetPolicy( FileTypeFilterPolicyID );
			return ( policy != null ) ? GetPatterns( policy ) : null;
		}

		/// <summary>
		/// Sets the file type filter associated with the specified domain.
		/// </summary>
		/// <param name="domainID">Domain that the filter is associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Set( string domainID, FileTypeEntry[] patterns )
		{
			Create( domainID, patterns );
		}

		/// <summary>
		/// Sets the file type filter associated with the specified member.
		/// </summary>
		/// <param name="member">Member that the filter is associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Set( Member member, FileTypeEntry[] patterns )
		{
			Create( member, patterns );
		}

		/// <summary>
		/// Sets the file type filter associated with the specified collection.
		/// </summary>
		/// <param name="collection">Collection that the filter is associated with.</param>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Set( Collection collection, FileTypeEntry[] patterns )
		{
			Create( collection, patterns );
		}

		/// <summary>
		/// Sets the file type filter associated with the current user on the current machine.
		/// </summary>
		/// <param name="patterns">File type patterns that will be used to filter files.</param>
		static public void Set( FileTypeEntry[] patterns )
		{
			Create( patterns );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the file type filter patterns for the specified policy.
		/// </summary>
		/// <param name="policy">Policy to retrieve the filter patterns from.</param>
		/// <returns>An array of file type filter patterns for the given policy.</returns>
		static private FileTypeEntry[] GetPatterns( Policy policy )
		{
			// List to hold the temporary results.
			ArrayList tempList = new ArrayList();

			// Return all of the rules.
			if ( policy != null )
			{
				foreach ( Rule rule in policy.Rules )
				{
					FileTypeEntry fte = new FileTypeEntry( rule.Operand as string, ( rule.RuleResult == Rule.Result.Allow ) ? true : false, ( rule.RuleOperation == Rule.Operation.RegExp ) ? false : true );
					tempList.Add( fte );
				}
			}

			return tempList.ToArray( typeof( FileTypeEntry ) ) as FileTypeEntry[];
		}

		/// <summary>
		/// Gets the file type filter patterns for the specified polic for a user
		/// </summary>
		/// <param name="policy">Policy to retrieve the filter patterns from.</param>
		/// <returns>An array of file type filter patterns for the given policy.</returns>
		static private FileTypeEntry[] GetUserPatterns( Policy policy )
		{
			// List to hold the temporary results.
			ArrayList tempList = new ArrayList();

			// Return all of the rules.
			if ( policy != null )
			{
				foreach ( Rule rule in policy.Rules)
				{
					FileTypeEntry fte = new FileTypeEntry( rule.Operand as string, ( rule.RuleResult == Rule.Result.Allow ) ? true : false, ( rule.RuleOperation == Rule.Operation.RegExp ) ? false : true );
					tempList.Add( fte );
					log.Debug("Filetype filter going to add : "+rule.Operand as string);
				}
			}

			return tempList.ToArray( typeof( FileTypeEntry ) ) as FileTypeEntry[];
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns whether the specified file is allowed to pass through the filter.
		/// </summary>
		/// <param name="fileName">Name of the file including its extension.</param>
		/// <returns>True if the file is allowed to pass through the filter. Otherwise false is returned.</returns>
		public bool Allowed( string fileName )
		{
			bool isAllowed = true;
			string fullPath = Path.GetFileName( fileName );

			// Check the overall domain/member policy first to make sure that the file type is not excluded.
			if ( memberPolicy != null )
			{
				// Apply the rule to see if there is space available.
				isAllowed = ( memberPolicy.Apply( fullPath, FileTypeFilterPolicyID ) == Rule.Result.Allow );
			}

			if ( ( collectionPolicy != null ) && isAllowed )
			{
 				// Apply the rule to see if the file type is allowed in the collection.
 				isAllowed = ( collectionPolicy.Apply( fullPath, FileTypeFilterPolicyID ) == Rule.Result.Allow );
			}
			return isAllowed;
		}
		#endregion
	}
}
