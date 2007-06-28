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
using System.Security.Cryptography;

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

		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Member));

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
		public static void SetOwner(string ifolderID, string userID, string accessID, bool OrphanAdopt)
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
			
			//If orphaned collection was adopted then delete the 'OrphanedOwner' property
			if(OrphanAdopt)
			{
				Store store = Store.GetStore();
				Collection c = store.GetCollectionByID(ifolderID);
				if (c == null)  throw new iFolderDoesNotExistException(ifolderID);
				Property p = c.Properties.GetSingleProperty( "OrphanedOwner" );
				if ( p != null )
				{
					c.Properties.DeleteSingleProperty( "OrphanedOwner" );
					c.Commit();
				}
			}	
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
		public static bool IsPassPhraseSet (string DomainID, string AccessID)
		{

			string  CryptoKeyBlob = null;
			try
			{
				Store store = Store.GetStore();
							
				Collection collection = store.GetCollectionByID(DomainID);
				Simias.Storage.Member member = collection.GetMemberByID(AccessID);
				
				CryptoKeyBlob =  member.ServerGetPassKeyHash();
			}
			catch(Exception ex)
			{
				log.Debug("IsPassPhraseSet : {0}", ex.Message);
				throw ex;
			}
			log.Debug("IsPassPhraseSet :{0}", CryptoKeyBlob);
			if(CryptoKeyBlob == String.Empty)
			{
				log.Debug("IsPassPhraseSet : false");
				return false;
			}
			else	
			{
				log.Debug("IsPassPhraseSet : true");
				return true;
			}
		}
		
		
		///<summary>
		///Validate the passphrase for the correctness
		///</summary>
		///<returns>passPhrase.</returns>
		public static Simias.Authentication.Status ValidatePassPhrase(string DomainID, string Passphrase, string AccessID)
		{
			string OldHash = null;
			string NewHash = null;
			
			try
			{
				Store store = Store.GetStore();
					
				Collection collection = store.GetCollectionByID(DomainID);
				Simias.Storage.Member member = collection.GetMemberByID(AccessID);
				
				log.Debug("Member ValidatePassPhrase User:{0}...{1} ", member.Name, member.UserID);
				
				log.Debug("ValidatePassPhrase : got PassKey");
				string EncrypCryptoKey = member.ServerGetEncrypPassKey();
				log.Debug("ValidatePassPhrase : got PassKey:{0}",EncrypCryptoKey);

				//Hash the passphrase and use it for encryption and decryption
				PassphraseHash hash = new PassphraseHash();
				byte[] passphrase = hash.HashPassPhrase(Passphrase);	
			
				//Decrypt it
				string DecryptedCryptoKey; 
				Key DeKey = new Key(EncrypCryptoKey);
				DeKey.DecrypytKey(passphrase, out DecryptedCryptoKey);

				//Encrypt using passphrase
				string EncryptedCryptoKey;
				Key EnKey = new Key(DecryptedCryptoKey);
				EnKey.EncrypytKey(passphrase, out EncryptedCryptoKey);

				//SHA1
				Key HashKey = new Key(EncryptedCryptoKey);
				NewHash = HashKey.HashKey();

				OldHash = member.ServerGetPassKeyHash();
				log.Debug("ValidatePassPhrase : getting OldHash:{0}", OldHash);
			}
			catch(Exception ex)
			{
				log.Debug("ValidatePassPhrase : {0}", ex.Message);
				throw ex;
			}
			
			//Compare
			log.Debug("ValidatePassPhrase : Comparing blobs {0}...{1}",OldHash, NewHash);
			if(String.Equals(OldHash, NewHash)==true)
			{
				log.Debug("ValidatePassPhrase : true");
				return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.Success);
			}
			else	
			{
				log.Debug("ValidatePassPhrase : false");			
				return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.PassPhraseInvalid);
			}
		}
		
		///<summary>
		///Set the passphrase and recovery agent
		///</summary>
		///<returns>passPhrase.</returns>
		public static void SetPassPhrase(string DomainID, string Passphrase, string RAName, string RAPublicKey, string AccessID)
		{
			try
			{
				if(RAPublicKey != null)
				{
					byte [] key = Convert.FromBase64String(PublicKey);
					if(key.Length > 64 && key.Length < 128) //remove the 5 byte header and 5 byte trailer
					{
						byte[] NewKey = new byte[key.Length-10];
						Array.Copy(key, 5, NewKey, 0, key.Length-10);
						PublicKey = Convert.ToBase64String(NewKey);
					}
					else if(key.Length > 128 && key.Length < 256) //remove the 7 byte header and 5 byte trailer
					{
						byte[] NewKey = new byte[key.Length-12];
						Array.Copy(key, 7, NewKey, 0, key.Length-12);
						PublicKey = Convert.ToBase64String(NewKey);
					}					
					else if(key.Length > 256) //remove the 9 byte header and 5 byte trailer
					{
						byte[] NewKey = new byte[key.Length-14];
						Array.Copy(key, 9, NewKey, 0, key.Length-14);
						PublicKey = Convert.ToBase64String(NewKey);
					}					
					else
						throw new SimiasException("Recovery key size not suported");				
				}
		
		
			
				Store store = Store.GetStore();
								
				Collection collection = store.GetCollectionByID(DomainID);
				Simias.Storage.Member member = collection.GetMemberByID(AccessID);

				//Hash the passphrase and use it for encryption and decryption
				PassphraseHash hash = new PassphraseHash();
				byte[] passphrase = hash.HashPassPhrase(Passphrase);	
				
				Key RAkey = new Key(128);
				string EncrypCryptoKey;
				RAkey.EncrypytKey(passphrase, out EncrypCryptoKey);
				Key HashKey = new Key(EncrypCryptoKey);
				
				log.Debug("SetPassPhrase {0}...{1}...{2}...{3}",EncrypCryptoKey, HashKey.HashKey(), RAName, RAPublicKey);
				member.ServerSetPassPhrase(EncrypCryptoKey, HashKey.HashKey(), RAName, RAPublicKey);
			}
			catch(Exception ex)
			{
				log.Debug("SetPassPhrase : {0}", ex.Message);
				//throw ex;
			}
		}		
	}
}
