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


using System;

namespace Simias.Storage
{
	/// <summary>
	/// Summary description for BaseSchema.
	/// </summary>
	public class BaseSchema
	{
		/// <summary>
		/// The name property for an object.
		/// </summary>
		public const string ObjectName = "Display Name";
		/// <summary>
		/// The type property for an object.
		/// </summary>
		public const string ObjectType = "Object Type";
		/// <summary>
		/// The id property for an object.
		/// </summary>
		public const string ObjectId = "GUID";

		/// <summary>
		/// Property that describes the collection that a node belongs to.
		/// </summary>
		public const string CollectionId = "CollectionId";
	}
}
