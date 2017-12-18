//
// Created by claunia on 14/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_IDENTIFY_DECODE_H
#define DISCIMAGECHEF_DEVICE_REPORT_IDENTIFY_DECODE_H

char *DecodeGeneralConfiguration(uint16_t configuration);

char *DecodeTransferMode(uint16_t transferMode);

char *DecodeCapabilities(uint16_t capabilities);

char *DecodeCapabilities2(uint16_t capabilities);

char *DecodeCapabilities3(uint8_t capabilities);

char *DecodeCommandSet(uint16_t commandset);

char *DecodeCommandSet2(uint16_t commandset);

char *DecodeCommandSet3(uint16_t commandset);

char *DecodeCommandSet4(uint16_t commandset);

char *DecodeCommandSet5(uint16_t commandset);

char *DecodeDataSetMgmt(uint16_t datasetmgmt);

char *DecodeDeviceFormFactor(uint16_t formfactor);

char *DecodeSATAFeatures(uint16_t features);

char *DecodeMajorVersion(uint16_t capabilities);

char *DecodeSATACapabilities(uint16_t capabilities);

char *DecodeSATACapabilities2(uint16_t transport);

char *DecodeSCTCommandTransport(uint16_t transport);

char *DecodeSecurityStatus(uint16_t status);

char *DecodeSpecificConfiguration(uint16_t configuration);

char *DecodeTrustedComputing(uint16_t trutedcomputing);

#endif //DISCIMAGECHEF_DEVICE_REPORT_IDENTIFY_DECODE_H
