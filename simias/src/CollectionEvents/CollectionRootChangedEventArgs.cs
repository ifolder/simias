/***********************************************************************
 *  CollectionRootChangedEventArgs.cs - Argument class for collection
 *  rootPath change events.
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Russ Young <ryoung@novell.com>
 * 
 ***********************************************************************/

using System;

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
			base(source, collection, collection, type, CollectionEventArgs.EventType.RootChanged)

		{
			this.oldRoot = oldRoot;
			this.newRoot = newRoot;
		}

		public string OldRoot
		{
			get {return oldRoot;}
		}
		public string NewRoot
		{
			get {return newRoot;}
		}
	}
}

