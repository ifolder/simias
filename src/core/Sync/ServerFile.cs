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
using System.Collections;
using System.Xml;
using System.Threading;
using Simias.Storage;
using Simias.Sync.Delta;
using Simias.Client;

namespace Simias.Sync
{
	/// <summary>
	/// Class used on ther server to determine the changes from the client file.
	/// </summary>
	public class ServerInFile : InFile
	{
		SyncNode	snode;
		SyncPolicy	policy;
		//FileStream		mapSrcStream;

		#region Constructors
		
		/// <summary>
		/// Contructs a ServerFile object that is used to sync a file from the client.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="snode">The node to sync.</param>
		/// <param name="policy">The policy to check the file against.</param>
		public ServerInFile(Collection collection, SyncNode snode, SyncPolicy policy) :
			base(collection)
		{
			this.snode = snode;
			this.policy = policy;
		}

		#endregion

		/// <summary>
		/// Open the server file and validate access.
		/// </summary>
		/// <returns>Status of the open.</returns>
		public SyncStatus Open()
		{
			if (snode == null)
			{
				return SyncStatus.ClientError;
			}
			XmlDocument xNode = new XmlDocument();
			xNode.LoadXml(snode.node);
			node = (BaseFileNode)Node.NodeFactory(collection.StoreReference, xNode);
			// Don't allow journal files to be updated from the client.
			if (node.IsType("Journal"))
				return SyncStatus.Access;
			map = new HashMap(collection, node);
			if (!policy.Allowed(node))
			{
				SyncStatus ss = SyncStatus.Policy;
				switch (policy.FailedType)
				{
					case SyncPolicy.PolicyType.Quota:
						ss = SyncStatus.PolicyQuota;
						break;
					case SyncPolicy.PolicyType.Size:
						ss = SyncStatus.PolicySize;
						break;
					case SyncPolicy.PolicyType.Type:
						ss = SyncStatus.PolicyType;
						break;
				}
				return ss;
			}
			collection.ImportNode(node, true, snode.MasterIncarnation);
			node.IncarnationUpdate = node.LocalIncarnation;
			SyncStatus status = SyncStatus.Success;
			try
			{
				base.Open(node);
			}
			catch (InsufficientStorageException)
			{
				status = SyncStatus.DiskFull;
			}
			catch(IOException e1)
			{
				if( e1.Message.IndexOf("Sharing violation") != -1)
				{
					Log.log.Info("Sending the status as InUse.{0}--{1}", e1.Message, e1.StackTrace);
					status = SyncStatus.InUse;
				}
				else
					throw;
			}
			if (NameConflict)
			{
				status = SyncStatus.FileNameConflict;
				Close(false);
				return status;
			}

			//First no file node at server so let it go with no update conflict
			ulong diskNodeIncarnation = snode.MasterIncarnation;
			Node diskNode = ( node.DiskNode != null ) ? node.DiskNode : collection.GetNodeByID( node.ID );
			//if file already present in the server take the disk version and compare with the expected version which came from client
			if(diskNode!=null)
			{
				diskNodeIncarnation = diskNode.LocalIncarnation;
				Log.log.Debug("Disk node at server....");
			}
			else
				Log.log.Debug("No disk node at server....");
			
			Log.log.Debug("Client Incarnation :{0} Server Incarnation :{1}", node.ExpectedIncarnation, diskNodeIncarnation);
			
			if(node.ExpectedIncarnation != diskNodeIncarnation)
			{
				if(node.IsType( NodeTypes.BaseFileNodeType))
				{
					status = SyncStatus.UpdateConflict;
					Log.log.Debug("Update conflict for file node {0} client master :{1} client local:{2} disk local:{3} ", node.ID, snode.MasterIncarnation, snode.LocalIncarnation, diskNode.LocalIncarnation);
				}
				else
					Log.log.Debug("Update conflict for non file node {0}..conflict not set", node.ID);
			}
			
			if (status != SyncStatus.Success)
			{
				Close(false);
			}
				
			return status;
		}

		/// <summary>
		/// Called to close the file.
		/// </summary>
		/// <param name="commit">True if changes should be commited.</param>
		/// <returns>The status of the sync.</returns>
		public new SyncNodeStatus Close(bool commit)
		{
			SyncNodeStatus status = new SyncNodeStatus();
			status.nodeID = node.ID;
			status.status = SyncStatus.ClientError;

			try
			{
				status = base.Close(commit);  // modified for new code
			}
			catch (CollisionException)
			{
				status.status = SyncStatus.UpdateConflict;
			}
			catch (Exception ex)
			{
				status.status = SyncStatus.ServerFailure;
				Log.log.Info("Exception on Close {0}--{1}", ex.Message, ex.StackTrace);
			}

			return status;
		}

		/// <summary>
		/// Get a hashed map of the file.  This can then be
		/// used to create an upload or download filemap.
		/// </summary>
		/// <param name="entryCount">The number of hash entries.</param>
		/// <param name="blockSize">The size of the hashed data blocks.</param>
		/// <returns></returns>
		public FileStream GetHashMap(out int entryCount, out int blockSize)
		{
			return map.GetHashMapStream(out entryCount, out blockSize, false, oldNode.LocalIncarnation);
		}
		
		/// <summary>
		/// Put the hashmap for this file
		/// </summary>
		public void PutHashMap(Stream stream, int Size)
		{
			map.StoreHashMapFile(stream, Size);
		}
	}

	/// <summary>
	/// Class used on ther server to determine the changes from the client file.
	/// </summary>
	public class ServerOutFile : OutFile
	{
		#region Constructors

		/// <summary>
		/// Constructs a ServerFile object that can be used to sync a file in from a client.
		/// </summary>
		/// /// <param name="collection">The collection the node belongs to.</param>
		/// <param name="node">The node to sync down</param>
		public ServerOutFile(Collection collection, BaseFileNode node) :
			base(collection)
		{
			this.node = node;
			map = new HashMap(collection, node);
		}

		#endregion

		/// <summary>
		/// Open the file for download access.
		/// </summary>
		/// <param name="sessionID">The unique session ID.</param>
		public void Open(string sessionID)
		{
			base.Open(node, sessionID);
		}

		/// <summary>
		/// Called to close the file.
		/// </summary>
		/// <returns>The status of the sync.</returns>
		public new SyncNodeStatus Close()
		{
			SyncNodeStatus status = new SyncNodeStatus();
			status.nodeID = node.ID;
			status.status = SyncStatus.Success;
			base.Close();  // OutFile, base class code not modified
			return status;
		}

		/// <summary>
		/// Get a hashed map of the file.  This can then be
		/// used to create an upload or download filemap.
		/// </summary>
		/// <param name="entryCount">The number of hash entries.</param>
		/// <param name="blockSize">The size of the hashed data blocks.</param>
		/// <returns></returns>
		public new FileStream GetHashMap(out int entryCount, out int blockSize)
		{
			return map.GetHashMapStream(out entryCount, out blockSize, true, node.LocalIncarnation);
		}
	}
}
