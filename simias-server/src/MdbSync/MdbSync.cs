/***********************************************************************
 *  $RCSfile: MdbSync.cs,v $
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
using System.Collections;
using System.Reflection;
using System.Text;
using System.Threading;

using Simias;
using Simias.Storage;
using Simias.Client;
using Simias.POBox;

//using Novell.iFolder.Ldap;

namespace Simias.MdbSync
{
	[Serializable]
	public enum Status
	{
		Syncing,
		Sleeping,
		IdentityStoreConnectionFailure,
		IdentityStoreAuthenticationFailure,
		SyncThreadDown,
		ConfigurationError,
		InternalException
	}

    public class SyncThread
    {
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		static bool up;
		static Thread syncThread = null;
		static AutoResetEvent syncEvent = null;

		internal static Status syncStatus = Status.SyncThreadDown;
		internal static bool errorDuringSync;
		internal static Exception syncException;
		internal static DateTime lastSyncTime;

		internal static Property syncGUID;
		internal static Store store = null;
		internal static Domain domain = null;

		internal static int Start()
		{
			log.Debug( "StartMdbSyncThread called" );

			up = true;
			syncEvent = new AutoResetEvent( false );
			syncThread = new Thread( new ThreadStart( SyncThread.MonitorThread ) );
			syncThread.IsBackground = true;
			syncThread.Start();

			log.Debug( "StartMdbSyncThread finished" );
			return 0;
		}

		internal static int Stop()
		{
			log.Debug( "StopMdbSyncThread called" );
			up = false;
			try
			{
				syncEvent.Set();
				Thread.Sleep( 32 );
				syncEvent.Close();
				Thread.Sleep(0);

				syncStatus = Status.SyncThreadDown;
				log.Debug( "StopMdbSyncThread finished" );
				return 0;
			}
			catch( Exception e )
			{
				log.Error( "StopMdbSyncThread failed with an exception" );
				log.Error( e.Message );
			}
			return -1;
		}

		public static Status SyncImmediate(string data)
		{
			log.Debug( "SyncImmediate called" );
			syncEvent.Set();
			log.Debug( "SyncImmediate finished" );
			return syncStatus;
		}

		internal static void MonitorThread()
		{
			// Sync thread alive
			syncStatus = Status.Syncing;

			//
			// Get configuration info
			//

			DomainConfiguration domainConfig;
			bool syncOnStart = true;

			while ( up == true )
			{
				string[] domains = Simias.MdbSync.DomainConfiguration.GetDomains();
				foreach( string currentDomain in domains )
				{
					// Somebody tell us to go bye bye?
					if ( up == false )
					{
						continue;
					}
					
					domainConfig = new DomainConfiguration( currentDomain );
					log.Debug( "Current Domain: " + domainConfig.DomainName );
					
					if ( syncOnStart == false )
					{
						// If the previous sync produced an error
						// keep that status through the wait
						if ( syncStatus == Status.Syncing )
						{
							syncStatus = Status.Sleeping;
						}

						log.Debug( "Waiting on sync interval" );
						syncEvent.WaitOne( ( domainConfig.SyncInterval ), false );
						if ( up == false )
						{
							continue;
						}
					}
					else
					{
						// First time up let the system settle a bit
						log.Debug( "Waiting a bit for the system to settle" );
						syncStatus = Status.Sleeping;
						syncEvent.WaitOne( ( 10 * 1000 ), false );
						if ( up == false )
						{
							continue;
						}
					}

					syncStatus = Status.Syncing;

					// Always wait after the first iteration
					syncOnStart = false;
					
					log.Debug( "Starting MDB -> " + domainConfig.DomainName + " sync" );

					// Authenticate against MDB	
					Simias.MdbSync.EnumUsers enumUsers = null;
					Simias.MdbSync.Mdb mdb = null;
					Simias.MdbSync.User mdbUser = null;

					try
					{
						log.Debug( "Authenticating proxy user: " + domainConfig.ProxyUsername );
						mdb = new Simias.MdbSync.Mdb( domainConfig.ProxyUsername, domainConfig.ProxyPassword );
						Console.WriteLine( "MDB Handle: " + mdb.Handle.ToString() );
					}
					catch( Exception mdbEx )
					{
						log.Error( "Failed to authenticate to MDB.  User: " + domainConfig.ProxyUsername );
						log.Error( mdbEx.Message );
						syncStatus = Status.ConfigurationError;
						up = false;
						continue;
					}
					
					// For now default domain
					Domain domain = SyncThread.CreateSimiasDomain( domainConfig );
					if ( domain == null )
					{
						log.Debug( "Failed to get an instance of the MDB domain" );
						syncStatus = Status.ConfigurationError;
						continue;
					}

					//
					// Create a sync iteration guid which will be stamped
					// in all matching objects as a local property
					//

					syncGUID = new Property("SyncGuid", Guid.NewGuid().ToString());
					syncGUID.LocalProperty = true;
					errorDuringSync = false;

					try
					{
						string container = domainConfig.Containers[0];
						log.Debug( "  syncing container: " + container );
						enumUsers = new Simias.MdbSync.EnumUsers( mdb.Handle, container, false );
						enumUsers.Reset();
						while( enumUsers.MoveNext() == true )
						{
							mdbUser = enumUsers.Current as Simias.MdbSync.User;
							ProcessMdbUser( domain, mdbUser );
						}
					}
					catch( SimiasShutdownException s )
					{
						log.Error( s.Message );
						errorDuringSync = true;
						syncException = s;
						syncStatus = Status.SyncThreadDown;
					}
					catch(Exception e)
					{
						log.Error( e.Message );
						log.Error( e.StackTrace );
						errorDuringSync = true;
						syncException = e;
						syncStatus = Status.InternalException;
					}
					finally
					{
						/*
						if ( conn != null )
						{
							log.Debug( "Disconnecting Ldap connection" );
							conn.Disconnect();
							conn = null;
						}
						*/
					}	

					//
					// Check if any members have been removed from the directory
					//
	
					/*
					if ( errorDuringSync == false )
					{
						log.Debug( "Checking for deleted Mdb entries" );

						try
						{
							ICSList	deleteList = 
								LdapSync.domain.Search( "SyncGuid", syncGUID.Value, SearchOp.Not_Equal );
							foreach( ShallowNode cShallow in deleteList )
							{
								Node cNode = new Node( domain, cShallow );
								if (LdapSync.domain.IsType( cNode, "Member" ) == true )
								{	
									Member cMember = new Member( cNode );
									RemoveUsersPOBox( cMember );
									RemoveUserFromCollections( cMember );
	
									// Delete this sucker...
									log.Debug( "deleting: " + cNode.Name );
	
									// gather info before commit
									string ldapDN = 
										cMember.Properties.GetSingleProperty( "DN" ).Value.ToString();
									string fn = cMember.Name;
									string id = cMember.ID;
	
									LdapSync.domain.Commit( LdapSync.domain.Delete( cNode ) );

									log.Info(
										String.Format(
											"Removed CN: {0} FN: {1} ID: {2} from Domain: {3}", 
											ldapDN,
											fn,
											id,
											LdapSync.domain.Name ) );
								}
							}
						}
						catch( Exception e1 )
						{	
							log.Debug( "Exception checking/deleting members" );
							log.Debug( e1.Message );
							log.Debug( e1.StackTrace );
						}

						log.Debug( "Finished checking for deleted MDB entries" );
	
						// Successful sync without errors so
						// record the last sync time
						lastSyncTime = DateTime.Now;
					}
					*/

					log.Debug( "Finished MDB -> Simias.Domain sync" );
				}
			}

			log.Debug( "MdbSyncThread going down" );
		}
		
		/// <summary>
		/// Create a Simias Domain based on the domain configuration template.
		/// If the object already exists, an instance of the domain will be
		/// returned
		/// </summary>
		internal static Domain CreateSimiasDomain( DomainConfiguration domainConfig )
		{
			//
			//  Check if the Simias Server domain exists in the store
			//

			Simias.Storage.Domain domain = null;
			Store store = Store.GetStore();
			foreach( ShallowNode sNode in store.GetDomainList() )
			{
				domain = store.GetDomain( sNode.ID );
				if ( domainConfig.DomainName == domain.Name )
				{
					// BUGBUG Verify other MDB tags
					
					/*
					Property p = domain.Properties.GetSingleProperty( "MDB" );
					if ( p != null && (bool) p.Value == true )
					{
						mdbDomain = tmpDomain;
						this.id = tmpDomain.ID;
						break;
					}
					*/
					
					return domain;
				}
			}

			try
			{
				string id = ( domainConfig.DomainID != null ) 
					? domainConfig.DomainID : Guid.NewGuid().ToString();

				// Create the MDB based domain
				domain = 
					new Simias.Storage.Domain(
						store, 
						domainConfig.DomainName, 
						id,
						domainConfig.Description, 
						Simias.Sync.SyncRoles.Master, 
                        Simias.Storage.Domain.ConfigurationType.ClientServer );
                        
				// The "Enterprise" type must be set on domai in order for the
				// enterprise domain location provider to resolve it
				domain.SetType( domain, "Enterprise" );

				// Set the known tag for an MDB originated domain
				Property p = new Property( "ORIGIN:MDB", true );
				p.LocalProperty = true;
				domain.Properties.ModifyProperty( p );

				// Get the short name (CN) for the admin
				string adminCN = null;
				char[] delim = {'\\'};
				string[] comps = domainConfig.DomainAdmin.Split( delim );
				if ( comps.Length > 0 )
				{
					adminCN = comps[comps.Length - 1];
				}
					
				if ( adminCN == null )
				{
					adminCN = domainConfig.DomainAdmin;
				}
				
				// Create the owner member for the domain.
				Member member = 
					new Member( adminCN, Guid.NewGuid().ToString(), Access.Rights.Admin );
					
				member.IsOwner = true;

				// Set the DN of the administrator
				Property dn = new Property( "DN", domainConfig.DomainAdmin );
				dn.LocalProperty = true;
				member.Properties.ModifyProperty( dn );

				// Set the known tag for an MDB originated member
				Property mdb = new Property( "ORIGIN:MDB", true );
				mdb.LocalProperty = true;
				member.Properties.ModifyProperty( mdb );

				domain.Commit( new Node[] { domain, member } );

				// Create the name mapping.
				store.AddDomainIdentity( domain.ID, member.UserID );
	
				// New default
				// BUGBUG address this for multi-domain support
				store.DefaultDomain = domain.ID;
			}
			catch( Exception gssd )
			{
				// BUGBUG the domain was not setup correctly
				// clean it up
				
				log.Error( gssd.Message );
				log.Error( gssd.StackTrace );
			}

			return domain;
		}

		private static void RemoveSubscriptions( Collection collection )
		{
		
		}
		
		/*
			// Get all subscription nodes for this collection.
			ICSList subList = 
				LdapSync.store.GetNodesByProperty( 
					new Property( Subscription.SubscriptionCollectionIDProperty, collection.ID ), 
					SearchOp.Equal );
			foreach ( ShallowNode sn in subList )
			{
				// Get the collection object for this node.
				Collection c = store.GetCollectionByID( sn.CollectionID );
				if ( c != null )
				{
					// Delete this node from the POBox collection.
					c.Commit( c.Delete( new Node( c, sn ) ) );
				}
			}
		}
		*/
		
		private static void ProcessMdbUser( Domain domain, MdbSync.User user )
		{
			log.Debug( "ProcessMdbUser - called" );
			log.Debug( "  user: " + user.DN );
			
			Simias.Storage.Member member = null;
			member = domain.GetMemberByName( user.UserName );
			if ( member == null )
			{
				log.Debug( "  creating new user" );
				
				// Create a new member 
				member = 
					new Member(
						user.UserName,
						Guid.NewGuid().ToString(), 
						Simias.Storage.Access.Rights.ReadOnly,
						user.GivenName,
						user.LastName );

					// Set the local property sync guid
					//cMember.Properties.ModifyProperty( syncGUID );

				// Add the DN to the member node
				Property dn = new Property( "DN", user.DN );
				dn.LocalProperty = true;
				member.Properties.ModifyProperty( dn );
			
				if ( user.FullName != null && user.FullName != "" )
				{
					member.FN = user.FullName;
				}
				
				// Set the known tag for an MDB originated member
				Property mdb = new Property( "ORIGIN:MDB", true );
				mdb.LocalProperty = true;
				member.Properties.ModifyProperty( mdb );

				domain.Commit( member );
			}
			else
			{
				log.Debug( "  found user - checking for updates" );
				
				bool changed = false;
				if ( member.Given != user.GivenName )
				{
					changed = true;
					member.Given = user.GivenName;
				}

				if ( member.Family != user.LastName )
				{
					changed = true;
					member.Family = user.LastName;
				}

				if ( member.FN != user.FullName )
				{
					changed = true;
					if ( user.FullName != null )
					{	
						member.FN = user.FullName;
					}
					else
					if ( member.Given != null && member.Family != null )
					{
						member.FN = member.Given + " " + member.Family;
					}
				}

				// Did the distinguished name change?
				Property dn = member.Properties.GetSingleProperty( "DN" );
				if ( dn != null && dn.ToString() != user.DN )
				{
					dn.Value = user.DN;
					member.Properties.ModifyProperty( "DN", dn );
					changed = true;
				}

				if ( changed == true )
				{
					log.Debug( "  found changed user info - committing node" );
					domain.Commit( member ); 
				}
			}
		}
		

		// If the user is removed from the domain scope, his POBox
		// should get removed from the system rather than orphaned
		private static void RemoveUsersPOBox( Member cMember )
		{
		}
		
		/*
			try
			{
				ICSList cList = store.GetCollectionsByOwner( cMember.UserID );
				foreach( ShallowNode sn in cList )
				{
					Collection c = new Collection( store, sn );
					if ( ( c.Domain == LdapSync.domain.ID ) && c.IsBaseType( c, NodeTypes.POBoxType ) )
					{
						c.Commit( c.Delete() );
					}
				}
			}
			catch( Exception e2 )
			{
				log.Error( e2.Message );
				log.Error( e2.StackTrace );
			}
		}
		*/

		private static void RemoveUserFromCollections( Member cMember )
		{
		}
		
		/*
			string ldapDN = cMember.Properties.GetSingleProperty( "DN" ).Value.ToString();

			// Get all of the collections that this user is member of.
			ICSList cList = store.GetCollectionsByUser( cMember.UserID );
			foreach ( ShallowNode sn in cList )
			{
				// Remove the user as a member of this collection.
				Collection c = new Collection( LdapSync.store, sn );

				// Only look for collections from the specified domain and
				// don't allow this user's membership removed from the domain.
				if ( ( c.Domain == LdapSync.domain.ID ) && !c.IsBaseType( c, NodeTypes.DomainType ) )
				{
					Member member = c.GetMemberByID( cMember.UserID );
					if (member != null)
					{
						if ( member.IsOwner )
						{
							// Don't remove an orphaned collection.
							if ( ( member.UserID != LdapSync.domain.Owner.UserID ) ) 
							{
								//
								// The desired IT behavior is to orphan all collections
								// where the bye-bye user is the owner of the collection.
								// Policy could dictate and force the collection deleted at
								// a later time but the job of the ldap sync code is to 
								// orphan the collection and assign the Simias admin as
								// the new owner.
								//

								// Simias Admin must be a member first before ownership
								// can be transfered
								Member adminMember = c.GetMemberByID( domain.Owner.UserID );
								if ( adminMember == null )
								{
									adminMember = 
										new Member( 
												domain.Owner.Name, 
												domain.Owner.UserID, 
												Simias.Storage.Access.Rights.Admin );

									c.Commit( adminMember );
								}

								Property prevProp = new Property( "OrphanedOwner", ldapDN );
								prevProp.LocalProperty = true;
								c.Properties.ModifyProperty( prevProp );
								c.Commit();

								c.Commit( c.ChangeOwner( adminMember, Simias.Storage.Access.Rights.Admin ) );

								// Now remove the old member
								c.Commit( c.Delete( c.Refresh( member ) ) );

							}
						}
						else
						{
							// Not the owner, just remove the membership.
							c.Commit( c.Delete( member ) );
						}
					}
				}
			}
		}
		*/

		private static string BuildGuidFilter( string guid )
		{
			Guid cGuid = new Guid( guid );
			byte[] bGuid = cGuid.ToByteArray();

			string guidFilter = "(GUID=";
			string tmp;

			// The CSharpLdap SDK expects each byte to
			// be zero padded else an exception is thrown
			for( int i = 0; i < 16; i++ )
			{
				guidFilter += "\\";
				tmp = Convert.ToString( bGuid[i], 16 );
				if ( tmp.Length == 1 )
				{
					guidFilter += "0";
				}
				guidFilter += tmp;
			}

			guidFilter += ")";
			return guidFilter;
		}


		/*
		private static string GetLdapGuid( LdapEntry entry )
		{
			string ldapGuid = null;

			try
			{
				LdapAttribute guidAttr = entry.getAttribute( "GUID" );
				if ( guidAttr != null && guidAttr.StringValue.Length != 0 )
				{
					byte[] bGuid = new byte[8];
					for( int i = 0; i < 8; i++ )
					{
						bGuid[i] = (byte) guidAttr.ByteValue[i];
					}

					Guid cGuid = 
						new Guid(
							BitConverter.ToInt32( bGuid, 0 ),
							BitConverter.ToInt16( bGuid, 4 ),
							BitConverter.ToInt16( bGuid, 6 ),
							(byte) guidAttr.ByteValue[8], 
							(byte) guidAttr.ByteValue[9],
							(byte) guidAttr.ByteValue[10],
							(byte) guidAttr.ByteValue[11],
							(byte) guidAttr.ByteValue[12],
							(byte) guidAttr.ByteValue[13],
							(byte) guidAttr.ByteValue[14],
							(byte) guidAttr.ByteValue[15] );

					ldapGuid = cGuid.ToString();
				}
			}
			catch{}
			return ldapGuid;
		}
		*/
		
		
		private static bool IsUser(String[] objectClasses)
		{
			try
		    {
				foreach( string s in objectClasses )
				{
					if ( s.ToLower() == "inetorgperson" )
					{
						return true;
					}
				}
			}
		    catch( Exception e )
		    {
				log.Error( "IsUser failed with exception" );
				log.Error( e.Message );
			}
		    return false;
		}

		private static bool IsContainer(String[] objectClasses)
		{
			bool isContainer = false;

			try
			{
				foreach( string s in objectClasses )
				{
					string lower = s.ToLower();
					if ( lower == "organization" )
					{
						log.Debug( "Processing Organization Object..." );
						isContainer = true;
						break;
					}
					else if ( lower == "organizationalunit" )
					{
						isContainer = true;
						log.Debug( "Processing OrganizationalUnit Object..." );
						break;
					}
					else if ( lower == "country" )
					{
						isContainer = true;
						log.Debug( "Processing Country Object..." );
						break;
					}
					else if ( lower == "locality" )
					{
						isContainer = true;
						log.Debug( "Processing Locality Object..." );
						break;
					}
				}
			}
			catch( Exception e )
			{
				log.Error( "IsContainer failed with exception" );
				log.Error( e.Message );
			}

			return isContainer;
		}

		
		// If the configured Simias Admin is different than the SimiasAdmin
		// identified in the store, make all the changes necessary to
		// make the configured admin the store admin.
		
		/*
		internal static void ChangeSimiasAdmin( LdapConnection conn )
		{
			char[] dnDelimiters = {',', '='};
			LdapEntry entry = null;
			Property dn;
			string commonName;
			string ldapGuid;
			string[] searchAttributes = { "cn", "sn", "GUID" };

			try
			{
				// Nothing in the config for a SimiasAdmin - we're done here
				if ( ldapSettings.AdminDN == null || ldapSettings.AdminDN == "" )
				{
					return;
				}

				// If the SimiasAdmin has been changed in the Simias.config, which BTW
				// is not something that is exposed in the normal management UI, 
				// we need to verify the new SimiasAdmin exists in the directory,
				// check if the new admin exists in local domain memberlist (and
				// if he doesn't create him), transfer default domain ownership
				// to the new SimiasAdmin and lastly transfer ownership of all
				// orphaned iFolders to the new SimiasAdmin.

				try
				{
					entry = conn.Read( LdapSync.ldapSettings.AdminDN, searchAttributes );
				}
				catch( LdapException lEx )
				{
					log.Error( "Could not verify the newly configured Simias Administrator in the directory" );
					log.Error( lEx.Message );
				}
				catch( Exception e1 )
				{
					log.Error( "Could not verify the newly configured Simias Administrator in the directory" );
					log.Error( e1.Message );
				}

				if ( entry == null )
				{
					return;
				}

				ldapGuid = GetLdapGuid( entry );
				if ( ldapGuid == null || ldapGuid == "" )
				{
					return;
				}

				// Get the common name from the Simias.config.AdminDN entry
				string[] components = LdapSync.ldapSettings.AdminDN.Split( dnDelimiters );
				commonName = ( components[0].ToLower() == "cn" ) ? components[1] : components[0];
				if ( commonName == null || commonName == "" )
				{
					return;
				}

				Member member = domain.GetMemberByName( commonName );
				if ( member == null )
				{
					// Create the member with the Ldap guid
					member = 
						new Member(	commonName,	ldapGuid, Simias.Storage.Access.Rights.ReadOnly );
					member.Properties.ModifyProperty( "DN", LdapSync.ldapSettings.AdminDN );
				}

				Property lguid = new Property( "LdapGuid", ldapGuid );
				lguid.LocalProperty = true;
				member.Properties.ModifyProperty( lguid );
				domain.Commit( member );

				// Transfer ownership of all collections owned by the 
				// previous admin that have the orphaned property
				Property orphaned;
				ICSList subList = LdapSync.store.GetCollectionsByOwner( domain.Owner.ID, domain.ID ); 
				foreach ( ShallowNode sn in subList )
				{
					// Get the collection object for this node.
					Collection c = store.GetCollectionByID( sn.CollectionID );
					if ( c != null )
					{
						orphaned = c.Properties.GetSingleProperty( "OrphanedOwner" );
						if ( orphaned != null )
						{
							dn = c.Owner.Properties.GetSingleProperty( "DN" );
							if ( dn != null )
							{
								c.PreviousOwner = dn.Value.ToString();
								c.Commit();
							}

							c.Commit( c.ChangeOwner( member, Simias.Storage.Access.Rights.ReadWrite ) );
						}
					}
				}

				// For now I'm just going to leave the LdapGuid property
				// on the old SimiasAdmin
				dn = domain.Owner.Properties.GetSingleProperty( "DN" );
				if ( dn != null )
				{
					domain.PreviousOwner = dn.Value.ToString();
					domain.Commit();
				}
				
				domain.Commit( domain.ChangeOwner( member, Simias.Storage.Access.Rights.ReadWrite ) );
			}
			catch( Exception vsa )
			{
				log.Error( vsa.Message );
				log.Error( vsa.StackTrace );
			}
		}
		*/

		// The SimiasAdmin is processed differently than normal simias users because
		// the account is aleady created in the Simias store before LdapSync runs
		// so the GUID has already been created.  The SimiasAdmin must always exist in the
		// store and the DN entry in the store must be correct with the Distinguished
		// Name in the directory.  LdapSync counts on the AdminDN entry in Simias.config
		// to be updated if the admin is moved in the directory.
		
		/*
		internal static void ProcessSimiasAdmin(LdapConnection conn)
		{
			// Since the first version of the iFolder 3.0 only
			// exposes a username, firstname, lastname and full
			// name, we'll limit the scope of the search
			string[] searchAttributes = {	
						"modifytimestamp",
						"cn", 
						"sn", 
						"GUID", 
						"givenName" }; 


			char[] dnDelimiters = {',', '='};
			LdapEntry entry = null;
			LdapAttribute timeStampAttr = null;
			Member cMember = null;
			Property dn = null;
			string ldapGuid = null;

			log.Debug( "ProcessSimiasAdmin( " + LdapSync.ldapSettings.AdminDN + ")" );

			// If the DN property has never been set on the SimiasAdmin,
			// set it now
			cMember = LdapSync.domain.Owner;
			dn = cMember.Properties.GetSingleProperty( "DN" );
			if ( dn == null || dn.Value.ToString() == "" )
			{
				if ( ldapSettings.AdminDN != null && ldapSettings.AdminDN != "" )
				{
					dn = new Property( "DN", ldapSettings.AdminDN );
					cMember.Properties.ModifyProperty( dn );
				}
			}

			// Check if the Simias Admin has changed in configuration
			if ( ldapSettings.AdminDN != null && ldapSettings.AdminDN != "" &&
				dn.Value.ToString() != ldapSettings.AdminDN )
			{
				ChangeSimiasAdmin( conn );
				cMember = LdapSync.domain.Owner;
			}

			// The Simias admin is tracked in the directory by the directory
			// guid.  Make sure the guid is stored in the node
			Property lguidProp = cMember.Properties.GetSingleProperty( "LdapGuid" );
			if ( lguidProp == null )
			{
				// This must be the first time thru so let's get the directory
				// entry based on the configured DN
				try
				{
					entry = conn.Read( LdapSync.ldapSettings.AdminDN, searchAttributes );
				}
				catch( LdapException lEx )
				{
					log.Error( "The Simias Administrator does not exist in the Ldap directory as configured in Simias.config!" );
					log.Error( lEx.Message );
				}
				catch( Exception e1 )
				{
					log.Error( "The Simias Administrator does not exist in the Ldap directory as configured in Simias.config!" );
					log.Error( e1.Message );
				}

				if ( entry != null )
				{
					ldapGuid = GetLdapGuid( entry );
					lguidProp = new Property( "LdapGuid", ldapGuid );
					lguidProp.LocalProperty = true;
					cMember.Properties.ModifyProperty( lguidProp );
				}
			}
			else
			{
				ldapGuid = lguidProp.Value.ToString();
			}

			if ( ldapGuid != null )
			{
				try
				{
					entry = null;

					// Now go find the SimiasAdmin in the Ldap directory
					string guidFilter = LdapSync.BuildGuidFilter( ldapGuid );
					LdapSearchResults results = 
						conn.Search(
						"",
						LdapConnection.SCOPE_SUB, 
						"(&(objectclass=inetOrgPerson)" + guidFilter + ")",
						searchAttributes,
						false);
					if ( results.hasMore() == true )
					{
						entry = results.next();
					}
				}
				catch ( LdapException e )
				{
					log.Error( e.LdapErrorMessage );
					log.Error( e.StackTrace );
				}
				catch ( Exception e )
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
				}

				if ( entry != null )
				{
					//
					// check if the ldap object's time stamp has changed
					//
					try
					{
						timeStampAttr = entry.getAttribute( "modifytimestamp" );
						Property pStamp = 
							cMember.Properties.GetSingleProperty( "LdapTimeStamp" );

						if ( ( pStamp == null ) ||
							( pStamp != null && 
							(string) pStamp.Value != timeStampAttr.StringValue ) )
						{
							// The time stamp changed let's look at first and
							// last name
	
							try
							{
								bool changed = false;
	
								// If we're tracking by ldap see if the common name
								// has changed
								LdapAttribute cnAttr = entry.getAttribute( "cn" );
								if ( cnAttr != null && cnAttr.StringValue.Length != 0 )
								{
									if ( cnAttr.StringValue != cMember.Name )
									{
										cMember.Name = cnAttr.StringValue;
									}
								}
	
								LdapAttribute givenAttr = entry.getAttribute( "givenName" );
								if ( givenAttr != null && givenAttr.StringValue.Length != 0 )
								{
									if ( givenAttr.StringValue != cMember.Given )
									{
										changed = true;
										cMember.Given = givenAttr.StringValue;
									}
								}

								LdapAttribute sirAttr = entry.getAttribute( "sn" );
								if ( sirAttr != null && sirAttr.StringValue.Length != 0 )
								{
									if ( sirAttr.StringValue != cMember.Family )
									{
										cMember.Family = sirAttr.StringValue;
										changed = true;
									}
								}


								// If the entry has changed and we have a valid
								// family and given
								if ( changed == true && 
									cMember.Given != null &&
									cMember.Given != "" && 
									cMember.Family != null &&
									cMember.Family != "" )
								{
									cMember.FN = cMember.Given + " " + cMember.Family;
								}

								// Did the distinguished name change?
								Property dnProp = cMember.Properties.GetSingleProperty( "DN" );
								if ( dnProp != null && ( dnProp.ToString() != entry.DN ) )
								{
									dnProp.Value = entry.DN;
									cMember.Properties.ModifyProperty( "DN", dnProp );
								}
							}
							catch {}

							pStamp = new Property( "LdapTimeStamp", timeStampAttr.StringValue );
							pStamp.LocalProperty = true;
							cMember.Properties.ModifyProperty( pStamp );
						}
					}
					catch{}
				}
				else
				{
					log.Error( "The Simias administrator could not be verified in the directory!" );
					log.Error( "Please update Simias.config with a valid Ldap user" );
				}
			}
			else
			{
				log.Error( "The Simias administrator could not be verified in the directory!" );
				log.Error( "Please update Simias.config with a valid Ldap user" );
			}

			// Now matter what always update the sync guid so
			// the SimiasAdmin won't be deleted from Simias
			cMember.Properties.ModifyProperty( syncGUID );
			LdapSync.domain.Commit( cMember );
		}
		*/

		/*
		internal static void ProcessUserEntry( LdapEntry entry )
		{
			log.Debug( "ProcessUserEntry(" + entry.DN + ")" );

		    char[] dnDelimiters = {',', '='};
		    LdapAttribute timeStampAttr = null;

			try
		    {
				timeStampAttr = entry.getAttribute( "modifytimestamp" );
			}
		    catch {}

			//
		    // Check if this member already exists
			// Note: The common name or username in Simias will
			// be set to the configured naming attribute in
			// the ldap directory, which by default is cn
		    //

			string commonName = null;
			string ldapGuid = null;
			Member cMember = null;

			try
			{
				ldapGuid = GetLdapGuid( entry );
				if ( ldapGuid !=  null )
				{
					cMember = LdapSync.domain.GetMemberByID( ldapGuid );
					if ( cMember != null )
					{
						log.Debug( "Found member by GUID" );
						commonName = cMember.Name;
					}
				}
			}
			catch( Exception gEx )
			{
				log.Error( gEx.Message );
				log.Error( gEx.StackTrace );
			}

			// Did we lookup via Ldap GUID?
			if ( cMember == null )
			{
				try
				{
					LdapAttribute cAttr = 
						entry.getAttribute( ldapSettings.NamingAttribute );
					if ( cAttr != null && cAttr.StringValue.Length != 0 )
					{
						commonName = cAttr.StringValue;
						cMember = LdapSync.domain.GetMemberByName( cAttr.StringValue );
					}
					else
					if ( ldapSettings.NamingAttribute.ToLower() == LdapSettings.DefaultNamingAttribute.ToLower() )
					{
						// If the naming attribute is default (cn) then we want to continue
						// to work the way we previously did so we don't break any existing installs.
						//
						// If the distinguishing attribute did not exist,
						// then make the Simias username the first component
						// of the ldap DN.
						string[] components = entry.DN.Split( dnDelimiters );
						commonName = components[1];
						cMember = LdapSync.domain.GetMemberByName( commonName );
					}

					if ( cMember != null )
					{
						//
						// Check if the cMember.DN and the entry.DN are different
						//
						if (entry.DN != (string) cMember.Properties.GetSingleProperty("DN").Value)
						{
							log.Debug( "entry.DN != (string) cMember.Properties.GetSingleProperty(\"DN\").Value" );
							log.Debug( "Duplicate Member.Name \"" + cMember.Name + "\"" );
							log.Debug( "Skipped: " + entry.DN );
							return;
						}
					}
				}
				catch {}
			}

		    if ( cMember != null )
			{
				//
				// check if the ldap object's time stamp has changed
				//

				try
				{
					Property pStamp = null;
					try
					{
						pStamp = cMember.Properties.GetSingleProperty( "LdapTimeStamp" );
						pStamp.LocalProperty = true;
					}
					catch
					{
						pStamp = new Property( "LdapTimeStamp", "ABA" );
						pStamp.LocalProperty = true;
					}

					if ( (string) pStamp.Value != timeStampAttr.StringValue )
					{
						// The time stamp changed let's look at first and
						// last name

						try
						{
							bool changed = false;

							// If we're tracking by ldap see if the common name
							// has changed
							if ( ldapGuid != null )
							{
								LdapAttribute cnAttr = 
									entry.getAttribute( ldapSettings.NamingAttribute );
								if ( cnAttr != null && cnAttr.StringValue.Length != 0 )
								{
									if ( cnAttr.StringValue != cMember.Name )
									{
										commonName = cnAttr.StringValue;
										cMember.Name = cnAttr.StringValue;
									}
								}
								else
								{
									string[] components = entry.DN.Split( dnDelimiters );
									commonName = components[1];
									if ( commonName != cMember.Name )
									{
										cMember.Name = commonName;
									}
								}
							}

							LdapAttribute givenAttr = entry.getAttribute( "givenName" );
							if ( givenAttr != null && givenAttr.StringValue.Length != 0 )
							{
								if ( givenAttr.StringValue != cMember.Given )
								{
									changed = true;
									cMember.Given = givenAttr.StringValue;
								}
							}

							LdapAttribute sirAttr = entry.getAttribute( "sn" );
							if ( sirAttr != null && sirAttr.StringValue.Length != 0 )
							{
								if ( sirAttr.StringValue != cMember.Family )
								{
									cMember.Family = sirAttr.StringValue;
									changed = true;
								}
							}

							// Did the distinguished name change?
							Property dnProp = cMember.Properties.GetSingleProperty( "DN" );
							if ( dnProp != null && dnProp.ToString() != entry.DN )
							{
								dnProp.Value = entry.DN;
								cMember.Properties.ModifyProperty( "DN", dnProp );
								changed = true;
							}

							if ( changed == true &&
								cMember.Given != null &&
								cMember.Given != "" &&
								cMember.Family != null &&
								cMember.Family != "" )
							{
								cMember.FN = cMember.Given + " " + cMember.Family;
							}
						}
						catch {}

						pStamp.Value = timeStampAttr.StringValue;
						cMember.Properties.ModifyProperty( pStamp );
					}
				}
				catch{}

				cMember.Properties.ModifyProperty( syncGUID );
				LdapSync.domain.Commit(cMember);
			}
			else
			if ( commonName != null && commonName != "" )
			{
				log.Debug( "Processing new member" );
				try
				{
					string givenName = null;
					string familyName = null;
	
					try
					{
						LdapAttribute givenAttr = entry.getAttribute( "givenName" );
						if ( givenAttr != null && givenAttr.StringValue.Length != 0 )
						{
							givenName = givenAttr.StringValue;
						}

						LdapAttribute sirAttr = entry.getAttribute( "sn" );
						if ( sirAttr != null && sirAttr.StringValue.Length != 0 )
						{
							familyName = sirAttr.StringValue;
						}
					}
					catch{}

					// Create a new member 
					cMember = 
						new Member(
							commonName,
							(ldapGuid != null) ? ldapGuid : Guid.NewGuid().ToString(), 
							Simias.Storage.Access.Rights.ReadOnly,
							givenName,
							familyName );

					// Set the local property sync guid
					cMember.Properties.ModifyProperty( syncGUID );

					// Add the DN to the member node
					Property dn = new Property( "DN", entry.DN );
					dn.LocalProperty = true;
					cMember.Properties.ModifyProperty( dn );
				}
				catch
				{
					return;
				}

				//
				// Save the Ldap object's time stamp in the member's node
				// so we'll know in the future if the ldap object changed.
				//

				if ( timeStampAttr != null && timeStampAttr.StringValue.Length != 0 )
				{
					Property ltsP = new Property( "LdapTimeStamp", timeStampAttr.StringValue );
					ltsP.LocalProperty = true;
					cMember.Properties.ModifyProperty( ltsP );
				}

				LdapSync.domain.Commit( cMember );
			}
			else
			{
				log.Debug( "Ldap entry did not contain the naming attribute specified - entry excluded" );
			}
		}
		*/
    }
}
