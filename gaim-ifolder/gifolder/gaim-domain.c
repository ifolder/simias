/***********************************************************************
 *  $RCSfile$
 *
 *  Gaim iFolder Plugin: Allows Gaim users to share iFolders.
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Some code in this file (mostly the saving and reading of the XML files) is
 *  directly based on code found in Gaim's core & plugin files, which is
 *  distributed under the GPL.
 ***********************************************************************/

#include "gaim-domain.h"
#include <simias.h>

/* Gaim Includes */
#include "account.h"
#include "blist.h"

#include <simiasgaimStub.h>
#include <simiasgaim.nsmap>

static char *the_soap_url = NULL;

/* Forward Declarations of Static Functions */
static void init_gsoap (struct soap *p_soap);
static void cleanup_gsoap (struct soap *p_soap);
static char *get_soap_url(gboolean reread_config);

/* Function Implementation */
/**
 * The purpose of this function is to have the Gaim iFolder Plugin call the
 * GaimDomainService (WebService) to wake it up and have it synchronize the
 * memberlist.  This will be called either when the plugin gets a new/updated
 * PO Box URL for a buddy that just signed on or...when the user presses the
 * "Synchronize Now" button from the preferences page.
 */
void
simias_sync_member_list()
{
	char *soap_url;
	struct soap soap;
	struct _ns1__SynchronizeMemberList req;
	struct _ns1__SynchronizeMemberListResponse resp;
	
	soap_url = get_soap_url(FALSE);
	if (!soap_url) {
		return;
	}
	
	init_gsoap(&soap);
	soap_call___ns1__SynchronizeMemberList(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		cleanup_gsoap(&soap);
		return;
	}
	
	cleanup_gsoap(&soap);
}

void
simias_update_member(const char *account_name, const char *account_prpl_id,
					 const char *buddy_name)
{
	char *soap_url;
	struct soap soap;
	struct _ns1__UpdateMember req;
	struct _ns1__UpdateMemberResponse resp;
	
	soap_url = get_soap_url(FALSE);
	if (!soap_url) {
		return;
	}
	
	/* Setup the Request */
	req.AccountName = (char *)account_name;
	req.AccountProtocolID = (char *)account_prpl_id;
	req.BuddyName = (char *)buddy_name;
	
	init_gsoap(&soap);
	soap_call___ns1__UpdateMember(&soap, soap_url, NULL, &req, &resp);
	if (soap.error) {
		cleanup_gsoap(&soap);
		return;
	}
	
	cleanup_gsoap(&soap);
}

/* Utility functions for gSOAP */
static char *
get_soap_url(gboolean reread_config)
{
	char *url;
	char gaim_domain_url[512];
	int err;
	
	if (!reread_config && the_soap_url) {
		return the_soap_url;
	}

	err = simias_get_local_service_url(&url);
	if (err == SIMIAS_SUCCESS) {
		sprintf(gaim_domain_url, "%s/GaimDomainService.asmx", url);
		free(url);
		the_soap_url = strdup(gaim_domain_url);
		/* FIXME: Figure out who and when this should ever be freed */
	}
	
	return NULL;
}

/**
 * gSOAP
 */
static void
init_gsoap (struct soap *p_soap)
{
	/* Initialize gSOAP */
	soap_init (p_soap);
	soap_set_namespaces (p_soap, simiasgaim_namespaces);
}

static void
cleanup_gsoap (struct soap *p_soap)
{
	/* Cleanup gSOAP */
	soap_end (p_soap);
}
