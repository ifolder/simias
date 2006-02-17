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
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace Novell.Collaboration.Feeds
{
	/// <summary>
	/// Summary description for Item.
	/// </summary>
	public class Item
	{
		#region Private Types
		private string title;
		private string description;
		private string link;
		private DateTime pubDate;
		private string author;
		private string guid;
		private string comments;
		private ArrayList categories;
		private Novell.Collaboration.Feeds.Enclosure enclosure = null;
		private string image;
		#endregion

		#region Properties
		public string Title
		{
			get{ return title; }
			set{ title = value; }
		}

		public string Description
		{
			get{ return description; }
			set{ description = value; }
		}

		public DateTime Published
		{
			get{ return pubDate; }
			set{ pubDate = value; }
		}

		public string Link
		{
			get{ return link; }
			set{ link = value; }
		}

		public string Author
		{
			get{ return author; }
			set{ author = value; }
		}

		public string Guid
		{
			get{ return guid; }
			set{ guid = value; }
		}

		public string Comments
		{
			get{ return comments; }
			set{ comments = value; }
		}

		public Novell.Collaboration.Feeds.Enclosure Enclosure
		{
			get{ return enclosure; }
			set{ enclosure = value; }
		}

		#endregion

		#region Constructors
		public Item()
		{
		}

		public Item( string Title, string Description )
		{
			title = Title;
			description = Description;
		}

		public Item( XmlNode node )
		{
			categories = new ArrayList();
			LoadFromXmlNode( node );
		}
		#endregion

		#region Private Methods
		private void LoadFromXmlNode( XmlNode Node)
		{
			if ( Node.Name.ToLower() != "item" )
			{
				throw new ApplicationException( "Node is not of type item" );
			}

			XmlNode enclosureNode = null;
			foreach( XmlNode node in Node )
			{
				switch( node.Name.ToLower() )
				{
					case "enclosure":
					{
						enclosureNode = node;
						break;
					}

					case "title":
					{
						this.title = node.InnerText;
						break;
					}

					case "description":
					{
						this.description = node.InnerText;
						break;
					}

					case "link":
					{
						this.link = node.InnerText;
						break;
					}

					case "author":
					{
						this.author = node.InnerText;
						break;
					}

					case "pubdate":
					{
						this.pubDate = System.Convert.ToDateTime( node.InnerText );
						break;
					}

					case "guid":
					{
						this.guid = node.InnerText;
						break;
					}

					case "category":
					{
						categories.Add( node.InnerText );
						break;
					}
				}
			}

			// wait until we're finished with the item before
			// setting up the enclosure object since 
			// the enclosure tag could come before the published tag
			// and vice-versa
			if ( enclosureNode != null )
			{
				this.enclosure = new Enclosure( this.pubDate, enclosureNode );
			}
		}
		#endregion

		#region Public Methods
		public string[] GetCategories()
		{
			if ( categories.Count == 0 )
			{
				return null;
			}

			return categories.ToArray( typeof( string ) ) as string[];
		}
		#endregion
	}
}
