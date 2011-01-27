#!/bin/sh

###
#
# Script to generate tarball for ifolder3 server and building the RPM
# using autobuild
#
###

#
# Stop on errors
set -e

# Generate the build number
BUILDNUM=`expr \`date +%G%j\` - 2000000`

# This script is for packaging sources that will be
# delivered to autobuild.

PACKAGE_DIR=../
PACKAGE_VER=${PACKAGE_VER:="3.8.4"}
PACKAGE=${PACKAGE:="ifolder3-enterprise"}
SRC_DIR=`basename \`pwd\``
TARBALL_NAME=$PACKAGE
NPS_BUILDNUM=`printf "%x%s\n" \`date +%_m\` \`date +%d\` | tr [:lower:] [:upper:]`

# Removing only those tarballs that have been generated by us
echo "Removing old tarball(s)..."
[ -d $PACKAGE ] && rm -rf $PACKAGE

# Create the symlinks - much more efficient than copying sources
##ln -s $SRC_DIR $PACKAGE_DIR/$TARBALL_NAME

# Update work area
if [ -d .svn ]
then
	echo "Updating work area..."
	svn up 
fi

# Handle the situation when Simias sources have been checked out
# / exported to a different directory, more so, in a directory called
# $TARBALL_NAME
if [ ! -d ../$TARBALL_NAME ]
then
        mkdir ../$TARBALL_NAME
        cp -rf ../`basename \`pwd\``/* ../$TARBALL_NAME
fi

# Prepare spec file
mkdir -p $PACKAGE
echo "Preparing spec file and copying to $PACKAGE/ ..."
sed -e "s/@@BUILDNUM@@/$BUILDNUM/" package/linux/$PACKAGE.spec.autobuild > $PACKAGE/$PACKAGE.spec

# Create the tarballs
echo "Generating tarball for $PACKAGE..."
pushd $PACKAGE_DIR
#tar -c --wildcards --exclude "*.svn*" --exclude "$PACKAGE" --exclude "*.tar.gz" -czf $TARBALL_NAME.tar.gz $TARBALL_NAME
tar -c --wildcards --exclude "*.svn*" --exclude "*.tar.gz" -czf $TARBALL_NAME.tar.gz $TARBALL_NAME
popd

# Copying tarballs
echo "Tarball generated!!"
mv $PACKAGE_DIR/$TARBALL_NAME.tar.gz $PACKAGE

# Let's not nuke ourselves!
[  "`basename \`pwd\``" = "$TARBALL_NAME" ] || rm -rf ../$TARBALL_NAME

