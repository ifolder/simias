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
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Simias;
using Simias.Sync;
using Simias.Service;
using Simias.Storage;
using Novell.iFolder;
using Novell.iFolder.iFolderCom;
using Novell.iFolder.Win32Util;

namespace Novell.iFolder.FormsTrayApp
{
	/// <summary>
	/// Summary description for GlobalProperties.
	/// </summary>
	public class GlobalProperties : System.Windows.Forms.Form
	{
		#region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(GlobalProperties));
		private const string iFolderRun = "iFolder";

		const string CFG_Section = "ServiceManager";
		const string CFG_Services = "Services";
		const string XmlServiceTag = "Service";

		private Hashtable ht;
		private EventSubscriber subscriber;
		private Simias.Service.Manager serviceManager = null;
		private iFolderManager manager = null;
		private Configuration config;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown defaultInterval;
		private System.Windows.Forms.CheckBox displayConfirmation;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox autoSync;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.PictureBox banner;
		private System.Windows.Forms.CheckBox autoStart;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView iFolderView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button syncNow;
		private System.Windows.Forms.ListBox log;
		private System.Windows.Forms.Button saveLog;
		private System.Windows.Forms.Button clearLog;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuOpen;
		private System.Windows.Forms.MenuItem menuCreate;
		private System.Windows.Forms.Label objectCount;
		private System.Windows.Forms.Label byteCount;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.ListView services;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ContextMenu contextMenu2;
		private System.Windows.Forms.MenuItem menuStart;
		private System.Windows.Forms.MenuItem menuPause;
		private System.Windows.Forms.MenuItem menuStop;
		private System.Windows.Forms.MenuItem menuRestart;
		private System.Windows.Forms.MenuItem menuShare;
		private System.Windows.Forms.MenuItem menuRevert;
		private System.Windows.Forms.MenuItem menuProperties;
		private System.Windows.Forms.MenuItem menuRefresh;
		private System.Windows.Forms.MenuItem menuSeparator1;
		private System.Windows.Forms.MenuItem menuSeparator2;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.MenuItem menuSyncNow;
		private System.Windows.Forms.MenuItem menuEnabled;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuFile;
		private System.Windows.Forms.MenuItem menuFileExit;
		private System.Windows.Forms.MenuItem menuAction;
		private System.Windows.Forms.MenuItem menuView;
		private System.Windows.Forms.MenuItem menuViewRefresh;
		private System.Windows.Forms.MenuItem menuActionOpen;
		private System.Windows.Forms.MenuItem menuActionCreate;
		private System.Windows.Forms.MenuItem menuActionRevert;
		private System.Windows.Forms.MenuItem menuActionShare;
		private System.Windows.Forms.MenuItem menuActionSync;
		private System.Windows.Forms.MenuItem menuActionProperties;
		private System.Windows.Forms.MenuItem menuActionEnable;
		private System.Windows.Forms.MenuItem menuActionStart;
		private System.Windows.Forms.MenuItem menuActionStop;
		private System.Windows.Forms.MenuItem menuActionPause;
		private System.Windows.Forms.MenuItem menuActionRestart;
		private System.Windows.Forms.MenuItem menuActionSeparator1;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.RadioButton noProxy;
		private System.Windows.Forms.RadioButton useProxy;
		private System.Windows.Forms.TextBox proxy;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox port;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Instantiates a GlobalProperties object.
		/// </summary>
		public GlobalProperties(Configuration config)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.config = config;

			// Set up the event handlers to watch for iFolder creates/deletes.
			subscriber = new EventSubscriber();
			subscriber.NodeChanged += new NodeEventHandler(subscriber_NodeChanged);
			subscriber.NodeCreated += new NodeEventHandler(subscriber_NodeCreated);
			subscriber.NodeDeleted += new NodeEventHandler(subscriber_NodeDeleted);

			ht = new Hashtable();

/*			try
			{
				manager = iFolderManager.Connect();
			}
			catch (SimiasException e)
			{
				e.LogFatal();
			}
			catch (Exception e)
			{
				logger.Fatal(e, "Fatal error initializing");
			}*/
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (subscriber != null)
				{
					subscriber.Dispose();
				}

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
			this.label1 = new System.Windows.Forms.Label();
			this.defaultInterval = new System.Windows.Forms.NumericUpDown();
			this.displayConfirmation = new System.Windows.Forms.CheckBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.objectCount = new System.Windows.Forms.Label();
			this.byteCount = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.iFolderView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuOpen = new System.Windows.Forms.MenuItem();
			this.menuCreate = new System.Windows.Forms.MenuItem();
			this.menuRefresh = new System.Windows.Forms.MenuItem();
			this.menuSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuRevert = new System.Windows.Forms.MenuItem();
			this.menuShare = new System.Windows.Forms.MenuItem();
			this.menuSyncNow = new System.Windows.Forms.MenuItem();
			this.menuSeparator2 = new System.Windows.Forms.MenuItem();
			this.menuProperties = new System.Windows.Forms.MenuItem();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.autoSync = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.autoStart = new System.Windows.Forms.CheckBox();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.clearLog = new System.Windows.Forms.Button();
			this.saveLog = new System.Windows.Forms.Button();
			this.log = new System.Windows.Forms.ListBox();
			this.syncNow = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.services = new System.Windows.Forms.ListView();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.contextMenu2 = new System.Windows.Forms.ContextMenu();
			this.menuEnabled = new System.Windows.Forms.MenuItem();
			this.menuStart = new System.Windows.Forms.MenuItem();
			this.menuPause = new System.Windows.Forms.MenuItem();
			this.menuStop = new System.Windows.Forms.MenuItem();
			this.menuRestart = new System.Windows.Forms.MenuItem();
			this.banner = new System.Windows.Forms.PictureBox();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuFile = new System.Windows.Forms.MenuItem();
			this.menuFileExit = new System.Windows.Forms.MenuItem();
			this.menuAction = new System.Windows.Forms.MenuItem();
			this.menuActionCreate = new System.Windows.Forms.MenuItem();
			this.menuActionSeparator1 = new System.Windows.Forms.MenuItem();
			this.menuActionOpen = new System.Windows.Forms.MenuItem();
			this.menuActionRevert = new System.Windows.Forms.MenuItem();
			this.menuActionShare = new System.Windows.Forms.MenuItem();
			this.menuActionSync = new System.Windows.Forms.MenuItem();
			this.menuActionProperties = new System.Windows.Forms.MenuItem();
			this.menuActionEnable = new System.Windows.Forms.MenuItem();
			this.menuActionStart = new System.Windows.Forms.MenuItem();
			this.menuActionPause = new System.Windows.Forms.MenuItem();
			this.menuActionStop = new System.Windows.Forms.MenuItem();
			this.menuActionRestart = new System.Windows.Forms.MenuItem();
			this.menuView = new System.Windows.Forms.MenuItem();
			this.menuViewRefresh = new System.Windows.Forms.MenuItem();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.noProxy = new System.Windows.Forms.RadioButton();
			this.useProxy = new System.Windows.Forms.RadioButton();
			this.proxy = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.port = new System.Windows.Forms.TextBox();
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.tabPage5.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 80);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(184, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Sync to host every:";
			// 
			// defaultInterval
			// 
			this.defaultInterval.Increment = new System.Decimal(new int[] {
																			  5,
																			  0,
																			  0,
																			  0});
			this.defaultInterval.Location = new System.Drawing.Point(144, 80);
			this.defaultInterval.Maximum = new System.Decimal(new int[] {
																			86400,
																			0,
																			0,
																			0});
			this.defaultInterval.Minimum = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			-2147483648});
			this.defaultInterval.Name = "defaultInterval";
			this.defaultInterval.Size = new System.Drawing.Size(64, 20);
			this.defaultInterval.TabIndex = 2;
			// 
			// displayConfirmation
			// 
			this.displayConfirmation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.displayConfirmation.Location = new System.Drawing.Point(16, 48);
			this.displayConfirmation.Name = "displayConfirmation";
			this.displayConfirmation.Size = new System.Drawing.Size(368, 24);
			this.displayConfirmation.TabIndex = 1;
			this.displayConfirmation.Text = "&Display iFolder creation confirmation.";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(288, 432);
			this.ok.Name = "ok";
			this.ok.TabIndex = 5;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(368, 432);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 6;
			this.cancel.Text = "Cancel";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(216, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "seconds";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Location = new System.Drawing.Point(8, 65);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(434, 359);
			this.tabControl1.TabIndex = 8;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.groupBox4);
			this.tabPage1.Controls.Add(this.iFolderView);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(426, 333);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "iFolders";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.objectCount);
			this.groupBox4.Controls.Add(this.byteCount);
			this.groupBox4.Controls.Add(this.label5);
			this.groupBox4.Controls.Add(this.label4);
			this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox4.Location = new System.Drawing.Point(8, 240);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(408, 72);
			this.groupBox4.TabIndex = 1;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Synchronization";
			// 
			// objectCount
			// 
			this.objectCount.Location = new System.Drawing.Point(296, 48);
			this.objectCount.Name = "objectCount";
			this.objectCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.objectCount.Size = new System.Drawing.Size(100, 16);
			this.objectCount.TabIndex = 3;
			// 
			// byteCount
			// 
			this.byteCount.Location = new System.Drawing.Point(296, 24);
			this.byteCount.Name = "byteCount";
			this.byteCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.byteCount.Size = new System.Drawing.Size(100, 16);
			this.byteCount.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(272, 16);
			this.label5.TabIndex = 2;
			this.label5.Text = "Files/folders to synchronize:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(272, 16);
			this.label4.TabIndex = 0;
			this.label4.Text = "Amount to upload:";
			// 
			// iFolderView
			// 
			this.iFolderView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.columnHeader1,
																						  this.columnHeader4,
																						  this.columnHeader5});
			this.iFolderView.ContextMenu = this.contextMenu1;
			this.iFolderView.FullRowSelect = true;
			this.iFolderView.HideSelection = false;
			this.iFolderView.Location = new System.Drawing.Point(8, 16);
			this.iFolderView.MultiSelect = false;
			this.iFolderView.Name = "iFolderView";
			this.iFolderView.Size = new System.Drawing.Size(408, 208);
			this.iFolderView.TabIndex = 0;
			this.iFolderView.View = System.Windows.Forms.View.Details;
			this.iFolderView.DoubleClick += new System.EventHandler(this.iFolderView_DoubleClick);
			this.iFolderView.SelectedIndexChanged += new System.EventHandler(this.iFolderView_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 100;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Location";
			this.columnHeader4.Width = 240;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Status";
			this.columnHeader5.Width = 64;
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuOpen,
																						 this.menuCreate,
																						 this.menuRefresh,
																						 this.menuSeparator1,
																						 this.menuRevert,
																						 this.menuShare,
																						 this.menuSyncNow,
																						 this.menuSeparator2,
																						 this.menuProperties});
			this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
			// 
			// menuOpen
			// 
			this.menuOpen.Index = 0;
			this.menuOpen.Text = "&Open...";
			this.menuOpen.Visible = false;
			this.menuOpen.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// menuCreate
			// 
			this.menuCreate.Index = 1;
			this.menuCreate.Text = "&Create iFolder";
			this.menuCreate.Click += new System.EventHandler(this.menuCreate_Click);
			// 
			// menuRefresh
			// 
			this.menuRefresh.Index = 2;
			this.menuRefresh.Text = "&Refresh list";
			this.menuRefresh.Click += new System.EventHandler(this.menuRefresh_Click);
			// 
			// menuSeparator1
			// 
			this.menuSeparator1.Index = 3;
			this.menuSeparator1.Text = "-";
			this.menuSeparator1.Visible = false;
			// 
			// menuRevert
			// 
			this.menuRevert.Index = 4;
			this.menuRevert.Text = "Revert to a normal folder";
			this.menuRevert.Visible = false;
			this.menuRevert.Click += new System.EventHandler(this.menuRevert_Click);
			// 
			// menuShare
			// 
			this.menuShare.Index = 5;
			this.menuShare.Text = "&Share with...";
			this.menuShare.Visible = false;
			this.menuShare.Click += new System.EventHandler(this.menuShare_Click);
			// 
			// menuSyncNow
			// 
			this.menuSyncNow.Index = 6;
			this.menuSyncNow.Text = "Sync now";
			this.menuSyncNow.Visible = false;
			this.menuSyncNow.Click += new System.EventHandler(this.menuSyncNow_Click);
			// 
			// menuSeparator2
			// 
			this.menuSeparator2.Index = 7;
			this.menuSeparator2.Text = "-";
			this.menuSeparator2.Visible = false;
			// 
			// menuProperties
			// 
			this.menuProperties.Index = 8;
			this.menuProperties.Text = "Properties...";
			this.menuProperties.Visible = false;
			this.menuProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(426, 333);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Preferences";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.Controls.Add(this.autoSync);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(16, 128);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(398, 184);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Synchronization";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.defaultInterval);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(16, 48);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(366, 112);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Synchronize to host";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(336, 48);
			this.label3.TabIndex = 0;
			this.label3.Text = "This value sets the default value for how often the host of an iFolder will be co" +
				"ntacted  to sync files.";
			// 
			// autoSync
			// 
			this.autoSync.Checked = true;
			this.autoSync.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoSync.Enabled = false;
			this.autoSync.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoSync.Location = new System.Drawing.Point(16, 24);
			this.autoSync.Name = "autoSync";
			this.autoSync.Size = new System.Drawing.Size(368, 16);
			this.autoSync.TabIndex = 0;
			this.autoSync.Text = "&Automatic sync";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.autoStart);
			this.groupBox3.Controls.Add(this.displayConfirmation);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(16, 24);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(398, 80);
			this.groupBox3.TabIndex = 0;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Application";
			// 
			// autoStart
			// 
			this.autoStart.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoStart.Location = new System.Drawing.Point(16, 20);
			this.autoStart.Name = "autoStart";
			this.autoStart.Size = new System.Drawing.Size(368, 24);
			this.autoStart.TabIndex = 0;
			this.autoStart.Text = "&Startup iFolder at login.";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.clearLog);
			this.tabPage3.Controls.Add(this.saveLog);
			this.tabPage3.Controls.Add(this.log);
			this.tabPage3.Controls.Add(this.syncNow);
			this.tabPage3.Controls.Add(this.label6);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(426, 333);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Log";
			// 
			// clearLog
			// 
			this.clearLog.Enabled = false;
			this.clearLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.clearLog.Location = new System.Drawing.Point(88, 288);
			this.clearLog.Name = "clearLog";
			this.clearLog.TabIndex = 4;
			this.clearLog.Text = "&Clear";
			// 
			// saveLog
			// 
			this.saveLog.Enabled = false;
			this.saveLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.saveLog.Location = new System.Drawing.Point(8, 288);
			this.saveLog.Name = "saveLog";
			this.saveLog.TabIndex = 3;
			this.saveLog.Text = "&Save...";
			// 
			// log
			// 
			this.log.HorizontalScrollbar = true;
			this.log.Location = new System.Drawing.Point(8, 48);
			this.log.Name = "log";
			this.log.ScrollAlwaysVisible = true;
			this.log.Size = new System.Drawing.Size(408, 225);
			this.log.TabIndex = 2;
			// 
			// syncNow
			// 
			this.syncNow.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.syncNow.Location = new System.Drawing.Point(320, 12);
			this.syncNow.Name = "syncNow";
			this.syncNow.Size = new System.Drawing.Size(96, 23);
			this.syncNow.TabIndex = 1;
			this.syncNow.Text = "S&ync now";
			this.syncNow.Click += new System.EventHandler(this.syncNow_Click);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 16);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(296, 16);
			this.label6.TabIndex = 0;
			this.label6.Text = "This log shows current iFolder activity.";
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.services);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(426, 333);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Services";
			// 
			// services
			// 
			this.services.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader2,
																					   this.columnHeader3});
			this.services.ContextMenu = this.contextMenu2;
			this.services.FullRowSelect = true;
			this.services.Location = new System.Drawing.Point(8, 16);
			this.services.MultiSelect = false;
			this.services.Name = "services";
			this.services.Size = new System.Drawing.Size(408, 208);
			this.services.TabIndex = 0;
			this.services.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Service";
			this.columnHeader2.Width = 209;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Status";
			this.columnHeader3.Width = 195;
			// 
			// contextMenu2
			// 
			this.contextMenu2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuEnabled,
																						 this.menuStart,
																						 this.menuPause,
																						 this.menuStop,
																						 this.menuRestart});
			this.contextMenu2.Popup += new System.EventHandler(this.contextMenu2_Popup);
			// 
			// menuEnabled
			// 
			this.menuEnabled.Index = 0;
			this.menuEnabled.Text = "Enabled";
			this.menuEnabled.Click += new System.EventHandler(this.menuEnabled_Click);
			// 
			// menuStart
			// 
			this.menuStart.Enabled = false;
			this.menuStart.Index = 1;
			this.menuStart.Text = "Start";
			this.menuStart.Click += new System.EventHandler(this.menuStart_Click);
			// 
			// menuPause
			// 
			this.menuPause.Enabled = false;
			this.menuPause.Index = 2;
			this.menuPause.Text = "Pause";
			this.menuPause.Click += new System.EventHandler(this.menuPause_Click);
			// 
			// menuStop
			// 
			this.menuStop.Enabled = false;
			this.menuStop.Index = 3;
			this.menuStop.Text = "Stop";
			this.menuStop.Click += new System.EventHandler(this.menuStop_Click);
			// 
			// menuRestart
			// 
			this.menuRestart.Enabled = false;
			this.menuRestart.Index = 4;
			this.menuRestart.Text = "Restart";
			this.menuRestart.Click += new System.EventHandler(this.menuRestart_Click);
			// 
			// banner
			// 
			this.banner.Location = new System.Drawing.Point(0, 0);
			this.banner.Name = "banner";
			this.banner.Size = new System.Drawing.Size(450, 65);
			this.banner.TabIndex = 9;
			this.banner.TabStop = false;
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuFile,
																					  this.menuAction,
																					  this.menuView});
			// 
			// menuFile
			// 
			this.menuFile.Index = 0;
			this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuFileExit});
			this.menuFile.Text = "File";
			// 
			// menuFileExit
			// 
			this.menuFileExit.Index = 0;
			this.menuFileExit.Text = "Exit";
			this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
			// 
			// menuAction
			// 
			this.menuAction.Index = 1;
			this.menuAction.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuActionCreate,
																					   this.menuActionSeparator1,
																					   this.menuActionOpen,
																					   this.menuActionRevert,
																					   this.menuActionShare,
																					   this.menuActionSync,
																					   this.menuActionProperties,
																					   this.menuActionEnable,
																					   this.menuActionStart,
																					   this.menuActionPause,
																					   this.menuActionStop,
																					   this.menuActionRestart});
			this.menuAction.Text = "Action";
			this.menuAction.Popup += new System.EventHandler(this.menuAction_Popup);
			// 
			// menuActionCreate
			// 
			this.menuActionCreate.Index = 0;
			this.menuActionCreate.Text = "Create iFolder...";
			this.menuActionCreate.Click += new System.EventHandler(this.menuCreate_Click);
			// 
			// menuActionSeparator1
			// 
			this.menuActionSeparator1.Index = 1;
			this.menuActionSeparator1.Text = "-";
			this.menuActionSeparator1.Visible = false;
			// 
			// menuActionOpen
			// 
			this.menuActionOpen.Index = 2;
			this.menuActionOpen.Text = "Open...";
			this.menuActionOpen.Visible = false;
			this.menuActionOpen.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// menuActionRevert
			// 
			this.menuActionRevert.Index = 3;
			this.menuActionRevert.Text = "Revert to a normal folder";
			this.menuActionRevert.Visible = false;
			this.menuActionRevert.Click += new System.EventHandler(this.menuRevert_Click);
			// 
			// menuActionShare
			// 
			this.menuActionShare.Index = 4;
			this.menuActionShare.Text = "Share with...";
			this.menuActionShare.Visible = false;
			this.menuActionShare.Click += new System.EventHandler(this.menuShare_Click);
			// 
			// menuActionSync
			// 
			this.menuActionSync.Index = 5;
			this.menuActionSync.Text = "Sync now";
			this.menuActionSync.Visible = false;
			this.menuActionSync.Click += new System.EventHandler(this.menuSyncNow_Click);
			// 
			// menuActionProperties
			// 
			this.menuActionProperties.Index = 6;
			this.menuActionProperties.Text = "Properties...";
			this.menuActionProperties.Visible = false;
			this.menuActionProperties.Click += new System.EventHandler(this.menuProperties_Click);
			// 
			// menuActionEnable
			// 
			this.menuActionEnable.Index = 7;
			this.menuActionEnable.Text = "Enable";
			this.menuActionEnable.Visible = false;
			this.menuActionEnable.Click += new System.EventHandler(this.menuEnabled_Click);
			// 
			// menuActionStart
			// 
			this.menuActionStart.Index = 8;
			this.menuActionStart.Text = "Start";
			this.menuActionStart.Visible = false;
			this.menuActionStart.Click += new System.EventHandler(this.menuStart_Click);
			// 
			// menuActionPause
			// 
			this.menuActionPause.Index = 9;
			this.menuActionPause.Text = "Pause";
			this.menuActionPause.Visible = false;
			this.menuActionPause.Click += new System.EventHandler(this.menuPause_Click);
			// 
			// menuActionStop
			// 
			this.menuActionStop.Index = 10;
			this.menuActionStop.Text = "Stop";
			this.menuActionStop.Visible = false;
			this.menuActionStop.Click += new System.EventHandler(this.menuStop_Click);
			// 
			// menuActionRestart
			// 
			this.menuActionRestart.Index = 11;
			this.menuActionRestart.Text = "Restart";
			this.menuActionRestart.Visible = false;
			this.menuActionRestart.Click += new System.EventHandler(this.menuRestart_Click);
			// 
			// menuView
			// 
			this.menuView.Index = 2;
			this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuViewRefresh});
			this.menuView.Text = "View";
			this.menuView.Popup += new System.EventHandler(this.menuView_Popup);
			// 
			// menuViewRefresh
			// 
			this.menuViewRefresh.Enabled = false;
			this.menuViewRefresh.Index = 0;
			this.menuViewRefresh.Text = "Refresh";
			this.menuViewRefresh.Click += new System.EventHandler(this.menuRefresh_Click);
			// 
			// tabPage5
			// 
			this.tabPage5.Controls.Add(this.groupBox5);
			this.tabPage5.Location = new System.Drawing.Point(4, 22);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Size = new System.Drawing.Size(426, 333);
			this.tabPage5.TabIndex = 4;
			this.tabPage5.Text = "Network";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.port);
			this.groupBox5.Controls.Add(this.label7);
			this.groupBox5.Controls.Add(this.proxy);
			this.groupBox5.Controls.Add(this.useProxy);
			this.groupBox5.Controls.Add(this.noProxy);
			this.groupBox5.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox5.Location = new System.Drawing.Point(16, 24);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(392, 100);
			this.groupBox5.TabIndex = 0;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Proxy Settings";
			// 
			// noProxy
			// 
			this.noProxy.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.noProxy.Location = new System.Drawing.Point(16, 24);
			this.noProxy.Name = "noProxy";
			this.noProxy.Size = new System.Drawing.Size(88, 24);
			this.noProxy.TabIndex = 0;
			this.noProxy.Text = "No Proxy";
			// 
			// useProxy
			// 
			this.useProxy.Location = new System.Drawing.Point(16, 56);
			this.useProxy.Name = "useProxy";
			this.useProxy.Size = new System.Drawing.Size(88, 24);
			this.useProxy.TabIndex = 1;
			this.useProxy.Text = "Use Proxy:";
			this.useProxy.CheckedChanged += new System.EventHandler(this.useProxy_CheckedChanged);
			// 
			// proxy
			// 
			this.proxy.Enabled = false;
			this.proxy.Location = new System.Drawing.Point(96, 56);
			this.proxy.Name = "proxy";
			this.proxy.Size = new System.Drawing.Size(168, 20);
			this.proxy.TabIndex = 2;
			this.proxy.Text = "";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(280, 60);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(56, 16);
			this.label7.TabIndex = 3;
			this.label7.Text = "Port:";
			// 
			// port
			// 
			this.port.Enabled = false;
			this.port.Location = new System.Drawing.Point(320, 56);
			this.port.Name = "port";
			this.port.Size = new System.Drawing.Size(56, 20);
			this.port.TabIndex = 4;
			this.port.Text = "";
			// 
			// GlobalProperties
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(450, 464);
			this.Controls.Add(this.banner);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Menu = this.mainMenu1;
			this.Name = "GlobalProperties";
			this.Text = "Global iFolder Properties";
			this.Load += new System.EventHandler(this.GlobalProperties_Load);
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.tabPage4.ResumeLayout(false);
			this.tabPage5.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		/// <summary>
		/// Sets the ServiceManager.
		/// </summary>
		public Simias.Service.Manager ServiceManager
		{
			set
			{
				serviceManager = value;
			}
		}

		/// <summary>
		/// Sets the iFolderManager.
		/// </summary>
		public iFolderManager IFManager
		{
			set
			{
				manager = value;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Set the run value in the Windows registery.
		/// </summary>
		/// <param name="enable"><b>True</b> will set the run value.</param>
		static public void SetRunValue(bool enable)
		{
			RegistryKey runKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");

			if (enable)
			{
				runKey.SetValue(iFolderRun, Path.Combine(Application.StartupPath, "iFolderApp.exe"));
			}
			else
			{
				runKey.DeleteValue(iFolderRun, false);
			}
		}
		#endregion

		#region Private Methods
		private void AddiFolderToListView(iFolder ifolder)
		{
			lock (ht)
			{
				// Add only if it isn't already in the list.
				if (ht[ifolder.ID] == null)
				{
					string status = ifolder.HasCollisions() ? "Conflicts exist" : "OK";
					ListViewItem lvi = new ListViewItem(new string[] {ifolder.Name, ifolder.LocalPath, status}, 0);
					lvi.Tag = ifolder.ID;
					iFolderView.Items.Add(lvi);

					// Add the listviewitem to the hashtable.
					ht.Add(ifolder.ID, lvi);
				}
			}
		}

		private bool IsRunEnabled()
		{
			string run = null;

			try
			{
				RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
				run = (string)runKey.GetValue(iFolderRun);
			}
			catch
			{
				return false;
			}

			return (run != null);
		}

		private void refreshiFolders()
		{
			Cursor.Current = Cursors.WaitCursor;

			iFolderView.Items.Clear();
			iFolderView.SelectedItems.Clear();

			lock(ht)
			{
				ht.Clear();
			}

			iFolderView.BeginUpdate();

			foreach (iFolder ifolder in manager)
			{
				AddiFolderToListView(ifolder);
			}

			iFolderView.EndUpdate();
			Cursor.Current = Cursors.Default;
		}

		private void invokeiFolderProperties(ListViewItem lvi, string activeTab)
		{
			iFolder ifolder = null;

			try
			{
				ifolder = manager.GetiFolderById((string)lvi.Tag);

				string windowName = "Advanced iFolder Properties for " + Path.GetFileName(ifolder.LocalPath);

				// Search for existing window and bring it to foreground ...
				Win32Window win32Window = Win32Util.Win32Window.FindWindow(null, windowName);
				if (win32Window != null)
				{
					win32Window.BringWindowToTop();
				}
				else
				{
					iFolderAdvanced ifolderAdvanced = new iFolderAdvanced();
					ifolderAdvanced.Name = ifolder.LocalPath;
					ifolderAdvanced.Text = windowName;
					ifolderAdvanced.CurrentiFolder = ifolder;
					ifolderAdvanced.ActiveTab = activeTab;

					ifolderAdvanced.ShowDialog();
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Sharing");

				if (ifolder == null)
				{
					MessageBox.Show("The selected iFolder is no longer valid and will be removed from the list.");
					iFolderView.Items.Remove(lvi);
				}
			}
		}

		private void synciFolder(string id)
		{
			try
			{
				ServiceCtl svc = serviceManager.GetService("Simias Sync Service");
				svc.Custom((int)(id == String.Empty ? SyncMessages.SyncAllNow : SyncMessages.SyncCollectionNow), id);
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "SyncCollection");
			}
		}
		#endregion

		#region Event Handlers
		private void GlobalProperties_Load(object sender, System.EventArgs e)
		{
			// Load the application icon and banner image.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
				this.banner.Image = Image.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
				this.iFolderView.SmallImageList = new ImageList();
				iFolderView.SmallImageList.Images.Add(new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico")));
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading graphics");
			}

			try
			{
				// Display the default sync interval.
				defaultInterval.Value = (decimal)manager.DefaultRefreshInterval;

				// Initialize displayConfirmation.
				string showWizard = config.Get("iFolderShell", "Show wizard", "true");
				displayConfirmation.Checked = showWizard == "true";

				autoStart.Checked = IsRunEnabled();

				refreshiFolders();

				foreach (ServiceCtl svc in serviceManager)
				{
					ListViewItem lvi = new ListViewItem(new string[] {
                                                                         svc.Name,
																		 svc.State.ToString()}, 0);
					lvi.Tag = new ServiceWithState(svc);
					services.Items.Add(lvi);
				}

				string proxyValue = null;
				string portValue = null;
				if (useProxy.Checked = Simias.Channels.SimiasChannelFactory.GetProxy(ref proxyValue, ref portValue))
				{
					proxy.Text = proxyValue;
					port.Text = portValue;
				}
				else
				{
					noProxy.Checked = true;
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Initializing");
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				// Save the default sync interval.
				manager.DefaultRefreshInterval = (int)defaultInterval.Value;

				// Save the auto start value.
				SetRunValue(autoStart.Checked);

				if (displayConfirmation.Checked)
				{
					config.Set("iFolderShell", "Show wizard", "true");
				}
				else
				{
					config.Set("iFolderShell", "Show wizard", "false");
				}

				// Update any services that have been changed.
				foreach (ListViewItem lvi in services.Items)
				{
					ServiceWithState service = (ServiceWithState)lvi.Tag;
					if (service.Changed)
					{
						this.serviceManager.Install(service.Svc);
					}
				}

				// Save the proxy settings.
				string proxyName = useProxy.Checked ? proxy.Text : null;
				string proxyPort = useProxy.Checked ? port.Text : null;
				Simias.Channels.SimiasChannelFactory.SetProxy(proxyName, proxyPort);
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Saving settings");
			}

			Cursor.Current = Cursors.Default;
		}

		private void menuFileExit_Click(object sender, System.EventArgs e)
		{
			this.ok_Click(this, e);

			this.Close();
		}

		private void menuAction_Popup(object sender, System.EventArgs e)
		{
			menuActionEnable.Visible = menuActionStart.Visible = menuActionPause.Visible =
				menuActionRestart.Visible = menuActionStop.Visible = tabControl1.SelectedIndex == 3;

			menuActionOpen.Visible = menuActionProperties.Visible = menuActionShare.Visible =
				menuActionSync.Visible = menuActionRevert.Visible = tabControl1.SelectedIndex == 0;

			this.menuActionSeparator1.Visible = (tabControl1.SelectedIndex == 0 || tabControl1.SelectedIndex == 3);

			menuActionShare.Enabled = menuActionProperties.Enabled = menuActionRevert.Enabled = 
				menuActionSync.Enabled = menuActionOpen.Enabled = iFolderView.SelectedItems.Count == 1;

			contextMenu2_Popup(this, e);
		}

		private void menuView_Popup(object sender, System.EventArgs e)
		{
			menuViewRefresh.Enabled = tabControl1.SelectedIndex == 0;
		}

		#region iFolders Tab
		private void iFolderView_DoubleClick(object sender, System.EventArgs e)
		{
			menuOpen_Click(sender, e);
		}

		private void iFolderView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (iFolderView.SelectedItems.Count == 1)
			{
				Cursor.Current = Cursors.WaitCursor;

				ListViewItem lvi = iFolderView.SelectedItems[0];
				try
				{
					// Get the sync node and byte counts.
					uint nodeCount;
					ulong bytesToSend;
					iFolder ifolder = manager.GetiFolderById((string)lvi.Tag);
					SyncSize.CalculateSendSize(ifolder, out nodeCount, out bytesToSend);
					objectCount.Text = nodeCount.ToString();
					byteCount.Text = bytesToSend.ToString();
				}
				catch (SimiasException ex)
				{
					ex.LogError();
				}
				catch (Exception ex)
				{
					logger.Debug(ex, "Selecting iFolder");
				}

				Cursor.Current = Cursors.Default;
			}
			else
			{
				objectCount.Text = byteCount.Text = "";
			}
		}

		private void contextMenu1_Popup(object sender, System.EventArgs e)
		{
			menuShare.Visible = menuProperties.Visible = menuRevert.Visible = 
				menuSeparator1.Visible = menuSeparator2.Visible = menuSyncNow.Visible =
				menuOpen.Visible = iFolderView.SelectedItems.Count == 1;
			menuRefresh.Visible = menuCreate.Visible = iFolderView.SelectedItems.Count == 0;
		}

		private void menuOpen_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];
			iFolder ifolder = null;

			try
			{
				ifolder = manager.GetiFolderById((string)lvi.Tag);
				Process.Start(ifolder.LocalPath);
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Opening iFolder");
				if (ifolder == null)
				{
					MessageBox.Show("The selected iFolder is no longer valid and will be removed from the list.");
					iFolderView.Items.Remove(lvi);
				}
			}
		}

		private void menuRevert_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = iFolderView.SelectedItems[0];

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				iFolder ifolder = manager.GetiFolderById((string)lvi.Tag);
				string path = ifolder.LocalPath;

				// Delete the iFolder.
				manager.DeleteiFolderById((string)lvi.Tag);

				// Notify the shell.
				Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, path, IntPtr.Zero);

				lock (ht)
				{
					ht.Remove((string)lvi.Tag);
				}

				lvi.Remove();
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Reverting");
			}

			Cursor.Current = Cursors.Default;
		}

		private void menuShare_Click(object sender, System.EventArgs e)
		{
			invokeiFolderProperties(iFolderView.SelectedItems[0], "share");
		}

		private void menuSyncNow_Click(object sender, System.EventArgs e)
		{
			synciFolder((string)iFolderView.SelectedItems[0].Tag);
		}

		private void menuProperties_Click(object sender, System.EventArgs e)
		{
			invokeiFolderProperties(iFolderView.SelectedItems[0], null);
		}

		private void menuCreate_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

			while (true)
			{
				if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						if (manager.CanBeiFolder(folderBrowserDialog.SelectedPath) && 
							((GlobalProperties.GetDriveType(Path.GetPathRoot(folderBrowserDialog.SelectedPath)) & DRIVE_REMOTE) != DRIVE_REMOTE))
						{
							// Create the iFolder.
							iFolder ifolder = manager.CreateiFolder(folderBrowserDialog.SelectedPath);

							// Notify the shell.
							Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, folderBrowserDialog.SelectedPath, IntPtr.Zero);

							// Add the iFolder to the listview.
							AddiFolderToListView(ifolder);

							// Display the new iFolder intro dialog.
							new iFolderComponent().NewiFolderWizard(Application.StartupPath, folderBrowserDialog.SelectedPath);
							break;
						}
						else
						{
							MessageBox.Show("An invalid folder was specified");
						}
					}
					catch (SimiasException ex)
					{
						ex.LogError();
					}
					catch (Exception ex)
					{
						logger.Debug(ex, "Creating iFolder");
					}
				}
				else
				{
					break;
				}
			}
		}

		private void menuRefresh_Click(object sender, System.EventArgs e)
		{
			refreshiFolders();
		}
		#endregion

		#region Log Tab
		private void syncNow_Click(object sender, System.EventArgs e)
		{
			synciFolder("");
		}
		#endregion

		#region Services Tab
		private void contextMenu2_Popup(object sender, System.EventArgs e)
		{
			if (services.SelectedItems.Count == 1)
			{
				ServiceWithState service = (ServiceWithState)services.SelectedItems[0].Tag;

				menuActionEnable.Text = service.Svc.Enabled ? "Disable" : "Enable";
				menuActionEnable.Enabled = true;

				// Set the state of the menu item.
				menuEnabled.Checked = service.Svc.Enabled;

				// Show the menu items.
				menuEnabled.Visible = menuStart.Visible = menuPause.Visible = menuStop.Visible = menuRestart.Visible = true;

				ListViewItem lvi = services.SelectedItems[0];
				switch (lvi.SubItems[1].Text)
				{
					case "Stopped":
						menuStart.Text = menuActionStart.Text = "Start";
						menuStart.Enabled = menuActionStart.Enabled = true;
						menuStop.Enabled = menuRestart.Enabled = 
							menuActionStop.Enabled = menuActionRestart.Enabled =
							menuPause.Enabled = menuActionPause.Enabled = false;
						break;
					case "Running":
						menuStart.Text = menuActionStart.Text = "Start";
						menuStart.Enabled = menuActionStart.Enabled = false;
						menuStop.Enabled = menuRestart.Enabled = 
							menuActionStop.Enabled = menuActionRestart.Enabled = 
							menuPause.Enabled = menuActionPause.Enabled = true;
						break;
					case "Paused":
						menuStart.Text = menuActionStart.Text = "Resume";
						menuStart.Enabled = menuActionStart.Enabled = 
							menuStop.Enabled = menuActionStop.Enabled = 
							menuRestart.Enabled = menuActionRestart.Enabled = true;
						menuPause.Enabled = menuActionPause.Enabled = false;
						break;
				}

				menuStart.Enabled = menuActionStart.Enabled = menuStart.Enabled && menuEnabled.Checked;
			}
			else
			{
				// Nothing is selected ... hide the menu items.
				menuEnabled.Enabled = menuStart.Enabled = menuPause.Enabled = menuStop.Enabled = menuRestart.Enabled = false;
				menuActionEnable.Enabled = menuActionStart.Enabled = menuActionPause.Enabled = menuActionStop.Enabled = menuActionRestart.Enabled = false;
			}
		}

		private void menuEnabled_Click(object sender, System.EventArgs e)
		{
			try
			{
				ServiceWithState service = (ServiceWithState)services.SelectedItems[0].Tag;

				// Toggle the enabled state.
				service.Svc.Enabled = !menuEnabled.Checked;

				// Set that the service has been changed.
				service.Changed = true;

				// Toggle the state of the menu item.
				menuEnabled.Checked = service.Svc.Enabled;
				menuActionEnable.Text = service.Svc.Enabled ? "Disable" : "Enable";
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Enabling/disabling service");
			}
		}

		private void menuStart_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = services.SelectedItems[0];
			ServiceWithState service = (ServiceWithState)lvi.Tag;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if (service.Svc.State == State.Stopped)
					service.Svc.Start();
				else
					service.Svc.Resume();
			}
			catch{}

			lvi.SubItems[1].Text = service.Svc.State.ToString();

			Cursor.Current = Cursors.Default;
		}

		private void menuRestart_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = services.SelectedItems[0];
			ServiceWithState service = (ServiceWithState)lvi.Tag;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				service.Svc.Stop();
				lvi.SubItems[1].Text = service.Svc.State.ToString();
				service.Svc.Start();
			}
			catch{}

			lvi.SubItems[1].Text = service.Svc.State.ToString();

			Cursor.Current = Cursors.Default;
		}

		private void menuStop_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = services.SelectedItems[0];
			ServiceWithState service = (ServiceWithState)lvi.Tag;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				service.Svc.Stop();
			}
			catch{}

			lvi.SubItems[1].Text = service.Svc.State.ToString();

			Cursor.Current = Cursors.Default;
		}

		private void menuPause_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = services.SelectedItems[0];
			ServiceWithState service = (ServiceWithState)lvi.Tag;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				service.Svc.Pause();
			}
			catch{}

			lvi.SubItems[1].Text = service.Svc.State.ToString();

			Cursor.Current = Cursors.Default;
		}
		#endregion

		#region Network Tab
		private void useProxy_CheckedChanged(object sender, System.EventArgs e)
		{
			proxy.Enabled = port.Enabled = useProxy.Checked;
		}
		#endregion

		#region Subscriber Handlers
		private void subscriber_NodeCreated(NodeEventArgs args)
		{
			try
			{
				iFolder ifolder = manager.GetiFolderById(args.ID);
				if (ifolder != null)
				{
					AddiFolderToListView(ifolder);
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "OnNodeCreated");
			}
		}

		private void subscriber_NodeDeleted(NodeEventArgs args)
		{
			lock (ht)
			{
				ListViewItem lvi = (ListViewItem)ht[args.Node];
				if (lvi != null)
				{
					lvi.Remove();
					ht.Remove(args.Node);
				}
			}
		}

		private void subscriber_NodeChanged(NodeEventArgs args)
		{
			// TODO: implement this if needed.
		}
		#endregion
		#endregion

		private const uint DRIVE_REMOTE = 4;

		[DllImport("kernel32.dll")]
		static extern uint GetDriveType(string rootPathName);
	}

	internal class ServiceWithState
	{
		private bool changed = false;
		private ServiceCtl svc;

		public ServiceWithState(ServiceCtl svc)
		{
			this.svc = svc;
		}

		public bool Changed
		{
			get
			{
				return changed;
			}
			set
			{
				changed = value;
			}
		}

		public ServiceCtl Svc
		{
			get
			{
				return svc;
			}
		}
	}
}
