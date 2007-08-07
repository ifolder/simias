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
 |   Author: Ravi Kumar M <rkumar1@novell.com>
 |***************************************************************************/
 
#if MONO
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using System.Text;
using Mono.Unix;

using Simias;
using Simias.Client;
using Simias.Event;
using Simias.Policy;
using Simias.Storage.Provider;
using Simias.Sync;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage 
{
        /// <summary>
        /// This is the top level object for the Collection Store.  The Store object can contain multiple
        /// collection objects.
        /// </summary>
        public class DataStore : IComparable
	{
		#region class members
		/// <summary>
                /// Object used to store free space available on Mount point.
                /// </summary>
                public long AvailableFreeSpace;

                /// <summary>
                /// Used to log messages.
                /// </summary>
                static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );

                /// <summary>
                /// Object used to store the name of the Unmanaged path.
                /// </summary>
                public string DataPath;

		/// <summary>
                /// Object used to store the full path of the Unmanaged Path.
                /// </summary>
                public string FullPath;

		/// <summary>
                /// Object used to store the status of datapaths.
                /// </summary>
                public bool Enabled;

		#endregion

		#region Properties
		
		///<summary>
                ///Constructor
                ///</summary>
                public DataStore(string datapath,string fullpath,string enabled)
		{
			try
			{
				this.DataPath = datapath;
				this.FullPath = fullpath;
				string enable = "True";
				UnixDriveInfo mntpt = new UnixDriveInfo(this.FullPath);
				this.AvailableFreeSpace =  mntpt.AvailableFreeSpace;
				if( enabled.Equals( enable ) )
					this.Enabled = true;
				else
					this.Enabled = false;
			}
			catch
			{
				//Ignore if the Volume is not mounted.
			}
		}

		///<summary>
                ///Constructor
                ///</summary>
                public DataStore(string datapath,string fullpath,bool enabled)
                {
                        try
                        {
                                this.DataPath = datapath;
                                this.FullPath = fullpath;
                                UnixDriveInfo mntpt = new UnixDriveInfo(this.FullPath);
                                this.AvailableFreeSpace =  mntpt.AvailableFreeSpace;
                                this.Enabled = enabled;
                        }
                        catch
                        {
                                //Ignore if the Volume is not mounted.
                        }
                }

		
		///<summary>
                ///Constructor
                ///</summary>
		public DataStore()
		{
		}		
		
		///<summary>
                ///Constructor
                ///</summary>
                public DataStore(string datapath)
                {
			Store store = Store.GetStore();
			this.DataPath = datapath;
			this.FullPath = Store.StorePath;
			UnixDriveInfo mntpt = new UnixDriveInfo(this.FullPath);
                        this.AvailableFreeSpace =  mntpt.AvailableFreeSpace;
			this.Enabled = true;
                }

                ///<summary>
                ///Implementing CompareTo
                ///</summary>
                public int CompareTo(Object obj)
                {
                        DataStore compare = (DataStore)obj;
                        int result = this.AvailableFreeSpace.CompareTo(compare.AvailableFreeSpace);
                        return result;
                }

		/// <summary>
                /// Add a data store for an iFolder Server.
                /// </summary>
                /// <returns>Bool true on success.</returns>
		public bool AddStore(string ServerID)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.DefaultDomain );
			string storepath = Store.StorePath;
			string tmppath = Path.Combine(storepath,"SimiasFiles");
			tmppath = Path.Combine(tmppath,this.DataPath);
			if( Directory.Exists( tmppath ) == true )
				return false;
			if( Directory.Exists( FullPath ) != true )
				Directory.CreateDirectory(this.FullPath);
			Mono.Posix.Syscall.symlink(this.FullPath,tmppath);

			string storageFormat = String.Format( "{0}|{1}", DataPath, FullPath);
			storageFormat = String.Format( "{0}|{1}",storageFormat,this.Enabled.ToString());
			HostNode host = HostNode.GetLocalHost();
			host.Properties.AddProperty( PropertyTags.DataPath, storageFormat );
			domain.Commit(host);
			return true;
		}

		/// <summary>
                /// Modify data store for an iFolder Server.
                /// </summary>
                /// <param name="name">The name of the data store.</param>
                /// <returns>Bool true on success.</returns>
                public bool ModifyStore(string datapath, bool enabled)
                {
			HostNode host = HostNode.GetLocalHost();
                        MultiValuedList mv = host.Properties.GetProperties( PropertyTags.DataPath );

			foreach( Property prop in mv )
                        {
                                string[] comps = ( (string) prop.Value ).Split( '|' );
                                if( (datapath.Equals(comps[0])))
                                {
					Store store = Store.GetStore();
                                        Domain domain = store.GetDomain( store.DefaultDomain );

					prop.Delete();
					domain.Commit(host);
                                        string storageFormat = String.Format( "{0}|{1}", comps[0], comps[1]);
                                        storageFormat = String.Format( "{0}|{1}", storageFormat, enabled.ToString());
					host.Properties.AddProperty( PropertyTags.DataPath, storageFormat );
	                        	domain.Commit(host);	
				}
                        }

			return true;
                }


		/// <summary>
                /// Restrns the storepath based on the load on each volume
                /// </summary>
		/// <returns>Storepath</returns>				
		public static DataStore[] GetVolumes()
		{
			HostNode host = HostNode.GetLocalHost();
			if( host != null )
			{
				MultiValuedList mv = host.Properties.GetProperties( PropertyTags.DataPath );
	                        ArrayList VolumeList = new ArrayList();
        	                int count = 1;
                	        if( mv != null )
                        	{
	                                foreach( Property prop in mv )
        	                        {
                	                        VolumeList.Add(prop.Value);
                        	        }
	                        }	
        	                string[] stringArray = new string[VolumeList.Count];
                	        VolumeList.CopyTo(stringArray);
                        	DataStore[] DataStoreArray = new DataStore[VolumeList.Count+1];
	                        DataStoreArray[0] = new DataStore("Default-Store");
        	                foreach(string a in stringArray)
                	        {
					log.Info(a,"j");
                        	        string[] comps = a.Split( '|' );
                                	DataStoreArray[ count ] = new DataStore(comps[0],comps[1],comps[2]);
	                                count++;
        	                }
                	        return DataStoreArray;
			}
			else
			{
				
				DataStore[] DataStoreArray = new DataStore[1];
				DataStoreArray[0] = new DataStore("Default-Store");	
				return DataStoreArray;
			}
		}
		#endregion
       	}
		
}
#endif
