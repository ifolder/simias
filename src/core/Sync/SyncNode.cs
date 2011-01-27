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
*                 $Modified by: Satyam <ssutapalli@novell.com>  26/06/2008   Added member details sync from server to client
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.IO;
using Simias.Storage;
using Simias.Client;
using Simias.Policy;

namespace Simias.Sync
{
	#region SyncNodeType

	/// <summary>
	/// Special Node types that sync deals with.
	/// </summary>
	public enum SyncNodeType : byte
	{
		/// <summary>
		/// This is a generic node.
		/// </summary>
		Generic,
		/// <summary>
		/// This node represents a file in the FS.
		/// </summary>
		File,
		/// <summary>
		/// This node represents a directory in the FS.
		/// </summary>
		Directory,
		/// <summary>
		/// This node represents a deleted node.
		/// </summary>
		Tombstone,
		/// <summary>
		/// This node represents a Journal or other types of managed file node.
		/// </summary>
		StoreFileNode
	}

	#endregion

	#region SyncNodeInfo

	/// <summary>
	/// class to represent the minimal information that the sync code needs
	/// to know about a node to determine if it needs to be synced.
	/// </summary>
	public class SyncNodeInfo: IComparable
	{
		#region Fields

		/// <summary>
		/// The Node ID.
		/// </summary>
		public string ID;
		/// <summary>
		/// The local incarnation for the node.
		/// </summary>
		public ulong LocalIncarnation;
		/// <summary>
		/// The Master incarnation for this node.
		/// </summary>
		public ulong MasterIncarnation;
		/// <summary>
		///	The base type of this node. 
		/// </summary>
		public SyncNodeType NodeType;
		/// <summary>
		/// The SyncOperation to perform on this node.
		/// </summary>
		public SyncOperation Operation;
		/// <summary>
		/// The size of this instance serialized.
		/// If fields are added this needs to be updated.
		/// </summary>
		public static int InstanceSize = 34;

		#endregion

		#region Constructor

		/// <summary>
		/// Empty Constructor
		/// </summary>
		public SyncNodeInfo()
		{
		}

		/// <summary>
		/// Constructs a SyncNodeInfo from a ShallowNode.
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="sn"></param>
		public SyncNodeInfo(Collection collection, ShallowNode sn) :
			this (new Node(collection, sn))
		{
		}

		/// <summary>
		/// Construct a SyncNodeInfo from a stream.
		/// </summary>
		/// <param name="reader"></param>
		internal SyncNodeInfo(BinaryReader reader)
		{
			this.ID = new Guid(reader.ReadBytes(16)).ToString();
			this.LocalIncarnation = reader.ReadUInt64();
			this.MasterIncarnation = reader.ReadUInt64();
			this.NodeType = (SyncNodeType)reader.ReadByte();
			this.Operation = (SyncOperation)reader.ReadByte();
		}

		/// <summary>
		/// Construct a SyncNodeStamp from a Node.
		/// </summary>
		/// <param name="node">the node to use.</param>
		internal SyncNodeInfo(Node node)
		{
			this.ID = node.ID;
			this.LocalIncarnation = node.LocalIncarnation;
			this.MasterIncarnation = node.MasterIncarnation;
			this.NodeType = GetSyncNodeType(node.Type);
			this.Operation = node.Type == NodeTypes.TombstoneType ? SyncOperation.Delete : SyncOperation.Unknown;
		}

		/// <summary>
		/// Consturct a SyncNodeStamp from a ChangeLogRecord.
		/// </summary>
		/// <param name="record">The record to use.</param>
		internal SyncNodeInfo(ChangeLogRecord record)
		{
			this.ID = record.EventID;
			this.LocalIncarnation = record.SlaveRev;
			this.MasterIncarnation = record.MasterRev;
			this.NodeType = GetSyncNodeType(record.Type.ToString());
			switch (record.Operation)
			{
				case ChangeLogRecord.ChangeLogOp.Changed:
					this.Operation = SyncOperation.Change;
					break;
				case ChangeLogRecord.ChangeLogOp.Created:
					this.Operation = SyncOperation.Create;
					break;
				case ChangeLogRecord.ChangeLogOp.Deleted:
					this.Operation = SyncOperation.Delete;
					break;
				case ChangeLogRecord.ChangeLogOp.Renamed:
					this.Operation = SyncOperation.Rename;
					break;
				default:
					this.Operation = SyncOperation.Unknown;
					break;
			}
		}

		#endregion
		
		#region publics

		/// <summary>
		/// Serializes this instance into a stream.
		/// </summary>
		/// <param name="writer">The stream to serialize to.</param>
		internal void Serialize(BinaryWriter writer)
		{
			writer.Write(new Guid(ID).ToByteArray());
			writer.Write(LocalIncarnation);
			writer.Write(MasterIncarnation);
			writer.Write((byte)NodeType);
			writer.Write((byte)Operation);
		}


		/// <summary> implement some convenient operator overloads </summary>
		public int CompareTo(object obj)
		{
			return ID.CompareTo(((SyncNodeInfo)obj).ID);
		}

		#endregion

		#region privates

		/// <summary>
		/// Converts the base type string into a SyncNodeType
		/// </summary>
		/// <param name="baseType">The base type.</param>
		/// <returns>the SyncNodeType.</returns>
		private SyncNodeType GetSyncNodeType(string baseType)
		{
			if (baseType == NodeTypes.DirNodeType)
			{
				return SyncNodeType.Directory;
			}
			else if (baseType == NodeTypes.FileNodeType)
			{
				return SyncNodeType.File;
			}
			else if(baseType == NodeTypes.StoreFileNodeType)
			{
				return SyncNodeType.StoreFileNode;
			}
			else if (baseType == NodeTypes.TombstoneType)
			{
				return SyncNodeType.Tombstone;
			}
			else
			{
				return SyncNodeType.Generic;
			}
		}

		#endregion
	}

	#endregion

	#region SyncNode

	/// <summary>
	/// This is the object that is used to sync a node.
	/// </summary>
	public class SyncNode : SyncNodeInfo
	{
private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncNode));
		#region fields

		/// <summary>
		/// The node as an XML string.
		/// </summary>
		public string node;

		/// <summary>
		/// The size of this instance serialized.
		/// </summary>
		public new int InstanceSize
		{
			get { return SyncNodeInfo.InstanceSize + node.Length; }
		}
		
		#endregion

		#region Constructor

		/// <summary>
		/// 
		/// </summary>
		internal SyncNode()
		{
		}

		/// <summary>
		/// Create a SyncNode from a Node.
		/// </summary>
		/// <param name="node">The node used to create the sync node.</param>
		internal SyncNode(Node node): this( node, false)
		{
			// If the data is being moved dont strip the local properties...
		}

        /// <summary>
        /// Create a sync node from a Node and whether data to move or not
        /// </summary>
        /// <param name="node">Node used to create sync node</param>
        /// <param name="DataMovement">Whether to move data or not</param>
		internal SyncNode(Node node, bool DataMovement) : base(node)
		{
			if( DataMovement == false )
				this.node = node.Properties.ToString(true);
			else
				this.node = node.Properties.ToString(false);
		}

		/// <summary>
		/// Create a SyncNode from a Node, modify s.t. only required policies are sent.
		/// </summary>
		/// <param name="node">The node used to create the sync node.</param>
		/// <param name="UserID"></param>
		internal SyncNode(Node node, string UserID) :
			base(node)
		{
			//Node tempnode = node;
			//bool IsSystemPolicy = (node.Type == NodeTypes.PolicyType && ((bool)node.Properties.GetSingleProperty(PropertyTags.SystemPolicy).Value) == true );
			bool LoggedInMember = false;
            		if (node.Type == NodeTypes.MemberType)
            		{
                		Member MemSyncObject = new Member(node);
                		if ((MemSyncObject.UserID == UserID ))
                		{
                    			LoggedInMember = true;
                    			Store store = Store.GetStore();
                    			Domain domain = store.GetDomain(store.DefaultDomain);

                    			Member member = domain.GetMemberByID(UserID);

                    			// Do a aggregate of system, group and user level policies
                    			AggregateAllPolicies(member, ref node);
                		}

				// client does not need secondary admin rights details. Since old client does not understand "Secondary" rigths, so 
				// replace it by ReadWrite.
				Property AceProp = node.Properties.GetSingleProperty(PropertyTags.Ace);
				if(AceProp != null)
				{
					string AceValue = AceProp.Value as string;
					if(AceValue.IndexOf("Secondary") >= 0 )
					{
						//StringBuilder NewAceValue = new StringBuilder(AceValue);
						string NewAceValue = AceValue.Replace("Secondary", "ReadWrite");
						Property NewAceProp = new Property(PropertyTags.Ace, NewAceValue);
						log.Debug("new ace prop going is :"+NewAceValue);
						node.Properties.ModifyNodeProperty(NewAceProp);
					}
				}
				else log.Debug("aceProp was null for this id :"+MemSyncObject.UserID);
				
            		}
			this.node = node.Properties.ToString(true, LoggedInMember);
		}

		/// <summary>
		/// Create a SyncNode from a stream.
		/// </summary>
		/// <param name="reader">The stream containing the SyncNode.</param>
		internal SyncNode(BinaryReader reader) :
			base(reader)
		{
			this.MasterIncarnation = reader.ReadUInt64();
			node = reader.ReadString();
		}

		#endregion

		#region publics

		/// <summary>
		/// Serialize the SyncNode to the stream.
		/// </summary>
		/// <param name="writer"></param>
		internal new void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(MasterIncarnation);
			writer.Write(node);
		}

        /// <summary>
        /// Collect the list of policies for a member for a particular node
        /// </summary>
        /// <param name="member">Member for which policies list needed</param>
        /// <param name="node">Reference Node with all policies</param>
		internal void AggregateAllPolicies(Member member , ref Node node)
		{
			Store store = Store.GetStore();
			//Domain domain = store.GetDomain(store.DefaultDomain);

			long AggregateNoOfiFoldersLimit = Simias.Policy.iFolderLimit.Get(member).Limit;	

			//All the encryption policies are local properties, but still they need to go
			//to clients for encryption related operations. In future this needs to be changed
			//to client local properties.
			if(member.RAName != "")
			{
				Property p = new Property(PropertyTags.RAName, member.RAName);
			 	node.Properties.DeleteNodeProperties(PropertyTags.RAName);
				node.Properties.ModifyNodeProperty(p);
			}

			if(member.EncryptionKey != "")
			{
				Property p = new Property(PropertyTags.EncryptionKey, member.EncryptionKey);
			 	node.Properties.DeleteNodeProperties(PropertyTags.EncryptionKey);
				node.Properties.ModifyNodeProperty(p);
			}

			if(member.EncryptionVersion != "")
			{
				Property p = new Property(PropertyTags.EncryptionVersion, member.EncryptionVersion);
			 	node.Properties.DeleteNodeProperties(PropertyTags.EncryptionVersion);
				node.Properties.ModifyNodeProperty(p);
			}

			if(member.EncryptionBlob != "")
			{
				Property p = new Property(PropertyTags.EncryptionBlob, member.EncryptionBlob);
			 	node.Properties.DeleteNodeProperties(PropertyTags.EncryptionBlob);
				node.Properties.ModifyNodeProperty(p);
			}
			
			if(member.EncryptionType != "")
			{
				Property p = new Property(PropertyTags.EncryptionType, member.EncryptionType);
			 	node.Properties.DeleteNodeProperties(PropertyTags.EncryptionType);
				node.Properties.ModifyNodeProperty(p);
			}

			if(member.RAPublicKey != "")
			{
				Property p = new Property(PropertyTags.RAPublicKey, member.RAPublicKey);
			 	node.Properties.DeleteNodeProperties(PropertyTags.RAPublicKey);
				node.Properties.ModifyNodeProperty(p);
			}

			



			Rule rule = new Rule( AggregateNoOfiFoldersLimit, Rule.Operation.Greater, Rule.Result.Deny );
			node.Properties.ModifyNodeProperty(Simias.Policy.iFolderLimit.iFolderLimitPolicyID, rule.ToString() );

			long AggregateDiskSpaceQuota = Simias.Policy.DiskSpaceQuota.Get(member).Limit;
			Rule DiskSpaceRule = new Rule( AggregateDiskSpaceQuota, Rule.Operation.Greater, Rule.Result.Deny );

			node.Properties.ModifyNodeProperty(Simias.Policy.DiskSpaceQuota.DiskSpaceQuotaPolicyID, DiskSpaceRule.ToString() );
			long UsedDiskSpace = Simias.Policy.DiskSpaceQuota.Get(member).UsedSpace;
			
			node.Properties.ModifyNodeProperty(Simias.Policy.DiskSpaceQuota.UsedDiskSpaceOnServer, UsedDiskSpace);

			Simias.Policy.SyncInterval SyncIntervalObject = Simias.Policy.SyncInterval.Get(member);
			int AggregateSyncInterval = SyncIntervalObject.Interval;
			node.Properties.ModifyNodeProperty(Simias.Policy.SyncInterval.SyncIntervalPolicyID, AggregateSyncInterval );

			long AggregateFileSizeLimit = Simias.Policy.FileSizeFilter.Get(member).Limit;
			Rule FileSizeRule = new Rule( AggregateFileSizeLimit, Rule.Operation.Greater, Rule.Result.Deny );
			node.Properties.ModifyNodeProperty(Simias.Policy.FileSizeFilter.FileSizeFilterPolicyID, FileSizeRule.ToString() );

			int AggregateEncryptionStatus = Simias.Policy.SecurityState.Get(member);
			node.Properties.ModifyNodeProperty(Simias.Policy.SecurityState.EncryptionStatePolicyID, AggregateEncryptionStatus );

			int AggregateSharingStatus = Simias.Policy.Sharing.Get(member);
			node.Properties.ModifyNodeProperty(Simias.Policy.Sharing.SharingPolicyID, AggregateSharingStatus );

			//Simias.Policy.FileTypeFilter.GetPatterns(domain)
		}

		#endregion
	}

	#endregion


	/// <summary>
	/// Used to report the status of a sync.
	/// </summary>
	public class SyncNodeStatus
	{
		#region Fields

		/// <summary>
		/// The ID of the node.
		/// </summary>
		public string		nodeID;
		/// <summary>
		/// The status of the sync.
		/// </summary>
		public SyncStatus	status;

		/// <summary>
		/// The size of this instance serialized.
		/// </summary>
		public int InstanceSize = 17;

		#endregion
	
		/// <summary>
		/// Constructs a SyncNodeStatus object.
		/// </summary>
		internal SyncNodeStatus()
		{
			status = SyncStatus.Error;
		}

		/// <summary>
		/// Create a SyncNodeStatus from a stream.
		/// </summary>
		/// <param name="reader">The stream containing the SyncNode.</param>
		internal SyncNodeStatus(BinaryReader reader)
		{
			this.nodeID = new Guid(reader.ReadBytes(16)).ToString();
			this.status = (SyncStatus)reader.ReadByte();
		}

		/// <summary>
		/// Serialize the SyncNodeStatus to the stream.
		/// </summary>
		/// <param name="writer"></param>
		internal void Serialize(BinaryWriter writer)
		{
			writer.Write(new Guid(nodeID).ToByteArray());
			writer.Write((byte)status);
		}
	}
}
