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

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="e"></param>
		public iFolderException(string message, Exception e) : base(message, e)
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
	public class EntryDoesNotExistException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public EntryDoesNotExistException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Entry Already Exists
	/// </summary>
	public class EntryAlreadyExistException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public EntryAlreadyExistException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Entry Name Contains Invalid Characters
	/// </summary>
	public class EntryInvalidCharactersException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public EntryInvalidCharactersException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Entry Has an Invalid Name
	/// </summary>
	public class EntryInvalidNameException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public EntryInvalidNameException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder Directory Entry Required
	/// </summary>
	public class DirectoryEntryRequiredException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public DirectoryEntryRequiredException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder File Lookup Failed
	/// </summary>
	public class FileDoesNotExistException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public FileDoesNotExistException(string message) : base(message)
		{
		}
	}

	/// <summary>
	/// iFolder File Not Open
	/// </summary>
	public class FileNotOpenException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public FileNotOpenException(string message) : base(message)
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
	/// Invalid Operation Given
	/// </summary>
	public class InvalidOperationException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public InvalidOperationException(string message) : base(message)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		/// <param name="e"></param>
		public InvalidOperationException(string message, Exception e) : base(message, e)
		{
		}
	}

	/// <summary>
	/// iFolder Member Lookup Failed
	/// </summary>
	public class MemberDoesNotExistException : iFolderException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"></param>
		public MemberDoesNotExistException(string message) : base(message)
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
