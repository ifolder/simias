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
using System.Collections;

using Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// Interface declaration for an enumerator that implements a dispose method.
	/// </summary>
	/// <remarks>
	/// The application should call Dispose() to free up system resources before releasing
	/// the reference to the ICSEnumerator.
	/// </remarks>
	public interface ICSEnumerator : IEnumerator, IDisposable
	{
		#region Properties
		/// <summary>
		/// Gets the total number of objects contained in the search.
		/// </summary>
		int Count { get; }
		#endregion

		#region Public Methods
		/// <summary>
		/// Set the cursor for the current search to the specified index.
		/// </summary>
		/// <param name="origin">The origin to move from.</param>
		/// <param name="offset">The offset to move the index by.</param>
		/// <returns>True if successful, otherwise false is returned.</returns>
		bool SetCursor( IndexOrigin origin, int offset );
		#endregion
	}

	/// <summary>
	/// Container object that encapsulates an ICSEnumerator.
	/// </summary>
	public class ICSList : IEnumerable
	{
		#region Class Members
		/// <summary>
		/// Array that will hold all of the multiple values.
		/// </summary>
		private ArrayList valueList;

		/// <summary>
		/// Enumerator used to enumerate list items.
		/// </summary>
		private ICSEnumerator iEnumerator;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the object.
		/// </summary>
		public ICSList()
		{
			this.valueList = new ArrayList();
			this.iEnumerator = null;
		}

		/// <summary>
		/// Constructor for the object.
		/// </summary>
		/// <param name="icsEnumerator">Enumerator that contains objects.</param>
		public ICSList( ICSEnumerator icsEnumerator )
		{
			this.valueList = null;
			this.iEnumerator = icsEnumerator;
		}

		/// <summary>
		/// Constructor for the object.
		/// </summary>
		/// <param name="list">ArrayList that contains objects.</param>
		public ICSList ( ArrayList list )
		{
			this.valueList = list;
			this.iEnumerator = null;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the total number of objects contained in the list.
		/// </summary>
		public int Count
		{
			get { return ( valueList != null ) ? valueList.Count : iEnumerator.Count; }
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Adds an object to the container.
		/// </summary>
		/// <param name="value">Adds a value to the container.</param>
		internal void Add( object value )
		{
			if ( valueList != null )
			{
				valueList.Add( value );
			}
			else
			{
				throw new InvalidOperationException( "Cannot add to this type of enumerator" );
			}
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that can iterate through the ICSList.
		/// </summary>
		/// <returns>An ICSEnumerator object.</returns>
		public IEnumerator GetEnumerator()
		{
			if ( valueList != null )
			{
				return new ICSListEnumerator( valueList );
			}
			else
			{
				return iEnumerator;
			}
		}
		#endregion

		/// <summary>
		/// Class used to implement the enumeration for the ICSList class.
		/// </summary>
		private class ICSListEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// List of items.
			/// </summary>
			private ArrayList list;

			/// <summary>
			/// Enumerator for the list.
			/// </summary>
			private int index = -1;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructs the object.
			/// </summary>
			/// <param name="list">Array list of ICSList object.</param>
			public ICSListEnumerator( ArrayList list )
			{
				this.list = list;
			}
			#endregion

			#region Properties
			/// <summary>
			/// Gets the total number of objects contained in the search.
			/// </summary>
			public int Count
			{
				get { return list.Count; }
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Sets the enumerator to its initial position, which is before
			/// the first element in the collection.
			/// </summary>
			public void Reset()
			{
				index = -1;
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get 
				{ 
					if ( ( index == -1 ) || ( index == Count ) )
					{
						throw new InvalidOperationException( "The enumerator is positioned before the first element of the collection or after the last element." );
					}

					return list[ index ];
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
				if ( index == Count )
				{
					return false;
				}
				else
				{
					return ( ++index < Count ) ? true : false;
				}
			}

			/// <summary>
			/// Set the cursor for the current search to the specified index.
			/// </summary>
			/// <param name="origin">The origin to move from.</param>
			/// <param name="offset">The offset to move the index by.</param>
			/// <returns>True if successful, otherwise false is returned.</returns>
			public bool SetCursor( IndexOrigin origin, int offset )
			{
				bool cursorSet = false;

				switch ( origin )
				{
					case IndexOrigin.CUR:
					{
						int newIndex = ( ( index == -1 ) ? 0 : index ) + offset;
						if ( ( newIndex >= 0 ) && ( newIndex < Count ) )
						{
							index = newIndex;
							cursorSet = true;
						}
						break;
					}

					case IndexOrigin.END:
					{
						int newIndex = Count + offset;
						if ( ( newIndex >= 0 ) && ( newIndex < Count ) )
						{
							index = newIndex;
							cursorSet = true;
						}
						break;
					}

					case IndexOrigin.SET:
					{
						if ( ( offset >= 0 ) && ( offset < Count ) )
						{
							index = offset;
							cursorSet = true;
						}
						break;
					}
				}

				return cursorSet;
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// This is declared here to satisfy the interface requirements, but the ICSListEnumerator
			/// does not use any unmanaged resources that it needs to dispose of.
			/// </summary>
			public void Dispose()
			{
			}
			#endregion
		}
	}
}
