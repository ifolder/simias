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
using System.Xml;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// Settings Page
	/// </summary>
	public class SettingsPage : Page
	{
		/// <summary>
		/// Page Size Label
		/// </summary>
		protected Label PageSizeLabel;

		/// <summary>
		/// Page Size List
		/// </summary>
		protected DropDownList PageSizeList;

		/// <summary>
                /// Context
                /// </summary>
                protected HomeContextControl HomeContext;

		/// <summary>
		/// Message Box
		/// </summary>
		protected MessageControl Message;

		/// <summary>
		/// Header 
		/// </summary>
		protected HeaderControl Head;

		/// <summary>
		/// Change Password Control 
		/// </summary>
		protected CheckBox ChangePassword;

		/// <summary>
		/// Change Password label control.
		/// </summary>
		protected Label ChangePasswordLabel;

		/// <summary>
		/// pass-wordlabel 
		/// </summary>
		protected Label CurrentPasswordLabel;

		/// <summary>
		/// pass-word text box
		/// </summary>
		protected TextBox CurrentPasswordText;

		/// <summary>
		/// pass-wordlabel 
		/// </summary>
		protected Label NewPasswordLabel;

		/// <summary>
		/// pass-word text box
		/// </summary>
		protected TextBox NewPasswordText;

		/// <summary>
		/// pass-wordlabel 
		/// </summary>
		protected Label VerifyNewPasswordLabel;

		/// <summary>
		/// pass-word text box
		/// </summary>
		protected TextBox VerifyNewPasswordText; 

		/// <summary>
		/// The Save Button
		/// </summary>
		protected Button SaveButton;

		/// <summary>
		/// The Cancel Button
		/// </summary>
		protected Button CancelButton;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		///enum for password change status 
		/// </summary>
		public enum PasswordChangeStatus
                {
                        /// <summary>
                        /// Password Change is successful
                        /// </summary>
                        Success=0,

                        /// <summary>
                        /// Invalid Old Passowrd provided
                        /// </summary>
                        IncorrectOldPassword,

                        /// <summary>
                        /// Failed to reset password
                        /// </summary>
                        FailedToResetPassword,

                        /// <summary>
                        /// Login Disabled
                        /// </summary>
                        LoginDisabled,

                        /// <summary>
                        /// User account expired
                        /// </summary>
                        UserAccountExpired,

                        /// <summary>
                        /// User can not change password
                        /// </summary>
                        CanNotChangePassword,

                        /// <summary>
                        /// User password expired
                        /// </summary>
                        LoginPasswordExpired,

                        /// <summary>
                        /// Minimum password length restriction not met
                        /// </summary>
                        PasswordMinimumLength,

                        /// <summary>
                        /// User not found in simias
                        /// </summary>
                        UserNotFoundInSimias
                };



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
				ChangePasswordLabel.Text = GetString("CHANGEPASSWORD");		
				CurrentPasswordLabel.Text = GetString("CURRENTPASSWORD");
				NewPasswordLabel.Text = GetString("NEWPASSWORD");
				VerifyNewPasswordLabel.Text = GetString("CONFIRMNEWPASSWORD");

				SaveButton.Text = GetString("SAVE");
				CancelButton.Text = GetString("CANCEL");
				PageSizeLabel.Text = GetString("PAGESIZE");

				ChangePassword.Checked = true;

				// view
				ViewState["Referrer"] = Request.UrlReferrer;
				Head.AddHelpLink("SETTINGS");
			}
			if( Request.QueryString.Get("Error") != "" && Request.QueryString.Get("Error") != null && Request.QueryString.Get("Error").IndexOf("UNSUPPORTEDCHAR") != -1)
				{
					Message.Text = rm.GetString("UNSUPPORTEDCHAR");
					return;
				}



		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			// page size options
			string[] sizes = { "10", "25", "50", "100" };

			foreach(string size in sizes)
			{
				PageSizeList.Items.Add(size);
			}

			// Search is not required for Settings Page
			HomeContext.HideSearch();

			try
			{
				// load
				WebSettings settings = new WebSettings(web);
				string pageSizeString = settings.PageSize.ToString();

				// page size
				foreach(ListItem item in PageSizeList.Items)
				{
					if (item.Value == pageSizeString)
					{
						item.Selected = true;
					}
				}
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
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
			this.SaveButton.Click += new EventHandler(SaveButton_Click);
			this.CancelButton.Click += new EventHandler(CancelButton_Click);
		}

		#endregion


		/// <summary>
		/// Check-Box changed Event 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnChangePassword_Changed(object sender, EventArgs e)
		{
			bool isChecked = ( sender as CheckBox ).Checked;	
			CurrentPasswordText.Enabled=
			NewPasswordText.Enabled=
			VerifyNewPasswordText.Enabled= isChecked;
		}

		/// <summary>
		/// Save Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void SaveButton_Click(object sender, EventArgs e)
		{
			try
			{
				// load
				WebSettings settings = new WebSettings(web);
				
				// page size
				settings.PageSize = int.Parse(PageSizeList.SelectedValue);

				// save
				settings.Save(web);

				Session["Settings"] = settings;
			
				if(ChangePassword.Checked == true)
				{	
					if(CurrentPasswordText.Text.Trim() == "" || NewPasswordText.Text.Trim() == "" || VerifyNewPasswordText.Text.Trim() == "")
					{
						Message.Text = GetString("EMPTY_PASSWORD");
						return;
					}
					if(NewPasswordText.Text.Trim() != VerifyNewPasswordText.Text.Trim())
					{
						Message.Text = GetString("PASSWORDS_NOT_MATCH");
						return;
					}
					if(CurrentPasswordText.Text.Trim() == NewPasswordText.Text.Trim())
					{
						Message.Text = GetString("SAMEOLDPASSWORD");
						return;
					}
	
					int status = web.ChangePassword(CurrentPasswordText.Text.Trim(), NewPasswordText.Text.Trim());
					if(status != 0)
					{
						string FailedStatus = GetString("PASSWORDCHANGEFAILED");
						switch(status)
						{
							case 1:
								FailedStatus += GetString("INCORRECTOLDPASSWORD");
								break;
							case 2:
								FailedStatus += GetString("FAILEDTORESETPASSWORD");
								break;
							case 3:
								FailedStatus += GetString("LOGINDISABLED");
								break;
							case 4:
								FailedStatus += GetString("USERACCOUNTEXPIRED");
								break;
							case 5:
								FailedStatus += GetString("CANNOTCHANGEPASSWORD");
								break;
							case 6:
								FailedStatus += GetString("LOGINPASSWORDEXPIRED");
								break;
							case 7:
								FailedStatus += GetString("PASSWORDMINLENGTH");
								break;
							case 8:
								FailedStatus += GetString("USERNOTFOUNDINSIMIAS");
								break;
							default:
								FailedStatus += GetString("CHANGE.UNKNOWN");
								break;
						}
						Message.Text = FailedStatus;
						return;
					}		
					else
					{
						OnPasswordChanged("true");
					}
				}

			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
			
			// return
			CancelButton_Click(sender, e);
		}

		/// <summary>
		/// Cancel Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CancelButton_Click(object sender, EventArgs e)
		{
			// return
			Uri referrer = (Uri) ViewState["Referrer"];
			string url;

			if ((referrer == null) || (referrer.AbsolutePath.IndexOf("Login.aspx") != -1)
				|| (referrer.AbsolutePath.IndexOf("Settings.aspx") != -1))
			{
				url = "iFolders.aspx";
			}
			else
			{
				string TrimmedUrl = web.TrimUrl(referrer.ToString());
				url = TrimmedUrl;
			}
			
			// redirect
			Response.Redirect(url);
		}

		/// <summary>
		/// go to new page after password change is successful 
		/// </summary>
		/// <param name="PasswordChanged">Value entered in the ChangePassword Text Box</param>
		private void OnPasswordChanged(string PasswordChanged)
		{
			// return
			Uri referrer = (Uri) ViewState["Referrer"];
			string url;

			if ((referrer == null) || (referrer.AbsolutePath.IndexOf("Login.aspx") != -1)
				|| (referrer.AbsolutePath.IndexOf("Settings.aspx") != -1))
			{
				url = String.Format("iFolders.aspx?PasswordChanged={0}",PasswordChanged);
			}
			else
			{
				string TrimmedUrl = web.TrimUrl(referrer.ToString());
				url = TrimmedUrl;
				url += String.Format("?PasswordChanged={0}",PasswordChanged);
			}
			
			Head.Logout("PASSWORDCHANGESUCCESS");	
			// redirect
			//Response.Redirect(url);
			
		}
	}
}
