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
*                 $Author: Mike Lasky <mlasky@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
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


using Simias;
using Simias.Client;
using Simias.Storage;

namespace Simias.Server
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
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( ReportLogHandler ) );

		/// <summary>
		/// The default length to use if no length is specified.
		/// </summary>
		static private int DefaultLength = 128 * 1024;

		/// <summary>
		/// Tags used as keys in query string.
		/// </summary>
		static private string Length = "length";
		static private string Offset = "offset";
		static private string Size = "size";

		/// <summary>
		/// Name of log directory
		/// </summary>
		static private string logDirName = "log";

		#endregion

		#region Private Methods

		/// <summary>
		/// Authorize the authenticated user.
		/// </summary>
		/// <param name="context"></param>
		private void Authorize( HttpContext context )
		{
			// check authentication
			string userID = GetUserID( context );

			// check for an admin ID cache
			string adminID = context.Session[ "AdminID" ] as string;

			// check the ID cache
			if ( ( adminID == null ) || ( adminID.Length == 0 ) || ( !adminID.Equals( userID ) ) )
			{
				if ( IsAdministrator( userID ) )
				{
					// authorized
					context.Session[ "AdminID" ] = userID;
				}
				else
				{
					// unauthorized
					log.Debug( "User {0} is not authorized", userID );
					throw new AuthorizationException( userID );
				}
			}
		}

		/// <summary>
		/// Gets chunks of data from the specified file.
		/// </summary>
		/// <param name="filePath">Path to the file.</param>
		/// <param name="offset">Offset to start reading at.</param>
		/// <param name="length">Length of data to return.</param>
		/// <returns>A byte array containing data read from the specified offset.</returns>
		private byte[] GetFileData( string filePath, long offset, int length )
		{
			FileStream fs = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
			using( BinaryReader br = new BinaryReader( fs ) )
			{
				// If the offset is negative, offset from the end of the file.
				if ( offset < 0 )
				{
					offset = br.BaseStream.Length + offset;
					if ( offset < 0 )
					{
						offset = 0;
					}
				}
				else
				{
					// Don't go beyond the end of the file.
					if ( offset > br.BaseStream.Length )
					{
						offset = br.BaseStream.Length;
					}
				}

				// Zero length array.
				byte[] buffer = new byte[ 0 ];

				if ( length > 0 )
				{
					try
					{
						br.BaseStream.Position = offset;
						buffer = br.ReadBytes( length );
					}
					catch ( EndOfStreamException )
					{}
				}

				log.Debug( "Read {0} bytes from log file {1} at offset = {2} with length = {3}", buffer.Length, filePath, offset, length );
				return buffer;
			}
		}

		/// <summary>
		/// Returns the length of the file in bytes.
		/// </summary>
		/// <param name="filePath">Path to get the file length for.</param>
		/// <returns>The length of the file in bytes.</returns>
		private long GetFileLength( string filePath )
		{
			long length = 0;

			using ( FileStream fs = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
			{
				length = fs.Length;
			}

			return length;
		}

		/// <summary>
		/// Get the authenticated user's id.
		/// </summary>
		/// <param name="context"></param>
		private string GetUserID( HttpContext context )
		{
			// check authentication
			string userID = context.User.Identity.Name;

			if ( ( userID == null ) || ( userID.Length == 0 ) )
			{
				log.Debug( "Connection has not been authenticated." );
				throw new AuthenticationException();
			}

			return userID;
		}

		/// <summary>
		/// Is the User an Administrator
		/// </summary>
		/// <remarks>
		/// A User is a system administrator if the user has "Admin" rights in the domain.
		/// </remarks>
		/// <param name="userID">The User ID</param>
		/// <returns>true, if the User is a System Administrator</returns>
		private bool IsAdministrator( string userID )
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain( store.DefaultDomain );
			Member member = domain.GetMemberByID( userID );
			Access.Rights rights = ( member != null ) ? member.Rights : Access.Rights.Deny;
			
			return (rights == Access.Rights.Admin);
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
			// Authorize this request.
			Authorize( context );

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

					// Query strings.
					NameValueCollection query = request.QueryString;
					long offset = ( query[ Offset ] != null ) ? Convert.ToInt64( query[ Offset ] ) : 0;
					int length = ( query[ Length ] != null ) ? Convert.ToInt32( query[ Length ] ) : DefaultLength;

					// Setup the response.
					response.Clear();
					response.Cache.SetCacheability( HttpCacheability.NoCache );
					response.BufferOutput = false;

					// Determine the file download type.
					switch( Path.GetExtension( fileName ) )
					{
						case ".log":

							// Full path to the log file.
						        string logDir = Path.Combine ( Store.StorePath, logDirName);
							string logFilePath = Path.Combine( logDir, fileName );
							byte[] logData = null;

							// See if there is a string that is asking for the length of the file.
							if ( query[ Size ] != null )
							{
								UTF8Encoding encoder = new UTF8Encoding();
								logData = encoder.GetBytes( GetFileLength( logFilePath ).ToString() );
							}
							else
							{
								try
								{
									logData = GetFileData( logFilePath, offset, length );
								}
								catch ( IOException ex )
								{
									log.Debug( "Error: reading log file: {0}", ex.Message );
									throw ex;
								}

								// Information for the download dialog.
								response.AddHeader( 
									"Content-Disposition", 
									String.Format("attachment; filename={0}", fileName ) );
							}

							response.ContentType = "text/plain";
							response.AddHeader("Content-Length", logData.Length.ToString() );
							response.OutputStream.Write( logData, 0, logData.Length );
							response.Close();

							break;

						case ".csv":

							// Full path to the report file.
							string csvFilePath = Path.Combine( Report.CurrentReportPath, fileName );
							byte[] csvData = new byte[ 0 ];

							if ( query[ Size ] != null )
							{
								UTF8Encoding encoder = new UTF8Encoding();
								csvData = encoder.GetBytes( GetFileLength( csvFilePath ).ToString() );
								response.ContentType = "text/plain";
							}
							else
							{
								try
								{
									csvData = GetFileData( csvFilePath, offset, length );
								}
								catch ( IOException ex )
								{
									log.Debug( "Error: reading report file: {0}", ex.Message );
									throw ex;
								}

								// Information for the download dialog.
								response.AddHeader(
									"Content-Disposition", 
									String.Format("attachment; filename={0}", fileName ) );

								response.ContentType = "text/csv";
							}

							response.AddHeader("Content-Length", csvData.Length.ToString() );
							response.OutputStream.Write( csvData, 0, csvData.Length );
							response.Close();

							break;

						default:
							log.Debug( "Error: Invalid query string value" );
							response.StatusCode = ( int )HttpStatusCode.BadRequest;
							response.Close();
							break;
					}
				}
				else
				{
					log.Debug( "Error: Invalid http method - {0}", request.HttpMethod );
					response.StatusCode = ( int )HttpStatusCode.BadRequest;
					response.Close();
				}
			}
			catch ( Exception ex )
			{
				log.Debug( "Error: {0}", ex.Message );
				log.Debug( "Stack trace: {0}", ex.StackTrace );
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
