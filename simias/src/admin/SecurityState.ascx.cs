/***********************************************************************
 *  $RCSfile: AccountEnabled.ascx.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Mike Lasky (mlasky@novell.com)
 * 
 ***********************************************************************/

namespace Novell.iFolderWeb.Admin
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Resources;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	/// <summary>
	///		Summary description for AccountEnabled.
	/// </summary>
	public class SecurityState : System.Web.UI.UserControl
	{
		
		public enum Encryption
		{
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

		protected CheckBox encryption;
		protected CheckBox enforceEncryption;
		protected CheckBox ssl;
		protected CheckBox enforceSSL;



		/// <summary>
		/// Account policy control.
		/// </summary>
	//	protected Label DisabledTag;


		/// <summary>
		/// Event that notifies consumer that the checkbox has changed.
		/// </summary>
		public event EventHandler CheckChanged = null;

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
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				EncryptionTitle.Text = "Encryption";
				SSLTitle.Text = "Secure Data Transfer";
//				EncryptionTag.Text = "Set Encryption";
//				Enabled.Checked = false;
				encryption.Text = "On";
				ssl.Text = "On";
		//		lbl_encryption.Text = "Encrypt";
		//		lbl_ssl.Text = "Use SSL";
				enforceEncryption.Text = "Enforced";
				enforceSSL.Text = "Enforced";
				enforceEncryption.Enabled = enforceSSL.Enabled = false;
				encryption.Enabled = ssl.Enabled = true;
				encryption.Checked = enforceEncryption.Checked = ssl.Checked = enforceSSL.Checked = false;
				
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
		protected void sslCheckChanged( Object sender, EventArgs e )
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

		}
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
		//	EncryptNav.Visible = true;
//			if( !Enabled.Checked )
//				return;

			int securityStatus = policy.EncryptionStatus;
			encryption.Checked = enforceEncryption.Checked = ssl.Checked = enforceSSL.Checked = false;
			int status = securityStatus & (int) Encryption.Encrypt;
			if( status == (int)Encryption.Encrypt)
			{
				encryption.Checked = true;
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
			status = securityStatus & (int) Encryption.SSL;
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
			}

		}
		
		/// <summary>
		/// Gets the account policy for the current user.
		/// </summary>
		/// <param name="policy">User policy object</param>
		public void GetEncryptionPolicy( UserPolicy policy )
		{
	//		EncryptNav.Visible = false;
	//		Enabled.Checked = !policy.LoginEnabled;
//			if( !Enabled.Checked )
//				return;
			int securityStatus = policy.EncryptionStatus;
			int status;
			encryption.Checked = enforceEncryption.Checked = ssl.Checked = enforceSSL.Checked = false;
			status = securityStatus & (int) Encryption.Encrypt;
			if( status  == (int)Encryption.Encrypt )
			{
				encryption.Checked = true;
				enforceEncryption.Enabled = true;
				status = securityStatus & (int) Encryption.EnforceEncrypt;
				if( status == (int)Encryption.EnforceEncrypt)
					enforceEncryption.Checked = true;
			}
			else
			{
				enforceEncryption.Checked = false;
				enforceEncryption.Enabled = false;
			}
			status = securityStatus & (int) Encryption.SSL;
			if( status == (int)Encryption.SSL)
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
			}
		
		}

		/// <summary>
		/// Sets the account policy for the user.
		/// </summary>
		/// <param name="policy">User policy where the account information will be set.</param>
		public void SetEncryptionPolicy( SystemPolicy policy )
		{
		//	policy.LoginEnabled = !Enabled.Checked;
	// Added by ramesh
//			if(!Enabled.Checked)
//				return;
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

		public void SetEncryptionPolicy( UserPolicy policy )
		{
		//	policy.LoginEnabled = !Enabled.Checked;
	// Added by ramesh
//			if(!Enabled.Checked)
//				return;
			int securityStatus=0;
			if( encryption.Checked)
				securityStatus += (int)Encryption.Encrypt; //1;
			if(enforceEncryption.Checked)
				securityStatus += (int)Encryption.EnforceEncrypt; //2;
			if(ssl.Checked)
				securityStatus += (int)Encryption.SSL; //4;
			if(enforceSSL.Checked)
				securityStatus += (int)Encryption.EnforceSSL; //8;
			policy.EncryptionStatus = securityStatus;
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
			ssl.CheckedChanged += new EventHandler( sslCheckChanged );
			enforceEncryption.CheckedChanged += new EventHandler( EnforceCheckChanged);
			enforceSSL.CheckedChanged += new EventHandler( EnforceCheckChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
