/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : scsi_mode.h
Version        : 4.0
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Contains definitions for SCSI MODE PAGEs.

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

#ifndef DISCIMAGECHEF_DEVICE_REPORT_SCSI_MODE_H
#define DISCIMAGECHEF_DEVICE_REPORT_SCSI_MODE_H

typedef struct
{
    uint8_t  Density;
    uint64_t Blocks;
    uint32_t BlockLength;
} BlockDescriptor;

typedef struct
{
    uint8_t         MediumType;
    int             WriteProtected;
    BlockDescriptor BlockDescriptors[4096];
    int             descriptorsLength;
    uint8_t         Speed;
    uint8_t         BufferedMode;
    int             EBC;
    int             DPOFUA;
    int             decoded;
} ModeHeader;

typedef struct
{
    ModeHeader    Header;
    unsigned char *Pages[256][256];
    size_t        pageSizes[256][256];
    int           decoded;
} DecodedMode;

ModeHeader *DecodeModeHeader6(unsigned char *modeResponse, uint8_t deviceType);

ModeHeader *DecodeModeHeader10(unsigned char *modeResponse, uint8_t deviceType);

DecodedMode *DecodeMode6(unsigned char *modeResponse, uint8_t deviceType);

DecodedMode *DecodeMode10(unsigned char *modeResponse, uint8_t deviceType);

#endif //DISCIMAGECHEF_DEVICE_REPORT_SCSI_MODE_H
