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
using System.Net;
using System.Runtime.InteropServices;
//using System.Runtime.Remoting;
//using System.Runtime.Remoting.Channels;
//using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Storage;

//using Mono.P2p.mDnsResponderApi;

namespace Simias.mDns
{
	// Quick and dirty - I need to clean this up and
	// put some thought into it
	public class RendezvousUser
	{
		public string ID;
		public string FriendlyName;
		public string Host;
		public int	  Port;
		public string ServicePath;
		public string PublicKey;
		public bool	  SimiasUser;

		public RendezvousUser()
		{

		}
	}

	public class UserLock
	{
		private int	lockIt;
		public UserLock()
		{
		}
	}

	/// <summary>
	/// Class used to broadcast the current user
	/// to the Rendezvous/mDns network
	/// </summary>
	public class User
	{
		#region DllImports
		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		RegisterLocalMember(
			string		ID,
			string		Name,
			short		Port,
			string		ServicePath,
			int			PublicKeyLength,
			string		PublicKey,
			ref IntPtr	Cookie);

		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		DeregisterLocalMember(string ID, int Cookie);

		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		GetMemberInfo(
			[MarshalAs(UnmanagedType.LPStr)] string	ID,
			[In, Out] char[]	MemberName,
			[In, Out] char[]	ServicePath,
			[In, Out] byte[]	PublicKey,
			[In, Out] char[]	HostName,
			ref       int		Port);

		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		BrowseMembersInit( MemberBrowseCallback	callback, ref IntPtr handle );
		
		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		BrowseMembersShutdown( int handle );

		[ DllImport( "simdezvous" ) ]
		private 
		extern 
		static 
		User.kErrorType
		BrowseMembers( int handle, int timeout );

		[ DllImport( "simdezvous" ) ]
		internal 
		extern 
		static 
		User.kErrorType
		ResolveAddress(
			[MarshalAs(UnmanagedType.LPStr)] string	hostName,
			int	BufferLength,
			[In, Out] char[] TextualIPAddress);
		#endregion

		#region Class Members
		private static bool registered = false;
		private static string  mDnsUserName;
		private static string  mDnsUserID = "";
		//private readonly string memberTag = "_ifolder_member._tcp.local";
		private static readonly string configSection = "ServiceManager";
		private static readonly string configServices = "WebServiceUri";
		private static Uri webServiceUri = null;
		private static IntPtr userHandle;
		private static IntPtr browseHandle;
		private static Thread browseThread = null;

		//private const string nativeLibrary = "ifolder-rendezvous";

		// State for maintaining the Rendezvous user list

		// TEMP need to fix protection level here
		internal static UserLock	memberListLock;
		internal static ArrayList	memberList;

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// possible error code values 
		public enum kErrorType : int
		{
			kDNSServiceErr_NoError             = 0,
			kDNSServiceErr_Unknown             = -65537,       /* 0xFFFE FFFF */
			kDNSServiceErr_NoSuchName          = -65538,
			kDNSServiceErr_NoMemory            = -65539,
			kDNSServiceErr_BadParam            = -65540,
			kDNSServiceErr_BadReference        = -65541,
			kDNSServiceErr_BadState            = -65542,
			kDNSServiceErr_BadFlags            = -65543,
			kDNSServiceErr_Unsupported         = -65544,
			kDNSServiceErr_NotInitialized      = -65545,
			kDNSServiceErr_AlreadyRegistered   = -65547,
			kDNSServiceErr_NameConflict        = -65548,
			kDNSServiceErr_Invalid             = -65549,
			kDNSServiceErr_Firewall            = -65550,
			kDNSServiceErr_Incompatible        = -65551,        /* client library incompatible with daemon */
			kDNSServiceErr_BadInterfaceIndex   = -65552,
			kDNSServiceErr_Refused             = -65553,
			kDNSServiceErr_NoSuchRecord        = -65554,
			kDNSServiceErr_NoAuth              = -65555,
			kDNSServiceErr_NoSuchKey           = -65556,
			kDNSServiceErr_NATTraversal        = -65557,
			kDNSServiceErr_DoubleNAT           = -65558,
			kDNSServiceErr_BadTime             = -65559
			/* mDNS Error codes are in the range
				 * FFFE FF00 (-65792) to FFFE FFFF (-65537) */
		};

		#endregion

		#region Properties
		/// <summary>
		/// Gets the current user's mDns ID
		/// </summary>
		public string ID
		{
			get { return( User.mDnsUserID ); }
		}

		/// <summary>
		/// Gets the mDnsDomain's friendly name
		/// </summary>
		public string Name
		{
			get { return( User.mDnsUserName ); }
		}
		#endregion

		#region Constructors

		/// <summary>
		/// Static constructor for mDns
		/// </summary>
		static User()
		{
			// Get the configured/generated web service path and store
			// it away.  The port and path are broadcast in Rendezvous
			XmlElement servicesElement = 
				Store.GetStore().Config.GetElement( configSection, configServices );
			webServiceUri = new Uri( servicesElement.GetAttribute( "value" ) );
			if ( webServiceUri != null )
			{
				log.Debug( "Web Service URI: " + webServiceUri.ToString() );
				log.Debug( "Absolute Path: " + webServiceUri.AbsolutePath );
			}

			User.memberListLock = new UserLock();
			User.memberList = new ArrayList();

			userHandle = new IntPtr(0);

			//Simias.mDns.Domain mdnsDomain = new Simias.mDns.Domain( true );
			//mDnsUserName = Environment.UserName + "@" + mdnsDomain.Host;
			User.mDnsUserName = Environment.UserName + "@" + Environment.MachineName;

			try
			{
				Simias.Storage.Domain rDomain = 
					Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
				if ( rDomain != null )
				{
					User.mDnsUserID = rDomain.GetMemberByName( mDnsUserName).UserID;
				}
			}
			catch( Exception e )
			{
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
			}
		}

		/// <summary>
		/// Constructor for newing up an mDns user object.
		/// </summary>
		internal User()
		{

			/*
			try
			{
				Simias.Storage.Domain rDomain = 
					Store.GetStore().GetDomain( Simias.mDns.Domain.ID );
				if ( rDomain != null )
				{
					this.mDnsUserID = rDomain.GetMemberByName( mDnsUserName).UserID;
				}
			}
			catch( Exception e )
			{
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
			}
			*/
		}

		#endregion

		#region Internal Methods
		internal static void RegisterUser()
		{
			if ( registered == true )
			{
				User.UnregisterUser();
			}

			if ( webServiceUri == null )
			{
				//throw new SimiasException( "Web Service URI not configured" );
				throw new ApplicationException( "Web Service URI not configured" );
			}

			//
			// Register the user and as an iFolder member with
			// the mDnsResponder
			//
			try
			{
				// Temp
				string	key = "1234567890";
				short sport = (short) webServiceUri.Port;

				kErrorType status =
					RegisterLocalMember( 
						User.mDnsUserID, 
						User.mDnsUserName,
						IPAddress.HostToNetworkOrder( sport ),
						webServiceUri.AbsolutePath,
						key.Length, 
						key, 
						ref userHandle );

				if ( status != kErrorType.kDNSServiceErr_NoError )
				{
					throw new SimiasException( "Failed to register local member with Rendezvous" );
				}
			}
			catch( Exception e2 )
			{
				log.Error( e2.Message );
				log.Error( e2.StackTrace );
			}			
		}

		internal static void UnregisterUser()
		{
			if ( registered == true )
			{
				DeregisterLocalMember( User.mDnsUserID, userHandle.ToInt32() );
				registered = false;
			}
		}

		internal static void StartMemberBrowsing()
		{
			User.browseHandle = new IntPtr( 0 );
			User.browseThread = new Thread( new ThreadStart( User.BrowseThread ) );
			User.browseThread.IsBackground = true;
			User.browseThread.Start();
		}

		internal static void StopMemberBrowsing()
		{
			if ( browseHandle.ToInt32() != 0 )
			{
				BrowseMembersShutdown( browseHandle.ToInt32() );
				Thread.Sleep( 1000 );
				if (User.browseThread.IsAlive == true )
				{
					// Shutdown the thread
					//User.browseThread.Interrupt();
				}
			}
		}

		internal static void BrowseThread()
		{
			User.kErrorType status;
			User.browseHandle = new IntPtr( 0 );
			MemberBrowseCallback myCallback = new MemberBrowseCallback( MemberCallback );

			do
			{
				status = BrowseMembersInit( myCallback, ref User.browseHandle );
				if ( status == User.kErrorType.kDNSServiceErr_NoError )
				{
					// A timeout is returning success so we're OK
					status = BrowseMembers( User.browseHandle.ToInt32(), 300 );
				}
			} while ( status == User.kErrorType.kDNSServiceErr_NoError );

			log.Debug( "BrowseThread down..." );
			log.Debug( "Status: " + status.ToString() );
		}

		internal
		static 
		bool 
		MemberCallback( 
			int			handle,
			int			flags,
			uint		ifIndex,
			kErrorType	errorCode,
			[MarshalAs(UnmanagedType.LPStr)] string serviceName,
			[MarshalAs(UnmanagedType.LPStr)] string regType,
			[MarshalAs(UnmanagedType.LPStr)] string domain,
			[MarshalAs(UnmanagedType.I4)] int context)
		{ 
			if ( errorCode == kErrorType.kDNSServiceErr_NoError )
			{
				// FIXME:: Need to handle the case where flags isn't set
				// to add so I can remove users as well

				log.Debug( "MemberCallback for: " + serviceName );

				RendezvousUser user = new RendezvousUser();
				user.ID = serviceName;

				bool found = false;
				foreach( RendezvousUser rUser in User.memberList )
				{
					if ( rUser.ID == user.ID )
					{
						found = true;
						break;
					}
				}

				if ( found == false )
				{
					// Since we're in an mDns callback just set the ID
					// and we'll pick up the rest of the meta-data in our
					// context

					log.Debug( "Adding: " + user.ID + " as a staged entry" );
					lock( User.memberListLock )
					{
						User.memberList.Add( user );
					}

					// Force a meta-data sync
					Simias.mDns.Sync.SyncNow("");
				}
			}
			else
			{
				log.Debug( 
					"Received an error on MemberCallback.  status: " + errorCode.ToString() );
			}
			return true;
		}
		#endregion

		#region Public Methods


		/// <summary>
		/// FIXME::Temporary method to automatically synchronize all mDns users
		/// </summary>
		/// <returns>n/a</returns>
		public void SynchronizeMembers()
		{
			// FIXME::define sizes
			char[] trimNull = { '\0' };
			char[] infoHost = new char[ 64 ];
			char[]	infoName = new char[ 128 ];
			char[]	infoServicePath = new char[ 128 ];
			byte[]	infoPublicKey = new byte[ 128 ];
			//int		infoPort = 0;

			log.Debug( "Syncing mDns members" );
			Simias.Storage.Member mdnsMember = null;
			Simias.Storage.Domain mdnsDomain = Store.GetStore().GetDomain( Simias.mDns.Domain.ID );

			lock( User.memberListLock )
			{
				foreach( RendezvousUser rUser in User.memberList )
				{
					// Check for a staged user
					if ( rUser.FriendlyName == null || rUser.FriendlyName == "" )
					{
						// Go get the rest of the meta-data for this user
						User.kErrorType status;

						try
						{
							log.Debug( "Calling GetMemberInfo for: " + rUser.ID );
							status = 
								GetMemberInfo( 
									rUser.ID, 
									infoName,
									infoServicePath,
									infoPublicKey,
									infoHost,
									ref rUser.Port );
							if ( status == kErrorType.kDNSServiceErr_NoError )
							{
								rUser.FriendlyName = (new string( infoName )).TrimEnd( trimNull );
								rUser.Host = (new string( infoHost )).TrimEnd( trimNull );
								rUser.ServicePath = (new string( infoServicePath )).TrimEnd( trimNull );
								//rUser.PublicKey = (new string( infoPublicKey )).TrimEnd( trimNull );
								log.Debug( "Adding meta-data for: " + rUser.FriendlyName );
							}
						}
						catch ( Exception e2 )
						{
							log.Debug( e2.Message );
							log.Debug( e2.StackTrace );
						}
					}

					// If we have all the meta-data for this user
					// see if he exists in the store.
					if ( rUser.FriendlyName != null )
					{
						mdnsMember = mdnsDomain.GetMemberByName( rUser.FriendlyName );
						if ( mdnsMember == null )
						{
							mdnsMember = 
								new Member( rUser.FriendlyName, rUser.ID, Access.Rights.ReadOnly );

							if ( rUser.PublicKey != null && rUser.PublicKey != "" )
							{
								mdnsMember.Properties.AddProperty( "PublicKey", rUser.PublicKey );
							}

							mdnsDomain.Commit( new Node[] { mdnsMember } );
						}

					}
				}
			}
		}

		public 
		delegate 
		bool 
		MemberBrowseCallback(
			int			handle,
			int			flags,
			uint		ifIndex,
			kErrorType	errorCode,
			[MarshalAs(UnmanagedType.LPStr)] string serviceName,
			[MarshalAs(UnmanagedType.LPStr)] string regType,
			[MarshalAs(UnmanagedType.LPStr)] string domain,
			[MarshalAs(UnmanagedType.I4)] int context);
		}
		#endregion
	}

