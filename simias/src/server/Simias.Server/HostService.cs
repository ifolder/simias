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
 |  Author: Russ Young ryoung@novell.com
 |***************************************************************************/

using System;
using System.Net;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Security.Cryptography;
using System.Threading;

using Simias;
using Simias.Storage;
using Simias.Client;
//using Simias.Enterprise;

namespace Simias.Host
{
	/// <summary>
	/// HostService - WebService class
	/// </summary>
	public class HostService : WebService
	{
		/// <summary>Instance of the store.</summary>
		static protected Store store = Store.GetStore();
		/// <summary>Property Tag used to store the ID of the Home Server.</summary>
		static protected string HomeServerTag = "HomeServer";

		protected Domain domain
		{
			get { return store.GetDomain(store.DefaultDomain); }
		}

		protected Domain hostDomain
		{
			get { return store.GetDomain(store.DefaultDomain); }
		}

		/// <summary>
		/// Gets the member object in the domain for the specified user.
		/// </summary>
		/// <param name="userID">The ID of the user.</param>
		/// <returns>The member object, or null if not found.</returns>
		protected Member GetDomainMemberByID(string userID)
		{
			try
			{
				return domain.GetMemberByID(userID);
			}
			catch { }
			return null;
		}

		protected Member GetDomainMemberByName(string userName)
		{
			try
			{
				return domain.GetMemberByName(userName);
			}
			catch { }
			return null;
		}

		protected Member GetAuthenticatedUser()
		{
			string userID = Thread.CurrentPrincipal.Identity.Name;
			if (userID != null && userID.Length != 0)
			{
				return domain.GetMemberByID(userID);
			}
			return null;
		}
	}

	/// <summary>
	/// HostLocation - WebService class
	/// </summary>
	[WebService(Namespace = "http://novell.com/simias/host/location")]
	public class HostLocation : HostService
	{
		/// <summary>
		/// 
		/// </summary>
		public HostLocation()
		{
		}

		/// <summary>
		/// Determins if the collection is on this Host.
		/// </summary>
		/// <param name="collectionID">The ID of the collection to check for.</param>
		/// <returns>True if hosted on this server, otherwise false.</returns>
		[WebMethod]
		public bool IsCollectionHost(string collectionID)
		{
			// This call requires no authentication.
			if (store.GetCollectionByID(collectionID) != null)
				return true;
			// The collection is not here return false.
			return false;
		}

		/// <summary>
		/// Returns the home server for the specified user.
		/// </summary>
		/// <param name="userName">The name of the user.</param>
		/// <returns>The ID of the machine that is the home server for this user.</returns>
		[WebMethod]
		public HostInfo GetHomeServer(string userName)
		{
			Member member = GetDomainMemberByName(userName);
			HostNode host = member.HomeServer;
			if (host != null)
			{
				// We need to provision this user.
				// This is a single server system.
				return new HostInfo(host);
			}
			return null;
		}

		/// <summary>
		///  Method to get all configured hosts
		/// </summary>
		[WebMethod]
		public HostInfo[] GetHosts()
		{
			HostInfo[] infoList;
			HostNode[] hosts = HostNode.GetHosts(hostDomain.ID);
			if (hosts.Length > 0)
			{
				infoList = new HostInfo[hosts.Length];
				int i = 0;
				foreach (HostNode hn in hosts)
				{
					infoList[i++] = new HostInfo(hn);
				}
			}
			else
			{
				infoList = new HostInfo[0];
			}

			return infoList;
		}
	}

	/// <summary>
	/// HostAdmin - 
	/// </summary>
	[WebService(Namespace = "http://novell.com/simias/host")]
	public class HostAdmin : HostService
	{
		private bool IsAdmin
		{
			get { return true; }
		}

		/// <summary>
		/// HostAdmin - default constructor 
		/// </summary>
		public HostAdmin() 
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userID"></param>
		/// <param name="server"></param>
		/// <returns></returns>
		[WebMethod(EnableSession=true)]
		public bool SetHomeServer(string userID, string serverID)
		{
			try
			{
				Member member = GetDomainMemberByID(userID);
				member.HomeServer = HostNode.GetHostByID(domain.ID, serverID);
				domain.Commit(member);
				return true;
			}
			catch { }
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="userID"></param>
		/// <returns></returns>
		[WebMethod(EnableSession = true)]
		public bool MigrateUser(string userID)
		{
			// Get the current home server for this user.
			// Lock the collection on the old server so that it will not be modified.
			// Syncronize the collection to this machine.
			// Delete the collection from the old server.
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[WebMethod(EnableSession = true)]
		public bool ProvisionUser()
		{
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="publicAddress"></param>
		/// <param name="privateAddress"></param>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		[WebMethod(EnableSession = true)]
		public string AddHost(string name, string publicAddress, string privateAddress, string publicKey, out bool created)
		{
			// Get the HostDomain
			// If the domain does not exist create it and and this host to it.
			// Add the host to the Host domain if it does not already exist.
			// Check if the host already exists.
			created = false;
			Member host = hostDomain.GetMemberByName(name);
			if (host == null)
			{
				// Now add the new host.
				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
				rsa.FromXmlString(publicKey);
				host = new HostNode(name, System.Guid.NewGuid().ToString(), publicAddress, privateAddress, rsa);
				domain.Commit(host);
				created = true;
			}
			return host.Properties.ToString(true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		[WebMethod(EnableSession=true)]
		public void DeleteHost(string id)
		{
			Domain domain = this.domain;
			HostNode host = (HostNode)domain.GetNodeByID(id);
			domain.Commit(domain.Delete(host));
		}

		/// <summary>
		/// Get the configuration from this server.
		/// </summary>
		/// <returns>XML string that represents the configuration.</returns>
		[WebMethod(EnableSession = true)]
		public string GetConfiguration()
		{
			return (Store.Config.ToXml());
		}

		/*
		/// <summary>
		/// Gets the password for the proxy user.
		/// </summary>
		/// <returns></returns>
		[WebMethod(EnableSession = true)]
		public string GetProxyInfo()
		{
			Simias.Enterprise.Common.ProxyUser pu = new Simias.Enterprise.Common.ProxyUser();
			return pu.Password;
		}
		*/

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[WebMethod(EnableSession = true)]
		public string GetDomain()
		{
			// We need to add the hostID to this node.
			Domain d = domain;
			d.Host = HostNode.GetLocalHost();
			return d.Properties.ToString(false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[WebMethod(EnableSession = true)]
		public string GetDomainOwner()
		{
			return domain.Owner.Properties.ToString(true);
		}
	}

	/// <summary>
	/// HostInfo class
	/// </summary>
	public class HostInfo
	{
		/// <summary>
		/// Host's unique ID
		/// </summary>
		public string ID;

		/// <summary>
		/// External facing address for clients
		/// </summary>
		public string PublicAddress;

		/// <summary>
		/// Internal facing address for server to 
		/// server communication.
		/// </summary>
		public string PrivateAddress;

		/// <summary>
		/// Public key for host to host authentication
		/// </summary>
		public string PublicKey;

		/// <summary>
		/// true = Master, false = Slave
		/// </summary>
		public bool Master;

		/// <summary>
		/// HostInfo default constructor
		/// </summary>
		public HostInfo()
		{
		}

		internal HostInfo(HostNode node)
		{
			ID = node.UserID;
			PublicAddress = node.PublicUrl;
			PrivateAddress = node.PrivateUrl;
			PublicKey = node.PublicKey.ToXmlString(false);
			Master = node.IsMasterHost;
		}
	}
}
