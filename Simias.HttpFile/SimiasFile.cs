/***********************************************************************
 *  $RCSfile: SimiasFile.cs,v $
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

using Simias;
using Simias.Storage;

namespace Simias.HttpFile
{
	public class Handler : IHttpHandler, IRequiresSessionState
	{
		const string serviceTag = "simias-http-file";
		
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		public void ProcessRequest( HttpContext context )
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;
			
			try
			{
				// Make sure that there is a session.
				if ( context.Session != null )
				{
					//HttpService service = Session[ serviceTag ];
					response.Cache.SetCacheability( HttpCacheability.NoCache );
					response.ContentType = "image/png";					

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
							Store store;
							
							string domainID = request.QueryString[ "did" ];
							string fileID = request.QueryString[ "fid" ];
							
							if ( domainID != null && fileID != null )
							{
								log.Debug( "  domainID: " + domainID );
								log.Debug( "  fileID: " + fileID );
								store = Store.GetStore();
								domain = Store.GetStore().GetDomain( store.DefaultDomain );
								
								node = domain.GetNodeByID( fileID );
								if ( node != null )
								{
									Property cid = node.Properties.GetSingleProperty( "CollectionId" );
									collection = store.GetCollectionByID ( cid.Value as string );
									fileNode = new FileNode( node );
									string fullPath = fileNode.GetFullPath( collection );
									log.Debug( "  nodename: " + fileNode.Name );
									log.Debug( "  filename: " + fileNode.GetFileName() );
									log.Debug( "  fullpath: " + fullPath );
									
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

								response.StatusCode = (int) HttpStatusCode.OK;
							}
							else
							{
								response.StatusCode = (int) HttpStatusCode.BadRequest;
							}
						}
						else
						{
							response.StatusCode = (int) HttpStatusCode.BadRequest;
						}
					}
					else
					{
						response.StatusCode = (int) HttpStatusCode.BadRequest;
					}
				}
				else
				{
					response.StatusCode = (int) HttpStatusCode.BadRequest;
				}
			}
			catch( Exception ex )
			{
				//Sync.Log.log.Debug("Request Failed exception\n{0}\n{1}", ex.Message, ex.StackTrace);
				throw ex;
			}
			finally
			{
				response.End();
			}
		}

		public bool IsReusable
		{
			// To enable pooling, return true here.
			// This keeps the handler in memory.
			get { return true; }
		}
	}
}
