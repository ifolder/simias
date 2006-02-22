/***********************************************************************
 *  $RCSfile: Enclosure.cs,v $
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
	/// Summary description for Enclosure.
	/// </summary>
	public class Enclosure
	{
		#region Private Types
		private string url;
		private long length;
		private string type;

		// The published date and time as reported by
		// enclosures parent (item)
		private DateTime published;

		private string username;
		private string password;

		private int timeout = 30;

		// Set to true if it is determined the
		// local file exists.
		private bool localFileExists = false;

		// Temporary name used when pulling a file from the server
		// to the local machine.
		private string workFileName;

		// Server file name determined from the
		// url or from the reply headers
		private string serverFileName;

		// Set by the download method
		// true the caller wants to overwrite a file
		// if it already exists.  false the caller does
		// not want to overwrite an existing file.
		private bool overwrite;

		private bool downloadError;

		#endregion

		#region Properties
		public string Url
		{
			get{ return url; }
			set{ url = value; }
		}

		public string Type
		{
			get{ return type; }
			set{ type = value; }
		}

		public long Length
		{
			get{ return length; }
			set{ length = value; }
		}

		public DateTime Published
		{
			get{ return published; }
		}

		#endregion

		#region Constructors
		public Enclosure()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		internal Enclosure( DateTime Published, XmlNode Node )
		{
			this.published = Published;
			LoadFromXmlNode( Node );
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


		private void DownloadChunk( Stream Writer, int Offset, long Length )
		{
			Console.WriteLine( "url: " + url );
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
				// Validate if the local file exists
				string disp = response.Headers[ "Content-Disposition" ];
				if ( disp != null )
				{
					Console.Error.WriteLine( "Content-Disposition: " + disp );
					string semi = ";";
					string eq = "=";
					string qt = "\"";
					string[] options = disp.Split( semi.ToCharArray() );
					foreach( string option in options )
					{
						if ( option.ToLower().StartsWith( "file" ) == true )
						{
							string[] nameValues = option.Split( eq.ToCharArray() );
							if ( nameValues != null )
							{
								string filename = nameValues[1].TrimStart( qt.ToCharArray() );
								filename = filename.TrimEnd( qt.ToCharArray() );
								Console.Error.WriteLine( "filename: " + filename );
							}
						}
					}
				}
				// Need to find filename=
				// Format inline; filename="foo.bar"

				try
				{
					// Get the stream associated with the response.
					Stream receiveStream = response.GetResponseStream();
					
					Console.WriteLine( "reading " + Length.ToString() + " bytes" );

					byte[] buffer = null;
					try
					{
						buffer = new byte[ Length ];
						int read = receiveStream.Read( buffer, 0, (int) Length );
						if ( read != 0 )
						{
							Writer.Write( buffer, 0, read );
						}
					}
					catch
					{
						downloadError = true;
					}
					finally
					{
						receiveStream.Close();
						if ( buffer != null )
						{
							buffer = null;
						}
					}
				}
				finally
				{
					response.Close();
				}
			}	
		}

		private void LoadFromXmlNode( XmlNode Node)
		{
			if ( Node.Name.ToLower() != "enclosure" )
			{
				throw new ApplicationException( "Node is not of type enclosure" );
			}

			foreach( XmlAttribute attr in Node.Attributes )
			{
				switch( attr.Name.ToLower() )
				{
					case "url":
					{
						this.url = attr.InnerText;
						break;
					}

					case "type":
					{
						this.type = attr.InnerText;
						break;
					}

					case "length":
					{
						this.length = System.Convert.ToInt64( attr.InnerText );
						break;
					}
				}
			}
		}
		#endregion

		#region Public Methods
		public void Download( Feed Parent, string LocalPath, string AlternateName, int Timeout, bool Overwrite )
		{
			overwrite = Overwrite;
			timeout = Timeout;
			username = Parent.Username;
			password = Parent.Password;

			downloadError = false;

			// Local path exist?
			DirectoryInfo info = Directory.CreateDirectory( LocalPath );

			workFileName = Guid.NewGuid().ToString();
			string fullWorkPath = String.Format( "{0}{1}{2}", LocalPath, Path.DirectorySeparatorChar, workFileName );
			string fullDestPath = null;

			// If the caller wants to set an alternate file name for the local
			// copy so we can check if the file exists here and now!
			if ( AlternateName != null )
			{
				fullDestPath = String.Format( "{0}{1}{2}", LocalPath, Path.DirectorySeparatorChar, AlternateName );
				localFileExists = File.Exists( fullDestPath );

				// Caller doesn't want to overwrite
				if ( Overwrite == false )
				{
					throw new ApplicationException( String.Format( "File: {0} already exists", fullDestPath ) );
				}
			}

			FileStream writer = null;

			try
			{
				writer = File.Create( fullWorkPath );
				this.DownloadChunk( writer, 0, this.length );
			}
			catch ( Exception dl )
			{
				downloadError = true;
				Console.Error.WriteLine( dl.Message );
				Console.Error.WriteLine( dl.StackTrace );
			}
			finally
			{
				if ( writer != null )
				{
					writer.Close();
				}

				if ( downloadError == false )
				{
					bool moveFailure = false;
					string tempPath = null;

					try
					{
						// If the local file exists move it out of the way
						// while we move and setup the new file.  If setup
						// of the new file is successful, delete the old file
						// otherwise move it back.
						if ( localFileExists == true )
						{
							tempPath = 
								String.Format( "{0}{1}{2}", 
									LocalPath, 
									Path.DirectorySeparatorChar, 
									Guid.NewGuid().ToString() );

							File.Move( fullDestPath, tempPath );
						}

						File.Move( fullWorkPath, fullDestPath );
						File.SetLastWriteTime( fullDestPath, this.published );
					}
					catch
					{
						moveFailure = true;
					}
					finally
					{
						if ( tempPath != null )
						{
							if ( moveFailure == true )
							{
								File.Delete( fullDestPath );
								File.Move( tempPath, fullDestPath );
							}
							else
							{
								File.Delete( tempPath );
							}
						}
					}
				}
				else
				{
					File.Delete( fullWorkPath );
				}

				workFileName = null;
			}
		}

		#endregion
	}
}
