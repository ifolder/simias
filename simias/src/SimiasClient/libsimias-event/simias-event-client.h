/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/

/**
 * SimiasEventClient is the main structure that is used to make the calls to the
 * Event Server.  In the sec_init function, memory is allocated for the
 * structure and in sec_cleanup, it is freed.
 */
typedef void * SimiasEventClient;

/**
 * Actions that indicate what to do with the simias events
 */
typedef enum
{
	ACTION_ADD_NODE_CREATED,
	ACTION_ADD_NODE_CHANGED,
	ACTION_ADD_NODE_DELETED,
	ACTION_ADD_COLLECTION_SYNC,
	ACTION_ADD_FILE_SYNC,
	ACTION_ADD_NOTIFY_MESSAGE,
	ACTION_REMOVE_NODE_CREATED,
	ACTION_REMOVE_NODE_CHANGED,
	ACTION_REMOVE_NODE_DELETED,
	ACTION_REMOVE_COLLECTION_SYNC,
	ACTION_REMOVE_FILE_SYNC,
	ACTION_REMOVE_NOTIFY_MESSAGE
} IPROC_EVENT_ACTION;

/**
 * Event Structures
 */
 
/* Node Event */
typedef struct
{
	char *event_type;
	char *action;
	char *time;
	char *source;
	char *collection;
	char *type;
	char *event_id;
	char *node;
	char *flags;
	char *master_rev;
	char *slave_rev;
	char *file_size;
} SimiasNodeEvent;

/* Collection Sync Event */
typedef struct 
{
	char *event_type;
	char *name;
	char *id;
	char *action;
	char *successful;
} SimiasCollectionSyncEvent;

/* File Sync Event */
typedef struct 
{
	char *event_type;
	char *collection_id;
	char *object_type;
	char *delete_str;
	char *name;
	char *size;
	char *size_to_sync;
	char *size_remaining;
	char *direction;
} SimiasFileSyncEvent;

/* Notify Event */
typedef struct 
{
	char *event_type;
	char *message;
	char *time;
	char *type;
} SimiasNotifyEvent;

/**
 * Callback function prototypes
 */
void (*simias_node_event_callback) (SimiasNodeEvent *);
void (*simias_collection_sync_event_callback) (SimiasCollectionSyncEvent *);
void (*simias_file_sync_event_callback) (SimiasFileSyncEvent *);
void (*simias_notify_event_callback) (SimiasNotifyEvent *);

/**
 * Public Functions
 */

/**
 * Initializes the Simias Event Client
 * 
 * param: error_handler Delegate that gets called if an error occurs.  A NULL
 * may be passed in if the application does not care to be notified of errors.
 * 
 * Returns 0 if successful or -1 if there was an error.
 */
static int sec_init (SimiasEventClient *sec,
									 void *error_handler);

/**
 * Cleans up the Simias Event Client
 * 
 * This function should be called when the application is no longer using the
 * Simias Event Client.
 */
static int sec_cleanup (SimiasEventClient *sec);

/**
 * Registers this client with the server to listen for Simias events.
 * 
 * Returns 0 if successful or -1 if there was an error.
 */
static int sec_register (SimiasEventClient sec);

/**
 * Deregisters this client with the server
 * 
 * Returns 0 if successful or -1 if there was an error.
 */
static int sec_deregister (SimiasEventClient sec);

/**
 * Start subscribing to or unsubscribing from the specified event.
 *
 * param action: Action to take regarding the event.
 * param handler: Callback function that gets called when the specified event
 * happens or is to be removed.
 */
static int sec_set_event (SimiasEventClient sec, 
						  IPROC_EVENT_ACTION action,
						  void (*handler)(void *));
