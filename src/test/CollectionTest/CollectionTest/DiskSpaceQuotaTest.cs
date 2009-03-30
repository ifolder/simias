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

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Handle to the store.</param>
        public DiskSpaceQuotaTest(Store store)
		{
			this.store = store;
		}

		#endregion

		#region Private Methods

		private Collection CreateCollectionWithFiles( long size )
		{
			Collection c = new Collection( store, "DiskSpaceQuotaTest", store.LocalDomain );
			DirNode dn = new DirNode( c, Directory.GetCurrentDirectory() );
			FileNode fn = new FileNode( c, dn, Path.GetTempFileName() );
			fn.Length = size;
			c.Commit( new Node[] { c, dn, fn } );
			return c;
		}

		private void DomainQuotaTest()
		{
			Collection c = CreateCollectionWithFiles( 1024 );

			try
			{
				DiskSpaceQuota.Create( store.LocalDomain, 2048 );
				if ( DiskSpaceQuota.GetLimit( store.LocalDomain ) != 2048 )
				{
					throw new ApplicationException( "Domain disk quota not set." );
				}

				DiskSpaceQuota dsq = DiskSpaceQuota.Get( c.GetCurrentMember() );
				if ( dsq.Limit != 2048 )
				{
					throw new ApplicationException( "Aggregate domain disk quota not set." );
				}

				if ( dsq.Allowed( 1024 ) == false )
				{
					throw new ApplicationException( "Domain disk quota failed." );
				}

				if ( dsq.Allowed( 1 ) == true )
				{
					throw new ApplicationException( "Domain disk quota failed." );
				}
			}
			finally
			{
				c.Commit( c.Delete() );
			}
		}

		private void MemberQuotaTest()
		{
			Collection c = CreateCollectionWithFiles( 1024 );

			try
			{
				Member member = c.GetCurrentMember();
				POBox.POBox.GetPOBox( store, store.LocalDomain, member.UserID );

				DiskSpaceQuota.Create( member, 2048 );
				if ( DiskSpaceQuota.GetLimit( member ) != 2048 )
				{
					throw new ApplicationException( "Member disk quota not set." );
				}

				DiskSpaceQuota dsq = DiskSpaceQuota.Get( member );
				if ( dsq.Limit != 2048 )
				{
					throw new ApplicationException( "Aggregate member disk quota not set." );
				}

				if ( dsq.Allowed( 1024 ) == false )
				{
					throw new ApplicationException( "Member disk quota failed." );
				}

				if ( dsq.Allowed( 1 ) == true )
				{
					throw new ApplicationException( "Member disk quota failed." );
				}
			}
			finally
			{
				c.Commit( c.Delete() );
			}
		}

		private void CollectionQuotaTest()
		{
			Collection c = CreateCollectionWithFiles( 1024 );

			try
			{
				DiskSpaceQuota.Create( c, 2048 );
				if ( DiskSpaceQuota.GetLimit( c ) != 2048 )
				{
					throw new ApplicationException( "Collection disk quota not set." );
				}

				DiskSpaceQuota dsq = DiskSpaceQuota.Get( c );
				if ( dsq.Limit != 2048 )
				{
					throw new ApplicationException( "Aggregate collection disk quota not set." );
				}

				if ( dsq.Allowed( 1024 ) == false )
				{
					throw new ApplicationException( "Collection disk quota failed." );
				}

				if ( dsq.Allowed( 1 ) == true )
				{
					throw new ApplicationException( "Collection disk quota failed." );
				}
			}
			finally
			{
				c.Commit( c.Delete() );
			}
		}

		private void AggregateQuotaTest()
		{
			Collection c1 = CreateCollectionWithFiles( 1024 );
			Collection c2 = CreateCollectionWithFiles( 0 );

			try
			{
				DiskSpaceQuota.Create( store.LocalDomain, 2048 );
				DiskSpaceQuota.Create( c2, 256 );

				DiskSpaceQuota cdsq = DiskSpaceQuota.Get( c2 );
				if ( cdsq.Allowed( 128 ) == false )
				{
					throw new ApplicationException( "Collection disk space quota failed." );
				}

				if ( cdsq.Allowed( 129 ) == true )
				{
					throw new ApplicationException( "Collection disk space quota failed." );
				}

				DiskSpaceQuota dsq = DiskSpaceQuota.Get( c2.GetCurrentMember() );
				if ( dsq.Allowed( 1024 ) == false )
				{
					throw new ApplicationException( "Member disk quota failed." );
				}
			}
			finally
			{
				c2.Commit( c2.Delete() );
				c1.Commit( c1.Delete() );
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Runs the collection owner tests.
		/// </summary>
		public void RunTests()
		{
			DomainQuotaTest();
			MemberQuotaTest();
			CollectionQuotaTest();
			AggregateQuotaTest();
		}

		#endregion


    }
    
}
