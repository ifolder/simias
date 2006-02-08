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

namespace Simias.MdbProvider
{
	public struct MdbValueStruct
	{
		public IntPtr Value;
		public int Used;
		public int ErrNo;
		public IntPtr Interface;
	}

	public class MdbUser : IDisposable
	{
		#region Class Members
		private string userDN;
		private string lastName = null;
		private string fullName = null;
		private string givenName = null;
		private readonly string delim = "\\";
		
		private IntPtr valueStruct = IntPtr.Zero;
		
		public const string MdbAttrLastName = "Surname";
		public const string MdbAttrFirstName = "Given Name";
		public const string MdbAttrFullName = "Full Name";
		
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
	
		[DllImport( Mdb.HulaLib )]
		protected static extern 
		int MDBRead( string ObjectDN, string Attribute, IntPtr ValueStruct );
	
		[DllImport( Mdb.HulaLib) ]
		protected static extern IntPtr MDBShareContext( IntPtr ValueStruct );
		#endregion

		#region Properties
		/// <summary>
		/// DN - Returns the user's distinguished name in the MDB database
		/// normally in a \Tree\Container\User format.
		/// </summary>
		public string DN
		{
			get{ return userDN; }
		}

		/// <summary>
		/// FullName - Returns the user's full name in the MDB database
		/// the full name is not a required MDB attribute so this property
		/// may return null.
		/// </summary>
		public string FullName
		{
			get{ return fullName; }
		}
		
		/// <summary>
		/// FullName - Returns the user's given or first name in the MDB database
		/// the given name is not a required MDB attribute so this property
		/// may return null.
		/// </summary>
		public string GivenName
		{
			get{ return givenName; }
		}
		
		/// <summary>
		/// LastName - Returns the user's last or sur name in the MDB database
		/// </summary>
		public string LastName
		{
			get{ return lastName; }
		}
		
		/// <summary>
		/// Username - Returns the user's common/userid in the MDB database
		/// </summary>
		public string UserName
		{
			get
			{
				string[] comps = userDN.Split( delim.ToCharArray() );
				if ( comps.Length > 0 )
				{
					return( comps[comps.Length - 1] );
				}
				
				return null;
			}
		}
		#endregion
		
		#region Constructors
		/// <summary>
		/// MDB User Constructor
		/// requires an MdbHandle returned as a property
		/// of the Mdb object and the distinguished name of
		/// the user.
		/// </summary>
		public MdbUser( MdbHandle Handle, string DN )
		{
			if ( Handle == IntPtr.Zero || DN == null || DN == "" )
			{
				throw new ArgumentNullException();
			}
			
			userDN = DN;
			
			this.valueStruct = MDBCreateValueStruct( Handle, userDN );
			if ( this.valueStruct  == IntPtr.Zero )
			{
				throw new ApplicationException( "could not create a ValueStruct" );
			}
			
			StringBuilder classSB = new StringBuilder( 256 );
			StringBuilder dnSB = new StringBuilder( 256 );
			
			if ( MDBGetObjectDetails( userDN, classSB, null, dnSB, valueStruct ) == false )
			{
				throw new ApplicationException( "failed to get object details for: " + userDN );
			}
			
			if ( classSB.ToString() != "User" )
			{
				throw new ApplicationException( userDN + ": is not an object of class type \"User\"" );
			}

			GetUserAttributes();			
		}
		
		~MdbUser()
		{
			Cleanup();
		}
		#endregion

		#region Private Methods
		private void Cleanup()
		{
			if ( valueStruct != IntPtr.Zero )
			{
				MDBDestroyValueStruct( valueStruct );
			}
		}
		
		private void GetUserAttributes()
		{
			int count;
			int currentOffset = 0;

			IntPtr tmpvs = MDBShareContext( this.valueStruct );
			if ( tmpvs == IntPtr.Zero )
			{
				throw new ApplicationException( "could not share a ValueStruct" );
			}
			
			// Given name
			try
			{
				count = MDBRead( userDN, MdbUser.MdbAttrFirstName, tmpvs );
				if ( count > 0 )
				{
					MdbValueStruct mdbvs = ( MdbValueStruct ) 
						Marshal.PtrToStructure( tmpvs, typeof( MdbValueStruct ) );
					this.givenName = 
						Marshal.PtrToStringAnsi( Marshal.ReadIntPtr( mdbvs.Value ) );
					currentOffset++;
				}
			}
			catch{}
			
			// Last name
			try
			{
				count = MDBRead( this.userDN, MdbUser.MdbAttrLastName, tmpvs );
				if ( count > 0 )
				{
					MdbValueStruct mdbvs = ( MdbValueStruct ) 
						Marshal.PtrToStructure( tmpvs, typeof( MdbValueStruct ) );
					this.lastName = 
						Marshal.PtrToStringAnsi( Marshal.ReadIntPtr( mdbvs.Value, currentOffset * IntPtr.Size ) );
					currentOffset++;
				}
			}
			catch{}
			
			// Full name
			try
			{
				count = MDBRead( this.userDN, MdbUser.MdbAttrFullName, tmpvs );
				if ( count > 0 )
				{
					MdbValueStruct mdbvs = ( MdbValueStruct ) 
						Marshal.PtrToStructure( tmpvs, typeof( MdbValueStruct ) );
					this.fullName = 
						Marshal.PtrToStringAnsi( Marshal.ReadIntPtr( mdbvs.Value, currentOffset * IntPtr.Size ) );
				}
			}
			catch{}

			MDBDestroyValueStruct( tmpvs );
		}
		#endregion
		
		#region Public Methods
		public void Dispose()
		{
			Cleanup();
			System.GC.SuppressFinalize( this );
		}
		#endregion
	}
}	
		
