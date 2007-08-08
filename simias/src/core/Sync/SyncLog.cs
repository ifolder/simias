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
 | Author: Dale Olds <olds@novell.com>
 |***************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary> catch-all log for misc sync classes</summary>
public class Log
{
	/// <summary>
	/// The logging object.
	/// </summary>
	public static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(Log));

	// don't always get line numbers from stack dumps, so force it here
	static void DumpStack()
	{
		StackTrace st = new StackTrace(2, true);
		for (int i = 0; i < st.FrameCount; ++i)
		{
			StackFrame sf = st.GetFrame(i);
			log.Debug(String.Format("  called from {0}, {1}: {2}",
					sf.GetFileName(), sf.GetFileLineNumber(),
					sf.GetMethod().ToString()));
		}
	}

	/// <summary>
	/// Prints out the stack frame.
	/// </summary>
	public static void Here()
	{
		StackFrame sf = new StackTrace(1, true).GetFrame(0);
		log.Debug("Here: {0}:{1} {2}", sf.GetFileName(), sf.GetFileLineNumber(), sf.GetMethod().ToString());
	}

	/// <summary>
	/// Prints the exception.
	/// </summary>
	/// <param name="e"></param>
	public static void Uncaught(Exception e)
	{
		log.Debug(String.Format("Uncaught exception: {0}\n{1}", e.Message, e.StackTrace));
	}

	/// <summary>
	/// Logs an assertion.
	/// </summary>
	/// <param name="assertion"></param>
	public static void Assert(bool assertion)
	{
		if (!assertion)
		{
			log.Error("Assertion failed ------------");
			DumpStack();
		}
	}

}

//===========================================================================
}
