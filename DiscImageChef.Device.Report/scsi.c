//
// Created by claunia on 11/12/17.
//

#include <unitypes.h>
#include <malloc.h>
#include <scsi/sg.h>
#include <sys/ioctl.h>
#include <errno.h>
#include <string.h>
#include <stdint.h>
#include "scsi.h"

#define FALSE 0
#define TRUE 1

int SendScsiCommand(int fd, void *cdb, unsigned char cdb_len, unsigned char *buffer, unsigned int buffer_len, unsigned char **senseBuffer, int direction)
{
    if(buffer == NULL || cdb == NULL)
        return -1;

    *senseBuffer = malloc(32);
    memset(*senseBuffer, 0, 32);

    sg_io_hdr_t io_hdr;
    memset(&io_hdr, 0, sizeof(sg_io_hdr_t));

    io_hdr.interface_id = 'S';
    io_hdr.cmd_len = cdb_len;
    io_hdr.mx_sb_len = 32;
    io_hdr.dxfer_direction = direction;
    io_hdr.dxfer_len = buffer_len;
    io_hdr.dxferp = buffer;
    io_hdr.cmdp = cdb;
    io_hdr.sbp = *senseBuffer;
    io_hdr.timeout = 10000;

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
    char cdb[] = {SCSI_PREVENT_ALLOW_MEDIUM_REMOVAL, 0, 0, 0, 0, 0};
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

int StartStopUnit(int fd, unsigned char **senseBuffer, int immediate, uint8_t formatLayer, uint8_t powerConditions, int changeFormatLayer, int loadEject, int start)
{
    unsigned char cmd_len = 6;
    char cdb[] = {SCSI_START_STOP_UNIT, 0, 0, 0, 0, 0};
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
    char cdb[] = {SCSI_PREVENT_ALLOW_MEDIUM_REMOVAL, 0, 0, 0, 0, 0};
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
    char cdb[] = {SCSI_LOAD_UNLOAD, 0, 0, 0, 0, 0};
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

int ModeSense6(int fd, unsigned char **buffer, unsigned char **senseBuffer, int DBD, uint8_t pageControl, uint8_t pageCode, uint8_t subPageCode)
{
    unsigned char cmd_len = 6;
    unsigned int buffer_len = 255;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_MODE_SENSE, 0, 0, 0, 0, 0};
    if(DBD)
        cdb[1] |= 0x08;
    cdb[2] |= pageControl;
    cdb[2] |= (pageCode & 0x3F);
    cdb[3] = subPageCode;
    cdb[4] = (uint8_t)(buffer_len & 0xFF);

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

int ModeSense10(int fd, unsigned char **buffer, unsigned char **senseBuffer, int LLBAA, int DBD, uint8_t pageControl, uint8_t pageCode, uint8_t subPageCode)
{
    unsigned char cmd_len = 10;
    unsigned int buffer_len = 4096;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_MODE_SENSE_10, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    if(LLBAA)
        cdb[1] |= 0x10;
    if(DBD)
        cdb[1] |= 0x08;
    cdb[2] |= pageControl;
    cdb[2] |= (pageCode & 0x3F);
    cdb[3] = subPageCode;
    cdb[7] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[8] = (uint8_t)(buffer_len & 0xFF);
    cdb[9] = 0;

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
    unsigned char cmd_len = 10;
    unsigned int buffer_len = 8;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_READ_CAPACITY, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    if(PMI)
    {
        cdb[8] = 0x01;
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
    unsigned char cmd_len = 16;
    unsigned int buffer_len = 32;
    *buffer = malloc(buffer_len);
    memset(*buffer, 0, buffer_len);

    unsigned char cdb[] = {SCSI_SERVICE_ACTION_IN, SCSI_READ_CAPACITY_16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    if(PMI)
    {
        cdb[14] = 0x01;
        cdb[2] = (uint8_t)((address & 0xFF00000000000000ULL) >> 56);
        cdb[3] = (uint8_t)((address & 0xFF000000000000ULL) >> 48);
        cdb[4] = (uint8_t)((address & 0xFF0000000000ULL) >> 40);
        cdb[5] = (uint8_t)((address & 0xFF00000000ULL) >> 32);
        cdb[6] = (uint8_t)((address & 0xFF000000ULL) >> 24);
        cdb[7] = (uint8_t)((address & 0xFF0000ULL) >> 16);
        cdb[8] = (uint8_t)((address & 0xFF00ULL) >> 8);
        cdb[9] = (uint8_t)(address & 0xFFULL);
    }

    cdb[10] = (uint8_t)((buffer_len & 0xFF000000) >> 24);
    cdb[11] = (uint8_t)((buffer_len & 0xFF0000) >> 16);
    cdb[12] = (uint8_t)((buffer_len & 0xFF00) >> 8);
    cdb[13] = (uint8_t)(buffer_len & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buffer_len, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int Read6(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint32_t lba, uint32_t blockSize, uint8_t transferLength)
{
    unsigned char cmd_len = 6;
    unsigned int buflen = transferLength == 0 ? 256 * blockSize : transferLength * blockSize;
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

int Read10(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t rdprotect, int dpo, int fua, int fuaNv, int relAddr, uint32_t lba, uint32_t blockSize, uint8_t groupNumber, uint16_t transferLength)
{
    unsigned char cmd_len = 10;
    unsigned int buflen = transferLength * blockSize;
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

int Read12(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t rdprotect, int dpo, int fua, int fuaNv, int relAddr, uint32_t lba, uint32_t blockSize, uint8_t groupNumber, uint32_t transferLength, int streaming)
{
    unsigned char cmd_len = 12;
    unsigned int buflen = transferLength * blockSize;
    *buffer = malloc(buflen);
    memset(*buffer, 0, buflen);

    unsigned char cdb[] = {SCSI_READ_12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

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
    cdb[6] = (uint8_t)((transferLength & 0xFF000000) >> 24);
    cdb[7] = (uint8_t)((transferLength & 0xFF0000) >> 16);
    cdb[8] = (uint8_t)((transferLength & 0xFF00) >> 8);
    cdb[9] = (uint8_t)(transferLength & 0xFF);
    cdb[10] = (uint8_t)(groupNumber & 0x1F);
    if(streaming)
        cdb[10] += 0x80;

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, buflen, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int Read16(int fd, unsigned char **buffer, unsigned char **senseBuffer, uint8_t rdprotect, int dpo, int fua, int fuaNv, uint64_t lba, uint32_t blockSize, uint8_t groupNumber, uint32_t transferLength, int streaming)
{
    unsigned char cmd_len = 16;
    unsigned int buflen = transferLength * blockSize;
    *buffer = malloc(buflen);
    memset(*buffer, 0, buflen);

    unsigned char cdb[] = {SCSI_READ_16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[1] = (uint8_t)((rdprotect & 0x07) << 5);
    if(dpo)
        cdb[1] += 0x10;
    if(fua)
        cdb[1] += 0x08;
    if(fuaNv)
        cdb[1] += 0x02;
    cdb[2] = (uint8_t)((lba & 0xFF00000000000000ULL) >> 56);
    cdb[3] = (uint8_t)((lba & 0xFF000000000000ULL) >> 48);
    cdb[4] = (uint8_t)((lba & 0xFF0000000000ULL) >> 40);
    cdb[5] = (uint8_t)((lba & 0xFF00000000ULL) >> 32);
    cdb[6] = (uint8_t)((lba & 0xFF000000ULL) >> 24);
    cdb[7] = (uint8_t)((lba & 0xFF0000ULL) >> 16);
    cdb[8] = (uint8_t)((lba & 0xFF00ULL) >> 8);
    cdb[9] = (uint8_t)(lba & 0xFFULL);
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

int ReadLong10(int fd, unsigned char **buffer, unsigned char **senseBuffer, int correct, int relAddr, uint32_t lba, uint16_t transferBytes)
{
    unsigned char cmd_len = 10;
    *buffer = malloc(transferBytes);
    memset(*buffer, 0, transferBytes);

    unsigned char cdb[] = {SCSI_READ_LONG, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    if(correct)
        cdb[1] += 0x02;
    if(relAddr)
        cdb[1] += 0x01;
    cdb[2] = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[3] = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4] = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5] = (uint8_t)(lba & 0xFF);
    cdb[7] = (uint8_t)((transferBytes & 0xFF00) >> 8);
    cdb[8] = (uint8_t)(transferBytes & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, *buffer, transferBytes, senseBuffer, SG_DXFER_FROM_DEV);

    return error;
}

int ReadLong16(int fd, unsigned char **buffer, unsigned char **senseBuffer, int correct, uint64_t lba, uint32_t transferBytes)
{
    unsigned char cmd_len = 16;
    *buffer = malloc(transferBytes);
    memset(*buffer, 0, transferBytes);

    unsigned char cdb[] = {SCSI_SERVICE_ACTION_IN, SCSI_READ_LONG_16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    cdb[2] = (uint8_t)((lba & 0xFF00000000000000ULL) >> 56);
    cdb[3] = (uint8_t)((lba & 0xFF000000000000ULL) >> 48);
    cdb[4] = (uint8_t)((lba & 0xFF0000000000ULL) >> 40);
    cdb[5] = (uint8_t)((lba & 0xFF00000000ULL) >> 32);
    cdb[6] = (uint8_t)((lba & 0xFF000000ULL) >> 24);
    cdb[7] = (uint8_t)((lba & 0xFF0000ULL) >> 16);
    cdb[8] = (uint8_t)((lba & 0xFF00ULL) >> 8);
    cdb[9] = (uint8_t)(lba & 0xFFULL);
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
    char cdb[] = {SCSI_SEEK, 0, 0, 0, 0, 0};
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
    char cdb[] = {SCSI_SEEK_10, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    unsigned char *buffer = malloc(0);

    cdb[2] = (uint8_t)((lba & 0xFF000000) >> 24);
    cdb[3] = (uint8_t)((lba & 0xFF0000) >> 16);
    cdb[4] = (uint8_t)((lba & 0xFF00) >> 8);
    cdb[5] = (uint8_t)(lba & 0xFF);

    int error = SendScsiCommand(fd, &cdb, cmd_len, buffer, 0, senseBuffer, SG_DXFER_NONE);

    return error;
}
