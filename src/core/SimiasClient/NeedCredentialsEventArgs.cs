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

namespace Simias.Client.Event
{
	/// <summary>
	/// Event args for a NeedCredentials Event.
	/// </summary>
	public class NeedCredentialsEventArgs : SimiasEventArgs
	{
		#region Fields
		
		string domainID;
		string collectionID;
		
		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a NeedCredentialsEventArgs for the specified domain and collection.
		/// </summary>
		/// <param name="domainID">The domain</param>
		/// <param name="collectionID">The collection.</param>
		public NeedCredentialsEventArgs(string domainID, string collectionID)
		{
			this.domainID = domainID;
			this.collectionID = collectionID;
		}

		/// <summary>
		/// Constructs a NeedCredetialsEventArgs for the specified domian.
		/// </summary>
		/// <param name="domainID"></param>
		public NeedCredentialsEventArgs(string domainID) :
			this(domainID, null)
		{
		}

		#endregion

		#region Properties
		/// <summary>
		/// Get the domain ID for which the credentials are needed.
		/// </summary>
		public string DomainID
		{
			get { return domainID; }
		}

		/// <summary>
		/// Get the collection ID for which the credentials are need.
		/// </summary>
		public string CollectionID
		{
			get { return collectionID; }
		}

		#endregion
	}
}
