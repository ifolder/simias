/***********************************************************************
 *  $RCSfile: iFolderWebLogger.cs,v $
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Web;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// iFolder Web Logger
	/// </summary>
	public class iFolderWebLogger
	{
		log4net.ILog log;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="type"></param>
		public iFolderWebLogger(Type type)
		{
			log = log4net.LogManager.GetLogger(type);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		public iFolderWebLogger(string name)
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
