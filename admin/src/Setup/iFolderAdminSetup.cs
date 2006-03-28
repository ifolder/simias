/***********************************************************************
 *  $RCSfile: iFolderAdminSetup.cs,v $
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
using System.IO;
using System.Xml;
using System.Net;
using System.Diagnostics;

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
		/// Apache User
		/// </summary>
		string apacheUser = "wwwrun";

		/// <summary>
		/// Apache Group
		/// </summary>
		string apacheGroup = "www";

		/// <summary>
		/// Web Path
		/// </summary>
#if MONO
		string webPath = Path.GetFullPath("../webaccess");
#else
		string webPath = Path.GetFullPath("./webaccess");
#endif

		#region Options

		/// <summary>
		/// Simias System Name
		/// </summary>
		public Option help = new Option("help,?", "Usage Help", false, null);

		/// <summary>
		/// Web Alias
		/// </summary>
		public Option webAlias = new Option("web-alias", "Web Alias", true, "/ifolder");

		/// <summary>
		/// Simias URL
		/// </summary>
		public Option simiasUrl = new Option("simias-url", "Simias URL", true, "http://localhost");

		/// <summary>
		/// Require SSL
		/// </summary>
		public Option requireSsl = new Option("require-ssl", "Require SSL", false, "true");

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
			
			// CheckConnection();
		}

		void Initialize()
		{
			// find user
			try
			{
				// uid.conf
				string path = Path.GetFullPath("/etc/apache2/uid.conf");

				TextReader reader = (TextReader)File.OpenText(path);
				
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
				simiasUrl.Value = (new UriBuilder(Uri.UriSchemeHttp, simiasUrl.Value)).ToString();
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

			foreach(Option o in options)
			{
				foreach(string name in o.Names)
				{
					Console.WriteLine("\t{0}", name);
				}

				Console.WriteLine("\t\t\t\t{0}", o.Description);
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
			PromptForOption(webAlias, "An Apache alias that will point to the iFolder Web Access application.");
			
			// Require SSL
			requireSsl.Value = Prompt.ForYesNo("Require a secure connection between the browsers and the iFolder Web Access application", bool.Parse(requireSsl.Value)).ToString().ToLower();

			// Simias URL
			WriteSection("iFolder Server", "The host or ip address of the iFolder server that will be used by the iFolder Web Access application.");
			Uri uri = new Uri(simiasUrl.Value);
			UriBuilder newUri = new UriBuilder();
			bool secure = uri.Scheme.ToLower().Equals(Uri.UriSchemeHttps) ? true : false;
			newUri.Host = Prompt.ForString("iFolder Server", uri.Host);
			secure = Prompt.ForYesNo("Require a secure connection between the iFolder server and the iFolder Web Access application", secure);
			newUri.Scheme = secure ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
			simiasUrl.Value = newUri.ToString();
			
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
		/// Setup the /etc/apache2/conf.d/ifolder_web.conf File
		/// </summary>
		void SetupModMono()
		{
			string path = "/etc/apache2/conf.d/ifolder_web.conf";

			Console.Write("Configuring {0}...", path);

			// create configuration
			using(StreamWriter writer = File.CreateText(path))
			{
				/* example
				Include /etc/apache2/conf.d/mod_mono.conf
					
				Alias /ifolder "/opt/novell/ifolder3/webaccess"
				AddMonoApplications ifolder "/ifolder:/opt/novell/ifolder3/webaccess"
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

				writer.WriteLine("Include /etc/apache2/conf.d/mod_mono.conf");
				writer.WriteLine();
				writer.WriteLine("Alias /{0} \"{1}\"", alias, webPath);
				writer.WriteLine("AddMonoApplications {0} \"/{0}:{1}\"", alias, webPath);
				writer.WriteLine("<Location /{0} >", alias);
				writer.WriteLine("\tMonoSetServerAlias {0}", alias);
				writer.WriteLine("\tOrder allow,deny");
				writer.WriteLine("\tAllow from all");
				writer.WriteLine("\tAddHandler mono asax aspx ascx asmx ashx");
				writer.WriteLine("\tDirectoryIndex Default.aspx index.html");
				writer.WriteLine("</Location>");
				writer.WriteLine();
                writer.WriteLine("# comment out the following lines to remove the SSL requirement");
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
			if (Execute("chown", "{0}:{1} {2}", apacheUser, apacheGroup, logPath) != 0)
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

					XmlDocument doc = new XmlDocument();
		
					doc.Load(path);

					XmlElement cert = (XmlElement)doc.DocumentElement.SelectSingleNode("//configuration/appSettings/add[@key='SimiasCert']");

					if (cert != null)
					{
						cert.Attributes["value"].Value = sp.Certificate.GetRawCertDataString();

						doc.Save(path);
	
						Console.WriteLine("Done");

						Console.WriteLine(sp.Certificate.ToString(true));
					}
					else
					{
						throw new Exception(String.Format("Unable to find \"SimiasCert\" tag in the {0} file.", path));
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
			return true;
		}

		#endregion
	}
}
