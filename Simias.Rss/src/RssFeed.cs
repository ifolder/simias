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
using System.Security.Principal;
using System.Threading;
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
		
		private string channelType;
		private string itemType;
		private bool strict = true;
		private bool items = true;
		private bool enclosures = false;
		private string specifiedFeed;
			
		private void ParseUrlQueryOptions( HttpRequest Request )
		{
			if ( Request.QueryString.Count > 0 )
			{
				specifiedFeed = Request.QueryString[ "feed" ];
				channelType = Request.QueryString[ "channel-type" ];
				itemType = Request.QueryString[ "item-type" ];
				
				// Simias clients may turn strictness off to get
				// more information about the Collection or File
				if ( Request.QueryString[ "strict" ] != null &&
						Request.QueryString[ "strict" ].ToLower() == "false" )
				{
					strict = false;
				}
				
				if ( Request.QueryString[ "enclosures" ] != null )
				{
					enclosures = 
						( Request.QueryString[ "enclosures" ].ToLower() == "true" ) 
							? true : false;
				}
				
				if ( Request.QueryString[ "items" ] != null )
				{
					items = 
						( Request.QueryString[ "items" ].ToLower() == "false" ) 
							? false : true;
				}
			}
		}
		
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
						ParseUrlQueryOptions( request );

						// Impersonate the caller.
						Domain domain = store.GetDomain( store.DefaultDomain );
						Member member = 
							domain.GetMemberByID( Thread.CurrentPrincipal.Identity.Name );
						domain.Impersonate( member );
						
						// If a query string was not passed in the request
						// then default behavior should be returned.
						//
						// The current default behavior is to return all
						// iFolders/Invitations the calling users owns or
						// is a member of.  Also, default behavior will
						// return strict RSS.
						if ( request.QueryString.Count == 0 )
						{
							ICSList ifolders = store.GetCollectionsByType( "iFolder" );
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
										channel.Items = false;
										channel.Enclosures = false;
										channel.Send();
									}
								}
								
								response.Write( "</rss>" );
							}
						}
						else
						if ( specifiedFeed != null )
						{
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
									channel.Enclosures = this.enclosures;
									channel.Items = this.items;
									channel.Strict = this.strict;
									channel.Send();
								}
								
								response.Write( "</rss>" );
							}
						}
						else
						if ( channelType != null )
						{
							log.Debug( "Processing channel type: " + channelType );
							ICSList list = store.GetCollectionsByType( channelType );
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
									channel.Enclosures = this.enclosures;
									channel.Items = this.items;
									channel.Strict = this.strict;
									channel.Send();
								}
								
								response.Write( "</rss>" );
							}
						}
						else
						{
							ICSList ifolders = store.GetCollectionsByType( "iFolder" );
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
										channel.Items = this.items;
										channel.Strict = this.strict;
										channel.Enclosures = this.enclosures;
										channel.Send();
									}
								}
								
								response.Write( "</rss>" );
							}
						}
					}
				}
			}
			catch( Exception ex )
			{
				log.Error( ex.Message );
				log.Error( ex.StackTrace );
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
