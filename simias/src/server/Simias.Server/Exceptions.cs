/***********************************************************************
 *  $RCSfile: Exceptions.cs,v $
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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.Collections;

using Simias;

namespace Simias.Server
{
	/// <summary>
	/// Server Exception
	/// </summary>
	public class ServerException : SimiasException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ServerException() : base()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public ServerException( string message ) : base( message )
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="e"></param>
		public ServerException( string message, Exception e ) : base( message, e )
		{
		}
	}

	/// <summary>
	/// Authentication Failed
	/// </summary>
	public class AuthenticationException : ServerException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public AuthenticationException() : base()
		{
		}
	}

	/// <summary>
	/// Authorization Failed
	/// </summary>
	public class AuthorizationException : ServerException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public AuthorizationException( string message ) : base( message )
		{
		}
	}
}
