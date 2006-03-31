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
	/// iFolder Web Web Service
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
		/// Create a New iFolder
		/// </summary>
		/// <param name="name">The New iFolder Name</param>
		/// <param name="description">The New iFolder Description</param>
		/// <returns>An iFolder Object</returns>
		[WebMethod(
			 Description="Create a New iFolder",
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
				result = iFolder.GetiFolder(ifolderID, GetAccessID());
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
				result = iFolderDetails.GetiFolderDetails(ifolderID, GetAccessID());
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
				iFolder.SetDescription(ifolderID, description, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Get iFolders
		/// </summary>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An Array of iFolder Objects</returns>
		[WebMethod(
			 Description="Get iFolders",
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
		/// Get an iFolder by Name
		/// </summary>
		/// <param name="ifolderName">The iFolder Name</param>
		/// <returns>An iFolder Object</returns>
		[WebMethod(
			 Description="Get an iFolder by Name",
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
		/// Publish an iFolder
		/// </summary>
		/// <param name="ifolder">The ID or friendly name of the iFolder.</param>
		/// <param name="publish">true == Publish, false == Unpublish.</param>
		[WebMethod(
			 Description="Publish an iFolder",
			 EnableSession=true)]
		public void PublishiFolder(string ifolder, bool publish)
		{
			try
			{
				if ( !iFolder.PublishiFolder( ifolder, publish, GetAccessID() ) )
				{
					ApplicationException ae = 
						new ApplicationException( "Failed to publish: " + ifolder );
					SmartException.Throw( ae );
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		#endregion

		#region Users

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
				result = iFolderUser.GetUser(null, userID, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get the Members of an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An Array of Member User Objects</returns>
		[WebMethod(
			 Description="Get the Members of an iFolder",
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
				iFolderUser.SetOwner(ifolderID, userID, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
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
				result = iFolderUser.GetUsers(null, index, count, out total, GetAccessID());
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
				result = iFolderUser.GetUsers(property, operation, pattern, index, count, out total, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		#endregion

		#region Entries

		/// <summary>
		/// Create An iFolder File or Directory Entry
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="parentID">The Parent Entry ID</param>
		/// <param name="type">The iFolder Entry Type</param>
		/// <param name="entryName">The New Entry Name</param>
		/// <returns>An iFolderEntry Object</returns>
		[WebMethod(
			 Description="Create An iFolder File or Directory Entry",
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
		/// Delete An iFolder Entry
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="entryID">The Entry ID</param>
		/// <remarks>This API will accept multiple Entry IDs in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Delete An iFolder Entry",
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
		/// Get An iFolder Entry
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="entryID">The Entry ID</param>
		/// <returns>An iFolderEntry Object</returns>
		[WebMethod(
			 Description="Get An iFolder Entry",
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
		/// Get An iFolder Entry By Relative Path
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="entryPath">The Entry Path</param>
		/// <returns>An iFolderEntry Object</returns>
		[WebMethod(
			 Description="Get An iFolder Entry",
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
		/// Get iFolder Entries
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="entryID">The Parent Entry ID</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An Array of iFolderEntry Objects</returns>
		[WebMethod(
			 Description="Get iFolders Entries",
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
		/// Get iFolder Entries by Name
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="parentID">The Parent Entry ID</param>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <returns>An Array of iFolderEntry Objects</returns>
		[WebMethod(
			 Description="Get iFolders Entries",
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

		#region Files

		/// <summary>
		/// Open An iFolder File For Reading
		/// </summary>
		/// <param name="ifolderID">The ID of the iFolder.</param>
		/// <param name="entryID">The ID of the iFolder File Entry.</param>
		/// <returns>A File ID.</returns>
		[WebMethod(
			 Description="Open An iFolder File For Reading",
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
		/// Open An iFolder File For Writing
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="entryID">The File Entry ID</param>
		/// <param name="length">The File Length</param>
		/// <returns>A File ID</returns>
		[WebMethod(
			 Description="Open An iFolder File For Writing",
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
		/// Read From An iFolder File
		/// </summary>
		/// <param name="fileID">The File ID</param>
		/// <param name="size">The File Buffer Size</param>
		/// <returns>An Array of Bytes</returns>
		[WebMethod(
			 Description="Read From An iFolder File",
			 EnableSession=true)]
		public byte[] ReadFile(string fileID, int size)
		{
			byte[] result = null;

			try
			{
				if (Session[fileID] != null)
				{
					result = (Session[fileID] as iFolderFile).Read(size);
				}
				else
				{
					throw new iFolderFileNotOpenException(fileID);
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Write To An iFolder File
		/// </summary>
		/// <param name="fileID">The File ID</param>
		/// <param name="buffer">The File Buffer</param>
		[WebMethod(
			 Description="Write To An iFolder File",
			 EnableSession=true)]
		public void WriteFile(string fileID, byte[] buffer)
		{
			try
			{
				if (Session[fileID] != null)
				{
					(Session[fileID] as iFolderFile).Write(buffer);
				}
				else
				{
					throw new iFolderFileNotOpenException(fileID);
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Close An iFolder File
		/// </summary>
		/// <param name="fileID">The File ID</param>
		[WebMethod(
			 Description="Close An iFolder File",
			 EnableSession=true)]
		public void CloseFile(string fileID)
		{
			try
			{
				if (Session[fileID] != null)
				{
					(Session[fileID] as iFolderFile).Close();
					Session[fileID] = null;
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

		#endregion
	}
}
