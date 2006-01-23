/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
using System.Collections;
using System.IO;
using System.Threading;
using System.Web;

using Simias.Client;
using Simias.Storage;

namespace Simias.IdentitySync
{
	public enum MemberStatus
	{
		Created,
		Updated,
		Unchanged,
		Disabled,
		Deleted
	}
	
	///
	/// <summary>
	/// Class used to maintain state across an external identity
	/// synchronization cycle.
	/// </summary>
	///
	public class State
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		internal Property syncGuid;
		private Store store;	
		private int disabled;
		private int deleted;
		private int reportedErrors;
		private ArrayList reportedMessages;
		private int processed;
		//private string lastMember;
		public Domain domain;
		#endregion
		
		#region Properties
		public Domain SDomain
		{
			get { return domain; }
		}
		
		public int Processed
		{
			get { return processed; }
		}
		
		public int Errors
		{
			get { return reportedErrors; }
		}
		
		public int Deleted
		{
			get { return deleted; }
		}
		
		public int Disabled
		{
			get { return disabled; }
		}
		
		public Property SyncGuid
		{
			get { return syncGuid; }
		}
		#endregion
		
		#region Constructors
		public State( string DomainID )
		{
			store = Store.GetStore();
			domain = store.GetDomain( DomainID );
			if ( domain == null )
			{
				throw new ArgumentException( "DomainID" );
			}
			
			syncGuid = new Property( "SyncGuid", Guid.NewGuid().ToString() );
			syncGuid.LocalProperty = true;
			
			reportedMessages = new ArrayList();
			reportedErrors = 0;
			processed = 0;
		}
		
		#endregion
		
		#region Public Methods
		/// <summary>
		/// External sync providers must call this method after
		/// retrieving member information from the external identity store.
		/// An in-memory object should be filled out with valid UserID, Name,
		/// Given, Last, FN and DN properties.
		/// Name is the distinguishing property valided against the
		/// domain.
		///
		/// Note: the sync provider should never commit the member
		/// object to the domain.
		/// </summary>
		public void ProcessedMember( Member member, MemberStatus Status )
		{
			log.Debug( "  processed: " + member.Name + " status: " + Status.ToString() );
			member.Properties.ModifyProperty( syncGuid );
			domain.Commit( member );
			
			processed++;		
		}
		
		public void ReportError( string ErrorMsg )
		{
			reportedErrors++;
			reportedMessages.Add( ErrorMsg );
		}

		/*		
		public void UpdateMember( Member member )
		{
			return;
		}
		*/
		#endregion
	}
	
	/// <summary>
	/// Class that implements the identity sync provider functionality.
	/// </summary>
	public class Service
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Table used to keep track of provider mappings.
		/// </summary>
		//static private Hashtable providerTable = new Hashtable();

		/// <summary>
		/// List that holds the registered providers.
		/// </summary>
		static private Hashtable registeredProviders = new Hashtable();
		
		static private IIdentitySyncProvider current = null;
		static AutoResetEvent syncEvent = null;
		static bool running = false;
		static bool quit;
		static bool syncOnStart = true;
		static int syncInterval = 30 * 1000;
		static int daysDisabled = 5;
		static Thread syncThread = null;
		
		static string disabledAtProperty = "IdentitySync:DisabledAt";
		
		#endregion

		#region Properties
		/// <summary>
		/// Gets the number of registered providers.
		/// </summary>
		static public int Count
		{
			get { return registeredProviders.Count; }
		}

		/// <summary>
		/// Returns the registered identity providers.
		/// </summary>
		static public IIdentitySyncProvider[] Providers
		{
			get
			{
				IIdentitySyncProvider[] providers = new IIdentitySyncProvider[ registeredProviders.Count ];
				lock ( typeof( IdentitySync.Service ) )
				{
					registeredProviders.CopyTo( providers, 0 );
				}
				return providers;
			}
		}
		#endregion

		#region Private Methods
		
		// If the user is removed from the domain scope, his POBox
		// should get removed from the system rather than orphaned
		private static void DeletePOBox( IdentitySync.State State, Member Zombie )
		{
			try
			{
				Store store = Store.GetStore();
				ICSList cList = store.GetCollectionsByOwner( Zombie.UserID );
				foreach( ShallowNode sn in cList )
				{
					Collection c = new Collection( store, sn );
					if ( ( c.Domain == State.SDomain.ID ) && 
						( (Node) c).IsBaseType( NodeTypes.POBoxType ) )
					{
						c.Commit( c.Delete() );
						
						Property dn = Zombie.Properties.GetSingleProperty( "DN" );
						string userName = ( dn.Value != null ) ? dn.Value as string : Zombie.Name;
						log.Info(
							String.Format(
								"Removed {0}'s POBox from Domain: {1}", 
								userName,
								State.SDomain.Name ) );
								
						break;
					}
				}
			}
			catch( Exception e2 )
			{
				log.Error( e2.Message );
				log.Error( e2.StackTrace );
			}
		}

		/// <summary>
		/// Method to orphan all collections where
		/// where Zombie is the owner of the collection.
		/// Ownership of orphaned collections is assigned over to the
		/// domain administrator.  The previous owner's DN is saved in
		/// the "OrphanedOwner" property on the collection.
		/// </summary>
		private
		static
		void OrphanCollections( IdentitySync.State State, Member Zombie )
		{
			string dn =	Zombie.Properties.GetSingleProperty( "DN" ).Value as string;
			if ( dn == null || dn == "" )
			{
				dn = Zombie.Name;
			}

			Store store = Store.GetStore();
			ICSList cList = store.GetCollectionsByOwner( Zombie.UserID );
			foreach ( ShallowNode sn in cList )
			{
				// Remove the user as a member of this collection.
				Collection c = new Collection( store, sn );

				// Only look for collections from the specified domain and
				// don't allow this user's membership removed from the domain.
				if ( ( c.Domain == State.SDomain.ID ) && 
					!( (Node) c).IsBaseType( NodeTypes.DomainType ) &&
					!( (Node) c).IsBaseType( NodeTypes.POBoxType ) )
				{
					Member member = c.GetMemberByID( Zombie.UserID );
					if (member != null && member.IsOwner == true )
					{
						// Don't remove an orphaned collection.
						if ( ( member.UserID != State.SDomain.Owner.UserID ) ) 
						{
							//
							// The desired IT behavior is to orphan all collections
							// where the zombie user is the owner of the collection.
							// Policy could dictate and force the collection deleted at
							// a later time but the job of the sync code is to 
							// orphan the collection and assign the Simias admin as
							// the new owner.
							//

							// Simias Admin must be a member first before ownership
							// can be transfered
							Member adminMember = 
								c.GetMemberByID( State.SDomain.Owner.UserID );
							if ( adminMember == null )
							{
								adminMember = 
									new Member( 
											State.SDomain.Owner.Name, 
											State.SDomain.Owner.UserID, 
											Simias.Storage.Access.Rights.Admin );
									c.Commit( adminMember );
							}

							Property prevProp = new Property( "OrphanedOwner", dn );
							prevProp.LocalProperty = true;
							c.Properties.ModifyProperty( prevProp );
							c.Commit();

							c.Commit( c.ChangeOwner( adminMember, Simias.Storage.Access.Rights.Admin ) );

							// Now remove the old member
							c.Commit( c.Delete( c.Refresh( member ) ) );
								
							log.Info(
								String.Format(
									"Orphaned Collection: {0} - previous owner: {1}", 
									c.Name,
									dn ));
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Method to remove membership from all collections that
		/// the zombie user is a member of.
		/// This method does not handle the case where the zombie
		/// user is the owner of a collection.
		/// </summary>
		private
		static
		void RemoveMemberships( IdentitySync.State State, Member Zombie )
		{
			Store store = Store.GetStore();
			
			// Get all of the collections that this user is member of.
			ICSList cList = store.GetCollectionsByUser( Zombie.UserID );
			foreach ( ShallowNode sn in cList )
			{
				// Remove the user as a member of this collection.
				Collection c = new Collection( store, sn );

				// Only look for collections from the specified domain and
				// don't allow this user's membership removed from the domain itself.
				if ( ( c.Domain == State.SDomain.ID ) && 
					!( (Node) c).IsBaseType( NodeTypes.DomainType ) )
				{
					Member member = c.GetMemberByID( Zombie.UserID );
					if (member != null && member.IsOwner == false )
					{
						// Not the owner, just remove the membership.
						c.Commit( c.Delete( member ) );
						Property dn = Zombie.Properties.GetSingleProperty( "DN" );
						string userName = ( dn.Value != null ) ? dn.Value as string : Zombie.Name; 
						log.Info(
							String.Format(
								"Removed {0}'s membership from Collection: {1}", 
								userName,
								c.Name ) );
					}
				}
			}
		}
		
		private static void ProcessDeletedMembers( IdentitySync.State State )
		{
			// check for deleted members
			log.Debug( "Checking for deleted members" );

			try
			{
				Property syncGUID = State.SyncGuid;
				
				ICSList	deleteList = 
					State.SDomain.Search( "SyncGuid", syncGUID.Value, SearchOp.Not_Equal );
				foreach( ShallowNode cShallow in deleteList )
				{
					Node cNode = new Node( State.SDomain, cShallow );
					if ( cNode.IsType( "Member" ) == true )
					{	
						Member cMember = new Member( cNode );
						string distinguishedName = 
							cMember.Properties.GetSingleProperty( "DN" ).Value.ToString();

						// See if this account has been previously disabled
						if ( State.SDomain.IsLoginDisabled( cMember.UserID ) == true )
						{
							// Did the sync service disable the account?
							Property p = cMember.Properties.GetSingleProperty( disabledAtProperty );
							if ( p != null )
							{
								DateTime dt = (DateTime) p.Value;

								// OK, this guy has been disabled past
								// the policy time so delete him from the
								// domain roster
								if ( dt.AddDays( Service.daysDisabled ) < DateTime.Now )
								{
									DeletePOBox( State, cMember );
									OrphanCollections( State, cMember );
									RemoveMemberships( State, cMember );

									// gather info before commit
									string fn = cMember.Name;
									string id = cMember.ID;

									State.SDomain.Commit( State.SDomain.Delete( cNode ) );

									log.Info(
										String.Format(
										"Removed DN: {0} FN: {1} ID: {2} from Domain: {3}", 
										distinguishedName,
										fn,
										id,
										State.SDomain.Name ) );
								}
								
								continue;
							}
						}
						
						log.Debug( " disabling member: " + cMember.Name );
						State.SDomain.SetLoginDisabled( cMember.UserID, true );
	
						Property disable = new Property( disabledAtProperty, DateTime.Now );
						disable.LocalProperty = true;
						cMember.Properties.ModifyProperty( disable );
						State.SDomain.Commit( cMember );
					}
				}
			}
			catch( Exception e1 )
			{	
				log.Debug( "Exception checking/deleting members" );
				log.Debug( e1.Message );
				log.Debug( e1.StackTrace );
			}
		}
		#endregion

		#region Public Methods
		
		/// <summary>
		/// Method for registering external synchronization providers
		/// </summary>
		/// <param name="provider">An ILocationProvider interface object.</param>
		static public void Register( IIdentitySyncProvider provider )
		{
			lock ( typeof( IdentitySync.Service ) )
			{
				log.Debug( "Registering provider {0}.", provider.Name );
				registeredProviders.Add( provider.Name, provider );
			}
		}

		/// <summary>
		/// Method for registering external synchronization providers
		/// </summary>
		/// <param name="provider">An ILocationProvider interface object.</param>
		static public void Unregister( IIdentitySyncProvider provider )
		{
			lock ( typeof( IdentitySync.Service ) )
			{
				log.Debug( "Unregistering provider {0}.", provider.Name );
				registeredProviders.Remove( provider.Name );
			}
		}
		
		/// <summary>
		/// Force a synchronization cycle immediately
		/// </summary>
		public static int SyncNow( string data )
		{
			log.Debug( "SyncNow called" );
			if ( running == false )
			{
				log.Debug( "  synchronization service not running" );
				return -1;
			}
			
			syncEvent.Set();
			log.Debug( "SyncNow finished" );
			return 0;
		}
		
		/// <summary>
		/// Starts the external identity sync container
		/// </summary>
		/// <returns>N/A</returns>
		static public void Start( )
		{
			if ( running == true )
			{
				log.Debug( "Identity sync service is already running" );
				return;
			}
			
			log.Debug( "Start - called" );
			quit = false;
			syncEvent = new AutoResetEvent( false );
			syncThread = new Thread( new ThreadStart( SyncThread ) );
			syncThread.IsBackground = true;
			syncThread.Start();
		}

		/// <summary>
		/// Stops the external identity sync container
		/// </summary>
		/// <returns>N/A</returns>
		static public void Stop( )
		{
			log.Debug( "Stop called" );
			quit = true;
			try
			{
				syncEvent.Set();
				Thread.Sleep( 32 );
				log.Debug( "Stop finished" );
			}
			catch(Exception e)
			{
				log.Debug( "failed with an exception" );
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
			
			return;
		}
		
		/// <summary>
		/// long term synchronization thread
		/// responsible for enforcing sync cycles
		/// and calling the sync providers.
		/// </summary>
		/// <returns>N/A</returns>
		private static void SyncThread()
		{
			log.Debug( "SyncThread - starting" );
			log.Debug( "  waiting for providers to load" );
			
			syncEvent.WaitOne( 1000 * 10, false );
			while ( quit == false )
			{
				running = true;
				if ( syncOnStart == false )
				{
					syncEvent.WaitOne( syncInterval, false );
					if ( quit == true )
					{
						continue;
					}
				}
				
				log.Debug( "Start - syncing identities" );
				Simias.IdentitySync.State state = null; 
				try
				{
					// Create a state object which is passed to the providers
					//Simias.Storage.Domain enterprise = new Simias.Server.Domain( false );
					state = new Simias.IdentitySync.State( Store.GetStore().DefaultDomain );

 					// Cycle thru the providers.
					foreach( IIdentitySyncProvider prov in registeredProviders.Values )
					{
						current = prov;
						current.Start( state );
						current = null;
					}
					
					if ( state.Errors == 0 )
					{
						ProcessDeletedMembers( state );											
					}
				}
				catch( Exception ex )
				{
					log.Error( ex.Message );
					log.Error( ex.StackTrace );
				}
				
				// Always wait after the first iteration
				syncOnStart = false;
				log.Debug( "Stop - syncing identities" );
			}
			
			syncEvent.Close();
			syncThread = null;
			running = false;
		}
		
		#endregion
	}
}
