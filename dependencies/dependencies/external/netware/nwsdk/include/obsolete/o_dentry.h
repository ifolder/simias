/******************************************************************************

  %name: %
  %version: %
  %date_modified: Fri Nov 20 09:55:30 1998 %
  $Copyright:

  Copyright (c) 1999 Novell, Inc.  All Rights Reserved.

  THIS WORK IS  SUBJECT  TO  U.S.  AND  INTERNATIONAL  COPYRIGHT  LAWS  AND
  TREATIES.   NO  PART  OF  THIS  WORK MAY BE  USED,  PRACTICED,  PERFORMED
  COPIED, DISTRIBUTED, REVISED, MODIFIED, TRANSLATED,  ABRIDGED, CONDENSED,
  EXPANDED,  COLLECTED,  COMPILED,  LINKED,  RECAST, TRANSFORMED OR ADAPTED
  WITHOUT THE PRIOR WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION
  OF THIS WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO
  CRIMINAL AND CIVIL LIABILITY.$

 *****************************************************************************/


/*
 * This file contains prototypes for library function calls that are being
 * deprecated. They are preserved here in the interest of all legacy software 
 * that depends on them. Please update such software to use the preferred API 
 * calls. 
 *
 * DO NOT INCLUDE THIS HEADER EXPLICITLY!!
 *
 * Include "nwconnec.h" and use a compiler switch to define INCLUDE_OBSOLETE
 * (i.e. -DINCLUDE_OBSOLETE).
 */


#ifndef _OBSOLETE_NWDENTRY_H
#define _OBSOLETE_NWDENTRY_H

#define NWScanDirEntryInfo(a, b, c, d, e, f) \
        NWIntScanDirEntryInfo(a, b, c, d, e, f, 0)

#define NWScanExtendedInfo(a, b, c, d, e, f) \
        NWIntScanExtendedInfo(a, b, c, d, e, f, 0)

#define NWMoveDirEntry(a, b, c, d, e, f) \
        NWIntMoveDirEntry(a, b, c, d, e, f, 0)

#endif
