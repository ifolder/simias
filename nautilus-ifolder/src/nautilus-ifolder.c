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
#include <libnautilus-extension/nautilus-extension-types.h>
#include <libnautilus-extension/nautilus-file-info.h>
#include <libnautilus-extension/nautilus-info-provider.h>
#include <libnautilus-extension/nautilus-menu-provider.h>

#include <eel/eel-stock-dialogs.h>

#include <gtk/gtk.h>
#include <glib/gi18n-lib.h>
#include <string.h>
#include <stdio.h>
#include <unistd.h>

#include <libgnomevfs/gnome-vfs-utils.h>

#include <simias/simias-event-client.h>

#include "iFolderClientStub.h"
#include "iFolderClient.nsmap"

#include "nautilus-ifolder.h"

#define IFOLDER_FIFO_NAME ".nautilus-ifolder-fifo"
#define IFOLDER_BUF_SIZE 1024

typedef struct {
	GObject parent_slot;
} iFolderNautilus;

typedef struct {
	GObjectClass parent_slot;
} iFolderNautilusClass;

typedef struct {
	GtkWidget	*window;
	gchar		*title;
	gchar		*message;
	gchar		*detail;
} iFolderErrorMessage;

static GType provider_types[1];

static void ifolder_extension_register_type (GTypeModule *module);
static void ifolder_nautilus_instance_init (iFolderNautilus *ifn);

static GObjectClass * parent_class = NULL;
static GType ifolder_nautilus_type;

static SimiasEventClient ec;
static gboolean b_nautilus_ifolder_running;

/**
 * This hashtable is used to track the iFolders that the extension has seen and
 * added an emblem to.  When a SimiasNodeDeleted event occurs and the iFolder is
 * in this list, it will be used to remove the iFolder emblem from the Folder.
 * 
 * Hashtable keys:		iFolder Simias Node ID
 * Hashtable values: 	Nautilus File URI
 */
static GHashTable *seen_ifolders_ht;

/**
 * FIXME: Once nautilus-extension provides nautilus_file_info_get () we
 * can change our implementation to use the functions defined in the internal
 * nautilus API.
 */
extern NautilusFile * nautilus_file_get_existing (const char *uri);
extern NautilusFile * nautilus_file_get (const char *uri);

/**
 * Private function forward declarations
 */
static void init_gsoap (struct soap *p_soap);
static void cleanup_gsoap (struct soap *p_soap);
gchar * get_file_path (NautilusFileInfo *file);
gboolean is_ifolder_running ();
gboolean is_ifolder (NautilusFileInfo *file);
gboolean can_be_ifolder (NautilusFileInfo *file);
gint create_local_ifolder (NautilusFileInfo *file);
gchar * get_ifolder_id_by_local_path (gchar *path);
gint revert_ifolder (NautilusFileInfo *file);
gchar * get_unmanaged_path (gchar *ifolder_id);
GSList *get_all_ifolder_paths ();

/* Functions for seen_ifolders_ht (GHashTable) */
void seen_ifolders_ht_destroy_key (gpointer key);
void seen_ifolders_ht_destroy_value (gpointer value);
void ht_invalidate_ifolder (gpointer key, gpointer value, gpointer user_data);
gboolean ht_remove_ifolder (gpointer key, gpointer value, gpointer user_data);

/* Functions used to work on GSList of iFolder local paths */
void slist_invalidate_local_path (gpointer data, gpointer user_data);
void slist_free_local_path_str (gpointer data, gpointer user_data);

/**
 * This function is intended to be called using g_idle_add by the event system
 * to let Nautilus invalidate the extension info for the given 
 * NautilusFileInfo *.  It must be run from the main loop (hence being called
 * with g_idle_add ()).
 */
gboolean
invalidate_ifolder_extension_info (void *user_data)
{
 	NautilusFileInfo *file = (NautilusFileInfo *)user_data;
	if (file) {
		nautilus_file_info_invalidate_extension_info (file);
		
		g_object_unref (G_OBJECT(file));
	}
	
	return FALSE;
}

/**
 * Callback functions for the Simias Event Client
 */
int
simias_node_created_cb (SimiasNodeEvent *event, void *data)
{
	char *file_uri;
	gchar *unmanaged_path;
	NautilusFile *file;
	printf ("nautilus-ifolder: simias_node_created_cb () entered\n");
	
	unmanaged_path = get_unmanaged_path (event->node);
	if (unmanaged_path != NULL) {
		file_uri = gnome_vfs_get_uri_from_local_path (unmanaged_path);
		free (unmanaged_path);

		if (file_uri) {
			/**
			 * If the extension has ever been asked to provide information about
			 * the folder, it needs to be invalidated so that nautilus will ask
			 * for the information again.  This will allow the iFolder emblem to
			 * appear when a new iFolder is created.
			 */		
			/* FIXME: Change the following to be nautilus_file_info_get_existing () once it's available in the Nautilus Extension API */
			file = nautilus_file_get_existing (file_uri);
														 
			if (file) {
				g_printf ("Found NautilusFile: %s\n", file_uri);
				/* Let nautilus run this in the main loop */
				g_idle_add (invalidate_ifolder_extension_info, file);
			}

			free (file_uri);
		}
	}
	
	return 0;
}

int
simias_node_deleted_cb (SimiasNodeEvent *event, void *data)
{
	gchar *file_uri;
	NautilusFile *file;
	
	printf ("nautilus-ifolder: simias_node_deleted_cb () entered\n");
	
	/**
	 * Look in the seen_ifolders_ht (GHashTable) to see if we've ever added an
	 * iFolder emblem onto this folder.
	 */
	file_uri = (gchar *)g_hash_table_lookup (seen_ifolders_ht, event->node);
	if (file_uri) {
		/**
		 * Get an existing (in memory) NautilusFileInfo object associated with
		 * this file_uri and invalidate the extension information.
		 */
		/* FIXME: Change the following to be nautilus_file_info_get_existing () once it's available in the Nautilus Extension API */
		file = nautilus_file_get_existing (file_uri);
		if (file) {
			/* Allow nautilus to run this in the main loop (problems otherwise) */
			g_idle_add (invalidate_ifolder_extension_info, file);
		}
		
		/**
		 * Now that this folder is not an iFolder anymore, we can remove it
		 * from the seen_ifolders_ht (GHashTable).
		 */
		g_hash_table_remove (seen_ifolders_ht, event->node);
	}
	
	return 0;
}

int
ec_state_event_cb (SEC_STATE_EVENT state_event, const char *message, void *data)
{
	SimiasEventClient *ec = (SimiasEventClient *)data;
	int i;
	GSList *ifolder_paths;
	
	switch (state_event) {
		case SEC_STATE_EVENT_CONNECTED:
			g_print ("nautilus-ifolder: Connected event received by SEC\n");

			/* Register our event handler */
			sec_set_event (*ec, ACTION_NODE_CREATED, true, (SimiasEventFunc)simias_node_created_cb, NULL);
			sec_set_event (*ec, ACTION_NODE_DELETED, true, (SimiasEventFunc)simias_node_deleted_cb, NULL);
			g_printf ("nautilus-ifolder: finished registering for simias events\n");
			
			/**
			 * Get a list of all the local paths that are iFolders.  For each of
			 * the paths returned, call nautilus_file_get_existing () and
			 * invalidate our extension information so that an iFolder icon will
			 * be added to folders that are iFolders.
			 */
			ifolder_paths = get_all_ifolder_paths ();
			if (!ifolder_paths)
				break;
			 	
			g_slist_foreach (ifolder_paths, slist_invalidate_local_path, NULL);
			g_slist_foreach (ifolder_paths, slist_free_local_path_str, NULL);
			g_slist_free (ifolder_paths);

			break;
		case SEC_STATE_EVENT_DISCONNECTED:
			g_print ("nautilus-ifolder: Disconnected event received by SEC\n");

			/**
			 * Iterate through seen_ifolders_ht and invalidate the extension
			 * info so that all iFolder emblems are removed when iFolder is not
			 * running.
			 */
			g_hash_table_foreach (seen_ifolders_ht,
								  ht_invalidate_ifolder, NULL);

			/**
			 * Since iFolder is no longer running (or at least the event server
			 * is down, remove all the entries in seen_ifolders_ht.
			 */
			g_hash_table_foreach_remove (seen_ifolders_ht,
										 ht_remove_ifolder, NULL);
			break;
		case SEC_STATE_EVENT_ERROR:
			if (message) {
				g_print ("Error in Simias Event Client: %s\n", message);
			} else {
				g_print ("An unknown error occurred in Simias Event Client\n");
			}
			break;
		default:
			fprintf (stderr, "An unknown Simias Event Client State Event occurred\n");
	}
	
	return 0;
}

static int
start_simias_event_client ()
{
	if (sec_init (&ec, ec_state_event_cb, &ec) != 0) {
		g_printf ("sec_init failed\n");
		return -1;
	}
	
	if (sec_register (ec) != 0) {
		g_printf ("sec_register failed\n");
		return -1;
	}
	
	printf ("sec registration complete\n");

	return 0;
}

/**
 * gSOAP
 */
char *soapURL = NULL;

static void
init_gsoap (struct soap *p_soap)
{
	/* Initialize gSOAP */
	soap_init (p_soap);
	soap_set_namespaces (p_soap, iFolderClient_namespaces);
}

static void
cleanup_gsoap (struct soap *p_soap)
{
	/* Cleanup gSOAP */
	soap_end (p_soap);
}

/**
 * Utility functions
 */
 
/**
 * g_free () must be called on the returned string
 */
gchar *
get_file_path (NautilusFileInfo *file)
{
	gchar *file_path, *uri;

	file_path = NULL;
	
	if (file) {
		uri = nautilus_file_info_get_uri (file);
		if (uri) {
			file_path = gnome_vfs_get_local_path_from_uri (uri);
			g_free (uri);
		}
	}

	return file_path;
}

/**
 * Calls to iFolder via GSoap
 */
gboolean
is_ifolder_running ()
{
	struct soap soap;
	gboolean b_is_ifolder_running = TRUE;
	int err_code;

	struct _ns1__Ping ns1__Ping;
	struct _ns1__PingResponse ns1__PingResponse;

	init_gsoap (&soap);
	err_code = soap_call___ns1__Ping (&soap,
										soapURL,
						   				NULL,
						   				&ns1__Ping,
						   				&ns1__PingResponse);
						   				
	if (err_code != SOAP_OK || soap.error) {
		b_is_ifolder_running = FALSE;
	}
	
	cleanup_gsoap (&soap);
	
	return b_is_ifolder_running;
}
 
gboolean
is_ifolder (NautilusFileInfo *file)
{
	struct soap soap;
	gboolean b_is_ifolder = FALSE;
	gchar *folder_path;
	
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		g_print ("****About to call IsiFolder (");
		g_print (folder_path);
		g_print (")...\n");
		struct _ns1__IsiFolder ns1__IsiFolder;
		struct _ns1__IsiFolderResponse ns1__IsiFolderResponse;
		ns1__IsiFolder.LocalPath = folder_path;
		init_gsoap (&soap);
		soap_call___ns1__IsiFolder (&soap, 
									soapURL, 
									NULL, 
									&ns1__IsiFolder, 
									&ns1__IsiFolderResponse);		
		if (soap.error) {
			g_print ("****error calling IsiFolder***\n");
			soap_print_fault (&soap, stderr);
		} else {
			g_print ("***calling IsiFolder succeeded***\n");
			if (ns1__IsiFolderResponse.IsiFolderResult)
				b_is_ifolder = TRUE;
		}

		cleanup_gsoap (&soap);
		g_free (folder_path);
	}

	return b_is_ifolder;
}

gboolean
can_be_ifolder (NautilusFileInfo *file)
{
	struct soap soap;
	gchar *folder_path;
	gboolean b_can_be_ifolder = TRUE;
	
	if (!nautilus_file_info_is_directory (file))
		return FALSE;
		
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		g_print ("****About to call CanBeiFolder (");
		g_print (folder_path);
		g_print (")...\n");
		struct _ns1__CanBeiFolder ns1__CanBeiFolder;
		struct _ns1__CanBeiFolderResponse ns1__CanBeiFolderResponse;
		ns1__CanBeiFolder.LocalPath = folder_path;
		init_gsoap (&soap);
		soap_call___ns1__CanBeiFolder (&soap,
									   soapURL, 
									   NULL, 
									   &ns1__CanBeiFolder, 
									   &ns1__CanBeiFolderResponse);
		if (soap.error) {
			g_print ("****error calling CanBeiFolder***\n");
			soap_print_fault (&soap, stderr);
		} else {
			g_print ("***calling CanBeiFolder succeeded***\n");
			if (!ns1__CanBeiFolderResponse.CanBeiFolderResult)
				b_can_be_ifolder = FALSE;
		}

		cleanup_gsoap (&soap);
		g_free (folder_path);
	}
	
	return b_can_be_ifolder;
}

gint
create_local_ifolder (NautilusFileInfo *file)
{
	struct soap soap;
	gchar *folder_path;
	
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		g_print ("****About to call CreateLocaliFolder (");
		g_print (folder_path);
		g_print (")...\n");
		struct _ns1__CreateLocaliFolder ns1__CreateLocaliFolder;
		struct _ns1__CreateLocaliFolderResponse ns1__CreateLocaliFolderResponse;
		ns1__CreateLocaliFolder.Path = folder_path;
		init_gsoap (&soap);
		soap_call___ns1__CreateLocaliFolder (&soap, 
											 soapURL, 
											 NULL, 
											 &ns1__CreateLocaliFolder, 
											 &ns1__CreateLocaliFolderResponse);
		g_free (folder_path);
		if (soap.error) {
			g_print ("****error calling CreateLocaliFolder***\n");
			soap_print_fault (&soap, stderr);
			cleanup_gsoap (&soap);
			return -1;
		} else {
			g_print ("***calling CreateLocaliFolder succeeded***\n");
			struct ns1__iFolderWeb *ifolder = 
				ns1__CreateLocaliFolderResponse.CreateLocaliFolderResult;
			if (ifolder == NULL) {
				g_print ("***The created iFolder is NULL\n");
				cleanup_gsoap (&soap);
				return -1;
			} else {
				g_print ("***The created iFolder's ID is: ");
				g_print (ifolder->ID);
				g_print ("\n");
			}
		}

		cleanup_gsoap (&soap);
	} else {
		/* Error getting the folder path */
		return -1;
	}
	
	return 0;
}

gchar *
get_ifolder_id_by_local_path (gchar *path)
{
	struct soap soap;
	gchar *ifolder_id;
	
	ifolder_id = NULL;

	if (path != NULL) {
		g_print ("****About to call GetiFolderByLocalPath (");
		g_print (path);
		g_print (")...\n");
		struct _ns1__GetiFolderByLocalPath ns1__GetiFolderByLocalPath;
		struct _ns1__GetiFolderByLocalPathResponse ns1__GetiFolderByLocalPathResponse;
		ns1__GetiFolderByLocalPath.LocalPath = path;
		init_gsoap (&soap);
		soap_call___ns1__GetiFolderByLocalPath (&soap, 
										soapURL, 
										NULL, 
										&ns1__GetiFolderByLocalPath, 
										&ns1__GetiFolderByLocalPathResponse);
		if (soap.error) {
			g_print ("****error calling GetiFolderByLocalPath***\n");
			soap_print_fault (&soap, stderr);
			cleanup_gsoap (&soap);
			return NULL;
		} else {
			g_print ("***calling GetiFolderByLocalPath succeeded***\n");
			struct ns1__iFolderWeb *ifolder = 
				ns1__GetiFolderByLocalPathResponse.GetiFolderByLocalPathResult;
			if (ifolder == NULL) {
				g_print ("***GetiFolderByLocalPath returned NULL\n");
				cleanup_gsoap (&soap);
				return NULL;
			} else {
				g_print ("***The iFolder's ID is: ");
				g_print (ifolder->ID);
				g_print ("\n");
				ifolder_id = strdup (ifolder->ID);
			}
		}

		cleanup_gsoap (&soap);
	}

	return ifolder_id;
}

gint
revert_ifolder (NautilusFileInfo *file)
{
	struct soap soap;
	gchar *folder_path;
	gchar *ifolder_id;
	
	folder_path = get_file_path (file);
	if (folder_path != NULL) {
		ifolder_id = get_ifolder_id_by_local_path (folder_path);
		g_free (folder_path);
		if (ifolder_id != NULL) {
			g_print ("****About to call RevertiFolder ()\n");
			struct _ns1__RevertiFolder ns1__RevertiFolder;
			struct _ns1__RevertiFolderResponse ns1__RevertiFolderResponse;
			ns1__RevertiFolder.iFolderID = ifolder_id;
			init_gsoap (&soap);
			soap_call___ns1__RevertiFolder (&soap, 
												 soapURL, 
												 NULL, 
												 &ns1__RevertiFolder, 
												 &ns1__RevertiFolderResponse);
			g_free (ifolder_id);
			if (soap.error) {
				g_print ("****error calling RevertiFolder***\n");
				soap_print_fault (&soap, stderr);
				cleanup_gsoap (&soap);
				return -1;
			} else {
				g_print ("***calling RevertiFolder succeeded***\n");
				struct ns1__iFolderWeb *ifolder = 
					ns1__RevertiFolderResponse.RevertiFolderResult;
				if (ifolder == NULL) {
					g_print ("***The reverted iFolder is NULL\n");
					return -1;
				} else {
					g_print ("***The reverted iFolder's ID was: ");
					g_print (ifolder->ID);
					g_print ("\n");
				}
			}

			cleanup_gsoap (&soap);
		}
	} else {
		/* Error getting the folder path */
		return -1;
	}
	
	return 0;
}

gchar *
get_unmanaged_path (gchar *ifolder_id)
{
	struct soap soap;
	gchar *unmanaged_path;
	
	unmanaged_path = NULL;

	if (ifolder_id != NULL) {
		g_print ("****About to call GetiFolder (");
		g_print (ifolder_id);
		g_print (")...\n");
		struct _ns1__GetiFolder ns1__GetiFolder;
		struct _ns1__GetiFolderResponse ns1__GetiFolderResponse;
		ns1__GetiFolder.iFolderID = ifolder_id;
		init_gsoap (&soap);
		soap_call___ns1__GetiFolder (&soap,
									 soapURL,
									 NULL,
									 &ns1__GetiFolder,
									 &ns1__GetiFolderResponse);
		if (soap.error) {
			g_print ("****error calling GetiFolder***\n");
			soap_print_fault (&soap, stderr);
			cleanup_gsoap (&soap);
			return NULL;
		} else {
			g_print ("***calling GetiFolder succeeded***\n");
			struct ns1__iFolderWeb *ifolder = 
				ns1__GetiFolderResponse.GetiFolderResult;
			if (ifolder == NULL) {
				g_print ("***GetiFolder returned NULL\n");
				cleanup_gsoap (&soap);
				return NULL;
			} else {
				if (ifolder->UnManagedPath != NULL) {
					g_print ("***The iFolder's Unmanaged Path is: ");
					g_print (ifolder->UnManagedPath);
					g_print ("\n");
					unmanaged_path = strdup (ifolder->UnManagedPath);
				}
			}
		}

		cleanup_gsoap (&soap);
	}

	return unmanaged_path;
}

/**
 * Returns a GSList (singly-linked list) containing the local path of all known
 * iFolders.  When the caller is finished with the GSList, they should call
 * g_slist_free () to cleanup the memory used by the list.
 */
GSList *
get_all_ifolder_paths ()
{
	GSList *ifolder_local_paths;
	int i;			
	struct soap soap;
	struct _ns1__GetAlliFolders ns1__GetAlliFolders;
	struct _ns1__GetAlliFoldersResponse ns1__GetAlliFoldersResponse;
	char *unmanaged_path;
	
	ifolder_local_paths = NULL;
	
	g_print ("****About to call GetiFolders ()\n");

	init_gsoap (&soap);
	soap_call___ns1__GetAlliFolders (&soap,
									 soapURL,
									 NULL,
									 &ns1__GetAlliFolders,
									 &ns1__GetAlliFoldersResponse);
	if (soap.error) {
		g_print ("****error calling GetAlliFolders***\n");
		soap_print_fault (&soap, stderr);
		cleanup_gsoap (&soap);
		return NULL;
	} else {
		g_print ("***calling GetAlliFolders succeeded***\n");
		struct ns1__ArrayOfIFolderWeb *array_of_ifolders =
			ns1__GetAlliFoldersResponse.GetAlliFoldersResult;
		if (array_of_ifolders == NULL) {
			g_print ("***GetAlliFolders returned NULL\n");
			cleanup_gsoap (&soap);
			return NULL;
		} else {
			/* Get all the iFolder ID's and copy them into a new array */
			if (array_of_ifolders->__sizeiFolderWeb > 0) {
				for (i = 0; i < array_of_ifolders->__sizeiFolderWeb; i++) {
					/**
					 * iFolders that are not local iFolders will not have an
					 * UnManagedPath.
					 */
					unmanaged_path = 
						array_of_ifolders->iFolderWeb [i]->UnManagedPath;
					if (unmanaged_path) {
						ifolder_local_paths = 
							g_slist_prepend (ifolder_local_paths,
											 strdup (unmanaged_path));
					}
				}
			}
		}
	}

	cleanup_gsoap (&soap);
	
	return ifolder_local_paths;
}

/**
 * Nautilus Info Provider Implementation
 */
static NautilusOperationResult
ifolder_nautilus_update_file_info (NautilusInfoProvider 	*provider,
								   NautilusFileInfo			*file,
								   GClosure					*update_complete,
								   NautilusOperationHandle	**handle)
{
	gchar *ifolder_id;
	gchar *file_uri;
	gchar *file_path;
	g_print ("--> ifolder_nautilus_update_file_info called\n");
	
	/* Don't do anything if the specified file is not a directory. */
	if (!nautilus_file_info_is_directory (file))
		return NAUTILUS_OPERATION_COMPLETE;
	
	if (is_ifolder_running ()) {
		file_path = get_file_path (file);
		if (file_path) {
			ifolder_id = get_ifolder_id_by_local_path (file_path);
			g_free (file_path);
			if (ifolder_id) {
				nautilus_file_info_add_emblem (file, "ifolder");

				file_uri = nautilus_file_info_get_uri (file);
				if (file_uri) {
					/**
					 * Store the file_uri into a hashtable with the key being
					 * the iFolder Simias Node ID.  This is needed because when
					 * we get a SimiasNodeDeleted event, the iFolder in Simias
					 * no longer has any path information.  This hash table is
					 * the only way we'll be able to cause Nautilus to
					 * invalidate our information so that the iFolder emblem
					 * will be removed.
					 */
					g_printf ("Adding iFolder to Hashtable: %s = %s\n", ifolder_id, file_uri);
					
					/**
					 * g_hash_table_insert () does not cleanup memory if a hash
					 * table entry is replaced by an insertion, so to make sure
					 * that we don't lose memory, call remove before the insert.
					 */
					g_hash_table_remove (seen_ifolders_ht, ifolder_id);
					
					/**
					 * The memory for ifolder_id and file_uri are freed when the
					 * hashtable item is removed from the hashtable.
					 */
					g_hash_table_insert (seen_ifolders_ht,
										 ifolder_id,	/* key */
										 file_uri);		/* value */
				} else {
					free (ifolder_id);
				}
			}
		}
	} else {
		g_print ("*** iFolder is NOT running\n");
	}

	return NAUTILUS_OPERATION_COMPLETE;
}

static void
ifolder_nautilus_info_provider_iface_init (NautilusInfoProviderIface *iface)
{
	iface->update_file_info	= ifolder_nautilus_update_file_info;
}

/**
 * Functions for seen_ifolders_ht (GHashTable)
 */
/**
 * This function gets called when the entry is being removed and this allows us
 * to cleanup the memory being used by the ifolder_id.
 */
void
seen_ifolders_ht_destroy_key (gpointer key)
{
	char *ifolder_id = (char *)key;
	
	free (ifolder_id);
}

/**
 * This function gets called when the entry is being removed and this allows us
 * to cleanup the memory being used by the file_uri.
 */
void
seen_ifolders_ht_destroy_value (gpointer value)
{
	gchar *file_uri = (gchar *)value;
	
	g_free (file_uri);
}

void
ht_invalidate_ifolder (gpointer key, gpointer value, gpointer user_data)
{
	gchar *file_uri;
	NautilusFileInfo *file;
	
	file_uri = (gchar *)value;
	file = nautilus_file_get_existing (file_uri);

	if (file) {
		g_printf ("ht_invalidate_ifolder: %s\n", file_uri);
		/* Let nautilus run this in the main loop */
		g_idle_add (invalidate_ifolder_extension_info, file);
	}
}

gboolean
ht_remove_ifolder (gpointer key, gpointer value, gpointer user_data)
{
	/**
	 * Since we have other functions that cleanup the memory on a remove, just
	 * return true and g_hash_table_remove will be called on this element.
	 */
	return TRUE;
}


/**
 * Functions used to work on GSList of iFolder local paths
 */
void
slist_invalidate_local_path (gpointer data, gpointer user_data)
{
	char *local_path;
	NautilusFileInfo *file;
	char *file_uri;
	
	local_path = (char *)data;
	
	file_uri = gnome_vfs_get_uri_from_local_path (local_path);
	if (file_uri) {
		file = nautilus_file_get_existing (file_uri);
													 
		if (file) {
			g_printf ("invalidate_local_path: %s\n", file_uri);
			/* Let nautilus run this in the main loop */
			g_idle_add (invalidate_ifolder_extension_info, file);
		} else {
	g_print ("nautilus-ifolder: \"%s\" existing not found\n", file_uri);
		}

		free (file_uri);
	}
}

void
slist_free_local_path_str (gpointer data, gpointer user_data)
{
	char *local_path = (char *)data;
	
	free (local_path);
}

/**
 * Nautilus Menu Provider Implementation
 */

gboolean
show_ifolder_error_message (void *user_data)
{
	g_print ("*** show_ifolder_error_message () called\n");
	iFolderErrorMessage *errMsg = (iFolderErrorMessage *)user_data;
	GtkDialog *message_dialog;

	message_dialog = eel_show_error_dialog (
						errMsg->message,
						errMsg->detail,
						errMsg->title,
						GTK_WINDOW (errMsg->window));
	gtk_dialog_run (message_dialog);
	gtk_object_destroy (GTK_OBJECT (message_dialog));
	
	free (errMsg);
	
	return FALSE;
}

/**
 * If this function returns NON-NULL, it contains a char * from the process
 * executed by popen and should be freed.  The char * only contains the first
 * line of output from the executed process.
 */
static void *
ifolder_dialog_thread (gpointer user_data)
{
	NautilusMenuItem *item;
	FILE *output;
	char readBuffer [1024];
	char *args = (char *)user_data;
	char *return_str = NULL;

	item = (NautilusMenuItem *)user_data;
	args = g_object_get_data (G_OBJECT (item), "ifolder_args");
	
	memset (readBuffer, '\0', sizeof (readBuffer));

	output = popen (args, "r");
	if (output == NULL) {
		/* error calling mono nautilus-ifolder.exe */
		g_print ("Error calling: ");
		g_print (args);
		g_print ("\n");
		free (args);
		iFolderErrorMessage *errMsg = malloc (sizeof (iFolderErrorMessage));
		errMsg->window = g_object_get_data (G_OBJECT (item), "parent_window");
		errMsg->title	= _("iFolder Error");
		errMsg->message	= _("Error opening dialog.");
		errMsg->detail	= _("Sorry, unable to open the window to perform the specified action.");
		g_idle_add (show_ifolder_error_message, errMsg);
		return;
	}
	
	if (fgets (readBuffer, 1024, output) != NULL) {
		return_str = strdup (readBuffer);
		g_print ("*** 1st line of STDOUT from popen: ");
		g_print (return_str);
		g_print ("\n");
	}

	free (args);
	pclose (output);
	
	return (void *)return_str;
}

static void *
create_ifolder_thread (gpointer user_data)
{
	NautilusMenuItem *item;
	GList *files;
	NautilusFileInfo *file;
	gint error;
	
	item = (NautilusMenuItem *)user_data;
	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);

	error = create_local_ifolder (file);
	if (error) {
		g_print ("An error occurred creating an iFolder\n");
		iFolderErrorMessage *errMsg = malloc (sizeof (iFolderErrorMessage));
		errMsg->window = g_object_get_data (G_OBJECT (item), "parent_window");
		errMsg->title	= _("iFolder Error");
		errMsg->message	= _("The folder could not be converted.");
		errMsg->detail	= _("Sorry, unable to convert the specified folder into an iFolder.");
		g_idle_add (show_ifolder_error_message, errMsg);
	}
}

static void
create_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Convert to iFolder selected\n");

	pthread_t thread;

	pthread_create (&thread, 
					NULL, 
					create_ifolder_thread,
					item);
}

static void *
revert_ifolder_thread (gpointer user_data)
{
	NautilusMenuItem *item;
	GList *files;
	NautilusFileInfo *file;
	gint error;

	item = (NautilusMenuItem *)user_data;
	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);
	
	error = revert_ifolder (file);
	if (error) {
		g_print ("An error occurred reverting an iFolder\n");
		iFolderErrorMessage *errMsg = malloc (sizeof (iFolderErrorMessage));
		errMsg->window = g_object_get_data (G_OBJECT (item), "parent_window");
		errMsg->title	= _("iFolder Error");
		errMsg->message	= _("The iFolder could not be reverted.");
		errMsg->detail	= _("Sorry, unable to revert the specified iFolder to a normal folder.");
		g_idle_add (show_ifolder_error_message, errMsg);
	}
}

static void
revert_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Revert to a Normal Folder selected\n");
	GtkDialog *message_dialog;
	GtkWidget *window;
	int response;
	pthread_t thread;

	window = g_object_get_data (G_OBJECT (item), "parent_window");

	message_dialog = eel_show_yes_no_dialog (
						_("Revert this iFolder?"), 
	                    _("This will revert this iFolder back to a normal folder and leave the files intact.  The iFolder will then be available from the server and will need to be setup in a different location in order to sync."),
						_("iFolder Confirmation"), 
						GTK_STOCK_YES,
						GTK_STOCK_NO,
						GTK_WINDOW (window));
	/* FIXME: Figure out why the next call doesn't set the default button to "NO" */
	gtk_dialog_set_default_response (message_dialog, GTK_RESPONSE_CANCEL);
	response = gtk_dialog_run (message_dialog);
	gtk_object_destroy (GTK_OBJECT (message_dialog));
	
	if (response == GTK_RESPONSE_YES) {
		pthread_create (&thread, 
						NULL, 
						revert_ifolder_thread,
						item);
	}
}

static void
share_ifolder_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Share with... selected\n");
	gchar *ifolder_path;
	gchar *ifolder_id;
	GList *files;
	NautilusFileInfo *file;
	pthread_t thread;
	char args [1024];
	memset (args, '\0', sizeof (args));
	
	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);
	if (file == NULL)
		return;
		
	ifolder_path = get_file_path (file);
	if (ifolder_path != NULL) {
		ifolder_id = get_ifolder_id_by_local_path (ifolder_path);
		if (ifolder_id != NULL) {
			sprintf (args, "%s share %s", NAUTILUS_IFOLDER_SH_PATH, ifolder_id);
			g_print ("args: ");
			g_print (args);
			g_print ("\n");
			
			g_free (ifolder_id);
		}
		
		g_free (ifolder_path);
	}
	
	if (strlen (args) <= 0)
		return;
		
	g_object_set_data (G_OBJECT (item),
				"ifolder_args",
				strdup(args));
		
	pthread_create (&thread, 
					NULL, 
					ifolder_dialog_thread,
					item);
}

static void
ifolder_properties_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Properties selected\n");	
	gchar *ifolder_path;
	gchar *ifolder_id;
	GList *files;
	NautilusFileInfo *file;
	pthread_t thread;
	char args [1024];
	memset (args, '\0', sizeof (args));
	
	files = g_object_get_data (G_OBJECT (item), "files");
	file = NAUTILUS_FILE_INFO (files->data);
	if (file == NULL)
		return;
		
	ifolder_path = get_file_path (file);
	if (ifolder_path != NULL) {
		ifolder_id = get_ifolder_id_by_local_path (ifolder_path);
		if (ifolder_id != NULL) {
			sprintf (args, "%s properties %s", 
					 NAUTILUS_IFOLDER_SH_PATH, ifolder_id);
			g_print ("args: ");
			g_print (args);
			g_print ("\n");
			
			g_free (ifolder_id);
		}
		
		g_free (ifolder_path);
	}
	
	if (strlen (args) <= 0)
		return;
		
	g_object_set_data (G_OBJECT (item),
				"ifolder_args",
				strdup(args));
		
	pthread_create (&thread, 
					NULL, 
					ifolder_dialog_thread,
					item);
}

static void
ifolder_help_callback (NautilusMenuItem *item, gpointer user_data)
{
	g_print ("Help... selected\n");	
	pthread_t thread;
	char args [1024];
	memset (args, '\0', sizeof (args));
	
	sprintf (args, "%s help", NAUTILUS_IFOLDER_SH_PATH);
	g_print ("args: ");
	g_print (args);
	g_print ("\n");
	
	if (strlen (args) <= 0)
		return;
		
	g_object_set_data (G_OBJECT (item),
				"ifolder_args",
				strdup(args));
		
	pthread_create (&thread, 
					NULL, 
					ifolder_dialog_thread,
					item);
}

static GList *
ifolder_nautilus_get_file_items (NautilusMenuProvider *provider,
								 GtkWidget *window,
								 GList *files)
{
	g_print ("--> ifolder_nautilus_get_file_items called\n");
	NautilusMenuItem *item;
	NautilusFileInfo *file;
	GList *items;

	/* For some reason, this function is called with files == NULL */
	if (files == NULL)
		return NULL;

	/**
	 * Multiple select on a file/folder is not supported.  If the user has
	 * selected more than one file/folder, don't add any iFolder context menus.
	 */
	if (g_list_length (files) > 1)
		return NULL;

	file = NAUTILUS_FILE_INFO (files->data);

	/**
	 * If the user selected a file (not a directory), don't add any iFolder
	 * context menus.
	 */
	if (!nautilus_file_info_is_directory (file))
		return NULL;
		
	/**
	 * Don't show any iFolder context menus if the iFolder client is not
	 * running
	 */
	if (!is_ifolder_running ())
		return NULL;
		
	items = NULL;
	
	if (is_ifolder (file)) {
		/* Menu item: Revert to a Normal Folder */
		item = nautilus_menu_item_new ("NautilusiFolder::revert_ifolder",
					_("Revert to a Normal Folder"),
					_("Revert the selected iFolder back to normal folder"),
					"stock_undo");
		g_signal_connect (item, "activate",
					G_CALLBACK (revert_ifolder_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		g_object_set_data_full (G_OBJECT (item), "parent_window",
								g_object_ref (window), g_object_unref);
		items = g_list_append (items, item);
		
		/* Menu item: Share with... */
		item = nautilus_menu_item_new ("NautilusiFolder::share_ifolder",
					_("Share iFolder with..."),
					_("Share the selected iFolder with another user"),
					NULL);
		g_signal_connect (item, "activate",
					G_CALLBACK (share_ifolder_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		g_object_set_data_full (G_OBJECT (item), "parent_window",
								g_object_ref (window), g_object_unref);
		items = g_list_append (items, item);
		
		/* Menu item: Properties */
		item = nautilus_menu_item_new ("NautilusiFolder::ifolder_properties",
					_("iFolder Properties"),
					_("View the properties of the selected iFolder"),
					"stock_properties");
		g_signal_connect (item, "activate",
					G_CALLBACK (ifolder_properties_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		g_object_set_data_full (G_OBJECT (item), "parent_window",
								g_object_ref (window), g_object_unref);
		items = g_list_append (items, item);
		
		/* Menu item: Help */
		item = nautilus_menu_item_new ("NautilusiFolder::ifolder_help",
					_("iFolder Help..."),
					_("View the iFolder help"),
					"stock_help");
		g_signal_connect (item, "activate",
					G_CALLBACK (ifolder_help_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		g_object_set_data_full (G_OBJECT (item), "parent_window",
								g_object_ref (window), g_object_unref);
		items = g_list_append (items, item);
	} else {
		/**
		 * If the iFolder API says that the current file cannot be an iFolder,
		 * don't add any iFolder context menus.
		 */
		if (!can_be_ifolder (file))
			return NULL;

		/* Menu item: Convert to an iFolder */
		item = nautilus_menu_item_new ("NautilusiFolder::create_ifolder",
					_("Convert to an iFolder"),
					_("Convert the selected folder to an iFolder"),
					"ifolder-folder");
		g_signal_connect (item, "activate",
					G_CALLBACK (create_ifolder_callback),
					provider);
		g_object_set_data (G_OBJECT (item),
					"files",
					nautilus_file_info_list_copy (files));
		g_object_set_data_full (G_OBJECT (item), "parent_window",
								g_object_ref (window), g_object_unref);
		items = g_list_append (items, item);
	}

	return items;
}
 
static void
ifolder_nautilus_menu_provider_iface_init (NautilusMenuProviderIface *iface)
{
	iface->get_file_items = ifolder_nautilus_get_file_items;
}

static GType
ifolder_nautilus_get_type (void)
{
	return ifolder_nautilus_type;
}


static void
ifolder_nautilus_instance_init (iFolderNautilus *ifn)
{
}

static void
ifolder_nautilus_class_init (iFolderNautilusClass *klass)
{
	parent_class = g_type_class_peek_parent (klass);
}

static void
ifolder_extension_register_type (GTypeModule *module)
{
	static const GTypeInfo info = {
		sizeof (iFolderNautilusClass),
		(GBaseInitFunc) NULL,
		(GBaseFinalizeFunc) NULL,
		(GClassInitFunc) ifolder_nautilus_class_init,
		NULL,
		NULL,
		sizeof (iFolderNautilus),
		0,
		(GInstanceInitFunc) ifolder_nautilus_instance_init,
	};
	
	ifolder_nautilus_type = g_type_module_register_type (module,
														  G_TYPE_OBJECT,
														  "NautilusiFolder",
														  &info, 0);
														  
	/* Nautilus Info Provider Interface */
	static const GInterfaceInfo info_provider_iface_info = 
	{
		(GInterfaceInitFunc)ifolder_nautilus_info_provider_iface_init,
		NULL,
		NULL
	};
	g_type_module_add_interface (module, 
								 ifolder_nautilus_type,
								 NAUTILUS_TYPE_INFO_PROVIDER,
								 &info_provider_iface_info);
								 
	/* Nautilus Menu Provider Interface */
	static const GInterfaceInfo menu_provider_iface_info = 
	{
		(GInterfaceInitFunc)ifolder_nautilus_menu_provider_iface_init,
		NULL,
		NULL
	};
	g_type_module_add_interface (module, 
								 ifolder_nautilus_type,
								 NAUTILUS_TYPE_MENU_PROVIDER,
								 &menu_provider_iface_info);
	/* Nautilus Property Page Interface */
	/* Nautilus Column Provider Interface (we probably won't need this one) */
}

static gchar *
getLocalServiceUrl ()
{
	char readBuffer [1024];
	char tmpUrl [1024];
	gchar *localServiceUrl = NULL;
	char args [1024];
	memset (args, '\0', sizeof (args));
	
	memset (readBuffer, '\0', sizeof (readBuffer));
	memset (tmpUrl, '\0', sizeof (tmpUrl));

	FILE *output;
	
	sprintf (args, "%s WebServiceURL", NAUTILUS_IFOLDER_SH_PATH);
	
	output = popen (args, "r");
	if (output == NULL) {
		/* error calling mono nautilus-ifolder.exe */
		g_print ("Error calling 'mono nautilus-ifolder.exe WebServiceURL");
		return NULL;
	}
	
	if (fgets (readBuffer, 1024, output) != NULL) {
		strcpy (tmpUrl, readBuffer);
		strcat (tmpUrl, "/iFolder.asmx");
		localServiceUrl = strdup (tmpUrl);
		g_print ("*** Web Service URL: ");
		g_print (localServiceUrl);
		g_print ("\n");
	}

	pclose (output);	
	
	return localServiceUrl;
}

void
nautilus_module_initialize (GTypeModule *module)
{
	g_print ("Initializing nautilus-ifolder extension\n");
	ifolder_extension_register_type (module);
	provider_types[0] = ifolder_nautilus_get_type ();
	
	b_nautilus_ifolder_running = TRUE;
	
	soapURL = getLocalServiceUrl ();
	
	/* Initialize the seen_ifolders_ht GHashTable */
	seen_ifolders_ht = 
		g_hash_table_new_full (g_str_hash, g_str_equal,
							   seen_ifolders_ht_destroy_key,
							   seen_ifolders_ht_destroy_value);
							   
	/* Start the Simias Event Client */
	start_simias_event_client ();
}

void
nautilus_module_shutdown (void)
{
	g_print ("Shutting down nautilus-ifolder extension\n");

	b_nautilus_ifolder_running = FALSE;

	/* Cleanup soapURL */	
	if (soapURL) {
		free (soapURL);
	}

	/* Cleanup the Simias Event Client */	
	if (sec_get_state (ec) == CLIENT_STATE_RUNNING) {
		if (sec_deregister (ec) != 0) {
			fprintf (stderr, "sec_deregister failed\n");
			return;
		}
	}
	
	/**
	 * Since we called sec_init (), call sec_cleanup () regardless if the event
	 * client is running or not.
	 */
	if (sec_cleanup (&ec) != 0) {
		fprintf (stderr, "sec_cleanup failed\n");
		return;
	}
	
	/* Cleanup the seen_ifolders_ht GHashTable */
	g_hash_table_destroy (seen_ifolders_ht);
}

void
nautilus_module_list_types (const GType **types, int *num_types)
{
	*types = provider_types;
	*num_types = G_N_ELEMENTS (provider_types);
}
