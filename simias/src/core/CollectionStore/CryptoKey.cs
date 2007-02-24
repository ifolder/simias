/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using System.Text;

using Simias;
using Simias.Client;
using Simias.Event;
using Simias.Policy;
using Simias.Storage.Provider;
using Simias.Sync;
using Persist = Simias.Storage.Provider;


namespace Simias.CryptoKey
{
	[Serializable]
	public sealed class CollectionKey
	{
		public string 	NodeID;
		public string	PEDEK;
		public string	REDEK;	

		public CollectionKey()
		{

		}
		public CollectionKey(string nodeID, string EncryptionKey, string RecoveryKey)
		{
			NodeID = nodeID;
			PEDEK = EncryptionKey;
			REDEK = RecoveryKey;				
		}
	}
}



