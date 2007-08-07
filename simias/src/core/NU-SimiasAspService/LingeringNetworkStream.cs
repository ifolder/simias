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
 | Mono.ASPNET.ApplicationServer
 |
 | Authors:
 |	Gonzalo Paniagua Javier (gonzalo@ximian.com)
 |
 |***************************************************************************/



using System;
using System.Net.Sockets;

namespace Mono.ASPNET
{
	public class LingeringNetworkStream : NetworkStream 
	{
		const int useconds_to_linger = 2000000;
		const int max_useconds_to_linger = 30000000;
		bool enableLingering = true;

		public LingeringNetworkStream (Socket sock, bool owns) : base (sock, owns)
		{
		}
		
		public bool EnableLingering
		{
			get { return enableLingering; }
			set { enableLingering = value; }
		}

		void LingeringClose ()
		{
			int waited = 0;
			byte [] buffer = null;

			Socket.Shutdown (SocketShutdown.Send);
			while (waited < max_useconds_to_linger) {
				int nread = 0;
				try {
					if (!Socket.Poll (useconds_to_linger, SelectMode.SelectRead))
						break;

					if (buffer == null)
						buffer = new byte [512];

					nread = Socket.Receive (buffer, 0, buffer.Length, 0);
				} catch { }

				if (nread == 0)
					break;

				waited += useconds_to_linger;
			}
		}

		public override void Close ()
		{
			if (enableLingering) {
				try {
					LingeringClose ();
				} finally {
					base.Close ();
				}
			}
			else
				base.Close ();
		}

		public bool Connected {
			get { return Socket.Connected; }
		}
	}
}
