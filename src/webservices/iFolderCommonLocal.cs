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
*                 $Author: Rob
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
using System.Web;
using System.Text;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections;

using Simias.Storage;
using Simias.Server;

namespace iFolder.WebService
{
	/// <summary>
	/// iFolder Common Local Web Service
	/// </summary>
	public abstract class iFolderCommonLocal : System.Web.Services.WebService
	{
		#region Constructors
		
		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderCommonLocal()
		{
		}
		
		#endregion

		#region System
		
		/// enum to store the different sharing policy combination
		
		/// <summary>
		/// Get information about the iFolder system.
		/// </summary>
		/// <returns>An iFolderSystem object describing the system.</returns>
		[WebMethod(
			 Description="Get information about the iFolder system.",
			 EnableSession=true)]
		public virtual iFolderSystem GetSystem()
		{
			iFolderSystem result = null;

			try
			{
				Authorize();

				result = iFolderSystem.GetSystem();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about the authenticated user's home iFolder server.
		/// </summary>
		/// <returns>An iFolderServer object describing the user's home iFolder server.</returns>
		[WebMethod(
			 Description="Get information about the authenticated user's home iFolder server.",
			 EnableSession=true)]
		public virtual iFolderServer GetHomeServer()
		{
			iFolderServer result = null;

			try
			{
				Authorize();

				result = iFolderServer.GetHomeServer();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get the list of all the server names.
		/// </summary>
		/// <returns> a string array of server names </returns>
		[WebMethod(
			 Description="Get the list of all the servers available .",
			 EnableSession=true)]
		public virtual string [] GetServerProvisioningList()
		{
			string [] result = null;
			iFolderServer [] ServerList = null;

			try
			{
				Authorize();

				ServerList = iFolderServer.GetServers();
				result = new string [ServerList.Length];
				int index ;
				if( ServerList.Length <= 0 )
				{
					return result;
				}
				for (index = 0; index < ServerList.Length ; index++ )
				{
					iFolderServer serverobject = ServerList[index];
					result[index] = serverobject.Name;
				} 
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get the location of the iFolder
		/// </summary>
		/// <returns>The PrivateUrl of the iFolder's HomeServer.</returns>
		[WebMethod(
			 Description="Get information about the authenticated user's home iFolder server.",
			 EnableSession=true)]
		public virtual string GetiFolderLocation ( string ifolderID )
		{
		        string result = null;

			try
			{
				Authorize();

				result = iFolder.GetiFolderLocation ( ifolderID );
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get all the searched orphaned ifolders 
		/// </summary>
		/// <returns>a list of orphaned ifolders. </returns>
		
		[WebMethod(
			 Description="Get Orphaned iFolders",
			 EnableSession=true)]
		public virtual iFolderSet GetOrphanediFolders(SearchOperation operation, string pattern, int index, int max  )
		{
			iFolderSet OrphiFolderList = new iFolderSet();
			try
			{
				Authorize();

				OrphiFolderList = iFolder.GetOrphanediFolders ( operation, pattern, index, max, GetAccessIDForGroup() );
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return OrphiFolderList;
		}

		/// <summary>
		/// Checks whether an ifolder is orphaned or not 
		/// </summary>
		/// <returns>. string 'false' if the ifolder is not orphaned otherwise returns userID of prev owner</returns>

		[WebMethod(
			 Description="check orphaned property",
			 EnableSession=true)]
		public virtual string IsOrphanediFolder(string iFolderID)
		{
			string isorphaned = "";
			
			try
			{
				Authorize();

				isorphaned = iFolder.IsOrphanediFolder (iFolderID, GetAccessID() );
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return isorphaned;
		}	

		/// <summary>
		/// return the shortened string 
		/// </summary>
		/// <returns>short string </returns>
		[WebMethod(
			 Description="shorten the given string and return shortname",
			 EnableSession=true)]
		public virtual string GetShortenedName(string name, int length)
		{
			StringBuilder ShortenedName = new StringBuilder("",length);
			
			try
			{
				Authorize();
				//ShortenedName = new StringBuilder("",length);
				for (int index=0 ; index<name.Length ; index++)
				{
					if(index < length-10) //40)
					{
						ShortenedName.Append(name[index]);
					}
					//if(index >= 40 && index < 45)
					if(index >= (length-10) && index < (length-5))
					{
						ShortenedName.Append('.');
					}
					if(index == (name.Length - 5))
					{
						for (int subindex = 0 ; subindex < 5 ; subindex++)
						{
							ShortenedName.Append(name[index++]);
						}
						break;
					}
				}
		
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return ShortenedName.ToString(); 
		}	
		
		/// <summary>
		/// Get the list of recovery agents
		/// </summary>
		/// <returns>The list containing recovery agents name</returns>
		[WebMethod(
			 Description="Get the list of recovery agents",
			 EnableSession=true)]
		public virtual string[] GetRAList ()
		{
		        string[] result = null;

			try
			{
				Authorize();

				result = iFolder.GetRAList ();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
		
		/// <summary>
		/// Get the RA Certificate
		/// </summary>
		/// <returns>The byte array containing the certificate</returns>
		[WebMethod(
			 Description="Get the RA Certificate",
			 EnableSession=true)]
		public virtual byte[] GetRACertificate(string rAgent)
		{
		        byte [] result = null;

			try
			{
				Authorize();

				result = iFolder.GetRACertificate (rAgent);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
	
		/// <summary>
		/// Get the PassPhrase Status 
		/// </summary>
		/// <returns>The Status of pass-phrase</returns>
		[WebMethod(
			 Description="Get the pass-phrase status",
			 EnableSession=true)]
		public virtual bool IsPassPhraseSet ()
		{
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);
		   // Simias.Authentication.Status result = new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
			bool result = false;

			try
			{
				Authorize();

				result = iFolderUser.IsPassPhraseSet(domain.ID, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
		
		///<summary>
		///Validate the passphrase for the correctness
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Validate the passphrase for the correctness.")]
		[SoapDocumentMethod]	
		public virtual Simias.Authentication.Status ValidatePassPhrase(string passPhrase)
		{
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);
		    Simias.Authentication.Status result = new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
		    try
			{
				Authorize();

				result = iFolderUser.ValidatePassPhrase(domain.ID, passPhrase, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
	
		}
		
		///<summary>
		///Set the passphrase and recovery agent
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Set the passphrase and recovery agent.")]
		[SoapDocumentMethod]
		public virtual void SetPassPhrase(string passPhrase, string recoveryAgentName, string publicKey)
		{
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);
		    try
			{
				Authorize();

				iFolderUser.SetPassPhrase(domain.ID, passPhrase, recoveryAgentName, publicKey, GetAccessID() );
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

		}

		///<summary>
		// Change the password for currently logged in user/
		///</summary>
		///<returns>enum state explaining the status of this operation</returns>
		[WebMethod(EnableSession=true, Description="Change the password for currently logged in user")]
		[SoapDocumentMethod]
		public virtual int ChangePassword(string OldPassword, string NewPassword)
		{
			int status = 0;
			try
			{
				Authorize();
				status = Simias.Server.User.ChangePassword(GetAccessID(), OldPassword, NewPassword);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return status;
		}

		///<summary>
		///checks if for this user, encryption is enforced
		///</summary>
		///<returns>true/false</returns>
		[WebMethod(EnableSession=true, Description="check if encryption is enforced for this user")]
		[SoapDocumentMethod]	
		public virtual bool IsUserOrSystemEncryptionEnforced(string userID)
		{
			bool result = false;
		    	try
			{
				Authorize();
				result = iFolderUser.IsUserOrSystemEncryptionEnforced(userID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return result;
		}
		
		/// <summary>
                /// whether this id is a group id or not
                /// </summary>
                [WebMethod(
                        Description="decides whether this id is group id or not ",
                        EnableSession=true)]
                public virtual bool IsGroupId(string UserID)
                {	
			Store store = Store.GetStore();
                        Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);
			bool IsGroup = false;
	                try
                        {
                                Authorize();
				Member Gmember = domain.GetMemberByID(UserID);
                                Property Groupproperty = Gmember.Properties.GetSingleProperty( "GroupType" );
                                if(null != Groupproperty )
                                	IsGroup = true;
                        }
                        catch(Exception e)
                        {
                                SmartException.Throw(e);
                        }
			return IsGroup;
		}

		/// <summary>
		/// Get the policy for an iFolder.
		/// </summary>
		/// <param name="policy">The iFolderPolicy object.</param>
		[WebMethod(
			Description="returns the higher priority policy which is applicable in the present situation ",
			EnableSession=true)]
		public virtual int GetSharingStatus(string iFolderID)
		{
			int result = 0;
			try
                        {
				Authorize();
				Store store = Store.GetStore();
				Collection col = store.GetCollectionByID(iFolderID);
				string accessID = GetAccessIDForGroup();
				if( accessID != null)
				{
					// thow access denied exception in else part...
					if( IsAccessAllowed(col.Owner.ID) )
						accessID = null;
					else
						return -1;
				}
				iFolderPolicy ifolder = iFolderPolicy.GetPolicy(iFolderID, GetAccessID());
				result = ifolder.SharingStatus;
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return result ;
			
		}

		
		/// <summary>
		/// Disable Past sharing for whole system  
		/// </summary>
		[WebMethod(
			Description="Disables the past sharing by removing members list from all the iFolders of the system ",
			EnableSession=true)]
		public virtual void DisableSystemPastSharing()
		{
			string iFolderCollectionType = "iFolder";
			try
                        {
				Authorize();
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);
				ICSList ColList = store.GetCollectionsByDomain(domain.ID);
				if ( ColList.Count > 0 )
				{
					foreach( ShallowNode sn in ColList )
					{
						 Collection c = store.GetCollectionByID(sn.ID);
						if (c != null && c.IsType(iFolderCollectionType))
						{
							/// got an iFolder, now remove the member list 
							ICSList MemberList = c.GetMemberList();
							foreach (ShallowNode MemberNode in MemberList)
							{
								Member member = new Member (domain, MemberNode);
								if(c.Owner.UserID != member.UserID)
								{
									RemoveMember(c.ID, member.UserID);

								}
							}
						}
					}

				}				
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			
		}

		/// <summary>
		/// Disable Past sharing for whole system  
		/// </summary>
		[WebMethod(
			Description="Disables the past sharing by removing members list from all the iFolders of the system ",
			EnableSession=true)]
		public virtual void DisableGroupPastSharing()
		{
			string iFolderCollectionType = "iFolder";
			try
                        {
				Authorize();
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);
				Member groupadmin = domain.GetMemberByID(GetAccessIDForGroup());
				Hashtable ht = groupadmin.GetMonitoredUsers(true);
				ICSList ColList = store.GetCollectionsByDomain(domain.ID);
				if ( ColList.Count > 0 )
				{
					foreach( ShallowNode sn in ColList )
					{
						Collection c = store.GetCollectionByID(sn.ID);
						if( ht.ContainsKey(c.Owner.UserID) == false)
							continue;
						if (c != null && c.IsType(iFolderCollectionType))
						{
							/// got an iFolder, now remove the member list 
							ICSList MemberList = c.GetMemberList();
							foreach (ShallowNode MemberNode in MemberList)
							{
								Member member = new Member (domain, MemberNode);
								if(c.Owner.UserID != member.UserID)
								{
									RemoveMember(c.ID, member.UserID);

								}
							}
						}
					}

				}				
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			
		}	

		/// <summary>
		/// Disable Past sharing for a particular user  
		/// </summary>
		[WebMethod(
			Description="Disables the past sharing by removing members list from all the iFolders of the user ",
			EnableSession=true)]
		public virtual void DisableUserPastSharing(string UserID)
		{
			if( IsAccessAllowed(UserID) == false)
			{
				/// thorw access violation exception
				return;
			}
			string iFolderCollectionType = "iFolder";
			Store store = Store.GetStore();
			bool Group = false;
			Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);
			try
                        {
				Authorize();
				if(Simias.Service.Manager.LdapServiceEnabled == true)
				{
					Member Gmember = domain.GetMemberByID(UserID);
                        		Property Groupproperty = Gmember.Properties.GetSingleProperty( "GroupType" );
                        		if(null != Groupproperty )
						Group = true;
					if(Group == true)
					{
						// to remove users of this group who are provisioned on some other server
						
					
						string[] MIDs = domain.GetGroupsMemberList(UserID);
                                		foreach(string id in MIDs)
                                		{
							// proceed only when the local hostnode is same as the hostnode where user is provisioned
							Member MemberOfGroup = domain.GetMemberByID(id);
							HostNode hNode = MemberOfGroup.HomeServer;
							if( hNode != null )
							if(hNode.IsLocalHost)
							{
								ICSList ColList = store.GetCollectionsByOwner(id);			
								foreach (ShallowNode sn in ColList)
								{
									Collection c = store.GetCollectionByID(sn.ID);
									if (c != null && c.IsType(iFolderCollectionType))
									{
										/// got an iFolder, now remove the member list
										ICSList MemberList = c.GetMemberList();
										foreach (ShallowNode MemberNode in MemberList)
										{
											Member member = new Member (domain, MemberNode);
											if(c.Owner.UserID != member.UserID)
											{
												RemoveMember(c.ID, member.UserID);
		
											}
										}
									}
								}
                                			}
						}
					}
				}

				if(Simias.Service.Manager.LdapServiceEnabled == false || Group == false)
				{
					ICSList ColList = store.GetCollectionsByOwner(UserID);			
					foreach (ShallowNode sn in ColList)
					{
						Collection c = store.GetCollectionByID(sn.ID);
						if (c != null && c.IsType(iFolderCollectionType))
						{
							/// got an iFolder, now remove the member list
							ICSList MemberList = c.GetMemberList();
							foreach (ShallowNode MemberNode in MemberList)
							{
								Member member = new Member (domain, MemberNode);
								if(c.Owner.UserID != member.UserID)
								{
									RemoveMember(c.ID, member.UserID);
	
								}
							}
						}
					}
				}

			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		
		/// <summary>
		/// Disable Past sharing for an iFolder  
		/// </summary>
		[WebMethod(
			Description="Disables the past sharing by removing members list from the iFolder ",
			EnableSession=true)]
		public virtual void DisableiFolderPastSharing(string iFolderID)
		{
			string iFolderCollectionType = "iFolder";
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);

			try
                        {
				Authorize();
				Collection c = store.GetCollectionByID(iFolderID);
				if (c != null && c.IsType(iFolderCollectionType))
				{
					if( !IsAccessAllowed(c.Owner.ID) )
					{
						/// throw access violation exception...
						return;
					}
					/// got an iFolder, now remove the member list
					ICSList MemberList = c.GetMemberList();
					foreach (ShallowNode MemberNode in MemberList)
					{
						Member member = new Member (domain, MemberNode);
						if(c.Owner.UserID != member.UserID)
						{
							RemoveMember(c.ID, member.UserID);

						}
					}
				}

			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Get information about an iFolder Server.
		/// </summary>
		/// <param name="serverID">The id of the iFolder Server.</param>
		/// <returns>An iFolderServer object describing the iFolder Server.</returns>
		[WebMethod(
			 Description="Get information about an iFolder Server.",
			 EnableSession=true)]
		public virtual iFolderServer GetServer(string serverID)
		{
			iFolderServer result = null;

			if(GetAccessIDForGroup() != null)
			{
				/// For a groupadmin dont show any servers..
				return null;
			}

			try
			{
				Authorize();

				result = iFolderServer.GetServer(serverID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
		
		/// <summary>
		/// Get information about all the iFolder servers.
		/// </summary>
		/// <returns>An array of iFolderServer objects.</returns>
		[WebMethod(
			 Description="Get information about all the iFolder servers.",
			 EnableSession=true)]
		public virtual iFolderServer[] GetServers()
		{
			iFolderServer[] result = null;

			if(GetAccessID() != null)
			{
				/// For a groupadmin dont show any servers..
				return null;
			}

			try
			{
				Authorize();

				result = iFolderServer.GetServers();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		[WebMethod(
			 Description= "Get information whether server is supporting multibyte login or not.",
			 EnableSession = true)]
		public virtual string GetServerStatus()
		{
		    string result = null;

		    try 
		    {
		        result = iFolderServer.GetServerStatus();
		    } 
		    catch (Exception e)
		    {
			SmartException.Throw (e);
		    }

		    return result;
		}


		/// <summary>
		/// Get the home server for the user
		/// </summary>
		/// <returns>Publiv url of the home server.</returns>
		[WebMethod(
			 Description="Get the home server for the user.",
			 EnableSession=true)]
		public virtual string GetHomeServerForUser( string username , string password)
		{
			/// Not using this anywhere...
		        string result;

			result = iFolderServer.GetHomeServerForUser ( username, password );

			return result;
		}

                /// <summary>
                /// Get the home server for the user using admin credential
                /// </summary>
                /// <returns>Publiv url of the home server.</returns>
                [WebMethod(
                         Description="Get the home server for the user.",
                         EnableSession=true)]
                public virtual string GetHomeServerURLForUserID( string userid )
                {
                        string result=null;

			if( IsAccessAllowed(userid) )
	                        result = iFolderServer.GetHomeServerURLForUserID( userid );

                        return result;
                }

		/// <summary>
		/// Get reports
		/// </summary>
		/// <returns>An array of iFolderServer objects.</returns>
		[WebMethod(
			 Description="Get information about the reports.",
			 EnableSession=true)]
		public virtual string[] GetReports()
		{
			string[] result = null;

			try
			{
				Authorize();

				if( GetAccessIDForGroup() != null)
				{
					/// throw access not allowed exception...
				}
				else
					result = iFolderServer.GetReports ();
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about iFolder Servers identified by a search on name.
		/// </summary>
		/// <param name="operation">The operation to compare the name and pattern.</param>
		/// <param name="pattern">The pattern to search.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="count">The max number of search results to be returned.</param>
		/// <returns>A set of iFolder Server objects.</returns>
		[WebMethod(
			 Description="Get information about iFolder Servers identified by a search on name.",
			 EnableSession=true)]
		public virtual iFolderServerSet GetServersByName(SearchOperation operation, string pattern,
			int index, int count)
		{
			iFolderServerSet result = null;

			try
			{
				Authorize();
				if( GetAccessIDForGroup() != null)
				{
					/// throw access not allowed exception...
					return result;
				}

				result = iFolderServer.GetServersByName(iFolderServerType.All, operation, pattern, index, count);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		#endregion

		#region iFolders

		/// <summary>
		/// Delete an iFolder
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder to be deleted.</param>
		/// <remarks>This API will accept multiple iFolder ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Delete an iFolder",
			 EnableSession=true)]
		public virtual void DeleteiFolder(string ifolderID)
		{
			Hashtable exceptions = new Hashtable();

			try
			{
				Authorize();

				string accessID = GetAccessID();

				string[] ids = ifolderID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolder.DeleteiFolder(id, accessID);
						}
						catch(Exception e)
						{
							exceptions.Add(id, e);
						}
					}
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			SmartException.Throw(exceptions);
		}

		/// <summary>
		/// Get information about an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <returns>An iFolder object describing the iFolder.</returns>
		[WebMethod(
			 Description="Get information about an iFolder.",
			 EnableSession=true)]
		public virtual iFolder GetiFolder(string ifolderID)
		{
			iFolder result = null;
			try
			{
				Authorize();
				result = iFolder.GetiFolder(ifolderID, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
		
		/// <summary>
		/// Get detailed information about an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder</param>
		/// <returns>An iFolderDetails object describing the iFolder</returns>
		/// <remarks>It is more expensive to call GetiFolderDetails than GetiFolder.</remarks>
		[WebMethod(
			 Description="Get detailed information about an iFolder.",
			 EnableSession=true)]
		public virtual iFolderDetails GetiFolderDetails(string ifolderID)
		{
			iFolderDetails result = null;

			try
			{
				Authorize();

				result = iFolderDetails.GetiFolderDetails(ifolderID, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}
		
		/// <summary>
		/// Set the description of an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="description">The new description for the iFolder.</param>
		[WebMethod(
			 Description="Set the description of an iFolder.",
			 EnableSession=true)]
		public virtual void SetiFolderDescription(string ifolderID, string description)
		{
			try
			{
				Authorize();

				iFolder.SetDescription(ifolderID, description, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Publish an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="publish">The published state of the iFolder</param>
		[WebMethod(
			 Description="Publish an iFolder.",
			 EnableSession=true)]
		public virtual void PublishiFolder(string ifolderID, bool publish)
		{
			try
			{
				Authorize();

				iFolder.PublishiFolder(ifolderID, publish, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		#endregion

		#region Changes
		
		/// <summary>
		/// Get a history of changes to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="itemID">The id of item to filter the results (can be null for no filtering).</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of ChangeEntry objects.</returns>
		[WebMethod(
			 Description="Get a history of changes to an iFolder.",
			 EnableSession=true)]
		public virtual ChangeEntrySet GetChanges(string ifolderID, string itemID, int index, int max)
		{
			ChangeEntrySet result = null;

			try
			{
				Authorize();

				result = ChangeEntry.GetChanges(ifolderID, itemID, index, max, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// To extract the aspx page from header
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[WebMethod(
			 Description="Get a trimmed url from full url",
			 EnableSession=true)]
		public virtual string TrimUrl(string FullUrl)
		{
			string TrimmedUrl = "iFolders.aspx";
			try
			{
				if(FullUrl != "")
				{
					 TrimmedUrl = FullUrl.Substring(FullUrl.LastIndexOf('/') + 1);
					 
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return TrimmedUrl;
		}
		
		#endregion

		#region User

		/// <summary>
		/// Get information about the authenticated user.
		/// </summary>
		/// <returns>An iFolderUser object describing the authenticated user.</returns>
		[WebMethod(
			 Description="Get information about the authenticated user.",
			 EnableSession=true)]
		public virtual iFolderUser GetAuthenticatedUser()
		{
			iFolderUser result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUser(null, GetUserID(), GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Set the rights of a member on an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="userID">The user id of the member.</param>
		/// <param name="rights">The rights to be set.</param>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Set the rights of a member on an iFolder.",
			 EnableSession=true)]
		public virtual void SetMemberRights(string ifolderID, string userID, Rights rights)
		{
			Hashtable exceptions = new Hashtable();

			try
			{
				Authorize();

				string[] ids = userID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolderUser.SetMemberRights(ifolderID, id, rights, GetAccessID());
						}
						catch(Exception e)
						{
							exceptions.Add(id, e);
						}
					}
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			SmartException.Throw(exceptions);
		}

		/// <summary>
		/// Add a member to an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of iFolder.</param>
		/// <param name="userID">The user id of the new member.</param>
		/// <param name="rights">The rights of the new member on the iFolder.</param>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Add a member to an iFolder.",
			 EnableSession=true)]
		public virtual void AddMember(string ifolderID, string userID, Rights rights)
		{   
			Hashtable exceptions = new Hashtable();
			
			try
			{
				Authorize();

				string[] ids = userID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolderUser.AddMember(ifolderID, id, rights, GetAccessID());
						}
						catch(Exception e)
						{
							// save any exceptions
							exceptions.Add(id, e);
						}
					}
				}
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			SmartException.Throw(exceptions);
		}

		/// <summary>
		/// Remove a member from an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="userID">The user id of the member.</param>
		/// <remarks>This API will accept multiple user ids in a comma delimited list.</remarks>
		[WebMethod(
			 Description="Remove a member from an iFolder.",
			 EnableSession=true)]
		public virtual void RemoveMember(string ifolderID, string userID)
		{
			Hashtable exceptions = new Hashtable();

			try
			{
				Authorize();

				string[] ids = userID.Split(new char[] {',', ' '});

				foreach(string id in ids)
				{
					if (id.Length > 0)
					{
						try
						{
							iFolderUser.RemoveMember(ifolderID, id, GetAccessID());
						}
						catch(Exception e)
						{
							exceptions.Add(id, e);
						}
					}
				}

			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
	
			SmartException.Throw(exceptions);
		}

		/// <summary>
		/// Get all groupids this member belongs to .
		/// </summary>
		/// <param name="userID">The user id of the member.</param>
		[WebMethod(
			 Description="Get group ids this member belongs to ",
			 EnableSession=true)]
		public virtual string [] GetGroupIDs(string userID)
		{
			string [] IDs = null;
			try
			{
				Authorize();
				IDs = iFolderUser.GetGroupIDs( userID );

			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
			return IDs;
		}

		/// <summary>
		/// Set the owner of an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="userID">The user id of the new owner.</param>
		[WebMethod(
			 Description="Set the owner of an iFolder.",
			 EnableSession=true)]
		public virtual void SetiFolderOwner(string ifolderID, string userID, bool OrphanAdopt)
		{
			try
			{
				Authorize();

				iFolderUser.SetOwner(ifolderID, userID, GetAccessID(), OrphanAdopt);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}
		}

		/// <summary>
		/// Get information about the members of an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects describing the members.</returns>
		[WebMethod(
			 Description="Get information about the members of an iFolder.",
			 EnableSession=true)]
		public virtual iFolderUserSet GetMembers(string ifolderID, int index, int max)
		{
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUsers(ifolderID, index, max, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about all of the iFolder users.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users.",
			 EnableSession=true)]
		public virtual iFolderUserSet GetUsers(int index, int max)
		{
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUsers(null, index, max, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about a user using an id or username.
		/// </summary>
		/// <param name="userID">The id or username of the user.</param>
		/// <returns>A iFolderUser object.</returns>
		[WebMethod(
			 Description="Get information about a user using an id or username.",
			 EnableSession=true)]
		public virtual iFolderUser GetUser(string userID)
		{
			iFolderUser result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUser(userID, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Provision Users to servers.
		/// </summary>
		/// <param name="ServerNames">A string array containing server names</param>
		/// <param name="UserIDs">A string array containing corresponding userids</param>
		/// <returns>No return value</returns>
		[WebMethod(
			 Description="Provision different users to different servers",
			 EnableSession=true)]
		public virtual void ProvisionUsersToServers(string [] ServerNames, string [] UserIDs)
		{

			try
			{
				Authorize();

				iFolderUser.ProvisionUsersToServers(ServerNames, UserIDs);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

		}

		/// <summary>
		/// Provision Users to a server.
		/// </summary>
		/// <param name="ServerName">A string containing server's name. </param>
		/// <param name="ListOfUsers">An array of string containing userIDs </param>
		/// <returns>No return value</returns>
		[WebMethod(
			 Description="Provision different users to one server",
			 EnableSession=true)]
		public virtual void ProvisionUsersToServer(string ServerName, string [] ListOfUsers)
		{

			try
			{
				Authorize();

				iFolderUser.ProvisionUsersToServer(ServerName, ListOfUsers);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

		}

		/// <summary>
		/// ReProvision Users to a server.
		/// </summary>
		/// <param name="ServerName">A string containing server's name. </param>
		/// <param name="UserID">string containing userID </param>
		/// <returns>No return value</returns>
		[WebMethod(
			 Description="ReProvision users to one server",
			 EnableSession=true)]
		public virtual void ReProvisionUsersToServer(string ServerName, string UserID)
		{

			try
			{
				Authorize();

				iFolderUser.ReProvisionUsersToServer(ServerName, UserID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

		}

		/// <summary>
		/// Get detailed information about a user.
		/// </summary>
		/// <param name="userID">The id of the user.</param>
		/// <returns>A iFolderUserDetails object.</returns>
		/// <remarks>It is more expensive to call GetUserDetails than GetUser.</remarks>
		[WebMethod(
			 Description="Get User Details",
			 EnableSession=true)]
		public virtual iFolderUserDetails GetUserDetails(string userID)
		{
			iFolderUserDetails result = null;

			try
			{
				Authorize();

				result = iFolderUserDetails.GetDetails(userID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about all of the iFolder users identified by the search property, operation, and pattern.
		/// </summary>
		/// <param name="property">The property to search.</param>
		/// <param name="operation">The operation to compare the property and pattern.</param>
		/// <param name="pattern">The pattern to search</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users identified by the search property, operation, and pattern.",
			 EnableSession=true)]
		public virtual iFolderUserSet GetUsersBySearch(SearchProperty property, SearchOperation operation, string pattern, int index, int max)
		{
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetUsers(property, operation, pattern, index, max, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about all of the iFolder users identified by the search property, operation, and pattern.
		/// </summary>
		/// <param name="property">The property to search.</param>
		/// <param name="operation">The operation to compare the property and pattern.</param>
		/// <param name="pattern">The pattern to search</param>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <param name="SecondaryAdminID">The max number of search results to be returned.</param>
		/// <param name="GetMonitoredGroups">The max number of search results to be returned.</param>
		/// <param name="adminrequest">whether request is coming from web-admin/web-access</param>
		/// <returns>A set of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users identified by the search property, operation, and pattern.",
			 EnableSession=true)]
		public virtual iFolderUserSet GetMonitoredGroupsBySearch(SearchProperty property, SearchOperation operation, string pattern, int index, int max, string SecondaryAdminID, bool GetMonitoredGroups, bool adminrequest)
		{
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetMonitoredGroupsSet(property, operation, pattern, index, max, GetAccessID(), SecondaryAdminID, GetMonitoredGroups, adminrequest);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get information about all of the iFolder users with DATA move property set.
		/// </summary>
		/// <param name="index">The starting index for the search results.</param>
		/// <param name="max">The max number of search results to be returned.</param>
		/// <returns>A set of iFolderUser objects.</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users with DATA move property set.",
			 EnableSession=true)]
		public virtual iFolderUserSet GetReprovisionUsers(int index, int max)
		{
			iFolderUserSet result = null;

			try
			{
				Authorize();

				result = iFolderUser.GetReprovisionUsers(index, max, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get agg disk quota associated with user.
		/// </summary>
		/// <param name="GroupID">Group id </param>
		/// <returns>value of the aggregate disk quota, -1 if no quota set on group</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users with DATA move property set.",
			 EnableSession=true)]
		public virtual long GetAggregateDiskQuota(string GroupID)
		{
			long result = -1;

			try
			{
				Authorize();

				result = iFolderUser.GetAggregateDiskQuota(GroupID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		/// <summary>
		/// Get the sum of space used by all members of group.
		/// </summary>
		/// <param name="GroupID">id of group</param>
		/// <returns>sum of space used by all group members</returns>
		[WebMethod(
			 Description="Get information about all of the iFolder users with DATA move property set.",
			 EnableSession=true)]
		public virtual long SpaceUsedByGroup(string GroupID)
		{
			long result = 0;

			try
			{
				Authorize();

				result = iFolderUser.SpaceUsedByGroup(GroupID);
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
		}

		#endregion

		#region Utility

		/// <summary>
		/// Get the access user's id.
		/// </summary>
		protected abstract string GetAccessID();

		protected abstract string GetAccessIDForGroup();

		/// <summary>
		/// Get the authenticated user's id.
		/// </summary>
		protected abstract string GetUserID();

		/// <summary>
		/// Authorize the authenticated user.
		/// </summary>
		protected abstract void Authorize();
		/// <summary>
		/// Authorize the authenticated user.
		/// </summary>
		protected abstract bool IsAccessAllowed(string id);

		#endregion
	}
}
