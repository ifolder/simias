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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Event;
using Simias.Storage;

namespace Simias.Web
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : HttpApplication
	{
		#region Class Members

		/// <summary>
		/// Environment variables used by apache.
		/// </summary>
		private static string EnvSimiasRunAsServer = "SimiasRunAsServer";
		private static string EnvSimiasDataPath = "SimiasDataPath";
		private static string EnvSimiasVerbose = "SimiasVerbose";
		
		/// <summary>
		/// Object used to manage the simias services.
		/// </summary>
		static private Simias.Service.Manager serviceManager;
		
		/// <summary>
		/// A thread to keep the application alive long enough to close Simias.
		/// </summary>
		private Thread keepAliveThread;

		/// <summary>
		/// Quit the application flag.
		/// </summary>
		private bool quit;

		/// <summary>
		/// Specifies whether to run as a client or server.
		/// </summary>
		private static bool runAsServer = false;

		/// <summary>
		/// Prints extra data for debugging purposes.
		/// </summary>
		private static bool verbose = false;

		/// <summary>
		/// Path to the simias data area.
		/// </summary>
		private static string simiasDataPath = null;

		/// <summary>
		/// Port used as an IPC between application domains.
		/// </summary>
		private static int ipcPort;

		/// <summary>
		/// Port used to talk local service. This will be (-1) if running
		/// in an enterprise configuration.
		/// </summary>
		private static int localServicePort = -1;

		#endregion

		#region Constructor

		/// <summary>
		/// Static constructor that starts only one shutdown thread.
		/// </summary>
		static Global()
		{
			// Check to see if there is an environment variable set to run as a server.
			// If so then we are being started by apache and there are no command line
			// parameters available.
			if ( !ParseEnvironmentVariables() )
			{
				// Parse the command line parameters.
				ParseConfigurationParameters( Environment.GetCommandLineArgs() );
			}

			// Make sure that there is a data path specified.
			if ( simiasDataPath == null )
			{
				ApplicationException apEx = new ApplicationException( "The Simias data path was not specified." );
				Console.Error.WriteLine( apEx.Message );
				throw apEx;
			}

			if ( verbose )
			{
				Console.Error.WriteLine("Simias Application Path: {0}", Environment.CurrentDirectory);
				Console.Error.WriteLine("Simias Data Path: {0}", simiasDataPath);
				Console.Error.WriteLine("Run in {0} configuration", runAsServer ? "server" : "client" );
				Console.Error.WriteLine("Local service port = {0}", localServicePort );
			}

			// Initialize the store.
			Store.Initialize( simiasDataPath, runAsServer, localServicePort );
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Parses the command line parameters to get the configuration for Simias.
		/// </summary>
		/// <param name="args">Command line parameters.</param>
		private static void ParseConfigurationParameters( string[] args )
		{
			for ( int i = 0; i < args.Length; ++i )
			{
				switch ( args[ i ].ToLower() )
				{
					case "--runasserver":
					{
						runAsServer = true;
						break;
					}

					case "--datadir":
					{
						if ( ( i + 1 ) < args.Length )
						{
							simiasDataPath = args[ ++i ];
						}
						else
						{
							ApplicationException apEx = new ApplicationException( "Error: The Simias data path was not specified." );
							Console.Error.WriteLine( apEx.Message );
							throw apEx;
						}

						break;
					}

					case "--port":
					{
						if ( ( i + 1 ) < args.Length )
						{
							localServicePort = Convert.ToInt32( args[ ++i ] );
						}
						else
						{
							ApplicationException apEx = new ApplicationException( "Error: The local service port was not specified." );
							Console.Error.WriteLine( apEx.Message );
							throw apEx;
						}

						break;
					}

					case "--ipcport":
					{
						if ( ( i + 1 ) < args.Length )
						{
							ipcPort = Convert.ToInt32( args[ ++i ] );
						}
						else
						{
							ApplicationException apEx = new ApplicationException( "Error: The IPC port was not specified." );
							Console.Error.WriteLine( apEx.Message );
							throw apEx;
						}

						break;
					}

					case "--verbose":
					{
						verbose = true;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Gets the Simias environment variables set by mod-mono-server in the apache process.
		/// </summary>
		/// <returns>True if the environment variables were found. Otherwise false is returned.</returns>
		private static bool ParseEnvironmentVariables()
		{
			bool foundEnv = false;

			if ( Environment.GetEnvironmentVariable( EnvSimiasRunAsServer ) != null )
			{
				runAsServer = true;
				simiasDataPath = Environment.GetEnvironmentVariable( EnvSimiasDataPath ).Trim( new char [] { '\"' } );
				if ( simiasDataPath == null )
				{
					ApplicationException apEx = new ApplicationException( "Error: The Simias data path was not specified." );
					Console.Error.WriteLine( apEx.Message );
					throw apEx;
				}

				verbose = ( Environment.GetEnvironmentVariable( EnvSimiasVerbose ) != null ) ? true : false;
				foundEnv = true;
			}

			return foundEnv;
		}

		/// <summary>
		/// Sends the specified message via the IPC to the listening process.
		/// </summary>
		/// <param name="message">Message to send.</param>
		private static void SendIpcMessage( string message )
		{
			// Put the message into a length preceeded buffer.
			UTF8Encoding utf8 = new UTF8Encoding();
			int msgLength = utf8.GetByteCount( message );
			byte[] msgHeader = BitConverter.GetBytes( msgLength );
			byte[] buffer = new byte[ msgHeader.Length + msgLength ];

			// Copy the message length and the message into the buffer.
			msgHeader.CopyTo( buffer, 0 );
			utf8.GetBytes( message, 0, message.Length, buffer, 4 );

			// Allocate a socket to send the shutdown message on.
			Socket socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			try
			{
				// Connect to the listening client on the specifed ipc port.
				socket.Connect( new IPEndPoint( IPAddress.Loopback, ipcPort ) );
				socket.Send( buffer );
				socket.Shutdown( SocketShutdown.Send );
				socket.Close();
			}
			catch
			{}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Application_Start
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Start(Object sender, EventArgs e)
		{
			// update the prefix of the installed directory
			SimiasSetup.prefix = Path.Combine(Server.MapPath(null), "..");
			Environment.CurrentDirectory = SimiasSetup.webbindir;

			if ( verbose )
			{
				Console.Error.WriteLine("Simias Process Starting");
			}

			serviceManager = Simias.Service.Manager.GetManager();
			serviceManager.StartServices();
			serviceManager.WaitForServicesStarted();

			// Send the simias up event.
			EventPublisher eventPub = new EventPublisher();
			eventPub.RaiseEvent( new NotifyEventArgs("Simias-Up", "The simias service is running", DateTime.Now) );

			if ( verbose )
			{
				Console.Error.WriteLine("Simias Process Running");
			}

			// start keep alive
			// NOTE: We have seen a FLAIM corruption because the database was not given
			// the opportunity to close on shutdown.  The solution is to work with
			// Dispose() methods with the native calls and to control our own life cycle
			// (which in a web application is difficult). It would mean separating the
			// database application from the web application, which is not very practical
			// today.  We can bend the rules in the web application by using a foreground
			// thread. This is a brittle solution, but it seems to work today.
			keepAliveThread = new Thread(new ThreadStart(this.KeepAlive));
			quit = false;
			keepAliveThread.Start();
		}
 
		/// <summary>
		/// Keep Alive Thread
		/// </summary>
		protected void KeepAlive()
		{
			TimeSpan span = TimeSpan.FromSeconds(1);
			
			while(!quit)
			{
				Thread.Sleep(span);
			}
		}

		/// <summary>
		/// Session_Start
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Session_Start(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Application_BeginRequest
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_BeginRequest(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Application_EndRequest
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_EndRequest(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Application_AuthenticateRequest
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Application_Error
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Error(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Session_End
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Session_End(Object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Application_End
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_End(Object sender, EventArgs e)
		{
			if ( verbose )
			{
				Console.Error.WriteLine("Simias Process Starting Shutdown");
			}

			if ( serviceManager != null )
			{
				// Send the simias down event and wait for 1/2 second for the message to be routed.
				EventPublisher eventPub = new EventPublisher();
				eventPub.RaiseEvent( new NotifyEventArgs("Simias-Down", "The simias service is terminating", DateTime.Now) );
				Thread.Sleep( 500 );

				serviceManager.StopServices();
				serviceManager.WaitForServicesStopped();
				serviceManager = null;
			}

			if ( verbose )
			{
				Console.Error.WriteLine("Simias Process Shutdown");
			}

			// end keep alive
			// NOTE: an interrupt or abort here is currently causing a hang on Mono
			quit = true;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Causes the controlling server process to shut down the web services.
		/// </summary>
		public static void SimiasProcessExit()
		{
			SendIpcMessage( "stop_server" );
		}

		#endregion
	}
}

