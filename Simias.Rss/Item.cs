/***********************************************************************
 *  $RCSfile: Item.cs,v $
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
	/// Summary description for Item.
	/// </summary>
	public class Item
	{
		//private bool detailed = false;
		private bool enclosures = false;
		private HttpContext ctx;
		private Collection collection;
		private Node node;
		private Store store;
		
		public bool Enclosures
		{
			get{ return enclosures; }
			set{ enclosures = value; }
		}

		public Item( HttpContext Context, Collection ParentCollection, ShallowNode SN )
		{
			ctx = Context;
			store = Store.GetStore();
			collection = ParentCollection;
			node = new Node( collection, SN );
		}

		public void Send()
		{
			ctx.Response.Write( "<item>" );

			ctx.Response.Write( "<description>" );
			ctx.Response.Write( node.Name );
			ctx.Response.Write("</description>");
			
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


			ctx.Response.Write( "<author>" );
			Domain domain = store.GetDomain( store.DefaultDomain );
			Member member = domain.GetMemberByID( node.Creator );
			if ( member.FN != null && member.FN != "" )
			{
				ctx.Response.Write( member.FN );
			}
			else
			{
				ctx.Response.Write( member.Name );
			}
			ctx.Response.Write( "</author>" );

			// Category  - use tags and types
			
			ctx.Response.Write( "<guid>" + node.ID + "</guid>" );
			Simias.RssFeed.Util.SendPublishDate( ctx, node.CreationTime );


			/*
			if (slog.Generator != "")
			{			
				ctx.Response.Write("<generator>");
				ctx.Response.Write(slog.Generator);
				ctx.Response.Write("</generator>");
			}
													
			if (slog.Cloud != "")
			{			
				ctx.Response.Write("<cloud>");
				ctx.Response.Write(slog.Cloud);
				ctx.Response.Write("</cloud>");
			}
			*/

			ctx.Response.Write( "</item>" );
		}
	}
}