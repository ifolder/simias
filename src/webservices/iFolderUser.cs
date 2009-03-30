/*****************************************************************************
* Copyright © [2007-08] Unpublished Work of Novell, Inc. All Rights Reserved.
*
* THIS IS AN UNPUBLISHED WORK OF NOVELL, INC.  IT CONTAINS NOVELL'S CONFIDENTIAL, 
* PROPRIETARY, AND TRADE SECRET INFORMATION.	NOVELL RESTRICTS THIS WORK TO 
* NOVELL EMPLOYEES WHO NEED THE WORK TO PERFORM THEIR ASSIGNMENTS AND TO 
* THIRD PARTIES AUTHORIZED BY NOVELL IN WRITING.  THIS WORK MAY NOT BE USED, 
* COPIED, DISTRIBUTED, DISCLOSED, ADAPTED, PERFORMED, DISPLAYED, COLLECTED,
* COMPILED, OR LINKED WITHOUT NOVELL'S PRIOR WRITTEN CONSENT.  USE OR 
* EXPLOITATION OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE 
* PERPETRATOR TO CRIMINAL AND  CIVIL LIABILITY.
*
* Novell is the copyright owner of this file.  Novell may have released an earlier version of this
* file, also owned by Novell, under the GNU General Public License version 2 as part of Novell's 
* iFolder Project; however, Novell is not releasing this file under the GPL.
*
*-----------------------------------------------------------------------------
*
*                 Novell iFolder Enterprise
*
*-----------------------------------------------------------------------------
*
*                 $Author: Rob
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
using System;
using System.Collections;
using System.Security.Cryptography;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Web;
using Simias.Authentication;
using Simias.Server;

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

		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(iFolderUser));

		/// <summary>
		/// encryption options.
		/// </summary>
		public enum Encryption
		{
			None = 0,
			Encrypt = 1,
			EnforceEncrypt = 2,
			SSL = 4,
			EnforceSSL = 8
		}

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
		/// The User HomeServer Name
		/// </summary>
	        public string NewHomeServer;

		/// <summary>
		/// Percentage Data Move details.
		/// </summary>
	        public int DataMovePercentage;

		/// <summary>
		/// Percentage Data Move details.
		/// </summary>
	        public string DataMoveStatus;

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
			DataMovePercentage = 0;
			DataMoveStatus = String.Empty;
			NewHomeServer = String.Empty;
		}

		/// <summary>
		/// Get an iFolder User Information Object, gets called only during user move object construction.
		/// </summary>
		/// <param name="member">The Member Object</param>
		/// <param name="collection">The Collection Object</param>
		/// <param name="domain">The Domain Object</param>
		/// <param name="DataMoveProp">Data move percentage calculation property</param>
		/// <returns>An iFolderUser Object</returns>
		protected iFolderUser(Member member, Collection collection, Domain domain, bool DataMoveProp)
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
			if(DataMoveProp == true)
			{
				int state = member.UserMoveState;
				switch(state)
				{
					case (int)Member.userMoveStates.Nousermove:
					case (int)Member.userMoveStates.Initialized:
						DataMoveStatus = "Initializing";
						DataMovePercentage = 0;
					break;
					case (int)Member.userMoveStates.UserDisabled:
						DataMoveStatus = "Initialized";
						DataMovePercentage = 5;
					break;
					case (int)Member.userMoveStates.DataMoveStarted:
						DataMoveStatus = "Moving iFolders";
						DataMovePercentage = 10;
					break;
					case (int)Member.userMoveStates.Reprovision:
						DataMoveStatus = "Resetting Home";
						DataMovePercentage = 10;
					break;
					case (int)Member.userMoveStates.MoveCompleted:
						DataMoveStatus = "Finalizing";
						DataMovePercentage = 15;
					break; 
					default:
						DataMovePercentage = 0;
						DataMoveStatus = "Initializing";
					break;
				}
				if( state < (int)Member.userMoveStates.DataMoveStarted)
					DataMovePercentage += 0;
				else if( state > (int)Member.userMoveStates.DataMoveStarted)
					DataMovePercentage += 80;
				else
				{
					Store store = Store.GetStore();
					long SpaceUsed = 0;
					long DataTransferred  = 1;
					int iFolderMoveState = 0;
                        		ICSList collectionList = store.GetCollectionsByOwner( member.UserID, domain.ID );
                        		foreach ( ShallowNode sn in collectionList )
                        		{
                                		Collection iFolderCol = new Collection( store, sn );
						iFolderMoveState = member.iFolderMoveState(domain.ID, false, iFolderCol.ID, 0, 0);
						if(iFolderMoveState  > 1 )
						{
							DataTransferred += member.MovediFolderSize(domain.ID, iFolderCol.ID);
							SpaceUsed += member.MovediFolderSize(domain.ID, iFolderCol.ID);
						}
						else
							SpaceUsed += iFolderCol.StorageSize;
                        		}
					if(SpaceUsed != 0)
						DataMovePercentage += (int)(( 80 * DataTransferred ) / SpaceUsed );
					else
						DataMovePercentage += 80;
						
				}
				if ( member.NewHomeServer != null )
				{
					HostNode newHomeNode = HostNode.GetHostByID(domain.ID, member.NewHomeServer);
					if(newHomeNode != null)
                            			this.NewHomeServer = (newHomeNode.Name == null ) ? string.Empty : newHomeNode.Name;
					else
						this.NewHomeServer = string.Empty;
				}
                        	else
                            	this.NewHomeServer = string.Empty;
			}
			else
			{ 
                        	DataMovePercentage = 0;
                        	DataMoveStatus = String.Empty;
                        	NewHomeServer = String.Empty;
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
			log.Debug("Domain ID {0} {1}", c.ID, accessID);

			// impersonate
			iFolder.Impersonate(c, accessID);

			// members
			ICSList members = c.GetMemberList();
			log.Debug("member {0}", members!=null?"not null":"null");
			
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
						Member tmpmember = domain.GetMemberByID(member.UserID);
						if(tmpmember!= null)
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
			ArrayList list = new ArrayList();
			int i = 0;

			if(property == SearchProperty.HomeServerName)
			{
				int MaxiFolderServer = 100000;
				iFolderServerSet iFSet = iFolderServer.GetServersByName(iFolderServerType.All, operation, pattern, 0, MaxiFolderServer);
				foreach(iFolderServer ifServer in iFSet.Items)
				{
					// create the search list
					searchProperty = PropertyTags.HostID;
					searchOperation = SearchOp.Equal;
					ICSList searchList = domain.Search(searchProperty, ifServer.ID, searchOperation);
			
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
				}
			}
			else
			{
				// create the search list
				ICSList searchList = domain.Search(searchProperty, pattern, searchOperation);
				Member member = null;
			
				foreach(ShallowNode sn in searchList)
				{
				  try
				  {
					if (sn.IsBaseType(NodeTypes.MemberType))
					{
						member = new Member(domain, sn);

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
				  catch {
					log.Debug("Member information failed {0}", member.FN);
					}
				}
			}

			return new iFolderUserSet((iFolderUser[])list.ToArray(typeof(iFolderUser)), i);
		}


		/// <summary>
		/// Get Users by Search, gets only users that are getting moved.
		/// </summary>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolder User Set</returns>
		public static iFolderUserSet GetReprovisionUsers(int index, int max, string accessID)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);

			// search operator
			SearchOp searchOperation = SearchOp.Greater_Equal;
			string searchProperty = PropertyTags.UserMoveState;

			// impersonate
			iFolder.Impersonate(domain, accessID);

			// create the search list
			ICSList searchList = domain.Search(searchProperty, 0, searchOperation);
			
			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in searchList)
			{
				if (sn.IsBaseType(NodeTypes.MemberType))
				{
					Member member = new Member(domain, sn);

					log.Debug("GetReprovisionUsers: user {0} State {1}", member.FN, member.UserMoveState.ToString() );
					// Don't include Host objects as iFolder users.
					if (!member.IsType("Host") && member.UserMoveState >= 0)
					{
						if ((i >= index) && (((max <= 0) || i < (max + index))))
						{
							list.Add(new iFolderUser(member, domain, domain, true));
						}

						++i;
					}
				}
			}

			return new iFolderUserSet((iFolderUser[])list.ToArray(typeof(iFolderUser)), i);
		}

		/// <summary>
		/// Provision Users to different servers given in the Hashtable
		/// </summary>
		/// <param name="ServerProvisioningNames">Hashtable containing userids and servernames </param>
		/// <param name="ServerNames">A string array containing server names</param>
		/// <param name="UserIDs">A string array containing corresponding userids</param>
		public static void ProvisionUsersToServers(string [] ServerNames, string [] UserIDs)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			int index = 0;
			try
			{
				if( ServerNames.Length <= 0 || UserIDs.Length <=0 )
					throw new InvalidOperationException("NOESERVER");
				for (index = 0 ; index < ServerNames.Length ; index++)
				{
					Simias.Host.HostInfo ServerInfo = ManualProvisionUserProvider.ProvisionUser(UserIDs[index], ServerNames[index]);
				}	
			}
			catch (Exception ex)
			{
				throw ex;	
			}
			
		}

		/// <summary>
		/// Provision Users to one server 
		/// </summary>
		/// <param name="ServerName">A string containing server's name. </param>
		/// <param name="ListOfUsers">An array of string containing userIDs </param>
		public static void ProvisionUsersToServer(string ServerName, string [] UserIDs)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			int index ; 
			iFolderServer serverobject = iFolderServer.GetServerByName(ServerName);
			try
			{
				if( UserIDs.Length <= 0)
					throw new InvalidOperationException("NOESERVER");
				for (index = 0 ; index < UserIDs.Length ; index++)
				{
					Simias.Host.HostInfo ServerInfo = ManualProvisionUserProvider.ProvisionUser(UserIDs[index], ServerName);
				}
			}
			catch (Exception ex)
			{
				throw ex; 
			}	
		}

		/// <summary>
		/// Provision Users to one server 
		/// </summary>
		/// <param name="ServerName">A string containing server's name. </param>
		/// <param name="UserID">string containing userID </param>
		public static void ReProvisionUsersToServer(string ServerName, string UserID)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			int index ; 
			iFolderServer serverobject = iFolderServer.GetServerByName(ServerName);
			log.Debug("ReProvisionUsersToServer: --{0}--{1}", UserID, ServerName );
			if(UserID == null || UserID == String.Empty || UserID == "" || ServerName == null || ServerName == String.Empty || ServerName == "")
				return;
			try
			{
				Simias.Host.HostInfo ServerInfo = ManualProvisionUserProvider.ProvisionUser(UserID, ServerName);
			}
			catch (Exception ex)
			{
				throw ex; 
			}	
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
				if (e.Message.IndexOf("group has ReadOnly rights") != -1)
				{
					throw new InvalidOperationException("Group user with readonly rights can not remove its membership", e);
				}

				throw ;
			}
		}

		/// <summary>
		/// Get Group IDs of this member 
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <param name="accessID">The Access User ID</param>
		public static string [] GetGroupIDs(string userID)
		{
			string [] GroupIDs = null;
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			try
			{
				GroupIDs = domain.GetMemberFamilyList(userID);
			}
			catch(Exception e)
			{
				throw;
			}
			return GroupIDs;
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

			//If orphaned collection was adopted then delete the 'OrphanedOwner' property
			// moving this code before changing the owner , as just after changing the owner event, catalog thread tries
			// to access the orphanedowner property before it is removed from the collection
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
		///checks whether encryption is enforced for this user or not, if no user level policy exists , then check for system level enforcement
		///</summary>
		///<returns>true/false</returns>
		public static bool IsUserOrSystemEncryptionEnforced(string userID)
		{
			UserPolicy UPolicy = null;
			SystemPolicy SysPolicy = null;
			try
			{
				
				UPolicy = UserPolicy.GetPolicy( userID );
				SysPolicy = SystemPolicy.GetPolicy();
				int usersecurityStatus = UPolicy.EncryptionStatus;
				int groupsecurityStatus = UserPolicy.GetUserGroupEncryptionPolicy(userID);
				int systemsecurityStatus = SysPolicy.EncryptionStatus;
				int userstatus = usersecurityStatus & (int) Encryption.EnforceEncrypt;
				int groupstatus = groupsecurityStatus & (int) Encryption.EnforceEncrypt;
				int systemstatus = systemsecurityStatus & (int) Encryption.EnforceEncrypt;
		
				// if userstatus is 0 , it means no encryption policy change on user level , so follow system.
				if(usersecurityStatus == 0)
				{
					if(systemsecurityStatus != 0)
					{
						// check if encryption is enforced on system level
						if( systemstatus == (int)Encryption.EnforceEncrypt)
						{
							return true;
						}
					}
					else if(groupsecurityStatus != 0)
					{
						// check if encryption is enforced on group level
						if( groupstatus == (int)Encryption.EnforceEncrypt)
						{
							return true;
						}
					}
				}
				else
				{
					// if userstatus is there , it means on user level some change happened so priority will be reversed
					if( userstatus == (int)Encryption.EnforceEncrypt )
					{
						return true;
					}
					else if(groupstatus == (int)Encryption.EnforceEncrypt)
					{
						return true;
					}
					else if( (int)(usersecurityStatus & (int)Encryption.Encrypt)  == (int)Encryption.Encrypt || (int)(groupsecurityStatus & (int)Encryption.Encrypt) == (int)Encryption.Encrypt )
					{
					log.Debug("on user level , no enforce encrypt but encryption policy is there so returning false" );
						return false;
					}
					else if(systemstatus == (int)Encryption.EnforceEncrypt)
					{
						return true;
					} 
				}
			}
			catch(Exception ex)
			{
				throw ex;
			}
			return false;
		}

		///<summary>
		///</summary>
		///<returns></returns>
		public static bool IsPassPhraseSet (string DomainID, string AccessID)
		{
			bool isset = false;
			try
			{
				Store store = Store.GetStore();
							
				Collection collection = store.GetCollectionByID(DomainID);
				Simias.Storage.Member member = collection.GetMemberByID(AccessID);
				
				isset = member.PassPhraseSetStatus();
			}
			catch(Exception ex)
			{
				log.Debug("IsPassPhraseSet : {0}", ex.Message);
				throw ex;
			}
			log.Debug("IsPassPhraseSet :{0}", isset.ToString());
			return isset;
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
				if(RAPublicKey != null && RAPublicKey != "" && RAName != null && RAName != "")
				{
					byte [] key = Convert.FromBase64String(RAPublicKey);
					if(key.Length > 64 && key.Length < 128) //remove the 5 byte header and 5 byte trailer
					{
						byte[] NewKey = new byte[key.Length-10];
						Array.Copy(key, 5, NewKey, 0, key.Length-10);
						RAPublicKey = Convert.ToBase64String(NewKey);
					}
					else if(key.Length > 128 && key.Length < 256) //remove the 7 byte header and 5 byte trailer
					{
						byte[] NewKey = new byte[key.Length-12];
						Array.Copy(key, 7, NewKey, 0, key.Length-12);
						RAPublicKey = Convert.ToBase64String(NewKey);
					}					
					else if(key.Length > 256) //remove the 9 byte header and 5 byte trailer
					{
						byte[] NewKey = new byte[key.Length-14];
						Array.Copy(key, 9, NewKey, 0, key.Length-14);
						RAPublicKey = Convert.ToBase64String(NewKey);
					}					
					else
					{
						log.Debug("KeyCorrection RAName: {0}", RAName);
						log.Debug("KeyCorrection RAPublicKey: {0}", RAPublicKey);
						log.Debug("KeyCorrection key.Length: {0}", key.Length);
						throw new SimiasException("Recovery key size not suported");
					}
				}
		
		
			
				Store store = Store.GetStore();
								
				Collection collection = store.GetCollectionByID(DomainID);
				Simias.Storage.Member member = collection.GetMemberByID(AccessID);

				//Hash the passphrase and use it for encryption and decryption
				PassphraseHash hash = new PassphraseHash();
				byte[] passphrase = hash.HashPassPhrase(Passphrase);	
				
				Key RAkey = new Key((passphrase.Length)*8);
				string EncrypCryptoKey;
				RAkey.EncrypytKey(passphrase, out EncrypCryptoKey);
				Key HashKey = new Key(EncrypCryptoKey);
				
				log.Debug("SetPassPhrase {0}...{1}...{2}...{3}",EncrypCryptoKey, HashKey.HashKey(), RAName, RAPublicKey);
				member.ServerSetPassPhrase(EncrypCryptoKey, HashKey.HashKey(), RAName, RAPublicKey);
				if(HashKey.HashKey() != null)
				{
					member.SetEncryptionBlobFlagServer(store, member.UserID);
				}
			}
			catch(Exception ex)
			{
				log.Debug("SetPassPhrase : {0}", ex.Message);
				//throw ex;
			}
		}		
	}
}
