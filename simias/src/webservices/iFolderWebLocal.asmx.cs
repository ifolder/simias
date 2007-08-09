/****************************************************************************
|
| Copyright (c) 2007 Novell, Inc.
| All Rights Reserved.
|
| This program is free software; you can redistribute it and/or
| modify it under the terms of version 2 of the GNU General Public License as
| published by the Free Software Foundation.
|
| This program is distributed in the hope that it will be useful,
| but WITHOUT ANY WARRANTY; without even the implied warranty of
| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
| GNU General Public License for more details.
|
| You should have received a copy of the GNU General Public License
| along with this program; if not, contact Novell, Inc.
|
| To contact Novell about this file by physical or electronic mail,
| you may find current contact information at www.novell.com 
|
| Author: Rob 
|***************************************************************************/

using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections;

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder Local Web Service
	/// </summary>
	[WebService(
		 Namespace="http://novell.com/ifolder/webservice/",
		 Name="iFolderWebLocal",
		 Description="iFolder Web Local Service")]
	public class iFolderWebLocal : iFolderCommonLocal
	{

	        enum Securitystate
        	{
			encrypt = 1,
			enforceEncrypt = 2,
			encryptionState = 3,
			SSL = 4,
			enforceSSL = 8,
			SSLState = 12,
			UserEncrypt = 16,
			UserSSL = 32
        	}

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderWebLocal()
		{
		}

		#endregion

		#region iFolders

		/// <summary>
		/// Get information about all the iFolders to which the authenticate user has rights.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about all the iFolders to which the authenticate user has rights.",
			 EnableSession=true)]
		public virtual iFolderSet GetiFolders(int index, int max)
		{
			iFolderSet result = null;

			try
			{
				string accessID = GetAccessID();

				result = iFolder.GetiFoldersByMember(accessID, MemberRole.Any, index, max, accessID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public virtual iFolder CreateiFolder(string name, string description, bool ssl, string encryptionAlgorithm, string PassPhrase)
		{
			iFolder result = null;

			try
			{
				Authorize();

				string accessID = GetAccessID();
				result = iFolder.CreateiFolder(name, accessID, description, accessID, ssl, encryptionAlgorithm, PassPhrase);
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
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about iFolders identified by the search operation and pattern.",
			 EnableSession=true)]
		public virtual iFolderSet GetiFoldersByName(SearchOperation operation, string pattern, int index, int max)
		{
			iFolderSet result = null;

			try
			{
				Authorize();

				string accessID = GetAccessID();

				result = iFolder.GetiFoldersByMember(GetUserID(), MemberRole.Any, operation, pattern, index, max, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// Whether the tab should be shown or not for encrypted folder
		/// </summary>
		/// <param name="file">passphrase and encryptionalgo name</param>
		[WebMethod(
			 Description="checks whether the tab should be shown or not for encrypted folder",
			 EnableSession=true)]
		public virtual bool ShowTabDetails(string PassPhrase, string EncryptionAlgorithm)
		{
			bool result = true;
			try
			{
				result = iFolder.ShowTabDetails(PassPhrase, EncryptionAlgorithm);
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
		/// <param name="after">The required earliest limit on the shared date of the iFolder. </param>
		/// <param name="operation">The operation for the search.</param>
		/// <param name="pattern">The pattern for the search.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder objects.</returns>
		[WebMethod(
			 Description="Get information about iFolder identified by role, creation time, and search operation and pattern.",
			 EnableSession=true)]
		public virtual iFolderSet GetiFoldersBySearch(MemberRole role, DateTime after, SearchOperation operation, string pattern, int index, int max)
		{
			iFolderSet result = null;

			try
			{
				Authorize();

				string accessID = GetAccessID();

				result = iFolder.GetiFoldersByMember(accessID, role, after, operation, pattern, index, max, accessID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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
		public virtual void RemoveMembership(string ifolderID)
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

		#endregion

		#region Policy
		
		/// <summary>
		/// Get policy information for the authenticated user.
		/// </summary>
		/// <returns>A UserPolicy object describing the authenticated user's policy.</returns>
		[WebMethod(
			 Description="Get policy information for the authenticated user.",
			 EnableSession=true)]
		public virtual UserPolicy GetAuthenticatedUserPolicy()
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
		public virtual iFolderPolicy GetiFolderPolicy(string ifolderID)
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
		/// Get the policy for an iFolder.
		/// </summary>
		/// <param name="policy">The iFolderPolicy object.</param>
		[WebMethod(
			 Description="Get policy information for an iFolder.",
			 EnableSession=true)]
		public virtual int GetEncryptionPolicy()
		{
			UserPolicy user = null;
			SystemPolicy system = null;
			int SysEncrPolicy = 0, UserEncrPolicy = 0, securityStatus = 0;
			try
			{
				string accessID = GetAccessID();
				user = UserPolicy.GetPolicy(accessID);
				system = SystemPolicy.GetPolicy();
				UserEncrPolicy = user.EncryptionStatus;
				SysEncrPolicy = system.EncryptionStatus;
				securityStatus += DeriveStatus( SysEncrPolicy, UserEncrPolicy, UserEncrPolicy);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return securityStatus;
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
				string accessID = GetAccessID();

				iFolderPolicy.SetPolicy(policy, accessID);
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
		public virtual iFolderEntry CreateEntry(string ifolderID, string parentID, iFolderEntryType type, string entryName)
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
		public virtual void DeleteEntry(string ifolderID, string entryID)
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
		public virtual iFolderEntry GetEntry(string ifolderID, string entryID)
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
		public virtual iFolderEntry GetEntryByPath(string ifolderID, string entryPath)
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
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderEntry objects.</returns>
		[WebMethod(
			 Description="Get information about entries identified by their parent entry.",
			 EnableSession=true)]
		public virtual iFolderEntrySet GetEntries(string ifolderID, string entryID, int index, int max)
		{
			iFolderEntrySet result = null;

			try
			{
				result = iFolderEntry.GetEntries(ifolderID, entryID, index, max, GetAccessID());
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
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderEntry objects.</returns>
		[WebMethod(
			 Description="Get information about entries in an iFolder identified by a search on the name.",
			 EnableSession=true)]
		public virtual iFolderEntrySet GetEntriesByName(string ifolderID, string parentID, SearchOperation operation, string pattern, int index, int max)
		{
			iFolderEntrySet result = null;

			try
			{
				result = iFolderEntry.GetEntriesByName(ifolderID, parentID, operation, pattern, index, max, GetAccessID());
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
		/// Get a setting specific to the authenticated user.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting.</returns>
		[WebMethod(
			 Description="Get a setting specific to the authenticated user.",
			 EnableSession=true)]
		public virtual string GetSetting(string name)
		{
			string result = null;

			try
			{
				result = Settings.GetUserSetting(GetAccessID(), name);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Set a setting specific to the authenticated user.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		[WebMethod(
			 Description="Set a setting specific to the authenticated user.",
			 EnableSession=true)]
		public virtual void SetSetting(string name, string value)
		{
			try
			{
				Settings.SetUserSetting(GetAccessID(), name, value);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return;
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
		public virtual string OpenFileRead(string ifolderID, string entryID)
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
		public virtual string OpenFileWrite(string ifolderID, string entryID, long length)
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
		public virtual byte[] ReadFile(string file, int size)
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
					throw new FileNotOpenException(file);
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
		public virtual void WriteFile(string file, byte[] buffer)
		{
			try
			{
				if (Session[file] != null)
				{
					(Session[file] as iFolderFile).Write(buffer);
				}
				else
				{
					throw new FileNotOpenException(file);
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
		public virtual void CloseFile(string file)
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
		
		/// set the file length
		/// </summary>
		/// <param name="file">The file handle.</param>
		[WebMethod(
			 Description="Set the basefile node length.",
			 EnableSession=true)]
		public virtual void SetFileLength(string ifolderID, string nodeID, long length)
		{
			try
			{
				iFolderEntry.SetFileLength(ifolderID, nodeID, GetAccessID(), length);		
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}


		#endregion

		#region Utility


		/// <summary>
		/// Get the access user's id.
		/// </summary>
		protected override string GetAccessID()
		{
			return GetUserID();
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
			// no authorization needed
		}

		/// <summary>
		/// Get the policy for an iFolder.
		/// </summary>
		/// <param name="policy">The iFolderPolicy object.</param>
		private int DeriveStatus(int system, int user, int preference)
		{
			//Preference is not done
			if( preference == 0)
			{
				if(system != 0)
				    return system;
				return user;
			}
			else
			{
				if(user != 0)
				    return user;
				return system;
			}
		}

			#endregion
	}
}
