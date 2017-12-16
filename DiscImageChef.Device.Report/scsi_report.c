//
// Created by claunia on 14/12/17.
//
#include <string.h>
#include <libxml/xmlwriter.h>
#include "scsi_report.h"
#include "scsi.h"
#include "inquiry_decode.h"
#include "scsi_mode.h"

void ScsiReport(int fd, xmlTextWriterPtr xmlWriter)
{
    unsigned char *sense = NULL;
    unsigned char *buffer = NULL;
    int error;
    int page_len;
    int removable = FALSE;
    char user_response = ' ';
    unsigned char* tmpString;

    printf("Querying SCSI INQUIRY...\n");

    error = Inquiry(fd, &buffer, &sense);

    if(error)
    {
        fprintf(stderr, "Error {0} requesting INQUIRY", error);
        return;
    }

    xmlTextWriterStartElement(xmlWriter, BAD_CAST DIC_SCSI_REPORT_ELEMENT); // <SCSI>
    page_len = *(buffer + 4) + 5;

    ScsiInquiry *inquiry = malloc(sizeof(ScsiInquiry));
    memset(inquiry, 0, sizeof(ScsiInquiry));
    memcpy(inquiry, buffer, page_len > sizeof(ScsiInquiry) ? sizeof(ScsiInquiry) : page_len);

    if(inquiry->RMB)
    {
        do
        {
            printf("Is the media removable from the reading/writing elements (flash memories ARE NOT removable)? (Y/N): ");
            scanf("%c", &user_response);
            printf("\n");
        } while(user_response != 'Y' && user_response != 'y' && user_response != 'N' && user_response != 'n');

        removable = (user_response == 'Y' || user_response == 'y');
    }

    xmlTextWriterStartElement(xmlWriter, BAD_CAST DIC_SCSI_INQUIRY_ELEMENT); // <Inquiry>
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "AccessControlCoordinator", "%s", inquiry->ACC ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ACKRequests", "%s", inquiry->ACKREQQ ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Address16", "%s", inquiry->Addr16 ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Address32", "%s", inquiry->Addr32 ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "AERCSupported", "%s", inquiry->AERC ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ANSIVersion", "%d", inquiry->ANSIVersion);
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "AsymmetricalLUNAccess", "%s", DecodeTPGSValues(inquiry->TPGS));
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BasicQueueing", "%s", inquiry->BQue ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DeviceTypeModifier", "%d", inquiry->DeviceTypeModifier);
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ECMAVersion", "%d", inquiry->ECMAVersion);
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "EnclosureServices", "%s", inquiry->EncServ ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "HierarchicalLUN", "%s", inquiry->HiSup ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ISOVersion", "%d", inquiry->ISOVersion);
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "IUS", "%s", inquiry->IUS ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "LinkedCommands", "%s", inquiry->Linked ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MediumChanger", "%s", inquiry->MChngr ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MultiPortDevice", "%s", inquiry->MultiP ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NormalACA", "%s", inquiry->NormACA ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PeripheralDeviceType", "%s", DecodePeripheralDeviceType(inquiry->PeripheralDeviceType));
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PeripheralQualifier", "%s", DecodePeripheralQualifier(inquiry->PeripheralQualifier));
    tmpString = malloc(17);
    memset(tmpString, 0, 17);
    memcpy(tmpString, inquiry->ProductIdentification, 16);
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ProductIdentification", "%s", tmpString);
    free(tmpString);
    tmpString = malloc(5);
    memset(tmpString, 0, 5);
    memcpy(tmpString, inquiry->ProductRevisionLevel, 4);
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ProductRevisionLevel", "%s", tmpString);
    free(tmpString);
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Protection", "%s", inquiry->Protect ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "QAS", "%s", inquiry->QAS ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "RelativeAddressing", "%s", inquiry->RelAddr ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Removable", "%s", inquiry->RMB ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ResponseDataFormat", "%d", inquiry->ResponseDataFormat);
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SoftReset", "%s", inquiry->SftRe ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SPIClocking", "%s", DecodeSPIClocking(inquiry->Clocking));
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StorageArrayController", "%s", inquiry->SCCS ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SyncTransfer", "%s", inquiry->Sync ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "TaggedCommandQueue", "%s", inquiry->CmdQue ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "TerminateTaskSupported", "%s", inquiry->TrmTsk ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ThirdPartyCopy", "%s", inquiry->ThreePC ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "TranferDisable", "%s", inquiry->TranDis ? "true" : "false");
    tmpString = malloc(9);
    memset(tmpString, 0, 9);
    memcpy(tmpString, inquiry->VendorIdentification, 8);
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "VendorIdentification", "%8s", tmpString);
    free(tmpString);
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WideBus16", "%s", inquiry->WBus16 ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WideBus32", "%s", inquiry->WBus32 ? "true" : "false");

    xmlTextWriterStartElement(xmlWriter, BAD_CAST "Data");
    xmlTextWriterWriteBase64(xmlWriter, buffer, 0, page_len);
    xmlTextWriterEndElement(xmlWriter);
    xmlTextWriterEndElement(xmlWriter); // </Inquiry>

    // TODO: EVPDs

    if(removable)
    {
        if(inquiry->PeripheralDeviceType == 0x05) // MultiMediaDevice
        {
            AllowMediumRemoval(fd, &sense);
            EjectTray(fd, &sense);
        }
        else if(inquiry->PeripheralDeviceType == 0x05) // SequentialAccess
        {
            SpcAllowMediumRemoval(fd, &sense);
            printf("Asking drive to unload tape (can take a few minutes)...\n");
            Unload(fd, &sense);
        }
        printf("Please remove any media from the device and press any key when it is out.\n");
        scanf("%c");
    }

    int supportsMode6 = FALSE;
    int supportsMode10 = FALSE;
    int supportsModeSubpages = FALSE;
    unsigned char* mode6Response = NULL;
    unsigned char* mode10Response = NULL;

    printf("Querying all mode pages and subpages using SCSI MODE SENSE (10)...\n");
    error = ModeSense10(fd, &mode10Response, &sense, FALSE, TRUE, MODE_PAGE_DEFAULT, 0x3F, 0xFF);

    if(error)
    {
        printf("Querying all mode pages using SCSI MODE SENSE (10)...");
        error = ModeSense10(fd, &mode10Response, &sense, FALSE, TRUE, MODE_PAGE_DEFAULT, 0x3F, 0x00);
        if(!error)
            supportsMode10 = TRUE;
    }
    else
    {
        supportsMode10 = TRUE;
        supportsModeSubpages = TRUE;
    }

    printf("Querying all mode pages and subpages using SCSI MODE SENSE (6)...\n");
    error = ModeSense6(fd, &mode6Response, &sense, FALSE, MODE_PAGE_DEFAULT, 0x3F, 0xFF);
    if(error)
    {
        printf("Querying all mode pages using SCSI MODE SENSE (6)...");
        error = ModeSense6(fd, &mode6Response, &sense, FALSE, MODE_PAGE_DEFAULT, 0x3F, 0x00);
        if(error)
        {
            printf("Querying SCSI MODE SENSE (6)...");
            error = ModeSense6(fd, &mode6Response, &sense, FALSE, MODE_PAGE_DEFAULT, 0x00, 0x00);
            if(!error)
                supportsMode6 = TRUE;
        }
        else
            supportsMode6 = TRUE;
    }
    else
    {
        supportsMode6 = TRUE;
        supportsModeSubpages = TRUE;
    }

    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsModeSense6", "%s", supportsMode6 ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsModeSense10", "%s", supportsMode10 ? "true" : "false");
    xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SupportsModeSubpages", "%s", supportsModeSubpages ? "true" : "false");

    if(supportsMode6)
    {
        xmlTextWriterStartElement(xmlWriter, BAD_CAST "ModeSense6Data");
        xmlTextWriterWriteBase64(xmlWriter, mode6Response, 0, *(mode6Response + 0) + 1);
        xmlTextWriterEndElement(xmlWriter);
    }

    if(supportsMode10)
    {
        xmlTextWriterStartElement(xmlWriter, BAD_CAST "ModeSense10Data");
        xmlTextWriterWriteBase64(xmlWriter, mode10Response, 0, (*(mode10Response + 0) << 8) + *(mode10Response + 1) + 2);
        xmlTextWriterEndElement(xmlWriter);
    }

    DecodedMode decMode;
    memset(&decMode, 0, sizeof(DecodedMode));

    if(supportsMode10)
        decMode = DecodeMode10(mode10Response, inquiry->PeripheralDeviceType);
    else if(supportsMode6)
        decMode = DecodeMode6(mode6Response, inquiry->PeripheralDeviceType);

    if(decMode.decoded)
    {
        int page, subpage;

        xmlTextWriterStartElement(xmlWriter, BAD_CAST "ModeSense"); // <ModeSense>
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BlankCheckEnabled", "%s", decMode.Header.EBC ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DPOandFUA", "%s", decMode.Header.DPOFUA ? "true" : "false");
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WriteProtected", "%s", decMode.Header.WriteProtected ? "true" : "false");

        if(decMode.Header.BufferedMode > 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BlankCheckEnabled", "%d", decMode.Header.BufferedMode);
        if(decMode.Header.Speed > 0)
            xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "Speed", "%d", decMode.Header.Speed);

        for(page = 0; page < 256; page++)
        {
            for(subpage = 0; subpage < 256; subpage++)
            {
                if(decMode.pageSizes[page][subpage] > 0 && decMode.Pages[page][subpage] != NULL)
                {
                    xmlTextWriterStartElement(xmlWriter, BAD_CAST "modePageType");
                    xmlTextWriterWriteFormatAttribute(xmlWriter, BAD_CAST "page", "%d", page);
                    xmlTextWriterWriteFormatAttribute(xmlWriter, BAD_CAST "subpage", "%d", subpage);
                    xmlTextWriterWriteBase64(xmlWriter, decMode.Pages[page][subpage], 0, decMode.pageSizes[page][subpage]);
                    xmlTextWriterEndElement(xmlWriter);

                    if(page == 0x2A && subpage == 0x00)
                    {
                        // TODO: Decode CD-ROM page
                    }
                }
            }
        }
        xmlTextWriterEndElement(xmlWriter); // </ModeSense>
    }

    xmlTextWriterEndElement(xmlWriter); // </SCSI>
}

ModeHeader DecodeModeHeader6(unsigned char* modeResponse, uint8_t deviceType)
{
    int i;
    ModeHeader header;

    if(modeResponse[3])
    {
        header.descriptorsLength = modeResponse[3] / 8;
        for(i = 0; i < header.descriptorsLength; i++)
        {
            header.BlockDescriptors[i].Density = modeResponse[0 + i * 8 + 4];
            header.BlockDescriptors[i].Blocks += (uint64_t)(modeResponse[1 + i * 8 + 4] << 16);
            header.BlockDescriptors[i].Blocks += (uint64_t)(modeResponse[2 + i * 8 + 4] << 8);
            header.BlockDescriptors[i].Blocks += modeResponse[3 + i * 8 + 4];
            header.BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[5 + i * 8 + 4] << 16);
            header.BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[6 + i * 8 + 4] << 8);
            header.BlockDescriptors[i].BlockLength += modeResponse[7 + i * 8 + 4];
        }
    }

    if(deviceType == 0x00 || deviceType == 0x05)
    {
        header.WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
        header.DPOFUA = ((modeResponse[2] & 0x10) == 0x10);
    }

    if(deviceType == 0x01)
    {
        header.WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
        header.Speed = (uint8_t)(modeResponse[2] & 0x0F);
        header.BufferedMode = (uint8_t)((modeResponse[2] & 0x70) >> 4);
    }

    if(deviceType == 0x02)
        header.BufferedMode = (uint8_t)((modeResponse[2] & 0x70) >> 4);

    if(deviceType == 0x07)
    {
        header.WriteProtected = ((modeResponse[2] & 0x80) == 0x80);
        header.EBC = ((modeResponse[2] & 0x01) == 0x01);
        header.DPOFUA = ((modeResponse[2] & 0x10) == 0x10);
    }

    header.decoded = 1;

    return header;
}

ModeHeader DecodeModeHeader10(unsigned char* modeResponse, uint8_t deviceType)
{
    uint16_t blockDescLength = (uint16_t)((modeResponse[6] << 8) + modeResponse[7]);
    int i;
    ModeHeader header;
    header.MediumType = modeResponse[2];

    int longLBA = (modeResponse[4] & 0x01) == 0x01;

    if(blockDescLength > 0)
    {
        if(longLBA)
        {
            header.descriptorsLength = blockDescLength / 16;
            for(i = 0; i < header.descriptorsLength; i++)
            {
                header.BlockDescriptors[i].Density = 0x00;
                header.BlockDescriptors[i].Blocks = be64toh((uint64_t)(*modeResponse + 0 + i * 16 + 8));
                header.BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[15 + i * 16 + 8] << 24);
                header.BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[14 + i * 16 + 8] << 16);
                header.BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[13 + i * 16 + 8] << 8);
                header.BlockDescriptors[i].BlockLength += modeResponse[12 + i * 16 + 8];
            }
        }
        else
        {
            header.descriptorsLength = blockDescLength / 8;
            for(i = 0; i < header.descriptorsLength; i++)
            {
                if(deviceType != 0x00)
                {
                    header.BlockDescriptors[i].Density = modeResponse[0 + i * 8 + 8];
                }
                else
                {
                    header.BlockDescriptors[i].Density = 0x00;
                    header.BlockDescriptors[i].Blocks += (uint64_t)(modeResponse[0 + i * 8 + 8] << 24);
                }
                header.BlockDescriptors[i].Blocks += (uint64_t)(modeResponse[1 + i * 8 + 8] << 16);
                header.BlockDescriptors[i].Blocks += (uint64_t)(modeResponse[2 + i * 8 + 8] << 8);
                header.BlockDescriptors[i].Blocks += modeResponse[3 + i * 8 + 8];
                header.BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[5 + i * 8 + 8] << 16);
                header.BlockDescriptors[i].BlockLength += (uint32_t)(modeResponse[6 + i * 8 + 8] << 8);
                header.BlockDescriptors[i].BlockLength += modeResponse[7 + i * 8 + 8];
            }
        }
    }

    if(deviceType == 0x00 || deviceType == 0x05)
    {
        header.WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
        header.DPOFUA = ((modeResponse[3] & 0x10) == 0x10);
    }

    if(deviceType == 0x01)
    {
        header.WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
        header.Speed = (uint8_t)(modeResponse[3] & 0x0F);
        header.BufferedMode = (uint8_t)((modeResponse[3] & 0x70) >> 4);
    }

    if(deviceType == 0x02)
        header.BufferedMode = (uint8_t)((modeResponse[3] & 0x70) >> 4);

    if(deviceType == 0x07)
    {
        header.WriteProtected = ((modeResponse[3] & 0x80) == 0x80);
        header.EBC = ((modeResponse[3] & 0x01) == 0x01);
        header.DPOFUA = ((modeResponse[3] & 0x10) == 0x10);
    }

    header.decoded = 1;

    return header;
}

DecodedMode DecodeMode6(unsigned char* modeResponse, uint8_t deviceType)
{
    DecodedMode decoded;

    ModeHeader hdr = DecodeModeHeader6(modeResponse, deviceType);
    if(!hdr.decoded)
        return decoded;

    decoded.Header = hdr;
    decoded.decoded = 1;

    int offset = 4 + decoded.Header.descriptorsLength * 8;
    int length = modeResponse[0] + 1;

    while(offset < length)
    {
        int isSubpage = (modeResponse[offset] & 0x40) == 0x40;

        uint8_t pageNo = (uint8_t)(modeResponse[offset] & 0x3F);
        int subpage;

        if(pageNo == 0)
        {
            decoded.pageSizes[0][0] = (size_t)(length - offset);
            decoded.Pages[0][0] = malloc(decoded.pageSizes[0][0]);
            memset(decoded.Pages[0][0], 0, decoded.pageSizes[0][0]);
            memcpy(decoded.Pages[0][0], modeResponse + offset, decoded.pageSizes[0][0]);
            offset += decoded.pageSizes[0][0];
        }
        else
        {
            if(isSubpage)
            {
                if(offset + 3 >= length)
                    break;

                pageNo = (uint8_t)(modeResponse[offset] & 0x3F);
                subpage = modeResponse[offset + 1];
                decoded.pageSizes[pageNo][subpage] = (size_t)((modeResponse[offset + 2] << 8) + modeResponse[offset + 3] + 4);
                decoded.Pages[pageNo][subpage] = malloc(decoded.pageSizes[pageNo][subpage]);
                memset(decoded.Pages[pageNo][subpage], 0, decoded.pageSizes[pageNo][subpage]);
                memcpy(decoded.Pages[pageNo][subpage], modeResponse + offset, decoded.pageSizes[pageNo][subpage]);
                offset += decoded.pageSizes[pageNo][subpage];
            }
            else
            {
                if(offset + 1 >= length)
                    break;

                pageNo = (uint8_t)(modeResponse[offset] & 0x3F);
                decoded.pageSizes[pageNo][0] = (size_t)(modeResponse[offset + 1] + 2);
                decoded.Pages[pageNo][0] = malloc(decoded.pageSizes[pageNo][0]);
                memset(decoded.Pages[pageNo][0], 0, decoded.pageSizes[pageNo][0]);
                memcpy(decoded.Pages[pageNo][0], modeResponse + offset, decoded.pageSizes[pageNo][0]);
                offset += decoded.pageSizes[pageNo][0];
            }
        }
    }

    return decoded;
}

DecodedMode DecodeMode10(unsigned char* modeResponse, uint8_t deviceType)
{
    DecodedMode decodedMode;

    decodedMode.Header = DecodeModeHeader10(modeResponse, deviceType);

    if(!decodedMode.Header.decoded)
        return decodedMode;

    decodedMode.decoded = 1;

    int longlba = (modeResponse[4] & 0x01) == 0x01;
    int offset;

    if(longlba)
        offset = 8 + decodedMode.Header.descriptorsLength * 16;
    else
        offset = 8 + decodedMode.Header.descriptorsLength * 8;
    int length = (modeResponse[0] << 8);
    length += modeResponse[1];
    length += 2;

    while(offset < length)
    {
        int isSubpage = (modeResponse[offset] & 0x40) == 0x40;

        uint8_t pageNo = (uint8_t)(modeResponse[offset] & 0x3F);
        int subpage;

        if(pageNo == 0)
        {
            decodedMode.pageSizes[0][0] = (size_t)(length - offset);
            decodedMode.Pages[0][0] = malloc(decodedMode.pageSizes[0][0]);
            memset(decodedMode.Pages[0][0], 0, decodedMode.pageSizes[0][0]);
            memcpy(decodedMode.Pages[0][0], modeResponse + offset, decodedMode.pageSizes[0][0]);
            offset += decodedMode.pageSizes[0][0];
        }
        else
        {
            if(isSubpage)
            {
                if(offset + 3 >= length)
                    break;

                pageNo = (uint8_t)(modeResponse[offset] & 0x3F);
                subpage = modeResponse[offset + 1];
                decodedMode.pageSizes[pageNo][subpage] = (size_t)((modeResponse[offset + 2] << 8) + modeResponse[offset + 3] + 4);
                decodedMode.Pages[pageNo][subpage] = malloc(decodedMode.pageSizes[pageNo][subpage]);
                memset(decodedMode.Pages[pageNo][subpage], 0, decodedMode.pageSizes[pageNo][subpage]);
                memcpy(decodedMode.Pages[pageNo][subpage], modeResponse + offset, decodedMode.pageSizes[pageNo][subpage]);
                offset += decodedMode.pageSizes[pageNo][subpage];
            }
            else
            {
                if(offset + 1 >= length)
                    break;

                pageNo = (uint8_t)(modeResponse[offset] & 0x3F);
                decodedMode.pageSizes[pageNo][0] = (size_t)(modeResponse[offset + 1] + 2);
                decodedMode.Pages[pageNo][0] = malloc(decodedMode.pageSizes[pageNo][0]);
                memset(decodedMode.Pages[pageNo][0], 0, decodedMode.pageSizes[pageNo][0]);
                memcpy(decodedMode.Pages[pageNo][0], modeResponse + offset, decodedMode.pageSizes[pageNo][0]);
                offset += decodedMode.pageSizes[pageNo][0];
            }
        }
    }

    return decodedMode;
}