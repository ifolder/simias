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
using System.Text;
using System.Reflection;
using System.Resources;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Upload Module
	/// </summary>
	public class UploadModule : IHttpModule
	{
		private int maxRequestLength;

		/// <summary>
		/// Constructor
		/// </summary>
		public UploadModule()
		{
		}
		
		#region IHttpModule Members

		/// <summary>
		/// Init
		/// </summary>
		/// <param name="context"></param>
		public void Init(HttpApplication context)
		{
			// events
			context.BeginRequest += new EventHandler(context_BeginRequest);
			context.EndRequest += new EventHandler(context_EndRequest);
			context.Error += new EventHandler(context_Error);

			// config
			object config = context.Context.GetConfig("system.web/httpRuntime");
			PropertyInfo property = config.GetType().GetProperty("MaxRequestLength",
				BindingFlags.Instance | BindingFlags.NonPublic);
			
			if (property != null)
			{
				maxRequestLength = (int) property.GetValue(config, null);
			}
			else
			{
				// for Mono
				FieldInfo field = config.GetType().GetField("MaxRequestLength",
					BindingFlags.Instance | BindingFlags.Public );
				maxRequestLength = ((int) field.GetValue(config)) * 1024;
			}
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
		}

		#endregion

		private void context_BeginRequest(object sender, EventArgs e)
		{
			HttpApplication application = (HttpApplication) sender;
		
			// upload?
			if (application.Request.ContentType.ToLower().StartsWith("multipart/form-data"))
			{
				// worker
				HttpWorkerRequest worker = (HttpWorkerRequest)
					(application.Context as IServiceProvider).GetService(
						typeof(HttpWorkerRequest));

				Encoding encoding = application.Context.Request.ContentEncoding;

				byte[] boundary = encoding.GetBytes("--" +
					application.Request.ContentType.Substring(
					application.Request.ContentType.IndexOf("boundary=") + 9));

				//if (application.Request.ContentLength > maxRequestLength)
				// KLUDGE: fixed limit of 10 MB because of Mono issues
				const long MAXFILESIZE = (10 * 1024 * 1024);
				
				if (application.Request.ContentLength > MAXFILESIZE)
				{
					// consume the upload file
					// NOTE: this is needed so we can return an error without the browser choking
					const int BUFFERSIZE = (32 * 1024);
					byte[] buffer = new byte[BUFFERSIZE];

					UploadStream upload = new UploadStream(worker);

					while(upload.Read(buffer, 0, BUFFERSIZE) > 0);
/*
					// update request
					BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

					Type type = worker.GetType();

					type.GetField("_contentAvailLength", flags).SetValue(worker, worker.GetPreloadedEntityBody().Length);
					type.GetField("_contentTotalLength", flags).SetValue(worker, worker.GetPreloadedEntityBody().Length);
					type.GetField("_preloadedContent", flags).SetValue(worker, worker.GetPreloadedEntityBody());
					type.GetField("_preloadedContentRead", flags).SetValue(worker, true);
*/
					// localization
					ResourceManager rm = (ResourceManager) application.Application["RM"];

					// redirect
					string ifolderID = application.Request.QueryString["iFolder"];
					string entryID = application.Request.QueryString["Entry"];

					application.Response.Redirect(String.Format(
						"{0}?iFolder={1}&Entry={2}&MessageType={3}&MessageText={4}",
						application.Request.Path, ifolderID, entryID,
						application.Context.Server.UrlEncode(rm.GetString("MESSAGE.ERROR")),
						application.Context.Server.UrlEncode(rm.GetString("ENTRY.MAXUPLOADSIZEEXCEPTION"))));
				}
				else
				{
					// save the upload file
				}
			}
		}

		private void context_EndRequest(object sender, EventArgs e)
		{
			HttpApplication application = (HttpApplication) sender;
		}

		private void context_Error(object sender, EventArgs e)
		{
			HttpApplication application = (HttpApplication) sender;
		}
	}
}
