//
// Created by claunia on 15/12/17.
//

#include <stdint.h>
#include <string.h>
#include "inquiry_decode.h"

char *DecodeTPGSValues(uint8_t capabilities)
{
    switch(capabilities)
    {
        case 0:
            return "NotSupported";
        case 1:
            return "OnlyImplicit";
        case 2:
            return "OnlyExplicit";
        case 3:
            return "Both";
        default:
            return NULL;
    }
}

char *DecodePeripheralDeviceType(uint8_t type)
{
    switch(type)
    {
        case 0x00:
            return "DirectAccess";
        case 0x01:
            return "SequentialAccess";
        case 0x02:
            return "PrinterDevice";
        case 0x03:
            return "ProcessorDevice";
        case 0x04:
            return "WriteOnceDevice";
        case 0x05:
            return "MultiMediaDevice";
        case 0x06:
            return "ScannerDevice";
        case 0x07:
            return "OpticalDevice";
        case 0x08:
            return "MediumChangerDevice";
        case 0x09:
            return "CommsDevice";
        case 0x0A:
            return "PrePressDevice1";
        case 0x0B:
            return "PrePressDevice2";
        case 0x0C:
            return "ArrayControllerDevice";
        case 0x0D:
            return "EnclosureServiceDevice";
        case 0x0E:
            return "SimplifiedDevice";
        case 0x0F:
            return "OCRWDevice";
        case 0x10:
            return "BridgingExpander";
        case 0x11:
            return "ObjectDevice";
        case 0x12:
            return "ADCDevice";
        case 0x13:
            return "SCSISecurityManagerDevice";
        case 0x14:
            return "SCSIZonedBlockDevice";
        case 0x1E:
            return "WellKnownDevice";
        case 0x1F:
            return "UnknownDevice";
        default:
            return NULL;
    }
}

char *DecodePeripheralQualifier(uint8_t qualifier)
{
    switch(qualifier)
    {
        case 0:
            return "Supported";
        case 1:
            return "Unconnected";
        case 2:
            return "Reserved";
        case 3:
            return "Unsupported";
        default:
            return NULL;
    }
}

char *DecodeSPIClocking(uint8_t qualifier)
{
    switch(qualifier)
    {
        case 0:
            return "ST";
        case 1:
            return "DT";
        case 2:
            return "Reserved";
        case 3:
            return "StandDT";
        default:
            return NULL;
    }
}