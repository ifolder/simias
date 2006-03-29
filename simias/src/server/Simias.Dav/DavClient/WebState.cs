/***********************************************************************
 *  $RCSfile: WebState.cs,v $
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
	public class WebState
	{
		#region Class Members
		private string serviceUrl;
		private CookieContainer cookies;
		private NetworkCredential credential;
		#endregion
		
		
		#region Properties
		public string ServiceUrl
		{
			get{ return serviceUrl; }
		}

		public string Username
		{
			get{ return credential.UserName; }
		}
		#endregion
		
		#region Constructors
		public WebState( string ServiceUrl, string Username, string Password )
		{
			serviceUrl = ServiceUrl;
			credential = new NetworkCredential( Username, Password );
			cookies = new CookieContainer();
		}
		#endregion

		#region Public Methods
		public void SetRequestState( HttpWebRequest request )
		{
			request.CookieContainer = cookies;
			request.Credentials = credential;
			request.PreAuthenticate = true;
		}
		#endregion
	}
}	
