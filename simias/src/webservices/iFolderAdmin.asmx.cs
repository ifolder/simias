/***********************************************************************
 *  $RCSfile: iFolderAdmin.asmx.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections;

using Simias.Storage;
using Simias.Server;

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder Admin Web Service
	/// </summary>
	[WebService(
		 Namespace="http://novell.com/ifolder/webservice/",
		 Name="iFolderAdmin",
		 Description="iFolder Admin Web Service")]
	public class iFolderAdmin : iFolderCommon
	{
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderAdmin()
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
		public iFolder CreateiFolder(string name, string userID, string description)
		{
			iFolder result = null;

			try
			{
				Authorize();

				result = iFolder.CreateiFolder(name, userID, description, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public iFolderSet GetiFolders(iFolderType type, int index, int max)
		{
			iFolderSet result = null;

			try
			{
				Authorize();

				result = iFolder.GetiFoldersByName(type, SearchOperation.BeginsWith, "", index, max, null);
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
		public iFolderSet GetiFoldersByMember(string userID, MemberRole role, int index, int max)
		{
			iFolderSet result = null;

			try
			{
				Authorize();

				result = iFolder.GetiFoldersByMember(userID, role, index, max, null);
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
		public iFolderSet GetiFoldersByName(SearchOperation operation, string pattern, int index, int count)
		{
			iFolderSet result = null;

			try
			{
				Authorize();

				result = iFolder.GetiFoldersByName(iFolderType.All, operation, pattern, index, count, GetAccessID());
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
		public string GetSystemSetting(string name)
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
		public void SetSystemSetting(string name, string value)
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
		public string GetUserSetting(string userID, string name)
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
		/// Set a setting specific to a user.
		/// </summary>
		/// <param name="userID">The id of the user.</param>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		[WebMethod(
			 Description="Set a setting specific to a user.",
			 EnableSession=true)]
		public void SetUserSetting(string userID, string name, string value)
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
		/// <param name="iFolderID">The id of the iFolder.</param>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting.</returns>
		[WebMethod(
			 Description="Get a setting specific to an iFolder.",
			 EnableSession=true)]
		public string GetiFolderSetting(string iFolderID, string name)
		{
			string result = null;

			try
			{
				Authorize();

				result = Settings.GetCollectionSetting(iFolderID, name);
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
		/// <param name="userID">The id of the iFolder.</param>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		[WebMethod(
			 Description="Set a setting specific to an iFolder.",
			 EnableSession=true)]
		public void SetiFolderSetting(string iFolderID, string name, string value)
		{
			try
			{
				Authorize();

				Settings.SetCollectionSetting(iFolderID, name, value);
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
		public iFolderUser CreateUser(
			string 	username,
			string 	password,
			string 	guid,
			string 	firstName,
			string 	lastName,
			string 	fullName,
			string	dn,
			string	email)
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
		public bool DeleteUser(string userID)
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
		public iFolderUser SetUser(string userID, iFolderUser user)
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
		public bool SetPassword(string userID, string password)
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

		#endregion

		#region System

		/// <summary>
		/// Update the editable information for the iFolder system.
		/// </summary>
		/// <param name="system">The update iFolderSystem object (Name, Description).</param>
		[WebMethod(
			 Description= "Update the editable information for the iFolder system (Name, Description).",
			 EnableSession = true)]
		public void SetSystem(iFolderSystem system)
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
		public void AddAdministrator(string userID)
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
		/// Revoke system administration rights from a user.
		/// </summary>
		/// <param name="userID">The user id of the administrator.</param>
		/// <remarks>Administration rights are removed by the user having "ReadOnly" rights in the domain.</remarks>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Revoke system administration rights from a user.",
			 EnableSession=true)]
		public void RemoveAdministrator(string userID)
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
		/// <remarks>A user is an administrator if the user has "Admin" rights in the domain.</remarks>
		/// <returns>An array of iFolderUser objects describing the administrators.</returns>
		[WebMethod(
			 Description="Get information about all the administrators.",
			 EnableSession=true)]
		public iFolderUserSet GetAdministrators(int index, int max)
		{
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetAdministrators(index, max);
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
		/// Get policy information for the system.
		/// </summary>
		/// <returns>A SystemPolicy object.</returns>
		[WebMethod(
			 Description="Get policy information for the system.",
			 EnableSession=true)]
		public SystemPolicy GetSystemPolicy()
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
		/// <param name="props">The SystemPolicy object.</param>
		[WebMethod(
			 Description="Set policy for the iFolder system.",
			 EnableSession=true)]
		public void SetSystemPolicy(SystemPolicy props)
		{
			try
			{
				Authorize();

				SystemPolicy.SetPolicy(props);
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
		/// <returns>A UserPolicy object.</returns>
		[WebMethod(
			 Description="Get policy information for a user.",
			 EnableSession=true)]
		public UserPolicy GetUserPolicy(string userID)
		{
			UserPolicy result = null;

			try
			{
				Authorize();

				result = UserPolicy.GetPolicy(userID);
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
		/// <param name="props">The UserPolicy object.</param>
		[WebMethod(
			 Description="Set the policy for a user.",
			 EnableSession=true)]
		public void SetUserPolicy(UserPolicy props)
		{
			try
			{
				Authorize();

				UserPolicy.SetPolicy(props);
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
		public iFolderPolicy GetiFolderPolicy(string ifolderID)
		{
			iFolderPolicy result = null;

			try
			{
				Authorize();

				result = iFolderPolicy.GetPolicy(ifolderID, null);
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
		/// <param name="props">The iFolderPolicy object.</param>
		[WebMethod(
			 Description="Set the policy for an iFolder.",
			 EnableSession=true)]
		public void SetiFolderPolicy(iFolderPolicy props)
		{
			try
			{
				Authorize();

				iFolderPolicy.SetPolicy(props, null);
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
		public IdentityPolicy GetIdentityPolicy()
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
		/// Method to disable the synchronization service
		/// true - disables
		/// false - enables the synchronization service
		/// Note! once enabled the service will enter a
		/// synchronization cycle ignoring the configured
		/// sync interval time.
		/// </summary>
		///
		[WebMethod(
			 Description= "Disables/enables the identity synchronization service",
			 EnableSession = true)]
		public
		void
		IdentitySyncDisableService( bool Disable )
		{
			try
			{
				Authorize();

				Simias.IdentitySync.Service.SyncDisabled = Disable;
			}
			catch ( Exception e )
			{
				SmartException.Throw( e );
			}
		}

		/// <summary>
		/// Get detailed information about the last synchronization cycle.
		/// </summary>
		[WebMethod(
			 Description= "Get detailed information about the last synchronization cycle",
			 EnableSession = true)]
		public
		LastSyncInfo
		IdentitySyncGetLastInfo()
		{
			LastSyncInfo info = null;

			try
			{
				Authorize();

				info = LastSyncInfo.GetLastSyncInfo();
			}
			catch ( Exception e )
			{
				SmartException.Throw( e );
			}

			return info;
		}

		/// <summary>
		/// Get the current status of the identity sync service thread
		/// status could be:
		/// Disabled
		/// Working
		/// Waiting
		/// Authentication Failure
		/// etc..
		/// </summary>
		///
		[WebMethod(
			 Description= "Get the current status of the identity sync service thread",
			 EnableSession = true)]
		public
		SyncServiceInfo
		IdentitySyncGetServiceInfo()
		{
			SyncServiceInfo info = null;

			try
			{
				Authorize();

				info = SyncServiceInfo.GetSyncServiceInfo();
			}
			catch ( Exception e )
			{
				SmartException.Throw( e );
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
			 Description= "Set the grace period for a member",
			 EnableSession = true)]
		public
		void
		IdentitySyncSetDeleteMemberGracePeriod( int Seconds )
		{
			try
			{
				Authorize();

				Simias.IdentitySync.Service.DeleteGracePeriod = Seconds;
			}
			catch ( Exception e )
			{
				SmartException.Throw( e );
			}
		}
	
		/// <summary>
		/// Method to set the synchronization interval for the
		/// sync engine.  Represented in seconds
		/// </summary>
		[WebMethod(
			 Description= "Set the synchronization interval for the identity sync service",
			 EnableSession = true)]
		public
		void
		IdentitySyncSetInterval( int Seconds )
		{
			try
			{
				Authorize();

				Simias.IdentitySync.Service.SyncInterval = Seconds;
			}
			catch ( Exception e )
			{
				SmartException.Throw( e );
			}
		}

		/// <summary>
		/// Tells the sync service to immediately start
		/// a synchronization cycle.
		/// </summary>
		///
		[WebMethod(
			 Description= "Cause the Identity Sync Service to synchronize immediately",
			 EnableSession = true)]
		public
		void
		IdentitySyncNow()
		{
			try
			{
				Authorize();

				Simias.IdentitySync.Service.SyncNow( "" );
			}
			catch ( Exception e )
			{
				SmartException.Throw( e );
			}
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
				else
				{
					// unauthroized
					throw new AuthorizationException(userID);
				}
			}
		}

		#endregion
	}
}
