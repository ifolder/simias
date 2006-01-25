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
			string identityDocumentPath = "../../etc/SimpleServer.xml";

			abort = false;
			try
			{
				// Load the SimpleServer domain and memberlist XML file.
				XmlDocument serverDoc = new XmlDocument();
				serverDoc.Load( identityDocumentPath );

				XmlElement domainElement = serverDoc.DocumentElement;

				XmlAttribute attr;
				//XmlNode ownerNode = null;
				for ( int i = 0; i < domainElement.ChildNodes.Count; i++ )
				{
					if ( abort == true )
					{
						// didn't finish because of an aborted mission
						return false;
					}
					
					firstName = null;
					lastName = null;

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
						}

						Property pwdProperty = null;
						XmlAttribute pwdAttr = 
							domainElement.ChildNodes[i].Attributes[ "Password" ];
						if ( pwdAttr != null )
						{
							pwdProperty = new Property( "SS:PWD", pwdAttr.Value );
							pwdProperty.LocalProperty = true;
						}
						
						// Test Property
						Property testProperty =  new Property( "SS:TEST", 1 );
						testProperty.LocalProperty = true;
						
						Property[] propertyList = { pwdProperty, testProperty };
						State.ProcessMember(
							member,
							firstName,
							lastName,
							firstName + " " + lastName,
							member,
							propertyList );
						
						/*
						memberNode = null;

						

						//
						// Check if this member already exists
						//

						Simias.Storage.Member dMember = null;
						try
						{	
							dMember = State.SDomain.GetMemberByName( member );
						}
						catch{}

						if ( dMember != null )
						{
							bool changed = false;
							
							// Check if the password has changed
							XmlAttribute pwdAttr = 
								domainElement.ChildNodes[i].Attributes[ "Password" ];
							if ( pwdAttr != null )
							{
								Property password = dMember.Properties.GetSingleProperty( "SS:PWD" );
								if ( password != null )
								{
									if ( password.Value as string != pwdAttr.Value as string )
									{
										password.Value = pwdAttr.Value;
										password.LocalProperty = true;
										dMember.Properties.ModifyProperty( password );
										changed = true;
									}
								}
								else
								{
									password = new Property( "SS:PWD", pwdAttr.Value as string );
									password.LocalProperty = true;
									dMember.Properties.ModifyProperty( password );
									changed = true;
								}
							}
							else
							{
								dMember.Properties.DeleteProperties( "SS:PWD" );
								changed = true;
							}

							//
							// Not sure if I modify a property with the same
							// value that already exists will force a node
							// update and consequently a synchronization so I'll
							// check just to be sure.
							//

							// First name change?
							if ( firstName != null )
							{
								if ( dMember.Given != firstName )
								{
									dMember.Given = firstName;
									changed = true;
								}
							}
							else
							{
								if ( dMember.Given != null && dMember.Given != "" )
								{
									dMember.Given = firstName;
									changed = true;
								}
							}

							// Last name change?
							if ( lastName != null )
							{
								if ( dMember.Family != lastName )
								{
									dMember.Family = lastName;
									changed = true;
								}
							}
							else
							{
								if ( dMember.Family != null && dMember.Family != "" )
								{
									dMember.Family = lastName;
									changed = true;
								}
							}

							if ( dMember.FN != dMember.Given + " " + dMember.Family )
							{
								dMember.FN = dMember.Given + " " + dMember.Family;
								changed = true;
							}
							
							// Must always have a DN - SimpleServer DN=CN
							Property dn = dMember.Properties.GetSingleProperty( "DN" );
							if ( dn == null || dn.Value as string != dMember.Name )
							{
								dn = new Property( "DN", dMember.Name );
								dMember.Properties.ModifyProperty( dn );
								changed = true;
							}

							// Call the sync service to finalize the processing
							// of this member.
							State.ProcessedMember( 
								dMember,
								( changed == true )
									? IdentitySync.MemberStatus.Updated
									: IdentitySync.MemberStatus.Unchanged );
						}
						else
						{
							//
							// The member didn't exist so let's create it
							//

							try
							{
								// Create a new member and then contact
								dMember = new
									Member(
										member,
										Guid.NewGuid().ToString(), 
										Simias.Storage.Access.Rights.ReadOnly,
										firstName,
										lastName);

								// Get the password
								XmlAttribute pwdAttr = 
									domainElement.ChildNodes[i].Attributes[ "Password" ];
								if ( pwdAttr != null )
								{
									Property pwd = new Property( "SS:PWD", pwdAttr.Value );
									pwd.LocalProperty = true;
									dMember.Properties.ModifyProperty( pwd );
								}

								// For simple server/sync CN=DN
								Property dn = new Property( "DN", member );
								dn.LocalProperty = true;
								dMember.Properties.ModifyProperty( dn );

								State.ProcessedMember( dMember, IdentitySync.MemberStatus.Created );
							}
							catch( Exception ex )
							{
								State.ReportError( ex.Message );
								continue;
							}
						}
						*/
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
