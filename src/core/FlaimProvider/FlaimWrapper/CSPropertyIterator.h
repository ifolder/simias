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
*                 $Author: Russ Young
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*
*
*******************************************************************************/
#ifndef _CSPropertyIterator_H_
#define _CSPropertyIterator_H_

#include "CSPStore.h"

class CSPPropertyIterator
{
public:
	CSPPropertyIterator(CSPStoreObject *pObject);
	virtual ~CSPPropertyIterator(void);
	CSPValue *Next();


	CSPStoreObject	*m_pObject;
	void			*m_pvField;
};

#endif // _CSPropertyIterator_H_
