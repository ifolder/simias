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
*                 $Author: Mike Lasky <mlasky@novell.com>
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
using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Simias;
using Simias.Authentication;
using Simias.Service;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;

// shorty
using SCodes = Simias.Authentication.StatusCodes;

namespace Simias
{
	/// <summary>
	/// Internal class for managing and authenticating local credentials
	/// 
	/// Note: Today we only support 'basic'
	/// </summary>
	internal class LocalCredentials
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		//private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( LocalCredentials ) );

		static private readonly int GuidLength = Guid.NewGuid().ToString().Length;
		private string domainID;
		private string username;
		private string password;
		private string authType;

		private readonly char[] colonDelimeter = {':'};
		private readonly char[] backDelimeter = {'\\'};
		#endregion

		#region Properties
		public string AuthType
		{
			get { return this.authType; }
			set { this.authType = value; }
		}

		public string DomainID
		{
			get { return this.domainID; }
			set { this.domainID = value; }
		}

		public string Password
		{
			get { return this.password; }
			set { this.password = value; }
		}

		public string Username 
		{
			get { return this.username; }
			set { this.username = value; }
		}
		#endregion

		#region Constructors
		public LocalCredentials()
		{
		}

        /// <summary>
        /// Set the credentials locally
        /// </summary>
        /// <param name="username">User name as string</param>
        /// <param name="password">Password as string</param>
		public LocalCredentials( string username, string password )
		{
			this.username = username;
			this.password = password;
			this.authType = "basic";
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the credentials from an encoded authorization header.
		/// </summary>
		/// <param name="authHeader"></param>
		/// <returns></returns>
		public bool AuthorizationHeaderToCredentials( string authHeader )
		{
			bool returnStatus = false;

			// Make sure we are dealing with "Basic" credentials
			if ( authHeader.StartsWith( "Basic " ) )
			{
				// The authHeader after the basic signature is encoded
				authHeader = authHeader.Remove( 0, 6 );
				byte[] credential = System.Convert.FromBase64String( authHeader );
				string decodedCredential = System.Text.Encoding.Default.GetString( credential, 0, credential.Length );
   
				// Clients that newed up a NetCredential object with a URL
				// come though on the authorization line in the following format:
				// http://domain:port/simias10/service.asmx\username:password

				string[] credentials = decodedCredential.Split( this.backDelimeter );
				if ( credentials != null )
				{
					if ( credentials.Length == 1 )
					{
						credentials = decodedCredential.Split( this.colonDelimeter, 2 );
					}
					else if ( credentials.Length >= 2 )
					{
						credentials = credentials[ credentials.Length - 1 ].Split( colonDelimeter, 2 );
					}

					if ( credentials.Length == 2 )
					{
						this.username = credentials[ 0 ];

						// The password portion is really two GUIDs back-to-back. 
						// The first GUID is the local domain ID and the second is the password.
						if ( credentials[1].Length >= ( GuidLength * 2 ) )
						{
							this.password = credentials[ 1 ].Substring( GuidLength );
							this.domainID = credentials[ 1 ].Substring( 0, GuidLength );

							this.authType = "basic";
							returnStatus = true;
						}
					}
				}
			}

			return returnStatus;
		}

		/// <summary>
		/// Returns whether the object has credentials.
		/// </summary>
		/// <returns></returns>
		public bool HasCredentials()
		{
			return ( ( this.username != null ) && ( this.password != null ) ) ? true : false;
		}
		#endregion
	}


	/// <summary>
	/// Implementation of the DomainProvider Interface for the local domain.
	/// </summary>
	public class LocalProvider : IDomainProvider, IThreadService
	{
		#region Class Members

		/// <summary>
		/// Used to log messages.
		/// </summary>
		//private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( LocalProvider ) );

		/// <summary>
		/// String used to identify domain provider.
		/// </summary>
		static private string providerName = "Local Domain Provider";
		static private string providerDescription = "Domain Provider for Simias Local Domain";

		/// <summary>
		/// Store object.
		/// </summary>
		private Store store = Store.GetStore();

		#endregion

		#region IDomainProvider Members

		/// <summary>
		/// Gets the name of the domain provider.
		/// </summary>
		public string Name
		{
			get { return providerName; }
		}

		/// <summary>
		/// Gets the description of the domain provider.
		/// </summary>
		public string Description
		{
			get { return providerDescription; }
		}

		/// <summary>
		/// Performs authentication to the specified domain.
		/// </summary>
		/// <param name="domain">Domain to authenticate to.</param>
		/// <param name="httpContext">HTTP-specific request information. This is passed as a parameter so that a domain 
		/// provider may modify the HTTP request by adding special headers as necessary.
		/// 
		/// NOTE: The domain provider must NOT end the HTTP request.
		/// </param>
		/// <returns>The status from the authentication.</returns>
		public Authentication.Status Authenticate( Domain domain, HttpContext httpContext )
		{
			// Assume failure
			Simias.Authentication.Status status = new Simias.Authentication.Status( SCodes.InvalidCredentials );

			// Check for an authorization header.
			string[] encodedCredentials = httpContext.Request.Headers.GetValues( "Authorization" );
			if ( ( encodedCredentials != null ) && ( encodedCredentials[ 0 ] != null ) )
			{
				// Get the credentials from the auth header.
				LocalCredentials creds = new LocalCredentials();
				bool success = creds.AuthorizationHeaderToCredentials( encodedCredentials[ 0 ] );
				if ( success )
				{
					// Valid credentials?
					if ( ( creds.Username != null ) && ( creds.Password != null ) && ( creds.DomainID != null ) )
					{
						// Only support basic and make sure that the domain is local.
						if ( ( creds.AuthType == "basic" ) && ( creds.DomainID == store.LocalDomain ) )
						{
							// Get the member of the local domain and compare the passwords.
							string CurrentUserID = store.GetUserIDFromDomainID(creds.DomainID);
							Member member = domain.GetMemberByID(CurrentUserID);
							if ( ( member != null ) && ( store.LocalPassword == creds.Password ) )
							{
								status.UserName = member.Name;
								status.UserID = member.UserID;
								status.statusCode = SCodes.Success;
							}
						}
					}
				}
			}

			return status;
		}

		/// <summary>
		/// Indicates to the provider that the specified collection has
		/// been deleted and a mapping is no longer required.
		/// </summary>
		/// <param name="domainID">The identifier for the domain from
		/// where the collection has been deleted.</param>
		/// <param name="collectionID">Identifier of the collection that
		/// is being deleted.</param>
		public void DeleteLocation( string domainID, string collectionID )
		{
		}

		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers or
		/// FindNextDomainMembers methods.</param>
		public void FindCloseDomainMembers( string searchContext )
		{
		}

		/// <summary>
		/// Starts a search for all domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, int count, out string searchContext, out Member[] memberList, out int total )
		{
			searchContext = null;
			memberList = null;
			total = 0;
			return false;
		}

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Name of attribute to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, string attributeName, string searchString, SearchOp operation, int count, out string searchContext, out Member[] memberList, out int total )
		{
			searchContext = null;
			memberList = null;
			total = 0;
			return false;
		}

		/// <summary>
		/// Continues the search for domain members from the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindNextDomainMembers( ref string searchContext, int count, out Member[] memberList )
		{
			memberList = null;
			return false;
		}

		/// <summary>
		/// Continues the search for domain members previous to the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindPreviousDomainMembers( ref string searchContext, int count, out Member[] memberList )
		{
			memberList = null;
			return false;
		}

		/// <summary>
		/// Continues the search for domain members from the specified record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="offset">Record offset to return members from.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindSeekDomainMembers( ref string searchContext, int offset, int count, out Member[] memberList )
		{
			memberList = null;
			return false;
		}

		/// <summary>
		/// Determines if the provider claims ownership for the 
		/// specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <returns>True if the provider claims ownership for the 
		/// specified domain. Otherwise, False is returned.</returns>
		public bool OwnsDomain( string domainID )
		{
			return ( domainID == store.LocalDomain ) ? true : false;
		}

		/// <summary>
		/// Informs the domain provider that the specified member object is about to be
		/// committed to the domain's member list. This allows an opportunity for the 
		/// domain provider to add any domain specific attributes to the member object.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <param name="member">Member object that is about to be committed to the domain's member list.</param>
		public void PreCommit( string domainID, Member member )
		{
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// domain.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID )
		{
			return null;
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// collection.
		/// </summary>
		/// <param name="domainID">Identifier for the domain that the
		/// collection belongs to.</param>
		/// <param name="collectionID">Identifier of the collection to
		/// find the network location for.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID, string collectionID )
		{
			return null;
		}

		/// <summary>
		/// Returns the network location of where to create a collection.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will own the 
		/// collection.</param>
		/// <param name="collectionID">Identifier of the collection that
		/// is being created.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID, string userID, string collectionID )
		{
			return null;
		}

		/// <summary>
		/// Returns the network location of where to the specified user's POBox is located.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will owns the POBox.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolvePOBoxLocation( string domainID, string userID )
		{
			return null;
		}

		/// <summary>
		/// Returns the network address of the host
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="hostID">The host to resolve.</param>
		/// <returns>A Uri object that contains the network location.</returns>
		public Uri ResolveHostAddress( string domainID, string hostID )
		{
			return null;
		}

		/// <summary>
		/// Sets a new host address for the domain.
		/// </summary>
		/// <param name="domainID">Identifier of the domain for network address
		/// to be changed.</param>
		/// <param name="hostLocation">A Uri object containing the new network
		/// address for the domain.</param>
		public void SetHostLocation( string domainID, Uri hostLocation )
		{
			// Not needed by this implementation.
		}

		#endregion

		#region IThreadService Members

		/// <summary>
		/// Starts the thread service.
		/// </summary>
		public void Start()
		{
			// Register with the domain provider service.
			DomainProvider.RegisterProvider( this );
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			// Unregister with the domain provider service.
			DomainProvider.Unregister( this );
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public int Custom( int message, string data )
		{
			return 0;
		}

		#endregion
	}
}
