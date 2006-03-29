//
// System.Web.Mail.MailMessageWrapper.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
//


using System;
using System.Collections;
using System.Text;

namespace Simias.Mail {


    // wraps a MailMessage to make an easier
    // interface to work with collections of
    // addresses instead of a single string
    internal class MailMessageWrapper
    {

        private MailAddressCollection bcc = new MailAddressCollection();
        private MailAddressCollection cc = new MailAddressCollection();     
        private MailAddress from;
        private MailAddressCollection to = new MailAddressCollection();
        private MailHeader header = new MailHeader();
        private MailMessage message;
        private string body;

    // Constructor		
        public MailMessageWrapper( MailMessage message )
        {
            this.message = message;

            if( message.From != null )
            {
                from = MailAddress.Parse( message.From );
                header.From = from.ToString();
            }

            if( message.To != null )
            {
                to = MailAddressCollection.Parse( message.To );
                header.To = to.ToString();
            }

            if( message.Cc != null )
            {
                cc = MailAddressCollection.Parse( message.Cc );
                header.Cc = cc.ToString();
            }

            if( message.Bcc != null )
            {
                bcc = MailAddressCollection.Parse( message.Bcc );
                header.Bcc = bcc.ToString();
            }


        // set the subject
            if( message.Subject != null )
            {

        // encode the subject if it needs encoding
                if( MailUtil.NeedEncoding( message.Subject ) )
                {

                    byte[] subjectBytes = message.BodyEncoding.GetBytes( message.Subject );
            // encode the subject with Base64
                    header.Subject = String.Format( "=?{0}?B?{1}?=" , 
                            message.BodyEncoding.BodyName ,
                            Convert.ToBase64String( subjectBytes ) );
                }
                else
                {

                    header.Subject = message.Subject;

                }
            }

        // convert single '.' on a line with ".." to not
        // confuse the smtp server since the DATA command
        // is terminated with a '.' on a single line.
        // this is also according to the smtp specs.
            if( message.Body != null )
            {
                body = message.Body.Replace( "\n.\n" , "\n..\n" );
                body = body.Replace( "\r\n.\r\n" , "\r\n..\r\n" );
            }


        // set the Contet-Base header
            if( message.UrlContentBase != null )
                header.ContentBase = message.UrlContentBase;

        // set the Contet-Location header
            if( message.UrlContentLocation != null )
                header.ContentLocation = message.UrlContentLocation;


        // set the content type
            switch( message.BodyFormat )
            {
                
                case MailFormat.Html: 
                    header.ContentType = 
                            String.Format( "text/html; charset=\"{0}\"" , message.BodyEncoding.BodyName ); 
                    break;

                case MailFormat.Text: 
                    header.ContentType = 
                            String.Format( "text/plain; charset=\"{0}\"" , message.BodyEncoding.BodyName );
                    break;

                default: 
                    header.ContentType = 
                            String.Format( "text/html; charset=\"{0}\"" , message.BodyEncoding.BodyName );
                    break;
            }


        // set the priority as in the same way as .NET sdk does
            switch( message.Priority )
            {
                
                case MailPriority.High: 
                    header.Importance = "high";
                    break;

                case MailPriority.Low: 
                    header.Importance = "low";
                    break;

                case MailPriority.Normal: 
                    header.Importance = "normal";
                    break;

                default: 
                    header.Importance = "normal";
                    break;

            }

        // .NET sdk allways sets this to normal
            header.Priority = "normal";


        // Set the mime version
            header.MimeVersion = "1.0";

        // Set the transfer encoding
            if( message.BodyEncoding is ASCIIEncoding )
            {
                header.ContentTransferEncoding = "7bit";
            }
            else
            {
                header.ContentTransferEncoding = "8bit";
            }


        // Add the custom headers
            foreach( string key in message.Headers.Keys )
            header.Data[ key ] = (string)this.message.Headers[ key ];
        }       

    // Properties
        public IList Attachments {
            get { return message.Attachments;}
        }       

        public MailAddressCollection Bcc {
            get { return bcc;} 
        }

        public string Body {
            get { return body;} 
            set { body = value;} 
        }

        public Encoding BodyEncoding {
            get { return message.BodyEncoding;} 
            set { message.BodyEncoding = value;}
        }

        public MailFormat BodyFormat {
            get { return message.BodyFormat;} 
            set { message.BodyFormat = value;}
        }       

        public MailAddressCollection Cc {
            get { return cc;} 
        }

        public MailAddress From {
            get { return from;} 
        }

        public MailHeader Header {
            get { return header;}
        }

        public MailPriority Priority {
            get { return message.Priority;} 
            set { message.Priority = value;}
        }

        public string Subject {
            get { return message.Subject;} 
            set { message.Subject = value;}
        }

        public MailAddressCollection To {
            get { return to;}   
        }

        public string UrlContentBase {
            get { return message.UrlContentBase;} 

        }

        public string UrlContentLocation {
            get { return message.UrlContentLocation;} 
        }
    }

}
