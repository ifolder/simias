/*****************************************************************************
* Copyright © [2007-08] Unpublished Work of Novell, Inc. All Rights Reserved.
*
* THIS IS AN UNPUBLISHED WORK OF NOVELL, INC.  IT CONTAINS NOVELL'S CONFIDENTIAL, 
* PROPRIETARY, AND TRADE SECRET INFORMATION.	NOVELL RESTRICTS THIS WORK TO 
* NOVELL EMPLOYEES WHO NEED THE WORK TO PERFORM THEIR ASSIGNMENTS AND TO 
* THIRD PARTIES AUTHORIZED BY NOVELL IN WRITING.  THIS WORK MAY NOT BE USED, 
* COPIED, DISTRIBUTED, DISCLOSED, ADAPTED, PERFORMED, DISPLAYED, COLLECTED,
* COMPILED, OR LINKED WITHOUT NOVELL'S PRIOR WRITTEN CONSENT.  USE OR 
* EXPLOITATION OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE 
* PERPETRATOR TO CRIMINAL AND  CIVIL LIABILITY.
*
* Novell is the copyright owner of this file.  Novell may have released an earlier version of this
* file, also owned by Novell, under the GNU General Public License version 2 as part of Novell's 
* iFolder Project; however, Novell is not releasing this file under the GPL.
*
*-----------------------------------------------------------------------------
*
*                 Novell iFolder Enterprise
*
*-----------------------------------------------------------------------------
*
*                 $Author: Rob
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
using System;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

using Simias.Storage;
using Simias.Web;
using Simias.POBox;

namespace iFolder.WebService
{
	/// <summary>
	/// Settings
	/// </summary>
	[Serializable]
	public class Settings
	{
		/// <summary>
		/// Settings Collection Type
		/// </summary>
		public static string SettingsCollectionType = "Settings";

		/// <summary>
		/// Private Constructor
		/// </summary>
		private Settings()
		{
		}
		
		/// <summary>
		/// Get a setting global to the system.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting.</returns>
		public static string GetSystemSetting(string name)
		{
			Store store = Store.GetStore();

			// find the domain
			Domain domain = store.GetDomain(store.DefaultDomain);

			return Settings.GetSetting(domain, name);
		}

		/// <summary>
		/// Set a setting global to the system.
		/// </summary>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		public static void SetSystemSetting(string name, string value)
		{
			Store store = Store.GetStore();

			// find the domain
			Domain domain = store.GetDomain(store.DefaultDomain);

			Settings.SetSetting(domain, name, value);
		}
		
		/// <summary>
		/// Get a setting specific to a user.
		/// </summary>
		/// <param name="collectionID">The id of the collection.</param>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting.</returns>
		public static string GetCollectionSetting(string collectionID, string name)
		{
			Store store = Store.GetStore();

			// find the collection
			Collection c = store.GetCollectionByID(collectionID);

			if (c == null)
			{
				// collection does not exist
				throw new iFolderDoesNotExistException(collectionID);
			}

			return Settings.GetSetting(c, name);
		}

		/// <summary>
		/// Set a setting specific to a collection.
		/// </summary>
		/// <param name="collectionID">The id of the collection.</param>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		public static void SetCollectionSetting(string collectionID, string name, string value)
		{
			Store store = Store.GetStore();

			// find the collection
			Collection c = store.GetCollectionByID(collectionID);

			if (c == null)
			{
				// collection does not exist
				throw new iFolderDoesNotExistException(collectionID);
			}
			
			Settings.SetSetting(c, name, value);
		}
		
		/// <summary>
		/// Get a setting specific to a user.
		/// </summary>
		/// <param name="userID">The id of the user.</param>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting.</returns>
		public static string GetUserSetting(string userID, string name)
		{
			Store store = Store.GetStore();

			// get the POBox
			POBox box = POBox.GetPOBox(store, store.DefaultDomain, userID);

			if (box == null)
			{
				// user or POBox does not exist
				throw new UserDoesNotExistException(userID);
			}

			return Settings.GetSetting(box, name);
		}

		/// <summary>
		/// Set a setting specific to a user.
		/// </summary>
		/// <param name="userID">The id of the user.</param>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		public static void SetUserSetting(string userID, string name, string value)
		{
			Store store = Store.GetStore();

			// get the POBox
			POBox box = POBox.GetPOBox(store, store.DefaultDomain, userID);

			if (box == null)
			{
				// user or POBox does not exist
				throw new UserDoesNotExistException(userID);
			}
			
			Settings.SetSetting(box, name, value);
		}
		
		/// <summary>
		/// Get a setting specific to a collection.
		/// </summary>
		/// <param name="c">The collection object.</param>
		/// <param name="name">The name of the setting.</param>
		/// <returns>The value of the setting.</returns>
		public static string GetSetting(Collection c, string name)
		{
			string result = null;

			// find the settings node
			Node node = c.GetSingleNodeByType(SettingsCollectionType);
		
			if (node != null)
			{
				Property prop = node.Properties.GetSingleProperty(name);

				if (prop != null)
				{
					result = (string) prop.Value;
				}
			}

			return result;
		}
		
		/// <summary>
		/// Set a setting specific to a collection.
		/// </summary>
		/// <param name="c">The collection object.</param>
		/// <param name="name">The name of the setting.</param>
		/// <param name="value">The value of the setting.</param>
		public static void SetSetting(Collection c, string name, string value)
		{
			// find the settings node
			Node node = c.GetSingleNodeByType(SettingsCollectionType);
		
			if (node == null)
			{
				node = new Node(SettingsCollectionType);
				c.SetType(node, SettingsCollectionType);
			}

			// set value
			node.Properties.ModifyProperty(name, value);

			// commit
			c.Commit(node);
		}
	}
}
