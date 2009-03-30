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
* Novell is the copyright owner of this file.  Novell may have released an earlier versoion of this
* file, also owned by Novell, under the GNU General Public License version 2 as part of Novell's 
* iFolder Project; however, Novell is not releasing this file under the GPL.
*
*-----------------------------------------------------------------------------
*
*                 Novell iFolder Enterprise
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

				// lock the file
				FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				try
				{
					// response
					context.Response.Clear();
					context.Response.AddHeader("Content-Disposition",
						String.Format("attachment; filename=\"{0}\"",
						HttpUtility.UrlEncode(filename, System.Text.Encoding.UTF8).Replace("+", "%20")));
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
