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
		private bool errorDuringSync;
		private Exception syncException;
		private DateTime lastSyncTime;

		private Property syncGUID;
		private Store store = null;
		private Domain domain = null;
		
		private LdapConnection conn = null;
		
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
		
		private void ProcessSearchObjects( LdapConnection conn, LdapSettings settings )
		{
			foreach ( string searchContext in settings.SearchContexts )
			{
				string[] searchAttributes = {
					"objectClass",
					"loginGraceLimit",
					"loginGraceRemaining" };

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
					log.Error( s.Message );
					throw s;
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
			}
		}
		
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

		// If the configured Simias Admin is different than the SimiasAdmin
		// identified in the store, make all the changes necessary to
		// make the configured admin the store admin.
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

		// The SimiasAdmin is processed differently than normal simias users because
		// the account is aleady created in the Simias store before LdapSync runs
		// so the GUID has already been created.  The SimiasAdmin must always exist in the
		// store and the DN entry in the store must be correct with the Distinguished
		// Name in the directory.  LdapSync counts on the AdminDN entry in Simias.config
		// to be updated if the admin is moved in the directory.
		private void ProcessSimiasAdmin( LdapConnection conn )
		{
			// Since the first version of the iFolder 3.0 only
			// exposes a username, firstname, lastname and full
			// name, we'll limit the scope of the search
			string[] searchAttributes = {	
						"modifytimestamp",
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
	
								// If we're tracking by ldap see if the common name
								// has changed
								LdapAttribute cnAttr = entry.getAttribute( "cn" );
								if ( cnAttr != null && cnAttr.StringValue.Length != 0 )
								{
									if ( cnAttr.StringValue != cMember.Name )
									{
										cMember.Name = cnAttr.StringValue;
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
			cMember.Properties.ModifyProperty( syncGUID );
			domain.Commit( cMember );
		}

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

		private void ProcessUserEntry( LdapEntry entry )
		{
			log.Debug( "ProcessUserEntry(" + entry.DN + ")" );

		    char[] dnDelimiters = {',', '='};
		    LdapAttribute timeStampAttr = null;

			try
		    {
				timeStampAttr = entry.getAttribute( "modifytimestamp" );
			}
		    catch {}

			//
		    // Check if this member already exists
			// Note: The common name or username in Simias will
			// be set to the configured naming attribute in
			// the ldap directory, which by default is cn
		    //

			string commonName = null;
			string ldapGuid = null;
			Member cMember = null;

			try
			{
				ldapGuid = GetLdapGuid( entry );
				if ( ldapGuid !=  null )
				{
					cMember = domain.GetMemberByID( ldapGuid );
					if ( cMember != null )
					{
						log.Debug( "Found member by GUID" );
						commonName = cMember.Name;
					}
				}
			}
			catch( Exception gEx )
			{
				log.Error( gEx.Message );
				log.Error( gEx.StackTrace );
			}

			// Did we lookup via Ldap GUID?
			if ( cMember == null )
			{
				try
				{
					LdapAttribute cAttr = 
						entry.getAttribute( ldapSettings.NamingAttribute );
					if ( cAttr != null && cAttr.StringValue.Length != 0 )
					{
						commonName = cAttr.StringValue;
						cMember = domain.GetMemberByName( cAttr.StringValue );
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
						cMember = domain.GetMemberByName( commonName );
					}

					if ( cMember != null )
					{
						//
						// Check if the cMember.DN and the entry.DN are different
						//
						if (entry.DN != (string) cMember.Properties.GetSingleProperty("DN").Value)
						{
							log.Debug( "entry.DN != (string) cMember.Properties.GetSingleProperty(\"DN\").Value" );
							log.Debug( "Duplicate Member.Name \"" + cMember.Name + "\"" );
							log.Debug( "Skipped: " + entry.DN );
							return;
						}
					}
				}
				catch {}
			}

		    if ( cMember != null )
			{
				//
				// check if the ldap object's time stamp has changed
				//

				try
				{
					Property pStamp = null;
					try
					{
						pStamp = cMember.Properties.GetSingleProperty( "LdapTimeStamp" );
						pStamp.LocalProperty = true;
					}
					catch
					{
						pStamp = new Property( "LdapTimeStamp", "ABA" );
						pStamp.LocalProperty = true;
					}

					if ( (string) pStamp.Value != timeStampAttr.StringValue )
					{
						// The time stamp changed let's look at first and
						// last name

						try
						{
							bool changed = false;

							// If we're tracking by ldap see if the common name
							// has changed
							if ( ldapGuid != null )
							{
								LdapAttribute cnAttr = 
									entry.getAttribute( ldapSettings.NamingAttribute );
								if ( cnAttr != null && cnAttr.StringValue.Length != 0 )
								{
									if ( cnAttr.StringValue != cMember.Name )
									{
										commonName = cnAttr.StringValue;
										cMember.Name = cnAttr.StringValue;
									}
								}
								else
								{
									string[] components = entry.DN.Split( dnDelimiters );
									commonName = components[1];
									if ( commonName != cMember.Name )
									{
										cMember.Name = commonName;
									}
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

							// Did the distinguished name change?
							Property dnProp = cMember.Properties.GetSingleProperty( "DN" );
							if ( dnProp != null && dnProp.ToString() != entry.DN )
							{
								dnProp.Value = entry.DN;
								cMember.Properties.ModifyProperty( "DN", dnProp );
								changed = true;
							}

							if ( changed == true &&
								cMember.Given != null &&
								cMember.Given != "" &&
								cMember.Family != null &&
								cMember.Family != "" )
							{
								cMember.FN = cMember.Given + " " + cMember.Family;
							}
						}
						catch {}

						pStamp.Value = timeStampAttr.StringValue;
						cMember.Properties.ModifyProperty( pStamp );
					}
				}
				catch{}

				cMember.Properties.ModifyProperty( syncGUID );
				domain.Commit(cMember);
			}
			else
			if ( commonName != null && commonName != "" )
			{
				log.Debug( "Processing new member" );
				try
				{
					string givenName = null;
					string familyName = null;
	
					try
					{
						LdapAttribute givenAttr = entry.getAttribute( "givenName" );
						if ( givenAttr != null && givenAttr.StringValue.Length != 0 )
						{
							givenName = givenAttr.StringValue;
						}

						LdapAttribute sirAttr = entry.getAttribute( "sn" );
						if ( sirAttr != null && sirAttr.StringValue.Length != 0 )
						{
							familyName = sirAttr.StringValue;
						}
					}
					catch{}

					// Create a new member 
					cMember = 
						new Member(
							commonName,
							(ldapGuid != null) ? ldapGuid : Guid.NewGuid().ToString(), 
							Simias.Storage.Access.Rights.ReadOnly,
							givenName,
							familyName );

					// Set the local property sync guid
					cMember.Properties.ModifyProperty( syncGUID );

					// Add the DN to the member node
					Property dn = new Property( "DN", entry.DN );
					dn.LocalProperty = true;
					cMember.Properties.ModifyProperty( dn );
				}
				catch
				{
					return;
				}

				//
				// Save the Ldap object's time stamp in the member's node
				// so we'll know in the future if the ldap object changed.
				//

				if ( timeStampAttr != null && timeStampAttr.StringValue.Length != 0 )
				{
					Property ltsP = new Property( "LdapTimeStamp", timeStampAttr.StringValue );
					ltsP.LocalProperty = true;
					cMember.Properties.ModifyProperty( ltsP );
				}

				domain.Commit( cMember );
			}
			else
			{
				log.Debug( "Ldap entry did not contain the naming attribute specified - entry excluded" );
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

			bool	 status = false;
			string member;
			string firstName;
			string lastName;
			string fullName;

			abort = false;
			try
			{
				//
				// Create a sync iteration guid which will be stamped
				// in all matching objects as a local property
				//

				syncGUID = new Property("SyncGuid", Guid.NewGuid().ToString());
				syncGUID.LocalProperty = true;
				errorDuringSync = false;

				try
				{	
					log.Debug( "new LdapConnection" );
					conn = new LdapConnection();

					log.Debug( "Connecting to: " + ldapSettings.Host + " on port: " + ldapSettings.Port.ToString() );
					conn.SecureSocketLayer = ldapSettings.SSL;
					conn.Connect( ldapSettings.Host, ldapSettings.Port );

					ProxyUser proxy = new ProxyUser();

					log.Debug( "Binding as: " + proxy.UserDN );
					conn.Bind( proxy.UserDN, proxy.Password );

					ProcessSimiasAdmin( conn );
					ProcessSearchObjects( conn, ldapSettings );
				}
				catch( SimiasShutdownException s )
				{
					log.Error( s.Message );
					errorDuringSync = true;
					syncException = s;
					syncStatus = Status.SyncThreadDown;
				}
				catch( LdapException e )
				{
					log.Error( e.LdapErrorMessage );
					log.Error( e.StackTrace );
					errorDuringSync = true;
					syncException = e;
					syncStatus = 
						( conn == null )
							? Status.LdapConnectionFailure
							: Status.LdapAuthenticationFailure;
				}
				catch(Exception e)
				{
					log.Error( e.Message );
					log.Error( e.StackTrace );
					errorDuringSync = true;
					syncException = e;
					syncStatus = Status.InternalException;
				}
				finally
				{
					if ( conn != null )
					{
						log.Debug( "Disconnecting Ldap connection" );
						conn.Disconnect();
						conn = null;
					}
				}	
				/*
				State.ProcessMember(
					null,
					member,
					firstName,
					lastName,
					fullName,
					member,
					propertyList );
				*/
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
