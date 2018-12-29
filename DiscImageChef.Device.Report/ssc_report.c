/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : ssc_report.c
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Creates report for SCSI Streaming devices.

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

#include <stdint.h>
#include <string.h>
#include <unistd.h>
#include <libxml/xmlwriter.h>
#include "ssc_report.h"
#include "scsi.h"
#include "scsi_mode.h"

DensitySupport *DecodeDensity(unsigned char *response);

MediaTypeSupport *DecodeMediumTypes(unsigned char *response);

void SscReport(int fd, xmlTextWriterPtr xmlWriter)
{
    unsigned char *sense        = NULL;
    unsigned char *buffer       = NULL;
    int           i, error, len;
    char          user_response = ' ';

    xmlTextWriterStartElement(xmlWriter, BAD_CAST "SequentialDevice"); // <SequentialDevice>

    printf("Querying SCSI READ BLOCK LIMITS...\n");
    error = ReadBlockLimits(fd, &buffer, &sense);
    if(!error)
    {
        uint8_t  granularity = (uint8_t)(buffer[0] & 0x1F);
        uint32_t maxBlockLen = (uint32_t)((buffer[1] << 16) + (buffer[2] << 8) + buffer[3]);
        uint16_t minBlockLen = (uint16_t)((buffer[4] << 8) + buffer[5]);

        if(granularity > 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BlockSizeGranularity", "%d", granularity);
        if(maxBlockLen > 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MaxBlockLength", "%d", maxBlockLen);
        if(minBlockLen > 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinBlockLength", "%d", minBlockLen);
    }

    printf("Querying SCSI REPORT DENSITY SUPPORT...\n");
    error = ReportDensitySupport(fd, &buffer, &sense, FALSE, FALSE);
    if(!error)
    {
        DensitySupport *dsh = DecodeDensity(buffer);

        xmlTextWriterStartElement(xmlWriter, BAD_CAST "SupportedDensities"); // <SupportedDensities>

        for(i = 0; i < dsh->count; i++)
        {
            xmlTextWriterStartElement(xmlWriter, BAD_CAST "SupportedDensity"); // <SupportedDensity>
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BitsPerMm", "%d",
                                            (dsh->descriptors[i]->bitsPerMm[0] << 16) +
                                            (dsh->descriptors[i]->bitsPerMm[1] << 8) +
                                            dsh->descriptors[i]->bitsPerMm[2]);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Capacity", "%d",
                                            be32toh(dsh->descriptors[i]->capacity));
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DefaultDensity", "%s",
                                            dsh->descriptors[i]->deflt ? "true" : "false");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Description", "%20s",
                                            dsh->descriptors[i]->description);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Duplicate", "%s",
                                            dsh->descriptors[i]->dup ? "true" : "false");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Name", "%8s", dsh->descriptors[i]->densityName);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Organization", "%8s",
                                            dsh->descriptors[i]->organization);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PrimaryCode", "%d", dsh->descriptors[i]->primaryCode);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SecondaryCode", "%d",
                                            dsh->descriptors[i]->secondaryCode);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Tracks", "%d", be16toh(dsh->descriptors[i]->tracks));
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Width", "%d",
                                            be16toh(dsh->descriptors[i]->mediaWidth));
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Writable", "%s",
                                            dsh->descriptors[i]->wrtok ? "true" : "false");
            xmlTextWriterEndElement(xmlWriter); // </SupportedDensity>
        }

        xmlTextWriterEndElement(xmlWriter); // </SupportedDensities>
    }

    printf("Querying SCSI REPORT DENSITY SUPPORT for medium types...\n");
    error = ReportDensitySupport(fd, &buffer, &sense, TRUE, FALSE);
    if(!error)
    {
        MediaTypeSupport *mtsh = DecodeMediumTypes(buffer);

        xmlTextWriterStartElement(xmlWriter, BAD_CAST "SupportedMediaTypes"); // <SupportedMediaTypes>

        for(i = 0; i < mtsh->count; i++)
        {
            xmlTextWriterStartElement(xmlWriter, BAD_CAST "SupportedMedia"); // <SupportedMedia>
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Description", "%20s",
                                            mtsh->descriptors[i]->description);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Length", "%d", be16toh(mtsh->descriptors[i]->length));
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediumType", "%d", mtsh->descriptors[i]->mediumType);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Name", "%8s", mtsh->descriptors[i]->densityName);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Organization", "%8s",
                                            mtsh->descriptors[i]->organization);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Width", "%d",
                                            be16toh(mtsh->descriptors[i]->mediaWidth));
            xmlTextWriterStartElement(xmlWriter, BAD_CAST "SupportedDensity"); // <SupportedDensity>
            // TODO: Density codes
            xmlTextWriterEndElement(xmlWriter); // </SupportedMedia>
        }

        xmlTextWriterEndElement(xmlWriter); // </SupportedMediaTypes>
    }

    user_response = ' ';
    int         anyMedia = FALSE;
    DecodedMode *decMode;

    while(user_response != 'N' && user_response != 'n')
    {
        do
        {
            printf("Do you have media that you can insert in the drive? (Y/N): ");
            scanf("%c", &user_response);
            printf("\n");
        }while(user_response != 'Y' && user_response != 'y' && user_response != 'N' && user_response != 'n');

        if(user_response == 'Y' || user_response == 'y')
        {
            printf("Please insert it in the drive and press any key when it is ready.\n");
            scanf("%c");

            char mediaManufacturer[256], mediaName[256], mediaModel[256];
            printf("Please write a description of the media type and press enter: ");
            gets(mediaName);
            printf("Please write the media manufacturer and press enter: ");
            gets(mediaManufacturer);
            printf("Please write the media model and press enter: ");
            gets(mediaModel);

            error = TestUnitReady(fd, &sense);
            int mediaRecognized = TRUE;
            int leftRetries     = 20;

            if(error)
            {
                if((sense[0] == 0x70 || sense[0] == 0x71) && (sense[2] & 0x0F) != 0x00)
                {
                    if(sense[12] == 0x3A || (sense[12] == 0x04 && sense[13] == 0x01))
                    {
                        while(leftRetries > 0)
                        {
                            printf("\rWating for drive to become ready");
                            sleep(2);
                            error = TestUnitReady(fd, &sense);
                            if(!error)
                                break;

                            leftRetries--;
                        }

                        printf("\n");
                        mediaRecognized = !error;
                    }
                    else
                        mediaRecognized = FALSE;
                }
                else
                    mediaRecognized = FALSE;
            }

            if(!anyMedia)
                xmlTextWriterStartElement(xmlWriter, BAD_CAST "TestedMedia"); // <TestedMedia>

            xmlTextWriterStartElement(xmlWriter, BAD_CAST "SequentialMedia"); // <SequentialMedia>

            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediaIsRecognized", "%s",
                                            mediaRecognized ? "true" : "false");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Manufacturer", "%s", mediaManufacturer);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediumTypeName", "%s", mediaName);
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Model", "%s", mediaModel);

            if(mediaRecognized)
            {
                printf("Querying SCSI MODE SENSE (10)...\n");
                error = ModeSense10(fd, &buffer, &sense, FALSE, TRUE, MODE_PAGE_DEFAULT, 0x3F, 0x00);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsModeSense10", "%s",
                                                !error ? "true" : "false");
                if(!error)
                {
                    xmlTextWriterStartElement(xmlWriter, BAD_CAST "ModeSense10Data");
                    xmlTextWriterWriteBase64(xmlWriter, buffer, 0, (*(buffer + 0) << 8) + *(buffer + 1) + 2);
                    xmlTextWriterEndElement(xmlWriter);
                    decMode = DecodeMode10(buffer, 0x01);
                }

                printf("Querying SCSI MODE SENSE (6)...\n");
                error = ModeSense6(fd, &buffer, &sense, FALSE, MODE_PAGE_DEFAULT, 0x00, 0x00);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsModeSense6", "%s",
                                                !error ? "true" : "false");
                if(!error)
                {
                    xmlTextWriterStartElement(xmlWriter, BAD_CAST "ModeSense6Data");
                    xmlTextWriterWriteBase64(xmlWriter, buffer, 0, *(buffer + 0) + 1);
                    xmlTextWriterEndElement(xmlWriter);
                    if(decMode == NULL || !decMode->decoded)
                        decMode = DecodeMode6(buffer, 0x01);
                }

                if(decMode != NULL && decMode->decoded)
                {
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediumType", "%d", decMode->Header.MediumType);
                    if(decMode->Header.descriptorsLength > 0)
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Density", "%d",
                                                        decMode->Header.BlockDescriptors[0].Density);
                }

                printf("Querying SCSI REPORT DENSITY SUPPORT for current media...\n");
                error = ReportDensitySupport(fd, &buffer, &sense, FALSE, TRUE);
                if(!error)
                {
                    DensitySupport *dsh = DecodeDensity(buffer);

                    xmlTextWriterStartElement(xmlWriter, BAD_CAST "SupportedDensities"); // <SupportedDensities>

                    for(i = 0; i < dsh->count; i++)
                    {
                        xmlTextWriterStartElement(xmlWriter, BAD_CAST "SupportedDensity"); // <SupportedDensity>
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BitsPerMm", "%d",
                                                        (dsh->descriptors[i]->bitsPerMm[0] << 16) +
                                                        (dsh->descriptors[i]->bitsPerMm[1] << 8) +
                                                        dsh->descriptors[i]->bitsPerMm[2]);
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Capacity", "%d",
                                                        be32toh(dsh->descriptors[i]->capacity));
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DefaultDensity", "%s",
                                                        dsh->descriptors[i]->deflt ? "true" : "false");
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Description", "%20s",
                                                        dsh->descriptors[i]->description);
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Duplicate", "%s",
                                                        dsh->descriptors[i]->dup ? "true" : "false");
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Name", "%8s",
                                                        dsh->descriptors[i]->densityName);
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Organization", "%8s",
                                                        dsh->descriptors[i]->organization);
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PrimaryCode", "%d",
                                                        dsh->descriptors[i]->primaryCode);
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SecondaryCode", "%d",
                                                        dsh->descriptors[i]->secondaryCode);
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Tracks", "%d",
                                                        be16toh(dsh->descriptors[i]->tracks));
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Width", "%d",
                                                        be16toh(dsh->descriptors[i]->mediaWidth));
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Writable", "%s",
                                                        dsh->descriptors[i]->wrtok ? "true" : "false");
                        xmlTextWriterEndElement(xmlWriter); // </SupportedDensity>
                    }

                    xmlTextWriterEndElement(xmlWriter); // </SupportedDensities>
                }


                printf("Querying SCSI REPORT DENSITY SUPPORT for medium types for current media...\n");
                error = ReportDensitySupport(fd, &buffer, &sense, TRUE, TRUE);
                if(!error)
                {
                    MediaTypeSupport *mtsh = DecodeMediumTypes(buffer);

                    xmlTextWriterStartElement(xmlWriter, BAD_CAST "SupportedMediaTypes"); // <SupportedMediaTypes>

                    for(i = 0; i < mtsh->count; i++)
                    {
                        xmlTextWriterStartElement(xmlWriter, BAD_CAST "SupportedMedia"); // <SupportedMedia>
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Description", "%20s",
                                                        mtsh->descriptors[i]->description);
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Length", "%d",
                                                        be16toh(mtsh->descriptors[i]->length));
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediumType", "%d",
                                                        mtsh->descriptors[i]->mediumType);
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Name", "%8s",
                                                        mtsh->descriptors[i]->densityName);
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Organization", "%8s",
                                                        mtsh->descriptors[i]->organization);
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Width", "%d",
                                                        be16toh(mtsh->descriptors[i]->mediaWidth));
                        xmlTextWriterStartElement(xmlWriter, BAD_CAST "SupportedDensity"); // <SupportedDensity>
                        // TODO: Density codes
                        xmlTextWriterEndElement(xmlWriter); // </SupportedMedia>
                    }

                    xmlTextWriterEndElement(xmlWriter); // </SupportedMediaTypes>
                }

                printf("Trying SCSI READ MEDIA SERIAL NUMBER...\n");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsRead", "%s",
                                                !ReadMediaSerialNumber(fd, &buffer, &sense) ? "true" : "false");
            }

            xmlTextWriterEndElement(xmlWriter); // </SequentialMedia>

            if(!anyMedia)
            {
                xmlTextWriterEndElement(xmlWriter); // </TestedMedia>
                anyMedia = TRUE;
            }
        }
    }

    xmlTextWriterEndElement(xmlWriter); // </SequentialDevice>
}

DensitySupport *DecodeDensity(unsigned char *response)
{
    DensitySupport *decoded = malloc(sizeof(DensitySupport));
    memset(decoded, 0, sizeof(DensitySupport));
    uint16_t responseLen = (uint16_t)((response[0] << 8) + response[1] + 2);
    int      offset      = 4;

    while(offset + 3 < responseLen)
    {
        int      lenValid = response[offset + 2] & 0x20;
        uint16_t descLen  = (uint16_t)((response[offset + 3] << 8) + response[offset + 4] + 5);

        decoded->descriptors[decoded->count] = malloc(sizeof(DensityDescriptor));
        memset(decoded->descriptors[decoded->count], 0, sizeof(DensityDescriptor));
        memcpy(decoded->descriptors[decoded->count], response + offset, sizeof(DensityDescriptor));

        if(lenValid)
            offset += descLen;
        else
            offset += 52;

        decoded->count++;
    }
}

MediaTypeSupport *DecodeMediumTypes(unsigned char *response)
{
    MediaTypeSupport *decoded = malloc(sizeof(MediaTypeSupport));
    memset(decoded, 0, sizeof(MediaTypeSupport));
    uint16_t responseLen = (uint16_t)((response[0] << 8) + response[1] + 2);
    int      offset      = 4;

    while(offset + 3 < responseLen)
    {
        decoded->descriptors[decoded->count] = malloc(sizeof(MediumDescriptor));
        memset(decoded->descriptors[decoded->count], 0, sizeof(MediumDescriptor));
        memcpy(decoded->descriptors[decoded->count], response + offset, sizeof(MediumDescriptor));

        offset += 56;
        decoded->count++;
    }
}