/***********************************************************************
 *  ContactEditor.cs - A contact editor implemented using Windows.Forms
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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
using System.IO;
using Simias.Storage;
using Novell.AddressBook;

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// Summary description for ContactEditor.
	/// </summary>
	public class ContactEditor : System.Windows.Forms.Form
	{
		#region Class Members
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox phone3;
		private System.Windows.Forms.TextBox phone2;
		private System.Windows.Forms.TextBox phone1;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox phoneSelect2;
		private System.Windows.Forms.ComboBox phoneSelect1;
		private System.Windows.Forms.TextBox phone4;
		private System.Windows.Forms.ComboBox phoneSelect4;
		private System.Windows.Forms.ComboBox phoneSelect3;
		private System.Windows.Forms.CheckBox mailHTML;
		private System.Windows.Forms.ComboBox addressSelect;
		private System.Windows.Forms.CheckBox mailAddress;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox webAddress;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.ListBox address;
		private System.Windows.Forms.TextBox eMail;
		private System.Windows.Forms.PictureBox pictureMail;
		private System.Windows.Forms.PictureBox pictureContact;
		private System.Windows.Forms.PictureBox picturePhone;
		private System.Windows.Forms.PictureBox pictureAddress;
		private System.Windows.Forms.PictureBox pictureWeb;
		private Hashtable phoneHT;
		private Hashtable emailHT;
		private Name name;

		private Novell.AddressBook.AddressBook addressBook = null;
		private Contact contact = null;
		private System.Windows.Forms.Button addr;
		private System.Windows.Forms.TextBox blogAddress;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button webConnect;
		private System.Windows.Forms.Button blogConnect;
		private System.Windows.Forms.Button fullNameButton;
		private System.Windows.Forms.Button changePicture;
		private System.Windows.Forms.TextBox userId;
		private System.Windows.Forms.TextBox organization;
		private System.Windows.Forms.TextBox fullName;
		private System.Windows.Forms.TextBox jobTitle;
		private System.Windows.Forms.ComboBox emailSelect;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public ContactEditor()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			try
			{
				// Load the images.
				// Get the base path.
				string basePath = Path.Combine(Application.StartupPath, "res");

				pictureContact.Image = Image.FromFile(Path.Combine(basePath, "blankhead.png"));
				picturePhone.Image = Image.FromFile(Path.Combine(basePath, "cellphone.png"));
				pictureMail.Image = Image.FromFile(Path.Combine(basePath, "ico-mail.png"));
				pictureAddress.Image = Image.FromFile(Path.Combine(basePath, "house.png"));
				pictureWeb.Image = Image.FromFile(Path.Combine(basePath, "globe.png"));
			}
			catch{}

			phoneHT = new Hashtable();
			emailHT = new Hashtable();
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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.changePicture = new System.Windows.Forms.Button();
			this.fullNameButton = new System.Windows.Forms.Button();
			this.blogConnect = new System.Windows.Forms.Button();
			this.webConnect = new System.Windows.Forms.Button();
			this.emailSelect = new System.Windows.Forms.ComboBox();
			this.blogAddress = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.addr = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.pictureWeb = new System.Windows.Forms.PictureBox();
			this.webAddress = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.mailAddress = new System.Windows.Forms.CheckBox();
			this.addressSelect = new System.Windows.Forms.ComboBox();
			this.pictureAddress = new System.Windows.Forms.PictureBox();
			this.address = new System.Windows.Forms.ListBox();
			this.mailHTML = new System.Windows.Forms.CheckBox();
			this.phoneSelect3 = new System.Windows.Forms.ComboBox();
			this.phoneSelect4 = new System.Windows.Forms.ComboBox();
			this.picturePhone = new System.Windows.Forms.PictureBox();
			this.phoneSelect1 = new System.Windows.Forms.ComboBox();
			this.phoneSelect2 = new System.Windows.Forms.ComboBox();
			this.userId = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.phone4 = new System.Windows.Forms.TextBox();
			this.phone3 = new System.Windows.Forms.TextBox();
			this.phone2 = new System.Windows.Forms.TextBox();
			this.phone1 = new System.Windows.Forms.TextBox();
			this.pictureMail = new System.Windows.Forms.PictureBox();
			this.eMail = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.pictureContact = new System.Windows.Forms.PictureBox();
			this.organization = new System.Windows.Forms.TextBox();
			this.jobTitle = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.fullName = new System.Windows.Forms.TextBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(792, 312);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.changePicture);
			this.tabPage1.Controls.Add(this.fullNameButton);
			this.tabPage1.Controls.Add(this.blogConnect);
			this.tabPage1.Controls.Add(this.webConnect);
			this.tabPage1.Controls.Add(this.emailSelect);
			this.tabPage1.Controls.Add(this.blogAddress);
			this.tabPage1.Controls.Add(this.label5);
			this.tabPage1.Controls.Add(this.addr);
			this.tabPage1.Controls.Add(this.groupBox4);
			this.tabPage1.Controls.Add(this.pictureWeb);
			this.tabPage1.Controls.Add(this.webAddress);
			this.tabPage1.Controls.Add(this.label4);
			this.tabPage1.Controls.Add(this.groupBox2);
			this.tabPage1.Controls.Add(this.mailAddress);
			this.tabPage1.Controls.Add(this.addressSelect);
			this.tabPage1.Controls.Add(this.pictureAddress);
			this.tabPage1.Controls.Add(this.address);
			this.tabPage1.Controls.Add(this.mailHTML);
			this.tabPage1.Controls.Add(this.phoneSelect3);
			this.tabPage1.Controls.Add(this.phoneSelect4);
			this.tabPage1.Controls.Add(this.picturePhone);
			this.tabPage1.Controls.Add(this.phoneSelect1);
			this.tabPage1.Controls.Add(this.phoneSelect2);
			this.tabPage1.Controls.Add(this.userId);
			this.tabPage1.Controls.Add(this.label7);
			this.tabPage1.Controls.Add(this.phone4);
			this.tabPage1.Controls.Add(this.phone3);
			this.tabPage1.Controls.Add(this.phone2);
			this.tabPage1.Controls.Add(this.phone1);
			this.tabPage1.Controls.Add(this.pictureMail);
			this.tabPage1.Controls.Add(this.eMail);
			this.tabPage1.Controls.Add(this.groupBox1);
			this.tabPage1.Controls.Add(this.pictureContact);
			this.tabPage1.Controls.Add(this.organization);
			this.tabPage1.Controls.Add(this.jobTitle);
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.fullName);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(784, 286);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			// 
			// changePicture
			// 
			this.changePicture.Location = new System.Drawing.Point(8, 88);
			this.changePicture.Name = "changePicture";
			this.changePicture.TabIndex = 4;
			this.changePicture.Text = "Change...";
			// 
			// fullNameButton
			// 
			this.fullNameButton.Location = new System.Drawing.Point(92, 12);
			this.fullNameButton.Name = "fullNameButton";
			this.fullNameButton.Size = new System.Drawing.Size(75, 26);
			this.fullNameButton.TabIndex = 0;
			this.fullNameButton.Text = "Full Name...";
			this.fullNameButton.Click += new System.EventHandler(this.fullNameButton_Click);
			// 
			// blogConnect
			// 
			this.blogConnect.Location = new System.Drawing.Point(348, 256);
			this.blogConnect.Name = "blogConnect";
			this.blogConnect.Size = new System.Drawing.Size(22, 22);
			this.blogConnect.TabIndex = 12;
			this.blogConnect.Text = "button2";
			// 
			// webConnect
			// 
			this.webConnect.Location = new System.Drawing.Point(348, 224);
			this.webConnect.Name = "webConnect";
			this.webConnect.Size = new System.Drawing.Size(22, 22);
			this.webConnect.TabIndex = 10;
			this.webConnect.Text = "button1";
			// 
			// emailSelect
			// 
			this.emailSelect.BackColor = System.Drawing.SystemColors.Control;
			this.emailSelect.Items.AddRange(new object[] {
															 "Business email:",
															 "Personal email:",
															 "Other email:"});
			this.emailSelect.Location = new System.Drawing.Point(64, 160);
			this.emailSelect.Name = "emailSelect";
			this.emailSelect.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.emailSelect.Size = new System.Drawing.Size(104, 21);
			this.emailSelect.TabIndex = 6;
			this.emailSelect.Text = "Business email:";
			this.emailSelect.SelectedIndexChanged += new System.EventHandler(this.emailSelect_SelectedIndexChanged);
			// 
			// blogAddress
			// 
			this.blogAddress.Location = new System.Drawing.Point(176, 256);
			this.blogAddress.Name = "blogAddress";
			this.blogAddress.Size = new System.Drawing.Size(168, 20);
			this.blogAddress.TabIndex = 11;
			this.blogAddress.Text = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(104, 256);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 16);
			this.label5.TabIndex = 51;
			this.label5.Text = "Blog Address:";
			// 
			// addr
			// 
			this.addr.Location = new System.Drawing.Point(472, 192);
			this.addr.Name = "addr";
			this.addr.Size = new System.Drawing.Size(96, 23);
			this.addr.TabIndex = 22;
			this.addr.Text = "Address...";
			// 
			// groupBox4
			// 
			this.groupBox4.Location = new System.Drawing.Point(400, 144);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(368, 4);
			this.groupBox4.TabIndex = 48;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "groupBox4";
			// 
			// pictureWeb
			// 
			this.pictureWeb.Location = new System.Drawing.Point(8, 224);
			this.pictureWeb.Name = "pictureWeb";
			this.pictureWeb.Size = new System.Drawing.Size(48, 48);
			this.pictureWeb.TabIndex = 46;
			this.pictureWeb.TabStop = false;
			// 
			// webAddress
			// 
			this.webAddress.Location = new System.Drawing.Point(176, 224);
			this.webAddress.Name = "webAddress";
			this.webAddress.Size = new System.Drawing.Size(168, 20);
			this.webAddress.TabIndex = 9;
			this.webAddress.Text = "";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(72, 224);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(104, 16);
			this.label4.TabIndex = 45;
			this.label4.Text = "Web page address:";
			// 
			// groupBox2
			// 
			this.groupBox2.Location = new System.Drawing.Point(8, 208);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(376, 4);
			this.groupBox2.TabIndex = 43;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "groupBox2";
			// 
			// mailAddress
			// 
			this.mailAddress.Location = new System.Drawing.Point(584, 264);
			this.mailAddress.Name = "mailAddress";
			this.mailAddress.Size = new System.Drawing.Size(160, 16);
			this.mailAddress.TabIndex = 24;
			this.mailAddress.Text = "This is the mailing address";
			// 
			// addressSelect
			// 
			this.addressSelect.BackColor = System.Drawing.SystemColors.Control;
			this.addressSelect.Items.AddRange(new object[] {
															   "Business:",
															   "Home:"});
			this.addressSelect.Location = new System.Drawing.Point(472, 160);
			this.addressSelect.Name = "addressSelect";
			this.addressSelect.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.addressSelect.Size = new System.Drawing.Size(96, 21);
			this.addressSelect.TabIndex = 21;
			this.addressSelect.Text = "Business:";
			// 
			// pictureAddress
			// 
			this.pictureAddress.Location = new System.Drawing.Point(408, 168);
			this.pictureAddress.Name = "pictureAddress";
			this.pictureAddress.Size = new System.Drawing.Size(48, 48);
			this.pictureAddress.TabIndex = 40;
			this.pictureAddress.TabStop = false;
			// 
			// address
			// 
			this.address.Location = new System.Drawing.Point(576, 160);
			this.address.Name = "address";
			this.address.Size = new System.Drawing.Size(192, 95);
			this.address.TabIndex = 23;
			// 
			// mailHTML
			// 
			this.mailHTML.Location = new System.Drawing.Point(176, 184);
			this.mailHTML.Name = "mailHTML";
			this.mailHTML.Size = new System.Drawing.Size(168, 16);
			this.mailHTML.TabIndex = 8;
			this.mailHTML.Text = "Wants to receive HTML mail";
			// 
			// phoneSelect3
			// 
			this.phoneSelect3.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect3.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Pager:"});
			this.phoneSelect3.Location = new System.Drawing.Point(472, 80);
			this.phoneSelect3.Name = "phoneSelect3";
			this.phoneSelect3.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.phoneSelect3.Size = new System.Drawing.Size(96, 21);
			this.phoneSelect3.TabIndex = 17;
			this.phoneSelect3.Text = "Home:";
			this.phoneSelect3.SelectedIndexChanged += new System.EventHandler(this.phoneSelect3_SelectedIndexChanged);
			// 
			// phoneSelect4
			// 
			this.phoneSelect4.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect4.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Pager:"});
			this.phoneSelect4.Location = new System.Drawing.Point(472, 112);
			this.phoneSelect4.Name = "phoneSelect4";
			this.phoneSelect4.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.phoneSelect4.Size = new System.Drawing.Size(96, 21);
			this.phoneSelect4.TabIndex = 19;
			this.phoneSelect4.Text = "Mobile:";
			this.phoneSelect4.SelectedIndexChanged += new System.EventHandler(this.phoneSelect4_SelectedIndexChanged);
			// 
			// picturePhone
			// 
			this.picturePhone.Location = new System.Drawing.Point(408, 16);
			this.picturePhone.Name = "picturePhone";
			this.picturePhone.Size = new System.Drawing.Size(48, 48);
			this.picturePhone.TabIndex = 35;
			this.picturePhone.TabStop = false;
			// 
			// phoneSelect1
			// 
			this.phoneSelect1.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect1.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Pager:"});
			this.phoneSelect1.Location = new System.Drawing.Point(472, 16);
			this.phoneSelect1.Name = "phoneSelect1";
			this.phoneSelect1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.phoneSelect1.Size = new System.Drawing.Size(96, 21);
			this.phoneSelect1.TabIndex = 13;
			this.phoneSelect1.Text = "Business:";
			this.phoneSelect1.SelectedIndexChanged += new System.EventHandler(this.phoneSelect1_SelectedIndexChanged);
			// 
			// phoneSelect2
			// 
			this.phoneSelect2.BackColor = System.Drawing.SystemColors.Control;
			this.phoneSelect2.Items.AddRange(new object[] {
															  "Business:",
															  "Business fax:",
															  "Mobile:",
															  "Home:",
															  "Pager:"});
			this.phoneSelect2.Location = new System.Drawing.Point(472, 48);
			this.phoneSelect2.Name = "phoneSelect2";
			this.phoneSelect2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.phoneSelect2.Size = new System.Drawing.Size(96, 21);
			this.phoneSelect2.TabIndex = 15;
			this.phoneSelect2.Text = "Business fax:";
			this.phoneSelect2.SelectedIndexChanged += new System.EventHandler(this.phoneSelect2_SelectedIndexChanged);
			// 
			// userId
			// 
			this.userId.Location = new System.Drawing.Point(176, 112);
			this.userId.Name = "userId";
			this.userId.Size = new System.Drawing.Size(192, 20);
			this.userId.TabIndex = 5;
			this.userId.Text = "";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(128, 112);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 16);
			this.label7.TabIndex = 30;
			this.label7.Text = "User ID:";
			// 
			// phone4
			// 
			this.phone4.Location = new System.Drawing.Point(576, 112);
			this.phone4.Name = "phone4";
			this.phone4.Size = new System.Drawing.Size(192, 20);
			this.phone4.TabIndex = 20;
			this.phone4.Text = "";
			this.phone4.TextChanged += new System.EventHandler(this.phone4_TextChanged);
			this.phone4.Leave += new System.EventHandler(this.phone4_Leave);
			// 
			// phone3
			// 
			this.phone3.Location = new System.Drawing.Point(576, 80);
			this.phone3.Name = "phone3";
			this.phone3.Size = new System.Drawing.Size(192, 20);
			this.phone3.TabIndex = 18;
			this.phone3.Text = "";
			this.phone3.TextChanged += new System.EventHandler(this.phone3_TextChanged);
			this.phone3.Leave += new System.EventHandler(this.phone3_Leave);
			// 
			// phone2
			// 
			this.phone2.Location = new System.Drawing.Point(576, 48);
			this.phone2.Name = "phone2";
			this.phone2.Size = new System.Drawing.Size(192, 20);
			this.phone2.TabIndex = 16;
			this.phone2.Text = "";
			this.phone2.TextChanged += new System.EventHandler(this.phone2_TextChanged);
			this.phone2.Leave += new System.EventHandler(this.phone2_Leave);
			// 
			// phone1
			// 
			this.phone1.Location = new System.Drawing.Point(576, 16);
			this.phone1.Name = "phone1";
			this.phone1.Size = new System.Drawing.Size(192, 20);
			this.phone1.TabIndex = 14;
			this.phone1.Text = "";
			this.phone1.TextChanged += new System.EventHandler(this.phone1_TextChanged);
			this.phone1.Leave += new System.EventHandler(this.phone1_Leave);
			// 
			// pictureMail
			// 
			this.pictureMail.Location = new System.Drawing.Point(8, 152);
			this.pictureMail.Name = "pictureMail";
			this.pictureMail.Size = new System.Drawing.Size(48, 48);
			this.pictureMail.TabIndex = 21;
			this.pictureMail.TabStop = false;
			// 
			// eMail
			// 
			this.eMail.Location = new System.Drawing.Point(176, 160);
			this.eMail.Name = "eMail";
			this.eMail.Size = new System.Drawing.Size(192, 20);
			this.eMail.TabIndex = 7;
			this.eMail.Text = "";
			this.eMail.Leave += new System.EventHandler(this.eMail_Leave);
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(8, 144);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(376, 4);
			this.groupBox1.TabIndex = 18;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "groupBox1";
			// 
			// pictureContact
			// 
			this.pictureContact.Location = new System.Drawing.Point(16, 16);
			this.pictureContact.Name = "pictureContact";
			this.pictureContact.Size = new System.Drawing.Size(64, 64);
			this.pictureContact.TabIndex = 17;
			this.pictureContact.TabStop = false;
			// 
			// organization
			// 
			this.organization.Location = new System.Drawing.Point(176, 80);
			this.organization.Name = "organization";
			this.organization.Size = new System.Drawing.Size(192, 20);
			this.organization.TabIndex = 3;
			this.organization.Text = "";
			// 
			// jobTitle
			// 
			this.jobTitle.Location = new System.Drawing.Point(176, 48);
			this.jobTitle.Name = "jobTitle";
			this.jobTitle.Size = new System.Drawing.Size(192, 20);
			this.jobTitle.TabIndex = 2;
			this.jobTitle.Text = "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(104, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 16);
			this.label3.TabIndex = 6;
			this.label3.Text = "Organization:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(128, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 16);
			this.label2.TabIndex = 5;
			this.label2.Text = "Job title:";
			// 
			// fullName
			// 
			this.fullName.Location = new System.Drawing.Point(176, 16);
			this.fullName.Name = "fullName";
			this.fullName.ReadOnly = true;
			this.fullName.Size = new System.Drawing.Size(192, 20);
			this.fullName.TabIndex = 1;
			this.fullName.Text = "";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(624, 320);
			this.ok.Name = "ok";
			this.ok.TabIndex = 1;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(704, 320);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 2;
			this.cancel.Text = "Cancel";
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// ContactEditor
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(786, 352);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ContactEditor";
			this.ShowInTaskbar = false;
			this.Text = "Contact Editor";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ContactEditor_Closing);
			this.Load += new System.EventHandler(this.ContactEditor_Load);
			this.Activated += new System.EventHandler(this.ContactEditor_Activated);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the contact that is being edited.
		/// </summary>
		public Contact CurrentContact
		{
			get
			{
				return contact;
			}

			set
			{
				contact = value;
			}
		}

		/// <summary>
		/// Sets the address book that the contact belongs to.
		/// </summary>
		public Novell.AddressBook.AddressBook CurrentAddressBook
		{
			set
			{
				this.addressBook = value;
			}
		}
		#endregion

		#region Private Methods
		private bool LoadAddresses()
		{
			try
			{
				// Get the e-mail addresses.
				foreach(Email tmpMail in contact.GetEmailAddresses())
				{
					EmailEntry entry = new EmailEntry();
					entry.Add = false;
					entry.EMail = tmpMail;

					if ((tmpMail.Types & EmailTypes.work) == EmailTypes.work)
					{
						emailHT.Add((string)emailSelect.Items[0], entry);

						if ((tmpMail.Types & EmailTypes.preferred) == EmailTypes.preferred)
						{
							emailSelect.SelectedIndex = 0;
							eMail.Text = tmpMail.Address;
						}
					}
					else if ((tmpMail.Types & EmailTypes.personal) == EmailTypes.personal)
					{
						emailHT.Add((string)emailSelect.Items[1], entry);
						if ((tmpMail.Types & EmailTypes.preferred) == EmailTypes.preferred)
						{
							emailSelect.SelectedIndex = 1;
							eMail.Text = tmpMail.Address;
						}
					}
					else if ((tmpMail.Types & EmailTypes.other) == EmailTypes.other)
					{
						emailHT.Add((string)emailSelect.Items[2], entry);
						if ((tmpMail.Types & EmailTypes.preferred) == EmailTypes.preferred)
						{
							emailSelect.SelectedIndex = 2;
							eMail.Text = tmpMail.Address;
						}
					}
				}
			}
			catch{}

			// Deal with phone numbers
			try
			{
				foreach(Telephone tmpPhone in contact.GetTelephoneNumbers())
				{
					TelephoneEntry phone = new TelephoneEntry();
					phone.Add = false;
					phone.Phone = tmpPhone;

					if ((tmpPhone.Types & (PhoneTypes.work | PhoneTypes.voice)) == (PhoneTypes.work | PhoneTypes.voice))
					{
						phoneHT.Add((string)this.phoneSelect1.Items[0], phone);
						SetPhoneInEdit(0, tmpPhone.Number);
					}
					else if ((tmpPhone.Types & (PhoneTypes.work | PhoneTypes.fax)) == (PhoneTypes.work | PhoneTypes.fax))
					{
						phoneHT.Add((string)this.phoneSelect1.Items[1], phone);
						SetPhoneInEdit(1, tmpPhone.Number);
					}
					else if ((tmpPhone.Types & PhoneTypes.cell) == PhoneTypes.cell)
					{
						phoneHT.Add((string)this.phoneSelect1.Items[2], phone);
						SetPhoneInEdit(2, tmpPhone.Number);
					}
					else if ((tmpPhone.Types & (PhoneTypes.home | PhoneTypes.voice)) == (PhoneTypes.home | PhoneTypes.voice))
					{
						phoneHT.Add((string)this.phoneSelect1.Items[3], phone);
						SetPhoneInEdit(3, tmpPhone.Number);
					}
					else if ((tmpPhone.Types & PhoneTypes.pager) == PhoneTypes.pager)
					{
						phoneHT.Add((string)this.phoneSelect1.Items[4], phone);
						SetPhoneInEdit(4, tmpPhone.Number);
					}
				}
			}
			catch{}

/*			foreach(Address addr in contact.GetAddresses())
			{
				if((addr.Types & AddressTypes.work) == AddressTypes.work)
				{
					bStreetEdit.Text = addr.Street;
					bCityEdit.Text = addr.Locality;
					bStateEdit.Text = addr.Region;
					bZipEdit.Text = addr.PostalCode;
					bCountryEdit.Text = addr.Country;
					foundWork = true;
				}
				else
					if((addr.Types & AddressTypes.home) == AddressTypes.home)
				{
					hStreetEdit.Text = addr.Street;
					hCityEdit.Text = addr.Locality;
					hStateEdit.Text = addr.Region;
					hZipEdit.Text = addr.PostalCode;
					hCountryEdit.Text = addr.Country;
					foundHome = true;
				}
			}*/

			return(true);
		}

		private void SetPhoneInEdit(int index, string number)
		{
			if (phoneSelect1.SelectedIndex == index)
				phone1.Text = number;
			
			if (phoneSelect2.SelectedIndex == index)
				phone2.Text = number;

			if (phoneSelect3.SelectedIndex == index)
				phone3.Text = number;

			if (phoneSelect4.SelectedIndex == index)
				phone4.Text = number;
		}

		private void UpdatePhoneTable(ComboBox select, TextBox phone)
		{
			TelephoneEntry entry = (TelephoneEntry)phoneHT[select.Text];
			if (entry != null)
			{
				if (entry.Phone.Number != phone.Text)
				{
					phoneHT.Remove(select.Text);

					if ((phone.Text == "") && !entry.Add)
					{
						entry.Remove = true;
					}

					entry.Phone.Number = phone.Text;
					phoneHT.Add(select.Text, entry);
				}
			}
			else
			{
				if (phone.Text != "")
				{
					entry = new TelephoneEntry();
					entry.Add = true;
					entry.Phone = new Telephone(phone.Text);

					switch (select.SelectedIndex)
					{
						case 0:
							entry.Phone.Types = (PhoneTypes.preferred | PhoneTypes.work | PhoneTypes.voice);
							break;
						case 1:
							entry.Phone.Types = (PhoneTypes.work | PhoneTypes.fax);
							break;
						case 2:
							entry.Phone.Types = (PhoneTypes.cell | PhoneTypes.voice);
							break;
						case 3:
							entry.Phone.Types = (PhoneTypes.home | PhoneTypes.voice);
							break;
						case 4:
							entry.Phone.Types = PhoneTypes.pager;
							break;
						default:
							entry.Phone.Types = 0;
							break;
					}

					phoneHT.Add(select.Text, entry);
				}
			}
		}

		private string BuildDisplayableName(Name name)
		{
			string displayName = name.Prefix + " " + name.Given + " " + name.Family + " " + name.Suffix;
			return displayName.Trim();
		}
		#endregion

		#region Event Handlers
		private void ContactEditor_Load(object sender, EventArgs e)
		{
			if (contact != null)
			{
				// Initialize the dialog with the specified contact.
				userId.Text = this.contact.UserName;

				try
				{
					name = this.contact.GetPreferredName();
					fullName.Text = BuildDisplayableName(name);
				}
				catch
				{
					fullName.Text = "";
				}

				jobTitle.Text = contact.Title;
				organization.Text = contact.Organization;
				webAddress.Text = contact.Url;
				blogAddress.Text = contact.Blog;

				bool results = LoadAddresses();
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			if (phone1.Focused)
				UpdatePhoneTable(phoneSelect1, phone1);
			else if (phone2.Focused)
				UpdatePhoneTable(phoneSelect2, phone2);
			else if (phone3.Focused)
				UpdatePhoneTable(phoneSelect3, phone3);
			else if (phone4.Focused)
				UpdatePhoneTable(phoneSelect4, phone4);
			else if (eMail.Focused)
				eMail_Leave(this, null);

			string	username = null;
			string	email = null;

			username = userId.Text.Trim();

			// TODO - will e-mail address be a required attribute?
//			email = eMail.Text.Trim();

			if (username != "" &&
//				email != "" &&
				name != null &&
				name.Given != null &&
				name.Given != "" &&
				name.Family != null &&
				name.Family != "")
			{
				if (contact == null)
				{
					// This is a new contact.
					contact = new Contact();
					contact.UserName = username;

					try
					{
						name.Preferred = true;
						contact.AddName(name);
					}
					catch{}

					IDictionaryEnumerator enumerator = emailHT.GetEnumerator();
					while (enumerator.MoveNext())
					{
						contact.AddEmailAddress(((EmailEntry)enumerator.Value).EMail);
					}

					// Add the phone numbers.
					enumerator = phoneHT.GetEnumerator();
					while (enumerator.MoveNext())
					{
						contact.AddTelephoneNumber(((TelephoneEntry)enumerator.Value).Phone);
					}

					contact.Organization = organization.Text.Trim();
					contact.Title = jobTitle.Text.Trim();
					contact.Url = webAddress.Text.Trim();
					contact.Blog = blogAddress.Text.Trim();

					addressBook.AddContact(contact);
					contact.Commit();
				}
				else
				{
					// Update email.
					try
					{
						IDictionaryEnumerator enumerator = emailHT.GetEnumerator();
						while (enumerator.MoveNext())
						{
							if (((EmailEntry)enumerator.Value).Add)
							{
								contact.AddEmailAddress(((EmailEntry)enumerator.Value).EMail);
							}
						}
					}
					catch{}

					// Update phone numbers.
					try
					{
						IDictionaryEnumerator enumerator = phoneHT.GetEnumerator();
						while (enumerator.MoveNext())
						{
							if (((TelephoneEntry)enumerator.Value).Add)
							{
								contact.AddTelephoneNumber(((TelephoneEntry)enumerator.Value).Phone);
							}
						}
					}
					catch{}

					if (contact.UserName != username)
					{
						contact.UserName = username;
					}

					contact.Title = jobTitle.Text.Trim();
					contact.Organization = organization.Text.Trim();
					contact.Url = webAddress.Text.Trim();
					contact.Blog = blogAddress.Text.Trim();

					contact.Commit();
				}
			}
		}

		private void ContactEditor_Closing(object sender, CancelEventArgs e)
		{
			string user = this.userId.Text.Trim();
			// TODO - will e-mail address be a required attribute?
//			string email = this.eMail.Text.Trim();

			// Make sure the mandatory fields are filled in if OK was clicked.
			if (this.DialogResult == DialogResult.OK &&
				(user == "" ||
//				email == "" ||
				name == null ||
				((name != null) && ((name.Given == null || name.Given == "") ||	(name.Family == null || name.Family == "")))))
			{
				MessageBox.Show("User ID, first name, last name, and e-mail are required attributes.", "Missing Required Attributes", MessageBoxButtons.OK);

				// Set the focus to the edit field that needs filled in.
				if (user == "")
				{
					this.userId.Focus();
				}
				else if (name == null ||
					(name != null) && ((name.Given == null || name.Given == "") || (name.Family == null || name.Family == "")))
				{
					// TODO - hmmm ...
					fullNameButton_Click(this, null);
				}
				else
				{
					this.eMail.Focus();
				}
				
				// Don't dismiss the dialog.
				e.Cancel = true;
			}
		}

		private void ContactEditor_Activated(object sender, EventArgs e)
		{
			// Set focus to the first edit box on the form.
			this.fullNameButton.Focus();
		}

		private void phone1_Leave(object sender, System.EventArgs e)
		{
			UpdatePhoneTable(phoneSelect1, phone1);
		}

		private void phone2_Leave(object sender, System.EventArgs e)
		{
			UpdatePhoneTable(phoneSelect2, phone2);
		}

		private void phone3_Leave(object sender, System.EventArgs e)
		{
			UpdatePhoneTable(phoneSelect3, phone3);
		}

		private void phone4_Leave(object sender, System.EventArgs e)
		{
			UpdatePhoneTable(phoneSelect4, phone4);
		}

		private void emailSelect_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			EmailEntry entry = (EmailEntry)emailHT[emailSelect.Text];
			if ((entry != null) && !entry.Remove)
			{
				eMail.Text = entry.EMail.Address;
			}
			else
			{
				eMail.Text = "";
			}
		}

		private void eMail_Leave(object sender, System.EventArgs e)
		{
			EmailEntry entry = (EmailEntry)emailHT[emailSelect.Text];
			if (entry != null)
			{
				if (entry.EMail.Address != eMail.Text)
				{
					emailHT.Remove(emailSelect.Text);

					if ((eMail.Text == "") && !entry.Add)
					{
						entry.Remove = true;

						// If this was the preferred address, we need to make a different
						// address the preferred one.
						// TODO - preferred should probably be added to the UI
						if ((entry.EMail.Types & EmailTypes.preferred) == EmailTypes.preferred)
						{
							if (emailHT.Count > 0)
							{
								bool preferredSet = false;
								IEnumerator enumerator = emailHT.Values.GetEnumerator();

								while (enumerator.MoveNext())
								{
									EmailEntry ee = (EmailEntry)enumerator.Current;
									if ((ee.EMail.Types & EmailTypes.work) == EmailTypes.work)
									{
										ee.EMail.Types |= EmailTypes.preferred;
										preferredSet = true;
									}
								}

								if (!preferredSet)
								{
									enumerator.Reset();
									enumerator.MoveNext();
									((EmailEntry)enumerator.Current).EMail.Types |= EmailTypes.preferred;
								}
							}
						}
					}

					entry.EMail.Address = eMail.Text;
					emailHT.Add(emailSelect.Text, entry);
				}
			}
			else
			{
				if (eMail.Text != "")
				{
					entry = new EmailEntry();
					entry.Add = true;
					entry.EMail = new Email();
					entry.EMail.Address = eMail.Text.Trim();

					switch (emailSelect.SelectedIndex)
					{
						case 0:
							entry.EMail.Types = EmailTypes.work;
							break;
						case 1:
							entry.EMail.Types = EmailTypes.personal;
							break;
						case 2:
							entry.EMail.Types = EmailTypes.other;
							break;
						default:
							entry.EMail.Types = 0;
							break;
					}

					// The first entry is set as the preferred address.
					// TODO - preferred should probably be added to the UI
					if (emailHT.Count == 0)
						entry.EMail.Types |= EmailTypes.preferred;

					emailHT.Add(emailSelect.Text, entry);
				}
			}
		}

		private void phoneSelect1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TelephoneEntry entry = (TelephoneEntry)phoneHT[this.phoneSelect1.Text];
			if ((entry != null) && !entry.Remove)
			{
				phone1.Text = entry.Phone.Number;
			}
			else
			{
				phone1.Text = "";
			}
		}

		private void phoneSelect2_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TelephoneEntry entry = (TelephoneEntry)phoneHT[this.phoneSelect2.Text];
			if ((entry != null) && !entry.Remove)
			{
				phone2.Text = entry.Phone.Number;
			}
			else
			{
				phone2.Text = "";
			}
		}

		private void phoneSelect3_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TelephoneEntry entry = (TelephoneEntry)phoneHT[this.phoneSelect3.Text];
			if ((entry != null) && !entry.Remove)
			{
				phone3.Text = entry.Phone.Number;
			}
			else
			{
				phone3.Text = "";
			}
		}

		private void phoneSelect4_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TelephoneEntry entry = (TelephoneEntry)phoneHT[this.phoneSelect4.Text];
			if ((entry != null) && !entry.Remove)
			{
				phone4.Text = entry.Phone.Number;
			}
			else
			{
				phone4.Text = "";
			}
		}

		private void fullNameButton_Click(object sender, System.EventArgs e)
		{
			FullName fullNameDlg = new FullName();
			// Initialize.
			if (name == null)
				name = new Name();

			fullNameDlg.Title = name.Prefix;
			fullNameDlg.FirstName = name.Given;
			fullNameDlg.LastName = name.Family;
			fullNameDlg.Suffix = name.Suffix;

			DialogResult result = fullNameDlg.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Save the information.
				name.Prefix = fullNameDlg.Title.Trim();
				name.Given = fullNameDlg.FirstName.Trim();
				name.Family = fullNameDlg.LastName.Trim();
				name.Suffix = fullNameDlg.Suffix.Trim();

				fullName.Text = BuildDisplayableName(name);
			}
		}

		private void phone1_TextChanged(object sender, System.EventArgs e)
		{
			// Make changes echo to any other edit box that is on the same setting.
			if (this.phoneSelect2.SelectedIndex == this.phoneSelect1.SelectedIndex)
			{
				phone2.Text = phone1.Text;
			}
		
			if (this.phoneSelect3.SelectedIndex == this.phoneSelect1.SelectedIndex)
			{
				phone3.Text = phone1.Text;
			}

			if (this.phoneSelect4.SelectedIndex == this.phoneSelect1.SelectedIndex)
			{
				phone4.Text = phone1.Text;
			}
		}

		private void phone2_TextChanged(object sender, System.EventArgs e)
		{
			// Make changes echo to any other edit box that is on the same setting.
			if (this.phoneSelect1.SelectedIndex == this.phoneSelect2.SelectedIndex)
			{
				phone1.Text = phone2.Text;
			}
		
			if (this.phoneSelect3.SelectedIndex == this.phoneSelect2.SelectedIndex)
			{
				phone3.Text = phone2.Text;
			}

			if (this.phoneSelect4.SelectedIndex == this.phoneSelect2.SelectedIndex)
			{
				phone4.Text = phone2.Text;
			}
		}

		private void phone3_TextChanged(object sender, System.EventArgs e)
		{
			// Make changes echo to any other edit box that is on the same setting.
			if (this.phoneSelect1.SelectedIndex == this.phoneSelect3.SelectedIndex)
			{
				phone1.Text = phone3.Text;
			}
		
			if (this.phoneSelect2.SelectedIndex == this.phoneSelect3.SelectedIndex)
			{
				phone2.Text = phone3.Text;
			}

			if (this.phoneSelect4.SelectedIndex == this.phoneSelect3.SelectedIndex)
			{
				phone4.Text = phone3.Text;
			}
		}

		private void phone4_TextChanged(object sender, System.EventArgs e)
		{
			// Make changes echo to any other edit box that is on the same setting.
			if (this.phoneSelect1.SelectedIndex == this.phoneSelect4.SelectedIndex)
			{
				phone1.Text = phone4.Text;
			}
		
			if (this.phoneSelect2.SelectedIndex == this.phoneSelect4.SelectedIndex)
			{
				phone2.Text = phone4.Text;
			}

			if (this.phoneSelect3.SelectedIndex == this.phoneSelect4.SelectedIndex)
			{
				phone3.Text = phone4.Text;
			}
		}
		#endregion

		private void cancel_Click(object sender, System.EventArgs e)
		{
//			contact.Rollback();
		}
	}
}
