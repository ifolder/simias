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

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder Exception
	/// </summary>
	public class iFolderException : SimiasException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderException() : base()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public iFolderException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// Authentication Failed
	/// </summary>
	public class AuthenticationException : iFolderException
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
	public class AuthorizationException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public AuthorizationException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Lookup Failed
	/// </summary>
	public class iFolderDoesNotExistException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public iFolderDoesNotExistException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// User Lookup Failed
	/// </summary>
	public class UserDoesNotExistException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public UserDoesNotExistException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Entry Lookup Failed
	/// </summary>
	public class iFolderEntryDoesNotExistException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public iFolderEntryDoesNotExistException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Entry Already Exists
	/// </summary>
	public class iFolderEntryAlreadyExistException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public iFolderEntryAlreadyExistException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Entry Name Contains Invalid Characters
	/// </summary>
	public class iFolderEntryInvalidCharactersException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public iFolderEntryInvalidCharactersException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Entry Has an Invalid Name
	/// </summary>
	public class iFolderEntryInvalidNameException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public iFolderEntryInvalidNameException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Directory Entry Required
	/// </summary>
	public class iFolderDirectoryEntryRequiredException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public iFolderDirectoryEntryRequiredException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder File Lookup Failed
	/// </summary>
	public class iFolderFileDoesNotExistException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public iFolderFileDoesNotExistException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder File Not Open
	/// </summary>
	public class iFolderFileNotOpenException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public iFolderFileNotOpenException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// Invalid Option Given
	/// </summary>
	public class InvalidOptionException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public InvalidOptionException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Member Lookup Failed
	/// </summary>
	public class iFolderMemberDoesNotExistException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public iFolderMemberDoesNotExistException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// File Size Policy Check Failed
	/// </summary>
	public class FileSizeException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public FileSizeException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// Disk Quota Policy Check Failed
	/// </summary>
	public class DiskQuotaException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public DiskQuotaException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// File Type Policy Check Failed
	/// </summary>
	public class FileTypeException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public FileTypeException(string message) : base(message)
		{
		}
	}
}
