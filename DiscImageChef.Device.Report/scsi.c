/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : scsi.c
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Contains SCSI commands.

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
Copyright © 2011-2019 Natalia Portillo
****************************************************************************/

#include <unitypes.h>
#include <malloc.h>
#include <scsi/sg.h>
#include <sys/ioctl.h>
#include <errno.h>
#include <string.h>
#include "scsi.h"

#define FALSE 0
#define TRUE 1

int SendScsiCommand(int fd, void *cdb, unsigned char cdb_len, unsigned char *buffer, unsigned int buffer_len,
                    unsigned char **senseBuffer, int direction)
{
    if(buffer == NULL || cdb == NULL)
        return -1;

    *senseBuffer = malloc(32);
    memset(*senseBuffer, 0, 32);

    sg_io_hdr_t io_hdr;
    memset(&io_hdr, 0, sizeof(sg_io_hdr_t));

    io_hdr.interface_id    = 'S';
    io_hdr.cmd_len         = cdb_len;
    io_hdr.mx_sb_len       = 32;
    io_hdr.dxfer_direction = direction;
    io_hdr.dxfer_len       = buffer_len;
    io_hdr.dxferp          = buffer;
    io_hdr.cmdp            = cdb;
    io_hdr.sbp             = *senseBuffer;
    io_hdr.timeout         = 10000;

    int error = ioctl(fd, SG_IO, &io_hdr);

    if(error < 0)
        error = errno;
    else if(io_hdr.status != 0)
        error = io_hdr.status;
    else if(io_hdr.host_status != 0)
        error = io_hdr.host_status;
    else if(io_hdr.info != 0)
        error = io_hdr.info & SG_INFO_OK_MASK;

    return error;
}

int Inquiry(int fd, unsigned char **buffer, unsigned char **senseBuffer)
{
    unsigned char cmd_len = 6;
    *buffer = malloc(36);
    memset(*buffer, 0, 36);
    char cdb[] = {SCSI_INQUIRY, 0, 0, 0, 36, 0};

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, 36, senseBuffer, SG_DXFER_FROM_DEV);

    if(error)
        return error;

    unsigned char pagesLength = *(*buffer + 4) + 5;

    free(*buffer);
    *buffer = malloc(pagesLength);
    memset(*buffer, 0, pagesLength);

    cdb[4] = pagesLength;
    error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, pagesLength, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int PreventMediumRemoval(int fd, unsigned char **senseBuffer)
{
    return PreventAllowMediumRemoval(fd, senseBuffer, FALSE, TRUE);
}

int AllowMediumRemoval(int fd, unsigned char **senseBuffer)
{
    return PreventAllowMediumRemoval(fd, senseBuffer, FALSE, FALSE);
}

int PreventAllowMediumRemoval(int fd, unsigned char **senseBuffer, int persistent, int prevent)
{
    unsigned char cmd_len = 6;
    char          cdb[]   = {SCSI_PREVENT_ALLOW_MEDIUM_REMOVAL, 0, 0, 0, 0, 0};
    unsigned char *buffer = malloc(0);

    if(prevent)
        cdb[4] += 0x01;
    if(persistent)
        cdb[4] += 0x02;

    int error = SendScsiCommand(fd, &cdb, cmd_len, buffer, 0, senseBuffer, SG_DXFER_NONE);

    return error;
}

int LoadTray(int fd, unsigned char **senseBuffer)
{
    return StartStopUnit(fd, senseBuffer, FALSE, 0, 0, FALSE, TRUE, TRUE);
}

int EjectTray(int fd, unsigned char **senseBuffer)
{
    return StartStopUnit(fd, senseBuffer, FALSE, 0, 0, FALSE, TRUE, FALSE);
}

int StartUnit(int fd, unsigned char **senseBuffer)
{
    return StartStopUnit(fd, senseBuffer, FALSE, 0, 0, FALSE, FALSE, TRUE);
}

int StopUnit(int fd, unsigned char **senseBuffer)
{
    return StartStopUnit(fd, senseBuffer, FALSE, 0, 0, FALSE, FALSE, FALSE);
}

int StartStopUnit(int fd, unsigned char **senseBuffer, int immediate, uint8_t formatLayer, uint8_t powerConditions,
                  int changeFormatLayer, int loadEject, int start)
{
    unsigned char cmd_len = 6;
    char          cdb[]   = {SCSI_START_STOP_UNIT, 0, 0, 0, 0, 0};
    unsigned char *buffer = malloc(0);

    if(immediate)
        cdb[1] += 0x01;
    if(changeFormatLayer)
    {
        cdb[3] = (formatLayer & 0x03);
        cdb[4] += 0x04;
    }
    else
    {
        if(loadEject)
            cdb[4] += 0x02;
        if(start)
            cdb[4] += 0x01;
    }
    cdb[4] += ((powerConditions & 0x0F) << 4);

    int error = SendScsiCommand(fd, &cdb, cmd_len, buffer, 0, senseBuffer, SG_DXFER_NONE);

    return error;
}

int SpcPreventMediumRemoval(int fd, unsigned char **senseBuffer)
{
    return SpcPreventAllowMediumRemoval(fd, senseBuffer, 0x01);
}

int SpcAllowMediumRemoval(int fd, unsigned char **senseBuffer)
{
    return SpcPreventAllowMediumRemoval(fd, senseBuffer, 0x00);
}

int SpcPreventAllowMediumRemoval(int fd, unsigned char **senseBuffer, uint8_t preventMode)
{
    unsigned char cmd_len = 6;
    char          cdb[]   = {SCSI_PREVENT_ALLOW_MEDIUM_REMOVAL, 0, 0, 0, 0, 0};
    unsigned char *buffer = malloc(0);
    cdb[4] = (preventMode & 0x03);

    int error = SendScsiCommand(fd, &cdb, cmd_len, buffer, 0, senseBuffer, SG_DXFER_NONE);

    return error;
}

int Load(int fd, unsigned char **senseBuffer)
{
    return LoadUnload(fd, senseBuffer, FALSE, TRUE, FALSE, FALSE, FALSE);
}

int Unload(int fd, unsigned char **senseBuffer)
{
    return LoadUnload(fd, senseBuffer, FALSE, FALSE, FALSE, FALSE, FALSE);
}

int LoadUnload(int fd, unsigned char **senseBuffer, int immediate, int load, int retense, int endOfTape, int hold)
{
    unsigned char cmd_len = 6;
    char          cdb[]   = {SCSI_LOAD_UNLOAD, 0, 0, 0, 0, 0};
    unsigned char *buffer = malloc(0);
    if(immediate)
        cdb[1] = 0x01;
    if(load)
        cdb[4] += 0x01;
    if(retense)
        cdb[4] += 0x02;
    if(endOfTape)
        cdb[4] += 0x04;
    if(hold)
        cdb[4] += 0x08;

    int error = SendScsiCommand(fd, &cdb, cmd_len, buffer, 0, senseBuffer, SG_DXFER_NONE);

    return error;
}

int
ModeSense6(int fd, unsigned char **buffer, unsigned char **senseBuffer, int DBD, uint8_t pageControl, uint8_t pageCode,
           uint8_t subPageCode)
{
    unsigned char cmd_len    = 6;
    unsigned int  buffer_len = 255;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_MODE_SENSE, 0, 0, 0, 0, 0};
    if(DBD)
        cdb[1] |= 0x08;
    cdb[2] |= pageControl;
    cdb[2] |= (pageCode & 0x3F);
    cdb[3]              = subPageCode;
    cdb[4]              = (uint8_t)(buffer_len & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    if(error)
        return error;

    buffer_len = (unsigned int)*(*buffer + 0) + 1;

    free(*buffer);
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    cdb[4] = (uint8_t)(buffer_len & 0xFF);

    error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ModeSense10(int fd, unsigned char **buffer, unsigned char **senseBuffer, int LLBAA, int DBD, uint8_t pageControl,
                uint8_t pageCode, uint8_t subPageCode)
{
    unsigned char cmd_len    = 10;
    unsigned int  buffer_len = 4096;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_MODE_SENSE_10, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    if(LLBAA)
        cdb[1] |= 0x10;
    if(DBD)
        cdb[1] |= 0x08;
    cdb[2] |= pageControl;
    cdb[2] |= (pageCode & 0x3F);
    cdb[3]              = subPageCode;
    cdb[7]              = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[8]              = (uint8_t)(buffer_len & 0xFF);
    cdb[9]              = 0;

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    if(error)
        return error;

    buffer_len = (unsigned int)(*(*buffer + 0) << 8) + *(*buffer + 1) + 2;

    free(*buffer);
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    cdb[7] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[8] = (uint8_t)(buffer_len & 0xFF);

    error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReadCapacity(int fd, unsigned char **buffer, unsigned char **senseBuffer, int RelAddr, uint32_t address, int PMI)
{
    unsigned char cmd_len    = 10;
    unsigned int  buffer_len = 8;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_READ_CAPACITY, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    if(PMI)
    {
        cdb[8]     = 0x01;
        if(RelAddr)
            cdb[1] = 0x01;

        cdb[2] = (uint8_t)((address & 0xFF000000) >> 24);
        cdb[3] = (uint8_t)((address & 0xFF0000) >> 16);
        cdb[4] = (uint8_t)((address & 0xFF00) >> 8);
        cdb[5] = (uint8_t)(address & 0xFF);
    }

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReadCapacity16(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint64_t address, int PMI)
{
    unsigned char cmd_len    = 16;
    unsigned int  buffer_len = 32;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_SERVICE_ACTION_IN, SCSI_READ_CAPACITY_16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    if(PMI)
    {
        cdb[14] = 0x01;
        cdb[2]  = (uint8_t)((address & 0xFF00000000000000ULL) >> 56);
        cdb[3]  = (uint8_t)((address & 0xFF000000000000ULL) >> 48);
        cdb[4]  = (uint8_t)((address & 0xFF0000000000ULL) >> 40);
        cdb[5]  = (uint8_t)((address & 0xFF00000000ULL) >> 32);
        cdb[6]  = (uint8_t)((address & 0xFF000000ULL) >> 24);
        cdb[7]  = (uint8_t)((address & 0xFF0000ULL) >> 16);
        cdb[8]  = (uint8_t)((address & 0xFF00ULL) >> 8);
        cdb[9]  = (uint8_t)(address & 0xFFULL);
    }

    cdb[10] = (uint8_t)((buffer_len & 0xFF000000) >> 24);
    cdb[11] = (uint8_t)((buffer_len & 0xFF0000) >> 16);
    cdb[12] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[13] = (uint8_t)(buffer_len & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int Read6(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t blockSize,
          uint8_t transferLength)
{
    unsigned char cmd_len = 6;
    unsigned int  buflen  = transferLength == 0 ? 256 * blockSize : transferLength * blockSize;
    *buffer = malloc(buflen);
    memset(*buffer, 0, buflen);

    unsigned char cdb[] = {SCSI_READ, 0, 0, 0, 0, 0};

    cdb[1] = (uint8_t)((lba & 0x1F0000) >> 16);
    cdb[2] = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[3] = (uint8_t)(lba & 0xFF);
    cdb[4] = transferLength;

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buflen, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int Read10(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t rdprotect, int dpo, int fua, int fuaNv,
           int relAddr, uint32_t lba, uint32_t blockSize, uint8_t groupNumber, uint16_t transferLength)
{
    unsigned char cmd_len = 10;
    unsigned int  buflen  = transferLength * blockSize;
    *buffer = malloc(buflen);
    memset(*buffer, 0, buflen);

    unsigned char cdb[] = {SCSI_READ_10, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[1] = (uint8_t)((rdprotect & 0x07) << 5);
    if(dpo)
        cdb[1] += 0x10;
    if(fua)
        cdb[1] += 0x08;
    if(fuaNv)
        cdb[1] += 0x02;
    if(relAddr)
        cdb[1] += 0x01;
    cdb[2] = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[3] = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4] = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5] = (uint8_t)(lba & 0xFF);
    cdb[6] = (uint8_t)(groupNumber & 0x1F);
    cdb[7] = (uint8_t)((transferLength & 0xFF00) >> 8);
    cdb[8] = (uint8_t)(transferLength & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buflen, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int Read12(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t rdprotect, int dpo, int fua, int fuaNv,
           int relAddr, uint32_t lba, uint32_t blockSize, uint8_t groupNumber, uint32_t transferLength, int streaming)
{
    unsigned char cmd_len = 12;
    unsigned int  buflen  = transferLength * blockSize;
    *buffer = malloc(buflen);
    memset(*buffer, 0, buflen);

    unsigned char cdb[] = {SCSI_READ_12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[1]  = (uint8_t)((rdprotect & 0x07) << 5);
    if(dpo)
        cdb[1] += 0x10;
    if(fua)
        cdb[1] += 0x08;
    if(fuaNv)
        cdb[1] += 0x02;
    if(relAddr)
        cdb[1] += 0x01;
    cdb[2]  = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[3]  = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4]  = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5]  = (uint8_t)(lba & 0xFF);
    cdb[6]  = (uint8_t)((transferLength & 0xFF000000) >> 24);
    cdb[7]  = (uint8_t)((transferLength & 0xFF0000) >> 16);
    cdb[8]  = (uint8_t)((transferLength & 0xFF00) >> 8);
    cdb[9]  = (uint8_t)(transferLength & 0xFF);
    cdb[10] = (uint8_t)(groupNumber & 0x1F);
    if(streaming)
        cdb[10] += 0x80;

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buflen, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int Read16(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t rdprotect, int dpo, int fua, int fuaNv,
           uint64_t lba, uint32_t blockSize, uint8_t groupNumber, uint32_t transferLength, int streaming)
{
    unsigned char cmd_len = 16;
    unsigned int  buflen  = transferLength * blockSize;
    *buffer = malloc(buflen);
    memset(*buffer, 0, buflen);

    unsigned char cdb[] = {SCSI_READ_16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[1]  = (uint8_t)((rdprotect & 0x07) << 5);
    if(dpo)
        cdb[1] += 0x10;
    if(fua)
        cdb[1] += 0x08;
    if(fuaNv)
        cdb[1] += 0x02;
    cdb[2]  = (uint8_t)((lba & 0xFF00000000000000ULL) >> 56);
    cdb[3]  = (uint8_t)((lba & 0xFF000000000000ULL) >> 48);
    cdb[4]  = (uint8_t)((lba & 0xFF0000000000ULL) >> 40);
    cdb[5]  = (uint8_t)((lba & 0xFF00000000ULL) >> 32);
    cdb[6]  = (uint8_t)((lba & 0xFF000000ULL) >> 24);
    cdb[7]  = (uint8_t)((lba & 0xFF0000ULL) >> 16);
    cdb[8]  = (uint8_t)((lba & 0xFF00ULL) >> 8);
    cdb[9]  = (uint8_t)(lba & 0xFFULL);
    cdb[10] = (uint8_t)((transferLength & 0xFF000000) >> 24);
    cdb[11] = (uint8_t)((transferLength & 0xFF0000) >> 16);
    cdb[12] = (uint8_t)((transferLength & 0xFF00) >> 8);
    cdb[13] = (uint8_t)(transferLength & 0xFF);
    cdb[14] = (uint8_t)(groupNumber & 0x1F);
    if(streaming)
        cdb[14] += 0x80;

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buflen, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReadLong10(int fd, unsigned char **buffer, unsigned char **senseBuffer, int correct, int relAddr, uint32_t lba,
               uint16_t transferBytes)
{
    unsigned char cmd_len = 10;
    *buffer = malloc(transferBytes);
    memset(*buffer, 0, transferBytes);

    unsigned char cdb[] = {SCSI_READ_LONG, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    if(correct)
        cdb[1] += 0x02;
    if(relAddr)
        cdb[1] += 0x01;
    cdb[2]              = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[3]              = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4]              = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5]              = (uint8_t)(lba & 0xFF);
    cdb[7]              = (uint8_t)((transferBytes & 0xFF00) >> 8);
    cdb[8]              = (uint8_t)(transferBytes & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, transferBytes, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReadLong16(int fd, unsigned char **buffer, unsigned char **senseBuffer, int correct, uint64_t lba,
               uint32_t transferBytes)
{
    unsigned char cmd_len = 16;
    *buffer = malloc(transferBytes);
    memset(*buffer, 0, transferBytes);

    unsigned char cdb[] = {SCSI_SERVICE_ACTION_IN, SCSI_READ_LONG_16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[2]  = (uint8_t)((lba & 0xFF00000000000000ULL) >> 56);
    cdb[3]  = (uint8_t)((lba & 0xFF000000000000ULL) >> 48);
    cdb[4]  = (uint8_t)((lba & 0xFF0000000000ULL) >> 40);
    cdb[5]  = (uint8_t)((lba & 0xFF00000000ULL) >> 32);
    cdb[6]  = (uint8_t)((lba & 0xFF000000ULL) >> 24);
    cdb[7]  = (uint8_t)((lba & 0xFF0000ULL) >> 16);
    cdb[8]  = (uint8_t)((lba & 0xFF00ULL) >> 8);
    cdb[9]  = (uint8_t)(lba & 0xFFULL);
    cdb[12] = (uint8_t)((transferBytes & 0xFF00) >> 8);
    cdb[13] = (uint8_t)(transferBytes & 0xFF);
    if(correct)
        cdb[14] += 0x01;

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, transferBytes, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int Seek6(int fd, unsigned char **senseBuffer, uint32_t lba)
{
    unsigned char cmd_len = 6;
    char          cdb[]   = {SCSI_SEEK, 0, 0, 0, 0, 0};
    unsigned char *buffer = malloc(0);

    cdb[1] = (uint8_t)((lba & 0x1F0000) >> 16);
    cdb[2] = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[3] = (uint8_t)(lba & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, buffer, 0, senseBuffer, SG_DXFER_NONE);

    return error;
}

int Seek10(int fd, unsigned char **senseBuffer, uint32_t lba)
{
    unsigned char cmd_len = 10;
    char          cdb[]   = {SCSI_SEEK_10, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    unsigned char *buffer = malloc(0);

    cdb[2] = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[3] = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4] = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5] = (uint8_t)(lba & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, buffer, 0, senseBuffer, SG_DXFER_NONE);

    return error;
}

int TestUnitReady(int fd, unsigned char **senseBuffer)
{
    unsigned char cmd_len = 6;
    char          cdb[]   = {SCSI_TEST_UNIT_READY, 0, 0, 0, 0, 0};
    unsigned char *buffer = malloc(0);

    int error = SendScsiCommand(fd, &cdb, cmd_len, buffer, 0, senseBuffer, SG_DXFER_NONE);

    return error;
}

int GetConfiguration(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint16_t startingFeatureNumber,
                     uint8_t RT)
{
    unsigned char cmd_len    = 10;
    uint16_t      buffer_len = 8;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    char cdb[] = {MMC_GET_CONFIGURATION, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[1] = (uint8_t)(RT & 0x03);
    cdb[2] = (uint8_t)((startingFeatureNumber & 0xFF00) >> 8);
    cdb[3] = (uint8_t)(startingFeatureNumber & 0xFF);
    cdb[7] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[8] = (uint8_t)(buffer_len & 0xFF);
    cdb[9] = 0;

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    if(error)
        return error;

    buffer_len = (uint16_t)(*(*buffer + 2) << 8) + *(*buffer + 3) + 2;
    cdb[7] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[8] = (uint8_t)(buffer_len & 0xFF);

    free(*buffer);
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReadTocPmaAtip(int fd, unsigned char **buffer, unsigned char **senseBuffer, int MSF, uint8_t format,
                   uint8_t trackSessionNumber)
{
    unsigned char cmd_len    = 10;
    uint16_t      buffer_len = 1024;
    char          cdb[]      = {MMC_READ_TOC_PMA_ATIP, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    if((format & 0xF) == 5)
        buffer_len = 32768;

    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    if(MSF)
        cdb[1] = 0x02;
    cdb[2]     = (uint8_t)(format & 0x0F);
    cdb[6]     = trackSessionNumber;
    cdb[7]     = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[8]     = (uint8_t)(buffer_len & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    if(error)
        return error;

    buffer_len = (uint16_t)(*(*buffer + 0) << 8) + *(*buffer + 1) + 2;
    cdb[7] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[8] = (uint8_t)(buffer_len & 0xFF);

    free(*buffer);
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReadDiscStructure(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t mediaType, uint32_t address,
                      uint8_t layerNumber, uint8_t format, uint8_t AGID)
{
    unsigned char cmd_len    = 12;
    uint16_t      buffer_len = 8;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    char cdb[] = {MMC_READ_DISC_STRUCTURE, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[1]  = (uint8_t)((uint8_t)mediaType & 0x0F);
    cdb[2]  = (uint8_t)((address & 0xFF000000) >> 24);
    cdb[3]  = (uint8_t)((address & 0xFF0000) >> 16);
    cdb[4]  = (uint8_t)((address & 0xFF00) >> 8);
    cdb[5]  = (uint8_t)(address & 0xFF);
    cdb[6]  = layerNumber;
    cdb[7]  = (uint8_t)format;
    cdb[8]  = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[9]  = (uint8_t)(buffer_len & 0xFF);
    cdb[10] = (uint8_t)((AGID & 0x03) << 6);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    if(error)
        return error;

    buffer_len = (uint16_t)(*(*buffer + 0) << 8) + *(*buffer + 1) + 2;
    cdb[8] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[9] = (uint8_t)(buffer_len & 0xFF);

    free(*buffer);
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReadCd(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t blockSize,
           uint32_t transferLength, uint8_t expectedSectorType, int DAP, int relAddr, int sync, uint8_t headerCodes,
           int userData, int edcEcc, uint8_t C2Error, uint8_t subchannel)
{
    unsigned char cmd_len    = 12;
    uint32_t      buffer_len = transferLength * blockSize;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    char cdb[] = {MMC_READ_CD, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[1]  = (uint8_t)((uint8_t)expectedSectorType << 2);
    if(DAP)
        cdb[1] += 0x02;
    if(relAddr)
        cdb[1] += 0x01;
    cdb[2]  = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[3]  = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4]  = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5]  = (uint8_t)(lba & 0xFF);
    cdb[6]  = (uint8_t)((transferLength & 0xFF0000) >> 16);
    cdb[7]  = (uint8_t)((transferLength & 0xFF00) >> 8);
    cdb[8]  = (uint8_t)(transferLength & 0xFF);
    cdb[9]  = (uint8_t)((uint8_t)C2Error << 1);
    cdb[9] += (uint8_t)((uint8_t)headerCodes << 5);
    if(sync)
        cdb[9] += 0x80;
    if(userData)
        cdb[9] += 0x10;
    if(edcEcc)
        cdb[9] += 0x08;
    cdb[10] = (uint8_t)subchannel;

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReadCdMsf(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t startMsf, uint32_t endMsf,
              uint32_t blockSize, uint8_t expectedSectorType, int DAP, int sync, uint8_t headerCodes, int userData,
              int edcEcc, uint8_t C2Error, uint8_t subchannel)
{
    unsigned char cmd_len = 12;
    char          cdb[]   = {MMC_READ_CD_MSF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[1]  = (uint8_t)((uint8_t)expectedSectorType << 2);
    if(DAP)
        cdb[1] += 0x02;
    cdb[3]  = (uint8_t)((startMsf & 0xFF0000) >> 16);
    cdb[4]  = (uint8_t)((startMsf & 0xFF00) >> 8);
    cdb[5]  = (uint8_t)(startMsf & 0xFF);
    cdb[6]  = (uint8_t)((endMsf & 0xFF0000) >> 16);
    cdb[7]  = (uint8_t)((endMsf & 0xFF00) >> 8);
    cdb[8]  = (uint8_t)(endMsf & 0xFF);
    cdb[9]  = (uint8_t)((uint8_t)C2Error << 1);
    cdb[9] += (uint8_t)((uint8_t)headerCodes << 5);
    if(sync)
        cdb[9] += 0x80;
    if(userData)
        cdb[9] += 0x10;
    if(edcEcc)
        cdb[9] += 0x08;
    cdb[10] = (uint8_t)subchannel;

    uint32_t      transferLength = (uint32_t)((cdb[6] - cdb[3]) * 60 * 75 + (cdb[7] - cdb[4]) * 75 + (cdb[8] - cdb[5]));
    uint32_t      buffer_len     = transferLength * blockSize;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int PlextorReadCdDa(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t blockSize,
                    uint32_t transferLength, uint8_t subchannel)
{
    unsigned char cmd_len    = 12;
    uint32_t      buffer_len = transferLength * blockSize;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    char cdb[] = {PIONEER_READ_CDDA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[2]  = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[3]  = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4]  = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5]  = (uint8_t)(lba & 0xFF);
    cdb[6]  = (uint8_t)((transferLength & 0xFF000000) >> 24);
    cdb[7]  = (uint8_t)((transferLength & 0xFF0000) >> 16);
    cdb[8]  = (uint8_t)((transferLength & 0xFF00) >> 8);
    cdb[9]  = (uint8_t)(transferLength & 0xFF);
    cdb[10] = (uint8_t)subchannel;

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int
PlextorReadRawDvd(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t transferLength)
{
    unsigned char cmd_len    = 10;
    uint32_t      buffer_len = transferLength * 2064;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    char cdb[] = {SCSI_READ_BUFFER, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[1] = 0x02;
    cdb[3] = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4] = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5] = (uint8_t)(lba & 0xFF);
    cdb[3] = (uint8_t)((buffer_len & 0xFF0000) >> 16);
    cdb[4] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[5] = (uint8_t)(buffer_len & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int PioneerReadCdDa(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t blockSize,
                    uint32_t transferLength, uint8_t subchannel)
{
    unsigned char cmd_len    = 12;
    uint32_t      buffer_len = transferLength * blockSize;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    char cdb[] = {PIONEER_READ_CDDA, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[2]  = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[3]  = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4]  = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5]  = (uint8_t)(lba & 0xFF);
    cdb[7]  = (uint8_t)((transferLength & 0xFF0000) >> 16);
    cdb[8]  = (uint8_t)((transferLength & 0xFF00) >> 8);
    cdb[9]  = (uint8_t)(transferLength & 0xFF);
    cdb[10] = (uint8_t)subchannel;

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int PioneerReadCdDaMsf(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t startMsf, uint32_t endMsf,
                       uint32_t blockSize, uint8_t subchannel)
{
    unsigned char cmd_len = 12;
    char          cdb[]   = {PIONEER_READ_CDDA_MSF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[3]  = (uint8_t)((startMsf & 0xFF0000) >> 16);
    cdb[4]  = (uint8_t)((startMsf & 0xFF00) >> 8);
    cdb[5]  = (uint8_t)(startMsf & 0xFF);
    cdb[7]  = (uint8_t)((endMsf & 0xFF0000) >> 16);
    cdb[8]  = (uint8_t)((endMsf & 0xFF00) >> 8);
    cdb[9]  = (uint8_t)(endMsf & 0xFF);
    cdb[10] = (uint8_t)subchannel;

    uint32_t      transferLength = (uint)((cdb[7] - cdb[3]) * 60 * 75 + (cdb[8] - cdb[4]) * 75 + (cdb[9] - cdb[5]));
    uint32_t      buffer_len     = transferLength * blockSize;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int NecReadCdDa(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t transferLength)
{
    unsigned char cmd_len    = 12;
    uint32_t      buffer_len = transferLength * 2352;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    char cdb[] = {NEC_READ_CDDA, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[2] = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[3] = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4] = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5] = (uint8_t)(lba & 0xFF);
    cdb[7] = (uint8_t)((transferLength & 0xFF00) >> 8);
    cdb[8] = (uint8_t)(transferLength & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int HlDtStReadRawDvd(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t transferLength)
{
    unsigned char cmd_len    = 12;
    uint32_t      buffer_len = transferLength * 2064;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    char cdb[] = {HLDTST_VENDOR, 0x48, 0x49, 0x54, 0x01, 0, 0, 0, 0, 0, 0, 0};

    cdb[6]  = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[7]  = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[8]  = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[9]  = (uint8_t)(lba & 0xFF);
    cdb[10] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[11] = (uint8_t)(buffer_len & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReadBlockLimits(int fd, unsigned char **buffer, unsigned char **senseBuffer)
{
    unsigned char cmd_len    = 6;
    unsigned int  buffer_len = 6;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_READ_BLOCK_LIMITS, 0, 0, 0, 0, 0};

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReportDensitySupport(int fd, unsigned char **buffer, unsigned char **senseBuffer, int mediumType, int currentMedia)
{
    unsigned char cmd_len    = 10;
    unsigned int  buffer_len = 256;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_REPORT_DENSITY_SUPPORT, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    if(currentMedia)
        cdb[1] |= 0x01;
    if(mediumType)
        cdb[1] |= 0x02;
    cdb[7]              = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[8]              = (uint8_t)(buffer_len & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    if(error)
        return error;

    buffer_len = (unsigned int)(*(*buffer + 0) << 8) + *(*buffer + 1) + 2;

    free(*buffer);
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    cdb[7] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[8] = (uint8_t)(buffer_len & 0xFF);

    error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}


int ReadMediaSerialNumber(int fd, unsigned char **buffer, unsigned char **senseBuffer)
{
    unsigned char cmd_len    = 12;
    unsigned int  buffer_len = 256;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_READ_MEDIA_SERIAL, 0x01, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    cdb[6] = (uint8_t)((buffer_len & 0xFF000000) >> 24);
    cdb[7] = (uint8_t)((buffer_len & 0xFF0000) >> 16);
    cdb[8] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[9] = (uint8_t)(buffer_len & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    if(error)
        return error;

    buffer_len =
            (unsigned int)((*(*buffer + 0) << 24) + (*(*buffer + 1) << 16)) + (*(*buffer + 2) << 8) + *(*buffer + 3) +
            4;

    free(*buffer);
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);
    cdb[6] = (uint8_t)((buffer_len & 0xFF000000) >> 24);
    cdb[7] = (uint8_t)((buffer_len & 0xFF0000) >> 16);
    cdb[8] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[9] = (uint8_t)(buffer_len & 0xFF);

    error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}
