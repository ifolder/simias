/***********************************************************************
 *  $RCSfile: Feeds.cs,v $
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
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace Novell.Collaboration.Feeds
{
	/// <summary>
	/// Summary description for Feed
	/// </summary>
	public class Feed
	{
		#region Private Types
		private string url;
		private string username;
		private string password;
		private string title;
		private string description;
		private string link;
		private string language;
		private string copyright;
		private string type;
		private string managingEditor;
		private string webMaster;
		private DateTime pubDate;
		private DateTime lastBuildDate;
		private int ttl;
		private string generator;
		private string rating;
		private ArrayList categories = null;
		private ArrayList items = null;
		private bool includeItems = true;
		#endregion

		#region Properties
		public string Url
		{
			get{ return url; }
			set{ url = value; }
		}

		public string Username
		{
			get{ return username; }
			set{ username = value; }
		}

		public string Title
		{
			get{ return title; }
		}

		public string Description
		{
			get{ return description; }
		}

		public string Link
		{
			get{ return link; }
		}

		public DateTime LastBuildDate
		{
			get{ return lastBuildDate; }
		}

		public DateTime PublishDate
		{
			get{ return pubDate; }
		}

		public int Ttl
		{
			get{ return ttl; }
		}

		public string Rating
		{
			get{ return rating; }
		}

		public string Generator
		{
			get{ return generator; }
		}

		public string Owner
		{
			get{ return this.managingEditor; }
		}

		public string Copyright
		{
			get{ return copyright; }
		}

		public string Password
		{
			get{ return password; }
		}

		#endregion

		#region Constructors
		public Feed()
		{
			//
			// TODO: Add constructor logic here
			//
			categories = new ArrayList();
			items = new ArrayList();
		}

		public Feed( string Url )
		{
			url = Url;
			categories = new ArrayList();
			items = new ArrayList();
		}

		public Feed( string Url, string Username, string Password )
		{
			url = Url;
			username = Username;
			password = Password;
			categories = new ArrayList();
			items = new ArrayList();
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

		private void ProcessDocument( XmlDocument Doc )
		{
			// Check if this document is RSS
			// For now that's all I'm going to understand and process

			XmlNode rootNode = Doc.FirstChild;

			// If there is an xml declaration attached to the document
			// move past it
			if ( rootNode.NodeType == XmlNodeType.XmlDeclaration )
			{
				rootNode = rootNode.NextSibling;
				if ( rootNode == null )
				{
					throw new ApplicationException( "Not a valid Feed stream!" );
				}
			}

			if ( rootNode.NodeType != XmlNodeType.Element )
			{
				throw new ApplicationException( "Not a valid Feed stream!" );
			}
		
			if ( rootNode.Name.ToLower() != "rss" )
			{
				throw new ApplicationException( "Not a valid Feed stream!" );
			}

			this.type = rootNode.Name;

			// For now assume 2.0
			// The next node should be the channel
			XmlNode channel = rootNode.ChildNodes[0];
			if ( channel.Name.ToLower() != "channel" )
			{
				throw new ApplicationException( "Not a valid Feed stream!" );
			}

			foreach( XmlNode node in channel )
			{
				switch( node.Name.ToLower() )
				{
					case "item":
					{
						if ( includeItems == true )
						{
							try
							{
								items.Add( new Item( node ) );
							}
							catch{}
						}	
						break;
					}
					case "title":
					{
						this.title = node.InnerText;
						break;
					}

					case "description":
					{
						this.description = node.InnerText;
						break;
					}

					case "link":
					{
						this.link = node.InnerText;
						break;
					}

					case "language":
					{
						this.language = node.InnerText;
						break;
					}

					case "copyright":
					{
						this.copyright = node.InnerText;
						break;
					}

					case "webmaster":
					{
						this.webMaster = node.InnerText;
						break;
					}

					case "managingeditor":
					{
						this.managingEditor = node.InnerText;
						break;
					}

					case "ttl":
					{
						this.ttl = XmlConvert.ToInt32( node.InnerText );
						break;
					}

					case "pubdate":
					{
						Console.WriteLine( "converting PubDate from: " + node.InnerText );
						//this.pubDate = XmlConvert.ToDateTime( node.InnerText );
						this.pubDate = System.Convert.ToDateTime( node.InnerText );
						break;
					}

					case "lastbuilddate":
					{
						Console.WriteLine( "converting LastBuildDate from: " + node.InnerText );
						this.lastBuildDate = System.Convert.ToDateTime( node.InnerText );
						break;
					}

					case "generator":
					{
						this.generator = node.InnerText;
						break;
					}

					case "rating":
					{
						this.rating = node.InnerText;
						break;
					}

					case "category":
					{
						categories.Add( node.InnerText );
						break;
					}
				}
			}
		}
		#endregion
		
		#region Public Methods
		public string[] GetCategories()
		{
			if ( this.categories.Count == 0 )
			{
				return null;
			}

			return categories.ToArray( typeof( string ) ) as string[];
		}

		public Item[] GetItems()
		{
			if ( this.items.Count == 0 )
			{
				return null;
			}

			return items.ToArray( typeof( Item ) ) as Item[];
		}

		public bool Load( int Timeout, bool IncludeItems )
		{
			bool status = false;
			
			includeItems = IncludeItems;
			Uri serviceUri = new Uri( url );
			HttpWebResponse response = null;
			CookieContainer cookieJar = new CookieContainer();

			// If a username and password exist build a Network Credential object
			NetworkCredential credentials = null;
			if ( username != null && password != null )
			{
				credentials = new NetworkCredential( username, password ); 
			}

			// Create the web request.
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( serviceUri );
			bool retry = true;
		
		proxyRetry:
			request.Credentials = credentials;
			request.Timeout = Timeout * 1000;
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

					try
					{
						XmlDocument document = new XmlDocument();
						document.Load( readStream );
						ProcessDocument( document );
						status = true;
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
			
			return status;

		}
		#endregion
	}
}
