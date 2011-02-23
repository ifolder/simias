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
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/


using System;
using System.Collections;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a property name/value pair for a node.  Properties have
	/// well-defined syntax types.
	/// </summary>
	public class PropertyTags
	{
		#region Class Members
		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Ace = "Ace";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Collision = "Collision";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string ReNamed = "ReName";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Rollback = "Rollback"; 

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string CollisionPolicy = "CollPol";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string CollectionLock = "Lock";

		/// <summary>
		/// Does the master collection need to be created?
		/// </summary>
		static public string CreateMaster = "Create Master Collection";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Creator = "Creator";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string CreationTime = "Create";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DefaultDomain = "DefaultDomain";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Description = "Description";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string UsersFullNameDisplay = "Users Full Name Display";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string GroupSegregated = "Segregated Groups";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string GroupQuotaRestrictionMethod = "Group Quota Restriction Method";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string GroupType = "GroupType";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string UserAdminRights = "UserAdminRights";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string GroupQuotaIsSet = "GroupQuotaIsSet";

		/// <summary>
		/// Catalog , Domain Sync Status
		/// </summary>
		static public string SystemSyncStatus = "System Sync Status";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Domain = "Domain";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DomainID = "DomainID";

		/// <summary>
		/// Well known property name (bitmap, hosts encryption and SSL, new things can also be added here in future)
		/// </summary>
		static public string SecurityStatus = "SecurityStatus";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string EncryptionType = "EncryptionType";

		/// <summary>
                /// Well known property name.
                /// </summary>
		static public string Disabled = "Disabled";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string EncryptionKey= "EncryptionKey";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string EncryptionVersion= "EncryptionVersion";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string EncryptionBlob= "EncryptionBlob";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Merge= "Merge";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DefaultAccount= "DefaultAccount";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string RecoveryKey= "RecoveryKey";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string RAName= "RAName";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string RAPublicKey= "RAPublicKey";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DomainType = "DomainType";

		/// <summary>
                /// Well known property name.
                /// </summary>
                static public string DataPath= "DataPath";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public readonly string Family = "Family";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public readonly string FullName = "FN";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string FileLength = "Length";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LoginDisabled = "LoginDisabled";

		/// <summary>
		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LoginAlreadyDisabled = "LoginAlreadyDisabled";

		/// <summary>
		/// <summary>
		/// Data Movement property
		/// </summary>
		static public string DataMovement = "DataMovement";

		/// <summary>
		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string UserMoveState = "UserMoveState";

		/// <summary>
		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string OldDN = "OldDN";

		/// Well known property name.
		/// </summary>
		static public string FileSystemPath = "FsPath";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public readonly string Given = "Given";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string HostAddress = "HostUri";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string HostID = "HostID";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string NewHostID = "NewHostID";
		
		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Journal = "JournalNode";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string JournalFor = "JournalFor";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string JournalModified = "JournalModified";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LastLoginTime = "LastLogin";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LastModified = "LastModified";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LastModifier = "LastModifier";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LastAccessTime = "Access";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LastWriteTime = "Write";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LinkReference = "LinkRef";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LocalIncarnation = "ClntRev";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string LocalPassword = "LocalPwd";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string MasterIncarnation = "SrvRev";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string MasterUrl = "Master Url";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string NodeCreationTime = "NodeCreate";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string NodeUpdateTime = "NodeUpdate";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Originator = "Orginator";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Owner = "Owner";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Parent = "ParentNode";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string PreviousOwner = "PrevOwner";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string PolicyID = "PolicyID";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string PolicyAssociation = "Association";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Priority = "Priority";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string PublicKey = "PublicKey";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Published = "Published";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Root = "Root";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string StorageSize = "StorageSize";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string StoreVersion = "Version";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string SyncRole = "Sync Role";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string SystemPolicy = "SystemPolicy";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string TombstoneType = "TombstoneType";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string Types = "Types";
		
		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string SyncStatusTag = "SyncStatus";

		/// <summary>
		/// Well known property name.
		/// The file represented by this node has disapeared.
		/// </summary>
		static public string GhostFile = "GhostFile";

		/// <summary>
		/// Well known property name.
		/// The Private address not accessible outside the firewall
		/// used for server to server communication.
		/// </summary>
		static public string PrivateUrl = "PrivateUrl";

		/// <summary>
		/// Well known propery name.
		/// </summary>
		static public string PublicUrl = "PublicUrl";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string PrivateKey = "PrivateKey";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string DomainVersion = "DomainVersion";
	

		/// <summary>
                /// Well known property name.
                /// </summary>
		static public string MigratediFolder = "MigratediFolder";

        /// <summary>
        /// Well known property name.
        /// </summary>
        static public string ServerVersion = "ServerVersion";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string UseSSL = "UseSSL";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string MasterHost = "MasterHost";

		/// <summary>
		/// Well known property name.
		/// </summary>
		static public string AggregateDiskQuota = "AggregateDiskQuota";

        /// <summary>
        /// Well known property name.
        /// </summary>
        static public string DomainTypeNameSpaceProperty = "Simias.Storage.Domain";
		
		
		/// <summary>
		/// Hashtable providing quick lookup to well-known system properties.
		/// </summary>
		private static Hashtable systemPropertyTable;
		#endregion

		#region Constructors
		/// <summary>
		/// Static constructor for the object.
		/// </summary>
		static PropertyTags()
		{
			// Allocate the tables to hold the reserved property names.
			systemPropertyTable = new Hashtable( 30, new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer() );

			// Add the well-known system properties to the hashtable.  Don't need to add values
			// with them.  Just need to know if they exist.
			systemPropertyTable.Add( Ace, null );
			systemPropertyTable.Add( BaseSchema.ObjectId, null );
			systemPropertyTable.Add( BaseSchema.ObjectName, null );
			systemPropertyTable.Add( BaseSchema.ObjectType, null );
			systemPropertyTable.Add( BaseSchema.CollectionId, null );
			systemPropertyTable.Add( Collision, null );
			systemPropertyTable.Add( CollisionPolicy, null );
			systemPropertyTable.Add( CollectionLock, null );
			systemPropertyTable.Add( CreateMaster, null );
			systemPropertyTable.Add( CreationTime, null );
			systemPropertyTable.Add( Creator, null );
			systemPropertyTable.Add( DefaultDomain, null );
			systemPropertyTable.Add( Domain, null );
			systemPropertyTable.Add( DomainID, null );
			systemPropertyTable.Add( EncryptionType, null);
			systemPropertyTable.Add( EncryptionKey, null);
			systemPropertyTable.Add( EncryptionVersion, null);
			systemPropertyTable.Add( EncryptionBlob, null);
			systemPropertyTable.Add( DefaultAccount, null);
			systemPropertyTable.Add( RecoveryKey, null);
			systemPropertyTable.Add( RAName, null);
			systemPropertyTable.Add( RAPublicKey, null);
			systemPropertyTable.Add( DomainType, null );
			systemPropertyTable.Add( FileLength, null );
			systemPropertyTable.Add( FileSystemPath, null );
			systemPropertyTable.Add( FullName, null );
			systemPropertyTable.Add( HostAddress, null );
			systemPropertyTable.Add( HostID, null );
			systemPropertyTable.Add( Journal, null );
			systemPropertyTable.Add( JournalFor, null );
	        	systemPropertyTable.Add( LastLoginTime, null );
			systemPropertyTable.Add( LastAccessTime, null );
			systemPropertyTable.Add( LastModified, null );
			systemPropertyTable.Add( LastModifier, null );
			systemPropertyTable.Add( LastWriteTime, null );
			systemPropertyTable.Add( LinkReference, null );
			systemPropertyTable.Add( LocalIncarnation, null );
			systemPropertyTable.Add( LocalPassword, null );
			systemPropertyTable.Add( LoginDisabled, null );
			systemPropertyTable.Add( MasterIncarnation, null );
			systemPropertyTable.Add( MasterUrl, null );
			systemPropertyTable.Add( NodeCreationTime, null );
			systemPropertyTable.Add( NodeUpdateTime, null );
			systemPropertyTable.Add( Originator, null );
			systemPropertyTable.Add( Owner, null );
			systemPropertyTable.Add( Parent, null );
			systemPropertyTable.Add( PreviousOwner, null );
			systemPropertyTable.Add( PolicyID, null );
			systemPropertyTable.Add( PolicyAssociation, null );
			systemPropertyTable.Add( Priority, null );
			systemPropertyTable.Add( PublicKey, null );
			systemPropertyTable.Add( Root, null );
            systemPropertyTable.Add( ServerVersion, null);
			systemPropertyTable.Add( StorageSize, null );
			systemPropertyTable.Add( StoreVersion, null );
			systemPropertyTable.Add( SyncRole, null );
			systemPropertyTable.Add( SystemPolicy, null );
			systemPropertyTable.Add( TombstoneType, null );
			systemPropertyTable.Add( Types, null );
			systemPropertyTable.Add( PrivateUrl, null);
			systemPropertyTable.Add( PublicUrl, null);
			systemPropertyTable.Add( PrivateKey, null);
			systemPropertyTable.Add( DomainVersion, null );
			systemPropertyTable.Add( UseSSL, null);
			systemPropertyTable.Add( MasterHost, null);
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Determines if the propertyName is a system (non-editable) property.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <returns>True if propertyName specifies a system property, otherwise false is returned.</returns>
		static public bool IsSystemProperty( string propertyName )
		{
			return systemPropertyTable.Contains( propertyName );
		}
		#endregion
	}
}
