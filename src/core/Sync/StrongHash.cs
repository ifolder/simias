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
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.Security.Cryptography;

namespace Simias.Sync.Delta
{
	/// <summary>
	/// Class to compute a strong Hash for a block of data.
	/// </summary>
	public class StrongHash
	{
		MD5		md5 = new MD5CryptoServiceProvider();
			
		/// <summary>
		/// Computes an MD5 hash of the data block passed in.
		/// </summary>
		/// <param name="buffer">The data to hash.</param>
		/// <param name="offset">The offset in the byte array to start hashing.</param>
		/// <param name="count">The number of bytes to include in the hash.</param>
		/// <returns>The hash code.</returns>
		public byte[] ComputeHash(byte[] buffer, int offset, int count)
		{
			return md5.ComputeHash(buffer, offset, count);
		}
	}
}
