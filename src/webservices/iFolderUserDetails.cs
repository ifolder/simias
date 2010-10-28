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

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Web;
using Simias.Server;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder User Details
	/// </summary>
	[Serializable]
	public class iFolderUserDetails : iFolderUser
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(iFolderUser));
		
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
		/// Specifies the user's group list or Groups Member list
		/// </summary>
		public string GroupOrMemberList;

		/// <summary>
		/// Specifies the user type. 0 -> User , 1 -> Ldap Group 2-> Local Group
		/// </summary>
		public int MemberType=0;

		/// <summary>
		/// Number of iFolders that the user owns.
		/// </summary>
		public int OwnediFolderCount = 0;

		/// <summary>
		/// Number of iFolders shared with the user.
		/// </summary>
		public int SharediFolderCount = 0;

	    /// <summary>
        /// The User HomeServer Name
        /// </summary>
        public string DetailHomeServer;

        /// <summary>
        /// The User HomeServer Name
        /// </summary>
        public string DetailNewHomeServer;

        /// <summary>
        /// Percentage Data Move details.
        /// </summary>
        public int DetailDataMovePercentage;

	    /// <summary>
	    /// Percentage Data Move details.
	    /// </summary>
	    public string DetailDataMoveStatus;	
		public long GroupDiskQuota = -1;
		public long SpaceUsedByGroup = 0;


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

			if ( member.HomeServer != null )
				this.DetailHomeServer = (member.HomeServer.Name == null ) ? string.Empty : member.HomeServer.Name;
			else
				this.DetailHomeServer = string.Empty;

			if ( member.NewHomeServer != null )
			{
				HostNode newHomeNode = HostNode.GetHostByID(domain.ID, member.NewHomeServer);
				if(newHomeNode != null)
					this.DetailNewHomeServer = (newHomeNode.Name == null ) ? string.Empty : newHomeNode.Name;
				else
					this.DetailNewHomeServer = string.Empty;
			}
			else
				this.DetailNewHomeServer = string.Empty;

			int state = member.UserMoveState;
			switch(state)
		        {
		             case (int)Member.userMoveStates.Nousermove:
		             case (int)Member.userMoveStates.Initialized:
		                       DetailDataMoveStatus = "Initializing";
		                       DetailDataMovePercentage = 0;
		                       break;
		              case (int)Member.userMoveStates.UserDisabled:
		                        DetailDataMoveStatus = "Initialized";
		                        DetailDataMovePercentage = 5;
		                       break;
		               case (int)Member.userMoveStates.DataMoveStarted:
		                        DetailDataMoveStatus = "Moving iFolders";
		                        DetailDataMovePercentage = 10;
		                       break;
		               case (int)Member.userMoveStates.Reprovision:
		                        DetailDataMoveStatus = "Resetting Home";
		                        DetailDataMovePercentage = 10;
		                       break;
		               case (int)Member.userMoveStates.MoveCompleted:
		                        DetailDataMoveStatus = "Finalizing";
		                        DetailDataMovePercentage = 15;
		                       break;
		               default:
		                        DetailDataMovePercentage = 0;
			                    DetailDataMoveStatus = "Initializing";
			                   break;
		     }
			 if( state < (int)Member.userMoveStates.DataMoveStarted)
					    DetailDataMovePercentage += 0;
			 else if( state > (int)Member.userMoveStates.DataMoveStarted)
					    DetailDataMovePercentage += 80;
			 else
			   {
                    Store stored = Store.GetStore();
					long SpaceUsed = 0;
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
							LocalHostIsOldHome = true;
					}

					if( ValidLocalHost && !String.IsNullOrEmpty(this.DetailNewHomeServer) && String.Equals( this.DetailNewHomeServer, LocalHostNode.Name))
					{		
							LocalHostIsNewHome = true;
					}	

					// If localhost is where user is moving, then start with 0 and add all collections in local store as moved ones
					// If localhost is older home, then start with total data size and subtract collections which are not in local store
					DataTransferred = LocalHostIsNewHome ? 0 : ( LocalHostIsOldHome ? TotalSpaceUsed : 0);
					

					ICSList collectionList = stored.GetCollectionsByOwner( member.UserID, domain.ID );
					foreach ( ShallowNode sn in collectionList )
					{
					    	Collection iFolderCol = new Collection( stored, sn );
						//SpaceUsed += iFolderCol.StorageSize;
						iFolderMoveState = member.iFolderMoveState(domain.ID, false, iFolderCol.ID, 0, 0);
						if(iFolderMoveState  > 1 )
						{
							// This is almost non-reachable code. because when iFolderMoveState becomes 2, it means collection is
							// moved and in that case, the collection will not be present in local store, so store.getco..Owner
							// will not return the collection's ID. should not we remove this true codepath????
							 //it comes momentarily here only once..
								DataTransferred += iFolderCol.StorageSize;
						}
						else
						{
							DataTransferred = LocalHostIsNewHome ? ( DataTransferred + iFolderCol.StorageSize /* local server is new home for the user*/) : ( DataTransferred - iFolderCol.StorageSize /* user is getting moved away from local server*/ );
						}
					}
					if(TotalSpaceUsed != 0)
					{
						log.Debug("iFolderUserDetails: After total calculation, now size of data that already moved is {0} and total space used by user/allcolls is {1}", DataTransferred, TotalSpaceUsed);
						DetailDataMovePercentage += (int)(( 80 * DataTransferred ) / TotalSpaceUsed );
						log.Debug("iFolderUserDetails: DataMovePercentage after calculation is :"+DetailDataMovePercentage);
					}
					else
							DetailDataMovePercentage += 80;
			   }
			 if ( member.NewHomeServer != null )
			 {
				 HostNode newHomeNode = HostNode.GetHostByID(domain.ID, member.NewHomeServer);
				 if(newHomeNode != null)
					 this.DetailNewHomeServer = (newHomeNode.Name == null ) ? string.Empty : newHomeNode.Name;
				 else
					 this.DetailNewHomeServer = string.Empty;
			 }
			 else
					 this.DetailNewHomeServer = string.Empty;
					   
			// sync interval
			this.SyncIntervalEffective = Simias.Policy.SyncInterval.Get(collection).Interval;
	
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

			// Get the GroupType property for the member if it exists.
			Property Groupproperty = domainMember.Properties.GetSingleProperty( "GroupType" );
			if(null == Groupproperty )
			{
				this.MemberType= 0;
				Property GOMproperty = domainMember.Properties.GetSingleProperty( "UserGroups" );
				this.GroupOrMemberList = ( GOMproperty != null ) ? GOMproperty.ToString() : String.Empty;
			}
			else
			{
				if(String.Compare(Groupproperty.ToString().ToLower(), "global") == 0)
					this.MemberType= 1;
				else
					this.MemberType= 2;
				{
					// This braces is for adding members into group object and vice-versa
					string FullMembersList = "";
					MultiValuedList mvl = member.Properties.GetProperties( "MembersList" );
					if( mvl != null && mvl.Count > 0)
					{
						foreach( Property prop in mvl )
						{
							if( prop != null)
							{
								FullMembersList += prop.Value as string ;
								FullMembersList += "; ";
							}
						}
					}
					this.GroupOrMemberList = FullMembersList;
					this.GroupDiskQuota = member.AggregateDiskQuota;
					this.SpaceUsedByGroup = iFolderUser.SpaceUsedByGroup(member.UserID);
	
				}


			}

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
