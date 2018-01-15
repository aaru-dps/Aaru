/****************************************************************************
The Disc Image Chef
-----------------------------------------------------------------------------

Filename       : dos.h
Author(s)      : Natalia Portillo

Component      : fstester.setter

--[ Description ] -----------------------------------------------------------

Contains DOS definitions

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

#ifndef DIC_FSTESTER_SETTER_DOS_H
#define DIC_FSTESTER_SETTER_DOS_H

#pragma pack(__push, 1)

typedef struct _Fat32FreeSpace
{
    unsigned short size;
    unsigned short version;
    unsigned long  sectorsPerCluster;
    unsigned long  bytesPerSector;
    unsigned long  freeClusters;
    unsigned long  totalClusters;
    unsigned long  freeSectors;
    unsigned long  totalSectors;
    unsigned long  freeUnits;
    unsigned long  totalUnits;
    unsigned char  reserved[8];
} Fat32FreeSpace;

#pragma pack(__pop)

#define YEAR(t) (((t & 0xFE00) >> 9) + 1980)
#define MONTH(t) ((t & 0x01E0) >> 5)
#define DAY(t) (t & 0x001F)
#define HOUR(t) ((t & 0xF800) >> 11)
#define MINUTE(t) ((t & 0x07E0) >> 5)
#define SECOND(t) ((t & 0x001F) << 1)

#endif

#endif
