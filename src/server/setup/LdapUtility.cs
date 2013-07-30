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
using System.Text;
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
						Regex cnRegex=null;
						int AccEnable = (int)ADS_USER_FLAGS.NORMAL_ACCOUNT | (int)ADS_USER_FLAGS.DONT_EXPIRE_PASSWD; // Flags set to 66048 
						string quotedPassword = "\"" + password + "\"";
						char [] unicodePassword = quotedPassword.ToCharArray();
						sbyte [] userPassword = new sbyte[unicodePassword.Length * 2];

						for (int i=0; i<unicodePassword.Length; i++) {
							userPassword[i*2 + 1] = (sbyte) (unicodePassword[i] >> 8);
							userPassword[i*2 + 0] = (sbyte) (unicodePassword[i] & 0xff);
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
						attributeSet.Add(new LdapAttribute("UnicodePwd", userPassword));
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
				try
				{
					LdapEntry entry = new LdapEntry(dn, attributeSet);
					connection.Add(entry);
				}
				catch(Exception ex)
				{
					if(ex.Message.IndexOf("Constraint Violation") != -1)
						throw new Exception("Constraint Violation: password too short or Q not active");
					if(ex.ToString().IndexOf("-16000") != -1)
						throw new Exception("Constraint Violation: password is too long");
					// This is applicable for both admin and proxy user.
					created = false;
				}
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
			if( DirectoryType.Equals( LdapDirectoryType.ActiveDirectory ))
			{
				// TODO: Modify this to support Active Directory
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
						String.Equals(attributeVal, "Novell, Inc.")==true || 
						String.Equals(attributeVal, "NetIQ Corporation") == true /* ||
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
		/// Extends the Active directory AD iFolderschema
		/// </summary>
		/// <returns>True on successful schema Extension and false on Failure. </returns>
		public bool ExtendADiFolderschema()
		{
			LdapAttribute attr	= null;
			LdapEntry entry	= null;
			string retschemaNamingContext = String.Empty;
			string[] searchAttributes = { "schemaNamingContext" };
			LdapSearchResults lsc=connection.Search(	"",
									LdapConnection.SCOPE_BASE,
									"objectClass=*",
									searchAttributes,
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
					continue;
				}
				LdapAttributeSet attributeSet = entry.getAttributeSet();
				System.Collections.IEnumerator ienum =  attributeSet.GetEnumerator();
				
				while(ienum.MoveNext())
				{
					attr = (LdapAttribute)ienum.Current;
					string attributeName = attr.Name;
					Console.WriteLine( attributeName + ": value :" + attr.StringValue);

					if( String.Equals(attributeName, searchAttributes[0]) == true )
					{
						retschemaNamingContext = attr.StringValue;
						break;
					}
				}
			}	
			try
			{
				LdapAttributeSet newattr_attributeSet = new LdapAttributeSet();
				newattr_attributeSet.Add(new LdapAttribute("adminDisplayName", "iFolderHomeServer"));
				newattr_attributeSet.Add(new LdapAttribute("attributeID", "2.16.840.1.113719.1.288.1.42"));
				newattr_attributeSet.Add(new LdapAttribute("cn", "iFolderHomeServer"));
				newattr_attributeSet.Add(new LdapAttribute("attributeSyntax", "2.5.5.12"));
				newattr_attributeSet.Add(new LdapAttribute("adminDescription", "iFolder 3.x iFolderHomeServer Attribute, stores DNS Name or IP address of Users/Groups iFolder server."));
				newattr_attributeSet.Add(new LdapAttribute("isMemberOfPartialAttributeSet", "FALSE"));
				newattr_attributeSet.Add(new LdapAttribute("isSingleValued", "TRUE"));
				newattr_attributeSet.Add(new LdapAttribute("showInAdvancedViewOnly", "FALSE"));
				newattr_attributeSet.Add(new LdapAttribute("lDAPDisplayName", "iFolderHomeServer"));
				newattr_attributeSet.Add(new LdapAttribute("distinguishedName", "CN=iFolderHomeServer," + retschemaNamingContext));
				newattr_attributeSet.Add(new LdapAttribute("objectCategory", "CN=Attribute-Schema," + retschemaNamingContext));
				newattr_attributeSet.Add(new LdapAttribute("objectClass", "attributeSchema"));
				newattr_attributeSet.Add(new LdapAttribute("oMSyntax", "64"));
				newattr_attributeSet.Add(new LdapAttribute("name", "iFolderHomeServer"));
				newattr_attributeSet.Add(new LdapAttribute("searchFlags", "0"));
			
				Console.WriteLine("\nExtending Active Directory Schema for {0}", "CN=iFolderHomeServer," + retschemaNamingContext);
				LdapEntry newattr_entry = new LdapEntry("CN=iFolderHomeServer," + retschemaNamingContext, newattr_attributeSet);
				connection.Add(newattr_entry);


				LdapAttribute newattr_modattribute = new LdapAttribute("schemaUpdateNow", "1");
				LdapModification newattr_modification = new LdapModification(LdapModification.REPLACE, newattr_modattribute);
				connection.Modify("", newattr_modification);

				Console.WriteLine("\n Updating {0}", "CN=user," + retschemaNamingContext);
				LdapAttribute newclass_modattribute = new LdapAttribute("mayContain", "iFolderHomeServer");
				LdapModification newclass_modification = new LdapModification(LdapModification.ADD, newclass_modattribute);
				connection.Modify("cn=user,"+retschemaNamingContext, newclass_modification);

				newclass_modattribute = new LdapAttribute("schemaUpdateNow", "1");
				newclass_modification = new LdapModification(LdapModification.REPLACE, newclass_modattribute);
				connection.Modify("", newclass_modification);
				Console.WriteLine("Completed.\n");
			}
			catch( LdapException e )
			{
				if(e.ResultCode == LdapException.ENTRY_ALREADY_EXISTS)
				{
					Console.WriteLine( "\n Active Directory iFolder Schema is already Extended.");
					return true;
				}
				else
				{
					Console.WriteLine( "\n Unable to extend Active Directory iFolder Schema. {0}::{1}",e.ResultCode.ToString(), e.Message);
					return false;
				}
			}
			catch( Exception e )
			{
				Console.WriteLine( "\n Unable to extend Active Directory iFolder Schema. Ex.Message {0}",e.Message);
				return false;
			}
			Console.WriteLine( "\nActive Directory iFolder Schema Extended.");
			return true;
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
