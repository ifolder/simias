/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// WebSettings
	/// </summary>
	public class WebSettings
	{
		/// <summary>
		/// Settings Name
		/// </summary>
		public static string Name = "WebAccessSettings";

		/// <summary>
		/// Page Size Setting Name
		/// </summary>
		public static string PageSizeSetting = "PageSize";

		/// <summary>
		/// Page Size Default
		/// </summary>
		public static int PageSizeDefault = 10;

		XmlDocument doc;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="web"></param>
		public WebSettings(iFolderWeb web)
		{
			string xml = web.GetSetting(Name);

			doc = new XmlDocument();
						
			try
			{
				doc.LoadXml(xml);
			}
			catch
			{
				// if the xml is not valid, reset the settings
			}

			// create the root element
			if (doc.FirstChild == null)
			{
				doc.AppendChild(doc.CreateElement(Name));
			}
		}

		/// <summary>
		/// Save Settings
		/// </summary>
		/// <param name="web"></param>
		public void Save(iFolderWeb web)
		{
			web.SetSetting(Name, doc.OuterXml);
		}

		/// <summary>
		/// Page Size
		/// </summary>
		public int PageSize
		{
			get { return GetIntValue(PageSizeSetting, PageSizeDefault); }
			set { SetIntValue(PageSizeSetting, value); }
		}

		/// <summary>
		/// Get Int Value
		/// </summary>
		/// <param name="name"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public int GetIntValue(string name, int defaultValue)
		{
			int result;

			string value = GetValue(name);

			try
			{
				result = int.Parse(value);
			}
			catch
			{
				// use default
				result = defaultValue;
			}

			return result;
		}

		/// <summary>
		/// Set Int Value
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetIntValue(string name, int value)
		{
			SetValue(name, value.ToString());
		}

		/// <summary>
		/// Get Value
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public string GetValue(string name)
		{
			string result = null;

			XmlNode node = doc.FirstChild.SelectSingleNode(name);

			if (node != null) result = node.InnerText;

			return result;
		}

		/// <summary>
		/// Set Value
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		public void SetValue(string name, string value)
		{
			XmlNode node = doc.FirstChild.SelectSingleNode(name);

			if (node == null)
			{
				node = doc.CreateElement(name);
				doc.FirstChild.AppendChild(node);
			}

			node.InnerText = value;
		}
	}
}
