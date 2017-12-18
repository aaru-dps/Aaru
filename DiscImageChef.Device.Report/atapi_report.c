//
// Created by claunia on 13/12/17.
//

#include <string.h>
#include <libxml/xmlwriter.h>
#include "atapi_report.h"
#include "ata.h"
#include "atapi.h"
#include "identify_decode.h"

void AtapiReport(int fd, xmlTextWriterPtr xmlWriter)
{
    unsigned char *atapi_ident = NULL;
    AtaErrorRegistersCHS *ata_error_chs;
    int error;

    printf("Querying ATAPI IDENTIFY...\n");

    error = IdentifyPacket(fd, &atapi_ident, &ata_error_chs);

    if(error)
    {
        fprintf(stderr, "Error {0} requesting IDENTIFY PACKET DEVICE", error);
        return;
    }

    IdentifyDevice *identify = malloc(512);
    memcpy(identify, atapi_ident, 512);

    xmlTextWriterStartElement(xmlWriter, BAD_CAST DIC_ATAPI_REPORT_ELEMENT); // <ATA>

    if((uint64_t)*identify->AdditionalPID != 0 && (uint64_t)*identify->AdditionalPID != 0x2020202020202020)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "AdditionalPid", AtaToCString(identify->AdditionalPID, 8));
    if(identify->APIOSupported)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "APIOSupported", DecodeTransferMode(le16toh(identify->APIOSupported)));
    if(identify->ATAPIByteCount)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ATAPIByteCount", "%u", le16toh(identify->ATAPIByteCount));
    if(identify->BufferType)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BufferType", "%u", le16toh(identify->BufferType));
    if(identify->BufferSize)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "BufferSize", "%u", le16toh(identify->BufferSize));
    if(identify->Capabilities)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "Capabilities", DecodeCapabilities(le16toh(identify->Capabilities)));
    if(identify->Capabilities2)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "Capabilities2", DecodeCapabilities2(le16toh(identify->Capabilities2)));
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
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DataSetMgmtSize", "%u", le16toh(identify->DataSetMgmtSize));
    if(identify->DeviceFormFactor)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "DeviceFormFactor", DecodeDeviceFormFactor(le16toh(identify->DeviceFormFactor)));
    if(identify->DMAActive)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "DMAActive", DecodeTransferMode(le16toh(identify->DMAActive)));
    if(identify->DMASupported)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "DMASupported", DecodeTransferMode(le16toh(identify->DMASupported)));
    if(identify->DMATransferTimingMode)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "DMATransferTimingMode", "%u", identify->DMATransferTimingMode);
    if(identify->EnhancedSecurityEraseTime)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "EnhancedSecurityEraseTime", "%u", le16toh(identify->EnhancedSecurityEraseTime));
    if(identify->EnabledCommandSet)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "EnabledCommandSet", DecodeCommandSet(le16toh(identify->EnabledCommandSet)));
    if(identify->EnabledCommandSet2)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "EnabledCommandSet2", DecodeCommandSet2(le16toh(identify->EnabledCommandSet2)));
    if(identify->EnabledCommandSet3)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "EnabledCommandSet3", DecodeCommandSet3(le16toh(identify->EnabledCommandSet3)));
    if(identify->EnabledCommandSet4)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "EnabledCommandSet4", DecodeCommandSet4(le16toh(identify->EnabledCommandSet4)));
    if(identify->EnabledSATAFeatures)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "EnabledSATAFeatures", DecodeSATAFeatures(le16toh(identify->EnabledSATAFeatures)));
    if(identify->ExtendedUserSectors)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ExtendedUserSectors", "%llu", le64toh(identify->ExtendedUserSectors));
    if(identify->FreeFallSensitivity)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "FreeFallSensitivity", "%u", identify->FreeFallSensitivity);
    xmlTextWriterWriteElement(xmlWriter, BAD_CAST "FirmwareRevision", AtaToCString(identify->FirmwareRevision, 8));
    if(identify->GeneralConfiguration)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "GeneralConfiguration", DecodeGeneralConfiguration(le16toh(identify->GeneralConfiguration)));
    if(identify->HardwareResetResult)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "HardwareResetResult", "%u", le16toh(identify->HardwareResetResult));
    if(identify->InterseekDelay)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "InterseekDelay", "%u", le16toh(identify->InterseekDelay));
    if(identify->MajorVersion)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "MajorVersion", DecodeMajorVersion(le16toh(identify->MajorVersion)));
    if(identify->MasterPasswordRevisionCode)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MasterPasswordRevisionCode", "%u", le16toh(identify->MasterPasswordRevisionCode));
    if(identify->MaxDownloadMicroMode3)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MaxDownloadMicroMode3", "%u", le16toh(identify->MaxDownloadMicroMode3));
    if(identify->MaxQueueDepth)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MaxQueueDepth", "%u", le16toh(identify->MaxQueueDepth));
    if(identify->MDMAActive)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "MDMAActive", DecodeTransferMode(le16toh(identify->MDMAActive)));
    if(identify->MDMASupported)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "MDMASupported", DecodeTransferMode(le16toh(identify->MDMASupported)));
    if(identify->MinDownloadMicroMode3)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinDownloadMicroMode3", "%u", le16toh(identify->MinDownloadMicroMode3));
    if(identify->MinMDMACycleTime)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinMDMACycleTime", "%u", le16toh(identify->MinMDMACycleTime));
    if(identify->MinorVersion)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinorVersion", "%u", le16toh(identify->MinorVersion));
    if(identify->MinPIOCycleTimeNoFlow)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinPIOCycleTimeNoFlow", "%u", le16toh(identify->MinPIOCycleTimeNoFlow));
    if(identify->MinPIOCycleTimeFlow)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MinPIOCycleTimeFlow", "%u", le16toh(identify->MinPIOCycleTimeFlow));
    xmlTextWriterWriteElement(xmlWriter, BAD_CAST "Model", AtaToCString(identify->Model, 40));
    if(identify->MultipleMaxSectors)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MultipleMaxSectors", "%u", identify->MultipleMaxSectors);
    if(identify->MultipleSectorNumber)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "MultipleSectorNumber", "%u", identify->MultipleSectorNumber);
    if(identify->NVCacheCaps)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NVCacheCaps", "%u", le16toh(identify->NVCacheCaps));
    if(identify->NVCacheSize)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NVCacheSize", "%u", le32toh(identify->NVCacheSize));
    if(identify->NVCacheWriteSpeed)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NVCacheWriteSpeed", "%u", le16toh(identify->NVCacheWriteSpeed));
    if(identify->NVEstimatedSpinUp)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "NVEstimatedSpinUp", "%u", identify->NVEstimatedSpinUp);
    if(identify->PacketBusRelease)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PacketBusRelease", "%u", le16toh(identify->PacketBusRelease));
    if(identify->PIOTransferTimingMode)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "PIOTransferTimingMode", "%u", identify->PIOTransferTimingMode);
    if(identify->RecommendedAAM)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "RecommendedAAM", "%u", identify->RecommendedAAM);
    if(identify->RecMDMACycleTime)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "RecMDMACycleTime", "%u", le16toh(identify->RecMDMACycleTime));
    if(identify->RemovableStatusSet)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "RemovableStatusSet", "%u", le16toh(identify->RemovableStatusSet));
    if(identify->SATACapabilities)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SATACapabilities", DecodeSATACapabilities(le16toh(identify->SATACapabilities)));
    if(identify->SATACapabilities2)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SATACapabilities2", DecodeSATACapabilities2(le16toh(identify->SATACapabilities2)));
    if(identify->SATAFeatures)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SATAFeatures", DecodeSATAFeatures(le16toh(identify->SATAFeatures)));
    if(identify->SCTCommandTransport)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SCTCommandTransport", DecodeSCTCommandTransport(le16toh(identify->SCTCommandTransport)));
    if(identify->SectorsPerCard)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SectorsPerCard", "%u", le32toh(identify->SectorsPerCard));
    if(identify->SecurityEraseTime)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "SecurityEraseTime", "%u", le16toh(identify->SecurityEraseTime));
    if(identify->SecurityStatus)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SecurityStatus", DecodeSecurityStatus(le16toh(identify->SecurityStatus)));
    if(identify->ServiceBusyClear)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "ServiceBusyClear", "%u", le16toh(identify->ServiceBusyClear));
    if(identify->SpecificConfiguration)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "SpecificConfiguration", DecodeSpecificConfiguration(le16toh(identify->SpecificConfiguration)));
    if(identify->StreamAccessLatency)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StreamAccessLatency", "%u", le16toh(identify->StreamAccessLatency));
    if(identify->StreamMinReqSize)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StreamMinReqSize", "%u", le16toh(identify->StreamMinReqSize));
    if(identify->StreamPerformanceGranularity)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StreamPerformanceGranularity", "%u", le32toh(identify->StreamPerformanceGranularity));
    if(identify->StreamTransferTimeDMA)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StreamTransferTimeDMA", "%u", le16toh(identify->StreamTransferTimeDMA));
    if(identify->StreamTransferTimePIO)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "StreamTransferTimePIO", "%u", le16toh(identify->StreamTransferTimePIO));
    if(identify->TransportMajorVersion)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "TransportMajorVersion", "%u", le16toh(identify->TransportMajorVersion));
    if(identify->TransportMinorVersion)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "TransportMinorVersion", "%u", le16toh(identify->TransportMinorVersion));
    if(identify->TrustedComputing)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "TrustedComputing", DecodeTrustedComputing(le16toh(identify->TrustedComputing)));
    if(identify->UDMAActive)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "UDMAActive", DecodeTransferMode(le16toh(identify->UDMAActive)));
    if(identify->UDMASupported)
        xmlTextWriterWriteElement(xmlWriter, BAD_CAST "UDMASupported", DecodeTransferMode(le16toh(identify->UDMASupported)));
    if(identify->WRVMode)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WRVMode", "%u", identify->WRVMode);
    if(identify->WRVSectorCountMode3)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WRVSectorCountMode3", "%u", le32toh(identify->WRVSectorCountMode3));
    if(identify->WRVSectorCountMode2)
        xmlTextWriterWriteFormatElement(xmlWriter, BAD_CAST "WRVSectorCountMode2", "%u", le32toh(identify->WRVSectorCountMode2));

    xmlTextWriterStartElement(xmlWriter, BAD_CAST "Identify"); // <Identify>
    xmlTextWriterWriteBase64(xmlWriter, atapi_ident, 0, 512);
    xmlTextWriterEndElement(xmlWriter); // </Identify>

    xmlTextWriterEndElement(xmlWriter); // </ATA>
}