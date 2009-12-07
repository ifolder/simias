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
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Threading;

using Simias.Client;
using Simias.Storage;
using Simias.Web;
using Simias.Server;
using Simias;

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

	public class RestoreParms
	{
		public string url;
		public string adminname;
		public string adminpassword;
		public string relativepath;
		public string basepath;
		public int startindex;
		public string ifolderid;
		public string LogLocation;

		public RestoreParms(string url, string adminname, string adminpassword, string ifolderid, string relativepath, string basepath, int startindex, string LogLocation)
		{
			this.url = url;
			this.adminname = adminname;
			this.adminpassword = adminpassword;
			this.relativepath = relativepath;
			this.basepath = basepath;
			this.startindex = startindex;
			this.ifolderid = ifolderid;
			this.LogLocation = LogLocation;
		}
	}

	/// <summary>
	/// An iFolder
	/// </summary>
	[Serializable]
	public class iFolder:IComparable
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( iFolder ) );

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
		/// If encryption enabled, the encryptionkey
		/// </summary>
		public int FolderMoveStatus = 0;

		/// <summary>
		/// Group admin rights...
		/// </summary>
		public int Preference= -1;

		public static Thread RestoreThread = null;

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
			log.Debug( "In iFolder Collection ID: ", accessID );
			// impersonate
			Rights rights = Impersonate(c, accessID);
			this.Preference = -1;

			this.ID = c.ID;
			this.Name = c.Name;
			this.Description = NodeUtility.GetStringProperty(c, PropertyTags.Description);
			this.DomainID = c.Domain;

			this.HostID = c.HostID;
			//Note : All iFolders (collections) have HostID. But A Domain on Master doesnt have it.
			if (this.HostID == null)    this.HostID = String.Empty;

			this.MemberRights = rights;
			this.Created = NodeUtility.GetDateTimeProperty(c, PropertyTags.NodeCreationTime);
			this.LastModified = NodeUtility.GetDateTimeProperty(c, PropertyTags.JournalModified);
			this.Published = NodeUtility.GetBooleanProperty(c, PropertyTags.Published);
			this.Enabled = !c.Disabled;
			this.MemberCount = c.GetMemberList().Count;

			// owner
			this.OwnerID = c.Owner.UserID;
			this.IsOwner = (accessID != null) && (accessID == this.OwnerID || GroupIsOwner(accessID, this.OwnerID, this.DomainID) );
			Domain domain = Store.GetStore().GetDomain(this.DomainID);
			Member domainMember = domain.GetMemberByID(this.OwnerID);
			this.Size = c.StorageSize;
			if(domainMember != null )
			{
				this.OwnerUserName = (domainMember.Name != null ) ? domainMember.Name:"";
				string fullName = domainMember.FN;
				this.OwnerFullName = (fullName != null) ? fullName : this.OwnerUserName;
            			this.FolderMoveStatus =  domainMember.iFolderMoveState(this.DomainID, false, this.ID, 0, 0); 
				if(this.FolderMoveStatus >= 1)
					this.Size = domainMember.MovediFolderSize(this.DomainID, this.ID);
			}

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
			log.Debug( "In iFolder CatalogEntry ID: ", accessID );
			// impersonate
//		        Rights rights = Impersonate(ce, accessID);
                        //TODO :
		        Rights rights = Rights.Admin;
			this.Preference = -1;

			this.ID = ce.CollectionID;
			this.Name = ce.Name;
                        //TODO:
//			this.Description = NodeUtility.GetStringProperty(c, PropertyTags.Description);
			this.Description = String.Empty;

			this.HostID = ce.HostID;
			this.Size = ce.CollectionSize;
			this.MemberRights = rights;
			this.Created = NodeUtility.GetDateTimeProperty(ce, PropertyTags.NodeCreationTime);
			this.LastModified = NodeUtility.GetDateTimeProperty(ce, PropertyTags.LastModified);
			this.Published = NodeUtility.GetBooleanProperty(ce, PropertyTags.Published);
                        //BUG : TODO
//			this.Enabled = !iFolderPolicy.IsLocked(c);
                        Store store = Store.GetStore();
                        this.Enabled = ce.Disabled;
			Collection col = null;
			if ( ce.UserIDs == null )
			{
				col = store.GetCollectionByID(this.ID);
				
				this.MemberCount = (col != null) ? col.GetMemberList().Count : 1;
			}
			else
				this.MemberCount = ce.UserIDs.Length;

			Domain domain = store.GetDomain(store.DefaultDomain);
			this.DomainID = domain.ID;

			// owner
			if ( ce.OwnerID == null )
			{
				// col might be null in one path where UserIDs is non-null
				if(col == null)
				{
					// till this time, owner has not been added to catalog, so fetch it from collection
					col = store.GetCollectionByID(this.ID);
					this.OwnerID = (col != null) ? col.Owner.UserID : String.Empty;
				}
				else
					this.OwnerID = col.Owner.UserID;

			}
			else
				this.OwnerID = ce.OwnerID;

			this.IsOwner = (accessID != null) && (accessID == this.OwnerID || GroupIsOwner(accessID, this.OwnerID, this.DomainID) );

			Member domainMember = domain.GetMemberByID(this.OwnerID);

			if (domainMember != null)
			{
			        string name = domainMember.Name;
 			        string fullName = domainMember.FN;

			    	this.OwnerUserName = (name != null) ? name : String.Empty;
				this.OwnerFullName = (fullName != null) ? fullName : this.OwnerUserName;
			}
			else
			{
			        this.OwnerUserName = String.Empty;
				this.OwnerFullName = String.Empty;
			}

		}
		
		/// <summary>
		/// Given an userID and Members list and ownerid , check whether this userid is part of the group (who is owner) or not  
		/// </summary>
		/// <param name="UserID">current userid</param>
		/// <param name="OwnerID">Owner's ID for this collection</param>
		private bool GroupIsOwner(string CurrentUserID, string OwnerID, string DomainID)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(DomainID);
			if(domain == null)
			{
				return false;
			}
			// current userid and ownerid do not match
			string[] IDs = domain.GetMemberFamilyList(CurrentUserID);
			foreach(string id in IDs)
			{
				if(id == OwnerID)
				{
					// one of current member's groups is same as ownerid , so return true
					log.Debug("going to return true: group is owner");
					return true;
				}
			}
			return false;
		}

		public int CompareTo(object iFolderobj)
		{
			iFolder Compare = (iFolder) iFolderobj;
			int result = this.Name.CompareTo(Compare.Name);
			if(result == 0)
				result = this.Name.CompareTo(Compare.Name);
			return result;
		}

		public static iFolder CreateEncryptediFolderWithID(string name, string userID, string description, string iFolderID, string eKey, string eBlob, string eAlgorithm, string rKey)
		{
			iFolder ifolder = CreateiFolder(name, userID, description, iFolderID );
			if( ifolder == null)
				return null;
			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(ifolder.ID);
			col = col.SetEncryptionProperties(eKey, eBlob, eAlgorithm, rKey);
			return new iFolder(col, null);
		}
		
		
		/// <summary>
                /// Create an iFolder through web admin
                /// </summary>
                /// <param name="name">The iFolder Name</param>
                /// <param name="userID">The User ID of the iFolder Owner</param>
                /// <param name="description">The iFolder Description</param>
                /// <param name="accessID">The Access ID</param>
                /// <returns>An iFolder Object</returns>
                public static iFolder CreateiFolder(string name, string userID, string description)
		{
			return CreateiFolder(name, userID, description, null);
		}
		public static iFolder CreateiFolder(string name, string userID, string description, string iFolderID)
                {
                        // NOTE: because the name of the iFolder will also be the
                        // name of entry, we must check it
                        iFolderEntry.CheckName(name);
			// Remove the catalog entry for this...
		//	Catalog.DeleteEntryByCollectionID( iFolderID);

                        Collection c = SharedCollection.CreateSharedCollection(
			               name, null, userID, iFolderCollectionType, true, null, description, null, iFolderID);

                        return new iFolder(c, null);
                }


		/// <summary>
		/// Create an iFolder through web access
		/// </summary>
		/// <param name="name">The iFolder Name</param>
		/// <param name="userID">The User ID of the iFolder Owner</param>
		/// <param name="description">The iFolder Description</param>
		/// <param name="accessID">The Access ID</param>
		/// <returns>An iFolder Object</returns>
		public static iFolder CreateiFolder(string name, string userID, string description, string accessID, bool ssl, string encryptionAlgorithm, string PassPhrase)
		{
			// NOTE: because the name of the iFolder will also be the
			// name of entry, we must check it
			iFolderEntry.CheckName(name);
			Collection c = SharedCollection.CreateSharedCollection(
				name, null, ssl, userID, iFolderCollectionType, true, null, description, accessID, encryptionAlgorithm, PassPhrase);
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
		/// Get the recovery agent list
		/// </summary>
		/// <returns>The list as a string array</returns>
		public static string[] GetRAList()
		{
		    ArrayList list = Simias.Security.CertificateStore.GetRAList();
		    if(list.Count > 0)
		    {		
		    	string[] ralist = new string [ list.Count ];
		    	int i=0;

		    	foreach (string ra in list)
		    	{
				ralist[ i++ ] = ra;
		    	}
		    	return ralist;
		    }
		    return null;	
		}
		
		/// <summary>
		/// check if the tabs should be shown for the encrypted ifolder 
		/// </summary>
		/// <returns>boolean value</returns>
		public static bool ShowTabDetails(string PassPhrase, string EncryptionAlgorithm)
		{
			if(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty))
			{
				// it means , this is not an encrypted ifolder 
				// enable the search 
				return true;
			}
			else if(PassPhrase != null)
			{
				// user is in current session , so enable it 
				return true;
			}
			else
				return false;
		}
	
		/// <summary>
		/// Get the Certificate for the specified store.
		/// </summary>
		/// <param name="host">The host who owns the certificate.</param>
		/// <returns>The certificate as a byte array.</returns>
		public static byte[] GetRACertificate(string rAgent)
        {
			// Normalize the RA name.
			return Simias.Security.CertificateStore.GetRACertificate(rAgent);
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
				if(ce == null)
					return null;
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
				if(member == null && Simias.Service.Manager.LdapServiceEnabled == true)
				{
					Store store = Store.GetStore();
                        		Domain domain = store.GetDomain(store.DefaultDomain);
					string[] IDs = domain.GetMemberFamilyList(accessID);
                                	foreach(string id in IDs)
                                	{
						member = collection.GetMemberByID(id);
						if(member != null)
							break;	
                                	}
					if(member == null)
						rights = Rights.Admin;
	
				}
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
			log.Debug( "Before return" );

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
			return GetiFoldersByMember(userID, role, index, max, accessID, false);
		}

		public static iFolderSet GetiFoldersByMember(string userID, MemberRole role, int index, int max, string accessID, bool adminrequest)
		{
			return GetiFoldersByMember(userID, role, DateTime.MinValue, SearchOperation.Contains, null, index, max, accessID, adminrequest);
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
			return GetiFoldersByMember(userID, role, operation, pattern, index, max, accessID, false);
		}

		public static iFolderSet GetiFoldersByMember(string userID, MemberRole role, SearchOperation operation, string pattern, int index, int max, string accessID, bool adminrequest)
		{
			return GetiFoldersByMember(userID, role, DateTime.MinValue, SearchOperation.Contains, pattern, index, max, accessID, adminrequest);
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
			return GetiFoldersByMember(userID, role, after, operation, pattern, index, max, accessID, false);
		}
		public static iFolderSet GetiFoldersByMember(string userID, MemberRole role, DateTime after, SearchOperation operation, string pattern, int index, int max, string accessID, bool adminrequest)
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

			string userid = null;
			Hashtable ht = new Hashtable();
			if( adminrequest == true && accessID != null)
			{
				userid = accessID;
				accessID = null;
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);
				Member groupadmin = domain.GetMemberByID(userid);
				ht = groupadmin.GetMonitoredUsers(true);
			}
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
					try
					{
						if( !adminrequest || (userid == null)||( ht.ContainsKey(ce.OwnerID)))
						{
							iFolder ifolder = new iFolder(ce, accessID);
							if( adminrequest )
							{
								if( userid == null)
									ifolder.Preference = 0xffff;
								else
									ifolder.Preference = iFolderUser.GetAdminRights(userid, ifolder.OwnerID);
							}
							list.Add(ifolder);
						}
					}
					catch(Exception ex)
					{
						log.Debug("Exception: {0}--{1}", ex.Message, ex.StackTrace);
					}
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
			
			CatalogEntry[] catalogEntries;
			ICSList searchList = null;
			int total = 0;

			// admin ID
			Domain domain = store.GetDomain(store.DefaultDomain);
			String adminID = domain.Owner.UserID;

			// search operator
			SearchOp searchOperation = SearchOp.Contains;

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
	
			ArrayList list = new ArrayList();
			int i=0;
			Rights rights = Impersonate(domain, accessID);
			if( accessID == adminID || rights.Equals(Rights.Admin) )	
			{
				searchList = Catalog.GetAllEntriesByName (pattern, searchOperation);
				total = searchList.Count;
				SearchState searchState = new SearchState( domain.ID, searchList.GetEnumerator() as ICSEnumerator, searchList.Count );
				if(index > 0)
					searchState.Enumerator.SetCursor(Simias.Storage.Provider.IndexOrigin.SET, index);
				foreach(ShallowNode sn in searchList)
				{
					if(max != 0 && i++ >= max )
						break;
					CatalogEntry cEntry = Catalog.ConvertToCataloEntry( sn );
					iFolder ifolder = new iFolder(cEntry, null);
					ifolder.Preference = iFolderUser.GetAdminRights(accessID, ifolder.OwnerID);
				   	list.Add(ifolder);			   
				}
			}
			else
			{
				catalogEntries = Catalog.GetAllEntriesByGroupAdminID(accessID, pattern, searchOperation, index, max, out total);
				foreach(CatalogEntry cEntry in catalogEntries)
				{
					iFolder ifolder = new iFolder(cEntry, null);
					ifolder.Preference = iFolderUser.GetAdminRights(accessID, ifolder.OwnerID);
				   	list.Add(ifolder);			   
				}
			}

			return new iFolderSet((iFolder[])list.ToArray(typeof(iFolder)), total);
		}

		/// <summary>
		/// Get Orphaned iFolders 
		/// </summary>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>A Set of iFolder Objects</returns>
		public static iFolderSet GetOrphanediFolders(SearchOperation operation, string pattern, int index, int max, string accessID  )
		{
			Store store = Store.GetStore();
			CatalogEntry[] catalogEntries;
			string OrphanedOwnerProperty = "OrphOwnerDN";
			string MemberProperty = "mid";

			// Get the default domain.
			Domain domain = store.GetDomain( store.DefaultDomain );

			if ( domain != null )
			{
				// Get all of the collections that have been orphaned.
					
				// search operator
				SearchOp searchOperation = SearchOp.Begins;

				 // match the pattern
				Regex regex = null;
				
				if ((pattern != null) && (pattern.Length > 0) && !pattern.Equals("*"))
				{

					switch(operation)
					{
						case SearchOperation.BeginsWith:
							searchOperation = SearchOp.Begins;
							pattern = "^" + pattern;
							break;

						case SearchOperation.EndsWith:
							searchOperation = SearchOp.Ends;
							pattern = pattern + "$";
							break;

						case SearchOperation.Equals:
							searchOperation = SearchOp.Equal;
							pattern = "^" + pattern + "$";
							break;

						case SearchOperation.Contains:
						default:
							searchOperation = SearchOp.Contains;
							break;
					}
				
					
					regex = new Regex(pattern, RegexOptions.IgnoreCase);
				}
				Simias.Storage.SearchPropertyList SearchPrpList = new Simias.Storage.SearchPropertyList();
				ArrayList list = new ArrayList();
				int i = 0;
				int total = 0;
				SearchPrpList.Add(OrphanedOwnerProperty, "*", SearchOp.Exists);
				SearchPrpList.Add(BaseSchema.ObjectName, pattern, searchOperation);
				if(accessID != null)
					SearchPrpList.Add(MemberProperty, accessID, SearchOp.Begins);
				ICSList searchList = Catalog.Search( SearchPrpList );
		
				total = searchList.Count;
				SearchState searchState = new SearchState( domain.ID, searchList.GetEnumerator() as ICSEnumerator, searchList.Count );
				if( index != 0 )
					searchState.Enumerator.SetCursor(Simias.Storage.Provider.IndexOrigin.SET, index);
				foreach(ShallowNode sn in searchList)
				{
					if(max != 0 && i++ >= max )
						break;
					list.Add(new iFolder(Catalog.ConvertToCataloEntry(sn), accessID));
				}
			
				return new iFolderSet((iFolder[])list.ToArray(typeof(iFolder)), total);
			}
			else 
				throw new Exception ("ifolder.cs : Can not get the default domain for the store. ");
			
		}

		/// <summary>
		/// check the orphaned property of an ifolder 
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>a string 'false' if the ifolder is not orphaned otherwise returns userID of prev owner</returns>

		public static string IsOrphanediFolder(string iFolderID, string AccessID)
		{
			string isorphaned = "";

			Store store = Store.GetStore();
			Collection c = store.GetCollectionByID(iFolderID);
			if (c == null)  throw new iFolderDoesNotExistException(iFolderID);
			Property p = c.Properties.GetSingleProperty( "OrphanedOwner" );
			if ( p != null )
			{
				//if (c.OrphanedOwner != null )//&& (!(c.PreviousOwner.Equals("PrevOwner"))))	
				//isorphaned = c.OrphanedOwner;
				isorphaned = p.Value as string;
				
			}	
			return isorphaned;
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

		/// <summary>
                /// Set the migrated flag for the iFolder to determine that the iFolder is a migrated one.
                /// </summary>
                /// <param name="ifolderID">The id of the iFolder.</param>
                /// <param name="MigrationSource">Determines from which source the iFolder is migrated. (whether from iFolder2 server or 3.2 server or anyother</param>
		/// <returns></returns>
                public static void SetMigratedFlag(string iFolderID, int MigrationSource)
		{
			Store store = Store.GetStore();
			Collection c = store.GetCollectionByID(iFolderID);

			if (c == null) throw new iFolderDoesNotExistException(iFolderID);

			c.MigratediFolder = MigrationSource;
			c.Commit();
		}

		public static int RestoreiFolderData(string url, string adminname, string adminpassword, string ifolderid, string relativepath, string basepath, int startindex, string LogLocation)
		{
			log.Info(" In the RestoreiFolderData webservice method.");
			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(ifolderid);
			col.RestoreStatus = 1;
			col.Commit();
			RestoreThread = new Thread( RestoreiFolderDataThread );
			RestoreParms parms = new RestoreParms(url, adminname, adminpassword, ifolderid, relativepath, basepath, startindex, LogLocation);
			RestoreThread.Priority = ThreadPriority.BelowNormal;
			//RestoreThread.Start(parms);
			RestoreThread.Start((Object)parms);
			return 0;
		}

		public static void RestoreiFolderDataThread(Object args)
		{
			log.Debug(" Entered RestoreiFolderDataThread...");
                        try
                        {
				RestoreParms arguments = (RestoreParms)args;
				string url = arguments.url;
				string relativepath = arguments.relativepath;
				int startindex = arguments.startindex;
				string adminname = arguments.adminname;
				string adminpassword = arguments.adminpassword;
				string ifolderid = arguments.ifolderid;
				string basepath = arguments.basepath;
				string LogLocation = arguments.LogLocation;
				string fileNALocation = Path.Combine(Path.GetDirectoryName(LogLocation),ifolderid+".notfound" );
				bool isFileExist = true;

				log.Debug("relativepath: {0} startindex: {1} basepath: {2} LogLocation: {3} fileNALogLocation:{4}", relativepath, startindex, basepath, LogLocation,fileNALocation);
				Logger FailedLog= new Logger(LogLocation);
				Logger FileNotFound= new Logger(fileNALocation);

				int count = 500;
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(ifolderid);
				SimiasWebService oldadmin = new SimiasWebService();
				oldadmin.Credentials = new NetworkCredential(adminname, adminpassword);
				oldadmin.PreAuthenticate = true;
				oldadmin.Url = url;

				int type= 0;
				try
				{
					if( relativepath != null && relativepath != string.Empty)
					{
						string path = Path.Combine(basepath, relativepath);
						if( File.Exists(path))
							type = 2;
						else if( Directory.Exists(path))
							type = 1;
						else
						{
							log.Debug("The path specified for the restore {0} does not exist. {0}", path);
							//TODO: check this flag and don't process below code if given path doesn't exist
							isFileExist = false;
						}
					}
				}
				catch { }
				try
				{
					int totalCount = 0;
					int CurrentCount = startindex;
					NodeEntrySet entryset = oldadmin.GetEntries(ifolderid, type, relativepath, startindex, count, null);
					totalCount = Convert.ToInt32(entryset.Count);
					log.Debug("The total count of entryset: {0} relativepath: {1} file type: {2}", entryset.Count, relativepath, type);
					//NodeEntry[] entries = (NodeEntry[])entryset.Items;
					col.TotalRestoreFileCount = totalCount;
					col.RestoreStatus = 2;
					col.Commit();
					if( entryset.Items ==null )
					{
						log.Info("The entry set has no entries object");
					}
					if( entryset.Items.Length == 0)
					{
						log.Info("The entry set has no entries object 1111");
					}
					string entryToUpload = null;
					while(entryset.Items !=null && entryset.Items.Length != 0 && CurrentCount < totalCount)
					{
						log.Info("Entered the while loop. The length of array: {0}", entryset.Items.Length);
						foreach(NodeEntry entry in entryset.Items)
						{
							log.Debug("Restoring --{0}--{1}--{2}---{3}--{4}--", ifolderid, entry.ID, entry.RelativePath, basepath, entry.Type.ToString());
							try
							{
								entryToUpload = Path.Combine(basepath, entry.RelativePath);
								if(!(File.Exists(entryToUpload)) && !(Directory.Exists(entryToUpload)))
								{
									FileNotFound.Write(string.Format("{0} {1} {2} {3} {4}", CurrentCount, entry.ID, entry.Length, entry.RelativePath, "NA"), false);
									log.Debug("{0} {1} {2} {3} {4}", CurrentCount, entry.ID, entry.Length, entry.RelativePath, "NA");

								}
								else
								{
									string file = relativepath +"/";
									int retval = -1;	
									if(entry.RelativePath.Equals(relativepath) || entry.RelativePath.Contains(file))
									{
										log.Debug(string.Format("relative path inside if is:{0}",entry.RelativePath));
										retval = store.RestoreData(ifolderid, entry.ID, entry.RelativePath, basepath, entry.Type, entry.Length);
									}	
									else
									{
										log.Debug(string.Format("relative path inside else is:{0}",entry.RelativePath));
										retval = 0;
									}	
									if( retval != 0)
									{
										log.Debug(string.Format("Failed: {0} {1} {2} {3} {4}", CurrentCount, entry.ID, entry.Length, entry.RelativePath, "failed"));
										FailedLog.Write(string.Format("{0} {1} {2} {3} {4}", CurrentCount, entry.ID, entry.Length, entry.RelativePath, "failed"), false);
									}
								}
							}
							catch(Exception e1)
							{
								log.Debug("Exception in RestoreiFolderData: {0}--{1}", e1.Message, e1.StackTrace);
							}
							CurrentCount++;
							if(CurrentCount%20 == 0)
							{
								// status update to collection object....
								col = store.GetCollectionByID(ifolderid);
								col.RestoredFileCount = CurrentCount;
								col.Commit();
							}
						}
						startindex+= entryset.Items.Length;
						try{

							entryset = oldadmin.GetEntries(ifolderid, type, relativepath, startindex, count, null);
							//entries = (NodeEntry[])entryset.Items;
						}catch(Exception ex)
						{
							log.Debug("Exception in GetEntrie: {0}--{1}", ex.Message, ex.StackTrace);
							break;
						}
					}
					col = store.GetCollectionByID(ifolderid);
					col.RestoredFileCount = CurrentCount;
					col.Commit();
				}/// end try
				catch(Exception ex)
				{
					log.Debug("Outer Exception in RestoreiFolderData: {0}--{1}", ex.Message, ex.StackTrace);
				}
				finally
				{
					col.RestoreStatus = 0;
					col.Commit();
				}
			 }
                        catch(Exception e1)
                        {
                                log.Debug(" Exception in RestoreThread. {0}--{1}", e1.Message, e1.StackTrace);
                        }

		}

		public static int GetRestoreStatusForCollection(string ifolderid, out int totalcount, out int finishedcount)
		{
			log.Info("In GetRestoreStatusForCollection {0}", ifolderid);
			int retval=-1;
			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(ifolderid);
			retval = col.RestoreStatus;
			totalcount = col.TotalRestoreFileCount;
			finishedcount = col.RestoredFileCount;
			if( retval == 1 || retval == 2)
			{
				if(RestoreThread == null || RestoreThread.IsAlive == false)
				{
					col.RestoreStatus = 3;
					col.Commit();
					retval = 3;
				}
			}
			if( RestoreThread == null || RestoreThread.IsAlive == false)
			{
				RestoreThread = null;
			}
			log.Debug("Exiting GetRestoreStatusForCollection: retval: {0} total count: {1} finished count: {2}", retval, totalcount, finishedcount);
			return retval;
		}

                        public static int RestoreFromFailedLog( string logpath, string iFolderID, iFolderServer oldserver, string oldpath, iFolderServer newserver, string newpath)
                        {
                                int retval = -1;
				Logger FailedLog = new Logger(logpath);
				Store store = Store.GetStore();
                                try
                                {
                                        if( File.Exists( logpath) == false)
                                                return retval;
                                        File.Move(logpath, logpath+".working");
                                        logpath += ".working";
                                        TextReader reader = (TextReader)File.OpenText( logpath);
                                        if( reader == null)
                                                return retval;  // unable to open file...
                                        string entry = null;
                                        char[] delimiter = {' '};
                                        while((entry = reader.ReadLine())!= null)
                                        {
                                                string[] entryset = entry.Split(delimiter);
                                                string index = entryset[0];
                                                string id = entryset[1];
                                                string length = entryset[2];
                                                string relativepath = null;
						string type = null;
                                                for( int i=3; i< entryset.Length-1; i++)
                                                {
                                                        /// The file path mioght have spaces. so concat entries
                                                        relativepath += entryset[i];
                                                }
                                                string fullpath = Path.Combine(oldpath, relativepath);
						if( Directory.Exists(fullpath))
							type = "DirNode";
						else if( File.Exists(fullpath))
							type = "FileNode";

						retval = store.RestoreData(iFolderID, id, relativepath, oldpath, type, Convert.ToInt32(length));
						if( retval != 0)
						{
							FailedLog.Write(string.Format("{0} {1} {2} {3} {4}", index, id, length, relativepath, "failed"), false);
						}
                                        }
                                        retval = 0;
                                }
                                catch(Exception ex)
                                {
                                        log.Debug(string.Format("Exception in restoring from failed log: {0}--{1}", ex.Message, ex.StackTrace));
                                }
                                return retval;
                        }


	}

        public class Logger
        {
                private string logFile;
                private StreamWriter stream;
                public Logger( string fileName)
                {
                        this.logFile = fileName;
                        stream = File.AppendText(logFile);
                }

                public Logger()
                {
                        this.logFile = null;
                }

                public string LogFile
                {
                        get
                        {
                                return this.logFile;
                        }
                }

                public void Write(string Message)
		{
			this.Write( Message, true);
		}
                public void Write(string Message, bool timestring)
                {
                        try
                        {
                                if( this.logFile != null && this.logFile != string.Empty)
                                {
                                        if( stream == null)
                                        {
                                                stream = File.AppendText(logFile);
                                        }
					if( timestring)
	                                        stream.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToString("f"), Message));
					else
						stream.WriteLine(Message);
                                        stream.Close();
                                        stream = null;
                                }
                        }
                        catch
                        {
                        }
                }

                public void Stop()
                {
                        try
                        {
                                if( this.logFile == null || this.logFile == string.Empty)
                                        return;
                                if( stream == null)
                                        return;
                                stream.Close();
                                stream = null;
                        }
                        catch
                        {
                        }
                }
        }
	
}
