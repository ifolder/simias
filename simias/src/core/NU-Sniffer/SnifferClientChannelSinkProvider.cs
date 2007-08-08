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
 | Author: Author: Rob
 |***************************************************************************/
 
using System;
using System.Collections;
using System.Runtime.Remoting.Channels;

using Simias;

namespace Simias.Sniffer
{
	/// <summary>
	/// Sniffer Client Channel Sink Provider
	/// </summary>
	public class SnifferClientChannelSinkProvider : IClientChannelSinkProvider
	{
		private IClientChannelSinkProvider nextProvider;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public SnifferClientChannelSinkProvider()
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		public SnifferClientChannelSinkProvider(IDictionary properties, ICollection providerData)
		{
		}
		
		#region IClientChannelSinkProvider Members

		IClientChannelSink IClientChannelSinkProvider.CreateSink(IChannelSender channel, string url, object remoteChannelData)
		{
			IClientChannelSink nextSink = null;
			
			if (nextProvider != null)
			{
				nextSink = nextProvider.CreateSink(channel, url, remoteChannelData);
			}

			return new SnifferClientChannelSink(nextSink);
		}

		IClientChannelSinkProvider IClientChannelSinkProvider.Next
		{
			get { return nextProvider; }

			set { nextProvider = value; }
		}

		#endregion
	}
}
