/***********************************************************************
 *  $RCSfile: iFolderWebClient.java,v $
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

import java.io.*;
import java.util.Date;
import java.text.DateFormat;
import java.net.*;

import org.apache.axis.*;
import org.w3c.dom.*;

import com.novell.ifolder.web.*;

// iFolder Web Access Java Client
public class iFolderWebClient
{
    // main
    public static void main(String args[])
    {
        try
        {
            final int MAXBUFFER = 64 * 1024;
            final String https = "https";
            boolean quit = false;
            String username = "SimiasAdmin";
            String password = "novell";
            String context = "/";
    
            System.out.println();
            System.out.println("iFolder Web Client - Java");
            System.out.println();
    
            // connect
            IFolderWebAccessLocator loc = new IFolderWebAccessLocator();
            loc.setMaintainSession(true);
    
            IFolderWebAccessSoap web;
    
            // uri
            URL url = new URL(loc.getiFolderWebAccessSoapAddress());
    
            if (args.length > 0)
            {
                URL tempUrl = new URL(args[0]);
    
                url = new URL(tempUrl.getProtocol(), tempUrl.getHost(), tempUrl.getPort(), url.getFile());
            }
            System.out.println("URL: " + url.toString());
    
            web = loc.getiFolderWebAccessSoap(url);
    
            // ssl
            if (url.getProtocol().startsWith(https))
            {
                // TODO
            }
    
            // credentials
            if (args.length > 2)
            {
                username = args[1];
                password = args[2];
            }
            System.out.println("User: " + username);
            System.out.println();
    
            ((IFolderWebAccessSoapStub) web).setUsername(username);
            ((IFolderWebAccessSoapStub) web).setPassword(password);

            // session
            ((IFolderWebAccessSoapStub) web).setMaintainSession(true);

            // authenticator (for handler calls)
            Authenticator.setDefault(new iFolderAuthenticator(username, password));

            // session (for handler calls)
            String sessionID = null;

            // reader
            BufferedReader reader = new BufferedReader(new InputStreamReader(System.in));

            while(!quit)
            {
                // prompt
                System.out.println();
                System.out.println("[" + context + "]");
                System.out.print("ifolder> ");
                String response = reader.readLine();
                System.out.println();

                if ((response == null) || (response.length() == 0))
                {
                    continue;
                }
                
                // command
                String command = response;
                String argument = null;
                int index = response.indexOf(' ');

                if (index != -1)
                {
                    command = response.substring(0, index);

                    if (response.length() > (index + 1))
                    {
                        argument = response.substring(index + 1);
                    }
                }

                // update the path with the argument
                String path = pathCombine(context, argument);
                System.out.println("Path: " + path);
                System.out.println();

                // determine ifolder
                String ifolderName = parseiFolder(path);
                IFolder ifolder = null;
                if (ifolderName != null)
                {
                    try
                    {
                        ifolder = web.getiFolderByName(ifolderName);
                    }
                    catch(Exception e)
                    {
                        // ignore
                    }
                }

                // switch on the first letter of the command
                try
                {
                    switch(Character.toLowerCase(command.charAt(0)))
                    {
                        // change context
                        case 'c':
                        {
                            if (ifolder != null)
                            {
                                IFolderEntry entry = web.getEntryByPath(ifolder.getID(), path);
                        
                                if (entry.isIsDirectory())
                                {
                                    context = path;
                                }
                                else
                                {
                                    System.out.println("Error: " + path + " is not a directory.");
                                }
                            }
                            else
                            {
                                context = "/";
                            }
                        }
                        break;
                        
                        // download a file
                        case 'd':
                        {
                            IFolderEntry entry = web.getEntryByPath(ifolder.getID(), path);
                        
                            long start = System.currentTimeMillis();

                            String fileID = web.openFileRead(ifolder.getID(), entry.getID());
                        
                            FileOutputStream stream = null;
                        
                            try
                            {
                                stream = new FileOutputStream(entry.getName());
                        
                                byte[] buffer;
                                
                                while((buffer = web.readFile(fileID, MAXBUFFER)) != null)
                                {
                                    stream.write(buffer);
                                    System.out.print(".");
                                }
                            }
                            finally
                            {
                                web.closeFile(fileID);

                                if (stream != null)
                                {
                                    stream.close();
                                }
                            }

                            System.out.println(entry.getName() + " (" + (System.currentTimeMillis() - start) / 1000.0 + ")");
                        }
                        break;
                        
                        // get a file (handler)
                        case 'g':
                        {
                            IFolderEntry entry = web.getEntryByPath(ifolder.getID(), path);

                            long start = System.currentTimeMillis();

                            String handlerPath = "/simias10/Download.ashx?iFolder="
                                + ifolder.getID() + "&Entry=" + entry.getID();

                            URL handlerUrl = new URL(url.getProtocol(), url.getHost(), url.getPort(), handlerPath);

                            HttpURLConnection con = (HttpURLConnection) handlerUrl.openConnection();
                            con.setRequestMethod("GET");

                            // use session if exists
                            if (sessionID != null)
                            {
                                con.setRequestProperty("Cookie", sessionID);
                            }

                            InputStream in = con.getInputStream();

                            FileOutputStream out = null;

                            try
                            {
                                out = new FileOutputStream(entry.getName());

                                byte[] buffer = new byte[MAXBUFFER];
                                int len;

                                while((len = in.read(buffer, 0, MAXBUFFER)) > 0)
                                {
                                    out.write(buffer, 0, len);
                                    System.out.print(".");
                                }
                            }
                            finally
                            {
                                in.close();

                                if (out != null)
                                {
                                    out.close();
                                }
                            }

                            System.out.println(entry.getName() + " (" + (System.currentTimeMillis() - start) / 1000.0 + ")");

                            // save session
                            String cookie = con.getHeaderField("Set-Cookie");

                            if (cookie != null)
                            {
                                sessionID = cookie.substring(0, cookie.indexOf(';'));
                            }
                        }
                        break;

                        // help
                        case 'h':
                            System.out.println("change download get help list make put quit remove upload");
                            break;
                        
                        // list entries or iFolders
                        case 'l':
                        {
                            if (ifolder == null)
                            {
                                IFolder[] ifolders = web.getiFolders().getIFolder();
                            
                                for(int i=0; i < ifolders.length; i++)
                                {
                                    System.out.println(ifolders[i].getName() + "\t" + ifolders[i].getOwnerName());
                                }
                            }
                            else
                            {
                                IFolderEntry parent = web.getEntryByPath(ifolder.getID(), path);
                            
                                IFolderEntry[] entries = web.getEntriesByParent(parent.getIFolderID(), parent.getID()).getIFolderEntry();
                            
                                if (entries != null)
                                {
                                    for(int i=0; i < entries.length; i++)
                                    {
                                        IFolderEntry entry = entries[i];
    
                                        String tag = " ";
                                        
                                        if (entry.isIsDirectory())
                                        {
                                            if (entry.isHasChildren())
                                            {
                                                tag = "+";
                                            }
                                            else
                                            {
                                                tag = "-";
                                            }
                                        }
                                        else
                                        {
                                            tag = "#";
                                        }
                                        
                                        Date time = entry.getModifiedTime().getTime();

                                        System.out.println(DateFormat.getInstance().format(time)
                                            + "\t" + entry.getSize()
                                            + "\t" + tag + " " + entry.getName());
                                    }
                                }
                            }
                        }
                        break;
                        
                        // make directories or iFolders
                        case 'm':
                        {
                            String name = getEntry(path);
                            String parentPath = getParent(path);
                            
                            if (name != null)
                            {
                                if (parentPath == null)
                                {
                                    web.createiFolder(name);
                                }
                                else
                                {
                                    IFolderEntry parent = web.getEntryByPath(ifolder.getID(), parentPath);
                                    IFolderEntry child = web.createDirectoryEntry(ifolder.getID(), parent.getID(), name);
                                }
                            }
                        }
                        break;
                        
                        // put a file (handler)
                        case 'p':
                        {
                            String parentPath = getParent(path);
                            String entryName = getEntry(path);
                            IFolderEntry entry = null;

                            try
                            {
                                entry = web.getEntryByPath(ifolder.getID(), path);
                            }
                            catch(Exception e)
                            {
                                // ignore
                            }

                            if (entry == null)
                            {
                                // create entry
                                IFolderEntry parent = web.getEntryByPath(ifolder.getID(), parentPath);

                                entry = web.createFileEntry(ifolder.getID(), parent.getID(), entryName);
                            }

                            long start = System.currentTimeMillis();

                            String handlerPath = "/simias10/Upload.ashx?iFolder="
                                + ifolder.getID() + "&Entry=" + entry.getID();

                            URL handlerUrl = new URL(url.getProtocol(), url.getHost(), url.getPort(), handlerPath);

                            HttpURLConnection con = (HttpURLConnection) handlerUrl.openConnection();
                            con.setRequestMethod("PUT");
                            con.setDoOutput(true);

                            // use session if exists
                            if (sessionID != null)
                            {
                                con.setRequestProperty("Cookie", sessionID);
                            }

                            OutputStream out = con.getOutputStream();

                            FileInputStream in = null;


                            try
                            {
                                in = new FileInputStream(entry.getName());

                                byte[] buffer = new byte[MAXBUFFER];

                                int len;

                                while((len = in.read(buffer)) > 0)
                                {
                                    out.write(buffer, 0, len);
                                    System.out.print(".");
                                }
                            }
                            finally
                            {
                                out.close();

                                if (in != null)
                                {
                                    in.close();
                                }
                            }

                            System.out.println(entry.getName() + " (" + (System.currentTimeMillis() - start) / 1000.0 + ")");

                            // save session
                            String cookie = con.getHeaderField("Set-Cookie");

                            if (cookie != null)
                            {
                                sessionID = cookie.substring(0, cookie.indexOf(';'));
                            }
                        }
                        break;

                        // quit
                        case 'q':
                            quit = true;
                            break;
                        
                        // remove entries or iFolders
                        case 'r':
                        {
                            String name = getEntry(path);
                            String parentPath = getParent(path);
                            
                            if (name != null)
                            {
                                if (parentPath == null)
                                {
                                    web.deleteiFolder(ifolder.getID());
                                }
                                else
                                {
                                    IFolderEntry entry = web.getEntryByPath(ifolder.getID(), path);
                                    web.deleteEntry(ifolder.getID(), entry.getID());
                                }
                            }
                        }
                        break;
                        
                        // upload a file
                        case 'u':
                        {
                            String parentPath = getParent(path);
                            String entryName = getEntry(path);
                            IFolderEntry entry = null;
                            
                            try
                            {
                                entry = web.getEntryByPath(ifolder.getID(), path);
                            }
                            catch(Exception e)
                            {
                                // ignore
                            }
                            
                            if (entry == null)
                            {
                                // create entry
                                IFolderEntry parent = web.getEntryByPath(ifolder.getID(), parentPath);
                                
                                entry = web.createFileEntry(ifolder.getID(), parent.getID(), entryName);
                            }
                            
                            long start = System.currentTimeMillis();

                            String fileID = null;
                            
                            FileInputStream stream = null;
                            
                            try
                            {
                                stream = new FileInputStream(entry.getName());
                                
                                fileID = web.openFileWrite(ifolder.getID(), entry.getID(),
                                    (new File(entry.getName())).length());

                                byte[] buffer = new byte[MAXBUFFER];
                                
                                int count;
                                
                                while((count = stream.read(buffer)) > 0)
                                {
                                    byte[] send = buffer;
                                
                                    if (send.length != count)
                                    {
                                        byte[] temp = new byte[count];
                                        System.arraycopy(buffer, 0, temp, 0, count);
                                        send = temp;
                                    }
                                    
                                    web.writeFile(fileID, send);
                                    System.out.print(".");
                                }
                            }
                            finally
                            {
                                web.closeFile(fileID);

                                if (stream != null)
                                {
                                    stream.close();
                                }
                            }

                            System.out.println(entry.getName() + " (" + (System.currentTimeMillis() - start) / 1000.0 + ")");
                        }
                        break;
                        
                        // bad command
                        default:
                            System.out.println("Bad Command: " + command);
                            break;
                    }
                }
                catch(AxisFault se)
                {
                    // soap exception
                    String type = se.getClass().getName();

                    try
                    {
                        Element[] details = se.getFaultDetails();

                        type = details[0].getAttribute("type");

                    }
                    catch(Exception e)
                    {
                        // ignore
                    }

                    // trim
                    int idx = type.lastIndexOf('.');

                    if (idx != -1)
                    {
                        type = type.substring(idx + 1);
                    }

                    System.out.println("Error: " + se.getMessage() + " (" + type + ")");
                }
                catch(Exception e)
                {
                    String type = e.getClass().getName();

                    // trim
                    int idx = type.lastIndexOf('.');

                    if (idx != -1)
                    {
                        type = type.substring(idx + 1);
                    }

                    System.out.println("Error: " + e.getMessage() + " (" + e.getClass().getName() + ")");
                }
            }
        }
        catch(Exception e)
        {
            System.out.println(e);
        }
    }

    // parse the iFolder name from the path
    static String parseiFolder(String path)
    {
        String result = null;

        if ((path != null) && (path.length() > 1))
        {
            int index = path.indexOf('/', 1);

            if (index == -1)
            {
                result = path.substring(1);
            }
            else
            {
                result = path.substring(1, index);
            }
        }

        return result;
    }

    // combine paths
    static String pathCombine(String path1, String path2)
    {
        String result = null;
        
        if ((path2 == null) || (path2.length() == 0))
        {
            result = path1;
        }
        else if (path2.startsWith("/"))
        {
            result = path2;
        }
        else if (path2.equals(".."))
        {
            int index = path1.lastIndexOf('/');
        
            if (index < 1) index = 1;
        
            result = path1.substring(0, index);
        }
        else
        {
            if (path1.endsWith("/"))
            {
                result = path1 + path2;
            }
            else
            {
                result = path1 + "/" + path2;
            }
        }
        
        return result;
    }

    // get the parent path
    static String getParent(String path)
    {
        String result = null;

        if ((path != null) && (path.length() > 1) && (path.lastIndexOf('/') > 0))
        {
            result = path.substring(0, path.lastIndexOf('/'));
        }

        return result;
    }

    // get the entry name
    static String getEntry(String path)
    {
        String result = null;

        if ((path != null) && (path.length() > 1))
        {
            result = path.substring(path.lastIndexOf('/') + 1);
        }

        return result;
    }
}

