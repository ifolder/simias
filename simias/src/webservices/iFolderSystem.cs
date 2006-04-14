/***********************************************************************
 *  $RCSfile: iFolderSystem.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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

using Simias.Storage;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder System
	/// </summary>
	[Serializable]
	public class iFolderSystem
	{
		/// <summary>
		/// System ID
		/// </summary>
		public string ID;

		/// <summary>
		/// System Name
		/// </summary>
		public string Name;

		/// <summary>
		/// System Version
		/// </summary>
		public string Version;

		/// <summary>
		/// System Description
		/// </summary>
		public string Description;

		/// <summary>
		/// Constructor
		/// </summary>
		public iFolderSystem()
		{
		}

		/// <summary>
		/// Get the iFolder System Information Object
		/// </summary>
		/// <returns>An iFolderSystem Object</returns>
		public static iFolderSystem GetSystem()
		{
			iFolderSystem system = new iFolderSystem();

			Store store = Store.GetStore();
			
			Domain domain = store.GetDomain(store.DefaultDomain);

			system.ID = domain.ID;
			system.Name = domain.Name;
			system.Version = domain.DomainVersion.ToString();
			system.Description = domain.Description;

            return system;
		}
    }
}