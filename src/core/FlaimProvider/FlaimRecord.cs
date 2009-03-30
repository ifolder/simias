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
using System.Xml;

namespace Simias.Storage.Provider.Flaim
{
	/// <summary>
	/// Class to handle flaim records.
	/// </summary>
	internal class FlaimRecord : Record
	{
		internal IntPtr pRecord = IntPtr.Zero;

        /// <summary>
        /// Constructor to create new object of Flaim Record
        /// </summary>
        /// <param name="recordEl"></param>
		internal FlaimRecord(XmlElement recordEl) :
			base(recordEl)
		{
		}

        /// <summary>
        /// Constructor to create new object of Flaim Record
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="type"></param>
		internal FlaimRecord(string name, string id, string type) :
			base(name, id, type)
		{
		}
	}
}
