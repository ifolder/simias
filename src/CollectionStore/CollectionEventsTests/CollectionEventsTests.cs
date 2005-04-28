/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using NUnit.Framework;
using Simias;
using Simias.Service;
using Simias.Event;

namespace Simias.Storage
{
	/// <summary>
	/// Test Fixture for the Collection Events.
	/// </summary>
	[TestFixture]
	public class EventsTests
	{
		#region Fields
		Manager				serviceManager;
		EventSubscriber		subscriber = null;
		EventPublisher		publisher = null;
		EventArgs			args;
		ManualResetEvent	mre = new ManualResetEvent(false);
		ManualResetEvent    shutdownEvent = new ManualResetEvent(false);
		string				collection = "Collection123";
		DateTime			firstEvent = DateTime.MinValue;
		DateTime			lastEvent = DateTime.MinValue;
		int					eventCount = 0;
		bool				performanceTest = false;
		static Configuration conf = Configuration.CreateDefaultConfig(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath));

		#endregion

		#region Setup/TearDown

		/// <summary>
		/// Test Setup.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			serviceManager = new Manager(conf);
			serviceManager.StartServices();
			serviceManager.WaitForServicesStarted();
			publish();
			subscribe();
		}

		/// <summary>
		/// Get a publisher.
		/// </summary>
		public void publish()
		{
			publisher = new EventPublisher(conf);
		}

		private void subscribe()
		{
			subscriber = new EventSubscriber(conf);
			subscriber.NodeChanged += new NodeEventHandler(OnNodeChange);
			subscriber.NodeCreated += new NodeEventHandler(OnNodeCreate);
			subscriber.NodeDeleted += new NodeEventHandler(OnNodeDelete);
			subscriber.CollectionRootChanged += new CollectionRootChangedHandler(OnCollectionRootChanged);
			subscriber.FileChanged += new FileEventHandler(OnFileChange);
			subscriber.FileCreated += new FileEventHandler(OnFileCreate);
			subscriber.FileDeleted += new FileEventHandler(OnFileDelete);
			subscriber.FileRenamed += new FileRenameEventHandler(OnFileRenamed);
		}

		/// <summary>
		/// Test cleanup.
		/// </summary>
		[TestFixtureTearDown]
		public void Cleanup()
		{
			if (subscriber != null)
				subscriber.Dispose();
			if (serviceManager != null)
			{
				serviceManager.StopServices();
				serviceManager.WaitForServicesStopped();
			}
		}

		#endregion

		#region Event Handlers

		void OnNodeChange(NodeEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Change: {0} {1} {2}", args.Node, args.Collection, args.Type);
		}

		void OnNodeCreate(NodeEventArgs args)
		{
			mre.Set();
			this.args = args;
			if (!performanceTest)
				Console.WriteLine("Create: {0} {1} {2}", args.Node, args.Collection, args.Type);
			else
			{
				if (firstEvent == DateTime.MinValue)
					firstEvent = DateTime.Now;
				++eventCount;
			}
		}

		void OnNodeDelete(NodeEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Delete: {0} {1} {2}", args.Node, args.Collection, args.Type);
		}

		void OnCollectionRootChanged(CollectionRootChangedEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("Collection Root Changed: from {0} to {1}", args.OldRoot, args.NewRoot);
		}

		void OnFileChange(FileEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("File Change: {0} {1} {2}", args.FullPath, args.Collection, args.Type);
		}

		void OnFileCreate(FileEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("File Create: {0} {1} {2}", args.FullPath, args.Collection, args.Type);
		}

		void OnFileDelete(FileEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("File Delete: {0} {1} {2}", args.FullPath, args.Collection, args.Type);
		}

		void OnFileRenamed(FileRenameEventArgs args)
		{
			mre.Set();
			this.args = args;
			Console.WriteLine("File Rename: {0} {1} {2}", args.OldName, args.FullPath, args.Collection);
		}

		#endregion

		#region Tests

		/// <summary>
		/// Change event test.
		/// </summary>
		[Test]
		public void NodeChangeTest()
		{
			try
			{
				args = null;
				publisher.RaiseEvent(new NodeEventArgs("CollectionEventsTests", "1", collection, "Node", EventType.NodeChanged));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Create event test.
		/// </summary>
		[Test]
		public void NodeCreateTest()
		{
			args = null;
			publisher.RaiseEvent(new NodeEventArgs("CollectionEventsTests", "2", collection, "Node", EventType.NodeCreated));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Delete event test.
		/// </summary>
		[Test]
		public void NodeDeleteTest()
		{
			args = null;
			publisher.RaiseEvent(new NodeEventArgs("CollectionEventsTests", "3", collection, "Node", EventType.NodeDeleted));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Rename event test.
		/// </summary>
		[Test]
		public void CollectionRootChangedTest()
		{
			args = null;
			publisher.RaiseEvent(new CollectionRootChangedEventArgs("CollectionEventsTests", collection, "collection", @"c:\path\oldroot", @"c:\path\newRoot"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Change event test.
		/// </summary>
		[Test]
		public void FileChangeTest()
		{
			args = null;
			publisher.RaiseEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", collection, EventType.FileChanged));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Create event test.
		/// </summary>
		[Test]
		public void FileCreateTest()
		{
			args = null;
			publisher.RaiseEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", collection, EventType.FileCreated));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Delete event test.
		/// </summary>
		[Test]
		public void FileDeleteTest()
		{
			args = null;
			publisher.RaiseEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", collection, EventType.FileDeleted));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}

		/// <summary>
		/// Rename event test.
		/// </summary>
		[Test]
		public void FileRenamedTest()
		{
			args = null;
			publisher.RaiseEvent(new FileRenameEventArgs("CollectionEventsTests", @"c:\path\newfile.txt", collection, @"c:\path\oldname.txt"));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}
		}


		/// <summary>
		/// Name filter test.
		/// </summary>
		[Test]
		public void NodeIDFilterTest()
		{
			// Check for a hit.
			string nodeId = "123456789";
			args = null;
			subscriber.NodeIDFilter = nodeId;
			publisher.RaiseEvent(new NodeEventArgs("CollectionEventsTests", nodeId, collection, "Node", EventType.NodeCreated));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseEvent(new NodeEventArgs("CollectionEventsTests", "987654321", collection, "Node", EventType.NodeCreated));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			subscriber.NodeIDFilter = null;
		}

		/// <summary>
		/// Type filter test.
		/// </summary>
		[Test]
		public void NodeTypeFilterTest()
		{
			// Check for a hit.
			string nodeId = "123456789";
			args = null;
			subscriber.NodeTypeFilter = "Node";
			publisher.RaiseEvent(new NodeEventArgs("CollectionEventsTests", nodeId, collection, "Node", EventType.NodeCreated));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseEvent(new NodeEventArgs("CollectionEventsTests", nodeId, collection, "Collection", EventType.NodeCreated));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			subscriber.NodeTypeFilter = null;
		}

		/// <summary>
		/// Name filter test.
		/// </summary>
		[Test]
		public void FileNameFilterTest()
		{
			// Check for a hit.
			args = null;
			subscriber.FileNameFilter = "test.*";
			publisher.RaiseEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\testNode.txt", collection, EventType.FileCreated));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\tastNode.txt", collection, EventType.FileCreated));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			subscriber.FileNameFilter = null;
		}


		/// <summary>
		/// Type filter test.
		/// </summary>
		[Test]
		public void FileTypeFilterTest()
		{
			// Check for a hit.
			args = null;
			subscriber.FileTypeFilter = ".txt";
			publisher.RaiseEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", collection, EventType.FileCreated));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.doc", collection, EventType.FileCreated));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			subscriber.FileTypeFilter = null;
		}



		/// <summary>
		/// Disable test.
		/// </summary>
		[Test]
		public void DisableTest()
		{
			// Check disabled.
			args = null;
			subscriber.Enabled = false;
			publisher.RaiseEvent(new NodeEventArgs("CollectionEventsTests", "123456789", collection, "Node", EventType.NodeCreated));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed disable");
			}

			// Check reenabled.
			args = null;
			subscriber.Enabled = true;
			publisher.RaiseEvent(new NodeEventArgs("CollectionEventsTests", "123456789", collection, "Node", EventType.NodeCreated));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed enable");
			}
		}

		#endregion

		#region privates

		private bool recievedCallback
		{
			get
			{
				bool b = mre.WaitOne(500, false);
				mre.Reset();
				return b;
			}
		}

		static void usage()
		{
			Console.WriteLine("Usage: CollectionEventsTest.exe (mode) [event count]");
			Console.WriteLine("      where mode = P(ublish) [event count]");
			Console.WriteLine("      or    mode = S(ubscribe) (P(erformance))");
			Console.WriteLine("      or    mode = SD(Subscribe Default Store)");
			Console.WriteLine("      or    mode = LD(Load service Default Store)");
			Console.WriteLine("      or    mode = L(Load service)");
		}

		#endregion

		#region Main

		/// <summary>
		/// Main entry.
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string [] args)
		{
			if (args.Length == 0)
			{
				usage();
				return;
			}

			EventsTests t = new EventsTests();
			switch (args[0])
			{
				case "P":
			
					if (args.Length > 1)
					{
						t.publisher = new EventPublisher(conf);
						int count = Int32.Parse(args[1]);
						for (int i = 0; i < count; ++i)
						{
							t.publisher.RaiseEvent(new NodeEventArgs("nifp", i.ToString(), t.collection, "Node", EventType.NodeCreated));
						}
					}
					break;

				case "PS":
					t.subscribe();
					t.publisher = new EventPublisher(conf);
					for (int i = 0; i < 1000; ++i)
					{
						t.publisher.RaiseEvent(new NodeEventArgs("nifp", i.ToString(), t.collection, "Node", EventType.NodeCreated));
					}
					Console.ReadLine();
					t.Cleanup();
					break;

				case "S":
					if (args.Length == 2 && args[1].StartsWith("P"))
						t.performanceTest = true;
					t.subscribe();
					t.shutdownEvent.WaitOne();
					t.Cleanup();
					break;

				case "SD":
					conf = Configuration.GetConfiguration();
					t.subscribe();
					Console.WriteLine("Press Enter to exit");
					Console.ReadLine();
					t.Cleanup();
					break;

				case "L":
				{
					Manager manager = new Manager(conf);
					manager.StartServices();
					Console.WriteLine("Press Enter to exit");
					Console.ReadLine();
					manager.StopServices();
					break;
				}

				case "LD":
				{
					conf = Configuration.GetConfiguration();
					Manager manager = new Manager(conf);
					manager.StartServices();
					Console.WriteLine("Press Enter to exit");
					Console.ReadLine();
					manager.StopServices();
					break;
				}

				default:
					usage();
					break;
			}
		}
		
		#endregion
	}
}