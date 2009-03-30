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
 ***********************************************************************/

using System;

using Simias;
using Simias.Storage;

namespace Simias.Storage.Test
{
    /// <summary>
    /// Class used to test the collection owner functionality.
    /// </summary>
    public class CollectionOwnerTests
    {
        #region Class Members

        private Store store;

        #endregion

        #region Properties
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="store">Handle to the store.</param>
        public CollectionOwnerTests(Store store)
        {
            this.store = store;
        }

        #endregion

        #region Private Methods

        private void NoOwnerTest()
        {
            Collection c = new Collection(store, "NoOwnerTest", store.LocalDomain);
            c.Proxy = true;

            try
            {
                c.Commit();
                throw new ApplicationException("Collection was created without an owner.");
            }
            catch (SimiasException)
            { }
        }

        private void DeleteOwnerTest()
        {
            Collection c = new Collection(store, "DeleteOwnerTest", store.LocalDomain);
            c.Commit();

            try
            {
                try
                {
                    c.Commit(c.Delete(c.Owner));
                    throw new ApplicationException("Collection owner was deleted.");
                }
                catch (SimiasException)
                { }
            }
            finally
            {
                c.Commit(c.Delete());
            }
        }

        private void MultiOwnerTest()
        {
            Collection c = new Collection(store, "MultiOwnerTest", store.LocalDomain);
            c.Commit();

            try
            {
                Member member = new Member("DuplicateMember", Guid.NewGuid().ToString(), Access.Rights.ReadOnly);
                member.IsOwner = true;

                try
                {
                    c.Commit(member);
                    throw new ApplicationException("More than one collection owner exists.");
                }
                catch (SimiasException)
                { }
            }
            finally
            {
                c.Commit(c.Delete());
            }
        }

        private void ChangeOwnerTest()
        {
            Collection c = new Collection(store, "ChangeOwnerTest", store.LocalDomain);
            c.Commit();

            try
            {
                Member member = new Member("NewOwner", Guid.NewGuid().ToString(), Access.Rights.ReadOnly);
                c.Commit(member);
                c.Commit(c.ChangeOwner(member, Access.Rights.ReadOnly));
            }
            finally
            {
                c.Commit(c.Delete());
            }
        }

        private void ChangeOwnerRights()
        {
            Collection c = new Collection(store, "ChangeOwnerRightsTest", store.LocalDomain);
            c.Commit();

            try
            {
                Member owner = c.Owner;
                owner.Rights = Access.Rights.ReadOnly;
                try
                {
                    c.Commit(owner);
                    throw new ApplicationException("Downgraded collection owner's rights.");
                }
                catch (SimiasException)
                { }
            }
            finally
            {
                c.Commit(c.Delete());
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Runs the collection owner tests.
        /// </summary>
        public void RunTests()
        {
            // Create a collection without an owner.
            NoOwnerTest();

            // Delete the collection owner.
            DeleteOwnerTest();

            // Create more than one collection owner.
            MultiOwnerTest();

            // Change the collection owner.
            ChangeOwnerTest();

            // Try and change the owner rights.
            ChangeOwnerRights();
        }

        #endregion
    }
}
