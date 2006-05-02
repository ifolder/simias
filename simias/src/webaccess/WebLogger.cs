/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
using System.Web;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// iFolder Web Logger
	/// </summary>
	public class WebLogger
	{
		log4net.ILog log;

		/// <summary>
		/// Static Constructor
		/// </summary>
		/// <remarks>
		/// This constructor will initialize the log file correctly based
		/// on what is setup in the Log4Net.conf file and the SimiasLogDir
		/// environment variable.
		/// </remarks>
		static WebLogger()
		{
			string logDir = Environment.GetEnvironmentVariable("SimiasLogDir");
			
			if (logDir != null)
			{
				foreach(log4net.Appender.IAppender iApp in 
					log4net.LogManager.GetRepository().GetAppenders())
				{
					if (iApp is log4net.Appender.FileAppender)
					{
						log4net.Appender.FileAppender fApp =
								(log4net.Appender.FileAppender)iApp;

						string fileName = System.IO.Path.GetFileName(fApp.File);
						if (fileName == null) fileName = "web.log";

						fApp.File = System.IO.Path.Combine(logDir, fileName);
						fApp.ActivateOptions();
					}
				}
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type"></param>
		public WebLogger(Type type)
		{
			log = log4net.LogManager.GetLogger(type);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		public WebLogger(string name)
		{
			log = log4net.LogManager.GetLogger(name);
		}

		/// <summary>
		/// Format the message with the HTTP context information.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		/// <returns>The formatted message.</returns>
		private string Format(HttpContext context, string format, params object[] args)
		{
			return String.Format(String.Format("[{0}] {1}",
				context.Request.UserHostAddress, format), args);
		}

		/// <summary>
		/// Log a DEBUG level message.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Debug(HttpContext context, string format, params object[] args)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(Format(context, format, args));
			}
		}

		/// <summary>
		/// Log a INFO level message.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Info(HttpContext context, string format, params object[] args)
		{
			if (log.IsInfoEnabled)
			{
				log.Info(Format(context, format, args));
			}
		}

		/// <summary>
		/// Log a WARN level message.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Warn(HttpContext context, string format, params object[] args)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(Format(context, format, args));
			}
		}

		/// <summary>
		/// Log a ERROR level message.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Error(HttpContext context, string format, params object[] args)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(Format(context, format, args));
			}
		}

		/// <summary>
		/// Log a FATAL level message.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Fatal(HttpContext context, string format, params object[] args)
		{
			if (log.IsFatalEnabled)
			{
				log.Fatal(Format(context, format, args));
			}
		}

		/// <summary>
		/// Log a DEBUG level message.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Debug(HttpContext context, Exception e, string format, params object[] args)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug(Format(context, format, args), e);
			}
		}

		/// <summary>
		/// Log a INFO level message.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Info(HttpContext context, Exception e, string format, params object[] args)
		{
			if (log.IsInfoEnabled)
			{
				log.Info(Format(context, format, args), e);
			}
		}

		/// <summary>
		/// Log a WARN level message.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Warn(HttpContext context, Exception e, string format, params object[] args)
		{
			if (log.IsWarnEnabled)
			{
				log.Warn(Format(context, format, args), e);
			}
		}

		/// <summary>
		/// Log a ERROR level message.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Error(HttpContext context, Exception e, string format, params object[] args)
		{
			if (log.IsErrorEnabled)
			{
				log.Error(Format(context, format, args), e);
			}
		}

		/// <summary>
		/// Log a FATAL level message.
		/// </summary>
		/// <param name="context">The HTTP context object.</param>
		/// <param name="e">An exception associated with the message.</param>
		/// <param name="format">A string with optional format items.</param>
		/// <param name="args">An optional array of objects to format.</param>
		public void Fatal(HttpContext context, Exception e, string format, params object[] args)
		{
			if (log.IsFatalEnabled)
			{
				log.Fatal(Format(context, format, args), e);
			}
		}
	}
}
