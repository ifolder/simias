/***********************************************************************
 *  $RCSfile: IdentityPolicy.cs,v $
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

using Simias;
using Simias.Storage;
using Simias.Policy;
using Simias.Server;

namespace iFolder.WebService
{
	/// <summary>
	/// Identity Policy
	/// </summary>
	[Serializable]
	public class IdentityPolicy 
	{
		/// <summary>
		/// The provider imports/authenticates users from
		/// an external identity source
		/// </summary>
		public bool ExternalIdentities;
		
		/// <summary>
		/// The provider can create users
		/// </summary>
		public bool CanCreate;
		
		/// <summary>
		/// The provider can delete users
		/// </summary>
		public bool CanDelete;

		/// <summary>
		/// The provider can modify user properties
		/// </summary>
		public bool CanModify;

		/// <summary>
		/// The providers friendly name
		/// </summary>
		public string Name;

		/// <summary>
		/// The providers description
		/// </summary>
		public string Description;

		/// <summary>
		/// Constructor
		/// </summary>
		public IdentityPolicy()
		{
		}

		/// <summary>
		/// Get the Identity Provider's Policy
		/// </summary>
		/// <returns>An IdentityPolicy Object</returns>
		public static IdentityPolicy GetPolicy()
		{
			IdentityPolicy idPolicy = null;
			IUserProvider provider = User.GetRegisteredProvider();
			if ( provider != null )
			{
				UserProviderCaps caps = provider.GetCapabilities();
				if ( caps != null )
				{
					idPolicy = new IdentityPolicy();
					idPolicy.CanCreate = caps.CanCreate;
					idPolicy.CanDelete = caps.CanDelete;
					idPolicy.CanModify = caps.CanModify;
					idPolicy.ExternalIdentities = caps.ExternalSync;
					idPolicy.Name = provider.Name;
					idPolicy.Description = provider.Description;
				}
			}		

			return idPolicy;
		}
	}
}	

