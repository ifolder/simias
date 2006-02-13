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

		public Channel( HttpContext Context, Store CurrentStore, ShallowNode SN )
		{
			ctx = Context;
			store = CurrentStore;
			collection = new Collection( Store.GetStore(), SN );
		}

		public void Send()
		{
			ctx.Response.Write( "<channel>" );

			ctx.Response.Write( "<title>" );
			ctx.Response.Write( collection.Name );
			ctx.Response.Write("</title>");
			
			/*
			if (slog.Description != "")
			{			
				ctx.Response.Write("<description>");
				ctx.Response.Write(slog.Description);
				ctx.Response.Write("</description>");
			}

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

			if ( collection.IsType( "iFolder" ) == true )
			{
				ctx.Response.Write( "<description>iFolder</description>" );
			}
			
			//IEnumerator nodesEnum = null;
			//ICSList nodes = collection.GetNodesByType( "FileNode" );
			
			DateTime latest;
			Property colProp = collection.Properties.GetSingleProperty( Simias.RssFeed.Util.LastModified );
			if ( colProp != null )
			{
				latest = (DateTime) colProp.Value;
			}
			else
			{
				latest = collection.CreationTime;
			}

			ArrayList chrono = new ArrayList();
			DateTime dt = new DateTime( 1992, 1, 1, 0, 0, 0 );
			ICSList nodes = collection.Search( Simias.RssFeed.Util.LastModified, dt, SearchOp.Greater );
			foreach( ShallowNode sn in nodes )
			{
				if ( sn.Type == "FileNode" || sn.Type == "DirNode" )
				{
					chrono.Add( sn );
				}
			}

			// Simias yuckiness
			ItemSort itemSort = new ItemSort();
			chrono.Sort( itemSort );

			if ( latest > ( (Item)chrono[0]).Published )
			{
				Simias.RssFeed.Util.SendPublishDate( ctx, latest );
			}
			else
			{
				Simias.RssFeed.Util.SendPublishDate( ctx, ( (Item) chrono[0]).Published );
			}

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

			ctx.Response.Write( "<ttl>" );
			ctx.Response.Write( "300" );
			ctx.Response.Write( "</ttl>" );
																																																																																																																																																																						
			ctx.Response.Write( "<rating>" );
			ctx.Response.Write( "NC-17" );
			ctx.Response.Write( "</rating>" );
			
			if ( items == true )
			{
				foreach( Item item in chrono )
				{
					//log.Debug( "Processing item: " + item.sn.Name );
					item.Send( ctx );
				}
			}

			ctx.Response.Write( "</channel>" );
		}
	}
}
