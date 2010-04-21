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
*                 $Author: Vikash Mehta <mvikash@novell.com>
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
using System.Xml;
using System.IO;
using System.Collections;
using Restore;

namespace xmlhandling
{

    class xmlTag
    {

  	public static string Filename = "Output.xml";	
	public static string domainList = Filename;
	public static string LogLocation = "";
	public static Logger DebugLog;// = new Logger(Path.Combine(LogLocation, "debug.log"));
	public static string recoverystring = "<recovery><details></details><Files></Files></recovery>";
	public static XmlDocument domainsDoc1;
	public static int count =0;	

	public xmlTag(string fileloc)
        {
  	    Filename = fileloc;
	    domainList = Filename;
	    DebugLog = new Logger(Path.Combine(Path.GetDirectoryName(fileloc), "debug.log"));
	   // initilizeXMLDoc();
        }

	public static void initilizeXMLDoc()
	{
	    //Console.WriteLine("Enter: function InitilizingXML Doc ");	
	    //domainList = Filename;	
		
	    XmlDocument domainsDoc = new XmlDocument();
	    try
	    {
		if (!File.Exists(domainList))
		{
		    domainsDoc.LoadXml(recoverystring);
		}
		else
		{
		    // Load the domain list file and clear it out.
		    domainsDoc.Load(domainList);
		    XmlNode node = domainsDoc.DocumentElement.SelectSingleNode("/Files");
		    if (node == null)
		    {
			//XmlElement element = domainsDoc.CreateElement("Files");
			//domainsDoc.AppendChild(element);
			domainsDoc.LoadXml(recoverystring);
		    }
		    else
		    {
			//TODO: at present any instance of XML doc, remove all entries and create fresh one
			//Make this function seperate and call initilizeXMLDoc explicityly not by constructor

			//node.RemoveAll();
		    }
		}
	    }
	    catch
	    {
		domainsDoc.LoadXml(recoverystring);
		//Console.WriteLine("Exception while initilizing xml doc, message:{0}", ex.Message);
	    }

            saveXmlFile(domainsDoc);
	    //Console.WriteLine("Exit: function InitilizingXML Doc ");	
	} //End of function initilizeXMLDoc
	
	public static void WriteDetailsToXML(string sectionname, string tagname, string name, string value)
	{
		Hashtable ht = new Hashtable();
		ht.Add( name, value);
		WriteDetailsToXML(sectionname, tagname, ht);
	}

	public static void WriteDetailsToXML(string sectionname, string tagname, Hashtable attributes)
	{
		XmlDocument doc = new XmlDocument();
		doc.Load(domainList);
		XmlElement element = (XmlElement)doc.DocumentElement.SelectSingleNode(sectionname);
		if( element == null)
		{
			DebugLog.Write(string.Format("No tag by name: {0} is present", sectionname));
			return ;
		}
	
		XmlElement pathelement = doc.CreateElement(tagname);
		foreach( string key in attributes.Keys)
		{
			pathelement.SetAttribute(key, (string)attributes[key]);
		}
		element.AppendChild( pathelement);
		saveXmlFile(doc);
	}

	public static void UpdateXML(string section, string tag, string attribute, string oldvalue, string newvalue)
	{
		XmlDocument doc = new XmlDocument();
		doc.Load(domainList);
		XmlElement element = (XmlElement)doc.DocumentElement.SelectSingleNode(section);
		if( element == null)
		{
			DebugLog.Write("No Details node present");
			return ;
		}	
		XmlNodeList nodeList = element.GetElementsByTagName(tag);
		XmlNode oldNode = null;
		foreach (XmlNode node in nodeList)
		{
			string value = ((XmlElement)node).GetAttribute(attribute);
			if (oldvalue == null || value.Equals(oldvalue))
			{
				oldNode = node;
				break;
			}
		} 
		XmlElement newNode = doc.CreateElement(tag);
		newNode = ((XmlElement)oldNode);
		//Create new node and update the value passed as argument for corresponding element.
		newNode.SetAttribute(attribute, newvalue);
		if (oldNode != null)
		{
			// Replace node.
			element.ReplaceChild(newNode,oldNode);
		}
		else
			element.AppendChild(newNode);

		saveXmlFile(doc);
	}

	public static string GetAttributeValue(string section, string tag, string attribute)
	{
		string returnvalue = null;
		XmlDocument doc = new XmlDocument();
		doc.Load(domainList);
		XmlElement element = (XmlElement)doc.DocumentElement.SelectSingleNode(section);
		if( element == null)
		{
			DebugLog.Write("No Details node present");
			return null;
		}	
		XmlNodeList nodeList = element.GetElementsByTagName(tag);
		XmlNode oldNode = null;
		foreach (XmlNode node in nodeList)
		{
			returnvalue = ((XmlElement)node).GetAttribute(attribute);
			break;
		} 
		return returnvalue;
	}

	public static void SetOldAdminStatus(string adminname, string status)
	{
	}

	public static void GetOldAdminStatus(string adminname, string status)
	{
	}

	public static void SetNewAdminStatus(string adminname, string status)
	{
	}

	public static void GetNewAdminStatus(string adminname, string status)
	{
	}

	public static int SetRecoveryStatus(string adminname, string status)
	{
		return 0;
	}

	public static int GetRecoveryStatus(string adminname, string status)
	{
		return 0;
	}



	public void ClearXMLDoc()
	{
	    //domainList = Filename;	
		
	    XmlDocument domainsDoc = new XmlDocument();
	    try
	    {
		if (!File.Exists(domainList))
		{
		    domainsDoc.LoadXml(recoverystring);
		}
		else
		{
		    // Load the domain list file and clear it out.
		    domainsDoc.Load(domainList);
		    XmlNode node = domainsDoc.DocumentElement.SelectSingleNode("/Files");
		    if (node == null)
		    {
			XmlElement element = domainsDoc.CreateElement("Files");
			domainsDoc.AppendChild(element);
		    }
		    else
		    {
			//TODO: at present any instance of XML doc, remove all entries and create fresh one
			//Make this function seperate and call initilizeXMLDoc explicityly not by constructor
			node.RemoveAll();
		    }
		}
	    }
	    catch(Exception ex)
	    {
		domainsDoc.LoadXml(recoverystring);
		DebugLog.Write(string.Format("Exception while initilizing xml doc, message:{0}", ex.Message));
	    }

            saveXmlFile(domainsDoc);
	}

	public static void loadxml()
	{
	    //Console.WriteLine("Enter: function addFileElement");	
	   

	    // Load the domain list file.
	    domainsDoc1 = new XmlDocument();
	    domainsDoc1.Load(domainList);

	}


	public void addFileElement(string ifolderid, string nodeid,  string filename,string type,
						  string relpath,string nodelen,string status)
	{
	  /*  //Console.WriteLine("Enter: function addFileElement");	
	    XmlDocument domainsDoc;

	    // Load the domain list file.
	    domainsDoc = new XmlDocument();
	    domainsDoc.Load(domainList);
	*/
	 
	
	    XmlElement element = (XmlElement)domainsDoc1.DocumentElement.SelectSingleNode("Files");
	    bool found = false;

	    // Look for a domain with this ID.
	    XmlNodeList nodeList = element.GetElementsByTagName("file");
	    foreach (XmlNode node in nodeList)
	    {
		string id = ((XmlElement)node).GetAttribute("nodeID");
		if (id.Equals(nodeid))
		{
		    // The domain is already in the list.
	            found = true;
		    break;
		}
	    }

	    if (!found)
	    {
		// Add the domain.

		// Create an element.
		XmlElement domain = domainsDoc1.CreateElement("file");

		// Add the attributes.
		domain.SetAttribute("ifolderID", ifolderid);
		domain.SetAttribute("nodeID", nodeid);
		domain.SetAttribute("filename", filename);
		domain.SetAttribute("type", type);
		domain.SetAttribute("relativepath", relpath);
		domain.SetAttribute("nodelength", nodelen);
		domain.SetAttribute("status", status);

		// Add the element.
		element.AppendChild(domain);
		count++;
	    }	  
  	  	
	    if(count > 500)
		{	 
            		saveXmlFile(domainsDoc1);
			count = 0;
		}
	    //Console.WriteLine("Exit: function addFileElement");	
	} //end of function addFileElement


	public static void saveXmlFile()
	{
	saveXmlFile(null);	
	}
	public static void saveXmlFile(XmlDocument doc)
	{
	    //Console.WriteLine("Enter: function saveXmlFile");	
	    // Save the config file.
	   if(doc == null)
		doc = domainsDoc1;		

	    XmlTextWriter xtw = new XmlTextWriter(domainList, System.Text.Encoding.UTF8);
	    try
	    {
		xtw.Formatting = Formatting.Indented;
		doc.WriteTo(xtw);
	    }
	    finally
	    {
		xtw.Close();
	    }	

	    //Console.WriteLine("Exit: function saveXmlFile");	
	} //End of function saveXmlFile


	public static void removeEntryFromXml(string nodeid)
	{
	    //Console.WriteLine("Enter: function remvoeEntryFromXml");	
	    XmlDocument domainsDoc;

	    // Load the domain list file.
	    domainsDoc = new XmlDocument();
	    domainsDoc.Load(domainList);

	    XmlElement element = (XmlElement)domainsDoc.DocumentElement.SelectSingleNode("/Files");


	    // Look for a domain with this ID.
	    XmlNode domainNode = null;
	    XmlNodeList nodeList = element.GetElementsByTagName("file");
	    foreach (XmlNode node in nodeList)
	    {
	    	string id = ((XmlElement)node).GetAttribute("nodeID");
		if (id.Equals(nodeid))
		{
		    domainNode = node;
		    break;
		}
	    }

	    if (domainNode != null)
	    {
	        // Remove the domain.
		element.RemoveChild(domainNode);
	    } 

	    saveXmlFile(domainsDoc);	
	    //Console.WriteLine("Exit: function removeEntryFromXml ");	
	} //End of function removeEntryFromXml

	public void updateEntryFromXml(string nodeid, string element, string data)
	{

            //Console.WriteLine("Enter: function updateEntryFromXml");
            XmlDocument domainsDoc;

            // Load the domain list file.
            domainsDoc = new XmlDocument();
            domainsDoc.Load(domainList);
            XmlElement xmlElement = (XmlElement)domainsDoc.DocumentElement.SelectSingleNode("Files");

            // Look for a domain with this ID.
            XmlNode oldNode = null;
            XmlNodeList nodeList = xmlElement.GetElementsByTagName("file");
            foreach (XmlNode node in nodeList)
            {
                string id = ((XmlElement)node).GetAttribute("nodeID");
                if (id.Equals(nodeid))
                {
                    oldNode = node;
                    break;
                }
            }

            // Create an element.
            XmlElement newNode = domainsDoc.CreateElement("file");
            newNode = ((XmlElement)oldNode);

            //Create new node and update the value passed as argument for corresponding element.    
            newNode.SetAttribute(element, data);
            if (oldNode != null)
            {
                // Replace node.
                xmlElement.ReplaceChild(newNode,oldNode);
            }
            saveXmlFile(domainsDoc);
            //Console.WriteLine("Exit: function updateEntryFromXml ");

	} //End of function updateEntryFromXml

	public int GetProgress()
	{
		int totfiles=0;
		int finishedfiles=0;
		XmlDocument recordDoc = new XmlDocument();
		if (File.Exists(Filename))
		{
			recordDoc.Load(Filename);
			XmlElement element = (XmlElement)recordDoc.DocumentElement.SelectSingleNode("Files");
			XmlNodeList nodeList = element.GetElementsByTagName("file");
			totfiles = nodeList.Count;
			string curStatus;
			foreach (XmlNode node1 in nodeList)
			{
				curStatus = ((XmlElement)node1).GetAttribute("status");
				if (!curStatus.Equals("Completed"))
					finishedfiles++;
			} 
		}
		if( totfiles != 0)
			return (int)(finishedfiles*100)/totfiles;
		else
			return 0;
	}

        public string VerifyStatus()
        {
		string status = "NotStarted";
            XmlDocument recordDoc = new XmlDocument();
            try{
                if (File.Exists(Filename))
                {
            
			status = GetAttributeValue( "details", "Status", "value");
			if( status == null || status == string.Empty)
				status = "UnKnown";
		}
		}
		catch(Exception ex)
		{
			Console.WriteLine("Exception in VerifyStatus: {0}", ex.Message);
		}
			if( status.Equals("Completed") )
			{
				/// Check if the members are removed...
				string adminaddedflag;
				string adminstatus;
				adminaddedflag = GetAttributeValue( "details", "oldadmin", "value");
				adminstatus = GetAttributeValue( "details", "oldadmin", "status");
				if( adminstatus.Equals( "Removed") == false && adminaddedflag.Equals( true.ToString() ) == false)
				{
					/// admin is added by tool and the admin member is not removed...
					status = "RemoveMembers";
				}
				else
				{
					adminaddedflag = GetAttributeValue( "details", "newadmin", "value");
					adminstatus = GetAttributeValue( "details", "newadmin", "status");
					if( adminstatus.Equals( "Removed") == false && adminaddedflag.Equals( true.ToString()) == false)
					{
						/// admin is added by tool and the admin member is not removed...
						status = "RemoveMembers";
					}
				}
			}

		return status;
	}

	/*
        public string VerifyStatus(out int totalitems, out int failedCount)
        {
		string status = "NotStarted";
		totalitems = 0;
		failedCount = 0;
            XmlDocument recordDoc = new XmlDocument();
            try{
                if (File.Exists(Filename))
                {
            
			status = GetAttributeValue( "details", "Status", "value");
			if( status == null || status == string.Empty)
				status = "UnKnown";
		        recordDoc.Load(Filename);
                    XmlElement element = (XmlElement)recordDoc.DocumentElement.SelectSingleNode("Files");
                    //XmlNode domainNode = null;
                    XmlNodeList nodeList = element.GetElementsByTagName("file");
                    string curStatus = null;
                    foreach (XmlNode node1 in nodeList)
                    {
			totalitems++;
                        curStatus = ((XmlElement)node1).GetAttribute("status");
                        if (!curStatus.Equals("Completed"))
                        {
                            failedCount++;
                        }
                    }
			if( status.Equals("Completed") )
			{
				/// Check if the members are removed...
				string adminaddedflag;
				string adminstatus;
				adminaddedflag = GetAttributeValue( "details", "oldadmin", "value");
				adminstatus = GetAttributeValue( "details", "oldadmin", "status");
				if( adminstatus.Equals( "Removed") == false && adminaddedflag.Equals( true.ToString() ) == false)
				{
					/// admin is added by tool and the admin member is not removed...
					status = "RemoveMembers";
				}
				else
				{
					adminaddedflag = GetAttributeValue( "details", "newadmin", "value");
					adminstatus = GetAttributeValue( "details", "newadmin", "status");
					if( adminstatus.Equals( "Removed") == false && adminaddedflag.Equals( true.ToString()) == false)
					{
						/// admin is added by tool and the admin member is not removed...
						status = "RemoveMembers";
					}
				}
			}
                }
            }
            catch(Exception ex)
            {
                recordDoc.LoadXml(recoverystring);
                DebugLog.Write(string.Format("Exception while initilizing xml doc, message:{0}", ex.Message));
            }

            DebugLog.Write(string.Format("Exit: function Veify Doc:{0} ",failedCount.ToString()));
		return status;
            //return failedCount;
        } //End of function VerifyStatus
	*/

    } //end of class xmlTag

		
	class MainClass
	{
		/*
		public static void Main(string[] args)
		{

			xmlTag xmltag = new xmlTag("ifolderid.xml");
			xmltag.ClearXMLDoc();
			xmlTag.initilizeXMLDoc();
			Hashtable ht = new Hashtable();
			ht.Add("relativepath", "abc1if1/fol1/file.txt");
			ht.Add("fullpath", "/home/banderso");
			xmlTag.WriteDetailsToXML("details", "relativepath", ht);
			ht.Clear();
			ht.Add("name","admin");
			ht.Add("ID", "jsej393w");
			ht.Add("Status", "NotAdded");
			xmlTag.WriteDetailsToXML("details", "oldadmin", ht);
			ht.Add("addednew", "value");
			//xmlTag.UpdateXML("details", "oldadmin", "name11", "admin", "updatedadmin");
			string str = xmlTag.GetAttributeValue("details", "oldadmin", "name");
			Console.WriteLine("value: {0}", str);
		//	xmlTag.WriteDetailsToXML("details", "newadmin", "admin1", "true");
		//	xmltag.addFileElement("1", "2", "3", "4", "5", "6", "7");
		}
		*/
	}
	
	

} //End of namespace xmlhandling
