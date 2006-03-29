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
using System.Reflection;
using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.Server;

namespace Simias.SimpleServer
{
	/// <summary>
	/// Implementation of the IUserProvider Service for
	/// Simple Server.
	///
	/// Simple Server does not allow creation and deletion
	/// of user's via the IUserProvider interface so those
	/// methods will return false.
	/// </summary>
	public class User : Simias.Server.IUserProvider
	{
		#region Fields
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		/// <summary>
		/// String used to identify domain provider.
		/// </summary>
		static private string providerName = "Simple Server User Provider";
		static private string providerDescription = "Simple Server provider";
		
		static private string missingDomainMessage = "Enterprise domain does not exist!";
		static internal string pwdProperty = "SS:PWD";

		// Frequently used Simias types
		private Store store = null;
		private Simias.Storage.Domain domain = null;
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		public string Name { get { return providerName; } }

		/// <summary>
		/// Gets the description of the provider.
		/// </summary>
		public string Description { get { return providerDescription; } }
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of this object.
		/// </summary>
		public User()
		{
			store = Store.GetStore();
			if ( store.DefaultDomain == null )
			{
				throw new SimiasException( User.missingDomainMessage );
			}
			
			domain = store.GetDomain( store.DefaultDomain );
			if ( domain == null )
			{
				throw new SimiasException( User.missingDomainMessage );
			}
			
			if ( domain.IsType( "Enterprise" ) == false )
			{
				throw new SimiasException( User.missingDomainMessage );
			}
			
			
			// Make sure the password is set on the admin
			SetAdminPassword();
		}
		#endregion
		
		#region Private Methods
		
		//
		// When the Enterprise Server Domain is instantiated in Simias.Server
		// the domain owner is determined via the AdminName in Simias.config.
		// Simple Server determines the domain owner via an attribute in
		// the SimpleServer.xml file so this method will actually change
		// ownership of the domain to the one specified in the xml file
		// unless they happen to be the same.
		//
		private bool SetAdminPassword()
		{
			bool status = false;

			// Domain administrator is determined by "owner" attribute
			// in the SimpleServer.xml document
			
			string identityDocumentPath = "../../etc/SimpleServer.xml";
			string owner = null;
			string ownerPassword = null;

			try
			{
				// Load the SimpleServer domain and memberlist XML file.
				XmlDocument serverDoc = new XmlDocument();
				serverDoc.Load( identityDocumentPath );
				XmlElement domainElement = serverDoc.DocumentElement;
				XmlAttribute attr;
				
				for ( int i = 0; i < domainElement.ChildNodes.Count; i++ )
				{
					attr = domainElement.ChildNodes[i].Attributes[ "Owner" ];
					if (attr != null)
					{
						XmlNode cNode = domainElement.ChildNodes[i];
						owner = cNode.Attributes[ "Name" ].Value;
						ownerPassword = cNode.Attributes[ "Password" ].Value;
						break;
					}	
				}
			}
			catch(Exception e)
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
			
			if ( owner != null && ownerPassword != null )
			{
				Member ownerMember = domain.Owner;
				if ( ownerMember.Name != owner )
				{
					// New owner must first be a member before ownership
					// can be transfered
					Member adminMember =
						new Member(
							owner,
							Guid.NewGuid().ToString(),
							Simias.Storage.Access.Rights.ReadOnly,
							null,
							null );
							
					domain.Commit( adminMember );		
					domain.Commit( domain.ChangeOwner( adminMember, Simias.Storage.Access.Rights.Admin ) );

					// Now remove the old member
					domain.Commit( domain.Delete( domain.Refresh( ownerMember ) ) );
					ownerMember = adminMember;
				}
				
				Property pwd = 
					ownerMember.Properties.GetSingleProperty( User.pwdProperty );
				if ( pwd == null || pwd.Value == null )
				{
					pwd = new Property( User.pwdProperty, HashPassword( ownerPassword ) );
					ownerMember.Properties.ModifyProperty( pwd );

					domain.Commit( ownerMember );
					status = true;
				}
			}
			
			return status;
		}		
		#endregion
		
		#region Internal Methods
		static internal string HashPassword( string password )
		{
			UTF8Encoding utf8Encoding = new UTF8Encoding();
			MD5CryptoServiceProvider md5Service = new MD5CryptoServiceProvider();
		
			byte[] bytes = new Byte[ utf8Encoding.GetByteCount( password ) ];
			bytes = utf8Encoding.GetBytes( password );
			return Convert.ToBase64String( md5Service.ComputeHash( bytes ) );
		}
		#endregion
		
		#region Public Methods
		/// <summary>
		/// Method to create a user/identity in the external user database.
		/// Some external systems may not allow for creation of new users.
		/// </summary>
		/// <param name="Guid" mandatory="false">Guid associated to the user.</param>
		/// <param name="Username" mandatory="true">User or short name for the new user.</param>
		/// <param name="Password" mandatory="true">Password associated to the user.</param>
		/// <param name="Firstname" mandatory="false">First or given name associated to the user.</param>
		/// <param name="Lastname" mandatory="false">Last or family name associated to the user.</param>
		/// <param name="Fullname" mandatory="false">Full or complete name associated to the user.</param>
		/// <returns>RegistrationStatus</returns>
		/// Note: Method assumes the mandatory arguments: Username, Password have already 
		/// been validated.  Also assumes the username does NOT exist in the domain
		public
		Simias.Server.RegistrationInfo
		Create(
			string Userguid,
			string Username,
			string Password,
			string Firstname,
			string Lastname,
			string Fullname )
		{
			RegistrationInfo info = new RegistrationInfo( RegistrationStatus.MethodNotSupported );
			info.Message = "Simple Server does not support creating users through the registration class";
			return info;
		}
		
		/// <summary>
		/// Method to delete a user from the external identity/user database.
		/// Some external systems may not allow deletion of users.
		/// </summary>
		/// <param name="Username">Name of the user to delete from the external system.</param>
		/// <returns>true - successful  false - failed</returns>
		public bool Delete( string Username )
		{
			// Simple Server does not support delete through the registration APIs
			return false;
		}
		
		/// <summary>
		/// Method to verify a user's password
		/// </summary>
		/// <param name="Username">User to verify the password against</param>
		/// <param name="Password">Password to verify</param>
		/// <returns>true - Valid password, false Invalid password</returns>
		public bool VerifyPassword( string Username, string Password )
		{
			bool status;
			
			try
			{
				Member member = domain.GetMemberByName( Username );
				if ( member != null )
				{
					Property pwd = member.Properties.GetSingleProperty( User.pwdProperty );
					if ( pwd != null && ( pwd.Value as string == HashPassword( Password ) ) )
					{
						status = true;
					}
					else
					{
						status = false;
					}
				}
				else
				{
					status = false;
				}
			}
			catch( Exception e1 )
			{
				log.Error( e1.Message );
				log.Error( e1.StackTrace );
				status = false;
			}			
			
			return status;
		}
		#endregion
	}	
}
