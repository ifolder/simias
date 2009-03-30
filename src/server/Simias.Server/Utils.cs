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
using System.Reflection;
using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Server
{
	/// <summary>
	/// Container for common server utility methods. 
	/// </summary>
	public class Util
	{
		#region Class Types
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log =
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion	
	
		/// <summary>
		/// Default constructor
		/// </summary>
		public Util()
		{
		}
		
		
		/// <summary>
		/// Utility method to delete a member's POBox from the system
		/// </summary>
		public static bool DeletePOBox( string DomainID, string UserID )
		{
			bool deleted = false;
			
			try
			{
				Store store = Store.GetStore();
				ICSList cList = store.GetCollectionsByOwner( UserID );
				foreach( ShallowNode sn in cList )
				{
					Collection c = new Collection( store, sn );
					if ( ( c.Domain == DomainID ) &&
						( (Node) c ).IsBaseType( NodeTypes.POBoxType ) )
					{
						c.Commit( c.Delete() );
						deleted = true;
						break;
					}
				}
			}
			catch( Exception e2 )
			{
				log.Error( e2.Message );
				log.Error( e2.StackTrace );
			}
			
			return deleted;
		}
		
		/// <summary>
		/// Method to remove collections owned by the specified user
		/// </summary>
		public
		static
		void DeleteOwnedCollections( string DomainID, string UserID, ArrayList reportLog )
		{
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain( DomainID );
				Member member = domain.GetMemberByID( UserID );
			
				// Get all of the collections this user is a member of.
				ICSList cList = store.GetCollectionsByOwner( UserID );
				foreach ( ShallowNode sn in cList )
				{
					Collection c = null;
					
					try
					{
						// Remove the user as a member of this collection.
						c = new Collection( store, sn );
	
						// Only look for collections from the specified domain and
						// don't allow this user's membership removed from the domain itself.
						if ( ( c.Domain == DomainID ) &&
							!( (Node) c).IsBaseType( NodeTypes.DomainType ) )
						{
							// The specified user is the owner delete the collection
							c.Commit( c.Delete( c ) );
							
							Property dn = member.Properties.GetSingleProperty( "DN" );
							string userName = ( dn.Value != null ) ? dn.Value as string : member.Name;
						
							string logMessage =
								String.Format(
									"Deleted collection: {0} owned by: {1} in domain: {2}",
									c.Name,
									userName,
									domain.Name );
							log.Info( logMessage );
								
							// If the caller gave us a report log add the message
							if ( reportLog != null )
							{
								reportLog.Add( logMessage );
							}
						}
					}
					catch( Exception inner )
					{
						if ( c != null )
						{
							string message =
								String.Format(
									"Exception occurred deleting collection: {0} in domain: {1} - exception message: {2}",
									c.Name,
									domain.Name,
									inner.Message );
									
							log.Error( message );
							if ( reportLog != null )
							{
								reportLog.Add( message );
							}
						}
						else
						{
							log.Error( inner.Message );
							if ( reportLog != null )
							{
								reportLog.Add( inner.Message );
							}
						}
						
						log.Error( inner.StackTrace );
					}
				}
			}
			catch( Exception outer )
			{
				log.Error( outer.Message );
				log.Error( outer.StackTrace );
			}
		}
		
		/// <summary>
		/// Method to remove a specified user from all collections
		/// he is a member of.
		/// This method does NOT remove the member if the member is the owner
		/// of the collection nor does it remove the member from
		/// the domain collection.
		/// </summary>
		public
		static
		void RemoveMemberships( string DomainID, string UserID, ArrayList reportLog )
		{
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain( DomainID );
				Member member = domain.GetMemberByID( UserID );
			
				// Get all of the collections this user is a member of.
				ICSList cList = store.GetCollectionsByUser( UserID );
				foreach ( ShallowNode sn in cList )
				{
					Collection c = null;
					
					try
					{
						// Remove the user as a member of this collection.
						c = new Collection( store, sn );
	
						// Only look for collections from the specified domain and
						// don't allow this user's membership removed from the domain itself.
						if ( ( c.Domain == DomainID ) &&
							!( (Node) c).IsBaseType( NodeTypes.DomainType ) )
						{
							Member cMember = c.GetMemberByID( UserID );
							if ( cMember != null && cMember.IsOwner == false )
							{
								// Not the owner, just remove the membership.
								c.Commit( c.Delete( cMember ) );
								Property dn = member.Properties.GetSingleProperty( "DN" );
								string userName = ( dn.Value != null ) ? dn.Value as string : member.Name;
						
								string logMessage =
									String.Format(
										"Removed {0}'s membership from collection: {1} in domain: {2}",
										userName,
										c.Name,
										domain.Name );
								log.Info( logMessage );
								
								// If the caller gave us a report log add the message
								if ( reportLog != null )
								{
									reportLog.Add( logMessage );
								}
							}
						}
					}
					catch( Exception inner )
					{
						if ( c != null )
						{
							string message =
								String.Format(
									"Exception occurred removing {0}'s membership from collection: {1} in domain: {2} - exception message: {3}",
									member.Name,
									c.Name,
									domain.Name,
									inner.Message );
									
							log.Error( message );
							if ( reportLog != null )
							{
								reportLog.Add( message );
							}
						}
						else
						{
							log.Error( inner.Message );
							if ( reportLog != null )
							{
								reportLog.Add( inner.Message );
							}
						}
						
						log.Error( inner.StackTrace );
					}
				}
			}
			catch( Exception outer )
			{
				log.Error( outer.Message );
				log.Error( outer.StackTrace );
			}
		}
	}
}	
