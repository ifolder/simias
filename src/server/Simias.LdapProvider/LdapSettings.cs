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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Diagnostics;


using Simias;
using Simias.Storage;
using Simias.Server;

namespace Simias.LdapProvider
{
	/// <summary>
	/// The LDAP directory type.
	/// </summary>
	public enum LdapDirectoryType
	{
		/// <summary>
		/// unknown
		/// </summary>
		Unknown,

		/// <summary>
		/// eDirectory
		/// </summary>
		eDirectory,

		/// <summary>
		/// Active Directory
		/// </summary>
		ActiveDirectory,

		/// <summary>
		/// OpenLDAP
		/// </summary>
		OpenLDAP
	}

	public class LdapSettings
    {
	    #region Fields
		private static readonly string SectionTag = "section";
		private static readonly string SettingTag = "setting";
		private static readonly string NameAttr = "name";
		private static readonly string ValueAttr = "value";

		private static readonly string ProxyPasswordFile = ".simias.ppf";

		private static readonly string LdapAuthenticationSection = "LdapAuthentication";
                private static readonly string ServerSection = "Server";
                public static readonly string MasterAddressKey = "MasterAddress";
		public static readonly string UriKey = "LdapUri";
		public static readonly string ProxyDNKey = "ProxyDN";
		public static readonly string ProxyPasswordKey = "ProxyPassword";

		public static readonly string DomainSection = "EnterpriseDomain";
		public static readonly string OldDomainSection = "Domain";
		public static readonly string SimiasAdminDNKey = "AdminName";
		public static readonly string SimiasOldAdminDNKey = "AdminDN";

		private static readonly string LdapSystemBookSection = "LdapProvider";
		private static readonly string OldLdapSystemBookSection = "LdapSystemBook";
		private static readonly string SearchKey = "Search";
		public static readonly string XmlContextTag = "Context";
		private static readonly string XmlDNAttr = "dn";
		private static readonly string NamingAttributeKey = "NamingAttribute";

		private static readonly string IdentitySection = "Identity";
		private static readonly string AssemblyKey = "Assembly";
		private static readonly string ClassKey = "Class";

		private static readonly string DefaultUri = "ldaps://localhost/";

		public static readonly string DefaultNamingAttribute = "cn";

		public static readonly string UriSchemeLdap = "ldap";
		public static readonly int UriPortLdap = 389;
		public static readonly string UriSchemeLdaps = "ldaps";
		public static readonly int UriPortLdaps = 636;
		#endregion

		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		private enum ChangeMap : uint
		{
			unchanged = 0x00000000,
            uri = 0x00000001,
            scheme = 0x00000002,
            host = 0x00000004,
            port = 0x00000008,
            proxy = 0x00000010,
			password = 0x00000020,
        		masterURL= 0x00000040,
			searchContexts = 0x00020000,
            syncInterval = 0x00040000,
            syncOnStart = 0x00080000,
	    	namingAttribute = 0x00100000
		}

        private ChangeMap settingChangeMap;
        private Uri uri = new Uri( DefaultUri );

		private string scheme;
        private string host;
        private int port;
		private LdapDirectoryType ldapType = LdapDirectoryType.Unknown;

        private string proxy = String.Empty;
        private string masterURL= String.Empty;
		private string password = String.Empty;
		private string simiasAdmin = String.Empty;

		private XmlElement searchElement;

		private ArrayList searchContexts = new ArrayList();
		private string namingAttribute = DefaultNamingAttribute;

		private string storePath;
    
		#region Properties
		/// <summary>
		/// Gets the admin DN.
		/// </summary>
		public string AdminDN
		{
			get { return ( this.simiasAdmin ); }
		}

		/// <summary>
		/// Gets/sets the LDAP directory type.
		/// </summary>
		public LdapDirectoryType DirectoryType
		{
			get { return ldapType; }
			set { ldapType = value; }
		}

		/// <summary>
		/// Gets/sets the host.
		/// </summary>
		public string Host
		{
			get { return (this.host); }
			set
			{
				this.host = value;
				settingChangeMap |= ChangeMap.host;
			}
		}

		/// <summary>
		/// Gets/sets the naming attribute.
		/// </summary>
		public string NamingAttribute
		{
			get { return ( this.namingAttribute ); }
			set
			{
				this.namingAttribute = value;
				settingChangeMap |= ChangeMap.namingAttribute;
			}
		}

		/// <summary>
		/// Gets/sets the port.
		/// </summary>
		public int Port
		{
			get { return (this.port); }
			set
			{
				this.port = value;
				settingChangeMap |= ChangeMap.port;
			}
		}

		/// <summary>
		/// Gets/sets the proxy DN.
		/// </summary>
		public string ProxyDN
		{
			get { return ( this.proxy ); }
			set
			{
				this.proxy = value;
				settingChangeMap |= ChangeMap.proxy;
			}
		}

		/// <summary>
		/// Gets/sets the proxy DN.
		/// </summary>
		public string MasterURL
		{
			get { return ( this.masterURL); }
			set
			{
				this.masterURL= value;
				settingChangeMap |= ChangeMap.masterURL;
			}
		}

		/// <summary>
		/// Gets/sets the proxy password.
		/// </summary>
		public string ProxyPassword
		{
			get { return ( this.password ); }
			set
			{
				this.password = value;
				settingChangeMap |= ChangeMap.password;
			}
		}

		/// <summary>
		/// Gets/sets the scheme.
		/// </summary>
		public string Scheme
		{
			get { return (this.scheme); }
			set
			{
				this.scheme = value;
				settingChangeMap |= ChangeMap.scheme;
			}
		}

		/// <summary>
		/// Gets/sets the contexts that are searched when provisioning users.
		/// </summary>
		public IEnumerable SearchContexts
		{
			get { return ( ( IEnumerable ) this.searchContexts.Clone() ); }
			set
			{
				searchContexts.Clear();
				foreach ( string context in value )
				{
					searchContexts.Add( context );
				}

				settingChangeMap |= ChangeMap.searchContexts;
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if SSL is being used.
		/// </summary>
		public bool SSL
		{
			get { return ( this.Scheme.Equals( UriSchemeLdaps ) ? true : false ); }
			set { this.Scheme = value ? UriSchemeLdaps : UriSchemeLdap; }
		}

		/// <summary>
		/// Gets/sets the Uri of the LDAP server.
		/// </summary>
		public Uri Uri
		{
			get { return( this.uri ); }
			set
			{
				this.uri = value;
				this.scheme = uri.Scheme;
				this.host = uri.Host;
				if ( ( this.port = uri.Port ) == -1 )
				{
					this.port = SSL ? UriPortLdaps : UriPortLdap;
				}

				settingChangeMap |= ChangeMap.uri;
			}
		}
		#endregion

		#region Constructors
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="storePath">path of simias store</param>
        private LdapSettings( string storePath )
        {
			this.storePath = storePath;

			Configuration config = new Configuration( storePath, true );
            settingChangeMap = 0;

			string identity = config.Get( IdentitySection, AssemblyKey );
			if ( identity != null )
			{
				switch ( identity )
				{
					case "Simias.LdapProvider":
						ldapType = LdapDirectoryType.eDirectory;
						break;
					case "Simias.ADLdapProvider":
						ldapType = LdapDirectoryType.ActiveDirectory;
						break;
					case "Simias.OpenLdapProvider":
						ldapType = LdapDirectoryType.OpenLDAP;
						break;
				}
			}

			// <setting name="LdapUri" />
			string uriString = config.Get( LdapAuthenticationSection, UriKey );
			if ( uriString != null )
			{
				this.uri = new Uri( uriString );
			}

		    this.scheme = uri.Scheme;
			this.host = uri.Host;
			if ( ( this.port = uri.Port ) == -1 )
			{
				this.port = SSL ? UriPortLdaps : UriPortLdap;
			}

            string proxyString = config.Get( LdapAuthenticationSection, ProxyDNKey );
			if ( proxyString != null )
			{
				proxy = proxyString;
			}

            string masterUrlString = config.Get( ServerSection, MasterAddressKey);
			if ( masterUrlString != null )
			{
				masterURL = masterUrlString;
			}

			// Get the password from the file if it exists.
			this.password = GetProxyPasswordFromFile();

			string simiasAdminString = config.Get( DomainSection, SimiasAdminDNKey );
			if ( simiasAdminString != null )
			{
				simiasAdmin = simiasAdminString;
			}

		    // <setting name="Search" />
		    searchElement = config.GetElement( LdapSystemBookSection, SearchKey );
			if ( searchElement != null )
			{
				XmlNodeList contextNodes = searchElement.SelectNodes( XmlContextTag );
				foreach( XmlElement contextNode in contextNodes )
				{
					searchContexts.Add( contextNode.GetAttribute( XmlDNAttr ) );
				}
			}

			string namingAttributeString = config.Get( LdapSystemBookSection, NamingAttributeKey );
			if ( namingAttributeString != null )
			{
				this.namingAttribute = namingAttributeString;
			}
        }

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="storePath">path of simias store</param>
		/// <param name="upgrade">is it an upgrade case</param>
        private LdapSettings( string storePath, bool upgrade )
        {
        	if(upgrade)
        	{
				this.storePath = storePath;
				
				Configuration config = new Configuration( storePath, true );
				settingChangeMap = 0;
				
				ldapType = LdapDirectoryType.eDirectory;
				
				// <setting name="LdapUri" />
				string uriString = config.Get( LdapAuthenticationSection, UriKey );
				if ( uriString != null )
				{
					this.uri = new Uri( uriString );
				}
				
				this.scheme = uri.Scheme;
				this.host = uri.Host;
				if ( ( this.port = uri.Port ) == -1 )
				{
					this.port = SSL ? UriPortLdaps : UriPortLdap;
				}
				
				string proxyString = config.Get( LdapAuthenticationSection, ProxyDNKey );
				if ( proxyString != null )
				{
					proxy = proxyString;
				}

            			string masterUrlString = config.Get( ServerSection, MasterAddressKey);
                        	if ( masterUrlString != null )
                        	{
                                	masterURL = masterUrlString;
                        	}
				
				// Get the password from the file if it exists.
				this.password = GetProxyPasswordFromFile();
				
				string simiasAdminString = config.Get( OldDomainSection, SimiasOldAdminDNKey );
				if ( simiasAdminString != null )
				{
					simiasAdmin = simiasAdminString;
				}
				
				// <setting name="Search" />
				searchElement = config.GetElement( OldLdapSystemBookSection, SearchKey );
				if ( searchElement != null )
				{
					XmlNodeList contextNodes = searchElement.SelectNodes( XmlContextTag );
					foreach( XmlElement contextNode in contextNodes )
					{
						searchContexts.Add( contextNode.GetAttribute( XmlDNAttr ) );
					}
				}
				
				string namingAttributeString = config.Get( OldLdapSystemBookSection, NamingAttributeKey );
				if ( namingAttributeString != null )
				{
					this.namingAttribute = namingAttributeString;
				}
        	}
				
        }
		#endregion

		#region Private Methods
        /// <summary>
        ///  get the proxy password from store
        /// </summary>
        /// <returns>password from the file. empty string on unsuccessful</returns>
		private string GetProxyPasswordFromFile()
		{
			string proxyPassword = String.Empty;
			string ppfPath = Path.Combine( storePath, ProxyPasswordFile );
			if ( File.Exists( ppfPath ) )
			{
				using ( StreamReader sr = File.OpenText( ppfPath ) )
				{
					// Password should be a single line of text.
					proxyPassword = sr.ReadLine();
				}
			}

			return proxyPassword;
		}

        /// <summary>
        /// sets the proxy password into store
        /// </summary>
		private void SetProxyPasswordInFile()
		{
			string ppfPath = Path.Combine( storePath, ProxyPasswordFile );
			using ( StreamWriter sw = File.CreateText( ppfPath ) )
			{
				sw.WriteLine( password );
			}
		}

        /// <summary>
        /// set the config value into the document
        /// </summary>
        /// <param name="document">XML document where change has to be made</param>
        /// <param name="section">section to be set</param>
        /// <param name="key">key to be set</param>
        /// <param name="configValue">value to be put</param>
        /// <returns>true if successful</returns>
		private bool SetConfigValue( XmlDocument document, string section, string key, string configValue )
		{
			bool status = false;

			// Build an xpath for the setting.
			string str = string.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key );
			XmlElement element = ( XmlElement )document.DocumentElement.SelectSingleNode( str );
			if ( element != null )
			{
				element.SetAttribute( ValueAttr, configValue );
				status = true;
			}
			else
			{
				// The setting doesn't exist, so create it.
				element = document.CreateElement(SettingTag);
				element.SetAttribute(NameAttr, key);
				element.SetAttribute(ValueAttr, configValue);
				str = string.Format("//{0}[@{1}='{2}']", SectionTag, NameAttr, section);
				XmlElement eSection = (XmlElement)document.DocumentElement.SelectSingleNode(str);
				if ( eSection == null )
				{
					// If the section doesn't exist, create it.
					eSection = document.CreateElement( SectionTag );
					eSection.SetAttribute( NameAttr, section );
					document.DocumentElement.AppendChild( eSection );
				}

				eSection.AppendChild(element);
				status = true;
			}

			return status;
		}

        /// <summary>
        /// get the ldap element from this document
        /// </summary>
        /// <param name="document">XML document to be searched</param>
        /// <returns>the XML element for ldap section</returns>
		private XmlElement GetSearchElement( XmlDocument document )
		{
			string str = String.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, LdapSystemBookSection, SettingTag, SearchKey );
			XmlElement element = ( XmlElement )document.DocumentElement.SelectSingleNode( str );
			if ( element == null )
			{
				// The setting doesn't exist, so create it.
				element = document.CreateElement( SettingTag );
				element.SetAttribute( NameAttr, SearchKey );
				str = string.Format( "//{0}[@{1}='{2}']", SectionTag, NameAttr, LdapSystemBookSection );
				XmlElement eSection = ( XmlElement )document.DocumentElement.SelectSingleNode(str);
				if ( eSection == null )
				{
					// If the section doesn't exist, create it.
					eSection = document.CreateElement( SectionTag );
					eSection.SetAttribute( NameAttr, LdapSystemBookSection );
					document.DocumentElement.AppendChild( eSection );
				}

				eSection.AppendChild(element);
			}

			return element;
		}
		#endregion

		#region Public Methods
        /// <summary>
        /// get the ldapsetting object
        /// </summary>
        /// <param name="storePath">path of store</param>
        /// <param name="upgrade">is it upgrade</param>
        /// <returns>get the ldapsetting object</returns>
        public static LdapSettings Get( string storePath, bool upgrade )
        {
        	if(upgrade)
	            return ( new LdapSettings( storePath, upgrade ) );
		else
	            return ( new LdapSettings( storePath ) );
        }

        /// <summary>
        /// get the ldapsetting object
        /// </summary>
        /// <param name="storePath">path of store</param>
        /// <returns>get ldapsetting object</returns>
        public static LdapSettings Get( string storePath )
        {
            return ( new LdapSettings( storePath ) );
        }

        /// <summary>
        /// commit the ldapsetting
        /// </summary>
		public void Commit()
		{
			if (settingChangeMap != 0)
			{
				// Build a path to the Simias.config file.
				string configFilePath = 
					Path.Combine( storePath, Simias.Configuration.DefaultConfigFileName );

				// Load the configuration file into an xml document.
				XmlDocument configDoc = new XmlDocument();
				configDoc.Load( configFilePath );

				if ( ( settingChangeMap & ChangeMap.uri ) != ChangeMap.unchanged )
				{
					SetConfigValue( configDoc, LdapAuthenticationSection, UriKey, uri.ToString() );
				}
				else if ( ( settingChangeMap & ( ChangeMap.scheme | ChangeMap.host | ChangeMap.port ) ) != ChangeMap.unchanged )
				{
					UriBuilder ub = new UriBuilder( scheme, host, port );
					SetConfigValue( configDoc, LdapAuthenticationSection, UriKey, ub.Uri.ToString() );
				}

				if ( ( settingChangeMap & ChangeMap.proxy ) == ChangeMap.proxy )
				{
					SetConfigValue( configDoc, LdapAuthenticationSection, ProxyDNKey, proxy );
				}

				if ( ( settingChangeMap & ChangeMap.namingAttribute ) == ChangeMap.namingAttribute )
				{
					SetConfigValue( configDoc, LdapSystemBookSection, NamingAttributeKey, namingAttribute );
				}

				if ( ( settingChangeMap & ChangeMap.searchContexts ) == ChangeMap.searchContexts )
				{
					XmlElement searchElement = GetSearchElement( configDoc );
					if ( searchElement != null )
					{
						XmlNodeList contextNodes = searchElement.SelectNodes( XmlContextTag );
						foreach( XmlElement contextNode in contextNodes )
						{
							searchElement.RemoveChild( contextNode );
						}

						foreach( string dn in searchContexts )
						{
							XmlElement element = configDoc.CreateElement( XmlContextTag );
							element.SetAttribute( XmlDNAttr, dn );
							searchElement.AppendChild( element );
						}
					}
				}

				if ( ( settingChangeMap & ChangeMap.password ) == ChangeMap.password )
				{
					SetProxyPasswordInFile();
				}

				switch ( ldapType )
				{
					case LdapDirectoryType.ActiveDirectory:
						SetConfigValue( configDoc, IdentitySection, AssemblyKey, "Simias.ADLdapProvider" );
						SetConfigValue( configDoc, IdentitySection, ClassKey, "Simias.ADLdapProvider.User" );
						break;
					case LdapDirectoryType.eDirectory:
						SetConfigValue( configDoc, IdentitySection, AssemblyKey, "Simias.LdapProvider" );
						SetConfigValue( configDoc, IdentitySection, ClassKey, "Simias.LdapProvider.User" );
						break;
					case LdapDirectoryType.OpenLDAP:
						SetConfigValue( configDoc, IdentitySection, AssemblyKey, "Simias.OpenLdapProvider" );
						SetConfigValue( configDoc, IdentitySection, ClassKey, "Simias.OpenLdapProvider.User" );
						break;
					default:
						throw new Exception( "The LDAP directory type is unknown!" );
				}

				// Write the configuration file settings.
				XmlTextWriter xtw = new XmlTextWriter( configFilePath, Encoding.UTF8 );
				try
				{
					xtw.Formatting = Formatting.Indented;
					configDoc.WriteTo( xtw );
				}
				finally
				{
					xtw.Close();
				}
			}
		}

        /// <summary>
        /// set ldap settings into Simias.config file
        /// </summary>
        /// <param name="LdapAdminDN">admin DN for ldap</param>
        /// <param name="LdapAdminPwd">password for ldap admin</param>
        /// <param name="IsMaster">Is this master server</param>
		public void Commit(string LdapAdminDN, string LdapAdminPwd, bool IsMaster)
		{
			if (settingChangeMap != 0)
			{
				// Build a path to the Simias.config file.
				string configFilePath = 
					Path.Combine( storePath, Simias.Configuration.DefaultConfigFileName );

				// Load the configuration file into an xml document.
				XmlDocument configDoc = new XmlDocument();
				configDoc.Load( configFilePath );


				if ( ( settingChangeMap & ChangeMap.uri ) != ChangeMap.unchanged )
				{
					UpdateLdapSettings(LdapAdminDN, LdapAdminPwd, "LDAPURI", IsMaster);
					SetConfigValue( configDoc, LdapAuthenticationSection, UriKey, uri.ToString() );
				}
				else if ( ( settingChangeMap & ( ChangeMap.scheme | ChangeMap.host | ChangeMap.port ) ) != ChangeMap.unchanged )
				{
					UpdateLdapSettings(LdapAdminDN, LdapAdminPwd, "LDAPURI", IsMaster);
					UriBuilder ub = new UriBuilder( scheme, host);//, port );
					SetConfigValue( configDoc, LdapAuthenticationSection, UriKey, ub.Uri.ToString() );
				}

				if ( ( settingChangeMap & ChangeMap.proxy ) == ChangeMap.proxy )
				{
					UpdateLdapSettings(LdapAdminDN, LdapAdminPwd, "PROXYDN", IsMaster);
					SetConfigValue( configDoc, LdapAuthenticationSection, ProxyDNKey, proxy );
				}

				if ( ( settingChangeMap & ChangeMap.namingAttribute ) == ChangeMap.namingAttribute )
				{
					SetConfigValue( configDoc, LdapSystemBookSection, NamingAttributeKey, namingAttribute );
				}

				if ( ( settingChangeMap & ChangeMap.searchContexts ) == ChangeMap.searchContexts )
				{
					UpdateLdapSettings(LdapAdminDN, LdapAdminPwd, "SEARCHCONTEXT", IsMaster);
					XmlElement searchElement = GetSearchElement( configDoc );
					if ( searchElement != null )
					{
						XmlNodeList contextNodes = searchElement.GetElementsByTagName( XmlContextTag );
						foreach( XmlElement contextNode in contextNodes )
						{
							searchElement.RemoveChild( contextNode );
						}

						foreach( string dn in searchContexts )
						{
							XmlElement element = configDoc.CreateElement( XmlContextTag );
							element.SetAttribute( XmlDNAttr, dn );
							searchElement.AppendChild( element );
						}
					}
				}

				if ( ( settingChangeMap & ChangeMap.password ) == ChangeMap.password )
				{
					UpdateLdapSettings(LdapAdminDN, LdapAdminPwd, "PROXYPWD", IsMaster);
					SetProxyPasswordInFile();
				}

				switch ( ldapType )
				{
					case LdapDirectoryType.ActiveDirectory:
						SetConfigValue( configDoc, IdentitySection, AssemblyKey, "Simias.ADLdapProvider" );
						SetConfigValue( configDoc, IdentitySection, ClassKey, "Simias.ADLdapProvider.User" );
						break;
					case LdapDirectoryType.eDirectory:
						SetConfigValue( configDoc, IdentitySection, AssemblyKey, "Simias.LdapProvider" );
						SetConfigValue( configDoc, IdentitySection, ClassKey, "Simias.LdapProvider.User" );
						break;
					case LdapDirectoryType.OpenLDAP:
						SetConfigValue( configDoc, IdentitySection, AssemblyKey, "Simias.OpenLdapProvider" );
						SetConfigValue( configDoc, IdentitySection, ClassKey, "Simias.OpenLdapProvider.User" );
						break;
					default:
						throw new Exception( "The LDAP directory type is unknown!" );
				}

				// Write the configuration file settings.
				XmlTextWriter xtw = new XmlTextWriter( configFilePath, Encoding.UTF8 );
				try
				{
					xtw.Formatting = Formatting.Indented;
					configDoc.WriteTo( xtw );
				}
				finally
				{
					xtw.Close();
				}
			}
		}
		
        /// <summary>
        /// Update the ldap settings
        /// </summary>
        /// <param name="LdapAdminDN">ldap admin DN</param>
        /// <param name="LdapAdminPwd">ldap admin password</param>
        /// <param name="FieldToUpdate">which field is to update in config file</param>
        /// <param name="IsMaster">Is it master server</param>
		private void UpdateLdapSettings(string LdapAdminDN, string LdapAdminPwd, string FieldToUpdate, bool IsMaster)
		{
			if (FieldToUpdate.Equals("LDAPURI"))
			{
				UriBuilder newUri = new UriBuilder();
				newUri.Host = host;
				newUri.Scheme = scheme; 
				Uri newldapUrl = new Uri(newUri.ToString());
				string ldapUrl = newldapUrl.ToString();
	                        log.Debug("into condition ldapuri modification, ldapurl is {0} and going to create an instance of ldaputility", ldapUrl);
				LdapWebUtility ldapUtility = new LdapWebUtility(ldapUrl, LdapAdminDN, LdapAdminPwd );

				if (ldapUtility.Secure )
				{
					string certfile = Path.Combine( storePath, "RootCert.cer" );

                                	if (Execute("../../../../bin/get-root-certificate", "{0} {1} {2} {3} get {4}",
                                            ldapUtility.Host, ldapUtility.Port, LdapAdminDN, LdapAdminPwd, certfile) != 0)
                                	{
						//Failed , getting certificate and install
						log.Debug("Failed : getting certificate for {0}",ldapUtility.Host);
						throw new Exception( string.Format( "Failed to get certificate."));
					}
                                	if (Execute("/usr/bin/mono", "/usr/lib/mono/1.0/certmgr.exe -add -c Trust {0}",
                                            certfile) != 0)
                                	{
						//Failed , getting certificate and install
						log.Debug("Failed : Installing certificate for {0}",ldapUtility.Host);
						throw new Exception( string.Format( "Failed to save certificate."));
					}
				}	
	                        log.Debug("Connecting to {0}", ldapUrl);
				try
				{
       	                		ldapUtility.Connect();
				}
				catch(Exception ex)
				{
						log.Debug("Ldap connect failed to server URL {0} ",ldapUrl);
						throw new Exception( string.Format( "Ldap connect failed to server URL {0} ", ldapUrl));
				}

				// get the directory type.
                        	log.Debug("Querying for directory type...");
                        	LdapDirectoryType directoryType = ldapUtility.QueryDirectoryType();
                        	log.Debug( " {0}", directoryType );
				ldapUtility.Disconnect();

                        	if ( directoryType.Equals( LdapDirectoryType.Unknown ) )
                        	{
                               		throw new Exception( string.Format( "Unable to determine directory type for {0}", ldapUtility.Host ) );
                        	}
				
				// now check connecting with this proxy 
				ldapUtility = new LdapWebUtility(ldapUrl, proxy, password);
				try
				{
       	                		ldapUtility.Connect();
				}
				catch(Exception ex)
				{
						log.Debug("Ldap connect failed to server URL {0} with proxy user {1} ",ldapUrl, proxy);
						throw new Exception( string.Format( "Ldap connect failed to server URL {0} with proxy user {1} ", ldapUrl, proxy));
				}
				ldapUtility.Disconnect();
			}
			else
			{
				// ldap IP and SSL status has not changed , other fields (context, proxyDN, proxypwd) might have changed
					
				UriBuilder newUri = new UriBuilder();
				newUri.Host = host;
				newUri.Scheme = scheme; 
				Uri newldapUrl = new Uri(newUri.ToString());
				string ldapUrl = newldapUrl.ToString();
				LdapWebUtility ldapUtility = new LdapWebUtility(ldapUrl, LdapAdminDN, LdapAdminPwd );
				// connect
        	                ldapUtility.Connect();
				if(FieldToUpdate.Equals("PROXYDN"))
				{
                        		LdapDirectoryType directoryType = ldapUtility.QueryDirectoryType();
					// proxy DN has changed , so either create user or change the password.
					if(password == null || password == "")
					{
						ProxyUser proxyDetails = new ProxyUser();
						ProxyPassword = proxyDetails.Password;
					}
					if (ldapUtility.CreateUser(proxy, password))
					{	
						// successful, proxy user is created
						log.Debug("New user created with DN = {0} ",proxy);
					}
					settingChangeMap |= ChangeMap.searchContexts;
				}
				else if (FieldToUpdate.Equals("PROXYPWD"))
				{
					if(password == null || password == "")
					{
						ProxyUser proxyDetails = new ProxyUser();
						password = proxyDetails.Password;
					}
					ldapUtility.ChangePassword(proxy, password);
				}	
				else
					UpdateLdapContexts(ldapUrl, LdapAdminDN, LdapAdminPwd);
					
				ldapUtility.Disconnect();
			}
		}
        
        /// <summary>
        /// to update ldap contexts , If proxy user is not having permission then grant permission
        /// </summary>
        /// <param name="ldapUrl">ldap url</param>
        /// <param name="LdapAdminDN">ldap admin dn</param>
        /// <param name="LdapAdminPwd">ldap admin password</param>
		private void UpdateLdapContexts(string ldapUrl, string LdapAdminDN, string LdapAdminPwd)
		{
			// for context , see if this proxy user has the rights or not in the search context (modified or unmodified)
			LdapWebUtility ldapUtility = new LdapWebUtility(ldapUrl , LdapAdminDN, LdapAdminPwd);
			// connect
	                log.Debug("context updation Connecting to {0}", ldapUrl);
        	        ldapUtility.Connect();
			foreach(string context in SearchContexts)
			{
				if ((context != null) && (context.Length > 0))
				{
					if ( !ldapUtility.ValidateSearchContext( context ) )
					{
						log.Debug("Invalid context entered :{0}", context);	
						throw new Exception( string.Format( "Invalid context entered: {0}", context ) );
					}
				}
				log.Debug("Granting Read Rights to {0} on {1}", proxy, context);	
				try
				{
					ldapUtility.GrantReadRights(proxy, context);
				}
				catch(Exception ex)
				{
					log.Debug("Some exception in granting read access to this proxy user (DN may exist already) {0} {1}", context, proxy);
				}
				
			}
			ldapUtility.Disconnect();
		}

		/// <summary>
                /// Execute the command in the shell.
                /// </summary>
                /// <param name="command">The command.</param>
                /// <param name="format">The arguments of the command.</param>
                /// <param name="args">The arguments for the format.</param>
                /// <returns>The results of the command.</returns>
                static int Execute(string command, string format, params object[] args)
                {
                        ProcessStartInfo info = new ProcessStartInfo( command, String.Format( format, args ) );
                        Process p = Process.Start(info);
                        p.WaitForExit();
                        return p.ExitCode;
                }

		#endregion
    }
}


