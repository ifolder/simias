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
*                 $Author: Russ Young
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
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Security.Cryptography;
using Simias.Storage;
using Simias.Event;
using Simias.Sync.Delta;
using Simias.Client;

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
	#region OutFile

	/// <summary>
	/// Class to handle file operations for a file to be synced out.
	/// </summary>
	public abstract class OutFile : SyncFile
	{
		#region fields

		StreamStream	workStream;

		/// <summary>
		/// Gets the output stream.
		/// </summary>
		protected StreamStream OutStream
		{
			get { return workStream; }
		}

		#endregion
		
		#region Constructor / Finalizer

		/// <summary>
		/// Constructs an OutFile object.
		/// </summary>
		/// <param name="collection">The collection that the node belongs to.</param>
		protected OutFile(Collection collection) :
			base(collection)
		{
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~OutFile()
		{
			Close (true);
		}

		#endregion

		#region public methods.

		/// <summary>
		/// Reads data into the buffer.
		/// </summary>
		/// <param name="stream">The stream to read into.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns></returns>
		public int Read(Stream stream, int count)
		{
			try
			{
				//Log.log.Debug("Reading File {0} : offset = {1}", file, ReadPosition);
				return workStream.Read(stream, count);
			}
			catch (Exception ex)
			{
				Log.log.Debug(ex, "Failed Reading {0}", file);
				throw ex;
			}
		}

		/// <summary>
		/// Get the platform file handle.
		/// </summary>
		public StreamStream outStream
		{
			get {return workStream;}
		}


		/// <summary>
		/// Gets or Sets the file position.
		/// </summary>
		public long ReadPosition
		{
			get { return workStream.Position; }
			set { workStream.Position = value; }
		}

		/// <summary>
		/// Gets the length of the stream.
		/// </summary>
		public long Length
		{
			get { return workStream.Length; }
		}

		#endregion

		#region protected methods.

		/// <summary>
		/// Called to open the file.
		/// </summary>
		/// <param name="node">The node that represents the file.</param>
		/// <param name="sessionID">The unique session ID.</param>
		protected void Open(BaseFileNode node, string sessionID)
		{
			SetupFileNames(node, sessionID);
			Log.log.Debug("Opening File {0} (OutFile Open)", file);
			FileInfo fi = new FileInfo(file);
			if (Store.IsEnterpriseServer || fi.Length > (1024 * 100000))
			{
				workStream = new StreamStream(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
				workFile = null;
			}
			else
			{
				// This file is being pushed make a copy to work from.
				File.Copy(file, workFile, true);
				File.SetAttributes(workFile, FileAttributes.Normal);
				workStream = new StreamStream(File.Open(workFile, FileMode.Open, FileAccess.Read));
			}
		}

		/// <summary>
		/// Called to create the hash map for the uploaded file.
		/// </summary>
		public void CreateHashMap()
		{
			map.CreateHashMap();
		}

		/// <summary>
		/// Called to create the hash map for the uploaded file.
		/// </summary>		
		public void CreateHashMapFile()
		{
			map.CreateHashMapFile();
		}

		/// <summary>
		/// Called to delete the hash map since it is not stored in the simias client
		/// </summary>
		public void DeleteHashMap()
		{
			map.Delete();
		}

		/// <summary>
		/// Called to get the hash map stream for the uploaded file.
		/// </summary>
		public FileStream GetHashMap(out int entryCount, out int blockSize)
		{
 			Log.log.Debug("The hash map state is: {0}", map.MapState);

			//mapState will become true regardless hashmap creation
			
			while(map.MapState == false)
			{
				Thread.Sleep(10);			
			}
			
			if (File.Exists(map.MapFile) == true)
			{
				return map.GetHashMapStream(out entryCount, out blockSize, false, node.LocalIncarnation);
			}
			else
			{
				//just assign the out variable, it will not be used by the caller we are returning null
				entryCount = -1;
				blockSize = -1;
				Log.log.Debug("Hash map thread failed to create the hash map");
				return null;
			}
		}

		/// <summary>
		/// Called to close the file and cleanup resources.
		/// </summary>
		protected void Close()
		{
			Log.log.Debug("Closing File {0} (OutFile Close())", file);
			Close (false);
		}
		
		#endregion

		#region private methods.

		/// <summary>
		/// Called to close the file and cleanup.
		/// </summary>
		/// <param name="InFinalizer">true if called from the finalizer.</param>
		private void Close(bool InFinalizer)
		{
			if (!InFinalizer)
				GC.SuppressFinalize(this);

			if (workStream != null)
			{
				workStream.Close();
				workStream = null;
			}
			// We need to delete the temp file.
			if (workFile != null)
				File.Delete(workFile);
		}

		#endregion
	}

	#endregion

	#region InFile

	/// <summary>
	/// Class to handle files that are being imported.
	/// </summary>
	public abstract class InFile : SyncFile
	{
		#region fields

		/// <summary>Stream to the Incoming file.</summary>
		StreamStream	workStream;
		/// <summary>Stream to the Original file.</summary>
		FileStream		stream;
		/// <summary>The partially downloaded file.</summary>
		string			partialFile;
		/// <summary>The Old Node if it exists.</summary>
		protected BaseFileNode	oldNode;

		protected bool 	fileExistLocally;
		protected bool		isLocalNodeDeleted = false;
		protected bool		isServerFileRenamed = false;

				
		#endregion
		
		#region Constructor / Finalizer.

		/// <summary>
		/// Constructs an InFile object.
		/// </summary>
		/// <param name="collection">The collection that the node belongs to.</param>
		protected InFile(Collection collection) :
			base(collection)
		{
			fileExistLocally =false;
			isLocalNodeDeleted = false;
			isServerFileRenamed = false;
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~InFile()
		{
			Close (true, false);
		}

		#endregion

		#region public methods.

		/// <summary>
		/// Reads data into the buffer.
		/// </summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="offset">The offset in the buffer to read into.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns></returns>
		public int Read(byte[] buffer, int offset, int count)
		{
			if (stream != null)	return stream.Read(buffer, offset, count);
			else return 0;
		}

		/// <summary>
		/// Writes data from buffer into file.
		/// </summary>
		/// <param name="stream">The stream to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(Stream stream, int count)
		{
			//Log.log.Debug("Writing File {0} : offset = {1}", file, WritePosition);
			workStream.Write(stream, count);
		}
		
		// BUGBUG Encryption Here.
		// Add decryption here.
		/// <summary>
		/// Writes data from buffer into file.
		/// </summary>
		/// <param name="stream">The stream to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		/// <param name="count">The encryption Key.</param>
		public int Write(Stream stream, int count,  int actualCount, string encryptionAlgorithm, string encryptionKey)
		{
			workStream.Write(stream, count, actualCount, encryptionAlgorithm, encryptionKey);
			return -1;
		}
		

		/// <summary>
		/// Copyt the data from the original file into the new file.
		/// </summary>
		/// <param name="originalOffset">The offset in the original file to copy from.</param>
		/// <param name="offset">The offset in the file where the data is to be written.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Copy(long originalOffset, long offset, int count)
		{
			lock (this)
			{
				ReadPosition = originalOffset;
				WritePosition = offset;
				workStream.Write(stream, count);
			}
		}

		/// <summary>
		/// Set the Length of the file.
		/// </summary>
		/// <param name="length">The size to set.</param>
		public void SetLength(long length)
		{
			workStream.SetLength(length);
		}
		
		/// <summary>
		/// Get the stream.
		/// </summary>
		public StreamStream inStream
		{
			get {return workStream;}
		}

		/// <summary>
		/// Gets the original stream.
		/// </summary>
		public FileStream ReadStream
		{
			get {return stream;}
		}


		/// <summary>
		/// Gets or Sets the file position.
		/// </summary>
		public long ReadPosition
		{
			get { return (stream == null ? 0 : stream.Position); }
			set { if (stream != null) stream.Position = value; }
		}

		/// <summary>
		/// Gets or Sets the file position.
		/// </summary>
		public long WritePosition
		{
			get { return workStream.Position; }
			set { workStream.Position = value; }
		}

		/// <summary>
		/// Gets the length of the stream.
		/// </summary>
		public long Length
		{
			get { return node.Length; }
		}

		/// <summary>
		/// Gets the length of the local file stream.
		/// </summary>
		public long LocalFileLength
		{
			get
			{
				return (stream !=null)? stream.Length : 0;
			}
		}
		
		#endregion

		#region protected methods.

		/// <summary>
		/// Called to open the file.
		/// </summary>
		/// <param name="node">The node that represents the file.</param>
		protected void Open(BaseFileNode node)
		{
			SetupFileNames(node, "");
			CheckForNameConflict();
			Log.log.Debug("Opening File {0} (InFile Open)", file);

			// Open the file so that it cannot be modified.
			oldNode = collection.GetNodeByID(node.ID) as BaseFileNode;
			try
			{
				if (!NameConflict)
				{
					stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
				}
			}
			catch (FileNotFoundException)
			{
				Log.log.Debug("file {0} not found", file);
				
				// Check to see if we have a partially downloaded file to delta sync with.
				if (collection.Role == SyncRoles.Slave && File.Exists(workFile))
				{
					if (File.Exists(partialFile))
						File.Delete(partialFile);
					partialFile = workFile + ".part";
                                        try
                                        {
                                                File.Move(workFile, partialFile);
                                        }
                                        catch(Exception e)
                                        {
                                                try
                                                {
                                                        Log.log.Debug("could not move the file, so copy/deleting the source file: {0}. Message: {1} stack: {2}", workFile, e.Message, e.StackTrace);
                                                        File.Copy(workFile, partialFile);
                                                        File.Delete(workFile);
                                                }
                                                catch
                                                {
                                                        File.Delete(workFile);
                                                        Log.log.Debug("exception while copying workfile so deleted.");
                                                        //throw e;
                                                }
                                        }
                                        if (File.Exists(partialFile))
                                                stream = File.Open(partialFile, FileMode.Open, FileAccess.Read, FileShare.None);
					Log.log.Debug("file {0} opened", partialFile);
				}
				else if (oldNode != null)
				{
					// The file may have been renamed.
					string oldPath = oldNode.GetFullPath(collection);
					if (oldPath != file)
					{
						stream = File.Open(oldPath, FileMode.Open, FileAccess.Read, FileShare.None);
						Log.log.Debug("file {0} opened", oldPath);
						isServerFileRenamed = true;						
					}					
				}
				else
					Log.log.Debug("file  not {0} opened",file);
			}
			catch(IOException e1)
			{

				try
				{

					string Fullpath = file;
					string rootNode = collection.GetRootDirectory().GetFullPath(collection);					
					rootNode = Path.GetDirectoryName(rootNode);
					int rootPathLength = rootNode.Length;	
					int fullPathLength = Fullpath.Length;	
					 	
					string Relativepath = Fullpath.Substring(rootPathLength); 

					//Relative Path excluding FileName
					Relativepath = Path.GetDirectoryName(Relativepath);

					bool pathExists = false;
					bool pathCreated = false;	
				
					//Array of relative parth directory
					char[] delimiterList = {'/'};
					string[] dirArray = Relativepath.Split(delimiterList);

					string tempPath = rootNode;
					
					//Maintaining progressive relative path, starting from root
					string FsPath = null;

					//List of node matching the Search criteria
					ICSList nodeList = null;

					foreach(string dir in dirArray)
					{	
						if(FsPath != null)
						{
							//Creating incremental path, starting form Parent, excluding system path	
							FsPath = Path.Combine(FsPath, dir);
						}
						else
						{
							FsPath = dir;
							//Initilizing if atleast on directory exist
							pathCreated = true;
						}
						
						//Creating incremental path, including system path
						tempPath = Path.Combine(tempPath, dir);
						
						//Verify if Directory exists, starting for root parent
						if(!System.IO.Directory.Exists(tempPath))
						{
							//Verify if directory  node exisit, then only create actual directory
							nodeList = collection.Search(PropertyTags.FileSystemPath, FsPath, SearchOp.Equal);
							if(nodeList != null)
							{
								//Create directory as Node exist
								System.IO.Directory.CreateDirectory(tempPath);
							}
							else
							{
								pathCreated = false;
								Log.log.Debug("Node doesn't exist for path:{0}", FsPath);
								//As parent node doesn't exist, no need to iterate for child
								break;
							}	
						}	
					}
				
					if(pathCreated == true)
						Log.log.Debug("Final path created is :{0}", tempPath);


				}
				catch(Exception excep)
				{
					Log.log.Info("Exception while re-creating missing directory: message {0}-- stacktrace:{1}", excep.Message, excep.StackTrace);
				}
				//throw below exception to log the failure
				Log.log.Info("IOException.{0}--{1}. The file is already open by some other thread.", e1.Message, e1.StackTrace);
				throw;

			}
			// Create the file in the parent directory and then move to the work area.
			// This will insure that the proper attributes are set.
			// This was added to support EFS (Encrypted File System).
			string createName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileName(workFile));
			FileStream tmpStream = File.Open(createName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
			if (File.Exists(workFile))
			{
				File.Delete(workFile);
			}
			// Make sure we have enough space for the file.
			try
			{
				tmpStream.SetLength(node.Length);
				tmpStream.Close();
				tmpStream = null;
#if MONO
				if (MyEnvironment.Unix)
				{
					if (node.Properties.GetSingleProperty(SyncFile.ModeProperty) != null)
					{
						// Get the posix mode flags for the file.
						Stat sStat;
						if (Syscall.stat(createName, out sStat) == 0)
						{
							// Now or in the execute bit and set it on the file.
							FilePermissions fp = sStat.st_mode | FilePermissions.S_IXUSR;
							Syscall.chmod(createName, fp);
						}
					}
				}
#endif 

				File.Move(createName, workFile);
			}
			catch (IOException)
			{
				if (tmpStream != null)
					tmpStream.Close();
				throw new InsufficientStorageException();
			}
			workStream = new StreamStream(File.Open(workFile, FileMode.Truncate, FileAccess.ReadWrite, FileShare.None));
		}

		/// <summary>
		/// Called to close the file and cleanup resources.
		/// </summary>
		protected SyncNodeStatus Close(bool commit)
		{
			Log.log.Debug("Closing File {0} (protected InFile Close)", file);
			return Close (false, commit);
		}

		/// <summary>
		/// Delete the node if the file names are same and node ID is different
		/// </summary>
		/// <param </param>
		/// <returns> bool.</returns>
		public void  CheckAndResolveNodeConflict(out string removeNodeToserver)
		{
			removeNodeToserver = null;
			Property FileSystemPath = node.Properties.GetSingleProperty(PropertyTags.FileSystemPath);
			if (FileSystemPath != null)
			{			
				//search the server file in client store
				string clienFilePath = FileSystemPath.Value.ToString();
				Log.log.Debug("search path :{0} for server file ", clienFilePath);
				
				ICSList nodeList;
				nodeList = collection.Search(PropertyTags.FileSystemPath, clienFilePath, SearchOp.Equal);
				foreach (ShallowNode sn in nodeList)
				{
					if (sn.ID == node.ID)
					{
						isLocalNodeDeleted = false;
						Log.log.Debug("CheckAndResolveNodeConflict isLocalNodeDeleted==false ");
					}
					else
					{
						Log.log.Debug("CheckAndResolveNodeConflict isLocalNodeDeleted==true ");
						///This may result into file conflict if both name (server and client) macth in case insensitive compare
						///If so it will result into name conflict
						///isLocalNodeAvailable = false; //not required sinc ethe default value is false
						
						FileNode localFileNode = new FileNode(collection.GetNodeByID(sn.ID));
						///do a case sensitive compare, case insensitive names will be considered as a name conflict in CheckFileNameConflict()
						if(String.Compare(new FileNode(node).GetRelativePath(), localFileNode.GetRelativePath(), false) == 0)
						{
							Log.log.Debug("Node conflict for file {0}", localFileNode.GetRelativePath());
							Log.log.Debug("Conflict nodes {0} and {1}", node.ID, sn.ID);
							Log.log.Debug("Delete node {0}", sn.ID);//Delete the local file node
							try
							{
								removeNodeToserver = sn.ID;
								Node n = collection.GetNodeByID(sn.ID);
								n.Properties.State = PropertyList.PropertyListState.Delete;
								collection.Commit(collection.Delete(n));
								
								n.Properties.State = PropertyList.PropertyListState.Delete;
								collection.Commit(n);
								collection.ClearNodeCache(n.ID);

								isLocalNodeDeleted = true;
								//collection.Refresh();

								/*BaseFileNode bfn = n as BaseFileNode;
								FileInfo fi = new FileInfo(bfn.GetFullPath(collection));
								fi.Attributes = fi.Attributes & ~FileAttributes.ReadOnly;
								fi.Delete();
								*/

								n = collection.GetNodeByID(n.ID);
								if( n== null)
									Log.log.Debug("node {0} is deleted properly", n.ID);
								else
									Log.log.Debug("node {0} {1} not deleted state:{2}", n.ID, n.Name, n.Properties.State);
							}
							catch (Exception ex)
							{
								Log.log.Debug(ex,"Unable to delete the local in cold merge for file : {0}",file);
							}
							break;
						}
					}
				}
			}
		}

		#endregion

		#region private methods.

		/// <summary>
		/// Called to cleanup any resources and close the file.
		/// </summary>
		/// <param name="InFinalizer"></param>
		/// <param name="commit"></param>
		private SyncNodeStatus Close(bool InFinalizer, bool commit)
		{
			if (!InFinalizer)
			{
				GC.SuppressFinalize(this);
			}

			SyncNodeStatus status = new SyncNodeStatus();
			status.nodeID = node.ID;
			status.status = SyncStatus.ClientError;

			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			if (workStream != null)
			{
				workStream.Close();
				workStream = null;
			}
			if (commit)
			{
				// backup file for rolling back on failure
				string tmpFile = "";
				if (File.Exists(file)){
					tmpFile = file + ".~stmp";
					File.Move(file, tmpFile);
					Log.log.Info("backing up {0}->{1}",file,tmpFile);
				}

				// first try to write the file
				try
				{
					Log.log.Info("trying to move {0}->{1}",workFile,file);
					File.Move(workFile, file);
					workFile = null;
				}
				catch
				{
					Log.log.Info("Couldn't move {0}->{1}", workFile, file);
					Log.log.Info("Restoring file {0}", file);
					try {
						if (File.Exists(file))
							File.Delete(file); // delete newly transferred file
						if (File.Exists(tmpFile))
							File.Move(tmpFile, file); // restore backup file into place
					}
					catch (Exception ex)
					{
						Log.log.Info("couldn't return to prior state{0}--{1}",  ex.Message, ex.StackTrace);
					};
					// DELETE HASHMAP ?

					throw;  // and don't try to commit
				}
				//  try to collection.commit()
				status.status = SyncStatus.Success;
				try
				{
					Log.log.Info("trying to commit collection");
					collection.Commit(node);
				}
				catch (CollisionException ce)
				{
					commit = false;
					status.status = SyncStatus.UpdateConflict;
					Log.log.Info("Couldn't commit collection: UpdateConflict {0}--{1}",  ce.Message, ce.StackTrace);
				}
				catch (Exception ex)
				{
					commit = false;
					status.status = SyncStatus.ServerFailure;
					Log.log.Info("Couldn't commit collection {0}--{1}",  ex.Message, ex.StackTrace);
				}
				if (!commit)  // restore orig file if collection.Commit failed.
				{
					try {
						// delete newly transferred file
						if (File.Exists(file))
							File.Delete(file);
						// restore backup file into place
						if (File.Exists(tmpFile))
							File.Move(tmpFile, file);
						// delete hashmap ?
					}
					catch (Exception ex)
					{
						Log.log.Info("couldn't return to prior state {0}--{1}",  ex.Message, ex.StackTrace);
					};
				}
				else  // commit successful, delete the temporary (.~stmp) file.
				{
					try
					{
						// restore backup file into place
						if (File.Exists(tmpFile))
							File.Delete(tmpFile);
					}
					catch (Exception ex)
					{
						Log.log.Info("problem deleting .~stmp file {0}--{1}",  ex.Message, ex.StackTrace);
					};
				}
				if (commit)
				{
					FileInfo fi = new FileInfo(file);
					fi.LastWriteTime = node.LastWriteTime;
					fi.CreationTime = node.CreationTime;
					if (oldNode != null)
					{
						// Check if this was a rename.
						// If the old path does not equal the new path
						// Delete the old file.
						string oldPath = oldNode.GetFullPath(collection);
						try
						{
							Log.log.Info("{0} may have been moved to {1}", file, oldPath);
							if (MyEnvironment.Windows)
							{
								if (string.Compare(oldPath, file, true) != 0)
									File.Delete(oldPath);
							}
							else
							{
								if (oldPath != file)
									File.Delete(oldPath);
							}
						}
						catch {};
					}
				}
			}

			// We need to delete the temp file if we are the master.
			// On the client leave for a delta sync.
			if (workFile != null)
			{
				if (collection.Role == SyncRoles.Master || (collection.Role == SyncRoles.Slave && File.Exists(file)))
				{
					File.Delete(workFile);
				}
			}

			if (partialFile != null)
				File.Delete(partialFile);

			return status;
		}

		#endregion
	}

	#endregion

	#region SyncFile

	/// <summary>
	/// Class used to determine the common data between two files.
	/// This is done from a copy of the local file and a map of hash code for the server file.
	/// </summary>
	public abstract class SyncFile
	{
		#region fields

		bool					nameConflict = false;
		bool					dateConflict = false;
		protected Node			conflictingNode = null;
		/// <summary>Used to signal to stop upload or downloading the file.</summary>
		protected bool			stopping = false;
		/// <summary>The Collection the file belongs to.</summary>
		protected Collection	collection;
		/// <summary> The node that represents the file.</summary>
		protected BaseFileNode	node;
		/// <summary>The ID of the node.</summary>
		protected string		nodeID;
		/// <summary>The maximun size of a transfer.</summary>
		protected const int		MaxXFerSize = 1024 * 256;
		/// <summary>The name of the actual file.</summary>
		protected string		file;
		/// <summary>The name of the working file.</summary>
		protected string		workFile;
		/// <summary>
		/// The HashMap for this file.
		/// </summary>
		protected HashMap		map;
		/// <summary>The Prefix of the working file.</summary>
		const string			WorkFilePrefix = ".simias.wf.";
		static string			workBinDir = "WorkArea";
		static string			workBin;
		// '/' is left out on purpose because all systems disallow this char.
		public static char[] InvalidChars = {'\\', ':', '*', '?', '\"', '<', '>', '|'};

		/// <summary>Used to publish Sync events.</summary>
		static public			EventPublisher	eventPublisher = new EventPublisher();
		static internal string	ModeProperty = "FAMode";
		[Flags]
		public enum FAMode
		{
			None = 0,
			Execute = 1,
		};
		
		#endregion

		#region protected methods.

		/// <summary>
		/// Constructor to create a sync file from collection
		/// </summary>
		/// <param name="collection">The collection that the node belongs to.</param>
		protected SyncFile(Collection collection)
		{
			this.collection = collection;
		}

		/// <summary>
		/// Called to get the name of the file and workFile;
		/// </summary>
		/// <param name="node">The node that represents the file.</param>
		/// <param name="sessionID">The unique session ID.</param>
		protected void SetupFileNames(BaseFileNode node, string sessionID)
		{
			this.node = node;
			this.nodeID = node.ID;
			try
			{
				this.file = node.GetFullPath(collection);
			}
			catch
			{
				// If this failed the file name has illegal characters.
				nameConflict = true;
			}
			if (workBin == null)
			{
				workBin = Path.Combine(collection.StorePath, workBinDir);
				if (!Directory.Exists(workBin))
					Directory.CreateDirectory(workBin);
			}
			this.workFile = Path.Combine(workBin, WorkFilePrefix + node.ID + sessionID);
		}

		/// <summary>
		/// Checks for a name conflict.
		/// </summary>
		/// <returns>True if conflict.</returns>
		protected bool CheckForNameConflict()
		{
			if (!NameConflict)
			{
				// Look up the FsPath property (StoreFileNodes don't have this property set).
				Property property = node.Properties.GetSingleProperty(PropertyTags.FileSystemPath);
				if (property != null)
				{
					string path = property.Value.ToString();
					ICSList nodeList;
					nodeList = collection.Search(PropertyTags.FileSystemPath, path, SearchOp.Equal);
					foreach (ShallowNode sn in nodeList)
					{
						FileNode localFileNode = new FileNode(collection.GetNodeByID(sn.ID));
						/// Set name conflict true if both file doesn't match
						if (sn.ID != node.ID && String.Compare(new FileNode(node).GetRelativePath(), localFileNode.GetRelativePath(), false) != 0)
						{
							conflictingNode = collection.GetNodeByID(sn.ID);
							nameConflict = true;
							break;
						}
					}
					// Now make sure we don't have any illegal characters.
					if (!IsNameValid(path))
						nameConflict = true;

					if (nameConflict)
					{
						node = Conflict.CreateNameConflict(collection, node) as BaseFileNode;
						file = Conflict.GetFileConflictPath(collection, node);
						if (conflictingNode != null)
						{
							string cnPath;
							FileNode tmpFn = conflictingNode as FileNode;
							DirNode tmpDn = conflictingNode as DirNode;
							if (tmpFn != null)
							{
								cnPath = tmpFn.GetFullPath(collection);
							}
							else
							{
								cnPath = tmpDn.GetFullPath(collection);
							}
							conflictingNode = Conflict.CreateNameConflict(collection, conflictingNode, cnPath);
							Conflict.LinkConflictingNodes(conflictingNode, node);
						}
					}
				}
			}
			return nameConflict;
		}

		/// <summary>
		/// Called to see if a node with the same name exists.
		/// </summary>
		/// <param name="collection">The collection that contains the node.</param>
		/// <param name="parent">The parent node</param>
		/// <param name="name">The leaf name of the file.</param>
		/// <returns>true if allowed.</returns>
		public static bool DoesNodeExist(Collection collection, DirNode parent, string name)
		{
			string path = parent.Properties.GetSingleProperty(PropertyTags.FileSystemPath).Value.ToString() + "/" + name;
			ICSList nodeList;
			nodeList = collection.Search(PropertyTags.FileSystemPath, path, SearchOp.Equal);
			if (nodeList.Count > 0)
			{
				return true;
			}
			return false;
		}


		/// <summary>
		/// Gets or Sets a NameConflict.
		/// </summary>
		protected bool NameConflict
		{
			get {return nameConflict;}
			set {nameConflict = value;}
		}

		/// <summary>
		/// Gets or Sets a NameConflict.
		/// </summary>
		protected bool DateConflict
		{
			get {return dateConflict;}
			set {dateConflict = value;}
		}		

		#endregion

		#region public methods.

		/// <summary>
		/// Delete ther file and map file.
		/// </summary>
		/// <param name="collection">The collection that the node belongs to.</param>
		/// <param name="node">The node that represents the file.</param>
		/// <param name="path">The full path to the file.</param>
		public static void DeleteFile(Collection collection, BaseFileNode node, string path)
		{
			if (File.Exists(path))
				File.Delete(path);
			try
			{
				// Now delete the map file.
				HashMap.Delete(collection, node);
			}
			catch {}
		}

		/// <summary>
		/// Get the file name.
		/// </summary>
		public string Name
		{
			get { return Path.GetFileName(file); }
		}

		/// <summary>
		/// Tells the file to stop and return.
		/// </summary>
		public bool Stop
		{
			set { stopping = value; }
		}

		/// <summary>
		/// Tests if the file name is valid.
		/// </summary>
		/// <param name="name">The file name.</param>
		/// <returns>true if valid.</returns>
		public static bool IsNameValid(string name)
		{
			return name.IndexOfAny(InvalidChars) == -1 ? true : false;
		}

		/// <summary>
		/// Tests if the relative path is valid.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>true if valid</returns>
		public static bool IsRelativePathValid(string path)
		{
			return path.IndexOfAny(InvalidChars) == -1 ? true : false;
		}

		/// <summary>
		/// Test if encryption is enabled
		/// </summary>
		public bool IsEncryptionEnabled()
		{
			string EncryptionAlgorithm="";
			Property p = collection.Properties.FindSingleValue(PropertyTags.EncryptionType);
			EncryptionAlgorithm = (p!=null) ? (string) p.Value as string : "";
			if(EncryptionAlgorithm =="")
				return false;
			else
				return true;
		}

		/// <summary>
		/// Gets the crypto key
		/// </summary>
		public bool GetCryptoKey(out string EncryptionKey)
		{
			Log.log.Debug("GetCryptoKey: Enter");
			try
			{
				string EncryptionAlgorithm="";
				Property p = collection.Properties.FindSingleValue(PropertyTags.EncryptionType);
				EncryptionAlgorithm = (p!=null) ? (string) p.Value as string : "";
             			Log.log.Debug("GetCryptoKey: EncryptionAlgorithm:{0} and collection.DataMovement:{1}", EncryptionAlgorithm.ToString(), collection.DataMovement.ToString());
				if(EncryptionAlgorithm != "" && collection.DataMovement == false)
				{
					p = collection.Properties.FindSingleValue(PropertyTags.EncryptionKey);
					string EncryptedKey = (p!=null) ? (string) p.Value as string : null;
					
					Store store = Store.GetStore();
					string Passphrase =  store.GetPassPhrase(collection.Domain);
					//Log.log.Debug("GetCryptoKey: passphrase:{0} -- domain:{1}", Passphrase, collection.Domain.ToString());
					if(Passphrase ==null)
						throw new CollectionStoreException("Passphrase not provided");

					//Hash the passphrase and use it for encryption and decryption
					PassphraseHash hash = new PassphraseHash();
					byte[] passphrase = hash.HashPassPhrase(Passphrase);					
			
					Key key = new Key(EncryptedKey);//send the key size and algorithm
					//Log.log.Debug("DecryptKey called");
					key.DecrypytKey(passphrase, out EncryptionKey);//send the passphrase to decrypt the key
					//Log.log.Debug("Encryption key is :{0}", EncryptionKey.ToString());
				
					p = collection.Properties.FindSingleValue(PropertyTags.EncryptionBlob);
					string EncryptionBlob = (p!=null) ? (string) p.Value as string : null;
					if(EncryptionBlob == null)
						throw new CollectionStoreException("The specified cryptographic key not found");
					
					Key hashKey = new Key(EncryptionKey);
					if(hashKey.HashKey() != EncryptionBlob)
						throw new CollectionStoreException("The specified cryptographic key does not match");

					return true;
				}

				else
				{
					Log.log.Debug("GetCryptoKey: entered else part");
					if( collection.DataMovement == true)
					{
						Log.log.Debug("GetCryptoKey: datamovement is under progress.");
					}
					EncryptionKey = "";
					return false;
				}
			}
			catch (Exception ex)
			{
				throw ex;	
			}
		}

		/// <summary>
		/// Remove the exitsing conflict for the file
		/// </summary>
		public void RemoveConflict(bool commit)
		{
			Node DiskNode = collection.GetNodeByID(node.ID);
			if(DiskNode !=null  && collection.HasCollisions(DiskNode))
			{
				Log.log.Debug("Disk node has collisions for node:{0}", node.ID);
				
				Conflict conflict = new Simias.Sync.Conflict(collection, DiskNode);

				// version conflict
				if (collection.GetCollisionType(DiskNode) == CollisionType.Node)
				{
					//Since we got a new file  from server to discard the old server file, always say localChangeswin=true
					conflict.Resolve(true); //localChangesWin(true)
					Log.log.Debug("Conflict removed for the disk node :{0}", node.ID);

					// Now decrement the local version which was incremented during conflict.Resolve()
					if(commit ==false)
					{
						Log.log.Debug("Decrement the local version since commit is false for :{0}", node.ID);
						Property p = new Property(PropertyTags.Rollback, true);
						p.LocalProperty = true;
						DiskNode.Properties.ModifyProperty(p);
						//This commit will decremet the local incarnation and and remove the property
						node.Properties.State = PropertyList.PropertyListState.Update;
						collection.Commit(DiskNode);
					}
				}
				//Name conflict, For name conlfict open will fail, commit=true means(open succeded) name conflict already resolved
				/*else if(collection.GetCollisionType(DiskNode) == CollisionType.File) 
				{
					//if(commit == true)
					//conflict.RenameConflictingFile(true); //localChangesWin(true)
					//Log.log.Debug("This is a Name Conflict .....not removed");
				}*/
			}
			else
				Log.log.Debug("Disk node has no collisions");

		}
		#endregion
	}

	#endregion

	#region SyncSize

	/// <summary>
	/// class to approximate amount of data that is out of sync with master
	/// Note that this is worst-case of data that may need to be sent from
	/// this collection to the master. It does not include data that may need
	/// to be retrieved from the master. It also does not account for
	/// delta-sync algorithms that may reduce what needs to be sent
	/// </summary>
	public class SyncSize
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="col"></param>
		/// <param name="nodeCount"></param>
		/// <param name="maxBytesToSend"></param>
		public static void CalculateSendSize(Collection col, out uint nodeCount, out ulong maxBytesToSend)
		{
			Log.log.Debug("starting to calculate size to send to master for collection {0}", col.Name);

			maxBytesToSend = 0;
			nodeCount = 0;
			SyncClient.GetCountToSync(col.ID, out nodeCount);
		}
	}

	#endregion
}
