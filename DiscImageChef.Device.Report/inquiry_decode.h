//
// Created by claunia on 15/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_INQUIRY_DECODE_H
#define DISCIMAGECHEF_DEVICE_REPORT_INQUIRY_DECODE_H

char *DecodeTPGSValues(uint8_t capabilities);

char *DecodePeripheralDeviceType(uint8_t capabilities);

char *DecodePeripheralQualifier(uint8_t capabilities);

char *DecodeSPIClocking(uint8_t capabilities);

#endif //DISCIMAGECHEF_DEVICE_REPORT_INQUIRY_DECODE_H
