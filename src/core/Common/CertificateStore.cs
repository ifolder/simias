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
*                 $Author: Russ Young
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using Simias.Storage;
using Simias.Client;

namespace Simias.Security
{
	/// <summary>
	/// Summary description for Certificates.
	/// </summary>
	public class CertificateStore
	{
		const string certificateProperty = "Certificate";
		const string hostProperty = "Host";
		const string raProperty = "RecoveryAgent";
		const string CertType = "X509Certificate";
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );

		private static string GetHostFromUri( string uriString )
		{
			string host = uriString;

			try
			{
				if ( uriString.StartsWith( Uri.UriSchemeHttp ) )
				{
					Uri uri = new Uri( uriString );
					host = uri.Host;
				}
				else
				{
					Uri uri = new Uri( Uri.UriSchemeHttp + Uri.SchemeDelimiter + uriString );
					host = uri.Host;
				}
			}
			catch
			{}

			return host;
		}

		/// <summary>
		/// Get the Certificate for the specified store.
		/// </summary>
		/// <param name="host">The host who owns the certificate.</param>
		/// <returns>The certificate as a byte array.</returns>
		public static byte[] GetCertificate(string host)
		{
			CertPolicy.CertificateState cs = CertPolicy.GetCertificate(GetHostFromUri(host));
			if (cs != null)
			{
				return cs.Certificate.GetRawCertData();
			}
			return null;
		}

		/// <summary>
		/// Get the Certificate for the specified store along with Problem.
		/// </summary>
		/// <param name="host">The host who owns the certificate.</param>
		/// <returns>The certificate as a byte array.</returns>
		public static byte[] GetCertificate(string host, out CertPolicy.CertificateProblem Problem)
		{
			CertPolicy.CertificateState cs = CertPolicy.GetCertificate(GetHostFromUri(host));
			if (cs != null)
			{
				Problem = cs.Problem;
				return cs.Certificate.GetRawCertData();
			}
			Problem = CertPolicy.CertificateProblem.CertOK;
			return null;
		}

		/// <summary>
		/// Store the certificate for the specified host.
		/// </summary>
		/// <param name="certificate">The certificate to store.</param>
		/// <param name="host">The host the certificate belongs to.</param>
		/// <param name="persist">If true save in store.</param>
		public static void StoreCertificate(byte[] certificate, string host, bool persist)
		{
			string uriHost = GetHostFromUri(host);
			CertPolicy.StoreCertificate(certificate, uriHost);
			if (persist)
			{
				// Save the cert in the store.
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.LocalDomain);

				// Check for an existing cert in the store.
				Node cn = null;
				ICSList nodeList = domain.Search(hostProperty, uriHost, SearchOp.Equal);
				foreach (ShallowNode sn in nodeList)
				{
					cn = new Node(domain, sn);
					if (!cn.IsType( CertType))
					{
						cn = null;
						continue;
					}
					break;
				}

				if (cn == null)
				{
					// The cert doesn't exist ... create it.
					cn = new Node("Certificate for " + uriHost);
					domain.SetType(cn, CertType);
					cn.Properties.ModifyNodeProperty(new Property(hostProperty, uriHost));
				}
				cn.Properties.ModifyNodeProperty(new Property(certificateProperty, Convert.ToBase64String(certificate)));
				domain.Commit(cn);
				log.Debug("committed the storage of certificate into local domain");
			}
		}

        /// <summary>
        /// Store the certificate for the specified host.
        /// </summary>
        /// <param name="certificate">The certificate to store.</param>
        /// <param name="host">The host the certificate belongs to.</param>
        /// <param name="domainID">the domainID to which host belongs</param>
        /// <param name="persist">If true save in store.</param>
        public static void StoreDomainCertificate(byte[] certificate, string host, string domainID, bool persist)
        {
		// this function is neither tested nor used 
            string uriHost = GetHostFromUri(host);
            log.Debug("storing certificate for host : {0}", uriHost);
            CertPolicy.StoreCertificate(certificate, uriHost);
            //try
            //{
                if (persist)
                {
                    // Save the cert in the store.
                    Store store = Store.GetStore();
                    Domain domain = store.GetDomain(domainID);

                    // Check for an existing cert in the store.
                    Node cn = null;
                    ICSList nodeList = domain.Search(hostProperty, uriHost, SearchOp.Equal);
                    foreach (ShallowNode sn in nodeList)
                    {
                        cn = new Node(domain, sn);
                        if (!cn.IsType(CertType))
                        {
                            cn = null;
                            continue;
                        }
                        break;
                    }

                    if (cn == null)
                    {
                        // The cert doesn't exist ... create it.
                        //cn=new Node("
                        cn = new Node("Certificate for " + uriHost);
                        domain.SetType(cn, CertType);
                        cn.Properties.ModifyNodeProperty(new Property(hostProperty, uriHost));
                    }
                    Property myprop = new Property(certificateProperty, Convert.ToBase64String(certificate));
                    myprop.LocalProperty = true;
                    cn.Properties.ModifyNodeProperty(myprop);
                    //cn.Properties.ModifyNodeProperty(new Property(certificateProperty, Convert.ToBase64String(certificate)));
                    log.Debug("committed the storage of certificate");
                    domain.Commit(cn);
                }
            //}catch(Exception ex){}
        }

	/// <summary>
	/// Remove the certificate for the specified host, only from CertTable
	/// </summary>
	/// <param name="host">The host the certificate belongs to.</param>
	public static void RemoveCertFromTable(string host)
	{
		string uriHost = GetHostFromUri(host);
		CertPolicy.RemoveCertificate(uriHost);
	}

        /// <summary>
        /// Remove the certificate for the specified host.
        /// </summary>
        /// <param name="host">The host the certificate belongs to.</param>
        public static void RemoveCertificate(string host)
        {
            string uriHost = GetHostFromUri(host);
            log.Debug("removing for host : {0}", uriHost);
            CertPolicy.RemoveCertificate(uriHost);
            log.Debug("removed for host : {0}", uriHost);

            // also remove it from store , if any
            Store store = Store.GetStore();
            Domain domain = store.GetDomain(store.LocalDomain);

            // Check for an existing cert in the store.
            Node cn = null;
            ICSList nodeList = domain.Search(hostProperty, uriHost, SearchOp.Equal);
            foreach (ShallowNode sn in nodeList)
            {
                cn = new Node(domain, sn);
                if (cn.IsType(CertType))
                {
                    Property myprop = cn.Properties.GetSingleProperty(certificateProperty);
                    if (myprop != null)
                    {
                        domain.Commit(domain.Delete(cn));
                        log.Debug("committed the deletion of certtype node for :{0} ", uriHost);
                    }
                } 
            }
        }

        /// <summary>
        /// Remove the certificate for the specified host.
        /// </summary>
        /// /// <param name="domainID">the domainid for this host</param>
        /// <param name="host">The host the certificate belongs to.</param>
        public static void RemoveDomainCertificate(string domainID, string host)
        {
            // this API is yet to be tested , and has not been used.

            string uriHost = GetHostFromUri(host);
            log.Debug("removing for host : {0}", uriHost);
            CertPolicy.RemoveCertificate(uriHost);
            log.Debug("removed for host : {0}", uriHost);
            
            // also remove it from store , if any
            Store store = Store.GetStore();
            Domain domain = store.GetDomain(domainID);

            // check for the cert 
            Node cn = null;
            ICSList nodelist =  domain.Search(hostProperty, "*", SearchOp.Equal); //domain.GetNodesByType(CertType);
            if (nodelist == null) log.Debug("returned null for hostproperty * ");
            foreach (ShallowNode sn in nodelist)
            {
                cn = new Node(domain, sn);
               if (!cn.IsType(CertType))
                {
                    log.Debug("returned non null but this node is not of type CertType");
                   continue;
                }
                
                if (cn != null)
                {
                    // cert type is found
                    domain.Commit(domain.Delete(cn));
                    log.Debug("committed the deletion of certtype node for :{0} ", uriHost);
                }
            }

        }



		/// <summary>
		/// Get the list of Recovery Agents 
		/// </summary>
		/// <returns>The list as a String array.</returns>
		public static ArrayList GetRAList()
		{
			log.Debug("In GetRAList certificate store");
			return CertPolicy.GetRAList();
		}

		/// <summary>
		/// Get the Certificate for the specified store.
		/// </summary>
		/// <param name="host">The host who owns the certificate.</param>
		/// <returns>The certificate as a byte array.</returns>
		public static byte[] GetRACertificate(string recoveryAgnt)
		{
			log.Debug("In Get RA Certificate in certificate store");
			CertPolicy.CertificateState cs = CertPolicy.GetRACertificate(recoveryAgnt);
			if (cs != null)
			{
				return cs.Certificate.GetRawCertData();
			}
			return null;
		}

		/// <summary>
		/// Store the certificate for the specified host.
		/// </summary>
		/// <param name="certificate">The certificate to store.</param>
		/// <param name="host">The host the certificate belongs to.</param>
		/// <param name="persist">If true save in store.</param>
		public static void StoreRACertificate(byte[] certificate, string recoveryAgnt, bool persist)
		{
			CertPolicy.StoreRACertificate(certificate, recoveryAgnt);
			if (persist)
			{
				// Save the cert in the store.
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(store.DefaultDomain);

				// Check for an existing cert in the store.
				Node cn = null;
				ICSList nodeList = domain.Search(raProperty, recoveryAgnt, SearchOp.Equal);
				foreach (ShallowNode sn in nodeList)
				{
					cn = new Node(domain, sn);
					if (!cn.IsType( CertType))
					{
						cn = null;
						continue;
					}
					break;
				}

				if (cn == null)
				{
					// The cert doesn't exist ... create it.
					cn = new Node("Certificate for " + recoveryAgnt);
					domain.SetType(cn, CertType);
					cn.CollisionPolicy = CollisionPolicy.ServerWins;
					cn.Properties.ModifyNodeProperty(new Property(raProperty, recoveryAgnt));
				}
				cn.Properties.ModifyNodeProperty(new Property(certificateProperty, Convert.ToBase64String(certificate)));
				domain.Commit(cn);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void LoadCertsFromStore()
		{
			string rAgent;
			Store store = Store.GetStore();
			ICSList domainList = store.GetDomainList();
			Boolean isLocalDomain = false;

			// We need to get rid of any duplicate certificates that may exist.
			Hashtable ht = new Hashtable();
			ArrayList nodesToDelete = new ArrayList();
			
			foreach (ShallowNode snd in domainList)
			{
				Domain domain = store.GetDomain ( snd.ID );
				ICSList certs = domain.GetNodesByType(CertType);
				ht.Clear();
				nodesToDelete.Clear();

				isLocalDomain = domain.Name.Equals(Store.LocalDomainName);

				foreach(ShallowNode sn in certs)
				{
					Node node = new Node(domain, sn);
					try
					{
						string nodeProperty = node.Properties.GetSingleProperty(isLocalDomain?hostProperty:raProperty).Value.ToString();
						if(nodeProperty != null)
						{
							if (ht.Contains(nodeProperty))
							{
								// A duplicate exists, use the most recent one.
								Node dupNode = (Node)ht[nodeProperty];
							
								DateTime nodeTime = (DateTime)node.Properties.GetSingleProperty(PropertyTags.NodeCreationTime).Value;
								DateTime dupNodeTime = (DateTime)dupNode.Properties.GetSingleProperty(PropertyTags.NodeCreationTime).Value;
							
								if (dupNodeTime > nodeTime)
								{
									nodesToDelete.Add( node );							
									node = dupNode;
								}
								else
								{
									nodesToDelete.Add(dupNode);
									ht[nodeProperty] = node;
								}
							}
							else
							{
								ht.Add(nodeProperty, node);
							}
							string sCert = node.Properties.GetSingleProperty(certificateProperty).Value.ToString();
							byte[] certificate = Convert.FromBase64String(sCert);
							
							if(isLocalDomain)
								CertPolicy.StoreCertificate(certificate, nodeProperty);
							else
								CertPolicy.StoreRACertificate(certificate, nodeProperty);
						}
					}
					catch {}
				}
						
				if (nodesToDelete.Count > 0)
				{
					try
					{
						domain.Commit(domain.Delete((Node[])(nodesToDelete.ToArray(typeof(Node)))));
					}
					catch {}
				}


			}

		}


		/// <summary>
		/// This API will clean all certificate (.?er files)
		/// before Server, store existing and new certificate and load them in Hash table.
		/// </summary>
		public static void CleanCertsFromStore()
		{
    		string rAgent;
    		Store store = Store.GetStore();
    		ICSList domainList = store.GetDomainList();
    		Boolean isLocalDomain = false;
    		foreach (ShallowNode snd in domainList)
			{
				Domain domain = store.GetDomain ( snd.ID );
		    	ICSList certs = domain.GetNodesByType(CertType);
		    	isLocalDomain = domain.Name.Equals(Store.LocalDomainName);
				foreach(ShallowNode sn in certs)
		    	{
			 		Node node = new Node(domain, sn);
			    	try
			    	{
			    		string nodeProperty = node.Properties.GetSingleProperty(isLocalDomain?hostProperty:raProperty).Value.ToString();
				    	if(nodeProperty != null)
				    	{
							log.Debug("Node deleted is:{0}",node.Name);
				    		domain.Commit( domain.Delete(node) );
                   		}
			    	}
			    	catch(Exception ex)
			    	{
			    		log.Error("Exception while clearing existing Certificates from Domain"); 
		        	}
																	
             	} //End of Inner foreach loop

	    	} // End of Outer foreach loop

		} //End of Function	
		



	}
}
