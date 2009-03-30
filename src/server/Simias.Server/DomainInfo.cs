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
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Text;

namespace Simias.DomainService.Web
{
	/// <summary>
	/// Domain information
	/// </summary>
	[Serializable]
	public class DomainInfo
	{
		/// <summary>
		/// Domain Name
		/// </summary>
		public string Name;

		/// <summary>
		/// Domain Description
		/// </summary>
		public string Description;

		/// <summary>
		/// Domain ID
		/// </summary>
		public string ID;

		/// <summary>
		/// The unique ID for the member node for this user.
		/// </summary>
		public string MemberNodeID;

		/// <summary>
		/// The name of the member object in the POBox collection.
		/// </summary>
		public string MemberNodeName;

		/// <summary>
		/// The rights that the member has in the POBox collection.
		/// </summary>
		public string MemberRights;

		/// <summary>
		/// Constructor
		/// </summary>
		public DomainInfo()
		{
		}

		/// <summary>
		/// Create a string representation
		/// </summary>
		/// <returns>A string representation</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			
			string newLine = Environment.NewLine;

			builder.AppendFormat("Domain Information{0}", newLine);
			builder.AppendFormat("  ID               : {0}{1}", this.ID, newLine);
			builder.AppendFormat("  Name             : {0}{1}", this.Name, newLine);
			builder.AppendFormat("  Description      : {0}{1}", this.Description, newLine);
			builder.AppendFormat("  Member Node ID   : {0}{1}", this.MemberNodeID, newLine);
			builder.AppendFormat("  Member Node Name : {0}{1}", this.MemberNodeName, newLine);
			builder.AppendFormat("  Member Rights    : {0}{1}", this.MemberRights, newLine);

			return builder.ToString();
		}
	}
}
