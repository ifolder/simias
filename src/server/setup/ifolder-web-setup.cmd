@echo off

rem /*****************************************************************************
rem *
rem * Copyright (c) [2009] Novell, Inc.
rem * All Rights Reserved.
rem *
rem * This program is free software; you can redistribute it and/or
rem * modify it under the terms of version 2 of the GNU General Public License as
rem * published by the Free Software Foundation.
rem *
rem * This program is distributed in the hope that it will be useful,
rem * but WITHOUT ANY WARRANTY; without even the implied warranty of
rem * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
rem * GNU General Public License for more details.
rem *
rem * You should have received a copy of the GNU General Public License
rem * along with this program; if not, contact Novell, Inc.
rem *
rem * To contact Novell about this file by physical or electronic mail,
rem * you may find current contact information at www.novell.com
rem *
rem *-----------------------------------------------------------------------------
rem *
rem *                 Novell iFolder Enterprise
rem *
rem *-----------------------------------------------------------------------------
rem *
rem *                 $Author:  Kalidas Balakrishnan <bkalidas@novell.com>
rem *                 $Modified by: <Modifier>
rem *                 $Mod Date: <Date Modified>
rem *                 $Modified by: <Modifier>
rem *                 $Mod Date: <Date Modified>
rem *                 $Revision: 0.1
rem *-----------------------------------------------------------------------------
rem * This module is used to:
rem *        <Description of the functionality of the file >
rem *
rem *
rem *******************************************************************************/

cd %0%\..

iFolderWebSetup.exe %*
