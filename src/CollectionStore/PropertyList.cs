/***********************************************************************
 *  PropertyList.cs - Class that implements containment, access and 
 *  manipulation of properties.
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
using System.Text.RegularExpressions;
using System.Xml;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// Represents the list of properties on an object.  Properties maybe added, retrieved,
	/// enumerated, or deleted from this object.
	/// </summary>
	public class PropertyList : IEnumerable
	{
		#region Class Members
		/// <summary>
		/// The node that this PropertyList belongs to.
		/// </summary>
		private readonly Node node;

		/// <summary>
		/// DOM document containing the property list for this node.
		/// </summary>
		private XmlDocument nodeDocument = null;

		/// <summary>
		/// Xml element that all properties are subordinate to.
		/// </summary>
		private XmlElement propertyRoot = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the count of properties in the list not including the hidden properties.
		/// </summary>
		public int Count
		{
			get {  return InternalCount - HiddenCount; }
		}

		/// <summary>
		/// Returns the DOM representing this node.
		/// </summary>
		internal XmlDocument PropertyDocument
		{
			get { return nodeDocument; }
		}

		/// <summary>
		/// Gets the connection associated with this property list. 
		/// </summary>
		internal Collection CollectionNode
		{
			get { return node.CollectionNode; }
		}

		/// <summary>
		/// Gets the count of hidden properties in the list.
		/// </summary>
		private int HiddenCount
		{
			get
			{
				// Get the list of xml property nodes that have a flags attribute.
				MultiValuedList mvl = new MultiValuedList( propertyRoot, Property.Hidden );
				return mvl.Count;
			}
		}

		/// <summary>
		/// Gets the total number of properties in the list.  No filter is applied.
		/// </summary>
		private int InternalCount
		{
			get { return propertyRoot.ChildNodes.Count; }
		}

		/// <summary>
		/// Gets the parent element where the xml properties are stored.
		/// </summary>
		internal XmlElement PropertyRoot
		{
			get { return propertyRoot; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for the property object.
		/// </summary>
		/// <param name="node">The node that this PropertyList belongs to.</param>
		internal PropertyList( Node node )
		{
			// Save the node that this PropertyList belongs to.
			this.node = node;

			// Create an empty DOM document that will hold the properties.
			nodeDocument = new XmlDocument();
			XmlElement element = nodeDocument.CreateElement( Property.ObjectListTag );
			nodeDocument.AppendChild( element );

			// Set the node attributes in the XML document.
			propertyRoot = nodeDocument.CreateElement( Property.ObjectTag );
			node.SetNodeAttribute( propertyRoot, Property.NameAttr, node.Name );
			node.SetNodeAttribute( propertyRoot, Property.IDAttr, node.Id );
			node.SetNodeAttribute( propertyRoot, Property.TypeAttr, node.NameSpaceType );
			nodeDocument.DocumentElement.AppendChild( propertyRoot );
		}

		/// <summary>
		/// Constructor for the object where the XML document describing the properties is
		/// already available.
		/// </summary>
		/// <param name="node">The node that this PropertyList belongs to.</param>
		/// <param name="xmlProperties">An XML element where the properties for this node are rooted.</param>
		internal PropertyList( Node node, XmlElement xmlProperties )
		{
			this.node = node;
			this.nodeDocument = xmlProperties.OwnerDocument;
			this.propertyRoot = xmlProperties;
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Adds a property to the existing property list.
		/// </summary>
		/// <param name="property">Property to add to the property list.</param>
		internal void AddNodeProperty( Property property )
		{
			// Always add new properties to the end of the list.  Order must be maintained on property lists.
			if ( !nodeDocument.Equals( property.OwnerDocument ) )
			{
				property.XmlProperty = ( XmlElement )nodeDocument.ImportNode( property.XmlProperty, true );
				property.XmlPropertyList = this;
			}

			propertyRoot.AppendChild( property.XmlProperty );
			SetListChanged();
		}

		/// <summary>
		/// Adds an object type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Object representation of the property to add.</param>
		internal void AddNodeProperty( string name, object propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a string type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">String value of the property to add.</param>
		internal void AddNodeProperty( string name, string propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a sbyte type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Signed 8-bit value of the property to add.</param>
		internal void AddNodeProperty( string name, sbyte propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a byte type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Unsigned 8-bit value of the property to add.</param>
		internal void AddNodeProperty( string name, byte propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a short type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Signed 16-bit value of the property to add.</param>
		internal void AddNodeProperty( string name, short propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a ushort type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Unsigned 16-bit value of the property to add.</param>
		internal void AddNodeProperty( string name, ushort propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds an int type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Signed 32-bit value of the property to add.</param>
		internal void AddNodeProperty( string name, int propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a uint type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Unsigned 32-bit value of the property to add.</param>
		internal void AddNodeProperty( string name, uint propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a long type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Signed 64-bit value of the property to add.</param>
		internal void AddNodeProperty( string name, long propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds an ulong type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Unsigned 64-bit value of the property to add.</param>
		internal void AddNodeProperty( string name, ulong propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a char type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Unicode character value of the property to add.</param>
		internal void AddNodeProperty( string name, char propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a float type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Single precision 32-bit value of the property to add.</param>
		internal void AddNodeProperty( string name, float propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a bool type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Boolean value of the property to add.</param>
		internal void AddNodeProperty( string name, bool propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a DateTime type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">DateTime value of the property to add.</param>
		internal void AddNodeProperty( string name, DateTime propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a Uri type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Uri value of the property to add.</param>
		internal void AddNodeProperty( string name, Uri propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds an XmlDocument type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">XmlDocument value of the property to add.</param>
		internal void AddNodeProperty( string name, XmlDocument propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a TimeSpan type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">TimeSpan value of the property to add.</param>
		internal void AddNodeProperty( string name, TimeSpan propertyValue )
		{
			AddNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Searches for a specified property name.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <returns>An property object.</returns>
		internal Property FindSingleValue( string name )
		{
			// Could use an XPath search here, but we don't want to deal with case sensitive issues and
			// don't think that XPath's translate() would be all that efficient.
			Property property = null;

			// Create a regular expression to use as the search string.
			Regex searchName = new Regex( "^" + name + "$", RegexOptions.IgnoreCase );

			// Walk each property node and do a case-insensitive compare on the names.
			foreach ( XmlElement x in propertyRoot )
			{
				if ( searchName.IsMatch( x.GetAttribute( Property.NameAttr ) ) )
				{
					property = new Property( this, x );
					break;
				}
			}

			return property;
		}

		/// <summary>
		/// Searches for a specified property name and returns a list of values. Does not return hidden values.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <returns>A MultiValuedList object containing the values for the property.</returns>
		internal MultiValuedList FindValues( string name )
		{
			return FindValues( name, false );
		}

		/// <summary>
		/// Searches for a specified property name and returns a list of values.
		/// </summary>
		/// <param name="name">Name of the property.</param>
		/// <param name="hiddenProperties">If true, return hidden properties.</param>
		/// <returns>A MultiValuedList object containing the values for the property.</returns>
		internal MultiValuedList FindValues( string name, bool hiddenProperties )
		{
			return new MultiValuedList( this, name, hiddenProperties );
		}

		/// <summary>
		/// Modifies the first matching property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="property">Property to modify.</param>
		internal void ModifyNodeProperty( Property property )
		{
			// Find an existing value.
			Property existingProperty = FindSingleValue( property.Name );
			if ( existingProperty != null )
			{
				// Just replace the value in the existing XML node.
				existingProperty.SetPropertyValue( property );
			}
			else
			{
				// The property doesn't exist.  It needs to be created.
				AddNodeProperty( property );
			}
		}

		/// <summary>
		/// Modifies the first matching property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the property to modify.</param>
		/// <param name="propertyValue">New property value.</param>
		internal void ModifyNodeProperty( string name, object propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching string property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the string property to modify.</param>
		/// <param name="propertyValue">New string property value.</param>
		internal void ModifyNodeProperty( string name, string propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching sbyte property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the sbyte property to modify.</param>
		/// <param name="propertyValue">New sbyte property value.</param>
		internal void ModifyNodeProperty( string name, sbyte propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching byte property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the byte property to modify.</param>
		/// <param name="propertyValue">New byte property value.</param>
		internal void ModifyNodeProperty( string name, byte propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching short property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the short property to modify.</param>
		/// <param name="propertyValue">New short property value.</param>
		internal void ModifyNodeProperty( string name, short propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching ushort property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the ushort property to modify.</param>
		/// <param name="propertyValue">New ushort property value.</param>
		internal void ModifyNodeProperty( string name, ushort propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching int property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the int property to modify.</param>
		/// <param name="propertyValue">New int property value.</param>
		internal void ModifyNodeProperty( string name, int propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching uint property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the uint property to modify.</param>
		/// <param name="propertyValue">New uint property value.</param>
		internal void ModifyNodeProperty( string name, uint propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching long property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the long property to modify.</param>
		/// <param name="propertyValue">New long property value.</param>
		internal void ModifyNodeProperty( string name, long propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching ulong property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the ulong property to modify.</param>
		/// <param name="propertyValue">New ulong property value.</param>
		internal void ModifyNodeProperty( string name, ulong propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching char property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the char property to modify.</param>
		/// <param name="propertyValue">New char property value.</param>
		internal void ModifyNodeProperty( string name, char propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching float property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the float property to modify.</param>
		/// <param name="propertyValue">New float property value.</param>
		internal void ModifyNodeProperty( string name, float propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching bool property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the bool property to modify.</param>
		/// <param name="propertyValue">New bool property value.</param>
		internal void ModifyNodeProperty( string name, bool propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching DateTime property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the DateTime property to modify.</param>
		/// <param name="propertyValue">New DateTime property value.</param>
		internal void ModifyNodeProperty( string name, DateTime propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching Uri property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the Uri property to modify.</param>
		/// <param name="propertyValue">New Uri property value.</param>
		internal void ModifyNodeProperty( string name, Uri propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching XmlDocument property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the XmlDocument property to modify.</param>
		/// <param name="propertyValue">New XmlDocument property value.</param>
		internal void ModifyNodeProperty( string name, XmlDocument propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching TimeSpan property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the TimeSpan property to modify.</param>
		/// <param name="propertyValue">New TimeSpan property value.</param>
		internal void ModifyNodeProperty( string name, TimeSpan propertyValue )
		{
			ModifyNodeProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Indicates that the property list has changed and its node needs to be added to the dirty list.  If the node
		/// does not belong to a collection, don't add it to the list.
		/// </summary>
		internal void SetListChanged()
		{
			// Add this node to the dirty list.
			node.CollectionNode.AddDirtyNodeToList( node );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a property to the existing property list.
		/// </summary>
		/// <param name="property">Property to add.</param>
		public void AddProperty( Property property )
		{
			// Check to see if the property being set is a system property.
			if ( !property.IsSystemProperty() )
			{
				// Make sure that current user has write rights to this collection.
				AddNodeProperty( property );
			}
			else
			{
				throw new ApplicationException( "Cannot set reserved property" );
			}
		}

		/// <summary>
		/// Adds an object type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Object representation of the property to add.</param>
		public void AddProperty( string name, object propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a string type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">String value of the property to add.</param>
		public void AddProperty( string name, string propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a sbyte type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Signed 8-bit value of the property to add.</param>
		public void AddProperty( string name, sbyte propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a byte type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Unsigned 8-bit value of the property to add.</param>
		public void AddProperty( string name, byte propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a short type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Signed 16-bit value of the property to add.</param>
		public void AddProperty( string name, short propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a ushort type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Unsigned 16-bit value of the property to add.</param>
		public void AddProperty( string name, ushort propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds an int type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Signed 32-bit value of the property to add.</param>
		public void AddProperty( string name, int propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a uint type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Unsigned 32-bit value of the property to add.</param>
		public void AddProperty( string name, uint propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a long type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Signed 64-bit value of the property to add.</param>
		public void AddProperty( string name, long propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds an ulong type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Unsigned 64-bit value of the property to add.</param>
		public void AddProperty( string name, ulong propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a char type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Unicode character value of the property to add.</param>
		public void AddProperty( string name, char propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a float type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Single precision 32-bit value of the property to add.</param>
		public void AddProperty( string name, float propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a bool type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Boolean value of the property to add.</param>
		public void AddProperty( string name, bool propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a DateTime type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">DateTime value of the property to add.</param>
		public void AddProperty( string name, DateTime propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a Uri type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">Uri value of the property to add.</param>
		public void AddProperty( string name, Uri propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds an XmlDocument type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">XmlDocument value of the property to add.</param>
		public void AddProperty( string name, XmlDocument propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Adds a TimeSpan type property to the existing property list.
		/// </summary>
		/// <param name="name">Name of the property to add.</param>
		/// <param name="propertyValue">TimeSpan value of the property to add.</param>
		public void AddProperty( string name, TimeSpan propertyValue )
		{
			AddProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Deletes the all occurances of the specified property from the property list.
		/// </summary>
		/// <param name="name">Name of property to delete.</param>
		public void DeleteProperties( string name )
		{
			// Find all of the existing values.
			MultiValuedList mvp = FindValues( name );
			foreach ( Property p in mvp )
			{
				p.Delete();
			}
		}

		/// <summary>
		/// Deletes the first occurance of the specified property from the property list.
		/// </summary>
		/// <param name="name">Name of property to delete.</param>
		public void DeleteSingleProperty( string name )
		{
			// Find the first existing value.
			Property existingProperty = FindSingleValue( name );
			if ( existingProperty != null )
			{
				// Remove this property from the node.
				existingProperty.Delete();
			}
		}

		/// <summary>
		/// Deletes the first occurance of the specified property from the property list.
		/// </summary>
		/// <param name="property">Property to delete from the list.</param>
		public void DeleteSingleProperty( Property property )
		{
			DeleteSingleProperty( property.Name );
		}

		/// <summary>
		/// Gets the first occurance of the specified property.
		/// </summary>
		/// <param name="name">Name of the property to retreive.</param>
		/// <returns>A property object containing the value of the property if it exists. Otherwise a null is returned.</returns>
		public Property GetSingleProperty( string name )
		{
			// Get the property.
			Property p = FindSingleValue( name );
			if ( ( p != null ) && ( p.HiddenProperty == false ) )
			{
				return p;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Gets all occurances of the specified property name.
		/// </summary>
		/// <param name="name">Name of the properties to retrieve.</param>
		/// <returns>A MultiValuedList object that contains all of the values for the specified property.</returns>
		public MultiValuedList GetProperties( string name )
		{
			return FindValues( name );
		}

		/// <summary>
		/// Modifies the first matching property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="property">Property to modify.</param>
		public void ModifyProperty( Property property )
		{
			if ( !property.IsSystemProperty() )
			{
				ModifyNodeProperty( property );
			}
			else
			{
				throw new ApplicationException( "Cannot modify a system property" );
			}
		}

		/// <summary>
		/// Modifies the first matching property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the property to modify.</param>
		/// <param name="propertyValue">New property value.</param>
		public void ModifyProperty( string name, object propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching string property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the string property to modify.</param>
		/// <param name="propertyValue">New string property value.</param>
		public void ModifyProperty( string name, string propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching sbyte property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the sbyte property to modify.</param>
		/// <param name="propertyValue">New sbyte property value.</param>
		public void ModifyProperty( string name, sbyte propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching byte property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the byte property to modify.</param>
		/// <param name="propertyValue">New byte property value.</param>
		public void ModifyProperty( string name, byte propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching short property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the short property to modify.</param>
		/// <param name="propertyValue">New short property value.</param>
		public void ModifyProperty( string name, short propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching ushort property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the ushort property to modify.</param>
		/// <param name="propertyValue">New ushort property value.</param>
		public void ModifyProperty( string name, ushort propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching int property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the int property to modify.</param>
		/// <param name="propertyValue">New int property value.</param>
		public void ModifyProperty( string name, int propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching uint property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the uint property to modify.</param>
		/// <param name="propertyValue">New uint property value.</param>
		public void ModifyProperty( string name, uint propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching long property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the long property to modify.</param>
		/// <param name="propertyValue">New long property value.</param>
		public void ModifyProperty( string name, long propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching ulong property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the ulong property to modify.</param>
		/// <param name="propertyValue">New ulong property value.</param>
		public void ModifyProperty( string name, ulong propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching char property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the char property to modify.</param>
		/// <param name="propertyValue">New char property value.</param>
		public void ModifyProperty( string name, char propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching float property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the float property to modify.</param>
		/// <param name="propertyValue">New float property value.</param>
		public void ModifyProperty( string name, float propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching bool property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the bool property to modify.</param>
		/// <param name="propertyValue">New bool property value.</param>
		public void ModifyProperty( string name, bool propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching DateTime property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the DateTime property to modify.</param>
		/// <param name="propertyValue">New DateTime property value.</param>
		public void ModifyProperty( string name, DateTime propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching Uri property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the Uri property to modify.</param>
		/// <param name="propertyValue">New Uri property value.</param>
		public void ModifyProperty( string name, Uri propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching XmlDocument property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the XmlDocument property to modify.</param>
		/// <param name="propertyValue">New XmlDocument property value.</param>
		public void ModifyProperty( string name, XmlDocument propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}

		/// <summary>
		/// Modifies the first matching TimeSpan property in the list.  If the property doesn't
		/// exist, it is created.
		/// </summary>
		/// <param name="name">Name of the TimeSpan property to modify.</param>
		/// <param name="propertyValue">New TimeSpan property value.</param>
		public void ModifyProperty( string name, TimeSpan propertyValue )
		{
			ModifyProperty( new Property( name, propertyValue ) );
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Method used by clients to enumerate the properties in the list.
		/// </summary>
		/// <remarks>
		/// The client must call Dispose() to free up system resources before releasing
		/// the reference to the ICSEnumerator.
		/// </remarks>
		/// <returns>A property object that can enumerate the property list.</returns>
		public IEnumerator GetEnumerator()
		{
			return new PropertyEnumerator( this, propertyRoot );
		}

		/// <summary>
		/// Enumerator class for the PropertyList object that allows enumeration of property objects
		/// within a node.
		/// </summary>
		private class PropertyEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// The enumerator that we will use to enumerate the DOM tree.
			/// </summary>
			private IEnumerator propertyEnumerator;

			/// <summary>
			/// The property list where the enumeration is being performed.
			/// </summary>
			private PropertyList propertyList;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor used to instaniate this object by means of an enumerator.
			/// </summary>
			/// <param name="propertyList">The property list object where the enumeration is being performed.</param>
			/// <param name="xmlProperties">XML element that contains the properties for a node.</param>
			internal PropertyEnumerator( PropertyList propertyList, XmlElement xmlProperties )
			{
				this.propertyList = propertyList;
				propertyEnumerator = xmlProperties.GetEnumerator();
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Sets the enumerator to its initial position, which is before
			/// the first element in the collection.
			/// </summary>
			public void Reset()
			{
				propertyEnumerator.Reset();
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get
				{
					return new Property( propertyList, ( XmlElement )propertyEnumerator.Current );
				}
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; 
			/// false if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				bool moreData = propertyEnumerator.MoveNext();
				while ( moreData )
				{
					// See if this is a property that is not supposed to be returned.
					if ( ( ( Property )Current ).HiddenProperty )
					{
						moreData = propertyEnumerator.MoveNext();
					}
					else
					{
						break;
					}
				}

				return moreData;
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// This is declared here to satisfy the interface requirements, but the PropertyEnumerator
			/// does not use any unmanaged resources that it needs to dispose of.
			/// </summary>
			public void Dispose()
			{
			}
			#endregion
		}
		#endregion
	}
}
