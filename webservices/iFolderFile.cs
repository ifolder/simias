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
using System.Collections;

using Simias;
using Simias.Client;
using Simias.Storage;
using Simias.Policy;

namespace iFolder.WebService
{
	/// <summary>
	/// An iFolder File
	/// </summary>
	[Serializable]
	public class iFolderFile
	{
		/// <summary>
		/// The File ID
		/// </summary>
		private string id;

		/// <summary>
		/// The File Path
		/// </summary>
		private string path;

		/// <summary>
		/// The File Stream
		/// </summary>
		private FileStream stream;

		/// <summary>
		/// The Collection Object
		/// </summary>
		private Collection collection;

		/// <summary>
		/// The FileNode Object
		/// </summary>
		private FileNode node;

		/// <summary>
		/// Is the File Changing?
		/// </summary>
		private bool updating;

		/// <summary>
		/// The New File Length
		/// </summary>
		private long length;

		/// <summary>
		/// The Access User ID
		/// </summary>
		private string accessID;

		/// <summary>
		/// The Backup File Path
		/// </summary>
		private string backupPath;

		/// <summary>
		/// Access Log
		/// </summary>
		protected SimiasAccessLogger log;

		/// <summary>
		/// Member
		/// </summary>
		private Member member;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ifolderID">The ID of the iFolder.</param>
		/// <param name="entryID">The ID of the Entry.</param>
		/// <param name="accessID">The Access ID.</param>
		public iFolderFile(string ifolderID, string entryID, string accessID)
		{
			Store store = Store.GetStore();

			collection = store.GetCollectionByID(ifolderID);

			if (collection == null)
			{
				throw new iFolderDoesNotExistException(ifolderID);
			}
			
			// impersonate
			this.accessID = accessID;
			iFolder.Impersonate(collection, accessID);
			
			// member
			member = collection.GetMemberByID(accessID);

			// does member exist?
			if (member == null)
			{
				throw new MemberDoesNotExistException(accessID);
			}
			
			// node
			Node n = collection.GetNodeByID(entryID);

			// does the node exist?
			if (n == null)
			{
				throw new EntryDoesNotExistException(entryID);
			}

			// is the node a file
			if (!n.IsBaseType(NodeTypes.FileNodeType))
			{
				throw new FileDoesNotExistException(entryID);
			}

			// log
			log = new SimiasAccessLogger(member.Name, collection.ID);

			// node
			node = (FileNode)n;
			id = String.Format("{0}:{1}", collection.ID, n.ID);
			path = node.GetFullPath(collection);
			updating = false;
		}

		/// <summary>
		/// The File ID.
		/// </summary>
		public string ID
		{
			get { return id; }
		}

		/// <summary>
		/// Open the File for Reading.
		/// </summary>
		public void OpenRead()
		{
			try
			{
				stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);

				// log
				log.LogAccess("OpenRead", node.GetRelativePath(), node.ID, "Success");
			}
			catch
			{
				// log
				log.LogAccess("OpenRead", node.GetRelativePath(), node.ID, "Failed");

				Close();

                throw;
			}
		}

		/// <summary>
		/// Open the File for Writing.
		/// </summary>
		/// <param name="length">New file length.</param>
		public void OpenWrite(long length)
		{
			this.length = length;

			try
			{
				// check access
				member = collection.GetMemberByID(accessID);

				// does the member exist?
				if (member == null)
				{
					throw new MemberDoesNotExistException(accessID);
				}
				
				// does the member have wright rights
				if ((member.Rights != Access.Rights.Admin) && (member.Rights != Access.Rights.ReadWrite))
				{
					throw new AccessException(collection, member, Access.Rights.ReadWrite);
				}

				// backup file
				backupPath = String.Format("{0}.simias.temp", path);
				File.Copy(path, backupPath, true);
				long deltaSize = length - (new FileInfo(backupPath)).Length;

				// open file
				stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
				updating = true;

				// check file size policy
				FileSizeFilter fsFilter = FileSizeFilter.Get(collection);
				if (!fsFilter.Allowed(deltaSize))
				{
					throw new FileSizeException(node.Name);
				}

				// check disk quota policy
				DiskSpaceQuota dsQuota = DiskSpaceQuota.Get(collection);
				if (!dsQuota.Allowed(deltaSize))
				{
					throw new DiskQuotaException(node.Name);
				}

				// log
				log.LogAccess("OpenWrite", node.GetRelativePath(), node.ID, "Success");
			}
			catch
			{
				// log
				log.LogAccess("OpenWrite", node.GetRelativePath(), node.ID, "Failed");

                Close(true);

                throw;
			}
		}

		/// <summary>
		/// Read from the File.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public byte[] Read(int size)
		{
			byte[] buffer = null;

			long togo = stream.Length - stream.Position;

			// pre-size array
			if (togo < size)
			{
				size = (int)togo;
			}

			// check size
			if (size > 0)
			{
				buffer = new byte[size];

				int count = stream.Read(buffer, 0, size);
			
				// correct array
				if (count < 1)
				{
					// done
					buffer = null;
					Close();
				}
				else if (size != count)
				{
					byte[] temp = new byte[count];
					Array.Copy(buffer, temp, count);
					buffer = temp;
				}
			}

			return buffer;
		}

		/// <summary>
		/// Write to the File.
		/// </summary>
		/// <param name="buffer"></param>
		public void Write(byte[] buffer)
		{
			try
			{
				if ((stream.Position + buffer.Length) > length)
				{
					throw new FileSizeException(node.Name);
				}

				stream.Write(buffer, 0, buffer.Length);
			}
			catch
			{
                Close(true);

                throw;
			}
		}

		/// <summary>
		/// Close the File.
		/// </summary>
		public void Close()
		{
			Close(false);
		}
		
		/// <summary>
		/// Close the File.
		/// </summary>
		public void Close(bool failed)
		{
			if (node != null)
			{
				try
				{
					// stream
					if (stream != null)
					{
						stream.Close();
						stream = null;
					}

					// log
					log.LogAccess("Closed", node.GetRelativePath(), node.ID, "Success");
    
					if (updating && !failed)
					{
						try
						{
							node.UpdateFileInfo(collection);
							collection.Commit(node);
						}
						catch
						{
							failed = true;
							throw;
						}
					}
				}
				finally
				{
					node = null;

					// check backup file
					if (File.Exists(backupPath))
					{
						if (failed)
						{
							// restore backup file
							File.Copy(backupPath, path, true);
						}

						// delete backup file
						File.Delete(backupPath);
					}
				}
			}
		}
	}
}
