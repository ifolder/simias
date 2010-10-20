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
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/


using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography;

//using Novell.Security.ClientPasswordManager;

using Simias;
using Simias.Authentication;
using Simias.Client;
using Simias.DomainServices;
using Simias.Storage;
using Simias.CryptoKey;
using Simias.Sync;
using Simias.Security.Web.AuthenticationService;
//using Simias.POBox;

namespace Simias.Web
{

        /// <summary>
        /// Class used to keep track of outstanding searches.
        /// </summary>
        internal class SearchState : IDisposable
        {
                #region Class Members
                /// <summary>
                /// Table used to keep track of outstanding search entries.
                /// </summary>
                static private Hashtable searchTable = new Hashtable();
 
                /// <summary>
                /// Indicates whether the object has been disposed.
                /// </summary>
                private bool disposed = false;
 
                /// <summary>
                /// Handle used to store and recall this context object.
                /// </summary>
                private string contextHandle = Guid.NewGuid().ToString();
 
                /// <summary>
                /// Identifier for the domain that is being searched.
                /// </summary>
                private string domainID;
 
                /// <summary>
                /// Object used to iteratively return the members from the domain.
                /// </summary>
                private ICSEnumerator enumerator;
 
                /// <summary>
                /// Total number of records contained in the search.
                /// </summary>
                private int totalRecords;
 
                /// <summary>
                /// The cursor for the caller.
                /// </summary>
                private int currentRecord = 0;
 
                /// <summary>
                /// The last count of records returned.
                /// </summary>
                private int previousCount = 0;
                #endregion
                #region Properties
                /// <summary>
                /// Indicates if the object has been disposed.
                /// </summary>
                public bool IsDisposed
                {
                        get { return disposed; }
                }
 
                /// <summary>
                /// Gets the context handle for this object.
                /// </summary>
                public string ContextHandle
                {
                        get { return contextHandle; }
                }
 
                /// <summary>
                /// Gets or sets the current record.
                /// </summary>
                public int CurrentRecord
                {
                        get { return currentRecord; }
                        set { currentRecord = value; }
                }
 
                /// <summary>
                /// Gets the domain ID for the domain that is being searched.
                /// </summary>
                public string DomainID
                {
                        get { return domainID; }
                }
 
                /// <summary>
                /// Gets or sets the last record count.
                /// </summary>
                public int LastCount
                {
                        get { return previousCount; }
                        set { previousCount = value; }
                }
 
                /// <summary>
                /// Gets the search iterator.
                /// </summary>
                public ICSEnumerator Enumerator
                {
                        get { return enumerator; }
                }
 
                /// <summary>
                /// Gets the total number of records contained by this search.
                /// </summary>
                public int TotalRecords
                {
                        get { return totalRecords; }
                }
                #endregion
 
                #region Constructor
                /// <summary>
                /// Initializes an instance of an object.
                /// </summary>
                /// <param name="domainID">Identifier for the domain that is being searched.</param>
                /// <param name="enumerator">Search iterator.</param>
                /// <param name="totalRecords">The total number of records contained in the search.</param>
                public SearchState( string domainID, ICSEnumerator enumerator, int totalRecords )
                {
                        this.domainID = domainID;
                        this.enumerator = enumerator;
                        this.totalRecords = totalRecords;
 
                        lock ( searchTable )
                        {
                                searchTable.Add( contextHandle, this );
                        }
                }
                #endregion
 
                #region Private Methods
                /// <summary>
                /// Removes this SearchState object from the search table.
                /// </summary>
                private void RemoveSearchState()
                {
                        lock ( searchTable )
                        {
                                // Remove the search context from the table and dispose it.
                                searchTable.Remove( contextHandle );
                        }
                }
                #endregion
 
                #region Public Methods
                /// <summary>
                /// Returns a search context object that contains the state information for an outstanding search.
                /// </summary>
                /// <param name="contextHandle">Context handle that refers to a specific search context object.</param>
                /// <returns>A SearchState object if a valid one exists, otherwise a null is returned.</returns>
                static public SearchState GetSearchState( string contextHandle )
                {
                        lock ( searchTable )
                        {
                                return searchTable[ contextHandle ] as SearchState;
                        }
                }
                #endregion
 
                #region IDisposable Members
                /// <summary>
                /// Allows for quick release of managed and unmanaged resources.
                /// Called by applications.
                /// </summary>
                public void Dispose()
                {
                        RemoveSearchState();
                        Dispose( true );
                        GC.SuppressFinalize( this );
                }
 
                /// <summary>
                /// Dispose( bool disposing ) executes in two distinct scenarios.
                /// If disposing equals true, the method has been called directly
                /// or indirectly by a user's code. Managed and unmanaged resources
                /// can be disposed.
                /// If disposing equals false, the method has been called by the
                /// runtime from inside the finalizer and you should not reference
                /// other objects. Only unmanaged resources can be disposed.
                /// </summary>
                /// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
                private void Dispose( bool disposing )
                {
                        // Check to see if Dispose has already been called.
                        if ( !disposed )
                        {
                                // Protect callers from accessing the freed members.
                                disposed = true;
 
                                // If disposing equals true, dispose all managed and unmanaged resources.
                                if ( disposing )
                                {
                                        // Dispose managed resources.
                                        enumerator.Dispose();
                                }
                        }
                }
 
                /// <summary>
                /// Use C# destructor syntax for finalization code.
                /// This destructor will run only if the Dispose method does not get called.
                /// It gives your base class the opportunity to finalize.
                /// Do not provide destructors in types derived from this class.
                /// </summary>
                ~SearchState()
                {
                        Dispose( false );
                }
                #endregion
        }


	/// <summary>
	/// Supported store search operators.
	/// </summary>
	public enum SearchType
	{
		/// <summary>
		/// Used to compare if two values are equal.
		/// </summary>
		Equal,

		/// <summary>
		/// Used to compare if two values are not equal.
		/// </summary>
		Not_Equal,

		/// <summary>
		/// Used to compare if a string value begins with a sub-string value.
		/// </summary>
		Begins,

		/// <summary>
		/// Used to compare if a string value ends with a sub-string value.
		/// </summary>
		Ends,

		/// <summary>
		/// Used to compare if a string value contains a sub-string value.
		/// </summary>
		Contains,

		/// <summary>
		/// Used to compare if a value is greater than another value.
		/// </summary>
		Greater,

		/// <summary>
		/// Used to compare if a value is less than another value.
		/// </summary>
		Less,

		/// <summary>
		/// Used to compare if a value is greater than or equal to another value.
		/// </summary>
		Greater_Equal,

		/// <summary>
		/// Used to compare if a value is less than or equal to another value.
		/// </summary>
		Less_Equal,

		/// <summary>
		/// Used to test for existence of a property.
		/// </summary>
		Exists,

		/// <summary>
		/// Used to do a case sensitive compare.
		/// </summary>
		CaseEqual
	};

	/// <summary>
	/// Class that represents a member that has rights to a collection.
	/// </summary>
	[ Serializable ]
	public class MemberInfo
	{
		/// <summary>
		/// </summary>
		public string	ObjectID;

		/// <summary>
		/// The user identitifer for this object.
		/// </summary>
		public string 	UserID;

		/// <summary>
		/// </summary>
		public string	Name;

		/// <summary>
		/// the member's given (first) name
		/// or null if the property is not present.
		/// </summary>
		public string	GivenName;

		/// <summary>
		/// The member's family (last) name
		/// or null if the property is not present.
		/// </summary>
		public string	FamilyName;

		/// <summary>
		/// The member's full name
		/// or null if the property is not present.
		/// </summary>
		public string	FullName;

		/// <summary>
		/// The members's access rights.
		/// </summary>
		public int	AccessRights;

		/// <summary>
		/// Whether this Member object is the collection owner.
		/// </summary>
		public bool	IsOwner;


		/// <summary>
		/// Whether this Member object is a user object.
		/// </summary>
		public bool	IsHost;

 		/// <summary>
		/// Property tag for User object.
 		/// </summary>
	        static private readonly string HostTag = "Host";

		/// <summary>
		/// </summary>
		public MemberInfo()
		{
		}

		//[ NonSerializable ]
		internal MemberInfo( Simias.Storage.Member member )
		{
			this.Name = member.Name;
			this.ObjectID = member.ID;
			this.UserID = member.UserID;
			this.GivenName = member.Given;
			this.FamilyName = member.Family;
			this.FullName = member.FN;
			this.AccessRights = (int) member.Rights;
			this.IsOwner = member.IsOwner;
			this.IsHost = member.IsType (HostTag);
		}
	}

        [ Serializable ]
        public class NodeEntrySet
        {
                public NodeEntry[] Items;
                public long Count;
 
                public NodeEntrySet()
                {
                }
 
                public NodeEntrySet(NodeEntry[] list, long count)
                {
                        this.Items = list;
                        this.Count = count;
                }
        }

        /// <summary>
        /// Class that represents a member that has rights to a collection.
        /// </summary>
        [ Serializable ]
        public class NodeEntry
        {
                public string ID;
                public string Name;
                public long Length;
                public string Type;
                public string RelativePath;
 
                public NodeEntry()
                {
                }
 
                public NodeEntry( Node n)
                {
                        try
                        {
                                this.ID = n.ID;
                                this.Type = n.Type;
                                this.Name = n.Name;
                                if( this.Type == "FileNode")
                                {
                                        this.Length = ((FileNode)n).Length;
                                        this.RelativePath = ((FileNode)n).GetRelativePath();
                                }
                                else if( this.Type == "DirNode")
                                {
                                        this.Length = 0;
                                        this.RelativePath = ((DirNode)n).GetRelativePath();
                                }
                        }
                        catch(Exception e1)
                        {
                        }
                }
        }



	/// <summary>
	/// This is the core of the SimiasServce.  All of the methods in the
	/// web service are implemented here.
	/// </summary>
	[WebService(
	Namespace="http://novell.com/simias/web/",
	Name="Simias Web Service",
	Description="Web Service providing access to Simias")]
	public class SimiasService : WebService
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SimiasService));
		private static int simiasReferenceCount = 0;

		private static string DomainServiceType = "Domain Service";
		private static string DomainServicePath = "/simias10/DomainService.asmx";
                private static string XmlFileRestore = "";
                private static string BasePathRestore = "";
                private static Thread RestoreThread = null;


		/// <summary>
		/// Creates the SimiasService and sets up logging
		/// </summary>
		public SimiasService()
		{
		}

		/// <summary>
		/// WebMethod that allows a client to ping the service to see
		/// if it is up and running
		/// </summary>
		[WebMethod(EnableSession=true, Description="Allows a client to ping to make sure the Web Service is up and running")]
		[SoapDocumentMethod]
		public void PingSimias()
		{
			// Nothing to do here, just return
		}

		/// <summary>
		/// Add a member to a domain.
		/// </summary>
		/// <param name="DomainID">The ID of the domain to add the member to.</param>
		/// <param name="MemberName">The name of the member.</param>
		/// <param name="MemberID">The ID of the member.</param>
		/// <param name="PublicKey">The public key for the member.</param>
		/// <param name="GivenName">The given name for the member.</param>
		/// <param name="FamilyName">The family name for the member.</param>
		[WebMethod(EnableSession=true, Description="Add a member to the domain.")]
		[SoapDocumentMethod]
		public void AddMemberToDomain(string DomainID, string MemberName, string MemberID, string PublicKey, string GivenName, string FamilyName)
		{
			try
			{
				Domain domain = Store.GetStore().GetDomain( DomainID );
				Simias.Storage.Member member = domain.GetMemberByName( MemberName );
				if ( member == null )
				{
					bool given;
					member = new Simias.Storage.Member( MemberName, MemberID, Access.Rights.ReadOnly );

					if ( PublicKey != null )
					{
						member.Properties.AddProperty( "PublicKey", PublicKey );
					}

					if ( GivenName != null && GivenName != "" )
					{
						member.Given = GivenName;
						given = true;
					}
					else
					{
						given = false;
					}

					if ( FamilyName != null && FamilyName != "" )
					{
						member.Family = FamilyName;
						if ( given == true )
						{
							member.FN = GivenName + " " + FamilyName;
						}
					}
					log.Debug("SetPassPhrase - called");

					domain.Commit( member );
				}
			}
			catch{}
		}

		/// <summary>
		/// Remove a member from a domain
		/// </summary>
		/// <param name="DomainID">The ID of the domain to remove the member from.</param>
		/// <param name="MemberID">The ID of the member to remove.</param>
		[WebMethod(EnableSession=true, Description="Remove a member from the domain.")]
		[SoapDocumentMethod]
		public void RemoveMemberFromDomain(string DomainID, string MemberID)
		{
			Domain domain = Store.GetStore().GetDomain(DomainID);
			Simias.Storage.Member member = domain.GetMemberByID( MemberID );
			if ( member != null )
			{
				domain.Commit( domain.Delete( member ) );
			}
		}



		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain.</param>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstMembers
		/// or FindFirstSpecificMembers methods.</param>
		[WebMethod(EnableSession=true, Description="End the search for domain members.")]
		[SoapDocumentMethod]
		public void FindCloseMembers( string domainID, string searchContext )
		{
			DomainProvider.FindCloseDomainMembers( domainID, searchContext );
		}



		/// <summary>
		/// Starts a search for all domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="totalMembers">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Starts a search for all domain members.")]
		[SoapDocumentMethod]
		public bool FindFirstMembers( 
			string domainID, 
			int count,
			out string searchContext, 
			out MemberInfo[] memberList, 
			out int totalMembers )
		{
			Member[] tempList;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers(
					domainID, 
					count,
					out searchContext, 
					out tempList, 
					out totalMembers );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				memberList = new MemberInfo[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new MemberInfo( tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Attribute name to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="totalMembers">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Starts a search for a specific set of domain members.")]
		[SoapDocumentMethod]
		public bool FindFirstSpecificMembers(
			string domainID, 
			string attributeName, 
			string searchString, 
			SearchType operation, 
			int count,
			out string searchContext, 
			out MemberInfo[] memberList, 
			out int totalMembers )
		{
			Member[] tempList;

			bool moreEntries = 
				DomainProvider.FindFirstDomainMembers(
					domainID,
					attributeName,
					searchString,
					( Simias.Storage.SearchOp )Enum.ToObject( typeof( Simias.Storage.SearchOp ), operation ),
					count,
					out searchContext, 
					out tempList, 
					out totalMembers );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				memberList = new MemberInfo[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new MemberInfo( tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// Continues the search for domain members from the current record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by 
		/// FindFirstMembers or FindFirstSpecificMembers methods.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Continues the search for domain members from the current record location.")]
		[SoapDocumentMethod]
		public bool FindNextMembers( 
			string domainID, 
			ref string searchContext, 
			int count,
			out MemberInfo[] memberList )
		{
			Member[] tempList;

			bool moreEntries = DomainProvider.FindNextDomainMembers( domainID, ref searchContext, count, out tempList );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				memberList = new MemberInfo[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new MemberInfo( tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// Continues the search for domain members previous to the current record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by 
		/// FindFirstMembers or FindFirstSpecificMembers methods.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Continues the search for domain members previous to the current record location.")]
		[SoapDocumentMethod]
		public bool FindPreviousMembers( 
			string domainID, 
			ref string searchContext, 
			int count,
			out MemberInfo[] memberList )
		{
			Member[] tempList;

			bool moreEntries = DomainProvider.FindPreviousDomainMembers( domainID, ref searchContext, count, out tempList );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				memberList = new MemberInfo[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new MemberInfo( tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}




		/// <summary>
		/// Continues the search for domain members from the specified record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by 
		/// FindFirstMembers or FindFirstSpecificMembers method.</param>
		/// <param name="offset">Record offset to return members from.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Continues the search for domain members from the specified record location.")]
		[SoapDocumentMethod]
		public bool FindSeekMembers( 
			string domainID, 
			ref string searchContext, 
			int offset, 
			int count, 
			out MemberInfo[] memberList )
		{
			Member[] tempList;

			bool moreEntries = DomainProvider.FindSeekDomainMembers( 
				domainID, 
				ref searchContext, 
				offset, 
				count, 
				out tempList );

			if ( ( tempList != null ) && ( tempList.Length > 0 ) )
			{
				memberList = new MemberInfo[ tempList.Length ];
				for ( int i = 0; i < tempList.Length; ++i )
				{
					memberList[ i ] = new MemberInfo( tempList[ i ] );
				}
			}
			else
			{
				memberList = null;
			}

			return moreEntries;
		}



		/// <summary>
		/// WebMethod that returns the Simias information
		/// </summary>
		/// <returns>
		/// string with Simias information
		/// </returns>
		[WebMethod(EnableSession=true, Description="GetSimiasInformation")]
		[SoapDocumentMethod]
		public string GetSimiasInformation()
		{
			return "TODO: Implement the Simias Web Service";
		}



		/// <summary>
		/// WebMethod to get information about a specified domain 
		/// </summary>
		/// <returns>
		/// DomainInformation object
		/// </returns>
		[WebMethod(EnableSession=true, Description="GetDomainInformation")]
		[SoapDocumentMethod]
		public
		DomainInformation
		GetDomainInformation(string domainID)
		{
			DomainInformation cDomainInfo = null;

			try
			{
				cDomainInfo = new DomainInformation(domainID);
			}
			catch(Exception e)
			{
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
				cDomainInfo = null;
			}

			return(cDomainInfo);
		}



		/// <summary>
		/// WebMethod to get a list of local domains
		/// </summary>
		/// <returns>
		/// DomainInformation objects
		/// </returns>
		[WebMethod(EnableSession=true, Description="Get a list of local domains")]
		[SoapDocumentMethod]
		public
		DomainInformation[]
		GetDomains(bool onlySlaves)
		{
			ArrayList domains = new ArrayList();

			try
			{
				Store store = Store.GetStore();
				ICSList domainList = store.GetDomainList();
				foreach( ShallowNode shallowNode in domainList )
				{
					try
					{
						// Get the information about this domain.
						log.Debug("GetDomains: shallownode id is {0}",shallowNode.ID);
						DomainInformation domainInfo = new DomainInformation(shallowNode.ID);
						if ( ( ( onlySlaves == false ) &&
							( domainInfo.Type.Equals( DomainType.Master ) || domainInfo.Type.Equals( DomainType.Slave ) ) ) ||
							( domainInfo.Type.Equals( DomainType.Slave ) ) )
						{
							domains.Add(domainInfo);
						}
					}
					catch(Exception e)
					{
						log.Error(e.Message);
						log.Error(e.StackTrace);
					}
				}
			}
			catch(Exception e)
			{
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			return((DomainInformation[]) domains.ToArray(typeof(DomainInformation)));
		}



		/// <summary>
		/// WebMethod to login or authenticate against a 
		/// remote domain.  The user must have previously joined
		/// or attached to this domain.
		/// </summary>
		/// <returns>
		/// Simias.Client.Authentication.Status status
		/// </returns>
		[WebMethod(EnableSession=true, Description="Login or authenticate to a remote domain")]
		[SoapDocumentMethod]
		public
		Simias.Authentication.Status
		LoginToRemoteDomain(string domainID, string password)
		{ 
			log.Debug( "LoginToRemoteDomain - called" );
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(domainID);
			if( domain == null )
			{
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
			}

			Simias.Storage.Member member = domain.GetCurrentMember();
			if( member == null )
			{
				return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownUser );
			}

			log.Debug( "  User: " + member.Name );

			
			DomainAgent domainAgent = new DomainAgent();
            Simias.Authentication.Status status = domainAgent.Login( domainID, member.Name, password );
            if( status.statusCode == Simias.Authentication.StatusCodes.Success)
            {
                 SyncClient.RescheduleAllColSync(domainID);
            }
            return status;
		}



		/// <summary>
		/// WebMethod to logout from a remote domain.
		/// The user must have previously joined and 
		/// authenticated to this domain.
		/// </summary>
		/// <returns>
		/// Simias.Client.Authentication.Status status
		/// </returns>
		[WebMethod(EnableSession=true, Description="Logout from a remote domain")]
		[SoapDocumentMethod]
		public
		Simias.Authentication.Status
		LogoutFromRemoteDomain(string domainID)
		{
			return new DomainAgent().Logout(domainID);
		}



		/// <summary>
		/// WebMethod to disable automatic authentication to a domain.
		/// </summary>
		/// <param name="domainID">The ID of the domain to disable automatic authentication to.</param>
		[WebMethod(EnableSession=true, Description="Disable automatic authentication to the specified domain")]
		[SoapDocumentMethod]
		public void	DisableDomainAutoLogin(string domainID)
		{
			new DomainAgent().SetDomainState(domainID, false, false);
		}




		/// <summary>
		/// WebMethod to check if a domain is "active"
		/// </summary>
		/// <param name = "domainID">
		/// The specified domain to check
		/// </param>
		/// <returns>
		/// 0 success, !0 failed
		/// </returns>
		[WebMethod(EnableSession=true, Description="WebMethod to check if a domain is active")]
		[SoapDocumentMethod]
		public bool IsDomainActive(string domainID)
		{
			return( new DomainAgent().IsDomainActive( domainID ) );
		}



		/// <summary>
		/// WebMethod to set a slave domain "active"
		/// A Domain marked "active" will synchronize
		/// collections, subscriptions etc. to the remote server
		/// </summary>
		/// <returns>
		/// 0 success, !0 failed
		/// </returns>
		[WebMethod(EnableSession=true, Description="SetDomainActive - enables synchronization to the remote server")]
		[SoapDocumentMethod]
		public int SetDomainActive(string domainID)
		{
			DomainAgent domainAgent = new DomainAgent();
			domainAgent.SetDomainActive( domainID );
			return(0);
		}




		/// <summary>
		/// WebMethod to mark a slave domain "inactive"
		/// Marking a domain inactive disables all synchronization
		/// to the remote machine.
		/// </summary>
		/// <returns>
		/// 0 success, !0 failed
		/// </returns>
		[WebMethod(EnableSession=true, Description="SetDomainInactive - disables remote synchronization")]
		[SoapDocumentMethod]
		public int SetDomainInactive(string domainID)
		{
			DomainAgent domainAgent = new DomainAgent();
			domainAgent.SetDomainInactive( domainID );
			return(0);
		}




		/// <summary>
		/// WebMethod that checks to see if a full set of credentials
		/// has been set on a domain for a specified user
		/// </summary>
		/// <returns>
		/// true - valid credentials for member on the domain, false
		/// </returns>
		[WebMethod(EnableSession=true, Description="ValidCredentials")]
		[SoapDocumentMethod]
		public
		bool ValidCredentials(string domainID, string memberID)
		{
			bool status = false;
			try
			{
				Store store = Store.GetStore();

				// domain
				Domain domain = store.GetDomain(domainID);

				// find user
				Simias.Storage.Member cMember = domain.GetMemberByID( memberID );
				
				BasicCredentials basic =
					new BasicCredentials( domainID, domainID, cMember.Name );
				if ( basic.Cached == true )
				{
					NetworkCredential realCreds = basic.GetNetworkCredential();
					if (realCreds != null)
					{
						status = true;
					}
				}

				/*
				NetCredential cCreds = 
					new NetCredential("iFolder", domainID, true, cMember.Name, null);

				UriBuilder cUri = 
					new UriBuilder(
						this.Context.Request.Url.Scheme,
						this.Context.Request.Url.Host,
						this.Context.Request.Url.Port,
						this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} ));
				*/

			}
			catch(Exception e)
			{
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
			}

			return(status);
		}




		/// <summary>
		/// Sets the domain credentials in the local store.
		/// </summary>
		/// <param name="domainID">The ID of the domain to set the credentials
		/// on.</param>
		/// <param name="credentials">Credentials to set.</param>
		/// <param name="type">Type of credentials.</param>
		[WebMethod(EnableSession=true, Description="Sets domain credentials in the local store")]
		[SoapDocumentMethod]
		public void SetDomainCredentials(	string domainID, 
											string credentials, 
											Simias.Storage.CredentialType type)
		{
			Store store = Store.GetStore();
			store.SetDomainCredentials(domainID, credentials, type);
		}

		                /// Method to set/reset public and private addresses
                /// of a host
                /// Note: The Host parameter can be represented as
                /// the Host ID or the Host name.  If the Host
                /// is null local host is assumed.
                /// </summary>
                /// <param name="Host"></param>
                /// <param name="PublicAddress"></param>
                /// <param name="PrivateAddress"></param>
                /// <returns></returns>
		[WebMethod(EnableSession=true, Description="Sets host's url to local domain")]
		[SoapDocumentMethod]
                public void SetHostAddress( string Host, string PublicUrl, string PrivateUrl, string DomainID )
                {
                        Store store = Store.GetStore();
                        Simias.Storage.Domain domain = store.GetDomain( DomainID );
                        if ( domain == null )
                        {
                                throw new SimiasException( "server domain does not exist." );
                        }

                        // Validate parameters
                        if ( PublicUrl == null && PrivateUrl == null )
                        {
                                throw new SimiasException( "Invalid parameter" );
                        }

                        HostNode host = null;
                        if ( Host == null || Host == String.Empty )
                        {
                                host = HostNode.GetLocalHost();
                        }
                        else
                        {
                                try
                                {
                                        host = HostNode.GetHostByID( domain.ID, Host );
                                }
                                catch{}
                                if ( host == null )
                                {
                                        try
                                        {
                                                host = HostNode.GetHostByName( domain.ID, Host );
                                        }
                                        catch{}
                                }

                                if ( host == null )
                                {
                                        throw new SimiasException( String.Format( "Specified host {0} does not exist", Host ) );
                                }
                        }

                        if ( PrivateUrl != null && PrivateUrl != String.Empty )
                        {
                                host.PrivateUrl = PrivateUrl;
                        }

                        if ( PublicUrl != null && PublicUrl != String.Empty )
                        {
                                host.PublicUrl = PublicUrl;
                        }
                        // Save the changes
                        domain.Commit( host );
                }



                /// <summary>
                /// Stores the passphrase in the local store.
                /// </summary>
                /// <param name="domainID"> The ID of the domain to store the passphrase
                /// on.</param>
                /// <param name="passPhrase">Passphrase to store.</param>
                /// <param name="type">Type of passphrase.</param>
                [WebMethod(EnableSession=true, Description="Stores domain passphrase in the local store")]
                [SoapDocumentMethod]
                public void StorePassPhrase(    string domainID,
                                                                                        string passPhrase,
                                                                                        Simias.Storage.CredentialType type, bool rememberPassPhrase)
                {
                        Store store = Store.GetStore();
                        store.StorePassPhrase(domainID, passPhrase, type, rememberPassPhrase);
                }


		/// <summary>
		/// Gets the credentials from the specified domain object.
		/// </summary>
		/// <param name="domainID">The ID of the domain to set the credentials on.</param>
		/// <param name="userID">Gets the ID of the user.</param>
		/// <param name="credentials">Gets the credentials for the domain.</param>
		/// <returns>The type of credentials.</returns>
		[WebMethod(EnableSession=true, Description="Get the saved credentials from a domain")]
		[SoapDocumentMethod]
		public Simias.Storage.CredentialType GetDomainCredentials(string domainID, out string userID, out string credentials)
		{
			Store store = Store.GetStore();
			return store.GetDomainCredentials(domainID, out userID, out credentials);
		}

               /// <summary>
                /// Gets the credentials from the specified domain object.
                /// </summary>
                /// <param name="domainID">The ID of the domain to set the credentials on.</param>
                /// <param name="userID">Gets the ID of the user.</param>
                /// <param name="passPhrase">Gets the passPhrase for the domain.</param>
                /// <returns>The type of credentials.</returns>
                [WebMethod(EnableSession=true, Description="Get the saved credentials from a domain")]
                [SoapDocumentMethod]
                public string GetPassPhrase(string domainID)
                {
                        Store store = Store.GetStore();
                        return store.GetPassPhrase(domainID);
                }

               /// <summary>
                /// Gets the credentials from the specified domain object.
                /// </summary>
                /// <param name="domainID">The ID of the domain to set the credentials on.</param>
                /// <param name="userID">Gets the ID of the user.</param>
                /// <param name="passPhrase">Gets the passPhrase for the domain.</param>
                /// <returns>The type of credentials.</returns>
                [WebMethod(EnableSession=true, Description="Get the saved credentials from a domain")]
                [SoapDocumentMethod]
                public bool GetRememberOption(string domainID)
                {
                        Store store = Store.GetStore();
                        return store.GetRememberOption(domainID);
                }

		/// <summary>
		/// WebMethod that connects up an iFolder Domain
		/// </summary>
		/// <param name = "UserName">
		/// The username to use to connect to the Domain
		/// </param>
		/// <param name = "Password">
		/// The password to use to connect to the Domain
		/// </param>
		/// <param name = "Host">
		/// The host of the enterprise server
		/// </param>
		/// <returns>
		/// The Domain object associated with this Server
		/// </returns>
		[WebMethod(EnableSession=true, Description="Connects to a Domain")]
		[SoapDocumentMethod]
		public DomainInformation ConnectToDomain(string UserName,
												 string Password,
												 string Host)
		{
			log.Debug("SimiasWebService.ConnectToDomain() called to connect to {0} as {1}", Host, UserName);
			DomainInformation domainInfo = null;			
			DomainAgent da = new DomainAgent();
			// Normalize the host address.
			Simias.Authentication.Status status = da.Attach(Host.ToLower(), UserName, Password);			
			if (status.statusCode == Simias.Authentication.StatusCodes.Success ||
				status.statusCode == Simias.Authentication.StatusCodes.SuccessInGrace)
			{
				domainInfo = new DomainInformation(status.DomainID);
				domainInfo.MemberName = UserName;
				domainInfo.RemainingGraceLogins = status.RemainingGraceLogins;
			}
            else if (status.statusCode == Simias.Authentication.StatusCodes.Timeout)
            {
                log.Debug("Didn't start sync, removing domain");
                domainInfo = new DomainInformation(status.DomainID);
                LeaveDomain(domainInfo.ID, true);
            }
            else
            {
                log.Debug("SimiasWebService.ConnectToDomain() status {0} Host {1}", status.statusCode, status.UserName);
                domainInfo = new DomainInformation();
                domainInfo.HostUrl = status.UserName;
            }
			domainInfo.StatusCode = status.statusCode;

			return domainInfo;
		}




		/// <summary>
		/// WebMethod that removes a domain account from the workstation.
		/// </summary>
		/// <param name = "DomainID">
		/// The ID of the domain that the account belongs to.
		/// </param>
		/// <param name = "LocalOnly">
		/// If true then the account is only removed from this workstation.
		/// If false, then the account will be deleted from every workstation 
		/// that the user owns.
		/// </param>
		[WebMethod(EnableSession=true, Description="Removes a domain account from the workstation")]
		[SoapDocumentMethod]
		public void LeaveDomain(string DomainID,
								bool LocalOnly)
		{
			DomainAgent da = new DomainAgent();
			da.Unattach(DomainID, LocalOnly);
			da.Logout(DomainID);
		}
	


		/// <summary>
		/// WebMethod that removes certificate from CertTable.
		/// </summary>
		/// <param name="host">Host for which certificate has to be removed from Table</param>
		[WebMethod(EnableSession=true, Description="Remove Certificate for this specified host")]
		[SoapDocumentMethod]
		public void RemoveCertFromTable(string host)
		{
			DomainAgent da = new DomainAgent();
			da.RemoveCertFromTable(host);
		}

		/// <summary>
		/// WebMethod that changes the default domain.
		/// </summary>
		/// <param name="domainID">The ID of the domain to set as the default.</param>
		[WebMethod(EnableSession=true, Description="Change the default domain to the specified domain ID")]
		[SoapDocumentMethod]
		public void SetDefaultDomain(string domainID)
		{
			Store store = Store.GetStore();
			store.DefaultDomain = domainID;
		}




		/// <summary>
		/// WebMethod that gets the ID of the default domain.
		/// </summary>
		/// <returns>The ID of the default domain.</returns>
		[WebMethod(EnableSession=true, Description="Get the ID of the default domain")]
		[SoapDocumentMethod]
		public string GetDefaultDomainID()
		{
			Store store = Store.GetStore();
			return store.DefaultDomain;
		}

		/// <summary>
		/// WebMethod to get the certificate for the specified host.
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		[WebMethod(EnableSession=true, Description="Get the certificate of the specified host.")]
		[SoapDocumentMethod]
		public byte[] GetCertificate(string host)
		{
			// Normalize the host address.
			return Simias.Security.CertificateStore.GetCertificate(host.ToLower());
		}

		/// <summary>
		/// WebMethod to get the certificate for the specified host along with problem.
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		[WebMethod(EnableSession=true, Description="Get the certificate of the specified host.")]
		[SoapDocumentMethod]
		public byte[] GetCertificate2(string host, out CertPolicy.CertificateProblem Problem)
		{
			// Normalize the host address.
			return Simias.Security.CertificateStore.GetCertificate(host.ToLower(), out Problem);
		}

		/// <summary>
		/// WebMethod to Store the certificate for the specified host locally.
		/// </summary>
		/// <param name="certificate">The certificate to store.</param>
		/// <param name="host">The host the certificate belongs to.</param>
		[WebMethod(EnableSession=true, Description="Store the certificate for the specified host.")]
		[SoapDocumentMethod]
		public void StoreCertificate(byte[] certificate, string host)
		{
			// Normalize the host address.
			Simias.Security.CertificateStore.StoreCertificate(certificate, host.ToLower(), true);
		}

        /// <summary>
        /// WebMethod to Store the certificate for the specified host in enterprise domain.
        /// </summary>
        /// <param name="certificate">The certificate to store.</param>
        /// <param name="host">The host the certificate belongs to.</param>
        [WebMethod(EnableSession = true, Description = "Store the certificate for the specified host.")]
        [SoapDocumentMethod]
        public void StoreDomainCertificate(byte[] certificate, string host, string domainID)
        {
            // Normalize the host address.
            Simias.Security.CertificateStore.StoreDomainCertificate(certificate, host.ToLower(), domainID, true);
        }

		/// <summary>
		/// WebMethod to get the list of recovery agents.
		/// </summary>
		/// <returns></returns>
		[WebMethod(EnableSession=true, Description="Get the Recovery Agent List.")]
		[SoapDocumentMethod]
		public string[] GetRAList()
		{
		    ArrayList list = Simias.Security.CertificateStore.GetRAList();
		    string[] ralist = new string [ list.Count ];
		    int i=0;

		    foreach (string ra in list)
		    {
			ralist[ i++ ] = ra;
		    }
		    return ralist;
		}

		/// <summary>
		/// WebMethod to get the list of recovery agents.
		/// </summary>
		/// <returns></returns>
		[WebMethod(EnableSession=true, Description="Get the Recovery Agent List.")]
		[SoapDocumentMethod]
		public string[] GetRAListOnClient(string DomainID)
		{
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				string UserID = store.GetUserIDFromDomainID(DomainID);
				Member m = domain.GetMemberByID(UserID);
				HostNode host = m.HomeServer; //home server
				SimiasConnection smConn = new SimiasConnection(DomainID,
										UserID,
										SimiasConnection.AuthType.BASIC,
										host);
				SimiasWebService svc = new SimiasWebService();
				svc.Url = host.PublicUrl;
				smConn.Authenticate ();
				smConn.InitializeWebClient(svc, "Simias.asmx");
				return svc.GetRAList();
			}
			catch(Exception ex)
			{
				//log.Debug("GetRAListOnClient:{0}", ex.message);
				//return null;
				throw ex;
			}
		}


		/// <summary>
		/// WebMethod to get the RA certificate for the specified host.
		/// </summary>
		/// <param name="host"></param>
		/// <returns></returns>
		[WebMethod(EnableSession=true, Description="Get the Recovery Agent certificate of the domain.")]
		[SoapDocumentMethod]
		public byte[] GetRACertificate(string rAgent)
		{
			// Normalize the RA name.
			return Simias.Security.CertificateStore.GetRACertificate(rAgent.ToLower());
		}

		/// <summary>
		/// WebMethod to get the list of recovery agents.
		/// </summary>
		/// <returns></returns>
		[WebMethod(EnableSession=true, Description="Get the Recovery Agent List.")]
		[SoapDocumentMethod]
		public byte[] GetRACertificateOnClient(string DomainID, string rAgent)
		{
			Store store = Store.GetStore();
			Simias.Storage.Domain domain = store.GetDomain(DomainID);
			string UserID = store.GetUserIDFromDomainID(DomainID);
			Member m = domain.GetMemberByID(UserID);
			HostNode host = m.HomeServer; //home server
			SimiasConnection smConn = new SimiasConnection(DomainID,
									UserID,
									SimiasConnection.AuthType.BASIC,
									host);
			SimiasWebService svc = new SimiasWebService();
			svc.Url = host.PublicUrl;
			smConn.Authenticate ();
			smConn.InitializeWebClient(svc, "Simias.asmx");
			return svc.GetRACertificate(rAgent);
		}

		/// <summary>
	       /// Gets the public key for the specified domain object.
	       /// </summary>
	       /// <param name="DomainID">The ID of the domain to set the credentials on.</param>
	       /// <param name="rAgent">Recovery Agent whose public key is needed.</param>
	       /// <returns>The public key of the certificate</returns>
	        [WebMethod(EnableSession=true, Description="Get the public key of the certificate")]
	        [SoapDocumentMethod]
	        public string GetPublicKey(string DomainID, string rAgent)
	        {
		    log.Debug("Inside GetPublicKey function");
	            Store store = Store.GetStore();
	            Simias.Storage.Domain domain = store.GetDomain(DomainID);
	            string UserID = store.GetUserIDFromDomainID(DomainID);
	            Member m = domain.GetMemberByID(UserID);
	            HostNode host = m.HomeServer; //home server
	            SimiasConnection smConn = new SimiasConnection(DomainID,
	                                    UserID,
	                                    SimiasConnection.AuthType.BASIC,
	                                    host);
	            SimiasWebService svc = new SimiasWebService();
	            svc.Url = host.PublicUrl;
	            smConn.Authenticate ();
	            smConn.InitializeWebClient(svc, "Simias.asmx"); 
	            byte[] certificate = svc.GetRACertificate(rAgent);
	            System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(certificate);
	            return Convert.ToBase64String(cert.GetPublicKey());
	        }

		/// <summary>
	       /// Gets the public key for the specified domain object.
	       /// </summary>
	       /// <param name="DomainID">The ID of the domain to set the credentials on.</param>
	       /// <param name="rAgent">Recovery Agent whose public key is needed.</param>
	       /// <returns>The public key of the certificate</returns>
	        [WebMethod(EnableSession=true, Description="Get the public key of the certificate")]
	        [SoapDocumentMethod]
	        public string GetDefaultPublicKey(string DomainID)
	        {
		    log.Debug("Inside GetDefaultPublicKey function");
	            Store store = Store.GetStore();
	            Simias.Storage.Domain domain = store.GetDomain(DomainID);
	            string UserID = store.GetUserIDFromDomainID(DomainID);
	            Member m = domain.GetMemberByID(UserID);
	            return m.GetDefaultPublicKey();
	        }

                /// <summary>
               /// Gets the credentials from the specified domain object - to be used only by thick client.
               /// </summary>
               /// <param name="DomainID">The ID of the domain to set the credentials on.</param>
               /// <returns>The key set of the RSA</returns>
                [WebMethod(EnableSession=true, Description="Get the Default RSA key")]
                [SoapDocumentMethod]
                public string GetDefaultRSAFromServer(string DomainID)
                {
                    log.Debug("Inside GetDefaultRSAKeyFromServer function");
                    Store store = Store.GetStore();
                    Simias.Storage.Domain domain = store.GetDomain(DomainID);
                    string UserID = store.GetUserIDFromDomainID(DomainID);
                    Member m = domain.GetMemberByID(UserID);
                    return m.GetDefaultRSAFromServer();
                }

                /// <summary>
               /// Gets the credentials from the specified domain object.
               /// </summary>
               /// <param name="DomainID">The ID of the domain to set the credentials on.</param>
               /// <returns>The key set of the RSA</returns>
                [WebMethod(EnableSession=true, Description="Get the Default RSA key")]
                [SoapDocumentMethod]
                public string GetDefaultRSAKey(string DomainID)
                {
                    log.Debug("Inside GetDefaultRSAKey function");
                    Store store = Store.GetStore();
                    Simias.Storage.Domain domain = store.GetDomain(DomainID);
                    string UserID = store.GetUserIDFromDomainID(DomainID);
                    Member m = domain.GetMemberByID(UserID);
                    return m.GetDefaultRSAKey();
                }


		/// <summary>
		/// WebMethod to Store the RA certificate for the domain.
		/// </summary>
		/// <param name="certificate">The certificate to store.</param>
		/// <param name="host">The host the certificate belongs to.</param>
		[WebMethod(EnableSession=true, Description="Store the certificate for the Domain.")]
		[SoapDocumentMethod]
		public void StoreRACertificate(byte[] certificate, string rAgent)
		{
			// Normalize the RA name.
			Simias.Security.CertificateStore.StoreRACertificate(certificate, rAgent.ToLower(), true);
		}
		
		/// <summary>
		/// Returns the characters which cannot be used for filenames in the
		/// Simias namespace (files and folders that contain any of these
		/// characters cannot be synchronized with iFolder and conflicts will
		/// be generated).
		/// </summary>
		/// <returns>The characters that cannot be used as a file or directory name for synchronizable files.</returns>
		[WebMethod(EnableSession=true, Description="Returns the characters which cannot be used for filenames in the Simias namespace (files and folders that contain any of these characters cannot be synchronized with iFolder and conflicts will be generated).")]
		[SoapDocumentMethod]
		public string GetInvalidSyncFilenameChars()
		{
			return new string(Simias.Sync.SyncFile.InvalidChars);
		}

		/// <summary>
		/// Sets a new server network address for a client.
		/// </summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <param name="hostAddress">The new IP host address for the domain. If the port has
		/// changed, then specify the port by appending a ':' + the port number to the host
		/// address.</param>
		/// <param name="user">The user changing the address.</param>
		/// <param name="password">The password of the user.</param>
		/// <returns>True if new address was set. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Sets a new server network address for a client.")]
		[SoapDocumentMethod]
		public bool SetDomainHostAddress( string domainID, string hostAddress, string user, string password )
		{
			bool addressSet = false;

			try
			{
				// Normalize the host adddress.
				Uri hostLocation = WSInspection.GetServiceUrl( hostAddress, DomainServiceType, user, password );
                if ( hostLocation == null )
                {

					if ( hostAddress.StartsWith( Uri.UriSchemeHttp ) || hostAddress.StartsWith( Uri.UriSchemeHttps ))
					{
                    	UriBuilder newUB = new UriBuilder(new Uri(hostAddress));
                        if (newUB.Uri.ToString().IndexOf("simias10") == -1)
                            hostLocation = new Uri(newUB.Uri.ToString().TrimEnd(new char[] { '/' }) + "/simias10");
                        else
                            hostLocation = new Uri(newUB.Uri.ToString().TrimEnd(new char[] { '/' }));
            			DomainProvider.SetHostLocation(domainID, hostLocation);
						addressSet = true;
					}
					else
					{
						string[] components = hostAddress.ToLower().Split( new char[] { ':' } );
                    	if ( components.Length > 1 )
                        {
                            // Get the current address for this domain.
                            Uri currentAddress = DomainProvider.ResolveLocation( domainID );
	                        if ( currentAddress != null )
        	                {
                	                UriBuilder ub = new UriBuilder( currentAddress );
                        	        ub.Host = components[ 0 ];
                                	ub.Port = Convert.ToInt32( components[ 1 ] );
                                    DomainProvider.SetHostLocation( domainID, ub.Uri );
                                    addressSet = true;
                            }
    	                }
            	        else
                    	{
                            if (hostAddress.IndexOf("simias10") == -1)
                                hostLocation = new Uri(Uri.UriSchemeHttp + Uri.SchemeDelimiter + hostAddress.TrimEnd(new char[] { '/' }) + "/simias10");
                            else
                                hostLocation = new Uri(Uri.UriSchemeHttp + Uri.SchemeDelimiter + hostAddress.TrimEnd(new char[] { '/' }));
                            DomainProvider.SetHostLocation(domainID, hostLocation);
		                    addressSet = true;
                        }
	
					}
				}
				else
				{
					DomainProvider.SetHostLocation(domainID, hostLocation);
					addressSet = true;
				}
			}
			catch ( Exception ex )
			{
				log.Debug( ex, "Cannot set new domain host address." );
			}

			return addressSet;
		}

		/// <summary>
		/// Sets a proxy address for the specified host.
		/// </summary>
		/// <param name="hostUri">String that contains the host address for the Simias server.</param>
		/// <param name="proxyUri">String that contains the proxy address for the host.</param>
		/// <param name="proxyUser">The proxy user name. May be null.</param>
		/// <param name="proxyPassword">The proxy password. May be null.</param>
		/// <returns>True if proxy was set. Otherwise false is returned.</returns>
		[WebMethod(EnableSession=true, Description="Sets a new proxy address for the specified host.")]
		[SoapDocumentMethod]
		public bool SetProxyAddress( string hostUri, string proxyUri, string proxyUser, string proxyPassword )
		{
			bool proxySet = true;

			try
			{
				Uri proxy = ( proxyUri != null ) ? new Uri( proxyUri ) : null;
				ProxyState.AddProxyState( new Uri( hostUri ), proxy, proxyUser, proxyPassword );
			}
			catch
			{
				proxySet = false;
			}

			return proxySet;
		}

		/// <summary>
		/// Checks to see if this instance of Simias is shareable.
		/// </summary>
		/// <param name="simiasDataPath">Application's path to it Simias data area.</param>
		/// <param name="isClient">True if the application wishing to share the service is running as a client.
		/// If it is running as a server, this parameter should be false.</param>
		/// <returns>The directory path for the simias directory.</returns>
		[WebMethod(EnableSession=true, Description="Checks to see if this instance of Simias is shareable.")]
		[SoapDocumentMethod]
		public bool CanShareService( string simiasDataPath, bool isClient )
		{
			bool canShare = false;
			bool ignoreCase = ( MyEnvironment.Platform == MyPlatformID.Windows ) ? true : false;
			Store store = Store.GetStore();

			// The application's simias data path must be the same as this instance's path.
			if ( String.Compare( Path.GetFullPath( simiasDataPath ), Store.StorePath, ignoreCase ) == 0 )
			{
				// Can't share services between clients and enterprise servers because their
				// configurations are different. Not all client services are available on a
				// server.
				if ( isClient && !Store.IsEnterpriseServer )
				{
					canShare = true;
				}
			}

			return canShare;
		}

		/// <summary>
		/// Causes the controlling server process to shutdown the web services and exit.
		/// </summary>
		[WebMethod(EnableSession=true, Description="Shuts down the controlling server process.")]
		[SoapDocumentMethod]
		public void StopSimiasProcess()
		{
			Global.SimiasProcessExit();
		}

		/// <summary>
		/// Increments the reference count that keeps Simias services running.
		/// </summary>
		/// <returns>The new reference count.</returns>
		[WebMethod(EnableSession=true, Description="Increments the reference count that keeps Simias services running.")]
		[SoapDocumentMethod]
		public int AddSimiasReference()
		{
			lock ( typeof( SimiasService ) )
			{
				return ++simiasReferenceCount;
			}
		}

		/// <summary>
		/// Decrements the Simias service reference count and signals the server to stop if the count goes to zero.
		/// </summary>
		/// <returns>The new reference count.</returns>
		[WebMethod(EnableSession=true, Description="Decrements the Simias service reference count and signals the server to stop if the count goes to zero.")]
		[SoapDocumentMethod]
		public int RemoveSimiasReference()
		{
			lock ( typeof( SimiasService ) )
			{
				// Don't let the count go negative.
				if ( simiasReferenceCount >= 1 )
				{
					if ( --simiasReferenceCount == 0 )
					{
						StopSimiasProcess();
					}
				}

				return simiasReferenceCount;
			}
		}

		///<summary>
		///Set the passphrase and recovery agent in the simias client
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Set the passphrase and recovery agent.")]
		[SoapDocumentMethod]
		public bool ServerSetDefaultAccount(string DomainID, string UserID, string iFolderID)
		{
			log.Debug("ServerSetDefaultAccount called");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					throw new SimiasException("Enterprise server domain does not exist.");
				}	
			
				// Member info
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				if(member == null)
				{
					throw new SimiasException("member does not exist.");
				}
				return member.ServerSetDefaultAccount(iFolderID);
			}
			catch(Exception ex)
			{
				log.Debug("SetDefault account: {0}", ex.Message);
				return false;
			}
		}

		///<summary>
		///Set the passphrase and recovery agent in the simias client
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Set the passphrase and recovery agent.")]
		[SoapDocumentMethod]
		public string ServerGetDefaultiFolder(string DomainID, string UserID)
		{
			log.Debug("ServerGetDefaultiFolder called");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					throw new SimiasException("Enterprise server domain does not exist.");
				}	
			
				// Member info
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				if(member == null)
				{
					throw new SimiasException("member does not exist.");
				}
				return member.ServerGetDefaultiFolder();
			}
			catch(Exception ex)
			{
				log.Debug("GetDefault account: {0}", ex.Message);
				return null;
			}
		}

                ///<summary>
                /// gets the GroupsiFolderLimitPolicy for users group
                ///</summary>
                ///<returns>User groups ifolder limit</returns>
                [WebMethod(EnableSession=true, Description="gets the GroupsiFolderLimitPolicy for users group.")]
                [SoapDocumentMethod]
                public int GetGroupsiFolderLimitPolicy(string DomainID, string UserID)
                {
                        int count = -1;
                        log.Debug("GetGroupsiFolderLimitPolicy called");
                        try
                        {
                                Store store = Store.GetStore();
                                Domain domain = store.GetDomain(DomainID);
                                if(domain == null)
                                {
                                        log.Debug("GetGroupsiFolderLimitPolicy Unable to get domain: {0}", DomainID);
                                        return count;
                                }

                                // Member info
                                Simias.Storage.Member member = domain.GetMemberByID(UserID);
                                if(member == null)
                                {
                                        log.Debug("GetGroupsiFolderLimitPolicy Member does not exists: {0}", UserID);
                                        return count;
                                }
                                return member.GroupsiFolderLimit(DomainID, UserID);
                        }
                        catch(Exception ex)
                        {
                                log.Debug("GetGroupsiFolderLimitPolicy: {0}", ex.Message);
                                return count;
                        }
                }

		 ///<summary>
                /// Gets the UseriFolderLimitPolicy for users - transfer of Ownership
                ///</summary>
                ///<returns>User groups ifolder limit</returns>
                [WebMethod(EnableSession=true, Description="gets the UseriFolderLimitPolicy for users - transfer of Ownership.")]
                [SoapDocumentMethod]
                public bool IsTransferAllowed(string DomainID, string UserID)
                {
                        bool result = true;
                        log.Debug("IsTransferAllowed called");
                        try
                        {
                                Store store = Store.GetStore();
                                Domain domain = store.GetDomain(DomainID);
                                if(domain == null)
                                {
                                        log.Debug("IsTransferAllowed Unable to get domain: {0}", DomainID);
                                        return result;
                                }

                                // Member info
                                Simias.Storage.Member member = domain.GetMemberByID(UserID);
                                if(member == null)
                                {
                                        log.Debug("Is Transferred Allowed Member does not exists: {0}", UserID);
                                        return result;
                                }
                                return member.IsTransferAllowed(DomainID, UserID);
                        }
                        catch(Exception ex)
                        {
                                log.Debug("IsTransferredAllowed: {0}", ex.Message);
                                return result;
                        }
                }

                ///<summary>
                /// gets the GroupsSharingPolicy for users group
                ///</summary>
                ///<returns>User groups ifolder limit</returns>
                [WebMethod(EnableSession=true, Description="gets the GroupsiFolderLimitPolicy for users group.")]
                [SoapDocumentMethod]
                public int GetGroupsSharingPolicy(string DomainID, string UserID)
                {
                        int Status = 0;
                        log.Debug("GetGroupsSharingPolicy called");
                        try
                        {
                                Store store = Store.GetStore();
                                Domain domain = store.GetDomain(DomainID);
                                if(domain == null)
                                {
                                        log.Debug("GetGroupsSharingPolicy Unable to get domain: {0}", DomainID);
                                        return Status;
                                }

                                // Member info
                                Simias.Storage.Member member = domain.GetMemberByID(UserID);
                                if(member == null)
                                {
                                        log.Debug("GetGroupsSharingPolicy Member does not exists: {0}", UserID);
                                        return Status;
                                }
                                return member.GroupsSharingPolicy(DomainID, UserID);
                        }
                        catch(Exception ex)
                        {
                                log.Debug("GetGroupsSharingPolicy: {0}", ex.Message);
                                return Status;
                        }
                }

                ///<summary>
                /// gets the GroupsSecurityPolicy for users group
                ///</summary>
                ///<returns>User groups Encryption policy</returns>
                [WebMethod(EnableSession=true, Description="gets the GroupsSecurityPolicy for users group.")]
                [SoapDocumentMethod]
                public int GetGroupsSecurityPolicy(string DomainID, string UserID)
                {
                        int Status = 0;
                        log.Debug("GetGroupsSecurityPolicy called");
                        try
                        {
                                Store store = Store.GetStore();
                                Domain domain = store.GetDomain(DomainID);
                                if(domain == null)
                                {
                                        log.Debug("GetGroupsSecurityPolicy Unable to get domain: {0}", DomainID);
                                        return Status;
                                }

                                // Member info
                                Simias.Storage.Member member = domain.GetMemberByID(UserID);
                                if(member == null)
                                {
                                        log.Debug("GetGroupsSecurityPolicy Member does not exists: {0}", UserID);
                                        return Status;
                                }
                                return member.GroupsSecurityPolicy(DomainID, UserID);
                        }
                        catch(Exception ex)
                        {
                                log.Debug("GroupsSecurityPolicy: {0}", ex.Message);
                                return Status;
                        }
                }

                ///<summary>
                /// gets the GetEffectiveSyncPolicy for users group
                ///</summary>
                ///<returns>User groups Sync policy</returns>
                [WebMethod(EnableSession=true, Description="gets the GroupsSecurityPolicy for users group.")]
                [SoapDocumentMethod]
                public int GetEffectiveSyncPolicy(string DomainID, string UserID, String CollectionID)
                {
                        int Status = 0;
                        log.Debug("GetEffectiveSyncPolicy called");
                        try
                        {
                                Store store = Store.GetStore();
                                Domain domain = store.GetDomain(DomainID);
                                if(domain == null)
                                {
                                        log.Debug("GetEffectiveSyncPolicy Unable to get domain: {0}", DomainID);
                                        return Status;
                                }
                                Simias.Storage.Member member = domain.GetMemberByID(UserID);
                                if(member == null)
                                {
                                        log.Debug("GetEffectiveSyncPolicy Member does not exists: {0}", UserID);
                                        return Status;
                                }
                                Collection col = store.GetCollectionByID(CollectionID);
                                return member.EffectiveSyncPolicy(col);
                        }
                        catch(Exception ex)
                        {
                                log.Debug("GetEffectiveSyncPolicy: {0}", ex.Message);
                                return Status;
                        }
                }

		///<summary>
		///Set the passphrase and recovery agent in the simias client
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Set the passphrase and recovery agent.")]
		[SoapDocumentMethod]
		public Simias.Authentication.Status ServerSetPassPhrase(string DomainID, string UserID, string EncryptedCryptoKey, string CryptoKeyBlob, string RAName, string RAPublicKey)
		{
			log.Debug("ServerSetPassPhrase called");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					throw new SimiasException("Enterprise server domain does not exist.");
				}	
			
				// Member info
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				if(member == null)
				{
					throw new SimiasException("member does not exist.");
				}
				member.ServerSetPassPhrase(EncryptedCryptoKey, CryptoKeyBlob, RAName, RAPublicKey);				
			}
			catch(Exception ex)
			{
				log.Debug("ServerGetEncrypPassKey Exception :{0}", ex.Message);
			}
			return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.Success);	
		}

		///<summary>
		///Validate the passphrase for the correctness (client only)
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="ServerGetEncrypPassKey.")]
		[SoapDocumentMethod]	
		public string ServerGetEncrypPassKey(string DomainID, string UserID)
		{
			log.Debug("ServerGetEncrypPassKey called");
			string PassKey=null;
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					throw new SimiasException("Enterprise server domain does not exist.");
				}
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				
				log.Debug("ServerGetEncrypPassKey called got the member");
				if(member == null)
				{
					log.Debug("member does not exist.");
					throw new SimiasException("member does not exist.");
				}
				PassKey = member.ServerGetEncrypPassKey();
			}
			catch(Exception ex)
			{
				log.Debug("ServerGetEncrypPassKey Exception : {0}", ex.Message);
			}
			return PassKey;
		}

		///<summary>
		///Validate the passphrase for the correctness (client only)
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="ServerGetPassKeyHash.")]
		[SoapDocumentMethod]	
		public string ServerGetPassKeyHash(string DomainID,  string UserID)
		{
			log.Debug("ServerGetPassKeyHash called");
			string KeyHash=null;
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					throw new SimiasException("Enterprise server domain does not exist.");
				}
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				
				log.Debug("ServerGetPassKeyHash called got the member");
				if(member == null)
				{
					log.Debug("member does not exist.");
					throw new SimiasException("member does not exist.");
				}
				KeyHash = member.ServerGetPassKeyHash();		
			}
			catch(Exception ex)
			{
				log.Debug("ServerGetPassKeyHash Exception : {0}", ex.Message);
				log.Debug("ServerGetPassKeyHash StackTrace : {0}", ex.StackTrace);
			}
			return KeyHash;
		}

		///<summary>
		///Updates the user move state property.
		///</summary>
		///<returns></returns>
		[WebMethod(EnableSession=true, Description="Updates the user move state property")]
		[SoapDocumentMethod]	
		public bool UpdateUserMoveState(string DomainID, string UserID, int userMoveStatus)
		{
			log.Debug("UpdateUserMoveState called to set status :"+userMoveStatus);
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					log.Debug("UpdateUserMoveState Domain does not exist.");
					throw new SimiasException("UpdateUserMoveState Enterprise server domain does not exist.");
				}
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				if(member == null)
				{
					log.Debug("UpdateUserMoveState Member does not exist. {0}", UserID);
					throw new SimiasException("UpdateUserMoveState member does not exist.");
				}
				if( userMoveStatus == (int)Member.userMoveStates.Initialized &&  member.UserMoveState != (int)Member.userMoveStates.PreProcessing)
				{
					log.Debug("Initialized state must come only after preprocessing...any other condition..return");
					return true;
				}
				member.UserMoveState = userMoveStatus;
                               	domain.Commit( member );
			}
			catch(Exception ex)
			{
				log.Debug("UpdateUserMoveState Exception : {0}", ex.Message);
				log.Debug("UpdateUserMoveState StackTrace : {0}", ex.StackTrace);

				return false;
			}
		
			return true;
		}

		///<summary>
		///Updates the home server property to new server. 
		///</summary>
		///<returns></returns>
		[WebMethod(EnableSession=true, Description="Update users HomeServer Object")]
		[SoapDocumentMethod]	
		public bool UpdateHomeServer(string DomainID, string UserID, string newHostID)
		{
			log.Debug("UpdateHomeServer called");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					log.Debug("UpdateHomeServer Domain does not exist.");
					throw new SimiasException("UpdateHomeServer Enterprise server domain does not exist.");
				}
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				if(member == null)
				{
					log.Debug("UpdateHomeServer Member does not exist. {0}", UserID);
					throw new SimiasException("UpdateHomeServer member does not exist.");
				}
				member.UserMoveState = (int)Member.userMoveStates.Reprovision;
                               	domain.Commit( member );
				member.HomeServer = new HostNode(domain.GetMemberByID(newHostID));
				member.DeleteProperty = PropertyTags.NewHostID;
				if(member.LoginAlreadyDisabled == false)
                                	domain.SetLoginDisabled(member.UserID, false);
				else
					 member.DeleteProperty = PropertyTags.LoginAlreadyDisabled;
				member.UserMoveState = (int)Member.userMoveStates.MoveCompleted;
                               	domain.Commit( member );
			}
			catch(Exception ex)
			{
				log.Debug("UpdateHomeServer Exception : {0}", ex.Message);
				log.Debug("UpdateHomeServer StackTrace : {0}", ex.StackTrace);
				return false;
			}
		
			return true;
		}

                /// <summary>
                /// Creates an iFolder collection on this location
                /// and puts it for sync...
                /// </summary>
                /// <returns>Does not return anything...</returns>
                [WebMethod(EnableSession=true, Description="Returns the characters which cannot be used for filenames in the Simias namespace (files and folders that contain any of these characters cannot be synchronized with iFolder and conflicts will be generated).")]
                [SoapDocumentMethod]
                public bool DownloadiFolder(string iFolderID, string name, string DomainID, string HostID, string DirNodeID, string MemberNodeID, string colMemberNodeID, string localPath, int sourcefilecount, int sourcedircount )
                {
                        return Collection.DownloadCollection( iFolderID, name, DomainID, HostID, DirNodeID, MemberNodeID, colMemberNodeID, localPath, sourcefilecount, sourcedircount );
		}

		///<summary>
		/// Updates the encryption related local properties.
		///</summary>
		///<returns></returns>
		[WebMethod(EnableSession=true, Description="Updates the encryption related local properties.")]
		[SoapDocumentMethod]	
		public bool UpdateLocalProperties(string DomainID,  string UserID, string EncryptionKey, string EncryptionVersion, string EncryptionBlob, string RAName, string RAPublicKey)
		{
			log.Debug("UpdateLocalProperties called");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					throw new SimiasException("UpdateLocalProperties: Enterprise server domain does not exist.");
				}
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				if(member == null)
				{
					log.Debug("UpdateLocalProperties: member does not exist.");
					throw new SimiasException("UpdateLocalProperties: member does not exist.");
				}
				if(EncryptionKey != null && EncryptionKey != String.Empty)
					member.EncryptionKey = EncryptionKey;
				if(EncryptionVersion != null && EncryptionVersion != String.Empty)
					member.EncryptionVersion = EncryptionVersion; 
				if(EncryptionBlob != null && EncryptionBlob != String.Empty)
					member.EncryptionBlob = EncryptionBlob;
				if(RAName != null && RAName != String.Empty)
					member.RAName = RAName; 
				if(RAPublicKey != null && RAPublicKey != String.Empty)
					member.RAPublicKey = RAPublicKey;
                               	domain.Commit( member );
				member.UpdateSearchContexts(true);
			}
			catch(Exception ex)
			{
				log.Debug("UpdateLocalProperties Exception : {0}", ex.Message);
				return false;
			}
		
			return true;
		}

		///<summary>
		/// Disable the user login, and user Move Property
		///</summary>
		///<returns></returns>
		[WebMethod(EnableSession=true, Description="Disable user object and set user move property")]
		[SoapDocumentMethod]	
		public bool DisableUser(string DomainID,  string UserID, string newHostID)
		{
			log.Debug("DisableUser called");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					throw new SimiasException("Enterprise server domain does not exist.");
				}
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				if(member == null)
				{
					log.Debug("member does not exist.");
					throw new SimiasException("member does not exist.");
				}
				bool preprocessing = false;
				if( member.UserMoveState <= (int)Member.userMoveStates.PreProcessing)
				{
					preprocessing = true;
				}

				member.NewHomeServer = newHostID;
				
				if( ! preprocessing)
				{
					if(domain.IsLoginDisabledForUser(member))
	       	                 	{
						if( member.UserMoveState < (int)Member.userMoveStates.UserDisabled )
						{
							member.LoginAlreadyDisabled = true;	
						}
                	        	}
                        		else
                                		domain.SetLoginDisabled(member.UserID, true);
					member.UserMoveState = (int)Member.userMoveStates.UserDisabled;
				}

                               	domain.Commit( member );
			}
			catch(Exception ex)
			{
				log.Debug("DisableUser Exception : {0}", ex.Message);
				return false;
			}
		
			return true;
		}

		///<summary>
		/// On master, sets a flag to indicate passphrase is set for this user 
		///</summary>
		///<returns></returns>
		[WebMethod(EnableSession=true, Description="SetOnMasterEncryptionBlobFlag")]
		[SoapDocumentMethod]	
		public void SetOnMasterEncryptionBlobFlag(string DomainID,  string UserID)
		{
			log.Debug("SetOnMasterEncryptionBlobFlag called");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					throw new SimiasException("Enterprise server domain does not exist.");
				}
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				
				if(member == null)
				{
					log.Debug("member does not exist.");
					throw new SimiasException("member does not exist.");
				}
				member.SetOnMasterEncryptionBlobFlag(DomainID);		
			}
			catch(Exception ex)
			{
				log.Debug("SetOnMasterEncryptionBlobFlag Exception : {0}", ex.Message);
			}
		
		}

		///<summary>
		/// On master, commit this member object 
		///</summary>
		///<returns></returns>
		[WebMethod(EnableSession=true, Description="CommitDomainMember")]
		[SoapDocumentMethod]	
		public bool CommitDomainMember(string DomainID,  XmlDocument ModifiedMemberXML)
		{
			log.Debug("CommitDomainMember called");
			try
			{
				Store store = Store.GetStore();
				Domain domain = store.GetDomain(DomainID);
				Node ModifiedMemberNode = Node.NodeFactory(store, ModifiedMemberXML);
				Member ModifiedMember = new Member(ModifiedMemberNode);
				if(domain == null)
				{
					throw new SimiasException("Enterprise server domain does not exist.");
				}
				Simias.Storage.Member member = domain.GetMemberByID(ModifiedMember.UserID);
				
				if(member == null)
				{
					log.Debug("member does not exist.");
					throw new SimiasException("member does not exist.");
				}
				member.ModifyMemberProperties(ModifiedMember, DomainID);
			}
			catch(Exception ex)
			{
				log.Debug("CommitDomainMember Exception : {0}", ex.Message);
				return false;
			}
			return true;
		}

		///<summary>
		///Get the ifolder crypto key hash 
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="GetCollectionHashKey.")]
		[SoapDocumentMethod]	
		public  string ServerGetCollectionHashKey(string CollectionID)
		{
			log.Debug("ServerGetCollectionHashKey called");
			string hash = null;
			try
			{
				Store store = Store.GetStore();							
				hash = store.GetCollectionCryptoKeyHash(CollectionID);
			}
			catch(Exception ex)
			{
				log.Debug("ServerGetCollectionHashKey: {0}", ex.Message);
			}
			return hash;
		}

		///<summary>
		///Set the ifolder crypto keys
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="GetiFolderCryptoKeys.")]
		[SoapDocumentMethod]	
		public CollectionKey GetiFolderCryptoKeys(string DomainID,  string UserID, int Index)
		{
		///This must return an array of cryptokeys for the user and not one. This implementation is incorrect.
			log.Debug("GetiFoldersCryptoKeys called");
			CollectionKey cKey=null;
			try
			{
				Store store = Store.GetStore();							
				cKey = store.GetCollectionCryptoKeysByOwner(UserID, DomainID, Index);
			}
			catch(Exception ex)
			{
				log.Debug("GetCollectionCryptoKeysByOwner: {0}", ex.Message);
			}
			return cKey;
		}
		
		///<summary>
		///Set the ifolder crypto keys
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="SetiFolderCryptoKeys.")]
		[SoapDocumentMethod]	
		public  bool SetiFolderCryptoKeys(string DomainID,  string UserID, CollectionKey CKey)
		{
			log.Debug("GetiFoldersCryptoKeys called");
			bool status = false;
			try
			{
				Store store = Store.GetStore();							
				status = store.SetCollectionCryptoKeysByOwner(UserID, DomainID, CKey);
			}
			catch(Exception ex)
			{
				log.Debug("GetiFoldersCryptoKeysByOwner: {0}", ex.Message);
				log.Debug("GetiFoldersCryptoKeysByOwner: {0}", ex.StackTrace);
			}
			return status;
		}

		///<summary>
		///Set the ifolder crypto keys
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="ExportiFoldersCryptoKeys.")]
		[SoapDocumentMethod]	
		public void ExportiFoldersCryptoKeys(string DomainID, string FilePath)
		{
			log.Debug("ExportFolderCryptoKeys - called");
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null )
				{
					log.Debug("ExportFolderCryptoKeys domain null");
					throw new CollectionStoreException("The specified domain not found");
				}
				Simias.Storage.Member member = domain.GetCurrentMember();
				if(member == null )
				{
					log.Debug("ExportFolderCryptoKeys member null");
					throw new CollectionStoreException("The specified domain member not found");
				}
				
				member.ExportiFoldersCryptoKeys(FilePath);
			}
			catch(Exception ex)
			{
				log.Debug("ExportFolderCryptoKeys Exception:{0} ", ex.Message);
				throw ex;
			}
		}

		///<summary>
		///Set the ifolder crypto keys
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="ImportiFoldersCryptoKeys.")]
		[SoapDocumentMethod]	
		public void ImportiFoldersCryptoKeys(string DomainID, string NewPassphrase, string OneTimePassphrase, string FilePath)
		{
			log.Debug("ImportiFoldersCryptoKeys - called");
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null )
				{
					log.Debug("ImportiFoldersCryptoKeys domain null");
					throw new CollectionStoreException("The specified domain not found");
				}
				Simias.Storage.Member member = domain.GetCurrentMember();
				if(member == null )
				{
					log.Debug("ImportiFoldersCryptoKeys member null");
					throw new CollectionStoreException("The specified domain member not found");
				}

				member.ImportiFoldersCryptoKeys(FilePath, NewPassphrase, OneTimePassphrase);
			}
			catch(Exception ex)
			{
				log.Debug("ExportFolderCryptoKeys Exception:{0} ", ex.Message);
				throw ex;
			}
		}

		///<summary>
		///Get the ifolder crypto keys
		///</summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <param name="UserID">User ID for whom the Export is to be performed</param>
		/// <param name="keyDocument">XmlDocument containing the set of iFolders corresponding to the keys.</param>
		///<returns>Void.</returns>
		[WebMethod(EnableSession=true, Description="Exports the iFolders Crypto Keys to a XML Document.")]
		[SoapDocumentMethod]	
		public void ExportiFoldersCryptoKeysToDoc(string DomainID, string UserID, out XmlDocument keyDocument)
		{
			log.Debug("ExportFolderCryptoKeys - called");
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null )
				{
					log.Debug("ExportFolderCryptoKeys domain null");
					throw new CollectionStoreException("The specified domain not found");
				}
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				if(member == null )
				{
					log.Debug("ExportFolderCryptoKeys member null");
					throw new CollectionStoreException("The specified domain member not found");
				}

				member.ExportiFoldersCryptoKeys(out keyDocument, null);
			}
			catch(Exception ex)
			{
				log.Debug("ExportFolderCryptoKeys Exception:{0} ", ex.Message);
				throw ex;
			}
		}

		
		///<summary>
		///Recover the ifolder crypto keys given a RA
		///</summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <param name="UserID">User ID for whom the Recovery is to be performed</param>
		/// <param name="RAName">The RA Name to use for recovery.</param>
		/// <param name="keyDocument">XmlDocument containing the set of iFolders corresponding to the keys.</param>
		/// <param name="oneTimePP">One time password to re-ecnrypt the recovered Keys.</param>
		/// <param name="decryptedKeyDoc">Out parameter containing XmlDocument containing the set of iFolders corresponding to the Recovered keys.</param>
		///<returns>True: If recovery was successful or returns False. The out parameter is allocated only if return is True otherwise it is null</returns>
		[WebMethod(EnableSession=true, Description="Recover the ifolder crypto keys for an user in a Domain given a RA.")]
		[SoapDocumentMethod]	
		public bool RecoverKeys(string DomainID, string UserID, string RAName, XmlDocument keyDocument, string oneTimePP, out XmlDocument decryptedKeyDoc)
		{
			log.Debug("ImportiFoldersCryptoKeys - called");
			bool status = true;
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null )
				{
					log.Debug("ImportiFoldersCryptoKeys domain null");
					throw new CollectionStoreException("The specified domain not found");
				}
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				if(member == null )
				{
					log.Debug("ImportiFoldersCryptoKeys member null");
					throw new CollectionStoreException("The specified domain member not found");
				}
				{
					member.RecoverKeys(RAName, true, keyDocument, oneTimePP, out decryptedKeyDoc);
				}
			}
			catch(Exception ex)
			{
				log.Debug("ExportFolderCryptoKeys Exception:{0} ", ex.Message);
				throw ex;
			}
			return status;
        	}

		///<summary>
		///Set the ifolder crypto keys
		///</summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <param name="UserID">User ID for whom the Import is to be performed</param>
		/// <param name="NewPassphrase">The new passphrase to set.</param>
		/// <param name="OneTimePassword">One time password to use for decrypting the input keys.</param>
		/// <param name="keyDocument">XmlDocument containing the set of iFolders corresponding to the keys.</param>
		///<returns>Void.</returns>
		[WebMethod(EnableSession=true, Description="Imports the iFolder Crypto Keys from an Array")]
		[SoapDocumentMethod]	
		public void ImportiFoldersCryptoKeysFromDoc(string DomainID, string UserID, string NewPassphrase, string OneTimePassword, XmlDocument keyDocument)
		{
			log.Debug("ImportiFoldersCryptoKeys - called");
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null )
				{
					log.Debug("ImportiFoldersCryptoKeys domain null");
					throw new CollectionStoreException("The specified domain not found");
				}
				Simias.Storage.Member member = domain.GetMemberByID(UserID);
				if(member == null )
				{
					log.Debug("ImportiFoldersCryptoKeys member null");
					throw new CollectionStoreException("The specified domain member not found");
				}
				log.Debug("KeyDocument - {0}", keyDocument == null?false:true);
				if(keyDocument != null)
				{
					member.ImportiFoldersCryptoKeys(keyDocument, NewPassphrase, OneTimePassword, false, null);
				}
			}
			catch(Exception ex)
			{
				log.Debug("ExportFolderCryptoKeys Exception:{0} ", ex.Message);
				throw ex;
			}
		}
		
		///<summary>
		///Set the new Passphrase - Enterprise solution with default RA
		///</summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <param name="UserID">User ID for whom the Passphrase Reset is to be performed</param>
		/// <param name="NewPassphrase">The new passphrase to set.</param>
		///<returns>Void</returns>
		[WebMethod(EnableSession=true, Description="Resets the passphrase by Export-Recover-Import automation")]
		[SoapDocumentMethod]	
		public void ExportRecoverImport(string DomainID, string UserID, string NewPassphrase)
		{
			XmlDocument encKeyDocument, decryptedKeyDoc;
			log.Debug("DomainID - {0} \n UserID - {1}", DomainID, UserID);
			log.Debug("NewPassPhrase {0}", NewPassphrase == null?false:true);
			if(DomainID != null && UserID != null && NewPassphrase != null && DomainID != String.Empty && UserID != String.Empty && NewPassphrase != String.Empty)
			{
			/// FIXME - all these 3 operations must be done at the server side instead of getting the data and processing at client
			/// This is a security issue, as we are getting the RSA key pair from Server over wire - might be OK for
			/// trusted environment -but still - BUGBUG (see member.RecoverKeys)
				ExportiFoldersCryptoKeysToDoc(DomainID, UserID, out encKeyDocument);
				RecoverKeys(DomainID, UserID, "DEFAULT", encKeyDocument, null, out decryptedKeyDoc);
				ImportiFoldersCryptoKeysFromDoc(DomainID, UserID, NewPassphrase, null, decryptedKeyDoc);
			}
		}

		
		///<summary>
		///Set the passphrase and recovery agent in the simias client
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Set the passphrase and recovery agent.")]
		[SoapDocumentMethod]
		public Simias.Authentication.Status SetPassPhrase(string DomainID, string PassPhrase, string RAName, string RAPublicKey)
		{
			log.Debug("SetPassPhrase - called");
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null )
				{
					log.Debug("SetPassPhrase domain null");
					return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.UnknownDomain );
				}
				Simias.Storage.Member member = domain.GetCurrentMember();
				if(member == null )
				{
					log.Debug("SetPassPhrase member null");
					return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.UnknownUser );
				}
				
				member.SetPassPhrase(PassPhrase, RAName, RAPublicKey);
			}
			catch(Exception ex)
			{
				log.Debug("SetPassPhrase Exception:{0} ", ex.Message);
                if (ex.Message.IndexOf("Unable to connect") != -1)
                    return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.ServerUnAvailable);
                else
                    throw ex;
				
                
			}
			return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.Success);
		}

		///<summary>
		///Set the passphrase and recovery agent in the simias client
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Set the passphrase and recovery agent.")]
		[SoapDocumentMethod]
		public bool DefaultAccount(string DomainID, string iFolderID)
		{
			log.Debug("Default account called");
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null )
				{
					log.Debug("Default Account domain Null");
					return false;
				}
				Simias.Storage.Member member = domain.GetCurrentMember();
				if(member == null )
				{
					log.Debug("default account member null");
					return false;
				}
				return member.DefaultAccount(iFolderID);
			}
			catch(Exception ex)
			{
				log.Debug("Exception: {0}", ex.Message);
				return false;
			}
		}
		///<summary>
		///Set the passphrase and recovery agent in the simias client
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Set the passphrase and recovery agent.")]
		[SoapDocumentMethod]
		public string GetDefaultiFolder(string DomainID)
		{
			log.Debug("Default account called");
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null )
				{
					log.Debug("Default Account domain Null");
					return null;
				}
				Simias.Storage.Member member = domain.GetCurrentMember();
				if(member == null )
				{
					log.Debug("default account member null");
					return null;
				}
				return member.GetDefaultiFolder();
			}
			catch(Exception ex)
			{
				log.Debug("Exception: {0}", ex.Message);
				return null;
			}
		}
		///<summary>
		///Reset passphrase and recovery agent
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Reset passphrase and recovery agent.")]
		[SoapDocumentMethod]
		public Simias.Authentication.Status ReSetPassPhrase(string DomainID, string OldPassPhrase, string PassPhrase, string RAName, string RAPublicKey)
		{
			log.Debug("ReSetPassPhrase - called");
			bool status = false;

			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null )
				{
					log.Debug("ReSetPassPhrase domain null" );
					return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
				}
				Simias.Storage.Member member = domain.GetCurrentMember();
				if(member == null )
				{
					log.Debug("ReSetPassPhrase member null");
					return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownUser );
				}

				status = member.ReSetPassPhrase(OldPassPhrase, PassPhrase,RAName, RAPublicKey);
			}
			catch(Exception ex)
			{
				log.Debug("ReSetPassPhrase Exception:{0} ", ex.Message);
				throw ex;
			}			
			if(status == true)
				return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.Success);				
			else
				return new Simias.Authentication.Status(Simias.Authentication.StatusCodes.PassPhraseInvalid);	
		}

		///<summary>
		///Validate the passphrase for the correctness (client only)
		///</summary>
		///<returns>passPhrase.</returns>
		[WebMethod(EnableSession=true, Description="Validate the passphrase for the correctness.")]
		[SoapDocumentMethod]	
		public Simias.Authentication.Status ValidatePassPhrase(string DomainID, string PassPhrase)
		{
			Simias.Authentication.Status status = new Simias.Authentication.Status(Simias.Authentication.StatusCodes.PassPhraseInvalid);
			log.Debug("ValidatePassPhrase - called");
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					log.Debug("ValidatePassPhrase domain null");
					return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownDomain );
				}

				Simias.Storage.Member member = domain.GetCurrentMember();
				if(member == null)
				{
					log.Debug("ValidatePassPhrase member null");
					return new Simias.Authentication.Status( Simias.Authentication.StatusCodes.UnknownUser );
				}
				log.Debug("ValidatePassPhrase - calling member");
			
				status = new Simias.Authentication.Status(member.ValidatePassPhrase(PassPhrase));
                if (status.statusCode == Simias.Authentication.StatusCodes.Success)
                {
                    SyncClient.RescheduleAllColSync(DomainID);
                }
			}
			catch(Exception ex)
			{
				log.Debug("ValidatePassPhrase :{0} ", ex.Message);
                if (ex.Message.IndexOf("Unable to connect") != -1)
                    status.statusCode = Simias.Authentication.StatusCodes.ServerUnAvailable;
				//Don't rethrow
			}
			return status;
		}
		
		///<summary>
		///Returns the passphrase state (client only)
		///</summary>
		///<returns></returns>
		[WebMethod(EnableSession=true, Description="Returns the passphrase state.")]
		[SoapDocumentMethod]
		public bool IsPassPhraseSet (string DomainID)
		{
			bool status = false;
			log.Debug("IsPassPhraseSet - called for...{0}", DomainID);
			try
			{
				Store store = Store.GetStore();
				Simias.Storage.Domain domain = store.GetDomain(DomainID);
				if(domain == null)
				{
					log.Debug("ValidatePassPhrase domain null");
				}

				Simias.Storage.Member member = domain.GetCurrentMember();
                if (member == null)
                {
                    log.Debug("ValidatePassPhrase member null");
                }
                else
                {
                    log.Debug("IsPassPhraseSet User: " + member.Name);
				    status = member.PassPhraseSetStatus();
                }
                			
				log.Debug("IsPassPhraseSet - called : {0}", status);
			}
			catch(Exception ex)
			{
				log.Debug("IsPassPhraseSet - called : {0}", ex.Message);
				throw ex;
			}
			return status;
		}
	

		/// <summary>
		/// Gets the directory path to the Simias data area.
		/// </summary>
		/// <returns>The path to the Simias data area.</returns>
		[WebMethod(EnableSession=true, Description="Gets the directory path to the Simias data area.")]
		[SoapDocumentMethod]
		public string GetSimiasDataPath()
		{
			return Store.StorePath;
		}

		/// <summary>
		/// Gets the search context from master.
		/// </summary>
		/// <returns>a string containing all search contexts from master</returns>
		[WebMethod(EnableSession=true, Description="Gets the ldap search context from master simias config file")]
		[SoapDocumentMethod]
		public string GetMasterSearchContext()
		{
			string contexts = "";
			string LdapSystemBookSection = "LdapProvider";
			string SearchKey = "Search";
			string XmlContextTag = "Context";
			string XmlDNAttr = "dn";

			string configFile = Path.Combine( Store.StorePath, Configuration.DefaultConfigFileName );
			if ( File.Exists( configFile ) == false )
				return contexts;
			Configuration config = new Configuration( Store.StorePath, true );
			//LdapSettings ldapSettings = LdapSettings.Get( configFile, true );
			//foreach(string context in ldapSettings.SearchContexts)

			XmlElement searchElement = config.GetElement( LdapSystemBookSection, SearchKey );
			if ( searchElement != null )
			{
				XmlNodeList contextNodes = searchElement.SelectNodes( XmlContextTag );
				foreach( XmlElement contextNode in contextNodes )
				{
					string context = contextNode.GetAttribute( XmlDNAttr);
					contexts += (context + "#");
				}
			}	
			return contexts;
		}

		/// <summary>
		/// Gets the process ID for the current running process.
		/// </summary>
		/// <returns></returns>
		[WebMethod(EnableSession=true, Description="Gets the process ID for the current running process.")]
		[SoapDocumentMethod]
		public int GetSimiasProcessID()
		{
			return Process.GetCurrentProcess().Id;
		}


                /// <summary>
                /// Get iFolder Entries
                /// </summary>
                /// <param name="ifolderID">The ID of the iFolder.</param>
                /// <param name="type">Type of serarch</param>
                /// <param name="relPath">relative Path of the intial object.</param>
                /// <param name="index">The Search Start Index</param>
                /// <param name="max">The Search Max Count of Results</param>
                /// <param name="accessID">The Access User ID.</param>
                /// <returns>A Set of iFolderEntry Objects</returns>
                [WebMethod(EnableSession=true, Description="Gets the process ID for the current running process.")]
                [SoapDocumentMethod]
                public NodeEntrySet GetEntries(string ifolderID, int type, string relPath, int index, int max, string accessID)
                {
                        int total = 0;
                        int i = 0;
                        long TotalCount = 0;
                        Simias.Storage.SearchPropertyList SearchPrpList = new Simias.Storage.SearchPropertyList();
 
                        Store store = Store.GetStore();
 
                        Collection c = store.GetCollectionByID(ifolderID);
 
                        if (c == null)
                        {
                                throw new SimiasException(ifolderID);
                        }
 
                        //iFolder.Impersonate(c, accessID);
 
                        // build the result list
                        ArrayList list = new ArrayList();
                        try
                        {
 
                                if(type  <= 0 || type > 2)
                                        SearchPrpList.Add(PropertyTags.FileSystemPath,"*",  SearchOp.Begins);
                                else if(type == 1)
                                        SearchPrpList.Add(PropertyTags.FileSystemPath,  relPath,  SearchOp.Begins);
                                else if(type == 2)
                                        SearchPrpList.Add(PropertyTags.FileSystemPath,  relPath,  SearchOp.Equal);
 
                                SearchPrpList.Add(BaseSchema.ObjectType, NodeTypes.MemberType, SearchOp.Not_Equal);
                                ICSList searchList = c.Search(SearchPrpList);
 
                                TotalCount = searchList.Count;
                                        SearchState searchState = new SearchState( c.ID, searchList.GetEnumerator() as ICSEnumerator, searchList.Count );
 
                                if(index > 0)
                                        searchState.Enumerator.SetCursor(Simias.Storage.Provider.IndexOrigin.SET, index);
 
 
                                foreach(ShallowNode sn in searchList)
                                {
                                        if(max != 0 && i++ >= max )
                                                break;
                                        try
                                        {
                                                Node n = c.GetNodeByID(sn.ID);
                                                NodeEntry entry = new NodeEntry( n);
                                                list.Add(entry);
                                        }
                                        catch (Exception ex)
                                        {
                                                log.Debug("Error: "+ex.Message);
                                                log.Debug("Error Trace: "+ex.StackTrace);
                                        }
                                }
                        }
                        catch (Exception ex)
                        {
                                log.Debug("Error: "+ex.Message);
                                log.Debug("Error Trace: "+ex.StackTrace);
                        }
 
                        return new NodeEntrySet((NodeEntry[])list.ToArray(typeof(NodeEntry)), TotalCount);
                }

                /// <summary>
                /// Get the Restore Status information for given ifolderid
                /// </summary>
                /// <returns></returns>
                [WebMethod(EnableSession=true, Description="Get the Restore Status information for given ifolderid.")]
                [SoapDocumentMethod]
                public int GetRestoreStatusForCollection(string ifolderid, out int totalcount, out int finishedcount)
                {
                        int retval=-1;
                        Store store = Store.GetStore();
                        Collection col = store.GetCollectionByID(ifolderid);
                        retval = col.RestoreStatus;
                        totalcount = col.TotalRestoreFileCount;
                        finishedcount = col.RestoredFileCount;
                        if( retval == 1)
                        {
                                if(RestoreThread == null || RestoreThread.IsAlive == false)
                                {
                                        col.RestoreStatus = 2;
                                        col.Commit();
                                        retval = 2;
                                }
                        }
                        if( RestoreThread == null || RestoreThread.IsAlive == false)
                        {
                                RestoreThread = null;
                        }
                        return retval;
                }

                /// <summary>
                /// Get the Restore Status information for given ifolderid
                /// </summary>
                /// <returns></returns>
                [WebMethod(EnableSession=true, Description="Set the Restore Status information for given ifolderid.")]
                [SoapDocumentMethod]
                public int SetRestoreStatusForCollection(string ifolderid,  int status, int totalcount, int finishedcount)
                {
                        int retval=-1;
                        try
                        {
                                Store store = Store.GetStore();
                                Collection col = store.GetCollectionByID(ifolderid);
                                col.RestoreStatus = status;
                                col.TotalRestoreFileCount = totalcount;
                                col.RestoredFileCount = finishedcount;
                                col.Commit();
                                retval = 0;
                        }
                        catch { }
                        return retval;
                }
 
 
 
                /// <summary>
                /// Gets the process ID for the current running process.
                /// </summary>
                /// <returns></returns>
                [WebMethod(EnableSession=true, Description="Gets the process ID for the current running process.")]
                [SoapDocumentMethod]
                public int ResetRootNode(string ifolderid)
                {
                        return Store.ResetRootNode(ifolderid);
                }

                /// <summary>
                /// Gets the process ID for the current running process.
                /// </summary>
                /// <returns></returns>
                [WebMethod(EnableSession=true, Description="Gets the process ID for the current running process.")]
                [SoapDocumentMethod]
                public bool GetEncryptionDetails(string ifolderid, out string eKey, out string eBlob, out string eAlgorithm, out string rKey)
                {
                        bool status = false;
                        try
                        {
                                Store store = Store.GetStore();
                                Collection col = store.GetCollectionByID(ifolderid);
                                eAlgorithm = col.EncryptionAlgorithm;
                                eKey = col.EncryptionKey;
                                eBlob = col.EncryptionBlob;
                                rKey = col.RecoveryKey;
                                status = true;
                        }
                        catch
                        {
                                eKey = null;
                                eBlob = null;
                                eAlgorithm = null;
                                rKey = null;
                        }
                        return status;
                }

	}




	/// <summary>
	/// Type of Domain (Enterprise/Workgroup)
	/// </summary>
	[Serializable]
	public enum DomainType
	{
		/// <summary>
		/// A Master Role
		/// </summary>
		Master,

		/// <summary>
		/// A Slave Role
		/// </summary>
		Slave,

		/// <summary>
		/// A Local Role
		/// </summary>
		Local,

		/// <summary>
		/// No Role
		/// </summary>
		None
	};

	/// <summary>
	/// Domain information
	/// </summary>
	[Serializable]
	public class DomainInformation
	{
        private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(DomainInformation));
		/// <summary>
		/// Domain Type
		/// </summary>
		public DomainType Type;

		/// <summary>
		/// Domain Active
		/// true - a state where collections belonging
		/// to the domain can synchronize, remote invitations
		/// can occur etc.
		/// false - no remote actions will take place
		/// </summary>
		public bool Active;

		/// <summary>
		/// Indicates if the domain is authenticated or not.
		/// </summary>
		public bool Authenticated;

		/// <summary>
		/// Domain Name
		/// </summary>
		public string Name;

		/// <summary>
		/// Domain Description
		/// </summary>
		public string Description;

		/// <summary>
		/// Domain ID
		/// </summary>
		public string ID;

		/// <summary>
		/// The unique member/user ID.
		/// </summary>
		public string MemberUserID;

		/// <summary>
		/// The name of the member object
		/// </summary>
		public string MemberName;

		/// <summary>
		/// Url to the remote domain service
		/// </summary>
		public string RemoteUrl;

		/// <summary>
		/// POBox ID
		/// </summary>
        public string POBoxID;

		/// <summary>
		/// The Simias URL for this domain.
		/// </summary>
		public string HostUrl;

		/// <summary>
		/// The host for this domain.
		/// </summary>
		public string Host;

		/// <summary>
		/// <b>True</b> if the local domain is a slave (client).
		/// </summary>
		public bool IsSlave;

		/// <summary>
		/// <b>True</b> if the local domain is the default domain.
		/// </summary>
		public bool IsDefault;

		// TODO: We need to rework this (possibly when multi-server work is done) so that
		// we don't return status and grace login counts in this structure.
		/// <summary>
		/// The status of the authentication request.
		/// </summary>
		public Simias.Authentication.StatusCodes StatusCode;

		/// <summary>
		/// The grace logins remaining.  Valid if StatusCode == StatusCode.SuccessInGrace
		/// </summary>
		public int RemainingGraceLogins;

		/// <summary>
		/// Constructor
		/// </summary>
		public DomainInformation()
		{
		}

		/// <summary>
		/// Constructs a DomainInformation object.
		/// </summary>
		/// <param name="domainID">The ID of the domain to base this object on.</param>
		public DomainInformation(string domainID)
		{
			Store store = Store.GetStore();

			Domain cDomain = store.GetDomain(domainID);
			log.Debug("DomainInformation: domainID is {0} and domain name is {1}",domainID,cDomain.Name);
			Simias.Storage.Member cMember = cDomain.GetCurrentMember();
			//Simias.POBox.POBox poBox = 	Simias.POBox.POBox.FindPOBox(store, domainID, cMember.UserID);
			//this.POBoxID = ( poBox != null ) ? poBox.ID : "";
			DomainAgent domainAgent = new DomainAgent();
			this.Active = domainAgent.IsDomainActive(cDomain.ID);
			this.Authenticated = domainAgent.IsDomainAuthenticated(cDomain.ID);
			this.Type = GetDomainTypeFromRole(cDomain.Role);
			this.ID = domainID;
			this.Name = cDomain.Name;
			this.Description = cDomain.Description;
			this.MemberUserID = cMember.UserID;
			this.MemberName = cMember.Name;

			Uri uri = DomainProvider.ResolveLocation(domainID);
			this.RemoteUrl = (uri != null) ?
				uri.ToString() + "/DomainService.asmx" :
				String.Empty;

			this.HostUrl = (uri != null) ? uri.ToString() : String.Empty;
			this.Host = ParseHostAndPort(this.HostUrl);
			this.IsSlave = cDomain.Role.Equals(Simias.Sync.SyncRoles.Slave);
			this.IsDefault = domainID.Equals(store.DefaultDomain);
		}

		/// <summary>
		/// Create a string representation
		/// </summary>
		/// <returns>A string representation</returns>
		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			
			string newLine = Environment.NewLine;

			builder.AppendFormat("Domain Information{0}", newLine);
			builder.AppendFormat("  ID               : {0}{1}", this.ID, newLine);
			builder.AppendFormat("  Type             : {0}{1}", this.Type.ToString(), newLine);
			builder.AppendFormat("  Name             : {0}{1}", this.Name, newLine);
			builder.AppendFormat("  Description      : {0}{1}", this.Description, newLine);
			builder.AppendFormat("  Member User ID   : {0}{1}", this.MemberUserID, newLine);
			builder.AppendFormat("  Member Node Name : {0}{1}", this.MemberName, newLine);
			builder.AppendFormat("  Remote Url       : {0}{1}", this.RemoteUrl, newLine);
			builder.AppendFormat("  POBox ID         : {0}{1}", this.POBoxID, newLine);
			builder.AppendFormat("  HostUrl          : {0}{1}", this.HostUrl, newLine);
			builder.AppendFormat("  Host             : {0}{1}", this.Host, newLine);

			return builder.ToString();
		}

		private DomainType GetDomainTypeFromRole(SyncRoles role)
		{
			DomainType type = DomainType.None;

			switch (role)
			{
				case SyncRoles.Master:
					type = DomainType.Master;
					break;

				case SyncRoles.Slave:
					type = DomainType.Slave;
					break;

				case SyncRoles.Local:
					type = DomainType.Local;
					break;
			}

			return type;
		}

		/// <summary>
		/// The purpose of this method is to be able to strip off the URL parts
		/// off of the Simias URL.  There's no reason to show this to an end
		/// user.
		/// </summary>
		private string ParseHostAndPort(string hostUrl)
		{
			// hostUrl will be in the following format:
			//     http(s)://servername[:optional port]/simias10
			//               ^^We're after this part^^^
			
			if (hostUrl == null || hostUrl == "") return "";	// Prevent a null return

			int doubleSlashPos = hostUrl.IndexOf("//");
			int lastSlashPos   = hostUrl.IndexOf('/', doubleSlashPos + 2);

			if (doubleSlashPos > 0 && lastSlashPos > 0)
				return hostUrl.Substring(doubleSlashPos + 2, lastSlashPos - doubleSlashPos - 2);

			return hostUrl;
		}
	}
}






