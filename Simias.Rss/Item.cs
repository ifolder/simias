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
		private char[] dotSep = {'.'};
		
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
			FileNode fileNode = null;

			ctx.Response.Write( "<description>" );
			if ( node.IsType( "Member" ) == true )
			{
				Member collectionMember = new Member( node );
				ctx.Response.Write( " - " + collectionMember.FN );
			}
			else
			if ( node.IsType( "FileNode" ) == true )
			{
				fileNode = new FileNode( node );
				ctx.Response.Write( fileNode.GetRelativePath() );
			}
			else
			{
				ctx.Response.Write( node.Name );
			}
			ctx.Response.Write("</description>");
			
			Domain domain = store.GetDomain( store.DefaultDomain );
			if ( fileNode != null )
			{
				ctx.Response.Write(
					String.Format( 
						"<link>{0}{1}:{2}{3}/sfile.ashx?fid={4}</link>",
						ctx.Request.IsSecureConnection ? "https://" : "http://",
						ctx.Request.Url.Host,
						ctx.Request.Url.Port.ToString(),
						ctx.Request.ApplicationPath,
						node.ID ) ); 

				ctx.Response.Write(
					String.Format( 
						"<enclosure>url=\"{0}{1}:{2}{3}/sfile.ashx?fid={4}\" length=\"{5}\" type=\"{6}\"</enclosure>",
						ctx.Request.IsSecureConnection ? "https://" : "http://",
						ctx.Request.Url.Host,
						ctx.Request.Url.Port.ToString(),
						ctx.Request.ApplicationPath,
						node.ID,
						fileNode.Length,
						Simias.HttpFile.Response.GetMimeType( fileNode.GetFileName() ) ) );
			}

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

			// Category  - use tags and types
			
			ctx.Response.Write( "<guid isPermaLink=\"false\">" + node.ID + "</guid>" );
			
			Property lastWrite = node.Properties.GetSingleProperty( Simias.RssFeed.Util.LastModified );
			if ( lastWrite != null )
			{
				Simias.RssFeed.Util.SendPublishDate( ctx, ( DateTime ) lastWrite.Value );
			}
			else
			{
				Simias.RssFeed.Util.SendPublishDate( ctx, node.CreationTime );
			}

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