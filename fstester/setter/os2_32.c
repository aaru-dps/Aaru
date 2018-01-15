/****************************************************************************
The Disc Image Chef
-----------------------------------------------------------------------------

Filename       : os2_32.c
Author(s)      : Natalia Portillo

Component      : fstester.setter.os2

--[ Description ] -----------------------------------------------------------

Contains 32-bit OS/2 code

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

#if (defined(__I386__) || defined (__i386__) || defined (__THW_INTEL) || defined (_M_IX86)) && (defined(__OS2__) || defined (__os2__)) && !defined (__DOS__)

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define INCL_DOSMISC
#define INCL_DOSFILEMGR

#include <os2.h>

#include "defs.h"
#include "consts.h"

#include "os2_16.h"
#include "dosos2.h"

void GetOsInfo()
{
    ULONG aulBuffer[3];
    APIRET rc;
    ULONG MajorVer;
    ULONG MinorVer;
    ULONG pathLen[1];

    rc = DosQuerySysInfo(QSV_VERSION_MAJOR, QSV_VERSION_REVISION, (PVOID) aulBuffer, 3 * sizeof(ULONG));
    
    if(rc)
    {
       printf("Error %d querying OS/2 version.\n", rc);
       return;
    }

    if(aulBuffer[0] = 20)
    {
       MajorVer = aulBuffer[1] / 10;
       MinorVer = aulBuffer[1] % 10;
    }
    else
    {
       MajorVer = aulBuffer[0];
       MinorVer = aulBuffer[1];
    }

    printf("OS information:\n");

    if(aulBuffer[2] > 0x20)
       printf("\tRunning under OS/2 %d.%d revision %c.\n", MajorVer, MinorVer, aulBuffer[2]);
    else
       printf("\tRunning under OS/2 %d.%d\n", MajorVer, MinorVer);

    rc = DosQuerySysInfo(QSV_MAX_PATH_LENGTH, QSV_MAX_PATH_LENGTH, (PVOID)pathLen, sizeof(ULONG));

    printf("\tMaximum path is %lu bytes.\n", pathLen[0]);
}

void GetVolumeInfo(const char *path, size_t *clusterSize)
{
    APIRET      rc;
    BYTE        bData[64];
    ULONG      cbData  = sizeof(bData);
    PFSALLOCATE pfsAllocateBuffer;
    ULONG      driveNo = path[0] - '@';
    char        *fsdName;
    PFSINFO     pfsInfo;

    if(driveNo > 32)
       driveNo-=32;

    *clusterSize = 0;
                   
    rc = DosQueryFSAttach((PSZ)path, 0, FSAIL_QUERYNAME, (PVOID) &bData, &cbData);

    printf("Volume information:\n");
    printf("\tPath: %s\n", path);
    printf("\tDrive number: %d\n", driveNo - 1);

    if(rc)
    {
        printf("Error %d requesting volume information.\n", rc);
    }
    else
    {
        fsdName = &bData[8 + (USHORT)bData[2] + 1];
        printf("\tFSD name: %s\n", fsdName);
    }

    pfsAllocateBuffer = (PFSALLOCATE)malloc(sizeof(FSALLOCATE));
    rc                = DosQueryFSInfo(driveNo, FSIL_ALLOC, (PBYTE)pfsAllocateBuffer, sizeof(FSALLOCATE));

    if(rc)
    {
        printf("Error %d requesting volume information.\n", rc);
    }
    else
    {
        printf("\tBytes per sector: %u\n", pfsAllocateBuffer->cbSector);
        printf("\tSectors per cluster: %lu (%lu bytes)\n", pfsAllocateBuffer->cSectorUnit,
               pfsAllocateBuffer->cSectorUnit * pfsAllocateBuffer->cbSector);
        printf("\tClusters: %lu (%lu bytes)\n", pfsAllocateBuffer->cUnit,
               pfsAllocateBuffer->cSectorUnit * pfsAllocateBuffer->cbSector * pfsAllocateBuffer->cUnit);
        printf("\tFree clusters: %lu (%lu bytes)\n", pfsAllocateBuffer->cUnitAvail,
               pfsAllocateBuffer->cSectorUnit * pfsAllocateBuffer->cbSector * pfsAllocateBuffer->cUnitAvail);

        *clusterSize = pfsAllocateBuffer->cSectorUnit * pfsAllocateBuffer->cbSector;
    }

    free(pfsAllocateBuffer);

    pfsInfo = (PFSINFO)malloc(sizeof(FSINFO));
    rc      = DosQueryFSInfo(driveNo, FSIL_VOLSER, (PBYTE)pfsInfo, sizeof(FSINFO));

    if(rc)
    {
        printf("Error %d requesting volume information.\n", rc);
    }
    else
    {
        printf("\tVolume label: %s\n", pfsInfo->vol.szVolLabel);
        printf("\tVolume created on %d/%02d/%02d %02d:%02d:%02d\n", pfsInfo->fdateCreation.year + 1980,
               pfsInfo->fdateCreation.month - 1, pfsInfo->fdateCreation.day, pfsInfo->ftimeCreation.hours,
               pfsInfo->ftimeCreation.minutes, pfsInfo->ftimeCreation.twosecs * 2);
    }

    free(pfsInfo);
}

void FileAttributes(const char *path)
{
    char   drivePath[4];
    APIRET rc          = 0, wRc = 0, cRc = 0;
    ULONG actionTaken = 0;
    HFILE  handle;
    FILESTATUS3 fileStatus = {{0}};

    drivePath[0] = path[0];
    drivePath[1] = ':';
    drivePath[2] = '\\';
    drivePath[3] = 0;

    rc = DosSetCurrentDir(drivePath);

    if(rc)
    {
        printf("Cannot change to specified path, not continuing.\n");
        return;
    }

    rc = DosCreateDir("ATTRS", NULL);

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = DosSetCurrentDir("ATTRS");

    printf("Creating attributes files.\n");

    rc = DosOpen("NONE", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)noAttributeText, strlen(noAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_NORMAL;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with no attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "NONE", rc, wRc, cRc);

    rc = DosOpen("ARCHIVE", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_ARCHIVED;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with archived attribute: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "ARCHIVE", rc, wRc, cRc);

    rc = DosOpen("SYSTEM", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_SYSTEM;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with system attribute: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "SYSTEM", rc, wRc, cRc);

    rc = DosOpen("HIDDEN", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_HIDDEN;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with hidden attribute: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "HIDDEN", rc, wRc, cRc);

    rc = DosOpen("READONLY", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_READONLY;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with read-only attribute: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "READONLY", rc, wRc, cRc);

    rc = DosOpen("HIDDREAD", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_HIDDEN | FILE_READONLY;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with hidden, read-only attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "HIDDREAD", rc,
           wRc, cRc);

    rc = DosOpen("SYSTREAD", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_SYSTEM | FILE_READONLY;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with system, read-only attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "SYSTREAD", rc,
           wRc, cRc);

    rc = DosOpen("SYSTHIDD", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_SYSTEM | FILE_HIDDEN;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with system, hidden attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "SYSTHIDD", rc, wRc,
           cRc);

    rc = DosOpen("SYSRDYHD", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_SYSTEM | FILE_READONLY | FILE_HIDDEN;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with system, read-only, hidden attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "SYSRDYHD",
           rc, wRc, cRc);

    rc = DosOpen("ARCHREAD", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_ARCHIVED | FILE_READONLY;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with archived, read-only attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "ARCHREAD", rc,
           wRc, cRc);

    rc = DosOpen("ARCHHIDD", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_ARCHIVED | FILE_HIDDEN;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with archived, hidden attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "ARCHHIDD", rc, wRc,
           cRc);

    rc = DosOpen("ARCHDRDY", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_ARCHIVED | FILE_HIDDEN | FILE_READONLY;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with archived, hidden, read-only attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n",
           "ARCHDRDY", rc, wRc, cRc);

    rc = DosOpen("ARCHSYST", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_ARCHIVED | FILE_SYSTEM;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with archived, system attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "ARCHSYST", rc, wRc,
           cRc);

    rc = DosOpen("ARSYSRDY", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_ARCHIVED | FILE_SYSTEM | FILE_READONLY;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with archived, system, read-only attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n",
           "ARSYSRDY", rc, wRc, cRc);

    rc = DosOpen("ARCSYSHD", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_ARCHIVED | FILE_SYSTEM | FILE_HIDDEN;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with archived, system, hidden attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "ARCSYSHD",
           rc, wRc, cRc);

    rc = DosOpen("ARSYHDRD", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_REPLACE_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        wRc = DosWrite(handle, (PVOID)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        wRc = DosWrite(handle, (PVOID)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        fileStatus.attrFile = FILE_ARCHIVED | FILE_SYSTEM | FILE_HIDDEN | FILE_READONLY;
        rc = DosSetFileInfo(handle, FIL_STANDARD, &fileStatus, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile with all (archived, system, hidden, read-only) attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n",
           "ARSYHDRD", rc, wRc, cRc);
}

void FilePermissions(const char *path)
{
    /* Do nothing, not supported by target operating system */
}

void ExtendedAttributes(const char *path)
{
    /* TODO: Implement */
}

void ResourceFork(const char *path)
{
    /* Do nothing, not supported by target operating system */
}

void Filenames(const char *path)
{
    char   drivePath[4];
    APIRET rc          = 0, wRc = 0, cRc = 0;
    ULONG actionTaken = 0;
    HFILE  handle;
    char   message[300];
    int    pos         = 0;

    drivePath[0] = path[0];
    drivePath[1] = ':';
    drivePath[2] = '\\';
    drivePath[3] = 0;

    rc = DosSetCurrentDir(drivePath);

    if(rc)
    {
        printf("Cannot change to specified path, not continuing.\n");
        return;
    }

    rc = DosCreateDir("FILENAME", NULL);

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = DosSetCurrentDir("FILENAME");

    printf("Creating files with different filenames.\n");

    for(pos = 0; filenames[pos]; pos++)
    {
        rc = DosOpen((PSZ)filenames[pos], &handle, &actionTaken, 0, FILE_NORMAL,
                     OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                     OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

        if(!rc)
        {
            memset(&message, 0, 300);
            sprintf(&message, FILENAME_FORMAT, filenames[pos]);
            wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
            cRc = DosClose(handle);
        }

        printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", filenames[pos], rc, wRc, cRc);
    }
}

#define DATETIME_FORMAT "This file is dated %04d/%02d/%02d %02d:%02d:%02d for %s\n"

void Timestamps(const char *path)
{
    char       drivePath[4];
    APIRET     rc          = 0, wRc = 0, cRc = 0, tRc = 0;
    ULONG     actionTaken = 0;
    HFILE      handle;
    char       message[300];
    FILESTATUS3 status;

    drivePath[0] = path[0];
    drivePath[1] = ':';
    drivePath[2] = '\\';
    drivePath[3] = 0;

    rc = DosSetCurrentDir(drivePath);

    if(rc)
    {
        printf("Cannot change to specified path, not continuing.\n");
        return;
    }

    rc = DosCreateDir("TIMES", NULL);

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = DosSetCurrentDir("TIMES");

    printf("Creating timestamped files.\n");

    rc = DosOpen((PSZ)"MAXCTIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS));
        status.fdateCreation.year    = 127;
        status.fdateCreation.month   = 12;
        status.fdateCreation.day     = 31;
        status.ftimeCreation.hours   = 23;
        status.ftimeCreation.minutes = 59;
        status.ftimeCreation.twosecs = 29;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateCreation.year + 1980, status.fdateCreation.month,
                status.fdateCreation.day, status.ftimeCreation.hours, status.ftimeCreation.minutes,
                status.ftimeCreation.twosecs * 2, "creation");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MAXCTIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"MINCTIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateCreation.year    = 0;
        status.fdateCreation.month   = 1;
        status.fdateCreation.day     = 1;
        status.ftimeCreation.hours   = 0;
        status.ftimeCreation.minutes = 0;
        status.ftimeCreation.twosecs = 0;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateCreation.year + 1980, status.fdateCreation.month,
                status.fdateCreation.day, status.ftimeCreation.hours, status.ftimeCreation.minutes,
                status.ftimeCreation.twosecs * 2, "creation");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MINCTIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"Y19CTIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateCreation.year    = 1999 - 1980;
        status.fdateCreation.month   = 12;
        status.fdateCreation.day     = 31;
        status.ftimeCreation.hours   = 23;
        status.ftimeCreation.minutes = 59;
        status.ftimeCreation.twosecs = 29;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateCreation.year + 1980, status.fdateCreation.month,
                status.fdateCreation.day, status.ftimeCreation.hours, status.ftimeCreation.minutes,
                status.ftimeCreation.twosecs * 2, "creation");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y19CTIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"Y2KCTIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateCreation.year    = 2000 - 1980;
        status.fdateCreation.month   = 1;
        status.fdateCreation.day     = 1;
        status.ftimeCreation.hours   = 0;
        status.ftimeCreation.minutes = 0;
        status.ftimeCreation.twosecs = 0;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateCreation.year + 1980, status.fdateCreation.month,
                status.fdateCreation.day, status.ftimeCreation.hours, status.ftimeCreation.minutes,
                status.ftimeCreation.twosecs * 2, "creation");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y19CTIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"MAXWTIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateLastWrite.year    = 127;
        status.fdateLastWrite.month   = 12;
        status.fdateLastWrite.day     = 31;
        status.ftimeLastWrite.hours   = 23;
        status.ftimeLastWrite.minutes = 59;
        status.ftimeLastWrite.twosecs = 29;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateLastWrite.year + 1980, status.fdateLastWrite.month,
                status.fdateLastWrite.day, status.ftimeLastWrite.hours, status.ftimeLastWrite.minutes,
                status.ftimeLastWrite.twosecs * 2, "last written");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MAXWTIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"MINWTIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateLastWrite.year    = 0;
        status.fdateLastWrite.month   = 1;
        status.fdateLastWrite.day     = 1;
        status.ftimeLastWrite.hours   = 0;
        status.ftimeLastWrite.minutes = 0;
        status.ftimeLastWrite.twosecs = 0;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateLastWrite.year + 1980, status.fdateLastWrite.month,
                status.fdateLastWrite.day, status.ftimeLastWrite.hours, status.ftimeLastWrite.minutes,
                status.ftimeLastWrite.twosecs * 2, "last written");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MINWTIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"Y19WTIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateLastWrite.year    = 1999 - 1980;
        status.fdateLastWrite.month   = 12;
        status.fdateLastWrite.day     = 31;
        status.ftimeLastWrite.hours   = 23;
        status.ftimeLastWrite.minutes = 59;
        status.ftimeLastWrite.twosecs = 29;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateLastWrite.year + 1980, status.fdateLastWrite.month,
                status.fdateLastWrite.day, status.ftimeLastWrite.hours, status.ftimeLastWrite.minutes,
                status.ftimeLastWrite.twosecs * 2, "last written");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y19WTIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"Y2KWTIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateLastWrite.year    = 2000 - 1980;
        status.fdateLastWrite.month   = 1;
        status.fdateLastWrite.day     = 1;
        status.ftimeLastWrite.hours   = 0;
        status.ftimeLastWrite.minutes = 0;
        status.ftimeLastWrite.twosecs = 0;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateLastWrite.year + 1980, status.fdateLastWrite.month,
                status.fdateLastWrite.day, status.ftimeLastWrite.hours, status.ftimeLastWrite.minutes,
                status.ftimeLastWrite.twosecs * 2, "last written");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y2KWTIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"MAXATIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateLastAccess.year    = 127;
        status.fdateLastAccess.month   = 12;
        status.fdateLastAccess.day     = 31;
        status.ftimeLastAccess.hours   = 23;
        status.ftimeLastAccess.minutes = 59;
        status.ftimeLastAccess.twosecs = 29;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateLastAccess.year + 1980, status.fdateLastAccess.month,
                status.fdateLastAccess.day, status.ftimeLastAccess.hours, status.ftimeLastAccess.minutes,
                status.ftimeLastAccess.twosecs * 2, "last access");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MAXATIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"MINATIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateLastAccess.year    = 0;
        status.fdateLastAccess.month   = 1;
        status.fdateLastAccess.day     = 1;
        status.ftimeLastAccess.hours   = 0;
        status.ftimeLastAccess.minutes = 0;
        status.ftimeLastAccess.twosecs = 0;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateLastAccess.year + 1980, status.fdateLastAccess.month,
                status.fdateLastAccess.day, status.ftimeLastAccess.hours, status.ftimeLastAccess.minutes,
                status.ftimeLastAccess.twosecs * 2, "last access");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MINATIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"Y19ATIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateLastAccess.year    = 1999 - 1980;
        status.fdateLastAccess.month   = 12;
        status.fdateLastAccess.day     = 31;
        status.ftimeLastAccess.hours   = 23;
        status.ftimeLastAccess.minutes = 59;
        status.ftimeLastAccess.twosecs = 29;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateLastAccess.year + 1980, status.fdateLastAccess.month,
                status.fdateLastAccess.day, status.ftimeLastAccess.hours, status.ftimeLastAccess.minutes,
                status.ftimeLastAccess.twosecs * 2, "last access");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y19ATIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"Y2KATIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateLastAccess.year    = 2000 - 1980;
        status.fdateLastAccess.month   = 1;
        status.fdateLastAccess.day     = 1;
        status.ftimeLastAccess.hours   = 0;
        status.ftimeLastAccess.minutes = 0;
        status.ftimeLastAccess.twosecs = 0;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateLastAccess.year + 1980, status.fdateLastAccess.month,
                status.fdateLastAccess.day, status.ftimeLastAccess.hours, status.ftimeLastAccess.minutes,
                status.ftimeLastAccess.twosecs * 2, "last access");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y2KATIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"MAX_TIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateCreation.year    = 127;
        status.fdateCreation.month   = 12;
        status.fdateCreation.day     = 31;
        status.ftimeCreation.hours   = 23;
        status.ftimeCreation.minutes = 59;
        status.ftimeCreation.twosecs = 29;
        status.fdateLastAccess       = status.fdateCreation;
        status.ftimeLastAccess       = status.ftimeCreation;
        status.fdateLastWrite        = status.fdateCreation;
        status.ftimeLastWrite        = status.ftimeCreation;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateCreation.year + 1980, status.fdateCreation.month,
                status.fdateCreation.day, status.ftimeCreation.hours, status.ftimeCreation.minutes,
                status.ftimeCreation.twosecs * 2, "all");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MAX_TIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"MIN_TIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateCreation.year    = 0;
        status.fdateCreation.month   = 1;
        status.fdateCreation.day     = 1;
        status.ftimeCreation.hours   = 0;
        status.ftimeCreation.minutes = 0;
        status.ftimeCreation.twosecs = 0;
        status.fdateLastAccess       = status.fdateCreation;
        status.ftimeLastAccess       = status.ftimeCreation;
        status.fdateLastWrite        = status.fdateCreation;
        status.ftimeLastWrite        = status.ftimeCreation;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateCreation.year + 1980, status.fdateCreation.month,
                status.fdateCreation.day, status.ftimeCreation.hours, status.ftimeCreation.minutes,
                status.ftimeCreation.twosecs * 2, "all");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MIN_TIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"Y19_TIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateCreation.year    = 1999 - 1980;
        status.fdateCreation.month   = 12;
        status.fdateCreation.day     = 31;
        status.ftimeCreation.hours   = 23;
        status.ftimeCreation.minutes = 59;
        status.ftimeCreation.twosecs = 29;
        status.fdateLastAccess       = status.fdateCreation;
        status.ftimeLastAccess       = status.ftimeCreation;
        status.fdateLastWrite        = status.fdateCreation;
        status.ftimeLastWrite        = status.ftimeCreation;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateCreation.year + 1980, status.fdateCreation.month,
                status.fdateCreation.day, status.ftimeCreation.hours, status.ftimeCreation.minutes,
                status.ftimeCreation.twosecs * 2, "all");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y19_TIME", rc, wRc, cRc, tRc);

    rc = DosOpen((PSZ)"Y2K_TIME", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);

    if(!rc)
    {
        memset(&status, 0, sizeof(FILESTATUS3));
        status.fdateCreation.year    = 2000 - 1980;
        status.fdateCreation.month   = 1;
        status.fdateCreation.day     = 1;
        status.ftimeCreation.hours   = 0;
        status.ftimeCreation.minutes = 0;
        status.ftimeCreation.twosecs = 0;
        status.fdateLastAccess       = status.fdateCreation;
        status.ftimeLastAccess       = status.ftimeCreation;
        status.fdateLastWrite        = status.fdateCreation;
        status.ftimeLastWrite        = status.ftimeCreation;
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, status.fdateCreation.year + 1980, status.fdateCreation.month,
                status.fdateCreation.day, status.ftimeCreation.hours, status.ftimeCreation.minutes,
                status.ftimeCreation.twosecs * 2, "all");

        wRc = DosWrite(handle, &message, strlen(message), &actionTaken);
        tRc = DosSetFileInfo(handle, 1, (PBYTE) & status, sizeof(FILESTATUS3));
        cRc = DosClose(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y2K_TIME", rc, wRc, cRc, tRc);
}

void DirectoryDepth(const char *path)
{
    char   drivePath[4];
    APIRET rc  = 0;
    char   filename[9];
    long   pos = 2;

    drivePath[0] = path[0];
    drivePath[1] = ':';
    drivePath[2] = '\\';
    drivePath[3] = 0;

    rc = DosSetCurrentDir(drivePath);

    if(rc)
    {
        printf("Cannot change to specified path, not continuing.\n");
        return;
    }

    rc = DosCreateDir("DEPTH", NULL);

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = DosSetCurrentDir("DEPTH");

    printf("Creating deepest directory tree.\n");

    while(!rc)
    {
        memset(&filename, 0, 9);
        sprintf(&filename, "%08d", pos);
        rc = DosCreateDir(filename, NULL);

        if(!rc)
            rc = DosSetCurrentDir(filename);

        pos++;
    }

    printf("\tCreated %d levels of directory hierarchy\n", pos);
}

void Fragmentation(const char *path, size_t clusterSize)
{
    size_t        halfCluster             = clusterSize / 2;
    size_t        quarterCluster          = clusterSize / 4;
    size_t        twoCluster              = clusterSize * 2;
    size_t        threeQuartersCluster    = halfCluster + quarterCluster;
    size_t        twoAndThreeQuartCluster = threeQuartersCluster + twoCluster;
    unsigned char *buffer;
    char          drivePath[4];
    APIRET        rc                      = 0, wRc = 0, cRc = 0;
    ULONG        actionTaken             = 0;
    HFILE         handle;
    long          i;

    drivePath[0] = path[0];
    drivePath[1] = ':';
    drivePath[2] = '\\';
    drivePath[3] = 0;

    rc = DosSetCurrentDir(drivePath);

    if(rc)
    {
        printf("Cannot change to specified path, not continuing.\n");
        return;
    }

    rc = DosCreateDir("FRAGS", NULL);

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = DosSetCurrentDir("FRAGS");

    rc = DosOpen((PSZ)"HALFCLST", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
    if(!rc)
    {
        buffer = malloc(halfCluster);
        memset(buffer, 0, halfCluster);

        for(i = 0; i < halfCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = DosWrite(handle, buffer, halfCluster, &actionTaken);
        cRc = DosClose(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "HALFCLST", halfCluster, rc, wRc, cRc);

    rc = DosOpen((PSZ)"QUARCLST", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
    if(!rc)
    {
        buffer = malloc(quarterCluster);
        memset(buffer, 0, quarterCluster);

        for(i = 0; i < quarterCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = DosWrite(handle, buffer, quarterCluster, &actionTaken);
        cRc = DosClose(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "QUARCLST", quarterCluster, rc, wRc, cRc);

    rc = DosOpen((PSZ)"TWOCLST", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
    if(!rc)
    {
        buffer = malloc(twoCluster);
        memset(buffer, 0, twoCluster);

        for(i = 0; i < twoCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = DosWrite(handle, buffer, twoCluster, &actionTaken);
        cRc = DosClose(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TWOCLST", twoCluster, rc, wRc, cRc);

    rc = DosOpen((PSZ)"TRQTCLST", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
    if(!rc)
    {
        buffer = malloc(threeQuartersCluster);
        memset(buffer, 0, threeQuartersCluster);

        for(i = 0; i < threeQuartersCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = DosWrite(handle, buffer, threeQuartersCluster, &actionTaken);
        cRc = DosClose(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TRQTCLST", threeQuartersCluster, rc, wRc,
           cRc);

    rc = DosOpen((PSZ)"TWTQCLST", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
    if(!rc)
    {
        buffer = malloc(twoAndThreeQuartCluster);
        memset(buffer, 0, twoAndThreeQuartCluster);

        for(i = 0; i < twoAndThreeQuartCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = DosWrite(handle, buffer, twoAndThreeQuartCluster, &actionTaken);
        cRc = DosClose(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TWTQCLST", twoAndThreeQuartCluster, rc,
           wRc, cRc);

    rc = DosOpen((PSZ)"TWO1", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
    if(!rc)
    {
        buffer = malloc(twoCluster);
        memset(buffer, 0, twoCluster);

        for(i = 0; i < twoCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = DosWrite(handle, buffer, twoCluster, &actionTaken);
        cRc = DosClose(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TWO1", twoCluster, rc, wRc, cRc);

    rc = DosOpen((PSZ)"TWO2", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
    if(!rc)
    {
        buffer = malloc(twoCluster);
        memset(buffer, 0, twoCluster);

        for(i = 0; i < twoCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = DosWrite(handle, buffer, twoCluster, &actionTaken);
        cRc = DosClose(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TWO2", twoCluster, rc, wRc, cRc);

    rc = DosOpen((PSZ)"TWO3", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
    if(!rc)
    {
        buffer = malloc(twoCluster);
        memset(buffer, 0, twoCluster);

        for(i = 0; i < twoCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = DosWrite(handle, buffer, twoCluster, &actionTaken);
        cRc = DosClose(handle);
        free(buffer);
    }

    printf("\tDeleting \"TWO2\".\n");
    rc = DosDelete((PSZ)"TWO2");

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TWO3", twoCluster, rc, wRc, cRc);

    rc = DosOpen((PSZ)"FRAGTHRQ", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
    if(!rc)
    {
        buffer = malloc(threeQuartersCluster);
        memset(buffer, 0, threeQuartersCluster);

        for(i = 0; i < threeQuartersCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = DosWrite(handle, buffer, threeQuartersCluster, &actionTaken);
        cRc = DosClose(handle);
        free(buffer);
    }

    printf("\tDeleting \"TWO1\".\n");
    rc = DosDelete((PSZ)"TWO1");
    printf("\tDeleting \"TWO3\".\n");
    rc = DosDelete((PSZ)"TWO3");

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "FRAGTHRQ", threeQuartersCluster, rc, wRc,
           cRc);

    rc = DosOpen((PSZ)"FRAGSIXQ", &handle, &actionTaken, 0, FILE_NORMAL,
                 OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                 OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
    if(!rc)
    {
        buffer = malloc(twoAndThreeQuartCluster);
        memset(buffer, 0, twoAndThreeQuartCluster);

        for(i = 0; i < twoAndThreeQuartCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = DosWrite(handle, buffer, twoAndThreeQuartCluster, &actionTaken);
        cRc = DosClose(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "FRAGSIXQ", twoAndThreeQuartCluster, rc,
           wRc, cRc);
}

void Sparse(const char *path)
{
    /* Do nothing, not supported by target operating system */
}

void Links(const char *path)
{
    // TODO: Check if can ask WPS to make Shadow
}

void MillionFiles(const char *path)
{
    char               drivePath[4];
    APIRET             rc          = 0;
    char               filename[9];
    unsigned long long pos         = 0;
    ULONG             actionTaken = 0;
    HFILE              handle;

    drivePath[0] = path[0];
    drivePath[1] = ':';
    drivePath[2] = '\\';
    drivePath[3] = 0;

    rc = DosSetCurrentDir(drivePath);

    if(rc)
    {
        printf("Cannot change to specified path, not continuing.\n");
        return;
    }

    rc = DosCreateDir("MILLION", NULL);

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = DosSetCurrentDir("MILLION");

    printf("Creating lots of files.\n");

    for(pos = 0; pos < 100000ULL; pos++)
    {
        memset(&filename, 0, 9);
        sprintf(&filename, "%08d", pos);
        rc = DosOpen(&filename, &handle, &actionTaken, 0, FILE_NORMAL,
                     OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                     OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
        if(rc)
            break;

        DosClose(handle);
    }

    printf("\tCreated %llu files\n", pos);
}

void DeleteFiles(const char *path)
{
    char   drivePath[4];
    APIRET rc          = 0;
    char   filename[9];
    short  pos         = 0;
    ULONG actionTaken = 0;
    HFILE  handle;

    drivePath[0] = path[0];
    drivePath[1] = ':';
    drivePath[2] = '\\';
    drivePath[3] = 0;

    rc = DosSetCurrentDir(drivePath);

    if(rc)
    {
        printf("Cannot change to specified path, not continuing.\n");
        return;
    }

    rc = DosCreateDir("DELETED", NULL);

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = DosSetCurrentDir("DELETED");

    printf("Creating and deleting files.\n");

    for(pos = 0; pos < 64; pos++)
    {
        memset(&filename, 0, 9);
        sprintf(&filename, "%X", pos);
        rc = DosOpen(&filename, &handle, &actionTaken, 0, FILE_NORMAL,
                     OPEN_ACTION_CREATE_IF_NEW | OPEN_ACTION_FAIL_IF_EXISTS,
                     OPEN_FLAGS_NOINHERIT | OPEN_FLAGS_NO_CACHE | OPEN_SHARE_DENYNONE | OPEN_ACCESS_READWRITE, 0);
        if(rc)
            break;

        DosClose(handle);
        DosDelete(&filename);
    }
}
#endif
