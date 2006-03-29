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


namespace Simias.MdbProvider
{

    /// <summary>
    /// Service class used to get an execution context
    /// so we can register ourselves with the external
    /// sync container
    /// </summary>
    public class SyncProvider : Simias.IIdentitySyncProvider
    {
        #region Class Members
        private readonly string name = "Mdb Synchronization";
        private readonly string description = "Hula external synchronization provider";
        private bool abort = false;

        /// <summary>
        /// Used to log messages.
        /// </summary>
        private static readonly ISimiasLog log = 
            SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name { get{ return name; } }

        /// <summary>
        /// Gets the description of the provider.
        /// </summary>
        public string Description { get{ return description; } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Call to abort an in process synchronization
        /// </summary>
        /// <returns>N/A</returns>
        public void Abort()
        {
            abort = true;
        }

        /// <summary>
        /// Call to inform a provider to start a synchronization cycle
        /// </summary>
        /// <returns> True - provider successfully finished a sync cycle, 
        /// False - provider failed the sync cycle
        /// </returns>
        public bool Start( Simias.IdentitySync.State State )
        {
            log.Debug( "Start called" );



            string[] domains = Simias.MdbProvider.DomainConfiguration.GetDomains();
            foreach( string currentDomain in domains )
            {
                if ( abort == true )
                {
                    return false;
                }

                DomainConfiguration domainConfig = new DomainConfiguration( currentDomain );
                log.Debug( "Current Domain: " + domainConfig.DomainName );
                log.Debug( "Starting MDB -> " + domainConfig.DomainName + " sync" );

                // Authenticate against MDB	
                Simias.MdbProvider.EnumUsers enumUsers = null;
                Simias.MdbProvider.Mdb mdb = null;
                Simias.MdbProvider.MdbUser mdbUser = null;

                try
                {
                    log.Debug( "Authenticating proxy user: " + domainConfig.ProxyUsername );
                    mdb = new Simias.MdbProvider.Mdb( domainConfig.ProxyUsername, domainConfig.ProxyPassword );
                    Console.WriteLine( "MDB Handle: " + mdb.Handle.ToString() );
                }
                catch( Exception mdbEx )
                {
                    log.Error( "Failed to authenticate to MDB.  User: " + domainConfig.ProxyUsername );
                    log.Error( mdbEx.Message );
                    return false;
                }


                try
                {
                    string container = domainConfig.Containers[0];
                    log.Debug( "  syncing container: " + container );
                    enumUsers = new Simias.MdbProvider.EnumUsers( mdb.Handle, container, false );
                    enumUsers.Reset();
                    while( enumUsers.MoveNext() == true )
                    {
                        mdbUser = enumUsers.Current as Simias.MdbProvider.MdbUser;

                        Property origin = new Property( "ORIGIN:MDB", true );
                        Property[] propertyList = { origin };
                        State.ProcessMember(
                            null,
                            mdbUser.UserName,
                            mdbUser.GivenName,
                            mdbUser.LastName,
                            null,
                            mdbUser.DN,
                            propertyList );
                    }
                }
                catch( SimiasShutdownException s )
                {
                    log.Error( s.Message );
                    State.ReportError( s.Message );
                    return false;
                }
                catch( Exception e )
                {
                    log.Error( e.Message );
                    log.Error( e.StackTrace );
                    State.ReportError( e.Message );
                    return false;
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
            }

            return true;
        }
        #endregion
    }
}
