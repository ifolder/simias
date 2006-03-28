/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
using System.Web;

namespace Novell.iFolderApp.Web
{
	/// <summary>
	/// HTTP Upload Stream
	/// </summary>
	public class UploadStream : Stream
	{
		private readonly HttpWorkerRequest worker;
		private readonly long length;
		private long position;
		private byte[] preloadedEntityBody;
		private byte[] readBuffer;

		/// <summary>
		/// Constructor
		/// </summary>
		public UploadStream(HttpWorkerRequest worker)
		{
			this.worker = worker;
			this.length = long.Parse(worker.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength));
			this.position = 0;

			// preloaded
			preloadedEntityBody = worker.GetPreloadedEntityBody();
			if (preloadedEntityBody == null)
			{
				preloadedEntityBody = new byte[0];
			}
		}
		
		#region Stream Properties

		/// <summary>
		/// Can Read
		/// </summary>
		public override bool CanRead
		{
			get { return true; }
		}

		/// <summary>
		/// Can Seek
		/// </summary>
		public override bool CanSeek
		{
			get { return false; }
		}

		/// <summary>
		/// Can Write
		/// </summary>
		public override bool CanWrite
		{
			get { return false; }
		}

		/// <summary>
		/// Length
		/// </summary>
		public override long Length
		{
			get { return length; }
		}

		/// <summary>
		/// Position
		/// </summary>
		public override long Position
		{
			get { return position; }
			set { throw new NotSupportedException(); }
		}

		#endregion

		#region Stream Members
		
		/// <summary>
		/// Flush
		/// </summary>
		public override void Flush()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Read
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			int size = 0;

			if (position < preloadedEntityBody.Length)
			{
				size = ReadPreloadedEntityBody(buffer, offset, count);

				if ((size < count) && worker.IsClientConnected()
					&& !worker.IsEntireEntityBodyIsPreloaded())
				{
					size += ReadEntityBody(buffer, size, (count - size));
				}
			}
			else
			{
				size = ReadEntityBody(buffer, offset, count);
			}

			position += size;

			return size;
		}

		private int ReadPreloadedEntityBody(byte[] buffer, int offset, int count)
		{
			long size = (position + count < preloadedEntityBody.Length) ? count
				: (preloadedEntityBody.Length - position);

			Buffer.BlockCopy(preloadedEntityBody, (int)position, buffer, offset, (int)size);

			return (int) size;
		}

		private int ReadEntityBody(byte[] buffer, int offset, int count)
		{
			long size = 0;

			if ((position + count) > length)
			{
				Console.WriteLine("test");
			}

			count = ((position + count) > length) ? (int)(length - position) : count;

			if (offset > 0)
			{
				if ((readBuffer == null) || (readBuffer.Length < count))
				{
					readBuffer = new byte[count];
				}

				size = worker.ReadEntityBody(readBuffer, count);
				Buffer.BlockCopy(readBuffer, 0, buffer, offset, (int)size);
			}
			else
			{
				size = worker.ReadEntityBody(buffer, count);
			}
			
			return (int) size;
		}

		/// <summary>
		/// Seek
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="origin"></param>
		/// <returns></returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Set Length
		/// </summary>
		/// <param name="value"></param>
		public override void SetLength(long value)
		{
			throw new NotSupportedException();	
		}

		/// <summary>
		/// Write
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();	
		}

		#endregion
	}
}
