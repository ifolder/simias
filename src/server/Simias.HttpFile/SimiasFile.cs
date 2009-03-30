/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Brady Anderson (banderso@novell.com)
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
 
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Web;
using System.Web.SessionState;

using Simias;
using Simias.Storage;

namespace Simias.HttpFile
{
	public class Handler : IHttpHandler, IRequiresSessionState
	{
		const string serviceTag = "simias-http-file";
		
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
        /// <summary>
        /// process the http request
        /// </summary>
        /// <param name="context">http context</param>
		public void ProcessRequest( HttpContext context )
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;
			response.StatusCode = (int) HttpStatusCode.BadRequest;

			try
			{
				// Make sure that there is a session.
				if ( context.Session != null )
				{
					//HttpService service = Session[ serviceTag ];
					response.Cache.SetCacheability( HttpCacheability.NoCache );

					string method = request.HttpMethod.ToLower();
					
					log.Debug( "Simias.HttpFile.Handler.ProcessRequest called" );
					log.Debug( "  method: " + method );
					
					//SyncMethod method = (SyncMethod)Enum.Parse(typeof(SyncMethod), Request.Headers.Get(SyncHeaders.Method), true);
					if ( method == "get" )
					{
						// Must a query string which contains the domainid and the 
						// filenodeid for the file caller is attempting to download.
						if ( request.QueryString.Count > 0 )
						{
							Collection collection;
							Domain domain;
							FileNode fileNode;
							Node node;
							Store store = Store.GetStore();
							
							string domainID = request.QueryString[ "did" ];
							if ( domainID == null )
							{
								domainID = store.DefaultDomain;
							}

							string fileID = request.QueryString[ "fid" ];
							if ( fileID != null )
							{
								log.Debug( "  domainID: " + domainID );
								log.Debug( "  fileID: " + fileID );
								domain = Store.GetStore().GetDomain( domainID );
								
								node = domain.GetNodeByID( fileID );
								if ( node != null )
								{
									Property cid = node.Properties.GetSingleProperty( "CollectionId" );
									collection = store.GetCollectionByID ( cid.Value as string );
									fileNode = new FileNode( node );
									string fullPath = fileNode.GetFullPath( collection );
									string fileName = fileNode.GetFileName();

									log.Debug( "  nodename: " + fileNode.Name );
									log.Debug( "  filename: " + fileName );
									log.Debug( "  fullpath: " + fullPath );

									response.StatusCode = (int) HttpStatusCode.OK;

									Property lastModified = node.Properties.GetSingleProperty( "LastModified" );
									if ( lastModified != null )
									{
										DateTime dt = (DateTime) lastModified.Value;
										response.AddHeader(
											"Last-Modified",
											Util.GetRfc822Date( (DateTime) lastModified.Value ) );
									}
							
									response.AddHeader(
										"Content-length",
										fileNode.Length.ToString() );	
							
									response.ContentType = 
										Simias.HttpFile.Response.GetMimeType( fileName ).ToLower();
									
									if ( response.ContentType.Equals( "text/plain" ) )
									{
										response.AddHeader(
											"Content-Disposition",
											"inline; filename=\"" + fileName + "\"" );
									}
									else
									if ( response.ContentType.Equals( "text/xml" ) )
									{
										response.AddHeader(
											"Content-Disposition",
											"inline; filename=\"" + fileName + "\"" );
									}
									else
									if ( response.ContentType.StartsWith( "image" ) )
									{
										response.AddHeader(
											"Content-Disposition",
											"inline; filename=\"" + fileName + "\"" );
									}
									else
									{
										response.AddHeader(
											"Content-Disposition",
											"attachment; filename=\"" + fileName + "\"" );
									}		
									
									response.TransmitFile( fullPath );
									
									/*
									response.
									FileStream stream = 
										File.Open( 
											fullPath,
											System.IO.FileMode.Open,
											System.IO.FileAccess.Read,
											System.IO.FileShare.Read );
									*/
											
								}
							}
						}
					}
				}
			}
			catch( Exception ex )
			{
				log.Error( ex.Message );
				log.Error( ex.StackTrace );

				response.StatusCode = (int) HttpStatusCode.InternalServerError;
			}
			finally
			{
				response.End();
			}
		}
        /// <summary>
        /// currently returns true always
        /// </summary>
		public bool IsReusable
		{
			// To enable pooling, return true here.
			// This keeps the handler in memory.
			get { return true; }
		}
	}

	public class Response
	{
		/*
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		*/

		static char[] dotSep = {'.'};
        /// <summary>
        /// Get mime type of the file
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
		static public string GetMimeType( string FileName )
		{
			string[] comps = FileName.Split( dotSep );
			if ( comps.Length == 0 )
			{
				return "text/plain";
			}

			switch( comps[ comps.Length - 1 ].ToLower() )
			{
				case "mp3":
					return "audio/mpeg";
	
				case "m4a":
					return "audio/mpeg";

				case "wma":
					return "audio/x-ms-wma";

				case "aac":
					return "audio/ac3";

				case "mpeg":
					return "video/mpeg";

				case "avi":
					return "video/avi";
					
				case "wmv":
					return "video/x-ms-wmv";

				case "wmx":
					return "video/x-ms-wmx";

				case "mov":
					return "video/quicktime";

				case "png":
					return "image/png";
				case "jpg":
				case "jpeg":
					return "image/jpg";

				case "gif":
					return "image/gif";

				case "tiff":
					return "image/tiff";

				case "xml":
					return "text/xml";

				case "exe":
					return "application/exe";

				default:
					return "text/plain";;
			}
		}
	}

	public class Util
	{
		public static string[] MonthsOfYear =
		{
			"Jan",
			"Feb",
			"Mar",
			"Apr",
			"May",
			"Jun",
			"Jul",
			"Aug",
			"Sep",
			"Oct",
			"Nov",
			"Dec"
		};
		
		static public string GetRfc822Date( DateTime DT )
		{
			return
				String.Format(
				"{0}, {1} {2} {3} {4}:{5}:{6} GMT",
				DT.DayOfWeek.ToString(),
				DT.Day,
				Util.MonthsOfYear[ DT.Month - 1 ],
				DT.Year.ToString(),
				DT.Hour,
				DT.Minute,
				DT.Second );
		}
	}
}
