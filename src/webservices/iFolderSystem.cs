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
using Simias.Server;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder System
	/// </summary>
	[Serializable]
	public class iFolderSystem
	{
		/// <summary>
		/// System ID
		/// </summary>
		public string ID;

		/// <summary>
		/// System Name
		/// </summary>
		public string Name;

		/// <summary>
		/// System Version
		/// </summary>
		public string Version;

		/// <summary>
		/// System Description
		/// </summary>
		public string Description;

		/// <summary>
		/// System Full Name Display Setting 
		/// </summary>
		public string UsersFullNameDisplay;

		/// <summary>
		/// Identifier for the System Report iFolder where iFolder 
		/// reports will be generated.
		/// </summary>
		public string ReportiFolderID;

		/// <summary>
		/// Report collection name for the System Report iFolder.
		/// </summary>
		public string ReportiFolderName;

		/// <summary>
		/// Directory path where iFolder reports will be generated if
		/// not specified to use the report iFolder.
		/// </summary>
		public string ReportPath;

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderSystem()
		{
		}

		/// <summary>
		/// Get the iFolder System Information Object
		/// </summary>
		/// <returns>An iFolderSystem Object</returns>
		public static iFolderSystem GetSystem()
		{
			iFolderSystem system = new iFolderSystem();

			Store store = Store.GetStore();
			
			Domain domain = store.GetDomain(store.DefaultDomain);

			system.ID = domain.ID;
			system.Name = domain.Name;
			system.Version = domain.DomainVersion.ToString();
			system.Description = domain.Description;
			system.UsersFullNameDisplay = domain.UsersFullNameDisplay;

			system.ReportPath = Report.ReportPath;
			system.ReportiFolderID = Report.ReportCollectionID;
			system.ReportiFolderName = Report.ReportCollectionName;

            return system;
		}

		/// <summary>
		/// Sets iFolder system information
		/// </summary>
		/// <param name="system">An iFolderSystem Object</param>
		public static void SetSystem(iFolderSystem system)
		{
			Store store = Store.GetStore();
			
			Domain domain = store.GetDomain(store.DefaultDomain);

			domain.Name = system.Name;
			domain.Description = system.Description;
			domain.UsersFullNameDisplay = system.UsersFullNameDisplay;

			domain.Commit();
		}
    }
}
