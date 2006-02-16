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
using System.Collections;
using System.Web;

using Simias;
using Simias.Storage;

namespace Simias.RssFeed
{
	public class ItemSort : IComparer
	{
		// Sort items in chronological order (last to first)
		public int Compare( object One, object Two )
		{
			Item one = One as Item;
			Item two = Two as Item;

			if ( one.Published == two.Published )
			{
				return 0;
			}
			else
			if ( one.Published < two.Published )
			{
				return 1;
			}

			return -1;
		}
	}

	/// <summary>
	/// Summary description for Item.
	/// </summary>
	public class Item
	{
		//private bool detailed = false;
		private bool enclosures = false;
		private bool strict = true;
		private HttpContext ctx;
		private Collection collection;
		private Node node;
		private Store store;
		private DateTime published;

		//private char[] dotSep = {'.'};
		
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

		public DateTime Published
		{
			get{ return published; }
		}

		public Item( HttpContext Context, Collection ParentCollection, ShallowNode SN )
		{
			ctx = Context;
			store = Store.GetStore();
			collection = ParentCollection;
			node = new Node( collection, SN );
		}

		public Item( Collection ParentCollection, ShallowNode SN )
		{
			store = Store.GetStore();
			collection = ParentCollection;
			node = new Node( collection, SN );
			published = (DateTime)
				node.Properties.GetSingleProperty( Simias.RssFeed.Util.LastModified ).Value;
		}

		public void Send( HttpContext Context )
		{
			ctx = Context;
			ctx.Response.Write( "<item>" );
			FileNode fileNode = null;

			ctx.Response.Write( "<title>" );
			if ( node.IsType( "Member" ) == true )
			{
				Member collectionMember = new Member( node );
				ctx.Response.Write( " - " + collectionMember.FN );
			}
			else
			if ( node.IsType( "FileNode" ) == true )
			{
				fileNode = new FileNode( node );
				ctx.Response.Write( fileNode.GetFileName() );
			}
			else
			{
				ctx.Response.Write( node.Name );
			}
			ctx.Response.Write("</title>");

			ctx.Response.Write( "<guid isPermaLink=\"false\">" + node.ID + "</guid>" );
			Simias.RssFeed.Util.SendPublishDate( ctx, published );
			
			if ( node.IsType( "Member" ) == true )
			{
				Member collectionMember = new Member( node );
				ctx.Response.Write( "<description>" );
				ctx.Response.Write( " - " + collectionMember.FN );
				ctx.Response.Write("</description>");

				ctx.Response.Write( "<type>Member</type>" );
			}
			else
			if ( node.IsType( "FileNode" ) == true )
			{
				fileNode = new FileNode( node );
				ctx.Response.Write( "<description>" );
				ctx.Response.Write( fileNode.GetRelativePath() );
				ctx.Response.Write("</description>");

				ctx.Response.Write(
					String.Format( 
						"<link>{0}{1}:{2}{3}/sfile.ashx?fid={4}</link>",
						ctx.Request.IsSecureConnection ? "https://" : "http://",
						ctx.Request.Url.Host,
						ctx.Request.Url.Port.ToString(),
						ctx.Request.ApplicationPath,
						fileNode.ID ) );

				if ( enclosures == true )
				{
					ctx.Response.Write(
						String.Format( 
							"<enclosure url=\"{0}{1}:{2}{3}/sfile.ashx?fid={4}\" length=\"{5}\" type=\"{6}\"/>",
							ctx.Request.IsSecureConnection ? "https://" : "http://",
							ctx.Request.Url.Host,
							ctx.Request.Url.Port.ToString(),
							ctx.Request.ApplicationPath,
							node.ID,
							fileNode.Length,
							Simias.HttpFile.Response.GetMimeType( fileNode.GetFileName() ) ) );
				}			
			}
			else
			if ( node.IsType( "DirNode" ) == true )
			{
				DirNode dirNode = new DirNode( node );
				ctx.Response.Write( "<description>" );
				ctx.Response.Write( dirNode.GetRelativePath() );
				ctx.Response.Write("</description>");
				
			}
			
			Domain domain = store.GetDomain( store.DefaultDomain );
			Member member = domain.GetMemberByID( node.Creator );
			if ( member != null )
			{
				ctx.Response.Write( "<author>" );
				if ( member.FN != null && member.FN != "" )
				{
					ctx.Response.Write( member.FN );
				}
				else
				{
					ctx.Response.Write( member.Name );
				}
				ctx.Response.Write( "</author>" );
			}
			
			if ( strict == false )
			{
				ctx.Response.Write( "<authorID>" + member.UserID + "</authorID>" );
				ctx.Response.Write( "<type>" + node.Type.ToString() + "</type>" );
				ctx.Response.Write( "<id>" + node.ID + "</id>" );
			}

			// Category  - use tags and types
			

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