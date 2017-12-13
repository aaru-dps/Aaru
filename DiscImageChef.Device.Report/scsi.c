//
// Created by claunia on 11/12/17.
//

#include <unitypes.h>
#include <malloc.h>
#include <scsi/sg.h>
#include <sys/ioctl.h>
#include <errno.h>
#include <string.h>
#include "scsi.h"

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