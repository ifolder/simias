/*****************************************************************************
* Copyright © [2007-08] Unpublished Work of Novell, Inc. All Rights Reserved.
*
* THIS IS AN UNPUBLISHED WORK OF NOVELL, INC.  IT CONTAINS NOVELL'S CONFIDENTIAL,
* PROPRIETARY, AND TRADE SECRET INFORMATION.    NOVELL RESTRICTS THIS WORK TO
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
*                 $Author: Mike Lasky <mlasky@novell.com>, Russ Young
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/


using System;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
//using Simias.Client;

namespace Certificate
{
	/// <summary>
	/// Summary description for CertPolicy.
	/// </summary>
	public class CertPolicy : ICertificatePolicy
	{
		/*
		/// <summary>
		/// Class used to verify a cert.
		/// </summary>
		public class CertificateState
		{
			/// <summary>The certificate.</summary>
			public X509Certificate		Certificate;
			bool						accepted;
			CertificateProblem			problem;
            		//static private readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Store));

			/// <summary>
			/// Creates a new certificateState object.
			/// </summary>
			/// <param name="certificate">The certificate.</param>
			/// <param name="accepted">If true accept the certificate.</param>
			/// <param name="problem">The problem with the certificate.</param>
			public CertificateState(X509Certificate certificate, bool accepted, CertificateProblem problem)
			{
				this.Certificate = certificate;
				this.accepted = accepted;
				this.problem = problem;
			}

			/// <summary>
			/// Is the certificate valid.
			/// </summary>
			/// <param name="certificate">The certificate to check.</param>
			/// <returns></returns>
			public bool IsValid(X509Certificate certificate)
			{
				if (accepted || this.Certificate.GetIssuerName().Equals(certificate.GetIssuerName()))
					return true;
				else
					return false;
			}

			/// <summary>
			/// Get the reason the certificate failed.
			/// </summary>
			public CertificateProblem Problem
			{
				get { return problem; }
			}

			/// <summary>
			/// Get whether the Cert is accepted.
			/// </summary>
			public bool Accepted
			{
				get { return accepted; }
			}
		}
		*/

		#region Class Members

		static public Hashtable CertTable = Hashtable.Synchronized(new Hashtable());

		static public Hashtable CertRATable = Hashtable.Synchronized(new Hashtable());

		static public ArrayList CertRAList = new ArrayList();

		/// <summary>
		/// The default certificate policy.
		/// </summary>
		private ICertificatePolicy defaultCertPolicy;

		public enum CertificateProblem  : uint
		{
			CertOK						  = 0,
			CertEXPIRED                   = 0x800B0101,
			CertVALIDITYPERIODNESTING     = 0x800B0102,
			CertROLE                      = 0x800B0103,
			CertPATHLENCONST              = 0x800B0104,
			CertCRITICAL                  = 0x800B0105,
			CertPURPOSE                   = 0x800B0106,
			CertISSUERCHAINING            = 0x800B0107,
			CertMALFORMED                 = 0x800B0108,
			CertUNTRUSTEDROOT             = 0x800B0109,
			CertCHAINING                  = 0x800B010A,
			CertREVOKED                   = 0x800B010C,
			CertUNTRUSTEDTESTROOT         = 0x800B010D,
			CertREVOCATION_FAILURE        = 0x800B010E,
			CertCN_NO_MATCH               = 0x800B010F,
			CertWRONG_USAGE               = 0x800B0110,
			CertUNTRUSTEDCA               = 0x800B0112
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		public CertPolicy()
		{
			defaultCertPolicy = ServicePointManager.CertificatePolicy;
			ServicePointManager.CertificatePolicy = this;
		}

		#endregion

		/*
		/// <summary>
		/// Get the Certificate for the specified store.
		/// </summary>
		/// <param name="host">The host who owns the certificate.</param>
		/// <returns>The certificate as a byte array.</returns>
		public static CertificateState GetCertificate(string host)
		{
#if MONO
			// Fix for Bug 156874 - Linux client doesn't support connecting to servers with non-default ports
			// Russ and I saw cases where a full Uri was being passed in on a GetCertificate.  If that happens,
			// this code will parse just the host out which will allow the code to find the cert it's looking for.
			host = GetHostFromUri(host);
#endif
			CertificateState cs = CertTable[host] as CertificateState;
			if (cs != null)
			{
				return cs;
			}
			return null;
		}

		/// <summary>
		/// Get the list of Recovery Agents 
		/// </summary>
		/// <returns>The list as a String array.</returns>
		public static ArrayList GetRAList()
		{
				return CertRAList;
		}

		/// <summary>
		/// Get the Certificate for the specified store.
		/// </summary>
		/// <param name="host">The host who owns the certificate.</param>
		/// <returns>The certificate as a byte array.</returns>
		public static CertificateState GetRACertificate(string recoveryAgnt)
		{
			CertificateState cs = CertRATable[recoveryAgnt] as CertificateState;
			if (cs != null)
			{
				return cs;
			}
			return null;
		}


		/// <summary>
		/// Store the certificate for the specified host.
		/// </summary>
		/// <param name="certificate">The certificate to store.</param>
		/// <param name="host">The host the certificate belongs to.</param>
		public static void StoreCertificate(byte[] certificate, string host)
		{
#if MONO
                        // this code will parse just the host out which will allow the code to find the cert it's looking for.
                        host = GetHostFromUri(host);
#endif
	            if (CertTable.ContainsKey(host))  
	            {
	                CertTable.Remove(host);
	                //CertTable[host] = null;
	            }

			CertTable[host] = new CertificateState(new X509Certificate(certificate), true, CertificateProblem.CertOK);
		}

	        /// <summary>
	        /// Remove the certificate for the specified host.
	        /// </summary>
	        /// <param name="certificate">The certificate to store.</param>
	        /// <param name="host">The host the certificate belongs to.</param>
	        public static void RemoveCertificate( string host)
	        {
#if MONO
	                        // this code will parse just the host out which will allow the code to find the cert it's looking for.
	                        host = GetHostFromUri(host);
#endif
	            if (CertTable.ContainsKey(host))  
	            {
	                CertTable.Remove(host);
	                //CertTable[host] = null;
	            }
	        }

		/// <summary>
		/// Store the certificate for the specified host.
		/// </summary>
		/// <param name="certificate">The certificate to store.</param>
		/// <param name="host">The host the certificate belongs to.</param>
		public static void StoreRACertificate(byte[] certificate, string recoveryAgent)
		{
			if(CertRAList.Contains(recoveryAgent) == false)
				CertRAList.Add(recoveryAgent);
			if(CertRATable.ContainsKey(recoveryAgent))
				CertRATable.Remove(recoveryAgent);
			CertRATable[recoveryAgent] = new CertificateState(new X509Certificate(certificate), true, CertificateProblem.CertOK);
		}
		*/
		#region ICertificatePolicy Members

		/// <summary>
		/// Implements the application certificate validation policy.
		/// </summary>
		/// <param name="srvPoint">The ServicePoint that will use the certificate.</param>
		/// <param name="certificate">The certificate to validate.</param>
		/// <param name="request">The request that received the certificate.</param>
		/// <param name="certificateProblem">The problem encountered when using the certificate.</param>
		/// <returns>True if the certificate is to be honored. Otherwise, false is returned.</returns>
		public bool CheckValidationResult( ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem )
		{
			return true;
			/*
			bool honorCert = false;
			string localHost = request.RequestUri.Host.ToLower();
			if ((certificateProblem == 0))
			{
				honorCert = true;                
			}
			else if(CertificateProblem.CertEXPIRED.Equals(certificateProblem))
			{
				CertificateState cs = CertTable[localHost] as CertificateState;
				if(cs != null)
				{
					//update with proper certificate problem - client has to check for expiry
					CertTable[localHost] = new CertificateState(new X509Certificate(certificate), cs.Accepted, (CertificateProblem)certificateProblem);
				}
				else
				{
					// This is a new cert add the certificate.
					CertTable[localHost] = new CertificateState(new X509Certificate(certificate), false, (CertificateProblem)certificateProblem);
				}
			}
			else
			{
				CertificateState cs = CertTable[localHost] as CertificateState;
				if (cs != null && cs.IsValid(certificate))
				{
					honorCert = true;
				}
				else
				{
					// This is a new cert or replace the certificate.
					CertTable[localHost] = new CertificateState(new X509Certificate(certificate), false, (CertificateProblem)certificateProblem);
				}
			}
			return honorCert;
			*/
		}

		#endregion
		
	}
}
