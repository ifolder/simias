/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Brady Anderson <banderso@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

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
		private bool publicAccess = false;
		private HttpContext ctx;
		private Collection collection;
		private Node node;
		private Store store;
		private DateTime published;

		//private char[] dotSep = {'.'};
		
        /// <summary>
        /// get/set enclosure
        /// </summary>
		public bool Enclosures
		{
			get{ return enclosures; }
			set{ enclosures = value; }
		}
		
        /// <summary>
        /// get/set strict variable
        /// </summary>
		public bool Strict
		{
			get{ return strict; }
			set{ strict = value; }
		}

        /// <summary>
        /// get published
        /// </summary>
		public DateTime Published
		{
			get{ return published; }
		}

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="Context">Http context</param>
        /// <param name="ParentCollection">parent collection of this collection</param>
        /// <param name="SN">shallow node for collection</param>
		public Item( HttpContext Context, Collection ParentCollection, ShallowNode SN )
		{
			ctx = Context;
			store = Store.GetStore();
			collection = ParentCollection;
			node = new Node( collection, SN );
		}
		
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ParentCollection">parent colection for this collection</param>
        /// <param name="SN">shallownode for collection</param>
		public Item( Collection ParentCollection, ShallowNode SN )
		{
			store = Store.GetStore();
			collection = ParentCollection;
			node = new Node( collection, SN );
			published = (DateTime)
				node.Properties.GetSingleProperty( Simias.RssFeed.Util.LastModified ).Value;
		}

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ParentCollection">parent collection of this collection</param>
        /// <param name="SN">shallownode</param>
        /// <param name="Public">whether it is for public access</param>
		public Item( Collection ParentCollection, ShallowNode SN, bool Public )
		{
			store = Store.GetStore();
			collection = ParentCollection;
			publicAccess = Public;
			node = new Node( collection, SN );
			published = (DateTime)
				node.Properties.GetSingleProperty( Simias.RssFeed.Util.LastModified ).Value;
		}

        /// <summary>
        /// process and send the reply for request
        /// </summary>
        /// <param name="Context">httpcontext object</param>
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

				/*
				ctx.Response.Write(
					String.Format( 
						"<link>{0}{1}:{2}{3}/sfile.ashx?fid={4}{5}</link>",
						ctx.Request.IsSecureConnection ? "https://" : "http://",
						ctx.Request.Url.Host,
						ctx.Request.Url.Port.ToString(),
						ctx.Request.ApplicationPath,
						fileNode.ID,
						HttpUtility.UrlEncode( "&name=" + fileNode.Name ) ) );
				*/
				
				ctx.Response.Write(
					String.Format( 
					"<link>{0}{1}:{2}{3}{4}?fid={5}</link>",
					ctx.Request.IsSecureConnection ? "https://" : "http://",
					ctx.Request.Url.Host,
					ctx.Request.Url.Port.ToString(),
					ctx.Request.ApplicationPath,
					( publicAccess == true ) ? "/pubsfile.ashx" : "/sfile.ashx",
					fileNode.ID ) );

				if ( enclosures == true )
				{
					ctx.Response.Write(
						String.Format( 
							"<enclosure url=\"{0}{1}:{2}{3}{4}?fid={5}\" length=\"{6}\" type=\"{7}\"/>",
							ctx.Request.IsSecureConnection ? "https://" : "http://",
							ctx.Request.Url.Host,
							ctx.Request.Url.Port.ToString(),
							ctx.Request.ApplicationPath,
							( publicAccess == true ) ? "/pubsfile.ashx" : "/sfile.ashx",
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
