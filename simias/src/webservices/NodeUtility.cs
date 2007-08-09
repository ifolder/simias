/****************************************************************************
|
| Copyright (c) 2007 Novell, Inc.
| All Rights Reserved.
|
| This program is free software; you can redistribute it and/or
| modify it under the terms of version 2 of the GNU General Public License as
| published by the Free Software Foundation.
|
| This program is distributed in the hope that it will be useful,
| but WITHOUT ANY WARRANTY; without even the implied warranty of
| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
| GNU General Public License for more details.
|
| You should have received a copy of the GNU General Public License
| along with this program; if not, contact Novell, Inc.
|
| To contact Novell about this file by physical or electronic mail,
| you may find current contact information at www.novell.com 
|
| Author: Rob 
|***************************************************************************/

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
