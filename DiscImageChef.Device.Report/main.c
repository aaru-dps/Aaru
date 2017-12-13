#include <stdio.h>
#include <fcntl.h>
#include <errno.h>
#include <string.h>
#include <unistd.h>
#include <malloc.h>
#include "scsi.h"
#include "ata.h"
#include "main.h"
#include "atapi.h"

#define DIC_VERSION "3.99.6.0"
#define DIC_COPYRIGHT "Copyright © 2011-2017 Natalia Portillo"

int main(int argc, void *argv[])
{
    int fd;
    unsigned char *scsi_sense = NULL;
    unsigned char *scsi_inq_data = NULL;
    unsigned char *ata_ident = NULL;
    unsigned char *atapi_ident = NULL;
    AtaErrorRegistersCHS *ata_error_chs;
    int scsi_error, ata_error;
    unsigned char* manufacturer;
    unsigned char* product;
    unsigned char* revision;
    int deviceType = DEVICE_TYPE_UNKNOWN;

    printf("The Disc Image Chef Device Reporter for Linux %s\n", DIC_VERSION);
    printf("%s\n", DIC_COPYRIGHT);

    if(argc != 2)
    {
        printf("Usage:\n");
        printf("%s <device_path>\n", argv[0]);
        return 1;
    }

    fd = open(argv[1], O_RDONLY | O_NONBLOCK);

    if(fd < 0)
    {
        printf("Error opening device: %s\n", strerror(errno));
        return 2;
    }

    // TODO: Support MMC, USB, FireWire, PCMCIA

    scsi_error = Inquiry(fd, &scsi_inq_data, &scsi_sense);

    if(scsi_error)
        scsi_inq_data = NULL;

    if(scsi_inq_data != NULL)
    {
        manufacturer = malloc(9);
        manufacturer[8] = 0;
        product = malloc(17);
        product[16] = 0;
        revision = malloc(5);
        revision[4] = 0;

        strncpy(manufacturer, scsi_inq_data + 8, 8);
        strncpy(product, scsi_inq_data + 16, 16);
        strncpy(revision, scsi_inq_data + 32, 4);

        deviceType = DEVICE_TYPE_SCSI;

        ata_error = IdentifyPacket(fd, &atapi_ident, &ata_error_chs);

        if(!ata_error)
            deviceType = DEVICE_TYPE_ATAPI;
    }

    if(scsi_inq_data == NULL || strcmp(manufacturer,"ATA"))
    {
        ata_error = Identify(fd, &ata_ident, &ata_error_chs);

        if(!ata_error)
        {
            deviceType = DEVICE_TYPE_ATA;
            revision = AtaToCString(ata_ident + (23*2), 8);
            product = AtaToCString(ata_ident + (27*2), 40);
        }
    }

    printf("Device type: %s\n", DeviceType[deviceType]);
    printf("Manufacturer: %s\n", manufacturer);
    printf("Product: %s\n", product);
    printf("Revision: %s\n", revision);

    close(fd);

    return 0;
}