/***********************************************************************
 *  $RCSfile: FileHandler.cs,v $
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
using System.IO;
using System.Web;
using System.Web.SessionState;

using Simias;
using Simias.Client;
using Simias.Storage;

namespace iFolder.WebService
{
	/// <summary>
	/// File Handler
	/// </summary>
	public abstract class FileHandler : IHttpHandler, IRequiresSessionState
	{
		/// <summary>
		/// iFolder ID
		/// </summary>
		protected string ifolderID;
		
		/// <summary>
		/// Entry ID
		/// </summary>
		protected string entryID;
		
		/// <summary>
		/// Entry Path
		/// </summary>
		protected string entryPath;
		
		/// <summary>
		/// Access ID
		/// </summary>
		protected string accessID;

		/// <summary>
		/// Access Member
		/// </summary>
		protected Member member;

		/// <summary>
		/// Store Object
		/// </summary>
		protected Store store;
		
		/// <summary>
		/// Collection Object
		/// </summary>
		protected Collection collection;
		
		/// <summary>
		/// File Node Object
		/// </summary>
		protected FileNode node;

		/// <summary>
		/// Filename
		/// </summary>
		protected string filename;
		
		/// <summary>
		/// File Path
		/// </summary>
		protected string filePath;

		/// <summary>
		/// Access Log
		/// </summary>
		protected SimiasAccessLogger log;

		/// <summary>
		/// Constructor
		/// </summary>
		public FileHandler()
		{
		}

		#region IHttpHandler Members

		/// <summary>
		/// Process the Request
		/// </summary>
		/// <param name="context">The HttpContext object.</param>
		public abstract void ProcessRequest(HttpContext context);

		/// <summary>
		/// Initialize the Request
		/// </summary>
		/// <param name="context">The HttpContext object.</param>
		protected void Initialize(HttpContext context)
		{
			// query
			ifolderID = context.Request.QueryString["iFolder"];
			entryID = context.Request.QueryString["Entry"];
			entryPath = context.Request.QueryString["Path"];
			
			// authentication
			accessID = context.User.Identity.Name;

			if ((accessID == null) || (accessID.Length == 0))
			{
				throw new AuthenticationException();
			}

			// store
			store = Store.GetStore();

			// collection
			collection = store.GetCollectionByID(ifolderID);

			if (collection == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}
			
			// member
			member = collection.GetMemberByID(accessID);

			// does member exist?
			if (member == null)
			{
				throw new MemberDoesNotExistException(accessID);
			}
			
			// impersonate
			iFolder.Impersonate(collection, accessID);

			// log
			log = new SimiasAccessLogger(member.Name, collection.ID);

			// node
			Node n = null;

			// use the path
			if ((entryPath != null) && (entryPath.Length != 0))
			{
				n = iFolderEntry.GetEntryByPath(collection, entryPath);
			}

			// use the id
			if ((entryID != null) && (entryID.Length != 0))
			{
				n = collection.GetNodeByID(entryID);
			}

			// check node
			if (n != null)
			{
				// is the node a file
				if (!n.IsBaseType(NodeTypes.FileNodeType))
				{
					throw new FileDoesNotExistException(entryID);
				}

				// file
				node = (FileNode)n;

				filename = node.GetFileName();
				filePath = node.GetFullPath(collection);
			}
		}

		/// <summary>
		/// Is this instance reusable?
		/// </summary>
		public bool IsReusable
		{
			get { return false; }
		}

		#endregion
	}
}
