/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Russ Young
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

namespace Simias.Client
{

	#region SyncStatus

	/// <summary>
	/// The status codes for a sync attempt.
	/// </summary>
	public enum SyncStatus : byte
	{
		/// <summary>
		/// The operation was successful.
		/// </summary>
		Success,
		/// <summary>
		/// There was an error.
		/// </summary>
		Error,
		/// <summary> 
		/// node update was aborted due to update from other client 
		/// </summary>
		UpdateConflict,
		/// <summary> 
		/// node update was completed, but temporary file could not be moved into place
		/// </summary>
		FileNameConflict,
		/// <summary> 
		/// node update was probably unsuccessful, unhandled exception on the server 
		/// </summary>
		ServerFailure,
		/// <summary> 
		/// node update is in progress 
		/// </summary>
		InProgess,
		/// <summary>
		/// The File is in use.
		/// </summary>
		InUse,
		/// <summary>
		/// The Server is busy.
		/// </summary>
		Busy,
		/// <summary>
		/// The client passed invalid data.
		/// </summary>
		ClientError,
		/// <summary>
		/// The policy doesnot allow this file.
		/// </summary>
		Policy,
		/// <summary>
		/// Insuficient rights for the operation.
		/// </summary>
		Access,
		/// <summary>
		/// The collection is Locked.
		/// </summary>
		Locked,
		/// <summary>
		/// The disk quota doesn't allow this file.
		/// </summary>
		PolicyQuota,
		/// <summary>
		/// The size policy doesn't allow this file.
		/// </summary>
		PolicySize,
		/// <summary>
		/// The type policy doesn't allow this file.
		/// </summary>
		PolicyType,
		/// <summary>
		/// The disk is full.
		/// </summary>
		DiskFull,
		/// <summary>
		/// The Object is readonly.
		/// </summary>
		ReadOnly,
		/// <summary>
		/// Only date changed for thsi file
		/// </summary>
		OnlyDateModified,
		/// <summary>
		/// Path too long for this file on this file system.
		/// </summary>
		PathTooLong,
        /// <summary>
        /// The sharing policy doesn't allow this member.
        /// </summary>
        PolicySharing,
        /// <summary>
        /// The ownership policy doesn't allow this member.
        /// </summary>
        PolicyEncryptionEnforced,
        /// <summary>
        /// The limit policy doesn't allow this member.
        /// </summary>
	PolicyLimit,
	/// <summary>
        /// Filesystem permissions doesn't allow file IO.
        /// </summary>
	IOError
	}

	#endregion
}
