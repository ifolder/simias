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
*                 $Author: Boyd Timothy <btimothy@novell.com> 
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

#include "simias.h"
#include "simiasweb.h"

#include <simiasStub.h>
#include <simias.nsmap>

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include <libxml/tree.h>
#include <libxml/parser.h>
#include <libxml/xpath.h>
#include <libxml/xpathInternals.h>

#ifdef DEBUG
#define SIMIAS_DEBUG(args) do {printf("libsimias: "); printf args;} while (0)
#else
#define SIMIAS_DEBUG(args) do {} while (0)
#endif

#if defined(WIN32)
#define DIR_SEP "\\"
#else
#define DIR_SEP "/"
#endif

/* Global Variables */
static char *the_soap_url = NULL;

/* Foward Declarations */
/* Foward Declarations */
static char *parse_local_service_url(FILE *file);
static void init_gsoap (struct soap *p_soap);
static void cleanup_gsoap (struct soap *p_soap);
static char *get_soap_url(bool reread_config);

char* DerivePassword(char* str)
{
        char* ptr;
        if( (ptr=strchr( str, ':')) !=NULL)
        {
                return ptr+1;
        }
        else return str;
}


/* Function Implementations */
int
simias_get_local_service_url(char **url)
{
	char user_profile_dir[1024];
	char xsp_port_config_file_path[1024];
	char *xspPort;
	FILE *simias_conf_file;
	xmlDoc	*doc;
	xmlNode *root_element, *cur_node;
	
	if (!simias_get_user_profile_dir_path(user_profile_dir)) {
		return SIMIAS_ERROR_NO_USER_PROFILE;
	}

	SIMIAS_DEBUG((stderr, "User Profile Dir: %s\n", user_profile_dir));

/*
	sprintf(simias_config_file_path, "%s%sSimias.config",
			user_profile_dir, DIR_SEP);
	
	SIMIAS_DEBUG((stderr, "Simias Config File: %s\n", simias_config_file_path));

	// Attempt to open the file 
	simias_conf_file = fopen(simias_config_file_path, "r");
	if (!simias_conf_file) {
		SIMIAS_DEBUG((stderr, "Error opening \"%s\"\n", simias_config_file_path));
		return SIMIAS_ERROR_OPENING_CONFIG_FILE;
	}

	*url = parse_local_service_url(simias_conf_file);

	fclose(simias_conf_file);
*/

	/*
		The xspport.cfg file is formatted like this:
		
		<XspPortConfiguration>
  			<Port>32830</Port>
  		</XspPortConfiguration>
	*/

	sprintf(xsp_port_config_file_path, "%s%sxspport.cfg",
			user_profile_dir, DIR_SEP);

	doc = xmlReadFile(xsp_port_config_file_path, NULL, 0);
	if(doc == NULL)
	{
		return SIMIAS_ERROR_UNKNOWN;
	}

	root_element = xmlDocGetRootElement(doc);
    for(cur_node = root_element->children; cur_node; cur_node = cur_node->next)
    {
		if (cur_node->type == XML_ELEMENT_NODE)
        {
			// once we have found a child element break;
			break;
		}
	}

	if(cur_node == NULL)
	{
		xmlFreeDoc(doc);
		return SIMIAS_ERROR_UNKNOWN;
	}

	xspPort = (char *)xmlNodeGetContent(cur_node);

	*url = malloc(56);
	if(*url != NULL)
		sprintf(*url, "http://127.0.0.1:%s/simias10/", xspPort);

	if (!(*url)) {
		SIMIAS_DEBUG((stderr, "Couldn't find Local Service URL in \"%s\"\n",
					 xsp_port_config_file_path));
		return SIMIAS_ERROR_UNKNOWN;
	}
	
	return SIMIAS_SUCCESS;
}

/**
 * Parse through the file looking for the following line:
 * 
 * 	<setting name="WebServiceUri" value="http://127.0.0.1:12345/simias10/username"/>
 * 
 * Return a strdup of the URL inside "value" (the caller must free the char *
 * when finished with it).
 */
static char *
parse_local_service_url(FILE *file)
{
	long file_size;
	char *buffer;
	char *setting_idx;
	char *value_idx;
	char *start_quote_idx;
	char *uri;
	int b_uri_found;
	
	b_uri_found = 0;
	
	/* Determine the file size */
	fseek(file, 0, SEEK_END);
	file_size = ftell(file);
	rewind(file);
	
	/* Allocate memory to suck in the whole file into the buffer */
	buffer = (char *) malloc(file_size);
	if (!buffer) {
		SIMIAS_DEBUG((stderr, "Couldn't allocate memory to read Simias.config\n"));
		return NULL;
	}
	
	/* Read the contents of the file into the buffer */
	fread(buffer, 1, file_size, file);
	
	/* Now parse for the URL */
	/* Look for "WebServiceUri" */
	setting_idx = strstr(buffer, "WebServiceUri");
	if (setting_idx) {
		value_idx = strstr(setting_idx, "value");
		if (value_idx) {
			start_quote_idx = strstr(value_idx, "\"");
			if (start_quote_idx) {
				uri = strtok(start_quote_idx + 1, "\"");
				if (uri) {
					b_uri_found = 1;
				}
			}
		}
	}
	
	/* Free up buffer memory */
	free(buffer);
	
	if (!b_uri_found) {
		return NULL;
	}

	return strdup(uri);
}

/******************************************************************************
 * gSOAP Wrappers for WebService Calls                                        *
 ******************************************************************************/
/* Wrapper for GetDomains */
int
simias_get_domains(bool only_slaves, SimiasDomainInfo **ret_domainsA[])
{
	char *soap_url;
	struct soap soap;
	struct _ns1__GetDomains req;
	struct _ns1__GetDomainsResponse resp;
	SimiasDomainInfo **domainInfosA;
	SimiasDomainInfo *domain;
	int num_of_domains = 0;
	int i = 0;
	struct ns1__ArrayOfDomainInformation *array_of_domain_infos;
	struct ns1__DomainInformation **domainsA;
	char username[512];
	char password[1024];
	
	soap_url = get_soap_url(true);
	if (!soap_url) {
		return -1;
	}

	/* Setup the Request */
	req.onlySlaves = only_slaves ? _true_ : _false_;
	
	init_gsoap(&soap);
	if (simias_get_web_service_credential(username, password) == SIMIAS_SUCCESS) {
		soap.userid = username;
		soap.passwd = DerivePassword(password);
	}
	soap_call___ns1__GetDomains(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		cleanup_gsoap(&soap);
		return SIMIAS_ERROR_IN_SOAP_CALL;
	}

	/* Allocate memory to return the domain information in */
	array_of_domain_infos = resp.GetDomainsResult;
	if (array_of_domain_infos) {
		num_of_domains = array_of_domain_infos->__sizeDomainInformation;
		if (num_of_domains >= 0) {
			domainInfosA = malloc(sizeof(SimiasDomainInfo *)
									 * (num_of_domains + 1));
			if (!domainInfosA) {
				/* Out of Memory error */
				cleanup_gsoap(&soap);
				return SIMIAS_ERROR_OUT_OF_MEMORY;
			}
			
			domainsA = array_of_domain_infos->DomainInformation;
			
			/* Populate the memory */
			for (i = 0; i < num_of_domains; i++) {
				/* Malloc a new SimiasDomainInfo */
				domain = malloc(sizeof(SimiasDomainInfo));
				if (!domain) {
					/* Out of Memory Error */
					return SIMIAS_ERROR_OUT_OF_MEMORY;
				}
				/* Type */
				switch (domainsA[i]->Type) {
					case ns1__DomainType__Master:
						domain->type = SIMIAS_DOMAIN_TYPE_MASTER;
						break;
					case ns1__DomainType__Slave:
						domain->type = SIMIAS_DOMAIN_TYPE_SLAVE;
						break;
					case ns1__DomainType__Local:
						domain->type = SIMIAS_DOMAIN_TYPE_LOCAL;
						break;
					case ns1__DomainType__None:
					default:
						domain->type = SIMIAS_DOMAIN_TYPE_NONE;
				}
				
				/* Active */
				if (domainsA[i]->Active == _true_) {
					domain->active = true;
				} else {
					domain->active = false;
				}

				/* Name */
				if (domainsA[i]->Name)
					domain->name = strdup(domainsA[i]->Name);
				else
					domain->name = strdup("");
				
				/* Description */
				if (domainsA[i]->Description)
					domain->description = strdup(domainsA[i]->Description);
				else
					domain->description = strdup("");

				/* ID */
				if (domainsA[i]->ID)
					domain->id = strdup(domainsA[i]->ID);
				else
					domain->id = strdup("");

				/* MemberUserID */
				if (domainsA[i]->MemberUserID)
					domain->member_user_id = strdup(domainsA[i]->MemberUserID);
				else
					domain->member_user_id = strdup("");

				/* MemberName */
				if (domainsA[i]->MemberName)
					domain->member_name = strdup(domainsA[i]->MemberName);
				else
					domain->member_name = strdup("");

				/* RemoteUrl */
				if (domainsA[i]->RemoteUrl)
					domain->remote_url = strdup(domainsA[i]->RemoteUrl);
				else
					domain->remote_url = strdup("");

				/* POBoxID */
				if (domainsA[i]->POBoxID)
					domain->po_box_id = strdup(domainsA[i]->POBoxID);
				else
					domain->po_box_id = strdup("");

				/* Host */
				if (domainsA[i]->Host)
					domain->host = strdup(domainsA[i]->Host);
				else
					domain->host = strdup("");
				
				/* IsSlave */
				if (domainsA[i]->IsSlave == _true_) {
					domain->is_slave = true;
				} else {
					domain->is_slave = false;
				}
				
				/* IsDefault */
				if (domainsA[i]->IsDefault == _true_) {
					domain->is_default = true;
				} else {
					domain->is_default = false;
				}
				
				/* Add this to the Array */
				domainInfosA[i] = domain;
			}

			/* NULL-terminate domainInfosA */
			domainInfosA[i] = 0x0;
			
			*ret_domainsA = domainInfosA;
		}
	} else {
		printf("array_of_domain_infos is NULL\n");
	}
	
	cleanup_gsoap(&soap);
	
	return SIMIAS_SUCCESS;
}

int
simias_free_domains(SimiasDomainInfo **domainsA[])
{
	SimiasDomainInfo *curr_domain;
	int i = 0;
	
	if (!*domainsA) {
		return SIMIAS_ERROR_UNKNOWN;
	}
	
	curr_domain = (*domainsA)[i];
	while (curr_domain) {
		/* First free all the char *'s */
		free(curr_domain->name);
		free(curr_domain->description);
		free(curr_domain->id);
		free(curr_domain->member_user_id);
		free(curr_domain->member_name);
		free(curr_domain->remote_url);
		free(curr_domain->po_box_id);
		free(curr_domain->host);
		free(curr_domain);
		
		curr_domain = (*domainsA)[++i];
	}
	
	free(*domainsA);

	return SIMIAS_SUCCESS;
}


/**
 * gSOAP
 */
static void
init_gsoap (struct soap *p_soap)
{
	/* Initialize gSOAP */
	soap_init (p_soap);
	soap_set_namespaces (p_soap, simias_namespaces);
}

static void
cleanup_gsoap (struct soap *p_soap)
{
	/* Cleanup gSOAP */
	soap_end (p_soap);
}

static char *
get_soap_url(bool reread_config)
{
	char *url;
	char simias_domain_url[512];
	int err;
	
	if (!reread_config && the_soap_url) {
		return the_soap_url;
	}

	err = simias_get_local_service_url(&url);
	if (err == SIMIAS_SUCCESS) {
		sprintf(simias_domain_url, "%s/Simias.asmx", url);
		free(url);
		if (the_soap_url)
			free(the_soap_url);
		the_soap_url = strdup(simias_domain_url);
		/* FIXME: Figure out who and when this should ever be freed */
	} else {
		printf("simias_get_local_service_url() returned: %d\n", err);
	}
	
	return the_soap_url;
}
