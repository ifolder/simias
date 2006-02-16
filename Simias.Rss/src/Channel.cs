/***********************************************************************
 *  $RCSfile: Channel.cs,v $
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
using System.Web;

using Simias;
using Simias.Storage;
using Simias.Storage.Provider;

namespace Simias.RssFeed
{
	/// <summary>
	/// Summary description for Channel.
	/// </summary>
	public class Channel
	{
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
		//private bool detailed = false;
		private bool items = true;
		private bool enclosures = false;
		private bool strict = true;
		private ArrayList types;
		private HttpContext ctx;
		private Collection collection;
		private Store store;
		
		public bool Items
		{
			get{ return items; }
			set{ items = value; }
		}
		
		public bool Enclosures
		{
			get{ return enclosures; }
			set{ enclosures = value; }
		}

		public bool Strict
		{
			get{ return strict; }
			set{ strict = value; }
		}
		
		public Channel( HttpContext Context, Store CurrentStore, ShallowNode SN )
		{
			ctx = Context;
			store = CurrentStore;
			collection = new Collection( Store.GetStore(), SN );
			types = new ArrayList();
		}

		public Channel( HttpContext Context, Store CurrentStore, Collection ThisCollection )
		{
			ctx = Context;
			store = CurrentStore;
			collection = ThisCollection;
			types = new ArrayList();
		}

		public void Send()
		{
			ctx.Response.Write( "<channel>" );

			ctx.Response.Write( "<title>" );
			ctx.Response.Write( collection.Name );
			ctx.Response.Write("</title>");
			
			/*
			if (slog.Link != "")
			{			
				ctx.Response.Write("<link>");
				ctx.Response.Write( slog.Link );
				ctx.Response.Write("</link>");
			}
			*/

			ctx.Response.Write( "<language>" );
			ctx.Response.Write( "en-us" );
			ctx.Response.Write( "</language>" );

			ctx.Response.Write( "<copyright>" );
			ctx.Response.Write( "(c) Novell, Inc." );
			ctx.Response.Write( "</copyright>" );

			ctx.Response.Write( "<managingEditor>" );
			Domain domain = store.GetDomain( store.DefaultDomain );
			if ( collection.Owner.FN != null && collection.Owner.FN != "" )
			{
				ctx.Response.Write( collection.Owner.FN );
			}
			else
			{
				ctx.Response.Write( collection.Owner.Name );
			}
			ctx.Response.Write( "</managingEditor>" );

			ctx.Response.Write( "<webmaster>" );
			//member = domain.GetMemberByID( domain.Owner );
			if ( domain.Owner.FN != null && domain.Owner.FN != "" )
			{
				ctx.Response.Write( domain.Owner.FN );
			}
			else
			{
				ctx.Response.Write( domain.Owner.Name );
			}
			ctx.Response.Write("</webmaster>");

			try
			{
				Simias.Storage.Property descProp = 
					collection.Properties.GetSingleProperty( "Description" );
				if ( descProp != null )
				{
					ctx.Response.Write( "<description>" + descProp.Value.ToString() + "</description>" );
				}
				else
				{
					ctx.Response.Write( "<description>" + collection.Type.ToString() + "</description>" );
				}
			}
			catch{}
			
			Simias.Storage.Property colProp = 
				collection.Properties.GetSingleProperty( Simias.RssFeed.Util.LastModified );
			DateTime latest = ( colProp != null ) ? (DateTime) colProp.Value : collection.CreationTime;

			DateTime dt = new DateTime( 1992, 1, 1, 0, 0, 0 );
			ICSList nodes = collection.Search( Simias.RssFeed.Util.LastModified, dt, SearchOp.Greater );
			ICSEnumerator nodesEnum = null;
			if ( nodes.Count > 0 )
			{
				nodesEnum = nodes.GetEnumerator() as ICSEnumerator;
				if ( nodesEnum != null &&
						nodesEnum.SetCursor( IndexOrigin.SET, nodesEnum.Count - 1 ) == true )
				{
					nodesEnum.MoveNext();
				
					try
					{
						ShallowNode sn = nodesEnum.Current as ShallowNode;
						log.Debug( "sn: " + sn.Name );
				
						Item item = new Item( collection, sn );
						if ( item.Published > latest )
						{
							latest = item.Published;
						}
					}
					catch( Exception e )
					{
						log.Debug( e.Message );
					}
				}
			}
			
			/*
			foreach( ShallowNode sn in nodes )
			{
				if ( sn.Type == "FileNode" || sn.Type == "DirNode" )
				{
					log.Debug( "sn: " + sn.Name );
					Item item = new Item( collection, sn );
					chrono.Add( item );
				}
			}

			if ( chrono.Count > 0 )
			{
				ItemSort itemSort = new ItemSort();
				chrono.Sort( itemSort );

				if ( latest > ( (Item)chrono[0]).Published )
				{
					Simias.RssFeed.Util.SendPublishDate( ctx, latest );
				}
				else
				{
					Simias.RssFeed.Util.SendPublishDate( ctx, ( (Item) chrono[0]).Published );
					latest = ( (Item) chrono[0]).Published;
				}
			}
			else
			{
				Simias.RssFeed.Util.SendPublishDate( ctx, latest );
			}
			*/

			Simias.RssFeed.Util.SendPublishDate( ctx, latest );
			
			ctx.Response.Write( "<lastBuildDate>" );
			ctx.Response.Write( Util.GetRfc822Date( latest ) );
			ctx.Response.Write( "</lastBuildDate>" );

			ctx.Response.Write("<generator>");
			ctx.Response.Write( "Simias" );
			ctx.Response.Write("</generator>");

			/*													
				ctx.Response.Write("<cloud>");
				ctx.Response.Write( node.Cloud );
				ctx.Response.Write("</cloud>");
			*/

			ctx.Response.Write( "<ttl>300</ttl>" );
			ctx.Response.Write( "<rating>PG-13</rating>" );
			
			if ( strict == false )
			{
				ctx.Response.Write( "<authorID>" + collection.Owner.UserID + "</authorID>" );
				ctx.Response.Write( "<type>" + collection.Type.ToString() + "</type>" );
				ctx.Response.Write( "<id>" + collection.ID + "</id>" );
			}
			

			if ( items == true && nodesEnum != null )
			{
				nodesEnum = nodes.GetEnumerator() as ICSEnumerator;
				int count = nodesEnum.Count;				
				while( count-- > 0 )
				{
					if ( nodesEnum.SetCursor( IndexOrigin.SET, count ) == true )
					{
						nodesEnum.MoveNext();
						ShallowNode sn = nodesEnum.Current as ShallowNode;
						Item item = null;
						if ( this.types.Count == 0 )
						{
							if ( sn.Type == "FileNode" )
							{
								item = new Item( collection, sn );
							}
						}
						else
						{
							foreach( string ctype in this.types )
							{
								if ( ctype == sn.Type )
								{
									item = new Item( collection, sn );
									break;
								}
							}
						}
					
						if ( item != null )
						{
							item.Strict = this.strict;
							item.Enclosures = this.enclosures;
							item.Send( ctx );
						}
					}
				}
			}	

			ctx.Response.Write( "</channel>" );
		}
	}
}
