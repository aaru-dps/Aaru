/****************************************************************************
The Disc Image Chef
-----------------------------------------------------------------------------

Filename       : dir.c
Author(s)      : Natalia Portillo

Component      : fstester.getter.os2

--[ Description ] -----------------------------------------------------------

Contains directory handlers

--[ License ] ---------------------------------------------------------------
     This program is free software: you can redistribute it and/or modify
     it under the terms of the GNU General Public License as
     published by the Free Software Foundation, either version 3 of the
     License, or (at your option) any later version.

     This program is distributed in the hope that it will be useful,
     but WITHOUT ANY WARRANTY; without even the implied warraty of
     MERCHANTIBILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     GNU General Public License for more details.

     You should have received a copy of the GNU General Public License
     along with this program.  If not, see <http://www.gnu.org/licenses/>.

-----------------------------------------------------------------------------
Copyright (C) 2011-2018 Natalia Portillo
*****************************************************************************/

#define INCL_DOSFILEMGR // File Manager values
#define INCL_DOSERRORS // DOS error values

#include <os2.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include "dir.h"
#include "ea.h"

int GetAllInDir(PSZ path, ULONG *eaCounter)
{
    HDIR         hdirFindHandle;
    FILEFINDBUF3 FindBuffer = {0}; // Returned from FindFirst/Next
    ULONG        ulResultBufLen;
    ULONG        ulFindCount;
    APIRET       rc; // Return code
    PSZ          pathWithWildcard;
    PSZ          fullPath;
    ULONG        flAttribute; // File attributes
    int          isDir      = 0;
    APIRET       drc;

    // Variables for EA saving
    char   *eaBuffer;
    size_t eaSize;
    char   eaPath[8 + 1 + 3 + 1];
    HFILE  eaFileHandle; // Address of the handle for the file
    ULONG  ulAction         = 0; // Address of the variable that receives the value that specifies the action taken by the DosOpen function
    APIRET earc;

    for(isDir = 0; isDir < 2; isDir++)
    {
        hdirFindHandle   = HDIR_CREATE;
        ulResultBufLen   = sizeof(FILEFINDBUF3);
        ulFindCount      = 1; // Look for 1 file at a time
        rc               = NO_ERROR; // Return code
        pathWithWildcard = malloc(strlen(path) + 5);
        strcpy(pathWithWildcard, path);
        strcat(pathWithWildcard, "\\*.*");    // Adds wildcard to passed path

        flAttribute = FILE_ARCHIVED | FILE_SYSTEM | FILE_HIDDEN | FILE_READONLY; // All files
        if(isDir)
            flAttribute |= MUST_HAVE_DIRECTORY; // Must be a directory

        rc = DosFindFirst(pathWithWildcard, // File pattern
                          &hdirFindHandle, // Directory search handle
                          flAttribute, // Search attribute
                          &FindBuffer, // Result buffer
                          ulResultBufLen, // Result buffer length
                          &ulFindCount, // Number of entries to find
                          FIL_STANDARD); // Return Level 1 file info

        if(rc != NO_ERROR)
        {
            free(pathWithWildcard);
            if(rc == ERROR_NO_MORE_FILES && !isDir)
                continue;

            printf("DosFindFirst error: return code = %u\n", rc);
            return rc;
        }
        else
        {
            if(strcmp(FindBuffer.achName, ".") != 0 && strcmp(FindBuffer.achName, "..") != 0)
            {
                fullPath = malloc(strlen(path) + 2 + strlen(FindBuffer.achName) + 1);
                fullPath = strcpy(fullPath, path); // Parent path
                strcat(fullPath, "\\"); // Adds slashes
                strcat(fullPath, FindBuffer.achName); // Adds filename
                if(isDir)
                    printf("%s\\\n", fullPath); // Print directory name
                else
                    printf("%s\n", fullPath); // Print file name
                if(strcmp(FindBuffer.achName, ".") != 0 && strcmp(FindBuffer.achName, "..") != 0)
                {
                    eaSize = 0;
                    GetEAs(fullPath, &eaBuffer, &eaSize);

                    if(eaSize != 0)
                    {
                        sprintf(&eaPath, "%08d.EA", *eaCounter);
                        earc = DosOpen(eaPath, &eaFileHandle, &ulAction, 0L, FILE_NORMAL,
                                       OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                                       OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, NULL);

                        if(earc == NO_ERROR)
                        {
                            earc = DosWrite(eaFileHandle, eaBuffer, eaSize, &ulAction);
                            if(earc == NO_ERROR)
                            {
                                printf("\tSaved %ld bytes from %ld bytes of EAs saved to %08d.EA\n", ulAction, eaSize,
                                       *eaCounter);
                                *eaCounter += 1;
                            }
                            DosClose(eaFileHandle);
                        }

                        free(eaBuffer);
                    }

                    if(isDir)
                    {
                        drc = GetAllInDir(fullPath, eaCounter);
                        if(drc != NO_ERROR && drc != ERROR_NO_MORE_FILES)
                        {
                            printf("GetAllInDir(%s) returned %u\n", fullPath, drc);
                            return drc;
                        }
                    }
                }

                free(fullPath);
            }
        }

        while(rc != ERROR_NO_MORE_FILES)
        {
            ulFindCount = 1; // Reset find count

            rc = DosFindNext(hdirFindHandle, // Directory handle
                             &FindBuffer, // Result buffer
                             ulResultBufLen, // Result buffer length
                             &ulFindCount); // Number of entries to find

            if(rc != NO_ERROR && rc != ERROR_NO_MORE_FILES)
            {
                free(pathWithWildcard);
                printf("DosFindNext error: return code = %u\n", rc);
                return rc;
            }
            else
            {
                if(strcmp(FindBuffer.achName, ".") == 0 || strcmp(FindBuffer.achName, "..") == 0)
                    continue;

                fullPath = malloc(strlen(path) + 2 + strlen(FindBuffer.achName) + 1);
                fullPath = strcpy(fullPath, path); // Parent path
                strcat(fullPath, "\\"); // Adds slashes
                strcat(fullPath, FindBuffer.achName); // Adds filename
                if(isDir)
                    printf("%s\\\n", fullPath); // Print directory name
                else
                    printf("%s\n", fullPath); // Print file name

                eaSize = 0;
                GetEAs(fullPath, &eaBuffer, &eaSize);

                if(eaSize != 0)
                {
                    sprintf(&eaPath, "%08d.EA", *eaCounter);
                    earc = DosOpen(eaPath, &eaFileHandle, &ulAction, 0L, FILE_NORMAL,
                                   OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                                   OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, NULL);

                    if(earc == NO_ERROR)
                    {
                        earc = DosWrite(eaFileHandle, eaBuffer, eaSize, &ulAction);
                        if(earc == NO_ERROR)
                        {
                            printf("\tSaved %ld bytes from %ld bytes of EAs saved to %08d.EA\n", ulAction, eaSize,
                                   *eaCounter);
                            *eaCounter += 1;
                        }
                        DosClose(eaFileHandle);
                    }
                    else
                        printf("Error %d calling DosOpen\n", earc);

                    free(eaBuffer);
                }

                if(isDir)
                {
                    drc = GetAllInDir(fullPath, eaCounter);
                    if(drc != NO_ERROR && drc != ERROR_NO_MORE_FILES)
                    {
                        printf("GetAllInDir(%s) returned %u\n", fullPath, drc);
                        return drc;
                    }
                }

                free(fullPath);
            } /* endif */
        } /* endwhile */

        free(pathWithWildcard);
        rc = DosFindClose(hdirFindHandle); // Close our directory handle
        if(rc != NO_ERROR)
        {
            printf("DosFindClose error: return code = %u\n", rc);
            return rc;
        }
    }
    return ERROR_NO_MORE_FILES;
}

