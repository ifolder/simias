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
*                 $Author: Bruce Getter
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

namespace Simias.Client.Event
{
	/// <summary>
	/// The action supported.
	/// </summary>
	[Flags]
	public enum Action : short
	{
		/// <summary>
		/// The event is for a sync start.
		/// </summary>
		StartSync = 1,
		/// <summary>
		/// The event is for a sync stop.
		/// </summary>
		StopSync = 2,
		/// <summary>
		/// We are starting the local sync.
		/// </summary>
		StartLocalSync = 3,
        	/// <summary>
        	///  Passphrase not provided.
        	/// </summary>
        	NoPassphrase = 4,
        	/// <summary>
        	///  Sync Disabled not provided.
        	/// </summary>
        	DisabledSync= 5,
	};

	/// <summary>
	/// The arguments for a collection sync event.
	/// </summary>
	[Serializable]
	public class CollectionSyncEventArgs : SimiasEventArgs
	{
		#region Fields

		private string name;
		private string id;
		private bool connected;
		private bool yielded;
		private Action action;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a CollectionSyncEventArgs that will be used by CollectionSyncHandler delegates.
		/// </summary>
		/// <param name="name">The name of the collection that the event belongs to.</param>
		/// <param name="id">The id of the collection that the event belongs to.</param>
		/// <param name="action">The sync action for the event.</param>
		/// <param name="connected">A value indicating if the server is available.</param>
		/// <param name="yielded">A value indicating if the sync yielded to let other collections sync.</param>
		public CollectionSyncEventArgs(string name, string id, Action action, bool connected, bool yielded) :
			base()
		{
			this.name = name;
			this.id = id;
			this.action = action;
			this.connected = connected;
			this.yielded = yielded;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the collection that the event belongs to.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Gets the id of the collection that the event belongs to.
		/// </summary>
		public string ID
		{
			get { return id; }
		}

		/// <summary>
		/// Gets the sync action for the event.
		/// </summary>
		public Action Action
		{
			get { return action; }
		}

		/// <summary>
		/// Gets a value indicating if the server that host the collection is available.
		/// </summary>
		public bool Connected
		{
			get { return connected; }
		}

		/// <summary>
		/// Gets a value indicating if the sync process yielded to allow another collection to synchronize.
		/// </summary>
		public bool Yielded
		{
			get { return yielded; }
		}
		#endregion
	}
}
