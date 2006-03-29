/***********************************************************************
 *  $RCSfile: Download.ashx.cs.cs,v $
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
using System.IO;
using System.Web;
using System.Net;

using Simias.Client;
using Simias.Storage;

namespace iFolder.WebService
{
	/// <summary>
	/// File Download Handler
	/// </summary>
	public class DownloadHandler : FileHandler
	{
		const int BUFFERSIZE = (16 * 1024);

		/// <summary>
		/// Constructor
		/// </summary>
		public DownloadHandler() : base()
		{
		}

		#region IHttpHandler Members

		/// <summary>
		/// Process the Request
		/// </summary>
		/// <param name="context">The HttpContext object.</param>
		public override void ProcessRequest(HttpContext context)
		{
			try
			{
				// initialize
				Initialize(context);

				// lock the file
				FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			
				try
				{
					// response
					context.Response.Clear();
					context.Response.AddHeader("Content-Disposition",
						String.Format("attachment; filename=\"{0}\"",
						HttpUtility.UrlEncode(filename, System.Text.Encoding.UTF8)));
					context.Response.AddHeader("Content-Length", stream.Length.ToString());
					context.Response.ContentType = "application/octet-stream";
					context.Response.BufferOutput = false;
					
					Stream output = context.Response.OutputStream;

					byte[] buffer = new byte[BUFFERSIZE];
					int count = 0;

					while((count = stream.Read(buffer, 0, BUFFERSIZE)) > 0)
					{
						output.Write(buffer, 0, count);
						output.Flush();
					}

					// log
					log.LogAccess("Download", node.GetRelativePath(), node.ID, "Success");
				}
				catch
				{
					// log
					log.LogAccess("Download", node.GetRelativePath(), node.ID, "Failed");

					throw;
				}
				finally
				{
					// release the file
					stream.Close();
				}
			}
			catch(Exception e)
			{
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				context.Response.StatusDescription = e.GetType().Name;
			}
		}

		#endregion
	}
}
