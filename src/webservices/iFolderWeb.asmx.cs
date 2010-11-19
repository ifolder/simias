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
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections;

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder Web Service
	/// </summary>
	[WebService(
		 Namespace="http://novell.com/ifolder/webservice/",
		 Name="iFolderWeb",
		 Description="iFolder Web Service")]
	public class iFolderWeb : iFolderWebLocal
	{
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderWeb()
		{
		}

		#endregion

		#region Common System

		/// <summary>
		/// Get information about the iFolder system.
		/// </summary>
		/// <returns>An iFolderSystem object describing the system.</returns>
		[WebMethod(
			 Description="Get information about the iFolder system.",
			 EnableSession=true)]
		public override iFolderSystem GetSystem()
		{
			return base.GetSystem();
		}
		
		/// <summary>
		/// Get information about the authenticated user's home iFolder server.
		/// </summary>
		/// <returns>An iFolderServer object describing the user's home iFolder server.</returns>
		[WebMethod(
			 Description="Get information about the authenticated user's home iFolder server.",
			 EnableSession=true)]
		public override iFolderServer GetHomeServer()
		{
			return base.GetHomeServer();
		}

                /// <summary>
                /// Set the migrated flag for the iFolder to determine that the iFolder is a migrated one.
                /// </summary>
                /// <param name="ifolderID">The id of the iFolder.</param>
                /// <param name="MigrationSource">Determines from which source the iFolder is migrated. (whther from iFolder2 server or 3.2 server or anyother</param>                /// <returns></returns>
                [WebMethod(
                         Description="Set the migrated flag for the iFolder to determine that the iFolder is a migrated one.",
                         EnableSession=true)]
		public override void SetMigratedFlag( string iFolderID, int MigrationSource )
		{
			base.SetMigratedFlag( iFolderID, MigrationSource );
		}

		/// <summary>
		/// Get information about all the iFolder servers.
		/// </summary>
		/// <returns>An array of iFolderServer objects.</returns>
		[WebMethod(
			 Description="Get information about all the iFolder servers.",
			 EnableSession=true)]
		public override iFolderServer[] GetServers()
		{
			return base.GetServers();
		}

		[WebMethod(
			 Description="Get information about the current server is having multibyteserver flag no or yes in Simias.config.",
			 EnableSession=true)]
		public override string GetServerStatus()
		{
			return base.GetServerStatus ();
		}

		/// <summary>
		/// Get information about iFolder Servers identified by a search on name.
		/// </summary>
		/// <param name="operation">The operation to compare the name and pattern.</param>
		/// <param name="pattern">The pattern to search.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder Server objects.</returns>
		[WebMethod(
			 Description="Get information about iFolder Servers identified by a search on name.",
			 EnableSession=true)]
		public override iFolderServerSet GetServersByName(SearchOperation operation, string pattern,
			int index, int count)
		{
			return base.GetServersByName(operation, pattern, index, count);
		}

		#endregion

		#region Common iFolders

		/// <summary>
		/// Delete an iFolder
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder to be deleted.</param>
		/// <remarks>This API will accept multiple iFolder ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Delete an iFolder",
			 EnableSession=true)]
		public override void DeleteiFolder(string ifolderID)
		{
			base.DeleteiFolder(ifolderID);
		}

		/// <summary>
		/// Get information about an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <returns>An iFolder object describing the iFolder.</returns>
		[WebMethod(
			 Description="Get information about an iFolder.",
			 EnableSession=true)]
		public override iFolder GetiFolder(string ifolderID)
		{
			return base.GetiFolder(ifolderID);
		}
		
	        /// <summary>
		/// Get the HomeServer for the specified user
		/// </summary>
		/// <returns>HomeServer ID</returns>
		[WebMethod(
			 Description="Get the private url of ifolder's homeserver",
			 EnableSession=true)]
		public override string GetiFolderLocation( string ifolderID )
		{
                         return base.GetiFolderLocation (ifolderID);
		}

	        /// <summary>
		/// Get the recovery agent list
		/// </summary>
		/// <returns>list as a string array</returns>
		[WebMethod(
			 Description="Get the list of recovery agents",
			 EnableSession=true)]
		public override string[] GetRAList()
		{
                         return base.GetRAList(); 
		}
		
		/// <summary>
		/// Get the recovery Agent certificate
		/// </summary>
		/// <returns>byte array containing the certificate </returns>
		[WebMethod(
			 Description="Get the RA Certificate",
			 EnableSession=true)]
		public override byte[] GetRACertificate(string rAgent)
		{
                         return base.GetRACertificate(rAgent); 
		}
		
		
		/// <summary>
		/// Get the PassPhrase status 
		/// </summary>
		/// <returns>the bool value </returns>
		[WebMethod(
			 Description="Get the PassPhrase status",
			 EnableSession=true)]
		public override bool IsPassPhraseSet ()
                {
			 
			return base.IsPassPhraseSet();
		}

		/// <summary>
		/// returns whether encryption is enforced for this user or not. checks the whole priority structure 
		/// </summary>
		/// <returns>the bool value </returns>
		[WebMethod(
			 Description="checks if this user has some user specific encryption enforced on it or not",
			 EnableSession=true)]
		public override bool IsUserOrSystemEncryptionEnforced (string UserID)
                {
			return base.IsUserOrSystemEncryptionEnforced(UserID);
		}


		/// <summary>
		/// Checks whether an encrypted ifolders should be shown 
		/// </summary>
		/// <returns>the bool value </returns>
		[WebMethod(
			 Description="checks for showing Tabs for encrypted ifolder",
			 EnableSession=true)]
		public override bool ShowTabDetails (string PassPhrase, string EncryptionAlgorithm)
                {
			 
			return base.ShowTabDetails(PassPhrase, EncryptionAlgorithm);
		}
		
		///<summary>
		///Validate the passphrase for the correctness
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Validate the passphrase for the correctness.")]
		[SoapDocumentMethod]	
		public override Simias.Authentication.Status ValidatePassPhrase(string passPhrase)
		{
			return base.ValidatePassPhrase(passPhrase);
			
		}
		
		///<summary>
		///Set the passphrase and recovery agent
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Set the passphrase and recovery agent.")]
		[SoapDocumentMethod]
		public override void SetPassPhrase(string passPhrase, string recoveryAgentName, string publicKey)
		{
			base.SetPassPhrase(passPhrase, recoveryAgentName, publicKey);
		}

		///<summary>
		/// Change the password for logged in user
		///</summary>
		///<returns>the enum explaining the status</returns>
		[WebMethod(EnableSession=true, Description="Change the password for logged in user")]
		[SoapDocumentMethod]
		public override int ChangePassword(string OldPassword, string NewPassword)
		{
			return base.ChangePassword(OldPassword, NewPassword);
		}
		

		/// <summary>
		/// Get detailed information about an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder</param>
		/// <returns>An iFolderDetails object describing the iFolder</returns>
		/// <remarks>It is more expensive to call GetiFolderDetails than GetiFolder.</remarks>
		[WebMethod(
			 Description="Get detailed information about an iFolder.",
			 EnableSession=true)]
		public override iFolderDetails GetiFolderDetails(string ifolderID)
		{
			return base.GetiFolderDetails(ifolderID);
		}
		
		/// <summary>
		/// Set the description of an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="description">The new description for the iFolder.</param>
		[WebMethod(
			 Description="Set the description of an iFolder.",
			 EnableSession=true)]
		public override void SetiFolderDescription(string ifolderID, string description)
		{
			base.SetiFolderDescription(ifolderID, description);
		}

		/// <summary>
		/// Publish an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="publish">The published state of the iFolder</param>
		[WebMethod(
			 Description="Publish an iFolder.",
			 EnableSession=true)]
		public override void PublishiFolder(string ifolderID, bool publish)
		{
			base.PublishiFolder(ifolderID, publish);
		}

		#endregion

		#region Common Changes
		
		/// <summary>
		/// Get a history of changes to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="itemID">The id of item to filter the results (can be null for no filtering).</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of ChangeEntry objects.</returns>
		[WebMethod(
			 Description="Get a history of changes to an iFolder.",
			 EnableSession=true)]
		public override ChangeEntrySet GetChanges(string ifolderID, string itemID, int index, int max)
		{
			return base.GetChanges(ifolderID, itemID, index, max);
		}

		/// <summary>
		/// Get a trimmed url from full url
		/// </summary>
		/// <param name="FullUrl">Full URL to be called</param>
		/// <returns>a trimmed url that will contain only aspx page name</returns>
		[WebMethod(
			 Description="Get a trimmed url from full url",
			 EnableSession=true)]
		public override string TrimUrl(string FullUrl)
		{
			return base.TrimUrl(FullUrl);
		}

		#endregion

		#region Common Users

		/// <summary>
		/// Get information about the authenticated user.
		/// </summary>
		/// <returns>An iFolderUser object describing the authenticated user.</returns>
		[WebMethod(
			 Description="Get information about the authenticated user.",
			 EnableSession=true)]
		public override iFolderUser GetAuthenticatedUser()
		{
			return base.GetAuthenticatedUser();
		}

		/// <summary>
		/// Set the rights of a member on an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="userID">The user id of the member.</param>
		/// <param name="rights">The rights to be set.</param>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Set the rights of a member on an iFolder.",
			 EnableSession=true)]
		public override void SetMemberRights(string ifolderID, string userID, Rights rights)
		{
			base.SetMemberRights(ifolderID, userID, rights);
		}

		/// <summary>
		/// Add a member to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of iFolder.</param>
		/// <param name="userID">The user id of the new member.</param>
		/// <param name="rights">The rights of the new member on the iFolder.</param>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Add a member to an iFolder.",
			 EnableSession=true)]
		public override void AddMember(string ifolderID, string userID, Rights rights)
		{   
			base.AddMember(ifolderID, userID, rights);
		}

		/// <summary>
		/// Remove a member from an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="userID">The user id of the member.</param>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Remove a member from an iFolder.",
			 EnableSession=true)]
		public override void RemoveMember(string ifolderID, string userID)
		{
			base.RemoveMember(ifolderID, userID);
		}

		/// <summary>
		/// Gets the group ids this member belongs to .
		/// </summary>
		/// <param name="userID">The user id of the member.</param>
		[WebMethod(
			 Description="Get group ids this member belongs to ",
			 EnableSession=true)]
		public override string [] GetGroupIDs(string userID)
		{
			return ( base.GetGroupIDs(userID) );
		}

		/// <summary>
		/// Set the owner of an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="userID">The user id of the new owner.</param>
		[WebMethod(
			 Description="Set the owner of an iFolder.",
			 EnableSession=true)]
		public override void SetiFolderOwner(string ifolderID, string userID, bool OrphanAdopt)
		{
			OrphanAdopt = false;
			base.SetiFolderOwner(ifolderID, userID, OrphanAdopt);
		}

		/// <summary>
		/// Get information about the members of an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects describing the members.</returns>
		[WebMethod(
			 Description="Get information about the members of an iFolder.",
			 EnableSession=true)]
		public override iFolderUserSet GetMembers(string ifolderID, int index, int max)
		{
			return base.GetMembers(ifolderID, index, max);
		}

		/// <summary>
		/// Get information about all of the iFolder users.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users.",
			 EnableSession=true)]
		public override iFolderUserSet GetUsers(int index, int max)
		{
			return base.GetUsers(index, max);
		}

		/// <summary>
		/// Get information about a user using an id or username.
		/// </summary>
		/// <param name="userID">The id or username of the user.</param>
		/// <returns>A iFolderUser object.</returns>
		[WebMethod(
			 Description="Get information about a user using an id or username.",
			 EnableSession=true)]
		public override iFolderUser GetUser(string userID)
		{
			return base.GetUser(userID);
		}

		/// <summary>
		/// Get detailed information about a user.
		/// </summary>
		/// <param name="userID">The id of the user.</param>
		/// <returns>A iFolderUserDetails object.</returns>
		/// <remarks>It is more expensive to call GetUserDetails than GetUser.</remarks>
		[WebMethod(
			 Description="Get User Details",
			 EnableSession=true)]
		public override iFolderUserDetails GetUserDetails(string userID)
		{
			return base.GetUserDetails(userID);
		}

		/// <summary>
		/// Get information about all of the iFolder users identified by the search property, operation, and pattern.
		/// </summary>
		/// <param name="property">The property to search.</param>
		/// <param name="operation">The operation to compare the property and pattern.</param>
		/// <param name="pattern">The pattern to search</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users identified by the search property, operation, and pattern.",
			 EnableSession=true)]
		public override iFolderUserSet GetUsersBySearch(SearchProperty property, SearchOperation operation, string pattern, int index, int max)
		{
			return base.GetUsersBySearch(property, operation, pattern, index, max);
		}

		#endregion

		#region iFolders

		/// <summary>
		/// Shorten the name so that it can fit inot dispaly .
		/// </summary>
		/// <param name="name">The name of the iFolder to be shortened.</param>
		/// <returns>< string which is short.</returns>
		[WebMethod(
			 Description="return a string which is short.",
			 EnableSession=true)]
		public override string GetShortenedName(string name, int length)
		{
			return base.GetShortenedName(name, length);
		}

		/// <summary>
		/// Create a new iFolder with the authenticated user as the owner.
		/// </summary>
		/// <param name="name">The name of the new iFolder.</param>
		/// <param name="description">The description of the new iFolder. </param>
		/// <returns>An iFolder object describing the new iFolder.</returns>
		[WebMethod(
			 Description="Create a new iFolder with the authenticated user as the owner.",
			 EnableSession=true)]
		public override iFolder CreateiFolder(string name, string description, bool ssl, string EncryptionAlgorithm, string PassPhrase)
		{
			return base.CreateiFolder(name, description, ssl,EncryptionAlgorithm, PassPhrase);
		}

		/// <summary>
		/// Get information about all the iFolders to which the authenticate user has rights.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about all the iFolders to which the authenticate user has rights.",
			 EnableSession=true)]
		public override iFolderSet GetiFolders(int index, int max)
		{
			return base.GetiFolders(index, max);
		}

		/// <summary>
		/// Get information about iFolders identified by the search operation and pattern.
		/// </summary>
		/// <param name="operation">The operation for the search.</param>
		/// <param name="pattern">The pattern for the search.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about iFolders identified by the search operation and pattern.",
			 EnableSession=true)]
		public override iFolderSet GetiFoldersByName(SearchOperation operation, string pattern, int index, int max)
		{
			return base.GetiFoldersByName(operation, pattern, index, max);
		}

		/// <summary>
                /// Get the ifolder limit policy for a User.
                /// </summary>
                /// <param name="userID">The user id of the user who owns the ifolder.</param>
                [WebMethod(
                         Description="Get ifolder limit policy information for an iFolder.",
                         EnableSession=true)]
                public override int GetiFolderLimitPolicyStatus(string userID)
		{
			return base.GetiFolderLimitPolicyStatus(userID);
		}

		/// <summary>
		/// Get information about iFolder identified by role, creation time, and search operation and pattern.
		/// </summary>
		/// <param name="role">The required authenticated user's role in the Folder.</param>
		/// <param name="after">The required earliest limit on the shared date of the iFolder. </param>
		/// <param name="operation">The operation for the search.</param>
		/// <param name="pattern">The pattern for the search.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about iFolder identified by role, creation time, and search operation and pattern.",
			 EnableSession=true)]
		public override iFolderSet GetiFoldersBySearch(MemberRole role, DateTime after, SearchOperation operation, string pattern, int index, int max)
		{
			return base.GetiFoldersBySearch(role, after, operation, pattern, index, max);
		}

		#endregion

		#region Users
		
		/// <summary>
		/// Remove the authenticated user's membership to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <remarks>This API will accept multiple iFolder ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Remove the authenticated user's membership to an iFolder.",
			 EnableSession=true)]
		public override void RemoveMembership(string ifolderID)
		{
			base.RemoveMembership(ifolderID);
		}

		#endregion

		#region Policy
		
		/// <summary>
		/// Get policy information for the authenticated user.
		/// </summary>
		/// <returns>A UserPolicy object describing the authenticated user's policy.</returns>
		[WebMethod(
			 Description="Get policy information for the authenticated user.",
			 EnableSession=true)]
		public override UserPolicy GetAuthenticatedUserPolicy()
		{
			return base.GetAuthenticatedUserPolicy();
		}

		/// <summary>
		/// Get policy information for an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of an iFolder.</param>
		/// <returns>A iFolderPolicy object.</returns>
		[WebMethod(
			 Description="Get policy information for an iFolder.",
			 EnableSession=true)]
		public override iFolderPolicy GetiFolderPolicy(string ifolderID)
		{
			return base.GetiFolderPolicy(ifolderID);
		}

        /// <summary>
        /// Gets the encryption policy for an iFolder
        /// </summary>
        /// <returns>the policy as integer</returns>
		[WebMethod(
			 Description="Get policy information for an iFolder.",
			 EnableSession=true)]
		public override int GetEncryptionPolicy()
		{
			return base.GetEncryptionPolicy();
		}

		/// <summary>
		/// Set the policy for an iFolder.
		/// </summary>
		/// <param name="policy">The iFolderPolicy object.</param>
		[WebMethod(
			 Description="Set the policy for an iFolder.",
			 EnableSession=true)]
		public override void SetiFolderPolicy(iFolderPolicy policy)
		{
			base.SetiFolderPolicy(policy);
		}
		
		#endregion

		#region Entries

		/// <summary>
		/// Create an iFolder entry (file or directory).
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="parentID">The id of the parent or containing entry.</param>
		/// <param name="type">The type (file or directory for the entry.</param>
		/// <param name="entryName">The name of the new entry.</param>
		/// <returns>An iFolderEntry object describing the new entry.</returns>
		[WebMethod(
			 Description="Create an iFolder entry (file or directory).",
			 EnableSession=true)]
		public override iFolderEntry CreateEntry(string ifolderID, string parentID, iFolderEntryType type, string entryName)
		{
			return base.CreateEntry(ifolderID, parentID, type, entryName);
		}

		/// <summary>
		/// Create nodes
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <returns>A boolean describing the new entry.</returns>
		[WebMethod(
			 Description="Create an iFolder entry (file or directory).",
			 EnableSession=true)]
		public override int CreateNodes(string ifolderID, bool RemoveEntry)
		{
			return base.CreateNodes(ifolderID, RemoveEntry);
		}

		/// <summary>
		/// Delete an iFolder entry.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="entryID">The id of the entry to be deleted.</param>
		/// <remarks>This API will accept multiple entry ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Delete an iFolder entry.",
			 EnableSession=true)]
		public override void DeleteEntry(string ifolderID, string entryID)
		{
			base.DeleteEntry(ifolderID, entryID);
		}
		
		/// <summary>
		/// Get information about an iFolder entry.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder containing the entry.</param>
		/// <param name="entryID">The id of the entry.</param>
		/// <returns>An iFolderEntry object.</returns>
		[WebMethod(
			 Description="Get information about an iFolder entry.",
			 EnableSession=true)]
		public override iFolderEntry GetEntry(string ifolderID, string entryID)
		{
			return base.GetEntry(ifolderID, entryID);
		}
		
		/// <summary>
		/// Get information about an entry identified by it's relative path in the iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder containing the entry.</param>
		/// <param name="entryPath">The relative path of entry.</param>
		/// <returns>An iFolderEntry object.</returns>
		[WebMethod(
			 Description="Get information about an entry identified by it's relative path in the iFolder.",
			 EnableSession=true)]
		public override iFolderEntry GetEntryByPath(string ifolderID, string entryPath)
		{
			return base.GetEntryByPath(ifolderID, entryPath);
		}
		
		/// <summary>
		/// Get information about entries in an iFolder identified by their parent entry.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="entryID">The id of the parent entry (can be null for the root of the iFolder).</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderEntry objects.</returns>
		[WebMethod(
			 Description="Get information about entries identified by their parent entry.",
			 EnableSession=true)]
		public override iFolderEntrySet GetEntries(string ifolderID, string entryID, int index, int max)
		{
			return base.GetEntries(ifolderID, entryID, index, max);
		}

		/// <summary>
		/// Get information about entries in an iFolder identified by a search on the name.
		/// </summary>
		/// <param name="ifolderID">The id of iFolder.</param>
		/// <param name="parentID">The id of the parent entry.</param>
		/// <param name="operation">The operation to compare the name and search pattern.</param>
		/// <param name="pattern">The pattern to search.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderEntry objects.</returns>
		[WebMethod(
			 Description="Get information about entries in an iFolder identified by a search on the name.",
			 EnableSession=true)]
		public override iFolderEntrySet GetEntriesByName(string ifolderID, string parentID, SearchOperation operation, string pattern, int index, int max)
		{
			return base.GetEntriesByName(ifolderID, parentID, operation, pattern, index, max);
		}

        /// <summary>
        /// Get information about entries identified by their parent entry. Search it with the passed folder array and return those matched
        /// </summary>
        /// <param name="ifolderID">ifolder id</param>
        /// <param name="entryID">entry id </param>
        /// <param name="folder">folder array where it will be searched</param>
        /// <returns>iFolderEntry set which has matched</returns>
		[WebMethod(
			 Description="Get information about entries identified by their parent entry. Search it with the passed folder array and return those matched",
			 EnableSession=true)]
		public override iFolderEntrySet GetMatchedEntries(string ifolderID, string entryID,string[] folder)
		{
			return base.GetMatchedEntries(ifolderID, entryID, folder);
		}

		#endregion

		#region Settings
		
		/// <summary>
		/// Get a setting specific to the authenticated user.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting.</returns>
		[WebMethod(
			 Description="Get a setting specific to the authenticated user.",
			 EnableSession=true)]
		public override string GetSetting(string name)
		{
			return base.GetSetting(name);
		}

		/// <summary>
		/// Set a setting specific to the authenticated user.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		[WebMethod(
			 Description="Set a setting specific to the authenticated user.",
			 EnableSession=true)]
		public override void SetSetting(string name, string value)
		{
			base.SetSetting(name, value);
		}

		#endregion

		#region Files

		/// <summary>
		/// Open an iFolder file entry for reading.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="entryID">The id of the file entry.</param>
		/// <returns>A file handle.</returns>
		[WebMethod(
			 Description="Open an iFolder file entry for reading.",
			 EnableSession=true)]
		public override string OpenFileRead(string ifolderID, string entryID)
		{
			return base.OpenFileRead(ifolderID, entryID);
		}

		/// <summary>
		/// Open an iFolder file entry for writing.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="entryID">The id of the file entry.</param>
		/// <param name="length">The length of the file to be written.</param>
		/// <returns>A file handle.</returns>
		[WebMethod(
			 Description="Open an iFolder file entry for writing.",
			 EnableSession=true)]
		public override string OpenFileWrite(string ifolderID, string entryID, long length)
		{
			return base.OpenFileWrite(ifolderID, entryID, length);
		}

		/// <summary>
		/// Read from an openned iFolder file entry.
		/// </summary>
		/// <param name="file">The file handle.</param>
		/// <param name="size">The max size of the return buffer.</param>
		/// <returns>An array of bytes.</returns>
		[WebMethod(
			 Description="Read from an openned iFolder file entry.",
			 EnableSession=true)]
		public override byte[] ReadFile(string file, int size)
		{
			return base.ReadFile(file, size);
		}

		/// <summary>
		/// Write to an openned iFolder file entry.
		/// </summary>
		/// <param name="file">The file handle.</param>
		/// <param name="buffer">The buffer to be written.</param>
		[WebMethod(
			 Description="Write to an openned iFolder file entry.",
			 EnableSession=true)]
		public override void WriteFile(string file, byte[] buffer)
		{
			base.WriteFile(file, buffer);
		}

		/// <summary>
		/// Close an openned iFolder file entry.
		/// </summary>
		/// <param name="file">The file handle.</param>
		[WebMethod(
			 Description="Close an openned iFolder file entry.",
			 EnableSession=true)]
		public override void CloseFile(string file)
		{
			base.CloseFile(file);
		}

		/// <summary>
		/// Set the file length
		/// </summary>
		/// <param name="file">The file handle.</param>
		[WebMethod(
			 Description="Close an openned iFolder file entry.",
			 EnableSession=true)]
		public override void SetFileLength(string ifolderID, string nodeID, long length)
		{
			base.SetFileLength( ifolderID,  nodeID,  length);
		}
		#endregion
	}
}
