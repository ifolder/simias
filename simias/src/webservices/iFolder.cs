/***********************************************************************
 *  $RCSfile: iFolder.cs,v $
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
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

using Simias.Storage;
using Simias.Web;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder
	/// </summary>
	[Serializable]
	public class iFolder
	{
		/// <summary>
		/// The iFolder ID
		/// </summary>
		public string ID;
		
		/// <summary>
		/// The iFolder Name
		/// </summary>
		public string Name;

		/// <summary>
		/// The iFolder Description
		/// </summary>
		public string Description;

		/// <summary>
		/// The iFolder OwnerID
		/// </summary>
		public string OwnerID;

		/// <summary>
		/// The iFolder Owner User Name
		/// </summary>
		public string OwnerUserName;
		
		/// <summary>
		/// The iFolder Owner Full Name
		/// </summary>
		public string OwnerFullName;
		
		/// <summary>
		/// The iFolder Domain ID
		/// </summary>
		public string DomainID;

		/// <summary>
		/// The iFolder Size
		/// </summary>
		public long Size = 0;

		/// <summary>
		/// iFolder/Domain Access Rights
		/// </summary>
		public Simias.Storage.Access.Rights Rights;

		/// <summary>
		/// iFolder Last Modified Time
		/// </summary>
		public DateTime LastModified = DateTime.MinValue;

		/// <summary>
		/// Has the iFolder been marked Published?
		/// </summary>
		public bool Published = false;

		/// <summary>
		/// iFolder Enabled?
		/// </summary>
		public bool Enabled = true;

		/// <summary>
		/// Number of Members
		/// </summary>
		public int MemberCount = 0;

		/// <summary>
		/// The Collection Type of an iFolder
		/// </summary>
		internal static readonly string iFolderCollectionType = "iFolder";

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolder()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="c">The iFolder Collection</param>
		/// <param name="accessID">The Access User ID</param>
		protected iFolder(Collection c, string accessID)
		{
			// impersonate
			Access.Rights rights = Impersonate(c, accessID);

			this.ID = c.ID;
			this.Name = c.Name;
			this.Description = NodeUtility.GetStringProperty(c, PropertyTags.Description);
			this.DomainID = c.Domain;
			this.Size = c.StorageSize;
			this.Rights = rights;
			this.LastModified = NodeUtility.GetDateTimeProperty(c, PropertyTags.JournalModified);
			this.Published = NodeUtility.GetBooleanProperty(c, PropertyTags.Published);
			this.Enabled = !iFolderPolicy.IsLocked(c);
			this.MemberCount = c.GetMemberList().Count;

			// owner
			this.OwnerID = c.Owner.UserID;
			Domain domain = Store.GetStore().GetDomain(this.DomainID);
			Member domainMember = domain.GetMemberByID(this.OwnerID);
			this.OwnerUserName = domainMember.Name;
			string fullName = domainMember.FN;
			this.OwnerFullName = (fullName != null) ? fullName : this.OwnerUserName;
		}

		/// <summary>
		/// Create an iFolder
		/// </summary>
		/// <param name="name">The iFolder Name</param>
		/// <param name="userID">The User ID of the iFolder Owner</param>
		/// <param name="description">The iFolder Description</param>
		/// <param name="accessID">The Access ID</param>
		/// <returns>An iFolder Object</returns>
		public static iFolder CreateiFolder(string name, string userID, string description, string accessID)
		{
			// NOTE: because the name of the iFolder will also be the
			// name of entry, we must check it
			iFolderEntry.CheckName(name);

			Collection c = SharedCollection.CreateSharedCollection(
				name, null, userID, iFolderCollectionType, true, null, description, accessID);

			return new iFolder(c, null);
		}

		/// <summary>
		/// Get an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolder Object</returns>
		public static iFolder GetiFolder(string ifolderID, string accessID)
		{
			Store store = Store.GetStore();
			
			Collection c = store.GetCollectionByID(ifolderID);
			
			if (c == null)  throw new iFolderDoesNotExistException(ifolderID);

			return new iFolder(c, accessID);
		}

		/// <summary>
		/// Get an iFolder by Name
		/// </summary>
		/// <param name="ifolderName">The iFolder Name</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolder Object</returns>
		public static iFolder GetiFolderByName(string ifolderName, string accessID)
		{
			Store store = Store.GetStore();
			
			Collection c = store.GetSingleCollectionByName(ifolderName);
			
			if (c == null)  throw new iFolderDoesNotExistException(ifolderName);

			return new iFolder(c, accessID);
		}

		/// <summary>
		/// Impersonate the User on the Collection
		/// </summary>
		/// <param name="collection">The iFolder Collection</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>Access Rights</returns>
		public static Access.Rights Impersonate(Collection collection, string accessID)
		{
			Simias.Storage.Access.Rights rights = Simias.Storage.Access.Rights.Deny;

			if ((accessID != null) && (accessID.Length != 0))
			{
				Member member = collection.GetMemberByID(accessID);

				if (member == null)
				{
					throw new iFolderMemberDoesNotExistException(accessID);
				}

				collection.Impersonate(member);

				rights = member.Rights;
			}
			else
			{
				// assume Admin rights with no access ID
				rights = Simias.Storage.Access.Rights.Admin;
			}

			return rights;
		}

		/// <summary>
		/// Delete an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="accessID">The Access User ID</param>
		public static void DeleteiFolder(string ifolderID, string accessID)
		{
			SharedCollection.DeleteSharedCollection(ifolderID, accessID);
		}

		/// <summary>
		/// Get iFolders
		/// </summary>
		/// <param name="type">iFolder Type</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An Array of iFolder Objects</returns>
		public static iFolder[] GetiFolders(iFolderType type, int index, int count, out int total, string accessID)
		{
			Store store = Store.GetStore();

			// admin ID
			Domain domain = store.GetDomain(store.DefaultDomain);
			String adminID = domain.Owner.UserID;

			ICSList collections = store.GetCollectionsByType(iFolderCollectionType);

			// sort the list
			ArrayList sortList = new ArrayList();
			
			foreach(ShallowNode sn in collections)
			{
				sortList.Add(sn);
			}
			
			sortList.Sort();

			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in sortList)
			{
				Collection c = store.GetCollectionByID(sn.ID);

				if (((c != null) && (c.IsType(iFolderCollectionType)))
					&& ((type != iFolderType.Orphaned) || (c.Owner.UserID == adminID)))
				{
					if ((i >= index) && (((count <= 0) || i < (count + index))))
					{
						list.Add(new iFolder(c, accessID));
					}

					++i;
				}
			}

			// save total
			total = i;

			return (iFolder[])list.ToArray(typeof(iFolder));
		}
			
		/// <summary>
		/// Get iFolders by Member
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <param name="role">The Member Role</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An Array of iFolder Objects</returns>
		public static iFolder[] GetiFoldersByMember(string userID, MemberRole role, int index, int count, out int total, string accessID)
		{
			return GetiFoldersByMember(userID, role, SearchOperation.Contains, null, index, count, out total, accessID);
		}
		
		/// <summary>
		/// Get iFolders by Member
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <param name="role">The Member Role</param>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An Array of iFolder Objects</returns>
		public static iFolder[] GetiFoldersByMember(string userID, MemberRole role, SearchOperation operation, string pattern, int index, int count, out int total, string accessID)
		{
			Store store = Store.GetStore();

			// get a list of members
			ICSList collections;
			
			if (role == MemberRole.Owner)
			{
				collections = store.GetCollectionsByOwner(userID);
			}
			else
			{
				collections = store.GetCollectionsByUser(userID);
			}

			// match the pattern
			Regex regex = null;
			
			if ((pattern != null) && (pattern.Length > 0))
			{
				switch(operation)
				{
					case SearchOperation.BeginsWith:
						pattern = "^" + pattern;
						break;

					case SearchOperation.EndsWith:
						pattern = pattern + "$";
						break;

					case SearchOperation.Equals:
						pattern = "^" + pattern + "$";
						break;

					case SearchOperation.Contains:
					default:
						break;
				}

				regex = new Regex(pattern, RegexOptions.IgnoreCase);
			}

			// sort the list
			ArrayList sortList = new ArrayList();
			
			foreach(ShallowNode sn in collections)
			{
				if ((regex == null) || regex.Match(sn.Name).Success)
				{
					sortList.Add(sn);
				}
			}
			
			sortList.Sort();

			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in sortList)
			{
				Collection c = store.GetCollectionByID(sn.ID);

				if (((c != null) && (c.IsType(iFolderCollectionType)))
					&& ((role != MemberRole.Shared) || (c.Owner.UserID != userID)))
				{
					if ((i >= index) && (((count <= 0) || i < (count + index))))
					{
						list.Add(new iFolder(c, accessID));
					}

					++i;
				}
			}

			// save total
			total = i;

			return (iFolder[])list.ToArray(typeof(iFolder));
		}

		/// <summary>
		/// Get iFolders by Name
		/// </summary>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An Array of iFolder Objects</returns>
		public static iFolder[] GetiFoldersByName(SearchOperation operation, string pattern, int index, int count, out int total, string accessID)
		{
			Store store = Store.GetStore();

			// search operator
			SearchOp searchOperation;

			switch(operation)
			{
				case SearchOperation.BeginsWith:
					searchOperation = SearchOp.Begins;
					break;

				case SearchOperation.EndsWith:
					searchOperation = SearchOp.Ends;
					break;

				case SearchOperation.Contains:
					searchOperation = SearchOp.Contains;
					break;

				case SearchOperation.Equals:
					searchOperation = SearchOp.Equal;
					break;

				default:
					searchOperation = SearchOp.Contains;
					break;
			}
			
			ICSList collections = store.GetCollectionsByName(pattern, searchOperation);

			// sort the list
			ArrayList sortList = new ArrayList();
			
			foreach(ShallowNode sn in collections)
			{
				sortList.Add(sn);
			}
			
			sortList.Sort();

			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in sortList)
			{
				Collection c = store.GetCollectionByID(sn.ID);

				if ((c != null) && (c.IsType(iFolderCollectionType)))
				{
					if ((i >= index) && (((count <= 0) || i < (count + index))))
					{
						list.Add(new iFolder(c, accessID));
					}

					++i;
				}
			}

			// save total
			total = i;

			return (iFolder[])list.ToArray(typeof(iFolder));
		}

		/// <summary>
		/// Se the Description of an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="description">The New iFolder Description</param>
		/// <param name="accessID">The Access User ID</param>
		public static void SetDescription(string ifolderID, string description, string accessID)
		{
			Store store = Store.GetStore();
			
			Collection c = store.GetCollectionByID(ifolderID);
			
			if (c == null)  throw new iFolderDoesNotExistException(ifolderID);

			// impersonate
			Impersonate(c, accessID);

			if ((description != null) && (description.Length > 0))
			{
				c.Properties.AddProperty(PropertyTags.Description, description);
			}
			else if (c.Properties.HasProperty(PropertyTags.Description))
			{
				c.Properties.DeleteSingleProperty(PropertyTags.Description);
			}

			// commit
			c.Commit();
		}
		
		/// <summary>
		/// Publish an iFolder
		/// </summary>
		/// <param name="ifolder">The ID or friendly name of the iFolder</param>
		/// <param name="publish">true == Publish, false == Unpublish.</param>
		/// <param name="accessID">The Access ID</param>
		/// <returns>true - Success, false - Failure</returns>
		/// <remarks>Only the owner can publich an iFolder.</remarks>
		public static bool PublishiFolder(string ifolder, bool publish, string accessID)
		{
			Store store = Store.GetStore();
			Collection c = store.GetCollectionByID( ifolder );
			if ( c == null )
			{
				c = store.GetSingleCollectionByName( ifolder );
				if ( c == null )
				{
					return false;
				}
			}
			
			if ( c.Owner.UserID != accessID )
			{
				return false;
			}
			
			if ( publish == true )
			{
				Property pubProp = new Property( PropertyTags.Published, true );
				c.Properties.ModifyProperty( pubProp );
			}
			else
			{
				c.Properties.DeleteSingleProperty( PropertyTags.Published );
			}
				
			c.Commit( c );
			return true;
		}
	}
}
