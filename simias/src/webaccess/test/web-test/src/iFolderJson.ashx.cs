/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
using System.Net;
using System.Data;

using iFolderWebClient.Simias10;

using Jayrock.Json;
using Jayrock.Json.Rpc;
using Jayrock.Json.Rpc.Web;

namespace iFolderWebClient
{
	/// <summary>
	/// iFolder JSON RPC Handler
	/// </summary>
	[ JsonRpcHelp("iFolder JSON RPC Handler") ]
	public class iFolderJson : JsonRpcHandler, IRequiresSessionState
	{
		/// <summary>
		/// Get Session Information
		/// </summary>
		/// <returns></returns>
		[ JsonRpcMethod("getSessionInfo") ]
		public DataTable GetSessionInfo()
		{
			iFolderWeb web = (iFolderWeb) Session["Connection"];

			iFolderUser user = web.GetUser();

			DataTable table = new DataTable("ifolders");
			table.Columns.Add("userId");
			table.Columns.Add("userName");
			table.Columns.Add("fullName");
			table.Columns.Add("rights");

			DataRow row = table.NewRow();
			
			row["userId"] = user.UserID;
			row["userName"] = user.UserName;
			row["fullName"] = user.FullName;
			row["rights"] = user.Rights;

			table.Rows.Add(row);

			return table;
		}

		/// <summary>
		/// Get the iFolders
		/// </summary>
		/// <returns></returns>
		[ JsonRpcMethod("getiFolders") ]
		public DataTable GetiFolders()
		{
			iFolderWeb web = (iFolderWeb) Session["Connection"];

			iFolder[] ifolders = web.GetiFolders();

			DataTable table = new DataTable("ifolders");
			table.Columns.Add("id");
			table.Columns.Add("name");
			table.Columns.Add("description");
			table.Columns.Add("ownerId");
			table.Columns.Add("ownerName");
			table.Columns.Add("managedPath");
			table.Columns.Add("unmanagedPath");
			table.Columns.Add("size");
			table.Columns.Add("rights");
			table.Columns.Add("context");
			table.Columns.Add("type");

			foreach(iFolder ifolder in ifolders)
			{
				DataRow row = table.NewRow();
				
				row["id"] = ifolder.ID;
				row["name"] = ifolder.Name;
				row["description"] = ifolder.Description;
				row["ownerID"] = ifolder.OwnerID;
				row["ownerName"] = ifolder.OwnerName;
				row["managedPath"] = ifolder.ManagedPath;
				row["unmanagedPath"] = ifolder.UnManagedPath;
				row["size"] = ifolder.Size;
				row["rights"] = ifolder.Rights;
				row["context"] = String.Format("{0}:{1}", ifolder.ID, ifolder.Name);
				row["type"] = "ifolder";

				table.Rows.Add(row);
			}

			return table;
		}

		/// <summary>
		/// Get the Entries
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		[ JsonRpcMethod("getEntries") ]
		public DataTable GetEntries(string ifolderID, string path)
		{
			iFolderWeb web = (iFolderWeb) Session["Connection"];

			// parent
			iFolderEntry parent = web.GetEntryByPath(ifolderID, path);

			// children
			iFolderEntry[] entries = web.GetEntriesByParent(ifolderID, parent.ID);

			DataTable table = new DataTable("entries");
			table.Columns.Add("id");
			table.Columns.Add("name");
			table.Columns.Add("description");
			table.Columns.Add("path");
			table.Columns.Add("size");
			table.Columns.Add("modifiedTime");
			table.Columns.Add("isRoot");
			table.Columns.Add("isDirectory");
			table.Columns.Add("hasChildren");
			table.Columns.Add("ifolderId");
			table.Columns.Add("parentId");
			table.Columns.Add("context");
			table.Columns.Add("type");

			foreach(iFolderEntry entry in entries)
			{
				DataRow row = table.NewRow();
				
				row["id"] = entry.ID;
				row["name"] = entry.Name;
				row["description"] = "";
				row["path"] = entry.Path;
				row["size"] = entry.IsDirectory ? "" : entry.Size.ToString();
				row["modifiedTime"] = entry.ModifiedTime;
				row["isRoot"] = entry.IsRoot;
				row["isDirectory"] = entry.IsDirectory;
				row["hasChildren"] = entry.HasChildren;
				row["ifolderId"] = entry.iFolderID;
				row["parentId"] = entry.ParentID;
				row["context"] = String.Format("{0}:{1}", entry.iFolderID, entry.Path);
				row["type"] = entry.IsDirectory ? "folder" : "file";

				table.Rows.Add(row);
			}

			return table;
		}

		/// <summary>
		/// Get iFolder Details
		/// </summary>
		/// <returns></returns>
		[ JsonRpcMethod("getiFolderDetails") ]
		public DataTable GetiFolderDetails(string ifolderID)
		{
			iFolderWeb web = (iFolderWeb) Session["Connection"];

			iFolder ifolder = web.GetiFolder(ifolderID);

			DataTable table = new DataTable("ifolder");
			table.Columns.Add("name");
			table.Columns.Add("description");
			table.Columns.Add("ownerName");
			table.Columns.Add("rights");
			table.Columns.Add("size");

			DataRow row = table.NewRow();
			
			row["name"] = ifolder.Name;
			row["description"] = ifolder.Description;
			row["ownerName"] = ifolder.OwnerName;
			row["rights"] = ifolder.Rights;
			row["size"] = ifolder.Size;

			table.Rows.Add(row);

			return table;
		}

		/// <summary>
		/// Get Entry Details
		/// </summary>
		/// <returns></returns>
		[ JsonRpcMethod("getEntryDetails") ]
		public DataTable GetEntryDetails(string ifolderID, string path)
		{
			iFolderWeb web = (iFolderWeb) Session["Connection"];

			iFolderEntry entry = web.GetEntryByPath(ifolderID, path);

			DataTable table = new DataTable("entry");
			table.Columns.Add("name");
			table.Columns.Add("size");

			DataRow row = table.NewRow();
			
			row["name"] = entry.Name;
			row["size"] = entry.Size;

			table.Rows.Add(row);

			return table;
		}

		/// <summary>
		/// Create a new iFolder
		/// </summary>
		/// <param name="name"></param>
		/// <param name="description"></param>
		[ JsonRpcMethod("createiFolder") ]
		public void CreateiFolder(string name, string description)
		{
			iFolderWeb web = (iFolderWeb) Session["Connection"];

			web.CreateiFolder(name, description);
		}

		/// <summary>
		/// Create a new Folder
		/// </summary>
		/// <param name="context"></param>
		/// <param name="name"></param>
		[ JsonRpcMethod("createFolder") ]
		public void CreateFolder(string context, string name)
		{
			iFolderWeb web = (iFolderWeb) Session["Connection"];

			// trim
			context = context.Trim(new char[] { '/' });
			int index = context.IndexOf('/');

			// ifolder and path
			string ifolderID = context.Substring(0, index);
			string path = context.Substring(index + 1);
			iFolderEntry entry = web.GetEntryByPath(ifolderID, path);
			string parentID;

			if (entry.IsDirectory)
			{
				parentID = entry.ID;
			}
			else
			{
				parentID = entry.ParentID;
			}

			web.CreateDirectoryEntry(ifolderID, parentID, name);
		}
	}
}
