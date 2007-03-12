/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
 *  Author: Anil
 *
 ***********************************************************************/

using System;
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
				AcceptButton.Text = GetString("ACCEPT");
				DenyButton.Text = GetString("DENY");
				
				// view
				ViewState["Referrer"] = Request.UrlReferrer;
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
			byte [] RACertificateObj = web.GetRACertificate(RAName);
			
			if(RACertificateObj != null && RACertificateObj.Length != 0)
			{	
				System.Security.Cryptography.X509Certificates.X509Certificate Cert = new System.Security.Cryptography.X509Certificates.X509Certificate(RACertificateObj);
				CertDetails.Text = Cert.ToString(true);
				Session["CertPublicKey"] = Cert.GetPublicKey();
			}
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
			string name, description;
			
			iFolder ifolder;
			try
			{
				RAName = Request.QueryString.Get("RAName");
				PassPhraseStr = Request.QueryString.Get("PassPhrase");
				//PassPhraseStr = Session["SessionPassPhrase"] as string;
				EncryptionAlgorithm = Request.QueryString.Get("EncryptionAlgorithm");
				name = Request.QueryString.Get("name");
				description = Request.QueryString.Get("description");
			
				//try getting publickey from current session
				CertPublicKey = Session["CertPublicKey"] as byte [] ;

				string PublicKey = Encoding.ASCII.GetString(CertPublicKey);
					
				web.SetPassPhrase(PassPhraseStr, RAName, PublicKey);

				// Send the ifolder Name, Description, Security details and the encryption algorithm
				ifolder = web.CreateiFolder(name, description, false, EncryptionAlgorithm, PassPhraseStr);
			
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
			Uri referrer = (Uri) ViewState["Referrer"];
			string url;

			if ((referrer == null) || (referrer.AbsolutePath.IndexOf("Login.aspx") != -1))
			{
				url = "iFolderNew.aspx";
			}
			else
			{
				url = referrer.ToString();
			}
			
			// redirect
			Response.Redirect(url);
		}
	}
}
