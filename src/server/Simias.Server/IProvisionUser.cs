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
*                 $Author: Russ Young ryoung@novell.com
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
using System.Collections;
using System.Reflection;

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
		private delegate string hostProvisionMethod( string userName );
		private static hostProvisionMethod callout = null;
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger( typeof( ProvisionService ) );
		private static readonly string userProvisionClass = "Simias.Identity.UserProvisionProvider";
		private static readonly string userProvisionmethod = "ProvisionUser";
		
		/// <summary>
		/// Register a provisioning provider
		/// </summary>
		public static void RegisterProvider( IProvisionUserProvider provider )
		{
			if(Simias.Service.Manager.LdapServiceEnabled == false)
			{
				logger.Debug( "Registering {0}", provider.GetType().ToString() );
				callout = new hostProvisionMethod( provider.ProvisionUser );	
			}
			else
			{
				logger.Debug( "Registering The new provider ");
                        	string assemblyName = Simias.Service.Manager.LdapAssemblyName;

                        	if ( assemblyName != null )
                        	{
                                	logger.Debug( "Provision assembly: {0}  class: {1}", assemblyName,
						"Simias.Identity.UserProvisionProvider" );
                                	Assembly idAssembly = Assembly.Load( assemblyName );
                                	if ( idAssembly != null )
                                	{
                                        	Type type = idAssembly.GetType( userProvisionClass );
                                        	if ( type != null )
                                        	{

						        MethodInfo provisionUser = type.GetMethod( userProvisionmethod,
									BindingFlags.Public | BindingFlags.Static );
							Delegate newDel = Delegate.CreateDelegate(typeof(hostProvisionMethod)										, provisionUser);
							callout = (hostProvisionMethod)newDel;
						}
					}
				}
				if(callout == null)
					callout = new hostProvisionMethod( provider.ProvisionUser );

			}
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
				string HostId = null;
				Simias.Host.HostInfo hinfo = null;
				logger.Debug( "Provsioning user {0}", Username );
				if(Simias.Service.Manager.LdapServiceEnabled == false)
					HostId = callout( Username );
				else
				{	
					HostId = callout( Username );
					if(HostId == null)
					{
                                		logger.Debug( "AttributeProvisionUserProvider Failed to provision: {0} {1}", 
						Username,"Calling LoadBalanceProvisionUserProvider for provisioning" );
						LoadBalanceProvisionUserProvider userProvider = 
								new LoadBalanceProvisionUserProvider();
						HostId = userProvider.ProvisionUser(Username);	
					}
				}
				Store store = Store.GetStore();
                        	Domain domain = store.GetDomain( store.DefaultDomain );
				HostNode hNode = HostNode.GetHostByID(domain.ID, HostId);
				hinfo = new Simias.Host.HostInfo( hNode );	
				return hinfo;
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
		string ProvisionUser( string userName );
	}

	public class HostEntry : IComparable
	{
		HostNode	host;
		int			userCount;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="hNode">HostNode object</param>
		internal HostEntry( HostNode hNode )
		{
			host = hNode;
			ICSList members = hNode.GetHostedMembers();
			userCount = members.Count;
		}

        /// <summary>
        /// get Info
        /// </summary>
		internal Simias.Host.HostInfo Info
		{
			get { return new Simias.Host.HostInfo( host ); }
		}

        /// <summary>
        /// get Host
        /// </summary>
		internal HostNode Host
		{
			get { return host; }
		}

        /// <summary>
        /// Add the member
        /// </summary>
        /// <param name="domain">domain object</param>
        /// <param name="member">member object</param>
		internal void AddMember( Domain domain, Member member )
		{
			member.HomeServer = host;
			System.Threading.Interlocked.Increment( ref userCount );
		}

		#region IComparable Members
        /// <summary>
        /// Icomparable object to compare 2 objects
        /// </summary>
        /// <param name="obj">obj to compare</param>
        /// <returns>matching result</returns>
		public int CompareTo( object obj )
		{
			HostEntry he = obj as HostEntry;
			return userCount.CompareTo( he.userCount );
		}
		#endregion
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
        /// Provision the user
		/// </summary>
        /// <returns>new Hostnode if successful, otherwise null</returns>
		public string ProvisionUser( string Username )
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

			return hostnode == null ? null : hostnode.UserID;
		}
		#endregion
	}


	/// <summary>
	/// Master host is a user provisioning provider that always
	/// provisions users to the master host.
	/// 
	/// Note: this provider is used for testing purposes or if
	/// a server is deployed in a single server only installation.
	/// </summary>
	public class MasterHostProvisionProvider : IProvisionUserProvider
	{
		ArrayList hosts = new ArrayList();
		Domain domain;
		HostNode masterHost = null;

		/// <summary>
		/// Constructor for the master host provider.
		/// 
		/// Find the master host and store an instance of
		/// the host object.
		/// </summary>
		public MasterHostProvisionProvider()
		{
			Store store = Store.GetStore();
			domain = store.GetDomain( store.DefaultDomain );
			HostNode[] hArray = HostNode.GetHosts( domain.ID );
			foreach ( HostNode host in hArray )
			{
				if ( host.IsMasterHost == true )
				{
					masterHost = host;
					break;
				}
			}
		}
		#region IProvisionUserProvider Members
		/// <summary>
		/// </summary>
		public string ProvisionUser( string Username )
		{
			HostNode hostnode = null;
			Member member = domain.GetMemberByName( Username );
			if ( member != null )
			{
				// Note! member.HomeServer is an expensive property for get/set
				hostnode = member.HomeServer;
				if ( hostnode == null )
				{
					hostnode = masterHost;
					member.HomeServer = masterHost;
				}
			}

			return hostnode == null ? null : hostnode.UserID;
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
		public string ProvisionUser( string userName )
		{
			// TODO:  Add AttributeProvisionUserProvider.ProvisionUser implementation
			return null;
		}
		#endregion
	}
			
	/// <summary>
	/// Manually assign the given user to a server passed as argument
	/// </summary>
	public static class ManualProvisionUserProvider 
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( ManualProvisionUserProvider ) );
        /// <summary>
        /// Check whether user is provisioned or not based on userID
        /// </summary>
        /// <param name="userID">userid to be checked</param>
        /// <returns>HostNode if provisioned, otherwise null</returns>
		public static HostNode checkUserProvisionedByID(string userID)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.DefaultDomain );
			HostNode hostnode = null;
			Member member = domain.GetMemberByID( userID );
			if ( member != null )
				hostnode = member.HomeServer;
			return hostnode;
		}

		#region IProvisionUserProvider Members
		/// <summary>
        /// Provision the user based on userID and HostName
		/// </summary>
        /// <returns>New hostInfo if successful otherwise null</returns>
		public static Simias.Host.HostInfo ProvisionUser( string userID, string hostName )
		{
			log.Debug("ProvisionUser: Request for  {0} to {1}",userID, hostName );
			HostNode hostnode = null;
			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.DefaultDomain );
			Member member = domain.GetMemberByID ( userID );
			log.Debug(String.Format("ProvisionUser {0}",userID ));
			hostnode = checkUserProvisionedByID(userID);
			if ( hostnode == null )
			{
				hostnode = HostNode.GetHostByName(domain.ID,hostName);
				if(hostnode == null )
					return null;
				log.Debug(String.Format("Setting the home server for {0} to {1}",userID, hostName ));
				HostEntry hentry = new HostEntry( hostnode );
				hostnode = hentry.Host;
				hentry.AddMember( domain, member );
			}
			else
			{
				if(Simias.Service.Manager.UserMoveServiceEnabled == false)
				{
					log.Debug(String.Format("User move service is not configured, User movement skipped for  {0} to {1}",userID, hostName ));
					throw new Exception(String.Format("User move service is not configured, User movement skipped for  {0} to {1}",userID, hostName ));
				}
                                hostnode = HostNode.GetHostByName(domain.ID,hostName);
                                if(hostnode == null )
                                        return null;
				return Simias.Server.UserMove.Reprovision(userID, HostNode.GetMaster(domain.ID), member.HomeServer, hostnode);	
			}
			return new Simias.Host.HostInfo( hostnode );
		}

		#endregion
	}

}
