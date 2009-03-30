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

	/// <summary>
	/// Summary description for Policy.
	/// </summary>
	public class Policy : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin remoteweb;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;
	
		private string MasterServerUrl;

		/// <summary>
		/// Account enabled control.
		/// </summary>
		protected AccountEnabled AccountEnabled;

		/// <summary>
		/// iFolder enabled control.
		/// </summary>
		protected iFolderEnabled iFolderEnabled;

		/// <summary>
                /// iFolder limit control.
                /// </summary>
                protected iFolderLimit iFolderLimit;


		/// <summary>
		/// Disk quota control.
		/// </summary>
		protected DiskSpaceQuota DiskQuota;

		// Added by ramesh
		protected SecurityState SecurityState;

		/// <summary>
		/// File size filter control.
		/// </summary>
		protected FileSizeFilter FileSize;

		/// <summary>
		/// File type filter control.
		/// </summary>
		protected FileTypeFilter FileType;

		/// <summary>
		/// Sync interval control.
		/// </summary>
		protected SyncInterval SyncInterval;

		/// <summary>
		/// Disable sharing policy.
		/// </summary>
		protected Sharing Sharing;

		/// <summary>
		/// Policy buttons that submit or cancel policy changes.
		/// </summary>
		public Button PolicyApplyButton;

		/// <summary>
		/// Policy buttons that submit or cancel policy changes.
		/// </summary>
		protected Button PolicyCancelButton;

		/// <summary>
		/// Incoming URL.
		/// </summary>
		protected string currentURL;

		/// <summary>
		/// Delegate to use to handle policy errors.
		/// </summary>
		public delegate void PolicyErrorHandler( object source, PolicyErrorArgs e );

	        /// <summary>
		/// Event that notifies consumer that an policy error occurred.
		/// </summary>
		public event PolicyErrorHandler PolicyError = null;
		
		

		#endregion

		#region Properties

		/// <summary>
		/// Enables or disables the policy apply and cancel buttons.
		/// </summary>
		private bool EnablePolicyButtons
		{
			set { PolicyApplyButton.Enabled = PolicyCancelButton.Enabled = value; }
		}

		/// <summary>
		/// If true then PolicyID is an iFolder ID.
		/// </summary>
		private bool IsiFolder
		{
			get { return Request.Path.EndsWith( "iFolderDetailsPage.aspx" ); }
		}

		/// <summary>
		/// If true then PolicyID is a user ID.
		/// </summary>
		private bool IsUser
		{
			get { return Request.Path.EndsWith( "UserDetails.aspx" ); }
		}

		/// <summary>
		/// If true then PolicyID is a system ID.
		/// </summary>
		private bool IsSystem
		{
			get { return Request.Path.EndsWith( "SystemInfo.aspx" ); }
		}

		/// <summary>
		/// Gets the ID to use for policy information.
		/// </summary>
		private string PolicyID
		{
			get { return Request.Params[ "ID" ]; } 
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Page_Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// connection
			web = Session[ "Connection" ] as iFolderAdmin;
			currentURL = web.Url;
			remoteweb = new iFolderAdmin();
			remoteweb.PreAuthenticate = true;
			remoteweb.Credentials = web.Credentials;
			remoteweb.Url = web.Url;

			iFolderServer[] list = web.GetServers();
			foreach( iFolderServer server in list )
			{
				if (server.IsMaster)
				{
					MasterServerUrl = server.PublicUrl;
					break;
				}
			}

			// localization
			rm = Application[ "RM" ] as ResourceManager;

			if ( !IsPostBack )
			{
				// Initialize the localized fields.
				PolicyApplyButton.Text = GetString( "SAVE" );
				PolicyCancelButton.Text = GetString( "CANCEL" );

				// Set the policy buttons to disabled on page load.
				EnablePolicyButtons = false;
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the apply policy button for an ifolder is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ApplyiFolderPolicy( Object sender, EventArgs e )
		{
			string ifolderLocation = web.GetiFolderLocation (PolicyID);
			if(ifolderLocation != null)
			{
				UriBuilder remoteurl = new UriBuilder(ifolderLocation);
				remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
				remoteweb.Url = remoteurl.Uri.ToString();
			}
		
			// Get the current policy settings.
			iFolderPolicy policy = remoteweb.GetiFolderPolicy( PolicyID );
			try
			{
				iFolderEnabled.SetiFolderEnabledPolicy( policy );
				DiskQuota.SetDiskSpacePolicy( policy );
				FileSize.SetFileSizePolicy( policy );
				FileType.SetFileTypePolicy( policy );
				SyncInterval.SetSyncPolicy( policy );
				Sharing.SetSharingPolicy( policy, PolicyID );
			}
			catch( ArgumentException ex )
			{
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( ex ) );
				}
			//	web.Url = currentURL;

				return;
			}

			// Verify and apply all the ifolder specified settings to the policy object.
			// Set the new policies and refresh the view.
			try
			{
				remoteweb.SetiFolderPolicy( policy );
			}
			catch ( Exception ex )
			{
				string errMsg = String.Format( GetString( "ERRORCANNOTSETIFOLDERPOLICY" ), PolicyID );
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( errMsg, ex ) );
				}
			//	web.Url = currentURL;

				return;
			}

			GetiFolderPolicies();
			EnablePolicyButtons = false;
			web.Url = currentURL;
		}

		/// <summary>
		/// Event handler that gets called when the apply policy button for an system policy is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ApplySystemPolicy( Object sender, EventArgs e )
		{
			// Verify and apply all the user specified settings to the policy object.

			iFolderServer[] list = web.GetServers();

                        foreach( iFolderServer server in list )
                        {
                                if (server.IsMaster)
                                {
                                       UriBuilder remoteurl = new UriBuilder(server.PublicUrl);
                                       remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                                       web.Url = remoteurl.Uri.ToString();
                                       break;
                                }
                        }
			
			SystemPolicy policy = web.GetSystemPolicy();
			try
			{
				DiskQuota.SetDiskSpacePolicy( policy );
				FileSize.SetFileSizePolicy( policy );
				FileType.SetFileTypePolicy( policy );
				SyncInterval.SetSyncPolicy( policy );
				iFolderLimit.SetiFolderLimitPolicy( policy );
				Sharing.SetSharingPolicy( policy );


			// Added by Ramesh
				SecurityState.SetEncryptionPolicy( policy );

			}
			catch( ArgumentException ex )
			{
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( ex ) );
				}

				return;
			}

			// Set the new policies and refresh the view.
			try
			{
				web.SetSystemPolicy( policy );
			}
			catch ( Exception ex )
			{
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( GetString( "ERRORCANNOTSETSYSTEMPOLICY" ), ex ) );
				}
				web.Url = currentURL;
				return;
			}

			GetSystemPolicies();
			EnablePolicyButtons = false;
			web.Url = currentURL;
		}

		/// <summary>
		/// Event handler that gets called when the apply policy button for an ifolder user is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ApplyUserPolicy( Object sender, EventArgs e )
		{
			if(MasterServerUrl == null)
			{
				// But MasterServerUrl must not be null
				MasterServerUrl = currentURL ;
			}
			remoteweb.Url = MasterServerUrl + "/iFolderAdmin.asmx";

			// Get the current policy settings.
			UserPolicy policy = remoteweb.GetUserPolicy( PolicyID );
			try
			{
				AccountEnabled.SetAccountPolicy( policy );
				DiskQuota.SetDiskSpacePolicy( policy );
				FileSize.SetFileSizePolicy( policy );
				FileType.SetFileTypePolicy( policy );
				iFolderLimit.SetiFolderLimitPolicy( policy );
				SyncInterval.SetSyncPolicy( policy );
				SecurityState.SetEncryptionPolicy( policy );
				Sharing.SetSharingPolicy( policy, PolicyID);
			}
			catch( ArgumentException ex )
			{
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( ex ) );
				}
//				web.Url = currentURL;
				
				return;
			}

			// Set the new policies and refresh the view.
			try
			{
				remoteweb.SetUserPolicy( policy );
			}
			catch ( Exception ex )
			{
				string errMsg = String.Format( GetString( "ERRORCANNOTSETUSERPOLICY" ), PolicyID );
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( errMsg, ex ) );
				}
//				web.Url = currentURL;

				return;
			}

			GetUserPolicies();
			EnablePolicyButtons = false;
//			web.Url = currentURL;
		}

		/// <summary>
		/// Event handler that gets called when the cancel policy button for an ifolder is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void CanceliFolderPolicy( Object sender, EventArgs e )
		{
			// Set the policy fields back to their defaults.
			GetiFolderPolicies();
			EnablePolicyButtons = false;
		}

		/// <summary>
		/// Event handler that gets called when the cancel policy button for a system policy is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void CancelSystemPolicy( Object sender, EventArgs e )
		{
			// Set the policy fields back to their defaults.
			GetSystemPolicies();
			EnablePolicyButtons = false;
		}

		/// <summary>
		/// Event handler that gets called when the cancel policy button for a ifolder user is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void CancelUserPolicy( Object sender, EventArgs e )
		{
			// Set the policy fields back to their defaults.
			GetUserPolicies();
			EnablePolicyButtons = false;
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
		/// Event handler that gets called when any of the policies change.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void PolicyChanged( Object sender, EventArgs e )
		{
			EnablePolicyButtons = true;
		}
		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the ifolder policies.
		/// </summary>
		public void GetiFolderPolicies()
		{
			iFolderPolicy policy = null;
			string ifolderLocation = web.GetiFolderLocation (PolicyID);
			if(ifolderLocation != null)
			{
				UriBuilder remoteurl = new UriBuilder(ifolderLocation);
				remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
				remoteweb.Url = remoteurl.Uri.ToString();
			}
			try
			{
				policy = remoteweb.GetiFolderPolicy( PolicyID );
			}
			catch
			{
				return;
			}
			AccountEnabled.GetAccountPolicy( policy );
			iFolderEnabled.GetiFolderEnabledPolicy( policy );
			iFolderLimit.GetiFolderLimitPolicy( policy );
			DiskQuota.GetDiskSpacePolicy( policy );
			SecurityState.GetEncryptionPolicy( policy );
			FileSize.GetFileSizePolicy( policy );
			FileType.GetFileTypePolicy( policy );
			SyncInterval.GetSyncPolicy( policy );
			Sharing.GetSharingPolicy( policy, PolicyID );
//			web.Url = currentURL;
		}

		/// <summary>
		/// Gets the system policies.
		/// </summary>
		public void GetSystemPolicies()
		{
			SystemPolicy policy = web.GetSystemPolicy();
			AccountEnabled.GetAccountPolicy( policy );
			iFolderEnabled.GetiFolderEnabledPolicy( policy );
			iFolderLimit.GetiFolderLimitPolicy( policy );
			DiskQuota.GetDiskSpacePolicy( policy );
			SecurityState.GetEncryptionPolicy( policy );

			FileSize.GetFileSizePolicy( policy );
			FileType.GetFileTypePolicy( policy );
			SyncInterval.GetSyncPolicy( policy );
			Sharing.GetSharingPolicy( policy );
		}

		/// <summary>
		/// Gets the user policies.
		/// </summary>
		public bool GetUserPolicies()
		{
			UserPolicy policy = null;
			try
			{
				policy = web.GetUserPolicy( PolicyID );
			}
			catch
			{
				return false;
			}
			AccountEnabled.GetAccountPolicy( policy );
			iFolderEnabled.GetiFolderEnabledPolicy( policy );
			iFolderLimit.GetiFolderLimitPolicy( policy );
			DiskQuota.GetDiskSpacePolicy( policy );
			SecurityState.GetEncryptionPolicy( policy );

			FileSize.GetFileSizePolicy( policy );
			FileType.GetFileTypePolicy( policy );
			SyncInterval.GetSyncPolicy( policy );
			Sharing.GetSharingPolicy( policy );
//			web.Url = currentURL;
			return true;
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
			AccountEnabled.CheckChanged += new EventHandler( PolicyChanged );
			iFolderEnabled.CheckChanged += new EventHandler( PolicyChanged );
			DiskQuota.LimitChanged += new EventHandler( PolicyChanged );
			FileSize.LimitChanged += new EventHandler( PolicyChanged );
			FileType.ListChanged += new EventHandler( PolicyChanged );
			iFolderLimit.LimitChanged += new EventHandler( PolicyChanged );
			SyncInterval.IntervalChanged += new EventHandler( PolicyChanged );
			SecurityState.CheckChanged += new EventHandler( PolicyChanged );
			Sharing.CheckChanged += new EventHandler( PolicyChanged );
	
			// Set the policy button events to the proper event handlers.
			if ( IsUser )
			{
				PolicyApplyButton.Click += new EventHandler( ApplyUserPolicy );
				PolicyCancelButton.Click += new EventHandler( CancelUserPolicy );
			}
			else if ( IsiFolder )
			{
				PolicyApplyButton.Click += new EventHandler( ApplyiFolderPolicy );
				PolicyCancelButton.Click += new EventHandler( CanceliFolderPolicy );
			}
			else if ( IsSystem )
			{
				PolicyApplyButton.Click += new EventHandler( ApplySystemPolicy );
				PolicyCancelButton.Click += new EventHandler( CancelSystemPolicy );
			}

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}

	#region PolicyEventArgs

	/// <summary>
	/// Args for policies error events.
	/// </summary>
	public class PolicyErrorArgs : EventArgs
	{
		#region Class Members

		/// <summary>
		/// Error message that occurred during policy operation.
		/// </summary>
		private string errorMessage;

		/// <summary>
		/// Exception that occurred during policy operation.
		/// </summary>
		private Exception exception;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the error message.
		/// </summary>
		public string Message
		{
			get { return errorMessage; }
		}

		/// <summary>
		/// Get the exception.
		/// </summary>
		public Exception Exception
		{
			get { return exception; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="msg"></param>
		public PolicyErrorArgs( string msg )
		{
			errorMessage = msg;
			exception = null;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ex"></param>
		public PolicyErrorArgs( Exception ex )
		{
			errorMessage = null;
			exception = ex;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="ex"></param>
		public PolicyErrorArgs( string msg, Exception ex )
		{
			errorMessage = msg;
			exception = ex;
		}

		#endregion
	}

	#endregion
}
