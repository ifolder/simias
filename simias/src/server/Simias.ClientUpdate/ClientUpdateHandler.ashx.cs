/***********************************************************************
 *  $RCSfile: ClientUpdateHandler.ashx.cs,v $
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

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
							if ( platform == MyPlatformID.Windows.ToString() )
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
