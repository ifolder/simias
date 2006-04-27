/***********************************************************************
 *  $RCSfile: NodeUtility.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;

using Simias.Storage;

namespace iFolder.WebService
{
	/// <summary>
	/// Node Utility
	/// </summary>
	public class NodeUtility
	{
		private NodeUtility()
		{
		}

		/// <summary>
		/// Get a string property from a Node.
		/// </summary>
		/// <param name="node">The node object.</param>
		/// <param name="property">The property name.</param>
		/// <returns></returns>
		public static string GetStringProperty(Node node, string property)
		{
			string result = null;

			Property p = node.Properties.GetSingleProperty(property);

			if ((p != null) && (p.Type == Syntax.String))
			{
				result = (string)p.Value;
			}

			return result;
		}

		/// <summary>
		/// Get a DateTime property from a Node.
		/// </summary>
		/// <param name="node">The node object.</param>
		/// <param name="property">The property name.</param>
		/// <returns></returns>
		public static DateTime GetDateTimeProperty(Node node, string property)
		{
			DateTime result = DateTime.MinValue;

			Property p = node.Properties.GetSingleProperty(property);

			if ((p != null) && (p.Type == Syntax.DateTime))
			{
				result = (DateTime)p.Value;
			}

			return result;
		}
		
		/// <summary>
		/// Get a boolean property from a Node.
		/// </summary>
		/// <param name="node">The node object.</param>
		/// <param name="property">The property name.</param>
		/// <returns>true/false</returns>
		public static bool GetBooleanProperty(Node node, string property)
		{
			Property p = node.Properties.GetSingleProperty( property );
			if ( ( p != null ) && ( p.Type == Syntax.Boolean ) )
			{
				return (bool) p.Value;
			}

			return false;
		}
	}
}
