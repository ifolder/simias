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

			domain.Commit();
		}
    }
}
