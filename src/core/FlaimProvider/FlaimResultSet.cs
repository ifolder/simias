/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Russ Young
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
 
using System;
using Simias.Storage.Provider;
using System.Runtime.InteropServices;

namespace Simias.Storage.Provider.Flaim
{
	/// <summary>
	/// Summary description for FlaimObjectIterator.
	/// </summary>
	public class FlaimResultSet : MarshalByRefObject, IResultSet
	{
		private bool AlreadyDisposed = false;
		int			count;
		IntPtr		pResults;
		FlaimServer Flaim;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pResultSet"></param>
		/// <param name="count"></param>
		/// <param name="flaimServer"></param>	
		public FlaimResultSet(IntPtr pResultSet, int count, FlaimServer flaimServer)
		{
			this.count = count;
			this.pResults = pResultSet;
			Flaim = flaimServer;
		}

		/// <summary>
		/// 
		/// </summary>
		~FlaimResultSet()
		{
			Dispose(true);
		}

		/// <summary>
		/// Get the pointer to the resultset.
		/// </summary>
		internal IntPtr ResultSet
		{
			get {return pResults;}
		}

		#region IObjectIterator Members

		/// <summary>
		/// Method to return the next set of objects.
		/// All the objects that can fit in the buffer will be returned.
		/// returns false when no more objects exist.
		/// </summary>
		/// <param name="buffer">Buffer used to return the objects.</param>
		/// <returns>true - objects returned. false - no more objects</returns>
		public int GetNext(ref char[] buffer)
		{
			// TODO:  Add FlaimObjectIterator.GetNext implementation
			if (AlreadyDisposed || pResults == IntPtr.Zero)
			{
				return 0;
			}
			else
			{
				return Flaim.GetNext(this, ref buffer);
			}
		}

		/// <summary>
		/// Set the Index to the specified offset from the origin.
		/// </summary>
		/// <param name="origin">The origin to move from</param>
		/// <param name="offset">The offset to move the index by.</param>
		/// <returns>True if successful.</returns>
		public bool SetIndex(IndexOrigin origin, int offset)
		{
			return Flaim.SetIndex(this, origin, offset);
		}

		/// <summary>
		/// Property to get the count of available objects.
		/// </summary>
		public int Count
		{
			get
			{
				if (AlreadyDisposed || pResults == IntPtr.Zero)
				{
					return 0;
				}
				else
				{
					return count;
				}
			}
		}



		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inFinalize"></param>
		private void Dispose(bool inFinalize)
		{
			if (!AlreadyDisposed)
			{
				AlreadyDisposed = true;
				Flaim.CloseSearch(this);
			
				if (!inFinalize)
				{
					GC.SuppressFinalize(this);
				}
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Method to cleanup any Flaim resources held.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}
}
