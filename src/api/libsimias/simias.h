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
#ifndef _SIMIAS_H
#define _SIMIAS_H

#include <stdbool.h>

#define SIMIAS_SUCCESS 0
#define SIMIAS_ERROR_UNKNOWN				-1

typedef void *SimiasHandle;

int simias_handle_init_local(SimiasHandle *handle);
int simias_handle_delete(SimiasHandle *handle);

int simias_ping(SimiasHandle handle);

#endif	// _SIMIAS_H
