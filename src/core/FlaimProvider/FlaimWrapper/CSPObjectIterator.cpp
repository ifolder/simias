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
*        <Description of the functionality of the file >
*
*
*******************************************************************************/
#include "CSPObjectIterator.h"

CSPObjectIterator::CSPObjectIterator(HFCURSOR cursor, int count, FLMBOOL includeColId) :
	m_Count(count),
	m_Index(0),
	m_pRecords(0),
	m_includeColId(includeColId)
{
	if (m_Count)
	{
		RCODE rc;
		m_pRecords = new FLMUINT[m_Count];
		if (m_pRecords)
		{
			int i;
			for (i = 0; i < count; ++i)
			{
				rc = FlmCursorNextDRN(cursor, &m_pRecords[i]);
				if (RC_BAD(rc))
				{
					m_Count = 0;
					break;
				}
			}
		}
	}
}

CSPObjectIterator::~CSPObjectIterator(void)
{
	if (m_pRecords)
	{
		delete [] m_pRecords;
	}
}

int CSPObjectIterator::NextXml(CSPStore *pStore, FLMUNICODE *pOriginalBuffer, int nChars)
{
	RCODE rc = FERR_OK;
	int charsWritten = nChars;
	int len = 0;
	FlmRecord	*pRec = 0;
	FLMUNICODE* pBuffer = pOriginalBuffer;
	int endTagLen = f_unilen((FLMUNICODE*)XmlObjectListEndString) + 1;

	if (m_Index < m_Count)
	{
		if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlObectListString, nChars)) != -1)
		{
			nChars -= len + endTagLen;
			pBuffer += len;
			
			while (RC_OK(rc) && m_Index < m_Count)
			{
				rc = FlmRecordRetrieve(pStore->GetDB(), FLM_DATA_CONTAINER, m_pRecords[m_Index], FO_EXACT, &pRec, 0);
				if (RC_OK(rc) && pRec)
				{
					CSPStoreObject *pObject = new CSPStoreObject(pStore, pRec);
					if (pObject)
					{
						if ((len = pObject->ToXML(pBuffer, nChars, false, m_includeColId)) != -1)
						{
							nChars -= len;
							pBuffer += len;
							m_Index++;
						}
						else
						{
							rc = FERR_MEM;
						}
						delete pObject;
						pRec = 0;
					}
				}
				else if (rc == FERR_NOT_FOUND)
				{
					m_Index++;
					rc = FERR_OK;
				}
				else if (RC_BAD(rc))
				{
					m_Index++;
					rc = FERR_OK;
				}
			}
			if ((len = flmstrcpy(pBuffer, (FLMUNICODE*)XmlObjectListEndString, nChars + endTagLen)) != -1)
			{
				nChars++;
			}
		}
	}
	return (len != -1 ? charsWritten - nChars : 0);
}

bool CSPObjectIterator::SetIndex(IndexOrigin origin, int offset)
{
	int newOffset = -1;
	switch (origin)
	{
	case CUR:
		newOffset += offset;
		break;
	case END:
		newOffset = m_Count + offset;
		break;
	case SET:
		newOffset = offset;
		break;
	}

	if (newOffset <= m_Count && newOffset >= 0)
	{
			m_Index = newOffset;
			return true;
	}
	else
		return false;
}



