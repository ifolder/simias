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
*                 $Author: Ramesh Sunder <sramesh@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Web;
using System.Web.Services.Protocols;
using xmlhandling;
using Novell.iFolder.Utility;
using Certificate;

namespace Restore
{

        /// <summary>
        /// Restore Tool return status
        /// </summary>
     	public enum status
	{
 		/// <summary>
                /// Tool run for Help
                /// </summary>
                Help=99,
                /// <summary>
                /// Operation is sucessfull
                /// </summary>
		Success=100,
		/// <summary>
		/// Opearation failed
		/// </summary>
		Failed,
		/// <summary>
		/// Operation failed due to  Invalid Input
		/// </summary>
		InvalidInput,
		/// <summary>
		/// Operation failed due to Invalid Format used for Passing Argument
		/// </summary>
		InvalidFormat,
		/// <summary>
		/// One or more required feilds are missing.
		/// </summary>
		MissingFields,
		/// <summary>
		/// Operation failed due to Invalid iFolder Datapath
		/// </summary>
		DataPathDoesNotExist,
		/// <summary>
		/// Backup server failed to start, killed, stoped or inaccessable
		/// </summary>
		BackupServerNotRunning,
		/// <summary>
		/// Current server failed to start, killed, stoped or inaccessable
		/// </summary>
		CurrentServerNotRunning,
		/// <summary>
		/// Invalid input Credential
		/// </summary>
		InvalidCredentials,
		/// <summary>
		/// Input Admin ID is null or Empty string
		/// </summary>
		EmptyAdminID,
		/// <summary>
		/// Input iFolderId is invalid or doesn't exist in old store
		/// </summary>
		InvalidiFolderIdOld,
		/// <summary>
		/// Tool run for Listing iFolder for given user
		/// </summary>
		ListingiFolder,
		/// <summary>
		///Operation failed due to failure in creating iFolder, in case of full ifolder restore
		/// </summary>
		FailediFolderCreation,
		/// <summary>
		/// Backup store doesn't exist, Invalid backup path
		/// </summary>
		BackupExistenceFailed,
		/// <summary>
		/// iFolder ID doesn't exist in current store, in case of partial restore
		/// </summary>
		InvalidiFolderIdNew,
		/// <summary>
		/// Relative path for restore is invalid or doesn't exist
		/// </summary>
		InvalidRelativePath,
		/// <summary>
		/// XML file doesn't exist, in case of Retry opearation
		/// </summary>
		InvalidXMLFileLocation,
		/// <summary>
		/// Web Call failed
		/// </summary>
		FailedRestoreWebCall,
		/// <summary>
		/// Directory creation failed
		/// </summary>
		DirCreationFailed,
		/// <summary>
		/// Operation failed while uploading file
		/// </summary>
		FileUploadFailed,
		/// <summary>
		/// Operation failed while writting XML
		/// </summary>
		XmlWrittingFailed,
		/// <summary>
		/// Input Username for listing ifolder doesn't exist
		/// </summary>
		InvalidUserName,
		/// <summary>
		/// Input current server IP is empty
		/// </summary>
		EmptyNewServerUrl,
		/// <summary>
		/// Failed to restore iFolder policy
		/// </summary>
		PolicyRestoreFailed,
		/// <summary>
        /// Invalid Path Specified 
        /// </summary>
          InvalidBackupPath,
	}

	public enum Command
	{
		Help=1,
		List,
		Restore,
		Retry,
	}


	class iFolderServer 
	{
		int MaxCount = 3;
		string PublicUrl;
		public string adminName;
		public string adminPassword ;
		public iFolderAdmin admin;
		public iFolderWeb web;
		public SimiasWebService simws;
        public string adminNameForAuth;
        public string adminPasswordForAuth;
	
		public iFolderServer(string url, string adminUserName, string password): this( url, adminUserName, password, true) 
		{
		}
	
		/// <summary>
		/// Initializes the iFolderServer object. 
		/// </summary>
		/// <param name="url">ifolder server url</param>
		/// <param name="adminUserName">The user name for authentication.</param>
		/// <param name="password">The password for authentication.</param>
		/// <param name="redirect"></param>
        /// <returns>.</returns>
		public iFolderServer(string url, string adminUserName, string password, bool redirect)
		{
			PublicUrl = url;
			adminName = adminUserName;
			adminPassword = password;
			adminNameForAuth = adminUserName;
			adminPasswordForAuth = password;

			web = new iFolderWeb();
			web.Credentials = new NetworkCredential(this.adminNameForAuth, this.adminPasswordForAuth);
			web.Url = this.PublicUrl + "/iFolderWeb.asmx";
			web.PreAuthenticate = true;

            if( !this.CheckCredentials() )
            {
                    MainClass.DebugLog.Write(string.Format("Credentials are not authorized. Trying with multibyte encoding."));
                    /// Change the admin name and password for MultiByte encoding...
                    UTF8Encoding utf8Name = new UTF8Encoding();
                    byte[] EncodedCredInByte = utf8Name.GetBytes(adminName);
                    adminNameForAuth = Convert.ToBase64String(EncodedCredInByte);
                    EncodedCredInByte = utf8Name.GetBytes(adminPassword);
                    adminPasswordForAuth =  Convert.ToBase64String(EncodedCredInByte);
                    web.Credentials = new NetworkCredential(this.adminNameForAuth, this.adminPasswordForAuth);

                    if( !this.CheckCredentials() )
                    {
                            MainClass.DebugLog.Write(string.Format("Error validating credentials for admin. May be Invalid credentials. "));
                    }
                    else
                            MainClass.DebugLog.Write(string.Format("Authorized with multibyte encoding..."));
            }

			MainClass.DebugLog.Write(string.Format("The public url is: {0}", this.PublicUrl));
			if( redirect )
			{
				try
				{
					String RedirectedUrl = this.GetHomeServer(adminUserName);
					if( ! String.IsNullOrEmpty(RedirectedUrl))
					{
						this.PublicUrl = RedirectedUrl;
					}
				}
				catch(Exception ex1)
				{
					MainClass.DebugLog.Write(string.Format("Exception: while getting homeServer for admin. May be Invalid credentials. {0}", ex1.Message));
					/*TBD: Can we continue to restore in case if this exception occurs? 
					 * Or should the tool throw error and come out */
				}
			}
			MainClass.DebugLog.Write(string.Format("The public url is: {0}", this.PublicUrl));

			admin = new iFolderAdmin();
			admin.Credentials = new NetworkCredential(this.adminNameForAuth, this.adminPasswordForAuth);
			admin.Url = this.PublicUrl + "/iFolderAdmin.asmx";
			admin.PreAuthenticate = true;
			MainClass.DebugLog.Write(string.Format("iFolderAdmin object is created " ));
/*TBD: web is already initialized above. The following lines look invalid*/
			web = new iFolderWeb();
			web.Credentials = new NetworkCredential(this.adminNameForAuth, this.adminPasswordForAuth);
			web.Url = this.PublicUrl + "/iFolderWeb.asmx";
			web.PreAuthenticate = true;

			MainClass.DebugLog.Write(string.Format("iFolderWeb object is created " ));
			simws = new SimiasWebService();
			simws.Credentials = new NetworkCredential(this.adminNameForAuth, this.adminPasswordForAuth);
			simws.Url = this.PublicUrl + "/Simias.asmx";
			simws.PreAuthenticate = true;
			MainClass.DebugLog.Write(string.Format("SimiasWebService object is created " ));
			try
			{
				iFolderUser AdminUser = null;
				MainClass.DebugLog.Write(string.Format("Calling GetAutheticated User " ));
				AdminUser = GetAuthenticatedUser();
				if( AdminUser != null)
					adminName = AdminUser.UserName;
				else
				 MainClass.DebugLog.Write(string.Format("Failed to get Admin user using GetAuthenticatedUser"));
			}
			catch(Exception ex)
			{
				 MainClass.DebugLog.Write(string.Format("Exception: Authentication failed, message{0}--stackTrace:{1}",ex.Message, ex.StackTrace));
			}
		}

		public string AdminName
		{
			get
			{
				return this.adminName;
			}
		}

		/// <summary>
		/// Validates the credentials for the user. 
		/// </summary>
		/// <param ></param>
        /// <returns>true if valid credentials and false if the .</returns>
		public bool CheckCredentials()
		{
			bool status = false;
			int count =0;
			while (count <= MaxCount)
			{
				try
				{
					iFolderUser adminUser = this.GetAuthenticatedUser();
					if( adminUser != null)	
						status = true;
				 MainClass.DebugLog.Write(string.Format("GetAuthenticatedUser Success"));
					break;
				}
				catch(InvalidOperationException /*inOpEx*/) {
				 MainClass.DebugLog.Write(string.Format("GetAuthenticatedUser Failed with Invalid Op"));
					count++;
					continue;
				}
				catch(Exception /*ex*/)
				{
				 MainClass.DebugLog.Write(string.Format("GetAuthenticatedUser Failed with Exception"));
					status = false;
					break;
				}	
			}
			return status;
		}


		/// <summary>
        /// Check if the server is up
        /// </summary>
        /// <param >.</param>
        /// <returns>true if the server is up, false otherwise.</returns>
		public bool PingServer()
		{
			bool retVal = false;
			if( simws == null)
			{
				return false;
			}	
			int count =0;
			while (count <= MaxCount)
			{					
		        	try
                        	{
                                	simws.PingSimias();
					retVal = true;
					break;
                        	}
				catch(Exception ex)
				{
				
				 MainClass.DebugLog.Write(string.Format("Exception in Pingsimias: {0}--{1}",  ex.Message, ex.StackTrace));
                                      	count++;
				}	
			}
			return retVal;
		}
		/// <summary>
		/// Creates a new iFolder with the given name. 
		/// </summary>
		/// <param ></param>
        /// <returns>true if sucess and false otherwise .</returns>
		public bool CreateiFolderWithID(string iFolderName, string OwnerUserID, string description, string iFolderID)
		{
			bool status = false;
			try
			{
				iFolder ifolder=null ;
				int count=0;	
				while (count <= MaxCount)	
				{
					try
					{
						ifolder	= this.admin.CreateiFolderWithID(iFolderName, OwnerUserID, description, iFolderID);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
							count++;
							continue;	
					}
				}
				if( ifolder != null)
					status = true;
			}
			catch(Exception ex)
			{
				 MainClass.DebugLog.Write(string.Format("Exception: in creating iFolder with ID: {0}. {1}--{2}", iFolderID, ex.Message, ex.StackTrace));
				throw;
			}
			return status;
		}
		/// <summary>
		/// Creates a new encrypted iFolder with the given name. 
		/// </summary>
		/// <param ></param>
        /// <returns>true if sucess and false otherwise .</returns>
		public bool CreateEncryptediFolderWithID(string iFolderName, string OwnerUserID, string description, string iFolderID, string eKey, string eBlob, string eAlgorithm, string rKey)
		{
		 	MainClass.DebugLog.Write("Enter: Function CreateEncryptediFolderWithID");
			bool status = false;
			try
			{
				iFolder ifolder=null ;
				int count=0;	
				while (count <= MaxCount)	
				{
					try
					{
						ifolder	= this.admin.CreateEncryptediFolderWithID(iFolderName, OwnerUserID, description, iFolderID, eKey, eBlob, eAlgorithm, rKey);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
							count++;
							continue;	
					}
				}
				if( ifolder != null)
				{
					status = true;
				}
			}
			catch(Exception ex)
			{
				 MainClass.DebugLog.Write(string.Format("Exception in creating encrypted iFolder with ID: {0}. {1}--{2}", iFolderID, ex.Message, ex.StackTrace));
				throw;
			}
		 	MainClass.DebugLog.Write("Exit: Function CreateEncryptediFolderWithID");
			return status;
		}	
		/// <summary>
		/// Returns the Home Server for the given user. 
		/// </summary>
		/// <param ></param>
        /// <returns>true if sucess and false otherwise .</returns>
		public string GetHomeServer(string userName)
		{
		 	MainClass.DebugLog.Write("Enter: Function GetHomeServer");
			string homeServer = null;
			try
			{
				DomainService dom = new DomainService();
				dom.Url = this.PublicUrl + "/DomainService.asmx";
				dom.Credentials = new NetworkCredential(this.adminNameForAuth, this.adminPasswordForAuth);
				dom.PreAuthenticate = true;
				int count = 0;
				while( count < MaxCount )
				{
					try
					{
						homeServer = dom.GetHomeServer(userName).PublicAddress;
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			}
			catch(Exception ex)
			{
			      MainClass.DebugLog.Write(string.Format("Unable to get the home server for the user: {0}. {1}", userName, ex.Message));
			       homeServer = null;
			}
		 	MainClass.DebugLog.Write("Exit: Function GetHomeServer");
			return homeServer;
		}

		/// <summary>
		/// Returns the userId from the user name. 
		/// </summary>
		/// <param ></param>
        /// <returns>user id if success , null otherwise.</returns>
		public string GetUserIDFromName(string UserName)
		{
		    MainClass.DebugLog.Write("Enter: Function GetUserIDFromName");
	            iFolderUser user = null; 
		    try
		    {
	            if( admin == null)
	            {
	                    return null;
	            }
				iFolderUserSet users = null ;
				int count =0;
				while(count <= MaxCount)
				{
					try
					{
					   	users = this.admin.GetUsersBySearch(SearchProperty.UserName, SearchOperation.Equals, UserName, 0,1);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
							count++;
							continue;	
					}
				}
	            if( users == null || users.Items == null)
	            	return null;
	            user = users.Items[0];
		    }
		    catch(Exception ex)
		    {
		             MainClass.DebugLog.Write(string.Format("Error: Unable to get the user ID for user: {0}. {1}--{2}", UserName, ex.Message, ex.StackTrace));
		            return null;
		    }
		    MainClass.DebugLog.Write("Exit: Function GetUserIDFromName");
	            return user.ID;
		}

		/// <summary>
		/// Searches and returns the users. 
		/// </summary>
		/// <param ></param>
        /// <returns></returns>
		public iFolderUserSet GetUsersBySearch( int currentOffset, int count)
		{
		    	MainClass.DebugLog.Write("Enter: Function GetUsersBySearch");
			iFolderUserSet userList = null ;
			count = 0;
			while(count <= MaxCount)
			{
				try
				{
					userList = this.admin.GetUsersBySearch( 0, 0, "*", currentOffset, count );
					break;
				}
				catch(InvalidOperationException /*inOpEx*/)
				{
					count++;
					continue;	
				}
				catch( Exception /*ex*/)
				{
					break;
				}
			}
		    MainClass.DebugLog.Write("Exit: Function GetUsersBySearch");
			return userList;
		}

		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
		public string[] GetAllCollectionIDsByUser(string userID)
		{
		    	MainClass.DebugLog.Write("Enter: Function GetAllCollectionIDsByUser");
			string[] collections = null;
			DiscoveryService discService = new DiscoveryService();
			discService.Url = this.PublicUrl + "/DiscoveryService.asmx";
			discService.Credentials = new NetworkCredential(this.adminNameForAuth, this.adminPasswordForAuth );
			discService.PreAuthenticate = true;
			int count = 0;
			while(count <= MaxCount)
			{
				try
				{
					collections = discService.GetAllCollectionIDsByUser( userID );
					break;
				}
				catch(InvalidOperationException /*inOpEx*/)
				{
					count++;
					continue;	
				}
				catch( Exception /*ex*/)
				{
					break;
				}
			}
		    	MainClass.DebugLog.Write("Exit: Function GetAllCollectionIDsByUser");
			return collections;
		}
//TBD: Need to provide the correct comments
		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
		public iFolder GetiFolder( string iFolderID)
		{
		    	MainClass.DebugLog.Write("Enter: Function GetiFolder");
			iFolder folder = null;	
			int count =0;
			while (count < MaxCount)
			{
				try
				{
					folder= this.admin.GetiFolder(iFolderID);
					break;
				}
				catch(InvalidOperationException /*inOpEx*/)
				{
					count++;
					continue;	
				}
				catch( Exception /*ex*/)
				{
					break;
				}
			}
		    MainClass.DebugLog.Write("Exit: Function GetiFolder");
			return folder;
		}
//TBD: Need to provide the correct comments
		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
		public iFolderUserSet GetMembers(string iFolderID, int offset, int count)
		{
		    	MainClass.DebugLog.Write("Enter: Function GetMembers");
			iFolderUserSet users = null;
			try
			{
				count=0;
				while(count < MaxCount)
				{
					try
					{
						users = this.admin.GetMembers( iFolderID, offset, count );	
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			}
			catch(Exception ex)
			{
				 MainClass.DebugLog.Write(string.Format("Exception in GetMembers: {0}--{1}", ex.Message, ex.StackTrace));
				return null;
			}
		    	MainClass.DebugLog.Write("Exit: Function GetMembers");
			return users;
		}
//TBD: Need to provide the correct comments
		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
		public bool RemoveMember( string iFolderID, string UserName)
		{
		    MainClass.DebugLog.Write("Enter: Function RemoveMember");
			bool status = false;
			try
			{
				string UserID = GetUserIDFromName( UserName );
				int count =0;
				while (count < MaxCount)
				{
					try
					{
						this.admin.RemoveMember(iFolderID, UserID);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
							count++;
							continue;	
					}
				}
				status = true;
			}
			catch(Exception ex)
			{
				  MainClass.DebugLog.Write(string.Format("Exception while removing membership: {0}--{1}", ex.Message, ex.StackTrace));
			}
		    MainClass.DebugLog.Write("Exit: Function RemoveMember");
			return status;
		}
//TBD: Need to provide the correct comments
		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
		public string AddMember( string iFolderID, string UserName, Rights rights)
		{
		    	MainClass.DebugLog.Write("Enter: Function AddMember");
			string status = null;
			try
			{
				string UserID = this.GetUserIDFromName( UserName );
				int count =0;
				if( UserID != null)
				{
					while(count <= MaxCount)
					{	
						try
						{
							this.admin.AddMember(iFolderID, UserID, rights);
							break;
						}
						catch(InvalidOperationException /*inOpEx*/)
						{
							count++;
							continue;	
						}
					}
					MainClass.DebugLog.Write(string.Format("Successfully added member {0} to {1}", UserName, iFolderID));
					status = UserID;
				}
			}
			catch(Exception ex)
			{
				 MainClass.DebugLog.Write(string.Format("Exception while adding member {0} to iFolder: {1}. {2}--{3}", UserName, iFolderID, ex.Message, ex.StackTrace));
			}
		    	MainClass.DebugLog.Write("Exit: Function AddMember");
			return status;
		}		
//TBD: Need to provide the correct comments
		public iFolderEntry GetEntry(string ifolderid, string nodeid)
		{
			iFolderEntry entry = null;
			try
			{
				int count = 0;
				while (count < MaxCount)
				{
					try
					{
						entry = this.web.GetEntry(ifolderid, nodeid);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			}
			catch(Exception ex)
			{
				  MainClass.DebugLog.Write(string.Format("Exception while creating entry: {0}--{1}", ex.Message, ex.StackTrace));
			}
			return entry;
		}
//TBD: Need to provide the correct comments
		public iFolderEntrySet GetEntries(string ifolderid, string nodeid, int start, int end)
		{
			iFolderEntrySet entries = null;
			try
			{
				int count = 0;
				while (count < MaxCount)
				{
					try
					{
						entries = this.web.GetEntries(ifolderid, nodeid, start, end);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			}
			catch(Exception ex)
			{
				  MainClass.DebugLog.Write(string.Format("Exception while creating entry: {0}--{1}", ex.Message, ex.StackTrace));
			}
			return entries;
		}
//TBD: Need to provide the correct comments
		public iFolderDetails GetiFolderDetails(string ifolderid)
		{
			iFolderDetails ifolder = null;
			try
			{
				int count = 0;
				while (count < MaxCount)
				{
					try
					{
						ifolder = this.admin.GetiFolderDetails(ifolderid);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			}
			catch(Exception ex)
			{
				  MainClass.DebugLog.Write(string.Format("Exception while creating entry: {0}--{1}", ex.Message, ex.StackTrace));
			}
			return ifolder;
		}
//TBD: Need to provide the correct comments
		public iFolderUser GetAuthenticatedUser()
		{
			iFolderUser user = null;
			try
			{
				int count=0;
				while (count < MaxCount)	
				{
					try
					{		
						user= web.GetAuthenticatedUser();
						break;
					}
					catch(InvalidOperationException Ex)
					{
						
						MainClass.DebugLog.Write(string.Format("InvalidOperation Exception in GetAuthenticatedUser: {0}--{1}", Ex.Message, Ex.StackTrace));
						count++;
						continue;	
					}

				}
			}
			catch(Exception ex)
			{
				MainClass.DebugLog.Write(string.Format("Exception in GetAuthenticatedUser: {0}--{1}", ex.Message, ex.StackTrace));
			}
			return user;
		}
	
//TBD: Need to provide the correct comments
		public int SetRestoreStatusForCollection(string ifolderid, int restorestatus, int totalcount, int finishedcount)
		{
			int retval = -1;
			try
			{
				int count = 0;
				while (count < MaxCount)	
				{
					try
					{		
						retval= simws.SetRestoreStatusForCollection(ifolderid, restorestatus, totalcount, finishedcount);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}

			}
			catch(Exception ex)
			{
				MainClass.DebugLog.Write(string.Format("Exception in GetRestoreStatusForCollection: {0}--{1}", ex.Message, ex.StackTrace));
				retval = (int)status.FailedRestoreWebCall;
			}
			return retval;
		}


//TBD: Need to provide the correct comments
		public int GetRestoreStatusForCollection(string ifolderid, out int totalcount, out int finishedcount)
		{
			int retval = -1;
			totalcount = 0; 
			finishedcount = 0;
			try
			{
				int count = 0;
				while (count < MaxCount)	
				{
					try
					{		
						MainClass.DebugLog.Write(string.Format("Calling GetRestoreStatusForCollection for {0}", ifolderid));
						retval= admin.GetRestoreStatusForCollection(ifolderid, out totalcount, out finishedcount);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}

			}
			catch(Exception ex)
			{
				MainClass.DebugLog.Write(string.Format("Exception in GetRestoreStatusForCollection: {0}--{1}", ex.Message, ex.StackTrace));
				retval = (int)status.FailedRestoreWebCall;
			}
			return retval;
		}
//TBD: Need to provide the correct comments
		public iFolderEntry GetEntryByPath(string ifolderid, string parentdir)
		{
			iFolderEntry entry = null;
			try
			{
				int count = 0;
				while (count < MaxCount)
				{
					try
					{
						entry = this.web.GetEntryByPath(ifolderid, parentdir);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			}
			catch(Exception ex)
			{
				  MainClass.DebugLog.Write(string.Format("Exception while creating entry: {0}--{1}", ex.Message, ex.StackTrace));
			}
			return entry;
		}
//TBD: Need to provide the correct comments
		public iFolderEntry CreateEntry(string ifolderid, string ParentEntryID, iFolderEntryType type, string name)
		{
			iFolderEntry entry = null;
			try
			{
				int count = 0;
				while (count < MaxCount)
				{
					try
					{
						entry = this.web.CreateEntry(ifolderid, ParentEntryID, type, name);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			}
			catch(Exception ex)
			{
				  MainClass.DebugLog.Write(string.Format("Exception while creating entry: {0}--{1}", ex.Message, ex.StackTrace));
			}
			return entry;
		}
//TBD: Need to provide the correct comments
		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
		public bool CreateDirectory(string ifolderid, string relativepath)
		{
		    	MainClass.DebugLog.Write("Enter: Function CreateDirectory");
			string parentdir = Path.GetDirectoryName(relativepath);
			if( String.IsNullOrEmpty(parentdir))
			{
				MainClass.DebugLog.Write("Creating the directory node for root level node which will be created as part of iFolder creation. ");
				return true;
			}
			iFolderEntry NewEntry = null;
			try
			{
				MainClass.DebugLog.Write(string.Format("Function CreateDirectory:relativepath:{0} and parentdir:{1} ",relativepath,parentdir));
				iFolderEntry ParentEntry = this.GetEntryByPath(ifolderid, parentdir);
				if( ParentEntry == null)
				{
					MainClass.DebugLog.Write("The parent entry is null.");
					return false;
				}
				else
					MainClass.DebugLog.Write(string.Format("The parent entry is:{0} ", ParentEntry.ID ));

				NewEntry = this.CreateEntry(ifolderid, ParentEntry.ID, iFolderEntryType.Directory, Path.GetFileName( relativepath ));
				if( NewEntry == null)
					NewEntry = this.GetEntryByPath(ifolderid, relativepath);
			}
			catch(Exception ex)
			{
				NewEntry = this.GetEntryByPath(ifolderid, relativepath);
				MainClass.DebugLog.Write(string.Format("Exception: while creating Directory:{0}--{1}",ex.Message,ex.StackTrace));
			}	
			if(NewEntry == null)		
			{
				MainClass.DebugLog.Write("Directory entry creation failed");
				return false;
			}

		    	MainClass.DebugLog.Write("Exit: Function CreateDirectory");
			return true;

		}


//TBD: Need to provide the correct comments
		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
		public bool  UploadFile( string iFolderID, string fileName, string filePath, long NodeLength)
		{
		    	MainClass.DebugLog.Write("Enter: Function UploadFile");
			bool retValue = true;
			string OnlyFileName = fileName;
			int ind = fileName.LastIndexOf('/');
			if( ind >=0 && ind != fileName.Length-1)
			       OnlyFileName = fileName.Substring(ind+1);
			const int BUFFERSIZE = (16 * 1024);
			UriBuilder uri = new UriBuilder( this.PublicUrl);
			filePath = Path.GetDirectoryName( filePath );

			string ConvertedPath = filePath;
			string ConvertedFilename = OnlyFileName;
			// even index should contain the string which has to be replaced , and next index should contain the new string
			// maintain the same info in file src/webservices/FileHandler.cs
			string [] ConversionTable = {"&", "amp@:quot"};

			for(int index=0 ; index < ConversionTable.Length ; index+=2)
			{
		       ConvertedPath = ConvertedPath.Replace(ConversionTable[index], ConversionTable[index+1]);
		       ConvertedFilename = ConvertedFilename.Replace(ConversionTable[index], ConversionTable[index+1]);
			}
			string path = string.Format( "{0}/{1}", ConvertedPath, ConvertedFilename);
			MainClass.DebugLog.Write(string.Format("The actual path:{0} ", path));
			MainClass.DebugLog.Write(string.Format("The absolute path:{0} relativepath:{1} , and file name:{2} ",fileName,ConvertedPath,ConvertedFilename));
			FileStream fs = null;
			try
			{
				fs = new FileStream(fileName, FileMode.Open);
			}
			catch( Exception ex)
			{
				MainClass.DebugLog.Write(string.Format("Unable to open the file: {0}--{1}", fileName, ex.Message));
				retValue = false;
			}
			
			if(retValue != false && fs != null)
			{
				uri.Path = String.Format("/simias10/Upload.ashx?iFolder={0}&Path={1}&Length={2}&DontCheckPolicies=true&NodeLength={3}",
						iFolderID, path, fs.Length.ToString(), NodeLength);

				HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(uri.Uri);
				webRequest.Method = "PUT";
				int count = 0;
				webRequest.ContentLength = fs.Length;

				webRequest.PreAuthenticate = true;
				webRequest.Credentials = new NetworkCredential( this.adminNameForAuth, this.adminPasswordForAuth); //web.Credentials;
				webRequest.AllowWriteStreamBuffering = false;
				Stream webStream = null;
				try
				{
					webStream = webRequest.GetRequestStream();
				}
				catch(Exception ex)
				{
					MainClass.DebugLog.Write(string.Format("Unable to write to the file {0} : {1}", fileName, ex.Message));
					retValue = false;
				}

				Stream stream = fs; //file.InputStream;
				//TODO: //verify below condition
				if(/*retValue != false && */ webStream != null)
				{
					try
					{
						byte[] buffer = new byte[BUFFERSIZE];

						while((count = stream.Read(buffer, 0, buffer.Length)) > 0)
						{
							webStream.Write(buffer, 0, count);
							webStream.Flush();
						}
					}
					catch(Exception ex)
					{
						MainClass.DebugLog.Write(string.Format("Exception-1: {0}--{1}", ex.Message, ex.StackTrace));
						retValue = false;
					}
					finally
					{
						webStream.Close();
						stream.Close();
					}

					// response
					try
					{
						webRequest.GetResponse().Close();
					}
					catch(Exception ex)
					{
						MainClass.DebugLog.Write(string.Format("Exception-2: {0}", ex.Message));
						retValue = false;
					}

				}

			}
		    	MainClass.DebugLog.Write("Exit: Function UploadFile");
			return retValue;
		}
//TBD: Need to provide the correct comments
		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
		public static string GetUnManagedPath( iFolderDetails ifolder )
		{
		    	MainClass.DebugLog.Write("Enter: Function GetUnManagedPath");
			if( ifolder == null)
				return null;
			string managedPath = ifolder.ManagedPath;
			string temppath = ifolder.UnManagedPath;
			temppath = Path.GetDirectoryName( temppath);
			temppath = Path.GetDirectoryName( temppath);
			temppath = Path.GetFileName( temppath);
			string UnManagedPath = null;
			DirectoryInfo dInfo = new DirectoryInfo( managedPath );
			UnManagedPath = dInfo.Parent.Parent.FullName;
			UnManagedPath = Path.Combine( UnManagedPath, "SimiasFiles");
			UnManagedPath = Path.Combine( UnManagedPath, temppath );
			UnManagedPath = Path.Combine( UnManagedPath, ifolder.ID );
			MainClass.DebugLog.Write(string.Format("The unmanaged path is {0}", UnManagedPath));
		    	MainClass.DebugLog.Write("Exit: Function GetUnManagedPath");
			return UnManagedPath;
		}

		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
		public bool WriteXMLDocument(string iFolderid,string entryID)
		{
			MainClass.DebugLog.Write(string.Format("Enter: Function WirteXMLDocument. ifolderID: {0} entry ID: {1}", iFolderid, entryID));
			bool retValue = false;	
			bool IsEntryNodeFile = false;
			iFolderEntry entryNode = GetEntry(iFolderid,entryID);
			if(entryNode != null)
			{
				if(entryNode.IsDirectory)
				{
					MainClass.xmlObj.addFileElement(iFolderid,entryNode.ID,entryNode.Name,"DirNode",entryNode.Path,entryNode.Size.ToString(),"Progress");
				}
				else
				{
					IsEntryNodeFile = true;
					MainClass.xmlObj.addFileElement(iFolderid,entryNode.ID,entryNode.Name,"FileNode",entryNode.Path,entryNode.Size.ToString(),"Progress");
				}
				//Itterate only if, entryID is directroy
				if(!IsEntryNodeFile )
				{
					iFolderEntrySet entries = null;
					//-1 indicate to fetch record from index 0 till end
					entries = GetEntries(iFolderid, entryID, 0,-1);
					foreach(iFolderEntry child in entries.Items)
					{
						if(child.IsDirectory)
						{
							MainClass.xmlObj.addFileElement(iFolderid,child.ID,child.Name,"DirNode",child.Path,child.Size.ToString(),"Progress");
							WriteXMLDocument(iFolderid,child.ID);                               
						}
						else
						{
							MainClass.xmlObj.addFileElement(iFolderid,child.ID,child.Name,"FileNode",child.Path,child.Size.ToString(),"Progress");
						}
					}
				}
				retValue = true;
			}
				
		    	MainClass.DebugLog.Write("Exit: Function WriteXMLDocument");
			return retValue;	
		} //End of function WriteXMLDocument 


		
//TBD: Need to provide the correct comments
		public NodeEntrySet GetEntries(string ifolderID, int type, string relPath, int index, int max, string accessID)
		{
			int count = 0;
			NodeEntrySet entryset = null;
			do
			{
				try
				{
					entryset = this.simws.GetEntries(ifolderID, type, relPath, index, max, accessID);
				}
				catch(InvalidOperationException /*inOpEx*/)
				{
					count++;
					continue;	
				}
				break;
			}while(count < MaxCount);
			return entryset;
		}
//TBD: Need to provide the correct comments
		public int RestoreiFolderData(string url, string adminname, string adminpassword, string ifolderid, string relativepath, string basepath, int startindex)
		{
			int retval =-1;
			int count = 0;
			while( count < MaxCount)
			{
				try
				{
					MainClass.DebugLog.Write(string.Format("Calling RestoreiFolderData"));
					retval = this.admin.RestoreiFolderData(url, adminname, adminpassword, ifolderid, relativepath, basepath, startindex, MainClass.FailedLog.LogFile);
				}
				catch(InvalidOperationException /*inOpEx*/)
				{
					count++;
					continue;	
				}
				break;
			}
			return retval;
		}
//TBD: Need to provide the correct comments
		public int GetRestoreStatus(string ifolderid, out int totalcount, out int finishedcount)
		{
			int retval =-1;
			int count = 0;
			totalcount = 0;
			finishedcount = 0;
			do
			{
				try
				{
					MainClass.DebugLog.Write(string.Format("Calling GetRestoreStatusForCollection for {0}", ifolderid));
					retval = this.admin.GetRestoreStatusForCollection( ifolderid, out totalcount, out finishedcount);
				}
				catch(InvalidOperationException /*inOpEx*/)
				{
					count++;
					continue;	
				}
				break;
			}while(count < MaxCount);
			return retval;	
		}
		/// <summary>
        /// Invokes the GetiFolderLimitPolicyStatus method to check the status of iFolder limits
        /// </summary>
        /// <param name="userId"> ID of the User for whom policy needs to be checked.</param>
        /// 
        /// <returns> 0 if there are no violations 
        /// 		  1 in case of violations
        /// 		 -1 if there is any exception while execting the api .</returns>

		public int GetiFolderLimitPolicyStatus( string userId )
		{
			try	{
				int count = 0;
				while (count < MaxCount){
					try	{
						 return this.admin.GetiFolderLimitPolicyStatus(userId);
						
					} 					
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			} catch(Exception ex){
				  MainClass.DebugLog.Write(string.Format("Exception while checking the iFolder limit policy {0}--{1}", ex.Message, ex.StackTrace));
			}
			return -1;
		}
		
		/// <summary>
        /// Retrieves the User policy from the given User ID.
        /// </summary>
        /// <param name="userId"> ID of the User for whom policy needs to be retreived.</param>
        /// 
        /// <returns>returns UserPolicy object on Success and null on Failures.</returns>

		public UserPolicy GetUserPolicy( string userId )
		{
			UserPolicy usrPolicy = null;
			try	{
				int count = 0;
				while (count < MaxCount){
					try	{
						 return this.admin.GetUserPolicy(userId, null);
						
					} 					
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			} catch(Exception ex){
				  MainClass.DebugLog.Write(string.Format("Exception while fetching User policy {0}--{1}", ex.Message, ex.StackTrace));
			}
			return usrPolicy;
		}
        /// Retrieves the iFolder policy from the given iFolder ID.
        /// </summary>
        /// <param name="iFolderID"> ID of the iFolder for which policy needs to be retreived.</param>
        /// 
        /// <returns>returns iFolderPolicy object on Success and null on Failures.</returns>

		public iFolderPolicy GetiFolderPolicy( string ifolderid )
		{
			iFolderPolicy ifdPolicy = null;
			try
			{
				int count = 0;
				while (count < MaxCount)
				{
					try
					{
						 return this.admin.GetiFolderPolicy(ifolderid, null);
						
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			}
			catch(Exception ex)
			{
				  MainClass.DebugLog.Write(string.Format("Exception while fetching iFolder policies {0}--{1}", ex.Message, ex.StackTrace));
			}
			return ifdPolicy;
		}
		
		/// <summary>
        /// Set the iFolder policy from the given iFolder.
        /// </summary>
        /// <param name="ifdPolicy"> iFolder policy that needs to be updated on the server.</param>
        /// 
        /// <returns>returns 0 on Success and PolicyRestoreFailed on Failures.</returns>

		public int SetiFolderPolicy(iFolderPolicy ifdPolicy )
		{
			int retval = 0;
			try
			{
				int count = 0;
				while (count < MaxCount)
				{
					try
					{
						this.admin.SetiFolderPolicy(ifdPolicy);
						break;
					}
					catch(InvalidOperationException /*inOpEx*/)
					{
						count++;
						continue;	
					}
				}
			}
			catch(Exception ex)
			{	
				retval = (int)status.PolicyRestoreFailed;
				MainClass.DebugLog.Write(string.Format("Exception while setting iFolder policy: {0}--{1}", ex.Message, ex.StackTrace));
			}
			return retval;
		}
	
	} //End of Class iFolderServer

	class CommandParsing 
	{
		private string[] args;

		public Option OldAdminNameOption = new Option("backup-admin,U", "Admin Name of backup", "Admin name of backup", true, "admin");
		public Option NewAdminNameOption = new Option("current-admin,u", "Admin Name of current server", "Admin name of current server", false, "admin");
		public Option OldAdminPasswordOption = new Option("backup-password,P", "Admin Password of backup", "Admin password of backup", true, "novell");
		public Option NewAdminPasswordOption = new Option("current-password,p", "Admin Password of current server", "Admin Password of current server", true, "novell");
		public Option RelativePathOption = new Option("relative-path", "relativepath of file folder", "relativepath of file folder", true, "/Default_iFolder/");
		public Option CurrentServerUrlOption = new Option("server-url", "current server IP", "current server IP", true, "http://127.0.0.1");
		public Option iFolderIDOption = new Option("ifolder-id", "ID of ifolder to be restored", "ID of ifolder to be restored", true, "null");
		public Option UserNameOption = new Option("user", "username for ifolder to be listed", "username for ifolder to be listed", true, "admin");

		public Option PolicyOption = new Option("restore-policies", "Overwrite the iFolder Policies with the Policy of the backup iFolder", " Use this option to overwrite the iFolder policies", false, null);
		public Option RecoverOption = new Option( "restore,r", "flag that tells that this is a restore scenario", "flag that tells that this is a restore scenario", false, null);
		public Option RetryOption = new Option( "retry", "flag that tells that this is a retry scenario", "flag that tells that this is a retry scenario", false, null);
		public Option PathOption = new Option( "path,f", "The path where the data is present", "Location of old database", true, null);
		public Option HelpOption = new Option( "help,h", "cli help", null, false, null);
		public Option UsageOption = new Option( "usage", "prints the usage of the tool", null, false, null);
		public Option DataLocationOption = new Option( "ifolder-path", "The path where the folder data is present", "Location of actual ifolder data in backup databse", true, null);
		public Option PrecheckOption = new Option( "precheck", "precheck phase", null, false, null);
		public Option ListingOption = new Option( "list,l", "listing ifolder for user", null, false, null);
		public Option UseWebAccessOption = new Option( "usewebaccess", "uses web access api's to restore data", null, false, null);
		public Option LogLocationOption = new Option("loglocation", "The location where the logs should be placed", "The location where the logs should be placed", true, null);


        	public CommandParsing(string[] args)
        	{
			this.args = args;

			OldAdminNameOption.OnOptionEntered = new Option.OptionEnteredHandler( OnOldAdminName );
			NewAdminNameOption.OnOptionEntered = new Option.OptionEnteredHandler( OnNewAdminName);
			OldAdminPasswordOption.OnOptionEntered = new Option.OptionEnteredHandler( OnOldAdminPassword );
			NewAdminPasswordOption.OnOptionEntered = new Option.OptionEnteredHandler( OnNewAdminPassword);
			RelativePathOption.OnOptionEntered = new Option.OptionEnteredHandler( OnRelativePath );
			CurrentServerUrlOption.OnOptionEntered = new Option.OptionEnteredHandler( OnCurrentServerUrl );
			iFolderIDOption.OnOptionEntered = new Option.OptionEnteredHandler( OniFolderID);
			UserNameOption.OnOptionEntered = new Option.OptionEnteredHandler( OnUserName);
			PathOption.OnOptionEntered = new Option.OptionEnteredHandler( OnPath );
			HelpOption.OnOptionEntered = new Option.OptionEnteredHandler( OnHelp);
			DataLocationOption.OnOptionEntered = new Option.OptionEnteredHandler( OnDataLocation);
			PrecheckOption.OnOptionEntered = new Option.OptionEnteredHandler( OnPrecheck);	
			ListingOption.OnOptionEntered = new Option.OptionEnteredHandler( OnListing);	
			UseWebAccessOption.OnOptionEntered = new Option.OptionEnteredHandler( OnUseWebAccess);
			RecoverOption.OnOptionEntered = new Option.OptionEnteredHandler( OnRecover);
			RetryOption.OnOptionEntered = new Option.OptionEnteredHandler( OnRetry);
			PolicyOption.OnOptionEntered = new Option.OptionEnteredHandler( OnRestorePolicy);
			LogLocationOption.OnOptionEntered = new Option.OptionEnteredHandler( OnLogLocation);

        	} //End of function CommandParsing Constructor


		/// <summary>
       	/// 
       	/// </summary>
        /// <param name="collectionID">.</param>
        /// <returns>.</returns>
        public bool ParseArguments()
        {
			bool status = false;
			try
			{
				Options.ParseArguments( this, args);
				status = true;
			}
			catch (Exception ex)
			{
				MainClass.DebugLog.Write(string.Format("Exception, invalid format, message:{0} --stack trace:{1}", ex.Message, ex.StackTrace));
			}

			return status;
        } //End of ParseArguments Function


        private bool OnOldAdminName()
        {
			MainClass.oldAdminName = OldAdminNameOption.Value;
			if (MainClass.useSameAdminName == true)
				MainClass.newAdminName = OldAdminNameOption.Value;
			return true;
        }

        private bool OnNewAdminName()
        {
			MainClass.newAdminName = NewAdminNameOption.Value;
			MainClass.useSameAdminName = false;
			return true;
        }

        private bool OnOldAdminPassword()
        {
			MainClass.oldAdminPassword = OldAdminPasswordOption.Value;
			return true;
        }

        private bool OnNewAdminPassword()
        {
            MainClass.newAdminPassword = NewAdminPasswordOption.Value;
            return true;
        }

        private bool OnRelativePath()
        {
            MainClass.relativePath = RelativePathOption.Value;
	    char[] trimchar= {'/'};
	    MainClass.relativePath = MainClass.relativePath.Trim(trimchar);	

            return true;
        }

		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
        private bool OnCurrentServerUrl()
        {
            string url = CurrentServerUrlOption.Value;
            bool result = url.StartsWith("https://");
            if(false == result)
            {
                result = url.StartsWith("http://");
            }
            if(false == result)
            {
                //TODO: use path.combine
                url = "http://" + url;
                CurrentServerUrlOption.Value = url;
            }

            MainClass.currentServerUrl = CurrentServerUrlOption.Value + "/simias10";
            return true;
        }

        private bool OniFolderID()
        {
            MainClass.collectionid = iFolderIDOption.Value;
            return true;
        }



        private bool OnUserName()
        {
            MainClass.userName =  UserNameOption.Value;
            return true;
        }


	
	private bool OnPath()
        {
	    MainClass.DataPath = PathOption.Value;
	    return true;
        }
	
	private bool OnDataLocation()
	{
		MainClass.FolderLocation = DataLocationOption.Value;
		return true;
	}

        private bool OnHelp()
        {
		MainClass.Operation = (int)Command.Help;
            return true;
        }


        private bool OnPrecheck()
        {
            MainClass.PrecheckFlag = true;
            return true;
        }

        private bool OnListing()
        {
		
		if( MainClass.Operation == -1)
			MainClass.Operation = (int)Command.List;
		else
			MainClass.Operation = 100;
            return true;
        }

        private bool OnRecover()
        {
		if( MainClass.Operation == -1)
			MainClass.Operation = (int)Command.Restore;
		else
			MainClass.Operation = 100;
            return true;
        }       

	private bool OnRetry()
        {
		if( MainClass.Operation == -1)
			MainClass.Operation = (int)Command.Retry;
		else
			MainClass.Operation = 100;
            return true;
        }

	private bool OnRestorePolicy()
    	{
		MainClass.OverwritePolicies = true;
              	return true;
    	}

	private bool OnLogLocation()
	{
		MainClass.LogLocation = LogLocationOption.Value;
		return true;
	}
        private bool OnUseWebAccess()
        {
            MainClass.UseWebAccess = true;
            return true;
        }

	} //End of CommandParsing Class

	class MainClass
	{

		public static string oldAdminName = null;
		public static string newAdminName = null;
		public static bool useSameAdminName = true;
		public static string oldAdminPassword = null;
		public static string newAdminPassword = null;
		public static string relativePath = null;
		public static string currentServerUrl = null;
		public static string collectionid = null;
		public static string nodeID = null;
		public static string userName = null;
		public static string userID = null;
		public static string DataPath = null;
		public static string FolderLocation = null;
		public static bool fullrecovery = false;
		public static Logger DebugLog; // = new Logger("debug.log");
		public static Logger FailedLog;
		public static Logger FileNotFound;
		public static bool UseWebAccess = false;
		public static bool OverwritePolicies = false;
		public static string RedirectedNewServerUrl = "";
		public static string OldServerUrl = "http://127.0.0.1:8086/simias10"; 
		public static bool PrecheckFlag = false;
		public static int Operation = -1;
		//3 indicate, number of attempt tool will try to perform web call in case of failure
		public static int MaxCount = 3;
		public static bool runpreviousiter = false;
		public static string previousstatus = "";

		public static string LogLocation = "";
		public static string xmlFileLoc;// = Path.Combine(Directory.GetCurrentDirectory(),"Output.xml");
		public static xmlTag xmlObj;  // = new xmlTag(xmlFileLoc);
	
		// Added XML Tag 
		public static string DetailsTag = "details";
		public static string FilesTag = "Files";
		public static string OldAdminTag = "oldadmin";
		public static string NewAdminTag = "newadmin";
		public static string StatusTag = "Status";
		public static string StatusAttribute = "status";
		public static string NameAttribute = "name";
		public static string ValueAttribute = "value";
		public static string IDAttribute = "ID";
		public static string RelativePathTag = "relativepath";
		public static int TotalItems = 0;
		public static int FinishedItems = 0;

		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
	public static int Main(string[] args)
	{
       	    int RestoreStatus = (int)status.Failed;
	    CertPolicy certPolicy = new CertPolicy();	
            try
            {
		string loglocation = Utility.ReadModMonoConfiguration();
		if( Directory.Exists( loglocation))
		{	
			LogLocation = loglocation;
			DebugLog = new Logger(Path.Combine(LogLocation, "debug.log"));

		}
		else if( Directory.Exists( LogLocation ) == false)
		{
			DebugLog = new Logger();
		}
		GrantAllRights(LogLocation);
		GrantAllRights( DebugLog.LogFile);


                CommandParsing Input = new CommandParsing( args );
		bool parseResult = false;
		try
		{
			parseResult = Input.ParseArguments();
		}
		catch
		{
			RestoreStatus = (int)status.InvalidInput;
			return RestoreStatus;
		}
                if( !parseResult )
                {
                    Console.WriteLine("|               Incorrect Input parameters. \n\n");
		    		PrintHelp();
               	    return (int)status.InvalidFormat;
    	        }
		/// In case of help operation, print help and exit.
		if( Operation == (int)Command.Help)
		{
			PrintHelp();
			RestoreStatus = (int)status.Help;
			return RestoreStatus;
		}
	
		if( PrecheckFlag == false)
		{
			//Echo for backup admin and current admin credentials
			
			if (MainClass.useSameAdminName == true)
			{
				if ( String.IsNullOrEmpty(oldAdminPassword) ) {
					oldAdminPassword = ForPasswordString(string.Format("|		Password for iFolder server admin (user={0}):",MainClass.newAdminName),null);
				}	
				newAdminPassword = oldAdminPassword;

			}
			else
			{
				if ( String.IsNullOrEmpty(oldAdminPassword ))
					oldAdminPassword = ForPasswordString(string.Format("|		Password for backup iFolder server admin (user={0}):",MainClass.oldAdminName),null);
				if ( String.IsNullOrEmpty(newAdminPassword)) 	
				newAdminPassword = ForPasswordString(string.Format("\n|               Password for current iFolder server admin (user={0}):",MainClass.newAdminName),null);
			}
			// Ensure the password has a valid string	
			if ( String.IsNullOrEmpty(newAdminPassword) || String.IsNullOrEmpty(oldAdminPassword)) {
				Console.WriteLine("|               Error: Invalid value for  Password                                       |");
				return (int)status.MissingFields;

			}
		}	

		if( (RestoreStatus = ValidateInput(PrecheckFlag)) != 0)
			return RestoreStatus;
		if( PrecheckFlag == true)
		{
			Console.WriteLine("");	
			Console.WriteLine("iFolder Restore Application from file system backup");	
			Console.WriteLine("version 1.0.0");	
			Console.WriteLine("");	
			Console.WriteLine(" _______________________________________________________________________________________ ");	
			Console.WriteLine("|                                                                                        |");	
			Console.WriteLine("|                                                                                        |");	
			Console.WriteLine("|               Validating the input......................                               |");	
			RestoreStatus = GetPathIndex(args);
			return RestoreStatus;
		}

		if(Operation == (int)Command.List)
			return ListiFolders();


		Console.WriteLine("\n|               Scanning the data for restore...                                         |");

		xmlFileLoc = Path.Combine(LogLocation, collectionid+".xml");
                xmlObj = new xmlTag(xmlFileLoc);
                //Accessing Old and New server Object.
                iFolderServer OldServer = new iFolderServer( OldServerUrl,oldAdminName, oldAdminPassword, false);
                iFolderServer NewServer = new iFolderServer( currentServerUrl, newAdminName, newAdminPassword);


		string oldAdminID = OldServer.GetUserIDFromName(oldAdminName);
		MainClass.DebugLog.Write(string.Format("The old admin ID is: {0}", oldAdminID));
		string newAdminID = NewServer.GetUserIDFromName(newAdminName);
		MainClass.DebugLog.Write(string.Format("The new admin ID is: {0}", newAdminID));
	
		if( String.IsNullOrEmpty(oldAdminID) || String.IsNullOrEmpty(newAdminID))
		{
		    MainClass.DebugLog.Write(string.Format("Incorrect Admin Credential Or iFolder Server is not accessible and exit status is:{0}.","EmptyAdminID"));
		    RestoreStatus = (int)status.EmptyAdminID;	
		    return (int)status.EmptyAdminID;
		}
	
		//TODO: Remove multiple returns form function	

		bool oldAdminAdded = false;
		bool newAdminAdded = false;
			
		// Create the iFolder and add members...
		// create another collection on the new server with same ID and add these members with same rights...
		iFolder ifolder = OldServer.GetiFolder(collectionid);
		if( ifolder == null )
		{
			MainClass.DebugLog.Write(string.Format("The iFolder with ID: {0} is not present on this box...with exit status:{1}", collectionid, "InvalidiFolderIdOld"));
			RestoreStatus = (int)status.InvalidiFolderIdOld;
			return (int)status.InvalidiFolderIdOld;
		}
		else
		{
			string failedFile = Path.Combine(LogLocation, collectionid+".failed");
			FailedLog = new Logger(failedFile);
			GrantAllRights(FailedLog.LogFile);
			MainClass.DebugLog.Write(string.Format("User Name: {0} iFolder name: {1}", ifolder.OwnerUserName, ifolder.Name));
			string fileName = Path.Combine(LogLocation, collectionid+".notfound");
			if(File.Exists(fileName))
			{
				File.Delete(fileName);
			}
			FileNotFound = new Logger(fileName);
			GrantAllRights(FileNotFound.LogFile);
			// Create the new iFolder and add members for that...
			string OwnerUserID = ifolder.OwnerID;
			RedirectedNewServerUrl = NewServer.GetHomeServer(ifolder.OwnerUserName);

			if( String.IsNullOrEmpty(RedirectedNewServerUrl) )
                        {
				MainClass.DebugLog.Write(string.Format("Failed, fetching Home Server information for ifolder owner user:{0} on current server with exit status as: {1}",ifolder.OwnerUserName, "EmptyNewServerUrl"));
				//Restore tool does not allow to restore the data if the owner does not exist on the server.
				Console.WriteLine("|               		Error: Unable to get the home server information for the user: \'{0}\'.    |",ifolder.OwnerUserName);		
				RestoreStatus = (int)status.EmptyNewServerUrl;
                return (int)status.EmptyNewServerUrl;
			}
			MainClass.DebugLog.Write(string.Format("The redirected url is: {0}", RedirectedNewServerUrl));

			NewServer = new iFolderServer(RedirectedNewServerUrl, newAdminName, newAdminPassword, false);

				/// Added	
			int remainingItems = 0;
			previousstatus = xmlObj.VerifyStatus();
			MainClass.DebugLog.Write(string.Format("Previous status = {0}, remainingitems = {1}", previousstatus, remainingItems));

			if( previousstatus.Equals("NotStarted") )
			{
					if( Operation == (int)Command.Retry)
					{
						Console.WriteLine("|               The previous restore is successful.                                      |");
						Console.WriteLine("|               Run with \"--restore\" option if you want to run the restore again       |");
						return 0;
					}
				xmlObj.ClearXMLDoc();
			}
			else if( previousstatus.Equals("Completed") )
			{
			    //If no remaining Items in XML for processing, Clearing XML doc	
				int total =0, completed =0;
				NewServer.GetRestoreStatusForCollection(collectionid , out total, out completed);
				
				string failedWorkingLog = Path.Combine(LogLocation, collectionid+".failed.working");
				FileInfo failedWorkingLoginfo = new FileInfo(failedWorkingLog);

				if(( total != -1 && total != completed ) || FailedLog.Size > 0 
				  || ( File.Exists(failedWorkingLog) && failedWorkingLoginfo.Length > 0) )
				{
					/// retry needed...
					if(Operation != (int)Command.Retry)
					{
						Console.WriteLine("|                                                                                        |");
						Console.WriteLine("|               There is some unclompleted recovery previously.                          |");
						Console.WriteLine("|               If you want to continue with old one run with                            |");
						Console.WriteLine("|               \"--retry\" command to run the old config.                                 |");
						Console.WriteLine("|               Otherwise move the xml file and rerun the tool.                          |");
						Console.WriteLine("|                                                                                        |");
						return 0;
					}
					else
					{
						relativePath = xmlTag.GetAttributeValue( DetailsTag, RelativePathTag, ValueAttribute);
						MainClass.DebugLog.Write(string.Format("We are in retry phase with relativepath {0}", relativePath));
						runpreviousiter = true;
					}
				}
				else
				{
					if(total != -1 && completed != -1)
						Console.WriteLine("|               The total count: {0} and finished count: {1}                             |", total, completed);
					/// If the command is retry fail saying that the previous state is usccess...
					if( Operation == (int)Command.Retry)
					{
						Console.WriteLine("|               The previous restore is successful.                                      |");
						Console.WriteLine("|               Run with \"--restore\" option if you want to run the restore again       |");
						return 0;
					}
				    xmlObj.ClearXMLDoc();
				}
			}
			else 
			{
				if(Operation != (int)Command.Retry)
				{
					Console.WriteLine("|                                                                                        |");
					Console.WriteLine("|               There is some unclompleted recovery previously.                          |");
					Console.WriteLine("|               If you want to continue with old one run with                            |");
					Console.WriteLine("|               \"--retry\" command to run the old config.                                 |");
					Console.WriteLine("|               Otherwise move the xml file and rerun the tool.                          |");
					Console.WriteLine("|                                                                                        |");
					return 0;
				}
				else
				{
					relativePath = xmlTag.GetAttributeValue( DetailsTag, RelativePathTag, ValueAttribute);
					MainClass.DebugLog.Write(string.Format("We are in retry phase with relativepath {0}", relativePath));
					runpreviousiter = true;
				}
			}
			MainClass.DebugLog.Write(string.Format("Running previous iteration: {0}", runpreviousiter.ToString()));
			if( runpreviousiter == false)
				WriteDetailsToXML( relativePath );
	
			if( String.IsNullOrEmpty(relativePath))
				fullrecovery = true;
			
			bool retStatus = false;
			if( fullrecovery)
			{
				retStatus = CreateiFolder(ifolder.ID, ifolder.Name, OwnerUserID, ifolder.Description, ifolder.EncryptionAlgorithm, NewServer, OldServer);
			}
			else
			{
							
				ifolder = NewServer.GetiFolder( collectionid );
				if( ifolder != null)
					retStatus = true;
			}

			if( retStatus == false)
			{
				MainClass.DebugLog.Write(string.Format("Unable to create iFolder for id: {0} and exited with status as :{1}",collectionid, "FailediFolderCreation"));
				RestoreStatus = (int)status.FailediFolderCreation;
				return (int)status.FailediFolderCreation;
			}
			else
			{
				MainClass.DebugLog.Write("Calling UpdateXML for AddMembers");
				xmlTag.UpdateXML(DetailsTag, StatusTag, ValueAttribute, null, "AddMembers");
				// Adding Members...
				AddSharingInformation(collectionid, oldAdminID, newAdminID, NewServer, OldServer, out oldAdminAdded, out newAdminAdded);
					
				AddAdminMember(collectionid, newAdminAdded, oldAdminAdded, runpreviousiter, NewServer, OldServer );
				xmlTag.UpdateXML(DetailsTag, StatusTag, ValueAttribute, null, "StartRecovery");
				
				if( fullrecovery )
					relativePath = ifolder.Name;

				RestoreStatus = RestoreiFolder(collectionid, relativePath);
				xmlTag.UpdateXML(DetailsTag, StatusTag, ValueAttribute, null, "EndRecovery");
				//previousstatus = xmlObj.VerifyStatus(out totalitems, out remainingItems);
				xmlTag.UpdateXML(DetailsTag, StatusTag, ValueAttribute, null, "RemoveMember");
				if(RestoreStatus != (int)status.Success )
				{
					MainClass.DebugLog.Write(string.Format("{0} {1} Error Code: {2}", collectionid, "Failed", RestoreStatus));
				}
				
				RemoveAdminMember(collectionid, newAdminAdded, oldAdminAdded, runpreviousiter, NewServer, OldServer );
				xmlTag.UpdateXML(DetailsTag, StatusTag, ValueAttribute, null, "Completed");
			}
			if(File.Exists(fileName))
			{
				long fileNotFound = NumberOfLines(fileName);
				if(fileNotFound > 0)
				{
					Console.WriteLine("\n|               {0} file not found in backup location.                                   |",fileNotFound);
				}
				else 
				{
					MainClass.DebugLog.Write(string.Format("Deleting empty file, file .notfound"));
					File.Delete(fileName);
				}
			}
			if(File.Exists(failedFile))
                        {
                                long noOfEntires = NumberOfLines(failedFile);
                                if(noOfEntires > 0)
                                {
                                        Console.WriteLine("\n|               {0} file failed in process of restoring.                                 |",noOfEntires);
                                }
                                else
                                {
                                        MainClass.DebugLog.Write(string.Format("Deleting empty file, .failed file"));
                                        File.Delete(failedFile);
                                }
                        }
		}
	}
	catch(Exception e1)
	{
		MainClass.DebugLog.Write(string.Format("Exception in RestoreTool. {0}--{1}", e1.Message, e1.StackTrace));
		if( MainClass.DebugLog != null)
			MainClass.DebugLog.Write(string.Format("Exception while handling SIGINT. {0}--{1}", e1.Message, e1.StackTrace));
		RestoreStatus = (int)status.Failed;
		try
		{
			xmlTag.UpdateXML(DetailsTag, StatusTag, ValueAttribute, null, "Failed");
		}
		catch { }
	}

	finally
	{
		PrintResult(RestoreStatus);
	}
	return RestoreStatus;
	} //End of Main Function

	public static void GrantAllRights(string path)
	{
		try
		{
   				Utility.Execute( "chmod", "0777 {0}", path);
		}
		catch(Exception /*ex*/)
		{
			MainClass.DebugLog.Write(string.Format("Grant rights failed for path {0}", path));
		}
	}

 	public static bool AddAdminMember(string collectionid, bool newAdminAdded, bool oldAdminAdded, bool runpreviousiter, iFolderServer NewServer, iFolderServer OldServer )
 	{ 	
					if( newAdminAdded == false)
					{
					      try
					      {
					              MainClass.DebugLog.Write("adding new admin as a member");
					              // add admin as a member on the new and old servers
					              NewServer.AddMember( collectionid, newAdminName, Rights.ReadWrite );
					              OldServer.AddMember(collectionid, newAdminName, Rights.ReadWrite);
					      }
					      catch(Exception ex)
					      {
					              MainClass.DebugLog.Write(string.Format("Exception while adding admin as member: {0}--{1}", ex.Message, ex.StackTrace));
					      }
					}
					if(runpreviousiter)	/// admin is added during member addition.
					{
						// If we are running from the previous iteration, and if the admin is not added that time, then the admin added status should be false...
						string previousadminadded = xmlTag.GetAttributeValue( DetailsTag, NewAdminTag, ValueAttribute);
						MainClass.DebugLog.Write(string.Format("Previous admin added: {0}", previousadminadded));
						if(previousadminadded == "False")
						{
							newAdminAdded = false;
						}
					}
					xmlTag.UpdateXML( DetailsTag, NewAdminTag, ValueAttribute, null, newAdminAdded.ToString());
					xmlTag.UpdateXML( DetailsTag, NewAdminTag, StatusAttribute, null, "Added");
					if( oldAdminName != newAdminName)
					{
						if( oldAdminAdded == false )
						{
						      try
						      {
						              MainClass.DebugLog.Write("adding old admin as member...");
						              // add admin as a member on the new and old servers
					        	      NewServer.AddMember( collectionid, oldAdminName, Rights.ReadWrite );
						              OldServer.AddMember(collectionid, oldAdminName, Rights.ReadWrite);
						      }
						      catch(Exception ex)
						      {
						              MainClass.DebugLog.Write(string.Format("Exception while adding admin as member: {0}--{1}", ex.Message, ex.StackTrace));
						      }
						}
						if(runpreviousiter)        /// admin is added during member addition.
						{
							// If we are running from the previous iteration, and if the admin is not added that time, then the admin added status should be false...
							string previousadminadded = xmlTag.GetAttributeValue( DetailsTag, OldAdminTag, ValueAttribute);
							if(previousadminadded == "False")
							{
								oldAdminAdded = false;
							}
						}
						xmlTag.UpdateXML( DetailsTag, OldAdminTag, StatusAttribute, null, "Added");
					}
					xmlTag.UpdateXML( DetailsTag, OldAdminTag, ValueAttribute, null, oldAdminAdded.ToString());
					return true;
 	}
 	
 	public static bool RemoveAdminMember(string collectionid, bool newAdminAdded, bool oldAdminAdded, bool runpreviousiter, iFolderServer NewServer, iFolderServer OldServer )
 	{

					if( newAdminAdded == false)
					{
					      // Remove admin membership...
					      try
					      {
					              MainClass.DebugLog.Write("removing new admin as member...");
					              NewServer.RemoveMember(collectionid, newAdminName);
					              OldServer.RemoveMember( collectionid, newAdminName);
						      xmlTag.UpdateXML( DetailsTag, NewAdminTag, StatusAttribute, null, "Removed");
					      }
					      catch(Exception ex)
					      {
					              MainClass.DebugLog.Write(string.Format("Exception while removing admin as member: {0}--{1}", ex.Message, ex.StackTrace));
					      }
					}
					else
						xmlTag.UpdateXML( DetailsTag, NewAdminTag, StatusAttribute, null, "Removed");
					if( oldAdminAdded == false && oldAdminName != newAdminName )
					{
					      // Remove admin membership...
					      try
					      {
					              MainClass.DebugLog.Write("Removing old admin as member...");
					              NewServer.RemoveMember(collectionid, oldAdminName);
					              OldServer.RemoveMember( collectionid, oldAdminName);
						      xmlTag.UpdateXML( DetailsTag, OldAdminTag, StatusAttribute, null, "Removed");
					      }
					      catch(Exception ex)
					      {
					              DebugLog.Write(string.Format("Exception while removing admin as member: {0}--{1}", ex.Message, ex.StackTrace));
					              MainClass.DebugLog.Write(string.Format("Exception while removing admin as member: {0}--{1}", ex.Message, ex.StackTrace));
					      }
					}
					else
						xmlTag.UpdateXML( DetailsTag, OldAdminTag, StatusAttribute, null, "Removed"); 	
 		return true;
 	}
 	
 	public static bool AddSharingInformation(string collectionid, string oldAdminID, string newAdminID, iFolderServer NewServer, iFolderServer OldServer, out bool oldAdminAdded, out bool newAdminAdded)
 	{ 				
 					newAdminAdded = false;
 					oldAdminAdded = false;
					int offset = 0;
					int ct = 20;
					do
					{	
					      iFolderUserSet members= null;
					      try
					      {
					              members = OldServer.GetMembers(collectionid, offset, ct);
					      }
					      catch(Exception ex)
					      {
					              MainClass.DebugLog.Write(string.Format("The iFolder does not exist on this server: {0}", ex.Message));
					      }
					      if( members == null || members.Items.Length == 0)
					              break;
					      foreach( iFolderUser member in members.Items)
					      {
					              if( member.ID == oldAdminID )
					              {
					                      DebugLog.Write(string.Format("old admin is a member..."));
					                      MainClass.DebugLog.Write("old admin is a member...");
					                      oldAdminAdded = true;
					              }
					              MainClass.DebugLog.Write(string.Format("Adding user: {0}", member.UserName));
					              string memID = NewServer.AddMember( collectionid, member.UserName, member.MemberRights);
					              if( memID == null)
					              {
					                      DebugLog.Write(string.Format("The member is not added properly."));
					                      MainClass.DebugLog.Write("The member is not added properly.");
					                      continue;
					              }
					              if( memID == newAdminID )
					              {
					                      DebugLog.Write(string.Format("The new admin is a member..."));
					                      MainClass.DebugLog.Write("The new admin is a member...");
					                      newAdminAdded = true;
					              }
					      }
					      offset += members.Items.Length;
					}while(ct >0 ); 	
 					return true;
 	}

	public static bool CreateiFolder( string iFolderID, string Name, string OwnerID, string Description, string EncryptionAlgorithm, iFolderServer NewServer, iFolderServer OldServer)
	{
		bool retStatus = false;

					try
					{
							if( !String.IsNullOrEmpty(EncryptionAlgorithm))
							{
								string eKey = null, eBlob = null, eAlgorithm = null, rKey = null;
								bool encrSettings = OldServer.simws.GetEncryptionDetails(iFolderID, out eKey, out eBlob, out eAlgorithm, out rKey);
								if( encrSettings )
								{
									MainClass.DebugLog.Write(string.Format("Creating encrypted iFolder. {0}-{1}-{2}-{3}", eKey, eBlob, eAlgorithm, rKey));
									retStatus = NewServer.CreateEncryptediFolderWithID(Name, OwnerID, Description, iFolderID, eKey, eBlob, eAlgorithm, rKey);
								}
							}
							else
							{
								MainClass.DebugLog.Write("Creating unencrypted iFolder.");
								retStatus = NewServer.CreateiFolderWithID(Name, OwnerID, Description, iFolderID);
							}
					}
                         		catch(Exception ex)
                         		{
                                 	/// Check if the iFolder already exists.....
                                 		iFolder ifdr = NewServer.GetiFolder( collectionid );
                                 		if( ifdr == null )
                                 		{
                                         		MainClass.DebugLog.Write(string.Format("Unable to create iFolder with given ID: {0}", ex.Message));
                                         		// continue with the next iFolder if the previous status is not created...
							retStatus = false;
                                 		}
                                 		else
                                 		{
                                         		retStatus = true;
                                 		}
                         		}
		return retStatus;
	}

	public static bool WriteDetailsToXML(string relativepath)
	{
					Hashtable ht = new Hashtable();
					xmlTag.WriteDetailsToXML(DetailsTag, RelativePathTag, ValueAttribute, relativepath);
					ht.Clear();
					ht.Add( ValueAttribute, "false");
					ht.Add(StatusAttribute, "0");
					xmlTag.WriteDetailsToXML(DetailsTag, OldAdminTag, ht);
					xmlTag.WriteDetailsToXML(DetailsTag, NewAdminTag, ht);
					MainClass.DebugLog.Write("Setting the status to initialized");
					xmlTag.WriteDetailsToXML(DetailsTag, StatusTag, ValueAttribute, "Initialized");
					return true;
	}

	public static int ValidateInput(bool precheck)
	{
		iFolderServer OldServer = null;
		iFolderServer NewServer = null;
		bool ListingFlag = false;
		if( Operation == (int)Command.List)
			ListingFlag = true;
		if( Operation == 100 || Operation == -1)
		{
			Console.WriteLine("|               No/Ambiguous Operation specified.                                        |");
			return (int)status.InvalidInput;
		}
		if( oldAdminName == null || DataPath== null )
		{
			Console.WriteLine("|               The input has some fields missing.                                       |");
			return (int)status.MissingFields;
		}
		if( !Directory.Exists(DataPath))
		{
			Console.WriteLine("|               The backup datapath does not exist                                       |");
			return (int)status.DataPathDoesNotExist;
		}
		else {
		//Check for mandatory files in the given simias path
				string simiasPath = Path.Combine(DataPath, "simias");;
				if( !Directory.Exists(simiasPath)) {
				Console.WriteLine("|		Error: simias directory does not exist at the specified path: {0} .|",DataPath);
					return (int)status.InvalidBackupPath;
				}
				string flaimdb= Path.Combine(DataPath, "simias/FlaimSimias.db");
				if(!File.Exists (flaimdb) ){

				Console.WriteLine("|		Error: iFolder simias database ({0}) does not exist at the specified path.|",flaimdb);
					return (int)status.InvalidBackupPath;
				}

		}
	        if( Operation == (int)Command.Restore )
                {
                        if(!String.IsNullOrEmpty(relativePath)) {
                                if( !File.Exists(Path.Combine(FolderLocation,relativePath) )
                                   && !Directory.Exists(Path.Combine(FolderLocation,relativePath) ))
                                {
                                        return (int)status.InvalidRelativePath;
                                }
								if (MainClass.OverwritePolicies )
								{
									Console.WriteLine("|               Option \"restore-policies\" must not be used for directory or file restore operation                                       |");
									return (int)status.InvalidInput;
								}
						}
                }
	
		/// In case of listing, we should have the temporary server up and running, and old server credentials should be proper. User name is expected.
		if( ListingFlag && String.IsNullOrEmpty(userName))
		{
			Console.WriteLine("|               The input has some fields missing.                                       |");
			return (int)status.MissingFields;
		}	
		if( String.IsNullOrEmpty(oldAdminName))
			return (int)status.MissingFields;
		if( !ListingFlag && ( String.IsNullOrEmpty(newAdminName) || String.IsNullOrEmpty(currentServerUrl) 
							|| collectionid == null || FolderLocation == null))
			return (int)status.MissingFields;
		
		if( !precheck)
		{
			/// In case of listing, we should have the temporary server up and running, and old server credentials should be proper. User name is expected.
				try
				{
					OldServer = new iFolderServer( OldServerUrl,oldAdminName, oldAdminPassword, false);
				}
				catch(Exception /*e1*/)
				{
					OldServer = null;
					Console.WriteLine("|               Unable to contact the backup server.                                     |");
				}
				if( OldServer == null || OldServer.PingServer() == false)
				{
					Console.WriteLine("|               Unable to ping the server.                                               |");
					return (int)status.BackupServerNotRunning;
				}
				if( OldServer.GetAuthenticatedUser() == null)
				{
					Console.WriteLine("\n|               Invalid credentials. Please try again.                                   |");
					return (int)status.InvalidCredentials;
				}
				if( ListingFlag == false)
				{
					/// Need to check the new admin related stuff...
					try
					{
						NewServer = new iFolderServer( currentServerUrl, newAdminName, newAdminPassword);
					}
					catch(Exception /*e2*/)
					{
						NewServer = null;
						MainClass.DebugLog.Write(string.Format("Unable to contact the new server."));
					}
					if( NewServer == null || NewServer.PingServer() == false)
						return (int)status.CurrentServerNotRunning;
					if( NewServer.GetAuthenticatedUser() == null)
						return (int)status.InvalidCredentials;		
				}
		}
		return 0;
	}

	public static int ListiFolders()
	{
		try
		{
			    iFolderServer OldServer = new iFolderServer( OldServerUrl,oldAdminName, oldAdminPassword, false);
			    iFolderDetails ifd = null;
			    

			    userID = OldServer.GetUserIDFromName(userName);
			    if(userID == null)	
			    {	
			    	Console.WriteLine("\n|               Invalid User: {0}, does not exist in backup store.                     |",userName);	
		    		MainClass.DebugLog.Write(string.Format("Invalid User: {0}, does not exist in backup store and exit status is :{1}.",userName, "InvalidUserName"));	
				return (int)status.InvalidUserName;
			    }

			    string[] ifolders = OldServer.GetAllCollectionIDsByUser( userID);
			    if(ifolders.Length == 0)
			    {	
                	    	Console.WriteLine("\n|               No iFolder exists for user: {0}                                        |", userName);		   
                    		MainClass.DebugLog.Write(string.Format("No iFolder exist of user: {0} and exit status is :(1)", userName, "Success"));		   
				return (int)status.Success;
			    }

			    Console.WriteLine("\n\n|               List of iFolders:                                                        |");		
			    foreach(string str in ifolders)
			    {
				    //TODO: Finalize and list all the required data
				    try
				    {	
					    ifd = OldServer.admin.GetiFolderDetails(str);
				    }
				    catch(Exception e)
				    {
					    MainClass.DebugLog.Write(string.Format("Exception in fetching details for: {0}. Message: {0}--{1}", str, e.Message, e.StackTrace));
				    }
				    if(ifd != null)
					    Console.WriteLine("\niFolder Name: {0} \r\n\tiFolderID: {1} \r\n\tPath :{2}", ifd.Name, ifd.ID, ifd.UnManagedPath);

				    MainClass.DebugLog.Write(string.Format("iFolder {0}", str));

		
 					
			    }
		}
		catch(Exception ex)
		{
			MainClass.DebugLog.Write(string.Format("Exception in ListiFolders: {0}--{1}", ex.Message, ex.StackTrace));
			Console.WriteLine("|               Unable to List iFolders. See the logs for more details.                  |");
		}
		return 0;
	}

	public static int GetPathIndex(string[] args)   	
	{
	    MainClass.DebugLog.Write(string.Format("Enter Function: GetPathIndex"));
	    int returnStatus = 101;
            try
            {
		
		//To find the index of path in command line 
		int index = 1;
		foreach (string input in args)
		{
		    if(input.StartsWith("--path") || input.StartsWith("--databasepath"))
		    {	
			MainClass.DebugLog.Write(string.Format("The path argument is: {0}", index));
			returnStatus = index;
			break;	
		    }	
		    index++;
		}
	    }
		catch(Exception e1)
		{
			MainClass.DebugLog.Write(string.Format("Exception while handling SIGINT. {0}--{1}", e1.Message, e1.StackTrace));
			returnStatus = (int)status.Failed;
		}

		MainClass.DebugLog.Write(string.Format("Exiting Function GetPathIndex with status: {0}", returnStatus));
		return returnStatus;
        } //End of GetPathIndex Function of MainClass


		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
			public static int RestoreiFolder(string iFolderID, string relativepath)
			{
				MainClass.DebugLog.Write(string.Format("In RestoreiFolder: {0}--{1}", iFolderID, relativepath));
				bool SourceOnSameServer = false;
				int retval = (int)status.Success;

				iFolderServer OldServer = new iFolderServer(OldServerUrl, oldAdminName, oldAdminPassword, false);
				iFolderServer NewServer = new iFolderServer(RedirectedNewServerUrl, newAdminName, newAdminPassword, false);

				if(retval == (int)status.Success)
				{
					//string oldUnManagedPath = iFolderServer.GetUnManagedPath(oldifolderDetails);
					string oldUnManagedPath = MainClass.FolderLocation;
					MainClass.DebugLog.Write(string.Format("The iFolder location is: {0}", oldUnManagedPath));
					if( Directory.Exists( oldUnManagedPath ))
					{
						MainClass.DebugLog.Write("The source is on same server...");
						SourceOnSameServer = true;
					}

					iFolderDetails newifolderDetails = null;
					int count = 0; //initilizing the count to 0
					while(count <= MaxCount)
					{
						try
						{
							newifolderDetails = NewServer.GetiFolderDetails( iFolderID );
							break;
						}
						catch(InvalidOperationException /*inOpEx*/)
						{
							count++;
							continue;	
						}
						catch(Exception /*ex*/)
						{
							break;

						}
					}

					string iFolderLocation = null;
					if( newifolderDetails == null)
					{
						MainClass.DebugLog.Write("The new iFolder is not present. Need to verify why...");
						retval = (int)status.InvalidiFolderIdOld;
					}
					else
					{
						iFolderLocation = newifolderDetails.UnManagedPath;
					}				

					if(  retval == (int)status.Success && SourceOnSameServer == false)
					{
						MainClass.DebugLog.Write(string.Format("The source is not on the same machine. Returning false. and exit status is:{0}","BackupExistenceFailed"));
						retval = (int)status.BackupExistenceFailed;
					}
					if( retval == (int)status.Success && iFolderLocation!= null && 
							MainClass.UseWebAccess == false && Directory.Exists( iFolderLocation ))
					{
						MainClass.DebugLog.Write("target is on a same server...");
					}
					else
						MainClass.UseWebAccess = true;

					if( retval == (int)status.Success)
					{
						retval = RestoreiFolderData(iFolderID, OldServer, oldUnManagedPath, NewServer,
								newifolderDetails.UnManagedPath, relativepath, MainClass.UseWebAccess);
					}
					if( (retval == 0) && MainClass.OverwritePolicies )
					{
						retval = RestoreiFolderPolicy(iFolderID, OldServer, NewServer);
					}
				
					Console.WriteLine("|               Checking policies post data restore                                                |");
					//This check needs to be done unconditionally. Policy violation can happen
				    	// during a partial or a full restore.
					CheckiFolderPolicyStatus(iFolderID, NewServer);
					CheckUserPolicyStatus(iFolderID, OldServer, NewServer);

				}
				return retval;
			}

			public static int RestoreDataLocally(string ifolderid, iFolderServer oldserver, string oldpath, iFolderServer newserver, string newpath, string relativepath, int startindex, string LogLocation)
			{
				int retval = -1;
				try
				{
					MainClass.DebugLog.Write(string.Format("url: {0}", OldServerUrl));
					string url = OldServerUrl+"/Simias.asmx";
					string adminname = oldserver.adminNameForAuth;
					string adminpassword = oldserver.adminPasswordForAuth;
					MainClass.DebugLog.Write(string.Format("Url: {0} relativepath: {1} oldpath: {2} ", url, relativepath, oldpath));
					retval = newserver.RestoreiFolderData(url, adminname, adminpassword, ifolderid, relativepath, oldpath, startindex);
					if( retval != 0)
					{
						MainClass.DebugLog.Write(string.Format("Error in newserver.RestoreiFolderData. retval: {0}", retval));
						return retval;
					}
					else
					{
						MainClass.DebugLog.Write(string.Format("zero is the return value."));
					}
					int RestoreStatus = -1;
					int totalcount;
					int finishedcount;
					do
					{
						RestoreStatus = newserver.GetRestoreStatusForCollection(ifolderid, out totalcount, out finishedcount);
						/// Display the progress here...
						if( RestoreStatus == 2)
							Console.Write("|               Processing {0} of {1} files.                                             |\r", finishedcount, totalcount);
						else if( RestoreStatus == 1)
							Console.WriteLine("|               Processing the files for restore                                         |");
						else
							MainClass.DebugLog.Write(string.Format("Restore status is: {0}", RestoreStatus));
						Thread.Sleep(4000);
					}while(RestoreStatus == 1 || RestoreStatus == 2);
					retval = RestoreStatus;
				}
				catch(Exception ex)
				{
					MainClass.DebugLog.Write(string.Format("Exception in RestoreDataLocally: {0}--{1}", ex.Message, ex.StackTrace));
				}
				return retval;
			}

			public static bool UpdateFailedLog( string filename, string tempfile, int count)
			{
				int ct= 0;
				bool retval = false;
				//string tempfile = filename + "temp";
				//tempfile = tempfile + count.ToString();
				if( File.Exists( tempfile))
					File.Delete( tempfile);

				TextReader reader = (TextReader)File.OpenText( filename);
				while( (reader.ReadLine()!= null) && ct < count-1)
					ct++;
				/// Copy the rest of the lines...
				TextWriter writer = (TextWriter)File.CreateText(tempfile);
				try
				{
				if( writer != null)
				{
					string str = null;
					while( (str = reader.ReadLine()) != null)
					{
						writer.WriteLine(str);
					}
					reader.Close();
					writer.Close();
					retval = true;
				}
				}
				catch(Exception /*ex*/)
				{
					retval = false;
				}	
				finally
				{
					reader.Close();
					writer.Close();
				}
				return retval;
			}

			public static int RestoreFromFailedLog( string logpath, string iFolderID, iFolderServer oldserver, string oldpath, iFolderServer newserver, string newpath)
			{

				MainClass.DebugLog.Write(string.Format("Enter RestoreFromFailedLog"));

			 	TextWriter logwriter = null;
                                TextReader logreader = null;

				int retval = -1;
				try
				{
			              if(File.Exists( logpath) && File.Exists( logpath+".working") )
                                        {
                                                MainClass.DebugLog.Write(string.Format("both file exist"));
                                                try
                                                {
                                                        logwriter = new FileInfo(logpath+".working").AppendText();
                                                        logreader = (TextReader)File.OpenText( logpath);
                                                }
                                                catch(Exception e1)
                                                {
                                                        MainClass.DebugLog.Write(string.Format("ex: {0}--{1}", e1.Message, e1.StackTrace));
                                                }
                                                if( logwriter == null)
                                                {
                                                        MainClass.DebugLog.Write(string.Format("writer is null"));
                                                        return retval;
                                                }
                                                if( logreader == null)
                                                {
                                                        MainClass.DebugLog.Write(string.Format("reader is null"));
                                                        return retval;
                                                }
                                                string str = null;
                                                while( (str = logreader.ReadLine()) != null)
                                                {
                                                        logwriter.WriteLine(str);
                                                }
						logwriter.Close();
						logreader.Close();
                                                File.Copy(logpath+".working",logpath, true);
                                                File.Delete(logpath+".working");
						MainClass.DebugLog.Write(string.Format("Appending logs into logpath from logpath.working completed and logpath.working file is removed"));
					}	
					else if(File.Exists( logpath +".working") == true)
					{
						if( File.Exists( logpath) == false)
						{
                                                	File.Copy(logpath+".working",logpath);
                                                	File.Delete(logpath+".working");
						}
					}	
				

					if( File.Exists( logpath+".working"))
						File.Delete(logpath+".working");
					File.Copy(logpath, logpath+".working");
					File.Delete(logpath);
					logpath += ".working";
					if(File.Exists(FailedLog.LogFile))
					{
						MainClass.DebugLog.Write(string.Format("File: {0} exists...", FailedLog.LogFile));
					}

					TextReader reader = (TextReader)File.OpenText( logpath);
					if( reader == null)
					{
						MainClass.DebugLog.Write(string.Format("Unable to open the failed logfile"));
						return retval;	// unable to open file...
					}
					string entry = null;
					char[] delimiter = {' '};
					int count = 0;

					long processedCount = 0;
					long totalCount = NumberOfLines(logpath);

					while((entry = reader.ReadLine())!= null)
					{
						string[] entryset = entry.Split(delimiter);
						string index = entryset[0];
						string id = entryset[1];
						string length = entryset[2];
						string relativepath = null;
						for( int i=3; i< entryset.Length-1; i++)
						{
							/// The file path might have spaces. so concat entries
							relativepath += entryset[i];
							if( i < entryset.Length-2 )
								relativepath += " ";
						}
						string fullpath = Path.Combine(oldpath, relativepath);
							
						MainClass.DebugLog.Write(string.Format("Entry: {0}.", entry));
						if (Directory.Exists(fullpath))
						{
							//MainClass.DebugLog.Write("Directory Creation");
							if(!newserver.CreateDirectory(iFolderID, relativepath))
							{
								MainClass.DebugLog.Write(string.Format("Directory Creation failed for folder {0}-{1}-{2}", iFolderID, relativepath, (int)status.DirCreationFailed));
								MainClass.FailedLog.Write(string.Format("{0} {1} {2} {3} {4}", index, id, length, relativepath, "failed"), false);
								//retValue = (int)status.DirCreationFailed;
							}
						}
						else if( File.Exists( fullpath))
						{
							//Call Web API to Create File
							if(!newserver.UploadFile( iFolderID, fullpath, relativepath, Convert.ToInt64(length)))
							{
								MainClass.DebugLog.Write(string.Format("File Creation failed for folder {0}-{1}-{2}", iFolderID, relativepath, fullpath));
								MainClass.FailedLog.Write(string.Format("{0} {1} {2} {3} {4}", index, id, length, relativepath, "failed"), false);
							}
						}
						count++;
						if( count %200 == 0)
						{
							/// close reader...
							string tempfile = logpath + "temp";
							bool success = UpdateFailedLog(logpath, tempfile, count);
							if( success)
							{
								count = 0;
								reader.Close();
								File.Copy(tempfile, logpath, true);
								reader = (TextReader)File.OpenText(logpath);
								/// Move the tempfile to existing failed log...
							}
						}
						processedCount++;
						Console.Write("|               Processed {0} of {1} Entries from failed log.                                |\r",processedCount, totalCount);
					}
					retval = 0;
					
					if( File.Exists( logpath) == true)
					{
						MainClass.DebugLog.Write(string.Format("Deleting file: {0}", logpath));
						if(File.Exists(FailedLog.LogFile))
						{
							MainClass.DebugLog.Write(string.Format("File: {0} exists...", FailedLog.LogFile));
						}
						File.Delete(logpath);
					}
				}
				catch(Exception ex)
				{
					MainClass.DebugLog.Write(string.Format("Exception in restoring from failed log: {0}--{1}", ex.Message, ex.StackTrace));
				}
				finally
				{
					 if( logreader != null)
                                                logreader.Close();
                                        if( logwriter != null)
                                                logwriter.Close();
				}
				return retval;
			}

			public static int RestoreDataUsingWebAccess(string iFolderID, iFolderServer oldserver, string oldpath, iFolderServer newserver, string newpath, string relativepath, int startindex)
			{
				int retval = -1;
				int type = -1;
				int max = 100;
				NodeEntrySet entryset = null;
				//NodeEntry[] entries = null;
					/// fetch 100 nodes at a time from old server and iteratively parse them...
					string fullpath = Path.Combine(oldpath, relativepath);	
					if( File.Exists(fullpath))
						type = 2;
					else if( Directory.Exists(fullpath))
						type =1;
					else
						return retval;
					MainClass.DebugLog.Write(string.Format("RestoreDataUsingWebAccess fullpath: {0} relativepath: {1}", fullpath, relativepath));
					try
					{	
						int totalCount = 0;
						int CurrentCount = startindex;
						MainClass.DebugLog.Write(string.Format("calling getentries: {0}--{1}--{2}--{3}", iFolderID, type, relativepath, startindex));
						entryset = oldserver.GetEntries(iFolderID, type, relativepath, startindex, max, null);
						MainClass.DebugLog.Write(string.Format("Got get entries..."));
						totalCount = Convert.ToInt32(entryset.Count);
						MainClass.DebugLog.Write(string.Format("The total count of entryset: {0} relativepath: {1} file type: {2}", entryset.Count, relativepath, type));
						//NodeEntry[] entries = (NodeEntry[])entryset.Items;
						if( entryset.Items ==null )
						{
							MainClass.DebugLog.Write(string.Format("The entry set has no entries object"));
						}
						if( entryset.Items.Length == 0)
						{
							MainClass.DebugLog.Write(string.Format("The entry set has no entries object 1111"));
						}
						while(entryset.Items !=null && entryset.Items.Length != 0 && CurrentCount < totalCount)
						{
							foreach(NodeEntry entry in entryset.Items)
							{
								string Type = entry.Type;
								long length = entry.Length;
								string name = entry.Name;
								string filerelativepath = entry.RelativePath;
								try
								{
								if(!entry.RelativePath.Equals(relativepath) && !entry.RelativePath.Contains(relativepath+"/"))	
								{
									MainClass.DebugLog.Write(string.Format("Restore using webaccess, file excluded for restore is:{0}",filerelativepath));
									continue;
								}

								fullpath = Path.Combine(oldpath, filerelativepath);	
							if (Type.Equals("DirNode"))
								{
									//MainClass.DebugLog.Write("Directory Creation");
									if(!newserver.CreateDirectory(iFolderID, filerelativepath))
									{
										MainClass.DebugLog.Write(string.Format("Directory Creation failed for folder {0}-{1}-{2}", iFolderID, filerelativepath, (int)status.DirCreationFailed));
										MainClass.FailedLog.Write(string.Format("{0} {1} {2} {3} {4}", CurrentCount, entry.ID, length, filerelativepath, "failed"), false);
										//retValue = (int)status.DirCreationFailed;
										if(!newserver.PingServer())
										{
											retval = (int)status.CurrentServerNotRunning;
											 return retval;
										}	
									}
								}
								else if( Type.Equals("FileNode"))
								{
									//Call Web API to Create File
									if( File.Exists( fullpath ) == false)
									{
										MainClass.DebugLog.Write(string.Format("The source file: {0} does not exist.", fullpath));
										MainClass.FileNotFound.Write(string.Format("{0} {1} {2} {3} {4}", CurrentCount, entry.ID, length, filerelativepath, "NA"), false);
									}
									else if(!newserver.UploadFile( iFolderID, fullpath, filerelativepath, length))
									{
										MainClass.DebugLog.Write(string.Format("File Creation failed for folder {0}-{1}-{2}", iFolderID, filerelativepath, fullpath));
										MainClass.FailedLog.Write(string.Format("{0} {1} {2} {3} {4}", CurrentCount, entry.ID, length, filerelativepath, "failed"), false);
										if(!newserver.PingServer())
										{
											retval = (int)status.CurrentServerNotRunning;
											return retval;
										}		
									}
								}
								}/// end try inside for
								catch(Exception ex)
								{
									MainClass.DebugLog.Write(string.Format("Exception in uploading entry: {0}. message: {1}--{2}", name, ex.Message, ex.StackTrace));
								}
								/// Show the progress here...
								CurrentCount++;
								if( CurrentCount%20 == 0)
								{
									newserver.SetRestoreStatusForCollection( iFolderID, 2, totalCount, CurrentCount);
								}
								Console.Write("|               Processed {0} of {1} files.                                              |\r", CurrentCount, totalCount);
							}
							startindex+= entryset.Items.Length;
							try
							{
								entryset = oldserver.GetEntries(iFolderID, type, relativepath, startindex, max, null);

							}
							catch(	Exception ex)
							{
								MainClass.DebugLog.Write(string.Format("Exception in GetEntrie: {0}--{1}", ex.Message, ex.StackTrace));
								break;
							}
						}
						newserver.SetRestoreStatusForCollection( iFolderID, 2, totalCount, CurrentCount);
						retval = 0;
					}
					catch(Exception e)
					{
						MainClass.DebugLog.Write(string.Format("Exception in RestoreUsingWebAccess: {0}--{1}", e.Message, e.StackTrace));
					}
					return retval;

			}
		
			/// <summary>
            /// Prints iFolder policy to the logger. 
            /// </summary>
            /// <param name="ifdPolicy">iFolder policy object.</param>
                /// 
            /// <returns>void.</returns>
			public static void PrintPolicyToLogger(iFolderPolicy ifdPolicy)
			{
				StringBuilder includeFilter = new StringBuilder();
				StringBuilder excludeFilter = new StringBuilder();
				
				foreach(string inc in ifdPolicy.FileTypesIncludes){
					if(includeFilter.Length == 0){
						includeFilter.Append(inc);
					} else {
						includeFilter.AppendFormat(", {0}",inc);
					}
				}
				
	
				foreach(string exstr in ifdPolicy.FileTypesExcludes){
					if(excludeFilter.Length == 0){
						excludeFilter.Append(exstr);
					} else {
						excludeFilter.AppendFormat(", {0}",exstr);
					}
				}
					
				MainClass.DebugLog.Write(string.Format(" iFolderID-{0},\n\tLocked-{1} \n\tSpaceLimit - {2}, \n\tSyncInterval - {3},\n\tSharingStatus - {4},\n\tFileSizeLimit-{5},\n\tFileTypesIncludes - {6},\n\tFileTypesExcludes - {7} ,\n\tEffectiveSpaceLimit={8},\n\tSpaceAvailable={9}",
				                                       ifdPolicy.iFolderID, ifdPolicy.Locked, ifdPolicy.SpaceLimit,
				                                       ifdPolicy.SyncInterval, ifdPolicy.SharingStatus, ifdPolicy.FileSizeLimit, includeFilter.ToString(), excludeFilter.ToString(),ifdPolicy.SpaceLimitEffective,ifdPolicy.SpaceAvailable));
				
			}
	     /// <summary>
            /// Checks the Userr policy voilations after the data is restored.
            /// </summary>
            /// <param name="UserID"> User ID.</param>
            /// <param name="oldserver">iFolderServer object where the iFolder exists..</param>
            ///
            /// <returns>void.</returns>

            public static void CheckUserPolicyStatus(string ifolderID, iFolderServer oldServer, iFolderServer ifServer){
		UserPolicy usrPolicy = null;
		//Get the Owner from the iFolder and check if there is any violation in the user policies.
		iFolder ifld = oldServer.GetiFolder(ifolderID);
		String userID = ifld.OwnerID;
		String userName = ifld.OwnerUserName;
		if( userID != null) {
			usrPolicy = ifServer.GetUserPolicy(userID);
			if( usrPolicy != null) {
				if( !usrPolicy.LoginEnabled) {
                     Console.WriteLine("|               Warning: Data is restored into a iFolder owned by disabled User.         |");
    	         	     Console.WriteLine("|               User - {0} needs to be enabled to access the data.                       |",userName);
				}
				int ifCountPolicyStatus = ifServer.GetiFolderLimitPolicyStatus(userID);
				if ( ifCountPolicyStatus == 0){
					   Console.WriteLine("|               Warning: Restoring the data resulted in violation of iFolders per user policy.         |");
		               Console.WriteLine("|               Check the number of iFolders owned by user {0}                        |", userName);
				} else if ( ifCountPolicyStatus == -1){
						Console.WriteLine("|               Warning: Error while checking the iFolder limit policy.                              |");
				}
			}
		} else {
			Console.WriteLine("|               Warning: User - {0} does not exist on current iFolder server.         |",userName);
		}
			
			
	    }
		public static string ConvertSizeToString(long size) {
			long OneKB = 1024;
			long OneMB = 1024*OneKB;
			long OneGB = 1024*OneMB;
			
			if (size > OneGB) {
				float Gbs = (float)size/OneGB;
				return String.Format("{0:F}G",Gbs);
			} else if (size > OneMB){
				float Mbs = (float)size/OneMB;
				return String.Format("{0:F}M",Mbs);
			} else if (size > OneKB) {
				float Kbs = (float)size/OneKB;
				return String.Format("{0:F}K",Kbs);
			} else {
				return String.Format("{0}",size);
			}

		
		}
			/// <summary>
            /// Checks the iFolder policy voilations after the data is restored.
            /// </summary>
            /// <param name="iFolderID">iFolder ID.</param>
            /// <param name="oldserver">iFolderServer object where the iFolder exists..</param>
            /// 
            /// <returns>void.</returns>
	
			public static void CheckiFolderPolicyStatus(string iFolderID, iFolderServer ifServer) 
			{
				iFolderPolicy currentPolicy = null;
			
				currentPolicy = ifServer.GetiFolderPolicy(iFolderID );	
				if ( currentPolicy == null) {
						Console.WriteLine("|                                                                                        |");
						Console.WriteLine("|               Warning: Failed to check iFolder policies after restoring data.          |");
					return;
				}
				
				if( currentPolicy.SpaceLimitEffective != -1)//-1 means Policies are not set	
				if( ( currentPolicy.SpaceUsed > currentPolicy.SpaceLimitEffective )
					|| (currentPolicy.SpaceAvailable == 0))
				{
						Console.WriteLine("|                                                                                        |");
						Console.WriteLine("|               Warning: Restoring the data resulted in violation of the space limit     |");
						Console.WriteLine("|               policy for the iFolder. User should delete unused data to reduce used    |");
		  				Console.WriteLine("|               space to permissible limits. Space Used = {0}, Effective Space Limit={1} |",ConvertSizeToString(currentPolicy.SpaceUsed), ConvertSizeToString(currentPolicy.SpaceLimitEffective));
		
				}
			
				if(currentPolicy.Locked  )
				{
						Console.WriteLine("|                                                                                        |");
						Console.WriteLine("|               Warning: Data is restored into a disabled iFolder.                       |");
						Console.WriteLine("|               iFolder needs to be enabled for the user to access the data.             |");
						Console.WriteLine("|                                                                                        |");
		
				}
				
			}
			/// <summary>
            /// Restores the iFolder policy from the backup iFolder.
            /// </summary>
            /// <param name="iFolderID">.</param>
            /// <param name="oldserver">.</param>
            /// <param name="newserver">.</param>
            /// 
            /// <returns>returns 0 on Success and PolicyRestoreFailed on Failures.</returns>
			public static int RestoreiFolderPolicy( string iFolderID, iFolderServer oldserver, iFolderServer newserver) {
			    iFolderPolicy oldPolicy = null;
				iFolderPolicy newPolicy = null;
				int retval = 0;
				MainClass.DebugLog.Write(string.Format(" iFolderID-{0}, oldserver-{1}, newserver-{2}",iFolderID.ToString(), oldserver.ToString(),  newserver.ToString()));
				Console.WriteLine("|               Restoring iFolder policies                                              |");
				MainClass.DebugLog.Write(string.Format("Fetching iFolder policy details from backup"));
		        oldPolicy = oldserver.GetiFolderPolicy(iFolderID);

		        if(oldPolicy == null) {
						retval = (int)status.PolicyRestoreFailed;
	 	        }
				else 
				{
		        	MainClass.DebugLog.Write("\nBackup iFolder policy details: ");
					PrintPolicyToLogger(oldPolicy);
					newPolicy = newserver.GetiFolderPolicy(iFolderID );	
					MainClass.DebugLog.Write("\nCurrent iFolder policy details: ");
				 	if(newPolicy != null) {
						PrintPolicyToLogger(newPolicy);
					}
					MainClass.DebugLog.Write(string.Format("Restoring iFolder policy details "));
					retval = newserver.SetiFolderPolicy(oldPolicy);
				}
				if( retval == 0)
					Console.WriteLine("|               Restoring iFolder policies sucessful                                     |");
				return retval;
			}
		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
			public static int RestoreiFolderData( string iFolderID, iFolderServer oldserver, string oldpath, iFolderServer newserver, string newpath, string relativepath, bool usewebaccess)
			{
				MainClass.DebugLog.Write(string.Format(" iFolderID-{0}, oldserver-{1}, oldpath-{2}, newserver-{3}, newpath-{4}, relativepath-{5}, usewebaccess-{6}",iFolderID.ToString(), oldserver.ToString(), oldpath.ToString(), newserver.ToString(), newpath.ToString(), relativepath.ToString(), usewebaccess.ToString()));
				int retval =0;
				int startindex = 0;
				int totalcount, finishedcount;
			
				if( runpreviousiter && previousstatus != null)
				{
					MainClass.DebugLog.Write(string.Format("Previous status: {0}", previousstatus));
					if( previousstatus == "StartRecovery" || previousstatus == "EndRecovery" || previousstatus == "RemoveMembers" || previousstatus == "Completed" ) // Start Recovery...
					{
						Console.WriteLine("|               Trying the previous run again for failed restore                        |");
						/// try to fetch the place we stopped previously...
						/// Also if the failed file exists, call the api that processes that information as well...
						retval = newserver.GetRestoreStatusForCollection(iFolderID, out totalcount, out finishedcount);	
						// Previous iteration has completed...
						if( totalcount == finishedcount)
							startindex = -1;
						else
							startindex = finishedcount;
						 MainClass.DebugLog.Write(string.Format("Return value of getrestore status: {0}--{1}", totalcount, finishedcount));
					}
				}

				if( startindex != -1)
				{
					if( usewebaccess)
					{
						/// call the webservice method and wait for the response...
						retval = RestoreDataUsingWebAccess(iFolderID, oldserver, oldpath, newserver, newpath, relativepath, startindex);

						if(retval != 0)
							return retval;
					}
					else
					{
						 MainClass.DebugLog.Write(string.Format("Calling RestoreDataLocally"));
						retval = RestoreDataLocally(iFolderID, oldserver, oldpath, newserver, newpath, relativepath, startindex, FailedLog.LogFile);
						if( retval == 0)
							Console.WriteLine("\n|               Restore process Successful.                                             |");
						else
							Console.WriteLine("\n|               Restore process failed.                                                  |");
					}

				}
			
				if( runpreviousiter )
				{
					 MainClass.DebugLog.Write(string.Format("In runprevious iter"));
					/// Perform the restore from the file entries present...
					//if( usewebaccess )
						retval = RestoreFromFailedLog( FailedLog.LogFile, iFolderID, oldserver, oldpath, newserver, newpath);
					//else
					//{
						/// Need to call the api present in iFolder.cs of admin web service...
						//retval = newserver.RestoreFromFailedLog();
					//}
				}
			
				retval = newserver.GetRestoreStatusForCollection(iFolderID, out totalcount, out finishedcount);
				if( totalcount == finishedcount && retval == 0)
				{
					 MainClass.DebugLog.Write(string.Format("Cleaning the restore status"));
					newserver.SetRestoreStatusForCollection( iFolderID, -1, -1, -1);
					if( runpreviousiter )
						Console.WriteLine("|               Retry Operation Completed Successfuly.                                   |");
				}
				else
				{
					//Read failed files and Print number of failed entries	
					if( runpreviousiter )
						Console.WriteLine("|               Retry Operation Completed.                                               |");

				}
			
				return retval;
			}
		

		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
			public static void PrintHelp()
			{

                                Console.WriteLine("Command For Execution: $ifolder-data-recovery <Operation> <Arguments>\n");
                                Console.WriteLine("Operation:\n\t-l, --list\tLists iFolders owned by the specified user and details such as Name,\n\t\t\tiFolderID, and Path (at the time of backup)");
                                Console.WriteLine("\t-r, --restore\tRestore requested data (File/Folder/iFolder) from specified backup store.");
                                Console.WriteLine("\t--retry\t\tRetry restore operation for failed data in last run.");
                                Console.WriteLine("\t-h, --help\tPrint help regarding Operation, argument and usage.");
                                Console.WriteLine("\t\nArguments:\n\t--path\t\t\tPath of simias directory that has the iFolder backup store, FlaimSimias.db");
                                Console.WriteLine("\t-U, --backup-admin\tLogin name of iFolder administrator who performed the backup");
                                Console.WriteLine("\t-u,--current-admin\tiFolder administrator login name for the current server. Use this Option if backup \n\t\t\t\tadministrator is different from current administrator.");
                                Console.WriteLine("\t--server-url\t\tPublic URL of the iFolder server where data is to be restored");
                                Console.WriteLine("\t--user\t\t\tUsername of the user for whom the specified operation is to be performed");
                                Console.WriteLine("\t--ifolder-id\t\tID of the iFolder for which the specified operation is to be performed");
                                Console.WriteLine("\t--ifolder-path\t\tAbsolute path (excluding the iFolder name) to the location where iFolder backup data is available. ");
                                Console.WriteLine("\t--relative-path\t\tRelative path of file/folder to be restored, starting from iFolder name");
//                                Console.WriteLine("\t--usewebaccess\t\tSpecifies the mode to restore. Does not take any value.");
                                Console.WriteLine("\t--usewebaccess\t\tSpecifies the mode of restore. Use this option while restoring the data to a remote iFolder Server.");
				Console.WriteLine("\t--restore-policies\tOverwrites current iFolder policies with the policies of the iFolder from backup");
                                Console.WriteLine("\t\nExamples:");
                                Console.WriteLine("\n\tFor Help:");
                                Console.WriteLine("\t\t$./ifolder-data-recovery --help");

                                Console.WriteLine("\n\tFor Listing iFolder for given user:");
                                Console.WriteLine("\t\t$./ifolder-data-recovery --list  --path=/home/recovery/data/ --backup-admin=admin --user=user1");

                                Console.WriteLine("\n\tFor Restoring iFolder:");
                                Console.WriteLine("\t\t$./ifolder-data-recovery --restore  --path=/home/recovery/data/ --backup-admin=oldadmin --current-admin=admin --server-url=https://192.162.1.10 --ifolder-id=7fe6cd5d-40d4-4982-bfa3-94292d4e36ab --ifolder-path=/home/ifolder/7fe6cd5d-40d4-4982-bfa3-94292d4e36ab");

                                Console.WriteLine("\n\tFor Restoring directory inside a iFolder:");
                                Console.WriteLine("\t\t$./ifolder-data-recovery --restore  --path=/home/recovery/data/ --backup-admin=admin  --server-url=http://192.162.1.10 --ifolder-id=7fe6cd5d-40d4-4982-bfa3-94292d4e36ab --ifolder-path=/home/ifolder/7fe6cd5d-40d4-4982-bfa3-94292d4e36ab --relative-path=MyiFolder/Subdir");

                                Console.WriteLine("\n\tFor Restoring File:");
                                Console.WriteLine("\t\t$./ifolder-data-recovery --restore  --path=/home/recovery/data/ --backup-admin=oldadmin --current-admin=admin --server-url=http://192.162.1.10 --ifolder-id=7fe6cd5d-40d4-4982-bfa3-94292d4e36ab --ifolder-path=/home/ifolder/7fe6cd5d-40d4-4982-bfa3-94292d4e36ab --relative-path=MyiFolder/Subdir/MyFile.txt");

                                Console.WriteLine("\n\tFor Retrying:");
                                Console.WriteLine("\t\t$./ifolder-data-recovery --retry  --path=/home/recovery/data/ --backup-admin=oldadmin --current-admin=admin --server-url=http://192.162.1.10 --ifolder-id=7fe6cd5d-40d4-4982-bfa3-94292d4e36ab --ifolder-path=/home/ifolder/7fe6cd5d-40d4-4982-bfa3-94292d4e36ab --relative-path=MyiFolder/Subdir\n");

			}

			public static void PrintResult( int result)
			{
				if( result < 100)
					return;
				switch(result)
				{
				        case (int)status.Help:
                                                Console.WriteLine("\n|               Show Help completed                                                      |");
                                                break;
                                        case (int)status.Success:
                                                Console.WriteLine("\n|               Restore Operation completed sucessfuly.                                  |");
                                                break;
                                        case (int)status.Failed:
                                                Console.WriteLine("\n|               Restore Operation failed.                                                |");
                                                break;
										 case (int)status.InvalidInput:
                                              Console.WriteLine("\n|               Restore Operation failed,Input data is Invalid.                          |");
                                        break;
                                        case (int)status.InvalidBackupPath:
							                  Console.WriteLine("|                 Invalid path specified for option --path              |        ");   
                                                break;
                                        case (int)status.InvalidFormat:
                                                Console.WriteLine("\n|               Restore Operation failed , Input data Format is invalid.                 |");
                                                break;
                                        case (int)status.MissingFields:
                                                Console.WriteLine("\n|               Restore Operation failed, one or more Mandatory Field are missing.       |");
                                                break;
                                        case (int)status.DataPathDoesNotExist:
                                                Console.WriteLine("\n|               Restore Operation failed, Input data path is invalid.                    |");
                                                break;
                                        case (int)status.BackupServerNotRunning:
                                                Console.WriteLine("\n|               Restore Operation failed, Backup server is inaccessible,Stoped or Killed.|");
                                                break;
                                        case (int)status.CurrentServerNotRunning:
                                                Console.WriteLine("\n|               Restore Operation failed, Current server is inaccessible,Stoped or Killed.|");
                                                break;
                                        case (int)status.InvalidCredentials:
                                                Console.WriteLine("\n|               Restore Operation failed, input credential are Invalid.                  |");
                                                break;
					case (int)status.EmptyAdminID:
                                                Console.WriteLine("\n|               Restore Operation failed, input admin login name is empty.               |");
                                                break;
                                        case (int)status.InvalidiFolderIdOld:
                                                Console.WriteLine("\n|               Restore Operation failed, ifolderID does not exist in back up store.     |");
                                                break;
                                        case (int)status.ListingiFolder:
                                                Console.WriteLine("\n|               iFolder Listing completed                                                |");
                                                break;
                                        case (int)status.FailediFolderCreation:
                                                Console.WriteLine("\n|               Restore Operation failed, ifolder creation failed.                       |");
                                                break;
                                        case (int)status.BackupExistenceFailed:
                                                Console.WriteLine("\n|               Restore Operation failed, invalid input backup data path.                |");
                                                break;
                                        case (int)status.InvalidiFolderIdNew:
                                                Console.WriteLine("\n|               Restore Operation failed, ifolderID is invalid or does not exist in current server.|");
                                                break;
                                        case (int)status.InvalidRelativePath:
                                                Console.WriteLine("\n                Restore Operation failed, input relative path is invalid.");
                                                break;
                                        case (int)status.InvalidXMLFileLocation:
                                                Console.WriteLine("\n|               Restore Operation failed, XML file location is invalid or does not exist.|");
                                                break;
                                        case (int)status.FailedRestoreWebCall:
                                                Console.WriteLine("\n|               Web Call to Restore Failed.                                              |");
                                                break;
                                        case (int)status.DirCreationFailed:
                                                Console.WriteLine("\n|               Directory Creation Operation Failed.                                     |");
                                                break;
                                        case (int)status.FileUploadFailed:
                                                Console.WriteLine("\n|               File Upload Operation Failed.                                            |");
                                                break;
                                        case (int)status.XmlWrittingFailed:
                                                Console.WriteLine("\n|               XML Update Operation Failed.                                             |");
                                                break;
                                        case (int)status.InvalidUserName:
                                                Console.WriteLine("\n|               Invalid User Name, User name does not exist in old data path.            |");
                                                break;
                                        case (int)status.EmptyNewServerUrl:
												Console.WriteLine("|               		Restore Operation failed, iFolder owner does not exist on the iFolder Server                                             |");
                                                break;
				                        case (int)status.PolicyRestoreFailed:
                                                Console.WriteLine("\n|               iFolder Policy Restore Failed.                                        |");
                                                break;
                                        default:
                                                Console.WriteLine("\n|               iFolder data restore Failed with error code: {0}                         |", result);
                                                break;

				}
			}

                /// <summary>
                /// Prompt for a Password String Value
                /// </summary>
                /// <param name="prompt">The Prompt String</param>
                /// <param name="defaultValue">The Default Value</param>
                /// <returns>A String Object</returns>
                public static string ForPasswordString(string prompt, string defaultValue)
                {
                        Console.Write("{0}", prompt);
                        string password = "";

                        ConsoleKeyInfo info = Console.ReadKey(true);
                        while (info.Key != ConsoleKey.Enter)
                        {
                                if (info.Key != ConsoleKey.Backspace)
                                {
                                        password += info.KeyChar;
                                        info = Console.ReadKey(true);
                                }
                                else if (info.Key == ConsoleKey.Backspace)
                                {
                                        if (!string.IsNullOrEmpty(password))
                                        {
                                                password = password.Substring(0, password.Length - 1);
                                        }
                                        info = Console.ReadKey(true);
                                }
                        }
                        if (string.IsNullOrEmpty(password))
                        {
                                password = defaultValue;
                        }
                        return password;
                }

                /// <summary>
                /// Calculate number of lines in a given file
                /// </summary>
                /// <param name="fileName">String specify file name</param>
                /// <returns>A integer, number of line in given file</returns>
                public static long NumberOfLines(string fileName)
		{
			MainClass.DebugLog.Write(string.Format("Enter Function NumberOfLines for file:{0}",fileName));	
			//string line  = null;
			long numOfLines = 0;
			TextReader reader = null;
			if( !String.IsNullOrEmpty(fileName) )
			{
				try{
					using( reader = (TextReader)File.OpenText(fileName) )
					{
						while(  reader.ReadLine() != null )
						{
							numOfLines++;	
						}
						reader.Close();
					}
				}
				finally
				{
					if(reader != null)
						reader.Close();	
				}
			}
			MainClass.DebugLog.Write(string.Format("Exit Function NumberOfLines for with number of Lines:{0}",numOfLines));	
			return numOfLines;
		} //End of function NumberOfLines

	} //End of Main Class






        class Utility
        {
		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
                public static void GetApacheUserGroup(out string apacheUser, out string apacheGroup)
                {
                        apacheUser = apacheGroup = "";
                        try
                        {
                                // uid.conf
                                using( TextReader reader = (TextReader)File.OpenText( Path.GetFullPath( "/etc/apache2/uid.conf" ) ) )
                                {
                                        string line;
                                        while( ( line = reader.ReadLine() ) != null )
                                        {
                                                if ( line.StartsWith( "User" ) )
                                                {
                                                         apacheUser = line.Split()[1];
                                                }
                                                else if ( line.StartsWith( "Group" ) )
                                                {
                                                         apacheGroup = line.Split()[1];
                                                }
                                        }
                                        reader.Close();
                                }
                        }
                        catch(Exception ex)
                        {
                                apacheUser = "";
                                apacheGroup = "";
			    MainClass.DebugLog.Write(string.Format("Exception while reading apache uid.conf file:{0}",ex.Message));	
                        }
                }

                /// <summary>
                /// Execute the command in the shell.
                /// </summary>
                /// <param name="command">The command.</param>
                /// <param name="format">The arguments of the command.</param>
                /// <param name="args">The arguments for the format.</param>
                /// <returns>The results of the command.</returns>
                public static int Execute(string command, string format, params object[] args)
                {
                        ProcessStartInfo info = new ProcessStartInfo( command, String.Format( format, args ) );
                        Process p = Process.Start(info);
                        p.WaitForExit();
                        return p.ExitCode;
                }

		/// <summary>
		/// Read the /etc/apache2/conf.d/simias.conf File and return data path. 
		/// </summary>
		public static string ReadModMonoConfiguration()
		{
			string path = Path.GetFullPath( "/etc/apache2/conf.d/simias.conf" );
			string dataPath = null;
			if ( path == null ||  File.Exists( path ) == false )
				return null;
			try 
        		{
            			using(StreamReader sr = new StreamReader(path))
            			{
                			string line;
					string SearchStr = "MonoSetEnv simias10 \"SimiasRunAsServer=true;SimiasDataDir=";
					char[] delimiter = {';'};
					char[] delimiter1 = {'='};
					char[] trimchar= {'"'};
					string[] entryset = null;
					string logpath= null;
					//string simiasDir = null;
				
                			while ((line = sr.ReadLine()) != null) 
                			{
						if( !String.IsNullOrEmpty(line) && line.StartsWith(SearchStr) == true )
						{
							int startIndex  = line.LastIndexOf('=');
							dataPath = line.Substring(startIndex+1,(line.Length - startIndex - 2 ));


							entryset = line.Split(delimiter);

							foreach(string val in entryset)	
							{
								if(val.StartsWith("SimiasDataDir"))
								{
									logpath=val.Split(delimiter1)[1];		
									dataPath= logpath.Trim(trimchar);
									break;
								}	
							}	
            						//Console.WriteLine("The datapath is:{0} and path is:{1}", dataPath, logpath);
            						//MainClass.DebugLog.Write(string.Format("The datapath is:{0} and path is:{1}", dataPath, logpath));
							


							break;
						}
                			}
					if( dataPath != null)
					{
						dataPath = Path.Combine( dataPath, "log");
						dataPath = Path.Combine( dataPath, "ifrecovery");
						if( Directory.Exists(dataPath) == false)
							Directory.CreateDirectory( dataPath);
					}
            			}
        		}
        		catch (Exception e) 
        		{
            			MainClass.DebugLog.Write(string.Format("The file {0} could not be read: {1}", path, e.Message));
				return null;
        		}
			return dataPath;
		}


	} //End of Class Utility	

        public class Logger
        {
                private string logFile;
                private StreamWriter stream;
                public Logger( string fileName)
                {
                        this.logFile = fileName;
                        stream = File.AppendText(logFile);
			stream.Close();
			stream = null;
                }

                public Logger()
                {
                        this.logFile = null;
                }

		public string LogFile
		{
			get
			{
				return this.logFile;
			}
		}

                public override string ToString()
                {
                        if( this.logFile == null)
                        {
                                return "logfile is null. redirected to stderr.";
                        }
                        else
                                return this.logFile;
                }

		/// <summary>
                /// 
                /// </summary>
                /// <param name="collectionID">.</param>
                /// <returns>.</returns>
                public void Write(string Message)
		{
			this.Write( Message, true);
		}
                public void Write(string Message, bool timestring)
                {
                        try
                        {
                                if( String.IsNullOrEmpty(this.logFile))
                                {
                                        //MainClass.DebugLog.Write(string.Format("{0}: {1}", DateTime.Now.ToString("f"), Message));
                                }
                                else
                                {
                                        if( stream == null)
                                        {
                                                stream = File.AppendText(logFile);
                                        }
					if( timestring)
	                                        stream.WriteLine(string.Format("{0}: {1}", DateTime.Now.ToString("f"), Message));
					else
						stream.WriteLine(Message);
                                        stream.Close();
                                        stream = null;
                                }
                        }
                        catch
                        {
                        }
                }

                public void Stop()
                {
                        try
                        {
                                if( String.IsNullOrEmpty (this.logFile))
                                        return;
                                if( stream == null)
                                        return;
                                stream.Close();
                                stream = null;
                        }
                        catch
                        {
                        }
                }
		
		public long Size
		{
			get
			{
				try
				{
					FileInfo finfo = new FileInfo(this.logFile);
					return finfo.Length;
				}
				catch
				{
					return 0;
				}
			}
		}
        }


}
