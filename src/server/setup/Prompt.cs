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
*                 $Author: Rob
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <iFolder setup interactive prompt class >
*
*
*******************************************************************************/

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
		
/********************************************************************************************************
		Note: This method is for Reading password with echo off,  This needs Mono runtime version 
		       1.2.3 or later.   

		/// <summary>
		/// Prompt for a Password String Value
		/// </summary>
		/// <param name="prompt">The Prompt String</param>
		/// <param name="defaultValue">The Default Value</param>
		/// <returns>A String Object</returns>
		public static string ForPasswordString(string prompt, string defaultValue)
                {
			Console.Write("{0}? [{1}]: ", prompt, defaultValue);
                        string password = "";

                        ConsoleKeyInfo info = Console.ReadKey(true);
                        while (info.Key != ConsoleKey.Enter)
                        {
                                if (info.Key != ConsoleKey.Backspace)
                                {
                                        password += info.KeyChar;
                                        info = Console.ReadKey(true);
                                }
                                else if (info.Key == ConsoleKey.Backspace)
                                {
                                        if (!string.IsNullOrEmpty(password))
                                        {
                                                password = password.Substring(0, password.Length - 1);
                                        }
                                        info = Console.ReadKey(true);
                                }
                        }
			if (string.IsNullOrEmpty(password))
			{
				password = defaultValue;
			}
			return password;
                }

**************************************************************************************************************/

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
				{
/********************************************************************************************************
		Note: This method is for Reading password with echo off,  This needs Mono runtime version 
		       1.2.3 or later.   

					if(String.Compare(option.Name, "ldap-admin-password") == 0 || 
						String.Compare(option.Name, "system-admin-password") == 0 || 
						String.Compare(option.Name, "ldap-proxy-password") == 0 )
						option.Value = ForPasswordString(option.Title, option.DefaultValue);
					else
						option.Value = ForString(option.Title, option.DefaultValue);

***********************************************************************************************************/
					option.Value = ForString(option.Title, option.DefaultValue);
				}
			}
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
