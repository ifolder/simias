/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using Simias;
using Novell.Win32Util;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for NewiFolder.
	/// </summary>
	[ComVisible(false)]
	public class NewiFolder : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button close;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel iFolderProperties;
		private System.Windows.Forms.PictureBox iFolderEmblem;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox dontAsk;
		private string folderName;
		private string loadPath;
		private const int SHOP_FILEPATH = 0x2;
		private System.Windows.Forms.LinkLabel iFolderHelp;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructs a NewiFolder object.
		/// </summary>
		public NewiFolder()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Center the window.
			this.StartPosition = FormStartPosition.CenterScreen;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.close = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.iFolderProperties = new System.Windows.Forms.LinkLabel();
			this.iFolderEmblem = new System.Windows.Forms.PictureBox();
			this.label2 = new System.Windows.Forms.Label();
			this.dontAsk = new System.Windows.Forms.CheckBox();
			this.iFolderHelp = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// close
			// 
			this.close.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.close.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.close.Location = new System.Drawing.Point(376, 192);
			this.close.Name = "close";
			this.close.TabIndex = 0;
			this.close.Text = "Close";
			this.close.Click += new System.EventHandler(this.close_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(424, 24);
			this.label1.TabIndex = 1;
			this.label1.Text = "Congratulations!  You successfully converted this normal folder to an iFolder.";
			// 
			// iFolderProperties
			// 
			this.iFolderProperties.LinkArea = new System.Windows.Forms.LinkArea(113, 14);
			this.iFolderProperties.Location = new System.Drawing.Point(16, 112);
			this.iFolderProperties.Name = "iFolderProperties";
			this.iFolderProperties.Size = new System.Drawing.Size(432, 32);
			this.iFolderProperties.TabIndex = 2;
			this.iFolderProperties.TabStop = true;
			this.iFolderProperties.Text = "To share your iFolder and its contents with others, right-click the iFolder, then" +
				" click iFolder > Share With, or click here now.";
			this.iFolderProperties.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.iFolderProperties_LinkClicked);
			// 
			// iFolderEmblem
			// 
			this.iFolderEmblem.Location = new System.Drawing.Point(16, 52);
			this.iFolderEmblem.Name = "iFolderEmblem";
			this.iFolderEmblem.Size = new System.Drawing.Size(48, 48);
			this.iFolderEmblem.TabIndex = 3;
			this.iFolderEmblem.TabStop = false;
			this.iFolderEmblem.Paint += new System.Windows.Forms.PaintEventHandler(this.iFolderEmblem_Paint);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(72, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(368, 24);
			this.label2.TabIndex = 4;
			this.label2.Text = "The iFolder emblem distinguishes iFolders from normal folders.";
			// 
			// dontAsk
			// 
			this.dontAsk.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.dontAsk.Location = new System.Drawing.Point(16, 192);
			this.dontAsk.Name = "dontAsk";
			this.dontAsk.Size = new System.Drawing.Size(304, 16);
			this.dontAsk.TabIndex = 6;
			this.dontAsk.Text = "Do not show this message again.";
			// 
			// iFolderHelp
			// 
			this.iFolderHelp.LinkArea = new System.Windows.Forms.LinkArea(65, 14);
			this.iFolderHelp.Location = new System.Drawing.Point(16, 152);
			this.iFolderHelp.Name = "iFolderHelp";
			this.iFolderHelp.Size = new System.Drawing.Size(432, 23);
			this.iFolderHelp.TabIndex = 7;
			this.iFolderHelp.TabStop = true;
			this.iFolderHelp.Text = "For help, right-click the iFolder, then click iFolder > Help, or click here now.";
			this.iFolderHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.iFolderHelp_LinkClicked);
			// 
			// NewiFolder
			// 
			this.AcceptButton = this.close;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(456, 224);
			this.Controls.Add(this.iFolderHelp);
			this.Controls.Add(this.dontAsk);
			this.Controls.Add(this.iFolderProperties);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.iFolderEmblem);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.close);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NewiFolder";
			this.Text = "iFolder Introduction";
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets/sets the name of the folder.
		/// </summary>
		public string FolderName
		{
			get
			{
				return folderName;
			}
			set
			{
				folderName = value;
			}
		}

		/// <summary>
		/// Gets/sets the path where the assembly was loaded from.
		/// </summary>
		public string LoadPath
		{
			get
			{
				return loadPath;
			}

			set
			{
				loadPath = value;
			}
		}
		#endregion

		#region Event Handlers
		private void iFolderProperties_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			// Invoke the iFolder properties dialog.
//			Win32Window.ShObjectProperties(IntPtr.Zero, SHOP_FILEPATH, FolderName, "iFolder");
			iFolderComponent ifCom = new iFolderComponent();
			ifCom.InvokeAdvancedDlg(LoadPath, FolderName, 1, false);
		}

		private void iFolderHelp_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			iFolderComponent ifCom = new iFolderComponent();
			ifCom.ShowHelp(LoadPath);
		}

		private void close_Click(object sender, System.EventArgs e)
		{
			if (dontAsk.Checked)
			{
				Configuration.GetConfiguration().Set("iFolderShell", "Show wizard", "false");
			}

			this.Close();
		}

		private void iFolderEmblem_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			try
			{
				IFSHFILEINFO fi;
				IntPtr ret = Win32Window.ShGetFileInfo(
					FolderName, 
					Win32Window.FILE_ATTRIBUTE_DIRECTORY,
					out fi,
					342,
					Win32Window.SHGFI_ICON | Win32Window.SHGFI_USEFILEATTRIBUTES);

				if (ret != IntPtr.Zero)
				{
					Bitmap bmap = Bitmap.FromHicon(fi.hIcon);
					e.Graphics.DrawImage(bmap, 0, 0);

					IntPtr hIcon = Win32Window.LoadImageFromFile(
						0,
						Path.Combine(loadPath, "ifolder_emblem.ico"),
						Win32Window.IMAGE_ICON,
						32,
						32,
						Win32Window.LR_LOADFROMFILE);

					bmap = Bitmap.FromHicon(hIcon);
					e.Graphics.DrawImage(bmap, 0, 0);
				}
			}
			catch{}
		}
		#endregion
	}
}
