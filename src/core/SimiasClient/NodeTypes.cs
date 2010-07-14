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
*                 $Author: Mike Lasky <mlasky@novell.com>
*                 $Modified by: Kalidas Balakrishnan <bkalidas@novell.com>
*                 $Mod Date: 14 Sep, 2008
*                 $Revision: 0.2
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/


using System;

namespace Simias.Client
{
	/// <summary>
	/// Contains class names derived from Node that are used as Node object types.
	/// </summary>
	public class NodeTypes
	{
		#region Class Members
		/// <summary>
		/// Base types for a node object.
		/// </summary>
		public enum NodeTypeEnum
		{
			/// <summary>
			/// BaseFileNode
			/// </summary>
			BaseFileNode,

			/// <summary>
			/// Collection
			/// </summary>
			Collection,

			/// <summary>
			/// DirNode
			/// </summary>
			DirNode,

			/// <summary>
			/// Domain
			/// </summary>
			Domain,

			/// <summary>
			/// FileNode
			/// </summary>
			FileNode,

			/// <summary>
			/// Identity
			/// </summary>
			Identity,

			/// <summary>
			/// LinkNode
			/// </summary>
			LinkNode,

			/// <summary>
			/// LocalDatabase
			/// </summary>
			LocalDatabase,

			/// <summary>
			/// Member
			/// </summary>
			Member,

			/// <summary>
			/// Message
			/// </summary>
			Message,

			/// <summary>
			/// Node
			/// </summary>
			Node,

			/// <summary>
			/// NotificationLog
			/// </summary>
			NotificationLog,

			/// <summary>
			/// POBox
			/// </summary>
			POBox,

			/// <summary>
			/// Policy
			/// </summary>
			Policy,

			/// <summary>
			/// StoreFileNode
			/// </summary>
			StoreFileNode,

			/// <summary>
			/// Subscription
			/// </summary>
			Subscription,

			/// <summary>
			/// Tombstone
			/// </summary>
			Tombstone,
		};

		#endregion

		#region Properties

		/// <summary>
		/// Gets the BaseFileNode class name.
		/// </summary>
		static public string BaseFileNodeType
		{
			get { return NodeTypeEnum.BaseFileNode.ToString(); }
		}

		/// <summary>
		/// Gets the Collection class name.
		/// </summary>
		static public string CollectionType
		{
			get { return NodeTypeEnum.Collection.ToString(); }
		}

		/// <summary>
		/// Gets the DirNode class name.
		/// </summary>
		static public string DirNodeType
		{
			get { return NodeTypeEnum.DirNode.ToString(); }
		}


		/// <summary>
		/// Gets the Domain class name.
		/// </summary>
		static public string DomainType
		{
			get { return NodeTypeEnum.Domain.ToString(); }
		}

		/// <summary>
		/// Gets the FileNode class name.
		/// </summary>
		static public string FileNodeType
		{
			get { return NodeTypeEnum.FileNode.ToString(); }
		}

		/// <summary>
		/// Gets the Identity class name.
		/// </summary>
		static public string IdentityType
		{
			get { return NodeTypeEnum.Identity.ToString(); }
		}

		/// <summary>
		/// Gets the LinkNode class name.
		/// </summary>
		static public string LinkNodeType
		{
			get { return NodeTypeEnum.LinkNode.ToString(); }
		}

		/// <summary>
		/// Gets the LocalDatabase class name.
		/// </summary>
		static public string LocalDatabaseType
		{
			get { return NodeTypeEnum.LocalDatabase.ToString(); }
		}

		/// <summary>
		/// Gets the Member class name.
		/// </summary>
		static public string MemberType
		{
			get { return NodeTypeEnum.Member.ToString(); }
		}

		/// <summary>
		/// Gets the Message class name.
		/// </summary>
		static public string MessageType
		{
			get { return NodeTypeEnum.Message.ToString(); }
		}

		/// <summary>
		/// Gets the Node class name.
		/// </summary>
		static public string NodeType
		{
			get { return NodeTypeEnum.Node.ToString(); }
		}

		/// <summary>
		/// Gets the NotificationLog class name.
		/// </summary>
		static public string NotificationLogType
		{
			get { return NodeTypeEnum.NotificationLog.ToString(); }
		}

		/// <summary>
		/// Gets the POBox class name.
		/// </summary>
		static public string POBoxType
		{
			get { return NodeTypeEnum.POBox.ToString(); }
		}

		/// <summary>
		/// Gets the Policy class name.
		/// </summary>
		static public string PolicyType
		{
			get { return NodeTypeEnum.Policy.ToString(); }
		}

		/// <summary>
		/// Gets the StoreFileNode class name.
		/// </summary>
		static public string StoreFileNodeType
		{
			get { return NodeTypeEnum.StoreFileNode.ToString(); }
		}

		/// <summary>
		/// Gets the Subscription class name.
		/// </summary>
		static public string SubscriptionType
		{
			get { return NodeTypeEnum.Subscription.ToString(); }
		}

		/// <summary>
		/// Gets the Tombstone class name.
		/// </summary>
		static public string TombstoneType
		{
			get { return NodeTypeEnum.Tombstone.ToString(); }
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Returns whether specified class name is a NodeType.
		/// </summary>
		/// <param name="type">Class name string.</param>
		/// <returns>True if specified class name is a Node object type. Otherwise false is returned.</returns>
		static public bool IsNodeType( string type )
		{
			bool isType = false;

			try
			{
				Enum.Parse( typeof( NodeTypeEnum ), type, true );
				isType = true;
			}
			catch
			{}

			return isType;
		}
		#endregion
	}
}
