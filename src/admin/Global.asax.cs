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
*                 $Author: Mike Lasky (mlasky@novell.com)
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
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Security.Cryptography.X509Certificates;


namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : System.Web.HttpApplication
	{
		/// <summary>
		/// Log
		/// </summary>
		private static readonly iFolderWebLogger log = new iFolderWebLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name );


		/// <summary>
		/// Constructor
		/// </summary>
		public Global()
		{
			InitializeComponent();
		}	
		
		/// <summary>
		/// Application Start
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Start( Object sender, EventArgs e )
		{
			// Set the current working directory.
			Environment.CurrentDirectory = Server.MapPath( null );

			// certificate policy
			ServicePointManager.CertificatePolicy = new SingleCertificatePolicy(
				System.Configuration.ConfigurationSettings.AppSettings.Get( "SimiasCert" ) );

			// resources
			Application[ "RM" ] = new ResourceManager( "Novell.iFolderWeb.Admin.iFolderAdmin",
				Assembly.GetExecutingAssembly() );
		}
 
		/* NOTE: Commented out for performance per Gonzalo
		/// <summary>
		/// Session Start
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Session_Start( Object sender, EventArgs e )
		{
		}

		/// <summary>
		/// Application Begin Request
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_BeginRequest( Object sender, EventArgs e )
		{
		}
		*/

		/// <summary>
		/// Application PreRequestHandlerExecute
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_PreRequestHandlerExecute( Object sender, EventArgs e )
		{
			// NOTE: always reset the UI culture code
			Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

			if (Context.Session != null)
			{
				// culture
				string code = Session[ "Language" ] as String;
				if ( ( code != null ) && ( code.Length > 0 ) )
				{
					try
					{
						Thread.CurrentThread.CurrentUICulture =	CultureInfo.CreateSpecificCulture( code );
					}
					catch( Exception ex )
					{
						log.Info( Context, ex, "Culture: {0}", code );
					}
				}
			}
		}

		/* NOTE: Commented out for performance per Gonzalo
		/// <summary>
		/// Application End Request
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_EndRequest( Object sender, EventArgs e )
		{
		}
		
		/// <summary>
		/// Application Authenticate Request
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_AuthenticateRequest( Object sender, EventArgs e )
		{
		}
		*/

		/// <summary>
		/// Application Error
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_Error( Object sender, EventArgs e )
		{

			Exception ex = Server.GetLastError().GetBaseException();
			
			// Pass the compile exceptions through for debugging
			if ( !( ex is HttpCompileException ) && !( ex is InvalidCastException ) )
			{
				Server.ClearError();

				// log
				log.Error( Context, ex, "Application Error" );

				// NOTE: with some errors a session is not available yet and the
				// preferred method of transfering the exception will not work
				// in this case, so we redirect with the text on the query
				// line (this might cut the message short in some browsers).
				if ( Context.Session != null )
				{
					Session[ "Exception" ] = ex;
					Server.Transfer( "Error.aspx" );
				}
				else
				{
					Response.Redirect( "Error.aspx?ex=" + Server.UrlEncode( ex.ToString() ) );
				}
			}
		}

		/* NOTE: Commented out for performance per Gonzalo
		/// <summary>
		/// Session End
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Session_End( Object sender, EventArgs e )
		{
		}

		/// <summary>
		/// Application End
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void Application_End( Object sender, EventArgs e )
		{
		}
		*/
			
		#region Web Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
		}

		#endregion
	}

	/// <summary>
	/// Certificate Problem
	/// </summary>
	internal enum CertificateProblem : long
	{
		CertEXPIRED                   = 0x800B0101,
		CertVALIDITYPERIODNESTING     = 0x800B0102,
		CertROLE                      = 0x800B0103,
		CertPATHLENCONST              = 0x800B0104,
		CertCRITICAL                  = 0x800B0105,
		CertPURPOSE                   = 0x800B0106,
		CertISSUERCHAINING            = 0x800B0107,
		CertMALFORMED                 = 0x800B0108,
		CertUNTRUSTEDROOT             = 0x800B0109,
		CertCHAINING                  = 0x800B010A,
		CertREVOKED                   = 0x800B010C,
		CertUNTRUSTEDTESTROOT         = 0x800B010D,
		CertREVOCATION_FAILURE        = 0x800B010E,
		CertCN_NO_MATCH               = 0x800B010F,
		CertWRONG_USAGE               = 0x800B0110,
		CertUNTRUSTEDCA               = 0x800B0112
	}

	/// <summary>
	/// Single Certificate Policy
	/// </summary>
	internal class SingleCertificatePolicy : ICertificatePolicy
	{
		string certificateString;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="certificateString"></param>
		public SingleCertificatePolicy( string certificateString )
		{
			this.certificateString = certificateString;
		}

		/// <summary>
		/// Check Validation Result
		/// </summary>
		/// <param name="srvPoint"></param>
		/// <param name="certificate"></param>
		/// <param name="request"></param>
		/// <param name="certificateProblem"></param>
		/// <returns></returns>
		public bool CheckValidationResult( ServicePoint srvPoint,
			System.Security.Cryptography.X509Certificates.X509Certificate certificate,
			WebRequest request, 
			int certificateProblem )
		{
			bool result = false;

			if ( ( certificateProblem == 0 ) || CertificateProblem.CertEXPIRED.Equals( certificateProblem ) ||
				( ( certificate != null ) && ( certificate.Issuer.Equals( (new X509Certificate(Convert.FromBase64String(certificateString)).Issuer ) ) ) ))
			{
				result = true;
			}

			return result;
		}
	}
}

