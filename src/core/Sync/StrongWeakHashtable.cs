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
using System.Security.Cryptography;

namespace Simias.Sync.Delta
{
	/// <summary>
	/// Hashtable class that contains a strong and a weak hash code for the value.
	/// Elements are stored in an ArrayList using the weak hash as the key.
	/// </summary>
	public class StrongWeakHashtable
	{
		Hashtable table = new Hashtable();
        //static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncClient));
		/// <summary>
		/// Add a new HashEntry to the table.
		/// </summary>
		/// <param name="entry">The entry to add.</param>
		private void Add(HashEntry entry)
		{
			lock (this)
			{
				ArrayList entryArray;
				entryArray = (ArrayList)table[entry.WeakHash];
				if (entryArray == null)
				{
                    //log.Info("This is a new entryarray for hash: {0}", entry.WeakHash);
					entryArray = new ArrayList();
					table.Add(entry.WeakHash, entryArray);
				}
                bool contains = false;
                for (int i = 0; i < entryArray.Count; i++)
                {
                    HashEntry entry1 = (HashEntry) entryArray[i];
                    if (entry1.BlockNumber == entry.BlockNumber && entry1.StrongHash == entry.StrongHash && entry1.WeakHash == entry.WeakHash)
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                {
                    //log.Info("StrongWeakHashtable: weakhash: {0}, block number: {1}. entry array does not contain.", entry.WeakHash, entry.BlockNumber);
                    entryArray.Add(entry);
                }
                else
                {
                    //log.Info("StrongWeakHashtable: weakhash: {0}, block number: {1}. entry array already contains this.", entry.WeakHash, entry.BlockNumber);
                }
			}
		}

		/// <summary>
		/// Add the List of entries to the table.
		/// </summary>
		/// <param name="entryList">The list of entries to add.</param>
		public void Add(HashData[] entryList)
		{
			foreach (HashData entry in entryList)
			{
				Add(new HashEntry(entry));
			}
		}

		/// <summary>
		/// Checks if the table contains the weak hash.
		/// </summary>
		/// <param name="weakHash">The hash to compare.</param>
		/// <returns>True if in the table.</returns>
		public bool Contains(UInt32 weakHash)
		{
			return table.Contains(weakHash);
		}

		/// <summary>
		/// Indexer for the table.  The index is the weakHash.
		/// Returns the ArrayList of entries that all have this weak hash.
		/// </summary>
		public ArrayList this[UInt32 weakHash]
		{
			get
			{
				return (ArrayList)table[weakHash];
			}
		}

		/// <summary>
		/// Returns the Entry that matches the weak and strong hash codes
		/// of the passed in HashEntry.
		/// </summary>
		/// <param name="entry">The entry to match.</param>
		/// <returns>The HashEntry that matched, or null.</returns>
		public HashEntry GetEntry(HashEntry entry)
		{
			ArrayList entryList = (ArrayList)table[entry.WeakHash];
			if (entryList != null)
			{
				int eIndex = entryList.IndexOf(entry);
				if (eIndex != -1)
				{
					HashEntry match = (HashEntry)entryList[eIndex];
					return match;
				}
			}
			return null;
		}

        /// <summary>
        /// Returns the Entry that matches the weak and strong hash codes
        /// of the passed in HashEntry.
        /// </summary>
        /// <param name="entry">The entry to match.</param>
	/// <param name="blocknum"></param>	
        /// <returns>The HashEntry that matched, or null.</returns>
        public HashEntry GetEntryAfterBlock(HashEntry entry, int blocknum)
        {
            ArrayList entryList = (ArrayList)table[entry.WeakHash];
            if (entryList != null)
            {
                for (int i = 0; i < entryList.Count; i++)
                {
                    HashEntry entry1 = (HashEntry)entryList[i];
                    if (entry1.BlockNumber >= blocknum)
                        return entry1;
                }
                /*
                int eIndex = entryList.IndexOf(entry, blocknum);
                if (eIndex != -1)
                {
                    HashEntry match = (HashEntry)entryList[eIndex];
                    return match;
                }
                 * */
            }
            return null;
        }

		/// <summary>
		/// Clears all elements from the table.
		/// </summary>
		public void Clear()
		{
			table.Clear();
		}
	}

	/// <summary>
	/// Class used to keep track of the file Blocks and hash
	/// codes assosiated with the block.
	/// </summary>
	public class HashEntry
	{
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
		/// Default constructor.
		/// </summary>
		public HashEntry()
		{
		}

		/// <summary>
		/// Constructs a HashEntry from a HashData object.
		/// </summary>
		/// <param name="entry">The HashData object.</param>
		public HashEntry(HashData entry)
		{
			BlockNumber = entry.BlockNumber;
			WeakHash = entry.WeakHash;
			StrongHash = entry.StrongHash;
		}

		/// <summary>
		/// Override to test for equality of a HashEntry.
		/// </summary>
		/// <param name="obj">The object to compare against.</param>
		/// <returns>True if equal.</returns>
		public override bool Equals(object obj)
		{
			if (this.WeakHash == ((HashEntry)obj).WeakHash)
			{
				for (int i = 0; i < StrongHash.Length; ++i)
				{
					if (StrongHash[i] != ((HashEntry)obj).StrongHash[i])
						return false;
				}
				return true;
			}
			return false;
		}

		///<summary>
		/// Overide needed because Equals is overriden. 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}
	}
}
