/***********************************************************************
 *  $RCSfile: iFolderReportsClient.cs,v $
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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Net;
using System.Web.Services.Protocols;

using Simias.Client;

namespace Novell.iFolder.Enterprise
{
	/// <summary>
	/// iFolder Reports Client
	/// </summary>
	class iFolderReportsClient
	{
		/// <summary>
		/// Main
		/// </summary>
		/// <param name="args">Command Arguments</param>
		static void Main(string[] args)
		{
			Console.WriteLine();
			Console.WriteLine("iFolder Reports Client");
			Console.WriteLine();

			// web service
			iFolderReports web = new iFolderReports();

			// uri
			if (args.Length >= 1)
			{
				Uri argUri = new Uri(args[0]);

				UriBuilder newUri = new UriBuilder(web.Url);
				
				// only pull the host and port
				newUri.Host = argUri.Host;
				if (argUri.Port != -1) newUri.Port = argUri.Port;
				web.Url = newUri.Uri.ToString();
			}

			// *** begin authenticate ***
			// kludge: Simias does not support a local authentication on the server from
			// a web service. This kludge makes a call to the login handler and uses
			// its cookie for authentication on the web service.
			
			// login handler request
			UriBuilder loginUri = new UriBuilder(web.Url);
			loginUri.Path = Simias.Security.Web.AuthenticationService.Login.Path;
			Console.WriteLine("Authentication Service: {0}", loginUri);
			
			HttpWebRequest request = WebRequest.Create(loginUri.Uri) as HttpWebRequest;
			request.CookieContainer = new CookieContainer();

			// local password file
			Simias.Configuration config = Simias.Configuration.GetServerBootStrapConfiguration();
			Console.WriteLine("Store Path: {0}", config.StorePath);
			string path = Path.Combine(config.StorePath, ".local.if");
			Console.WriteLine("Password File: {0}", path);

			string password;

			using (StreamReader reader = new StreamReader(path))
			{
				password = reader.ReadLine();
			}

			// parse cn from Simias admin
			string username = config.Get(Simias.Storage.Domain.SectionName, Simias.Storage.Domain.AdminDNTag, null);
			if (username != null)
			{
				int start = username.ToLower().StartsWith("cn=") ? 3 : 0;
				int end = username.IndexOf(',');
				int length = (end == -1) ? (username.Length - start) : (end - start);
				username = username.Substring(start, length);
			}
			Console.WriteLine("Admin Username: {0}", username);
			
			// credentials
			request.Credentials = new NetworkCredential(username, password);
			
			// set the local domain id from the local password
			string domainID = password.Substring(0, Guid.NewGuid().ToString().Length);
			request.Headers.Add( 
				Simias.Security.Web.AuthenticationService.Login.DomainIDHeader,
				domainID);
			Console.WriteLine("Domain ID: {0}", domainID);
			
			// authenticate
			Console.Write("Authenticating...");
			HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            Console.WriteLine(response.StatusCode);
			
			// save the cookie from the authentication
			web.CookieContainer = request.CookieContainer;

			// *** end authenticate ***

			// generate reports
			Console.WriteLine();
			Console.WriteLine("Reports Service: {0}", web.Url);
			Console.Write("Generating Reports...");
			web.Generate();
			Console.WriteLine("Done");

			Console.WriteLine();
		}
	}
}