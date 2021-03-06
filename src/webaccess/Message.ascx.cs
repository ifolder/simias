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

namespace Novell.iFolderApp.Web
{
	/// <summary>
	///	Message Control
	/// </summary>
	public class MessageControl : UserControl
	{
		/// <summary>
		/// Message Div Tag
		/// </summary>
		protected HtmlGenericControl Message;

		/// <summary>
		/// Message Text
		/// </summary>
		protected Literal TextLiteral;

		/// <summary>
		/// Message Text
		/// </summary>
		protected Literal InfoLiteral;

		/// <summary>
		/// Message Text
		/// </summary>
		private string text;

		/// <summary>
		/// Message Text
		/// </summary>
		private string info;

		/// <summary>
		/// Page Init
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Init(object sender, EventArgs e)
		{
			text = null;
			info = null;
		}

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			if (!IsPostBack)
			{
				// query message
				string temp = Request.QueryString.Get("Message");

				if ((temp != null) && (temp.Length > 0))
				{
					text = temp;
					info = temp;
				}
			}
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
			this.Init += new EventHandler(Page_Init);
			this.Load += new System.EventHandler(this.Page_Load);
			this.PreRender += new EventHandler(Message_PreRender);
		}
		
		#endregion

		/// <summary>
		/// Message Text
		/// </summary>
		public String Text
		{
			get { return text; }
			
			set { text = value; }
		}

		/// <summary>
		/// Message Text
		/// </summary>
		public String Info 
		{
			get { return info; }
			
			set { info = value; }
		}

		/// <summary>
		/// Page Pre-Render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Message_PreRender(object sender, EventArgs e)
		{
			TextLiteral.Text = text;
			Message.Visible = (text != null) && (text.Length > 0);
			Message.Attributes["style"] = "background-color: #ffff99";
			if(text == null || text.Length <= 0)
			{
				InfoLiteral.Text = info;
				Message.Attributes["style"] = "background-color: #a3ffa3";
				Message.Visible = (info != null) && (info.Length > 0);
			}
		}
	}
}
