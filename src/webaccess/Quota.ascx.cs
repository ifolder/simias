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
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using System.Threading;
using System.Resources;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	///	Quota Control
	/// </summary>
	public class QuotaControl : UserControl
	{
		/// <summary>
		/// Log
		/// </summary>
		private static readonly WebLogger log = new WebLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Title
		/// </summary>
		protected Literal Title;

		/// <summary>
		/// Space Used
		/// </summary>
		protected Literal SpaceUsed;

		/// <summary>
		/// Space Available
		/// </summary>
		protected Literal SpaceAvailable;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// iFolder Connection
		/// </summary>
		private iFolderWeb web;
	
		/// <summary>
		/// iFolder ID
		/// </summary>
		private string ifolderID;

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// query
			//ifolderID = Request.QueryString.Get("iFolder");

			// localization
			rm = (ResourceManager) Application["RM"];

			// check connection
			web = (iFolderWeb)Session["Connection"];
			
			if (!IsPostBack)
			{
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			long used = 0;
			long limit = 0;
			ifolderID = Request.QueryString.Get("iFolder");

			if ((ifolderID != null) && (ifolderID.Length != 0))
			{
				// ifolder
				iFolderPolicy policy = web.GetiFolderPolicy(ifolderID);
				used = policy.SpaceUsed;
				limit = policy.SpaceLimitEffective;

				Title.Text = GetString("IFOLDERQUOTA");
			}
			else
			{
				// global
				try
				{
					UserPolicy policy = web.GetAuthenticatedUserPolicy();
					used = policy.SpaceUsed;
					limit = policy.SpaceLimitEffective;

					Title.Text = GetString("HOMEQUOTA");
				}
				catch (Exception e)
				{
//					Title.Text = e.Message;
				}
			}

			// used
			SpaceUsed.Text = WebUtility.FormatSize(used, rm);
			
			// limit
			if (limit == -1)
			{
				// no limit
				SpaceAvailable.Text = GetString("NOLIMIT");
			}
			else
			{
				// limit
				
				long Available = (limit - used) >= 0 ? (limit - used) : 0;
				SpaceAvailable.Text = WebUtility.FormatSize(Available, rm);
			}
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
		/// On Intialize
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
			this.PreRender += new EventHandler(Quota_PreRender);
		}
		
		#endregion

		/// <summary>
		/// Quota Pre-Render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Quota_PreRender(object sender, EventArgs e)
		{
			// bind
			// NOTE: bind the footer late so modifications can be shown
			BindData();
		}
	}
}
