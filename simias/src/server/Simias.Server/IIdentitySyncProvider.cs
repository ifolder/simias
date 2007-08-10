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
 |  Author: Brady Anderson <banderso@novell.com>
 |***************************************************************************/

using System;

namespace Simias
{
	/// <summary>
	/// Interface for an external identity sync provider
	/// </summary>
	public interface IIdentitySyncProvider
	{
		#region Properties
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the description of the provider.
		/// </summary>
		string Description { get; }
		#endregion

		#region Public Methods
		/// <summary>
		/// Call to abort an in process synchronization
		/// </summary>
		/// <returns>N/A</returns>
		void Abort();
		
		/// <summary>
		/// Call to inform a provider to start a synchronization cycle
		/// </summary>
		/// <returns>True - provider successfully started a sync cycle, False - provider could
		/// not start the sync cycle.</returns>
		bool Start( Simias.IdentitySync.State State );
		#endregion
	}
}	
