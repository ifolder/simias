/***********************************************************************
 *  $RCSfile: HostProvider.cs,v $
 *
 *  Copyright (C) 2005 Novell, Inc.
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
using System.IO;
using System.Net;
using System.Xml;
using System.Security.Cryptography;
using System.Threading;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Authentication;
using Simias.Client.Event;
using Simias.Server;
//using Simias.Provision;

namespace Simias.Host
{
	/// <summary>
	/// </summary>
	public class SlaveSetup
	{
		private static string tempHostFileName = ".host.xml";
		private static string tempPPKFileName = ".hostppk.xml";
		private static string tempDomainFileName = ".domain.xml";
		private static string tempOwnerFileName = ".owner.xml";

		private static void SaveXmlDoc(string storePath, string fileName, string sObject)
		{
			string fullPath = Path.Combine(storePath, fileName);
			StreamWriter stream = new StreamWriter(File.Open(fullPath, FileMode.Create, FileAccess.Write, FileShare.None));
			stream.Write(sObject);
			stream.Close();
		}

		private static string GetXmlDoc(string storePath, string fileName)
		{
			string fullPath = Path.Combine(storePath, fileName);
			StreamReader stream = new StreamReader(File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.None));
			string objectXml = stream.ReadToEnd();
			stream.Close();
			return objectXml;
		}
	
		internal static Domain GetDomain(string storePath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(GetXmlDoc(storePath, tempDomainFileName));
			Domain domain = (Domain)Node.NodeFactory(Store.GetStore(), doc);
			domain.Proxy = true;
			return domain;
		}

		private static void SaveDomain(string storePath, string domain)
		{
			SaveXmlDoc(storePath, tempDomainFileName, domain);
		}

		internal static Member GetOwner(string storePath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(GetXmlDoc(storePath, tempOwnerFileName));
			Member owner = (Member)Node.NodeFactory(Store.GetStore(), doc);
			owner.Proxy = true;
			return owner;
		}

		private static void SaveOwner(string storePath, string owner)
		{
			SaveXmlDoc(storePath, tempOwnerFileName, owner);
		}

		internal static HostNode GetHost(string storePath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(GetXmlDoc(storePath, tempHostFileName));
			HostNode hnode = new HostNode(Node.NodeFactory(Store.GetStore(), doc));
			return hnode;
		}

		private static void SaveHost(string storePath, string host)
		{
			SaveXmlDoc(storePath, tempHostFileName, host);
		}

		internal static RSACryptoServiceProvider GetKeys(string storePath)
		{
			RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
			string keys = GetXmlDoc(storePath, tempPPKFileName);
			rsa.FromXmlString(keys);
			return rsa;
		}

		/// <summary>
		/// </summary>
		public static RSACryptoServiceProvider CreateKeys(string storePath)
		{
			try
			{
				RSACryptoServiceProvider rsa = GetKeys(storePath);
				return rsa;
			}
			catch
			{
				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
				return rsa;
			}
		}

		/// <summary>
		/// </summary>
		public static void SaveInitObjects(string storePath, string domain, string owner, string host, RSACryptoServiceProvider rsa)
		{
			SaveDomain(storePath, domain);
			SaveOwner(storePath, owner);
			SaveHost(storePath, host);
			SaveXmlDoc(storePath, tempPPKFileName, rsa.ToXmlString(true));
		}
				
		internal static void DeleteTempSetupFiles(string storePath)
		{
			File.Delete(Path.Combine(storePath, tempDomainFileName));
			File.Delete(Path.Combine(storePath, tempDomainFileName));
			File.Delete(Path.Combine(storePath, tempHostFileName));
			File.Delete(Path.Combine(storePath, tempPPKFileName));
		}
	}

	/// <summary>
	/// Summary description for HostDomainProvider.
	/// </summary>
	public class HostProvider
	{
		private Domain hostDomain;
		private HostNode host;
		private Store store = Store.GetStore();
		
		private static string ServerSection = "Server";
		private static string ServerNameKey = "Name";
		private static string PublicAddressKey = "PublicAddress";
		private static string PrivateAddressKey = "PrivateAddress";
		private static string MasterAddressKey = "MasterAddress";
		/// <summary>
		/// The ID of the Host domain. used for Host to Host authentication.
		/// </summary>
		private CollectionSyncClient syncClient;
		private AutoResetEvent syncEvent = new AutoResetEvent(false);
		private SimiasConnection connection;
		
		
		/// <summary>
		/// Construct a Host domain.
		/// </summary>
		/// <param name="domain">The enterprise domain.</param>
		public HostProvider( Domain domain )
		{
			hostDomain = domain;

			// Check if this is the master server.
			bool master = ( hostDomain.Role == SyncRoles.Master ) ? true : false;
			
			// Get the HostDomain
			// If the HostNode does not exist create it.
			lock( typeof( HostProvider ) )
			{
				// Check if the host node exists.
				string hName = Store.Config.Get( ServerSection, ServerNameKey );

				// Make sure a master host can run without any pre-configured settings
				// so if the public address wasn't configured get a non-loopback local
				// address and configure the public address with it.
				string publicAddress = Store.Config.Get( ServerSection, PublicAddressKey );
				if ( publicAddress == null || publicAddress == String.Empty )
				{
					// Get the first non-localhost address
					string[] addresses = MyDns.GetHostAddresses();
					foreach( string addr in addresses )
					{
						if ( IPAddress.IsLoopback( IPAddress.Parse( addr ) ) == false )
						{
							publicAddress = addr;
							break;
						}
					}
				}

				string privateAddress = Store.Config.Get( ServerSection, PrivateAddressKey );
				if ( privateAddress == null || privateAddress == String.Empty )
				{
					if ( publicAddress != null )
					{
						privateAddress = publicAddress;
					}
				}

				string masterAddress = Store.Config.Get( ServerSection, MasterAddressKey );
				Member mNode = hostDomain.GetMemberByName( hName );
				host = ( mNode == null ) ? null : new HostNode( mNode );
				if ( host == null )
				{
					RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
					if( master == true )
					{
						host = new HostNode( hName, System.Guid.NewGuid().ToString(), publicAddress, privateAddress, rsa );
						host.IsMasterHost = true;
					}
					else
					{
						host = SlaveSetup.GetHost( Store.StorePath );
						host.Proxy = true;
						rsa = SlaveSetup.GetKeys( Store.StorePath );
						// TODO remove
						Property p = new Property( PropertyTags.HostAddress, new Uri(masterAddress));
						p.LocalProperty = true;
						hostDomain.Properties.AddNodeProperty( p );
						// END TODO
					}

					host.IsLocalHost = true;
					hostDomain.Commit(new Node[] {hostDomain, host});
					
					// Now Associate this host with the local identity.
					store.AddDomainIdentity( hostDomain.ID, host.UserID, rsa.ToXmlString(true), CredentialType.PPK );
					SlaveSetup.DeleteTempSetupFiles( Store.StorePath );
				}
				else
				{
					// Make sure the address has not changed.
					bool hostChanged = false;
					if (host.PublicUrl != publicAddress)
					{
						host.PublicUrl = publicAddress;
						hostChanged = true;
					}
					if (host.PrivateUrl != privateAddress)
					{
						host.PrivateUrl = privateAddress;
						hostChanged = true;
					}

					if ( hostChanged == true )
					{
						hostDomain.Commit(host);
					}
				}
			}

			if ( master == true )
			{
				// Register the ProvisionUser Provider.
				//ProvisionService.RegisterProvider( new LoadBalanceProvisionUserProvider() );
				ProvisionService.RegisterProvider( new MasterHostProvisionProvider() );
			}
			else
			{
				// Now start the sync process for the domain.
				Thread syncThread = new Thread(new ThreadStart(SyncDomain));
				syncThread.IsBackground = true;
				syncThread.Start();
			}
		}

		private void SyncDomain()
		{
			// Get a connection object to the server.
			connection = new SimiasConnection(hostDomain.ID, host.UserID, SimiasConnection.AuthType.PPK, hostDomain );
			// We need to get a one time password to use to authenticate.
			connection.Authenticate();
			
			syncClient = new CollectionSyncClient(hostDomain.ID, new TimerCallback(TimerFired));
			while (true)
			{
				syncEvent.WaitOne();
				try
				{
					syncClient.SyncNow();
				}
				catch {}
				syncClient.Reschedule(true, 30);
			}
		}

		/// <summary>
		/// Called by The CollectionSyncClient when it is time to run another sync pass.
		/// </summary>
		/// <param name="collectionClient">The client that is ready to sync.</param>
		public void TimerFired(object collectionClient)
		{
			syncEvent.Set();
		}
	}
}
