/***********************************************************************
 *  $RCSfile: Mdb.cs,v $
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
 *  Author: Brady Anderson (banderso@novell.com)
 * 
 ***********************************************************************/
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

//using Simias;
//using Simias.Storage;

using MdbHandle = System.IntPtr;


namespace Simias.MdbSync
{
	public class Mdb : IDisposable
	{
		#region Class Members
		public const string HulaLib = "hulamdb";
		public const string HulaMessageApiLib = "hulamsgapi";
		private readonly string thisModule = "simias-mdb-sync";
		
		private int apiVersion = -1;
		private MdbHandle mdbHandle = System.IntPtr.Zero;
		private string hostDN;
		//private string username;
		//private string password;
		#endregion
		
		#region DllImports
		// Native MDB functions used via PInvoke
		[DllImport( HulaLib ) ]
		private static extern bool MDBInit();
		
		[DllImport( HulaLib ) ]
		private static extern bool MDBShutdown();
		
		[DllImport( HulaLib ) ] 
		private static extern 
		int 
		MDBGetAPIVersion(
			bool wantCompatibleVersion, 
			StringBuilder description,
			MdbHandle context);
		
		[DllImport( HulaLib ) ]
		private static extern
		bool 
		MDBGetServerInfo(
			StringBuilder hostDN, 
			StringBuilder hostTree,
			IntPtr valueStruct);
				
		[DllImport( HulaLib ) ]
		private static extern 
		MdbHandle
		MDBAuthenticate( string Module, string Principal, string Password );
		
		[DllImport( HulaLib )]
		private static extern
		bool 
		MDBRelease( MdbHandle handle );
		
		/* We have to call some msgapi function to force it to load for MDB 
		   This might be a bug.  This seems to happen only on Linux when 
		   running under Mono. */
 		[DllImport( HulaMessageApiLib)]
		private static extern MdbHandle MsgDirectoryHandle();
		
	    [DllImport( HulaMessageApiLib )]
	    public static extern bool MsgFindObject(string user, StringBuilder dn, string type, IntPtr nmap, IntPtr valueStruct);
		
		[DllImport( "hulamemmgr" )]
		public static extern bool MemoryManagerOpen(string agentName);
	
		[DllImport( "hulamemmgr" )]
		public static extern bool MemoryManagerClose(string agentName);
		#endregion

		#region Properties
		public MdbHandle Handle
		{
			get{ return mdbHandle; }
		}
		#endregion
		
		#region Constructors
		public Mdb( string UserDN, string Password )
		{
			if ( UserDN == null || Password == null )
			{
				//throw new ParmException();
				throw new ApplicationException( "Invalid Arguments" );
			}
			
			/*
			this.username = Username;
			this.password = Password;
			*/
			
			Console.WriteLine( "attempting to load the message api library first" );
			MsgDirectoryHandle();			
			Console.WriteLine( "message api library loaded" );
			
			Console.WriteLine( "attempting to load the memory manager library" );
			MemoryManagerOpen( "SimiasMdbSync" );			
			Console.WriteLine( "memory manager library loaded" );
			
			// Call the native initialization API
			if ( MDBInit() == false )
			{
				Console.WriteLine( "failed to load \"libhulamdb\"" );
				throw new ApplicationException( "Failed to load libhulamdb!" );
			}
			Console.WriteLine( "mdb library loaded" );
		
			StringBuilder host = new StringBuilder( 256 );
			StringBuilder hostTree = new StringBuilder( 256 );
			if ( MDBGetServerInfo( host, hostTree, System.IntPtr.Zero ) == true )
			{
				Console.WriteLine( "host tree: " + hostTree.ToString() );
				hostDN = host.ToString();
				Console.WriteLine( "host dn: " + hostDN );
			}
			
			apiVersion = MDBGetAPIVersion( false, null, System.IntPtr.Zero );
			Console.WriteLine( "API Version: " + apiVersion.ToString() );
			
			// Authenticate
			Console.WriteLine( "Authenticating to MDB" );
			Console.WriteLine( "  Username: " + UserDN );
			Console.WriteLine( "  Password: " + Password );
			
			/*
			StringBuilder userDN = new StringBuilder( 256 );
			if ( MsgFindObject( Username, userDN, null, IntPtr.Zero, IntPtr.Zero ) == true )
			{
				this.mdbHandle = MDBAuthenticate( "Hula", userDN.ToString(), Password );
				if ( this.mdbHandle == System.IntPtr.Zero )
				{
					Console.WriteLine( this.mdbHandle.ToString() );
					throw new ApplicationException( "Failed to authenticate against MDB" );
				}
			}
			*/
			
			this.mdbHandle = MDBAuthenticate( thisModule, UserDN, Password );
			if ( this.mdbHandle == System.IntPtr.Zero )
			{
				Console.WriteLine( this.mdbHandle.ToString() );
				throw new ApplicationException( "Failed to authenticate against MDB" );
			}
		}
		
		~Mdb()
		{
			Console.WriteLine( "calling deconstructor" );
			Cleanup();
		}
		#endregion

		#region Private Methods
		private void Cleanup()
		{
			if ( this.mdbHandle != System.IntPtr.Zero )
			{
				MDBRelease( this.mdbHandle );
				this.mdbHandle = System.IntPtr.Zero;
			}
			
			MDBShutdown();
		}
		#endregion

		#region Public Methods	
		public void Dispose()
		{
			Console.WriteLine( "calling Dispose" );
			Cleanup();
			System.GC.SuppressFinalize( this );
		}
		#endregion
	}		
}


