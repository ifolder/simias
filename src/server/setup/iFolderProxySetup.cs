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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
//using Simias;
using Novell.Directory.Ldap;
//using Novell.iFolder.Enterprise.Web;
//using iFolder.WebService;

public class GrantRights
{
	private static LdapConnection connection;

	private static string logfilename = "/var/opt/novell/log/proxymgmt/pxymgmt.log" ;

	static int Main( string[] args)
	{

		// WriteInFile() -- it will call webservice and write the ldap details to %DATAPATH/proxyfile.
		// ReadModMonoConfiguration() -- It will return the data path by reading /etc/apache2/conf.d/simias.conf.
		// ReadConfigDetails() -- It will read the ldap details from 'proxyfile' and store the result in loccal variables.
		// Connect() -- It is used to give rights to proxy user that is sent by common proxy framework. Internally, it checks whether ldap configured
		// 		with iFolder is e-dir and if the treename matches with the treename of localhsot. Then it will give rights to the proxy
		// 		user that is passed by common proxy framework.
		// IseDirectory() -- tells whether the current connection passed as parameter is e-dir or not. if last parameter is not null, then it will
		//			match the tree name also (passed as 2nd parameter), and update the last parameter with the bool value (true if 
		//			treename matches)	
		// GetTreeName() -- Get the treename for current ldap connection passed as parameter. returns null if it is not e-dir or error.
		
		bool FnGetProxyCreds = false;

		FileStream file; 
		TextWriter tw;

		for( int count = 0; count < args.Length; count++)
		{
			if( count == 0)
			{
				// We are dealing with 1st argument, that must be the servicename/function that is asked for
				if( args[count] == "retrieve_proxy_creds" )
				{
					FnGetProxyCreds = true;
				}
				else if( args[count] == "proxy_rights_assign")
				{
					int rval = -1;
					UpdateOldConfFile();		
					try
					{
						rval = AssignProxyRights( );
					}catch(Exception ex2)
					{
						file = new FileStream( logfilename, FileMode.Append);
						tw = new StreamWriter(file);
						tw.WriteLine("iFolder {0}- caught exception while assigning proxy rights : {1}",DateTime.Now.ToString(), ex2.Message);
						tw.Close();
						DeleteProxyFile();
						return rval;
					}
					file = new FileStream( logfilename, FileMode.Append);
					tw = new StreamWriter(file);
					tw.WriteLine("iFolder {0}- Assigning Proxy Rights: Success ",DateTime.Now.ToString());
					tw.Close();
					return rval;
				}
				else if( args[count] == "update_proxy_cred_store" )
				{
					int rval = -1;
					UpdateOldConfFile();		
					try
					{
						rval = UpdateProxyDetails( );
					}catch(Exception ex3)
					{
						file = new FileStream( logfilename, FileMode.Append);
						tw = new StreamWriter(file);
						tw.WriteLine("iFolder {0}- caught exception while assigning proxy rights : {1}",DateTime.Now.ToString(), ex3.Message);
						tw.Close();
						DeleteProxyFile();
						return rval;
					}
					file = new FileStream( logfilename, FileMode.Append);
					tw = new StreamWriter(file);
					tw.WriteLine("iFolder {0}- Updating proxy credentials to iFolder: Success ",DateTime.Now.ToString());
					tw.Close();
					return rval;
				}
				continue;
			}
			if(count == 1)
			{


				// 2nd arg is meant for options that come after fn name
				if( FnGetProxyCreds )
				{
					bool GetUserName = false;
					bool GetPassword = false;
		
					//FileStream file; 
					//TextWriter tw;

					// If they want proxy credentials then 2nd parameter will be either username/password. 
					if( args[count] == "username")
					{
						GetUserName = true;
					}
					else if( args[count] == "password")
					{
						GetPassword = true;
					}
					count++;
					string ProxyWritePath = null;
					if( args[ count ] != null )
					{
						ProxyWritePath = args[count]; 
					}
					if( ProxyWritePath == null)
					{
						Console.WriteLine(" Please specify the filepath where to write the proxy credentials.");
						return -1;
					}
					string retval = null;
					try
					{
						// First update the Simias.config file with AuthNotRequired flag
						UpdateOldConfFile();		
						retval = GetProxyCreds( GetUserName, GetPassword );

						string directoryName = Path.GetDirectoryName(ProxyWritePath);
						if( directoryName == null || !Directory.Exists( directoryName))
							return 0;

						file = new FileStream( ProxyWritePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
						tw = new StreamWriter(file);
						tw.WriteLine(retval);
						tw.Close();
					}catch(Exception ex)
					{
						//Console.WriteLine(" caught ex :");
						DeleteProxyFile();

						file = new FileStream( logfilename, FileMode.Append);
						tw = new StreamWriter(file);
						tw.WriteLine("iFolder {0}- caught exception while fetching proxy credential : {1}",DateTime.Now.ToString(), ex.Message);
						tw.Close();
						return -1;
					}

					file = new FileStream( logfilename, FileMode.Append);
					tw = new StreamWriter(file);
					tw.WriteLine("iFolder {0}- Retrieving Proxy credentials: Success ",DateTime.Now.ToString());
					tw.Close();


					return 0;
				}
				//else
				//	Console.WriteLine("Entered else");
			}	
		
		}

		

		// Since common proxy is expected to run on localhost, so we take 127.0.0.1 for getting local tree name.

		//Console.WriteLine("");
		//Console.WriteLine("Main ends");
		return 0;
	}

	static int AssignProxyRights( )
	{
		string cpHost = "127.0.0.1";
		int cpPort = 389;
		
		FileStream file;
		TextWriter tw;

		//Console.WriteLine("Assign Rights..started");
		bool TreeMatched =  CheckLdapTreeMatch( cpHost, cpPort);
		if(TreeMatched == false)
		{
			file = new FileStream( logfilename, FileMode.Append);
			tw = new StreamWriter(file);
			tw.WriteLine("iFolder {0}- Assigning rights to proxy user: The treenames of iFolder ldap tree and OES server does not match: Error ",DateTime.Now.ToString());
			tw.Close();
			
			return -1;
		}

		// since the tree name has matched, so proceed 

		AssignRights( );

		DeleteProxyFile();
	
		file = new FileStream( logfilename, FileMode.Append);
		tw = new StreamWriter(file);
		tw.WriteLine("iFolder {0}- Assigning rights to proxy user: Success ",DateTime.Now.ToString());
		tw.Close();
		return 0;

	}

	static int UpdateProxyDetails()//string cpProxyDN, string cpProxyPwd)
	{
		string cpHost = "127.0.0.1";
		int cpPort = 389;

		FileStream file;
		TextWriter tw;

		// We have proxy user name and prox password. Update the same in Simias.config and ppf file
		// Only if it is in the same tree.
		//Console.WriteLine("Update ProxyDetails to iFolder..started");

		bool TreeMatched =  CheckLdapTreeMatch( cpHost, cpPort);//, cpProxyDN, cpProxyPwd, null, null );
		if(TreeMatched == false)
		{
			file = new FileStream( logfilename, FileMode.Append);
			tw = new StreamWriter(file);
			tw.WriteLine("iFolder {0}- Updating proxy creds to store: The treenames of iFolder ldap tree and OES server does not match: Error ",DateTime.Now.ToString());
			tw.Close();
			DeleteProxyFile();
			return -1;
		}

                // If the ldap IP of iFolder is some AD or non-edir then return immediately.

		WebCallMethod( "ServiceProxyRequests");

		DeleteProxyFile();

		file = new FileStream( logfilename, FileMode.Append);
		tw = new StreamWriter(file);
		tw.WriteLine("iFolder {0} - Updating Proxy credentials to store: Success ",DateTime.Now.ToString());
		tw.Close();
		return 0;
	}

	static string GetProxyCreds( bool ProxyDN, bool ProxyPwd )
	{

		string cpHost = "127.0.0.1";
		int cpPort = 389;
		bool TreeMatched =  CheckLdapTreeMatch( cpHost, cpPort);
		if(TreeMatched == false)
		{
			FileStream file = new FileStream( logfilename, FileMode.Append);
			TextWriter tw = new StreamWriter(file);
			tw.WriteLine("iFolder {0}- Retrieving proxy credentials: The treenames of iFolder ldap tree and OES server does not match: Error ",DateTime.Now.ToString());
			tw.Close();
			DeleteProxyFile();
			return null;
		}

		// Get the current ldap setting from Simias.config file.
		string iFLdapHost, iFProxyUserDN, iFProxyPwd, iFAllContexts = null;
		bool iFLdapSSL = false;
		
		ReadConfigDetails( out iFLdapHost, out iFLdapSSL, out iFProxyUserDN, out iFProxyPwd, out iFAllContexts);
		DeleteProxyFile();

		if( ProxyDN == true)
			return iFProxyUserDN; 
		else if( ProxyPwd == true)
			return iFProxyPwd;
		return null;
	}


	static bool CheckLdapTreeMatch( string cpHost, int cpPort)
	{
		// call the webservice so that ldap details from Simias.config file will be written into one file called proxyfile.
		WebCallMethod("WriteToFile");

		// Get the current ldap setting from Simias.config file.
		string iFLdapHost, iFProxyUserDN, iFProxyPwd, iFAllContexts = null;
		bool iFLdapSSL = false;

		ReadConfigDetails( out iFLdapHost, out iFLdapSSL, out iFProxyUserDN, out iFProxyPwd, out iFAllContexts);

		bool treeMatched = false;

		// If the ldap IP of iFolder is some AD or non-edir then return immediately.

		connection = new LdapConnection();

		int iFLdapPort;
		if( iFLdapSSL == true)
		{
			iFLdapPort = 636;
			connection.SecureSocketLayer = true;
		}
		else 	
		{
			iFLdapPort = 389;
			connection.SecureSocketLayer = false;
		}

		connection.Connect( iFLdapHost, iFLdapPort);


		if( !IseDirectory( connection, null, out treeMatched ))
		{
			Console.WriteLine(" iFolder configured ldap IP is not e-dir, it willr eturn ");
			DeleteProxyFile();
			return false;
		}

		// Now see if the tree which corresponds the cpHost is same as the tree that corresponds to iFolder ldap configured one.

		LdapConnection cpConnection = new LdapConnection();
		cpConnection.SecureSocketLayer = false;
		cpConnection.Connect(cpHost, 389);
		string cpTreeName = GetTreeName(cpConnection);
		if(cpTreeName == null)
		{
			Console.WriteLine(" common proxy tree name is null..it will return ");
			DeleteProxyFile();
			return false;
		}

		if( !IseDirectory( connection, cpTreeName, out treeMatched ))
		{
			Console.WriteLine(" local host is not e-dir, it will return.. ");
			DeleteProxyFile();
			return false;
		}

		if( treeMatched == true)
		{
			//Console.WriteLine(" Both iFolder's ldap tree and CommonProxy ldap tree name matched..it will proceed ");
		}
		else	Console.WriteLine(" iFolder and CommonProxy ldap tree name did not match, it will return..");

		if( treeMatched == false)
		{
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
		string storepath = ReadModMonoConfiguration();
		string proxyfilename = storepath + "/proxyfile";
		if (File.Exists(proxyfilename))
			File.Delete(proxyfilename);
	}

	// this function will make a webservice call to iFolder Admin
	static void WebCallMethod(string MethodName)
	{

		HttpWebRequest HttpWReq;
		HttpWebResponse HttpWResp;
		//HttpWReq = (HttpWebRequest)WebRequest.Create("http://164.99.101.25/simias10/iFolderAdmin.asmx/WriteToFile?");
		string req = "http://127.0.0.1/simias10/iFolderAdmin.asmx/" + MethodName+"?";
		

		HttpWReq = (HttpWebRequest)WebRequest.Create(req);
		HttpWReq.Headers.Add("SOAPAction", MethodName);
		HttpWReq.Method = "GET";
		HttpWResp = (HttpWebResponse)HttpWReq.GetResponse();
		if (HttpWResp.StatusCode == HttpStatusCode.OK)
		{
		}
		else
			Console.WriteLine(" WebRequest Failed, it would not work");

	}
	
	// this menthod updates Simias.Config file with updates list of API's which doesnot require Auth. Adding WriteLdapDetails to the list
	static void UpdateOldConfFile()
        {
		// Read apache simias conf file and fetch data path
		// get the Simias.config file and read it
		// Check if server is old or new based on the 	
		// SimiasAuthNotRequired field.
		// if Old, then update it with WriteLdapDetails API
                

		string dataPath = ReadModMonoConfiguration();

		if( dataPath != null )
                {
                        string path = Path.GetFullPath(Path.Combine(dataPath, "Simias.config"));
                        XmlDocument doc = new XmlDocument();
                        doc.Load(path);

                        string str = string.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", "section", "name", "Authentication", "setting", "SimiasAuthNotRequired" );

                        XmlElement element = ( XmlElement )doc.DocumentElement.SelectSingleNode( str );
                        if ( element != null )
                        {
                                string val = element.GetAttribute( "value" );

                                if( val.IndexOf("WriteToFile") >= 0 )
				{ //no need to update since this entry is there in config file.
				}
                                else
                                {
                                        Console.WriteLine("Old server, append it");
                                        string st = String.Concat(val,", iFolderAdmin.asmx:WriteToFile, iFolderAdmin.asmx:ServiceProxyRequests");
                                        // Console.WriteLine(" {0}", st);
                                        element.SetAttribute( "value", st );
                                }
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


                }

                return;
                        
        }


	// as input, pass the ldap IP/port where edir is configured on OES. also pass proxy username/pwd and also ldap admin username/pwd.

		// get the server's datapath
                static string ReadModMonoConfiguration()
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
							string [] GroupMemberAndRight = (line).Split(';');
							foreach( string str1 in GroupMemberAndRight)
							{
								if( str1.IndexOf("SimiasDataDir=") >= 0)
								{
									int startIndex  = str1.IndexOf('=');

									int len = str1.Length;
									int lastIndex = 0;
									if( str1[len - 1] == '\"')
									{
										lastIndex = str1.Length - startIndex - 2;
									}
									else
									{
										lastIndex = str1.Length - startIndex - 1;
									}
                                                        		dataPath = str1.Substring(startIndex+1, lastIndex);
									break;
								}
							}
                                                }
                                        }
                                }
                        }
                        catch (Exception e)
                        {
                                Console.WriteLine("The file {0} could not be read: {1}", path, e.Message);
				FileStream file = new FileStream( logfilename, FileMode.Append);
				TextWriter tw = new StreamWriter(file);
				tw.WriteLine("iFolder {0}- Could not read datapath from apache configuration: Error ",DateTime.Now.ToString());
				tw.Close();
                                return null;
                        }
                        return dataPath;
                }


	// Function to read ldap scheme, host and port from Simias.config
	static void ReadConfigDetails( out string iFLdapHost, out bool iFLdapSSL, out string iFProxyUserDN, out string iFProxyPwd, out string iFAllContexts)
	{

		iFLdapHost = iFProxyUserDN = iFProxyPwd = iFAllContexts = null;
		iFLdapSSL = false;

		string dataPath = ReadModMonoConfiguration();	
		string fileName = dataPath+"/proxyfile";

		string readcontents;         
		StreamReader FileStream; 
		FileStream = File.OpenText(fileName);
		readcontents = FileStream.ReadLine();
		while(readcontents != null)
    		{

			string [] KeyAndValue = readcontents.Split(':');
			if( KeyAndValue != null && KeyAndValue.Length > 1 )
			{
				if(KeyAndValue[0] == "host")
				{
					iFLdapHost = KeyAndValue[1];	
				}
				else if(KeyAndValue[0] == "SSL")
				{
					iFLdapSSL =  KeyAndValue[1] == "True" ? true : false;
				}
				else if(KeyAndValue[0] == "proxydn")
				{
					iFProxyUserDN = KeyAndValue[1];

				}
				if(KeyAndValue[0] == "proxypwd")
				{
					iFProxyPwd = KeyAndValue[1];
	
				}
				if(KeyAndValue[0] == "Context")
				{
					iFAllContexts = KeyAndValue[1];
					
				}
			}

			//Console.WriteLine(" data read from file is :"+readcontents);
			readcontents = FileStream.ReadLine();
    		}
		FileStream.Close();
	}
		


	// Function will call admin webservice to assign rights. Prerequisite is : The Details for assigning rights must be present in a file which will be
	// used during webservice function call.
	public static void AssignRights( )
	{
		//Console.WriteLine("Connect started");

		// If the ldap IP of iFolder is some AD or non-edir then return immediately.
		WebCallMethod( "ServiceProxyRequests");

	}

		// returns whether connection is e-dir or not. Also updates treeMatched with true or false depending on whether dirTreeName matches with the
		// tree name corresponding to connections's treeName
                public static bool IseDirectory(LdapConnection connection, string dirTreeName, out bool treeMatched)
                {
                        //Console.WriteLine("get directory type");

                        LdapAttribute attr      = null;
                        LdapEntry entry = null;
                        bool eDirectory = false;
                        LdapSearchResults lsc=connection.Search("",
                                                                                                LdapConnection.SCOPE_BASE,
                                                                                                "objectClass=*",
                                                                                                null,
                                                                                                false);
			treeMatched = false;
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
                                //Console.WriteLine("\n" + entry.DN);
                                LdapAttributeSet attributeSet = entry.getAttributeSet();
                                System.Collections.IEnumerator ienum =  attributeSet.GetEnumerator();
                                while(ienum.MoveNext())
                                {
                                        attr = (LdapAttribute)ienum.Current;
                                        string attributeName = attr.Name;
                                        string attributeVal = attr.StringValue;
                                        //Console.WriteLine( attributeName + ": value :" + attributeVal);
					if( String.Equals(attributeName, "directoryTreeName"))
					{
						if(dirTreeName != null &&  String.Equals( dirTreeName, attributeVal))
						{
							//Console.WriteLine("TreeName from iFolder's ldap IP source is :"+attributeVal);
							//Console.WriteLine("TreeName from localhost's ldap IP source is :"+dirTreeName);
							treeMatched = true;
						}
					}

                                        //eDirectory specific attributes
                                        //If any of the following attribute is found, conclude this as eDirectory
                                        if(     /*String.Equals(attributeName, "vendorVersion")==true ||        */
                                                String.Equals(attributeVal, "Novell, Inc.")==true /* ||
                                                String.Equals(attributeName, "dsaName")==true ||*/

                                                /*String.Equals(attributeName, "directoryTreeName")==true*/)
                                        {
                                                eDirectory = true;
                                                //break;
                                        }
                                }
                        }

                        if ( eDirectory == true)
                        {
				return true;
                        }
			else
                        {
				return false;
                        }

                }

		// returns the tree name for connection
                public static string GetTreeName(LdapConnection connection )
                {

                        LdapAttribute attr      = null;
                        LdapEntry entry = null;
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
                                //Console.WriteLine("\n" + entry.DN);
                                LdapAttributeSet attributeSet = entry.getAttributeSet();
                                System.Collections.IEnumerator ienum =  attributeSet.GetEnumerator();
                                while(ienum.MoveNext())
                                {
                                        attr = (LdapAttribute)ienum.Current;
                                        string attributeName = attr.Name;
                                        string attributeVal = attr.StringValue;
                                        //Console.WriteLine( attributeName + ": value :" + attributeVal);
					if( String.Equals(attributeName, "directoryTreeName"))
					{
						return attributeVal;
					}
				}
			}
			return null;
		}


}
