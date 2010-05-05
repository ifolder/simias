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
using System.IO;
using System.Xml;
using System.Net;
using System.Web.Services.Protocols;
using System.Globalization;
using System.Resources;
using System.Threading;

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
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <param name="rm"></param>
		/// <returns></returns>
		public static string GetString(string key, ResourceManager rm)
		{
			string result = null;
			
			if (rm != null)
			{
				result = rm.GetString(key);
			}

			if ((result == null) || (result.Length == 0))
			{
				result = key;
			}

			return result;
		}

		/// <summary>
		/// Get the File Name with the given Path
		/// </summary>
		/// <param name="path">The File Path String</param>
		/// <returns>The File Name</returns>
		public static string GetFileName(String path)
		{
			string result = null;
			
			if (path != null)
			{
				result = Path.GetFileName(path);

				// KLUDGE: this is a kludge for Mono because it no longer
				// (after 1.1.6) recognizes the backslash as a directory
				// seperator
				int index = result.LastIndexOf('\\');
				if (index != -1) result = result.Substring(index + 1);
			}

			return result;
		}


		/// <summary>
		/// Get the Exception Type
		/// </summary>
		/// <param name="e">The Exception</param>
		/// <returns>The Exception Type</returns>
		public static string GetExceptionType(Exception e)
		{
			string type = e.GetType().Name;

			if (e is SoapException)
			{
				type = WebUtility.GetSmartExceptionType(e as SoapException);
			}
			else if (e is WebException)
			{
				type = WebUtility.GetWebExceptionType(e as WebException);	
			}

			return type;
		}

		/// <summary>
		/// Get the SmartException Type
		/// </summary>
		/// <param name="e">The SoapException with Detail.</param>
		/// <returns>The Exception Type</returns>
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
		/// Get the WebException Type
		/// </summary>
		/// <param name="e">The WebException with Status Detail.</param>
		/// <returns>The Exception Type</returns>
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

			double temp;
			double tempsize = (double) size;
			int index = 0;

			// adjust
			while((index < (int)Index.GB) && ((temp = ((double)tempsize / (double)K)) > 1))
			{
				++index;
				tempsize = temp;
			}
			// modifier
			switch((Index)index)
			{
					// B
				case Index.B:
					modifier = WebUtility.GetString("ENTRY.B", rm);
					break;

					// KB
				case Index.KB:
					modifier = WebUtility.GetString("ENTRY.KB", rm);
					break;

					// MB
				case Index.MB:
					modifier = WebUtility.GetString("ENTRY.MB", rm);
					break;

					// GB
				case Index.GB:
					modifier = WebUtility.GetString("ENTRY.GB", rm);
					break;
			}

			return String.Format("{0:N1}&nbsp;{1}", tempsize, modifier);
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
					result = WebUtility.GetString("RIGHTS.ADMIN", rm);
					break;

					// Read Only
				case Rights.ReadOnly:
					result = WebUtility.GetString("RIGHTS.READONLY", rm);
					break;

					// Read Write
				case Rights.ReadWrite:
					result = WebUtility.GetString("RIGHTS.READWRITE", rm);
					break;

					// Deny
				case Rights.Deny:
				default:
					result = WebUtility.GetString("RIGHTS.DENY", rm);
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
			string code = Thread.CurrentThread.CurrentUICulture.Name;
			CultureInfo ci = new CultureInfo(code);

			string result = date.ToString("d MMM yyyy");

			DateTime today = DateTime.Today;
			if (date.Year == today.Year)
			{
				result = date.ToString("d MMM");

				if (date.Month == today.Month)
				{
					if (date.Day == today.Day)
					{
						result = WebUtility.GetString("TODAY", rm);
					}
					else if (date.Day == today.AddDays(-1).Day)
					{
						result = WebUtility.GetString("YESTERDAY", rm);
					}
				}
				else
				{
					result = date.ToString("d MMM ", ci);
				}
			}
			else
			{
				result = date.ToString("d MMM yyyy", ci);
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
		/// Format Yes/No
		/// </summary>
		/// <param name="value"></param>
		/// <param name="rm"></param>
		/// <returns></returns>
		public static string FormatYesNo(bool value, ResourceManager rm)
		{
			return value ? WebUtility.GetString("YES", rm) : WebUtility.GetString("NO", rm);
		}

		/// <summary>
		/// Format Change Type
		/// </summary>
		/// <param name="action"></param>
		/// <param name="rm"></param>
		/// <returns></returns>
		public static string FormatChangeAction(ChangeEntryAction action, ResourceManager rm)
		{
			string result;

			switch(action)
			{
				case ChangeEntryAction.Modify:
					result = WebUtility.GetString("CHANGE.MODIFY", rm);
					break;

				case ChangeEntryAction.Add:
					result = WebUtility.GetString("CHANGE.ADD", rm);
					break;

				case ChangeEntryAction.Delete:
					result = WebUtility.GetString("CHANGE.DELETE", rm);
					break;

				case ChangeEntryAction.Unknown:
				default:
					result = WebUtility.GetString("CHANGE.UNKNOWN", rm);
					break;
			}

			return result;
		}
	}
}
