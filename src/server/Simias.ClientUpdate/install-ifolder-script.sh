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

tar -zxvf ./ifolder3-*.tar.gz
cd ifolder3-*
if [ "$(uname -i)" != 'x86_64' ] ;
then
	cd i586
	echo "i586"
else 
	cd x86_64
	echo "64 bit hardware"
fi
echo "Quitting the current running client."
ps -ef | grep iFolderClient| awk -F' ' '{print "kill -9 "$2""}' | sudo sh
echo "quitted iFolder"
if [ "pgrep simias | wc -l != 0" ];
then
	echo "Exiting simias"
	sudo pkill simias
fi
echo "Installing ifolder package"
sleep 5
mkdir -p $HOME/.local/share/simias
if test -d $HOME/.local/share/simias
then
echo "Starting the installation. The error logs are redirected to $HOME/.local/share/simias/upgrade.log"
sudo rpm -Uvh simias-1.*.rpm ifolder3-3.*.rpm nautilus-ifolder3-3.*.rpm novell-ifolder-client-plugins-3.*.rpm 2> $HOME/.local/share/simias/upgrade.log
abc=$?
else
echo "The directory $HOME/.local/share/simias does not exist. The error logs could not be redirected.";
sudo rpm -Uvh simias-1.*.rpm ifolder3-3.*.rpm nautilus-ifolder3-3.*.rpm novell-ifolder-client-plugins-3.*.rpm
abc=$?
fi
if [[ $abc = 0 ]]
then
echo "Finishing installation."
else
echo "Installation failed because of some errors. Go through $HOME/.local/share/simias/upgrade.log for error details."
if test -f $HOME/.local/share/simias/upgrade.log
then
cat $HOME/.local/share/simias/upgrade.log
grep "dependencies" $HOME/.local/share/simias/upgrade.log 
if [[ $? = 0 ]]
then 
echo "Verify that mono-core, mono-data, mono-web, log4net, xsp, gconf-sharp2, gnome-sharp2, and gtk-sharp2 are installed on the system before installing iFolder."
fi
fi
sleep 5;
exit 0;
fi
sleep 5
echo "Successfully installed iFolder."
echo "Restart the system before you start using iFolder"
echo "Restart the system before you start using iFolder" >> $HOME/.local/share/simias/upgrade.log
exit
