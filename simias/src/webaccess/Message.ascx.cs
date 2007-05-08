/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
 *  Author: Rob
 *
 ***********************************************************************/

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
		private string text;

		/// <summary>
		/// Page Init
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Init(object sender, EventArgs e)
		{
			text = null;
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
		/// Page Pre-Render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Message_PreRender(object sender, EventArgs e)
		{
			TextLiteral.Text = text;
			Message.Visible = (text != null) && (text.Length > 0);
		}
	}
}
