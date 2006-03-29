/***********************************************************************
 *  $RCSfile: PublicFeedHandler.cs,v $
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
	public class PublicHandler : IHttpHandler, IRequiresSessionState
	{
		const string serviceTag = "simias-rss";
		
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		//private string channelType;
		//private string itemType = null;
		private bool strict = true;
		private bool items = true;
		private bool enclosures = false;
		private string specifiedFeed;
		private bool published = false;
		private string searchTerm;
			
		private void ParseUrlQueryOptions( HttpRequest Request )
		{
			if ( Request.QueryString.Count > 0 )
			{
				log.Debug( "ParseUrlQueryOptions" );
				
				specifiedFeed = Request.QueryString[ "feed" ];
				//channelType = Request.QueryString[ "channel-type" ];
				//itemType = Request.QueryString[ "item-type" ];
				
				// Is the caller looking for published collections
				if ( Request.QueryString[ "pub" ] != null &&
						Request.QueryString[ "pub" ].ToLower() == "true" )
				{
					published = true;
				}

				searchTerm = Request.QueryString[ "query" ];
				
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
						Domain domain = store.GetDomain( store.DefaultDomain );
						Member member = null;
						
						ParseUrlQueryOptions( request );

						// If the caller is not looking for published collections
						// impersonate him
						if ( published == false )
						{
							// Impersonate
							member = domain.GetMemberByID( Thread.CurrentPrincipal.Identity.Name );
							domain.Impersonate( member );
							log.Debug( "Impersonating user: " + member.Name );
						}	
						else
						{
							member = domain.GetCurrentMember();
						}
						
						// If a query string was not passed in the request
						// then default behavior should be returned.
						//
						// The current default behavior is to return all
						// iFolders/Invitations the calling users owns or
						// is a member of.  Also, default behavior will
						// return strict RSS.
						if ( request.QueryString.Count == 0 )
						{
							bool foundOne = false;
							Property searchProp = new Property( "Published", true );
							ICSList list = store.GetNodesByProperty( searchProp, SearchOp.Equal );
							foreach( ShallowNode sn in list )
							{
								if ( sn.IsBaseType( "Collection" ) )
								{
									Collection col = new Collection( store, sn );
									if ( col.Domain == domain.ID )
									{
										Simias.RssFeed.Channel channel = 
											new Simias.RssFeed.Channel( context, store, col );
										channel.Enclosures = false;
										channel.Items = false;
										channel.Strict = false;
											
										if ( foundOne == false )
										{
											response.StatusCode = (int) HttpStatusCode.OK;
											response.ContentType = "text/xml";
											response.Write( "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" );
											response.Write( "<rss version=\"2.0\">" );
											foundOne = true;
										}
											
										channel.Send();
									}
								}		
							}
								
							if ( foundOne == true )
							{
								response.Write( "</rss>" );
							}
						}
						else
						if ( specifiedFeed == null && searchTerm != null )
						{
							log.Debug( "looking for collections that contain: " + searchTerm );
								
							Hashtable ht = new Hashtable();
							ICSList list = null;

							Property descProp = new Property( "Description", searchTerm );
							list = store.GetNodesByProperty( descProp, SearchOp.Contains );
							if ( list != null && list.Count > 0 )
							{
								foreach( ShallowNode sn in list )
								{
									if ( sn.IsBaseType( "Collection" ) )
									{
										Collection col = new Collection( store, sn );
										if ( col.Domain == domain.ID )
										{
											Property pub = col.Properties.GetSingleProperty( PropertyTags.Published );
											if ( (bool) pub.Value == true )
											{
												ht.Add( col.ID, col );
											}
										}
									}
								}
							}
								
							list = store.GetCollectionsByName( this.searchTerm, SearchOp.Contains );
							if ( list != null && list.Count > 0 )
							{
								foreach( ShallowNode sn in list )
								{
									log.Debug( "found in name: " + sn.Name );
									if ( ht.Contains( sn.ID ) == false )
									{
										Collection col = new Collection( store, sn );
										if ( col.Domain == domain.ID )
										{
											Property pub = col.Properties.GetSingleProperty( PropertyTags.Published );
											if ( (bool) pub.Value == true )
											{
												ht.Add( col.ID, col );
											}
										}
									}
								}
							}
								
							if ( ht.Count > 0 )
							{
								response.StatusCode = (int) HttpStatusCode.OK;
								response.ContentType = "text/xml";
								response.Write( "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" );
								response.Write( "<rss version=\"2.0\">" );

								foreach( Collection col in ht.Values )
								{
									Simias.RssFeed.Channel channel = 
										new Simias.RssFeed.Channel( context, store, col );
									channel.Enclosures = this.enclosures;
									channel.Items = this.items;
									channel.Strict = this.strict;
									channel.Send();
								}
							
								response.Write( "</rss>" );
							}
						}
						else
						if ( specifiedFeed != null )
						{
							log.Debug( "Processing channel: " + specifiedFeed );
							Collection col = store.GetSingleCollectionByName( specifiedFeed );
							if ( col != null )
							{
								Property pub = col.Properties.GetSingleProperty( PropertyTags.Published );
								if ( (bool) pub.Value == true )
								{
									response.StatusCode = (int) HttpStatusCode.OK;
									response.ContentType = "text/xml";
									response.Write( "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" );
									response.Write( "<rss version=\"2.0\">" );
								
									Simias.RssFeed.Channel channel = 
										new Simias.RssFeed.Channel( context, store, col );
									channel.Enclosures = this.enclosures;
									channel.Items = this.items;
									channel.Strict = this.strict;
									channel.Send();
								
									response.Write( "</rss>" );
								}	
							}
						}
						/*
						else
						if ( channelType != null )
						{
							bool foundOne = false;
							log.Debug( "Processing channel type: " + channelType );
							ICSList list = store.GetCollectionsByType( channelType );
							if ( list != null && list.Count > 0 )
							{
								IEnumerator colEnum = list.GetEnumerator();
								if( colEnum.MoveNext() == true )
								{
									Collection col = new Collection( store, colEnum.Current as ShallowNode );
									if ( col.IsAccessAllowed( member, Access.Rights.ReadOnly ) )
									{
										Simias.RssFeed.Channel channel = 
											new Simias.RssFeed.Channel( context, store, col );
										channel.Enclosures = this.enclosures;
										channel.Items = this.items;
										channel.Strict = this.strict;
										
										if ( foundOne == false )
										{
											response.StatusCode = (int) HttpStatusCode.OK;
											response.ContentType = "text/xml";
											response.Write( "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" );
											response.Write( "<rss version=\"2.0\">" );
											foundOne = true;
										}
											
										channel.Send();
										
										if ( foundOne == true )
										{
											response.Write( "</rss>" );
										}
									}
								}
							}
						}
						*/
						else
						{
							bool foundOne = false;
							ICSList ifolders = store.GetCollectionsByType( "iFolder" );
							foreach( ShallowNode sn in ifolders )
							{
								Collection col = new Collection( store, sn );
								Property pub = col.Properties.GetSingleProperty( PropertyTags.Published );
								if ( (bool) pub.Value == true )
								{
									Simias.RssFeed.Channel channel = 
											new Simias.RssFeed.Channel( context, store, sn );
									channel.Items = false;
									channel.Strict = this.strict;
									channel.Enclosures = this.enclosures;
											
									if ( foundOne == false )
									{
										response.StatusCode = (int) HttpStatusCode.OK;
										response.ContentType = "text/xml";
										response.Write( "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" );
										response.Write( "<rss version=\"2.0\">" );
										foundOne = true;
									}
											
									channel.Send();
								}
							}
										
							if ( foundOne == true )
							{
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
