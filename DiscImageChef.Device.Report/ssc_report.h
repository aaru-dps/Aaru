/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : ssc_report.h
Version        : 4.0
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Contains definitions used in SCSI Streaming device reports.

--[ License ] --------------------------------------------------------------

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2018 Claunia.com
****************************************************************************/

#ifndef DISCIMAGECHEF_DEVICE_REPORT_SSC_REPORT_H
#define DISCIMAGECHEF_DEVICE_REPORT_SSC_REPORT_H

void SscReport(int fd, xmlTextWriterPtr xmlWriter);

#pragma pack(push, 1)
typedef struct
{
    uint8_t       primaryCode;
    uint8_t       secondaryCode;
    uint8_t       dlv : 1;
    uint8_t       reserved : 4;
    uint8_t       deflt : 1;
    uint8_t       dup : 1;
    uint8_t       wrtok : 1;
    uint16_t      length;
    uint8_t       bitsPerMm[3];
    uint16_t      mediaWidth;
    uint16_t      tracks;
    uint32_t      capacity;
    unsigned char organization[8];
    unsigned char densityName[8];
    unsigned char description[20];
} DensityDescriptor;
#pragma pack(pop)

#pragma pack(push, 1)
typedef struct
{
    uint8_t       mediumType;
    uint8_t       reserved;
    uint16_t      length;
    uint8_t       codes_len;
    uint8_t       codes[9];
    uint16_t      mediaWidth;
    uint16_t      mediumLength;
    uint16_t      reserved2;
    unsigned char organization[8];
    unsigned char densityName[8];
    unsigned char description[20];
} MediumDescriptor;
#pragma pack(pop)

typedef struct
{
    uint16_t          count;
    DensityDescriptor *descriptors[1260];
} DensitySupport;

typedef struct
{
    uint16_t         count;
    MediumDescriptor *descriptors[1170];
} MediaTypeSupport;
#endif //DISCIMAGECHEF_DEVICE_REPORT_SSC_REPORT_H
