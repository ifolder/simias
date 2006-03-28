/***********************************************************************
 *  $RCSfile: Prompt.cs,v $
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
using System.IO;

namespace Novell.iFolder.Utility
{
	/// <summary>
	/// Prompt Methods
	/// </summary>
	public class Prompt
	{
		/// <summary>
		/// Constructor
		/// </summary>
		private Prompt()
		{
		}
		
		/// <summary>
		/// Prompt for a String Value
		/// </summary>
		/// <param name="prompt">The Prompt String</param>
		/// <param name="defaultValue">The Default Value</param>
		/// <returns>A String Object</returns>
		public static string ForString(string prompt, string defaultValue)
		{
			Console.Write("{0}? [{1}]: ", prompt, defaultValue);
			string response = Console.ReadLine();

			if ((response == null) || (response.Length == 0))
			{
				response = defaultValue;
			}
			
			return response;
		}

		/// <summary>
		/// Prompt for a Yes/No Value
		/// </summary>
		/// <param name="prompt">The Prompt String</param>
		/// <param name="defaultValue">The Default Value</param>
		/// <returns>A bool Object</returns>
		public static bool ForYesNo(string prompt, bool defaultValue)
		{
			bool result = defaultValue;

			Console.Write("{0}? [{1}]: ", prompt, result ? "Y" : "N");
			string response = Console.ReadLine();

			if ((response != null) && (response.Length != 0))
			{
				result = response.ToLower().StartsWith("y");
			}
			
			return result;
		}
	}
}
