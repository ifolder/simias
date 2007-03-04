/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2007 Novell, Inc.
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
 *  Author: Kalidas Balakrishnan <bkalidas@novell.com>
 *
 ***********************************************************************/

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Mono.Security.Authenticode;

using Novell.iFolder;
using Novell.iFolder.Utility;

namespace Novell.iFolder
{

	class RSAEncoder
	{
		// Use a member variable to hold the RSA key for encoding and decoding.
		private static RSA eKey, dKey;
		public Option inpath = new Option("input-path,ip", "Encrypted Key file path", "Path to the Encrypted key file", true, null);
		public Option outpath = new Option("output-path,op", "Decrypted Key file path", "Path to the Decrypted key file", true, null);
		public NoPromptOption prompt = new NoPromptOption("prompt", "Prompt For Options", "Prompt the user for missing options", false, null);
		public NoPromptOption help = new NoPromptOption("help,?", "Usage Help", "Show This Screen", false, null);
		string [] args;
		#region Utilities

		/// <summary>
		/// Show Usage
		/// </summary>
		private void ShowUsage()
		{
			Console.WriteLine( "USAGE: key_converter <Path to Encrypted Key file> <Path to Decrypted key file output>" );
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

			Environment.Exit(-1);
		}

		#endregion
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
				PromptForArguments();
			}
			else
			{
				// parse arguments
				Options.ParseArguments( this, args );

				// help
				if ( help.Assigned )
				{
					ShowUsage();
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
		}
		/// <summary>
		/// Prompt for Arguments
		/// </summary>
		void PromptForArguments()
		{
			Console.Write("This script configures a server installation of Simias to setup a new Simias system. ");
			Console.Write("The script is intended for testing purposes only. ");
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

 		#endregion

		RSAEncoder(string [] cmdargs)
		{
			args = cmdargs;
		} 
		[STAThread]
		static void Main(string[] args)
		{
			string message = "Welcome to iFolder 3.6.";
	
			RSAEncoder rsaEncoder = new RSAEncoder(args);
			rsaEncoder.ParseArguments();
			rsaEncoder.InitializeKey();
	
			StreamReader reader = File.OpenText(rsaEncoder.inpath.Value);
			StreamWriter writer = File.CreateText(rsaEncoder.outpath.Value);
			{
				while((message = reader.ReadLine()) != null)
			{
					writer.WriteLine(rsaEncoder.DecodeMessage(Encoding.ASCII.GetBytes(message)));
				}
			
			}
			reader.Close();
			writer.Close();
// Testing code below -- commented
//		byte [] eMess = rsaEncoder.EncodeMessage(message);
//			Console.WriteLine("Decoded Mess {0}", rsaEncoder.DecodeMessage(eMess));
	
		}
	
		// Initialize an rsaKey member variable with the specified RSA key.
		private void InitializeKey()
		{
			try{
			PrivateKey prvkey = PrivateKey.CreateFromFile("prvkey.pvk");
			eKey = prvkey.RSA;
			dKey = prvkey.RSA;
			}
			catch (System.Security.Cryptography.CryptographicException e){
				Console.WriteLine("Exception" , e.ToString());
			}
		}
	
		// Use the RSAPKCS1KeyExchangeDeformatter class to decode the 
		// specified message.
		private byte[] EncodeMessage(string message)
		{
			byte[] encodedMessage = null;
	
			try
			{
				// Construct a formatter with the specified RSA key.
				RSAPKCS1KeyExchangeFormatter keyEncryptor =
					new RSAPKCS1KeyExchangeFormatter(eKey);
	
				// Convert the message to bytes to create the encrypted data.
				byte[] byteMessage = Encoding.ASCII.GetBytes(message);
				encodedMessage = keyEncryptor.CreateKeyExchange(byteMessage);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unexpected exception caught:" + ex.ToString());
			}
	
			return encodedMessage;
		}
	
		// Use the RSAPKCS1KeyExchangeDeformatter class to decode the
		// specified message.
		private string DecodeMessage(byte[] encodedMessage)
		{
			string decodedMessage = null;
			string[] keyMessage = null;
			string teststr = Encoding.ASCII.GetString(encodedMessage);
	
			try
			{
				Console.WriteLine("mess {0}", teststr);
				// Construct a deformatter with the specified RSA key.
				RSAPKCS1KeyExchangeDeformatter keyDecryptor =
					new RSAPKCS1KeyExchangeDeformatter(dKey);

				// Split the folder ID and the encrypted key
				keyMessage = teststr.Split(new char [] {':'});
				Console.WriteLine("msg 1 {0} 2 {1}", keyMessage[0], keyMessage[1]);

				// Decrypt the encoded message.
				byte[] decodedBytes =
					keyDecryptor.DecryptKeyExchange(encodedMessage);
	
				// Retrieve a string representation of the decoded message.
				decodedMessage = Encoding.ASCII.GetString(decodedBytes);

				decodedMessage = keyMessage[0] + ":" + decodedMessage;
			}
			catch (Exception ex)
			{
			Console.WriteLine("Unexpected exception caught:" + ex.ToString());
			}
	
			return decodedMessage;
		}
	

	}
}
