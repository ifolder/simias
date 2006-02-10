/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Web.SessionState;

using Simias.Authentication;

namespace Simias
{
	/// <summary>
	/// Summary description for ClientUpdateHandler.
	/// </summary>
	public class Login : IHttpHandler, IRequiresSessionState
	{
		#region Class Members

		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Login ) );

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
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;

			try
			{
				// Only respond to GET or POST method.
				if ( ( String.Compare( request.HttpMethod, "GET", true ) == 0 ) ||
					 ( String.Compare( request.HttpMethod, "POST", true ) == 0 ) )
				{
					string domainID = context.Request.Headers[ Simias.Security.Web.AuthenticationService.Login.DomainIDHeader ];
					Http.GetMember( domainID, context );
				}
				else
				{
					log.Debug( "Error: Invalid http method - {0}", request.HttpMethod );
					response.StatusCode = ( int ) HttpStatusCode.BadRequest;
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
