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

namespace User.Management
{	class Command
	{
		static private bool verbose = false;
		static private string username = null;
		static private string password = null;
		static private string first = null;
		static private string last = null;
		static private string email = null;
		static private string full = null;
		static private string url = "http://localhost:8086/simias10/iFolderAdmin.asmx";
		static private string action = null;
		static private string adminName = null;
		static private string adminPassword = null;
		static private string quota = null;
		
		/// <summary>
		/// Parses the command line parameters gathering user info
		/// for the Simias registration.
		/// </summary>
		/// <param name="args">Command line parameters.</param>
		/// <returns>True if successful.</returns>
		private static bool ParseCommandLine( string[] args )
		{
			bool status = false;
			
			if ( args.Length < 2 )
			{
				ShowUseage();
			}
			else
			{
				action = args[0].ToLower();
				
				for ( int i = 1; i < args.Length; ++i )
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
						
						case "--admin-name":
						{
							if ( ++i < args.Length )
							{
								adminName = args[i];
							}
							break;
						}
						
						case "--admin-password":
						{
							if ( ++i < args.Length )
							{
								adminPassword = args[i];
							}
							break;
						}
						
						case "--quota":
						{
							if ( ++i < args.Length )
							{
								quota = args[i];
							}
							break;
						}
						
						case "--help":
						{
							ShowUseage();
							break;
						}

						case "--verbose":
						{
							verbose = true;
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
			Console.WriteLine( "A command line utility to manage users in a Simias Server domain." );
			Console.WriteLine();
			Console.WriteLine( "Useage: UserCmd action <options>" );
//			Console.WriteLine();
			Console.WriteLine( "    action <create|delete|modify|list>");
			Console.WriteLine( "        Mandatory argument" );
			Console.WriteLine();
			Console.WriteLine( "    --url <server url>" );
			Console.WriteLine( "        Mandatory argument" );
			Console.WriteLine( "        Url to the Simias server the user is registering with" );
			Console.WriteLine();
			Console.WriteLine( "    --user <username>" );
			Console.WriteLine( "        Mandatory argument if action == create,delete,modify" );
			Console.WriteLine( "        The username the caller wants for their account." );
			Console.WriteLine( "		    username is the distinguishing property on the account." );
			Console.WriteLine();
			Console.WriteLine( "    --password <password>" );
			Console.WriteLine( "        Mandatory argument if action == create" );
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
			Console.WriteLine( "        Optional argument if action == create or modify" );
			Console.WriteLine( "        The user's primary email address." );
			Console.WriteLine();
			Console.WriteLine( "    --admin-name <Simias administrator username>" );
			Console.WriteLine( "        Mandatory for all actions" );
			Console.WriteLine();
			Console.WriteLine( "    --admin-password <Simias administrator password>" );
			Console.WriteLine( "        Mandatory for all actions" );
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
			
			if ( action == null || url == null )
			{
				Console.WriteLine( "missing mandatory command line arguments" );
				return;
			}

			if ( adminName == null )
			{
				Console.Write( "Please enter simias admin: " );
				adminName = Console.ReadLine();
				
				Console.Write( "password: " );
				adminPassword = Console.ReadLine();
			}
			
			// New up a web service proxy to administrate the system
			iFolderAdmin admin = new iFolderAdmin();
			admin.CookieContainer = new CookieContainer();
				
			// Build a credential from the admin name and password.
			admin.Credentials = new NetworkCredential( adminName, adminPassword );
			admin.PreAuthenticate = true;

			// Build a url to the admin web service
			string notEndingWith = @"/\:";
			admin.Url = url.TrimEnd( notEndingWith.ToCharArray() );
			admin.Url += "/simias10/iFolderAdmin.asmx";
				
			if ( action == "create" )
			{
				if ( username == null || password == null )
				{
					Console.WriteLine( "missing mandatory command line arguments" );
					return;
				}
				
				// Make the web service call to register/create the user
				RegistrationInfo info =
					admin.CreateUser(
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
			}
			else
			if ( action == "delete" )
			{
			}
			else
			if ( action == "modify" )
			{
				int total = 0;
				
				iFolderUser[] users =
					admin.GetUsersBySearch( SearchProperty.UserName, SearchOperation.BeginsWith, username, 0, 1, out total );

				if ( users != null )
				{
					bool changed = false;
					UserPolicy userPolicy = admin.GetUserPolicy( users[0].UserID );
					
					if ( quota != null )
					{
						if ( verbose == true )
						{
							Console.WriteLine( "Changing quota from {0} to {1}", userPolicy.SpaceLimit, Convert.ToInt64( quota ) );
						}
						userPolicy.SpaceLimit =  Convert.ToInt64( quota );
						changed = true;
					}

					if ( changed == true )
					{
						admin.SetUserPolicy( userPolicy );
					}
				}
			}
			else
			if ( action == "list" )
			{
				int total = 0;
				
				iFolderUser[] users =
					admin.GetUsers( 0, -1, out total );
					//admin.GetUsersBySearch( SearchProperty.UserName, SearchOperation.BeginsWith, "*", 0, 99999, out total );
					
				foreach( iFolderUser user in users )
				{
					Console.Write( "ID: {0}  ", user.UserID );
					Console.Write( "User: {0}  ", user.UserName );
					if ( user.FullName != null && user.FullName != "" )
					{
						Console.Write( "Fullname: {0}  ", user.FullName );
					}
					
					Console.Write( "Enabled: {0}  ", user.Enabled.ToString() );
					Console.WriteLine();
				}
			}
			else
			{
				Console.WriteLine( "Error: invalid action" );
			}
			
			return;
		}
	}
}

