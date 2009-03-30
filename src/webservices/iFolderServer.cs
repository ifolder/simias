/*****************************************************************************
* Copyright © [2007-08] Unpublished Work of Novell, Inc. All Rights Reserved.
*
* THIS IS AN UNPUBLISHED WORK OF NOVELL, INC.  IT CONTAINS NOVELL'S CONFIDENTIAL, 
* PROPRIETARY, AND TRADE SECRET INFORMATION.	NOVELL RESTRICTS THIS WORK TO 
* NOVELL EMPLOYEES WHO NEED THE WORK TO PERFORM THEIR ASSIGNMENTS AND TO 
* THIRD PARTIES AUTHORIZED BY NOVELL IN WRITING.  THIS WORK MAY NOT BE USED, 
* COPIED, DISTRIBUTED, DISCLOSED, ADAPTED, PERFORMED, DISPLAYED, COLLECTED,
* COMPILED, OR LINKED WITHOUT NOVELL'S PRIOR WRITTEN CONSENT.  USE OR 
* EXPLOITATION OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE 
* PERPETRATOR TO CRIMINAL AND  CIVIL LIABILITY.
*
* Novell is the copyright owner of this file.  Novell may have released an earlier version of this
* file, also owned by Novell, under the GNU General Public License version 2 as part of Novell's 
* iFolder Project; however, Novell is not releasing this file under the GPL.
*
*-----------------------------------------------------------------------------
*
*                 Novell iFolder Enterprise
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

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Server;
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

		/// <summary>
		/// Get LDAP details
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
		/// Get the HomeServer URL for User.
		/// </summary>
		/// <returns>Return the URL for the server </returns>
		public static string[] GetReports ()
		{
			Store store = Store.GetStore();
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

		public static string GetSimiasRequiresSSLStatus()
		{
			string SectionTag = "section";
                        string SettingTag = "setting";
                        string NameAttr = "name";
                        string ValueAttr = "value";
			string AuthenticationSection = "Authentication";
			string SimiasSSLKey = "SimiasRequireSSL";

			string requiressl = null;
			Store store = Store.GetStore();
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

        	public static string[] GetLogLevels ()
		{
		        string[] loglevels = new string[2];
			Store store = Store.GetStore();
			string log4netConfigurationPath = Path.Combine ( Store.StorePath, "Simias.log4net");

			XmlDocument configDoc = new XmlDocument ();
			configDoc.Load (log4netConfigurationPath);

			loglevels [(int)LoggerType.RootLogger] = GetXmlKeyValue (configDoc, xpathRootLogger, "value");
			loglevels [(int)LoggerType.AccessLogger] = GetXmlKeyValue (configDoc, xpathAccessLogger, "value");

			return loglevels;
		}


		/// <summary>
		/// Get the HomeServer URL for User.
		/// </summary>
		/// <returns>Return the URL for the server </returns>
	    public static void SetLogLevel (LoggerType loggerType, string logLevel)
		{
			Store store = Store.GetStore();
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


        private static string GetXmlKeyValue( XmlDocument xmldoc, string xpath, string attribute )
		{
			XmlElement xmlElement = xmldoc.DocumentElement.SelectSingleNode( xpath ) as XmlElement;
			return xmlElement.GetAttribute (attribute);
		}

	        private static void SetXmlKeyValue( XmlDocument xmldoc, string xpath, string attribute, string value )
		{
			XmlElement xmlElement = xmldoc.DocumentElement.SelectSingleNode( xpath ) as XmlElement;
			xmlElement.SetAttribute (attribute, value);
		}

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
			Store store = Store.GetStore();
			if (!privateUrl.ToLower().StartsWith(Uri.UriSchemeHttp))
                        {
                                privateUrl = (new UriBuilder(Uri.UriSchemeHttp, privateUrl)).ToString();
                        }
			if (!publicUrl.ToLower().StartsWith(Uri.UriSchemeHttp))
                        {
                                publicUrl = (new UriBuilder(Uri.UriSchemeHttp, publicUrl)).ToString();
                        }
			
			// adding /simias10
			privateUrl = AddVirtualPath( privateUrl );
			publicUrl = AddVirtualPath( publicUrl );
		

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
                                }			

				// Commit the config file changes.
        	                CommitConfiguration( document , SimiasConfigFilePath);
				UpdateStatus = true;
			}
			catch(Exception ex)
			{
				SmartException.Throw(ex);
			}
			return UpdateStatus;	
		}
		private static string AddVirtualPath( string path )
                {
                        path = path.TrimEnd( '/' );
                        if ( path.EndsWith( "/simias10" ) == false )
                        {
                                path += "/simias10";
                        }

                        return path;
                }
		
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
				SmartException.Throw(ex);
			}
			return true;
		}

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

		private static void SetupSSLForMaster(string masterAddress)
                {
                        if (masterAddress.ToLower().StartsWith(Uri.UriSchemeHttps))
                        {
				string webPath = Path.GetFullPath("../../../../lib/simias/web");
                                // swap policy
                                ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();

                                // connect
                                HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(masterAddress);

                                try
                                {
                                        request.GetResponse();
                                }
                                catch
                                {
                                        // ignore
                                }

                                // restore policy

                                // service point
                                ServicePoint sp = request.ServicePoint;
				if(sp == null) throw new Exception("sp is null for master "+masterAddress);
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

                                                //Console.WriteLine("Done");
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
					domainService.Credentials = new NetworkCredential(username, password);
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
			bool isMaster = ((type == iFolderServerType.Master) || (type == iFolderServerType.All));
			bool isLocal = ((type == iFolderServerType.Local) || (type == iFolderServerType.All));

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
			
			ICSList members = domain.Search(BaseSchema.ObjectName, pattern, searchOperation);

			// build the result list
			ArrayList list = new ArrayList();
			int i = 0;

			foreach(ShallowNode sn in members)
			{
			  try
			  {
				// throw away non-members
				if (sn.IsBaseType(NodeTypes.MemberType))
				{
					Member member = new Member(domain, sn);

					if (member.IsType(HostNode.HostNodeType))
					{
						HostNode node = new HostNode(member);

					        if ((i >= index) && ((max <= 0) || i < (max + index)))
						    //&& ((isMaster && node.IsMasterHost) || (isLocal && node.IsLocalHost)))
						{
							list.Add(new iFolderServer(node));
						}

						++i;
					}
				}
			  }
			  catch {
				//ignore
			  }
			}

			return new iFolderServerSet(list.ToArray(typeof(iFolderServer)) as iFolderServer[], i);
		}
		
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
 

