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
#*                 $Author: Mahabaleshwar M A (amahabaleshwar@novell.com)
#*                 $Modified by: <Modifier>
#*                 $Mod Date: <Date Modified>
#*                 $Revision: 0.1
#*-----------------------------------------------------------------------------
#* This module is used to:
#*        <iFolder  mod-mono-server2 wrapper script >
#*
#*
#*******************************************************************************/

export MONO_PATH=/opt/novell/ifolder3/bin/../mono/lib/mono/2.0/:/opt/novell/ifolder3/bin/../lib64:/opt/novell/ifolder3/bin/../lib64/simias/web/bin:/opt/novell/ifolder3/bin/../lib64/simias/admin/bin:/opt/novell/ifolder3/bin/../lib64/simias/webaccess/bin
export JAVA_HOME=/home/maha/hadoop/Hadoop21/jdk1.6.0_18/

export CLASSPATH=/home/maha/hadoop/Hadoop21/hadoop-0.20.1/:/home/maha/hadoop/Hadoop21/hadoop-0.20.1/lib:/home/maha/hadoop/Hadoop21/hadoop-0.20.1/conf/:/home/maha/hadoop/Hadoop21/hadoop-0.20.1/hadoop-0.20.1-core.jar:/home/maha/hadoop/Hadoop21/hadoop-0.20.1/lib/commons-logging-1.0.4.jar:/home/maha/hadoop/Hadoop21/hadoop-0.20.1/lib/commons-logging-api-1.0.4.jar


source /opt/novell/ifolder3/bin/../mono/bin/novell-ifolder-mono-environment.sh

export MONO_CFG_DIR=/opt/novell/ifolder3/bin/../mono/etc

gmcs DeleteMissingNodes.cs /r:System.dll /r:System.Data.dll /r:System.Xml.dll /r:System.Web.dll /r:System.Web.Services.dll /r:SimiasLib.dll /r:SimiasClient.dll /r:Simias.Server.dll

