/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : mmc_report.h
Version        : 4.0
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Contains definitions used in SCSI MultiMedia device reports.

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

#ifndef DISCIMAGECHEF_DEVICE_REPORT_MMC_REPORT_H
#define DISCIMAGECHEF_DEVICE_REPORT_MMC_REPORT_H

void MmcReport(int fd, xmlTextWriterPtr xmlWriter, unsigned char *cdromMode);

typedef struct
{
    int           present;
    size_t        len;
    unsigned char *data;
} FeatureDescriptors;

typedef struct
{
    uint32_t           DataLength;
    uint16_t           CurrentProfile;
    FeatureDescriptors Descriptors[65536];
} SeparatedFeatures;
#endif //DISCIMAGECHEF_DEVICE_REPORT_MMC_REPORT_H
