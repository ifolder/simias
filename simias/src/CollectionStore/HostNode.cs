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
		const string HostNodeType = "Host";
		#endregion

		#region Properties
		/// <summary>
		/// Gets/Sets the public address for this host.
		/// </summary>
		public string PublicAddress
		{
			get
			{
				Property pa = Properties.GetSingleProperty(PropertyTags.PublicAddress);
				if (pa != null)
				{
					return pa.Value.ToString();
				}
				throw new NotExistException(PropertyTags.PublicAddress);
			}
			set
			{
				Properties.ModifyNodeProperty(new Property(PropertyTags.PublicAddress, value));
			}
		}

		/// <summary>
		/// Gets/Sets the private address for this host.
		/// </summary>
		public string PrivateAddress
		{
			get
			{
				Property pa = Properties.GetSingleProperty(PropertyTags.PrivateAddress);
				if (pa != null)
				{
					return pa.Value.ToString();
				}
				throw new NotExistException(PropertyTags.PrivateAddress);
			}
			set
			{
				Properties.ModifyNodeProperty(new Property(PropertyTags.PrivateAddress, value));
			}
		}
		#endregion

		#region Consturctors
		/// <summary>
		/// Construct a new host node.
		/// </summary>
		/// <param name="name">The name of the host.</param>
		/// <param name="userId">Unique identifier for the user.</param>
		/// <param name="publicAddress">The public address for the host.</param>
		/// <param name="privateAddress">The private address for the host.</param>
		public HostNode(string name, string userId, string publicAddress, string privateAddress) :
			this(name, userId, publicAddress, privateAddress, null)
		{
		}

		/// <summary>
		/// Construct a new host node.
		/// </summary>
		/// <param name="name">The name of the host.</param>
		/// <param name="userId">Unique identifier for the user.</param>
		/// <param name="publicAddress">The public address for the host.</param>
		/// <param name="privateAddress">The private address for the host.</param>
		/// <param name="publicKey"></param>
		public HostNode(string name, string userId, string publicAddress, string privateAddress, RSACryptoServiceProvider publicKey) :
			base(name, userId, Access.Rights.Admin, publicKey)
		{
			// Set the Addresses.
			//Properties.ModifyProperty(new Property(PropertyTags.Types, 
			PublicAddress = publicAddress;
			PrivateAddress = privateAddress;
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
			if (IsType(HostNodeType))
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
			if (IsType(HostNodeType))
			{
				throw new CollectionStoreException(String.Format("Cannot construct an object type of {0}.", HostNodeType));
			}
		}

		/// <summary>
		/// Construct a HostNode from the serialized XML.
		/// </summary>
		/// <param name="document">The XML represention of the HostNode.</param>
		internal HostNode(XmlDocument document)
			:
			base(document)
		{
			if (IsType(HostNodeType))
			{
				throw new CollectionStoreException(String.Format("Cannot construct an object type of {0}.", HostNodeType));
			}
		}
		#endregion
	}
}