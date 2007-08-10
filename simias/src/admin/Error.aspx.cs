/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com
 |
 | Author: Mike Lasky (mlasky@novell.com)
 |***************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Resources;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Error Page
	/// </summary>
	public class Error : System.Web.UI.Page
	{
		#region Class Members

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;


		/// <summary>
		/// Top navigation panel control.
		/// </summary>
		protected TopNavigation TopNav;

		/// <summary>
		/// Div
		/// </summary>
		protected HtmlGenericControl ExceptionNav;

		/// <summary>
		/// Stack dump
		/// </summary>
		protected TextBox StackDump;

		#endregion

		#region Private Methods

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load( object sender, System.EventArgs e )
		{
			// localization
			rm = Application[ "RM" ] as ResourceManager;
		}

		/// <summary>
		/// Page_PreRender
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_PreRender( object sender, EventArgs e )
		{
			// message from query string
			string message = Request.QueryString.Get( "ex" );
			if ( ( message == null ) || ( message.Length < 0 ) )
			{
				Exception ex = Session[ "Exception" ] as Exception;
				if ( ex != null )
				{
					TopNav.ShowError( ex );

					if ( TopNav.ShowExceptionDetail( ex ) )
					{
						StringWriter sw = new StringWriter();

						sw.WriteLine( "Server Version: {0}", Session["Version"] );
						sw.WriteLine( "HostName:       {0}", Session["HostName"] );
						sw.WriteLine( "MachineName:    {0}", Session["MachineName"] );
						sw.WriteLine( "OS Version:     {0}", Session["OSVersion"] );
						sw.WriteLine( "CLR Version:    {0}", Session["ClrVersion"] );
						sw.WriteLine();
						sw.WriteLine();

						sw.WriteLine( "Exception detail:" );
						sw.WriteLine();
						sw.WriteLine( "Exception type: {0}", TopNavigation.GetExceptionType( ex ) );
						sw.WriteLine();
						sw.WriteLine( ex.Message );
						sw.WriteLine();
						sw.WriteLine( ex.StackTrace );
						StackDump.Text = sw.ToString();
					}
					else
					{
						ExceptionNav.Visible = false;
					}
				}
				else
				{
					TopNav.ShowError( GetString( "UNKNOWNERROR" ) );
				}
			}
			else
			{
				TopNav.ShowError( message );
			}
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString(string key)
		{
			return rm.GetString(key);
		}

		#endregion

		#region Web Form Designer
		
		/// <summary>
		/// On Intialization
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
			if ( !Page.IsPostBack )
			{
				// Set the render event to happen only on page load.
				Page.PreRender += new EventHandler( Page_PreRender );
			}

			this.Load += new System.EventHandler(this.Page_Load);
		}

		#endregion
	}
}
