/***********************************************************************
 *  $RCSfile: WebDav.cs,v $
 * 
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Brady Anderson (banderso@novell.com)
 * 
 ***********************************************************************/
using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.SessionState;

using Simias;
//using Simias.Storage;

namespace Simias.Dav
{
	public class Handler : IHttpHandler, IRequiresSessionState
	{
		const string serviceTag = "Simias-Dav";
		
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		public void ProcessRequest( HttpContext context )
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;
			HttpSessionState session = context.Session;
			
			try
			{
				// Make sure that there is a session.
				if ( context.Session != null )
				{
					//HttpService service = Session[ serviceTag ];
					response.Cache.SetCacheability( HttpCacheability.NoCache );

					string method = request.HttpMethod.ToLower();
					
					log.Debug( "Simias.Dav.Handler.ProcessRequest called" );
					log.Debug( "  method: " + method );
					
					//SyncMethod method = (SyncMethod)Enum.Parse(typeof(SyncMethod), Request.Headers.Get(SyncHeaders.Method), true);
					if ( method == "post" )
					//string.Compare( request.HttpMethod ) httpMethod, "POST", true) == 0)
					{
						response.StatusCode = (int) HttpStatusCode.BadRequest;
					}
					else
					if ( method == "get" )
					//string.Compare( request.HttpMethod ) httpMethod, "POST", true) == 0)
					{
						response.StatusCode = (int) HttpStatusCode.BadRequest;
					}
					else
					if ( method == "propfind" )
					{
						response.StatusCode = (int) HttpStatusCode.BadRequest;
					}
					else
					if ( method == "options" )
					{
						response.StatusCode = (int) HttpStatusCode.OK;
						response.AddHeader( 
							"Allow", 
							"OPTIONS, GET, HEAD, POST, PUT, DELETE, TRACE, COPY, MOVE, MKCOL, PROPFIND, PROPPATCH, LOCK, UNLOCK" );
						response.AddHeader( "DAV", "1, 2" );
					}
					else
					{
						response.StatusCode = (int) HttpStatusCode.BadRequest;
					}
				}
				else
				{
					response.StatusCode = (int) HttpStatusCode.BadRequest;
				}
			}
			catch( Exception ex )
			{
				//Sync.Log.log.Debug("Request Failed exception\n{0}\n{1}", ex.Message, ex.StackTrace);
				throw ex;
			}
			
			response.End();
		}

		public bool IsReusable
		{
			// To enable pooling, return true here.
			// This keeps the handler in memory.
			get { return true; }
		}
	}
}
