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
 | Author: Rob
 |***************************************************************************/

using System;

namespace Simias.Service
{
	/// <summary>
	/// Thread Service Interface.
	/// A Thread service must implement this interface.
	/// </summary>
	public interface IThreadService
	{
		/// <summary>
		/// Called to start the service.
		/// </summary>
		/// <param name="conf"></param>
		void Start();
		/// <summary>
		/// Called to stop the service.
		/// </summary>
		void Stop();
		/// <summary>
		/// Called to pause the service.
		/// </summary>
		void Pause();
		/// <summary>
		/// Called to resume the service after a pause.
		/// </summary>
		void Resume();
		/// <summary>
		/// Called to process the service defined message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		int Custom(int message, string data);
	}
}
