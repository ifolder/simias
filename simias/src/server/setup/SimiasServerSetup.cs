/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com
 |
 |  Author: Rob
 |***************************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Diagnostics;
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
		private static readonly string ServerInstallPath = Path.Combine( SimiasSetup.prefix, "server" );

		private static string ServerSection = "Server";
		private static string ServerNameKey = "Name";
		private static string MasterAddressKey = "MasterAddress";
		private static string PublicAddressKey = "PublicAddress";
		private static string PrivateAddressKey = "PrivateAddress";
		private static string oldConfigPath = "/var/lib/wwwrun/.local/share/simias/";

		private static string TemplateScriptFile = "simias-server";

	        //Invalid Character List.
		public static char[] InvalidChars = {'\\', ':', '*', '?', '\"', '<', '>', '|', ' '};

		#endregion

		#region Member Fields

		/// <summary>
		/// Command Arguments
		/// </summary>
		string[] args;

		/// <summary>
		/// The uri to the ldap server.
		/// </summary>
		Uri ldapUrl;

		bool usingLDAP = false;

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
		
		#endregion

		#region Options

		/// <summary>
		/// The store path.
		/// </summary>
		public Option path = new Option("path,p", "Server's Data Path", "Path to the server's data files", true, null);

/*		/// <summary>
		/// The default configuration path.
		/// </summary>
		public Option defaultConfigPath = new Option("default-config-path,c", "Default Configuration Path", "Path to the default configuration files", false, null); 
*/
		/// <summary>
		/// The port to listen on.
		/// </summary>
		public NoPromptOption port = new NoPromptOption("listen-port", "Listen Port", "The port to listen on", false, null);

		/// <summary>
		/// The name of this server.
		/// </summary>
		public Option serverName = new Option("server-name", "Server Name", "The name of this server", true, System.Net.Dns.GetHostName());
		
		/// <summary>
		/// The public address or name for this server.
		/// </summary>
		public Option publicUrl = new Option("public-url", "Public URL", "Public URL of this Simias Server", true, null);
		
		/// <summary>
		/// The Private address or name for this server.
		/// </summary>
		public Option privateUrl = new Option("private-url", "Private URL", "Private URL of this Simias Server", true, null);

		/// <summary>
		/// Use SSL;
		/// </summary>
		public Option useSsl = new BoolOption("use-ssl", "SSL", "Require SSL to communicate with this server", false, true);
		
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
		public Option recoveryAgentCertificatePath = new Option("ra-cert-path", "Recovery Agent Certificate Path", "Path to the Recovery agent certificate's.", false, null);

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
		public Option systemAdminDN = new Option("system-admin-dn", "System Admin", "The Simias default administrator.  If the system is configured to use an external identity source, the distinguished name (dn) should be used.", true, "admin");

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
		/// The store path.
		/// </summary>
		public Option apachePath = new Option("apache-path,ap", "Apache Configuration Path", "Path where Apache maintains the configuration files", false, "/etc/apache2/conf.d");

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
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="cmdArgs">Command Arguments</param>
		SimiasServerSetup(string[] cmdArgs)
		{
			args = cmdArgs;
			System.Net.IPHostEntry hostInfo = System.Net.Dns.GetHostByName( System.Net.Dns.GetHostName() );
			Uri pubUrl = new Uri( Uri.UriSchemeHttp + "://" + hostInfo.AddressList[0].ToString() + ":80" + "/simias10" );
			publicUrl.DefaultValue = pubUrl.ToString();
			ldapServer.DefaultValue = hostInfo.AddressList[0].ToString();
			if ( MyEnvironment.Windows )
			{
				// On windows we do not want to prompt for these values.
				apache.Value = false;
			}

			path.OnOptionEntered = new Option.OptionEnteredHandler( OnPath );
//			defaultConfigPath.OnOptionEntered = new Option.OptionEnteredHandler( OnDefaultConfig );

			serverName.OnOptionEntered = new Option.OptionEnteredHandler( OnServerName );
			slaveServer.OnOptionEntered = new Option.OptionEnteredHandler( OnSlave );
			publicUrl.OnOptionEntered = new Option.OptionEnteredHandler( OnPublicUrl );
			privateUrl.OnOptionEntered = new Option.OptionEnteredHandler( OnPrivateUrl );
			masterAddress.OnOptionEntered = new Option.OptionEnteredHandler( OnMasterAddress );
			useLdap.OnOptionEntered = new Option.OptionEnteredHandler( OnLdap );
			useRA.OnOptionEntered = new Option.OptionEnteredHandler( OnRA );
			recoveryAgentCertificatePath.OnOptionEntered = new Option.OptionEnteredHandler( OnRAPath );
			apache.OnOptionEntered = new Option.OptionEnteredHandler ( OnApache );
			upgrade.OnOptionEntered = new Option.OptionEnteredHandler ( OnUpgrade );
		}

		#endregion

		#region Option Handlers
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
		private bool OnLdap()
		{
			if ( !useLdap.Value )
			{
				usingLDAP = ldapServer.Prompt = secure.Prompt = ldapAdminDN.Prompt =
					ldapAdminPassword.Prompt = ldapProxyDN.Prompt =
					ldapProxyPassword.Prompt = ldapSearchContext.Prompt = 
					namingAttribute.Prompt = false;
			} else {
			        usingLDAP = true;
			}

			return true;
		}

		private bool OnRA()
		{
			if ( !useRA.Value )
			{
				recoveryAgentCertificatePath.Prompt = false;
			} 
			
			return true;
		}

		private bool OnRAPath()
		{
			raPath = Path.GetFullPath( recoveryAgentCertificatePath.Value );
			return System.IO.Directory.Exists( raPath );
		}

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

                                //TestCode: to be merged.
				usingLDAP= ldapServer.Prompt = secure.Prompt = ldapProxyDN.Prompt = ldapSearchContext.Prompt = true;
				Console.WriteLine ("onslave : setting to true");

			}
			return true;
		}

		private bool OnApache()
		{
			if ( !apache.Value )
			{
				apacheUser.Prompt = apacheGroup.Prompt = apachePath.Prompt = false;
			}
			return true;
		}

		private bool OnPublicUrl()
		{
			//privateUrl.DefaultValue = publicUrl.Value;
			if (!publicUrl.Value.ToLower().StartsWith(Uri.UriSchemeHttp))
			{
				publicUrl.Value = (new UriBuilder(Uri.UriSchemeHttp, publicUrl.Value)).ToString();
			}
			privateUrl.DefaultValue = publicUrl.InternalValue = AddVirtualPath( publicUrl.Value );
			Uri pubUri = new Uri( publicUrl.Value );
			if ( string.Compare( pubUri.Scheme, Uri.UriSchemeHttps, true ) == 0 )
				useSsl.Value = true.ToString();
			else
				useSsl.Value = false.ToString();
			port.Value = pubUri.Port.ToString();
			return true;
		}

		private bool OnPrivateUrl()
		{
			if (!privateUrl.Value.ToLower().StartsWith(Uri.UriSchemeHttp))
			{
				privateUrl.Value = (new UriBuilder(Uri.UriSchemeHttp, privateUrl.Value)).ToString();
			}
			privateUrl.InternalValue = AddVirtualPath( privateUrl.Value );
			return true;
		}

		private bool OnMasterAddress()
		{
			// Don't prompt for the following options. They are not needed 
			// or will be obtained from the master.
			// system
			if (!masterAddress.Value.ToLower().StartsWith(Uri.UriSchemeHttp))
			{
				masterAddress.Value = (new UriBuilder(Uri.UriSchemeHttp, masterAddress.Value)).ToString();
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

		private string AddVirtualPath( string path )
		{
			path = path.TrimEnd( '/' );
			if ( path.EndsWith( "/simias10" ) == false )
			{
				path += "/simias10";
			}

			return path;
		}

		private HostAdmin GetHostAdminService()
		{
			HostAdmin adminService = new HostAdmin();
			InitializeServiceUrl( adminService );
			return adminService;
		}

		private void InitializeServiceUrl( System.Web.Services.Protocols.WebClientProtocol service )
		{
			UriBuilder serverUrl = new UriBuilder( service.Url );
			Uri masterUri = new Uri( masterAddress.Value );
			serverUrl.Host = masterUri.Host;
			serverUrl.Port = masterUri.Port;
			string target = service.Url.Substring( service.Url.LastIndexOf( '/' ) );
			serverUrl.Path = masterUri.AbsolutePath + target;
			service.Url = serverUrl.ToString();
		}

		/// <summary>
		/// Initialize
		/// Read /etc/apache2/uid.conf, grab the apache user or group.
		/// </summary>
		void Initialize()
		{
			apachePath.Prompt = false;
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
			catch ( Exception e )
			{
				// Failed. Prompt for apache user & group.
			        apacheUser.Prompt = apacheGroup.Prompt = true;
			}

		}

		bool Configure()
		{
			ParseArguments();
			if ( SetupSimias() == false )
			{
				return false;
			}

			SetupModMono();
			if ( usingLDAP )
			{
				SetupLdap();
			}
			if(apache.Value == false)
				SetupScriptFiles(); //not needed for OES as iFolder runs behind apache. Also helps rpm Uninstall.
			SetupLog4Net();
			SetupPermissions();
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

				Console.Write("This script configures a server installation of Simias to setup a new Simias system. ");

				PromptForArguments();
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
					// Get the Domain ID from the domain service on the master.
					DomainService dService = new DomainService();
					InitializeServiceUrl( dService );
					domainId = dService.GetDomainID();
				
					// Get the configuration file from the master server.
					string[] dnSegs = systemAdminDN.Value.Split(new char[] {',', '=', '.'});
					string admin = ( dnSegs.Length == 1 ) ? dnSegs[0] : dnSegs[1];
					credentials = new System.Net.NetworkCredential( admin, systemAdminPassword.Value, domainId);
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
					Console.WriteLine( "Failed setting up slave server" );
					Console.WriteLine( ex.StackTrace );
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

		private bool OnUpgrade()
		{
			Configuration oldConfig = new Configuration( oldConfigPath, true );
			publicUrl.Prompt = privateUrl.Prompt = serverName.Prompt = true;
			
			// Public address
			Uri defaultUrl = 
				new Uri( 
						Uri.UriSchemeHttp + 
						"://" + 
						System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName()).AddressList[0].ToString() + 
						":80" +
						"/simias10");
			
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
			ldapServer.DefaultValue = ldapSettings.Uri.Host;
			
			// naming Attribute
			namingAttribute.DefaultValue = ldapSettings.NamingAttribute.ToString();
			Console.WriteLine("naming attr: {0}", namingAttribute.DefaultValue);
			
			
			// ldap proxy dn
			if ((ldapSettings.ProxyDN != null) && (ldapSettings.ProxyDN.Length > 0))
			{
				ldapProxyDN.DefaultValue = ldapSettings.ProxyDN;
				Console.WriteLine("ldapProxyDN: {0}", ldapProxyDN.DefaultValue);
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
				ldapSearchContext.Value = ldapSearchContext.DefaultValue = contexts.Substring(0, contexts.Length - 1);
				Console.WriteLine("ldapSearch Name: {0}", ldapSearchContext.DefaultValue);
				ldapSearchContext.Assigned = true;
			}

			//Information already fetched from config
			systemName.Prompt = systemDescription.Prompt = systemAdminDN.Prompt = path.Prompt = false;
			systemName.Required = systemDescription.Required = systemAdminDN.Required = path.Required = false;
			
			// LDAP information will be read from the old config.
			useLdap.Prompt = ldapServer.Prompt = secure.Prompt = 
				ldapProxyDN.Prompt = ldapSearchContext.Prompt = 
				namingAttribute.Prompt = false;

			useLdap.Required = ldapServer.Required = secure.Required = 
				ldapProxyDN.Required = ldapSearchContext.Required = 
				namingAttribute.Required = false;

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
			Console.WriteLine("old {0} New {1}", changelogPath, changelogPathNew);
			System.IO.Directory.Move(changelogPath, changelogPathNew);

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
				if ( serverNameStr == serverName.Value )
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
				usingLDAP = ldapSettings.DirectoryType != LdapDirectoryType.Unknown;
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
		/// Setup the Simias.config File
		/// Write the options to the Simias.config file in the datadir.
		/// </summary>
		bool SetupSimias()
		{
			bool status = true;

			// Start with default
			string configFile = configFilePath;
			if ( File.Exists( Path.Combine( storePath, Configuration.DefaultConfigFileName ) ) == true )
			{
				configFile = Path.Combine( storePath, Configuration.DefaultConfigFileName );
			}

			Console.Write( "Configuring {0}...", configFile );

			// Load the configuration file into an xml document.
			XmlDocument document = new XmlDocument();
			document.Load( configFile );

			// system
			SetConfigValue( document, "EnterpriseDomain", "SystemName", systemName.Value );
			SetConfigValue( document, "EnterpriseDomain", "Description", systemDescription.Value );
			SetConfigValue( document, "Authentication", "SimiasRequireSSL", bool.Parse( useSsl.Value ) ? "yes" : "no");
			SetConfigValue( document, "EnterpriseDomain", "AdminName", systemAdminDN.Value );
			SetConfigValue( document, "StoreProvider", "Path", storePath );
			if ( slaveServer.Value )
			{
				SetConfigValue( document, ServerSection, MasterAddressKey, masterAddress.Value);
			}
			else
			{
				SetConfigValue( document, "EnterpriseDomain", "AdminPassword", usingLDAP ? null : systemAdminPassword.Value );
			}

			// server
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
		/// Setup the /etc/apache2/conf.d/simias.conf File
		/// </summary>
		void SetupModMono()
		{
			string DefaultPath = Path.GetFullPath("/etc/apache2/conf.d");
			if(System.IO.Directory.Exists(DefaultPath) == false)
			{
			Console.Write("check failed");
				apachePath.Prompt = true;
				apachePath.Required = true;

				PromptForArguments();
				DefaultPath = apachePath.Value;
			}
			string path = Path.Combine( DefaultPath, "simias.conf" );
			Console.Write("Configuring {0}...", path);

			if ( apache.Value == true )
			{
				// create configuration
				using( StreamWriter writer = File.CreateText( path ) )
				{
					/* example
					Include /etc/apache2/conf.d/mod_mono.conf
					
					Alias /simias10 "/opt/novell/ifolder3/web"
					AddMonoApplications simias10 "/simias10:/opt/novell/ifolder3/web"
					MonoSetEnv SimiasRunAsServer=true;SimiasDataPath="/var/opt/novell/ifolder3/simias"
					<Location /simias10 >
						MonoSetServerAlias simias10
						Order allow,deny
						Allow from all
						SetHandler mono
					</Location>
					*/
				
					string alias = "simias10";

					writer.WriteLine( "Include {0}mod_mono.conf",DefaultPath );
					writer.WriteLine();
					writer.WriteLine("Alias /{0} \"{1}\"", alias, SimiasSetup.webdir);
					writer.WriteLine("AddMonoApplications {0} \"/{0}:{1}\"", alias, SimiasSetup.webdir);
					writer.WriteLine("MonoSetEnv {0} \"SimiasRunAsServer=true;SimiasDataDir={1}\"", alias, this.storePath);
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
		/// Setup the LDAP server
		/// </summary>
		void SetupLdap()
		{
			LdapUtility ldapUtility;
			UriBuilder newUri = new UriBuilder();
			bool upgrade_if = upgrade.Value != null? Boolean.Parse(upgrade.Value) : false;
			
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
    			        ldapUtility = new LdapUtility(ldapUrl.ToString() , systemAdminDN.Value, systemAdminPassword.Value);
			}

			// intall SSL root certificate
			Console.Write("Installing certificate from {0}...\n", ldapUrl.ToString());
				
			if (ldapUtility.Secure && MyEnvironment.Mono)
			{
				const string certfile = "RootCert.cer";
#if DEBUG
				Console.WriteLine("{0} {1} {2} {3} get {4}",
                                            ldapUtility.Host, ldapUtility.Port, slaveServer.Value ? systemAdminDN.Value : ldapAdminDN.Value,
                                            slaveServer.Value ? systemAdminPassword.Value : ldapAdminPassword.Value, certfile);
#endif
								
				if (Execute("./get-root-certificate", "{0} {1} {2} {3} get {4}",
					    ldapUtility.Host, ldapUtility.Port, slaveServer.Value ? systemAdminDN.Value : ldapAdminDN.Value,
					    slaveServer.Value ? systemAdminPassword.Value : ldapAdminPassword.Value, certfile) == 0)
				{
					Console.WriteLine();
					X509Certificate ldapCert = X509Certificate.CreateFromCertFile(certfile);
					string certDetail = ldapCert.ToString(true);
					BoolOption ldapCertAcc = new BoolOption("ldap-cert-acceptance", "Accept LDAP Certificate", certDetail, false, true);
					ldapCertAcc.Prompt = true;
					Prompt.ForOption(ldapCertAcc);

					if (ldapCertAcc.Value == true && Execute("certmgr", "-add -c -m Trust {0}", certfile) == 0)
					{
						Console.WriteLine("Done");
					}
					else
					{
						Console.WriteLine("Failed (Install Certificate)");
						throw new Exception( string.Format( "User Certificate validation failed for {0}", ldapUtility.Host ) );
					}
				}
				else
				{
					Console.WriteLine("Failed (Get Certificate)");
				}

				// delete file
				if (File.Exists(certfile))
				{
					File.Delete(certfile);
				}
			}
			else
			{
				Console.WriteLine("Skipped ({0})", MyEnvironment.Mono ? (upgrade_if == true)? "Not Required" : "Not Supported" : "Mono Only");
			}


			// connect
			Console.Write("Connecting to {0}...", ldapUrl.ToString());
			ldapUtility.Connect();
				
			Console.WriteLine("Done");

			// get the directory type.
			Console.Write("Querying for directory type...");
			LdapDirectoryType directoryType = ldapUtility.QueryDirectoryType();
			Console.WriteLine( " {0}", directoryType );

			if ( directoryType.Equals( LdapDirectoryType.Unknown ) )
			{
				throw new Exception( string.Format( "Unable to determine directory type for {0}", ldapUtility.Host ) );
			}

			if (!slaveServer.Value && (upgrade_if == false))
			{
				// create admin
				Console.Write("Creating {0}...", systemAdminDN.Value);

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
					Console.Write("Creating {0}...", ldapProxyDN.Value);
					i++;
					if (ldapUtility.CreateUser(ldapProxyDN.Value, ldapProxyPassword.Value))
					{
	//					Console.Write("Created...{0} ... {1}", ldapUtility.DirectoryType, ldapProxyPassword.Value);
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
										Console.Write("Granting Read Rights to {0} on {1}...", proxyDN, context);
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
								Console.Write("Checking {0}...", proxyDN);
								LdapUtility ldapUtility2 = new LdapUtility(ldapUrl.ToString(), proxyDN, ldapProxyPassword.Value);
								ldapUtility2.Connect();
								created = true;
								Console.WriteLine("Done");
								Console.WriteLine("Skipped (User Exists)");
							}
							catch(Exception ex)
							{
								//proxy user password is incorrect, create a new ID
								string[] dnSegs = proxyDN.Split(new char[] {',', '=', '.'});
								string old_proxy = dnSegs[1];
								Console.WriteLine("Old Proxy user {0}...", dnSegs[1]);
								dnSegs[1] = String.Concat(dnSegs[1], i.ToString());
								Console.WriteLine("new Proxy user {0}...", dnSegs[1]);
								proxyDN = proxyDN.Replace(old_proxy, dnSegs[1]);
								ldapProxyDN.Value = proxyDN;
								Console.Write("Checked2 {0}...", proxyDN);
							}
					}
				}
			}
			Console.Write("Checked {0}...", ldapProxyDN.Value);
			// disconnect
			ldapUtility.Disconnect();

			// check admin
			Console.Write("Checking {0}...", systemAdminDN.Value);
			ldapUtility = new LdapUtility(ldapUrl.ToString(), systemAdminDN.Value, systemAdminPassword.Value);
			ldapUtility.Connect();
			Console.WriteLine("Done");
			ldapUtility.Disconnect();

			Console.Write( "Adding LDAP settings to {0}...", Path.Combine( storePath, "Simias.config" ) );

			// Update simias.config file
			LdapSettings ldapSettings = LdapSettings.Get( storePath );

			ldapSettings.DirectoryType = directoryType;

			// ldap uri
			ldapSettings.Uri = ldapUrl;

			// ldap proxy
			ldapSettings.ProxyDN = ldapProxyDN.Value;
			ldapSettings.ProxyPassword = ldapProxyPassword.Value;

			if (!slaveServer.Value)
			{
				// check proxy
				Console.Write("Checking {0}...", ldapProxyDN.Value);
				ldapUtility = new LdapUtility(ldapUrl.ToString(), ldapProxyDN.Value, ldapProxyPassword.Value);
				ldapUtility.Connect();
				Console.WriteLine("Done");
				// context
				ArrayList list = new ArrayList();
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
				
							list.Add(context);
						}
					}
				}
				ldapUtility.Disconnect();
				ldapSettings.SearchContexts = list;
				
				// naming attribute to control login
				ldapSettings.NamingAttribute = namingAttribute.Value;
			}

			ldapSettings.Commit();

			Console.WriteLine( "Done" );
		}

		private void SetupConfigFiles()
		{
			// Setup the links to the store configuration.
			Console.Write( "Setting up store Configuration files..." );

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

		// Method to discover the path to the default config files
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

		private void SetupPermissions()
		{
			// Setup the permissions to the store configuration.
			Console.Write( "Setting up permissions..." );
				
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

		private void SetupScriptFiles()
		{
			Console.Write( "Setting up script files..." );

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

		private void SetupLog4Net()
		{
			Console.Write( "Setting up Log4Net file..." );
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
					Console.Write( "{0}--{1}", nameCount == 0 ? "\n\t" : ", ", name );
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
			Process p = Process.Start(info);
			p.WaitForExit();
			return p.ExitCode;
		}

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
}
