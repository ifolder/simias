/***********************************************************************
 *  $RCSfile: iFolder.cs,v $
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Rob
 *
 ***********************************************************************/

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
	public enum ChangeType
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
		/// Change Type
		/// </summary>
		public ChangeType Type;

		/// <summary>
		/// Change Entry ID
		/// </summary>
		public string EntryID;

		/// <summary>
		/// Change Entry Name
		/// </summary>
		public string EntryName;

		/// <summary>
		/// Changed by User ID
		/// </summary>
		public string UserID;

		/// <summary>
		/// Changed by User Full Name
		/// </summary>
		public string UserFullName;

		/// <summary>
		/// Is the Entry a Directory?
		/// </summary>
		public bool IsDirectory = false;

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
			this.EntryID = entry.FileID;
			this.EntryName = entry.FileName;
			this.UserID = entry.UserID;
			this.UserFullName = entry.UserName;
			this.IsDirectory = entry.IsFolder;

			// parse the journal entry type
			switch(entry.Type)
			{
				case "modify":
					this.Type = ChangeType.Modify;
					break;

				case "add":
					this.Type = ChangeType.Add;
					break;

				case "delete":
					this.Type = ChangeType.Delete;
					break;

				default:
					this.Type = ChangeType.Unknown;
					break;
			}
		}

		/// <summary>
		/// Get Changes
		/// </summary>
		/// <param name="ifolderID">The iFolder ID</param>
		/// <param name="entryID">The iFolder Entry ID</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>A Set of ChangeEntry Objects</returns>
		public static ChangeEntrySet GetChanges(string ifolderID, string entryID, int index, int max, string accessID)
		{
			JournalEntry[] entries;

			// TODO: access check?

			// get entries
			int total = 0;
			Journal journal = new Journal(ifolderID);
			journal.GetSeekEntries(entryID, null, max, (uint)index, out entries, out total);

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
