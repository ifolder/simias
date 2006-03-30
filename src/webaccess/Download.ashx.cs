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
using System.Web;
using System.Web.SessionState;
using System.Net;
using System.Resources;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Download
	/// </summary>
	public class Download : IHttpHandler, IRequiresSessionState
	{
		/// <summary>
		/// File Transfer Buffer Size
		/// </summary>
		private const int BUFFERSIZE = (16 * 1024);

		/// <summary>
		/// Constructor
		/// </summary>
		public Download()
		{
		}

		#region IHttpHandler Members

		/// <summary>
		/// Process the Request
		/// </summary>
		/// <param name="context">The HttpContext object</param>
		public void ProcessRequest(HttpContext context)
		{
			// query
			string ifolderID = context.Request.QueryString["iFolder"];
			string entryID = context.Request.QueryString["Entry"];
			string parentID = context.Request.QueryString["Parent"];
			
			try
			{
				// connection
				iFolderWeb web = (iFolderWeb)context.Session["Connection"];
				if (web == null) context.Response.Redirect("Login.aspx");

				// request
				UriBuilder uri = new UriBuilder(web.Url);
						
				uri.Path = String.Format("/simias10/Download.ashx?iFolder={0}&Entry={1}", ifolderID, entryID);

				HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(uri.Uri);
				webRequest.Method = "GET";
				webRequest.PreAuthenticate = true;
				webRequest.Credentials = web.Credentials;
				webRequest.CookieContainer = web.CookieContainer;

				HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

				Stream webStream = webResponse.GetResponseStream();

				// filename
				string filename = webResponse.Headers["Content-Disposition"];
				filename = filename.Substring(filename.IndexOf('=') + 1);

				// response
				context.Response.Clear();
				context.Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", filename));
				context.Response.AddHeader("Content-Length", webResponse.ContentLength.ToString());
				context.Response.ContentType = "application/octet-stream";
				context.Response.BufferOutput = false;

				try
				{
					Stream output = context.Response.OutputStream;

					byte[] buffer = new byte[BUFFERSIZE];
					int count = 0;

					while((count = webStream.Read(buffer, 0, BUFFERSIZE)) > 0)
					{
						output.Write(buffer, 0, count);
						output.Flush();
					}
				}
				finally
				{
					webStream.Close();
				}
			}
			catch
			{
				ResourceManager rm = (ResourceManager) context.Application["RM"];

				context.Server.Transfer(String.Format(
					"Entries.aspx?iFolder={0}&Entry={1}&Message={2}",
					ifolderID, parentID,
					context.Server.UrlEncode(WebUtility.GetString("ENTRY.FAILEDDOWNLOAD", rm))));
			}
		}

		/// <summary>
		/// Is this instance reusable?
		/// </summary>
		public bool IsReusable
		{
			get { return false; }
		}

		#endregion
	}
}
