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
*                 $Author: Johnny Jacob <jjohnny@novell.com>
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

using System.Web.Services.Protocols;
using System.Xml;

using Simias;
using Simias.Storage;

namespace Simias.Discovery
{
	[Serializable]
        public class CollectionInfo
	{
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

			this.ID = c.ID;
			this.Name = c.Name;
			this.Description = GetStringProperty(c, PropertyTags.Description);
			this.DomainID = c.Domain;
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
	}
}
