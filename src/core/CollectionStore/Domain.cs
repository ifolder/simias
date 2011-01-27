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
*                 $Author: Mike Lasky <mlasky@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
 

using System;
using System.Xml;

using Simias.Client;
using Simias.Sync;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents a Domain object in the Collection Store.
	/// </summary>
	public class Domain : Collection
	{
		#region Class Members
                static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Domain ) );
		/// <summary>
		/// Defines the different types of synchronization configurations.
		/// </summary>
		public enum ConfigurationType
		{
			/// <summary>
			/// Doesn't sychronize.
			/// </summary>
			None,

			/// <summary>
			/// Workgroup (e.g. Rendevous, Gaim, etc)
			/// </summary>
			Workgroup,

			/// <summary>
			/// Client/Server (e.g. Enterprise, SimpleServer, etc)
			/// </summary>
			ClientServer
		}

                /// <summary>
                /// Group Quota Restriction Method.
                /// </summary>
                private enum QuotaRestriction
                {
                        // For current Implementation, enum value AllAdmins is not used, can be used in future
                        UI_Based,
                        Sync_Based
                }


		/// <summary>
		/// Configuration section name where enterprise key value pairs are stored.
		/// </summary>
		static public string SectionName = "Domain";
		/// <summary>
		/// Admin DN Tag
		/// </summary>
		static public string AdminDNTag = "AdminDN";
		/// <summary>
		/// Encoding Tag
		/// </summary>
		static public string Encoding = "Encoding";

		/// <summary>
		/// Current version of the domain.
		/// </summary>
		public static readonly Version CurrentDomainVersion = new Version( "1.0.0.0" );

		#endregion

		#region Properties
		/// <summary>
		/// Gets the domain configuration type.
		/// </summary>
		public ConfigurationType ConfigType
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.DomainType );
				return ( p != null ) ? ( ConfigurationType )p.Value : ConfigurationType.None;
			}
		}

		/// <summary>
		/// Gets or sets the domain description.
		/// </summary>
		public string Description
		{
			get 
			{ 
				Property p = properties.GetSingleProperty( PropertyTags.Description );
				return ( p != null ) ? p.Value as String : null;
			}

			set { properties.ModifyNodeProperty( PropertyTags.Description, value ); }
		}

		/// <summary>
		/// Gets or sets Users full name display setting.
		/// </summary>
		public string UsersFullNameDisplay
		{
			get 
			{ 
				Property p = properties.GetSingleProperty( PropertyTags.UsersFullNameDisplay );
				return ( p != null ) ? p.Value as String : "FirstNameLastName";
			}

			set { properties.ModifyNodeProperty( PropertyTags.UsersFullNameDisplay, value ); }
		}

		/// <summary>
		/// Gets or sets the group splitting property of system.
		/// </summary>
		public string GroupSegregated
		{
			get 
			{ 
				Property p = properties.GetSingleProperty( PropertyTags.GroupSegregated );
				return ( p != null ) ? p.Value as String : "no";
			}

			set 
			{ 
				Property p = new Property(PropertyTags.GroupSegregated, value);
				p.ServerOnlyProperty = true;	
				properties.ModifyNodeProperty( p ); 
			}
		}

		/// <summary>
		/// Gets or sets the group quota restriction method/time. To check whether the group is exceeding the limit set at which place
		/// The check can be made when secodnary administrator is allocating disk quota to each member, or when members are doing a sync/upload
		/// or at both times. For first option, It is mandatory that secondary administrator must allocate disk quota to each member of his group
		/// </summary>
		public int GroupQuotaRestrictionMethod
		{
			get 
			{ 
				Property p = properties.GetSingleProperty( PropertyTags.GroupQuotaRestrictionMethod );
				return ( p != null ) ? (int) p.Value : (int)QuotaRestriction.UI_Based;
			}

			set 
			{ 
				Property p = new Property(PropertyTags.GroupQuotaRestrictionMethod, value);
				p.ServerOnlyProperty = true;
				properties.ModifyNodeProperty( p ); 
			}
		}


		/// <summary>
		/// Catalog , Domain Sync status
		/// </summary>
		public ulong SystemSyncStatus
		{
			get { 
				Property p = properties.FindSingleValue( PropertyTags.SystemSyncStatus );
				return ( p != null ) ? (ulong)p.Value : (ulong)0;
			}

			set { 
				properties.ModifyNodeProperty( PropertyTags.SystemSyncStatus, (ulong)value ); 
			}
		}

        /// <summary>
        /// Gets or sets the version of this domain.
        /// </summary>
        public Version ServerVersion
        {
            get
            {
                Property p = properties.GetSingleProperty(PropertyTags.ServerVersion);
                return (p != null) ? new Version(p.Value as String) : new Version("0.0.0.0");
            }

            set {
                Property p = new Property(PropertyTags.ServerVersion, value.ToString());
                p.LocalProperty = true;
                properties.ModifyNodeProperty(p); 
            }
        }

#if ( !REMOVE_OLD_INVITATION )
		/// <summary>
		/// Used to determine if the domain supports the new invitation model.
		/// </summary>
		public bool SupportsNewInvitation
		{
			get { return ( DomainVersion >= CurrentDomainVersion ) ? true : false; }
		}
#endif
		/// <summary>
		/// Gets or sets the version of this domain.
		/// </summary>
		public Version DomainVersion
		{
			get
			{
				Property p = properties.GetSingleProperty( PropertyTags.DomainVersion );
				return ( p != null ) ? new Version( p.Value as String ) : new Version( "0.0.0.0" );
			}

			set { properties.ModifyNodeProperty( PropertyTags.DomainVersion, value.ToString() ); }
		}
		#endregion

		#region Constructors
		/// <param name="store">Store object.</param>
		/// <param name="domainName">Name of the domain.</param>
		/// <param name="domainID">Well known unique identifier for the domain.</param>
		/// <param name="description">String that describes this domain.</param>
		/// <param name="role">The type of synchronization role this domain has.</param>
		/// <param name="configType">The synchronization configuration type for this domain.</param>
		public Domain( Store store, string domainName, string domainID, string description, SyncRoles role, ConfigurationType configType ) :
			base ( store, domainName, domainID, NodeTypes.DomainType, domainID )
		{
			// Add the description attribute.
			if ( ( description != null ) && ( description.Length > 0 ) )
			{
				properties.AddNodeProperty( PropertyTags.Description, description );
			}

			// Set the current domain version.
			DomainVersion = CurrentDomainVersion;

			// Add the sync role for this collection.
			Role = role;
			
			// Add the configuration type.
			Property p = new Property( PropertyTags.DomainType, configType );
			p.LocalProperty = true;
			properties.AddNodeProperty( p );

			// Add catalog and domian sync property.
			Property pSystemSyncStatus = new Property( PropertyTags.SystemSyncStatus, (ulong)0 );
			pSystemSyncStatus.LocalProperty = true;
			properties.AddNodeProperty( pSystemSyncStatus );
			
			// When sync the policy node (always sync is initiated from slave) is syned from master to slave
			// the master is overwritten to slave, the slave changes will be lost
			// when there is no change in master, the slave changes will be synced to master
			// This is to ensure that the master policy over rides the slave policy.			
			// Ideally we should have a master policy which is read only in slave and
			// slave specific policy which should not be synced to master.			
			CollisionPolicy = CollisionPolicy.ServerWins;
		}

		/// <summary>
		/// Constructor to create an existing Domain object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="node">Node object to construct this object from.</param>
		public Domain( Store storeObject, Node node ) :
			base( storeObject, node )
		{
		}

		/// <summary>
		/// Constructor for creating an existing Domain object from a ShallowNode.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		public Domain( Store storeObject, ShallowNode shallowNode ) :
			base( storeObject, shallowNode )
		{
		}

		/// <summary>
		/// Constructor to create an existing Domain object from an Xml document object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document object to construct this object from.</param>
		internal Domain( Store storeObject, XmlDocument document ) :
			base( storeObject, document )
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns if the specified user's login is disabled.
		/// </summary>
		/// <param name="member">Member for which login information to be retrived.</param>
		/// <returns>True if the login for the specified user is disabled.</returns>
		public bool IsLoginDisabledForUser( Member member )
		{
			Property p = member.Properties.GetSingleProperty( PropertyTags.LoginDisabled );
			return ( p != null ) ? ( bool )p.Value : false;
		}

		/// <summary>
		/// Returns if the specified user's login is disabled.
		/// </summary>
		/// <param name="userID">User ID for the member to check.</param>
		/// <returns>True if the login for the specified user is disabled.</returns>
		public bool IsLoginDisabled( string userID )
		{
			Member member = GetMemberByID( userID );
			if ( member == null )
			{
				throw new DoesNotExistException( "The specified user does not exist." );
			}
			if(member.Rights == Access.Rights.Admin)
                                return false;

			Property p = member.Properties.GetSingleProperty( PropertyTags.LoginDisabled );
			if(  p == null  &&  Simias.Service.Manager.LdapServiceEnabled == true )
			{
				string[] GIDs = GetMemberFamilyList(userID);
                        	foreach(string gid in GIDs)
                        	{
                                	if(gid != userID)
                                	{
						Member groupMember = GetMemberByID( gid );
						if ( member != null )
						{
							p = groupMember.Properties.GetSingleProperty( PropertyTags.LoginDisabled );
							if(p != null && ( bool ) p.Value == true )
							{
								log.Debug("User is login is disabled at group level: " + userID + "  " + gid);
								return true;
							}
						}
                                	}
                        	}
			}
			return ( p != null ) ? ( bool )p.Value : false;
		}

		/// <summary>
		/// Returns if the specified user's login is disabled.
		/// </summary>
		/// <param name="userID">User ID for the member to check.</param>
		/// <returns>True if the login for the specified user is disabled.</returns>
		public bool GetLoginpolicy( string userID )
		{
			Member member = GetMemberByID( userID );
			if ( member == null )
			{
				throw new DoesNotExistException( "The specified user does not exist." );
			}

			if(member.Rights == Access.Rights.Admin)
                                return false;

			Property p = member.Properties.GetSingleProperty( PropertyTags.LoginDisabled );
			if(  p == null  &&  Simias.Service.Manager.LdapServiceEnabled == true )
			{
				string[] GIDs = GetMemberFamilyList(userID);
                        	foreach(string gid in GIDs)
                        	{
                                	if(gid != userID)
                                	{
						Member groupMember = GetMemberByID( gid );
						if ( member != null )
						{
							p = groupMember.Properties.GetSingleProperty( PropertyTags.LoginDisabled );
							if(p != null && ( bool ) p.Value == true )
							{
								log.Debug("User is login is disabled at group level: " + userID + "  " + gid);
								return true;
							}
						}
                                	}
                        	}
			}
			return ( p != null ) ? ( bool )p.Value : false;
		}

		/// <summary>
		/// Sets the specified user's login disabled status.
		/// </summary>
		/// <param name="userID">User ID for the member to set the status for.</param>
		/// <param name="disable">True to disable login or False to enable login.</param>
		public void SetLoginDisabled( string userID, bool disable )
		{
			Member member = GetMemberByID( userID );
			if ( member == null )
			{
				throw new DoesNotExistException( "The specified user does not exist." );
			}

			if ( disable )
			{
				Property p = new Property( PropertyTags.LoginDisabled, true );
				//making it non-local so that it syncs across server
				//p.LocalProperty = true;
				member.Properties.ModifyNodeProperty( p );
				Commit( member );
			}
			else
			{
				Property p = member.Properties.GetSingleProperty( PropertyTags.LoginDisabled );
				if ( p != null )
				{
					p.DeleteProperty();
					Commit( member );
				}
				else
				{
					p = new Property( PropertyTags.LoginDisabled, false );
					//p.LocalProperty = true;
					member.Properties.ModifyNodeProperty( p );
					Commit( member );
				}
			}
		}

		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return Name;
		}
		#endregion
	}
}
