
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
 | Authors:
 | Gonzalo Paniagua Javier (gonzalo@ximian.com)
 |***************************************************************************/
 

using System;
using System.Collections;
using System.Diagnostics;

namespace Mono.ASPNET
{
	internal class WebTrace
	{
		static Stack ctxStack;
		static bool trace;
		static int indentation; // Number of \t

		static WebTrace ()
		{
			ctxStack = new Stack ();
		}

		[Conditional("WEBTRACE")]
		static public void PushContext (string context)
		{
			ctxStack.Push (context);
			indentation++;
		}
		
		[Conditional("WEBTRACE")]
		static public void PopContext ()
		{
			if (ctxStack.Count == 0)
				return;

			indentation--;
			ctxStack.Pop ();
		}

		static public string Context
		{
			get {
				if (ctxStack.Count == 0)
					return String.Empty;

				return (string) ctxStack.Peek ();
			}
		}

		static public bool StackTrace
		{
			get { return trace; }

			set { trace = value; }
		}
		
		[Conditional("WEBTRACE")]
		static public void WriteLine (string msg)
		{
			Console.WriteLine (Format (msg));
		}

		[Conditional("WEBTRACE")]
		static public void WriteLine (string msg, object arg)
		{
			Console.WriteLine (Format (String.Format (msg, arg)));
		}

		[Conditional("WEBTRACE")]
		static public void WriteLine (string msg, object arg1, object arg2)
		{
			Console.WriteLine (Format (String.Format (msg, arg1, arg2)));
		}

		[Conditional("WEBTRACE")]
		static public void WriteLine (string msg, object arg1, object arg2, object arg3)
		{
			Console.WriteLine (Format (String.Format (msg, arg1, arg2, arg3)));
		}

		[Conditional("WEBTRACE")]
		static public void WriteLine (string msg, params object [] args)
		{
			Console.WriteLine (Format (String.Format (msg, args)));
		}

		static string Tabs
		{
			get {
				if (indentation == 0)
					return String.Empty;

				return new String ('\t', indentation);
			}
		}

		static string Format (string msg)
		{
			string ctx = Tabs + Context;
			if (ctx.Length != 0)
				ctx += ": ";
			
			string result = ctx + msg;
			if (trace)
				result += "\n" + Environment.StackTrace;

			return result;
		}
	}
}

