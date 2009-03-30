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
*                 $Author: Mike Lasky <mlasky@novell.com>
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
using System.IO;
using System.Xml;

using Simias.Client;
using Simias.Sync;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents the local address book that contains identity objects which
	/// represent users of the collection store.
	/// </summary>
	public class LocalDatabase : Collection
	{
		#region Properties
		/// <summary>
		/// Gets or sets the default domain ID.
		/// </summary>
		public string DefaultDomain
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.DefaultDomain );
				return (p != null ) ? p.ToString() : null; 
			}

			set 
			{ 
				if ( value == null )
				{
					properties.DeleteSingleNodeProperty( PropertyTags.DefaultDomain );
				}
				else
				{
					properties.ModifyNodeProperty( PropertyTags.DefaultDomain, value ); 
				}
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for this object that creates the local database object.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="domainID">Domain to which this collection belongs.</param>
		internal LocalDatabase( Store storeObject, string domainID ) :
			base ( storeObject, "LocalDatabase", Guid.NewGuid().ToString(), NodeTypes.LocalDatabaseType, domainID )
		{
			// Set a sync role on this collection.
			Property p = new Property( PropertyTags.SyncRole, SyncRoles.Local );
			p.LocalProperty = true;
			properties.ModifyNodeProperty( p );
		}

		/// <summary>
		/// Constructor to create an existing LocalDatabase object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="node">Node object to construct this object from.</param>
		internal LocalDatabase( Store storeObject, Node node ) :
			base( storeObject, node )
		{
			if ( type != NodeTypes.LocalDatabaseType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.LocalDatabaseType, type ) );
			}
		}

		/// <summary>
		/// Constructor for creating an existing LocalDatabase object from a ShallowNode.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		internal LocalDatabase( Store storeObject, ShallowNode shallowNode ) :
			base( storeObject, shallowNode )
		{
			if ( type != NodeTypes.LocalDatabaseType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.LocalDatabaseType, type ) );
			}
		}

		/// <summary>
		/// Constructor to create an existing LocalDatabase object from an Xml document object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document object to construct this object from.</param>
		internal LocalDatabase( Store storeObject, XmlDocument document ) :
			base( storeObject, document )
		{
			if ( type != NodeTypes.LocalDatabaseType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.LocalDatabaseType, type ) );
			}
		}
		#endregion
	}
}
