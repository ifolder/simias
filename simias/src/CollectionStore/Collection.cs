/***********************************************************************
 *  Collection.cs - Class that implements the containment and access of
 *  properties and nodes that form a collection.
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
 *  Author: Mike Lasky <mlasky@novell.com>
 * 
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Xml;
using Simias;
using Simias.Event;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// A collection is contained by a Store.  It contains properties and nodes that describe a grouping
	/// of objects (such as files).  A collection cannot be contained in another Collection or within a
	/// node.
	/// </summary>
	public class Collection : Node, IDisposable
	{
		#region Class Members
		/// <summary>
		/// Initial size of the list that keeps track of the dirty nodes.
		/// </summary>
		private const int initialDirtyNodeListSize = 10;

		/// <summary>
		/// Reference to the persistent database object.
		/// </summary>
		private Persist.IProvider database;

		/// <summary>
		/// Domain that this collection belongs to.
		/// </summary>
		private string domainName = null;

		/// <summary>
		/// Subscriber event used to keep the cached node table up to date.
		/// </summary>
		private EventSubscriber subscriber = null;

		/// <summary>
		/// Array of node Ids to filter events with.
		/// </summary>
		private string[] nodeFilter = null;

		/// <summary>
		/// Event handler for node change delegates.
		/// </summary>
		private event NodeChangeHandler nEventHandler = null;

		/// <summary>
		/// Indicates if object has been disposed.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Delegate to capture node change events for this collection.
		/// </summary>
		public delegate void NodeChangeHandler( NodeEventArgs args );
		#endregion

		#region Properties
		/// <summary>
		/// Gets whether current user has owner access rights to this collection.
		/// </summary>
		internal bool HasOwnerAccess
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return cNode.accessControl.IsOwnerAccessAllowed(); 
			}
		}

		/// <summary>
		/// Gets the identity that the current user is known as in the collection's domain.
		/// </summary>
		internal string DomainIdentity
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return store.CurrentIdentity.GetDomainUserGuid( DomainName ); 
			}
		}

		/// <summary>
		/// Gets the local store handle.
		/// </summary>
		public Store LocalStore
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return store; 
			}
		}

		/// <summary>
		///  Gets the current owner of the collection.
		/// </summary>
		public string Owner
		{
			// An owner property will always exist.
			get 
			{ 
				lock ( store )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					return cNode.accessControl.Owner; 
				}
			}
		}

		/// <summary>
		/// Gets or sets whether this collection can be shared.  By default, a collection is always shareable.
		/// The Collection Store cannot prevent an application from sharing a collection even though this property
		/// is set non-shareable.  This property is only meant as a common means to indicate shareability and must
		/// be enforced at a higher layer.
		/// </summary>
		public bool Shareable
		{
			get 
			{
				lock ( store )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					Property p = Properties.GetSingleProperty( Property.Shareable );
					bool shareable = ( p != null ) ? ( bool )p.Value : true;
					return ( IsAccessAllowed( Access.Rights.Admin ) && shareable && Synchronizable ) ? true : false;
				}
			}

			set 
			{
				lock ( store )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					// Only allow the collection owner to set this property.
					if ( !HasOwnerAccess )
					{
						throw new ApplicationException( "Current user is not the collection owner." );
					}

					Properties.ModifyNodeProperty( Property.Shareable, value );
				}
			}
		}

		/// <summary>
		/// Gets or sets whether this collection can be synchronized.  By default, a collection is always synchronizeable.
		/// The Collection Store cannot prevent an application from synchronizing a collection even though this property
		/// is set not synchronizable.  This property is only meant as a common means to indicate synchronizability and must
		/// be enforced at a higher layer.
		/// </summary>
		public bool Synchronizable
		{
			get 
			{
				lock ( store )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					Property p = Properties.GetSingleProperty( Property.Syncable );
					return ( p != null ) ? ( bool )p.Value : true;
				}
			}

			set 
			{
				lock ( store )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					// Only allow the collection owner to set this property.
					if ( !HasOwnerAccess )
					{
						throw new ApplicationException( "Current user is not the collection owner." );
					}

					Properties.ModifyNodeProperty( Property.Syncable, value );
				}
			}
		}

		/// <summary>
		/// Gets or sets whether this collection can be synchronized.  By default, a collection is always synchronizeable.
		/// The Collection Store cannot prevent an application from synchronizing a collection even though this property
		/// is set not synchronizable.  This property is only meant as a common means to indicate synchronizability and must
		/// be enforced at a higher layer.
		/// </summary>
		[ Obsolete( "This property is marked for removal.  Use Property 'Synchronizable' instead.", false ) ]
		public bool Synchronizeable
		{
			get { return Synchronizable; }
			set { Synchronizable = value; }
		}

		/// <summary>
		/// Gets the domain name that this collection belongs to.
		/// </summary>
		public string DomainName
		{
			get 
			{
				lock ( store )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					if ( domainName == null )
					{
						Property p = Properties.GetSingleProperty( Property.DomainName );
						domainName = p.ToString();
					}

					return domainName;
				}
			}
		}

		/// <summary>
		/// Gets and sets the document root where all files belonging to the collection are rooted.  If the document
		/// root is changed, all files belonging to the collection are moved to the new document root in the file system.
		/// </summary>
		public Uri DocumentRoot
		{
			get 
			{
				lock ( store )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					Property p = Properties.GetSingleProperty( Property.DocumentRoot );
					return ( p != null ) ? ( Uri )p.Value : null;
				}
			}

			set 
			{ 
				lock ( store )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					MoveRoot( value ); 
				}
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor to create a new collection object.
		/// </summary>
		/// <param name="store">Virtual store that this collection belongs to.</param>
		/// <param name="name">Name that is used by applications to describe the collection.</param>
		/// <param name="id">Globally unique identifier for this collection.</param>
		/// <param name="type">Type of collection.</param>
		/// <param name="documentRoot">Path where the collection documents are rooted.</param>
		public Collection( Store store, string name, string id, string type, Uri documentRoot ) :
			base( store, name, ( id == String.Empty ) ? Guid.NewGuid().ToString() : id, CollectionType + type, false )
		{
			lock ( store )
			{
				// Set the collection into the node.  Since node is a sub-class of collection, its 
				// constructor runs before the collection constructor, so we can't set the 'this' value.
				InternalCollectionHandle = this;

				// Setup the dirty list.
				cNode.dirtyNodeList = new Hashtable( initialDirtyNodeListSize );

				// Don't allow another database object to be created.
				if ( ( type == Store.DatabaseType ) && ( store.GetDatabaseObject() != null ) )
				{
					throw new ApplicationException( Store.DatabaseType + " already exists." );
				}

				// Don't allow this collection to be created, if one already exist by the same id.
				if ( store.GetCollectionById( Id ) != null )
				{
					throw new ApplicationException( "Collection already exists with specified ID." );
				}

				// Initialize my class members.
				this.store = store;
				this.database = store.StorageProvider;

				// Initialize the access control object.
				cNode.accessControl = new AccessControl( this );

				// Set the default access control for this collection.
				cNode.accessControl.SetDefaultAccessControl();
				UpdateAccessControl();

				// If no document root was passed in, use the default one.
				if ( documentRoot == null )
				{
					documentRoot = GetStoreManagedPath();
				}

				// If the document root directory does not exist, create it.
				if ( !Directory.Exists( documentRoot.LocalPath ) )
				{
					Directory.CreateDirectory( documentRoot.LocalPath );
				}

				// Set the default properties for this node.
				Properties.AddNodeProperty( Property.CreationTime, DateTime.UtcNow );
				Properties.AddNodeProperty( Property.ModifyTime, DateTime.UtcNow );
				Properties.AddNodeProperty( Property.CollectionID, Id );
				Properties.AddNodeProperty( Property.IDPath, "/" + Id );
				Properties.AddNodeProperty( Property.DomainName, store.DomainName );

				// Add the document root as a local property.
				Property docRootProp = new Property( Property.DocumentRoot, documentRoot );
				docRootProp.LocalProperty = true;
				Properties.AddNodeProperty( docRootProp );

				// Set the sync versions.
				Property mvProp = new Property( Property.MasterIncarnation, ( ulong )0 );
				mvProp.LocalProperty = true;
				Properties.AddNodeProperty( mvProp );

				Property lvProp = new Property( Property.LocalIncarnation, ( ulong )0 );
				lvProp.LocalProperty = true;
				Properties.AddNodeProperty( lvProp );

				// Add this node to the cache table.
				cNode = cNode.AddToCacheTable();
			}
		}

		/// <summary>
		/// Constructor to create a new collection object.
		/// </summary>
		/// <param name="store">Virtual store that this collection belongs to.</param>
		/// <param name="name">Name that is used by applications to describe the collection.</param>
		/// <param name="type">Type of collection.</param>
		/// <param name="documentRoot">Path where the collection documents are rooted.</param>
		public Collection( Store store, string name, string type, Uri documentRoot ) :
			this( store, name, String.Empty, type, documentRoot )
		{
		}

		/// <summary>
		/// Constructor to create a new collection object that contains store-managed files.
		/// </summary>
		/// <param name="store">Virtual store that this collection belongs to.</param>
		/// <param name="name">Name that is used by applications to describe the collection.</param>
		/// <param name="type">Type of collection.</param>
		public Collection( Store store, string name, string type ) :
			this( store, name, String.Empty, type, ( Uri )null )
		{
		}

		/// <summary>
		/// Constructor to create an existing collection object.
		/// </summary>
		/// <param name="store">Virtual store that this collection belongs to.</param>
		/// <param name="xmlProperties">List of properties that belong to this collection.</param>
		/// <param name="imported">Set to true if collection is being imported.</param>
		internal Collection( Store store, XmlElement xmlProperties, bool imported ) :
			base( store, xmlProperties )
		{
			// Set the collection into the node.  Since node is a sub-class of collection, its 
			// constructor runs before the collection constructor, so we can't set the value.
			InternalCollectionHandle = this;

			// Setup the dirty list.
			cNode.dirtyNodeList = new Hashtable( initialDirtyNodeListSize );

			// Initialize my class members.
			this.store = store;
			this.database = store.StorageProvider;

			// Initialize the access control object.
			cNode.accessControl = new AccessControl( this );
			UpdateAccessControl();

			if ( imported )
			{
				// Because this node is being imported, it needs to be the one in the cache node table.
				// Otherwise all import changes will be lost.
				CacheNode tempCacheNode = store.GetCacheNode( Id );
				if ( tempCacheNode != null )
				{
					// Copy this cache node to the one in the table so that all node will see the import
					// changes.
					tempCacheNode.Copy( cNode );

					// GetCacheNode() incremented the reference count.  Need to decrement it.
					store.RemoveCacheNode( tempCacheNode, false );
				}

				// Add this node to the cache table.
				cNode = cNode.AddToCacheTable();

				// Add this object to the dirty list.
				AddDirtyNodeToList( this );
			}
			else
			{
				// Add this node to the cache table.
				cNode = cNode.AddToCacheTable();
			}
		}

		/// <summary>
		/// Constructor to create an existing collection object without properties.
		/// </summary>
		/// <param name="store">Virtual store that this collection belongs to.</param>
		/// <param name="name">Name used by applications to describe the collection.</param>
		/// <param name="id">Globally unique identifier for this collection.</param>
		/// <param name="type">Type of collection.</param>
		internal Collection( Store store, string name, string id, string type ) :
			base( store, name, id, type, true )
		{
			// Set the collection into the node.  Since node is a sub-class of collection, its 
			// constructor runs before the collection constructor, so we can't set the value.
			InternalCollectionHandle = this;

			// Setup the dirty list.
			cNode.dirtyNodeList = new Hashtable( initialDirtyNodeListSize );

			// Initialize my class members.
			this.store = store;
			this.database = store.StorageProvider;

			// Initialize the access control object.
			cNode.accessControl = new AccessControl( this );

			// Add this node to the cache table.
			cNode = cNode.AddToCacheTable();
		}

		/// <summary>
		/// Constructor to create an existing collection object without properties with a specified owner.
		/// This constructor is used at store construction time because there is no current owner of the store 
		/// established yet.
		/// </summary>
		/// <param name="store">Virtual store that this collection belongs to.</param>
		/// <param name="name">Name used by applications to describe the collection.</param>
		/// <param name="id">Globally unique identifier for this collection.</param>
		/// <param name="type">Type of collection.</param>
		/// <param name="constructorId">Identifier of user opening this object.</param>
		internal Collection( Store store, string name, string id, string type, string constructorId ) :
			base( store, name, id, type, true )
		{
			// Set the collection into the node.  Since node is a sub-class of collection, its 
			// constructor runs before the collection constructor, so we can't set the value.
			InternalCollectionHandle = this;

			// Setup the dirty list.
			cNode.dirtyNodeList = new Hashtable( initialDirtyNodeListSize );

			// Initialize my class members.
			this.store = store;
			this.database = store.StorageProvider;

			// Initialize the access control object.
			cNode.accessControl = new AccessControl( this, constructorId );

			// Add this node to the cache table.
			cNode = cNode.AddToCacheTable();
		}

		/// <summary>
		/// Constructor for creating a collection from a cache node.
		/// </summary>
		/// <param name="store">Object representing the local store.</param>
		/// <param name="cNode">Cache node that contains the node data.</param>
		/// <param name="incReference">Increments the reference count on cNode if true.</param>
		internal Collection( Store store, CacheNode cNode, bool incReference ) :
			base( cNode, incReference )
		{
			// Initialize my class members.
			this.store = store;
			this.database = store.StorageProvider;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Checks to see if this event should be filtered out.
		/// </summary>
		/// <param name="args">Event context received from callback node changed event.</param>
		/// <returns>True if event is to be published, otherwise false is returned.</returns>
		private bool ApplyNodeFilter( NodeEventArgs args )
		{
			bool publish = false;

			// Don't publish events that came from this store handle.
			if ( args.EventId != store.Instance )
			{
				// If no filter specified, then publish all events.
				if ( nodeFilter == null )
				{
					publish = true;
				}
				else
				{
					// Walk the filter list looking for a matching node id.
					foreach ( string nodeId in nodeFilter )
					{
						if ( nodeId == args.Node )
						{
							publish = true;
							break;
						}
					}
				}
			}

			return publish;
		}

		/// <summary>
		/// Moves where the files in the collection are rooted in the filesystem.  This change will automatically commit
		/// the collection node and cannot be rolled back.
		/// </summary>
		/// <param name="newRoot">New location to root collection files.</param>
		private void MoveRoot( Uri newRoot )
		{
			// Make sure the current user has write access to this collection.
			if ( !IsAccessAllowed( Access.Rights.ReadWrite ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
			}

			// Move the file system directory where all of the files are contained.
			string sourcePathString = DocumentRoot.LocalPath;

			// Try and move the files to the new directory.
			Directory.Move( sourcePathString, newRoot.LocalPath );

			try
			{
				// Now reset the new document root.
				Properties.ModifyNodeProperty( Property.DocumentRoot, newRoot );
				Commit();
			}
			catch ( Exception e )
			{
				try
				{
					// Attempt to move the files back.
					Directory.Move( newRoot.LocalPath, sourcePathString );

					// Generate event that document root was changed.
					store.Publisher.RaiseEvent( new CollectionRootChangedEventArgs( store.ComponentId, Id, NameSpaceType, sourcePathString, newRoot.LocalPath ) );
				}
				catch
				{
					// Don't report any errors putting the files back.
					;
				}

				throw e;
			}
		}

		/// <summary>
		/// Callback that handles node events from the event broker.
		/// </summary>
		/// <param name="args">Arguments that give context for the call.</param>
		private void OnNodeChanged( NodeEventArgs args )
		{
			try
			{
				// See if there is any delegates registered.
				if ( nEventHandler != null )
				{
					if ( ApplyNodeFilter( args ) )
					{
						nEventHandler( args ); 
					}
				}
			}
			catch ( Exception e )
			{
				MyTrace.WriteLine( e );
			}
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Adds nodes to a list that need to be written to the persistent store.
		/// </summary>
		/// <param name="dirtyNode">Node object to add to the list.</param>
		internal void AddDirtyNodeToList( Node dirtyNode )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			if ( !cNode.dirtyNodeList.ContainsKey( dirtyNode.Id ) && !dirtyNode.IsTombstone )
			{
				cNode.dirtyNodeList.Add( dirtyNode.Id, dirtyNode.cNode );
			}
		}

		/// <summary>
		/// Clears out the dirty node list.
		/// </summary>
		internal void ClearDirtyList()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			cNode.dirtyNodeList.Clear();
		}

		/// <summary>
		/// Gets a path to where the store managed files for this collection should be created.
		/// </summary>
		/// <returns>A Uri object that represents the store managed path.</returns>
		internal Uri GetStoreManagedPath()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			return new Uri( Path.Combine( database.StoreDirectory.LocalPath, Path.Combine( store.StoreManagedPath.LocalPath, Id ) ) );
		}

		/// <summary>
		/// Removes the specified node from the list.
		/// </summary>
		/// <param name="nodeId">Node identifier to remove from the dirtyList.</param>
		internal void RemoveDirtyNodeFromList( string nodeId )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			cNode.dirtyNodeList.Remove( nodeId );
		}

		// Updates the access control list from the committed properties.
		internal void UpdateAccessControl()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			cNode.accessControl.GetCommittedAcl();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Changes the owner of the collection and assigns the specified right to the old owner.
		/// Only the current owner can set new ownership on the collection. 
		/// </summary>
		/// <param name="newOwnerId">User identifier of the new owner.</param>
		/// <param name="oldOwnerRights">Rights to give the old owner of the collection.</param>
		public void ChangeOwner( string newOwnerId, Access.Rights oldOwnerRights )
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				cNode.accessControl.ChangeOwner( newOwnerId.ToLower(), oldOwnerRights );
			}
		}

		/// <summary>
		/// Commits all changes in the collection to persistent storage if deep is set to true.
		/// Otherwise, just commits the collection node. After a node has been committed, it 
		/// will be updated to reflect any new changes that occurred if it had to be merged 
		/// with the current node in the database.
		/// </summary>
		public void Commit( bool deep )
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Make sure the current user has write access to this collection.
				if ( !IsAccessAllowed( Access.Rights.ReadWrite ) )
				{
					throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
				}

				if ( deep )
				{
					// Allocate a queue to hold committed nodes.
					Queue nodeQ = new Queue( cNode.dirtyNodeList.Count );

					// Create an XML document that will contain all of the changed nodes.
					XmlDocument commitDoc = new XmlDocument();
					commitDoc.AppendChild( commitDoc.CreateElement( Property.ObjectListTag ) );

					try
					{
						// Acquire the store lock.
						store.LockStore();

						// Increment the collection incarnation number here so it gets added to the dirty list and
						// processed with the rest of the changed nodes.
						IncrementLocalIncarnation();

						// Parse the node into an XML document because that is the format that the provider expects.
						foreach ( CacheNode tempCacheNode in cNode.dirtyNodeList.Values )
						{
							// Instantiate a node object from the cache node.
							Node tempNode = new Node( tempCacheNode, true );

							// If this node has not been persisted, no need to do a merge.
							if ( tempNode.IsPersisted )
							{
								// Merge this node with the current node in the database.
								tempNode = tempNode.MergeNodeProperties( true );
								if ( tempNode == null )
								{
									// The node has been deleted in the database, move to the next one.
									continue;
								}

								// Update this node to reflect the latest changes.
								tempCacheNode.Copy( tempNode.cNode );
							}

							// Set the modify time for this node.
							tempNode.Properties.ModifyNodeProperty( "ModifyTime", DateTime.UtcNow );

							// Don't increment the incarnation number on the collection again.
							if ( !tempNode.IsCollection )
							{
								// Increment the local incarnation number.
								tempNode.IncrementLocalIncarnation();
							}

							// Copy the XML node over to the modify document.
							XmlNode xmlNode = commitDoc.ImportNode( tempNode.Properties.PropertyRoot, true );
							commitDoc.DocumentElement.AppendChild( xmlNode );

							// Add the cache node to the queue.
							nodeQ.Enqueue( tempCacheNode );
						}

						// If this collection is new, call to create it before sending down the nodes.
						if ( !IsPersisted )
						{
							database.CreateCollection( Id );
						}

						// Call the store provider to create the records.
						database.CreateRecord( commitDoc.OuterXml, Id );
					}
					finally
					{
						// Release the store lock.
						store.UnlockStore();
					}

					// Set all of the nodes in the list as committed.
					foreach ( CacheNode tempCacheNode in nodeQ )
					{
						// Fire an event for this commit action.
						if ( tempCacheNode.isPersisted )
						{
							// Fire an event to notify that this node has been changed.
							store.Publisher.RaiseEvent( new NodeEventArgs( store.ComponentId, tempCacheNode.id, Id, tempCacheNode.type, EventType.NodeChanged, store.Instance ) );
						}
						else
						{
							// Fire an event to notify that this node has been created.
							store.Publisher.RaiseEvent( new NodeEventArgs( store.ComponentId, tempCacheNode.id, Id, tempCacheNode.type, EventType.NodeCreated, store.Instance ) );

							// Mark the node as persisted.
							tempCacheNode.isPersisted = true;
						}
					}

					// Clear the cached node queue.
					nodeQ.Clear();

					// Clear the dirty node queue.
					ClearDirtyList();

					// Update the access control list.
					UpdateAccessControl();
				}
				else
				{
					base.Commit();
				}
			}
		}

		/// <summary>
		/// Deletes the specified collection from the persistent store.  If there are nodes
		/// subordinate to this collection, an exception will be thrown.
		/// </summary>
		public new void Delete()
		{
			Delete( false );
		}

		/// <summary>
		/// Deletes the specified collection from the persistent store.  There is no access check on delete of a
		/// collection.
		/// </summary>
		/// <param name="deep">Indicates whether to all children nodes of this node are deleted also.</param>
		public new void Delete( bool deep )
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Delete the collection and all of its members if specified.
				base.Delete( deep );

				// If there are store managed files, delete them also.
				Uri documentRoot = GetStoreManagedPath();
				if ( Directory.Exists( documentRoot.LocalPath ) )
				{
					Directory.Delete( documentRoot.LocalPath, true );
				}
			}
		}

		/// <summary>
		/// Gets the access control list for this collection object.
		/// </summary>
		/// <returns>An ICSEnumerator object that will enumerate the access control list.</returns>
		public ICSList GetAccessControlList()
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return new ICSList( new Access( this ) );
			}
		}

		/// <summary>
		/// Gets the access rights for the specified user on the collection.
		/// </summary>
		/// <param name="userId">User ID to get rights for.</param>
		/// <returns>Access rights for the specified user ID.</returns>
		public Access.Rights GetUserAccess( string userId )
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return cNode.accessControl.GetUserRights( userId.ToLower() );
			}
		}

		/// <summary>
		/// Checks whether the current user has sufficient access rights for an operation.
		/// </summary>
		/// <param name="desiredRights">Desired access rights.</param>
		/// <returns>True if the user has the desired access rights, otherwise false.</returns>
		public bool IsAccessAllowed( Access.Rights desiredRights )
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return cNode.accessControl.IsAccessAllowed( desiredRights );
			}
		}

		/// <summary>
		/// Removes all access rights on the collection for the specified user.
		/// </summary>
		/// <param name="userId">User ID to remove rights for.</param>
		public void RemoveUserAccess( string userId )
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				cNode.accessControl.RemoveUserRights( userId.ToLower() );
			}
		}

		/// <summary>
		/// Rolls back changes made to the last time the collection was committed.  If the a node has never been committed,
		/// it is just removed from the transaction list.
		/// </summary>
		new public void Rollback()
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Take each node that is currently on the dirty list and roll it back to it's post committed state.
				foreach ( CacheNode tempCacheNode in cNode.dirtyNodeList.Values )
				{
					new Node( tempCacheNode, true ).RollbackNode();
				}

				ClearDirtyList();
			}
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="property">Property object containing the value to search for.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( Property property, Property.Operator queryOperator )
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return new ICSList( new NodeEnumerator( this, property, Property.MapQueryOp( queryOperator ) ) );
			}
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, object propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, string propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified sbyte property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">sbyte value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, sbyte propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified byte property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">byte value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, byte propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified short property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">short value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, short propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified ushort property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">ushort value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, ushort propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified int properties.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">int value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, int propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified uint property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">uint value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, uint propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified long property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">long value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, long propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified ulong property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">ulong value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, ulong propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified char property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">char value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, char propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified float property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">float value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, float propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified bool property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">bool value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, bool propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified DateTime property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">DateTime value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, DateTime propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified Uri property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Uri value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, Uri propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified XmlDocument property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">XmlDocument value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, XmlDocument propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified TimeSpan property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">TimeSpan value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, TimeSpan propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Sets the specified access rights for the specified user on the collection.
		/// </summary>
		/// <param name="userId">User to add to the collection's access control list.</param>
		/// <param name="desiredRights">Rights to assign to user.</param>
		public void SetUserAccess( string userId, Access.Rights desiredRights )
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				if ( userId == String.Empty )
				{
					throw new ApplicationException( "Invalid user guid." );
				}

				cNode.accessControl.SetUserRights( userId.ToLower(), desiredRights );
			}
		}

		/// <summary>
		/// Subscribes to node change events.
		/// </summary>
		/// <param name="handler">Delegate which defines handler signature.</param>
		/// <param name="nodeIdFilter">Specifies a list of nodes to watch for changes.  If null, then all
		/// node changes in the collection will be indicated.</param>
		public void NodeEventsSubscribe( NodeChangeHandler handler, string[] nodeIdFilter )
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Setup to watch for node changes on this collection.
				subscriber = new EventSubscriber( store.Config, Id);
				subscriber.NodeChanged += new NodeEventHandler( OnNodeChanged );
				subscriber.NodeCreated += new NodeEventHandler( OnNodeChanged );
				subscriber.NodeDeleted += new NodeEventHandler( OnNodeChanged );

				// Register the delegate with the event handler.
				nEventHandler += handler;
				nodeFilter = nodeIdFilter;
			}
		}

		/// <summary>
		/// Unsubscribes from node change events.
		/// </summary>
		/// <param name="handler">Delegate passed to NodeEventsSubscribe.</param>
		public void NodeEventsUnsubscribe( NodeChangeHandler handler )
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Deregister from the event broker.
				subscriber = new EventSubscriber( store.Config );
				subscriber.NodeChanged -= new NodeEventHandler( OnNodeChanged );
				subscriber.NodeCreated -= new NodeEventHandler( OnNodeChanged );
				subscriber.NodeDeleted -= new NodeEventHandler( OnNodeChanged );
				subscriber.Dispose();

				// Deregister the delegate with the event handler.
				nEventHandler -= handler;
				nodeFilter = null;
			}
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Mandatory method used by clients to enumerate node objects.
		/// </summary>
		/// <remarks>
		/// The client must call Dispose() to free up system resources before releasing
		/// the reference to the ICSEnumerator.
		/// </remarks>
		/// <returns>IEnumerator object used to enumerate nodes within collections.</returns>
		public new IEnumerator GetEnumerator()
		{
			lock ( store )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return new NodeEnumerator( this, new Property( Property.CollectionID, Id ), Persist.Query.Operator.Equal );
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
			lock ( store )
			{
				Dispose( true );
				GC.SuppressFinalize( this );
			}
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
					if ( nEventHandler != null )
					{
						Delegate[] delegateList = nEventHandler.GetInvocationList();
						foreach ( Delegate d in delegateList )
						{
							nEventHandler -= d as NodeChangeHandler;
						}

						nEventHandler = null;
					}

					if ( subscriber != null )
					{
						subscriber.Dispose();
					}

					database = null;
					store = null;
				}
			}
		}
		
		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~Collection()      
		{
			lock ( store )
			{
				Dispose( false );
			}
		}
		#endregion
	}
}
