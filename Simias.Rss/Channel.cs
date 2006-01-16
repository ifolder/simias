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
			
			Simias.RssFeed.Util.SendPublishDate( ctx, collection.CreationTime );

			ctx.Response.Write( "<lastBuildDate>" );
			Property lastWrite = collection.Properties.GetSingleProperty( "LastWrite" );
			if ( lastWrite != null )
			{
				ctx.Response.Write( Util.GetRfc822Date( (DateTime) lastWrite.Value ) );
			}
			else
			{
				ctx.Response.Write( Util.GetRfc822Date( collection.CreationTime ) );
			}
			ctx.Response.Write( "</lastBuildDate>" );

			ctx.Response.Write("<generator>");
			ctx.Response.Write( "Simias" );
			ctx.Response.Write("</generator>");

			/*													
			if (slog.Cloud != "")
			{			
				ctx.Response.Write("<cloud>");
				ctx.Response.Write(slog.Cloud);
				ctx.Response.Write("</cloud>");
			}
			*/

			ctx.Response.Write( "<ttl>" );
			ctx.Response.Write( "300" );
			ctx.Response.Write( "</ttl>" );
																																																																																																																																																																						
			ctx.Response.Write( "<rating>" );
			ctx.Response.Write( "NC-17" );
			ctx.Response.Write( "</rating>" );
			
			if ( items == true )
			{
				//ICSList nodes = collection.GetNodesByType( "FileNode" );
				ICSList nodes = collection.GetNodesByType( "Node" );
				foreach( ShallowNode sn in nodes )
				{
					log.Debug( "Processing item: " + sn.Name );
					Item item = new Item( ctx, collection, sn );
					item.Send();
				}
			}

			ctx.Response.Write( "</channel>" );
		}
	}
}
