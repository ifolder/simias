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
		bool commitCollection = false;
//		FileStream stream;

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

				// Open the stream to the temporary file.
//				stream = File.Open( filename, FileMode.Open, FileAccess.ReadWrite );
			}
		}

		#endregion

		#region Properties
		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the journal entries for the collection.
		/// </summary>
		/// <param name="searchString">The string to search for.</param>
		/// <param name="searchType">The type of search to perform.</param>
		/// <returns>An ICSList object that contains JournalEntry objects for the collection.  Journal entries are 
		/// stored in the ICSList in reverse order (i.e. the most recent entry is first).</returns>
		private ICSList GetJournalEntries( string searchString, string searchType )
		{
			ArrayList list = new ArrayList();

			string filename = journalNode.GetFullPath( collection );

			using ( StreamReader reader = new StreamReader( filename ) )
			{
				JournalEntry je = null;
				string record;
				while ( ( record = reader.ReadLine() ) != null )
				{
					// By default each record is returned.
					bool insertValue = true;
					if ( !searchString.Equals( string.Empty ) )
					{
						// If searching, only return the record if it contains the search string.
						insertValue = record.IndexOf( searchString ) != -1;
					}

					if ( insertValue )
					{
						try
						{
							je = new JournalEntry( record );
							list.Insert( 0, je );
						}
						catch ( SimiasException )
						{}
					}
				}

				// TODO: Is there a way to update the journal for newly added files? (The journal on the
				// the client is always a sync cycle behind the journal on the server).
				// Add the last modifier to the journal if it isn't already there.
				if ( searchType.Equals( "fileID" ) )
				{
					Node node = collection.GetNodeByID( searchString );
					if ( node != null )
					{
						Property property = node.Properties.GetSingleProperty( PropertyTags.LastModified );
						if ( property != null )
						{
							string lastModified = (string)property.Value;

							property = node.Properties.GetSingleProperty( PropertyTags.LastModifier );
							if ( property != null )
							{
								string lastModifier = (string)property.Value;

								if ( ( je == null ) || ( !je.UserID.Equals( lastModifier ) ) )
								{
									je = new JournalEntry( "modify", lastModifier, lastModified );
									list.Insert( 0, je );
								}
							}
						}
					}
				}
			}

			return new ICSList( list );
		}

		/// <summary>
		/// Gets all journal entries for the collection.
		/// </summary>
		/// <returns></returns>
		private ICSList GetJournalEntries()
		{
			return GetJournalEntries( string.Empty, string.Empty );
		}

		/// <summary>
		/// Gets the journal entries for a file.
		/// </summary>
		/// <param name="relativeFilename">The relative name of the file.</param>
		/// <returns>An ICSList object that contains JournalEntry objects for the file.  Journal entries are
		/// stored in the ICSList in reverse order (i.e. the most recent entry is first).</returns>
		private ICSList GetJournalEntriesForPath( string relativeFilename )
		{
			ArrayList arrayList = new ArrayList();

			Property property = new Property( PropertyTags.FileSystemPath, relativeFilename );
			ICSList list = collection.Search( property, SearchOp.Equal );
			if ( list.Count == 1 )
			{
				foreach ( ShallowNode sn in list )
				{
					Node node = new Node( collection, sn );

					// If this is the root DirNode, then get the journal for the collection.
					if ( node.Properties.GetSingleProperty( PropertyTags.Root ) != null )
					{
						return GetJournalEntries();
					}
					else
					{
						return GetJournalEntries( node.ID, "fileID" );
					}
				}
			}

			// If more than one was returned by the search, a collision exists ... cannot view the journal in this case

			return new ICSList( arrayList );
		}

		#endregion

		#region Public Methods

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
		/// <param name="collectionID">The identifier of the collection to search for members in.</param>
		/// <param name="relativePath">The relative path of the directory/file to retrieve the journal for.</param>
		/// <param name="count">Maximum number of JournalEntry objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="journalList">Receives an array object that contains the JournalEntry objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more journal entries. Otherwise false is returned.</returns>
		public bool FindFirstEntries( string relativePath, int count, out string searchContext, out JournalEntry[] journalList, out int total )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			searchContext = null;
			journalList = null;
			total = 0;

			ICSList list = GetJournalEntriesForPath( relativePath );
			JournalSearchState searchState = new JournalSearchState( collection.ID, list.GetEnumerator() as ICSEnumerator, list.Count );
			searchContext = searchState.ContextHandle;
			total = list.Count;
			moreEntries = FindNextEntries( ref searchContext, count, out journalList );

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for journal entries from the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstJournalEntries method.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
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
				// See if entries are to be returned.
				if ( count > 0 )
				{
					// Get the domain for this collection.
					Domain domain = store.GetDomain( collection.Domain );
					if ( domain != null )
					{
						// Allocate a list to hold the member objects.
						ArrayList tempList = new ArrayList( count );
						ICSEnumerator enumerator = searchState.Enumerator;
						while( ( count > 0 ) && enumerator.MoveNext() )
						{
							// The enumeration returns ShallowNode objects.
							JournalEntry je = enumerator.Current as JournalEntry;
							Member member = domain.GetMemberByID( je.UserID );
							je.UserName = member.FN != null ? member.FN : member.Name;

							if ( je.FileID != null )
							{
								Node node = collection.GetNodeByID( je.FileID );
								if ( node != null )
								{
									if ( node.IsType( NodeTypes.FileNodeType ) )
									{
										FileNode fileNode = new FileNode( node );
										if ( fileNode != null )
										{
											je.FileName = fileNode.GetRelativePath();
										}
									}
									else 
									{
										DirNode dirNode = new DirNode( node );
										if ( dirNode != null )
										{
											je.FileName = dirNode.GetRelativePath();
										}
									}
								}
								else
								{
//									je.FileName = GetDeletedFileName( je.FileID );
								}
							}

							tempList.Add( je );
							--count;
						}

						if ( tempList.Count > 0 )
						{
							journalList = tempList.ToArray( typeof ( JournalEntry ) ) as JournalEntry[];
							searchState.CurrentRecord += journalList.Length;
							searchState.LastCount = journalList.Length;
							moreEntries = ( count == 0 ) ? true : false;
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
		/// <param name="count">Maximum number of member objects to return.</param>
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
				// Backup the current cursor, but don't go passed the first record.
				if ( searchState.CurrentRecord > 0 )
				{
					bool invalidIndex = false;
					int cursorIndex = ( searchState.CurrentRecord - ( searchState.LastCount + count ) );
					if ( cursorIndex < 0 )
					{
						invalidIndex = true;
						count = searchState.CurrentRecord - searchState.LastCount;
						cursorIndex = 0;
					}

					// Set the new index for the cursor.
					if ( searchState.Enumerator.SetCursor( Simias.Storage.Provider.IndexOrigin.SET, cursorIndex ) )
					{
						// Reset the current record.
						searchState.CurrentRecord = cursorIndex;

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
		/// <param name="count">Maximum number of JournalEntry objects to return.</param>
		/// <param name="journalList">Receives an array object that contains the JournalEntry objects.</param>
		/// <returns>True if there are more journal entries. Otherwise false is returned.</returns>
		public bool FindSeekEntries( ref string searchContext, int offset, int count, out JournalEntry[] journalList )
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
					if ( searchState.Enumerator.SetCursor( Simias.Storage.Provider.IndexOrigin.SET, offset ) )
					{
						// Reset the current record.
						searchState.CurrentRecord = offset;

						// Complete the search.
						moreEntries = FindNextEntries( ref searchContext, count, out journalList );
					}
				}
			}

			return moreEntries;
		}

		#endregion

		#region Internal Methods

		public void UpdateJournal( NodeEventArgs args )
		{
			if ( collection.Role.Equals(SyncRoles.Master) )
			{
				// TODO: check for renames.
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

				// Put a relationship to the journal on the collection if it doesn't already exist.
//				if ( collection.Properties.GetSingleProperty( PropertyTags.Journal ) == null )
//				{
//					collection.Properties.AddNodeProperty( PropertyTags.Journal, new Relationship( collection.ID, journalNode.ID ) );
//					commitCollection = true;
//				}
			}
		}

		public void Commit()
		{
			if ( collection.Role.Equals(SyncRoles.Master) )
			{
				if ( commitCollection )
				{
					collection.Commit( new Node[] { collection, journalNode } );
				}
				else
				{
					collection.Commit( journalNode );
				}
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
	[ Serializable ]
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
			this.timeStamp = new DateTime( long.Parse( timeStamp ) );
		}

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
			}

			this.userID = entries[1];
			this.fileID = entries[2];
			this.timeStamp = new DateTime( long.Parse( entries[3] ) );

			if ( entries.Length == 5 )
			{
				this.fileName = entries[4];
			}
		}
		#endregion
	}
}
