/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 |   	Author: Rob
 |***************************************************************************/

using System;

using log4net;
//using log4net.spi;

namespace Simias
{
	/// <summary>
	/// Simias Log Interface
	/// </summary>
	public interface ISimiasLog
	{
		/// <summary>
		/// Log a DEBUG level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		void Debug(string format, params object[] args);

		/// <summary>
		/// Log a INFO level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		void Info(string format, params object[] args);

		/// <summary>
		/// Log a WARN level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		void Warn(string format, params object[] args);

		/// <summary>
		/// Log a ERROR level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		void Error(string format, params object[] args);

		/// <summary>
		/// Log a FATAL level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		void Fatal(string format, params object[] args);

		/// <summary>
		/// Log a DEBUG level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		void Debug(Exception e, string format, params object[] args);

		/// <summary>
		/// Log a INFO level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		void Info(Exception e, string format, params object[] args);

		/// <summary>
		/// Log a WARN level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		void Warn(Exception e, string format, params object[] args);

		/// <summary>
		/// Log a ERROR level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		void Error(Exception e, string format, params object[] args);

		/// <summary>
		/// Log a FATAL level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		void Fatal(Exception e, string format, params object[] args);
	}
}
