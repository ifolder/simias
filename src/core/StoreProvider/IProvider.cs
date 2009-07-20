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
using System.Xml;

namespace Simias.Storage.Provider
{
	/// <summary>
	/// Collection Store Provider interface.
	/// </summary>
	public interface IProvider : IDisposable
	{
		#region Store Calls

		/// <summary>
		/// Called to Create a new Collection Store at the specified location.
		/// </summary>
		void CreateStore();

		/// <summary>
		/// Called to Delete the opened CollectionStore.
		/// </summary>
		void DeleteStore();

		/// <summary>
		/// Called to Open an existing Collection store at the specified location.
		/// </summary>
		void OpenStore();
		
		#endregion

		#region ContainerCalls Calls.
		
		/// <summary>
		/// Called to create a container to hold records.  This call does not need to
		/// be made.  If a record is created and the container does not exist. it will be created.
		/// </summary>
		/// <param name="name">The name of the container.</param>
		void CreateContainer(string name);

		/// <summary>
		/// Called to Delete a record container.  
		/// This call is deep (all records contained are deleted).
		/// </summary>
		/// <param name="name">The name of the container.</param>
		void DeleteContainer(string name);

		#endregion

		#region Record Calls.

		/// <summary>
		/// Used to Create, Modify or Delete records from the store.
		/// </summary>
		/// <param name="container">The container that the commit applies to.</param>
		/// <param name="createDoc">The records to create or modify.</param>
		/// <param name="deleteDoc">The records to delete.</param>
		void CommitRecords(string container, XmlDocument createDoc, XmlDocument deleteDoc);
		
		/// <summary>
		/// Called to get a Record.  The record is returned as an XML string representation.  
		/// </summary>
		/// <param name="recordId">string that contains the ID of the Record to retrieve</param>
		/// <param name="container">The container that holds the record.</param>
		/// <returns>XMLDocument describing the Record</returns>
		XmlDocument GetRecord(string recordId, string container);

		/// <summary>
		/// Called to get a shallow record.
		/// </summary>
		/// <param name="recordId">The record id to get.</param>
		/// <returns>XmlDocument describing the shallow Record.</returns>
		XmlDocument GetShallowRecord(string recordId);

		#endregion
		
		#region Query Calls

		/// <summary>
		/// Method used to search for Records using the specified query.
		/// </summary>
		/// <param name="query">Query used for this search</param>
		/// <returns></returns>
		IResultSet Search(Query query);

		/// <summary>
		/// Method used to search for Records using the specified multiple queries.
		/// </summary>
		/// <param name="query">Queries used for this search</param>
		/// <returns></returns>
		IResultSet MQSearch(Query[] query);

		#endregion


	}
}
