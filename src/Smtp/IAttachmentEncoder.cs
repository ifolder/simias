/***********************************************************************
 *  Simias.Mail.IAttachmentEncoder.cs
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
 *  Author: Per Arneng <pt99par@student.bth.se>
 * 
 ***********************************************************************/

using System;
using System.IO;

namespace Simias.Mail
{
    // An interface for attachment encoders ex Base64, UUEncode
    interface IAttachmentEncoder
    {
        void EncodeStream(  Stream ins , Stream outs ); 
    }
}
