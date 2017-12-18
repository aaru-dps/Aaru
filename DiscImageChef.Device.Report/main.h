//
// Created by claunia on 11/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_MAIN_H
#define DISCIMAGECHEF_DEVICE_REPORT_MAIN_H

#define DIC_VERSION "3.99.6.0"
#define DIC_COPYRIGHT "Copyright © 2011-2017 Natalia Portillo"
#define XML_ENCODING "UTF-8"
#define DIC_REPORT_ROOT "DicDeviceReport"

typedef enum
{
    DEVICE_TYPE_UNKNOWN,
    DEVICE_TYPE_SCSI,
    DEVICE_TYPE_ATA,
    DEVICE_TYPE_ATAPI,
    DEVICE_TYPE_USB,
    DEVICE_TYPE_FIREWIRE,
    DEVICE_TYPE_PCMCIA,
    DEVICE_TYPE_MMC,
    DEVICE_TYPE_SD
} DeviceTypes;

const char *DeviceType[] = {"Unknown", "SCSI", "ATA", "ATAPI", "USB", "FireWire", "PCMCIA", "MultiMediaCard",
                            "SecureDigital"};
#endif //DISCIMAGECHEF_DEVICE_REPORT_MAIN_H
