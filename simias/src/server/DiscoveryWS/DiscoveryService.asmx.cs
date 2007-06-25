/***********************************************************************
 *  $RCSfile$
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
 *  Author: Johnny Jacob <jjohnny@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Client;
using Simias.Server;

namespace Simias.DiscoveryService.Web
{
	[Serializable]
        public class CatalogInfo
	{
		/// <summary>
		/// The iFolder ID
		/// </summary>
		public string CollectionID;

		/// <summary>
		/// HostNode ID of Collection's location
		/// </summary>
		public string HostID;
	    
		/// <summary>
		/// List of members in this collection.
		/// </summary>
	        public string[] UserIDs;

		/// <summary>
		/// Name of the collection
		/// </summary>
	        public string CollectionName;

		/// <summary>
		/// ID of the owner of the collection
		/// </summary>
	        public string CollectionOwnerID;

		/// <summary>
		/// Name of the collection
		/// </summary>
	        public long CollectionSize;

	        public CatalogInfo ()
		{
		}

	        public CatalogInfo ( CatalogEntry ce)
		{
		        this.CollectionID = ce.CollectionID;
			this.HostID = ce.HostID;
			this.UserIDs = ce.UserIDs;
			this.CollectionName = ce.Name;
			this.CollectionSize = ce.CollectionSize;
			this.CollectionOwnerID = ce.OwnerID;
		}
	}

	[Serializable]
	public class CollectionInfo
	{
		/// <summary>
		/// The iFolder ID
		/// </summary>
		public string ID;
		
		/// <summary>
		/// The iFolder ID
		/// </summary>
		public string CollectionID;

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
		/// iFolder Created Time
		/// </summary>
 		public DateTime Created = DateTime.MinValue;

		/// <summary>
		/// iFolder Last Modified Time
		/// </summary>
		public DateTime LastModified = DateTime.MinValue;

		/// <summary>
		/// Number of Members
		/// </summary>
		public int MemberCount = 0;

		/// <summary>
		/// HostNode ID
		/// </summary>
		public string HostID;

		/// <summary>
		/// DirNode ID
		/// </summary>
		public string DirNodeID;

		/// <summary>
		/// DirNode Name
		/// </summary>
		public string DirNodeName;

		/// <summary>
		/// DirNode ID
		/// </summary>
		public string MemberNodeID;

		/// <summary>
		/// DirNode ID
		/// </summary>
		public string MemberUserID;

		/// <summary>
		/// DirNode Name
		/// </summary>
		public string UserRights;
		public string encryptionAlgorithm;

	        public CollectionInfo ()
		{
		}
		/// <summary>
		/// Get a string property from a Node.
		/// </summary>
		/// <param name="node">The node object.</param>
		/// <param name="property">The property name.</param>
		/// <returns></returns>
		private string GetStringProperty(Node node, string property)
		{
			string result = null;

			Property p = node.Properties.GetSingleProperty(property);

			if ((p != null) && (p.Type == Syntax.String))
			{
				result = (string)p.Value;
			}

			return result;
		}

		/// <summary>
		/// Get a DateTime property from a Node.
		/// </summary>
		/// <param name="node">The node object.</param>
		/// <param name="property">The property name.</param>
		/// <returns></returns>
		private static DateTime GetDateTimeProperty(Node node, string property)
		{
			DateTime result = DateTime.MinValue;

			Property p = node.Properties.GetSingleProperty(property);

			if ((p != null) && (p.Type == Syntax.DateTime))
			{
				result = (DateTime)p.Value;
			}

			return result;
		}

	        public CollectionInfo ( string CollectionID )
		{
		        Collection c = Store.GetStore().GetCollectionByID( CollectionID );

		        CatalogEntry entry = Catalog.GetEntryByCollectionID( CollectionID );

//			this.ID = entry.ID;
			this.ID = c.ID;
			this.CollectionID = c.ID;
			this.Name = c.Name;
			this.Description = GetStringProperty(c, PropertyTags.Description);
			this.DomainID = c.Domain;
			this.HostID = c.HostID;
			this.DirNodeID = c.GetRootDirectory().ID;
			this.DirNodeName = c.GetRootDirectory().Name;
			this.Size = c.StorageSize;
			this.Created = GetDateTimeProperty(c, PropertyTags.NodeCreationTime);
			this.LastModified = GetDateTimeProperty(c, PropertyTags.JournalModified);
			this.MemberCount = c.GetMemberList().Count;

			this.OwnerID = c.Owner.UserID;
			Domain domain = Store.GetStore().GetDomain(this.DomainID);
			Member domainMember = domain.GetMemberByID(this.OwnerID);
			this.OwnerUserName = domainMember.Name;
			string fullName = domainMember.FN;
			this.OwnerFullName = (fullName != null) ? fullName : this.OwnerUserName;
                       if( c.EncryptionAlgorithm != null)
                               this.encryptionAlgorithm = c.EncryptionAlgorithm;
                       else
                               this.encryptionAlgorithm = "No Encryption";


		}

	        public CollectionInfo ( string CollectionID, string UserID )
		{
		        Collection c = Store.GetStore().GetCollectionByID( CollectionID );

		        CatalogEntry entry = Catalog.GetEntryByCollectionID( CollectionID );

//			this.ID = entry.ID;
			this.ID = c.ID;
			this.CollectionID = c.ID;
			this.Name = c.Name;
			this.Description = GetStringProperty(c, PropertyTags.Description);
			this.DomainID = c.Domain;

//			this.HostID = c.HostID;
			this.HostID = HostNode.GetLocalHost().UserID;
			this.DirNodeID = c.GetRootDirectory().ID;
			this.DirNodeName = c.GetRootDirectory().Name;
			this.Size = c.StorageSize;
			this.Created = GetDateTimeProperty(c, PropertyTags.NodeCreationTime);
			this.LastModified = GetDateTimeProperty(c, PropertyTags.JournalModified);
			this.MemberCount = c.GetMemberList().Count;

			this.OwnerID = c.Owner.UserID;
			Domain domain = Store.GetStore().GetDomain(this.DomainID);
			Member domainMember = domain.GetMemberByID(this.OwnerID);
			this.OwnerUserName = domainMember.Name;
			string fullName = domainMember.FN;
			this.OwnerFullName = (fullName != null) ? fullName : this.OwnerUserName;
                       if( c.EncryptionAlgorithm != null)
                               this.encryptionAlgorithm = c.EncryptionAlgorithm;
                       else
                               this.encryptionAlgorithm = "No Encryption";


			Member member = c.GetMemberByID (UserID);
			this.MemberNodeID = member.ID;
			this.MemberUserID = member.UserID;
			this.UserRights = member.Rights.ToString();
		}

	}


        [WebService(Namespace="http://novell.com/simias/discovery/")]
	public class DiscoveryService : System.Web.Services.WebService
	{
		/// <summary>
		/// </summary>
		public DiscoveryService()
		{
		}
	        
                //get all the collections that this user is associated with.
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public string[] GetAllCollectionIDsByUser ( string UserID )
		{
  		        return Catalog.GetAllCollectionIDsByUserID( UserID );
		}

                //get all the collections that this user is associated with.
	        [WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public CatalogInfo[] GetAllCatalogInfoForUser ( string UserID )
		{
  		        CatalogEntry[] entries = Catalog.GetAllEntriesByUserID( UserID );
			CatalogInfo[] ci = new CatalogInfo [ entries.Length ];
			int x = 0;

			foreach (CatalogEntry ce in entries )
			{
			    ci [ x++ ] = new CatalogInfo ( ce );
			}

			return ci;
		}

                //get all the collections that this user is associated with.
	    [WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public ArrayList GetAllCollectionsByUser ( string UserID )
		{
		        ArrayList collectionList = new ArrayList ();
  		        string[] collectionIDs = Catalog.GetAllCollectionIDsByUserID( UserID );
			foreach (string id in collectionIDs )
			{
			    collectionList.Add (new CollectionInfo (id, UserID));
			}
			return collectionList;
		}

                //get all the members in this collection
		/// <summary>
		/// </summary>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public string[] GetAllMembersOfCollection ( string CollectionID )
		{
		        CatalogEntry entry = Catalog.GetEntryByCollectionID( CollectionID );
			return entry.UserIDs;
		}

		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
 	        public bool RemoveMemberFromCollection( string CollectionID, string UserID)
 		{
		        IsCollectionOnHost ( CollectionID );

		        Collection collection = Store.GetStore().GetCollectionByID( CollectionID );
			Member member = collection.GetMemberByID( UserID );
//			CatalogEntry entry = Catalog.GetEntryByCollectionID( CollectionID );

			if ( member != null )
			{
			        //Note : When collection is deleted, information in catalog will be updated.
//				entry.RemoveMember(member.UserID);
				collection.Commit( collection.Delete( member ) );
				return true;
			}
			return false;
 		}

                [WebMethod(EnableSession=true)]
                [SoapDocumentMethod]
                public void DeleteCollectionInCatalog( string CollectionID)
                {
                        IsCollectionOnHost ( CollectionID );

                        Collection collection = Store.GetStore().GetCollectionByID( CollectionID );

                        if ( collection != null )
                        {
				Catalog.DeleteEntryByCollectionID(CollectionID);
                        }
                        return ;
                }

		/// <summary>
		/// Get Collection Information
		/// </summary>
		/// <param name="collectionID">The ID of the collection to check for.</param>
		/// <returns>CollectionInfo for the collection</returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public CollectionInfo GetCollectionInfo ( string CollectionID, string UserID)
		{
		        IsCollectionOnHost ( CollectionID );

		        return new CollectionInfo ( CollectionID, UserID );
		}

		/// <summary>
		/// Get Collection Information
		/// </summary>
		/// <param name="collectionID">The ID of the collection to check for.</param>
		/// <returns>CollectionInfo for the collection</returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public CollectionInfo[] GetAllCollectionInfo ( string[] CollectionIDs, string UserID)
		{
			ArrayList CollectionInfos = new ArrayList();

			foreach( string CollectionID in CollectionIDs )
			{
			        try 
				{
				    CollectionInfos.Add( GetCollectionInfo ( CollectionID, UserID  ) );
				} 
				catch ( Exception e )
				{
				    //Nothing. We just continue .. :-)
				}
			}

			return CollectionInfos.ToArray( typeof( CollectionInfo ) ) as CollectionInfo[];
		}

		/// <summary>
		/// Fetches the DirNodeID for Collection
		/// </summary>
		/// <param name="collectionID">The ID of the collection</param>
		/// <returns> DirNode ID of the collectio. </returns>
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public string GetCollectionDirNodeID ( string CollectionID )
		{
		        IsCollectionOnHost ( CollectionID );

			Collection collection = Store.GetStore().GetCollectionByID( CollectionID );
		        return collection.GetRootDirectory().ID;
		}

		/// <summary>
		/// Determins if the collection is on this Host.
		/// </summary>
		/// <param name="collectionID">The ID of the collection to check for.</param>
		/// <returns>Throws a exception otherwise</returns>
	        private void IsCollectionOnHost (string CollectionID)
		{
       			if ( Store.GetStore().GetCollectionByID( CollectionID ) == null )
			{
			        throw new Exception ("Collection not on this Host.");
			}
		}
	}
}
