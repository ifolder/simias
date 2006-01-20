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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Simias.SimpleServer
{
	/// <summary>
	/// Class to initialize/verify a SimpleServer domain in the store
	/// </summary>
	public class Domain
	{
		#region Class Members
		/// <summary>
		/// GUID for this SimpleServer domain
		/// </summary>
		private string id = "";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		private string domainName = "Simple Server";
		private string hostAddress;
		private string description = "Simple Server domain";

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string serverDocumentPath = "../../etc/SimpleServer.xml";
		private XmlDocument serverDoc;
		private Store store;
		private Simias.Storage.Domain domain = null;
		#endregion

		#region Properties

		/// <summary>
		/// Gets the SimpleServer domain's unique ID
		/// </summary>
		public string ID
		{
			get { return(this.id); }
		}

		/// <summary>
		/// Gets the SimpleServer domain's friendly ID
		/// </summary>
		public string Name
		{
			get { return(this.domainName); }
		}

		/// <summary>
		/// Gets the SimpleServer domain's description
		/// </summary>
		public string Description
		{
			get { return(this.description); }
		}

		/// <summary>
		/// Gets the SimpleServer domain's host address
		/// </summary>
		public string Host
		{
			get { return(this.hostAddress); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for creating/initializing a 
		/// simple server domain
		/// </summary>
		internal Domain( bool Create )
		{
			store = Store.GetStore();
			domain = this.GetSimpleServerDomain( Create );
			if ( domain == null )
			{
				throw new SimiasException( "Could not create or initialize Simple Server domain" );
			}
		}
		#endregion

		/// <summary>
		/// Method to get the Simias simple server domain
		/// If the the domain does not exist and the create flag is true
		/// the domain will be created.  If create == false, ownerName is ignored
		/// </summary>
		internal Simias.Storage.Domain GetSimpleServerDomain( bool Create )
		{
			//  Check if the SimpleServer domain exists in the store
			Simias.Storage.Domain ssDomain = null;
			
			try
			{
				foreach( ShallowNode sNode in store.GetDomainList() )
				{
					Simias.Storage.Domain tmpDomain = store.GetDomain( sNode.ID );
					Node node = tmpDomain as Node;
					if ( node.IsType( "SimpleServer" ) == true) 
					{
						ssDomain = tmpDomain;
						this.id = tmpDomain.ID;
						break;
					}
				}

				if ( ssDomain == null && Create == true )
				{
					// Load the configuration document from the file.
					serverDoc = new XmlDocument();
					serverDoc.Load( serverDocumentPath );

					XmlElement domainElement = serverDoc.DocumentElement;
					domainName = domainElement.GetAttribute( "Name" );
					string tmpDescription = "";
					try
					{
						tmpDescription = domainElement.GetAttribute( "Description" );
					}
					catch{}
					if ( tmpDescription != null )
					{
						description = tmpDescription;
					}

					XmlAttribute attr;
					XmlNode ownerNode = null;
					string ownerMember = null;
					for ( int i = 0; i < domainElement.ChildNodes.Count; i++ )
					{
						attr = domainElement.ChildNodes[i].Attributes["Owner"];
						if ( attr != null && attr.Value == "true" )
						{
							ownerNode = domainElement.ChildNodes[i];
							ownerMember = ownerNode.Attributes["Name"].Value;
							break;
						}
					}

					if ( ownerMember == null || ownerMember == "" )
					{
						// Take the first child node and make it the owner
						ownerNode = domainElement.ChildNodes[0];
						ownerMember = ownerNode.Attributes["Name"].Value;
					}
				
					this.id = Guid.NewGuid().ToString();

					// Create the simple server domain.
					ssDomain = 
						new Simias.Storage.Domain(
							store, 
							this.domainName, 
							this.id,
							this.description, 
							Simias.Sync.SyncRoles.Master, 
                            Simias.Storage.Domain.ConfigurationType.ClientServer );

					// This needs to be added to allow the enterprise location provider
					// to be able to resolve this domain.
					ssDomain.SetType( ssDomain, "Enterprise" );
					
					// For us!
					ssDomain.SetType( ssDomain, "SimpleServer" );

					// Create the owner member for the domain.
					Member member = 
						new Member(
							ownerMember, 
							Guid.NewGuid().ToString(), 
							Access.Rights.Admin );

					member.IsOwner = true;
					ssDomain.Commit( new Node[] { ssDomain, member } );
					
					// Set the domain default
					store.DefaultDomain = ssDomain.ID;

					// Create the name mapping.
					store.AddDomainIdentity( ssDomain.ID, member.UserID );
				}
			}
			catch( Exception gssd )
			{
				log.Error( gssd.Message );
				log.Error( gssd.StackTrace );
			}

			return ssDomain;
		}

		#region Public Methods
		#endregion
	}
}
