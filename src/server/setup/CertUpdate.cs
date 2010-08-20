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
*                 $Author: Mahabaleshwar Asundi
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        < Certificate Import / Update tool class >
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

using Syscert = System.Security.Cryptography.X509Certificates;
using Mono.Security.X509;
using Mono.Security.Cryptography;


using Novell.Directory.Ldap;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.LdapProvider;
using Novell.iFolder;
using Novell.iFolder.Utility;

namespace Novell.iFolder
{
	/// <summary>
	///  Certificate Import / Update tool class.
	/// </summary>
	class CertUpdate
	{
		private const string apacheSimiasConf = "/etc/apache2/conf.d/simias.conf";
		private const string SearchStr = "MonoSetEnv simias10 \"SimiasRunAsServer=true;SimiasDataDir=";
		private const string datapathEnvStr = "SimiasDataDir=";
		private const string certMgrCmd= "certmgr"; 
		private const string certMgrArg= "-ssl -m ldaps://{0}:{1}";
		private const string LdapAuthentication = "LdapAuthentication";
		private const string EnterpriseDomain   = "EnterpriseDomain";
		private const string LdapUri		= "LdapUri";
		private const string AdminName		= "AdminName";
		private const string messagePrompt = "iFolder Admin Password";
		private const string defaultPass = "novell";
		
		private bool CertFailure = false;
		
		public  enum    CertificateProblem  : int 
		{
			TRUST_E_BASIC_CONSTRAINTS 	= (int)-2146869223,
			TRUST_E_BAD_DIGEST		= (int)-2146869232,
			CERT_E_VALIDITYPERIODNESTING	= (int)-2146762494,
			CERT_E_EXPIRED			= (int)-2146762495,
			CERT_E_CHAINING			= (int)-2146762486,
			CERT_E_UNTRUSTEDROOT		= (int)-2146762487,
		}

		/// <summary>
		/// GetiFolderDataPath: Reads Simias apache config file to extract Datapath
		/// </summary>
		string GetiFolderDataPath()
		{
			string path = Path.GetFullPath( apacheSimiasConf );
			string dataPath = null;
			if ( apacheSimiasConf == null ||  File.Exists( apacheSimiasConf ) == false )
				return null;
			try 
        		{
            			using(StreamReader sr = new StreamReader(path))
            			{
                			string line;
                			while ((line = sr.ReadLine()) != null) 
                			{
						if(line != null && line != String.Empty && line.StartsWith(SearchStr) == true )
						{
							string[] envArray = line.Split(new char[] { ';' });
							foreach(string env in envArray)
							{
								if(env.StartsWith(datapathEnvStr) == true)
								{
									int startIndex  = env.IndexOf('=');
									if(env.EndsWith("\""))
										dataPath = env.Substring(startIndex+1,(env.Length - startIndex -2 ));
									else
										dataPath = env.Substring(startIndex+1,(env.Length - startIndex -1 ));
            								Console.WriteLine("iFolder server DATA PATH :  {0} ", dataPath);
            								Console.WriteLine("");
								}
							}
							break;
						}
                			}
            			}
        		}
        		catch (Exception e) 
        		{
            			Console.WriteLine("Failed to read {0} : {1}", path, e.Message);
				return null;
        		}
			return dataPath;
		}

		/// <summary>
		/// Get the settings from the config file.
		/// </summary>
		/// <param name="storePath">iFolder LDAP Store Path</param>
		/// <param name="ldapURI">LDAP URL</param>
		/// <param name="iFolderAdminDN">iFolderAdmin name to connect with</param>
		/// <param name="iFolderAdminPassword">password to use to connect to ldap server</param>
		/// <returns>void.</returns>
		private void GetSettingsFromConfig(string storePath, out string ldapURI, out string iFolderAdminDN, out string iFolderAdminPass)
		{
			ldapURI = iFolderAdminDN = iFolderAdminPass = null;
			try
			{
				Configuration config = new Configuration( storePath, true );
				ldapURI = config.Get( LdapAuthentication, LdapUri );
				iFolderAdminDN = config.Get( EnterpriseDomain, AdminName );
            			Console.WriteLine("iFolder Admin DN: {0}", iFolderAdminDN);
            			Console.WriteLine("");
				iFolderAdminPass = PasswordString(messagePrompt, defaultPass);
            			Console.WriteLine("");
            			Console.WriteLine("");
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}

		/// <summary>
		/// Prompt for a Password String Value
		/// </summary>
		/// <param name="prompt">The Prompt String</param>
		/// <param name="defaultValue">The Default Value</param>
		/// <returns>password string collected</returns>
		public static string PasswordString(string prompt, string defaultValue)
                {
			Console.Write("{0}? [{1}]: ", prompt, defaultValue);
                        string password = "";

                        ConsoleKeyInfo info = Console.ReadKey(true);
                        while (info.Key != ConsoleKey.Enter)
                        {
                                if (info.Key != ConsoleKey.Backspace)
                                {
                                        password += info.KeyChar;
                                        info = Console.ReadKey(true);
                                }
                                else if (info.Key == ConsoleKey.Backspace)
                                {
                                        if (!string.IsNullOrEmpty(password))
                                        {
                                                password = password.Substring(0, password.Length - 1);
                                        }
                                        info = Console.ReadKey(true);
                                }
                        }
			if (string.IsNullOrEmpty(password))
			{
				password = defaultValue;
			}
			return password;
                }

		/// <summary>
		/// SetupLdapCert: Check connectivity and update the certificate
		/// </summary>
		/// <param name="lurl">ldap URl</param>
		/// <param name="iFolderAdminDN">iFolderAdmin name to connect with</param>
		/// <param name="iFolderAdminPassword">password to use to connect to ldap server</param>
		/// <returns>Success true or false.</returns>
		bool SetupLdapCert(string lUrl, string iFolderAdminDN, string iFolderAdminPassword)
		{
			Uri ldapUrl = new Uri(lUrl);
			string Host = ldapUrl.Host;
			bool Secure = ldapUrl.Scheme.ToLower().Equals(LdapUtility.LDAP_SCHEME_SECURE) ? true : false;
			int Port = (ldapUrl.Port != -1) ? ldapUrl.Port : (Secure ?  LdapSettings.UriPortLdaps : LdapSettings.UriPortLdap);


			Connect(Host, Port, Secure, iFolderAdminDN, iFolderAdminPassword);
			Console.WriteLine("Installing certificate from {0}...\n", ldapUrl.ToString());
			int retCode = ExecuteWithIO( false, certMgrCmd, certMgrArg, Host, Port);
			Console.WriteLine("");
			if (retCode == 0)
			{
				Console.WriteLine("LDAP server certificate import is successful.");
			}
			else
				Console.WriteLine("LDAP server certificate is not imported.");
			return true;
		}

		/// <summary>
		/// Connect to LDAP server and check the connectivity
		/// </summary>
		/// <param name="host">Host IP/Domain name</param>
		/// <param name="port">Port Number</param>
		/// <param name="secure">SSL yes or no.</param>
		/// <param name="user">User name to connect with</param>
		/// <param name="pass">password to use to connect to ldap server</param>
		/// <returns>void.</returns>
		void Connect(string host, int port, bool secure, string user, string pass)
		{
			LdapConnection connection = null;
			Console.WriteLine("");
			Console.WriteLine("============================================================");
			Console.WriteLine("Attempting to connect as Folder Administrator");
			Console.WriteLine("");
			try
			{
				connection = new LdapConnection();
				connection.SecureSocketLayer = secure;
				connection.UserDefinedServerCertValidationDelegate += new CertificateValidationCallback( SSLHandler );
				connection.Connect(host, port);
				connection.Bind(user, pass);
				Console.WriteLine("Successful...");
			}
			catch( LdapException e )
			{
				//Console.WriteLine("Failed to connect to server. Return code :{0}:{1} ", e.Message, e.StackTrace);
				switch ( e.ResultCode )
				{
					case LdapException.INVALID_CREDENTIALS:
						Console.WriteLine(""); 
						Console.WriteLine("Invalid Credentials entered");
						Environment.Exit(-1);
						break;
					case LdapException.CONNECT_ERROR:
						Console.WriteLine(""); 
						if(CertFailure == true)
							Console.WriteLine("Failed to connect to server. Return code :{0} ", e.ResultCode);
						else
							Console.WriteLine("Failed to connect to server, Check Ldap server availability. Return code :{0} ", e.ResultCode);
						Console.WriteLine(""); 
						break;
					case LdapException.SSL_HANDSHAKE_FAILED:
						Console.WriteLine(""); 
						Console.WriteLine("SSL handshake failed, Return code :{0}", e.ResultCode);
						Console.WriteLine("");
						break;
					default:
						Console.WriteLine("Failed to connect to server. Return code:{0}", e.ResultCode);
						break;
				}	

			}
			catch( Exception e )
			{
				Console.WriteLine("Exception {0} : {1} ", e.Message, e.StackTrace);
			}
			finally
			{
				CertFailure = false;
				if(connection != null)
				{
					try
					{
						connection.Disconnect();
					}catch{}
				}
				Console.WriteLine("============================================================");
				Console.WriteLine("");
			}
		}

		/// <summary>
		/// ExecuteWithIO executes the command in the shell with IO.
		/// </summary>
		/// <param name="interact">interactive yes/no.</param>
		/// <param name="command">The command.</param>
		/// <param name="format">The arguments of the command.</param>
		/// <param name="args">The arguments for the format.</param>
		/// <returns>Certificate installed or not status 0 -- success and >0 failure.</returns>
		int ExecuteWithIO(bool interact, string command, string format, params object[] args)
		{
			ProcessStartInfo info = new ProcessStartInfo( command, String.Format( format, args ) );
			int retCode = 0;
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
				BoolOption ldapCertAcc = new BoolOption("ldap-cert-acceptance", "Accept LDAP Certificate", null, false, false);
                                ldapCertAcc.Description = "";
                                ldapCertAcc.Prompt = true;
                                Prompt.CanPrompt = true;
                                Prompt.ForOption(ldapCertAcc);
				try
				{
					// certmgr askes question for, adding certificate to machine store and also 
					// to addressbook, so we send "Y" for machine store, "N" for addressbook.
					if(ldapCertAcc.Value == true)
					{
						inStreamWriter.WriteLine("{0}","Y");	
						inStreamWriter.WriteLine("{0}","N");	
					}
					else
					{
						retCode = 1;
						inStreamWriter.WriteLine("{0}","N");	
						inStreamWriter.WriteLine("{0}","N");	
					}
				}
				catch(Exception){}
			}
			output = exProcess.StandardOutput.ReadToEnd();
			exProcess.StandardError.ReadToEnd();
			exProcess.WaitForExit();
			return retCode == 1 ? 1 : exProcess.ExitCode;
		}

		/// <summary>
		/// SSLHandler: call back method to check certificate
		/// </summary>
		/// <param name="certificate">Actual certificate</param>
		/// <param name="certificateErrors">certificateErrors errors</param>
		/// <returns>certificate validity true or false.</returns>
		public bool SSLHandler( Syscert.X509Certificate certificate, int[] certificateErrors)
		{
			bool retFlag=true;

                        if (certificateErrors != null && certificateErrors.Length > 0)
				if( !(certificateErrors.Length==1 && certificateErrors[0] == -2146762481))
                                {
                                	Console.WriteLine("Detected errors in the Server Certificate:");
                                	Console.WriteLine("");
       		                        for (int i = 0; i < certificateErrors.Length; i++)
               		                {
							switch(certificateErrors[i])
							{
								case (int)CertificateProblem.TRUST_E_BASIC_CONSTRAINTS:
									Console.WriteLine("Certificate problem TRUST_E_BASIC_CONSTRAINTS");
								break;
								case (int)CertificateProblem.TRUST_E_BAD_DIGEST:
									Console.WriteLine("Certificate problem TRUST_E_BAD_DIGEST");
								break;
								case (int)CertificateProblem.CERT_E_VALIDITYPERIODNESTING:
									Console.WriteLine("Certificate problem CERT_E_VALIDITYPERIODNESTING");
								break;
								case (int)CertificateProblem.CERT_E_EXPIRED:
									Console.WriteLine("Certificate problem CERT_E_EXPIRED");
								break;
								case (int)CertificateProblem.CERT_E_CHAINING:
									Console.WriteLine("Certificate problem CERT_E_CHAINING");
								break;
								case (int)CertificateProblem.CERT_E_UNTRUSTEDROOT:
									Console.WriteLine("Certificate problem CERT_E_UNTRUSTEDROOT");
								break;
								default:
									Console.WriteLine("Certificate problem unknown");
								break;
							}
                                	}
					retFlag = false;
                                	CertFailure = true;                                                                
				}
                        return retFlag;
                }

		/// <summary>
		/// Main
		/// </summary>
		/// <param name="args">Command Arguments</param>
		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine();
			Console.WriteLine("iFolder server LDAP certificate update tool");
			Console.WriteLine();
		
			try
			{
				CertUpdate cUpdate = new CertUpdate();
				string DataPath = cUpdate.GetiFolderDataPath();
				string ldapURI = String.Empty;
				string iFolderAdminDN = String.Empty;
				string iFolderAdminPass = String.Empty;
				cUpdate.GetSettingsFromConfig(DataPath, out ldapURI, out iFolderAdminDN, out iFolderAdminPass);
				if(cUpdate.SetupLdapCert(ldapURI, iFolderAdminDN, iFolderAdminPass) == false)
					throw new ApplicationException("Falied to configure");	
				
			}
			catch(Exception e)
			{
				Console.WriteLine("Failed");
				Console.WriteLine(e.StackTrace);
				Console.WriteLine();
				Environment.Exit(-1);
			}
			Console.WriteLine();

			Environment.Exit(0);
		}
	}
}
