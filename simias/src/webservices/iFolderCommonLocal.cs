/***********************************************************************
 *  $RCSfile: iFolderAdmin.asmx.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.Web;
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
		///Reset passphrase and recovery agent
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Reset passphrase and recovery agent.")]
		[SoapDocumentMethod]
		public virtual bool ReSetPassPhrase(string oldPass, string newPass, string recoveryAgentName, string publicKey)
		{
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(store.DefaultDomain);
			bool result = false;
		    try
			{
				Authorize();

				result = iFolderUser.ReSetPassPhrase(domain.ID,  oldPass, newPass, recoveryAgentName, publicKey, GetAccessID());
			}
			catch(Exception e)
			{
				SmartException.Throw(e);
			}

			return result;
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


		/// <summary>
		/// Get the home server for the user
		/// </summary>
		/// <returns>Publiv url of the home server.</returns>
		[WebMethod(
			 Description="Get the home server for the user.",
			 EnableSession=true)]
		public virtual string GetHomeServerForUser( string username , string password)
		{
		        string result;

			result = iFolderServer.GetHomeServerForUser ( username, password );

			return result;
		}

		/// <summary>
		/// 
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
		/// Set the owner of an iFolder.
		/// </summary>
		/// <param name="ifolderID">The id of the iFolder.</param>
		/// <param name="userID">The user id of the new owner.</param>
		[WebMethod(
			 Description="Set the owner of an iFolder.",
			 EnableSession=true)]
		public virtual void SetiFolderOwner(string ifolderID, string userID)
		{
			try
			{
				Authorize();

				iFolderUser.SetOwner(ifolderID, userID, GetAccessID());
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

		#endregion

		#region Utility

		/// <summary>
		/// Get the access user's id.
		/// </summary>
		protected abstract string GetAccessID();

		/// <summary>
		/// Get the authenticated user's id.
		/// </summary>
		protected abstract string GetUserID();

		/// <summary>
		/// Authorize the authenticated user.
		/// </summary>
		protected abstract void Authorize();

		#endregion
	}
}
