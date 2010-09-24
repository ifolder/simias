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
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/


using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using System.Diagnostics;
using System.Web.SessionState;
using System.Security.Cryptography.X509Certificates;

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
		private static string EnvSimiasRunAsClient = "SimiasRunAsClient";
		private static string EnvSimiasDataDir = "SimiasDataDir";
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
		private static bool runAsServer = true;

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
			// The presedence for settings is
			// 1. Command Line
			// 2. Environment
			// 3. defaults.conf (in etc)

			ReadDefaultsConfig();
			ParseEnvironmentVariables();
			ParseConfigurationParameters( Environment.GetCommandLineArgs() );


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

			// Check the current datadir, if it's not setup then copy the bootstrap files there
			// only do this if we are starting up as a server.
			if( runAsServer )
			{
				Setup_Datadir();
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
					case "--runasclient":
					{
						runAsServer = false;
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
							ApplicationException apEx = new ApplicationException( "Error: The Simias data dir was not specified." );
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
		private static void ParseEnvironmentVariables()
		{
			string tmpPath;
			if( Environment.GetEnvironmentVariable( EnvSimiasRunAsClient ) != null )
			{
				runAsServer = false;
			}

			tmpPath = Environment.GetEnvironmentVariable( EnvSimiasDataDir );
			if( tmpPath != null )
			{
				simiasDataPath = tmpPath.Trim( new char [] { '\"' } );
			}

			if( Environment.GetEnvironmentVariable( EnvSimiasVerbose ) != null )
			{
				verbose = true;
			}
		}


		/// <summary>
		/// Gets the default settings from the /etc/defaults.config file
		/// </summary>
		private static void ReadDefaultsConfig()
		{
			runAsServer = !Simias.Defaults.RunsAsClient;
			simiasDataPath = Simias.Defaults.SimiasDataDir;
			//verbose = Simias.Defaults.Verbose;
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
#if WINDOWS
			// update the prefix of the installed directory
			// but only if we are on windows
			SimiasSetup.prefix = Path.Combine(Server.MapPath(null), "..\\..\\..");
#endif
			Environment.CurrentDirectory = SimiasSetup.webbindir;

			if ( verbose )
			{
				Console.Error.WriteLine("Simias Process Starting");
			}

			if (runAsServer )
			{
				string certString = System.Configuration.ConfigurationSettings.AppSettings.Get( "SimiasCert" );

				if(certString != null && certString != "" && certString != String.Empty)
				{
					Console.Error.WriteLine("Certstring {0}", certString);
	                        // certificate policy
	        	                ServicePointManager.CertificatePolicy = new SingleCertificatePolicy(
        	        	                new X509Certificate(Convert.FromBase64String(System.Configuration.ConfigurationSettings.AppSettings.Get( "SimiasCert" ) )));
				}
			}

			serviceManager = Simias.Service.Manager.GetManager();
			serviceManager.RunAsServer = runAsServer;
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
            keepAliveThread.Name = "Keep Alive Thread";
            keepAliveThread.Priority = ThreadPriority.BelowNormal;
			quit = false;
			keepAliveThread.Start();
		}


		/// <summary>
		/// Setup_Datadir
		/// </summary>
		protected static void Setup_Datadir()
		{
			if(simiasDataPath == null)
			{
				if(verbose)
					Console.Error.WriteLine("SimiasDataPath was null, ignoring bootstrap process");
				return;
			}

			// if we don't have a bootstrap, nothing to do!
			if(!Directory.Exists(SimiasSetup.bootstrapdir))
			{
				if(verbose)
					Console.Error.WriteLine("Ignoring bootstrap process because file not found at : " + SimiasSetup.bootstrapdir);
				return;
			}

			if (!Directory.Exists(simiasDataPath))
			{
				if(verbose)
					Console.Error.WriteLine("Simias data path {0} does not exist", simiasDataPath);
				return;
			}

			string[] fileEntries = 
				Directory.GetFileSystemEntries(simiasDataPath);

			// if we find entries in our current
			// data path, bail out
			if(fileEntries.Length > 1)
			{
				if(verbose)
					Console.Error.WriteLine("Files found in exising simias data area... Ignoring bootstrap process");
				return;
			}

			// move all of the files
			fileEntries = Directory.GetFiles(SimiasSetup.bootstrapdir);
        	foreach(string fileName in fileEntries)
			{
				try
				{
					File.Copy(fileName, Path.Combine(simiasDataPath, 
								Path.GetFileName(fileName)) );
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("Error bootstraping file: " + fileName);
					Console.Error.WriteLine(e.Message);
				}
			}

			// move all of the dirs
			fileEntries = Directory.GetDirectories(SimiasSetup.bootstrapdir);
        	foreach(string fileName in fileEntries)
			{
				try
				{
					CopyDirectory(fileName, simiasDataPath);
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("Error bootstraping diretory: " + fileName);
					Console.Error.WriteLine(e.Message);
				}
			}
		}

		protected static void CopyDirectory(string src, string targetdir)
		{
			string dirName = Path.GetFileName(src);
			string targetName = Path.Combine(targetdir, dirName);

			DirectoryInfo di = new DirectoryInfo(targetdir);
			di.CreateSubdirectory(dirName);

			// move all of the files
			string[] fileEntries = Directory.GetFiles(src);
   	     	foreach(string fileName in fileEntries)
			{
				File.Copy(fileName, Path.Combine(targetName, 
								Path.GetFileName(fileName)) );
			}

			// move all of the dirs
			fileEntries = Directory.GetDirectories(src);
        	foreach(string fileName in fileEntries)
			{
				CopyDirectory(fileName, targetdir);
			}
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

			quit = true;
			// Exiting here immediately as delay in this exit usually means that the next process started will
			// will not get FLAIM Resources.
			if ( runAsServer )
			{
				int param = 0;
				KillStrayProcesses( param );
				Console.Error.WriteLine("ALL Simias Threads are stopped,  Exiting from Simias process");
				Environment.Exit(0);
			}
		}

		/// <summary>
		/// It should kill, if stray mono process for simias10 are still running after simias thread is stopped 
		/// Maximum number of retry to kill itself is 3
		/// </summary>
		private void KillStrayProcesses(int count)
		{
			if( count >= 3 )
				return;

			int ProcessID = -1;
			Process clsProcess = Process.GetCurrentProcess();
			
			ProcessModule MyProcessModule = clsProcess.MainModule;
			if( ( MyProcessModule.FileName ).IndexOf( SimiasSetup.prefix ) >= 0 )
			{
				// current process full path contains ifolder binary prefix
				try
				{
					ProcessID = clsProcess.Id;
					clsProcess.Kill();
					//If the process kills itself, then it should not execute any statement after Kill() .
				}
				catch
				{
					// Some exception during kill, proceed without throwing error
				}
			}
			try
			{
				// rare case, if the kill command did not kill itself, cross-check
				// If the process does not exist, then it throws exception

				Process RemainingProcess = Process.GetProcessById(ProcessID);
				int newcount = ++count;
				KillStrayProcesses(newcount);
			}catch(Exception ex){
				if(ex.Message.IndexOf("Can't find process") >= 0)
					return;
				}
	
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
        /// <summary>
        /// Certificate Problem
        /// </summary>
        internal enum CertificateProblem : long
        {
                CertEXPIRED                   = 0x800B0101,
                CertVALIDITYPERIODNESTING     = 0x800B0102,
                CertROLE                      = 0x800B0103,
                CertPATHLENCONST              = 0x800B0104,
                CertCRITICAL                  = 0x800B0105,
                CertPURPOSE                   = 0x800B0106,
                CertISSUERCHAINING            = 0x800B0107,
                CertMALFORMED                 = 0x800B0108,
                CertUNTRUSTEDROOT             = 0x800B0109,
                CertCHAINING                  = 0x800B010A,
                CertREVOKED                   = 0x800B010C,
                CertUNTRUSTEDTESTROOT         = 0x800B010D,
                CertREVOCATION_FAILURE        = 0x800B010E,
                CertCN_NO_MATCH               = 0x800B010F,
                CertWRONG_USAGE               = 0x800B0110,
                CertUNTRUSTEDCA               = 0x800B0112
        }

        /// <summary>
        /// Single Certificate Policy
        /// </summary>
        internal class SingleCertificatePolicy : ICertificatePolicy
        {
                X509Certificate certificateString;

                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="certificateString"></param>
                public SingleCertificatePolicy( X509Certificate certificateString )
                {
                        this.certificateString = certificateString;
                }

                /// <summary>
                /// Check Validation Result
                /// </summary>
                /// <param name="srvPoint"></param>
                /// <param name="certificate"></param>
                /// <param name="request"></param>
                /// <param name="certificateProblem"></param>
                /// <returns></returns>
                public bool CheckValidationResult( ServicePoint srvPoint,
                        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        WebRequest request,
                        int certificateProblem )
                {
                        bool result = false;

                        if ( ( certificateProblem == 0 ) || CertificateProblem.CertEXPIRED.Equals( certificateProblem ) ||
                                ( ( certificate != null ) && ( certificate.GetIssuerName().Equals( this.certificateString.GetIssuerName() ) ) )  )
                        {
                                result = true;
                        }

                        return result;
                }
        }

}

