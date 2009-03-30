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

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// New iFolder Page
	/// </summary>
	public class iFolderNewPage : Page
	{
        /// <summary>
        /// enum to denote encryption status
        /// </summary>
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
		/// Header page
		/// </summary>
		protected HeaderControl Head;

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
		protected RadioButton Encryption;

		/*/// <summary>
		/// ssl the thick client to server data transfer
		/// </summary>
		protected CheckBox ssl;*/

		/// <summary>
		/// share the thick client to server data transfer
		/// </summary>
		protected RadioButton shared;
		
		/// <summary>
		/// List the RA Agents
		/// </summary>
		protected DropDownList RAList;

		/// <summary>
		/// The Select RA Label
		/// </summary>
		protected Label SelectLabel;

		/// <summary>
		/// The pass-phrase Label Button
		/// </summary>
		protected Label PassPhraseLabel;

		/// <summary>
		/// The pass-phrase Label Button
		/// </summary>
		protected Label VerifyPassPhraseLabel;


		/// <summary>
		/// pass-phrase text box
		/// </summary>
		protected TextBox PassPhraseText;

		/// <summary>
		/// pass-phrase text box
		/// </summary>
		protected TextBox VerifyPassPhraseText;
		
		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// SHARED
		/// </summary>
		bool Sharable;

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
				Encryption.Text = GetString("ENCRYPTTHEIFOLDER");
				Encryption.ToolTip = GetString("ENCRYPTIONCONDITION");
				//ssl.Text = GetString("Secure Data Transfer");
				shared.Text = GetString("SHARABLE");
				shared.Checked = true;
				shared.ToolTip = GetString("SHARECONDITION");
				SelectLabel.Text = GetString("SELECTRECOVERYAGENT");
				PassPhraseLabel.Text = GetString("ENTERPASSPHRASE");
				VerifyPassPhraseLabel.Text = GetString("REENTER_PASSPHRASE");
				VerifyPassPhraseLabel.Visible=VerifyPassPhraseText.Visible = false;
				RAList.Enabled = false;
				PassPhraseText.Enabled = false;
				ChangeStatus();
				
				// Pass this page information to create the help link
				Head.AddHelpLink(GetString("IFOLDERNEW"));
			}
		}
		
		/// <summayr>
		/// Whenever encryption is checked by user or enforced in the policy , then this will be called 
		/// <summary>
		private void ChangeForEncryption()
		{
			bool PassPhraseSet = web.IsPassPhraseSet();
				
			if(PassPhraseSet )
			{
				SelectLabel.Visible = RAList.Visible = VerifyPassPhraseLabel.Visible = VerifyPassPhraseText.Visible = false;
				PassPhraseText.Enabled = true;
				string SessionPassPhrase = Session["SessionPassPhrase"] as string;
				if(SessionPassPhrase != null)
				{
					// user is in current session, so don't ask again for the passphrase
					PassPhraseLabel.Visible = PassPhraseText.Enabled = PassPhraseText.Visible = false;
				}
				else
				{
					PassPhraseText.Enabled = PassPhraseText.Visible = true;
				}
			}
			else
			{
				string [] RAListStrTemp= web.GetRAList();
				
				if (RAListStrTemp != null)
				{
					string [] RAListStr = new string [RAListStrTemp.Length + 1];
					// making 1st entry of dropdownlist as None
					RAListStr[0] = GetString("NONE");
					//There may be a case that there is no certificate in the configured certificate path
					for (int i = 1; i <= RAListStrTemp.Length ; i++)
						RAListStr[i] = String.Copy(RAListStrTemp[i-1]);
					RAList.DataSource = RAListStr;
					RAList.DataBind();
					SelectLabel.Visible = RAList.Enabled = RAList.Visible = true;
				}
				else
				{
					// admin had not configurred Recovery-agent 
					SelectLabel.Visible = RAList.Enabled = RAList.Visible = false;
				}
	
				PassPhraseText.Enabled = PassPhraseText.Visible = VerifyPassPhraseLabel.Visible = VerifyPassPhraseText.Visible = VerifyPassPhraseText.Enabled = true;
			}	
		}
		
		
		/// <summary>		
		/// Get the policy from the server and displayed in the check box
		/// </summary>
		private void ChangeStatus()
		{
			int SecurityPolicy= web.GetEncryptionPolicy();
                        Encryption.Checked = shared.Checked = false;
                        Encryption.Enabled = shared.Enabled = false;

                        if(SecurityPolicy !=0)
                        {
                        		
                                if( (SecurityPolicy & (int)SecurityState.encryption) == (int) SecurityState.encryption)
                                {
                                        if( (SecurityPolicy & (int)SecurityState.enforceEncryption) == (int) SecurityState.enforceEncryption)
                                        {
                                                Encryption.Checked = true;
                                         		Encryption.Enabled = true;
                                         		shared.Enabled = false;
                                         		ChangeForEncryption();
                                        }        
                                        else
                                        {
												Encryption.Enabled = true;
												shared.Enabled = true;
												Encryption.Checked = true; 
												ChangeForEncryption();
                                        }       
                                }
                                else
                                {
                                	// it means enforce sharing is enabled for this user. 
									Encryption.Checked = false;
									Encryption.Enabled = false;
									shared.Enabled = true;
									shared.Checked = true;
                                }
                                /*if( (SecurityPolicy & (int)SecurityState.SSL) == (int) SecurityState.SSL)
                                {
                                        if( (SecurityPolicy & (int)SecurityState.enforceSSL) == (int) SecurityState.enforceSSL)
                                        {
                                                shared.Checked = true;
                                        }  
                                        else
                                        {
                                                shared.Enabled = true;
                                          		//Encryption.Enabled = false;      
                                        }  
                                }*/
                        }
                        else
                        {
							Encryption.Checked = false;
							Encryption.Enabled = false;
							shared.Enabled = true;
							shared.Checked = true;
                        }
                        
                        if(shared.Checked)
						{
							RAList.Enabled = false;
							PassPhraseText.Enabled = false;
							VerifyPassPhraseText.Enabled = false;
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
				case "PathTooLongException":
					Message.Text = GetString("IFOLDER.PATHTOOLONGEXCEPTION");
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
			this.Encryption.CheckedChanged += new EventHandler(Encryption_CheckedChanged);
			this.shared.CheckedChanged += new EventHandler(shared_CheckedChanged);
		}

		#endregion

		/// <summary>
		/// Encrypt radio button checked/unchecked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Encryption_CheckedChanged(object sender, EventArgs e)
		{
			if(Encryption.Checked)
			{	
				ChangeForEncryption();
			}	
		}

		/// <summary>
		/// shared radio button checked/unchecked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void shared_CheckedChanged(object sender, EventArgs e)
		{
			if(shared.Checked)
			{
				RAList.SelectedValue = GetString("NONE");
				RAList.Enabled = false;
				PassPhraseText.Enabled = false;
				PassPhraseText.Text = "";
				VerifyPassPhraseText.Enabled = false;
				VerifyPassPhraseText.Text = "";
			}
		
		}

		/// <summary>
		/// Create Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CreateButton_Click(object sender, EventArgs e)
		{
			string name = NewiFolderName.Text.Trim();
			string description = NewiFolderDescription.Text.Trim();
			string PassPhraseStr = PassPhraseText.Text.Trim();
			string VerifyPassPhraseStr = VerifyPassPhraseText.Text.Trim();
			string SessionPassPhrase = Session["SessionPassPhrase"] as string;

			//Validate the inputs
			if(name.Length == 0)
			{
				Message.Text = GetString("IFOLDER.NONAME");
				return;
			}

			//Very first time this code will be executed
			if(Encryption.Checked == true && (web.IsPassPhraseSet()==false))
			{
				if((PassPhraseStr == "" || VerifyPassPhraseStr == ""))
				{
					Message.Text = GetString("ENTER_PASSPHRASE");
					return;
				}
				if((! PassPhraseStr.Equals(VerifyPassPhraseStr)))
				{
					Message.Text = GetString("PASSPHRASE_NOT_MATCH");
					VerifyPassPhraseText.Text = "";
					return;
				}
			}
			else if(Encryption.Checked == true && PassPhraseStr == "" && SessionPassPhrase == null)
			{
				//create encrypted folder and pasphrase is not provided
				Message.Text = GetString("ENTER_PASSPHRASE");
				return;
			}

			// create iFolder
			iFolder ifolder;				
			try
			{
				if(Encryption.Checked == true)
				{
					EncryptionAlgorithm = "BlowFish";
					Sharable = false;

					//If not avaiable in the session 
					if(SessionPassPhrase == null)
					{
						bool PassPhraseSet = web.IsPassPhraseSet();
						if(PassPhraseSet)
						{
							Status ObjValidate = web.ValidatePassPhrase(PassPhraseStr);
							if(ObjValidate.statusCode != StatusCodes.Success)
							{
								Message.Text = GetString("PASSPHRASE_INCORRECT");
								PassPhraseText.Text = "";
								return;
							}
							else
								Session["SessionPassPhrase"]= PassPhraseStr;	
						}
						else 
						{
							//This block will get executed very first time
							
							// i am storing the passphrase temporarily in session variable , avoiding passing through URL 
							Session["SessionPassPhrase"]= PassPhraseStr;

							// Do a utf-8 encoding as there may be multibyte characters in the name
							UTF8Encoding utf8Name = new UTF8Encoding();
							byte[] EncodediFolderNameInByte = utf8Name.GetBytes(name);
							string iFolderNameBase64 = Convert.ToBase64String(EncodediFolderNameInByte);
					
							byte[] EncodediFolderDescInByte = utf8Name.GetBytes(description);
							string iFolderDescBase64 = Convert.ToBase64String(EncodediFolderDescInByte);

							if(web.GetRAList() != null)
							{
								Response.Redirect(String.Format("iFolderCertificate.aspx?RAName={0}&EncryptionAlgorithm={1}&name={2}&description={3}",
												RAList.SelectedValue, EncryptionAlgorithm, iFolderNameBase64, iFolderDescBase64));
								//SetPassphrase will be done in the redirected page and store in the session
							}
							else
							{
								//This case should come when no RA is configured by the admin
								web.SetPassPhrase(PassPhraseStr, null, null);
							}
						}
					}
					else
						PassPhraseStr = Session["SessionPassPhrase"] as string;
				}
				else
				{
					PassPhraseStr = null;
					//if not encrypted then sharable must be true
					Sharable = true;
				}
			
				//check for no of ifolders per user limit policy
				if ( web.GetiFolderLimitPolicyStatus(null) != 1)
				{
					Message.Text = GetString("ERRORIFOLDERLIMITEXCEPTION");
					return;
				}	
				
	
				// Send the ifolder Name, Description, Security details and the encryption algorithm				
				ifolder = web.CreateiFolder(name, description, Sharable, EncryptionAlgorithm, PassPhraseStr);
				
				if(SessionPassPhrase == null)
					Session["SessionPassPhrase"] = PassPhraseStr;
					
				// Redirect to the browser page
				Response.Redirect("Browse.aspx?iFolder=" + ifolder.ID);
			}	
			catch(SoapException ex)
			{
				if (!HandleException(ex))
				{
					Message.Text = ex.Message;
					return;
				}
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
