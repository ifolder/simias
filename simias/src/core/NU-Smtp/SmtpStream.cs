
/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 | Author: Per Arneng <pt99par@student.bth.se>
 |***************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Security.Cryptography;

namespace Simias.Mail {


    internal class SmtpStream
    {

        protected Stream stream;
        protected Encoding encoding;
        protected SmtpResponse lastResponse;
        protected string command = "";

        public SmtpStream( Stream stream )
        {
            this.stream = stream;
            encoding = new ASCIIEncoding();
        }

        public Stream Stream {
            get { return stream;}
        }

        public SmtpResponse LastResponse {
            get { return lastResponse;}
        }

        public void WriteRset()
        {
            command = "RSET";
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 250 );

        }

        public void WriteHelo( string hostName )
        {
            command = "HELO " + hostName;
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 250 );

        }

        public void WriteMailFrom( string from )
        {
            command = "MAIL FROM: <" + from + ">";
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 250 );

        }

        public void WriteRcptTo( string to )
        {
            command = "RCPT TO: <" + to + ">";  
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 250 );

        }


        public void WriteData()
        {
            command = "DATA";
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 354 );

        }

        public void WriteQuit()
        {
            command = "QUIT";
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 221 );

        }

        public void WriteBoundary( string boundary )
        {

            WriteLine( "\r\n--{0}" , boundary );

        }

        public void WriteFinalBoundary( string boundary )
        {

            WriteLine( "\r\n--{0}--" , boundary );

        }

    // single dot by itself
        public void WriteDataEndTag()
        {
            command = "\r\n.";
            WriteLine( command );
            ReadResponse();
            CheckForStatusCode( 250 );

        }


        public void WriteHeader( MailHeader header )
        {
        // write the headers
            foreach( string key in header.Data.AllKeys )
            WriteLine( "{0}: {1}" , key , header.Data[ key ] );

        // write the header end tag
            WriteLine( "" );
        }

        public void CheckForStatusCode( int statusCode )
        {

            if( LastResponse.StatusCode != statusCode )
            {

                string msg = "" + 
                        "Server reponse: '" + lastResponse.RawResponse + "';" +
                        "Status code: '" +  lastResponse.StatusCode + "';" + 
                        "Expected status code: '" + statusCode + "';" + 
                        "Last command: '" + command + "'";

                throw new SmtpException( msg ); 

            }
        }


    // write buffer's bytes to the stream
        public void WriteBytes( byte[] buffer )
        {
            stream.Write( buffer , 0 , buffer.Length );
        }

    // writes a formatted line to the server
        public void WriteLine( string format ,  params object[] args )
        {
            WriteLine( String.Format( format , args ) );
        }

    // writes a line to the server
        public void WriteLine( string line )
        {
            byte[] buffer = encoding.GetBytes( line + "\r\n" );

            stream.Write( buffer , 0 , buffer.Length );

#if DEBUG 
            DebugPrint( line );
#endif
        }

    // read a line from the server
        public void ReadResponse( )
        {
            string line = null;

            byte[] buffer = new byte[ 4096 ];

            int readLength = stream.Read( buffer , 0 , buffer.Length );

            if( readLength > 0 )
            {

                line = encoding.GetString( buffer , 0 , readLength );

                line = line.TrimEnd( new Char[]{ '\r' , '\n' , ' '} );

            }

        // parse the line to the lastResponse object
            lastResponse = SmtpResponse.Parse( line );

#if DEBUG
            DebugPrint( line );
#endif
        }

		ISimiasLog	logger = SimiasLogManager.GetLogger(typeof(SmtpStream));
    /// debug printing 
        private void DebugPrint( string line )
        {
            logger.Debug(line);
        }

    }


}
