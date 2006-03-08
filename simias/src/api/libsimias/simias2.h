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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/
#ifndef _SIMIAS2_H
#define _SIMIAS2_H

#include <stdbool.h>

#define SIMIAS_SUCCESS 0
#define SIMIAS_ERROR_UNKNOWN	-1

typedef void *SimiasHandle;
typedef void *SimiasNodeList;
typedef void *SimiasNode;
typedef void *SimiasProperty;

int simias_init_local(SimiasHandle *hSimias);
int simias_free(SimiasHandle *hSimias);

int simias_ping(SimiasHandle hSimias);
int simias_commit(SimiasHandle hSimias,
				  SimiasNode hNode);

/*******************************************
	Domain Functions
********************************************/
int simias_get_domains(SimiasHandle hSimias, SimiasNodeList *hNodeList);


/*******************************************
	Collection Functions
********************************************/
int simias_get_collections(SimiasHandle hSimias, SimiasNodeList *hNodeList);
int simias_get_collections_by_type(SimiasHandle hSimias, 
								   SimiasNodeList *hNodeList, 
								   const char *type);
int simias_get_collections_for_domain(SimiasHandle hSimias, 
									  SimiasNodeList *hNodeList, 
									  const char *domainID);
int simias_get_collections_for_domain_by_type(SimiasHandle hSimias, 
											  SimiasNodeList *hNodeList, 
											  const char *domainID, 
											  const char *type);
int simias_create_collection( SimiasNode *hNode, 
							  const char *domainID, 
							  const char *type);




/*******************************************
	Node Functions
********************************************/
int simias_get_nodes(SimiasHandle hSimias, 
					SimiasNodeList *hNodeList, 
					const char *collectionID);
int simias_get_nodes_by_type(SimiasHandle hSimias, 
					SimiasNodeList *hNodeList, 
					const char *collectionID,
					const char *type);

char *simias_node_get_name(SimiasNode hNode);
char *simias_node_get_id(SimiasNode hNode);
char *simias_node_get_type(SimiasNode hNode);

int simias_node_create(SimiasNode *hNode, char *name, char *type);
int simias_node_free(SimiasNodeList *hNodeList);


int simias_node_set_property(SimiasNode hNode, SimiasProperty hProp);
int simias_node_remove_property(SimiasNode hNode, char *name);


int simias_nodelist_extract_node(SimiasNodeList hNodeList, 
								SimiasNode *hNode, 
								int index);
int simias_nodelist_extract_node_by_name(SimiasNodeList hNodeList,
										SimiasNode *hNode,
										const char *name);
int simias_nodelist_get_node(SimiasNodeList hNodeList, 
								SimiasNode *hNode, 
								int index);
int simias_nodelist_get_node_count(SimiasNodeList hNodeList, int *count);
int simias_nodelist_free(SimiasNodeList *hNodeList);


/*******************************************
	Property Functions
********************************************/
int simias_property_create(SimiasProperty *hProperty, char *name, 
									char *type,	char *value);
int simias_property_free(SimiasProperty *hProp);
int simias_property_get_count(SimiasNode hNode);
int simias_property_extract_property(SimiasNode hNode, 
									  SimiasProperty *hProp,
									  int index);
									  
char *simias_property_get_name(SimiasProperty hProp);
char *simias_property_get_type(SimiasProperty hProp);
char *simias_property_get_value_as_string(SimiasProperty hProp);

#endif	// _SIMIAS2_H