/***********************************************************************
 *  $RCSfile$ Stats.cs,v $
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
using System.Collections;
using System.Text;

namespace Simias.Server.Statistics
{
	/// <summary>
	/// Class for updating and reporting system statistics for
	/// an instance of the server.
	/// </summary>
	public class System
	{
		static Int64 Requests = 0;
		static Int64	 Authentications = 0;
		static Int32	 OutstandingRequests = 0;
		static Int32 MaximumOutstandingRequests = 0;
		static Int32	 AuthenticationFailures = 0;
		static Int32	 ExceptionFailures = 0;
		static Int32	 MaximumSessions = 0;
		
		static DateTime UpSince;
	}
	
	/// <summary>
	/// Class for updating and reporting session stats for
	/// an instance of a user session.
	/// </summary>
	public class Session
	{
		static Hashtable Sessions;
		
		internal string userID;
		internal Int64 requests;
		internal DateTime created;
		internal DateTime lastRequest;
		
		public string UserID
		{
			get { return( userID ); }
		}
		
		public Int64 Requets
		{
			get { return( requests ); }
		}
		
		public DateTime Created
		{
			get { return( created ); }
		}
		
		public DateTime LastRequest
		{
			get { return( lastRequest ); }
		}
	}
}
