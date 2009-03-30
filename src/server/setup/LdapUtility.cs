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
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <iFolder setup LdapUtility Class >
*
*
*******************************************************************************/

using System;
using System.IO;
using System.Text.RegularExpressions;

using Novell.Directory.Ldap;
using Simias.LdapProvider;

namespace Novell.iFolder.Utility
{
	/// <summary>
	/// LDAP Utility Object
	/// </summary>
	public class LdapUtility
	{
		#region Class Members
		/// <summary>
		/// LDAP Scheme
		/// </summary>
		public static readonly string LDAP_SCHEME = "ldap";

		/// <summary>
		/// Secure LDAP Scheme
		/// </summary>
		public static readonly string LDAP_SCHEME_SECURE = "ldaps";

		/// <summary>
		/// LDAP connection
		/// </summary>
		private LdapConnection connection;
		private string host;
		private int port;
		private bool secure;
		private string dn;
		private string password;
		private LdapDirectoryType ldapType = LdapDirectoryType.Unknown;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="url">LDAP URL</param>
		/// <param name="dn">LDAP User DN</param>
		/// <param name="password">LDAP User Password</param>
		public LdapUtility(string url, string dn, string password)
		{
			Uri ldapUrl = new Uri(url);

			host = ldapUrl.Host;

			// secure
			secure = ldapUrl.Scheme.ToLower().Equals(
				LDAP_SCHEME_SECURE) ? true : false;

			// port
			
			port = (ldapUrl.Port != -1) ? ldapUrl.Port : (secure ?
				LdapSettings.UriPortLdaps : LdapSettings.UriPortLdap);
			

			this.dn = dn;
			this.password = password;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Connect and/or Bind to the LDAP Server
		/// </summary>
		public void Connect()
		{
			connection = new LdapConnection();
			connection.SecureSocketLayer = secure;
			connection.Connect(host, port);
			connection.Bind(dn, password);
		}


		/// <summary>
		/// Disconnect from the LDAP Server
		/// </summary>
		public void Disconnect()
		{
			// In Mono 2.0 runtime environment, first connection.Disconnect() 
			// always throws exception(bug 449092). Since Disconnect() is 
			// called at the end all LDAP operations and this executable is 
			// a standalone executable(), simply ignoring the exception. 
			try
			{
				connection.Disconnect();
			}
			catch { }
			connection = null;
		}

        /// <summary>
        /// change the password
        /// </summary>
        /// <param name="dn">sn for which change is to be done</param>
        /// <param name="password">password</param>
        /// <returns>true if changed successfully</returns>
		public bool ChangePassword(string dn, string password)
		{
                        // TODO: Modify this to support Active Directory.

                        LdapAttribute attribute = new LdapAttribute("userPassword", password);

                        LdapModification modification = new LdapModification(LdapModification.REPLACE, attribute);

                        connection.Modify(dn, modification);
			return true;
		}


		/// <summary>
		/// Create a New User in the LDAP Tree
		/// </summary>
		/// <param name="dn">The New User DN</param>
		/// <param name="password">The New User Password</param>
		/// <returns>true, if the user was created. false, if the user already exists.</returns>
		public bool CreateUser(string dn, string password)
		{
			// TODO: Modify this to support OpenLDAP and Active Directory.

			// KLUDGE: The search method is currently failing with the LDAP libraries.

			//LdapSearchResults results = connection.Search(proxyDN,
			//	LdapConnection.SCOPE_BASE, "(objectclass=*)", null, false);

			bool created = true;
			//if (!results.hasMore())
			try
			{
				// find
				connection.Read(dn);

				created = false;
			}
			catch
			{
				LdapAttributeSet attributeSet = new LdapAttributeSet();
				switch ( ldapType )
				{
					case LdapDirectoryType.ActiveDirectory:
					{
						// TODO:
						break;
					}
					case LdapDirectoryType.eDirectory:
					{
						// parse the cn
						Regex cnRegex=null;
						if(dn.ToLower().StartsWith("cn="))
							cnRegex = new Regex(@"^cn=(.*?),.*$",RegexOptions.IgnoreCase | RegexOptions.Compiled);
						else if (dn.ToLower().StartsWith("uid="))
							cnRegex = new Regex(@"^uid=(.*?),.*$",RegexOptions.IgnoreCase | RegexOptions.Compiled);
						string cn = cnRegex.Replace(dn, "$1");

						// create user attributes
						attributeSet.Add(new LdapAttribute("objectClass", "inetOrgPerson"));
						attributeSet.Add(new LdapAttribute("cn", cn));
						attributeSet.Add(new LdapAttribute("sn", cn));
						attributeSet.Add(new LdapAttribute("userPassword", password));
						break;
					}
					case LdapDirectoryType.OpenLDAP:
					{
						Regex uidRegex = new Regex(@"^(.*?)=(.*?),.*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
						string uid = uidRegex.Replace(dn, "$2");

						// I think we can get away with just creating an inetOrgPerson ...
						// we don't need a posixAccount ... hmm, maybe a shadowAccount
						// so that the password can expire?
						attributeSet.Add(new LdapAttribute("objectClass", "inetOrgPerson"));//new string[]{"inetOrgPerson", "posixAccount", "shadowAccount"}));
						attributeSet.Add(new LdapAttribute("uid", uid));
						attributeSet.Add(new LdapAttribute("cn", uid));
						attributeSet.Add(new LdapAttribute("sn", uid));
						attributeSet.Add(new LdapAttribute("givenName", uid));
						attributeSet.Add(new LdapAttribute("displayName", uid));
						// TODO: Need to encrypt the password first.
						attributeSet.Add(new LdapAttribute("userPassword", password));
						break;
					}
				}
                                    
				// add user entry
				Console.Write("Creating proxy {0}", dn);
				LdapEntry entry = new LdapEntry(dn, attributeSet);
				connection.Add(entry);
			}

			// result
			return created;
		}

		/// <summary>
		/// Grant Read Rights to the LDAP User on the the LDAP Container
		/// </summary>
		/// <param name="userDN">The LDAP User DN</param>
		/// <param name="containerDN">The LDAP Container DN</param>
		public void GrantReadRights(string userDN, string containerDN)
		{
			// TODO: Modify this to support OpenLDAP and Active Directory.

			LdapAttribute attribute = new LdapAttribute("acl", new String[]
			{
				String.Format("1#subtree#{0}#[Entry Rights]", userDN),
				String.Format("3#subtree#{0}#[All Attributes Rights]", userDN)
			});
			
			LdapModification modification = new LdapModification(LdapModification.ADD, attribute);

			// at the root
			connection.Modify(containerDN, modification);
		}

		/// <summary>
		/// Queries to find the type of directory  
		/// </summary>
		/// <returns>The LDAP directory type.</returns>
		public LdapDirectoryType QueryDirectoryType()
		{
			Console.WriteLine("get directory type");			

			LdapAttribute attr	= null;
			LdapEntry entry	= null;
			bool eDirectory	= false;
			LdapSearchResults lsc=connection.Search("",
												LdapConnection.SCOPE_BASE,
												"objectClass=*",
												null,
												false);
			while (lsc.hasMore())
			{
				entry = null;				
				try 
				{
					entry = lsc.next();
				}
				catch(LdapException e) 
				{
					Console.WriteLine("Error: " + e.LdapErrorMessage);
					// Exception is thrown, go for next entry
					continue;
				}
				Console.WriteLine("\n" + entry.DN);
				LdapAttributeSet attributeSet = entry.getAttributeSet();
				System.Collections.IEnumerator ienum =  attributeSet.GetEnumerator();
				
				while(ienum.MoveNext())
				{
					attr = (LdapAttribute)ienum.Current;
					string attributeName = attr.Name;
					string attributeVal = attr.StringValue;
					Console.WriteLine( attributeName + ": value :" + attributeVal);

					//eDirectory specific attributes
					//If any of the following attribute is found, conclude this as eDirectory					
					if(	/*String.Equals(attributeName, "vendorVersion")==true ||	*/
						String.Equals(attributeVal, "Novell, Inc.")==true /* ||
						String.Equals(attributeName, "dsaName")==true ||*/
						
						/*String.Equals(attributeName, "directoryTreeName")==true*/)
					{
						eDirectory = true;
						Console.WriteLine("Type : Novell eDirectory");
						break;
					}
				}
			}	
			
			if ( eDirectory == true)
			{
				ldapType = LdapDirectoryType.eDirectory;
			}
			else
			{
				// Decide is this a Active Directory or not. If not AD then the assumption is OpenLDAP
				entry = connection.Read( "" );
				attr = entry.getAttribute( "defaultNamingContext" );
				if ( attr != null )
				{
					ldapType = LdapDirectoryType.ActiveDirectory;
				}
				else
				{
					ldapType = LdapDirectoryType.OpenLDAP;
				}
			}

			return ldapType;		
			
		}

		/// <summary>
		/// Validates a context.
		/// </summary>
		/// <param name="context">The context to validate.</param>
		/// <returns><b>True</b> if the context is valid; otherwise, <b>False</b> is returned.</returns>
		public bool ValidateSearchContext( string context )
		{
			bool result = false;

			try
			{
				// find
				connection.Read( context );
				result = true;
			}
			catch
			{}

			return result;
		}
		#endregion

		#region Properties
		/// <summary>
		/// LDAP Bind User DN
		/// </summary>
		public string DN
		{
			get { return dn; }
		}

		/// <summary>
		/// LDAP Host
		/// </summary>
		public string Host
		{
			get { return host; }
		}

		/// <summary>
		/// Gets the LDAP directory type.
		/// </summary>
		public LdapDirectoryType DirectoryType
		{
			get { return ldapType; }
		}

		/// <summary>
		/// LDAP Bind User Password
		/// </summary>
		public string Password
		{
			get { return password; }
		}

		/// <summary>
		/// LDAP Port
		/// </summary>
		public int Port
		{
			get { return port; }
		}

		/// <summary>
		/// LDAP Secure Connection
		/// </summary>
		public bool Secure
		{
			get { return secure; }
		}
		#endregion
	}
}
