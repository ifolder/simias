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
*                 $Author: Russ Young
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
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Xml;
using System.Threading;
using System.Reflection;
using Simias;
using Simias.Event;
using Simias.Storage;
using Simias.Client;
using Simias.Client.Event;

namespace Simias.Service
{
	/// <summary>
	/// System Manager
	/// </summary>
	public class Manager : IEnumerable
	{
		#region fields

		/// <summary>
		/// Used to log service events.
		/// </summary>
		static public ISimiasLog logger = Simias.SimiasLogManager.GetLogger(typeof(Manager));

		private static Manager instance;

		static internal string XmlAssemblyAttr = "assembly";
		static internal string XmlEnabledAttr = "enabled";
		static internal string XmlNameAttr = "name";
		static internal string MutexBaseName = "ServiceManagerMutex___";

		private Thread startThread = null;
		private Thread stopThread = null;
		private ArrayList serviceList = new ArrayList();

		private const string ModulesDirectory = "modules";
		private const string ServiceConfigFiles = "*.conf";

		private ManualResetEvent servicesStarted = new ManualResetEvent(false);
		private ManualResetEvent servicesStopped = new ManualResetEvent(true);
		private DefaultSubscriber subscriber = null;
		
		private static bool shuttingDown = false;
	
		private static bool ldapserviceenabled = false;

		private static bool usermoveserviceenabled = false;

		private static bool runasserver = false;

		/// <summary>
		/// Ldap service name 
		/// </summary>
		private static string LdapModuleName = "IdentityManagement";

		/// <summary>
		/// Ldap service name 
		/// </summary>
		private static string UserMovementModuleName = "UserMovement";

		private static string SimiasServerModuleName = "Simias.Server";


		#endregion

		#region Events
		/// <summary>
		/// Delegate to handle Shutdown events.
		/// </summary>
		public event ShutdownEventHandler Shutdown;
		
		#endregion
		
		#region Constructor

		/// <summary>
		/// Creates a Manager.
		/// </summary>
		private Manager()
		{
			// configure
			SimiasLogManager.Configure(Store.StorePath);

			// Get an event subscriber to handle shutdown events.
			subscriber = new DefaultSubscriber();
			subscriber.SimiasEvent +=new SimiasEventHandler(OnSimiasEvent);

			lock (this)
			{
				// Add the Change Log Service and Local Domain Service by default.
				serviceList.Add( new ThreadServiceCtl( "Simias Change Log Service", "SimiasLib", "Simias.Storage.ChangeLog" ) );
				serviceList.Add( new ThreadServiceCtl( "Simias Local Domain Provider", "SimiasLib", "Simias.LocalProvider" ) );

				// Check if there is an overriding modules directory in the simias data area.
				string[] confFileList = null;
				string modulesDir = Path.Combine( Store.StorePath, ModulesDirectory );
				if ( Directory.Exists( modulesDir ) )
				{
					confFileList = Directory.GetFiles( modulesDir, ServiceConfigFiles );
				}

				// Check if there are any service configuration files.
				if ( ( confFileList == null ) || ( confFileList.Length == 0 ) )
				{
					// There is no overriding directory, use the default one.
					modulesDir = SimiasSetup.modulesdir;
					confFileList = Directory.GetFiles( modulesDir, ServiceConfigFiles );
				}

				// Get all of the configuration files from the modules directory.
				foreach ( string confFile in confFileList )
				{
					string assembly = Path.GetFileNameWithoutExtension( confFile );
					XmlDocument confDoc = new XmlDocument();
					confDoc.Load( confFile );

					if(String.Compare(assembly, LdapModuleName) == 0)
						ldapserviceenabled = true;
					if(String.Compare(assembly, UserMovementModuleName) == 0)
						usermoveserviceenabled = true;

					// Get the XmlElement for the Services.
					foreach ( XmlElement serviceNode in confDoc.DocumentElement )
					{
						serviceList.Add( new ThreadServiceCtl( assembly, serviceNode ) );
					}
				}
			}

			// reset the log4net configurations after a lock on Flaim was obtained
			SimiasLogManager.ResetConfiguration();
		}

		#endregion

		#region Factory Method
		/// <summary>
		/// Gets the static instance of the Manager object.
		/// </summary>
		/// <returns>The static instance of the Manager object.</returns>
		static public Manager GetManager()
		{
			lock(typeof(Manager))
			{
				if (instance == null)
				{
					instance = new Manager();
				}

				return instance;
			}
		}
		#endregion

		#region Callbacks

        /// <summary>
        /// Callback for handling simias event received
        /// </summary>
        /// <param name="args">Simias even details</param>
		private void OnSimiasEvent(SimiasEventArgs args)
		{
			try
			{
				string typeString = args.GetType().ToString();
				switch (typeString)
				{
					case "Simias.Service.ShutdownEventArgs":
						if (Shutdown != null)
							Shutdown((ShutdownEventArgs)args);
						break;
				}
			}
			catch (Exception ex)
			{
				new SimiasException(args.ToString(), ex);
			}
		}

		#endregion

		#region Message handling

        /// <summary>
        /// Dispatching the message
        /// </summary>
        /// <param name="msg">Message details</param>
		private void messageDispatcher(Message msg)
		{
			ServiceCtl	svcCtl = msg.service;
			try
			{
				lock (this)
				{
					switch (msg.MajorMessage)
					{
						case MessageCode.Start:
							if ( (svcCtl.State == Simias.Service.State.Stopped) && svcCtl.Enabled)
							{
								try
								{
									svcCtl.Start();
								}
								catch(Exception ex)
								{
									if( RunAsServer == false || String.Compare(ex.Message, "Failed to create Flaim DB.") != 0 )
										throw ex;
									else
									{
										logger.Debug("Exception Message {0} recieved", ex.Message);
										logger.Debug("Exiting from the current process, as process failed to Open FLAIM Database ");
										StopServices();
										Environment.Exit(-1);
									}
								}
							}
							break;
						case MessageCode.Stop:
							if (svcCtl.state == State.Running || svcCtl.state == State.Paused)
							{
								svcCtl.Stop();
							}
							break;
						case MessageCode.Pause:
							if (svcCtl.state == State.Running)
							{
								svcCtl.Pause();
							}
							break;
						case MessageCode.Resume:
							if (svcCtl.state == State.Paused)
							{
								svcCtl.Resume();
							}
							break;
						case MessageCode.Custom:
							svcCtl.Custom(msg.CustomMessage, msg.Data);
							break;
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, ex.Message);
			}
		}
		
		#endregion

		#region Control Methods.
		/// <summary>
		/// Start the installed services.
		/// This call is asynchronous. Use ServicesStarted to now when this call has finished.
		/// </summary>
		public void StartServices()
		{
			lock (this)
			{
				startThread = new Thread(new ThreadStart(StartServicesThread));
                startThread.Priority = ThreadPriority.BelowNormal;
                startThread.Name = "Simias Manager Start";
                startThread.IsBackground = true;
				startThread.Start();
			}
		}

        /// <summary>
        /// Start services Thread
        /// </summary>
		private void StartServicesThread()
		{
			foreach (ServiceCtl svc in this)
			{
				if (svc.State == State.Stopped)
				{
					messageDispatcher(new StartMessage(svc));
				}
			}
			servicesStopped.Reset();
			servicesStarted.Set();
			logger.Info("Services started.");
			startThread = null;
		}

		/// <summary>
		/// Stop the installed services.
		/// This call is asynchronous. Use ServicesStarted to know when this call has finished.
		/// </summary>
		public void StopServices()
		{
			// Set the global static variable so other services can get an
			// advance warning that we're shutting down and they can stop
			// processing.
			shuttingDown = true;

			lock (this)
			{
				stopThread = new Thread(new ThreadStart(StopServicesThread));
                stopThread.Priority = ThreadPriority.BelowNormal;
                stopThread.IsBackground = true;
                stopThread.Name = "Simias Manager Stop";
				stopThread.Start();
			}
		}

        /// <summary>
        /// Stop services thread
        /// </summary>
		private void StopServicesThread()
		{
			// Set that the database is being shut down so that no more changes can be made.
			logger.Info("The database is being shut down.");
			Store.GetStore().ShutDown();

			for (int i = serviceList.Count; i > 0; --i)
			{
				ServiceCtl svc = (ServiceCtl)serviceList[i-1];
				messageDispatcher(new StopMessage(svc));
			}
			servicesStarted.Reset();
			servicesStopped.Set();
			logger.Info("Services stopped.");
			stopThread = null;
		}
		
		/// <summary>
		/// Block until services are started.
		/// </summary>
		public void WaitForServicesStarted()
		{
			servicesStarted.WaitOne();
		}

		/// <summary>
		/// Block until services are stoped.
		/// </summary>
		public void WaitForServicesStopped()
		{
			servicesStopped.WaitOne();
		}

/*		/// <summary>
		/// Ldap Assebly name
		/// </summary>
		public static string LdapAssemblyName()
		{
			return LdapModuleName;
		}
*/
		/// <summary>
		/// Get the named ServiceCtl object.
		/// </summary>
		/// <param name="name">The name of the service to get.</param>
		/// <returns>The ServiceCtl for the service.</returns>
		public static ServiceCtl GetService(string name)
		{
			lock (instance)
			{
				foreach (ServiceCtl svc in instance)
				{
					if (svc.Name.Equals(name))
						return svc;
				}
				return null;
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the started state of the services.
		/// </summary>
		public bool ServiceStarted
		{
			get { return servicesStarted.WaitOne(0, false); }
		}

		/// <summary>
		/// Ldap service enable/disable status 
		/// </summary>
		public static bool LdapServiceEnabled
		{
			get { return ldapserviceenabled; }
			set { ldapserviceenabled = value; }
		}

		/// <summary>
		/// user move service enable/disable status 
		/// </summary>
		public static bool UserMoveServiceEnabled
		{
			get { return usermoveserviceenabled; }
			set { usermoveserviceenabled = value; }
		}

		/// <summary>
		/// This instance is running as server or client
		/// </summary>
		public bool RunAsServer
		{
			get { return runasserver; }
			set { runasserver = value; }
		}

		/// <summary>
		/// Ldap Assebly name
		/// </summary>
		public static string LdapAssemblyName
		{
			get {return LdapModuleName; }
			set {LdapModuleName = value; }
		}

		/// <summary>
		/// User movement Assebly name
		/// </summary>
		public static string UserMovementAssemblyName
		{
			get {return UserMovementModuleName; }
			set {UserMovementModuleName = value; }
		}

		/// <summary>
		/// Catalog/Simias.Server Assebly name
		/// </summary>
		public static string CatalogAssemblyName
		{
			get {return SimiasServerModuleName; }
			set {SimiasServerModuleName = value; }
		}

		/// <summary>
		/// Get the status of services stopped
		/// </summary>
		public bool ServicesStopped
		{
			get { return servicesStopped.WaitOne(0, false); }
		}
		
		/// <summary>
		/// Allows other threads in the process know when Simias is shutting
		/// down so that they can also stop processing.
		/// </summary>
		public static bool ShuttingDown
		{
			get { return shuttingDown; }
		}
		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Gets the enumerator for the ServiceCtl objects.
		/// </summary>
		/// <returns>An enumerator.</returns>
		public IEnumerator GetEnumerator()
		{
			return serviceList.GetEnumerator();
		}

		#endregion
	}

	#region Delegate Definitions.

	/// <summary>
	/// Delegate definition for handling shutdown events.
	/// </summary>
	public delegate void ShutdownEventHandler(ShutdownEventArgs args);

	#endregion

	/// <summary>
	/// Event class for shutdown requests.
	/// </summary>
	[Serializable]
	public class ShutdownEventArgs : SimiasEventArgs
	{
	}
}
