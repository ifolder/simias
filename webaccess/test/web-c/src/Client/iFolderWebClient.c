/***********************************************************************
 *  $RCSfile: iFolderWebClient.c,v $
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
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob
 *
 ***********************************************************************/

#include <stdio.h>
#include <string.h>
#include <ctype.h>
#include <time.h>

#include "iFolderWebAccessStub.h"
#include "iFolderWebAccessSoap.nsmap"

#define MAX_STRING      512
#define MAX_BUFFER      (64 * 1024)
#define MAX_COOKIES     10

void path_combine(char *, size_t, const char *, const char *);
void parse_ifolder(char *, size_t, const char *);
void get_parent(char *, size_t, const char *);
void get_entry(char *, size_t, const char *);

int main(int argc, char **argv)
{
    const char *https = "https";
    const char *default_host = "http://127.0.0.1";
    const char *soap_path = "/simias10/iFolderWebAccess.asmx";
    char url[MAX_STRING];

    char *username = "SimiasAdmin";
    char *password = "novell";

    int quit = 0;
    struct soap soap;

    char context[MAX_STRING] = "/";
    char path[MAX_STRING] = "/";

    printf("\niFoler Web Client - C\n\n");

    // connect
    soap_init(&soap);
    soap_set_namespaces(&soap, namespaces);

    // uri
    if (argc > 1)
    {
        strncpy(url, argv[1], MAX_STRING);
    }
    else
    {
        strncpy(url, default_host, MAX_STRING);
    }
    strncat(url, soap_path, MAX_STRING);
    printf("URL: %s\n", url);

    // ssl
    if (!strncmp(https, url, strlen(https)))
    {
        if (soap_ssl_client_context(&soap, SOAP_SSL_NO_AUTHENTICATION,
            NULL, NULL, NULL, NULL, NULL))
        {
            soap_print_fault(&soap, stderr);
            exit(1);
        }
    }

    // creditials
    if (argc > 3)
    {
        username = argv[2];
        password = argv[3];
    }

    soap.userid = username;
    soap.passwd = password;

    printf("User: %s\n\n", username);

    // cookies
    soap.cookie_max = MAX_COOKIES;

    while(!quit)
    {
        char response[MAX_STRING] = "";
        char command[MAX_STRING] = "";
        char argument[MAX_STRING] = "";
        char ifolder_name[MAX_STRING] = "";

        // clean-up
        soap_end(&soap);

        // prompt
        printf("\n[%s]\n", context);
        printf("ifolder> ");

        fgets(response, MAX_STRING, stdin);
        printf("\n");

        if (strlen(response) == 0)
        {
            continue;
        }

        // trim
        response[strlen(response) - 1] = 0;

        // command
        char *p = index(response, ' ');

        if (p == NULL)
        {
            strcpy(command, response);
        }
        else
        {
            *p++ = '\0';

            strcpy(command, response);
            strcpy(argument, p);
        }

        // update the path with the argument
        path_combine(path, MAX_STRING, context, argument);
        printf("Path: %s\n\n", path);

        // determine ifolder
        parse_ifolder(ifolder_name, MAX_STRING, path);
        struct ifolder__iFolder *ifolder = NULL;
        struct _ifolder__GetiFolderByNameResponse output;

        if (strlen(ifolder_name) > 0)
        {
            struct _ifolder__GetiFolderByName input;

            input.ifolderName = ifolder_name;

            soap_call___ifolder__GetiFolderByName(&soap, url, NULL, &input, &output);

            if (!soap.error) 
            {
                ifolder = output.GetiFolderByNameResult;
            }
        }

        // switch on the first letter of the command
        switch(tolower(command[0]))
        {
            // change context
            case 'c':
                {
                    if (ifolder != NULL)
                    {
                        struct _ifolder__GetEntryByPath input;
                        struct _ifolder__GetEntryByPathResponse output;

                        input.ifolderID = ifolder->ID;
                        input.entryPath = path;

                        soap_call___ifolder__GetEntryByPath(&soap, url, NULL, &input, &output);

                        if (soap.error) 
                        {
                            soap_print_fault(&soap, stderr);
                            continue;
                        }

                        struct ifolder__iFolderEntry *entry = output.GetEntryByPathResult;

                        if ((entry != NULL) && (entry->IsDirectory))
                        {
                            strcpy(context, path);
                        }
                        else
                        {
                            printf("Error: %s is not a directory\n\n", path);
                        }
                    }
                    else
                    {
                        // reset
                        strcpy(context, "/");
                    }
                }
                break;

            // download a file
            case 'd':
                {
                    struct _ifolder__GetEntryByPath input;
                    struct _ifolder__GetEntryByPathResponse output;

                    input.ifolderID = ifolder->ID;
                    input.entryPath = path;

                    soap_call___ifolder__GetEntryByPath(&soap, url, NULL, &input, &output);

                    if (soap.error) 
                    {
                        soap_print_fault(&soap, stderr);
                        continue;
                    }

                    struct ifolder__iFolderEntry *entry = output.GetEntryByPathResult;

                    // start
                    clock_t start = clock();

                    struct _ifolder__OpenFileRead input2;
                    struct _ifolder__OpenFileReadResponse output2;

                    input2.ifolderID = ifolder->ID;
                    input2.entryID = entry->ID;

                    soap_call___ifolder__OpenFileRead(&soap, url, NULL, &input2, &output2);

                    if (soap.error) 
                    {
                        soap_print_fault(&soap, stderr);
                        continue;
                    }

                    char *file_id = output2.OpenFileReadResult;

                    FILE *fp = fopen(entry->Name, "wb");

                    if (fp == NULL)
                    {
                        printf("Error: %s\n\n", strerror(errno));
                        continue;
                    }

                    while(1)
                    {
                        struct _ifolder__ReadFile input3;
                        struct _ifolder__ReadFileResponse output3;

                        input3.fileID = file_id;
                        input3.size = MAX_BUFFER;

                        soap_call___ifolder__ReadFile(&soap, url, NULL, &input3, &output3);

                        if (soap.error) 
                        {
                            soap_print_fault(&soap, stderr);
                            break;
                        }

                        struct xsd__base64Binary *buffer = output3.ReadFileResult;

                        if (buffer == NULL)
                        {
                            break;
                        }

                        fwrite(buffer->__ptr, sizeof(unsigned char), buffer->__size, fp);
                        printf(".");
                        fflush(stdout);
                    }

                    fclose(fp);

                    // close file
                    struct _ifolder__CloseFile input4;
                    struct _ifolder__CloseFileResponse output4;

                    input4.fileID = file_id;

                    soap_call___ifolder__CloseFile(&soap, url, NULL, &input4, &output4);

                    if (soap.error) 
                    {
                        soap_print_fault(&soap, stderr);
                        continue;
                    }

                    printf("%s (%f)\n\n", entry->Name, ((double)(clock() - start))/CLOCKS_PER_SEC);
                }
                break;

            // help
            case 'h':
                printf("change download help list make quit remove upload");
                break;

            // list entries or iFolders
            case 'l':
                {
                    if (ifolder == NULL)
                    {
                        struct _ifolder__GetiFolders input;
                        struct _ifolder__GetiFoldersResponse output;
        
                        soap_call___ifolder__GetiFolders(&soap, url, NULL, &input, &output);
        
                        if (soap.error) 
                        {
                            soap_print_fault(&soap, stderr);
                            continue;
                        }

                        struct ifolder__ArrayOfIFolder *ifolders = output.GetiFoldersResult;
    
                        int i;
    
                        if (ifolders != NULL) 
                        {
                            for (i = 0; i < ifolders->__sizeiFolder; i++) 
                            {
                                struct ifolder__iFolder *ifolder = ifolders->iFolder[i];

                                printf("%-24s\t%s\n", ifolder->Name, ifolder->OwnerName);
                            }
                        }
                    }
                    else
                    {
                        struct _ifolder__GetEntryByPath input;
                        struct _ifolder__GetEntryByPathResponse output;
    
                        input.ifolderID = ifolder->ID;
                        input.entryPath = path;
    
                        soap_call___ifolder__GetEntryByPath(&soap, url, NULL, &input, &output);
    
                        if (soap.error) 
                        {
                            soap_print_fault(&soap, stderr);
                            continue;
                        }

                        struct ifolder__iFolderEntry *entry = output.GetEntryByPathResult;
                        struct _ifolder__GetEntriesByParent input2;
                        struct _ifolder__GetEntriesByParentResponse output2;

                        input2.ifolderID = ifolder->ID;
                        input2.entryID = entry->ID;

                        soap_call___ifolder__GetEntriesByParent(&soap, url, NULL, &input2, &output2);

                        if (soap.error) 
                        {
                            soap_print_fault(&soap, stderr);
                            continue;
                        }

                        struct ifolder__ArrayOfIFolderEntry *entries = output2.GetEntriesByParentResult;
                        int i;

                        if (entries != NULL) 
                        {
                            for (i = 0; i < entries->__sizeiFolderEntry; i++) 
                            {
                                char time[MAX_STRING];
                                struct ifolder__iFolderEntry *entry = entries->iFolderEntry[i];
                                int tag = ' ';

                                if (entry->IsDirectory)
                                {
                                    if (entry->HasChildren)
                                    {
                                        tag = '+';
                                    }
                                    else
                                    {
                                        tag = '-';
                                    }
                                }
                                else
                                {
                                    tag = '#';
                                }

                                // time
                                strftime(time, MAX_STRING, "%m/%d/%Y %I:%M:%S %p", localtime(&entry->ModifiedTime));

                                printf("%s\t%12d\t%c %s\n", time, (int)entry->Size, tag, entry->Name);
                            }
                        }
                    }
                }
                break;

            // make directories or iFolders
            case 'm':
                {
                    char name[MAX_STRING];
                    char parent_path[MAX_STRING];

                    get_entry(name, MAX_STRING, path);
                    get_parent(parent_path, MAX_STRING, path);
    
                    if (strlen(name))
                    {
                        if (!strlen(parent_path))
                        {
                            // create iFolder
                            struct _ifolder__CreateiFolder input;
                            struct _ifolder__CreateiFolderResponse output;

                            input.ifolderName = name;

                            soap_call___ifolder__CreateiFolder(&soap, url, NULL, &input, &output);

                            if (soap.error) 
                            {
                                soap_print_fault(&soap, stderr);
                                continue;
                            }
                        }
                        else
                        {
                            // create directory
                            struct _ifolder__GetEntryByPath input;
                            struct _ifolder__GetEntryByPathResponse output;

                            input.ifolderID = ifolder->ID;
                            input.entryPath = parent_path;

                            soap_call___ifolder__GetEntryByPath(&soap, url, NULL, &input, &output);

                            if (soap.error) 
                            {
                                soap_print_fault(&soap, stderr);
                                continue;
                            }

                            struct ifolder__iFolderEntry *entry = output.GetEntryByPathResult;
                            struct _ifolder__CreateDirectoryEntry input2;
                            struct _ifolder__CreateDirectoryEntryResponse output2;

                            input2.entryName = name;
                            input2.ifolderID = ifolder->ID;
                            input2.parentID = entry->ID;

                            soap_call___ifolder__CreateDirectoryEntry(&soap, url, NULL, &input2, &output2);

                            if (soap.error) 
                            {
                                soap_print_fault(&soap, stderr);
                                continue;
                            }
                        }
                    }
                }
                break;
    
            // quit
            case 'q':
                quit = 1;
                break;
    
            // remove entries or iFolders
            case 'r':
                {
                    char name[MAX_STRING];
                    char parent_path[MAX_STRING];

                    get_entry(name, MAX_STRING, path);
                    get_parent(parent_path, MAX_STRING, path);

                    if (strlen(name))
                    {
                        if (!strlen(parent_path))
                        {
                            // delete iFolder
                            struct _ifolder__DeleteiFolder input;
                            struct _ifolder__DeleteiFolderResponse output;

                            input.ifolderID = ifolder->ID;

                            soap_call___ifolder__DeleteiFolder(&soap, url, NULL, &input, &output);

                            if (soap.error) 
                            {
                                soap_print_fault(&soap, stderr);
                                continue;
                            }
                        }
                        else
                        {
                            // delete directory
                            struct _ifolder__GetEntryByPath input;
                            struct _ifolder__GetEntryByPathResponse output;

                            input.ifolderID = ifolder->ID;
                            input.entryPath = path;

                            soap_call___ifolder__GetEntryByPath(&soap, url, NULL, &input, &output);

                            if (soap.error) 
                            {
                                soap_print_fault(&soap, stderr);
                                continue;
                            }

                            struct ifolder__iFolderEntry *entry = output.GetEntryByPathResult;
                            struct _ifolder__DeleteEntry input2;
                            struct _ifolder__DeleteEntryResponse output2;

                            input2.ifolderID = ifolder->ID;
                            input2.entryID = entry->ID;

                            soap_call___ifolder__DeleteEntry(&soap, url, NULL, &input2, &output2);

                            if (soap.error) 
                            {
                                soap_print_fault(&soap, stderr);
                                continue;
                            }
                        }
                    }
                }
                break;

            // upload a file
            case 'u':
                {
                    struct ifolder__iFolderEntry *entry = NULL;

                    struct _ifolder__GetEntryByPath input;
                    struct _ifolder__GetEntryByPathResponse output;

                    input.ifolderID = ifolder->ID;
                    input.entryPath = path;

                    soap_call___ifolder__GetEntryByPath(&soap, url, NULL, &input, &output);

                    if (soap.error) 
                    {
                        // create entry
                        char name[MAX_STRING];
                        char parent_path[MAX_STRING];

                        get_entry(name, MAX_STRING, path);
                        get_parent(parent_path, MAX_STRING, path);

                        struct _ifolder__GetEntryByPath input2;
                        struct _ifolder__GetEntryByPathResponse output2;

                        input2.ifolderID = ifolder->ID;
                        input2.entryPath = parent_path;

                        soap_call___ifolder__GetEntryByPath(&soap, url, NULL, &input2, &output2);

                        if (soap.error) 
                        {
                            soap_print_fault(&soap, stderr);
                            continue;
                        }

                        entry = output2.GetEntryByPathResult;

                        struct _ifolder__CreateFileEntry input3;
                        struct _ifolder__CreateFileEntryResponse output3;

                        input3.entryName = name;
                        input3.ifolderID = ifolder->ID;
                        input3.parentID = entry->ID;

                        soap_call___ifolder__CreateFileEntry(&soap, url, NULL, &input3, &output3);

                        if (soap.error) 
                        {
                            soap_print_fault(&soap, stderr);
                            continue;
                        }

                        entry = output3.CreateFileEntryResult;
                    }
                    else
                    {
                        entry = output.GetEntryByPathResult;
                    }

                    // start
                    clock_t start = clock();

                    // open local file
                    FILE *fp = fopen(entry->Name, "rb");

                    if (fp == NULL)
                    {
                        printf("Error: %s\n\n", strerror(errno));
                        continue;
                    }

                    // length
                    fseek(fp, 0, SEEK_END);
                    long length = ftell(fp);
                    fseek(fp, 0, SEEK_SET);
                    
                    // open file
                    struct _ifolder__OpenFileWrite input4;
                    struct _ifolder__OpenFileWriteResponse output4;

                    input4.ifolderID = ifolder->ID;
                    input4.entryID = entry->ID;
                    input4.length = length;

                    soap_call___ifolder__OpenFileWrite(&soap, url, NULL, &input4, &output4);

                    if (soap.error) 
                    {
                        soap_print_fault(&soap, stderr);
                        continue;
                    }

                    char *file_id = output4.OpenFileWriteResult;

                    // write file
                    while(1)
                    {
                        unsigned char buffer[MAX_BUFFER];

                        int size = fread(buffer, sizeof(unsigned char), MAX_BUFFER, fp);

                        if (size <= 0)
                        {
                            break;
                        }

                        struct _ifolder__WriteFile input5;
                        struct _ifolder__WriteFileResponse output5;
                        struct xsd__base64Binary packet;

                        input5.fileID = file_id;
                        input5.buffer = &packet;
                        packet.__ptr = buffer;
                        packet.__size = size;

                        soap_call___ifolder__WriteFile(&soap, url, NULL, &input5, &output5);

                        if (soap.error) 
                        {
                            soap_print_fault(&soap, stderr);
                            break;
                        }

                        printf(".");
                        fflush(stdout);
                    }

                    fclose(fp);

                    // close file
                    struct _ifolder__CloseFile input6;
                    struct _ifolder__CloseFileResponse output6;

                    input6.fileID = file_id;

                    soap_call___ifolder__CloseFile(&soap, url, NULL, &input6, &output6);

                    if (soap.error) 
                    {
                        soap_print_fault(&soap, stderr);
                        continue;
                    }

                    printf("%s (%f)\n\n", entry->Name, ((double)(clock() - start))/CLOCKS_PER_SEC);
                }
                break;

            // bad command
            default:
                printf("Bad Command: %s", command);
                break;
        }

    }

    // clean-up
    soap_end(&soap);
    soap_done(&soap); 

    return 0;
}

// parse the iFolder name from the path
void parse_ifolder(char *ifolder_name, size_t size, const char *path)
{
    *ifolder_name = '\0';

    if ((path != NULL) && (strlen(path) > 1))
    {
        strncpy(ifolder_name, ++path, size);

        char *p = index(ifolder_name, '/');

        if (p != NULL)
        {
            *p = '\0';
        }
    }
}

// combine the two paths
void path_combine(char *path, size_t size, const char *path1, const char *path2)
{
    if ((path2 == NULL) || (strlen(path2) == 0))
    {
        strncpy(path, path1, size);
    }
    else if (path2[0] == '/')
    {
        strncpy(path, path2, size);
    }
    else if (strcmp(path2, "..") == 0)
    {
        strncpy(path, path1, size);

        char *p = rindex(path, '/');

        if (p != NULL)
        {
            *p = '\0';
    
            if (strlen(path) == 0)
            {
                strncpy(path, "/", size);
            }
        }
    }
    else
    {
        strncpy(path, path1, size);

        int l = strlen(path);

        char *p = path + l - 1;

        if (*p != '/')
        {
            strncat(path, "/", (size - l++));
        }

        strncat(path, path2, (size - l));
    }
}

// get the parent path
void get_parent(char *parent, size_t size, const char *path)
{
    char temp[MAX_STRING];

    *parent = '\0';

    if ((path != NULL) && (strlen(path) > 1))
    {
        strncpy(temp, path, MAX_STRING);

        char *p = rindex(temp, '/');

        if (p != NULL)
        {
            *p = '\0';

            strncpy(parent, temp, size);
        }
    }
}

// get the entry name
void get_entry(char *entry, size_t size, const char *path)
{
    *entry = '\0';

    if ((path != NULL) && (strlen(path) > 1))
    {
        char *p = rindex(path, '/');

        strncpy(entry, ++p, size);
    }
}


