//
// Created by claunia on 14/12/17.
//
#include <string.h>
#include <libxml/xmlwriter.h>
#include "scsi_report.h"
#include "scsi.h"
#include "inquiry_decode.h"

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

    xmlTextWriterStartElement(xmlWriter, BAD_CAST DIC_SCSI_REPORT_ELEMENT);
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
    xmlTextWriterStartElement(xmlWriter, BAD_CAST DIC_SCSI_INQUIRY_ELEMENT);

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

    xmlTextWriterEndElement(xmlWriter);
    xmlTextWriterEndElement(xmlWriter);
}