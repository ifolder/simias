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
using Simias.Authentication;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder User Result Set
	/// </summary>
	[Serializable]
	public class iFolderUserSet
	{
		/// <summary>
		/// An Array of iFolder Users
		/// </summary>
		public iFolderUser[] Items;

		/// <summary>
		/// The Total Number of iFolder Users
		/// </summary>
		public int Total;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public iFolderUserSet()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="items"></param>
		/// <param name="total"></param>
		public iFolderUserSet(iFolderUser[] items, int total)
		{
			this.Items = items;
			this.Total = total;
		}
	}

	/// <summary>
	/// An iFolder User
	/// </summary>
	[Serializable]
	public class iFolderUser
	{
		/// <summary>
		/// Email Property Name
		/// </summary>
		private static string EmailProperty = "Email";

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
		/// The User First Name
		/// </summary>
		public string FirstName;

		/// <summary>
		/// The User Last Name
		/// </summary>
		public string LastName;
		/// <summary>
		/// The User Rights in the iFolder/Domain
		/// </summary>
		public Rights MemberRights;

		/// <summary>
		/// Is the User's Login Enabled
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// Is the User the Owner in the iFolder/Domain
		/// </summary>
		public bool IsOwner;

		/// <summary>
		/// The User Email Address
		/// </summary>
		public string Email;

		/// <summary>
		/// The User HomeServer Name
		/// </summary>
	        public string HomeServer;

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
            this.MemberRights = RightsUtility.Convert(member.Rights);
			this.FullName = (member.FN != null) ? member.FN : member.Name;
			this.FirstName = member.Given;
			this.LastName = member.Family;
			this.Enabled = !(domain.IsLoginDisabled(this.ID));
			this.IsOwner = (member.UserID == collection.Owner.UserID);
			this.Email = NodeUtility.GetStringProperty(member, EmailProperty);

			if ( member.HomeServer != null )
			    this.HomeServer = (member.HomeServer.Name == null ) ? string.Empty : member.HomeServer.Name;
			else 
			    this.HomeServer = string.Empty;

			// NOTE: The member object may not be complete if it did not come from the
			// domain object.
			if (collection != domain)
			{
				Member domainMember = domain.GetMemberByID(this.ID);
				this.FullName = (domainMember.FN != null) ? domainMember.FN : domainMember.Name;
				this.FirstName = domainMember.Given;
				this.LastName = domainMember.Family;
				this.Email = NodeUtility.GetStringProperty(member, EmailProperty);
			}
		}

		/// <summary>
		/// Update a User.
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <param name="user">The iFolderUser object with updated fields.</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An updated iFolderUser Object</returns>
		public static iFolderUser SetUser(string userID, iFolderUser user, string accessID)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);

			// impersonate
			iFolder.Impersonate(domain, accessID);

			Member member = domain.GetMemberByID(userID);

			// check username also
			if (member == null) member = domain.GetMemberByName(userID);

			// not found
			if (member == null) throw new UserDoesNotExistException(userID);

			// update values
			member.FN = user.FullName;
			member.Given = user.FirstName;
			member.Family = user.LastName;
			member.Properties.ModifyProperty(EmailProperty, user.Email);

			// no full name policy
			if (((user.FullName == null) || (user.FullName.Length == 0))
				&& ((user.FirstName != null) && (user.FirstName.Length != 0))
				&& ((user.LastName != null) && (user.LastName.Length != 0)))
			{
				member.FN = String.Format("{0} {1}", user.FirstName, user.LastName);
			}

			// commit
			domain.Commit(member);

			return GetUser(userID, accessID);
		}

		/// <summary>
		/// Get a Member of an iFolder
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolderUser Object</returns>
		public static iFolderUser GetUser(string userID, string accessID)
		{
			return GetUser(null, userID, accessID);
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

			// check username also
			if (member == null) member = c.GetMemberByName(userID);

			// not found
			if (member == null) throw new UserDoesNotExistException(userID);

			// user
			return new iFolderUser(member, c, domain);
		}

		/// <summary>
		/// Get the Members of an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolder User Set</returns>
		public static iFolderUserSet GetUsers(string ifolderID, int index, int max, string accessID)
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

				// Don't include the Host objects as iFolder users.
				if (!member.IsType("Host"))
				{
					if ((i >= index) && (((max <= 0) || i < (max + index))))
					{
						list.Add(new iFolderUser(member, c, domain));
					}

					++i;
				}
			}

			return new iFolderUserSet((iFolderUser[])list.ToArray(typeof(iFolderUser)), i);
		}

		/// <summary>
		/// Get Users by Search
		/// </summary>
		/// <param name="property">The Search Property</param>
		/// <param name="operation">The Search Operator</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolder User Set</returns>
		public static iFolderUserSet GetUsers(SearchProperty property, SearchOperation operation, string pattern, int index, int max, string accessID)
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

				case SearchProperty.FullName:
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
					Member member = new Member(domain, sn);

					// Don't include Host objects as iFolder users.
					if (!member.IsType("Host"))
					{
						if ((i >= index) && (((max <= 0) || i < (max + index))))
						{
							list.Add(new iFolderUser(member, domain, domain));
						}

						++i;
					}
				}
			}

			return new iFolderUserSet((iFolderUser[])list.ToArray(typeof(iFolderUser)), i);
		}

		/// <summary>
		/// Set the iFolder Rights of a Member
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The Member User ID</param>
		/// <param name="rights">The New Rights</param>
		/// <param name="accessID">The Access User ID</param>
		public static void SetMemberRights(string ifolderID, string userID, Rights rights, string accessID)
		{
			try
			{
				SharedCollection.SetMemberRights(ifolderID, userID, RightsUtility.Convert(rights).ToString(), accessID);
			}
			catch(CollectionStoreException e)
			{
				if (e.Message.IndexOf("change owner's rights") != -1)
				{
					throw new InvalidOperationException("The rights of the owner of the iFolder can not be changed.", e);
				}

				throw;
			}
		}

		/// <summary>
		/// Add a Member to an iFolder
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="userID">The User ID</param>
		/// <param name="rights">The New Rights</param>
		/// <param name="accessID">The Access User ID</param>
		public static void AddMember(string ifolderID, string userID, Rights rights, string accessID)
		{
			try
			{
				SharedCollection.AddMember(ifolderID, userID, RightsUtility.Convert(rights).ToString(),
					iFolder.iFolderCollectionType, accessID);
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
			try
			{
				SharedCollection.RemoveMember(ifolderID, userID, accessID);
			}
			catch(Exception e)
			{
				// guess and improve exception
				if (e.Message.IndexOf("iFolder owner") != -1)
				{
					throw new InvalidOperationException("The owner of an iFolder can not be removed.", e);
				}

				throw;
			}
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
				AddMember(ifolderID, userID, Rights.Admin, accessID);
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

			SetMemberRights(domain.ID, userID, Rights.Admin, null);
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

			SetMemberRights(domain.ID, userID, Rights.ReadOnly, null);
		}

		/// <summary>
		/// Get the Administrators
		/// </summary>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <remarks>
		/// A User is a system administrator if the user has "Admin" rights in the domain.
		/// </remarks>
		/// <returns>An iFolder User Set</returns>
		public static iFolderUserSet GetAdministrators(int index, int max)
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
				Member member = new Member(domain, sn);

				// Don't include Host objects as iFolder administrators.
				if (!member.IsType("Host"))
				{
					if ((i >= index) && (((max <= 0) || i < (max + index))))
					{
						list.Add(new iFolderUser(member, domain, domain));
					}

					++i;
				}
			}

			return new iFolderUserSet((iFolderUser[])list.ToArray(typeof(iFolderUser)), i);
		}
	


		///<summary>
		///</summary>
		///<returns></returns>
		public static Simias.Authentication.Status IsPassPhraseSet (string domainID)
		{

			//log.Debug( "IsPassPhraseSet - called" );
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(domainID);
			if( domain == null )
			{
			return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
			}

			Simias.Storage.Member member = domain.GetCurrentMember();
			if( member == null )
			{
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownUser );
			}

			//log.Debug( "IsPassPhraseSet User: " + member.Name );

			string EncryptionBlob = member.EncryptionBlob;

			if(EncryptionBlob =="")
			{
				//log.Info( "IsPassPhraseSet : FALSE" );
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.PassPhraseNotSet);
			}
			else
			{
				//log.Info( "IsPassPhraseSet User: TRUE" );
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.Success);
			}
		}
		
		///<summary>
		///Validate the passphrase for the correctness
		///</summary>
		///<returns>passPhrase.</returns>
		public static Simias.Authentication.Status ValidatePassPhrase(string domainID, string passPhrase)
		{
			//log.Debug( "ValidatePassPhrase - called" );
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(domainID);
			if( domain == null )
			{
				//log.Debug( "ValidatePassPhrase domain null" );
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
			}

			Simias.Storage.Member member = domain.GetCurrentMember();
			if( member == null )
			{
				//log.Debug( "ValidatePassPhrase member null" );
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownUser );
			}
			//log.Debug( "SetPassPhrase  User: " + member.Name );

			return member.ValidatePassPhrase(passPhrase);
				//return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.Success );
			//else
				//return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.PassPhraseInvalid);
		}
		
		///<summary>
		///Set the passphrase and recovery agent
		///</summary>
		///<returns>passPhrase.</returns>
		public static Simias.Authentication.Status SetPassPhrase(string domainID, string passPhrase, string recoveryAgentName, string publicKey)
		{
			//log.Debug( "SetPassPhrase - called" );
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(domainID);
			if( domain == null )
			{
				//log.Debug( "domain null" );
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
			}

			Simias.Storage.Member member = domain.GetCurrentMember();
			if( member == null )
			{
				//log.Debug( "member null" );
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownUser );
			}
			//log.Debug( "SetPassPhrase  User: " + member.Name );

			return member.SetPassPhrase("EncryptedCryptoKey", passPhrase, recoveryAgentName, publicKey);
			//	return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.Success);
			//else
			//	return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.PassPhraseNotSet);
		}
		
		///<summary>
		///Reset passphrase and recovery agent
		///</summary>
		///<returns>passPhrase.</returns>
		public static Simias.Authentication.Status ReSetPassPhrase(string domainID, string oldPass, string newPass, string recoveryAgentName, string publicKey)
		{
			//temp
			return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );							
			
			//reset either or both
			/* //log.Debug( "SetPassPhrase - called" );
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(domainID);
			if( domain == null )
			{
				//log.Debug( "domain null" );
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
			}

			Simias.Storage.Member member = domain.GetCurrentMember();
			if( member == null )
			{
				//log.Debug( "member null" );
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownUser );
			}
			//log.Debug( "SetPassPhrase  User: " + member.Name );
			*/
		}
		
		
	}
}
