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
		/// File length
		/// </summary>
		protected long Length;
		

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
				
			string ppath = entryPath ;
			
			string [] ConversionTable = {"&", "amp@:quot"};
			if(ppath != null)
			{
				for(int index=0 ; index < ConversionTable.Length ; index+=2)
				{
					ppath = ppath.Replace(ConversionTable[index+1], ConversionTable[index]);
                        	}
			}

			entryPath = ppath;
			
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
                        if(member == null && Simias.Service.Manager.LdapServiceEnabled == true)
                        {
                        	Domain domain = store.GetDomain(store.DefaultDomain);
                        	string[] IDs = domain.GetMemberFamilyList(accessID);
                        	foreach(string id in IDs)
                        	{
                        		member = collection.GetMemberByID(id);
                        		if(member != null)
                        		break;
                        	}
                        }
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
