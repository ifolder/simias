/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright � Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.IO;
using System.Threading;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Event;

namespace Simias.Web
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : HttpApplication
	{
		#region Class Members
		/// <summary>
		/// Object used to manage the simias services.
		/// </summary>
		static private Simias.Service.Manager serviceManager;
		#endregion

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
			Console.Error.WriteLine("Simias Application Path: {0}", Environment.CurrentDirectory);

            Simias.Storage.Store.GetStore();

            if (serviceManager == null)
            {
                serviceManager = new Simias.Service.Manager(Configuration.GetConfiguration());
            }
			
			Console.Error.WriteLine("Simias Process Starting");
			serviceManager.StartServices();
			serviceManager.WaitForServicesStarted();

			// Send the simias up event.
			EventPublisher eventPub = new EventPublisher();
			eventPub.RaiseEvent( new NotifyEventArgs("Simias-Up", "The simias service is running", DateTime.Now) );
			Console.Error.WriteLine("Simias Process Running");
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
            Console.Error.WriteLine("Simias Process Starting Shutdown");

            // Send the simias down event and wait for 1/2 second for the message to be routed.
			EventPublisher eventPub = new EventPublisher();
			eventPub.RaiseEvent( new NotifyEventArgs("Simias-Down", "The simias service is terminating", DateTime.Now) );
			Thread.Sleep( 500 );

			serviceManager.StopServices();
			serviceManager.WaitForServicesStopped();
			serviceManager = null;
			Console.Error.WriteLine("Simias Process Shutdown");
		}
	}
}

