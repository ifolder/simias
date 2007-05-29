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
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using Mono.Security.Authenticode;

using Novell.iFolder;
using Novell.iFolder.Utility;

namespace Novell.iFolder
{

	class RSAEncoder
	{
		// Use a member variable to hold the RSA key for encoding and decoding.
/// Encrption key holder
///		private static RSA eKey = null;
		private static RSA dKey = null;
	        private string titleTag = "CryptoKeyRecovery";
        	private string CollectionIDTag = "iFolderCollection";
	        private string iFolderIDTag = "iFolderID";
        	private string KeyTag = "Key";
	        public Option keyPath = new Option("key-path,kp", "Private Key", "Path to the Private key file (.pvk format)", true, null);
        	public Option inpath = new Option("input-path,ip", "Encrypted Key file path", "Path to the Encrypted key file", true, null);
	        public Option outpath = new Option("output-path,op", "Decrypted Key file path", "Path to the Decrypted key file", true, null);
        	public Option pvkPass = new Option("private-pass,pp", "Private Key Password", "Password to decrypt the Private key", true, null);
	        public BoolOption OTP = new BoolOption("onetime,ot", "Encrypt result key", "Encrypt the decrypted key with one time passpharse", true, true);
        	public Option OTPass = new Option("onetime-pass,s", "One Time Password", "Enter One Time passphrase to Encrypt with", true, null);
	        public NoPromptOption prompt = new NoPromptOption("prompt", "Prompt For Options", "Prompt the user for missing options", false, null);
        	public NoPromptOption help = new NoPromptOption("help,?", "Usage Help", "Show This Screen", false, null);
	        string[] args = null;
		#region Utilities

		/// <summary>
		/// Show Usage
		/// </summary>
		private void ShowUsage()
		{
			Console.WriteLine( "USAGE: key_converter <Path to Private Key file> <Path to Encrypted Key file> <Path to Decrypted key file>" );
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
//			Console.Write("This script configures a server installation of Simias to setup a new Simias system. ");
	//		Console.Write("The script is intended for testing purposes only. ");
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
			OTP.OnOptionEntered = new Option.OptionEnteredHandler(OnOTP);
		} 
		[STAThread]
		static void Main(string[] args)
		{
/// Test message
///			string message = "Welcome to iFolder 3.6.";
	
			RSAEncoder rsaEncoder = new RSAEncoder(args);
			rsaEncoder.ParseArguments();
			rsaEncoder.InitializeKey();
			rsaEncoder.ProcessInputKeyFile();
/// Testing code below -- commented
///		byte [] eMess = rsaEncoder.EncodeMessage(message);
///			Console.WriteLine("Decoded Mess {0}", rsaEncoder.DecodeMessage(eMess));
	
		}
	
		// Initialize an rsaKey member variable with the specified RSA key.
		private void InitializeKey()
		{
			try{
			PrivateKey prvkey = PrivateKey.CreateFromFile(keyPath.Value);
/// Encryption Key
///			eKey = prvkey.RSA;
			dKey = prvkey.RSA;
			}
			catch (System.Security.Cryptography.CryptographicException e){
				Console.WriteLine("Exception" , e.ToString());
			}
		}

#if ENC	
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
#endif	
		// Use the RSAPKCS1KeyExchangeDeformatter class to decode the
		// specified message.
		private byte[] DecryptMessage(byte[] encodedMessage)
		{
			byte[] keyMessage = null;
			string teststr = Convert.ToBase64String(encodedMessage);
	
			try
			{
				Console.WriteLine("mess {0}", teststr);
				// Construct a deformatter with the specified RSA key.
				RSAPKCS1KeyExchangeDeformatter keyDecryptor =
					new RSAPKCS1KeyExchangeDeformatter(dKey);


				// Decrypt the encoded message.
				keyMessage =
					keyDecryptor.DecryptKeyExchange(encodedMessage);
	
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unexpected exception caught:" + ex.ToString());
			}
	
			return keyMessage;
		}
	

        private bool OnOTP()
        {
            if (OTP.Value == false)
            {
                OTPass.Required = false;
                OTPass.Prompt = false;
            }
            return true;
        }

        private void ProcessInputKeyFile()
        {
            string strKey = string.Format("//{0}/{1}", CollectionIDTag, KeyTag);
            string strID = string.Format("//{0}/{1}", CollectionIDTag, iFolderIDTag);
            string decKey;
            byte[] decKeyByteArray;
            string inKeyPath = Path.GetFullPath(inpath.Value);
            string outKeyPath = Path.GetFullPath(outpath.Value);
            XmlDocument encFile = new XmlDocument();
            encFile.Load(inKeyPath);
            XmlNodeList keyNodeList, idNodeList;

            XmlElement root = encFile.DocumentElement;

            keyNodeList = root.SelectNodes(strKey);
            idNodeList = root.SelectNodes(strID);

            XmlDocument document = new XmlDocument();
            XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
            document.InsertBefore(xmlDeclaration, document.DocumentElement);
            XmlElement title = document.CreateElement(titleTag);
            document.AppendChild(title);
            int i = 0;
            foreach (XmlNode idNode in idNodeList)
            {
                Console.WriteLine(idNode.InnerText);
                XmlNode newNode = document.CreateNode("element", CollectionIDTag, "");
                newNode.InnerText = "";
                document.DocumentElement.AppendChild(newNode);
                XmlNode innerNode = document.CreateNode("element", iFolderIDTag, "");
                innerNode.InnerText = idNode.InnerText;
                newNode.AppendChild(innerNode);
                {
                    XmlNode keyNode = keyNodeList[i++];
                    Console.WriteLine(decKey = keyNode.InnerText);
                    decKeyByteArray = Convert.FromBase64String(decKey);
                    XmlNode newElem2 = document.CreateNode("element", KeyTag, "");
                    if (OTP.Value == true)
                        newElem2.InnerText = DecodeMessage(decKeyByteArray, OTPass.Value);
                    else
                        newElem2.InnerText = DecodeMessage(decKeyByteArray); 
                    newNode.AppendChild(newElem2);
                }
            }
            if (File.Exists(outKeyPath))
                File.Delete(outKeyPath);
            document.Save(outKeyPath);
        }

        
        private string DecodeMessage(byte[] encmess)
        {
            string mess = null;
            try
            {
                mess = Convert.ToBase64String(DecryptMessage(encmess));
            }
            catch (CryptographicException cExp)
            {
                Console.WriteLine("Crpto Error {0}", cExp.Message);
            }
            return mess;
        }

        private string DecodeMessage(byte[] encmess, string otpass)
        {
            string retStr = null;
            byte[] mess;
            try
            {
                mess = DecryptMessage(encmess);
                TripleDESCryptoServiceProvider tdesp = new TripleDESCryptoServiceProvider();
                byte[] IV ={ 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
                byte[] input = HashPassPhrase(otpass);
                tdesp.KeySize = input.Length * 8;
                byte[] output = Transform(mess, tdesp.CreateEncryptor(input, IV));
                retStr = Convert.ToBase64String(output);
            }
            catch (CryptographicException cryExp)
            {
                Console.WriteLine("Crpto Error {0}", cryExp.Message);
            }
            return retStr;
        }

        private byte[] HashPassPhrase(string Passphrase)
        {
            /*change to PasswordDeriveBytes.CryptDeriveKey once the  implementation is done mono

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Passphrase, salt);
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, tdes.IV);
            //tdes.Key is the NewPassphrase
			
            */
            byte[] NewPassphrase = null;
            byte[] salt ={ 0x49, 0x46, 0x4F, 0x4C, 0x44, 0x45, 0x52 };
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] data = utf8.GetBytes(Passphrase);
            try
            {
                HMACSHA1 sha1 = new HMACSHA1();
                sha1.Key = salt;
                for (int i = 0; i < 1000; i++)
                {
                    sha1.ComputeHash(data);
                    data = sha1.Hash;
                }
                NewPassphrase = new byte[data.Length + 4]; //20+4
                Array.Copy(data, 0, NewPassphrase, 0, data.Length);
                Array.Copy(data, 0, NewPassphrase, 20, 4);
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception {0}", exp.Message);
            }

            return NewPassphrase;
        }

        private byte[] Transform(byte[] input, ICryptoTransform CryptoTransform)
        {
            byte[] result = null;
            try
            {
                // create the necessary streams
                MemoryStream memStream = new MemoryStream();
                CryptoStream cryptStream = new CryptoStream(memStream, CryptoTransform, CryptoStreamMode.Write);
                // transform the bytes as requested
                cryptStream.Write(input, 0, input.Length);
                cryptStream.FlushFinalBlock();
                // Read the memory stream and
                // convert it back into byte array
                memStream.Position = 0;
                result = memStream.ToArray();
                // close and release the streams
                memStream.Close();
                cryptStream.Close();
            }
            catch (Exception exp)
            {
                Console.WriteLine("Exception {0}", exp.Message);
            }
            // hand back the encrypted buffer
            return result;
        }


	}
}
