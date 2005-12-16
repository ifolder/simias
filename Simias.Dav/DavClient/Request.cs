/***********************************************************************
 *  $RCSfile: Request.cs,v $
 * 
 *  Copyright (C) 2005 Novell, Inc.
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
using System.Web.SessionState;

namespace Novell.DavClient
{
	public class Request
	{
		private class HttpHeader
		{
			public string Name;
			public string Value;
		}
		
		#region Class Members
		private CookieContainer cookies;
		private HttpWebResponse response = null;
		private HttpWebRequest request = null;
		private	HttpStatusCode status = 0;
		private int contentLength = 0;
		private string content = "";
		private string method = null;
		private string password = null;
		private string username = null;
		private Uri serverUri = null;
		private string resource = null;
		private ArrayList headers;
		static internal readonly string xmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
		private readonly string contentType = "text/xml; charset=\"utf-8\"";
		#endregion
	
		#region Properties

		public string Method
		{
			get{ return method; }
			set{ method = value.ToUpper(); }
		}
		
		public string Password
		{
			get{ return password; }
			set{ password = value; }
		}
		
		public Uri ServerUri
		{
			get{ return serverUri; }
			set{ serverUri = value; }
		}
		
		public string Resource
		{
			get{ return resource; }
			set{ resource = value; }
		}
		
		public string Username
		{
			get{ return username; }
			set{ username = value; }
		}
		
		public HttpStatusCode ResponseStatus
		{
			get
			{
				if ( response != null )
				{
					return status;
				}
				
				
				// throw exception
				return 0;
			}
		}
		
		#endregion
		
		#region Constructors
		public Request( string ServerUrl, string Username, string Password )
		{
			serverUri = new Uri( ServerUrl );
			username = Username;
			password = Password;
			
			cookies = new CookieContainer();
			
			headers = new ArrayList();
			method = "POST";
		}
		#endregion
	
	
		public void AddHeader( string Name, string Value )
		{
			HttpHeader header = new HttpHeader();
			header.Name = Name;
			header.Value = Value;
			
			headers.Add( header);
		}

		public void RemoveHeader( string Name )
		{
			return;
		}
		
		public string GetResponseHeader( string Name )
		{
			if ( response != null )
			{
				return response.GetResponseHeader( Name );
			}
			
			return null;
		}
		
		public void SetBodyContent( string content )
		{
			contentLength = content.Length;
			this.content = content;
		}
		
		public virtual void Send()
		{
			if ( resource != null )
			{
				Uri requestUri = new Uri( serverUri.ToString() + resource );
				Console.WriteLine( "request URI: " + requestUri.ToString() );
				request = WebRequest.Create( requestUri ) as HttpWebRequest;
			}
			else
			{
				request = WebRequest.Create( serverUri ) as HttpWebRequest;
			}
				

			request.CookieContainer = cookies;

			NetworkCredential creds = new NetworkCredential();
			creds.UserName = this.username;
			creds.Password = this.password;
			
			request.Credentials = creds;
			request.PreAuthenticate = true;
			request.ContentType = contentType;
			
			foreach( HttpHeader header in headers )
			{
				request.Headers.Add( header.Name, header.Value ); 
			}
			
			request.Method = method;
			request.ProtocolVersion = HttpVersion.Version11;
			request.ContentLength = contentLength;
			
			try
			{
				if ( contentLength > 0 && content != "" )
				{
					StreamWriter s = new StreamWriter( request.GetRequestStream(), Encoding.UTF8 );
					//StreamWriter s = new StreamWriter( request.GetRequestStream(), Encoding.ASCII );
                    s.Write( content );
                    s.Close();				
				}
				request.GetRequestStream().Close();
				
				response = request.GetResponse() as HttpWebResponse;
				if ( response != null )
				{
					request.CookieContainer.Add( response.Cookies );
					
					status = HttpStatusCode.OK;
				}
			}
			catch(WebException webEx)
			{
				status = (HttpStatusCode) webEx.Status;
				Console.WriteLine( webEx.Status.ToString() );
				Console.WriteLine( webEx.Message );
			}
			catch(Exception ex)
			{
				Console.WriteLine( ex.Message );
				throw ex;
			}

//			return status;
		
		}
	
	}
}
