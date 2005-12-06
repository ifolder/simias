/***********************************************************************
 *  $RCSfile: DomainWatcher.cs,v $
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
 *  Author: banderso@novell.com
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting;
using System.Threading;
using System.Web;

using Simias;
using Simias.Authentication;
using Simias.Client;
using Simias.DomainServices;

//using Simias.Client.Event;
using Simias.Event;
using Simias.Storage;
using Simias.Sync;

using Novell.Security.ClientPasswordManager;
using SCodes = Simias.Authentication.StatusCodes;

namespace Simias.DomainWatcher
{
	/// <summary>
	/// DomainWatcher Manager
	/// </summary>
	public class Manager : IDisposable
	{
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );
			
		private bool			started = false;
		private	bool			stop = false;
		private Thread			watcherThread = null;
		private AutoResetEvent	stopEvent;
		private Store			store;
		private int				waitTime = ( 30 * 1000 );
		private int				initialWait = ( 5 * 1000 );
		
		private class MDomain
		{
			public bool		Authenticated;
			public bool		UpSignal;
			
			public string	ID;
		}
		
		private ArrayList domainList = new ArrayList();

		/// <summary>
		/// Constructor
		/// </summary>
		public Manager()
		{
			// store
			store = Store.GetStore();
		}

		/// <summary>
		/// Start the Watcher.
		/// </summary>
		public void Start()
		{
			log.Debug("Start called");

			try
			{
				lock(this)
				{
					if (started == false)
					{
						this.watcherThread = new Thread(new ThreadStart(this.WatcherThread));
						this.watcherThread.IsBackground = true;
						this.watcherThread.Priority = ThreadPriority.BelowNormal;
						this.stopEvent = new AutoResetEvent(false);

						this.watcherThread.Start();
					}
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to start Domain Watcher thread.");
				throw e;
			}

			log.Debug("Start exit");
		}

		/// <summary>
		/// Stop the Domain Watcher Thread.
		/// </summary>
		public void Stop()
		{
			log.Debug("Stop called");
			try
			{
				lock(this)
				{
					// Set state and then signal the event
					this.stop = true;
					this.stopEvent.Set();
					Thread.Sleep(32);
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to stop Domain Watcher.");
				throw e;
			}
			log.Debug("Stop exit");
		}

		/// <summary>
		/// Domain Watcher Thread.
		/// </summary>
		public void WatcherThread()
		{
			log.Debug("WatcherThread started");
			bool finishedWithDomain;
			bool firstTime = true;
			EventPublisher cEvent = new EventPublisher();
			Simias.Authentication.Status authStatus = null;

			string userID;
			string credentials;

			// Let the caller know we're good to go
			this.started = true;

			do
			{
				//
				// Cycle through the domains
				//

				try
				{
					ICSList domainList = store.GetDomainList();
					foreach( ShallowNode shallowNode in domainList )
					{
						Domain cDomain = store.GetDomain( shallowNode.ID );
					
						// Make sure this domain is a slave since we don't watch
						// mastered domains.
						if ( cDomain.Role == SyncRoles.Slave )
						{
							Member cMember;
							log.Debug( "checking Domain: " + cDomain.Name );
							
							DomainAgent domainAgent = new DomainAgent();
							
							finishedWithDomain = false;
							foreach( MDomain mdomain in domainList )
							{
								if ( mdomain.ID == cDomain.ID )
								{
									if ( mdomain.Authenticated == true || mdomain.UpSignal == true )
									{
										finishedWithDomain = true;
										break;
									}
								}
							}
							
							if ( finishedWithDomain == true )
							{
								continue;
							}
							
							// If the domain is default attempt to get a full credential
							// set which under the covers will attempt to get it from
							// CASA.
							if ( store.DefaultDomain == cDomain.ID )
							{
								log.Debug( "  domain is default - getting credentials" );
								HttpBasicCredentials basicCredentials =
									new HttpBasicCredentials( 
											cDomain.ID,
											cDomain.ID,
											cDomain.GetCurrentMember().UserID,
											true);
											
								if ( basicCredentials.Cached == true )
								{
									// We have credentials for this domain so
									// attempt to login
									
									log.Debug( "  yes - we have credentials for default" );
									log.Debug( "  username: " + basicCredentials.Username );
									log.Debug( "  password: " + basicCredentials.Password );
									
									try
									{
										authStatus =
											domainAgent.Login(
												cDomain.ID,
												basicCredentials.Username,
												basicCredentials.Password );
									
									}
									catch( WebException we )
									{
										log.Debug( "  failed authentication. " );
										log.Debug( we.Message );
										
										if ( we.Status == WebExceptionStatus.TrustFailure )
										{
											// The certificate is invalid. Tell the client to login
											Simias.Client.Event.NotifyEventArgs cArg =
												new Simias.Client.Event.NotifyEventArgs(
														"Domain-Up",
														cDomain.ID,
														System.DateTime.Now );
	
											cEvent.RaiseEvent( cArg );// which will inform him of the invalid cert.
											continue;
										}
										
									}
								
									if ( authStatus != null )
									{
										if ( authStatus.statusCode == SCodes.Success )
										{
											log.Debug( "  successful authentication to the default domain" );
											MDomain mdomain = new MDomain();
											mdomain.ID = cDomain.ID;
											mdomain.Authenticated = true;
											mdomain.UpSignal = false;
											domainList.Add( mdomain );
											continue;
										}
										else
										if ( authStatus.statusCode == SCodes.SuccessInGrace )
										{
											// The certificate is invalid. Tell the client to login
											Simias.Client.Event.NotifyEventArgs cArg =
												new Simias.Client.Event.NotifyEventArgs(
														"Domain-Up",
														cDomain.ID,
														System.DateTime.Now);
		
											cEvent.RaiseEvent(cArg);// which will inform him of the invalid cert.
											
											MDomain mdomain = new MDomain();
											mdomain.ID = cDomain.ID;
											mdomain.Authenticated = false;
											mdomain.UpSignal = true;
											domainList.Add( mdomain );
											continue;
										}
									}
								}
								else
								{
									log.Debug( "  no credentials for the default domain - bummer" );
								}
							}
							

							try
							{
								if ( domainAgent.IsDomainActive( cDomain.ID ) == false )
								{
									log.Debug( "Domain: " + cDomain.Name + " is off-line" );
									continue;
								}
							}
							catch(WebException we)
							{
								if (we.Status == WebExceptionStatus.TrustFailure)
								{
									// The certificate is invalid. Tell the client to login
									Simias.Client.Event.NotifyEventArgs cArg =
										new Simias.Client.Event.NotifyEventArgs(
										"Domain-Up",
										cDomain.ID,
										System.DateTime.Now);

									cEvent.RaiseEvent(cArg);// which will inform him of the invalid cert.
								}
								throw we;
							}
							
							if ( domainAgent.IsDomainAutoLoginEnabled( cDomain.ID ) == false )
							{
								log.Debug( "Domain: " + cDomain.Name + " auto-login is disabled" );
								continue;
							}

							CredentialType credType =
								store.GetDomainCredentials( cDomain.ID, out userID, out credentials );

							// Don't set this credential in the cache.
							credentials = null;

							// Only basic type authentication is supported right now.
							if ( credType != CredentialType.Basic )
							{
								cMember = cDomain.GetCurrentMember();
							}
							else
							{
								cMember = cDomain.GetMemberByID( userID );
							}

							// Can we talk to the domain?
							// Check to see if a full set of credentials exist
							// for this domain

							NetCredential cCreds =
								new NetCredential(
									"iFolder",
									cDomain.ID,
									true,
									cMember.Name,
									credentials);

							Uri cUri = DomainProvider.ResolveLocation( cDomain.ID );
							NetworkCredential netCreds = cCreds.GetCredential( cUri, "BASIC" );
							if ( ( netCreds == null ) || firstTime )
							{
								bool raiseEvent = false;

								if ( netCreds == null )
								{
									authStatus =
										domainAgent.Login(
											cDomain.ID,
											Guid.NewGuid().ToString(),
											"12" );

									if ( authStatus.statusCode == SCodes.UnknownUser )
									{
										raiseEvent = true;
									}
								}
								else
								{
									raiseEvent = true;
								}

								if ( raiseEvent == true )
								{
									Simias.Client.Event.NotifyEventArgs cArg =
										new Simias.Client.Event.NotifyEventArgs(
										"Domain-Up",
										cDomain.ID,
										System.DateTime.Now);

									cEvent.RaiseEvent(cArg);
								}
							}
						}
					}
				}
				catch(Exception e)
				{
					log.Error(e.Message);
					log.Error(e.StackTrace);
				}

				if ( this.stop == false )
				{
					stopEvent.WaitOne( ( firstTime == true ) ? initialWait : waitTime, false );
					firstTime = false;
				}

			} while( this.stop == false );

			this.started = false;
			this.stopEvent.Close();
			this.stopEvent = null;
		}

		#region IDisposable Members
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Stop();
		}
		#endregion
	}
}
