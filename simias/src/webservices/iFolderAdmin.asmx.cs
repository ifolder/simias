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
	public class iFolderAdmin : System.Web.Services.WebService
	{
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderAdmin()
		{
		}

		#endregion

		#region System

		/// <summary>
		/// Get information about the current iFolder server.
		/// </summary>
		/// <returns>An iFolderServer object describing the current server.</returns>
		[WebMethod(
			 Description="Get information about the current iFolder server.",
			 EnableSession=true)]
		public iFolderServer GetServer()
		{
			iFolderServer result = null;

			try
			{
				Authorize();

				result = iFolderServer.GetServer();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about the iFolder system.
		/// </summary>
		/// <returns>An iFolderSystem object describing the system.</returns>
		[WebMethod(
			 Description="Get information about the iFolder system.",
			 EnableSession=true)]
		public iFolderSystem GetSystem()
		{
			iFolderSystem result = null;

			try
			{
				Authorize();

				result = iFolderSystem.GetSystem();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		/// Delete an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of iFolder to delete.</param>
		/// <remarks>This API will accept multiple iFolder ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Delete an iFolder.",
			 EnableSession=true)]
		public void DeleteiFolder(string ifolderID)
		{
			Hashtable exceptions = new Hashtable();

			try
			{
				Authorize();

				string[] ids = ifolderID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolder.DeleteiFolder(id, null);
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
		/// Get information about an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <returns>An iFolder object.</returns>
		[WebMethod(
			 Description="Get information about an iFolder.",
			 EnableSession=true)]
		public iFolder GetiFolder(string ifolderID)
		{
			iFolder result = null;

			try
			{
				Authorize();

				result = iFolder.GetiFolder(ifolderID, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
		
		/// <summary>
		/// Get detailed information about an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <returns>An iFolderDetails object.</returns>
		/// <remarks>It is more expensive to call GetiFolderDetails than GetiFolder.</remarks>
		[WebMethod(
			 Description="Get detailed information about an iFolder.",
			 EnableSession=true)]
		public iFolderDetails GetiFolderDetails(string ifolderID)
		{
			iFolderDetails result = null;

			try
			{
				Authorize();

				result = iFolderDetails.GetiFolderDetails(ifolderID, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
		
		/// <summary>
		/// Set the description of an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="description">The new description for the iFolder.</param>
		[WebMethod(
			 Description="Set the description of an iFolder.",
			 EnableSession=true)]
		public void SetiFolderDescription(string ifolderID, string description)
		{
			try
			{
				Authorize();

				iFolder.SetDescription(ifolderID, description, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Get inforamtion about all iFolders.
		/// </summary>
		/// <param name="type">An iFolder type filter of the results.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolder objects.</returns>
		[WebMethod(
			 Description="Get iFolders",
			 EnableSession=true)]
		public iFolder[] GetiFolders(iFolderType type, int index, int count, out int total)
		{
			iFolder[] result = null;
			total = 0;

			try
			{
				Authorize();

				result = iFolder.GetiFoldersByName(type, SearchOperation.BeginsWith, "", index, count, out total, null);
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
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about all iFolders identified by a member.",
			 EnableSession=true)]
		public iFolder[] GetiFoldersByMember(string userID, MemberRole role, int index, int count, out int total)
		{
			iFolder[] result = null;
			total = 0;

			try
			{
				Authorize();

				result = iFolder.GetiFoldersByMember(userID, role, index, count, out total, null);
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
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about all iFolders identified by a search on the it's name.",
			 EnableSession=true)]
		public iFolder[] GetiFoldersByName(SearchOperation operation, string pattern, int index, int count, out int total)
		{
			iFolder[] result = null;
			total = 0;

			try
			{
				Authorize();

				result = iFolder.GetiFoldersByName(iFolderType.All, operation, pattern, index, count, out total, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		#endregion

		#region Changes
		
		/// <summary>
		/// Get a history of changes to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="entryID">The id of entry to filter the results (can be null for no filtering).</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of ChangeEntry objects.</returns>
		[WebMethod(
			 Description="Get a history of changes to an iFolder.",
			 EnableSession=true)]
		public ChangeEntry[] GetChanges(string ifolderID, string entryID, int index, int count, out int total)
		{
			ChangeEntry[] result = null;
			total = 0;

			try
			{
				Authorize();

				result = ChangeEntry.GetChanges(ifolderID, entryID, index, count, out total, null);
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

		#endregion

		#region User

		/// <summary>
		/// Get information about the authenticated user.
		/// </summary>
		/// <returns>An iFolderUser object describing the authenticated user.</returns>
		[WebMethod(
			 Description="Get information about the authenticated user.",
			 EnableSession=true)]
		public iFolderUser GetAuthenticatedUser()
		{
			iFolderUser result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUser(null, GetAccessID(), null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public void SetMemberRights(string ifolderID, string userID, Simias.Storage.Access.Rights rights)
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
							iFolderUser.SetMemberRights(ifolderID, id, rights, null);
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
		/// Add a member to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of iFolder.</param>
		/// <param name="userID">The user id of the new member.</param>
		/// <param name="rights">The rights of the new member on the iFolder.</param>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Add a member to an iFolder.",
			 EnableSession=true)]
		public void AddMember(string ifolderID, string userID, Simias.Storage.Access.Rights rights)
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
							iFolderUser.AddMember(ifolderID, id, rights, null);
						}
						catch(Exception e)
						{
							// save any exceptions
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
		/// Remove a member from an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="userID">The user id of the member.</param>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Remove a member from an iFolder.",
			 EnableSession=true)]
		public void RemoveMember(string ifolderID, string userID)
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
							iFolderUser.RemoveMember(ifolderID, id, null);
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
		/// Set the owner of an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="userID">The user id of the new owner.</param>
		[WebMethod(
			 Description="Set the owner of an iFolder.",
			 EnableSession=true)]
		public void SetiFolderOwner(string ifolderID, string userID)
		{
			try
			{
				Authorize();

				iFolderUser.SetOwner(ifolderID, userID, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Get information about the members of an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolderUser objects describing the members.</returns>
		[WebMethod(
			 Description="Get information about the members of an iFolder.",
			 EnableSession=true)]
		public iFolderUser[] GetMembers(string ifolderID, int index, int count, out int total)
		{
			iFolderUser[] result = null;
			total = 0;

			try
			{
				Authorize();

				result = iFolderUser.GetUsers(ifolderID, index, count, out total, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about all of the iFolder users.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users.",
			 EnableSession=true)]
		public iFolderUser[] GetUsers(int index, int count, out int total)
		{
			iFolderUser[] result = null;
			total = 0;

			try
			{
				Authorize();

				result = iFolderUser.GetUsers(null, index, count, out total, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about a user.
		/// </summary>
		/// <param name="userID">The id of the user.</param>
		/// <returns>A iFolderUser object.</returns>
		[WebMethod(
			 Description="Get information about a user.",
			 EnableSession=true)]
		public iFolderUser GetUser(string userID)
		{
			iFolderUser result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUser(null, userID, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public iFolderUserDetails GetUserDetails(string userID)
		{
			iFolderUserDetails result = null;

			try
			{
				Authorize();

				result = iFolderUserDetails.GetDetails(userID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about all of the iFolder users identified by the search property, operation, and pattern.
		/// </summary>
		/// <param name="property">The property to search.</param>
		/// <param name="operation">The operation to compare the property and pattern.</param>
		/// <param name="pattern">The pattern to search</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users identified by the search property, operation, and pattern.",
			 EnableSession=true)]
		public iFolderUser[] GetUsersBySearch(SearchProperty property, SearchOperation operation, string pattern, int index, int count, out int total)
		{
			iFolderUser[] result = null;
			total = 0;

			try
			{
				Authorize();

				result = iFolderUser.GetUsers(property, operation, pattern, index, count, out total, null);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <remarks>A user is an administrator if the user has "Admin" rights in the domain.</remarks>
		/// <returns>An array of iFolderUser objects describing the administrators.</returns>
		[WebMethod(
			 Description="Get information about all the administrators.",
			 EnableSession=true)]
		public iFolderUser[] GetAdministrators(int index, int count, out int total)
		{
			iFolderUser[] result = null;
			total = 0;

			try
			{
				Authorize();

				result = iFolderUser.GetAdministrators(index, count, out total);
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
		
		/// <summary>
		/// Create a new user in the iFolder system.
		/// </summary>
		/// <param name="username">A short unique name for the user (mandatory).</param>
		/// <param name="password">The password for the user (mandatory).</param>
		/// <param name="userGuid">A GUID for the user (optional).</param>
		/// <param name="firstName">The first/given name of the user (optional).</param>
		/// <param name="lastName">The last/family name of the user (optional).</param>
		/// <param name="fullName">The full name of the user (optional).</param>
		/// <param name="dn">The distinguished name, from an external identity store, for the user (optional).</param>
		/// <param name="email">The primary email address of the user (optional).</param>
		/// <returns>An iFolderUser object describing the new user.</returns>
		/// <remarks>
		/// Some identity providers DO NOT allow the creation,
		/// modification or deletion of new users.
		/// </remakrs>
		/// <remarks>
		/// If the firstName and lastName are specified but the fullName is null, the fullName is
		/// created with the firstName and lastName.
		/// </remarks>
		[WebMethod(
			Description= "Method to create a new user in the Simias domain",
			EnableSession = true)]
		public RegistrationInfo CreateUser(
			string 	Username,
			string 	Password,
			string 	UserGuid,
			string 	FirstName,
			string 	LastName,
			string 	FullName,
			string	DistinguishedName,
			string	Email)
		{
			RegistrationInfo info = null;

			try
			{
				Authorize();

				// check if the registered provider allows user creation
				IUserProvider provider = Simias.Server.User.GetRegisteredProvider();
				UserProviderCaps caps = provider.GetCapabilities();
				
				if (caps.CanCreate == false)
				{
					info = new RegistrationInfo(RegistrationStatus.MethodNotSupported);
					info.Message = "Identity provider does not allow creation";
				}
				else if (Username == null || Username == "" || Password == null)
				{
					info = new RegistrationInfo(RegistrationStatus.InvalidParameters);
					info.Message = "Missing mandatory parameters";
				}
				else
				{
					Simias.Server.User user = new Simias.Server.User(Username);
					user.FirstName = FirstName;
					user.LastName = LastName;
					user.UserGuid = UserGuid;
					user.FullName = FullName;
					user.DN = DistinguishedName;
					user.Email = Email;
				
					info = user.Create(Password);
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			
			return info;
		}
		
		/// <summary>
		/// Delete a user from the iFolder system.
		/// </summary>
		/// <param name="username">The username of the user to be deleted.</param>
		/// <remarks>
		/// Some identity providers DO NOT allow the creation,
		/// modification or deletion of new users.
		/// </remarks>
		[WebMethod(
			Description= "Delete a user from the iFolder system.",
			EnableSession = true)]
		public bool DeleteUser(string Username)
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
					if ((Username != null) && (Username != ""))
					{
						Simias.Server.User user = new Simias.Server.User(Username);
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
		/// Set a user's password.
		/// </summary>
		/// <param name="username">The username of the user.</param>
		/// <param name="password">The new password for the user.</param>
		/// <remarks>
		/// Some identity providers DO NOT allow the creation,
		/// modification or deletion of new users.
		/// </remarks>
		[WebMethod(
			Description= "Set a user's password.",
			EnableSession = true)]
		public bool SetPassword(string Username, string Password)
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
					if ((Username != null) && (Username != "") && (Password != null))
					{
						status = Simias.Server.User.SetPassword(Username, Password);
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
		/// Get the authenticated user's id.
		/// </summary>
		private string GetAccessID()
		{
			// check authentication
			string accessID = Context.User.Identity.Name;

			if ((accessID == null) || (accessID.Length == 0))
			{
				throw new AuthenticationException();
			}

			return accessID;
		}

		/// <summary>
		/// Authorize the authenticated user.
		/// </summary>
		private void Authorize()
		{
			// check authentication
			string accessID = GetAccessID();

			// check for an admin ID cache
			string adminID = (string)Session["AdminID"];

			// check the ID cache
			if ((adminID == null) || (adminID.Length == 0) || (!adminID.Equals(accessID)))
			{
				if (iFolderUser.IsAdministrator(accessID))
				{
					// authorized
					Session["AdminID"] = accessID;
				}
				else
				{
					// unauthroized
					throw new AuthorizationException(accessID);
				}
			}
		}

		#endregion
	}
}
