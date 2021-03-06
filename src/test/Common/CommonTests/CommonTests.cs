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
            {
            }

            /// <summary>
            /// Test fixture setup
            /// </summary>
            [TestFixtureSetUp]
            public void FixtureSetup()
            {
                string path = Path.GetFullPath("./common1");
                Directory.CreateDirectory(path);
                SimiasLogManager.Configure(path);
            }

            /// <summary>
            /// Log Test 1
            /// </summary>
            [Test]
            public void TestLog1()
            {
                log.Debug("Test 1");
            }

            /// <summary>
            /// Log Test 2
            /// </summary>
            [Test]
            public void TestTrace2()
            {
                log.Debug("Test 2 : {0}", "hello");
            }

            /// <summary>
            /// Log Test 3
            /// </summary>
            [Test]
            public void TestTrace3()
            {
                log.Debug(new Exception(), "Test 3");
            }

            /// <summary>
            /// Dns Test
            /// </summary>
            [Test]
            public void TestDns()
            {
                log.Debug("Host: {0}", MyDns.GetHostName());
            }

            /// <summary>
            /// Environment Test
            /// </summary>
            [Test]
            public void TestEnvironment()
            {
                log.Debug("Platform: {0}", MyEnvironment.Platform);
                log.Debug("Runtime: {0}", MyEnvironment.Runtime);
            }

            /// <summary>
            /// Path Test
            /// </summary>
            [Test]
            public void TestPath()
            {
                string path1 = @"/home/jdoe";
                string path2 = @"c:\home\jdoe";

                log.Debug("Full Local Path: {0} ({1})", MyPath.GetFullLocalPath(path1), path1);
                log.Debug("Full Local Path: {0} ({1})", MyPath.GetFullLocalPath(path2), path2);
            }

        }
    
}
