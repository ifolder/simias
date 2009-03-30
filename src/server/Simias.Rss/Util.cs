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
*                 $Author: Brady Anderson <banderso@novell.com>
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
using System.Web;

using Simias;
using Simias.Storage;

namespace Simias.RssFeed
{
	public class Util
	{
		public static string LastModified = "LastModified";
		public static int DefaultTtl = 300;
		public static string DefaultRating = "PG-13";

		public static string[] DaysOfTheWeek =
		{
			"Sun",
			"Mon",
			"Tue",
			"Wed",
			"Thu",
			"Fri",
			"Sat"
		};

		public static string[] MonthsOfYear =
		{
			"Jan",
			"Feb",
			"Mar",
			"Apr",
			"Jun",
			"Jul",
			"Aug",
			"Sep",
			"Oct",
			"Nov",
			"Dec"
		};
		
        /// <summary>
        /// get the date
        /// </summary>
        /// <param name="DT">datetime object</param>
        /// <returns>formatted date </returns>
		static public string GetRfc822Date( DateTime DT )
		{
			return String.Format( "{0:r}", DT );
				/*
				String.Format(
					"{0}, {1:00} {2} {3} {4:00}:{5:00}:{6:00} GMT",
					Simias.RssFeed.Util.DaysOfTheWeek[ 0 ],
					//DT.DayOfWeek.ToString(),
					DT.Day,
					Simias.RssFeed.Util.MonthsOfYear[ DT.Month - 1 ],
					DT.Year.ToString(),
					DT.Hour,
					DT.Minute,
					DT.Second );
					*/
		}
		
        /// <summary>
        /// send the publish date
        /// </summary>
        /// <param name="Ctx">httpcontext object</param>
        /// <param name="DT">datetime object</param>
		static public void SendPublishDate( HttpContext Ctx, DateTime DT )
		{
			//	Ex. Sat, 07 Sep 2002 00:00:01 GMT
			Ctx.Response.Write( "<pubDate>" );
			Ctx.Response.Write(	String.Format( "{0:r}",DT ) );
			
			/*
			Ctx.Response.Write( 
				String.Format( 
					"{0}, {1:00} {2} {3} {4:00}:{5:00}:{6:00} GMT",
					Simias.RssFeed.Util.DaysOfTheWeek[ 0 ],
					//DT.DayOfWeek.ToString(),
					DT.Day,
					Simias.RssFeed.Util.MonthsOfYear[ DT.Month - 1 ],
					DT.Year.ToString(),
					DT.Hour,
					DT.Minute,
					DT.Second ) );
			*/

			Ctx.Response.Write( "</pubDate>" );
		}
	}
}
