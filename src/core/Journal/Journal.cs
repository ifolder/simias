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
using System.IO;
using System.Text;
using System.Threading;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Service;
using Simias.Sync;


namespace Simias.Storage
{
	/// <summary>
	/// 
	/// </summary>
	public class Journal
	{
		#region Class Members

		Store store;
		Collection collection;
		StoreFileNode journalNode;
		string tempFile = string.Empty;
		bool collectionModified = false;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a Journal object.
		/// </summary>
		/// <param name="collectionID">The identifier of the collection that this Journal object belongs to.</param>
		public Journal (string collectionID)
		{
			store = Store.GetStore();
			collection = store.GetCollectionByID( collectionID );
			if ( collection == null )
			{
				throw new SimiasException( string.Format( "Unable to instantiate collection for ID '{0}'", collectionID ) );
			}

			// Check if this node already has a journal.
			Property property = collection.Properties.GetSingleProperty( PropertyTags.Journal );
			if ( property != null )
			{
				Relationship relationship = ( Relationship ) property.Value;
				journalNode = collection.GetNodeByID( relationship.NodeID ) as StoreFileNode;
			}
			else
			{
				Relationship relationship = new Relationship( collection.ID, collection.ID );
				ICSList entries = collection.Search( PropertyTags.JournalFor, relationship );
				foreach ( ShallowNode sn in entries )
				{
					if ( sn.Type == NodeTypes.StoreFileNodeType )
					{
						journalNode = new StoreFileNode( collection, sn );
						break;
					}
				}
			}

			if ( ( journalNode == null ) &&
				collection.Role.Equals(SyncRoles.Master) )
			{
				// Build a name for a temporary file.
				tempFile = Path.Combine( collection.ManagedPath, collection.ID );
			}
		}

		#endregion

		#region Properties
		#endregion

		#region Private Methods

		#endregion

		#region Public Methods

		/// <summary>
		/// Commit the changes made to the journal.
		/// </summary>
		public void Commit()
		{
			if ( collection.Role.Equals(SyncRoles.Master) )
			{
				if ( collectionModified )
				{
					collection.Commit( new Node[] { collection, journalNode } );
					collectionModified = false;
				}
				else
				{
					collection.Commit( journalNode );
				}
			}
		}

		/// <summary>
		/// End the search for journal entries.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstJournalEntries or
		/// FindNextEntries methods.</param>
		public void FindCloseEntries( string searchContext )
		{
			// See if there is a valid search context.
			JournalSearchState searchState = JournalSearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				searchState.Dispose();
			}
		}

		/// <summary>
		/// Starts a search for journal entries.
		/// </summary>
		/// <param name="fileID">The fileID to filter on.  Only journal entries with this fileID will be returned.  
		/// Pass in <b>null</b> to retrieve journal entries regardless of fileID.</param>
		/// <param name="userID">The UserID to filter on.  Only journal entries with this userID will be returned.  
		/// Pass in <b>null</b> to retrieve journal entries regardless of userID.</param>
		/// <param name="count">Maximum number of JournalEntry objects to return.  Pass in zero to return all entries.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="journalList">Receives an array object that contains the JournalEntry objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more journal entries. Otherwise false is returned.</returns>
		public bool FindFirstEntries( string fileID, string userID, int count, out string searchContext, out JournalEntry[] journalList, out int total )
		{
			return FindFirstEntries( fileID, userID, DateTime.MinValue, DateTime.MaxValue, count, out searchContext, out journalList, out total );
		}

		/// <summary>
		/// Starts a search for journal entries.
		/// </summary>
		/// <param name="fileID">The fileID to filter on.  Only journal entries with this fileID will be returned.  
		/// Pass in <b>null</b> to retrieve journal entries regardless of fileID.</param>
		/// <param name="userID">The UserID to filter on.  Only journal entries with this userID will be returned.  
		/// Pass in <b>null</b> to retrieve journal entries regardless of userID.</param>
		/// <param name="fromTime">The minimum time to filter on.  Any journal entries that have occurred since this
		/// time will be returned.</param>
		/// <param name="toTime">The maximum time to filter on.  Any journal entries that occurred before this time
		/// will be returned.</param>
		/// <param name="count">Maximum number of JournalEntry objects to return.  Pass in zero to return all entries.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="journalList">Receives an array object that contains the JournalEntry objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more journal entries. Otherwise false is returned.</returns>
		public bool FindFirstEntries( string fileID, string userID, DateTime fromTime, DateTime toTime, int count, out string searchContext, out JournalEntry[] journalList, out int total )
		{
			bool moreEntries = false;

			searchContext = null;
			journalList = null;
			total = 0;

			string journalFile = journalNode.GetFullPath( collection );
			JournalSearchState searchState = new JournalSearchState( collection, journalFile, fileID, userID, fromTime, toTime );
			searchContext = searchState.ContextHandle;
			moreEntries = FindNextEntries( ref searchContext, count, out journalList );
			total = searchState.TotalRecords;

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for journal entries from the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstJournalEntries method.</param>
		/// <param name="count">Maximum number of JournalEntry objects to return.  Pass in zero to return all entries.</param>
		/// <param name="journalList">Receives an array object that contains the JournalEntry objects.</param>
		/// <returns>True if there are more journal entries. Otherwise false is returned.</returns>
		public bool FindNextEntries( ref string searchContext, int count, out JournalEntry[] journalList )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			journalList = null;

			// See if there is a valid search context.
			JournalSearchState searchState = JournalSearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				bool returnAll = false;
				if ( count == 0 )
				{
					count = searchState.TotalRecords;
					returnAll = true;
				}

				// See if entries are to be returned.
				if ( count > 0 )
				{
					// Get the domain for this collection.
					Domain domain = store.GetDomain( collection.Domain );
					if ( domain != null )
					{
						// Allocate a list to hold the member objects.
						ArrayList tempList = new ArrayList( count );
						JournalEntry je;
						while( ( count > 0 ) && ((je = searchState.GetNextEntry()) != null ))
						{
							Member member = domain.GetMemberByID( je.UserID );
							if ( member != null )
							{
								je.UserName = member.FN != null ? member.FN : member.Name;
							}

							Node node = collection.GetNodeByID( je.FileID );
							je.IsFolder = node != null && node.IsType( NodeTypes.DirNodeType );

							tempList.Add( je );
							--count;
						}

						if ( tempList.Count > 0 )
						{
							journalList = tempList.ToArray( typeof ( JournalEntry ) ) as JournalEntry[];
							searchState.LastCount = journalList.Length;
							moreEntries = ( count == 0 ) ? !returnAll : false;
						}
					}
				}
				else
				{
					if ( searchState.CurrentRecord < searchState.TotalRecords )
					{
						moreEntries = true;
					}
				}
			}

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for journal entries previous to the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstJournalEntries method.</param>
		/// <param name="count">Maximum number of JournalEntry objects to return.  Pass in zero to return all entries.</param>
		/// <param name="journalList">Receives an array object that contains the JournalEntry objects.</param>
		/// <returns>True if there are more journal entries. Otherwise false is returned.</returns>
		public bool FindPreviousEntries( ref string searchContext, int count, out JournalEntry[] journalList )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			journalList = null;

			// See if there is a valid search context.
			JournalSearchState searchState = JournalSearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				// Backup the current cursor, but don't go past the first record.
				if ( searchState.CurrentRecord > 0 )
				{
					bool invalidIndex = false;
					int numberOfEntries = searchState.LastCount + count;
					int newIndex = searchState.CurrentRecord - numberOfEntries;
					if ( newIndex < 0 )
					{
						invalidIndex = true;
						count = searchState.CurrentRecord - searchState.LastCount;
						numberOfEntries = -1;
					}

					// Set the new index for the cursor.
					if ( searchState.MovePrevious( numberOfEntries ) )
					{
						// Complete the search.
						FindNextEntries( ref searchContext, count, out journalList );

						if ( ( invalidIndex == false ) && ( journalList != null ) )
						{
							moreEntries = true;
						}
					}
				}
			}

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for journal entries from the specified record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstJournalEntries method.</param>
		/// <param name="offset">Record offset to return journal entries from.</param>
		/// <param name="count">Maximum number of JournalEntry objects to return.  Pass in zero to return all entries.</param>
		/// <param name="journalList">Receives an array object that contains the JournalEntry objects.</param>
		/// <returns>True if there are more journal entries. Otherwise false is returned.</returns>
		public bool FindSeekEntries( ref string searchContext, uint offset, int count, out JournalEntry[] journalList )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			journalList = null;

			// See if there is a valid search context.
			JournalSearchState searchState = JournalSearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				// Make sure that the specified offset is valid.
				if ( ( offset >= 0 ) && ( offset <= searchState.TotalRecords ) )
				{
					// Set the cursor to the specified offset.
					if ( searchState.Seek( offset ) )
					{
						// Complete the search.
						moreEntries = FindNextEntries( ref searchContext, count, out journalList );
					}
				}
			}

			return moreEntries;
		}

		/// <summary>
		/// Retrieves all journal entries from the specified offset.
		/// </summary>
		/// <param name="fileID">The fileID to filter on.  Only journal entries with this fileID will be returned.  
		/// Pass in <b>null</b> to retrieve journal entries regardless of fileID.</param>
		/// <param name="userID">The UserID to filter on.  Only journal entries with this userID will be returned.  
		/// Pass in <b>null</b> to retrieve journal entries regardless of userID.</param>
		/// <param name="fromTime">The minimum time to filter on.  Any journal entries that have occurred since this
		/// time will be returned.</param>
		/// <param name="toTime">The maximum time to filter on.  Any journal entries that occurred before this time
		/// will be returned.</param>
		/// <param name="count">Maximum number of JournalEntry objects to return.  Pass in zero to return all entries.</param>
		/// <param name="offset">Record offset to return journal entries from.</param>
		/// <param name="journalList">Receives an array object that contains the JournalEntry objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns><b>True</b> if there are more journal entries; otherwise, <b>false</b> is returned.</returns>
		public bool GetSeekEntries( string fileID, string userID, DateTime fromTime, DateTime toTime, int count, uint offset, out JournalEntry[] journalList, out int total )
		{
			bool result;
			string searchContext;
			FindFirstEntries( fileID, userID, fromTime, toTime, -1, out searchContext, out journalList, out total );
			result = FindSeekEntries( ref searchContext, offset, count, out journalList );
			FindCloseEntries( searchContext );

			return result;
		}

		/// <summary>
		/// Retrieves all journal entries from the specified offset.
		/// </summary>
		/// <param name="fileID">The fileID to filter on.  Only journal entries with this fileID will be returned.  
		/// Pass in <b>null</b> to retrieve journal entries regardless of fileID.</param>
		/// <param name="userID">The UserID to filter on.  Only journal entries with this userID will be returned.  
		/// Pass in <b>null</b> to retrieve journal entries regardless of userID.</param>
		/// <param name="count">Maximum number of JournalEntry objects to return.  Pass in zero to return all entries.</param>
		/// <param name="offset">Record offset to return journal entries from.</param>
		/// <param name="journalList">Receives an array object that contains the JournalEntry objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns><b>True</b> if there are more journal entries; otherwise, <b>false</b> is returned.</returns>
		public bool GetSeekEntries( string fileID, string userID, int count, uint offset, out JournalEntry[] journalList, out int total )
		{
			return GetSeekEntries( fileID, userID, DateTime.MinValue, DateTime.MaxValue, count, offset, out journalList, out total );
		}

		/// <summary>
		/// Update the journal.
		/// </summary>
		/// <param name="args">Contains the information to put in the journal.</param>
		public void UpdateJournal( NodeEventArgs args )
		{
			if ( collection.Role.Equals(SyncRoles.Master) )
			{
				// Update the history in the journal.
				string filename;
				bool newJournal = false;

				if ( !tempFile.Equals(string.Empty) )
				{
					newJournal = true;
					filename = tempFile;
				}
				else
				{
					filename = journalNode.GetFullPath( collection );
				}

				// Build the record.
				// eventType:modifierID:nodeID:timeStamp:relativePath
				string record = Enum.Format( typeof( EventType ), args.EventType, "d" ) + ":" + args.Modifier + ":" + args.Node + ":" + args.TimeStamp.Ticks.ToString();

				if ( args.EventType != EventType.NodeDeleted )
				{
					// If the node hasn't been deleted, get the relative path (if applicable).
					if ( args.Type == NodeTypes.DirNodeType )
					{
						DirNode dn = new DirNode( collection.GetNodeByID( args.Node ) );
						record += ":" + dn.GetRelativePath();
					}
					else if ( args.Type == NodeTypes.FileNodeType )
					{
						FileNode fn = new FileNode( collection.GetNodeByID( args.Node ) );
						record += ":" + fn.GetRelativePath();
					}
				}

				// TODO: Can we collapse entries?

				using ( StreamWriter sw = File.AppendText( filename ) )
				{
					sw.WriteLine( record );
				}


				if ( newJournal )
				{
					// Create the journal node.
					FileStream stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite);
					journalNode = new StoreFileNode( collection.Name, stream );

					if ( journalNode != null )
					{
						journalNode.FlushStreamData( collection );

						// Add the journal type.
						journalNode.Properties.AddNodeProperty( PropertyTags.Types, "Journal" );

						// Create a relationship to the collection and add it to the journal.
						journalNode.Properties.AddNodeProperty( PropertyTags.JournalFor, new Relationship( collection.ID, collection.ID ) );
					}

					// Delete the temporary file.
					File.Delete( filename );
				}
				else
				{
					// Update the file length in the existing journal node.
					FileInfo fi = new FileInfo(filename);
					journalNode.Length = fi.Length;
				}

				// After the initial sync of the collection, put a relationship to the journal on the collection 
				// if it doesn't already exist.
				if ( (ulong)collection.Properties.GetSingleProperty( PropertyTags.LocalIncarnation ).Value != 0 &&
					collection.Properties.GetSingleProperty( PropertyTags.Journal ) == null )
				{
					collection.Properties.AddNodeProperty( PropertyTags.Journal, new Relationship( collection.ID, journalNode.ID ) );
					collectionModified = true;
				}

				// Update the journal modified time on the collection.
				Property property = new Property( PropertyTags.JournalModified, args.TimeStamp );
				property.LocalProperty = true;
				collection.Properties.ModifyNodeProperty( property );
				collection.Commit( collection );
			}
		}

		#endregion

	}

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
		/// Encoding used when reading the journal file.
		/// </summary>
		private UTF8Encoding encoding = new UTF8Encoding();

		/// <summary>
		/// Stream used to read the journal file.
		/// </summary>
		private FileStream stream;

		/// <summary>
		/// String array used to hold records read from the journal file.
		/// </summary>
		private string[] records = null;

		/// <summary>
		/// Index used to read a record from the records array.
		/// </summary>
		private int index = -1;

		/// <summary>
		/// Offset used to track carriage return line feeds so that complete records
		/// are read from the journal file.
		/// </summary>
		private int offset = 0;

		/// <summary>
		/// Used to read specific entries from the journal file based on file ID.
		/// </summary>
		private string fileID = null;

		/// <summary>
		/// Used to read specific entries from the journal file based on user ID.
		/// </summary>
		private string userID;

		/// <summary>
		/// Used to read specific entries from the journal file based on timestamp.
		/// </summary>
		private DateTime fromTime;
		private DateTime toTime;

		/// <summary>
		/// Total number of records contained in the search.
		/// </summary>
		private int totalRecords = 0;

		/// <summary>
		/// The index of the current record.
		/// </summary>
		private int currentRecord = 0;

		/// <summary>
		/// The last count of records returned.
		/// </summary>
		private int previousCount = 0;

		/// <summary>
		/// The path of the journal file.
		/// </summary>
		private string journalFile;

		/// <summary>
		/// The collection that the journal belongs to.
		/// </summary>
		private Collection collection;

		/// <summary>
		/// The most recent entry for a given file that has been modified locally since the last synchronization cycle.
		/// </summary>
		private JournalEntry firstEntry = null;

		/// <summary>
		/// Used to keep track when the end of the file has been reached.
		/// </summary>
		private bool eof = false;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes an instance of a JournalSearchState object.
		/// </summary>
		/// <param name="collection">The collection to return journal entries for.</param>
		/// <param name="journalFile">The path of the journal file to search.</param>
		/// <param name="fileID">The file ID to search for in the journal file.  Pass in a null or an empty
		/// string to not search for a specific file.</param>
		/// <param name="userID">The user ID to search for in the journal file.  Pass in a null or an empty
		/// string to not search for a specific user.</param>
		/// <param name="fromTime">Timestamp used to filter journal file entries.  Return all entries with a
		/// timestamp greater than fromTime.  Passing in DateTime.MinValue will cause this parameter to not
		/// affect which entries are returned.</param>
		/// <param name="toTime">Timestamp used to filter journal file entries.  Return all entries with a
		/// timestamp less than toTime.  Passing in DateTime.MaxValue will cause this parameter to not
		/// affect which entries are returned.</param>
		public JournalSearchState( Collection collection, string journalFile, string fileID, string userID, DateTime fromTime, DateTime toTime )
		{
			this.collection = collection;
			this.fileID = fileID;
			this.userID = userID;
			this.fromTime = fromTime;
			this.toTime = toTime;
			this.journalFile = journalFile;
			this.stream = new FileStream( journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );

			lock ( searchTable )
			{
				searchTable.Add( contextHandle, this );
			}

			// Get the number of entries to return.
			totalRecords = GetJournalEntries();
		}

		/// <summary>
		/// Initializes an instance of a JournalSearchState object.
		/// </summary>
		/// <param name="collection">The collection to return journal entries for.</param>
		/// <param name="journalFile">The path of the journal file to search.</param>
		/// <param name="fileID">The file ID to search for in the journal file.  Pass in a null or an empty
		/// string to not search for a specific file.</param>
		/// <param name="userID">The user ID to search for in the journal file.  Pass in a null or an empty
		/// string to not search for a specific user.</param>
		/// <param name="totalRecords">The total number of records contained in the search.</param>
		public JournalSearchState( Collection collection, string journalFile, string fileID, string userID ) :
			this( collection, journalFile, fileID, userID, DateTime.MinValue, DateTime.MaxValue )
		{
		}

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
		/// Gets or sets the last record count.
		/// </summary>
		public int LastCount
		{
			get { return previousCount; }
			set { previousCount = value; }
		}

		/// <summary>
		/// Gets the total number of records contained by this search.
		/// </summary>
		public int TotalRecords
		{
			get { return totalRecords; }
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the number of journal entries in a collection.
		/// </summary>
		/// <returns>The number of journal entries in the collection.</returns>
		private int GetJournalEntries()
		{
			int count = 0;

			using ( StreamReader reader = new StreamReader( new FileStream( journalFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ) )
			{
				JournalEntry je = null;
				string record;
				while ( ( record = reader.ReadLine() ) != null )
				{
					try
					{
						je = new JournalEntry( record );

						if ( ReturnEntry( je ) )
						{
							count++;
						}
					}
					catch ( SimiasException )
					{}
				}

				// TODO: Is there a way to update the journal for newly added files? (The journal on the
				// the client is always a sync cycle behind the journal on the server).
				// Add the last modifier to the journal if it isn't already there.
				if ( fileID != null && !fileID.Equals( string.Empty ) )
				{
					try
					{
						Node node = collection.GetNodeByID( fileID );
						string lastModifier = (string)node.Properties.GetSingleProperty( PropertyTags.LastModifier ).Value;
						if ( userID == null || userID.Equals( string.Empty ) || userID.Equals( lastModifier ) )
						{
							firstEntry = 
								new JournalEntry( "modify", 
								(string)node.Properties.GetSingleProperty( PropertyTags.FileSystemPath ).Value, 
								lastModifier,
								(DateTime)node.Properties.GetSingleProperty( PropertyTags.LastModified ).Value );

							count++;
						}
					}
					catch // Ignore.
					{}
				}
			}

			return count;
		}

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
		/// Get the next entry from the journal file.
		/// </summary>
		/// <returns>A JournalEntry object representing the next entry in the journal file.  If no more entries
		/// are found a null is returned.</returns>
		public JournalEntry GetNextEntry()
		{
			JournalEntry journalEntry = null;
			string record;

			if ( currentRecord == 0 && firstEntry != null )
			{
				// Return the first entry if it exists.
				// TODO: Probably need to re-work this ... this only works when returning the journal entries for a given
				// file.  If the journal for the entire collection is asked for, the file modifications (made locally since
				// the last synchronization) will not be displayed.  We could query the collection for any nodes with a
				// LastModified value greater than the timestamp of the last entry in the journal ... and then return 
				// entries for the FileNodes and DirNodes.
				currentRecord++;
				return firstEntry;
			}

			if ( currentRecord != totalRecords )
			{
				// Read the next record from the file.
				while ( ( record = ReadNextRecord() ) != null )
				{
					try
					{
						// New up a JournalEntry object based on the record.
						JournalEntry tempEntry = new JournalEntry( record );

						// If the JournalEntry object meets the filter criteria, we're done.
						if ( ReturnEntry( tempEntry ) )
						{
							journalEntry = tempEntry;
							currentRecord++;
							break;
						}
					}
					catch ( SimiasException ) // Ignore.
					{}
				}
			}

			return journalEntry;
		}

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

		/// <summary>
		/// Move forward in the journal file the specified number of entries.
		/// </summary>
		/// <param name="numberOfEntries">The number of entries to advance past in the journal file.</param>
		/// <returns><b>True</b> if the move was successful; otherwise <b>False</b> is returned.</returns>
		public bool MoveNext( long numberOfEntries )
		{
			string record;

			// Loop until we've reached the entry.
			while ( numberOfEntries > 0 && ( record = ReadNextRecord() ) != null )
			{
				try
				{
					// Check if the current entry meets the filter criteria.
					if ( ReturnEntry( new JournalEntry( record ) ) )
					{
						currentRecord++;
						numberOfEntries--;
					}
				}
				catch ( SimiasException ) // Ignore.
				{}
			}

			return numberOfEntries == 0;
		}

		/// <summary>
		/// Move back in the journal file the specified number of entries.
		/// </summary>
		/// <param name="numberOfEntries">The number of entries to retreat past in the journal file.  Pass in -1 to move to the beginning of the file.</param>
		/// <returns><b>True</b> if the move was successful; otherwise <b>False</b> is returned.</returns>
		public bool MovePrevious( long numberOfEntries )
		{
			if ( numberOfEntries == -1 )
			{
				// Seeking to the beginning of the file ... reset index and entries then ReadNextRecord will
				// automatically go to the most recent entry (the end of the file).
				index = -1;
				records = null;
				currentRecord = 0;
				return true;
			}

			// Pre-increment the index so that we start in the correct place.
			index++;
			while ( numberOfEntries > 0)
			{
				while ( index < records.Length )
				{
					try
					{
						JournalEntry journalEntry = new JournalEntry( records[ index++ ] );

						// Check if the current entry meets the filter criteria.
						if ( ReturnEntry( journalEntry ) )
						{
							numberOfEntries--;
							currentRecord--;

							if ( numberOfEntries == 0 )
							{
								// We're done, decrement the index so that we read the correct next entry.
								index--;
								break;
							}
						}
					}
					catch ( SimiasException ) // Ignore.
					{}
				}

				if ( numberOfEntries != 0 )
				{
					// Read more data from the file.
					int nBytes = 1024;
					byte[] buffer = new byte[ nBytes ];
					int bytesRead = stream.Read( buffer, 0, nBytes );
					if ( bytesRead > 0 )
					{
						// Find the last CRLF
						int end = 0;
						for ( int n = bytesRead - 1; n >= 0; n-- )
						{
							if ( buffer[ n ] == '\n' )
							{
								end = n + 1;
								break;
							}
						}

						// Reset the file pointer to the next byte after the last CRLF.
						stream.Seek( end - bytesRead, SeekOrigin.Current );

						// Convert the data to an array of strings.
						records = encoding.GetString( buffer, 0, end ).Split( '\n' );
						index = 0;
					}
				}
			}

			return numberOfEntries == 0;
		}

		/// <summary>
		/// Reads the next record in the journal file.
		/// </summary>
		/// <returns>A string representation of the next record in the file.  A null is returned if the end of the file is reached.</returns>
		public string ReadNextRecord()
		{
			string record = null;

			// Check if the index is valid.
			if ( index == -1 )
			{
				if ( !eof )
				{
					// The index is invalid, need to read more data from the file.
					int nBytes;
					if ( stream.Length > 1024 )
					{
						nBytes = 1024;
					}
					else
					{
						nBytes = (int)stream.Length;
						eof = true;
					}

					if ( records == null )
					{
						// Start reading the most-recent entries (at the end of the file).
						stream.Seek( -nBytes, SeekOrigin.End );
					}
					else
					{
						// Continue reading where we left off.
						int cOffset = offset - ( 2 * nBytes );
						if ( stream.Position + cOffset < 0 )
						{
							// Reset the number of bytes to read.
							nBytes = (int)stream.Position - 1024 + offset;
							if ( nBytes < 0 )
								nBytes = 0;

							// Seek to the beginning of the file.
							stream.Seek( -stream.Position, SeekOrigin.Current );
							eof = true;
						}
						else
						{
							stream.Seek( cOffset, SeekOrigin.Current );
						}
					}

					byte[] buffer = new byte[ nBytes ];
					int bytesRead = stream.Read( buffer, 0, nBytes );
					if ( bytesRead > 0 )
					{
						if ( !eof )
						{
							// Find the first CRLF
							for ( int n = 0; n < bytesRead; n++ )
							{
								if ( buffer[ n ] == '\n' )
								{
									offset = n + 1;
									break;
								}
							}
						}
						else
						{
							offset = 0;
						}

						// Convert the data to an array of strings.
						records = encoding.GetString( buffer, offset, bytesRead - offset ).Split( '\n' );

						// Set the index.  The last entry in the array is an empty string.
						index = records.Length - 2;
					}
				}
				else
				{
					eof = false;
				}
			}

			if ( records != null && index != -1 )
			{
				record = records[ index-- ];
			}

			return record;
		}

		/// <summary>
		/// Compares the specified JournalEntry object against the filter criteria.
		/// </summary>
		/// <param name="entry">The JournalEntry object to compare against the filter criteria.</param>
		/// <returns><b>True</b> if the JournalEntry object meets the filter criteria; otherwise <b>False</b> is returned.</returns>
		public bool ReturnEntry( JournalEntry entry )
		{
			bool result = true;

			// Check the fileID.
			if ( fileID != null && !fileID.Equals( string.Empty ) )
			{
				result = fileID.Equals( entry.FileID );
			}

			// Check the userID.
			if ( result && userID != null && !userID.Equals( string.Empty ) )
			{
				result = userID.Equals( entry.UserID );
			}

			// Check the timestamp.
			if ( result && ( fromTime != DateTime.MinValue || toTime != DateTime.MaxValue ) )
			{
				result &= entry.TimeStamp >= fromTime && entry.TimeStamp <= toTime;
			}

			return result;
		}

		/// <summary>
		/// Seeks to the entry with the specified offset.
		/// </summary>
		/// <param name="offset">The index of the entry to seek to.</param>
		/// <returns><b>True</b> if the seek was successful; otherwise <b>False</b> is returned.</returns>
		public bool Seek( uint offset )
		{
			bool result = true;

			if ( offset < currentRecord )
			{
				// Move back in the file.
				result = MovePrevious( offset == 0 ? -1 : currentRecord - offset );
			}
			else if ( offset > currentRecord )
			{
				// Move forward in the file.
				result = MoveNext( offset - currentRecord );
			}

			return result;
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
					stream.Close();
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
	[ Serializable ]
	public class JournalEntry
	{
		// TODO: May need to distinguish between files and directories.
		#region Class Members

		/// <summary>
		/// A value used to indicate if this JournalEntry pertains to a folder.
		/// </summary>
		private bool isFolder;

		/// <summary>
		/// The type of change that this entry refers to.
		/// </summary>
		private string type;

		/// <summary>
		/// The name of the file that this entry refers to.
		/// </summary>
		private string fileName;

		/// <summary>
		/// The identifier of the file node that this entry refers to.
		/// </summary>
		private string fileID = string.Empty;

		/// <summary>
		/// The name of the user that caused this entry.
		/// </summary>
		private string userName;

		/// <summary>
		/// The identifier of the user that caused this entry.
		/// </summary>
		private string userID = string.Empty;

		/// <summary>
		/// The time that the change was made.
		/// </summary>
		private DateTime timeStamp;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the identifier for the file or folder.
		/// </summary>
		public string FileID
		{
			get { return fileID; }
		}

		/// <summary>
		/// Gets/sets the filename for this entry.
		/// </summary>
		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}

		/// <summary>
		/// Gets/sets a value indicating if this entry is related to a folder.
		/// </summary>
		public bool IsFolder
		{
			get { return isFolder; }
			set { isFolder = value; }
		}

		/// <summary>
		/// Gets the timestamp for this entry.
		/// </summary>
		public DateTime TimeStamp
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

		/// <summary>
		/// Gets the identifier of the user for this entry.
		/// </summary>
		public string UserID
		{
			get { return userID; }
		}

		/// <summary>
		/// Gets/sets the username for this entry.
		/// </summary>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Instantiates a JournalEntry object.
		/// </summary>
		/// <param name="type">The type of change that caused the entry.</param>
		/// <param name="userID">The identifier of the user that made the change.</param>
		/// <param name="timeStamp">The time that the change occurred.</param>
		public JournalEntry( string type, string userID, string timeStamp ) :
			this( type, string.Empty, userID, new DateTime( long.Parse( timeStamp ) ) )
		{
		}

		/// <summary>
		/// Instantiates a JournalEntry object.
		/// </summary>
		/// <param name="type">The type of change that caused the entry.</param>
		/// <param name="fileName">The name of the file that the journal entry applies to.</param>
		/// <param name="userID">The identifier of the user that made the change.</param>
		/// <param name="timeStamp">The time that the change occurred.</param>
		public JournalEntry( string type, string fileName, string userID, string timeStamp ) :
			this( type, fileName, userID, new DateTime( long.Parse( timeStamp ) ) )
		{
		}

		/// <summary>
		/// Instantiates a JournalEntry object.
		/// </summary>
		/// <param name="type">The type of change that caused the entry.</param>
		/// <param name="fileName">The name of the file that the journal entry applies to.</param>
		/// <param name="userID">The identifier of the user that made the change.</param>
		/// <param name="timeStamp">The time that the change occurred.</param>
		public JournalEntry( string type, string fileName, string userID, DateTime timeStamp )
		{
			this.type = type;
			this.FileName = fileName;
			this.userID = userID;
			this.timeStamp = timeStamp;
		}

		/// <summary>
		/// Instantiates a JournalEntry object.
		/// </summary>
		/// <param name="record">A string representation of a journal entry from which to construct the JournalEntry
		/// object.</param>
		public JournalEntry( string record )
		{
			string[] entries = record.Split( ':' );

			if ( entries.Length < 4 )
			{
				throw new SimiasException( "Incomplete record" );
			}
			
			switch (  entries[0] )
			{
				case "1":
					type = "add";
					break;
				case "2":
					type = "delete";
					break;
				case "4":
					type = "modify";
					break;
				default:
					type = "unknown";
					break;
			}

			this.userID = entries[1];
			this.fileID = entries[2];
			this.timeStamp = new DateTime( long.Parse( entries[3] ) );

			if ( entries.Length == 5 )
			{
				this.fileName = entries[4];

				// Make sure the file name doesn't end with a CR.
				if ( fileName.EndsWith( "\r" ) )
				{
					fileName = fileName.Substring( 0, fileName.Length - 1 );
				}
			}
		}

		#endregion
	}
}
