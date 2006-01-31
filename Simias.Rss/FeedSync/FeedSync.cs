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
		private string feed;
		private string localPath;
		private string user;
		private string password;
		static private string FeedDirectory = "SimiasFeeds";
		static private string DefaultFeedDirectory = "My Feeds";
		#endregion

		#region Constructors
		public FeedSync( string LocalPath, string Url, string Channel )
		{
			localPath = LocalPath;
			url = Url;
			feed = Channel;
		}

		public FeedSync( string LocalPath, string Url, string Channel, string User, string Password )
		{
			localPath = LocalPath;
			url = Url;
			feed = Channel;
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

		/// <summary>
		/// Private method to create and populate a feed
		/// subscription.
		/// A feed document must contain name and url which are
		/// presented in construction
		/// All parameters CAN be null.
		/// </summary>
		private bool CreateFeedConfiguration( string Link, string Description, string Ttl )
		{
			string feedPath = GetFeedConfigurationPath();
			if ( feedPath == null )
			{
				return false;
			}

			// Create and populate and XML document with
			// Feed data
			XmlDocument document = new XmlDocument();

			// Create a new element node.
			XmlNode feedElem;
			feedElem = document.CreateNode( XmlNodeType.Element, "Feed", "");  

			XmlAttribute nameAttr = document.CreateAttribute( null, "Name", null );
			nameAttr.InnerText = this.feed;
			feedElem.Attributes.Append( nameAttr );

			XmlAttribute urlAttr = document.CreateAttribute( null, "Url", null );
			urlAttr.InnerText = this.url;
			feedElem.Attributes.Append( urlAttr );

			// Credentials?
			if ( this.user != null && 
				this.user != "" &&
				this.password != null &&
				this.password != "" )
			{
				XmlAttribute userAttr = document.CreateAttribute( null, "User", null );
				userAttr.InnerText = this.user;
				feedElem.Attributes.Append( userAttr );

				XmlAttribute pwdAttr = document.CreateAttribute( null, "Pwd", null );
				pwdAttr.InnerText = this.password;
				feedElem.Attributes.Append( pwdAttr );
			}

			if ( Description != null && Description != "" )
			{
				XmlNode descNode = 
					document.CreateNode( XmlNodeType.Element, "Description", null );
				descNode.InnerText = Description;
				feedElem.AppendChild( descNode );
			}

			// Add the local path to the document
			XmlNode pathElem = document.CreateNode( XmlNodeType.Element, "LocalPath", "");
			if ( localPath != null && localPath != "" )
			{
				pathElem.InnerText = this.localPath;
			}
			else
			{
				pathElem.InnerText = 
					Environment.SpecialFolder.CommonApplicationData + 
					Path.DirectorySeparatorChar.ToString() +
					FeedSync.DefaultFeedDirectory +
					Path.DirectorySeparatorChar.ToString() +
					this.feed;
			}
			feedElem.AppendChild( pathElem );

			// Add "Link" to the document
			if ( Link != null && Link != "" )
			{
				XmlNode linkElem = document.CreateNode( XmlNodeType.Element, "Link", "");
				linkElem.InnerText = Link;
				feedElem.AppendChild( linkElem );
			}

			// Add "TTL" to the document
			if ( Ttl != null && Ttl != "" )
			{
				XmlNode ttlElem = document.CreateNode( XmlNodeType.Element, "TTL", "");
				ttlElem.InnerText = Ttl;
				feedElem.AppendChild( ttlElem );
			}

			XmlNode pubElem = document.CreateNode( XmlNodeType.Element, "Published", "");
			pubElem.InnerText = "0";
			feedElem.AppendChild( pubElem );

			XmlNode syncElem = document.CreateNode( XmlNodeType.Element, "Synchronized", "");
			syncElem.InnerText = "0";
			feedElem.AppendChild( syncElem );

			//Console.WriteLine("Add the new element to the document...");
			document.AppendChild( feedElem );

			try
			{
				XmlTextWriter tw = new XmlTextWriter( feedPath, null);
				tw.Formatting = Formatting.Indented;
				document.Save( tw );

				/*
				FileStream fileStream = System.IO.File.Create( feedPath );
				XmlTextWriter writer = new XmlTextWriter( fileStream as TextWriter );
				writer.Formatting = Formatting.Indented;
				document.WriteTo( writer );
				writer.Flush();
				*/

				return true;
			}
			catch( Exception ex )
			{
				Console.WriteLine( "ERROR: failed to create feed configuration file: " + feedPath );
				Console.WriteLine( ex.Message );
				// log
			}

			// Create the file and stream the document out
			return false;
		}

		/// <summary>
		/// Private method to return the full path to the
		/// configuration file for a subscribed feed.
		/// 
		/// Note: expects local member "feed" to contain the name
		/// of the feed the caller is removing.
		/// </summary>
		private string GetFeedConfigurationPath( )
		{
			string path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			if ( ( path == null ) || ( path.Length == 0 ) )
			{
				path = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
			}

			// and our feeds directory
			path = path + Path.DirectorySeparatorChar.ToString() + FeedSync.FeedDirectory;
			if ( !Directory.Exists( path ) )
			{
				return null;
				//Directory.CreateDirectory( path );
			}

			// assemble the full feed path
			return path + Path.DirectorySeparatorChar.ToString() + this.feed + ".xml";
		}

		/// <summary>
		/// Private method to a subscription from the system.
		/// If param content == true remove the content (enclosures) 
		/// of the the subscription as well.
		/// 
		/// Note: expects local member "feed" to contain the name
		/// of the feed the caller is removing.
		/// </summary>
		private bool RemoveSubscription( bool content )
		{
			string feedPath = this.GetFeedConfigurationPath();
			if ( feedPath == null )
			{
				return false;
			}

			XmlDocument document = new XmlDocument();

			try
			{
				document.Load( feedPath );

				return true;
			}
			catch( FileNotFoundException )
			{
				return false;
			}

			return false;
		}

		/// <summary>
		/// Validates a subscription to a specified feed has been
		/// setup.
		/// 
		/// Note: expects local member "feed" to contain the name
		/// of the feed the caller is validating.
		/// </summary>
		private bool ValidateFeedSubscription()
		{
			string feedPath = GetFeedConfigurationPath();
			if ( feedPath == null )
			{
				return false;
			}

			// Does the file exist
			if ( !File.Exists( feedPath ) )
			{
				return false;
			}

			try
			{
				// Load the document and check for a few mandatory elements
				XmlDocument document = new XmlDocument();
				document.Load( feedPath );

				XmlNode feedNode = document.SelectSingleNode( "//Feed" );
				if ( feedNode == null )
				{
					throw new ApplicationException( "Feed node not found" );
				}

				XmlNode localPathNode = document.SelectSingleNode( "//LocalPath" );
				if ( localPathNode == null )
				{
					throw new ApplicationException( "LocalPath element not found" );
				}

				return true;
			}
			catch( FileNotFoundException )
			{
				// log
				//return false;
			}
			catch( Exception ex )
			{
				// log
				Console.WriteLine( ex.Message );
			}

			return false;
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
						string queryString = "//title";
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

		/// <summary>
		/// Subscribe to a specified feed.
		/// 
		/// This method is used to persistently subscribe to a
		/// specified feed.  After the initial subscription has
		/// processed and downloaded the feed any call to Process
		/// will update the feed contents to the specified local
		/// directory.
		/// </summary>
		public void Subscribe( )
		{
			// Check if there is already a subscription to
			// this feed.
			if ( ValidateFeedSubscription() == true )
			{
				Console.WriteLine( "Subscription exists" );
				return;
			}


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
						XmlNode serviceNode = document.DocumentElement.SelectSingleNode( "//title" );
						if ( serviceNode != null )
						{
							string description = null;
							string link = null;
							string ttl = null;

							XmlNode tempNode;
							tempNode = document.DocumentElement.SelectSingleNode( "//description" );
							if ( tempNode != null )
							{
								description = tempNode.InnerText;
							}

							tempNode = document.DocumentElement.SelectSingleNode( "//ttl" );
							if ( tempNode != null )
							{
								description = tempNode.InnerText;
							}

							tempNode = document.DocumentElement.SelectSingleNode( "//link" );
							if ( tempNode != null )
							{
								description = tempNode.InnerText;
							}

							// Create the configuration file
							if ( this.CreateFeedConfiguration( link, description, ttl ) == true )
							{
								this.ProcessFeed();
							}
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
			FeedSync feedSync = 
				new FeedSync( 
						null, 
						"http://localhost:8086/simias10/rss.ashx?feed=Public", 
						"Public",
						"banderso",
						"novell" );

			feedSync.Subscribe();

			//feedSync.ProcessFeed();
		}
		#endregion
	}
}
