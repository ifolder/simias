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
*                 $Author: <Creator>
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
		const int BUFFERSIZE = (64 * 1024);

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

				// does node exist
				if (node == null)
				{
					string id = ((entryID != null) && (entryID.Length != 0)) ? entryID : entryPath;
					throw new EntryDoesNotExistException(id);
				}

				//FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			
				FileInfo MyFileInfo = new FileInfo(filePath);
				long FileSize = MyFileInfo.Length;

				try
				{
					// response
					context.Response.Clear();
					context.Response.AddHeader("Content-Disposition",
						String.Format("attachment; filename=\"{0}\"",
						HttpUtility.UrlEncode(filename, System.Text.Encoding.UTF8).Replace("+", "%20")));
					context.Response.AddHeader("Content-Length", FileSize.ToString());
					context.Response.ContentType = "application/octet-stream";
					context.Response.BufferOutput = false;

					context.Response.WriteFile(filePath);
					
					//Stream output = context.Response.OutputStream;

					//byte[] buffer = new byte[BUFFERSIZE];
					//int count = 0;


					//while((count = stream.Read(buffer, 0, BUFFERSIZE)) > 0)
					//{
					//	output.Write(buffer, 0, count);
					//	output.Flush();
					//}

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
					//stream.Close();
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
