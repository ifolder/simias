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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;

using Simias;
using Simias.Event;
using Simias.POBox;
using Simias.Storage;

namespace Simias.LdapProvider
{
	[Serializable]
	public enum Status
	{
		Syncing,
		Sleeping,
		LdapConnectionFailure,
		LdapAuthenticationFailure,
		SyncThreadDown,
		ConfigurationError,
		InternalException
	}

	/// <summary>
	/// Service class used to get an execution context
	/// so we can register ourselves with the external
	/// sync container
	/// </summary>
	public class Sync : Simias.IIdentitySyncProvider
	{
		#region Class Members
		private readonly string name = "LDAP Synchronization";
		private readonly string description = "LDAP Synchronization provider to synchronize identities from an ldap store to a simias domain";
		private bool abort = false;
		
		private Status syncStatus;
		private static LdapSettings ldapSettings;
		private Exception syncException;

		private Store store = null;
		private Domain domain = null;
		
		private LdapConnection conn = null;
		private Simias.IdentitySync.State state = null;
		
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		public string Name { get{ return name; } }

		/// <summary>
		/// Gets the description of the provider.
		/// </summary>
		public string Description { get{ return description; } }
		#endregion
		
		#region Private Methods
		
        /// <summary>
        /// Process the users, groups and container through this ldap connection
        /// </summary>
        /// <param name="conn">ldap connection</param>
        /// <param name="settings">ldapsettings</param>
		private void ProcessSearchObjects( LdapConnection conn, LdapSettings settings )
		{
			foreach ( string searchContext in settings.SearchContexts )
			{
				string[] searchAttributes = { "objectClass" };

				log.Debug( "SearchObject: " + searchContext );

				try
				{
					LdapEntry ldapEntry = conn.Read( searchContext, searchAttributes );
					LdapAttribute attrObjectClass = ldapEntry.getAttribute( "objectClass" );
					String[] values = attrObjectClass.StringValueArray;

					if ( IsUser( values ) == true )
					{
						// Process SearchDN as 
						log.Debug( "Processing User Object..." );
						ProcessSearchUser( conn, searchContext );
					}
					else if ( IsGroup( values ) == true )
					{
						// Process SearchDN as 
						log.Debug( "Processing Group Object..." );
						ProcessSearchGroup( conn, searchContext );
					}
					else if ( IsContainer( values ) == true )
					{
						// Process SearchDN as Container
						log.Debug( "Processing Container Object..." );
						ProcessSearchContainer( conn, searchContext );
					}
					else
					{
						log.Debug( "Invalid objectClass: " + values[0] );
						log.Debug( attrObjectClass.ToString() );
					}
				}
				catch( SimiasShutdownException s )
				{
					log.Error( "ProcessSearchObjects SimiasShutdownException" );
					log.Error( s.Message );
					throw s;
				}
				catch ( LdapException e )
				{
					log.Error( "ProcessSearchObjects LdapException" );
					log.Error( e.LdapErrorMessage );
					log.Error( e.StackTrace );
					throw e;
				}
				catch ( Exception e )
				{
					log.Error( "ProcessSearchObjects Exception" );
					log.Error( e.Message );
					log.Error( e.StackTrace );
					throw e;
				}
			}
		}
		
        /// <summary>
        /// build the GUID filter using GUID=
        /// </summary>
        /// <param name="guid">guid</param>
        /// <returns>returns new filtered GUID</returns>
		private string BuildGuidFilter( string guid )
		{
			Guid cGuid = new Guid( guid );
			byte[] bGuid = cGuid.ToByteArray();

			string guidFilter = "(GUID=";
			string tmp;

			// The CSharpLdap SDK expects each byte to
			// be zero padded else an exception is thrown
			for( int i = 0; i < 16; i++ )
			{
				guidFilter += "\\";
				tmp = Convert.ToString( bGuid[i], 16 );
				if ( tmp.Length == 1 )
				{
					guidFilter += "0";
				}
				guidFilter += tmp;
			}

			guidFilter += ")";
			return guidFilter;
		}

        /// <summary>
        /// get ldap GUID
        /// </summary>
        /// <param name="entry">ldap entry</param>
        /// <returns>ldap GUID</returns>
		private string GetLdapGuid( LdapEntry entry )
		{
			string ldapGuid = null;

			try
			{
				LdapAttribute guidAttr = entry.getAttribute( "GUID" );
				if ( guidAttr != null && guidAttr.StringValue.Length != 0 )
				{
					byte[] bGuid = new byte[8];
					for( int i = 0; i < 8; i++ )
					{
						bGuid[i] = (byte) guidAttr.ByteValue[i];
					}

					Guid cGuid = 
						new Guid(
							BitConverter.ToInt32( bGuid, 0 ),
							BitConverter.ToInt16( bGuid, 4 ),
							BitConverter.ToInt16( bGuid, 6 ),
							(byte) guidAttr.ByteValue[8], 
							(byte) guidAttr.ByteValue[9],
							(byte) guidAttr.ByteValue[10],
							(byte) guidAttr.ByteValue[11],
							(byte) guidAttr.ByteValue[12],
							(byte) guidAttr.ByteValue[13],
							(byte) guidAttr.ByteValue[14],
							(byte) guidAttr.ByteValue[15] );

					ldapGuid = cGuid.ToString();
				}
			}
			catch{}
			return ldapGuid;
		}

        /// <summary>
        /// whether objectclass has user object
        /// </summary>
        /// <param name="objectClasses">object classes to be searched</param>
        /// <returns>true if it has inetorgperson attribute</returns>
		private bool IsUser( String[] objectClasses )
		{
			try
		    {
				foreach( string s in objectClasses )
				{
					if ( s.ToLower() == "inetorgperson" )
					{
						return true;
					}
				}
			}
		    catch( Exception e )
		    {
				log.Error( "IsUser failed with exception" );
				log.Error( e.Message );
			}
		    return false;
		}

        /// <summary>
        /// whether objectclass has group attribute
        /// </summary>
        /// <param name="objectClasses">object classes to be searched</param>
        /// <returns>true if it has group</returns>
		private bool IsGroup( String[] objectClasses )
		{
			try
			{
				foreach( string s in objectClasses )
				{
					if ( s.ToLower() == "groupofnames" )
					{
						return true;
					}
				}
		    }
		    catch( Exception e )
			{
				log.Error( "IsGroup failed with exception" );
				log.Error( e.Message );
			}
		    return false;
		}

        /// <summary>
        /// whether this objectclases contain container
        /// </summary>
        /// <param name="objectClasses">to be searched into</param>
        /// <returns>true if it has container</returns>
		private bool IsContainer( String[] objectClasses )
		{
			bool isContainer = false;

			try
			{
				foreach( string s in objectClasses )
				{
					string lower = s.ToLower();
					if ( lower == "organization" )
					{
						log.Debug( "Processing Organization Object..." );
						isContainer = true;
						break;
					}
					else if ( lower == "organizationalunit" )
					{
						isContainer = true;
						log.Debug( "Processing OrganizationalUnit Object..." );
						break;
					}
					else if ( lower == "country" )
					{
						isContainer = true;
						log.Debug( "Processing Country Object..." );
						break;
					}
					else if ( lower == "locality" )
					{
						isContainer = true;
						log.Debug( "Processing Locality Object..." );
						break;
					}
				}
			}
			catch( Exception e )
			{
				log.Error( "IsContainer failed with exception" );
				log.Error( e.Message );
			}

			return isContainer;
		}

        /// <summary>
        /// process this user by calling processuserentry
        /// </summary>
        /// <param name="connection">ldapconnection</param>
        /// <param name="searchUser">user to be searched</param>
		private void ProcessSearchUser( LdapConnection connection, String searchUser )
		{
			// Since the first version of the iFolder 3.0 only
		    // exposes a username, firstname, lastname and full
		    // name, we'll limit the scope of the search
			string[] searchAttributes = {	
					"modifytimestamp",
					ldapSettings.NamingAttribute,
					"cn",
					"sn",
					"GUID",
					"givenName",
					"ou" };

		    log.Debug( "ProcessSearchUser(" + searchUser + ")" );

			try
			{
				LdapEntry ldapEntry = connection.Read( searchUser, searchAttributes );
				ProcessUserEntry( ldapEntry );
			}
			catch( SimiasShutdownException s )
			{
				throw s;
			}
			catch( LdapException e )
			{
				log.Error( e.LdapErrorMessage );
				log.Error( e.StackTrace );
			}
			catch( Exception e ) 
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
		}

        /// <summary>
        /// If the configured Simias Admin is different than the SimiasAdmin
        /// identified in the store, make all the changes necessary to
        /// make the configured admin the store admin.
        /// </summary>
        /// <param name="conn"></param>
		private void ChangeSimiasAdmin( LdapConnection conn )
		{
			char[] dnDelimiters = {',', '='};
			LdapEntry entry = null;
			Property dn;
			string commonName;
			string ldapGuid;
			string[] searchAttributes = { "cn", "sn", "GUID" };

			try
			{
				// Nothing in the config for a SimiasAdmin - we're done here
				if ( ldapSettings.AdminDN == null || ldapSettings.AdminDN == "" )
				{
					return;
				}

				// If the SimiasAdmin has been changed in the Simias.config, which BTW
				// is not something that is exposed in the normal management UI, 
				// we need to verify the new SimiasAdmin exists in the directory,
				// check if the new admin exists in local domain memberlist (and
				// if he doesn't create him), transfer default domain ownership
				// to the new SimiasAdmin and lastly transfer ownership of all
				// orphaned iFolders to the new SimiasAdmin.

				try
				{
					entry = conn.Read( Sync.ldapSettings.AdminDN, searchAttributes );
				}
				catch( LdapException lEx )
				{
					log.Error( "Could not verify the newly configured Simias Administrator in the directory" );
					log.Error( lEx.Message );
				}
				catch( Exception e1 )
				{
					log.Error( "Could not verify the newly configured Simias Administrator in the directory" );
					log.Error( e1.Message );
				}

				if ( entry == null )
				{
					return;
				}

				ldapGuid = GetLdapGuid( entry );
				if ( ldapGuid == null || ldapGuid == "" )
				{
					return;
				}

				// Get the common name from the Simias.config.AdminDN entry
				string[] components = Sync.ldapSettings.AdminDN.Split( dnDelimiters );
				commonName = ( components[0].ToLower() == "cn" ) ? components[1] : components[0];
				if ( commonName == null || commonName == "" )
				{
					return;
				}

				store = Store.GetStore();
				if ( domain == null )
				{
					domain = store.GetDomain( store.DefaultDomain );
					if ( domain == null )
					{
						throw new SimiasException( "Enterprise domain does not exist!" );
					}
				}

				Member member = domain.GetMemberByName( commonName );
				if ( member == null )
				{
					// Create the member with the Ldap guid
					member = 
						new Member(	commonName,	ldapGuid, Simias.Storage.Access.Rights.ReadOnly );
					member.Properties.ModifyProperty( "DN", ldapSettings.AdminDN );
				}

				Property lguid = new Property( "LdapGuid", ldapGuid );
				lguid.LocalProperty = true;
				member.Properties.ModifyProperty( lguid );
				domain.Commit( member );

				// Transfer ownership of all collections owned by the 
				// previous admin that have the orphaned property
				Property orphaned;
				ICSList subList = store.GetCollectionsByOwner( domain.Owner.ID, domain.ID ); 
				foreach ( ShallowNode sn in subList )
				{
					// Get the collection object for this node.
					Collection c = store.GetCollectionByID( sn.CollectionID );
					if ( c != null )
					{
						orphaned = c.Properties.GetSingleProperty( "OrphanedOwner" );
						if ( orphaned != null )
						{
							dn = c.Owner.Properties.GetSingleProperty( "DN" );
							if ( dn != null )
							{
								c.PreviousOwner = dn.Value.ToString();
								c.Commit();
							}

							c.Commit( c.ChangeOwner( member, Simias.Storage.Access.Rights.ReadWrite ) );
						}
					}
				}

				// For now I'm just going to leave the LdapGuid property
				// on the old SimiasAdmin
				dn = domain.Owner.Properties.GetSingleProperty( "DN" );
				if ( dn != null )
				{
					domain.PreviousOwner = dn.Value.ToString();
					domain.Commit();
				}
				
				domain.Commit( domain.ChangeOwner( member, Simias.Storage.Access.Rights.ReadWrite ) );
			}
			catch( Exception vsa )
			{
				log.Error( vsa.Message );
				log.Error( vsa.StackTrace );
			}
		}


        /// <summary>
        /// The SimiasAdmin is processed differently than normal simias users because
        /// the account is aleady created in the Simias store before LdapSync runs
        /// so the GUID has already been created.  The SimiasAdmin must always exist in the
        /// store and the DN entry in the store must be correct with the Distinguished
        /// Name in the directory.  LdapSync counts on the AdminDN entry in Simias.config
        /// to be updated if the admin is moved in the directory.
        /// </summary>
        /// <param name="conn">ldap connection</param>
		private void ProcessSimiasAdmin( LdapConnection conn )
		{
			// Since the first version of the iFolder 3.0 only
			// exposes a username, firstname, lastname and full
			// name, we'll limit the scope of the search
			string[] searchAttributes = {
						"modifytimestamp",
						ldapSettings.NamingAttribute,
						"cn",
						"sn",
						"GUID",
						"givenName" };

			char[] dnDelimiters = {',', '='};
			LdapEntry entry = null;
			LdapAttribute timeStampAttr = null;
			Member cMember = null;
			Property dn = null;
			string ldapGuid = null;

			log.Debug( "ProcessSimiasAdmin( " + ldapSettings.AdminDN + ")" );

			if ( domain == null )
			{
				store = Store.GetStore();
				domain = store.GetDomain( store.DefaultDomain );
				if ( domain == null )
				{
					throw new SimiasException( "Enterprise domain does not exist!" );
				}
			}

			// If the DN property has never been set on the SimiasAdmin,
			// set it now
			cMember = domain.Owner;
			dn = cMember.Properties.GetSingleProperty( "DN" );
			if ( dn == null || dn.Value.ToString() == "" )
			{
				if ( ldapSettings.AdminDN != null && ldapSettings.AdminDN != "" )
				{
					dn = new Property( "DN", ldapSettings.AdminDN );
					cMember.Properties.ModifyProperty( dn );
				}
			}

			// Check if the Simias Admin has changed in configuration
			if ( ldapSettings.AdminDN != null && ldapSettings.AdminDN != "" &&
				dn.Value.ToString() != ldapSettings.AdminDN )
			{
				ChangeSimiasAdmin( conn );
				cMember = domain.Owner;
			}

			// The Simias admin is tracked in the directory by the directory
			// guid.  Make sure the guid is stored in the node
			Property lguidProp = cMember.Properties.GetSingleProperty( "LdapGuid" );
			if ( lguidProp == null )
			{
				// This must be the first time thru so let's get the directory
				// entry based on the configured DN
				try
				{
					entry = conn.Read( ldapSettings.AdminDN, searchAttributes );
				}
				catch( LdapException lEx )
				{
					log.Error( "The Simias Administrator does not exist in the Ldap directory as configured in Simias.config!" );
					log.Error( lEx.Message );
				}
				catch( Exception e1 )
				{
					log.Error( "The Simias Administrator does not exist in the Ldap directory as configured in Simias.config!" );
					log.Error( e1.Message );
				}

				if ( entry != null )
				{
					ldapGuid = GetLdapGuid( entry );
					lguidProp = new Property( "LdapGuid", ldapGuid );
					lguidProp.LocalProperty = true;
					cMember.Properties.ModifyProperty( lguidProp );
				}
			}
			else
			{
				ldapGuid = lguidProp.Value.ToString();
			}

			if ( ldapGuid != null )
			{
				try
				{
					entry = null;

					// Now go find the SimiasAdmin in the Ldap directory
					string guidFilter = BuildGuidFilter( ldapGuid );
					LdapSearchResults results = 
						conn.Search(
							"",
							LdapConnection.SCOPE_SUB,
							"(&(objectclass=inetOrgPerson)" + guidFilter + ")",
							searchAttributes,
							false);
					if ( results.hasMore() == true )
					{
						entry = results.next();
					}
				}
				catch ( LdapException e )
				{
					log.Error( e.LdapErrorMessage );
					log.Error( e.StackTrace );
				}
				catch ( Exception e )
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
				}

				if ( entry != null )
				{
					//
					// check if the ldap object's time stamp has changed
					//
					try
					{
						timeStampAttr = entry.getAttribute( "modifytimestamp" );
						Property pStamp = 
							cMember.Properties.GetSingleProperty( "LdapTimeStamp" );

						if ( ( pStamp == null ) ||
							( pStamp != null && 
							(string) pStamp.Value != timeStampAttr.StringValue ) )
						{
							// The time stamp changed let's look at first and
							// last name
	
							try
							{
								bool changed = false;
	
								// If we're tracking by ldap see if the naming attribute
								// has changed
								LdapAttribute namingAttr = entry.getAttribute( ldapSettings.NamingAttribute );
								if ( namingAttr != null && namingAttr.StringValue.Length != 0 )
								{
									if ( namingAttr.StringValue != cMember.Name )
									{
										cMember.Name = namingAttr.StringValue;
									}
								}
	
								LdapAttribute givenAttr = entry.getAttribute( "givenName" );
								if ( givenAttr != null && givenAttr.StringValue.Length != 0 )
								{
									if ( givenAttr.StringValue != cMember.Given )
									{
										changed = true;
										cMember.Given = givenAttr.StringValue;
									}
								}

								LdapAttribute sirAttr = entry.getAttribute( "sn" );
								if ( sirAttr != null && sirAttr.StringValue.Length != 0 )
								{
									if ( sirAttr.StringValue != cMember.Family )
									{
										cMember.Family = sirAttr.StringValue;
										changed = true;
									}
								}


								// If the entry has changed and we have a valid
								// family and given
								if ( changed == true && 
									cMember.Given != null &&
									cMember.Given != "" && 
									cMember.Family != null &&
									cMember.Family != "" )
								{
									cMember.FN = cMember.Given + " " + cMember.Family;
								}

								// Did the distinguished name change?
								Property dnProp = cMember.Properties.GetSingleProperty( "DN" );
								if ( dnProp != null && ( dnProp.ToString() != entry.DN ) )
								{
									dnProp.Value = entry.DN;
									cMember.Properties.ModifyProperty( "DN", dnProp );
								}
							}
							catch {}

							pStamp = new Property( "LdapTimeStamp", timeStampAttr.StringValue );
							pStamp.LocalProperty = true;
							cMember.Properties.ModifyProperty( pStamp );
						}
					}
					catch{}
				}
				else
				{
					log.Error( "The Simias administrator could not be verified in the directory!" );
					log.Error( "Please update Simias.config with a valid Ldap user" );
				}
			}
			else
			{
				log.Error( "The Simias administrator could not be verified in the directory!" );
				log.Error( "Please update Simias.config with a valid Ldap user" );
			}

			// Now matter what always update the sync guid so
			// the SimiasAdmin won't be deleted from Simias

			cMember.Properties.ModifyProperty( state.SyncGuid );
			domain.Commit( cMember );
		}

        /// <summary>
        /// process each member of this group
        /// </summary>
        /// <param name="conn">ldap connection</param>
        /// <param name="searchGroup">group to be processed</param>
		private void ProcessSearchGroup( LdapConnection conn, String searchGroup )
		{
			string[] searchAttributes = {
			"objectClass",
			"cn",
			"member" };

			log.Debug( "ProcessSearchGroup(" + searchGroup + ")" );

			int count = 0;

			try
			{
				LdapEntry ldapEntry = conn.Read( searchGroup, searchAttributes );
				String[] members = ldapEntry.getAttribute("member").StringValueArray;
	
				foreach( String member in members )
				{
					// Check if the sync engine wants us to abort
					if ( this.abort == true )
					{
						return;
					}

					log.Debug( "   Processing member: " + member );
					count++;
					ProcessSearchUser( conn, member );
				}
			}
			catch( SimiasShutdownException s )
			{
				throw s;
			}
			catch( LdapException e )
			{
				log.Error( e.LdapErrorMessage );
				log.Error( e.StackTrace );
			}
			catch( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}

			log.Debug( "Processed " + count.ToString() + " entries" );
		}

        /// <summary>
        /// process each member of the container
        /// </summary>
        /// <param name="conn">ldap connection</param>
        /// <param name="searchContainer">container to be processed</param>
		private void ProcessSearchContainer(LdapConnection conn, String searchContainer)
		{
			String searchFilter = "(objectclass=user)";
			string[] searchAttributes = {
						"modifytimestamp",
						ldapSettings.NamingAttribute,
						"cn",
						"sn",
						"GUID",
						"givenName",
						"ou" };

			log.Debug( "ProcessSearchContainer(" + searchContainer + ")" );

			int count = 0;
			LdapSearchConstraints searchConstraints = new LdapSearchConstraints();
			searchConstraints.MaxResults = 0;

		    LdapSearchQueue queue = 
				conn.Search(
					searchContainer, 
					LdapConnection.SCOPE_SUB, 
					searchFilter, 
					searchAttributes, 
					false,
					(LdapSearchQueue) null,
					searchConstraints);

		    LdapMessage ldapMessage;
		    while( ( ldapMessage = queue.getResponse() ) != null )
			{
				// Check if the sync engine wants us to abort
				if ( this.abort == true )
				{
					return;
				}

				if ( ldapMessage is LdapSearchResult )
				{
					LdapEntry cEntry = ((LdapSearchResult) ldapMessage).Entry;
					if (cEntry == null)
					{
						continue;
					}

					try
					{
						ProcessUserEntry( cEntry );
						count++;
					}
					catch( SimiasShutdownException s )
					{
						log.Error( s.Message );
						throw s;
					}
					catch( LdapException e )
					{
						log.Error( "   Failed processing: " + cEntry.DN );
						log.Error( e.LdapErrorMessage );
						log.Error( e.StackTrace );
					}
					catch( Exception e )
					{
						log.Error( "   Failed processing: " + cEntry.DN );
						log.Error( e.Message );
						log.Error( e.StackTrace );
					}
				}
			}

			log.Debug( "Processed " + count.ToString() + " entries" );
		}

        /// <summary>
        /// add the ldap details into this user object
        /// </summary>
        /// <param name="entry">ldap entry</param>
		private void ProcessUserEntry( LdapEntry entry )
		{
			log.Debug( "ProcessUserEntry(" + entry.DN + ")" );

			string commonName = String.Empty;
			string firstName = String.Empty;
			string lastName = String.Empty;
			string fullName = String.Empty;
			string distinguishedName = String.Empty;
			string ldapGuid = null;

		    char[] dnDelimiters = {',', '='};
		    LdapAttribute timeStampAttr = null;
		
			bool attrError = false;
			 string FullNameDisplay = "";

			store = Store.GetStore();
			Domain domain = store.GetDomain( store.DefaultDomain );
			if ( domain != null )
			{
				FullNameDisplay = domain.UsersFullNameDisplay;
			}

			try
			{
				// get the last update time
				timeStampAttr = entry.getAttribute( "modifytimestamp" );

				ldapGuid = GetLdapGuid( entry );
				distinguishedName = entry.DN;

				// retrieve from configuration the directory attribute configured
				// for naming in Simias.  
				LdapAttribute cAttr = 
					entry.getAttribute( ldapSettings.NamingAttribute );
				if ( cAttr != null && cAttr.StringValue.Length != 0 )
				{
					commonName = cAttr.StringValue;
				}
				else
				if ( ldapSettings.NamingAttribute.ToLower() == LdapSettings.DefaultNamingAttribute.ToLower() )
				{
					// If the naming attribute is default (cn) then we want to continue
					// to work the way we previously did so we don't break any existing installs.
					//
					// If the distinguishing attribute did not exist,
					// then make the Simias username the first component
					// of the ldap DN.
					string[] components = entry.DN.Split( dnDelimiters );
					commonName = components[1];
				}

				LdapAttribute givenAttr = entry.getAttribute( "givenName" );
				if ( givenAttr != null && givenAttr.StringValue.Length != 0 )
				{
					firstName = givenAttr.StringValue as string;
				}

				LdapAttribute sirAttr = entry.getAttribute( "sn" );
				if ( sirAttr != null && sirAttr.StringValue.Length != 0 )
				{
					lastName = sirAttr.StringValue as string;
				}

				if ( firstName != null && lastName != null )
				{
					if(FullNameDisplay == "FirstNameLastName")
						fullName = firstName + " " + lastName;
					else
						fullName = lastName + " " + firstName;
				}
				else
					fullName = commonName;
			}
			catch( Exception gEx )
			{
				log.Error( gEx.Message );
				log.Error( gEx.StackTrace );

				state.ReportError( gEx.Message );
				attrError = true;
			}

			// No exception were generated gathering member info
			// so call the sync engine to process this member
			if ( attrError == false )
			{
				if ( timeStampAttr != null && timeStampAttr.StringValue.Length != 0 )
				{
					Property ts = new Property( "LdapTimeStamp", timeStampAttr.StringValue );
					ts.LocalProperty = true;
					Property[] propertyList = { ts };

					state.ProcessMember(
						ldapGuid,
						commonName,
						firstName,
						lastName,
						fullName,
						distinguishedName,
						propertyList );
				}
				else
				{
					state.ProcessMember(
						ldapGuid,
						commonName,
						firstName,
						lastName,
						fullName,
						distinguishedName,
						null );
				}
			}
		}
		
		#endregion

		#region Public Methods
		/// <summary>
		/// Call to abort an in process synchronization
		/// </summary>
		/// <returns>N/A</returns>
		public void Abort()
		{
			abort = true;
		}
		
		/// <summary>
		/// Call to inform a provider to start a synchronization cycle
		/// </summary>
		/// <returns> True - provider successfully finished a sync cycle, 
		/// False - provider failed the sync cycle
		/// </returns>
		public bool Start( Simias.IdentitySync.State State )
		{
			log.Debug( "Start called" );
			int MaxConnectRetry = 5;

			bool status = false;
			abort = false;
			try
			{
				this.state = State;

				try
				{
					ldapSettings = LdapSettings.Get( Store.StorePath );
					log.Debug( "new LdapConnection" );
					conn = new LdapConnection();

					log.Debug( "Connecting to: " + ldapSettings.Host + " on port: " + ldapSettings.Port.ToString() );
					conn.SecureSocketLayer = ldapSettings.SSL;
					for(int i =0; i < MaxConnectRetry ; i++)
					{
						try
						{
							conn.Connect( ldapSettings.Host, ldapSettings.Port );
						}
						catch( Exception ex )
						{
							log.Debug( "Failed to connect to : " + ldapSettings.Host + ", retry count: " + i.ToString() + ", Error Message : " + ex.Message );
							continue;
						}
						break;
					}

					ProxyUser proxy = new ProxyUser();

					log.Debug( "Binding as: " + proxy.UserDN );
					conn.Bind( proxy.UserDN, proxy.Password );

					ProcessSimiasAdmin( conn );
					ProcessSearchObjects( conn, ldapSettings );
				}
				catch( SimiasShutdownException s )
				{
					log.Error( s.Message );
					syncException = s;
					syncStatus = Status.SyncThreadDown;
				}
				catch( LdapException e )
				{
					log.Error( e.LdapErrorMessage );
					log.Error( e.StackTrace );
					syncException = e;
					syncStatus = 
						( conn == null )
							? Status.LdapConnectionFailure
							: Status.LdapAuthenticationFailure;

					state.ReportError( e.LdapErrorMessage );
				}
				catch(Exception e)
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
					syncException = e;
					syncStatus = Status.InternalException;

					state.ReportError( e.Message );
				}
				finally
				{
					if ( conn != null )
					{
						log.Debug( "Disconnecting Ldap connection" );
						try{
							conn.Disconnect();
						}catch{}
						conn = null;
					}
				}	
				status = true;
			}
			catch( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
				State.ReportError( e.Message );
			}
			
			return status;
		}
		#endregion
	}
}
