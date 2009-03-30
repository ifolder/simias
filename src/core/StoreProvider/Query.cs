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
using Simias.Storage;

namespace Simias.Storage.Provider
{
	/// <summary>
	/// Represents a query into a Collection Store Provider.
	/// </summary>
	[Serializable]
	public class Query
	{
		string collectionId;
		string property;
		SearchOp operation;
		string value;
		Syntax type;
		
		/// <summary>
		/// Construct a Query object that can be used to perform a search.
		/// </summary>
		/// <param name="collectionId">The Collection to perform the search in.</param>
		/// <param name="property">The property that contains the value.</param>
		/// <param name="op">The operator used for the match criteria.</param>
		/// <param name="value">The value to match.</param>
		/// <param name="type">The Type of the value to search for.</param>
		public Query(string collectionId, string property, SearchOp op, string value, Syntax type)
		{
			this.collectionId = collectionId;
			this.property = property;
			this.operation = op;
			this.type = type;

			if (type == Syntax.Boolean)
			{
				this.value = 
					( String.Compare(value, "true", true) == 0 || 
					  String.Compare(value, "1", false) == 0) ? "1" : "0";
			}
			else
			{
				this.value = value;
			}
		}

		/// <summary>
		/// Construct a Query that can be used to perform a global query.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="op"></param>
		/// <param name="value"></param>
		/// <param name="type"></param>
		public Query(string property, SearchOp op, string value, Syntax type) :
			this(null, property, op, value, type)
		{
		}

		/// <summary>
		/// Property to get the collectionId.
		/// </summary>
		public string CollectionId
		{
			get
			{
				return collectionId;
			}
		}

		/// <summary>
		/// Read only property to get the Store property name for this query.
		/// </summary>
		public string Property
		{
			get
			{
				return property;
			}
		}

		/// <summary>
		/// Read only property to get the query operation.
		/// </summary>
		public SearchOp Operation
		{
			get
			{
				return operation;
			}
		}

		/// <summary>
		/// Read only property that gets the string value for this query.
		/// </summary>
		public string Value
		{
			get
			{
				return value;
			}
		}

		/// <summary>
		/// Read only property that gets the value type.
		/// </summary>
		public Syntax Type
		{
			get
			{
				return type;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("In {0} Collection, Return all Records with {1} {2} {3}", 
				collectionId != null ? collectionId : "any",
				property,
				operation.ToString(),
				value);

		}

	}
}
