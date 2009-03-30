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
    [TestFixture]
    public class CollectionStoreTests : CollectionTest.ICollectionStoreTests
    {
        #region data members 
        // Object used to access the store.
        private Store store = null;
        private Service.Manager manager = null;
        #endregion 


        #region Test Setup
        /// <summary>
        /// Performs pre-initialization tasks.
        /// </summary>
        [TestFixtureSetUp]
        public void Init()
        {
            // Connect to the store.
            Console.WriteLine("TestFixtureSetup");
            SimiasSetup.prefix = @"C:\iFolder_stage\";
            Store.Initialize(@"C:\iFolder_stage\lib\simias", false, 35550);
            store = Store.GetStore();
            manager = Simias.Service.Manager.GetManager();
            manager.StartServices();
            manager.WaitForServicesStarted();
            
            Console.WriteLine("ID"+store.ID);
            Console.WriteLine("Local Domain"+store.LocalDomain);
            Console.WriteLine("Test Fixture Complete!");
        }
        #endregion
        
       #region testcases
       [Test]
        public void CreateCollectionTest()
        {
            // Create a new collection and remember its ID. Use all the special XML characters in the name.
            Collection collection = new Collection(store,"&<>\"\'CS_TestCollection", store.LocalDomain);
           // Remember the id for later.
            string ID = collection.ID;
            Console.WriteLine("ID: "+ID.ToUpper());
            Console.WriteLine("Collection Name: "+collection.Name);
            Console.WriteLine("Collection Domain: "+ collection.Domain);
            Domain domain = store.GetDomain(collection.Domain);
            collection.HostID = domain.HostID;
            Console.WriteLine("HOst ID:", collection.HostID);

            try
            {
                // Now commit it to the store.
                collection.CreateMaster = false;
                collection.Commit();

                // Make sure the collection exists.
                if (store.GetCollectionByID(ID) == null)
                {
                    throw new ApplicationException("Collection was committed but does not exist in the store");
                }

                // This was added to make sure that we handle all the XML special characters in the ShallowNode list.
                foreach (ShallowNode sn in collection)
                {
                    if ((sn.ID == collection.ID) && (sn.Name != collection.Name))
                    {
                        throw new ApplicationException("Special XML characters not being handled properly.");
                    }
                }
            }
            finally
            {
                // Delete the collection.
                collection.Commit(collection.Delete());
                if (store.GetCollectionByID(ID) != null)
                {
                    throw new ApplicationException("Collection object not deleted");
                }
            }
        }

        /// <summary>
        /// Create a collection and adds a child node to the collection.
        /// </summary>
        [Test]
        public void CreateChildNodeTest()
        {
            // Create the collection.
            Collection collection = new Collection(store, "CS_TestCollection", store.LocalDomain);
            try
            {
                // Create a node subordinate to this collection.
                Node child = new Node("CS_ChildNode");

                // Add a relationship that will reference the parent Node.
                Relationship parentChild = new Relationship(collection.ID, collection.ID);
                child.Properties.AddProperty("MyParent", parentChild);

                // Commit this collection.
                Node[] commitList = { collection, child };
                collection.Commit(commitList);

                // Search this collection for this child.
                bool foundChild = false;
                ICSList results = collection.Search("MyParent", parentChild);
                foreach (ShallowNode shallowNode in results)
                {
                    Node node = new Node(collection, shallowNode);
                    if (node.ID == child.ID)
                    {
                        foundChild = true;
                        break;
                    }
                }

                // Make sure the child was found.
                if (!foundChild)
                {
                    throw new ApplicationException("CreateChildNode: Hierarchical linkage failure");
                }

                // Delete the child node and then delete the tombstone.
                collection.Commit(collection.Delete(child));

                // See if the child node still exists.
                if (collection.GetSingleNodeByName("CS_ChildNode") != null)
                {
                    throw new ApplicationException("Child node not deleted.");
                }
            }
            finally
            {
                // Get rid of this collection.
                collection.Commit(collection.Delete());
            }
        }
       #endregion
       #region Test Clean Up
        /// <summary>
        /// Clean up for tests.
        /// </summary>
        [TestFixtureTearDown]
        public void Cleanup()
        {
            // Stop the services.
            manager.StopServices();
            manager.WaitForServicesStopped();

            // Delete the database.  Must be store owner to delete the database.
            store.Delete();

            // Remove the created directory.
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "CollectionStoreTestDir");
            if (Directory.Exists(dirPath))
            {
                //				Directory.Delete( dirPath, true );
            }
        }
        #endregion
    }
}
