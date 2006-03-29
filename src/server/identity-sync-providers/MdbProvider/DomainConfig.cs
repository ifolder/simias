/***********************************************************************
 *  $RCSfile: DomainConfig.cs,v $
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Simias.MdbProvider
{
	/// <summary>
	/// DomainConfiguration - Contains required information to perform
	/// an MDB -> Simias.Domain one way synchronization
	/// 
	/// Usage - First call the static method GetDomains to retrieve
	/// the domains configured for the one way sync.
	///
	/// Note - For now this all has hard coded values which will 
	/// work against the MDB file driver straight away
	/// 
	/// </summary>
    public class DomainConfiguration
    {
		#region Class Members
		private string id = null;
		private string description = "This domain is synchronized from Hula";
		private string admin = "\\Tree\\Context\\admin";
		private string name;
		private string proxyUsername = "\\Tree\\Context\\admin";
		private string proxyPassword = "hula";
		//private ArrayList containers;
		private string[] containers = { "\\Tree\\Context" };
		private int	syncInterval = 1000 * 30;
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets a string array of configured containers
		/// to sync to the Simias domain		/// <summary>
		/// Gets the domainID of the configured domain
		/// Note: may return NULL if the MDB driver doesn't
		/// export an ID for the domain.
		/// </summary>
		public string DomainID
		{
			get{ return id; }
		}

		/// </summary>
		public string[] Containers
		{
			get{ return containers; }
		}
		
		/// <summary>
		/// Gets the description of the configured domain
		/// Note: may return NULL since a description is
		/// not required.
		/// </summary>
		public string Description
		{
			get{ return description; }
		}

		
		/// <summary>
		/// Gets the friendly name of the domain
		/// </summary>
		public string DomainName
		{
			get{ return name; }
		}
		
		/// <summary>
		/// Gets the full DN of the admin for this domain
		/// </summary>
		public string DomainAdmin
		{
			get{ return admin; }
		}
		
		/// <summary>
		/// Gets the proxy user which should be used to
		/// authenticate to MDB when syncing this domain
		/// </summary>
		public string ProxyUsername
		{
			get{ return proxyUsername; }
		}
				
		/// <summary>
		/// Gets the proxy password which should be used to
		/// authenticate to MDB when syncing this domain
		/// </summary>
		public string ProxyPassword
		{
			get{ return proxyPassword; }
		}
		
		/// <summary>
		/// Gets the synchronization interval for this domain
		/// expressed in milliseconds
		/// </summary>
		public int SyncInterval
		{
			get{ return syncInterval; }
		}
		
		/// <summary>
		/// True immediately synchronize the domain when 
		/// the service starts.  False wait for one 
		/// synchronization interval before syncing.
		/// </summary>
		public bool SyncOnStart
		{
			get{ return true; }
		}
		#endregion
		
		#region Constructors
		public DomainConfiguration( string Name )
		{
			name = Name;
		}
		#endregion
		
		#region Public Methods
		/// <summary>
		/// Return a string array of configured domains
		/// to synchronize to Simias
		/// </summary>
    	static public string[] GetDomains()
    	{
    		string[] domains = { "Hula" };
    		return domains;
    	}
		#endregion
    }
}
