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

namespace Novell.iFolderWeb.Admin
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Resources;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using Simias;
	using Simias.Storage;

	/// <summary>
	///		Summary description for AccountEnabled.
	/// </summary>
	public class SecurityState : System.Web.UI.UserControl
	{
		/// <summary>
		/// Different encryption states
		/// </summary>
		public enum Encryption
		{
			//EnforceSSL is currently used to denote EnforceSharing, pl don't remove it.
			
			None = 0,
			Encrypt = 1,
			EnforceEncrypt = 2,
			SSL = 4,
			EnforceSSL = 8
		}
		
		#region Class Members

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Account DIV.
		/// </summary>
		protected HtmlGenericControl EncryptNav;

		/// <summary>
		/// Account policy control.
		/// </summary>
		protected Label EncryptionTitle;

//		protected CheckBox Enabled;

		/// <summary>
		/// Account policy control.
		/// </summary>

		/// <summary>
		/// Control that contains the limit tag.
		/// </summary>
		protected Label SSLTitle;

		/// <summary>
		/// Control that contains the limit value.
		/// </summary>
	//	protected TextBox Status;
        /// <summary>
        /// Encryption checkbox
        /// </summary>
		protected CheckBox encryption;

        /// <summary>
        /// checkbox for enforcing encryption 
        /// </summary>
		protected CheckBox enforceEncryption;

        /// <summary>
        /// SSL checkbox
        /// </summary>
		protected CheckBox ssl;

        /// <summary>
        /// Enforce SSL checkbox
        /// </summary>
		protected CheckBox enforceSSL;

        /// <summary>
        /// iFolderAdmin instance
        /// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// Account policy control.
		/// </summary>
	//	protected Label DisabledTag;


		/// <summary>
		/// Event that notifies consumer that the checkbox has changed.
		/// </summary>
		public event EventHandler CheckChanged = null;

		/// <summary>
		/// to keep track of encryption check box on page load
		/// </summary>
		protected bool EncryptionWasChecked;
	
		/// <summary>
		/// to keep track whether sharing was enforced for this particular user earlier
		/// </summary>
		protected bool SharingWasEnforced;


		#endregion

		#region Properties
/*
		public bool PolicyChanged
		{
			get
			{
				return Enabled.Checked;
			}
		}
*/
		#endregion

		#region Private Methods

		/// <summary>
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{web = Session[ "Connection" ] as iFolderAdmin;
			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				EncryptionTitle.Text = GetString("ENCRYPTION");
				//SSLTitle.Text = "Secure Data Transfer";
//				EncryptionTag.Text = "Set Encryption";
//				Enabled.Checked = false;
				encryption.Text = rm.GetString("ON");
				//ssl.Text = "On";
		//		lbl_encryption.Text = "Encrypt";
		//		lbl_ssl.Text = "Use SSL";
				ssl.Visible = enforceSSL.Visible = false;
				enforceEncryption.Text = rm.GetString("ENFORCED");
				//enforceSSL.Text = "Enforced";
				enforceEncryption.Enabled = enforceSSL.Enabled = false;
				encryption.Enabled = ssl.Enabled = true;
				encryption.Checked = enforceEncryption.Checked = false;
				//ssl.Checked = enforceSSL.Checked = false;
				
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the account enable checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void EncryptionCheckChanged( Object sender, EventArgs e )
		{
			if( encryption.Checked == true )
			{
				enforceEncryption.Enabled = true;

			}
			else
			{
				enforceEncryption.Enabled = false;
				enforceEncryption.Checked = false;
			}

			if ( CheckChanged != null )
			{
				CheckChanged( sender, e );
			}
		
		}

		/*protected void sslCheckChanged( Object sender, EventArgs e )
		{
			if( ssl.Checked == true)
			{
				enforceSSL.Enabled = true;
			}
			else
			{
				enforceSSL.Enabled = false;
				enforceSSL.Checked = false;
			}
			if ( CheckChanged != null )
			{
				CheckChanged( sender, e );
			}

		}*/

        /// <summary>
        /// Event handler to handle if Enforce encryption checkbox is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		protected void EnforceCheckChanged( Object sender, EventArgs e )
		{
			if( CheckChanged != null )
			{
				CheckChanged( sender, e );
			}
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key">Key to the localized string.</param>
		/// <returns>Localized string.</returns>
		protected string GetString( string key )
		{
			return rm.GetString( key );
		}

		/// <summary>
		/// If true then PolicyID is a user ID.
		/// </summary>
		private bool IsUser
		{
			get { return Request.Path.EndsWith( "UserDetails.aspx" ); }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the account policy for the current ifolder user.
		/// </summary>
		/// <param name="policy">iFolder policy object</param>
		public void GetEncryptionPolicy( iFolderPolicy policy )
		{
			EncryptNav.Visible = false;
		}

		/// <summary>
		/// Gets the account policy for the system.
		/// </summary>
		/// <param name="policy">System policy object</param>
		public void GetEncryptionPolicy( SystemPolicy policy )
		{
			int securityStatus = policy.EncryptionStatus;
			encryption.Checked = enforceEncryption.Checked = ssl.Checked = enforceSSL.Checked = false;
			int status = securityStatus & (int) Encryption.Encrypt;
			if( status == (int)Encryption.Encrypt)
			{
				encryption.Checked = true;
				/// next line for bug id 296014 , where if the encryption check box is enabled (irrespective of enforce checkbox) once on system level, it should be disabled.
				encryption.Enabled = false;
				encryption.Enabled = false;
				enforceEncryption.Enabled = true;
				status = securityStatus & (int) Encryption.EnforceEncrypt;
				if( status == (int)Encryption.EnforceEncrypt )
					enforceEncryption.Checked = true;
			}
			else
			{
				enforceEncryption.Checked = false;
				enforceEncryption.Enabled = false;
			}
			/*status = securityStatus & (int) Encryption.SSL;
			if( status == (int)Encryption.SSL )
			{
				ssl.Checked = true;
				enforceSSL.Enabled = true;
				status = securityStatus & (int) Encryption.EnforceSSL;
				if( status == (int)Encryption.EnforceSSL)
					enforceSSL.Checked = true;
			}
			else
			{
				enforceSSL.Checked = false;
				enforceSSL.Enabled = false;
			}*/

		}
		
		/// <summary>
		/// Gets the account policy for the current user.
		/// </summary>
		/// <param name="policy">User policy object</param>
		public void GetEncryptionPolicy( UserPolicy policy )
		{
			int securityStatus = policy.EncryptionStatus;
			encryption.Checked = enforceEncryption.Checked = ssl.Checked = enforceSSL.Checked = false;
			SystemPolicy SystemPolicy = web.GetSystemPolicy();
			int SysEncrPolicy = SystemPolicy.EncryptionStatus;
			int GroupEncrPolicy = web.GetGroupEncryptionPolicy(policy.UserID);
			int DerivedStatus = 0;
			Session["SharingWasEnforced"] = "false";
			Session["EncryptionWasChecked"] = "false";
			DerivedStatus = DeriveStatus(SysEncrPolicy, GroupEncrPolicy, securityStatus, securityStatus);
			if(DerivedStatus != 0)
			{
				if( (DerivedStatus & (int)Encryption.Encrypt) == (int) Encryption.Encrypt)
				{
					encryption.Enabled = false;
					Session["EncryptionWasChecked"] = "true";
        			if( (DerivedStatus & (int)Encryption.EnforceEncrypt) == (int) Encryption.EnforceEncrypt)
        			{
            				encryption.Checked = enforceEncryption.Checked = true;
            				enforceEncryption.Enabled = true;
       				}
					else
        			{
          				encryption.Checked = true; 
            			enforceEncryption.Checked = false;
             			enforceEncryption.Enabled = true;
            		}
        			// next check for bug id 296014 , where if this user has created an encrypted iFolder then disable 
					// the encryption check box for him. 
        			if(IsUser)
        			{
        	
        				if(web.IsPassPhraseSetForUser(Request.Params["ID"]))
        					encryption.Enabled = false;
        			}	
				}
				else if((DerivedStatus & (int) Encryption.EnforceSSL) == (int) Encryption.EnforceSSL)
				{
					// this is the case of enforceSharing for the particular user on userlevel policy
					encryption.Checked = enforceEncryption.Checked = false;
					Session["SharingWasEnforced"] = "true";
				}
			}
		}	
			
		///<summary>
        	/// Get the policy for an iFolder.
        	/// </summary>
        	/// <param name="policy">The iFolderPolicy object.</param>
		private int DeriveStatus(int system, int group, int user, int preference)
		{
			//Preference is not done
			if( preference == 0)
    		{
    			if(system != 0) {
					if(group != 0){
						return group|system;
					} else {
    					return system;
					}
				}
				else if(group != 0)
					return group;
    			return user;
    		}
    		else
    		{
    			if(user != 0)
    				return user;
				else if (group != 0)
					return group;
				else
    				return system;
    		}
		}

		

		/// <summary>
		/// Sets the account policy for the user.
		/// </summary>
		/// <param name="policy">User policy where the account information will be set.</param>
		public void SetEncryptionPolicy( SystemPolicy policy )
		{
			int securityStatus=0;
			if( encryption.Checked)
				securityStatus += (int) Encryption.Encrypt; //1;
			if(enforceEncryption.Checked)
				securityStatus += (int) Encryption.EnforceEncrypt; //2;
			if(ssl.Checked)
				securityStatus += (int) Encryption.SSL; //4;
			if(enforceSSL.Checked)
				securityStatus += (int) Encryption.EnforceSSL; //8;
			policy.EncryptionStatus = securityStatus;
		}

        /// <summary>
        /// Sets encryption policy for this user
        /// </summary>
        /// <param name="policy">UserPolicy that will be set</param>
		public void SetEncryptionPolicy( UserPolicy policy )
		{
			int securityStatus=0;
			string SharingWasEnforced = Session["SharingWasEnforced"] as string;
			string EncryptionWasChecked = Session["EncryptionWasChecked"] as string;
			if( encryption.Checked)
				securityStatus += (int)Encryption.Encrypt; //1;
			if(enforceEncryption.Checked)
				securityStatus += (int)Encryption.EnforceEncrypt; //2;
				
				//obsolete
			//if(ssl.Checked)
				//securityStatus += (int)Encryption.SSL; //4;
			//if(enforceSSL.Checked)
				//securityStatus += (int)Encryption.EnforceSSL; //8;
				
			// Temporarily i am using enforceSSL for enforceSharing , because enforseSSL is obsolete and enforceSharing is needed.
			// Apply enforceSharing on user level only if encryption check box was 'on' or earlier sharing was enforced for this user
			
			
			if((SharingWasEnforced.Equals("true") || EncryptionWasChecked.Equals("true")) && encryption.Checked == false && enforceEncryption.Checked == false)
				securityStatus += (int) Encryption.EnforceSSL ; //8
			Session["SharingWasEnforced"] = "false";
			Session["EncryptionWasChecked"] = "false";
				
			policy.EncryptionStatus = securityStatus;
		}

		/// <summary>
		/// Sets the checkbox enabled or disabled state
		/// </summary>
		public bool SetCheckBoxEnabledState
		{
			set
			{
				encryption.Enabled = value;
				enforceEncryption.Enabled = value;
			}
		}

		#endregion

		#region Web Form Designer generated code

		/// <summary>
		/// OnInit
		/// </summary>
		/// <param name="e"></param>
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		//	Enabled.CheckedChanged += new EventHandler( EncryptionCheckChanged );
			encryption.CheckedChanged += new EventHandler( EncryptionCheckChanged );
			//ssl.CheckedChanged += new EventHandler( sslCheckChanged );
			enforceEncryption.CheckedChanged += new EventHandler( EnforceCheckChanged);
			enforceSSL.CheckedChanged += new EventHandler( EnforceCheckChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
