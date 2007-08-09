/****************************************************************************
|
| Copyright (c) 2007 Novell, Inc.
| All Rights Reserved.
|
| This program is free software; you can redistribute it and/or
| modify it under the terms of version 2 of the GNU General Public License as
| published by the Free Software Foundation.
|
| This program is distributed in the hope that it will be useful,
| but WITHOUT ANY WARRANTY; without even the implied warranty of
| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
| GNU General Public License for more details.
|
| You should have received a copy of the GNU General Public License
| along with this program; if not, contact Novell, Inc.
|
| To contact Novell about this file by physical or electronic mail,
| you may find current contact information at www.novell.com 
|
| Author: Rob 
|***************************************************************************/

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
