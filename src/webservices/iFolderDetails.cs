/*****************************************************************************
* Copyright © [2007-08] Unpublished Work of Novell, Inc. All Rights Reserved.
*
* THIS IS AN UNPUBLISHED WORK OF NOVELL, INC.  IT CONTAINS NOVELL'S CONFIDENTIAL, 
* PROPRIETARY, AND TRADE SECRET INFORMATION.	NOVELL RESTRICTS THIS WORK TO 
* NOVELL EMPLOYEES WHO NEED THE WORK TO PERFORM THEIR ASSIGNMENTS AND TO 
* THIRD PARTIES AUTHORIZED BY NOVELL IN WRITING.  THIS WORK MAY NOT BE USED, 
* COPIED, DISTRIBUTED, DISCLOSED, ADAPTED, PERFORMED, DISPLAYED, COLLECTED,
* COMPILED, OR LINKED WITHOUT NOVELL'S PRIOR WRITTEN CONSENT.  USE OR 
* EXPLOITATION OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE 
* PERPETRATOR TO CRIMINAL AND  CIVIL LIABILITY.
*
* Novell is the copyright owner of this file.  Novell may have released an earlier version of this
* file, also owned by Novell, under the GNU General Public License version 2 as part of Novell's 
* iFolder Project; however, Novell is not releasing this file under the GPL.
*
*-----------------------------------------------------------------------------
*
*                 Novell iFolder Enterprise
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
