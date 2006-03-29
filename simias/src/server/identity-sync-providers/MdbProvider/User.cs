/***********************************************************************
 *  $RCSfile$
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
using System.Reflection;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Server;

using MdbHandle = System.IntPtr;

namespace Simias.MdbProvider
{
	/// <summary>
	/// Implementation of the IUserProvider Service for
	/// Simple Server.
	///
	/// Simple Server does not allow creation and deletion
	/// of user's via the IUserProvider interface so those
	/// methods will return false.
	/// </summary>
	public class User : Simias.Server.IUserProvider
	{
		#region Fields
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		/// <summary>
		/// String used to identify domain provider.
		/// </summary>
		static private string providerName = "Hula Provider";
		static private string providerDescription = "Hula MDB to Simias identity provider";
		static private string missingDomainMessage = "Enterprise domain does not exist!";

		// Frequently used Simias types
		private Store store = null;
		private Simias.Storage.Domain domain = null;
        private string admin = null;

        private readonly string thisModule = "simias-mdb-sync";
        private int apiVersion = -1;
        private MdbHandle mdbHandle = System.IntPtr.Zero;

		#endregion


        // Native MDB functions used via PInvoke
        [DllImport( Mdb.HulaLib ) ]
        private static extern bool MDBInit();

        /*
        [DllImport( Mdb.HulaLib ) ]
        private static extern bool MDBShutdown();
        */

        /*
        [DllImport( Mdb.HulaLib ) ] 
        private static extern 
        int 
        MDBGetAPIVersion(
            bool wantCompatibleVersion, 
            StringBuilder description,
            IntPtr context);
        */

        [DllImport( Mdb.HulaLib )]
        protected static extern
        IntPtr MDBCreateValueStruct( IntPtr Handle, string Context );

        [DllImport( Mdb.HulaLib )]
        protected static extern bool MDBDestroyValueStruct( IntPtr ValueStruct );

        [DllImport( Mdb.HulaLib ) ]
        private static extern 
        IntPtr
        MDBAuthenticate( string Module, string Principal, string Password );

        [DllImport( Mdb.HulaLib ) ]
        private static extern 
        bool
        MDBVerifyPassword( string ObjectDN, string Password, IntPtr v );

        [DllImport( Mdb.HulaLib )]
        private static extern
        bool 
        MDBRelease( IntPtr handle );

        /* We have to call some msgapi function to force it to load for MDB 
           This might be a bug.  This seems to happen only on Linux when 
           running under Mono. */
        [DllImport( Mdb.HulaMessageApiLib)]
        private static extern IntPtr MsgDirectoryHandle();

        [DllImport( Mdb.HulaMessageApiLib )]
        public static extern bool MsgFindObject(string user, StringBuilder dn, string type, IntPtr nmap, IntPtr valueStruct);

        [DllImport( "hulamemmgr" )]
        public static extern bool MemoryManagerOpen(string agentName);

        [DllImport( "hulamemmgr" )]
        public static extern bool MemoryManagerClose(string agentName);

		
		#region Properties
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		public string Name { get { return providerName; } }

		/// <summary>
		/// Gets the description of the provider.
		/// </summary>
		public string Description { get { return providerDescription; } }
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of this object.
		/// </summary>
		public User()
		{
			store = Store.GetStore();
			if ( store.DefaultDomain == null )
			{
				throw new SimiasException( User.missingDomainMessage );
			}
			
			domain = store.GetDomain( store.DefaultDomain );
			if ( domain == null )
			{
				throw new SimiasException( User.missingDomainMessage );
			}
			
			if ( domain.IsType( "Enterprise" ) == false )
			{
				throw new SimiasException( User.missingDomainMessage );
			}


            // Must load the message and memory manager libraries
            MsgDirectoryHandle();
            MemoryManagerOpen( "SimiasAuthentication" );			

            // Call the native initialization API
            if ( MDBInit() == false )
            {
                Console.WriteLine( "failed to load \"libhulamdb\"" );
                throw new SimiasException( "Failed to load libhulamdb!" );
            }

            // BUGBUG must get this through configuration
            this.mdbHandle = MDBAuthenticate( thisModule, "\\Tree\\Context\\admin", "hula" );
            if ( this.mdbHandle == System.IntPtr.Zero )
            {
                Console.WriteLine( this.mdbHandle.ToString() );
                throw new SimiasException( "Failed to authenticate against MDB" );
            }

			// Make sure the password is set on the admin
			SetAdminPassword();
		}

        ~User()
        {
            if ( mdbHandle != IntPtr.Zero )
            {
                MDBRelease( mdbHandle );
            }
        }

		#endregion
		
		#region Private Methods
		
		//
		// When the Enterprise Server Domain is instantiated in Simias.Server
		// the domain owner is determined via the AdminName in Simias.config.
		// Simple Server determines the domain owner via an attribute in
		// the SimpleServer.xml file so this method will actually change
		// ownership of the domain to the one specified in the xml file
		// unless they happen to be the same.
		//
		private bool SetAdminPassword()
		{
            bool status = false;

            // Bootstrap the domain from the Simias.config file
            Simias.Configuration config = Store.Config;
            string admin = config.Get( "EnterpriseDomain", "AdminName" );
            string adminPassword = config.Get( "EnterpriseDomain", "AdminPassword" );

            if ( admin != null && admin != "" && adminPassword != null )
            {
                try
                {
                    Member member = domain.GetMemberByName( admin );
                    if ( member != null )
                    {
                        Property pwd = member.Properties.GetSingleProperty( "PWD" );
                        if ( pwd == null || pwd.Value == null )
                        {
                            pwd = new Property( "PWD", HashPassword( adminPassword ) );
                            member.Properties.ModifyProperty( pwd );

                            // Marker so we know this member was created internally
                            // and not through an external identity sync.
                            domain.SetType( member as Node, "Internal" );
                            domain.Commit( member );
                            status = true;
                        }	
                    }
                }
                catch( Exception ap )
                {
                    log.Error( ap.Message );
                    log.Error( ap.StackTrace );
                }
            }
			
			return status;
		}		
		#endregion
		
		#region Internal Methods
		static internal string HashPassword( string password )
		{
			UTF8Encoding utf8Encoding = new UTF8Encoding();
			MD5CryptoServiceProvider md5Service = new MD5CryptoServiceProvider();
		
			byte[] bytes = new Byte[ utf8Encoding.GetByteCount( password ) ];
			bytes = utf8Encoding.GetBytes( password );
			return Convert.ToBase64String( md5Service.ComputeHash( bytes ) );
		}
		#endregion
		
		#region Public Methods
		/// <summary>
		/// Method to create a user/identity in the external user database.
		/// Some external systems may not allow for creation of new users.
		/// </summary>
		/// <param name="Guid" mandatory="false">Guid associated to the user.</param>
		/// <param name="Username" mandatory="true">User or short name for the new user.</param>
		/// <param name="Password" mandatory="true">Password associated to the user.</param>
		/// <param name="Firstname" mandatory="false">First or given name associated to the user.</param>
		/// <param name="Lastname" mandatory="false">Last or family name associated to the user.</param>
		/// <param name="Fullname" mandatory="false">Full or complete name associated to the user.</param>
		/// <returns>RegistrationStatus</returns>
		/// Note: Method assumes the mandatory arguments: Username, Password have already 
		/// been validated.  Also assumes the username does NOT exist in the domain
		public
		Simias.Server.RegistrationInfo
		Create(
			string Userguid,
			string Username,
			string Password,
			string Firstname,
			string Lastname,
			string Fullname )
		{
			RegistrationInfo info = new RegistrationInfo( RegistrationStatus.MethodNotSupported );
			info.Message = "MDB provider does not support creating users through the registration class";
			return info;
		}
		
		/// <summary>
		/// Method to delete a user from the external identity/user database.
		/// Some external systems may not allow deletion of users.
		/// </summary>
		/// <param name="Username">Name of the user to delete from the external system.</param>
		/// <returns>true - successful  false - failed</returns>
		public bool Delete( string Username )
		{
			// Simple Server does not support delete through the registration APIs
			return false;
		}
		
		/// <summary>
		/// Method to verify a user's password
		/// </summary>
		/// <param name="Username">User to verify the password against</param>
		/// <param name="Password">Password to verify</param>
		/// <returns>true - Valid password, false Invalid password</returns>
		public bool VerifyPassword( string Username, string Password )
		{
			bool status = false;
			
			try
			{
                Member member = domain.GetMemberByName( Username );
                if ( member != null )
                {
                    if ( admin == Username )
                    {
                        Property pwd = member.Properties.GetSingleProperty( "PWD" );
                        if ( pwd != null && pwd.Value != null )
                        {
                            if ( pwd.Value as string == HashPassword( Password ) )
                            {
                                log.Debug( "  auth successful" );
                                status = true;
                            }
                        }
                    }
                    else
                    {
                        // This identity originated from MDB so let's verify the 
                        // password there

                        IntPtr v = MDBCreateValueStruct( mdbHandle, "\\Tree\\Context" );
                        if ( v != IntPtr.Zero )
                        {
                            Property dn = member.Properties.GetSingleProperty( "DN" );
                            if ( dn != null )
                            {
                                log.Debug( "attempting to authenticate: " + dn.Value );
                                if ( MDBVerifyPassword( (string) dn.Value, Password, v ) == true )
                                {
                                    log.Debug( "  auth successful" );
                                    status = true;
                                }
                            }

                            MDBDestroyValueStruct( v );
                        }
                    }
                }
			}
			catch( Exception e1 )
			{
				log.Error( e1.Message );
				log.Error( e1.StackTrace );
				status = false;
			}			
			
			return status;
		}
		#endregion
	}	
}
