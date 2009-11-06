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
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Policy;
using Simias.Sync;
using Simias.Sync.Delta;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder Entry Result Set
	/// </summary>
	[Serializable]
	public class iFolderEntrySet
	{
		/// <summary>
		/// An Array of Entries
		/// </summary>
		public iFolderEntry[] Items;

		/// <summary>
		/// The Total Number of Entries
		/// </summary>
		public int Total;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public iFolderEntrySet()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="items"></param>
		/// <param name="total"></param>
		public iFolderEntrySet(iFolderEntry[] items, int total)
		{
			this.Items = items;
			this.Total = total;
		}
	}

	/// <summary>
	/// An iFolder Entry
	/// </summary>
	[Serializable]
	public class iFolderEntry
	{
		/// <summary>
		/// The iFolder Entry ID
		/// </summary>
		public string ID = null;
		
		/// <summary>
		/// The iFolder Entry Name
		/// </summary>
		public string Name = null;

		/// <summary>
		/// The iFolder Entry Relative Path
		/// </summary>
		public string Path = null;

		/// <summary>
		/// The iFolder ID
		/// </summary>
		public string iFolderID = null;

		/// <summary>
		/// The iFolder ID
		/// </summary>
		public string ParentID = null;

		/// <summary>
		/// Is the iFolder Entry a Directory?
		/// </summary>
		public bool IsDirectory = false;

		/// <summary>
		/// Is the iFolder Entry a Root Directory?
		/// </summary>
		public bool IsRoot = false;

		/// <summary>
		/// Does the iFolder Entry (Directory) Have Children?
		/// </summary>
		public bool HasChildren = false;

		/// <summary>
		/// iFolder Entry Last Modified Time
		/// </summary>
		public DateTime LastModified = DateTime.MinValue;

		/// <summary>
		/// iFolder Entry Size
		/// </summary>
		public long Size = 0;

		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderEntry()
		{
		}

		/// <summary>
		/// Get an iFolder Entry
		/// </summary>
		/// <param name="c">The Collection Object</param>
		/// <param name="n">The Node Object</param>
		/// <returns>An iFolderEntry Object</returns>
		private static iFolderEntry GetEntry(Collection c, Node n)
		{
			iFolderEntry entry = new iFolderEntry();

			entry.ID = n.ID;
			entry.Name = n.Name;
			entry.iFolderID = c.ID;
			
			try
			{
				entry.ParentID = (n.Properties.GetSingleProperty(PropertyTags.Parent).Value as Relationship).NodeID;
			}
			catch
			{
				// ignore
			}

			// file node
			if (n.IsBaseType(NodeTypes.FileNodeType))
			{
				FileNode fileNode = (FileNode)FileNode.NodeFactory(c, n);
				
				entry.Path = fileNode.GetRelativePath();
				entry.LastModified = fileNode.LastWriteTime;
				entry.Size = fileNode.Length;
			}
			
			// dir node
			else if (n.IsBaseType(NodeTypes.DirNodeType))
			{
				DirNode dirNode = (DirNode)DirNode.NodeFactory(c, n);

				entry.Path = dirNode.GetRelativePath();
				entry.LastModified = dirNode.CreationTime;
				entry.IsDirectory = true;
				entry.IsRoot = dirNode.IsRoot;
				entry.HasChildren = dirNode.HasChildren(c);

				entry.Size = (long)dirNode.GetSize(c);
			}
			
			// bad node
			else
			{
				throw new EntryDoesNotExistException(n.ID);
			}

			return entry;
		}

		/// <summary>
		/// Get an iFolder Entry
		/// </summary>
		/// <param name="ifolderID">The ID of the iFolder.</param>
		/// <param name="entryID">The ID of the Entry.</param>
		/// <param name="accessID">The Access User ID.</param>
		/// <returns>An iFolderEntry Object</returns>
		public static iFolderEntry GetEntry(string ifolderID, string entryID, string accessID)
		{
			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}
			
			// impersonate
			iFolder.Impersonate(c, accessID);

			Node n = c.GetNodeByID(entryID);

			if (n == null)
			{
				throw new EntryDoesNotExistException(entryID);
			}

			return iFolderEntry.GetEntry(c, n);
		}

		/// <summary>
		/// Get an iFolder Entry By Relative Path
		/// </summary>
		/// <param name="ifolderID">The ID of the iFolder.</param>
		/// <param name="entryPath">The ID of the Entry.</param>
		/// <param name="accessID">The Access User ID.</param>
		/// <returns>An iFolderEntry Object</returns>
		public static iFolderEntry GetEntryByPath(string ifolderID, string entryPath, string accessID)
		{
			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}
			
			// impersonate
			iFolder.Impersonate(c, accessID);

			Node n = GetEntryByPath(c, entryPath);

			if (n == null)
			{
				throw new EntryDoesNotExistException(entryPath);
			}

			return iFolderEntry.GetEntry(c, n);
		}

		/// <summary>
		/// Get an iFolder Entry By Relative Path
		/// </summary>
		/// <param name="c"></param>
		/// <param name="entryPath"></param>
		/// <returns></returns>
		internal static Node GetEntryByPath(Collection c, string entryPath)
		{
			Node n = null;

			entryPath = entryPath.Trim(new char[] { '/' });

			ICSList children = c.Search(PropertyTags.FileSystemPath, entryPath, SearchOp.Equal);

			foreach(ShallowNode sn in children)
			{
				Node child = c.GetNodeByID(sn.ID);

				if (child.IsBaseType(NodeTypes.FileNodeType) || child.IsBaseType(NodeTypes.DirNodeType))
				{
					n = child;
					break;
				}
			}
		
			return n;
		}

		/// <summary>
		/// Get iFolder Entries
		/// </summary>
		/// <param name="ifolderID">The ID of the iFolder.</param>
		/// <param name="entryID">The ID of the Parent Entry.</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID.</param>
		/// <returns>A Set of iFolderEntry Objects</returns>
		public static iFolderEntrySet GetEntries(string ifolderID, string entryID, int index, int max, string accessID)
		{
			int total = 0;

			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}
			
			// impersonate
			iFolder.Impersonate(c, accessID);

			// build the result list
			ArrayList list = new ArrayList();

			if (!ifolderID.Equals(entryID))
			{
				// not a collection
				ICSList children = c.Search(PropertyTags.Parent, new Relationship(c.ID, entryID));

				// sort the list
				ArrayList sortList = new ArrayList();
			
				foreach(ShallowNode sn in children)
				{
					sortList.Add(sn);
				}
			
				sortList.Sort(new EntryComparer());

				int i = 0;

				foreach(ShallowNode sn in sortList)
				{
					if (sn.IsBaseType(NodeTypes.FileNodeType) || sn.IsBaseType(NodeTypes.DirNodeType))
					{
						if ((i >= index) && (((max <= 0) || i < (max + index))))
						{
							Node n = c.GetNodeByID(sn.ID);

							list.Add(iFolderEntry.GetEntry(c, n));
						}

						++i;
					}
				}

				// save total
				total = i;
			}
			else
			{
				// a collection
				list.Add(iFolderEntry.GetEntry(c, c.GetRootDirectory()));

				// save total
				total = 1;
			}

			return new iFolderEntrySet((iFolderEntry[])list.ToArray(typeof(iFolderEntry)), total);
		}
       
	/* 
		/// <summary>
		/// Get iFolder Entries
		/// </summary>
		/// <param name="ifolderID">The ID of the iFolder.</param>
		/// <param name="type">Type of serarch</param>
		/// <param name="relPath">relative Path of the intial object.</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID.</param>
		/// <returns>A Set of iFolderEntry Objects</returns>
		public static iFolderEntrySet GetEntries(string ifolderID, int type, string relPath, int index, int max, string accessID)
		{
			int total = 0;
			Simias.Storage.SearchPropertyList SearchPrpList = new Simias.Storage.SearchPropertyList();

			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}
			
			// impersonate
			iFolder.Impersonate(c, accessID);

			// build the result list
			ArrayList list = new ArrayList();

			if(type  <= 0 || type > 2)
				SearchPrpList.Add(PropertyTags.FileSystemPath,"*",  SearchOp.Begins);
			else if(type == 1)
				SearchPrpList.Add(PropertyTags.FileSystemPath,	relPath,  SearchOp.Begins);
			else if(type == 2)
				SearchPrpList.Add(PropertyTags.FileSystemPath,	relPath,  SearchOp.Equal);

			SearchPrpList.Add(BaseSchema.ObjectType, NodeTypes.MemberType, SearchOp.Not_Equal);
			ICSList searchList = c.Search(SearchPrpList);

			total = searchList.Count;
			SearchState searchState = new SearchState( domain.ID, searchList.GetEnumerator() as ICSEnumerator, searchList.Count );

			if(index > 0)
				searchState.Enumerator.SetCursor(Simias.Storage.Provider.IndexOrigin.SET, index);


			foreach(ShallowNode sn in searchList)
			{
				if(max != 0 && i++ >= max )
					break;
				try
				{
					Node n = c.GetNodeByID(sn.ID);
					list.Add(iFolderEntry.GetEntry(c, n));
				}
				catch (Exception ex)
				{
					log.Debug("Error: "+ex.Message);
					log.Debug("Error Trace: "+ex.StackTrace);
				}
			}
			return new iFolderEntrySet((iFolderEntry[])list.ToArray(typeof(iFolderEntry)), total);
		}

	*/

        /// <summary>
        /// Get all matched iFolder
        /// </summary>
        /// <param name="ifolderID">ifolder id</param>
        /// <param name="entryID">entry id</param>
        /// <param name="folder">folder to search</param>
        /// <param name="accessID">accessID of the user</param>
        /// <returns>Matchinf iFolder Entry set</returns>
		public static iFolderEntrySet GetMatchedEntries(string ifolderID, string entryID, string[]  folder, string accessID)
		{

			int total = 0;

			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}
			
			// impersonate
			iFolder.Impersonate(c, accessID);

			// build the result list
			ArrayList list = new ArrayList();

			if (!ifolderID.Equals(entryID))
			{
				string temp1=null;
				string temp2=null;
				// not a collection
				ICSList children = c.Search(PropertyTags.Parent, new Relationship(c.ID, entryID));

				// sort the list
				ArrayList sortList = new ArrayList();
			
				foreach(ShallowNode sn in children)
				{
					sortList.Add(sn);
				}
			
				sortList.Sort(new EntryComparer());

				int i = 0;
				iFolderEntry entry;

				foreach(ShallowNode sn in sortList)
				{
					if (sn.IsBaseType(NodeTypes.FileNodeType) || sn.IsBaseType(NodeTypes.DirNodeType))
					{
						Node n = c.GetNodeByID(sn.ID);
                        			foreach (string foldername in folder)
						{
						   temp1 = foldername;	
						   entry = iFolderEntry.GetEntry(c, n);		
						   temp2 = entry.Name;
						   temp2 = temp2.ToLower();	
                                                   temp1 = temp1.ToLower();	
						   if ((temp2).Equals(temp1))
                           			   {
						       list.Add(entry);
						       ++i;
						       break;
						    }
						}
					}
				}

				// save total
				total = i;
			}
		/*	else
			{
				// a collection
				list.Add(iFolderEntry.GetEntry(c, c.GetRootDirectory()));

				// save total
			//	total = 1;
			}*/

			return new iFolderEntrySet((iFolderEntry[])list.ToArray(typeof(iFolderEntry)), total);
		}


		
		/// <summary>
		/// Get iFolder Entries by Name
		/// </summary>
		/// <param name="ifolderID">The ID of the iFolder.</param>
		/// <param name="parentID">The ID of the Parent Entry.</param>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID.</param>
		/// <returns>A Set of iFolderEntry Objects</returns>
		public static iFolderEntrySet GetEntriesByName(string ifolderID, string parentID, SearchOperation operation, string pattern, int index, int max, string accessID)
		{
			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}
			
			// impersonate
			iFolder.Impersonate(c, accessID);

			// path
			string path;

			if ((parentID == null) || ifolderID.Equals(parentID))
			{
				path = c.Name + "/";
			}
			else
			{
				Node n = c.GetNodeByID(parentID);
				DirNode dirNode = (DirNode)DirNode.NodeFactory(c, n);

				path = dirNode.GetRelativePath() + "/";
			}

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

			// find children deep
			ICSList children = c.Search(PropertyTags.FileSystemPath, path, SearchOp.Begins);

			// sort the list
			ArrayList sortList = new ArrayList();
		
			foreach(ShallowNode sn in children)
			{
				if ((regex == null) || regex.Match(sn.Name).Success)
				{
					sortList.Add(sn);
				}
			}
		
			sortList.Sort(new EntryComparer());

			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in sortList)
			{
				if (sn.IsBaseType(NodeTypes.FileNodeType) || sn.IsBaseType(NodeTypes.DirNodeType))
				{
					if ((i >= index) && (((max <= 0) || i < (max + index))))
					{
						Node n = c.GetNodeByID(sn.ID);

						list.Add(iFolderEntry.GetEntry(c, n));
					}

					++i;
				}
			}

			return new iFolderEntrySet((iFolderEntry[])list.ToArray(typeof(iFolderEntry)), i);
		}

		/// <summary>
		/// Create An iFolder Entry
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="parentID">The Parent Entry ID</param>
		/// <param name="entryName">The New Entry Name</param>
		/// <param name="type">The iFolder Entry Type</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolderEntry Object</returns>
		public static int CreateNodes(string ifolderID, bool RemoveEntry)
		{
			// 0 Completed
			// 1 Started
			// -1 Not Started. (Not in queue. Try again.)
			// -2 Busy (Creating nodes for another thread)
			// -3 Exception during Creation
			// -4 Collection does not exist.

			log.Debug("Calling Creating nodes thread for {0}", ifolderID);
			bool retVal = false;
			Store store = Store.GetStore();
			// collection
			Collection c = store.GetCollectionByID(ifolderID);
			if( c== null)
				return -4;
			if( RemoveEntry == true)
			{
				// Remove the entry..
				CreateNodeEntries.RemoveEntry( ifolderID );
				return 0;
			}		
			int status = CreateNodeEntries.GetStatus(ifolderID);
			if( status == -1 )
			{
				// This iFolder is not in queue. Try to add to queue.
				retVal = CreateNodeEntries.SetEntry( ifolderID );
				if( retVal == false)
					return -2;
				status = CreateNodeEntries.GetStatus(ifolderID);
			}
			return status;
		}

        /// <summary>
        /// Creates the fileNode for given directory
        /// </summary>
        /// <param name="source">directory info</param>
        /// <param name="ifolderID">ifolder id</param>
        /// <param name="parentID">parent id</param>
        /// <param name="entryName">entry name for ifolder</param>
        /// <returns>true if successful</returns>
		public static bool CreateFileNodesForDirectory(DirectoryInfo source, string ifolderID, string parentID, string entryName)
		{
			log.Debug("Creating nodes for directory: {0}", source.Name);
			Store store = Store.GetStore();

			// collection
			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}			

			Node n = iFolderEntry.GetEntryByPath(c, entryName);
			if( n == null)
				throw new Exception(string.Format("Node entry for {0} does not exist", source.FullName));

			if( ifolderID == parentID)
			{
				iFolderEntry entry = iFolderEntry.GetEntry(c, c.GetRootDirectory());
				if( entry == null)
				{
					throw new Exception(string.Format("Node entry for {0} does not exist", parentID));
				}
				parentID = entry.ID;
			}
			Node parent = c.GetNodeByID(parentID);
			string path;
			FileInfo[] files = source.GetFiles();
			iFolderEntryType type;
			foreach(FileInfo file in files)
			{
				log.Debug("CreateEntry for file name: {0}", file.Name);
				string tempEntryPath = entryName + "/" + file.Name;
				Node n1 = iFolderEntry.GetEntryByPath( c, tempEntryPath);
				if( n1!= null)
					continue;
				type = iFolderEntryType.File;
				try
				{
					Node entry = CreateEntry(c, parent, type, file.Name, out path, true);
					// update
					(entry as FileNode).UpdateFileInfo(c);
					c.Commit(entry);
					// Create Hashmap also...
                                        if( c.EncryptionAlgorithm == null || c.EncryptionAlgorithm == "")
                                        {
                                                // Upload hashmap for unencrypted iFolders..
						FileNode node = new FileNode(entry);
                                                HashMap map = new HashMap(c, (BaseFileNode)node);
                                                map.CreateHashMapFile();
                                        }					
				}
				catch(Exception e)
				{
					log.Debug("Exception In creating file entry for: {0}. Exception: {1}--{2}", file.FullName, e.Message, e.StackTrace);
				}
				
			}
			DirectoryInfo[] dirs = source.GetDirectories();
			foreach(DirectoryInfo dir in dirs)
			{
				string tempEntryPath = entryName + "/" + dir.Name;
				Node entry = iFolderEntry.GetEntryByPath( c, tempEntryPath);
				if( entry == null)
				{
					try
					{
						entry = CreateEntry(c, parent, iFolderEntryType.Directory, dir.Name, out path);
						// create directory and node
						// DirectoryInfo info = Directory.CreateDirectory(path);

						// update
						(entry as DirNode).CreationTime = dir.CreationTime;
						c.Commit(entry);
						CreateFileNodesForDirectory(dir, ifolderID, entry.ID, (entry as DirNode).GetRelativePath());			
					}
					catch(Exception ex)
					{
						log.Debug("Exception in creating dir entry for: {0}",ex.Message); 
						/*
						if (Directory.Exists(path))
						{
							Directory.Delete(path);
						}
						throw;
						*/
					}
				}
				else
					CreateFileNodesForDirectory(dir, ifolderID, entry.ID, (entry as DirNode).GetRelativePath());
			}
			return true;
		}

		/// <summary>
		/// Create An iFolder Entry
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="parentID">The Parent Entry ID</param>
		/// <param name="entryName">The New Entry Name</param>
		/// <param name="type">The iFolder Entry Type</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolderEntry Object</returns>
		public static iFolderEntry CreateEntry(string ifolderID, string parentID,
			iFolderEntryType type, string entryName, string accessID)
		{
			bool dontCheckForPolicy = false;
			return CreateEntry( ifolderID, parentID, type, entryName, accessID, dontCheckForPolicy);
		}

		/// <summary>
		/// Create An iFolder Entry
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="parentID">The Parent Entry ID</param>
		/// <param name="entryName">The New Entry Name</param>
		/// <param name="type">The iFolder Entry Type</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolderEntry Object</returns>
		public static iFolderEntry CreateEntry(string ifolderID, string parentID,
			iFolderEntryType type, string entryName, string accessID, bool DontCheckPolicies)
		{
			Store store = Store.GetStore();

			// collection
			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}
			
			// does member exist?
			Member member = c.GetMemberByID(accessID);
			if(member == null && Simias.Service.Manager.LdapServiceEnabled == true)
                        {
                        	Domain domain = store.GetDomain(store.DefaultDomain);
                        	string[] IDs = domain.GetMemberFamilyList(accessID);
                        	foreach(string id in IDs)
                        	{
                        		member = c.GetMemberByID(id);
                        		if(member != null)
                       	 			break;
                        	}
                        }

			if (member == null)
			{
				throw new MemberDoesNotExistException(accessID);
			}

			// impersonate
			iFolder.Impersonate(c, accessID);
			
			Node parent = c.GetNodeByID(parentID);

			string path;
			Node entry = CreateEntry(c, parent, type, entryName, out path, DontCheckPolicies);

			// directory
			if (type == iFolderEntryType.Directory)
			{
				try
				{
					// create directory and node
					DirectoryInfo info = Directory.CreateDirectory(path);

					// update
					(entry as DirNode).CreationTime = info.CreationTime;

					c.Commit(entry);
				}
				catch
				{
					if (Directory.Exists(path))
					{
						Directory.Delete(path);
					}

					throw;
				}
			}
			
				// file
			else
			{
				if( DontCheckPolicies == false)
				{
					// check file type policy
					FileTypeFilter filter = FileTypeFilter.Get(c);
					if (!filter.Allowed(entryName))
					{
						throw new FileTypeException(entryName);
					}
				}

				try
				{
					// create the file and node
					File.Create(path).Close();

					// update
					(entry as FileNode).UpdateFileInfo(c);

					c.Commit(entry);
				}
				catch
				{
					if (File.Exists(path))
					{
						File.Delete(path);
					}

					throw;
				}
			}

			// open the new node
			Node n = c.GetNodeByID(entry.ID);

			return iFolderEntry.GetEntry(c, n);
		}

		/// <summary>
		/// Create An iFolder Entry
		/// </summary>
		/// <param name="c"></param>
		/// <param name="parent"></param>
		/// <param name="type"></param>
		/// <param name="entryName"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		internal static Node CreateEntry(Collection c, Node parent,
			iFolderEntryType type, string entryName, out string path)
		{
			bool DontCheckPolicies = false;
			return CreateEntry( c, parent, type, entryName, out path, DontCheckPolicies );
		}

		
		/// <summary>
		/// Create An iFolder Entry
		/// </summary>
		/// <param name="c"></param>
		/// <param name="parent"></param>
		/// <param name="type"></param>
		/// <param name="entryName"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		internal static Node CreateEntry(Collection c, Node parent,
			iFolderEntryType type, string entryName, out string path, bool DontCheckPolicies)
		{
			Node result = null;

			// NOTE: a new entry off the iFolder is not allowed, it must be off the root directory node or lower
			if ((parent == null) || (c.ID.Equals(parent.ID)))
			{
				throw new EntryDoesNotExistException(parent.ID);
			}
			
			// NOTE: only directories can have children
			if (!parent.IsBaseType(NodeTypes.DirNodeType))
			{
				throw new DirectoryEntryRequiredException(parent.ID);
			}

			// check the name
			CheckName(entryName);

			// create new path
			DirNode parentDirNode = (DirNode) parent;
			path = parentDirNode.GetFullPath(c);
			path = System.IO.Path.Combine(path, entryName);

			// check for existing entry (case insensitive test)
			if (SyncFile.DoesNodeExist(c, parentDirNode, entryName))
			{
				throw new EntryAlreadyExistException(entryName);
			}

			// directory
			if (type == iFolderEntryType.Directory)
			{
				result = new DirNode(c, parentDirNode, entryName);
			}
			// file
			else
			{
				if( DontCheckPolicies == false)
				{
					// check file type policy
					FileTypeFilter filter = FileTypeFilter.Get(c);
					if (!filter.Allowed(entryName))
					{
						throw new FileTypeException(entryName);
					}
				}

				result = new FileNode(c, parentDirNode, entryName);
			}

			return result;
		}
		
		/// <summary>
		/// Delete An iFolder Entry
		/// </summary>
		/// <param name="ifolderID">The ID of the iFolder.</param>
		/// <param name="entryID">The ID of the Entry.</param>
		/// <param name="accessID">The Access User ID.</param>
		public static void DeleteEntry(string ifolderID, string entryID, string accessID)
		{
			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}

			// impersonate
			iFolder.Impersonate(c, accessID);
			
			Node n = c.GetNodeByID(entryID);

			if (n == null)
			{
				throw new EntryDoesNotExistException(entryID);
			}
		
			// directory
			if (n.IsBaseType(NodeTypes.DirNodeType))
			{
				DirNode dn = (DirNode)n;

				if (dn.IsRoot)
				{
					throw new DirectoryEntryRequiredException(entryID);
				}

				string path = dn.GetFullPath(c);

				if (Directory.Exists(path))
				{
					Directory.Delete(path, true);
				}

				// delete recursivley
				c.Commit(c.Delete(dn, PropertyTags.Parent));
			}
			
			// file
			else if (n.IsBaseType(NodeTypes.FileNodeType))
			{
				FileNode fn = (FileNode)n;

				string path = fn.GetFullPath(c);

				if (File.Exists(path))
				{
					File.Delete(path);
				}

				c.Commit(c.Delete(fn));
			}
			
			// not an entry
			else
			{
				throw new EntryDoesNotExistException(entryID);
			}

		}

		/// <summary>
		/// Check for a valid name.
		/// </summary>
		/// <param name="name"></param>
		public static void CheckName(string name)
		{
			// check for invalid characters
			if (!SyncFile.IsNameValid(name) || (name.IndexOf('/') != -1))
			{
				throw new EntryInvalidCharactersException(name);
			}

			// check for invalid names
			if ((name == ".") || (name == ".."))
			{
				throw new EntryInvalidNameException(name);
			}
		}
		
		/// <summary>
		/// set the lenghth of the file from web access
		/// </summary>
		public static void SetFileLength(string ifolderID, string entryID,  string accessID, long length)
		{
			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(ifolderID);

			if (c == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}

			iFolder.Impersonate(c, accessID);

			Node n = c.GetNodeByID(entryID);

			if (n == null)
			{
				throw new EntryDoesNotExistException(entryID);
			}
			
			FileNode fileNode = (FileNode)FileNode.NodeFactory(c, n);
			fileNode.Length =  length;
		}
	}

	public class CreateNodeEntries
	{
		public static string currentiFolderID = null;
		public static Hashtable CompletedTable = new Hashtable();
		private static Object LockObject = new Object();
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );
	
        /// <summary>
        /// Set the iFolder entry
        /// </summary>
        /// <param name="iFolderID"></param>
        /// <returns>true if successful</returns>
		public static bool SetEntry(string iFolderID)
		{
			bool retVal = false;
			if( currentiFolderID != null)
			{
				if( currentiFolderID == iFolderID )
				{
					log.Debug("Creating nodes for {0} is already under progress.", iFolderID);
					return true;
				}
				else
					return false;
			}
			if( CompletedTable.ContainsKey(iFolderID) == true && (int)CompletedTable[iFolderID] == 0)
				return true;
			log.Debug("setEntry for {0}", iFolderID);
			lock( LockObject)
			{
				if( currentiFolderID == null)
				{
					currentiFolderID = iFolderID;
					CompletedTable[iFolderID] = 1;
					retVal = true;
				}
			}

			Thread nodeCreationThread = new Thread( new ThreadStart( CreateNodeEntries.CreateNodes ));
			nodeCreationThread.Start();
			return retVal;
		}

        /// <summary>
        /// Removes iFolder entry from the table
        /// </summary>
        /// <param name="iFolderID"></param>
        /// <returns>true if removed successfully</returns>
		public static bool RemoveEntry( string iFolderID)
		{
			log.Debug("Removing CreateNodeQueueEntry for: {0}", iFolderID);
			if( currentiFolderID == iFolderID )
				return false;
			else if( CompletedTable.ContainsKey( iFolderID) == true)
				CompletedTable.Remove( iFolderID);	
			return true;
		}

        /// <summary>
        /// Get the status for this iFolder id into Completed table
        /// </summary>
        /// <param name="iFolderID"></param>
        /// <returns>-1 if unsuccessful otherwise the position</returns>
		public static int GetStatus( string iFolderID)
		{
			if( CompletedTable.ContainsKey( iFolderID ) )
				return (int)CompletedTable[iFolderID];
			else
				return -1;
		}

		/// <summary>
		/// Create An iFolder Entry
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="parentID">The Parent Entry ID</param>
		/// <param name="entryName">The New Entry Name</param>
		/// <param name="type">The iFolder Entry Type</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>An iFolderEntry Object</returns>
		public static void CreateNodes()
		{
			try
			{
				string ifolderID = currentiFolderID;
				if( currentiFolderID == null)
					return;
				Store store = Store.GetStore();
				// collection
				Collection c = store.GetCollectionByID(ifolderID);
				string unManagedPath = c.GetRootDirectory().GetFullPath(c);
				int ind = unManagedPath.LastIndexOf(c.Name);
				
				string path = unManagedPath.Substring(0, ind-1);
				string entryName = unManagedPath.Substring( ind);
				
				DirectoryInfo dInfo = new DirectoryInfo(path);
				DirectoryInfo[] dirs = dInfo.GetDirectories();
				bool retVal = iFolderEntry.CreateFileNodesForDirectory(dirs[0], currentiFolderID, currentiFolderID, entryName);
				if( retVal == true)
					CompletedTable[currentiFolderID] = 0;
				else
					CompletedTable[currentiFolderID] = -2;
			}
			catch(Exception ex)
			{
				log.Debug("Exception while creating nodes for the iFolder: {0}. exception: {1}--{2}", currentiFolderID, ex.Message, ex.StackTrace);
				CompletedTable[currentiFolderID] = -3;
			}
			// Resetting the current entry...
			currentiFolderID = null;
		}
	}

	/// <summary>
	/// Entry Comparer Class
	/// </summary>
	public class EntryComparer : IComparer
	{
		#region IComparer Members

		/// <summary>
		/// Compare File and Directory Shallow Nodes
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(object x, object y)
		{
			ShallowNode a = (ShallowNode) x;
			ShallowNode b = (ShallowNode) y;

			int result = 0;

			if (a.IsBaseType(NodeTypes.DirNodeType))
			{
				if (b.IsBaseType(NodeTypes.DirNodeType))
				{
					// a and b are both directories so sort by name
					result = a.Name.CompareTo(b.Name);
				}
				else
				{
					// a is a directy and b is a file, so a less than b
					result = -1;
				}
			}
			else
			{
				if (b.IsBaseType(NodeTypes.DirNodeType))
				{
					// a is a file and b is a directory, so b greater than a
					result = 1;
				}
				else
				{
					// a and b are both files so sort by name
					result = a.Name.CompareTo(b.Name);
				}
			}

			return result;
		}

		#endregion
	}

}
