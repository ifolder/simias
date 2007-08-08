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
 |  Author: Rob
 |***************************************************************************/
 
using System;
using System.Collections;
using System.Runtime.Remoting.Channels;

using Simias;

namespace Simias.Sniffer
{
	/// <summary>
	/// Sniffer Server Channel Sink Provider
	/// </summary>
	public class SnifferServerChannelSinkProvider : IServerChannelSinkProvider
	{
		private IServerChannelSinkProvider nextProvider;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public SnifferServerChannelSinkProvider()
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		public SnifferServerChannelSinkProvider(IDictionary properties, ICollection providerData)
		{
		}
		
		#region IServerChannelSinkProvider Members

		/// <summary>
		/// Create a new channel sink.
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		public IServerChannelSink CreateSink(IChannelReceiver channel)
		{
			IServerChannelSink nextSink = null;
			
			if (nextProvider != null)
			{
				nextSink = nextProvider.CreateSink(channel);
			}

			return new SnifferServerChannelSink(nextSink);
		}

		/// <summary>
		/// The next provider in the chain.
		/// </summary>
		public IServerChannelSinkProvider Next
		{
			get { return nextProvider; }

			set { nextProvider = value; }
		}

		/// <summary>
		/// Ignored.
		/// </summary>
		/// <param name="channelData"></param>
		public void GetChannelData(IChannelDataStore channelData)
		{
		}

		#endregion
	}
}
