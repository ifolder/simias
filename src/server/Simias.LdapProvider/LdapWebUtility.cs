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
*                 $Author: Anil Kumar <kuanil@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.IO;
using System.Text.RegularExpressions;

using Simias;
using Simias.Storage;
using Simias.Server;
using Novell.Directory.Ldap;

namespace Simias.LdapProvider
{
	/// <summary>
	/// LDAP Utility Object
	/// </summary>
	public class LdapWebUtility
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
		/// AD userAccountControl flags
		/// </summary>
		[Flags]
		private enum ADS_USER_FLAGS
		{
			SCRIPT = 0X0001,
			ACCOUNTDISABLE = 0X0002,
			HOMEDIR_REQUIRED = 0X0008,
			LOCKOUT = 0X0010,
			PASSWD_NOTREQD = 0X0020,
			PASSWD_CANT_CHANGE = 0X0040,
			ENCRYPTED_TEXT_PASSWORD_ALLOWED = 0X0080,
			TEMP_DUPLICATE_ACCOUNT = 0X0100,
			NORMAL_ACCOUNT = 0X0200,
			INTERDOMAIN_TRUST_ACCOUNT = 0X0800,
			WORKSTATION_TRUST_ACCOUNT = 0X1000,
			SERVER_TRUST_ACCOUNT = 0X2000,
			DONT_EXPIRE_PASSWD = 0X10000,
			MNS_LOGON_ACCOUNT = 0X20000,
			SMARTCARD_REQUIRED = 0X40000,
			TRUSTED_FOR_DELEGATION = 0X80000,
			NOT_DELEGATED = 0X100000,
			USE_DES_KEY_ONLY = 0x200000,
			DONT_REQUIRE_PREAUTH = 0x400000,
			PASSWORD_EXPIRED = 0x800000,
			TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 0x1000000
		}

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
		public LdapWebUtility(string url, string dn, string password)
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
			try{
				connection.Disconnect();
			}catch{}
			connection = null;
		}

		public bool ChangePassword(string dn, string password)
		{
			LdapDirectoryType LdapType = QueryDirectoryType();
			switch ( LdapType )
			{
				case LdapDirectoryType.ActiveDirectory:
				{

					string quotedPassword = "\"" + password + "\"";
					char [] unicodePassword = quotedPassword.ToCharArray();
					sbyte [] passwordArray = new sbyte[unicodePassword.Length * 2];

					for (int i=0; i<unicodePassword.Length; i++) {
						passwordArray[i*2 + 1] = (sbyte) (unicodePassword[i] >> 8);
						passwordArray[i*2 + 0] = (sbyte) (unicodePassword[i] & 0xff);
					}

					LdapAttribute attribute = new LdapAttribute("UnicodePwd", passwordArray);
					LdapModification modification = new LdapModification(LdapModification.REPLACE, attribute);
					connection.Modify(dn, modification);

					return true;
	
				}
				case LdapDirectoryType.OpenLDAP:
				case LdapDirectoryType.eDirectory:
				deafult:
				{
                        		LdapAttribute attribute = new LdapAttribute("userPassword", password);

                        		LdapModification modification = new LdapModification(LdapModification.REPLACE, attribute);

                        		connection.Modify(dn, modification);
					return true;
				}
				//return false;
			}
			return false;
		}


		/// <summary>
		/// Create a New User in the LDAP Tree
		/// </summary>
		/// <param name="dn">The New User DN</param>
		/// <param name="password">The New User Password</param>
		/// <returns>true, if the user was created. false, if the user already exists.</returns>
		public bool CreateUser(string dn, string password)
		{
			// KLUDGE: The search method is currently failing with the LDAP libraries.

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
						Regex cnRegex=null;
						string quotedPassword = "\"" + password + "\"";
						int AccEnable = (int)ADS_USER_FLAGS.NORMAL_ACCOUNT | (int)ADS_USER_FLAGS.DONT_EXPIRE_PASSWD; // Flags set to 66048
						char [] unicodePassword = quotedPassword.ToCharArray();
						sbyte [] passwordArray = new sbyte[unicodePassword.Length * 2];
						for (int i=0; i<unicodePassword.Length; i++) {
							passwordArray[i*2 + 1] = (sbyte) (unicodePassword[i] >> 8);
							passwordArray[i*2 + 0] = (sbyte) (unicodePassword[i] & 0xff);
						}
						if(dn.ToLower().StartsWith("cn="))
							cnRegex = new Regex(@"^cn=(.*?),.*$",RegexOptions.IgnoreCase | RegexOptions.Compiled);
						else if (dn.ToLower().StartsWith("uid="))
							cnRegex = new Regex(@"^uid=(.*?),.*$",RegexOptions.IgnoreCase | RegexOptions.Compiled);
						string cn = cnRegex.Replace(dn, "$1");

						// create user attributes
						attributeSet.Add(new LdapAttribute("objectClass", "user"));
						attributeSet.Add(new LdapAttribute("objectClass", "InetOrgPerson"));
						attributeSet.Add(new LdapAttribute("cn", cn));
						attributeSet.Add(new LdapAttribute("SamAccountName", cn));
						attributeSet.Add(new LdapAttribute("sn", cn));
						attributeSet.Add(new LdapAttribute("userAccountControl", AccEnable.ToString()));
						attributeSet.Add(new LdapAttribute("UnicodePwd", passwordArray));
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
			QueryDirectoryType();
			if( DirectoryType.Equals( LdapDirectoryType.ActiveDirectory ))
			{
			}
			else if (DirectoryType.Equals( LdapDirectoryType.OpenLDAP ))
			{
				// TODO: Modify this to support OpenLDAP.
			}
			else if ( DirectoryType.Equals( LdapDirectoryType.eDirectory ) )
			{
				LdapAttribute attribute = new LdapAttribute("acl", new String[]
				{
					String.Format("1#subtree#{0}#[Entry Rights]", userDN),
					String.Format("3#subtree#{0}#[All Attributes Rights]", userDN)
				});
				LdapModification modification = new LdapModification(LdapModification.ADD, attribute);
				connection.Modify(containerDN, modification);
			}
		}

		/// <summary>
		/// Queries to find the type of directory  
		/// </summary>
		/// <returns>The LDAP directory type.</returns>
		public LdapDirectoryType QueryDirectoryType()
		{
			LdapEntry entry = connection.Read( "" );
			LdapAttribute attr = entry.getAttribute( "vendorName" );
			if ( attr != null )
			{
				ldapType = LdapDirectoryType.eDirectory;
			}
			else
			{
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
