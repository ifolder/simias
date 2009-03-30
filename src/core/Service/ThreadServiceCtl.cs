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
*                 $Author: Russ Young
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Threading;

namespace Simias.Service
{
	/// <summary>
	/// Summary description for ThreadService.
	/// </summary>
	public class ThreadServiceCtl : ServiceCtl
	{
		const string XmlClassAttr = "class";
		IThreadService			service = null;
		string					classType;
		

		#region Constructor

		/// <summary>
		/// Control to thread service
		/// </summary>
		/// <param name="assembly">Assembly</param>
		/// <param name="serviceElement">XML Service element</param>
		public ThreadServiceCtl(string assembly, XmlElement serviceElement) :
			base(assembly, serviceElement)
		{
			classType = serviceElement.GetAttribute(XmlClassAttr);
		}

		/// <summary>
		/// Control to thread service
		/// </summary>
		/// <param name="name">Name of the thread</param>
		/// <param name="assembly">Assembly</param>
		/// <param name="classType">Type of the class</param>
		public ThreadServiceCtl(string name, string assembly, string classType) :
			base(name, assembly)
		{
			this.classType = classType;
		}

		#endregion

		
		#region IServiceCtl members

		/// <summary>
		/// Start the thread control
		/// </summary>
		public override void Start()
		{
			lock (typeof(ThreadServiceCtl))
			{
				// Load the assembly and start it.
				Assembly pAssembly = AppDomain.CurrentDomain.Load(Assembly);
				service = (IThreadService)pAssembly.CreateInstance(classType);
				service.Start();
				state = Service.State.Running;
				Manager.logger.Info("\"{0}\" service started.", Name);
			}
		}

		/// <summary>
		/// Stop the thread control service
		/// </summary>
		public override void Stop()
		{
			lock (this)
			{
				service.Stop();
				service = null;
				state = Service.State.Stopped;
				Manager.logger.Info("\"{0}\" service stopped.", Name);
			}
		}

		/// <summary>
		/// Kill the thread service control
		/// </summary>
		public override void Kill()
		{
			lock (this)
			{
				service.Stop();
				service = null;
				state = Service.State.Stopped;
				Manager.logger.Info("\"{0}\" service killed.", Name);
			}
		}

		/// <summary>
		/// Pause the service
		/// </summary>
		public override void Pause()
		{
			lock (this)
			{
				service.Pause();
				state = Service.State.Paused;
				Manager.logger.Info("\"{0}\" service paused.", Name);
			}
		}

		/// <summary>
		/// Resume the service
		/// </summary>
		public override void Resume()
		{
			lock (this)
			{
				service.Resume();
				state = Service.State.Running;
				Manager.logger.Info("\"{0}\" service resumed.", Name);
			}
		}

		/// <summary>
		/// Log the custom message
		/// </summary>
		/// <param name="message">Actual Message to log</param>
		/// <param name="data">Data to be inserted in the log</param>
		public override void Custom(int message, string data)
		{
			service.Custom(message, data);
			Manager.logger.Info("\"{0}\" service message {1}.", Name, message);
		}


		/// <summary>
		/// Convert XML element to base XML 
		/// </summary>
		/// <param name="element">XML element to be converted</param>
		public override void ToXml(XmlElement element)
		{
			base.ToXml(element);
			element.SetAttribute(XmlClassAttr, classType);
		}

		/// <summary>
		/// Called to check if the service has exited.
		/// </summary>
		public override bool HasExited
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}
