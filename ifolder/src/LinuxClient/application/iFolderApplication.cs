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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Authors:
 *		Calvin Gaisford <cgaisford@novell.com>
 *		Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

using Gtk;
using Gdk;
using Gnome;
using GtkSharp;
using GLib;
using Egg;
using Simias.Client.Event;
using Simias.Client;
using Simias.Client.Authentication;
using Novell.iFolder.Events;
using Novell.iFolder.Controller;


namespace Novell.iFolder
{
	public enum iFolderState : uint
	{
		Starting		= 0x0001,
		Stopping		= 0x0002,
		Running			= 0x0003,
		Stopped			= 0x0004
	}


	public class iFolderApplication : Gnome.Program
	{
		private static iFolderApplication	application = null;

		private Gtk.Image			gAppIcon;
		private Gdk.Pixbuf			RunningPixbuf;
		private Gdk.Pixbuf			StartingPixbuf;
		private Gdk.Pixbuf			StoppingPixbuf;
		private Gdk.PixbufAnimation	SyncingPixbuf;
		private Gtk.EventBox		eBox;
		private TrayIcon			tIcon;
		private iFolderWebService	ifws;
		private SimiasWebService	simws;
		private iFolderData			ifdata;

		private iFolderState 		CurrentState;
		private Gtk.ThreadNotify	iFolderStateChanged;
		private SimiasEventBroker	EventBroker;
		private	iFolderLoginDialog	LoginDialog;
//		private bool				logwinShown;

		private DomainController	domainController;
		private Manager				simiasManager;

		public iFolderApplication(string[] args)
			: base("ifolder", "1.0", Modules.UI, args)
		{
			Util.InitCatalog();

			tIcon = new TrayIcon("iFolder");

			eBox = new EventBox();
			eBox.ButtonPressEvent += 
				new ButtonPressEventHandler(trayapp_clicked);

			RunningPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder24.png"));
			StartingPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder-startup.png"));
			StoppingPixbuf = 
					new Pixbuf(Util.ImagesPath("ifolder-shutdown.png"));
			SyncingPixbuf =
					new Gdk.PixbufAnimation(Util.ImagesPath("ifolder24.gif"));

			gAppIcon = new Gtk.Image(RunningPixbuf);

			eBox.Add(gAppIcon);
			tIcon.Add(eBox);
			tIcon.ShowAll();	

			LoginDialog = null;

			iFolderStateChanged = new Gtk.ThreadNotify(
							new Gtk.ReadyEvent(OniFolderStateChanged));

			// Create the simias manager object and set any command line
			// configuration in the object.
			simiasManager = new Manager(args);

//			logwin = new LogWindow();
//			logwin.Destroyed += 
//					new EventHandler(LogWindowDestroyedHandler);
//			logwinShown = false;
		}


		static public iFolderApplication GetApplication()
		{
			return application;
		}


		public new void Run()
		{
			System.Threading.Thread startupThread = 
					new System.Threading.Thread(new ThreadStart(StartiFolder));
			startupThread.Start();


			base.Run();
		}



		private void StartiFolder()
		{
			bool simiasRunning = false;

			CurrentState = iFolderState.Starting;
			iFolderStateChanged.WakeupMain();

			if(ifws == null)
			{
				simiasManager.Start();

				string localServiceUrl = simiasManager.WebServiceUri.ToString();
				ifws = new iFolderWebService();
				ifws.Url = localServiceUrl + "/iFolder.asmx";
				LocalService.Start(ifws, simiasManager.WebServiceUri, simiasManager.DataPath);
				
				simws = new SimiasWebService();
				simws.Url = localServiceUrl + "/Simias.asmx";
				LocalService.Start(simws, simiasManager.WebServiceUri, simiasManager.DataPath);

				// wait for simias to start up
				while(!simiasRunning)
				{
					try
					{
						ifws.Ping();
						simiasRunning = true;
					}
					catch(Exception e)
					{
						simiasRunning = false;
					}

					// Wait and ping it again
					System.Threading.Thread.Sleep(10);
				}

				try
				{
					// Set up to have data ready for events
					ifdata = iFolderData.GetData(simiasManager);

					EventBroker = SimiasEventBroker.GetSimiasEventBroker(simiasManager);
					domainController = DomainController.GetDomainController();
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
					ifws = null;
				}
			}

			CurrentState = iFolderState.Running;
			iFolderStateChanged.WakeupMain();
		}




		private void StopiFolder()
		{
			CurrentState = iFolderState.Stopping;
			iFolderStateChanged.WakeupMain();

			try
			{
				if(EventBroker != null)
					EventBroker.Deregister();
				simiasManager.Stop();
			}
			catch(Exception e)
			{
				// ignore
				Console.WriteLine(e);
			}

			CurrentState = iFolderState.Stopped;
			iFolderStateChanged.WakeupMain();
		}

		private void OnDomainNeedsCredentialsEvent(object sender, DomainEventArgs args)
		{
			ReLogin(args.DomainID);
		}

		private void ReLogin(string domainID)
		{
			if (LoginDialog == null)
			{
				DomainInformation dom = domainController.GetDomain(domainID);
				if (dom != null)
				{
					LoginDialog =
						new iFolderLoginDialog(dom.ID, dom.Name, dom.MemberName);

					LoginDialog.Response +=
						new ResponseHandler(OnReLoginDialogResponse);

					LoginDialog.ShowAll();
				}
			}
			else
			{
				LoginDialog.Present();
			}
		}

		private void OnReLoginDialogResponse(object o, ResponseArgs args)
		{
			switch (args.ResponseId)
			{
				case Gtk.ResponseType.Ok:
					Status status = 
						domainController.AuthenticateDomain(
							LoginDialog.Domain,
							LoginDialog.Password,
							LoginDialog.ShouldSavePassword);
					if (status == null ||
						(status.statusCode != StatusCodes.Success &&
						 status.statusCode != StatusCodes.SuccessInGrace))
					{
						Util.ShowLoginError(LoginDialog, status.statusCode);
					}
					else
					{
						// Login was successful so close the Login dialog
						LoginDialog.Hide();
						LoginDialog.Destroy();
						LoginDialog = null;
					}

					break;
				case Gtk.ResponseType.Cancel:
				case Gtk.ResponseType.DeleteEvent:
					// Prevent the auto login feature from being called again
					domainController.DisableDomainAutoLogin(LoginDialog.Domain);

					LoginDialog.Hide();
					LoginDialog.Destroy();
					LoginDialog = null;
					break;
			}
		}


		private void OniFolderFileSyncEvent(object o, FileSyncEventArgs args)
		{
			if (args == null || args.CollectionID == null || args.Name == null)
				return;	// Prevent an exception

			try
			{
				iFolderWindow ifwin = Util.GetiFolderWindow();
				if(ifwin != null)
					ifwin.HandleFileSyncEvent(args);

				LogWindow logwin = Util.GetLogWindow();
				if(logwin != null)
					logwin.HandleFileSyncEvent(args);
			}
			catch {}
		}




		private void OniFolderSyncEvent(object o, CollectionSyncEventArgs args)
		{
			if (args == null || args.ID == null || args.Name == null)
				return;	// Prevent an exception

			switch(args.Action)
			{
				case Action.StartSync:
				{
					gAppIcon.FromAnimation = SyncingPixbuf;
					break;
				}
				case Action.StopSync:
				{
					gAppIcon.Pixbuf = RunningPixbuf;
					break;
				}
			}

			try
			{
				iFolderWindow ifwin = Util.GetiFolderWindow();
				if(ifwin != null)
					ifwin.HandleSyncEvent(args);
	
				LogWindow logwin = Util.GetLogWindow();
				if(logwin != null)
					logwin.HandleSyncEvent(args);
			}
			catch {}
		}


		private void OniFolderAddedEvent(object o, iFolderAddedEventArgs args)
		{
			if (args == null || args.iFolderID == null)
				return;	// Prevent an exception
		
			// don't notify us of our own iFolders
			iFolderHolder ifHolder = ifdata.GetiFolder(args.iFolderID);

			if(!ifdata.IsCurrentUser(ifHolder.iFolder.OwnerID))
			{
				if(ifHolder.iFolder.IsSubscription &&
					(ClientConfig.Get(ClientConfig.KEY_NOTIFY_IFOLDERS, "true") 
						 == "true"))
				{
					NotifyWindow notifyWin = new NotifyWindow(
							tIcon, 
							string.Format(Util.GS("New iFolder \"{0}\""), 
								ifHolder.iFolder.Name),
							string.Format(Util.GS("{0} has invited you to participate in this shared iFolder"), ifHolder.iFolder.Owner),
							Gtk.MessageType.Info, 10000);
					notifyWin.ShowAll();
				}
			}

			iFolderWindow ifwin = Util.GetiFolderWindow();
			if(ifwin != null)
				ifwin.iFolderCreated(args.iFolderID);
		}




		private void OniFolderChangedEvent(object o, 
									iFolderChangedEventArgs args)
		{
			if (args == null || args.iFolderID == null)
				return;	// Prevent an exception
		
			// don't notify us of our own iFolders
			iFolderHolder ifHolder = ifdata.GetiFolder(args.iFolderID);

			// You can put the client into a state where we no longer have the iFolderHolder
			// around.  One way to do this is to create an iFolder, wait for it to start
			// synchronizing and while it's synchronizing, right-click, select "Delete",
			// and then answer, "Yes".  To prevent this from blowing up the client entirely
			// I'm adding in the following check for null.
			//
			// When the user deletes the iFolder, we remove it from the iFolderData
			// structure so when we receive this event, it would, naturally be null.
			if (ifHolder == null)
				return;

			// Also, just in case...
			if (ifHolder.iFolder == null)
				return;

			if(ifHolder.iFolder.IsSubscription)
			{
				iFolderWindow ifwin = Util.GetiFolderWindow();
				if(ifwin != null)
					ifwin.iFolderChanged(args.iFolderID);
			}
			else
			{
				// this may be called when a subscription gets updated
				if(ifHolder.iFolder.HasConflicts)
				{
					if(ClientConfig.Get(ClientConfig.KEY_NOTIFY_COLLISIONS, 
							"true") == "true")
					{
						NotifyWindow notifyWin = new NotifyWindow(
							tIcon, Util.GS("Action Required"),
							string.Format(Util.GS("A conflict has been detected in iFolder \"{0}\""), ifHolder.iFolder.Name),
							Gtk.MessageType.Info, 10000);
						notifyWin.ShowAll();
					}

					iFolderWindow ifwin = Util.GetiFolderWindow();
					if(ifwin != null)
						ifwin.iFolderHasConflicts(args.iFolderID);
				}
			}
		}




		private void OniFolderDeletedEvent(object o, 
									iFolderDeletedEventArgs args)
		{
			if (args == null || args.iFolderID == null)
				return;	// Prevent an exception
			
			iFolderWindow ifwin = Util.GetiFolderWindow();
			if(ifwin != null)
				ifwin.iFolderDeleted(args.iFolderID);
		}


		private void OniFolderUserAddedEvent(object o,
									iFolderUserAddedEventArgs args)
		{
			if (args == null || args.iFolderID == null || args.iFolderUser == null)
				return;	// Prevent an exception
			
			if(ClientConfig.Get(ClientConfig.KEY_NOTIFY_USERS, "true") 
							== "true")
			{
				string username;
				iFolderHolder ifHolder = ifdata.GetiFolder(args.iFolderID);

				if( (args.iFolderUser.FN != null) &&
					(args.iFolderUser.FN.Length > 0) )
					username = args.iFolderUser.FN;
				else
					username = args.iFolderUser.Name;

				NotifyWindow notifyWin = new NotifyWindow(
					tIcon, Util.GS("New iFolder User"), 
					string.Format(Util.GS("{0} has joined the iFolder \"{1}\""), username, ifHolder.iFolder.Name),
					Gtk.MessageType.Info, 10000);

				notifyWin.ShowAll();
			}
							
			// TODO: update any open windows?
//			if(ifwin != null)
//			ifwin.NewiFolderUser(ifolder, newuser);
		}




		// ThreadNotify Method that will react to a fired event
		private void OniFolderStateChanged()
		{
			switch(CurrentState)
			{
				case iFolderState.Starting:
					gAppIcon.Pixbuf = StartingPixbuf;
					break;

				case iFolderState.Running:
					if(EventBroker != null)
					{
						EventBroker.iFolderAdded +=
							new iFolderAddedEventHandler(
												OniFolderAddedEvent);
						EventBroker.iFolderChanged +=
							new iFolderChangedEventHandler(
												OniFolderChangedEvent);
						EventBroker.iFolderDeleted +=
							new iFolderDeletedEventHandler(
												OniFolderDeletedEvent);
						EventBroker.iFolderUserAdded +=
							new iFolderUserAddedEventHandler(
												OniFolderUserAddedEvent);
						EventBroker.CollectionSyncEventFired +=
							new CollectionSyncEventHandler(
												OniFolderSyncEvent);
						EventBroker.FileSyncEventFired +=
							new FileSyncEventHandler(
												OniFolderFileSyncEvent);
					}
					
					if (domainController != null)
					{
						domainController.DomainNeedsCredentials +=
							new DomainNeedsCredentialsEventHandler(OnDomainNeedsCredentialsEvent);
					}

					gAppIcon.Pixbuf = RunningPixbuf;

					// Bring up the accounts dialog if there are no domains
					Gtk.Timeout.Add(500, new Gtk.Function(PromptIfNoDomains));

					break;

				case iFolderState.Stopping:
					gAppIcon.Pixbuf = StoppingPixbuf;
					break;

				case iFolderState.Stopped:
					// Start up a thread that will guarantee we completely
					// exit after 2 seconds.
					//ThreadPool.QueueUserWorkItem(new WaitCallback(GuaranteeShutdown));
					System.Threading.Thread th = new System.Threading.Thread (new System.Threading.ThreadStart (GuaranteeShutdown));
					th.IsBackground = true;
					th.Start ();

					Application.Quit();
					break;
			}
		}
		
		static private void GuaranteeShutdown()
		{
			// Sleep for 2 seconds and if the process is still
			// running, we'll kill it.  If the process stops appropriately
			// before this is finished sleeping, this thread will terminate
			// before forcing a shutdown anyway.
			System.Threading.Thread.Sleep(2000);
			System.Environment.Exit(1);
		}

		private bool PromptIfNoDomains()
		{
			DomainInformation[] domains = simws.GetDomains(false);
			if (domains.Length < 1)
			{
				// Prompt the user about there not being any domains
				iFolderWindow ifwin = Util.GetiFolderWindow();
				iFolderMsgDialog dg = new iFolderMsgDialog(
					ifwin,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					"",
					Util.GS("Set up an iFolder account?"),
					Util.GS("To begin using iFolder, you must first set up an iFolder account."));
				int rc = dg.Run();
				dg.Hide();
				dg.Destroy();
				if (rc == -8)
				{
					showPrefsPage(1);
				}
			}

			return false;	// Prevent this from being called over and over by Gtk.Timeout
		}

		private void trayapp_clicked(object obj, ButtonPressEventArgs args)
		{
			// Prevent the trayapp context menu from showing if we're
			// starting up or shutting down.
			if(CurrentState == iFolderState.Starting ||
				CurrentState == iFolderState.Stopping)
				return;

			switch(args.Event.Button)
			{
				case 1: // first mouse button
					if(args.Event.Type == Gdk.EventType.TwoButtonPress)
					{
						showiFolderWindow(obj, args);
					}
					break;
				case 2: // second mouse button
					break;
				case 3: // third mouse button
					show_tray_menu();
					break;
			}
		}




		private void show_tray_menu()
		{
//			AccelGroup agrp = new AccelGroup();
			Menu trayMenu = new Menu();


			MenuItem iFolders_item = 
					new MenuItem (Util.GS("iFolders"));
			trayMenu.Append (iFolders_item);
			iFolders_item.Activated += 
					new EventHandler(showiFolderWindow);


			MenuItem accounts_item = 
					new MenuItem (Util.GS("Accounts"));
			trayMenu.Append (accounts_item);
			accounts_item.Activated += 
					new EventHandler(show_accounts);


			MenuItem logview_item = 
					new MenuItem (Util.GS("Synchronization Log"));
			trayMenu.Append (logview_item);
			logview_item.Activated += 
					new EventHandler(showLogWindow);

			trayMenu.Append(new SeparatorMenuItem());



			ImageMenuItem prefs_item = new ImageMenuItem (
											Util.GS("Preferences"));
			prefs_item.Image = new Gtk.Image(Gtk.Stock.Preferences,
											Gtk.IconSize.Menu);
			trayMenu.Append(prefs_item);
			prefs_item.Activated += 
					new EventHandler(show_preferences);


			ImageMenuItem help_item = new ImageMenuItem (
											Util.GS("Help"));
			help_item.Image = new Gtk.Image(Gtk.Stock.Help,
											Gtk.IconSize.Menu);
			trayMenu.Append(help_item);
			help_item.Activated += 
					new EventHandler(show_help);


			ImageMenuItem about_item = new ImageMenuItem (
											Util.GS("About"));
			about_item.Image = new Gtk.Image(Gnome.Stock.About,
											Gtk.IconSize.Menu);
			trayMenu.Append(about_item);
			about_item.Activated += 
					new EventHandler(show_about);

			
/*			if( (ifSettings != null) && (!ifSettings.HaveEnterprise) )
			{
				MenuItem connect_item = 
						new MenuItem (Util.GS("Join Enterprise Server"));
				trayMenu.Append (connect_item);
				connect_item.Activated += new EventHandler(OnJoinEnterprise);
			}

			if(!ShowReLoginWindow)
			{
				MenuItem show_relogin =
						new MenuItem(Util.GS("Login to iFolder Server"));
				trayMenu.Append(show_relogin);
				show_relogin.Activated += new EventHandler(OnShowReLogin);
			}
*/

			trayMenu.Append(new SeparatorMenuItem());

			ImageMenuItem quit_item = new ImageMenuItem (
											Util.GS("Quit"));
			quit_item.Image = new Gtk.Image(Gtk.Stock.Quit,
											Gtk.IconSize.Menu);
			trayMenu.Append(quit_item);
			quit_item.Activated += 
					new EventHandler(quit_ifolder);


			trayMenu.ShowAll();

			trayMenu.Popup(null, null, null, IntPtr.Zero, 3, 
					Gtk.Global.CurrentEventTime);
		}




		private void quit_ifolder(object o, EventArgs args)
		{
			Util.CloseiFolderWindows();

			if(CurrentState == iFolderState.Stopping)
			{
				System.Environment.Exit(1);
			}
			else
			{
				System.Threading.Thread stopThread = 
					new System.Threading.Thread(new ThreadStart(StopiFolder));
				stopThread.Start();
			}
		}




		private void show_help(object o, EventArgs args)
		{
			Util.ShowHelp("front.html", null);
		}



		private void show_about(object o, EventArgs args)
		{
			Util.ShowAbout();
		}



		private void show_preferences(object o, EventArgs args)
		{
			showPrefsPage(0);
		}




		private void show_accounts(object o, EventArgs args)
		{
			showPrefsPage(1);
		}

		private void showPrefsPage(int page)
		{
			Util.ShowPrefsPage(page, simiasManager);
		}

		private void showiFolderWindow(object o, EventArgs args)
		{
			Util.ShowiFolderWindow(simiasManager);
		}

		private void showLogWindow(object o, EventArgs args)
		{
			Util.ShowLogWindow();
		}

		public static void Main (string[] args)
		{
			Process[] processes = 
				System.Diagnostics.Process.GetProcessesByName("iFolderClient");

			if(processes.Length > 1)
			{
				Console.WriteLine("iFolder is already running!");
				return;
			}

			try
			{
				application = new iFolderApplication(args);
				application.Run();
			}
			catch(Exception bigException)
			{
				Console.WriteLine(bigException);
				iFolderCrashDialog cd = new iFolderCrashDialog(bigException);
				cd.Run();
				cd.Hide();
				cd.Destroy();
				cd = null;
				Application.Quit();
			}
		}
	}
}
