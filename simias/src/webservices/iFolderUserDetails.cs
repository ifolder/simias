/***********************************************************************
 *  $RCSfile: iFolderUserDetails.cs,v $
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
	/// An iFolder User Details
	/// </summary>
	[Serializable]
	public class iFolderUserDetails : iFolderUser
	{
		/// <summary>
		/// The User Effective Sync Interval in the Current iFolder
		/// </summary>
		public int SyncIntervalEffective;

		/// <summary>
		/// The Last Login (Authentication) by the User
		/// </summary>
		public DateTime LastLogin;

		/// <summary>
		/// Specifies the ldap context for the user. If the user
		/// does not exist in an ldap directory, this member will
		/// be an empty string.
		/// </summary>
		public string LdapContext;

		/// <summary>
		/// Number of iFolders that the user owns.
		/// </summary>
		public int OwnediFolderCount = 0;

		/// <summary>
		/// Number of iFolders shared with the user.
		/// </summary>
		public int SharediFolderCount = 0;


		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderUserDetails()
		{
		}

		/// <summary>
		/// Get an iFolder User Details Object
		/// </summary>
		/// <param name="member">The Member Object</param>
		/// <param name="collection">The Collection Object</param>
		/// <param name="domain">The Domain Object</param>
		/// <returns>An iFolderUser Object</returns>
		protected iFolderUserDetails(Member member, Collection collection, Domain domain)
			: base(member, collection, domain)
		{
			// sync interval
			this.SyncIntervalEffective = Simias.Policy.SyncInterval.Get(member, collection).Interval;
	
			// last login
			Member domainMember = domain.GetMemberByID(this.ID);
			Property p = domainMember.Properties.GetSingleProperty(PropertyTags.LastLoginTime);

			if (p != null)
			{
				this.LastLogin = (DateTime)p.Value;
			}

			// Get the DN property for the member if it exists.
			Property property = domainMember.Properties.GetSingleProperty( "DN" );
			this.LdapContext = ( property != null ) ? property.ToString() : String.Empty;

			// Get the number of iFolders owned and shared by the user.
			Store store = Store.GetStore();
			ICSList ifList = store.GetCollectionsByUser(this.ID);
			foreach ( ShallowNode sn in ifList )
			{
				Collection c = new Collection( store, sn );
				if ( c.IsType( "iFolder" ) )
				{
					if ( c.Owner.UserID == this.ID )
					{
						++OwnediFolderCount;
					}
					else
					{
						++SharediFolderCount;
					}
				}
			}
		}


		/// <summary>
		/// Get User Details of a User of the iFolder System
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <returns>An iFolderUserDetails Object</returns>
		public static iFolderUserDetails GetDetails(string userID)
		{
			return GetDetails(userID, null);
		}

		/// <summary>
		/// Get User Details of a Member of an iFolder
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <returns>An iFolderUserDetails Object</returns>
		public static iFolderUserDetails GetDetails(string userID, string ifolderID)
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
			
			Member member = c.GetMemberByID(userID);

			if (member == null) throw new UserDoesNotExistException(userID);

			// user
			return new iFolderUserDetails(member, c, domain);
		}
	}
}
