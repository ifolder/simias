@echo off
rem ######################################################################
rem
rem  $RCSfile: ifolder-reports-generate.cmd,v $
rem
rem  Copyright (C) 2004 Novell, Inc.
rem
rem  Author: Rob
rem
rem ######################################################################

cd %0%\..

iFolderReportsClient.exe %*
