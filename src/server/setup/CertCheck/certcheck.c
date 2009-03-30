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
*                 $Author: Calvin Gaisford (cgaisford)
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.1
*-----------------------------------------------------------------------------
* This module is used to:
*        < utility to get LDAP certificates >
*
*
*******************************************************************************/


//===[  Platform header files    ]==============================
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ldap.h>
#include <ldap_ssl.h>
#include <unistd.h>

//===[  Local Defines            ]==============================
#define MAX_DN_LEN 1024

//===[  Prototypes               ]==============================
LDAP *ldapBind (char *ldapServer, int ldapPort, char *ldapUser,
		char *ldapPasswd, char *ldapCACertPath);
LDAP *ldapUnBind (LDAP * ld);
int LIBCALL cert_callback (void *pHandle);

//===[  Global Variables         ]==============================
LDAPSSL_Cert gLDAPCert;
int gHaveCACert;
int gExtractCert;
char gLdapCACertPath[2048];






int main (int argc, char **argv)
{
	char ldapServerDNSorIP[MAX_DN_LEN];
	int ldapServerPort = 389;
	char ldapServerPortString[12];
	char ldapAdminDN[MAX_DN_LEN];
	char ldapAdminPasswd[MAX_DN_LEN];
	LDAP *pLDAP;

	if (argc < 7)
	{
		printf("Usage: certcheck <host> <port> <dn> <passwd> < get | use > <CA cert path>\n");
		return -1;
	}

	gLDAPCert.data = NULL;
	gLDAPCert.length = 0;
	gHaveCACert = 0;
	gExtractCert = 0;

	strncpy (ldapServerDNSorIP, argv[1], MAX_DN_LEN);
	strncpy (ldapServerPortString, argv[2], 12);
	ldapServerPort = atoi (ldapServerPortString);

	strncpy (ldapAdminDN, argv[3], MAX_DN_LEN);
	strncpy (ldapAdminPasswd, argv[4], MAX_DN_LEN);
	strncpy (gLdapCACertPath, argv[6], 2048);
	if (strcmp (argv[5], "get") == 0)
	{
		gExtractCert = 1;
	}

//	printf ("LDAP Server      : %s\n", ldapServerDNSorIP);
//	printf ("LDAP Port        : %d\n", ldapServerPort);
//	printf ("LDAP DN          : %s\n", ldapAdminDN);
//	printf ("LDAP passwd      : %s\n", ldapAdminPasswd);
//	printf ("CA Cert Path     : %s\n", gLdapCACertPath);

	pLDAP=ldapBind (ldapServerDNSorIP, ldapServerPort, ldapAdminDN, ldapAdminPasswd, gLdapCACertPath);
	if (pLDAP == NULL)
	{
		printf ("Unable to bind to LDAP!\n");
		return -1;
	}
//	printf ("Successfully called ldapBind\n");

	pLDAP = ldapUnBind (pLDAP);
	
	return(0);
}



LDAP* ldapBind (char *ldapServer, int ldapPort, char *ldapUser, 
				char *ldapPasswd, char *ldapCACertPath)
{
	struct timeval timeOut = { 20, 0 };
	LDAP *pLDAP = NULL;
	int ldapVersion = LDAP_VERSION3;
	int rc;

	ldap_set_option (NULL, LDAP_OPT_PROTOCOL_VERSION, &ldapVersion);
	ldap_set_option (NULL, LDAP_OPT_NETWORK_TIMEOUT, &timeOut);

	if (!gExtractCert)
	{
		rc = ldapssl_client_init(ldapCACertPath, NULL);
		if(rc != LDAP_SUCCESS)
		{
			fprintf(stderr, "Unable to initialize client ldap ssl\n");
			return NULL;
		}

		rc = ldapssl_set_verify_callback(cert_callback);
		if (rc != LDAP_SUCCESS)
		{
			fprintf(stderr, "ldapssl_set_verify_callback error: %d\n", rc);
			ldapssl_client_deinit();
			return NULL;
		}
	}
	else
	{
		rc = ldapssl_client_init (NULL, NULL);
		if (rc != LDAP_SUCCESS)
		{
			fprintf (stderr, "Unable to initialize client ldap ssl\n");
			return NULL;
		}

		rc = ldapssl_set_verify_callback (cert_callback);
		if (rc != LDAP_SUCCESS)
		{
			fprintf (stderr, "ldapssl_set_verify_callback error: %d\n", rc);
			ldapssl_client_deinit ();
			return NULL;
		}
	}

	pLDAP = ldapssl_init (ldapServer, ldapPort, 1);
	if (pLDAP == NULL)
		return pLDAP;

	rc = ldap_simple_bind_s (pLDAP, ldapUser, ldapPasswd);
	if (rc != LDAP_SUCCESS)
	{
		fprintf (stderr, "Unable to perform simple bind, err: 0x%02X\n", rc);
		pLDAP = NULL;
	}

	return pLDAP;
}



LDAP* ldapUnBind (LDAP * ld)
{
	if (ld != NULL)
	{
		ldap_unbind (ld);
	}

	ldapssl_client_deinit ();

	if (NULL != gLDAPCert.data)
		free (gLDAPCert.data);
	return NULL;
}


int LIBCALL cert_callback (void *pHandle)
{
	int rc = 0;
	int callbackRc = LDAPSSL_CERT_REJECT;
	FILE *certFile = NULL;
	int length = 0;
	int certStatus = 0;

//	printf ("The cert_callback is being called\n");

	if (gHaveCACert == 1)
		return LDAPSSL_CERT_ACCEPT;

	length = sizeof (certStatus);
	rc = ldapssl_get_cert_attribute (pHandle,	/* cert Handle */
			LDAPSSL_CERT_GET_STATUS,			/* desired attribute */
			&certStatus,						/* attribute value */
			&length);							/* length */

	if (LDAPSSL_SUCCESS != rc)
	{
		fprintf (stderr, "ldapssl_get_cert_attribute LDAPSSL_CERT_GET_STATUS failed\n");
		goto err;
	}

	switch (certStatus)
	{
		case UNABLE_TO_GET_ISSUER_CERT:
			fprintf (stderr, "CERTCALLBACK: unable to get issuer certificate\n");
			goto err;
			break;
		case UNABLE_TO_DECODE_ISSUER_PUBLIC_KEY:
			fprintf (stderr, "CERTCALLBACK: unable to decode issuer public key\n");
			goto err;
			break;
		case CERT_SIGNATURE_FAILURE:
			fprintf (stderr, "CERTCALLBACK: certificate signature failure\n");
			goto err;
			break;
		case CERT_NOT_YET_VALID:
			fprintf (stderr, "CERTCALLBACK: certificate is not yet valid\n");
			goto err;
			break;
		case CERT_HAS_EXPIRED:
			fprintf (stderr, "CERTCALLBACK: Certificate has expired\n");
			goto err;
			break;
		case ERROR_IN_CERT_NOT_BEFORE_FIELD:
			fprintf (stderr, "CERTCALLBACK: format error in certificate's notBefore field\n");
			goto err;
			break;
		case ERROR_IN_CERT_NOT_AFTER_FIELD:
			fprintf (stderr, "CERTCALLBACK: format error in certificate's notAfter field\n");
			goto err;
			break;
		case DEPTH_ZERO_SELF_SIGNED_CERT:
			if (!gExtractCert)
			{
				// We only want to error on this if we are not
				// extracting the CA Cert
				fprintf (stderr, "CERTCALLBACK: self signed certificate\n");
				goto err;
			}
			break;
		case SELF_SIGNED_CERT_IN_CHAIN:
			if (!gExtractCert)
			{
				// We only want to error on this if we are not
				// extracting the CA Cert
				fprintf (stderr, "CERTCALLBACK: self signed certificate in certificate chain\n");
				goto err;
			}
			break;
		case UNABLE_TO_GET_ISSUER_CERT_LOCALLY:
			fprintf (stderr, "CERTCALLBACK: unable to get local issuer certificate\n");
			goto err;
			break;
		case UNABLE_TO_VERIFY_LEAF_SIGNATURE:
			fprintf (stderr, "CERTCALLBACK: unable to verify the first certificate\n");
			goto err;
			break;
		case INVALID_CA:
			fprintf (stderr, "CERTCALLBACK: invalid CA certificate\n");
			goto err;
			break;
		case PATH_LENGTH_EXCEEDED:
			fprintf (stderr, "CERTCALLBACK: path length constraint exceeded\n");
			goto err;
			break;
		case INVALID_PURPOSE:
			fprintf (stderr, "CERTCALLBACK: unsupported certificate purpose\n");
			goto err;
			break;
		case CERT_UNTRUSTED:
			fprintf (stderr, "CERTCALLBACK: certificate not trusted\n");
			goto err;
			break;
		case CERT_REJECTED:
			fprintf (stderr, "CERTCALLBACK: certificate rejected\n");
			goto err;
			break;
		default:
			fprintf (stderr, "CERTCALLBACK: Status returned %d\n", certStatus);
			// don't goto err here, just print the status
			break;
	}


	/* ldapssl_get_cert allows an application to get the certificate in 
	 * a buffer encoded in Base64 or DER format -  Applications could
	 * then save the certificate buffer to persistant storage (file, 
	 * database, etc ).
	 */

	/* Query the length of the buffer needed by first calling 
	 * ldapssl_get_cert with the data field set to NULL (ie cert.data=NULL).
	 * Upon return the length field is set to the required size.
	 */
	gLDAPCert.data = NULL;
	gLDAPCert.length = 0;
	rc = ldapssl_get_cert (pHandle,		/* cert handle */
			LDAPSSL_CERT_BUFFTYPE_DER,	/* desired encoding */
			&gLDAPCert);				/* certificate structure */

	if (LDAPSSL_SUCCESS != rc)
	{
		fprintf (stderr, "ldapssl_get_cert length failed\n");
		goto err;
	}

	/* Allocate necessary memory */
	gLDAPCert.data = (void *) malloc (gLDAPCert.length);
	if (NULL == gLDAPCert.data)
	{
		fprintf (stderr, "Could not allocate buffer!\n");
		goto err;
	}

	/* Retrieve the certificate DER Encoded */
	rc = ldapssl_get_cert (pHandle,		/* cert handle */
			LDAPSSL_CERT_BUFFTYPE_DER,	/* desired encoding */
			&gLDAPCert);				/* certificate structure */
	if (LDAPSSL_SUCCESS != rc)
	{
		fprintf (stderr, "ldapssl_get_cert failed\n");
		goto err;
	}

	rc = ldapssl_add_trusted_cert (&gLDAPCert, LDAPSSL_CERT_BUFFTYPE_DER);
	if (LDAPSSL_SUCCESS != rc)
	{
		fprintf (stderr, "ldapssl_add_trusted_cert failed\n");
		goto err;
	}

	callbackRc = LDAPSSL_CERT_ACCEPT;
	// In this code, we only want the CA because we are extracting
	// it so only get the CA cert once!
	gHaveCACert = 1;
	if (gExtractCert)
	{
		certFile = fopen (gLdapCACertPath, "w+");
		if (certFile != NULL)
		{
			fwrite (gLDAPCert.data, gLDAPCert.length, 1, certFile);
			fclose (certFile);
		}
		else
		{
			fprintf (stderr, "The RootCert.der file could not be written to %s\n", gLdapCACertPath);
		}
	}
err:
	return (callbackRc);
}


//=======================================================================
//=======================================================================
// File CVS History:
//
// $Log$
// Revision 1.1  2004/12/14 00:07:59  bberg
// initial check-in of CertCheck
//
// Revision 1.4  2003/11/20 14:46:48  cgaisford
// cleaned up certcheck.c
//
// Revision 1.3  2003/11/20 03:55:09  cgaisford
// fixed up spacing on certcheck.c
//
// Revision 1.2  2003/11/20 03:53:47  cgaisford
// updated certcheck.c
//
// Revision 1.8  2003/11/06 15:39:06  aagrawal
// This file have been modified for the defect DEFECT000352139.
// Earlier install was not checking if the ifolder_server object
// with similar name is available or not. Now this check is enabled.
//
// Revision 1.7  2003/10/06 17:23:56  cgaisford
// updated to now install the root cert into the LDAP object when creating it
//
// Revision 1.6  2003/10/06 16:11:40  cgaisford
// updated ldiffer and main to grab the root cert and store it in /etc/opt/novell/ifolder/RootCert.der
//
// Revision 1.5  2003/09/30 22:21:35  cgaisford
// updated so the config doesn't output anything
//
// Revision 1.4  2003/09/23 14:17:53  cgaisford
// updated
//
// Revision 1.3  2003/09/23 04:52:02  cgaisford
// updated main to build on advanced server
//
// Revision 1.2  2003/09/23 04:15:34  cgaisford
// removed extra java code tagged on bottom of the file
//
// Revision 1.1  2003/09/23 04:12:48  cgaisford
// added the ldapsetup module to create ldap objects by default
//
//
//
//=======================================================================
