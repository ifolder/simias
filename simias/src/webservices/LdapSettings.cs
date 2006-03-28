/***********************************************************************
 *  $RCSfile: LdapSettings.cs,v $
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Rob
 *
 ***********************************************************************/

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
