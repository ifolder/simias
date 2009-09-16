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
*                 $Modified by: Kalidas Balakrishnan 
*                 $Mod Date: 19-12-2007
*                 $Revision: 0.2
*-----------------------------------------------------------------------------
* This module is used to:
*        < iFolder Simias server setup class >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.LdapProvider;
using Novell.iFolder;
using Novell.iFolder.Utility;

namespace Novell.iFolder
{
	/// <summary>
	/// Simias Server Setup
	/// </summary>
	class SimiasServerSetup
	{
		#region Static Fields

		// environment variables
		private static readonly string SIMIAS_SYSTEM_ADMIN_PASSWORD = "SIMIAS_SYSTEM_ADMIN_PASSWORD";
		private static readonly string SIMIAS_LDAP_PROXY_PASSWORD = "SIMIAS_LDAP_PROXY_PASSWORD";
		private static readonly string SIMIAS_LDAP_ADMIN_PASSWORD = "SIMIAS_LDAP_ADMIN_PASSWORD";
		private static readonly string SIMIAS_UPGRADE = "UPGRADE";

		// Configuration file xml tags
		private static readonly string SectionTag = "section";
		private static readonly string SettingTag = "setting";
		private static readonly string NameAttr = "name";
		private static readonly string ValueAttr = "value";

		private static readonly string Log4NetFile = "Simias.log4net";
		private static readonly string ModulesDir = "modules";
		private static readonly string LdapModule = "IdentityManagement.conf";
		private static readonly string UserMoveModule = "UserMovement.conf";

		private static string ServerSection = "Server";
		private static string IdentitySection = "Identity";
		private static string LdapPluginAssembly = "ServiceAssembly";
		private static string ServerNameKey = "Name";
		private static string MasterAddressKey = "MasterAddress";
		private static string PublicAddressKey = "PublicAddress";
		private static string PrivateAddressKey = "PrivateAddress";
		private static string oldConfigPath = "/var/lib/wwwrun/.local/share/simias/";

		private static string TemplateScriptFile = "simias-server";

	        //Invalid Character List.
		public static char[] InvalidChars = {'\\', ':', '*', '?', '\"', '<', '>', '|', ' '};

		#endregion
		protected ArrayList masterSearch = null;

		#region Member Fields
#if MONO
                string webPath = Path.GetFullPath("../lib/simias/web");
#endif

		/// <summary>
		/// Command Arguments
		/// </summary>
		string[] args;

		/// <summary>
		/// The uri to the ldap server.
		/// </summary>
		Uri ldapUrl;

		bool usingLDAP = false;

		string NonSsl = "NONSSL";

        /// <summary>
		/// BOTH option for setting simias server communication method 
		/// </summary>
		string Both = "BOTH";

		int UpgradeFrom = 0;

		/// <summary>
		/// Is Ldap admin value set from command line
		/// </summary>
		bool isLdapAdminSet = false;

		/// <summary>
		/// Is Ldap proxy dn value set from command line
		/// </summary>
		bool isLdapProxySet = false; 

		/// <summary>
		/// Is Ldap admin password value set from command line
		/// </summary>
		bool isLdapAdminPasswordSet = false;

		string storePath;
		string raPath;

		/// <summary>
		/// The path to the directory where the
		/// config file lives.
		/// </summary>
		string configPath;
		
		/// <summary>
		/// The path to the config file.
		/// </summary>
		string configFilePath;

		string domainId;
		System.Net.NetworkCredential credentials;

		bool possibly_yast = false;
		
		#endregion

		#region Options

		/// <summary>
		/// The store path.
		/// </summary>
		public Option path = new Option("path,p", "Server's Data Path", "Path to the server's data files", true, "/var/simias/data");

/*		/// <summary>
		/// The default configuration path.
		/// </summary>
		public Option defaultConfigPath = new Option("default-config-path,c", "Default Configuration Path", "Path to the default configuration files", false, null); 
*/
		/// <summary>
		/// The name of this server.
		/// </summary>
		public Option serverName = new Option("server-name", "Server Name", "The name of this server", true, System.Net.Dns.GetHostName());
	
		/// <summary>
		/// Use SSL;
		/// </summary>
        public Option useSsl = new Option("use-ssl", "SSL", "Select SSL/NONSSL communication for this server. Options are  SSL, NONSSL or BOTH", true, "SSL");
	
		/// <summary>
		/// The public address or name for this server.
		/// </summary>
		public Option publicUrl = new Option("public-url", "Public URL", "Public URL of this Simias Server", true, null);
		
		/// <summary>
		/// The Private address or name for this server.
		/// </summary>
		public Option privateUrl = new Option("private-url", "Private URL", "Private URL of this Simias Server", true, null);

		/// <summary>
		/// The port to listen on.
		/// </summary>
		public NoPromptOption port = new NoPromptOption("listen-port", "Listen Port", "The port to listen on", false, null);

		
		/// <summary>
		/// Slave Server.
		/// </summary>
		public BoolOption slaveServer = new BoolOption("slave-server,ssl", "Slave Server", "Install into existing Simias Domain", false, false);
		
		/// <summary>
		/// The master server address.
		/// </summary>
		public Option masterAddress = new Option("master-address,ma", "Master Server Private URL", "Private URL of Master Server", false, null);

		/// <summary>
		/// Display Help
		/// </summary>
		public NoPromptOption help = new NoPromptOption("help,?", "Usage Help", "Show This Screen", false, null);

		/// <summary>
		/// Simias System Name
		/// </summary>
		public Option systemName = new Option("system-name", "System Name", "A name used to identify the Simias system to users.", true, "Simias System");

		/// <summary>
		/// Simias System Description
		/// </summary>
		public Option systemDescription = new Option("system-description", "System Description", "A detailed description of the Simias system for users.", false, "Simias Enterprise Server");

		/// <summary>
		/// Use LDAP
		/// </summary>
		public BoolOption useRA = new BoolOption("use-recovery-agent", "Use Key Recovery Agent", "Use Key Recovery Agents to recovery the encryption key if the user forgets the pass-phrase used for encryption?", false, true);

		/// <summary>
		/// LDAP URL
		/// </summary>
		public Option recoveryAgentCertificatePath = new Option("ra-cert-path", "Recovery Agent Certificate Path", "Path to the Recovery agent certificate's.", false, "/var/simias/data");

		/// <summary>
		/// Use LDAP
		/// </summary>
		public BoolOption useLdap = new BoolOption("use-ldap", "Use LDAP", "Use LDAP to provision and authenticate users?", false, true);

		/// <summary>
		/// LDAP URL
		/// </summary>
		public Option ldapServer = new Option("ldap-server", "LDAP Server", "The host or ip address of an LDAP server.  The server will be searched for users to provision into Simias and will be used by Simias for authentication.", false, null);

		/// <summary>
		/// LDAP Secure
		/// </summary>
		public BoolOption secure = new BoolOption("ldap-ssl", "LDAP Secure", "Require a secure connection between the LDAP server and the Simias server", false, true);

		/// <summary>
		/// LDAP Admin DN
		/// </summary>
		public Option ldapAdminDN = new Option("ldap-admin-dn", "LDAP Admin DN", "An existing LDAP user, used by this script only, to connect to the LDAP server and create and/or check required LDAP users for Simias.", false, "cn=admin,o=novell");

		/// <summary>
		/// LDAP Admin Password
		/// </summary>
		public Option ldapAdminPassword = new Option("ldap-admin-password", "LDAP Admin Password", null, false, "novell");

		/// <summary>
		/// System Admin DN
		/// </summary>
		public Option systemAdminDN = new Option("system-admin-dn", "System Admin", "The Simias default administrator.  If the system is configured to use an external identity source, the distinguished name (dn) should be used.", true, "cn=admin,o=novell");

		/// <summary>
		/// System Admin Password
		/// </summary>
		public Option systemAdminPassword = new Option("system-admin-password", "System Admin Password", null, true, "novell");

		/// <summary>
		/// LDAP Proxy DN
		/// </summary>
		public Option ldapProxyDN = new Option("ldap-proxy-dn", "LDAP Proxy DN", "An LDAP user that will be used to provision the users between Simias and the LDAP server.  If this user does not already exist in the LDAP tree it will be created and granted read rights at the root of the tree. The user's dn and password are stored by Simias.", false, "cn=SimiasProxy,o=novell");

		/// <summary>
		/// LDAP Proxy Password
		/// </summary>
		public Option ldapProxyPassword = new Option("ldap-proxy-password", "LDAP Proxy Password", null, false, "novell");

		/// <summary>
		/// LDAP Search Context
		/// </summary>
		public Option ldapSearchContext = new Option("ldap-search-context", "LDAP Search Context", "A list of LDAP tree contexts (delimited by '#') that will be searched for users to provision into Simias.", false, "o=novell");

		/// <summary>
		/// Login Type based on what attribute
		/// </summary>
		public Option namingAttribute = new Option("naming-attribute", "Naming Attribute", "The LDAP attribute you want all users to login using.  I.E. 'cn' or 'email'.", false, "cn");

		/// <summary>
		/// Use apache.
		/// </summary>
		public BoolOption apache = new BoolOption("apache", "Configure Apache", "Configure Simias to run behind Apache", false, false);

		/// <summary>
                /// LDAP groups plugin configure option
                /// </summary>
                public BoolOption ldapPlugin = new BoolOption("ldap-plugin", "Ldap Groups Plugin", "Configure Ldap Groups Plugin", false, false);

		/// <summary>
                /// Plugin option for User Movement accross servers
                /// </summary>
                public NoPromptOption usermovePlugin = new NoPromptOption("usermove-plugin", "Plugin for User Movement accross servers", "Configure User Movement Plugin", false, "true");

                /// <summary>
                /// Extend iFolder LDAP schema
                /// </summary>
                public NoPromptOption extendSchema = new NoPromptOption("extend-schema", "Extend iFolder LDAP schema", "Extend iFolder LDAP schema", false, null);

		/// <summary>
		/// Apache User.
		/// </summary>
		public Option apacheUser = new Option("apache-user", "Apache User", "Apache User", false, "wwwrun");

		/// <summary>
		/// Apache Group.
		/// </summary>
		public Option apacheGroup = new Option("apache-group", "Apache Group", "Apache Group", false, "www");

		/// <summary>
		/// Prompt for options.
		/// </summary>
		public NoPromptOption prompt = new NoPromptOption("prompt", "Prompt For Options", "Prompt the user for missing options", false, null);

		/// <summary>
		/// Upgrade 3.2
		/// </summary>
		public NoPromptOption upgrade = new NoPromptOption("upgrade,ug", "Upgrade from 3.2 or older version of iFolder", "Auto Upgrade the store", false, null);

		/// <summary>
                /// Migrate from 3.2
                /// </summary>
                public NoPromptOption migrate = new NoPromptOption("migrate", "Migrate from 3.2 or older version of iFolder", "Migrate the store", false, null);

		/// <summary>
		/// Update Ldap setting
		/// </summary>
		public NoPromptOption updateLdap = new NoPromptOption("updateLdap,ul", "Change LDAP Settings ", "Change LDAP releared settings to reflect in store and LDAP", false, null);

		/// <summary>
		/// Remove Slave server entry from masters domain
		/// </summary>
		public NoPromptOption  remove = new NoPromptOption("remove", "Remove the slave server entry from masters domain", "Remove the slave server", false, null);
                /// <summary>
                /// Configure plugin options
                /// </summary>
                public NoPromptOption configurePlugins = new NoPromptOption("configure-plugins", "Configure Server Plugins", "Configure Server Plugins", false, null);
                 /// <summary>
                /// Set Domain ID for Domain creation
                /// </summary>
                public NoPromptOption domainID = new NoPromptOption("domain-id", "Set Domain ID", "Create the Master server with a specified Domain ID", false, null);
                /// <summary>
                /// LDAP certificate acceptance options - for now we prompt only when required (YaST configure issue)
                /// </summary>
		public BoolOption ldapCertAcc = new BoolOption("ldap-cert-acceptance", "Accept LDAP Certificate", null, false, true);


		#endregion

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="cmdArgs">Command Arguments</param>
		SimiasServerSetup(string[] cmdArgs)
		{
			args = cmdArgs;
#if WINDOWS
				// On windows we do not want to prompt for these values.
				apache.Value = false;
#endif

			path.OnOptionEntered = new Option.OptionEnteredHandler( OnPath );
//			defaultConfigPath.OnOptionEntered = new Option.OptionEnteredHandler( OnDefaultConfig );

			serverName.OnOptionEntered = new Option.OptionEnteredHandler( OnServerName );
			useSsl.OnOptionEntered = new Option.OptionEnteredHandler( OnUseSSL );
			slaveServer.OnOptionEntered = new Option.OptionEnteredHandler( OnSlave );
			publicUrl.OnOptionEntered = new Option.OptionEnteredHandler( OnPublicUrl );
			privateUrl.OnOptionEntered = new Option.OptionEnteredHandler( OnPrivateUrl );
			masterAddress.OnOptionEntered = new Option.OptionEnteredHandler( OnMasterAddress );
			useLdap.OnOptionEntered = new Option.OptionEnteredHandler( OnLdap );
			useRA.OnOptionEntered = new Option.OptionEnteredHandler( OnRA );
			recoveryAgentCertificatePath.OnOptionEntered = new Option.OptionEnteredHandler( OnRAPath );
			apache.OnOptionEntered = new Option.OptionEnteredHandler ( OnApache );
			ldapAdminDN.OnOptionEntered = new Option.OptionEnteredHandler ( OnldapAdminDN );
			ldapProxyDN.OnOptionEntered = new Option.OptionEnteredHandler ( OnldapProxyDN );
			ldapAdminPassword.OnOptionEntered = new Option.OptionEnteredHandler ( OnldapAdminPassword );
			upgrade.OnOptionEntered = new Option.OptionEnteredHandler ( OnUpgrade );
			migrate.OnOptionEntered = new Option.OptionEnteredHandler ( OnMigrate );
			remove.OnOptionEntered = new Option.OptionEnteredHandler ( OnRemove );
			port.OnOptionEntered = new Option.OptionEnteredHandler ( OnPort );
			configurePlugins.OnOptionEntered = new Option.OptionEnteredHandler ( OnConfigurePlugins );
			updateLdap.OnOptionEntered = new Option.OptionEnteredHandler ( OnUpdateLdap );
		}

		#endregion

		#region Option Handlers
        /// <summary>
        /// To handle of SSL option is selected
        /// </summary>
        /// <returns>true if changes are set successfully</returns>
		private bool OnUseSSL()
		{
			bool enableSSL = String.Compare(useSsl.Value, NonSsl, true) == 0 ? false : true ;
			string scheme = enableSSL?Uri.UriSchemeHttps:Uri.UriSchemeHttp;
                       	System.Net.IPHostEntry hostInfo = System.Net.Dns.GetHostByName( System.Net.Dns.GetHostName() );
			if(publicUrl.Assigned)
			{
				UriBuilder urlBild = new UriBuilder(publicUrl.Value);
                                urlBild.Scheme = enableSSL?Uri.UriSchemeHttps:Uri.UriSchemeHttp;
                                privateUrl.DefaultValue = publicUrl.InternalValue = AddVirtualPath( urlBild.ToString());
			}
			else
			{
                        	Uri pubUrl = new Uri( scheme + Uri.SchemeDelimiter + hostInfo.AddressList[0].ToString() + "/simias10" );
                        	publicUrl.DefaultValue = pubUrl.ToString();
			}

			if(privateUrl.Assigned)
			{
				UriBuilder urlBild = new UriBuilder(privateUrl.Value);
                                urlBild.Scheme = enableSSL?Uri.UriSchemeHttps:Uri.UriSchemeHttp;
                                privateUrl.InternalValue = AddVirtualPath( urlBild.ToString() );
			}

                        ldapServer.DefaultValue = hostInfo.AddressList[0].ToString();
			return true;
		}

        /// <summary>
        /// to check if ldap proxy DN is present
        /// </summary>
        /// <returns>true if ldapproxy Dn is present</returns>
		private bool OnldapProxyDN()
		{
			if ( ldapProxyDN.Value  != null)
				isLdapProxySet = true;
			return true;
		}
        /// <summary>
        /// check if ldap admin DN is present
        /// </summary>
        /// <returns>true if ldap admin DN is present</returns>
		private bool OnldapAdminDN()
		{
			if ( ldapAdminDN.Value  != null)
				isLdapAdminSet = true;
			return true;
		}

        /// <summary>
        /// check if ldap admin password is present
        /// </summary>
        /// <returns>true if ldapadmin password field is non-null</returns>
		private bool OnldapAdminPassword()
		{
			if ( ldapAdminPassword.Value  != null)
				isLdapAdminPasswordSet = true;
			return true;
		}

        /// <summary>
        /// check whether store path exists or not and take appropriate action
        /// </summary>
        /// <returns>true if path exists and updation by default value is done</returns>
		private bool OnPath()
		{
			storePath = Path.GetFullPath( path.Value );
			if ( Path.GetFileName( storePath ) != "simias" )
			{
				storePath = Path.Combine( storePath, "simias" );
			}

			// Check if a configuration file exists in the store path location
			if ( System.IO.Directory.Exists( storePath ) == true )
			{
				if ( File.Exists( Path.Combine( storePath, Simias.Configuration.DefaultConfigFileName ) ) == true )
				{
					configFilePath = Path.Combine( storePath, Simias.Configuration.DefaultConfigFileName );
					configPath = storePath;

					UpdateDefaults();

//					defaultConfigPath.Prompt = false;
//					defaultConfigPath.Required = false;

					return true;
				}
			}

			bool remove_slave = remove.Value != null? Boolean.Parse(remove.Value) : false;
			if(remove_slave)
				return true;

			// Check if a default Simias.config exists in the normal 
			// specified areas
			if ( SetupDefaultConfigPath() == true )
			{
//				defaultConfigPath.Prompt = false;
//				defaultConfigPath.Required = false;

				SetupConfigFiles();
				UpdateDefaults();
			}

			return true;
		}

        /// <summary>
        /// check whether server name can be prompted or not
        /// </summary>
        /// <returns>true if servername prompt can be shown</returns>
		private bool OnServerName()
		{
		    //Check For invalid characters
		        if (serverName.Value.IndexOfAny(InvalidChars) == -1 ? false : true )
			{
			        if (!Prompt.CanPrompt)
				{
				    throw new Exception ("Server Name contains invalid characters");
				}

			        Console.WriteLine ("ServerName contains invalid characters. Please re-enter Server Name");

			        serverName.Assigned = false;
				serverName.Prompt = true;

				Prompt.ForOption (serverName);
				return true;
			}
			return true;
		}

/*		private bool OnDefaultConfig()
		{
			configPath = Path.GetFullPath( defaultConfigPath.Value );
			if ( System.IO.Directory.Exists( configPath ) == true )
			{
				if ( File.Exists( Path.Combine( configPath, Simias.Configuration.DefaultConfigFileName ) ) == true )
				{
					configFilePath = Path.Combine( configPath, Simias.Configuration.DefaultConfigFileName );

					SetupConfigFiles();
					UpdateDefaults();
					return true;
				}
			}

			return false;
		}
*/

        /// <summary>
        /// if useldap value is true then ask for ldap attributes
        /// </summary>
        /// <returns>true if useldap is true</returns>
		private bool OnLdap()
		{
			if ( !useLdap.Value )
			{
				usingLDAP = ldapServer.Prompt = ldapPlugin.Prompt = secure.Prompt = ldapAdminDN.Prompt =
					ldapAdminPassword.Prompt = ldapProxyDN.Prompt =
					ldapProxyPassword.Prompt = ldapSearchContext.Prompt = 
					namingAttribute.Prompt = false;
			} else {
			        usingLDAP = true;
			}

			return true;
		}

        /// <summary>
        /// whether use RA is specified or not
        /// </summary>
        /// <returns>true if asked to use RA</returns>
		private bool OnRA()
		{
			if ( !useRA.Value )
			{
				recoveryAgentCertificatePath.Prompt = false;
			} 
			
			return true;
		}

        /// <summary>
        /// if RA path is given, then it is called
        /// </summary>
        /// <returns>if the given path exists, return true</returns>
		private bool OnRAPath()
		{
			raPath = Path.GetFullPath( recoveryAgentCertificatePath.Value );
			return System.IO.Directory.Exists( raPath );
		}

        /// <summary>
        /// whether this server is slave
        /// </summary>
        /// <returns>true if slave, and ask for accepting ldap certificate</returns>
		private bool OnSlave()
		{
			if ( !slaveServer.Value )
			{
				masterAddress.Prompt = false;
			}
			else
			{
				// LDAP information will be read from the master.
				usingLDAP = ldapServer.Prompt = secure.Prompt = ldapAdminDN.Prompt =
					ldapProxyDN.Prompt = ldapSearchContext.Prompt = 
					ldapProxyPassword.Prompt = namingAttribute.Prompt = false;

				bool remove_slave = remove.Value != null? Boolean.Parse(remove.Value) : false;
				if(remove_slave)
					return true;
				usingLDAP= ldapServer.Prompt = ldapPlugin.Prompt = secure.Prompt = ldapProxyDN.Prompt = ldapSearchContext.Prompt = true;
				Console.WriteLine ("onslave : setting to true");

			}
			ldapCertAcc.Prompt = false;
			return true;
		}

        /// <summary>
        /// prompting for using apache
        /// </summary>
        /// <returns>true if using apache was selected</returns>
		private bool OnApache()
		{
			if ( !apache.Value )
			{
				apacheUser.Prompt = apacheGroup.Prompt = false;
			}
			return true;
		}

        /// <summary>
        /// called during public URL selection
        /// </summary>
        /// <returns>true if public url is set successfully</returns>
		private bool OnPublicUrl()
		{
			bool enableSSL = String.Compare(useSsl.Value, NonSsl, true) == 0 ? false : true ;
			//privateUrl.DefaultValue = publicUrl.Value;
			if (!publicUrl.Value.ToLower().StartsWith(Uri.UriSchemeHttp))
			{
				publicUrl.InternalValue = (new UriBuilder(enableSSL?Uri.UriSchemeHttps:Uri.UriSchemeHttp, publicUrl.Value)).ToString();
			}

			if(port.Value != null)
			{
				UriBuilder urlBild = new UriBuilder(publicUrl.Value);
				urlBild.Scheme = enableSSL?Uri.UriSchemeHttps:Uri.UriSchemeHttp; 
				urlBild.Port = Convert.ToInt32(port.Value);
				privateUrl.DefaultValue = publicUrl.InternalValue = AddVirtualPath( urlBild.ToString());
			}
			else
				privateUrl.DefaultValue = publicUrl.InternalValue = AddVirtualPath( publicUrl.Value );
			Uri pubUri = new Uri( publicUrl.Value );

/*			if ( string.Compare( pubUri.Scheme, Uri.UriSchemeHttps, true ) == 0 )
				useSsl.Value = true.ToString();
			else
				useSsl.Value = false.ToString();*/
			if(port.Value == null)
				port.DefaultValue = pubUri.Port.ToString();
			return true;
		}

        /// <summary>
        /// called during prompt for private url
        /// </summary>
        /// <returns>true if private url set successfully</returns>
		private bool OnPrivateUrl()
		{
			bool enableSSL = String.Compare(useSsl.Value, NonSsl, true) == 0 ? false : true ;
			if (!privateUrl.Value.ToLower().StartsWith(Uri.UriSchemeHttp))
			{
					privateUrl.InternalValue = (new UriBuilder(enableSSL?Uri.UriSchemeHttps:Uri.UriSchemeHttp, privateUrl.Value)).ToString();
			}
			if(port.Value != null && port.Assigned == true)
			{
				UriBuilder urlBild = new UriBuilder(privateUrl.Value);
				urlBild.Scheme = enableSSL?Uri.UriSchemeHttps:Uri.UriSchemeHttp; 
				urlBild.Port = Convert.ToInt32(port.Value);
				privateUrl.InternalValue = AddVirtualPath( urlBild.ToString() );
			Console.WriteLine("Private {0}\n\n\n", privateUrl.Value);
			}
			else
				privateUrl.InternalValue = AddVirtualPath( privateUrl.Value );
			return true;
		}

        /// <summary>
        /// called during prompt for master address
        /// </summary>
        /// <returns>true if all parameters set successfully, false if master address was null</returns>
		private bool OnMasterAddress()
		{
			// Don't prompt for the following options. They are not needed 
			// or will be obtained from the master.
			// system
			if(masterAddress.Value == null)
			{
				masterAddress.Assigned = false;
				masterAddress.Prompt = true;
				Console.WriteLine("Master server's URL can not be null, Enter Master server's URL.");
				Prompt.ForOption(masterAddress);
				return false;
			}

			masterAddress.InternalValue = AddVirtualPath( masterAddress.Value );
			
			systemName.Prompt = false;
			systemName.Required = false;

			systemDescription.Prompt = false;
			systemDescription.Required = false;
	
			useLdap.Prompt = false;
			useLdap.Required = false;

			// ldap uri
			ldapServer.Prompt = false;
			ldapServer.Required = false;

			secure.Prompt = false;
			secure.Required = false;

			// naming Attribute
			namingAttribute.Prompt = false;
			namingAttribute.Required = false;

			// ldap proxy dn
			ldapProxyDN.Prompt = false;
			ldapProxyDN.Required = false;
				
			// ldap proxy password
			ldapProxyPassword.Prompt = false;
			ldapProxyPassword.Required = false;

			ldapSearchContext.Prompt = false;
			ldapSearchContext.Required = false;
				
			//Don't prompt for the following option.
			ldapAdminDN.Prompt = false;
			ldapAdminDN.Required = false;

			ldapAdminPassword.Prompt = false;
			ldapAdminPassword.Required = false;

			return true;
		}
		#endregion

        /// <summary>
        /// add simias10 to given path
        /// </summary>
        /// <param name="path">path which is to be added</param>
        /// <returns>newly formed string</returns>
		private string AddVirtualPath( string path )
		{
			path = path.TrimEnd( '/' );
			if ( path.EndsWith( "/simias10" ) == false )
			{
				path += "/simias10";
			}

			return path;
		}

        /// <summary>
        /// get the hostadmin service object
        /// </summary>
        /// <returns>an object of type HostAdmin</returns>
		private HostAdmin GetHostAdminService()
		{
			HostAdmin adminService = new HostAdmin();
			InitializeServiceUrl( adminService );
			return adminService;
		}

        /// <summary>
        /// Initialize the service URL
        /// </summary>
        /// <param name="service">service objects with all fields set</param>
		private void InitializeServiceUrl( System.Web.Services.Protocols.WebClientProtocol service )
		{
			UriBuilder serverUrl = new UriBuilder( service.Url );
			Uri masterUri = new Uri(masterAddress.Value);
			serverUrl.Scheme = masterUri.Scheme;
			serverUrl.Host = masterUri.Host;
			serverUrl.Port = masterUri.Port;
			string target = service.Url.Substring( service.Url.LastIndexOf( '/' ) );
			serverUrl.Path = masterUri.AbsolutePath + target;
			service.Url = serverUrl.ToString();
Console.WriteLine("Url {0}", service.Url);
		}

        /// <summary>
        /// setup the ssl
        /// </summary>
		void SetupSSL()
		{
                        if (masterAddress.Value.ToLower().StartsWith(Uri.UriSchemeHttps))
                        {
				SetupLdapCert();
                                // swap policy
                                //ICertificatePolicy policy = ServicePointManager.CertificatePolicy;
                                ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();

                                // connect
                                HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(masterAddress.Value);

                                try
                                {
                                        request.GetResponse();
                                }
                                catch
                                {
                                        // ignore
                                }

				string MachineArch = Environment.GetEnvironmentVariable("OS_ARCH");
                                webPath =( MachineArch == null )? Path.GetFullPath("../lib/simias/web"): Path.GetFullPath("../lib64/simias/web");

                                // restore policy
                               // ServicePointManager.CertificatePolicy = policy;

                                // service point
                                ServicePoint sp = request.ServicePoint;
                                if ((sp != null) && (sp.Certificate != null))
                                {
                                        string path = Path.GetFullPath(Path.Combine(webPath, "web.config"));
                                        string certRawDetail = Convert.ToBase64String(sp.Certificate.GetRawCertData());
                                        string certDetail = sp.Certificate.ToString(true);

                                        XmlDocument doc = new XmlDocument();

                                        doc.Load(path);

                                        XmlElement cert = (XmlElement)doc.DocumentElement.SelectSingleNode("//configuration/appSettings/add[@key='SimiasCert']");

                                        BoolOption iFolderCertAcc = new BoolOption("iFolder-cert-acceptance", "Accept iFolder Master Server Certificate", certDetail, false, true);
					if(possibly_yast == true)
						iFolderCertAcc.Value = true;
					else
					{
                                        	iFolderCertAcc.Prompt = true;
                                        	Prompt.ForOption(iFolderCertAcc);
					}

                                        if (iFolderCertAcc.Value == true )
                                        {
                                                if (cert != null)
                                                {
                                                        cert.Attributes["value"].Value = certRawDetail;

                                                        doc.Save(path);

                                                        Console.WriteLine("Done");
                                                }
                                                else
                                                {
                                                        throw new Exception(String.Format("Unable to find \"SimiasCert\" tag in the {0} file.", path));
                                                }

                                        }
                                        else
                                        {
                                                Console.WriteLine("Failed (Install Certificate)");
                                                throw new Exception( string.Format( "User Certificate validation failed " ) );
                                        }

                                }
                                else
                                {
                                        throw new Exception("Unable to retrieve the certificate from the iFolder server.");
                                }

                        }

		}


		/// <summary>
		/// Initialize
		/// Read /etc/apache2/uid.conf, grab the apache user or group.
		/// </summary>
		void Initialize()
		{
			// find user
			try
			{
				// uid.conf
				using( TextReader reader = (TextReader)File.OpenText( Path.GetFullPath( "/etc/apache2/uid.conf" ) ) )
				{
					string line;
					while( ( line = reader.ReadLine() ) != null )
					{
						if ( line.StartsWith( "User" ) )
						{
							apacheUser.Value = line.Split()[1];
						}
						else if ( line.StartsWith( "Group" ) )
						{
							apacheGroup.Value = line.Split()[1];
						}
					}
				}
			}
			catch ( Exception )
			{
				// Failed. Prompt for apache user & group.
			        apacheUser.Prompt = apacheGroup.Prompt = true;
			}

		}
    		/// <summary>
		/// Change the ownership of web.config to apache user so that iFolder
		/// server can chnage the values while running.
		/// </summary>
		void UpdateOwnership()
		{
			string MachineArch = Environment.GetEnvironmentVariable("OS_ARCH");
			string webpath = (MachineArch == null) ? Path.GetFullPath("../lib/simias/web"): Path.GetFullPath("../lib64/simias/web");			
			string webconfigfile = Path.Combine(webpath, "web.config"); 

			if (Execute("chown", "{0}:{1} {2}", apacheUser.Value, apacheGroup.Value, webconfigfile) != 0)
			{
				Console.WriteLine("Unable to set an owner for the log path.");
			}
		}

        /// <summary>
        /// configure the plugins
        /// </summary>
        /// <returns>true if configured successfully</returns>
		bool Configure()
		{
			ParseArguments();
			bool updateldp = updateLdap.Value != null? Boolean.Parse(updateLdap.Value) : false;

			if(configurePlugins.Value != null && Boolean.Parse(configurePlugins.Value) == true )
				return ConfigurePlugins(); 

			if(updateldp == true)
			{
				return ChangeLdapSettings();
			}

			if ( SetupSimias() == false )
			{
				return false;
			}

			if( migrate.Assigned)
			{
				Console.WriteLine("Our changes start from here...");
				isLdapAdminSet = true;
				isLdapAdminPasswordSet = true;
				SetupLdap();
				SetupScriptFiles();

				string destModulesDir = Path.Combine( storePath, ModulesDir );
                                File.Delete( Path.Combine( destModulesDir, UserMoveModule ) );

                                destModulesDir = Path.Combine( storePath, ModulesDir );
                                File.Delete( Path.Combine( destModulesDir, LdapModule ) );

				Console.WriteLine("Out of Our changes start from here...");

				return true;
			}

			bool remove_slave = remove.Value != null? Boolean.Parse(remove.Value) : false;
			if(remove_slave)
				return true;

			try
			{
				SetupModMono();
				UpdateOwnership();
				if ( usingLDAP )
				{
					SetupLdap();
				}
				if(apache.Value == false)
				{
					SetupScriptFiles(); //not needed for OES as iFolder runs behind apache. Also helps rpm Uninstall.
				}
			} catch(Exception ex)
			{
				if ( slaveServer.Value )
					UnRegisterSlaveFromMaster(systemAdminDN.Value,serverName.Value);
				throw ex;
			}

			if(Boolean.Parse(usermovePlugin.Value) == true)
			{
				if(SetupUserMovePlugin() != true)
				{
					Console.WriteLine("User Move plugin is not configured..\n");
				}
			}
			else
			{
				string destModulesDir = Path.Combine( storePath, ModulesDir );
                               	File.Delete( Path.Combine( destModulesDir, UserMoveModule ) );
			}
			if(usingLDAP == true && ldapPlugin.Value == true)
			{
				if(SetupLdapPlugin() != true)
				{
					Console.WriteLine("Ldap plugin configuration failed, Exiting from simias server setup..\n");
					return false;
				}
			}
			else
			{
				string destModulesDir = Path.Combine( storePath, ModulesDir );
                               	File.Delete( Path.Combine( destModulesDir, LdapModule ) );
			}
			try
			{
				SetupLog4Net();
				SetupPermissions();
			} catch(Exception ex)
			{
				if ( slaveServer.Value )
					UnRegisterSlaveFromMaster(systemAdminDN.Value,serverName.Value);
				throw ex;
			}

			return true;
		}

		/// <summary>
		/// Setup All plugins 
		/// </summary>
        /// <returns>true if configured successfully</returns>
		bool ConfigurePlugins()
		{
			if(ldapServer.Value != null && String.Compare(ldapServer.Value,"localhost") != 0 && ldapPlugin.Value == true)
			{
				if(SetupLdapPlugin() != true)
				{
					Console.WriteLine("Ldap plugin configuration failed, Exiting from simias server setup..\n");
					return false;
				}
			}
			else if(ldapPlugin.Value == false)
			{
				string destModulesDir = Path.Combine( storePath, ModulesDir );
                               	File.Delete( Path.Combine( destModulesDir, LdapModule ) );
			}
			else if(ldapServer.Value == null || String.Compare(ldapServer.Value,"localhost") == 0 )
			{
				Console.WriteLine("Ldap plugin configuration failed, LDAP is not configured in the existing simias server setup..\n");
				return false;
			}

			if( Boolean.Parse(usermovePlugin.Value) == true )
			{
				if(SetupUserMovePlugin() != true)
				{
					Console.WriteLine("Failed to configure User Move plugin ..\n");
					return false;
				}
			}
			else
			{
				string destModulesDir = Path.Combine( storePath, ModulesDir );
                               	File.Delete( Path.Combine( destModulesDir, UserMoveModule ) );
			}
			Console.WriteLine("All plugins Configured successfully..\n");
			return true;
		}

		/// <summary>
		/// Setup User Move Plugin
		/// </summary>
		bool SetupUserMovePlugin()
		{
				Console.WriteLine("Configuring User Movement plugin..\n");
				string usermoveModuleConfigPath = String.Format( "{0}{1}{2}{3}{4}{5}{6}{7}{8}",
					SimiasSetup.sysconfdir,
                                        Path.DirectorySeparatorChar.ToString(),
                                        "simias",
					Path.DirectorySeparatorChar.ToString(),
					"bill",
					Path.DirectorySeparatorChar.ToString(),
					ModulesDir,
					Path.DirectorySeparatorChar.ToString(),
					UserMoveModule );


				if ( File.Exists( usermoveModuleConfigPath ) == false )
				{
					Console.WriteLine("Unable to find the User Move plugin configuration files..");
					Console.WriteLine("{0}", usermoveModuleConfigPath );
					Console.WriteLine("Please make sure, ifolder-enterprise-plugins rpm is installed.\n\n");
					return false;
				}
				string destModulesDir = Path.Combine( storePath, ModulesDir );
				if(System.IO.Directory.Exists( destModulesDir ) == false)
				{
					Console.WriteLine("{0} directory does not exist.. Configure iFolder server first with simias-server-setup.",destModulesDir);
					return false;
				}
				if ( File.Exists( Path.Combine( destModulesDir, UserMoveModule ) ) == false )
				{
                                	File.Copy(usermoveModuleConfigPath, Path.Combine( destModulesDir, UserMoveModule ));
                                        if( Execute( "chown", " -R {0}:{1} {2}", apacheUser.Value, apacheGroup.Value, Path.Combine(destModulesDir,UserMoveModule) ) != 0 )
                                                Console.WriteLine( "Unable to set an owner {0} for the store path.{1}", apacheUser.Value, 																Path.Combine(destModulesDir,UserMoveModule) );
				}
				return true;
		}


		/// <summary>
		/// Setup Ldap Plugin
		/// </summary>
		bool SetupLdapPlugin()
		{
                                UriBuilder newUri = new UriBuilder();
                                newUri.Host = ldapServer.Value;
                                newUri.Scheme = secure.Value ? LdapSettings.UriSchemeLdaps : LdapSettings.UriSchemeLdap;

				Console.WriteLine("Configuring Ldap plugin..\n");
				string baseInstallPath = String.Format( "{0}{1}{2}{3}",
                                        System.IO.Directory.GetCurrentDirectory(),
                                        Path.DirectorySeparatorChar.ToString(),
					"../..",
                                        Path.DirectorySeparatorChar.ToString()
					);
				string ldapModuleConfigPath = String.Format( "{0}{1}{2}{3}{4}{5}{6}{7}{8}",
					SimiasSetup.sysconfdir,
                                        Path.DirectorySeparatorChar.ToString(),
                                        "simias",
					Path.DirectorySeparatorChar.ToString(),
					"bill",
					Path.DirectorySeparatorChar.ToString(),
					ModulesDir,
					Path.DirectorySeparatorChar.ToString(),
					LdapModule );

				if ( File.Exists( ldapModuleConfigPath ) == false )
				{
					Console.WriteLine("Failed to find the Ldap plugin configuration files..");
					Console.WriteLine("Please make sure, ifolder-enterprise-plugins rpm is installed.\n\n");
					return false;
				}
				string destModulesDir = Path.Combine( storePath, ModulesDir );
				if(System.IO.Directory.Exists( destModulesDir ) == false)
				{
					Console.WriteLine("{0} directory does not exist.. Configure iFolder server with simias-server-setup.",destModulesDir);
					return false;
				}
				if ( File.Exists( Path.Combine( destModulesDir, LdapModule ) ) == false )
				{
                                	File.Copy( ldapModuleConfigPath, Path.Combine( destModulesDir, LdapModule ) );
                                        if( Execute( "chown", " -R {0}:{1} {2}", apacheUser.Value, apacheGroup.Value, Path.Combine(destModulesDir,LdapModule) ) != 0 )
                                                Console.WriteLine( "Unable to set an owner {0} for the store path.{1}", apacheUser.Value, 																Path.Combine(destModulesDir,LdapModule) );
				}
				string configFile = configFilePath;
				if ( File.Exists( Path.Combine( storePath, Configuration.DefaultConfigFileName ) ) == true )
				{
					configFile = Path.Combine( storePath, Configuration.DefaultConfigFileName );
				}
				else
				{
					Console.WriteLine("{0} config file does not exist..  Configure iFolder server with simias-server-setup.",Path.Combine( storePath, Configuration.DefaultConfigFileName ));
					return false;
				}
				XmlDocument document = new XmlDocument();
				document.Load( configFile );
				LdapSettings ldapSettings = LdapSettings.Get( storePath );
                                if ( ldapSettings.DirectoryType.Equals( LdapDirectoryType.eDirectory ) )
				{
					string LDAPProviderString = "Simias.Identity.LdapProvider";
				 	string ldapModuleSchemaPath = String.Format( "{0}{1}{2}{3}",
						baseInstallPath,
                                        	"etc",
                                        	Path.DirectorySeparatorChar.ToString(),
						"iFolderLdapPlugin.ldif"
                                        	);
					if(extendSchema.Value != null && Boolean.Parse(extendSchema.Value) == true )
					{
                                        	int Result = Execute("ldapmodify", "-x -Z -H {0} -D {1} -w {2} -f {3}",
                                             	newUri.ToString(),ldapAdminDN.Value,
                                            	ldapAdminPassword.Value, ldapModuleSchemaPath);

						if ( Result != 0 && Result != 20)
                                		{
							Console.WriteLine("Ldap schema Extension returned error..");
						}
					}
					SetConfigValue( document, IdentitySection, LdapPluginAssembly,LDAPProviderString);
	
					
				}
                                else if(ldapSettings.DirectoryType.Equals( LdapDirectoryType.ActiveDirectory ))
				{
					string LDAPProviderString = "Simias.Identity.ADLdapProvider";
					LdapUtility ldapUtility;

					newUri.Host = ldapServer.Value;
					newUri.Scheme = secure.Value ? LdapSettings.UriSchemeLdaps : LdapSettings.UriSchemeLdap;
					ldapUrl = new Uri(newUri.ToString());

			        	ldapUtility = new LdapUtility(ldapUrl.ToString() , ldapAdminDN.Value, ldapAdminPassword.Value);
                        		ldapUtility.Connect();
                        		ldapUtility.ExtendADiFolderschema();
					
					SetConfigValue( document, IdentitySection, LdapPluginAssembly,LDAPProviderString);
				}
                                else if(ldapSettings.DirectoryType.Equals( LdapDirectoryType.OpenLDAP ))
				{
					string LDAPProviderString = "Simias.Identity.OpenLdapProvider";
					SetConfigValue( document, IdentitySection, LdapPluginAssembly,LDAPProviderString);
				}
				CommitConfiguration( document );
				return true;
		}

		#region Arguments

		/// <summary>
		/// Parse the Command-Line Arguments
		/// </summary>
		void ParseArguments()
		{
			if ( args.Length == 0 )
			{
				// prompt
				Prompt.CanPrompt = true;
				prompt.Value = true.ToString();

				Console.WriteLine("This script configures a server installation of Simias to setup a new Simias system. ");

				PromptForArguments();
				possibly_yast = false;
			}
			else
			{
				// environment variables
				systemAdminPassword.FromEnvironment( SIMIAS_SYSTEM_ADMIN_PASSWORD );
				ldapProxyPassword.FromEnvironment(SIMIAS_LDAP_PROXY_PASSWORD);
				ldapAdminPassword.FromEnvironment(SIMIAS_LDAP_ADMIN_PASSWORD);
				upgrade.FromEnvironment(SIMIAS_UPGRADE);

				// parse arguments
				Options.ParseArguments( this, args );
				if(ldapCertAcc.Assigned && ldapCertAcc.Value == true)
					possibly_yast = true;

				// help
				if ( help.Assigned )
				{
					ShowUsage();
				}

				if(upgrade.Assigned)
				{
					Prompt.CanPrompt = true;
					PromptForArguments();
				}

				if(port.Assigned)
				{
					Prompt.CanPrompt = true;
					PromptForArguments();
				}

				if(updateLdap.Assigned)
				{
					Prompt.CanPrompt = true;
					PromptForArguments();
					return;
				}

				if(remove.Assigned)
				{
					Prompt.CanPrompt = true;
					PromptForArguments();
					return;
				}

				if(configurePlugins.Assigned)
				{
					Prompt.CanPrompt = true;
					PromptForArguments();
					return;
				}

				if ( prompt.Assigned )
				{
					Prompt.CanPrompt = true;
					PromptForArguments();
				}
				else
				{
#if DEBUG
					// show options for debugging
					Options.WriteOptions( this, Console.Out );
					Console.WriteLine();
#endif
					// check for required options
					Options.CheckRequiredOptions( this );
				}
			}

			if ( slaveServer.Value )
			{
				try
				{
					if (!masterAddress.Value.ToLower().StartsWith(Uri.UriSchemeHttp))
					{
						masterAddress.Value = (new UriBuilder(Uri.UriSchemeHttp, masterAddress.Value)).ToString();
					}
					else
						SetupSSL();

					// Get the Domain ID from the domain service on the master.
					DomainService dService = new DomainService();
                                        // Get the configuration file from the master server.
                                        string[] dnSegs = systemAdminDN.Value.Split(new char[] {',', '=', '.'});
                                        string admin = ( dnSegs.Length == 1 ) ? dnSegs[0] : dnSegs[1];
                                        credentials = new System.Net.NetworkCredential( admin, systemAdminPassword.Value, domainId);

					InitializeServiceUrl( dService );
					domainId = dService.GetDomainID();
				
					HostAdmin adminService = this.GetHostAdminService();
					adminService.Credentials = credentials;
					string configXml = adminService.GetConfiguration();
					// TODO: need to get the proxy password ... should we prompt for it?
//					ldapProxyPassword.Value = adminService.GetProxyInfo();
					XmlDocument configDoc = new XmlDocument();
					configDoc.LoadXml( configXml );
					CommitConfiguration( configDoc );
					GetSettingsFromConfig();
				}
				catch (Exception ex)
				{
					Console.WriteLine( "Failed to connect to master server, Slave setup failed.");
					Console.WriteLine( "Check iFolder server is running on master server.");
					Console.WriteLine("{0}.\n..{1}", ex.Message, ex.StackTrace);
					System.Environment.Exit( -1 );
				}
			}
		}

		/// <summary>
		/// Prompt for Arguments
		/// </summary>
		void PromptForArguments()
		{
			Console.WriteLine();

			Option[] options = Options.GetOptions( this );
			foreach( Option option in options )
			{
				Prompt.ForOption( option );
			}

			Console.WriteLine();
			Console.WriteLine( "Working..." );
			Console.WriteLine();
		}

        /// <summary>
        /// check for the config filename
        /// </summary>
        /// <returns>true if configured correctly</returns>
		private bool OnConfigurePlugins()
		{
			if( path.Assigned == false )
			{
				path.Value =  ReadModMonoConfiguration();
				if( path.Value == null )
				{
					Console.WriteLine("Unable to extract the DATA path. Exiting the simias-server-setup..\n");
					Environment.Exit(-1);
				}
				storePath = Path.GetFullPath( path.Value );
				if ( Path.GetFileName( storePath ) != "simias" )
				{
					storePath = Path.Combine( storePath, "simias" );
				}

				// Check if a configuration file exists in the store path location
				if ( System.IO.Directory.Exists( storePath ) == true )
				{
					if ( File.Exists( Path.Combine( storePath, Simias.Configuration.DefaultConfigFileName ) ) == false )
					{
						Console.WriteLine("Unable to {0} file.",Path.Combine( storePath, Simias.Configuration.DefaultConfigFileName ));
						Environment.Exit(-1);
					}
					configFilePath = Path.Combine( storePath, Simias.Configuration.DefaultConfigFileName );
					configPath = storePath;
				}
			}
			ldapPlugin.Prompt = true;
			useLdap.Prompt = systemName.Prompt = systemDescription.Prompt = systemAdminDN.Prompt =  publicUrl.Prompt = slaveServer.Prompt =
				ldapServer.Prompt = secure.Prompt = ldapProxyDN.Prompt = ldapSearchContext.Prompt = 
				namingAttribute.Prompt = privateUrl.Prompt = serverName.Prompt = useSsl.Prompt = 
				useRA.Prompt = recoveryAgentCertificatePath.Prompt = ldapAdminDN.Prompt = masterAddress.Prompt = 
				apache.Prompt = ldapAdminPassword.Prompt = ldapProxyPassword.Prompt = systemAdminPassword.Prompt = path.Prompt = false;
			return true;
		}

        /// <summary>
        /// called when remove server is called
        /// </summary>
        /// <returns>true if fields of simias-server-setup set successfully</returns>
		private bool OnRemove()
		{
			path.Prompt = systemAdminPassword.Prompt = true;
			systemName.Prompt = systemDescription.Prompt = systemAdminDN.Prompt =  publicUrl.Prompt = slaveServer.Prompt =
				useLdap.Prompt = ldapServer.Prompt = ldapPlugin.Prompt = secure.Prompt = ldapProxyDN.Prompt = ldapSearchContext.Prompt = 
				namingAttribute.Prompt = privateUrl.Prompt = serverName.Prompt = useSsl.Prompt = 
				useRA.Prompt = recoveryAgentCertificatePath.Prompt = ldapAdminDN.Prompt = masterAddress.Prompt = 
				apache.Prompt = ldapAdminPassword.Prompt = ldapProxyPassword.Prompt = false;
			return true;
		}

        /// <summary>
        /// called when port is specified
        /// </summary>
        /// <returns>true if url is built successfully</returns>
		private bool OnPort()
		{
			if(publicUrl.Assigned == true)
			{
				bool enableSSL = String.Compare(useSsl.Value, NonSsl, true) == 0 ? false : true ;
				UriBuilder urlBild = new UriBuilder(publicUrl.Value);
				urlBild.Port = Convert.ToInt32(port.Value);
				urlBild.Scheme = enableSSL?Uri.UriSchemeHttps:Uri.UriSchemeHttp; 
				publicUrl.DefaultValue = publicUrl.InternalValue = AddVirtualPath( urlBild.ToString());
			}
			if(privateUrl.Assigned == true)
			{
				bool enableSSL = String.Compare(useSsl.Value, NonSsl, true) == 0 ? false : true ;
				UriBuilder urlBild = new UriBuilder(privateUrl.Value);
				urlBild.Port = Convert.ToInt32(port.Value);
				urlBild.Scheme = enableSSL?Uri.UriSchemeHttps:Uri.UriSchemeHttp; 
				privateUrl.DefaultValue = privateUrl.InternalValue = AddVirtualPath( urlBild.ToString());
			}
			return true;
		}

        /// <summary>
        /// called when update ldap is called
        /// </summary>
        /// <returns>true</returns>
		private bool OnUpdateLdap()
		{
			path.Prompt = true;
			Console.WriteLine("setting Path");
			ldapAdminDN.Prompt = ldapAdminPassword.Prompt = ldapProxyDN.Prompt = ldapProxyPassword.Prompt = false;

			systemName.Prompt = systemDescription.Prompt = systemAdminDN.Prompt =  publicUrl.Prompt = slaveServer.Prompt =
				useLdap.Prompt = ldapServer.Prompt = ldapPlugin.Prompt = secure.Prompt = ldapSearchContext.Prompt = 
				namingAttribute.Prompt = privateUrl.Prompt = serverName.Prompt = useSsl.Prompt = 
				useRA.Prompt = recoveryAgentCertificatePath.Prompt = masterAddress.Prompt = 
				apache.Prompt = systemAdminPassword.Prompt = false;

			return true;
		}

        /// <summary>
        /// called when migrate option is slelected
        /// </summary>
        /// <returns>true</returns>
		private bool OnMigrate()
		{
			PerformOES1Upgrade();
			publicUrl.Prompt = privateUrl.Prompt = serverName.Prompt = false;
                        privateUrl.Value = publicUrl.Value = "http://127.0.0.1:8086/simias10";
                        serverName.Value = "temporary-server";
                        useSsl.Value = "NONSSL";
			LdapSettings ldapSettings = LdapSettings.Get( oldConfigPath, true);
			ldapServer.Value = ldapSettings.Uri.Host;
			namingAttribute.Value = ldapSettings.NamingAttribute.ToString();
			if ((ldapSettings.ProxyDN != null) && (ldapSettings.ProxyDN.Length > 0))
			{
				ldapProxyDN.Value = ldapSettings.ProxyDN;
			}
			if ((ldapSettings.ProxyPassword != null) && (ldapSettings.ProxyPassword.Length > 0))
			{
				ldapProxyPassword.Value = ldapSettings.ProxyPassword;
			}
			return true;
		}

        /// <summary>
        /// called when upgrade is called
        /// </summary>
        /// <returns>true</returns>
		private bool OnUpgrade()
		{
			switch(int.Parse(upgrade.Value))
			{
                                case 1:
					UpgradeFrom = 1;
					if(!PerformOES1Upgrade())
                               			Environment.Exit(-1);
                                        break;
                                case 2:
					ldapProxyDN.Prompt = ldapProxyPassword.Prompt = false;
					ldapProxyDN.Required = ldapProxyPassword.Required = false;
					UpgradeFrom = 2;
					if(!PerformOES2Upgrade())
                               			Environment.Exit(-1);
                                        break;
				default:
					UpgradeFrom = 0;
                                        break;
			}
                        return true;	
		}

        /// <summary>
        /// called when upgrade of OES2 is to be done
        /// </summary>
        /// <returns>true if all fields are set successfully</returns>
		private bool PerformOES2Upgrade()
		{

			publicUrl.Prompt = privateUrl.Prompt = serverName.Prompt = true;
			
			string storeDataPath = ReadModMonoConfiguration();
			if(storeDataPath == null || storeDataPath == String.Empty || !System.IO.Directory.Exists(storeDataPath))
			{
                               Console.WriteLine("Data Path: {0} not accessible.",storeDataPath == null ? "Null" : storeDataPath);
                               return false;
                        }

			path.DefaultValue = storeDataPath ;
			storePath = Path.GetFullPath(path.Value);
                        if ( Path.GetFileName( storePath ) != "simias" )
                        {
                                storePath = Path.Combine( storePath, "simias" );
                        }

			Console.WriteLine("Path Name: {0}", path.DefaultValue);

			//Information already fetched from config
			
			// LDAP information will be read from the old config.
			useLdap.Prompt = ldapServer.Prompt =  secure.Prompt = 
			systemName.Prompt = systemDescription.Prompt = systemAdminDN.Prompt = path.Prompt = 
			ldapProxyDN.Prompt = ldapProxyPassword.Prompt = ldapSearchContext.Prompt = 
			namingAttribute.Prompt = systemAdminPassword.Prompt = slaveServer.Prompt = 
			masterAddress.Prompt = false;

			useLdap.Required = ldapServer.Required = secure.Required = 
			systemName.Required = systemDescription.Required = systemAdminDN.Required = path.Required = 
			ldapProxyDN.Required = ldapProxyPassword.Required = ldapSearchContext.Required = 
			namingAttribute.Required = systemAdminPassword.Required = slaveServer.Required = 
			masterAddress.Required = false;

			usingLDAP = true;
			//ldapSearchContext.Assigned = true;
			
			LdapSettings ldapSettings = LdapSettings.Get(storePath);
			ldapServer.InternalValue = ldapSettings.Uri.Host;

			secure.InternalValue =  ( ldapSettings.Uri.Scheme == LdapSettings.UriSchemeLdaps) ? "true" : "false" ;
			return true;
		}

        /// <summary>
        /// called if upgrade of OES is called
        /// </summary>
        /// <returns>true if all fields are set successfully</returns>
		private bool PerformOES1Upgrade()
		{

			Configuration oldConfig = new Configuration( oldConfigPath, true );
			publicUrl.Prompt = privateUrl.Prompt = serverName.Prompt = true;
			
			string systemNameStr = oldConfig.Get( "Domain", "EnterpriseName" );
			systemName.DefaultValue = ( systemNameStr != null ) ? systemNameStr : systemName.Value;
//			systemName.Assigned = true;
			Console.WriteLine("System Name: {0}", systemName.DefaultValue);
			
			string systemDescriptionStr = oldConfig.Get( "Domain", "EnterpriseDescription" );
			systemDescription.DefaultValue = ( systemDescriptionStr != null ) ? systemDescriptionStr : systemDescription.Value;
//			systemDescription.Assigned = true;
			Console.WriteLine("System Desc: {0}", systemDescription.DefaultValue);
			
			// system admin dn
			string systemAdminDNStr = oldConfig.Get( "Domain", "AdminDN" );
			systemAdminDN.DefaultValue = ( systemAdminDNStr != null ) ? systemAdminDNStr : systemAdminDN.Value;
//			systemAdminDN.Assigned = true;
			Console.WriteLine("System Admin DN Name: {0}", systemAdminDN.DefaultValue);

			string storeDataPath = oldConfig.Get( "StoreProvider", "Path" );
			path.DefaultValue = ( storeDataPath != null ) ? storeDataPath : path.Value;
//			path.Assigned = true;
			storePath = Path.GetFullPath(path.Value);
                        if ( Path.GetFileName( storePath ) != "simias" )
                        {
                                storePath = Path.Combine( storePath, "simias" );
                        }

			Console.WriteLine("Path Name: {0}", path.DefaultValue);

			// ldap settings
			LdapSettings ldapSettings = LdapSettings.Get( oldConfigPath, true );
			
			// ldap uri
			ldapServer.InternalValue = ldapSettings.Uri.Host;
			
			// naming Attribute
			namingAttribute.DefaultValue = ldapSettings.NamingAttribute.ToString();
			Console.WriteLine("naming attr: {0}", namingAttribute.DefaultValue);
			
			
			// ldap proxy dn
			if ((ldapSettings.ProxyDN != null) && (ldapSettings.ProxyDN.Length > 0))
			{
				ldapProxyDN.InternalValue = ldapSettings.ProxyDN;
				Console.WriteLine("ldapProxyDN: {0}", ldapProxyDN.Value);
			}
			
			// ldap proxy password
			if ((ldapSettings.ProxyPassword != null) && (ldapSettings.ProxyPassword.Length > 0))
			{
				ldapProxyPassword.InternalValue = ldapSettings.ProxyPassword;
			}
			
			// context
			string contexts = "";
			foreach(string context in ldapSettings.SearchContexts)
			{
				contexts += (context + "#");
			}
			
			if (contexts.Length > 1)
			{
				ldapSearchContext.Value = ldapSearchContext.DefaultValue = contexts.Substring(0, contexts.Length - 1);
				Console.WriteLine("ldapSearch Name: {0}", ldapSearchContext.DefaultValue);
				ldapSearchContext.Assigned = true;
			}
			//Information already fetched from config
			
			// LDAP information will be read from the old config.
			useLdap.Prompt = ldapServer.Prompt =  secure.Prompt = 
			systemName.Prompt = systemDescription.Prompt = systemAdminDN.Prompt = path.Prompt = 
			ldapProxyDN.Prompt = ldapProxyPassword.Prompt = ldapSearchContext.Prompt = 
			namingAttribute.Prompt = systemAdminPassword.Prompt = slaveServer.Prompt = 
			masterAddress.Prompt = false;

			useLdap.Required = ldapServer.Required = secure.Required = 
			systemName.Required = systemDescription.Required = systemAdminDN.Required = path.Required = 
			ldapProxyDN.Required = ldapProxyPassword.Required = ldapSearchContext.Required = 
			namingAttribute.Required = systemAdminPassword.Required = slaveServer.Required = 
			masterAddress.Required = false;

			usingLDAP = true;

                        // Check if a default Simias.config exists in the normal
                        // specified areas
                        if ( SetupDefaultConfigPath() == true )
                        {
//                                defaultConfigPath.Prompt = false;
//                                defaultConfigPath.Required = false;

                                SetupConfigFiles();
                        //        UpdateDefaults();
                        }

			//Convert the log to Changelog
			string changelogPath = Path.Combine(storePath,"log");
			string changelogPathNew = Path.Combine(storePath,"changelog");
			if( System.IO.Directory.Exists(changelogPath) )
			{
				if( System.IO.Directory.Exists( changelogPathNew ) == false ||  migrate.Assigned == false )
	                               System.IO.Directory.Move(changelogPath, changelogPathNew);
				else
					Console.Error.WriteLine("Performing a directory move only if in migrate case");
			}
                        else 
                        {
                               Console.WriteLine("Data Path: {0} not accessible.",changelogPath);
                               return false;
                        }

			Console.WriteLine("old {0} New {1}", changelogPath, changelogPathNew);
			return true;
		}

		/// <summary>
		/// Update the Default Values by reading any previously used settings.
		/// </summary>
		void UpdateDefaults()
		{
			try
			{
				Configuration config = new Configuration( storePath, true );

				// server name
				string serverNameStr = config.Get(ServerSection, ServerNameKey);
				serverName.DefaultValue = (serverNameStr == null) ? System.Net.Dns.GetHostName() : serverNameStr;

				// Public address
				Uri defaultUrl = 
					new Uri( 
							Uri.UriSchemeHttp + 
							"://" + 
							System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList[0].ToString() + 
							":80" +
							"/simias10");
				string pubAddress = config.Get( ServerSection, PublicAddressKey );
				publicUrl.DefaultValue = ( pubAddress == null ) ? defaultUrl.ToString() : pubAddress;

				// Private address
				string privAddress = config.Get( ServerSection, PrivateAddressKey );
				privateUrl.DefaultValue = ( privAddress == null ) ? defaultUrl.ToString() : privAddress;

				// system
				string systemNameStr = config.Get( "EnterpriseDomain", "SystemName" );
				systemName.DefaultValue = ( systemNameStr != null ) ? systemNameStr : systemName.Value;

				string systemDescriptionStr = config.Get( "EnterpriseDomain", "Description" );
				systemDescription.DefaultValue = ( systemDescriptionStr != null ) ? systemDescriptionStr : systemDescription.Value;

				// system admin dn
				string systemAdminDNStr = config.Get( "EnterpriseDomain", "AdminName" );
				systemAdminDN.DefaultValue = ( systemAdminDNStr != null ) ? systemAdminDNStr : systemAdminDN.Value;

				// ldap settings
				LdapSettings ldapSettings = LdapSettings.Get( storePath );

				// ldap uri
				ldapServer.DefaultValue = ldapSettings.Uri.Host;

				// naming Attribute
				namingAttribute.DefaultValue = ldapSettings.NamingAttribute.ToString();

				
				// ldap proxy dn
				if ((ldapSettings.ProxyDN != null) && (ldapSettings.ProxyDN.Length > 0))
				{
					ldapProxyDN.DefaultValue = ldapSettings.ProxyDN;
				}
			
				// ldap proxy password
				if ((ldapSettings.ProxyPassword != null) && (ldapSettings.ProxyPassword.Length > 0))
				{
					ldapProxyPassword.DefaultValue = ldapSettings.ProxyPassword;
				}

				// context
				string contexts = "";
				foreach(string context in ldapSettings.SearchContexts)
				{
					contexts += (context + "#");
				}

				if (contexts.Length > 1)
				{
					ldapSearchContext.DefaultValue = contexts.Substring(0, contexts.Length - 1);
				Console.WriteLine("ldapSearch Name: {0}", ldapSearchContext.DefaultValue);
				}

				// Get the Slave settings. This must be last.
				string masterAddressStr = config.Get( ServerSection, MasterAddressKey );
				if ( masterAddressStr != null )
				{
					slaveServer.Value = true;
					masterAddress.Value = masterAddressStr;
				}
			}
			catch{}
		}

		/// <summary>
		/// Get the settings from the config file.
		/// This is used to get the settings from the master server
		/// config file.
		/// </summary>
		private void GetSettingsFromConfig()
		{
			try
			{
				Configuration config = new Configuration( storePath, true );

				// Make sure that our name does not conflict with the master.
				// server Name
				string serverNameStr = config.Get( ServerSection, ServerNameKey );
				bool upLdap = updateLdap.Value != null?  Boolean.Parse(updateLdap.Value) : false;
				if ( upLdap == false && serverNameStr == serverName.Value )
				{
					Console.WriteLine( "The server name must be unique" );
					Environment.Exit(-1);
				}
				
				// system
				systemName.Value = config.Get( "EnterpriseDomain", "SystemName" );
				systemDescription.Value = config.Get( "EnterpriseDomain", "Description" );
				
				// system admin dn
				systemAdminDN.Value = config.Get( "EnterpriseDomain", "AdminName" );

				// ldap settings
				LdapSettings ldapSettings = LdapSettings.Get( storePath );
				masterSearch = new ArrayList();
				string contexts = "";
				foreach(string context in ldapSettings.SearchContexts)
				{
					Console.WriteLine("Context {0}", context);
					masterSearch.Add(context);
					contexts += (context + "#");
				}
				if(contexts.Length > 1)
					ldapSearchContext.DefaultValue = contexts.Substring(0, contexts.Length - 1);
				usingLDAP = ldapSettings.DirectoryType != LdapDirectoryType.Unknown;
				if(!isLdapProxySet)
					ldapProxyDN.Value = ldapSettings.ProxyDN;

				if ( usingLDAP )
				{
					// ldap uri
					// We may need to use a different ldap server prompt for it.
					// Prompt for the ldap server.
					ldapServer.Description = "The host or ip address of an LDAP server.  The server will be used by Simias for authentication.";
					ldapServer.DefaultValue = ldapSettings.Uri.Host;
					ldapServer.Prompt = !slaveServer.Value;
					Prompt.ForOption(ldapServer);

					secure.DefaultValue = ldapSettings.SSL;
					secure.Prompt = !slaveServer.Value;
					Prompt.ForOption(secure);
				}
			}
			catch{}
		}
	
        /// <summary>
        /// set the value inside config file
        /// </summary>
        /// <param name="document">XML document to be changed</param>
        /// <param name="section">section name to be changed</param>
        /// <param name="key">key to be changes</param>
        /// <param name="configValue">value for the key</param>
        /// <returns>true if set successfully</returns>
		private bool SetConfigValue(XmlDocument document, string section, string key, string configValue)
		{
			bool status = false;

			// Build an xpath for the setting.
			string str = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
			XmlElement element = ( XmlElement )document.DocumentElement.SelectSingleNode(str);
			if ( configValue == null )
			{
				// If a null value is passed in, remove the element.
				try
				{
					element.ParentNode.RemoveChild( element );
				}
				catch {}
			}
			else
			{
				if (element != null)
				{
					element.SetAttribute(ValueAttr, configValue);
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
			}

			return status;
		}

        /// <summary>
        /// commit the document
        /// </summary>
        /// <param name="document">XML document</param>
		private void CommitConfiguration( XmlDocument document )
		{
			// Write the configuration file settings.
			XmlTextWriter xtw = 
				new XmlTextWriter( Path.Combine( storePath, Configuration.DefaultConfigFileName ), Encoding.UTF8 );
			try
			{
				xtw.Formatting = Formatting.Indented;
				document.WriteTo( xtw );
			}
			finally
			{
				xtw.Close();
			}
		}
		#endregion

		/// <summary>
		/// Change LDAP Setting
		/// Change LDAP setting and the simias Store
		/// </summary>
		bool ChangeLdapSettings()
		{
			string oldProxyDN = null;
			GetSettingsFromConfig();
			oldProxyDN = ldapProxyDN.Value;
			ldapAdminDN.Prompt = true;
			Prompt.ForOption(ldapAdminDN);
			ldapAdminPassword.Prompt = true;
			Prompt.ForOption(ldapAdminPassword);
			ldapProxyDN.Prompt = true;
			ldapProxyDN.Assigned = false;
			Prompt.ForOption(ldapProxyDN);
			ldapProxyPassword.Prompt = true;
			Prompt.ForOption(ldapProxyPassword);
			// Start with default
			string configFile = configFilePath;
			if ( File.Exists( Path.Combine( storePath, Configuration.DefaultConfigFileName ) ) == true )
			{
				configFile = Path.Combine( storePath, Configuration.DefaultConfigFileName );
			}
			if ( File.Exists( configFile ) == false )
			{
				Console.WriteLine("Simias configuration file \"{0}\" does not exist",configFile);
				return false;
			}
			Console.WriteLine( "Configuring {0}...", configFile );
//			Console.WriteLine( "oldProxy {0}...new Proxy {1} compare {2}", oldProxyDN, ldapProxyDN.Value,  String.Compare(oldProxyDN, ldapProxyDN.Value) != 0);
			UpdateLDAP(String.Compare(oldProxyDN, ldapProxyDN.Value) != 0);
			return true;
		}

		/// <summary>
		/// Update LDAP Setting
		/// Update LDAP setting and the simias Store
		/// </summary>
		void UpdateLDAP(bool changedProxyDN)
		{
			LdapUtility ldapUtility;
			UriBuilder newUri = new UriBuilder();
			
			if(ldapServer.Value.Equals("localhost"))
			{
				System.Net.IPHostEntry hostInfo = System.Net.Dns.GetHostByName( System.Net.Dns.GetHostName() );
				ldapServer.Value = hostInfo.AddressList[0].ToString();
			}
			newUri.Host = ldapServer.Value;
			newUri.Scheme = secure.Value ? LdapSettings.UriSchemeLdaps : LdapSettings.UriSchemeLdap;
			ldapUrl = new Uri(newUri.ToString());
                        ldapUtility = new LdapUtility(ldapUrl.ToString() , ldapAdminDN.Value, ldapAdminPassword.Value);

                        // intall SSL root certificate
                        Console.WriteLine("Installing certificate from {0}...\n", ldapUrl.ToString());

                        if (ldapUtility.Secure && MyEnvironment.Mono)
                        {
                                if (ExecuteWithIO( possibly_yast, "certmgr", "-ssl -m ldaps://{0}:{1}", ldapUtility.Host, ldapUtility.Port) == 0)
                                                Console.WriteLine("Done");
                                else
                                {
                                        Console.WriteLine("Failed (Get Certificate)");
                                }

                        }
                        else
                        {
                                Console.WriteLine("Skipped ({0})", MyEnvironment.Mono ? (UpgradeFrom > 0)? "Not Required" : "Not Supported" : "Mono Only");
                        }


                        // connect
                        Console.WriteLine("Connecting to {0}...", ldapUrl.ToString());
                        ldapUtility.Connect();

                        Console.WriteLine("Done");

                        // get the directory type.
                        Console.WriteLine("Querying for directory type...");
                        LdapDirectoryType directoryType = ldapUtility.QueryDirectoryType();
                        Console.WriteLine( " {0}", directoryType );

                        if ( directoryType.Equals( LdapDirectoryType.Unknown ) )
                        {
                                throw new Exception( string.Format( "Unable to determine directory type for {0}", ldapUtility.Host ) );
                        }

                        // create proxy 
			if(changedProxyDN)
			{
	                        Console.WriteLine("Creating {0}...", ldapProxyDN.Value);
	
        	                if (ldapUtility.CreateUser(ldapProxyDN.Value, ldapProxyPassword.Value))
                	        {
					Console.WriteLine("Done");
	                        }
        	                else
                	        {
					Console.WriteLine();
		                        Console.WriteLine("Updating Password for {0}...", ldapProxyDN.Value);
					ldapUtility.ChangePassword(ldapProxyDN.Value, ldapProxyPassword.Value);
	                        }
			}
			else
			{
	                        Console.WriteLine("Updating Password for {0}...", ldapProxyDN.Value);
				ldapUtility.ChangePassword(ldapProxyDN.Value, ldapProxyPassword.Value);
			}
			ldapUtility.Disconnect();

                        // check proxy
                        Console.WriteLine("Checking {0}...", ldapProxyDN.Value);
                        ldapUtility = new LdapUtility(ldapUrl.ToString(), ldapProxyDN.Value, ldapProxyPassword.Value);
                        ldapUtility.Connect();
                        Console.WriteLine("Done");
                        ldapUtility.Disconnect();


                        // Update simias.config file
                        LdapSettings ldapSettings = LdapSettings.Get( storePath );

                        ldapSettings.DirectoryType = directoryType;

                        // ldap uri
                        ldapSettings.Uri = ldapUrl;

                        // ldap proxy
                        ldapSettings.ProxyDN = ldapProxyDN.Value;
                        ldapSettings.ProxyPassword = ldapProxyPassword.Value;

                        if (changedProxyDN)
                        {
	                        ldapUtility = new LdapUtility(ldapUrl.ToString() , ldapAdminDN.Value, ldapAdminPassword.Value);
        	                ldapUtility.Connect();
                                // context
                                ArrayList list = new ArrayList();
                                foreach(string context in ldapSettings.SearchContexts)
                                {
                                        if ((context != null) && (context.Length > 0))
                                        {
                                                if ( !ldapUtility.ValidateSearchContext( context ) )
                                                {
                                                       throw new Exception( string.Format( "Invalid context entered: {0}", context ) );
                                                }
                                                Console.WriteLine("Granting Read Rights to {0} on {1}...", ldapProxyDN.Value, context);
                                                ldapUtility.GrantReadRights(ldapProxyDN.Value, context);

						list.Add(context);
                                        }
                                }
	                        ldapUtility.Disconnect();
				if(slaveServer.Value)
					ChangedSearchContexts(ref list);	
	                        ldapSettings.SearchContexts = list;

                                // naming attribute to control login
                                ldapSettings.NamingAttribute = namingAttribute.Value;
                        }
                        Console.WriteLine( "Adding LDAP settings to {0}...", Path.Combine( storePath, "Simias.config" ) );

                        ldapSettings.Commit();

                        Console.WriteLine( "Done" );
                        Console.WriteLine( "Restart Novell iFolder server to get the updates..." );

		}

		/// <summary>
		/// Check if search context is changed compared to master
		/// </summary>
		void ChangedSearchContexts(ref ArrayList sc)
		{
			ArrayList templist = new ArrayList();
			foreach(string context in sc)
                        {
				if(masterSearch.Contains(context) == false)
				{
				Console.WriteLine("Context2 {0}", context);
					templist.Add(context);
				}
			}
			sc.Clear();
			foreach(string context in templist)
				sc.Add(context);
		}

		/// <summary>
		/// Setup the Simias.config File
		/// Write the options to the Simias.config file in the datadir.
		/// </summary>
		bool SetupSimias()
		{
			bool status = true;
			bool remove_slave = remove.Value != null? Boolean.Parse(remove.Value) : false;

			// Start with default
			string configFile = configFilePath;
			if ( File.Exists( Path.Combine( storePath, Configuration.DefaultConfigFileName ) ) == true )
			{
				configFile = Path.Combine( storePath, Configuration.DefaultConfigFileName );
			}

			if(remove_slave)
			{
				if ( File.Exists( configFile ) == false )
				{
					Console.WriteLine("Simias configuration file \"{0}\" does not exist",configFile);
					return false;
				}
				Configuration oldConfig = new Configuration( storePath, true );
			
				string adminNameStr = oldConfig.Get( "EnterpriseDomain", "AdminName" );
				if(adminNameStr != null && adminNameStr != String.Empty )
				{
					systemAdminDN.DefaultValue = ( adminNameStr != null ) ? adminNameStr : systemAdminDN.Value;
					Console.WriteLine("Administrator user DN: {0}", systemAdminDN.DefaultValue);
				}
				else
				{
					Console.WriteLine("Failed to read iFolder \"AdminName\" from \"{0}\"",configFile);
					return false;
				}

				string serverNameStr = oldConfig.Get( "Server", "Name" );
				if( serverNameStr != null && serverNameStr != String.Empty )
				{	
					serverName.DefaultValue = ( serverNameStr != null ) ? serverNameStr : serverName.Value;
					Console.WriteLine("Slave server Name: {0}", serverName.DefaultValue);
				}
				else
				{
					Console.WriteLine("Failed to read slave server \"Name\" from \"{0}\"",configFile);
					return false;
				}
			
			
				string masterIpStr = oldConfig.Get( "Server", "MasterAddress" );
				if( masterIpStr != null && masterIpStr != String.Empty )
				{
					masterAddress.Value = ( masterIpStr != null) ? masterIpStr : masterAddress.Value;
					Console.WriteLine("Master Servers URL: {0}", masterAddress.Value);
				}
				else
				{
					Console.WriteLine("Failed to read \"Master server URL\" from \"{0}\"",configFile);
					return false;
				}
				if(!UnRegisterSlaveFromMaster(systemAdminDN.Value,serverName.Value))
				{
					return false;
				}
				return status;
			}

			Console.WriteLine( "Configuring {0}...", configFile );

			// Load the configuration file into an xml document.
			XmlDocument document = new XmlDocument();
			document.Load( configFile );

			// system
			SetConfigValue( document, "EnterpriseDomain", "SystemName", systemName.Value );
			SetConfigValue( document, "EnterpriseDomain", "Description", systemDescription.Value );
			SetConfigValue( document, "EnterpriseDomain", "DomainID", domainID.Value );
            if (String.Compare(useSsl.Value, NonSsl, true) == 0 || String.Compare(useSsl.Value, Both, true) == 0)
				SetConfigValue( document, "Authentication", "SimiasRequireSSL", "no");
			else
				SetConfigValue( document, "Authentication", "SimiasRequireSSL", "yes");
			SetConfigValue( document, "EnterpriseDomain", "AdminName", systemAdminDN.Value );
			SetConfigValue( document, "StoreProvider", "Path", storePath );
			if ( slaveServer.Value )
			{
				//setting appropriate Scheme as entered - see InitializeServiceUrl
//				bool enableSSL = String.Compare(useSsl.Value, NonSsl, true) == 0 ? false : true ;
				UriBuilder masterUri = new UriBuilder( masterAddress.Value );
//				masterUri.Scheme = enableSSL?Uri.UriSchemeHttps:Uri.UriSchemeHttp;
				SetConfigValue( document, ServerSection, MasterAddressKey, masterUri.ToString());
			}
			else
			{
				SetConfigValue( document, "EnterpriseDomain", "AdminPassword", usingLDAP ? null : systemAdminPassword.Value );
			}

			// server
			if(UpgradeFrom != 2)
			SetConfigValue( document, ServerSection, ServerNameKey, serverName.Value);

			SetConfigValue( document, ServerSection, PublicAddressKey, publicUrl.Value );
			SetConfigValue( document, ServerSection, PrivateAddressKey, privateUrl.Value );
			if( useRA.Value )
				SetConfigValue( document, ServerSection, "RAPath", recoveryAgentCertificatePath.Value );

			// Commit the config file changes.
			CommitConfiguration( document );

			if( slaveServer.Value )
			{
//				ldapSettings.SyncInterval = int.MaxValue;
//				ldapSettings.SyncOnStart = false;
				// We need to authenticate to get the domain and owner.
                                string[] dnSegs = systemAdminDN.Value.Split(new char[] {',', '=', '.'});
                                string admin = ( dnSegs.Length == 1 ) ? dnSegs[0] : dnSegs[1];
                                credentials = new System.Net.NetworkCredential( admin, systemAdminPassword.Value, domainId);
				HostAdmin adminService = GetHostAdminService();
				adminService.Credentials = credentials;
				adminService.PreAuthenticate = true;

				// Get and save the domain.
				string domain = adminService.GetDomain();
				// Get and save the owner.
				string dOwner = adminService.GetDomainOwner();
				// Get and save the keypair.
				RSACryptoServiceProvider rsa = Simias.Host.SlaveSetup.CreateKeys( storePath );
				// Join this host to the server and save the node.
				bool created = false;
				string host = 
					adminService.AddHost( 
						serverName.Value, 
						publicUrl.Value, 
						privateUrl.Value, 
						rsa.ToXmlString( false ),
						out created );
				if ( host != null && host.Length != 0 )
				{
					if ( created == true )
					{
						// Save the objects so that they can be created later.
						Simias.Host.SlaveSetup.SaveInitObjects( storePath, domain, dOwner, host, rsa );
					}
					else
					{
						Console.WriteLine( "{0} has already been created and added to the iFolder system.  Please choose a different host name", serverName.Value );
						status = false;
					}
				}
			}
			else
			{
				// interval
//				ldapSettings.SyncInterval = Ldap.LdapSettings.DefaultSyncInterval;

				// sync on start
//				ldapSettings.SyncOnStart = Ldap.LdapSettings.DefaultSyncOnStart;
			}

			if (slaveServer.Value && useLdap.Value)
			{
			        //prompt for ldap settings. 
			        ldapServer.Prompt = true;
			        ldapServer.Required = true;

				secure.Prompt = true;
				secure.Required = true;

				ldapPlugin.Prompt = true;
				ldapPlugin.Required = true;

				// ldap proxy password
				ldapProxyPassword.Prompt = true;
				ldapProxyPassword.Required = true;
				
				ldapSearchContext.Prompt = true;
				ldapSearchContext.Required = true;

				PromptForArguments ();
			}

		
			Console.WriteLine( "SetupSimias - {0}", ( status == true ) ? "Done" : "Failed" );
			return status;
		}


		/// <summary>
		/// Remove slave from master environment
		/// </summary>
		bool UnRegisterSlaveFromMaster(string systemAdminDN, string serverName)
		{
			Console.WriteLine("Removing slave from master");
	               	try
	                {
	                       	// set the Certificate Policy so that SSL connection to master is successful
	                        // we will not backup or reset this policy as this is an one time activity
	                        ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
	                        HostAdmin adminService = GetHostAdminService();
	                        DomainService dService = new DomainService();
	                        string[] dnSegs = systemAdminDN.Split(new char[] {',', '=', '.'});
	                        string admin = ( dnSegs.Length == 1 ) ? dnSegs[0] : dnSegs[1];
	                        credentials = new System.Net.NetworkCredential( admin, systemAdminPassword.Value, domainId);

	                         InitializeServiceUrl( dService );
	                         domainId = dService.GetDomainID();
	
                                adminService.Credentials = credentials;
                                adminService.PreAuthenticate = true;
                                adminService.GetDomain();
	
                                adminService.DeleteHostByName(serverName);
                        }
                        catch(Exception Ex)
                        {
		                Console.WriteLine(" Removing Slave Failed !! {0} ",Ex.ToString());
				return false;
	                }
			return true;
		}

		/// <summary>
		/// Setup the /etc/apache2/conf.d/simias.conf File
		/// </summary>
		void SetupModMono()
		{
			string path = Path.GetFullPath( "/etc/apache2/conf.d/simias.conf" );
			Console.WriteLine("Configuring {0}...", path);
			string ModMonoServer2 = Environment.GetEnvironmentVariable("IFOLDER_MOD_MONO_SERVER2_PATH");
			string iFolderMonoPath = Environment.GetEnvironmentVariable("IFOLDER_MONO_PATH");

			if ( apache.Value == true )
			{
				// create configuration
				using( StreamWriter writer = File.CreateText( path ) )
				{
					/* example
					Include /etc/apache2/conf.d/mod_mono.conf
					
					Alias /simias10 "/usr/web"
					AddMonoApplications simias10 "/simias10:/usr/web"
					MonoSetEnv SimiasRunAsServer=true;SimiasDataPath="/var/opt/novell/ifolder3/simias"
					<Location /simias10 >
						MonoSetServerAlias simias10
						Order allow,deny
						Allow from all
						SetHandler mono
					</Location>
					*/
				
					string alias = "simias10";
					if( iFolderMonoPath != null )
						writer.WriteLine( "Include {0}{1}", iFolderMonoPath, "/etc/apache2/conf.d/mod_mono.conf");
					else
					{
						string mod_mono2_path = "/etc/apache2/conf.d/mod_mono.conf";
						if( File.Exists( mod_mono2_path ))
							writer.WriteLine( "Include {0}", mod_mono2_path );
						else
							writer.WriteLine( "Include {0}", "/etc/apache2/mod_mono.conf" );
					}
					writer.WriteLine();
					writer.WriteLine("Alias /{0} \"{1}\"", alias, SimiasSetup.webdir);
					writer.WriteLine("AddMonoApplications {0} \"/{0}:{1}\"", alias, SimiasSetup.webdir);
					writer.WriteLine("MonoSetEnv {0} \"SimiasRunAsServer=true;SimiasDataDir={1}\"", alias, this.storePath);
					// Set MonoServerPath to the path where ifolder-mod-mono-server2 script file is there
					if(ModMonoServer2 != null)
					{
						writer.WriteLine("MonoServerPath {0} {1}/ifolder-mod-mono-server2", alias, ModMonoServer2);
						writer.WriteLine("MonoMaxActiveRequests {0} {1}", alias, 150);
						writer.WriteLine("MonoMaxWaitingRequests {0} {1}", alias, 250);
					}
					writer.WriteLine("<Location /{0} >", alias);
					writer.WriteLine("\tMonoSetServerAlias {0}", alias);
					writer.WriteLine("\tOrder allow,deny");
					writer.WriteLine("\tAllow from all");
					writer.WriteLine("\tSetHandler mono");
					writer.WriteLine("</Location>");
					writer.WriteLine();
					writer.Close();
				}

				// chmod
				if (Execute("chmod", "644 {0}", path) != 0)
				{
					throw new Exception(String.Format("Unable to change {0} file permissions.", path));
				}

				Console.WriteLine("Done");
			}
			else
			{
       			        apacheUser.Prompt = apacheGroup.Prompt = false;
				Console.WriteLine("Skipped (Apache & Mono Only)");
			}
		}

		/// <summary>
		/// Read the /etc/apache2/conf.d/simias.conf File and return data path. 
		/// </summary>
		string ReadModMonoConfiguration()
		{
			string path = Path.GetFullPath( "/etc/apache2/conf.d/simias.conf" );
			string dataPath = null;
			if ( path == null ||  File.Exists( path ) == false )
				return null;
			try 
        		{
            			using(StreamReader sr = new StreamReader(path))
            			{
                			string line;
					string SearchStr = "MonoSetEnv simias10 \"SimiasRunAsServer=true;SimiasDataDir=";
                			while ((line = sr.ReadLine()) != null) 
                			{
						if(line != null && line != String.Empty && line.StartsWith(SearchStr) == true )
						{
							int startIndex  = line.LastIndexOf('=');
							dataPath = line.Substring(startIndex+1,(line.Length - startIndex - 2 ));
            						Console.WriteLine("Server DATA PATH is set to :  {0} ", dataPath);
							break;
						}
                			}
            			}
        		}
        		catch (Exception e) 
        		{
            			Console.WriteLine("The file {0} could not be read: {1}", path, e.Message);
				return null;
        		}
			return dataPath;
		}

		/// <summary>
		/// Setup the LDAP server
		/// </summary>
		void SetupLdapCert()
		{
			Console.WriteLine("Inside SetupLdapCert");
			LdapUtility ldapUtility;
			UriBuilder newUri = new UriBuilder();
			
			if(ldapServer.Value.Equals("localhost"))
			{
				System.Net.IPHostEntry hostInfo = System.Net.Dns.GetHostByName( System.Net.Dns.GetHostName() );
				ldapServer.Value = hostInfo.AddressList[0].ToString();
			}
			newUri.Host = ldapServer.Value;
			newUri.Scheme = secure.Value ? LdapSettings.UriSchemeLdaps : LdapSettings.UriSchemeLdap;
			ldapUrl = new Uri(newUri.ToString());

			if (!slaveServer.Value) // Master
			{
			        ldapUtility = new LdapUtility(ldapUrl.ToString() , ldapAdminDN.Value, ldapAdminPassword.Value);
			}
			else //Slave
			{
				if(isLdapAdminSet && isLdapAdminPasswordSet)
    			        	ldapUtility = new LdapUtility(ldapUrl.ToString() , ldapAdminDN.Value, ldapAdminPassword.Value);
				else
    			        	ldapUtility = new LdapUtility(ldapUrl.ToString() , systemAdminDN.Value, systemAdminPassword.Value);
			}

			// intall SSL root certificate
			Console.WriteLine("Installing certificate from {0}...\n", ldapUrl.ToString());
				
			if (ldapUtility.Secure && MyEnvironment.Mono)
			{
                                if (ExecuteWithIO( possibly_yast, "certmgr", "-ssl -m ldaps://{0}:{1}", ldapUtility.Host, ldapUtility.Port) == 0)
                                                Console.WriteLine("Done");
                                else
                                {
                                        Console.WriteLine("Failed (Get Certificate)");
                                }

			}
			else
			{
				Console.WriteLine("Skipped ({0})", MyEnvironment.Mono ? (UpgradeFrom > 0)? "Not Required" : "Not Supported" : "Mono Only");
			}

		}

		/// <summary>
		/// Setup the LDAP server
		/// </summary>
		void SetupLdap()
		{
			LdapUtility ldapUtility;
			UriBuilder newUri = new UriBuilder();
			
			if(ldapServer.Value.Equals("localhost"))
			{
				System.Net.IPHostEntry hostInfo = System.Net.Dns.GetHostByName( System.Net.Dns.GetHostName() );
				ldapServer.Value = hostInfo.AddressList[0].ToString();
			}
			newUri.Host = ldapServer.Value;
			newUri.Scheme = secure.Value ? LdapSettings.UriSchemeLdaps : LdapSettings.UriSchemeLdap;
			ldapUrl = new Uri(newUri.ToString());

			if (!slaveServer.Value && !migrate.Assigned) // Master
			{
			        ldapUtility = new LdapUtility(ldapUrl.ToString() , ldapAdminDN.Value, ldapAdminPassword.Value);
			}
			else //Slave
			{
				if(isLdapAdminSet && isLdapAdminPasswordSet && !migrate.Assigned)
    			        	ldapUtility = new LdapUtility(ldapUrl.ToString() , ldapAdminDN.Value, ldapAdminPassword.Value);
				else
    			        	ldapUtility = new LdapUtility(ldapUrl.ToString() , systemAdminDN.Value, systemAdminPassword.Value);
			}

			// intall SSL root certificate
			Console.WriteLine("Installing certificate from {0}...\n", ldapUrl.ToString());
				
			if (ldapUtility.Secure && MyEnvironment.Mono)
			{
				if (ExecuteWithIO( possibly_yast, "certmgr", "-ssl -m ldaps://{0}:{1}", ldapUtility.Host, ldapUtility.Port) == 0)
						Console.WriteLine("Done");
				else
				{
					Console.WriteLine("Failed (Get Certificate)");
				}

			}
			else
			{
				Console.WriteLine("Skipped ({0})", MyEnvironment.Mono ? (UpgradeFrom > 0)? "Not Required" : "Not Supported" : "Mono Only");
			}


			// connect
			Console.WriteLine("Connecting to {0}...", ldapUrl.ToString());
			try
			{
				ldapUtility.Connect();
			}catch(Exception ex)
			{	
				Console.WriteLine(ex.StackTrace);
				throw ex;
			}
				
			Console.WriteLine("Done");

			// get the directory type.
			Console.WriteLine("Querying for directory type...");
			LdapDirectoryType directoryType = ldapUtility.QueryDirectoryType();
			Console.WriteLine( " {0}", directoryType );

			if ( directoryType.Equals( LdapDirectoryType.Unknown ) )
			{
				throw new Exception( string.Format( "Unable to determine directory type for {0}", ldapUtility.Host ) );
			}

			if ((!slaveServer.Value || (isLdapAdminSet && isLdapAdminPasswordSet )) && UpgradeFrom == 0 )
			{
				// create admin
				Console.WriteLine("Creating {0}...", systemAdminDN.Value);

				if (ldapUtility.CreateUser(systemAdminDN.Value, systemAdminPassword.Value))
				{
					Console.WriteLine("Done");
				}
				else
				{
					Console.WriteLine("Skipped (User Exists)");
				}

				// create proxy
				bool created = false;
				int i = 0;
				string proxyDN = ldapProxyDN.Value;
				while(!created)
				{
					Console.WriteLine("Creating {0}...", ldapProxyDN.Value);
					i++;
					if (ldapUtility.CreateUser(ldapProxyDN.Value, ldapProxyPassword.Value))
					{
	//					Console.WriteLine("Created...{0} ... {1}", ldapUtility.DirectoryType, ldapProxyPassword.Value);
						created = true;
						if ( ldapUtility.DirectoryType.Equals( LdapDirectoryType.eDirectory ) )
						{
							// rights
							if (ldapSearchContext.Assigned)
							{
								string[] contexts = ldapSearchContext.Value.Split(new char[] { '#' });
								foreach(string context in contexts)
								{
									if ((context != null) && (context.Length > 0))
									{
										if ( !ldapUtility.ValidateSearchContext( context ) )
										{
											throw new Exception( string.Format( "Invalid context entered: {0}", context ) );
										}
										Console.WriteLine("Granting Read Rights to {0} on {1}...", proxyDN, context);
										ldapUtility.GrantReadRights(proxyDN, context);
									}
								}
							}
					
					
							Console.WriteLine("Done");
						}
						else if ( ldapUtility.DirectoryType.Equals( LdapDirectoryType.ActiveDirectory ) )
						{
							// rights
							if (ldapSearchContext.Assigned)
							{
								string[] contexts = ldapSearchContext.Value.Split(new char[] { '#' });
								foreach(string context in contexts)
								{
									if ((context != null) && (context.Length > 0))
									{
										if ( !ldapUtility.ValidateSearchContext( context ) )
										{
											throw new Exception( string.Format( "Invalid context entered: {0}", context ) );
										}
										Console.WriteLine("Granting Read Rights to {0} on {1}...", proxyDN, context);
										ldapUtility.GrantReadRights(proxyDN, context);
									}
								}
							}
					
					
							Console.WriteLine("Done");
						}
					}
					else
					{
							try
							{
								// check proxy if proxy user is already present and password is incorrect - workaround for 298762
								Console.WriteLine("Checking {0}...", proxyDN);
								LdapUtility ldapUtility2 = new LdapUtility(ldapUrl.ToString(), proxyDN, ldapProxyPassword.Value);
								ldapUtility2.Connect();
								if ( ldapUtility.DirectoryType.Equals( LdapDirectoryType.eDirectory ) )
								{
									// rights
									if (ldapSearchContext.Assigned)
									{
										string[] contexts = ldapSearchContext.Value.Split(new char[] { '#' });
										foreach(string context in contexts)
										{
											if ((context != null) && (context.Length > 0))
											{
												if ( !ldapUtility.ValidateSearchContext( context ) )
												{
													throw new Exception( string.Format( "Invalid context entered: {0}", context ) );
												}
												Console.WriteLine("Granting Read Rights to {0} on {1}...", proxyDN, context);
												try		
												{	
													ldapUtility.GrantReadRights(proxyDN, context);
												}	
												catch(Exception)
												{	
													Console.WriteLine("\nAccess rights are already set for user {0} on {1}", proxyDN, context);
												}
												
											}
										}
									}
								}
								created = true;
								Console.WriteLine("Done");
								Console.WriteLine("Skipped (User Exists)");
							}
							catch(Exception)
							{
								//proxy user password is incorrect, create a new ID
								string[] dnSegs = proxyDN.Split(new char[] {',', '=', '.'});
								string old_proxy = dnSegs[1];
								Console.WriteLine("Old Proxy user {0}...", dnSegs[1]);
								dnSegs[1] = String.Concat(dnSegs[1], i.ToString());
								Console.WriteLine("new Proxy user {0}...", dnSegs[1]);
								proxyDN = proxyDN.Replace(old_proxy, dnSegs[1]);
								ldapProxyDN.Value = proxyDN;
								Console.WriteLine("Checked2 {0}...", proxyDN);
							}
					}
				}
			}
			Console.WriteLine("Checked {0}...", ldapProxyDN.Value);
			// disconnect
			ldapUtility.Disconnect();

			if(UpgradeFrom == 0)
			{
				// check admin
				Console.WriteLine("Checking {0}...", systemAdminDN.Value);
				ldapUtility = new LdapUtility(ldapUrl.ToString(), systemAdminDN.Value, systemAdminPassword.Value);
				ldapUtility.Connect();
				Console.WriteLine("Done");
				ldapUtility.Disconnect();
			}

			Console.WriteLine( "Adding LDAP settings to {0}...", Path.Combine( storePath, "Simias.config" ) );

			// Update simias.config file
			LdapSettings ldapSettings = LdapSettings.Get( storePath );

			ldapSettings.DirectoryType = directoryType;

			// ldap uri
			ldapSettings.Uri = ldapUrl;

			// ldap proxy
			if(UpgradeFrom != 2)
				ldapSettings.ProxyDN = ldapProxyDN.Value;
			if(UpgradeFrom == 0)
			{
				ldapSettings.ProxyPassword = ldapProxyPassword.Value;
			}
			if (slaveServer.Value || (isLdapAdminSet && isLdapAdminPasswordSet))
			{
				if(UpgradeFrom == 0)
				{
					// check proxy
					Console.WriteLine("Checking {0}...", ldapProxyDN.Value);
					ldapUtility = new LdapUtility(ldapUrl.ToString(), ldapProxyDN.Value, ldapProxyPassword.Value);
					ldapUtility.Connect();
					Console.WriteLine("Done");
				}
				// context
				ArrayList list = new ArrayList();
				if (ldapSearchContext.Assigned)
				{
					string[] contexts = ldapSearchContext.Value.Split(new char[] { '#' });
					foreach(string context in contexts)
					{
						if ((context != null) && (context.Length > 0))
						{
							if(UpgradeFrom == 0)
								if ( !ldapUtility.ValidateSearchContext( context ) )
								{
									throw new Exception( string.Format( "Invalid context entered: {0}", context ) );
								}
				
							list.Add(context);
						}
					}
				}
				if(UpgradeFrom == 0)
					ldapUtility.Disconnect();
				if(UpgradeFrom != 2)
				{
					if(slaveServer.Value)
						ChangedSearchContexts(ref list);
					ldapSettings.SearchContexts = list;
				}
				
				// naming attribute to control login
				ldapSettings.NamingAttribute = namingAttribute.Value;
			}

			ldapSettings.Commit();

			Console.WriteLine( "Done" );
		}

        /// <summary>
        /// setup the config files, e.g log4net etc
        /// </summary>
		private void SetupConfigFiles()
		{
			// Setup the links to the store configuration.
			Console.WriteLine( "Setting up store Configuration files..." );

			// Make sure the store path exists.
			if ( System.IO.Directory.Exists( storePath ) == false )
			{
				System.IO.Directory.CreateDirectory( storePath );
			}
				
			// Copy the default configuration file
			// 
			string destConfigFile = Path.Combine( storePath, Configuration.DefaultConfigFileName );
			if ( File.Exists( destConfigFile ) == false )
			{
				File.Copy( configFilePath, destConfigFile );
			}

			// trim off "bill" for log4net
			string comp = Path.DirectorySeparatorChar.ToString() + "bill";
			string srcLog = configPath.TrimEnd( comp.ToCharArray() );

			// Make sure that the log4net file exists.
			string destLog4NetFile = Path.Combine( storePath, Log4NetFile );
			string srcLog4NetFile = Path.Combine( srcLog, Log4NetFile );
			if ( File.Exists( destLog4NetFile ) == false )
			{
				File.Copy( srcLog4NetFile, destLog4NetFile );
			}

			// Make sure that the modules directory exists.
			string destModulesDir = Path.Combine( storePath, ModulesDir );
			string srcModulesDir = Path.Combine( configPath, ModulesDir );
			if ( System.IO.Directory.Exists( destModulesDir ) == false )
			{
				System.IO.Directory.CreateDirectory( destModulesDir );
				string[] files = System.IO.Directory.GetFiles( srcModulesDir );
				foreach( string file in files )
				{
					string fname = Path.GetFileName( file );
					File.Copy(
						Path.Combine( srcModulesDir, fname ),
						Path.Combine( destModulesDir, fname ) );
				}
			}
			
			Console.WriteLine( "Done" );
		}

        /// <summary>
        /// Method to discover the path to the default config files
        /// </summary>
        /// <returns></returns>
		private bool SetupDefaultConfigPath()
		{
			// Check /etc first
			string path =
				String.Format( "{0}{1}{2}{3}{4}{5}",
					Path.DirectorySeparatorChar.ToString(),
					"etc", 
					Path.DirectorySeparatorChar.ToString(),
					"simias",
					Path.DirectorySeparatorChar.ToString(),
					"bill" );
			if ( System.IO.Directory.Exists( path ) == true )
			{
				if ( File.Exists( Path.Combine( path, Simias.Configuration.DefaultConfigFileName ) ) == true )
				{
					configPath = path;
					configFilePath = Path.Combine( configPath, Simias.Configuration.DefaultConfigFileName );
					return true;
				}
			}

			// Check the target area
			path = 
				String.Format( "{0}{1}{2}{3}{4}",
					System.IO.Directory.GetCurrentDirectory(),
					Path.DirectorySeparatorChar.ToString(),
					"etc", 
					Path.DirectorySeparatorChar.ToString(),
					"simias" );

			if ( System.IO.Directory.Exists( path ) == true )
			{
				// bill/Simias.config exist?
				path = Path.Combine( path, "bill" );
				if ( System.IO.Directory.Exists( path ) == true )
				{
					configPath = path;

					if ( File.Exists( Path.Combine( configPath, Simias.Configuration.DefaultConfigFileName ) ) == true )
					{
						configFilePath = Path.Combine( configPath, Simias.Configuration.DefaultConfigFileName ); 
						return true;
					}
				}
			}
			
			// backup up one component in case we're in {target}/bin			
			string cwd = System.IO.Directory.GetCurrentDirectory().TrimEnd( Path.DirectorySeparatorChar );
			int lastComp = cwd.LastIndexOf( Path.DirectorySeparatorChar );
			cwd = cwd.Remove( lastComp, ( cwd.Length - lastComp ) );
			
			path = 
				String.Format( "{0}{1}{2}{3}{4}",
					cwd,
					Path.DirectorySeparatorChar.ToString(),
					"etc", 
					Path.DirectorySeparatorChar.ToString(),
					"simias" );

			if ( System.IO.Directory.Exists( path ) == true )
			{
				// bill/Simias.config exist?
				path = Path.Combine( path, "bill" );
				if ( System.IO.Directory.Exists( path ) == true )
				{
					configPath = path;

					if ( File.Exists( Path.Combine( configPath, Simias.Configuration.DefaultConfigFileName ) ) == true )
					{
						configFilePath = Path.Combine( configPath, Simias.Configuration.DefaultConfigFileName ); 
						return true;
					}
				}
			}

			configPath = null;
			return false;
		}

        /// <summary>
        /// setup the permissions isnide simias dir
        /// </summary>
		private void SetupPermissions()
		{
			// Setup the permissions to the store configuration.
			Console.WriteLine( "Setting up permissions..." );
				
			if ( MyEnvironment.Mono && apache.Value )
			{
				if ( storePath.TrimEnd( new char[] { '/' } ).EndsWith( "simias" ) )
				{
				//	if ( Execute( "chown", " -R {0}:{1} {2}", apacheUser, apacheGroup, System.IO.Directory.GetParent( storePath ).FullName ) != 0 )
					if( Execute( "chown", " -R {0}:{1} {2}", apacheUser.Value, apacheGroup.Value, storePath ) != 0 )
					{
						Console.WriteLine( "Unable to set an owner {0} for the store path.{1}", apacheUser.Value, storePath );
					}
				}
				else
				{
					storePath = Path.Combine(storePath, "simias");
					if ( Execute( "chown", "-R {0}:{1} {2}", apacheUser.Value, apacheGroup.Value, storePath ) != 0 )
					{
						Console.WriteLine( "Unable to set an owner {0} for the store path.{1}", apacheUser.Value, storePath );
					}
				}
			}

			Console.WriteLine( "Done" );
		}

        /// <summary>
        /// setup the script files into store path
        /// </summary>
		private void SetupScriptFiles()
		{
			Console.WriteLine( "Setting up script files..." );

			string fileData;
//			string templatePath = Path.Combine( SimiasSetup.bindir, "simiasserver" + ( MyEnvironment.Windows ? ".cmd" : "" ) );
//			string scriptPath = Path.Combine( SimiasSetup.bindir, serverName.Value + ( MyEnvironment.Windows ? ".cmd" : "" ) );
			string templatePath = Path.Combine( System.IO.Directory.GetCurrentDirectory(), SimiasServerSetup.TemplateScriptFile + ( MyEnvironment.Windows ? ".cmd" : "" ) );
			string scriptPath = Path.Combine( System.IO.Directory.GetCurrentDirectory(), serverName.Value + ( MyEnvironment.Windows ? ".cmd" : "" ) );
			
			try
			{
				using ( StreamReader sr = new StreamReader( templatePath ) )
				{
					fileData = sr.ReadToEnd();
				}
				
				fileData = fileData.Replace( "DataDir=\"\"", String.Format( "DataDir=\"{0}\"", storePath ) );
				fileData = fileData.Replace( "Port=\"\"", String.Format( "Port=\"{0}\"", port.Value ) );
				
				using ( StreamWriter sw = new StreamWriter( scriptPath ) )
				{
					sw.WriteLine( fileData );
				}
				
				if ( MyEnvironment.Mono )
				{
					// Make sure the execute bit is set.
					Execute( "chmod", "ug+x {0}", scriptPath );
				}
			}
			catch
			{
				throw new Exception( String.Format( "Unable to set simias data path in {0}", scriptPath ) );
			}

			Console.WriteLine( "Done" );
			Console.WriteLine( "Run {0} script to load the server", scriptPath );
		}

        /// <summary>
        /// setup log4net for logging framework
        /// </summary>
		private void SetupLog4Net()
		{
			Console.WriteLine( "Setting up Log4Net file..." );
			string filePath = Path.Combine( storePath, "Simias.log4net" );

			char[] seps = {'/', '\\'};

			// update log file names to process name
			XmlDocument doc = new XmlDocument();
			doc.Load( filePath );

			XmlNodeList list = doc.GetElementsByTagName( "file" );						
			for ( int i = 0; i < list.Count; i++ )
			{   
				XmlNode attr = list[i].Attributes.GetNamedItem( "value" );

				string[] comps = attr.Value.Split( seps );
				attr.Value = Path.Combine( storePath, "log" );
				attr.Value = Path.Combine( attr.Value, comps[ comps.Length - 1 ] );
				string logDir = System.IO.Directory.GetParent(attr.Value).FullName;
				if ( System.IO.Directory.Exists( logDir ) == false )
				{
					System.IO.Directory.CreateDirectory( logDir );
				}
			}

			list = doc.GetElementsByTagName( "header" );
			for ( int i = 0; i < list.Count; i++ )
			{   
				XmlNode attr = list[i].Attributes.GetNamedItem( "value" );
				attr.Value = attr.Value.Replace( "%n", Environment.NewLine );
			}

			XmlTextWriter writer = new XmlTextWriter( filePath, null );
			writer.Formatting = Formatting.Indented;
			doc.Save( writer );
			writer.Close();

			/*
			string fileData;
			try
			{
				using ( StreamReader sr = new StreamReader( filePath ) )
				{
					fileData = sr.ReadToEnd();
				}

				fileData = fileData.Replace( "@_LogFilePath_@", storePath.Replace( '\\', '/' ) );

				using ( StreamWriter sw = new StreamWriter( filePath ) )
				{
					sw.WriteLine( fileData );
				}
			}
			catch
			{
				throw new Exception( String.Format( "Unable to set log file path in {0}", filePath ) );
			}
			*/

			Console.WriteLine( "Done" );
		}

		#region Utilities

		/// <summary>
		/// Show Usage
		/// </summary>
		private void ShowUsage()
		{
			Console.WriteLine( "USAGE: simias-server-setup <Path to Simias data directory> [OPTIONS]" );
			Console.WriteLine();
			Console.WriteLine( "OPTIONS:" );
			Console.WriteLine();

			Option[] options = Options.GetOptions( this );

			foreach( Option o in options )
			{
				int nameCount = 0;
				foreach( string name in o.Names )
				{
					Console.WriteLine( "{0}--{1}", nameCount == 0 ? "\n\t" : ", ", name );
					nameCount++;
				}
	
				// Format the description.
				string description = o.Description == null ? o.Title : o.Description;
				Regex lineSplitter = new Regex(@".{0,50}[^\s]*");
				MatchCollection matches = lineSplitter.Matches(description);
				Console.WriteLine();
				if (o.Required)
					Console.WriteLine("\t\t(REQUIRED)");
				foreach (Match line in matches)
				{	
					Console.WriteLine("\t\t{0}", line.Value.Trim());
				}
			}

			Console.WriteLine();
// 			Console.WriteLine("ENVIRONMENT VARIABLES:");
// 			Console.WriteLine();
// 			Console.WriteLine("\t{0}{1}\t\t\t\tLDAP Admin Password", SIMIAS_LDAP_ADMIN_PASSWORD, Environment.NewLine);
// 			Console.WriteLine("\t{0}{1}\t\t\t\tSystem Admin Password", SIMIAS_SYSTEM_ADMIN_PASSWORD, Environment.NewLine);
// 			Console.WriteLine("\t{0}{1}\t\t\t\tLDAP Proxy Password", SIMIAS_LDAP_PROXY_PASSWORD, Environment.NewLine);
// 			Console.WriteLine();

			Environment.Exit(-1);
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
			info.UseShellExecute = false;
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			Process p = Process.Start(info);
			p.StandardOutput.ReadToEnd();
			p.StandardError.ReadToEnd();
			p.WaitForExit();
			return p.ExitCode;
		}

		/// <summary>
		/// ExecuteWithIO executes the command in the shell with IO.
		/// </summary>
		/// <param name="interact">interactive yes/no.</param>
		/// <param name="command">The command.</param>
		/// <param name="format">The arguments of the command.</param>
		/// <param name="args">The arguments for the format.</param>
		/// <returns>The results of the command.</returns>
		int ExecuteWithIO(bool interact, string command, string format, params object[] args)
		{
			ProcessStartInfo info = new ProcessStartInfo( command, String.Format( format, args ) );
			string output ;
			info.UseShellExecute = false;
			info.RedirectStandardInput = true;
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			Process exProcess = Process.Start(info);
			StreamWriter inStreamWriter = exProcess.StandardInput;
			StreamReader outStreamReader = exProcess.StandardOutput;
			Console.WriteLine("Ldap certificate : ");	
			Console.WriteLine("");	
			for(int i=0; i < 10; i++)
			{
				output = outStreamReader.ReadLine();
				Console.WriteLine("{0}",output);	
			}
			Console.WriteLine("");	
			if(interact)
			{
				try
				{
					// certmgr askes question for, adding certificate to machine store and also 
					// to addressbook, so we send "Y" for machine store, "N" for addressbook.
					inStreamWriter.WriteLine("{0}","Y");	
					inStreamWriter.WriteLine("{0}","N");	
				}
				catch(Exception){}
			}
			else
			{
				BoolOption ldapCertAcc = new BoolOption("ldap-cert-acceptance", "Accept LDAP Certificate", null, false, true);
                                ldapCertAcc.Description = "";
                                ldapCertAcc.Prompt = true;
                                Prompt.ForOption(ldapCertAcc);
				try
				{
					// certmgr askes question for, adding certificate to machine store and also 
					// to addressbook, so we send "Y" for machine store, "N" for addressbook.
					if(ldapCertAcc.Value == true)
						inStreamWriter.WriteLine("{0}","Y");	
					else
						inStreamWriter.WriteLine("{0}","N");	
					inStreamWriter.WriteLine("{0}","N");	
				}
				catch(Exception){}
			}
			output = exProcess.StandardOutput.ReadToEnd();
			exProcess.StandardError.ReadToEnd();
			exProcess.WaitForExit();
			return exProcess.ExitCode;
		}

		/*
		/// <summary>
		/// Makes sure that the specified path conforms to the format for the simias data path.
		/// </summary>
		/// <param name="dataPath">Path to the simias data area.</param>
		/// <returns>Processed path if successful, otherwise a null is returned.</returns>
		private string ProcessSimiasDataPath( string dataPath )
		{
			string processedPath = null;
			bool ignoreCase = MyEnvironment.Windows ? true : false;

			try
			{
				string tempPath = Path.GetFullPath( dataPath );
				if ( String.Compare( Path.GetFileName( tempPath ), "simias", ignoreCase ) == 0 )
				{
					processedPath = tempPath;
				}
				else
				{
					processedPath = Path.Combine( tempPath, "simias" );
				}
			}
			catch
			{}

			return processedPath;
		}
		*/

		#endregion

		#region Static Methods

		/// <summary>
		/// Main
		/// </summary>
		/// <param name="args">Command Arguments</param>
		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine();
			Console.WriteLine("SIMIAS SERVER SETUP");
			Console.WriteLine();
		
			try
			{
				SimiasServerSetup setup = new SimiasServerSetup( args );
				setup.Initialize();
				if( setup.Configure() == false )
					Environment.Exit(-1);;
			}
			catch(Exception e)
			{
				Console.WriteLine("Failed");
				Console.WriteLine();
				Console.WriteLine(e);
				Console.WriteLine(e.StackTrace);
				
				Console.WriteLine();
				Console.WriteLine("FAILED");
				Console.WriteLine();

				Environment.Exit(-1);
			}
				
			Console.WriteLine();
			Console.WriteLine("SUCCESS");
			Console.WriteLine();

			Environment.Exit(0);
		}
		#endregion
	}

        internal class TrustAllCertificatePolicy : ICertificatePolicy
        {
                #region ICertificatePolicy Members

                public bool CheckValidationResult(ServicePoint srvPoint,
                        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        WebRequest request, int certificateProblem)
                {
			Console.WriteLine("In cert validation");
			// Accept all, since there is no way to validate other than the user                
                        return true;
                }

                #endregion
        }

}
