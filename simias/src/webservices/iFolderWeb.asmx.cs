/***********************************************************************
 *  $RCSfile: iFolderWeb.asmx.cs,v $
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

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder Web Service
	/// </summary>
	[WebService(
		 Namespace="http://novell.com/ifolder/webservice/",
		 Name="iFolderWeb",
		 Description="iFolder Web Service")]
	public class iFolderWeb : System.Web.Services.WebService
	{
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderWeb()
		{
		}

		#endregion

		#region System


		/// <summary>
		/// Get information about the authenticated user's home iFolder server.
		/// </summary>
		/// <returns>An iFolderServer object describing the user's home iFolder server.</returns>
		[WebMethod(
			 Description="Get information about the authenticated user's home iFolder server.",
			 EnableSession=true)]
		public iFolderServer GetServer()
		{
			iFolderServer result = null;

			try
			{
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
		/// <returns>An iFolderSystem object.</returns>
		[WebMethod(
			 Description="Get information about the iFolder system.",
			 EnableSession=true)]
		public iFolderSystem GetSystem()
		{
			iFolderSystem result = null;

			try
			{
				result = iFolderSystem.GetSystem();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		#endregion

		#region iFolders

		/// <summary>
		/// Create a new iFolder with the authenticated user as the owner.
		/// </summary>
		/// <param name="name">The name of the new iFolder.</param>
		/// <param name="description">The description of the new iFolder. </param>
		/// <returns>An iFolder object describing the new iFolder.</returns>
		[WebMethod(
			 Description="Create a new iFolder with the authenticated user as the owner.",
			 EnableSession=true)]
		public iFolder CreateiFolder(string name, string description)
		{
			iFolder result = null;

			try
			{
				string accessID = GetAccessID();

				result = iFolder.CreateiFolder(name, accessID, description, accessID);
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
		public void DeleteiFolder(string ifolderID)
		{
			Hashtable exceptions = new Hashtable();

			try
			{
				string accessID = GetAccessID();

				string[] ids = ifolderID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolder.DeleteiFolder(id, accessID);
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
		/// Remove the authenticated user's rights to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <remarks>This API will accept multiple iFolder ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Remove the authenticated user's rights to an iFolder.",
			 EnableSession=true)]
		public void RemoveiFolder(string ifolderID)
		{
			Hashtable exceptions = new Hashtable();

			try
			{
				string accessID = GetAccessID();

				string[] ids = ifolderID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							// NOTE: the accessID on this call in null, because
							// we want the user to be able to remove themselves 
							// from any iFolder regardless of rights
							iFolderUser.RemoveMember(id, accessID, null);
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
		/// <returns>An iFolder object describing the iFolder.</returns>
		[WebMethod(
			 Description="Get information about an iFolder.",
			 EnableSession=true)]
		public iFolder GetiFolder(string ifolderID)
		{
			iFolder result = null;

			try
			{
				result = iFolder.GetiFolder(ifolderID, GetAccessID());
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
		/// <param name="ifolderID">The id of the iFolder</param>
		/// <returns>An iFolderDetails object describing the iFolder</returns>
		/// <remarks>It is more expensive to call GetiFolderDetails than GetiFolder.</remarks>
		[WebMethod(
			 Description="Get detailed information about an iFolder.",
			 EnableSession=true)]
		public iFolderDetails GetiFolderDetails(string ifolderID)
		{
			iFolderDetails result = null;

			try
			{
				result = iFolderDetails.GetiFolderDetails(ifolderID, GetAccessID());
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
				iFolder.SetDescription(ifolderID, description, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Get information about all the iFolders to which the authenticate user has rights.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about all the iFolders to which the authenticate user has rights.",
			 EnableSession=true)]
		public iFolder[] GetiFolders(int index, int count, out int total)
		{
			iFolder[] result = null;
			total = 0;

			try
			{
				string accessID = GetAccessID();

				result = iFolder.GetiFoldersByMember(accessID, MemberRole.Any, index, count, out total, accessID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about an iFolder identified by the given name.
		/// </summary>
		/// <param name="ifolderName">The name of the iFolder.</param>
		/// <returns>An iFolder object.</returns>
		[WebMethod(
			 Description="Get information about an iFolder identified by the given name.",
			 EnableSession=true)]
		public iFolder GetiFolderByName(string ifolderName)
		{
			iFolder result = null;

			try
			{
				result = iFolder.GetiFolderByName(ifolderName, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
		
		/// <summary>
		/// Get information about iFolders identified by the search operation and pattern.
		/// </summary>
		/// <param name="operation">The operation for the search.</param>
		/// <param name="pattern">The pattern for the search.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about iFolders identified by the search operation and pattern.",
			 EnableSession=true)]
		public iFolder[] GetiFoldersByName(SearchOperation operation, string pattern, int index, int count, out int total)
		{
			iFolder[] result = null;
			total = 0;

			try
			{
				string accessID = GetAccessID();

				result = iFolder.GetiFoldersByMember(accessID, MemberRole.Any, operation, pattern, index, count, out total, accessID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about iFolder identified by role, creation time, and search operation and pattern.
		/// </summary>
		/// <param name="role">The required authenticated user's role in the Folder.</param>
		/// <param name="after">The required earliest limit on the creation date of the iFolder. </param>
		/// <param name="operation">The operation for the search.</param>
		/// <param name="pattern">The pattern for the search.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about iFolder identified by role, creation time, and search operation and pattern.",
			 EnableSession=true)]
		public iFolder[] GetiFoldersBySearch(MemberRole role, DateTime after, SearchOperation operation, string pattern, int index, int count, out int total)
		{
			iFolder[] result = null;
			total = 0;

			try
			{
				string accessID = GetAccessID();

				result = iFolder.GetiFoldersByMember(accessID, role, after, operation, pattern, index, count, out total, accessID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Publish an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="publish">The published state of the iFolder</param>
		[WebMethod(
			 Description="Publish an iFolder.",
			 EnableSession=true)]
		public void PublishiFolder(string ifolderID, bool publish)
		{
			try
			{
				iFolder.PublishiFolder(ifolderID, publish, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		#endregion

		#region Users

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
				string accessID = GetAccessID();

				result = iFolderUser.GetUser(null, accessID, accessID);
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
		/// <returns>A user object.</returns>
		[WebMethod(
			 Description="Get information about a user.",
			 EnableSession=true)]
		public iFolderUser GetUser(string userID)
		{
			iFolderUser result = null;

			try
			{
				result = iFolderUser.GetUser(null, userID, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
				result = iFolderUser.GetUsers(ifolderID, index, count, out total, GetAccessID());
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
				string accessID = GetAccessID();

				string[] ids = userID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolderUser.SetMemberRights(ifolderID, id, rights, accessID);
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
				string accessID = GetAccessID();

				string[] ids = userID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolderUser.AddMember(ifolderID, id, rights, accessID);
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
				string accessID = GetAccessID();

				string[] ids = userID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolderUser.RemoveMember(ifolderID, id, accessID);
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
				iFolderUser.SetOwner(ifolderID, userID, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Get information about all of the iFolder users.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
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
				result = iFolderUser.GetUsers(null, index, count, out total, GetAccessID());
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
				result = iFolderUser.GetUsers(property, operation, pattern, index, count, out total, GetAccessID());
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
		/// Get policy information for the authenticated user.
		/// </summary>
		/// <returns>A UserPolicy object describing the authenticated user's policy.</returns>
		[WebMethod(
			 Description="Get policy information for the authenticated user.",
			 EnableSession=true)]
		public UserPolicy GetAuthenticatedUserPolicy()
		{
			UserPolicy result = null;

			try
			{
				string accessID = GetAccessID();

				result = UserPolicy.GetPolicy(accessID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
				string accessID = GetAccessID();

				result = iFolderPolicy.GetPolicy(ifolderID, accessID);
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
				string accessID = GetAccessID();

				iFolderPolicy.SetPolicy(props, accessID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
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
		public iFolderEntry CreateEntry(string ifolderID, string parentID, iFolderEntryType type, string entryName)
		{
			iFolderEntry result = null;

			try
			{
				result = iFolderEntry.CreateEntry(ifolderID, parentID, type, entryName, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public void DeleteEntry(string ifolderID, string entryID)
		{
			Hashtable exceptions = new Hashtable();

			string accessID = GetAccessID();

			string[] ids = entryID.Split(new char[] {',', ' '});

			foreach(string id in ids)
			{
				if (id.Length > 0)
				{
					try
					{
						iFolderEntry.DeleteEntry(ifolderID, id, accessID);
					}
					catch(Exception e)
					{
						exceptions.Add(id, e);
					}
				}
			}

			SmartException.Throw(exceptions);
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
		public iFolderEntry GetEntry(string ifolderID, string entryID)
		{
			iFolderEntry result = null;

			try
			{
				result = iFolderEntry.GetEntry(ifolderID, entryID, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public iFolderEntry GetEntryByPath(string ifolderID, string entryPath)
		{
			iFolderEntry result = null;

			try
			{
				result = iFolderEntry.GetEntryByPath(ifolderID, entryPath, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
		
		/// <summary>
		/// Get information about entries in an iFolder identified by their parent entry.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="entryID">The id of the parent entry (can be null for the root of the iFolder).</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolderEntry objects.</returns>
		[WebMethod(
			 Description="Get information about entries identified by their parent entry.",
			 EnableSession=true)]
		public iFolderEntry[] GetEntries(string ifolderID, string entryID, int index, int count, out int total)
		{
			iFolderEntry[] result = null;
			total = 0;

			try
			{
				result = iFolderEntry.GetEntries(ifolderID, entryID, index, count, out total, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about entries in an iFolder identified by a search on the name.
		/// </summary>
		/// <param name="ifolderID">The id of iFolder.</param>
		/// <param name="parentID">The id of the parent entry.</param>
		/// <param name="operation">The operation to compare the name and search pattern.</param>
		/// <param name="pattern">The pattern to search.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <param name="total">The total number of search results available.</param>
		/// <returns>An array of iFolderEntry objects.</returns>
		[WebMethod(
			 Description="Get information about entries in an iFolder identified by a search on the name.",
			 EnableSession=true)]
		public iFolderEntry[] GetEntriesByName(string ifolderID, string parentID, SearchOperation operation, string pattern, int index, int count, out int total)
		{
			iFolderEntry[] result = null;
			total = 0;

			try
			{
				result = iFolderEntry.GetEntriesByName(ifolderID, parentID, operation, pattern, index, count, out total, GetAccessID());
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
		public string OpenFileRead(string ifolderID, string entryID)
		{
			string id = null;

			try
			{
				iFolderFile file = new iFolderFile(ifolderID, entryID, GetAccessID());
				file.OpenRead();
				id = file.ID;
				Session[id] = file;
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return id;
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
		public string OpenFileWrite(string ifolderID, string entryID, long length)
		{
			string id = null;

			try
			{
				iFolderFile file = new iFolderFile(ifolderID, entryID, GetAccessID());
				file.OpenWrite(length);
				id = file.ID;
				Session[id] = file;
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return id;
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
		public byte[] ReadFile(string file, int size)
		{
			byte[] result = null;

			try
			{
				if (Session[file] != null)
				{
					result = (Session[file] as iFolderFile).Read(size);
				}
				else
				{
					throw new iFolderFileNotOpenException(file);
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Write to an openned iFolder file entry.
		/// </summary>
		/// <param name="file">The file handle.</param>
		/// <param name="buffer">The buffer to be written.</param>
		[WebMethod(
			 Description="Write to an openned iFolder file entry.",
			 EnableSession=true)]
		public void WriteFile(string file, byte[] buffer)
		{
			try
			{
				if (Session[file] != null)
				{
					(Session[file] as iFolderFile).Write(buffer);
				}
				else
				{
					throw new iFolderFileNotOpenException(file);
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Close an openned iFolder file entry.
		/// </summary>
		/// <param name="file">The file handle.</param>
		[WebMethod(
			 Description="Close an openned iFolder file entry.",
			 EnableSession=true)]
		public void CloseFile(string file)
		{
			try
			{
				if (Session[file] != null)
				{
					(Session[file] as iFolderFile).Close();
					Session[file] = null;
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
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

		#endregion
	}
}
