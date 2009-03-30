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
*                 $Author: Rob
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
using System;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

using Simias.Storage;
using Simias.Web;

namespace iFolder.WebService
{
	/// <summary>
	/// Change Type
	/// </summary>
	[Serializable]
	public enum ChangeEntryType
	{
		/// <summary>
		/// iFolder
		/// </summary>
		iFolder,

		/// <summary>
		/// File
		/// </summary>
		File,

		/// <summary>
		/// Directory
		/// </summary>
		Directory,

		/// <summary>
		/// Member
		/// </summary>
		Member,

		/// <summary>
		/// Unknown
		/// </summary>
		Unknown
	}

	/// <summary>
	/// Change Action
	/// </summary>
	[Serializable]
	public enum ChangeEntryAction
	{
		/// <summary>
		/// Add
		/// </summary>
		Add,

		/// <summary>
		/// Modified
		/// </summary>
		Modify,

		/// <summary>
		/// Deleted
		/// </summary>
		Delete,

		/// <summary>
		/// Unknown
		/// </summary>
		Unknown
	}

	/// <summary>
	/// A Change Entry Result Set
	/// </summary>
	[Serializable]
	public class ChangeEntrySet
	{
		/// <summary>
		/// An Array of Change Entries
		/// </summary>
		public ChangeEntry[] Items;

		/// <summary>
		/// The Total Number of Change Entries
		/// </summary>
		public int Total;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public ChangeEntrySet()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="items"></param>
		/// <param name="total"></param>
		public ChangeEntrySet(ChangeEntry[] items, int total)
		{
			this.Items = items;
			this.Total = total;
		}
	}

	/// <summary>
	/// Change Entry
	/// </summary>
	[Serializable]
	public class ChangeEntry
	{
		/// <summary>
		/// Change Time
		/// </summary>
		public DateTime Time;
		
		/// <summary>
		/// Change Object Type
		/// </summary>
		public ChangeEntryType Type;

		/// <summary>
		/// Change Action
		/// </summary>
		public ChangeEntryAction Action;

		/// <summary>
		/// Change Object ID
		/// </summary>
		public string ID;

		/// <summary>
		/// Change Object Name
		/// </summary>
		public string Name;

		/// <summary>
		/// Changed by User ID
		/// </summary>
		public string UserID;

		/// <summary>
		/// Changed by User Full Name
		/// </summary>
		public string UserFullName;

		/// <summary>
		/// Member's New Rights
		/// </summary>
		public Rights MemberNewRights;

		/// <summary>
		/// Member's Old Rights
		/// </summary>
		public Rights MemberOldRights;

		/// <summary>
		/// Constructor
		/// </summary>
		public ChangeEntry()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ChangeEntry(JournalEntry entry)
		{
			this.Time = entry.TimeStamp;
			this.ID = entry.FileID;
			this.Name = entry.FileName;
			this.UserID = entry.UserID;
			this.UserFullName = entry.UserName;

			// parse the journal entry type to object type
			switch(entry.EntryType)
			{
				case EntryTypes.File:
					this.Type = ChangeEntryType.File;
					break;

				case EntryTypes.Folder:
					// check for iFolder
					if (this.Name.IndexOf('/') == -1)
					{
						this.Type = ChangeEntryType.iFolder;
					}
					else
					{
						this.Type = ChangeEntryType.Directory;
					}
					break;

				case EntryTypes.Member:
					this.Type = ChangeEntryType.Member;
					break;

				case EntryTypes.Unknown:
				default:
					this.Type = ChangeEntryType.Unknown;
					break;
			}

			// parse the journal change type to change action
			switch(entry.ChangeType)
			{
				case ChangeTypes.Modify:
					this.Action = ChangeEntryAction.Modify;
					break;

				case ChangeTypes.Add:
					this.Action = ChangeEntryAction.Add;
					break;

				case ChangeTypes.Delete:
					this.Action = ChangeEntryAction.Delete;
					break;

				case ChangeTypes.Unknown:
				default:
					this.Action = ChangeEntryAction.Unknown;
					break;
			}

			// parse the rights changes
			switch(entry.MemberRights)
			{
				case MemberRights.Admin:
					MemberNewRights = Rights.Admin;
					MemberOldRights = Rights.Unknown;
					break;

				case MemberRights.ReadWriteToAdmin:
					MemberNewRights = Rights.Admin;
					MemberOldRights = Rights.ReadWrite;
					break;

				case MemberRights.ReadOnlyToAdmin:
					MemberNewRights = Rights.Admin;
					MemberOldRights = Rights.ReadOnly;
					break;

				case MemberRights.ReadWrite:
					MemberNewRights = Rights.ReadWrite;
					MemberOldRights = Rights.Unknown;
					break;

				case MemberRights.AdminToReadWrite:
					MemberNewRights = Rights.ReadWrite;
					MemberOldRights = Rights.Admin;
					break;

				case MemberRights.ReadOnlyToReadWrite:
					MemberNewRights = Rights.ReadWrite;
					MemberOldRights = Rights.ReadOnly;
					break;

				case MemberRights.ReadOnly:
					MemberNewRights = Rights.ReadOnly;
					MemberOldRights = Rights.Unknown;
					break;

				case MemberRights.AdminToReadOnly:
					MemberNewRights = Rights.ReadOnly;
					MemberOldRights = Rights.Admin;
					break;

				case MemberRights.ReadWriteToReadOnly:
					MemberNewRights = Rights.ReadOnly;
					MemberOldRights = Rights.ReadWrite;
					break;

				default:
					MemberNewRights = Rights.Unknown;
					MemberOldRights = Rights.Unknown;
					break;
			}
		}

		/// <summary>
		/// Get Changes
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="itemID">The Item ID</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>A Set of ChangeEntry Objects</returns>
		public static ChangeEntrySet GetChanges(string ifolderID, string itemID, int index, int max, string accessID)
		{
			JournalEntry[] entries;

			// TODO: access check?

			// check for a member and convert the User ID to a Node ID
			if ((itemID != null) && (itemID.Length != 0))
			{
				Store store = Store.GetStore();
				Collection collection = store.GetCollectionByID(ifolderID);

				if (collection == null) throw new iFolderDoesNotExistException(ifolderID);

				Member member = collection.GetMemberByID(itemID);

				if (member != null) itemID = member.ID;
			}

			// get entries
			int total = 0;
			Journal journal = new Journal(ifolderID);
			journal.GetSeekEntries(itemID, null, max, (uint)index, out entries, out total);

			// list
			ArrayList list = new ArrayList();

			foreach(JournalEntry entry in entries)
			{
				list.Add(new ChangeEntry(entry));
			}
			
			return new ChangeEntrySet((ChangeEntry[])list.ToArray(typeof(ChangeEntry)), total);
		}
	}
}
