/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------

Filename       : ata_report.c
Author(s)      : Natalia Portillo

Component      : DiscImageChef.Device.Report

--[ Description ] ----------------------------------------------------------

Creates report for ATA devices.

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

#include <string.h>
#include <libxml/xmlwriter.h>
#include "ata_report.h"
#include "ata.h"
#include "identify_decode.h"

void AtaReport(int fd, xmlTextWriterPtr xmlWriter)
{
    unsigned char          *ata_ident    = NULL;
    unsigned char          *buffer       = NULL;
    AtaErrorRegistersCHS   *ata_error_chs;
    AtaErrorRegistersLBA28 *ata_error_lba;
    AtaErrorRegistersLBA48 *ata_error_lba48;
    int                    error;
    int                    removable     = FALSE;
    char                   user_response = ' ';

    printf("Querying ATA IDENTIFY...\n");
    error = Identify(fd, &ata_ident, &ata_error_chs);

    if(error)
    {
        fprintf(stderr, "Error {0} requesting IDENTIFY DEVICE", error);
        return;
    }

    IdentifyDevice *identify = malloc(512);
    memcpy(identify, ata_ident, 512);

    xmlTextWriterStartElement(xmlWriter, BAD_CAST DIC_ATA_REPORT_ELEMENT);

    if(le16toh(identify->GeneralConfiguration) == 0x848A)
    {
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CompactFlash", "%s", "TRUE");
        removable = FALSE;
    }
    else if(identify->GeneralConfiguration & 0x0080)
    {
        do
        {
            printf("Is the media removable from the reading/writing elements (flash memories ARE NOT removable)? (Y/N): ");
            scanf("%c", &user_response);
            printf("\n");
        }while(user_response != 'Y' && user_response != 'y' && user_response != 'N' && user_response != 'n');

        removable = (user_response == 'Y' || user_response == 'y');
    }

    if(removable)
    {
        printf("Please remove any media from the device and press any key when it is out.\n");
        scanf("%c");
        printf("Querying ATA IDENTIFY...\n");
        error = Identify(fd, &ata_ident, &ata_error_chs);
        free(identify);
        identify = malloc(512);
        memcpy(identify, ata_ident, 512);
    }

    if((uint64_t)*identify->AdditionalPID != 0 && (uint64_t)*identify->AdditionalPID != 0x2020202020202020)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "AdditionalPid", AtaToCString(identify->AdditionalPID, 8));
    if(identify->APIOSupported)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "APIOSupported",
                                  DecodeTransferMode(le16toh(identify->APIOSupported)));
    if(identify->BufferType)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BufferType", "%u", le16toh(identify->BufferType));
    if(identify->BufferSize)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BufferSize", "%u", le16toh(identify->BufferSize));
    if(identify->Capabilities)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "Capabilities",
                                  DecodeCapabilities(le16toh(identify->Capabilities)));
    if(identify->Capabilities2)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "Capabilities2",
                                  DecodeCapabilities2(le16toh(identify->Capabilities2)));
    if(identify->Capabilities3)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "Capabilities3", DecodeCapabilities3(identify->Capabilities3));
    if(identify->CFAPowerMode)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CFAPowerMode", "%u", le16toh(identify->CFAPowerMode));
    if(identify->CommandSet)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "CommandSet", DecodeCommandSet(le16toh(identify->CommandSet)));
    if(identify->CommandSet2)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "CommandSet2", DecodeCommandSet2(le16toh(identify->CommandSet2)));
    if(identify->CommandSet3)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "CommandSet3", DecodeCommandSet3(le16toh(identify->CommandSet3)));
    if(identify->CommandSet4)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "CommandSet4", DecodeCommandSet4(le16toh(identify->CommandSet4)));
    if(identify->CommandSet5)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "CommandSet5", DecodeCommandSet5(le16toh(identify->CommandSet5)));
    if(identify->CurrentAAM)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CurrentAAM", "%u", identify->CurrentAAM);
    if(identify->CurrentAPM)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CurrentAPM", "%u", le16toh(identify->CurrentAPM));
    if(identify->DataSetMgmt)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "DataSetMgmt", DecodeDataSetMgmt(le16toh(identify->DataSetMgmt)));
    if(identify->DataSetMgmtSize)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DataSetMgmtSize", "%u",
                                        le16toh(identify->DataSetMgmtSize));
    if(identify->DeviceFormFactor)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "DeviceFormFactor",
                                  DecodeDeviceFormFactor(le16toh(identify->DeviceFormFactor)));
    if(identify->DMAActive)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "DMAActive", DecodeTransferMode(le16toh(identify->DMAActive)));
    if(identify->DMASupported)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "DMASupported",
                                  DecodeTransferMode(le16toh(identify->DMASupported)));
    if(identify->DMATransferTimingMode)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DMATransferTimingMode", "%u",
                                        identify->DMATransferTimingMode);
    if(identify->EnhancedSecurityEraseTime)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "EnhancedSecurityEraseTime", "%u",
                                        le16toh(identify->EnhancedSecurityEraseTime));
    if(identify->EnabledCommandSet)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "EnabledCommandSet",
                                  DecodeCommandSet(le16toh(identify->EnabledCommandSet)));
    if(identify->EnabledCommandSet2)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "EnabledCommandSet2",
                                  DecodeCommandSet2(le16toh(identify->EnabledCommandSet2)));
    if(identify->EnabledCommandSet3)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "EnabledCommandSet3",
                                  DecodeCommandSet3(le16toh(identify->EnabledCommandSet3)));
    if(identify->EnabledCommandSet4)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "EnabledCommandSet4",
                                  DecodeCommandSet4(le16toh(identify->EnabledCommandSet4)));
    if(identify->EnabledSATAFeatures)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "EnabledSATAFeatures",
                                  DecodeSATAFeatures(le16toh(identify->EnabledSATAFeatures)));
    if(identify->ExtendedUserSectors)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ExtendedUserSectors", "%llu",
                                        le64toh(identify->ExtendedUserSectors));
    if(identify->FreeFallSensitivity)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "FreeFallSensitivity", "%u", identify->FreeFallSensitivity);
    xmlTextWriterWriteElement(xmlWriter, BAD_CAST "FirmwareRevision", AtaToCString(identify->FirmwareRevision, 8));
    if(identify->GeneralConfiguration)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "GeneralConfiguration",
                                  DecodeGeneralConfiguration(le16toh(identify->GeneralConfiguration)));
    if(identify->HardwareResetResult)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "HardwareResetResult", "%u",
                                        le16toh(identify->HardwareResetResult));
    if(identify->InterseekDelay)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "InterseekDelay", "%u", le16toh(identify->InterseekDelay));
    if(identify->MajorVersion)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "MajorVersion",
                                  DecodeMajorVersion(le16toh(identify->MajorVersion)));
    if(identify->MasterPasswordRevisionCode)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MasterPasswordRevisionCode", "%u",
                                        le16toh(identify->MasterPasswordRevisionCode));
    if(identify->MaxDownloadMicroMode3)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MaxDownloadMicroMode3", "%u",
                                        le16toh(identify->MaxDownloadMicroMode3));
    if(identify->MaxQueueDepth)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MaxQueueDepth", "%u", le16toh(identify->MaxQueueDepth));
    if(identify->MDMAActive)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "MDMAActive", DecodeTransferMode(le16toh(identify->MDMAActive)));
    if(identify->MDMASupported)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "MDMASupported",
                                  DecodeTransferMode(le16toh(identify->MDMASupported)));
    if(identify->MinDownloadMicroMode3)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinDownloadMicroMode3", "%u",
                                        le16toh(identify->MinDownloadMicroMode3));
    if(identify->MinMDMACycleTime)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinMDMACycleTime", "%u",
                                        le16toh(identify->MinMDMACycleTime));
    if(identify->MinorVersion)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinorVersion", "%u", le16toh(identify->MinorVersion));
    if(identify->MinPIOCycleTimeNoFlow)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinPIOCycleTimeNoFlow", "%u",
                                        le16toh(identify->MinPIOCycleTimeNoFlow));
    if(identify->MinPIOCycleTimeFlow)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinPIOCycleTimeFlow", "%u",
                                        le16toh(identify->MinPIOCycleTimeFlow));
    xmlTextWriterWriteElement(xmlWriter, BAD_CAST "Model", AtaToCString(identify->Model, 40));
    if(identify->MultipleMaxSectors)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MultipleMaxSectors", "%u", identify->MultipleMaxSectors);
    if(identify->MultipleSectorNumber)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MultipleSectorNumber", "%u",
                                        identify->MultipleSectorNumber);
    if(identify->NVCacheCaps)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NVCacheCaps", "%u", le16toh(identify->NVCacheCaps));
    if(identify->NVCacheSize)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NVCacheSize", "%u", le32toh(identify->NVCacheSize));
    if(identify->NVCacheWriteSpeed)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NVCacheWriteSpeed", "%u",
                                        le16toh(identify->NVCacheWriteSpeed));
    if(identify->NVEstimatedSpinUp)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NVEstimatedSpinUp", "%u", identify->NVEstimatedSpinUp);
    if(identify->PacketBusRelease)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PacketBusRelease", "%u",
                                        le16toh(identify->PacketBusRelease));
    if(identify->PIOTransferTimingMode)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PIOTransferTimingMode", "%u",
                                        identify->PIOTransferTimingMode);
    if(identify->RecommendedAAM)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "RecommendedAAM", "%u", identify->RecommendedAAM);
    if(identify->RecMDMACycleTime)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "RecMDMACycleTime", "%u",
                                        le16toh(identify->RecMDMACycleTime));
    if(identify->RemovableStatusSet)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "RemovableStatusSet", "%u",
                                        le16toh(identify->RemovableStatusSet));
    if(identify->SATACapabilities)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SATACapabilities",
                                  DecodeSATACapabilities(le16toh(identify->SATACapabilities)));
    if(identify->SATACapabilities2)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SATACapabilities2",
                                  DecodeSATACapabilities2(le16toh(identify->SATACapabilities2)));
    if(identify->SATAFeatures)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SATAFeatures",
                                  DecodeSATAFeatures(le16toh(identify->SATAFeatures)));
    if(identify->SCTCommandTransport)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SCTCommandTransport",
                                  DecodeSCTCommandTransport(le16toh(identify->SCTCommandTransport)));
    if(identify->SectorsPerCard)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SectorsPerCard", "%u", le32toh(identify->SectorsPerCard));
    if(identify->SecurityEraseTime)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SecurityEraseTime", "%u",
                                        le16toh(identify->SecurityEraseTime));
    if(identify->SecurityStatus)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SecurityStatus",
                                  DecodeSecurityStatus(le16toh(identify->SecurityStatus)));
    if(identify->ServiceBusyClear)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ServiceBusyClear", "%u",
                                        le16toh(identify->ServiceBusyClear));
    if(identify->SpecificConfiguration)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SpecificConfiguration",
                                  DecodeSpecificConfiguration(le16toh(identify->SpecificConfiguration)));
    if(identify->StreamAccessLatency)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StreamAccessLatency", "%u",
                                        le16toh(identify->StreamAccessLatency));
    if(identify->StreamMinReqSize)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StreamMinReqSize", "%u",
                                        le16toh(identify->StreamMinReqSize));
    if(identify->StreamPerformanceGranularity)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StreamPerformanceGranularity", "%u",
                                        le32toh(identify->StreamPerformanceGranularity));
    if(identify->StreamTransferTimeDMA)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StreamTransferTimeDMA", "%u",
                                        le16toh(identify->StreamTransferTimeDMA));
    if(identify->StreamTransferTimePIO)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StreamTransferTimePIO", "%u",
                                        le16toh(identify->StreamTransferTimePIO));
    if(identify->TransportMajorVersion)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "TransportMajorVersion", "%u",
                                        le16toh(identify->TransportMajorVersion));
    if(identify->TransportMinorVersion)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "TransportMinorVersion", "%u",
                                        le16toh(identify->TransportMinorVersion));
    if(identify->TrustedComputing)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "TrustedComputing",
                                  DecodeTrustedComputing(le16toh(identify->TrustedComputing)));
    if(identify->UDMAActive)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "UDMAActive", DecodeTransferMode(le16toh(identify->UDMAActive)));
    if(identify->UDMASupported)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "UDMASupported",
                                  DecodeTransferMode(le16toh(identify->UDMASupported)));
    if(identify->WRVMode)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WRVMode", "%u", identify->WRVMode);
    if(identify->WRVSectorCountMode3)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WRVSectorCountMode3", "%u",
                                        le32toh(identify->WRVSectorCountMode3));
    if(identify->WRVSectorCountMode2)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WRVSectorCountMode2", "%u",
                                        le32toh(identify->WRVSectorCountMode2));

    xmlTextWriterStartElement(xmlWriter, BAD_CAST "Identify");
    xmlTextWriterWriteBase64(xmlWriter, ata_ident, 0, 512);
    xmlTextWriterEndElement(xmlWriter);

    if(removable)
    {
        user_response = ' ';
        int anyMedia = FALSE;

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

                error = Identify(fd, &ata_ident, &ata_error_chs);

                if(!anyMedia)
                    xmlTextWriterStartElement(xmlWriter, BAD_CAST "RemovableMedias"); // <RemovableMedias>

                xmlTextWriterStartElement(xmlWriter, BAD_CAST "testedMediaType"); // <testedMediaType>

                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediaIsRecognized", "%s",
                                                !error ? "TRUE" : "FALSE");

                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediumTypeName", "%s", mediaName);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Model", "%s", mediaModel);

                if(error)
                {
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Manufacturer", "%s", mediaManufacturer);
                    xmlTextWriterEndElement(xmlWriter); // </testedMediaType>
                    anyMedia = TRUE;
                    continue;
                }

                free(identify);
                identify = malloc(512);
                memcpy(identify, ata_ident, 512);

                if(identify->UnformattedBPT)
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "UnformattedBPT", "%u",
                                                    le16toh(identify->UnformattedBPT));
                if(identify->UnformattedBPS)
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "UnformattedBPS", "%u",
                                                    le16toh(identify->UnformattedBPS));

                uint64_t blocks = 0;

                if(identify->Cylinders > 0 && identify->Heads > 0 && identify->SectorsPerTrack != 0)
                {
                    xmlTextWriterStartElement(xmlWriter, BAD_CAST "CHS"); // <CHS>
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Cylinders", "%u",
                                                    le16toh(identify->Cylinders));
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Heads", "%u", le16toh(identify->Heads));
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Sectors", "%u",
                                                    le16toh(identify->SectorsPerTrack));
                    blocks = le16toh(identify->Cylinders) * le16toh(identify->Heads) *
                             le16toh(identify->SectorsPerTrack);
                    xmlTextWriterEndElement(xmlWriter); // </CHS>
                }

                if(identify->CurrentCylinders > 0 && identify->CurrentHeads > 0 &&
                   identify->CurrentSectorsPerTrack != 0)
                {
                    xmlTextWriterStartElement(xmlWriter, BAD_CAST "CurrentCHS"); // <CurrentCHS>
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Cylinders", "%u",
                                                    le16toh(identify->CurrentCylinders));
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Heads", "%u", le16toh(identify->CurrentHeads));
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Sectors", "%u",
                                                    le16toh(identify->CurrentSectorsPerTrack));
                    if(blocks == 0)
                        blocks = le16toh(identify->CurrentCylinders) * le16toh(identify->CurrentHeads) *
                                 le16toh(identify->CurrentSectorsPerTrack);
                    xmlTextWriterEndElement(xmlWriter); // </CurrentCHS>
                }

                if(le16toh(identify->Capabilities) & 0x0200)
                {
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LBASectors", "%u",
                                                    le32toh(identify->LBASectors));
                    blocks = le32toh(identify->LBASectors);
                }

                if(le16toh(identify->CommandSet2) & 0x0400)
                {
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LBA48Sectors", "%llu",
                                                    le64toh(identify->LBA48Sectors));
                    blocks = le64toh(identify->LBA48Sectors);
                }

                if(identify->NominalRotationRate != 0x0000 && identify->NominalRotationRate != 0xFFFF)
                {
                    if(le16toh(identify->NominalRotationRate) == 0x0001)
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SolidStateDevice", "%s", "TRUE");
                    else
                    {
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SolidStateDevice", "%s", "TRUE");
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NominalRotationRate", "%u",
                                                        le16toh(identify->NominalRotationRate));
                    }
                }

                uint32_t logicalsectorsize  = 0;
                uint32_t physicalsectorsize = 0;

                if((le16toh(identify->PhysLogSectorSize) & 0x8000) == 0x0000 &&
                   (le16toh(identify->PhysLogSectorSize) & 0x4000) == 0x4000)
                {
                    if(le16toh(identify->PhysLogSectorSize) & 0x1000)
                    {
                        if(le16toh(identify->LogicalSectorWords) <= 255 || identify->LogicalAlignment == 0xFFFF)
                            logicalsectorsize = 512;
                        else
                            logicalsectorsize = le16toh(identify->LogicalSectorWords) * 2;
                    }
                    else
                        logicalsectorsize = 512;

                    if(le16toh(identify->PhysLogSectorSize) & 0x2000)
                        physicalsectorsize = logicalsectorsize * (1 << (le16toh(identify->PhysLogSectorSize) & 0xF));
                    else
                        physicalsectorsize = logicalsectorsize;
                }
                else
                {
                    logicalsectorsize  = 512;
                    physicalsectorsize = 512;
                }

                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BlockSize", "%u", logicalsectorsize);
                if(physicalsectorsize != logicalsectorsize)
                {
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalBlockSize", "%u", physicalsectorsize);
                    if((le16toh(identify->LogicalAlignment) & 0x8000) == 0x0000 &&
                       (le16toh(identify->LogicalAlignment) & 0x4000) == 0x4000)
                        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LogicalAlignment", "%u",
                                                        le16toh(identify->LogicalAlignment) & 0x3FFF);
                }

                uint16_t longblocksize = 0;
                if(identify->EccBytes != 0x0000 && identify->EccBytes != 0xFFFF)
                    longblocksize = le16toh(identify->EccBytes);

                if(le16toh(identify->UnformattedBPS) > logicalsectorsize &&
                   (longblocksize == 0 || longblocksize == 516))
                    longblocksize = le16toh(identify->UnformattedBPS);

                if(longblocksize > 0)
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LongBlockSize", "%u", longblocksize);

                if((le16toh(identify->CommandSet3) & 0x8000) == 0x0000 &&
                   (le16toh(identify->CommandSet3) & 0x4000) == 0x4000 &&
                   (le16toh(identify->EnabledCommandSet3) & 0x0004) == 0x0004)
                {
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadMediaSerial", "%s", "TRUE");
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Manufacturer", "%s",
                                                    AtaToCString(identify->MediaManufacturer, 20));
                }
                else
                    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Manufacturer", "%s", mediaManufacturer);

                printf("Trying READ SECTOR(S) in CHS mode...\n");
                error = Read(fd, &buffer, &ata_error_chs, FALSE, 0, 0, 1, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsRead", "%s",
                                                (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                                 ata_error_chs->error == 0 && buffer != NULL) ? "true" : "false");

                printf("Trying READ SECTOR(S) RETRY in CHS mode...\n");
                error = Read(fd, &buffer, &ata_error_chs, TRUE, 0, 0, 1, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadRetry", "%s",
                                                (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                                 ata_error_chs->error == 0 && buffer != NULL) ? "true" : "false");

                printf("Trying READ DMA in CHS mode...\n");
                error = ReadDma(fd, &buffer, &ata_error_chs, FALSE, 0, 0, 1, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadDma", "%s",
                                                (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                                 ata_error_chs->error == 0 && buffer != NULL) ? "true" : "false");

                printf("Trying READ DMA RETRY in CHS mode...\n");
                error = ReadDma(fd, &buffer, &ata_error_chs, TRUE, 0, 0, 1, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadDmaRetry", "%s",
                                                (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                                 ata_error_chs->error == 0 && buffer != NULL) ? "true" : "false");

                printf("Trying SEEK in CHS mode...\n");
                error = Seek(fd, &ata_error_chs, 0, 0, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsSeek", "%s",
                                                (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                                 ata_error_chs->error == 0) ? "true" : "false");

                printf("Trying READ SECTOR(S) in LBA mode...\n");
                error = ReadLba(fd, &buffer, &ata_error_lba, FALSE, 0, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLba", "%s",
                                                (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                                 ata_error_lba->error == 0 && buffer != NULL) ? "true" : "false");

                printf("Trying READ SECTOR(S) RETRY in LBA mode...\n");
                error = ReadLba(fd, &buffer, &ata_error_lba, TRUE, 0, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadRetryLba", "%s",
                                                (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                                 ata_error_lba->error == 0 && buffer != NULL) ? "true" : "false");

                printf("Trying READ DMA in LBA mode...\n");
                error = ReadDmaLba(fd, &buffer, &ata_error_lba, FALSE, 0, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadDmaLba", "%s",
                                                (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                                 ata_error_lba->error == 0 && buffer != NULL) ? "true" : "false");

                printf("Trying READ DMA RETRY in LBA mode...\n");
                error = ReadDmaLba(fd, &buffer, &ata_error_lba, TRUE, 0, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadDmaRetryLba", "%s",
                                                (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                                 ata_error_lba->error == 0 && buffer != NULL) ? "true" : "false");

                printf("Trying SEEK in LBA mode...\n");
                error = SeekLba(fd, &ata_error_lba, 0);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsSeekLba", "%s",
                                                (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                                 ata_error_lba->error == 0) ? "true" : "false");

                printf("Trying READ SECTOR(S) in LBA48 mode...\n");
                error = ReadLba48(fd, &buffer, &ata_error_lba48, 0, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLba48", "%s",
                                                (!error && (ata_error_lba48->status & 0x01) != 0x01 &&
                                                 ata_error_lba48->error == 0 && buffer != NULL) ? "true" : "false");

                printf("Trying READ DMA in LBA48 mode...\n");
                error = ReadDmaLba48(fd, &buffer, &ata_error_lba48, 0, 1);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadDmaLba48", "%s",
                                                (!error && (ata_error_lba48->status & 0x01) != 0x01 &&
                                                 ata_error_lba48->error == 0 && buffer != NULL) ? "true" : "false");


                printf("Trying READ LONG in CHS mode...\n");
                error = ReadLong(fd, &buffer, &ata_error_chs, FALSE, 0, 0, 1, longblocksize);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLong", "%s",
                                                (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                                 ata_error_chs->error == 0 && buffer != NULL &&
                                                 (uint64_t)(*buffer) != 0) ? "true" : "false");

                printf("Trying READ LONG RETRY in CHS mode...\n");
                error = ReadLong(fd, &buffer, &ata_error_chs, TRUE, 0, 0, 1, longblocksize);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLongRetry", "%s",
                                                (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                                 ata_error_chs->error == 0 && buffer != NULL &&
                                                 (uint64_t)(*buffer) != 0) ? "true" : "false");

                printf("Trying READ LONG in LBA mode...\n");
                error = ReadLongLba(fd, &buffer, &ata_error_lba, FALSE, 0, longblocksize);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLongLba", "%s",
                                                (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                                 ata_error_lba->error == 0 && buffer != NULL &&
                                                 (uint64_t)(*buffer) != 0) ? "true" : "false");

                printf("Trying READ LONG RETRY in LBA mode...\n");
                error = ReadLongLba(fd, &buffer, &ata_error_lba, TRUE, 0, longblocksize);
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLongRetryLba", "%s",
                                                (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                                 ata_error_lba->error == 0 && buffer != NULL &&
                                                 (uint64_t)(*buffer) != 0) ? "true" : "false");

                xmlTextWriterEndElement(xmlWriter); // </testedMediaType>

                if(!anyMedia)
                    anyMedia = TRUE;
            }
        }

        if(anyMedia)
            xmlTextWriterEndElement(xmlWriter); // </RemovableMedias>
    }
    else
    {
        error = Identify(fd, &ata_ident, &ata_error_chs);

        xmlTextWriterStartElement(xmlWriter, BAD_CAST "ReadCapabilities"); // <RemovableMedias>

        free(identify);
        identify = malloc(512);
        memcpy(identify, ata_ident, 512);

        if(identify->UnformattedBPT)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "UnformattedBPT", "%u",
                                            le16toh(identify->UnformattedBPT));
        if(identify->UnformattedBPS)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "UnformattedBPS", "%u",
                                            le16toh(identify->UnformattedBPS));

        uint64_t blocks;

        if(identify->Cylinders > 0 && identify->Heads > 0 && identify->SectorsPerTrack != 0)
        {
            xmlTextWriterStartElement(xmlWriter, BAD_CAST "CHS"); // <CHS>
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Cylinders", "%u", le16toh(identify->Cylinders));
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Heads", "%u", le16toh(identify->Heads));
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Sectors", "%u", le16toh(identify->SectorsPerTrack));
            blocks = le16toh(identify->Cylinders) * le16toh(identify->Heads) * le16toh(identify->SectorsPerTrack);
            xmlTextWriterEndElement(xmlWriter); // </CHS>
        }

        if(identify->CurrentCylinders > 0 && identify->CurrentHeads > 0 && identify->CurrentSectorsPerTrack != 0)
        {
            xmlTextWriterStartElement(xmlWriter, BAD_CAST "CurrentCHS"); // <CurrentCHS>
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Cylinders", "%u", le16toh(identify->CurrentCylinders));
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Heads", "%u", le16toh(identify->CurrentHeads));
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Sectors", "%u",
                                            le16toh(identify->CurrentSectorsPerTrack));
            if(blocks == 0)
                blocks = le16toh(identify->CurrentCylinders) * le16toh(identify->CurrentHeads) *
                         le16toh(identify->CurrentSectorsPerTrack);
            xmlTextWriterEndElement(xmlWriter); // </CurrentCHS>
        }

        if(le16toh(identify->Capabilities) & 0x0200)
        {
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LBASectors", "%u", le32toh(identify->LBASectors));
            blocks = le32toh(identify->LBASectors);
        }

        if(le16toh(identify->CommandSet2) & 0x0400)
        {
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LBA48Sectors", "%llu",
                                            le64toh(identify->LBA48Sectors));
            blocks = le64toh(identify->LBA48Sectors);
        }

        if(identify->NominalRotationRate != 0x0000 && identify->NominalRotationRate != 0xFFFF)
        {
            if(le16toh(identify->NominalRotationRate) == 0x0001)
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SolidStateDevice", "%s", "TRUE");
            else
            {
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SolidStateDevice", "%s", "TRUE");
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NominalRotationRate", "%u",
                                                le16toh(identify->NominalRotationRate));
            }
        }

        uint32_t logicalsectorsize  = 0;
        uint32_t physicalsectorsize = 0;

        if((le16toh(identify->PhysLogSectorSize) & 0x8000) == 0x0000 &&
           (le16toh(identify->PhysLogSectorSize) & 0x4000) == 0x4000)
        {
            if(le16toh(identify->PhysLogSectorSize) & 0x1000)
            {
                if(le16toh(identify->LogicalSectorWords) <= 255 || identify->LogicalAlignment == 0xFFFF)
                    logicalsectorsize = 512;
                else
                    logicalsectorsize = le16toh(identify->LogicalSectorWords) * 2;
            }
            else
                logicalsectorsize = 512;

            if(le16toh(identify->PhysLogSectorSize) & 0x2000)
                physicalsectorsize = logicalsectorsize * (1 << (le16toh(identify->PhysLogSectorSize) & 0xF));
            else
                physicalsectorsize = logicalsectorsize;
        }
        else
        {
            logicalsectorsize  = 512;
            physicalsectorsize = 512;
        }

        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BlockSize", "%u", logicalsectorsize);
        if(physicalsectorsize != logicalsectorsize)
        {
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PhysicalBlockSize", "%u", physicalsectorsize);
            if((le16toh(identify->LogicalAlignment) & 0x8000) == 0x0000 &&
               (le16toh(identify->LogicalAlignment) & 0x4000) == 0x4000)
                xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LogicalAlignment", "%u",
                                                le16toh(identify->LogicalAlignment) & 0x3FFF);
        }

        uint16_t longblocksize = 0;
        if(identify->EccBytes != 0x0000 && identify->EccBytes != 0xFFFF)
            longblocksize = le16toh(identify->EccBytes);

        if(le16toh(identify->UnformattedBPS) > logicalsectorsize && (longblocksize == 0 || longblocksize == 516))
            longblocksize = le16toh(identify->UnformattedBPS);

        if(longblocksize > 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LongBlockSize", "%u", longblocksize);

        if((le16toh(identify->CommandSet3) & 0x8000) == 0x0000 && (le16toh(identify->CommandSet3) & 0x4000) == 0x4000 &&
           (le16toh(identify->EnabledCommandSet3) & 0x0004) == 0x0004)
        {
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "CanReadMediaSerial", "%s", "TRUE");
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Manufacturer", "%s",
                                            AtaToCString(identify->MediaManufacturer, 20));
        }

        printf("Trying READ SECTOR(S) in CHS mode...\n");
        error = Read(fd, &buffer, &ata_error_chs, FALSE, 0, 0, 1, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsRead", "%s",
                                        (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                         ata_error_chs->error == 0 && buffer != NULL) ? "true" : "false");

        printf("Trying READ SECTOR(S) RETRY in CHS mode...\n");
        error = Read(fd, &buffer, &ata_error_chs, TRUE, 0, 0, 1, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadRetry", "%s",
                                        (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                         ata_error_chs->error == 0 && buffer != NULL) ? "true" : "false");

        printf("Trying READ DMA in CHS mode...\n");
        error = ReadDma(fd, &buffer, &ata_error_chs, FALSE, 0, 0, 1, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadDma", "%s",
                                        (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                         ata_error_chs->error == 0 && buffer != NULL) ? "true" : "false");

        printf("Trying READ DMA RETRY in CHS mode...\n");
        error = ReadDma(fd, &buffer, &ata_error_chs, TRUE, 0, 0, 1, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadDmaRetry", "%s",
                                        (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                         ata_error_chs->error == 0 && buffer != NULL) ? "true" : "false");

        printf("Trying SEEK in CHS mode...\n");
        error = Seek(fd, &ata_error_chs, 0, 0, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsSeek", "%s",
                                        (!error && (ata_error_chs->status & 0x01) != 0x01 && ata_error_chs->error == 0)
                                        ? "true" : "false");

        printf("Trying READ SECTOR(S) in LBA mode...\n");
        error = ReadLba(fd, &buffer, &ata_error_lba, FALSE, 0, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLba", "%s",
                                        (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                         ata_error_lba->error == 0 && buffer != NULL) ? "true" : "false");

        printf("Trying READ SECTOR(S) RETRY in LBA mode...\n");
        error = ReadLba(fd, &buffer, &ata_error_lba, TRUE, 0, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadRetryLba", "%s",
                                        (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                         ata_error_lba->error == 0 && buffer != NULL) ? "true" : "false");

        printf("Trying READ DMA in LBA mode...\n");
        error = ReadDmaLba(fd, &buffer, &ata_error_lba, FALSE, 0, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadDmaLba", "%s",
                                        (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                         ata_error_lba->error == 0 && buffer != NULL) ? "true" : "false");

        printf("Trying READ DMA RETRY in LBA mode...\n");
        error = ReadDmaLba(fd, &buffer, &ata_error_lba, TRUE, 0, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadDmaRetryLba", "%s",
                                        (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                         ata_error_lba->error == 0 && buffer != NULL) ? "true" : "false");

        printf("Trying SEEK in LBA mode...\n");
        error = SeekLba(fd, &ata_error_lba, 0);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsSeekLba", "%s",
                                        (!error && (ata_error_lba->status & 0x01) != 0x01 && ata_error_lba->error == 0)
                                        ? "true" : "false");

        printf("Trying READ SECTOR(S) in LBA48 mode...\n");
        error = ReadLba48(fd, &buffer, &ata_error_lba48, 0, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLba48", "%s",
                                        (!error && (ata_error_lba48->status & 0x01) != 0x01 &&
                                         ata_error_lba48->error == 0 && buffer != NULL) ? "true" : "false");

        printf("Trying READ DMA in LBA48 mode...\n");
        error = ReadDmaLba48(fd, &buffer, &ata_error_lba48, 0, 1);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadDmaLba48", "%s",
                                        (!error && (ata_error_lba48->status & 0x01) != 0x01 &&
                                         ata_error_lba48->error == 0 && buffer != NULL) ? "true" : "false");


        printf("Trying READ LONG in CHS mode...\n");
        error = ReadLong(fd, &buffer, &ata_error_chs, FALSE, 0, 0, 1, longblocksize);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLong", "%s",
                                        (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                         ata_error_chs->error == 0 && buffer != NULL && (uint64_t)(*buffer) != 0)
                                        ? "true" : "false");

        printf("Trying READ LONG RETRY in CHS mode...\n");
        error = ReadLong(fd, &buffer, &ata_error_chs, TRUE, 0, 0, 1, longblocksize);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLongRetry", "%s",
                                        (!error && (ata_error_chs->status & 0x01) != 0x01 &&
                                         ata_error_chs->error == 0 && buffer != NULL && (uint64_t)(*buffer) != 0)
                                        ? "true" : "false");

        printf("Trying READ LONG in LBA mode...\n");
        error = ReadLongLba(fd, &buffer, &ata_error_lba, FALSE, 0, longblocksize);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLongLba", "%s",
                                        (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                         ata_error_lba->error == 0 && buffer != NULL && (uint64_t)(*buffer) != 0)
                                        ? "true" : "false");

        printf("Trying READ LONG RETRY in LBA mode...\n");
        error = ReadLongLba(fd, &buffer, &ata_error_lba, TRUE, 0, longblocksize);
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsReadLongRetryLba", "%s",
                                        (!error && (ata_error_lba->status & 0x01) != 0x01 &&
                                         ata_error_lba->error == 0 && buffer != NULL && (uint64_t)(*buffer) != 0)
                                        ? "true" : "false");

        xmlTextWriterEndElement(xmlWriter); // </ReadCapabilities>
    }

    xmlTextWriterEndElement(xmlWriter);
}