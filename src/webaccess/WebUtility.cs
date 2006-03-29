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
using System.IO;
using System.Xml;
using System.Net;
using System.Web.Services.Protocols;
using System.Resources;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// iFolder Web Utilities
	/// </summary>
	public class WebUtility
	{
		/// <summary>
		/// Private Constructor
		/// </summary>
		private WebUtility()
		{
		}

		/// <summary>
		/// Get the File Name with the given Path
		/// </summary>
		/// <param name="path">The File Path String</param>
		/// <returns>The File Name</returns>
		public static string GetFileName(String path)
        {
            string result = Path.GetFileName(path);

            // KLUDGE: this is a kludge for Mono because it no longer
			// (after 1.1.6) recognizes the backslash as a directory
			// seperator
            int index = result.LastIndexOf('\\');
            if (index != -1) result = result.Substring(index + 1);

            return result;
        }

		/// <summary>
		/// Get the SmartException Class Type
		/// </summary>
		/// <param name="e">The SoapException with Detail.</param>
		/// <returns>The Class Type</returns>
		public static string GetSmartExceptionType(SoapException e)
		{
			// exception type
			string type = e.GetType().Name;

			try
			{
				XmlNode node = e.Detail.SelectSingleNode("OriginalException");

				if (node == null)
				{
					// try inside the <detail> tags
					node = e.Detail.SelectSingleNode("*/OriginalException");
				}

				if (node != null)
				{
					type = node.Attributes.GetNamedItem("type").Value;
					type = type.Substring(type.LastIndexOf(".") + 1);
				}
			}
			catch { }

			return type;
		}

		/// <summary>
		/// Get the WebException Class Type
		/// </summary>
		/// <param name="e">The WebException with Status Detail.</param>
		/// <returns>The Class Type</returns>
		public static string GetWebExceptionType(WebException e)
		{
			// exception type
			string type = e.Status.ToString();

			if (e.Status == WebExceptionStatus.ProtocolError)
			{
				HttpWebResponse response = e.Response as HttpWebResponse;

				type = response.StatusDescription;
			}

			return type;
		}
		
		/// <summary>
		/// Size Format Index
		/// </summary>
		private enum Index 
		{
			B = 0,
			KB = 1,
			MB = 2,
			GB = 3
		}

		/// <summary>
		/// Format Size
		/// </summary>
		/// <param name="size"></param>
		/// <param name="rm"></param>
		/// <returns></returns>
		public static string FormatSize(long size, ResourceManager rm)
		{
			const int K = 1024;

			string modifier = "";

			long temp;
			int index = 0;

			// adjust
			while((index < (int)Index.GB) && ((temp = (size / K)) > 1))
			{
				++index;
				size = temp;
			}

			// modifier
			switch((Index)index)
			{
					// B
				case Index.B:
					modifier = rm.GetString("ENTRY.B");
					break;

					// KB
				case Index.KB:
					modifier = rm.GetString("ENTRY.KB");
					break;

					// MB
				case Index.MB:
					modifier = rm.GetString("ENTRY.MB");
					break;

					// GB
				case Index.GB:
					modifier = rm.GetString("ENTRY.GB");
					break;
			}

			return String.Format("{0:N0}&nbsp;{1}", size, modifier);
		}

		/// <summary>
		/// Format Rights
		/// </summary>
		/// <param name="rights"></param>
		/// <param name="rm"></param>
		/// <returns></returns>
		public static string FormatRights(Rights rights, ResourceManager rm)
		{
			string result;

			switch(rights)
			{
					// Admin
				case Rights.Admin:
					result = rm.GetString("RIGHTS.ADMIN");
					break;

					// Read Only
				case Rights.ReadOnly:
					result = rm.GetString("RIGHTS.READONLY");
					break;

					// Read Write
				case Rights.ReadWrite:
					result = rm.GetString("RIGHTS.READWRITE");
					break;

					// Deny
				case Rights.Deny:
				default:
					result = rm.GetString("RIGHTS.DENY");
					break;
			}

			return result;
		}

		/// <summary>
		/// Format Date
		/// </summary>
		/// <param name="date"></param>
		/// <param name="rm"></param>
		/// <returns></returns>
		public static string FormatDate(DateTime date, ResourceManager rm)
		{
			string result = date.ToString("d MMM yyyy");

			DateTime today = DateTime.Today;

			if (date.Year == today.Year)
			{
				result = date.ToString("d MMM");

				if (date.Month == today.Month)
				{
					if (date.Day == today.Day)
					{
						result = rm.GetString("TODAY");
					}
					else if (date.Day == today.AddDays(-1).Day)
					{
						result = rm.GetString("YESTERDAY");
					}
				}
			}

			return result.Replace(" ", "&nbsp;");
		}

		/// <summary>
		/// Format Date and Time
		/// </summary>
		/// <param name="date"></param>
		/// <param name="rm"></param>
		/// <returns></returns>
		public static string FormatDateTime(DateTime date, ResourceManager rm)
		{
			string result = String.Format("{0} @ {1}",
				WebUtility.FormatDate(date, rm),
				date.ToString("h:mm tt"));

			return result.Replace(" ", "&nbsp;");
		}

		/// <summary>
		/// Format Change Type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="rm"></param>
		/// <returns></returns>
		public static string FormatChangeType(ChangeType type, ResourceManager rm)
		{
			string result;

			switch(type)
			{
				case ChangeType.Modify:
					result = rm.GetString("CHANGE.MODIFY");
					break;

				case ChangeType.Add:
					result = rm.GetString("CHANGE.ADD");
					break;

				case ChangeType.Delete:
					result = rm.GetString("CHANGE.DELETE");
					break;

				case ChangeType.Unknown:
				default:
					result = rm.GetString("CHANGE.UNKNOWN");
					break;
			}

			return result;
		}
	}
}
