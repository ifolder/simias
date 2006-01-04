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
	public struct MdbValueStruct
	{
		public IntPtr Value;
		public int Used;
		public int ErrNo;
		public IntPtr Interface;
	}

	public class EnumUsers : IDisposable, IEnumerator
	{
		#region Class Members
		private bool goDeep;
		private readonly string thisModule = "simias-mdb-sync";
		private MdbHandle mdbHandle;
		private string containerDN;
		private IntPtr e = System.IntPtr.Zero;
		private IntPtr v = System.IntPtr.Zero;
		private User currentUser = null;
		#endregion
		
		#region Constructors
		public EnumUsers( MdbHandle Handle, string ContainerDN, bool Deep )
		{
			mdbHandle = Handle;
			containerDN = ContainerDN;
			goDeep = Deep;
		}
		
		~EnumUsers()
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
		
		#region IEnumerator Members
		/// <summary>
		/// Sets the enumerator to its initial position
		/// </summary>
		public void Reset()
		{
			Console.WriteLine( "Calling Reset()" );
			this.v = MDBCreateValueStruct( mdbHandle, containerDN );
			if ( v == IntPtr.Zero )
			{
				throw new ApplicationException( "Couldn't create ValueStruct" );
			}

			this.e = MDBCreateEnumStruct( v );
			if ( this.e == IntPtr.Zero )
			{
				throw new ApplicationException( "Couldn't create EnumStruct" );
			}
		}

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get
			{
				return currentUser as object;
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
			Console.WriteLine( "MoveNext() called" );
			Console.WriteLine( "  container: " + containerDN );
			string userDN = MDBEnumerateObjectsEx( containerDN, null, null, (uint) 0, e, v );
			if ( userDN != null )
			{
				Console.WriteLine( "User: " + userDN );
				this.currentUser = new Simias.MdbSync.User( this.mdbHandle, userDN );
				return true;
			}
			
			MDBDestroyEnumStruct( e, v );
			MDBDestroyValueStruct( v );
			e = IntPtr.Zero;
			v = IntPtr.Zero;
			
			Console.WriteLine( "MoveNext() returning false" );
			return false;
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

		/* BOOL	MDBAddValue(
					const unsigned char	*Value,
					MDBValueStruct	*V ); */
		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool
		MDBAddValue( string Value, IntPtr ValueStruct );
				
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
		int MDBReadDN( string Obj, string Attribute, IntPtr ValueStruct );
				
		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool MDBWrite( string obj, string attribute, IntPtr valueStruct );
				
		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool MDBAdd(
				string obj, string attribute, string val, IntPtr valueStruct);
		
		/* This was a macro in mdb.h */
		protected static bool MDBAddStringAttribute(
				string attribute, string value, IntPtr attributes, IntPtr data) {
			string temp = "S" + attribute;
			
			if (!MDBAddValue(temp, attributes))
				return false;
			if (!MDBAddValue(value, data))
				return false;
				
			return true;
		}
		
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
				
		/*BOOL	MDBGetObjectDetails(
		 			const unsigned char	*Object,
					unsigned char	*Type,
					unsigned char	*RDN,
					unsigned char	*DN,
					MDBValueStruct	*V );*/

		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool MDBGetObjectDetails(
				string dn, 
				StringBuilder type, 
				StringBuilder rdn,
				StringBuilder adn, 
				IntPtr valueStruct );

		/*long	MDBEnumerateObjects(
					const unsigned char	*Container,
					const unsigned char	*Type,
					const unsigned char	*Pattern,
					MDBValueStruct	*V );*/

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

		/* const unsigned char	*MDBEnumerateAttributesEx(
								   const unsigned char	*Object,
								   MDBEnumStruct	*E,
								   MDBValueStruct	*V); */

		/* this function is not implemented in the mdb file driver yet (9/2005)
		 * [DllImport( Mdb.HulaLib )]
		protected static extern string MDBEnumerateAttributesEx(
			string Object, IntPtr enumStruct, IntPtr valueStruct);*/

		[DllImport( Mdb.HulaLib )]
		protected static extern
		bool MDBIsObject( string Obj, IntPtr ValueStruct );
		
		[DllImport( Mdb.HulaLib )]
		private static extern
		bool MDBVerifyPassword(
			string Obj, string Password, IntPtr ValueStruct );

		[DllImport( Mdb.HulaLib )]
		private static extern
		bool MDBChangePassword(
			string Obj, 
			string OldPassword, 
			string NewPassword, 
			IntPtr ValueStruct );
		
		[DllImport( Mdb.HulaLib )]
		private static extern
		bool MDBChangePasswordEx(
			string Obj, 
			string OldPassword,
			string NewPassword,
			IntPtr ValueStruct );
		#endregion	
	}
}	
