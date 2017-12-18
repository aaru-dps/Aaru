//
// Created by claunia on 17/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_MMC_REPORT_H
#define DISCIMAGECHEF_DEVICE_REPORT_MMC_REPORT_H

void MmcReport(int fd, xmlTextWriterPtr xmlWriter, unsigned char *cdromMode);

typedef struct
{
    int           present;
    size_t        len;
    unsigned char *data;
} FeatureDescriptors;

typedef struct
{
    uint32_t           DataLength;
    uint16_t           CurrentProfile;
    FeatureDescriptors Descriptors[65536];
} SeparatedFeatures;
#endif //DISCIMAGECHEF_DEVICE_REPORT_MMC_REPORT_H
