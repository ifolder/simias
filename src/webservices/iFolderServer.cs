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
using System.Net;
using System.IO;
using System.Xml;
using System.Text;
using Novell.Directory.Ldap;
using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Server;
using Simias.Sync;
using Simias.LdapProvider;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder Server Result Set
	/// </summary>
	[Serializable]
	public class iFolderServerSet
	{
		/// <summary>
		/// An Array of iFolder Servers
		/// </summary>
		public iFolderServer[] Items;

		/// <summary>
		/// The Total Number of iFolder Servers
		/// </summary>
		public int Total;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public iFolderServerSet()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="items"></param>
		/// <param name="total"></param>
		public iFolderServerSet(iFolderServer[] items, int total)
		{
			this.Items = items;
			this.Total = total;
		}
	}

	/// <summary>
	/// An iFolder Server
	/// </summary>
	[Serializable]
	public class iFolderServer
	{
		internal static string catalogID = "a93266fd-55de-4590-b1c7-428f2fed815d";  //TODO: refer.
		/// <summary>
		/// iFolder Log Instance
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(iFolderServer));

		/// <summary>
		/// Server ID
		/// </summary>
		public string ID;

		/// <summary>
		/// Server Name
		/// </summary>
		public string Name;
		
		/// <summary>
		/// Server Version
		/// </summary>
		public string Version;
		
		/// <summary>
		/// The Host Name
		/// </summary>
		public string HostName;
		
		/// <summary>
		/// The Machine Name
		/// </summary>
		public string MachineName;
		
		/// <summary>
		/// The OS Version
		/// </summary>
		public string OSVersion;
		
		/// <summary>
		/// The User Name
		/// </summary>
		public string UserName;

		/// <summary>
		/// The common language runtime version.
		/// </summary>
		public string ClrVersion;

		/// <summary>
		/// The public address for this server.
		/// </summary>
		public string PublicUrl;

		/// <summary>
		/// The private address for this server.
		/// </summary>
		public string PrivateUrl;

		/// <summary>
		/// True if this server is the master.
		/// </summary>
		public bool IsMaster;

		/// <summary>
		/// True if this server is the local server.
		/// </summary>
		public bool IsLocal;

		/// <summary>
		/// Number of users provisioned.
		/// </summary>
		public int UserCount;

		/// <summary>
		/// xpath for access-logger log level in Simias.log4net
		/// </summary>
	        private const string xpathAccessLogger = "//logger[@name='AccessLogger']/level";

		/// <summary>
		/// xpath for root-logger log level in Simias.log4net
		/// </summary>
	        private const string xpathRootLogger = "//root/level";

	        public enum LoggerType
		{
		    /// <summary>
		    /// iFolder User Username
		    /// </summary>
		    RootLogger = 0,

		    /// <summary>
		    /// iFolder User Full Name
		    /// </summary>
		    AccessLogger = 1,
		}

// 	        public readonly string log4netConfigurationPath;
	        
// 	        static iFolderServer ()
// 		{
// 		    log4netConfiguration = Store.GetStore().StorePath;
// 		}

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderServer() : this(HostNode.GetLocalHost())
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="server">HostNode object</param>
		public iFolderServer(HostNode server)
		{
			ID = server.UserID;
			Name = server.Name;
			PublicUrl = server.PublicUrl;
			PrivateUrl = server.PrivateUrl;
			IsMaster = server.IsMasterHost;
			IsLocal = server.IsLocalHost;
			UserCount = server.GetHostedMembers().Count;

			Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
			HostName = System.Net.Dns.GetHostName();
			MachineName = System.Environment.MachineName;
			OSVersion = System.Environment.OSVersion.ToString();
			UserName = System.Environment.UserName;
			ClrVersion = System.Environment.Version.ToString();
		}

		/// <summary>
		/// Get the Master iFolder Server in the system
		/// </summary>
		/// <returns>An iFolder Server Object</returns>
		public static iFolderServer GetMasterServer ()
		{
			iFolderServerSet ServerList = GetServersByName (iFolderServerType.Master, SearchOperation.BeginsWith, "*", 0, 0);
			iFolderServer MasterServer = null;
			foreach (iFolderServer server in ServerList.Items)
			{
				if (server.IsMaster)
				{
					MasterServer = server;
					break;
				}
			}
			return MasterServer;
		}

		/// <summary>
		/// Get LDAP details
		/// </summary>
		/// <returns>Return the URL for the server </returns>
		public static LdapInfo GetLdapDetails ()
		{
		    LdapSettings ldapSettings = LdapSettings.Get ( Store.StorePath );
		    return new LdapInfo (ldapSettings) ;
		}

                public static void WriteLdapDetails ()
                {
                    LdapSettings ldapSettings = LdapSettings.Get ( Store.StorePath );
                        string host = ldapSettings.Host;
                        bool SSL = ldapSettings.SSL;
                        string proxydn = ldapSettings.ProxyDN;
                        string proxypwd = ldapSettings.ProxyPassword;
                        ProxyUser pu = new Simias.LdapProvider.ProxyUser();
                        string pwd2 = pu.Password;
			string proxyfilename = "ldapdetails";

                        string allContexts = "";
                        foreach( string context in ldapSettings.SearchContexts)
                        {
                                allContexts = allContexts + context + "#";
                        }

                        //FileStream fileStream = new FileStream( Store.StorePath, FileMode.Create);
                        string filename = Path.Combine( Store.StorePath, proxyfilename);
                        FileStream file = new FileStream( filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        TextWriter tw = new StreamWriter(file);
                        tw.WriteLine("host:"+host);
                        tw.WriteLine("SSL:"+SSL);
                        tw.WriteLine("proxydn:"+proxydn);
                        tw.WriteLine("proxypwd:"+pwd2);
                        tw.WriteLine("Context:"+allContexts);
                        tw.Close();

                }

		public static void ServiceProxyRequests()
		{
							log.Debug(" ServiceProxyRequests: Entered");
			const string FnStoreProxyCreds = "update_proxy_cred_store";
			const string FnRightsAssignment = "proxy_rights_assign";
			bool Proxy_Rights_Assign = false;
			bool Proxy_Creds_Store = false;
			string proxydn = null;	
			string proxypwd = null;
			string ldapadmindn = null;
			string ldapadminpwd = null;
			string proxyfilename2 = "proxydetails";
			string filename = Path.Combine(Store.StorePath, proxyfilename2);
			using(StreamReader sr = new StreamReader(filename))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					if(line != null && line != String.Empty )
					{
						string TrimmedLine = line.Trim();
						if( String.Equals( TrimmedLine, FnRightsAssignment))
						{
							Proxy_Rights_Assign = true;
							continue;
						}
						else if( String.Equals( TrimmedLine, FnStoreProxyCreds ) )
						{
							Proxy_Creds_Store = true;
							continue;
						}
						if( Proxy_Rights_Assign)
						{
							proxydn = TrimmedLine;
							line = sr.ReadLine();
							if(line != null && line != String.Empty )
							{
								ldapadmindn = line.Trim();
							}
							line = sr.ReadLine();
							if(line != null && line != String.Empty )
							{
								ldapadminpwd = line.Trim();
							}
							break;
						}
						else if ( Proxy_Creds_Store )
						{
							//line = sr.ReadLine();
							if(line != null && line != String.Empty )
							{
								proxydn = line.Trim();
							}
							line = sr.ReadLine();
							if(line != null && line != String.Empty )
							{
								proxypwd = line.Trim();
							}
							break;
						}
					}	
				}	
			}
			LdapSettings ldapSettings = LdapSettings.Get ( Store.StorePath );
                        string host = ldapSettings.Host;
                        bool SSL = ldapSettings.SSL;

			// now based on service request, either assign the rights or update the proxy credentials into store
			if (Proxy_Rights_Assign)
			{
				LdapConnection connection = new LdapConnection();
				connection.SecureSocketLayer = SSL ;
				int port = SSL ? 636 : 389;
				connection.Connect( host, port);
				connection.Bind( ldapadmindn, ldapadminpwd);
				LdapAttribute attribute = new LdapAttribute("acl", new String[]
				{
					String.Format("1#subtree#{0}#[Entry Rights]", proxydn),
					String.Format("3#subtree#{0}#[All Attributes Rights]", proxydn)
				});
				LdapModification modification = new LdapModification(LdapModification.ADD, attribute);
				foreach( string context in ldapSettings.SearchContexts)
				{
					try
					{
						connection.Modify( context, modification);
					}
					catch(Exception ex)
					{
						if( ex.Message.IndexOf("Attribute Or Value Exists") >= 0)
							log.Debug(" The rights were already provided earlier, so ignore");
						else
							throw ex;
					}
				}
				try
				{
					connection.Disconnect();
				}
				catch{}
			}
			else if( Proxy_Creds_Store )
			{
				string dataPath = Store.StorePath;
				string path = Path.GetFullPath(Path.Combine(dataPath, "Simias.config"));
				XmlDocument doc = new XmlDocument();
				doc.Load(path);

				string str = string.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", "section", "name", "LdapAuthentication", "setting", "ProxyDN" );

				XmlElement element = ( XmlElement )doc.DocumentElement.SelectSingleNode( str );
				if ( element != null )
				{
					//string val = element.GetAttribute( "value" );

					element.SetAttribute( "value", proxydn );

				}

				// Write the configuration file settings.
				XmlTextWriter xtw = new XmlTextWriter( path, Encoding.UTF8 );
				try
				{
					xtw.Formatting = Formatting.Indented;
					doc.WriteTo( xtw );
				}
				finally
				{
					xtw.Close();
				}


			                //Write Password to ppf file
				string ppwdfile = @".simias.ppf";
				string path1 = Path.Combine(dataPath, ppwdfile);
				FileStream file = new FileStream( path1, FileMode.OpenOrCreate, FileAccess.ReadWrite);
				TextWriter tw = new StreamWriter(file);
				tw.WriteLine( proxypwd );
				log.Debug("method: proxycredstore: wrote the creds into  file");
				tw.Close();
	
			}
		}


		/// <summary>
		/// Set LDAP details
		/// </summary>
		/// <returns>Return the URL for the server </returns>
		public static void SetLdapDetails (LdapInfo ldapInfo, string LdapAdminDN, string LdapAdminPwd, string ServerID)
		{
		    iFolderServer currentServer= GetServer(ServerID);
		    bool IsMasterServer =  currentServer.IsMaster;

		    LdapSettings ldapSettings = LdapSettings.Get ( Store.StorePath );
		    if(String.Compare(ldapSettings.Host,ldapInfo.Host) != 0)
		    	ldapSettings.Host = ldapInfo.Host;
		    if( ldapSettings.SSL != ldapInfo.SSL )
		    	ldapSettings.SSL = ldapInfo.SSL;
		    if(String.Compare(ldapSettings.ProxyDN, ldapInfo.ProxyDN) != 0)
		    	ldapSettings.ProxyDN = ldapInfo.ProxyDN;
		    if(ldapInfo.ProxyPassword != "")
		    	ldapSettings.ProxyPassword = ldapInfo.ProxyPassword;

		    ArrayList list = new ArrayList();
		    string[] contexts = ldapInfo.SearchContexts.Split(new char[] { '#' });

		    foreach(string context in contexts)
		    {
			if ((context != null) && (context.Length > 0))
			{
			    list.Add (context);
			}
		    }
		    if(ldapSettings.SearchContexts != list )
		    	ldapSettings.SearchContexts = list;
		    ldapSettings.Commit(LdapAdminDN, LdapAdminPwd, IsMasterServer);
		}

		/// <summary>
		/// Get the reports
		/// </summary>
		/// <returns>Return string array containg report files </returns>
		public static string[] GetReports ()
		{
			string ReportPath = Report.CurrentReportPath;

			DirectoryInfo di = new DirectoryInfo (ReportPath);

			FileInfo[] finfo = di.GetFiles();

			string[] files = new string [finfo.Length];
			int x = 0;
			// BUG : no proper exception handling.
		        foreach (FileInfo fi  in finfo)
		                files [x++] = fi.Name;

			return files;
		}

        /// <summary>
        /// Checks whether SimiasSSLStatus is set into Simias.config file or not
        /// </summary>
        /// <returns>string containing the value of this key</returns>
		public static string GetSimiasRequiresSSLStatus()
		{
			string SectionTag = "section";
                        string SettingTag = "setting";
                        string NameAttr = "name";
                        string ValueAttr = "value";
			string AuthenticationSection = "Authentication";
			string SimiasSSLKey = "SimiasRequireSSL";

			string requiressl = null;
			string SimiasConfigFilePath = Path.Combine ( Store.StorePath, "Simias.config");
			XmlDocument configDoc = new XmlDocument ();
			configDoc.Load (SimiasConfigFilePath);	
			string str = String.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, AuthenticationSection, SettingTag, SimiasSSLKey );
                        XmlElement element = ( XmlElement )configDoc.DocumentElement.SelectSingleNode( str );
                        if ( element != null )
                        {
				requiressl = element.GetAttribute(ValueAttr);
			}				
			return requiressl;
		}

        /// <summary>
        /// Modify URL so that it beomes ssl enabled
        /// </summary>
        /// <param name="UriString">string to be modified</param>
        /// <param name="sslVal">value of ssl : non-ssl, ssl or both</param>
        /// <returns>new string which is ssl enabled</returns>
		private static string ModifyUriForSSLVal(string UriString, string sslVal)
		{
			Uri uri = new Uri(UriString);
			string modifieduri = UriString;
			int PortVal = -1; 
			try
			{
				if(sslVal == "ssl" || sslVal == "both")
				{
					if(uri.Scheme != Uri.UriSchemeHttps)
					{
						// Simias.config has http:// , change it to https://   
						PortVal = uri.Port;
						if(PortVal == -1 || PortVal == 80)
						{
							PortVal = 443;
						}
						modifieduri = (new UriBuilder(Uri.UriSchemeHttps, uri.Host, PortVal)).ToString();
					}		
				}
				else if(sslVal == "nonssl")
				{
					if(uri.Scheme == Uri.UriSchemeHttps)
					{
						// Simias.config has https:// , change it to http://   
						PortVal = uri.Port;
						if(PortVal == -1 || PortVal == 443)
						{
							PortVal = 80;
						}
						modifieduri = (new UriBuilder(Uri.UriSchemeHttp, uri.Host, PortVal)).ToString();
					}
				}
			}
			catch(Exception ex)
			{
				SmartException.Throw(ex);
			}
			return modifieduri;
		}

        /// <summary>
        /// Sets the SimiasRequireSSL key inside Simias.config file
        /// </summary>
        /// <param name="sslVal">value as string : ssl/nonssl/both</param>
        /// <returns>true if set successfully</returns>
		public static bool SetSimiasSSLStatus(string sslVal)
		{
			string SectionTag = "section";
			string SettingTag = "setting";
			string NameAttr = "name";
			string ValueAttr = "value";
			string AuthenticationSection = "Authentication";
			string SimiasSSLKey = "SimiasRequireSSL";
			string ServerSection = "Server";
			string PrivateAddressKey = "PrivateAddress";
			string PublicAddressXml, PrivateAddressXml;
			string PublicAddressKey = "PublicAddress";
			string PublicAddressStr, PrivateAddressStr, ModifiedPrivateAddressStr=null, ModifiedPublicAddressStr=null;
			string value = "no";
			bool updated = false;
			XmlElement element = null;

			// First read the config file for http scheme and port, see if it needs to be changes according to sslVal
                        string SimiasConfigFilePath = Path.Combine ( Store.StorePath, "Simias.config");	
			try
			{
				XmlDocument configDoc = new XmlDocument ();
				configDoc.Load (SimiasConfigFilePath);
				
				PublicAddressXml = String.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, ServerSection, SettingTag, PublicAddressKey );			
				PrivateAddressXml = String.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, ServerSection, SettingTag, PrivateAddressKey );			
				element = ( XmlElement )configDoc.DocumentElement.SelectSingleNode( PrivateAddressXml );
				if( element != null )
				{
					PrivateAddressStr = element.GetAttribute(ValueAttr);
					ModifiedPrivateAddressStr = ModifyUriForSSLVal(PrivateAddressStr, sslVal);
				}
	
				element = ( XmlElement )configDoc.DocumentElement.SelectSingleNode( PublicAddressXml );
				if ( element != null )
				{
					PublicAddressStr = element.GetAttribute(ValueAttr);
					ModifiedPublicAddressStr = ModifyUriForSSLVal(PublicAddressStr, sslVal);
				}				
				// commit the IPs, it may/may not be same of what was there in Simias.config
				updated = SetIPDetails(ModifiedPrivateAddressStr, ModifiedPublicAddressStr, "");
				if(updated == false)
				{
					return false;
				}
				
				// After setting the IPs, change SimiasRequireSSL value
				// going to update the config file 
                	        XmlDocument document = new XmlDocument();
                        	document.Load(SimiasConfigFilePath );
				if(sslVal == "ssl")
				{
					value = "yes";
				}
				else
				{
					value = "no";
				}
				updated = SetConfigValue( document, AuthenticationSection, SimiasSSLKey, value );
				if(updated == false)
				{
					return false;
				}
        	                CommitConfiguration( document , SimiasConfigFilePath);
			
				SetOnMasterUpdateUri(ModifiedPrivateAddressStr, ModifiedPublicAddressStr);

				updated = true;
			}
			catch(Exception ex)
			{
				SmartException.Throw(ex);
			}
			return updated;
		}
		
        /// <summary>
        /// Updated URL will be set on master, this is for slave servers
        /// </summary>
        /// <param name="ModifiedPrivateAddressStr">private URL </param>
        /// <param name="ModifiedPublicAddressStr">Public url</param>
		public static void SetOnMasterUpdateUri(string ModifiedPrivateAddressStr, string ModifiedPublicAddressStr)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			HostNode masterNode = HostNode.GetMaster(domain.ID);
                        bool OnMaster = false;
			HostNode hNode = HostNode.GetLocalHost();
                        if(hNode.UserID == masterNode.UserID)
                        {
                               OnMaster = true;
                        }
			if(! OnMaster)
			{
				ModifiedPublicAddressStr += "simias10";
				ModifiedPrivateAddressStr += "simias10";
	                        Member cmember = domain.GetCurrentMember();
       	                 	SimiasConnection smConn = new SimiasConnection(domain.ID, cmember.UserID,
       	                                                                                  SimiasConnection.AuthType.PPK,
       	                                                                                   masterNode);
				SimiasWebService svc = new SimiasWebService();
        	                svc.Url = masterNode.PrivateUrl;
                	        smConn.Authenticate ();
                        	smConn.InitializeWebClient(svc, "Simias.asmx");
				svc.SetHostAddress(hNode.UserID, ModifiedPublicAddressStr, ModifiedPrivateAddressStr, domain.ID);
			}
		}

        /// <summary>
        /// Gets the log level from file Simias.log4net
        /// </summary>
        /// <returns>string array containing value of loglevels</returns>
        	public static string[] GetLogLevels ()
		{
		        string[] loglevels = new string[2];
			string log4netConfigurationPath = Path.Combine ( Store.StorePath, "Simias.log4net");

			XmlDocument configDoc = new XmlDocument ();
			configDoc.Load (log4netConfigurationPath);

			loglevels [(int)LoggerType.RootLogger] = GetXmlKeyValue (configDoc, xpathRootLogger, "value");
			loglevels [(int)LoggerType.AccessLogger] = GetXmlKeyValue (configDoc, xpathAccessLogger, "value");

			return loglevels;
		}


		/// <summary>
		/// Set the log level
		/// </summary>
		/// <returns>None </returns>
	    public static void SetLogLevel (LoggerType loggerType, string logLevel)
		{
			string log4netConfigurationPath = Path.Combine ( Store.StorePath, "Simias.log4net");

			XmlDocument configDoc = new XmlDocument ();
			configDoc.Load (log4netConfigurationPath);

			switch (loggerType)
			{
			        case LoggerType.RootLogger:
				        SetXmlKeyValue (configDoc, xpathRootLogger, "value", logLevel);
				break;

			        case LoggerType.AccessLogger:
				        SetXmlKeyValue (configDoc, xpathAccessLogger, "value", logLevel);
				break;
			}

			CommitConfiguration (configDoc, log4netConfigurationPath);
		}

        /// <summary>
        /// Gets the key's value 
        /// </summary>
        /// <param name="xmldoc">document to be looked into</param>
        /// <param name="xpath">path of document</param>
        /// <param name="attribute">attribute/key to be searched</param>
        /// <returns>value as string</returns>
        private static string GetXmlKeyValue( XmlDocument xmldoc, string xpath, string attribute )
		{
			XmlElement xmlElement = xmldoc.DocumentElement.SelectSingleNode( xpath ) as XmlElement;
			return xmlElement.GetAttribute (attribute);
		}

        /// <summary>
        /// Sets the value for a key
        /// </summary>
        /// <param name="xmldoc">XML doc</param>
        /// <param name="xpath">path of doc</param>
        /// <param name="attribute">key </param>
        /// <param name="value">value</param>
	        private static void SetXmlKeyValue( XmlDocument xmldoc, string xpath, string attribute, string value )
		{
			XmlElement xmlElement = xmldoc.DocumentElement.SelectSingleNode( xpath ) as XmlElement;
			xmlElement.SetAttribute (attribute, value);
		}

        /// <summary>
        /// commit the configuration
        /// </summary>
        /// <param name="document">XML doc</param>
        /// <param name="tofile">filename</param>
		private static void CommitConfiguration( XmlDocument document, string tofile )
		{
			// Write the configuration file settings.
			XmlTextWriter xtw = 
				new XmlTextWriter( tofile, Encoding.UTF8 );
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

		/// <summary>
		/// set the public and private IP address into the config file.
		/// </summary>
		/// <returns>true/false based upon the success/failure </returns>
	    public static bool SetIPDetails (string privateUrl, string publicUrl, string MasterUrl)
		{
			bool UpdateStatus = false;
			string ServerSection="Server";
			string PublicAddressKey = "PublicAddress";
		        string PrivateAddressKey = "PrivateAddress";
	                string MasterAddressKey = "MasterAddress";
			if (!privateUrl.ToLower().StartsWith(Uri.UriSchemeHttp))
                        {
                                privateUrl = (new UriBuilder(Uri.UriSchemeHttp, privateUrl)).ToString();
                        }
			if (!publicUrl.ToLower().StartsWith(Uri.UriSchemeHttp))
                        {
                                publicUrl = (new UriBuilder(Uri.UriSchemeHttp, publicUrl)).ToString();
                        }
			if (MasterUrl != null && !privateUrl.ToLower().StartsWith(Uri.UriSchemeHttp))
			{
				MasterUrl = (new UriBuilder(Uri.UriSchemeHttp, MasterUrl)).ToString();
			}
			
			// adding /simias10
			privateUrl = AddVirtualPath( privateUrl );
			publicUrl = AddVirtualPath( publicUrl );
				
			log.Debug("private Url = {0}", privateUrl);
			log.Debug("public url = {0}", publicUrl);

                        string SimiasConfigFilePath = Path.Combine ( Store.StorePath, "Simias.config");	
			if ( File.Exists( Path.Combine( Store.StorePath, Simias.Configuration.DefaultConfigFileName ) ) == true )
                        {
                                SimiasConfigFilePath = Path.Combine( Store.StorePath, Simias.Configuration.DefaultConfigFileName );
                        }
			if ( File.Exists( SimiasConfigFilePath ) == false )
			{
				UpdateStatus = false;
			}
			try
			{
				// going to update the config file 
				// Load the configuration file into an xml document.
                	        XmlDocument document = new XmlDocument();
                        	document.Load(SimiasConfigFilePath );


				SetConfigValue( document, ServerSection, PublicAddressKey, publicUrl );
        	                SetConfigValue( document, ServerSection, PrivateAddressKey, privateUrl );
				if(MasterUrl != "")
                                {
                                        /// it means its a slave server, so set the masters IP into config file
                                        UpdateStatus = SetConfigValueWithSSL( document, ServerSection, MasterAddressKey, MasterUrl );
					if(UpdateStatus == false)
						return false;
					if (MasterUrl != null)
					{
						UpdateStatus = UpdateMasterURL(MasterUrl);
						if(UpdateStatus == false) 
							return false;
					}
                                }			

				// Commit the config file changes.
        	                CommitConfiguration( document , SimiasConfigFilePath);
				UpdateStatus = true;
			}
			catch(Exception ex)
			{
				SmartException.Throw(ex);
			}
			// Also update in the simias.
			SetOnMasterUpdateUri(RemoveVirtualPath(privateUrl), RemoveVirtualPath(publicUrl));

			return UpdateStatus;	
		}
		/// <summary>
		/// This method is used to get the status of server, will verify the
		/// attributes that were changed while upgrading slave to server.
		/// </summary>
		/// <returns>true/false based upon the success/failure </returns>
		public static bool ServerNeedsRepair()
		{
			bool needsRepair= false;
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.DefaultDomain);
			Collection cat = store.GetCollectionByID(catalogID); //Simias.Server.Catalog.catalogID); 

			HostNode localhostNode = HostNode.GetLocalHost();

			if( (domain.Role ==  Simias.Sync.SyncRoles.Slave) &&
					(localhostNode.IsMasterHost == true ) )
			{
				needsRepair = true;	
			}
			else if ( (domain.Role == Simias.Sync.SyncRoles.Master) &&
					(localhostNode.IsMasterHost != true) )
			{
				needsRepair = true;
			}
			else if ( (domain.Role == Simias.Sync.SyncRoles.Slave ) &&
					( (domain.HostID == null) || (domain.HostUri == null)))
			{
				needsRepair = true;
			}
			else if (domain.Role != cat.Role)
			{
				needsRepair = true;
			}
			return needsRepair;
		}
		
		/// <summary>
		/// Run repair on the node, verify the inconsistancy on the node 
		/// </summary>
		/// <returns> true/false for success/failure</returns>
		public static bool RepairChangeMasterUpdates()
		{
			log.Info("RepairChangeMasterUpdates started");
			bool status = true;
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				HostNode localhostNode = HostNode.GetLocalHost();
				if( (domain.Role ==  Simias.Sync.SyncRoles.Slave) &&
						(localhostNode.IsMasterHost == true ) )
				{
					log.Info("Removing the Master Node Attribute from this node");
					localhostNode.IsMasterHost = false;
				}
				domain.Commit(localhostNode);
				log.Info("changes commited after repair");
			}
			catch (Exception ex)
			{
				log.Info("Exception throw while RepairChangeMasterUpdate()");
				status = false;
			}
			return status;
		}

		/// <summary>
		/// This method is used to set the Master server url into simias.config file.
		/// </summary>
		/// <returns>true/false based upon the success/failure </returns>
		public static bool SetMasterServerUrl (string HostID, string MasterUrl)
		{
			bool UpdateStatus = false;
			string ServerSection="Server";
			string MasterAddressKey = "MasterAddress";

			if (MasterUrl != null && !MasterUrl.ToLower().StartsWith(Uri.UriSchemeHttps))
			{
				MasterUrl = (new UriBuilder(Uri.UriSchemeHttps, MasterUrl)).ToString();
			}
			try 
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				HostNode localhostNode = HostNode.GetLocalHost();
				domain.HostID = HostID;
				domain.HostUri = MasterUrl;
				domain.Commit();

				string SimiasConfigFilePath = Path.Combine ( Store.StorePath, "Simias.config");	
				if ( File.Exists( Path.Combine( Store.StorePath, Simias.Configuration.DefaultConfigFileName ) ) == true )
				{
					SimiasConfigFilePath = Path.Combine( Store.StorePath, Simias.Configuration.DefaultConfigFileName );
				}
				XmlDocument document = new XmlDocument();
				document.Load(SimiasConfigFilePath );
				UpdateStatus = SetConfigValueWithSSL( document, ServerSection, MasterAddressKey, MasterUrl );
				if(UpdateStatus == false) return false;
				CommitConfiguration( document , SimiasConfigFilePath);
				UpdateStatus = true;
			}
			catch(Exception ex)
			{
				SmartException.Throw(ex);
			}
			return UpdateStatus;	
		}

		/// <summary>
		/// This method is used to set the Current Server as Slave Server 
		/// To be called while setting the Master as Slave and creating a new master server
		/// Ensure to make this call on the server which is Master Server
		/// <param name ="newMasterHostID">HostID (Ace value) of the new Master Server </param>
		/// <param name ="newMasterMasterPublicUrl">New Masters Public Url, this is to be set as HostUri on the current server domain
		/// for this server to connect to new Master Server </param>
		/// </summary>
		/// <returns>true/false based upon the success/failure </returns>
		public static bool SetAsSlaveServer(string newMasterHostID, string newMasterPublicUrl)
		{
			log.Info("Starting SetAsSlave.....");

			try 
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				Collection cat = store.GetCollectionByID(catalogID); //Simias.Server.Catalog.catalogID); 

				HostNode localhostNode = HostNode.GetLocalHost();

				localhostNode.ChangeMasterState = (int)HostNode.changeMasterStates.Started;
				domain.Commit(localhostNode);

				//Setting the Maset node attribute to slave
				localhostNode.IsMasterHost = false;

				// HostID of the newMaster is updated in doman HostID
				domain.HostID = newMasterHostID;
				log.Info("Settting HostID with new Master Server HostID : {0}", domain.HostID);

				// public url of the newmaster is updated in domain HostUri
				domain.HostUri = newMasterPublicUrl;
				log.Info("Setting HostUri with new Master Servers Public Url : {0}", domain.HostUri);

				// Sync Roles updated for each of the collections.

				//Updating the domain.
				domain.Role =  Simias.Sync.SyncRoles.Slave;
				log.Info("Setting SyncRole on the Domain to Slave ({0})", domain.Role);

				// Updating Catalog
				cat.Role =  Simias.Sync.SyncRoles.Slave;
				log.Info("Setting SyncRole on the catalog");

				domain.Commit();
				cat.Commit(cat);
				log.Info("Commiting all value to set this server ({0} as Slave Server", localhostNode.Name);

				localhostNode.ChangeMasterState = (int)HostNode.changeMasterStates.Complete; 
				domain.Commit(localhostNode);

				string ServerSection="Server";
				string MasterAddressKey = "MasterAddress";
				string SimiasConfigFilePath = Path.Combine ( Store.StorePath, "Simias.config");	
				if ( File.Exists( Path.Combine( Store.StorePath, Simias.Configuration.DefaultConfigFileName ) ) == true )
				{
					SimiasConfigFilePath = Path.Combine( Store.StorePath, Simias.Configuration.DefaultConfigFileName );
				}
				// going to update the config file 
				// Load the configuration file into an xml document.
				XmlDocument document = new XmlDocument();
				document.Load(SimiasConfigFilePath );

				log.Info("Setting the MasterAddress in simias.config file");
				if (! SetConfigValue( document, ServerSection, MasterAddressKey, newMasterPublicUrl))
				{
					log.Error("Unable to add MasterAddress from Simias.config file");
				}
				CommitConfiguration( document , SimiasConfigFilePath);
				log.Debug("Simias.config file updated");
			}
			catch(Exception ex)
			{
				log.Error("Exception while running SetAsSlave : {0} : {1} ", ex.Message, ex.StackTrace);
				return false;
			}
			return true;
		}

		/// <summary>
		/// This method is used to set the Current Slave Server as Master
		/// To be called while setting the Master as Slave and creating a new master server
		/// Ensure to make this call on the server which is Slave Server
		/// <param name ="HostID">HostID (Ace value) of the new Master Server </param>
		/// for this server to connect to new Master Server </param>
		/// </summary>
		/// <returns>true/false based upon the success/failure </returns>
		public static bool SetAsMasterServer(string HostID)
		{
			log.Info("Starting SetAsMasterServer");
			log.Info(" HostName = {0}", HostID);

			try 
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				Collection cat = store.GetCollectionByID(catalogID); //Simias.Server.Catalog.catalogID);

				HostNode localhostNode = HostNode.GetHostByID(domain.ID, HostID);

				localhostNode.ChangeMasterState = (int)HostNode.changeMasterStates.Started;
				domain.Commit(localhostNode);
				log.Info("ChangeMasterState tag set to Started on this server");

				// MasterNodeAttribute updated on this server.
				localhostNode.IsMasterHost = true;
				log.Info("Master Node Attribute set on New Master");

				// Sync Roles updated for each of the collections.
				
				//Updating the domain.
				domain.Role =  Simias.Sync.SyncRoles.Master;
				log.Info("Setting SyncRole on the Domain");

				// Updating Catalog
				cat.Role = Simias.Sync.SyncRoles.Master;
				log.Info("Setting SyncRole on the catalog");

				// Remove the HostId
				domain.HostID = "";
				log.Info("Removing the HostID entry from Slave server");

				// Remove the HostUrl
				domain.HostUri = "";
				log.Info("Removing the HostUri entry from Slave server");

				domain.Commit(); 
				domain.Commit(localhostNode); 
				cat.Commit(cat); 
				log.Info("This server is set as Master server successfully.");

				localhostNode.ChangeMasterState = (int)HostNode.changeMasterStates.Complete; 
				domain.Commit(localhostNode);
				log.Info("ChangeMasterState tag updated on this server");

				string ServerSection="Server";
				string MasterAddressKey = "MasterAddress";
				string SimiasConfigFilePath = Path.Combine ( Store.StorePath, "Simias.config");	
				if ( File.Exists( Path.Combine( Store.StorePath, Simias.Configuration.DefaultConfigFileName ) ) == true )
				{
					SimiasConfigFilePath = Path.Combine( Store.StorePath, Simias.Configuration.DefaultConfigFileName );
				}
				// going to update the config file 
				// Load the configuration file into an xml document.
				XmlDocument document = new XmlDocument();
				document.Load(SimiasConfigFilePath );

				log.Info("Removing the MasterAddress from simias.config file");
				if (! SetConfigValue( document, ServerSection, MasterAddressKey, null ))
				{
					log.Error("Unable to remove MasterAddress from simias.config file");
				}
				CommitConfiguration( document , SimiasConfigFilePath);
				log.Debug("Simias.config file updated");
			}
			catch(Exception ex)
			{
				log.Error("Exception while setting new Master Server :{0} : {1}", ex.Message, ex.StackTrace);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Get the MasterNode Attribute for the host node 
		/// </summary>
		/// <param name="HostID"> ID of the hostnode</param>
		/// <param name="Value"> true/false for master/slave</param>
		/// <returns> true/false for success/failure</returns>
		public static bool SetMasterNodeAttribute (string HostID, bool Value)
		{
			try
			{
				Store store = Store.GetStore();
				log.Info("HostID: {0}, value: {1}", HostID, Value);
				Domain domain = store.GetDomain(store.DefaultDomain);
				//HostNode lHostNode = HostNode.GetHostByID(domain.ID, HostID);
				HostNode lHostNode = HostNode.GetLocalHost();
				lHostNode.IsMasterHost = Value;
				domain.Commit(lHostNode);
			}
			catch (Exception ex)
			{
				log.Error("Unable to set Master Node Attribute :{0} :{1}", ex.Message, ex.StackTrace);
				return false;
			}
			return true;	
		}

		/// <summary>
		/// Get the MasterNode Attribute for the host node 
		/// </summary>
		/// <param name="HostID"> ID of the hostnode</param>
		/// <returns> true/false for master/slave</returns>
		public static bool GetMasterNodeAttribute(string HostID)
		{
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				HostNode lHostNode = HostNode.GetHostByID(domain.ID, HostID);
				return lHostNode.IsMasterHost ;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Log of all changes done after Changing the Master server 
		/// </summary>
		/// <param name="currentMasterID"> ID of the hostnode</param>
		/// <param name="newMasterID"> ID of the hostnode</param>
		/// /// <returns> true/false on success/failure </returns>
		public static bool VerifyChangeMaster(string currentMasterID, string newMasterID)
		{
			bool retval = true; 
			log.Info("Verifying all values after Changing Master and Slave Server");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				Collection cat = store.GetCollectionByID(catalogID); //Simias.Server.Catalog.catalogID);

				HostNode currentMasterNode = HostNode.GetHostByID(domain.ID, currentMasterID);
				HostNode newMasterNode = HostNode.GetHostByID(domain.ID, newMasterID);

				log.Info("Domain attributes");
				log.Info("HostID : {0}", domain.HostID);
				log.Info("HostUrl : {0}", domain.HostUri);
				log.Info("Domain Sync Role : {0}", domain.Role);

				log.Info("Catalog attributes");
				log.Info("Catalog Sync Role : {0}", cat.Role);

				log.Info("Current Master node attributes");
				log.Info("Name : {0}", currentMasterNode.Name);
				log.Info("IsMasterHost : {0}", currentMasterNode.IsMasterHost);
				log.Info("ChangeMasterState : {0}", currentMasterNode.ChangeMasterState);

				log.Info("New Master node attributes");
				log.Info("Name : {0}", newMasterNode.Name);
				log.Info("IsMasterHost : {0}", newMasterNode.IsMasterHost);
				log.Info("ChangeMasterState : {0}", newMasterNode.ChangeMasterState);
			}
			catch (Exception ex)
			{
				log.Error("Uable to verify change master");
				retval = false;
			}
			return retval;
		}

		/// <summary>
		/// Update the Master Url in the local store
		/// </summary>
		/// <param name="masterUrl"> url of the master server for the domain </param>
		/// /// <returns> true/false on success/failure </returns>
		private static bool UpdateMasterURL( string masterUrl)
		{
			bool retVal = true;;
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);
				HostNode masterNode = HostNode.GetMaster(domain.ID);
				masterNode.PrivateUrl = masterUrl;
				domain.Commit(masterNode);
			}
			catch (Exception ex)
			{
				retVal = false;
			}
			return retVal;
		}

        	/// <summary>
	        /// Adds simias10 into the path
        	/// </summary>
	        /// <param name="path"></param>
        	/// <returns>new path</returns>
		private static string RemoveVirtualPath(string path)
		{
			path = path.TrimEnd('/');
			if(path.EndsWith("/simias10") == true)
			{
				path = path.Substring(0, path.IndexOf("simias10"));
			}
			return path;
		}

        /// <summary>
        /// Adds simias10 into the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>new path</returns>
		private static string AddVirtualPath( string path )
                {
                        path = path.TrimEnd( '/' );
                        if ( path.EndsWith( "/simias10" ) == false )
                        {
                                path += "/simias10";
                        }

                        return path;
                }
		
        /// <summary>
        /// Sets master's address with SSL
        /// </summary>
        /// <param name="document">XML doc</param>
        /// <param name="ServerSection">Server section name</param>
        /// <param name="MasterAddressKey">master's address key name</param>
        /// <param name="MasterUrl">Master's URL</param>
        /// <returns>true if successful</returns>
		private static bool SetConfigValueWithSSL( XmlDocument document, string ServerSection, string MasterAddressKey, string MasterUrl )
		{
			bool updated = false;
			try
			{
				SetupSSLForMaster(MasterUrl);	
				updated = SetConfigValue( document, ServerSection, MasterAddressKey, MasterUrl );
			}
			catch(Exception ex)
			{
				log.Error("Ex at SetConfigValuewithSSL");
				SmartException.Throw(ex);
			}
			return updated;
		}

        /// <summary>
        /// Sets the config value of a section with given key/value
        /// </summary>
        /// <param name="document">XML doc</param>
        /// <param name="section">section name to be set</param>
        /// <param name="key">key name</param>
        /// <param name="configValue">value</param>
        /// <returns>true if successful</returns>
		private static bool SetConfigValue(XmlDocument document, string section, string key, string configValue)
		{
			bool status = false;			
			// xml tags
			string SectionTag = "section";
			string SettingTag = "setting";
			string NameAttr = "name";
			string ValueAttr = "value";

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

		private static string GetServerArch()
		{
			string machArch = null;
			try 
			{

				string path = Path.GetFullPath("/etc/apache2/conf.d/simias.conf");

				TextReader reader = (TextReader)File.OpenText(path);
	
				if(reader == null)
					return null;
				
				string line;

				while((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("Alias"))
					{
						machArch = (line.IndexOf("lib64") >= 0) ? "x86_64" : "x86_32";
						break;
					}
				}
				
				reader.Close();
			}catch
			{
			}
			return machArch;
		}

        /// <summary>
        /// Set up ssl for master server
        /// </summary>
        /// <param name="masterAddress">address of the master server</param>
		private static void SetupSSLForMaster(string masterAddress)
		{
			if (masterAddress.ToLower().StartsWith(Uri.UriSchemeHttps))
			{
				string machineArch = GetServerArch();
				string webPath = webPath = Path.GetFullPath("../../../../lib/simias/web");

				if (machineArch != null)
				{
					webPath = ( machineArch.IndexOf("_64" ) > 0 ? Path.GetFullPath("../../../../lib64/simias/web"): Path.GetFullPath("../../../../lib/simias/web"));
				}

				// swap policy
				ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
				HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(masterAddress);

				try
				{
					HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				}
				catch (Exception ex)
				{
					// ignore
				}

				// restore policy

				// service point
				ServicePoint sp = request.ServicePoint;
				if(sp == null) throw new Exception("sp is null for master "+ masterAddress);
				if(sp.Certificate == null) throw new Exception("sp.Certificate is null for master "+masterAddress);
				if ((sp != null) && (sp.Certificate != null))
				{
					string path = Path.GetFullPath(Path.Combine(webPath, "web.config"));
					string certRawDetail = Convert.ToBase64String(sp.Certificate.GetRawCertData());
					string certDetail = sp.Certificate.ToString(true);
					XmlDocument doc = new XmlDocument();
					doc.Load(path);
					XmlElement cert = (XmlElement)doc.DocumentElement.SelectSingleNode("//configuration/appSettings/add[@key='SimiasCert']");
					if (cert != null)
					{
						cert.Attributes["value"].Value = certRawDetail;
						doc.Save(path);
					}
					else
					{
						throw new Exception(String.Format("Unable to find \"SimiasCert\" tag in the {0} file.", path));
					}
				}
				else
				{
					throw new Exception("Unable to retrieve the certificate from the iFolder server.webpath is :"+webPath);
				}
			}
		}


		/// <summary>
		/// Get the HomeServer URL for User.
		/// </summary>
		/// <returns>Return the URL for the server </returns>
		public static string GetHomeServerForUser( string username, string password )
		{
//
		        string publicUrl;
			try
			{
			        Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);

				// find user
				Member member = domain.GetMemberByName( username );

				if (member == null) throw new UserDoesNotExistException( username );

				HostNode hNode = member.HomeServer;

				if ( hNode == null )
				{
				        //User still not provisioned. Talk to Master Server.
				        //Note : User provisioning is done only in master!!

				        iFolderServer MasterServer = GetMasterServer();
						//Do we really need this???
					ServicePointManager.CertificatePolicy = new WebCertificatePolicy();
					DomainService domainService = new DomainService();

					domainService.Url = MasterServer.PublicUrl + "/DomainService.asmx";

					UTF8Encoding utf8Name = new UTF8Encoding();
	                                byte[] encodedCredsByteArray = utf8Name.GetBytes(username);
        	                        string iFolderUserBase64 = Convert.ToBase64String(encodedCredsByteArray);

					encodedCredsByteArray = utf8Name.GetBytes(password);
	                                string iFolderPassBase64 = Convert.ToBase64String(encodedCredsByteArray);

					domainService.Credentials = new NetworkCredential(iFolderUserBase64, iFolderPassBase64);
					domainService.PreAuthenticate = true;

					publicUrl = domainService.GetHomeServer( username ).PublicAddress;
				} else {
				        //Yay!! User already provisioned.
				        publicUrl = hNode.PublicUrl;
				}


			}
			catch ( Exception ex )
			{
			        throw (ex);
			}

			return publicUrl;
		}

                /// <summary>
                /// Get the HomeServer URL for User.
                /// </summary>
                /// <returns>Return the URL for the server </returns>
                public static string GetHomeServerURLForUserID( string userid )
                {
                        string serverUrl = null;
                        try
                        {
                                Store store = Store.GetStore();
                                Domain domain = store.GetDomain(store.DefaultDomain);

                                // find user
                                Member member = domain.GetMemberByID( userid );

                                if (member == null) throw new UserDoesNotExistException( userid );

                                HostNode hNode = member.HomeServer;

                                if ( hNode != null )
                                        serverUrl = hNode.PublicUrl;
                        }
                        catch ( Exception ex )
                        {
                                        throw (ex);
                        }

                        return serverUrl;
                }

		/// <summary>
		/// Get the iFolder Home Server Information Object
		/// </summary>
		/// <returns>An iFolder Server Object</returns>
		public static iFolderServer GetHomeServer()
		{
			return new iFolderServer();
		}

		/// <summary>
		/// Get the iFolder Server Information Objects
		/// </summary>
		/// <returns>An Array of iFolder Server Object</returns>
		public static iFolderServer[] GetServers()
		{
			iFolderServerSet list = GetServersByName ( iFolderServerType.All, SearchOperation.BeginsWith, "", 0, 0);

			return list.Items;
		}

		/// <summary>
		/// Get an iFolder Server Information object.
		/// </summary>
		/// <param name="serverID">The Server ID</param>
		/// <returns>An iFolderServer Object</returns>
		public static iFolderServer GetServer(string serverID)
		{
			Store store = Store.GetStore();

			// use host id
			HostNode host = HostNode.GetHostByID(store.DefaultDomain, serverID);

			// check username also
			if (host == null) host = HostNode.GetHostByName(store.DefaultDomain, serverID);

			// not found
			if (host == null) throw new ServerDoesNotExistException(serverID);

			// server
			return new iFolderServer(host);
		}

		/// <summary>
		/// Get an iFolder Server Information object by Name
		/// </summary>
		/// <param name="serverName">The iFolder Server Name</param>
		/// <returns>An iFolder Server Object</returns>
		public static iFolderServer GetServerByName(string serverName)
		{
			Store store = Store.GetStore();
			
			HostNode host = HostNode.GetHostByName(store.DefaultDomain, serverName);

			// not found
			if (host == null) throw new ServerDoesNotExistException(serverName);

			// server
			return new iFolderServer(host);
		}

		/// <summary>
		/// Get iFolder Servers by Name
		/// </summary>
		/// <param name="type">iFolder Server Type</param>
		/// <param name="operation">The Search Operation</param>
		/// <param name="pattern">The Search Pattern</param>
		/// <param name="index">The Search Start Index</param>
		/// <param name="max">The Search Max Count of Results</param>
		/// <returns>A Set of iFolder Server Objects</returns>
		public static iFolderServerSet GetServersByName(iFolderServerType type, SearchOperation operation, string pattern, int index, int max)
		{
			//bool isMaster = ((type == iFolderServerType.Master) || (type == iFolderServerType.All));
			//bool isLocal = ((type == iFolderServerType.Local) || (type == iFolderServerType.All));

			Store store = Store.GetStore();

			// domain
			Domain domain = store.GetDomain(store.DefaultDomain);

			// search operator
			SearchOp searchOperation;

			switch(operation)
			{
				case SearchOperation.BeginsWith:
					searchOperation = SearchOp.Begins;
					break;

				case SearchOperation.EndsWith:
					searchOperation = SearchOp.Ends;
					break;

				case SearchOperation.Contains:
					searchOperation = SearchOp.Contains;
					break;

				case SearchOperation.Equals:
					searchOperation = SearchOp.Equal;
					break;

				default:
					searchOperation = SearchOp.Contains;
					break;
			}
			

			Simias.Storage.SearchPropertyList SearchPrpList = new Simias.Storage.SearchPropertyList();
			SearchPrpList.Add(BaseSchema.ObjectName, pattern, searchOperation);
			SearchPrpList.Add(BaseSchema.ObjectType, NodeTypes.MemberType, SearchOp.Equal);
			SearchPrpList.Add(PropertyTags.Types, HostNode.HostNodeType, SearchOp.Equal);
			ICSList searchList = domain.Search(SearchPrpList);
			DateTime t1= DateTime.Now;
			SearchState searchState = new SearchState( domain.ID, searchList.GetEnumerator() as ICSEnumerator, searchList.Count );
			int total = searchList.Count;	
			int i = 0;
			if(index > 0)
				searchState.Enumerator.SetCursor(Simias.Storage.Provider.IndexOrigin.SET, index);
			Member member = null;
			ArrayList list = new ArrayList();
			foreach(ShallowNode sn in searchList)
			{
				if(max != 0 && i++ >= max )
					break;
				member = new Member(domain, sn);
				HostNode node = new HostNode(member);
				list.Add(new iFolderServer(node));
			}

			return new iFolderServerSet(list.ToArray(typeof(iFolderServer)) as iFolderServer[], i);
		}
		
        /// <summary>
        /// Trust all certificates
        /// </summary>
        internal class TrustAllCertificatePolicy : ICertificatePolicy
        {
                #region ICertificatePolicy Members

                public bool CheckValidationResult(ServicePoint srvPoint,
                        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        WebRequest request, int certificateProblem)
                {
                        // Accept all, since there is no way to validate other than the user
                        return true;
                }

                #endregion
        }

	}
	/// <summary>
	/// An iFolder Server
	/// </summary>
	[Serializable]
	public class LdapInfo
	{
		/// <summary>
		/// Gets/sets the host.
		/// </summary>
	        public string Host;

		/// <summary>
		/// Gets/sets the contexts that are searched when provisioning users.
		/// </summary>
	        public string SearchContexts;

		/// <summary>
		/// Gets/sets the host.
		/// </summary>
	        public string ProxyDN;

		/// <summary>
		/// Gets/sets the Master Url.
		/// </summary>
    		public string MasterURL;

		/// <summary>
		/// Proxy User password.
		/// </summary>
	        public string ProxyPassword;

		/// <summary>
		/// Gets/sets a value indicating if SSL is being used.
		/// </summary>
	        public bool SSL;
	    
		/// <summary>
		/// Constructor
		/// </summary>
		public LdapInfo () 
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="server">HostNode object</param>

		public LdapInfo ( LdapSettings ldapSettings)
		{
		        this.Host = ldapSettings.Host;
			this.ProxyDN = ldapSettings.ProxyDN;
			this.ProxyPassword = ldapSettings.ProxyPassword;
			this.SSL = ldapSettings.SSL;
			this.MasterURL = ldapSettings.MasterURL;

			this.SearchContexts = "";
			foreach(string context in ldapSettings.SearchContexts)
			{
				SearchContexts += (context + "#");
			}

		}
	}
	/// <summary>
        /// An Volume Result Set
        /// </summary>
        [Serializable]
        public class VolumesList
        {
                /// <summary>
                /// An Array of Volumes
                /// </summary>
                public Volumes[] ItemsArray;

                /// <summary>
                /// The Total Number of Volumes
                /// </summary>
                public int NumberOfVolumes;

                /// <summary>
                /// Default Constructor
                /// </summary>
                public VolumesList()
                {
                }

                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="items"></param>
                /// <param name="total"></param>
                public VolumesList( Volumes[] itemsarray, int numberofvolumes )
                {
                        this.ItemsArray = itemsarray;
                        this.NumberOfVolumes = numberofvolumes;
                }
		

        }
	
	/// <summary>
        /// A DataPath for the server
        /// </summary>
        [Serializable]
        public class Volumes 
        {
                /// <summary>
                /// Name of the DataPath
                /// </summary>
                public string DataPath;

                /// <summary>
                /// Fullpath for the datapath
                /// </summary>
                public string FullPath;

		/// <summary>
                /// Fullpath for the datapath
                /// </summary>
                public long AvailableFreeSpace;

		/// <summary>
                ///Status of the datapath
                /// </summary>
                public bool Enabled;


                /// <summary>
                /// Default Constructor
                /// </summary>
                public Volumes()
                {
                }

                /// <summary>
                /// Constructor
                /// </summary>
                /// <param name="datastore"></param>
                public Volumes(DataStore datastore)
                {
                        this.DataPath = datastore.DataPath;
                        this.FullPath = datastore.FullPath;
			this.AvailableFreeSpace = datastore.AvailableFreeSpace;
			this.Enabled = datastore.Enabled;
                }
		
		/// <summary>
                /// Gets an array of datastore of an iFolder Server.
                /// </summary>
                /// <returns>Bool true on success.</returns>
                public static VolumesList GetVolumes(int index, int max)
                {
			int ItemIndex = 0;
                        DataStore[] datastore = DataStore.GetVolumes();
                        ArrayList DataStoreList = new ArrayList();
                        foreach(DataStore item in datastore)
                        {
				
				if (( ItemIndex >= index ) && ItemIndex < (max + index))
				{
                                	DataStoreList.Add(new Volumes(item));
				}
				++ItemIndex;
                        }
                        return new VolumesList(DataStoreList.ToArray(typeof(Volumes)) as Volumes[], ItemIndex);
                }
	}

        /// <summary>
        /// Single Certificate Policy
        /// </summary>
        internal class WebCertificatePolicy : ICertificatePolicy
        {

                /// <summary>
                /// Check Validation Result
                /// </summary>
                /// <param name="srvPoint"></param>
                /// <param name="certificate"></param>
                /// <param name="request"></param>
                /// <param name="certificateProblem"></param>
                /// <returns></returns>
                public bool CheckValidationResult( ServicePoint srvPoint,
                        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        WebRequest request,
                        int certificateProblem )
                {
			//This needs validation, but against what??? For now use system default
                        return true;
                }
        }

}
 

