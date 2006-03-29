/***********************************************************************
 *  $RCSfile$
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Event;
using Simias.POBox;
using Simias.Storage;

namespace Simias.SimpleServer
{
	/// <summary>
	/// Service class used to get an execution context
	/// so we can register ourselves with the external
	/// sync container
	/// </summary>
	public class SyncProvider : Simias.IIdentitySyncProvider
	{
		#region Class Members
		private readonly string name = "Simple Synchronization";
		private readonly string description = "Simple external synchronization provider based on an identities in an XML file";
		private bool abort = false;
		
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets the name of the provider.
		/// </summary>
		public string Name { get{ return name; } }

		/// <summary>
		/// Gets the description of the provider.
		/// </summary>
		public string Description { get{ return description; } }
		#endregion

		#region Public Methods
		/// <summary>
		/// Call to abort an in process synchronization
		/// </summary>
		/// <returns>N/A</returns>
		public void Abort()
		{
			abort = true;
		}
		
		/// <summary>
		/// Call to inform a provider to start a synchronization cycle
		/// </summary>
		/// <returns> True - provider successfully finished a sync cycle, 
		/// False - provider failed the sync cycle
		/// </returns>
		public bool Start( Simias.IdentitySync.State State )
		{
			log.Debug( "Start called" );

			string member;
			string firstName;
			string lastName;
			string fullName;
			string identityDocumentPath = "../../etc/SimpleServer.xml";

			abort = false;
			try
			{
				// Load the SimpleServer domain and memberlist XML file.
				XmlDocument serverDoc = new XmlDocument();
				serverDoc.Load( identityDocumentPath );
				XmlElement domainElement = serverDoc.DocumentElement;
				XmlAttribute attr;
				
				for ( int i = 0; i < domainElement.ChildNodes.Count; i++ )
				{
					if ( abort == true )
					{
						// didn't finish because of an aborted mission
						return false;
					}
					
					firstName = null;
					lastName = null;
					fullName = null;

					attr = domainElement.ChildNodes[i].Attributes[ "Name" ];
					if (attr != null)
					{
						XmlNode cNode = domainElement.ChildNodes[i];
						member = cNode.Attributes[ "Name" ].Value;

						// Retrieve the contact properties from SimpleServer.xml
						XmlNode memberNode = domainElement.ChildNodes[i];
						for ( int x = 0; x < memberNode.ChildNodes.Count; x++ )
						{
							if ( memberNode.ChildNodes[x].Name == "First" )
							{
								firstName = memberNode.ChildNodes[x].InnerText;
							}
							else
							if ( memberNode.ChildNodes[x].Name == "Last" )
							{
								lastName = memberNode.ChildNodes[x].InnerText;
							}
							else
							if ( memberNode.ChildNodes[x].Name == "Full" )
							{
								fullName = memberNode.ChildNodes[x].InnerText;
							}
						}

						Property pwdProperty = null;
						XmlAttribute pwdAttr = 
							domainElement.ChildNodes[i].Attributes[ "Password" ];
						if ( pwdAttr != null )
						{
							pwdProperty = 
								new Property( User.pwdProperty, User.HashPassword( pwdAttr.Value ) );
							pwdProperty.LocalProperty = true;
						}
						
						if ( fullName == null )
						{
							fullName = firstName + " " + lastName;
						}
						
						Property[] propertyList = { pwdProperty };
						State.ProcessMember(
							null,
							member,
							firstName,
							lastName,
							fullName,
							member,
							propertyList );
					}
				}
			}
			catch(Exception e)
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
				State.ReportError( e.Message );
			}
			
			return true;
		}
		#endregion
	}
}
