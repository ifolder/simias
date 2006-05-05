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
	/// iFolder Common Web Service
	/// </summary>
	public abstract class iFolderCommon : System.Web.Services.WebService
	{
		#region Constructors
		
		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderCommon()
		{
		}
		
		#endregion

		#region System

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
		

		/// <summary>
		/// Get information about the authenticated user's home iFolder server.
		/// </summary>
		/// <returns>An iFolderServer object describing the user's home iFolder server.</returns>
		[WebMethod(
			 Description="Get information about the authenticated user's home iFolder server.",
			 EnableSession=true)]
		public iFolderServer GetHomeServer()
		{
			iFolderServer result = null;

			try
			{
				Authorize();

				result = iFolderServer.GetHomeServer();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about all the iFolder servers.
		/// </summary>
		/// <returns>An array of iFolderServer objects.</returns>
		[WebMethod(
			 Description="Get information about all the iFolder servers.",
			 EnableSession=true)]
		public iFolderServer[] GetServers()
		{
			iFolderServer[] result = null;

			try
			{
				Authorize();

				result = iFolderServer.GetServers();
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
				Authorize();

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
				Authorize();

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
				Authorize();

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
				Authorize();

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
				Authorize();

				iFolder.SetDescription(ifolderID, description, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
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
				Authorize();

				iFolder.PublishiFolder(ifolderID, publish, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		#endregion

		#region Changes
		
		/// <summary>
		/// Get a history of changes to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="entryID">The id of entry to filter the results (can be null for no filtering).</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of ChangeEntry objects.</returns>
		[WebMethod(
			 Description="Get a history of changes to an iFolder.",
			 EnableSession=true)]
		public ChangeEntrySet GetChanges(string ifolderID, string entryID, int index, int max)
		{
			ChangeEntrySet result = null;

			try
			{
				Authorize();

				result = ChangeEntry.GetChanges(ifolderID, entryID, index, max, GetAccessID());
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

				result = iFolderUser.GetUser(null, GetUserID(), GetAccessID());
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
							iFolderUser.SetMemberRights(ifolderID, id, rights, GetAccessID());
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
							iFolderUser.AddMember(ifolderID, id, rights, GetAccessID());
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
							iFolderUser.RemoveMember(ifolderID, id, GetAccessID());
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

				iFolderUser.SetOwner(ifolderID, userID, GetAccessID());
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
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects describing the members.</returns>
		[WebMethod(
			 Description="Get information about the members of an iFolder.",
			 EnableSession=true)]
		public iFolderUserSet GetMembers(string ifolderID, int index, int max)
		{
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUsers(ifolderID, index, max, GetAccessID());
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
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users.",
			 EnableSession=true)]
		public iFolderUserSet GetUsers(int index, int max)
		{
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUsers(null, index, max, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about a user using an id or username.
		/// </summary>
		/// <param name="userID">The id or username of the user.</param>
		/// <returns>A iFolderUser object.</returns>
		[WebMethod(
			 Description="Get information about a user using an id or username.",
			 EnableSession=true)]
		public iFolderUser GetUser(string userID)
		{
			iFolderUser result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUser(userID, GetAccessID());
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
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users identified by the search property, operation, and pattern.",
			 EnableSession=true)]
		public iFolderUserSet GetUsersBySearch(SearchProperty property, SearchOperation operation, string pattern, int index, int max)
		{
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUsers(property, operation, pattern, index, max, GetAccessID());
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
		protected abstract string GetAccessID();

		/// <summary>
		/// Get the authenticated user's id.
		/// </summary>
		protected abstract string GetUserID();

		/// <summary>
		/// Authorize the authenticated user.
		/// </summary>
		protected abstract void Authorize();

		#endregion
	}
}
