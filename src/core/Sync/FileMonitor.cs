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
*                 $Author: Dale Olds <olds@novell.com>, Russ Young
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/



using System;
using System.Threading;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Text;

using Simias.Storage;
using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Service;
using Simias.Event;

#if MONO
#if MONONATIVE
	// This is used if configure.in detected mono 1.1.13 or newer
	using Mono.Unix.Native;
#else
	using Mono.Unix;
#endif
#endif


namespace Simias.Sync
{

	//---------------------------------------------------------------------------
	/// <summary>
	/// class to sync a portion of the file system with a collection
	/// applying iFolder specific behavior
	/// </summary>

	/* TODO: need to handle if we are on a case-insensitive file system and file name
	 * changes only by case? Actually this would be a rather rare optimization and
	 * probably not worth it for the dredger (except perhaps for a directory rename).
	 * If the event system is up, we catch it as a rename. If not, the dredger treats
	 * it as a delete and create. Dredger should always be case sensitive.
	 */

	public class FileWatcher
	{
		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(FileWatcher));

		/// <summary>
		/// The collection to monitor.
		/// </summary>
		public Collection collection = null;

		/* TODO: onServer needs to be removed. It controls how tombstones are handled:
		 *   they are deleted on the server but left on the client. What it
		 *   really needs to be is deleted if there is no upstream server. Perhaps
		 *   the best way to handle it would be for this code to always leave a
		 *   tombstone, but the sync code would just remove them if there was no
		 *   upstream server.
		 */
		bool onServer = false;
		/// <summary>
		/// 
		/// </summary>
		public bool scanThreadRunning=false;
		const string lastDredgeProp = "LastDredgeTime";
		DateTime dredgeTimeStamp;
		bool needToDredge = true;
        bool toDredge = true;
		int limit = 100;
		int ct = 0;
		/// <summary>
		/// 
		/// </summary>
		public bool NeedToDredge
		{
			set { needToDredge = value; }
		}
		/// <summary>
		/// 
		/// </summary>
        public bool ToDredge
        {
            set { toDredge = value; }
        }

		DateTime lastDredgeTime = DateTime.MinValue;
		bool foundChange;
		string rootPath;
		
		bool						disposed;
//		string						collectionId;
		internal FileSystemWatcher	watcher;
		Hashtable					changes = new Hashtable();
		Hashtable					oldNames = new Hashtable();
		EventPublisher	eventPublisher = new EventPublisher();
		
		
		internal class fileChangeEntry : IComparable
		{
			internal static int				settleTime = 2;
			internal static int				counter = 0;
			internal FileSystemEventArgs	eArgs;
			internal int					eventNumber;
			internal DateTime				eventTime;

			internal fileChangeEntry(FileSystemEventArgs e)
			{
				eArgs = e;
				eventNumber = Interlocked.Increment(ref counter);
				eventTime = DateTime.Now;
			}
		
			internal void update(FileSystemEventArgs e)
			{
				eArgs = e;
				eventTime = DateTime.Now;
			}

			internal void update()
			{
				eventTime = DateTime.Now;
			}
			
			#region IComparable Members

            /// <summary>
            /// Compare two Member objects
            /// </summary>
            /// <param name="obj">Object to compare with</param>
            /// <returns>Event number after comparing the two objects</returns>
			public int CompareTo(object obj)
			{
				fileChangeEntry cobj = obj as fileChangeEntry;
				return eventNumber.CompareTo(cobj.eventNumber);
			}

			#endregion
		}

		/// <summary>
		/// Creates a dredger for this collection and dredges the system.
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="onServer"></param>
		public FileWatcher(Collection collection, bool onServer)
		{
			// TODO: Syncronize the dredger with the sync engine.
			this.collection = collection;
			this.onServer = onServer;
//			this.collectionId = collection.ID;
			
//			if (!MyEnvironment.Mono)
//			{
				// We are on .Net use events to watch for changes.
				DirNode rootDir = collection.GetRootDirectory();
				if (rootDir != null)
				{
					string rootPath = collection.GetRootDirectory().GetFullPath(collection);
				
					watcher = new FileSystemWatcher(rootPath);
					log.Debug("New File Watcher at {0}", rootPath);
					watcher.Changed += new FileSystemEventHandler(OnChanged);
					watcher.Created += new FileSystemEventHandler(OnCreated);
					watcher.Deleted += new FileSystemEventHandler(OnDeleted);
					watcher.Renamed += new RenamedEventHandler(OnRenamed);
					watcher.Error += new ErrorEventHandler(watcher_Error);
					watcher.IncludeSubdirectories = true;
					watcher.EnableRaisingEvents = true;
					// Now dredge to find any files that were changed while we were down.
				}
//			}
			disposed = false;
		}

		/// <summary>
		/// // Delete the specified node.
		/// </summary>
		/// <param name="node">The node to delete.</param>
		void DeleteNode(Node node)
		{
			Log.log.Debug("File Monitor deleting orphaned node {0}, {1}", node.Name, node.ID);
			
			// Check to see if this is a ghost file.
			// If it is we do not want to delete the node.
			if (node.Properties.GetSingleProperty(PropertyTags.GhostFile) != null)
				return;
			
			// Check to see if we have a collision.
			bool isDir = (collection.BaseType == NodeTypes.DirNodeType);
			if (collection.HasCollisions(node))
			{
				Conflict cNode = new Conflict(collection, node);
				if (cNode.IsFileNameConflict)
				{
					// This is a name collision make sure that we delete the right node.
					// Only delete if the file no longer exists.
				//	Log.log.Debug("File Monitor deleting :path {0}", Path);
					if (Path.GetFileName(cNode.FileNameConflictPath) != node.Name)
					{
						node = Conflict.GetConflictingNode(collection, node as FileNode);
						if (node == null)
							return;
						cNode = new Conflict(collection, node);
					}
					if (File.Exists(cNode.FileNameConflictPath))
					{
						return;
					}
				}
				cNode.DeleteConflictFile();
			}
			Node[] deleted = collection.Delete(node, PropertyTags.Parent);
			collection.Commit(deleted);
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, isDir ? ObjectType.Directory : ObjectType.File, true, node.Name, 0, 0, 0, Direction.Local));
			foundChange = true;
		}

        /// <summary>
        /// Rename directories with relative to old relative path
        /// </summary>
        /// <param name="collection">Collection where renaming has to be made</param>
        /// <param name="dn">Directory node to rename</param>
        /// <param name="oldRelativePath">Old relative path of the directory</param>
		public static void RenameDirsChildren(Collection collection, DirNode dn, string oldRelativePath)
		{
			string relativePath = dn.GetRelativePath();
			// We need to rename all of the children nodes.
			ArrayList nodeList = new ArrayList();
			ICSList csnList = collection.Search(PropertyTags.FileSystemPath, oldRelativePath, SearchOp.Begins);
			foreach (ShallowNode csn in csnList)
			{
				// Skip the dirnode.
				if (csn.ID == dn.ID)
					continue;

				Node childNode = collection.GetNodeByID(csn.ID);
				if (childNode != null)
				{
					Property childRP = childNode.Properties.GetSingleProperty(PropertyTags.FileSystemPath);
					if (childRP != null)
					{
						string newRP = childRP.ValueString;
						if (newRP.Length > oldRelativePath.Length && newRP.StartsWith(oldRelativePath) && newRP[oldRelativePath.Length] == '/')
						{
							childRP.SetPropertyValue(newRP.Replace(oldRelativePath, relativePath));
							childNode.Properties.ModifyNodeProperty(childRP);
							nodeList.Add(childNode);
						}
					}
				}
			}
			collection.Commit((Node[])nodeList.ToArray(typeof(Node)));
		}
        
        /// <summary>
        /// Set the rename property for the directory under collection
        /// </summary>
        /// <param name="collection">Collection in which dir rename property has to be set</param>
        /// <param name="dn">Directory node</param>
        /// <param name="oldRelativePath">Old relative path of the directory</param>
		public static void SetRenamePropertyForDirChildren(Collection collection, DirNode dn, string oldRelativePath)
		{
			// We need to rename all of the children nodes.
			ArrayList nodeList = new ArrayList();

			ICSList csnList = collection.Search(PropertyTags.FileSystemPath, oldRelativePath, SearchOp.Begins);
			foreach (ShallowNode csn in csnList)
			{
				// Skip the dirnode.
				if (csn.ID == dn.ID)
					continue;
	
				Node childNode = collection.GetNodeByID(csn.ID);
				if (childNode != null)
				{
					Property p = new Property(PropertyTags.ReNamed, true);
					p.LocalProperty = true;
					childNode.Properties.ModifyProperty(p); 
					nodeList.Add(childNode);					
				}
				else
					log.Debug("SetRenamePropertyForDirChildren  child node null");
			}
			collection.Commit((Node[])nodeList.ToArray(typeof(Node)));
		}

        /// <summary>
        /// Check whether execute bit was set or not
        /// </summary>
        /// <param name="path">Path to check for the execute bit</param>
        /// <returns>True if set else false</returns>
		bool ExecuteBitSet(string path)
		{
#if MONO
			if (MyEnvironment.Unix)
			{
				// Get the posix access flags for owner.
				Stat sStat;
				if (Syscall.stat(path, out sStat) == 0)
				{
					if ((sStat.st_mode & FilePermissions.S_IXUSR) != 0)
					{
						return true;
					}
				}
			}
#endif 
			return false;
		}

		/// <summary>
		/// Create a FileNode for the specified file.
		/// </summary>
		/// <param name="path">The path to the node to create.</param>
		/// <param name="parentNode">The parent of the node to create.</param>
		/// <param name="conflict">The node should be created with a conflict.</param>
		/// <returns>The new FileNode.</returns>
		FileNode CreateFileNode(string path, DirNode parentNode, bool conflict)
		{
			if (isSyncFile(path) || collection.HasCollisions(parentNode))
				return null;
			FileNode fnode = new FileNode(collection, parentNode, Path.GetFileName(path));
			if (ExecuteBitSet(path))
			{
				// The execute bit is set for the user save the value.
				fnode.Properties.ModifyProperty(SyncFile.ModeProperty, SyncFile.FAMode.Execute);
			}
			log.Debug("Adding file node for {0} {1}", path, fnode.ID);
			// Make sure that we support the Simias Name Space.
			if (!SyncFile.IsNameValid(fnode.Name))
			{
				conflict = true;
			}
			if (conflict)
			{
				// We have a name collision set the collision state.
				fnode = Conflict.CreateNameConflict(collection, fnode, path) as FileNode;
			}
			collection.Commit(fnode);
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, fnode.Name, 0, 0, 0, Direction.Local));
			foundChange = true;
			return fnode;
		}

		/// <summary>
		/// Modify the FileNode for the changed file.
		/// </summary>
		/// <param name="path">The path of the file that has changed.</param>
		/// <param name="fn">The node to modify.</param>
		/// <param name="hasChanges">If the node has changes set to true.</param>
		void ModifyFileNode(string path, BaseFileNode fn, bool hasChanges)
		{
			// here we are just checking for modified files
			FileInfo fi = new FileInfo(path);
			TimeSpan ts = fi.LastWriteTime - fn.LastWriteTime;

			// Fat32 has a 2 second time resolution, Linux has a 1 second resolution. Check for > 1;
			if ((fi.Length != fn.Length || ((uint)ts.Seconds > 1)) && (fn.UpdateFileInfo(collection, path)))
			{
				hasChanges = true;
				log.Debug("Updating file node for {0} {1}", path, fn.ID);
			}
			if (!SyncFile.IsNameValid(fn.Name))
			{
				// This is a conflict.
				fn = Conflict.CreateNameConflict(collection, fn, path) as BaseFileNode;
				hasChanges = true;
			}
			bool exAlready = false;
			if (fn.Properties.GetSingleProperty(SyncFile.ModeProperty) != null)
				exAlready = true;
			if (ExecuteBitSet(path))
			{
				if (!exAlready)
				{
					// The execute bit is set for the user save the value.
					fn.Properties.ModifyProperty(SyncFile.ModeProperty, SyncFile.FAMode.Execute);
					hasChanges = true;
				}
			}
			else if (exAlready)
			{
				fn.Properties.DeleteSingleProperty(SyncFile.ModeProperty);
				hasChanges = true;
			}
			if (hasChanges)
			{
				collection.Commit(fn);
				eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, fn.Name, 0, 0, 0, Direction.Local));
				foundChange = true;
			}
		}

		/// <summary>
		/// Rename the file node.
		/// </summary>
		/// <param name="newName">The new name.</param>
		/// <param name="node">The node to rename.</param>
		/// <returns>The renamed node.</returns>
		BaseFileNode RenameFileNode(string newName, BaseFileNode node)
		{
			node.Name = Path.GetFileName(newName);
			string relativePath = GetNormalizedRelativePath(rootPath, newName);
			node.Properties.ModifyNodeProperty(new Property(PropertyTags.FileSystemPath, Syntax.String, relativePath));
			// The file may have been modified.  If it has, we need to make sure the length is updated.
			FileInfo fi = new FileInfo(newName);
			if (fi.Length != node.Length)
				node.UpdateFileInfo(collection, newName);

			//set the local rename property for dir children
			//this local property will be checked and cleard in the  the sync method ploadFile()
			Property p = new Property(PropertyTags.ReNamed, true);
			p.LocalProperty = true;
			node.Properties.ModifyProperty(p); 
			
			// Commit the directory.
			collection.Commit(node);
			return node;
		}

		/// <summary>
		/// Rename the directory and fixup children.
		/// </summary>
		/// <param name="newPath">The new name of the dir.</param>
		/// <param name="node">The dir node to rename.</param>
		/// <returns>The modified node.</returns>
		DirNode RenameDirNode(string newPath, DirNode node)
		{
			node.Name = Path.GetFileName(newPath);
			string relativePath = GetNormalizedRelativePath(rootPath, newPath);
			string oldRelativePath = node.Properties.GetSingleProperty(PropertyTags.FileSystemPath).ValueString;
			node.Properties.ModifyNodeProperty(new Property(PropertyTags.FileSystemPath, Syntax.String, relativePath));

			//set the local rename property for dir children
			//this local property will be checked and cleard in the  the sync method ploadFile()
			SetRenamePropertyForDirChildren(collection, node, oldRelativePath);
			
			// Commit the directory.
			collection.Commit(node);
			// We need to rename all of the children nodes.
			RenameDirsChildren(collection, node, oldRelativePath);
			DoSubtree(newPath, node, node.ID, true);
			return node;
		}

		/// <summary>
		/// Create a DirNode for the specified directory.
		/// </summary>
		/// <param name="path">The path to the directory.</param>
		/// <param name="parentNode">The parent DirNode.</param>
		/// <param name="conflict">The node should be created with a conflict.</param>
		/// <returns>The new DirNode.</returns>
		DirNode CreateDirNode(string path, DirNode parentNode, bool conflict)
		{
			if (isSyncFile(path))
				return null;

			string fName = Path.GetFileName(path);
			DirNode dnode = new DirNode(collection, parentNode, fName);
			log.Debug("Adding dir node for {0} {1}", path, dnode.ID);
			// Make sure that we support the Simias Name Space.
			if (!SyncFile.IsNameValid(dnode.Name) || SyncFile.DoesNodeExist(collection, parentNode, fName))
			{
				conflict = true;
			}
			if (conflict)
			{
				// We have a name collision set the collision state.
				dnode = Conflict.CreateNameConflict(collection, dnode, path) as DirNode;
			}
			collection.Commit(dnode);
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Directory, false, dnode.Name, 0, 0, 0, Direction.Local));
			if (!conflict)
				DoSubtree(path, dnode, dnode.ID, true);
			foundChange = true;
			return dnode;
		}

		/// <summary>
		/// Check whether parent has changed
		/// </summary>
		/// <param name="oldPath">Old path of the file</param>
		/// <param name="newPath">New path of the file</param>
		/// <returns></returns>
		bool HasParentChanged(string oldPath, string newPath)
		{
			if (MyEnvironment.Windows)
			{
				return String.Compare(Path.GetDirectoryName(oldPath), Path.GetDirectoryName(newPath), true) == 0 ? false : true;
			}
			else
			{
				return (!(Path.GetDirectoryName(oldPath).Equals(Path.GetDirectoryName(newPath))));
			}
		}

		/// <summary>
		/// Get the normalized relative path
		/// </summary>
		/// <param name="rootPath">Root path to replace with</param>
		/// <param name="path">Path to which normalized relative path is needed</param>
		/// <returns>Relative path after normalized</returns>
		public static string GetNormalizedRelativePath(string rootPath, string path)
		{
		
			string relPath = path.Replace(rootPath, "");
			relPath = relPath.TrimStart(Path.DirectorySeparatorChar);
			if (Path.DirectorySeparatorChar != '/')
				relPath = relPath.Replace('\\', '/');
			return relPath;
		}

		/// <summary>
		/// Get a ShallowNode for the named file or directory.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <param name="haveConflict"></param>
		/// <returns>The ShallowNode for this file.</returns>
		ShallowNode GetShallowNodeForFile(string path, out bool haveConflict)
		{
			ShallowNode sNode = null;
			haveConflict = false;
			string relPath = GetNormalizedRelativePath(rootPath, path);
			ICSList nodeList;
			nodeList = collection.Search(PropertyTags.FileSystemPath, relPath, SearchOp.Equal);
			foreach (ShallowNode sn in nodeList)
			{
				if (sn.Name == Path.GetFileName(path))
				{
					sNode = sn;
				}
				else
				{
					haveConflict = true;
					sNode = sNode == null ? sn : sNode;
				}
			}
			return sNode;
		}

		/// <summary>
		/// Return the parent for this path.
		/// </summary>
		/// <param name="path">Path to the file whose parent is wanted.</param>
		/// <returns></returns>
		DirNode GetParentNode(string path)
		{
			bool haveConflict;
			ShallowNode sn = GetShallowNodeForFile(Path.GetDirectoryName(path), out haveConflict);
			if (sn != null)
			{
				return (DirNode)collection.GetNodeByID(sn.ID);
			}
			return null;
		}

		/// <summary>
		/// Check if the file is an internal sync file.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private bool isSyncFile(string name)
		{
			string fname = Path.GetFileName(name);
			return fname.StartsWith(".simias.");
		}


		/// <summary>
		/// Create a shallow node
		/// </summary>
		/// <param name="parent">Parent directory node</param>
		/// <param name="sn">shallow node</param>
		/// <param name="path">Path for which shallow node is needed</param>
		/// <param name="isDir">Whether it is directory or not</param>
		void DoShallowNode(DirNode parent, ShallowNode sn, string path, bool isDir)
		{
			Node node = null;
			DirNode dn = null;
			FileNode fn = null;
			string name = Path.GetFileName(path);
		
			// don't let temp files from sync, into the collection as regular nodes
			if (isSyncFile(name))
				return;

			// If the lastwritetime has not changed the node is up to date.
			if (File.GetLastWriteTime(path) <= lastDredgeTime)
			{
				if (isDir)
					DoSubtree(path, null, sn.ID, false);
				return;
			}

			// If the case of the names does not match we have a conflict.
			if (name != sn.Name)
			{
				if (isDir)
				{
					CreateDirNode(path, parent, true);
				}
				else
				{
					CreateFileNode(path, parent, true);
				}
				return;
			}

			node = Node.NodeFactory(collection, sn);
			if (isDir)
			{
				// This is a directory.
				dn = node as DirNode;
				if (dn == null)
				{
					// This node is the wrong type.
					DeleteNode(node);
					dn = CreateDirNode(path, parent, false);
				}
				else
				{
					DoSubtree(path, dn, dn.ID, true);
				}
			}
			else
			{
				fn = node as FileNode;
				if (fn != null)
				{
					ModifyFileNode(path, fn, false);
				}
				else
				{
					DeleteNode(node);
					fn = CreateFileNode(path, parent, false);
				}
			}
		}

	
		/// <summary>
		/// Create node for the path mentioned
		/// </summary>
		/// <param name="parentNode">Parent directory of the path</param>
		/// <param name="path">Path for which node has to be created</param>
		/// <param name="isDir">Whether the path is directory or not</param>
		void DoNode(DirNode parentNode, string path, bool isDir)
		{
			string name = Path.GetFileName(path);

			if (isSyncFile(name))
				return;
		
			// find if node for this file or dir already exists
			bool haveConflict;
			ShallowNode sn = GetShallowNodeForFile(path, out haveConflict);
			if (sn != null)
			{
				DoShallowNode(parentNode, sn, path, isDir);
			}
		}

		/// <summary>
		/// Checks to see if the current file is a recursive symlink.
		/// </summary>
		/// <param name="path">The path of the possible link.</param>
		/// <returns>true if recursive link</returns>
		bool IsRecursiveLink(string path)
		{
#if MONO
			Stat stat;
			if (Syscall.lstat(path, out stat) == 0)
			{
				if ((stat.st_mode & FilePermissions.S_IFLNK) != 0)
				{
					// If the path begins with the link path this is a recursive link.
					StringBuilder stringBuff = new StringBuilder(1024);
					if (Syscall.readlink(path, stringBuff, (ulong)stringBuff.Capacity) != -1)
					{
						string linkPath = stringBuff.ToString();
						if (!Path.IsPathRooted(linkPath))
						{
							linkPath = Path.Combine(Path.GetDirectoryName(path), linkPath);
							linkPath = Path.GetFullPath(linkPath) + "/";
						}
						// We need to check for link to a link.
						if (IsRecursiveLink(linkPath))
							return true;
            			else if (path.StartsWith(linkPath))
							return true;
					}
				}
			}
#else
			if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
			{
				// We need to determine if the link is recursive.
				// Lets check to see if we have any of the directories in our hiarchy.
				// Strip of our name.
				string parentPath = Path.GetDirectoryName(path);
				string[] fsEntries = Directory.GetDirectories(path);
				foreach (string dPath in fsEntries)
				{
					// If any of the directory names match my parent path we could be recursive.
					// Get the name of the child directory.
					string childDir = Path.GetFileName(dPath);
					int sIndex = parentPath.IndexOf(childDir);
					if (sIndex != -1)
					{
						if (parentPath[sIndex -1] == Path.DirectorySeparatorChar
							&& (((sIndex + childDir.Length) == parentPath.Length) 
							|| parentPath[sIndex + childDir.Length] == Path.DirectorySeparatorChar))
						{
							// We have a possible recursion problem.
							// We need to see if the directories are the same
							// We will do it by creating a child file and checking for existance
							// in the suspect directory.
							string suspectFile = Path.Combine(parentPath.Substring(0, sIndex - 1), ".simias.tmp");
							string localFile = Path.Combine(path, ".simias.tmp");
							File.Create(localFile).Close();
							try
							{
								if (File.Exists(suspectFile))
									return true;
							}
							finally
							{
								File.Delete(localFile);
							}
						}
					}
				}
			}
#endif
			return false;
		}

        /// <summary>
        /// Check the limit of setup and suspend if needed
        /// </summary>
		private bool CheckSuspend
		{
			// Read the limit from the setup
			//limit = Simias.Client.SimiasSetup.Limit;
			//log.Debug("Setting the limit to {0}", limit);
			get
			{
				ct++;
			
				if( ct > limit)
				{
					ct =0;
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// Create the sub tree
		/// </summary>
		/// <param name="path">Path of the directory</param>
		/// <param name="dnode">Directory node to be present in subtree</param>
		/// <param name="nodeID">Node ID of the dir node</param>
		/// <param name="subTreeHasChanged">Whether subtree changed or not</param>
		void DoSubtree(string path, DirNode dnode, string nodeID, bool subTreeHasChanged)
		{
			if (Simias.Service.Manager.ShuttingDown)
				return;

			try
			{
				//Log.Spew("Dredger processing subtree of path {0}", path);
				if (!SyncFile.IsNameValid(Path.GetFileName(path)))
				{
					// This is a name collision this needs to be resolved before
					// the files can be added.
					return;
				}

				// Make sure we are not a recursive reparse point or symlink
				if (IsRecursiveLink(path))
					return;
				
				if (subTreeHasChanged)
				{
					// A file or directory has been added or deleted from this directory. We need to find it.
					Hashtable existingNodes = new Hashtable();
					// Put all the existing nodes in a hashtable to match against the file system.
					foreach (ShallowNode sn in collection.Search(PropertyTags.Parent, new Relationship(collection.ID, dnode.ID)))
					{
						if (Simias.Service.Manager.ShuttingDown)
							return;
			
						existingNodes[sn.Name] = sn;
					}

					
					// Look for new and modified files.
					foreach (String filepath in Directory.GetFiles(path))
					{
						String file=filepath;
						if (Simias.Service.Manager.ShuttingDown)
							return;
						if(CheckSuspend)
						{
                            //during startup Sync might exit due to server failure (or login delay), but the scan thread will
                            //start its job
                            if(CollectionSyncClient.running == false)
                                SyncClient.ScheduleSync(collection.ID);
							Thread.Sleep(10);
						}

						string fName = Path.GetFileName(file);
						ShallowNode sn = (ShallowNode)existingNodes[fName];
#if DARWIN                //fix for bug 574310
						if(sn == null){ 
							try {
								file=file.Normalize();
								fName=Path.GetFileName(file);
								log.Debug("normalized file name:{0}",  file);	
								sn = (ShallowNode)existingNodes[fName];
							}catch(ArgumentException e){ 
								//log and ignore ; this would happen only if there is some invalid Unicode characters in filename 
								log.Debug("Exception :{0} while file name :{1} normalization  ", e.Message , fName);
							}
							
						}
#endif			
						if (sn != null)
						{
							DoShallowNode(dnode, sn, file, false);
							existingNodes.Remove(fName);
						}
						else
						{
							// The file is new create a new file node.
							CreateFileNode(file, dnode, false);
						}
					}

					// look for new directories
					
					foreach (string dirpath in Directory.GetDirectories(path))
					{
						String dir =dirpath;
						if (Simias.Service.Manager.ShuttingDown)
							return;
						if(CheckSuspend)
						{
                            //during startup Sync might exit due to server failure (or login delay), but the scan thread will
                            //start its job
                            if (CollectionSyncClient.running == false)
                                SyncClient.ScheduleSync(collection.ID);
                            Thread.Sleep(10);
						}
						string dName = Path.GetFileName(dir);
						ShallowNode sn = (ShallowNode)existingNodes[dName];
#if DARWIN                //fix for bug 574310
						if(sn==null){
						try{
							dir = dir.Normalize();
							dName = Path.GetFileName(dir);
							log.Debug("normalized dir name:{0}",  dName);
						    sn = (ShallowNode)existingNodes[dName];
							}catch(ArgumentException e){ 
								//log and ignore ; this would happen only if there is some invalid Unicode characters in filename 
								log.Debug("Exception :{0} while file name :{1} normalization  ", e.Message , dir);
							}
						}
#endif
						if (sn != null)
						{
							DoShallowNode(dnode, sn, dir, true);
							existingNodes.Remove(dName);
						}
						else
						{
							// The directory is new create a new directory node.
							CreateDirNode(dir, dnode, false);
						}
					}
					// look for deleted files.
					// All remaining nodes need to be deleted.
					foreach (ShallowNode sn in existingNodes.Values)
					{
						if (Simias.Service.Manager.ShuttingDown)
							return;
						if(CheckSuspend)
						{
                            //during startup Sync might exit due to server failure (or login delay), but the scan thread will
                            //start its job
                            if (CollectionSyncClient.running == false)
                                SyncClient.ScheduleSync(collection.ID);
                            Thread.Sleep(10);
						}
						DeleteNode(new Node(collection, sn));
					}
				}
				else
				{
					// Just look for modified files.
					foreach (string file in Directory.GetFiles(path))
					{
						if (Simias.Service.Manager.ShuttingDown)
							return;
						if(CheckSuspend)
						{
                            //during startup Sync might exit due to server failure (or login delay), but the scan thread will
                            //start its job
                            if (CollectionSyncClient.running == false)
                                SyncClient.ScheduleSync(collection.ID);
                            Thread.Sleep(10);
						}
						if (File.GetLastWriteTime(file) > lastDredgeTime)
						{
							if (dnode == null)
								dnode = collection.GetNodeByID(nodeID) as DirNode;
							DoNode(dnode, file, false);
						}
					}
			
					foreach (string dir in Directory.GetDirectories(path))
					{
						if (Simias.Service.Manager.ShuttingDown)
							return;
						if(CheckSuspend)
						{
                            //during startup Sync might exit due to server failure (or login delay), but the scan thread will
                            //start its job
                            if (CollectionSyncClient.running == false)
                                SyncClient.ScheduleSync(collection.ID);
                            Thread.Sleep(10);
						}
						if (Directory.GetLastWriteTime(dir) > lastDredgeTime)
						{
							if (dnode == null)
								dnode = collection.GetNodeByID(nodeID) as DirNode;
							DoNode(dnode, dir, true);
						}
						else 
						{
							bool haveConflict;
							ShallowNode sn = GetShallowNodeForFile(dir, out haveConflict);
							if (sn != null)
								DoSubtree(dir, null, sn.ID, false);
							else
							{
								// This should never happen but if it does recall with the modified true.
								DoSubtree(path, dnode, nodeID, true);
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.log.Debug(ex, "Failed adding contents of directory {0}", path);
			}
		}

		/// <summary>
		/// Dredge the Managed path.
		/// </summary>
		/// <param name="path"></param>
		void DoManagedPath(string path)
		{
		
//			DirectoryInfo tmpDi = new DirectoryInfo(path);
            try
            {
                foreach (string file in Directory.GetFiles(path))
                {
					if (File.GetLastWriteTime(file) > lastDredgeTime && !isSyncFile(file))
                    {
                        // here we are just checking for modified files
                        // Because we create temporary journal files in the store managed area,
                        // we make sure there is a corresponding node before we proceed.
                        Node node = collection.GetNodeByID(Path.GetFileName(file));
                        if ((node != null) && node.IsType(NodeTypes.BaseFileNodeType))
                        {
                            BaseFileNode unode = (BaseFileNode)collection.GetNodeByID(Path.GetFileName(file));
                            if (unode != null)
                            {
                                // Don't allow journal files (or temporary journal files) to be updated from the client.
                                if (!unode.IsType("Journal") &&
                                    !unode.IsType(NodeTypes.FileNodeType))
                                {
                                    if (CheckSuspend)
                                    {
                                        Thread.Sleep(10);
                                    }
                                    DateTime lastWrote = File.GetLastWriteTime(file);
                                    DateTime created = File.GetCreationTime(file);
                                    if (unode.LastWriteTime != lastWrote)
                                    {
                                        unode.LastWriteTime = lastWrote;
                                        unode.CreationTime = created;
                                        log.Debug("Updating store file node for {0} {1}", path, file);
                                        collection.Commit(unode);
                                        foundChange = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                //do nothing
            }
			// merge files from file system to store
		}

		/// <summary>
		/// Has any changed nodes
		/// </summary>
		public bool IsFirstSyncAfterClientUp()
		{
			if(toDredge == true)
			{
				toDredge = false;
				return true;
			}
			else if(watcher == null)
			{
				// if the collection is generic, then watcher will be null, as the unmanaged path will be null
				// currently handling only Domain and Catalog, but might need to handle others as well - FIXME 
				if(collection.IsType(NodeTypes.DomainType) || collection.IsType("Catalog") )
					return false;
				else
					return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Dredge the file sytem to find changes.
		/// </summary>
		public void CheckForFileChanges()
		{
			//make this atomic
			scanThreadRunning = true;
			collection.Refresh();
			/**************************************************
			Satyam: The following code has to be modified 
			depending on Mono bug. The bug id for Mono is 425468.
			Also once the filewatcher is enabled fixes for handling umlaut characters has to be taken care for filewatcher events , 
			which requires normalizing the filepath strings (Bug 574310)
			**************************************************/
			bool mac = true;
		#if DARWIN
			if (mac)
		#else
			if (watcher == null || needToDredge)
		#endif

			{
				Dredge();
                //during startup Sync might exit due to server failure (or login delay), but the scan thread will 	
                //start its job
                if (CollectionSyncClient.running == false)
                    SyncClient.ScheduleSync(collection.ID);
				needToDredge = false;
			}
			else
			{
				try
				{
					// Make sure the root directory still exists.
					if (!Directory.Exists(collection.GetRootDirectory().GetFullPath(collection)))
					{
						collection.Commit(collection.Delete());
						return;
					}
					
					dredgeTimeStamp = DateTime.Now;
					fileChangeEntry[] fChanges;

					lock (changes)
					{
						fChanges = new fileChangeEntry[changes.Count];
						changes.Values.CopyTo(fChanges, 0);
					}

					// Sort these by time that way they will be put in the change log in order.
					Array.Sort(fChanges);

					foreach (fileChangeEntry fc in fChanges)
					{
						if (fc.eArgs.ChangeType == WatcherChangeTypes.Renamed)
						{
							RenamedEventArgs args = (RenamedEventArgs)fc.eArgs;
							log.Debug("Queued Event ---- {0}: {1} -> {2}", args.ChangeType, args.OldFullPath, args.FullPath);
						}
						else
						{
							log.Debug("Queued Event ---- {0}: {1}", fc.eArgs.ChangeType, fc.eArgs.FullPath);
						}
			
						string fullName = GetName(fc.eArgs.FullPath);
						bool isDir = false;

						if (fullName == null)
						{
							lock(changes)
							{
								changes.Remove(fc.eArgs.FullPath);
							}
							continue;
						}

						FileInfo fi = new FileInfo(fullName);
						isDir = (fi.Attributes & FileAttributes.Directory) > 0;

						// Make sure that we let modifications settle.
						TimeSpan tSpan = DateTime.Now - fc.eventTime;
                        if (tSpan.Seconds < fileChangeEntry.settleTime)
						{
							continue;
						}

						if (!isDir && fi.Exists)
						{
							tSpan = DateTime.Now - fi.LastWriteTime;
							if (tSpan.Seconds < fileChangeEntry.settleTime)
								continue;
						}
						
						bool haveConflict;
						ShallowNode sn = GetShallowNodeForFile(fullName, out haveConflict);
						Node node = null;
						DirNode dn = null;
						BaseFileNode fn = null;

						//sn.Name == Path.GetFileName(fullName) --->means file node available for fullName so goto if and modify it
						//sn.Name != Path.GetFileName(fullName) --->means file node not available for fullName so goto else and create it
						if (sn != null && sn.Name == Path.GetFileName(fullName))
						{
							node = collection.GetNodeByID(sn.ID);
							fn = node as BaseFileNode;
							dn = node as DirNode;
							// Make sure the type is still valid.
							if (fi.Exists && ((isDir && fn != null) || (!isDir && dn != null)))
							{
								needToDredge = true;
								break;
							}
							
							// We have a node update it.
							switch (fc.eArgs.ChangeType)
							{
								case WatcherChangeTypes.Created:
								case WatcherChangeTypes.Changed:
									if (!isDir)
										ModifyFileNode(fullName, fn, false);
									break;
								case WatcherChangeTypes.Deleted:
									DeleteNode(node);
									break;
								case WatcherChangeTypes.Renamed:
								{
									RenamedEventArgs args = (RenamedEventArgs)fc.eArgs;
									oldNames.Remove(args.OldFullPath);

									// Remove any name collisions.
									if (collection.HasCollisions(node))
									{
										if (collection.GetCollisionType(node) == CollisionType.File)
											node = Conflict.RemoveNameConflict(collection, node);
									}
									// Check for a name conflict.
									if (!SyncFile.IsNameValid(args.Name))
									{
										// This is a conflict.
										node = Conflict.CreateNameConflict(collection, node, fullName);
									}

									
									// Since we are here we have a node already.
									// Make sure the case of the names has not changed.
									if (Path.GetFileName(fullName) == node.Name)
									{
										// This is a rename back to the original name update it.
										if (!isDir)
											ModifyFileNode(fullName, fn, false);
										else
											DoSubtree(fullName, dn, node.ID, true);
									}
									else
									{
										// This is a case rename.
										if (!isDir)
										{
											node = RenameFileNode(fullName, fn);
										}
										else
										{
											node = RenameDirNode(fullName, dn);
										}
									}
									
									// Make sure that there is not a node for the old name.
									sn = GetShallowNodeForFile(args.OldFullPath, out haveConflict);
									
									//(sn.Name == args.OldFullPath) means, check for case sensitive file path
									//GetShallowNodeForFile gives sn, regardless of file found or not, we are not fixing this in GetShallowNodeForFile since it may break something else
									if (sn != null && sn.ID != node.ID && (sn.Name == args.OldFullPath))
									{
										// If the file no longer exists delet the node.
										if (!File.Exists(args.OldFullPath))
										{
											Log.log.Debug("Renamed: Delete the file node for {0}", args.OldFullPath);
											node = collection.GetNodeByID(sn.ID);
											DeleteNode(node);
										}
									}
									break;
								}
							}
						}
						else
						{
							// The node does not exist.
							switch (fc.eArgs.ChangeType)
							{
								case WatcherChangeTypes.Deleted:
									// The node does not exist just continue.
									break;
								case WatcherChangeTypes.Created:
								case WatcherChangeTypes.Changed:
									// The node does not exist create it.
									if (isDir)
									{
										CreateDirNode(fullName, GetParentNode(fullName), haveConflict);
									}
									else
									{
										CreateFileNode(fullName, GetParentNode(fullName), haveConflict);
									}
									break;

								case WatcherChangeTypes.Renamed:
									// Check if there is a node for the old name.
									// Get the node from the old name.
									RenamedEventArgs args = (RenamedEventArgs)fc.eArgs;
									oldNames.Remove(args.OldFullPath);
									DirNode parent = null;
									sn = GetShallowNodeForFile(args.OldFullPath, out haveConflict);
									if (sn != null)
									{
										node = collection.GetNodeByID(sn.ID);
										fn = node as FileNode;
										dn = node as DirNode;

										// Remove any name collisions.
										if (collection.HasCollisions(node))
										{
											if (collection.GetCollisionType(node) == CollisionType.File)
												node = Conflict.RemoveNameConflict(collection, node);
										}
										// Check for a name conflict.
										if (!SyncFile.IsNameValid(Path.GetFileName(fullName)))
										{
											// This is a conflict.
											node = Conflict.CreateNameConflict(collection, node, fullName);
										}

										// Make sure the parent has not changed.
										if (HasParentChanged(args.OldFullPath, fullName))
										{
											// We have a new parent find the parent node.
											parent = GetParentNode(fullName);
											if (parent != null)
											{
												// We have a parent reset the parent node.
												node.Properties.ModifyNodeProperty(PropertyTags.Parent, new Relationship(collection.ID, parent.ID));
											}
											else
											{
												// We do not have a node for the parent.
												// Do a dredge.
												needToDredge = true;
												break;
											}
										}
										if (!isDir)
										{
											node = RenameFileNode(fullName, node as BaseFileNode);
										}
										else
										{
											node = RenameDirNode(fullName, node as DirNode);
										}
									}
									else
									{
										// The node does not exist create it.
										haveConflict = sn == null ? false : true;
										//Node tempNode; fixed : a warnig valriable never used
										if (isDir)
										{
											 CreateDirNode(fullName, GetParentNode(fullName), haveConflict);
										}
										else
										{
											 CreateFileNode(fullName, GetParentNode(fullName), haveConflict);
										}
									}
									break;
							}
						}
						lock(changes)
						{
							changes.Remove(fc.eArgs.FullPath);
						}
					}

					if (needToDredge)
					{
						Dredge();
                        //during startup Sync might exit due to server failure (or login delay), but the scan thread will
                        //start its job
                        if (CollectionSyncClient.running == false)
                            SyncClient.ScheduleSync(collection.ID);
                        needToDredge = false;
					}
					else
					{
						DoManagedPath(collection.ManagedPath);
					}
				}
				catch
				{
					Dredge();
                    //during startup Sync might exit due to server failure (or login delay), but the scan thread will
                    //start its job
                    if (CollectionSyncClient.running == false)
                        SyncClient.ScheduleSync(collection.ID);
                    needToDredge = false;
				}
			}
			if (foundChange)
			{
				// We may have just created or deleted nodes wait for the events to settle.
				// We will wait for 2 seconds because of file time resolution on fat32
				// This will ensure that we don't miss any changes.
				Thread.Sleep(2000);
			}
			//make this atomic
			scanThreadRunning = false;
		}

		/// <summary>
		/// Find File changes by dredging the file system.
		/// </summary>
		public void Dredge()
		{
			// Clear the event changes since we are going to dredge.
			lock (changes)
			{
				changes.Clear();
			}
				
			collection.Refresh();
			foundChange = false;
		
			try
			{
				lastDredgeTime = (DateTime)(collection.Properties.GetSingleProperty(lastDredgeProp).Value);
			}
			catch
			{
				// Set found change so the lastDredgeTime will get updated.
				foundChange = true;
				log.Debug("Failed to get the last dredge time");
			}
			// Make sure that the RootDir still exists. IF it has been deleted on a slave remove the collection
			// And exit.
			DirNode dn = collection.GetRootDirectory();
			if (dn != null)
			{
				rootPath = dn.Properties.GetSingleProperty(PropertyTags.Root).Value as string;
				string path = dn.GetFullPath(collection);
				if (onServer || Directory.Exists(path))
				{
					DoSubtree(path, dn, dn.ID, Directory.GetLastWriteTime(path) > lastDredgeTime ? true : false);
				}
				else
				{
					// The directory no loger exits. Delete the collection.
					collection.Delete();
					collection.Commit();
					foundChange = false;
				}
			}
			
			DoManagedPath(collection.ManagedPath);
			
			if (foundChange)
			{
				Property tsp = new Property(lastDredgeProp, dredgeTimeStamp);
				tsp.LocalProperty = true;
				collection.Properties.ModifyProperty(tsp);
				collection.Properties.State = PropertyList.PropertyListState.Internal;
                try
                {
                    collection.Commit(collection);
                }
                catch { }
			}
		}

		

		/// <summary>
		/// Finalizer.
		/// </summary>
		~FileWatcher()
		{
			Dispose(true);
		}

        /// <summary>
        /// Get the Name of the file path to sync
        /// </summary>
        /// <param name="fullPath">Full path of the file to sync</param>
        /// <returns>Name of the file to sync</returns>
		private string GetName(string fullPath)
		{
			if (MyEnvironment.Windows)
			{
				try
				{
					string path = rootPath;
					// We need to get the name with case preserved.
					string relPath = fullPath.Replace(rootPath, "");
					relPath = relPath.TrimStart(Path.DirectorySeparatorChar);
					string[] pathComponents = relPath.Split(Path.DirectorySeparatorChar);
					foreach (string pc in pathComponents)
					{
						//string[] caseSensitivePath = Directory.GetFileSystemEntries(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath));
						string[] caseSensitivePath = Directory.GetFileSystemEntries(path, pc);
						if (caseSensitivePath.Length == 1)
						{
							// We should only have one match.
							path = Path.Combine(path, Path.GetFileName(caseSensitivePath[0]));
						}
						else
						{
							// We didn't find the component return the passed in name.
							return fullPath;
						}
					}
					fullPath = path;
				}
				catch {}
			}

			// If this is a sync generated file return null.
			if (isSyncFile(fullPath))
				return null;

			return fullPath;
		}

        /// <summary>
        /// Call back if the file has been changed
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="e">File system event details</param>
		private void OnChanged(object source, FileSystemEventArgs e)
		{
			string fullPath = e.FullPath;
			log.Debug("Changed ---- {0}", e.FullPath);
			if (isSyncFile(e.Name))
				return;
			
			lock (changes)
			{
				fileChangeEntry entry = (fileChangeEntry)changes[fullPath];
				if (entry != null)
				{
					// This file has already been modified.
					// Combine the state.
					switch (entry.eArgs.ChangeType)
					{
						case WatcherChangeTypes.Created:
						case WatcherChangeTypes.Deleted:
						case WatcherChangeTypes.Changed:
							entry.update(e);
							break;
						case WatcherChangeTypes.Renamed:
							entry.update();
							break;
					}
				}
				else
				{
					changes[fullPath] = new fileChangeEntry(e);
				}
			}
		}

        /// <summary>
        /// Call back when file is renamed
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="e">Renamed event details</param>
		private void OnRenamed(object source, RenamedEventArgs e)
		{
			string fullPath = e.FullPath;
			log.Debug("Renamed ---- {0} -> {1}", e.OldFullPath, e.FullPath);
			
			if (isSyncFile(e.Name) || isSyncFile(e.OldName))
				return;
			
			lock (changes)
			{
				// Any changes made to the old file need to be removed.
				changes.Remove(e.OldFullPath);
				changes[fullPath] = new fileChangeEntry(e);
				oldNames[e.OldFullPath] = e.FullPath;
				// If We have an oldName of the new name this is a rename back.
				// Create the node for the original rename.
				string createName = (string)oldNames[e.FullPath];
				if (createName != null)
				{
					oldNames.Remove(e.FullPath);

					if (File.Exists(createName))
					{
						changes[createName] = new fileChangeEntry(
							new FileSystemEventArgs(
							WatcherChangeTypes.Created, Path.GetDirectoryName(createName), Path.GetFileName(createName)));
					}
				}
			}
		}

        /// <summary>
        /// Call back when file is deleted
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="e">File system event details</param>
		private void OnDeleted(object source, FileSystemEventArgs e)
		{
			string fullPath = e.FullPath;
			log.Debug("Deleted ---- {0}", e.FullPath);
			
			if (isSyncFile(e.Name))
				return;
						
			lock (changes)
			{
				// Check to see if we had a pending rename.
				// If so we may need to delete both the new and old
				// name.
				fileChangeEntry entry = (fileChangeEntry)changes[fullPath];
				changes[fullPath] = new fileChangeEntry(e);
				if (entry != null && entry.eArgs.ChangeType == WatcherChangeTypes.Renamed)
				{
					RenamedEventArgs args = entry.eArgs as RenamedEventArgs;
					if (!changes.Contains(args.OldFullPath))
					{
						// We do not have a file by the old name delete it.
						changes[args.OldFullPath] = new fileChangeEntry(
							new FileSystemEventArgs(
								WatcherChangeTypes.Deleted, Path.GetDirectoryName(args.OldFullPath), args.OldName));
					}
					// Now remove the old name entry.
					oldNames.Remove(args.OldFullPath);
				}
			}
		}

        /// <summary>
        /// Call back when a file is created
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="e">File system event details</param>
		private void OnCreated(object source, FileSystemEventArgs e)
		{
			string fullPath = e.FullPath;
			log.Debug("Created ---- {0}", e.FullPath);
			
			if (isSyncFile(e.Name))
				return;
						
			lock (changes)
			{
				changes[fullPath] = new fileChangeEntry(e);
			}
		}

        /// <summary>
        /// Watch for error in case of lost events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void watcher_Error(object sender, ErrorEventArgs e)
		{
			// We have lost events. we need to dredge.
			needToDredge = true;
		}
	
        /// <summary>
        /// Clean up the resources that are allocated in this class
        /// </summary>
        /// <param name="inFinalize"></param>
		private void Dispose(bool inFinalize)
		{
			lock (this)
			{
				if (!disposed)
				{
					if (!inFinalize)
					{
						System.GC.SuppressFinalize(this);
					}
					if (watcher != null)
					{
						watcher.Dispose();
					}
					disposed = true;
				}
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Called to cleanup unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion

	}
}
