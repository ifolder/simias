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

using Simias.Storage;
using Simias.Server;
using Simias;
using Simias.Web;

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder Admin Web Service
	/// </summary>
	[WebService(
		 Namespace="http://novell.com/ifolder/webservice/",
		 Name="iFolderAdmin",
		 Description="iFolder Admin Web Service")]
	public class iFolderAdmin : iFolderAdminLocal
	{
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderAdmin()
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
		/// Get the HomeServer for the specified user
		/// </summary>
		/// <returns>HomeServer ID</returns>
		[WebMethod(
			 Description="Get the homeserver for the user.Server Provisioning will be done if user has no HomeServer.",
			 EnableSession=true)]
		public override string GetHomeServerForUser( string username, string password )
		{
		        //Note : DomainService.GetHomeServer will be called.
			return base.GetHomeServerForUser ( username, password );
		}

                /// <summary>
                /// Get the HomeServer for the specified user using admin credential
                /// </summary>
                /// <returns>HomeServer ID</returns>
                [WebMethod(
                         Description="Get the homeserver for the user.Server Provisioning will be done if user has no HomeServer.",
                         EnableSession=true)]
                public override string GetHomeServerURLForUserID( string userid )
                {
                                //Note : DomainService.GetHomeServer will be called.
                        return base.GetHomeServerURLForUserID( userid );
                }

                /// <summary>
                /// returns whether encryption is enforced for this user or not  
                /// </summary>
                /// <returns>true/false</returns>
                [WebMethod(
                         Description="returns true/false based on whether encryption is enforced for this user or not ",
                         EnableSession=true)]
                public override bool IsUserOrSystemEncryptionEnforced( string userid )
                {
                        return base.IsUserOrSystemEncryptionEnforced( userid );
                }

	        /// <summary>
		/// Get the HomeServer for the specified user
		/// </summary>
		/// <returns>HomeServer ID</returns>
		[WebMethod(
		         Description="Get the private url of ifolder's homeserver.",
			 EnableSession=true)]
		public override string GetiFolderLocation( string ifolderID )
		{
                         return base.GetiFolderLocation (ifolderID);
		}

		    /// <summary>
		/// Get the Orphaned ifolders 
		/// </summary>
		/// <returns> a list of orphaned ifolders</returns>
		[WebMethod(
		         Description="Get the list of orphaned ifolders",
			 EnableSession=true)]
		public override iFolderSet GetOrphanediFolders(SearchOperation operation, string pattern, int index, int max  )
		{
                         return base.GetOrphanediFolders ( operation, pattern, index, max );
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
		/// check the Orphaned property 
		/// </summary>
		/// <returns> a string value </returns>
		[WebMethod(
		         Description="check the orphaned property",
			 EnableSession=true)]
		public override string IsOrphanediFolder(string iFolderID)
		{
                         return base.IsOrphanediFolder ( iFolderID );
		}
		
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
		/// returns whether the id is a group id or not  
		/// </summary>
		/// <returns> bool  </returns>
		[WebMethod(
		         Description="returns true if the parameter is a group id ",
			 EnableSession=true)]
		public override bool IsGroupId(string userid)
		{
			return base.IsGroupId (userid);
		}

		/// <summary>
		/// evaluate the disable sharing policy for an iFolder 
		/// </summary>
		/// <returns> an integer value which has disable sharing policy information  </returns>
		[WebMethod(
		         Description="evaluate the disable sharing policy",
			 EnableSession=true)]
		public override int GetSharingStatus(string iFolderID)
		{
			return base.GetSharingStatus ( iFolderID );
		}

		/// <summary>
		/// DisablePast Sharing for the system  
		/// </summary>
		/// <returns> No return value  </returns>
		[WebMethod(
		         Description="to remove past sharing of all iFolders of the system ",
			 EnableSession=true)]
		public override void DisableSystemPastSharing()
		{
			base.DisableSystemPastSharing ();
		}

		
		/// <summary>
		/// DisablePastSharing for an user 
		/// </summary>
		/// <returns> No return value  </returns>
		[WebMethod(
		         Description="to remove past sharing of all iFolders of this user ",
			 EnableSession=true)]
		public override void DisableUserPastSharing(string UserID)
		{
			base.DisableUserPastSharing (UserID);
		}
			
				
		/// <summary>
		/// Disable Past Sharing for an iFolder 
		/// </summary>
		/// <returns> No return value  </returns>
		[WebMethod(
		         Description="to remove past sharing of this iFolder ",
			 EnableSession=true)]
		public override void DisableiFolderPastSharing(string iFolderID)
		{
			base.DisableiFolderPastSharing (iFolderID);
		}

		/// <summary>
                /// Add a data store for an iFolder Server.
                /// </summary>
                /// <param name="DataPathname">The name of the data store.</param>
		/// <param name="FullPath">The Full Path of the data store.</param>
		/// <param name="serverID">Server ID of the server</param>
                /// <returns>Integer : 0 on success.</returns>
                [WebMethod(
                         Description="Add a data store for an iFolder Server.",
                         EnableSession=true)]
                public override int AddDataStore(string datapathname,string fullpath,string ServerID)
                {
                        return base.AddDataStore(datapathname,fullpath,ServerID);
                }
	
		/// <summary>
                /// Modify data store for an iFolder Server.
                /// </summary>
                /// <param name="name">The name of the data store.</param>
                /// <returns>Bool true on success.</returns>
                [WebMethod(
                         Description="Modify a data store for an iFolder Server.",
                         EnableSession=true)]
                public virtual bool ModifyDataStore(string datapathname, bool enabled)
                {
                        return base.ModifyStore( datapathname , enabled );
                }

		/// <summary>
                /// Delete data store for an iFolder Server.
                /// </summary>
                /// <param name="name">The name of the data store.</param>
                /// <returns>Bool true on success.</returns>
                [WebMethod(
                         Description="Delete a data store for an iFolder Server.",
                         EnableSession=true)]
                public virtual bool DeleteDataStore(string datapathname)
                {
                        return base.DeleteDataPath(datapathname);
                }


		/// <summary>
                /// Gets all the data store for an iFolder Server.
                /// </summary>
                /// <returns>An array of DataPaths.</returns>
                [WebMethod(
                         Description="Gets all the Data Store for an iFolder Server.",
                         EnableSession=true)]
                public override VolumesList GetVolumes(int index, int max)
                {
                        return base.GetVolumes(index,max);
                }

	        /// <summary>
		/// Get the HomeServer for the specified user
		/// </summary>
		/// <returns>HomeServer ID</returns>
		[WebMethod(
			 Description="Fetch the list of available reports.",
			 EnableSession=true)]
		public override string[] GetReports()
		{
			return base.GetReports ();
		}

		/// <summary>
		/// Get information about an iFolder Server.
		/// </summary>
		/// <param name="serverID">The id of the iFolder Server.</param>
		/// <returns>An iFolderServer object describing the iFolder Server.</returns>
		[WebMethod(
			 Description="Get information about an iFolder Server.",
			 EnableSession=true)]
		public override iFolderServer GetServer(string serverID)
		{
			return base.GetServer(serverID);
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
                /// Delete an iFolder
                /// </summary>
                /// <param name="ifolderID">The id of the iFolder .</param>
                /// <remarks>This API will accept multiple iFolder ids in a comma delimited list.</remarks>
                [WebMethod(
                         Description="Restore Data file/folder/ifolder",
                         EnableSession=true)]
                public override int RestoreiFolderData(string url, string adminname, string adminpassword, string ifolderid, string relativepath, string basepath, int startindex, string LogLocation)
                {
                        return base.RestoreiFolderData(url, adminname, adminpassword, ifolderid, relativepath, basepath, startindex, LogLocation);
                }
 
                /// <summary>
                /// Delete an iFolder
                /// </summary>
                /// <param name="ifolderID">The id of the iFolder .</param>
                /// <remarks>This API will accept multiple iFolder ids in a comma delimited list.</remarks>
                [WebMethod(
                         Description="Retrive status of restore operation",
                         EnableSession=true)]
                public override int GetRestoreStatusForCollection(string ifolderid, out int totalcount, out int finishedcount)
                {
                        return base.GetRestoreStatusForCollection(ifolderid, out totalcount, out finishedcount);
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if( !IsAccessAllowed(col.Owner.UserID) )
				{
					return ;
				}
			}
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if( !IsAccessAllowed(col.Owner.UserID) )
				{
					return ;
				}
			}
			base.RemoveMember(ifolderID, userID);
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if( !IsAccessAllowed(col.Owner.UserID) )
				{
					return ;
				}
			}
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if( !IsAccessAllowed(col.Owner.UserID) )
				{
					return null;
				}
			}
			return base.GetMembers(ifolderID, index, max);
		}

		/// <summary>
		/// check whether passphrase is set for a particular user.
		/// </summary>
		/// <param name="userID">The id of the user.</param>
		/// <returns>A Bool value.</returns>
		[WebMethod(
			 Description="Get PassPhrase set status",
			 EnableSession=true)]
		public override bool IsPassPhraseSetForUser(string userID)
		{
			return base.IsPassPhraseSetForUser(userID);
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
		/// Provision the users to servers.
		/// </summary>
		/// <param name="ServerNames">A string array containing server names</param>
		/// <param name="UserIDs">A string array containing corresponding userids</param>
		/// <returns>No Return value</returns>
		[WebMethod(
			 Description="Provision the users to different servers",
			 EnableSession=true)]
		public override void ProvisionUsersToServers(string [] ServerNames, string [] UserIDs)
		{
			base.ProvisionUsersToServers(ServerNames, UserIDs);
		}

		/// <summary>
		/// Provision the users to a server.
		/// </summary>
		/// <param name="ServerName">A string containing server's name. </param>
		/// <param name="ListOfUsers">An array of string containing userIDs </param>
		/// <returns>No Return value</returns>
		[WebMethod(
			 Description="Provision the users to one server",
			 EnableSession=true)]
		public override void ProvisionUsersToServer(string ServerName, string [] ListOfUsers)
		{
			base.ProvisionUsersToServer(ServerName, ListOfUsers);
		}

		/// <summary>
		/// Get the list of servers.
		/// </summary>
		/// <returns>a string array of all the server names </returns>
		[WebMethod(
			 Description="Get the list of servers",
			 EnableSession=true)]
		public override string [] GetServerProvisioningList()
		{
			return base.GetServerProvisioningList();
		}

		/// <summary>
		/// Get the simiasserverrequiressl status.
		/// </summary>
		/// <returns>a string </returns>
		[WebMethod(
			 Description="Get the status of simiasrequiressl",
			 EnableSession=true)]
		public override string GetSimiasRequiresSSLStatus()
		{
			return base.GetSimiasRequiresSSLStatus();
		}
		
		/// <summary>
               /// set the new IP addresses.
               /// </summary>
               /// <returns>bool value based on success/failure </returns>
               [WebMethod(
                        Description="sets IP details for the server",
                        EnableSession=true)]
               public override bool SetIPDetails(string privateIP , string publicIP, string MastersIP )
               {
                       return base.SetIPDetails(privateIP, publicIP, MastersIP);
               }

			   /// <summary>
			   /// Get the Master Server
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description="gets the Master server", EnableSession=true)]
	        	public override iFolderServer GetMasterServer ()
				   {
					   return base.GetMasterServer();
				   }

			   /// <summary>
			   /// Set the Master Url for node
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description="sets the MasterUrl for the server", EnableSession=true)]
				   public override bool SetMasterServerUrl (string HostID, string MasterUrl)
				   {
					   return base.SetMasterServerUrl (HostID, MasterUrl);
				   }

			   /// <summary>
			   /// Sets this server as Master Server
			   /// </summary>
			   /// <param name="HostID"> ID(Ace value) of the server</param>
			   /// <returns>true on success/false on failure</returns>
			   [WebMethod(Description= "set the new master server", EnableSession = true)]
				   public override bool SetAsMasterServer(string hostID)
				   {
					   return base.SetAsMasterServer(hostID);
				   }

			   /// <summary>
			   /// Sets this server as Slave Server
			   /// </summary>
			   /// <param name="HostID"> ID(Ace value) of the new server</param>
			   /// <param name="newMasterPublicUrl"> public url of the new master server</param>
			   /// <returns>true on success/false on failure</returns>
			   [WebMethod(Description= "set the server as Slave", EnableSession = true)]
				   public override bool SetAsSlaveServer(string newMasterHostID, string newMasterPublicUrl)
				   {
					   return base.SetAsSlaveServer(newMasterHostID, newMasterPublicUrl);
				   }

			   /// <summary>
			   /// Set the Master Url for node
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description="sets the Master node attriute for the host", EnableSession=true)]
				   public override bool SetMasterNodeAttribute(string HostID, bool Value)
				   {
					   return base.SetMasterNodeAttribute (HostID, Value);
				   }

			   /// <summary>
			   /// Get the value of IsMaster for this server
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description="Get the MasterNodeAttributes for the host", EnableSession=true)]
				   public override bool GetMasterNodeAttribute (string HostID)
				   {
					   return base.GetMasterNodeAttribute (HostID);
				   }

			   /// <summary>
			   /// Verify attributes of servers
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description="Get the Attributes for the host", EnableSession=true)]
				   public override bool VerifyChangeMaster(string currentMasterHostID, string newMasterHostID)
				   {
					   return base.VerifyChangeMaster( currentMasterHostID, newMasterHostID);
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				if( !IsAccessAllowed(userID) )
				{
					/// access violation ...
					return null;
				}
			}
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				if( !IsAccessAllowed(userID) )
				{
					/// access violation...
					return null;
				}
			}
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

		/// <summary>
		/// Get information about all of the iFolder users identified by the search property, operation, and pattern.
		/// </summary>
		/// <param name="property">The property to search.</param>
		/// <param name="operation">The operation to compare the property and pattern.</param>
		/// <param name="pattern">The pattern to search</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <param name="SecondaryAdminID">SecondaryAdministrator ID</param>
		/// <param name="GetMonitoredGroups">a bool telling whether monitored groups by this admin is required or not</param>
		/// <returns>A set of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users identified by the search property, operation, and pattern.",
			 EnableSession=true)]
		public override iFolderUserSet GetMonitoredGroupsBySearch(SearchProperty property, SearchOperation operation, string pattern, int index, int max, string SecondaryAdminiD, bool GetMonitoredGroups, bool adminrequest)
		{
			return base.GetMonitoredGroupsBySearch(property, operation, pattern, index, max, SecondaryAdminiD, GetMonitoredGroups, adminrequest);
		}

		/// <summary>
		/// Get information about all of the iFolder users with DATA move property set.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users with DATA move property set.",
			 EnableSession=true)]
		public override iFolderUserSet GetReprovisionUsers( int index, int max)
		{
			return base.GetReprovisionUsers(index, max);
		}

		#endregion

		#region iFolder

                 /// <summary>
                 /// Create a new iFolder with given ID.
                 /// </summary>
                 /// <param name="name">The name of the new iFolder.</param>
                 /// <param name="userID">The user id of the owner of the new iFolder.</param>
                 /// <param name="description">The description of the new iFolder (can be null).</param>
                 /// <returns>An iFolder object describing the new iFolder.</returns>
                 [WebMethod(
                          Description="Create a new encrypted iFolder.",
                          EnableSession=true)]
                 public override iFolder CreateEncryptediFolderWithID(string name, string userID, string description, string iFolderID, string eKey, string eBlob, string eAlgorithm, string rKey)
                 {
                         return base.CreateEncryptediFolderWithID(name, userID, description, iFolderID, eKey, eBlob, eAlgorithm, rKey);
                 }               /// <summary>

	

		/// <summary>
		/// Create a new iFolder.
		/// </summary>
		/// <param name="name">The name of the new iFolder.</param>
		/// <param name="userID">The user id of the owner of the new iFolder.</param>
		/// <param name="description">The description of the new iFolder (can be null).</param>
		/// <returns>An iFolder object describing the new iFolder.</returns>
		[WebMethod(
			 Description="Create a new iFolder.",
			 EnableSession=true)]
		public override iFolder CreateiFolder(string name, string userID, string description)
		{
			return base.CreateiFolder(name, userID, description);
		}

                /// <summary>
                /// Create a new iFolder with given ID.
                /// </summary>
                /// <param name="name">The name of the new iFolder.</param>
                /// <param name="userID">The user id of the owner of the new iFolder.</param>
                /// <param name="description">The description of the new iFolder (can be null).</param>
                /// <returns>An iFolder object describing the new iFolder.</returns>
                [WebMethod(
                         Description="Create a new iFolder.",
                         EnableSession=true)]
                public override iFolder CreateiFolderWithID(string name, string userID, string description, string iFolderID)
                {
                        return base.CreateiFolderWithID(name, userID, description, iFolderID);
                }

                /// <summary>

		/// <summary>
		/// Get inforamtion about all iFolders.
		/// </summary>
		/// <param name="type">An iFolder type filter of the results.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder objects.</returns>
		[WebMethod(
			 Description="Get iFolders",
			 EnableSession=true)]
		public override iFolderSet GetiFolders(iFolderType type, int index, int max)
		{
			return base.GetiFolders(type, index, max);
		}

		/// <summary>
		/// Get information about all iFolders identified by a member.
		/// </summary>
		/// <param name="userID">The user id of the member.</param>
		/// <param name="role">The member's role in the iFolder.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about all iFolders identified by a member.",
			 EnableSession=true)]
		public override iFolderSet GetiFoldersByMember(string userID, MemberRole role, int index, int max)
		{
			return base.GetiFoldersByMember(userID, role, index, max);
		}

		/// <summary>
		/// Get information about all iFolders identified by a search on the it's name.
		/// </summary>
		/// <param name="operation">The operation to compare the name and pattern.</param>
		/// <param name="pattern">The pattern to search.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about all iFolders identified by a search on the it's name.",
			 EnableSession=true)]
		public override iFolderSet GetiFoldersByName(SearchOperation operation, string pattern, int index, int count)
		{
			return base.GetiFoldersByName(operation, pattern, index, count);
		}

		#endregion

		#region Settings
		
		/// <summary>
		/// Get a setting global to the system.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting.</returns>
		[WebMethod(
			 Description="Get a setting global to the system.",
			 EnableSession=true)]
		public override string GetSystemSetting(string name)
		{
			return base.GetSystemSetting(name);
		}

		/// <summary>
		/// Set a setting global to the system.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		[WebMethod(
			 Description="Set a setting global to the system.",
			 EnableSession=true)]
		public override void SetSystemSetting(string name, string value)
		{
			base.SetSystemSetting(name, value);
		}
		
		/// <summary>
		/// Get a setting specific to a user.
		/// </summary>
		/// <param name="userID">The id of the user.</param>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting.</returns>
		[WebMethod(
			 Description="Get a setting specific to a user.",
			 EnableSession=true)]
		public override string GetUserSetting(string userID, string name)
		{
			return base.GetUserSetting(userID, name);
		}

		/// <summary>
		/// Set a setting specific to a user.
		/// </summary>
		/// <param name="userID">The id of the user.</param>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		[WebMethod(
			 Description="Set a setting specific to a user.",
			 EnableSession=true)]
		public override void SetUserSetting(string userID, string name, string value)
		{
			base.SetUserSetting(userID, name, value);
		}

		/// <summary>
		/// Get a setting specific to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting.</returns>
		[WebMethod(
			 Description="Get a setting specific to an iFolder.",
			 EnableSession=true)]
		public override string GetiFolderSetting(string ifolderID, string name)
		{
			return base.GetiFolderSetting(ifolderID, name);
		}

		/// <summary>
		/// Set a setting specific to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		[WebMethod(
			 Description="Set a setting specific to an iFolder.",
			 EnableSession=true)]
		public override void SetiFolderSetting(string ifolderID, string name, string value)
		{
			base.SetiFolderSetting(ifolderID, name, value);
		}

		#endregion

		#region Users
		
		/// <summary>
		/// Create a new user in the iFolder system.
		/// </summary>
		/// <param name="username">A short unique name for the user (required).</param>
		/// <param name="password">The password for the user (required).</param>
		/// <param name="guid">A GUID for the user (optional).</param>
		/// <param name="firstName">The first/given name of the user (optional).</param>
		/// <param name="lastName">The last/family name of the user (optional).</param>
		/// <param name="fullName">The full name of the user (optional).</param>
		/// <param name="dn">The distinguished name, from an external identity store, for the user (optional).</param>
		/// <param name="email">The primary email address of the user (optional).</param>
		/// <returns>An iFolderUser object describing the new user.</returns>
		/// <remarks>
		/// Some identity providers DO NOT allow the creation,
		/// modification or deletion of new users.
		/// </remarks>
		/// <remarks>
		/// If the firstName and lastName are specified but the fullName is null, the fullName is
		/// created with the firstName and lastName.
		/// </remarks>
		[WebMethod(
			 Description= "Create a new user in the iFolder system.",
			 EnableSession = true)]
		public override iFolderUser CreateUser(string username, string password, string guid, string firstName,
			string lastName, string fullName, string dn, string email)
		{
			return base.CreateUser(username, password, guid, firstName, lastName, fullName, dn, email);
		}
		
		/// <summary>
		/// Delete a user from the iFolder system.
		/// </summary>
		/// <param name="userID">The ID of the user to be deleted.</param>
		/// <remarks>
		/// Some identity providers DO NOT allow the creation,
		/// modification or deletion of new users.
		/// </remarks>
		[WebMethod(
			 Description= "Delete a user from the iFolder system.",
			 EnableSession = true)]
		public override bool DeleteUser(string userID)
		{
			return base.DeleteUser(userID);
		}

		/// <summary>
		/// Update a user in the iFolder system.
		/// </summary>
		/// <param name="userID">The ID of the user to be updated.</param>
		/// <param name="user">The update iFolderUser object (FullName, FirstName, LastName, Email).</param>
		/// <remarks>
		/// Some identity providers DO NOT allow the creation,
		/// modification or deletion of new users.
		/// </remarks>
		[WebMethod(
			 Description= "Update a user in the iFolder system (FullName, FirstName, LastName, Email).",
			 EnableSession = true)]
		public override iFolderUser SetUser(string userID, iFolderUser user)
		{
			return base.SetUser(userID, user);
		}

		/// <summary>
		/// Set a user's password.
		/// </summary>
		/// <param name="userID">The username of the user.</param>
		/// <param name="password">The new password for the user.</param>
		/// <remarks>
		/// Some identity providers DO NOT allow the creation,
		/// modification or deletion of new users.
		/// </remarks>
		[WebMethod(
			 Description= "Set a user's password.",
			 EnableSession = true)]
		public override bool SetPassword(string userID, string password)
		{
			return base.SetPassword(userID, password);
		}

		#endregion

		#region System

		/// <summary>
		/// Update the editable information for the iFolder system.
		/// </summary>
		/// <param name="system">The update iFolderSystem object (Name, Description).</param>
		[WebMethod(
			 Description= "Update the editable information for the iFolder system (Name, Description).",
			 EnableSession = true)]
		public override void SetSystem(iFolderSystem system)
		{
			base.SetSystem(system);
		}

		/// <summary>
		/// Update the ssl information for the iFolder system.
		/// </summary>
		/// <param name="SimiasSSLStatus">The updated ssl info as string (ssl/nonssl/both)</param>
		[WebMethod(
			 Description= "Update the ssl information for the iFolder system .",
			 EnableSession = true)]
		public override bool SetSimiasSSLStatus(string SimiasSSLStatus)
		{
			return base.SetSimiasSSLStatus(SimiasSSLStatus);
		}

		#endregion

		#region Administrators

		/// <summary>
		/// Grant a user system administration rights.
		/// </summary>
		/// <param name="userID">The id of user.</param>
		/// <remarks>A user is an administrator if the user has "Admin" rights in the domain.</remarks>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Grant a user system administration rights.",
			 EnableSession=true)]
		public override void AddAdministrator(string userID)
		{
			base.AddAdministrator(userID);
		}

		/// <summary>
		/// Grant a user system administration rights.
		/// </summary>
		/// <param name="userID">The id of user.</param>
		/// <remarks>A user is an administrator if the user has "Admin" rights in the domain.</remarks>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Grant a user system administration rights.",
			 EnableSession=true)]
		public override void AddGroupAdministrator(string groupid, string userID, int preference)
		{
			base.AddGroupAdministrator(groupid, userID, preference);
		}

		/// <summary>
		/// Grant a user system administration rights.
		/// </summary>
		/// <param name="userID">The id of user.</param>
		/// <remarks>A user is an administrator if the user has "Admin" rights in the domain.</remarks>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Grant a user system administration rights.",
			 EnableSession=true)]
		public override void RemoveGroupAdministrator(string groupid, string userID)
		{
			base.RemoveGroupAdministrator(groupid, userID);
		}

		/// <summary>
		/// Grant a user system administration rights.
		/// </summary>
		/// <param name="userID">The id of user.</param>
		/// <remarks>A user is an administrator if the user has "Admin" rights in the domain.</remarks>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Grant a user system administration rights.",
			 EnableSession=true)]
		public override bool DiskQuotaPolicyChangeAllowed(string userID, long limit )
		{
			return base.DiskQuotaPolicyChangeAllowed(userID, limit);
		}

		/// <summary>
		/// Revoke system administration System rights from a user.
		/// </summary>
		/// <param name="userID">The user id of the administrator.</param>
		/// <remarks>Administration rights are removed by the user having "ReadOnly" rights in the domain.</remarks>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Revoke system administration rights from a user.",
			 EnableSession=true)]
		public override int GetUserSystemRights(string userid, string groupid)
		{
			return base.GetUserSystemRights(userid, groupid);
		}

		/// <summary>
		/// Revoke system administration Group rights from a user.
		/// </summary>
		/// <param name="userID">The user id of the administrator.</param>
		/// <remarks>Administration rights are removed by the user having "ReadOnly" rights in the domain.</remarks>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Revoke system administration rights from a user.",
			 EnableSession=true)]
		public override int GetUserGroupRights(string userid, string groupid)
		{
			return base.GetUserGroupRights(userid, groupid);
		}

		/// <summary>
		/// Revoke system administration rights from a user.
		/// </summary>
		/// <param name="userID">The user id of the administrator.</param>
		/// <remarks>Administration rights are removed by the user having "ReadOnly" rights in the domain.</remarks>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Revoke system administration rights from a user.",
			 EnableSession=true)]
               public override string[] GetMonitoredGroups(string userid)
                {
                        return base.GetMonitoredGroups(userid);
                }

		/// <summary>
		/// GetMonitoredgroup names in an array.
		/// </summary>
		/// <param name="userID">The user id of the secondary administrator.</param>
		[WebMethod(
			 Description="GetMonitoredgroup names in an array",
			 EnableSession=true)]
               public override string[] GetMonitoredGroupNames(string userid)
                {
                        return base.GetMonitoredGroupNames(userid);
                }

		/// <summary>
		/// Grant a user system administration rights.
		/// </summary>
		/// <param name="userID">The id of user.</param>
		/// <remarks>A user is an administrator if the user has "Admin" rights in the domain.</remarks>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Grant a user system administration rights.",
			 EnableSession=true)]
		public override long GetAggregateDiskQuota(string groupid)
		{
			return base.GetAggregateDiskQuota(groupid);
		}

		/// <summary>
		/// Get space used by all members of group.
		/// </summary>
		/// <param name="groupid">The id of group.</param>
		[WebMethod(
			 Description="Grant a user system administration rights.",
			 EnableSession=true)]
		public override long SpaceUsedByGroup(string groupid)
		{
			return base.SpaceUsedByGroup(groupid);
		}

		/// <summary>
		/// Set aggregate disk quota for a group.
		/// </summary>
		/// <param name="groupid">group id</param>
		/// <param name="value">value to be set</param>
		[WebMethod(
			 Description="Grant a user system administration rights.",
			 EnableSession=true)]
		public override bool SetAggregateDiskQuota(string groupid, long value)
		{
			return base.SetAggregateDiskQuota(groupid, value);
		}

		/// <summary>
		/// Revoke system administration rights from a user.
		/// </summary>
		/// <param name="userID">The user id of the administrator.</param>
		/// <remarks>Administration rights are removed by the user having "ReadOnly" rights in the domain.</remarks>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Revoke system administration rights from a user.",
			 EnableSession=true)]
		public override void RemoveAdministrator(string userID)
		{
			base.RemoveAdministrator(userID);
		}

		/// <summary>
		/// Get information about all the administrators.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <remarks>A user is an administrator if the user has "Admin" rights in the domain.</remarks>
		/// <returns>An array of iFolderUser objects describing the administrators.</returns>
		[WebMethod(
			 Description="Get information about all the administrators.",
			 EnableSession=true)]
		public override iFolderUserSet GetAdministrators(int index, int max, int admintype)
		{
			return base.GetAdministrators(index, max, admintype);
		}
	
		#endregion

		#region Policy
		
		/// <summary>
		/// Get policy information for the system.
		/// </summary>
		/// <returns>A SystemPolicy object.</returns>
		[WebMethod(
			 Description="Get policy information for the system.",
			 EnableSession=true)]
		public override SystemPolicy GetSystemPolicy()
		{
			return base.GetSystemPolicy();
		}

                /// <summary>
                /// Get policy information for Group.
                /// </summary>
                /// <returns>A integer value of user's group objects sharing policy object.</returns>
		[WebMethod(
			 Description="Get policy information for User groups sharing policy value.",
			 EnableSession=true)]
		public override int GetGroupSharingPolicy(string userID)
		{
			return base.GetGroupSharingPolicy(userID);
		}

                /// <summary>
                /// Get Encryption policy information for Group.
                /// </summary>
                /// <returns>A integer value of user's group objects Encryption policy object.</returns>
		[WebMethod(
			 Description="Get policy information for User groups Encryption policy value.",
			 EnableSession=true)]
		public override int GetGroupEncryptionPolicy(string userID)
		{
			return base.GetGroupEncryptionPolicy(userID);
		}

		/// <summary>
		/// Set policy for the iFolder system.
		/// </summary>
		/// <param name="policy">The SystemPolicy object.</param>
		[WebMethod(
			 Description="Set policy for the iFolder system.",
			 EnableSession=true)]
		public override void SetSystemPolicy(SystemPolicy policy)
		{
			base.SetSystemPolicy(policy);
		}

		/// <summary>
		/// Get policy information for a user.
		/// </summary>
		/// <param name="userID">A id of user.</param>
		/// <returns>A UserPolicy object.</returns>
		[WebMethod(
			 Description="Get policy information for a user.",
			 EnableSession=true)]
		public override UserPolicy GetUserPolicy(string userID, string AdminId)
		{
			return base.GetUserPolicy(userID, AdminId);
		}

		/// <summary>
		/// Set the policy for a user.
		/// </summary>
		/// <param name="policy">The UserPolicy object.</param>
		[WebMethod(
			 Description="Set the policy for a user.",
			 EnableSession=true)]
		public override void SetUserPolicy(UserPolicy policy)
		{
			base.SetUserPolicy(policy);
		}

		/// <summary>
		/// Get policy information for an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of an iFolder.</param>
		/// <returns>A iFolderPolicy object.</returns>
		[WebMethod(
			 Description="Get policy information for an iFolder.",
			 EnableSession=true)]
		public override iFolderPolicy GetiFolderPolicy(string ifolderID, string adminID)
		{
			return base.GetiFolderPolicy(ifolderID, adminID);
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
		
		/// <summary>
		/// Get policy information for the registered identity provider.
		/// </summary>
		/// <returns>An IdentityPolicy object.</returns>
		[WebMethod(
			Description="Get policy information for the registered identity provider.",
			EnableSession = true)]
		public override IdentityPolicy GetIdentityPolicy()
		{
			return base.GetIdentityPolicy();
		}
		
		#endregion

		#region Identity Sync

		/// <summary>
		/// Disables/enables the identity synchronization service.
		/// true - disables
		/// false - enables the synchronization service
		/// Note! once enabled the service will enter a
		/// synchronization cycle ignoring the configured
		/// sync interval time.
		/// </summary>
		///
		[WebMethod(
			 Description= "Disables/enables the identity synchronization service.",
			 EnableSession = true)]
		public override void IdentitySyncDisableService(bool disable)
		{
			base.IdentitySyncDisableService(disable);
		}

		/// <summary>
		/// Get detailed information about the last synchronization cycle.
		/// </summary>
		[WebMethod(
			 Description= "Get detailed information about the last synchronization cycle.",
			 EnableSession = true)]
		public override LastSyncInfo IdentitySyncGetLastInfo()
		{
			return base.IdentitySyncGetLastInfo();
		}

		/// <summary>
		/// Get the current status of the identity sync service thread.
		/// status could be:
		/// Disabled
		/// Working
		/// Waiting
		/// Authentication Failure
		/// etc..
		/// </summary>
		/// <returns>current status as SyncServiceInfo object</returns>
		[WebMethod(
			 Description= "Get the current status of the identity sync service thread.",
			 EnableSession = true)]
		public override SyncServiceInfo IdentitySyncGetServiceInfo()
		{
			return base.IdentitySyncGetServiceInfo();
		}

		/// <summary>
		/// Method to set the grace period a member is given
		/// before they are removed from the domain.
		/// Members are disabled during this grace period.
		/// Represented in seconds
		/// </summary>
		[WebMethod(
			 Description= "Set the grace period for a member.",
			 EnableSession = true)]
		public override void IdentitySyncSetDeleteMemberGracePeriod(int seconds)
		{
			base.IdentitySyncSetDeleteMemberGracePeriod(seconds);
		}
	
		/// <summary>
		/// Method to set the synchronization interval for the
		/// sync engine.  Represented in seconds
		/// </summary>
		[WebMethod(
			 Description= "Set the synchronization interval for the identity sync service.",
			 EnableSession = true)]
		public override void IdentitySyncSetInterval(int seconds)
		{
			base.IdentitySyncSetInterval(seconds);
		}

		/// <summary>
		/// Tells the sync service to immediately start
		/// a synchronization cycle.
		/// </summary>
		///
		[WebMethod(
			 Description= "Cause the Identity Sync Service to synchronize immediately.",
			 EnableSession = true)]
		public override void IdentitySyncNow()
		{
			base.IdentitySyncNow();
		}

		/// <summary>
		/// Get the loglevels set in Simias.log4net
		/// </summary>
		///
		[WebMethod(
			 Description= "Cause the Identity Sync Service to synchronize immediately.",
			 EnableSession = true)]
		public override string[] GetLogLevels()
		{
			return base.GetLogLevels();
		}

		/// <summary>
		/// Get the loglevels set in Simias.log4net
		/// </summary>
		///
		[WebMethod(
			 Description= "Cause the Identity Sync Service to synchronize immediately.",
			 EnableSession = true)]
		public override void SetLogLevel(iFolderServer.LoggerType type, string logLevel )
		{
			base.SetLogLevel(type, logLevel);
		}

		/// <summary>
		/// Get information about a user using an id or username.
		/// </summary>
		/// <param name="userID">The id or username of the user.</param>
		/// <returns>A iFolderUser object.</returns>
		[WebMethod(
			 Description="Get information about a user using an id or username.",
			 EnableSession=true)]
		public override LdapInfo GetLdapDetails()
		{
			return base.GetLdapDetails ();

		}

		/// <summary>
		/// Set information about a user using an id or username.
		/// </summary>
		/// <param name="userID">The id or username of the user.</param>
		/// <returns>A iFolderUser object.</returns>
		[WebMethod(
			 Description="Get information about a user using an id or username.",
			 EnableSession=true)]
		public override void SetLdapDetails(LdapInfo ldapInfo, string LdapAdminDN, string LdapAdminPwd, string ServerID)
		{
			base.SetLdapDetails (ldapInfo, LdapAdminDN, LdapAdminPwd, ServerID);
		}

		#endregion
	}
}
