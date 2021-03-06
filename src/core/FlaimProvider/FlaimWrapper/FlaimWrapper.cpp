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
*		Defines the entry point for the DLL application
*
*******************************************************************************/

#include "FlaimWrapper.h"
#include "stdio.h"
#include "CSPStore.h"

#ifdef WIN32
BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
    switch (ul_reason_for_call)
	{
		case DLL_PROCESS_ATTACH:
		case DLL_THREAD_ATTACH:
		case DLL_THREAD_DETACH:
		case DLL_PROCESS_DETACH:
			break;
    }
    return TRUE;
}
#else
#ifdef UNIX
#endif
#endif


// This is an example of an exported variable
FLAIMWRAPPER_API int nFlaimWrapper=0;

// This is an example of an exported function.
FLAIMWRAPPER_API int fnFlaimWrapper(void)
{
	return 42;
}

extern "C"
{
FLAIMWRAPPER_API int FWCreateStore(char *pStorePath, PCSStore *ppStore, CSPDB **ppDB)
{
	return (CSPStore::_CREATE(pStorePath, ppStore, ppDB));
}

FLAIMWRAPPER_API int FWDeleteStore(FLMBYTE* dbPath)
{
	return (CSPStore::DeleteStore(dbPath));
}

FLAIMWRAPPER_API void FWCloseStore(PCSStore pStore)
{
	pStore->Close();
}


FLAIMWRAPPER_API int FWOpenStore(char *pStorePath, PCSStore *ppStore, CSPDB **ppDB)
{
	return (CSPStore::_OPEN(pStorePath, ppStore, ppDB));
}

FLAIMWRAPPER_API int FWBeginTrans(PCSStore pStore)
{
	return (pStore->BeginTrans());
} // CSPStore::BeginTrans()


FLAIMWRAPPER_API void FWAbortTrans(PCSStore pStore)
{
	pStore->AbortTrans();
} // CSPStore::AbortTrans()


FLAIMWRAPPER_API int FWEndTrans(PCSStore pStore)
{
	return (pStore->EndTrans());
} // CSPStore::EndTrans()


FLAIMWRAPPER_API int FWCreateObject(PCSStore pStore, FLMUNICODE *pName, FLMUNICODE *pId, FLMUNICODE *pType, FLMBOOL *pNewObject, FLMINT flmId, CSPStoreObject **ppObject)
{
	CSPStoreObject * pObject = pStore->CreateObject(pName, pId, pType, pNewObject, flmId);
	*ppObject = pObject;

	return (pObject ? 0 : -1);
}

FLAIMWRAPPER_API int FWCloseObject(CSPStoreObject *pObject, bool abort)
{
	int rc = 0;

	if (abort)
	{
		// Abort the changes.
		pObject->Abort();
	}
	else
	{
		rc = pObject->Flush();
	}
	delete pObject;

	return rc;
}

FLAIMWRAPPER_API int FWDeleteObject(CSPStore *pStore, FLMUNICODE *pId, int *pFlmId)
{
	return (pStore->DeleteObject(pId, pFlmId));
}

FLAIMWRAPPER_API int FWGetObject(CSPStore *pStore, FLMUNICODE *pId, int *pLength, FLMUNICODE *pBuffer)
{
	return (pStore->GetObject(CS_Name_GUID ,pId, pLength, pBuffer));
}

FLAIMWRAPPER_API int FWSetProperties(CSPStoreObject *pObject, FLMUNICODE *pProperties)
{
	return (pObject->SetProperties(pProperties));
}

FLAIMWRAPPER_API int FWSetProperty(CSPStoreObject *pObject, FLMUNICODE *pName, FLMUNICODE *pType, FLMUNICODE *pValue, FLMUNICODE *pFlags)
{
	return (pObject->SetProperty(pName, pType, pValue, pFlags));
}

FLAIMWRAPPER_API int FWDefineField(CSPStore *pStore, FLMUNICODE *pName, FLMUNICODE *pType, int index)
{
	RCODE	rc = FERR_OK;
	FLMINT	type;
	FLMUINT	fieldId = 0;
	type = pStore->StringToType(pType);
	if (type != CSP_Type_Undefined)
	{
		rc = pStore->RegisterField(pName, type, &fieldId);
		if (RC_OK(rc) && index != 0)
		{
			rc = pStore->AddIndex(pName, fieldId);
		}
	}
	return rc;
}

FLAIMWRAPPER_API int FWSearch(CSPStore *pStore, FLMUNICODE *pCollectionId, FLMUNICODE *pName, FLMINT op, FLMUNICODE *pValue, FLMUNICODE *pType, FLMBOOL caseSensitive, FLMUINT* pCount, CSPObjectIterator **ppResults)
{
	return (pStore->Search(pCollectionId, pName, op, pValue, pType, caseSensitive, pCount, ppResults));
}

FLAIMWRAPPER_API int FWMQSearch(CSPStore *pStore, FLMUNICODE *pCollectionId, FLMUNICODE *pName, FLMINT op, FLMUNICODE *pValue, FLMUNICODE *pType, FLMUNICODE *pName1, FLMINT op1, FLMUNICODE *pValue1, FLMUNICODE *pType1, FLMUNICODE *pName2, FLMINT op2, FLMUNICODE *pValue2, FLMUNICODE *pType2, FLMUNICODE *pName3, FLMINT op3, FLMUNICODE *pValue3, FLMUNICODE *pType3, FLMINT QueryCount, FLMBOOL caseSensitive, FLMUINT* pCount, CSPObjectIterator **ppResults)
{
	return (pStore->MQSearch(pCollectionId, pName, op, pValue, pType, pName1, op1, pValue1, pType1, pName2, op2, pValue2, pType2, pName3, op3, pValue3, pType3 ,QueryCount, caseSensitive, pCount, ppResults));
}

FLAIMWRAPPER_API void FWCloseSearch(CSPObjectIterator *pResults)
{
	if (pResults != 0)
	{
		delete pResults;
		pResults = 0;
	}
}

FLAIMWRAPPER_API int FWGetNextObjectList(CSPObjectIterator *pResults, CSPStore *pStore, FLMUNICODE * pBuffer, int nChars)
{
	return (pResults->NextXml(pStore, pBuffer, nChars));
}

FLAIMWRAPPER_API bool FWSetListIndex(CSPObjectIterator *pResults, int origin, int offset)
{
	return (pResults->SetIndex((IndexOrigin)origin, offset));
}


FLAIMWRAPPER_API void FWNOP()
{
}

} // extern 'C'

// This is the constructor of a class that has been exported.
// see FlaimWrapper.h for the class definition
CFlaimWrapper::CFlaimWrapper()
{ 
	return; 
}

