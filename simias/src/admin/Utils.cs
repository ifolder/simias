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
 | Author: Mike Lasky (mlasky@novell.com)
 |***************************************************************************/

using System;
using System.Net;
using System.Resources;
using System.Threading;
using System.Web.Services.Protocols;
using System.Web.UI.WebControls;
using System.Xml;

namespace Novell.iFolderWeb.Admin
{
	/// <summary>
	/// Summary description for Utils.
	/// </summary>
	public class Utils
	{
		#region Class Members

		/// <summary>
		/// Default constant for Byte.
		/// </summary>
		public const decimal KiloByte = 1024;

		/// <summary>
		/// Default constant for MegaByte.
		/// </summary>
		public const decimal MegaByte = KiloByte * 1024;

		/// <summary>
		/// Default constant for GigaByte.
		/// </summary>
		public const decimal GigaByte = MegaByte * 1024;

		#endregion

		#region Private Methods

		#endregion

		#region Public Methods

		/// <summary>
		/// Converts a regular expression file type string to a simpler wildcard type.
		/// </summary>
		/// <param name="regEx">String containing regular expression.</param>
		/// <returns>String containing simple wildcard.</returns>
		public static string ConvertFromRegEx( string regEx )
		{
			string wcs = regEx.Replace( @"\.", "." ).Replace( ".*", "*" ).Replace( ".?", "?" );
			return wcs.TrimStart( new char[] { '^' } ).TrimEnd( new char[] { '$' } );
		}

		/// <summary>
		/// Converts from a simple wildcard type to a regular expression file type.
		/// </summary>
		/// <param name="simpleWildcard">String containing a simple wildcard expression.</param>
		/// <returns>String containing a regular expression.</returns>
		public static string ConvertToRegEx( string simpleWildcard )
		{
			string res = simpleWildcard.Trim().Replace( ".", @"\." ).Replace( "*", ".*" ).Replace( "?", ".?" );
			return "^" + res + "$";
		}

		/// <summary>
		/// Converts the size to megabytes.
		/// </summary>
		/// <param name="size">Size of the number in bytes.</param>
		/// <param name="appendUnits">If true the proper units are appended.</param>
		/// <param name="rm">Resource manager object that will allow retrieval of localized strings.</param>
		/// <returns>A string containing the size in megabytes.</returns>
		public static string ConvertToMBString( long size, bool appendUnits, ResourceManager rm )
		{
			decimal units = Convert.ToDecimal( size ) / MegaByte;
			string sizeString = ( units > 0 ) ? String.Format( "{0:##.##}", units ) : "0";
			if ( appendUnits )
			{
				sizeString += ( " " + rm.GetString( "MB" ) );
			}

			return sizeString;
		}

		/// <summary>
		/// Gets a string that represents the size in bytes of a number with the proper
		/// unit size appended.
		/// </summary>
		/// <param name="size">Size of the number in bytes.</param>
		/// <param name="appendUnits">If true the proper units are appended.</param>
		/// <param name="rm">Resource manager object that will allow retrieval of localized strings.</param>
		/// <returns>A string containing the size.</returns>
		public static string ConvertToUnitString( long size, bool appendUnits, ResourceManager rm )
		{
			decimal dbSize = Convert.ToDecimal( size );
			string sizeString;

			decimal unitSize = dbSize / GigaByte;
			if ( unitSize >= 1 )
			{
				sizeString = String.Format( "{0:##.##}", unitSize );
				if ( appendUnits )
				{
					sizeString += ( " " + rm.GetString( "GB" ) );
				}
			}
			else
			{
				unitSize = dbSize / MegaByte;
				if ( unitSize >= 1 )
				{
					sizeString = String.Format( "{0:##.##}", unitSize );
					if ( appendUnits )
					{
						sizeString += ( " " + rm.GetString( "MB" ) );
					}
				}
				else
				{
					unitSize = dbSize / KiloByte;
					if ( unitSize >= 1 )
					{
						sizeString = String.Format( "{0:##.##}", unitSize );
						if ( appendUnits )
						{
							sizeString += ( " " + rm.GetString( "KB" ) );
						}
					}
					else
					{
						sizeString = String.Format( "{0}", size );
						if ( appendUnits )
						{
							sizeString += ( " " + rm.GetString( "B" ) );
						}
					}
				}
			}

			return sizeString;
		}

		/// <summary>
		/// Gets a string that represents the size in bytes of a number with the proper
		/// unit size appended.
		/// </summary>
		/// <param name="size">Size of the number in bytes.</param>
		/// <param name="rm">Resource manager object that will allow retrieval of localized strings.</param>
		/// <param name="units">Gets the unit string.</param>
		/// <returns>A string containing the size.</returns>
		public static string ConvertToUnitString( long size, ResourceManager rm, out string units )
		{
			decimal dbSize = Convert.ToDecimal( size );
			string sizeString;

			decimal unitSize = dbSize / GigaByte;
			if ( unitSize > 1 )
			{
				sizeString = String.Format( "{0:##.#}", unitSize );
				units = rm.GetString( "GB" );
			}
			else
			{
				unitSize = dbSize / MegaByte;
				if ( unitSize > 1 )
				{
					sizeString = String.Format( "{0:##.#}", unitSize );
					units = rm.GetString( "MB" );
				}
				else
				{
					unitSize = dbSize / KiloByte;
					if ( unitSize > 1 )
					{
						sizeString = String.Format( "{0:##.#}", unitSize );
						units = rm.GetString( "KB" );
					}
					else
					{
						sizeString = String.Format( "{0}", size );
						units = rm.GetString( "B" );
					}
				}
			}

			return sizeString;
		}

		/// <summary>
		/// Gets an ifolder policy object that is set-able.
		/// </summary>
		/// <param name="ifolderID"></param>
		/// <returns></returns>
		public static iFolderPolicy GetiFolderPolicyObject( string ifolderID )
		{
			iFolderPolicy policy = new iFolderPolicy();
			policy.iFolderID = ifolderID;
			policy.FileSizeLimit = policy.SpaceLimit = policy.SyncInterval = -1;
			policy.FileTypesExcludes = policy.FileTypesIncludes = null;
			policy.Locked = false;
			return policy;
		}

		/// <summary>
		/// Gets user policy object that is set-able.
		/// </summary>
		/// <param name="userID"></param>
		/// <returns></returns>
		public static UserPolicy GetUserPolicyObject( string userID )
		{
			UserPolicy policy = new UserPolicy();
			policy.UserID = userID;
			policy.FileSizeLimit = policy.SpaceLimit = policy.SyncInterval = -1;
			policy.FileTypesExcludes = policy.FileTypesIncludes = null;
			policy.LoginEnabled = true;
			return policy;
		}

		/// <summary>
		/// Formats the DateTime to a string for the CurrentUICulture on the current thread.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string ToDateTimeString( DateTime dt )
		{
			return dt.ToString( Thread.CurrentThread.CurrentUICulture );
		}

		/// <summary>
		/// Formats the DateTime to a string for the CurrentUICulture on the current thread.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string ToDateTimeString( string format, DateTime dt )
		{
			return dt.ToString( format, Thread.CurrentThread.CurrentUICulture );
		}

		#endregion
	}
}
