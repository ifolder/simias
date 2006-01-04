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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Xml;

using Simias;
using Simias.Client;

namespace Simias.Storage
{
	/// <summary>
	/// Class used to keep track of outstanding searches.
	/// </summary>
	internal class JournalSearchState : IDisposable
	{
		#region Class Members
		/// <summary>
		/// Table used to keep track of outstanding search entries.
		/// </summary>
		static private Hashtable searchTable = new Hashtable();

		/// <summary>
		/// Indicates whether the object has been disposed.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Handle used to store and recall this context object.
		/// </summary>
		private string contextHandle = Guid.NewGuid().ToString();

		/// <summary>
		/// Identifier for the collection that is being searched.
		/// </summary>
		private string collectionID;

		/// <summary>
		/// Object used to iteratively return the members from the domain.
		/// </summary>
		private ICSEnumerator enumerator;

		/// <summary>
		/// Total number of records contained in the search.
		/// </summary>
		private int totalRecords;

		/// <summary>
		/// The cursor for the caller.
		/// </summary>
		private int currentRecord = 0;

		/// <summary>
		/// The last count of records returned.
		/// </summary>
		private int previousCount = 0;
		#endregion

		#region Properties
		/// <summary>
		/// Indicates if the object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return disposed; }
		}

		/// <summary>
		/// Gets the context handle for this object.
		/// </summary>
		public string ContextHandle
		{
			get { return contextHandle; }
		}

		/// <summary>
		/// Gets or sets the current record.
		/// </summary>
		public int CurrentRecord
		{
			get { return currentRecord; }
			set { currentRecord = value; }
		}

		/// <summary>
		/// Gets the ID for the collection that is being searched.
		/// </summary>
		public string CollectionID
		{
			get { return collectionID; }
		}

		/// <summary>
		/// Gets or sets the last record count.
		/// </summary>
		public int LastCount
		{
			get { return previousCount; }
			set { previousCount = value; }
		}

		/// <summary>
		/// Gets the search iterator.
		/// </summary>
		public ICSEnumerator Enumerator
		{
			get { return enumerator; }
		}

		/// <summary>
		/// Gets the total number of records contained by this search.
		/// </summary>
		public int TotalRecords
		{
			get { return totalRecords; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of an object.
		/// </summary>
		/// <param name="collectionID">Identifier for the collection that is being searched.</param>
		/// <param name="enumerator">Search iterator.</param>
		/// <param name="totalRecords">The total number of records contained in the search.</param>
		public JournalSearchState( string collectionID, ICSEnumerator enumerator, int totalRecords )
		{
			this.collectionID = collectionID;
			this.enumerator = enumerator;
			this.totalRecords = totalRecords;

			lock ( searchTable )
			{
				searchTable.Add( contextHandle, this );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Removes this SearchState object from the search table.
		/// </summary>
		private void RemoveSearchState()
		{
			lock ( searchTable )
			{
				// Remove the search context from the table and dispose it.
				searchTable.Remove( contextHandle );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns a search context object that contains the state information for an outstanding search.
		/// </summary>
		/// <param name="contextHandle">Context handle that refers to a specific search context object.</param>
		/// <returns>A SearchState object if a valid one exists, otherwise a null is returned.</returns>
		static public JournalSearchState GetSearchState( string contextHandle )
		{
			lock ( searchTable )
			{
				return searchTable[ contextHandle ] as JournalSearchState;
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Allows for quick release of managed and unmanaged resources.
		/// Called by applications.
		/// </summary>
		public void Dispose()
		{
			RemoveSearchState();
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// Dispose( bool disposing ) executes in two distinct scenarios.
		/// If disposing equals true, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals false, the method has been called by the 
		/// runtime from inside the finalizer and you should not reference 
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
		private void Dispose( bool disposing )
		{
			// Check to see if Dispose has already been called.
			if ( !disposed )
			{
				// Protect callers from accessing the freed members.
				disposed = true;

				// If disposing equals true, dispose all managed and unmanaged resources.
				if ( disposing )
				{
					// Dispose managed resources.
					enumerator.Dispose();
				}
			}
		}
		
		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~JournalSearchState()      
		{
			Dispose( false );
		}
		#endregion
	}

	/// <summary>
	/// Summary description for JournalEntry.
	/// </summary>
	public class JournalEntry
	{
		#region Class Members
		/// <summary>
		/// The type of change that this entry refers to.
		/// </summary>
		private string type;

		/// <summary>
		/// The name of the file that this entry refers to.
		/// </summary>
		private string fileName;

		private string fileID;

		/// <summary>
		/// The name of the user that caused this entry.
		/// </summary>
		private string userName;

		private string userID;

		/// <summary>
		/// The time that the change was made.
		/// </summary>
		private string timeStamp;
		#endregion

		#region Properties
		public string FileID
		{
			get { return fileID; }
		}

		/// <summary>
		/// Gets the filename for this entry.
		/// </summary>
		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}

		/// <summary>
		/// Gets the timestamp for this entry.
		/// </summary>
		public string TimeStamp
		{
			get { return timeStamp; }
		}

		/// <summary>
		/// Gets the type of this entry.
		/// </summary>
		public string Type
		{
			get { return type; }
		}

		public string UserID
		{
			get { return userID; }
		}

		/// <summary>
		/// Gets the username for this entry.
		/// </summary>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}
		#endregion

		#region Constructor
		public JournalEntry( string type, string userID, string timeStamp ) :
			this( type, string.Empty, userID, timeStamp )
		{
		}

		public JournalEntry( string type, string fileName, string userID, string timeStamp )
		{
			this.type = type;
			this.fileName = fileName;
			this.userID = userID;
			this.timeStamp = timeStamp;
		}

		public JournalEntry( XmlNode xmlNode )
		{
			type = xmlNode.Name;
			XmlAttribute attr;
			IEnumerator ienum = xmlNode.Attributes.GetEnumerator();
			while ( ienum.MoveNext() )
			{
				attr = (XmlAttribute)ienum.Current;
				switch (attr.Name)
				{
					case "fnID":
						fileID = attr.Value;
						break;
					case "userID":
						userID = attr.Value;
						break;
					case "path":
						fileName = attr.Value;
						break;
					case "ts":
						timeStamp = new DateTime( long.Parse( attr.Value ) ).ToString();
						break;
				}
			}
		}
		#endregion
	}
}
