/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
        [WebService(Namespace="http://novell.com/simias/discovery/")]
	public class DiscoveryService : System.Web.Services.WebService
	{
//		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(DiscoveryService));

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

                //get all the members in this collection
		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
		public string[] GetAllMembersOfCollection ( string CollectionID )
		{
		        CatalogEntry entry = Catalog.GetEntryByCollectionID( CollectionID );
			return entry.UserIDs;
		}

		[WebMethod(EnableSession=true)]
		[SoapDocumentMethod]
 	        public void RemoveMemberFromCollection( string collectionID, string userID)
 		{
 		}
	}
}
