/****************************************************************************
The Disc Image Chef
-----------------------------------------------------------------------------

Filename       : main.c
Author(s)      : Natalia Portillo

Component      : fstester.setter

--[ Description ] -----------------------------------------------------------

Contains global definitions

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

#include <stdio.h>

#include "main.h"
#include "defs.h"

int main(int argc, char **argv)
{
    size_t clusterSize = 0;

    printf("The Disc Image Chef Filesystem Tester (Setter) %s\n", DIC_FSTESTER_VERSION);
    printf("%s\n", DIC_COPYRIGHT);

    printf("Running in %s (%s)\n", OS_NAME, OS_ARCH);
    printf("\n");

    if(argc != 2)
    {
        printf("Usage %s <path>\n", argv[0]);
        return -1;
    }

    GetOsInfo();
    GetVolumeInfo(argv[1], &clusterSize);
    FileAttributes(argv[1]);
    FilePermissions(argv[1]);
    ExtendedAttributes(argv[1]);
    ResourceFork(argv[1]);
    Filenames(argv[1]);
    Timestamps(argv[1]);
    DirectoryDepth(argv[1]);
    Fragmentation(argv[1], clusterSize);
    Sparse(argv[1]);
    MillionFiles(argv[1]);
    DeleteFiles(argv[1]);
    GetVolumeInfo(argv[1], &clusterSize);

    return 0;
}

