/***********************************************************************
 *  $RCSfile$ LogTailHandler.ashx.cs
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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections;
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
	/// Summary description for LogTailHandler.
	/// </summary>
	public class LogTailHandler : IHttpHandler, IRequiresSessionState
	{
		#region Class Members

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly iFolderWebLogger log = new iFolderWebLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name );

		/// <summary>
		/// Maximum read buffer size.
		/// </summary>
		private const int BufferSize = 128 * 1024;

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the size of the specified file.
		/// </summary>
		/// <param name="web">iFolderAdmin object.</param>
		/// <param name="fileName">The name of the file to get the size for.</param>
		/// <returns>The current size of the specified file.</returns>
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

		/// <summary>
		/// Gets a line from the specified buffer. The line must be bounded by an eol
		/// at the end and at the beginning of the data in order to be a complete line.
		/// The eol at the beginning of the line is not returned with the data.
		/// </summary>
		/// <param name="buffer">Buffer that contains line data.</param>
		/// <param name="eob">Index to the end of the buffer data.</param>
		/// <param name="strict">If false line does not need start eol bounds character.</param>
		/// <returns>A byte array containing a line with a terminating eol char if
		/// successful. Otherwise an empty buffer is returned.</returns>
		private byte[] GetLineData( byte[] buffer, int eob, bool strict )
		{
			bool foundEol = false;
			byte[] data = new byte[ 0 ];
			int eol;
			int index;

			// If the last chars in the buffer are not eols, discard them until an eol is found.
			for( index = eob, eol = 0; index >= 0; --index )
			{
				// Look for both types of line terminators.
				if ( ( buffer[ index ] == '\r' ) || ( buffer[ index ] == '\n' ) )
				{
					if ( foundEol == false )
					{
						eol = index;
						foundEol = true;
					}
				}
				else if ( foundEol )
				{
					break;
				}
			}

			// See if any eol chars were found.
			if ( index >= 0 )
			{
				// Need to find another eol before we can count this as a complete line.
				int sol;
				for ( sol = index; sol >= 0; --sol )
				{
					if ( ( buffer[ sol ] == '\r' ) || ( buffer[ sol ] == '\n' ) )
					{
						// Point back at the start of the line.
						++sol;
						break;
					}
				}

				// See if the start of the line was found.
				if ( sol >= 0 )
				{
					int length = ( eol - sol ) + 1;
					data = new byte[ length ];
					Array.Copy( buffer, sol, data, 0, length );
				}
				else if ( !strict )
				{
					int length = eol + 1;
					data = new byte[ length ];
					Array.Copy( buffer, 0, data, 0, length );
				}
			}

			return data;
		}

		/// <summary>
		/// Gets the number of lines to read from the request.
		/// </summary>
		/// <param name="request">HttpRequest object</param>
		/// <returns>The number of lines from the url query string.</returns>
		private int GetLineCount( HttpRequest request )
		{
			// Default the number of lines.
			int lines = 10;

			// New requests will contain the number of lines to send back.
			NameValueCollection query = request.QueryString;
			if ( query[ "lines" ] != null )
			{
				// This is a new request. Save the request information on the session.
				lines = Convert.ToInt32( query[ "lines" ] );
				if ( lines > 512 ) lines = 512;
			}

			return lines;
		}

		/// <summary>
		/// Gets the specified tail data.
		/// </summary>
		/// <param name="web">iFolderAdmin object</param>
		/// <param name="fileName">The name of the file to tail.</param>
		/// <param name="fileLength">The current length of the file.</param>
		/// <param name="ti">The tail information saved on the session.</param>
		/// <param name="length">Receives the total length of the data in the ArrayList.</param>
		/// <returns>An array of byte arrays containing file data.</returns>
		private ArrayList GetTailData( iFolderAdmin web, string fileName, long fileLength, TailInfo ti, out int length )
		{
			ArrayList tailLines = new ArrayList( ti.Lines );
			length = 0;

			// Build the path to the log handler.
			UriBuilder uri = new UriBuilder( web.Url );
			uri.Path = String.Format( "/simias10/admindata/{0}", fileName );

			// Is this a first request?
			if ( ti.Offset == -1 )
			{
				// Have to guess how much data to request.
				byte[] buffer = new byte[ ti.Lines * 256 ];

				// Read one buffer size from the end of the file.
				long readLength = ( fileLength > buffer.Length ) ? buffer.Length : fileLength;

				// Calculate the offset to read from.
				ti.Offset = fileLength - readLength;

				// Add the query string part.
				uri.Query = String.Format( "offset={0}&length={1}", ti.Offset, readLength );

				// Build the web request to get the data.
				HttpWebRequest webRequest = WebRequest.Create( uri.Uri ) as HttpWebRequest;
				webRequest.Method = "GET";
				webRequest.PreAuthenticate = true;
				webRequest.Credentials = web.Credentials;
				webRequest.CookieContainer = web.CookieContainer;

				HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
				try
				{
					Stream sr = webResponse.GetResponseStream();
					int bytesRead = sr.Read( buffer, 0, ( int )readLength );
					if ( bytesRead > 0 )
					{
						// Get the specified number of lines until the data is all read.
						for ( int lines = 0, eob = bytesRead - 1; 
							  ( lines < ti.Lines ) && ( eob >= 0 ); 
							  ++lines )
						{
							byte[] line = GetLineData( buffer, eob, true );
							if ( line.Length > 0 )
							{
								tailLines.Add( line );
								eob -= line.Length;
								length += line.Length;
							}
							else
							{
								// No lines exist in the buffer. Return no data.
								eob = -1;
							}
						}

						// If any data was returned, update the offset.
						if ( tailLines.Count > 0 )
						{
							ti.Offset += bytesRead;
							tailLines.Reverse();
						}
					}
				}
				finally
				{
					webResponse.Close();
				}
			}
			else
			{
				// See if there is data to read.
				long readLength = fileLength - ti.Offset;
				if ( readLength > 0 )
				{
					// Make sure the read request is not too large.
					if ( readLength > BufferSize )
					{
						readLength = BufferSize;
					}

					// Read to the end of the file.
					byte[] buffer = new byte[ readLength ];

					// Add the query string part.
					uri.Query = String.Format( "offset={0}&length={1}", ti.Offset, readLength );

					// Build the web request to get the data.
					HttpWebRequest webRequest = WebRequest.Create( uri.Uri ) as HttpWebRequest;
					webRequest.Method = "GET";
					webRequest.PreAuthenticate = true;
					webRequest.Credentials = web.Credentials;
					webRequest.CookieContainer = web.CookieContainer;

					HttpWebResponse webResponse = webRequest.GetResponse() as HttpWebResponse;
					try
					{
						Stream sr = webResponse.GetResponseStream();
						int bytesRead = sr.Read( buffer, 0, ( int )readLength );
						if ( bytesRead > 0 )
						{
							// Get the specified number of lines until the data is all read.
							for ( int eob = bytesRead - 1; eob >= 0; )
							{
								byte[] line = GetLineData( buffer, eob, false );
								if ( line.Length > 0 )
								{
									tailLines.Add( line );
									eob -= line.Length;
									length += line.Length;
								}
								else
								{
									// No lines exist in the buffer. Return no data.
									eob = -1;
								}
							}

							// If any data was returned, update the offset.
							if ( tailLines.Count > 0 )
							{
								ti.Offset += length;
								tailLines.Reverse();
							}
						}
					}
					finally
					{
						webResponse.Close();
					}
				}
			}

			return tailLines;
		}

		/// <summary>
		/// Gets or creates a TailInfo object that contains information about tailing
		/// the specified file.
		/// </summary>
		/// <param name="context">HttpContext object</param>
		/// <param name="fileName">The name of the file to tail.</param>
		/// <param name="lines">The number of lines to return on the initial request.</param>
		/// <returns>A TailInfo object that is saved on the session.</returns>
		private TailInfo GetTailInfo( HttpContext context, string fileName, int lines )
		{
			TailInfo ti = null;

			// Is there already tail information on the session?
			Hashtable ht = context.Session[ "TailInfo" ] as Hashtable;
			if ( ht == null )
			{
				context.Session[ "TailInfo" ] = ht = new Hashtable();
			}

			// See if there is information for this file in the hashtable.
			if ( ht.ContainsKey( fileName ) )
			{
				ti = ht[ fileName ] as TailInfo;
			}
			else
			{
				ht[ fileName ] = ti = new TailInfo( lines, -1 );
			}

			return ti;
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

					// New requests will contain the number of lines to send back.
					int lines = GetLineCount( request );

					// Get information about tailing the specified file.
					TailInfo ti = GetTailInfo( context, fileName, lines );

					// Get the line data.
					int length;
					ArrayList lineData = GetTailData( web, fileName, fileSize, ti, out length );
					
					// Setup the response.
					response.Clear();
					response.BufferOutput = false;
					response.Cache.SetCacheability( HttpCacheability.NoCache );
					response.ContentType = "text/plain";
					response.AddHeader("Content-Length", length.ToString() );

					foreach( byte[] line in lineData )
					{
						response.OutputStream.Write( line, 0, line.Length );
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

		#region TailInfo Class

		/// <summary>
		/// Class used to keep track of per file tail information.
		/// </summary>
		private class TailInfo
		{
			#region Class Members

			/// <summary>
			///  Last offset read from file.
			/// </summary>
			private long offset;

			/// <summary>
			/// Number of lines to initially read.
			/// </summary>
			private int lines;

			#endregion

			#region Properties

			/// <summary>
			/// Gets or sets the last offset that the file was read from.
			/// </summary>
			public long Offset
			{
				get { return offset; }
				set { offset = value; }
			}

			/// <summary>
			/// Gets the number of lines to initially read from the file.
			/// </summary>
			public int Lines
			{
				get { return lines; }
			}

			#endregion

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="lines">Number of lines initially requested.</param>
			/// <param name="offset">Current read offset of the file.</param>
			public TailInfo( int lines, long offset )
			{
				this.lines = lines;
				this.offset = offset;
			}

			#endregion
		}

		#endregion
	}
}
