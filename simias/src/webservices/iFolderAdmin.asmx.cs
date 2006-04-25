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
		/// Get iFolder Server Information
		/// </summary>
		/// <returns>An iFolderServer Object</returns>
		[WebMethod(
			 Description="Get iFolder Server Information",
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
		/// Get iFolder System Information
		/// </summary>
		/// <returns>An iFolderSystem Object</returns>
		[WebMethod(
			 Description="Get iFolder System Information",
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
		/// Create a New iFolder
		/// </summary>
		/// <param name="name">The New iFolder Name</param>
		/// <param name="userID">The New iFolder Owner's User ID</param>
		/// <param name="description">The New iFolder Description</param>
		/// <returns>An iFolder Object</returns>
		[WebMethod(
			 Description="Create a New iFolder",
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
		/// Delete an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <remarks>This API will accept multiple iFolder IDs in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Delete an iFolder",
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
		/// Get an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <returns>An iFolder Object</returns>
		[WebMethod(
			 Description="Get an iFolder",
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
		/// Get iFolder Details
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <returns>An iFolderDetails Object</returns>
		[WebMethod(
			 Description="Get iFolder Details",
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
		/// Set the Description of an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="description">The New iFolder's Description</param>
		[WebMethod(
			 Description="Set the Description of an iFolder",
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
		/// Get iFolders
		/// </summary>
		/// <param name="type">The iFolder Type</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An Array of iFolder Objects</returns>
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
		/// Get iFolders by Member
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <param name="role">The Member Role</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An Array of iFolder Objects</returns>
		[WebMethod(
			 Description="Get iFolders by Member",
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
		/// Get iFolders by Name
		/// </summary>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An Array of iFolder Objects</returns>
		[WebMethod(
			 Description="Get iFolders by Name",
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
		/// Get Changes
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="entryID">The Entry ID</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An Array of iFolderEntry Objects</returns>
		[WebMethod(
			 Description="Get Changes",
			 EnableSession=true)]
		public ChangeEntry[] GetChanges(string ifolderID, string entryID, int index, int count, out int total)
		{
			ChangeEntry[] result = null;
			total = 0;

			try
			{
				result = ChangeEntry.GetChanges(ifolderID, entryID, index, count, out total, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		#endregion

		#region User

		/// <summary>
		/// Get the Authenticated User
		/// </summary>
		/// <returns>An iFolderUser Object</returns>
		[WebMethod(
			 Description="Get the Authenticated User",
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
		/// Set the Rights of an iFolder Member
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The Member's User ID</param>
		/// <param name="rights">The Member's New Rights</param>
		/// <remarks>This API will accept multiple User IDs in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Set the Rights of an iFolder Member",
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
		/// Add a Member to an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The New Member's User ID</param>
		/// <param name="rights">The New Member's Rights</param>
		/// <remarks>This API will accept multiple User IDs in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Add a Member to an iFolder",
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
		/// Remove a Member from an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The Member's User ID</param>
		/// <remarks>This API will accept multiple User IDs in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Remove a Member from an iFolder",
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
		/// Set the Owner of an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The New Owner's User ID</param>
		[WebMethod(
			 Description="Set the Owner of an iFolder",
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
		/// Get the Members of an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An Array of Members</returns>
		[WebMethod(
			 Description="Get the Members of an iFolder",
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
		/// Get the iFolder Users
		/// </summary>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An array of Users</returns>
		[WebMethod(
			 Description="Get the iFolder Users",
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
		/// Get a User
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <returns>A User Object</returns>
		[WebMethod(
			 Description="Get a User",
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
		/// Get User Details
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <returns>A User Details Object</returns>
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
		/// Search the Users of the iFolder System
		/// </summary>
		/// <param name="property">The Search Property</param>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An Array of iFolderUser Objects</returns>
		[WebMethod(
			 Description="Search the Users of the iFolder System",
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
		/// Give a User Administration Rights
		/// </summary>
		/// <remarks>
		/// A User is an administrator if the user has "Admin" rights in the domain.
		/// </remarks>
		/// <param name="userID">The User ID</param>
		/// <remarks>This API will accept multiple User IDs in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Give a User Administration Rights",
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
		/// Remove Administration Rights from a User
		/// </summary>
		/// <remarks>
		/// Administration rights are removed by giving the user "ReadOnly" rights in the domain.
		/// </remarks>
		/// <param name="userID">The Administrator's User ID</param>
		/// <remarks>This API will accept multiple User IDs in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Remove Administration Rights from a User",
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
		/// Get all the Administrators
		/// </summary>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <remarks>
		/// A User is an administrator if the user has "Admin" rights in the domain.
		/// </remarks>
		/// <returns>An Array of iFolderUser Objects</returns>
		[WebMethod(
			 Description="Get all the Administrators",
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
		/// Get the System Policy
		/// </summary>
		/// <returns>A SystemPolicy Object</returns>
		[WebMethod(
			 Description="Get the System Policy",
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
		/// Set the System Policy
		/// </summary>
		/// <param name="props">The SystemPolicy Object</param>
		[WebMethod(
			 Description="Set the System Policy",
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
		/// Get User Policy
		/// </summary>
		/// <param name="userID">A User ID</param>
		/// <returns>A UserPolicy Object</returns>
		[WebMethod(
			 Description="Get User Policy",
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
		/// Set User Policy
		/// </summary>
		/// <param name="props">The UserPolicy Object</param>
		[WebMethod(
			 Description="Set User Policy",
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
		/// Get iFolder Policy
		/// </summary>
		/// <param name="ifolderID">An iFolder ID</param>
		/// <returns>A iFolderPolicy Object</returns>
		[WebMethod(
			 Description="Get iFolder Policy",
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
		/// Set iFolder Policy
		/// </summary>
		/// <param name="props">The iFolderPolicy Object</param>
		[WebMethod(
			 Description="Set iFolder Policy",
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
		/// Get Identity Policy
		/// </summary>
		/// <returns>An IdentityPolicy Object</returns>
		[WebMethod(
		Description="Get the policy of the registered identity provider",
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
		/// Method to create a new user in the default simias domain
		/// Note: some identity providers DO NOT allow the creation,
		/// modification or deletion of new users
		/// </summary>
		/// <param name="Username">Username (mandatory) short name of the user</param>
		/// <param name="Password">Password (mandatory)</param>
		/// <param name="UserGuid">UserGuid (optional) caller can specify the guid for the user</param>
		/// <param name="FirstName">FirstName (optional) first/given name of the user</param>
		/// <param name="LastName">LastName (optional) last/family name of the user</param>
		/// <param name="FullName">FullName (optional) Fullname of the user</param>
		/// <param name="DistinguishedName">DistinguishedName (optional) usually the distinguished name from an external identity store</param>
		/// <param name="Email">Email (optional) Primary email address</param>
		/// <remarks>
		/// If the FirstName and LastName are specified but the FullName is null, FullName is
		/// autocreated using: FirstName + " " + LastName
		/// </remarks>
		[WebMethod(
		Description= "Method to create a new user in the Simias domain",
		EnableSession = true)]
		public
		RegistrationInfo
		CreateUser(
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

				// Check if the registered provider allows user creation
				IUserProvider provider = Simias.Server.User.GetRegisteredProvider();
				UserProviderCaps caps = provider.GetCapabilities();
				if ( caps.CanCreate == false )
				{
					info = new RegistrationInfo( RegistrationStatus.MethodNotSupported );
					info.Message = "Identity provider does not allow creation";
				}
				else
				if ( Username == null || Username == "" || Password == null )
				{
					info = new RegistrationInfo( RegistrationStatus.InvalidParameters );
					info.Message = "Missing mandatory parameters";
				}
				else
				{
					Simias.Server.User user = new Simias.Server.User( Username );
					user.FirstName = FirstName;
					user.LastName = LastName;
					user.UserGuid = UserGuid;
					user.FullName = FullName;
					user.DN = DistinguishedName;
					user.Email = Email;
				
					info = user.Create( Password );
				}
			}
			catch( Exception e )
			{
				SmartException.Throw( e );
			}
			
			return info;
		}
		
		/// <summary>
		/// Method to delete a user from the default simias domain
		/// Note: some identity providers DO NOT allow the creation,
		/// modification or deletion of new users
		/// </summary>
		/// <param name="Username">Username (mandatory) short name of the user</param>
		/// <remarks>
		/// </remarks>
		[WebMethod(
		Description= "Method to delete a user from the Simias domain",
		EnableSession = true)]
		public
		bool
		DeleteUser( string Username )
		{
			bool status = false;
			try
			{
				Authorize();

				// Check if the registered provider allows deletes
				IUserProvider provider = Simias.Server.User.GetRegisteredProvider();
				UserProviderCaps caps = provider.GetCapabilities();
				if ( caps.CanDelete == true )
				{
					if ( Username != null && Username != "" )
					{
						Simias.Server.User user = new Simias.Server.User( Username );
						status = user.Delete();
					}
				}
			}
			catch( Exception e )
			{
				SmartException.Throw( e );
			}
			
			return status;
		}

		/// <summary>
		/// Method to reset a user's password.
		/// Note: some identity providers DO NOT allow the creation,
		/// modification or deletion of new users
		/// Note2: This method is probably temporary until the
		/// self-service framework is designed and implemented.
		/// </summary>
		/// <param name="Username">(mandatory) short name of the user</param>
		/// <param name="Password">(mandatory) new password to set on the user</param>
		/// <remarks>
		/// </remarks>
		[WebMethod(
		Description= "Method to reset a user's password",
		EnableSession = true)]
		public
		bool
		SetPassword( string Username, string Password )
		{
			bool status = false;
			try
			{
				Authorize();

				// Check if the registered provider allows deletes
				IUserProvider provider = Simias.Server.User.GetRegisteredProvider();
				UserProviderCaps caps = provider.GetCapabilities();
				if ( caps.CanModify == true )
				{
					if ( Username != null && Username != "" && Password != null )
					{
						status = Simias.Server.User.SetPassword( Username, Password );
					}
				}
			}
			catch( Exception e )
			{
				SmartException.Throw( e );
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

		#region Sample Data
		
		/// <summary>
		/// GenerateSampleData
		/// </summary>
		[WebMethod(
			 Description="Get the Authenticated User",
			 EnableSession=true)]
		public void GenerateSampleData()
		{
			try
			{
				SampleData.Generate();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		#endregion

		#region Utility

		/// <summary>
		/// Get the Current Principal Access User ID
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
		/// Authorize the Current Principal
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
