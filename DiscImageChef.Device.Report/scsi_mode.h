//
// Created by claunia on 16/12/17.
//

#ifndef DISCIMAGECHEF_DEVICE_REPORT_SCSI_MODE_H
#define DISCIMAGECHEF_DEVICE_REPORT_SCSI_MODE_H

typedef struct
{
    uint8_t  Density;
    uint64_t Blocks;
    uint32_t BlockLength;
} BlockDescriptor;

typedef struct
{
    uint8_t         MediumType;
    int             WriteProtected;
    BlockDescriptor BlockDescriptors[4096];
    int             descriptorsLength;
    uint8_t         Speed;
    uint8_t         BufferedMode;
    int             EBC;
    int             DPOFUA;
    int             decoded;
} ModeHeader;

typedef struct
{
    ModeHeader    Header;
    unsigned char *Pages[256][256];
    size_t        pageSizes[256][256];
    int           decoded;
} DecodedMode;

ModeHeader *DecodeModeHeader6(unsigned char *modeResponse, uint8_t deviceType);

ModeHeader *DecodeModeHeader10(unsigned char *modeResponse, uint8_t deviceType);

DecodedMode *DecodeMode6(unsigned char *modeResponse, uint8_t deviceType);

DecodedMode *DecodeMode10(unsigned char *modeResponse, uint8_t deviceType);

#endif //DISCIMAGECHEF_DEVICE_REPORT_SCSI_MODE_H
