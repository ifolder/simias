/***********************************************************************
 *  CollectionEventTests.cs - A unit test suite for events.
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
 *  Author: Russ Young <ryoung@novell.com>
 * 
 ***********************************************************************/
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using NUnit.Framework;
using Simias;

namespace Simias.Event
{
	/// <summary>
	/// Test Fixture for the Collection Events.
	/// </summary>
	[TestFixture]
	public class EventsTests
	{
		#region Fields
		EventSubscriber		subscriber;
		EventPublisher		publisher;
		ServiceEventSubscriber serviceSubscriber;
		EventArgs			args;
		ManualResetEvent	mre = new ManualResetEvent(false);
		ManualResetEvent    shutdownEvent = new ManualResetEvent(false);
		string				collection = "Collection123";
		string				domainName = "TestDomain_123456789";
		Configuration conf = new Configuration(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath));

		#endregion

		#region Setup/TearDown

		/// <summary>
		/// Test Setup.
		/// </summary>
		[TestFixtureSetUp]
		public void Init()
		{
			EventBroker.overrideConfig = true;
			PublishSubscribe();
		}

		public void PublishSubscribe()
		{
			
			publisher = new EventPublisher(conf, domainName);
			subscriber = new EventSubscriber(conf, domainName);
			subscriber.NodeChanged += new NodeEventHandler(OnNodeChange);
			subscriber.NodeCreated += new NodeEventHandler(OnNodeCreate);
			subscriber.NodeDeleted += new NodeEventHandler(OnNodeDelete);
			subscriber.CollectionRootChanged += new CollectionEventHandler(OnCollectionRootChanged);
			subscriber.FileChanged += new FileEventHandler(OnFileChange);
			subscriber.FileCreated += new FileEventHandler(OnFileCreate);
			subscriber.FileDeleted += new FileEventHandler(OnFileDelete);
			subscriber.FileRenamed += new FileRenameEventHandler(OnFileRenamed);

			serviceSubscriber = new ServiceEventSubscriber(conf, domainName);
			serviceSubscriber.ServiceControl += new ServiceEventHandler(ServiceCtlHandler);
		}

		/// <summary>
		/// Test cleanup.
		/// </summary>
		[TestFixtureTearDown]
		public void Cleanup()
		{
			subscriber.Dispose();
			serviceSubscriber.Dispose();
			publisher.RaiseServiceEvent(new ServiceEventArgs(ServiceEventArgs.TargetAll, ServiceEventArgs.ServiceEvent.Shutdown));
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
			Console.WriteLine("Create: {0} {1} {2}", args.Node, args.Collection, args.Type);
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

		void ServiceCtlHandler(ServiceEventArgs args)
		{
			mre.Set();
			Console.WriteLine("Service Control Event = {0}", args.EventType); 
			if (args.EventType == ServiceEventArgs.ServiceEvent.Shutdown)
				shutdownEvent.Set();
		}

		#endregion

		#region Tests

		/// <summary>
		/// Change event test.
		/// </summary>
		[Test]
		public void NodeChangeTest()
		{
			args = null;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "1", collection, domainName, "Node", NodeEventArgs.EventType.Changed));
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
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "2", collection, domainName, "Node", NodeEventArgs.EventType.Created));
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
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "3", collection, domainName, "Node", NodeEventArgs.EventType.Deleted));
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
			publisher.RaiseCollectionRootChangedEvent(new CollectionRootChangedEventArgs("CollectionEventsTests", collection, domainName, "collection", @"c:\path\oldroot", @"c:\path\newRoot"));
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
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", collection, domainName, FileEventArgs.EventType.Changed));
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
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", collection, domainName, FileEventArgs.EventType.Created));
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
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", collection, domainName, FileEventArgs.EventType.Deleted));
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
			publisher.RaiseFileEvent(new FileRenameEventArgs("CollectionEventsTests", @"c:\path\newfile.txt", collection, domainName, @"c:\path\oldname.txt"));
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
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", nodeId, collection, domainName, "Node", NodeEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "987654321", collection, domainName, "Node", NodeEventArgs.EventType.Created));
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
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", nodeId, collection, domainName, "Node", NodeEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", nodeId, collection, domainName, "Collection", NodeEventArgs.EventType.Created));
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
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\testNode.txt", collection, domainName, FileEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\tastNode.txt", collection, domainName, FileEventArgs.EventType.Created));
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
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.txt", collection, domainName, FileEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed test");
			}

			// Check for a miss.
			args = null;
			publisher.RaiseFileEvent(new FileEventArgs("CollectionEventsTests", @"c:\path\file.doc", collection, domainName, FileEventArgs.EventType.Created));
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
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "123456789", collection, domainName, "Node", NodeEventArgs.EventType.Created));
			if (recievedCallback)
			{
				throw new ApplicationException("Failed disable");
			}

			// Check reenabled.
			args = null;
			subscriber.Enabled = true;
			publisher.RaiseNodeEvent(new NodeEventArgs("CollectionEventsTests", "123456789", collection, domainName, "Node", NodeEventArgs.EventType.Created));
			if (!recievedCallback)
			{
				throw new ApplicationException("Failed enable");
			}
		}

		/// <summary>
		/// Service Control Test.
		/// </summary>
		[Test]
		public void ServiceControlTest()
		{
			publisher.RaiseServiceEvent(new ServiceEventArgs(ServiceEventArgs.TargetAll, ServiceEventArgs.ServiceEvent.Shutdown));
			publisher.RaiseServiceEvent(new ServiceEventArgs(ServiceEventArgs.TargetAll, ServiceEventArgs.ServiceEvent.Reconfigure));
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
			Console.WriteLine("      where mode = P (Publish)");
			Console.WriteLine("      or    mode = S (Subscribe)");
			Console.WriteLine("      where event count = number of events to publish");
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
						t.publisher = new EventPublisher(t.conf, t.domainName);
						int count = Int32.Parse(args[1]);
						for (int i = 0; i < count; ++i)
						{
							t.publisher.RaiseNodeEvent(new NodeEventArgs("nifp", i.ToString(), t.collection, t.domainName, "Node", NodeEventArgs.EventType.Created));
						}
					}
					break;

				case "PS":
					t.publisher = new EventPublisher(t.conf, t.domainName);
					t.publisher.RaiseServiceEvent(new ServiceEventArgs(ServiceEventArgs.TargetAll, ServiceEventArgs.ServiceEvent.Shutdown));
					break;

				case "S":
					t.PublishSubscribe();
					t.shutdownEvent.WaitOne();
					t.subscriber.Dispose();
					break;

				default:
					usage();
					break;
			}
		}
		
		#endregion
	}
}
