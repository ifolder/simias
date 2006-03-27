/***********************************************************************
 *  $RCSfile$
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/
using System;
using System.Web;
using System.Net;
using System.Web.Services.Protocols;


namespace Registration
{	class Command
	{
		static private string username = null;
		static private string password = null;
		static private string first = null;
		static private string last = null;
		static private string email = null;
		static private string full = null;
		static private string url = null;
		
		/// <summary>
		/// Parses the command line parameters gathering user info
		/// for the Simias registration.
		/// </summary>
		/// <param name="args">Command line parameters.</param>
		/// <returns>True if successful.</returns>
		private static bool ParseCommandLine( string[] args )
		{
			bool status = false;
			
			if ( args.Length == 0 )
			{
				ShowUseage();
			}
			else
			{
				for ( int i = 0; i < args.Length; ++i )
				{
					switch ( args[i].ToLower() )
					{
						case "--user":
						{
							if ( ++i < args.Length )
							{
								username = args[i];
							}
							break;
						}

						case "--password":
						{
							if ( ++i < args.Length )
							{
								password = args[i];
							}
							break;
						}

						case "--first":
						{
							if ( ++i < args.Length )
							{
								first = args[i];
							}
							break;
						}

						case "--last":
						{
							if ( ++i < args.Length )
							{
								last = args[i];
							}
							break;
						}

						case "--full":
						{
							if ( ++i < args.Length )
							{
								full = args[i];
							}
							break;
						}

						case "--email":
						{
							if ( ++i < args.Length )
							{
								email = args[i];
							}
							break;
						}

						case "--url":
						{
							if ( ++i < args.Length )
							{
								url = args[i];
							}
							break;
						}
						
						case "--help":
						{
							ShowUseage();
							break;
						}

						default:
						{
							// Unknown command line option.
							Console.Error.WriteLine( "{0} is an invalid command line option.", args[ i ] );
							break;
						}
					}
				}
			}

			return status;
		}

		private static void ShowUseage()
		{
			Console.WriteLine();
			Console.WriteLine( "RegisterCmd is a command line utility to register a user" );
			Console.WriteLine( "with a Simias Server domain." );
			Console.WriteLine();
			Console.WriteLine( "Command line arguments:" );
			Console.WriteLine( "    --url <server url>" );
			Console.WriteLine( "        Mandatory argument" );
			Console.WriteLine( "        Url to the Simias server the user is registering with" );
			Console.WriteLine();
			Console.WriteLine( "    --user <username>" );
			Console.WriteLine( "        Mandatory argument" );
			Console.WriteLine( "        The username the caller wants for their account." );
			Console.WriteLine( "		User is the distinguishing property on the account." );
			Console.WriteLine();
			Console.WriteLine( "    --password <password>" );
			Console.WriteLine( "        Mandatory argument" );
			Console.WriteLine( "        Password to associate to the account" );
			Console.WriteLine();
			Console.WriteLine( "    --first <first name>" );
			Console.WriteLine( "        Optional argument" );
			Console.WriteLine( "        The user's first or given name." );
			Console.WriteLine();
			Console.WriteLine( "    --last <last name>" );
			Console.WriteLine( "        Optional argument" );
			Console.WriteLine( "        The user's last or family name." );
			Console.WriteLine();
			Console.WriteLine( "    --full <full name>" );
			Console.WriteLine( "        Optional argument" );
			Console.WriteLine( "        The user's full name." );
			Console.WriteLine();
			Console.WriteLine( "    --email <email address>" );
			Console.WriteLine( "        Optional argument" );
			Console.WriteLine( "        The user's primary email address." );
			Console.WriteLine();
			Console.WriteLine( "    --help" );
			Console.WriteLine( "        Displays this help." );
			Console.WriteLine();
		}
		
		
		public static void Main(string[] args)
		{
			if ( args.Length == 0 )
			{
				ShowUseage();
				return;
			}
			
			ParseCommandLine( args );
			
			if ( url == null || username == null || password == null )
			{
				Console.WriteLine( "missing mandatory command line arguments" );
				return;
			}
			
			// New up a web service proxy to register a user
			UserRegistration register = new UserRegistration();
			register.CookieContainer = new CookieContainer();
			
			string notEndingWith = @"/\:";
			register.Url = url.TrimEnd( notEndingWith.ToCharArray() );

			register.Url += "/simias10/Registration.asmx";
			
			//register.Credentials = myCred;
			//register.PreAuthenticate = true;
			//register.Proxy = ProxyState.GetProxyState( domainServiceUrl );

			// Make the web service call to register the user
			RegistrationInfo info =
				register.CreateUser(
					username, 
					password,
					null,
					first,
					last,
					full,
					null,
					email);
					
			if ( info.Status == RegistrationStatus.UserCreated )
			{
				Console.WriteLine( "successful" );
			}
			
			return;
		}
	}
}

