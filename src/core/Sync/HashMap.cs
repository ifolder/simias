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
*                 $Author: Dale Olds <olds@novell.com>
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
using System.Collections;
using System.Threading;
using Simias.Storage;

namespace Simias.Sync.Delta
{
	#region HashData

	/// <summary>
	/// Class used to keep track of the file Blocks and hash
	/// codes assosiated with the block.
	/// </summary>
	public class HashData
	{
		/// <summary>
		/// The serialized size of the instance.
		/// </summary>
		public static int InstanceSize = 4 + 4 + 16;
		/// <summary>
		/// The Block number that this hash represents. 0 based.
		/// </summary>
		public int		BlockNumber;
		/// <summary>
		/// The Weak or quick hash of this block.
		/// </summary>
		public UInt32	WeakHash;
		/// <summary>
		/// The strong hash of this block.
		/// </summary>
		public byte[]	StrongHash;

		/// <summary>
		/// Constructs a HashData Object.
		/// </summary>
		/// <param name="blockNumber"></param>
		/// <param name="weakHash"></param>
		/// <param name="strongHash"></param>
		public HashData(int blockNumber, UInt32 weakHash, byte[] strongHash)
		{
			this.BlockNumber = blockNumber;
			this.WeakHash = weakHash;
			this.StrongHash = strongHash;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reader">Initialize with reader details</param>
		public HashData(BinaryReader reader)
		{
			BlockNumber = reader.ReadInt32();
			WeakHash = reader.ReadUInt32();
			StrongHash = reader.ReadBytes(16);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(BlockNumber);
			writer.Write(WeakHash);
			writer.Write(StrongHash);
		}
	}

	#endregion

	#region HashMap

	/// <summary>
	/// Used to Write and Get HashMaps of a stream.
	/// </summary>
	public class HashMap
	{
		const string			MapFilePrefix = ".simias.map.";
		Collection				collection;
		BaseFileNode			node;
		string					file;
		static int				MaxThreadCount = 20;
		static int				ThreadCount = 0;
		static Queue			mapQ = new Queue();
		static AutoResetEvent	queueEvent = new AutoResetEvent(false);
		delegate void	HashMapDelegate();
		static int				version = 1;

		bool mapState;

        /// <summary>
        /// Get the Map file
        /// </summary>
		public string MapFile
		{
			get {return file;}
		}

        /// <summary>
        /// Get the Map State
        /// </summary>
		public bool MapState
		{
			get
			{
				return this.mapState;
			}
			set
			{
				this.mapState = value;
			}
		}

		internal struct HashFileHeader
		{
			static byte[]	signature = {(byte)'!', (byte)'M', (byte)'a', (byte)'P', (byte)'f', (byte)'I', (byte)'l', (byte)'e'};
			static int		headerSize = 24;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="reader"></param>
			/// <param name="blockSize"></param>
			/// <param name="entryCount"></param>
			/// <param name="nodeRev">The revision of the node.</param>
			/// <returns></returns>
			internal static bool ReadHeader(BinaryReader reader, out int blockSize, out int entryCount, ulong nodeRev)
			{
				byte[] sig = reader.ReadBytes(8);
				blockSize = reader.ReadInt32();
				int ver = reader.ReadInt32();
				ulong fileRev = reader.ReadUInt64();
				entryCount = 0;
				if (sig.Length == signature.Length)
				{
					for (int i= 0; i < sig.Length; ++i)
					{
						if (sig[i] != signature[i])
							return false;
					}
					if (version != ver)
						return false;
					if (fileRev != nodeRev)
						return false;
					entryCount = (int)((reader.BaseStream.Length - headerSize)/ HashData.InstanceSize);
					return true;
				}
				return false;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="blockSize"></param>
			/// <param name="nodeRev">The revision of the node.</param>
			internal static void WriteHeader(BinaryWriter writer, int blockSize, ulong nodeRev)
			{
				writer.Write(signature);
				writer.Write(blockSize);
				writer.Write(version);
				writer.Write(nodeRev);
			}
		}

		// We want to keep Map file size small enough that it will work over a 56Kb/Sec modem.
		// Lets try to get the file in 60 seconds.
		// In 60 seconds we can transfer 56k / 8 = 430,080 bytes of data.
		// In 60 seconds we can transfer 17920 blocks rount to 18000
		// We are assuming that larger files are less likely to change.
		static int maxBlocks = 18000;

		/// <summary>
		/// Constructor for Hash Map to create from Collection and Node
		/// </summary>
		/// <param name="collection">Collection to create hashmap</param>
		/// <param name="node">File node to add</param>
		public HashMap(Collection collection, BaseFileNode node)
		{
			this.collection = collection;
			this.node = node;
			file = Path.Combine(collection.ManagedPath, MapFilePrefix + node.ID);
			this.mapState = false;
		}

		/// <summary>
		/// Add the code here to do in the hash map thread
		/// </summary>
		private static void HashMapThread()
		{
			// BUGBUG Encryption Here.
			// Not needed map is generated on client.
			
		
			// Now see if we have any work queued.
			while (true)
			{
				// If we have had no work for 5 min exit thread.
				bool timedOut = !queueEvent.WaitOne(5 * 60 * 1000, false);
				try
				{
					while (true)
					{
						HashMapDelegate hmd;
						lock (mapQ)
						{
							hmd = mapQ.Count > 0 ? mapQ.Dequeue() as HashMapDelegate : null;
							if (hmd == null)
							{
								if (timedOut)
								{
									// Exit the thread.
									--ThreadCount;
									return;
								}
								break;
							}
						}
						try { hmd(); }
						catch { /* Don't let the thread go away.*/ }
					}
				}
				catch{}
			}
		}

	/// <summary>
	/// Create a hash map file to sync
	/// </summary>
        public void CreateHashMapFile()
        {
	
		FileStream mapSrcStream = null;
		try
		{
			mapSrcStream = File.Open(node.GetFullPath(collection), FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		catch (Exception ex)
		{
			Log.log.Debug("CreateHashMapFile: Exception in opening the file: {0}.. {1}..{2}", node.GetFullPath(collection), ex.Message, ex.StackTrace);
		}
		
		if (mapSrcStream == null)
		{
			Log.log.Debug("CreateHashMapFile: map stream null. setting mapCreationFailed flag to true");			
			MapState = true;
		}
		else
		{
			try
			{
				CreateHashMapFileWithStream(mapSrcStream);
				MapState = true;
			}
			catch (Exception e)
			{
				Log.log.Debug("CreateHashMapFile (retval): Exception {0}--{1}", e.Message, e.StackTrace);
				MapState = true;
			}
		}
        }

	/// <summary>
        /// Create a hash map file using file stream
	/// </summary>
	/// <param name="mapSrcStream">File stream thru which hash map file will be created</param>
	public void CreateHashMapFileWithStream(FileStream mapSrcStream)
	{
		// Makre sure that mapState is set to true for succes anf failure cases
		
		if (mapSrcStream == null)
		{
			Log.log.Debug("CreateHashMapFileWithStream: The mapsrcstream is null");
			MapState = true;
			return;
		}
		
		
		int blockSize = CalculateBlockSize(mapSrcStream.Length);
		try
		{
			string mapFile = file;
			string tmpMapFile = mapFile + ".tmp";
			// Copy the current file to a tmp name.
			if (File.Exists(mapFile))
				File.Move(mapFile, tmpMapFile);

			BinaryWriter writer = new BinaryWriter( File.OpenWrite(tmpMapFile));

			// Write the header.
			HashFileHeader.WriteHeader(writer, blockSize, node.LocalIncarnation);
			try
			{
				mapSrcStream.Position = 0;
				HashMap.SerializeHashMap(mapSrcStream, writer, blockSize);
				writer.Close();                
				
				File.Move(tmpMapFile, mapFile);
				File.SetCreationTime(mapFile, node.CreationTime);
				File.SetLastWriteTime(mapFile, node.LastWriteTime);                
			}
			catch (Exception ex)
			{
				Log.log.Debug("CreateHashMapFileWithStream Exception in {0}--{1}", ex.Message, ex.StackTrace);
				writer.Close();
				writer = null;
				File.Delete(mapFile);
				if (File.Exists(tmpMapFile))
				File.Move(tmpMapFile, mapFile);
				
				MapState=true;
				
				throw ex;
			}
			finally
			{
				if (File.Exists(tmpMapFile))
				File.Delete(tmpMapFile);
			}
		}
		finally
		{
			// Close the file.
			mapSrcStream.Close();
		}
		MapState=true;
	}

		/// <summary>
		/// Calculate the block size to use for hash blocks
		/// </summary>
		/// <param name="streamSize">The size of the file.</param>
		/// <returns>The blockSize</returns>
		private static int CalculateBlockSize(long streamSize)
		{
			long size = streamSize / HashMap.maxBlocks;
			if (size < 0x1000)
				return 0x1000;
			if (size < 0x2000)
				return 0x2000;
			if (size < 0x4000)
				return 0x4000;
			if (size < 0x8000)
				return 0x8000;
			if (size < 0x10000)
				return 0x10000;
			if (size < 0x20000)
				return 0x20000;
			if (size < 0x40000)
				return 0x40000;
			return 0x80000;
		}


		/// <summary>
		/// Create a hash map
		/// </summary>
		internal void CreateHashMap()
		{
			// Delete the file now.
			Delete();
			bool startThread = false;
			lock (mapQ)
			{
				mapQ.Enqueue(new HashMapDelegate(CreateHashMapFile));
				if (ThreadCount == 0 || (mapQ.Count > 1 && ThreadCount < MaxThreadCount))
				{
					// Startup a thread.
					startThread = true;
					++ThreadCount;
				}
			}
			if (startThread)
			{
				Thread thread = new Thread(new ThreadStart(HashMapThread));
                thread.Name = "Hash Map Generation";
                //thread.Priority = ThreadPriority.BelowNormal;
				thread.IsBackground = true;
				thread.Start();
			}
			queueEvent.Set();
		}

        /// <summary>
        /// Delete the hash map matching collection and node
        /// </summary>
        /// <param name="collection">Collection which has to be removed from hash map</param>
        /// <param name="node">Node which has to be removed</param>
		internal static void Delete(Collection collection, BaseFileNode node)
		{
			new HashMap(collection, node).Delete();
		}

        /// <summary>
        /// Delete the file if exists
        /// </summary>
		internal void Delete()
		{
			if (File.Exists(file))
				File.Delete(file);
		}

		/// <summary>
		/// Gets the array of HashMap
		/// </summary>
		/// <param name="entryCount">The number of hash entries.</param>
		/// <param name="blockSize">The size of the data blocks that were hashed.</param>
		/// <param name="create">If true create hashmap on error.</param>
		/// <param name="mapRev">The desired map revision.</param>
		/// <returns></returns>
		internal FileStream GetHashMapStream(out int entryCount, out int blockSize, bool create, ulong mapRev)
		{
			if (File.Exists(file))
			{
				FileStream stream = File.OpenRead(file);
				if (HashFileHeader.ReadHeader(new BinaryReader(stream), out blockSize, out entryCount, mapRev))
				{
					return stream;
				}
				stream.Close();			
			}
			
		//Since we do not create hash map at server we are commenting this	
		/*	if (create)
				this.CreateHashMap();
			else
				Delete();
		*/		

			entryCount = 0;
			blockSize = 0;
			return null;
		}

		/// <summary>
		/// Store the hash map
		/// </summary>
		/// <param name="stream">Stream from whcih file comes</param>
		/// <param name="Size">Size of the file</param>
		public void StoreHashMapFile(Stream stream, int Size)
		{
			string mapFile = file;
			
			// Delete the existing file, any way we are going to store the new one
			if(File.Exists(mapFile))
				Delete();
			
			FileStream HashStream = File.OpenWrite(mapFile);
			try
			{
				StreamStream ss = new StreamStream(HashStream);
				ss.Write(stream, Size);
			}
			catch (Exception ex)
			{
				throw ex;
			}				
			finally
			{
				HashStream.Close();
			}
		}

        /// <summary>
        /// Deserialize the hash map 
        /// </summary>
        /// <param name="reader">Binary reader object</param>
        /// <param name="count">Number objects to create</param>
        /// <returns></returns>
		public static HashData[] DeSerializeHashMap(BinaryReader reader, int count)
		{
			HashData[] fileMap = new HashData[count];
			for (int i = 0; i < count; ++i)
			{
				fileMap[i] = new HashData(reader);
			}
			return fileMap;
		}
		
		/// <summary>
		/// Return the number of Blocks that the HashMap will need.
		/// </summary>
		/// <param name="streamSize"></param>
		/// <param name="blockSize"></param>
		/// <returns></returns>
		public static int GetBlockCount(long streamSize, out int blockSize)
		{
			blockSize = CalculateBlockSize(streamSize);
			return (int)((streamSize + blockSize -1)/ blockSize);
		}

		/// <summary>
		/// Serialized the Hash Created from the input stream to the writer stream.
		/// </summary>
		/// <param name="inStream">The stream of raw data to create the HashMap from.</param>
		/// <param name="writer">The stream to write the HashMap to.</param>
		/// <param name="blockSize">The size of the hashed data blocks.</param>
		public static void SerializeHashMap(Stream inStream, BinaryWriter writer, int blockSize)
		{
			//
			//if (inStream.Length <= blockSize)
			//{
			//	return;
			//}
			
			byte[]			buffer = new byte[blockSize];
			StrongHash		sh = new StrongHash();
			WeakHash		wh = new WeakHash();
			int				bytesRead;
			int				currentBlock = 0;
		
			// Compute the hash codes.
			inStream.Position = 0;
			while ((bytesRead = inStream.Read(buffer, 0, blockSize)) != 0)
			{
				new HashData(
					currentBlock++,
					wh.ComputeHash(buffer, 0, (UInt16)bytesRead),
					sh.ComputeHash(buffer, 0, bytesRead)).Serialize(writer);
			}
		}
	}
	
	#endregion
}
