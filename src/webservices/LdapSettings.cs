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
*                 $Author: Rob
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

using Simias;
using Simias.Storage;
using Simias.Policy;
using Novell.iFolder;

namespace Novell.iFolder.Enterprise.Web
{
	/// <summary>
	/// iFolder LDAP Settings
	/// </summary>
	[Serializable]
	public class LdapSettings 
	{
		/// <summary>
		/// The LDAP Server Host
		/// </summary>
		public string LdapHost;

		/// <summary>
		/// Is LDAP Connection Secure
		/// </summary>
		public string LdapSecure;

		/// <summary>
		/// The LDAP Server Port
		/// </summary>
		public string LdapPort;

		/// <summary>
		/// The LDAP Proxy User DN
		/// </summary>
		public string ProxyDN;
		
		/// <summary>
		/// The LDAP Proxy User Password
		/// </summary>
		public string ProxyPassword;

		/// <summary>
		/// The LDAP Search Context
		/// </summary>
		public string[] SearchContexts;

		/// <summary>
		/// The LDAP Sync Interval
		/// </summary>
		public string SyncInterval;

		/// <summary>
		/// Sync with the LDAP on Start
		/// </summary>
		public string SyncOnStart;

		/// <summary>
		/// Constructor
		/// </summary>
		public LdapSettings()
		{
		}

		/// <summary>
		/// Get the iFolder LDAP Settings
		/// </summary>
		/// <returns>An LdapSettings Object</returns>
		public static LdapSettings GetSettings()
		{
			LdapSettings settings = new LdapSettings();
			Ldap.LdapSettings ldap = Ldap.LdapSettings.Get(Store.StorePath);

			// host
			settings.LdapHost = ldap.Uri.Host;

			// secure
			settings.LdapSecure = ldap.SSL.ToString();

			// port
			settings.LdapPort = ldap.Port.ToString();

			// DN
			settings.ProxyDN = ldap.ProxyDN;

            //Pull the password from the store, not the config file. The ldap sync removes it from the config
            Simias.Enterprise.Common.ProxyUser user = new Simias.Enterprise.Common.ProxyUser();
			settings.ProxyPassword = user.Password;

			// context
			ArrayList list = new ArrayList();
			foreach(string context in ldap.SearchContexts)
			{
				list.Add(context);
			}
			settings.SearchContexts = (string[])list.ToArray(typeof(string));

			// interval
			settings.SyncInterval = ldap.SyncInterval.ToString();
		
			// sync on start
			settings.SyncOnStart = ldap.SyncOnStart.ToString();

			return settings;
		}

		/// <summary>
		/// Set the iFolder LDAP Settings
		/// </summary>
		/// <param name="settings">The LdapSettings Object</param>
		public static void SetSettings(LdapSettings settings)
		{
			Ldap.LdapSettings ldap = Ldap.LdapSettings.Get(Store.StorePath);

			// host
			ldap.Host = settings.LdapHost;

			// secure
			ldap.SSL = bool.Parse(settings.LdapSecure.ToLower());

			// port
			ldap.Port = int.Parse(settings.LdapPort);

			// DN
			ldap.ProxyDN = settings.ProxyDN;
			
			// context
			ldap.SearchContexts = settings.SearchContexts;

			// interval
			ldap.SyncInterval = int.Parse(settings.SyncInterval);
		
			// sync on start
			ldap.SyncOnStart = bool.Parse(settings.SyncOnStart.ToLower());

			// save changes
			ldap.Commit();

			//Set the password in the store, not the config file. The ldap sync removes it from the config
			Simias.Enterprise.Common.ProxyUser user = new Simias.Enterprise.Common.ProxyUser();
			user.Password = settings.ProxyPassword;
		}
	}
}
