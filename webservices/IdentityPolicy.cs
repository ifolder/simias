/*****************************************************************************
* Copyright © [2007-08] Unpublished Work of Novell, Inc. All Rights Reserved.
*
* THIS IS AN UNPUBLISHED WORK OF NOVELL, INC.  IT CONTAINS NOVELL'S CONFIDENTIAL, 
* PROPRIETARY, AND TRADE SECRET INFORMATION.	NOVELL RESTRICTS THIS WORK TO 
* NOVELL EMPLOYEES WHO NEED THE WORK TO PERFORM THEIR ASSIGNMENTS AND TO 
* THIRD PARTIES AUTHORIZED BY NOVELL IN WRITING.  THIS WORK MAY NOT BE USED, 
* COPIED, DISTRIBUTED, DISCLOSED, ADAPTED, PERFORMED, DISPLAYED, COLLECTED,
* COMPILED, OR LINKED WITHOUT NOVELL'S PRIOR WRITTEN CONSENT.  USE OR 
* EXPLOITATION OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE 
* PERPETRATOR TO CRIMINAL AND  CIVIL LIABILITY.
*
* Novell is the copyright owner of this file.  Novell may have released an earlier version of this
* file, also owned by Novell, under the GNU General Public License version 2 as part of Novell's 
* iFolder Project; however, Novell is not releasing this file under the GPL.
*
*-----------------------------------------------------------------------------
*
*                 Novell iFolder Enterprise
*
*-----------------------------------------------------------------------------
*
*                 $Author: Brady Anderson <banderso@novell.com>
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
					if(Simias.Service.Manager.LdapServiceEnabled == true)
					{
						idPolicy.CanCreate = false;
						idPolicy.CanDelete = false;
						idPolicy.CanModify = false;
						idPolicy.ExternalIdentities = true;
						idPolicy.Name = provider.Name;
						idPolicy.Description = provider.Description;
					}
					else
					{
						idPolicy.CanCreate = caps.CanCreate;
						idPolicy.CanDelete = caps.CanDelete;
						idPolicy.CanModify = caps.CanModify;
						idPolicy.ExternalIdentities = caps.ExternalSync;
						idPolicy.Name = provider.Name;
						idPolicy.Description = provider.Description;
					}
				}
			}		

			return idPolicy;
		}
	}
}	

