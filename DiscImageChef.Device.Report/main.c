/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : main.c
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

For Linux, opens a device and generates a report, without needing a .NET
environment, for systems that don't have it.

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
Copyright © 2011-2018 Natalia Portillo
****************************************************************************/

#include <stdio.h>
#include <fcntl.h>
#include <errno.h>
#include <string.h>
#include <unistd.h>
#include <malloc.h>
#include <libxml/xmlwriter.h>
#include "main.h"
#include "scsi.h"
#include "ata.h"
#include "atapi.h"
#include "atapi_report.h"
#include "scsi_report.h"
#include "ata_report.h"

int main(int argc, void *argv[])
{
    int                  fd, rc;
    unsigned char        *scsi_sense    = NULL;
    unsigned char        *scsi_inq_data = NULL;
    unsigned char        *ata_ident     = NULL;
    unsigned char        *atapi_ident   = NULL;
    AtaErrorRegistersCHS *ata_error_chs;
    int                  scsi_error, ata_error;
    unsigned char        *manufacturer;
    unsigned char        *product;
    unsigned char        *revision;
    int                  deviceType     = DEVICE_TYPE_UNKNOWN;
    char                 *xmlFilename   = malloc(NAME_MAX + 1);
    xmlTextWriterPtr     xmlWriter;
    const char           *ataName       = "ATA";

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

    if(scsi_inq_data == NULL || !strncmp((const char *)manufacturer, ataName, 3))
    {
        ata_error = Identify(fd, &ata_ident, &ata_error_chs);

        if(!ata_error)
        {
            deviceType = DEVICE_TYPE_ATA;
            revision   = AtaToCString(ata_ident + (23 * 2), 8);
            product    = AtaToCString(ata_ident + (27 * 2), 40);
        }
    }

    printf("Device type: %s\n", DeviceType[deviceType]);
    printf("Manufacturer: %s\n", manufacturer);
    printf("Product: %s\n", product);
    printf("Revision: %s\n", revision);

    if(deviceType != DEVICE_TYPE_ATA && deviceType != DEVICE_TYPE_ATAPI && deviceType != DEVICE_TYPE_SCSI)
    {
        printf("Unsupported device type %s.", DeviceType[deviceType]);
        return 3;
    }

    sprintf(xmlFilename, "%s_%s_%s.xml", manufacturer, product, revision);

    xmlWriter = xmlNewTextWriterFilename(xmlFilename, 0);
    if(xmlWriter == NULL)
    {
        printf("Could not create XML report file.\n");
        return 4;
    }

    rc = xmlTextWriterStartDocument(xmlWriter, NULL, XML_ENCODING, NULL);
    if(rc < 0)
    {
        printf("Could not create XML report file.\n");
        return 4;
    }

    rc = xmlTextWriterStartElement(xmlWriter, BAD_CAST DIC_REPORT_ROOT);
    if(rc < 0)
    {
        printf("Could not create XML report file.\n");
        return 4;
    }

    char *xmlComment = malloc(255);
    sprintf(xmlComment, "Report created with DiscImageChef.Device.Report v%s", DIC_VERSION);
    rc = xmlTextWriterWriteComment(xmlWriter, xmlComment);
    if(rc < 0)
    {
        printf("Could not create XML report file.\n");
        return 4;
    }

    if(deviceType == DEVICE_TYPE_ATAPI)
        AtapiReport(fd, xmlWriter);

    if(deviceType == DEVICE_TYPE_ATAPI || deviceType == DEVICE_TYPE_SCSI)
        ScsiReport(fd, xmlWriter);

    if(deviceType == DEVICE_TYPE_ATA)
        AtaReport(fd, xmlWriter);

    rc = xmlTextWriterEndDocument(xmlWriter);
    if(rc < 0)
    {
        printf("Could not close XML report file.\n");
        return 4;
    }

    close(fd);

    return 0;
}