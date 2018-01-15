/****************************************************************************
The Disc Image Chef
-----------------------------------------------------------------------------

Filename       : dos.c
Author(s)      : Natalia Portillo

Component      : fstester.setter

--[ Description ] -----------------------------------------------------------

Contains DOS code

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

#if defined(__DOS__) || defined (MSDOS)

#include <i86.h>
#include <direct.h>
#include <io.h>

#include <stdio.h>
#include <string.h>
#include <malloc.h>
#include <stdlib.h>

#include "defs.h"
#include "dos.h"
#include "dosos2.h"
#include "consts.h"

void GetOsInfo()
{
    union REGS regs;
    unsigned char major, minor;

    regs.w.ax = 0x3306;

    int86(0x21, &regs, &regs);
    
    if(regs.h.al == 0xFF || (regs.w.ax == 0x1 && regs.w.cflag))
    {
        memset(&regs, 0, sizeof(regs));
        regs.w.ax = 0x3000;
        int86(0x21, &regs, &regs);
        major = regs.h.al;
        minor = regs.h.ah;
    }
    else
    {
        major = regs.h.bl;
        minor = regs.h.bh;
    }

    if(major == 10 || major == 20)
    {
       printf("Will not run under OS/2. Exiting...\n");
//       exit(1);
    }

    if(major == 5 && minor == 50)
    {
       printf("Will not run under Windows NT. Exiting...\n");
       exit(1);
    }

    if(major == 0)
       major = 1;

    printf("OS information:\n");
    printf("\tRunning under DOS %d.%d\n", major, minor);
}

void GetVolumeInfo(const char *path, size_t *clusterSize)
{
    union REGS regs;
    struct SREGS sregs;
    char   drivePath[4];
    char   driveNo = path[0] - '@';
    struct diskfree_t oldFreeSpace;
    Fat32FreeSpace *freeSpace = malloc(sizeof(Fat32FreeSpace));

    memset(freeSpace, 0, sizeof(Fat32FreeSpace));

    if(driveNo > 32)
       driveNo-=32;

    drivePath[0] = path[0];
    drivePath[1] = ':';
    drivePath[2] = '\\';
    drivePath[3] = 0;

    regs.w.ax = 0x7303;
    sregs.ds = FP_SEG(drivePath);
    regs.w.dx = FP_OFF(drivePath);
    sregs.es = FP_SEG(freeSpace);
    regs.w.di = FP_OFF(freeSpace);
    regs.w.cx = sizeof(Fat32FreeSpace);

    int86x(0x21, &regs, &regs, &sregs);

    if(regs.h.al == 0 && !regs.w.cflag)
    {
       _dos_getdiskfree(driveNo, &oldFreeSpace);
       freeSpace->sectorsPerCluster = oldFreeSpace.sectors_per_cluster;
       freeSpace->freeClusters = oldFreeSpace.avail_clusters;
       freeSpace->bytesPerSector = oldFreeSpace.bytes_per_sector;
       freeSpace->totalClusters = oldFreeSpace.total_clusters;
    }
    else if(regs.w.cflag)
    {
        printf("Error %d requesting volume information.\n", regs.w.ax);
        free(freeSpace);
        return;
    }
    
    if(!regs.w.cflag)
    {
        printf("\tBytes per sector: %lu\n", freeSpace->bytesPerSector);
        printf("\tSectors per cluster: %lu (%lu bytes)\n", freeSpace->sectorsPerCluster,
               freeSpace->sectorsPerCluster * freeSpace->bytesPerSector);
        printf("\tClusters: %lu (%lu bytes)\n", freeSpace->totalClusters,
               freeSpace->sectorsPerCluster * freeSpace->bytesPerSector * freeSpace->totalClusters);
        printf("\tFree clusters: %lu (%lu bytes)\n", freeSpace->freeClusters,
               freeSpace->sectorsPerCluster * freeSpace->bytesPerSector *  freeSpace->freeClusters);

        *clusterSize = freeSpace->sectorsPerCluster * freeSpace->bytesPerSector;
    }
}

void FileAttributes(const char *path)
{
    char   driveNo = path[0] - '@';
    unsigned total, actionTaken;
    int rc, wRc, cRc, handle;

    if(driveNo > 32)
       driveNo-=32;

    _dos_setdrive(driveNo, &total);
    chdir("\\");

    rc = mkdir("ATTRS");

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    chdir("ATTRS");
    
    printf("Creating attributes files.\n");

    rc = _dos_creat("NONE", 0, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)noAttributeText, strlen(noAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("NONE", _A_NORMAL);
    }

    printf("\tFile with no attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "NONE", rc, wRc, cRc);

    rc = _dos_creat("ARCHIVE", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("ARCHIVE", _A_ARCH);
    }

    printf("\tFile with archived attribute: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "ARCHIVE", rc, wRc, cRc);

    rc = _dos_creat("SYSTEM", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("SYSTEM", _A_SYSTEM);
    }

    printf("\tFile with system attribute: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "SYSTEM", rc, wRc, cRc);

    rc = _dos_creat("HIDDEN", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("HIDDEN", _A_HIDDEN);
    }

    printf("\tFile with hidden attribute: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "HIDDEN", rc, wRc, cRc);

    rc = _dos_creat("READONLY", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("READONLY", _A_RDONLY);
    }

    printf("\tFile with read-only attribute: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "READONLY", rc, wRc, cRc);

    rc = _dos_creat("HIDDREAD", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("HIDDREAD", _A_HIDDEN | _A_RDONLY);
    }

    printf("\tFile with hidden, read-only attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "HIDDREAD", rc,
           wRc, cRc);

    rc = _dos_creat("SYSTREAD", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("SYSTREAD", _A_SYSTEM | _A_RDONLY);
    }

    printf("\tFile with system, read-only attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "SYSTREAD", rc,
           wRc, cRc);

    rc = _dos_creat("SYSTHIDD", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("SYSTHIDD", _A_SYSTEM | _A_HIDDEN);
    }

    printf("\tFile with system, hidden attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "SYSTHIDD", rc, wRc,
           cRc);

    rc = _dos_creat("SYSRDYHD", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("SYSRDYHD", _A_SYSTEM | _A_RDONLY | _A_HIDDEN);
    }

    printf("\tFile with system, read-only, hidden attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "SYSRDYHD",
           rc, wRc, cRc);

    rc = _dos_creat("ARCHREAD", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("ARCHREAD", _A_ARCH | _A_RDONLY);
    }

    printf("\tFile with archived, read-only attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "ARCHREAD", rc,
           wRc, cRc);

    rc = _dos_creat("ARCHHIDD", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("ARCHHIDD", _A_ARCH | _A_HIDDEN);
    }

    printf("\tFile with archived, hidden attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "ARCHHIDD", rc, wRc,
           cRc);

    rc = _dos_creat("ARCHDRDY", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("ARCHDRDY", _A_ARCH | _A_HIDDEN | _A_RDONLY);
    }

    printf("\tFile with archived, hidden, read-only attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n",
           "ARCHDRDY", rc, wRc, cRc);

    rc = _dos_creat("ARCHSYST", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("ARCHSYST", _A_ARCH | _A_SYSTEM);
    }

    printf("\tFile with archived, system attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "ARCHSYST", rc, wRc,
           cRc);

    rc = _dos_creat("ARSYSRDY", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("ARSYSRDY", _A_ARCH | _A_SYSTEM | _A_RDONLY);
    }

    printf("\tFile with archived, system, read-only attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n",
           "ARSYSRDY", rc, wRc, cRc);

    rc = _dos_creat("ARCSYSHD", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("ARCSYSHD", _A_ARCH | _A_SYSTEM | _A_HIDDEN);
    }

    printf("\tFile with archived, system, hidden attributes: name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", "ARCSYSHD",
           rc, wRc, cRc);

    rc = _dos_creat("ARSYHDRD", _A_NORMAL, &handle);

    if(!rc)
    {
        wRc = _dos_write(handle, (void *)archivedAttributeText, strlen(archivedAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)systemAttributeText, strlen(systemAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)hiddenAttributeText, strlen(hiddenAttributeText), &actionTaken);
        wRc = _dos_write(handle, (void *)readonlyAttributeText, strlen(readonlyAttributeText), &actionTaken);
        cRc = _dos_close(handle);
        rc  = _dos_setfileattr("ARSYHDRD", _A_ARCH | _A_SYSTEM | _A_HIDDEN | _A_RDONLY);
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
    /* Do nothing, not supported by target operating system */
}

void ResourceFork(const char *path)
{
    /* Do nothing, not supported by target operating system */
}

void Filenames(const char *path)
{
    char   driveNo = path[0] - '@';
    int rc          = 0, wRc = 0, cRc = 0;
    unsigned actionTaken, total;
    int handle;
    char   message[300];
    int    pos         = 0;

    if(driveNo > 32)
       driveNo-=32;

    _dos_setdrive(driveNo, &total);
    chdir("\\");

    rc = mkdir("FILENAME");

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = chdir("FILENAME");

    printf("Creating files with different filenames.\n");

    for(pos = 0; filenames[pos]; pos++)
    {
        rc = _dos_creatnew(filenames[pos], _A_NORMAL, &handle);

        if(!rc)
        {
            memset(&message, 0, 300);
            sprintf(&message, FILENAME_FORMAT, filenames[pos]);
            wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
            cRc = _dos_close(handle);
        }

        printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d\n", filenames[pos], rc, wRc, cRc);
    }
}

#define DATETIME_FORMAT "This file is dated %04d/%02d/%02d %02d:%02d:%02d for %s\n"

void Timestamps(const char *path)
{
    char   driveNo = path[0] - '@';
    int     rc          = 0, wRc = 0, cRc = 0, tRc = 0;
    unsigned actionTaken, total;
    int handle;
    char       message[300];
    union REGS regs;
    unsigned short maxtime = 0xBF7D;
    unsigned short maxdate = 0xFF9F;
    unsigned short y1kdate = 0x2621;
    unsigned short y2kdate = 0x2821;
    unsigned short mindate = 0x0021;

    if(driveNo > 32)                        
       driveNo-=32;

    _dos_setdrive(driveNo, &total);
    chdir("\\");

    rc = mkdir("TIMES");

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = chdir("TIMES");

    printf("Creating timestamped files.\n");

    rc = _dos_creatnew("MAXCTIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(maxdate), MONTH(maxdate), DAY(maxdate),
                HOUR(maxtime), MINUTE(maxtime), SECOND(maxtime), "creation");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = maxtime;
        regs.w.dx = maxdate;
        regs.w.ax = 0x5707;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;                       
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MAXCTIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("MINCTIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(mindate), MONTH(mindate), DAY(mindate),
                HOUR(0), MINUTE(0), SECOND(0), "creation");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = 0;
        regs.w.dx = mindate;
        regs.w.ax = 0x5707;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MINCTIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("Y19CTIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(y1kdate), MONTH(y1kdate), DAY(y1kdate),
                HOUR(maxtime), MINUTE(maxtime), SECOND(maxtime), "creation");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = maxtime;
        regs.w.dx = y1kdate;
        regs.w.ax = 0x5707;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y19CTIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("Y2KCTIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(y2kdate), MONTH(y2kdate), DAY(y2kdate),
                HOUR(0), MINUTE(0), SECOND(0), "creation");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = 0;
        regs.w.dx = y2kdate;
        regs.w.ax = 0x5707;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y19CTIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("MAXWTIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(maxdate), MONTH(maxdate), DAY(maxdate),
                HOUR(maxtime), MINUTE(maxtime), SECOND(maxtime), "last written");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = maxtime;
        regs.w.dx = maxdate;
        regs.w.ax = 0x5701;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MAXWTIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("MINWTIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(mindate), MONTH(mindate), DAY(mindate),
                HOUR(0), MINUTE(0), SECOND(0), "last written");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = 0;
        regs.w.dx = mindate;
        regs.w.ax = 0x5701;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MINWTIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("Y19WTIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(y1kdate), MONTH(y1kdate), DAY(y1kdate),
                HOUR(maxtime), MINUTE(maxtime), SECOND(maxtime), "last written");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = maxtime;
        regs.w.dx = y1kdate;
        regs.w.ax = 0x5701;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y19WTIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("Y2KWTIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(y2kdate), MONTH(y2kdate), DAY(y2kdate),
                HOUR(0), MINUTE(0), SECOND(0), "last written");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = 0;
        regs.w.dx = y2kdate;
        regs.w.ax = 0x5701;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y2KWTIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("MAXATIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(maxdate), MONTH(maxdate), DAY(maxdate),
                HOUR(0), MINUTE(0), SECOND(0), "last access");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = 0;
        regs.w.dx = maxdate;
        regs.w.ax = 0x5705;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MAXATIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("MINATIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(mindate), MONTH(mindate), DAY(mindate),
                HOUR(0), MINUTE(0), SECOND(0), "last access");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = 0;
        regs.w.dx = mindate;
        regs.w.ax = 0x5705;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "MINATIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("Y19ATIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(y1kdate), MONTH(y1kdate), DAY(y1kdate),
                HOUR(0), MINUTE(0), SECOND(0), "last access");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = 0;
        regs.w.dx = y1kdate;
        regs.w.ax = 0x5705;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y19ATIME", rc, wRc, cRc, tRc);

    rc = _dos_creatnew("Y2KATIME", _A_NORMAL, &handle);

    if(!rc)
    {
        memset(&message, 0, 300);
        sprintf(&message, DATETIME_FORMAT, YEAR(y2kdate), MONTH(y2kdate), DAY(y2kdate),
                HOUR(0), MINUTE(0), SECOND(0), "last access");

        wRc = _dos_write(handle, &message, strlen(message), &actionTaken);
        memset(&regs, 0, sizeof(regs));
        regs.w.bx = handle;
        regs.w.cx = 0;
        regs.w.dx = y2kdate;
        regs.w.ax = 0x5705;
        int86(0x21, &regs, &regs);
        tRc = regs.w.ax;
        cRc = _dos_close(handle);
    }

    printf("\tFile name = \"%s\", rc = %d, wRc = %d, cRc = %d, tRc = %d\n", "Y2KATIME", rc, wRc, cRc, tRc);
}

void DirectoryDepth(const char *path)
{
    char   driveNo = path[0] - '@';
    int rc  = 0;
    unsigned total;
    char   filename[9];
    long   pos = 2;

    if(driveNo > 32)                        
       driveNo-=32;

    _dos_setdrive(driveNo, &total);
    chdir("\\");

    rc = mkdir("DEPTH");

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = chdir("DEPTH");

    printf("Creating deepest directory tree.\n");

    while(!rc)
    {
        memset(&filename, 0, 9);
        sprintf(&filename, "%08d", pos);
        rc = mkdir(filename);

        if(!rc)
            rc = chdir(filename);

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
    char   driveNo = path[0] - '@';
    int        rc                      = 0, wRc = 0, cRc = 0;
    unsigned total, actionTaken             = 0;
    int         handle;
    long          i;

    if(driveNo > 32)                        
       driveNo-=32;

    _dos_setdrive(driveNo, &total);
    chdir("\\");

    rc = mkdir("FRAGS");

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = chdir("FRAGS");

    rc = _dos_creatnew("HALFCLST", _A_NORMAL, &handle);
    if(!rc)
    {
        buffer = malloc(halfCluster);
        memset(buffer, 0, halfCluster);

        for(i = 0; i < halfCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = _dos_write(handle, buffer, halfCluster, &actionTaken);
        cRc = _dos_close(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "HALFCLST", halfCluster, rc, wRc, cRc);

    rc = _dos_creatnew("QUARCLST", _A_NORMAL, &handle);
    if(!rc)
    {
        buffer = malloc(quarterCluster);
        memset(buffer, 0, quarterCluster);

        for(i = 0; i < quarterCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = _dos_write(handle, buffer, quarterCluster, &actionTaken);
        cRc = _dos_close(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "QUARCLST", quarterCluster, rc, wRc, cRc);

    rc = _dos_creatnew("TWOCLST", _A_NORMAL, &handle);
    if(!rc)
    {
        buffer = malloc(twoCluster);
        memset(buffer, 0, twoCluster);

        for(i = 0; i < twoCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = _dos_write(handle, buffer, twoCluster, &actionTaken);
        cRc = _dos_close(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TWOCLST", twoCluster, rc, wRc, cRc);

    rc = _dos_creatnew("TRQTCLST", _A_NORMAL, &handle);
    if(!rc)
    {
        buffer = malloc(threeQuartersCluster);
        memset(buffer, 0, threeQuartersCluster);

        for(i = 0; i < threeQuartersCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = _dos_write(handle, buffer, threeQuartersCluster, &actionTaken);
        cRc = _dos_close(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TRQTCLST", threeQuartersCluster, rc, wRc,
           cRc);

    rc = _dos_creatnew("TWTQCLST", _A_NORMAL, &handle);
    if(!rc)
    {
        buffer = malloc(twoAndThreeQuartCluster);
        memset(buffer, 0, twoAndThreeQuartCluster);

        for(i = 0; i < twoAndThreeQuartCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = _dos_write(handle, buffer, twoAndThreeQuartCluster, &actionTaken);
        cRc = _dos_close(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TWTQCLST", twoAndThreeQuartCluster, rc,
           wRc, cRc);

    rc = _dos_creatnew("TWO1", _A_NORMAL, &handle);
    if(!rc)
    {
        buffer = malloc(twoCluster);
        memset(buffer, 0, twoCluster);

        for(i = 0; i < twoCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = _dos_write(handle, buffer, twoCluster, &actionTaken);
        cRc = _dos_close(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TWO1", twoCluster, rc, wRc, cRc);

    rc = _dos_creatnew("TWO2", _A_NORMAL, &handle);
    if(!rc)
    {
        buffer = malloc(twoCluster);
        memset(buffer, 0, twoCluster);

        for(i = 0; i < twoCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = _dos_write(handle, buffer, twoCluster, &actionTaken);
        cRc = _dos_close(handle);
        free(buffer);
    }

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TWO2", twoCluster, rc, wRc, cRc);

    rc = _dos_creatnew("TWO3", _A_NORMAL, &handle);
    if(!rc)
    {
        buffer = malloc(twoCluster);
        memset(buffer, 0, twoCluster);

        for(i = 0; i < twoCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = _dos_write(handle, buffer, twoCluster, &actionTaken);
        cRc = _dos_close(handle);
        free(buffer);
    }

    printf("\tDeleting \"TWO2\".\n");
    rc = unlink("TWO2");         

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "TWO3", twoCluster, rc, wRc, cRc);

    rc = _dos_creatnew("FRAGTHRQ", _A_NORMAL, &handle);
    if(!rc)
    {
        buffer = malloc(threeQuartersCluster);
        memset(buffer, 0, threeQuartersCluster);

        for(i = 0; i < threeQuartersCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = _dos_write(handle, buffer, threeQuartersCluster, &actionTaken);
        cRc = _dos_close(handle);
        free(buffer);
    }

    printf("\tDeleting \"TWO1\".\n");
    rc = unlink("TWO1");
    printf("\tDeleting \"TWO3\".\n");
    rc = unlink("TWO3");

    printf("\tFile name = \"%s\", size = %d, rc = %d, wRc = %d, cRc = %d\n", "FRAGTHRQ", threeQuartersCluster, rc, wRc,
           cRc);

    rc = _dos_creatnew("FRAGSIXQ", _A_NORMAL, &handle);
    if(!rc)
    {
        buffer = malloc(twoAndThreeQuartCluster);
        memset(buffer, 0, twoAndThreeQuartCluster);

        for(i = 0; i < twoAndThreeQuartCluster; i++)
            buffer[i] = clauniaBytes[i % CLAUNIA_SIZE];

        wRc = _dos_write(handle, buffer, twoAndThreeQuartCluster, &actionTaken);
        cRc = _dos_close(handle);
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
    /* Do nothing, not supported by target operating system */
}

void MillionFiles(const char *path)
{
    char   driveNo = path[0] - '@';
    int             rc          = 0;
    char               filename[9];
    unsigned long long pos         = 0;
    int              handle;
    unsigned total;

    if(driveNo > 32)                        
       driveNo-=32;

    _dos_setdrive(driveNo, &total);
    chdir("\\");

    rc = mkdir("MILLION");

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = chdir("MILLION");

    printf("Creating lots of files.\n");

    for(pos = 0; pos < 100000ULL; pos++)
    {
        memset(&filename, 0, 9);
        sprintf(&filename, "%08llu", pos);
        rc = _dos_creatnew(&filename, _A_NORMAL, &handle);
        if(rc)
            break;

        _dos_close(handle);
    }

    printf("\tCreated %llu files\n", pos);
}

void DeleteFiles(const char *path)
{
    char   driveNo = path[0] - '@';
    int rc          = 0;
    char   filename[9];
    short  pos         = 0;
    unsigned total;
    int handle;

    if(driveNo > 32)                        
       driveNo-=32;

    _dos_setdrive(driveNo, &total);
    chdir("\\");

    rc = mkdir("DELETED");

    if(rc)
    {
        printf("Cannot create working directory.\n");
        return;
    }

    rc = chdir("DELETED");

    printf("Creating and deleting files.\n");

    for(pos = 0; pos < 64; pos++)
    {
        memset(&filename, 0, 9);
        sprintf(&filename, "%X", pos);
        rc = _dos_creatnew(&filename, _A_NORMAL, &handle);
        if(rc)
            break;

        _dos_close(handle);
        unlink(&filename);
    }
}
#endif
