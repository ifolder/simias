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
*                 $Author: Brady Anderson <banderso@novell.com>
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
using System.Net;

using Simias;

namespace Simias.Client.Authentication
{

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
