#!/bin/sh
#######################################################################
#
#  $RCSfile: install-ifolder-script.sh,v $
#
#  Copyright (C) 2007 Novell, Inc.
#
#  Author: Ramesh Sunder <sramesh@novell.com>
#
#######################################################################

tar -zxvf ./ifolder3-linux.tar.gz
cd ifolder3-linux
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
echo "quitting iFolder"
if [ pgrep simias | wc -l != 0 ];
then
	echo "Exiting simias"
	sudo pkill simias
fi
echo "Installing ifolder package"
sleep 5
sudo rpm -Uvh simias-1.6.* ifolder3-3.6.* nautilus-ifolder3-3.6*
sleep 5
echo "Successfully installed iFolder. Press any key to exit."
read -m 1
exit
