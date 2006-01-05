/***********************************************************************
 *  $RCSfile: MdbUser.cs,v $
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
	public class User : IDisposable
	{
		#region Class Members
		private readonly string thisModule = "simias-mdb-sync";
		private string userDN;
		private string lastName;
		private string fullName;
		private string givenName;
		#endregion
		
		#region Dll Imports
		/* Native MDB functions */
		[DllImport( Mdb.HulaLib )]
		protected static extern
		IntPtr MDBCreateValueStruct( MdbHandle Handle, string Context );
		
		[DllImport( Mdb.HulaLib )]
		protected static extern bool MDBDestroyValueStruct( IntPtr ValueStruct );
		
		[DllImport( Mdb.HulaLib )]
		protected static extern 
		bool MDBGetObjectDetails(
			string DN, 
			StringBuilder Type, 
			StringBuilder Rdn,
			StringBuilder Adn,
			IntPtr ValueStruct);
		#endregion
		

		#region Properties
		public string DN
		{
			get{ return userDN; }
		}

		public string FullName
		{
			get{ return fullName; }
		}
		
		public string GivenName
		{
			get{ return givenName; }
		}
		
		public string LastName
		{
			get{ return lastName; }
		}
		
		public string UserName
		{
			get{ return userDN; }
		}
		#endregion
		
		#region Constructors
		public User( MdbHandle Handle, string DN )
		{
		
			if ( DN == null || DN == "" )
			{
				// throw new argexception;
			}
			
			userDN = DN;
			
			IntPtr valueStruct = MDBCreateValueStruct( Handle, userDN );
			if ( valueStruct  == IntPtr.Zero )
			{
				throw new ApplicationException( "could not create a ValueStruct" );
			}
			
			StringBuilder classSB = new StringBuilder( 256 );
			StringBuilder dnSB = new StringBuilder( 256 );
			
			if ( MDBGetObjectDetails( userDN, classSB, null, dnSB, valueStruct ) == false )
			{
				throw new ApplicationException( "failed to get object details for: " + userDN );
			}
			
			Console.WriteLine( dnSB.ToString() );
			
			if ( classSB.ToString() != "User" )
			{
				throw new ApplicationException( userDN + ": is not an object of class type \"User\"" );
			}
			
			// Grab given, lastname and full name
			
			MDBDestroyValueStruct( valueStruct );
		}
		
		~User()
		{
			Console.WriteLine( "calling deconstructor" );
			Cleanup();
		}
		#endregion

		#region Private Methods
		private void Cleanup()
		{
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
		
