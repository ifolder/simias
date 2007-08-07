/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |   Author: Mike Lasky <mlasky@novell.com>
 |***************************************************************************/
 

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
					string nonceValue = request.QueryString[Simias.Authentication.Http.NonceKey];
					string userID = request.QueryString[Simias.Authentication.Http.PpkAuthKey];
					if (nonceValue != null)
					{
						string nonce = Simias.Authentication.Http.Nonce.GetNonce();
						context.Session[Simias.Authentication.Http.NonceKey] = nonce;
						response.AddHeader(Simias.Authentication.Http.NonceKey, nonce);
					}
					else if (userID != null)
					{
						int length = request.ContentLength;
						byte[] signed = new byte[length];
						int lread = request.InputStream.Read(signed, 0, length);
						if (lread == length)
						{
							Http.VerifyWithPPK(domainID, userID, signed, context);
						}
					}
					else
					{
						Http.GetMember( domainID, context );
					}
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
