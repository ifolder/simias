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
 *  Author: Russ Young <ryoung@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Xml;
using System.Security.Cryptography;

using Simias.Client;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents a Simias Host.
	/// </summary>
	[ Serializable ]
	public class HostNode : Member
	{
		#region Class Members
		public const string HostNodeType = "Host";
		const string LocalHostTag = "LocalHost";
		#endregion

		#region Properties
		/// <summary>
		/// Gets/Sets the public address for this host.
		/// </summary>
		public string PublicUrl
		{
			get
			{
				Property pa = Properties.GetSingleProperty(PropertyTags.PublicUrl);
				if (pa != null)
				{
					return pa.Value.ToString();
				}
				throw new NotExistException(PropertyTags.PublicUrl);
			}
			set
			{
				Properties.ModifyNodeProperty(new Property(PropertyTags.PublicUrl, value));
			}
		}

		/// <summary>
		/// Gets/Sets the private address for this host.
		/// </summary>
		public string PrivateUrl
		{
			get
			{
				Property pa = Properties.GetSingleProperty(PropertyTags.PrivateUrl);
				if (pa != null)
				{
					return pa.Value.ToString();
				}
				throw new NotExistException(PropertyTags.PrivateUrl);
			}
			set
			{
				Properties.ModifyNodeProperty(new Property(PropertyTags.PrivateUrl, value));
			}
		}

		/// <summary>
		/// Gets/Sets if HostNode is the Master Host.
		/// </summary>
		public bool IsMasterHost
		{
			get
			{
				Property pa = Properties.GetSingleProperty(PropertyTags.MasterHost);
				if (pa != null)
				{
					return (bool)pa.Value;
				}
				return false;
			}
			set
			{
				Properties.ModifyNodeProperty(new Property(PropertyTags.MasterHost, value));
			}
		}

		/// <summary>
		/// Gets/Sets If HostNode is the local host.
		/// </summary>
		public bool IsLocalHost
		{
			get
			{
				Property pa = Properties.GetSingleProperty(LocalHostTag);
				if (pa != null)
				{
					return (bool)pa.Value;
				}
				return false;
			}
			set
			{
				Property localHost = new Property(LocalHostTag, true);
				localHost.LocalProperty = true;
				Properties.AddNodeProperty(localHost);
			}
		}

		#endregion

		#region Consturctors
		/// <summary>
		/// Construct a new host node.
		/// </summary>
		/// <param name="name">The name of the host.</param>
		/// <param name="userId">Unique identifier for the user.</param>
		/// <param name="publicUrl">The public url for the host.</param>
		/// <param name="privateUrl">The private url for the host.</param>
		public HostNode(string name, string userId, string publicUrl, string privateUrl) :
			this(name, userId, publicUrl, privateUrl, null)
		{
		}

		/// <summary>
		/// Construct a new host node.
		/// </summary>
		/// <param name="name">The name of the host.</param>
		/// <param name="userId">Unique identifier for the user.</param>
		/// <param name="publicUrl">The public URL for the host.</param>
		/// <param name="privateUrl">The private URL for the host.</param>
		/// <param name="publicKey"></param>
		public HostNode(string name, string userId, string publicUrl, string privateUrl, RSACryptoServiceProvider publicKey) :
			base(name, userId, Access.Rights.Admin, publicKey)
		{
			// Set the Addresses.
			// Get the port that we are using.
			PublicUrl = publicUrl;
			PrivateUrl = privateUrl;
			Properties.AddNodeProperty(new Property(PropertyTags.Types, HostNodeType));
		}

		/// <summary>
		/// Consturct a new host node.
		/// </summary>
		/// <param name="name">The name of the host.</param>
		/// <param name="userId">Unique identifier for the user.</param>
		/// <param name="publicAddress">The public address for the host.</param>
		public HostNode(string name, string userId, string publicAddress) :
			this(name, userId, publicAddress, publicAddress)
		{
		}

		/// <summary>
		/// Construct a host node from a node.
		/// </summary>
		/// <param name="node">The host node.</param>
		public HostNode(Node node) :
			base(node)
		{
			if (!IsType(HostNodeType))
			{
				throw new CollectionStoreException(String.Format("Cannot construct an object type of {0}.", HostNodeType));
			}
		}

		/// <summary>
		/// Construct a host node from a shallow node.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="shallowNode">The shallow node that represents the HostNode.</param>
		public HostNode(Collection collection, ShallowNode shallowNode)
			:
			base(collection, shallowNode)
		{
			if (!IsType(HostNodeType))
			{
				throw new CollectionStoreException(String.Format("Cannot construct an object type of {0}.", HostNodeType));
			}
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Create a HostNode from the xml document.
		/// </summary>
		/// <param name="store">The store.</param>
		/// <param name="document">The xml document.</param>
		/// <returns></returns>
		public static HostNode FromXml(Store store, XmlDocument document)
		{
			return new HostNode(Node.NodeFactory(store, document));
		}

		public static HostNode GetHostByID(string domainId, string hostId)
		{
			Domain domain = Store.GetStore().GetDomain(domainId);
			return new HostNode(domain.GetMemberByID(hostId));
		}

		public static HostNode GetHostByName(string domainId, string hostName)
		{
			Domain domain = Store.GetStore().GetDomain(domainId);
			return new HostNode(domain.GetMemberByName(hostName));
		}

		public static HostNode[] GetHosts(string domainId)
		{
			Domain domain = Store.GetStore().GetDomain(domainId);
			ICSList snHosts = domain.GetNodesByType(HostNodeType);
			ArrayList hosts = new ArrayList();
			foreach (ShallowNode sn in snHosts)
			{
				HostNode hn = new HostNode(domain.GetNodeByID(sn.ID));
				hosts.Add(hn);
			}
			return (HostNode[])hosts.ToArray(typeof(HostNode));
		}

		public static HostNode GetMaster(string domainId)
		{
			Domain domain = Store.GetStore().GetDomain(domainId);
			ICSList searchList = domain.Search(PropertyTags.MasterHost, Syntax.Boolean, SearchOp.Exists);
			if (searchList.Count == 1)
			{
				IEnumerator list = searchList.GetEnumerator();
				list.MoveNext();
				ShallowNode sn = (ShallowNode)list.Current;
				return new HostNode(domain.GetNodeByID(sn.ID));
			}
			return null;
		}

		#endregion

		public ICSList GetHostedMembers()
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(GetDomainID(store));
			return domain.Search(PropertyTags.HostID, this.UserID, SearchOp.Equal);
		}

		public static HostNode GetLocalHost()
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			ICSList searchList = domain.Search(LocalHostTag, Syntax.Boolean, SearchOp.Exists);
			if (searchList.Count == 1)
			{
				IEnumerator list = searchList.GetEnumerator();
				list.MoveNext();
				ShallowNode sn = (ShallowNode)list.Current;
				return new HostNode(domain.GetNodeByID(sn.ID));
			}
			return null;
		}
	}
}