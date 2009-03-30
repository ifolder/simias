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
#include "CSPropertyIterator.h"

CSPPropertyIterator::CSPPropertyIterator(CSPStoreObject *pObject) :
	m_pObject(pObject),
	m_pvField(0)
{
	// Skip Level 0 and the first three properties Name, Id, Type.
	m_pvField = m_pObject->m_pRec->root();
	m_pvField = m_pObject->m_pRec->next(m_pvField);
	m_pvField = m_pObject->m_pRec->nextSibling(m_pvField);
	m_pvField = m_pObject->m_pRec->nextSibling(m_pvField);
	m_pvField = m_pObject->m_pRec->nextSibling(m_pvField);
} // CSPPropertyIterator::CSPPropertyIterator()

CSPPropertyIterator::~CSPPropertyIterator(void)
{
} // CSPPropertyIterator::~CSPPropertyIterator()

CSPValue *CSPPropertyIterator::Next() 
{
	CSPValue *pValue = 0;

	if (m_pvField != 0)
	{
		pValue = m_pObject->GetProperty(m_pvField);
		m_pvField = m_pObject->m_pRec->nextSibling(m_pvField);
	}

	return (pValue);
} // CSPPropertyIterator::Next()
