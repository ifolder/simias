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
using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using Simias;
using Simias.Storage;
using Simias.Security;

namespace Novell.iFolder.Enterprise
{
	/// <summary>
	/// iFolder Reports Web Service
	/// </summary>
	[WebService(
		Namespace="http://novell.com/ifolder/reports/",
		Name="iFolderReports",
		Description="iFolder Reports Web Service")]
	public class iFolderReports : WebService
	{
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderReports()
		{
		}

		#endregion

		#region System

		/// <summary>
		/// Generate the iFolder Reports
		/// </summary>
		[WebMethod(
			 Description="Generate the iFolder Reports",
			 EnableSession=true)]
		public void Generate()
		{
			Authorize();

			DateTime timestamp = DateTime.Now;
			Configuration config = Configuration.GetServerBootStrapConfiguration();

			// report path
			string path = Path.Combine(config.StorePathRoot, "reports");
			
			if (!Directory.Exists(path))
			{
				// create report path
				Directory.CreateDirectory(path);
			}

			// iFolder report
			iFolderReport.Generate(path, timestamp);
		}

		#endregion

		#region Utility

		/// <summary>
		/// Authorize the Current Principal
		/// </summary>
		private void Authorize()
		{
			// check authentication
			string accessID = Context.User.Identity.Name;

			if ((accessID == null) || (accessID.Length == 0))
			{
				throw new Exception("No Access ID");
			}

			// check for an admin ID cache
			string adminID = (string)Session["AdminID"];

			// check the ID cache
			if ((adminID == null) || (adminID.Length == 0) || (!adminID.Equals(accessID)))
			{
				if (IsAdministrator(accessID))
				{
					// authorized
					Session["AdminID"] = accessID;
				}
				else
				{
					// unauthroized
					throw new Exception("Not An Administrator");
				}
			}
		}

		/// <summary>
		/// Is the user an administrator?
		/// </summary>
		/// <param name="userID"></param>
		/// <returns></returns>
		private bool IsAdministrator(string userID)
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomain(store.DefaultDomain);

			Member member = domain.GetMemberByID(userID);

			return (member.Rights == Access.Rights.Admin);
		}

		#endregion
	}
}
