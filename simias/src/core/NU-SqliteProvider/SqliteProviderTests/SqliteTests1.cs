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
 |  Author: Russ Young
 |***************************************************************************/
 
using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using System.Xml;
using Simias.Storage.Provider;
using Simias.Storage.Provider.Sqlite;

namespace Simias.Storage.Provider.Sqlite.Tests
{
	/// <summary>
	/// Unit Test for the Flaim Storage Provider.
	/// </summary>
	/// 
	[TestFixture]
	public class SqliteTests1 : Simias.Storage.Provider.Tests
	{
		/// <summary>
		/// Used to setup all the needed resources before the tests are run.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			base.Init("SqliteProvider.dll", "Simias.Storage.Provider.Sqlite.SqliteProvider");
		}

	}
}
