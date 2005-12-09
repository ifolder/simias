/***********************************************************************
 *  $RCSfile$
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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Xml;

using Simias;
using Simias.Client;

namespace Simias.Storage
{
	/// <summary>
	/// Summary description for JournalEntry.
	/// </summary>
	public class JournalEntry
	{
		#region Class Members
		/// <summary>
		/// The type of change that this entry refers to.
		/// </summary>
		private string type;

		/// <summary>
		/// The name of the file that this entry refers to.
		/// </summary>
		private string fileName;

		private string fileID;

		/// <summary>
		/// The name of the user that caused this entry.
		/// </summary>
		private string userName;

		private string userID;

		/// <summary>
		/// The time that the change was made.
		/// </summary>
		private string timeStamp;
		#endregion

		#region Properties
		public string FileID
		{
			get { return fileID; }
		}

		/// <summary>
		/// Gets the filename for this entry.
		/// </summary>
		public string FileName
		{
			get { return fileName; }
			set { fileName = value; }
		}

		/// <summary>
		/// Gets the timestamp for this entry.
		/// </summary>
		public string TimeStamp
		{
			get { return timeStamp; }
		}

		/// <summary>
		/// Gets the type of this entry.
		/// </summary>
		public string Type
		{
			get { return type; }
		}

		public string UserID
		{
			get { return userID; }
		}

		/// <summary>
		/// Gets the username for this entry.
		/// </summary>
		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}
		#endregion

		#region Constructor
		public JournalEntry(string type, string fileName, string userName, string timeStamp)
		{
			this.type = type;
			this.fileName = fileName;
			this.userName = userName;
			this.timeStamp = timeStamp;
		}

		public JournalEntry(string type, string userName, string timeStamp) :
			this(type, string.Empty, userName, timeStamp)
		{
		}

		public JournalEntry(XmlNode xmlNode, Collection collection)
		{
			type = xmlNode.Name;
			XmlAttribute attr;
			IEnumerator ienum = xmlNode.Attributes.GetEnumerator();
			while ( ienum.MoveNext() )
			{
				attr = (XmlAttribute)ienum.Current;
				switch (attr.Name)
				{
					case "fnID":
/*						Node node = collection.GetNodeByID( attr.Value );
						if ( node != null )
						{
							if ( collection.IsType( node, NodeTypes.FileNodeType ) )
							{
								FileNode fileNode = new FileNode( node );
								if ( fileNode != null )
								{
									fileName = fileNode.GetRelativePath();
								}
							}
							else 
							{
								DirNode dirNode = new DirNode( node );
								if ( dirNode != null )
								{
									fileName = dirNode.GetRelativePath();
								}
							}
						}*/
						fileID = attr.Value;
						break;
					case "userID":
/*						Domain domain = collection.StoreReference.GetDomain( collection.Domain );
						if ( domain != null )
						{
							Member member = domain.GetMemberByID( attr.Value );
							userName = member.FN != null ? member.FN : member.Name;
						}*/
						userID = attr.Value;
						break;
					case "path":
						fileName = attr.Value;
						break;
					case "ts":
						timeStamp = new DateTime( long.Parse( attr.Value ) ).ToString();
						break;
				}
			}
		}
		#endregion
	}
}
