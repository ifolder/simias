/***********************************************************************
 *  Simias.Mail.SmtpClient.cs
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author:
 *         Per Arneng (pt99par@student.bth.se)
 * 
 ***********************************************************************/

using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Net.Sockets;

namespace Simias.Mail {


    /// represents a conntection to a smtp server
    internal class SmtpClient
    {

        private string server;
        private TcpClient tcpConnection;
        private SmtpStream smtp;
        private Encoding encoding;

    //Initialise the variables and connect
        public SmtpClient( string server )
        {

            this.server = server;
            encoding = new ASCIIEncoding( );

            Connect();
        }

    // make the actual connection
    // and HELO handshaking
        private void Connect()
        {
            tcpConnection = new TcpClient( server , 25 );

            Stream stream = tcpConnection.GetStream();
            smtp = new SmtpStream( stream );

        // read the server greeting
            smtp.ReadResponse();
            smtp.CheckForStatusCode( 220 );

        // write the HELO command to the server
            smtp.WriteHelo( Dns.GetHostName() );

        }

        public void Send( MailMessageWrapper msg )
        {

            if( msg.From == null )
            {
                throw new SmtpException( "From property must be set." );
            }

            if( msg.To == null )
            {
                if( msg.To.Count < 1 ) throw new SmtpException( "Atleast one recipient must be set." );
            }


        // start with a reset incase old data
        // is present at the server in this session
            smtp.WriteRset();

        // write the mail from command
            smtp.WriteMailFrom( msg.From.Address );

        // write the rcpt to command for the To addresses
            foreach( MailAddress addr in msg.To )
            {
                smtp.WriteRcptTo( addr.Address );
            }

        // write the rcpt to command for the Cc addresses
            foreach( MailAddress addr in msg.Cc )
            {
                smtp.WriteRcptTo( addr.Address );
            }

        // write the rcpt to command for the Bcc addresses
            foreach( MailAddress addr in msg.Bcc )
            {
                smtp.WriteRcptTo( addr.Address );
            }

        // write the data command and then
        // send the email
            smtp.WriteData();


            if( msg.Attachments.Count == 0 )
            {

                SendSinglepartMail( msg );

            }
            else
            {

                SendMultipartMail( msg );

            }

        // write the data end tag "."
            smtp.WriteDataEndTag();

        }

    // sends a single part mail to the server
        private void SendSinglepartMail( MailMessageWrapper msg )
        {

        // write the header
            smtp.WriteHeader( msg.Header );

        // send the mail body
            smtp.WriteBytes( msg.BodyEncoding.GetBytes( msg.Body ) );

        }

    // sends a multipart mail to the server
        private void SendMultipartMail( MailMessageWrapper msg )
        {

        // generate the boundary between attachments
            string boundary = MailUtil.GenerateBoundary();

        // set the Content-Type header to multipart/mixed
            msg.Header.ContentType = 
                    String.Format( "multipart/mixed;\r\n   boundary={0}" , boundary );

        // write the header
            smtp.WriteHeader( msg.Header );

        // write the first part text part
        // before the attachments
            smtp.WriteBoundary( boundary );

            MailHeader partHeader = new MailHeader();
            partHeader.ContentType = "text/plain";

            smtp.WriteHeader( partHeader );


        // FIXME: probably need to use QP or Base64 on everything higher
        // then 8-bit .. like utf-16
            smtp.WriteBytes( msg.BodyEncoding.GetBytes( msg.Body )  );

            smtp.WriteBoundary( boundary );

        // now start to write the attachments

            for( int i=0; i< msg.Attachments.Count ; i++ )
            {
                MailAttachment a = (MailAttachment)msg.Attachments[ i ];

                FileInfo fileInfo = new FileInfo( a.Filename );

                MailHeader aHeader = new MailHeader();

                aHeader.ContentType = 
                        String.Format( "application/octet-stream; name=\"{0}\"", 
                        fileInfo.Name  );

                aHeader.ContentDisposition = 
                        String.Format( "attachment; filename=\"{0}\"" , fileInfo.Name );

                aHeader.ContentTransferEncoding = a.Encoding.ToString();

                smtp.WriteHeader( aHeader );

        // perform the actual writing of the file.
        // read from the file stream and write to the tcp stream
                FileStream ins = new FileStream( fileInfo.FullName  , FileMode.Open );

        // create an apropriate encoder
                IAttachmentEncoder encoder;
                if( a.Encoding == MailEncoding.UUEncode )
                {
                    encoder = new UUAttachmentEncoder( 644 , fileInfo.Name  );
                }
                else
                {
                    encoder = new Base64AttachmentEncoder();
                }

                encoder.EncodeStream( ins , smtp.Stream );

                ins.Close();


                smtp.WriteLine( "" );

        // if it is the last attachment write
        // the final boundary otherwise write
        // a normal one.
                if( i < (msg.Attachments.Count - 1) )
                {
                    smtp.WriteBoundary( boundary );
                }
                else
                {
                    smtp.WriteFinalBoundary( boundary );
                }


            }

        }

    // send quit command and
    // closes the connection
        public void Close()
        {

            smtp.WriteQuit();
            tcpConnection.Close();

        }


    }

}
