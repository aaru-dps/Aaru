//
// Created by claunia on 11/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_SCSI_H
#define DISCIMAGECHEF_DEVICE_REPORT_SCSI_H

int SendScsiCommand(int fd, void *cdb, unsigned char cdb_len, unsigned char *buffer, unsigned int buffer_len, unsigned char **senseBuffer, int direction);
int Inquiry(int fd, unsigned char **buffer, unsigned char **senseBuffer);

typedef enum
{
    SCSI_INQUIRY = 0x12,
    SCSI_ATA_PASSTHROUGH_16 = 0x85
} ScsiCommands;

#endif //DISCIMAGECHEF_DEVICE_REPORT_SCSI_H
