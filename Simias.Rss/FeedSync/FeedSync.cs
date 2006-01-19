/***********************************************************************
 *  $RCSfile: FeedSync.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Brady Anderson (banderso@novell.com)
 * 
 ***********************************************************************/
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace Simias.RssClient
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class FeedSync
	{
		#region Class Members
		private string url;
		private string channel;
		private string localPath;
		private string user;
		private string password;
		#endregion

		#region Constructors
		public FeedSync( string LocalPath, string Url, string Channel )
		{
			localPath = LocalPath;
			url = Url;
			channel = Channel;
		}

		public FeedSync( string LocalPath, string Url, string Channel, string User, string Password )
		{
			localPath = LocalPath;
			url = Url;
			channel = Channel;
			user = User;
			password = Password;
		}
		#endregion

		#region Private Methods
		static void IsTrustFailure( string host, WebException we )
		{
			if (we.Status == WebExceptionStatus.TrustFailure )
			{
				throw we;	
			}

			/*
			CertPolicy.CertificateState cs = CertPolicy.GetCertificate( host );
			if ( cs != null && !cs.Accepted )
			{
				// BUGBUG this is here to work around a mono bug.
				throw new WebException( we.Message, we, WebExceptionStatus.TrustFailure, we.Response );
			}
			*/
		}
		#endregion

		#region Public Methods
		public void ProcessFeed( )
		{
			Uri serviceUri = new Uri( url );
			HttpWebResponse response = null;
			CookieContainer cookieJar = new CookieContainer();

			// Build a credential from the user name and password.
			NetworkCredential credentials = null;
			if ( user != null && password != null )
			{
				credentials = new NetworkCredential( user, password ); 
			}

			// Create the web request.
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( serviceUri );
			bool retry = true;
		
		proxyRetry:

			request.Credentials = credentials;
			request.Timeout = 15 * 1000;
			request.CookieContainer = cookieJar;
			//request.Proxy = ProxyState.GetProxyState( request.RequestUri );

			try
			{
				// Get the response from the web server.
				response = request.GetResponse() as HttpWebResponse;

				// Mono has a bug where it doesn't set the cookies in the cookie jar.
				cookieJar.Add( response.Cookies );
			}
			catch ( WebException we )
			{
				IsTrustFailure( serviceUri.Host, we );
				if ( ( we.Status == WebExceptionStatus.Timeout ) ||
					( we.Status == WebExceptionStatus.NameResolutionFailure ) )
				{
					throw we;	
				}
				else
				{
					response = we.Response as HttpWebResponse;
					if (response != null)
					{
						cookieJar.Add( response.Cookies );
						if ( response.StatusCode == HttpStatusCode.Unauthorized && retry == true )
						{
							// This should be a free call we must be behind iChain.
							request = (HttpWebRequest)WebRequest.Create( response.ResponseUri );
							retry = false;
							goto proxyRetry;
						}
					}
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

					//Console.WriteLine( readStream.ReadToEnd() );

					try
					{
						XmlDocument document = new XmlDocument();
						document.Load( readStream );

						//Create an XmlNamespaceManager for resolving namespaces.
						//XmlNamespaceManager nsmgr = new XmlNamespaceManager( document.NameTable );
						//nsmgr.AddNamespace( "wsil", document.DocumentElement.NamespaceURI );

						// Search for the named service element.
//						string queryString = "rss/channel/[contains(title," + channel + ")]";
//						string queryString = "rss/channel/[title = \'" + channel + "\']";
						string queryString = "rss/channel/title";
						XmlNode serviceNode = document.DocumentElement.SelectSingleNode( queryString );
						if ( serviceNode != null )
						{
							Console.WriteLine( "found channel" );
							Console.WriteLine( serviceNode.ToString() );

							/*
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
										// Check to see if we need to use ssl.
										// Make the request and see if we get redirected 302;
										// Create the web request.
										request = (HttpWebRequest)WebRequest.Create( serviceUrl );
										request.CookieContainer = cks;
										request.Proxy = ProxyState.GetProxyState( request.RequestUri );
										response.Close();
										try
										{
											response = request.GetResponse() as HttpWebResponse;
											serviceUrl = response.ResponseUri;
										}
										catch (WebException wex)
										{
											IsTrustFailure(host, wex);
											response = wex.Response as HttpWebResponse;
											if (response != null)
											{
												if (response.StatusCode == HttpStatusCode.Unauthorized)
												{
													if (response.Headers.Get("Simias-Error") != null)
													{
														// This is expected because this service requires authentication.
														serviceUrl = response.ResponseUri;
													}
												}
											}
										}
									}
									else
									{
										serviceUrl = new Uri( uriString );
									}
								}
							}
							*/
						}
					}
					finally
					{
						readStream.Close();
					}
				}
				finally
				{
					response.Close();
				}
			}	
		}

		#endregion

		#region Static Methods
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			//
			// TODO: Add code to start application here
			//

			FeedSync feedSync = 
				new FeedSync( 
						null, 
						"http://192.168.1.101:8086/simias10/rss.ashx", 
						"My Pictures",
						"banderso",
						"novell" );

			feedSync.ProcessFeed();
		}
		#endregion
	}
}
