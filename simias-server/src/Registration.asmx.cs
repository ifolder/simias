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

namespace Simias.Server
{
	public enum RegistrationStatus
	{
		UserCreated = 0,
		UserExists,
		InvalidParameters,
		InvalidDomain,
		UsernamePolicyException,
		PasswordPolicyException,
		InternalException
	}
	
	/// <summary>
	/// Class that represents the current state and configuration
	/// of the synchronization service.
	/// </summary>
	[ Serializable ]
	public class RegistrationInfo
	{
		/// <summary>
		/// Status result from a create or delete
		/// method
		/// </summary>
		public RegistrationStatus Status;
		
		/// <summary>
		/// Message returned from the CreateUser method.
		/// </summary>
		public string Message;
		
		/// <summary>
		/// Guid assigned to the user.
		/// Not valid if the registration method fails.
		/// </summary>
		public string UserGuid;
		
		/// <summary>
		/// If the Registration.CreateUser method fails with a
		/// UserExists status, the provider MAY return a list of
		/// suggested names the caller could try.
		/// </summary>
		public string[] SuggestedNames;
	}
	
	/// <summary>
	/// Registration
	/// Web service methods to manage the Identity Sync Service
	/// </summary>
	[WebService(
	 Namespace="http://novell.com/simias-server/registration",
	 Name="User Registration",
	 Description="Web Service providing self provisioning/registration for Simias users.")]
	public class Registration : System.Web.Services.WebService
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
		public Registration()
		{
			store = Store.GetStore();
		}
		
		/// <summary>
		/// Method to get the domain's public key
		/// </summary>
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public
		string
		GetPublicKey()
		{
			Simias.Storage.Domain domain = store.GetDomain( store.LocalDomain );
			RSACryptoServiceProvider pubKey = domain.Owner.PublicKey;
			
			log.Debug( "Public Key: " + pubKey.ToString() );
			log.Debug( "Public Key (XML): " + pubKey.ToXmlString( false ) );
			
			return pubKey.ToXmlString( false );
		}
		
		/// <summary>
		/// Method to add/create a new user in the system.
		/// <param>Username (mandatory) short name of the user</param>
		/// <param>Encrypted true if the Password is encrypted false password is in the clear</param>
		/// <param>Password (mandatory)
		/// <param>UserGuid (optional) caller can specify the guid for the user</param>
		/// <param>FirstName (optional) first/given name of the user</param>
		/// <param>LastName (optional) last/family name of the user</param>
		/// <param>FullName (optional) Fullname of the user</param>
		/// <param>DistinguishedName (optional) usually the distinguished name from an external identity store</param>
		/// If the FirstName and LastName are specified but the FullName is null, FullName is
		/// autocreated using: FirstName + " " + LastName
		/// </summary>
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public
		RegistrationInfo
		CreateUser(
			string 	Username,
			bool	Encrypted,
			string 	Password,
			string 	UserGuid,
			string 	FirstName,
			string 	LastName,
			string 	FullName,
			string	DistinguishedName)
		{
			Member member = null;
			RegistrationInfo info = new RegistrationInfo();
			
			if ( Username == null || Username == "" || Password == null )
			{
				info.Status = RegistrationStatus.InvalidParameters;
				info.Message = "Missing mandatory parameters";
				log.Info( "called with missing mandatory parameters" );
				return info;
			}
			else
			{
				try
				{
					Simias.Storage.Domain domain = store.GetDomain( store.DefaultDomain );
					if ( domain != null )
					{
						member = domain.GetMemberByName( Username );
						if ( member == null )
						{
							string guid;
							if ( UserGuid != null && UserGuid != "" )
							{
								guid = UserGuid;
							}
							else
							{
								guid = Guid.NewGuid().ToString();
							}
							
							log.Debug( "Creating member: {0}  guid: {1}", Username, guid );
							// Add the new user to the domain
							member = 
								new Member(
									Username,
									guid, 
									Access.Rights.ReadOnly,
									FirstName,
									LastName );

							// Set the admin hashed password
							// FIXME:: This needs to go through the provision framework
							// when it becomes available
							Property pwd = new Property( "SS:PWD",	Password );
							pwd.LocalProperty = true;
							member.Properties.ModifyProperty( pwd );
							
							member.FN = ( FullName != null ) ? FullName : FirstName + " " + LastName;

							if ( DistinguishedName != null && DistinguishedName != "" )
							{
								Property dnProp = new Property( "DN", DistinguishedName );
								member.Properties.ModifyProperty( dnProp );
							}
							
							domain.Commit( member );
							
							info.Status = RegistrationStatus.UserCreated;
							info.Message = "Successful";
							info.UserGuid = guid;
						}
						else
						{
							info.Status = RegistrationStatus.UserExists;
							info.Message = "Specified user already exists!";
						}
					}
					else
					{
						info.Status = RegistrationStatus.InvalidDomain;
						info.Message = "A default server domain does not exist";
					}
				}
				catch( Exception e1 )
				{
					info.Status = RegistrationStatus.InternalException;
					info.Message = e1.Message;
				}			
			}
			
			return info;
		}

		/// <summary>
		/// Method to delete an existing user from the system.
		/// Only the user or the system administrator can delete
		/// a user from the system.
		/// </summary>
		///
		[WebMethod( EnableSession = true )]
		[SoapDocumentMethod]
		public bool DeleteUser( string UserGuid )
		{
			return true;
		}
	}
}
