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
 |***************************************************************************/

using System;
using System.IO;

//using Simias.Client;
//using Simias.Client.Authentication;

namespace Simias.Security.Web.AuthenticationService
{
    /// <summary>
	/// Attribute used to identify a class that implements the
	/// IAuthenticationService.
	/// 
	/// It is necessary to associate this attribute with the
	/// class that implements the interface to allow for your
	/// authentication service to be configured via Web Configuration.
	/// </summary>
	public class IAuthenticationServiceAttribute: System.Attribute {}


	/// <summary>
	/// Well known path for logging into a Simias domain
	/// An HttpRequest (get/post) can be issued against this path
	/// </summary>
	public class Login
	{
		public static string Path = "/simias10/Login.ashx";

		// Response headers set by the Http Authentication Module
		public readonly static string DaysUntilPwdExpiresHeader = "Simias-Days-Until-Pwd-Expires";
		public readonly static string GraceTotalHeader = "Simias-Grace-Total";
		public readonly static string GraceRemainingHeader = "Simias-Grace-Remaining";
		public readonly static string SimiasErrorHeader = "Simias-Error";
		public readonly static string DomainIDHeader = "Domain-ID";
		public readonly static string BasicEncodingHeader = "Basic-Encoding";
	}

	/// <summary>
	/// Defines the AuthenticationService interface.
    /// </summary>
	public interface IAuthenticationService
	{
       /// <summary>
       /// Authenticates the user with password
       /// </summary>
       /// <returns>
       /// Returns a string representing the user name for the identity
       /// of the principle to be set on the current context and session.
       /// </returns>

	   string Authenticate(string user, string password);

		/// <summary>
		/// Authenticates the user by name and password
		/// </summary>
        /// <returns>
		/// Returns an authentication status object
		/// </returns>

		Simias.Authentication.Status AuthenticateByName(string user, string password);

		/// <summary>
		/// Authenticates the user using their unique ID and a password
		/// </summary>
        /// <returns>
		/// Returns an authentication status object
		/// </returns>

		Simias.Authentication.Status AuthenticateByID(string id, string password);
    }
}
