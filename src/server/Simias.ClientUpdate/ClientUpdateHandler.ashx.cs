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
using System.Web;
using System.Web.SessionState;

using Simias;
using Simias.Client;

namespace Novell.iFolder.Enterprise.Web
{
	/// <summary>
	/// Summary description for ClientUpdateHandler.
	/// </summary>
	public class ClientUpdateHandler : IHttpHandler, IRequiresSessionState
	{
		#region Class Members

		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( ClientUpdateHandler ) );

		/// <summary>
		/// Strings used in the handler query.
		/// </summary>
		private static string PlatformQuery = "Platform";
		private static string FileQuery = "File";

		#endregion

		#region IHttpHandler Members

		/// <summary>
		/// Enables processing of HTTP Web requests by a custom HttpHandler 
		/// that implements the IHttpHandler interface.
		/// </summary>
		/// <param name="context">An HttpContext object that provides 
		/// references to the intrinsic server objects (for example, 
		/// Request, Response, Session, and Server) used to service HTTP requests.</param>
		public void ProcessRequest(HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;
			response.Cache.SetCacheability( HttpCacheability.NoCache );
			response.StatusCode = ( int )HttpStatusCode.BadRequest;

			try
			{
				// Only respond to GET method.
				if ( String.Compare( request.HttpMethod, "GET", true ) == 0 )
				{
					// Get the name of the file and the platform type from the query 
					// string and make sure that only one file was specified.
					NameValueCollection nvc = request.QueryString;
					if ( nvc.Count == 2 )
					{
						// Make sure the right query strings are present.
						if ( ( nvc[ PlatformQuery ] != null ) && ( nvc[ FileQuery ] != null ) )
						{
							string updateDir = null;
							string platform = nvc[ PlatformQuery ] as string;
							log.Debug( "PlatformQuery: {0}", platform );
							log.Debug( "FileQuery: {0}", nvc[ FileQuery ] );
							//MyPlatformID platform = ( MyPlatformID )Enum.Parse( typeof( MyPlatformID ), nvc[ PlatformQuery ], true );
							if ( platform.Equals("Darwin") )
							{
								log.Debug("Processing request for downloading Darwin files");
								updateDir = Path.Combine( SimiasSetup.webdir, ClientUpdate.MacUpdateDir);

							}
							else if ( platform.StartsWith("windows") || platform == MyPlatformID.Windows.ToString() )
							{
								// Build a path to the windows update directory.
								updateDir = Path.Combine( SimiasSetup.webdir, ClientUpdate.WindowsUpdateDir );
							}
							else
							{
								string downloadDir = ClientUpdate.GetDistributionDownloadDirectory( platform );
								if ( downloadDir != null )
								{
									updateDir = Path.Combine( SimiasSetup.webdir, ClientUpdate.UnixUpdateDir );
									updateDir = Path.Combine( updateDir, downloadDir );
									
								}
							}
							
							if ( updateDir != null )
							{
								response.ContentType = "application/octet-stream";
								response.WriteFile( Path.Combine( updateDir, nvc[ FileQuery ] ) );
								response.StatusCode = ( int )HttpStatusCode.OK;
							}
							else
							{
								response.StatusCode = ( int )HttpStatusCode.NotImplemented;
							}
						}
						else
						{
							log.Debug( "Error: Invalid query string" );
						}
					}
					else
					{
						log.Debug( "Error: Invalid query string parameter count - {0}", nvc.Count );
					}
				}
				else
				{
					log.Debug( "Error: Invalid http method - {0}", request.HttpMethod );
				}
			}
			catch ( Exception ex )
			{
				log.Debug( "Error: {0}", ex.Message );
				response.StatusCode = ( int ) HttpStatusCode.InternalServerError;
			}
		}

		/// <summary>
		/// Gets a value indicating whether another request can use the 
		/// IHttpHandler instance.
		/// </summary>
		public bool IsReusable
		{
			get { return true; }
		}

		#endregion
	}
}
