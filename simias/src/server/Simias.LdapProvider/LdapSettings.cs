/***********************************************************************
 *  $RCSfile: LdapSettings.cs,v $
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
 *  Author: Bruce Bergeson <bberg@novell.com>
 *
 ***********************************************************************/


using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using Simias;
using Simias.Storage;

namespace Simias.LdapProvider
{
	public class LdapSettings
    {
	    #region Fields
		private static readonly string SectionTag = "section";
		private static readonly string SettingTag = "setting";
		private static readonly string NameAttr = "name";
		private static readonly string ValueAttr = "value";

		private static readonly string ProxyPasswordFile = ".simias.ppf";

		private static readonly string LdapAuthenticationSection = "LdapAuthentication";
		public static readonly string UriKey = "LdapUri";
		public static readonly string ProxyDNKey = "ProxyDN";

		public static readonly string DomainSection = "Domain";
		public static readonly string SimiasAdminDNKey = "AdminDN";

		private static readonly string LdapSystemBookSection = "LdapProvider";
		private static readonly string SearchKey = "Search";
		public static readonly string XmlContextTag = "Context";
		private static readonly string XmlDNAttr = "dn";
		private static readonly string NamingAttributeKey = "NamingAttribute";


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

        private string proxy = String.Empty;
		private string password = String.Empty;
		private string simiasAdmin = String.Empty;

		private XmlElement searchElement;

		private ArrayList searchContexts = new ArrayList();
		private string namingAttribute = DefaultNamingAttribute;
    
		#region Properties
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

		public string Scheme
		{
			get { return (this.scheme); }
			set
			{
				this.scheme = value;
				settingChangeMap |= ChangeMap.scheme;
			}
		}

        public string Host
        {
            get { return (this.host); }
            set
			{
                this.host = value;
                settingChangeMap |= ChangeMap.host;
            }
        }

        public bool SSL
        {
            get { return ( this.Scheme.Equals( UriSchemeLdaps ) ? true : false ); }
            set { this.Scheme = value ? UriSchemeLdaps : UriSchemeLdap; }
        }

        public int Port
        {
			get { return (this.port); }
            set
			{
                this.port = value;
                settingChangeMap |= ChangeMap.port;
            }
        }

        public string ProxyDN
        {
            get { return ( this.proxy ); }
            set
			{
                this.proxy = value;
                settingChangeMap |= ChangeMap.proxy;
            }
        }

		public string ProxyPassword
		{
			get { return ( this.password ); }
			set
			{
				this.password = value;
				settingChangeMap |= ChangeMap.password;
			}
		}

		public string AdminDN
		{
			get { return ( this.simiasAdmin ); }
		}

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

		public string NamingAttribute
		{
			get { return ( this.namingAttribute ); }
        		set
			{
        	        this.namingAttribute = value;
       	        	settingChangeMap |= ChangeMap.namingAttribute;
	    		}
		}
		#endregion

		#region Constructors
        private LdapSettings()
        {
			Configuration config = Store.Config;
            settingChangeMap = 0;

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
		#endregion

		#region Private Methods
		private string GetProxyPasswordFromFile()
		{
			string proxyPassword = String.Empty;
			string ppfPath = Path.Combine( Store.StorePath, ProxyPasswordFile );
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

		private void SetProxyPasswordInFile()
		{
			string ppfPath = Path.Combine( Store.StorePath, ProxyPasswordFile );
			using ( StreamWriter sw = File.CreateText( ppfPath ) )
			{
				sw.WriteLine( password );
			}
		}

		private bool SetConfigValue( XmlDocument document, string section, string key, string configValue )
		{
			bool status = false;

			string str = string.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key );
			XmlElement element = ( XmlElement )document.DocumentElement.SelectSingleNode( str );
			if ( element != null )
			{
				element.SetAttribute( ValueAttr, configValue );
				status = true;
			}

			return status;
		}

		private XmlElement GetSearchElement( XmlDocument document )
		{
			string str = String.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, LdapSystemBookSection, SettingTag, SearchKey );
			return ( XmlElement )document.DocumentElement.SelectSingleNode( str );
		}
		#endregion

		#region Public Methods
        public static LdapSettings Get()
        {
            return ( new LdapSettings( ) );
        }

		public void Commit()
		{
			if (settingChangeMap != 0)
			{
				// Build a path to the Simias.config file.
				string configFilePath = 
					Path.Combine( Store.StorePath, Simias.Configuration.DefaultConfigFileName );

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
		#endregion
    }
}


