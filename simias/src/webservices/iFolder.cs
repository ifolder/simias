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

using Simias.Client;
using Simias.Storage;
using Simias.Web;
using Simias.Server;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder Result Set
	/// </summary>
	[Serializable]
	public class iFolderSet
	{
		/// <summary>
		/// An Array of iFolders
		/// </summary>
		public iFolder[] Items;

		/// <summary>
		/// The Total Number of iFolders
		/// </summary>
		public int Total;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public iFolderSet()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="items"></param>
		/// <param name="total"></param>
		public iFolderSet(iFolder[] items, int total)
		{
			this.Items = items;
			this.Total = total;
		}
	}

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
		/// Owner?
		/// </summary>
		public bool IsOwner;

		/// <summary>
		/// iFolder/Domain Access Rights
		/// </summary>
		public Rights MemberRights;

		/// <summary>
		/// iFolder Created Time
		/// </summary>
		public DateTime Created = DateTime.MinValue;

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
 		/// HostID of the location of the collection
 		/// </summary>
 	        public string HostID;

		/// <summary>
		/// The Collection Type of an iFolder
		/// </summary>
		internal static readonly string iFolderCollectionType = "iFolder";

		/// <summary>
		/// bitmap has encryption and SSL state for the ifolder
		/// </summary>
		public uint IfolderSecurity = 0;

		/// <summary>
		/// If encryption enabled, the algorithm type
		/// </summary>
		public string EncryptionAlgorithm = "";

		/// <summary>
		/// If encryption enabled, the encryptionkey
		/// </summary>
		public string EncryptionKey = "";
		
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
			Rights rights = Impersonate(c, accessID);

			this.ID = c.ID;
			this.Name = c.Name;
			this.Description = NodeUtility.GetStringProperty(c, PropertyTags.Description);
			this.DomainID = c.Domain;

			this.HostID = c.HostID;
			//Note : All iFolders (collections) have HostID. But A Domain on Master doesnt have it.
			if (this.HostID == null)    this.HostID = String.Empty;

			this.Size = c.StorageSize;
			this.MemberRights = rights;
			this.Created = NodeUtility.GetDateTimeProperty(c, PropertyTags.NodeCreationTime);
			this.LastModified = NodeUtility.GetDateTimeProperty(c, PropertyTags.JournalModified);
			this.Published = NodeUtility.GetBooleanProperty(c, PropertyTags.Published);
			this.Enabled = !iFolderPolicy.IsLocked(c);
			this.MemberCount = c.GetMemberList().Count;

			// owner
			this.OwnerID = c.Owner.UserID;
			this.IsOwner = (accessID != null) && (accessID == this.OwnerID);
			Domain domain = Store.GetStore().GetDomain(this.DomainID);
			Member domainMember = domain.GetMemberByID(this.OwnerID);
			this.OwnerUserName = domainMember.Name;
			string fullName = domainMember.FN;
			this.OwnerFullName = (fullName != null) ? fullName : this.OwnerUserName;

			//Only algorithm is needed by the middle tier, ssl can be configured by the admin
			this.EncryptionAlgorithm = c.EncryptionAlgorithm;
			this.EncryptionKey= c.EncryptionKey;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ce">The Catalog Entry</param>
		/// <param name="accessID">The Access User ID</param>
		protected iFolder(CatalogEntry ce, string accessID)
		{
			// impersonate
//		        Rights rights = Impersonate(ce, accessID);
                        //TODO :
		        Rights rights = Rights.Admin;

			this.ID = ce.CollectionID;
			this.Name = ce.Name;
                        //TODO:
//			this.Description = NodeUtility.GetStringProperty(c, PropertyTags.Description);
			this.Description = String.Empty;

			this.HostID = ce.HostID;
			this.Size = ce.CollectionSize;
			this.MemberRights = rights;
			this.Created = NodeUtility.GetDateTimeProperty(ce, PropertyTags.NodeCreationTime);
			this.LastModified = NodeUtility.GetDateTimeProperty(ce, PropertyTags.JournalModified);
			this.Published = NodeUtility.GetBooleanProperty(ce, PropertyTags.Published);
                        //BUG : TODO
//			this.Enabled = !iFolderPolicy.IsLocked(c);
			this.Enabled = true;
			this.MemberCount = ce.UserIDs.Length;

			// owner
			this.OwnerID = ce.OwnerID;
			this.IsOwner = (accessID != null) && (accessID == this.OwnerID);

			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			this.DomainID = domain.ID;
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
		public static iFolder CreateiFolder(string name, string userID, string description, string accessID, bool ssl, string encryptionAlgorithm)
		{
			// NOTE: because the name of the iFolder will also be the
			// name of entry, we must check it
			iFolderEntry.CheckName(name);

			Collection c = SharedCollection.CreateSharedCollection(
				name, null, ssl, userID, iFolderCollectionType, true, null, description, accessID, encryptionAlgorithm);

			return new iFolder(c, null);
		}

		/// <summary>
		/// Get the private url of iFolder's HomeServer
		/// </summary>
		/// <param name="name">The iFolder ID</param>
		/// <returns>Private url of iFolder's HomeServer</returns>
	        public static string GetiFolderLocation (string ifolderID)
		{
			Store store = Store.GetStore ();
		        Domain domain = store.GetDomain(store.DefaultDomain);

		        CatalogEntry ce = Catalog.GetEntryByCollectionID (ifolderID);

			HostNode remoteHost =  new HostNode (domain.GetMemberByID(ce.HostID));

			return remoteHost.PublicUrl;
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

			if (c == null)
			{
		                CatalogEntry ce = Catalog.GetEntryByCollectionID (ifolderID);
			        return new iFolder (ce, accessID);
			}

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
		/// <returns>Member Rights</returns>
		public static Rights Impersonate(Collection collection, string accessID)
		{
			Rights rights = Rights.Unknown;

			if ((accessID != null) && (accessID.Length != 0))
			{
				Member member = collection.GetMemberByID(accessID);

				if (member == null)
				{
					throw new MemberDoesNotExistException(accessID);
				}

				collection.Impersonate(member);

				rights = RightsUtility.Convert(member.Rights);
			}
			else
			{
				// assume Admin rights with no access ID
				rights = Rights.Admin;
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
		/// Get iFolders by Member
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <param name="role">The Member Role</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>A Set of iFolder Objects</returns>
		public static iFolderSet GetiFoldersByMember(string userID, MemberRole role, int index, int max, string accessID)
		{
			return GetiFoldersByMember(userID, role, DateTime.MinValue, SearchOperation.Contains, null, index, max, accessID);
		}
		
		/// <summary>
		/// Get iFolders by Member
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <param name="role">The Member Role</param>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>A Set of iFolder Objects</returns>
		public static iFolderSet GetiFoldersByMember(string userID, MemberRole role, SearchOperation operation, string pattern, int index, int max, string accessID)
		{
			return GetiFoldersByMember(userID, role, DateTime.MinValue, SearchOperation.Contains, pattern, index, max, accessID);
		}

		/// <summary>
		/// Get iFolders by Member
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <param name="role">The Member Role</param>
		/// <param name="after">Shared After Date/Time</param>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>A Set of iFolder Objects</returns>
		public static iFolderSet GetiFoldersByMember(string userID, MemberRole role, DateTime after, SearchOperation operation, string pattern, int index, int max, string accessID)
		{

			CatalogEntry[] catalogEntries;

		        catalogEntries = Catalog.GetAllEntriesByUserID (userID);			

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
			
			foreach(CatalogEntry ce in catalogEntries)
			{
			        if (role == MemberRole.Owner) 
				{
				    //Only Owned iFolders
				    if (!(ce.OwnerID == userID))
					    continue;
				}
				if (role == MemberRole.Shared)
				{
				    //Only Shared iFolders
    				    if (ce.OwnerID == userID)
					    continue;
				}

				if ((regex == null) || regex.Match(ce.Name).Success)
				{
					sortList.Add(ce);
				}
			}
			
			sortList.Sort();

			if (sortList == null )
			    throw new Exception ("ifolder.cs : sortlist is null");

			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(CatalogEntry ce in sortList)
			{
				// is iFolder?
				//TODO : collection type from catalog
//				if ((c == null) || !c.IsType(iFolderCollectionType)) continue;

				// role check
//				if ((role == MemberRole.Shared) && (ce.Owner.UserID == userID)) continue;

			        // This information is not available in CatalogEntry.
// 				// shared after
// 				if (after != DateTime.MinValue)
// 				{
// 					Member member = c.GetMemberByID(userID);
					
// 					if (after > NodeUtility.GetDateTimeProperty(ce, PropertyTags.NodeCreationTime))
// 						continue;
// 				}

				// pagging
				if ((i >= index) && (((max <= 0) || i < (max + index))))
				{
					list.Add(new iFolder(ce, accessID));
				}

				++i;
			}

			return new iFolderSet((iFolder[])list.ToArray(typeof(iFolder)), i);
		}

		/// <summary>
		/// Get iFolders by Name
		/// </summary>
		/// <param name="type">iFolder Type</param>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>A Set of iFolder Objects</returns>
		public static iFolderSet GetiFoldersByName(iFolderType type, SearchOperation operation, string pattern, int index, int max, string accessID)
		{

			Store store = Store.GetStore();

			// admin ID
			Domain domain = store.GetDomain(store.DefaultDomain);
			String adminID = domain.Owner.UserID;

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

			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in collections)
			{
				// throw away non-collections
				if (sn.IsBaseType(NodeTypes.CollectionType))
				{
					Collection c = store.GetCollectionByID(sn.ID);

					if (((c != null) && (c.IsType(iFolderCollectionType)))
						&& ((type != iFolderType.Orphaned) || (c.Owner.UserID == adminID)))
					{
						if ((i >= index) && (((max <= 0) || i < (max + index))))
						{
							list.Add(new iFolder(c, accessID));
						}

						++i;
					}
				}
			}

			return new iFolderSet((iFolder[])list.ToArray(typeof(iFolder)), i);
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
			
			if (c == null) throw new iFolderDoesNotExistException(ifolderID);

			// impersonate
			Impersonate(c, accessID);

			if ((description != null) && (description.Length > 0))
			{
				c.Properties.ModifyProperty(PropertyTags.Description, description);
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
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="publish">The new published state.</param>
		/// <param name="accessID">The access id.</param>
		/// <remarks>Only the owner of the iFolder can publish it.</remarks>
		public static void PublishiFolder(string ifolderID, bool publish, string accessID)
		{
			Store store = Store.GetStore();
			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null) throw new iFolderDoesNotExistException(ifolderID);
			
			if (c.Owner.UserID != accessID)
				throw new AccessException(c, null, Access.Rights.Admin, "Only the owner can publish an iFolder.");
			
			if (publish)
			{
				Property prop = new Property(PropertyTags.Published, true);
				c.Properties.ModifyProperty(prop);
			}
			else
			{
				c.Properties.DeleteSingleProperty(PropertyTags.Published);
			}
				
			c.Commit(c);
		}
	}
}
