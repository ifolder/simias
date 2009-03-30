#!/bin/sh
#/*****************************************************************************
#*
#* Copyright (c) [2009] Novell, Inc.
#* All Rights Reserved.
#*
#* This program is free software; you can redistribute it and/or
#* modify it under the terms of version 2 of the GNU General Public License as
#* published by the Free Software Foundation.
#*
#* This program is distributed in the hope that it will be useful,
#* but WITHOUT ANY WARRANTY; without even the implied warranty of
#* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
#* GNU General Public License for more details.
#*
#* You should have received a copy of the GNU General Public License
#* along with this program; if not, contact Novell, Inc.
#*
#* To contact Novell about this file by physical or electronic mail,
#* you may find current contact information at www.novell.com
#*
#*-----------------------------------------------------------------------------
#*
#*                 Novell iFolder Enterprise
#*
#*-----------------------------------------------------------------------------
#*
#*                 $Author: Ramesh Sunder <sramesh@novell.com>
#*                 $Modified by: <Modifier>
#*                 $Mod Date: <Date Modified>
#*                 $Revision: 0.1
#*-----------------------------------------------------------------------------
#* This module is used to:
#*        <Description of the functionality of the file >
#*
#*
#*******************************************************************************/
if test -f install-ifolder-script.sh ;
then
	sleep 1
else
	cd /tmp/ead51d60-cd98-4d35-8c7c-b43a2ca949c8
fi
gnome-terminal -x /bin/sh ./install-ifolder-script.sh
