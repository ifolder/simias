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
using System.Web.Services.Protocols;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	///	iFolder Actions Control
	/// </summary>
	public class iFolderActionsControl : UserControl
	{
		/// <summary>
		/// Log
		/// </summary>
		private static readonly WebLogger log = new WebLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

		/// <summary>
		/// Message Box
		/// </summary>
		protected MessageControl Message;

		/// <summary>
		/// Remove Container
		/// </summary>
		protected HtmlContainerControl Remove;

		/// <summary>
		/// Remove iFolder Membership Button
		/// </summary>
		protected LinkButton RemoveButton;

		/// <summary>
		/// Delete Container
		/// </summary>
		protected HtmlContainerControl Delete;

		/// <summary>
		/// Delete iFolder Button
		/// </summary>
		protected LinkButton DeleteButton;

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
			ifolderID = Request.QueryString.Get("iFolder");

			// localization
			rm = (ResourceManager) Application["RM"];

			// check connection
			web = (iFolderWeb)Session["Connection"];
			
			if (!IsPostBack)
			{
				BindData();

				// strings
				RemoveButton.Text = GetString("REMOVEMEMBERSHIP");
				DeleteButton.Text = GetString("DELETEIFOLDER");
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			try
			{
				iFolder ifolder = web.GetiFolder(ifolderID);

				Remove.Visible = !ifolder.IsOwner;
				Delete.Visible = ifolder.IsOwner;
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

				case "LockException":
					Message.Text = GetString("ENTRY.LOCKEXCEPTION");
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
			this.RemoveButton.PreRender += new EventHandler(RemoveButton_PreRender);
			this.RemoveButton.Click += new EventHandler(RemoveButton_Click);
			this.DeleteButton.PreRender += new EventHandler(DeleteButton_PreRender);
			this.DeleteButton.Click += new EventHandler(DeleteButton_Click);
		}
		
		#endregion

		/// <summary>
		/// Remove Buton Pre-Render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveButton_PreRender(object sender, EventArgs e)
		{
			RemoveButton.Attributes["onclick"] = "return ConfirmiFolderRemove(this.form);";
		}

		/// <summary>
		/// Remove Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RemoveButton_Click(object sender, EventArgs e)
		{
			try
			{
				web.RemoveMembership(ifolderID);

				Response.Redirect("iFolders.aspx");
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
		}

		/// <summary>
		/// Delete Button Pre-Render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteButton_PreRender(object sender, EventArgs e)
		{
			DeleteButton.Attributes["onclick"] = "return ConfirmiFolderDelete(this.form);";
		}

		/// <summary>
		/// Delete Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DeleteButton_Click(object sender, EventArgs e)
		{
			try
			{
				web.DeleteiFolder(ifolderID);

				Response.Redirect("iFolders.aspx");
			}
			catch(SoapException ex)
			{
				if (!HandleException(ex)) throw;
			}
		}
	}
}
