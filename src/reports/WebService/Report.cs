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
*                 $Author:  
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

namespace Novell.iFolder.Enterprise
{
	/// <summary>
	/// Report
	/// </summary>
	public class Report
	{
		private Report()
		{
		}

        /// <summary>
        /// write the header row
        /// </summary>
        /// <param name="writer"a stream writer object></param>
        /// <param name="columns">columns to be written</param>
		public static void WriteHeaderRow(StreamWriter writer, ReportColumn[] columns)
		{
			for(int i=0; i < columns.Length; i++)
			{
				writer.Write("\"{0}\"{1}", columns[i].Header,
					(i < (columns.Length - 1)) ? "," : "");
			}

			writer.WriteLine();
		}

        /// <summary>
        /// write a particular row
        /// </summary>
        /// <param name="writer">streamwriter object</param>
        /// <param name="columns">columns to be written</param>
        /// <param name="cells">calls to be written</param>
		public static void WriteRow(StreamWriter writer, ReportColumn[] columns, object[] cells)
		{
			for(int i=0; i < columns.Length; i++)
			{
				writer.Write("\"" + columns[i].Format + "\"{1}", cells[i],
					(i < (columns.Length - 1)) ? "," : "");
			}

			writer.WriteLine();
		}
	}
}
