/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
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
	/// Class used to keep track of outstanding searches.
	/// </summary>
	internal class SearchState : IDisposable
	{
		#region Class Members
		/// <summary>
		/// Table used to keep track of outstanding search entries.
		/// </summary>
		static private Hashtable searchTable = new Hashtable();

		/// <summary>
		/// Indicates whether the object has been disposed.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Handle used to store and recall this context object.
		/// </summary>
		private string contextHandle = Guid.NewGuid().ToString();

		/// <summary>
		/// Identifier for the domain that is being searched.
		/// </summary>
		private string domainID;

		/// <summary>
		/// Object used to iteratively return the members from the domain.
		/// </summary>
		private ICSEnumerator enumerator;

		/// <summary>
		/// Total number of records contained in the search.
		/// </summary>
		private int totalRecords;

		/// <summary>
		/// The cursor for the caller.
		/// </summary>
		private int currentRecord = 0;

		/// <summary>
		/// The last count of records returned.
		/// </summary>
		private int previousCount = 0;
		#endregion

		#region Properties
		/// <summary>
		/// Indicates if the object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return disposed; }
		}

		/// <summary>
		/// Gets the context handle for this object.
		/// </summary>
		public string ContextHandle
		{
			get { return contextHandle; }
		}

		/// <summary>
		/// Gets or sets the current record.
		/// </summary>
		public int CurrentRecord
		{
			get { return currentRecord; }
			set { currentRecord = value; }
		}

		/// <summary>
		/// Gets the domain ID for the domain that is being searched.
		/// </summary>
		public string DomainID
		{
			get { return domainID; }
		}

		/// <summary>
		/// Gets or sets the last record count.
		/// </summary>
		public int LastCount
		{
			get { return previousCount; }
			set { previousCount = value; }
		}

		/// <summary>
		/// Gets the search iterator.
		/// </summary>
		public ICSEnumerator Enumerator
		{
			get { return enumerator; }
		}

		/// <summary>
		/// Gets the total number of records contained by this search.
		/// </summary>
		public int TotalRecords
		{
			get { return totalRecords; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of an object.
		/// </summary>
		/// <param name="domainID">Identifier for the domain that is being searched.</param>
		/// <param name="enumerator">Search iterator.</param>
		/// <param name="totalRecords">The total number of records contained in the search.</param>
		public SearchState( string domainID, ICSEnumerator enumerator, int totalRecords )
		{
			this.domainID = domainID;
			this.enumerator = enumerator;
			this.totalRecords = totalRecords;

			lock ( searchTable )
			{
				searchTable.Add( contextHandle, this );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Removes this SearchState object from the search table.
		/// </summary>
		private void RemoveSearchState()
		{
			lock ( searchTable )
			{
				// Remove the search context from the table and dispose it.
				searchTable.Remove( contextHandle );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns a search context object that contains the state information for an outstanding search.
		/// </summary>
		/// <param name="contextHandle">Context handle that refers to a specific search context object.</param>
		/// <returns>A SearchState object if a valid one exists, otherwise a null is returned.</returns>
		static public SearchState GetSearchState( string contextHandle )
		{
			lock ( searchTable )
			{
				return searchTable[ contextHandle ] as SearchState;
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Allows for quick release of managed and unmanaged resources.
		/// Called by applications.
		/// </summary>
		public void Dispose()
		{
			RemoveSearchState();
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// Dispose( bool disposing ) executes in two distinct scenarios.
		/// If disposing equals true, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals false, the method has been called by the 
		/// runtime from inside the finalizer and you should not reference 
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
		private void Dispose( bool disposing )
		{
			// Check to see if Dispose has already been called.
			if ( !disposed )
			{
				// Protect callers from accessing the freed members.
				disposed = true;

				// If disposing equals true, dispose all managed and unmanaged resources.
				if ( disposing )
				{
					// Dispose managed resources.
					enumerator.Dispose();
				}
			}
		}
		
		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~SearchState()      
		{
			Dispose( false );
		}
		#endregion
	}

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
		[Serializable]
		public enum Encryption
		{
			None = 0,
			Encrypt = 1,
			EnforceEncrypt = 2,
			SSL = 4,
			EnforceSSL = 8
		}

                /// <summary>
                /// Group Quota Restriction Method.
                /// </summary>
                private enum QuotaRestriction
                {
                                // For current Implementation, enum value AllAdmins is not used, can be used in future
                                UI_Based,
                                Sync_Based
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
		/// The User HomeServer Url 
		/// </summary>
	        public string NewHomeServerUrl;

		/// <summary>
		/// Percentage Data Move details.
		/// </summary>
	        public int DataMovePercentage;

		/// <summary>
		/// Percentage Data Move details.
		/// </summary>
	        public string DataMoveStatus;

		/// <summary>
		/// Percentage Data Move details.
		/// </summary>
	        public bool IsGroup;

		/// <summary>
		/// preferences in case of group admin.
		/// </summary>
	        public int Preference;

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderUser()
		{
			Preference = -1;
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
			try
			{
            			this.MemberRights = RightsUtility.Convert(member.Rights);
			}
			catch
			{
				// some problem in accessing member tights, so log it and return false
				log.Debug("Error: Exception while checking rights for user :"+member.Name);
				this.MemberRights = Rights.ReadOnly;
			}
			this.FullName = (member.FN != null) ? member.FN : member.Name;
			this.FirstName = member.Given;
			this.LastName = member.Family;
			try
			{
				this.Enabled = !(domain.IsLoginDisabled(this.ID));
			}
			catch
			{
				// some problem in accessing member and property, so log it and return true
				log.Debug("Error: Exception while checking whether user is disabled or not for user :"+member.Name);
				this.Enabled = true;
			}
			this.IsOwner = (member.UserID == collection.Owner.UserID);
			this.Email = NodeUtility.GetStringProperty(member, EmailProperty);
			this.IsGroup = (member.GroupType != null ) ? true : false;

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
			this.Preference = -1;
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
			this.Preference = -1;

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

				if ( member.NewHomeServer != null )
				{
					HostNode newHomeNode = HostNode.GetHostByID(domain.ID, member.NewHomeServer);
					if(newHomeNode != null)
					{
                            			this.NewHomeServer = (newHomeNode.Name == null ) ? string.Empty : newHomeNode.Name;
						this.NewHomeServerUrl = newHomeNode.PublicUrl;
					}
					else
						this.NewHomeServer = string.Empty;
				}
                        	else
                            	this.NewHomeServer = string.Empty;

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
					long DataTransferred  = 1;
					int iFolderMoveState = 0;

					// From catalog, get the total size of the collections owned by this user.
					// Then, check which iFolders are present on local machine, those which are not present are moved, minus them
					long TotalSpaceUsed = Catalog.GetSpaceUsedByOwnerID( member.UserID );
					HostNode LocalHostNode = HostNode.GetLocalHost();
					bool LocalHostIsNewHome = false;
		
					bool LocalHostIsOldHome = false;
					bool ValidLocalHost = ( LocalHostNode != null && !String.IsNullOrEmpty(LocalHostNode.Name) );
					if( ValidLocalHost && !String.IsNullOrEmpty(this.HomeServer) && String.Equals( this.HomeServer, LocalHostNode.Name))
					{
						if( LocalHostNode.Name == this.HomeServer )
						{
							LocalHostIsOldHome = true;
						}
					}
					if( ValidLocalHost && !String.IsNullOrEmpty(this.NewHomeServer) && String.Equals( this.NewHomeServer, LocalHostNode.Name))
					{
							LocalHostIsNewHome = true;
					}

					// If localhost is where user is moving, then start with 0 and add all collections in local store as moved ones
					// If localhost is older home, then start with total data size and subtract collections which are not in local store 
					DataTransferred = LocalHostIsNewHome ? 0 : ( LocalHostIsOldHome ? TotalSpaceUsed : 0); 

                        		ICSList collectionList = store.GetCollectionsByOwner( member.UserID, domain.ID );
                        		foreach ( ShallowNode sn in collectionList )
                        		{
                                		Collection iFolderCol = new Collection( store, sn );
						iFolderMoveState = member.iFolderMoveState(domain.ID, false, iFolderCol.ID, 0, 0);
						log.Debug("iFolderUser: The iFolderMoveState is :"+iFolderMoveState);
						if(iFolderMoveState  > 1 )
						{
							// This is almost non-reachable code. because when iFolderMoveState becomes 2, it means collection is
							// moved and in that case, the collection will not be present in local store, so store.getco..Owner
							// will not return the collection's ID. should not we remove this true codepath????
							DataTransferred += member.MovediFolderSize(domain.ID, iFolderCol.ID);
						}
						else
						{
							DataTransferred = LocalHostIsNewHome ? ( DataTransferred + iFolderCol.StorageSize /* local server is new home for the user*/) : ( DataTransferred - iFolderCol.StorageSize /* user is getting moved away from local server*/ );

						}
                        		}
					if(TotalSpaceUsed != 0)
					{
						log.Debug("iFolderUser: After total calculation, now size of data that already moved is {0} and total space used by user/allcolls is {1}", DataTransferred, TotalSpaceUsed);
						DataMovePercentage += (int) ((80*DataTransferred)/TotalSpaceUsed);
					}
					else
						DataMovePercentage += 80;
						
				}
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

			// impersonate
			iFolder.Impersonate(c, accessID);

			ICSList searchList = c.GetMemberList();
			SearchState searchState = new SearchState( domain.ID, searchList.GetEnumerator() as ICSEnumerator, searchList.Count );
			int total = searchList.Count;	
			int i = 0;
			if(index > 0)
				searchState.Enumerator.SetCursor(Simias.Storage.Provider.IndexOrigin.SET, index);
			Member member = null;
			ArrayList list = new ArrayList();
			foreach(ShallowNode sn in searchList)
			{
				member = new Member(domain, sn);
				iFolderUser user = new iFolderUser(member, c, domain);
				list.Add(user);
				if(i++ >= max )
					break;
			}
			return new iFolderUserSet((iFolderUser[])list.ToArray(typeof(iFolderUser)), total);
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
			return GetUsers(property, operation, pattern, index, max, accessID, false);
		}

		/// <summary>
		/// Get Monitored groups by Search
		/// </summary>
		/// <param name="property">The Search Property</param>
		/// <param name="operation">The Search Operator</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <param name="adminrequest">whether the request is coming from web-admin/web-access</param>
		/// <returns>An iFolder User Set</returns>
		public static iFolderUserSet GetUsers(SearchProperty property, SearchOperation operation, string pattern, int index, int max, string accessID, bool adminrequest)
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
			int usertype = -1;
			if( accessID != null && adminrequest)
				usertype = 2; // groupadmin
			
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

				case SearchProperty.GroupOnly:
					searchProperty = BaseSchema.ObjectName;//PropertyTags.GroupType;
					break;

				default:
					searchProperty = BaseSchema.ObjectName;
					break;
			}
			log.Debug("GetUsers entered...");
			
			// impersonate
			Rights UserRight = iFolder.Impersonate(domain, accessID);

			//bool UserIsDomainOwner = (UserRight == Rights.Admin);

			Simias.Storage.SearchPropertyList SearchPrpList = new Simias.Storage.SearchPropertyList();
			ArrayList list = new ArrayList();
			string iFolderServerUsers = null;
			int i = 0;
			int total = 0;

			if(property == SearchProperty.HomeServerName)
			{
				iFolderServerSet iFSet = iFolderServer.GetServersByName(iFolderServerType.All, operation, pattern, 0, 1);
				foreach(iFolderServer ifServer in iFSet.Items)
					iFolderServerUsers = ifServer.ID;	
				if(iFolderServerUsers == null)
					return new iFolderUserSet((iFolderUser[])list.ToArray(typeof(iFolderUser)), 0);
			}

			if(accessID == null || ( usertype != 2  && domain.GroupSegregated != "yes" ))
			{
				// Root Admin searching all the users to list a set of users
				if(iFolderServerUsers == null)
				{
					if(property != SearchProperty.GroupOnly)
					{
						SearchPrpList.Add(searchProperty, pattern, searchOperation);
						SearchPrpList.Add(BaseSchema.ObjectType, NodeTypes.MemberType, SearchOp.Equal);
						SearchPrpList.Add("DN","*", SearchOp.Exists);
					}
					else
					{
						SearchPrpList.Add(PropertyTags.GroupType,"*", SearchOp.Exists);
						SearchPrpList.Add(searchProperty, pattern, searchOperation);
					}
				}
				else
					SearchPrpList.Add(PropertyTags.HostID, iFolderServerUsers, SearchOp.Equal);
				ICSList searchList = domain.Search(SearchPrpList);

				total = searchList.Count;	
				SearchState searchState = new SearchState( domain.ID, searchList.GetEnumerator() as ICSEnumerator, searchList.Count );
				if(index > 0)
					searchState.Enumerator.SetCursor(Simias.Storage.Provider.IndexOrigin.SET, index);
				Member member = null;
				foreach(ShallowNode sn in searchList)
				{
					if(max != 0 && i++ >= max )
						break;
					try
					{
						member = new Member(domain, sn);
						iFolderUser user = new iFolderUser(member, domain, domain);
						list.Add(user);
					}
					catch (Exception ex)
					{
						// Because of some exception, this user has missed the list, log it and proceed
						if(member != null)
						{
							log.Debug("Error: This member could not be added in the user's list."+member.Name);
							log.Debug("Error: User Object might be corrupted in iFolder database. Try doing a Ldap Sync.");
						}
						else
						{
							log.Debug("Error: Could not add the member because it is null. Try doing a Ldap sync.");
						}
						log.Debug("Error: "+ex.Message);
						log.Debug("Error Trace: "+ex.StackTrace);
					}
				}
			}
			else if(accessID != null)
			{
				Hashtable UniqueObjectsHashTable = new Hashtable();
				ArrayList sortList = new ArrayList();
				Member memberObj = domain.GetMemberByID(accessID);
				Member member = null;
				string[] AdminGroups = null;
				if(usertype == 2)
				{
					//secondary Admin and loggeded in to Web admin
					AdminGroups = memberObj.GetMonitoredSubGroups();
				}
				else
				{
					// web access with segregated groups enabled
					string dn = String.Empty;
                                       	try
                                       	{
                                               	dn = memberObj.Properties.GetSingleProperty( "DN" ).Value as string;
                                       	}
                                       	catch{}
					AdminGroups = domain.GetMemberFamilyDNList(dn);
				}
				if( AdminGroups != null && AdminGroups.Length != 0)
				{

					foreach(string group in AdminGroups)
					{
						if(iFolderServerUsers == null)
							SearchPrpList.Add(searchProperty, pattern, searchOperation);
						else
							SearchPrpList.Add(PropertyTags.HostID, iFolderServerUsers, SearchOp.Equal);
						SearchPrpList.Add("UserGroups",group, SearchOp.Contains);
						ICSList searchList = domain.Search(SearchPrpList);
						if(searchList != null)
							foreach(ShallowNode sn in searchList)
							{
								if(!UniqueObjectsHashTable.ContainsKey(sn.ID))
								{
									UniqueObjectsHashTable.Add(sn.ID,"");
									sortList.Add(sn);
								}
							}
						SearchPrpList.Clean();
						SearchPrpList.Add("DN", group, SearchOp.Equal);
						if(iFolderServerUsers == null)
							SearchPrpList.Add(searchProperty, pattern, searchOperation);
						else
							SearchPrpList.Add(PropertyTags.HostID, iFolderServerUsers, SearchOp.Equal);
						searchList = domain.Search(SearchPrpList);
						if(searchList != null)
							foreach(ShallowNode sn in searchList)
							{
								if(!UniqueObjectsHashTable.ContainsKey(sn.ID))
								{
									UniqueObjectsHashTable.Add(sn.ID,"");
									sortList.Add(sn);
								}
							}
						SearchPrpList.Clean();
					}
				}
				sortList.Sort();	
				total = sortList.Count;	
				foreach(ShallowNode sn in sortList)
				{
					if ((i >= index) && (i < (max + index)))
					{
						try
						{
							member = new Member(domain, sn);
							list.Add(new iFolderUser(member, domain, domain));
						}
						catch (Exception ex)
						{
							// Because of some exception, this user has missed the list, log it and proceed
							if(member != null)
							{
								log.Debug("Error: This member could not be added in the user's list."+member.Name);
								log.Debug("Error: User Object might be corrupted in iFolder database. Try doing a Ldap Sync.");
							}
							else
							{
								log.Debug("Error: Could not add the member because it is null. Try doing a Ldap Sync.");
							}
							log.Debug("Error: "+ex.Message);
							log.Debug("Error Trace: "+ex.StackTrace);
						}
					}
					++i;
				}
			}

			return new iFolderUserSet((iFolderUser[])list.ToArray(typeof(iFolderUser)), total);
		}


		/// <summary>
		/// Get Monitored groups by Search
		/// </summary>
		/// <param name="property">The Search Property</param>
		/// <param name="operation">The Search Operator</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <param name="SecondaryAdminIDID">The User ID of Secondary Admin for which groups has to be retrieved</param>
		/// <param name="MonitoredGroups">For Future use, currently always true for group retrieval, can be used for monitored servers</param>
		/// <param name="adminrequest">whether the request is coming from web-admin/web-access</param>
		/// <returns>An iFolder User Set</returns>
		public static iFolderUserSet GetMonitoredGroupsSet(SearchProperty property, SearchOperation operation, string pattern, int index, int max, string accessID, string SecondaryAdminID, bool MonitoringGroups, bool adminrequest)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);

			string [] MonitoredGroups = null;
			if(MonitoringGroups)
				MonitoredGroups = GetMonitoredGroups(SecondaryAdminID);


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
			int usertype = -1;
			string userid = null;
			Hashtable ht = new Hashtable();
			if( accessID != null && adminrequest)
			{
				usertype = 2; // groupadmin
				userid = accessID;
				accessID = null;
				Domain dom = store.GetDomain(store.DefaultDomain);
				Member mem = dom.GetMemberByID(userid);
				ht = mem.GetMonitoredUsers(true);
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

				case SearchProperty.GroupOnly:
					searchProperty = PropertyTags.FullName;//PropertyTags.GroupType;
					break;

				default:
					searchProperty = PropertyTags.FullName;
					break;
			}
			
			// impersonate
			Rights UserRight = iFolder.Impersonate(domain, accessID);
			
			bool UserIsDomainOwner = (UserRight == Rights.Admin);

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
								{
									if ((i >= index) && (((max <= 0) || i < (max + index))))
									{
										iFolderUser user = new iFolderUser(member, domain, domain);
										if( Array.IndexOf( MonitoredGroups, user.ID) >= 0)
											list.Add(user);
									}
									++i;
								}
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
						bool GroupMatched = true;

						// Don't include Host objects as iFolder users.
						if (!member.IsType("Host"))
						{
							// if search was based on groupandmembers, then add only group first.
							if( Array.IndexOf(MonitoredGroups, member.UserID) >= 0 )
							{			
								if( (UserIsDomainOwner || (domain.GroupSegregated == "no" || (domain.GroupSegregated == "yes" && GroupMatched == true))) )
								{
									if ((i >= index) && (((max <= 0) || i < (max + index))))
									{
											if( usertype == 2 && ht.ContainsKey(member.UserID) == false)
												continue;
											iFolderUser user = new iFolderUser(member, domain, domain);
											if( usertype == 2)
												user.Preference = GetAdminRights(userid, member.UserID);
		
											list.Add(user);
									}
	
									++i;
							}
						}
		
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

		public static void SetMemberRights(string ifolderID, string userID, Rights rights, string accessID)
                {
                       SetMemberRights( ifolderID, null, userID, rights, accessID);
                } 


		/// <summary>
		/// Provision Users to different servers given in the Hashtable
		/// </summary>
		/// <param name="ServerProvisioningNames">Hashtable containing userids and servernames </param>
		/// <param name="ServerNames">A string array containing server names</param>
		/// <param name="UserIDs">A string array containing corresponding userids</param>
		public static void ProvisionUsersToServers(string [] ServerNames, string [] UserIDs)
		{
			int index = 0;
			try
			{
				if( ServerNames.Length <= 0 || UserIDs.Length <=0 )
					throw new InvalidOperationException("NOESERVER");
				for (index = 0 ; index < ServerNames.Length ; index++)
				{
					//Simias.Host.HostInfo ServerInfo = ManualProvisionUserProvider.ProvisionUser(UserIDs[index], ServerNames[index]);
					ManualProvisionUserProvider.ProvisionUser(UserIDs[index], ServerNames[index]);
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
			int index ; 
			iFolderServer serverobject = iFolderServer.GetServerByName(ServerName);
			try
			{
				if( UserIDs.Length <= 0)
					throw new InvalidOperationException("NOESERVER");
				for (index = 0 ; index < UserIDs.Length ; index++)
				{
					ManualProvisionUserProvider.ProvisionUser(UserIDs[index], ServerName);
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
			iFolderServer serverobject = iFolderServer.GetServerByName(ServerName);
			log.Debug("ReProvisionUsersToServer: --{0}--{1}", UserID, ServerName );
			if(UserID == null || UserID == String.Empty || UserID == "" || ServerName == null || ServerName == String.Empty || ServerName == "")
				return;
			try
			{
				ManualProvisionUserProvider.ProvisionUser(UserID, ServerName);
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
		public static void SetMemberRights(string ifolderID, string groupid, string userID, Rights rights, string accessID)
		{
			try
			{
				SharedCollection.SetMemberRights(ifolderID, groupid, userID, RightsUtility.Convert(rights).ToString(), accessID);
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
		/// checks whether the user is a secondary administrator or not 
		/// </summary>
		/// <param name="userID">The User ID</param>
		/// <returns>true if user rights are of a secondary admin</returns>
		public static bool IsGroupAdministrator(string userid)
                {
                       Store store = Store.GetStore();
                       Domain domain = store.GetDomain(store.DefaultDomain);
                       Member member = domain.GetMemberByID( userid );
                       Access.Rights rights = (member != null) ? member.Rights : Access.Rights.Deny;
                       return rights == Access.Rights.Secondary;
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
		/// Give a User Administration Rights
		/// </summary>
		/// <remarks>
		/// A User is a system administrator if the user has "Admin" rights in the domain.
		/// </remarks>
		/// <param name="userID">The User ID</param>
		public static void AddGroupAdministrator(string groupid, string userID, int preference)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID(userID);
			SetMemberRights(domain.ID, groupid, userID, Rights.Secondary, null);
			member.AddToGroupList(groupid, preference);
		}


		/// <summary>
		/// Set Aggregate Disk Quota for whole group 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <param name="groupid">ID of group for which the quota has to be set</param>
		/// <param name="value">the value to be set</param>
		/// <returns>true if set, false if because of some restrictions it cannot be set</returns>
		public static bool SetAggregateDiskQuota(string groupid, long value)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID(groupid);
			// If UI based restriction was there then initialize the user's in the group quota with <minimum> MB, so that it is mandatory
			// for secondary administrator to initialize them, and that time it can be checked that whether quota for all users, when added
			// do not cross group quota limit.

			// for setting 0 as aggregate disk quota, first the group level per user disk quota should be made less. check it out
			if( value < 0 )
			{		
				member.AggregateDiskQuota = value;
				domain.Commit();
				return true;
			}

			if( domain.GroupQuotaRestrictionMethod == (int)QuotaRestriction.UI_Based )
			{
				log.Debug("UI/Both based restriction");
				string [] MembersList = domain.GetGroupsMemberList( groupid );
				long SizeAllocatedToMembers = 0;
				long UsedSpaceByMembers = 0;
				foreach(string MemberID in MembersList)
				{
					// now check, if space allocated to each member of group, when added does not exceed 'value'
					long MemberAllocation = Simias.Policy.DiskSpaceQuota.Get( domain.GetMemberByID(MemberID)).Limit;
					long UsedSpace = Simias.Policy.DiskSpaceQuota.Get( domain.GetMemberByID(MemberID)).UsedSpace;
					if( MemberAllocation >= 0 )
						SizeAllocatedToMembers += MemberAllocation;
					if(UsedSpace >= 0)
						UsedSpaceByMembers += UsedSpace;
				}

				if(value >= SizeAllocatedToMembers && value >= UsedSpaceByMembers)
				{
					if( Simias.Policy.DiskSpaceQuota.Get(member).Limit <= -1 )
					{
						// for this group, no disk quota was set, initialize with 0 so that it is mandatory for admin
						// to set the quota explicitly, that time aggregate disk quota limit violation for group can 
						// be checked
						log.Debug("Going to set the disk quota limit for group as 0 Bytes");
						// If no disk quota was set on group, then set 0 bytes
						Simias.Policy.DiskSpaceQuota.Set(member, 0);
					}
				}
				else
					return false;
			}
			member.AggregateDiskQuota = value;
			domain.Commit();
			return true;
		}

		/// <summary>
		/// Get Aggregate Disk Quota for whole group 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <param name="groupid">ID of group for which the quota has to be set</param>
		/// <returns>the value in bytes, -1 if no quota was set</returns>
		public static long GetAggregateDiskQuota(string groupid)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID(groupid);
			return member.AggregateDiskQuota ;
		}

		/// <summary>
		/// Set Aggregate Disk Quota for whole group 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <param name="groupid">ID of group for which the quota has to be set</param>
		/// <param name="value">the value to be set</param>
		/// <returns>returns true if group quota restriction check has not to be done on UI</returns>
		/// <returns>return true if this member's root groups does not have aggregate disk quota limit set and param limit is any long value</returns>
		/// <returns>returns true if this member's root groups have aggregate quota set and limit+allocated space to members of groups is lower</returns> 
		/// <returns>than group's aggregate disk quota set.</returns>
		/// <returns>returns false if member's root groups have aggregate quota set and limit is -1</returns>
		/// <returns>returns false if member's root groups have aggregate quota set and limit+allocated space to members of groups is higher</returns>
		/// <returns>than group's aggregate disk quota set.</returns> 

		public static bool DiskQuotaPolicyChangeAllowed(string memberID, long limit)
		{
			bool Allowed = false;
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			// if limit is -1, it means admin wants to set the limit to unlimited
			string [] GroupIDs = domain.GetMemberFamilyList(memberID);
			Member OperatedMember = domain.GetMemberByID(memberID);
			bool IsGroupID = false;
			string ParentGroups = null;
			Property Groupproperty = OperatedMember.Properties.GetSingleProperty( "GroupType" );
			if(Groupproperty != null)
				IsGroupID = true;
			foreach(string GroupID in GroupIDs)
			{
				bool IsChildGroup = false;
				Member GroupAsMember = domain.GetMemberByID(GroupID);
				long GroupDiskQuota = GroupAsMember.AggregateDiskQuota;
				// If groups are not configured then this call returns null
				Property p = GroupAsMember.Properties.GetSingleProperty( "UserGroups" );
				if(p != null)
					ParentGroups = p.Value as string;
				if(ParentGroups != null && ParentGroups != String.Empty)
				{
					IsChildGroup = true;
				}
				if ( IsChildGroup && GroupDiskQuota <= -1 )
				{
					// If it is a child group, and no aggregate disk quota set on it, then we will still iterate for 
					// parent group to know exact used space and if any group disk quota is set there
					continue;
				}
				
				if(( ! IsChildGroup && GroupDiskQuota <= -1 ) || limit == 0)
				{
					// It is a root group and disk quota was not set for whole group, so any value can be set.
					return true;
						
				}
				if( domain.GroupQuotaRestrictionMethod == (int)QuotaRestriction.UI_Based )
				{

					// If GroupDiskQuota was set and GroupQuotaRestriction has to be done on UI, then unlimited space cannot be set
					if( limit == -1 )
					{
						// check if any other group does not have aggregate quota set, then under that group, it can be allowed
						if( Simias.Policy.DiskSpaceQuota.GetLimit( domain.GetMemberByID(memberID)) == -1)
						{
							// This is the case when some other policies are getting changed and earlier also
							// disk quota for this user was not set, so no problem in setting -1 again.
							// Since it is 100% sure that on group level some quota per user is set, so 
							// during upload/sync automatically that will be followed
							Allowed = true ;
							return Allowed;
						}
						else
							Allowed = false;
						continue;
					}

					long SizeAllocatedToMembers = 0;
					// this check means, before setting policy, it has to be checked on UI
					string [] MembersList = domain.GetGroupsMemberList( GroupID );
					foreach( string GroupMemberID in MembersList)
					{
						// check if all member's allocated space + new space does not exceed disk quota for group
						if(GroupMemberID != memberID)
						{
							// Do not add current member allocated disk quota, we will add in last
							Member LoopingMember = domain.GetMemberByID(GroupMemberID);
							long MemberAllocation = Simias.Policy.DiskSpaceQuota.GetLimit( LoopingMember );
							long MemberAggAllocation = Simias.Policy.DiskSpaceQuota.Get( LoopingMember ).Limit;
							//bool LoopingMemberIsGroup = (LoopingMember.Properties.GetSingleProperty( "UserGroups" ).Value as string) != null ? true : false;
							if( MemberAllocation >= 0 )
							{
								SizeAllocatedToMembers += MemberAllocation;
							}
							else
							if( MemberAggAllocation >= 0 )
							{
								SizeAllocatedToMembers += MemberAggAllocation;
							}
							
							if(IsGroupID && MemberAllocation < 0)
							{				
								// no group level disk quota, add new group level per user quota will exceed
								SizeAllocatedToMembers += limit;	
							}
						}
					}
					SizeAllocatedToMembers += limit;
					if( GroupDiskQuota >= SizeAllocatedToMembers )
					{
						Allowed = true;
						return Allowed;
					}
					else
					{
						Allowed = false;
						if( ! IsChildGroup)
						{
							// this is the case where allowed is false on root level of group, but we still want
							// to iterate on other groups where it may be allowed
							continue;
						}
						else
						{
							// FIXME: if it is a child group, then we return immediately if Allowed is false,
							// Ideally, we should iterate and see if any other group allows that (same as root
							// level in above if case), but we return immediately fearing that if parent of
							// this child group allows to change the quota, then it will be a problem because
							// precedence of child group is higher than its own parent groups.
							return Allowed;
						}	
					}	
				}
				else
				{
					Allowed = true;
					return Allowed;
				}
			}
			return Allowed;	
		}

		/// <summary>
		/// Get rights for admin, the preferences set for this administrator 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <param name="userid">userid for which rights are asked</param>
		/// <param name="groupid">ID of group for which right is asked</param>
		/// <returns>int value for rights</returns>
		public static int GetAdminRights(string userid, string groupid)
		{
			if( userid == null)
				return 0xffff;
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID(userid);
			if(member.Rights.ToString() == Rights.Admin.ToString())
				return 0xffff;
			return member.GetPreferencesForGroup(groupid);		
		}

		/// <summary>
		/// Get ids of monitored groups 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <param name="userid">userid for which monitored groups are asked</param>
		/// <returns>string array containing ids of managed groups</returns>
		public static string[] GetMonitoredGroups(string userid)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID(userid);
			return member.GetMonitoredGroups();
		}

		/// <summary>
		/// Get namess of monitored groups 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <param name="userid">userid for which monitored groups are asked</param>
		/// <returns>string array containing namess of managed groups</returns>
		public static string[] GetMonitoredGroupNames(string userid)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID(userid);
			string [] IDs = member.GetMonitoredGroups();
			if ( IDs == null || IDs.Length <= 0)
				return null;
			string [] MemberNames = new string [IDs.Length];
			int count = 0;
			foreach(string id in IDs)
			{
				Member mem = domain.GetMemberByID(id);	
				MemberNames [ count++ ] = mem.FN;
			}
			return MemberNames;
		}

		/// <summary>
		/// Give a User Administration Rights
		/// </summary>
		/// <remarks>
		/// A User is a system administrator if the user has "Admin" rights in the domain.
		/// </remarks>
		/// <param name="userID">The User ID</param>
		public static void RemoveGroupAdministrator(string groupid, string userID)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);
			Member member = domain.GetMemberByID(userID);
			int ct = member.RemoveFromGroupList(groupid);
			if( ct == 0)
			{
				SetMemberRights(domain.ID, groupid, userID, Rights.ReadWrite, null);
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
			catch
			{
				throw;
			}
			return GroupIDs;
		}

		/// <summary>
		///  Check if upload is allowed or not based on group aggregate quota restriction 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <param name="userid">userid who is uploading </param>
		/// <param name="deltaSize">the size that is getting uploaded </param>
		/// <returns>true even if one of the groups (out of all group this user is member of) allows the upload</returns>
		public static bool GroupQuotaUploadAllowed(string UserID, long deltaSize)
		{
			bool Allowed = true;
			bool SpaceAllowed = false;
			string [] GroupIDs = GetGroupIDs(UserID);
			foreach(string groupID in GroupIDs)
			{
				if( groupID != UserID )
				{
					long AggregateQuota = GetAggregateDiskQuota(groupID);
					if(AggregateQuota < 0 )
					{
						// no group quota applied, so allow
						return true;
					}
					long SpaceUsed = SpaceUsedByGroup(groupID);
				 	SpaceAllowed = ( AggregateQuota - SpaceUsed ) > deltaSize ? true: false;
					if( SpaceAllowed == true)
					{
						return true;
					}
					else
						Allowed = false;
				}
			}
			log.Debug("Allowed boolean is  :"+Allowed);
			return Allowed;
		}

		/// <summary>
		///  Get the sun of space used by all members of group 
		/// </summary>
		/// <remarks>
		/// 
		/// </remarks>
		/// <param name="GroupID">GroupID for which space used is asked </param>
		/// <returns>sum of spaces used by group members</returns>
		public static long SpaceUsedByGroup(string GroupID)
                {
                        return Catalog.GetSpaceUsedByGroupMembers( GroupID );
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
		/// <param name="admintype">type of admint o be retrieved, 0 for all admins, 1 for secondary admins, 2 for root admins</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <remarks>
		/// A User is a system administrator if the user has "Admin" rights in the domain.
		/// </remarks>
		/// <returns>An iFolder User Set</returns>
		public static iFolderUserSet GetAdministrators(int index, int max, int admintype)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);

			ICSList members = null;

			// sort the list
			ArrayList sortList = new ArrayList();

			if(admintype == 0 || admintype == 2)
			{
				members = domain.GetMembersByRights(Access.Rights.Admin);
				foreach(ShallowNode sn in members)
				{
					sortList.Add(sn);
				}
			}
			if(admintype == 0 || admintype == 1)
			{
				members = domain.GetMembersByRights(Access.Rights.Secondary);
				foreach(ShallowNode sn in members)
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
        /// Checks whether passphrase is set for this member or not
		///</summary>
		///<returns>true if passphrase was set earlier</returns>
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
		///<returns>status</returns>
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
				string DecryptedCryptoKey = String.Empty;
				Key DeKey = new Key(EncrypCryptoKey);
                                try
				{
					// With Mono2.4, it throws cryptographic exception for wring passphrase, it was suggested by mono team to catch
					// this exception and interpret it. bug#507169
					DeKey.DecrypytKey(passphrase, out DecryptedCryptoKey);
				}
				catch (System.Security.Cryptography.CryptographicException ex)
				{
					//This particular exception is thrown only for wrong passphrase, so return failure messgae, no need to check further
					if(ex.Message.IndexOf("Bad PKCS7") >= 0)
					{
						log.Debug("ValidatePassPhrase : false");
						return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.PassPhraseInvalid);

					}
				}

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
		///<returns>None</returns>
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
