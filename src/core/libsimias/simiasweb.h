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

#ifndef _SIMIASWEB_H
#define _SIMIASWEB_H 1

#include <stdbool.h>

#include "simias.h"

#ifndef _SIMIAS_DOMAIN_TYPE
#define _SIMIAS_DOMAIN_TYPE
typedef enum
{
  SIMIAS_DOMAIN_TYPE_MASTER = 0,
  SIMIAS_DOMAIN_TYPE_SLAVE  = 1,
  SIMIAS_DOMAIN_TYPE_LOCAL  = 2,
  SIMIAS_DOMAIN_TYPE_NONE   = 3
} SIMIAS_DOMAIN_TYPE;
#endif
typedef enum {_false_, _true_} xsd__boolean;

/**************************************************************************/
/* Data Structures                                                        */
/**************************************************************************/

typedef struct _SimiasDomainInfo SimiasDomainInfo;
struct _SimiasDomainInfo {
	SIMIAS_DOMAIN_TYPE	type;
    bool				active;
	char *				name;
	char *				description;
	char *				id;
	char *				member_user_id;
	char *				member_name;
	char *				remote_url;
	char *				po_box_id;
	char *				host;
	bool				is_slave;
	bool				is_default;
};


/**
 * The following methods wrapper the gSOAP calls (WebService).  Get any
 * documentation about the real call by referencing the documentation elsewhere.
 */

/**
 * Wrapper for GetDomains
 *
 * When this call is successful, it fills out a NULL-terminated array of
 * SimiasDomainInfo.  Callers must call simias_free_domains() on the returned
 * array.
 */
int simias_get_domains(bool only_slaves, SimiasDomainInfo **ret_domainsA[]);

/**
 * Free an array of SimiasDomainInfo
 */
int simias_free_domains(SimiasDomainInfo **domainsA[]);

#endif
