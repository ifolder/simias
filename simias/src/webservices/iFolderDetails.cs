/***********************************************************************
 *  $RCSfile: iFolder.cs,v $
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
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

using Simias.Client;
using Simias.Storage;
using Simias.Web;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder
	/// </summary>
	[Serializable]
	public class iFolderDetails : iFolder
	{
		/// <summary>
		/// Number of Files
		/// </summary>
		public int FileCount = 0;

		/// <summary>
		/// Number of Directories
		/// </summary>
		public int DirectoryCount = 0;

		/// <summary>
		/// The iFolder Managed Path
		/// </summary>
		public string ManagedPath;

		/// <summary>
		/// The iFolder Un-Manged Path
		/// </summary>
		public string UnManagedPath;

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderDetails()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="c">The iFolder Collection</param>
		/// <param name="accessID">The Access User ID</param>
		protected iFolderDetails(Collection c, string accessID)
			: base(c, accessID)
		{
			this.FileCount = c.GetNodesByType(NodeTypes.FileNodeType).Count;
			this.DirectoryCount = c.GetNodesByType(NodeTypes.DirNodeType).Count;

			// paths
			this.ManagedPath = c.ManagedPath;

			DirNode dirNode = c.GetRootDirectory();
			
			if (dirNode != null)
			{
				this.UnManagedPath = dirNode.GetFullPath(c);
			}
		}

		/// <summary>
		/// Get an iFolder Details
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolderDetails Object</returns>
		public static iFolderDetails GetiFolderDetails(string ifolderID, string accessID)
		{
			Store store = Store.GetStore();
			
			Collection c = store.GetCollectionByID(ifolderID);
			
			if (c == null)  throw new iFolderDoesNotExistException(ifolderID);

			return new iFolderDetails(c, accessID);
		}
	}
}
