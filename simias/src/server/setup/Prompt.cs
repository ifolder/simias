/***********************************************************************
 *  $RCSfile: Prompt.cs,v $
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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Novell.iFolder.Utility
{
	/// <summary>
	/// Prompt Methods
	/// </summary>
	public class Prompt
	{
		static bool canPrompt = false;
		public static bool CanPrompt
		{
			get { return canPrompt; }
			set { canPrompt = value; }
		}
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
		private static string ForString(string prompt, string defaultValue)
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
		/// Prompt for and get the value for the option.
		/// </summary>
		/// <param name="option">The option to prompt for.</param>
		public static void ForOption(Option option)
		{
			if (!option.Assigned && option.Prompt && CanPrompt)
			{
				if (option.Description != null)
				{
					Console.WriteLine();
					Console.WriteLine("----- {0} -----", option.Title.ToUpper());
					// Format the description.
					Regex lineSplitter = new Regex(@".{0,50}[^\s]*");
					MatchCollection matches = lineSplitter.Matches(option.Description);
					foreach (Match line in matches)
					{	
						Console.WriteLine(line.Value.Trim());
					}
					Console.WriteLine();
				}
				if (option.GetType() == typeof(BoolOption))
					option.Value = ForYesNo(option.Title, Boolean.Parse(option.DefaultValue)).ToString();
				else
					option.Value = ForString(option.Title, option.DefaultValue);
			}
		}

		/// <summary>
		/// Prompt for a Yes/No Value
		/// </summary>
		/// <param name="prompt">The Prompt String</param>
		/// <param name="defaultValue">The Default Value</param>
		/// <returns>A bool Object</returns>
		private static bool ForYesNo(string prompt, bool defaultValue)
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
