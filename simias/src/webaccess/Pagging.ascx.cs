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

namespace Novell.iFolderApp.Web
{
	/// <summary>
	///	Pagging Control
	/// </summary>
	public class Pagging : UserControl
	{
		/// <summary>
		/// Default Items / Page
		/// </summary>
		public static int DEFAULT_ITEMS_PER_PAGE = 50;

		/// <summary>
		/// Pagging Start Index
		/// </summary>
		protected Literal StartIndex;
		
		/// <summary>
		/// Pagging End Index
		/// </summary>
		protected Literal EndIndex;
		
		/// <summary>
		/// Pagging Total
		/// </summary>
		protected Literal TotalLabel;

		/// <summary>
		/// Item Label Plural
		/// </summary>
		protected Literal ItemLabelPlural;

		/// <summary>
		/// Item Label Singular
		/// </summary>
		protected Literal ItemLabelSingular;

		/// <summary>
		/// Pagging First Image
		/// </summary>
		protected ImageButton FirstImage;

		/// <summary>
		/// Pagging First Image Disabled
		/// </summary>
		protected System.Web.UI.WebControls.Image FirstImageDisabled;

		/// <summary>
		/// Pagging Previous Image
		/// </summary>
		protected ImageButton PreviousImage;

		/// <summary>
		/// Pagging Previous Image Disabled
		/// </summary>
		protected System.Web.UI.WebControls.Image PreviousImageDisabled;

		/// <summary>
		/// Pagging Next Image
		/// </summary>
		protected ImageButton NextImage;

		/// <summary>
		/// Pagging Next Image Disabled
		/// </summary>
		protected System.Web.UI.WebControls.Image NextImageDisabled;

		/// <summary>
		/// Pagging Last Image
		/// </summary>
		protected ImageButton LastImage;

		/// <summary>
		/// Pagging Last Image Disabled
		/// </summary>
		protected System.Web.UI.WebControls.Image LastImageDisabled;

		/// <summary>
		/// Resource Manager
		/// </summary>
		private ResourceManager rm;
	
		/// <summary>
		/// Pagging Index
		/// </summary>
		private int index;

		/// <summary>
		/// Pagging Page Size
		/// </summary>
		private int page;

		/// <summary>
		/// Pagging Current Count
		/// </summary>
		private int count;

		/// <summary>
		/// Pagging Total
		/// </summary>
		private int total;

		#region Events
		
		/// <summary>
		/// On Page Change Event Handler
		/// </summary>
		public event EventHandler PageChange;
		
		#endregion

		/// <summary>
		/// Page Init
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Init(object sender, EventArgs e)
		{
			index = 0;

			// page size
			try
			{
				page = (int)Session["ItemsPerPage"];
			}
			catch
			{
				page = DEFAULT_ITEMS_PER_PAGE;
			}
		}

		/// <summary>
		/// Page Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Page_Load(object sender, System.EventArgs e)
		{
			// localization
			rm = (ResourceManager) Application["RM"];
			
			if (!IsPostBack)
			{
				// strings
				FirstImage.ToolTip = FirstImageDisabled.ToolTip = GetString("FIRST");
				PreviousImage.ToolTip = PreviousImageDisabled.ToolTip = GetString("PREVIOUS");
				NextImage.ToolTip = NextImageDisabled.ToolTip = GetString("NEXT");
				LastImage.ToolTip = LastImageDisabled.ToolTip = GetString("LAST");
			}
			else
			{
				// read state
				try
				{
					total = int.Parse(TotalLabel.Text);
					index = (total > 0) ? int.Parse(StartIndex.Text) - 1 : 0;
					count = int.Parse(EndIndex.Text) - index;
				}
				catch
				{
					// ignore parsing error
				}
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

		#region Public Methods
		#endregion

		#region Properties

		/// <summary>
		/// Index
		/// </summary>
		public int Index
		{
			get { return index; }
			set { index = value; }
		}

		/// <summary>
		/// Page Size
		/// </summary>
		public int PageSize
		{
			get { return page; }
		}

		/// <summary>
		/// Count
		/// </summary>
		public int Count
		{
			get { return count; }
			set { count = value; }
		}

		/// <summary>
		/// Total
		/// </summary>
		public int Total
		{
			get { return total; }
			
			set
			{
				total = value;
			
				// if the total is now smaller than the index
				// move the index to the last page
				if (total == 0)
				{
					index = 0;
				}
				else if (index >= total)
				{
					index = ((total - 1) / page) * page;
				}
			}
		}

		/// <summary>
		/// Plural Label
		/// </summary>
		public string LabelPlural
		{
			get { return ItemLabelPlural.Text; }
			set { ItemLabelPlural.Text = value; }
		}

		/// <summary>
		/// Singular Label
		/// </summary>
		public string LabelSingular
		{
			get { return ItemLabelSingular.Text; }
			set { ItemLabelSingular.Text = value; }
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
			this.Init += new EventHandler(Page_Init);
			this.Load += new EventHandler(Page_Load);
			this.FirstImage.Click += new ImageClickEventHandler(FirstImage_Click);
			this.PreviousImage.Click += new ImageClickEventHandler(PreviousImage_Click);
			this.NextImage.Click += new ImageClickEventHandler(NextImage_Click);
			this.LastImage.Click += new ImageClickEventHandler(LastImage_Click);
			this.PreRender += new EventHandler(Pagging_PreRender);
		}

		#endregion

		/// <summary>
		/// First Image Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FirstImage_Click(object sender, ImageClickEventArgs e)
		{
			index = 0;
			
			PageChange(this, null);
		}

		/// <summary>
		/// Previous Image Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PreviousImage_Click(object sender, ImageClickEventArgs e)
		{
			index -= page;
			
			PageChange(this, null);
		}

		/// <summary>
		/// Next Image Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void NextImage_Click(object sender, ImageClickEventArgs e)
		{
			index += page;
			
			PageChange(this, null);
		}

		/// <summary>
		/// Last Button Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LastImage_Click(object sender, ImageClickEventArgs e)
		{
			index = ((total - 1) / page) * page;
			
			PageChange(this, null);
		}

		/// <summary>
		/// Pre-Render
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Pagging_PreRender(object sender, EventArgs e)
		{
			StartIndex.Text = (total > 0) ? (index + 1).ToString() : "0";
			EndIndex.Text = (index + count).ToString();
			TotalLabel.Text = total.ToString();
			FirstImage.Visible = PreviousImage.Visible = (index > 0);
			FirstImageDisabled.Visible = PreviousImageDisabled.Visible = !FirstImage.Visible;
			NextImage.Visible = LastImage.Visible = ((index + count) < total);
			NextImageDisabled.Visible = LastImageDisabled.Visible = !NextImage.Visible;
			ItemLabelSingular.Visible = (total == 1);
			ItemLabelPlural.Visible = !ItemLabelSingular.Visible;
		}
	}
}
