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
 | Author: Novell
 |***************************************************************************/


using System;

namespace Simias.Service
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class ProcessServiceTest : BaseProcessService
	{
		static ProcessServiceTest service;
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			service = new ProcessServiceTest();
			service.Run();
		}

		protected override void Start()
		{
		}

		protected override void Stop()
		{
		}

		protected override void Pause()
		{
		}

		protected override void Resume()
		{
		}

		protected override void Custom(int messageId, string message)
		{
		}
	}
}
