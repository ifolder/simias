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
 | Author: [Name] <mailID>
 |   Lawrence Pit (loz@cable.a2000.nl)
 |   Per Arneng (pt99par@student.bth.se)
 |***************************************************************************/

    Lawrence Pit (loz@cable.a2000.nl)
    Per Arneng (pt99par@student.bth.se)



using System;

namespace Simias.Mail
{
    public class MailAttachment
    {
        private string filename;
        private MailEncoding encoding;

        public MailAttachment (string filename) : 
        this (filename, MailEncoding.Base64) 
        {
        }

        public MailAttachment (string filename, MailEncoding encoding) 
        {
            this.filename = filename;
            this.encoding = encoding;
            try
            {
                System.IO.File.OpenRead (filename).Close ();
            }
            catch
            {
                throw new Exception("Cannot find file: '" + 
                        filename + "'." );
            }
        }

            // Properties
        public string Filename 
        {
            get { return filename;} 
        }

        public MailEncoding Encoding 
        {
            get { return encoding;} 
        }       

    }

} //namespace System.Web.Mail
