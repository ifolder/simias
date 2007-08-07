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
 | Mono.ASPNET.BaseRequestBroker
 |
 | Authors:
 |	Gonzalo Paniagua Javier (gonzalo@ximian.com)
 |	Lluis Sanchez Gual (lluis@ximian.com)
 |	
 |***************************************************************************/
 

using System;
using System.Collections;

namespace Mono.ASPNET
{
	public class BaseRequestBroker: MarshalByRefObject, IRequestBroker
	{
		ArrayList requests = new ArrayList ();
		Queue freeSlots = new Queue ();
		
		internal int RegisterRequest (IWorker worker)
		{
			lock (requests)
			{
				if (freeSlots.Count == 0)
					return requests.Add (worker);
				
				int freeSlot = (int)freeSlots.Dequeue ();
				requests [freeSlot] = worker;
				return freeSlot;
			}
		}
		
		internal void UnregisterRequest (int id)
		{
			lock (requests)
			{
				requests [id] = null;
				freeSlots.Enqueue (id);
			}
		}
		
		public int Read (int requestId, int size, out byte[] buffer)
		{
			buffer = new byte[size];
			IWorker w;
			lock (requests) {
				w = (IWorker) requests [requestId];
			}
			int nread = w.Read (buffer, 0, size);
			return nread;
		}
		
		public IWorker GetWorker (int requestId)
		{
			lock (requests) {
				return (IWorker) requests [requestId];
			}
		}
		
		public void Write (int requestId, byte[] buffer, int position, int size)
		{
			GetWorker (requestId).Write (buffer, position, size);
		}
		
		public void Close (int requestId)
		{
			GetWorker (requestId).Close ();
		}
		
		public void Flush (int requestId)
		{
			GetWorker (requestId).Flush ();
		}

		public override object InitializeLifetimeService ()
		{
			return null;
		}
	}
}
