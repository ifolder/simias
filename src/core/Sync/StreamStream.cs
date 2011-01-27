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
*                 $Author: Russ Young
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Threading;
using Simias.Encryption;

namespace Simias.Sync
{
	/// <summary>
	/// Class to copy data from one stream to another.
	/// </summary>
	public class StreamStream : Stream 
	{
		Stream					stream;
		Stream					wStream;
		static Queue			Buffer = Queue.Synchronized(new Queue(2));
		static int				buffSize = 1024 * 64;
		AutoResetEvent			writeComplete = new AutoResetEvent(true);
		Exception				exception;

		//private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(StreamStream));

			
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stream">The Stream to construct the object from.</param>
		public StreamStream(Stream stream)
		{
			this.stream = stream;
		}

		/// <summary>
		/// Get a buffer to use.
		/// </summary>
		/// <returns>The next available buffer.</returns>
		byte[] GetBuffer()
		{
            lock (Buffer.SyncRoot)
			{
				if (Buffer.Count == 0)
					return new byte[buffSize];
				else
					return (byte[])Buffer.Dequeue();
			}
		}

		/// <summary>
		/// Return the buffer, so that it can be reused.
		/// </summary>
		/// <param name="buffer">The buffer to free.</param>
		void FreeBuffer(byte[] buffer)
		{
			lock (Buffer.SyncRoot)
			{
				Buffer.Enqueue(buffer);
			}
		}

		/// <summary>
		/// Returns true if the stream support reads.
		/// </summary>
		public override bool CanRead
		{
			get {return stream.CanRead;}
		}

		/// <summary>
		/// Returns true is the stream supports seeks.
		/// </summary>
		public override bool CanSeek
		{
			get {return stream.CanSeek;}
		}

		/// <summary>
		/// Returns true if the stream supports writes.
		/// </summary>
		public override bool CanWrite
		{
			get {return stream.CanWrite;}
		}

		/// <summary>
		/// Returns the length of the stream.
		/// </summary>
		public override long Length
		{
			get {return stream.Length;}
		}

		/// <summary>
		/// Gets or Sets the posistion of the stream.
		/// </summary>
		public override long Position
		{
			get {return stream.Position;}
			set {stream.Position = value;}
		}

		/// <summary>
		/// Get the stream
		/// </summary>
		public Stream fStream
		{
			get {return stream;}
		}

		/// <summary>
		/// Flushes the stream.
		/// </summary>
		public override void Flush()
		{
			stream.Flush();
		}

		/// <summary>
		/// Read data from the stream.
		/// </summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="offset">The offset in the buffer to begin storing data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns></returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			return stream.Read(buffer, offset, count);
		}

		/// <summary>
		/// Write data to the stream.
		/// </summary>
		/// <param name="buffer">The buffer containing the data to write.</param>
		/// <param name="offset">The offset in the buffer where the data begins.</param>
		/// <param name="count">The number of bytes to write.</param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			stream.Write(buffer, offset, count);
		}

		/// <summary>
		/// Move the current stream posistion to the specified location.
		/// </summary>
		/// <param name="offset">The offset from the origin to seek.</param>
		/// <param name="origin">The origin to seek from.</param>
		/// <returns>The new position.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		/// <summary>
		/// Set the stream length.
		/// </summary>
		/// <param name="value">The length to set.</param>
		public override void SetLength(long value)
		{
			stream.SetLength(value);
		}

		/// <summary>
		/// The write has completed.
		/// </summary>
		/// <param name="result">The result of the async write.</param>
		private void Read_WriteComplete(IAsyncResult result)
		{
			byte[] buffer = (byte[])result.AsyncState;
			try
			{
				wStream.EndWrite(result);
			}
			catch (Exception ex)
			{
				Log.log.Debug(ex, "Read_WriteComplete");
				exception = ex;
			}
			finally
			{
				FreeBuffer(buffer);
				writeComplete.Set();
			}
		}

		/// <summary>
		/// Read into the supplied stream.
		/// </summary>
		/// <param name="outStream">The stream to recieve the data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns>The number of bytes read.</returns>
		public int Read(Stream outStream, int count)
		{
//			wStream = outStream;
			int bytesLeft = count;
			while(bytesLeft > 0)
			{
				byte[] buffer = GetBuffer();
				// Default buffer size of socket is 32 Kb,
				// if continuously 64 KB each write data is pushed to out stream
				// and if the reader is slower than the writer then connection loss and
				// "Operation on non-blocking socket would block" error messages are thrown.
				// For more info on why it is getting reset refer 506930.
				int MaxWriteBufferSize = Math.Min((8 * 1024), buffer.Length);
				int bytesRead = stream.Read(buffer, 0, Math.Min(bytesLeft, MaxWriteBufferSize));
				if (bytesRead != 0)
				{
					writeComplete.WaitOne();
					wStream = outStream;
					if (exception != null)
						throw exception;
					wStream.BeginWrite(buffer, 0, bytesRead, new AsyncCallback(Read_WriteComplete), buffer);
					bytesLeft -= bytesRead;
				}
				else break;
			}
			writeComplete.WaitOne();
			writeComplete.Set();
			wStream = null;
			return count - bytesLeft;
		}

		/// <summary>
		/// Read into the supplied stream.
		/// </summary>
		/// <param name="outStream">The stream to recieve the data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <param name="encryptionAlgorithm">algorithm used for encrypting data.</param>
		/// <param name="EncryptionKey">The key string to encrypt the data.</param>
		/// <returns>The number of bytes read.</returns>
		public int Read(Stream outStream, int count, string encryptionAlgorithm, string EncryptionKey)
		{
			wStream = outStream;
			int paddingLength = 0, reminder = 0;
			int bytesLeft = count;
			

			while(bytesLeft > 0)
			{
				byte[] buffer = GetBuffer();
				int bytesRead = stream.Read(buffer, 0, Math.Min(bytesLeft, buffer.Length));
				if (bytesRead != 0)
				{
					//Blowfish is the default algorithm
					if(encryptionAlgorithm != "BlowFish")
						throw exception;
					
//					UTF8Encoding utf8 = new UTF8Encoding();
					Blowfish bf = new Blowfish(Convert.FromBase64String(EncryptionKey));
					
					reminder = (int) bytesRead % 8;
					if (reminder != 0)
					{
						paddingLength = 0;
						paddingLength = 8 - reminder;
					}
				
					if (paddingLength > 0) {
						// add padding
						for (int i=0; i< paddingLength; i++) 
							buffer [bytesRead+i] = 0; //TODO: use some other signature
					}
					int totalRead = bytesRead + paddingLength;
					bf.Encipher (buffer, totalRead);

					writeComplete.WaitOne();
					if (exception != null)
						throw exception;
					outStream.BeginWrite(buffer, 0, totalRead, new AsyncCallback(Read_WriteComplete), buffer);
					bytesLeft -= bytesRead;
				}
				else break;
			}
			writeComplete.WaitOne();
			writeComplete.Set();
			wStream = null;
			return count - bytesLeft + paddingLength;
		}


		/// <summary>
		/// The async write has completed.
		/// </summary>
		/// <param name="result">The results of the async write.</param>
		private void Write_WriteComplete(IAsyncResult result)
		{
			byte[] buffer = (byte[])result.AsyncState;
			try
			{
				stream.EndWrite(result);
			}
			catch (Exception ex)
			{
				Log.log.Debug(ex, "Write_WriteComplete");
				exception = ex;
			}
			finally
			{
				FreeBuffer(buffer);
				writeComplete.Set();
			}
		}
		
		/// <summary>
		/// Write the data from the supplied stream to this stream.
		/// </summary>
		/// <param name="inStream">The data to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(Stream inStream, long count)
		{
			long bytesLeft = count;
			while(bytesLeft > 0)
			{
				byte[] buffer = GetBuffer();
                		int bytesRead = inStream.Read(buffer, 0, (int)Math.Min(buffer.Length, bytesLeft));
				if (bytesRead != 0)
				{
					writeComplete.WaitOne();
					if (exception != null)
						throw exception;
					stream.BeginWrite(buffer, 0, bytesRead, new AsyncCallback(Write_WriteComplete), buffer);
					bytesLeft -= bytesRead;
				}
				else break;
			}
			writeComplete.WaitOne();
			writeComplete.Set();
		}
		
		/// <summary>
		/// Write the data from the supplied stream to this stream.
		/// </summary>
		/// <param name="inStream">The data to write.</param>
		/// <param name="count">The number of bytes to write.</param>	
		/// <param name="actualCount">The number of bytes to write.</param>
		/// <param name="encryptionAlgorithm">Key to encrypt the data with.</param>
	        /// <param name="EncryptionKey"></param>
		public void Write(Stream inStream, int count, int actualCount, string encryptionAlgorithm, string EncryptionKey)
		{
			int bytesLeft = count;
			int bytesToWrite;
			while(bytesLeft > 0)
			{
				byte[] buffer = GetBuffer();
                		int bytesRead = 0;
				int bytesToRead = Math.Min(buffer.Length, bytesLeft);
				
				while(bytesToRead !=0)		
				{
					int currentRead = inStream.Read(buffer, bytesRead, bytesToRead);
					if(currentRead==0)
						break;
					bytesRead +=currentRead;
					bytesToRead -=currentRead;				
				}
                		
				if (bytesRead != 0)
				{
					if(encryptionAlgorithm != "BlowFish")
						throw exception;
					
//					UTF8Encoding utf8 = new UTF8Encoding();
					Blowfish bf = new Blowfish(Convert.FromBase64String(EncryptionKey));
					bf.Decipher (buffer, buffer.Length);
					
					//Discard the bytes padded
					if((bytesLeft - bytesRead == 0) && (actualCount != count))
						bytesToWrite = bytesRead -(count-actualCount);
					else					
						bytesToWrite = bytesRead;

					writeComplete.WaitOne();
					if (exception != null)
						throw exception;
					stream.BeginWrite(buffer, 0, bytesToWrite, new AsyncCallback(Write_WriteComplete), buffer);
					bytesLeft -= bytesRead;
				}
				else break;
			}

			writeComplete.WaitOne();
			writeComplete.Set();
		}

		/// <summary>
		/// Close the stream
		/// </summary>
		public override void Close()
		{
			stream.Close();
		}
	}
}
