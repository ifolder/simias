/***********************************************************************
 *  $RCSfile: IProvisionUser.cs,v $
 *
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Russ Young ryoung@novell.com
 *
 ***********************************************************************/
using System;
using System.Collections;

using Simias.Host;
using Simias.Service;
using Simias.Storage;

namespace Simias.Server
{
	/// <summary>
	/// ProvisionService
	/// Simple class for registering/deregistering provisioning
	/// providers.  The system only supports one registered provider.
	/// 
	/// External agents call the static method ProvisionUser to
	/// provision a simias user into the system.
	/// </summary>
	public class ProvisionService
	{
		private delegate Simias.Host.HostInfo hostProvisionMethod( string userName );
		private static hostProvisionMethod callout;
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger( typeof( ProvisionService ) );
		
		/// <summary>
		/// Register a provisioning provider
		/// </summary>
		public static void RegisterProvider( IProvisionUserProvider provider )
		{
			logger.Debug( "Registering {0}", provider.GetType().ToString() );
			callout = new hostProvisionMethod( provider.ProvisionUser );	
		}

		/// <summary>
		/// Unregister a previously registered provisioning provider.
		/// </summary>
		public static void UnRegisterProvider( IProvisionUserProvider provider )
		{
			logger.Debug( "Unregistering {0}", provider.GetType().ToString() );
			callout -= new hostProvisionMethod( provider.ProvisionUser );
		}

		/// <summary>
		/// Method to provision a member into the Simias system.
		/// The actual provisioning is handled by the registered
		/// provider.
		/// </summary>
		public static Simias.Host.HostInfo ProvisionUser( string Username )
		{
			if (callout != null)
			{
				logger.Debug( "Provsioning user {0}", Username );
				return callout( Username );
			}
			else
			{
				logger.Debug( "Provisioning user {0} using the default algorithm", Username );
				return defaultProvisionUser( Username );
			}
		}

		private static Simias.Host.HostInfo defaultProvisionUser( string Username )
		{
			// Return the host we are running on.
			return new Simias.Host.HostInfo( HostNode.GetLocalHost() );
		}
	}

	/// <summary>
	/// Summary description for IProvisionUser.
	/// </summary>
	public interface IProvisionUserProvider
	{
		/// <summary>
		/// </summary>
		Simias.Host.HostInfo ProvisionUser( string userName );
	}

	/// <summary>
	/// Load Balance is a user provisioning provider that attempts
	/// to keep the number of users across the all servers balanced.
	/// 
	/// The provider does not attempt to balance load just the actual
	/// number of users.
	/// </summary>
	public class LoadBalanceProvisionUserProvider : IProvisionUserProvider
	{
		private class HostEntry : IComparable
		{
			HostNode	host;
			int			userCount;

			internal HostEntry( HostNode hNode )
			{
				host = hNode;
				ICSList members = hNode.GetHostedMembers();
				userCount = members.Count;
			}

			internal Simias.Host.HostInfo Info
			{
				get { return new Simias.Host.HostInfo( host ); }
			}

			internal HostNode Host
			{
				get { return host; }
			}

			internal void AddMember( Domain domain, Member member )
			{
				member.HomeServer = host;
				System.Threading.Interlocked.Increment( ref userCount );
			}

			#region IComparable Members
			public int CompareTo( object obj )
			{
				HostEntry he = obj as HostEntry;
				return userCount.CompareTo( he.userCount );
			}
			#endregion
		}

		ArrayList hosts = new ArrayList();
		Domain domain;
		EventSubscriber nodeEvents;

		/// <summary>
		/// Constructor for the load balance provider.
		/// 
		/// Read all the hosts that are members of the domain and
		/// keep in a sorted list.  Register for member node changes
		/// so we know when new hosts come and go in the system.
		/// </summary>
		public LoadBalanceProvisionUserProvider()
		{
			Store store = Store.GetStore();
			domain = store.GetDomain( store.DefaultDomain );
			HostNode[] hArray = HostNode.GetHosts( domain.ID );
			foreach ( HostNode host in hArray )
			{
				HostEntry hostentry = new HostEntry( host );
				hosts.Add( hostentry );
			}
			
			hosts.Sort();
			nodeEvents = new EventSubscriber( domain.ID );
			nodeEvents.NodeTypeFilter = Simias.Client.NodeTypes.MemberType;
			nodeEvents.NodeCreated += new NodeEventHandler( es_NodeCreated );
		}

		/// <summary>
		/// Dispose
		/// Unregister from the event system.
		/// </summary>
		~LoadBalanceProvisionUserProvider()
		{
			nodeEvents.NodeCreated -= new NodeEventHandler( es_NodeCreated );
		}

		private void es_NodeCreated( Simias.Client.Event.NodeEventArgs args )
		{
			Member member = domain.GetNodeByID( args.Node ) as Member;
			if (member.IsType( HostNode.HostNodeType ) )
			{
				HostNode hostnode = new HostNode( member );
				HostEntry hostentry = new HostEntry( hostnode );
				hosts.Add( hostentry );
			}
		}
	
		#region IProvisionUserProvider Members
		/// <summary>
		/// </summary>
		public Simias.Host.HostInfo ProvisionUser( string Username )
		{
			HostNode hostnode = null;
			Member member = domain.GetMemberByName( Username );
			if ( member != null )
			{
				hostnode = member.HomeServer;
				if ( hostnode == null )
				{
					HostEntry hostentry = hosts[0] as HostEntry;
					hostnode = hostentry.Host;
					hostentry.AddMember( domain, member );
					lock ( hosts )
					{
						hosts.Sort();
					}
				}
			}

			return hostnode == null ? null : new Simias.Host.HostInfo( hostnode );
		}
		#endregion
	}

	/// <summary>
	/// </summary>
	public class AttributeProvisionUserProvider : IProvisionUserProvider
	{
		#region IProvisionUserProvider Members
		/// <summary>
		/// </summary>
		public Simias.Host.HostInfo ProvisionUser( string userName )
		{
			// TODO:  Add AttributeProvisionUserProvider.ProvisionUser implementation
			return null;
		}
		#endregion
	}
}
