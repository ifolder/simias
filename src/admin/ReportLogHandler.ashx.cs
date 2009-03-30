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
*                 $Author: Mike Lasky (mlasky@novell.com)
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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.SessionState;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for ReportLogHandler.
	/// </summary>
	public class ReportLogHandler : IHttpHandler, IRequiresSessionState
	{
		#region Class Members

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly iFolderWebLogger log = new iFolderWebLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name );

		/// <summary>
		/// Buffer size used to chunk up files.
		/// </summary>
		private const int BufferSize = 128 * 1024;

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the size of the specified file.
		/// </summary>
		/// <param name="web"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private long GetFileSize( iFolderAdmin web, string fileName )
		{
			long fileSize = 0;

			UriBuilder uri = new UriBuilder( web.Url );
			uri.Path = String.Format( "/simias10/admindata/{0}?size=1", fileName );

			HttpWebRequest webRequest = WebRequest.Create( uri.Uri ) as HttpWebRequest;
			webRequest.Method = "GET";
			webRequest.PreAuthenticate = true;
			webRequest.Credentials = web.Credentials;
			webRequest.CookieContainer = web.CookieContainer;

			HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
			try
			{
				StreamReader sr = new StreamReader( webResponse.GetResponseStream(), Encoding.GetEncoding( "utf-8" ) );
				fileSize = Convert.ToInt64( sr.ReadLine() );
			}
			finally
			{
				webResponse.Close();
			}

			return fileSize;
		}

		#endregion

		#region IHttpHandler Members

		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HttpHandler 
		/// that implements the IHttpHandler interface.
		/// </summary>
		/// <param name="context">An HttpContext object that provides 
		/// references to the intrinsic server objects (for example, 
		/// Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest( HttpContext context )
		{
			// Setup the response.
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

			try
			{
				// Only respond to GET method.
				if ( String.Compare( request.HttpMethod, "GET", true ) == 0 )
				{
					// The file name of the url is the file that is to be downloaded.
					string fileName = Path.GetFileName( request.Url.LocalPath );

					iFolderAdmin web = context.Session[ "Connection" ] as iFolderAdmin;
					if ( web == null ) context.Response.Redirect( "Login.aspx" );

					// Get the size of the file.
					long fileSize = GetFileSize( web, fileName );

					// Setup the response.
					response.Clear();
					response.BufferOutput = false;
					response.Cache.SetCacheability( HttpCacheability.Private );
					response.Cache.SetExpires( DateTime.Now );
					response.AddHeader( "Content-Disposition", String.Format( "attachment; filename={0}", fileName ) );
					response.AddHeader("Content-Length", fileSize.ToString() );
					response.ContentType = ( Path.GetExtension( fileName ) == ".log" ) ? "text/plain" : "text/csv";

					// Uri to log file handler.
					UriBuilder uri = new UriBuilder( web.Url );
					uri.Path = String.Format( "/simias10/admindata/{0}", fileName );

					long offset = 0;
					byte[] buffer = new byte[ BufferSize ];
					Stream output = response.OutputStream;

					while ( offset < fileSize )
					{
						// The log file size will change while we are reading the file.
						// Don't read past what the content-length says is the size.
						long readLength = fileSize - offset;
						if ( readLength > buffer.Length ) readLength = buffer.Length;

						// Add the query string part.
						uri.Query = String.Format( "offset={0}&length={1}", offset, readLength );

						HttpWebRequest webRequest = WebRequest.Create( uri.Uri ) as HttpWebRequest;
						webRequest.Method = "GET";
						webRequest.PreAuthenticate = true;
						webRequest.Credentials = web.Credentials;
						webRequest.CookieContainer = web.CookieContainer;

						HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
						try
						{
							Stream webStream = webResponse.GetResponseStream();

							int length = webStream.Read( buffer, 0, ( int )readLength );
							output.Write( buffer, 0, length );
							offset += length;
						}
						finally
						{
							webResponse.Close();
						}
					}

					response.Close();
				}
				else
				{
					log.Debug( context, "Error: Invalid http method - {0}", request.HttpMethod );
					response.StatusCode = ( int )HttpStatusCode.BadRequest;
					response.Close();
				}
			}
			catch ( Exception ex )
			{
				log.Debug( context, "Error: {0}", ex.Message );
				log.Debug( context, "Stack trace: {0}", ex.StackTrace );
				response.StatusCode = ( int ) HttpStatusCode.InternalServerError;
				response.Close();
			}
		}

		/// <summary>
		/// Gets a value indicating whether another request can use the IHttpHandler instance.
		/// </summary>
		public bool IsReusable
		{
			get { return true; }
		}

		#endregion
	}
}
