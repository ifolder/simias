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
*                 $Author:   Brady Anderson (banderso@novell.com)
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
using System.Configuration;
using System.ComponentModel;
//using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Net;

using Simias;
using Simias.Storage;


namespace Simias.Authentication
{
	/// <summary>
	/// 
	/// </summary>
	public class Session
	{
		/// <summary>
		/// 
		/// </summary>
		public Session()
		{
			Requests = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		public GenericPrincipal User;
		/// <summary>
		/// 
		/// </summary>
		public string			MemberID;
		/// <summary>
		/// 
		/// </summary>
		public UInt64			Requests;
	}

	/// <summary>
	/// Summary description for Http
	/// </summary>
	public class Http
	{
		/// <summary>
		/// Used to log messages.
		/// </summary>
		//private static readonly ISimiasLog log = SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		// Response headers set by the Http Authentication Module
		/// </summary>
		public readonly static string DaysUntilPwdExpiresHeader = "Simias-Days-Until-Pwd-Expires";
		/// <summary>
		/// 
		/// </summary>
		public readonly static string GraceTotalHeader = "Simias-Grace-Total";
		/// <summary>
		/// 
		/// </summary>
		public readonly static string GraceRemainingHeader = "Simias-Grace-Remaining";
		/// <summary>
		/// 
		/// </summary>
		public readonly static string SimiasErrorHeader = "Simias-Error";
		/// <summary>
		/// 
		/// </summary>
		public readonly static string DomainIDHeader = "Domain-ID";
		/// <summary>
		/// 
		/// </summary>
		public readonly static string UserMovedHeader = "User-Moved";

		private static readonly string sessionTag = "simias";
		private static readonly string[] rolesArray = { "users" };
		private static readonly string[] hostRoles = {"users", "hosts"};

		/// <summary>
		/// 
		/// </summary>
		public static readonly string NonceKey = "nonce";
		/// <summary>
		/// 
		/// </summary>
		public static readonly string PpkAuthKey = "pkauth";
		/// <summary>
		/// 
		/// </summary>
		public static readonly string PpkType = "PPK";
		/// <summary>
		/// 
		/// </summary>
		public static int UserMoved = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
		public Http()
		{
		}

		/// <summary>
		/// Sets the last login time on the user.
		/// </summary>
		/// <param name="domain">The domain the user has authenticated to.</param>
		/// <param name="member">Member to set last login time for.</param>
		private static void SetLastLoginTime( Domain domain, Member member )
		{
			Property p = new Property( PropertyTags.LastLoginTime, DateTime.Now );
			p.LocalProperty = true;
			member.Properties.ModifyNodeProperty( p );
			domain.Commit( member );
		}

        /// <summary>
        /// Set the response headers for Http actions
        /// </summary>
        /// <param name="ctx">Context for the Http</param>
        /// <param name="status">Status whether authenticated or not</param>
		private static void SetResponseHeaders( HttpContext ctx, Simias.Authentication.Status status )
		{
			switch ( status.statusCode )
			{
				case StatusCodes.Success:
				{
					if ( status.DaysUntilPasswordExpires != -1 )
					{
						ctx.Response.AppendHeader(
							DaysUntilPwdExpiresHeader,
							status.DaysUntilPasswordExpires.ToString() );
					}
 					if(UserMoved == 1)
                                        {
                                                ctx.Response.AppendHeader(
                                                        UserMovedHeader,
                                                        StatusCodes.UserAlreadyMoved.ToString() );
                                        }

					break;
				}

				case StatusCodes.SuccessInGrace:
				{
					ctx.Response.AppendHeader(
						GraceTotalHeader,
						status.TotalGraceLogins.ToString() );

					ctx.Response.AppendHeader(
						GraceRemainingHeader,
						status.RemainingGraceLogins.ToString() );
                                        if(UserMoved == 1)
                                        {
                                                ctx.Response.AppendHeader(
                                                        UserMovedHeader,
                                                        StatusCodes.UserAlreadyMoved.ToString() );
                                        }
				
					break;
				}

				case StatusCodes.AccountDisabled:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.AccountDisabled.ToString() );
					break;
				}

				case StatusCodes.AccountLockout:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.AccountLockout.ToString() );
					break;
				}

				case StatusCodes.SimiasLoginDisabled:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.SimiasLoginDisabled.ToString() );
					break;
				}

				case StatusCodes.AmbiguousUser:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.AmbiguousUser.ToString() );
					break;
				}

				case StatusCodes.UnknownUser:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.UnknownUser.ToString() );
					break;
				}

				case StatusCodes.Unknown:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.Unknown.ToString() );
					break;
				}

				case StatusCodes.InvalidCredentials:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.InvalidCredentials.ToString() );
					break;
				}

				case StatusCodes.InvalidCertificate:
				{
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.InvalidCertificate.ToString() );
					break;
				}

				case StatusCodes.InvalidPassword:
				{
					ctx.Response.StatusCode = 401;
					/*
					context.Response.AppendHeader(
						Login.SimiasErrorHeader,
						StatusCodes.InvalidPassword.ToString() );
					*/
					break;
				}

				case StatusCodes.InternalException:
				{
					ctx.Response.StatusCode = 500;
					ctx.Response.AddHeader(
						SimiasErrorHeader,
						StatusCodes.InternalException.ToString() );
					break;
				}

            case StatusCodes.UserAlreadyMoved:
                {
                    ctx.Response.AddHeader(
                        UserMovedHeader,
                        StatusCodes.UserAlreadyMoved.ToString());
                    break;
                }

				default:
					ctx.Response.StatusCode = 401;
					break;
			}
		}

		/// <summary>
		/// Summary description for Http
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="ctx"></param>
		static public Simias.Storage.Member GetMember( string domainID, HttpContext ctx )
		{
			Simias.Authentication.Session simiasSession;
			Simias.Authentication.Status status;
			Simias.Storage.Domain domain = null;
			Simias.Storage.Member member = null;
			Store store = Store.GetStore();

			ctx.Response.Cache.SetCacheability( HttpCacheability.NoCache );

			//
			// Look for the special domain ID header in the request.  If the
			// header doesn't exist use the default domain
			//

			if ( ( domainID != null ) && ( domainID != String.Empty ) )
			{
				domain = store.GetDomain( domainID );
			}

			if ( domain == null )
			{
				ctx.Response.StatusCode = 500;
				ctx.Response.StatusDescription = "Invalid Domain";
				ctx.ApplicationInstance.CompleteRequest();
				return null;
			}

			if ( ctx.Session != null )
			{
				simiasSession = ctx.Session[ sessionTag ] as Simias.Authentication.Session;
				if (simiasSession != null)
					ctx.User = simiasSession.User;

				if ( ctx.User.Identity.IsAuthenticated == false )
				{
					status = DomainProvider.Authenticate( domain, ctx );
					if ( status.statusCode != StatusCodes.Success &&
						status.statusCode != StatusCodes.SuccessInGrace )
					{
						Simias.Authentication.Http.SetResponseHeaders( ctx, status );
						if ( ctx.Response.StatusCode == 401 )
						{
							ctx.Response.AddHeader( 
								"WWW-Authenticate", 
								String.Concat("Basic realm=\"", domain.Name, "\""));
						}

						ctx.ApplicationInstance.CompleteRequest();
						return null;
					}

					// Authentication modules are required to set the member's
					// userID on successful authentication - let's make sure
					if ( status.UserID == null || status.UserID == "")
					{
						ctx.Response.StatusCode = 500;
						ctx.Response.StatusDescription = "Unknown UserID";
						ctx.ApplicationInstance.CompleteRequest();
						return null;
					}

					member = domain.GetMemberByID( status.UserID );
					if ( member == null )
					{
						ctx.Response.StatusCode = 500;
						ctx.Response.StatusDescription = "Unknown Member in Domain";
						ctx.ApplicationInstance.CompleteRequest();
						return null;
					}

					if ( status.statusCode == StatusCodes.SuccessInGrace ||
						status.statusCode == StatusCodes.Success )
					{
						Simias.Authentication.Http.SetResponseHeaders( ctx, status );
					}

					simiasSession = new Simias.Authentication.Session();
					simiasSession.MemberID = member.UserID;
					simiasSession.Requests++;
					ctx.Session[ sessionTag ] = simiasSession;

					// Setup a principal
					simiasSession.User = 
						new GenericPrincipal(
						new GenericIdentity(
						member.UserID,
						"Basic authentication"), 
						rolesArray);

					ctx.User = simiasSession.User;
					Thread.CurrentPrincipal = ctx.User;

					// Set the last login time for the user.
					SetLastLoginTime( domain, member );
				}
				else
				{
					simiasSession.Requests++;
					Thread.CurrentPrincipal = ctx.User;
					member = domain.GetMemberByID( simiasSession.MemberID );
				}
			}
			else
			{
				// No session exists so "authenticate" every request
				status = DomainProvider.Authenticate( domain, ctx );
				if ( status.statusCode != StatusCodes.Success &&
					status.statusCode != StatusCodes.SuccessInGrace )
				{
					Simias.Authentication.Http.SetResponseHeaders( ctx, status );
					if ( ctx.Response.StatusCode == 401 )
					{
						ctx.Response.AddHeader( 
							"WWW-Authenticate", 
							String.Concat("Basic realm=\"", domain.Name, "\""));
					}

					ctx.ApplicationInstance.CompleteRequest();
					return null;
				}

				// Authentication modules are required to set the member's
				// userID on successful authentication - let's make sure
				if ( status.UserID == null || status.UserID == "")
				{
					ctx.Response.StatusCode = 500;
					ctx.Response.StatusDescription = "Unknown UserID-2";
					ctx.ApplicationInstance.CompleteRequest();
					return null;
				}

				member = domain.GetMemberByID( status.UserID );
				if ( member == null )
				{
					ctx.Response.StatusCode = 500;
					ctx.Response.StatusDescription = "Unknown Member in Domain-2";
					ctx.ApplicationInstance.CompleteRequest();
					return null;
				}

				// Setup a principal
				ctx.User = 
					new GenericPrincipal(
					new GenericIdentity(
					member.UserID,
					"Basic authentication"), 
					rolesArray);

				Thread.CurrentPrincipal = ctx.User;

				// Set the last login time for the user.
				SetLastLoginTime( domain, member );
			}

			return member;
		}

		/// <summary>
		/// Used by server to validate the signature using PPK.
		/// </summary>
		/// <param name="domainId"></param>
		/// <param name="memberId"></param>
		/// <param name="signed"></param>
		/// <param name="ctx"></param>
		static public void VerifyWithPPK( string domainId, string memberId, byte[] signed, HttpContext ctx)
		{
			Simias.Authentication.Session simiasSession;
			Simias.Storage.Domain domain = null;
			Simias.Storage.Member member = null;
			Store store = Store.GetStore();

			ctx.Response.Cache.SetCacheability( HttpCacheability.NoCache );

			domain = store.GetDomain( domainId );
			if ( domain == null )
			{
				ctx.Response.StatusCode = 500;
				ctx.Response.StatusDescription = "Invalid Domain";
				ctx.ApplicationInstance.CompleteRequest();
				return;
			}

			member = domain.GetMemberByID(memberId);
			if ( member == null )
			{
				ctx.Response.StatusCode = 500;
				ctx.Response.StatusDescription = "Invalid Member";
				ctx.ApplicationInstance.CompleteRequest();
				return;
			}
			
			if ( ctx.Session == null )
			{
				// Must have a session.
				ctx.Response.StatusCode = 401;
				ctx.Response.AddHeader( 
					"WWW-Authenticate", 
					String.Concat("Basic realm=\"", domain.Name, "\""));
				
				ctx.ApplicationInstance.CompleteRequest();
				return;
			}
			
			simiasSession = ctx.Session[ sessionTag ] as Simias.Authentication.Session;
			if (simiasSession != null)
				ctx.User = simiasSession.User;

			if ( ctx.User.Identity.IsAuthenticated == false )
			{
				// Validate signature.
				string nonce = (string)ctx.Session[NonceKey];
				byte[] nonceBytes = Nonce.GetBytes(nonce);
				if (member.PublicKey.VerifyData(nonceBytes, new SHA1CryptoServiceProvider(), signed))
				{
					simiasSession = new Simias.Authentication.Session();
					simiasSession.MemberID = member.UserID;
					simiasSession.Requests++;
					ctx.Session[ sessionTag ] = simiasSession;

					// Setup a principal
					simiasSession.User = 
						new GenericPrincipal(
						new GenericIdentity(
						member.UserID,
						PpkType), 
						hostRoles);

					ctx.User = simiasSession.User;
					Thread.CurrentPrincipal = ctx.User;

					// Set the last login time for the user.
					SetLastLoginTime( domain, member );
				}
				else
				{
					// Failed
					ctx.Response.StatusCode = 401;
					ctx.Response.AddHeader( 
						"WWW-Authenticate", 
						String.Concat("Basic realm=\"", domain.Name, "\""));
					ctx.ApplicationInstance.CompleteRequest();
					return;
				}
			}
			else
			{
				simiasSession.Requests++;
				Thread.CurrentPrincipal = ctx.User;
				member = domain.GetMemberByID( simiasSession.MemberID );
			}
		}

		/// <summary>
		/// Used by the client to autenticate using Private Key Authentication.
		/// </summary>
		/// <param name="domainId"></param>
		/// <param name="memberId"></param>
		/// <param name="baseUrl"></param>
		/// <returns></returns>
		public static bool AuthenticateWithPPK(string domainId, string memberId, string baseUrl)
		{
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(domainId);
				Member member = domain.GetMemberByID(memberId);
				WebState webState = new WebState(domainId);
				// Get the challenge and sign it with the Private Key to use as a one time password.
				string url = baseUrl + "/Login.ashx?" + NonceKey + "=Get";
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				webState.InitializeWebRequest(request, domain.ID);
				request.ContentType = "application/octet-stream";
				request.Method = "GET";
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				request.CookieContainer.Add(response.Cookies);

				if (response.StatusCode != HttpStatusCode.OK)
				{
					CookieCollection cc = request.CookieContainer.GetCookies(new Uri(url));
					foreach (Cookie cookie in cc)
					{
						cookie.Expired = true;
					}
					return false;
				}

				string nonce = response.Headers.Get(NonceKey);
				if (nonce != null)
				{
					byte[] bChallenge = Nonce.GetBytes(nonce);
					byte[] signed = store.CurrentUser.GetDomainCredential(domain.ID).SignData(bChallenge, new SHA1CryptoServiceProvider());
					// Now authenticate using signed data
					url = baseUrl + "/Login.ashx?" + PpkAuthKey + "=" + member.UserID;
					request = (HttpWebRequest)WebRequest.Create(url);
					webState.InitializeWebRequest(request, domain.ID);
					request.ContentType = "application/octet-stream";
					request.Method = "POST";
					request.Headers.Add(Simias.Security.Web.AuthenticationService.Login.DomainIDHeader, domain.ID);
					Stream rStream = request.GetRequestStream();
					rStream.Write(signed, 0, signed.Length);
					rStream.Close();
					response = (HttpWebResponse)request.GetResponse();
				}
			}
			catch {return false;}
			return true;
		}

		/// <summary>
		/// Class to deal with nonce.
		/// </summary>
		public class Nonce
		{
			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public static string GetNonce()
			{
				return Guid.NewGuid().ToString();
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public static byte[] GetBytes(string nonce)
			{
				System.Text.ASCIIEncoding ByteConverter = new System.Text.ASCIIEncoding();
				return ByteConverter.GetBytes(nonce);
			}
		}
	}
}
