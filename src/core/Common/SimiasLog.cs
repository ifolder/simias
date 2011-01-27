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
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Text;

using log4net;
//using log4net.spi;
using log4net.Layout;

namespace Simias
{
	/// <summary>
	/// A light wrapper around the log4net ILog class.
	/// </summary>
	public class SimiasLog : ISimiasLog
	{
		private ILog log;

		/// <summary>
		/// Internal Constructor
		/// </summary>
		/// <param name="log">The ILog object.</param>
		internal SimiasLog(ILog log)
		{
			this.log = log;
		}

		/// <summary>
		/// Log a DEBUG level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Debug(string format, params object[] args)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(String.Format(format, args));
			}
		}

		/// <summary>
		/// Log a INFO level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Info(string format, params object[] args)
		{
			if (log.IsInfoEnabled)
			{
				log.Info(String.Format(format, args));
			}
		}

		/// <summary>
		/// Log a WARN level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Warn(string format, params object[] args)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(String.Format(format, args));
			}
		}

		/// <summary>
		/// Log a ERROR level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Error(string format, params object[] args)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(String.Format(format, args));
			}
		}

		/// <summary>
		/// Log a FATAL level message.
		/// </summary>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Fatal(string format, params object[] args)
		{
			if (log.IsFatalEnabled)
			{
				log.Fatal(String.Format(format, args));
			}
		}

		/// <summary>
		/// Log a DEBUG level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Debug(Exception e, string format, params object[] args)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(String.Format(format, args), e);
			}
		}

		/// <summary>
		/// Log a INFO level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Info(Exception e, string format, params object[] args)
		{
			if (log.IsInfoEnabled)
			{
				log.Info(String.Format(format, args), e);
			}
		}

		/// <summary>
		/// Log a WARN level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Warn(Exception e, string format, params object[] args)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(String.Format(format, args), e);
			}
		}

		/// <summary>
		/// Log a ERROR level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Error(Exception e, string format, params object[] args)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(String.Format(format, args), e);
			}
		}

		/// <summary>
		/// Log a FATAL level message.
		/// </summary>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Fatal(Exception e, string format, params object[] args)
		{
			if (log.IsFatalEnabled)
			{
				log.Fatal(String.Format(format, args), e);
			}
		}
	}

	/// <summary>
	///
	/// </summary>
	public class SimiasAccessLogger
	{
		private static readonly string AccessLoggerName = "AccessLogger";
		static string Fields;
		static string FormatString;
		
		ILog	logger;
		string	user;
		string	collectionId;
		
		
		/// <summary>
		///
		/// </summary>
		/// <param name="user"></param>
		/// <param name="collectionId"></param>
		public SimiasAccessLogger(string user, string collectionId)
		{
			this.user = user;
			this.collectionId = collectionId;
			logger = LogManager.GetLogger(AccessLoggerName);

			lock (typeof (SimiasAccessLogger))
			{
				if (FormatString == null)
				{
					try
					{
						log4net.Repository.Hierarchy.Logger ll = logger.Logger as log4net.Repository.Hierarchy.Logger;
						string header = ((log4net.Appender.AppenderSkeleton)ll.GetAppender("AccessLogFile")).Layout.Header;
						string[] headers = header.Split('\n');
						foreach (string line in headers)
						{
							if (line.StartsWith("#Fields:"))
							{
								Fields = line.ToLower();
								string [] fields = Fields.Split("**".ToCharArray());
					
								// Now create the format string.
								StringBuilder sb = new StringBuilder(512);
								foreach (string field in fields)
								{
									switch(field)
									{
										case "date":
											sb.Append("{5}\t");
											break;
										case "time":
											sb.Append("{6}\t");
											break;
										case "user":
											sb.Append("\"{0}\"\t");
											break;
										case "method":
											sb.Append("\"{1}\"\t");
											break;
										case "uri":
											sb.Append("\"{2}\"\t");
											break;
										case "id":
											sb.Append("\"{3}\"\t");
											break;
										case "status":
											sb.Append("\"{4}\"\t");
											break;
										default:
											break;
									}
								}
								FormatString = sb.ToString();
								break;
							}
						}
					}
					catch
					{
						FormatString = "{5}\t{4}\t\"{0}\"\t\"{1}\"\t\"{2}\"\t\"{3}\"";
					}
				}
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="method"></param>
		/// <param name="uri"></param>
		/// <param name="id"></param>
		/// <param name="status"></param>
		public void LogAccess(string method, string uri, string id, string status)
		{
			if (logger.IsInfoEnabled)
			{
				DateTime time = DateTime.Now;
				logger.Info(string.Format(FormatString, user, method, uri, id, status, time.ToString("dd-MM-yyy"), time.ToString("HH:mm:ss")));
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="method"></param>
		/// <param name="uri"></param>
		/// <param name="id"></param>
		/// <param name="status"></param>
		public void LogAccessDebug(string method, string uri, string id, string status)
		{
			if (logger.IsInfoEnabled)
			{
				DateTime time = DateTime.Now;
				logger.Debug(string.Format(FormatString, user, method, uri, id, status, time.ToString("dd-MM-yyy"), time.ToString("HH:mm:ss")));
			}
		}
	}
}
