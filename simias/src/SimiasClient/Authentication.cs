/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright  Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Net;

using Simias;

namespace Simias.Client.Authentication
{
	/// <summary>
	/// Defines the credential types stored on a domain.
	/// </summary>
	[Serializable]
	public enum CredentialType
	{
		/// <summary>
		/// Credentials have not been set on this domain.
		/// </summary>
		None,

		/// <summary>
		/// Credentials are not required for this domain.
		/// </summary>
		NotRequired,

		/// <summary>
		/// HTTP basic credentials.
		/// </summary>
		Basic,

		/// <summary>
		/// Public/Private key credentials.
		/// </summary>
		PPK
	}

	/// <summary>
	/// Summary description for Credentials
	/// </summary>
	public class DomainAuthentication
	{
		private string serviceName;
		private string domainID;
		private string password;
		private static CertPolicy certPolicy;

		/// <summary>
		/// Static constructor for the object.
		/// </summary>
		static DomainAuthentication()
		{
			// Set the credential policy for this process.
			certPolicy = new CertPolicy();
		}

		/// <summary>
		/// Constructor with all the necessary credentials for a basic authentication
		/// </summary>
		public DomainAuthentication(string serviceName, string domainID, string password)
		{
			this.serviceName = serviceName;
			this.domainID = domainID;
			this.password = password;
		}

		/// <summary>
		/// Authenticate to a remote Simias server
		/// </summary>
		/// <returns>Simias.Client.Authentication.Status object</returns>
		public Status Authenticate(Uri webServiceUri, string simiasDataPath)
		{
			Status status = null;

			try
			{
				SimiasWebService simiasSvc = new SimiasWebService();
				simiasSvc.Url = webServiceUri.ToString() + "/Simias.asmx";
				LocalService.Start( simiasSvc, webServiceUri, simiasDataPath );

				DomainInformation cInfo = simiasSvc.GetDomainInformation( this.domainID );
				if ( cInfo != null )
				{
					// Call Simias for a remote domain authentication
					status = simiasSvc.LoginToRemoteDomain(	this.domainID, this.password );
				}
				else
				{
					//status = new Status( StatusCodes.UnknownDomain );
				}
			}
			catch(Exception ex)
			{
				// DEBUG
				if (MyEnvironment.Mono)
					Console.WriteLine( "Authentication - caught exception: {0}", ex.Message );

				//status = new Status( StatusCodes.InternalException );
				//status.ExceptionMessage = ex.Message;
			}

			return status;
		}

		/// <summary>
		/// Logout from a remote Simias server
		/// </summary>
		/// <returns>Simias.Client.Authentication.Status object</returns>
		public Status Logout( Uri webServiceUri, string simiasDataPath)
		{
			Status status = null;

			try
			{
				SimiasWebService simiasSvc = new SimiasWebService();
				simiasSvc.Url = webServiceUri.ToString() + "/Simias.asmx";
				LocalService.Start( simiasSvc, webServiceUri, simiasDataPath );

				DomainInformation cInfo = simiasSvc.GetDomainInformation( this.domainID );
				if ( cInfo != null )
				{
					// Call Simias for a remote domain authentication
					status = simiasSvc.LogoutFromRemoteDomain( this.domainID );
				}
				else
				{
					//status = new Simias.Authentication.Status( StatusCodes.UnknownDomain );
				}
			}
			catch( Exception ex )
			{
				// DEBUG
				if (MyEnvironment.Mono)
					Console.WriteLine( "Authentication - caught exception: {0}", ex.Message );

				//status = new Status( StatusCodes.InternalException );
				//status.ExceptionMessage = ex.Message;
			}

			return status;
		}
	}
}
