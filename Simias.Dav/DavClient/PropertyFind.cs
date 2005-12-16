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
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace Novell.DavClient
{
	public class PropertyFind : Request
	{
		#region Class Members
		private bool all = false;
		private int depth = 0;
		private ArrayList properties;
		private readonly string propFindBeginTag = "<D:propfind xmlns:D=\"DAV:\">\r";
		private readonly string propFindEndTag = "</D:propfind>\r  ";
		private readonly string propertyBeginTag = "<D:prop xmlns:S=";
		private readonly string propertyEndTag = "</D:prop>\r";
		private readonly string allPropTag = "<D:allprop/>\r";
		private string resourcePath = "/";
		private string schemaUri = "http://www.novell.com/davschema/";
		private StringBuilder xmlBody;
		
		#endregion
		
		#region Properties
		public int Depth
		{
			get{ return depth; }
			set{ depth = value; }
		}

		public string SchemaUri
		{
			get{ return schemaUri; }
			set{ schemaUri = value; }
		}

		#endregion
		
		
		#region Constructors	
		public PropertyFind( WebState State, string Resource ) :
			base( State )
		
		{
			this.resourcePath = Resource;
			this.Method = "PROPFIND";
			this.properties = new ArrayList();
		}
		
		public PropertyFind( WebState State, string Resource, bool All ) :
			base( State )
		
		{
			this.resourcePath = Resource;
			this.Method = "PROPFIND";
			this.all = All;
			this.properties = new ArrayList();
		}
		
		#endregion

		#region Private Methods
		private void BuildAllPropBody()
		{
			xmlBody = new StringBuilder( Novell.DavClient.Request.xmlHeader );
			
			xmlBody.Append( propFindBeginTag );
			xmlBody.Append( allPropTag );
			xmlBody.Append( propFindEndTag );
		}

		private void BuildXmlBody()
		{
			StringBuilder prop = new StringBuilder();
			prop.Capacity = 512;
			
			xmlBody = new StringBuilder( Novell.DavClient.Request.xmlHeader );
		
			xmlBody.Append( propFindBeginTag );
			xmlBody.Append( propertyBeginTag );
			xmlBody.Append( schemaUri );
			xmlBody.Append( "\">" );

			foreach( string deadProperty in this.properties )
			{
				prop.Insert( 0, "<S:" );
				prop.Append( deadProperty );
				prop.Append( "/>" );
			
				xmlBody.Append( prop.ToString() );
			}
			
			xmlBody.Append( propertyEndTag );
			xmlBody.Append( propFindEndTag );
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
			if ( this.all == true )
			{
				this.AddHeader( "Depth", "1" );
				BuildAllPropBody();
			}
			else
			{
				if ( depth > 0 )
				{
					this.AddHeader( "Depth", depth.ToString() );
				}
				BuildXmlBody();
			}
			
			this.Method = "PROPFIND";
			this.Resource = resourcePath;
			this.SetBodyContent( xmlBody.ToString() );
			base.Send();
		}
		
		#endregion
	}
}
