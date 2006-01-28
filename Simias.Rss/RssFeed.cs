/***********************************************************************
 *  $RCSfile: RssFeed.cs,v $
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

namespace Simias.RssFeed
{
	public class Handler : IHttpHandler, IRequiresSessionState
	{
		const string serviceTag = "simias-rss";
		
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
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
					
					log.Debug( "Simias.Rss.Handler.ProcessRequest called" );
					log.Debug( "  method: " + method );
					
					//SyncMethod method = (SyncMethod)Enum.Parse(typeof(SyncMethod), Request.Headers.Get(SyncHeaders.Method), true);
					if ( method == "get" )
					{
						Store store = Store.GetStore();

						// If a query string was not passed in the request return
						// a list of collections off the default domain
						if ( request.QueryString.Count == 0 )
						{
							//Domain domain = store.GetDomain( store.DefaultDomain );

							ICSList ifolders = store.GetCollectionsByType( "iFolder" );
							//ICSList ifolders = store.GetCollectionsByDomain( store.DefaultDomain );
							if ( ifolders.Count > 0 )
							{
								response.StatusCode = (int) HttpStatusCode.OK;
								response.ContentType = "text/xml";
								response.Write( "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" );
								response.Write( "<rss version=\"2.0\">" );
								
								log.Debug( "Number collections: " + ifolders.Count );
								foreach( ShallowNode sn in ifolders )
								{
									log.Debug( "Type: " + sn.Type );
									if ( sn.Name.ToLower().StartsWith( "pobox" ) == false )
									{
										log.Debug( "RSSizing collection: " + sn.Name );
										Simias.RssFeed.Channel channel = 
											new Simias.RssFeed.Channel( context, store, sn );
										channel.Send();
									}
								}
								
								response.Write( "</rss>" );
							}

						}
						else
						{
							// Was a specific channel specified
							string specifiedFeed = request.QueryString[ "feed" ];
							log.Debug( "Processing channel: " + specifiedFeed );
							ICSList list = store.GetCollectionsByName( specifiedFeed, SearchOp.Equal );
							if ( list != null && list.Count > 0 )
							{
								response.StatusCode = (int) HttpStatusCode.OK;
								response.ContentType = "text/xml";
								response.Write( "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" );
								response.Write( "<rss version=\"2.0\">" );

								IEnumerator colEnum = list.GetEnumerator();
								if( colEnum.MoveNext() == true )
								{
									Simias.RssFeed.Channel channel = 
										new Simias.RssFeed.Channel( context, store, colEnum.Current as ShallowNode );
									channel.Send();
								}
								
								response.Write( "</rss>" );
							}
						}
					}
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
