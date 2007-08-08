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
 | Author:
 |   Lawrence Pit (loz@cable.a2000.nl)
 |   Per Arneng (pt99par@student.bth.se)
 |***************************************************************************/ 



using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Simias.Mail {

    /// <remarks>
    /// </remarks>
    public class MailMessage
    {
        private ArrayList attachments;
        private string bcc;
        private string body;
        private Encoding bodyEncoding;
        private MailFormat bodyFormat;
        private string cc;      
        private string from;
        private ListDictionary headers;
        private MailPriority priority;
        private string subject;
        private string to;
        private string urlContentBase;
        private string urlContentLocation;

        // Constructor		
        public MailMessage ()
        {
            attachments = new ArrayList (8);
            headers = new ListDictionary ();
            bodyEncoding = Encoding.Default;
        }       

        // Properties
        public IList Attachments {
            get { return(IList) attachments;}
        }       

        public string Bcc {
            get { return bcc;} 
            set { bcc = value;}
        }

        public string Body {
            get { return body;} 
            set { body = value;}
        }

        public Encoding BodyEncoding {
            get { return bodyEncoding;} 
            set { bodyEncoding = value;}
        }

        public MailFormat BodyFormat {
            get { return bodyFormat;} 
            set { bodyFormat = value;}
        }       

        public string Cc {
            get { return cc;} 
            set { cc = value;}
        }

        public string From {
            get { return from;} 
            set { from = value;}
        }

        public IDictionary Headers {
            get { return(IDictionary) headers;}
        }

        public MailPriority Priority {
            get { return priority;} 
            set { priority = value;}
        }

        public string Subject {
            get { return subject;} 
            set { subject = value;}
        }

        public string To {
            get { return to;}   
            set { to = value;}
        }

        public string UrlContentBase {
            get { return urlContentBase;} 
            set { urlContentBase = value;}
        }

        public string UrlContentLocation {
            get { return urlContentLocation;} 
            set { urlContentLocation = value;}
        }

#if NET_1_1
        [MonoTODO]
                public IDictionary Fields {
            get { throw new NotImplementedException ();}
        }
#endif
    }

} //namespace System.Web.Mail
