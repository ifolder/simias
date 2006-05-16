/***********************************************************************
 *  $RCSfile$ Report.cs
 *
 *  Copyright (C) 2006 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/
using System;
using System.IO;

using Simias;
using Simias.Storage;

namespace Simias.Server
{
	/// <summary>
	/// Implements the report collection. This is the collection intended
	/// for reports that are synced between master and slave servers and
	/// clients.
	/// </summary>
	public class Report
	{
		#region Class Members

		/// <summary>
		/// Well-known identifier for the report collection.
		/// </summary>
		private static string reportCollectionID = "3E49B2F8-C13E-45d1-B69E-811521411ABA";

		/// <summary>
		/// Well-known name for the report collection.
		/// </summary>
		private static string reportCollectionName = "Simias Reports";

		#endregion

		#region Properties

		/// <summary>
		/// Returns the well-known report collection identifier.
		/// </summary>
		public string ReportCollectionID
		{
			get { return reportCollectionID; }
		}

		/// <summary>
		/// Returns the well-known report collection name.
		/// </summary>
		public string ReportCollectionName
		{
			get { return reportCollectionName; }
		}

		/// <summary>
		/// Returns the absolute path to the report directory.
		/// </summary>
		public string ReportPath
		{
			get { return Path.Combine( Store.StorePath, "report" ); }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public Report()
		{
		}

		#endregion

		#region Private Methods
		#endregion

		#region Internal Methods

		/// <summary>
		/// Creates the system report collection.
		/// </summary>
		/// <param name="store">Store object</param>
		/// <param name="domain">The domain to which this collection will belong.</param>
		/// <returns>A collection object that represents the report collection.</returns>
		internal static Collection CreateReportCollection( Store store, Domain domain )
		{
			// Check to see if the report has already been created.
			Collection report = store.GetCollectionByID( reportCollectionID );
			if ( report == null )
			{
				// Create the new report.
				report = new Collection( store, reportCollectionName, reportCollectionID, domain.ID );

				// Set the type as an iFolder so it can be accessed and shared by iFolder.
				report.SetType( report, "iFolder" );

				// Add the admin user for the domain as the owner.
				Member member = new Member( domain.Owner.Name, domain.Owner.UserID, Access.Rights.Admin );
				member.IsOwner = true;

				// Add the directory node 
				string dirPath = Path.Combine( report.UnmanagedPath, reportCollectionName );
				DirNode dirNode = new DirNode( report, dirPath );

				// Create the unmanaged directory for the reports.
				if ( !Directory.Exists( dirPath ) )
				{
					Directory.CreateDirectory( dirPath );
				}

				// Commit the changes.
				report.Commit( new Node[] { report, member, dirNode } );
			}

			return report;
		}

		#endregion

		#region Public Methods

		#endregion
	}
}
