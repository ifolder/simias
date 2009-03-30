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
	public class DisableSharing : System.Web.UI.UserControl
	{
		#region Class Members
		
		/// <summary>
		/// enum value to denote different combinations of Disabling options
		/// <summary>
		public enum DisableShare
		{	
			DisableSharing = 1,
			EnforcedDisableSharing = 4,
			EnableSharing = 8
		}
		/// <summary>
		/// enum to denote priority of entities
		/// </summary>
		public enum HigherPriority
		{
			System = 1,
			User = 2,
			iFolder = 3
		}

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Account DIV.
		/// </summary>
		protected HtmlGenericControl DisableSharingNav;

		/// <summary>
		/// Title of this policy
		/// </summary>
		protected Label DisableSharingTitle;

		/// <summary>
		/// check box to provide disabling options 
		/// </summary>
		protected CheckBox disableSharingOn;
		
		/// <summary>
		/// check box to provide disabling options 
		/// </summary>
		protected CheckBox enforcedDisableSharing;
		
		/// <summary>
		/// check box to provide disabling options 
		/// </summary>
		protected CheckBox disablePastSharing;
		
		/// <summary>
		/// web variable
		/// </summary>
		private iFolderAdmin web;

		/// <summary>
		/// Event that notifies consumer that the checkbox has changed.
		/// </summary>
		public event EventHandler CheckChanged = null;
		
		//private bool SharingOnChecked = false;
/*
		/// <summary>
		/// to keep track of encryption check box on page load
		/// </summary>
		protected bool EncryptionWasChecked;
	
		/// <summary>
		/// to keep track whether sharing was enforced for this particular user earlier
		/// </summary>
		protected bool SharingWasEnforced;
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
				DisableSharingTitle.Text = GetString("DISABLESHARING");
				disableSharingOn.Text = GetString("ON");
				enforcedDisableSharing.Text = GetString("ENFORCE");
				disablePastSharing.Text = GetString("DISABLEPASTSHARING");
				disableSharingOn.Checked = enforcedDisableSharing.Checked = false;
				
				
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the disable sharing check box is changed. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void disableSharingOnCheckChanged( Object sender, EventArgs e )
		{
			if( disableSharingOn.Checked == true )
			{
				enforcedDisableSharing.Enabled = true;
				
			}
			else
			{
				enforcedDisableSharing.Enabled = false;
			}

			if ( CheckChanged != null )
			{
				CheckChanged( sender, e );
			}
		
		}
		
		/// <summary>
		/// Event handler that gets called when the disablePastSharing checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void disablePastSharingCheckChanged( Object sender, EventArgs e )
		{
			/// currently do nothing...
			if ( CheckChanged != null )
			{
				CheckChanged( sender, e );
			}
		
		}
		
		/// <summary>
		/// Event handler that gets called when the enforcedDisableSharing checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void enforcedDisableSharingCheckChanged( Object sender, EventArgs e )
		{
			if( enforcedDisableSharing.Checked == true )
			{
				disableSharingOn.Enabled = false;
				
			}
			else
			{
				disableSharingOn.Enabled = true;
				//enforcedDisableSharing.Enabled = false;
			}
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
	/*
		/// <summary>
		/// If true then PolicyID is a user ID.
		/// </summary>
		private bool IsUser
		{
			get { return Request.Path.EndsWith( "UserDetails.aspx" ); }
		}
*/
		#endregion

		#region Public Methods

		/// <summary>
		/// Gets the Disable Sharing for current iFolder.
		/// </summary>
		/// <param name="policy">iFolder policy object</param>
		/*public void GetDisableSharingPolicy( iFolderPolicy policy )
		{
			/// write the code to obtain and display the policy for this ifolder
		}*/

		/// <summary>
		/// Gets the Disable Sharing policy for the system.
		/// </summary>
		/// <param name="policy">System policy object</param>
		public void GetDisableSharingPolicy( SystemPolicy policy )
		{
			int DisableStatus = policy.DisableSharingStatus;
			disableSharingOn.Checked = enforcedDisableSharing.Checked = disablePastSharing.Checked = false;
			disableSharingOn.Enabled = true;
			enforcedDisableSharing.Enabled = false;
			
			// check for Disable sharing and/or Enforce disable sharing
			if(DisableStatus == (int) DisableShare.DisableSharing )
			{
				disableSharingOn.Checked = true;
				enforcedDisableSharing.Enabled = true;
			}
			else 
			{
				// check for enforce disabling of sharing
			 
				if( (DisableStatus & (int) DisableShare.DisableSharing) == (int) DisableShare.DisableSharing)
				{ 
					disableSharingOn.Checked = true;
					enforcedDisableSharing.Enabled = true;
					if((DisableStatus & (int) DisableShare.EnforcedDisableSharing) == (int) DisableShare.EnforcedDisableSharing)
					{
						//disableSharingOn.Checked = true;
						disableSharingOn.Enabled = false;
						enforcedDisableSharing.Enabled = true;
						enforcedDisableSharing.Checked = true;	
					}
				}
			}
		}
		
		/// <summary>
		/// Gets the Disable Sharing policy for the current user.
		/// </summary>
		/// <param name="policy">User policy object</param>
		public void GetDisableSharingPolicy( UserPolicy policy )
		{
			disableSharingOn.Checked = enforcedDisableSharing.Checked = disablePastSharing.Checked = false;
			disableSharingOn.Enabled = true;
			enforcedDisableSharing.Enabled = false;
			
			// compare system policy also to decide
			SystemPolicy systemPolicy = web.GetSystemPolicy();
			int UserDisableStatus = policy.DisableSharingStatus;
			int SystemDisableStatus = systemPolicy.DisableSharingStatus;
			/// the function will return who has higher priority : system or user. 
			int DerivedStatus = DeriveStatus (SystemDisableStatus, UserDisableStatus);
			// If user modified the policy, then show it .
			if (UserDisableStatus != 0 )
			{
				if(UserDisableStatus == (int) DisableShare.DisableSharing )
				{
					disableSharingOn.Checked = true;
					enforcedDisableSharing.Enabled = true;
				}
				else
				{				
					if( (UserDisableStatus & (int)DisableShare.DisableSharing) == (int)DisableShare.DisableSharing)
					{
						disableSharingOn.Checked = true;
						enforcedDisableSharing.Enabled = true;
						if((UserDisableStatus &  (int)DisableShare.EnforcedDisableSharing) == (int) DisableShare.EnforcedDisableSharing)
						{
							//disableSharingOn.Checked = true;
							disableSharingOn.Enabled = false;
							//enforcedDisableSharing.Enabled = true;
							enforcedDisableSharing.Checked = true;	
						}
					}
				}
			}	
			if (DerivedStatus == (int) HigherPriority.System)
			{
				// System has either enforced the policy or there is no user level policy
				// No policy on user level is also equal to (disable only past sharing)
				if(UserDisableStatus == 0 )
				{
					// if there is no user level policy defined, then copy it from system level
					if(SystemDisableStatus == (int) DisableShare.DisableSharing )
					{
						disableSharingOn.Checked = true;
						enforcedDisableSharing.Enabled = true;
					}
					else
					{
						if( (SystemDisableStatus & (int)DisableShare.DisableSharing) == (int)DisableShare.DisableSharing)
						{
							disableSharingOn.Checked = true;
							enforcedDisableSharing.Enabled = true;
							if((SystemDisableStatus &  (int)DisableShare.EnforcedDisableSharing) == (int) DisableShare.EnforcedDisableSharing)
							{	
								//system has higher priority , so disable the check boxes
								disableSharingOn.Enabled = enforcedDisableSharing.Enabled = false;
							}
						}
					}
				}
				else
				{
					//User modified the policy, but system has enforced the priority , so disable the check boxes
					disableSharingOn.Enabled = enforcedDisableSharing.Enabled = false;
				}	
			}
			
		}	
		
		/// <summary>
		/// Gets the Disable Sharing policy for the current iFolder.
		/// </summary>
		/// <param name="policy">User policy object</param>
		public void GetDisableSharingPolicy( iFolderPolicy policy ,string PolicyID)
		{
			// compare system policy, and owner policy also to decide
			//to get owner ID
			iFolder ifolder = web.GetiFolder(PolicyID);
			string OwnerID = ifolder.OwnerID;
			SystemPolicy systemPolicy = web.GetSystemPolicy();
			UserPolicy userPolicy = web.GetUserPolicy(OwnerID);  
			int iFolderDisableStatus = policy.DisableSharingStatus;
			int UserDisableStatus = userPolicy.DisableSharingStatus;
			int SystemDisableStatus = systemPolicy.DisableSharingStatus;
			/// the function will return who has higher priority : system or user or iFolder. 
			int DerivedStatus = DeriveStatus (SystemDisableStatus, UserDisableStatus, iFolderDisableStatus);
			/// If policy was modified on iFolder level, then show it.
			if (iFolderDisableStatus != 0 )
			{
				if(iFolderDisableStatus == (int) DisableShare.DisableSharing )
				{
					disableSharingOn.Checked = true;
					enforcedDisableSharing.Enabled = true;
				}
				else
				{				
					if( (iFolderDisableStatus & (int)DisableShare.DisableSharing) == (int)DisableShare.DisableSharing)
					{
						disableSharingOn.Checked = true;
						enforcedDisableSharing.Enabled = true;
						if((iFolderDisableStatus &  (int)DisableShare.EnforcedDisableSharing) == (int) DisableShare.EnforcedDisableSharing)
						{
							//disableSharingOn.Checked = true;
							disableSharingOn.Enabled = false;
							//enforcedDisableSharing.Enabled = true;
							enforcedDisableSharing.Checked = true;	
						}
					}
				}	
			}	
			// Either there was no iFolder level policy, or user level has enforced its policy.
			if (DerivedStatus == (int) HigherPriority.User)
			{
				if(iFolderDisableStatus == 0 )
				{ 
					/// There was not any iFolder level policy but user level policy is there so show that.
					if(UserDisableStatus == (int) DisableShare.DisableSharing )
					{
						disableSharingOn.Checked = true;
						enforcedDisableSharing.Enabled = true;
					}
					else
					{
							if( (UserDisableStatus & (int)DisableShare.DisableSharing) == (int)DisableShare.DisableSharing)
							{
								disableSharingOn.Checked = true;
								enforcedDisableSharing.Enabled = true;
								if((UserDisableStatus &  (int)DisableShare.EnforcedDisableSharing) == (int) DisableShare.EnforcedDisableSharing)
								{
									//disableSharingOn.Checked = true;
									//enforcedDisableSharing.Checked = true;	
									//system has higher priority , so disable the check boxes
									disableSharingOn.Enabled = enforcedDisableSharing.Enabled = false;
								}
							}
					}
				}
				else
				{
					//user or system has higher priority , disable the check boxes
					disableSharingOn.Enabled = enforcedDisableSharing.Enabled = false;
				}	
			}
			
			if (DerivedStatus == (int) HigherPriority.System)
			{
				if(iFolderDisableStatus == 0 )
				{ 
					/// There was not any iFolder level policy but system level policy is there so show that.
					if(SystemDisableStatus == (int) DisableShare.DisableSharing )
					{
						disableSharingOn.Checked = true;
						enforcedDisableSharing.Enabled = true;
					}
					else
					{
							if( (SystemDisableStatus & (int)DisableShare.DisableSharing) == (int)DisableShare.DisableSharing)
							{
								disableSharingOn.Checked = true;
								enforcedDisableSharing.Enabled = true;
								if((SystemDisableStatus &  (int)DisableShare.EnforcedDisableSharing) == (int) DisableShare.EnforcedDisableSharing)
								{
									//disableSharingOn.Checked = true;
									//enforcedDisableSharing.Checked = true;	
									//system has higher priority , so disable the check boxes
									disableSharingOn.Enabled = enforcedDisableSharing.Enabled = false;
								}
							}
					}
				}
				else
				{
					/// iFolder level policy was modified, but system has higher priority , disable the check boxes
					disableSharingOn.Enabled = enforcedDisableSharing.Enabled = false;
				}	
			}
			
			// There is no requirement of enforce check box on iFolder level, so make it invisible
			enforcedDisableSharing.Visible = false;
		}
		
		
		///<summary>
        /// Get the policy for an iFolder.
        /// </summary>
        /// <param name="policy">The iFolderPolicy object.</param>
		private int DeriveStatus(int system, int user)
		{
            //check whether there is any enforcement of policy on system level
            bool SystemEnforcement = ( (system & (int)DisableShare.EnforcedDisableSharing) == (int)DisableShare.EnforcedDisableSharing );
            if ( SystemEnforcement)
            {
            	/// although next check is redundant , i am putting ... should be checked and removed
            	if(system != 0 )
            		return (int)HigherPriority.System;
            	return (int)HigherPriority.User;
            }
            else
            {
            	if(user != 0 )
            		return (int)HigherPriority.User;
            	return (int)HigherPriority.System;
            }
		}

		///<summary>
        /// Get the policy for an iFolder.
        /// </summary>
        /// <param name="policy">The iFolderPolicy object.</param>
		private int DeriveStatus(int system, int user, int iFolder)
		{
			
			//check for enforcement of policy on system, or user level
			bool SystemEnforcement = ( (system & (int)DisableShare.EnforcedDisableSharing) == (int)DisableShare.EnforcedDisableSharing );
			bool UserEnforcement = ( (user & (int)DisableShare.EnforcedDisableSharing) == (int)DisableShare.EnforcedDisableSharing );
			
			if( SystemEnforcement)
            {
            	// redundant check, should be removed
            	if(system != 0)
            		return (int)HigherPriority.System;
            	return (int)HigherPriority.iFolder;	
            }
            if( UserEnforcement)
            {
            	// redundant check, should be removed 
            	if(user != 0)
            		return (int)HigherPriority.User;
            	return (int)HigherPriority.iFolder;
            }
            else
            {
            	if(iFolder != 0 )
            		return (int)HigherPriority.iFolder;	
            	if(user != 0 )
            		return (int)HigherPriority.User;
            	return (int)HigherPriority.System;	
            }
		}

		/// <summary>
		/// Sets the policy for the system.
		/// </summary>
		/// <param name="policy">System policy where the account information will be set.</param>
		public void SetDisableSharingPolicy( SystemPolicy policy )
		{
			int DisableSharingStatus=0;
			if( disableSharingOn.Checked)
				DisableSharingStatus += (int) DisableShare.DisableSharing; //1;
			if(enforcedDisableSharing.Checked)
				DisableSharingStatus += (int) DisableShare.EnforcedDisableSharing; //4;
			
			policy.DisableSharingStatus = DisableSharingStatus;
			
			/// Next change is to remove the past sharing ,  
			if( disablePastSharing.Checked )
			{
				
				web.DisableSystemPastSharing() ;
				disablePastSharing.Checked = false;
			}	
		}
		
		/// <summary>
		/// Sets the  policy for the user.
		/// </summary>
		/// <param name="policy">User policy where the information will be set.</param>
		public void SetDisableSharingPolicy( UserPolicy policy, string PolicyID)
		{
			int DisableSharingStatus=0;
			int UserDisableStatus = policy.DisableSharingStatus;
			SystemPolicy systemPolicy = web.GetSystemPolicy();
			int SystemDisableStatus = systemPolicy.DisableSharingStatus;
	
			
			if (( (UserDisableStatus & (int) DisableShare.EnableSharing) == (int) DisableShare.EnableSharing)  && disableSharingOn.Checked == false )
			{
				/// it means for this user , admin has enable the sharing on UserDetails page, so retain that
				DisableSharingStatus += (int) DisableShare.EnableSharing; //8
			}
			/// if for this user disable sharing was "on" (either on user level or system level) and now admin has unchecked the checkbox then also store enable sharing -- 8
			else if( ( ( (SystemDisableStatus & (int) DisableShare.DisableSharing) == (int)DisableShare.DisableSharing) || ((UserDisableStatus & (int) DisableShare.DisableSharing) == (int)DisableShare.DisableSharing)) && disableSharingOn.Checked == false)
			{
				DisableSharingStatus += (int) DisableShare.EnableSharing; //8
			}
			
			if( disableSharingOn.Checked && UserDisableStatus != 0)
			{ 
				/// add this value only if on user level some policy was set
				DisableSharingStatus += (int)DisableShare.DisableSharing; //1;
			}
			else if( disableSharingOn.Checked && (SystemDisableStatus == 0))
			{
				DisableSharingStatus += (int)DisableShare.DisableSharing; //1;
			}
			
			if(enforcedDisableSharing.Checked)
				DisableSharingStatus += (int)DisableShare.EnforcedDisableSharing; //4;
				
			policy.DisableSharingStatus = DisableSharingStatus;
			
			/// Next change is to remove the past sharing  
			if( disablePastSharing.Checked )
			{
				/// pass the userID
				web.DisableUserPastSharing(PolicyID) ;
				disablePastSharing.Checked = false;
			}	
		}

		/// <summary>
		/// Sets the policy for the user.
		/// </summary>
		/// <param name="policy">iFolder policy where the  information will be set.</param>
		public void SetDisableSharingPolicy( iFolderPolicy policy, string PolicyID )
		{
			int DisableSharingStatus=0;
			iFolder ifolder = web.GetiFolder(PolicyID);
			string OwnerID = ifolder.OwnerID;
			SystemPolicy systemPolicy = web.GetSystemPolicy();
			UserPolicy userPolicy = web.GetUserPolicy(OwnerID);
			int iFolderDisableStatus = policy.DisableSharingStatus;
			int UserDisableStatus = userPolicy.DisableSharingStatus;
			int SystemDisableStatus = systemPolicy.DisableSharingStatus;
			if (( (iFolderDisableStatus & (int) DisableShare.EnableSharing) == (int) DisableShare.EnableSharing)  && disableSharingOn.Checked == false)
			{
				/// it means for this iFolder , admin had earlier enabled the sharing on iFolderDetails page, so retain that
				DisableSharingStatus += (int) DisableShare.EnableSharing; //8
			}
			else if( ( ((SystemDisableStatus & (int) DisableShare.DisableSharing) == (int)DisableShare.DisableSharing) || ((UserDisableStatus & (int) DisableShare.DisableSharing) == (int)DisableShare.DisableSharing) || ((iFolderDisableStatus & (int) DisableShare.DisableSharing) == (int)DisableShare.DisableSharing)) && disableSharingOn.Checked == false )
			{
				/// if for this iFolder , disable sharing was "on" (either iFolder, user or system level) , and now admin has unchecked the box , then also store enable sharing -- 8
				DisableSharingStatus += (int) DisableShare.EnableSharing; //8
			}
			if( disableSharingOn.Checked && iFolderDisableStatus != 0)
			{
				/// consider the case when no disable of sharing on iFolder level but disable past sharing is applied. 
				DisableSharingStatus += (int)DisableShare.DisableSharing; //1;
			}	
			else if( disableSharingOn.Checked && (UserDisableStatus == 0 || ( (UserDisableStatus & (int) DisableShare.EnableSharing) == (int) DisableShare.EnableSharing)))
			{
				/// if on user level, it was enabled and then if on iFolder level it is disabled , then for that case
				DisableSharingStatus += (int)DisableShare.DisableSharing; //1;
			}	
				
			policy.DisableSharingStatus = DisableSharingStatus;
			
			/// Next change is to remove the past sharing ,
			if( disablePastSharing.Checked )
			{
				/// pass the iFolderID
				web.DisableiFolderPastSharing(PolicyID) ;
				disablePastSharing.Checked = false;
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
		
			disableSharingOn.CheckedChanged += new EventHandler( disableSharingOnCheckChanged );
			disablePastSharing.CheckedChanged += new EventHandler( disablePastSharingCheckChanged);
			enforcedDisableSharing.CheckedChanged += new EventHandler( enforcedDisableSharingCheckChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
