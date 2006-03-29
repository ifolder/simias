/***********************************************************************
 *  $RCSfile: SimiasServerSetup.cs,v $
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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.Text;

using Simias;
using Simias.Client;
using Novell.iFolder;
//using Novell.iFolder.Ldap;
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

		// Configuration file xml tags
		private static readonly string SectionTag = "section";
		private static readonly string SettingTag = "setting";
		private static readonly string NameAttr = "name";
		private static readonly string ValueAttr = "value";

		private static readonly string Log4NetFile = "Simias.log4net";
		private static readonly string ModulesDir = "modules";
		private static readonly string ServerInstallPath = Path.Combine(SimiasSetup.prefix, "server");

		#endregion

		#region Member Fields

		/// <summary>
		/// Command Arguments
		/// </summary>
		string[] args;

		/// <summary>
		/// Configure Apache
		/// </summary>
		bool apache = true;

		/// <summary>
		/// Apache User
		/// </summary>
		string apacheUser = "wwwrun";

		/// <summary>
		/// Apache Group
		/// </summary>
		string apacheGroup = "www";

		/// <summary>
		/// Path to the simias store data directory.
		/// </summary>
		string storePath;

		#endregion

		#region Options

		/// <summary>
		/// Simias System Name
		/// </summary>
		public Option help = new Option("help,?", "Usage Help", false, null);

		/// <summary>
		/// Simias System Name
		/// </summary>
		public Option systemName = new Option("system-name", "System Name", true, "Simias System");

		/// <summary>
		/// Simias System Description
		/// </summary>
		public Option systemDescription = new Option("system-description", "System Description", false, "");

		/// <summary>
		/// LDAP URL
		/// </summary>
		public Option ldapUrl = new Option("ldap-url", "LDAP URL", true, MyEnvironment.Windows ? "ldap://localhost" : "ldaps://localhost");

		/// <summary>
		/// LDAP Admin DN
		/// </summary>
		public Option ldapAdminDN = new Option("ldap-admin-dn", "LDAP Admin DN", true, "cn=admin,o=novell");

		/// <summary>
		/// LDAP Admin Password
		/// </summary>
		public Option ldapAdminPassword = new Option("ldap-admin-password", "LDAP Admin Password", true, "novell");

		/// <summary>
		/// System Admin DN
		/// </summary>
		public Option systemAdminDN = new Option("system-admin-dn", "System Admin DN", true, "cn=SimiasAdmin,o=novell");

		/// <summary>
		/// System Admin Password
		/// </summary>
		public Option systemAdminPassword = new Option("system-admin-password", "System Admin Password", true, "novell");

		/// <summary>
		/// LDAP Proxy DN
		/// </summary>
		public Option ldapProxyDN = new Option("ldap-proxy-dn", "LDAP Proxy DN", true, "cn=SimiasProxy,o=novell");

		/// <summary>
		/// LDAP Proxy Password
		/// </summary>
		public Option ldapProxyPassword = new Option("ldap-proxy-password", "LDAP Proxy Password", true, "novell");

		/// <summary>
		/// LDAP Search Context
		/// </summary>
		public Option ldapSearchContext = new Option("ldap-search-context", "LDAP Search Context", false, "o=novell");

		/// <summary>
		/// Login Type based on what attribute
		/// </summary>
		public Option namingAttribute = new Option("naming-attribute", "Naming Attribute", true, "cn");

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="cmdArgs">Command Arguments</param>
		SimiasServerSetup(string[] cmdArgs)
		{
			if (cmdArgs.Length == 0)
			{
				throw new ApplicationException("Simias data store path was not specified.");
			}

			// Assume that the first argument is a path.
			storePath = ProcessSimiasDataPath(cmdArgs[0]);
			if ( storePath == null )
			{
				throw new Exception("An invalid Simias store path was specified.");
			}

			// Copy the rest of the command line strings to the array of arguments.
			args = new string[cmdArgs.Length - 1];
			Array.Copy(cmdArgs, 1, args, 0, args.Length);
		}

		#endregion

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
				using (TextReader reader = (TextReader)File.OpenText(Path.GetFullPath("/etc/apache2/uid.conf")))
				{
					string line;
					while((line = reader.ReadLine()) != null)
					{
						if (line.StartsWith("User"))
						{
							apacheUser = line.Split()[1];
						}
						else if (line.StartsWith("Group"))
						{
							apacheGroup = line.Split()[1];
						}
					}
				}
			}
			catch
			{
				// ignore
			}

			// Setup links to the store.
			SetupStoreLinks();
		}


		/// <summary>
		/// Setup the Simias Server
		/// </summary>
		void Setup()
		{
			Initialize();
			ParseArguments();
			SetupSimias();
			SetupPermissions();
			SetupModMono();
			//SetupLdap();
			SetupScriptFiles();
			SetupLog4Net();
		}

		#region Arguments

		/// <summary>
		/// Parse the Command-Line Arguments
		/// </summary>
		void ParseArguments()
		{
			if (args.Length == 0)
			{
				// prompt
				UpdateDefaults();
				PromptForArguments();
			}
			else
			{
				// environment variables
				systemAdminPassword.FromEnvironment(SIMIAS_SYSTEM_ADMIN_PASSWORD);
				ldapProxyPassword.FromEnvironment(SIMIAS_LDAP_PROXY_PASSWORD);
				ldapAdminPassword.FromEnvironment(SIMIAS_LDAP_ADMIN_PASSWORD);

				// parse arguments
				Options.ParseArguments(this, args);

				// help
				if (help.Assigned)
				{
					ShowUsage();
				}

#if DEBUG
				// show options for debugging
				Options.WriteOptions(this, Console.Out);
				Console.WriteLine();
#endif

				// check for required options
				Options.CheckRequiredOptions(this);
			}
		}

		/// <summary>
		/// Prompt for Arguments
		/// </summary>
		void PromptForArguments()
		{
			Console.Write("This script configures a server installation of Simias to setup a new Simias system. ");
			Console.Write("The script is intended for testing purposes only. ");
			Console.WriteLine();

			PromptForOption(systemName, "A name used to identify the Simias system to users.");
			PromptForOption(systemDescription, "A detailed description of the Simias system for users.");

			// ldap URL
			WriteSection("LDAP Server", "The host or ip address of an LDAP server.  The server will be searched for users to provision into Simias and will be used by Simias for authentication.");
			Uri uri = new Uri(ldapUrl.Value);
			UriBuilder newUri = new UriBuilder();
			bool secure = uri.Scheme.ToLower().Equals(LdapUtility.LDAP_SCHEME_SECURE) ? true : false;
			newUri.Host = Prompt.ForString("LDAP Server", uri.Host);
			if (MyEnvironment.Windows)
			{
				// Not supported on windows right now.
				secure = false;
			}
			else
			{
				secure = Prompt.ForYesNo("Require a secure connection between the LDAP server and the Simias server", secure);
			}
			newUri.Scheme = secure ? LdapUtility.LDAP_SCHEME_SECURE : LdapUtility.LDAP_SCHEME;
			ldapUrl.Value = newUri.ToString();
			
			PromptForOption(ldapAdminDN, "An existing LDAP user, used by this script only, to connect to the LDAP server and create and/or check required LDAP users for Simias.");
			PromptForOption(ldapAdminPassword);

			PromptForOption(systemAdminDN, "An LDAP user that will be used as a new Simias system's default administrator.  If this user does not already exist in the LDAP tree it will be created. The user's dn, but not the user's password, is stored by Simias.");
			PromptForOption(systemAdminPassword);

			PromptForOption(ldapProxyDN, "An LDAP user that will be used to provision the users between Simias and the LDAP server.  If this user does not already exist in the LDAP tree it will be created and granted read rights at the root of the tree. The user's dn and password are stored by Simias.");
			PromptForOption(ldapProxyPassword);

			PromptForOption(ldapSearchContext, "A list of LDAP tree contexts (delimited by '#') that will be searched for users to provision into Simias." );

			PromptForOption(namingAttribute, "The LDAP attribute you want all users to login using.  I.E. 'cn' or 'email'." );


			if (!MyEnvironment.Windows)
			{
				Console.WriteLine();
				Console.WriteLine("Perform the necessary setup for Simias to run behind Apache?");
				apache = Prompt.ForYesNo("Configure Apache", apache);
			}
			else
			{
				apache = false;
			}

			Console.WriteLine();
			Console.WriteLine("Working...");
			Console.WriteLine();
		}

		private void WriteSection(string title, string text)
		{
			Console.WriteLine();
			Console.WriteLine("----- {0} -----", title.ToUpper());
			Console.WriteLine(text);
			Console.WriteLine();
		}

		private void PromptForOption(Option option)
		{
			option.Value = Prompt.ForString(option.Description, option.DefaultValue);
		}

		private void PromptForOption(Option option, string text)
		{
			WriteSection(option.Description, text);
			PromptForOption(option);
		}

		/// <summary>
		/// Update the Default Values by reading any previously used settings.
		/// </summary>
		void UpdateDefaults()
		{
			Configuration config = new Configuration( storePath, true );

			// system
			string systemNameStr = config.Get("Domain", "EnterpriseName");
			systemName.DefaultValue = (systemNameStr != null) ? systemNameStr : systemName.Value;

			string systemDescriptionStr = config.Get("Domain", "EnterpriseDescription");
			systemDescription.DefaultValue = (systemDescriptionStr != null) ? systemDescriptionStr : systemDescription.Value;

			// system admin dn
			string systemAdminDNStr = config.Get("Domain", "AdminDN");
			systemAdminDN.DefaultValue = (systemAdminDNStr != null) ? systemAdminDNStr : systemAdminDN.Value;

			/*
			// ldap settings
			Ldap.LdapSettings ldapSettings = Ldap.LdapSettings.Get(storePath);

			// ldap uri
			ldapUrl.DefaultValue = ldapSettings.Uri.ToString();

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
			*/
		}
	
		private bool SetConfigValue(XmlDocument document, string section, string key, string configValue)
		{
			bool status = false;

			string str = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
			XmlElement element = ( XmlElement )document.DocumentElement.SelectSingleNode(str);
			if (element != null)
			{
				element.SetAttribute(ValueAttr, configValue);
				status = true;
			}

			return status;
		}

		private void CommitConfiguration(string configFilePath, XmlDocument document)
		{
			// Write the configuration file settings.
			XmlTextWriter xtw = new XmlTextWriter(configFilePath, Encoding.UTF8);
			try
			{
				xtw.Formatting = Formatting.Indented;
				document.WriteTo(xtw);
			}
			finally
			{
				xtw.Close();
			}
		}

		#endregion

		/// <summary>
		/// Setup the Simias.config File
		/// Write the options to the simias-server-bootstrap.config file.
		/// </summary>
		void SetupSimias()
		{
			string configFilePath = Path.Combine(storePath, Simias.Configuration.DefaultConfigFileName);
			Console.Write("Configuring {0}...", configFilePath);

			// Load the configuration file into an xml document.
			XmlDocument document = new XmlDocument();
			document.Load( configFilePath );

			// system
			SetConfigValue( document, "Domain", "EnterpriseName", systemName.Value);
			SetConfigValue( document, "Domain", "EnterpriseDescription", systemDescription.Value);

			// system admin dn
			SetConfigValue( document, "Domain", "AdminDN", systemAdminDN.Value);

			// Commit the config file changes.
			CommitConfiguration( configFilePath, document );

			/*
			// ldap settings
			Ldap.LdapSettings ldapSettings = Ldap.LdapSettings.Get(storePath);

			// ldap uri
			ldapSettings.Uri = new Uri(ldapUrl.Value);

			// ldap proxy
			ldapSettings.ProxyDN = ldapProxyDN.Value;
			ldapSettings.ProxyPassword = ldapProxyPassword.Value;

			// context
			ArrayList list = new ArrayList();
			if (ldapSearchContext.Assigned)
			{
				string[] contexts = ldapSearchContext.Value.Split(new char[] { '#' });
				foreach(string context in contexts)
				{
					if ((context != null) && (context.Length > 0))
					{
						list.Add(context);
					}
				}
			}
			ldapSettings.SearchContexts = list;

			// interval
			ldapSettings.SyncInterval = Ldap.LdapSettings.DefaultSyncInterval;

			// sync on start
			ldapSettings.SyncOnStart = Ldap.LdapSettings.DefaultSyncOnStart;
		
			// naming attribute to control login
			ldapSettings.NamingAttribute = namingAttribute.Value;
			ldapSettings.Commit();
			
			*/

			Console.WriteLine("Done");
		}

		/// <summary>
		/// Setup the /etc/apache2/conf.d/simias.conf File
		/// </summary>
		void SetupModMono()
		{
			string path = Path.GetFullPath("/etc/apache2/conf.d/simias.conf");

			Console.Write("Configuring {0}...", path);

			if (MyEnvironment.Mono && apache)
			{
				// create configuration
				using(StreamWriter writer = File.CreateText(path))
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

					writer.WriteLine("Include /etc/apache2/conf.d/mod_mono.conf");
					writer.WriteLine();
					writer.WriteLine("Alias /{0} \"{1}\"", alias, SimiasSetup.webdir);
					writer.WriteLine("AddMonoApplications {0} \"/{0}:{1}\"", alias, SimiasSetup.webdir);
					writer.WriteLine("MonoSetEnv {0} \"SimiasRunAsServer=true;SimiasDataPath={1}\"", alias, storePath);
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
				Console.WriteLine("Skipped (Apache & Mono Only)");
			}
		}

		/// <summary>
		/// Setup the LDAP server
		/// </summary>
		
		/*
		void SetupLdap()
		{
			LdapUtility ldapUtility = new LdapUtility(ldapUrl.Value, ldapAdminDN.Value, ldapAdminPassword.Value);

			// intall SSL root certificate
			Console.Write("Installing certificate from {0}...", ldapUrl.Value);
				
			if (ldapUtility.Secure && MyEnvironment.Mono)
			{
				const string certfile = "RootCert.cer";

				if (Execute("./get-root-certificate", "{0} {1} {2} {3} get {4}",
					ldapUtility.Host, ldapUtility.Port, ldapUtility.DN, ldapUtility.Password, certfile) == 0)
				{
					Console.WriteLine();

					if (Execute("certmgr", "-add -c -m Trust {0}", certfile) == 0)
					{
						Console.WriteLine("Done");
					}
					else
					{
						Console.WriteLine("Failed (Install Certificate)");
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
				Console.WriteLine("Skipped ({0})", ldapUtility.Secure ? "Mono Only" : "Not Required");
			}

			// connect
			Console.Write("Connecting to {0}...", ldapUrl.Value);

			ldapUtility.Connect();

			Console.WriteLine("Done");

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
			Console.Write("Creating {0}...", ldapProxyDN.Value);

			if (ldapUtility.CreateUser(ldapProxyDN.Value, ldapProxyPassword.Value))
			{
				// use the container of the system admin user
				string containerDN = "";
				string[] parts = ldapAdminDN.Value.Split(new char[] { ',' }, 2);
				if (parts.Length == 2) containerDN = parts[1];

				// rights
				Console.Write("Granting Read Rights to {0} on {1}...", ldapProxyDN.Value, containerDN);

				ldapUtility.GrantReadRights(ldapProxyDN.Value, containerDN);

				Console.WriteLine("Done");
			}
			else
			{
				Console.WriteLine("Skipped (User Exists)");
			}

			// disconnect
			ldapUtility.Disconnect();

			// check admin
			Console.Write("Checking {0}...", systemAdminDN.Value);
			ldapUtility = new LdapUtility(ldapUrl.Value, systemAdminDN.Value, systemAdminPassword.Value);
			ldapUtility.Connect();
			ldapUtility.Disconnect();
			Console.WriteLine("Done");

			// check proxy
			Console.Write("Checking {0}...", ldapProxyDN.Value);
			ldapUtility = new LdapUtility(ldapUrl.Value, ldapProxyDN.Value, ldapProxyPassword.Value);
			ldapUtility.Connect();
			ldapUtility.Disconnect();
			Console.WriteLine("Done");
		}
		*/

		private void SetupStoreLinks()
		{
			// Setup the links to the store configuration.
			Console.Write("Setting up store links...");
				
			if (MyEnvironment.Mono)
			{
				// Create the store directory if needed.
				if (!System.IO.Directory.Exists(storePath))
				{
					System.IO.Directory.CreateDirectory(storePath);
				}

				// Make sure that the configuration file exists.
				if (!File.Exists(Path.Combine(ServerInstallPath, Configuration.DefaultConfigFileName)))
				{
					throw new ApplicationException(String.Format("The {0} file does not exist in the {1} directory", Configuration.DefaultConfigFileName, ServerInstallPath));
				}

				// Create symlinks from the store area to the installed configuration files.
				if ( Execute( "ln", "-sf {0} {1}",
					Path.Combine(ServerInstallPath, Simias.Configuration.DefaultConfigFileName),
					Path.Combine(storePath, Simias.Configuration.DefaultConfigFileName)) != 0)
				{
					throw new Exception(String.Format("Unable to create link for file {0}.", Simias.Configuration.DefaultConfigFileName));
				}

				// Make sure that the log4net file exists.
				if (!File.Exists(Path.Combine(ServerInstallPath, Log4NetFile)))
				{
					throw new ApplicationException(String.Format("The {0} file does not exist in the {1} directory", Log4NetFile, ServerInstallPath));
				}

				if ( Execute( "ln", "-sf {0} {1}",
					Path.Combine(ServerInstallPath, Log4NetFile),
					Path.Combine(storePath, Log4NetFile)) != 0)
				{
					throw new Exception(String.Format("Unable to create link for file {0}.", Log4NetFile));
				}

				// Make sure that the modules directory exists.
				if (!System.IO.Directory.Exists(Path.Combine(ServerInstallPath, ModulesDir)))
				{
					throw new ApplicationException(String.Format("The {0} directory does not exist", Path.Combine(ServerInstallPath, ModulesDir)));
				}

				if ( Execute( "ln", "-sfT {0} {1}",
					Path.Combine(ServerInstallPath, ModulesDir),
					Path.Combine(storePath, ModulesDir)) != 0)
				{
					throw new Exception(String.Format("Unable to create link for file {0}.", ModulesDir));
				}
			}
			else
			{
				// Make sure the store path exists.
				if (!System.IO.Directory.Exists(storePath))
				{
					throw new Exception(String.Format("Store path {0} does not exist.", storePath));
				}

				// Make sure that the configuration file exists.
				if (!File.Exists(Path.Combine(storePath, Configuration.DefaultConfigFileName)))
				{
					throw new ApplicationException(String.Format("The {0} file does not exist in the {1} directory", Configuration.DefaultConfigFileName, storePath));
				}

				// Make sure that the log4net file exists.
				if (!File.Exists(Path.Combine(storePath, Log4NetFile)))
				{
					throw new ApplicationException(String.Format("The {0} file does not exist in the {1} directory", Log4NetFile, storePath));
				}

				// Make sure that the modules directory exists.
				if (!System.IO.Directory.Exists(Path.Combine(storePath, ModulesDir)))
				{
					throw new ApplicationException(String.Format("The {0} directory does not exist", Path.Combine(storePath, ModulesDir)));
				}
			}

			Console.WriteLine("Done");
		}

		private void SetupPermissions()
		{
			// Setup the permissions to the store configuration.
			Console.Write("Setting up permissions...");
				
			if (MyEnvironment.Mono && apache)
			{
				if ( storePath.TrimEnd( new char[] { '/' } ).EndsWith( "simias" ) )
				{
					if (Execute("chown", "{0}:{1} {2}", apacheUser, apacheGroup, System.IO.Directory.GetParent(storePath).FullName) != 0)
					{
						throw new Exception("Unable to set an owner for the store path.");
					}
				}
				else
				{
					if (Execute("chown", "{0}:{1} {2}", apacheUser, apacheGroup, storePath) != 0)
					{
						throw new Exception("Unable to set an owner for the store path.");
					}
				}
			}

			Console.WriteLine("Done");
		}

		private void SetupScriptFiles()
		{
			Console.Write("Setting up script files...");

			string fileData;
			string filePath = Path.Combine(SimiasSetup.bindir, MyEnvironment.Windows ? "simias-server.cmd" : "simias-server");
			try
			{
				using (StreamReader sr = new StreamReader(filePath))
				{
					fileData = sr.ReadToEnd();
				}

				fileData = fileData.Replace("DataDir=\"\"", String.Format("DataDir=\"{0}\"", storePath));
				
				using (StreamWriter sw = new StreamWriter(filePath))
				{
					sw.WriteLine(fileData);
				}
			}
			catch
			{
				throw new Exception(String.Format("Unable to set simias data path in {0}", filePath));
			}

			Console.WriteLine("Done");
		}

		private void SetupLog4Net()
		{
			Console.Write("Setting up Log4Net file...");

			string fileData;
			string filePath = Path.Combine(storePath, "Simias.log4net");
			try
			{
				using (StreamReader sr = new StreamReader(filePath))
				{
					fileData = sr.ReadToEnd();
				}

				fileData = fileData.Replace("@_LogFilePath_@", storePath.Replace( '\\', '/' ) );

				using (StreamWriter sw = new StreamWriter(filePath))
				{
					sw.WriteLine(fileData);
				}
			}
			catch
			{
				throw new Exception(String.Format("Unable to set log file path in {0}", filePath));
			}

			Console.WriteLine("Done");
		}

		#region Utilities

		/// <summary>
		/// Show Usage
		/// </summary>
		private void ShowUsage()
		{
			Console.WriteLine("USAGE: simias-server-setup <Path to Simias data directory> [OPTIONS]");
			Console.WriteLine();
			Console.WriteLine("OPTIONS:");
			Console.WriteLine();

			Option[] options = Options.GetOptions(this);

			foreach(Option o in options)
			{
				foreach(string name in o.Names)
				{
					Console.WriteLine("\t{0}", name);
				}

				Console.WriteLine("\t\t\t\t{0}", o.Description);
			}

			Console.WriteLine();
			Console.WriteLine("ENVIRONMENT VARIABLES:");
			Console.WriteLine();
			Console.WriteLine("\t{0}{1}\t\t\t\tLDAP Admin Password", SIMIAS_LDAP_ADMIN_PASSWORD, Environment.NewLine);
			Console.WriteLine("\t{0}{1}\t\t\t\tSystem Admin Password", SIMIAS_SYSTEM_ADMIN_PASSWORD, Environment.NewLine);
			Console.WriteLine("\t{0}{1}\t\t\t\tLDAP Proxy Password", SIMIAS_LDAP_PROXY_PASSWORD, Environment.NewLine);
			Console.WriteLine();

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
			string arguments = String.Format(format, args);

			ProcessStartInfo info = new ProcessStartInfo(command, arguments);

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
				SimiasServerSetup setup = new SimiasServerSetup(args);
				setup.Setup();
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
