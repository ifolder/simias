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
using System.Xml;

using Simias;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a Node object without properties.
	/// </summary>
	public class ShallowNode : IComparable
	{
		#region Class Members
		/// <summary>
		/// The display name for the object.  This is the "friendly" name that applications will use.
		/// </summary>
		private readonly string name;

		/// <summary>
		/// The globally unique identifier for the object.
		/// </summary>
		private readonly string id;

		/// <summary>
		/// The object type.
		/// </summary>
		private readonly string type;

		/// <summary>
		/// The collection identifier.
		/// </summary>
		private readonly string cid;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the globally unique identifier for this object.
		/// </summary>
		public string ID
		{
			get { return id; }
		}

		/// <summary>
		/// Gets the name of this object.
		/// </summary>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Gets the base type (Collection or Node) for this object.
		/// </summary>
		public string Type
		{
			get { return type; }
		}

		/// <summary>
		/// Gets the collection identifier for this object.
		/// </summary>
		public string CollectionID
		{
			get { return cid; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for creating a ShallowNode object.
		/// </summary>
		/// <param name="xmlElement">Xml element that describes a Node object.</param>
		internal ShallowNode( XmlElement xmlElement )
		{
			name = xmlElement.GetAttribute( XmlTags.NameAttr );
			id = xmlElement.GetAttribute( XmlTags.IdAttr );
			type = xmlElement.GetAttribute( XmlTags.TypeAttr );
			cid = xmlElement.GetAttribute( XmlTags.CIdAttr );
		}

		/// <summary>
		/// Constructor for creating a ShallowNode object.
		/// </summary>
		/// <param name="xmlElement">Xml element that describes a Node object.</param>
		/// <param name="collectionID">The collection that this node belongs to.</param>
		internal ShallowNode( XmlElement xmlElement, string collectionID )
		{
			name = xmlElement.GetAttribute( XmlTags.NameAttr );
			id = xmlElement.GetAttribute( XmlTags.IdAttr );
			type = xmlElement.GetAttribute( XmlTags.TypeAttr );
			cid = collectionID;

			// TODO: Debug remove
			if ( ( cid == null ) || ( cid == String.Empty ) )
				throw new CollectionStoreException( "Shallow node has null collection id." );
			// TODO End Debug
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns whether specified Node object is the specified base type.
		/// </summary>
		/// <param name="typeString">Type of Node object.</param>
		/// <returns>True if Node object is the specified type, otherwise false is returned.</returns>
		public bool IsBaseType(string typeString )
		{
			return ( Type == typeString ? true : false );
		}
		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares this node by name to the specified node.
		/// </summary>
		/// <param name="node">Node object to compare.</param>
		/// <returns>Zero if objects are equal, less than zero if this object is 
		/// less than the specified object, greater than zero if this object is
		/// greater than the specified object.</returns>
		public int CompareTo( object node )
		{
			return Name.CompareTo( ( node as ShallowNode ).Name );
		}

		#endregion
	}
}
