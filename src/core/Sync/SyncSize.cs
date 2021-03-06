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
*                 $Author: Dale Olds <olds@novell.com>
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Simias.Storage;
using Simias;
using Simias.Sync.Client;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// class to approximate amount of data that is out of sync with master
/// Note that this is worst-case of data that may need to be sent from
/// this collection to the master. It does not include data that may need
/// to be retrieved from the master. It also does not account for
/// delta-sync algorithms that may reduce what needs to be sent
/// </summary>
	public class SyncSize
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="col"></param>
		/// <param name="nodeCount"></param>
		/// <param name="maxBytesToSend"></param>
		public static void CalculateSendSize(Collection col, out uint nodeCount, out ulong maxBytesToSend)
		{
			Log.log.Debug("starting to calculate size to send to master for collection {0}", col.Name);

			maxBytesToSend = 0;
			nodeCount = 0;

			maxBytesToSend = SyncClient.GetSizeToSync(col.ID, out nodeCount);
		}
	}

}
//===========================================================================
