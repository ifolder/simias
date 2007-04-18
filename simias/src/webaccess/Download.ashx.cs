/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.Net;
using System.Resources;
using System.Text;
using Simias.Encryption;
using Simias.Storage;
//using iFolder.WebService;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Download
	/// </summary>
	public class Download : IHttpHandler, IRequiresSessionState
	{
		/// <summary>
		/// File Transfer Buffer Size
		/// </summary>
		private const int BUFFERSIZE = (64 * 1024);

		/// <summary>
		/// Constructor
		/// </summary>
		public Download()
		{
		}

		#region IHttpHandler Members

		/// <summary>
		/// Process the Request
		/// </summary>
		/// <param name="context">The HttpContext object</param>
		public void ProcessRequest(HttpContext context)
		{
			Blowfish	bf=null;
		  	int		boundary=0;
			int 		count=0;
			long		bytesWritten = 0;
			// query
			string ifolderID = context.Request.QueryString["iFolder"];
			string entryID = context.Request.QueryString["Entry"];			
			
			try
			{
				// connection
				iFolderWeb web = (iFolderWeb)context.Session["Connection"];
				if (web == null) context.Response.Redirect("Login.aspx");

				// request
				UriBuilder uri = new UriBuilder(web.Url);

			        // Location of ifolder.
                                string ifolderLocation = web.GetiFolderLocation (ifolderID);

				UriBuilder remoteurl = new UriBuilder(ifolderLocation);
				remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
				web.Url = remoteurl.Uri.ToString();

				uri.Path = String.Format("/simias10/Download.ashx?iFolder={0}&Entry={1}", ifolderID, entryID);

				HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(uri.Uri);
				webRequest.Method = "GET";
				webRequest.PreAuthenticate = true;
				webRequest.Credentials = web.Credentials;
				webRequest.CookieContainer = web.CookieContainer;

				HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

				Stream webStream = webResponse.GetResponseStream();

				// filename
				string filename = webResponse.Headers["Content-Disposition"];
				filename = filename.Substring(filename.IndexOf('=') + 1);

				// filename fix-up for Firefox and Safari
				if ((context.Request.UserAgent.IndexOf("Firefox") != -1)
					|| (context.Request.UserAgent.IndexOf("Safari") != -1))
				{
					filename = HttpUtility.UrlDecode(filename, System.Text.Encoding.UTF8);
				}

				// response
				iFolder ifolder = web.GetiFolder(ifolderID);
				iFolderEntry nodeEntry = web.GetEntry(ifolderID, entryID);
				
				context.Response.Clear();
				context.Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", filename));
				//context.Response.AddHeader("Content-Length", webResponse.ContentLength.ToString());
				context.Response.AddHeader("Content-Length", nodeEntry.Size.ToString());//actual size (padd bytes discarded below)
				context.Response.ContentType = "application/octet-stream";
				context.Response.BufferOutput = false;


				if(ifolder.EncryptionAlgorithm !="")
				{  
					string PassPhrase = context.Request.QueryString["PassPhrase"];
					UTF8Encoding utf8 = new UTF8Encoding();
					string DecryptedCryptoKey;

					//Hash the passphrase and use it for encryption and decryption
					PassphraseHash hash = new PassphraseHash();
					byte[] passphrase = hash.HashPassPhrase(PassPhrase);	
					
					Key key = new Key(ifolder.EncryptionKey);
					key.DecrypytKey(passphrase, out DecryptedCryptoKey);
					//Decrypt the key using passphrase and use it
					bf = new Blowfish(utf8.GetBytes(DecryptedCryptoKey));
					boundary = 8;					
				}

				try
				{
					Stream output = context.Response.OutputStream;

					byte[] buffer = new byte[BUFFERSIZE];

					while((count = webStream.Read(buffer, 0, BUFFERSIZE)) > 0)
					{					
						if(ifolder.EncryptionAlgorithm !="")
						{
							bf.Decipher (buffer, count);
							
							if((bytesWritten+count) > nodeEntry.Size)
								count = count -(boundary - (int)(nodeEntry.Size % boundary));
							
						}														
						output.Write(buffer, 0, count);

						bytesWritten +=count;
						output.Flush();
					}
				}
				finally
				{
					webStream.Close();
				}
			}
			catch
			{
				ResourceManager rm = (ResourceManager) context.Application["RM"];

				context.Server.Transfer(String.Format(
					"{0}&Message={1}",
					context.Request.UrlReferrer,
					context.Server.UrlEncode(WebUtility.GetString("ENTRY.FAILEDDOWNLOAD", rm))));
			}
		}

		/// <summary>
		/// Is this instance reusable?
		/// </summary>
		public bool IsReusable
		{
			get { return false; }
		}

		#endregion
	}
}
