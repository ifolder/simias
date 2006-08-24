/***********************************************************************
 *  $RCSfile$
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
 *  Author: Russ Young
 *
 ***********************************************************************/
	 
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
		const string CertType = "X509Certificate";

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
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void LoadCertsFromStore()
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain(store.LocalDomain);
			ICSList certs = domain.GetNodesByType(CertType);

			// We need to get rid of any duplicate certificates that may exist.
			Hashtable ht = new Hashtable();
			ArrayList nodesToDelete = new ArrayList();

			foreach(ShallowNode sn in certs)
			{
				Node node = new Node(domain, sn);
				try
				{
					string host = node.Properties.GetSingleProperty(hostProperty).Value.ToString();

					if (ht.Contains(host))
					{
						// A duplicate exists, use the most recent one.
						Node dupNode = (Node)ht[host];

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
							ht[host] = node;
						}
					}
					else
					{
						ht.Add(host, node);
					}

					string sCert = node.Properties.GetSingleProperty(certificateProperty).Value.ToString();
					byte[] certificate = Convert.FromBase64String(sCert);
					CertPolicy.StoreCertificate(certificate, host);
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
}