//
// System.Web.Mail.Base64AttachmentEncoder.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
//


using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Simias.Mail
{
    // a class that handles Base64 encoding for attachments
    internal class Base64AttachmentEncoder : IAttachmentEncoder
    {
        // reads bytes from a stream and writes the encoded
        // as base64 encoded characters. ( 60 chars on each row)
        public void EncodeStream(  Stream ins , Stream outs )
        {
            if( ( ins == null ) || ( outs == null ) )
                throw new ArgumentNullException(
                                               "The input and output streams may not be null.");

            ICryptoTransform base64 = new ToBase64Transform();

            // the buffers
            byte[] plainText = new byte[ base64.InputBlockSize ];
            byte[] cipherText = new byte[ base64.OutputBlockSize ];

            int readLength = 0;
            int trLength = 0;
            int count = 0;
            byte[] newln = new byte[] { 13 , 10}; //CR LF with mail

            // read through the stream until there 
            // are no more bytes left
            while( true )
            {
                // read some bytes
                readLength = ins.Read( plainText , 0 , plainText.Length );

                // break when there is no more data
                if( readLength < 1 ) break;

                // transfrom and write the blocks. If the block size
                // is less than the InputBlockSize then write the final block
                if( readLength == plainText.Length )
                {

                    trLength = base64.TransformBlock( plainText , 0 , 
                                                      plainText.Length ,
                                                      cipherText , 0 );

                    // write the data
                    outs.Write( cipherText , 0 , cipherText.Length );


                    // do this to output lines that
                    // are 60 chars long
                    count += cipherText.Length;
                    if( count == 60 )
                    {
                        outs.Write( newln , 0 , newln.Length );
                        count = 0;
                    }

                }
                else
                {
                    // convert the final blocks of bytes and write them
                    cipherText = base64.TransformFinalBlock( plainText , 
                                                             0 , readLength );
                    outs.Write( cipherText , 0 , cipherText.Length );
                }
            } 
            outs.Write( newln , 0 , newln.Length );
        }
    }   
}
