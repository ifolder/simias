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
using System.Reflection;

using Simias.Storage;
using Simias.Server;
using Simias.LdapProvider;

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder Admin Local Web Service
	/// </summary>
	[WebService(
		 Namespace="http://novell.com/ifolder/webservice/",
		 Name="iFolderAdminLocal",
		 Description="iFolder Admin Local Web Service")]
	public class iFolderAdminLocal : iFolderCommonLocal
	{
		delegate int IdentitySynchronizationMethod( string Data );
		delegate int IdentitySynchronizationIntervalSetMethod( int interval );
		string identitySyncClass = "Simias.IdentitySynchronization.Service";

                /// enum to store different disable sharing value
                public enum Share
                {
                        Sharing = 1,
                        EnforcedSharing = 4,
                        DisableSharing = 8
                }


		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderAdminLocal()
		{
		}

		#endregion

		#region iFolder

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
		public virtual iFolder CreateiFolder(string name, string userID, string description)
		{
			iFolder result = null;

			try
			{
				Authorize();

			        result = iFolder.CreateiFolder(name, userID, description);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		[WebMethod(
			 Description=" write ldap details to a file",
			 EnableSession=true)]
		public virtual void GetProxyInfo()
		{
			try
			{

			    	iFolderServer.WriteLdapDetails();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

		}

		/// <summary>
		/// Create a new iFolder with a given ID.
		/// </summary>
		/// <param name="name">The name of the new iFolder.</param>
		/// <param name="userID">The user id of the owner of the new iFolder.</param>
		/// <param name="description">The description of the new iFolder (can be null).</param>
		/// <returns>An iFolder object describing the new iFolder.</returns>
		[WebMethod(
			 Description="Create a new iFolder.",
			 EnableSession=true)]
		public virtual iFolder CreateiFolderWithID(string name, string userID, string description, string iFolderID)
		{
			iFolder result = null;

			try
			{
				Authorize();

			        result = iFolder.CreateiFolder(name, userID, description, iFolderID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}


                /// <summary>
                /// Create a new iFolder with a given ID.
                /// </summary>
                /// <param name="name">The name of the new iFolder.</param>
                /// <param name="userID">The user id of the owner of the new iFolder.</param>
                /// <param name="description">The description of the new iFolder (can be null).</param>
                /// <returns>An iFolder object describing the new iFolder.</returns>
                [WebMethod(
                         Description="Create a new iFolder.",
                         EnableSession=true)]
                public virtual iFolder CreateEncryptediFolderWithID(string name, string userID, string description, string iFolderID, string eKey, string eBlob, string eAlgorithm, string rKey)
                {
                        iFolder result = null;
 
                        try
                        {
                                Authorize();
                                result = iFolder.CreateEncryptediFolderWithID(name, userID, description, iFolderID, eKey, eBlob, eAlgorithm, rKey);
                        }
                        catch(Exception e)
                        {
                                SmartException.Throw(e);
                        }
 
                        return result;
                }

                /// <summary>
                /// Delete an iFolder
                /// </summary>
                /// <param name="ifolderID">The id of the iFolder to be deleted.</param>
                /// <remarks>This API will accept multiple iFolder ids in a comma delimited list.</remarks>
                [WebMethod(
                         Description="Delete an iFolder",
                         EnableSession=true)]
                public virtual int RestoreiFolderData(string url, string adminname, string adminpassword, string ifolderid, string relativepath, string basepath, int startindex, string LogLocation)
                {
                        return iFolder.RestoreiFolderData(url, adminname, adminpassword, ifolderid, relativepath, basepath, startindex, LogLocation);
                }
 
                /// <summary>
                /// Delete an iFolder
                /// </summary>
                /// <param name="ifolderID">The id of the iFolder to be deleted.</param>
                /// <remarks>This API will accept multiple iFolder ids in a comma delimited list.</remarks>
                [WebMethod(
                         Description="Delete an iFolder",
                         EnableSession=true)]
                public virtual int GetRestoreStatusForCollection(string ifolderid, out int totalcount, out int finishedcount)
                {
                        return iFolder.GetRestoreStatusForCollection(ifolderid, out totalcount, out finishedcount);
                }




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
		public virtual iFolderSet GetiFolders(iFolderType type, int index, int max)
		{
			iFolderSet result = null;

			try
			{
				Authorize();
				string accessID =  GetUserID();

				result = iFolder.GetiFoldersByName(type, SearchOperation.BeginsWith, "", index, max, accessID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public virtual iFolderSet GetiFoldersByMember(string userID, MemberRole role, int index, int max)
		{
			iFolderSet result = null;

			try
			{
				Authorize();
				if( !IsAccessAllowed( userID ))
					return new iFolderSet((iFolder[])(new ArrayList()).ToArray(typeof(iFolder)), 0);
				result = iFolder.GetiFoldersByMember(userID, role, index, max, GetAccessIDForGroup(), true);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public virtual iFolderSet GetiFoldersByName(SearchOperation operation, string pattern, int index, int count)
		{
			iFolderSet result = null;

			try
			{
				Authorize();

				result = iFolder.GetiFoldersByName(iFolderType.All, operation, pattern, index, count, GetUserID() );
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public virtual string GetSystemSetting(string name)
		{
			string result = null;

			try
			{
				Authorize();

				result = Settings.GetSystemSetting(name);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Set a setting global to the system.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		[WebMethod(
			 Description="Set a setting global to the system.",
			 EnableSession=true)]
		public virtual void SetSystemSetting(string name, string value)
		{
			try
			{
				Authorize();

				Settings.SetSystemSetting(name, value);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return;
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
		public virtual string GetUserSetting(string userID, string name)
		{
			string result = null;

			try
			{
				Authorize();

				result = Settings.GetUserSetting(userID, name);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

                /// <summary>
                /// evaluate the disable sharing policy for an iFolder
                /// </summary>
                /// <returns> an integer value which has disable sharing policy information  </returns>
                [WebMethod(
                        Description="evaluate the disable sharing policy",
                        EnableSession=true)]
                public virtual bool GetSharingPolicy(string iFolderID)
                {
	  	   try
		   {
			Authorize();
	               //return base.GetDisableSharingPolicy ( iFolderID );
                        iFolder ifolder = base.GetiFolder(iFolderID);
                        UserPolicy userPolicy = UserPolicy.GetPolicy(ifolder.OwnerID);
                        SystemPolicy systemPolicy = SystemPolicy.GetPolicy();
                        iFolderPolicy ifolderPolicy = iFolderPolicy.GetPolicy(iFolderID, GetAccessID());
                        int iFolderSharingStatus = ifolderPolicy.SharingStatus;
                        int UserSharingStatus = userPolicy.SharingStatus;
                        int GroupSharingStatus = UserPolicy.GetUserGroupSharingPolicy(ifolder.OwnerID);
                        int SystemSharingStatus = systemPolicy.SharingStatus;
                        if(( (SystemSharingStatus & (int) Share.EnforcedSharing) == (int) Share.EnforcedSharing))
                        {
                                /// If on system level or user level, enforcement of policy is there, it means the iFolder must not be shared
                                if( (SystemSharingStatus & (int) Share.Sharing) == (int) Share.Sharing)
                                        return true;
                                return false;
                        }
                        else if(( (GroupSharingStatus & (int) Share.EnforcedSharing) == (int) Share.EnforcedSharing))
                        {
                                if( (GroupSharingStatus & (int) Share.Sharing) == (int) Share.Sharing)
                                        return true;
                                return false;
                        }
                        else if(( (UserSharingStatus & (int) Share.EnforcedSharing) == (int) Share.EnforcedSharing))
                        {
                                if( (UserSharingStatus & (int) Share.Sharing) == (int) Share.Sharing)
                                        return true;
                                return false;
                        }
                        else
                        {
                                if(iFolderSharingStatus != 0 )
                                {
                                       if( (iFolderSharingStatus & (int) Share.Sharing) == (int) Share.Sharing || iFolderSharingStatus == 0)
                                        {
                                                /// it means, on iFolder Details page, admin had unchecked the box so sharing is enabled now
                                                return true;
                                        }
                                        if( (iFolderSharingStatus & (int) Share.DisableSharing) == (int) Share.DisableSharing)
                                        {
                                                /// it means, on iFolder Details page, admin had checked the box so sharing is disabled
                                                return false;
                                        }
                                }
                                else
                                {
                                        /// no iFolder level policy present , now if current user is not an owner , then check for system level policy
                                        /// if current user is owner of the iFolder, then check for user level and then for system level

                                        if( (UserSharingStatus & (int) Share.Sharing) == (int) Share.Sharing )
                                        {
	   	                                /// it means, on User Details page, admin had unchecked the box so sharing is enabled now
                                                return true;
                                        }
                                        if( (UserSharingStatus & (int) Share.DisableSharing) == (int) Share.DisableSharing)
                                        {
  	                                      /// it means, on User Details page, admin had checked the box so sharing is disabled
                                              return false;
                                        }

                                        /// check for Group level policy as there was no user level or ifolder level policy applied
                                        /// No policy found on iFolder level or User level, no enforcement also, so follow group level
                                        if( (GroupSharingStatus & (int) Share.DisableSharing) == (int) Share.DisableSharing)
                                        {
    	                                	return false;
                                        }
                                        if( (GroupSharingStatus & (int) Share.Sharing) == (int) Share.Sharing )
                                        {
                                                return true;
                                        }

                                        /// check for system level policy as there was no user level or ifolder level policy applied
                                        /// No policy found on iFolder level or User level, no enforcement also, so follow system level
                                        if( (SystemSharingStatus & (int) Share.DisableSharing) == (int) Share.DisableSharing)
                                        {
                                                return false;
                                        }
                                        if( (SystemSharingStatus & (int) Share.Sharing) == (int) Share.Sharing || SystemSharingStatus == 0)
                                        {
                                                return true;
                                        }

                                }
                        }
 		}
		catch(Exception e)
		{
			SmartException.Throw(e);
		}
                       return false;

                }


		/// <summary>
                /// Get the ifolder limit policy for a User.
                /// </summary>
                /// <param name="userID">The user id of the user who owns the ifolder.</param>
                [WebMethod(
                         Description="Get ifolder limit policy information for an iFolder.",
                         EnableSession=true)]
		public virtual int GetiFolderLimitPolicyStatus(string userID)
                {
                        long userpolicy = 0,syspolicy = 0;
                        UserPolicy user = null;
                        SystemPolicy system = null;
                        int result = 1;

                        try
                        {
                                iFolderUserDetails userdetails = iFolderUserDetails.GetDetails( userID );
                                user = UserPolicy.GetPolicy(userID);
                                system = SystemPolicy.GetPolicy();
                                userpolicy = user.NoiFoldersLimit;
                                syspolicy = system.NoiFoldersLimit;

                                if(  userpolicy != -1 && userpolicy != -2)
                                {
                                	if( userpolicy <= userdetails.OwnediFolderCount )
                                	result = 0;
                                }
        	                else
                	        {
                                	if(Simias.Service.Manager.LdapServiceEnabled == true)
                                	{
                                		int groupStatus = UserPolicy.GetUserGroupiFolderLimitPolicy(userID,                                                                                                  userdetails.OwnediFolderCount);
                                		if(groupStatus == 0)
                                			return result;
                                		else if(groupStatus == -1)
                                			result = 0;
                                	}
                        		if( syspolicy  <= userdetails.OwnediFolderCount && syspolicy !=-1)
                                		result = 0;
	                        }
				return result;
                        }
                        catch(Exception e)
                        {
                                SmartException.Throw(e);
                        }
                        return result;
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
		public virtual void SetUserSetting(string userID, string name, string value)
		{
			try
			{
				Authorize();

				Settings.SetUserSetting(userID, name, value);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return;
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
		public virtual string GetiFolderSetting(string ifolderID, string name)
		{
			string result = null;

			try
			{
				Authorize();

				result = Settings.GetCollectionSetting(ifolderID, name);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public virtual void SetiFolderSetting(string ifolderID, string name, string value)
		{
			try
			{
				Authorize();

				Settings.SetCollectionSetting(ifolderID, name, value);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return;
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
		public virtual iFolderUser CreateUser(string username, string password, string guid, string firstName,
			string lastName, string fullName, string dn, string email)
		{
			iFolderUser result = null;

			try
			{
				Authorize();

				// check if the registered provider allows user creation
				IUserProvider provider = Simias.Server.User.GetRegisteredProvider();
				UserProviderCaps caps = provider.GetCapabilities();
				
				if (caps.CanCreate == false)
				{
					throw new NotSupportedException("The current identity provider does not allow user creation.");
				}
				else if (username == null || username.Length == 0 || password == null)
				{
					throw new InvalidOperationException("Missing required parameters.");
				}
				else
				{
					Simias.Server.User user = new Simias.Server.User(username);
					user.FirstName = firstName;
					user.LastName = lastName;
					user.UserGuid = guid;
					user.FullName = fullName;
					user.DN = dn;
					user.Email = email;
				
					RegistrationInfo info = user.Create(password);

					result = iFolderUser.GetUser(info.UserGuid, GetAccessID());
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			
			return result;
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
		public virtual bool DeleteUser(string userID)
		{
			bool status = false;
			try
			{
				Authorize();

				// check if the registered provider allows deletes
				IUserProvider provider = Simias.Server.User.GetRegisteredProvider();
				UserProviderCaps caps = provider.GetCapabilities();
				
				if (caps.CanDelete == true)
				{
					if ((userID != null) && (userID.Length != 0))
					{
						Simias.Server.User user = new Simias.Server.User(userID);
						status = user.Delete();
					}
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			
			return status;
		}

		/// <summary>
		/// Remove a user from usermove queue. Precondition is: UserMove must not have started for this user.
		/// </summary>
		/// <param name="userID">The user id </param>
		[WebMethod(
			 Description="Remove an user from usermove queue",
			 EnableSession=true)]
		public virtual bool DeleteFromUserMoveQueue( string userID )
		{
			bool retval = false;
			try
			{
				Authorize();
				retval = iFolderUser.DeleteFromUserMoveQueue(userID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return retval;
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
		public virtual iFolderUser SetUser(string userID, iFolderUser user)
		{
			iFolderUser result = null;

			try
			{
				Authorize();

				// check if the registered provider allows deletes
				IUserProvider provider = Simias.Server.User.GetRegisteredProvider();
				UserProviderCaps caps = provider.GetCapabilities();
				
				if (caps.CanModify == false)
				{
					throw new NotSupportedException("The current identity provider does not allow user modification.");
				}

				result = iFolderUser.SetUser(userID, user, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			
			return result;
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
		public virtual bool SetPassword(string userID, string password)
		{
			bool status = false;
			
			try
			{
				Authorize();

				// check if the registered provider allows modification
				IUserProvider provider = Simias.Server.User.GetRegisteredProvider();
				UserProviderCaps caps = provider.GetCapabilities();
				
				if (caps.CanModify == true)
				{
					if ((userID != null) && (userID.Length != 0) && (password != null))
					{
						status = Simias.Server.User.SetPassword(userID, password);
					}
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			
			return status;
		}

		/// <summary>
		/// return whether passphrase is set for that user.
		/// </summary>
		/// <param name="userID">The id of the user.</param>
		[WebMethod(
			 Description="check whether passphrase is set.",
			 EnableSession=true)]
		public virtual bool IsPassPhraseSetForUser(string userID)
		{
			bool PassPhraseSet = false;
			try
			{
				Authorize();
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				PassPhraseSet = iFolderUser.IsPassPhraseSet(domain.ID, userID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return PassPhraseSet;
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
		public virtual void SetSystem(iFolderSystem system)
		{
			try
			{
				Authorize();

				iFolderSystem.SetSystem(system);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Update the ssl value for the system.
		/// </summary>
		/// <param name="system">The update iFolderSystem object (SSL value will be used only).</param>
		[WebMethod(
			 Description= "Update the ssl information for the iFolder system (Name, Description).",
			 EnableSession = true)]
		public virtual bool SetSimiasSSLStatus(string SimiasSSLValue)
		{
			bool result = false;
			try
			{
				Authorize();
				result = iFolderServer.SetSimiasSSLStatus(SimiasSSLValue);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return result;
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
		public virtual void AddAdministrator(string userID)
		{
			Hashtable exceptions = new Hashtable();

			try
			{
				Authorize();

				string[] ids = userID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolderUser.AddAdministrator(id);
						}
						catch(Exception e)
						{
							exceptions.Add(id, e);
						}
					}
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			SmartException.Throw(exceptions);
		}

		/// <summary>
		/// Get User System rights from a user.
		/// </summary>
		/// <param name="userID">The user id of the administrator.</param>
		[WebMethod(
			 Description="Get User's right for the system",
			 EnableSession=true)]
		public virtual int GetUserSystemRights(string userid, string sysID)
		{
			try
			{
				Authorize();

						try
						{
							if(sysID != null)
								return iFolderUser.GetAdminRights(userid, sysID);
							else
							{
								HostNode hNode = HostNode.GetLocalHost();	
								if(hNode != null && hNode.UserID != null)
									return iFolderUser.GetAdminRights(userid, hNode.UserID);
							}
						}
						catch {}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return 0;
		}

		/// <summary>
		/// Get group rights for a user.
		/// </summary>
		/// <param name="userID">The user id of the administrator.</param>
		/// <param name="groupid">The groupid for which we want to get the rights..</param>
		[WebMethod(
			 Description="Get User Rights for the group",
			 EnableSession=true)]
		public virtual int GetUserGroupRights(string userid, string groupid)
		{
			try
			{
				Authorize();

						try
						{
							return iFolderUser.GetAdminRights(userid, groupid);
						}
						catch {} 
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return 0;
		}

		/// <summary>
		/// Returns a list of groups for which the user is an admin.
		/// </summary>
		/// <param name="userID">The user id of the administrator.</param>
		[WebMethod(
			 Description="Get Monitored groups by this user",
			 EnableSession=true)]
		public virtual string[] GetMonitoredGroups(string userID)
		{
			try
			{
				Authorize();

						try
						{
							return iFolderUser.GetMonitoredGroups(userID);
						}
						catch {} 
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return null;
		}

		/// <summary>
		/// Returns a list of groups' names for which the user is an admin.
		/// </summary>
		/// <param name="userID">The user id of the administrator.</param>
		[WebMethod(
			 Description="Get Monitored groups by this user",
			 EnableSession=true)]
		public virtual string[] GetMonitoredGroupNames(string userID)
		{
			try
			{
				Authorize();

						try
						{
							return iFolderUser.GetMonitoredGroupNames(userID);
						}
						catch {} 
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return null;
		}

		/// <summary>
		/// Removes a group admin
		/// </summary>
		/// <param name="groupid">The id of group for which the admin is being removed.</param>
		/// <param name="userID">The id of user.</param>
		[WebMethod(
			 Description="remove a user's group rights.",
			 EnableSession=true)]
		public virtual void RemoveGroupAdministrator(string groupid, string userID)
		{
			try
			{
				Authorize();

				iFolderUser.RemoveGroupAdministrator(groupid, userID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

		}

		/// <summary>
		/// Removes a group admin
		/// </summary>
		/// <param name="groupid">The id of group for which the admin is being removed.</param>
		/// <param name="userID">The id of user.</param>
		[WebMethod(
			 Description="Checks whether the disk quota policy for this user can be changed or not",
			 EnableSession=true)]
		public virtual bool DiskQuotaPolicyChangeAllowed(string userID, long limit)
		{
			bool result = true;

			try
			{
				Authorize();

				if( iFolderUser.DiskQuotaPolicyChangeAllowed(userID, limit) )
					return true;
				else
					throw new Exception("DiskQuotaPolicyError");
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
		
		/// <summary>
		/// SetAggregateDiskQuota for a group.
		/// </summary>
		/// <param name="groupid">The id of group.</param>
		/// <param name="value">The disk quota value</param>
		[WebMethod(
			 Description="Set agrgegate disk quota from this group",
			 EnableSession=true)]
		public virtual bool SetAggregateDiskQuota(string groupid, long value)
		{

			try
			{
				Authorize();

				return iFolderUser.SetAggregateDiskQuota(groupid, value);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return false;
		}
		/// <summary>
		/// Add a group administrator.
		/// </summary>
		/// <param name="groupid">The id of group.</param>
		/// <param name="userID">The id of user.</param>
		/// <param name="preference">The rights for user upon the group.</param>
		[WebMethod(
			 Description="Add this group administrator",
			 EnableSession=true)]
		public virtual void AddGroupAdministrator(string groupid, string userID, int preference)
		{
			Hashtable exceptions = new Hashtable();

			try
			{
				Authorize();

				string[] ids = userID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolderUser.AddGroupAdministrator(groupid, id, preference);
						}
						catch(Exception e)
						{
							exceptions.Add(id, e);
						}
					}
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			SmartException.Throw(exceptions);
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
		public virtual void RemoveAdministrator(string userID)
		{
			Hashtable exceptions = new Hashtable();

			try
			{
				Authorize();

				string[] ids = userID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolderUser.RemoveAdministrator(id);
						}
						catch(Exception e)
						{
							exceptions.Add(id, e);
						}
					}
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			SmartException.Throw(exceptions);
		}

		/// <summary>
		/// Get information about all the administrators.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <param name="admintype">the int value representing the type of admin, 0 for all admins, 1 for secondary admins, 2 for root admins</param>
		/// <remarks>A user is an administrator if the user has "Admin" rights in the domain.</remarks>
		/// <returns>An array of iFolderUser objects describing the administrators.</returns>
		[WebMethod(
			 Description="Get information about all the administrators.",
			 EnableSession=true)]
		public virtual iFolderUserSet GetAdministrators(int index, int max, int admintype )
		{
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetAdministrators(index, max, admintype);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
	
		#endregion

		#region Policy
		
		/// <summary>
		/// Get Encryption policy information for Group.
		/// </summary>
		/// <returns>A integer value of user's group objects Encryption policy object.</returns>
		[WebMethod(
			 Description="Get Encryption policy information for Users group object.",
			 EnableSession=true)]
		public virtual int GetGroupEncryptionPolicy(string userID)
		{
			int result = 0;

			try
			{
				Authorize();

				result = UserPolicy.GetUserGroupEncryptionPolicy(userID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get policy information for Group.
		/// </summary>
		/// <returns>A integer value of user's group objects sharing policy object.</returns>
		[WebMethod(
			 Description="Get policy information for Users group object.",
			 EnableSession=true)]
		public virtual int GetGroupSharingPolicy(string userID)
		{
			int result = 0;

			try
			{
				Authorize();

				result = UserPolicy.GetUserGroupSharingPolicy(userID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get policy information for the system.
		/// </summary>
		/// <returns>A SystemPolicy object.</returns>
		[WebMethod(
			 Description="Get policy information for the system.",
			 EnableSession=true)]
		public virtual SystemPolicy GetSystemPolicy()
		{
			SystemPolicy result = null;

			try
			{
				Authorize();

				result = SystemPolicy.GetPolicy();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Set policy for the iFolder system.
		/// </summary>
		/// <param name="policy">The SystemPolicy object.</param>
		[WebMethod(
			 Description="Set policy for the iFolder system.",
			 EnableSession=true)]
		public virtual void SetSystemPolicy(SystemPolicy policy)
		{
			try
			{
				Authorize();

				SystemPolicy.SetPolicy(policy);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
                /// Get policy information for a user.
                /// </summary>
                /// <param name="userID">A id of user.</param>
                /// <returns>A Group Policy status object.</returns>
                [WebMethod(
                         Description="Get policy information for a user.",
                         EnableSession=true)]
                public virtual int GetUserGroupSharingPolicy(string ownerID)
                {
                        int result = 0;

                        try
                        {
                                Authorize();

                                result = UserPolicy.GetUserGroupSharingPolicy(ownerID);
                        }
                        catch(Exception e)
                        {
                                SmartException.Throw(e);
                        }

                        return result;
                }


		/// <summary>
		/// Get policy information for a user.
		/// </summary>
		/// <param name="userID">A id of user.</param>
		/// <returns>A UserPolicy object.</returns>
		[WebMethod(
			 Description="Get policy information for a user.",
			 EnableSession=true)]
		public virtual UserPolicy GetUserPolicy(string userID, string AdminId)
		{
			UserPolicy result = null;

			try
			{
				Authorize();
				if(AdminId == null)
					result = UserPolicy.GetPolicy(userID);
				else
					result = UserPolicy.GetPolicy(userID, AdminId);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Set the policy for a user.
		/// </summary>
		/// <param name="policy">The UserPolicy object.</param>
		[WebMethod(
			 Description="Set the policy for a user.",
			 EnableSession=true)]
		public virtual void SetUserPolicy(UserPolicy policy)
		{
			try
			{
				Authorize();
				if( !IsAccessAllowed(policy.UserID))
				{
					throw new Exception("Group admin rights not available");
				}

				UserPolicy.SetPolicy(policy);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Get policy information for an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of an iFolder.</param>
		/// <returns>A iFolderPolicy object.</returns>
		[WebMethod(
			 Description="Get policy information for an iFolder.",
			 EnableSession=true)]
		public virtual iFolderPolicy GetiFolderPolicy(string ifolderID, string adminID)
		{
			iFolderPolicy result = null;

			try
			{
				Authorize();
				if(adminID == null)
					result = iFolderPolicy.GetPolicy(ifolderID, null);
				else
					result = iFolderPolicy.GetPolicy(ifolderID, null, adminID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Set the policy for an iFolder.
		/// </summary>
		/// <param name="policy">The iFolderPolicy object.</param>
		[WebMethod(
			 Description="Set the policy for an iFolder.",
			 EnableSession=true)]
		public virtual void SetiFolderPolicy(iFolderPolicy policy)
		{
			try
			{
				Authorize();
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(policy.iFolderID);
				if( !IsAccessAllowed(col.Owner.UserID))
				{
					throw new Exception("The member is not a group admin for ifolder");
				}

				iFolderPolicy.SetPolicy(policy, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}
		
		/// <summary>
		/// Get policy information for the registered identity provider.
		/// </summary>
		/// <returns>An IdentityPolicy object.</returns>
		[WebMethod(
			Description="Get policy information for the registered identity provider.",
			EnableSession = true)]
		public virtual IdentityPolicy GetIdentityPolicy()
		{
			IdentityPolicy result = null;

			try
			{
				Authorize();

				result = IdentityPolicy.GetPolicy();
			}
			catch( Exception e )
			{
				SmartException.Throw( e );
			}

			return result;
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
		public virtual void IdentitySyncDisableService(bool disable)
		{
			try
			{
				Authorize();

				Simias.IdentitySync.Service.SyncDisabled = disable;
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Get detailed information about the last synchronization cycle.
		/// </summary>
		[WebMethod(
			 Description= "Get detailed information about the last synchronization cycle.",
			 EnableSession = true)]
		public virtual LastSyncInfo IdentitySyncGetLastInfo()
		{
			LastSyncInfo info = null;

			try
			{
				Authorize();

				info = LastSyncInfo.GetLastSyncInfo();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return info;
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
		///
		[WebMethod(
			 Description= "Get the current status of the identity sync service thread.",
			 EnableSession = true)]
		public virtual SyncServiceInfo IdentitySyncGetServiceInfo()
		{
			SyncServiceInfo info = null;

			try
			{
				Authorize();

				info = SyncServiceInfo.GetSyncServiceInfo();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return info;
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
		public virtual void IdentitySyncSetDeleteMemberGracePeriod(int seconds)
		{
                	string DeleteMemberGracePeriodMethod = "SetDeleteMemberGracePeriod";
			IdentitySynchronizationIntervalSetMethod callout = null;
			try
			{
				Authorize();

				if(Simias.Service.Manager.LdapServiceEnabled == true)
				{	

                                	string assemblyName = Simias.Service.Manager.LdapAssemblyName;

                                	if ( assemblyName != null )
                                	{
                                        	Assembly idAssembly = Assembly.Load( assemblyName );
                                        	if ( idAssembly != null )
                                        	{
                                                	Type type = idAssembly.GetType( identitySyncClass );
                                                	if ( type != null )
                                                	{

                                                       	 	MethodInfo DeleteMemberGracePeriodNow=type.GetMethod(DeleteMemberGracePeriodMethod,														BindingFlags.Public | BindingFlags.Static );
								callout = (IdentitySynchronizationIntervalSetMethod)
										Delegate.CreateDelegate(																		typeof(IdentitySynchronizationIntervalSetMethod), 
											DeleteMemberGracePeriodNow);
								callout( seconds );
                                                	}
                                        	}
                                	}

				}
				else
					Simias.IdentitySync.Service.DeleteGracePeriod = seconds;
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}
	
		/// <summary>
		/// Method to set the synchronization interval for the
		/// sync engine.  Represented in seconds
		/// </summary>
		[WebMethod(
			 Description= "Set the synchronization interval for the identity sync service.",
			 EnableSession = true)]
		public virtual void IdentitySyncSetInterval(int seconds)
		{
                	string SyncIntervalMethod = "SetSyncInterval";
			IdentitySynchronizationIntervalSetMethod callout = null;
			try
			{
				Authorize();
				if(Simias.Service.Manager.LdapServiceEnabled == true)
				{	

                                	string assemblyName = Simias.Service.Manager.LdapAssemblyName;

                                	if ( assemblyName != null )
                                	{
                                        	Assembly idAssembly = Assembly.Load( assemblyName );
                                        	if ( idAssembly != null )
                                        	{
                                                	Type type = idAssembly.GetType( identitySyncClass );
                                                	if ( type != null )
                                                	{

                                                       	 	MethodInfo SyncIntervalNow=type.GetMethod(SyncIntervalMethod,										BindingFlags.Public | BindingFlags.Static );
								callout = (IdentitySynchronizationIntervalSetMethod)
										Delegate.CreateDelegate(													typeof(IdentitySynchronizationIntervalSetMethod), 
											SyncIntervalNow);
								callout( seconds );
                                                	}
                                        	}
                                	}

				}
				else
					Simias.IdentitySync.Service.SyncInterval = seconds;
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Tells the sync service to immediately start
		/// a synchronization cycle.
		/// </summary>
		///
		[WebMethod(
			 Description= "Cause the Identity Sync Service to synchronize immediately.",
			 EnableSession = true)]
		public virtual void IdentitySyncNow()
		{
                	string identitySyncMethod = "SyncNow";
			IdentitySynchronizationMethod callout = null;
			try
			{
				Authorize();
				
				//In case of Ldap plugin, The request must reach the plugin service. 
				if(Simias.Service.Manager.LdapServiceEnabled == true)
				{	

                                	string assemblyName = Simias.Service.Manager.LdapAssemblyName;

                                	if ( assemblyName != null )
                                	{
                                        	Assembly idAssembly = Assembly.Load( assemblyName );
                                        	if ( idAssembly != null )
                                        	{
                                                	Type type = idAssembly.GetType( identitySyncClass );
                                                	if ( type != null )
                                                	{

                                                       	 	MethodInfo identitySyncNow=type.GetMethod(identitySyncMethod,										BindingFlags.Public | BindingFlags.Static );
								callout = (IdentitySynchronizationMethod)
										Delegate.CreateDelegate(													typeof(IdentitySynchronizationMethod), 
											identitySyncNow);
								callout( "" );
                                                	}
                                        	}
                                	}

				}
				else
					Simias.IdentitySync.Service.SyncNow("");
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}
		
		/// <summary>
		/// Get LogLevel Information
		/// </summary>
		///
		[WebMethod(
			 Description= "Fetches the logleve information.",
			 EnableSession = true)]
		public virtual string[] GetLogLevels()
		{
		        string[] result = null;

			try
			{
				Authorize();

				result = iFolderServer.GetLogLevels();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
                /// Add a data  path for an iFolder Server.
                /// </summary>
                /// <param name="DataPathname">The name of the data store.</param>
                /// <param name="FullPath">The Full Path of the data store.</param>
                /// <param name="ServerID">Server ID of the server</param>
                /// <returns>Integer : 0 on success.</returns>
                [WebMethod(
                         Description="Add a data store for an iFolder Server.",
                         EnableSession=true)]
                public virtual int AddDataStore(string datapathname,string fullpath,string ServerID)
                {
			bool enabled = true;
                        DataStore datastore = new DataStore(datapathname, fullpath, enabled);
			return datastore.AddStore(ServerID);	
                }


		/// <summary>
                /// Modify data store for an iFolder Server.
                /// </summary>
                /// <param name="name">The name of the data store.</param>
                /// <returns>Bool true on success.</returns>
                [WebMethod(
                         Description="Modify a data store for an iFolder Server.",
                         EnableSession=true)]
                public virtual bool ModifyStore(string datapathname, bool enabled)
                {
                        DataStore datastore = new DataStore();
                        return datastore.ModifyStore(datapathname,enabled);
                }

		/// <summary>
                /// Delete data store for an iFolder Server.
                /// </summary>
                /// <param name="name">The name of the data store.</param>
                /// <returns>Bool true on success.</returns>
                [WebMethod(
                         Description="Delete a data store for an iFolder Server.",
                         EnableSession=true)]
                public virtual bool DeleteDataPath(string datapathname)
                {
                        DataStore datastore = new DataStore();
                        return datastore.DeleteStore(datapathname);
                }

		/// <summary>
                /// Gets an array of datastore of an iFolder Server.
                /// </summary>
                /// <returns>Bool true on success.</returns>
                [WebMethod(
                         Description="Gets an array data store for an iFolder Server.",
                         EnableSession=true)]
                public virtual VolumesList GetVolumes(int index, int max)
                {
			VolumesList result = null;

                        try
                        {
                                Authorize();

                                result = Volumes.GetVolumes(index,max);
                        }
                        catch(Exception e)
                        {
                                SmartException.Throw(e);
                        }

                        return result;
                }

		/// <summary>
		/// Set LogLevel Information
		/// </summary>
		///
		[WebMethod(
			 Description= "Fetches the logleve information.",
			 EnableSession = true)]
		public virtual void SetLogLevel(iFolderServer.LoggerType type, string logLevel)
		{
			try
			{
				Authorize();

				iFolderServer.SetLogLevel(type, logLevel);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

        /// <summary>
        /// Get log level information
        /// </summary>
        /// <returns>ldap info object containing all informations, null if unsuccessful</returns>
		[WebMethod(
			 Description= "Fetches the logleve information.",
			 EnableSession = true)]
		public virtual LdapInfo GetLdapDetails()
		{
		    LdapInfo result = null;

		    try 
		    {
		        result = iFolderServer.GetLdapDetails ();
		    } 
		    catch (Exception e)
		    {
			SmartException.Throw (e);
		    }

		    return result;
		}

                /// <summary>
                /// Get the new home server url for the user
                /// </summary>
                /// <returns>Publiv url of the new home server where user is getting moved.</returns>
                [WebMethod(
                         Description="Get the new home server url for the user.",
                         EnableSession=true)]
                public virtual string GetNewHomeServerURLForUserID( string userid )
                {
                        string result=null;

			if( IsAccessAllowed(userid) )
	                        result = iFolderServer.GetNewHomeServerURLForUserID( userid );

                        return result;
                }

		/// <summary>
		/// Getsimiasrequiressl info  
		/// </summary>
		/// <returns>string representing SimiasRequiresSSL value is set into config file</returns>
		[WebMethod(
			 Description= "returns simiasrequiressl status.",
			 EnableSession = true)]
		public virtual string GetSimiasRequiresSSLStatus()
		{
		    string result = null;

		    try 
		    {
		        result = iFolderServer.GetSimiasRequiresSSLStatus ();
		    } 
		    catch (Exception e)
		    {
			SmartException.Throw (e);
		    }

		    return result;
		}
		/// <summary>
		/// Get LogLevel Information
		/// </summary>
		[WebMethod(
			 Description= "sets the ldap details to config and ldap store",
			 EnableSession = true)]
		public virtual void SetLdapDetails(LdapInfo ldapInfo, string LdapAdminDN, string LdapAdminPwd, string ServerID)
		{

		    try 
		    {
		        iFolderServer.SetLdapDetails (ldapInfo, LdapAdminDN, LdapAdminPwd, ServerID);
		    } 
		    catch (Exception e)
		    {
			SmartException.Throw (e);
		    }
		}
		
		/// <summary>
               ///sets new IP
               /// </summary>
               /// <returns>true if successful</returns>
               [WebMethod(
                        Description= "set the new public and private IP ",
                        EnableSession = true)]
               public virtual bool SetIPDetails(string privateIP , string publicIP, string MastersIP)
               {
                   bool result = false;

                   try
                   {
                       result = iFolderServer.SetIPDetails (privateIP, publicIP, MastersIP);
                   }
                   catch (Exception e)
                   {
                       SmartException.Throw (e);
                   }
                   return result;
               }

			   /// <summary>
			   /// Get the Master Server
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description="gets the Master server", EnableSession=true)]
	        	public virtual iFolderServer GetMasterServer ()
				   {
					   return iFolderServer.GetMasterServer();
				   }


			   /// <summary>
			   /// set the Master Url on this node.
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description= "set the Master Url", EnableSession = true)]
				   public virtual bool SetMasterServerUrl (string HostID, string MasterUrl)
				   {
					   bool result = false;
					   try
					   {
						   result = iFolderServer.SetMasterServerUrl (HostID, MasterUrl);
					   }
					   catch (Exception e)
					   {
						   SmartException.Throw (e);
					   }
					   return result;
				   }

			   /// <summary>
			   /// Sets this server as Master Server
			   /// </summary>
			   /// <param name="HostID"> ID(Ace value) of the server</param>
			   /// <returns>true on success/false on failure</returns>
			   [WebMethod(Description= "set the new master server", EnableSession = true)]
				   public virtual bool SetAsMasterServer(string hostID)
				   {
					   return iFolderServer.SetAsMasterServer( hostID );
				   }

			   /// <summary>
			   /// Sets this server as Slave Server
			   /// </summary>
			   /// <param name="HostID"> ID(Ace value) of the new server</param>
			   /// <param name="newMasterPublicUrl"> public url of the new master server</param>
			   /// <returns>true on success/false on failure</returns>
			   [WebMethod(Description= "set the HostID on Domain", EnableSession = true)]
				   public virtual bool SetAsSlaveServer(string newMasterHostID, string newMasterPublicUrl)
				   {
					   return iFolderServer.SetAsSlaveServer(newMasterHostID, newMasterPublicUrl);
				   }

			   /// <summary>
			   /// set the MasterNodeAttribute
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description= "set the Master node attribute", EnableSession = true)]
				   public virtual bool SetMasterNodeAttribute (string HostID, bool Value)
				   {
					   bool result = false;

					   try
					   {
						   result = iFolderServer.SetMasterNodeAttribute (HostID, Value);
					   }
					   catch (Exception e)
					   {
						   SmartException.Throw (e);
					   }
					   return result;
				   }

			   /// <summary>
			   /// get the MasterNodeAttribute
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description= "get the Master node attribute", EnableSession = true)]
				   public virtual bool GetMasterNodeAttribute (string HostID)
				   {
					   bool result = false;

					   try
					   {
						   result = iFolderServer.GetMasterNodeAttribute (HostID);
					   }
					   catch (Exception e)
					   {
						   SmartException.Throw (e);
					   }
					   return result;
				   }

			   /// <summary>
			   /// get the server attributes 
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description= "get the  node attributes", EnableSession = true)]
				   public virtual bool VerifyChangeMaster(string cmHostID, string nmHostID)
				   {
					   bool result = false;

					   try
					   {
						   result = iFolderServer.VerifyChangeMaster(cmHostID, nmHostID);
					   }
					   catch (Exception e)
					   {
						   SmartException.Throw (e);
					   }
					   return result;
				   }
			   /// <summary>
			   /// get the server attributes 
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description= "Check if server requires repair", EnableSession = true)]
				   public virtual bool ServerNeedsRepair()
				   {
					   bool result = false;
					   try
					   {
						   result = iFolderServer.ServerNeedsRepair();
					   }
					   catch (Exception e)
					   {
						   SmartException.Throw (e);
					   }
					   return result;
				   }


			   /// <summary>
			   /// Rectifies the error/failure that may have caused by ChangeMaster 
			   /// </summary>
			   /// <returns>true/false</returns>
			   [WebMethod(Description= "Rectify the errors/failures caused by ChangeMaster", EnableSession = true)]
				   public virtual bool RepairChangeMasterUpdates()
				   {
					   bool result = false;
					   try
					   {
						   result = iFolderServer.RepairChangeMasterUpdates();
					   }
					   catch (Exception e)
					   {
						   SmartException.Throw (e);
					   }
					   return result;
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
			if( GetAccessID() == null)
				base.DisableSystemPastSharing ();
			else
				base.DisableGroupPastSharing();
		}

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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if( !IsAccessAllowed(col.Owner.UserID) )
				{
					/// throw access violation exception...
					return;
				}
			}
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if(col.GetMemberByID(accessID) == null)
				{
					if( !IsAccessAllowed(col.Owner.UserID) )
					{
						/// throw access violation exception...
						return null;
					}
				}
			}		
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if( !IsAccessAllowed(col.Owner.UserID) )
				{
					/// throw access violation exception...
					return null;
				}
			}
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if( !IsAccessAllowed(col.Owner.UserID) )
				{
					/// throw access violation exception...
					return ;
				}
			}
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if( !IsAccessAllowed(col.Owner.UserID) )
				{
					/// throw access violation exception...
					return ;
				}
			}
			base.PublishiFolder(ifolderID, publish);
		}

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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if( !IsAccessAllowed(col.Owner.UserID) )
				{
					/// throw access violation exception...
					return null;
				}
			}
			return base.GetChanges(ifolderID, itemID, index, max);
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
			string accessID = GetAccessIDForGroup();
			if( accessID != null)
			{
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderID);
				if( !IsAccessAllowed(col.Owner.UserID) )
				{
					/// throw access violation exception...
					return ;
				}
			}
			base.SetMemberRights(ifolderID, userID, rights);
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
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUsers(property, operation, pattern, index, max, GetAccessIDForGroup(), true);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		#endregion

		#region Utility

		/// <summary>
		/// Get the access user's id.
		/// </summary>
		protected override string GetAccessID()
		{
			// no access control as Admin
			return null;
		}

		protected override string GetAccessIDForGroup()
		{
			string userID = GetUserID();
			if( iFolderUser.IsAdministrator(userID) )
				return null;
			else
				return userID;
		}

		/// <summary>
		/// Get the authenticated user's id.
		/// </summary>
		protected override string GetUserID()
		{
			// check authentication
			string userID = Context.User.Identity.Name;

			if ((userID == null) || (userID.Length == 0))
			{
				throw new AuthenticationException();
			}

			return userID;
		}

		/// <summary>
		/// Authorize the authenticated user.
		/// </summary>
		protected override void Authorize()
		{
			// check authentication
			string userID = GetUserID();

			// check for an admin ID cache
			string adminID = (string)Session["AdminID"];

			// check the ID cache
			if ((adminID == null) || (adminID.Length == 0) || (!adminID.Equals(userID)))
			{
				if (iFolderUser.IsAdministrator(userID))
				{
					// authorized
					Session["AdminID"] = userID;
				}
				else if(iFolderUser.IsGroupAdministrator(userID))
				{
					Session["AdminID"] = userID;
				}
				else
				{
					// unauthroized
					throw new AuthorizationException(userID);
				}
			}
		}

		protected override bool IsAccessAllowed(string nodeid)
		{
			try
			{
				bool status = false;
				string userID = GetUserID();
				/// If administrator return true...
				if( iFolderUser.IsAdministrator(userID))
				{
					return true;
				}
				/// else check whether the nodeid is administered by ythe group admin.
				if( !iFolderUser.IsGroupAdministrator(userID))
				{
					return false;
				}
				else
				{
					/// if nodeid is a group, check if this group is present in the group list of the GroupAdmin...
					Store store = Store.GetStore();
					Domain domain = store.GetDomain(store.DefaultDomain);
					Member GroupAdminMember = domain.GetMemberByID(userID);
					if( GroupAdminMember == null)
					{
						return false;
					}
					Hashtable ht = GroupAdminMember.GetMonitoredUsers(true);
					status = ht.ContainsKey(nodeid);
				}
				return status;
			}
			catch
			{
				return false;
			}

			/// Check if this user has the rights mentioned for the corresponding member...
		}

		#endregion
	}
}
