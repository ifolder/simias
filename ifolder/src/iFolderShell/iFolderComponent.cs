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
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using Novell.iFolder;
using Simias.Storage;
using Novell.AddressBook;
using Novell.iFolder.FormsBookLib;
using Novell.iFolder.Win32Util;
using Simias;

namespace Novell.iFolder.iFolderCom
{
	public interface IiFolderComponent
	{
		String Description{get; set;}
		bool CanBeiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool IsiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool IsShareable([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool CreateiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		void DeleteiFolder([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool GetiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool IsiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path);
		bool GetiFolderPropInit();
		bool GetNextiFolderProp(out string name, out string val);
		void InvokeAdvancedDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path, bool modal);
		void NewiFolderWizard([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path);
		void ShowHelp([MarshalAs(UnmanagedType.LPWStr)] string dllPath);
	}

	/// <summary>
	/// Summary description for iFolderComponent.
	/// </summary>
	[
		ClassInterface(ClassInterfaceType.None),
		GuidAttribute("AA81D832-3B41-497c-B508-E9D02F8DF421")
	]
	public class iFolderComponent : IiFolderComponent
	{
		static private iFolderManager manager = null;//= Manager.Connect();
		private iFolderNode ifoldernode;
		private ICSEnumerator propEnumerator;
		private ICSEnumerator aclEnumerator;

		static private AddressBook.Manager abManager = null;
		private Novell.AddressBook.AddressBook addressBook = null;

		private IEnumerator items;
//		public iFolderComponent(Uri location)
//		{
//			manager= iFolderManager.Connect(location);
//		}

		public iFolderComponent()
		{
			//
			// TODO: Add constructor logic here
			//
			System.Diagnostics.Debug.WriteLine("In iFolderComponent()");

			try
			{
				if (manager == null)
				{
					manager= iFolderManager.Connect();
				}

				// Connect to the address book manager
				if (abManager == null)
				{
					abManager = Novell.AddressBook.Manager.Connect();
				}

				// Open the default address book
				if (addressBook == null)
				{
					addressBook = abManager.OpenDefaultAddressBook();
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
		}

		public String Description
		{
			get { return ifoldernode.Description; }
			set
			{
				ifoldernode.Description = value;

				// TODO - move this so that the commit can be done once at the end of
				// a bunch of modifies.
//				ifoldernode.iFolder.CurrentNode.Commit();
			}
		}

		public bool CanBeiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				return manager.CanBeiFolder(path);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			return false;
		}

		public bool IsiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			return manager.IsiFolder(path);
		}

		public bool IsShareable([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				if (IsiFolder(path))
				{
					return manager.GetiFolderByPath(path).IsShareable();
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			return false;
		}

		public bool CreateiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			iFolder ifolder = null;
			try
			{
				ifolder = manager.CreateiFolder(path);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			return (ifolder != null);
		}

		public void DeleteiFolder([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			try
			{
				manager.DeleteiFolderByPath(path);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}
		}

		public bool GetiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			System.Diagnostics.Debug.WriteLine("In GetiFolderNode()");

			try
			{
				foreach(iFolder ifolder in manager)
				{
					if (path.StartsWith(ifolder.LocalPath))
					{
						ifoldernode = ifolder.GetiFolderNodeByPath(path);
						if (ifoldernode != null)
						{
							System.Diagnostics.Debug.WriteLine("GetiFolderNode() returning true");
							return true;
						}

						break;
					}
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
				System.Diagnostics.Debug.WriteLine(e.StackTrace);
			}

			System.Diagnostics.Debug.WriteLine("GetiFolderNode() returning false");

			return false;
		}

		public bool IsiFolderNode([MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			return manager.IsPathIniFolder(path);
		}

		public bool GetiFolderPropInit()
		{
			// Set up the enumerator to get the Properties on the Node.
			propEnumerator = ( ICSEnumerator )ifoldernode.iFolder.CurrentNode.Properties.GetEnumerator();

			return (propEnumerator != null);
			return false;
		}

		public bool GetNextiFolderProp(out string name, out string val)
		{
			if (propEnumerator.MoveNext())
			{
				Property p = (Property)propEnumerator.Current;
				name = new string(p.Name.ToCharArray());
				val = new string(p.Value.ToString().ToCharArray());
				return true;
			}
			else
			{
				propEnumerator.Dispose();
				name = null;
				val = null;
				ifoldernode = null;
				return false;
			}
		}

		public void InvokeAdvancedDlg([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path, bool modal)
		{
			string windowName = "Advanced iFolder Properties for " + Path.GetFileName(path);

			// Search for existing window and bring it to foreground ...
			Win32Window win32Window = Win32Util.Win32Window.FindWindow(null, windowName);
			if (win32Window != null)
			{
				win32Window.BringWindowToTop();
			}
			else
			{
				iFolderAdvanced ifolderAdvanced = new iFolderAdvanced();
				ifolderAdvanced.Name = path;
				ifolderAdvanced.Text = windowName;
				ifolderAdvanced.ABManager = abManager;
				ifolderAdvanced.CurrentiFolder = manager.GetiFolderByPath(path);
				ifolderAdvanced.LoadPath = dllPath;

				if (modal)
				{
					ifolderAdvanced.ShowDialog();
				}
				else
				{
					ifolderAdvanced.Show();
				}
			}
		}

		public void NewiFolderWizard([MarshalAs(UnmanagedType.LPWStr)] string dllPath, [MarshalAs(UnmanagedType.LPWStr)] string path)
		{
			Configuration config = new Configuration();
			string showWizard = config.Get("iFolderShell", "Show wizard", "true");
			if (showWizard == "true")
			{
				NewiFolder newiFolder = new NewiFolder();
				newiFolder.FolderName = path;
				newiFolder.LoadPath = dllPath;
				newiFolder.Show();
			}
		}

		public void ShowHelp([MarshalAs(UnmanagedType.LPWStr)] string dllPath)
		{
			// TODO - need to use locale-specific path
			string helpPath = Path.Combine(dllPath, @"help\en\doc\user\data\front.html");

			try
			{
				Process.Start(helpPath);
			}
			catch
			{
				MessageBox.Show("Unable to open help file: \n" + helpPath, "Help File Not Found");
			}
		}
	}
}
