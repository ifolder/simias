/*****************************************************************************
*
* Copyright (c) [2012] Novell, Inc.
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
*                 $Author: Hegde G. G.
*                 $Modified by: hegdegg@novell.com
*                 $Mod Date: May 01, 2012

*-----------------------------------------------------------------------------
* This module is used to:
*        Check the readability of the simias database
*******************************************************************************/

using System;
using System.Net;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Security.Cryptography;
using System.Diagnostics;

using Simias;
using Simias.Storage;
using Persist = Simias.Storage.Provider;
using Simias.Authentication;
using Simias.DomainServices;
using Simias.Policy;
using Simias.Security.Web.AuthenticationService;
using Simias.Sync;
using Simias.Server;
using Simias.Client;
using Simias.Web;


namespace Simias.Storage.CheckSimisDB
{
	public class BrowserNode
	{
		private string nodeData;
		public string NodeData
		{
			get { return nodeData; }
			set { nodeData = value; }
		}
		public BrowserNode()
		{
			this.nodeData = String.Empty;
		}
		public BrowserNode( string nodeData )
		{
			this.nodeData = nodeData;
		}
	}
	public class Relationship
	{
		private const string RootID = "a9d9a742-fd42-492c-a7f2-4ec4f023c625";
		private string collectionID;
		private string nodeID;
		public string CollectionID
		{
			get { return collectionID; }
		}

		public bool IsRoot
		{
			get { return ( nodeID == RootID ) ? true : false; }
		}

		public string NodeID
		{
			get { return nodeID; }
		}
		public Relationship( string relationString )
		{
			int index = relationString.IndexOf( ':' );
			if ( index == -1 )
			{
				throw new ApplicationException( String.Format( "Invalid relationship format: {0}.", relationString ) );
			}
			nodeID = relationString.Substring( 0, index );
			collectionID = relationString.Substring( index + 1 );
		}
	}
	public class DisplayProperty
	{
		private static string FlagTag = "flags";
		private static string NameTag = "name";
		private static string TypeTag = "type";
		
		internal const uint Local		= 0x00020000;
		internal const uint MultiValued = 0x00040000;
		private XmlElement element;
		
		public string Name
		{
			get { return element.GetAttribute(NameTag); }
		}
		public string Type
		{
			get { return element.GetAttribute(TypeTag); }
		}
		public string Value
		{
			get { return (Type == "XmlDocument") ? element.InnerXml : element.InnerText; }
		}
		public uint Flags
		{
			get 
			{ 
				string flagValue = element.GetAttribute( FlagTag );
				return (flagValue != null && flagValue != String.Empty) ? Convert.ToUInt32(flagValue) : 0; 
			}
		}
		public bool IsLocal
		{
			get { return ((Flags & Local) == Local) ? true : false; }
		}
		public bool IsMultiValued
		{
			get { return ((Flags & MultiValued) == MultiValued) ? true : false; }
		}
		public DisplayProperty( XmlElement element )
		{
			this.element = element;
		}
	}
	public class DisplayNode : IEnumerable
	{
		private static string CollectionIDTag 	= "CollectionId";
		private static string IDTag 			= "id";
		private static string NameTag 			= "name";
		private static string ObjectTag 		= "Object";
		private static string TypeTag 			= "type";
		
		private XmlDocument document;
		
		public virtual string Name
		{
			get { return document.DocumentElement[ ObjectTag ].GetAttribute( NameTag ); }
		}
		public virtual string ID
		{
			get { return document.DocumentElement[ ObjectTag ].GetAttribute( IDTag ); }
		}
		public virtual string Type
		{
			get { return document.DocumentElement[ ObjectTag ].GetAttribute( TypeTag ); }
		}
		public virtual string CollectionID
		{
			get	{ return FindSingleValue( CollectionIDTag ); }
		}
		public virtual bool IsCollection
		{
			get { return ( CollectionID == ID ) ? true : false; }
		}
		public virtual XmlDocument Document
		{
			get { return document; } 
			set { document = value; }
		}
		protected DisplayNode()
		{
			document = null;
		}
		public DisplayNode( BrowserNode bNode )
		{
			document = new XmlDocument();
			document.LoadXml( bNode.NodeData );
		}
		internal string FindSingleValue( string name )
		{
			string singleValue = null;

			// Create a regular expression to use as the search string.
			Regex searchName = new Regex( "^" + name + "$", RegexOptions.IgnoreCase );

			// Walk each property node and do a case-insensitive compare on the names.
			foreach ( XmlElement x in document.DocumentElement[ ObjectTag ] )
			{
				if ( searchName.IsMatch( x.GetAttribute( NameTag ) ) )
				{
					DisplayProperty p = new DisplayProperty( x );
					singleValue = p.Value;
					break;
				}
			}

			return singleValue;
		}
		public IEnumerator GetEnumerator()
		{
			return ( document != null ) ? new DisplayNodeEnum( document ) : null;
		}
		private class DisplayNodeEnum : IEnumerator
		{
			private IEnumerator e;
			public DisplayNodeEnum( XmlDocument document )
			{
				e = document.DocumentElement[ ObjectTag ].GetEnumerator();
			}
			public void Reset()
			{
				e.Reset();
			}
			public object Current
			{
				get { return new DisplayProperty( e.Current as XmlElement ); }
			}
			public bool MoveNext()
			{
				return e.MoveNext();
			}
		}
	}
	public class CheckSimiasDB
	{
		public static bool dbg 	= false;
		public static int level = 1;

		public const string CollectionTag 		= "Collection";
		public const string DirNodeTag 			= "DirNode" ;
		public const string FileNodeTag 		= "FileNode" ;
		public const string MemberTag 			= "Member";
		public const string PolicyTag 			= "Policy";
		public const string StoreFileNodeTag	= "StoreFileNode";
		public const string TombStoneTag 		= "Tombstone";
		
		public static string SimiasDataPath 	= null;
		public static string catalogID 			= "a93266fd-55de-4590-b1c7-428f2fed815d";
		
		public static Collection catalog ;
		public static StreamWriter sw = null;
		
		public static void writeXml(XmlDocument doc, TextWriter tw)
		{
			if (doc != null)
			{
				XmlTextWriter w = new XmlTextWriter(tw);
				w.Formatting = Formatting.Indented;
				doc.WriteTo(w);
			}
		}
		
		public static void LogIt(string format, params object[] args)
		{
			//sw.WriteLine(String.Format(format, args));
			Console.WriteLine(String.Format(format, args));
		}
		
		public static void BasicRead (Store store)
		{
			LogIt ("Starting basic check");
			Domain domain = store.GetDomain(store.DefaultDomain);
			Collection c = domain;
			
			//# Check for the existance of domain.
			LogIt("Reading domains");
			ICSList domainList = store.GetDomainList();
			foreach (ShallowNode snd in domainList)
			{
				LogIt("\t{0} : {1}", snd.Name, snd.ID);
			}
			LogIt("Done");
			LogIt("Reading user information");
			ICSList members = c.GetMemberList();
			int i = 1;
			catalog = store.GetCollectionByID( catalogID );
			LogIt("\tTotal Users : {0}", members.Count);
			foreach (ShallowNode sn in members)
			{
				Member member = new Member(c, sn);
				if (dbg)LogIt("\t{0}  : {1}", i++, member.FN);
			
				ICSList cList = store.GetCollectionsByUser(member.UserID);
				foreach (ShallowNode sn1 in cList)
				{
					Collection c1 = new Collection(store, sn1);
					if (c1.IsType("iFolder"))
					{
						if (dbg) LogIt("\t\tiFolder :  {0} : {1}", sn1.ID, sn1.Name);
					}
				}
			}
			LogIt("Done");
			LogIt("Reading iFolder information");
			i=1;
			ICSList iFolderList = null;
			iFolderList = store.GetCollectionsByType("iFolder");
			LogIt("\tTotal iFolders : {0}", iFolderList.Count);
			LogIt("Reading iFolders and owners");
			foreach (ShallowNode sn in iFolderList)
			{
				Collection ifolder = store.GetCollectionByID(sn.ID);
				Member owner = domain.GetMemberByID(ifolder.Owner.UserID);
				if(dbg) LogIt("\t{0} : {1} : {2} : {3}", i++, ifolder.Name, ifolder.ID, owner.Name);
			}
			LogIt("Done");
			LogIt("Basic test successful.");
		}
		
		public static void CompleteRead (Store store)
		{
			long collectionCount 	= 0;
			long dirNodeCount		= 0;
			long fileNodeCount 		= 0;
			long memberCount 		= 0;
			long otherNodeCount 	= 0;
			long policyCount 		= 0;
			long storeFileNodeCount = 0;
			long tombStoneCount 	= 0;
			
			LogIt ("Starting advanced check");
			LogIt("Reading the whole store record by record");
			foreach(ShallowNode sn in store)
			{
				if(dbg) LogIt("\t{0} :  {1} : {2}", sn.Name, sn.ID, sn.Type);
				Collection c2 = store.GetCollectionByID(sn.CollectionID);
				if ( c2 != null )
				{	

					Node n = c2.GetNodeByID(sn.ID);
					if ( n != null)
					{
						BrowserNode bNode = new BrowserNode(n.Properties.ToString());
						DisplayNode dspNode = new DisplayNode(bNode);
						if (dbg)
						{
							foreach (DisplayProperty p in dspNode)
							{
								if(dbg) LogIt("\t\t{0} : {1} : {2} : {3} : {4} : {5}",
										p.Name, p.Type, p.Value, p.Flags, p.IsLocal, p.IsMultiValued);
							}
							LogIt("");
						}
					}
					foreach(ShallowNode sn1 in c2)
					{	
						if(dbg) LogIt("\t\t{0} : {1} : {2}", sn1.Name, sn1.ID, sn1.Type);
						
						switch ( sn1.Type )
						{
							case CollectionTag:
								collectionCount++;
								break;
							case DirNodeTag:
								dirNodeCount++;
								break;
							case FileNodeTag:
								fileNodeCount++;
								break;
							case MemberTag:
								memberCount++;
								break;
							case PolicyTag:
								policyCount++;
								break;
							case StoreFileNodeTag:
								storeFileNodeCount++;
								break;
							case TombStoneTag:
								tombStoneCount++;
								break;
							default:
								otherNodeCount++;
								break;
						}
						Collection c3 = store.GetCollectionByID(sn1.CollectionID);
						if ( c3 != null )
						{		
							Node n2 = c3.GetNodeByID(sn1.ID);
							if ( n2 != null)
							{
								BrowserNode bNode2 = new BrowserNode(n2.Properties.ToString());
								DisplayNode dspNode2 = new DisplayNode(bNode2);
								if ( dbg)
								{
									foreach (DisplayProperty p in dspNode2)
									{
										if(dbg) LogIt("\t\t\t{0} : {1} : {2} : {3} : {4} : {5}",
												p.Name, p.Type, p.Value, p.Flags, p.IsLocal, p.IsMultiValued);
									}
									LogIt("");
								}
							}
						}
					}
				}
			}
			LogIt ("\t{0, -30}:  {1}", CollectionTag, collectionCount);
			LogIt ("\t{0, -30}:  {1}", DirNodeTag, dirNodeCount);
			LogIt ("\t{0, -30}:  {1}", FileNodeTag, fileNodeCount);
			LogIt ("\t{0, -30}:  {1}", MemberTag , memberCount);
			LogIt ("\t{0, -30}:  {1}", PolicyTag, policyCount);
			LogIt ("\t{0, -30}:  {1}", StoreFileNodeTag, storeFileNodeCount);
			LogIt ("\t{0, -30}:  {1}", TombStoneTag, tombStoneCount);
			LogIt ("\t{0, -30}:  {1}", "OtherNodes", otherNodeCount);
			LogIt("Done");
			LogIt("Advanced check successful.");
		}
		
		static int Main(string[] args)
		{
			int returnValue = 0;
			if (args.Length == 1)
			{
				SimiasDataPath = args[0];
			}
			else if (args.Length == 2)
			{
				SimiasDataPath = args[0];
				level = int.Parse(args[1]);
				if (level != 2) level = 1;  // any number other than 2 is treated as 1
			}
			else
			{
				LogIt("Usage : ifolder-database-check <storepath> <level>");
				return 1;
			}
			string var = Environment.GetEnvironmentVariable("IFDBDBG");
			if ( var != null )
			{
				dbg = true; 
			}
			LogIt ("{0} - START - iFolder recoverability test.", DateTime.Now.ToString());
			string logFile = "/tmp/recoverability.log";
			DateTime startTime = DateTime.Now;
			TimeSpan brTs, crTs;
			sw = File.CreateText(logFile);
			try 
			{
				LogIt ("Trying to read simias database");
				Store.Initialize(SimiasDataPath, false, -1);//# Initialize the database
				LogIt("Done");
				Store store = Store.GetStore();
				DateTime brStartTime = DateTime.Now;
				BasicRead (store);
				DateTime brEndTime = DateTime.Now;
				brTs = brEndTime - brStartTime;
				if (level == 2)
				{
					DateTime crStartTime = DateTime.Now;
					CompleteRead (store);
					DateTime crEndTime = DateTime.Now;
					crTs = crEndTime - crStartTime;
				}
				Store.DeleteInstance();//# Closing the db
			}
			catch(Exception ex)
			{
				LogIt("Unable to check simias db. {0}", ex.StackTrace);
				returnValue = 1;
			}
			DateTime endTime = DateTime.Now;
			LogIt("Total time taken              : {0}", endTime - startTime);
			LogIt("Time taken for basic read     : {0}", brTs);
			//LogIt("Time taken for complete read  : {0}", crTs);
			LogIt("Simias Database is intact, this could be backed up for later recovery.");
			sw.Close();
			LogIt ("{0} - END - iFolder recoverability test.", DateTime.Now.ToString());
			return returnValue;
		}
	}
}
