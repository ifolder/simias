/***********************************************************************
 *  Defaults.cs
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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/
	 
using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace Simias
{
	/// <summary>
	/// Defaults class for simias defaults.
	/// </summary>
	public sealed class Defaults
	{
		public const string DefaultConfigFile = "@sysconfdir@/defaults.config";
		//public const string SimiasDataDir = "simiasdatadir";
		//public const string RunAsClient = "runasclient";


		/// <summary>
		/// Constructor.
		/// </summary>
		private Defaults()
		{
		}

		public static string SimiasDataDir
		{
			get
			{
                		return "/var/lib/simias";
			}
		}

		public static bool RunsAsClient
		{
			get
			{
				return false;
			}
		}

	}
}


