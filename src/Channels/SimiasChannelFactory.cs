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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Runtime.Serialization.Formatters;
using System.Reflection;

using Novell.Security.SecureSink;
using Novell.Security.SecureSink.SecurityProvider;
using Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider;

using Simias;
using Simias.Storage;
using Simias.Sniffer;

namespace Simias.Channels
{
	/// <summary>
	/// Simias Channel Factory
	/// </summary>
	public class SimiasChannelFactory
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SimiasChannelFactory));
		
		private static ulong index = 0;

		static SimiasChannelFactory()
		{
#if DEBUG
			// send the errors back on debug
			RemotingConfiguration.CustomErrorsEnabled(false);
#endif
			// TODO: remove or update
			string config = "remoting.config";

			try
			{
				RemotingConfiguration.Configure(config);
			}
			catch
			{
				config = "Not Found";
			}

			log.Debug("Remoting Configuration File: {0}", config);
		}

		/// <summary>
		/// Hidden constructor
		/// </summary>
		private SimiasChannelFactory()
		{
		}

		/// <summary>
		/// Get a Simias channel
		/// </summary>
		/// <param name="uri">Channel URI</param>
		/// <param name="sinks">Channel Sinks</param>
		/// <returns>A Simias Channel</returns>
		public static SimiasChannel Create(Uri uri, SimiasChannelSinks sinks)
		{
			return Create(uri, sinks, false);
		}

		/// <summary>
		/// Get a Simias channel
		/// </summary>
		/// <param name="uri">Channel URI</param>
		/// <param name="sinks">Channel Sinks</param>
		/// <param name="server">Server Channel?</param>
		/// <returns>A Simias Channel</returns>
		public static SimiasChannel Create(Uri uri, SimiasChannelSinks sinks, bool server)
		{
			IChannel channel = null;
			string name = null;

			lock(typeof(SimiasChannelFactory))
			{
				// name
				name = String.Format("Simias Channel {0}", ++index);
			}

			// setup channel properties
			ListDictionary props = new ListDictionary();
			props.Add("name", name);
			
			// server properties
			if (server)
			{
				props.Add("port", uri.Port);
				//props.Add("useIpAddress", true);
				
				//props.Add("bindTo", uri.Host);
			}
			
			// client properties
			else
			{
				props.Add("port", 0);
				props.Add("clientConnectionLimit", 5);

				// proxy
				//props.Add("proxyName", "");
				//props.Add("proxyPort", "");

				// TODO: why doesn't this work?
				//props.Add("timeout", TimeSpan.FromSeconds(30).Milliseconds);

			}

			// common properties
			//props.Add("machineName", uri.Host);

			// provider notes
			// server providers: security sink -> monitor sink -> formatter sink
			// client providers: formatter sink -> monitor sink -> security sink

			// server providers
			if (server)
			{
				IServerChannelSinkProvider serverProvider = null;

				// setup format provider
				if ((sinks & SimiasChannelSinks.Soap) > 0)
				{
					// soap
					serverProvider = new SoapServerFormatterSinkProvider();
					(serverProvider as SoapServerFormatterSinkProvider).TypeFilterLevel = TypeFilterLevel.Full;
				}
				else
				{
					// binary
					serverProvider = new BinaryServerFormatterSinkProvider();
					(serverProvider as BinaryServerFormatterSinkProvider).TypeFilterLevel = TypeFilterLevel.Full;
				}

				// setup monitor provider
				if ((sinks & SimiasChannelSinks.Sniffer) > 0)
				{
					IServerChannelSinkProvider serverMonitorProvider = new SnifferServerChannelSinkProvider();
					serverMonitorProvider.Next = serverProvider;
					serverProvider = serverMonitorProvider;
				}

				// setup security provider
				if ((sinks & SimiasChannelSinks.Security) > 0)
				{
					/* TODO: add back
					ISecurityServerFactory securityServerFactory = (ISecurityServerFactory) new RsaSecurityServerFactory(store.KeyStore);
					IServerChannelSinkProvider serverSecurityProvider = (IServerChannelSinkProvider) new SecureServerSinkProvider(securityServerFactory, SecureServerSinkProvider.MsgSecurityLevel.privacy);
					serverSecurityProvider.Next = serverProvider;
					serverProvider = serverSecurityProvider;
					*/
				}

				// create channel
				if (uri.Scheme.ToLower() == "http")
				{
					// http channel
					channel = new HttpServerChannel(props, serverProvider);
				}
				else
				{
					// tcp channel
					channel = new TcpServerChannel(props, serverProvider);
				}
			}

			// client providers
			else
			{
				IClientChannelSinkProvider clientProvider = null;

				// setup security provider
				if ((sinks & SimiasChannelSinks.Security) > 0)
				{
					/* TODO: add back
					ISecurityClientFactory[] secClientFactories = new ISecurityClientFactory[1];
					secClientFactories[0] = (ISecurityClientFactory) new RsaSecurityClientFactory(store.KeyStore);
					clientProvider = (IClientChannelSinkProvider) new SecureClientSinkProvider(secClientFactories);
					*/
				}

				// setup monitor provider
				if ((sinks & SimiasChannelSinks.Sniffer) > 0)
				{
					IClientChannelSinkProvider clientMonitorProvider = new SnifferClientChannelSinkProvider();
					clientMonitorProvider.Next = clientProvider;
					clientProvider = clientMonitorProvider;
				}

				// setup format provider
				if ((sinks & SimiasChannelSinks.Soap) > 0)
				{
					// soap
					IClientChannelSinkProvider clientFormatProvider = new SoapClientFormatterSinkProvider();
					clientFormatProvider.Next = clientProvider;
					clientProvider = clientFormatProvider;
				}
				else
				{
					// binary
					IClientChannelSinkProvider clientFormatProvider = new BinaryClientFormatterSinkProvider();
					clientFormatProvider.Next = clientProvider;
					clientProvider = clientFormatProvider;
				}


				// create channel
				if (uri.Scheme.ToLower() == "http")
				{
					// http channel
					channel = new HttpClientChannel(props, clientProvider);
				}
				else
				{
					// tcp channel
					channel = new TcpClientChannel(props, clientProvider);
				}
			}

			return new SimiasChannel(channel);
		}
	}
}
