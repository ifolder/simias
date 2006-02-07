/***********************************************************************
 *  $RCSfile: EnumUsers.cs,v $
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
 *  Author: Brady Anderson (banderso@novell.com)
 * 
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

using MdbHandle = System.IntPtr;

namespace Simias.MdbSync
{
	/// <summary>
	/// Class to enumerate Users in a specified container
	/// in an MDB identity store.
	/// </summary>
	public class EnumUsers : IDisposable, IEnumerator
	{
		#region Class Members
		private bool deep;
		private MdbHandle mdbHandle;
		private string containerDN;
		//private IntPtr e = IntPtr.Zero;
		private IntPtr v = IntPtr.Zero;
		private int count;
		private int offset;
		MdbValueStruct mdbvs;
		#endregion
		
		#region Constructors
		public EnumUsers( MdbHandle Handle, string ContainerDN, bool Deep )
		{
			mdbHandle = Handle;
			containerDN = ContainerDN;
			deep = Deep;
		}
		
		~EnumUsers()
		{
			Cleanup();
		}
		
		#endregion

		#region Private Methods
		private void Cleanup()
		{
			if ( this.v != IntPtr.Zero )
			{
				MDBDestroyValueStruct( v );
			}	
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
		
		#region IEnumerator Members
		/// <summary>
		/// Sets the enumerator to its initial position
		/// </summary>
		public void Reset()
		{
			this.v = MDBCreateValueStruct( mdbHandle, containerDN );
			if ( v == IntPtr.Zero )
			{
				throw new ApplicationException( "Couldn't create ValueStruct" );
			}

			/*
			this.e = MDBCreateEnumStruct( v );
			if ( this.e == IntPtr.Zero )
			{
				throw new ApplicationException( "Couldn't create EnumStruct" );
			}
			*/
			
			// BUGBUG for now we're not traversing deep
			offset = -1;			
			count = MDBEnumerateObjects( containerDN, "User", null, v );
			if ( count > 0 )
			{
				Console.WriteLine( "enumerated: " + count.ToString() );
				mdbvs = ( MdbValueStruct ) Marshal.PtrToStructure( v, typeof( MdbValueStruct ) );
			}
			
			if ( deep == true )
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get
			{
				if ( count > 0 && offset >= 0 )
				{
					int off = ( offset == count ) ? offset - 1 : offset;
					string userCN = 
						Marshal.PtrToStringAnsi( Marshal.ReadIntPtr( mdbvs.Value, off * IntPtr.Size ) );
					
					if ( userCN != null && userCN != "" )
					{
						return new Simias.MdbSync.User( mdbHandle, containerDN + "\\" + userCN );
					}
				}
				
				return null;
			}			
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; 
		/// false if the enumerator has passed the end of the collection.
		/// </returns>
		public bool MoveNext()
		{
			if ( count > 0 )
			{
				if ( offset < count )
				{
					offset++;
					if ( offset == count )
					{
						MDBDestroyValueStruct( v );
						this.v = IntPtr.Zero;
						return false;
					}
					
					return true;
				}
			}
			
			return false;

			/*			
			string userDN = MDBEnumerateObjectsEx( containerDN, null, null, (uint) 1, e, v );
			if ( userDN != null )
			{
				Console.WriteLine( "User: " + userDN );
				this.currentUser = new Simias.MdbSync.User( this.mdbHandle, userDN );
				return true;
			}
			
			MDBDestroyEnumStruct( e, v );
			MDBDestroyValueStruct( v );
			*/
			//Console.WriteLine( "MoveNext() returning false" );
			//return false;
		}
		#endregion
		
		#region Dll Imports
		/* Native MDB functions */
		[DllImport( Mdb.HulaLib )]
		protected static extern
		IntPtr 
		MDBCreateValueStruct( MdbHandle Handle, string Context );
		
		[DllImport( Mdb.HulaLib )]
		protected static extern bool MDBDestroyValueStruct( IntPtr ValueStruct );
				
		/* MDBValueStruct *	MDBShareContext(
								MDBValueStruct	*Vorg); */
		[DllImport( Mdb.HulaLib )]
		protected static extern IntPtr MDBShareContext( IntPtr ValueStruct );
				
		/* BOOL	MDBSetValueStructContext(
					const unsigned char	*Context,
					MDBValueStruct	*V); */
		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool
		MDBSetValueStructContext( string Context, IntPtr ValueStruct );

		/* 
		 *MDBEnumStruct	*MDBCreateEnumStruct( MDBValueStruct	*V ); 
		 */
		[DllImport( Mdb.HulaLib )]
		protected static extern IntPtr MDBCreateEnumStruct( IntPtr ValueStruct );
		 
		/* 
		 * BOOL	MDBDestroyEnumStruct(
					MDBEnumStruct	*E,
					MDBValueStruct	*V );
		 */
		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool 
		MDBDestroyEnumStruct( IntPtr EnumStruct, IntPtr ValueStruct );

		[DllImport( Mdb.HulaLib )]
		protected static extern
		int 
		MDBRead( string Obj, string Attribute, IntPtr ValueStruct );

		[DllImport( Mdb.HulaLib )]
		protected static extern
		string MDBReadEx(
			string Obj,
			string Attribute,
			IntPtr EnumStruct,
			IntPtr valueStruct);
		
		[DllImport( Mdb.HulaLib )]
		protected static extern
		int MDBReadDN( string ObjectDN, string Attribute, IntPtr ValueStruct );
				
		/*
		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool MDBWrite( string obj, string attribute, IntPtr valueStruct );
				
		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool MDBRemove(
			string obj, string attribute, string val, IntPtr valueStruct );	
			
		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool MDBCreateObject(
			string Obj, 
			string Class, 
			IntPtr Attribute,
			IntPtr Data,
			IntPtr V );
		
		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool MDBDeleteObject( string Obj, bool Recursive, IntPtr ValueStruct );
		*/
				
		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool MDBGetObjectDetails(
				string dn, 
				StringBuilder type, 
				StringBuilder rdn,
				StringBuilder adn, 
				IntPtr valueStruct );

		[DllImport( Mdb.HulaLib )]
		protected static extern
		int MDBEnumerateObjects(
			string dn, 
			string type, 
			string pattern, 
			IntPtr valueStruct );
				
		/* online doc
			* const unsigned char*	MDBEnumerateObjectsEx(
					const unsigned char	*Container,
					const unsigned char	*Type,
					const unsigned char	*Pattern,
					unsigned int Flags,			** set to true/1 if you want the result set to include containers ??
					MDBEnumStruct	*E,
					MDBValueStruct	*V
					);
			the mdbfile driver doesn't seem to respect the flag parameter
			from looking at the source, the eDir driver should filter out(0) or include(1) containers
		*/
		[DllImport( Mdb.HulaLib )]
		protected static extern
		string MDBEnumerateObjectsEx(
			string DN, 
			string Type, 
			string Pattern, 
			uint Flags, 
			IntPtr EnumStruct, 
			IntPtr ValueStruct );

		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool MDBIsObject( string Object, IntPtr ValueStruct );
		
		/*
		[DllImport( Mdb.HulaLib )]
		private static extern
		bool MDBVerifyPassword(
			string Object, string Password, IntPtr ValueStruct );
		*/
		#endregion
	}
}	
