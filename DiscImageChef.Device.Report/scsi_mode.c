/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : scsi_mode.c
Version        : 4.0
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Contains decoders for SCSI MODE PAGEs.

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

#include <malloc.h>
#include <string.h>
#include <endian.h>
#include <stdint.h>
#include "scsi_mode.h"

ModeHeader *DecodeModeHeader10(unsigned char *modeResponse, uint8_t deviceType)
{
    uint16_t   blockDescLength = (uint16_t)((modeResponse[6] << 8) + modeResponse[7]);
    int        i;
    ModeHeader *header         = malloc(sizeof(ModeHeader));
    memset(header, 0, sizeof(ModeHeader));
    header->MediumType = modeResponse[2];

    int longLBA = (modeResponse[4] & 0x01) == 0x01;

    if(blockDescLength > 0)
    {
        if(longLBA)
        {
            header->descriptorsLength = blockDescLength / 16;
            for(i = 0; i < header->descriptorsLength; i++)
            {
                header->BlockDescriptors[i].Density = 0x00;
                header->BlockDescriptors[i].Blocks  = be64toh((uint64_t)(*modeResponse + 0 + i * 16 + 8));
                header->BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[15 + i * 16 + 8] << 24);
                header->BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[14 + i * 16 + 8] << 16);
                header->BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[13 + i * 16 + 8] << 8);
                header->BlockDescriptors[i].BlockLength += modeResponse[12 + i * 16 + 8];
            }
        }
        else
        {
            header->descriptorsLength = blockDescLength / 8;
            for(i = 0; i < header->descriptorsLength; i++)
            {
                if(deviceType != 0x00)
                {
                    header->BlockDescriptors[i].Density = modeResponse[0 + i * 8 + 8];
                }
                else
                {
                    header->BlockDescriptors[i].Density = 0x00;
                    header->BlockDescriptors[i].Blocks += (uint64_t)(modeResponse[0 + i * 8 + 8] << 24);
                }
                header->BlockDescriptors[i].Blocks += (uint64_t)(modeResponse[1 + i * 8 + 8] << 16);
                header->BlockDescriptors[i].Blocks += (uint64_t)(modeResponse[2 + i * 8 + 8] << 8);
                header->BlockDescriptors[i].Blocks += modeResponse[3 + i * 8 + 8];
                header->BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[5 + i * 8 + 8] << 16);
                header->BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[6 + i * 8 + 8] << 8);
                header->BlockDescriptors[i].BlockLength += modeResponse[7 + i * 8 + 8];
            }
        }
    }

    if(deviceType == 0x00 || deviceType == 0x05)
    {
        header->WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
        header->DPOFUA         = ((modeResponse[3] & 0x10) == 0x10);
    }

    if(deviceType == 0x01)
    {
        header->WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
        header->Speed          = (uint8_t)(modeResponse[3] & 0x0F);
        header->BufferedMode   = (uint8_t)((modeResponse[3] & 0x70) >> 4);
    }

    if(deviceType == 0x02)
        header->BufferedMode = (uint8_t)((modeResponse[3] & 0x70) >> 4);

    if(deviceType == 0x07)
    {
        header->WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
        header->EBC            = ((modeResponse[3] & 0x01) == 0x01);
        header->DPOFUA         = ((modeResponse[3] & 0x10) == 0x10);
    }

    header->decoded = 1;

    return header;
}

DecodedMode *DecodeMode10(unsigned char *modeResponse, uint8_t deviceType)
{
    DecodedMode *decodedMode = malloc(sizeof(DecodedMode));

    ModeHeader *hdrPtr = DecodeModeHeader10(modeResponse, deviceType);
    memcpy(&(decodedMode->Header), hdrPtr, sizeof(ModeHeader));
    free(hdrPtr);

    if(!decodedMode->Header.decoded)
        return decodedMode;

    decodedMode->decoded = 1;

    int longlba = (modeResponse[4] & 0x01) == 0x01;
    int offset;

    if(longlba)
        offset = 8 + decodedMode->Header.descriptorsLength * 16;
    else
        offset = 8 + decodedMode->Header.descriptorsLength * 8;
    int length = (modeResponse[0] << 8);
    length += modeResponse[1];
    length += 2;

    while(offset < length)
    {
        int isSubpage = (modeResponse[offset] & 0x40) == 0x40;

        uint8_t pageNo = (uint8_t)(modeResponse[offset] & 0x3F);
        int     subpage;

        if(pageNo == 0)
        {
            decodedMode->pageSizes[0][0] = (size_t)(length - offset);
            decodedMode->Pages[0][0]     = malloc(decodedMode->pageSizes[0][0]);
            memset(decodedMode->Pages[0][0], 0, decodedMode->pageSizes[0][0]);
            memcpy(decodedMode->Pages[0][0], modeResponse + offset, decodedMode->pageSizes[0][0]);
            offset += decodedMode->pageSizes[0][0];
        }
        else
        {
            if(isSubpage)
            {
                if(offset + 3 >= length)
                    break;

                pageNo  = (uint8_t)(modeResponse[offset] & 0x3F);
                subpage = modeResponse[offset + 1];
                decodedMode->pageSizes[pageNo][subpage] = (size_t)((modeResponse[offset + 2] << 8) +
                                                                   modeResponse[offset + 3] + 4);
                decodedMode->Pages[pageNo][subpage]     = malloc(decodedMode->pageSizes[pageNo][subpage]);
                memset(decodedMode->Pages[pageNo][subpage], 0, decodedMode->pageSizes[pageNo][subpage]);
                memcpy(decodedMode->Pages[pageNo][subpage], modeResponse + offset,
                       decodedMode->pageSizes[pageNo][subpage]);
                offset += decodedMode->pageSizes[pageNo][subpage];
            }
            else
            {
                if(offset + 1 >= length)
                    break;

                pageNo = (uint8_t)(modeResponse[offset] & 0x3F);
                decodedMode->pageSizes[pageNo][0] = (size_t)(modeResponse[offset + 1] + 2);
                decodedMode->Pages[pageNo][0]     = malloc(decodedMode->pageSizes[pageNo][0]);
                memset(decodedMode->Pages[pageNo][0], 0, decodedMode->pageSizes[pageNo][0]);
                memcpy(decodedMode->Pages[pageNo][0], modeResponse + offset, decodedMode->pageSizes[pageNo][0]);
                offset += decodedMode->pageSizes[pageNo][0];
            }
        }
    }

    return decodedMode;
}


ModeHeader *DecodeModeHeader6(unsigned char *modeResponse, uint8_t deviceType)
{
    int        i;
    ModeHeader *header = malloc(sizeof(ModeHeader));
    memset(header, 0, sizeof(ModeHeader));

    if(modeResponse[3])
    {
        header->descriptorsLength = modeResponse[3] / 8;
        for(i = 0; i < header->descriptorsLength; i++)
        {
            header->BlockDescriptors[i].Density = modeResponse[0 + i * 8 + 4];
            header->BlockDescriptors[i].Blocks += (uint64_t)(modeResponse[1 + i * 8 + 4] << 16);
            header->BlockDescriptors[i].Blocks += (uint64_t)(modeResponse[2 + i * 8 + 4] << 8);
            header->BlockDescriptors[i].Blocks += modeResponse[3 + i * 8 + 4];
            header->BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[5 + i * 8 + 4] << 16);
            header->BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[6 + i * 8 + 4] << 8);
            header->BlockDescriptors[i].BlockLength += modeResponse[7 + i * 8 + 4];
        }
    }

    if(deviceType == 0x00 || deviceType == 0x05)
    {
        header->WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
        header->DPOFUA         = ((modeResponse[2] & 0x10) == 0x10);
    }

    if(deviceType == 0x01)
    {
        header->WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
        header->Speed          = (uint8_t)(modeResponse[2] & 0x0F);
        header->BufferedMode   = (uint8_t)((modeResponse[2] & 0x70) >> 4);
    }

    if(deviceType == 0x02)
        header->BufferedMode = (uint8_t)((modeResponse[2] & 0x70) >> 4);

    if(deviceType == 0x07)
    {
        header->WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
        header->EBC            = ((modeResponse[2] & 0x01) == 0x01);
        header->DPOFUA         = ((modeResponse[2] & 0x10) == 0x10);
    }

    header->decoded = 1;

    return header;
}

DecodedMode *DecodeMode6(unsigned char *modeResponse, uint8_t deviceType)
{
    DecodedMode *decodedMode = malloc(sizeof(DecodedMode));

    ModeHeader *hdrPtr = DecodeModeHeader6(modeResponse, deviceType);
    memcpy(&(decodedMode->Header), hdrPtr, sizeof(ModeHeader));
    free(hdrPtr);

    if(!decodedMode->Header.decoded)
        return decodedMode;

    decodedMode->decoded = 1;

    int offset = 4 + decodedMode->Header.descriptorsLength * 8;
    int length = modeResponse[0] + 1;

    while(offset < length)
    {
        int isSubpage = (modeResponse[offset] & 0x40) == 0x40;

        uint8_t pageNo = (uint8_t)(modeResponse[offset] & 0x3F);
        int     subpage;

        if(pageNo == 0)
        {
            decodedMode->pageSizes[0][0] = (size_t)(length - offset);
            decodedMode->Pages[0][0]     = malloc(decodedMode->pageSizes[0][0]);
            memset(decodedMode->Pages[0][0], 0, decodedMode->pageSizes[0][0]);
            memcpy(decodedMode->Pages[0][0], modeResponse + offset, decodedMode->pageSizes[0][0]);
            offset += decodedMode->pageSizes[0][0];
        }
        else
        {
            if(isSubpage)
            {
                if(offset + 3 >= length)
                    break;

                pageNo  = (uint8_t)(modeResponse[offset] & 0x3F);
                subpage = modeResponse[offset + 1];
                decodedMode->pageSizes[pageNo][subpage] = (size_t)((modeResponse[offset + 2] << 8) +
                                                                   modeResponse[offset + 3] + 4);
                decodedMode->Pages[pageNo][subpage]     = malloc(decodedMode->pageSizes[pageNo][subpage]);
                memset(decodedMode->Pages[pageNo][subpage], 0, decodedMode->pageSizes[pageNo][subpage]);
                memcpy(decodedMode->Pages[pageNo][subpage], modeResponse + offset,
                       decodedMode->pageSizes[pageNo][subpage]);
                offset += decodedMode->pageSizes[pageNo][subpage];
            }
            else
            {
                if(offset + 1 >= length)
                    break;

                pageNo = (uint8_t)(modeResponse[offset] & 0x3F);
                decodedMode->pageSizes[pageNo][0] = (size_t)(modeResponse[offset + 1] + 2);
                decodedMode->Pages[pageNo][0]     = malloc(decodedMode->pageSizes[pageNo][0]);
                memset(decodedMode->Pages[pageNo][0], 0, decodedMode->pageSizes[pageNo][0]);
                memcpy(decodedMode->Pages[pageNo][0], modeResponse + offset, decodedMode->pageSizes[pageNo][0]);
                offset += decodedMode->pageSizes[pageNo][0];
            }
        }
    }

    return decodedMode;
}