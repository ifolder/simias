/***********************************************************************
 *  $RCSfile: ProxyPassword.cs,v $
 *
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Brady Anderson (banderso@novell.com)
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Simias;
using Simias.Storage;

namespace Simias.LdapProvider
{
	public class ProxyUser
	{
		public readonly string ProxyPasswordFile = ".simias.ppf";
		private readonly string LdapSection = "LdapAuthentication";
		private readonly string ProxyDNKey = "ProxyDN";
		private Store store = Store.GetStore();

		#region Properties
		/// <summary>
		/// Retrieve the proxy user's distinguished name
		/// The dn always exists in the Simias configuration file
		/// </summary>
		public string UserDN
		{
			get
			{ 
				string userDN = Store.Config.Get( LdapSection, ProxyDNKey );
				return ( userDN != null ) ? userDN : String.Empty;
			}
		}

		/// <summary>
		/// Retrieve the proxy user's password
		/// The proxy user's password is first retrieved from
		/// the configuration file and then stored encrypted in the
		/// enterprise domain.  If the proxy user password is changed
		/// the Simias management utility simply needs to reset the
		/// password back into the configuration file.
		/// </summary>
		public string Password
		{
			get
			{
				string proxyPassword = null;

				try
				{
					string ppfPath = Path.Combine( Store.StorePath, ProxyPasswordFile );
					if ( !File.Exists( ppfPath ) )
					{
						proxyPassword = this.GetProxyPasswordFromStore();
					}
					else
					{
						using ( StreamReader sr = File.OpenText( ppfPath ) )
						{
							// Password should be a single line of text.
							proxyPassword = sr.ReadLine();
						}

						// See if the password was successfully read from the file.
						if ( proxyPassword != null )
						{
							// Save the password to the store.
							if ( SaveProxyPasswordToStore( proxyPassword ) == true )
							{
								// Delete the password file.
								File.Delete( ppfPath );
							}
						}
					}
				}
				catch{}
				return( proxyPassword );
			}

			set
			{
				string ppfPath = Path.Combine( Store.StorePath, ProxyPasswordFile );

				// Save the password to the store.
				if ( SaveProxyPasswordToStore( value ) )
				{
					// Delete the password file.
					if ( File.Exists( ppfPath ) )
					{
						File.Delete( ppfPath );
					}
				}
				else
				{
					using ( StreamWriter sw = File.CreateText( ppfPath ) )
					{
						sw.WriteLine( value );
					}
				}
			}
		}
		#endregion

		#region Private Methods
		private string GetProxyPasswordFromStore()
		{
			string password = null;

			try
			{
				Domain domain = store.GetDomain( store.DefaultDomain );
				string encodedCypher = domain.Properties.GetSingleProperty( "ProxyPassword" ).ToString();
				byte[] cypher = Convert.FromBase64String( encodedCypher );
				RSACryptoServiceProvider credential = store.CurrentUser.Credential;
				password = new UTF8Encoding().GetString( credential.Decrypt( cypher, false ) );
			}
			catch{}
			return password;
		}

		private bool SaveProxyPasswordToStore( string password )
		{
			try
			{
				Domain domain = store.GetDomain( store.DefaultDomain );

				RSACryptoServiceProvider credential = 
					Store.GetStore().CurrentUser.Credential;

				byte[] cypher = credential.Encrypt( new UTF8Encoding().GetBytes( password ), false );
				string encryptedPassword = Convert.ToBase64String( cypher );

				Property proxyPwd = new Property( "ProxyPassword", encryptedPassword );
				proxyPwd.LocalProperty = true;
				domain.Properties.ModifyProperty( proxyPwd );
				domain.Commit( domain );
				return true;
			}
			catch{}
			return false;
		}
		#endregion
	}
}
