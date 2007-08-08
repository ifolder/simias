
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
using System.Text;

namespace Simias.Mail {


    // This class contains some utillity functions
    // that doesnt fit in other classes and to keep
    // high cohesion on the other classes.
    internal class MailUtil
    {

    // determines if a string needs to
    // be encoded for transfering over
    // the smtp protocol without risking
    // that it would be changed.
        public static bool NeedEncoding( string str )
        {

            foreach( char chr in str )
            {

                int ch = (int)chr;

                if( ! ( (ch > 61) && (ch < 127) || (ch>31) && (ch<61) ) )
                {

                    return true;
                }
            }

            return false;
        }

    // Encodes a string to base4
        public static string Base64Encode( string str )
        {
            return Convert.ToBase64String( Encoding.Default.GetBytes( str ) );
        }

    // Generate a unique boundary
        public static string GenerateBoundary()
        {
            StringBuilder  boundary = new StringBuilder("__MONO__Boundary");

            boundary.Append("__");

            DateTime now = DateTime.Now;
            boundary.Append(now.Year);
            boundary.Append(now.Month);
            boundary.Append(now.Day);
            boundary.Append(now.Hour);
            boundary.Append(now.Minute);
            boundary.Append(now.Second);
            boundary.Append(now.Millisecond);

            boundary.Append("__");
            boundary.Append((new Random()).Next());
            boundary.Append("__");

            return boundary.ToString();
        }

    }


}
