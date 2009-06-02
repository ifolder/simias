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
*                 $Author: Brady Anderson <banderso@novell.com> 
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
using System.Web;
using System.Net;
using System.Web.Services.Protocols;

namespace User.Management
{	class Command
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
			
			if ( args.Length == 1 && args[0].ToLower() == "--help" )
			{
				action = "help";
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
							action = "help";
							return status;
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

    /// <summary>
    /// Display the usage of this utility
    /// </summary>
		private static void ShowUseage()
		{
			Console.WriteLine();
			Console.WriteLine( "A command line utility to manage users in a Simias Server domain." );
			Console.WriteLine( "UserCmd.exe action <options>" );
//			Console.WriteLine();
			Console.WriteLine( "    action <create|delete|modify|list|setpwd>");
			Console.WriteLine( "        Mandatory argument" );
			Console.WriteLine();
			Console.WriteLine( "    --url <server url>" );
			Console.WriteLine( "        Mandatory argument" );
			Console.WriteLine( "        Url to the Simias server the user is registering with" );
			Console.WriteLine();
			Console.WriteLine( "    --user <username>" );
			Console.WriteLine( "        Mandatory argument if action == create,delete,modify" );
			Console.WriteLine( "        The username the caller wants for their account." );
			Console.WriteLine( "        username is the distinguishing property on the account." );
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
		
		/// <summary>
		/// Main to start the execution
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args)
		{
			if ( args.Length == 0 )
			{
				Console.WriteLine( "A command line utility to manage users in a Simias Server domain." );
				Console.WriteLine( "UserCmd.exe action <options>" );
				return;
			}
			
			ParseCommandLine( args );
			
			if ( action == "help" )
			{
				ShowUseage();
				return;
			}
			
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
                iFolderUser user = null;
                
                try
                {
                    user = admin.CreateUser(
                        username, 
                        password,
                        null,
                        first,
                        last,
                        full,
                        null,
                        email);

					if ( user != null )
					{
						Console.WriteLine( "Successful" );
					}
				}
				catch( WebException we )
				{
					if ( we.Message.LastIndexOf( "401" ) != 0 )
					{
						Console.WriteLine( "Failed - Invalid admin credentials" );
					}
					else
					{
						Console.Write( "Failed - " );
						Console.WriteLine( we.Message );
					}
				}
				catch( Exception ex )
                {
					Console.WriteLine( "Failed creating {0}'s account", username );
                }
			}
			else
			if ( action == "delete" )
			{
				try
				{
					bool status = admin.DeleteUser( username );
					if ( verbose == true )
					{
						Console.WriteLine( "Deleting user {0}  Status {1)", username, status.ToString() );
					}
				}
				catch( WebException we )
				{
					if ( we.Message.LastIndexOf( "401" ) != 0 )
					{
						Console.WriteLine( "Failed - Invalid admin credentials" );
					}
					else
					{
						Console.Write( "Failed - " );
						Console.WriteLine( we.Message );
					}
				}
				catch( Exception ex )
				{
					Console.WriteLine( "Failed deleting {0}'s account", username );
				}
			}
			else
			if ( action == "modify" )
			{
				iFolderUser user = null;
				bool exception = false;
				try
				{
					user = admin.GetUser( username );
				} 
				catch( WebException we )
				{
					if ( we.Message.LastIndexOf( "401" ) != 0 )
					{
						Console.WriteLine( "Failed - Invalid admin credentials" );
					}
					else
					{
						Console.Write( "Failed - " );
						Console.WriteLine( we.Message );
					}
					exception = true;
				}
				catch( Exception ex )
				{
					Console.WriteLine( "Failed deleting {0}'s account", username );
					exception = true;
				}

				if ( user != null )
				{
					if ( quota != null )
					{
						bool changed = false;
						UserPolicy userPolicy = admin.GetUserPolicy( user.ID, null );
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

					if ( email != null || first != null || last != null || full != null )
					{
						if ( first != null )
						{
							user.FirstName = first;
						}

						if ( last != null )
						{
							user.LastName = last;
						}

						if ( full != null )
						{
							user.FullName = full;
						}

						if ( email != null )
						{
							user.Email = email;
						}

						// Write the changes
						admin.SetUser( user.ID, user );
					}

					if ( password != null )
					{
						bool status = admin.SetPassword( username, password );
						if ( verbose == true )
						{
							Console.WriteLine( "SetPassord for {0} - {1}", username, status.ToString() );
						}
					}
				}
				else if ( exception == false )
				{
					Console.WriteLine( "Failed - user {0} does not exist", username );
				}
			}
			else
			if ( action == "list" )
			{
				try
				{
					iFolderUserSet userSet = admin.GetUsers( 0, 0 );
					//admin.GetUsersBySearch( SearchProperty.UserName, SearchOperation.BeginsWith, "*", 0, 0 );
					
					foreach( iFolderUser user in userSet.Items )
					{
						Console.Write( "ID: {0}  ", user.ID );
						Console.Write( "User: {0}  ", user.UserName );
						if ( user.FullName != null && user.FullName != "" )
						{
							Console.Write( "Fullname: {0}  ", user.FullName );
						}
					
						Console.Write( "Enabled: {0}  ", user.Enabled.ToString() );
						Console.WriteLine();
					}
				}
				catch( WebException we )
				{
					if ( we.Message.LastIndexOf( "401" ) != 0 )
					{
						Console.WriteLine( "Failed - Invalid admin credentials" );
					}
					else
					{
						Console.Write( "Failed - " );
						Console.WriteLine( we.Message );
					}
				}
				catch( Exception ex )
				{
					Console.WriteLine( "Failed getting a list of users" );
				}
			}
			else
			if ( action == "setpwd" )
			{
				if ( username == null || username == String.Empty || password == null )
				{
					Console.WriteLine( "missing mandatory command line arguments" );
					return;
				}

				try
				{
					bool status = admin.SetPassword( username, password );
					Console.WriteLine( "SetPassord for {0} - {1}", username, status.ToString() );
				}
				catch( WebException we )
				{
					if ( we.Message.LastIndexOf( "401" ) != 0 )
					{
						Console.WriteLine( "Failed - Invalid admin credentials" );
					}
					else
					{
						Console.Write( "Failed - " );
						Console.WriteLine( we.Message );
					}
				}
				catch( Exception ex )
				{
					Console.WriteLine( "Failed setting {0}'s password", username );
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

