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
 *  Author: Rob
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

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// New iFolder Page
	/// </summary>
	public class iFolderNewPage : Page
	{
                enum SecurityState
                {
                        encryption = 1,
                        enforceEncryption = 2,
                        SSL = 4,
                        enforceSSL = 8
                }

		/// <summary>
		/// Message Box
		/// </summary>
		protected MessageControl Message;

		/// <summary>
		/// The Create Button
		/// </summary>
		protected Button CreateButton;

		/// <summary>
		/// The Cancel Button
		/// </summary>
		protected Button CancelButton;

		/// <summary>
		/// New iFolder Name
		/// </summary>
		protected TextBox NewiFolderName;

		/// <summary>
		/// New iFolder Description
		/// </summary>
		protected TextBox NewiFolderDescription;

		/// <summary>
		/// Encrypt the file data
		/// </summary>
		protected CheckBox Encryption;

		/// <summary>
		/// ssl the thick client to server data transfer
		/// </summary>
		protected CheckBox ssl;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// SSL
		/// </summary>
		bool SSL;

		/// <summary>
		/// Encry Algorithm (in future it can be selected from gui)
		/// </summary>
		string EncryptionAlgorithm="";
		

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
				// strings
				CreateButton.Text = GetString("CREATE");
				CancelButton.Text = GetString("CANCEL");
				//Localization need to be enabled
				Encryption.Text = GetString("Encryption");
				ssl.Text = GetString("Secure Data Transfer");
				ChangeStatus();
			}
		}
		
		/// <summary>		
		/// Get the policy from the server and displayed in the check box
		/// </summary>
		private void ChangeStatus()
		{
			int SecurityPolicy= web.GetEncryptionPolicy();
                        Encryption.Checked = ssl.Checked = false;
                        Encryption.Enabled = ssl.Enabled = false;

                        if(SecurityPolicy !=0)
                        {
                                if( (SecurityPolicy & (int)SecurityState.encryption) == (int) SecurityState.encryption)
                                {
                                        if( (SecurityPolicy & (int)SecurityState.enforceEncryption) == (int) SecurityState.enforceEncryption)
                                                Encryption.Checked = true;
                                        else
                                                Encryption.Enabled = true;
                                }
                                if( (SecurityPolicy & (int)SecurityState.SSL) == (int) SecurityState.SSL)
                                {
                                        if( (SecurityPolicy & (int)SecurityState.enforceSSL) == (int) SecurityState.enforceSSL)
                                                ssl.Checked = true;
                                        else
                                                ssl.Enabled = true;
                                }
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
					
					// TEMP
					Message.Text = type;

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
			this.CreateButton.Click += new EventHandler(CreateButton_Click);
			this.CancelButton.Click += new EventHandler(CancelButton_Click);
		}

		#endregion

		/// <summary>
		/// Create Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CreateButton_Click(object sender, EventArgs e)
		{
			string name = NewiFolderName.Text.Trim();
			string description = NewiFolderDescription.Text.Trim();

			if (name.Length == 0)
			{
				Message.Text = GetString("IFOLDER.NONAME");
				return;
			}

			// create iFolder
			iFolder ifolder;				
			try
			{
				if(Encryption.Checked == true)
					EncryptionAlgorithm = "BlowFish";
				if(ssl.Checked == true)
					SSL = true;
					
				// Send the ifolder Name, Description, Security details and the encryption algorithm
				ifolder = web.CreateiFolder(name, description, SSL, EncryptionAlgorithm);

				// redirect
				Response.Redirect("Browse.aspx?iFolder=" + ifolder.ID);
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
		}

		/// <summary>
		/// Cancel Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CancelButton_Click(object sender, EventArgs e)
		{
			// redirect
			Response.Redirect("iFolders.aspx");
		}
	}
}
