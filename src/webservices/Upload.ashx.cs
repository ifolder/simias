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
using System.IO;
using System.Web;
using System.Net;

using Simias.Client;
using Simias.Storage;
using Simias.Policy;
using Simias.Sync.Delta;

namespace iFolder.WebService
{
	/// <summary>
	/// File Upload Handler
	/// </summary>
	public class UploadHandler : FileHandler
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public UploadHandler() : base()
		{
		}

		#region IHttpHandler Members

		/// <summary>
		/// Process the Request
		/// </summary>
		/// <param name="context">The HttpContext object.</param>
		public override void ProcessRequest(HttpContext context)
		{
			const int BUFFERSIZE = (16 * 1024);

			try
			{
				bool DontCheckPolicies = false;
				// initialize
				Initialize(context);
				Length = int.Parse(context.Request.QueryString["Length"]);
				string dontCheckPolicies = null;
				try
				{
					dontCheckPolicies = context.Request.QueryString["DontCheckPolicies"];
					if( dontCheckPolicies != null && dontCheckPolicies == "true")
					{
						DontCheckPolicies = true;
					}
				}
				catch(Exception ex)
				{
				}

				// does member have write rights
				if ((member.Rights != Access.Rights.Admin) && (member.Rights != Access.Rights.ReadWrite))
				{
					throw new AccessException(collection, member, Access.Rights.ReadWrite);
				}

				string backupPath = null;
				long backupLength = 0;

				// new file?
				if (node == null)
				{
					filename = System.IO.Path.GetFileName(entryPath);

					Node parent = iFolderEntry.GetEntryByPath(collection,
						System.IO.Path.GetDirectoryName(entryPath).Replace('\\', '/'));

					node = (FileNode) iFolderEntry.CreateEntry(collection, parent,
						iFolderEntryType.File, filename, out filePath, DontCheckPolicies);
				}
				else
				{
					// backup file
					backupPath = String.Format("{0}.simias.temp", filePath);
					File.Copy(filePath, backupPath, true);
					backupLength = (new FileInfo(backupPath)).Length;
				}

				try
				{
					long deltaSize = context.Request.ContentLength - backupLength;
				
					if( DontCheckPolicies == false )
					{
						// check file size policy
						FileSizeFilter fsFilter = FileSizeFilter.Get(collection);
						if (!fsFilter.Allowed(deltaSize))
						{
							throw new FileSizeException(filename);
						}

						// check disk quota policy
						DiskSpaceQuota dsQuota = DiskSpaceQuota.Get(collection);
						if (!dsQuota.Allowed(deltaSize))
						{
							throw new DiskQuotaException(filename);
						}
					}

					// lock the file
					FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
			
					// reader
					Stream reader = context.Request.InputStream;

					try
					{
						byte[] buffer = new byte[BUFFERSIZE];

						int count = 0;

						// download file
						while((count = reader.Read(buffer, 0, BUFFERSIZE)) > 0)
						{
							stream.Write(buffer, 0, count);
							stream.Flush();
						}
					}
					finally
					{
						// release the file
						stream.Close();

						// release the reader
						reader.Close();
					}


					// update node
					node.UpdateWebFileInfo(collection, Length);
					//node.UpdateFileInfo(collection);
					collection.Commit(node);
					/*
						As of now the hash map from web access is being uploaded only for unencrypted files. So for encrypted files, if we are doing delta sync, then the existing hashmap should be removed immediately after the file is uploaded ( because we are not uploading the new hashmap for the file.)
					*/
					/* Uploading Hash map for unencrypted files */
                                        if( collection.EncryptionAlgorithm == null || collection.EncryptionAlgorithm == "")
                                        {
                                                // Upload hashmap for unencrypted iFolders..
						log.LogAccess("Upload hash", node.GetRelativePath(), node.ID, "Success");
                                                HashMap map = new HashMap(collection, node);
                                                map.CreateHashMapFile();
                                        }

					// log
					log.LogAccess("Upload", node.GetRelativePath(), node.ID, "Success");
				}
				catch
				{
					// restore backup
					if (backupPath != null) File.Copy(backupPath, filePath, true);

					// log
					log.LogAccess("Upload", node.GetRelativePath(), node.ID, "Failed");

					throw;
				}
				finally
				{
					// delete backup file
					if ((backupPath != null) && File.Exists(backupPath))
					{
						File.Delete(backupPath);
					}
				}
			}
			catch(Exception e)
			{
				// consume the file for better error reporting
				Stream reader = null;
				
				try
				{
					// reader
					reader = context.Request.InputStream;

					byte[] buffer = new byte[BUFFERSIZE];

					// download file
					while(reader.Read(buffer, 0, BUFFERSIZE) > 0);
				}
				catch
				{
					// ignore
				}
				finally
				{
					// release the reader
					if (reader != null) reader.Close();
				}

				// create an HTTP error
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				context.Response.StatusDescription = e.GetType().Name;
			}
		}

		#endregion
	}
}
