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
*                 $Author : Ravi Kumar <rkumar@novell.com>
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
	public class Sharing : System.Web.UI.UserControl
	{
		#region Class Members
		
		/// <summary>
		/// enum value to denote different combinations of Disabling/Enabling options
		/// <summary>
		public enum Share
		{	
			Sharing = 1,
			EnforcedSharing = 4,
			DisableSharing = 8
		}
		
        /// <summary>
        /// enum to denote priority levels
        /// </summary>
		public enum HigherPriority
		{
			System = 1,
			User = 2,
			iFolder = 3,
			Group = 4
		}

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Account DIV.
		/// </summary>
		protected HtmlGenericControl SharingNav;
		
		        /// <summary>
                /// iFolder Connection to the remote server
                /// </summary>
                private iFolderAdmin remoteweb;

		        /// <summary>
                /// Incoming URL.
                /// </summary>
                protected string currentURL;

		/// <summary>
		/// Title of this policy
		/// </summary>
		protected Label SharingTitle;

		/// <summary>
		/// check box to provide enabling / disabling options 
		/// </summary>
		protected CheckBox SharingOn;
		
		/// <summary>
		/// check box to provide enabling / disabling options 
		/// </summary>
		protected CheckBox enforcedSharing;
		
		/// <summary>
		/// check box to provide disabling/enabling options 
		/// </summary>
		protected CheckBox disablePastSharing;
		
		/// <summary>
		/// web variable
		/// </summary>
		private iFolderAdmin web;
		
		        /// <summary>
                ///  variable to check if changes were made
                /// </summary>
		public bool valuechanged;

		//private bool SharingOnChecked = false;

		/// <summary>
		/// to keep track of encryption check box on page load
		/// </summary>
		protected bool ValueChanged;
/*	
		/// <summary>
		/// to keep track whether sharing was enforced for this particular user earlier
		/// </summary>
		protected bool SharingWasEnforced;
*/
		        /// <summary>
                /// Event that notifies consumer that the checkbox has changed.
                /// </summary>
                public event EventHandler CheckChanged = null;
		
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
			
			currentURL = web.Url;
                        remoteweb = new iFolderAdmin();
                        remoteweb.PreAuthenticate = true;
                        remoteweb.Credentials = web.Credentials;
                        remoteweb.Url = web.Url;

			if ( !IsPostBack )
			{
				SharingTitle.Text = GetString("SHARING");
				SharingOn.Text = GetString("ON");
				enforcedSharing.Text = GetString("ENFORCED");
				disablePastSharing.Text = GetString("DISABLEPASTSHARING");
				SharingOn.Checked = enforcedSharing.Checked = false;
				disablePastSharing.Checked =  false;
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Event handler that gets called when the sharing check box is changed. 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void SharingOnCheckChanged( Object sender, EventArgs e )
		{
			Session["ValueChanged"] = "true";
			if( SharingOn.Checked == true )
			{
				enforcedSharing.Enabled = true;
				disablePastSharing.Enabled = false;	
			}
			else
			{
				enforcedSharing.Enabled = true;
				disablePastSharing.Enabled = true;
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
			Session["ValueChanged"] = "true";
			if ( CheckChanged != null )
			{
				CheckChanged( sender, e );
			}
		
		}
		
		/// <summary>
		/// Event handler that gets called when the enforcedSharing checkbox is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void enforcedSharingCheckChanged( Object sender, EventArgs e )
		{
			Session["ValueChanged"] = "true";
			if( enforcedSharing.Checked == true )
			{
				SharingOn.Enabled = false;
			}
			else
			{
				SharingOn.Enabled = true;
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
		/// Gets the Sharing for current iFolder.
		/// </summary>
		/// <param name="policy">iFolder policy object</param>
		/*public void GetSharingPolicy( iFolderPolicy policy )
		{
			/// write the code to obtain and display the policy for this ifolder
		}*/

		/// <summary>
		/// Gets the Sharing policy for the system.
		/// </summary>
		/// <param name="policy">System policy object</param>
		public void GetSharingPolicy( SystemPolicy policy )
		{
			int Status = policy.SharingStatus;
			SharingOn.Checked = enforcedSharing.Checked = false;
			disablePastSharing.Checked = false;
			disablePastSharing.Enabled =false;
			SharingOn.Enabled = true;
			
			// check for Disable sharing and/or Enforce disable sharing
			if( Status == 0)
			{
				SharingOn.Checked = true;
				enforcedSharing.Enabled = true;
				disablePastSharing.Enabled = false;
			}
			else 
			{
				// check for enforce disabling of sharing
			 
				if( (Status & (int) Share.Sharing) == (int) Share.Sharing)
				{ 
					SharingOn.Checked = true;
					enforcedSharing.Enabled = true;
					disablePastSharing.Enabled = false;
					if((Status & (int) Share.EnforcedSharing) == (int) Share.EnforcedSharing)
					{
						//disableSharingOn.Checked = true;
						SharingOn.Enabled = false;
						enforcedSharing.Enabled = true;
						enforcedSharing.Checked = true;	
					}
				}
				else if( (Status & (int) Share.DisableSharing) == (int) Share.DisableSharing)
				{
					SharingOn.Checked = false;
                                        enforcedSharing.Enabled = true;
                                        disablePastSharing.Enabled = true;
                                        if((Status & (int) Share.EnforcedSharing) == (int) Share.EnforcedSharing)
                                        {
                                                //disableSharingOn.Checked = true;
                                                SharingOn.Enabled = false;
                                                enforcedSharing.Enabled = true;
                                                enforcedSharing.Checked = true;
                                        }

				}
			}
		}
		
		/// <summary>
		/// Gets the  Sharing policy for the current user.
		/// </summary>
		/// <param name="policy">User policy object</param>
		public void GetSharingPolicy( UserPolicy policy )
		{
			SharingOn.Checked = enforcedSharing.Checked = disablePastSharing.Checked = false;
			SharingOn.Enabled = true;
			
			// compare system policy also to decide
			SystemPolicy systemPolicy = web.GetSystemPolicy();
			int UserStatus = policy.SharingStatus;
			int GroupStatus = web.GetGroupSharingPolicy(policy.UserID);
			int SystemStatus = systemPolicy.SharingStatus;
			/// the function will return who has higher priority : system or user. 
			int DerivedStatus = DeriveStatus(SystemStatus, GroupStatus ,UserStatus);
			// If user modified the policy, then show it .

			Session["ValueChanged"] = "false";
			if( UserStatus == 0 )
			{
				SharingOn.Checked = true;
				enforcedSharing.Enabled = true;
				disablePastSharing.Enabled = false;
				if( GroupStatus != 0 )
				{
					if((GroupStatus & (int) Share.Sharing) == (int)Share.Sharing || GroupStatus == 0 )
					{
						SharingOn.Checked = true;
		                                enforcedSharing.Enabled = true;
        		                        disablePastSharing.Enabled = false;	
					}
					else 
					{
						SharingOn.Checked = false;
		                                enforcedSharing.Enabled = true;
        		                        disablePastSharing.Enabled = true;
					}
				}
				else
				{
					if((SystemStatus & (int) Share.Sharing) == (int)Share.Sharing || SystemStatus == 0)
					{
						SharingOn.Checked = true;
	                                        enforcedSharing.Enabled = true;
        	                                disablePastSharing.Enabled = false;

					}
					else 
					{
						SharingOn.Checked = false;
	                                        enforcedSharing.Enabled = true;
        	                                disablePastSharing.Enabled = true;
					}
				}
				
			}
			else
			{				
				if( (UserStatus & (int)Share.Sharing) == (int)Share.Sharing)
				{
					SharingOn.Checked = true;
					enforcedSharing.Enabled = true;
					disablePastSharing.Enabled = false;
					if((UserStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
					{
						//disableSharingOn.Checked = true;
						SharingOn.Enabled = false;
						//enforcedDisableSharing.Enabled = true;
						enforcedSharing.Checked = true;	
					}
				}
				else if( (UserStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                                {
                                        SharingOn.Checked = false;
                                        enforcedSharing.Enabled = true;
                                        disablePastSharing.Enabled = true;
                                        if((UserStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
                                        {
                                                //disableSharingOn.Checked = true;
                                                SharingOn.Enabled = false;
                                                //enforcedDisableSharing.Enabled = true;
                                                enforcedSharing.Checked = true;
                                        }
                                }

			}
			if (DerivedStatus == (int) HigherPriority.Group)
			{
				// Group has either enforced the policy or there is no user level policy
				// No policy on user level is also equal to (disable only past sharing)
				// if there is no user level policy defined, then copy it from group level
				if(GroupStatus == (int) Share.Sharing || GroupStatus == 0)
				{
					SharingOn.Checked = true;
					enforcedSharing.Enabled = true;
					disablePastSharing.Enabled = false;
				}
				else
				{
					if( (GroupStatus & (int)Share.Sharing) == (int)Share.Sharing)
					{
						SharingOn.Checked = true;
						enforcedSharing.Enabled = true;
						disablePastSharing.Enabled = false;
						if((GroupStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
						{	
							//system has higher priority , so disable the check boxes
							SharingOn.Enabled = enforcedSharing.Enabled = false;
						}
					}
					else if( (GroupStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                                        {
                                                SharingOn.Checked = false;
                                                enforcedSharing.Enabled = true;
                                                disablePastSharing.Enabled = true;
                                                if((GroupStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
                                                {
  	                                        	//system has higher priority , so disable the check boxes
                                                        SharingOn.Enabled = enforcedSharing.Enabled = false;
                                                }
                                        }

				}
			}
			if (DerivedStatus == (int) HigherPriority.System)
			{
				// System has either enforced the policy or there is no user, group level policy
				// No policy on user level is also equal to (disable only past sharing)
				// if there is no user level policy defined, then copy it from system level
				if(SystemStatus == (int) Share.Sharing || SystemStatus == 0)
				{
					SharingOn.Checked = true;
					enforcedSharing.Enabled = true;
					disablePastSharing.Enabled = false;
				}
				else
				{
					if( (SystemStatus & (int)Share.Sharing) == (int)Share.Sharing)
					{
						SharingOn.Checked = true;
						enforcedSharing.Enabled = true;
						disablePastSharing.Enabled = false;
						if((SystemStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
						{	
							//system has higher priority , so disable the check boxes
							SharingOn.Enabled = enforcedSharing.Enabled = false;
						}
					}
					else if( (SystemStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                                        {
                                                SharingOn.Checked = false;
                                                enforcedSharing.Enabled = true;
                                                disablePastSharing.Enabled = true;
                                                if((SystemStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
                                                {
  	                                        	//system has higher priority , so disable the check boxes
                                                        SharingOn.Enabled = enforcedSharing.Enabled = false;
                                                }
                                        }

				}
			}
		}
	
		/// <summary>
		/// Gets the Sharing policy for the current iFolder.
		/// </summary>
		/// <param name="policy">User policy object</param>
		public void GetSharingPolicy( iFolderPolicy policy ,string PolicyID)
		{
			// compare system policy, and owner policy also to decide
			//to get owner ID
			iFolder ifolder = web.GetiFolder(PolicyID);
			string OwnerID = ifolder.OwnerID;
			SystemPolicy systemPolicy = web.GetSystemPolicy();
			UserPolicy userPolicy = web.GetUserPolicy(OwnerID, null);  
			int iFolderStatus = policy.SharingStatus;
			int UserStatus = userPolicy.SharingStatus;
			int SystemStatus = systemPolicy.SharingStatus;
			int GroupStatus = web.GetGroupSharingPolicy(OwnerID);
			/// the function will return who has higher priority : system or user or iFolder. 
			int DerivedStatus = DeriveStatus (SystemStatus, GroupStatus, UserStatus, iFolderStatus);
			/// If policy was modified on iFolder level, then show it.

			Session["ValueChanged"] = "false";
			if(iFolderStatus == 0)
			{
				SharingOn.Checked = true;
				enforcedSharing.Enabled = true;
				disablePastSharing.Enabled = false;
				if( UserStatus != 0 )
				{
					if((UserStatus & (int) Share.Sharing) == (int)Share.Sharing || UserStatus == 0)
        	                        {
                	                        SharingOn.Checked = true;
                        	                disablePastSharing.Enabled = false;
                                	}
                                	else 
                                	{
                                        	SharingOn.Checked = false;
	                                        disablePastSharing.Enabled = true;
        	                        }
				}
				else if( GroupStatus != 0 )
				{
					if((GroupStatus & (int) Share.Sharing) == (int)Share.Sharing || GroupStatus == 0)
                                	{
                                        	SharingOn.Checked = true;
	                                        disablePastSharing.Enabled = false;
        	                        }
                	                else 
                                	{
                                        	SharingOn.Checked = false;
	                                        disablePastSharing.Enabled = true;
        	                        }
				}
				else
				{
                                	if((SystemStatus & (int) Share.Sharing) == (int)Share.Sharing || SystemStatus == 0)
	                                {
        	                                SharingOn.Checked = true;
                	                        disablePastSharing.Enabled = false;
	                                }
        	                        else 
                	                {
                        	                SharingOn.Checked = false;
                                	        disablePastSharing.Enabled = true;

                                	}
				}
			}
			else
			{				
				if( (iFolderStatus & (int)Share.Sharing) == (int)Share.Sharing)
				{
					SharingOn.Checked = true;
					enforcedSharing.Enabled = true;
					disablePastSharing.Enabled = false;
					if((iFolderStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
					{
						//disableSharingOn.Checked = true;
						SharingOn.Enabled = false;
						//enforcedDisableSharing.Enabled = true;
						enforcedSharing.Checked = true;	
					}
				}
				else if ( (iFolderStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                                {
                                        SharingOn.Checked = false;
                                        enforcedSharing.Enabled = true;
                                        disablePastSharing.Enabled = true;
                                        if((iFolderStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
                                        {
                                                SharingOn.Enabled = false;
                                                //enforcedDisableSharing.Enabled = true;
                                                enforcedSharing.Checked = true;
                                        }
                                }

			}	
			// Either there was no iFolder level policy, or user level has enforced its policy.
			if (DerivedStatus == (int) HigherPriority.User)
			{
					/// There was not any iFolder level policy but user level policy is there so show that.
					if(UserStatus == (int) Share.Sharing || UserStatus == 0)
					{
						SharingOn.Checked = true;
						enforcedSharing.Enabled = true;
						disablePastSharing.Enabled = false;
					}
					else
					{
							if( (UserStatus & (int)Share.Sharing) == (int)Share.Sharing)
							{
								SharingOn.Checked = true;
								enforcedSharing.Enabled = true;
								disablePastSharing.Enabled = false;
								if((UserStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
								{
									//disableSharingOn.Checked = true;
									//enforcedDisableSharing.Checked = true;	
									//system has higher priority , so disable the check boxes
									SharingOn.Enabled = enforcedSharing.Enabled = false;
								}
							}
							else if( (UserStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                                                        {
                                                                SharingOn.Checked = false;
                                                                enforcedSharing.Enabled = true;
                                                                disablePastSharing.Enabled = true;
                                                                if((UserStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
                                                                {
                                                                        //disableSharingOn.Checked = true;
                                                                        //enforcedDisableSharing.Checked = true;
                                                                        //system has higher priority , so disable the check boxes
                                                                        SharingOn.Enabled = enforcedSharing.Enabled = false;
                                                                }
                                                        }

					}
			}

			// Either there was no iFolder level policy, or Group level has enforced its policy.
			if (DerivedStatus == (int) HigherPriority.Group)
			{
					/// There was not any iFolder level policy but group level policy is there so show that.
					if(GroupStatus == (int) Share.Sharing || UserStatus == 0)
					{
						SharingOn.Checked = true;
						enforcedSharing.Enabled = true;
						disablePastSharing.Enabled = false;
					}
					else
					{
							if( (GroupStatus & (int)Share.Sharing) == (int)Share.Sharing)
							{
								SharingOn.Checked = true;
								enforcedSharing.Enabled = true;
								disablePastSharing.Enabled = false;
								if((GroupStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
								{
									//disableSharingOn.Checked = true;
									//enforcedDisableSharing.Checked = true;	
									//Group has higher priority , so disable the check boxes
									SharingOn.Enabled = enforcedSharing.Enabled = false;
								}
							}
							else if( (GroupStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                                                        {
                                                                SharingOn.Checked = false;
                                                                enforcedSharing.Enabled = true;
                                                                disablePastSharing.Enabled = true;
                                                                if((GroupStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
                                                                {
                                                                        //disableSharingOn.Checked = true;
                                                                        //enforcedDisableSharing.Checked = true;
                                                                        //group has higher priority , so disable the check boxes
                                                                        SharingOn.Enabled = enforcedSharing.Enabled = false;
                                                                }
                                                        }

					}
			}
			
			if (DerivedStatus == (int) HigherPriority.System)
			{
					/// There was not any iFolder level policy but system level policy is there so show that.
					if(SystemStatus == (int) Share.Sharing || SystemStatus == 0)
					{
						SharingOn.Checked = true;
						enforcedSharing.Enabled = true;
						disablePastSharing.Enabled = false;
					}
					else
					{
							if( (SystemStatus & (int)Share.Sharing) == (int)Share.Sharing)
							{
								SharingOn.Checked = true;
								enforcedSharing.Enabled = true;
								disablePastSharing.Enabled = false;
								if((SystemStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
								{
									//disableSharingOn.Checked = true;
									//enforcedDisableSharing.Checked = true;	
									//system has higher priority , so disable the check boxes
									SharingOn.Enabled = enforcedSharing.Enabled = false;
								}
							}
							else if( (SystemStatus & (int)Share.DisableSharing) == (int)Share.DisableSharing)
                                                        {
                                                                SharingOn.Checked = false;
                                                                enforcedSharing.Enabled = true;
                                                                disablePastSharing.Enabled = true;
                                                                if((SystemStatus &  (int)Share.EnforcedSharing) == (int) Share.EnforcedSharing)
                                                                {
                                                                        //disableSharingOn.Checked = true;
                                                                        //enforcedDisableSharing.Checked = true;
                                                                        //system has higher priority , so disable the check boxes
                                                                        SharingOn.Enabled = enforcedSharing.Enabled = false;
                                                                }
                                                        }

					}
			}
			
			// There is no requirement of enforce check box on iFolder level, so make it invisible
			enforcedSharing.Visible = false;
		}
		
		
		///<summary>
	        /// Get the policy for an iFolder.
        	/// </summary>
	        /// <param name="policy">The iFolderPolicy object.</param>
		private int DeriveStatus(int system, int group, int user)
		{
       			//check whether there is any enforcement of policy on system or group level
		        bool SystemEnforcement = ( (system & (int)Share.EnforcedSharing) == (int)Share.EnforcedSharing );
		        bool GroupEnforcement = ( (group & (int)Share.EnforcedSharing) == (int)Share.EnforcedSharing );
        	        if ( SystemEnforcement)
	                {
        	    		/// although next check is redundant , i am putting ... should be checked and removed
	        	    	if(system != 0 )
	        	    		return (int)HigherPriority.System;
        	    		return (int)HigherPriority.User;
		        }
			else if(GroupEnforcement)
			{
				if(group != 0)
					 return (int)HigherPriority.Group;
        	    		return (int)HigherPriority.User;
			}
        		else
            		{
        	    		return (int)HigherPriority.User;
                	}
		}

		///<summary>
	        /// Get the policy for an iFolder.
        	/// </summary>
	        /// <param name="policy">The iFolderPolicy object.</param>
		private int DeriveStatus(int system, int group, int user, int iFolder)
		{

			//check for enforcement of policy on system,group or user level
			bool SystemEnforcement = ( (system & (int)Share.EnforcedSharing) == (int)Share.EnforcedSharing );
                        bool GroupEnforcement = ( (group & (int)Share.EnforcedSharing) == (int)Share.EnforcedSharing );
			bool UserEnforcement = ( (user & (int)Share.EnforcedSharing) == (int)Share.EnforcedSharing );
		
			if( SystemEnforcement)
                	{
            			// redundant check, should be removed
		            	if(system != 0)
        		    		return (int)HigherPriority.System;
            			return (int)HigherPriority.iFolder;	
                	}

			if(GroupEnforcement)
                        {
                                if(group != 0)
                                         return (int)HigherPriority.Group;
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
            			return (int)HigherPriority.iFolder;	
                	}
		}

		/// <summary>
		/// Sets the policy for the system.
		/// </summary>
		/// <param name="policy">System policy where the account information will be set.</param>
		public void SetSharingPolicy( SystemPolicy policy )
		{
			int SharingStatus=0;
			if( SharingOn.Checked)
				SharingStatus += (int) Share.Sharing; //1;
			else 
				SharingStatus += (int) Share.DisableSharing; //8
			if(enforcedSharing.Checked)
				SharingStatus += (int) Share.EnforcedSharing; //4;
				
			policy.SharingStatus = SharingStatus;
			
			/// Next change is to remove the past sharing ,  
			if( disablePastSharing.Checked )
			{
				iFolderServer [] iFolderServers = web.GetServers();
					
				foreach(iFolderServer server in iFolderServers)
				{				
					if( server.PublicUrl != null)
					{
						remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
					}
					try
					{
						remoteweb.DisableSystemPastSharing() ;
					}
					catch (Exception ex)
					{
						throw new ArgumentException( GetString("REVERTSHARINGCONNECTFAILED" ) );
					}
				}
				disablePastSharing.Checked = false;
			}	
		}
		
		/// <summary>
		/// Sets the  policy for the user.
		/// </summary>
		/// <param name="policy">User policy where the information will be set.</param>
		public void SetSharingPolicy( UserPolicy policy, string PolicyID)
		{
			int SharingStatus=0;
			int UserStatus = policy.SharingStatus;
			string valuechanged = Session["ValueChanged"] as string;
	
			if( valuechanged.Equals("true"))
			{
				if( SharingOn.Checked )
	                	        SharingStatus += (int) Share.Sharing; //1;
	                        else
	        	                SharingStatus += (int) Share.DisableSharing; //8
        	        	if(enforcedSharing.Checked)
                	                SharingStatus += (int) Share.EnforcedSharing; //4;
			}
			else
			{
				SharingStatus = UserStatus;
			}

			policy.SharingStatus = SharingStatus;

			if( disablePastSharing.Checked )
			{
				// check whether it is groupid or userid 

				if(web.IsGroupId(PolicyID))
				{
					// for group , get all the servers and pass this id so that all the users provisioned on different servers
					// will be revoked one by one.

					iFolderServer [] iFolderServers = web.GetServers();

	                                foreach(iFolderServer server in iFolderServers)
	                                {
	                                        if( server.PublicUrl != null)
        	                                {
                	                                remoteweb.Url = server.PublicUrl + "/iFolderAdmin.asmx";
                        	                }
                                	        try
                                        	{
	                                                remoteweb.DisableUserPastSharing(PolicyID) ;
       		                                }
                	                        catch (Exception ex)
                        	                {
                                	                throw new ArgumentException( GetString("REVERTSHARINGCONNECTFAILED" ) );
                                        	}
                                	}

				}		
				else
				{
					// it is not a group , so get home server for this user and revoke the sharing.
					string publicUrl = web.GetHomeServerURLForUserID(PolicyID);
        	                	if(publicUrl != null)
					{
                        	        	remoteweb.Url = publicUrl + "/iFolderAdmin.asmx";
					}
					try
					{
						remoteweb.DisableUserPastSharing(PolicyID) ;
	
					}
					catch ( Exception ex)
					{
						throw new ArgumentException( GetString("REVERTSHARINGCONNECTFAILED" ) );
					}
				}
				disablePastSharing.Checked = false;
			}	
		}

		/// <summary>
		/// Sets the policy for the user.
		/// </summary>
		/// <param name="policy">iFolder policy where the  information will be set.</param>
		public void SetSharingPolicy( iFolderPolicy policy, string PolicyID )
		{
			int SharingStatus=0;
			int iFolderStatus = policy.SharingStatus;

			string valuechanged = Session["ValueChanged"] as string;

                        if( valuechanged.Equals("true"))
			{
				if( SharingOn.Checked )
                                        SharingStatus += (int) Share.Sharing; //1;
                                else
                                        SharingStatus += (int) Share.DisableSharing; //8
			}
			else
			{
				SharingStatus = iFolderStatus;
			}

                        policy.SharingStatus = SharingStatus;

                        if( disablePastSharing.Checked )
                        {
                                /// pass the iFolder ID

				string ifolderLocation = web.GetiFolderLocation (PolicyID);
                        	if(ifolderLocation != null)
                        	{
                               	 	UriBuilder remoteurl = new UriBuilder(ifolderLocation);
                               	 	remoteurl.Path = (new Uri(web.Url)).PathAndQuery;
                                	remoteweb.Url = remoteurl.Uri.ToString();
                        	}
				try
				{	
                                	remoteweb.DisableiFolderPastSharing(PolicyID) ;
				}
				catch (Exception ex)
				{
					throw ex;
				}
                                disablePastSharing.Checked = false;
                        }

		}

		/// <summary>
		/// Sets the checkbox enabled or disabled state
		/// </summary>
		public bool SetCheckBoxEnabledState
		{
			set
			{
				SharingOn.Enabled = value;
				enforcedSharing.Enabled = value;
				disablePastSharing.Enabled = value;
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
		
			SharingOn.CheckedChanged += new EventHandler( SharingOnCheckChanged );
			disablePastSharing.CheckedChanged += new EventHandler( disablePastSharingCheckChanged);
			enforcedSharing.CheckedChanged += new EventHandler( enforcedSharingCheckChanged );

			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion
	}
}
