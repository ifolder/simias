/***********************************************************************
 *  $RCSfile: Error.aspx.cs,v $
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
		/// <summary>
		/// Div
		/// </summary>
		protected HtmlGenericControl DetailsButtonRegion;
		
		/// <summary>
		/// Error Type
		/// </summary>
		protected Label ErrorType;
		
		/// <summary>
		/// Error Instructions
		/// </summary>
		protected Label ErrorInstructions;

		/// <summary>
		/// Error Message
		/// </summary>
		protected Literal ErrorMessage;

		/// <summary>
		/// Error Stack Trace
		/// </summary>
		protected Literal ErrorStackTrace;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = (ResourceManager) Application["RM"];
				
			// strings
			ErrorType.Text = rm.GetString("ERRORTYPE");
			ErrorInstructions.Text = rm.GetString("ERRORINSTRUCTIONS");

			// message from query string
			string message = Request.QueryString.Get("Exception");

			if ((message == null) || (message.Length < 0))
			{
				// message from session
				message = null;

				Exception ex = (Exception)Session["Exception"];

				if (ex != null)
				{
					message = ex.ToString();
				}
			}
			
			// did we find a message
			if (message != null)
			{
				ErrorMessage.Text = "\n\n" + message + "\n";
			}
		}

		/// <summary>
		/// Get a Localized String
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		protected string GetString(string key)
		{
			return rm.GetString(key);
		}

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
			this.Load += new System.EventHandler(this.Page_Load);
		}

		#endregion
	}
}
