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
*                 $Author: Rob, Kalidas Balakrishnan
*                 $Modified by: Kalidas Balakrishnan
*                 $Mod Date: 19-12-2007
*                 $Revision: 0.2
*-----------------------------------------------------------------------------
* This module is used to:
*        < iFolder Web setup class >
*
*
*******************************************************************************/

using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

using Novell.iFolder.Utility;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// iFolder Web Setup
	/// </summary>
	class iFolderWebSetup
	{
		/// <summary>
		/// Command Arguments
		/// </summary>
		string[] args;

		/// <summary>
		/// Web Path
		/// </summary>
#if MONO
		string webPath = Path.GetFullPath("../lib/simias/webaccess");
#else
		string webPath = Path.GetFullPath("./webaccess");
#endif

		#region Options

		/// <summary>
		/// Simias System Name
		/// </summary>
		public Option help = new Option("help,?", "Usage Help", "Show This Screen", false, null);

		/// <summary>
		/// Web Alias
		/// </summary>
		public Option webAlias = new Option("web-alias", "Web Alias", "Web Alias for iFolder Web Access", true, "/ifolder");

		/// <summary>
		/// Simias URL
		/// </summary>
		public Option simiasUrl = new Option("simias-url", "iFolder URL", "The host or ip address of the iFolder server that will be used by the iFolder Web Access application", true, "http://localhost");

                /// <summary>
                /// Simias iChain logout URL
                /// </summary>
                public Option logoutUrl = new Option("logout-url", "Redirect URL", "Redirect URL for iChain / AccessGateway", false, "");

		/// <summary>
		/// Require SSL
		/// </summary>
		public Option requireSsl = new BoolOption("require-ssl", "Require SSL", "Require a secure connection between the browsers and the iFolder Web Access application", false, true);

		/// <summary>
		/// Require SSL
		/// </summary>
		public Option requireServerSsl = new BoolOption("require-server-ssl", "Require Server SSL", "Require a secure connection between the iFolder Server and the iFolder Web Access application", false, true);

                /// <summary>
                /// Apache User.
                /// </summary>
                public Option apacheUser = new Option("apache-user", "Apache User", "Apache User to use for providing permissions", false, "wwwrun");

                /// <summary>
                /// Apache Group.
                /// </summary>
                public Option apacheGroup = new Option("apache-group", "Apache Group", "Apache Group to use for providing permissions", false, "www");

                /// <summary>
                /// The port to connect on.
                /// </summary>
                public NoPromptOption port = new NoPromptOption("connect-port", "Connect Port", "The port to Connect on", false, null);

		#endregion
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="args">Command Arguments</param>
		iFolderWebSetup(string[] args)
		{
			this.args = args;
		}

		/// <summary>
		/// Setup iFolder Web
		/// </summary>
		void Setup()
		{
			Initialize();
			ParseArguments();
			
			SetupWeb();
#if MONO
			SetupModMono();
#endif
			SetupSsl();

			UpdateOwnership();
			// CheckConnection();
		}
    		/// <summary>
		/// Change the ownership of web.config to apache user so that iFolder
		/// server can chnage the values while running.
		/// </summary>
		void UpdateOwnership()
		{
			try
			{
				string MachineArch = Environment.GetEnvironmentVariable("HOSTTYPE");
				string webpath = (MachineArch.IndexOf("_64") > 0) ? Path.GetFullPath("../lib64/simias/web"): Path.GetFullPath("../lib/simias/web");			
				string webconfigfile = Path.Combine(webpath, "web.config"); 

				if (Execute("chown", "{0}:{1} {2}", apacheUser.Value, apacheGroup.Value, webconfigfile) != 0)
				{
					throw new Exception("Unable to set an owner for the web.config file.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to set an owner for web.config file");

			}
		}

        /// <summary>
        /// Initialize web-access setup
        /// </summary>

        /// <summary>
        /// Initialize web-access setup
        /// </summary>
		void Initialize()
		{
			// find user
			try
			{
				string MachineArch = Environment.GetEnvironmentVariable("OS_ARCH");
                        	webPath = (MachineArch == null) ? Path.GetFullPath("../lib/simias/webaccess"): Path.GetFullPath("../lib64/simias/webaccess");
				// uid.conf
				string path = Path.GetFullPath("/etc/apache2/uid.conf");

				TextReader reader = (TextReader)File.OpenText(path);
	
				if(reader == null)
					return;
				
				string line;

				while((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("User"))
					{
						apacheUser.Value = line.Split()[1];
					}
					else if (line.StartsWith("Group"))
					{
						apacheGroup.Value = line.Split()[1];
					}
				}
				
				reader.Close();
			}
			catch
			{
				// ignore
			}
		}

		/// <summary>
		/// Parse the Command-Line Arguments
		/// </summary>
		void ParseArguments()
		{
			if (args.Length == 0)
			{
				// prompt
				Prompt.CanPrompt = true;
				PromptForArguments();
			}
			else
			{
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
			
			// default scheme
			if (!simiasUrl.Value.ToLower().StartsWith(Uri.UriSchemeHttp))
			{
                                UriBuilder urlBuild = new UriBuilder( bool.Parse(requireServerSsl.Value) == true ? Uri.UriSchemeHttps:Uri.UriSchemeHttp, simiasUrl.Value);
                                if(port.Value != null)
                                        urlBuild.Port = Convert.ToInt32(port.Value);
                                simiasUrl.Value = urlBuild.ToString();

			}
			else
                        {
                                UriBuilder urlBuild = new UriBuilder(simiasUrl.Value);
                                if(port.Value != null)
                                        urlBuild.Port = Convert.ToInt32(port.Value);
                                simiasUrl.Value = urlBuild.ToString();
                        }

		}

		/// <summary>
		/// Show Usage
		/// </summary>
		private void ShowUsage()
		{
			Console.WriteLine("USAGE: ifolder-web-setup [OPTIONS]");

			Console.WriteLine();
			Console.WriteLine("OPTIONS:");
			Console.WriteLine();

			Option[] options = Options.GetOptions(this);

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

			Environment.Exit(-1);
		}

		/// <summary>
		/// Prompt for Arguments
		/// </summary>
		void PromptForArguments()
		{
			Console.Write("This script configures a server installation of iFolder Web Access application. ");
			Console.Write("The script is intended for testing purposes only. ");
			Console.WriteLine();

			// Web Alias
			Prompt.ForOption(webAlias);
			
			// Require SSL
			Prompt.ForOption(requireSsl);

			//Require Server SSL
			Prompt.ForOption(requireServerSsl);

                        Uri uri = new Uri(simiasUrl.DefaultValue);
                        UriBuilder newUri = new UriBuilder();
                        newUri.Host = uri.Host;
			newUri.Port = bool.Parse(requireServerSsl.Value) ? 443 : uri.Port;
                        newUri.Scheme = bool.Parse(requireServerSsl.Value) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
                        simiasUrl.DefaultValue = newUri.ToString();

			// Simias URL
			Prompt.ForOption(simiasUrl);

                        // Logout URL
                        Prompt.ForOption(logoutUrl);

                        // Logout URL
                        Prompt.ForOption(apacheUser);

                        // Logout URL
                        Prompt.ForOption(apacheGroup);


			Console.WriteLine();
			Console.WriteLine("Working...");
			Console.WriteLine();
		}

        /// <summary>
        /// write the section on console
        /// </summary>
        /// <param name="title">title </param>
        /// <param name="text">text under title</param>
		private void WriteSection(string title, string text)
		{
			Console.WriteLine();
			Console.WriteLine("----- {0} -----", title.ToUpper());
			Console.WriteLine(text);
			Console.WriteLine();
		}

        /// <summary>
        /// prompt for the option
        /// </summary>
        /// <param name="option">option to be prompted</param>
		private void PromptForOption(Option option)
		{
			option.Value = Prompt.ForString(option.Description, option.DefaultValue);
		}

        /// <summary>
        /// prompt this text for option
        /// </summary>
        /// <param name="option">option</param>
        /// <param name="text">text to be prompted</param>
		private void PromptForOption(Option option, string text)
		{
			WriteSection(option.Description, text);
			PromptForOption(option);
		}

		/// <summary>
		/// Setup the Web.config File
		/// </summary>
		void SetupWeb()
		{
			string path = Path.GetFullPath(Path.Combine(webPath, "Web.config"));

			Console.Write("Configuring {0}...", path);

			XmlDocument doc = new XmlDocument();
		
			doc.Load(path);

			XmlElement url = (XmlElement)doc.DocumentElement.SelectSingleNode("//configuration/appSettings/add[@key='SimiasUrl']");

                        if (url != null)
                        {
                                url.Attributes["value"].Value = simiasUrl.Value;

                                doc.Save(path);

                                Console.WriteLine("Done");
                        }
                        else
                        {
                                Console.WriteLine("Failed (Tag Not Found)");
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
		/// Setup the /etc/apache2/conf.d/ifolder_web.conf File
		/// </summary>
		void SetupModMono()
		{
			string path = "/etc/apache2/conf.d/ifolder_web.conf";
			string datapath = ReadModMonoConfiguration();


			Console.Write("Configuring {0}...", path);
			
			string ModMonoServer2 = Environment.GetEnvironmentVariable("IFOLDER_MOD_MONO_SERVER2_PATH");
			string iFolderMonoPath = Environment.GetEnvironmentVariable("IFOLDER_MONO_PATH");

			// create configuration
			using(StreamWriter writer = File.CreateText(path))
			{
				/* example
				Include /etc/apache2/conf.d/mod_mono.conf
					
				Alias /ifolder "/usr/webaccess"
				AddMonoApplications ifolder "/ifolder:/usr/webaccess"
				<Location /ifolder >
					MonoSetServerAlias ifolder
					Order allow,deny
					Allow from all
					AddHandler mono asax aspx ascx asmx ashx
					DirectoryIndex Default.aspx index.html
				</Location>
				*/
			
				string alias = webAlias.Value.Trim(new char[] { '/' });
                string sslPrefix = "";

                // do not require SSL
                if (!bool.Parse(requireSsl.Value))
                {
                    sslPrefix = "#";
                }
				if( iFolderMonoPath != null )
					writer.WriteLine( "Include {0}{1}", iFolderMonoPath, "/etc/apache2/conf.d/mod_mono.conf" );
				else
				{
					string mod_mono2_path = "/etc/apache2/conf.d/mod_mono.conf";
					if( File.Exists( mod_mono2_path ))
						writer.WriteLine( "Include {0}", mod_mono2_path );
					else
						writer.WriteLine( "Include {0}", "/etc/apache2/mod_mono.conf" );
				}
				writer.WriteLine();
				writer.WriteLine("Alias /{0} \"{1}\"", alias, webPath);
				writer.WriteLine("AddMonoApplications {0} \"/{0}:{1}\"", alias, webPath);
                                //if (logoutUrl.Value != String.Empty || logoutUrl.Value.Trim () != "" )
                                //    writer.WriteLine("MonoSetEnv {1} LogoutUrl={0}",logoutUrl.Value, alias);
				//if(datapath != null || datapath != String.Empty)
					//writer.WriteLine("MonoSetEnv {1} \"SimiasLogDir={0}/log\"", datapath, alias);
				// Set MonoServerPath to the path where ifolder-mod-mono-server2 script file is there
                                if( ModMonoServer2 != null )
                                {
                                        if(logoutUrl.Value != String.Empty || logoutUrl.Value.Trim () != "" )
                                                writer.WriteLine("MonoSetEnv {0} LogoutUrl={1};MONO_THREADS_PER_CPU={2}",alias, logoutUrl.Value,2000);
                                        else
                                                writer.WriteLine("MonoSetEnv {0} MONO_THREADS_PER_CPU={1}", alias, 2000);
                                        writer.WriteLine("MonoServerPath {0} {1}/ifolder-mod-mono-server2", alias,  ModMonoServer2);
                                        writer.WriteLine("MonoMaxActiveRequests {0} {1}", alias, 150);
                                        writer.WriteLine("MonoMaxWaitingRequests {0} {1}", alias, 250);
                                }
                                else if (logoutUrl.Value != String.Empty || logoutUrl.Value.Trim () != "" )
                                        writer.WriteLine("MonoSetEnv {1} LogoutUrl={0}",logoutUrl.Value, alias);
				writer.WriteLine("<Location /{0} >", alias);
				writer.WriteLine("\tMonoSetServerAlias {0}", alias);
				writer.WriteLine("\tOrder allow,deny");
				writer.WriteLine("\tAllow from all");
				writer.WriteLine("\tAddHandler mono asax aspx ascx asmx ashx");
				writer.WriteLine("\tDirectoryIndex Default.aspx index.html");
				writer.WriteLine("</Location>");
				writer.WriteLine();
                		writer.WriteLine("# comment out the following lines to remove the SSL requirement");
				if(Environment.GetEnvironmentVariable("OS_ARCH") != null)
                			writer.WriteLine("{0}LoadModule rewrite_module /usr/lib64/apache2/mod_rewrite.so", sslPrefix);
				else
                			writer.WriteLine("{0}LoadModule rewrite_module /usr/lib/apache2/mod_rewrite.so", sslPrefix);
                		writer.WriteLine("{0}RewriteEngine On", sslPrefix);
                		writer.WriteLine("{0}RewriteCond %{{HTTPS}} !=on", sslPrefix);
                		writer.WriteLine("{0}RewriteRule ^/{1}/(.*) https://%{{SERVER_NAME}}/{1}/$1 [R,L]", sslPrefix, alias);
                		writer.WriteLine();
                		writer.Close();
			}

			// chmod
			if (Execute("chmod", "644 {0}", path) != 0)
			{
				throw new Exception(String.Format("Unable to change {0} file permissions.", path));
			}

			// create directory
			string logPath = "/var/log/ifolder3";

			if (!System.IO.Directory.Exists(logPath))
			{
				System.IO.Directory.CreateDirectory(logPath);
			}

			// set permissions
			if (Execute("chown", "{0}:{1} {2}", apacheUser.Value, apacheGroup.Value, logPath) != 0)
			{
				throw new Exception("Unable to set an owner for the log path.");
			}
			
			Console.WriteLine("Done");
		}

		/// <summary>
		/// Setup SSL
		/// </summary>
		void SetupSsl()
		{
			Console.Write("Installing certificate...");

			Uri url = new Uri(simiasUrl.Value);

			if (url.Scheme == Uri.UriSchemeHttps)
			{
				// swap policy
				ICertificatePolicy policy = ServicePointManager.CertificatePolicy;
				ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
				
				// connect
				HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(url);
				
				try
				{
					request.GetResponse();
				}
				catch
				{
					// ignore
				}

				// restore policy
				ServicePointManager.CertificatePolicy = policy;

				// service point
				ServicePoint sp = request.ServicePoint;

				if ((sp != null) && (sp.Certificate != null))
				{
					string path = Path.GetFullPath(Path.Combine(webPath, "Web.config"));
                                        string certRawDetail = Convert.ToBase64String(sp.Certificate.GetRawCertData());
                                        string certDetail = sp.Certificate.ToString(true);

					XmlDocument doc = new XmlDocument();
		
					doc.Load(path);

					XmlElement cert = (XmlElement)doc.DocumentElement.SelectSingleNode("//configuration/appSettings/add[@key='SimiasCert']");
                                        BoolOption iFolderCertAcc = new BoolOption("iFolder-cert-acceptance", "Accept iFolder Server Certificate", certDetail, false, true);
                                        iFolderCertAcc.Prompt = true;
                                        Prompt.ForOption(iFolderCertAcc);

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
			else
			{
				Console.WriteLine("Skipped (Not Required)");
			}
		}

		/// <summary>
		/// Check Connection
		/// </summary>
		void CheckConnection()
		{
			Console.Write("Checking connection...");

			// swap policy
			ICertificatePolicy policy = ServicePointManager.CertificatePolicy;
			ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();

			UriBuilder url = new UriBuilder(simiasUrl.Value);

			url.Path = "/simias10/iFolderWeb.asmx?WSDL";

			// connect
			HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(url.Uri);
				
			try
			{
				request.GetResponse();

				Console.WriteLine("Done");
			}
			catch
			{
				Console.WriteLine("Failed");
			}
			
			// restore policy
			ServicePointManager.CertificatePolicy = policy;
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
			int result = -1;

			try
			{
				string arguments = String.Format(format, args);

				ProcessStartInfo info = new ProcessStartInfo(command, arguments);

				Process p = Process.Start(info);

				p.WaitForExit();

				result = p.ExitCode;
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);	
			}

			return result;
		}

		/// <summary>
		/// Main
		/// </summary>
		/// <param name="args">Command Arguments</param>
		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine();
			Console.WriteLine("IFOLDER WEB ACCESS SETUP");
			Console.WriteLine();

			iFolderWebSetup setup = new iFolderWebSetup(args);
			
			try
			{
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
