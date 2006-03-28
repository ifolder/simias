@echo off
rem ######################################################################
rem
rem  $RCSfile: iFolderWebClientJava.cmd,v $
rem
rem  Copyright (C) 2004 Novell, Inc.
rem
rem  Author: Rob
rem
rem ######################################################################

cd %0%\..

set CLASSPATH=%CLASSPATH%;iFolderWebClient.jar;iFolderWebAccess.jar
set CLASSPATH=%CLASSPATH%;axis.jar;jaxrpc.jar;saaj.jar
set CLASSPATH=%CLASSPATH%;commons-logging.jar;commons-discovery.jar

java -classpath "%CLASSPATH%" iFolderWebClient %*
