/***********************************************************************
 *  $RCSfile: Policy.ascx.cs,v $
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
	///		Summary description for Policy.
	/// </summary>
	public class Policy : System.Web.UI.UserControl
	{
		#region Class Members

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// Account enabled control.
		/// </summary>
		protected AccountEnabled AccountEnabled;

		/// <summary>
		/// iFolder enabled control.
		/// </summary>
		protected iFolderEnabled iFolderEnabled;

		/// <summary>
		/// Disk quota control.
		/// </summary>
		protected DiskSpaceQuota DiskQuota;

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
		/// Policy buttons that submit or cancel policy changes.
		/// </summary>
		protected Button PolicyApplyButton;

		/// <summary>
		/// Policy buttons that submit or cancel policy changes.
		/// </summary>
		protected Button PolicyCancelButton;


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
			// Get the current policy settings.
			iFolderPolicy policy = null;
			try
			{
				policy = web.GetiFolderPolicy( PolicyID );
			}
			catch ( Exception ex )
			{
				string errMsg = String.Format( GetString( "ERRORCANNOTGETIFOLDERPOLICY" ), PolicyID );
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( errMsg, ex ) );
				}

				return;
			}

			// Verify and apply all the ifolder specified settings to the policy object.
			iFolderEnabled.SetiFolderEnabledPolicy( policy );
			DiskQuota.SetDiskSpacePolicy( policy );
			FileSize.SetFileSizePolicy( policy );
			FileType.SetFileTypePolicy( policy );
			SyncInterval.SetSyncPolicy( policy );

			// Set the new policies and refresh the view.
			try
			{
				web.SetiFolderPolicy( policy );
			}
			catch ( Exception ex )
			{
				string errMsg = String.Format( GetString( "ERRORCANNOTSETIFOLDERPOLICY" ), PolicyID );
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( errMsg, ex ) );
				}

				return;
			}

			GetiFolderPolicies();
			EnablePolicyButtons = false;
		}

		/// <summary>
		/// Event handler that gets called when the apply policy button for an system policy is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ApplySystemPolicy( Object sender, EventArgs e )
		{
			SystemPolicy policy = null;

			try
			{
				policy = web.GetSystemPolicy();
			}
			catch ( Exception ex )
			{
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( GetString( "ERRORCANNOTGETSYSTEMPOLICY" ), ex ) );
				}

				return;
			}

			// Verify and apply all the user specified settings to the policy object.
			DiskQuota.SetDiskSpacePolicy( policy );
			FileSize.SetFileSizePolicy( policy );
			FileType.SetFileTypePolicy( policy );
			SyncInterval.SetSyncPolicy( policy );

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

				return;
			}

			GetSystemPolicies();
			EnablePolicyButtons = false;
		}

		/// <summary>
		/// Event handler that gets called when the apply policy button for an ifolder user is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void ApplyUserPolicy( Object sender, EventArgs e )
		{
			// Get the current policy settings.
			UserPolicy policy = null;
			try
			{
				policy = web.GetUserPolicy( PolicyID );
			}
			catch ( Exception ex )
			{
				string errMsg = String.Format( GetString( "ERRORCANNOTGETUSERPOLICY" ), PolicyID );
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( errMsg, ex ) );
				}

				return;
			}

			// Verify and apply all the user specified settings to the policy object.
			AccountEnabled.SetAccountPolicy( policy );
			DiskQuota.SetDiskSpacePolicy( policy );
			FileSize.SetFileSizePolicy( policy );
			FileType.SetFileTypePolicy( policy );
			SyncInterval.SetSyncPolicy( policy );

			// Set the new policies and refresh the view.
			try
			{
				web.SetUserPolicy( policy );
			}
			catch ( Exception ex )
			{
				string errMsg = String.Format( GetString( "ERRORCANNOTSETUSERPOLICY" ), PolicyID );
				if ( PolicyError != null )
				{
					PolicyError( this, new PolicyErrorArgs( errMsg, ex ) );
				}

				return;
			}

			GetUserPolicies();
			EnablePolicyButtons = false;
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
			try
			{
				policy = web.GetiFolderPolicy( PolicyID );
			}
			catch ( Exception ex )
			{
				string errMsg = String.Format( GetString( "ERRORCANNOTGETIFOLDERPOLICY" ), PolicyID );
				Response.Redirect( String.Format( "Error.aspx?ex={0} {1}", errMsg, Utils.ExceptionMessage( ex ) ), true );
				return;
			}

			AccountEnabled.GetAccountPolicy( policy );
			iFolderEnabled.GetiFolderEnabledPolicy( policy );
			DiskQuota.GetDiskSpacePolicy( policy );
			FileSize.GetFileSizePolicy( policy );
			FileType.GetFileTypePolicy( policy );
			SyncInterval.GetSyncPolicy( policy );
		}

		/// <summary>
		/// Gets the system policies.
		/// </summary>
		public void GetSystemPolicies()
		{
			SystemPolicy policy = null;
			try
			{
				policy = web.GetSystemPolicy();
			}
			catch ( Exception ex )
			{
				string errMsg = GetString( "ERRORCANNOTGETSYSTEMPOLICY" );
				Response.Redirect( String.Format( "Error.aspx?ex={0} {1}", errMsg, Utils.ExceptionMessage( ex ) ), true );
				return;
			}

			AccountEnabled.GetAccountPolicy( policy );
			iFolderEnabled.GetiFolderEnabledPolicy( policy );
			DiskQuota.GetDiskSpacePolicy( policy );
			FileSize.GetFileSizePolicy( policy );
			FileType.GetFileTypePolicy( policy );
			SyncInterval.GetSyncPolicy( policy );
		}

		/// <summary>
		/// Gets the user policies.
		/// </summary>
		public void GetUserPolicies()
		{
			UserPolicy policy = null;
			try
			{
				policy = web.GetUserPolicy( PolicyID );
			}
			catch ( Exception ex )
			{
				string errMsg = String.Format( GetString( "ERRORCANNOTGETUSERPOLICY" ), PolicyID );
				Response.Redirect( String.Format( "Error.aspx?ex={0} {1}", errMsg, Utils.ExceptionMessage( ex ) ), true );
				return;
			}

			AccountEnabled.GetAccountPolicy( policy );
			iFolderEnabled.GetiFolderEnabledPolicy( policy );
			DiskQuota.GetDiskSpacePolicy( policy );
			FileSize.GetFileSizePolicy( policy );
			FileType.GetFileTypePolicy( policy );
			SyncInterval.GetSyncPolicy( policy );
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
			SyncInterval.IntervalChanged += new EventHandler( PolicyChanged );

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
