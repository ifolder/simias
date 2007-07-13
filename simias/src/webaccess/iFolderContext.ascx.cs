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
using System.Threading;
using System.Resources;
using System.Web.Services.Protocols;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	///	iFolder Context Control
	/// </summary>
	public class iFolderContextControl : UserControl
	{
		/// <summary>
		/// Home Link
		/// </summary>
		protected HyperLink HomeLink;
		
		/// <summary>
		/// iFolder Link
		/// </summary>
		protected HyperLink iFolderLink;

		/// <summary>
		/// iFolder Image Link
		/// </summary>
		protected HyperLink iFolderImageLink;
		
		/// <summary>
		/// iFolder Image Url
		/// </summary>
		protected System.Web.UI.WebControls.Image iFolderImageUrl;

		/// <summary>
		/// Search Pattern
		/// </summary>
		protected TextBox SearchPattern;

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

			// connection
			web = (iFolderWeb) Session["Connection"];

			// localization
			rm = (ResourceManager) Application["RM"];

			if (!IsPostBack)
			{
				/*string PassPhrase = Session["SessionPassPhrase"] as string;
				ifolderID = Request.QueryString.Get("iFolder");
				iFolder ifolder = web.GetiFolder(ifolderID);
				string EncryptionAlgorithm = ifolder.EncryptionAlgorithm;
				SearchPattern.Enabled = true;
				if(PassPhrase == null && EncryptionAlgorithm == "")
				{
					// it means , this is not an encrypted ifolder 
					// enable the search text box
					SearchPattern.Enabled = true;
				}
				else if(PassPhrase != null)
				{
					// user is in current session , so enable it 
					SearchPattern.Enabled = true;			
				}	
				else if(PassPhrase == null && web.IsPassPhraseSet())
				{
					SearchPattern.Enabled = false;
				}*/
				
				// query
				SearchPattern.Text = Request.QueryString.Get("Pattern");

				// data
				BindData();

				// strings
				HomeLink.Text = GetString("HOME");
				
				bool encrypted = false;
				iFolder folder = web.GetiFolder(ifolderID);
				string EncryptionAlgorithm = folder.EncryptionAlgorithm;
				if(!(EncryptionAlgorithm == null || (EncryptionAlgorithm == String.Empty)))
				{
					// It is an encrypted ifolder 
					encrypted = true;
				}
				
				bool shared = ( folder.MemberCount > 1 ) ? true : false;
				
				iFolderImageUrl.ImageUrl = encrypted ? "images/encrypt_ilock2_16.gif" : (shared ? "images/ifolder_user_16.gif" : "images/ifolder.png");

				// links
				iFolderLink.NavigateUrl = iFolderImageLink.NavigateUrl =
					"Browse.aspx?iFolder=" + ifolderID;
			}
		}

		/// <summary>
		/// Bind the Data to the Page.
		/// </summary>
		private void BindData()
		{
			try
			{
				// ifolder
				iFolder ifolder = web.GetiFolder(ifolderID);
				iFolderLink.Text = ifolder.Name;
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

		#region Properties
		
		/// <summary>
		/// Search Pattern
		/// </summary>
		public string Pattern
		{
			get { return SearchPattern.Text; }
		}

		#endregion

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
			this.SearchPattern.TextChanged += new EventHandler(SearchPattern_TextChanged);
		}
		
		#endregion

			
		/// <summary>
		/// Search Pattern Text Changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SearchPattern_TextChanged(object sender, EventArgs e)
		{
				Response.Redirect(String.Format("Search.aspx?iFolder={0}&Pattern={1}", ifolderID,
					Server.UrlEncode(Pattern)));
		}
	}
}
