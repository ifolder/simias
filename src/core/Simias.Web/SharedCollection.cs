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
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
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
using System.Data;
using System.Diagnostics;
using System.IO;
using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Discovery;

#if MONO
#if MONONATIVE
	// This is used if configure.in detected mono 1.1.13 or newer
	using Mono.Unix.Native;
#else
	using Mono.Unix;
#endif
#endif


namespace Simias.Web
{
	/// <summary>
	/// Return codes for check Collection Path
	/// </summary>
	public enum CollectionPathStatus
	{
		/// <summary>
		/// Indicates the path is valid for a collection
		/// </summary>
		ValidPath,
	
		/// <summary>
		/// Indicates the path is at the root of the drive
		/// </summary>
		RootOfDrivePath,
	
		/// <summary>
		/// Indicates the path contains invalid characters for an iFolder
		/// </summary>
		InvalidCharactersPath,

		/// <summary>
		/// Indicates the path is at or below the store path
		/// </summary>
		AtOrInsideStorePath,

		/// <summary>
		/// Indicates the path contains the store path
		/// </summary>
		ContainsStorePath,

		/// <summary>
		/// Indicates the path is not a fixed drive
		/// </summary>
		NotFixedDrivePath,

		/// <summary>
		/// Indicates the path is a system directory
		/// </summary>
		SystemDirectoryPath,

		/// <summary>
		/// Indicates the path is a system drive
		/// </summary>
		SystemDrivePath,

		/// <summary>
		/// Indicates the path includes the Windows direcctory
		/// </summary>
		IncludesWinDirPath,

		/// <summary>
		/// Indicates the path includes the Program Files direcctory
		/// </summary>
		IncludesProgFilesPath,

		/// <summary>
		/// Indicates the path doesn't exist
		/// </summary>
		DoesNotExistPath,

		/// <summary>
		/// Indicates the current users doesn't have read rights
		/// </summary>
		NoReadRightsPath,

		/// <summary>
		/// Indicates the current users doesn't have write rights
		/// </summary>
		NoWriteRightsPath,

		/// <summary>
		/// Indicates there is another collection below this path
		/// </summary>
		ContainsCollectionPath,

		/// <summary>
		/// Indicates the path is at or inside another collection
		/// </summary>
		AtOrInsideCollectionPath
	}


	/// <summary>
	/// SharedCollection implements all of the APIs needed to use a Shared
	/// Collection in Simias.  The APIs are designed to be wrapped by a
	/// WebService
	/// </summary>
	public class SharedCollection
	{
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		/// <summary>
		/// 
		/// </summary>
		public static readonly string FilesDirName = "SimiasFiles";
		
		/// <summary>
		/// Creates a new collection of the type specified
		/// </summary>
		/// <param name = "Name">
		/// The name of the Collection to be created
		/// </param>
		/// <param name = "UserID">
		/// The UserID to be made the owner of this Collection.
		/// A subsciption will be placed in this UserID's POBox.
		/// </param>
		/// <param name = "Type">
		/// A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type
		/// will be added.
		/// </param>
		/// <returns>
		/// Collection that was created
		/// </returns>
		public static Collection CreateSharedCollection(
				string Name, string UserID, string Type)
		{
			return CreateSharedCollection(Name, UserID, Type, false, null);
		}




		/// <summary>
		/// Creates a new collection of the type specified.  It gets the
		/// current member and makes them the owner
		/// </summary>
		/// <param name = "Name">
		/// The name of the Collection to be created
		/// </param>
		/// <param name = "Type">
		/// A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type
		/// will be added.
		/// </param>
		/// <returns>
		/// Collection that was created
		/// </returns>
		public static Collection CreateSharedCollection(
				string Name, string Type)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);
			if(domain == null)
				throw new Exception("Unable to obtain domain");

			Simias.Storage.Member member = domain.GetCurrentMember();
			if(member == null)
				throw new Exception("Unable to obtain current member");

			return CreateSharedCollection(Name, member.UserID, 
						Type, false, null);
		}




		/// <summary>
		/// Creates a new collection of the type specified.  It gets the
		/// current member and makes them the owner
		/// </summary>
		/// <param name = "LocalPath">
		/// The name of the Collection to be created
		/// </param>
		/// <param name = "Type">
		/// A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type
		/// will be added.
		/// </param>
		/// <returns>
		/// Collection that was created
		/// </returns>
		public static Collection CreateLocalSharedCollection(
				string LocalPath, string Type)
		{
			Store store = Store.GetStore();
            Domain domain = 
					store.GetDomain(store.DefaultDomain);
			if(domain == null)
				throw new Exception("Unable to obtain default domain");

			Simias.Storage.Member member = domain.GetCurrentMember();
			if(member == null)
				throw new Exception("Unable to obtain current member");

            String name = Path.GetFileName(LocalPath);
            return CreateSharedCollection(name, member.UserID, 
						Type, true, LocalPath);
		}




		/// <summary>
		/// Creates a new collection of the type specified.  It gets the
		/// current member and makes them the owner
		/// </summary>
		/// <param name = "LocalPath">
		/// The name of the Collection to be created
		/// </param>
		/// <param name="DomainID"></param>
		/// <param name = "Type">
		/// A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type
		/// will be added.
		/// </param>
		/// <returns>
		/// Collection that was created
		/// </returns>
		public static Collection CreateLocalSharedCollection(
				string LocalPath, string DomainID, string Type)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(DomainID);
			if(domain == null)
				throw new Exception("Unable to obtain specified domain");

			Simias.Storage.Member member = domain.GetCurrentMember();
			if(member == null)
				throw new Exception("Unable to obtain current member");


			String name = Path.GetFileName(LocalPath);

			return CreateSharedCollection(name, DomainID, member.UserID, 
						Type, true, LocalPath);
		}

        /// <summary>
        /// Creates a new collection of the type specified.  It gets the
        /// current member and makes them the owner
        /// </summary>
        /// <param name="LocalPath">The name of the Collection to be created</param>
        /// <param name="DomainID">Domain in which collection has to be created</param>
        /// <param name="Ssl">Whether SSL required or not</param>
        /// <param name="Type"> A Type value to add to the collection type.  Examples would be
        /// iFolder, AB:AddressBook, etc. Leave this blank and no type
        /// will be added.</param>
        /// <param name="EncryptionAlgorithm">What is the encryption alogirthm used for encryption</param>
        /// <param name="Passphrase">With what passphrase it has to be encrypted</param>
        /// <returns>Collection that was created</returns>
		public static Collection CreateLocalSharedCollection(
				string LocalPath, string DomainID, bool Ssl, string Type, string EncryptionAlgorithm, string Passphrase)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(DomainID);
			if(domain == null)
				throw new Exception("Unable to obtain specified domain");

			Simias.Storage.Member member = domain.GetCurrentMember();
			if(member == null)
				throw new Exception("Unable to obtain current member");


			String name = Path.GetFileName(LocalPath);

			return CreateSharedCollection(name, DomainID, Ssl, member.UserID, 
						Type, true, LocalPath, EncryptionAlgorithm, Passphrase);
		}

		/// <summary>
		/// WebMethod that creates and SharedCollection
		/// </summary>
		/// <param name = "Name">
		/// The name of the SharedCollection to create.  If a Path is
		/// Specified, it must match the name of the last folder in the path
		/// </param>
		/// <param name = "UserID">
		/// The UserID to be made the owner of this SharedCollection. 
		/// A subsciption will be placed in this UserID's POBox.
		/// </param>
		/// <param name = "Type">
		/// A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type
		/// will be added.
		/// </param>
		/// <param name="UnmanagedFiles"></param>
		/// <param name = "CollectionPath">
		/// The full path to this SharedCollection.  If Path is null or "",
		/// it will be ignored. The last folder name in the path should
		/// match the name of this SharedCollection
		/// </param>
		/// <returns>
		/// Collection object that was created
		/// </returns>
		public static Collection CreateSharedCollection(
				string Name, string UserID, string Type, 
				bool UnmanagedFiles, string CollectionPath)
		{
			return (CreateSharedCollection(Name, null, UserID, Type,
				UnmanagedFiles, CollectionPath));
		}




		/// <summary>
		/// WebMethod that creates a SharedCollection
		/// </summary>
		/// <param name="Name">The name of the SharedCollection to create.  If a Path is
		/// specified, it must match the name of the last folder in the path.</param>
		/// <param name="DomainID">The ID of the domain to create the collection in.</param>
		/// <param name="UserID">The UserID to be made the owner of this SharedCollection. 
		/// A subscription will be placed in this UserID's POBox.</param>
		/// <param name="Type">A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type will be added.</param>
		/// <param name="UnmanagedFiles">A value indicating if this collection contains files
		/// that are not store-managed.</param>
		/// <param name="CollectionPath">The full path to this SharedCollection.  If Path is 
		/// null or "", it will be ignored. The last folder name in the path should match the 
		/// name of this SharedCollection</param>
		/// <returns>The Collection object that was created.</returns>
		public static Collection CreateSharedCollection(
			string Name, string DomainID, string UserID, string Type,
			bool UnmanagedFiles, string CollectionPath)
		{
			return (CreateSharedCollection(Name, DomainID, UserID, Type,
				UnmanagedFiles, CollectionPath, null, null));
		}

	        /// <summary>
       		/// 
	        /// </summary>
        	/// <param name="Name"></param>
	        /// <param name="DomainID"></param>
        	/// <param name="Ssl"></param>
	        /// <param name="UserID"></param>
        	/// <param name="Type"></param>
	        /// <param name="UnmanagedFiles"></param>
        	/// <param name="CollectionPath"></param>
	        /// <param name="EncryptionAlgorithm"></param>
	        /// <param name="Passphrase"></param>
        	/// <returns></returns>
		public static Collection CreateSharedCollection(
			string Name, string DomainID, bool Ssl, string UserID, string Type,
			bool UnmanagedFiles, string CollectionPath, string EncryptionAlgorithm, string Passphrase)
		{
			return (CreateSharedCollection(Name, DomainID, Ssl, UserID, Type,
				UnmanagedFiles, CollectionPath, null, EncryptionAlgorithm, Passphrase));
		}

		/// <summary>
		/// WebMethod that creates a SharedCollection
		/// </summary>
		/// <param name="Name">The name of the SharedCollection to create.  If a Path is
		/// specified, it must match the name of the last folder in the path.</param>
		/// <param name="DomainID">The ID of the domain to create the collection in.</param>
		/// <param name="UserID">The UserID to be made the owner of this SharedCollection. 
		/// A subscription will be placed in this UserID's POBox.</param>
		/// <param name="Type">A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type will be added.</param>
		/// <param name="UnmanagedFiles">A value indicating if this collection contains files
		/// that are not store-managed.</param>
		/// <param name="CollectionPath">The full path to this SharedCollection.  If Path is 
		/// null or "", it will be ignored. The last folder name in the path should match the 
		/// name of this SharedCollection</param>
		/// <param name="Description">The description of the SharedCollection to create.</param>
		/// <param name="AccessID">The access ID for impersonation.</param>
		/// <returns>The Collection object that was created.</returns>
		public static Collection CreateSharedCollection(
			string Name, string DomainID, string UserID, string Type,
			bool UnmanagedFiles, string CollectionPath, string Description, string AccessID)
		{
			return (CreateSharedCollection(Name, DomainID, UserID, Type, UnmanagedFiles, CollectionPath, Description, AccessID, null));
		}

        /// <summary>
        /// WebMethod that creates and SharedCollection
        /// </summary>
        /// <param name="Name">The name of the SharedCollection to create.  If a Path is
        /// specified, it must match the name of the last folder in the path.</param>
        /// <param name="DomainID">The ID of the domain to create the collection in</param>
        /// <param name="UserID">The UserID to be made the owner of this SharedCollection. 
        /// A subscription will be placed in this UserID's POBox.</param>
        /// <param name="Type">A Type value to add to the collection type.  Examples would be
        /// iFolder, AB:AddressBook, etc. Leave this blank and no type will be added</param>
        /// <param name="UnmanagedFiles">A value indicating if this collection contains files
        /// that are not store-managed</param>
        /// <param name="CollectionPath">The full path to this SharedCollection.  If Path is 
        /// null or "", it will be ignored. The last folder name in the path should match the 
        /// name of this SharedCollection</param>
        /// <param name="Description">The description of the SharedCollection to create.</param>
        /// <param name="AccessID">The access ID for impersonation.</param>
        /// <param name="iFolderID">The iFolder ID for which shared collection belongs</param>
        /// <returns>The Collection object that was created.</returns>
		public static Collection CreateSharedCollection(
			string Name, string DomainID, string UserID, string Type,
			bool UnmanagedFiles, string CollectionPath, string Description, string AccessID, string iFolderID)
		{
			ArrayList nodeList = new ArrayList();
			
			log.Debug( "CreateSharedCollection-1 entered" );
			log.Debug( "  DomainID:    " + DomainID );
			log.Debug( "  UserID:      " + UserID );
			log.Debug( "  Collection:  " + Name );
			log.Debug( "  Type:        " + Type );
			if ( Description != null && Description != "" )
			{
				log.Debug( "  Description: " + Description );
			}

			// check DomainID and default
			if (DomainID == null)
			{
				DomainID = Store.GetStore().DefaultDomain;
			}

			// if they are attempting to create a Collection using
			// a path, then check to see if that path can be used
			if(	UnmanagedFiles && (CollectionPath != null) )
			{
				CollectionPathStatus pStatus;

				pStatus = CheckCollectionPath(CollectionPath);
				switch(pStatus)
				{
					case CollectionPathStatus.ValidPath:
						break;
					case CollectionPathStatus.RootOfDrivePath:
						throw new Exception("RootOfDrivePath");
					case CollectionPathStatus.InvalidCharactersPath:
						throw new Exception("InvalidCharactersPath");
					case CollectionPathStatus.AtOrInsideStorePath:
						throw new Exception("AtOrInsideStorePath");
					case CollectionPathStatus.ContainsStorePath:
						throw new Exception("ContainsStorePath");
					case CollectionPathStatus.NotFixedDrivePath:
						throw new Exception("NotFixedDrivePath");
					case CollectionPathStatus.SystemDirectoryPath:
						throw new Exception("SystemDirectoryPath");
					case CollectionPathStatus.SystemDrivePath:
						throw new Exception("SystemDrivePath");
					case CollectionPathStatus.IncludesWinDirPath:
						throw new Exception("IncludesWinDirPath");
					case CollectionPathStatus.IncludesProgFilesPath:
						throw new Exception("IncludesProgFilesPath");
					case CollectionPathStatus.DoesNotExistPath:
						throw new Exception("DoesNotExistPath");
					case CollectionPathStatus.NoReadRightsPath:
						throw new Exception("NoReadRightsPath");
					case CollectionPathStatus.NoWriteRightsPath:
						throw new Exception("NoWriteRightsPath");
					case CollectionPathStatus.ContainsCollectionPath:
						throw new Exception("ContainsCollectionPath");
					case CollectionPathStatus.AtOrInsideCollectionPath:
						throw new Exception("AtOrInsideCollectionPath");
				}
			}

			Store store = Store.GetStore();

			Domain domain = store.GetDomain(DomainID);
			if(domain == null)
				throw new Exception("Unable to obtain default domain");

			// Create the Collection and set it as an iFolder
			
			Collection c = null;
			if( iFolderID == null || iFolderID == "")
				c = new Collection(store, Name, DomainID);
			else
				c = new Collection(store, Name, iFolderID, DomainID);

			if (AccessID != null)
				c.Impersonate(domain.GetMemberByID(AccessID));

			// type
			if( (Type != null) && (Type.Length > 0) )
				c.SetType(c, Type);

			// description
			if ((Description != null) && (Description.Length > 0))
			{
				c.Properties.AddProperty(PropertyTags.Description, Description);
			}

			nodeList.Add(c);

			// Create the member and add it as the owner
			Simias.Storage.Member member = domain.GetMemberByID(UserID);
			if(member == null)
				throw new Exception("UserID is invalid");
				
			Simias.Storage.Member newMember = 
					new Simias.Storage.Member(	member.Name,
												member.UserID,
												Access.Rights.Admin);
			newMember.IsOwner = true;
			nodeList.Add(newMember);

			if(UnmanagedFiles)
			{
				string dirNodePath;

				if( (CollectionPath == null) || (CollectionPath.Length == 0) )
				{
					// create a root dir node for this iFolder in the
					// <data-dir>/simias/SimiasFiles/xx/<guid>/name
					// directory
					dirNodePath = c.UnmanagedPath;
					dirNodePath = Path.Combine(dirNodePath, Name);
					log.Debug("dirnodepath {0}", dirNodePath);

					if(!Directory.Exists(dirNodePath) )
						Directory.CreateDirectory(dirNodePath);
				}
				else
					dirNodePath = CollectionPath;

				if(!Directory.Exists(dirNodePath) )
					throw new Exception("Path did not exist");

				// create root directory node
				DirNode dn = new DirNode(c, dirNodePath);
				nodeList.Add(dn);
			}

			// Commit the new collection and the fileNode at the root
			c.Commit(nodeList.ToArray( typeof( Node) ) as Node[] );

/*#if ( !REMOVE_OLD_INVITATION )
			AddSubscription( store, c, member, 
				newMember, SubscriptionStates.Ready, Type);
#endif
*/
			log.Debug("CreateSharedCollection-1 - End");
			return c;
		}

        /// <summary>
        /// WebMethod that creates and SharedCollection
        /// </summary>
        /// <param name="Name">The name of the SharedCollection to create.  If a Path is
        /// specified, it must match the name of the last folder in the path</param>
        /// <param name="DomainID">The ID of the domain to create the collection in</param>
        /// <param name="Ssl">Whenter SSL required or not</param>
        /// <param name="UserID">The UserID to be made the owner of this SharedCollection. 
        /// A subscription will be placed in this UserID's POBox</param>
        /// <param name="Type">A Type value to add to the collection type.  Examples would be
        /// iFolder, AB:AddressBook, etc. Leave this blank and no type will be added</param>
        /// <param name="UnmanagedFiles">A value indicating if this collection contains files
        /// that are not store-managed</param>
        /// <param name="CollectionPath">The full path to this SharedCollection.  If Path is 
        /// null or "", it will be ignored. The last folder name in the path should match the 
        /// name of this SharedCollection</param>
        /// <param name="Description">The description of the SharedCollection to create</param>
        /// <param name="EncryptionAlgorithm">Algorithm with which encryption has to be done</param>
        /// <param name="Passphrase">Pass phrase with which encryption will be performed</param>
        /// <returns>The Collection object that was created</returns>
		public static Collection CreateSharedCollection(
			string Name, string DomainID, bool Ssl, string UserID, string Type,
			bool UnmanagedFiles, string CollectionPath, string Description, string EncryptionAlgorithm, string Passphrase)		{
			ArrayList nodeList = new ArrayList();
			
			log.Debug( "CreateSharedCollection-2 entered" );
			log.Debug( "  DomainID:    " + DomainID );
			log.Debug( "  UserID:      " + UserID );
			log.Debug( "  Collection:  " + Name );
			log.Debug( "  Type:        " + Type );
			if ( Description != null && Description != "" )
			{
				log.Debug( "  Description: " + Description );
			}

			// check DomainID and default
			if (DomainID == null)
			{
				DomainID = Store.GetStore().DefaultDomain;
			}

			// if they are attempting to create a Collection using
			// a path, then check to see if that path can be used
			if (UnmanagedFiles && (CollectionPath != null) )
			{
				CollectionPathStatus pStatus;

				pStatus = CheckCollectionPath(CollectionPath);
				switch(pStatus)
				{
					case CollectionPathStatus.ValidPath:
						break;
					case CollectionPathStatus.RootOfDrivePath:
						throw new Exception("RootOfDrivePath");
					case CollectionPathStatus.InvalidCharactersPath:
						throw new Exception("InvalidCharactersPath");
					case CollectionPathStatus.AtOrInsideStorePath:
						throw new Exception("AtOrInsideStorePath");
					case CollectionPathStatus.ContainsStorePath:
						throw new Exception("ContainsStorePath");
					case CollectionPathStatus.NotFixedDrivePath:
						throw new Exception("NotFixedDrivePath");
					case CollectionPathStatus.SystemDirectoryPath:
						throw new Exception("SystemDirectoryPath");
					case CollectionPathStatus.SystemDrivePath:
						throw new Exception("SystemDrivePath");
					case CollectionPathStatus.IncludesWinDirPath:
						throw new Exception("IncludesWinDirPath");
					case CollectionPathStatus.IncludesProgFilesPath:
						throw new Exception("IncludesProgFilesPath");
					case CollectionPathStatus.DoesNotExistPath:
						throw new Exception("DoesNotExistPath");
					case CollectionPathStatus.NoReadRightsPath:
						throw new Exception("NoReadRightsPath");
					case CollectionPathStatus.NoWriteRightsPath:
						throw new Exception("NoWriteRightsPath");
					case CollectionPathStatus.ContainsCollectionPath:
						throw new Exception("ContainsCollectionPath");
					case CollectionPathStatus.AtOrInsideCollectionPath:
						throw new Exception("AtOrInsideCollectionPath");
				}
			}

			Store store = Store.GetStore();
			
			Domain domain = store.GetDomain(DomainID);
			if(domain == null)
				throw new Exception("Unable to obtain default domain");

			Simias.Storage.Member member = domain.GetMemberByID(UserID);
			if(member == null)
				throw new Exception("UserID is invalid");
			

			// Create the Collection and set it as an iFolder
			Collection c = new Collection(store, Name, DomainID, Ssl, EncryptionAlgorithm, Passphrase, member.RAPublicKey);

			// type
			if( (Type != null) && (Type.Length > 0) )
				c.SetType(c, Type);

			// description
			if ((Description != null) && (Description.Length > 0))
			{
				c.Properties.AddProperty(PropertyTags.Description, Description);
			}

			nodeList.Add(c);

			// Create the member and add it as the owner
			Simias.Storage.Member newMember = 
					new Simias.Storage.Member(member.Name,
								  member.UserID,
								  Access.Rights.Admin);
			newMember.IsOwner = true;
			nodeList.Add(newMember);

			if(UnmanagedFiles)
			{
				string dirNodePath;

				if( (CollectionPath == null) || (CollectionPath.Length == 0) )
				{
					// create a root dir node for this iFolder in the
					// ~/.local/shared/simias/SimiasFiles/<guid>/name
					// directory
					dirNodePath = c.UnmanagedPath;
					dirNodePath = Path.Combine(dirNodePath, Name);
					log.Debug("dirnodepath {0}", dirNodePath);

					if(!Directory.Exists(dirNodePath) )
						Directory.CreateDirectory(dirNodePath);
				}
				else
					dirNodePath = CollectionPath;

				if(!Directory.Exists(dirNodePath) )
					throw new Exception("Path did not exist");

				// create root directory node
				DirNode dn = new DirNode(c, dirNodePath);
				nodeList.Add(dn);
			}
			

			// Commit the new collection and the fileNode at the root
			c.Commit(nodeList.ToArray( typeof( Node) ) as Node[] );
/*#if ( !REMOVE_OLD_INVITATION )

			AddSubscription( store, c, member, 
					newMember, SubscriptionStates.Ready, Type);
#endif
*/

			log.Debug("CreateSharedCollection-2 - End");
			return c;
		}
		
		/// <summary>
		/// WebMethod that creates a SharedCollection
		/// </summary>
		/// <param name="Name">The name of the SharedCollection to create.  If a Path is
		/// specified, it must match the name of the last folder in the path.</param>
		/// <param name="DomainID">The ID of the domain to create the collection in.</param>
		/// <param name="Ssl"></param>
		/// <param name="UserID">The UserID to be made the owner of this SharedCollection. 
		/// A subscription will be placed in this UserID's POBox.</param>
		/// <param name="Type">A Type value to add to the collection type.  Examples would be
		/// iFolder, AB:AddressBook, etc. Leave this blank and no type will be added.</param>
		/// <param name="UnmanagedFiles">A value indicating if this collection contains files
		/// that are not store-managed.</param>
		/// <param name="CollectionPath">The full path to this SharedCollection.  If Path is 
		/// null or "", it will be ignored. The last folder name in the path should match the 
		/// name of this SharedCollection</param>
		/// <param name="Description">The description of the SharedCollection to create.</param>
		/// <param name="AccessID">The access ID for impersonation.</param>
	        /// <param name="EncryptionAlgorithm"></param>
        	/// <param name="Passphrase"></param>
		/// <returns>The Collection object that was created.</returns>
		public static Collection CreateSharedCollection(
			string Name, string DomainID, bool Ssl, string UserID, string Type,
			bool UnmanagedFiles, string CollectionPath, string Description, string AccessID, string EncryptionAlgorithm, string Passphrase)
		{
                        ArrayList nodeList = new ArrayList();

                        log.Debug( "CreateSharedCollection entered" );
                        log.Debug( "  DomainID:    " + DomainID );
                        log.Debug( "  UserID:      " + UserID );
                        log.Debug( "  Collection:  " + Name );
                        log.Debug( "  Type:        " + Type );
                        if ( Description != null && Description != "" )
                        {
                                log.Debug( "  Description: " + Description );
                        }

                        // check DomainID and default
                        if (DomainID == null)
                        {
                                DomainID = Store.GetStore().DefaultDomain;
                        }

                        // if they are attempting to create a Collection using
                        // a path, then check to see if that path can be used
                        if(     UnmanagedFiles && (CollectionPath != null) )
                        {
                                CollectionPathStatus pStatus;

                                pStatus = CheckCollectionPath(CollectionPath);
                                switch(pStatus)
                                {
                                        case CollectionPathStatus.ValidPath:
                                                break;
                                        case CollectionPathStatus.RootOfDrivePath:
                                                throw new Exception("RootOfDrivePath");
                                        case CollectionPathStatus.InvalidCharactersPath:
                                                throw new Exception("InvalidCharactersPath");
                                        case CollectionPathStatus.AtOrInsideStorePath:
                                                throw new Exception("AtOrInsideStorePath");
                                        case CollectionPathStatus.ContainsStorePath:
                                                throw new Exception("ContainsStorePath");
                                        case CollectionPathStatus.NotFixedDrivePath:
                                                throw new Exception("NotFixedDrivePath");
                                        case CollectionPathStatus.SystemDirectoryPath:
                                                throw new Exception("SystemDirectoryPath");
                                        case CollectionPathStatus.SystemDrivePath:
                                                throw new Exception("SystemDrivePath");
                                        case CollectionPathStatus.IncludesWinDirPath:
                                                throw new Exception("IncludesWinDirPath");
                                        case CollectionPathStatus.IncludesProgFilesPath:
                                                throw new Exception("IncludesProgFilesPath");
                                        case CollectionPathStatus.DoesNotExistPath:
                                                throw new Exception("DoesNotExistPath");
                                        case CollectionPathStatus.NoReadRightsPath:
                                                throw new Exception("NoReadRightsPath");
                                        case CollectionPathStatus.NoWriteRightsPath:
                                                throw new Exception("NoWriteRightsPath");
                                        case CollectionPathStatus.ContainsCollectionPath:
                                                throw new Exception("ContainsCollectionPath");
                                        case CollectionPathStatus.AtOrInsideCollectionPath:
                                                throw new Exception("AtOrInsideCollectionPath");
                                }
                        }

                        Store store = Store.GetStore();

                        Domain domain = store.GetDomain(DomainID);
                        if(domain == null)
                                throw new Exception("Unable to obtain default domain");
						
			   Simias.Storage.Member member = domain.GetMemberByID(UserID);
			   if(member == null)
				throw new Exception("UserID is invalid");

                        Collection c = new Collection( store, Name, DomainID, Ssl, EncryptionAlgorithm, Passphrase, member.RAPublicKey);

                        if (AccessID != null)
                                c.Impersonate(domain.GetMemberByID(AccessID));

                        // type
                        if( (Type != null) && (Type.Length > 0) )
                                c.SetType(c, Type);

                        // description
                        if ((Description != null) && (Description.Length > 0))
                        {
                                c.Properties.AddProperty(PropertyTags.Description, Description);
                        }

                        nodeList.Add(c);

                        // Create the member and add it as the owner
			  Simias.Storage.Member newMember =
                                        new Simias.Storage.Member(      member.Name,
                                                                                                member.UserID,
                                                                                                Access.Rights.Admin);
                        newMember.IsOwner = true;
                        nodeList.Add(newMember);

                        if(UnmanagedFiles)
                        {
                                string dirNodePath;

                                if( (CollectionPath == null) || (CollectionPath.Length == 0) )
                                {
                                        // create a root dir node for this iFolder in the
                                        // ~/.local/shared/simias/SimiasFiles/<guid>/name
                                        // directory
						dirNodePath = c.UnmanagedPath;
						dirNodePath = Path.Combine(dirNodePath, Name);

                                        if(!Directory.Exists(dirNodePath) )
                                                Directory.CreateDirectory(dirNodePath);
                                }
                                else
                                        dirNodePath = CollectionPath;

                                if(!Directory.Exists(dirNodePath) )
                                        throw new Exception("Path did not exist");

                                // create root directory node
                                DirNode dn = new DirNode(c, dirNodePath);
                                nodeList.Add(dn);
                        }

                        // Commit the new collection and the fileNode at the root
                        c.Commit(nodeList.ToArray( typeof( Node) ) as Node[] );

/*#if ( !REMOVE_OLD_INVITATION )
                        AddSubscription( store, c, member,
                                newMember, SubscriptionStates.Ready, Type);
#endif
*/
                        return c;
                }


		/// <summary>
		/// Checks whether it is valid to make a given directory a
		/// Collection
		/// </summary>
		/// <param name="path">
		/// An absolute path to check.
		/// </param>
		/// <returns>
		/// True if the path can be a Collection, otherwise false
		/// </returns>
		/// <remarks>
		/// Nested Collections (iFolder) are not permitted.  The path is
		/// checked to see if it is within, or contains, a Collection .
		/// </remarks>
		public static bool CanBeCollection( string path )
		{
			return(	CheckCollectionPath(path) == 
						CollectionPathStatus.ValidPath);
		}


		/// <summary>
		/// Checks whether it is valid to make a given directory a
		/// Collection
		/// </summary>
		/// <param name="path">
		/// An absolute path to check.
		/// </param>
		/// <returns>
		/// CollectionPathStatus that contains the path status
		/// </returns>
		/// <remarks>
		/// There are many restrictions on a collection that has
		/// unmanaged files (like an iFolder).  This will check for all of
		/// those restrictions.
		/// </remarks>
		public static CollectionPathStatus 
							CheckCollectionPath(string path )
		{
			// Don't allow the root of a drive to be a collection.
			string parentDir = Path.GetDirectoryName( path );
			if ( ( parentDir == null ) || ( parentDir == String.Empty ) )
			{
				return CollectionPathStatus.RootOfDrivePath;
			}

			// Make sure the name of the collection doesn't contain any invalid
			// characters.  Also make sure that the path doesn't end with a
			// slash character.

            //The below check is used to verify whether the next character after parent Directory in Path 
            // is '\' (last direcotry limiter) or not. If it's '\' it should be skiped to find the exact 
            //directory name and if not, then the next character shouldn't be skiped.

            string collectionName="";
            string nextchar = path.Substring(parentDir.Length,1);
            string dirlimiter = "\\" ;
            if (string.Equals(nextchar, dirlimiter))
            {
                // nextchar is '\' , hence should be skiped
                collectionName = path.Substring(parentDir.Length + 1);
            }
            else
            {
                //nextchar is other then '\', hence shouldn't be skiped
                collectionName = path.Substring(parentDir.Length);
            }
            
			if (collectionName == null || collectionName == String.Empty
				|| !Simias.Sync.SyncFile.IsNameValid(collectionName))
			{
				return CollectionPathStatus.InvalidCharactersPath;
			}

			// Make sure the paths end with a separator.
			// Create a normalized path that can be compared on any platform.
			Uri nPath = GetUriPath(path);

			// The store path cannot be used nor any path under the store path.
			string excludeDirectory = Store.StorePath;
			if (ExcludeDirectory(nPath, excludeDirectory, true))
			{
				return CollectionPathStatus.AtOrInsideStorePath;
			}

			// Any path containing the store path cannot be used
			while (true)
			{
				excludeDirectory = Path.GetDirectoryName(excludeDirectory);
				if ((excludeDirectory == null) || excludeDirectory.Equals(Path.DirectorySeparatorChar.ToString()))
				{
					break;
				}

				if (ExcludeDirectory(nPath, excludeDirectory, false))
				{
					return CollectionPathStatus.ContainsStorePath;
				}
			}

#if WINDOWS
			// Only allow fixed drives to become iFolders.
			if (GetDriveType(Path.GetPathRoot(path)) != DRIVE_FIXED)
			{
				return CollectionPathStatus.NotFixedDrivePath;
			}

			// Don't allow System directories to become iFolders.
			if (Directory.Exists(path) && ((new DirectoryInfo(path).Attributes & FileAttributes.System) == FileAttributes.System))
			{
				return CollectionPathStatus.SystemDirectoryPath;
			}
#endif

			if (MyEnvironment.Windows)
			{
				// Don't allow the system drive to become an iFolder.
				excludeDirectory = Environment.GetEnvironmentVariable("SystemDrive");
				if (ExcludeDirectory(nPath, excludeDirectory, false))
				{
					return CollectionPathStatus.SystemDrivePath;
				}

				// Don't allow the Windows directory or subdirectories become an iFolder.
				excludeDirectory = Environment.GetEnvironmentVariable("windir");
				if (ExcludeDirectory(nPath, excludeDirectory, true))
				{
					return CollectionPathStatus.IncludesWinDirPath;
				}

				// Don't allow the Program Files directory or subdirectories become an iFolder.
				excludeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
				if (ExcludeDirectory(nPath, excludeDirectory, true))
				{
					return CollectionPathStatus.IncludesProgFilesPath;
				}
			}

#if MONO
			string ifPath = null; 
	
			// Sometimes we are asking about a folder that exists and
			// sometimes we are asking about one that doesn't.  If the
			// current folder doesn't exist, and the parent doesn't
			// exist, it ain't valid, return false;
			if(!System.IO.Directory.Exists(path))
			{
				DirectoryInfo di = System.IO.Directory.GetParent(path);
				if(di.Exists)
					ifPath = di.FullName;
				else
					return CollectionPathStatus.DoesNotExistPath;
			}
			else
				ifPath = path;

			// Check to see if the user has read rights to the
			// path used as a collection
			try
			{
#if MONONATIVE
				if(Syscall.access(ifPath,
							AccessModes.R_OK) != 0)
#else
				if(Syscall.access(ifPath,
							AccessMode.R_OK) != 0)
#endif
				{
					return CollectionPathStatus.NoReadRightsPath;
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}

			// Check to see if the user has write rights to the
			// path used as a collection
			try
			{
#if MONONATIVE
				if(Syscall.access(ifPath,
							AccessModes.R_OK) != 0)
#else
				if(Syscall.access(ifPath,
							AccessMode.R_OK) != 0)
#endif
				{
					return CollectionPathStatus.NoWriteRightsPath;
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}


			// put an ugly try catch around this to see what is 
			// happening
			try
			{
				// This will check on Linux to see if a path is on a physical
				// drive and not mounted off the network
				if(File.Exists("/proc/mounts"))
				{
					bool retval = false;
	
					FileStream fs = File.OpenRead("/proc/mounts");
					if( (fs != null) && (ifPath != null) )
					{
						StreamReader sr = new StreamReader(fs);
						string mntLine = sr.ReadLine();
	
						// Get the stat structure on the path
						Stat pathStat;
						Syscall.stat(ifPath, out pathStat);
	
						while(mntLine != null)
						{
							// verify it's a device on this box
							if(mntLine.StartsWith("/dev") && (mntLine.IndexOf("iso9660") == -1))
							{
								Stat stat;
								string[] entries;
		
								entries = mntLine.Split(' ');
								Syscall.stat(entries[1], out stat);
		
								if(stat.st_dev == pathStat.st_dev)
								{
									retval = true;
									break;
								}
							}
							mntLine = sr.ReadLine();
						}
						sr.Close();
					}
					else
					{
						Console.WriteLine("ERROR: Unable to open /proc/mounts");
					}
	
					if(!retval)
					{
						return CollectionPathStatus.NotFixedDrivePath;
					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
#endif


			Store store = Store.GetStore();

			bool ignoreCase = true;
#if LINUX
			ignoreCase = false;
#endif

			// TODO: Change this into a search
			foreach(ShallowNode sn in store)
			{
				Collection col = store.GetCollectionByID(sn.ID);
				DirNode dirNode = col.GetRootDirectory();
				if(dirNode != null)
				{
					Uri colPath = GetUriPath(dirNode.GetFullPath(col));

					if(nPath.LocalPath.Length < colPath.LocalPath.Length)
					{
						if(	string.Compare(nPath.LocalPath, 0,
								colPath.LocalPath,	0,
								nPath.LocalPath.Length, ignoreCase) == 0)
						{
							return CollectionPathStatus.ContainsCollectionPath;
						}
					}
					else
					{
						if(	string.Compare(nPath.LocalPath, 0,
								colPath.LocalPath,	0,
								colPath.LocalPath.Length, ignoreCase) == 0)
						{
							return CollectionPathStatus.AtOrInsideCollectionPath;
						}
					}
				}
			}

			return CollectionPathStatus.ValidPath;
		}




		/// <summary>
		/// Generates a comparable URI path
		/// </summary>
		/// <param name="path">
		/// Path to build URI from
		/// </param>
		/// <returns>
		/// Uri that can be compared against another Uri
		/// </returns>
		public static Uri GetUriPath(string path)
		{
			Uri uriPath = new Uri( path.EndsWith(
								Path.DirectorySeparatorChar.ToString()) ?
								path :
								path + Path.DirectorySeparatorChar.ToString());
			return uriPath;
		}




		/// <summary>
		/// Checks whether a given path is within an existing Collection.
		/// </summary>
		/// <param name="path">
		/// An absolute path to check.
		/// </param>
		/// <returns>
		/// <b>true</b> if <paramref name="path"/> is in a Collection;
		/// otherwise, <b>false</b>.
		/// </returns>
		public static bool IsPathInCollection( string path )
		{
			bool inCollection = false;
			Store store = Store.GetStore();

			// Create a normalized path that can be compared on any platform.
			Uri nPath = GetUriPath( path );

			bool ignoreCase = true;
#if LINUX
			ignoreCase = false;
#endif
			foreach(ShallowNode sn in store)
			{
				Collection col = store.GetCollectionByID(sn.ID);
				DirNode dirNode = col.GetRootDirectory();
				if(dirNode != null)
				{
					Uri colPath = GetUriPath( dirNode.GetFullPath(col) );
					if( (colPath.LocalPath.Length < nPath.LocalPath.Length) &&
						(string.Compare(nPath.LocalPath, 0, colPath.LocalPath, 0, colPath.LocalPath.Length, ignoreCase) == 0) )
					{
						inCollection = true;
						break;
					}
				}
			}
			return inCollection;
		}




		/// <summary>
		/// Checks whether a given directory is a Collection
		/// </summary>
		/// <param name="path">
		/// An absolute path to check.
		/// </param>
		/// <returns>
		/// <b>true</b> if <paramref name="path"/> is a Collection;
		/// otherwise, <b>false</b>.
		/// </returns>
		public static bool IsCollection( string path )
		{
			return ( GetCollectionByPath( path ) != null ) ? true : false;
		}




		/// <summary>
		/// Get a Collection by it's local path
		/// </summary>
		/// <param>
		/// The rooted local path of the Collection.
		/// </param>
		/// <returns>
		/// A Collection object if found
		/// </returns>
		public static Collection GetCollectionByPath( string path )
		{
			Collection col = null;
			Uri nPath = GetUriPath( path );

			Store store = Store.GetStore();

			Property p = new Property( PropertyTags.Root, Path.GetDirectoryName(path) );
			ICSList list = store.GetCollectionsByProperty(p, SearchOp.Equal);
			foreach (ShallowNode sn in list)
			{
				Collection tmpCol = store.GetCollectionByID(sn.ID);
				DirNode dirNode = tmpCol.GetRootDirectory();
				if (dirNode != null)
				{
					Uri colPath = GetUriPath( dirNode.GetFullPath(tmpCol) );
					// Compare the two paths and ignore the case if our
					// platform is not Unix
					if ( String.Compare( nPath.LocalPath, colPath.LocalPath, 
						!MyEnvironment.Unix ) == 0 )
					{
						col = tmpCol;
						break;
					}
				}
			}

			return col;
		}


		/// <summary>
		/// WebMethod that deletes a SharedCollection or the specified subscription
		/// and removes all subscriptions from all members.  Any files that were in 
		/// place if there was a DirNode will remain there
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection or subscription to delete
		/// </param>
		public static void DeleteSharedCollection(string CollectionID)
		{
			DeleteSharedCollection(CollectionID, null);
		}


		/// <summary>
		/// WebMethod that deletes a SharedCollection or the specified subscription
		/// and removes all subscriptions from all members.  Any files that were in 
		/// place if there was a DirNode will remain there
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection or subscription to delete
		/// </param>
		/// <param name="accessID">Access User ID</param>
		public static void DeleteSharedCollection(string CollectionID, string accessID)
		{
			Store store = Store.GetStore();
			log.Debug("Delete called {0}", CollectionID);
			Collection collection = store.GetCollectionByID(CollectionID);
			if(collection != null)
			{
				// impersonate
				if ((accessID != null) && (accessID.Length != 0))
				{
					Member member = collection.GetMemberByID(accessID);
					if(member != null)
					{
						// admin rights
						// note: check for admin rights before cleaning subscriptions
						if (member.Rights != Access.Rights.Admin)
						{
							throw new AccessException(collection, member, Access.Rights.Admin);
						}

						// Impersonate before the delete.
						collection.Impersonate(member);
					}
					else
					{
						Access.Rights rights = Impersonate(collection, accessID);
						if (rights != Access.Rights.Admin)
						{
							throw new AccessException(collection, member, Access.Rights.Admin);
						}
					}
				}
			
				collection.Commit(collection.Delete());
			}
/*			else
			{
				// Look for a subscription Node by it's ID. Should only ever return a single node.
				ICSList nodeList = store.GetNodesByProperty(new Property(BaseSchema.ObjectId, CollectionID), SearchOp.Equal);
				if (nodeList.Count <= 1)
				{
					foreach (ShallowNode sn in nodeList)
					{
						// Make sure that this Node is a subscription.
						if (sn.Type == NodeTypes.SubscriptionType)
						{
							// Get the user's POBox for this subscription object.
							POBox.POBox poBox = store.GetCollectionByID(sn.CollectionID) as POBox.POBox;
							if (poBox != null)
							{
								// Turn this ShallowNode into a Subscription object.
								Subscription subscription = new Subscription(poBox, sn);
								poBox.Commit(poBox.Delete(subscription));
							}
							else
							{
								throw new Exception("Invalid ID");
							}
						}
					}
				}
				else
				{
					throw new Exception("Multiple objects returned for the same ID.");
				}
			}*/
		}


                /// <summary>
                /// WebMethod that removes a SharedCollection from the local store
                /// but will leave the subscription intact.  It will result in
                /// removing the SharedCollection from this computer but remain
                /// a member.
                /// </summary>
                /// <param name = "CollectionID">
                /// The ID of the collection representing this iFolder to delete
                /// </param>
                /// <returns>
                /// The subscription for this iFolder
                /// </returns>
                public static CollectionInfo RevertSharedCollection(string CollectionID)
                {
                        log.Debug( "RevertSharedCollection called" );
                        log.Debug( "  ID: " + CollectionID );

                        Store store = Store.GetStore();
                        Collection collection = store.GetCollectionByID(CollectionID);
                        if(collection == null)
                                throw new Exception("Invalid CollectionID");

                        log.Debug( "  Name: " + collection.Name );
                        CollectionInfo cinfo = DiscoveryFramework.GetLocalCollectionInfo(collection.ID);
                        log.Debug("cinfo {0}", cinfo);
			if(cinfo == null)
				cinfo = DiscoveryFramework.GetCollectionInfo(collection.ID);
                // need to Fix this...Workgroup mastered, then delete the collection membership itself, not doing now.
/*
                        if(cinfo != null)
                        {
                                if(collection.Role == SyncRoles.Master)
                                {
                                        Domain domain = store.GetDomain( collection.Domain );
                                        if ( domain != null &&
                                                domain.ConfigType == Simias.Storage.Domain.ConfigurationType.Workgroup )
                                        {
                                                poBox.Commit( poBox.Delete( sub ) );
                                                sub = null;
                                        }

                                }
                        }
*/
/*
                        // Get the subscription for this iFolder to return.
                        Subscription sub = null;

                        // Get the member's POBox
                        Simias.POBox.POBox poBox =
                                Simias.POBox.POBox.GetPOBox( store, collection.Domain );
                        if (poBox != null)
                        {
                                Member member = collection.GetCurrentMember();

                                // Search for the matching subscription
                                sub = poBox.GetSubscriptionByCollectionID( collection.ID, member.UserID );

                                // If this collection is workgroup mastered delete the
                                // subscription as well
                                if ( sub != null && collection.Role == SyncRoles.Master )
                                {
                                        Domain domain = store.GetDomain( collection.Domain );
                                        if ( domain != null &&
                                                domain.ConfigType == Simias.Storage.Domain.ConfigurationType.Workgroup )
                                        {
                                                poBox.Commit( poBox.Delete( sub ) );
                                                sub = null;
                                        }
                                }
                        }
*/
                        collection.Commit( collection.Delete() );
                        return cinfo;
                }



		/*
		/// <summary>
		/// WebMethod that removes a SharedCollection from the local store
		/// but will leave the subscription intact.  It will result in
		/// removing the SharedCollection from this computer but remain
		/// a member.
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing this iFolder to delete
		/// </param>
		/// <returns>
		/// The subscription for this iFolder
		/// </returns>
		public static Subscription RevertSharedCollection1(string CollectionID)
		{
			log.Debug( "RevertSharedCollection called" );
			log.Debug( "  ID: " + CollectionID );
			
			Store store = Store.GetStore();
			Collection collection = store.GetCollectionByID(CollectionID);
			if(collection == null)
				throw new Exception("Invalid CollectionID");

			log.Debug( "  Name: " + collection.Name );
			
			// Get the subscription for this iFolder to return.
			Subscription sub = null;

			// Get the member's POBox
			Simias.POBox.POBox poBox = 
				Simias.POBox.POBox.GetPOBox( store, collection.Domain );
			if (poBox != null)
			{
				Member member = collection.GetCurrentMember();

				// Search for the matching subscription
				sub = poBox.GetSubscriptionByCollectionID( collection.ID, member.UserID );
				
				// If this collection is workgroup mastered delete the
				// subscription as well
				if ( sub != null && collection.Role == SyncRoles.Master )
				{
					Domain domain = store.GetDomain( collection.Domain );
					if ( domain != null && 
						domain.ConfigType == Simias.Storage.Domain.ConfigurationType.Workgroup )
					{
						poBox.Commit( poBox.Delete( sub ) );
						sub = null;
					}
				}
			}

			collection.Commit( collection.Delete() );
			return sub;
		}
		*/



		/// <summary>
		/// Impersonate the User on the Collection
		/// </summary>
		/// <param name="collection">The iFolder Collection</param>
		/// <param name="accessID">The Access User ID</param>
		/// <returns>Access Rights</returns>
		private static Access.Rights Impersonate(Collection collection, string accessID)
		{
			Simias.Storage.Access.Rights rights = Simias.Storage.Access.Rights.Deny;

			if ((accessID != null) && (accessID.Length != 0))
			{
				Member member = collection.GetMemberByID(accessID);
                                if(member == null && Simias.Service.Manager.LdapServiceEnabled == true)
                                {
                                	Store store = Store.GetStore();
                                	Domain domain = store.GetDomain(store.DefaultDomain);
                                	string[] IDs = domain.GetMemberFamilyList(accessID);
                                	foreach(string id in IDs)
                                	{
                                		log.Debug( "In iFolder getting group id: ", id);
                                		member = collection.GetMemberByID(id);
                                		if(member != null)
                                			break;
                               	 	}
                                }
				if (member == null)
				{
					throw new Exception("Invalid Access User ID");;
				}

				collection.Impersonate(member);

				rights = member.Rights;
			}
			else
			{
				// assume Admin rights with no access ID
				rights = Simias.Storage.Access.Rights.Admin;
			}

			return rights;
		}

		/// <summary>
		/// WebMethod that to set the Rights of a user on a Collection
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the Collection to which
		/// the member is to be added
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be added
		/// </param>
		/// <param name = "Rights">
		/// The Rights to be given to the newly added member
		/// Rights can be "Admin", "ReadOnly", or "ReadWrite"
		/// </param>
		/// <returns>
		/// True if the member was successfully added
		/// </returns>
		public static void SetMemberRights(	string CollectionID, 
											string UserID,
											string Rights)
		{
			SetMemberRights(CollectionID, UserID, Rights, null);
		}
	        /// <summary>
        	/// 
	        /// </summary>
	        /// <param name="CollectionID"></param>
	        /// <param name="UserID"></param>
	        /// <param name="Rights"></param>	
        	/// <param name="AccessID"></param>
		public static void SetMemberRights(     string CollectionID, string UserID, string Rights, string AccessID)
		{
			SetMemberRights(CollectionID, null, UserID, Rights, AccessID);
		}

		/// <summary>
		/// WebMethod that to set the Rights of a user on a Collection
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the Collection to which
		/// the member is to be added
		/// </param>
		/// <param name="groupid"></param>
		/// <param name = "UserID">
		/// The ID of the member to be added
		/// </param>
		/// <param name = "Rights">
		/// The Rights to be given to the newly added member
		/// Rights can be "Admin", "ReadOnly", or "ReadWrite"
		/// </param>
		/// <param name="AccessID">
		/// Perform the action as this user.
		/// </param>
		/// <returns>
		/// True if the member was successfully added
		/// </returns>
		public static void SetMemberRights(	string CollectionID, 
											string groupid,
											string UserID,
											string Rights,
											string AccessID)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col == null)
				throw new Exception("Invalid CollectionID");

			// impersonate
			Impersonate(col, AccessID);

			Simias.Storage.Member member = col.GetMemberByID(UserID);
			if(member == null)
				throw new Exception("Invalid UserID");

			if(Rights == "Admin")
				member.Rights = Access.Rights.Admin;
			else if( Rights == "Secondary")
			{
				if( member.Rights == Access.Rights.Admin )
					log.Debug("Member already has admin rights");
				else if( groupid != null)
				{
					member.Rights = Access.Rights.Secondary;
				}
			}
			else if(Rights == "ReadOnly")
				member.Rights = Access.Rights.ReadOnly;
			else if(Rights == "ReadWrite")
				member.Rights = Access.Rights.ReadWrite;
			else
				throw new Exception("Invalid Rights Specified");

			col.Commit(member);
		}



		/// <summary>
		/// WebMethod that sets the owner of a Collection
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the iFolder to which
		/// the member is to be added
		/// </param>
		/// <param name = "NewOwnerUserID">
		/// The ID of the member to be added
		/// </param>
		/// <param name = "OldOwnerRights">
		/// The Rights to be given to the newly added member
		/// Rights can be "Admin", "ReadOnly", or "ReadWrite"
		/// </param>
		/// <returns>
		/// True if the member was successfully added
		/// </returns>
		public static void ChangeOwner(	string CollectionID, 
										string NewOwnerUserID,
										string OldOwnerRights)
		{
			ChangeOwner(CollectionID, NewOwnerUserID, OldOwnerRights, null);
		}

		/// <summary>
		/// WebMethod that sets the owner of a Collection
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the iFolder to which
		/// the member is to be added
		/// </param>
		/// <param name = "NewOwnerUserID">
		/// The ID of the member to be added
		/// </param>
		/// <param name = "OldOwnerRights">
		/// The Rights to be given to the newly added member
		/// Rights can be "Admin", "ReadOnly", or "ReadWrite"
		/// </param>
		/// <param name="AccessID">
		/// Perform the action as this user.
		/// </param>
		/// <returns>
		/// True if the member was successfully added
		/// </returns>
		public static void ChangeOwner(	string CollectionID, 
										string NewOwnerUserID,
										string OldOwnerRights,
										string AccessID)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			// impersonate
			Impersonate(col, AccessID);

			Simias.Storage.Member member = 
					col.GetMemberByID(NewOwnerUserID);

			if(member == null)
				throw new Exception("UserID is not a Collection Member");

			Access.Rights rights;

			if(OldOwnerRights == "Admin")
				rights = Access.Rights.Admin;
			else if(OldOwnerRights == "ReadOnly")
				rights = Access.Rights.ReadOnly;
			else if(OldOwnerRights == "ReadWrite")
				rights = Access.Rights.ReadWrite;
			else
				throw new Exception("Invalid Rights Specified");
			
			// If it is an encrypted iFolder, then old owner must be deleted, so send Deby rights
			string EncryptionAlgorithm = col.EncryptionAlgorithm;
			if( EncryptionAlgorithm != null && EncryptionAlgorithm != string.Empty) 
				rights = Access.Rights.Deny;

			Node[] nodes = col.ChangeOwner(member, rights);

			col.Commit(nodes);
		}





		/// <summary>
		/// WebMethod that adds a member to a Collection granting the Rights
		/// specified.  Note:  This is not inviting a member, rather it is
		/// adding them and placing a subscription in the "ready" state in
		/// their POBox.
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the Collection to which
		/// the member is to be added
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be added
		/// </param>
		/// <param name = "Rights">
		/// The Rights to be given to the newly added member
		/// </param>
		/// <param name="collectionType"></param>
		public static void AddMember(	string CollectionID, 
										string UserID,
										string Rights,
										string collectionType)
		{
			AddMember(CollectionID, UserID, Rights, collectionType, null);
		}

		/// <summary>
		/// WebMethod that adds a member to a Collection granting the Rights
		/// specified.  Note:  This is not inviting a member, rather it is
		/// adding them and placing a subscription in the "ready" state in
		/// their POBox.
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the Collection to which
		/// the member is to be added
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be added
		/// </param>
		/// <param name = "Rights">
		/// The Rights to be given to the newly added member
		/// </param>
		/// <param name="collectionType"></param>
		/// <param name="AccessID">
		/// Perform the action as this user.
		/// </param>
		public static void AddMember(	string CollectionID, 
										string UserID,
										string Rights,
										string collectionType,
										string AccessID)
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(CollectionID);
			if(col == null)
				throw new Simias.NotExistException(CollectionID);

			// impersonate
			Impersonate(col, AccessID);

			Domain domain = store.GetDomain(col.Domain);
			if(domain == null)
				throw new Simias.NotExistException(col.Domain);

			Simias.Storage.Member member = domain.GetMemberByID(UserID);
			if(member == null)
				throw new Simias.NotExistException(UserID);

			Access.Rights newRights;

			if(Rights == "Admin")
				newRights = Access.Rights.Admin;
			else if(Rights == "ReadOnly")
				newRights = Access.Rights.ReadOnly;
			else if(Rights == "ReadWrite")
				newRights = Access.Rights.ReadWrite;
			else
				throw new Exception("Invalid Rights Specified");

			// Check to see if the user is already a member of the collection.
			Simias.Storage.Member newMember = col.GetMemberByID(member.UserID);
			if(newMember != null)
			{
				throw new Simias.ExistsException(member.UserID);
			}

			newMember = 
				new Simias.Storage.Member(	member.Name,
				member.UserID,
				newRights);

			col.Commit(newMember);

/*s#if ( !REMOVE_OLD_INVITATION )
			AddSubscription( store, col, 
				newMember, newMember, SubscriptionStates.Ready,
				collectionType);
#endif
*/
		}



		/// <summary>
		/// WebMethod that removes a member from a Collection. The subscription
		/// is also removed from the member's POBox.
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the iFolder from which
		/// the member is to be removed
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be removed
		/// </param>
		public static void RemoveMember(	string CollectionID, 
											string UserID)
		{
			RemoveMember(CollectionID, UserID, null);
		}

		/// <summary>
		/// WebMethod that removes a member from a Collection. The subscription
		/// is also removed from the member's POBox.
		/// </summary>
		/// <param name = "CollectionID">
		/// The ID of the collection representing the iFolder from which
		/// the member is to be removed
		/// </param>
		/// <param name = "UserID">
		/// The ID of the member to be removed
		/// </param>
		/// <param name="AccessID">
		/// Perform the action as this user.
		/// </param>
		public static void RemoveMember(	string CollectionID, 
											string UserID,
											string AccessID)
		{
			Member GroupMember = null;
			Member tmpMember = null;
			Member member = null;
			Member MemberInCol = null;
			Access.Rights rights = Access.Rights.Admin;
			Domain domain = null;

			Store store = Store.GetStore();
			Collection col = store.GetCollectionByID(CollectionID);
			if(col == null)
				throw new Exception("Invalid CollectionID");

			domain = store.GetDomain(col.Domain);
			if(domain == null)
			{
				throw new Exception("Domain is null");
			}

			MemberInCol = col.GetMemberByID(UserID);
			if(MemberInCol == null && Simias.Service.Manager.LdapServiceEnabled == true)
			{
				string[] IDs = domain.GetMemberFamilyList(UserID);
				foreach(string id in IDs)
				{
					log.Debug( "In iFolder getting group id: ", id);
					tmpMember = col.GetMemberByID(id);
					if(tmpMember != null)
					{
						// Member is not part of the collection , but this group is where he belongs to 
						GroupMember = tmpMember;
						break;
					}
				}
			}

			if(GroupMember != null)
			{
				// it means he is not part of collection but the group he belongs to is, now check for group's rights
				// so that if group has read only rights, we will not allow the user to remove himself

				rights = Impersonate(col, UserID);
				if( rights == Access.Rights.ReadOnly )
				{
					// User is not member of collection and part of the group and group has readonly rights, so refuse
					throw new Exception("Can not remove the membership as group has readonly rights");
				}
			}
			else
			{
				// Normal case, and 
				// if member is part of collection and group is also part of collection , give preference to member, and remove
				// impersonate
				rights = Impersonate(col, AccessID);
			}
	

#if ( !REMOVE_OLD_INVITATION )
			if ( domain.SupportsNewInvitation == false )
			{
				// Get my member object.
				Member currentMember = null;
				if((AccessID != null) && (AccessID.Length != 0))
					currentMember = col.GetCurrentMember();
				else
					currentMember = col.GetMemberByID(UserID);
					
				if (currentMember == null)
					throw new Exception("Invalid current member");

				// Get a handle to my POBox.
				POBox.POBox poBox = POBox.POBox.FindPOBox(store, col.Domain, currentMember.UserID);
				if(poBox == null)
				{
					throw new Exception("Unable to access POBox");
				}

				// Try to find any pending subscription that I am sending to the specified user.
				ICSList poList = poBox.Search(Subscription.ToIdentityProperty, UserID, SearchOp.Equal);
				foreach(ShallowNode sNode in poList)
				{
					Subscription sub = new Subscription(poBox, sNode);
					if (sub != null)
					{
						poBox.Commit(poBox.Delete(sub));
					}
				}
			}
#endif

			// Remove the user if they are a member of the collection already.
			member = col.GetMemberByID(UserID);
			if (member == null && rights == Access.Rights.Admin && GroupMember != null)
			{
				// member is not part of collection , but his group is part of the collection and group has fullcontrol/ownership
				log.Debug("assigning group id to member so that group can be removed from the collection");
				member = GroupMember;
			}
			if(member != null)
			{
				log.Debug("going to call collection.delete for this member");
				if(member.IsOwner)
					throw new Exception("UserID is the iFolder owner");

				col.Commit(col.Delete(member));

#if ( !REMOVE_OLD_INVITATION )
				// even if the member is null, try to clean up the subscription
				RemoveMemberSubscription(store, col, UserID);
#endif
			}
		}




		/// <summary>
		/// WebMethod that removes a subscription for an iFolder.
		/// </summary>
		/// <param name="DomainID">
		/// The ID of the domain that the subscription belongs to.
		/// </param>
		/// <param name="SubscriptionID">
		/// The ID of the subscription to remove.
		/// </param>
		/// <param name="UserID">
		/// The ID of the user owning the POBox where the subscription is stored.
		/// </param>
		public static void RemoveSubscription(string DomainID,
											  string SubscriptionID,
											  string UserID)
		{
			Store store = Store.GetStore();

			// Get the current member's POBox
			Simias.POBox.POBox poBox = Simias.POBox.POBox.FindPOBox(store,
																   DomainID,
																   UserID);
			if (poBox != null)
			{
				Node node = poBox.GetNodeByID(SubscriptionID);
				if (node != null)
				{
					Subscription sub = new Subscription(node);

					if(sub != null)
					{
						poBox.Commit(poBox.Delete(sub));
					}
				}
			}
		}




		/// <summary>
		/// WebMethod that calculates the number of nodes and bytes that need to be sync'd.
		/// </summary>
		/// <param name="collection">The collection to calculate the sync size.</param>
		/// <param name="nodeCount">On return, contains the number of nodes that need to be sync'd.</param>
		/// <param name="maxBytesToSend">On return, contains the number of bytes that need to be sync'd.</param>
		public static void CalculateSendSize(Collection collection,	out uint nodeCount, out ulong maxBytesToSend)
		{
			SyncSize.CalculateSendSize(collection, out nodeCount, out maxBytesToSend);
		}




		/// <summary>
		/// WebMethod that causes the collection of the specified ID to be sync'd immediately.
		/// </summary>
		/// <param name="CollectionID">The ID of the collection to sync.</param>
		public static void SyncCollectionNow(string CollectionID)
		{
			SyncClient.ScheduleSync(CollectionID);
		}

		/// <summary>
		/// WebMethod that gets the default public key for ifolder key encryption
		/// </summary>
		/// <param name="DomainID"> The domain for which the public key needs to be fetched </param>
		/// <param name="UserID"> The member of the Domain </param>
		/// <returns> The default public key of the domain </returns>
		public static string GetDefaultPublicKey(string DomainID, string UserID)
		{
                        Store store = Store.GetStore();
                        Domain domain = store.GetDomain(DomainID);
			Simias.Storage.Member member = null;

                        if(domain == null)
                                throw new Exception("Unable to obtain default domain");

			if(UserID == null)
                        	member = domain.GetCurrentMember();
			else
	                        member = domain.GetMemberByID(UserID);

                        if(member == null)
                                throw new Exception("UserID is invalid");

			return member.GetDefaultPublicKeyFromServer();
		}


#if ( !REMOVE_OLD_INVITATION )
/*		
		/// <summary>
		/// Utility method that should be moved into the POBox class.
		/// This will create a subscription and place it in the POBox
		/// of the invited user.
		/// </summary>
		/// <param name = "store">
		/// The store where the POBox and collection for this subscription
		/// is to be found.
		/// </param>
		/// <param name = "collection">
		/// The Collection for which the subscription is being created
		/// </param>
		/// <param name = "inviteMember">
		/// The Member from which the subscription is being created
		/// </param>
		/// <param name = "newMember">
		/// The Member being invited
		/// </param>
		/// <param name = "state">
		/// The initial state of the subscription when placed in the POBox
		/// of the invited Member
		/// </param>
		private static void AddSubscription(	Store store, 
											Collection collection, 
											Simias.Storage.Member inviteMember,
											Simias.Storage.Member newMember,
											SubscriptionStates state,
											string collectionType)
		{
			Domain domain = store.GetDomain(collection.Domain);
			if ((domain != null) && (domain.SupportsNewInvitation == false))
			{
				Simias.POBox.POBox poBox = 
					Simias.POBox.POBox.GetPOBox(store, collection.Domain, 
					newMember.UserID );

				Subscription sub = poBox.CreateSubscription(collection,
					inviteMember,
					collectionType);
				sub.ToName = newMember.Name;
				sub.ToIdentity = newMember.UserID;
				sub.ToMemberNodeID = newMember.ID;
				sub.ToPublicKey = newMember.PublicKey;
				sub.SubscriptionRights = newMember.Rights;
				sub.SubscriptionState = state;

				DirNode dirNode = collection.GetRootDirectory();
				if(dirNode != null)
				{
					sub.DirNodeID = dirNode.ID;
					sub.DirNodeName = dirNode.Name;
				}

				poBox.Commit(sub);
			}
		}
*/



		/// <summary>
		/// Utility method that will find all subscriptions to a collection
		/// and remove the subscription to this collection
		/// </summary>
		/// <param name = "store">
		/// The store where the POBox and collection for this subscription
		/// is to be found.
		/// </param>
		/// <param name = "col">
		/// The Collection for which the subscription is being removed
		/// </param>
		private static void RemoveAllSubscriptions(Store store, Collection col)
		{
			Domain domain = store.GetDomain(col.Domain);
			if ((domain != null) && (domain.SupportsNewInvitation == false))
			{
				ICSList subList = store.GetNodesByProperty(
					new Property(Subscription.SubscriptionCollectionIDProperty, col.ID),
					SearchOp.Equal);

				foreach(ShallowNode sn in subList)
				{
					Collection c = store.GetCollectionByID(sn.CollectionID);
					if(c != null)
					{
						c.Commit( c.Delete( new Node(c, sn) ) );
					}
				}
			}
		}



		/// <summary>
		/// Utility method that removes a subscription for the specified
		/// collection from the specified UserID
		/// </summary>
		/// <param name = "store">
		/// The store where the POBox and collection for this subscription
		/// is to be found.
		/// </param>
		/// <param name = "col">
		/// The Collection for which the subscription is being removed
		/// </param>
		/// <param name = "UserID">
		/// The UserID from which to remove the subscription
		/// </param>
		private static void RemoveMemberSubscription(	Store store, Collection col, string UserID)
		{
			Domain domain = store.GetDomain(col.Domain);
			if((domain != null) && (domain.SupportsNewInvitation == false))
			{
				// Get the member's POBox
				Simias.POBox.POBox poBox = Simias.POBox.POBox.FindPOBox(store, col.Domain, UserID );
				if (poBox != null)
				{
					// Search for the matching subscription
					Subscription sub = poBox.GetSubscriptionByCollectionID(col.ID);
					if(sub != null)
					{
						poBox.Commit(poBox.Delete(sub));
					}
				}
			}
		}
#endif

        /// <summary>
        /// Whether a directory to be excluded or not
        /// </summary>
        /// <param name="path">Uri of the path</param>
        /// <param name="excludeDirectory">Directory to be excluded</param>
        /// <param name="deep">Depth of the directory</param>
        /// <returns>Returns true if to be excluded else false</returns>
		private static bool ExcludeDirectory(Uri path, string excludeDirectory, bool deep)
		{
			Uri excludePath = new Uri(excludeDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()) ?
								excludeDirectory :
								excludeDirectory + Path.DirectorySeparatorChar.ToString());

			if (!(path.LocalPath.Length < excludePath.LocalPath.Length) &&
				(String.Compare(deep ? path.LocalPath.Substring(0, excludePath.LocalPath.Length) : path.LocalPath, excludePath.LocalPath, true) == 0))
			{
				return true;
			}

			return false;
		}

#if WINDOWS
		private const uint DRIVE_REMOVABLE = 2;
		private const uint DRIVE_FIXED = 3;

		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		private static extern uint GetDriveType(string rootPathName);
#endif

	}
}
