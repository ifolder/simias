/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

using NUnit.Framework;

using Simias;
using Simias.Sync;

namespace Simias.Sync.Tests
{
	/// <summary>
	/// Sync Manager Tests
	/// </summary>
	[TestFixture]
	public class SyncManagerTests
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public SyncManagerTests()
		{
			MyTrace.SendTraceToStandardOutput();
		}

		/// <summary>
		/// Simple test of the sync manager.
		/// </summary>
		[Test]
		public void TestSyncManager()
		{
			SyncProperties properties = new SyncProperties();
			properties.StorePath = "./manager1";

			SyncManager manager = new SyncManager(properties);
			
			manager.Start();

			Thread.Sleep(TimeSpan.FromSeconds(5));

			manager.Stop();

			manager.Dispose();
		}
	}
}
