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
*                 $Author: banderso@novell.com 
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
		private int				maxWaitTime = ( 300 * 1000 );
		private int				waitTime = ( 5 * 1000 );
		
		private ArrayList domainList = new ArrayList();

		/// <summary>
		/// Constructor
		/// </summary>
		public Manager()
		{
			log.Debug( "Constructor called" );
			store = Store.GetStore();
		}

		/// <summary>
		/// Start the Watcher.
		/// </summary>
		public void Start()
		{
			log.Debug( "Start called" );

			try
			{
				lock(this)
				{
					if (started == false)
					{
						this.watcherThread = new Thread(new ThreadStart(this.WatcherThread));
						this.watcherThread.IsBackground = true;
						this.watcherThread.Priority = ThreadPriority.BelowNormal;
                        this.watcherThread.Name = "Domain Watcher Thread";
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
			log.Debug( "Stop called" );
			try
			{
				lock( this )
				{
					// Set state and then signal the event
					this.stop = true;
					this.stopEvent.Set();
					Thread.Sleep( 32 );
				}
			}
			catch( Exception e )
			{
				log.Error( e, "Unable to stop Domain Watcher." );
				throw e;
			}
			log.Debug( "Stop exit" );
		}

		/// <summary>
		/// Domain Watcher Thread.
		/// </summary>
		public void WatcherThread()
		{
			log.Debug( "WatcherThread started" );
			EventPublisher cEvent = new EventPublisher();
			Simias.Authentication.Status authStatus = null;

			// Let the caller know we're good to go
			this.started = true;

			do
			{
				//
				// Cycle through the domains
				//

				try
				{
					//ICSList domainList = store.GetDomainList();
					foreach( ShallowNode shallowNode in store.GetDomainList() )
					{
						Domain cDomain = store.GetDomain( shallowNode.ID );
						log.Debug("In domain watcher: domain id: {0} and wait time is {1}", shallowNode.ID, waitTime);
					
						// Make sure this domain is a slave since we don't watch
						// mastered domains.
						if ( cDomain.Role == SyncRoles.Slave )
						{
							DomainAgent domainAgent = new DomainAgent();

							log.Debug( "Checking domain: " + cDomain.Name );

							// Skip this domain if it is inactive
							if ( domainAgent.IsDomainActive( cDomain.ID ) == false )
							{
								log.Debug( "  domain: " + cDomain.Name + " - is inactive" );
								continue;
							}
							
							// Skip this domain if it's already authenticated
							if ( domainAgent.IsDomainAuthenticated( cDomain.ID ) )
							{
								log.Debug( "  domain: " + cDomain.Name + " - is authenticated" );
								continue;
							}
							
							// If the domain is default, attempt to get a full credential
							// set which under the covers will attempt to get it from
							// CASA.
							if ( store.DefaultDomain == cDomain.ID )
							{
								log.Debug( "  domain is default - getting credentials" );
								
								Member member = cDomain.GetCurrentMember();
								BasicCredentials basicCredentials =
									new BasicCredentials( 
											cDomain.ID,
											cDomain.ID,
											member.Name );
											
								if ( basicCredentials.Cached == true )
								{
									// We have credentials for this domain so
									// attempt to login
									
									log.Debug( "  yes - we have credentials for default" );
									log.Debug( "  username: " + member.Name );
									
									try
									{
										authStatus =
											domainAgent.Login(
												cDomain.ID,
												member.Name,
												basicCredentials.Password );
									}
									catch( WebException we )
									{
										log.Debug( "  failed authentication. " );
										log.Debug( we.Message );
										
										if ( we.Status == WebExceptionStatus.TrustFailure )
										{
											// The certificate is invalid. Force the client to
											// login thru a UI so the user can make a decision
											cEvent.RaiseEvent(
												new Simias.Client.Event.NotifyEventArgs(
														"Domain-Up",
														cDomain.ID,
														System.DateTime.Now ) );
											continue;
										}
									}
								
									if ( authStatus != null )
									{
										if ( authStatus.statusCode == SCodes.Success ||
												authStatus.statusCode == SCodes.SuccessInGrace )
										{
											log.Debug( "  successful authentication to the default domain" );
											
											if ( authStatus.statusCode == SCodes.SuccessInGrace )
											{
												log.Debug( "  BUT - we're in grace" );
											
												// Tell the clients we authenticated successfully
												// but that we're in a grace period
											
												cEvent.RaiseEvent(
													new Simias.Client.Event.NotifyEventArgs(
															"in-grace-period",
															cDomain.ID,
															System.DateTime.Now ) );
											}
											
											continue;
										}
									}
								}
								else
								{
									log.Debug( "  no credentials for the default domain - bummer" );
								}
							}
							
							if ( domainAgent.IsDomainAutoLoginEnabled( cDomain.ID ) == false )
							{
								log.Debug( "  domain: " + cDomain.Name + " auto-login is disabled" );
								continue;
							}
					/*
						The try-catch section below fixes the exception on the thick-clients during login. Wait for the member node to be synced to be synced before trying to ping the domain. 
					*/
                            /*
                             * Commenting out this code since the member node sync happens immediately...
                             * Fix for bug #341552. 
							try
							{
								string userID = store.GetUserIDFromDomainID(shallowNode.ID);
								Member m = cDomain.GetMemberByID(userID);
								log.Debug("trying to get the member id: home server is: {0}", m.HomeServer.ID);
							}
							catch(Exception ex)
							{
								continue;
							}
                             */ 
							log.Debug( "  attempting to ping the domain" );
							if ( domainAgent.Ping( cDomain.ID ) == true )
							{
								log.Debug( "  domain up" );
								cEvent.RaiseEvent(
									new Simias.Client.Event.NotifyEventArgs(
											"Domain-Up",
											cDomain.ID,
											System.DateTime.Now ) );
							}
							else
							{
								log.Debug( "  domain down" );							
							}
						}
					}
				}
				catch( Exception e )
				{
					log.Error( "Exception at DomainWatcherThread(): {0}",e.Message );
					log.Error( "Exception at DomainWatcherThread(): {0}",e.StackTrace );
				}

				if ( this.stop == false )
				{
					stopEvent.WaitOne( waitTime, false );
					waitTime = ( waitTime * 2 < maxWaitTime ) ? waitTime * 2 : maxWaitTime;
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
