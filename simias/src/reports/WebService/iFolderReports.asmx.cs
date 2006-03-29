/***********************************************************************
 *  $RCSfile: iFolderReports.asmx.cs,v $
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
