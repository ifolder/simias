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
 *  Author: Mike Lasky
 *
 ***********************************************************************/
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Simias.Client
{
	/// <summary>
	/// Summary description for WSInspection.
	/// </summary>
	public class WSInspection
	{
		#region Class Members
		/// <summary>
		/// Default name for WSIL document.
		/// </summary>
		private static string WSInspectionDocument = "inspection.wsil";

		/// <summary>
		/// WSIL specified XML tags.
		/// </summary>
		private static string WSIL_ServiceTag = "wsil:service";
		private static string WSIL_NameTag = "wsil:name";
		private static string WSIL_DescriptionTag = "description";
		private static string WSIL_LocationAttrTag = "location";
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the URL for the specified service by using WS-Inspection.
		/// </summary>
		/// <param name="host">Address and optionally port of the host server.</param>
		/// <param name="serviceName">Service name to find URL for.</param>
		/// <returns>A URL that references the specified service.</returns>
		static public Uri GetServiceUrl( string host, string serviceName )
		{
			Uri serviceUrl = null;
			HttpWebResponse response = null;

			// Parse the host string in case there is a port specified.
			Uri parseUri = new Uri( Uri.UriSchemeHttp + Uri.SchemeDelimiter + host );

			// Try 'https' first.
			UriBuilder wsUri = new UriBuilder( Uri.UriSchemeHttps, parseUri.Host, parseUri.Port, WSInspectionDocument );

			// Create the web request.
			HttpWebRequest request = WebRequest.Create( wsUri.Uri ) as HttpWebRequest;

			try
			{
				// Get the response from the web server.
				response = request.GetResponse() as HttpWebResponse;
			}
			catch
			{
				// Try 'http' next.
				wsUri.Scheme = Uri.UriSchemeHttp;
				request = WebRequest.Create( wsUri.Uri ) as HttpWebRequest;

				try
				{
					response = request.GetResponse() as HttpWebResponse;
				}
				catch
				{
					response = null;
				}
			}
	
			// Make sure that there was an answer.
			if ( response != null )
			{
				try
				{
					// Get the stream associated with the response.
					Stream receiveStream = response.GetResponseStream();

					// Pipes the stream to a higher level stream reader with the required encoding format. 
					StreamReader readStream = new StreamReader( receiveStream, Encoding.UTF8 );
					try
					{
						XmlDocument document = new XmlDocument();
						document.Load( readStream );

						//Create an XmlNamespaceManager for resolving namespaces.
						XmlNamespaceManager nsmgr = new XmlNamespaceManager( document.NameTable );
						nsmgr.AddNamespace( "wsil", document.DocumentElement.NamespaceURI );

						// Search for the named service element.
						XmlNode serviceNode = document.DocumentElement.SelectSingleNode( WSIL_ServiceTag + "[" + WSIL_NameTag + "='" + "Domain Service" + "']", nsmgr );
						if ( serviceNode != null )
						{
							// Get the description node.
							XmlElement description = serviceNode[ WSIL_DescriptionTag ];
							if ( description != null )
							{
								// Get the uri location.
								string uriString = description.GetAttribute( WSIL_LocationAttrTag );
								if ( uriString != null )
								{
									// Fix up the URI if it is relative.
									if ( !uriString.ToLower().StartsWith( Uri.UriSchemeHttp ) )
									{
										Uri respUri = response.ResponseUri;
										UriBuilder urb = new UriBuilder( respUri.Scheme, respUri.Host, respUri.Port, uriString.TrimStart( new char[] { '/' } ) );
										serviceUrl = urb.Uri;
									}
									else
									{
										serviceUrl = new Uri( uriString );
									}
								}
							}
						}
					}
					finally
					{
						readStream.Close ();
					}
				}
				finally
				{
					response.Close ();
				}
			}

			return serviceUrl;
		}
		#endregion
	}
}
