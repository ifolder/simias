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
using System.Threading;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;

//using Novell.AddressBook;

namespace Simias.Server
{
	/// <summary>
	/// Class to initialize/verify a SimpleServer domain in the Collection Store
	/// </summary>
	public class Domain
	{
		#region Class Members
		
		private Store store;

		/// <summary>
		/// GUID for this SimpleServer domain
		/// </summary>
		private string id = "";

		/// <summary>
		/// Default friendly name for the Enterprise domain
		/// </summary>
		private string domainName = "Simias";
		private string hostAddress;
		private string description = "Simias enterprise domain";
		private string adminName = "admin";
		private string adminPassword = "simias";

		private readonly string DomainSection = "Domain";


		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#endregion

		#region Properties

		/// <summary>
		/// Gets the server's unique ID
		/// </summary>
		public string ID
		{
			get { return( this.id ); }
		}

		/// <summary>
		/// Gets the server's friendly ID
		/// </summary>
		public string Name
		{
			get { return( this.domainName ); }
		}

		/// <summary>
		/// Gets the server's description
		/// </summary>
		public string Description
		{
			get { return( this.description ); }
		}

		/// <summary>
		/// Gets the server's host address
		/// </summary>
		public string Host
		{
			get { return( this.hostAddress ); }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for creating a new Simias Server Domain object.
		/// </summary>
		internal Domain( bool Create )
		{
			this.store = Store.GetStore();
			if ( this.GetSimiasServerDomain( Create ) == null )
			{
				throw new SimiasException( "Enterprise domain does not exist or could not be created" );
			}
		}

		/// <summary>
		/// Constructor for creating a new Simias Server Domain object.
		/// </summary>
		/// <param name="init"></param>
		/// <param name="description">String that describes this domain.</param>
		internal Domain( bool Create, string Description ) 
		{
			this.store = Store.GetStore();
			this.description = Description;
			this.GetSimiasServerDomain( Create );
		}
		#endregion

		/// <summary>
		/// Method to get the Simias Server domain
		/// If the domain does not exist and the create flag is true
		/// the domain will be created.  If create == false, ownerName is ignored
		/// </summary>
		internal Simias.Storage.Domain GetSimiasServerDomain( bool Create )
		{
			Simias.Storage.Domain ssDomain = null;

			try
			{
				foreach( ShallowNode sNode in store.GetDomainList() )
				{
					Simias.Storage.Domain tmpDomain = store.GetDomain( sNode.ID );
					Node node = tmpDomain as Node;
					if ( node.IsType( "Enterprise" ) == true) 
					{
						ssDomain = tmpDomain;
						this.id = tmpDomain.ID;
						break;
					}
				}

				if ( ssDomain == null && Create == true )
				{
					// Get the domain name and description from the config file
					Configuration config = Store.Config;
					if ( config != null )
					{
						string name = config.Get( DomainSection, "EnterpriseName" );
						if ( name != null && name != "" )
						{
							this.domainName = name;
						}

						string description = config.Get( DomainSection, "EnterpriseDescription" );
						if ( description != null && description != "" )
						{
							this.description = description;
						}

						string admin = config.Get( DomainSection, "ServerAdmin" );
						if ( admin != null && admin != "" )
						{
							this.adminName = admin;
						}

						string adminPwd = config.Get( DomainSection, "ServerAdminPassword" );
						if ( adminPwd != null && adminPwd != "" )
						{
							this.adminPassword = adminPwd;
						}
					}

					this.id = Guid.NewGuid().ToString();
					ssDomain = 
						new Simias.Storage.Domain(
							store, 
							this.domainName, 
							this.id,
							this.description, 
							Simias.Sync.SyncRoles.Master, 
                            Simias.Storage.Domain.ConfigurationType.ClientServer );

					// The "Enterprise" type must be set on domain in order for the
					// enterprise domain location provider to resolve it
					ssDomain.SetType( ssDomain, "Enterprise" );
					store.DefaultDomain = ssDomain.ID;

					// Create the owner member for the domain.
					Member member = 
						new Member(
							this.adminName,
							Guid.NewGuid().ToString(), 
							Access.Rights.Admin );

					member.IsOwner = true;

					// Set the admin password
					string hashedPwd = SimiasCredentials.HashPassword( this.adminPassword );
					Property pwd = new Property( "SS:PWD", hashedPwd );
					pwd.LocalProperty = true;
					member.Properties.ModifyProperty( pwd );

					ssDomain.Commit( new Node[] { ssDomain, member } );

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

		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return this.domainName;
		}
		#endregion
	}
}
