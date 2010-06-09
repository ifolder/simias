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
*                 Novell iFolder Enterprise
*
*-----------------------------------------------------------------------------
*
*                 $Author: Anil Kumar (kuanil@novell.com)
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <build an exe which will support proxy user functionality. Written for common proxy user support>
*
*
*******************************************************************************/
using System;
using System.Net;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Security.Cryptography;
using System.Diagnostics;

using Novell.Directory.Ldap;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.LdapProvider;
using Simias.Storage.Provider;

public class GrantRights
{
	private static LdapConnection connection;

	private static string LogFileName = null;//"/var/opt/novell/log/proxymgmt/pxymgmt.log" ;
	static	FileStream file; 
	static	TextWriter tw = null;

	private static string SimiasDataPath = null;
	private static readonly string StoreProviderSection = "StoreProvider";
	private static readonly string CPLogFileKey = "CommonProxyLogPath";
	private static readonly string ServerSection = "Server";
	private static readonly string EnterpriseDomainSection = "EnterpriseDomain";
	private static readonly string SystemNameKey = "SystemName";
	private static readonly string PublicIPKey = "PublicAddress";

	private static string FnRightsAssignment = "proxy_rights_assign";
	private static string FnStoreProxyCreds = "update_proxy_cred_store";
	private static string FnRetrieveProxyCreds = "retrieve_proxy_creds";
	private static string UserName = "username";
	private static string PassWord = "password";

	private static string WSGetProxyInfo = "GetProxyInfo";
	private static string SimiasConf = "Simias.config";

	private static int SSLPort = 636;
	private static int NonSSLPort = 389;

	private static string proxyfilename = "ldapdetails";

	static string cpHost = IPAddress.Loopback.ToString();
	static int cpPort = -1;

	static string IFLdapHost = null;
	static bool IFLdapSSL = false;
	static string IFProxyUserDN = null;
	static string IFContext = null;

	static int Main( string[] args)
	{

		// ReadConfigDetails() -- It will read the ldap details from 'proxyfile' and store the result in loccal variables.
		// Connect() -- It is used to give rights to proxy user that is sent by common proxy framework. Internally, it checks whether ldap configured
		// 		with iFolder is e-dir and if the treename matches with the treename of localhsot. Then it will give rights to the proxy
		// 		user that is passed by common proxy framework.
		// IseDirectory() -- tells whether the current connection passed as parameter is e-dir or not. if last parameter is not null, then it will
		//			match the tree name also (passed as 2nd parameter), and update the last parameter with the bool value (true if 
		//			treename matches)	
		// GetTreeName() -- Get the treename for current ldap connection passed as parameter. returns null if it is not e-dir or error.
		
		int failval = -1;
		int successval = 0;
		int rval = -1;

		try
		{
			if( args.Length > 1 && !String.IsNullOrEmpty(args[0]))
				SimiasDataPath = args[0];
			else
				return failval;

			Configuration SimiasConfig = new Configuration( SimiasDataPath, true) ;
	
			LogFileName = SimiasConfig.Get( StoreProviderSection, CPLogFileKey );
			if( LogFileName == null)
			{
				tw.WriteLine("iFolder {0}- Failed to get the common proxy log file path from config file.",DateTime.Now.ToString());
				return failval;
			}

			string logdirName = Path.GetDirectoryName( LogFileName );
			if( logdirName == null || !Directory.Exists( logdirName))
			{
				return failval;
			}

			file = new FileStream( LogFileName, FileMode.Append, FileAccess.Write, FileShare.Write);
			tw = new StreamWriter(file);
			int ArgIndex = 1;

			GetLdapDetails();
	
			if( String.Equals( args[ArgIndex], FnRightsAssignment))
			{
				try
				{
					if( args.Length > 2)
					{
						ArgIndex++;
						cpPort = Convert.ToInt32( args[ ArgIndex ] );
					}
					else
					{
						cpPort = SSLPort;
					}
					rval = AssignProxyRights();
				}catch(Exception ex2)
				{
					tw.WriteLine("iFolder {0}- caught exception while assigning proxy rights : {1}",DateTime.Now.ToString(), ex2.Message);
					DeleteProxyFile();
					rval = -1;
				}
				return rval;
			}
			else if( String.Equals(args[ArgIndex], FnStoreProxyCreds) )
			{
				try
				{
					//if( args.Length == 3)
					if( args.Length > 2)
					{
						ArgIndex++;
						cpPort = Convert.ToInt32( args[ ArgIndex ] );
					}
					else
					{
						cpPort = SSLPort;
					}		
					rval = UpdateProxyDetails( );
				}catch(Exception ex3)
				{
					tw.WriteLine("iFolder {0}- caught exception while assigning proxy rights : {1}",DateTime.Now.ToString(), ex3.Message);
					DeleteProxyFile();
					rval = -1;
				}
				return rval;
			}
			else if( String.Equals( args[ArgIndex], FnRetrieveProxyCreds ))
			{
				bool GetUserName = false;
				bool GetPassword = false;
	
	
				// If they want proxy credentials then 2nd parameter will be either username/password. 
				ArgIndex++;
				if( args.Length < 4)
				{
					tw.WriteLine("iFolder {0}- less number of arguments passed. Please check the number of arguments. ",DateTime.Now.ToString());
					tw.Close();
					return failval;
				}
	
				if( String.Equals( args[ArgIndex], UserName ) )
				{
					GetUserName = true;
				}
				else if( String.Equals( args[ArgIndex], PassWord ) )
				{
					GetPassword = true;
				}
				else 
				{
					tw.WriteLine("iFolder {0}- Either specify username or password to retrieve ",DateTime.Now.ToString());
					return failval;
	
				}
				ArgIndex++;
				string ProxyWritePath = null;
				if( args[ ArgIndex ] != null )
				{
					ProxyWritePath = args[ArgIndex]; 
				}
				if( ProxyWritePath == null)
				{
					return failval;
				}
				
				if( args.Length >= 5 )
				{
					cpPort = Convert.ToInt32( args[ ++ArgIndex ] );
				}
				else
				{
					cpPort = SSLPort;
				}

				string retval = null;
				FileStream pfile;
				TextWriter ptw = null;
				try
				{
					retval = GetProxyCreds( GetUserName, GetPassword );
	
					string directoryName = Path.GetDirectoryName(ProxyWritePath);
					if( retval == null || directoryName == null || !Directory.Exists( directoryName))
					{
						return failval;
					}
	
					if( File.Exists( ProxyWritePath ))
					{
						File.Delete( ProxyWritePath );
					}
					pfile = new FileStream( ProxyWritePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
					ptw = new StreamWriter(pfile);
					ptw.WriteLine(retval);
				}catch(Exception ex)
				{
					DeleteProxyFile();
	
					tw.WriteLine("iFolder {0}- caught exception while fetching proxy credential : {1}",DateTime.Now.ToString(), ex.Message);
					return failval;
				}
				finally
				{
					if( ptw != null )
						ptw.Close();
				}
	
				tw.WriteLine("iFolder {0}- Retrieving Proxy credentials: Success ",DateTime.Now.ToString());
				return successval;
			}
		}
		catch( Exception MainEx)
		{
			tw.WriteLine("iFolder {0}- caught exception in Main : {1}",DateTime.Now.ToString(), MainEx.Message);
			return failval;
		}
		finally
		{
			if (tw != null)
				tw.Close();
		}

		// Since common proxy is expected to run on localhost, so we take 127.0.0.1 for getting local tree name.

		return successval;
	}

	// make webservice call so that ifolder server assigns rights to proxy user
	static int AssignProxyRights( )
	{
		bool TreeMatched =  CheckLdapTreeMatch();
		if(TreeMatched == false)
		{
			tw.WriteLine("iFolder {0}- Assigning rights to proxy user: The treenames of iFolder ldap tree and OES server does not match: Error ",DateTime.Now.ToString());
			
			return -1;
		}

		// since the tree name has matched, so proceed 

		AssignRights( );

		DeleteProxyFile();
	
		tw.WriteLine("iFolder {0}- Assigning rights to proxy user: Success ",DateTime.Now.ToString());
		return 0;

	}

	// Go to iFolder store and write these proxy user details
	static int UpdateProxyDetails()
	{

		// We have proxy user name and prox password. Update the same in Simias.config and ppf file
		// Only if it is in the same tree.

		bool TreeMatched =  CheckLdapTreeMatch();
		if(TreeMatched == false)
		{
			tw.WriteLine("iFolder {0}- Updating proxy creds to store: The treenames of iFolder ldap tree and OES server does not match: Error ",DateTime.Now.ToString());
			DeleteProxyFile();
			return -1;
		}

                // If the ldap IP of iFolder is some AD or non-edir then return immediately.

		ServiceProxyRequests();

		DeleteProxyFile();

		tw.WriteLine("iFolder {0} - Updating Proxy credentials to store: Success ",DateTime.Now.ToString());
		return 0;
	}

	// based on the boolean parameter, it will return either username or password
	static string GetProxyCreds( bool ProxyDN, bool ProxyPwd )
	{
		bool TreeMatched =  CheckLdapTreeMatch();
		if(TreeMatched == false)
		{
			tw.WriteLine("iFolder {0}- Retrieving proxy credentials: The treenames of iFolder ldap tree and OES server does not match: Error ",DateTime.Now.ToString());
			DeleteProxyFile();
			return null;
		}

		string iFProxyPwd = null;

		if( ProxyDN == true)
		{
			return IFProxyUserDN; 
		}
		else if( ProxyPwd == true)
		{
			// call the webservice method which will write the proxypwd into a file.
			try
			{
				WebCallMethod( WSGetProxyInfo );
				ReadConfigDetails( out iFProxyPwd);
			}
			catch( Exception ex)	
			{
				DeleteProxyFile();
				if( ex.Message.IndexOf( "ConnectFailure" ) >= 0)
				{
					tw.WriteLine("iFolder : iFolder Server not running, now it will try to get credentials directly from simias store");
					iFProxyPwd = getProxyPwd();
				}
				
			}
			return iFProxyPwd;
		}
		return null;
	}


	// check whether ldap tree used by ifolder is same as the local cp host IP
	static bool CheckLdapTreeMatch()
	{

		string treeName = null;

		// If the ldap IP of iFolder is some AD or non-edir then return immediately.

		connection = new LdapConnection();

		int iFLdapPort;
		if( IFLdapSSL == true)
		{
			iFLdapPort = SSLPort;
			connection.SecureSocketLayer = true;
		}
		else 	
		{
			iFLdapPort = NonSSLPort;
			connection.SecureSocketLayer = false;
		}

		try
		{
			connection.Connect( IFLdapHost, iFLdapPort);
		}
		catch (Exception ex)
		{
			tw.WriteLine("iFolder {0}- Could not connect to iFolder configured ldap host ",DateTime.Now.ToString());
		}

		if( !IseDirectory( connection, null, out treeName ) || treeName == null )
		{
			DeleteProxyFile();
			return false;
		}

		// Now see if the tree which corresponds the cpHost is same as the tree that corresponds to iFolder ldap configured one.

		LdapConnection cpConnection = new LdapConnection();
		bool sslUsed = false;
		if( cpPort == SSLPort)
		{
			connection.SecureSocketLayer = true;
			sslUsed = true;
		}
		else 	
		{
			connection.SecureSocketLayer = false;
		}
		
		try
		{
			cpConnection.Connect(cpHost, cpPort);
		}
		catch
		{
			// Fall back to other port which was used
			if( sslUsed )
			{
				connection.SecureSocketLayer = false;
				cpPort = NonSSLPort; 
				cpConnection.Connect(cpHost, cpPort);
			}
			else
			{
				connection.SecureSocketLayer = true;
				cpPort = SSLPort;
				cpConnection.Connect(cpHost, cpPort);
			}	
		}

		string cptreeName = null;

		if( !IseDirectory( connection, cptreeName, out cptreeName ) || cptreeName == null)
		{
			// local host is not e-dir, it will return.. 
			DeleteProxyFile();
			return false;
		}


		if( !String.Equals( cptreeName, treeName) )
		{
			// treenames do not match, return false
			DeleteProxyFile();
			return false;
		}

		try	
		{
			connection.Disconnect();
			cpConnection.Disconnect();
		}
		catch{}
		return true;
			
	}

	static void DeleteProxyFile()
	{
		string proxyfile = Path.Combine( SimiasDataPath, proxyfilename) ;
		if (File.Exists(proxyfile))
			File.Delete(proxyfile);
	}

	static void GetLdapDetails()
	{
		LdapSettings ldapSettings = LdapSettings.Get ( SimiasDataPath );
		IFLdapHost = ldapSettings.Host;
		IFLdapSSL = ldapSettings.SSL;
		IFProxyUserDN = ldapSettings.ProxyDN;
		string allContexts = "";
		foreach( string context in ldapSettings.SearchContexts)
		{
			allContexts = allContexts + context + "#";
		}
		IFContext = allContexts;
	}

	// this function will make a webservice call to iFolder Admin
	static void WebCallMethod(string MethodName)
	{

		HttpWebRequest HttpWReq;
		HttpWebResponse HttpWResp = null;

		Configuration SimiasConfig = new Configuration( SimiasDataPath, true) ;
	
		string ServerAddress = SimiasConfig.Get( ServerSection, PublicIPKey );
		if( ServerAddress == null)
		{
			tw.WriteLine("iFolder {0}- Failed to get the iFolder server address from config file.",DateTime.Now.ToString());
			return;
		}
		try
		{
			string req = ServerAddress + Path.AltDirectorySeparatorChar + "iFolderAdmin.asmx" + Path.AltDirectorySeparatorChar;

			req += (MethodName +"?");
			
			StringBuilder Request = new StringBuilder(req);
			ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
			
			HttpWReq = (HttpWebRequest)WebRequest.Create(Request.ToString());
			HttpWReq.Headers.Add("SOAPAction", MethodName);
			HttpWReq.Method = "GET";
			HttpWResp = (HttpWebResponse)HttpWReq.GetResponse();
			if (HttpWResp.StatusCode != HttpStatusCode.OK)
				tw.WriteLine("iFolder {0}- WebRequest Failed with status not OK. ",DateTime.Now.ToString());
		}
		catch
		{
			//tw.WriteLine("iFolder {0}- WebRequest Failed with Exception: {1} ",DateTime.Now.ToString(), ex.Message);
			throw;
		}

	}

	// Function to read ldap scheme, host and port from Simias.config
	static void ReadConfigDetails( out string iFProxyPwd)
	{

		iFProxyPwd = null;

		string fileName = Path.Combine( SimiasDataPath, proxyfilename );

		string readcontents;         
		StreamReader FileStream; 
		FileStream = File.OpenText(fileName);
		readcontents = FileStream.ReadLine();
		while(readcontents != null)
    		{

			string [] KeyAndValue = readcontents.Split(':');
			if( KeyAndValue != null && KeyAndValue.Length > 1 )
			{
				if(KeyAndValue[0] == "proxypwd")
				{
					iFProxyPwd = KeyAndValue[1];
	
				}
			}

			readcontents = FileStream.ReadLine();
    		}
		FileStream.Close();
	}
		


	// Function will call admin webservice to assign rights. Prerequisite is : The Details for assigning rights must be present in a file which will be
	// used during webservice function call.
	public static void AssignRights( )
	{

		// If the ldap IP of iFolder is some AD or non-edir then return immediately.
		ServiceProxyRequests();

	}

		// returns whether connection is e-dir or not. Also updates treeMatched with true or false depending on whether dirTreeName matches with the
		// tree name corresponding to connections's treeName
                public static bool IseDirectory(LdapConnection connection, string dirTreeName, out string treeName)
                {

                        LdapAttribute attr      = null;
                        LdapEntry entry = null;
                        bool eDirectory = false;
			treeName = null;
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
                                        continue;
                                }
                                LdapAttributeSet attributeSet = entry.getAttributeSet();
                                System.Collections.IEnumerator ienum =  attributeSet.GetEnumerator();
                                while(ienum.MoveNext())
                                {
                                        attr = (LdapAttribute)ienum.Current;
                                        string attributeName = attr.Name;
                                        string attributeVal = attr.StringValue;
					if( String.Equals(attributeName, "directoryTreeName"))
					{
						treeName = attributeVal;
					}

                                        if( String.Equals(attributeVal, "Novell, Inc.")==true )     
                                        {
                                                eDirectory = true;
                                        }
                                }
                        }
			return eDirectory ; 
                }

		public static void ServiceProxyRequests()
		{
			const string FnStoreProxyCreds = "update_proxy_cred_store";
			const string FnRightsAssignment = "proxy_rights_assign";
			bool Proxy_Rights_Assign = false;
			bool Proxy_Creds_Store = false;
			string proxydn = null;	
			string proxypwd = null;
			string ldapadmindn = null;
			string ldapadminpwd = null;
			string proxyfilename2 = "proxydetails";
			string filename = Path.Combine( SimiasDataPath, proxyfilename2);
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

			// now based on service request, either assign the rights or update the proxy credentials into store
			if (Proxy_Rights_Assign)
			{
				LdapConnection connection = new LdapConnection();
				connection.SecureSocketLayer = IFLdapSSL ? true : false ;
				int port = IFLdapSSL ? 636 : 389;
				//connection.Connect( host, port);
				connection.Connect( IFLdapHost, port);
				connection.Bind( ldapadmindn, ldapadminpwd);
				LdapAttribute attribute = new LdapAttribute("acl", new String[]
				{
					String.Format("1#subtree#{0}#[Entry Rights]", proxydn),
					String.Format("3#subtree#{0}#[All Attributes Rights]", proxydn)
				});
				LdapModification modification = new LdapModification(LdapModification.ADD, attribute);

			string [] KeyAndValue = IFContext.Split('#');
			if( KeyAndValue != null && KeyAndValue.Length > 1 )
			{
				// get the search contexts in an array or some enumerator
				for( int count = 0; count < KeyAndValue.Length; count++)
				{
					if( String.IsNullOrEmpty( KeyAndValue[count] ))
						continue;
					try
					{
						connection.Modify( KeyAndValue[count], modification);
					}
					catch(Exception ex)
					{
						if( ex.Message.IndexOf("Attribute Or Value Exists") >= 0)
							tw.WriteLine("iFolder: RightsAssignment, The rights were already provided earlier, so ignore");
						else
							throw ex;
					}
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
				string path = Path.GetFullPath(Path.Combine( SimiasDataPath, "Simias.config"));
				XmlDocument doc = new XmlDocument();
				doc.Load(path);

				string str = string.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", "section", "name", "LdapAuthentication", "setting", "ProxyDN" );

				XmlElement element = ( XmlElement )doc.DocumentElement.SelectSingleNode( str );
				if ( element != null )
				{

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
				string path1 = Path.Combine(SimiasDataPath, ppwdfile);
				FileStream file = new FileStream( path1, FileMode.OpenOrCreate, FileAccess.ReadWrite);
				TextWriter ptw = new StreamWriter(file);
				ptw.WriteLine( proxypwd );
				tw.WriteLine("method: proxycredstore: wrote the creds into  file");
				ptw.Close();

				if (Execute("chown", "wwwrun:www {0}", path1) != 0)
					tw.WriteLine("method: proxycredstore: Unable to set wwwrun as owner of {0} file. Please do it manually",path1);

				if (Execute("chmod", "600 {0}", path1) != 0)
					tw.WriteLine("method: proxycredstore: Unable to set rights 600 to {0} file. Please do it manually",path1);

	
			}
		}

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

	// open store and read the details
	public static string getProxyPwd()
	{
		Configuration SimiasConf = new Configuration( SimiasDataPath, true) ;

		string iFolderSystemName = SimiasConf.Get( EnterpriseDomainSection, SystemNameKey );

		Store.Initialize(SimiasDataPath, true , -1);
		Store store = Store.GetStore();
		Domain domain = store.GetDomain(store.DefaultDomain);

		string encodedCypher = domain.Properties.GetSingleProperty( "ProxyPassword" ).ToString();

		byte[] cypher = Convert.FromBase64String( encodedCypher );
		RSACryptoServiceProvider credential = store.CurrentUser.Credential;
		string password = new UTF8Encoding().GetString( credential.Decrypt( cypher, false ) );

		Store.DeleteInstance();

		return password;
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
