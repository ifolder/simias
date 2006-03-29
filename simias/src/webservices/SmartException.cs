/***********************************************************************
 *  $RCSfile: SmartException.cs,v $
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
using System.Xml;
using System.Collections;
using System.Web.Services.Protocols;

using Simias;

namespace iFolder.WebService
{
	/// <summary>
	/// Smart Exception
	/// </summary>
	public class SmartException
	{
		private static readonly string OriginalExceptionElementName = "OriginalException";
		private static readonly string TypeAttributeName = "type";
		private static readonly string IDAttributeName = "id";
		private static readonly string ComplexExceptionMessage = "Complex Exception";

		/// <summary>
		/// Constructor
		/// </summary>
		private SmartException()
		{
		}

		/// <summary>
		/// Throw a Smart SOAP Exception
		/// </summary>
		/// <param name="e"></param>
		public static void Throw(Exception e)
		{
			XmlDocument doc = new XmlDocument();
			XmlNode node = doc.CreateNode(XmlNodeType.Element,
				SoapException.DetailElementName.Name,
				SoapException.DetailElementName.Namespace);
						
			XmlNode child;
			XmlNode attr;
			
			// exception node
			child = doc.CreateNode(XmlNodeType.Element,
				OriginalExceptionElementName,
				SoapException.DetailElementName.Namespace);
							
			// exception type
			attr = doc.CreateNode(XmlNodeType.Attribute,
				TypeAttributeName, SoapException.DetailElementName.Namespace);
			attr.Value = e.GetType().ToString();
			child.Attributes.SetNamedItem(attr);
			
			// exception message
			child.InnerText = e.Message;
						
			node.AppendChild(child);

			throw new SoapException(e.Message, SoapException.ServerFaultCode,
				null, node);
		}

		/// <summary>
		/// Throw a Smart SOAP Exception
		/// </summary>
		/// <param name="exceptions"></param>
		public static void Throw(Hashtable exceptions)
		{
			XmlDocument doc = new XmlDocument();
			XmlNode node = doc.CreateNode(XmlNodeType.Element,
				SoapException.DetailElementName.Name,
				SoapException.DetailElementName.Namespace);
						
			XmlNode child;
			XmlNode attr;

			foreach(string id in exceptions.Keys)
			{
				Exception e = (Exception)exceptions[id];

				// exception node
				child = doc.CreateNode(XmlNodeType.Element,
					OriginalExceptionElementName,
					SoapException.DetailElementName.Namespace);
							
				// exception type
				attr = doc.CreateNode(XmlNodeType.Attribute,
					TypeAttributeName, SoapException.DetailElementName.Namespace);
				attr.Value = e.GetType().ToString();
				child.Attributes.SetNamedItem(attr);
			
				// exception id
				attr = doc.CreateNode(XmlNodeType.Attribute,
					IDAttributeName, SoapException.DetailElementName.Namespace);
				attr.Value = id;
				child.Attributes.SetNamedItem(attr);
			
				// exception message
				child.InnerText = e.Message;
						
				node.AppendChild(child);
			}

			// only throw the exception if we have some
			if (exceptions.Keys.Count > 0)
			{
				throw new SoapException(ComplexExceptionMessage,
					SoapException.ServerFaultCode,
					null, node);
			}
		}

	}
}
