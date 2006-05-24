/***********************************************************************
 *  $RCSfile: SearchOperation.cs,v $
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Rob
 *
 ***********************************************************************/

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
