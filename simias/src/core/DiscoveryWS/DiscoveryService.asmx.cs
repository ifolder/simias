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
		/// DirNode Name
		/// </summary>
		public string UserRights;

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

			this.ID = entry.ID;
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

		}
	        public CollectionInfo ( string CollectionID, string UserID )
		{
		        Collection c = Store.GetStore().GetCollectionByID( CollectionID );
		        CatalogEntry entry = Catalog.GetEntryByCollectionID( CollectionID );

			this.ID = entry.ID;
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

			Member member = c.GetMemberByID (UserID);
			this.MemberNodeID = member.ID;
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
		        Collection collection = Store.GetStore().GetCollectionByID( CollectionID );
			Member member = collection.GetMemberByID( UserID );

			if ( member != null )
			{
				collection.Commit( collection.Delete( member ) );
				return true;
			}
			return false;
 		}

		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public CollectionInfo GetCollectionInfo ( string CollectionID )
		{
		        return new CollectionInfo ( CollectionID );
		}

		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public string GetCollectionDirNodeID ( string CollectionID )
		{
			Collection collection = Store.GetStore().GetCollectionByID( CollectionID );
		        return collection.GetRootDirectory().ID;
		}

	}
}
