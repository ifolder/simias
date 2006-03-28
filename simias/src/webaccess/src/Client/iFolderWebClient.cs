/***********************************************************************
 *  $RCSfile: iFolderWebClient.cs,v $
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
using System.Net;
using System.Web.Services.Protocols;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// iFolder Web Access Client
	/// </summary>
	class iFolderWebClient
	{
		/// <summary>
		/// Main
		/// </summary>
		/// <param name="args">Command Arguments</param>
		static void Main(string[] args)
		{
            const int MAXBUFFER = 64 * 1024;
            const string https = "https";
			bool quit = false;
			string username = "admin";
			string password = "simias";
			string context = "/";

			Console.WriteLine();
			Console.WriteLine("iFolder Web Client - C#");
			Console.WriteLine();

			// connect
			iFolderWeb web = new iFolderWeb();
			
			// uri
			if (args.Length >= 1)
			{
				// update url
				UriBuilder newUri = new UriBuilder(args[0]);
				newUri.Path = (new Uri(web.Url)).PathAndQuery;
				web.Url = newUri.Uri.ToString();
			}
			Console.WriteLine("URL: {0}", web.Url);

			// ssl
			if (web.Url.StartsWith(https))
			{
				ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
			}

			// credentials
			if (args.Length >= 3)
			{
				username = args[1];
				password = args[2];
			}
			Console.WriteLine("User: {0}", username);
			Console.WriteLine();

			web.Credentials = new NetworkCredential(username, password);

			// cookies
			web.CookieContainer = new CookieContainer();

			while(!quit)
			{
				// prompt
				Console.WriteLine();
				Console.WriteLine("[{0}]", context);
				Console.Write("ifolder> ");
				string response = Console.ReadLine();
				Console.WriteLine();

				if ((response == null) || (response.Length == 0))
				{
					continue;
				}

				// command
				string command = response;
				string argument = null;
				int index = response.IndexOf(' ');

				if (index != -1)
				{
					command = response.Substring(0, index);

					if (response.Length > (index + 1))
					{
						argument = response.Substring(index + 1);
					}
				}

				// update the path with the argument
				string path = PathCombine(context, argument);
				Console.WriteLine("Path: {0}", path);
				Console.WriteLine();

				// determine ifolder
				string ifolderName = ParseiFolder(path);
				iFolder ifolder = null;
				if (ifolderName != null)
				{
					try
					{
						ifolder = web.GetiFolderByName(ifolderName);
					}
					catch
					{
						// ignore
					}
				}
				
				// switch on the first letter of the command
				try
				{
					switch(Char.ToLower(command[0]))
					{
						// change context
						case 'c':
						{
							if (ifolder != null)
							{
								iFolderEntry entry = web.GetEntryByPath(ifolder.ID, path);
									
								if (entry.IsDirectory)
								{
									context = path;
								}
								else
								{
									Console.WriteLine("Error: {0} is not a directory.", path);
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
							iFolderEntry entry = web.GetEntryByPath(ifolder.ID, path);

							DateTime start = DateTime.Now;

							string fileID = web.OpenFileRead(ifolder.ID, entry.ID);

							FileStream stream = null;
							
							try
							{
								stream = File.Create(entry.Name);

								byte[] buffer;

								while((buffer = web.ReadFile(fileID, MAXBUFFER)) != null)
								{
									stream.Write(buffer, 0, buffer.Length);
									Console.Write(".");
								}
							}
							finally
							{
								web.CloseFile(fileID);
								if (stream != null)
								{
									stream.Close();
								}
							}

							Console.WriteLine("{0} ({1})", entry.Name, DateTime.Now.Subtract(start));
						}
							break;

						// get a file (handler)
						case 'g':
						{
							iFolderEntry entry = web.GetEntryByPath(ifolder.ID, path);

							DateTime start = DateTime.Now;

							UriBuilder uri = new UriBuilder(web.Url);
							
							uri.Path = String.Format("/simias10/Download.ashx?iFolder={0}&Entry={1}",
								ifolder.ID, entry.ID);

							HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri.Uri);
							request.Method = "GET";
							request.Credentials = web.Credentials;
							request.CookieContainer = web.CookieContainer;

							HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();

							Stream webStream = webResponse.GetResponseStream();

							FileStream stream = null;
							
							try
							{
								stream = File.Create(entry.Name);

								byte[] buffer = new byte[MAXBUFFER];
								int count = 0;

								while((count = webStream.Read(buffer, 0, MAXBUFFER)) > 0)
								{
									stream.Write(buffer, 0, count);
								}
							}
							finally
							{
								if (stream != null)
								{
									stream.Close();
								}

								webStream.Close();
							}

							Console.WriteLine("{0} ({1})", entry.Name, DateTime.Now.Subtract(start));
						}
							break;

						// help
						case 'h':
							Console.WriteLine("change download get help list make put quit remove upload xhistory");
							break;

						// list entries or iFolders
						case 'l':
                        {
							if (ifolder == null)
							{
								int total;
								iFolder[] ifolders = web.GetiFolders(0, 0, out total);

								for(int i=0; i < ifolders.Length; i++)
								{
									Console.WriteLine("{0,-24}\t{1}", ifolders[i].Name, ifolders[i].OwnerName);
								}
							}
							else
							{
								iFolderEntry parent = web.GetEntryByPath(ifolder.ID, path);

								int total = 0;
								iFolderEntry[] entries = web.GetEntries(parent.iFolderID, parent.ID, 0, 0, out total);
		
								foreach(iFolderEntry entry in entries)
								{
									string tag = " ";

									if (entry.IsDirectory)
									{
										if (entry.HasChildren)
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
						
									Console.WriteLine("{0,12}\t{1,12}\t{2} {3}", entry.LastModified, entry.Size, tag, entry.Name);
								}
							}
						}
							break;

						// make directories or iFolders
						case 'm':
						{
                            string name = GetEntry(path);
							string parentPath = GetParent(path);

							if (name != null)
							{
								if (parentPath == null)
								{
									web.CreateiFolder(name, null);
								}
								else
								{
									iFolderEntry parent = web.GetEntryByPath(ifolder.ID, parentPath);
									iFolderEntry child = web.CreateEntry(ifolder.ID, parent.ID, iFolderEntryType.Directory, name);
								}
							}
						}
							break;

						// put a file (handler)
						case 'p':
						{
							string parentPath = GetParent(path);
							string entryName = GetEntry(path);
							iFolderEntry entry = null;
							
							try
							{
								entry = web.GetEntryByPath(ifolder.ID, path);
							}
							catch { }

							if (entry == null)
							{
								// create entry
								iFolderEntry parent = web.GetEntryByPath(ifolder.ID, parentPath);

								entry = web.CreateEntry(ifolder.ID, parent.ID, iFolderEntryType.File, entryName);
							}

							DateTime start = DateTime.Now;

							// put
							UriBuilder uri = new UriBuilder(web.Url);
							
							uri.Path = String.Format("/simias10/Upload.ashx?iFolder={0}&Entry={1}",
								ifolder.ID, entry.ID);

							HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri.Uri);
							request.Method = "PUT";
							request.Credentials = web.Credentials;
							request.CookieContainer = web.CookieContainer;

							Stream webStream = request.GetRequestStream();

							FileStream stream = null;
							
							try
							{
								stream = File.OpenRead(entry.Name);

								byte[] buffer = new byte[MAXBUFFER];

								int count;
								
								while((count = stream.Read(buffer, 0, buffer.Length)) > 0)
								{
									webStream.Write(buffer, 0, count);
								}
							}
							finally
							{
								if (stream != null)
								{
									stream.Close();
								}
								
								webStream.Close();
							}

							request.GetResponse();

							Console.WriteLine("{0} ({1})", entry.Name, DateTime.Now.Subtract(start));
						}
							break;

						// quit
						case 'q':
							quit = true;
							break;

						// remove entries or iFolders
						case 'r':
						{
							string name = GetEntry(path);
							string parentPath = GetParent(path);

							if (name != null)
							{
								if (parentPath == null)
								{
									web.DeleteiFolder(ifolder.ID);
								}
								else
								{
									iFolderEntry entry = web.GetEntryByPath(ifolder.ID, path);
									web.DeleteEntry(ifolder.ID, entry.ID);
								}
							}
						}
							break;

						// upload a file
						case 'u':
						{
							string parentPath = GetParent(path);
							string entryName = GetEntry(path);
							iFolderEntry entry = null;
							
							try
							{
								entry = web.GetEntryByPath(ifolder.ID, path);
							}
							catch { }

							if (entry == null)
							{
                                // create entry
                                iFolderEntry parent = web.GetEntryByPath(ifolder.ID, parentPath);

								entry = web.CreateEntry(ifolder.ID, parent.ID, iFolderEntryType.File, entryName);
							}

							DateTime start = DateTime.Now;

							FileStream stream = null;
							
							string fileID = null;

							try
							{
								stream = File.OpenRead(entry.Name);

								fileID = web.OpenFileWrite(ifolder.ID, entry.ID, stream.Length);

								byte[] buffer = new byte[MAXBUFFER];

								int count;
								
								while((count = stream.Read(buffer, 0, buffer.Length)) > 0)
								{
									byte[] send = buffer;

									if (send.Length != count)
									{
										byte[] temp = new byte[count];
										Array.Copy(buffer, temp, count);
										send = temp;
									}

									web.WriteFile(fileID, send);
									Console.Write(".");
								}
							}
							finally
							{
								web.CloseFile(fileID);
								if (stream != null)
								{
									stream.Close();
								}
							}

							Console.WriteLine("{0} ({1})", entry.Name, DateTime.Now.Subtract(start));
						}
							break;
							
						// list changes
						case 'x':
						{
							if (ifolder != null)
							{
								int total;
								ChangeEntry[] changes = web.GetChanges(ifolder.ID, null, 0, 0, out total);

								foreach(ChangeEntry change in changes)
								{
									Console.WriteLine("{0}\t{1}\t{2}\t{3}", change.Time, change.Type, change.EntryName, change.UserFullName);
								}
							}
						}
						break;

						// bad command
						default:
							Console.WriteLine("Bad Command: {0}", command);
							break;
					}
				}
				catch(SoapException se)
				{
					// soap exception
					string type = se.GetType().Name;

					try
					{
						type = se.Detail["OriginalException"].Attributes.GetNamedItem("type").Value;
						type = type.Substring(type.LastIndexOf(".") + 1);
					}
					catch { }

					Console.WriteLine("Error: {0} ({1})", se.Message, type);
				}
				catch(Exception e)
				{
					// all other exceptions
					Console.WriteLine("Error: {0} ({1})", e.Message, e.GetType().Name);
				}
			}

			Environment.Exit(0);
		}

		/// <summary>
		/// Parse the iFolder Name from the Path
		/// </summary>
		/// <param name="path">The Path String</param>
		/// <returns>The iFolder Name</returns>
		static string ParseiFolder(string path)
		{
			string result = null;

			if ((path != null) && (path.Length > 1))
			{
				int index = path.IndexOf('/', 1);

				if (index == -1)
				{
					result = path.Substring(1);
				}
				else
				{
					result = path.Substring(1, (index - 1));
				}
			}

			return  result;
		}

		/// <summary>
		/// Combine the Paths
		/// </summary>
		/// <param name="path1">The first path.</param>
		/// <param name="path2">The second path.</param>
		/// <returns>The combined path.</returns>
		static string PathCombine(string path1, string path2)
		{
			string result = null;

			if ((path2 == null) || (path2.Length == 0))
			{
				result = path1;
			}
			else if (path2.StartsWith("/"))
			{
				result = path2;
			}
			else if (path2.Equals(".."))
			{
				int index = path1.LastIndexOf('/');

				if (index < 1) index = 1;

				result = path1.Substring(0, index);
			}
			else
			{
				path1 = path1.TrimEnd(new char[] {'/'});
				result = String.Format("{0}/{1}", path1, path2);
			}

			return result;
		}

		/// <summary>
		/// Get the Parent Path
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>The parent path.</returns>
		static string GetParent(string path)
		{
			string result = null;

			if ((path != null) && (path.Length > 1) && (path.LastIndexOf('/') > 0))
			{
				result = path.Substring(0, path.LastIndexOf('/'));
			}

			return result;
		}

		/// <summary>
		/// Get the Entry Name
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>the entry name.</returns>
		static string GetEntry(string path)
		{
			string result = null;

			if ((path != null) && (path.Length > 1))
			{
				result = path.Substring(path.LastIndexOf('/') + 1);
			}

			return result;
		}
	}

	internal class TrustAllCertificatePolicy : ICertificatePolicy
	{
		#region ICertificatePolicy Members

		public bool CheckValidationResult(ServicePoint srvPoint,
			System.Security.Cryptography.X509Certificates.X509Certificate certificate,
			WebRequest request, int certificateProblem)
		{
			return true;
		}

		#endregion
	}
}