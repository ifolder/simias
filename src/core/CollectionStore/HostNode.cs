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
*                 $Author: Russ Young <ryoung@novell.com>

*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

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
		/// <summary>
        	/// 
	        /// </summary>
		public const string HostNodeType = "Host";
		const string LocalHostTag = "LocalHost";
		#endregion

		/// <summary>
        	/// 
	        /// </summary>
		public enum changeMasterStates
		{
			/// <summary>
			/// Changemaster process started
			/// </summary>
			Started,

			/// <summary>
			/// Changemaster all updates complete 
			/// </summary>
			Complete,

			/// <summary>
			/// Changemaster verified after restart 
			/// </summary>
			Verified
		};

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
		/// Gets/Sets the Master server address for this host.
		/// </summary>
		public string MasterUrl
		{
			get
			{
				Property pa = Properties.GetSingleProperty(PropertyTags.MasterUrl);
				if (pa != null)
				{
					return pa.Value.ToString();
				}
				throw new NotExistException(PropertyTags.MasterUrl);
			}
			set
			{
				Properties.ModifyNodeProperty(new Property(PropertyTags.MasterUrl, value));
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
				if ( value )
				{
					Properties.ModifyNodeProperty(new Property(PropertyTags.MasterHost, value));
				}
				else
				{
					properties.DeleteSingleNodeProperty( PropertyTags.MasterHost);
				}
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


		/// <summary>
		/// Gets/Sets ChangeMasterState  
		/// </summary>
		public int ChangeMasterState 
		{
			get
			{
				Property pa = Properties.GetSingleProperty(PropertyTags.ChangeMasterState);
				int value= (pa!=null) ? (int) pa.Value:(int)-1;
				return value;
			}
			set
			{
				if ( value != -1)
				{
					Properties.ModifyNodeProperty(new Property(PropertyTags.ChangeMasterState, value));
				}
				else
				{
					properties.DeleteSingleNodeProperty( PropertyTags.ChangeMasterState);
				}
			}
		}



		#endregion

		#region Constructors
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
			base(name, userId, Access.Rights.ReadOnly, publicKey)
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
        /// 
        /// </summary>
        /// <param name="hInfo"></param>
        /// <param name="publicAddress"></param>
        public HostNode(HostInfo hInfo, string publicAddress) :
            this(hInfo.Name, hInfo.ID, hInfo.MemberID, publicAddress, publicAddress, null)
        {
        }
	/// <summary>
	///
	/// </summary>
	/// <param name="name"></param>
        /// <param name="nodeID"></param>
        /// <param name="userId"></param>
        /// <param name="publicUrl"></param>
        /// <param name="privateUrl"></param>
        /// <param name="publicKey"></param>	
        public HostNode(string name, string nodeID, string userId, string publicUrl, string privateUrl, RSACryptoServiceProvider publicKey):
            base(name, nodeID, userId, Access.Rights.ReadOnly, publicKey)
        {
            // Set the Addresses.
            // Get the port that we are using.
            PublicUrl = publicUrl;
            PrivateUrl = privateUrl;
            Properties.AddNodeProperty(new Property(PropertyTags.Types, HostNodeType));
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

        /// <summary>
        /// Get HostNode from host ID
        /// </summary>
        /// <param name="domainId">Domain ID where host is available</param>
        /// <param name="hostId">ID of the host whose name we have to get</param>
        /// <returns>Retuns the Host node</returns>
		public static HostNode GetHostByID(string domainId, string hostId)
		{
			Domain domain = Store.GetStore().GetDomain(domainId);
			return new HostNode(domain.GetMemberByID(hostId));
		}

        /// <summary>
        /// Get the host from name
        /// </summary>
        /// <param name="domainId">Domain ID where host is available</param>
        /// <param name="hostName">Name of the host</param>
        /// <returns>Returns the HostNode</returns>
		public static HostNode GetHostByName(string domainId, string hostName)
		{
			Domain domain = Store.GetStore().GetDomain(domainId);
			return new HostNode(domain.GetMemberByName(hostName));
		}

        /// <summary>
        /// Gets the hosts available in a domain
        /// </summary>
        /// <param name="domainId">Domain ID</param>
        /// <returns>Array of HostNode</returns>
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

        /// <summary>
        /// Gets the master in a domain
        /// </summary>
        /// <param name="domainId">Domain ID for which master details needed</param>
        /// <returns>Returns the HostNode of master</returns>
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

        /// <summary>
        /// Gets the list of hosted members of a domain
        /// </summary>
        /// <returns>ICSList with host members</returns>
		public ICSList GetHostedMembers()
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(GetDomainID(store));
			return domain.Search(PropertyTags.HostID, this.UserID, SearchOp.Equal);
		}

        /// <summary>
        /// Gets the local host
        /// </summary>
        /// <returns>Hostnode with local host details</returns>
		public static HostNode GetLocalHost()
		{
			Store store = Store.GetStore();
			Domain domain = null;
			// this might given an exception if the Default Domain is not set up.
			// this is true when the entire iFolder store is restored - so the try/catch
			// Need to find a better way to fix this without try/catch - FIXME
			try
			{
				domain = store.GetDomain(store.DefaultDomain);
				ICSList searchList = domain.Search(LocalHostTag, Syntax.Boolean, SearchOp.Exists);
				if (searchList.Count == 1)
				{
					IEnumerator list = searchList.GetEnumerator();
					list.MoveNext();
					ShallowNode sn = (ShallowNode)list.Current;
					return new HostNode(domain.GetNodeByID(sn.ID));
				}
				else if (!Store.IsEnterpriseServer)
				{
					return domain.Host;
				}
				return null;
			}
			catch
			{
				return null;
			}
		}
	}
}
