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

namespace Simias.RssFeed
{
	/// <summary>
	/// Summary description for RssHeaders.
	/// </summary>
	public class Headers
	{
        /// <summary>
        /// send the start tag
        /// </summary>
        /// <param name="Ctx">Httpcontext object</param>
		static public void SendStartTag( HttpContext Ctx )
		{
			Ctx.Response.ContentType = "text/xml";
			Ctx.Response.Write( "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>" );
			Ctx.Response.Write( "<rss version=\"2.0\">" );
		}

        /// <summary>
        /// send the end tag
        /// </summary>
        /// <param name="Ctx">httpcontext object</param>
		static public void SendEndTag( HttpContext Ctx )
		{
			Ctx.Response.Write( "</rss>" );
		}
	}
}
