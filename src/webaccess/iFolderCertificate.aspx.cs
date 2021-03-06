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
*                 $Author: Anil Kumar (kuanil@novell.com)
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
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Resources;
using System.Web.Services.Protocols;
using System.Net;
using System.Text;


namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Item History Page
	/// </summary>
	public class iFolderCertificatePage : Page
	{
		
		/// <summary>
		/// Message Box
		/// </summary>
		protected MessageControl Message;
		
		/// <summary>
		/// Header control 
		/// </summary>
		protected HeaderControl Head;

		/// <summary>
		/// The Literal
		/// </summary>
		protected Literal Certificate;


		/// <summary>
		/// The text box to display certificate
		/// </summary>
		protected TextBox CertDetails;

		/// <summary>
		/// The Accept Button
		/// </summary>
		protected Button AcceptButton;
		
		/// <summary>
		/// The Deny Button
		/// </summary>
		protected Button DenyButton;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
                /// Gets the user or iFolder name.
                /// </summary>
                private string iFolderName
                {
                        // Now , through asp pages , for ifoldername field ,only utf8 encoded and base64 converted strings should be sent
                        get
                        {
                                string iFolderNameBase64 = Request.Params[ "name" ] ;
				if ( ( iFolderNameBase64 == null ) || ( iFolderNameBase64 == String.Empty ) )
                                {
                                        throw new HttpException( ( int )HttpStatusCode.BadRequest, "No ifolder name was specified." );
                                }
                                string param;
                                try{

                                        byte[] iFolderNameInByte = Convert.FromBase64String(iFolderNameBase64);
                                        UTF8Encoding utf8Name = new UTF8Encoding();
                                        param = utf8Name.GetString(iFolderNameInByte);
                                }catch(Exception ex)
                                {
                                        throw ex;
                                }

                                return param;
                        }
                }
	
		/// <summary>
                /// Gets the user or iFolder description.
                /// </summary>
                private string iFolderDescription
                {
                        // Now , through asp pages , for ifolderdescription field ,only utf8 encoded and base64 converted strings should be sent
                        get
                        {
                                string iFolderDescBase64 = Request.Params[ "description" ] ;

				if(iFolderDescBase64 == null)
                                {
                                        return String.Empty;
                                }

				string param = "";
				try{

                                        byte[] iFolderDescInByte = Convert.FromBase64String(iFolderDescBase64);
                                        UTF8Encoding utf8Name = new UTF8Encoding();
                                        param = utf8Name.GetString(iFolderDescInByte);
                                }catch(Exception ex)
                                {
                                        throw ex;
                                }
				return param;
			}
		}
	
		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, EventArgs e)
		{
			// connection
			web = (iFolderWeb)Session["Connection"];

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				// data
				BindData();
								
				// strings
				string RAName = Request.QueryString.Get("RAName");
				if(! RAName.Equals(GetString("NONE")))
				{
					Certificate.Text = GetString("CERTIFICATE");
					AcceptButton.Text = GetString("ACCEPT");
				}	
				else
				{
					AcceptButton.Text = GetString("CREATE");
					Certificate.Visible = false;
					CertDetails.Visible = false;
				}
				DenyButton.Text = GetString("DENY");
				
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			string RAName; 

			// query
			RAName = Request.QueryString.Get("RAName");
			
			//proceed only if there is a valid RAName 
			if(! RAName.Equals(GetString("NONE")))
			{
				byte [] RACertificateObj = web.GetRACertificate(RAName);
			
				if(RACertificateObj != null && RACertificateObj.Length != 0)
				{	
					System.Security.Cryptography.X509Certificates.X509Certificate Cert = new System.Security.Cryptography.X509Certificates.X509Certificate(RACertificateObj);
					CertDetails.Text = Cert.ToString(true);
					Session["CertPublicKey"] = Cert.GetPublicKey();
				}
			}
			else
			{
				Message.Text = GetString("NO.RA.SELECTED");
				return;
			}

			// Pass the page information so that it can be added to help link 
			Head.AddHelpLink(GetString("CERTIFICATE"));
		}	


		/// <summary>
		/// Handle Exceptions
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private bool HandleException(Exception e)
		{
			bool result = true;

			string type = WebUtility.GetExceptionType(e);

			// types
			switch(type)
			{
				case "AccessException":
					Message.Text = GetString("ENTRY.ACCESSEXCEPTION");
					break;

				default:
					result = false;
					break;
			}

			return result;
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString(string key)
		{
			return WebUtility.GetString(key, rm);
		}

		#region Web Form Designer

		/// <summary>
		/// On Initialize
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Initialize Components
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
			this.AcceptButton.Click += new EventHandler(AcceptButton_Click);
			this.DenyButton.Click += new EventHandler(DenyButton_Click);
		}

		#endregion

		/// <summary>
		/// Accept Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void AcceptButton_Click(object sender, EventArgs e)
		{
			string RAName, PassPhraseStr, EncryptionAlgorithm;
			byte [] CertPublicKey;
	
			iFolder ifolder;
			try
			{
				RAName = Request.QueryString.Get("RAName");
				//PassPhraseStr = Request.QueryString.Get("PassPhrase");
				PassPhraseStr = Session["SessionPassPhrase"] as string;
				EncryptionAlgorithm = Request.QueryString.Get("EncryptionAlgorithm");
			
				// If there was not any RA selected then RAName and PublicKey will be null
			
				if(! RAName.Equals(GetString("NONE")))
				{
					//try getting publickey from current session
					CertPublicKey = Session["CertPublicKey"] as byte [] ;

					string PublicKey = Convert.ToBase64String(CertPublicKey);
					
					web.SetPassPhrase(PassPhraseStr, RAName, PublicKey);
				}
				else
					web.SetPassPhrase(PassPhraseStr, null, null);
				
				// Send the ifolder Name, Description, Security details and the encryption algorithm
				ifolder = web.CreateiFolder(iFolderName, iFolderDescription, false, EncryptionAlgorithm, PassPhraseStr);
			
				Session["SessionPassPhrase"] = PassPhraseStr;
			
				// redirect
				Response.Redirect("Browse.aspx?iFolder=" + ifolder.ID);
			}
			catch(SoapException ex)
			{
				if(!HandleException(ex))
				{
					Message.Text = ex.Message; 
					AcceptButton.Enabled = false;
				}
				return;
			}
		}
		
		/// <summary>
		/// Deny Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DenyButton_Click(object sender, EventArgs e)
		{
			Session["CertPublicKey"] = null;
			Session["SessionPassPhrase"] = null;

			string url = "iFolderNew.aspx";
			// redirect
			Response.Redirect(url);
		}
	}
}
