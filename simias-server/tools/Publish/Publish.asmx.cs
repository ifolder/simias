/***********************************************************************
 *  $RCSfile$
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Security.Cryptography;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using Simias;
using Simias.Storage;

namespace Simias.WebService
{
	/// <summary>
	/// Registration
	/// Web service methods to manage the Identity Sync Service
	/// </summary>
	[WebService(
	 Namespace="http://novell.com/simias/publish",
	 Name="Collection Publisher",
	 Description="Web Service to publish a collection")]
	public class Publish : System.Web.Services.WebService
	{
		private Store store = null;
		
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log =
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
		/// <summary>
		/// Constructor
		/// </summary>
		public Publish()
		{
			store = Store.GetStore();
		}
		
		/// <summary>
		/// Method to Publish a collection
		/// <param>DomainID Domain the collection belongs to</param>
		/// <param>Collection - Collection Name or ID to publish</param>
		/// <param> Publish - true publish, false unpublish</param>
		/// </summary>
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public
		bool
		PublishCollection( string DomainID, string Collection, bool Publish ) 
		{
			Domain domain = null;
			Collection collection = null;
			
			/*
			if ( DomainID == null || DomainID == "" )
			{
				domain = store.GetDomain( store.DefaultDomain );
			}
			else
			{
				domain = store.GetDomain( DomainID );
			}
			*/
			
			domain = store.GetDomain( store.DefaultDomain );
			if ( domain == null )
			{
				return false;
			}
			
			collection = store.GetCollectionByID( Collection );
			if ( collection == null )
			{
				ICSList collections = store.GetCollectionsByName( Collection );
				foreach( ShallowNode sn in collections )
				{
					collection = new Collection( store, sn );
					break;
				}
			}
			
			if ( collection == null )
			{
				return false;
			}

			if ( Publish == true )
			{
				Property pubProp = new Property( "Published", true );
				collection.Properties.ModifyProperty( pubProp );
			}
			else
			{
				collection.Properties.DeleteSingleProperty( "Published" );
			}
				
			collection.Commit( collection );
			return true;
		}
		
		/// <summary>
		/// Method to get the collection's description
		/// <param>DomainID Domain the collection belongs to</param>
		/// <param>Collection - Collection Name or ID to publish</param>
		/// </summary>
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public
		string
		GetDescription( string DomainID, string Collection ) 
		{
			Domain domain = null;
			Collection collection = null;
			
			/*
			if ( DomainID == null || DomainID == "" )
			{
				domain = store.GetDomain( store.DefaultDomain );
			}
			else
			{
				domain = store.GetDomain( DomainID );
			}
			*/
			
			domain = store.GetDomain( store.DefaultDomain );
			if ( domain == null )
			{
				return "";
			}
			
			collection = store.GetCollectionByID( Collection );
			if ( collection == null )
			{
				ICSList collections = store.GetCollectionsByName( Collection );
				foreach( ShallowNode sn in collections )
				{
					collection = new Collection( store, sn );
					break;
				}
			}
			
			if ( collection == null )
			{
				return "";
			}

			Property descProp = collection.Properties.GetSingleProperty( "Description" );
			if ( descProp != null && descProp.Value != null )
			{
				return descProp.Value as string;
			}
			
			return "";
		}
		
		
		/// <summary>
		/// Method to set a description on a collection
		/// <param>DomainID Domain the collection belongs to</param>
		/// <param>Collection - Collection Name or ID to publish</param>
		/// <param> Description - text describing the collection</param>
		/// </summary>
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public
		bool
		SetDescription( string DomainID, string Collection, string Description ) 
		{
			Domain domain = null;
			Collection collection = null;
			
			/*
			if ( DomainID == null || DomainID == "" )
			{
				domain = store.GetDomain( store.DefaultDomain );
			}
			else
			{
				domain = store.GetDomain( DomainID );
			}
			*/
			
			domain = store.GetDomain( store.DefaultDomain );
			if ( domain == null )
			{
				return false;
			}
			
			collection = store.GetCollectionByID( Collection );
			if ( collection == null )
			{
				ICSList collections = store.GetCollectionsByName( Collection );
				foreach( ShallowNode sn in collections )
				{
					collection = new Collection( store, sn );
					break;
				}
			}
			
			if ( collection == null )
			{
				return false;
			}

			Property descProp = new Property( "Description", Description );
			collection.Properties.ModifyProperty( descProp );
			collection.Commit( collection );

			return true;
		}
	}
}

