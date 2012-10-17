#!/bin/sh
OS_ARCH=`uname -m | grep -c x86_64`
if [ $OS_ARCH -gt 0 ]
then
        export OS_ARCH=`uname -m`
fi


rpm -q novell-ifolder-mono > /dev/null 2>&1
if [ $? -gt 0 ]
then

        export MONO_PATH=/opt/novell/ifolder3/lib64/simias/web/bin:/opt/novell/ifolder3/bin
        export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/opt/novell/ifolder3/lib64/simias/web/bin
        export DYLD_LIBRARY_PATH=$DYLD_LIBRARY_PATH:/opt/novell/ifolder3/lib64/simias/web/bin
        cd /opt/novell/ifolder3/bin

        MONO_CMD=mono

else
        MONO_RUNTIME_PATH=/opt/novell/ifolder3/bin/../mono
        export MONO_PATH=$MONO_RUNTIME_PATH/lib/mono/:$MONO_RUNTIME_PATH/lib/mono/2.0:/opt/novell/ifolder3/lib64/simias/web/bin:/opt/novell/ifolder3/bin
        source $MONO_RUNTIME_PATH/bin/novell-ifolder-mono-environment.sh
        export MONO_CFG_DIR=$MONO_RUNTIME_PATH/etc
        export IFOLDER_MOD_MONO_SERVER2_PATH=/opt/novell/ifolder3/bin
        export IFOLDER_MONO_PATH=$MONO_RUNTIME_PATH
        cd /opt/novell/ifolder3/bin
        MONO_CMD=$MONO_RUNTIME_PATH/bin/mono
fi
$MONO_CMD OrphanReport.exe $@
