/***********************************************************************
 *  $RCSfile: Options.cs,v $
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
using System.Web;
using System.Web.SessionState;

namespace Novell.DavClient
{
	public class PropertyFind : Request
	{
		#region Class Members
		private string resource;
		private ArrayList properties;
		private readonly string propFindBeginTag = "<D:propfind xmlns:D=\"DAV:\">\n";
		private readonly string propFindEndTag = "</D:propfind>\n";
		private readonly string propertyBeginTag = "<D:prop xmlns:S=";
		private readonly string propertyEndTag = "</D:prop>\n";
		private string resourcePath = "/";
		private string schemaUri = "http://www.novell.com/davschema/";
		private string xmlBody = Novell.DavClient.Request.xmlHeader;
		
		#endregion
		
		#region Properties
		public string SchemaUri
		{
			get{ return schemaUri; }
			set{ schemaUri = value; }
		}

		#endregion
		
		
		#region Constructors	
		public PropertyFind( string ServerUri, string Resource, string Username, string Password ) :
			base( ServerUri, Username, Password )
		
		{
			this.resourcePath = Resource;
			this.Method = "PROPFIND";
			this.properties = new ArrayList();
		}
		#endregion

		#region Private Methods
		private void BuildXmlBody()
		{
			xmlBody += propFindBeginTag;
			xmlBody += propertyBeginTag;
			xmlBody += schemaUri;
			xmlBody += "\">";

			foreach( string deadProperty in this.properties )
			{
				xmlBody += "<S:";
				xmlBody += deadProperty;
				xmlBody += "/>";
			}
			
			xmlBody += propertyEndTag;
			xmlBody += propFindEndTag;
		}
		#endregion
		
		#region Public Methods
		
		public void AddProperty( string Name )
		{
			this.properties.Add( Name );
		}
		
		public void RemoveProperty( string Name )
		{
			this.properties.Remove( Name );
		}
		
		public override void Send()
		{
			this.Method = "PROPFIND " + resourcePath + " HTTP/1.1";
			this.BuildXmlBody();
			this.SetBodyContent( xmlBody );
			base.Send();
		}
		
		#endregion
	}
}
