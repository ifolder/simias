/*****************************************************************************
* Copyright © [2007-08] Unpublished Work of Novell, Inc. All Rights Reserved.
*
* THIS IS AN UNPUBLISHED WORK OF NOVELL, INC.  IT CONTAINS NOVELL'S CONFIDENTIAL, 
* PROPRIETARY, AND TRADE SECRET INFORMATION.	NOVELL RESTRICTS THIS WORK TO 
* NOVELL EMPLOYEES WHO NEED THE WORK TO PERFORM THEIR ASSIGNMENTS AND TO 
* THIRD PARTIES AUTHORIZED BY NOVELL IN WRITING.  THIS WORK MAY NOT BE USED, 
* COPIED, DISTRIBUTED, DISCLOSED, ADAPTED, PERFORMED, DISPLAYED, COLLECTED,
* COMPILED, OR LINKED WITHOUT NOVELL'S PRIOR WRITTEN CONSENT.  USE OR 
* EXPLOITATION OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE 
* PERPETRATOR TO CRIMINAL AND  CIVIL LIABILITY.
*
* Novell is the copyright owner of this file.  Novell may have released an earlier version of this
* file, also owned by Novell, under the GNU General Public License version 2 as part of Novell's 
* iFolder Project; however, Novell is not releasing this file under the GPL.
*
*-----------------------------------------------------------------------------
*
*                 Novell iFolder Enterprise
*
*-----------------------------------------------------------------------------
*
*                 $Author: Rob
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
			string type = e.GetType().ToString();
			attr = doc.CreateNode(XmlNodeType.Attribute,
				TypeAttributeName, SoapException.DetailElementName.Namespace);
			attr.Value = type;
			child.Attributes.SetNamedItem(attr);
			
			// exception message
			child.InnerText = e.Message;
						
			node.AppendChild(child);

			string message = String.Format("{0}-{1}", type, e.Message);

			throw new SoapException(message, SoapException.ServerFaultCode,
				null, node);
		}

		/// <summary>
		/// Throw a Smart SOAP Exception
		/// </summary>
		/// <param name="exceptions"></param>
		public static void Throw(Hashtable exceptions)
		{
			if (exceptions.Count < 1)
			{
				// do nothing
			}
			else if (exceptions.Count == 1)
			{
				SmartException.Throw((Exception)(new ArrayList(exceptions.Values))[0]);
			}
			else
			{
				XmlDocument doc = new XmlDocument();
				XmlNode node = doc.CreateNode(XmlNodeType.Element,
					SoapException.DetailElementName.Name,
					SoapException.DetailElementName.Namespace);
							
				XmlNode child;
				XmlNode attr;

				string message = null;

				foreach(string id in exceptions.Keys)
				{
					Exception e = (Exception)exceptions[id];

					// exception node
					child = doc.CreateNode(XmlNodeType.Element,
						OriginalExceptionElementName,
						SoapException.DetailElementName.Namespace);
								
					// exception type
					string type = e.GetType().ToString();
					attr = doc.CreateNode(XmlNodeType.Attribute,
						TypeAttributeName, SoapException.DetailElementName.Namespace);
					attr.Value = type;
					child.Attributes.SetNamedItem(attr);
				
					// message
					if (message == null)
					{
						message = String.Format("{0}-{1}", type, e.Message);
					}
					else
					{
						message = String.Format("{0}, {1}-{2}", message, type, e.Message);
					}

					// exception id
					attr = doc.CreateNode(XmlNodeType.Attribute,
						IDAttributeName, SoapException.DetailElementName.Namespace);
					attr.Value = id;
					child.Attributes.SetNamedItem(attr);
				
					// exception message
					child.InnerText = e.Message;
							
					node.AppendChild(child);
				}

				message = String.Format("{0} : {1}", ComplexExceptionMessage, message);
	
				throw new SoapException(message,
					SoapException.ServerFaultCode,
					null, node);
			}
		}

	}
}
