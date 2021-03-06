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
using System.Text;

namespace Simias.Client.Event
{
	/// <summary>
	/// The Event types supported.
	/// </summary>
	[Flags]
	public enum EventType : short
	{
		/// <summary>
		/// The event is for a node create.
		/// </summary>
		NodeCreated = 1,
		/// <summary>
		/// The event is for a node delete.
		/// </summary>
		NodeDeleted = 2,
		/// <summary>
		/// The event is for a node change.
		/// </summary>
		NodeChanged = 4,
		/// <summary>
		/// The event is for no access.
		/// </summary>
		NoAccess = 8
	};

	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public class NodeEventArgs : SimiasEventArgs
	{
		#region Fields

		string					source;
		string					id;
		string					collection;
		string					modifier;
		string					type;
		int						eventId;
		ulong					masterRev;
		ulong					slaveRev;
		long					fileSize;

		/// <summary>
		/// Flags for the node event.
		/// </summary>
		[Flags]
		public enum EventFlags : ushort
		{
			/// <summary>
			/// The event was caused by local only properties.
			/// </summary>
			LocalOnly = 1,
		}

		EventFlags				flags;
		
		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a SimiasEventArgs that will be used by CollectionHandler delegates.
		/// Describes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="node">The object of the event.</param>
		/// <param name="collection">The Collection that the node belongs to.</param>\
		/// <param name="type">The Type of the Node.</param>
		/// <param name="changeType">The type of change that occured.</param>
		/// <param name="eventId">A user defined event ID. Only has meaning to a publisher.</param>
		/// <param name="time">The time of the event.</param>
		/// <param name="masterRev">The master revision for the node.</param>
		/// <param name="slaveRev">The local revision for the node.</param>
		/// <param name="fileSize">The length of the file if the node is a BaseFileNode. Otherwise
		/// the value of this parameter will be zero.</param>
		public NodeEventArgs(string source, string node, string collection, string type, EventType changeType, int eventId, DateTime time, ulong masterRev, ulong slaveRev, long fileSize) :
			this(source, node, collection, string.Empty, type, changeType, eventId, time, masterRev, slaveRev, fileSize)
		{
		}

		/// <summary>
		/// Constructs a SimiasEventArgs that will be used by CollectionHandler delegates.
		/// Describes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="node">The object of the event.</param>
		/// <param name="collection">The Collection that the node belongs to.</param>
		/// <param name="modifier">The user that modified the node.</param>
		/// <param name="type">The Type of the Node.</param>
		/// <param name="changeType">The type of change that occured.</param>
		/// <param name="eventId">A user defined event ID. Only has meaning to a publisher.</param>
		/// <param name="time">The time of the event.</param>
		/// <param name="masterRev">The master revision for the node.</param>
		/// <param name="slaveRev">The local revision for the node.</param>
		/// <param name="fileSize">The length of the file if the node is a BaseFileNode. Otherwise
		/// the value of this parameter will be zero.</param>
		public NodeEventArgs(string source, string node, string collection, string modifier, string type, EventType changeType, int eventId, DateTime time, ulong masterRev, ulong slaveRev, long fileSize) :
			base(changeType.ToString(), time)
		{
			this.source = source;
			this.id = node;
			this.collection = collection;
			this.modifier = modifier;
			this.type = type;
			this.eventId = eventId;
			this.masterRev = masterRev;
			this.slaveRev = slaveRev;
			this.fileSize = fileSize;
		}

		#endregion
		
		#region Properties

		/// <summary>
		/// Gets the string that represents the source of the event.
		/// </summary>
		public string Source
		{
			get {return source;}
		}

		/// <summary>
		/// Gets the ID of the affected Node/Collection.
		/// </summary>
		public string ID
		{
			get {return id;}
		}
		
		/// <summary>
		/// Gets the containing collection ID.
		/// </summary>
		public string Collection
		{
			get {return collection;}
		}

		/// <summary>
		/// Gets the ID of the user that modified the node.
		/// </summary>
		public string Modifier
		{
			get {return modifier;}
		}

		/// <summary>
		/// Gets the Type of the affected Node.
		/// </summary>
		public string Type
		{
			get {return type;}
		}

		/// <summary>
		/// Gets a Sets an event ID.  Usually 0. 
		/// Used by a publisher. Can be used to detect circular events.
		/// </summary>
		public int EventId
		{
			get {return eventId;}
		}

		/// <summary>
		/// Gets the Node ID.
		/// </summary>
		public string Node
		{
			get {return ID;}
		}

		/// <summary>
		/// Gets or sets the flags.
		/// </summary>
		public ushort Flags
		{
			get {return (ushort)flags;}
			set {flags = (EventFlags)value; }
		}

		/// <summary>
		/// Gets or sets if the event refers to local only changes.
		/// </summary>
		public bool LocalOnly
		{
			get
			{
				return ((flags & EventFlags.LocalOnly) > 0);
			}
			set
			{
				if (value)
					flags |= EventFlags.LocalOnly;
				else
					flags &= ~EventFlags.LocalOnly;
			}
		}

		/// <summary>
		/// Gets the master revision for the node.
		/// </summary>
		public ulong MasterRev
		{
			get { return masterRev; }
		}

		/// <summary>
		/// Gets the slave revision for the node.
		/// </summary>
		public ulong SlaveRev
		{
			get { return slaveRev; }
		}

		/// <summary>
		/// Gets the file size for the node. If the node is 
		/// not a file type, zero is returned.
		/// </summary>
		public long FileSize
		{
			get { return fileSize; }
		}


		/// <summary>
		/// Gets the type of event that occurred
		/// </summary>
		public EventType EventType
		{
			get
			{
				return (EventType)Enum.Parse(typeof(EventType), EventData);
			}
		}

		#endregion
	}
}
