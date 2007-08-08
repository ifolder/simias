/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |   Author: Brady Anderson <banderso@novell.com>
 |***************************************************************************/
 
using System;
using System.Collections;

using Simias;
using Simias.Client;
using Simias.Storage;

namespace Simias.Tags
{
	/// <summary>
	/// Class used for querying nodes that contain that
	/// specified tag(s)
	/// </summary>
	public class Query
	{
		private ArrayList tagList;
		private Collection collection;

		#region Constructors
		/// <summary>
		/// Constructor will throw an exception if the collection does not exist
		/// </summary>
		public Query( string collectionID )
		{
			collection = Store.GetStore().GetCollectionByID( collectionID );
			if ( collection == null )
			{
				throw new SimiasException( "Specified Collection does not exist" );
			}

			tagList = new ArrayList();
		}

		/// <summary>
		/// Constructor will throw an exception if the collection does not exist
		/// </summary>
		public Query( string collectionID, Tag tag )
		{
			collection = Store.GetStore().GetCollectionByID( collectionID );
			if ( collection == null )
			{
				throw new SimiasException( "Specified Collection does not exist" );
			}

			tagList = new ArrayList();
			tagList.Add( tag );
		}
		#endregion

		public bool AddTag( Tag searchTag )
		{
			tagList.Add( searchTag );
			return true;
		}

		/// <summary>
		/// Query for all nodes that contain the tag(s) setup in this
		/// query object.
		/// </summary>
		/// <returns>An ICSList object containing the ShallowNode objects for the search results</returns>
		public ICSList QueryNodes()
		{
			ICSList returnedResults = null;
			Property p;

			if ( tagList.Count == 1 )
			{
				Tag tag = tagList[0] as Tag;
				Relationship relationshipTag = new Relationship( collection.ID, tag.ID );
				p = new Property( "Tag", relationshipTag );
				returnedResults = collection.Search( p, SearchOp.Equal );
			}
			else
			if ( tagList.Count > 1 )
			{
				ICSList queryResults;
				Hashtable hashedList = new Hashtable();
				foreach( Tag tag in tagList )
				{
					Relationship relationshipTag = new Relationship( collection.ID, tag.ID );
					p = new Property( "Tag", relationshipTag );
					queryResults = collection.Search( p, SearchOp.Equal );

					foreach( ShallowNode sn in queryResults )
					{
						if ( hashedList.ContainsKey( sn.ID ) == false )
						{
							hashedList.Add( sn.ID, sn );
						}
					}
				}

				if ( hashedList.Count > 0 )
				{
					returnedResults = new ICSList();
					foreach( ShallowNode sn in hashedList.Values )
					{
						returnedResults.Add( sn );
					}
				}
			}
			
			return returnedResults;
		}

		/// <summary>
		/// Query for all nodes that contain the tag(s) setup in this
		/// query object.
		/// </summary>
		/// <returns>An ICSList object containing the ShallowNode objects for the search results
		static public ICSList Nodes( string collectionID, Tag searchTag )
		{
			ICSList queryResult = null;
			Collection collection = Store.GetStore().GetCollectionByID( collectionID );
			if ( collection != null )
			{
				Relationship relationshipTag = 
					new Relationship( collection.ID, searchTag.ID );
				Property p = new Property( "Tag", relationshipTag );
				queryResult = collection.Search( p, SearchOp.Equal );
			}
			
			return queryResult;
		}

		/// <summary>
		/// Query for all tags that exist in the collection.
		/// </summary>
		/// <returns>An ICSList object containing the ShallowNode objects for the search results
		static public ICSList Tags( Collection collection )
		{
			return collection.Search( PropertyTags.Types, NodeTypes.TagType, SearchOp.Equal );
		}

		/// <summary>
		/// Query for all tags that exist in the collection.
		/// </summary>
		/// <returns>An ICSList object containing the ShallowNode objects for the search results
		static public ICSList Tags( string collectionID )
		{
			ICSList queryResult = null;
			Collection collection = Store.GetStore().GetCollectionByID( collectionID );
			if ( collection != null )
			{
				queryResult =
					collection.Search( PropertyTags.Types, NodeTypes.TagType, SearchOp.Equal );
			}
			else
			{
				throw new NotExistException( collectionID );
			}

			return queryResult;
		}
	}
}
