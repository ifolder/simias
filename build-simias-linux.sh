#!/bin/sh

###
#
# Script to generate tarball for simias client and building the RPM
# using autobuild
#
###

### TODO
#
# 1. Provide option to run mbuild
# 2. Allow options to be passed to 'build'
# 3. Smart handling of PEBKAC

# Stop on errors
set -e

# Generate the build number
BUILDNUM=`expr \`date +%G%j\` - 2000000`

# This script is for packaging sources that will be
# delivered to autobuild.

# This the script should be execute from the directory
# workarea/versioning/trunk/ark-iman/install
PACKAGE=${PACKAGE:="simias"}
PACKAGE_VER=${PACKAGE_VER:="1.8.2"}
PACKAGE_DIR=../
SRC_DIR=`basename \`pwd\``
TARBALL_NAME=$PACKAGE
NPS_BUILDNUM=`printf "%x%s\n" \`date +%_m\` \`date +%d\` | tr [:lower:] [:upper:]`
RPM_DIR="../rpms/$NPS_BUILDNUM"
HOST_ARCH=`uname -i`
OES2=${OES2:="10.3"}

PUB_DIR=x86_64
[ "$HOST_ARCH" = "i386" ] && PUB_DIR=i586

# Env variables for autobuild
#  - Check if BUILD_ROOT and BUILD_DIST have already been set
#  - If they are set, use them 
#  - else, define our own
HOST_DIST=`echo ${BUILD_DIST:="$OES2-$HOST_ARCH"}`
HOST_ROOT=`echo ${BUILD_ROOT:="/tmp/$TARBALL_NAME"}`

function usage() {
cat << EOM

Usage: $0 [options]

Known [options] are:

--mbuild                  Will trigger mbuild for both $OES2-i386 & $OES2-x86_64

--mbuild-only-for <arch>  Will trigger mbuild *only* for $OES2-<arch>,
                          where, <arch> should be one of i386 or x86_64

--help                    Show this message and exit

EOM
}

function trigger_mbuild() {
	echo -e "*** To be implemented...***\n"
	exit
}

# Picked shamelessly from autobuild's 'build' script :(
while test -n "$1"; do
	PARAM="$1"
	ARG="$2"
	shift
	case $PARAM in
		*--help|-help|-h)
			usage
			exit
		;;
		*--mbuild|-mbuild|-m)
			trigger_mbuild
			exit
		;;
		*--mbuild-only-for)
			MBUILD_ARCH="$ARG"
			trigger_mbuild
		;;
	esac
done

mkdir -p $RPM_DIR/{i586,x86_64}

./simias-client.sh

pushd $PACKAGE

# Check if autobuild is available, else point to wiki
if [ -x /opt/SuSE/bin/build ]
then
	. /opt/SuSE/bin/.profile
	echo "Running autobuild..."
	echo "You might want to set BUILD_DIST and BUILD_ROOT env variables"
	echo "before starting this build. (Refer to README.build for more info)"
	export BUILD_ROOT="$HOST_ROOT"
	export BUILD_DIST="$HOST_DIST"

	# If we are running on a 64bit machine, then build the 32bit RPMs
	# too
	if [ "$HOST_ARCH" = "x86_64" ]
	then
		export BUILD_DIST="$OES2-i386"
		linux32 build $PACKAGE.spec --prefer-rpms=../$RPM_DIR/i586 $ABUILD_OPTS
		cp `find $BUILD_ROOT/usr/src/packages/RPMS/ -name *.rpm` ../$RPM_DIR/i586
		rm -rf $BUILD_ROOT

		# Set BUILD_DIST back to its original value
		export BUILD_DIST="$HOST_DIST"
	fi

	build $PACKAGE.spec --prefer-rpms=../$RPM_DIR/$PUB_DIR $ABUILD_OPTS
else
	echo "##################################################################"
	echo "# You don't have autobuild setup on your machine. Please refer   #"
	echo "# to README.build for the pre-requisites for running this script #"
	echo "##################################################################"
fi
popd

cp `find $BUILD_ROOT/usr/src/packages/RPMS/ -name *.rpm` $RPM_DIR/$PUB_DIR

