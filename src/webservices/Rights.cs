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
*                 $Author: Rob
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

using Simias.Storage;

namespace iFolder.WebService
{
	/// <summary>
	/// Member Rights
	/// </summary>
	public enum Rights
	{
		/// <summary>
		/// Admin (Full Control)
		/// </summary>
		Admin,

		/// <summary>
		/// Read/Write
		/// </summary>
		ReadWrite,

		/// <summary>
		/// ReadOnly
		/// </summary>
		ReadOnly,

		/// <summary>
		/// Deny
		/// </summary>
		Deny,

		/// <summary>
		/// Unknown
		/// </summary>
		Unknown,
	}

	/// <summary>
	/// Rights Utility
	/// </summary>
	public class RightsUtility
	{
		/// <summary>
		/// Hidden Constructor
		/// </summary>
		private RightsUtility()
		{
		}

		/// <summary>
		/// Convert Rights
		/// </summary>
		/// <param name="rights"></param>
		/// <returns></returns>
		public static Rights Convert(Access.Rights rights)
		{
			Rights result;

			switch(rights)
			{
				case Access.Rights.Admin:
					result = Rights.Admin;
					break;

				case Access.Rights.ReadWrite:
					result = Rights.ReadWrite;
					break;

				case Access.Rights.ReadOnly:
					result = Rights.ReadOnly;
					break;

				case Access.Rights.Deny:
					result = Rights.Deny;
					break;

				default:
					result = Rights.Unknown;
					break;
			}

			return result;
		}

		/// <summary>
		/// Convert Rights
		/// </summary>
		/// <param name="rights"></param>
		/// <returns></returns>
		public static Access.Rights Convert(Rights rights)
		{
			Access.Rights result;

			switch(rights)
			{
				case Rights.Admin:
					result = Access.Rights.Admin;
					break;

				case Rights.ReadWrite:
					result = Access.Rights.ReadWrite;
					break;

				case Rights.ReadOnly:
					result = Access.Rights.ReadOnly;
					break;

				case Rights.Deny:
				case Rights.Unknown:
				default:
					result = Access.Rights.Deny;
					break;
			}

			return result;
		}
	}
}
