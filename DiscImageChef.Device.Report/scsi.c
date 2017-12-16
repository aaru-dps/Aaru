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
    else
        free(*senseBuffer);

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