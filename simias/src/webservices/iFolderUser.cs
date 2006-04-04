/***********************************************************************
 *  $RCSfile: iFolderUser.cs,v $
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

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Web;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder User
	/// </summary>
	[Serializable]
	public class iFolderUser
	{
		/// <summary>
		/// The User ID
		/// </summary>
		public string ID;

		/// <summary>
		/// The User Name
		/// </summary>
		public string UserName;

		/// <summary>
		/// The User Preferred Full Name
		/// </summary>
		public string FullName;

		/// <summary>
		/// The User Rights in the iFolder/Domain
		/// </summary>
		public Simias.Storage.Access.Rights Rights;

		/// <summary>
		/// Is the User's Login Enabled
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// Is the User the Owner in the iFolder/Domain
		/// </summary>
		public bool IsOwner;

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderUser()
		{
		}

		/// <summary>
		/// Get an iFolder User Information Object.
		/// </summary>
		/// <param name="member">The Member Object</param>
		/// <param name="collection">The Collection Object</param>
		/// <param name="domain">The Domain Object</param>
		/// <returns>An iFolderUser Object</returns>
		protected iFolderUser(Member member, Collection collection, Domain domain)
		{
			this.ID = member.UserID;
			this.UserName = member.Name;
            this.Rights = member.Rights;
			this.FullName = (member.FN != null) ? member.FN : member.Name;
			this.Enabled = !(domain.IsLoginDisabled(this.ID));
			this.IsOwner = (member.UserID == collection.Owner.UserID);

			// NOTE: The member object may not be complete if it did not come from the
			// domain object.
			if (collection != domain)
			{
				Member domainMember = domain.GetMemberByID(this.ID);
				this.FullName = domainMember.FN;
			}
		}

		/// <summary>
		/// Get a Member of an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The User ID</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolderUser Object</returns>
		public static iFolderUser GetUser(string ifolderID, string userID, string accessID)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);

			Collection c = null;

			if (ifolderID == null)
			{
				// default to the domain
				c = domain;
			}
			else
			{
				// get the collection
				c = store.GetCollectionByID(ifolderID);

				if (c == null) throw new iFolderDoesNotExistException(ifolderID);
			}
			
			// impersonate
			iFolder.Impersonate(c, accessID);

			Member member = c.GetMemberByID(userID);

			if (member == null) throw new UserDoesNotExistException(userID);

			// user
			return new iFolderUser(member, c, domain);
		}

		/// <summary>
		/// Get the Members of an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An Array of iFolderUser Objects</returns>
		public static iFolderUser[] GetUsers(string ifolderID, int index, int count, out int total, string accessID)
		{
			Store store = Store.GetStore();

            Domain domain = store.GetDomain(store.DefaultDomain);

            Collection c = null;

			if (ifolderID == null)
			{
				// default to the domain
				c = domain;
			}
			else
			{
				// get the collection
				c = store.GetCollectionByID(ifolderID);

				if (c == null) throw new iFolderDoesNotExistException(ifolderID);
			}

			// impersonate
			iFolder.Impersonate(c, accessID);

			// members
			ICSList members = c.GetMemberList();
			
			// sort the list
			ArrayList sortList = new ArrayList();
			
			foreach(ShallowNode sn in members)
			{
				sortList.Add(sn);
			}
			
			sortList.Sort();

			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in sortList)
			{
				Member member = new Member(c, sn);

				if ((i >= index) && (((count <= 0) || i < (count + index))))
				{
					list.Add(new iFolderUser(member, c, domain));
				}

				++i;
			}

			// save total
			total = i;

			return (iFolderUser[])list.ToArray(typeof(iFolderUser));
		}

		/// <summary>
		/// Get Users by Search
		/// </summary>
		/// <param name="property">The Search Property</param>
		/// <param name="operation">The Search Operator</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An Array of iFolderUser Objects</returns>
		public static iFolderUser[] GetUsers(SearchProperty property, SearchOperation operation, string pattern, int index, int count, out int total, string accessID)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);

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
			
			// search property
			string searchProperty;

			switch(property)
			{
				case SearchProperty.UserName:
					searchProperty = BaseSchema.ObjectName;
					break;

				case SearchProperty.Name:
					searchProperty = PropertyTags.FullName;
					break;

				case SearchProperty.LastName:
					searchProperty = PropertyTags.Family;
					break;

				case SearchProperty.FirstName:
					searchProperty = PropertyTags.Given;
					break;

				default:
					searchProperty = PropertyTags.FullName;
					break;
			}
			
			// impersonate
			iFolder.Impersonate(domain, accessID);

			// create the search list
			ICSList searchList = domain.Search(searchProperty, pattern, searchOperation);
			
			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in searchList)
			{
				if (sn.IsBaseType(NodeTypes.MemberType))
				{
					if ((i >= index) && (((count <= 0) || i < (count + index))))
					{
						list.Add(new iFolderUser(new Member(domain, sn), domain, domain));
					}

					++i;
				}
			}

			// save total
			total = i;

			return (iFolderUser[])list.ToArray(typeof(iFolderUser));
		}

		/// <summary>
		/// Set the iFolder Rights of a Member
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The Member User ID</param>
		/// <param name="rights">The New Rights</param>
		/// <param name="accessID">The Access User ID</param>
		public static void SetMemberRights(string ifolderID, string userID, Simias.Storage.Access.Rights rights, string accessID)
		{
			SharedCollection.SetMemberRights(ifolderID, userID, rights.ToString(), accessID);
		}

		/// <summary>
		/// Add a Member to an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The User ID</param>
		/// <param name="rights">The New Rights</param>
		/// <param name="accessID">The Access User ID</param>
		public static void AddMember(string ifolderID, string userID, Simias.Storage.Access.Rights rights, string accessID)
		{
			try
			{
				SharedCollection.AddMember(ifolderID, userID, rights.ToString(), iFolder.iFolderCollectionType, accessID);
			}
			catch(ExistsException)
			{
				// ignore an already exists exception
			}
		}

		/// <summary>
		/// Remove a Member from an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The User ID</param>
		/// <param name="accessID">The Access User ID</param>
		public static void RemoveMember(string ifolderID, string userID, string accessID)
		{
			SharedCollection.RemoveMember(ifolderID, userID, accessID);
		}

		/// <summary>
		/// Se the Owner of an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The User ID</param>
		/// <param name="accessID">The Access User ID</param>
		public static void SetOwner(string ifolderID, string userID, string accessID)
		{
			try
			{
				// check that the member exists
				GetUser(ifolderID, userID, accessID);
			}
			catch
			{
				// member does not exist
				AddMember(ifolderID, userID, Access.Rights.Admin, accessID);
			}

			// note: default the previous owner to "ReadOnly" rights
			SharedCollection.ChangeOwner(ifolderID, userID, Access.Rights.ReadOnly.ToString(), accessID);
		}

		/// <summary>
		/// Is the User an Administrator
		/// </summary>
		/// <remarks>
		/// A User is a system administrator if the user has "Admin" rights in the domain.
		/// </remarks>
		/// <param name="userID">The User ID</param>
		/// <returns>true, if the User is a System Administrator</returns>
		public static bool IsAdministrator(string userID)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID( userID );
			Access.Rights rights = (member != null) ? member.Rights : Access.Rights.Deny;
			
			return (rights == Access.Rights.Admin);
		}

		/// <summary>
		/// Give a User Administration Rights
		/// </summary>
		/// <remarks>
		/// A User is a system administrator if the user has "Admin" rights in the domain.
		/// </remarks>
		/// <param name="userID">The User ID</param>
		public static void AddAdministrator(string userID)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);

			SetMemberRights(domain.ID, userID, Access.Rights.Admin, null);
		}

		/// <summary>
		/// Remove Administration Rights from a User
		/// </summary>
		/// <remarks>
		/// Administration rights are removed by giving the user "ReadOnly" rights in the domain.
		/// </remarks>
		/// <param name="userID">The User ID</param>
		public static void RemoveAdministrator(string userID)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);

			SetMemberRights(domain.ID, userID, Access.Rights.ReadOnly, null);
		}

		/// <summary>
		/// Get the Administrators
		/// </summary>
		/// <param name="index">The Search Start Index</param>
		/// <param name="count">The Search Max Count of Results</param>
		/// <param name="total">The Total Number of Results</param>
		/// <remarks>
		/// A User is a system administrator if the user has "Admin" rights in the domain.
		/// </remarks>
		/// <returns>An Array of iFolderUser Objects</returns>
		public static iFolderUser[] GetAdministrators(int index, int count, out int total)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);

			ICSList members = domain.GetMembersByRights(Access.Rights.Admin);
			
			// sort the list
			ArrayList sortList = new ArrayList();
			
			foreach(ShallowNode sn in members)
			{
				sortList.Add(sn);
			}
			
			sortList.Sort();

			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in sortList)
			{
				if ((i >= index) && (((count <= 0) || i < (count + index))))
				{
					Member member = new Member(domain, sn);

					list.Add(new iFolderUser(member, domain, domain));
				}

				++i;
			}

			// save total
			total = i;

			return (iFolderUser[])list.ToArray(typeof(iFolderUser));
		}
	}
}
