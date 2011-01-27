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
*                 $Author: Arul Selvan <rarul@novell.com>

*                 $Modified by: Kalidas Balakrishnan
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*       Added functionality to use RSA infra along with X509. 
*	RecoveryAgent class is extensible for other types of enc
*
*******************************************************************************/
 
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using System.Security.Cryptography;
#if MONO
using Mono.Security.X509;
#endif
using System.Threading;
using System.Xml;
using Simias.Storage;
/// <summary>
///
/// </summary>
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
	///
	/// </summary>
	public enum EncType : int
	{
	/// <summary>
	///
	/// </summary>
		X509 = 0x00000001,
	/// <summary>
	///
	/// </summary>
		RSA = 0x00000010,
	/// <summary>
	///
	/// </summary>
		PGP = 0x00000100
	};
	/// <summary>
	/// Class to encrypt the DEK with RA publickey
	/// </summary>
	public sealed class RecoveryAgent
	{
		
		private byte [] publicKey;
		private RSACryptoServiceProvider RSAData;
		private int encType;
 private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(RecoveryAgent));

		///<summary>
		///constructor
		///</summary>
		public RecoveryAgent(string RAPublicKey)
		{
			if(RAPublicKey.StartsWith("SimiasRSA"))
			{
				log.Debug("Before trim {0}", RAPublicKey);
				/// Find a better way of doing this - BUGBUG - length if SimiasRSA is 9
				publicKey = Convert.FromBase64String(RAPublicKey.Substring(9));
				log.Debug("After trim {0}", publicKey);
				encType = (int)EncType.RSA;
			}
			else
			{
				publicKey	 = Convert.FromBase64String(RAPublicKey);
				encType = (int)EncType.X509;
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="RAData"></param>
		public RecoveryAgent(RSACryptoServiceProvider RAData)
		{
			RSAData = RAData;
			encType = (int)EncType.RSA;
		}
#if MONO
		public RecoveryAgent(X509Certificate RAData)
		{
			RSAData = new RSACryptoServiceProvider();
			RSAData.ImportParameters(RAData.RSA.ExportParameters(true));
			encType = (int)EncType.X509;
		}
#endif

		///<summary>
		///encrypt the message
		///</summary>
		public string EncodeMessage(string message)
		{
			string encodedMessage ;
/// FIXME - very bad hack - get the entire Public key set -see Member.cs GetPublicKey/GetDefaultPublicKey- BUGBUG
			byte [] X509Exponent = {1,0,1};
			byte [] RSAExponent = {17};

			try
			{
				RSAParameters rsaParameters = new RSAParameters();
				rsaParameters.Modulus = publicKey;
				if(encType == (int)EncType.X509)
					rsaParameters.Exponent = X509Exponent;
				else
					rsaParameters.Exponent = RSAExponent;
				// Construct a formatter with the specified RSA key.
				RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
				RSA.ImportParameters(rsaParameters);
				
				// Convert the message to bytes to create the encrypted data.
				byte[] byteMessage = Convert.FromBase64String(message);
				encodedMessage = Convert.ToBase64String(RSA.Encrypt(byteMessage, false));
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return encodedMessage;
		}

		/// <summary>
		/// Message Decoder
		/// </summary>
		/// <param name="encmess">Byte stream which needs to be decoded</param>
		/// <returns>decoded string from te byte stream</returns>
		public string DecodeMessage(byte[] encmess)
		{
			string mess = null;
			try
			{
				mess = Convert.ToBase64String(RSAData.Decrypt(encmess, false));
		log.Debug("Decrypted Mess {0}", mess);
			}
			catch (CryptographicException cExp)
			{
				Console.WriteLine("Crpto Error {0}", cExp.Message);
			}
			return mess;
		}

		/// <summary>
		/// Decoder of Byte Stream to a string with One time  PP
		/// </summary>
		/// <param name="encmess">A byte stream which needs to be decoded</param>
		/// <param name="otpass">One time PP</param>
		/// <returns>Decoded string</returns>
		public string DecodeMessage(byte[] encmess, string otpass)
		{
			string retStr = null;
			string mess;
			try
			{
				mess = DecodeMessage(encmess);
				Key dKey = new Key(mess);
				PassphraseHash phash = new PassphraseHash();
				dKey.EncrypytKey(phash.HashPassPhrase(otpass), out retStr);
			}
			catch (CryptographicException cryExp)
			{
				Console.WriteLine("Crpto Error {0}", cryExp.Message);
			}
			return retStr;
		}

	}
}
