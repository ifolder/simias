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
using System.Xml;
using System.Collections;
using System.Security.Cryptography;
using System.Threading;
using System.Net;
using Simias.Client;
using Simias.Client.Event;
using Simias.Storage;
using Simias.Sync.Http;
using Simias.Sync.Delta;
using Simias.Event;
using Simias.Encryption;

namespace Simias.Sync
{
	#region FileSegment

	/// <summary>
	/// The base class for an upload file segment.
	/// </summary>
	[Serializable]
	public abstract class FileSegment
	{
		/// <summary>
		/// The offset of where the data goes.
		/// </summary>
		public long Offset;
	}

	#endregion

	#region BlockSegment

	/// <summary>
	/// Describes a file segment using a block from the remote file. Can be a
	/// Range of block.
	/// </summary>
	[Serializable]
	class BlockSegment : FileSegment
	{
		/// <summary>
		/// This is the start block for the unchanged segment of data.
		/// </summary>
		public int				StartBlock;
		/// <summary>
		/// The ending block in the range of contiguous blocks.
		/// </summary>
		public int				EndBlock;
		
		public static int		InstanceSize = (8 + 4 + 4);
		/// <summary>
		/// Initialize a new Offset Segment.
		/// </summary>
		/// <param name="offset">The offset of where to copy.</param>
		/// <param name="block">The block to copy.</param>
		public BlockSegment(long offset, int block)
		{
			this.Offset = offset;
			this.StartBlock = block;
			this.EndBlock = block;
		}

	    /// <summary>
	    /// Block the segment of file
	    /// </summary>
	    /// <param name="reader">Reader details</param>
		public BlockSegment(BinaryReader reader)
		{
			Offset = reader.ReadInt64();
			StartBlock = reader.ReadInt32();
			EndBlock = reader.ReadInt32();
		}

		/// <summary>
		/// Adds the segment to the array.  If this segment is contiguous with the last segment
		/// combine them.
		/// </summary>
		/// <param name="segArray">The array to add the segment to.</param>
		/// <param name="seg">The new segment to add.</param>
		/// <param name="blockSize">The size of the hashed data blocks.</param>
		public static void AddToArray(ArrayList segArray, BlockSegment seg, int blockSize)
		{
			BlockSegment lastSeg;
			if (segArray.Count > 0)
			{
				lastSeg = segArray[segArray.Count -1] as BlockSegment;
				// Make sure the source and destination are contiguous.
				if ((lastSeg.EndBlock + 1 == seg.StartBlock) 
					&& ((lastSeg.Offset + (blockSize * (lastSeg.EndBlock - lastSeg.StartBlock + 1))) == seg.Offset))
				{
					lastSeg.EndBlock = seg.StartBlock;
					return;
				}
			}
			segArray.Add(seg);
		}

		/// <summary>
		/// Serialize the details to write
		/// </summary>
		/// <param name="writer">Writer object details to serialize</param>
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Offset);
			writer.Write(StartBlock);
			writer.Write(EndBlock);
		}
	}

	#endregion

	#region OffsetSegment

	/// <summary>
	/// Descibes a file segment using the offset and the length from the local file.
	/// </summary>
	[Serializable]
	class OffsetSegment : FileSegment
	{
		/// <summary>
		/// The length of the segment.
		/// </summary>
		public long		Length;
		
		/// <summary>
		/// Initialize a new Offset Segment.
		/// </summary>
		/// <param name="length">The length of the segment.</param>
		/// <param name="offset">The offset of the segment.</param>
		public OffsetSegment(long length, long offset)
		{
			this.Length = length;
			this.Offset = offset;
		}

		/// <summary>
		/// Adds the segment to the array.  If this segment is contiguous with the last segment
		/// combine them.
		/// </summary>
		/// <param name="segArray">The array to add the segment to.</param>
		/// <param name="seg">The new segment to add.</param>
		public static void AddToArray(ArrayList segArray, OffsetSegment seg)
		{
			OffsetSegment lastSeg;
			if (segArray.Count > 0)
			{
				lastSeg = segArray[segArray.Count -1] as OffsetSegment;
				if (seg.Offset - lastSeg.Length == lastSeg.Offset)
				{
					lastSeg.Length += seg.Length;
					return;
				}
			}
			segArray.Add(seg);
		}
	}

	#endregion

	#region DownloadSegment

	/// <summary>
	/// Describes a file segment using a block from the remote file. Can be a
	/// Range of blocks.
	/// </summary>
	public class DownloadSegment
	{
		/// <summary>
		/// This is the start block for the unchanged segment of data.
		/// </summary>
		public int				StartBlock;
		/// <summary>
		/// The ending block in the range of contiguous blocks.
		/// </summary>
		public int				EndBlock;
		
		public static int		InstanceSize = (4 + 4);
		/// <summary>
		/// Initialize a new Offset Segment.
		/// </summary>
		/// <param name="block">The block to copy.</param>
		public DownloadSegment(int block)
		{
			this.StartBlock = block;
			this.EndBlock = block;
		}

		/// <summary>
		/// Constructor to initialize the Download segment with reader
		/// </summary>
		/// <param name="reader">Reader object to download</param>
		public DownloadSegment(BinaryReader reader)
		{
			StartBlock = reader.ReadInt32();
			EndBlock = reader.ReadInt32();
		}

		/// <summary>
		/// Adds the segment to the array.  If this segment is contiguous with the last segment
		/// combine them.
		/// </summary>
		/// <param name="segArray">The array to add the segment to.</param>
		/// <param name="seg">The new segment to add.</param>
		public static void AddToArray(ArrayList segArray, DownloadSegment seg, int blockSize)
		{
			DownloadSegment lastSeg;
			if (segArray.Count > 0)
			{
				lastSeg = segArray[segArray.Count -1] as DownloadSegment;
				int blocksInSeg = lastSeg.EndBlock - lastSeg.StartBlock + 1;
				// Make sure the source and destination are contiguous.
				if (lastSeg.EndBlock + 1 == seg.StartBlock && ((blocksInSeg * blockSize) < (1024 * 300))) 
				{
					lastSeg.EndBlock = seg.StartBlock;
					return;
				}
			}
			segArray.Add(seg);
		}

		/// <summary>
		/// Serialize the writer object
		/// </summary>
		/// <param name="writer">Writer object to serialize</param>
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(StartBlock);
			writer.Write(EndBlock);
		}
	}

	#endregion

	#region HttpClientInFile

	/// <summary>
	/// ClientInFile class that uses HTTP to download the file from the server.
	/// </summary>
	public class HttpClientInFile : InFile
	{
		#region fields

		StrongWeakHashtable		table = new StrongWeakHashtable();
		HttpSyncProxy				syncService;
		/// <summary>True if the node should be marked readOnly.</summary>
		bool					readOnly = false;
		public string 				removeNodeToserver = null;

		
		#endregion
		
		#region Constructor

		/// <summary>
		/// Constructs a HttpClientFile object that can be used to sync a file down from the server.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="nodeID">The id of the node to sync down</param>
		/// <param name="syncService">The client used to access the server.</param>
		public HttpClientInFile(Collection collection, string nodeID, HttpSyncProxy syncService) :
			base(collection)
		{
			this.syncService = syncService;
			this.nodeID = nodeID;
		}

		#endregion

		#region publics

		/// <summary>
		/// Open the file.
		/// </summary>
		/// <param name="readOnly">True if the file should be marked readonly.</param>
		/// <returns>True if the file was opened.</returns>
		public bool Open(bool readOnly)
		{
			
			this.readOnly = readOnly;
			SyncNode snode = syncService.OpenFileGet(nodeID);
			if (snode == null)
			{
				return false;
			}
			try
			{
				XmlDocument xNode = new XmlDocument();
				xNode.LoadXml(snode.node);
				node = (BaseFileNode)Node.NodeFactory(collection.StoreReference, xNode);
				collection.ImportNode(node, false, 0);
				node.IncarnationUpdate = node.LocalIncarnation;

				
				Log.log.Debug("Download Open LI: {0}",node.LocalIncarnation);
				

				fileExistLocally = File.Exists( node.GetFullPath(collection));
				Log.log.Debug("Download the file exists status is: {0}", fileExistLocally.ToString());

				base.Open(node);
				return true;
			}
			catch (Exception ex)
			{
				if (ex is InsufficientStorageException)
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, 0, 0, 0, Direction.Downloading, SyncStatus.DiskFull));
				
				syncService.CloseFile();
				base.Close(false);
				Log.log.Debug(ex, "Failed opening file {0}", file);
				throw ex;
			}
		}		

		/*
		void HandleJoe()
		{
			Property FileSystemPath = node.Properties.GetSingleProperty(PropertyTags.FileSystemPath);
			if (FileSystemPath != null)
			{			
				//search the server file in client store
				string clienFilePath = FileSystemPath.Value.ToString();
				Log.log.Debug("clienFilePath  :{0}", clienFilePath);
				ICSList nodeList;
				nodeList = collection.Search(PropertyTags.FileSystemPath, clienFilePath, SearchOp.Equal);
				foreach (ShallowNode sn in nodeList)
				{					
					if (sn.ID != node.ID)
					{
						Log.log.Debug("Joe - There is a name conflict  client node to be removed");
						FileNode localFileNode = new FileNode(collection.GetNodeByID(sn.ID));

						///do a case sensitive compare, case insensitive names will be considered as a name conflict in CheckFileNameConflict()
						if(String.Compare(new FileNode(node).GetRelativePath(), localFileNode.GetRelativePath(), false) == 0)
						{
							//remove the local (client) node
							try
							{
								// set the value to update so it get deleted when we call delete
								Node n = collection.GetNodeByID(sn.ID);
								Log.log.Debug(" Joe - server node.....{0}", node.ID);
								Log.log.Debug(" Joe - deleting......{0}", n.ID);
								n.Properties.State = PropertyList.PropertyListState.Delete;
								collection.Commit(collection.Delete(n));
								collection.ClearNodeCache(n.ID);
								collection.Refresh();

								Log.log.Debug(" Joe -to see from cache or disk");

								n = collection.GetNodeByID(n.ID);
								if( n== null)
									Log.log.Debug(" Joe -The node {0} is deleted properly", n.ID);
								else
									Log.log.Debug("Joe -The node {0}--{1} exists. Not deleted......{2}", n.ID, n.Name, n.Properties.State);
							}
							catch (Exception ex)
							{
								Log.log.Debug(ex,"Joe -Unable to delete the local in cold merge for file : {0}",file);								
							}							
							break;
						}
					}
				}
			}
		}		
		*/

		/// <summary>
		/// Called to close the file.
		/// </summary>
		/// <param name="commit">True if changes should be commited.</param>
		/// <returns>true if successful.</returns>
		public new bool Close(bool commit)
		{
			Log.log.Debug("Closing File success = {0}", commit);
			
			bool bStatus = commit;			

			//Return success since DateConflict is true so nothing to be downloaded
			if(commit ==false && DateConflict ==true)
				 bStatus = true;
				
			// Close the file on the server.
			try
			{
				syncService.CloseFile();
			}
			catch{}


			// Check for previous conflicts and remove it forcefully
			// Current conflicts will get generated below
			
			///scenario: conflict created during download and resolved out of bound in server (server file is in sync with client file).
			///now download will say DateConflict since the conflict is resolved out of bound, so nothing to download
			///so remove the conflict, remove conflict will not disturb the local incarnation value
			/// commit may be true or false since we are removing the conflicts blindly
			
			if(DateConflict == true)
				RemoveConflict(commit);

			
			if (commit)
			{
				try
				{
					if(collection.Merge == true)
						isLocalNodeDeleted = true;

					/// Handle the merge case where we need to create a conflict since the data (server and local) doesnot match
					///If no data conflict and this is a merge and file exits locally and server file length is non zero then create a conflict for non encryted folders
					///Encrypted folder full file is downloaded always
					//if( fileExistLocally == true &&  collection.Merge == true && DateConflict == false &&  Length !=0 && IsEncryptionEnabled() == false)
					if( fileExistLocally == true &&  isLocalNodeDeleted == true && DateConflict == false &&  Length !=0)					
					{
						//This call will ensure that the next commit will create a conflict
						CreateFileConflict();
						//The following commit will create a conflict
					}
	
					collection.Commit(node);
					if (node.Properties.GetSingleProperty(PropertyTags.GhostFile) != null)
					{
						node.Properties.DeleteSingleProperty(PropertyTags.GhostFile);
						collection.Commit(node);
					}
					if (conflictingNode != null)
						collection.Commit(conflictingNode);

				}
				catch (CollisionException)
				{
					Log.log.Debug("Exception while colsing the file: In httpClientInFile");
					// Make sure that the versions are not the same
					Node tnode = collection.GetNodeByID(node.ID);
					if (tnode.MasterIncarnation == node.LocalIncarnation)
					{
						Log.log.Debug("False Collsion on file {0}", node.GetFullPath(collection));
						// We already have this file. Make sure we havn't changed it.
						if (tnode.MasterIncarnation != tnode.LocalIncarnation)
						{
							// We need to push this file to the server.
							// Modify it to cause a change.
							tnode.UpdateTime = tnode.UpdateTime;
							collection.Commit(tnode);
							commit = false;
						}
					}
					else
					{
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, 0, 0, 0, Direction.Downloading, SyncStatus.UpdateConflict));
						// Create an update conflict.
						file = Conflict.GetUpdateConflictPath(collection, node);
						collection.Commit(collection.CreateCollision(node, false));
					}
					oldNode = null;
				}
				catch
				{
					bStatus = false;
					commit = false;
				}
			}

			/// if DateConflict==true, then we need to commit the node for two purposes
			/// 1. commit the node, cases like merge, node will not be available locally
			/// 2. if node available, conflict may come so avoid the conflict by setting a property, based on this property conflict check 
			///     is avoided in incrementincarnation during the commit
			if(DateConflict == true)
			{
				bool versionCollision= false;
				try
				{
					/// The first commit will succed if node is not present in the disk, example merge, where node creation is postphoned
					collection.Commit(node);					
				}
				catch (CollisionException)
				{
					versionCollision =true;				
					Log.log.Debug("CollisionException in the first commit failed attempt the next commit by ignoring the disk node incarnation check");
				}
				if(versionCollision== true)
				{
					try
					{
						///There is a node in the disk
						///Add this property to roll back the local version to avoid the conflict in server node commit
						///Here we are rolling back the version since no data conflict only date(version) conflict detected
						Node DiskNode = collection.GetNodeByID(node.ID);
						Property p = new Property(PropertyTags.Rollback, true);
						p.LocalProperty = true;
						DiskNode.Properties.ModifyProperty(p);
						
						Log.log.Debug(" Disk Node Before commit LI: {0}", DiskNode.LocalIncarnation);
						//This commit will decremet the local incarnation and and remove the property
						node.Properties.State = PropertyList.PropertyListState.Update;
						collection.Commit(DiskNode);
						Log.log.Debug(" Disk Node After commit LI: {0}", DiskNode.LocalIncarnation);

						Log.log.Debug(" Clear the cache");
						collection.ClearNodeCache(DiskNode.ID);
						DiskNode = collection.GetNodeByID(node.ID);						
						Log.log.Debug(" Disk Node now local version rolled back  LI: {0}", DiskNode.LocalIncarnation);
						
						//now commit the node from server
						Log.log.Debug(" Now commit the server node");
						node.Properties.State = PropertyList.PropertyListState.Import;
						collection.Commit(node);
					}
					catch(Exception ex)
					{
						Log.log.Debug("CollisionException in the next commit, attempt failed, error not propagated");
					}
				}
			}	
			
			Log.log.Debug("Download Close MI: {0} LI: {1}",node.LocalIncarnation, node.LocalIncarnation); 
		
			// Make sure the file is not read only.
			FileInfo fi = new FileInfo(file);
			FileAttributes fa;
			if (fi.Exists)
			{
				fa = fi.Attributes;
				fi.Attributes = fa & ~FileAttributes.ReadOnly;
			}
			else
			{
				fa = FileAttributes.Normal;
			}
			try
			{
				base.Close(commit);
				
				if(DateConflict == true)
				{
					/// Sine the commit(false) is false we need to set the last write time of file to the node last write timetime
					FileInfo Fi = new FileInfo(file);
					Fi.LastWriteTime = node.LastWriteTime;
				}
			}
			catch (FileNotFoundException)
			{
				if (commit)
				{
					// The file was deleted. We will assume this is from a virus scanner.
					Property p = new Property(PropertyTags.GhostFile, true);
					p.LocalProperty = true;
					node.Properties.ModifyProperty(p);
					collection.Commit(node);
				}
			}
			if (readOnly)
			{
				// BUGBUG this is commented out until we decide what to do with readonly collections.
				//fa |= FileAttributes.ReadOnly;
			}
			if (fi.Exists)
				fi.Attributes = fa;
			return bStatus;
		}


		/// <summary>
		/// Downloads the file from the server.
		/// </summary>
		public bool DownLoadFile()
		{
			long	fileSize = Length;
			long	sizeToSync;
			long	sizeRemaining;
			int	blockSize;
			Blowfish bf = null;
			bool needDecryption=false;
			string EncryptionKey ;
			int boundary = 0;

			CheckAndResolveNodeConflict(out removeNodeToserver);

		
			long[] fileMap = GetDownloadFileMap(out sizeToSync, out blockSize);
			//Size need to be synced from server
			sizeRemaining = sizeToSync;
			WritePosition = 0;
				
			Log.log.Debug("Downloading {0} bytes, filesize = {1}", sizeToSync, fileSize); 
			Log.log.Debug("localFileSize  :{0}  serverFileSize :{1}", LocalFileLength, node.Length); 
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Downloading));
			// If We don't have any entries in the file map the file is of length 0.
			if (fileMap.Length == 0)
			{
				return true;
			}

			ArrayList downloadMap = new ArrayList();
			// Copy the matches from the local file.
			for (int i = 0; i < fileMap.Length; ++i)
			{
				if (fileMap[i] != -1)
				{
					Copy(fileMap[i], (long)i * (long)blockSize, blockSize);
				}
				else
				{
					DownloadSegment.AddToArray(downloadMap, new DownloadSegment(i), blockSize);
				}
			}

			Log.log.Debug("downloadMap.Count :{0} isServerFileRenamed :{1} LocalFileLength :{2} Length :{3}", downloadMap.Count, isServerFileRenamed, LocalFileLength, Length );

			///Determine whether both the files (server copy and local copy) are identical
			///There may be instances that the server file blocks need to be duplicated or removed through copyfile, so check the local size and server size
			
			if(sizeToSync == 0 && downloadMap.Count == 0 && isServerFileRenamed == false  && LocalFileLength == Length)
			{
				Log.log.Debug("Downloadfile DateConflict = true"); 
				DateConflict = true; //version conflict, data intact
				return false;	
			}			


			/// Get the key and decrypt it to Decrypt the file data
			if(GetCryptoKey(out EncryptionKey)== true)
			{
				needDecryption=true;
				boundary = 8;
			}
			
			
			// Get the file blocks from the server.
			foreach (DownloadSegment seg in downloadMap)
			{
				if (stopping)
					break;
				HttpWebResponse response = syncService.ReadFile(seg, blockSize);
				Stream inStream = response.GetResponseStream();
				try
				{	
					int bytesToWrite = (int)Math.Min(sizeRemaining, (seg.EndBlock - seg.StartBlock + 1) * blockSize);

					WritePosition = (long)seg.StartBlock * (long)blockSize;
					
					if(needDecryption == true)	
					{
						int actualBytesToWrite;
						if((sizeRemaining -bytesToWrite) == 0 && (node.Length%boundary !=0))
							 actualBytesToWrite = bytesToWrite -(int)(boundary-(node.Length%boundary));
						else
							actualBytesToWrite = bytesToWrite;

						Write(inStream, bytesToWrite, actualBytesToWrite , "BlowFish", EncryptionKey);
					}
					else
						Write(inStream, bytesToWrite);

					sizeRemaining -= bytesToWrite;
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Downloading));
				}
				catch
				{
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, 0, 0, 0, Direction.Downloading, SyncStatus.Error));
					throw;
				}
				finally
				{
					inStream.Close();
					response.Close();
				}
			}
			Log.log.Debug("Finished Download bytes remaining = {0}", sizeRemaining);
			if (sizeRemaining != 0)
			{
				eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, 0, Direction.Downloading));
				return false;
			}
			return true;
		}

		#endregion

		#region private
		
		/// <summary>
		/// Compute the Blocks that need to be downloaded from the server. This builds
		/// an array of offsets where the blocks need to be placed in the local file.
		/// The block number is represented by the index of the array. -1 means no match.
		/// </summary>
		/// <param name="sizeToSync">The number of bytes that need to be synced.</param>
		/// <param name="blockSize">The size of the hashed data blocks.</param>
		/// <returns>The file map.</returns>
		private long[] GetDownloadFileMap(out long sizeToSync, out int blockSize)
		{
			// Since we are doing the diffing on the client we will download all blocks that
			// don't match.
			table.Clear();
			HashData[] serverHashMap = null;
			long[] fileMap;
			blockSize = 0;
			long remainingBytes;
			bool Encrypted = false;
			bool value=false;
			string EncryptionType="";
			int boundary=0;

		
			if(IsEncryptionEnabled() == true)
			{
				Encrypted =  true;
				boundary=8;
			}

			if(ReadStream != null)
				serverHashMap = syncService.GetHashMap(out blockSize);
			else
				serverHashMap = new HashData[0];

			/*
			In case the hash map is not present, we cant do a delta-sync. so the full file download is needed.
			For an encrypted iFolder, in case of not a merge operation, we do a full file sync.
			For an encrypted iFolder, during merge, we compare the hashmap if the hash map is present.
			*/
			if(serverHashMap == null || serverHashMap.Length == 0 || (IsEncryptionEnabled() == true ))
			{
				if(Encrypted == true)
				{
					if(node.Length%boundary !=0)
						sizeToSync = node.Length+ (boundary-(node.Length%boundary));
					else
						sizeToSync = node.Length;
				}
				else
					sizeToSync = node.Length;

				fileMap = new long[HashMap.GetBlockCount(node.Length, out blockSize)];
				for (int i = 0; i < fileMap.Length; ++i)
					fileMap[i] = -1;
			
				if(serverHashMap == null || serverHashMap.Length == 0 || (IsEncryptionEnabled() && collection.Merge == false))	
					return fileMap;
				
				//For the merge on the encrypted file,  continue the match process to determine zero byte download or full download
			}
                     sizeToSync = (long)blockSize * (long)serverHashMap.Length;
			if( Encrypted == true)
			{
				if(node.Length%boundary !=0) 
					remainingBytes = (node.Length+(boundary-(node.Length%boundary))) % blockSize;
				else
					remainingBytes = node.Length % blockSize;
			}
			else
				remainingBytes = node.Length % blockSize;
			if(remainingBytes != 0)
				sizeToSync = sizeToSync - blockSize + remainingBytes;
	
			table.Add(serverHashMap);
			fileMap = new long[serverHashMap.Length];

			int				bytesRead = Math.Max(blockSize, 1024 * 32) * 2;
			byte[]			buffer = new byte[bytesRead];
			int				readOffset = 0;
			WeakHash		wh = new WeakHash();
			StrongHash		sh = new StrongHash();
			bool			recomputeWeakHash = true;
			int				startByte = 0;
			int				endByte = 0;
			byte			dropByte = 0;
			
			// Set the file map to not match anything.
			for (int i = 0; i < fileMap.Length; ++ i)
			{
				fileMap[i] = -1;
			}

			ReadPosition = 0;
            int PrevMatchedBlock = -1;
			while (bytesRead != 0)
			{
				bytesRead = Read(buffer, readOffset, bytesRead - readOffset);
				//Log.log.Info("bytesRead  1:{0}", bytesRead);
				
				bytesRead = bytesRead + readOffset;

				//Log.log.Info("bytesRead  2:{0}", bytesRead);
							
				HashEntry entry = new HashEntry();
                
				if (bytesRead >= blockSize)
				{
					endByte = startByte + blockSize - 1; 
					while (endByte < bytesRead)
					{
						if (recomputeWeakHash)
						{
							entry.WeakHash = wh.ComputeHash(buffer, startByte, (ushort)blockSize);
							recomputeWeakHash = false;
						}
						else
							entry.WeakHash = wh.RollHash(blockSize, dropByte, buffer[endByte]);
						if (table.Contains(entry.WeakHash))
						{
							entry.StrongHash = sh.ComputeHash(buffer, startByte, blockSize);
                            HashEntry match = null;
                            if (this.collection.Merge)
                            {
                                //Log.log.Info("Calling get entry after block because merge...");
                                match = table.GetEntryAfterBlock(entry, PrevMatchedBlock + 1);
                            }
                            else
                            {
                                //Log.log.Info("Calling get entry because not merge...");
                                match = table.GetEntry(entry);
                            }
							if (match != null)
							{
                                PrevMatchedBlock = match.BlockNumber;
                                //Log.log.Info("found a match between :{0} ... {1}. PrevMatchedBlock: {2}", startByte, endByte, PrevMatchedBlock);
								// We found a match save the match;
								if (fileMap[match.BlockNumber] == -1)
								{
									fileMap[match.BlockNumber] = ReadPosition - bytesRead + startByte;
									sizeToSync -= blockSize;
								}
								//Log.log.Info("sizeToSync :{0}", sizeToSync);
								startByte += blockSize;
								endByte += blockSize;
								recomputeWeakHash = true;
								continue;
							}
						}
						dropByte = buffer[startByte];
						++startByte;
						++endByte;
					}

					readOffset = bytesRead - startByte;
					Array.Copy(buffer, startByte, buffer, 0, readOffset);
					startByte = 0;
					//Log.log.Info("readOffset for the next file read :{0}", readOffset);
				}
				else 
				{
					//For the merge on the encrypted file, subtract the padding from the sizeToSync
					if(Encrypted ==true)
					{
						if(node.Length%boundary !=0)
						{
							sizeToSync = sizeToSync - (boundary-(node.Length%boundary));
							//Log.log.Debug(" Encrypted file merge sizeToSync before the last block comparision {0} ",sizeToSync);
						}
					}
					///Compare the lastblock(which is less than the block size) provided all the blocks are matched
					///Process the incomplete last block (always less than the block size)
					///Process only once, donot increment the startbyte and compare since we are doing this only to verify the files are identical or not
					
					//Log.log.Debug(" blockSize :{0}  bytesRead :{1}  sizeToSync :{2} ", blockSize, bytesRead, sizeToSync);
					if(sizeToSync == bytesRead)
					{
						startByte = 0;
						endByte = bytesRead;
						//Log.log.Debug("local hash :{0}", wh.ComputeHash(buffer, startByte, (ushort)endByte));
						//Log.log.Debug("server hash length :{0}",(long)serverHashMap.Length);
						//Log.log.Debug("serverl hash :{0}", serverHashMap[(long)serverHashMap.Length-1].WeakHash);

						entry.WeakHash = wh.ComputeHash(buffer, startByte, (ushort)endByte);
						if(table.Contains(entry.WeakHash))
						{
							//Log.log.Debug("Weak hash found for the last block");							
							entry.StrongHash = sh.ComputeHash(buffer, startByte, (ushort)endByte);
                            HashEntry match = null;
                            if (this.collection.Merge)
                            {
                                //Log.log.Debug("Calling get entry after block because merge...");
                                match = table.GetEntryAfterBlock(entry, PrevMatchedBlock + 1);
                            }
                            else
                            {
                                //Log.log.Info("Calling get entry because not a merge...");
                                match = table.GetEntry(entry);
                            }

							if (match != null && fileMap[match.BlockNumber] == -1)
							{
								//Log.log.Debug("Strong hash found for the last block");
								// We found a match save the match;	
								fileMap[match.BlockNumber] = ReadPosition - bytesRead + startByte;
								sizeToSync -= bytesRead;
							}
							else
								Log.log.Debug("No strong hash found for the last block");
						}
						else
							Log.log.Debug("No weak hash found for the last block");
					}
					
					//Break from the loop since we are done with the file				
					break;
				}
			}

			//For the merge on the encrypted file, add the padding from the sizeToSync
			if(Encrypted == true)
			{
				//check atleat one block need to be downloaded
				if(sizeToSync > 0)
				{	//if so download the entire file since the file is encrypted
					for (int j = 0; j < fileMap.Length; ++j)
						fileMap[j] = -1;

					//set it to the original value
					if(node.Length%boundary !=0)
						sizeToSync = node.Length+ (boundary-(node.Length%boundary));
					else
						sizeToSync = node.Length;
				}
				//Log.log.Debug("Encrypted file merge final sizeToSync {0}", sizeToSync);
			}
			return fileMap;
		}

		/// <summary>
		/// Called to get a string description of the diffs.
		/// </summary>
		/// <param name="fileMap">The filmap array.</param>
		/// <returns>The string description.</returns>
		private string ReportDiffs(long[] fileMap)
		{
			StringWriter sw = new StringWriter();
			int startBlock = -1;
			int endBlock = 0;
			for (int i = 0; i < fileMap.Length; ++i)
			{
				if (fileMap[i] == -1)
				{
					if (startBlock == -1)
					{
						startBlock = i;
					}
					endBlock = i;
				}
				else
				{
					if (startBlock != -1)
						sw.WriteLine("Found Missing Block {0} to Block {1}", startBlock, endBlock);
					startBlock = -1;
				}
			}
			if (startBlock != -1)
				sw.WriteLine("Found Missing Block {0} to Block {1}", startBlock, endBlock);
			return sw.ToString();
		}

		/// <summary>
		/// Called to create a conflict during the merge
		/// </summary>
		private void  CreateFileConflict()
		{
			Log.log.Debug("CreateFileConflict node state before first commit :{0}", node.Properties.State);
			Log.log.Debug("1. CreateFileConflict LI: {0} ", node.LocalIncarnation);

			//Step 1	(avoid the local incanation increment and decrement the server incarnation)
			//Step 1.1 avoid he local incarnation
			Property p= new Property(PropertyTags.Rollback, true);
			p.LocalProperty=true;
			node.Properties.ModifyProperty(p);
			///Commit the node to disk, so that the disk node is now available, this is equivalent to having a file node before download
			collection.Commit(node);			
			Log.log.Debug("2. CreateFileConflict LI: {0}", node.LocalIncarnation);
			
			
			//Step 1.2 decrement the master incarnation
			node.SetMasterIncarnation(node.LocalIncarnation-1);
			/// After the commit the state is update, eventhough it is update, we need to change a property so the next commit
			/// will increase the local incarnation, So just touch the property and update to the same value
			//node.Properties.ModifyNodeProperty( PropertyTags.LocalIncarnation, node.LocalIncarnation);
			DateTime LastWriteTime= node.LastWriteTime;
			long serverLength = node.Length;			
			FileInfo fi = new FileInfo(file);
			node.LastWriteTime = fi.LastWriteTime;
			node.Length = fi.Length;
			Log.log.Debug("3. CreateFileConflict LI: {0} ", node.LocalIncarnation); 
			Log.log.Debug("node state before commit :{0}", node.Properties.State);
			

			///Step 2. Now the node is available in the disk, commit it once again to increase the incarnation value, this will help to raise the conflict in commit which is called in close
			collection.Commit(node);
			Log.log.Debug(" node state after commit :{0}", node.Properties.State);		

			//Reset the saved 
			node.LastWriteTime = LastWriteTime;
			node.Length = serverLength;
			
			Log.log.Debug("CreateFileConflict  4=  MI: {0} LI: {1}",node.MasterIncarnation, node.LocalIncarnation); 			

			/// Step 3
			///Change back to import, so that the process commit creates a conflict
			///Consider this is a node from server and we do a fresh import on the client node
			node.Properties.State = PropertyList.PropertyListState.Import;
		}

		#endregion
	}

	#endregion

	#region HttpClientOutFile

	/// <summary>
	/// Class used to push a file to the server using HTTP.
	/// </summary>
	class HttpClientOutFile : OutFile
	{
		#region fields

		StrongWeakHashtable		table = new StrongWeakHashtable();
		HttpSyncProxy			syncService;
		
		#endregion
		
		#region Constructors

		/// <summary>
		/// Contructs a ClientFile object that can be used to sync a file up to the server.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="node">The node to sync up.</param>
		/// <param name="syncService">The service to access the server side sync.</param>
		public HttpClientOutFile(Collection collection, BaseFileNode node, HttpSyncProxy syncService) :
			base(collection)
		{
			this.node = node;
			this.syncService = syncService;
			map = new HashMap(collection, node);
		}

		#endregion

		#region publics

		/// <summary>
		/// Open the file.
		/// </summary>
		/// <returns>The status of the open.</returns>
		public virtual SyncStatus Open()
		{
			Log.log.Debug("Upload  open exit LI: {0}",node.LocalIncarnation);

			SyncNode snode = new SyncNode(node);
			SyncStatus status = syncService.OpenFilePut(snode);
			switch (status)
			{
				case SyncStatus.Success:
					try
					{
						base.Open(node, "");
					}
					catch (Exception ex)
					{
						UnauthorizedAccessException uex = ex as UnauthorizedAccessException;
						if (uex != null)
						{
							eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, node.Length, node.Length, node.Length, Direction.Uploading));
							eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, node.Length, 0, 0, Direction.Uploading, SyncStatus.Access));
						}
						Log.log.Debug(ex, "Failed opening file {0}", file);
						syncService.CloseFile(false);
						base.Close();
						throw ex;
					}
					break;
				case SyncStatus.PolicyQuota:
				case SyncStatus.PolicySize:
				case SyncStatus.PolicyType:
					Property p = new Property(PropertyTags.SyncStatusTag, status.ToString());
					p.LocalProperty = true;
					node.Properties.ModifyProperty(p);
					collection.Commit(node);
					break;
			}
			
			return status;
		}

		/// <summary>
		/// Called to close the file.
		/// </summary>
		/// <param name="commit">True if changes should be commited.</param>
		/// <returns>true if successful.</returns>
		public virtual SyncNodeStatus Close(bool commit)
		{
			Log.log.Debug("Upload  Close Entry LI: {0}",node.LocalIncarnation);
			// Close the file on the server.
			try
			{
				SyncNodeStatus status = syncService.CloseFile(commit);
				if (commit && status.status == SyncStatus.Success)
				{
					node.SetMasterIncarnation(node.LocalIncarnation);
					collection.Commit(node);
					if (node.Properties.GetSingleProperty(PropertyTags.SyncStatusTag) != null)
					{
						node.Properties.DeleteSingleProperty(PropertyTags.SyncStatusTag);
						collection.Commit(node);
					}
				}
				
				//check the rollback property to roll back the client vetrsion if no upload is required
				// for example no data change, only the file date modified
				if(DateConflict == true)
				{
					///scenario: conflict created during download and resolved out of bound in client (client file is in sync with server file). 
					///now upload will say DateConflict since the conflict is resolved out of bound, so nothing to upload
					///so remove the conflict, remove conflict will not disturb the local incarnation value 
					RemoveConflict(commit);					
						
					///Set the roolback local property, this will decrement the local incarnation number for upload case where nothing to be uploaded
					Property p = new Property(PropertyTags.Rollback, true);
					p.LocalProperty = true;
					node.Properties.ModifyProperty(p);

					collection.Commit(node);
					
					status.status = SyncStatus.OnlyDateModified;
				}
				Log.log.Debug("Upload  Close exit LI: {0} MI: {1}",node.LocalIncarnation, node.MasterIncarnation);

				return status;
			}
			finally
			{
				base.Close();
			}
		}
		
		/// <summary>
		/// Uploads the file to the server.
		/// </summary>
		public bool UploadFile()
		{
			long	fileSize = Length;
			long	sizeToSync;
			long	sizeRemaining;
			ArrayList copyArray;
			ArrayList writeArray;
			int blockSize;
			bool needEncryption=false;
			string EncryptionKey="";
			bool serverFileAvailable = false;
			int serverBlockCount = 0;
			int clientBlockCount = 0;

			//Do it parallel with file upload, this will delegate the job to a seperate thread
			CreateHashMap();

			GetUploadFileMap(out sizeToSync, out copyArray, out writeArray, out blockSize, out serverFileAvailable, out serverBlockCount);

			//Get the renamed state, if set reset back
			Property reNamed = node.Properties.FindSingleValue(PropertyTags.ReNamed);
			if(reNamed !=null)
			{
				collection.Properties.DeleteSingleProperty(PropertyTags.ReNamed); 				
				collection.Commit(node);
			}

			if(blockSize != 0)
			{
				if(Length % blockSize !=0) 
					clientBlockCount = (int)Length/blockSize+1;
				else
					clientBlockCount = (int)Length/blockSize;
				Log.log.Debug("client 	block count	={0}", clientBlockCount);
				Log.log.Debug("server   block count	={0}", serverBlockCount);
			}

			/// No data to sync,  file is available in server, not renamed and block count matches
			if(sizeToSync == 0 && serverFileAvailable == true && reNamed == null && clientBlockCount == serverBlockCount)
			{	
				Log.log.Debug("serverFileAvailable	={0}", serverFileAvailable);
				Log.log.Debug("sizeToSync 			={0}", sizeToSync);
				Log.log.Debug("copyArray.Count		={0}", copyArray.Count);
				Log.log.Debug("writeArray.Count		={0}", writeArray.Count);

				Log.log.Debug("only date conflict so revert back the changes made in the server and client");
				DateConflict = true;
				return false;
			}
			
			sizeRemaining = sizeToSync;
			
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Uploading));

			if (copyArray.Count > 0)
			{
				syncService.CopyFile(copyArray, blockSize);
			}


			
			/// Get the key and decrypt it to encrypt the file data
			if(GetCryptoKey(out EncryptionKey)== true)
			{
				needEncryption = true;
			}
			
			
			foreach(OffsetSegment seg in writeArray)
			{
				// Write the bytes to the output stream.
				if (seg.Length > 0)
				{
					long leftToSend = seg.Length;
					ReadPosition = seg.Offset;
					while (leftToSend > 0)
					{	
						// BUGBUG Encryption Here.
						// Add encryption here.							
						if (stopping)
							break;
						int bytesToSend = (int)Math.Min(MaxXFerSize, leftToSend);
						
						/*If simias runs behind apache with mod mono server, first 16k transfer failes,
						as a work around two 8k blocks are sent, chages done in web access also*/
						if(leftToSend ==  0x4000)
							bytesToSend = 0x2000;						
						if(needEncryption == true)
						{
							syncService.WriteFile(OutStream, ReadPosition, bytesToSend, "BlowFish", EncryptionKey);
						}
						else
							syncService.WriteFile(OutStream, ReadPosition, bytesToSend, null, null);
			
						leftToSend -= bytesToSend;
						sizeRemaining -= bytesToSend;
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Uploading));
					}
				}
			}
			if (sizeRemaining == 0)
				return true;
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, 0, Direction.Uploading));
			return false;
		}

		#endregion

		#region private


		/// <summary>
		/// Gets the copy and write arrays that are used to create the file on the server.
		/// </summary>
		/// <param name="sizeToSync"></param>
		/// <param name="copyArray">The array of BlockSegments that need to be copied from the old file.</param>
		/// <param name="writeArray">The array of OffsetSegments that need to be sent from the client.</param>
		/// <param name="blockSize">The size of the hashed data blocks.</param>
		private void GetUploadFileMap(out long sizeToSync, out ArrayList copyArray, out ArrayList writeArray, out int blockSize, out bool serverFileAvailable, out int serverBlockCount)
		{
			sizeToSync = 0;
			copyArray = new ArrayList();
			writeArray = new ArrayList();
			HashData[] serverHashMap = null;
			blockSize = 0;
			serverFileAvailable = false;
			serverBlockCount = 0;

			// Get the hash map from the server. If the file is on the server.
			if (node.MasterIncarnation != 0)
			{
				serverHashMap = syncService.GetHashMap(out blockSize);
			}
			if(serverHashMap != null)
			{
				serverFileAvailable =  true;
				serverBlockCount = serverHashMap.Length;
			}

			//If not available in server and encrypted file
			if (serverHashMap == null || serverHashMap.Length == 0 /*|| IsEncryptionEnabled() == true*/)
			{
				if(serverHashMap == null)
					Log.log.Debug("serverHashMap is null");
				else
					Log.log.Debug("serverHashMap.Length :{0}", serverHashMap.Length);
								
				// Send the whole file.
				sizeToSync = Length;
				writeArray.Add(new OffsetSegment(sizeToSync, 0));
				return;
			}
			
			Log.log.Debug("GetUploadFileMap called.....");


			table.Clear();
			table.Add(serverHashMap);
			
			int				bytesRead = Math.Max(blockSize, 1024 * 32) * 2;
			byte[]			buffer = new byte[bytesRead];
			int				readOffset = 0;
			WeakHash		wh = new WeakHash();
			StrongHash		sh = new StrongHash();
			bool			recomputeWeakHash = true;
			int				startByte = 0;
			int				endByte = 0;
			int				endOfLastMatch = 0;
			byte			dropByte = 0;
			
			ReadPosition = 0;		
			while (bytesRead != 0)
			{
				bytesRead = outStream.Read(buffer, readOffset, bytesRead - readOffset);
				Log.log.Debug("Upload bytesRead.....{0}", bytesRead);
				if (bytesRead == 0)
					break;

				bytesRead = bytesRead + readOffset;
				
				if (bytesRead >= blockSize)
				{
					endByte = startByte + blockSize - 1;
					HashEntry entry = new HashEntry();
					while (endByte < bytesRead)
					{
						if (recomputeWeakHash)
						{
							entry.WeakHash = wh.ComputeHash(buffer, startByte, (ushort)blockSize);
							recomputeWeakHash = false;
						}
						else
							entry.WeakHash = wh.RollHash(blockSize, dropByte, buffer[endByte]);
						if (table.Contains(entry.WeakHash))
						{
							entry.StrongHash = sh.ComputeHash(buffer, startByte, blockSize);
							HashEntry match = table.GetEntry(entry);
							if (match != null)
							{
								Log.log.Info("Strong hash found from {0} to {1}", startByte, endByte);
								// We found a match save the data that does not match;
								if (endOfLastMatch != startByte)
								{
									Log.log.Info("data doesn't match between {0} to {1}", endOfLastMatch, startByte);
									long segLen = startByte - endOfLastMatch;
									long segOffset = ReadPosition - bytesRead + endOfLastMatch;
									OffsetSegment seg = new OffsetSegment(segLen, segOffset);
									OffsetSegment.AddToArray(writeArray, seg);
									sizeToSync += segLen;
								}
								// Save the matched block.
								long blockOffset = ReadPosition - bytesRead + startByte;								
								BlockSegment.AddToArray(copyArray, new BlockSegment(blockOffset, match.BlockNumber), blockSize);
								
								startByte = endByte + 1;
								endByte = startByte + blockSize - 1;
								endOfLastMatch = startByte;
								recomputeWeakHash = true;
								continue;
							}
						}
						dropByte = buffer[startByte];
						++startByte;
						++endByte;
					}

					// We need to copy any data that has not been saved.
					if (endOfLastMatch == 0)
					{
						// Add this segment to the array.
						long segOffset = ReadPosition - bytesRead + endOfLastMatch;
						long segLen = startByte - endOfLastMatch;
						OffsetSegment seg = new OffsetSegment(segLen, segOffset);
						OffsetSegment.AddToArray(writeArray, seg);
						sizeToSync += segLen;
						endOfLastMatch = startByte;
					}
					readOffset = bytesRead - endOfLastMatch;
					Array.Copy(buffer, endOfLastMatch, buffer, 0, readOffset);
					startByte = startByte - endOfLastMatch; //0;
					endOfLastMatch = 0;
					endByte = readOffset - 1;
				}
				else
				{
					endByte = bytesRead - 1;
					/// If the file size is less than minimum block size (4096) set the readOffset to compare the hash below
					if(readOffset == 0)
						readOffset = bytesRead - endOfLastMatch;
					break;
				}
			}
			
			Log.log.Debug("writeArray.Count:{0}  copyArray.Count:{1}", writeArray.Count, copyArray.Count);
			bool		lastBlockMatch = false;
			
			/// Check the last block (insufficient block) match to avoid the data transfer
			if(readOffset > 0)
			{
				bytesRead = readOffset;
				Log.log.Debug("now comparing the last block of size{0}", bytesRead);
				
				HashEntry Entry = new HashEntry();
				Entry.WeakHash = wh.ComputeHash(buffer, 0, (ushort)bytesRead);
				Log.log.Debug("Upload Weak Hash : {0}  bytesRead :{1}", Entry.WeakHash, bytesRead);					
				if (table.Contains(Entry.WeakHash))
				{
					Entry.StrongHash = sh.ComputeHash(buffer, 0, (ushort)bytesRead);
					HashEntry match = table.GetEntry(Entry);
					if (match != null)
					{
						Log.log.Debug("last block no data change so add into copy array");
						lastBlockMatch = true;
						long blockOffset = ReadPosition - bytesRead;
						BlockSegment.AddToArray(copyArray, new BlockSegment(blockOffset, match.BlockNumber), bytesRead);
					}					
				}
			}	

			// Get the remaining changes.
			if ((endOfLastMatch + 1) != endByte && lastBlockMatch == false)
			{
				long segLen = endByte - endOfLastMatch + 1;
				long segOffset = ReadPosition - segLen;
				OffsetSegment seg = new OffsetSegment(segLen, segOffset);
				OffsetSegment.AddToArray(writeArray, seg);
				sizeToSync += segLen;
			}
			//Defect 488056, touch the file should not upload the file
			if(IsEncryptionEnabled() == true)
			{
				if((writeArray.Count > 0))
				{
                    copyArray.Clear();
                    writeArray.Clear();
					//File data modified, upload the entire file
					sizeToSync = Length;
					writeArray.Add(new OffsetSegment(sizeToSync, 0));
				}
				else
				{
                    copyArray.Clear();
                    writeArray.Clear();
					//Nothing to upload since the file data is modified (might have been 'touch'ed
					sizeToSync = 0;
				}
			}
		}


		/// <summary>
		/// Called to upload the hash map of the uploaded file
		/// </summary>
		public void UploadHashMap()
		{
			int 		entryCount;
			int 		blockSize;
			FileStream mapStream = null;
			try
			{
				//CreateHashMap();
				// Send the hash map
				
				mapStream = GetHashMap(out entryCount, out blockSize);
				if (mapStream == null)
				{
					Log.log.Debug("Hasp not generated, try with the file handle used for data upload");
					// Reset the stream position
					this.OutStream.Position = 0;
					map.CreateHashMapFileWithStream((FileStream)this.OutStream.fStream);
					mapStream = GetHashMap(out entryCount, out blockSize);
				}
				Log.log.Debug("mapStream.Length :{0}", mapStream.Length);
				mapStream.Position = 0;
				syncService.PutHashMap(mapStream, (int)mapStream.Length);
				//not saved in the simias client, may be in future for down sync perf. enhancement 
				//catch at ProcessFilesToServer
			}
			catch (Exception ex)
			{
				// If any exception comes we have to delete the haspmap on the server, log a defect and fix it for SP2
				Log.log.Info("Exception in uploading hashmap. {0}--{1}", ex.Message, ex.StackTrace);
			}
			finally
			{
				if(mapStream !=null)
					mapStream.Close();
				
				DeleteHashMap();				
			}			
		}		


		/// <summary>
		/// Called to get a string description of the Diffs.
		/// </summary>
		/// <param name="segments">An array of segment descriptions.</param>
		/// <returns>The string description.</returns>
		private string ReportDiffs(ArrayList segments)
		{
			StringWriter sw = new StringWriter();
			foreach (FileSegment segment in segments)
			{
				if (segment is BlockSegment)
				{
					BlockSegment bs = (BlockSegment)segment;
					sw.WriteLine("Found Match Offset = {0} Block {1} - {2}", bs.Offset, bs.StartBlock, bs.EndBlock);
				}
				else
				{
					OffsetSegment seg = (OffsetSegment)segment;
					sw.WriteLine("Found change size = {0}", seg.Length);
				}
			}
			return sw.ToString();
		}

		#endregion
	}

	#endregion
}
