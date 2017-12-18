//
// Created by claunia on 18/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_SSC_REPORT_H
#define DISCIMAGECHEF_DEVICE_REPORT_SSC_REPORT_H
void SscReport(int fd, xmlTextWriterPtr xmlWriter);

#pragma pack(push, 1)
typedef struct
{
    uint8_t primaryCode;
    uint8_t secondaryCode;
    uint8_t dlv : 1;
    uint8_t reserved : 4;
    uint8_t deflt : 1;
    uint8_t dup : 1;
    uint8_t wrtok : 1;
    uint16_t length;
    uint8_t bitsPerMm[3];
    uint16_t mediaWidth;
    uint16_t tracks;
    uint32_t capacity;
    unsigned char organization[8];
    unsigned char densityName[8];
    unsigned char description[20];
} DensityDescriptor;
#pragma pack(pop)

#pragma pack(push, 1)
typedef struct
{
    uint8_t mediumType;
    uint8_t reserved;
    uint16_t length;
    uint8_t codes_len;
    uint8_t codes[9];
    uint16_t mediaWidth;
    uint16_t mediumLength;
    uint16_t reserved2;
    unsigned char organization[8];
    unsigned char densityName[8];
    unsigned char description[20];
} MediumDescriptor;
#pragma pack(pop)

typedef struct
{
    uint16_t count;
    DensityDescriptor *descriptors[1260];
} DensitySupport;

typedef struct
{
    uint16_t count;
    MediumDescriptor *descriptors[1170];
} MediaTypeSupport;
#endif //DISCIMAGECHEF_DEVICE_REPORT_SSC_REPORT_H
