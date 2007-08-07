/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |
 |	Mono.ASPNET.IWebSource
 |
 |	Authors:
 |	Gonzalo Paniagua Javier (gonzalo@ximian.com)
 |	Lluis Sanchez Gual (lluis@ximian.com)
 |
 |***************************************************************************/


using System;
using System.Net.Sockets;

namespace Mono.ASPNET
{
	public interface IWebSource
	{
		Socket CreateSocket ();
		IWorker CreateWorker (Socket client, ApplicationServer server);
		Type GetApplicationHostType ();
		IRequestBroker CreateRequestBroker ();
	}
	
	public interface IWorker
	{
		void Run (object state);
		int Read (byte[] buffer, int position, int size);
		void Write (byte[] buffer, int position, int size);
		void Close ();
		void Flush ();
	}
}
