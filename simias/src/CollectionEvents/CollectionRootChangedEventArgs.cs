/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young
 *
 ***********************************************************************/
using System;
using System.Text;

namespace Simias.Event
{
	/// <summary>
	/// The event arguments for a Collection event.
	/// </summary>
	[Serializable]
	public class CollectionRootChangedEventArgs : NodeEventArgs
	{
		string oldRoot;
		string newRoot;

		/// <summary>
		/// Constructs a CollectionRootChangedEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="collection">The Collection that the node belongs to.</param>
		/// <param name="type">The Type of the Node.</param>
		/// <param name="oldRoot">The old path to the root path.</param>
		/// <param name="newRoot">The new path to the root path.</param>
		public CollectionRootChangedEventArgs(string source, string collection, string type, string oldRoot, string newRoot) :
			this(source, collection, type, oldRoot, newRoot, 0)

		{
		}

		/// <summary>
		/// Constructs a CollectionRootChangedEventArgs that will be used by CollectionHandler delegates.
		/// Descibes the node affected by the event.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="collection">The Collection that the node belongs to.</param>
		/// <param name="type">The Type of the Node.</param>
		/// <param name="oldRoot">The old path to the root path.</param>
		/// <param name="newRoot">The new path to the root path.</param>
		/// <param name="eventId">A user defined event ID. Only has meaning to a publisher.</param>
		public CollectionRootChangedEventArgs(string source, string collection, string type, string oldRoot, string newRoot, int eventId) :
			base(source, collection, collection, type, EventType.CollectionRootChanged, eventId)

		{
			this.oldRoot = oldRoot;
			this.newRoot = newRoot;
		}

		internal CollectionRootChangedEventArgs(string args)
		{
			int index = 0;
			string [] aArgs = args.Split(seperatorChar);
			MarshallFromString(aArgs, ref index);
		}

		internal override string MarshallToString()
		{
			StringBuilder sb = new StringBuilder(base.MarshallToString());
			sb.Append(oldRoot + seperatorChar);
			sb.Append(newRoot + seperatorChar);
			return sb.ToString();
		}

		internal override void MarshallFromString(string [] args, ref int index)
		{
			//int i = 0;
			//string [] sArg = sArgs.Split(seperatorChar);
			base.MarshallFromString(args, ref index);
			oldRoot = args[index++];
			newRoot = args[index++];
		}

		/// <summary>
		/// Gets the Old full name.
		/// </summary>
		public string OldRoot
		{
			get {return oldRoot;}
		}

		/// <summary>
		/// Gets the new full name.
		/// </summary>
		public string NewRoot
		{
			get {return newRoot;}
		}
	}
}

