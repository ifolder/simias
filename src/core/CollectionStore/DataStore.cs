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
*
*                 $Author: Ravi Kumar M <rkumar1@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
****************************************************************************/
 
#if MONO
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
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

		/// </summary>
                private const string apacheUser = "wwwrun";

		/// </summary>
                private const string apacheGroup = "www";



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
		public int AddStore(string ServerID)
		{
			Store store = Store.GetStore();
			Domain domain = store.GetDomain( store.DefaultDomain );
			string storepath = Store.StorePath;
			string tmppath = Path.Combine(storepath,"SimiasFiles");
			tmppath = Path.Combine(tmppath,this.DataPath);
			int result = 0;
			if( Directory.Exists( tmppath ) == true )
				return 1;
			else if( Directory.Exists( this.FullPath ) != true )
				return 2;

			Mono.Posix.Syscall.symlink(this.FullPath,tmppath);

			string storageFormat = String.Format( "{0}|{1}", this.DataPath, this.FullPath);
			storageFormat = String.Format( "{0}|{1}",storageFormat,this.Enabled.ToString());
			HostNode host = HostNode.GetLocalHost();
			Property p = new Property( PropertyTags.DataPath, storageFormat );
			p.LocalProperty = true;
			host.Properties.AddProperty( p ); 
			domain.Commit(host);
			return result;
		}

		/// <summary>
                /// Modify data store for an iFolder Server.
                /// </summary>
                /// <param name="name">The name of the data store.</param>
                /// <returns>Bool true on success.</returns>
                public bool ModifyStore( string datapath, bool enabled )
                {
			log.Debug(" Modify store called - :  {0}", datapath );
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
                /// Delete data store for an iFolder Server.
                /// </summary>
                /// <param name="name">The name of the data store.</param>
                /// <returns>Bool true on success.</returns>
                public bool DeleteStore( string datapathname )
                {
			log.Debug(" Delete Store called with : {0}", datapathname );
			HostNode host = HostNode.GetLocalHost();
                        MultiValuedList mv = host.Properties.GetProperties( PropertyTags.DataPath );

                        foreach( Property prop in mv )
                        {
                                string[] comps = ( (string) prop.Value ).Split( '|' );
                                if( (datapathname.Equals(comps[0])))
                                {

					Store store = Store.GetStore();
		                        Domain domain = store.GetDomain( store.DefaultDomain );

					if( ! Directory.Exists( Path.Combine(comps[1],"SimiasFiles" ) ) )
					{
						prop.Delete();
                                        	domain.Commit(host);

						//delete the source link of the symbolic link created
						string storepath = Store.StorePath;
			                        string tmppath = Path.Combine(storepath,"SimiasFiles");
                        			tmppath = Path.Combine(tmppath,datapathname);
						Mono.Posix.Syscall.unlink(tmppath);
					}
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

		/// <summary>
                /// Execute the command in the shell.
                /// </summary>
                /// <param name="command">The command.</param>
                /// <param name="format">The arguments of the command.</param>
                /// <param name="args">The arguments for the format.</param>
                /// <returns>The results of the command.</returns>
                static int Execute(string command, string format, params object[] args)
                {
                        ProcessStartInfo info = new ProcessStartInfo( command, String.Format( format, args ) );                        Process p = Process.Start(info);
                        p.WaitForExit();
                        return p.ExitCode;
                }

		#endregion
       	}
		
}
#endif
