/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 | Author: Arul Selvan <rarul@novell.com>
 |***************************************************************************/
 
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;

namespace Simias.CryptoKey
{
	[Serializable]
	public sealed class CollectionKey
	{
		/// <summary>
		/// Node ID
		/// </summary>
		public string 	NodeID;

		/// <summary>
		/// passphrase encrypted data encryption key
		/// </summary>
		public string	PEDEK;

		/// <summary>
		/// EncryptionBlob
		/// </summary>
		public string	REDEK;	

		/// <summary>
		/// default construtor
		/// </summary>
		public CollectionKey()
		{
			//do nothing
		}
		
		/// <summary>
		/// construtor
		/// </summary>
		public CollectionKey(string nodeID, string EncryptionKey, string RecoveryKey)
		{
			NodeID = nodeID;
			PEDEK = EncryptionKey;
			REDEK = RecoveryKey;				
		}
	}

	/// <summary>
	/// Class to encrypt the DEK with RA publickey
	/// </summary>
	public sealed class RecoveryAgent
	{
		
		private byte [] publicKey;

		///<summary>
		///constructor
		///</summary>
		public RecoveryAgent(string RAPublicKey)
		{
			publicKey	 = Convert.FromBase64String(RAPublicKey);
		}

		///<summary>
		///encrypt the message
		///</summary>
		public string EncodeMessage(string message)
		{
			string encodedMessage ;
			byte [] Exponent = {1,0,1};

			try
			{
				RSAParameters rsaParameters = new RSAParameters();
				rsaParameters.Modulus = publicKey;
				rsaParameters.Exponent = Exponent;
				// Construct a formatter with the specified RSA key.
				RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
				RSA.ImportParameters(rsaParameters);
				
				// Convert the message to bytes to create the encrypted data.
				UTF8Encoding Utf8 = new UTF8Encoding();
				byte[] byteMessage = Utf8.GetBytes(message);
				encodedMessage = Convert.ToBase64String(RSA.Encrypt(byteMessage, false));
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return encodedMessage;
		}
	}
}
