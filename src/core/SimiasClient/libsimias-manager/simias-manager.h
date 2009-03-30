/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Mike Lasky <mlasky@novell.com> 
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#ifndef _simias_manager_h_
#define _simias_manager_h_

typedef void *	Manager;

/*
 * Method:	Allocates a Manager object and populates it with the defaults.
 * Returns:	A pointer to a Manager object.
 */
extern Manager *AllocateManager();

/*
 * Method:	Allocates a Manager object and populates it with the specified
 * command line arguments.
 * Returns:	A pointer to a Manager object.
 */
extern Manager *AllocateManagerWithArgs( int argsLength, char *args[] );

/*
 * Frees the specified Manager object.
 */
extern void FreeManager( Manager *pManager );

/*
 * Gets the path to the Simias.exe application.
 */
extern const char *GetApplicationPath( Manager *pManager );

/*
 * Gets the path to the simias data directory.
 */
extern const char *GetDataPath( Manager *pManager );

/*
 * Gets whether to run in a server configuration.
 */
extern char GetIsServer( Manager *pManager );

/*
 * Gets the web service listener port.
 */
extern int GetWebPort( Manager *pManager );

/*
 * Gets whether to show console output for the Simias process.
 */
extern char GetShowConsole( Manager *pManager );

/*
 * Gets the url for the local web service.
 */
extern const char *GetWebServiceUrl( Manager *pManager );

/*
 * Gets whether to print extra informational messages.
 */
extern char GetVerbose( Manager *pManager );

/*
 * Sets a new path to the Simias.exe application.
 */
extern void SetApplicationPath( Manager *pManager, const char *pApplicationPath );

/*
 * Sets a new path to the simias data directory.
 */
extern void SetDataPath( Manager *pManager, const char *pDataPath );

/*
 * Sets whether to run in a server configuration.
 */
extern void SetIsServer( Manager *pManager, char isServer );

/*
 * Sets a new web service listener port.
 */
extern void SetWebPort( Manager *pManager, int port );

/*
 * Sets whether to show console output for the Simias process.
 */
extern void SetShowConsole( Manager *pManager, char showConsole );

/*
 * Sets whether to print extra informational messages.
 */
extern void SetVerbose( Manager *pManager, char verbose );

/*
 * Starts the simias process running.
 */
extern const char *Start( Manager *pManager );

/*
 * Stops the simias process from running.
 */
extern int Stop( Manager *pManager );

#endif	/*-- _simias_manager_h_ --*/
