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
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

using Novell.Collaboration.Feeds;

namespace Simias.RssClient
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class FeedSync
	{
		#region Class Members
		private string url;
		private string feedID;
		private string feed;
		private string localPath;
		private string username;
		private string password;
		private string itemDirectory;
		static private string FeedsDirectory = "simias-feeds";
		static private string DefaultFeedDirectory = "My Feeds";
		private bool listAvailable = false;
		private bool listSubscribed = false;
		private bool listPublished = false;
		private bool subscribe;
		private bool unsubscribe;
		private bool synchronize = false;
		private bool help = false;
		private bool verbose = false;
		private int timeout = 60;
		
		#endregion

		#region Constructors
		
		public FeedSync()
		{
		}
		
		/*
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
			username = User;
			password = Password;
		}
		*/
		#endregion

		#region Private Methods

		private void ShowUseage()
		{
			Console.WriteLine( "" );
			Console.WriteLine( "A simple .NET command line utility for caching iFolder enclosures in the local file system" );
			Console.WriteLine( "Useage: FeedSync command <command options> <options>" );
			Console.WriteLine( "  FeedSync list <all|available|subscribed|published> --url <url for available> --username <user> --password <password>" );
			Console.WriteLine( "  FeedSync subscribe --url <path to feed> --feed <name of the feed/channel> --username <user credential>" );
			Console.WriteLine( "                     --password <password credential> --path <local path where to mount/cache enclosures>" );
			Console.WriteLine( "  FeedSync unsubscribe <feed name>" );
			Console.WriteLine( "  FeedSync synchronize" );
			Console.WriteLine( "" );
		}
		
		private void ParseCommandLine( string[] args )
		{
			for( int i = 0; i < args.Length; i++ )
			{
				switch ( args[i].ToLower() )
				{
					case "list":
					{
						if ( i + 1 < args.Length )
						{
							switch( args[i + 1].ToLower() )
							{
								case "all":
								{
									listAvailable = true;
									listSubscribed = true;
									break;
								}
								
								case "available":
								{
									listAvailable = true;
									break;
								}
								
								case "subscribed":
								{
									listSubscribed = true;
									break;
								}
								
								case "published":
								{
									listPublished = true;
									break;
								}
								
								default:
								{
									listAvailable = true;
									listSubscribed = true;
									break;
								}
							}
						}
						break;
					}
				
					case "subscribe":
					{
						subscribe = true;
						break;
					}
					
					case "unsubscribe":
					{
						unsubscribe = true;
						break;
					}
					
					case "synchronize":
					{
						synchronize = true;
						break;
					}
					
					case "--url":
					{
						if ( i + 1 < args.Length )
						{
							url = args[ i + 1 ];
						}
						break;
					}
					
					case "--username":
					{
						if ( i + 1 < args.Length )
						{
							username = args[ i + 1 ];
						}
						break;
					}
					
					case "--password":
					{
						if ( i + 1 < args.Length )
						{
							password = args[ i + 1 ];
						}
						break;
					}
					
					case "--feed":
					{
						if ( i + 1 < args.Length )
						{
							feed = args[ i + 1 ];
						}
						break;
					}
					
					case "--path":
					{
						if ( i + 1 < args.Length )
						{
							localPath = args[ i + 1 ];
						}
						break;
					}
					
					case "--help":
					{
						help = true;
						return;
					}
					
					case "--verbose":
					{
						verbose = true;
						break;
					}
				}
			}
		}
		
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
		/// Private method to enumerate all Feed files
		/// </summary>
		private ArrayList GetLocalFeedFiles()
		{
			ArrayList feeds = null;
			string feedsPath = GetFeedsPath();
			if ( feedsPath == null )
			{
				return feeds;
			}
			
			
			DirectoryInfo dinfo = new DirectoryInfo( feedsPath );
			foreach( FileInfo file in dinfo.GetFiles( "*.xml") )
			{
				if ( feeds == null )
				{
					feeds = new ArrayList();
				}
				
				feeds.Add( file.FullName );
			}
			
			return feeds;
		}		

		/// <summary>
		/// Private method to create and populate a feed
		/// subscription.
		/// A feed document must contain name and url which are
		/// presented in construction
		/// All parameters CAN be null.
		/// </summary>
		private bool CreateFeedConfiguration( string FeedGuid, string Description, string Ttl, DateTime LastBuild )
		{
			string feedPath = GetFeedConfigurationPath();
			if ( feedPath == null )
			{
				return false;
			}

			// Create and populate and XML document with Feed data
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
			if ( this.username != null && 
				this.username != "" &&
				this.password != null &&
				this.password != "" )
			{
				XmlAttribute userAttr = document.CreateAttribute( null, "User", null );
				userAttr.InnerText = this.username;
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
			/*
			if ( Link != null && Link != "" )
			{
				XmlNode linkElem = document.CreateNode( XmlNodeType.Element, "Link", "");
				linkElem.InnerText = Link;
				feedElem.AppendChild( linkElem );
			}
			*/

			// Add "TTL" to the document
			if ( Ttl != null && Ttl != "" )
			{
				XmlNode ttlElem = document.CreateNode( XmlNodeType.Element, "TTL", "");
				ttlElem.InnerText = Ttl;
				feedElem.AppendChild( ttlElem );
			}

			XmlNode pubElem = document.CreateNode( XmlNodeType.Element, "Published", "");
			pubElem.InnerText = String.Format( "{0:r}", LastBuild );
			feedElem.AppendChild( pubElem );

			XmlNode syncElem = document.CreateNode( XmlNodeType.Element, "Synchronized", "");
			syncElem.InnerText = "0";
			feedElem.AppendChild( syncElem );

			XmlNode itemElem = document.CreateNode( XmlNodeType.Element, "ItemDirectory", "");

			this.itemDirectory = FeedGuid;
			itemElem.InnerText = itemDirectory;
			feedElem.AppendChild( itemElem );

			string fullItemDirectoryPath = 
				GetFeedsPath() + Path.DirectorySeparatorChar + this.itemDirectory;

			//Console.WriteLine("Add the new element to the document...");
			document.AppendChild( feedElem );

			bool itemDirCreated = false;

			try
			{
				// Create the item directory
				Directory.CreateDirectory( fullItemDirectoryPath );
				itemDirCreated = true;
				XmlTextWriter tw = new XmlTextWriter( feedPath, null);
				tw.Formatting = Formatting.Indented;
				document.Save( tw );

				return true;
			}
			catch( Exception ex )
			{
				Console.WriteLine( "ERROR: failed to create feed configuration file: " + feedPath );
				Console.WriteLine( ex.Message );

				// If the item directory was created delete it
				if ( itemDirCreated == true )
				{
					Directory.Delete( fullItemDirectoryPath );
				}

				itemDirectory = null;
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
			string feedPath = null;
			string feedsPath = GetFeedsPath();
			if ( feedsPath != null )
			{
				// assemble the full feed path
				feedPath = feedsPath + Path.DirectorySeparatorChar.ToString() + this.feed + ".xml";
			}

			return feedPath;
		}

		/// <summary>
		/// Private method to return the full path to the
		/// configuration file for a subscribed feed.
		/// 
		/// Note: expects local member "feed" to contain the name
		/// of the feed the caller is removing.
		/// </summary>
		private string GetFeedsPath( )
		{
			string path = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			if ( ( path == null ) || ( path.Length == 0 ) )
			{
				path = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
			}

			// and our feeds directory
			path = path + Path.DirectorySeparatorChar.ToString() + FeedSync.FeedsDirectory;
			if ( !Directory.Exists( path ) )
			{
				Directory.CreateDirectory( path );
			}

			return path;
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
			bool status = false;
			string feedPath = this.GetFeedConfigurationPath();
			if ( feedPath != null )
			{
				XmlDocument document = new XmlDocument();

				try
				{
					document.Load( feedPath );
					status = true;
				}
				catch( FileNotFoundException )
				{
					return false;
				}
			}

			return status;
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
		/// Method to sync the actual enclosures down to the
		/// local file system
		/// </summary>
		public void ProcessFeed( XmlDocument doc )
		{
			try
			{
				foreach( XmlNode itemNode in doc.DocumentElement.SelectNodes( "//item" ) )
				{
					Console.WriteLine( "Processing item: " + itemNode.InnerText );
					foreach( XmlNode child in itemNode.ChildNodes )
					{
						Console.WriteLine( 
							"  tag: {0} value: {0}",
							child.Name,
							child.InnerText );
					}
					
				}
			}
			catch( Exception ex )
			{
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
		public void Subscribe()
		{
			if ( url == null || url == "" )
			{
				Console.WriteLine( "ERROR: missing command line argument \"--url\"" );
				return;
			}
			
			if ( localPath == null || localPath == "" )
			{
				Console.WriteLine( "ERROR: missing command line argument \"--path\"" );
			}
			
			if ( this.feed == null || this.feed == "" )
			{
				Console.WriteLine( "ERROR: missing command line argument \"--feed\"" );
			}
			
			// Check if there is already a subscription to
			// this feed.
			if ( ValidateFeedSubscription() == true )
			{
				Console.WriteLine( "ERROR: subscription already exists" );
				return;
			}
			
			string slash = "/";
			string fullUrl = 
				url.TrimEnd( slash.ToCharArray() ) + 
				"/simias10/rss.ashx?feed=" + 
				HttpUtility.UrlEncode( this.feed );
				
			Novell.Collaboration.Feeds.Feed feed =
				new Novell.Collaboration.Feeds.Feed( fullUrl, this.username, this.password );

			feed.Load( 30, false );

			Console.WriteLine( "FEED" );
			Console.WriteLine( "  Title: " + feed.Title );
			Console.WriteLine( "  Owner: " + feed.Owner );
			Console.WriteLine( "  Description: " + feed.Description );
			Console.WriteLine( "  Link: " + feed.Link );
			Console.WriteLine( "  Build Date: " + feed.LastBuildDate.ToString() );
			Console.WriteLine( "  Publish Date: " + feed.PublishDate.ToString() );
			Console.WriteLine( "  TTL: " + feed.Ttl.ToString() );

			// Create the configuration file
			CreateFeedConfiguration( 
				Guid.NewGuid().ToString(), 
				feed.Description, 
				feed.Ttl.ToString(), 
				feed.LastBuildDate );
		}
		

		/// <summary>
		/// List feeds the caller has subscribed to
		/// 
		/// </summary>
		public void ListSubscribed( )
		{
			string feed;
			string description;
			string link;
			string path;
			
			ArrayList feedFiles = this.GetLocalFeedFiles();
			foreach( string file in feedFiles )
			{
				XmlDocument doc = new XmlDocument();
				doc.Load( file );
				
				feed = description = link = path = null;
				
				XmlNode rootNode = doc.FirstChild;

				// If there is an xml declaration attached to the document
				// move past it
				if ( rootNode.NodeType == XmlNodeType.XmlDeclaration )
				{
					rootNode = rootNode.NextSibling;
					if ( rootNode == null )
					{
						throw new ApplicationException( "ERROR: not a valid Feed configuration file!" );
					}
				}

				if ( rootNode.NodeType != XmlNodeType.Element )
				{
					throw new ApplicationException( "ERROR: not a valid Feed configuration file!" );
				}
		
				if ( rootNode.Name.ToLower() != "feed" )
				{
					throw new ApplicationException( "ERROR: not a valid Feed configuration file!" );
				}

				//this.type = rootNode.Name;

				string ttl = null;
				string author = null;
				string pubDate = null;
				string buildDate = null;
				
				foreach( XmlAttribute attr in rootNode.Attributes )
				{
					if ( attr.Name.ToLower() == "name" )
					{
						feed = attr.Value;
					}
					
					if ( attr.Name.ToLower() == "url" )
					{
						link = attr.Value;
					}
				}
			
				foreach( XmlNode node in rootNode.ChildNodes )
				{
					switch( node.Name.ToLower() )
					{
						case "description":
						{
							description = node.InnerText;
							break;
						}
						
						case "localpath":
						{
							path = node.InnerText;
							break;
						}
					}
				}
				
				if ( feed != null && link != null && description != null && path != null )
				{
					if ( verbose == false )
					{
						Console.Write( "\"" + feed + "\"" );
						if ( description != null )
						{
							Console.Write( ",\"" + description + "\"" );
						}
					
						if ( link != null )
						{
							Console.Write( ",\"" + link + "\"" );
						}
						
						if ( path != null )
						{
							Console.WriteLine( ",\"" + path + "\"" );
						}
						
						Console.WriteLine( "" );
					}
					else
					{
						Console.WriteLine( "Feed: " + feed );
						if ( description != null )
						{
							Console.WriteLine( "Description: " + description );
						}
						
						if ( link != null )
						{
							Console.WriteLine( "Link: " + link );
						}
						
						if ( path != null )
						{
							Console.WriteLine( "Local Path: " + path );
						}
						/*
						if ( ttl != null )
						{
							Console.WriteLine( "Refresh Time: " + ttl );
						}
						
						if ( buildDate != null )
						{
							Console.WriteLine( "Build Date: " + buildDate );
						}
						
						if ( pubDate != null )
						{
							Console.WriteLine( "Publish Date: " + pubDate );
						}
						*/
					}
				}
				else
				{
					Console.WriteLine( "ERROR: missing mandatory \"Feed\" element" );
				}
			}
		}	
		
		/// <summary>
		/// Perform a pull synchronization on all subscribed feeds
		/// 
		/// </summary>
		public void Synchronize()
		{
			string feedName;
			string description;
			string link;
			string path;
			string username;
			string password;
			
			ArrayList feedFiles = this.GetLocalFeedFiles();
			foreach( string file in feedFiles )
			{
				XmlDocument doc = new XmlDocument();
				doc.Load( file );
				
				feedName = description = link = path = username = password = null;
				
				XmlNode rootNode = doc.FirstChild;

				// If there is an xml declaration attached to the document
				// move past it
				if ( rootNode.NodeType == XmlNodeType.XmlDeclaration )
				{
					rootNode = rootNode.NextSibling;
					if ( rootNode == null )
					{
						throw new ApplicationException( "ERROR: not a valid Feed configuration file!" );
					}
				}

				if ( rootNode.NodeType != XmlNodeType.Element )
				{
					throw new ApplicationException( "ERROR: not a valid Feed configuration file!" );
				}
		
				if ( rootNode.Name.ToLower() != "feed" )
				{
					throw new ApplicationException( "ERROR: not a valid Feed configuration file!" );
				}

				//this.type = rootNode.Name;

				string ttl = null;
				string author = null;
				string pubDate = null;
				string buildDate = null;
				
				foreach( XmlAttribute attr in rootNode.Attributes )
				{
					switch( attr.Name.ToLower() )
					{
						case "name":
						{
							feedName = attr.Value;
							break;
						}
						
						case "url":
						{
							link = attr.Value;
							break;
						}
						
						case "user":
						{
							username = attr.Value;
							break;
						}
						
						case "pwd":
						{
							password = attr.Value;
							break;
						}
					}
				}
			
				foreach( XmlNode node in rootNode.ChildNodes )
				{
					switch( node.Name.ToLower() )
					{
						case "description":
						{
							description = node.InnerText;
							break;
						}
						
						case "localpath":
						{
							path = node.InnerText;
							break;
						}
					}
				}
				
				if ( feedName != null && link != null && description != null && path != null )
				{
					string url = link + 
						"/simias10/rss.ashx?feed=" +
						HttpUtility.UrlEncode( feedName ) +
						"&strict=false&items=true&enclosures=true";
						
					Novell.Collaboration.Feeds.Feed feed =
						new Novell.Collaboration.Feeds.Feed( url, username, password );

					feed.Load( 30, true );

					Item[] items = feed.GetItems();
					if ( items.Length != 0 )
					{
						foreach( Item item in items )
						{
							if ( item.Enclosure != null )
							{
								Enclosure enclosure = item.Enclosure;
								Console.WriteLine( "    ENCLOSURE" );
								Console.WriteLine( "      Url: " + enclosure.Url );
								Console.WriteLine( "      Type: " + enclosure.Type );
								Console.WriteLine( "      Length: " + enclosure.Length.ToString() );
					
								string downloadPath =
									path + Path.DirectorySeparatorChar.ToString() + feedName;
								enclosure.Download( feed, downloadPath, item.Title, 30, true );
							}
						}
					}	
				}
				else
				{
					Console.WriteLine( "ERROR: missing mandatory \"Feed\" element" );
				}
			}
		}	
		
		
		/// <summary>
		/// List available feeds at the specified url
		/// 
		/// Most url targets will only contain one feed but
		/// in the case of an iFolder server, all iFolders the
		/// the user owns or is a member of will be returned.
		///
		/// This method expects the private type url to exist
		/// in a valid format.  If the target expects credentials
		/// the username and password types must also be present.
		/// </summary>
		public void ListAvailable()
		{
			Console.WriteLine( "ListAvailable called" );
			
			// must have a url
			if ( url == null || url == "" )
			{
				throw new ApplicationException( "Url was not present on the command line" );
			}
			
			string slash = "/";
			string availableUrl = url.TrimEnd( slash.ToCharArray() ) + "/simias10/rss.ashx?strict=false";
			
			// Build a credential from the user name and password.
			NetworkCredential credentials = null;
			if ( username != null && password != null )
			{
				credentials = new NetworkCredential( username, password ); 
			}
			
			XmlDocument document = this.LoadDocument( availableUrl, credentials );
			if ( document != null )
			{
				this.ProcessFeeds( document );
				document = null;
			}
			
			return;
		}

		/// <summary>
		/// List published feeds at the specified url
		/// 
		/// Most url targets will only contain one feed but
		/// in the case of an iFolder server, all iFolders the
		/// the user owns or is a member of will be returned.
		///
		/// This method expects the private type url to exist
		/// in a valid format.  If the target expects credentials
		/// the username and password types must also be present.
		/// </summary>
		public void ListPublished()
		{
			Console.WriteLine( "ListPublished called" );
			
			// must have a url
			if ( url == null || url == "" )
			{
				throw new ApplicationException( "Url was not present on the command line" );
			}

			string slash = "/";
			string publishUrl =
				String.Format( 
					"{0}/simias10/rss.ashx?{1}",
					url.TrimEnd( slash.ToCharArray() ),
					"pub=true&items=false&strict=false" );
			
			// Build a credential from the user name and password.
			NetworkCredential credentials = null;
			if ( username != null && password != null )
			{
				credentials = new NetworkCredential( username, password ); 
			}
			
			XmlDocument document = this.LoadDocument( publishUrl, credentials );
			if ( document != null )
			{
				this.ProcessFeeds( document );
				document = null;
			}
			
			return;
		}
		
		/// <summary>
		/// Method to loadup an RSS Feed into an XML document
		/// </summary>
		private XmlDocument LoadDocument( string url, NetworkCredential Credentials )
		{
			Console.WriteLine( "LoadDocument called" );
			XmlDocument doc = null;
			
			Uri serviceUri = new Uri( url );
			HttpWebResponse response = null;
			CookieContainer cookieJar = new CookieContainer();

			// Create the web request.
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( serviceUri );
			bool retry = true;
		
			proxyRetry:

			request.Credentials = Credentials;
			request.Timeout = timeout * 1000;
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
						doc = new XmlDocument();
						doc.Load( readStream );
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
			
			return doc;
		}
		
		private void ProcessFeeds( XmlDocument Doc )
		{
			Console.WriteLine( "ProcessFeeds called" );
			
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

			//this.type = rootNode.Name;

			string feedName = null;
			string description = null;
			string link = null;
			string ttl = null;
			string author = null;
			string pubDate = null;
			string buildDate = null;
			string rating = null;
			
			foreach( XmlNode channel in rootNode.ChildNodes )
			{
				// For now assume 2.0
				// The next node should be the channel
				if ( channel.Name.ToLower() != "channel" )
				{
					break;
				}

				foreach( XmlNode node in channel )
				{
					switch( node.Name.ToLower() )
					{
						case "title":
						{
							feedName = node.InnerText;
							Console.WriteLine( "processing: " + feedName );
							break;
						}

						case "description":
						{
							description = node.InnerText;
							break;
						}

						case "link":
						{
							link = node.InnerText;
							break;
						}

						case "managingeditor":
						{
							author = node.InnerText;
							break;
						}

						case "ttl":
						{
							ttl = node.InnerText;
							break;
						}

						case "pubdate":
						{
							//this.pubDate = XmlConvert.ToDateTime( node.InnerText );
							pubDate = node.InnerText;
							break;
						}

						case "lastbuilddate":
						{
							buildDate = node.InnerText;
							break;
						}

						case "rating":
						{
							rating = node.InnerText;
							break;
						}
					}
				}
				
				if ( feedName != null )
				{
					if ( verbose == false )
					{
						Console.Write( "\"" + feedName + "\"" );
						if ( description != null )
						{
							Console.Write( ",\"" + description + "\"" );
						}
					
						if ( link != null )
						{
							Console.WriteLine( ",\"" + link + "\"" );
						}
						
						Console.WriteLine( "" );
					}
					else
					{
						Console.WriteLine( "Feed: " + feedName );
						if ( description != null )
						{
							Console.WriteLine( "Description: " + description );
						}
						
						if ( link != null )
						{
							Console.WriteLine( "Link: " + link );
						}
						
						if ( author != null )
						{
							Console.WriteLine( "Author: " + author );
						}
						
						if ( ttl != null )
						{
							Console.WriteLine( "Refresh Time: " + ttl );
						}
						
						if ( buildDate != null )
						{
							Console.WriteLine( "Build Date: " + buildDate );
						}
						
						if ( pubDate != null )
						{
							Console.WriteLine( "Publish Date: " + pubDate );
						}	
					}
				}
				else
				{
					Console.WriteLine( "ERROR: missing mandatory \"Title\" element" );
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
			FeedSync feedSync = new FeedSync();

			// no args == show help		
			if ( args.Length == 0 )
			{
				feedSync.ShowUseage();
				return;
			}

			feedSync.ParseCommandLine( args );
			
			if ( feedSync.help == true )
			{
				feedSync.ShowUseage();
				return;
			}
			
			if ( feedSync.listAvailable == true )
			{
				feedSync.ListAvailable();
			}
			
			if ( feedSync.listSubscribed == true )
			{
				feedSync.ListSubscribed();
			}
			
			if ( feedSync.listPublished == true )
			{
				feedSync.ListPublished();
			}
			
			if ( feedSync.subscribe == true )
			{	
				feedSync.Subscribe();
			}

			if ( feedSync.synchronize == true )
			{	
				feedSync.Synchronize();
			}
			
			/*
			FeedSync feedSync = 
				new FeedSync( 
						null, 
						"http://localhost:8086/simias10/rss.ashx", 
						"TestFolder",
						"admin",
						"simias" );
			*/


			return;

			/*
			Novell.Collaboration.Feeds.Feed feed =
				new Novell.Collaboration.Feeds.Feed( "http://localhost:8086/simias10/rss.ashx?items=true&enclosures=true", "admin", "simias" );

			feed.Load( 30, true );

			Console.WriteLine( "FEED" );
			Console.WriteLine( "  Title: " + feed.Title );
			Console.WriteLine( "  Owner: " + feed.Owner );
			Console.WriteLine( "  Description: " + feed.Description );
			Console.WriteLine( "  Link: " + feed.Link );
			Console.WriteLine( "  Build Date: " + feed.LastBuildDate.ToString() );
			Console.WriteLine( "  Publish Date: " + feed.PublishDate.ToString() );
			Console.WriteLine( "  TTL: " + feed.Ttl.ToString() );

			Item[] items = feed.GetItems();
			if ( items.Length != 0 )
			{
				Console.WriteLine( "ITEMS" );
			}
			foreach( Item item in items )
			{
				Console.WriteLine( "  ITEM" );
				Console.WriteLine( "    Title: " + item.Title );
				Console.WriteLine( "    Description: " + item.Description );
				Console.WriteLine( "    Guid: " + item.Guid );
				Console.WriteLine( "    Author: " + item.Author );
				Console.WriteLine( "    Link: " + item.Link );
				Console.WriteLine( "    Publish Date: " + item.Published.ToString() );

				if ( item.Enclosure != null )
				{
					Enclosure enclosure = item.Enclosure;
					Console.WriteLine( "    ENCLOSURE" );
					Console.WriteLine( "      Url: " + enclosure.Url );
					Console.WriteLine( "      Type: " + enclosure.Type );
					Console.WriteLine( "      Length: " + enclosure.Length.ToString() );
					
					enclosure.Download( feed, @"c:\download", item.Title, 30, true );

				}
			}
			*/
		}
		#endregion
	}
}
