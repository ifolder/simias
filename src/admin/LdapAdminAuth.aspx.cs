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
*                 $Author : Anil Kumar <kuanil@novell.com>
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
using System.Resources;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace Novell.iFolderWeb.Admin
{
        /// <summary>
        /// Summary description for editing ldap details 
        /// </summary>
        public class LdapAdminAuth : System.Web.UI.Page
	{
#region Class Members
		private static readonly iFolderWebLogger log = new iFolderWebLogger(
				System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// iFolder Connection to the remote server
		/// </summary>
		private iFolderAdmin remoteweb;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Logged in admin system rights instance
		/// </summary>
		UserSystemAdminRights uRights;

		/// <summary>
		/// Logged in user system rights value
		/// </summary>
		int sysAccessPolicy = 0;

		/// <summary>
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;

		/// <summary>
		/// ldap admin name edit control.
		/// </summary>
		protected TextBox LdapAdminName;

		/// <summary>
		/// ldap admin password control.
		/// </summary>
		protected TextBox LdapAdminPwd;

		/// <summary>
		/// LDAP Server name.
		/// </summary>
		protected TextBox LdapServer;

		/// <summary>
		/// LDAP Proxy User
		/// </summary>
		protected TextBox LdapProxyUser;

		/// <summary>
		/// LDAP Proxy User
		/// </summary>
		protected TextBox LdapProxyUserPwd;

		/// <summary>
		/// LDAP Proxy User
		/// </summary>
		protected TextBox ConfirmLdapProxyUserPwd;

		/// <summary>
		/// Log list control.
		/// </summary>
		protected DropDownList LdapSslList;

		/// <summary>
		/// Search context
		/// </summary>
		protected TextBox LdapSearchContext;

		/// <summary>
		/// OK button control.
		/// </summary>
		protected Button OkButton;

		/// <summary>
		/// cancel button control.
		/// </summary>
		protected Button CancelButton;

#endregion

#region Properties

		/// <summary>
		/// Gets the Server ID.
		/// </summary>
		private string ServerID
		{
			get { return Request.Params[ "ServerID" ]; } 
		}

		/// <summary>
		/// Gets the Server Name.
		/// </summary>
		private string serverName
		{
			get { return Request.Params[ "serverName" ]; }
		}

#endregion

#region Private Methods

		/// <summary>
		///  Builds the breadcrumb list for this page.
		/// </summary>
		private void BuildBreadCrumbList(string ServerID,string serverName)
		{
			TopNav.AddBreadCrumb( GetString( "SERVERS" ), "Servers.aspx" );
			TopNav.AddBreadCrumb( serverName, String.Format( "ServerDetails.aspx?id={0}", ServerID ) );
			TopNav.AddBreadCrumb( GetString( "LDAPDETAILS" ), null );
			// Pass this page information to create the help link
			TopNav.AddHelpLink(GetString("LDAPADMINAUTH"));
		}

		/// <summary>
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// connection
			web = Session[ "Connection" ] as iFolderAdmin;

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			string userID = Session[ "UserID" ] as String;
			if(userID != null && ServerID != null && ServerID != String.Empty)
				sysAccessPolicy = web.GetUserSystemRights(userID, ServerID);
			else
				sysAccessPolicy = 0; 
			uRights = new UserSystemAdminRights(sysAccessPolicy);
			if(uRights.ServerPolicyManagementAllowed == false)
				Page.Response.Redirect(String.Format("Error.aspx?ex={0}&Msg={1}",GetString( "ACCESSDENIED" ), GetString( "ACCESSDENIEDERROR" )));

			if ( !IsPostBack )
			{
				remoteweb = new iFolderAdmin ();


				OkButton.Text = GetString( "OK" );
				CancelButton.Text = GetString( "CANCEL" );

			}
		}


		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender(object sender, EventArgs e)
		{
			GetLdapDetails ();
			// Set the breadcrumb list.
			BuildBreadCrumbList(ServerID,serverName);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			this.Load += new System.EventHandler(this.Page_Load);
		}


		/// <summary>
		/// Gets Ldap details into the fields
		/// </summary>
		/// <returns>The name of the host node.</returns>
		private void GetLdapDetails()
		{
			iFolderServer server = web.GetServer( ServerID );

			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;
			remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
			remoteweb.GetAuthenticatedUser();
			server = remoteweb.GetServer ( ServerID);

			//Pick information from IdentityProvider
			LdapInfo ldapInfo = remoteweb.GetLdapDetails();
			LdapServer.Text = ldapInfo.Host ;
			LdapSearchContext.Text = ldapInfo.SearchContexts;
			LdapProxyUser.Text = ldapInfo.ProxyDN;
			LdapProxyUserPwd.Text = ldapInfo.ProxyPassword;
			ConfirmLdapProxyUserPwd.Text = ldapInfo.ProxyPassword;
			string [] options = new string[2];

			options[0] =  GetString( "YES" );
			options[1] =  GetString( "NO" ) ;

			LdapSslList.DataSource = options;
			LdapSslList.DataBind();

			LdapSslList.SelectedValue = ldapInfo.SSL ? GetString ("YES") : GetString ("NO");
		}

		private bool UpdateProxyUserInfo(string ldapAdmin, string ldapAdminPwd, string proxyDN, string ProxyDNPwd)
		{
			bool status = true;
			try
			{
				iFolderServer[] list = web.GetServers();
				foreach( iFolderServer server in list )
				{
					//check all servers other than current server.
					if(server.ID != ServerID)
					{
						log.Info("Connecting to {0}", server.ID);
						remoteweb = new iFolderAdmin ();
						remoteweb.PreAuthenticate = true;
						remoteweb.Credentials = web.Credentials;
						remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
						remoteweb.GetAuthenticatedUser();
						//Pick information from IdentityProvider
						LdapInfo ldapInfo = remoteweb.GetLdapDetails();
						log.Info("Proxy User = {0} on {1}", ldapInfo.ProxyDN, server.ID);
						if(proxyDN == ldapInfo.ProxyDN)
						{
							//ldapInfo.ProxyDN = ProxyDN;
							ldapInfo.ProxyPassword = ProxyDNPwd;
							log.Info("Changing proxy user password on : {0}", server.ID);
							remoteweb.SetLdapDetails (ldapInfo, ldapAdmin, ldapAdminPwd, server.ID);
						}
					}
				}
			}
			catch(Exception ex)
			{
				status = false;
				log.Info("Exception in UpdateProxyUserInfo : (0)", ex.Message);
				TopNav.ShowInfo(String.Format(GetString("ALLSERVERPROXYPASSWORDCHANGEERROR")));
				return status;
			}
			return status;
		}

		/// <summary>
                /// To check if ldap admin name and password text boxes are empty 
                /// </summary>
		private bool validOK()
		{
			if (LdapAdminName.Text.Trim() == "" || LdapAdminPwd.Text == "")
				return false;
			return true;
		}

		/// <summary>
                /// To check if valid data are entered while editing ldap details 
                /// </summary>
		private bool DataValidated()
		{
			if ( LdapServer.Text.Trim() == "" ||  LdapSearchContext.Text.Trim() == "" || LdapProxyUser.Text.Trim() == "" )
			{
				return false;
			}
			return true;
		}

		#endregion

		#region Protected Methods
		/// <summary>
		/// Event handler that gets called when OK button is clicked.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void OnOkButton_Click( object source, EventArgs e )
		{
			string currentServerProxyDN = null;
			string currentServerProxyDNPwd = null; 
			bool proxychagnestatus = true;
			if( ! validOK() )
			{
				TopNav.ShowError (GetString("ENTERLDAPDETAILS"));
				return;
			}
			if(!DataValidated())
			{
				TopNav.ShowError(GetString("ERRORINVALIDDATA"));
				return;
			}
			if(String.Compare(LdapProxyUserPwd.Text.Trim(), ConfirmLdapProxyUserPwd.Text.Trim()) != 0)
			{
				TopNav.ShowError(GetString("ERRORPROXYPASSWORDSDOESNOTMATCH"));
				return;

			}
			/// if ldap admin username and password is right, go ahead
			iFolderServer server = web.GetServer( ServerID );
			remoteweb = new iFolderAdmin ();
			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;
			remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
			remoteweb.GetAuthenticatedUser();
			server = remoteweb.GetServer ( ServerID);
			LdapInfo ldapInfo = new LdapInfo ();
			ldapInfo.Host = LdapServer.Text.Trim();
			ldapInfo.SearchContexts = LdapSearchContext.Text;
			ldapInfo.ProxyDN = LdapProxyUser.Text;
			currentServerProxyDN = LdapProxyUser.Text;
			ldapInfo.ProxyPassword = LdapProxyUserPwd.Text;
			currentServerProxyDNPwd = LdapProxyUserPwd.Text;
			ldapInfo.SSL = (LdapSslList.SelectedValue == GetString("YES")) ? true : false;

			try
			{
				remoteweb.SetLdapDetails (ldapInfo, LdapAdminName.Text.Trim(), LdapAdminPwd.Text, ServerID);
				//now the proxy user info changed, check if the same proxy is
				//used in any other servers.
				if (ldapInfo.ProxyDN != null && ldapInfo.ProxyPassword != null)
				{
					proxychagnestatus = UpdateProxyUserInfo(LdapAdminName.Text.Trim(), LdapAdminPwd.Text, 
							currentServerProxyDN, currentServerProxyDNPwd );
				}
			}
			catch(Exception ex)
			{
				TopNav.ShowInfo(String.Format( "{0} {1}",GetString("UNABLETOEDITLDAPDETAILS"), ex.Message));
				GetLdapDetails();
				return;
			}
			if (proxychagnestatus != false) 
				Response.Redirect(String.Format("ServerDetails.aspx?ID={0}",ServerID));
		}

		/// <summary>
                /// Event handler that gets called when the Cancel button is clicked.
                /// </summary>
                /// <param name="source"></param>
                /// <param name="e"></param>
                protected void OnCancelButton_Click( object source, EventArgs e )
                {
      	                Response.Redirect(String.Format("ServerDetails.aspx?ID={0}",ServerID));
                }

		#endregion

		#region Web Form Designer generated code^M

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
		#endregion

                #region Protected Methods

                /// <summary>
                /// Get a Localized String
                /// </summary>
                /// <param name="key"></param>
                /// <returns></returns>
                protected string GetString( string key )
                {
                        return rm.GetString( key );
                }

		#endregion	
	}
}






























