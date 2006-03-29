/***********************************************************************
 *  $RCSfile: PropertyFind.cs,v $
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
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Xml;

using Simias;
//using Simias.Storage;

namespace Simias.Dav
{
	public class PropertyFind
	{
		#region Class Members
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			
		HttpContext ctx;		
		#endregion
		
		#region Constructor
		public PropertyFind( HttpContext Context )
		{
			ctx = Context;
		}
		#endregion
		
		#region Public Methods
		public void ProcessRequest()
		{
			Stream requestStream = null;
			StreamReader readStream = null;
	
			try
			{
				requestStream = ctx.Request.InputStream;
				readStream = new StreamReader( requestStream, Encoding.UTF8 );
						
				XmlDocument document = new XmlDocument();
				document.Load( readStream );

				/*
				//Create an XmlNamespaceManager for resolving namespaces.
				XmlNamespaceManager nsmgr = new XmlNamespaceManager( document.NameTable );
				nsmgr.AddNamespace( "wsil", document.DocumentElement.NamespaceURI );

				// Search for the named service element.
				XmlNode serviceNode = document.DocumentElement.SelectSingleNode( WSIL_ServiceTag + "[" + WSIL_NameTag + "='" + "Domain Service" + "']", nsmgr );
				if ( serviceNode != null )
				{
				
				}
				*/
				
				log.Debug( document.ToString() );
				ctx.Response.StatusCode = (int) HttpStatusCode.OK;
			}
			catch( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
			finally
			{
				if ( readStream != null )
				{
					readStream.Close();
				}
				
				if ( requestStream != null )
				{
					requestStream.Close();
				}
			}
		}
		
		public void Write()
		{
			log.Debug( "Write - called" );
		
		}
		
		#endregion
	}
}