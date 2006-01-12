/***********************************************************************
 *  $RCSfile: Util.cs,v $
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
 *  Author: Brady Anderson (banderso@novell.com)
 * 
 ***********************************************************************/
using System;
using System.Web;

using Simias;
using Simias.Storage;

namespace Simias.RssFeed
{
	public class Util
	{
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
		
		static public string GetRfc822Date( DateTime DT )
		{
			return
				String.Format(
					"{0}, {1} {2} {3} {4}:{5}:{6} GMT",
					DT.DayOfWeek.ToString(),
					DT.Day,
					Simias.RssFeed.Util.MonthsOfYear[ DT.Month - 1 ],
					DT.Year.ToString(),
					DT.Hour,
					DT.Minute,
					DT.Second );
		}
		
		static public void SendPublishDate( HttpContext Ctx, DateTime DT )
		{
			//	Ex. Sat, 07 Sep 2002 00:00:01 GMT
			Ctx.Response.Write( "<pubDate>" );
			Ctx.Response.Write( 
				String.Format( 
					"{0}, {1} {2} {3} {4}:{5}:{6} GMT",
					DT.DayOfWeek.ToString(),
					DT.Day,
					Simias.RssFeed.Util.MonthsOfYear[ DT.Month - 1 ],
					DT.Year.ToString(),
					DT.Hour,
					DT.Minute,
					DT.Second ) );

			Ctx.Response.Write( "</pubDate>" );
		}
	}
}